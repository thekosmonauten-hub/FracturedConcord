using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum ItemRarity
{
    Normal,     // White - No affixes
    Magic,      // Blue - 1-2 affixes (0-1 prefix, 0-1 suffix)
    Rare,       // Gold - 3-6 affixes (1-3 prefix, 1-3 suffix, always at least 3 total)
    Unique      // Orange - Fixed affixes, non-random
}

public enum Handedness
{
    Both,       // Compatible with both one-handed and two-handed weapons
    OneHand,    // Only compatible with one-handed weapons
    TwoHand     // Only compatible with two-handed weapons
}

[System.Serializable]
public class Affix
{
    public string name;
    public string description;
    public AffixType affixType; // Prefix or Suffix
    public AffixTier tier;      // Tier 1-10 for random generation
    public List<AffixModifier> modifiers = new List<AffixModifier>();
    public List<string> requiredTags = new List<string>(); // Item tags this affix can apply to (legacy)
    public List<string> compatibleTags = new List<string>(); // New smart compatibility tags
    public float weight = 100f; // Drop weight for random generation
    public Handedness handedness = Handedness.Both; // Handedness requirement
    public int minLevel = 1; // Minimum item level required for this affix to roll
    
    // Range values for the affix (parsed from description)
    public int minValue = 0;
    public int maxValue = 0;
    public bool hasRange = false; // Whether this affix has a valid range
    
    // Rolled value (single number that gets added to weapon damage)
    public int rolledValue = 0;
    public bool isRolled = false; // Whether this affix has been rolled
    
    public Affix(string name, string description, AffixType affixType, AffixTier tier)
    {
        this.name = name;
        this.description = description;
        this.affixType = affixType;
        this.tier = tier;
        this.modifiers = new List<AffixModifier>();
        this.requiredTags = new List<string>();
        this.weight = 1000f; // Default weight
        this.handedness = Handedness.Both; // Default to both
        this.minValue = 0;
        this.maxValue = 0;
        this.hasRange = false;
        this.rolledValue = 0;
        this.isRolled = false;
    }
    
    /// <summary>
    /// Generates a "rolled" version of this affix with actual values instead of ranges
    /// </summary>
    /// <returns>A new Affix with rolled values</returns>
    public Affix GenerateRolledAffix()
    {
        return GenerateRolledAffix(Random.Range(0, int.MaxValue));
    }
    
