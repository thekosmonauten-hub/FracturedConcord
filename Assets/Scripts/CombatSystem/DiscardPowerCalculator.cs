using UnityEngine;

/// <summary>
/// Calculates "Discard Power" based on a discarded card's stats.
/// Discard Power is used in various Apostle card effects.
/// </summary>
public static class DiscardPowerCalculator
{
    /// <summary>
    /// Calculate discard power from a discarded card
    /// Discard Power = base damage + base guard + (scaling bonuses)
    /// </summary>
    public static float CalculateDiscardPower(CardDataExtended card, Character player)
    {
        if (card == null) return 0f;
        
        float discardPower = 0f;
        
        // Base damage
        discardPower += card.damage;
        
        // Base guard
        discardPower += card.block;
        
        // Add attribute scaling bonuses
        if (card.damageScaling != null && player != null)
        {
            discardPower += card.damageScaling.CalculateScalingBonus(player);
        }
        
        if (card.guardScaling != null && player != null)
        {
            discardPower += card.guardScaling.CalculateScalingBonus(player);
        }
        
        // Round to nearest integer (discard power is typically whole numbers)
        return Mathf.RoundToInt(discardPower);
    }
}


