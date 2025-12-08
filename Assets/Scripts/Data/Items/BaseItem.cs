using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Item", menuName = "Dexiled/Items/Base Item")]
public abstract class BaseItem : ScriptableObject
{
    [Header("Basic Information")]
    public string itemName = "New Item";
    [TextArea(3, 5)]
    public string description = "Item description";
    public Sprite itemIcon;
    public ItemRarity rarity = ItemRarity.Normal;
    public int requiredLevel = 1;
    
    [Tooltip("Item level determines which affix tiers can roll. Separate from requiredLevel (character requirement).")]
    public int itemLevel = 1;
    
    [Header("Item Properties")]
    public ItemType itemType;
    public EquipmentType equipmentType;
    public bool isStackable = false;
    public int maxStackSize = 1;
    
    [Header("Quality")]
    [Range(0, 20)]
    public int quality = 0; // 0 = normal, 1-20 = superior
    
    [Header("Affix System")]
    public List<Affix> implicitModifiers = new List<Affix>(); // Fixed modifiers (always present)
    public List<Affix> prefixes = new List<Affix>(); // Random prefixes (from AffixDatabase)
    public List<Affix> suffixes = new List<Affix>(); // Random suffixes (from AffixDatabase)
    public bool isUnique = false; // If true, affixes are fixed and non-random
    
    [Header("Item Tags")]
    public List<string> itemTags = new List<string>();
    
    [Header("Generated Name (Runtime)")]
    [Tooltip("Auto-generated name for Magic/Rare items. Set at runtime.")]
    public string generatedName = "";
    
    // Virtual methods for derived classes
    public virtual string GetDisplayName()
    {
        // Use generated name if available (Magic/Rare items)
        if (!string.IsNullOrEmpty(generatedName))
        {
            return generatedName;
        }
        
        // Fallback to legacy format for items without generated names
        string qualityPrefix = quality > 0 ? $"Superior " : "";
        string rarityName = GetRarityName();
        string rarityPrefix = rarity != ItemRarity.Normal ? $"{rarityName} " : "";
        return rarityPrefix + qualityPrefix + itemName;
    }
    
    public virtual string GetRarityName()
    {
        return ItemRarityCalculator.GetRarityName(GetCalculatedRarity());
    }
    
    public virtual ItemRarity GetCalculatedRarity()
    {
        return ItemRarityCalculator.CalculateRarity(prefixes.Count, suffixes.Count, isUnique);
    }
    
    public virtual string GetFullDescription()
    {
        string desc = description;
        
        // Add rarity information
        ItemRarity calculatedRarity = GetCalculatedRarity();
        desc += $"\n\n{ItemRarityCalculator.GetRarityName(calculatedRarity)} Item";
        
        // Add implicit modifiers (always present)
        if (implicitModifiers.Count > 0)
        {
            desc += "\n\nImplicit Modifiers:";
            foreach (var modifier in implicitModifiers)
            {
                desc += $"\n  {modifier.name}: {modifier.description}";
            }
        }
        
        // Add random affixes (prefixes and suffixes)
        if (prefixes.Count > 0 || suffixes.Count > 0)
        {
            desc += "\n\nExplicit Modifiers:";
            
            // Show prefixes
            if (prefixes.Count > 0)
            {
                desc += "\nPrefixes:";
                foreach (var prefix in prefixes)
                {
                    desc += $"\n  {prefix.name}: {prefix.description}";
                }
            }
            
            // Show suffixes
            if (suffixes.Count > 0)
            {
                desc += "\nSuffixes:";
                foreach (var suffix in suffixes)
                {
                    desc += $"\n  {suffix.name}: {suffix.description}";
                }
            }
        }
        
        // Add quality bonus description
        if (quality > 0)
        {
            desc += $"\n\nQuality: +{quality}% to all stats";
        }
        
        return desc;
    }
    
    // Affix management methods
    public virtual void ClearAffixes()
    {
        prefixes.Clear();
        suffixes.Clear();
    }
    
    public virtual bool CanAddPrefix()
    {
        return prefixes.Count < 3;
    }
    
    public virtual bool CanAddSuffix()
    {
        return suffixes.Count < 3;
    }
    
