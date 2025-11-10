using UnityEngine;
using System.Linq;

/// <summary>
/// Diagnostic tool to identify why item generation is failing.
/// Run this FIRST before testing item generation!
/// </summary>
public class LootSystemDiagnostics : MonoBehaviour
{
    [ContextMenu("Run Full Diagnostics")]
    public void RunFullDiagnostics()
    {
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
        Debug.Log("<color=cyan><b>LOOT SYSTEM DIAGNOSTICS - FULL REPORT</b></color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>\n");
        
        bool allChecksPass = true;
        
        // Check 1: AreaLootManager
        allChecksPass &= CheckAreaLootManager();
        
        // Check 2: ItemDatabase
        allChecksPass &= CheckItemDatabase();
        
        // Check 3: AffixDatabase
        allChecksPass &= CheckAffixDatabase();
        
        // Check 4: Loot Table Configuration
        allChecksPass &= CheckLootTableConfiguration();
        
        // Final verdict
        Debug.Log("\n<color=cyan>═══════════════════════════════════════════════════════</color>");
        if (allChecksPass)
        {
            Debug.Log("<color=green><b>✅ ALL CHECKS PASSED - SYSTEM READY!</b></color>");
            Debug.Log("<color=green>You can now generate items successfully!</color>");
        }
        else
        {
            Debug.Log("<color=red><b>❌ SYSTEM NOT READY - PLEASE FIX ISSUES ABOVE</b></color>");
        }
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
    }
    
    private bool CheckAreaLootManager()
    {
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>CHECK 1: AreaLootManager</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        
        if (AreaLootManager.Instance == null)
        {
            Debug.LogError("❌ <b>AreaLootManager.Instance is NULL!</b>");
            Debug.LogError("   <b>FIX:</b> Add an AreaLootManager to your scene:");
            Debug.LogError("   1. Hierarchy → Create Empty → Name: 'AreaLootManager'");
            Debug.LogError("   2. Add Component → AreaLootManager");
            return false;
        }
        
        Debug.Log("✅ AreaLootManager.Instance found");
        
        if (AreaLootManager.Instance.defaultLootTable == null)
        {
            Debug.LogError("❌ <b>defaultLootTable is NOT ASSIGNED!</b>");
            Debug.LogError("   <b>FIX:</b> Create and assign a loot table:");
            Debug.LogError("   1. Right-click in Project → Create → Dexiled → Loot → Area Loot Table");
            Debug.LogError("   2. Name it: 'DefaultAreaLootTable'");
            Debug.LogError("   3. Select AreaLootManager in Hierarchy");
            Debug.LogError("   4. Drag 'DefaultAreaLootTable' to 'Default Loot Table' field in Inspector");
            return false;
        }
        
        Debug.Log($"✅ defaultLootTable assigned: <color=cyan>{AreaLootManager.Instance.defaultLootTable.name}</color>");
        return true;
    }
    
    private bool CheckItemDatabase()
    {
        Debug.Log("\n<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>CHECK 2: ItemDatabase</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("❌ <b>ItemDatabase.Instance is NULL!</b>");
            Debug.LogError("   <b>FIX:</b> ItemDatabase should exist at: Assets/Resources/ItemDatabase.asset");
            Debug.LogError("   If missing, create it:");
            Debug.LogError("   1. Right-click in Assets/Resources/");
            Debug.LogError("   2. Create → Dexiled → Items → Item Database");
            Debug.LogError("   3. Name it: 'ItemDatabase'");
            return false;
        }
        
        Debug.Log("✅ ItemDatabase.Instance found");
        
        var allItems = ItemDatabase.Instance.GetAllItems();
        
        if (allItems.Count == 0)
        {
            Debug.LogError("❌ <b>ItemDatabase is EMPTY (0 items)!</b>");
            Debug.LogError("   <b>FIX:</b> Import weapons and armor:");
            Debug.LogError("   1. Unity Menu → Dexiled → Import Weapons from CSV");
            Debug.LogError("   2. Unity Menu → Dexiled → Import Armor from CSV");
            return false;
        }
        
        Debug.Log($"✅ ItemDatabase contains <color=cyan><b>{allItems.Count}</b></color> items");
        
        // Count by type
        int weaponCount = allItems.Count(i => i.itemType == ItemType.Weapon);
        int armorCount = allItems.Count(i => i.itemType == ItemType.Armour);
        
        Debug.Log($"   - Weapons: <color=cyan>{weaponCount}</color>");
        Debug.Log($"   - Armor: <color=cyan>{armorCount}</color>");
        
        if (weaponCount == 0 && armorCount == 0)
        {
            Debug.LogWarning("⚠️ <b>No weapons or armor found!</b>");
            Debug.LogWarning("   Import some items before testing.");
            return false;
        }
        
        return true;
    }
    
