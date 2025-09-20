using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AttackCardDamage
{
    public Card card;
    public Character character;
    
    public AttackCardDamage(Card attackCard, Character character)
    {
        this.card = attackCard;
        this.character = character;
    }
    
    // Calculate total damage for attack card
    public DamageCalculation CalculateAttackDamage()
    {
        if (card.cardType != CardType.Attack)
        {
            Debug.LogError($"Cannot calculate attack damage for non-attack card: {card.cardName}");
            return null;
        }
        
        // Get base damage from card
        float baseDamage = card.baseDamage;
        
        // Get weapon scaling damage
        float weaponDamage = card.GetWeaponScalingDamage(character);
        
        // Total base damage (card + weapon)
        float totalBaseDamage = baseDamage + weaponDamage;
        
        // Get character damage modifiers
        DamageModifiers characterModifiers = character.GetDamageModifiers();
        
        // Create combined modifiers for calculation
        DamageModifiers combinedModifiers = new DamageModifiers();
        combinedModifiers.baseDamage = totalBaseDamage;
        
        // Add character modifiers
        combinedModifiers.addedPhysicalDamage = characterModifiers.addedPhysicalDamage;
        combinedModifiers.addedFireDamage = characterModifiers.addedFireDamage;
        combinedModifiers.addedColdDamage = characterModifiers.addedColdDamage;
        combinedModifiers.addedLightningDamage = characterModifiers.addedLightningDamage;
        combinedModifiers.addedChaosDamage = characterModifiers.addedChaosDamage;
        
        // Add increased damage modifiers
        combinedModifiers.increasedPhysicalDamage.AddRange(characterModifiers.increasedPhysicalDamage);
        combinedModifiers.increasedFireDamage.AddRange(characterModifiers.increasedFireDamage);
        combinedModifiers.increasedColdDamage.AddRange(characterModifiers.increasedColdDamage);
        combinedModifiers.increasedLightningDamage.AddRange(characterModifiers.increasedLightningDamage);
        combinedModifiers.increasedChaosDamage.AddRange(characterModifiers.increasedChaosDamage);
        
        // Add more damage modifiers
        combinedModifiers.morePhysicalDamage.AddRange(characterModifiers.morePhysicalDamage);
        combinedModifiers.moreFireDamage.AddRange(characterModifiers.moreFireDamage);
        combinedModifiers.moreColdDamage.AddRange(characterModifiers.moreColdDamage);
        combinedModifiers.moreLightningDamage.AddRange(characterModifiers.moreLightningDamage);
        combinedModifiers.moreChaosDamage.AddRange(characterModifiers.moreChaosDamage);
        
        // Critical strike stats
        combinedModifiers.criticalStrikeChance = characterModifiers.criticalStrikeChance;
        combinedModifiers.criticalStrikeMultiplier = characterModifiers.criticalStrikeMultiplier;
        
        // Calculate damage for primary damage type
        return DamageCalculator.CalculateDamage(card.primaryDamageType, combinedModifiers);
    }
    
    // Get detailed damage breakdown
    public string GetDamageBreakdown()
    {
        var damageCalc = CalculateAttackDamage();
        if (damageCalc == null) return "Invalid attack card";
        
        string breakdown = $"Attack Card: {card.cardName}\n";
        breakdown += $"Base Card Damage: {card.baseDamage}\n";
        breakdown += $"Weapon Scaling: +{card.GetWeaponScalingDamage(character):F0}\n";
        breakdown += $"Total Base Damage: {card.baseDamage + card.GetWeaponScalingDamage(character):F0}\n\n";
        breakdown += damageCalc.GetCalculationBreakdown();
        
        return breakdown;
    }
    
    // Get damage preview for UI
    public string GetDamagePreview()
    {
        var damageCalc = CalculateAttackDamage();
        if (damageCalc == null) return "Invalid";
        
        string preview = $"{damageCalc.finalDamage:F0} {card.primaryDamageType}";
        if (damageCalc.isCritical)
            preview += " (CRIT!)";
        
        return preview;
    }
}