    public bool AddPrefix(Affix affix)
    {
        if (!CanAddPrefix())
            return false;
            
        // Generate a rolled version of the affix with actual values
        Affix rolledAffix = affix.GenerateRolledAffix();
        prefixes.Add(rolledAffix);
        return true;
    }
    
    public bool AddSuffix(Affix affix)
    {
        if (!CanAddSuffix())
            return false;
            
        // Generate a rolled version of the affix with actual values
        Affix rolledAffix = affix.GenerateRolledAffix();
        suffixes.Add(rolledAffix);
        return true;
    }
    
    public virtual int GetTotalAffixCount()
    {
        return implicitModifiers.Count + prefixes.Count + suffixes.Count;
    }
    
    public virtual float GetModifierValue(string statName)
    {
        float totalValue = 0f;
        
        // Check implicit modifiers
        foreach (var affix in implicitModifiers)
        {
            foreach (var modifier in affix.modifiers)
            {
                if (modifier.statName == statName)
                {
                    // Use the actual rolled value (minValue and maxValue should be the same after rolling)
                    totalValue += modifier.minValue;
                }
            }
        }
        
        // Check prefixes
        foreach (var affix in prefixes)
        {
            foreach (var modifier in affix.modifiers)
            {
                if (modifier.statName == statName)
                {
                    // Use the actual rolled value (minValue and maxValue should be the same after rolling)
                    totalValue += modifier.minValue;
                }
            }
        }
        
        // Check suffixes
        foreach (var affix in suffixes)
        {
            foreach (var modifier in affix.modifiers)
            {
                if (modifier.statName == statName)
                {
                    // Use the actual rolled value (minValue and maxValue should be the same after rolling)
                    totalValue += modifier.minValue;
                }
            }
        }
        
        return totalValue;
    }
    
    /// <summary>
    /// Gets dual-range modifier values for damage calculations.
    /// Returns (min, max) where min is added to weapon's minDamage and max to maxDamage.
    /// For dual-range affixes, uses rolledFirstValue and rolledSecondValue.
    /// For normal affixes, returns the same value for both.
    /// </summary>
    public virtual (float min, float max) GetDualModifierValue(string statName)
    {
        float totalMin = 0f;
        float totalMax = 0f;
        
        // Check implicit modifiers
        foreach (var affix in implicitModifiers)
        {
            foreach (var modifier in affix.modifiers)
            {
                if (modifier.statName == statName)
                {
                    if (modifier.isDualRange)
                    {
                        totalMin += modifier.rolledFirstValue;
                        totalMax += modifier.rolledSecondValue;
                    }
                    else
                    {
                        totalMin += modifier.minValue;
                        totalMax += modifier.minValue; // Same for both
                    }
                }
            }
        }
        
        // Check prefixes
        foreach (var affix in prefixes)
        {
            foreach (var modifier in affix.modifiers)
            {
                if (modifier.statName == statName)
                {
                    if (modifier.isDualRange)
                    {
                        totalMin += modifier.rolledFirstValue;
                        totalMax += modifier.rolledSecondValue;
                    }
                    else
                    {
                        totalMin += modifier.minValue;
                        totalMax += modifier.minValue; // Same for both
                    }
                }
            }
        }
        
        // Check suffixes
        foreach (var affix in suffixes)
        {
            foreach (var modifier in affix.modifiers)
            {
                if (modifier.statName == statName)
                {
                    if (modifier.isDualRange)
                    {
                        totalMin += modifier.rolledFirstValue;
                        totalMax += modifier.rolledSecondValue;
                    }
                    else
                    {
                        totalMin += modifier.minValue;
                        totalMax += modifier.minValue; // Same for both
                    }
                }
            }
        }
        
        return (totalMin, totalMax);
    }
    
    // Virtual method for character compatibility
    public virtual bool CanBeEquippedBy(Character character)
    {
        return character.level >= requiredLevel;
    }
}

// Remove the old ItemStat class - no longer needed
// [System.Serializable]
// public class ItemStat
// {
//     public string statName;
//     public float baseValue;
//     public bool scalesWithQuality = true;
//     public StatType statType = StatType.Flat;
// }

public enum StatType
{
    Flat,
    Percentage,
    Multiplier
}
