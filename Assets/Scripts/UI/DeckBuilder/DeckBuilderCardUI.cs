using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// UI component for individual cards in the collection grid.
/// Displays card information and handles click/hover interactions.
/// </summary>
public class DeckBuilderCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Serializable]
    private class EmbossingSlotReference
    {
        public Transform container;
        public Image emptySlotImage;
        public Image filledSlotImage;
        public Image iconImage;
    }

    [Header("Structure")]
    [SerializeField] private RectTransform visualRoot; // scale this instead of the layout root to avoid layout jitter
    [Header("Card Visual Elements")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image cardImage;
    [SerializeField] private Image rarityFrame;
    [SerializeField] private Image elementFrame;
    [SerializeField] private Image costBubble;
    
    [Header("Card Text Elements")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private TextMeshProUGUI additionalEffectsText;
    [SerializeField] private Image categoryIconImage;
    [SerializeField] private CardVisualAssets cardVisualAssets;
    [SerializeField] private Transform embossingSlotContainer;
    [Header("Embossing Slots (Optional Overrides)")]
    [SerializeField] private EmbossingSlotReference[] embossingSlotReferences = new EmbossingSlotReference[5];
    
    [Header("State Visuals")]
    [SerializeField] private GameObject disabledOverlay;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color disabledColor = Color.gray;
    
    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float animationDuration = 0.2f;
    
    private CardData cardData;
    private DeckBuilderUI deckBuilder;
    private Character ownerCharacter; // For dynamic descriptions in combat
    private int currentQuantity;
    private int maxQuantity;
    private bool isInteractable = true;
    private Vector3 originalScale;
    private RectTransform rectTransform;
    private Button button;
    private Transform scaleTarget;
    private bool referencesResolved = false;
    private readonly Dictionary<Image, Sprite> embossingSlotDefaultSprites = new Dictionary<Image, Sprite>();
    private readonly Dictionary<Image, Color> embossingSlotDefaultColors = new Dictionary<Image, Color>();
    private readonly Dictionary<Transform, Image> embossingIconImageCache = new Dictionary<Transform, Image>();
    private static readonly string[] EmbossingIconChildNames = { "EmbossingIcon", "Icon" };
    private static Dictionary<string, EmbossingEffect> embossingEffectCache;
    private bool isHovering = false;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        EnsureCardReferences();
        // Decide which transform to scale on hover
        scaleTarget = visualRoot != null ? (Transform)visualRoot : transform;
        originalScale = scaleTarget.localScale;
        
        // Disable Button component if it exists - we use IPointerClickHandler instead
        // This prevents the Button from consuming click events
        if (button != null)
        {
            button.transition = Selectable.Transition.None; // Disable visual transitions
            // Don't set interactable to false - that blocks ALL pointer events!
        }

        // Ensure hover visuals do not block raycasts
        if (glowEffect != null)
        {
            var graphics = glowEffect.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].raycastTarget = false;
            }
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to momentum changes to refresh card cost display
        if (StackSystem.Instance != null)
        {
            StackSystem.Instance.OnStacksChanged += OnMomentumChanged;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from momentum changes
        if (StackSystem.Instance != null)
        {
            StackSystem.Instance.OnStacksChanged -= OnMomentumChanged;
        }
    }
    
    private void OnMomentumChanged(StackType stackType, int value)
    {
        // Refresh card display when momentum changes (affects cost display)
        if (stackType == StackType.Momentum && cardData != null)
        {
            UpdateDisplay();
        }
    }
    
    /// <summary>
    /// Initialize the card UI with card data.
    /// </summary>
    public void Initialize(CardData card, DeckBuilderUI deckBuilderUI)
    {
        EnsureCardReferences();
        cardData = card;
        deckBuilder = deckBuilderUI;
        ownerCharacter = null; // No character in deck builder
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// Initialize the card UI with card data and character (for combat with dynamic descriptions).
    /// </summary>
    public void Initialize(CardData card, DeckBuilderUI deckBuilderUI, Character character)
    {
        EnsureCardReferences();
        cardData = card;
        deckBuilder = deckBuilderUI;
        ownerCharacter = character;
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// Update the card's visual display.
    /// </summary>
    private void UpdateDisplay()
    {
        if (cardData == null) return;
        EnsureCardReferences();
        
        // Update card image
        if (cardImage != null && cardData.cardImage != null)
        {
            // Use full-resolution sprite for detailed card display
            cardImage.sprite = cardData.GetCardSprite(CardSpriteContext.Full);
        }

        // Background remains styled by color/frames for collection grid; no sprite assignment here
        // EXCEPTION: Check for Temporal tag and apply special background
        UpdateTemporalCardVisual();
        
        // Update text elements
        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }
        
        // Mana cost will be updated after embossings are set (see UpdateManaCostDisplay)
        UpdateManaCostDisplay(null);
        
        if (descriptionText != null)
        {
            Character descCharacter = ownerCharacter;
            if (descCharacter == null)
            {
                var charManager = CharacterManager.Instance;
                if (charManager != null)
                {
                    if (!charManager.HasCharacter())
                    {
                        charManager.EnsureCharacterLoadedFromPrefs();
                    }
                    descCharacter = charManager.GetCurrentCharacter();
                }
            }

            if (descCharacter != null && cardData is CardDataExtended extendedCard)
            {
                string resolved = extendedCard.GetDynamicDescription(descCharacter);
#if UNITY_EDITOR
                if (!string.IsNullOrWhiteSpace(resolved) && resolved.Contains("{"))
                {
                    Debug.LogWarning($"[DeckBuilderCardUI] Unresolved placeholders for '{cardData.cardName}': {resolved}");
                }
#endif
                descriptionText.text = resolved;
            }
            else
            {
                // Fall back to template if no character is available
                descriptionText.text = cardData.description;
            }
        }
        
        if (categoryText != null)
        {
            categoryText.text = cardData.category.ToString();
        }
        
        SetEmbossingSlots(null);
        IList<EmbossingInstance> storedEmbossings = ResolveStoredEmbossings();
        if (storedEmbossings != null && storedEmbossings.Count > 0)
        {
            Card previewCard = BuildPreviewCardWithEmbossings(storedEmbossings);
            if (previewCard != null)
            {
                SetEmbossingSlots(previewCard);
                // Update mana cost after embossings are set
                UpdateManaCostDisplay(previewCard);
            }
        }
        else
        {
            // No embossings, just update with base cost
            UpdateManaCostDisplay(null);
        }

        UpdateAdditionalEffectsDisplay();
        UpdateCategoryIconDisplay();
        
        // Update visual assets
        if (rarityFrame != null && cardData.rarityFrame != null)
        {
            rarityFrame.sprite = cardData.rarityFrame;
        }
        
        if (elementFrame != null && cardData.elementFrame != null)
        {
            elementFrame.sprite = cardData.elementFrame;
        }
        
        if (costBubble != null && cardData.costBubble != null)
        {
            costBubble.sprite = cardData.costBubble;
        }
        
        // Update rarity-based styling
        UpdateRarityVisuals();
    }
    
    /// <summary>
    /// Update quantity display and interactability.
    /// </summary>
    public void UpdateQuantity(int quantity, int max)
    {
        currentQuantity = quantity;
        maxQuantity = max;
        
        // Update quantity text
        if (quantityText != null)
        {
            if (quantity > 0)
            {
                quantityText.text = $"x{quantity}/{max}";
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }
        
        // Update visual state
        UpdateVisualState();
    }
    
    /// <summary>
    /// Set whether the card is interactable (can be added to deck).
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        UpdateVisualState();
    }
    
    private void UpdateVisualState()
    {
        if (disabledOverlay != null)
        {
            disabledOverlay.SetActive(!isInteractable);
        }
        
        if (cardBackground != null)
        {
            cardBackground.color = isInteractable ? normalColor : disabledColor;
        }
        
        // Note: Don't set button.interactable - it blocks IPointerClickHandler events
        // We handle interactability manually in OnPointerClick/OnPointerEnter
    }
    
    private void UpdateRarityVisuals()
    {
        if (cardData == null) return;
        
        // Update rarity frame background color
        UpdateRarityFrameColor();
    }
    
    /// <summary>
    /// Updates the rarity frame background color based on card rarity.
    /// Maintains semi-transparency while providing distinct colors for each rarity tier.
    /// </summary>
    private void UpdateRarityFrameColor()
    {
        if (rarityFrame == null || cardData == null) return;
        
        rarityFrame.color = RarityColorUtility.GetRarityFrameColor(cardData.rarity);
    }
    
    /// <summary>
    /// Update visual for Temporal cards (Temporal Savant ascendancy).
    /// Applies special background to cards marked with "Temporal" tag.
    /// Uses the CardBackground GameObject to display the TemporalCard.png sprite.
    /// </summary>
    private void UpdateTemporalCardVisual()
    {
        if (cardData == null) return;
        
        // Check if card has Temporal tag
        bool isTemporal = false;
        if (cardData is CardDataExtended extended)
        {
            isTemporal = extended.tags != null && extended.tags.Contains("Temporal");
        }
        
        if (!isTemporal)
        {
            // Reset background if card is not Temporal
            if (cardBackground != null && cardBackground.sprite != null)
            {
                // Only reset if it was a temporal sprite (could check sprite name or store original)
                // For now, we'll let the normal card display handle non-temporal cards
            }
            return;
        }
        
        // Apply Temporal visual - use CardBackground GameObject
        EnsureCardReferences();
        
        if (cardBackground == null)
        {
            Debug.LogWarning("[DeckBuilderCardUI] CardBackground GameObject not found - cannot apply Temporal card visual");
            return;
        }
        
        // Try to load the Temporal card sprite if not already in visual assets
        Sprite temporalSprite = null;
        if (cardVisualAssets != null && cardVisualAssets.temporalCard != null)
        {
            temporalSprite = cardVisualAssets.temporalCard;
        }
        else
        {
            // Fallback: Try to load directly from Resources
            // Check Resources/CardArt/TemporalCard first (current location)
            temporalSprite = Resources.Load<Sprite>("CardArt/TemporalCard");
            if (temporalSprite == null)
            {
                // Try alternative paths
                temporalSprite = Resources.Load<Sprite>("CardParts/TemporalCard");
            }
            if (temporalSprite == null)
            {
                temporalSprite = Resources.Load<Sprite>("Art/CardArt/CardParts/TemporalCard");
            }
        }
        
        if (temporalSprite != null)
        {
            // Apply temporal background to CardBackground
            cardBackground.sprite = temporalSprite;
            cardBackground.color = Color.white; // Use full color, no tint needed
            Debug.Log($"[DeckBuilderCardUI] Applied Temporal card background to '{cardData.cardName}'");
        }
        else
        {
            // Fallback: Apply a color tint to indicate Temporal status
            cardBackground.color = new Color(0.7f, 0.8f, 1f, 0.5f); // Cyan/blue tint for Temporal
            Debug.LogWarning($"[DeckBuilderCardUI] TemporalCard sprite not found - using color tint fallback for '{cardData.cardName}'");
        }
    }

    private void EnsureCardReferences()
    {
        if (referencesResolved)
            return;

        if (additionalEffectsText == null)
        {
            additionalEffectsText = GetComponentsInChildren<TextMeshProUGUI>(true)
                .FirstOrDefault(t =>
                    string.Equals(t.gameObject.name, "AdditionalEffectText", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t.gameObject.name, "AdditionalEffectsText", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t.gameObject.name, "AdditionalEffects", StringComparison.OrdinalIgnoreCase));
        }

        if (categoryIconImage == null)
        {
            categoryIconImage = GetComponentsInChildren<Image>(true)
                .FirstOrDefault(img => string.Equals(img.gameObject.name, "CategoryIcon", StringComparison.OrdinalIgnoreCase));
        }

        if (cardVisualAssets == null)
        {
            var adapter = GetComponent<CombatCardAdapter>();
            if (adapter != null && adapter.VisualAssets != null)
            {
                cardVisualAssets = adapter.VisualAssets;
            }
        }

        if (cardVisualAssets == null)
        {
            cardVisualAssets = Resources.Load<CardVisualAssets>("CardVisualAssets");
        }
        
        // Auto-find CardBackground if not assigned
        if (cardBackground == null)
        {
            cardBackground = GetComponentsInChildren<Image>(true)
                .FirstOrDefault(img => string.Equals(img.gameObject.name, "CardBackground", StringComparison.OrdinalIgnoreCase));
        }

        if (embossingSlotContainer == null)
        {
            Transform searchRoot = visualRoot != null ? (Transform)visualRoot : transform;
            embossingSlotContainer = searchRoot.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => string.Equals(t.name, "EmbossingSlots", StringComparison.OrdinalIgnoreCase));
        }

        referencesResolved = true;
    }

    private void UpdateAdditionalEffectsDisplay()
    {
        string comboText = string.Empty;

        if (cardData is CardDataExtended extended && extended.enableCombo)
        {
            Character referenceCharacter = ResolveReferenceCharacter();
            string dynamicCombo = extended.GetDynamicComboDescription(referenceCharacter);
            comboText = string.IsNullOrWhiteSpace(dynamicCombo) ? extended.comboDescription : dynamicCombo;
        }

        SetAdditionalEffectText(comboText);
    }

    private Character ResolveReferenceCharacter()
    {
        if (ownerCharacter != null)
        {
            return ownerCharacter;
        }

        if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
        {
            return CharacterManager.Instance.currentCharacter;
        }

        return null;
    }

    private void UpdateCategoryIconDisplay()
    {
        if (cardData == null)
        {
            SetCategoryIconSprite(null);
            return;
        }

        Sprite sprite = GetCategoryIconSprite(cardData.category, cardVisualAssets);
        SetCategoryIconSprite(sprite);
    }

    private static Sprite GetCategoryIconSprite(CardCategory category, CardVisualAssets assets)
    {
        if (assets == null) return null;

        switch (category)
        {
            case CardCategory.Attack:
                return assets.attackIcon;
            case CardCategory.Guard:
                return assets.guardIcon;
            case CardCategory.Skill:
                return assets.skillIcon;
            case CardCategory.Power:
                return assets.powerIcon;
            default:
                return null;
        }
    }

    public void SetAdditionalEffectText(string text)
    {
        EnsureCardReferences();

        if (additionalEffectsText == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            additionalEffectsText.text = string.Empty;
            if (additionalEffectsText.gameObject.activeSelf)
            {
                additionalEffectsText.gameObject.SetActive(false);
            }
        }
        else
        {
            additionalEffectsText.text = text;
            if (!additionalEffectsText.gameObject.activeSelf)
            {
                additionalEffectsText.gameObject.SetActive(true);
            }
        }
    }

    public void SetCategoryIcon(CardData card, CardVisualAssets assets = null)
    {
        if (assets != null)
        {
            cardVisualAssets = assets;
        }

        Sprite sprite = card != null ? GetCategoryIconSprite(card.category, cardVisualAssets) : null;
        SetCategoryIconSprite(sprite);
    }

    public void SetCategoryIconSprite(Sprite sprite)
    {
        EnsureCardReferences();

        if (categoryIconImage == null)
        {
            return;
        }

        bool hasSprite = sprite != null;
        categoryIconImage.sprite = sprite;
        categoryIconImage.enabled = hasSprite;
        if (categoryIconImage.gameObject.activeSelf != hasSprite)
        {
            categoryIconImage.gameObject.SetActive(hasSprite);
        }
    }

    public void ApplyEmbossingVisuals(Card card)
    {
        SetEmbossingSlots(card);
    }

    private IList<EmbossingInstance> ResolveStoredEmbossings()
    {
        if (deckBuilder == null || cardData == null)
            return null;

        DeckPreset deck = deckBuilder.CurrentDeck;
        if (deck == null)
            return null;

        string groupKey = ResolveGroupKey(cardData);
        if (string.IsNullOrEmpty(groupKey))
            return null;

        return deck.GetEmbossingsForGroup(groupKey);
    }

    private static string ResolveGroupKey(CardData data)
    {
        if (data is CardDataExtended extended && !string.IsNullOrEmpty(extended.groupKey))
            return extended.groupKey;

        return data != null ? data.cardName : string.Empty;
    }

    private Card BuildPreviewCardWithEmbossings(IList<EmbossingInstance> embossings)
    {
        if (embossings == null || embossings.Count == 0 || cardData == null)
            return null;

        // Calculate base mana cost
        int baseCost = cardData.playCost;
        if (cardData is CardDataExtended extendedCard)
        {
            baseCost = CombatDeckManager.GetDisplayCost(extendedCard, cardData.playCost, ownerCharacter);
        }

        Card previewCard = new Card
        {
            cardName = cardData.cardName,
            embossingSlots = cardData.embossingSlots,
            appliedEmbossings = DeckCardEntry.CopyEmbossings(embossings),
            groupKey = ResolveGroupKey(cardData),
            manaCost = baseCost // Set base cost for embossing calculation
        };

        return previewCard;
    }
    
    /// <summary>
    /// Update the mana cost display text, accounting for embossings
    /// </summary>
    private void UpdateManaCostDisplay(Card cardWithEmbossings)
    {
        if (costText == null || cardData == null)
            return;

        // Calculate base display cost (includes momentum-based cost reductions)
        int displayCost = cardData.playCost;
        CardDataExtended extendedCard = cardData as CardDataExtended;
        if (extendedCard != null)
        {
            // Get the base cost - for Skill cards with percentage, use percentageManaCost; otherwise use playCost
            int baseCostForDisplay = extendedCard.playCost;
            if (extendedCard.percentageManaCost > 0 && string.Equals(extendedCard.cardType, "Skill", System.StringComparison.OrdinalIgnoreCase))
        {
                baseCostForDisplay = extendedCard.percentageManaCost;
            }
            displayCost = CombatDeckManager.GetDisplayCost(extendedCard, baseCostForDisplay, ownerCharacter);
        }

        // Apply embossing cost increases if embossings are present
        if (cardWithEmbossings != null && cardWithEmbossings.appliedEmbossings != null && cardWithEmbossings.appliedEmbossings.Count > 0)
        {
            // Use the card's GetCurrentManaCost method which accounts for embossings and Skill card percentages
            displayCost = cardWithEmbossings.GetCurrentManaCost(ownerCharacter);
        }
        else if (cardWithEmbossings != null && cardWithEmbossings.cardType == CardType.Skill && ownerCharacter != null && extendedCard != null)
        {
            // For Skill cards with percentage cost, calculate percentage-based cost
            if (extendedCard.percentageManaCost > 0)
            {
                float percentageCost = extendedCard.percentageManaCost / 100.0f;
                displayCost = Mathf.RoundToInt(ownerCharacter.maxMana * percentageCost);
            }
            // If percentageManaCost is 0, displayCost is already the flat cost from playCost
        }

        // For Skill cards with percentage cost, show percentage format
        if (cardWithEmbossings != null && cardWithEmbossings.cardType == CardType.Skill && extendedCard != null)
        {
            if (extendedCard.percentageManaCost > 0)
            {
                // Use percentageManaCost
                int percentageValue = extendedCard.percentageManaCost;
                if (ownerCharacter != null)
                {
                    costText.text = $"{percentageValue}% ({displayCost})";
                }
                else
                {
                    costText.text = $"{percentageValue}%";
                }
            }
            else
            {
                // Fallback: use playCost as flat cost
        costText.text = displayCost.ToString();
            }
        }
        else
        {
            costText.text = displayCost.ToString();
        }
    }

    public static EmbossingEffect ResolveEmbossingEffect(string embossingId)
    {
        if (string.IsNullOrEmpty(embossingId))
            return null;

        EmbossingEffect effect = EmbossingDatabase.Instance != null
            ? EmbossingDatabase.Instance.GetEmbossing(embossingId)
            : null;

        if (effect != null)
            return effect;

        if (embossingEffectCache != null && embossingEffectCache.TryGetValue(embossingId, out effect))
            return effect;

        if (embossingEffectCache == null)
            embossingEffectCache = new Dictionary<string, EmbossingEffect>(StringComparer.OrdinalIgnoreCase);

        static EmbossingEffect[] LoadFromPaths()
        {
            string[] candidatePaths =
            {
                "Embossings",
                "Embossing/Effects",
                "Embossing"
            };

            foreach (string path in candidatePaths)
            {
                EmbossingEffect[] results = Resources.LoadAll<EmbossingEffect>(path);
                if (results != null && results.Length > 0)
                {
                    return results;
                }
            }

            return Array.Empty<EmbossingEffect>();
        }

        EmbossingEffect[] allEmbossings = LoadFromPaths();
        foreach (EmbossingEffect embossing in allEmbossings)
        {
            if (embossing == null || string.IsNullOrEmpty(embossing.embossingId))
                continue;

            embossingEffectCache[embossing.embossingId] = embossing;

            if (effect == null && string.Equals(embossing.embossingId, embossingId, StringComparison.OrdinalIgnoreCase))
            {
                effect = embossing;
            }
        }

        return effect;
    }

    private void SetEmbossingSlots(Card card)
    {
        IList<EmbossingInstance> embossings = card?.appliedEmbossings;
        int slotCount = card != null ? card.embossingSlots : (cardData != null ? cardData.embossingSlots : 0);

        EnsureCardReferences();

        if (embossingSlotContainer == null && (embossingSlotReferences == null || embossingSlotReferences.Length == 0))
        {
            return;
        }

        // Embossing slot logging disabled by default (too verbose) - uncomment if needed for debugging
        // Debug.Log($"[DeckBuilderCardUI] Refreshing embossing slots for {cardData?.cardName ?? card?.cardName ?? "Unknown"} - slots:{slotCount}, embossings:{embossings?.Count ?? 0}");

        for (int i = 0; i < 5; i++)
        {
            ResolveEmbossingSlot(i, out Transform slotContainer, out GameObject emptySlotGO, out Transform filledIndicator, out Image filledImage, out Image iconImage);

            if (slotContainer == null)
            {
                continue;
            }

            bool shouldBeActive = i < slotCount;
            if (slotContainer.gameObject.activeSelf != shouldBeActive)
            {
                slotContainer.gameObject.SetActive(shouldBeActive);
            }

            CacheDefaultEmbossingVisual(filledImage);
            if (iconImage != null && iconImage != filledImage)
            {
                CacheDefaultEmbossingVisual(iconImage);
            }

            EmbossingInstance slotInstance = embossings?.FirstOrDefault(e => e != null && e.slotIndex == i);
            if (slotInstance == null && embossings != null && i < embossings.Count)
            {
                slotInstance = embossings[i];
            }
            bool isFilled = shouldBeActive && slotInstance != null && !string.IsNullOrEmpty(slotInstance.embossingId);

            if (isFilled)
            {
                ApplyEmbossingSlotVisual(filledIndicator, filledImage, iconImage, slotInstance);
            }
            else
            {
                ResetEmbossingSlot(filledIndicator, filledImage, iconImage);
            }

            if (emptySlotGO != null)
            {
                bool emptyShouldBeActive = shouldBeActive;
                if (emptySlotGO.activeSelf != emptyShouldBeActive)
                {
                    emptySlotGO.SetActive(emptyShouldBeActive);
                }
            }
        }
    }

    private void ResolveEmbossingSlot(int zeroBasedSlotIndex, out Transform slotContainer, out GameObject emptySlotGO, out Transform filledIndicator, out Image filledImage, out Image iconImage)
    {
        slotContainer = null;
        emptySlotGO = null;
        filledIndicator = null;
        filledImage = null;
        iconImage = null;

        EmbossingSlotReference reference = GetEmbossingSlotReference(zeroBasedSlotIndex);

        if (reference != null)
        {
            slotContainer = reference.container != null ? reference.container : slotContainer;
            if (reference.emptySlotImage != null)
            {
                emptySlotGO = reference.emptySlotImage.gameObject;
            }
            if (reference.filledSlotImage != null)
            {
                filledImage = reference.filledSlotImage;
                filledIndicator = reference.filledSlotImage.transform;
            }
            if (reference.iconImage != null)
            {
                iconImage = reference.iconImage;
            }
        }

        if (slotContainer == null && embossingSlotContainer != null)
        {
            slotContainer = embossingSlotContainer.Find($"Slot{zeroBasedSlotIndex + 1}Container");
        }

        if (slotContainer != null)
        {
            if (emptySlotGO == null)
            {
                Transform emptySlot = slotContainer.Find($"Slot{zeroBasedSlotIndex + 1}Embossing");
                if (emptySlot != null)
                {
                    emptySlotGO = emptySlot.gameObject;
                    if (reference != null && reference.emptySlotImage == null)
                    {
                        reference.emptySlotImage = emptySlot.GetComponent<Image>();
                    }
                }
            }

            if (filledIndicator == null)
            {
                filledIndicator = slotContainer.Find($"Slot{zeroBasedSlotIndex + 1}Filled");
                if (filledIndicator != null && filledImage == null)
                {
                    filledImage = filledIndicator.GetComponent<Image>();
                    if (reference != null && reference.filledSlotImage == null)
                    {
                        reference.filledSlotImage = filledImage;
                    }
                }
            }
        }

        if (filledIndicator != null)
        {
            if (iconImage == null)
            {
                iconImage = ResolveEmbossingIconImage(filledIndicator);
            }
            else
            {
                embossingIconImageCache[filledIndicator] = iconImage;
            }
        }
    }

    private void CacheDefaultEmbossingVisual(Image image)
    {
        if (image == null)
        {
            return;
        }

        if (!embossingSlotDefaultSprites.ContainsKey(image))
        {
            embossingSlotDefaultSprites[image] = image.sprite;
        }

        if (!embossingSlotDefaultColors.ContainsKey(image))
        {
            embossingSlotDefaultColors[image] = image.color;
        }
    }

    private Image ResolveEmbossingIconImage(Transform filledIndicator)
    {
        if (filledIndicator == null)
        {
            return null;
        }

        if (embossingIconImageCache.TryGetValue(filledIndicator, out Image cached))
        {
            return cached;
        }

        Image icon = null;
        foreach (string childName in EmbossingIconChildNames)
        {
            Transform child = filledIndicator.Find(childName);
            if (child != null)
            {
                icon = child.GetComponent<Image>();
                if (icon != null)
                {
                    break;
                }
            }
        }

        embossingIconImageCache[filledIndicator] = icon;
        return icon;
    }

    private void RestoreEmbossingImage(Image image, bool resetSprite = true, bool resetColor = true, bool resetPreserveAspect = true)
    {
        if (image == null)
        {
            return;
        }

        if (resetSprite && embossingSlotDefaultSprites.TryGetValue(image, out Sprite defaultSprite))
        {
            image.sprite = defaultSprite;
        }

        if (resetColor && embossingSlotDefaultColors.TryGetValue(image, out Color defaultColor))
        {
            image.color = defaultColor;
        }

        if (resetPreserveAspect)
        {
            image.preserveAspect = false;
        }
    }

    private void ApplyEmbossingSlotVisual(Transform filledIndicator, Image backgroundImage, Image iconImage, EmbossingInstance instance)
    {
        if (filledIndicator == null)
        {
            return;
        }

        Sprite sprite = null;
        Color color = Color.white;

        if (instance != null && !string.IsNullOrEmpty(instance.embossingId))
        {
            EmbossingEffect effect = ResolveEmbossingEffect(instance.embossingId);
            if (effect != null)
            {
                if (effect.embossingIcon != null)
                {
                    sprite = effect.embossingIcon;
                }
                color = effect.embossingColor;
            }
            else
            {
                Debug.LogWarning($"[DeckBuilderCardUI] Embossing '{instance.embossingId}' not found in database/resources.");
            }

            if (effect != null && effect.embossingIcon == null)
            {
                Debug.LogWarning($"[DeckBuilderCardUI] Embossing '{instance.embossingId}' has no icon assigned.");
            }
        }

        bool hasIcon = sprite != null;

        if (backgroundImage != null)
        {
            if (hasIcon)
            {
                backgroundImage.sprite = sprite;
                backgroundImage.preserveAspect = true;
                backgroundImage.color = Color.white;
                Debug.Log($"[DeckBuilderCardUI] Applying embossing '{instance.embossingId}' to slot {instance.slotIndex + 1}. Icon: {sprite.name}");
            }
            else if (embossingSlotDefaultSprites.TryGetValue(backgroundImage, out Sprite defaultSprite))
            {
                backgroundImage.sprite = defaultSprite;
                backgroundImage.preserveAspect = false;
                backgroundImage.color = color;
            }
        }

        if (iconImage != null && iconImage != backgroundImage)
        {
            Image targetIconImage = iconImage;
            if (!hasIcon)
            {
                Debug.LogWarning($"[DeckBuilderCardUI] Embossing '{instance.embossingId}' resolved without an icon. Slot {instance.slotIndex + 1} will use default visuals.");
                RestoreEmbossingImage(targetIconImage);
                if (targetIconImage != backgroundImage)
                {
                    targetIconImage.gameObject.SetActive(false);
                }
            }
            else
            {
                targetIconImage.gameObject.SetActive(false);
            }
        }

        filledIndicator.gameObject.SetActive(hasIcon);
    }

    private void ResetEmbossingSlot(Transform filledIndicator, Image backgroundImage, Image iconImage)
    {
        RestoreEmbossingImage(backgroundImage);

        if (iconImage != null && iconImage != backgroundImage)
        {
            RestoreEmbossingImage(iconImage);
            iconImage.gameObject.SetActive(false);
        }

        if (filledIndicator != null)
        {
            filledIndicator.gameObject.SetActive(false);
        }
    }
    
    public void ConfigureEmbossingSlot(int index, Transform container, Image emptySlotImage, Image filledSlotImage, Image iconImage = null)
    {
        EmbossingSlotReference reference = GetEmbossingSlotReference(index);
        if (reference == null)
        {
            return;
        }

        reference.container = container;
        reference.emptySlotImage = emptySlotImage;
        reference.filledSlotImage = filledSlotImage;
        reference.iconImage = iconImage;
    }

    private EmbossingSlotReference GetEmbossingSlotReference(int index)
    {
        if (index < 0)
        {
            return null;
        }

        int requiredLength = Math.Max(5, index + 1);
        if (embossingSlotReferences == null || embossingSlotReferences.Length < requiredLength)
        {
            var newArray = new EmbossingSlotReference[requiredLength];
            if (embossingSlotReferences != null)
            {
                Array.Copy(embossingSlotReferences, newArray, Math.Min(embossingSlotReferences.Length, requiredLength));
            }

            embossingSlotReferences = newArray;
        }

        if (embossingSlotReferences[index] == null)
        {
            embossingSlotReferences[index] = new EmbossingSlotReference();
        }

        return embossingSlotReferences[index];
    }
    
    #region Interaction Handlers
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        isHovering = true;
        
        // Scale up animation on visual root only (keeps layout root stable)
        LeanTween.cancel(scaleTarget.gameObject);
        LeanTween.scale(scaleTarget.gameObject, originalScale * hoverScale, animationDuration)
            .setEase(LeanTweenType.easeOutBack);
        
        // Highlight effect
        if (cardBackground != null)
        {
            cardBackground.color = hoverColor;
        }
        
        // Enhance rarity frame color on hover (make it more opaque)
        if (rarityFrame != null && cardData != null)
        {
            rarityFrame.color = RarityColorUtility.GetRarityFrameHoverColor(cardData.rarity);
        }
        
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
        }
        
        // Check for ALT key to show tooltip
        CheckAndShowTooltip(eventData);
        
        // Note: SetAsLastSibling() removed because it causes grid recalculation issues
        // Cards will scale up on hover but won't overlap other cards
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        isHovering = false;
        
        // Scale down animation on visual root only
        LeanTween.cancel(scaleTarget.gameObject);
        LeanTween.scale(scaleTarget.gameObject, originalScale, animationDuration)
            .setEase(LeanTweenType.easeInBack);
        
        // Remove highlight
        if (cardBackground != null)
        {
            cardBackground.color = normalColor;
        }
        
        // Restore normal rarity frame color
        if (rarityFrame != null && cardData != null)
        {
            rarityFrame.color = RarityColorUtility.GetRarityFrameColor(cardData.rarity);
        }
        
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
        
        // Hide tooltip when leaving card
        HideTooltip();
    }
    
    private void Update()
    {
        // If hovering, check ALT key state and show/hide tooltip accordingly
        if (isHovering && ItemTooltipManager.Instance != null && cardData != null)
        {
            bool altPressed = Keyboard.current != null && 
                             (Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed);
            
            if (altPressed)
            {
                // Show tooltip if ALT is held while hovering
                // Use current mouse position for tooltip (using new Input System)
                Vector2 mousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
                PointerEventData fakeEventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                {
                    position = mousePosition
                };
                CheckAndShowTooltip(fakeEventData);
            }
            else
            {
                // Hide tooltip if ALT is not pressed
                HideTooltip();
            }
        }
    }
    
    private void CheckAndShowTooltip(PointerEventData eventData)
    {
        // Only show tooltip when ALT is held
        bool altPressed = Keyboard.current != null && 
                         (Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed);
        
        if (!altPressed || cardData == null || ItemTooltipManager.Instance == null)
        {
            return;
        }
        
        // Convert CardData to Card for tooltip
        Card card = ConvertCardDataToCard(cardData);
        if (card == null)
        {
            return;
        }
        
        // Get character for tooltip calculations
        Character character = ResolveReferenceCharacter();
        
        // Show tooltip
        ItemTooltipManager.Instance.ShowCardTooltipForPointer(card, character, eventData);
    }
    
    private void HideTooltip()
    {
        if (ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.HideTooltip();
        }
    }
    
    /// <summary>
    /// Convert CardData to Card object for tooltip display
    /// </summary>
    private Card ConvertCardDataToCard(CardData cardData)
    {
        if (cardData == null) return null;
        
        // If it's a CardDataExtended, use its ToCard method, but override embossings with deck embossings if available
        if (cardData is CardDataExtended extendedCardData)
        {
            Card tooltipCard = extendedCardData.ToCard();
            
            // Override embossings with deck embossings if available (deck embossings take priority)
            IList<EmbossingInstance> deckEmbossings = ResolveStoredEmbossings();
            if (deckEmbossings != null && deckEmbossings.Count > 0)
            {
                tooltipCard.appliedEmbossings = DeckCardEntry.CopyEmbossings(deckEmbossings);
                Debug.Log($"[DeckBuilderCardUI] Tooltip: Found {deckEmbossings.Count} embossings from deck for '{cardData.cardName}'");
            }
            else if (extendedCardData.appliedEmbossings != null && extendedCardData.appliedEmbossings.Count > 0)
            {
                // If no deck embossings, use embossings from CardDataExtended asset
                tooltipCard.appliedEmbossings = DeckCardEntry.CopyEmbossings(extendedCardData.appliedEmbossings);
                Debug.Log($"[DeckBuilderCardUI] Tooltip: Found {extendedCardData.appliedEmbossings.Count} embossings from asset for '{cardData.cardName}'");
            }
            else
            {
                Debug.Log($"[DeckBuilderCardUI] Tooltip: No embossings found for '{cardData.cardName}'");
            }
            
            return tooltipCard;
        }
        
        // Otherwise, create a basic Card from CardData
        Character character = ResolveReferenceCharacter();
        Card basicCard = new Card
        {
            cardName = cardData.cardName,
            description = cardData.description,
            manaCost = cardData.playCost,
            baseDamage = cardData.damage,
            baseGuard = cardData.block,
            cardArt = cardData.cardImage,
            cardArtName = cardData.cardImage != null ? cardData.cardImage.name : "",
            cardType = cardData.category == CardCategory.Attack ? CardType.Attack :
                       cardData.category == CardCategory.Guard ? CardType.Guard :
                       cardData.category == CardCategory.Skill ? CardType.Skill :
                       cardData.category == CardCategory.Power ? CardType.Power : CardType.Attack,
            primaryDamageType = cardData is CardDataExtended extendedData ? extendedData.primaryDamageType : DamageType.Physical,
            embossingSlots = cardData.embossingSlots,
            appliedEmbossings = new List<EmbossingInstance>(),
            sourceCardData = cardData as CardDataExtended
        };
        
        // Copy tags if available
        if (cardData is CardDataExtended extendedForTags && extendedForTags.tags != null)
        {
            basicCard.tags = new List<string>(extendedForTags.tags);
        }
        
        // Get embossings from deck if available
        IList<EmbossingInstance> deckEmbossingsForBasic = ResolveStoredEmbossings();
        if (deckEmbossingsForBasic != null && deckEmbossingsForBasic.Count > 0)
        {
            basicCard.appliedEmbossings = DeckCardEntry.CopyEmbossings(deckEmbossingsForBasic);
            Debug.Log($"[DeckBuilderCardUI] Tooltip: Found {deckEmbossingsForBasic.Count} embossings from deck for '{cardData.cardName}'");
        }
        else
        {
            // If no embossings from deck, check if CardDataExtended has embossings
            if (cardData is CardDataExtended extendedForEmbossings && extendedForEmbossings.appliedEmbossings != null && extendedForEmbossings.appliedEmbossings.Count > 0)
            {
                basicCard.appliedEmbossings = DeckCardEntry.CopyEmbossings(extendedForEmbossings.appliedEmbossings);
                Debug.Log($"[DeckBuilderCardUI] Tooltip: Found {extendedForEmbossings.appliedEmbossings.Count} embossings from asset for '{cardData.cardName}'");
            }
            else
            {
                Debug.Log($"[DeckBuilderCardUI] Tooltip: No embossings found for '{cardData.cardName}'");
            }
        }
        
        return basicCard;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"OnPointerClick received! Button: {eventData.button}, Card: {cardData?.cardName}");
        
        // Only respond to left-click to add cards
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Check for Shift key using new Input System
            bool shiftPressed = Keyboard.current != null && 
                               (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
            
            // Shift + Left-Click: Add 2 cards
            if (shiftPressed)
            {
                Debug.Log("Shift+Click detected - adding 2 cards");
                OnCardClicked(); // Add first card
                OnCardClicked(); // Add second card
            }
            else
            {
                Debug.Log("Regular click - adding 1 card");
                OnCardClicked(); // Add 1 card
            }
        }
        
        // Right-click could be used for other actions (preview, info, etc.)
        // Currently disabled - only left-click adds cards
    }
    
    private void OnCardClicked()
    {
        if (!isInteractable || cardData == null || deckBuilder == null) return;
        
        // Play click animation
        PlayClickAnimation();
        
        // Notify deck builder
        deckBuilder.OnCardAdded(cardData);
    }
    
    private void PlayClickAnimation()
    {
        // Quick punch animation on visual root only; ensure it doesn't block clicks
        LeanTween.cancel(scaleTarget.gameObject);
        LeanTween.scale(scaleTarget.gameObject, originalScale * 1.1f, 0.1f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(scaleTarget.gameObject, originalScale, 0.1f)
                    .setEase(LeanTweenType.easeInQuad);
            });
    }
    #endregion
    
    private void OnDestroy()
    {
        // Clean up tweens
        LeanTween.cancel(gameObject);
    }

	/// <summary>
	/// Expose the current CardData for consumers (e.g., combat adapters/tooltips).
	/// </summary>
	public CardData GetCardData()
	{
		return cardData;
	}
}
