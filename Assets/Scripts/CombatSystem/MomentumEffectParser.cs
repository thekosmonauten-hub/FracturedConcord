using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Parser for momentum effects in card descriptions.
/// Handles patterns like "Gain X Momentum", "Spend all Momentum", "per Momentum spent", etc.
/// </summary>
public static class MomentumEffectParser
{
    /// <summary>
    /// Parse "Gain X Momentum" from description
    /// </summary>
    public static int ParseGainMomentum(string description)
    {
        if (string.IsNullOrEmpty(description)) return 0;
        
        var match = Regex.Match(
            description, 
            @"Gain\s+(\d+)\s+Momentum", 
            RegexOptions.IgnoreCase
        );
        
        if (match.Success && int.TryParse(match.Groups[1].Value, out int amount))
            return amount;
            
        return 0;
    }
    
    /// <summary>
    /// Check if description says "Spend all Momentum"
    /// </summary>
    public static bool ShouldSpendAllMomentum(string description)
    {
        if (string.IsNullOrEmpty(description)) return false;
        return description.Contains("Spend all Momentum", System.StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Parse "Spend X Momentum" or "Spend up to X Momentum" from description
    /// Returns -1 if "Spend all Momentum", otherwise returns the amount
    /// </summary>
    public static int ParseSpendMomentum(string description)
    {
        if (string.IsNullOrEmpty(description)) return 0;
        
        if (ShouldSpendAllMomentum(description))
            return -1; // Special value for "all"
            
        var match = Regex.Match(
            description, 
            @"Spend\s+(?:up\s+to\s+)?(\d+)\s+Momentum", 
            RegexOptions.IgnoreCase
        );
        
        if (match.Success && int.TryParse(match.Groups[1].Value, out int amount))
            return amount;
            
        return 0;
    }
    
    /// <summary>
    /// Parse all momentum thresholds from description (e.g., "If you have 3+ Momentum", "If you have 5+ Momentum")
    /// </summary>
    public static List<int> ParseThresholds(string description)
    {
        var thresholds = new List<int>();
        if (string.IsNullOrEmpty(description)) return thresholds;
        
        // Pattern: "If you have X+ Momentum" or "If you have X Momentum" (exact)
        var matches = Regex.Matches(
            description, 
            @"If\s+you\s+have\s+(\d+)(?:\+|)\s+Momentum", 
            RegexOptions.IgnoreCase
        );
        
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Groups[1].Value, out int threshold))
            {
                if (!thresholds.Contains(threshold))
                    thresholds.Add(threshold);
            }
        }
        
