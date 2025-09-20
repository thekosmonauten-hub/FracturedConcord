using UnityEngine;
using Dexiled.Data.Items;
using System.Collections.Generic;

public class ArmorDefenseTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestOnStart = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            TestArmorDefenseCalculation();
        }
    }
    
    [ContextMenu("Test Armor Defense Calculation")]
    public void TestArmorDefenseCalculation()
    {
        Debug.Log("=== Testing Armor Defense Calculation ===");
        
        // Create a test armor ItemData with affixes
        ItemData testArmor = new ItemData();
        testArmor.itemName = "Test Plate Vest";
        testArmor.itemType = ItemType.Armour;
        testArmor.equipmentType = EquipmentType.BodyArmour;
        testArmor.rarity = ItemRarity.Magic;
        testArmor.baseArmour = 24f;
        testArmor.baseEvasion = 0f;
        testArmor.baseEnergyShield = 0f;
        
        // Add affix stats to the stats dictionary
        testArmor.stats = new Dictionary<string, float>();
        testArmor.stats["Armour"] = 12f; // +12 Armour
        testArmor.stats["IncreasedArmour"] = 25f; // 25% increased Armour
        
        // Test the calculation
        float totalArmour = testArmor.GetTotalArmour();
        
        // Expected: (24 + 12) * 1.25 = 45 (rounded up)
        float expectedArmour = Mathf.Ceil((24f + 12f) * 1.25f);
        
        Debug.Log($"Original base armour: 24");
        Debug.Log($"Added Armour: 12");
        Debug.Log($"Increased Armour: 25%");
        Debug.Log($"Expected armour: {expectedArmour}");
        Debug.Log($"Calculated armour: {totalArmour}");
        Debug.Log($"Calculation correct: {Mathf.Approximately(expectedArmour, totalArmour)}");
        
        Debug.Log("=== Test Complete ===");
    }
    
    [ContextMenu("Test Armor ItemData Creation")]
    public void TestArmorItemDataCreation()
    {
        Debug.Log("=== Testing Armor ItemData Creation from Armour ===");
        
        // Get a random armor from the database
        Armour randomArmor = ItemDatabase.Instance.GetRandomArmour();
        if (randomArmor == null)
        {
            Debug.LogError("No armor found in database!");
            return;
        }
        
        // Create ItemData from Armour
        ItemData itemData = new ItemData();
        itemData.itemName = randomArmor.GetDisplayName();
        itemData.itemType = ItemType.Armour;
        itemData.equipmentType = ConvertArmourSlotToEquipmentType(randomArmor.armourSlot);
        itemData.rarity = randomArmor.rarity;
        itemData.baseArmour = randomArmor.armour;
        itemData.baseEvasion = randomArmor.evasion;
        itemData.baseEnergyShield = randomArmor.energyShield;
        itemData.requiredLevel = randomArmor.requiredLevel;
        itemData.requiredStrength = randomArmor.requiredStrength;
        itemData.requiredDexterity = randomArmor.requiredDexterity;
        itemData.requiredIntelligence = randomArmor.requiredIntelligence;
        
        // Convert affixes to stats
        Dictionary<string, float> stats = new Dictionary<string, float>();
        
        // Process all affix lists
        ProcessAffixList(randomArmor.implicitModifiers, stats);
        ProcessAffixList(randomArmor.prefixes, stats);
        ProcessAffixList(randomArmor.suffixes, stats);
        
        itemData.stats = stats;
        
        // Test the calculation
        float totalArmour = itemData.GetTotalArmour();
        float totalEvasion = itemData.GetTotalEvasion();
        float totalEnergyShield = itemData.GetTotalEnergyShield();
        
        Debug.Log($"Armor: {itemData.itemName}");
        Debug.Log($"Original base defence: Armour={itemData.baseArmour}, Evasion={itemData.baseEvasion}, ES={itemData.baseEnergyShield}");
        Debug.Log($"Calculated total defence: Armour={totalArmour}, Evasion={totalEvasion}, ES={totalEnergyShield}");
        
        // Show all stats
        Debug.Log("Stats dictionary contents:");
        foreach (var stat in itemData.stats)
        {
            Debug.Log($"  {stat.Key}: {stat.Value}");
        }
        
        Debug.Log("=== Test Complete ===");
    }
    
    // Helper method to convert armour slot to equipment type
    private EquipmentType ConvertArmourSlotToEquipmentType(ArmourSlot armourSlot)
    {
        switch (armourSlot)
        {
            case ArmourSlot.Helmet:
                return EquipmentType.Helmet;
            case ArmourSlot.BodyArmour:
                return EquipmentType.BodyArmour;
            case ArmourSlot.Gloves:
                return EquipmentType.Gloves;
            case ArmourSlot.Boots:
                return EquipmentType.Boots;
            case ArmourSlot.Shield:
                return EquipmentType.OffHand;
            default:
                return EquipmentType.BodyArmour;
        }
    }
    
    // Helper method to process affix list (copied from EquipmentScreen)
    private void ProcessAffixList(List<Affix> affixes, Dictionary<string, float> stats)
    {
        foreach (var affix in affixes)
        {
            foreach (var modifier in affix.modifiers)
            {
                string statName = modifier.statName;
                
                // Handle damage type modifiers
                if (modifier.damageType != DamageType.None)
                {
                    if (modifier.modifierType == ModifierType.Flat)
                    {
                        // For flat damage, use the damage type as the stat name
                        statName = $"{modifier.damageType}Damage";
                    }
                    else if (modifier.modifierType == ModifierType.Increased)
                    {
                        // For increased damage, use "Increased" + damage type
                        statName = $"Increased{modifier.damageType}Damage";
                    }
                }
                
                // Add or accumulate the value
                float value = modifier.minValue; // Use minValue as the rolled value
                if (stats.ContainsKey(statName))
                {
                    stats[statName] += value;
                }
                else
                {
                    stats[statName] = value;
                }
            }
        }
    }
}
