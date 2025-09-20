using UnityEngine;
using System.Collections.Generic;

public enum CardType
{
    Attack,
    Guard,
    Skill,
    Power,
    Aura
}

[System.Serializable]
public class AttributeScaling
{
    public float strengthScaling = 0f;    // How much damage scales with Strength
    public float dexterityScaling = 0f;   // How much damage scales with Dexterity  
    public float intelligenceScaling = 0f; // How much damage scales with Intelligence
    
    public float CalculateScalingBonus(Character character)
    {
        float bonus = 0f;
        bonus += character.strength * strengthScaling;
        bonus += character.dexterity * dexterityScaling;
        bonus += character.intelligence * intelligenceScaling;
        return bonus;
    }
}

[System.Serializable]
public class Card
{
    [Header("Card Properties")]
    public string cardName;
    public string description; // Static description template
    public CardType cardType;
    public int manaCost;
    public float baseDamage;
    public float baseGuard;
    public DamageType primaryDamageType = DamageType.Physical;
    
    [Header("Weapon Scaling")]
    public bool scalesWithMeleeWeapon = false;
    public bool scalesWithProjectileWeapon = false;
    public bool scalesWithSpellWeapon = false;
    
    [Header("Area of Effect")]
    public bool isAoE = false; // Area of Effect - hits multiple enemies
    public int aoeTargets = 3; // Number of enemies hit (default to all 3)
    
    [Header("Requirements")]
    public CardRequirements requirements = new CardRequirements();
    
    [Header("Card Tags")]
    public List<string> tags = new List<string>();
    
    [Header("Additional Properties")]
    public List<DamageType> additionalDamageTypes = new List<DamageType>();
    
    [Header("Attribute Scaling")]
    public AttributeScaling damageScaling = new AttributeScaling();
    public AttributeScaling guardScaling = new AttributeScaling(); // For Guard cards
    
    [Header("Card Effects")]
    public List<CardEffect> effects = new List<CardEffect>();
    
    // Check if character can use this card
    public bool CanUseCard(Character character)
    {
        // Check requirements
        if (!requirements.MeetsRequirements(character)) return false;
        
        // Check mana cost
        if (manaCost > 0 && character.mana < manaCost) return false;
        
        return true;
    }
    
    // Get detailed requirement check
    public (bool canUse, string reason) CheckCardUsage(Character character)
    {
        var (meetsReqs, reqReason) = requirements.CheckRequirements(character);
        if (!meetsReqs)
            return (false, reqReason);
        
        if (manaCost > 0 && character.mana < manaCost)
            return (false, $"Requires {manaCost} Mana (has {character.mana})");
        
        return (true, "Can use card");
    }
    
    // Get weapon scaling damage for attack cards
    public float GetWeaponScalingDamage(Character character)
    {
        if (cardType != CardType.Attack) return 0f;
        
        float weaponDamage = 0f;
        
        if (scalesWithMeleeWeapon)
            weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Melee);
        
        if (scalesWithProjectileWeapon)
            weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Projectile);
        
        if (scalesWithSpellWeapon)
            weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Spell);
        
        return weaponDamage;
    }
    
    // Get dynamic description with actual calculated values
    public string GetDynamicDescription(Character character)
    {
        if (string.IsNullOrEmpty(description))
            return GetCardTooltip();
        
        string dynamicDesc = description;
        
        // Replace damage placeholders with actual calculated values
        if (cardType == CardType.Attack && baseDamage > 0)
        {
            float totalDamage = DamageCalculator.CalculateCardDamage(this, character, character.weapons.meleeWeapon);
            dynamicDesc = dynamicDesc.Replace("{damage}", totalDamage.ToString("F0"));
            dynamicDesc = dynamicDesc.Replace("{baseDamage}", baseDamage.ToString("F0"));
        }
        
        // Replace guard placeholders with actual calculated values
        if (cardType == CardType.Guard && baseGuard > 0)
        {
            float totalGuard = DamageCalculator.CalculateGuardAmount(this, character);
            dynamicDesc = dynamicDesc.Replace("{guard}", totalGuard.ToString("F0"));
            dynamicDesc = dynamicDesc.Replace("{baseGuard}", baseGuard.ToString("F0"));
        }
        
        // Replace attribute scaling placeholders
        if (damageScaling.strengthScaling > 0)
        {
            float strBonus = character.strength * damageScaling.strengthScaling;
            dynamicDesc = dynamicDesc.Replace("{strBonus}", strBonus.ToString("F0"));
        }
        
        if (damageScaling.dexterityScaling > 0)
        {
            float dexBonus = character.dexterity * damageScaling.dexterityScaling;
            dynamicDesc = dynamicDesc.Replace("{dexBonus}", dexBonus.ToString("F0"));
        }
        
        if (damageScaling.intelligenceScaling > 0)
        {
            float intBonus = character.intelligence * damageScaling.intelligenceScaling;
            dynamicDesc = dynamicDesc.Replace("{intBonus}", intBonus.ToString("F0"));
        }
        
        // Replace weapon damage placeholders
        if (scalesWithMeleeWeapon)
        {
            float weaponDamage = character.weapons.GetWeaponDamage(WeaponType.Melee);
            dynamicDesc = dynamicDesc.Replace("{weaponDamage}", weaponDamage.ToString("F0"));
        }
        
        // Replace AoE target count
        if (isAoE)
        {
            dynamicDesc = dynamicDesc.Replace("{aoeTargets}", aoeTargets.ToString());
        }
        
        return dynamicDesc;
    }
    
    // Get card tooltip with requirements and scaling info
    public string GetCardTooltip()
    {
        string tooltip = $"{cardName}\n";
        tooltip += $"Type: {cardType}\n";
        tooltip += $"Cost: {manaCost} Mana\n";
        
        if (baseDamage > 0)
        {
            tooltip += $"Damage: {baseDamage}";
            if (damageScaling.strengthScaling > 0)
                tooltip += $" + {damageScaling.strengthScaling * 100}% STR";
            if (damageScaling.dexterityScaling > 0)
                tooltip += $" + {damageScaling.dexterityScaling * 100}% DEX";
            if (damageScaling.intelligenceScaling > 0)
                tooltip += $" + {damageScaling.intelligenceScaling * 100}% INT";
            tooltip += "\n";
        }
        
        if (baseGuard > 0)
        {
            tooltip += $"Guard: {baseGuard}";
            if (guardScaling.strengthScaling > 0)
                tooltip += $" + {guardScaling.strengthScaling * 100}% STR";
            if (guardScaling.dexterityScaling > 0)
                tooltip += $" + {guardScaling.dexterityScaling * 100}% DEX";
            if (guardScaling.intelligenceScaling > 0)
                tooltip += $" + {guardScaling.intelligenceScaling * 100}% INT";
            tooltip += "\n";
        }
        
        if (isAoE)
        {
            tooltip += $"AoE: Hits {aoeTargets} enemies\n";
        }
        
        if (scalesWithMeleeWeapon)
            tooltip += "Scales with Melee Weapon\n";
        if (scalesWithProjectileWeapon)
            tooltip += "Scales with Projectile Weapon\n";
        if (scalesWithSpellWeapon)
            tooltip += "Scales with Spell Weapon\n";
        
        return tooltip;
    }
}
