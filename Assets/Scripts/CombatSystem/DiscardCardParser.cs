using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// Parser for detecting and parsing discard card requirements from card descriptions.
/// Handles patterns like "Discard 1 card: draw 3 cards" or "Discard 1 card. Gain 6 Guard"
/// </summary>
public static class DiscardCardParser
{
    /// <summary>
    /// Check if a card requires discarding other cards
    /// </summary>
    public static bool RequiresDiscard(string description)
    {
        if (string.IsNullOrEmpty(description)) return false;
        
        // Pattern: "Discard X card" or "Discard X cards"
        return Regex.IsMatch(description, @"Discard\s+\d+\s+card", RegexOptions.IgnoreCase);
    }
    
    /// <summary>
    /// Parse how many cards need to be discarded
    /// </summary>
    public static int ParseDiscardCount(string description)
    {
        if (string.IsNullOrEmpty(description)) return 0;
        
        var match = Regex.Match(description, @"Discard\s+(\d+)\s+card", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }
        
        return 0;
    }
    
    /// <summary>
    /// Check if description has "ifDiscardedEffect" pattern
    /// </summary>
    public static bool HasIfDiscardedEffect(string ifDiscardedEffect)
    {
        return !string.IsNullOrEmpty(ifDiscardedEffect);
    }
}


