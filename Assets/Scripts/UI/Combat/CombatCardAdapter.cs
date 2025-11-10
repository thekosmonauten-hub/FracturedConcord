using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Adapter component for using DeckBuilderCardUI prefabs in combat.
/// Converts Card/CardData to the format DeckBuilderCardUI expects.
/// Add this component to your CardPrefab alongside DeckBuilderCardUI.
/// </summary>
public class CombatCardAdapter : MonoBehaviour
{
    [Serializable]
    private class EmbossingSlotReference
    {
        public Transform container;
        public Image emptySlotImage;
        public Image filledSlotImage;
        public Image iconImage;
    }

    private DeckBuilderCardUI deckBuilderCard;
    private Card currentCard;
    private Character ownerCharacter;
    
    [Header("Category Icon Mapping")]
    [SerializeField] private Image categoryIcon; // optional, auto-find if not assigned
    [SerializeField] private CardVisualAssets visualAssets; // assign in prefab or via Resources
    [Header("Embossing Slots (Optional Overrides)")]
    [SerializeField] private EmbossingSlotReference[] embossingSlotReferences = new EmbossingSlotReference[5];
    
    private Transform embossingSlotContainer;
    private readonly Dictionary<Image, Sprite> fallbackEmbossingDefaults = new Dictionary<Image, Sprite>();
    private readonly Dictionary<Image, Color> fallbackEmbossingDefaultColors = new Dictionary<Image, Color>();
    private readonly Dictionary<Transform, Image> fallbackEmbossingIconCache = new Dictionary<Transform, Image>();
    private static readonly string[] EmbossingIconChildNames = { "EmbossingIcon", "Icon" };
    
    public CardVisualAssets VisualAssets => visualAssets;
    
    private void Awake()
    {
        deckBuilderCard = GetComponent<DeckBuilderCardUI>();
        if (categoryIcon == null)
        {
            var t = transform.Find("CategoryIcon");
            if (t != null) categoryIcon = t.GetComponent<Image>();
        }
        if (visualAssets == null)
        {
            // Optional: try to load from Resources
            visualAssets = Resources.Load<CardVisualAssets>("CardVisualAssets");
        }

        if (embossingSlotContainer == null)
        {
            embossingSlotContainer = transform.Find("EmbossingSlots");
        }
    }
    
    /// <summary>
    /// Set card from Card class (full featured)
    /// </summary>
    public void SetCard(Card card, Character character)
    {
        currentCard = card;
        ownerCharacter = character;
        
        // Convert Card to CardData format that DeckBuilderCardUI expects
        CardData cardData = ConvertToCardData(card, character);
        
        // Initialize the DeckBuilderCardUI (pass null for deckBuilder since we're in combat)
        if (deckBuilderCard != null)
        {
            deckBuilderCard.Initialize(cardData, null);
        }
        else
        {
            // Fallback: Update directly
            UpdateCardVisualsDirectly(card, character);
        }
        
        // Map Additional Effect (Combo Description) onto prefab if present
        UpdateAdditionalEffectText(card?.comboDescription);
        
        // Update category icon
        UpdateCategoryIcon(cardData);

        ApplyEmbossingVisuals(card);
    }
    
    /// <summary>
    /// Set card from CardData ScriptableObject
    /// </summary>
    public void SetCardData(CardData cardData)
    {
        if (deckBuilderCard != null)
        {
            deckBuilderCard.Initialize(cardData, null);
        }
        
        // If this is an extended card, set additional effect text
        if (cardData is CardDataExtended ext)
        {
            // Try dynamic combo description with current character if available
            var ch = CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter() ? CharacterManager.Instance.GetCurrentCharacter() : null;
            string dyn = ext.GetDynamicComboDescription(ch);
            string comboText = string.IsNullOrEmpty(dyn) ? ext.comboDescription : dyn;
            
            Debug.Log($"[CombatCardAdapter] Card '{cardData.cardName}' combo description: '{comboText}' (dynamic: '{dyn}', static: '{ext.comboDescription}')");
            UpdateAdditionalEffectText(comboText);
        }
        else
        {
            Debug.Log($"[CombatCardAdapter] Card '{cardData.cardName}' is not CardDataExtended, clearing additional effect text");
            UpdateAdditionalEffectText("");
        }
        
        // Update category icon
        UpdateCategoryIcon(cardData);

        ApplyEmbossingVisuals(null);
    }
    
