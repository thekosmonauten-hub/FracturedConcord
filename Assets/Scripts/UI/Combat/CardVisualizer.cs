using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles visual display of card data on a card prefab.
/// Updates text, images, and colors based on card properties.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardVisualizer : MonoBehaviour
{
    [Header("Card Elements")]
    [SerializeField] private Text cardNameText;
    [SerializeField] private Text cardCostText;
    [SerializeField] private Text cardDescriptionText;
    [SerializeField] private Text cardDamageText;
    [SerializeField] private Text cardTypeText;
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image cardBorder;
    [SerializeField] private Image elementIcon;
    [SerializeField] private Image cardArtImage; // ⭐ NEW: Card artwork display
    
    private Card currentCard;
    private Character ownerCharacter;
    
    /// <summary>
    /// Set card data and update visuals
    /// </summary>
    public void SetCard(Card card, Character character)
    {
        currentCard = card;
        ownerCharacter = character;
        
        // Debug logging to trace card art
        Debug.Log($"<color=cyan>[CardArt] CardVisualizer.SetCard() called for: {card.cardName}</color>");
        Debug.Log($"<color=cyan>[CardArt]   - Card Art Sprite: {(card.cardArt != null ? "✅ LOADED" : "❌ NULL")}</color>");
        Debug.Log($"<color=cyan>[CardArt]   - Card Art Name: '{card.cardArtName}'</color>");
        Debug.Log($"<color=cyan>[CardArt]   - cardArtImage component: {(cardArtImage != null ? "✅ Found" : "❌ NULL")}</color>");
        
        UpdateVisuals();
    }
    
    /// <summary>
    /// Update all visual elements based on current card data
    /// </summary>
    public void UpdateVisuals()
    {
        if (currentCard == null) return;
        
        // Update texts
        if (cardNameText != null)
            cardNameText.text = currentCard.cardName;
        
        if (cardCostText != null)
            cardCostText.text = currentCard.manaCost.ToString();
        
        if (cardTypeText != null)
            cardTypeText.text = currentCard.cardType.ToString();
        
        if (cardDescriptionText != null)
        {
            if (ownerCharacter != null)
            {
                Debug.Log($"<color=lime>CardVisualizer: Calling GetDynamicDescription for {currentCard.cardName}</color>");
                cardDescriptionText.text = currentCard.GetDynamicDescription(ownerCharacter);
            }
            else
            {
                Debug.Log($"<color=orange>CardVisualizer: No character, using static description for {currentCard.cardName}</color>");
                cardDescriptionText.text = currentCard.description;
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>CardVisualizer: cardDescriptionText is null for {currentCard.cardName}</color>");
        }
        
        // Update damage/guard value
        if (cardDamageText != null)
        {
            if (currentCard.cardType == CardType.Attack && currentCard.baseDamage > 0)
            {
                float totalDamage = currentCard.baseDamage;
                if (ownerCharacter != null)
                {
                    totalDamage = DamageCalculator.CalculateCardDamage(
                        currentCard, 
                        ownerCharacter, 
                        ownerCharacter.weapons.meleeWeapon
                    );
                }
                cardDamageText.text = Mathf.RoundToInt(totalDamage).ToString();
            }
            else if (currentCard.cardType == CardType.Guard && currentCard.baseGuard > 0)
            {
                float totalGuard = currentCard.baseGuard;
                if (ownerCharacter != null)
                {
                    totalGuard = DamageCalculator.CalculateGuardAmount(currentCard, ownerCharacter);
                }
                cardDamageText.text = Mathf.RoundToInt(totalGuard).ToString();
            }
            else
            {
                cardDamageText.text = "";
            }
        }
        
        // ⭐ NEW: Update card artwork
        if (cardArtImage != null)
        {
            if (currentCard.cardArt != null)
            {
                cardArtImage.sprite = currentCard.cardArt;
                cardArtImage.enabled = true;
                Debug.Log($"<color=lime>[CardArt] ✓ Card art displayed for {currentCard.cardName}!</color>");
            }
            else
            {
                cardArtImage.enabled = false; // Hide if no art
                if (!string.IsNullOrEmpty(currentCard.cardArtName))
                {
                    Debug.LogWarning($"Card {currentCard.cardName} has cardArtName '{currentCard.cardArtName}' but sprite didn't load");
                }
            }
        }
        
        // Update colors based on card type
        UpdateColors();
    }
    
    /// <summary>
    /// Update card colors based on type and damage type
    /// </summary>
    private void UpdateColors()
    {
        Color backgroundColor = GetCardTypeColor(currentCard.cardType);
        Color borderColor = GetDamageTypeColor(currentCard.primaryDamageType);
        
        if (cardBackground != null)
            cardBackground.color = backgroundColor;
        
        if (cardBorder != null)
            cardBorder.color = borderColor;
    }
    
    /// <summary>
    /// Get background color based on card type
    /// </summary>
    private Color GetCardTypeColor(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Attack:
                return new Color(0.8f, 0.3f, 0.3f, 1f); // Red
            case CardType.Guard:
                return new Color(0.3f, 0.6f, 0.8f, 1f); // Blue
            case CardType.Skill:
                return new Color(0.3f, 0.8f, 0.3f, 1f); // Green
            case CardType.Power:
                return new Color(0.8f, 0.6f, 0.2f, 1f); // Gold
            case CardType.Aura:
                return new Color(0.6f, 0.3f, 0.8f, 1f); // Purple
            default:
                return Color.gray;
        }
    }
    
    /// <summary>
    /// Get border color based on damage type
    /// </summary>
    private Color GetDamageTypeColor(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                return new Color(0.7f, 0.7f, 0.7f, 1f); // Gray
            case DamageType.Fire:
                return new Color(1f, 0.4f, 0f, 1f); // Orange
            case DamageType.Cold:
                return new Color(0.3f, 0.7f, 1f, 1f); // Light Blue
            case DamageType.Lightning:
                return new Color(0.7f, 0.7f, 1f, 1f); // Light Purple
            case DamageType.Chaos:
                return new Color(0.8f, 0.2f, 0.8f, 1f); // Magenta
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Auto-find UI elements if not assigned
    /// </summary>
    private void Awake()
    {
        if (cardNameText == null)
            cardNameText = transform.Find("CardName")?.GetComponent<Text>();
        
        if (cardCostText == null)
            cardCostText = transform.Find("Cost")?.GetComponent<Text>();
        
        if (cardDescriptionText == null)
            cardDescriptionText = transform.Find("Description")?.GetComponent<Text>();
        
        if (cardDamageText == null)
            cardDamageText = transform.Find("Damage")?.GetComponent<Text>();
        
        if (cardTypeText == null)
            cardTypeText = transform.Find("Type")?.GetComponent<Text>();
        
        if (cardBackground == null)
            cardBackground = transform.Find("Background")?.GetComponent<Image>();
        
        if (cardBorder == null)
            cardBorder = GetComponent<Image>();
        
        // ⭐ NEW: Auto-find card art image
        if (cardArtImage == null)
        {
            // Try common names for card art Image component
            // Your prefab has "CardImage" (no space)
            Transform artTransform = transform.Find("CardImage") 
                ?? transform.Find("Card Image") 
                ?? transform.Find("CardArt") 
                ?? transform.Find("Art") 
                ?? transform.Find("Image")
                ?? transform.Find("Artwork");
            
            if (artTransform != null)
            {
                cardArtImage = artTransform.GetComponent<Image>();
                Debug.Log($"<color=green>✓ Found card art Image: {artTransform.name}</color>");
            }
            else
            {
                Debug.LogWarning("[CardArt] No card art Image found. CardVisualizer will not display artwork.");
                Debug.LogWarning($"[CardArt] Searched for: CardImage, Card Image, CardArt, Art, Image, Artwork");
            }
        }
    }
}

