using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Dexiled.Data.Items;

[CustomEditor(typeof(LootTable))]
public class LootTableEditor : Editor
{
    private LootTable lootTable;
    private CurrencyType selectedCurrency = CurrencyType.OrbOfGeneration;
    private float currencyDropChance = 100f;
    private int currencyMinQty = 1;
    private int currencyMaxQty = 1;
    private bool addToGuaranteed = true;
    
    // Item drop settings
    private int selectedItemIndex = 0;
    private string[] itemNames = new string[0];
    private float itemDropChance = 30f;
    private bool showWeapons = true;
    private bool showArmor = true;
    private bool showJewellery = true;
    
    private void OnEnable()
    {
        lootTable = (LootTable)target;
        RefreshItemList();
    }
    
    private void RefreshItemList()
    {
        if (lootTable.itemDatabase == null)
        {
            itemNames = new string[] { "Assign Item Database first" };
            return;
        }
        
        List<string> names = new List<string>();
        
        // Add weapons
        foreach (var weapon in lootTable.itemDatabase.weapons)
        {
            if (weapon != null)
                names.Add($"[Weapon] {weapon.itemName}");
        }
        
        // Add armor
        foreach (var armor in lootTable.itemDatabase.armour)
        {
            if (armor != null)
                names.Add($"[Armor] {armor.itemName}");
        }
        
        // Add jewellery
        foreach (var jewel in lootTable.itemDatabase.jewellery)
        {
            if (jewel != null)
                names.Add($"[Jewellery] {jewel.itemName}");
        }
        
        if (names.Count == 0)
            names.Add("No items in database");
            
        itemNames = names.ToArray();
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Add Tools", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Use these tools to quickly add drops from your databases.", MessageType.Info);
        
        // Currency Quick Add
        DrawCurrencyQuickAdd();
        
        EditorGUILayout.Space(10);
        
        // Item Quick Add
        DrawItemQuickAdd();
        
        EditorGUILayout.Space(10);
        
        // Quick Setup Buttons
        DrawQuickSetupButtons();
        
