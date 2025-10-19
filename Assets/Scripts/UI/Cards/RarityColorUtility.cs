using UnityEngine;

/// <summary>
/// Centralized utility for managing rarity colors across all UI components.
/// Ensures consistent color schemes for card rarity throughout the game.
/// </summary>
public static class RarityColorUtility
{
    /// <summary>
    /// Gets the standard rarity color for text and UI elements.
    /// These colors are optimized for readability and visual distinction.
    /// </summary>
    public static Color GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return new Color(0.7f, 0.7f, 0.7f); // Light grey
            case CardRarity.Magic:
                return new Color(0.4f, 0.6f, 1f); // Blue
            case CardRarity.Rare:
                return new Color(1f, 0.8f, 0.4f); // Gold
            case CardRarity.Unique:
                return new Color(1f, 0.6f, 0.2f); // Orange
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Gets the rarity color with custom alpha for background/frame elements.
    /// </summary>
    /// <param name="rarity">The card rarity</param>
    /// <param name="alpha">Alpha value (0-1)</param>
    /// <returns>Color with specified alpha</returns>
    public static Color GetRarityColorWithAlpha(CardRarity rarity, float alpha)
    {
        Color baseColor = GetRarityColor(rarity);
        return new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }
    
    /// <summary>
    /// Gets the standard rarity frame color (semi-transparent for backgrounds).
    /// </summary>
    public static Color GetRarityFrameColor(CardRarity rarity)
    {
        return GetRarityColorWithAlpha(rarity, 0.3f);
    }
    
    /// <summary>
    /// Gets the enhanced rarity frame color for hover states.
    /// </summary>
    public static Color GetRarityFrameHoverColor(CardRarity rarity)
    {
        return GetRarityColorWithAlpha(rarity, 0.6f);
    }
    
    /// <summary>
    /// Gets the rarity color name for debugging and logging.
    /// </summary>
    public static string GetRarityColorName(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common: return "Light Grey";
            case CardRarity.Magic: return "Blue";
            case CardRarity.Rare: return "Gold";
            case CardRarity.Unique: return "Orange";
            default: return "White";
        }
    }
}