    /// <summary>
    /// Rolls a single value from the affix's range and returns a new affix with that value
    /// </summary>
    /// <param name="seed">Random seed for reproducible results</param>
    /// <returns>A new Affix with a single rolled value</returns>
    public Affix GenerateRolledAffix(int seed)
    {
        Affix rolledAffix = new Affix(name, description, affixType, tier);
        rolledAffix.weight = weight;
        rolledAffix.requiredTags = new List<string>(requiredTags);
        rolledAffix.handedness = handedness;
        rolledAffix.minValue = minValue;
        rolledAffix.maxValue = maxValue;
        rolledAffix.hasRange = hasRange;
        
        // Roll modifiers (handle dual-range for damage affixes)
        System.Random random = new System.Random(seed);
        
        // Roll a single value from the range
        if (hasRange && minValue > 0 && maxValue > 0)
        {
            // Use the seed to generate a random value within the range
            rolledAffix.rolledValue = random.Next(minValue, maxValue + 1); // +1 because Next is exclusive of max
            rolledAffix.isRolled = true;
        }
        else
        {
            rolledAffix.rolledValue = 0;
            rolledAffix.isRolled = false;
        }
        
        // Check if this is an "All Attributes" affix (has Strength, Dexterity, and Intelligence modifiers)
        // OR if the description mentions "all attributes" (fallback detection)
        bool isAllAttributesAffix = false;
        bool hasStrength = false;
        bool hasDexterity = false;
        bool hasIntelligence = false;
        float? sharedRollValue = null;
        
        // First check: Look for "AllAttributes" stat name directly
        bool hasAllAttributesStat = modifiers.Any(m => m != null && m.statName == "AllAttributes");
        
        // Second check: Check if we have all three attribute modifiers
        foreach (var mod in modifiers)
        {
            if (mod != null)
            {
                if (mod.statName == "Strength") hasStrength = true;
                else if (mod.statName == "Dexterity") hasDexterity = true;
                else if (mod.statName == "Intelligence") hasIntelligence = true;
            }
        }
        
        // Third check: Check description for "all attributes" (case-insensitive)
        bool descriptionHasAllAttributes = !string.IsNullOrEmpty(description) && 
            (description.Contains("all attributes", System.StringComparison.OrdinalIgnoreCase) ||
             description.Contains("All Attributes", System.StringComparison.OrdinalIgnoreCase));
        
        // If we have the "AllAttributes" stat directly, or all three attributes, or description mentions it
        if (hasAllAttributesStat || (hasStrength && hasDexterity && hasIntelligence) || descriptionHasAllAttributes)
        {
            isAllAttributesAffix = true;
            
            // If we already have "AllAttributes" stat, use it directly
            if (hasAllAttributesStat)
            {
                var allAttributesMod = modifiers.FirstOrDefault(m => m != null && m.statName == "AllAttributes");
                if (allAttributesMod != null)
                {
                    float rollMin = (allAttributesMod.minValue > 0 || allAttributesMod.originalMinValue > 0) 
                        ? (allAttributesMod.minValue > 0 ? allAttributesMod.minValue : allAttributesMod.originalMinValue)
                        : 0f;
                    float rollMax = (allAttributesMod.maxValue > 0 || allAttributesMod.originalMaxValue > 0)
                        ? (allAttributesMod.maxValue > 0 ? allAttributesMod.maxValue : allAttributesMod.originalMaxValue)
                        : 0f;
                    
                    if (rollMax > rollMin)
                    {
                        sharedRollValue = random.Next((int)rollMin, (int)rollMax + 1);
                    }
                    else if (rollMin > 0)
                    {
                        sharedRollValue = rollMin;
                    }
                }
            }
            else
            {
                // Find the first attribute modifier to get the range (they should all have the same range)
                var firstAttributeMod = modifiers.FirstOrDefault(m => m != null && 
                    (m.statName == "Strength" || m.statName == "Dexterity" || m.statName == "Intelligence"));
                
                if (firstAttributeMod != null)
                {
                    // Roll once for all attributes
                    float rollMin = (firstAttributeMod.minValue > 0 || firstAttributeMod.originalMinValue > 0) 
                        ? (firstAttributeMod.minValue > 0 ? firstAttributeMod.minValue : firstAttributeMod.originalMinValue)
                        : 0f;
                    float rollMax = (firstAttributeMod.maxValue > 0 || firstAttributeMod.originalMaxValue > 0)
                        ? (firstAttributeMod.maxValue > 0 ? firstAttributeMod.maxValue : firstAttributeMod.originalMaxValue)
                        : 0f;
                    
                    if (rollMax > rollMin)
                    {
                        sharedRollValue = random.Next((int)rollMin, (int)rollMax + 1);
                        Debug.Log($"[All Attributes Roll] Detected All Attributes affix '{name}'. Rolling once for all attributes: {rollMin}-{rollMax} → {sharedRollValue}");
                    }
                    else if (rollMin > 0)
                    {
                        sharedRollValue = rollMin;
                        Debug.Log($"[All Attributes Roll] Detected All Attributes affix '{name}'. Fixed value for all attributes: {sharedRollValue}");
                    }
                    else
                    {
                        Debug.LogWarning($"[All Attributes Roll] Detected All Attributes affix '{name}' but couldn't determine roll range (min={rollMin}, max={rollMax})");
                    }
                }
                else if (descriptionHasAllAttributes)
                {
                    // Fallback: Try to extract range from description if we can't find modifiers
                    // This handles cases where the affix description says "all attributes" but modifiers aren't set up correctly
                    var match = System.Text.RegularExpressions.Regex.Match(description, @"([+-]?)(\d+)[-–—](\d+)");
                    if (match.Success)
                    {
                        float rollMin = float.Parse(match.Groups[2].Value);
                        float rollMax = float.Parse(match.Groups[3].Value);
                        if (rollMax > rollMin)
                        {
                            sharedRollValue = random.Next((int)rollMin, (int)rollMax + 1);
                            Debug.Log($"[All Attributes Roll] Detected All Attributes affix '{name}' from description. Rolling once: {rollMin}-{rollMax} → {sharedRollValue}");
                        }
                    }
                }
                
                if (!sharedRollValue.HasValue)
                {
                    Debug.LogWarning($"[All Attributes Roll] Detected All Attributes affix '{name}' but couldn't determine roll range");
                }
            }
        }
        
        // If this is an "All Attributes" affix, convert it to use "AllAttributes" stat name
        // This replaces the three separate modifiers with a single "AllAttributes" modifier
        if (isAllAttributesAffix && sharedRollValue.HasValue)
        {
            Debug.Log($"[All Attributes Conversion] Converting affix '{name}' to use AllAttributes stat. Detected via: {(hasAllAttributesStat ? "AllAttributes stat" : (hasStrength && hasDexterity && hasIntelligence ? "three modifiers" : "description"))}");
            
            // Find the first attribute modifier to get description and other properties
            // If we already have AllAttributes stat, use that; otherwise use first attribute modifier
            var firstAttributeMod = hasAllAttributesStat 
                ? modifiers.FirstOrDefault(m => m != null && m.statName == "AllAttributes")
                : modifiers.FirstOrDefault(m => m != null && 
                    (m.statName == "Strength" || m.statName == "Dexterity" || m.statName == "Intelligence"));
            
            // Create a single "AllAttributes" modifier instead of three separate ones
            // Even if firstAttributeMod is null (detected from description only), we can still create the modifier
            AffixModifier allAttributesModifier = new AffixModifier(
                "AllAttributes",
                sharedRollValue.Value,
                sharedRollValue.Value,
                firstAttributeMod?.modifierType ?? ModifierType.Flat
            );
            
            // Copy properties from the first attribute modifier if available
            if (firstAttributeMod != null)
            {
                allAttributesModifier.scope = firstAttributeMod.scope;
                allAttributesModifier.damageType = firstAttributeMod.damageType;
                allAttributesModifier.originalMinValue = firstAttributeMod.originalMinValue;
                allAttributesModifier.originalMaxValue = firstAttributeMod.originalMaxValue;
            }
            else
            {
                // Default values when detected from description only
                allAttributesModifier.scope = ModifierScope.Global;
                allAttributesModifier.damageType = DamageType.None;
                // Try to extract original range from description
                var match = System.Text.RegularExpressions.Regex.Match(description ?? "", @"([+-]?)(\d+)[-–—](\d+)");
                if (match.Success)
                {
                    allAttributesModifier.originalMinValue = float.Parse(match.Groups[2].Value);
                    allAttributesModifier.originalMaxValue = float.Parse(match.Groups[3].Value);
                }
            }
            
            // Create description - use the affix description if it mentions "All Attributes", otherwise create one
            string baseDescription = description ?? firstAttributeMod?.description ?? "";
            if (baseDescription.Contains("All Attributes", System.StringComparison.OrdinalIgnoreCase) || 
                baseDescription.Contains("all attributes", System.StringComparison.OrdinalIgnoreCase))
            {
                // Replace any value in the description with the rolled value
                allAttributesModifier.description = System.Text.RegularExpressions.Regex.Replace(
                    baseDescription, 
                    @"[+-]?\d+(?:\.\d+)?(?:[-–—]\d+(?:\.\d+)?)?", 
                    $"+{sharedRollValue.Value:F0}",
                    System.Text.RegularExpressions.RegexOptions.None,
                    System.TimeSpan.FromMilliseconds(100)
                );
            }
            else
            {
                // Create new description
                allAttributesModifier.description = $"+{sharedRollValue.Value:F0} to All Attributes";
            }
            
            allAttributesModifier.rolledValue = sharedRollValue.Value;
            allAttributesModifier.isRolled = true;
            
            // Replace all modifiers with the single "AllAttributes" modifier
            rolledAffix.modifiers.Clear();
            rolledAffix.modifiers.Add(allAttributesModifier);
            
            Debug.Log($"[All Attributes Roll] Converted '{name}' to use 'AllAttributes' stat name with value {sharedRollValue.Value}. Replaced {modifiers.Count} modifiers with 1 AllAttributes modifier.");
            
            // Skip the normal modifier processing loop since we've already handled it
            return rolledAffix;
        }
        
        foreach (var modifier in modifiers)
        {
            // Skip individual attribute modifiers if this was detected as "All Attributes" but conversion didn't happen
            // (This shouldn't happen if conversion worked, but serves as a safety check)
            if (isAllAttributesAffix && 
                (modifier.statName == "Strength" || modifier.statName == "Dexterity" || modifier.statName == "Intelligence"))
            {
                Debug.LogWarning($"[All Attributes] Skipping individual attribute modifier '{modifier.statName}' - affix '{name}' should have been converted to AllAttributes but wasn't!");
                continue;
            }
            
            // Create a copy of the modifier
            AffixModifier rolledModifier = new AffixModifier(
                modifier.statName,
                modifier.minValue,
                modifier.maxValue,
                modifier.modifierType
            );
            
            // Copy all properties
            rolledModifier.scope = modifier.scope;
            rolledModifier.damageType = modifier.damageType;
            rolledModifier.description = modifier.description;
            rolledModifier.originalMinValue = modifier.originalMinValue;
            rolledModifier.originalMaxValue = modifier.originalMaxValue;
            
            // Handle dual-range modifiers
            if (modifier.isDualRange)
            {
                rolledModifier.isDualRange = true;
                rolledModifier.firstRangeMin = modifier.firstRangeMin;
                rolledModifier.firstRangeMax = modifier.firstRangeMax;
                rolledModifier.secondRangeMin = modifier.secondRangeMin;
                rolledModifier.secondRangeMax = modifier.secondRangeMax;
                
                // STEP 1: Roll each range to get two values
                rolledModifier.rolledFirstValue = random.Next((int)modifier.firstRangeMin, (int)modifier.firstRangeMax + 1);
                rolledModifier.rolledSecondValue = random.Next((int)modifier.secondRangeMin, (int)modifier.secondRangeMax + 1);
                
                // STEP 2: Roll between those two values to get FINAL single value
                int minRolled = (int)rolledModifier.rolledFirstValue;
                int maxRolled = (int)rolledModifier.rolledSecondValue;
                rolledModifier.rolledValue = random.Next(minRolled, maxRolled + 1);
                rolledModifier.isRolled = true;
                
                // Update minValue to be the final rolled value (for GetModifierValue)
                rolledModifier.minValue = rolledModifier.rolledValue;
                rolledModifier.maxValue = rolledModifier.rolledValue;
                
                Debug.Log($"[Dual-Range Roll] {modifier.statName}: ({rolledModifier.firstRangeMin}-{rolledModifier.firstRangeMax}) to ({rolledModifier.secondRangeMin}-{rolledModifier.secondRangeMax}) → {rolledModifier.rolledFirstValue}-{rolledModifier.rolledSecondValue} → FINAL: {rolledModifier.rolledValue}");
            }
            else
            {
                // Check if this is an attribute modifier in an "All Attributes" affix
                bool isAttributeModifier = modifier.statName == "Strength" || modifier.statName == "Dexterity" || modifier.statName == "Intelligence";
                
                // If this is an "All Attributes" affix and this is an attribute modifier, use the shared roll value
                if (isAllAttributesAffix && isAttributeModifier && sharedRollValue.HasValue)
                {
                    rolledModifier.rolledValue = sharedRollValue.Value;
                    rolledModifier.isRolled = true;
                    rolledModifier.minValue = rolledModifier.rolledValue;
                    rolledModifier.maxValue = rolledModifier.rolledValue;
                    Debug.Log($"[All Attributes Roll] {modifier.statName}: Using shared roll value {sharedRollValue.Value} (affix: '{name}')");
                }
                else if (isAllAttributesAffix && isAttributeModifier && !sharedRollValue.HasValue)
                {
                    Debug.LogWarning($"[All Attributes Roll] {modifier.statName}: All Attributes affix detected but sharedRollValue is null! Affix: '{name}'");
                }
                else
                {
                    // Single-range modifier: Roll from min to max
                    // Use originalMinValue/originalMaxValue if minValue/maxValue are 0 or invalid (fallback)
                    float rollMin = (modifier.minValue > 0 || modifier.originalMinValue > 0) 
                        ? (modifier.minValue > 0 ? modifier.minValue : modifier.originalMinValue)
                        : 0f;
                    float rollMax = (modifier.maxValue > 0 || modifier.originalMaxValue > 0)
                        ? (modifier.maxValue > 0 ? modifier.maxValue : modifier.originalMaxValue)
                        : 0f;
                    
                    // If we still don't have valid values, try using the affix's range values as last resort
                    if (rollMin == 0 && rollMax == 0 && modifier.originalMinValue == 0 && modifier.originalMaxValue == 0)
                    {
                        // This shouldn't happen, but log it
                        Debug.LogWarning($"[Roll Error] {modifier.statName}: All values are 0 - minValue={modifier.minValue}, maxValue={modifier.maxValue}, originalMin={modifier.originalMinValue}, originalMax={modifier.originalMaxValue}");
                        rolledModifier.rolledValue = 0;
                        rolledModifier.isRolled = false;
                    }
                    else if (rollMax > rollMin)
                    {
                        rolledModifier.rolledValue = random.Next((int)rollMin, (int)rollMax + 1);
                        rolledModifier.isRolled = true;
                        
                        // Update minValue/maxValue to be the final rolled value (for GetModifierValue)
                        rolledModifier.minValue = rolledModifier.rolledValue;
                        rolledModifier.maxValue = rolledModifier.rolledValue;
                        
                        Debug.Log($"[Single-Range Roll] {modifier.statName}: {rollMin}-{rollMax} (using {(modifier.minValue > 0 ? "minValue/maxValue" : "originalMinValue/originalMaxValue")}) → {rolledModifier.rolledValue}");
                    }
                    else if (rollMin > 0)
                    {
                        // Fixed value (no range) - min and max are the same
                        rolledModifier.rolledValue = rollMin;
                        rolledModifier.isRolled = true;
                        rolledModifier.minValue = rolledModifier.rolledValue;
                        rolledModifier.maxValue = rolledModifier.rolledValue;
                        Debug.Log($"[Fixed Value Roll] {modifier.statName}: {rollMin} (fixed value, no range)");
                    }
                    else
                    {
                        // Invalid range (max <= min and both might be 0)
                        rolledModifier.rolledValue = 0;
                        rolledModifier.isRolled = false;
                        Debug.LogWarning($"[Roll Warning] {modifier.statName}: Invalid range - rollMin={rollMin}, rollMax={rollMax}, minValue={modifier.minValue}, maxValue={modifier.maxValue}, originalMin={modifier.originalMinValue}, originalMax={modifier.originalMaxValue}");
                    }
                }
            }
            
            rolledAffix.modifiers.Add(rolledModifier);
        }
        
        return rolledAffix;
    }
    
