using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Diagnostic tool specifically for debugging affix generation issues.
/// Use this when items generate but have no affixes.
/// </summary>
public class AffixGenerationDiagnostics : MonoBehaviour
{
    [Header("Test Configuration")]
    public int testItemLevel = 80;
    
    [ContextMenu("Diagnose Affix System")]
    public void DiagnoseAffixSystem()
    {
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
        Debug.Log("<color=cyan><b>AFFIX GENERATION DIAGNOSTICS</b></color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>\n");
        
        // Check 1: AffixDatabase
        if (!CheckAffixDatabase())
            return;
        
        // Check 2: Try to generate a test item
        CheckTestItemGeneration();
        
        // Check 3: Check affix compatibility for a sample item
        CheckAffixCompatibility();
        
        Debug.Log("\n<color=cyan>═══════════════════════════════════════════════════════</color>");
        Debug.Log("<color=cyan><b>DIAGNOSTICS COMPLETE</b></color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
    }
    
    private bool CheckAffixDatabase()
    {
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>CHECK: AffixDatabase Contents</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        
        if (AffixDatabase_Modern.Instance == null)
        {
            Debug.LogError("❌ <b>AffixDatabase_Modern.Instance is NULL!</b>");
            Debug.LogError("   Cannot generate affixes without AffixDatabase.");
            return false;
        }
        
        Debug.Log("✅ AffixDatabase_Modern.Instance found");
        
        // Count affixes in each category
        int weaponPrefixes = CountAffixesInCategories(AffixDatabase_Modern.Instance.weaponPrefixCategories);
        int weaponSuffixes = CountAffixesInCategories(AffixDatabase_Modern.Instance.weaponSuffixCategories);
        int armourPrefixes = CountAffixesInCategories(AffixDatabase_Modern.Instance.armourPrefixCategories);
        int armourSuffixes = CountAffixesInCategories(AffixDatabase_Modern.Instance.armourSuffixCategories);
        int jewelleryPrefixes = CountAffixesInCategories(AffixDatabase_Modern.Instance.jewelleryPrefixCategories);
        int jewellerySuffixes = CountAffixesInCategories(AffixDatabase_Modern.Instance.jewellerySuffixCategories);
        
        int totalAffixes = weaponPrefixes + weaponSuffixes + armourPrefixes + armourSuffixes + jewelleryPrefixes + jewellerySuffixes;
        
        Debug.Log($"\n<b>Affix Counts by Category:</b>");
        Debug.Log($"  Weapon Prefixes: <color=cyan>{weaponPrefixes}</color>");
        Debug.Log($"  Weapon Suffixes: <color=cyan>{weaponSuffixes}</color>");
        Debug.Log($"  Armour Prefixes: <color=cyan>{armourPrefixes}</color>");
        Debug.Log($"  Armour Suffixes: <color=cyan>{armourSuffixes}</color>");
        Debug.Log($"  Jewellery Prefixes: <color=cyan>{jewelleryPrefixes}</color>");
        Debug.Log($"  Jewellery Suffixes: <color=cyan>{jewellerySuffixes}</color>");
        Debug.Log($"  <b>TOTAL: <color=cyan>{totalAffixes}</color></b>");
        
        if (totalAffixes == 0)
        {
            Debug.LogError("\n❌ <b>AffixDatabase is EMPTY!</b>");
            Debug.LogError("   <b>FIX:</b> Import affixes from CSV:");
            Debug.LogError("   1. Unity Menu → Dexiled → Import Affixes from CSV");
            Debug.LogError("   2. Select: Comprehensive_Mods.csv");
            Debug.LogError("   3. Select: AffixDatabase.asset");
            Debug.LogError("   4. Click 'Import Affixes'");
            return false;
        }
        
        if (weaponPrefixes == 0 && weaponSuffixes == 0)
        {
            Debug.LogWarning("\n⚠️ <b>No weapon affixes found!</b>");
            Debug.LogWarning("   Weapons will generate as Normal (0 affixes)");
        }
        
        if (armourPrefixes == 0 && armourSuffixes == 0)
        {
            Debug.LogWarning("\n⚠️ <b>No armour affixes found!</b>");
            Debug.LogWarning("   Armour will generate as Normal (0 affixes)");
        }
        
        return totalAffixes > 0;
    }
    
    private void CheckTestItemGeneration()
    {
        Debug.Log("\n<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>CHECK: Test Item Generation</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        
        Debug.Log($"Generating test item at level {testItemLevel}...");
        
        // Try to generate a Rare item
        BaseItem testItem = AreaLootManager.Instance.GenerateSingleItemForArea(testItemLevel, ItemRarity.Rare);
        
        if (testItem == null)
        {
            Debug.LogError("❌ <b>Failed to generate test item!</b>");
            Debug.LogError("   Run 'LootSystemDiagnostics' to check loot system setup.");
            return;
        }
        
        Debug.Log($"✅ Generated: <color=cyan>{testItem.itemName}</color>");
        Debug.Log($"   Type: {testItem.itemType}");
        Debug.Log($"   Level: {testItem.requiredLevel}");
        Debug.Log($"   Prefixes: <color=cyan>{testItem.prefixes.Count}</color>");
        Debug.Log($"   Suffixes: <color=cyan>{testItem.suffixes.Count}</color>");
        Debug.Log($"   Calculated Rarity: <color=cyan>{testItem.GetCalculatedRarity()}</color>");
        
        if (testItem.prefixes.Count == 0 && testItem.suffixes.Count == 0)
        {
            Debug.LogWarning("\n⚠️ <b>Item has NO AFFIXES even though we forced Rare!</b>");
            Debug.LogWarning("   This means no compatible affixes were found.");
            Debug.LogWarning("   Checking compatibility below...");
        }
        else
        {
            Debug.Log("\n✅ <b>Affixes were successfully applied!</b>");
            
            if (testItem.prefixes.Count > 0)
            {
                Debug.Log($"\n<b>Prefixes ({testItem.prefixes.Count}):</b>");
                foreach (var affix in testItem.prefixes)
                {
                    Debug.Log($"  • {affix.name} - {affix.description}");
                    Debug.Log($"    Tier: {affix.tier}, MinLevel: {affix.minLevel}");
                }
            }
            
            if (testItem.suffixes.Count > 0)
            {
                Debug.Log($"\n<b>Suffixes ({testItem.suffixes.Count}):</b>");
                foreach (var affix in testItem.suffixes)
                {
                    Debug.Log($"  • {affix.name} - {affix.description}");
                    Debug.Log($"    Tier: {affix.tier}, MinLevel: {affix.minLevel}");
                }
            }
        }
    }
    
    private void CheckAffixCompatibility()
    {
        Debug.Log("\n<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>CHECK: Affix Compatibility</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        
        // Get a sample item from database
        var allItems = ItemDatabase.Instance?.GetAllItems();
        if (allItems == null || allItems.Count == 0)
        {
            Debug.LogError("❌ No items in ItemDatabase to test!");
            return;
        }
        
        // Test with first weapon
        BaseItem sampleWeapon = allItems.FirstOrDefault(i => i.itemType == ItemType.Weapon);
        if (sampleWeapon != null)
        {
            Debug.Log($"\n<b>Testing weapon compatibility:</b>");
            Debug.Log($"Sample Weapon: <color=cyan>{sampleWeapon.itemName}</color>");
            TestAffixCompatibilityForItem(sampleWeapon);
        }
        
        // Test with first armor
        BaseItem sampleArmor = allItems.FirstOrDefault(i => i.itemType == ItemType.Armour);
        if (sampleArmor != null)
        {
            Debug.Log($"\n<b>Testing armor compatibility:</b>");
            Debug.Log($"Sample Armor: <color=cyan>{sampleArmor.itemName}</color>");
            TestAffixCompatibilityForItem(sampleArmor);
        }
    }
    
    private void TestAffixCompatibilityForItem(BaseItem item)
    {
        Debug.Log($"  Item Type: {item.itemType}");
        Debug.Log($"  Item Level: {item.requiredLevel}");
        
        if (item.itemTags != null && item.itemTags.Count > 0)
        {
            Debug.Log($"  Item Tags: {string.Join(", ", item.itemTags)}");
        }
        else
        {
            Debug.LogWarning("  ⚠️ Item has NO TAGS! This might prevent affix compatibility.");
        }
        
        // Try to get compatible affixes
        var prefixes = AffixDatabase_Modern.Instance.GetRandomCompatiblePrefix(item, testItemLevel, AffixTier.Tier1);
        var suffixes = AffixDatabase_Modern.Instance.GetRandomCompatibleSuffix(item, testItemLevel, AffixTier.Tier1);
        
        if (prefixes != null)
        {
            Debug.Log($"  ✅ Found compatible PREFIX: <color=green>{prefixes.name}</color>");
            Debug.Log($"     {prefixes.description}");
            if (prefixes.compatibleTags != null && prefixes.compatibleTags.Count > 0)
            {
                Debug.Log($"     Compatible Tags: {string.Join(", ", prefixes.compatibleTags)}");
            }
        }
        else
        {
            Debug.LogError($"  ❌ NO compatible prefixes found for this item!");
            Debug.LogError($"     Problem: Either no prefixes exist, or none are compatible.");
        }
        
        if (suffixes != null)
        {
            Debug.Log($"  ✅ Found compatible SUFFIX: <color=green>{suffixes.name}</color>");
            Debug.Log($"     {suffixes.description}");
            if (suffixes.compatibleTags != null && suffixes.compatibleTags.Count > 0)
            {
                Debug.Log($"     Compatible Tags: {string.Join(", ", suffixes.compatibleTags)}");
            }
        }
        else
        {
            Debug.LogError($"  ❌ NO compatible suffixes found for this item!");
            Debug.LogError($"     Problem: Either no suffixes exist, or none are compatible.");
        }
    }
    
    private int CountAffixesInCategories(List<AffixCategory> categories)
    {
        int count = 0;
        foreach (var category in categories)
        {
            count += category.GetAllAffixes().Count;
        }
        return count;
    }
    
    [ContextMenu("Quick Test - Generate One Rare Weapon")]
    public void QuickTestRareWeapon()
    {
        Debug.Log("<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=cyan><b>QUICK TEST: Generating ONE Rare Weapon</b></color>");
        Debug.Log("<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        
        BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(testItemLevel, ItemRarity.Rare);
        
        if (item == null)
        {
            Debug.LogError("❌ Failed to generate item!");
            return;
        }
        
        Debug.Log($"\n<b>Generated:</b> <color=yellow>{item.GetDisplayName()}</color>");
        Debug.Log($"Type: {item.itemType} | Level: {item.requiredLevel}");
        Debug.Log($"Rarity: <color=yellow>{item.GetCalculatedRarity()}</color>");
        Debug.Log($"Affixes: {item.prefixes.Count} Prefix + {item.suffixes.Count} Suffix = {item.prefixes.Count + item.suffixes.Count} total");
        
        if (item.prefixes.Count + item.suffixes.Count == 0)
        {
            Debug.LogError("\n❌ <b>PROBLEM: Item has NO AFFIXES!</b>");
            Debug.LogError("This is why it shows as Normal instead of Rare.");
            Debug.LogError("\nRun 'Diagnose Affix System' for detailed analysis.");
        }
        else
        {
            Debug.Log("\n✅ <b>SUCCESS! Item has affixes:</b>");
            foreach (var affix in item.prefixes)
            {
                Debug.Log($"  PREFIX: {affix.name} - {affix.description}");
            }
            foreach (var affix in item.suffixes)
            {
                Debug.Log($"  SUFFIX: {affix.name} - {affix.description}");
            }
        }
    }
}

