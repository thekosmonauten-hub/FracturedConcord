using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simple card display for carousel
/// Shows card information in the embossing UI
/// </summary>
public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Serializable]
    private class EmbossingSlotReference
    {
        public Transform container;
        public Image emptySlotImage;
        public Image filledSlotImage;
        public Image iconImage;
    }

    [Header("References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image categoryIcon;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardDescriptionText;
    [SerializeField] private TextMeshProUGUI additionalEffectText;
    [SerializeField] private Transform embossingSlotContainer; // Container for embossing slot indicators
    [SerializeField] private TextMeshProUGUI cardLevelText; // Optional: Shows "Lv. X" on card
    [SerializeField] private UnityEngine.UI.Slider cardXPSlider; // Optional: Shows XP progress to next level
    [SerializeField] private RectTransform visualRoot;
    [Header("Embossing Slots (Optional Overrides)")]
    [SerializeField] private EmbossingSlotReference[] embossingSlotReferences = new EmbossingSlotReference[5];
    
    [Header("Visual Settings")]
    [SerializeField] private Sprite defaultCardSprite;
    [SerializeField] private CardVisualAssets visualAssets;
    
    private Card displayedCard;
    private readonly Dictionary<Image, Sprite> embossingSlotDefaultSprites = new Dictionary<Image, Sprite>();
    private readonly Dictionary<Image, Color> embossingSlotDefaultColors = new Dictionary<Image, Color>();
    private readonly Dictionary<Transform, Image> embossingIconImageCache = new Dictionary<Transform, Image>();
    private static readonly string[] EmbossingIconChildNames = { "EmbossingIcon", "Icon" };

    void Awake()
    {
        // Auto-find components if not assigned
        if (cardImage == null)
        {
            Transform imageChild = transform.Find("CardImage");
            if (imageChild != null)
                cardImage = imageChild.GetComponent<Image>();
        }
        
        if (categoryIcon == null)
        {
            Transform iconChild = transform.Find("CategoryIcon");
            if (iconChild != null)
                categoryIcon = iconChild.GetComponent<Image>();
        }
        
        if (cardNameText == null)
        {
            Transform nameChild = transform.Find("CardName");
            if (nameChild != null)
                cardNameText = nameChild.GetComponent<TextMeshProUGUI>();
        }
        
        if (cardDescriptionText == null)
        {
            Transform descChild = transform.Find("Description");
            if (descChild != null)
                cardDescriptionText = descChild.GetComponent<TextMeshProUGUI>();
        }
        
        if (additionalEffectText == null)
        {
            Transform effectChild = transform.Find("AdditionalEffectText");
            if (effectChild != null)
                additionalEffectText = effectChild.GetComponent<TextMeshProUGUI>();
        }
        
        if (cardLevelText == null)
        {
            // Try to find CardLevel text (in CardLevelContainer)
            Transform containerChild = transform.Find("CardLevelContainer");
            if (containerChild != null)
            {
                // Look for Text (TMP) child
                Transform levelChild = containerChild.Find("Text (TMP)");
                if (levelChild != null)
                    cardLevelText = levelChild.GetComponent<TextMeshProUGUI>();
            }
        }
        
        if (cardXPSlider == null)
        {
            // Try to find Slider (in separate CardXpSlider container)
            Transform sliderContainer = transform.Find("CardXpSlider");
            if (sliderContainer != null)
            {
                // Look for Slider component on container or child
                UnityEngine.UI.Slider slider = sliderContainer.GetComponent<UnityEngine.UI.Slider>();
                if (slider != null)
                {
                    cardXPSlider = slider;
                }
                else
                {
                    // Try to find Slider child
                    Transform sliderChild = sliderContainer.Find("Slider");
                    if (sliderChild != null)
                        cardXPSlider = sliderChild.GetComponent<UnityEngine.UI.Slider>();
                }
            }
        }
        
        if (embossingSlotContainer == null)
        {
            // Try to find in VisualRoot first (for CardPrefab_combat)
            Transform visualRoot = transform.Find("VisualRoot ");
            if (visualRoot != null)
            {
                Transform container = visualRoot.Find("EmbossingSlots");
                if (container != null)
                    embossingSlotContainer = container;
            }
            
            // If not found, try direct child (for CardPrefab)
            if (embossingSlotContainer == null)
            {
                Transform container = transform.Find("EmbossingSlots");
                if (container != null)
                    embossingSlotContainer = container;
            }
        }
        
        if (visualRoot == null)
        {
            Transform visualRootCandidate = transform.Find("VisualRoot");
            if (visualRootCandidate == null)
            {
                visualRootCandidate = transform.Find("VisualRoot ");
            }
            if (visualRootCandidate != null)
                visualRoot = visualRootCandidate as RectTransform;
        }
        
        // Load visual assets if not assigned
        if (visualAssets == null)
        {
            visualAssets = Resources.Load<CardVisualAssets>("CardVisualAssets");
        }
    }
    
    /// <summary>
    /// Set the card to display
    /// </summary>
    public void SetCard(Card card)
    {
        displayedCard = card;
        
        if (card == null)
        {
            Debug.LogWarning("[CardDisplay] Cannot display null card");
            return;
        }
        
        // Set card name
        if (cardNameText != null)
        {
            cardNameText.text = card.cardName;
        }
        
        // Set card level (optional, only if levelText exists)
        if (cardLevelText != null)
        {
            if (card.cardLevel > 1)
            {
                cardLevelText.text = $"Lv. {card.cardLevel}";
                cardLevelText.enabled = true;
            }
            else
            {
                cardLevelText.enabled = false; // Hide for level 1 cards
            }
        }
        
        // Set card XP progress slider (optional)
        if (cardXPSlider != null)
        {
            if (card.cardLevel >= 20)
            {
                // Max level - hide slider or show as full
                cardXPSlider.value = 1.0f;
                cardXPSlider.gameObject.SetActive(false); // Hide at max level
            }
            else
            {
                cardXPSlider.gameObject.SetActive(true);
                
                // Calculate progress to next level
                int required = card.GetRequiredExperienceForNextLevel();
                float progress = required > 0 ? (float)card.cardExperience / required : 0f;
                cardXPSlider.value = Mathf.Clamp01(progress);
            }
        }
        
        Character activeCharacter = ResolveActiveCharacter();

        // Set card description
        if (cardDescriptionText != null)
        {
            string resolvedDescription = ResolveDescription(card, activeCharacter);
#if UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(resolvedDescription) && resolvedDescription.Contains("{"))
            {
                Debug.LogWarning($"[CardDisplay] Unresolved placeholders in description for '{card.cardName}': {resolvedDescription}");
            }
#endif
            cardDescriptionText.text = string.IsNullOrWhiteSpace(resolvedDescription)
                ? "No description available"
                : resolvedDescription;
        }
        
        // Set card image/sprite
        if (cardImage != null)
        {
            if (card.cardArt != null)
            {
                cardImage.sprite = card.cardArt;
                cardImage.enabled = true;
            }
            else if (defaultCardSprite != null)
            {
                cardImage.sprite = defaultCardSprite;
                cardImage.enabled = true;
            }
            else
            {
                cardImage.enabled = false;
            }
        }
        
        // Set category icon based on card type
        if (categoryIcon != null && visualAssets != null)
        {
            Sprite icon = GetCategoryIconSprite(card.cardType);
            if (icon != null)
            {
                categoryIcon.sprite = icon;
                categoryIcon.enabled = true;
            }
            else
            {
                categoryIcon.enabled = false;
            }
        }
        
        // Set additional effect text (Combo, Dual Wield, etc.)
        UpdateAdditionalEffectSection(card, activeCharacter);

        // Display embossing slots (if card has them)
        DisplayEmbossingSlots(card);
    }
    
    /// <summary>
    /// Get the category icon sprite for a card type.
    /// </summary>
    private Sprite GetCategoryIconSprite(CardType cardType)
    {
        if (visualAssets == null) return null;
        
        switch (cardType)
        {
            case CardType.Attack:
                return visualAssets.attackIcon;
            case CardType.Guard:
                return visualAssets.guardIcon;
            case CardType.Skill:
                return visualAssets.skillIcon;
            case CardType.Power:
                return visualAssets.powerIcon;
            default:
                return null;
        }
    }
    
    private Character ResolveActiveCharacter()
    {
        var manager = CharacterManager.Instance;
        if (manager == null)
            return null;

        if (!manager.HasCharacter())
        {
            manager.EnsureCharacterLoadedFromPrefs();
        }

        return manager.GetCurrentCharacter();
    }

    private string ResolveDescription(Card card, Character character)
    {
        if (card == null)
            return string.Empty;

        if (character != null)
        {
            try
            {
                return card.GetDynamicDescription(character);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CardDisplay] Error resolving dynamic description for card '{card.cardName}': {ex.Message}");
            }

            if (card.sourceCardData != null)
            {
                try
                {
                    return card.sourceCardData.GetDynamicDescription(character);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[CardDisplay] Error resolving source description for card '{card.cardName}': {ex.Message}");
                }
            }
        }

        return SanitizePlaceholderText(card.description, card);
    }

    private void UpdateAdditionalEffectSection(Card card, Character character)
    {
        if (additionalEffectText == null)
            return;

        string comboText = ResolveComboDescription(card, character);
        string supplementalText = BuildSupplementalEffectText(card);

        string finalText;
        if (!string.IsNullOrWhiteSpace(comboText) && !string.IsNullOrWhiteSpace(supplementalText))
        {
            finalText = comboText + "\n" + supplementalText;
        }
        else if (!string.IsNullOrWhiteSpace(comboText))
        {
            finalText = comboText;
        }
        else
        {
            finalText = supplementalText;
        }

#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(comboText) && comboText.Contains("{"))
        {
            Debug.LogWarning($"[CardDisplay] Unresolved placeholders in combo text for '{card.cardName}': {comboText}");
        }
#endif

        bool hasText = !string.IsNullOrWhiteSpace(finalText);
        additionalEffectText.text = hasText ? finalText : string.Empty;
        additionalEffectText.enabled = hasText;
        if (additionalEffectText.gameObject.activeSelf != hasText)
        {
            additionalEffectText.gameObject.SetActive(hasText);
        }
    }

    private string ResolveComboDescription(Card card, Character character)
    {
        if (card == null)
            return string.Empty;

        string combo = string.Empty;
        if (card.sourceCardData != null && character != null)
        {
            try
            {
                combo = card.sourceCardData.GetDynamicComboDescription(character);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CardDisplay] Error resolving dynamic combo description for card '{card.cardName}': {ex.Message}");
            }
        }

        if (string.IsNullOrWhiteSpace(combo))
        {
            if (card.sourceCardData != null)
            {
                combo = card.sourceCardData.comboDescription;
            }
            else
            {
                combo = card.comboDescription;
            }
        }

        if (string.IsNullOrWhiteSpace(combo))
            return string.Empty;

        return character != null ? combo : SanitizePlaceholderText(combo, card);
    }

    private string BuildSupplementalEffectText(Card card)
    {
        List<string> effects = new List<string>();

        if (card.tags != null && card.tags.Contains("Dual"))
        {
            effects.Add("Dual Wield: Deal damage with both weapons");
        }

        if (card.tags != null && card.tags.Contains("Discard"))
        {
            effects.Add("Discard: Special effect when discarded");
        }

        if (card.isAoE)
        {
            effects.Add($"Hits {card.aoeTargets} enemies");
        }

        return effects.Count > 0 ? string.Join("\n", effects) : string.Empty;
    }

    private string SanitizePlaceholderText(string template, Card card)
    {
        if (string.IsNullOrWhiteSpace(template))
            return string.Empty;

        string result = template;

        if (card != null)
        {
            result = result.Replace("{damage}", card.baseDamage.ToString("F0"));
            result = result.Replace("{baseDamage}", card.baseDamage.ToString("F0"));
            result = result.Replace("{guard}", card.baseGuard.ToString("F0"));
            result = result.Replace("{baseGuard}", card.baseGuard.ToString("F0"));
            result = result.Replace("{manaCost}", card.manaCost.ToString());
            result = result.Replace("{cost}", card.manaCost.ToString());
            result = result.Replace("{aoeTargets}", card.aoeTargets.ToString());
        }

        result = Regex.Replace(result, "\\{[^}]*\\}", string.Empty);
        return result.Replace("  ", " ").Trim();
    }
    
    /// <summary>
    /// Display embossing slots on the card.
    /// Works with manually created SlotXContainer structure in the prefab.
    /// 
    /// Structure:
    /// EmbossingSlots (parent)
    ///   → Slot1Container (active when card.embossingSlots >= 1)
    ///       → Slot1Embossing (empty slot visual)
    ///       → Slot1Filled (filled slot visual, disabled by default)
    ///   → Slot2Container (active when card.embossingSlots >= 2)
    ///       → Slot2Embossing
    ///       → Slot2Filled
    ///   ... (up to Slot5Container)
    /// </summary>
    void DisplayEmbossingSlots(Card card)
    {
        if (embossingSlotContainer == null && (embossingSlotReferences == null || embossingSlotReferences.Length == 0))
            return;

        int slotCount = card != null ? card.embossingSlots : 0;
        IList<EmbossingInstance> embossings = card?.appliedEmbossings;

        Debug.Log($"[CardDisplay] Refreshing embossing slots for {card?.cardName ?? "Unknown"} - slots:{slotCount}, embossings:{embossings?.Count ?? 0}");

        for (int slotIndex = 0; slotIndex < 5; slotIndex++)
        {
            ResolveEmbossingSlot(slotIndex, out Transform slotContainer, out GameObject emptySlotGO, out Transform filledIndicator, out Image filledImage, out Image iconImage);

            if (slotContainer == null)
            {
                continue;
            }

            bool shouldBeActive = slotIndex < slotCount;
            if (slotContainer.gameObject.activeSelf != shouldBeActive)
            {
                slotContainer.gameObject.SetActive(shouldBeActive);
            }

            CacheDefaultEmbossingVisual(filledImage);
            if (iconImage != null && iconImage != filledImage)
            {
                CacheDefaultEmbossingVisual(iconImage);
            }

            EmbossingInstance slotInstance = embossings?.FirstOrDefault(e => e != null && e.slotIndex == slotIndex);
            if (slotInstance == null && embossings != null && slotIndex < embossings.Count)
            {
                slotInstance = embossings[slotIndex];
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

    private EmbossingSlotReference GetEmbossingSlotReference(int index)
    {
        if (index < 0)
        {
            return null;
        }

        if (embossingSlotReferences == null || embossingSlotReferences.Length < Math.Max(5, index + 1))
        {
            int newLength = Math.Max(5, index + 1);
            var newArray = new EmbossingSlotReference[newLength];
            if (embossingSlotReferences != null)
            {
                Array.Copy(embossingSlotReferences, newArray, Math.Min(embossingSlotReferences.Length, newLength));
            }
            embossingSlotReferences = newArray;
        }

        if (embossingSlotReferences[index] == null)
        {
            embossingSlotReferences[index] = new EmbossingSlotReference();
        }

        return embossingSlotReferences[index];
    }

    private void CacheDefaultEmbossingVisual(Image image)
    {
        if (image == null)
            return;

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
            return null;

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
            return;

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
            return;

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
                Debug.LogWarning($"[CardDisplay] Embossing '{instance.embossingId}' not found in database/resources.");
            }

            if (effect != null && effect.embossingIcon == null)
            {
                Debug.LogWarning($"[CardDisplay] Embossing '{instance.embossingId}' has no icon assigned.");
            }
        }

        bool hasIcon = sprite != null;

        if (backgroundImage != null)
        {
            if (hasIcon)
            {
                Debug.Log($"[CardDisplay] Applying embossing '{instance.embossingId}' to slot {instance.slotIndex + 1}. Icon: {sprite?.name ?? "NULL"}");
                backgroundImage.sprite = sprite;
                backgroundImage.preserveAspect = true;
                backgroundImage.color = Color.white;
            }
            else
            {
                if (embossingSlotDefaultSprites.TryGetValue(backgroundImage, out Sprite defaultSprite))
                {
                    backgroundImage.sprite = defaultSprite;
                }
                backgroundImage.preserveAspect = false;
                backgroundImage.color = color;
            }
        }

        if (iconImage != null && iconImage != backgroundImage)
        {
            if (hasIcon)
            {
                iconImage.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"[CardDisplay] Embossing '{instance.embossingId}' resolved without an icon. Slot {instance.slotIndex + 1} will use default visuals.");
                RestoreEmbossingImage(iconImage);
                iconImage.gameObject.SetActive(false);
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
    
    /// <summary>
    /// Get the displayed card
    /// </summary>
    public Card GetCard()
    {
        return displayedCard;
    }
    
    public void ConfigureEmbossingSlot(int index, Transform container, Image emptySlotImage, Image filledSlotImage, Image iconImage = null)
    {
        if (index < 0)
            return;

        EmbossingSlotReference reference = GetEmbossingSlotReference(index);
        reference.container = container;
        reference.emptySlotImage = emptySlotImage;
        reference.filledSlotImage = filledSlotImage;
        reference.iconImage = iconImage;
    }
    
    public void ApplyScale(float scale)
    {
        if (visualRoot != null)
        {
            visualRoot.localScale = Vector3.one * scale;
        }
        else
        {
            transform.localScale = Vector3.one * scale;
        }
    }
    
    #region Editor Helpers
#if UNITY_EDITOR
    /// <summary>
    /// Auto-assigns component references (call from Inspector context menu).
    /// </summary>
    [ContextMenu("Auto-Assign References")]
    private void AutoAssignReferences()
    {
        if (cardImage == null)
        {
            Transform imageChild = transform.Find("CardImage");
            if (imageChild != null)
            {
                cardImage = imageChild.GetComponent<Image>();
                Debug.Log("[CardDisplay] Auto-assigned cardImage");
            }
        }
        
        if (categoryIcon == null)
        {
            Transform iconChild = transform.Find("CategoryIcon");
            if (iconChild != null)
            {
                categoryIcon = iconChild.GetComponent<Image>();
                Debug.Log("[CardDisplay] Auto-assigned categoryIcon");
            }
        }
        
        if (cardNameText == null)
        {
            Transform nameChild = transform.Find("CardName");
            if (nameChild != null)
            {
                cardNameText = nameChild.GetComponent<TextMeshProUGUI>();
                Debug.Log("[CardDisplay] Auto-assigned cardNameText");
            }
        }
        
        if (cardDescriptionText == null)
        {
            Transform descChild = transform.Find("Description");
            if (descChild != null)
            {
                cardDescriptionText = descChild.GetComponent<TextMeshProUGUI>();
                Debug.Log("[CardDisplay] Auto-assigned cardDescriptionText");
            }
        }
        
        if (additionalEffectText == null)
        {
            Transform effectChild = transform.Find("AdditionalEffectText");
            if (effectChild != null)
            {
                additionalEffectText = effectChild.GetComponent<TextMeshProUGUI>();
                Debug.Log("[CardDisplay] Auto-assigned additionalEffectText");
            }
        }
        
        if (cardLevelText == null)
        {
            // Try to find CardLevel text (could be in CardLevelContainer)
            Transform levelChild = transform.Find("CardLevel");
            if (levelChild == null)
            {
                Transform containerChild = transform.Find("CardLevelContainer");
                if (containerChild != null)
                    levelChild = containerChild.Find("Text (TMP)");
            }
            
            if (levelChild != null)
            {
                cardLevelText = levelChild.GetComponent<TextMeshProUGUI>();
                Debug.Log("[CardDisplay] Auto-assigned cardLevelText");
            }
        }
        
        if (cardXPSlider == null)
        {
            // Try to find Slider (in separate CardXpSlider container)
            Transform sliderContainer = transform.Find("CardXpSlider");
            if (sliderContainer != null)
            {
                // Look for Slider component on container itself or child
                UnityEngine.UI.Slider slider = sliderContainer.GetComponent<UnityEngine.UI.Slider>();
                if (slider != null)
                {
                    cardXPSlider = slider;
                    Debug.Log("[CardDisplay] Auto-assigned cardXPSlider (on container)");
                }
                else
                {
                    // Try to find Slider child
                    Transform sliderChild = sliderContainer.Find("Slider");
                    if (sliderChild != null)
                    {
                        cardXPSlider = sliderChild.GetComponent<UnityEngine.UI.Slider>();
                        Debug.Log("[CardDisplay] Auto-assigned cardXPSlider (child)");
                    }
                }
            }
        }
        
        if (embossingSlotContainer == null)
        {
            // Try to find in VisualRoot first (for CardPrefab_combat)
            Transform visualRoot = transform.Find("VisualRoot ");
            if (visualRoot != null)
            {
                Transform container = visualRoot.Find("EmbossingSlots");
                if (container != null)
                {
                    embossingSlotContainer = container;
                    Debug.Log("[CardDisplay] Auto-assigned embossingSlotContainer from VisualRoot");
                }
            }
            
            // If not found, try direct child (for CardPrefab)
            if (embossingSlotContainer == null)
            {
                Transform container = transform.Find("EmbossingSlots");
                if (container != null)
                {
                    embossingSlotContainer = container;
                    Debug.Log("[CardDisplay] Auto-assigned embossingSlotContainer");
                }
            }
        }
        
        if (visualRoot == null)
        {
            Transform visualRootCandidate = transform.Find("VisualRoot");
            if (visualRootCandidate == null)
            {
                visualRootCandidate = transform.Find("VisualRoot ");
            }
            if (visualRootCandidate != null)
                visualRoot = visualRootCandidate as RectTransform;
        }
        
        if (visualAssets == null)
        {
            visualAssets = Resources.Load<CardVisualAssets>("CardVisualAssets");
            if (visualAssets != null)
            {
                Debug.Log("[CardDisplay] Loaded CardVisualAssets from Resources");
            }
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
    #endregion
    
    #region Tooltip Handling
    
    /// <summary>
    /// Handle pointer enter to show card tooltip
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (displayedCard == null || ItemTooltipManager.Instance == null)
            return;
        
        Character character = ResolveActiveCharacter();
        if (character != null)
        {
            ItemTooltipManager.Instance.ShowCardTooltipForPointer(displayedCard, character, eventData);
        }
    }
    
    /// <summary>
    /// Handle pointer exit to hide card tooltip
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.HideTooltip();
        }
    }
    
    #endregion
}


