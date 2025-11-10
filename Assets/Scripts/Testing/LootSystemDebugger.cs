using UnityEngine;
using System.Collections.Generic;

public class LootSystemDebugger : MonoBehaviour
{
    [Header("Debug Configuration")]
    [SerializeField] private int testAreaLevel = 30;
    
    [ContextMenu("Debug Loot System Setup")]
    public void DebugLootSystemSetup()
    {
        Debug.Log("=== LOOT SYSTEM SETUP DEBUGGING ===");
        
        // Check 1: AreaLootManager Instance
        if (AreaLootManager.Instance == null)
        {
            Debug.LogError("❌ AreaLootManager.Instance is NULL! Please add AreaLootManager to scene.");
            return;
        }
        Debug.Log("✅ AreaLootManager.Instance found");
        
        // Check 2: ItemDatabase
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("❌ ItemDatabase.Instance is NULL! Check if ItemDatabase exists in Resources folder.");
            return;
        }
        Debug.Log("✅ ItemDatabase.Instance found");
        
        // Check 3: Item counts in database
        List<BaseItem> allItems = ItemDatabase.Instance.GetAllItems();
        Debug.Log($"✅ ItemDatabase contains {allItems.Count} total items");
        
        // Check 4: Level range items
        var (minLevel, maxLevel) = AreaLootManager.Instance.GetValidLevelRangeForArea(testAreaLevel);
        Debug.Log($"✅ Area {testAreaLevel} level range: {minLevel}-{maxLevel}");
        
        // Check 5: Eligible items by type
        foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
        {
            List<BaseItem> eligible = AreaLootManager.Instance.GetEligibleItemsForArea(testAreaLevel, itemType);
            Debug.Log($"  {itemType}: {eligible.Count} eligible items");
            
            // Show first few items for verification
            for (int i = 0; i < Mathf.Min(3, eligible.Count); i++)
            {
                Debug.Log($"    - {eligible[i].itemName} (Level {eligible[i].requiredLevel})");
            }
        }
        
        // Check 6: AreaLootTable setup
        AreaLootTable lootTable = AreaLootManager.Instance.GetLootTableForArea(testAreaLevel);
        if (lootTable == null)
        {
            Debug.LogError("❌ No AreaLootTable found for area level " + testAreaLevel);
            Debug.LogError("   Please assign a defaultLootTable in AreaLootManager or create area-specific tables");
            return;
        }
        Debug.Log($"✅ AreaLootTable found: {lootTable.name}");
        Debug.Log($"   Base Drop Chance: {lootTable.baseDropChance * 100:F1}%");
        
        // Check 7: Test basic item generation
        Debug.Log("\n--- TESTING BASIC ITEM GENERATION ---");
        for (int i = 0; i < 10; i++)
        {
            BaseItem testItem = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel);
            if (testItem != null)
            {
                Debug.Log($"✅ Generated: {testItem.GetDisplayName()} (Level {testItem.requiredLevel})");
            }
            else
            {
                Debug.Log($"❌ Generation attempt {i + 1}: null result");
            }
        }
        
        // Check 8: AffixDatabase
        if (AffixDatabase_Modern.Instance == null)
        {
            Debug.LogWarning("⚠️ AffixDatabase_Modern.Instance is NULL - affixes won't be applied");
        }
        else
        {
            Debug.Log("✅ AffixDatabase_Modern.Instance found");
        }
    }
    
    [ContextMenu("Test Direct Loot Table Generation")]
    public void TestDirectLootTableGeneration()
    {
        Debug.Log("=== DIRECT LOOT TABLE TESTING ===");
        
        AreaLootTable lootTable = AreaLootManager.Instance.GetLootTableForArea(testAreaLevel);
        if (lootTable == null)
        {
            Debug.LogError("❌ No loot table found");
            return;
        }
        
        Debug.Log($"Testing direct generation from: {lootTable.name}");
        Debug.Log($"Base drop chance: {lootTable.baseDropChance * 100:F1}%");
        
        // Test direct table generation
        for (int i = 0; i < 20; i++) // More attempts since there's RNG involved
        {
            BaseItem item = lootTable.GenerateRandomItem();
            if (item != null)
            {
                Debug.Log($"✅ Direct generation: {item.GetDisplayName()} (Level {item.requiredLevel}, {item.GetCalculatedRarity()})");
            }
            else
            {
                Debug.Log($"❌ Direct generation attempt {i + 1}: null (failed drop chance or no eligible items)");
            }
        }
    }
    
    [ContextMenu("Create Test Loot Table")]
    public void CreateTestLootTable()
    {
        Debug.Log("=== CREATING TEST LOOT TABLE ===");
        
        // This will guide the user through creating a proper loot table
        Debug.Log("To create a test loot table:");
        Debug.Log("1. Right-click in Project → Create → Dexiled → Loot → Area Loot Table");
        Debug.Log("2. Name it 'TestAreaLootTable'");
        Debug.Log("3. Set Area Level: 30");
        Debug.Log("4. Set Base Drop Chance: 1.0 (100% for testing)");
        Debug.Log("5. Assign it to AreaLootManager.defaultLootTable");
        Debug.Log("6. Run this debugger again");
    }
}
