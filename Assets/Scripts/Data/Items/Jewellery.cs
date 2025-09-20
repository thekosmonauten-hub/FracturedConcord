using UnityEngine;

[CreateAssetMenu(fileName = "New Jewellery", menuName = "Dexiled/Items/Jewellery")]
public class Jewellery : BaseItem
{
    [Header("Jewellery Base Properties")]
    public JewelleryType jewelleryType;
    public JewellerySlot jewellerySlot;
    
    [Header("Base Stats")]
    public float life = 0f;
    public float mana = 0f;
    public float energyShield = 0f;
    public float ward = 0f;
    
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
        
        // Add jewellery type and slot
        desc += $"\n\n{jewellerySlot} - {jewelleryType}";
        
        // Add base stats
        desc += "\n\nBase Stats:";
        if (life > 0) desc += $"\n+{life} to Maximum Life";
        if (mana > 0) desc += $"\n+{mana} to Maximum Mana";
        if (energyShield > 0) desc += $"\n+{energyShield} to Maximum Energy Shield";
        if (ward > 0) desc += $"\n+{ward} to Maximum Ward";
        
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