    private bool CheckAffixDatabase()
    {
        Debug.Log("\n<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>CHECK 3: AffixDatabase</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        
        if (AffixDatabase_Modern.Instance == null)
        {
            Debug.LogError("❌ <b>AffixDatabase_Modern.Instance is NULL!</b>");
            Debug.LogError("   <b>FIX:</b> AffixDatabase should exist at: Assets/Resources/AffixDatabase.asset");
            Debug.LogError("   If missing, create it:");
            Debug.LogError("   1. Right-click in Assets/Resources/");
            Debug.LogError("   2. Create → Dexiled → Items → Affix Database");
            Debug.LogError("   3. Name it: 'AffixDatabase'");
            return false;
        }
        
        Debug.Log("✅ AffixDatabase_Modern.Instance found");
        
        // Count affixes in all categories
        int totalAffixes = 0;
        
        foreach (var category in AffixDatabase_Modern.Instance.weaponPrefixCategories)
        {
            totalAffixes += category.GetAllAffixes().Count;
        }
        foreach (var category in AffixDatabase_Modern.Instance.weaponSuffixCategories)
        {
            totalAffixes += category.GetAllAffixes().Count;
        }
        foreach (var category in AffixDatabase_Modern.Instance.armourPrefixCategories)
        {
            totalAffixes += category.GetAllAffixes().Count;
        }
        foreach (var category in AffixDatabase_Modern.Instance.armourSuffixCategories)
        {
            totalAffixes += category.GetAllAffixes().Count;
        }
        foreach (var category in AffixDatabase_Modern.Instance.jewelleryPrefixCategories)
        {
            totalAffixes += category.GetAllAffixes().Count;
        }
        foreach (var category in AffixDatabase_Modern.Instance.jewellerySuffixCategories)
        {
            totalAffixes += category.GetAllAffixes().Count;
        }
        
        if (totalAffixes == 0)
        {
            Debug.LogError("❌ <b>AffixDatabase is EMPTY (0 affixes)!</b>");
            Debug.LogError("   <b>FIX:</b> Import affixes from CSV:");
            Debug.LogError("   1. Unity Menu → Dexiled → Import Affixes from CSV");
            Debug.LogError("   2. Select: Comprehensive_Mods.csv");
            Debug.LogError("   3. Select: AffixDatabase.asset");
            Debug.LogError("   4. Click 'Import Affixes'");
            return false;
        }
        
        Debug.Log($"✅ AffixDatabase contains <color=cyan><b>{totalAffixes}</b></color> affixes");
        Debug.Log("   <i>(Affixes will be applied to Magic and Rare items)</i>");
        
        return true;
    }
    
    private bool CheckLootTableConfiguration()
    {
        Debug.Log("\n<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>CHECK 4: Loot Table Configuration</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        
        if (AreaLootManager.Instance == null || AreaLootManager.Instance.defaultLootTable == null)
        {
            Debug.LogError("❌ Cannot check loot table (AreaLootManager or defaultLootTable is null)");
            return false;
        }
        
        var lootTable = AreaLootManager.Instance.defaultLootTable;
        
        // Check base drop chance
        if (lootTable.baseDropChance <= 0f)
        {
            Debug.LogError("❌ <b>baseDropChance is 0 or negative!</b>");
            Debug.LogError("   <b>FIX:</b> Set baseDropChance:");
            Debug.LogError("   1. Select your loot table asset in Project");
            Debug.LogError("   2. In Inspector, set 'Base Drop Chance' to 1.0 (100% for testing)");
            return false;
        }
        
        Debug.Log($"✅ baseDropChance: <color=cyan>{lootTable.baseDropChance * 100:F0}%</color>");
        
        if (lootTable.baseDropChance < 1f)
        {
            Debug.LogWarning($"⚠️ <b>Base drop chance is {lootTable.baseDropChance * 100:F0}%</b>");
            Debug.LogWarning("   For testing, set it to 1.0 (100%) to guarantee drops");
        }
        
        // Check item type weights
        if (lootTable.itemTypeWeights == null || lootTable.itemTypeWeights.Length == 0)
        {
            Debug.LogError("❌ <b>itemTypeWeights is empty!</b>");
            Debug.LogError("   <b>FIX:</b> Configure item type weights:");
            Debug.LogError("   1. Select your loot table asset");
            Debug.LogError("   2. Expand 'Item Type Weights'");
            Debug.LogError("   3. Add entries: Weapon (weight: 1.0), Armour (weight: 1.0)");
            return false;
        }
        
        Debug.Log($"✅ itemTypeWeights configured ({lootTable.itemTypeWeights.Length} types)");
        foreach (var weight in lootTable.itemTypeWeights)
        {
            Debug.Log($"   - {weight.itemType}: <color=cyan>{weight.weight}</color>");
        }
        
        // Check rarity weights
        if (lootTable.rarityWeights == null || lootTable.rarityWeights.Length == 0)
        {
            Debug.LogError("❌ <b>rarityWeights is empty!</b>");
            Debug.LogError("   <b>FIX:</b> Configure rarity weights:");
            Debug.LogError("   1. Select your loot table asset");
            Debug.LogError("   2. Expand 'Rarity Weights'");
            Debug.LogError("   3. Add: Normal (0.60), Magic (0.30), Rare (0.10)");
            return false;
        }
        
        Debug.Log($"✅ rarityWeights configured ({lootTable.rarityWeights.Length} rarities)");
        foreach (var weight in lootTable.rarityWeights)
        {
            Debug.Log($"   - {weight.rarity}: <color=cyan>{weight.weight * 100:F0}%</color>");
        }
        
        return true;
    }
    
    [ContextMenu("Quick Fix - Create Default Loot Table")]
    public void QuickFixCreateLootTable()
    {
        Debug.Log("<color=cyan>Creating default loot table configuration...</color>");
        
#if UNITY_EDITOR
        // This will only work in the editor
        string assetPath = "Assets/Resources/LootTables/DefaultAreaLootTable.asset";
        
        // Check if directory exists
        string directory = System.IO.Path.GetDirectoryName(assetPath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
            Debug.Log($"Created directory: {directory}");
        }
        
        // Check if asset already exists
        var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<AreaLootTable>(assetPath);
        if (existing != null)
        {
            Debug.LogWarning($"Loot table already exists at: {assetPath}");
            Debug.Log("Please configure it manually in the Inspector.");
            UnityEditor.Selection.activeObject = existing;
            return;
        }
        
        // Create new loot table
        AreaLootTable lootTable = ScriptableObject.CreateInstance<AreaLootTable>();
        lootTable.name = "DefaultAreaLootTable";
        
        // Configure with sensible defaults
        lootTable.areaLevel = 80;
        lootTable.baseDropChance = 1.0f; // 100% for testing
        
        // Set item type weights
        lootTable.itemTypeWeights = new ItemTypeWeight[]
        {
            new ItemTypeWeight { itemType = ItemType.Weapon, weight = 1.0f },
            new ItemTypeWeight { itemType = ItemType.Armour, weight = 1.0f }
        };
        
        // Set rarity weights
        lootTable.rarityWeights = new RarityWeight[]
        {
            new RarityWeight { rarity = ItemRarity.Normal, weight = 0.60f },
            new RarityWeight { rarity = ItemRarity.Magic, weight = 0.30f },
            new RarityWeight { rarity = ItemRarity.Rare, weight = 0.10f }
        };
        
        UnityEditor.AssetDatabase.CreateAsset(lootTable, assetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>✅ Created loot table at: {assetPath}</color>");
        Debug.Log("<color=yellow>Next step: Assign it to AreaLootManager.defaultLootTable</color>");
        
        UnityEditor.Selection.activeObject = lootTable;
#else
        Debug.LogError("This function only works in the Unity Editor!");
#endif
    }
}

