using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CardRequirements
{
    [Header("Attribute Requirements")]
    public int requiredStrength = 0;
    public int requiredDexterity = 0;
    public int requiredIntelligence = 0;
    
    [Header("Weapon Requirements")]
    public List<WeaponType> requiredWeaponTypes = new List<WeaponType>();
    
    [Header("Level Requirements")]
    public int requiredLevel = 1;
    
    // Check if character meets all requirements
    public bool MeetsRequirements(Character character)
    {
        // Check attribute requirements
        if (character.strength < requiredStrength) return false;
        if (character.dexterity < requiredDexterity) return false;
        if (character.intelligence < requiredIntelligence) return false;
        
        // Check level requirement
        if (character.level < requiredLevel) return false;
        
        // Check weapon requirements (if any)
        if (requiredWeaponTypes.Count > 0)
        {
            bool hasRequiredWeapon = false;
            foreach (WeaponType weaponType in requiredWeaponTypes)
            {
                if (character.HasWeaponType(weaponType))
                {
                    hasRequiredWeapon = true;
                    break;
                }
            }
            if (!hasRequiredWeapon) return false;
        }
        
        return true;
    }
    
    // Get requirement description for UI
    public string GetRequirementDescription()
    {
        List<string> requirements = new List<string>();
        
        if (requiredStrength > 0) requirements.Add($"STR: {requiredStrength}");
        if (requiredDexterity > 0) requirements.Add($"DEX: {requiredDexterity}");
        if (requiredIntelligence > 0) requirements.Add($"INT: {requiredIntelligence}");
        if (requiredLevel > 1) requirements.Add($"Level: {requiredLevel}");
        if (requiredWeaponTypes.Count > 0)
        {
            string weaponReq = "Weapon: " + string.Join(" or ", requiredWeaponTypes);
            requirements.Add(weaponReq);
        }
        
        return string.Join(", ", requirements);
    }
    
    // Check if character meets requirements and return specific failure reason
    public (bool meets, string failureReason) CheckRequirements(Character character)
    {
        if (character.strength < requiredStrength)
            return (false, $"Requires {requiredStrength} Strength (has {character.strength})");
        
        if (character.dexterity < requiredDexterity)
            return (false, $"Requires {requiredDexterity} Dexterity (has {character.dexterity})");
        
        if (character.intelligence < requiredIntelligence)
            return (false, $"Requires {requiredIntelligence} Intelligence (has {character.intelligence})");
        
        if (character.level < requiredLevel)
            return (false, $"Requires Level {requiredLevel} (has {character.level})");
        
        if (requiredWeaponTypes.Count > 0)
        {
            bool hasRequiredWeapon = false;
            foreach (WeaponType weaponType in requiredWeaponTypes)
            {
                if (character.HasWeaponType(weaponType))
                {
                    hasRequiredWeapon = true;
                    break;
                }
            }
            if (!hasRequiredWeapon)
                return (false, $"Requires weapon type: {string.Join(" or ", requiredWeaponTypes)}");
        }
        
        return (true, "Requirements met");
    }
}
