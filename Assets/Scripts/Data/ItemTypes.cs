using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Core item type definitions used throughout the game
/// Extracted from legacy EquipmentScreen.cs for compatibility
/// </summary>

[System.Serializable]
public class EquipmentData
{
    public BaseItem helmet;
    public BaseItem amulet;
    public BaseItem mainHand;
    public BaseItem bodyArmour;
    public BaseItem offHand;
    public BaseItem gloves;
    public BaseItem leftRing;
    public BaseItem rightRing;
    public BaseItem belt;
    public BaseItem boots;
}

[System.Serializable]
public class ItemData
{
    public string itemName;
    public ItemType itemType;
    public EquipmentType equipmentType;
    public ItemRarity rarity;
    public int level;
    public Sprite itemSprite;
    public int requiredLevel;
    
    // Weapon-specific properties
    public float baseDamageMin;
    public float baseDamageMax;
    public float criticalStrikeChance;
    public float attackSpeed;
    
    // Armor-specific properties
    public float baseArmour;
    public float baseEvasion;
    public float baseEnergyShield;
    
    // Attribute requirements
    public int requiredStrength;
    public int requiredDexterity;
    public int requiredIntelligence;
    
    // Modifiers and stats
    public Dictionary<string, float> stats = new Dictionary<string, float>();
    public List<string> implicitModifiers = new List<string>();
    public List<string> prefixModifiers = new List<string>();
    public List<string> suffixModifiers = new List<string>();
    
    // Source item reference
    public BaseItem sourceItem;
    
    // Helper methods
    public float GetTotalMinDamage()
    {
        return baseDamageMin + (stats.ContainsKey("AddedPhysicalDamageMin") ? stats["AddedPhysicalDamageMin"] : 0);
    }
    
    public float GetTotalMaxDamage()
    {
        return baseDamageMax + (stats.ContainsKey("AddedPhysicalDamageMax") ? stats["AddedPhysicalDamageMax"] : 0);
    }
    
    public float GetTotalArmour()
    {
        return baseArmour + (stats.ContainsKey("Armour") ? stats["Armour"] : 0);
    }
    
    public float GetTotalEvasion()
    {
        return baseEvasion + (stats.ContainsKey("Evasion") ? stats["Evasion"] : 0);
    }
    
    public float GetTotalEnergyShield()
    {
        return baseEnergyShield + (stats.ContainsKey("EnergyShield") ? stats["EnergyShield"] : 0);
    }
    
    public (float min, float max) GetCalculatedTotalDamage()
    {
        float min = GetTotalMinDamage();
        float max = GetTotalMaxDamage();
        return (min, max);
    }
    
    // Conversion helper from BaseItem to ItemData
    public static ItemData FromBaseItem(BaseItem baseItem)
    {
        if (baseItem == null) return null;
        
        ItemData itemData = new ItemData
        {
            itemName = baseItem.GetDisplayName(),
            itemType = baseItem.itemType,
            equipmentType = baseItem.equipmentType,
            rarity = baseItem.GetCalculatedRarity(),
            level = baseItem.requiredLevel,
            itemSprite = baseItem.itemIcon,
            requiredLevel = baseItem.requiredLevel,
            sourceItem = baseItem
        };
        
        // Convert weapon-specific properties
        if (baseItem is WeaponItem weapon)
        {
            itemData.baseDamageMin = weapon.minDamage;
            itemData.baseDamageMax = weapon.maxDamage;
            itemData.criticalStrikeChance = weapon.criticalStrikeChance;
            itemData.attackSpeed = weapon.attackSpeed;
            itemData.requiredStrength = weapon.requiredStrength;
            itemData.requiredDexterity = weapon.requiredDexterity;
            itemData.requiredIntelligence = weapon.requiredIntelligence;
        }
        
        // Convert armor-specific properties
        if (baseItem is Armour armor)
        {
            itemData.baseArmour = armor.armour;
            itemData.baseEvasion = armor.evasion;
            itemData.baseEnergyShield = armor.energyShield;
            itemData.requiredStrength = armor.requiredStrength;
            itemData.requiredDexterity = armor.requiredDexterity;
            itemData.requiredIntelligence = armor.requiredIntelligence;
        }
        
        // Convert jewellery-specific properties
        if (baseItem is Jewellery jewellery)
        {
            itemData.requiredStrength = jewellery.requiredStrength;
            itemData.requiredDexterity = jewellery.requiredDexterity;
            itemData.requiredIntelligence = jewellery.requiredIntelligence;
        }
        
        return itemData;
    }
}

public enum EquipmentType
{
    Helmet,
    Amulet,
    MainHand,
    BodyArmour,
    OffHand,
    Gloves,
    LeftRing,
    RightRing,
    Belt,
    Boots
}

public enum ItemType
{
    Weapon,
    Armor,
    Armour,      // UK spelling for compatibility
    Accessory,
    Consumable,
    Currency,
    QuestItem,
    Effigy,
    Material     // Crafting materials
}