        thresholds.Sort();
        return thresholds;
    }
    
    /// <summary>
    /// Check if description has "per Momentum spent" pattern
    /// </summary>
    public static bool HasPerMomentumSpent(string description)
    {
        if (string.IsNullOrEmpty(description)) return false;
        return Regex.IsMatch(description, @"per\s+Momentum\s+spent", RegexOptions.IgnoreCase);
    }
    
    /// <summary>
    /// Parse damage amount from "Deal X damage per Momentum spent" pattern
    /// Returns the base damage amount (e.g., 3 from "Deal 3 damage per Momentum spent")
    /// Also handles patterns like "Deal 3 damage (+dex/6) per Momentum spent"
    /// </summary>
    public static float ParseDamagePerMomentum(string description)
    {
        if (string.IsNullOrEmpty(description)) return 0f;
        
        // Pattern 1: "Deal X damage per Momentum spent" or "Deal X physical damage per Momentum spent"
        // Pattern 2: "Deal X damage (+STAT/Y) per Momentum spent"
        var match = Regex.Match(
            description,
            @"Deal\s+(\d+(?:\.\d+)?)\s+(?:\w+\s+)?damage\s+(?:\([^)]+\)\s+)?per\s+Momentum\s+spent",
            RegexOptions.IgnoreCase
        );
        
        if (match.Success && float.TryParse(match.Groups[1].Value, out float damage))
            return damage;
            
        return 0f;
    }
    
    /// <summary>
    /// Parse attribute scaling from "Deal X damage (+STAT/Y) per Momentum spent" pattern
    /// Returns a tuple: (attribute type, divisor)
    /// Attribute type: "str", "dex", "int", or empty string if none
    /// </summary>
    public static (string attribute, float divisor) ParseScalingPerMomentum(string description)
    {
        if (string.IsNullOrEmpty(description)) return ("", 0f);
        
        // Pattern: "(+STR/6)", "(+DEX/6)", "(+INT/6)", etc.
        var match = Regex.Match(
            description,
            @"\(\+(\w+)/(\d+(?:\.\d+)?)\)",
            RegexOptions.IgnoreCase
        );
        
        if (match.Success)
        {
            string attr = match.Groups[1].Value.ToLower();
            if (float.TryParse(match.Groups[2].Value, out float divisor))
            {
                // Normalize attribute name
                if (attr.StartsWith("str")) return ("str", divisor);
                if (attr.StartsWith("dex")) return ("dex", divisor);
                if (attr.StartsWith("int")) return ("int", divisor);
            }
        }
        
        return ("", 0f);
    }
    
    /// <summary>
    /// Calculate total damage for "per Momentum spent" effects
    /// Example: "Deal 3 damage (+dex/6) per Momentum spent" with 5 momentum
    /// Returns: (3 + dex/6) * 5
    /// 
    /// If description uses {damage} placeholder, uses card.baseDamage as fallback.
    /// </summary>
    public static float CalculatePerMomentumDamage(string description, int momentumSpent, Character character, Card card = null)
    {
        if (momentumSpent <= 0) return 0f;
        
        float baseDamagePerMomentum = ParseDamagePerMomentum(description);
        
        // If parser couldn't find a number (e.g., description uses {damage} placeholder), use card's baseDamage
        if (baseDamagePerMomentum <= 0f && card != null)
        {
            baseDamagePerMomentum = card.baseDamage;
            Debug.Log($"<color=yellow>[Momentum] Using card.baseDamage ({card.baseDamage}) as base damage per momentum (description uses placeholder)</color>");
        }
        
        if (baseDamagePerMomentum <= 0f)
        {
            Debug.LogWarning($"[Momentum] CalculatePerMomentumDamage: baseDamagePerMomentum is 0! Description: {description}, card.baseDamage: {(card != null ? card.baseDamage.ToString() : "null")}");
            return 0f;
        }
        
        // Get attribute scaling
        var (attribute, divisor) = ParseScalingPerMomentum(description);
        float scalingBonus = 0f;
        
        if (character != null)
        {
            if (divisor > 0f)
            {
                // Use scaling from description (e.g., "+dex/6")
                switch (attribute)
                {
                    case "str":
                        scalingBonus = character.strength / divisor;
                        break;
                    case "dex":
                        scalingBonus = character.dexterity / divisor;
                        break;
                    case "int":
                        scalingBonus = character.intelligence / divisor;
                        break;
                }
            }
            else if (card != null && card.damageScaling != null)
            {
                // Fallback: Use card's damageScaling if description doesn't specify scaling
                scalingBonus = card.damageScaling.CalculateScalingBonus(character);
            }
        }
        
        // Calculate damage per momentum instance
        float damagePerInstance = baseDamagePerMomentum + scalingBonus;
        
        // Multiply by momentum spent
        return damagePerInstance * momentumSpent;
    }
    
    /// <summary>
    /// Parse guard amount from "Gain X Guard per Momentum spent" pattern
    /// Returns the base guard amount (e.g., 4 from "Gain 4 Guard per Momentum spent")
    /// </summary>
    public static float ParseGuardPerMomentum(string description)
    {
        if (string.IsNullOrEmpty(description)) return 0f;
        
        // Pattern: "Gain X Guard per Momentum spent"
        var match = Regex.Match(
            description,
            @"Gain\s+(\d+(?:\.\d+)?)\s+Guard\s+per\s+Momentum\s+spent",
            RegexOptions.IgnoreCase
        );
        
        if (match.Success && float.TryParse(match.Groups[1].Value, out float guard))
            return guard;
            
        return 0f;
    }
    
    /// <summary>
    /// Check if description has "Guard per Momentum spent" pattern
    /// </summary>
    public static bool HasGuardPerMomentumSpent(string description)
    {
        if (string.IsNullOrEmpty(description)) return false;
        return Regex.IsMatch(description, @"Guard\s+per\s+Momentum\s+spent", RegexOptions.IgnoreCase);
    }
    
    /// <summary>
    /// Calculate total guard for "per Momentum spent" effects
    /// Example: "Gain 4 Guard per Momentum spent" with 2 momentum spent
    /// Returns: 4 * 2 = 8
    /// 
    /// If card is provided, uses card's base guard and guard scaling instead of parsing from description.
    /// This allows {guard} placeholder to work with attribute scaling.
    /// Example: block=4, dexterityDivisor=8, dex=16 → (4 + 16/8) = 6 guard per momentum
    /// </summary>
    public static float CalculatePerMomentumGuard(string description, int momentumSpent, Card card = null, Character character = null)
    {
        if (momentumSpent <= 0) return 0f;
        
        float guardPerMomentum = 0f;
        
        // If card is provided, use card's guard calculation (includes base + scaling)
        if (card != null && character != null)
        {
            // Use DamageCalculator to get total guard (base + scaling)
            // This includes: baseGuard + guardScaling.CalculateScalingBonus(character)
            // Example: block=4, dexterityDivisor=8, dex=16 → (4 + 16/8) = 6
            guardPerMomentum = DamageCalculator.CalculateGuardAmount(card, character);
        }
        else
        {
            // Fallback: Parse from description (literal number)
            guardPerMomentum = ParseGuardPerMomentum(description);
        }
        
        if (guardPerMomentum <= 0f) return 0f;
        
        // Multiply guard per momentum by momentum spent
        return guardPerMomentum * momentumSpent;
    }
}

