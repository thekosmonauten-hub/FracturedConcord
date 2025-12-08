using UnityEngine;
using Dexiled.Data.Items;
using System.Collections.Generic; // Added missing import for Dictionary

public class TooltipDamageTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestOnStart = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            TestTooltipDamageCalculation();
        }
    }
    
    [ContextMenu("Test Tooltip Damage Calculation")]
    public void TestTooltipDamageCalculation()
    {
        Debug.Log("=== Testing Tooltip Damage Calculation ===");
        
        // Create a test ItemData with affixes
        ItemData testItem = new ItemData();
        testItem.itemName = "Test Rusted Sword";
        testItem.itemType = ItemType.Weapon;
        testItem.equipmentType = EquipmentType.MainHand;
        testItem.rarity = ItemRarity.Normal;
        testItem.baseDamageMin = 8f;
        testItem.baseDamageMax = 8f;
        testItem.criticalStrikeChance = 5f;
        testItem.attackSpeed = 1.0f;
        
        // Add affix stats to the stats dictionary
        testItem.stats = new Dictionary<string, float>();
        testItem.stats["PhysicalDamage"] = 6f; // +6 Physical damage
        testItem.stats["IncreasedPhysicalDamage"] = 40f; // 40% increased Physical damage
        
        // Test the calculation
        float totalMinDamage = testItem.GetTotalMinDamage();
        float totalMaxDamage = testItem.GetTotalMaxDamage();
        (float min, float max) = testItem.GetCalculatedTotalDamage();
        float averageDamage = (min + max) / 2f;
        
        // Expected: (8 + 6) * 1.4 = 19.6 → 20 (rounded up)
        float expectedDamage = Mathf.Ceil((8f + 6f) * 1.4f);
        
        Debug.Log($"Original base damage: 8");
        Debug.Log($"Added Physical Damage: 6");
        Debug.Log($"Increased Physical Damage: 40%");
        Debug.Log($"Expected damage: {expectedDamage}");
        Debug.Log($"Calculated min damage: {totalMinDamage}");
        Debug.Log($"Calculated max damage: {totalMaxDamage}");
        Debug.Log($"Calculated average damage: {averageDamage}");
        Debug.Log($"Calculation correct: {Mathf.Approximately(expectedDamage, averageDamage)}");
        
        Debug.Log("=== Test Complete ===");
    }
    
    [ContextMenu("Test ItemData Creation")]
    public void TestItemDataCreation()
    {
        Debug.Log("=== Testing ItemData Creation from WeaponItem ===");
        
        // Get the Rusted Sword from the database
        WeaponItem rustedSword = ItemDatabase.Instance.weapons.Find(w => w.itemName == "Rusted Sword");
        if (rustedSword == null)
        {
            Debug.LogError("Rusted Sword not found in database!");
            return;
        }
        
        // Create ItemData from WeaponItem
        ItemData itemData = new ItemData();
        itemData.itemName = rustedSword.GetDisplayName();
        itemData.itemType = ItemType.Weapon;
        itemData.equipmentType = EquipmentType.MainHand;
        itemData.rarity = rustedSword.rarity;
        itemData.baseDamageMin = rustedSword.minDamage;
        itemData.baseDamageMax = rustedSword.maxDamage;
        itemData.criticalStrikeChance = rustedSword.criticalStrikeChance;
        itemData.attackSpeed = rustedSword.attackSpeed;
        itemData.requiredLevel = rustedSword.requiredLevel;
        itemData.requiredStrength = rustedSword.requiredStrength;
        itemData.requiredDexterity = rustedSword.requiredDexterity;
        itemData.requiredIntelligence = rustedSword.requiredIntelligence;
        
        // Convert affixes to stats
        Dictionary<string, float> stats = new Dictionary<string, float>();
        
        // Process all affix lists
        ProcessAffixList(rustedSword.implicitModifiers, stats);
        ProcessAffixList(rustedSword.prefixes, stats);
        ProcessAffixList(rustedSword.suffixes, stats);
        
        itemData.stats = stats;
        
        // Test the calculation
        float totalMinDamage = itemData.GetTotalMinDamage();
        float totalMaxDamage = itemData.GetTotalMaxDamage();
        (float min, float max) = itemData.GetCalculatedTotalDamage();
        float averageDamage = (min + max) / 2f;
        
        Debug.Log($"Weapon: {itemData.itemName}");
        Debug.Log($"Original base damage: {itemData.baseDamageMin}-{itemData.baseDamageMax}");
        Debug.Log($"Calculated total damage: {totalMinDamage}-{totalMaxDamage}");
        Debug.Log($"Calculated average damage: {averageDamage}");
        
        // Show all stats
        Debug.Log("Stats dictionary contents:");
        foreach (var stat in itemData.stats)
        {
            Debug.Log($"  {stat.Key}: {stat.Value}");
        }
        
        Debug.Log("=== Test Complete ===");
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