    /// <summary>
    /// Set card from CardDataExtended (PREFERRED METHOD - no conversion!)
    /// </summary>
    public void SetCardDataExtended(CardDataExtended cardData, Character character)
    {
        Debug.Log($"<color=lime>[CardDataExtended] CombatCardAdapter: Setting card {cardData.cardName}</color>");
        Debug.Log($"<color=lime>[CardDataExtended]   - Card Image: {(cardData.cardImage != null ? "✅ PRESENT" : "❌ NULL")}</color>");
        Debug.Log($"<color=lime>[CardDataExtended]   - Character: {(character != null ? character.characterName : "NULL")}</color>");
        
        if (deckBuilderCard != null)
        {
            // Initialize DeckBuilderCardUI with CardDataExtended AND character!
            // This enables dynamic descriptions with calculated values
            deckBuilderCard.Initialize(cardData, null, character);
            
            Debug.Log($"<color=lime>[CardDataExtended] ✓ Card initialized with dynamic descriptions!</color>");
        }
        else
        {
            Debug.LogError($"[CardDataExtended] DeckBuilderCardUI component not found!");
        }
        
        // Map Additional Effect (Combo Description)
        string dynCombo = cardData.GetDynamicComboDescription(character);
        UpdateAdditionalEffectText(string.IsNullOrEmpty(dynCombo) ? cardData.comboDescription : dynCombo);
        
        // Update category icon
        UpdateCategoryIcon(cardData);

        ApplyEmbossingVisuals(null);
    }
    
    /// <summary>
    /// Convert Card class to CardData for display
    /// </summary>
    private CardData ConvertToCardData(Card card, Character character)
    {
        CardData cardData = ScriptableObject.CreateInstance<CardData>();
        
        // Basic info
        cardData.cardName = card.cardName;
        cardData.cardType = card.cardType.ToString();
        cardData.playCost = card.manaCost;
        
        // Description (use dynamic if character available)
        if (character != null)
        {
            Debug.Log($"<color=cyan>CombatCardAdapter: Calling GetDynamicDescription for {card.cardName}</color>");
            cardData.description = card.GetDynamicDescription(character);
        }
        else
        {
            Debug.Log($"<color=orange>CombatCardAdapter: No character, using static description for {card.cardName}</color>");
            cardData.description = card.description;
        }
        
        // Convert enums
        cardData.rarity = CardRarity.Common; // Default, could map from card tags
        cardData.element = MapDamageTypeToElement(card.primaryDamageType);
        cardData.category = MapCardTypeToCategory(card.cardType);
        
        // Visual Assets - CRITICAL: Copy card artwork sprite!
        cardData.cardImage = card.cardArt;
        Debug.Log($"<color=cyan>[CardArt] CombatCardAdapter: Converted {card.cardName} - cardImage: {(cardData.cardImage != null ? "✅ SET" : "❌ NULL")}</color>");
        
        // Values
        cardData.damage = Mathf.RoundToInt(card.baseDamage);
        cardData.block = Mathf.RoundToInt(card.baseGuard);
        
        // Calculate total values if character available
        if (character != null)
        {
            if (card.cardType == CardType.Attack)
            {
                float totalDamage = DamageCalculator.CalculateCardDamage(card, character, character.weapons.meleeWeapon);
                cardData.damage = Mathf.RoundToInt(totalDamage);
            }
            else if (card.cardType == CardType.Guard)
            {
                float totalGuard = DamageCalculator.CalculateGuardAmount(card, character);
                cardData.block = Mathf.RoundToInt(totalGuard);
            }
        }
        
        return cardData;
    }
    
