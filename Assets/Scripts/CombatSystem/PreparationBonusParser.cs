using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// Parser for extracting preparation bonuses from card descriptions.
/// Handles patterns like "Prepare: When this card is unleashed, deal 7 damage (+Dex/2)"
/// </summary>
public static class PreparationBonusParser
{
    /// <summary>
    /// Parse preparation bonus damage from card description
    /// Returns: (baseDamage, dexDivisor) or (0, 0) if no bonus found
    /// </summary>
    public static (float baseDamage, float dexDivisor) ParsePreparationDamage(string description)
    {
        if (string.IsNullOrEmpty(description)) return (0f, 0f);
        
        // Pattern: "Prepare: When this card is unleashed, deal X damage (+Dex/Y)"
        var match = Regex.Match(
            description,
            @"Prepare:.*?deal\s+(\d+(?:\.\d+)?)\s+damage\s*(?:\([^)]*Dex\s*/\s*(\d+)[^)]*\))?",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        
        if (match.Success)
        {
            float baseDamage = float.Parse(match.Groups[1].Value);
            float dexDivisor = match.Groups[2].Success ? float.Parse(match.Groups[2].Value) : 0f;
            return (baseDamage, dexDivisor);
        }
        
        return (0f, 0f);
    }
    
    /// <summary>
    /// Parse preparation bonus guard from card description
    /// Returns: (baseGuard, dexDivisor) or (0, 0) if no bonus found
    /// </summary>
    public static (float baseGuard, float dexDivisor) ParsePreparationGuard(string description)
    {
        if (string.IsNullOrEmpty(description)) return (0f, 0f);
        
        // Pattern: "Prepare: Gain +X (+Dex/Y) Guard"
        var match = Regex.Match(
            description,
            @"Prepare:.*?Gain\s+\+?(\d+(?:\.\d+)?)\s*(?:\([^)]*Dex\s*/\s*(\d+)[^)]*\))?\s+Guard",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        
        if (match.Success)
        {
            float baseGuard = float.Parse(match.Groups[1].Value);
            float dexDivisor = match.Groups[2].Success ? float.Parse(match.Groups[2].Value) : 0f;
            return (baseGuard, dexDivisor);
        }
        
        return (0f, 0f);
    }
    
    /// <summary>
    /// Parse preparation bonus temporary Dexterity from card description
    /// Returns: amount of temporary Dexterity to grant, or 0 if not found
    /// </summary>
    public static int ParsePreparationTempDexterity(string description)
    {
        if (string.IsNullOrEmpty(description)) return 0;
        
        // Pattern: "Prepare: ... and X temporary Dexterity"
        var match = Regex.Match(
            description,
            @"Prepare:.*?(\d+)\s+temporary\s+Dexterity",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }
        
        return 0;
    }
    
    /// <summary>
    /// Check if card has a preparation bonus in its description
    /// </summary>
    public static bool HasPreparationBonus(string description)
    {
        if (string.IsNullOrEmpty(description)) return false;
        return Regex.IsMatch(description, @"Prepare:", RegexOptions.IgnoreCase);
    }
}