        serializedObject.ApplyModifiedProperties();
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(lootTable);
        }
    }
    
    private void DrawCurrencyQuickAdd()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Add Currency Drop", EditorStyles.boldLabel);
        
        // Currency selection
        selectedCurrency = (CurrencyType)EditorGUILayout.EnumPopup("Currency Type", selectedCurrency);
        
        // Show currency description
        if (lootTable.currencyDatabase != null)
        {
            var currencyData = lootTable.currencyDatabase.GetCurrency(selectedCurrency);
            if (currencyData != null)
            {
                EditorGUILayout.HelpBox(currencyData.description, MessageType.None);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Assign Currency Database to see descriptions", MessageType.Warning);
        }
        
        // Drop settings
        addToGuaranteed = EditorGUILayout.Toggle("Guaranteed Drop", addToGuaranteed);
        
        if (!addToGuaranteed)
        {
            currencyDropChance = EditorGUILayout.Slider("Drop Chance %", currencyDropChance, 0f, 100f);
        }
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Quantity Range", GUILayout.Width(100));
        currencyMinQty = EditorGUILayout.IntField(currencyMinQty, GUILayout.Width(50));
        EditorGUILayout.LabelField("to", GUILayout.Width(20));
        currencyMaxQty = EditorGUILayout.IntField(currencyMaxQty, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Add Currency Drop", GUILayout.Height(30)))
        {
            AddCurrencyDrop();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawItemQuickAdd()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Add Item Drop", EditorStyles.boldLabel);
        
        if (lootTable.itemDatabase == null)
        {
            EditorGUILayout.HelpBox("Assign Item Database to add item drops", MessageType.Warning);
            
            if (GUILayout.Button("Refresh Item List"))
            {
                RefreshItemList();
            }
            
            EditorGUILayout.EndVertical();
            return;
        }
        
        // Refresh button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Item List", GUILayout.Width(120)))
        {
            RefreshItemList();
        }
        EditorGUILayout.LabelField($"{itemNames.Length} items available");
        EditorGUILayout.EndHorizontal();
        
        // Item selection
        if (itemNames.Length > 0 && itemNames[0] != "Assign Item Database first" && itemNames[0] != "No items in database")
        {
            selectedItemIndex = EditorGUILayout.Popup("Select Item", selectedItemIndex, itemNames);
            
            // Drop settings
            itemDropChance = EditorGUILayout.Slider("Drop Chance %", itemDropChance, 0f, 100f);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Item Drop", GUILayout.Height(30)))
            {
                AddItemDrop();
            }
            
            if (GUILayout.Button("Add All Items", GUILayout.Height(30)))
            {
                AddAllItems();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox($"'Add All Items' will add all {itemNames.Length} items with {itemDropChance}% drop chance each.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("No items in database. Add items to Item Database first.", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawQuickSetupButtons()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Quick Setup Presets", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Basic Encounter\nRewards", GUILayout.Height(40)))
        {
            SetupBasicRewards();
        }
        
        if (GUILayout.Button("Boss Encounter\nRewards", GUILayout.Height(40)))
        {
            SetupBossRewards();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Add All Currencies\n(Balanced)", GUILayout.Height(40)))
        {
            SetupAllCurrencies();
        }
        
        if (GUILayout.Button("Clear All\nDrops", GUILayout.Height(40)))
        {
            ClearAllDrops();
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    
    private void AddCurrencyDrop()
    {
        LootEntry entry = new LootEntry
        {
            rewardType = RewardType.Currency,
            currencyType = selectedCurrency,
            dropChance = addToGuaranteed ? 100f : currencyDropChance,
            minQuantity = currencyMinQty,
            maxQuantity = currencyMaxQty
        };
        
        if (addToGuaranteed)
        {
            lootTable.guaranteedCurrencyDrops.Add(entry);
            Debug.Log($"Added guaranteed {selectedCurrency} drop ({currencyMinQty}-{currencyMaxQty})");
        }
        else
        {
            lootTable.randomCurrencyDrops.Add(entry);
            Debug.Log($"Added random {selectedCurrency} drop ({currencyDropChance}% chance, {currencyMinQty}-{currencyMaxQty})");
        }
        
        EditorUtility.SetDirty(lootTable);
    }
    
    private void AddItemDrop()
    {
        if (lootTable.itemDatabase == null || itemNames.Length == 0)
        {
            Debug.LogWarning("Cannot add item: Item Database not assigned or empty");
            return;
        }
        
        string selectedName = itemNames[selectedItemIndex];
        ItemData itemData = null;
        
        // Parse the item name to find the actual item
        if (selectedName.StartsWith("[Weapon]"))
        {
            string itemName = selectedName.Substring("[Weapon] ".Length);
            var weapon = lootTable.itemDatabase.weapons.Find(w => w != null && w.itemName == itemName);
            if (weapon != null)
            {
                itemData = ConvertWeaponToItemData(weapon);
            }
        }
        else if (selectedName.StartsWith("[Armor]"))
        {
            string itemName = selectedName.Substring("[Armor] ".Length);
            var armor = lootTable.itemDatabase.armour.Find(a => a != null && a.itemName == itemName);
            if (armor != null)
            {
                itemData = ConvertArmorToItemData(armor);
            }
        }
        else if (selectedName.StartsWith("[Jewellery]"))
        {
            string itemName = selectedName.Substring("[Jewellery] ".Length);
            var jewel = lootTable.itemDatabase.jewellery.Find(j => j != null && j.itemName == itemName);
            if (jewel != null)
            {
                itemData = ConvertJewelleryToItemData(jewel);
            }
        }
        
        if (itemData != null)
        {
            LootEntry entry = new LootEntry
            {
                rewardType = RewardType.Item,
                itemData = itemData,
                dropChance = itemDropChance,
                minQuantity = 1,
                maxQuantity = 1
            };
            
            lootTable.itemDrops.Add(entry);
            Debug.Log($"Added item drop: {itemData.itemName} ({itemDropChance}% chance)");
            EditorUtility.SetDirty(lootTable);
        }
        else
        {
            Debug.LogWarning($"Could not find item: {selectedName}");
        }
    }
    
    private ItemData ConvertWeaponToItemData(WeaponItem weapon)
    {
        var itemData = new ItemData
        {
            itemName = weapon.itemName,
            itemType = ItemType.Weapon,
            equipmentType = EquipmentType.MainHand,
            rarity = weapon.rarity,
            level = weapon.requiredLevel,
            itemSprite = weapon.itemIcon,
            
            // Weapon base stats
            baseDamageMin = weapon.minDamage,
            baseDamageMax = weapon.maxDamage,
            criticalStrikeChance = weapon.criticalStrikeChance,
            attackSpeed = weapon.attackSpeed,
            
            // Requirements
            requiredLevel = weapon.requiredLevel,
            requiredStrength = weapon.requiredStrength,
            requiredDexterity = weapon.requiredDexterity,
            requiredIntelligence = weapon.requiredIntelligence,
            
            // Initialize lists
            implicitModifiers = new System.Collections.Generic.List<string>()
        };
        
        // Copy implicit modifiers
        if (weapon.implicitModifiers != null && weapon.implicitModifiers.Count > 0)
        {
            foreach (var modifier in weapon.implicitModifiers)
            {
                if (modifier != null)
                    itemData.implicitModifiers.Add(modifier.description);
            }
        }
        
        return itemData;
    }
    
    private ItemData ConvertArmorToItemData(Armour armor)
    {
        var itemData = new ItemData
        {
            itemName = armor.itemName,
            itemType = ItemType.Armour,
            equipmentType = armor.equipmentType,
            rarity = armor.rarity,
            level = armor.requiredLevel,
            itemSprite = armor.itemIcon,
            
            // Armor base stats
            baseArmour = armor.armour,
            baseEvasion = armor.evasion,
            baseEnergyShield = armor.energyShield,
            
            // Requirements
            requiredLevel = armor.requiredLevel,
            requiredStrength = armor.requiredStrength,
            requiredDexterity = armor.requiredDexterity,
            requiredIntelligence = armor.requiredIntelligence,
            
            // Initialize lists
            implicitModifiers = new System.Collections.Generic.List<string>()
        };
        
        // Copy implicit modifiers
        if (armor.implicitModifiers != null && armor.implicitModifiers.Count > 0)
        {
            foreach (var modifier in armor.implicitModifiers)
            {
                if (modifier != null)
                    itemData.implicitModifiers.Add(modifier.description);
            }
        }
        
        return itemData;
    }
    
    private ItemData ConvertJewelleryToItemData(Jewellery jewel)
    {
        var itemData = new ItemData
        {
            itemName = jewel.itemName,
            itemType = ItemType.Accessory,
            equipmentType = jewel.equipmentType,
            rarity = jewel.rarity,
            level = jewel.requiredLevel,
            itemSprite = jewel.itemIcon,
            
            // Requirements
            requiredLevel = jewel.requiredLevel,
            requiredStrength = jewel.requiredStrength,
            requiredDexterity = jewel.requiredDexterity,
            requiredIntelligence = jewel.requiredIntelligence,
            
            // Initialize stats dictionary
            stats = new System.Collections.Generic.Dictionary<string, float>(),
            
            // Initialize lists
            implicitModifiers = new System.Collections.Generic.List<string>()
        };
        
        // Add jewellery base stats to dictionary
        if (jewel.life > 0)
            itemData.stats["MaxLife"] = jewel.life;
        if (jewel.mana > 0)
            itemData.stats["MaxMana"] = jewel.mana;
        if (jewel.energyShield > 0)
            itemData.stats["MaxEnergyShield"] = jewel.energyShield;
        if (jewel.ward > 0)
            itemData.stats["MaxWard"] = jewel.ward;
        
        // Copy implicit modifiers
        if (jewel.implicitModifiers != null && jewel.implicitModifiers.Count > 0)
        {
            foreach (var modifier in jewel.implicitModifiers)
            {
                if (modifier != null)
                    itemData.implicitModifiers.Add(modifier.description);
            }
        }
            
        return itemData;
    }
    
    private void AddAllItems()
    {
        if (lootTable.itemDatabase == null)
        {
            Debug.LogWarning("Cannot add items: Item Database not assigned");
            return;
        }
        
        if (!EditorUtility.DisplayDialog("Add All Items", 
            $"This will add all {itemNames.Length} items from the database to the loot table with {itemDropChance}% drop chance. Continue?", 
            "Yes", "Cancel"))
        {
            return;
        }
        
        int addedCount = 0;
        
        // Add all weapons
        foreach (var weapon in lootTable.itemDatabase.weapons)
        {
            if (weapon != null)
            {
                var itemData = ConvertWeaponToItemData(weapon);
                LootEntry entry = new LootEntry
                {
                    rewardType = RewardType.Item,
                    itemData = itemData,
                    dropChance = itemDropChance,
                    minQuantity = 1,
                    maxQuantity = 1
                };
                lootTable.itemDrops.Add(entry);
                addedCount++;
            }
        }
        
        // Add all armor
        foreach (var armor in lootTable.itemDatabase.armour)
        {
            if (armor != null)
            {
                var itemData = ConvertArmorToItemData(armor);
                LootEntry entry = new LootEntry
                {
                    rewardType = RewardType.Item,
                    itemData = itemData,
                    dropChance = itemDropChance,
                    minQuantity = 1,
                    maxQuantity = 1
                };
                lootTable.itemDrops.Add(entry);
                addedCount++;
            }
        }
        
        // Add all jewellery
        foreach (var jewel in lootTable.itemDatabase.jewellery)
        {
            if (jewel != null)
            {
                var itemData = ConvertJewelleryToItemData(jewel);
                LootEntry entry = new LootEntry
                {
                    rewardType = RewardType.Item,
                    itemData = itemData,
                    dropChance = itemDropChance,
                    minQuantity = 1,
                    maxQuantity = 1
                };
                lootTable.itemDrops.Add(entry);
                addedCount++;
            }
        }
        
        EditorUtility.SetDirty(lootTable);
        Debug.Log($"Added {addedCount} items to loot table with {itemDropChance}% drop chance");
        
        EditorUtility.DisplayDialog("Success", 
            $"Added {addedCount} items to the loot table!\n\nEach item has a {itemDropChance}% chance to drop.", 
            "OK");
    }
    
    private void SetupBasicRewards()
    {
        if (!EditorUtility.DisplayDialog("Setup Basic Rewards", 
            "This will clear existing currency drops and add basic encounter rewards. Continue?", 
            "Yes", "Cancel"))
        {
            return;
        }
        
        lootTable.guaranteedCurrencyDrops.Clear();
        lootTable.randomCurrencyDrops.Clear();
        
        // Basic rewards
        lootTable.guaranteedCurrencyDrops.Add(LootTableHelper.CreateGuaranteedCurrencyDrop(CurrencyType.OrbOfGeneration, 1, 3));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfInfusion, 25f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.FireSpirit, 15f, 1, 1));
        
        lootTable.baseExperience = 50;
        lootTable.experiencePerLevel = 10f;
        
        Debug.Log("Basic encounter rewards configured!");
        EditorUtility.SetDirty(lootTable);
    }
    
    private void SetupBossRewards()
    {
        if (!EditorUtility.DisplayDialog("Setup Boss Rewards", 
            "This will clear existing currency drops and add boss encounter rewards. Continue?", 
            "Yes", "Cancel"))
        {
            return;
        }
        
        lootTable.guaranteedCurrencyDrops.Clear();
        lootTable.randomCurrencyDrops.Clear();
        
        // Boss rewards
        lootTable.guaranteedCurrencyDrops.Add(LootTableHelper.CreateGuaranteedCurrencyDrop(CurrencyType.OrbOfGeneration, 3, 5));
        lootTable.guaranteedCurrencyDrops.Add(LootTableHelper.CreateGuaranteedCurrencyDrop(CurrencyType.OrbOfPerfection, 2, 3));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfMutation, 50f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.DivineSpirit, 10f, 1, 1));
        
        lootTable.baseExperience = 250;
        lootTable.experiencePerLevel = 30f;
        
        Debug.Log("Boss encounter rewards configured!");
        EditorUtility.SetDirty(lootTable);
    }
    
    private void SetupAllCurrencies()
    {
        if (!EditorUtility.DisplayDialog("Add All Currencies", 
            "This will add all 28 currency types as random drops with balanced drop chances. Continue?", 
            "Yes", "Cancel"))
        {
            return;
        }
        
        lootTable.randomCurrencyDrops.Clear();
        
        // ORBS (with your specified drop chances)
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfGeneration, 15f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfInfusion, 15f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfPerfection, 7f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfPerpetuity, 6f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfRedundancy, 3f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfTheVoid, 2f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfMutation, 6f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfProliferation, 12f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.OrbOfAmnesia, 4f, 1, 1));
        
        // SPIRITS (awaiting your drop chances - using defaults for now)
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.FireSpirit, 10f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.ColdSpirit, 10f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.LightningSpirit, 10f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.ChaosSpirit, 8f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.PhysicalSpirit, 10f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.LifeSpirit, 10f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.DefenseSpirit, 10f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.CritSpirit, 8f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.DivineSpirit, 3f, 1, 1));
        
        // SEALS (awaiting your drop chances - using defaults for now)
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.TranspositionSeal, 5f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.ChaosSeal, 7f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.MemorySeal, 3f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.InscriptionSeal, 5f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.AdaptationSeal, 5f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.CorrectionSeal, 7f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.EtchingSeal, 3f, 1, 1));
        
        // FRAGMENTS (awaiting your drop chances - using defaults for now)
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.Fragment1, 5f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.Fragment2, 5f, 1, 1));
        lootTable.randomCurrencyDrops.Add(LootTableHelper.CreateRandomCurrencyDrop(CurrencyType.Fragment3, 5f, 1, 1));
        
        Debug.Log("Added all 28 currencies as random drops!");
        EditorUtility.SetDirty(lootTable);
        
        EditorUtility.DisplayDialog("Success", 
            "Added all 28 currency types as random drops!\n\n" +
            "Orbs: Using your specified drop chances\n" +
            "Spirits, Seals, Fragments: Using default values\n\n" +
            "You can adjust individual drop chances in the Random Currency Drops list.", 
            "OK");
    }
    
    private void ClearAllDrops()
    {
        if (!EditorUtility.DisplayDialog("Clear All Drops", 
            "This will remove all currency and item drops. Continue?", 
            "Yes", "Cancel"))
        {
            return;
        }
        
        lootTable.guaranteedCurrencyDrops.Clear();
        lootTable.randomCurrencyDrops.Clear();
        lootTable.itemDrops.Clear();
        lootTable.cardDrops.Clear();
        
        Debug.Log("All drops cleared!");
        EditorUtility.SetDirty(lootTable);
    }
}