    /// <summary>
    /// Rolls a single value from the affix's range and stores it in rolledValue
    /// </summary>
    /// <param name="seed">Random seed for reproducible results</param>
    public void RollAffix(int seed)
    {
        if (hasRange && minValue > 0 && maxValue > 0)
        {
            // Use the seed to generate a random value within the range
            System.Random random = new System.Random(seed + name.GetHashCode());
            rolledValue = random.Next(minValue, maxValue + 1); // +1 because Next is exclusive of max
            isRolled = true;
        }
        else
        {
            rolledValue = 0;
            isRolled = false;
        }
    }
    
    /// <summary>
    /// Rolls a single value from the affix's range using Unity's Random
    /// </summary>
    public void RollAffix()
    {
        if (hasRange && minValue > 0 && maxValue > 0)
        {
            // Use Unity's Random for convenience
            rolledValue = Random.Range(minValue, maxValue + 1); // +1 because Range is inclusive
            isRolled = true;
        }
        else
        {
            rolledValue = 0;
            isRolled = false;
        }
    }
}

public enum AffixType
{
    Prefix,
    Suffix
}

public enum AffixTier
{
    Tier1,  // Highest tier (best stats)
    Tier2,
    Tier3,
    Tier4,
    Tier5,
    Tier6,
    Tier7,
    Tier8,
    Tier9,
    Tier10  // Lowest tier (worst stats)
}

