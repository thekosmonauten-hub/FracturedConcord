using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Compatibility extensions for BaseItem to work with legacy ItemData code
/// Provides extension methods so legacy code can access BaseItem properties
/// </summary>
public static class ItemDataCompatibility
{
    // Sprite compatibility
    public static Sprite GetItemSprite(this BaseItem item)
    {
        return item.itemIcon;
    }
    
    // Weapon-specific properties
    public static float GetTotalMinDamage(this BaseItem item)
    {
        if (item is WeaponItem weapon)
        {
            (float min, float max) = item.GetDualModifierValue("AddedPhysicalDamage");
            return weapon.minDamage + min;
        }
        return 0f;
    }
    
    public static float GetTotalMaxDamage(this BaseItem item)
    {
        if (item is WeaponItem weapon)
        {
            (float min, float max) = item.GetDualModifierValue("AddedPhysicalDamage");
            return weapon.maxDamage + max;
        }
        return 0f;
    }
    
    public static float GetCriticalStrikeChance(this BaseItem item)
    {
        if (item is WeaponItem weapon)
        {
            return weapon.criticalStrikeChance + item.GetModifierValue("IncreasedCriticalStrikeChance");
        }
        return 0f;
    }
    
    public static float GetAttackSpeed(this BaseItem item)
    {
        if (item is WeaponItem weapon)
        {
            return weapon.attackSpeed * (1 + item.GetModifierValue("IncreasedAttackSpeed") / 100f);
        }
        return 0f;
    }
    
    // Armor-specific properties
    public static float GetTotalArmour(this BaseItem item)
    {
        if (item is Armour armor)
        {
            return armor.armour + item.GetModifierValue("Armour");
        }
        return 0f;
    }
    
    public static float GetTotalEvasion(this BaseItem item)
    {
        if (item is Armour armor)
        {
            return armor.evasion + item.GetModifierValue("Evasion");
        }
        return 0f;
    }
    
    public static float GetTotalEnergyShield(this BaseItem item)
    {
        if (item is Armour armor)
        {
            return armor.energyShield + item.GetModifierValue("EnergyShield");
        }
        return 0f;
    }
    
    // Attribute requirements
    public static int GetRequiredStrength(this BaseItem item)
    {
        if (item is WeaponItem weapon) return weapon.requiredStrength;
        if (item is Armour armor) return armor.requiredStrength;
        if (item is Jewellery jewel) return jewel.requiredStrength;
        return 0;
    }
    
    public static int GetRequiredDexterity(this BaseItem item)
    {
        if (item is WeaponItem weapon) return weapon.requiredDexterity;
        if (item is Armour armor) return armor.requiredDexterity;
        if (item is Jewellery jewel) return jewel.requiredDexterity;
        return 0;
    }
    
    public static int GetRequiredIntelligence(this BaseItem item)
    {
        if (item is WeaponItem weapon) return weapon.requiredIntelligence;
        if (item is Armour armor) return armor.requiredIntelligence;
        if (item is Jewellery jewel) return jewel.requiredIntelligence;
        return 0;
    }
    
    // Modifier lists (for compatibility with legacy UI code)
    public static List<string> GetPrefixModifiers(this BaseItem item)
    {
        List<string> modifiers = new List<string>();
        foreach (Affix prefix in item.prefixes)
        {
            modifiers.Add(prefix.description);
        }
        return modifiers;
    }
    
    public static List<string> GetSuffixModifiers(this BaseItem item)
    {
        List<string> modifiers = new List<string>();
        foreach (Affix suffix in item.suffixes)
        {
            modifiers.Add(suffix.description);
        }
        return modifiers;
    }
    
    public static List<string> GetImplicitModifiers(this BaseItem item)
    {
        List<string> modifiers = new List<string>();
        foreach (Affix implicitMod in item.implicitModifiers)
        {
            modifiers.Add(implicitMod.description);
        }
        return modifiers;
    }
}

