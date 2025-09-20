using UnityEngine;
using System.Collections.Generic;

public enum WeaponType
{
    None,
    Melee,
    Projectile,
    Spell
}

[System.Serializable]
public class WeaponAffix
{
    public string affixName;
    public float addedPhysicalDamage = 0f;
    public float addedFireDamage = 0f;
    public float addedColdDamage = 0f;
    public float addedLightningDamage = 0f;
    public float addedChaosDamage = 0f;
    public float increasedPhysicalDamage = 0f;
    public float increasedFireDamage = 0f;
    public float increasedColdDamage = 0f;
    public float increasedLightningDamage = 0f;
    public float increasedChaosDamage = 0f;
}

[System.Serializable]
public class Weapon
{
    [Header("Basic Information")]
    public string weaponName;
    public WeaponType weaponType;
    
    [Header("Implicit Base Damage")]
    public float baseDamageMin = 0f;
    public float baseDamageMax = 0f;
    public DamageType baseDamageType = DamageType.Physical;
    
    [Header("Affixes")]
    public List<WeaponAffix> affixes = new List<WeaponAffix>();
    
    [Header("Calculated Total Damage")]
    public float totalDamageMin = 0f;
    public float totalDamageMax = 0f;
    
    // Calculate total damage including affixes
    public void CalculateTotalDamage()
    {
        // Start with base damage
        totalDamageMin = baseDamageMin;
        totalDamageMax = baseDamageMax;
        
        // Add flat damage from affixes
        foreach (WeaponAffix affix in affixes)
        {
            switch (baseDamageType)
            {
                case DamageType.Physical:
                    totalDamageMin += affix.addedPhysicalDamage;
                    totalDamageMax += affix.addedPhysicalDamage;
                    break;
                case DamageType.Fire:
                    totalDamageMin += affix.addedFireDamage;
                    totalDamageMax += affix.addedFireDamage;
                    break;
                case DamageType.Cold:
                    totalDamageMin += affix.addedColdDamage;
                    totalDamageMax += affix.addedColdDamage;
                    break;
                case DamageType.Lightning:
                    totalDamageMin += affix.addedLightningDamage;
                    totalDamageMax += affix.addedLightningDamage;
                    break;
                case DamageType.Chaos:
                    totalDamageMin += affix.addedChaosDamage;
                    totalDamageMax += affix.addedChaosDamage;
                    break;
            }
        }
        
        // Apply increased damage multipliers
        float increasedMultiplier = 1f;
        foreach (WeaponAffix affix in affixes)
        {
            switch (baseDamageType)
            {
                case DamageType.Physical:
                    increasedMultiplier += affix.increasedPhysicalDamage / 100f;
                    break;
                case DamageType.Fire:
                    increasedMultiplier += affix.increasedFireDamage / 100f;
                    break;
                case DamageType.Cold:
                    increasedMultiplier += affix.increasedColdDamage / 100f;
                    break;
                case DamageType.Lightning:
                    increasedMultiplier += affix.increasedLightningDamage / 100f;
                    break;
                case DamageType.Chaos:
                    increasedMultiplier += affix.increasedChaosDamage / 100f;
                    break;
            }
        }
        
        totalDamageMin *= increasedMultiplier;
        totalDamageMax *= increasedMultiplier;
        
        // Round to nearest whole number
        totalDamageMin = Mathf.Round(totalDamageMin);
        totalDamageMax = Mathf.Round(totalDamageMax);
    }
    
    // Get average total damage (for card scaling)
    public float GetWeaponDamage()
    {
        return (totalDamageMin + totalDamageMax) / 2f;
    }
    
    // Get base damage (implicit only)
    public float GetBaseDamage()
    {
        return (baseDamageMin + baseDamageMax) / 2f;
    }
    
    // Get damage range as string
    public string GetDamageRangeString()
    {
        return $"{totalDamageMin}-{totalDamageMax} {baseDamageType}";
    }
    
    // Get base damage range as string
    public string GetBaseDamageRangeString()
    {
        return $"{baseDamageMin}-{baseDamageMax} {baseDamageType}";
    }
    
    // Add an affix to the weapon
    public void AddAffix(WeaponAffix affix)
    {
        affixes.Add(affix);
        CalculateTotalDamage();
    }
    
