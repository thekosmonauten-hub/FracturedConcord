using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardVisualManager : MonoBehaviour
{
    [Header("Card Components")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardTypeText;
    [SerializeField] private TextMeshProUGUI cardDescriptionText;
    [SerializeField] private TextMeshProUGUI playCostText;
    
    [Header("Visual Elements")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image elementFrame;
    [SerializeField] private Image costBubble;
    [SerializeField] private Image rarityBackground;
    
    [Header("Asset References")]
    [SerializeField] private CardVisualAssets visualAssets;
    
    private CardData currentCardData;
    
    private void Awake()
    {
        // Auto-find components if not assigned
        if (cardNameText == null) cardNameText = FindTextComponent("CardName", "Name", "Title", "Card Name");
        if (cardTypeText == null) cardTypeText = FindTextComponent("CardType", "Type", "Category", "Card Type");
        if (cardDescriptionText == null) cardDescriptionText = FindTextComponent("CardDescription", "Description", "Desc", "Text", "Card Description");
        if (playCostText == null) playCostText = FindTextComponent("PlayCost", "Cost", "Energy", "Play Cost");
        
        if (cardImage == null) cardImage = FindImageComponent("CardImage", "Image", "Art", "Card Image");
        if (elementFrame == null) elementFrame = FindImageComponent("ElementFrame", "Frame", "Border", "Element Frame");
        if (costBubble == null) costBubble = FindImageComponent("CostBubble", "Bubble", "CostIcon", "Cost Bubble");
        if (rarityBackground == null) rarityBackground = FindImageComponent("RarityBackground", "Background", "RarityFrame", "Rarity Background");
        
        // Load visual assets if not assigned
        if (visualAssets == null)
        {
            visualAssets = Resources.Load<CardVisualAssets>("CardVisualAssets");
            if (visualAssets == null)
            {
                Debug.LogError("Failed to load CardVisualAssets from Resources! Make sure CardVisualAssets.asset is in the Resources folder.");
            }
            else
            {
                Debug.Log("CardVisualAssets loaded successfully from Resources.");
            }
        }
        
        // Log what we found
        LogComponentStatus();
    }
    
    private TextMeshProUGUI FindTextComponent(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            TextMeshProUGUI component = transform.Find(name)?.GetComponent<TextMeshProUGUI>();
            if (component != null)
            {
                Debug.Log($"Found text component: {name}");
                return component;
            }
        }
        
        // Try to find any TextMeshProUGUI component if we couldn't find by name
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>();
        if (allTexts.Length > 0)
        {
            Debug.Log($"Found {allTexts.Length} TextMeshProUGUI components, using first one");
            return allTexts[0];
        }
        
        Debug.LogWarning($"Could not find any TextMeshProUGUI component with names: {string.Join(", ", possibleNames)}");
        return null;
    }
    
    private Image FindImageComponent(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            Image component = transform.Find(name)?.GetComponent<Image>();
            if (component != null)
            {
                Debug.Log($"Found image component: {name}");
                return component;
            }
        }
        
        // Try to find any Image component if we couldn't find by name
        Image[] allImages = GetComponentsInChildren<Image>();
        if (allImages.Length > 0)
        {
            Debug.Log($"Found {allImages.Length} Image components, using first one");
            return allImages[0];
        }
        
        Debug.LogWarning($"Could not find any Image component with names: {string.Join(", ", possibleNames)}");
        return null;
    }
    
    private void LogComponentStatus()
    {
        Debug.Log("=== CardVisualManager Component Status ===");
        Debug.Log($"Card Name Text: {(cardNameText != null ? "Found" : "Missing")}");
        Debug.Log($"Card Type Text: {(cardTypeText != null ? "Found" : "Missing")}");
        Debug.Log($"Card Description Text: {(cardDescriptionText != null ? "Found" : "Missing")}");
        Debug.Log($"Play Cost Text: {(playCostText != null ? "Found" : "Missing")}");
        Debug.Log($"Card Image: {(cardImage != null ? "Found" : "Missing")}");
        Debug.Log($"Element Frame: {(elementFrame != null ? "Found" : "Missing")}");
        Debug.Log($"Cost Bubble: {(costBubble != null ? "Found" : "Missing")}");
        Debug.Log($"Rarity Background: {(rarityBackground != null ? "Found" : "Missing")}");
        Debug.Log($"Visual Assets: {(visualAssets != null ? "Found" : "Missing")}");
        Debug.Log("==========================================");
        
        // Log all available components for debugging
        LogAllAvailableComponents();
    }
    
    private void LogAllAvailableComponents()
    {
        Debug.Log("=== All Available Components in Card Prefab ===");
        
        // Log all TextMeshProUGUI components
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>();
        Debug.Log($"Found {allTexts.Length} TextMeshProUGUI components:");
        foreach (var text in allTexts)
        {
            Debug.Log($"  - {text.name} (GameObject: {text.gameObject.name})");
        }
        
        // Log all Image components
        Image[] allImages = GetComponentsInChildren<Image>();
        Debug.Log($"Found {allImages.Length} Image components:");
        foreach (var image in allImages)
        {
            Debug.Log($"  - {image.name} (GameObject: {image.gameObject.name})");
        }
        
        Debug.Log("===============================================");
    }
    
    public void UpdateCardVisuals(CardData cardData)
    {
        if (cardData == null) return;
        
        currentCardData = cardData;
        
        // Update text elements
        UpdateTextElements();
        
        // Update visual elements
        UpdateVisualElements();
        
        // Update colors based on rarity
        UpdateRarityColors();
    }
    
    /// <summary>
    /// Update card visuals from Card object (for JSON-loaded cards)
    /// </summary>
    public void UpdateCardVisuals(Card card)
    {
        if (card == null) return;
        
        // Convert Card to CardData format for display
        // Create a temporary CardData to reuse existing rendering logic
        CardData tempCardData = ScriptableObject.CreateInstance<CardData>();
        tempCardData.cardName = card.cardName;
        tempCardData.description = card.description;
        tempCardData.cardType = card.cardType.ToString();
        tempCardData.playCost = card.manaCost;
        tempCardData.damage = (int)card.baseDamage;
        tempCardData.block = (int)card.baseGuard;
        
        // Parse rarity and element from Card
        tempCardData.rarity = ParseCardRarity(card);
        tempCardData.element = ParseCardElement(card);
        tempCardData.category = ParseCardCategory(card.cardType);
        
        // Most importantly: Set the card art from the Card object
        tempCardData.cardImage = card.cardArt;
        
        // Now use the existing UpdateCardVisuals method
        UpdateCardVisuals(tempCardData);
        
        Debug.Log($"<color=cyan>[CardArt] Updated card visuals from Card object: {card.cardName} (Art: {(card.cardArt != null ? "Loaded" : "Missing")})</color>");
    }
    
    private CardRarity ParseCardRarity(Card card)
    {
        // Try to parse from tags or use default
        if (card.tags != null && card.tags.Contains("Rare"))
            return CardRarity.Rare;
        if (card.tags != null && card.tags.Contains("Magic"))
            return CardRarity.Magic;
        if (card.tags != null && card.tags.Contains("Unique"))
            return CardRarity.Unique;
        
        return CardRarity.Common;
    }
    
    private CardElement ParseCardElement(Card card)
    {
        // Parse based on damage type or tags
        if (card.primaryDamageType == DamageType.Fire)
            return CardElement.Fire;
        if (card.primaryDamageType == DamageType.Cold)
            return CardElement.Cold;
        if (card.primaryDamageType == DamageType.Lightning)
            return CardElement.Lightning;
        if (card.primaryDamageType == DamageType.Physical)
            return CardElement.Physical;
        if (card.primaryDamageType == DamageType.Chaos)
            return CardElement.Chaos;
        
        return CardElement.Basic;
    }
    
    private CardCategory ParseCardCategory(CardType cardType)
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
    
    private void UpdateTextElements()
    {
        // Update card name
        if (cardNameText != null)
        {
            cardNameText.text = currentCardData.cardName;
            cardNameText.color = GetRarityColor(currentCardData.rarity);
        }
        
        // Update card type
        if (cardTypeText != null)
        {
            cardTypeText.text = currentCardData.cardType;
            cardTypeText.color = GetRarityColor(currentCardData.rarity);
        }
        
        // Update card description with full effects
        if (cardDescriptionText != null)
        {
            cardDescriptionText.text = currentCardData.GetFullDescription();
            cardDescriptionText.color = Color.white; // Keep description readable
        }
        
        // Update play cost
        if (playCostText != null)
        {
            playCostText.text = currentCardData.playCost.ToString();
            playCostText.color = Color.white; // Keep cost readable
        }
        
        // Log the update for debugging
        Debug.Log($"Updated card: {currentCardData.cardName} - {currentCardData.cardType} ({currentCardData.element}, {currentCardData.rarity})");
    }
    
    private void UpdateVisualElements()
    {
        if (visualAssets == null)
        {
            Debug.LogError("VisualAssets is null! Cannot update visual elements.");
            return;
        }
        
        Debug.Log($"Updating visual elements for {currentCardData.cardName} (Element: {currentCardData.element}, Rarity: {currentCardData.rarity})");
        
        // Update card image
        if (cardImage != null && currentCardData.cardImage != null)
        {
            cardImage.sprite = currentCardData.cardImage;
            Debug.Log($"Updated card image for {currentCardData.cardName}");
        }
        
        // Update element frame
        if (elementFrame != null)
        {
            Sprite elementSprite = GetElementFrame(currentCardData.element);
            if (elementSprite != null)
            {
                elementFrame.sprite = elementSprite;
                Debug.Log($"Updated element frame for {currentCardData.cardName} to {currentCardData.element}");
            }
            else
            {
                Debug.LogWarning($"No element sprite found for {currentCardData.element} on card {currentCardData.cardName}");
            }
        }
        else
        {
            Debug.LogWarning($"ElementFrame component is null for card {currentCardData.cardName}");
        }
        
        // Update cost bubble
        if (costBubble != null)
        {
            Sprite bubbleSprite = GetCostBubble(currentCardData.element);
            if (bubbleSprite != null)
            {
                costBubble.sprite = bubbleSprite;
                Debug.Log($"Updated cost bubble for {currentCardData.cardName} to {currentCardData.element}");
            }
            else
            {
                Debug.LogWarning($"No cost bubble sprite found for {currentCardData.element} on card {currentCardData.cardName}");
            }
        }
        else
        {
            Debug.LogWarning($"CostBubble component is null for card {currentCardData.cardName}");
        }
        
        // Update rarity background
        if (rarityBackground != null)
        {
            Sprite raritySprite = GetRarityFrame(currentCardData.rarity);
            if (raritySprite != null)
            {
                rarityBackground.sprite = raritySprite;
                Debug.Log($"Updated rarity background for {currentCardData.cardName} to {currentCardData.rarity}");
            }
            else
            {
                Debug.LogWarning($"No rarity sprite found for {currentCardData.rarity} on card {currentCardData.cardName}");
            }
        }
        else
        {
            Debug.LogWarning($"RarityBackground component is null for card {currentCardData.cardName}");
        }
    }
    
    private void UpdateRarityColors()
    {
        // Rarity colors are now handled in UpdateTextElements for better organization
        // This method can be used for additional rarity-based visual effects if needed
        Color rarityColor = GetRarityColor(currentCardData.rarity);
        
        // You could add additional rarity-based effects here, such as:
        // - Glow effects
        // - Particle systems
        // - Border animations
        // - Background tinting
    }
    
    private Sprite GetElementFrame(CardElement element)
    {
        if (visualAssets == null) return null;
        
        switch (element)
        {
            case CardElement.Basic: return visualAssets.basicCard;
            case CardElement.Fire: return visualAssets.fireCard;
            case CardElement.Cold: return visualAssets.coldCard;
            case CardElement.Lightning: return visualAssets.lightningCard;
            case CardElement.Physical: return visualAssets.physCard;
            case CardElement.Chaos: return visualAssets.chaosCard;
            default: return visualAssets.basicCard;
        }
    }
    
    private Sprite GetCostBubble(CardElement element)
    {
        if (visualAssets == null) return null;
        
        switch (element)
        {
            case CardElement.Basic: return visualAssets.basicBubble;
            case CardElement.Fire: return visualAssets.fireBubble;
            case CardElement.Cold: return visualAssets.coldBubble;
            case CardElement.Lightning: return visualAssets.lightningBubble;
            case CardElement.Physical: return visualAssets.physBubble;
            case CardElement.Chaos: return visualAssets.chaosBubble;
            default: return visualAssets.basicBubble;
        }
    }
    
    private Sprite GetRarityFrame(CardRarity rarity)
    {
        if (visualAssets == null) return null;
        
        switch (rarity)
        {
            case CardRarity.Common: return visualAssets.commonFrame;
            case CardRarity.Magic: return visualAssets.magicFrame;
            case CardRarity.Rare: return visualAssets.rareFrame;
            case CardRarity.Unique: return visualAssets.uniqueFrame;
            default: return visualAssets.commonFrame;
        }
    }
    
    private Color GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common: return Color.white;
            case CardRarity.Magic: return new Color(0.4f, 0.6f, 1f); // Blue
            case CardRarity.Rare: return new Color(1f, 0.8f, 0.4f); // Gold
            case CardRarity.Unique: return new Color(1f, 0.6f, 0.2f); // Orange
            default: return Color.white;
        }
    }
    
    // Public method to get current card data
    public CardData GetCurrentCardData()
    {
        return currentCardData;
    }
}