    /// <summary>
    /// Fallback: Update card visuals directly using TMP components
    /// </summary>
    private void UpdateCardVisualsDirectly(Card card, Character character)
    {
        // Find text components
        TextMeshProUGUI nameText = transform.Find("CardName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI costText = transform.Find("CostBubble/CostText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI categoryText = transform.Find("CategoryText")?.GetComponent<TextMeshProUGUI>();
        
        // Find card art image - CRITICAL for displaying artwork!
        Image cardArtImage = transform.Find("CardImage")?.GetComponent<Image>();
        if (cardArtImage == null)
            cardArtImage = transform.Find("Card Image")?.GetComponent<Image>();
        
        // Update texts
        if (nameText != null)
            nameText.text = card.cardName;
        
        if (costText != null)
            costText.text = card.manaCost.ToString();
        
        if (descText != null)
        {
            if (character != null)
            {
                Debug.Log($"<color=cyan>CombatCardAdapter Fallback: Calling GetDynamicDescription for {card.cardName}</color>");
                string desc = card.GetDynamicDescription(character);
#if UNITY_EDITOR
                if (!string.IsNullOrWhiteSpace(desc) && desc.Contains("{"))
                {
                    Debug.LogWarning($"[CombatCardAdapter] Unresolved placeholders for '{card.cardName}': {desc}");
                }
#endif
                descText.text = desc;
            }
            else
            {
                Debug.Log($"<color=orange>CombatCardAdapter Fallback: No character, using static description for {card.cardName}</color>");
                descText.text = card.description;
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>CombatCardAdapter Fallback: descText is null for {card.cardName}</color>");
        }
        
        if (categoryText != null)
            categoryText.text = card.cardType.ToString();
        
        // Update card artwork - CRITICAL!
        if (cardArtImage != null && card.cardArt != null)
        {
            cardArtImage.sprite = card.cardArt;
            cardArtImage.enabled = true;
            Debug.Log($"<color=lime>[CardArt] ✓ Fallback path: Card art displayed for {card.cardName}!</color>");
        }
        else if (cardArtImage != null)
        {
            cardArtImage.enabled = false;
            Debug.LogWarning($"[CardArt] ⚠ Fallback path: No card art for {card.cardName} (cardArt: {(card.cardArt != null ? "exists" : "NULL")})");
        }
        
        // Update background color based on card type
        Image background = transform.Find("CardBackground")?.GetComponent<Image>();
        if (background != null)
        {
            background.color = GetCardTypeColor(card.cardType);
        }

        UpdateEmbossingSlotsFallback(card);
    }
    
    /// <summary>
    /// Map DamageType to CardElement
    /// </summary>
    private CardElement MapDamageTypeToElement(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Fire: return CardElement.Fire;
            case DamageType.Cold: return CardElement.Cold;
            case DamageType.Lightning: return CardElement.Lightning;
            case DamageType.Physical: return CardElement.Physical;
            case DamageType.Chaos: return CardElement.Chaos;
            default: return CardElement.Basic;
        }
    }
    
    /// <summary>
    /// Map CardType to CardCategory
    /// </summary>
    private CardCategory MapCardTypeToCategory(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Attack: return CardCategory.Attack;
            case CardType.Guard: return CardCategory.Guard;
            case CardType.Skill: return CardCategory.Skill;
            case CardType.Power: return CardCategory.Power;
            default: return CardCategory.Attack;
        }
    }
    