    // Remove an affix from the weapon
    public void RemoveAffix(WeaponAffix affix)
    {
        affixes.Remove(affix);
        CalculateTotalDamage();
    }
    
    // Get weapon description with damage breakdown
    public string GetWeaponDescription()
    {
        string desc = $"{weaponName}\n";
        desc += $"Base Damage: {GetBaseDamageRangeString()}\n";
        desc += $"Total Damage: {GetDamageRangeString()}\n";
        
        if (affixes.Count > 0)
        {
            desc += "Affixes:\n";
            foreach (WeaponAffix affix in affixes)
            {
                desc += $"- {affix.affixName}\n";
            }
        }
        
        return desc;
    }
}

[System.Serializable]
public class CharacterWeapons
{
    [Header("Equipped Weapons")]
    public Weapon meleeWeapon;
    public Weapon projectileWeapon;
    public Weapon spellWeapon;
    
    // Get weapon damage for a specific type (uses total damage including affixes)
    public float GetWeaponDamage(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Melee:
                return meleeWeapon?.GetWeaponDamage() ?? 0f;
            case WeaponType.Projectile:
                return projectileWeapon?.GetWeaponDamage() ?? 0f;
            case WeaponType.Spell:
                return spellWeapon?.GetWeaponDamage() ?? 0f;
            default:
                return 0f;
        }
    }
    
    // Get base weapon damage for a specific type (implicit only)
    public float GetBaseWeaponDamage(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Melee:
                return meleeWeapon?.GetBaseDamage() ?? 0f;
            case WeaponType.Projectile:
                return projectileWeapon?.GetBaseDamage() ?? 0f;
            case WeaponType.Spell:
                return spellWeapon?.GetBaseDamage() ?? 0f;
            default:
                return 0f;
        }
    }
    
    // Check if character has a specific weapon type
    public bool HasWeaponType(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Melee:
                return meleeWeapon != null;
            case WeaponType.Projectile:
                return projectileWeapon != null;
            case WeaponType.Spell:
                return spellWeapon != null;
            default:
                return false;
        }
    }
    
    // Get total weapon damage for all types (uses total damage including affixes)
    public float GetTotalWeaponDamage()
    {
        float total = 0f;
        if (meleeWeapon != null) total += meleeWeapon.GetWeaponDamage();
        if (projectileWeapon != null) total += projectileWeapon.GetWeaponDamage();
        if (spellWeapon != null) total += spellWeapon.GetWeaponDamage();
        return total;
    }
    
    // Get total base weapon damage for all types (implicit only)
    public float GetTotalBaseWeaponDamage()
    {
        float total = 0f;
        if (meleeWeapon != null) total += meleeWeapon.GetBaseDamage();
        if (projectileWeapon != null) total += projectileWeapon.GetBaseDamage();
        if (spellWeapon != null) total += spellWeapon.GetBaseDamage();
        return total;
    }
    
    // Helper method to create a Steel Axe example weapon
    public static Weapon CreateSteelAxe()
    {
        Weapon steelAxe = new Weapon
        {
            weaponName = "Steel Axe",
            weaponType = WeaponType.Melee,
            baseDamageMin = 5f,
            baseDamageMax = 9f,
            baseDamageType = DamageType.Physical
        };
        
        // Add affix: "Added 5-8 Physical damage"
        WeaponAffix addedDamageAffix = new WeaponAffix
        {
            affixName = "Added 5-8 Physical damage",
            addedPhysicalDamage = 6.5f // Average of 5-8
        };
        
        // Add affix: "21% increased Physical damage"
        WeaponAffix increasedDamageAffix = new WeaponAffix
        {
            affixName = "21% increased Physical damage",
            increasedPhysicalDamage = 21f
        };
        
        steelAxe.AddAffix(addedDamageAffix);
        steelAxe.AddAffix(increasedDamageAffix);
        
        return steelAxe;
    }
    
    // Helper method to create a basic weapon without affixes
    public static Weapon CreateBasicWeapon(string name, WeaponType type, float minDamage, float maxDamage, DamageType damageType)
    {
        Weapon weapon = new Weapon
        {
            weaponName = name,
            weaponType = type,
            baseDamageMin = minDamage,
            baseDamageMax = maxDamage,
            baseDamageType = damageType
        };
        
        weapon.CalculateTotalDamage();
        return weapon;
    }
}