[System.Serializable]
public class AffixModifier
{
    public string statName;
    public float minValue;
    public float maxValue;
    public ModifierType modifierType;
    public DamageType damageType = DamageType.None;
    public ModifierScope scope = ModifierScope.Global; // Default to global for safety
    public string description; // Optional description for complex modifiers
    
    // Store original range for rolled affixes
    public float originalMinValue;
    public float originalMaxValue;
    
    // Dual-range support for modifiers like "Adds (6-9) to (13-15)"
    public bool isDualRange = false;
    public float firstRangeMin;
    public float firstRangeMax;
    public float secondRangeMin;
    public float secondRangeMax;
    public float rolledFirstValue;  // First range result (e.g., 41 from 1-61)
    public float rolledSecondValue; // Second range result (e.g., 143 from 84-151)
    
    // FINAL rolled value (single number for card damage)
    // For single-range: Rolled from minValue-maxValue
    // For dual-range: Rolled from rolledFirstValue-rolledSecondValue
    public float rolledValue = 0f;
    public bool isRolled = false;
    
    public AffixModifier(string statName, float minValue, float maxValue, ModifierType modifierType)
    {
        this.statName = statName;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.modifierType = modifierType;
        this.originalMinValue = minValue;
        this.originalMaxValue = maxValue;
    }
    
