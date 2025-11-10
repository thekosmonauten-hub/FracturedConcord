using UnityEngine;

/// <summary>
/// Context for which sprite to use when displaying a card
/// </summary>
public enum CardSpriteContext
{
    Full,       // Full card artwork (high-res, for detailed card display)
    Thumbnail,  // Medium-sized thumbnail (for card rows/lists)
    Icon        // Small icon (for inventory/compact lists)
}

[CreateAssetMenu(fileName = "New Card", menuName = "Dexiled/Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Information")]
    public string cardName = "Card Name";
    
    [Tooltip("Group key for card variants. Cards with the same groupKey are treated as the same card type for embossing/upgrades. Defaults to cardName if empty.")]
    public string groupKey = "";
    
    public string cardType = "Attack";
    public int playCost = 1;
    [TextArea(3, 5)]
    public string description = "Card description";
    
    [Header("Card Properties")]
    public CardRarity rarity = CardRarity.Common;
    public CardElement element = CardElement.Basic;
    public CardCategory category = CardCategory.Attack;
    
    [Header("Visual Assets")]
    [Tooltip("Main card artwork (high-res, for full card display)")]
    public Sprite cardImage;
    
    [Tooltip("Thumbnail/icon artwork (optional, for simplified card rows/lists). Falls back to cardImage if empty.")]
    public Sprite cardThumbnail;
    
    [Tooltip("Small icon (optional, for inventory/deck builder lists). Falls back to cardThumbnail or cardImage if empty.")]
    public Sprite cardIcon;
    
    public Sprite elementFrame;
    public Sprite costBubble;
    public Sprite rarityFrame;
    
    [Header("Card Effects")]
    public int damage = 0;
    public int block = 0;
    public bool isDiscardCard = false;
    public bool isDualWield = false;
    
    [Header("Special Effects")]
    [TextArea(2, 3)]
    public string ifDiscardedEffect = "";
    [TextArea(2, 3)]
    public string dualWieldEffect = "";
    
    [Header("Embossing System")]
    [Tooltip("Number of embossing slots this card has. Higher rarity cards typically have more slots.")]
    [Range(0, 5)]
    public int embossingSlots = 1;
    
    [Header("Card Leveling System")]
    [Tooltip("Current level of this card (1-20). Tracked per groupKey, not per card instance.")]
    [Range(1, 20)]
    public int cardLevel = 1;
    
    [Tooltip("Current experience points for this card. Required XP scales with level.")]
    public int cardExperience = 0;
    
    public string GetDisplayName()
    {
        return cardName;
    }
    
    /// <summary>
    /// Get the appropriate card sprite for the given context.
    /// Falls back to cardImage if specific sprite is not assigned.
    /// </summary>
    public Sprite GetCardSprite(CardSpriteContext context = CardSpriteContext.Full)
    {
        switch (context)
        {
            case CardSpriteContext.Icon:
                // Try icon → thumbnail → full
                return cardIcon != null ? cardIcon : 
                       (cardThumbnail != null ? cardThumbnail : cardImage);
            
            case CardSpriteContext.Thumbnail:
                // Try thumbnail → full
                return cardThumbnail != null ? cardThumbnail : cardImage;
            
            case CardSpriteContext.Full:
            default:
                // Always use full image
                return cardImage;
        }
    }
    
    /// <summary>
    /// Get the group key for this card. Returns cardName if groupKey is not set.
    /// Used for grouping card variants (base, upgraded, foil, etc.) together.
    /// </summary>
    public string GetGroupKey()
    {
        return string.IsNullOrEmpty(groupKey) ? cardName : groupKey;
    }
    
    /// <summary>
    /// Calculate the damage/effect bonus multiplier based on card level.
    /// Max level (20) provides +10% bonus (1.10x multiplier).
    /// </summary>
    public float GetLevelBonusMultiplier()
    {
        // Level 1 = 1.00x (no bonus)
        // Level 20 = 1.10x (+10% bonus)
        // Linear scaling: 0.5% per level
        return 1.0f + ((cardLevel - 1) * 0.005263f); // 0.005263 * 19 levels = ~0.10
    }
    
    /// <summary>
    /// Get experience required for next level.
    /// Uses exponential curve for progression.
    /// </summary>
    public int GetRequiredExperienceForNextLevel()
    {
        if (cardLevel >= 20) return 0; // Max level
        
        // Base XP: 100
        // Scaling: 1.15x per level (exponential growth)
        return Mathf.RoundToInt(100f * Mathf.Pow(1.15f, cardLevel - 1));
    }
    
    /// <summary>
    /// Check if card is ready to level up.
    /// </summary>
    public bool CanLevelUp()
    {
        return cardLevel < 20 && cardExperience >= GetRequiredExperienceForNextLevel();
    }
    
    public string GetFullDescription()
    {
        string fullDesc = description;
        
        if (damage > 0)
        {
            fullDesc += $"\nDeal {damage} damage.";
        }
        
        if (block > 0)
        {
            fullDesc += $"\nGain {block} block.";
        }
        
        if (!string.IsNullOrEmpty(ifDiscardedEffect))
        {
            fullDesc += $"\nIf discarded: {ifDiscardedEffect}";
        }
        
        if (!string.IsNullOrEmpty(dualWieldEffect))
        {
            fullDesc += $"\nDual wield: {dualWieldEffect}";
        }
        
        return fullDesc;
    }
}

public enum CardRarity
{
    Common,
    Magic,
    Rare,
    Unique
}

public enum CardElement
{
    Basic,
    Fire,
    Cold,
    Lightning,
    Physical,
    Chaos
}

public enum CardCategory
{
    Attack,
    Skill,
    Power,
    Guard
}
