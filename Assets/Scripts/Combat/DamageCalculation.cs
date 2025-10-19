using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class DamageModifiers
{
    [Header("Base Damage")]
    public float baseDamage = 0f;
    
    [Header("Added Damage")]
    public float addedPhysicalDamage = 0f;
    public float addedFireDamage = 0f;
    public float addedColdDamage = 0f;
    public float addedLightningDamage = 0f;
    public float addedChaosDamage = 0f;
    
    [Header("Increased Damage (Additive)")]
    public List<float> increasedPhysicalDamage = new List<float>();
    public List<float> increasedFireDamage = new List<float>();
    public List<float> increasedColdDamage = new List<float>();
    public List<float> increasedLightningDamage = new List<float>();
    public List<float> increasedChaosDamage = new List<float>();
    
    [Header("More Damage (Multiplicative)")]
    public List<float> morePhysicalDamage = new List<float>();
    public List<float> moreFireDamage = new List<float>();
    public List<float> moreColdDamage = new List<float>();
    public List<float> moreLightningDamage = new List<float>();
    public List<float> moreChaosDamage = new List<float>();
    
    [Header("Critical Strike")]
    public float criticalStrikeChance = 5f; // 5% base
    public float criticalStrikeMultiplier = 1.5f; // 150% base
    
    // Get total increased damage for a type
    public float GetTotalIncreasedDamage(DamageType damageType)
    {
        List<float> increasedList = GetIncreasedList(damageType);
        return increasedList.Sum();
    }
    
    // Get total more damage multiplier for a type
    public float GetTotalMoreDamageMultiplier(DamageType damageType)
    {
        List<float> moreList = GetMoreList(damageType);
        float multiplier = 1f;
        foreach (float more in moreList)
        {
            multiplier *= (1f + more);
        }
        return multiplier;
    }
    
    // Get added damage for a type
    public float GetAddedDamage(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Physical: return addedPhysicalDamage;
            case DamageType.Fire: return addedFireDamage;
            case DamageType.Cold: return addedColdDamage;
            case DamageType.Lightning: return addedLightningDamage;
            case DamageType.Chaos: return addedChaosDamage;
            default: return 0f;
        }
    }
    
    // Helper methods to get the correct lists
    private List<float> GetIncreasedList(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Physical: return increasedPhysicalDamage;
            case DamageType.Fire: return increasedFireDamage;
            case DamageType.Cold: return increasedColdDamage;
            case DamageType.Lightning: return increasedLightningDamage;
            case DamageType.Chaos: return increasedChaosDamage;
            default: return new List<float>();
        }
    }
    
    private List<float> GetMoreList(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Physical: return morePhysicalDamage;
            case DamageType.Fire: return moreFireDamage;
            case DamageType.Cold: return moreColdDamage;
            case DamageType.Lightning: return moreLightningDamage;
            case DamageType.Chaos: return moreChaosDamage;
            default: return new List<float>();
        }
    }
}

[System.Serializable]
public class DamageCalculation
{
    public DamageType damageType;
    public float baseDamage;
    public float addedDamage;
    public float increasedDamage;
    public float moreDamageMultiplier;
    public float finalDamage;
    public bool isCritical;
    public float criticalMultiplier;
    
    public DamageCalculation(DamageType type, float baseDmg, float added, float increased, float more, bool crit, float critMult)
    {
        damageType = type;
        baseDamage = baseDmg;
        addedDamage = added;
        increasedDamage = increased;
        moreDamageMultiplier = more;
        isCritical = crit;
        criticalMultiplier = critMult;
        
        // Calculate final damage using PoE formula
        finalDamage = CalculateFinalDamage();
    }
    
    private float CalculateFinalDamage()
    {
        // PoE Formula: (Base + Added) * (1 + Increased) * More * Crit
        float baseAndAdded = baseDamage + addedDamage;
        float withIncreased = baseAndAdded * (1f + increasedDamage);
        float withMore = withIncreased * moreDamageMultiplier;
        float final = isCritical ? withMore * criticalMultiplier : withMore;
        
        return final;
    }
    
    public string GetCalculationBreakdown()
    {
        string breakdown = $"{damageType} Damage Calculation:\n";
        breakdown += $"Base Damage: {baseDamage}\n";
        breakdown += $"Added Damage: {addedDamage}\n";
        breakdown += $"Base + Added: {baseDamage + addedDamage}\n";
        breakdown += $"Increased Damage: {increasedDamage:P0} ({increasedDamage:F2})\n";
        breakdown += $"After Increased: {(baseDamage + addedDamage) * (1f + increasedDamage):F1}\n";
        breakdown += $"More Damage Multiplier: {moreDamageMultiplier:F2}\n";
        breakdown += $"After More: {(baseDamage + addedDamage) * (1f + increasedDamage) * moreDamageMultiplier:F1}\n";
        
        if (isCritical)
        {
            breakdown += $"Critical Strike: x{criticalMultiplier:F2}\n";
            breakdown += $"Final Damage: {finalDamage:F1}";
        }
        else
        {
            breakdown += $"Final Damage: {finalDamage:F1}";
        }
        
        return breakdown;
    }
}