    public AffixModifier(string statName, float minValue, float maxValue, ModifierType modifierType, ModifierScope scope)
    {
        this.statName = statName;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.modifierType = modifierType;
        this.scope = scope;
        this.originalMinValue = minValue;
        this.originalMaxValue = maxValue;
    }
    
    /// <summary>
    /// Creates a dual-range modifier for patterns like "Adds (6-9) to (13-15)"
    /// </summary>
    public AffixModifier(string statName, float firstMin, float firstMax, float secondMin, float secondMax, ModifierType modifierType, ModifierScope scope)
    {
        this.statName = statName;
        this.modifierType = modifierType;
        this.scope = scope;
        this.isDualRange = true;
        
        // Store the dual ranges
        this.firstRangeMin = firstMin;
        this.firstRangeMax = firstMax;
        this.secondRangeMin = secondMin;
        this.secondRangeMax = secondMax;
        
        // Set the overall range (min of first range to max of second range)
        this.minValue = firstMin;
        this.maxValue = secondMax;
        this.originalMinValue = firstMin;
        this.originalMaxValue = secondMax;
    }
    
    /// <summary>
    /// Gets a random value within the modifier's range
    /// </summary>
    /// <returns>A random integer between minValue and maxValue (inclusive)</returns>
    public float GetRandomValue()
    {
        return Random.Range((int)minValue, (int)maxValue + 1);
    }
    
