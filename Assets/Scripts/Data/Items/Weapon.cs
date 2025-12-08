using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Dexiled/Items/Weapons/Weapon")]
public class WeaponItem : BaseItem
{
    [Header("Weapon Base Properties")]
    public WeaponItemType weaponType;
    public WeaponHandedness handedness = WeaponHandedness.OneHanded;
    public float attackSpeed = 1.0f; // Attacks per second
    public float criticalStrikeChance = 5.0f; // Base crit chance
    public float criticalStrikeMultiplier = 150.0f; // Base crit multiplier
    
    [Header("Damage")]
    public float minDamage = 10f;
    public float maxDamage = 15f;
    [Tooltip("Rolled base damage value (set when item is generated)")]
    public float rolledBaseDamage = 0f; // Rolled once when item drops
    public DamageType primaryDamageType = DamageType.Physical;
    public DamageType secondaryDamageType = DamageType.None;
    public float secondaryDamageMin = 0f;
    public float secondaryDamageMax = 0f;
    
    [Header("Weapon Requirements")]
    public int requiredStrength = 0;
    public int requiredDexterity = 0;
    public int requiredIntelligence = 0;
    
    public override string GetDisplayName()
    {
        string qualityPrefix = quality > 0 ? $"Superior " : "";
        string rarityPrefix = rarity != ItemRarity.Normal ? $"[{rarity}] " : "";
        return rarityPrefix + qualityPrefix + itemName;
    }
    
    public override string GetFullDescription()
    {
        string desc = description;
        
        // Add weapon type and handedness
        desc += $"\n\n{handedness} {weaponType}";
        
        // Add calculated damage information (shows final damage after affixes)
        int totalMinDamage = Mathf.CeilToInt(GetTotalMinDamage());
        int totalMaxDamage = Mathf.CeilToInt(GetTotalMaxDamage());
        int averageDamage = Mathf.CeilToInt((totalMinDamage + totalMaxDamage) / 2f);
        desc += $"\nBase damage: {averageDamage} {primaryDamageType}";
        if (secondaryDamageType != DamageType.None && secondaryDamageMax > 0)
        {
            desc += $"\nSecondary: {secondaryDamageMin}-{secondaryDamageMax} {secondaryDamageType}";
        }
        
        // Add attack speed
        desc += $"\nAttack Speed: {attackSpeed:F2} attacks per second";
        
        // Add critical strike info
        desc += $"\nCritical Strike: {criticalStrikeChance:F1}% chance, {criticalStrikeMultiplier:F0}% multiplier";
        
        // Add requirements
        if (requiredStrength > 0 || requiredDexterity > 0 || requiredIntelligence > 0)
        {
            desc += "\n\nRequirements:";
            if (requiredStrength > 0) desc += $"\n{requiredStrength} Strength";
            if (requiredDexterity > 0) desc += $"\n{requiredDexterity} Dexterity";
            if (requiredIntelligence > 0) desc += $"\n{requiredIntelligence} Intelligence";
        }
        
        // Add quality bonus
        if (quality > 0)
        {
            desc += $"\n\nQuality: +{quality}% to all stats";
        }
        
        // Add implicit modifiers (fixed, always present)
        if (implicitModifiers.Count > 0)
        {
            desc += "\n\nImplicit Modifiers:";
            foreach (var modifier in implicitModifiers)
            {
                desc += $"\n{modifier.description}";
            }
        }
        
        // Add explicit modifiers (random prefixes and suffixes)
        if (prefixes.Count > 0 || suffixes.Count > 0)
        {
            desc += "\n\nAffixes:";
            
            // Show prefixes
            if (prefixes.Count > 0)
            {
                foreach (var prefix in prefixes)
                {
                    desc += $"\n{prefix.description}";
                }
            }
            
            // Show suffixes
            if (suffixes.Count > 0)
            {
                foreach (var suffix in suffixes)
                {
                    desc += $"\n{suffix.description}";
                }
            }
        }
        
        return desc;
    }
    
    public float GetAverageDamage()
    {
        return (minDamage + maxDamage) / 2f;
    }
    
    // Get original base damage (before affixes are applied)
    public float GetOriginalBaseDamage()
    {
        return (minDamage + maxDamage) / 2f;
    }
    
    // Get calculated total damage (after affixes are applied)
    public float GetCalculatedTotalDamage()
    {
        return (GetTotalMinDamage() + GetTotalMaxDamage()) / 2f;
    }
    
    public float GetDPS()
    {
        return GetCalculatedTotalDamage() * attackSpeed;
    }
    
    public new bool CanBeEquippedBy(Character character)
    {
        // Weapon-specific equipment logic
        return character.level >= requiredLevel;
    }
    
    /// <summary>
    /// Get total damage with rolled values.
    /// Now uses rolledBaseDamage and rolled affix values!
    /// </summary>
    public float GetTotalMinDamage()
    {
        // Use rolled base damage if available, otherwise use minDamage
        float totalDamage = rolledBaseDamage > 0f ? rolledBaseDamage : minDamage;
        
        // Add ROLLED damage from affixes (all affixes now roll to single values)
        totalDamage += GetModifierValue("addedPhysicalDamage");
        totalDamage += GetModifierValue("addedFireDamage");
        totalDamage += GetModifierValue("addedColdDamage");
        totalDamage += GetModifierValue("addedLightningDamage");
        totalDamage += GetModifierValue("addedChaosDamage");
        
        // Apply LOCAL increased damage multipliers (% on the weapon itself)
        float increasedDamage = GetModifierValue("increasedPhysicalDamage") + 
                               GetModifierValue("increasedFireDamage") + 
                               GetModifierValue("increasedColdDamage") + 
                               GetModifierValue("increasedLightningDamage") + 
                               GetModifierValue("increasedChaosDamage");
        totalDamage *= (1f + increasedDamage / 100f);
        
        return Mathf.Ceil(totalDamage);
    }
    
    public float GetTotalMaxDamage()
    {
        // For rolled affixes, min and max are the same (single value)
        // This method exists for backward compatibility
        return GetTotalMinDamage();
    }
    
    public float GetTotalAttackSpeed()
    {
        float baseSpeed = attackSpeed;
        float speedModifier = GetModifierValue("AttackSpeed");
        return baseSpeed * (1f + speedModifier / 100f); // Convert percentage to multiplier
    }
    
    public float GetTotalCriticalStrikeChance()
    {
        float baseChance = criticalStrikeChance;
        float chanceModifier = GetModifierValue("CriticalStrikeChance");
        return baseChance + chanceModifier;
    }
}

// Remove the old WeaponModifier class - now using unified Affix system
// [System.Serializable]
// public class WeaponModifier
// {
//     public string description;
//     public WeaponModifierType modifierType;
//     public float value;
//     public DamageType damageType = DamageType.None;
//     public StatType statType = StatType.Flat;
// }

public enum WeaponItemType
{
    Axe,
    Bow,
    Claw,
    Dagger,
    RitualDagger,
    Mace,
    Sceptre,
    Staff,
    Sword,
    Wand
}

public enum WeaponHandedness
{
    OneHanded,
    TwoHanded
}

// Keep WeaponModifierType for reference, but it's now handled by AffixModifier
public enum WeaponModifierType
{
    IncreasedDamage,
    MoreDamage,
    AddedDamage,
    AttackSpeed,
    CriticalStrikeChance,
    CriticalStrikeMultiplier,
    Accuracy,
    Range,
    Other
}
