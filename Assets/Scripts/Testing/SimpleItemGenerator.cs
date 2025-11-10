using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

/// <summary>
/// Simple script to test random item generation with affixes.
/// Add this to any GameObject and use the context menu options to generate items!
/// </summary>
public class SimpleItemGenerator : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("Area level for item generation (1-100)")]
    public int testAreaLevel = 80;
    
    [Tooltip("How many items to generate per test")]
    public int itemsPerTest = 5;
    
    [Header("Forced Rarity Settings")]
    [Tooltip("Force all generated items to be a specific rarity")]
    public bool forceRarity = false;
    
    [Tooltip("The rarity to force when 'Force Rarity' is enabled")]
    public ItemRarity forcedRarity = ItemRarity.Rare;
    
    [Header("Item Type Filter (Optional)")]
    [Tooltip("Force all generated items to be a specific type")]
    public bool forceItemType = false;
    
    [Tooltip("The item type to force when 'Force Item Type' is enabled")]
    public ItemType forcedItemType = ItemType.Weapon;
    
    [Header("Runtime Testing (Play Mode)")]
    [Tooltip("Press this key to generate a random item during play")]
    public Key generateKey = Key.F5;
    
    [Tooltip("Press this key to generate a Rare item during play")]
    public Key generateRareKey = Key.F6;
    