    /// <summary>
    /// Gets a random value within the modifier's range using a specific seed
    /// </summary>
    /// <param name="seed">Random seed for reproducible results</param>
    /// <returns>A random integer between minValue and maxValue (inclusive)</returns>
    public float GetRandomValue(int seed)
    {
        Random.State originalState = Random.state;
        Random.InitState(seed);
        float result = Random.Range((int)minValue, (int)maxValue + 1);
        Random.state = originalState;
        return result;
    }
    
    /// <summary>
    /// Gets random values for dual-range modifiers
    /// </summary>
    /// <param name="seed">Random seed for reproducible results</param>
    /// <returns>A tuple with (firstValue, secondValue)</returns>
    public (float first, float second) GetDualRandomValues(int seed)
    {
        if (!isDualRange)
        {
            float singleValue = GetRandomValue(seed);
            return (singleValue, singleValue);
        }
        
        Random.State originalState = Random.state;
        Random.InitState(seed);
        
        float firstValue = Random.Range((int)firstRangeMin, (int)firstRangeMax + 1);
        Random.InitState(seed + 1000); // Use different seed for second value
        float secondValue = Random.Range((int)secondRangeMin, (int)secondRangeMax + 1);
        
        Random.state = originalState;
        return (firstValue, secondValue);
    }
    
    /// <summary>
    /// Gets the current damage range for dual-range modifiers
    /// </summary>
    /// <returns>A tuple with (minDamage, maxDamage)</returns>
    public (float min, float max) GetCurrentDamageRange()
    {
        if (isDualRange)
        {
            return (rolledFirstValue, rolledSecondValue);
        }
        else
        {
            return (minValue, maxValue);
        }
    }
}

public enum ModifierType
{
    Flat,
    Increased,
    More,
    Reduced,
    Less
}


public static class ItemRarityCalculator
{
    public static ItemRarity CalculateRarity(int prefixCount, int suffixCount, bool isUnique = false)
    {
        if (isUnique)
            return ItemRarity.Unique;
            
        int totalAffixes = prefixCount + suffixCount;
        
        if (totalAffixes == 0)
            return ItemRarity.Normal;
        else if (totalAffixes <= 2)
            return ItemRarity.Magic;
        else if (totalAffixes >= 3 && totalAffixes <= 6)
            return ItemRarity.Rare;
        else
            return ItemRarity.Rare; // Cap at 6 affixes (3 prefix + 3 suffix)
    }
    
    public static bool IsValidAffixCount(int prefixCount, int suffixCount)
    {
        return prefixCount >= 0 && prefixCount <= 3 && 
               suffixCount >= 0 && suffixCount <= 3;
    }
    
    public static string GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return "#FFFFFF"; // White
            case ItemRarity.Magic: return "#8888FF";  // Blue
            case ItemRarity.Rare: return "#FFD700";   // Gold
            case ItemRarity.Unique: return "#FF8C00"; // Orange
            default: return "#FFFFFF";
        }
    }
    
    public static string GetRarityName(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return "Normal";
            case ItemRarity.Magic: return "Magic";
            case ItemRarity.Rare: return "Rare";
            case ItemRarity.Unique: return "Unique";
            default: return "Unknown";
        }
    }
}
