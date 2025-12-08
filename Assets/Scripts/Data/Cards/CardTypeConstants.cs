using UnityEngine;

/// <summary>
/// Centralized constants for card type properties.
/// Provides base critical strike chances and other card type defaults.
/// </summary>
public static class CardTypeConstants
{
    // Base critical strike chances by card type
    public const float ATTACK_BASE_CRIT = 2f;      // 2% for Attack cards
    public const float SPELL_BASE_CRIT = 5f;       // 5% for Spell cards (Skill/Power)
    public const float POWER_BASE_CRIT = 5f;       // 5% for Power cards
    public const float DEFAULT_BASE_CRIT = 0f;     // 0% for other types
    
    /// <summary>
    /// Get base critical strike chance for a card type.
    /// Attack cards: 2%
    /// Spell cards (Skill/Power): 5%
    /// Others: 0%
    /// </summary>
    public static float GetBaseCritChance(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Attack:
                return ATTACK_BASE_CRIT;
            case CardType.Skill:
            case CardType.Power:
                return SPELL_BASE_CRIT;
            default:
                return DEFAULT_BASE_CRIT;
        }
    }
    
    /// <summary>
    /// Get base critical strike chance for a card (convenience method).
    /// Checks both cardType and tags for spell detection.
    /// </summary>
    public static float GetBaseCritChance(Card card)
    {
        if (card == null) return DEFAULT_BASE_CRIT;
        
        // Check if it's explicitly a spell via tags
        bool isSpell = card.tags != null && (card.tags.Contains("Spell") || card.tags.Contains("spell"));
        
        if (isSpell)
        {
            return SPELL_BASE_CRIT;
        }
        
        return GetBaseCritChance(card.cardType);
    }
}

