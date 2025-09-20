using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : Editor
{
    private bool showDatabaseContents = true;
    private bool showWeapons = true;
    private bool showArmour = true;
    private bool showJewellery = true;
    private bool showOffHand = true;
    private bool showConsumables = true;
    
    public override void OnInspectorGUI()
    {
        ItemDatabase itemDatabase = (ItemDatabase)target;
        
        if (itemDatabase == null)
        {
            EditorGUILayout.HelpBox("ItemDatabase is null. Please select a valid ItemDatabase asset.", MessageType.Error);
            return;
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Database Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Database Management Section
        DrawDatabaseManagement(itemDatabase);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Database Contents", EditorStyles.boldLabel);
        
        // Database Contents Section
        showDatabaseContents = EditorGUILayout.Foldout(showDatabaseContents, "Database Contents", true);
        
        if (showDatabaseContents)
        {
            EditorGUI.indentLevel++;
            
                         // Weapons
             int weaponCount = itemDatabase.weapons != null ? itemDatabase.weapons.Count : 0;
             showWeapons = EditorGUILayout.Foldout(showWeapons, $"Weapons ({weaponCount})", true);
             if (showWeapons)
             {
                 EditorGUI.indentLevel++;
                 DrawItemList(itemDatabase.weapons, "Weapon");
                 EditorGUI.indentLevel--;
             }
             
             // Armour
             int armourCount = itemDatabase.armour != null ? itemDatabase.armour.Count : 0;
             showArmour = EditorGUILayout.Foldout(showArmour, $"Armour ({armourCount})", true);
             if (showArmour)
             {
                 EditorGUI.indentLevel++;
                 DrawItemList(itemDatabase.armour, "Armour");
                 EditorGUI.indentLevel--;
             }
             
             // Jewellery
             int jewelleryCount = itemDatabase.jewellery != null ? itemDatabase.jewellery.Count : 0;
             showJewellery = EditorGUILayout.Foldout(showJewellery, $"Jewellery ({jewelleryCount})", true);
             if (showJewellery)
             {
                 EditorGUI.indentLevel++;
                 DrawItemList(itemDatabase.jewellery, "Jewellery");
                 EditorGUI.indentLevel--;
             }
             
             // Off-Hand Equipment
             int offHandCount = itemDatabase.offHandEquipment != null ? itemDatabase.offHandEquipment.Count : 0;
             showOffHand = EditorGUILayout.Foldout(showOffHand, $"Off-Hand Equipment ({offHandCount})", true);
             if (showOffHand)
             {
                 EditorGUI.indentLevel++;
                 DrawItemList(itemDatabase.offHandEquipment, "Off-Hand");
                 EditorGUI.indentLevel--;
             }
             
             // Consumables
             int consumableCount = itemDatabase.consumables != null ? itemDatabase.consumables.Count : 0;
             showConsumables = EditorGUILayout.Foldout(showConsumables, $"Consumables ({consumableCount})", true);
             if (showConsumables)
             {
                 EditorGUI.indentLevel++;
                 DrawItemList(itemDatabase.consumables, "Consumable");
                 EditorGUI.indentLevel--;
             }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        DrawStatistics(itemDatabase);
    }
    
    private void DrawDatabaseManagement(ItemDatabase itemDatabase)
    {
        EditorGUILayout.LabelField("Database Management", EditorStyles.boldLabel);
        
        // Main scanning buttons
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Scan All Items", GUILayout.Height(30)))
        {
            itemDatabase.ScanAndPopulateFromResources();
            EditorUtility.SetDirty(itemDatabase);
            AssetDatabase.SaveAssets();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tag Management", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Update All Item Tags", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Update All Item Tags", 
                "This will update tags for all items in the database based on their defense values and requirements. Continue?", "Yes", "No"))
            {
                UpdateAllItemTags(itemDatabase);
                EditorUtility.SetDirty(itemDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("Updated tags for all items in database!");
            }
        }
        
        if (GUILayout.Button("Update Armour Tags Only", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Update Armour Tags", 
                "This will update tags for all armour items based on their defense values and requirements. Continue?", "Yes", "No"))
            {
                UpdateArmourTags(itemDatabase);
                EditorUtility.SetDirty(itemDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("Updated tags for all armour items!");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Clear All Items", GUILayout.Height(30)))
        {
            itemDatabase.ClearAllItems();
            EditorUtility.SetDirty(itemDatabase);
            AssetDatabase.SaveAssets();
        }
        
        EditorGUILayout.Space();
        
        // Individual category scanning
        EditorGUILayout.LabelField("Scan Individual Categories", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Scan Weapons"))
        {
            itemDatabase.ScanWeaponsOnly();
            EditorUtility.SetDirty(itemDatabase);
            AssetDatabase.SaveAssets();
        }
        
        if (GUILayout.Button("Scan Armour"))
        {
            itemDatabase.ScanArmourOnly();
            EditorUtility.SetDirty(itemDatabase);
            AssetDatabase.SaveAssets();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Scan Jewellery"))
        {
            itemDatabase.ScanJewelleryOnly();
            EditorUtility.SetDirty(itemDatabase);
            AssetDatabase.SaveAssets();
        }
        
        if (GUILayout.Button("Scan Off-Hand"))
        {
            itemDatabase.ScanOffHandOnly();
            EditorUtility.SetDirty(itemDatabase);
            AssetDatabase.SaveAssets();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Scan Consumables"))
        {
            itemDatabase.ScanConsumablesOnly();
            EditorUtility.SetDirty(itemDatabase);
            AssetDatabase.SaveAssets();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Folder structure info
        EditorGUILayout.HelpBox(
            "Expected folder structure:\n" +
            "Assets/Resources/Items/\n" +
            "├── Weapons/\n" +
            "├── Armour/\n" +
            "├── Jewellery/\n" +
            "├── OffHand/\n" +
            "└── Consumables/", 
            MessageType.Info);
    }
    
    private void DrawItemList<T>(List<T> items, string itemType) where T : BaseItem
    {
        if (items == null || items.Count == 0)
        {
            EditorGUILayout.LabelField($"No {itemType.ToLower()} items found", EditorStyles.miniLabel);
            return;
        }
        
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item == null) continue;
            
            EditorGUILayout.BeginHorizontal();
            
            // Item name with rarity color
            string displayName = $"{item.itemName} ({item.GetCalculatedRarity()})";
            Color originalColor = GUI.color;
            
            // Color code by rarity
            switch (item.GetCalculatedRarity())
            {
                case ItemRarity.Normal:
                    GUI.color = Color.white;
                    break;
                case ItemRarity.Magic:
                    GUI.color = Color.blue;
                    break;
                case ItemRarity.Rare:
                    GUI.color = Color.yellow;
                    break;
                case ItemRarity.Unique:
                    GUI.color = Color.magenta;
                    break;
            }
            
            EditorGUILayout.LabelField(displayName, GUILayout.Width(200));
            GUI.color = originalColor;
            
            // Item level
            EditorGUILayout.LabelField($"Level {item.requiredLevel}", GUILayout.Width(80));
            
            // Affix count
            int affixCount = item.GetTotalAffixCount();
            EditorGUILayout.LabelField($"{affixCount} affixes", GUILayout.Width(100));
            
            // Select button
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = item;
                EditorGUIUtility.PingObject(item);
            }
            
            // Remove button
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Remove Item", 
                    $"Are you sure you want to remove '{item.itemName}' from the database?", "Yes", "No"))
                {
                    items.RemoveAt(i);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    break;
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
    
    private void DrawStatistics(ItemDatabase itemDatabase)
    {
        if (itemDatabase == null)
        {
            EditorGUILayout.LabelField("Database Statistics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("No database selected.");
            return;
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Database Statistics", EditorStyles.boldLabel);
        
        string stats = itemDatabase.GetDatabaseStatistics();
        if (!string.IsNullOrEmpty(stats))
        {
            EditorGUILayout.LabelField(stats);
        }
        
        // Additional statistics
        var allItems = itemDatabase.GetAllItems();
        if (allItems != null)
        {
            int totalItems = allItems.Count;
            EditorGUILayout.LabelField($"Total Items: {totalItems}");
            
            // Rarity breakdown
            var rarityBreakdown = new Dictionary<ItemRarity, int>();
            
            foreach (var item in allItems)
            {
                if (item != null)
                {
                    var rarity = item.GetCalculatedRarity();
                    if (!rarityBreakdown.ContainsKey(rarity))
                        rarityBreakdown[rarity] = 0;
                    rarityBreakdown[rarity]++;
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rarity Breakdown:", EditorStyles.boldLabel);
            
            foreach (var kvp in rarityBreakdown)
            {
                EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value} items");
            }
        }
        else
        {
            EditorGUILayout.LabelField("Total Items: 0");
            EditorGUILayout.LabelField("No items found in database.");
        }
    }
    
    /// <summary>
    /// Updates tags for all items in the database based on their properties
    /// </summary>
    private void UpdateAllItemTags(ItemDatabase itemDatabase)
    {
        int updatedCount = 0;
        
        // Update weapon tags
        if (itemDatabase.weapons != null)
        {
            foreach (var weapon in itemDatabase.weapons)
            {
                if (weapon != null)
                {
                    SetAppropriateTags(weapon);
                    EditorUtility.SetDirty(weapon);
                    updatedCount++;
                }
            }
        }
        
        // Update armour tags
        if (itemDatabase.armour != null)
        {
            foreach (var armour in itemDatabase.armour)
            {
                if (armour != null)
                {
                    SetAppropriateTags(armour);
                    EditorUtility.SetDirty(armour);
                    updatedCount++;
                }
            }
        }
        
        // Update jewellery tags
        if (itemDatabase.jewellery != null)
        {
            foreach (var jewellery in itemDatabase.jewellery)
            {
                if (jewellery != null)
                {
                    SetAppropriateTags(jewellery);
                    EditorUtility.SetDirty(jewellery);
                    updatedCount++;
                }
            }
        }
        
        // Update off-hand equipment tags
        if (itemDatabase.offHandEquipment != null)
        {
            foreach (var offHand in itemDatabase.offHandEquipment)
            {
                if (offHand != null)
                {
                    SetAppropriateTags(offHand);
                    EditorUtility.SetDirty(offHand);
                    updatedCount++;
                }
            }
        }
        
        Debug.Log($"Updated tags for {updatedCount} items in the database.");
    }
    
    /// <summary>
    /// Updates tags for armour items only
    /// </summary>
    private void UpdateArmourTags(ItemDatabase itemDatabase)
    {
        int updatedCount = 0;
        
        if (itemDatabase.armour != null)
        {
            foreach (var armour in itemDatabase.armour)
            {
                if (armour != null)
                {
                    SetAppropriateTags(armour);
                    EditorUtility.SetDirty(armour);
                    updatedCount++;
                }
            }
        }
        
        Debug.Log($"Updated tags for {updatedCount} armour items.");
    }
    
    /// <summary>
    /// Sets appropriate tags for an item based on its properties
    /// </summary>
    private void SetAppropriateTags(BaseItem item)
    {
        if (item.itemTags == null)
            item.itemTags = new List<string>();
        
        item.itemTags.Clear();
        
        // Set base tags based on item type
        switch (item.itemType)
        {
            case ItemType.Weapon:
                item.itemTags.Add("weapon");
                
                // Add weapon-specific tags based on weapon type and handedness
                if (item is WeaponItem weapon)
                {
                    // Add weapon type tag
                    item.itemTags.Add(weapon.weaponType.ToString().ToLower());
                    
                    // Add handedness tag
                    item.itemTags.Add(weapon.handedness.ToString().ToLower());
                    
                    // Add specific weapon type tags for affix compatibility
                    switch (weapon.weaponType)
                    {
                        case WeaponItemType.Sword:
                        case WeaponItemType.Axe:
                        case WeaponItemType.Mace:
                        case WeaponItemType.Dagger:
                        case WeaponItemType.Claw:
                        case WeaponItemType.RitualDagger:
                            item.itemTags.Add("melee");
                            break;
                        case WeaponItemType.Bow:
                        case WeaponItemType.Wand:
                            item.itemTags.Add("ranged");
                            break;
                        case WeaponItemType.Staff:
                        case WeaponItemType.Sceptre:
                            item.itemTags.Add("spell");
                            break;
                    }
                    
                    // Add attack type tags
                    switch (weapon.weaponType)
                    {
                        case WeaponItemType.Sword:
                        case WeaponItemType.Axe:
                        case WeaponItemType.Mace:
                        case WeaponItemType.Dagger:
                        case WeaponItemType.Claw:
                        case WeaponItemType.RitualDagger:
                        case WeaponItemType.Bow:
                            item.itemTags.Add("attack");
                            break;
                        case WeaponItemType.Wand:
                        case WeaponItemType.Staff:
                        case WeaponItemType.Sceptre:
                            item.itemTags.Add("spell");
                            break;
                    }
                    
                    // Add requirement-based tags
                    AddRequirementTags(item.itemTags, weapon.requiredStrength, weapon.requiredDexterity, weapon.requiredIntelligence);
                }
                break;
                
            case ItemType.Armour:
                // Always add "defence" tag for armor items since they all provide some form of defense
                item.itemTags.Add("defence");
                
                // Add armor-specific tags based on defense values and requirements
                if (item is Armour armour)
                {
                    // Add defense-based tags (only adds specific tags when values > 0)
                    AddDefenseTags(item.itemTags, armour.armour, armour.evasion, armour.energyShield);
                    
                    // Add requirement-based tags
                    AddRequirementTags(item.itemTags, armour.requiredStrength, armour.requiredDexterity, armour.requiredIntelligence);
                    
                    // Add equipment type tag
                    AddEquipmentTypeTag(item.itemTags, armour.armourSlot);
                }
                break;
                
            case ItemType.Accessory:
                item.itemTags.Add("jewellery");
                item.itemTags.Add("accessory");
                
                // Add accessory-specific tags
                if (item is Jewellery jewellery)
                {
                    // Add requirement-based tags
                    AddRequirementTags(item.itemTags, jewellery.requiredStrength, jewellery.requiredDexterity, jewellery.requiredIntelligence);
                    
                    // Add equipment type tag
                    AddEquipmentTypeTag(item.itemTags, jewellery.jewelleryType);
                }
                break;
                
            case ItemType.Consumable:
                item.itemTags.Add("consumable");
                break;
        }
    }
    
    /// <summary>
    /// Adds defense-based tags based on armor, evasion, and energy shield values
    /// </summary>
    private void AddDefenseTags(List<string> tags, float armour, float evasion, float energyShield)
    {
        bool hasArmour = armour > 0;
        bool hasEvasion = evasion > 0;
        bool hasEnergyShield = energyShield > 0;
        
        // Single defense types
        if (hasArmour && !hasEvasion && !hasEnergyShield)
        {
            tags.Add("armour");
        }
        else if (hasEvasion && !hasArmour && !hasEnergyShield)
        {
            tags.Add("evasion");
        }
        else if (hasEnergyShield && !hasArmour && !hasEvasion)
        {
            tags.Add("es");
        }
        
        // Dual defense types
        if (hasArmour && hasEvasion && !hasEnergyShield)
        {
            tags.Add("armour_evasion");
        }
        else if (hasArmour && hasEnergyShield && !hasEvasion)
        {
            tags.Add("armour_es");
        }
        else if (hasEvasion && hasEnergyShield && !hasArmour)
        {
            tags.Add("evasion_es");
        }
        
        // All three defense types
        if (hasArmour && hasEvasion && hasEnergyShield)
        {
            tags.Add("armour_evasion_es");
        }
    }
    
    /// <summary>
    /// Adds requirement-based tags based on strength, dexterity, and intelligence requirements
    /// </summary>
    private void AddRequirementTags(List<string> tags, int requiredStrength, int requiredDexterity, int requiredIntelligence)
    {
        bool hasStrength = requiredStrength > 0;
        bool hasDexterity = requiredDexterity > 0;
        bool hasIntelligence = requiredIntelligence > 0;
        
        // Single attribute requirements
        if (hasStrength && !hasDexterity && !hasIntelligence)
        {
            tags.Add("strength");
        }
        else if (hasDexterity && !hasStrength && !hasIntelligence)
        {
            tags.Add("dex");
        }
        else if (hasIntelligence && !hasStrength && !hasDexterity)
        {
            tags.Add("int");
        }
        
        // Dual attribute requirements
        if (hasStrength && hasDexterity && !hasIntelligence)
        {
            tags.Add("str_dex");
        }
        else if (hasStrength && hasIntelligence && !hasDexterity)
        {
            tags.Add("str_int");
        }
        else if (hasDexterity && hasIntelligence && !hasStrength)
        {
            tags.Add("dex_int");
        }
        
        // All three attribute requirements
        if (hasStrength && hasDexterity && hasIntelligence)
        {
            tags.Add("str_dex_int");
        }
    }
    
    /// <summary>
    /// Adds equipment type tags based on armor slot or jewellery type
    /// </summary>
    private void AddEquipmentTypeTag(List<string> tags, ArmourSlot armourSlot)
    {
        switch (armourSlot)
        {
            case ArmourSlot.Helmet:
                tags.Add("helmet");
                break;
            case ArmourSlot.BodyArmour:
                tags.Add("body_armour");
                break;
            case ArmourSlot.Gloves:
                tags.Add("gloves");
                break;
            case ArmourSlot.Boots:
                tags.Add("boots");
                break;
            case ArmourSlot.Shield:
                tags.Add("shield");
                break;
        }
    }
    
    /// <summary>
    /// Adds equipment type tags based on jewellery type
    /// </summary>
    private void AddEquipmentTypeTag(List<string> tags, JewelleryType jewelleryType)
    {
        switch (jewelleryType)
        {
            case JewelleryType.Ring:
                tags.Add("ring");
                break;
            case JewelleryType.Amulet:
                tags.Add("amulet");
                break;
            case JewelleryType.Belt:
                tags.Add("belt");
                break;
        }
    }
}
