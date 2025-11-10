using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates affixes for Effigies based on their element, size tier, and rarity
/// </summary>
public static class EffigyAffixGenerator
{
    /// <summary>
    /// Generate affixes for an Effigy based on its properties
    /// </summary>
    public static List<Affix> GenerateAffixes(Effigy effigy, AffixDatabase affixDatabase)
    {
        if (effigy == null || affixDatabase == null)
        {
            Debug.LogWarning("[EffigyAffixGenerator] Cannot generate affixes - effigy or database is null");
            return new List<Affix>();
        }
        
        List<Affix> generatedAffixes = new List<Affix>();
        
        // Unique effigies have fixed affixes (no generation)
        if (effigy.rarity == ItemRarity.Unique)
        {
            return effigy.modifiers; // Return existing fixed affixes
        }
        
        // Get available affix pools for this element
        var affixPool = GetAffixPoolForElement(effigy.element, effigy.sizeTier);
        
        // Determine how many affixes to generate based on rarity
        int affixCount = GetAffixCountForRarity(effigy.rarity, effigy.sizeTier);
        
        // Generate prefixes
        int prefixCount = Mathf.Min(affixCount / 2 + (affixCount % 2), affixPool.prefixes.Count);
        for (int i = 0; i < prefixCount; i++)
        {
            var prefix = GetRandomAffixFromList(affixPool.prefixes);
            if (prefix != null)
            {
                // Create a rolled copy of the affix
                Affix rolledAffix = prefix.GenerateRolledAffix();
                generatedAffixes.Add(rolledAffix);
            }
        }
        
        // Generate suffixes
        int suffixCount = affixCount - prefixCount;
        suffixCount = Mathf.Min(suffixCount, affixPool.suffixes.Count);
        for (int i = 0; i < suffixCount; i++)
        {
            var suffix = GetRandomAffixFromList(affixPool.suffixes);
            if (suffix != null)
            {
                // Create a rolled copy of the affix
                Affix rolledAffix = suffix.GenerateRolledAffix();
                generatedAffixes.Add(rolledAffix);
            }
        }
        
        return generatedAffixes;
    }
    
    /// <summary>
    /// Get the number of affixes to generate based on rarity and size
    /// </summary>
    private static int GetAffixCountForRarity(ItemRarity rarity, EffigySizeTier sizeTier)
    {
        int baseCount = 0;
        
        switch (rarity)
        {
            case ItemRarity.Normal:
                baseCount = 1;
                break;
            case ItemRarity.Magic:
                baseCount = Random.Range(1, 3); // 1-2
                break;
            case ItemRarity.Rare:
                baseCount = Random.Range(2, 4); // 2-3
                break;
        }
        
        // Size tier bonus
        if (sizeTier == EffigySizeTier.Large && rarity >= ItemRarity.Magic)
        {
            baseCount += 1; // Large effigies can have one extra affix
        }
        
        return baseCount;
    }
    
    /// <summary>
    /// Affix pool structure for an element
    /// </summary>
    private class ElementAffixPool
    {
        public List<Affix> prefixes = new List<Affix>();
        public List<Affix> suffixes = new List<Affix>();
    }
    
    /// <summary>
    /// Get affix pools for a specific element (simplified - should query AffixDatabase)
    /// </summary>
    private static ElementAffixPool GetAffixPoolForElement(EffigyElement element, EffigySizeTier sizeTier)
    {
        ElementAffixPool pool = new ElementAffixPool();
        
        // TODO: This should query the AffixDatabase for element-specific affixes
        // For now, return placeholder affixes based on element theme
        
        switch (element)
        {
            case EffigyElement.Fire:
                pool.prefixes.Add(CreateFirePrefix());
                pool.suffixes.Add(CreateFireSuffix());
                break;
            case EffigyElement.Cold:
                pool.prefixes.Add(CreateColdPrefix());
                pool.suffixes.Add(CreateColdSuffix());
                break;
            case EffigyElement.Lightning:
                pool.prefixes.Add(CreateLightningPrefix());
                pool.suffixes.Add(CreateLightningSuffix());
                break;
            case EffigyElement.Physical:
                pool.prefixes.Add(CreatePhysicalPrefix());
                pool.suffixes.Add(CreatePhysicalSuffix());
                break;
            case EffigyElement.Chaos:
                pool.prefixes.Add(CreateChaosPrefix());
                pool.suffixes.Add(CreateChaosSuffix());
                break;
        }
        
        return pool;
    }
    
    private static Affix GetRandomAffixFromList(List<Affix> affixes)
    {
        if (affixes == null || affixes.Count == 0)
            return null;
        
        return affixes[Random.Range(0, affixes.Count)];
    }
    
    // Placeholder affix creators - these should query the actual AffixDatabase
    private static Affix CreateFirePrefix()
    {
        return new Affix("Burning", "+15-24% increased Fire Damage", AffixType.Prefix, AffixTier.Tier5)
        {
            minValue = 15,
            maxValue = 24,
            hasRange = true
        };
    }
    
    private static Affix CreateFireSuffix()
    {
        return new Affix("of Ignition", "5-10% chance to Ignite on hit", AffixType.Suffix, AffixTier.Tier5)
        {
            minValue = 5,
            maxValue = 10,
            hasRange = true
        };
    }
    
    private static Affix CreateColdPrefix()
    {
        return new Affix("Frozen", "+20-30% increased Energy Shield", AffixType.Prefix, AffixTier.Tier5)
        {
            minValue = 20,
            maxValue = 30,
            hasRange = true
        };
    }
    
    private static Affix CreateColdSuffix()
    {
        return new Affix("of Freezing", "+20-30% increased Freeze Duration", AffixType.Suffix, AffixTier.Tier5)
        {
            minValue = 20,
            maxValue = 30,
            hasRange = true
        };
    }
    
    private static Affix CreateLightningPrefix()
    {
        return new Affix("Crackling", "+10-15% increased Critical Strike Chance", AffixType.Prefix, AffixTier.Tier5)
        {
            minValue = 10,
            maxValue = 15,
            hasRange = true
        };
    }
    
    private static Affix CreateLightningSuffix()
    {
        return new Affix("of Shocking", "+20-30% increased Shock Effect", AffixType.Suffix, AffixTier.Tier5)
        {
            minValue = 20,
            maxValue = 30,
            hasRange = true
        };
    }
    
    private static Affix CreatePhysicalPrefix()
    {
        return new Affix("Stalwart", "+15-25% increased Armor", AffixType.Prefix, AffixTier.Tier5)
        {
            minValue = 15,
            maxValue = 25,
            hasRange = true
        };
    }
    
    private static Affix CreatePhysicalSuffix()
    {
        return new Affix("of Endurance", "+10-20% increased maximum Life", AffixType.Suffix, AffixTier.Tier5)
        {
            minValue = 10,
            maxValue = 20,
            hasRange = true
        };
    }
    
    private static Affix CreateChaosPrefix()
    {
        return new Affix("Corrupting", "+12-20% increased Chaos Damage", AffixType.Prefix, AffixTier.Tier5)
        {
            minValue = 12,
            maxValue = 20,
            hasRange = true
        };
    }
    
    private static Affix CreateChaosSuffix()
    {
        return new Affix("of Decay", "+15-25% increased Damage over Time Multiplier", AffixType.Suffix, AffixTier.Tier5)
        {
            minValue = 15,
            maxValue = 25,
            hasRange = true
        };
    }
}