    /// <summary>
    /// Get color for card type
    /// </summary>
    private Color GetCardTypeColor(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Attack: return new Color(0.8f, 0.3f, 0.3f); // Red
            case CardType.Guard: return new Color(0.3f, 0.6f, 0.8f); // Blue
            case CardType.Skill: return new Color(0.3f, 0.8f, 0.3f); // Green
            case CardType.Power: return new Color(0.8f, 0.6f, 0.2f); // Gold
            case CardType.Aura: return new Color(0.6f, 0.3f, 0.8f); // Purple
            default: return Color.gray;
        }
    }
    
    /// <summary>
    /// Get the current card data
    /// </summary>
    public Card GetCard()
    {
        return currentCard;
    }
    
    private void ApplyEmbossingVisuals(Card card)
    {
        if (deckBuilderCard != null)
        {
            deckBuilderCard.ApplyEmbossingVisuals(card);
        }
        else
        {
            UpdateEmbossingSlotsFallback(card);
        }
    }

    private void UpdateEmbossingSlotsFallback(Card card)
    {
        if (embossingSlotContainer == null && (embossingSlotReferences == null || embossingSlotReferences.Length == 0))
        {
            embossingSlotContainer = transform.Find("EmbossingSlots");
            if (embossingSlotContainer == null && (embossingSlotReferences == null || embossingSlotReferences.Length == 0))
            {
                return;
            }
        }

        int slotCount = card != null ? card.embossingSlots : 0;
        IList<EmbossingInstance> embossings = card?.appliedEmbossings;

        Debug.Log($"[CombatCardAdapter] Refreshing embossing slots for {card?.cardName ?? "Unknown"} - slots:{slotCount}, embossings:{embossings?.Count ?? 0}");

        for (int slotIndex = 0; slotIndex < 5; slotIndex++)
        {
            ResolveFallbackEmbossingSlot(slotIndex, out Transform slotContainer, out GameObject emptySlotGO, out Transform filledIndicator, out Image filledImage, out Image iconImage);

            if (slotContainer == null)
            {
                continue;
            }

            bool shouldBeActive = slotIndex < slotCount;
            if (slotContainer.gameObject.activeSelf != shouldBeActive)
            {
                slotContainer.gameObject.SetActive(shouldBeActive);
            }

            CacheFallbackEmbossingVisual(filledImage);
            if (iconImage != null && iconImage != filledImage)
            {
                CacheFallbackEmbossingVisual(iconImage);
            }

            EmbossingInstance slotInstance = embossings?.FirstOrDefault(e => e != null && e.slotIndex == slotIndex);
            if (slotInstance == null && embossings != null && slotIndex < embossings.Count)
            {
                slotInstance = embossings[slotIndex];
            }
            bool isFilled = shouldBeActive && slotInstance != null && !string.IsNullOrEmpty(slotInstance.embossingId);

            if (isFilled)
            {
                ApplyFallbackEmbossingVisual(filledIndicator, filledImage, iconImage, slotInstance);
            }
            else
            {
                ResetFallbackEmbossingSlot(filledIndicator, filledImage, iconImage);
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

    private void ResolveFallbackEmbossingSlot(int zeroBasedSlotIndex, out Transform slotContainer, out GameObject emptySlotGO, out Transform filledIndicator, out Image filledImage, out Image iconImage)
    {
        slotContainer = null;
        emptySlotGO = null;
        filledIndicator = null;
        filledImage = null;
        iconImage = null;

        EmbossingSlotReference reference = GetEmbossingSlotReference(zeroBasedSlotIndex);

        if (reference != null)
        {
            if (reference.container != null)
            {
                slotContainer = reference.container;
            }
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

        if (slotContainer == null)
        {
            if (embossingSlotContainer == null)
            {
                embossingSlotContainer = transform.Find("EmbossingSlots");
            }
            if (embossingSlotContainer != null)
            {
                slotContainer = embossingSlotContainer.Find($"Slot{zeroBasedSlotIndex + 1}Container");
            }
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
                iconImage = ResolveFallbackEmbossingIconImage(filledIndicator);
            }
            else
            {
                fallbackEmbossingIconCache[filledIndicator] = iconImage;
            }
        }
    }

    private void CacheFallbackEmbossingVisual(Image image)
    {
        if (image == null)
        {
            return;
        }

        if (!fallbackEmbossingDefaults.ContainsKey(image))
        {
            fallbackEmbossingDefaults[image] = image.sprite;
        }

        if (!fallbackEmbossingDefaultColors.ContainsKey(image))
        {
            fallbackEmbossingDefaultColors[image] = image.color;
        }
    }

    private Image ResolveFallbackEmbossingIconImage(Transform filledIndicator)
    {
        if (filledIndicator == null)
        {
            return null;
        }

        if (fallbackEmbossingIconCache.TryGetValue(filledIndicator, out Image cached))
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

        fallbackEmbossingIconCache[filledIndicator] = icon;
        return icon;
    }

    private void RestoreFallbackEmbossingImage(Image image, bool resetSprite = true, bool resetColor = true, bool resetPreserveAspect = true)
    {
        if (image == null)
        {
            return;
        }

        if (resetSprite && fallbackEmbossingDefaults.TryGetValue(image, out Sprite defaultSprite))
        {
            image.sprite = defaultSprite;
        }

        if (resetColor && fallbackEmbossingDefaultColors.TryGetValue(image, out Color defaultColor))
        {
            image.color = defaultColor;
        }

        if (resetPreserveAspect)
        {
            image.preserveAspect = false;
        }
    }

    private void ApplyFallbackEmbossingVisual(Transform filledIndicator, Image backgroundImage, Image iconImage, EmbossingInstance instance)
    {
        if (filledIndicator == null)
        {
            return;
        }

        Sprite sprite = null;
        Color color = Color.white;

        if (instance != null && !string.IsNullOrEmpty(instance.embossingId))
        {
            EmbossingEffect effect = DeckBuilderCardUI.ResolveEmbossingEffect(instance.embossingId);
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
                Debug.LogWarning($"[CombatCardAdapter] Embossing '{instance.embossingId}' not found in database/resources.");
            }

            if (effect != null && effect.embossingIcon == null)
            {
                Debug.LogWarning($"[CombatCardAdapter] Embossing '{instance.embossingId}' has no icon assigned.");
            }
        }

        bool hasIcon = sprite != null;

        if (backgroundImage != null)
        {
            if (hasIcon)
            {
                Debug.Log($"[CombatCardAdapter] Applying embossing '{instance.embossingId}' to slot {instance.slotIndex + 1}. Icon: {sprite.name}");
                backgroundImage.sprite = sprite;
                backgroundImage.preserveAspect = true;
                backgroundImage.color = Color.white;
            }
            else
            {
                if (fallbackEmbossingDefaults.TryGetValue(backgroundImage, out Sprite defaultSprite))
                {
                    backgroundImage.sprite = defaultSprite;
                }
                backgroundImage.preserveAspect = false;
                backgroundImage.color = color;
            }
        }

        if (iconImage != null && iconImage != backgroundImage)
        {
            if (!hasIcon)
            {
                Debug.LogWarning($"[CombatCardAdapter] Embossing '{instance.embossingId}' resolved without an icon. Slot {instance.slotIndex + 1} will use default visuals.");
                RestoreFallbackEmbossingImage(iconImage);
                iconImage.gameObject.SetActive(false);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        filledIndicator.gameObject.SetActive(hasIcon);
    }

    private void ResetFallbackEmbossingSlot(Transform filledIndicator, Image backgroundImage, Image iconImage)
    {
        RestoreFallbackEmbossingImage(backgroundImage);

        if (iconImage != null && iconImage != backgroundImage)
        {
            RestoreFallbackEmbossingImage(iconImage);
            iconImage.gameObject.SetActive(false);
        }

        if (filledIndicator != null)
        {
            filledIndicator.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Update the AdditionalEffectText child (if present) with provided text
    /// </summary>
    private void UpdateAdditionalEffectText(string text)
    {
        if (deckBuilderCard != null)
        {
            deckBuilderCard.SetAdditionalEffectText(text);
            return;
        }

        // Search recursively through all descendants (AdditionalEffectText is nested under VisualRoot)
        var textObjects = GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        TMPro.TextMeshProUGUI tmp = null;
        Transform t = null;
        
        // Debug: List all TextMeshProUGUI components found
        Debug.Log($"[CombatCardAdapter] Searching for AdditionalEffectText on {gameObject.name}. Found {textObjects.Length} TextMeshProUGUI components:");
        foreach (var textComp in textObjects)
        {
            Debug.Log($"  - GameObject: '{textComp.gameObject.name}' (Active: {textComp.gameObject.activeInHierarchy})");
        }
        
        foreach (var textComp in textObjects)
        {
            if (textComp.gameObject.name == "AdditionalEffectText" || 
                textComp.gameObject.name == "Additional Effects" || 
                textComp.gameObject.name == "AdditionalEffect")
            {
                tmp = textComp;
                t = textComp.transform;
                Debug.Log($"[CombatCardAdapter] Found matching GameObject: '{textComp.gameObject.name}'");
                break;
            }
        }
        
        if (t == null || tmp == null)
        {
            Debug.LogWarning($"[CombatCardAdapter] AdditionalEffectText GameObject not found on {gameObject.name}. Expected names: 'AdditionalEffectText', 'Additional Effects', or 'AdditionalEffect'");
            Debug.LogWarning($"[CombatCardAdapter] Available GameObjects with TextMeshProUGUI:");
            foreach (var textComp in textObjects)
            {
                Debug.LogWarning($"  - '{textComp.gameObject.name}'");
            }
            return;
        }
        
        if (string.IsNullOrWhiteSpace(text))
        {
            tmp.text = "";
            t.gameObject.SetActive(false);
            Debug.Log($"[CombatCardAdapter] {gameObject.name}: AdditionalEffectText hidden (no combo description).");
        }
        else
        {
            tmp.text = text;
            t.gameObject.SetActive(true);
            Debug.Log($"[CombatCardAdapter] {gameObject.name}: AdditionalEffectText set to: \"{text}\"");
        }
    }
    
    /// <summary>
    /// Update the CategoryIcon image from CardVisualAssets based on CardData category
    /// </summary>
    private void UpdateCategoryIcon(CardData cardData)
    {
        if (deckBuilderCard != null)
        {
            deckBuilderCard.SetCategoryIcon(cardData, visualAssets);
        }

        if (categoryIcon == null || visualAssets == null || cardData == null) return;
        Sprite sprite = null;
        switch (cardData.category)
        {
            case CardCategory.Attack: sprite = visualAssets.attackIcon; break;
            case CardCategory.Guard: sprite = visualAssets.guardIcon; break;
            case CardCategory.Skill: sprite = visualAssets.skillIcon; break;
            case CardCategory.Power: sprite = visualAssets.powerIcon; break;
            default: sprite = null; break;
        }
        categoryIcon.sprite = sprite;
        categoryIcon.enabled = sprite != null;
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
}

