using System.Collections.Generic;
using UnityEngine;

public static class TooltipFormattingUtils
{
    // Affix colors matching WeaponTooltips.prefab
    private static readonly Color ImplicitColor = new Color(0.8773585f, 0.74985373f, 0.3600481f, 1f); // Gold
    private static readonly Color PrefixColor = new Color(1f, 1f, 0.496f, 1f);                        // Yellow
    private static readonly Color SuffixColor = new Color(1f, 1f, 0.716f, 1f);                        // Light Yellow
    
    public static string ColorizeByRarity(string text, ItemRarity rarity)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        string hex = ItemRarityCalculator.GetRarityColor(rarity);
        return $"<color={hex}>{text}</color>";
    }
    
    public static string ColorizeImplicit(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(ImplicitColor)}>{text}</color>";
    }
    
    public static string ColorizePrefix(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(PrefixColor)}>{text}</color>";
    }
    
    public static string ColorizeSuffix(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(SuffixColor)}>{text}</color>";
    }

    public static string FormatAffix(Affix affix, bool showRanges = false, AffixType? colorOverride = null)
    {
        if (affix == null)
            return string.Empty;

        // If ALT is held (showRanges = true), show original ranges
        // Otherwise show rolled values
        string description = showRanges ? affix.description : GetRolledAffixDescription(affix);
        
        if (!string.IsNullOrEmpty(description))
        {
            string formatted = !string.IsNullOrEmpty(affix.name) 
                ? $"{affix.name}: {description}" 
                : description;
            
            // Apply color based on affix type
            AffixType typeToColor = colorOverride ?? affix.affixType;
            switch (typeToColor)
            {
                case AffixType.Prefix:
                    return ColorizePrefix(formatted);
                case AffixType.Suffix:
                    return ColorizeSuffix(formatted);
                default:
                    return formatted; // No color for unknown types
            }
        }

        return affix.name ?? string.Empty;
    }
    
    public static string FormatImplicit(Affix affix, bool showRanges = false)
    {
        string formatted = FormatAffix(affix, showRanges, null);
        return ColorizeImplicit(formatted);
    }
    
    /// <summary>
    /// Generate affix description using rolled values instead of ranges
    /// </summary>
    public static string GetRolledAffixDescription(Affix affix)
    {
        if (affix == null || affix.modifiers == null || affix.modifiers.Count == 0)
            return affix?.description ?? string.Empty;
        
        // Check if any modifiers have rolled values
        bool hasRolledValues = false;
        foreach (var mod in affix.modifiers)
        {
            if (mod.isRolled) // Changed: removed rolledValue > 0 check (0 is valid!)
            {
                hasRolledValues = true;
                break;
            }
        }
        
        if (!hasRolledValues)
        {
            // No rolled values, use original description
            UnityEngine.Debug.Log($"[TooltipFormat] Affix '{affix.name}' has no rolled values, using original description");
            return affix.description;
        }
        
        UnityEngine.Debug.Log($"[TooltipFormat] Formatting rolled affix '{affix.name}' (isRolled: {affix.modifiers[0].isRolled}, rolledValue: {affix.modifiers[0].rolledValue})");
        
        // Build description using rolled values
        var modifier = affix.modifiers[0]; // Most affixes have one modifier
        
        if (modifier.isDualRange)
        {
            // Dual-range rolled to single value
            // Example: "Adds (1-61) to (84-151) Lightning" → "Adds 111 Lightning"
            // Example: "ADDS (3-6) TO (7-10) CHAOS DAMAGE" → "ADDS 8 CHAOS DAMAGE"
            string desc = affix.description;
            
            // Replace the dual-range pattern with single value
            // Pattern: (34-47) to (72-84) → 63 or (3–6) TO (7–10) → 8
            // NOTE: Handles both hyphen (-) and en-dash (–)
            string pattern = @"\(\d+[-–]\d+\)\s*[tT][oO]\s*\(\d+[-–]\d+\)";
            string replacement = modifier.rolledValue.ToString("F0");
            desc = System.Text.RegularExpressions.Regex.Replace(desc, pattern, replacement);
            
            UnityEngine.Debug.Log($"[TooltipFormat] Dual-range: '{affix.description}' → '{desc}'");
            return desc;
        }
        else
        {
            // Single-range rolled to single value
            // Example: "Adds 41-71 Fire Damage" → "Adds 57 Fire Damage"
            // Example: "+59-78% Increased Lightning" → "+67% Increased Lightning"
            // Example: "Adds 2 to (4–5) Physical Damage" → "Adds 3 Physical Damage"
            string desc = affix.description;
            
            // Special case: "Adds X to (Y-Z)" format (fixed min, variable max in parentheses)
            // Pattern: "Adds 2 to (4–5)" → "Adds 3" (replace entire "X to (Y-Z)" with rolled value)
            string addsToPattern = @"Adds\s+\d+\s+to\s+\([\d]+[-–][\d]+\)";
            if (System.Text.RegularExpressions.Regex.IsMatch(desc, addsToPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                desc = System.Text.RegularExpressions.Regex.Replace(
                    desc, 
                    addsToPattern, 
                    $"Adds {modifier.rolledValue:F0}",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                UnityEngine.Debug.Log($"[TooltipFormat] Single-range (Adds X to Y-Z): '{affix.description}' → '{desc}'");
                return desc;
            }
            
            // Standard range pattern (number-number) with single value
            // Pattern: 41-71 → 57 or +59–78% → +67%
            // NOTE: Handles both hyphen (-) and en-dash (–)
            string pattern = @"\d+[-–]\d+";
            string replacement = modifier.rolledValue.ToString("F0");
            desc = System.Text.RegularExpressions.Regex.Replace(desc, pattern, replacement);
            
            UnityEngine.Debug.Log($"[TooltipFormat] Single-range: '{affix.description}' → '{desc}'");
            return desc;
        }
    }

    public static string FormatRequirements(int requiredLevel, int strength, int dexterity, int intelligence)
    {
        var lines = new List<string>();

        if (requiredLevel > 1)
            lines.Add($"Level {requiredLevel}");

        if (strength > 0)
            lines.Add($"{strength} Strength");

        if (dexterity > 0)
            lines.Add($"{dexterity} Dexterity");

        if (intelligence > 0)
            lines.Add($"{intelligence} Intelligence");

        if (lines.Count == 0)
            return "None";

        return string.Join("\n", lines);
    }
    
    /// <summary>
    /// Format requirements in compact format (e.g., "Req: lvl 2, 15 Str")
    /// </summary>
    public static string FormatRequirementsCompact(int requiredLevel, int strength, int dexterity, int intelligence)
    {
        var parts = new List<string>();

        if (requiredLevel > 1)
            parts.Add($"lvl {requiredLevel}");

        if (strength > 0)
            parts.Add($"{strength} Str");

        if (dexterity > 0)
            parts.Add($"{dexterity} Dex");

        if (intelligence > 0)
            parts.Add($"{intelligence} Int");

        if (parts.Count == 0)
            return "Req: None";

        return "Req: " + string.Join(", ", parts);
    }
}


