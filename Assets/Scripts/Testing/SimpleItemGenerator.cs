using System;
using System.Collections.Generic;
using UnityEngine;
using Dexiled.Data.Items;
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
    
    [Header("Name Generation")]
    [Tooltip("Data for generating Magic and Rare item names (optional - will use AreaLootManager's if not set)")]
    public NameGenerationData nameGenerationData;
    
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
                // Ensure name is generated (fallback if AreaLootTable didn't generate it)
                EnsureNameGenerated(item);
                
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
                EnsureNameGenerated(item);
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
                EnsureNameGenerated(item);
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
            EnsureNameGenerated(item);
            
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
            EnsureNameGenerated(item);
            
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
    /// Test weapon base damage rolling - generates 4 weapons directly from asset to verify rolling works
    /// </summary>
    [ContextMenu("Test Weapon Rolling (4 Weapons - Direct)")]
    public void TestWeaponRolling()
    {
        Debug.Log($"<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log($"<color=cyan><b>TESTING WEAPON BASE DAMAGE ROLLING</b></color>");
        Debug.Log($"<color=cyan>Creating 4 weapons directly to verify rolling logic...</color>");
        Debug.Log($"<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
        
        // Load Worn Hatchet asset directly
        WeaponItem wornHatchetAsset = Resources.Load<WeaponItem>("Items/Weapons/Axes/OneHanded/Worn Hatchet");
        
        if (wornHatchetAsset == null)
        {
            Debug.LogError("<color=red>❌ Could not load Worn Hatchet asset from Resources!</color>");
            Debug.LogError("   Path should be: Assets/Resources/Items/Weapons/Axes/OneHanded/Worn Hatchet.asset");
            Debug.LogError("   Make sure the file exists and is in a Resources folder!");
            return;
        }
        
        Debug.Log($"<color=green>✅ Loaded asset: {wornHatchetAsset.itemName} (Base: {wornHatchetAsset.minDamage}-{wornHatchetAsset.maxDamage})</color>\n");
        
        // Generate 4 weapon instances with rolled damage
        List<WeaponItem> generatedWeapons = new List<WeaponItem>();
        
        for (int i = 0; i < 4; i++)
        {
            WeaponItem weapon = CreateWeaponWithRolling(wornHatchetAsset);
            
            if (weapon != null)
            {
                generatedWeapons.Add(weapon);
                LogWeaponRollingDetails(weapon, i + 1);
            }
        }
        
        // Add all generated weapons to player inventory
        if (generatedWeapons.Count > 0)
        {
            Debug.Log($"\n<color=lime>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
            Debug.Log($"<color=lime><b>ADDING TO INVENTORY</b></color>");
            
            foreach (var weapon in generatedWeapons)
            {
                if (CharacterManager.Instance != null)
                {
                    CharacterManager.Instance.inventoryItems.Add(weapon);
                    CharacterManager.Instance.OnItemAdded?.Invoke(weapon);
                    Debug.Log($"<color=lime>✅ Added to inventory: {weapon.itemName} (Rolled: {weapon.rolledBaseDamage:F0})</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=yellow>⚠️ CharacterManager.Instance is null, could not add {weapon.itemName} to inventory</color>");
                }
            }
            
            Debug.Log($"<color=lime>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        }
        
        Debug.Log($"\n<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log($"<color=cyan><b>ROLLING TEST COMPLETE</b></color>");
        Debug.Log($"<color=cyan>Generated {generatedWeapons.Count} weapons with unique rolled values!</color>");
        Debug.Log($"<color=cyan>Added {generatedWeapons.Count} weapons to inventory!</color>");
        Debug.Log($"<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
    }
    
    /// <summary>
    /// Create a weapon instance with rolled base damage
    /// </summary>
    private WeaponItem CreateWeaponWithRolling(WeaponItem original)
    {
        WeaponItem copy = ScriptableObject.CreateInstance<WeaponItem>();
        
        // Copy base properties
        copy.itemName = original.itemName;
        copy.description = original.description;
        copy.itemIcon = original.itemIcon;
        copy.weaponType = original.weaponType;
        copy.handedness = original.handedness;
        copy.attackSpeed = original.attackSpeed;
        copy.criticalStrikeChance = original.criticalStrikeChance;
        copy.criticalStrikeMultiplier = original.criticalStrikeMultiplier;
        copy.primaryDamageType = original.primaryDamageType;
        copy.requiredLevel = original.requiredLevel;
        copy.itemType = ItemType.Weapon;
        copy.equipmentType = EquipmentType.MainHand;
        
        // Copy damage ranges
        copy.minDamage = original.minDamage;
        copy.maxDamage = original.maxDamage;
        
        // ⚔️ ROLL base damage between min and max (whole numbers only)
        copy.rolledBaseDamage = UnityEngine.Random.Range((int)original.minDamage, (int)original.maxDamage + 1); // +1 to make maxDamage inclusive
        
        // Copy item tags
        if (original.itemTags != null)
        {
            copy.itemTags = new List<string>(original.itemTags);
        }
        
        // Initialize affix lists
        copy.implicitModifiers = new List<Affix>();
        copy.prefixes = new List<Affix>();
        copy.suffixes = new List<Affix>();
        
        // Optionally add affixes for more realistic testing
        if (AffixDatabase_Modern.Instance != null)
        {
            AffixDatabase_Modern.Instance.GenerateRandomAffixes(copy, original.requiredLevel, ItemRarity.Rare);
        }
        
        // Generate name for Magic/Rare items
        EnsureNameGenerated(copy);
        
        return copy;
    }
    
    /// <summary>
    /// Ensure item has a generated name (fallback if AreaLootTable didn't generate it)
    /// </summary>
    private void EnsureNameGenerated(BaseItem item)
    {
        if (item == null) return;
        
        // Only generate names for Magic and Rare items
        if (item.rarity == ItemRarity.Magic || item.rarity == ItemRarity.Rare)
        {
            // If name already generated, skip
            if (!string.IsNullOrEmpty(item.generatedName))
            {
                return;
            }
            
            // Try to get nameGenerationData from various sources (in priority order)
            NameGenerationData nameData = null;
            
            // 1. Use this generator's nameGenerationData (if assigned)
            if (nameGenerationData != null)
            {
                nameData = nameGenerationData;
            }
            // 2. Fallback to AreaLootManager's nameGenerationData
            else if (AreaLootManager.Instance != null && AreaLootManager.Instance.nameGenerationData != null)
            {
                nameData = AreaLootManager.Instance.nameGenerationData;
            }
            // 3. Fallback to LootManager's nameGenerationData
            else if (LootManager.Instance != null && LootManager.Instance.nameGenerationData != null)
            {
                nameData = LootManager.Instance.nameGenerationData;
            }
            
            // Generate name
            item.generatedName = ItemNameGenerator.GenerateItemName(item, nameData);
            
            if (!string.IsNullOrEmpty(item.generatedName))
            {
                Debug.Log($"<color=lime>[SimpleItemGenerator] Generated name: '{item.generatedName}'</color>");
            }
            else if (nameData == null)
            {
                Debug.LogWarning($"<color=yellow>[SimpleItemGenerator] No NameGenerationData available. Item '{item.itemName}' will use default name.</color>");
            }
        }
    }
    
    /// <summary>
    /// Log weapon with focus on rolled damage
    /// </summary>
    private void LogWeaponRollingDetails(WeaponItem weapon, int index)
    {
        if (weapon == null) return;
        
        Debug.Log($"\n<color=lime><b>⚔️ Weapon #{index}: {weapon.itemName}</b></color>");
        Debug.Log($"<color=white>Base Damage Range: {weapon.minDamage}-{weapon.maxDamage}</color>");
        
        if (weapon.rolledBaseDamage > 0f)
        {
            // Verify it's within range
            bool isValid = weapon.rolledBaseDamage >= weapon.minDamage && 
                          weapon.rolledBaseDamage <= weapon.maxDamage;
            
            string status = isValid ? "✅" : "❌ OUT OF RANGE!";
            Debug.Log($"<color=lime>{status} Rolled Base Damage: {weapon.rolledBaseDamage:F0}</color> (for card scaling)");
            
            if (!isValid)
            {
                Debug.LogError($"<color=red>ERROR: Rolled value {weapon.rolledBaseDamage:F0} is outside range {weapon.minDamage}-{weapon.maxDamage}!</color>");
            }
        }
        else
        {
            Debug.LogWarning($"<color=yellow>⚠️ Rolled Base Damage: NOT SET (0.00)</color>");
            Debug.LogWarning($"   This weapon was not rolled properly! Check AreaLootTable.CreateWeaponCopy()");
        }
        
        Debug.Log($"<color=orange>Final Damage: {weapon.GetTotalMinDamage()}-{weapon.GetTotalMaxDamage()}</color>");
        Debug.Log($"<color=gray>Rarity: {weapon.GetCalculatedRarity()} | Affixes: {weapon.prefixes.Count}P + {weapon.suffixes.Count}S</color>");
        
        // Log rolled affix values
        if (weapon.prefixes.Count > 0 || weapon.suffixes.Count > 0)
        {
            Debug.Log($"<color=cyan>Affix Details:</color>");
            
            foreach (var prefix in weapon.prefixes)
            {
                string rolledDesc = TooltipFormattingUtils.FormatAffix(prefix);
                Debug.Log($"  <color=cyan>PREFIX: {rolledDesc}</color>");
                
                if (prefix.modifiers.Count > 0)
                {
                    var mod = prefix.modifiers[0];
                    Debug.Log($"    isRolled: {mod.isRolled}, isDualRange: {mod.isDualRange}, rolledValue: {mod.rolledValue}");
                }
            }
            
            foreach (var suffix in weapon.suffixes)
            {
                string rolledDesc = TooltipFormattingUtils.FormatAffix(suffix);
                Debug.Log($"  <color=magenta>SUFFIX: {rolledDesc}</color>");
                
                if (suffix.modifiers.Count > 0)
                {
                    var mod = suffix.modifiers[0];
                    Debug.Log($"    isRolled: {mod.isRolled}, isDualRange: {mod.isDualRange}, rolledValue: {mod.rolledValue}");
                }
            }
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
            string rolledInfo = weapon.rolledBaseDamage > 0f ? $" (Rolled: {weapon.rolledBaseDamage:F1})" : "";
            summary += $" | Dmg: {weapon.GetTotalMinDamage()}-{weapon.GetTotalMaxDamage()}{rolledInfo}";
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
            Debug.Log($"<color=orange>Base Damage Range: {weapon.minDamage}-{weapon.maxDamage}</color>");
            if (weapon.rolledBaseDamage > 0f)
            {
                Debug.Log($"<color=lime>✨ Rolled Base Damage: {weapon.rolledBaseDamage:F2}</color> (for card scaling)");
            }
            else
            {
                Debug.Log($"<color=yellow>⚠️ Rolled Base Damage: NOT SET (using average fallback)</color>");
            }
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

