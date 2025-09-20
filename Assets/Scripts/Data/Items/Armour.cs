using UnityEngine;

[CreateAssetMenu(fileName = "New Armour", menuName = "Dexiled/Items/Armour")]
public class Armour : BaseItem
{
    [Header("Armour Base Properties")]
    public ArmourSlot armourSlot;
    public ArmourType armourType;
    
    [Header("Defence Values")]
    public float armour = 0f;
    public float evasion = 0f;
    public float energyShield = 0f;
    public float ward = 0f;
    
    [Header("Requirements")]
    public int requiredStrength = 0;
    public int requiredDexterity = 0;
    public int requiredIntelligence = 0;
    
    [Header("Movement Penalties")]
    public float movementSpeedPenalty = 0f; // Percentage reduction
    public float attackSpeedPenalty = 0f; // Percentage reduction
    
    public override string GetDisplayName()
    {
        string qualityPrefix = quality > 0 ? $"Superior " : "";
        string rarityPrefix = rarity != ItemRarity.Normal ? $"[{rarity}] " : "";
        return rarityPrefix + qualityPrefix + itemName;
    }
    
    public override string GetFullDescription()
    {
        string desc = description;
        
        // Add armour type and slot
        desc += $"\n\n{armourSlot} - {armourType}";
        
        // Add defence values
        desc += "\n\nDefence:";
        if (armour > 0) desc += $"\nArmour: {armour}";
        if (evasion > 0) desc += $"\nEvasion: {evasion}";
        if (energyShield > 0) desc += $"\nEnergy Shield: {energyShield}";
        if (ward > 0) desc += $"\nWard: {ward}";
        
        // Add requirements
        if (requiredStrength > 0 || requiredDexterity > 0 || requiredIntelligence > 0)
        {
            desc += "\n\nRequirements:";
            if (requiredStrength > 0) desc += $"\n{requiredStrength} Strength";
            if (requiredDexterity > 0) desc += $"\n{requiredDexterity} Dexterity";
            if (requiredIntelligence > 0) desc += $"\n{requiredIntelligence} Intelligence";
        }
        
        // Add penalties
        if (movementSpeedPenalty > 0 || attackSpeedPenalty > 0)
        {
            desc += "\n\nPenalties:";
            if (movementSpeedPenalty > 0) desc += $"\n{movementSpeedPenalty}% reduced Movement Speed";
            if (attackSpeedPenalty > 0) desc += $"\n{attackSpeedPenalty}% reduced Attack Speed";
        }
        
        // Add quality bonus
        if (quality > 0)
        {
            desc += $"\n\nQuality: +{quality}% to all defence values";
        }
        
        // Use the unified affix system from BaseItem
        return base.GetFullDescription() + "\n" + desc;
    }
    
    public float GetTotalDefence()
    {
        float totalDefence = armour + evasion + energyShield + ward;
        
        // Apply quality bonus
        if (quality > 0)
        {
            totalDefence += totalDefence * (quality * 0.05f); // 5% per quality
        }
        
        return totalDefence;
    }
    
    public override bool CanBeEquippedBy(Character character)
    {
        return character.level >= requiredLevel &&
               character.strength >= requiredStrength &&
               character.dexterity >= requiredDexterity &&
               character.intelligence >= requiredIntelligence;
    }
}
