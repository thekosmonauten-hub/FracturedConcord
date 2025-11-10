using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Comprehensive tester for rarity-based affix generation in the Area Loot System
/// </summary>
public class RarityAffixTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private int testAreaLevel = 30;
    [SerializeField] private int testItemsPerRarity = 10;
    [SerializeField] private bool showDetailedAffixInfo = true;
    [SerializeField] private bool testSpecificItemTypes = true;
    
    [Header("Force Rarity Testing")]
    [SerializeField] private bool forceNormalRarity = false;
    [SerializeField] private bool forceMagicRarity = false;
    [SerializeField] private bool forceRareRarity = false;
    
    [ContextMenu("Test All Rarities")]
    public void TestAllRarities()
    {
        Debug.Log("=== COMPREHENSIVE RARITY & AFFIX TESTING ===");
        Debug.Log($"Area Level: {testAreaLevel}");
        
        var (minLevel, maxLevel) = AreaLootManager.Instance.GetValidLevelRangeForArea(testAreaLevel);
        Debug.Log($"Item Level Range: {minLevel}-{maxLevel}");
        
        // Test each rarity category
        TestNormalItems();
        TestMagicItems();
        TestRareItems();
        
        // Test rarity distribution
        TestRarityDistribution();
        
        // Test affix compatibility
        TestAffixCompatibility();
    }
    
    [ContextMenu("Test Normal Items")]
    public void TestNormalItems()
    {
        Debug.Log("\n--- TESTING NORMAL RARITY ITEMS ---");
        List<BaseItem> normalItems = GenerateItemsWithForcedRarity(ItemRarity.Normal, testItemsPerRarity);
        
        foreach (var item in normalItems)
        {
            AnalyzeItem(item, "Normal");
        }
        
        Debug.Log($"Generated {normalItems.Count} Normal items");
    }
    
    [ContextMenu("Test Magic Items")]
    public void TestMagicItems()
    {
        Debug.Log("\n--- TESTING MAGIC RARITY ITEMS ---");
        List<BaseItem> magicItems = GenerateItemsWithForcedRarity(ItemRarity.Magic, testItemsPerRarity);
        
        foreach (var item in magicItems)
        {
            AnalyzeItem(item, "Magic");
        }
        
        // Analyze affix counts for Magic items
        AnalyzeMagicAffixCounts(magicItems);
    }
    
    [ContextMenu("Test Rare Items")]
    public void TestRareItems()
    {
        Debug.Log("\n--- TESTING RARE RARITY ITEMS ---");
        List<BaseItem> rareItems = GenerateItemsWithForcedRarity(ItemRarity.Rare, testItemsPerRarity);
        
        foreach (var item in rareItems)
        {
            AnalyzeItem(item, "Rare");
        }
        
        // Analyze affix counts for Rare items
        AnalyzeRareAffixCounts(rareItems);
    }
    
    [ContextMenu("Test Rarity Distribution")]
    public void TestRarityDistribution()
    {
        Debug.Log("\n--- TESTING RARITY DISTRIBUTION ---");
        
        int sampleSize = 100;
        List<BaseItem> sampleItems = new List<BaseItem>();
        
        for (int i = 0; i < sampleSize; i++)
        {
            BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel);
            if (item != null)
            {
                sampleItems.Add(item);
            }
        }
        
        // Count rarities
        int normalCount = sampleItems.Count(i => i.GetCalculatedRarity() == ItemRarity.Normal);
        int magicCount = sampleItems.Count(i => i.GetCalculatedRarity() == ItemRarity.Magic);
        int rareCount = sampleItems.Count(i => i.GetCalculatedRarity() == ItemRarity.Rare);
        int uniqueCount = sampleItems.Count(i => i.GetCalculatedRarity() == ItemRarity.Unique);
        
        Debug.Log($"Rarity Distribution (Sample: {sampleItems.Count}):");
        Debug.Log($"  Normal: {normalCount} ({(float)normalCount/sampleItems.Count*100:F1}%)");
        Debug.Log($"  Magic:  {magicCount} ({(float)magicCount/sampleItems.Count*100:F1}%)");
        Debug.Log($"  Rare:   {rareCount} ({(float)rareCount/sampleItems.Count*100:F1}%)");
        Debug.Log($"  Unique: {uniqueCount} ({(float)uniqueCount/sampleItems.Count*100:F1}%)");
    }
    
    [ContextMenu("Test Affix Compatibility")]
    public void TestAffixCompatibility()
    {
        Debug.Log("\n--- TESTING AFFIX COMPATIBILITY ---");
        
        // Test different item types
        foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
        {
            if (itemType == ItemType.Material) continue; // Skip materials
            
            Debug.Log($"\n Testing {itemType} items:");
            
            List<BaseItem> eligibleItems = AreaLootManager.Instance.GetEligibleItemsForArea(testAreaLevel, itemType);
            if (eligibleItems.Count == 0)
            {
                Debug.Log($"  No eligible {itemType} items for level {testAreaLevel}");
                continue;
            }
            
            // Generate a few Magic items of this type
            for (int i = 0; i < 3; i++)
            {
                BaseItem testItem = eligibleItems[Random.Range(0, eligibleItems.Count)];
                BaseItem generatedItem = CreateItemCopyForTesting(testItem);
                
                // Force Magic rarity for testing
                ApplyForcedRarity(generatedItem, ItemRarity.Magic);
                
                Debug.Log($"  {generatedItem.GetDisplayName()}:");
                if (generatedItem.itemTags != null && generatedItem.itemTags.Count > 0)
                {
                    Debug.Log($"    Tags: {string.Join(", ", generatedItem.itemTags)}");
                }
                
                foreach (var affix in generatedItem.prefixes)
                {
                    Debug.Log($"    Prefix: {affix.description}");
                    if (showDetailedAffixInfo)
                    {
                        Debug.Log($"      Required Tags: {string.Join(", ", affix.requiredTags)}");
                        Debug.Log($"      Modifiers: {affix.modifiers.Count}");
                    }
                }
                
                foreach (var affix in generatedItem.suffixes)
                {
                    Debug.Log($"    Suffix: {affix.description}");
                    if (showDetailedAffixInfo)
                    {
                        Debug.Log($"      Required Tags: {string.Join(", ", affix.requiredTags)}");
                        Debug.Log($"      Modifiers: {affix.modifiers.Count}");
                    }
                }
            }
        }
    }
    
    private List<BaseItem> GenerateItemsWithForcedRarity(ItemRarity targetRarity, int count)
    {
        List<BaseItem> items = new List<BaseItem>();
        
        for (int i = 0; i < count; i++)
        {
            BaseItem baseItem = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel);
            if (baseItem != null)
            {
                // Force the desired rarity
                ApplyForcedRarity(baseItem, targetRarity);
                items.Add(baseItem);
            }
        }
        
        return items;
    }
    
    private void ApplyForcedRarity(BaseItem item, ItemRarity targetRarity)
    {
        if (AffixDatabase_Modern.Instance == null)
        {
            Debug.LogWarning("AffixDatabase_Modern.Instance is null - cannot apply affixes");
            return;
        }
        
        // Clear existing affixes
        item.prefixes.Clear();
        item.suffixes.Clear();
        
        // Apply affixes based on target rarity
        switch (targetRarity)
        {
            case ItemRarity.Normal:
                // No affixes for Normal items
                break;
                
            case ItemRarity.Magic:
                // 1-2 affixes for Magic items
                AffixDatabase_Modern.Instance.GenerateRandomAffixes(item, testAreaLevel, 1.0f, 0.0f);
                break;
                
            case ItemRarity.Rare:
                // 3-6 affixes for Rare items  
                AffixDatabase_Modern.Instance.GenerateRandomAffixes(item, testAreaLevel, 0.0f, 1.0f);
                break;
        }
    }
    
    private void AnalyzeItem(BaseItem item, string rarityLabel)
    {
        if (item == null) return;
        
        string itemInfo = $"{rarityLabel}: {item.GetDisplayName()} (Level {item.requiredLevel})";
        
        // Count affixes
        int prefixCount = item.prefixes?.Count ?? 0;
        int suffixCount = item.suffixes?.Count ?? 0;
        int implicitCount = item.implicitModifiers?.Count ?? 0;
        
        itemInfo += $" | Affixes: {prefixCount}P + {suffixCount}S + {implicitCount}I";
        
        Debug.Log(itemInfo);
        
        if (showDetailedAffixInfo && (prefixCount > 0 || suffixCount > 0))
        {
            // Show prefix details
            foreach (var prefix in item.prefixes)
            {
                Debug.Log($"  PREFIX: {prefix.description} (Tier: {prefix.tier})");
                foreach (var modifier in prefix.modifiers)
                {
                    Debug.Log($"    → {modifier.statName}: {modifier.minValue}-{modifier.maxValue} ({modifier.modifierType})");
                }
            }
            
            // Show suffix details
            foreach (var suffix in item.suffixes)
            {
                Debug.Log($"  SUFFIX: {suffix.description} (Tier: {suffix.tier})");
                foreach (var modifier in suffix.modifiers)
                {
                    Debug.Log($"    → {modifier.statName}: {modifier.minValue}-{modifier.maxValue} ({modifier.modifierType})");
                }
            }
        }
    }
    
    private void AnalyzeMagicAffixCounts(List<BaseItem> magicItems)
    {
        Debug.Log("\n--- MAGIC ITEM AFFIX ANALYSIS ---");
        
        int[] affixCounts = new int[5]; // 0, 1, 2, 3, 4+ affixes
        
        foreach (var item in magicItems)
        {
            int totalAffixes = item.prefixes.Count + item.suffixes.Count;
            int index = Mathf.Min(totalAffixes, 4);
            affixCounts[index]++;
        }
        
        Debug.Log("Magic Item Affix Distribution:");
        Debug.Log($"  0 affixes: {affixCounts[0]} items");
        Debug.Log($"  1 affix:   {affixCounts[1]} items");
        Debug.Log($"  2 affixes: {affixCounts[2]} items");
        Debug.Log($"  3 affixes: {affixCounts[3]} items");
        Debug.Log($"  4+ affixes: {affixCounts[4]} items");
    }
    
    private void AnalyzeRareAffixCounts(List<BaseItem> rareItems)
    {
        Debug.Log("\n--- RARE ITEM AFFIX ANALYSIS ---");
        
        int[] affixCounts = new int[8]; // 0-7+ affixes
        
        foreach (var item in rareItems)
        {
            int totalAffixes = item.prefixes.Count + item.suffixes.Count;
            int index = Mathf.Min(totalAffixes, 7);
            affixCounts[index]++;
        }
        
        Debug.Log("Rare Item Affix Distribution:");
        for (int i = 0; i < affixCounts.Length; i++)
        {
            string label = i < 7 ? $"{i} affixes" : "7+ affixes";
            Debug.Log($"  {label}: {affixCounts[i]} items");
        }
    }
    
    private BaseItem CreateItemCopyForTesting(BaseItem original)
    {
        // Create a runtime copy (same logic as AreaLootTable)
        if (original is WeaponItem weapon)
        {
            return CreateWeaponCopy(weapon);
        }
        else if (original is Armour armor)
        {
            return CreateArmourCopy(armor);
        }
        
        // Generic fallback
        return original;
    }
    
    private WeaponItem CreateWeaponCopy(WeaponItem original)
    {
        WeaponItem copy = ScriptableObject.CreateInstance<WeaponItem>();
        
        // Copy all properties (simplified version)
        copy.itemName = original.itemName;
        copy.itemType = original.itemType;
        copy.equipmentType = original.equipmentType;
        copy.weaponType = original.weaponType;
        copy.minDamage = original.minDamage;
        copy.maxDamage = original.maxDamage;
        copy.itemTags = new List<string>(original.itemTags);
        copy.implicitModifiers = new List<Affix>(original.implicitModifiers);
        copy.prefixes = new List<Affix>();
        copy.suffixes = new List<Affix>();
        
        return copy;
    }
    
    private Armour CreateArmourCopy(Armour original)
    {
        Armour copy = ScriptableObject.CreateInstance<Armour>();
        
        // Copy all properties (simplified version)
        copy.itemName = original.itemName;
        copy.itemType = original.itemType;
        copy.equipmentType = original.equipmentType;
        copy.armourSlot = original.armourSlot;
        copy.armour = original.armour;
        copy.evasion = original.evasion;
        copy.energyShield = original.energyShield;
        copy.itemTags = new List<string>(original.itemTags);
        copy.implicitModifiers = new List<Affix>(original.implicitModifiers);
        copy.prefixes = new List<Affix>();
        copy.suffixes = new List<Affix>();
        
        return copy;
    }
}