public class DamageCalculator : MonoBehaviour
{
    // Calculate damage for a single damage type
    public static DamageCalculation CalculateDamage(DamageType damageType, DamageModifiers modifiers)
    {
        // Get base damage (from skill/card)
        float baseDamage = modifiers.baseDamage;
        
        // Get added damage
        float addedDamage = modifiers.GetAddedDamage(damageType);
        
        // Get total increased damage (additive)
        float increasedDamage = modifiers.GetTotalIncreasedDamage(damageType);
        
        // Get total more damage multiplier (multiplicative)
        float moreDamageMultiplier = modifiers.GetTotalMoreDamageMultiplier(damageType);
        
        // Check for critical strike
        bool isCritical = Random.Range(0f, 100f) < modifiers.criticalStrikeChance;
        float criticalMultiplier = isCritical ? modifiers.criticalStrikeMultiplier : 1f;
        
        return new DamageCalculation(damageType, baseDamage, addedDamage, increasedDamage, moreDamageMultiplier, isCritical, criticalMultiplier);
    }
    
    // Calculate card damage with attribute scaling and weapon scaling
    public static float CalculateCardDamage(Card card, Character character, Weapon equippedWeapon = null)
    {
        float totalDamage = card.baseDamage;
        
        // Add attribute scaling
        float scalingBonus = card.damageScaling.CalculateScalingBonus(character);
        totalDamage += scalingBonus;
        
        // Add weapon scaling if applicable (uses total damage including affixes)
        if (card.scalesWithMeleeWeapon && equippedWeapon != null && equippedWeapon.weaponType == WeaponType.Melee)
        {
            float weaponDamage = equippedWeapon.GetWeaponDamage();
            totalDamage += weaponDamage;
        }
        else if (card.scalesWithProjectileWeapon && equippedWeapon != null && equippedWeapon.weaponType == WeaponType.Projectile)
        {
            float weaponDamage = equippedWeapon.GetWeaponDamage();
            totalDamage += weaponDamage;
        }
        else if (card.scalesWithSpellWeapon && equippedWeapon != null && equippedWeapon.weaponType == WeaponType.Spell)
        {
            float weaponDamage = equippedWeapon.GetWeaponDamage();
            totalDamage += weaponDamage;
        }
        
        // Apply character's damage modifiers
        float increasedMultiplier = (1f + character.increasedDamage);
        // If melee physical (identified by scalesWithMeleeWeapon), apply STR-based increased melee phys
        if (card.scalesWithMeleeWeapon)
        {
            increasedMultiplier *= (1f + Mathf.Max(0f, character.increasedMeleePhysicalDamage));
        }
        float moreMultiplier = Mathf.Max(1f, character.moreDamage); // Ensure minimum of 1 to avoid zero damage
        
        // Debug logging
        Debug.Log($"<color=cyan>CalculateCardDamage Debug for {card.cardName}:</color>");
        Debug.Log($"  Base Damage: {card.baseDamage}");
        Debug.Log($"  Scaling Bonus: {scalingBonus}");
        Debug.Log($"  Before Modifiers: {totalDamage}");
        Debug.Log($"  Increased Damage: {character.increasedDamage} (multiplier: {increasedMultiplier})");
        Debug.Log($"  More Damage: {character.moreDamage}");
        
        totalDamage *= increasedMultiplier;
        totalDamage *= moreMultiplier;
        
        Debug.Log($"  Final Damage: {totalDamage}");
        
        return totalDamage;
    }
    
    // Calculate guard amount with attribute scaling
    public static float CalculateGuardAmount(Card card, Character character)
    {
        float guardAmount = card.baseGuard;
        
        // Add attribute scaling for guard
        guardAmount += card.guardScaling.CalculateScalingBonus(character);
        
        return guardAmount;
    }
    
    // Calculate damage for multiple damage types
    public static List<DamageCalculation> CalculateAllDamage(DamageModifiers modifiers)
    {
        List<DamageCalculation> calculations = new List<DamageCalculation>();
        
        // Calculate for each damage type that has any damage
        DamageType[] damageTypes = { DamageType.Physical, DamageType.Fire, DamageType.Cold, DamageType.Lightning, DamageType.Chaos };
        
        foreach (DamageType damageType in damageTypes)
        {
            float addedDamage = modifiers.GetAddedDamage(damageType);
            float increasedDamage = modifiers.GetTotalIncreasedDamage(damageType);
            float moreDamage = modifiers.GetTotalMoreDamageMultiplier(damageType);
            
            // Only calculate if there's actual damage to deal
            if (modifiers.baseDamage > 0 || addedDamage > 0 || increasedDamage > 0 || moreDamage > 1f)
            {
                DamageCalculation calc = CalculateDamage(damageType, modifiers);
                calculations.Add(calc);
            }
        }
        
        return calculations;
    }
    
    // Get total damage from all calculations
    public static float GetTotalDamage(List<DamageCalculation> calculations)
    {
        return calculations.Sum(calc => calc.finalDamage);
    }
    
    // Apply damage to target with resistances
    public static float ApplyResistances(float damage, DamageType damageType, Character target)
    {
        float resistance = target.damageStats.GetResistance(damageType);
        
        if (damageType == DamageType.Physical)
        {
            // Physical resistance (armor) reduces damage by flat amount
            return Mathf.Max(0, damage - resistance);
        }
        else
        {
            // Elemental resistances reduce damage by percentage
            float reduction = 1f - (resistance / 100f);
            return damage * Mathf.Max(0.1f, reduction); // Minimum 10% damage
        }
    }
}