#if ENABLE_INPUT_SYSTEM
    private KeyControl SafeGetKeyControl(Keyboard keyboard, Key key)
    {
        if (keyboard == null)
            return null;

        if (!Enum.IsDefined(typeof(Key), key) || key == Key.None)
            return null;

        try
        {
            return keyboard[key];
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
#endif

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        KeyControl generateControl = SafeGetKeyControl(keyboard, generateKey);
        if (generateControl != null && generateControl.wasPressedThisFrame)
        {
            GenerateSingleRandomItem();
        }

        KeyControl generateRareControl = SafeGetKeyControl(keyboard, generateRareKey);
        if (generateRareControl != null && generateRareControl.wasPressedThisFrame)
        {
            GenerateSingleRareItem();
        }
#endif
    }
    
    /// <summary>
    /// Generate multiple random items with mixed rarities (respects Inspector settings)
    /// </summary>
    [ContextMenu("Generate Items (Respects Inspector Settings)")]
    public void GenerateRandomItems()
    {
        string rarityText = forceRarity ? $"FORCED {forcedRarity.ToString().ToUpper()}" : "RANDOM";
        string typeText = forceItemType ? $"{forcedItemType.ToString().ToUpper()} " : "";
        
        Debug.Log($"<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log($"<color=cyan><b>GENERATING {itemsPerTest} {typeText}ITEMS - {rarityText} (Area Level {testAreaLevel})</b></color>");
        Debug.Log($"<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
        
        int normalCount = 0;
        int magicCount = 0;
        int rareCount = 0;
        
        for (int i = 0; i < itemsPerTest; i++)
        {
            BaseItem item = null;
            int maxRetries = 50; // Prevent infinite loops
            int retryCount = 0;
            
            // Keep trying until we get a valid item or hit max retries
            while (item == null && retryCount < maxRetries)
            {
                // Generate with forced rarity if enabled
                if (forceRarity)
                {
                    item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel, forcedRarity);
                }
                else
                {
                    item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel);
                }
                
                if (item != null)
                {
                    // Filter by item type if enabled
                    if (forceItemType && item.itemType != forcedItemType)
                    {
                        item = null; // Reset and retry
                        retryCount++;
                        continue;
                    }
                    
                    // Valid item found!
                    break;
                }
                
                retryCount++;
            }
            
            if (item != null)
            {
                ItemRarity rarity = item.GetCalculatedRarity();
                LogItemDetailed(item, i + 1);
                
                // Count rarities
                switch (rarity)
                {
                    case ItemRarity.Normal: normalCount++; break;
                    case ItemRarity.Magic: magicCount++; break;
                    case ItemRarity.Rare: rareCount++; break;
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow>Item {i + 1}: Failed to generate after {retryCount} attempts (null)</color>");
                Debug.LogWarning($"  This likely means no {forcedItemType} items exist for Area Level {testAreaLevel} (item levels {testAreaLevel - 25} to {testAreaLevel + 2})");
            }
        }
        
        Debug.Log($"\n<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log($"<color=cyan><b>GENERATION COMPLETE</b></color>");
        Debug.Log($"<color=white>Normal: {normalCount} | </color><color=blue>Magic: {magicCount} | </color><color=yellow>Rare: {rareCount}</color>");
        Debug.Log($"<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
    }
    
    /// <summary>
    /// Generate multiple Rare items (forced rarity)
    /// </summary>
    [ContextMenu("Generate 5 Rare Items (Forced)")]
    public void GenerateRareItems()
    {
        Debug.Log($"<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log($"<color=yellow><b>GENERATING {itemsPerTest} RARE ITEMS (Area Level {testAreaLevel})</b></color>");
        Debug.Log($"<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
        
        for (int i = 0; i < itemsPerTest; i++)
        {
            BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel, ItemRarity.Rare);
            
            if (item != null)
            {
                LogItemDetailed(item, i + 1);
            }
        }
        
        Debug.Log($"<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
    }
    
    /// <summary>
    /// Generate multiple Magic items (forced rarity)
    /// </summary>
    [ContextMenu("Generate 5 Magic Items (Forced)")]
    public void GenerateMagicItems()
    {
        Debug.Log($"<color=blue>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log($"<color=blue><b>GENERATING {itemsPerTest} MAGIC ITEMS (Area Level {testAreaLevel})</b></color>");
        Debug.Log($"<color=blue>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
        
        for (int i = 0; i < itemsPerTest; i++)
        {
            BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel, ItemRarity.Magic);
            
            if (item != null)
            {
                LogItemDetailed(item, i + 1);
            }
        }
        
        Debug.Log($"<color=blue>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
    }
    
    /// <summary>
    /// Generate a single item (respects Inspector settings)
    /// </summary>
    [ContextMenu("Generate 1 Item (Respects Inspector Settings)")]
    public void GenerateSingleRandomItem()
    {
        BaseItem item = null;
        int maxRetries = 50; // Prevent infinite loops
        int retryCount = 0;
        
        // Keep trying until we get a valid item or hit max retries
        while (item == null && retryCount < maxRetries)
        {
            // Generate with forced rarity if enabled
            if (forceRarity)
            {
                item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel, forcedRarity);
            }
            else
            {
                item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel);
            }
            
            if (item != null)
            {
                // Filter by item type if enabled
                if (forceItemType && item.itemType != forcedItemType)
                {
                    item = null; // Reset and retry
                    retryCount++;
                    continue;
                }
                
                // Valid item found!
                break;
            }
            
            retryCount++;
        }
        
        if (item != null)
        {
            string rarityText = forceRarity ? $" ({forcedRarity})" : "";
            string typeText = forceItemType ? $" [{forcedItemType}]" : "";
            Debug.Log($"<color=green>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
            Debug.Log($"<color=green><b>GENERATED ITEM{rarityText}{typeText}</b></color>");
            LogItemDetailed(item, 1);
            Debug.Log($"<color=green>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        }
        else
        {
            Debug.LogError($"Failed to generate item after {retryCount} attempts (null result)");
            if (forceItemType)
            {
                Debug.LogError($"  No {forcedItemType} items exist for Area Level {testAreaLevel} (item levels {testAreaLevel - 25} to {testAreaLevel + 2})");
            }
        }
    }
    
    /// <summary>
    /// Generate a single Rare item (for quick testing)
    /// </summary>
    [ContextMenu("Generate 1 Rare Item")]
    public void GenerateSingleRareItem()
    {
        BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel, ItemRarity.Rare);
        
        if (item != null)
        {
            Debug.Log($"<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
            LogItemDetailed(item, 1);
            Debug.Log($"<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        }
        else
        {
            Debug.LogError("Failed to generate Rare item (null result)");
        }
    }
    
    /// <summary>
    /// Log item with summary info (one line)
    /// </summary>
    private void LogItemSummary(BaseItem item, int index)
    {
        ItemRarity rarity = item.GetCalculatedRarity();
        string rarityColor = GetRarityColor(rarity);
        
        string summary = $"<b>Item #{index}:</b> <color={rarityColor}>{item.GetDisplayName()}</color> ";
        summary += $"| Lvl {item.requiredLevel} | {item.itemType} | ";
        summary += $"<color={rarityColor}>{rarity}</color> | ";
        summary += $"Affixes: {item.prefixes.Count}P + {item.suffixes.Count}S";
        
        // Add damage/defense info
        if (item is WeaponItem weapon)
        {
            summary += $" | Dmg: {weapon.GetTotalMinDamage()}-{weapon.GetTotalMaxDamage()}";
        }
        else if (item is Armour armor)
        {
            if (armor.armour > 0) summary += $" | Arm: {armor.armour}";
            if (armor.evasion > 0) summary += $" | Eva: {armor.evasion}";
            if (armor.energyShield > 0) summary += $" | ES: {armor.energyShield}";
        }
        
        Debug.Log(summary);
    }
    
    /// <summary>
    /// Log item with detailed affix breakdown
    /// </summary>
    private void LogItemDetailed(BaseItem item, int index)
    {
        ItemRarity rarity = item.GetCalculatedRarity();
        string rarityColor = GetRarityColor(rarity);
        
        Debug.Log($"\n<b>Item #{index}:</b> <color={rarityColor}><b>{item.GetDisplayName()}</b></color>");
        Debug.Log($"Type: {item.itemType} | Rarity: <color={rarityColor}>{rarity}</color> | Level: {item.requiredLevel}");
        
        // Log weapon-specific stats
        if (item is WeaponItem weapon)
        {
            Debug.Log($"<color=orange>Base Damage: {weapon.minDamage}-{weapon.maxDamage}</color>");
            Debug.Log($"<color=orange>Final Damage: {weapon.GetTotalMinDamage()}-{weapon.GetTotalMaxDamage()}</color>");
            Debug.Log($"Crit Chance: {weapon.criticalStrikeChance}% | Attack Speed: {weapon.attackSpeed}");
        }
        
        // Log armor-specific stats
        if (item is Armour armor)
        {
            if (armor.armour > 0) Debug.Log($"<color=orange>Armour: {armor.armour}</color>");
            if (armor.evasion > 0) Debug.Log($"<color=orange>Evasion: {armor.evasion}</color>");
            if (armor.energyShield > 0) Debug.Log($"<color=orange>Energy Shield: {armor.energyShield}</color>");
        }
        
        // Log affixes
        int totalAffixes = item.prefixes.Count + item.suffixes.Count;
        Debug.Log($"<b>Affixes: {totalAffixes} total ({item.prefixes.Count} Prefix, {item.suffixes.Count} Suffix)</b>");
        
        if (item.prefixes.Count > 0)
        {
            Debug.Log($"<color=cyan><b>Prefixes ({item.prefixes.Count}):</b></color>");
            foreach (var affix in item.prefixes)
            {
                Debug.Log($"  ✅ <color=cyan>{affix.name}</color> - {affix.description}");
                Debug.Log($"     <i>(Tier: {affix.tier}, MinLevel: {affix.minLevel})</i>");
            }
        }
        
        if (item.suffixes.Count > 0)
        {
            Debug.Log($"<color=magenta><b>Suffixes ({item.suffixes.Count}):</b></color>");
            foreach (var affix in item.suffixes)
            {
                Debug.Log($"  ✅ <color=magenta>{affix.name}</color> - {affix.description}");
                Debug.Log($"     <i>(Tier: {affix.tier}, MinLevel: {affix.minLevel})</i>");
            }
        }
        
        if (totalAffixes == 0)
        {
            Debug.Log($"<color=gray>  (No affixes - Normal rarity)</color>");
        }
    }
    
    /// <summary>
    /// Get color code for rarity
    /// </summary>
    private string GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return "white";
            case ItemRarity.Magic: return "cyan";
            case ItemRarity.Rare: return "yellow";
            case ItemRarity.Unique: return "orange";
            default: return "white";
        }
    }
}

