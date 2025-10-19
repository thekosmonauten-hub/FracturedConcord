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
    private DeckBuilderCardUI deckBuilderCard;
    private Card currentCard;
    private Character ownerCharacter;
    
    [Header("Category Icon Mapping")]
    [SerializeField] private Image categoryIcon; // optional, auto-find if not assigned
    [SerializeField] private CardVisualAssets visualAssets; // assign in prefab or via Resources
    
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
            UpdateAdditionalEffectText(string.IsNullOrEmpty(dyn) ? ext.comboDescription : dyn);
        }
        else
        {
            UpdateAdditionalEffectText("");
        }
        
        // Update category icon
        UpdateCategoryIcon(cardData);
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
                descText.text = card.GetDynamicDescription(character);
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
    
    /// <summary>
    /// Update the AdditionalEffectText child (if present) with provided text
    /// </summary>
    private void UpdateAdditionalEffectText(string text)
    {
        // Search recursively through all descendants (AdditionalEffectText is nested under VisualRoot)
        var textObjects = GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        TMPro.TextMeshProUGUI tmp = null;
        Transform t = null;
        
        foreach (var textComp in textObjects)
        {
            if (textComp.gameObject.name == "AdditionalEffectText" || 
                textComp.gameObject.name == "Additional Effects" || 
                textComp.gameObject.name == "AdditionalEffect")
            {
                tmp = textComp;
                t = textComp.transform;
                break;
            }
        }
        
        if (t == null || tmp == null)
        {
            Debug.LogWarning($"[CombatCardAdapter] AdditionalEffectText GameObject not found on {gameObject.name}. Check prefab hierarchy.");
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
}

