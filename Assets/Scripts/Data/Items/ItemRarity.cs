using UnityEngine;
using System.Collections.Generic;

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
    public List<string> requiredTags = new List<string>(); // Item tags this affix can apply to
    public float weight = 100f; // Drop weight for random generation
    public Handedness handedness = Handedness.Both; // Handedness requirement
    
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
        
        // Roll a single value from the range
        if (hasRange && minValue > 0 && maxValue > 0)
        {
            // Use the seed to generate a random value within the range
            System.Random random = new System.Random(seed + name.GetHashCode());
            rolledAffix.rolledValue = random.Next(minValue, maxValue + 1); // +1 because Next is exclusive of max
            rolledAffix.isRolled = true;
        }
        else
        {
            rolledAffix.rolledValue = 0;
            rolledAffix.isRolled = false;
        }
        
        // Copy modifiers (simplified - no complex rolling needed)
        foreach (var modifier in modifiers)
        {
            rolledAffix.modifiers.Add(modifier);
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
    public float rolledFirstValue;
    public float rolledSecondValue;
    
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

public enum ModifierScope
{
    Local,
    Global
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
