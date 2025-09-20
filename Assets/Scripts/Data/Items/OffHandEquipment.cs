using UnityEngine;

[CreateAssetMenu(fileName = "New Off-Hand Equipment", menuName = "Dexiled/Items/Off-Hand Equipment")]
public class OffHandEquipment : BaseItem
{
    [Header("Off-Hand Base Properties")]
    public OffHandType offHandType;
    public OffHandSlot offHandSlot;
    
    [Header("Base Stats")]
    public float defence = 0f;
    public float blockChance = 0f;
    public float blockValue = 0f;
    public float attackSpeed = 1.0f;
    public float criticalStrikeChance = 5f;
    
    [Header("Requirements")]
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
        
        // Add off-hand type and slot
        desc += $"\n\n{offHandSlot} - {offHandType}";
        
        // Add base stats
        desc += "\n\nBase Stats:";
        if (defence > 0) desc += $"\nDefence: {defence}";
        if (blockChance > 0) desc += $"\nBlock Chance: {blockChance}%";
        if (blockValue > 0) desc += $"\nBlock Value: {blockValue}";
        if (attackSpeed != 1.0f) desc += $"\nAttack Speed: {attackSpeed}";
        if (criticalStrikeChance != 5f) desc += $"\nCritical Strike Chance: {criticalStrikeChance}%";
        
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
        
        // Use the unified affix system from BaseItem
        return base.GetFullDescription() + "\n" + desc;
    }
    
    public override bool CanBeEquippedBy(Character character)
    {
        return character.level >= requiredLevel &&
               character.strength >= requiredStrength &&
               character.dexterity >= requiredDexterity &&
               character.intelligence >= requiredIntelligence;
    }
}
