using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq; // Added for .Select() and .Where()

[CustomEditor(typeof(AffixDatabase))]
public class AffixDatabaseEditor : Editor
{
    private bool showWeaponPrefixes = true;
    private bool showWeaponSuffixes = true;
    private bool showArmourPrefixes = true;
    private bool showArmourSuffixes = true;
    private bool showJewelleryPrefixes = true;
    private bool showJewellerySuffixes = true;
    
    // Track expanded states for categories and sub-categories
    private Dictionary<string, bool> expandedCategories = new Dictionary<string, bool>();
    private Dictionary<string, bool> expandedSubCategories = new Dictionary<string, bool>();
    private Dictionary<string, bool> expandedAffixes = new Dictionary<string, bool>();
    
    public override void OnInspectorGUI()
    {
        AffixDatabase affixDatabase = (AffixDatabase)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Affix Database Inspector", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Database Management Section
        DrawDatabaseManagement(affixDatabase);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Database Contents", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Draw hierarchical view of all affixes
        DrawWeaponPrefixes(affixDatabase);
        DrawWeaponSuffixes(affixDatabase);
        DrawArmourPrefixes(affixDatabase);
        DrawArmourSuffixes(affixDatabase);
        DrawJewelleryPrefixes(affixDatabase);
        DrawJewellerySuffixes(affixDatabase);
        
        EditorGUILayout.Space();
        DrawStatistics(affixDatabase);
    }
    
    private void DrawDatabaseManagement(AffixDatabase affixDatabase)
    {
        EditorGUILayout.LabelField("Database Management", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Add Physical Damage Affixes button
        if (GUILayout.Button("Add Physical Damage Affixes"))
        {
            affixDatabase.AddPhysicalDamageAffixes();
            EditorUtility.SetDirty(affixDatabase);
        }
        
        // Add Elemental Damage Affixes button
        if (GUILayout.Button("Add Elemental Damage Affixes"))
        {
            affixDatabase.AddElementalDamageAffixes();
            EditorUtility.SetDirty(affixDatabase);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Bulk Import Section
        EditorGUILayout.LabelField("Bulk Affix Import", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Import Affixes from Clipboard"))
        {
            ImportAffixesFromClipboard(affixDatabase);
        }
        
        if (GUILayout.Button("Show Import Format"))
        {
            ShowImportFormat();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Quick Remove Section
        EditorGUILayout.LabelField("Quick Remove", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Remove All Elemental"))
        {
            if (EditorUtility.DisplayDialog("Remove All Elemental", 
                "Are you sure you want to remove all Elemental categories from all affix types?", "Yes", "No"))
            {
                RemoveAllElementalCategories(affixDatabase);
                EditorUtility.SetDirty(affixDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("All Elemental categories removed!");
            }
        }
        
        if (GUILayout.Button("Remove All Physical"))
        {
            if (EditorUtility.DisplayDialog("Remove All Physical", 
                "Are you sure you want to remove all Physical categories from all affix types?", "Yes", "No"))
            {
                RemoveAllPhysicalCategories(affixDatabase);
                EditorUtility.SetDirty(affixDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("All Physical categories removed!");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Remove Empty Categories"))
        {
            RemoveEmptyCategories(affixDatabase);
            EditorUtility.SetDirty(affixDatabase);
            AssetDatabase.SaveAssets();
            Debug.Log("Empty categories removed!");
        }
        
        if (GUILayout.Button("Remove Duplicate Affixes"))
        {
            int removed = RemoveDuplicateAffixes(affixDatabase);
            EditorUtility.SetDirty(affixDatabase);
            AssetDatabase.SaveAssets();
            Debug.Log($"Removed {removed} duplicate affixes!");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Remove All Affixes", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Remove All Affixes", 
                "Are you sure you want to remove ALL affixes from the database? This action cannot be undone.", 
                "Yes, Remove All", "Cancel"))
            {
                RemoveAllAffixes(affixDatabase);
                EditorUtility.SetDirty(affixDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("All affixes removed from database!");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bulk Scope Management", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Set All Flat Damage to Local"))
        {
            if (EditorUtility.DisplayDialog("Set Flat Damage to Local", 
                "Set all 'Adds' damage modifiers to Local scope?", "Yes", "No"))
            {
                SetAllFlatDamageToLocal(affixDatabase);
                EditorUtility.SetDirty(affixDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("All flat damage modifiers set to Local scope!");
            }
        }
        
        if (GUILayout.Button("Set All Increased to Global"))
        {
            if (EditorUtility.DisplayDialog("Set Increased to Global", 
                "Set all 'increased' damage modifiers to Global scope?", "Yes", "No"))
            {
                SetAllIncreasedToGlobal(affixDatabase);
                EditorUtility.SetDirty(affixDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("All increased damage modifiers set to Global scope!");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Clear All Buttons
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Clear All Weapon Prefixes"))
        {
            if (EditorUtility.DisplayDialog("Clear Weapon Prefixes", 
                "Are you sure you want to clear all weapon prefixes?", "Yes", "No"))
            {
                affixDatabase.weaponPrefixCategories.Clear();
                EditorUtility.SetDirty(affixDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("All weapon prefixes cleared!");
            }
        }
        
        if (GUILayout.Button("Clear All Weapon Suffixes"))
        {
            if (EditorUtility.DisplayDialog("Clear Weapon Suffixes", 
                "Are you sure you want to clear all weapon suffixes?", "Yes", "No"))
            {
                affixDatabase.weaponSuffixCategories.Clear();
                EditorUtility.SetDirty(affixDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("All weapon suffixes cleared!");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Clear All Armour Prefixes"))
        {
            if (EditorUtility.DisplayDialog("Clear Armour Prefixes", 
                "Are you sure you want to clear all armour prefixes?", "Yes", "No"))
            {
                affixDatabase.armourPrefixCategories.Clear();
                EditorUtility.SetDirty(affixDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("All armour prefixes cleared!");
            }
        }
        
        if (GUILayout.Button("Clear All Armour Suffixes"))
        {
            if (EditorUtility.DisplayDialog("Clear Armour Suffixes", 
                "Are you sure you want to clear all armour suffixes?", "Yes", "No"))
            {
                affixDatabase.armourSuffixCategories.Clear();
                EditorUtility.SetDirty(affixDatabase);
                AssetDatabase.SaveAssets();
                Debug.Log("All armour suffixes cleared!");
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawWeaponPrefixes(AffixDatabase affixDatabase)
    {
        showWeaponPrefixes = EditorGUILayout.Foldout(showWeaponPrefixes, $"Weapon Prefixes ({GetTotalAffixCount(affixDatabase.weaponPrefixCategories)})", true);
        
        if (showWeaponPrefixes)
        {
            EditorGUI.indentLevel++;
            DrawAffixCategories(affixDatabase.weaponPrefixCategories, "WeaponPrefix");
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawWeaponSuffixes(AffixDatabase affixDatabase)
    {
        showWeaponSuffixes = EditorGUILayout.Foldout(showWeaponSuffixes, $"Weapon Suffixes ({GetTotalAffixCount(affixDatabase.weaponSuffixCategories)})", true);
        
        if (showWeaponSuffixes)
        {
            EditorGUI.indentLevel++;
            DrawAffixCategories(affixDatabase.weaponSuffixCategories, "WeaponSuffix");
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawArmourPrefixes(AffixDatabase affixDatabase)
    {
        showArmourPrefixes = EditorGUILayout.Foldout(showArmourPrefixes, $"Armour Prefixes ({GetTotalAffixCount(affixDatabase.armourPrefixCategories)})", true);
        
        if (showArmourPrefixes)
        {
            EditorGUI.indentLevel++;
            DrawAffixCategories(affixDatabase.armourPrefixCategories, "ArmourPrefix");
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawArmourSuffixes(AffixDatabase affixDatabase)
    {
        showArmourSuffixes = EditorGUILayout.Foldout(showArmourSuffixes, $"Armour Suffixes ({GetTotalAffixCount(affixDatabase.armourSuffixCategories)})", true);
        
        if (showArmourSuffixes)
        {
            EditorGUI.indentLevel++;
            DrawAffixCategories(affixDatabase.armourSuffixCategories, "ArmourSuffix");
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawJewelleryPrefixes(AffixDatabase affixDatabase)
    {
        showJewelleryPrefixes = EditorGUILayout.Foldout(showJewelleryPrefixes, $"Jewellery Prefixes ({GetTotalAffixCount(affixDatabase.jewelleryPrefixCategories)})", true);
        
        if (showJewelleryPrefixes)
        {
            EditorGUI.indentLevel++;
            DrawAffixCategories(affixDatabase.jewelleryPrefixCategories, "JewelleryPrefix");
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawJewellerySuffixes(AffixDatabase affixDatabase)
    {
        showJewellerySuffixes = EditorGUILayout.Foldout(showJewellerySuffixes, $"Jewellery Suffixes ({GetTotalAffixCount(affixDatabase.jewellerySuffixCategories)})", true);
        
        if (showJewellerySuffixes)
        {
            EditorGUI.indentLevel++;
            DrawAffixCategories(affixDatabase.jewellerySuffixCategories, "JewellerySuffix");
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawAffixCategories(List<AffixCategory> categories, string prefix)
    {
        if (categories == null || categories.Count == 0)
        {
            EditorGUILayout.LabelField("No categories", EditorStyles.miniLabel);
            return;
        }
        
        for (int i = 0; i < categories.Count; i++)
        {
            var category = categories[i];
            string categoryKey = $"{prefix}_{category.categoryName}";
            
            // Category header with count and remove button
            EditorGUILayout.BeginHorizontal();
            bool isExpanded = GetExpandedState(expandedCategories, categoryKey);
            isExpanded = EditorGUILayout.Foldout(isExpanded, $"{category.categoryName} ({GetTotalAffixCount(category)})", true);
            SetExpandedState(expandedCategories, categoryKey, isExpanded);
            
            // Remove category button
            if (GUILayout.Button("Remove Category", GUILayout.Width(120)))
            {
                if (EditorUtility.DisplayDialog("Remove Category", 
                    $"Are you sure you want to remove the '{category.categoryName}' category and all its contents?", "Yes", "No"))
                {
                    categories.RemoveAt(i);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Removed category: {category.categoryName}");
                    return; // Exit the loop since we modified the collection
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (isExpanded)
            {
                EditorGUI.indentLevel++;
                
                // Draw sub-categories
                if (category.subCategories != null)
                {
                    for (int j = 0; j < category.subCategories.Count; j++)
                    {
                        var subCategory = category.subCategories[j];
                        string subCategoryKey = $"{categoryKey}_{subCategory.subCategoryName}";
                        
                        EditorGUILayout.BeginHorizontal();
                        bool isSubExpanded = GetExpandedState(expandedSubCategories, subCategoryKey);
                        isSubExpanded = EditorGUILayout.Foldout(isSubExpanded, $"{subCategory.subCategoryName} ({subCategory.affixes.Count})", true);
                        SetExpandedState(expandedSubCategories, subCategoryKey, isSubExpanded);
                        
                        // Remove subcategory button
                        if (GUILayout.Button("Remove Sub", GUILayout.Width(100)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Subcategory", 
                                $"Are you sure you want to remove the '{subCategory.subCategoryName}' subcategory and all its affixes?", "Yes", "No"))
                            {
                                category.subCategories.RemoveAt(j);
                                EditorUtility.SetDirty(target);
                                AssetDatabase.SaveAssets();
                                Debug.Log($"Removed subcategory: {subCategory.subCategoryName}");
                                return; // Exit the loop since we modified the collection
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        if (isSubExpanded)
                        {
                            EditorGUI.indentLevel++;
                            
                            // Draw individual affixes
                            if (subCategory.affixes != null)
                            {
                                for (int k = 0; k < subCategory.affixes.Count; k++)
                                {
                                    var affix = subCategory.affixes[k];
                                    string affixKey = $"{subCategoryKey}_{affix.name}";
                                    
                                    EditorGUILayout.BeginHorizontal();
                                    bool isAffixExpanded = GetExpandedState(expandedAffixes, affixKey);
                                    isAffixExpanded = EditorGUILayout.Foldout(isAffixExpanded, $"{affix.name} (Tier {affix.tier})", true);
                                    SetExpandedState(expandedAffixes, affixKey, isAffixExpanded);
                                    
                                    // Remove affix button
                                    if (GUILayout.Button("Remove", GUILayout.Width(80)))
                                    {
                                        if (EditorUtility.DisplayDialog("Remove Affix", 
                                            $"Are you sure you want to remove the affix '{affix.name}'?", "Yes", "No"))
                                        {
                                            subCategory.affixes.RemoveAt(k);
                                            EditorUtility.SetDirty(target);
                                            AssetDatabase.SaveAssets();
                                            Debug.Log($"Removed affix: {affix.name}");
                                            return; // Exit the loop since we modified the collection
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    
                                    if (isAffixExpanded)
                                    {
                                        EditorGUI.indentLevel++;
                                        DrawAffixDetails(affix, categories, i, category, j, subCategory, k);
                                        EditorGUI.indentLevel--;
                                    }
                                }
                            }
                            
                            EditorGUI.indentLevel--;
                        }
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
    }
    
    private void DrawAffixDetails(Affix affix, List<AffixCategory> categories, int categoryIndex, AffixCategory category, int subCategoryIndex, AffixSubCategory subCategory, int affixIndex)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Basic affix information
        EditorGUILayout.LabelField("Affix Details", EditorStyles.boldLabel);
        
        // Name
        string newName = EditorGUILayout.TextField("Name", affix.name);
        if (newName != affix.name)
        {
            affix.name = newName;
            EditorUtility.SetDirty(target);
        }
        
        // Description
        string newDescription = EditorGUILayout.TextField("Description", affix.description);
        if (newDescription != affix.description)
        {
            affix.description = newDescription;
            EditorUtility.SetDirty(target);
        }
        
        // Tier
        AffixTier newTier = (AffixTier)EditorGUILayout.EnumPopup("Tier", affix.tier);
        if (newTier != affix.tier)
        {
            affix.tier = newTier;
            EditorUtility.SetDirty(target);
        }
        
        // Weight
        float newWeight = EditorGUILayout.FloatField("Weight", affix.weight);
        if (newWeight != affix.weight)
        {
            affix.weight = newWeight;
            EditorUtility.SetDirty(target);
        }
        
        // Affix Type
        AffixType newType = (AffixType)EditorGUILayout.EnumPopup("Type", affix.affixType);
        if (newType != affix.affixType)
        {
            affix.affixType = newType;
            EditorUtility.SetDirty(target);
        }
        
        EditorGUILayout.Space();
        
        // Modifiers
        EditorGUILayout.LabelField("Modifiers", EditorStyles.boldLabel);
        if (affix.modifiers != null)
        {
            for (int i = 0; i < affix.modifiers.Count; i++)
            {
                var modifier = affix.modifiers[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField($"Modifier {i + 1}", EditorStyles.boldLabel);
                
                // Stat Name
                string newStatName = EditorGUILayout.TextField("Stat Name", modifier.statName);
                if (newStatName != modifier.statName)
                {
                    modifier.statName = newStatName;
                    EditorUtility.SetDirty(target);
                }
                
                // Min/Max Values
                EditorGUILayout.BeginHorizontal();
                float newMinValue = EditorGUILayout.FloatField("Min Value", modifier.minValue);
                float newMaxValue = EditorGUILayout.FloatField("Max Value", modifier.maxValue);
                EditorGUILayout.EndHorizontal();
                
                if (newMinValue != modifier.minValue)
                {
                    modifier.minValue = newMinValue;
                    EditorUtility.SetDirty(target);
                }
                if (newMaxValue != modifier.maxValue)
                {
                    modifier.maxValue = newMaxValue;
                    EditorUtility.SetDirty(target);
                }
                
                // Modifier Type
                ModifierType newModType = (ModifierType)EditorGUILayout.EnumPopup("Modifier Type", modifier.modifierType);
                if (newModType != modifier.modifierType)
                {
                    modifier.modifierType = newModType;
                    EditorUtility.SetDirty(target);
                }
                
                // Damage Type
                DamageType newDamageType = (DamageType)EditorGUILayout.EnumPopup("Damage Type", modifier.damageType);
                if (newDamageType != modifier.damageType)
                {
                    modifier.damageType = newDamageType;
                    EditorUtility.SetDirty(target);
                }

                // Scope
                ModifierScope newScope = (ModifierScope)EditorGUILayout.EnumPopup("Scope", modifier.scope);
                if (newScope != modifier.scope)
                {
                    modifier.scope = newScope;
                    EditorUtility.SetDirty(target);
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        // Required Tags
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Required Tags", EditorStyles.boldLabel);
        if (affix.requiredTags != null)
        {
            for (int i = 0; i < affix.requiredTags.Count; i++)
            {
                string newTag = EditorGUILayout.TextField($"Tag {i + 1}", affix.requiredTags[i]);
                if (newTag != affix.requiredTags[i])
                {
                    affix.requiredTags[i] = newTag;
                    EditorUtility.SetDirty(target);
                }
            }
        }
        
        // Add/Remove buttons
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Add Modifier"))
        {
            affix.modifiers.Add(new AffixModifier("NewStat", 0, 0, ModifierType.Flat, ModifierScope.Global));
            EditorUtility.SetDirty(target);
        }
        
        if (GUILayout.Button("Add Tag"))
        {
            affix.requiredTags.Add("new_tag");
            EditorUtility.SetDirty(target);
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Toggle Scope button for each modifier
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Modifier Scope Management", EditorStyles.boldLabel);
        
        for (int i = 0; i < affix.modifiers.Count; i++)
        {
            var modifier = affix.modifiers[i];
            EditorGUILayout.BeginHorizontal();
            
            string scopeLabel = $"Modifier {i + 1} ({modifier.statName}): {modifier.scope}";
            EditorGUILayout.LabelField(scopeLabel, GUILayout.Width(200));
            
            string toggleText = modifier.scope == ModifierScope.Local ? "Set Global" : "Set Local";
            if (GUILayout.Button(toggleText, GUILayout.Width(100)))
            {
                modifier.scope = modifier.scope == ModifierScope.Local ? ModifierScope.Global : ModifierScope.Local;
                EditorUtility.SetDirty(target);
                Debug.Log($"Changed scope for {affix.name} modifier {i + 1} to {modifier.scope}");
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Remove Affix"))
        {
            if (EditorUtility.DisplayDialog("Remove Affix", 
                $"Are you sure you want to remove '{affix.name}'?", "Yes", "No"))
            {
                subCategory.affixes.RemoveAt(affixIndex);
                EditorUtility.SetDirty(target);
            }
        }
        
        if (GUILayout.Button("Duplicate Affix"))
        {
            var newAffix = new Affix(affix.name + "_Copy", affix.description, affix.affixType, affix.tier);
            newAffix.weight = affix.weight;
            newAffix.requiredTags = new List<string>(affix.requiredTags);
            newAffix.modifiers = new List<AffixModifier>();
            foreach (var mod in affix.modifiers)
            {
                newAffix.modifiers.Add(new AffixModifier(mod.statName, mod.minValue, mod.maxValue, mod.modifierType)
                {
                    damageType = mod.damageType,
                    scope = mod.scope
                });
            }
            subCategory.affixes.Add(newAffix);
            EditorUtility.SetDirty(target);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawStatistics(AffixDatabase affixDatabase)
    {
        EditorGUILayout.LabelField("Database Statistics", EditorStyles.boldLabel);
        
        // Weapon Statistics
        int weaponPrefixes = 0;
        int weaponSuffixes = 0;
        
        foreach (var category in affixDatabase.weaponPrefixCategories)
        {
            weaponPrefixes += category.GetAllAffixes().Count;
        }
        
        foreach (var category in affixDatabase.weaponSuffixCategories)
        {
            weaponSuffixes += category.GetAllAffixes().Count;
        }
        
        // Armour Statistics
        int armourPrefixes = 0;
        int armourSuffixes = 0;
        
        foreach (var category in affixDatabase.armourPrefixCategories)
        {
            armourPrefixes += category.GetAllAffixes().Count;
        }
        
        foreach (var category in affixDatabase.armourSuffixCategories)
        {
            armourSuffixes += category.GetAllAffixes().Count;
        }
        
        // Jewellery Statistics
        int jewelleryPrefixes = 0;
        int jewellerySuffixes = 0;
        
        foreach (var category in affixDatabase.jewelleryPrefixCategories)
        {
            jewelleryPrefixes += category.GetAllAffixes().Count;
        }
        
        foreach (var category in affixDatabase.jewellerySuffixCategories)
        {
            jewellerySuffixes += category.GetAllAffixes().Count;
        }
        
        // Calculate totals
        int totalPrefixes = weaponPrefixes + armourPrefixes + jewelleryPrefixes;
        int totalSuffixes = weaponSuffixes + armourSuffixes + jewellerySuffixes;
        int totalAffixes = totalPrefixes + totalSuffixes;
        
        // Display statistics
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Weapon Affixes", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"  Prefixes: {weaponPrefixes}");
        EditorGUILayout.LabelField($"  Suffixes: {weaponSuffixes}");
        EditorGUILayout.LabelField($"  Total: {weaponPrefixes + weaponSuffixes}");
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Armour Affixes", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"  Prefixes: {armourPrefixes}");
        EditorGUILayout.LabelField($"  Suffixes: {armourSuffixes}");
        EditorGUILayout.LabelField($"  Total: {armourPrefixes + armourSuffixes}");
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Jewellery Affixes", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"  Prefixes: {jewelleryPrefixes}");
        EditorGUILayout.LabelField($"  Suffixes: {jewellerySuffixes}");
        EditorGUILayout.LabelField($"  Total: {jewelleryPrefixes + jewellerySuffixes}");
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Overall Totals", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"  Total Prefixes: {totalPrefixes}");
        EditorGUILayout.LabelField($"  Total Suffixes: {totalSuffixes}");
        EditorGUILayout.LabelField($"  Total Affixes: {totalAffixes}");
        
        // Show category breakdown
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Category Breakdown", EditorStyles.boldLabel);
        
        // Weapon categories
        if (affixDatabase.weaponPrefixCategories.Count > 0 || affixDatabase.weaponSuffixCategories.Count > 0)
        {
            EditorGUILayout.LabelField("Weapon Categories:");
            foreach (var category in affixDatabase.weaponPrefixCategories)
            {
                int count = category.GetAllAffixes().Count;
                if (count > 0)
                {
                    EditorGUILayout.LabelField($"  {category.categoryName}: {count} prefixes");
                }
            }
            foreach (var category in affixDatabase.weaponSuffixCategories)
            {
                int count = category.GetAllAffixes().Count;
                if (count > 0)
                {
                    EditorGUILayout.LabelField($"  {category.categoryName}: {count} suffixes");
                }
            }
        }
        
        // Armour categories
        if (affixDatabase.armourPrefixCategories.Count > 0 || affixDatabase.armourSuffixCategories.Count > 0)
        {
            EditorGUILayout.LabelField("Armour Categories:");
            foreach (var category in affixDatabase.armourPrefixCategories)
            {
                int count = category.GetAllAffixes().Count;
                if (count > 0)
                {
                    EditorGUILayout.LabelField($"  {category.categoryName}: {count} prefixes");
                }
            }
            foreach (var category in affixDatabase.armourSuffixCategories)
            {
                int count = category.GetAllAffixes().Count;
                if (count > 0)
                {
                    EditorGUILayout.LabelField($"  {category.categoryName}: {count} suffixes");
                }
            }
        }
        
        // Jewellery categories
        if (affixDatabase.jewelleryPrefixCategories.Count > 0 || affixDatabase.jewellerySuffixCategories.Count > 0)
        {
            EditorGUILayout.LabelField("Jewellery Categories:");
            foreach (var category in affixDatabase.jewelleryPrefixCategories)
            {
                int count = category.GetAllAffixes().Count;
                if (count > 0)
                {
                    EditorGUILayout.LabelField($"  {category.categoryName}: {count} prefixes");
                }
            }
            foreach (var category in affixDatabase.jewellerySuffixCategories)
            {
                int count = category.GetAllAffixes().Count;
                if (count > 0)
                {
                    EditorGUILayout.LabelField($"  {category.categoryName}: {count} suffixes");
                }
            }
        }
    }
    
    private void ImportAffixesFromClipboard(AffixDatabase affixDatabase)
    {
        string clipboardText = GUIUtility.systemCopyBuffer;
        
        if (string.IsNullOrEmpty(clipboardText))
        {
            EditorUtility.DisplayDialog("Import Error", "Clipboard is empty! Please copy affix data first.", "OK");
            return;
        }
        
        var importedAffixes = ParseAffixData(clipboardText);
        
        if (importedAffixes.Count == 0)
        {
            EditorUtility.DisplayDialog("Import Error", "No valid affix data found in clipboard. Check the format.", "OK");
            return;
        }
        
        int successCount = 0;
        int errorCount = 0;
        int duplicateCount = 0;
        var errors = new List<string>();
        var duplicates = new List<string>();
        
        foreach (var affixData in importedAffixes)
        {
            try
            {
                if (IsDuplicateAffix(affixDatabase, affixData))
                {
                    duplicateCount++;
                    duplicates.Add($"{affixData.name} ({affixData.affixType})");
                    continue;
                }
                
                if (CreateAffixFromData(affixDatabase, affixData))
                {
                    successCount++;
                }
                else
                {
                    errorCount++;
                    errors.Add($"Failed to create affix: {affixData.name}");
                }
            }
            catch (System.Exception e)
            {
                errorCount++;
                errors.Add($"Error creating {affixData.name}: {e.Message}");
            }
        }
        
        EditorUtility.SetDirty(affixDatabase);
        
        string resultMessage = $"Import Complete!\n\nSuccessfully imported: {successCount} affixes";
        
        if (duplicateCount > 0)
        {
            resultMessage += $"\nSkipped duplicates: {duplicateCount}";
        }
        
        if (errorCount > 0)
        {
            resultMessage += $"\nErrors: {errorCount}";
        }
        
        if (duplicateCount > 0)
        {
            resultMessage += "\n\nSkipped duplicates:\n" + string.Join("\n", duplicates);
        }
        
        if (errorCount > 0)
        {
            resultMessage += "\n\nError details:\n" + string.Join("\n", errors);
        }
        
        EditorUtility.DisplayDialog("Import Results", resultMessage, "OK");
        
        Debug.Log($"Imported {successCount} affixes from clipboard. Skipped {duplicateCount} duplicates. {errorCount} errors occurred.");
    }
    
    private void ShowImportFormat()
    {
        string formatExample = @"Expected Format:
Affix Slot	Name	Item Level	Stat	Tags	Weapon Types	Handedness	Scope

Example:
Suffix	of Skill	1	(5-7)% increased Attack Speed	Attack, Speed	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger	OneHand	Local
Prefix	Humming	3	Adds 1 to (5-6) Lightning Damage	Damage, Elemental, Lightning, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger	TwoHand	Global

Format Rules:
- Tab-separated columns
- Affix Slot: Prefix or Suffix
- Name: Affix name
- Item Level: Required item level (number)
- Stat: The stat description
- Tags: Comma-separated tags
- Weapon Types: Comma-separated weapon types
- Handedness: OneHand or TwoHand
- Scope: Local or Global (optional, defaults to Local)

Supported Weapon Types:
Sword, Axe, Mace, Dagger, Claw, RitualDagger, Bow, Wand, Staff, Sceptre

Supported Tags:
weapon, attack, melee, ranged, spell, damage, elemental, physical, fire, cold, lightning, chaos, speed, etc.";

        EditorUtility.DisplayDialog("Import Format", formatExample, "OK");
    }
    
    private class AffixImportData
    {
        public AffixType affixType;
        public string name;
        public int itemLevel;
        public string statDescription;
        public string handedness;
        public List<string> tags;
        public List<string> weaponTypes;
        public string scope;
    }
    
    private List<AffixImportData> ParseAffixData(string clipboardText)
    {
        var affixes = new List<AffixImportData>();
        var lines = clipboardText.Split('\n');
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine)) continue;
            
            // Skip header lines
            if (trimmedLine.Contains("Affix Slot") || trimmedLine.Contains("---")) continue;
            
            var columns = trimmedLine.Split('\t');
            if (columns.Length < 7) continue; // Now expecting 7 columns
            
            try
            {
                var affixData = new AffixImportData();
                
                // Parse Affix Slot
                string slotText = columns[0].Trim().ToLower();
                affixData.affixType = slotText == "prefix" ? AffixType.Prefix : AffixType.Suffix;
                
                // Parse Name
                affixData.name = columns[1].Trim();
                
                // Parse Item Level
                if (!int.TryParse(columns[2].Trim(), out affixData.itemLevel))
                {
                    Debug.LogWarning($"Invalid item level for {affixData.name}: {columns[2]}");
                    continue;
                }
                
                // Parse Stat Description
                affixData.statDescription = columns[3].Trim();
                
                // Parse Handedness
                affixData.handedness = columns[4].Trim();
                
                // Parse Tags
                affixData.tags = ParseCommaSeparatedList(columns[5]);
                
                // Parse Weapon Types
                affixData.weaponTypes = ParseCommaSeparatedList(columns[6]);
                
                // Parse Scope (if present)
                affixData.scope = columns.Length > 7 ? columns[7].Trim() : "Local";
                
                affixes.Add(affixData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing line: {trimmedLine}\nError: {e.Message}");
            }
        }
        
        return affixes;
    }
    
    private List<string> ParseCommaSeparatedList(string input)
    {
        if (string.IsNullOrEmpty(input)) return new List<string>();
        
        return input.Split(',')
                   .Select(s => s.Trim().ToLower())
                   .Where(s => !string.IsNullOrEmpty(s))
                   .ToList();
    }
    
    private bool CreateAffixFromData(AffixDatabase affixDatabase, AffixImportData data)
    {
        // Check for duplicates before creating (now includes handedness)
        if (IsDuplicateAffix(affixDatabase, data))
        {
            Debug.Log($"Skipping duplicate affix: {data.name} ({data.affixType}) with handedness {data.handedness}");
            return false;
        }
        
        // Determine tier based on item level
        AffixTier tier = DetermineTierFromLevel(data.itemLevel);
        
        // Create the affix
        Affix affix = new Affix(data.name, data.statDescription, data.affixType, tier);
        
        // Parse the stat description to create modifiers
        ParseStatDescription(affix, data.statDescription);
        
        // Set handedness
        switch (data.handedness.ToLower())
        {
            case "onehand":
                affix.handedness = Handedness.OneHand;
                break;
            case "twohand":
                affix.handedness = Handedness.TwoHand;
                break;
            case "both":
            default:
                affix.handedness = Handedness.Both;
                break;
        }
        
        // Set required tags based on weapon types
        affix.requiredTags = new List<string> { "weapon" };
        
        // Add weapon type tags
        foreach (var weaponType in data.weaponTypes)
        {
            if (IsValidWeaponType(weaponType))
            {
                affix.requiredTags.Add(weaponType);
            }
        }
        
        // Add additional tags from the tags column
        foreach (var tag in data.tags)
        {
            if (!affix.requiredTags.Contains(tag))
            {
                affix.requiredTags.Add(tag);
            }
        }
        
        // Determine which category list to use based on affix type and item types
        var categoryList = DetermineCategoryList(affixDatabase, data);
        if (categoryList == null)
        {
            Debug.LogError($"Could not determine category list for affix: {data.name}");
            return false;
        }
        
        // Add to appropriate category
        string categoryName = DetermineCategoryName(data.tags, affix.handedness);
        string subCategoryName = DetermineSubCategoryName(data.tags, data.statDescription, affix.handedness);
        
        var category = GetOrCreateCategory(categoryList, categoryName);
        var subCategory = GetOrCreateSubCategory(category, subCategoryName);
        subCategory.affixes.Add(affix);
        
        return true;
    }
    
    private bool IsDuplicateAffix(AffixDatabase affixDatabase, AffixImportData data)
    {
        // Parse handedness for comparison
        Handedness dataHandedness = Handedness.Both;
        switch (data.handedness.ToLower())
        {
            case "onehand":
                dataHandedness = Handedness.OneHand;
                break;
            case "twohand":
                dataHandedness = Handedness.TwoHand;
                break;
            case "both":
            default:
                dataHandedness = Handedness.Both;
                break;
        }
        
        // Determine which category list to check based on item types
        var categoryList = DetermineCategoryList(affixDatabase, data);
        if (categoryList == null) return false;
        
        // Check all categories in the appropriate list
        foreach (var category in categoryList)
        {
            foreach (var subCategory in category.subCategories)
            {
                foreach (var existingAffix in subCategory.affixes)
                {
                    // Check for exact duplicates: same name, type, handedness, AND description
                    if (existingAffix.name == data.name && 
                        existingAffix.affixType == data.affixType && 
                        existingAffix.handedness == dataHandedness &&
                        existingAffix.description == data.statDescription)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    private AffixTier DetermineTierFromLevel(int itemLevel)
    {
        if (itemLevel <= 5) return AffixTier.Tier9;
        if (itemLevel <= 12) return AffixTier.Tier8;
        if (itemLevel <= 20) return AffixTier.Tier7;
        if (itemLevel <= 28) return AffixTier.Tier6;
        if (itemLevel <= 35) return AffixTier.Tier5;
        if (itemLevel <= 45) return AffixTier.Tier4;
        if (itemLevel <= 55) return AffixTier.Tier3;
        if (itemLevel <= 65) return AffixTier.Tier2;
        return AffixTier.Tier1;
    }
    
    private void ParseStatDescription(Affix affix, string description)
    {
        // Store the description as-is
        affix.description = description;
        
        // Extract range values from the description
        var range = ExtractDamageRange(description);
        
        // Set the affix's range values
        affix.minValue = range.min;
        affix.maxValue = range.max;
        affix.hasRange = (range.min > 0 || range.max > 0);
        
        // Roll a single value from the range for immediate use
        if (affix.hasRange)
        {
            affix.RollAffix();
            Debug.Log($"Rolled affix '{affix.name}': Range {range.min}-{range.max} -> Rolled value: {affix.rolledValue}");
        }
        
        // Determine damage type from description
        DamageType damageType = DetermineDamageType(description);
        
        // Determine modifier type from description
        ModifierType modifierType = DetermineModifierType(description);
        
        // Determine scope based on damage type and description
        ModifierScope scope = DetermineModifierScope(description, damageType);
        
        // Determine stat name based on damage type
        string statName = DetermineStatName(description, damageType);
        
        // Create a modifier with the extracted range and proper settings
        var modifier = new AffixModifier(statName, range.min, range.max, modifierType, scope);
        modifier.description = description;
        modifier.damageType = damageType;
        affix.modifiers.Add(modifier);
        
        // Debug logging to verify the fix
        Debug.Log($"Created modifier for '{affix.name}':");
        Debug.Log($"  Stat Name: {statName}");
        Debug.Log($"  Min Value: {range.min}, Max Value: {range.max}");
        Debug.Log($"  Modifier Type: {modifierType}");
        Debug.Log($"  Damage Type: {damageType}");
        Debug.Log($"  Scope: {scope} (Flat=Local, Increased Physical=Local, Increased Others=Global)");
        Debug.Log($"  Description: {description}");
    }
    
    /// <summary>
    /// Extracts damage range from descriptions like:
    /// - "Adds 1 to (2-3) Physical Damage" -> min=1, max=3
    /// - "Adds (16-21) to (32-38) Physical Damage" -> min=16, max=38
    /// - "Adds (5-7) Physical Damage" -> min=5, max=7
    /// - "(40-54)% increased Lightning Damage" -> min=40, max=54
    /// </summary>
    private (int min, int max) ExtractDamageRange(string description)
    {
        Debug.Log($"Extracting damage range from: '{description}'");
        
        // Pattern 1: "Adds (X-Y) to (Z-W) Damage" - dual range (MUST come before simple range)
        var dualRangeMatch = System.Text.RegularExpressions.Regex.Match(description, @"Adds\s+\((\d+)-(\d+)\)\s+to\s+\((\d+)-(\d+)\)");
        if (dualRangeMatch.Success)
        {
            int firstMin = int.Parse(dualRangeMatch.Groups[1].Value);
            int firstMax = int.Parse(dualRangeMatch.Groups[2].Value);
            int secondMin = int.Parse(dualRangeMatch.Groups[3].Value);
            int secondMax = int.Parse(dualRangeMatch.Groups[4].Value);
            
            // For dual ranges, min can be anywhere in first range, max can be anywhere in second range
            Debug.Log($"Found dual range: first({firstMin}-{firstMax}) to second({secondMin}-{secondMax}) -> overall({firstMin}-{secondMax})");
            return (firstMin, secondMax);
        }
        
        // Pattern 2: "Adds X to (Y-Z) Damage" - single range
        var singleRangeMatch = System.Text.RegularExpressions.Regex.Match(description, @"Adds\s+(\d+)\s+to\s+\((\d+)-(\d+)\)");
        if (singleRangeMatch.Success)
        {
            int min = int.Parse(singleRangeMatch.Groups[1].Value);
            int max = int.Parse(singleRangeMatch.Groups[3].Value);
            Debug.Log($"Found single range: {min}-{max}");
            return (min, max);
        }
        
        // Pattern 3: "Adds (X-Y) Damage" - simple range
        var simpleRangeMatch = System.Text.RegularExpressions.Regex.Match(description, @"Adds\s+\((\d+)-(\d+)\)");
        if (simpleRangeMatch.Success)
        {
            int min = int.Parse(simpleRangeMatch.Groups[1].Value);
            int max = int.Parse(simpleRangeMatch.Groups[2].Value);
            Debug.Log($"Found simple range: {min}-{max}");
            return (min, max);
        }
        
        // Pattern 4: "Adds X to Y Damage" - fixed values
        var fixedMatch = System.Text.RegularExpressions.Regex.Match(description, @"Adds\s+(\d+)\s+to\s+(\d+)");
        if (fixedMatch.Success)
        {
            int min = int.Parse(fixedMatch.Groups[1].Value);
            int max = int.Parse(fixedMatch.Groups[2].Value);
            Debug.Log($"Found fixed range: {min}-{max}");
            return (min, max);
        }
        
        // Pattern 5: "(X-Y)% increased Damage" - percentage range
        var increasedMatch = System.Text.RegularExpressions.Regex.Match(description, @"\((\d+)-(\d+)\)%\s+increased");
        if (increasedMatch.Success)
        {
            int min = int.Parse(increasedMatch.Groups[1].Value);
            int max = int.Parse(increasedMatch.Groups[2].Value);
            Debug.Log($"Found increased percentage range: {min}-{max}%");
            return (min, max);
        }
        
        // Pattern 6: "+X-Y% chance to [Status] on Hit" - status effect chance (e.g., "+5-7% chance to Shock on Hit")
        var statusChanceMatch = System.Text.RegularExpressions.Regex.Match(description, @"\+(\d+)-(\d+)%\s+chance\s+to\s+(?:apply\s+)?(\w+)\s+on\s+Hit", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (statusChanceMatch.Success)
        {
            int min = int.Parse(statusChanceMatch.Groups[1].Value);
            int max = int.Parse(statusChanceMatch.Groups[2].Value);
            Debug.Log($"Found status effect chance range: {min}-{max}% (effect: {statusChanceMatch.Groups[3].Value})");
            return (min, max);
        }
        
        // Pattern 7: "+X-Y% Chance to [Status]" - alternative format (e.g., "+5-7% Chance to Shock")
        var statusChanceAltMatch = System.Text.RegularExpressions.Regex.Match(description, @"\+(\d+)-(\d+)%\s+Chance\s+to\s+(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (statusChanceAltMatch.Success)
        {
            int min = int.Parse(statusChanceAltMatch.Groups[1].Value);
            int max = int.Parse(statusChanceAltMatch.Groups[2].Value);
            Debug.Log($"Found status effect chance range (alt format): {min}-{max}% (effect: {statusChanceAltMatch.Groups[3].Value})");
            return (min, max);
        }
        
        // Pattern 8: "+X-Y% chance to cause [Status]" - another format (e.g., "+2-3% chance to cause Bleeding")
        var statusCauseMatch = System.Text.RegularExpressions.Regex.Match(description, @"\+(\d+)-(\d+)%\s+chance\s+to\s+cause\s+(\w+)\s+on\s+Hit", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (statusCauseMatch.Success)
        {
            int min = int.Parse(statusCauseMatch.Groups[1].Value);
            int max = int.Parse(statusCauseMatch.Groups[2].Value);
            Debug.Log($"Found status effect chance range (cause format): {min}-{max}% (effect: {statusCauseMatch.Groups[3].Value})");
            return (min, max);
        }
        
        // No range found, return default values
        Debug.LogWarning($"No range found in description: '{description}'");
        return (0, 0);
    }
    
    /// <summary>
    /// Determines the damage type from the description
    /// </summary>
    private DamageType DetermineDamageType(string description)
    {
        string descLower = description.ToLower();
        
        if (descLower.Contains("physical damage"))
            return DamageType.Physical;
        if (descLower.Contains("fire damage"))
            return DamageType.Fire;
        if (descLower.Contains("cold damage"))
            return DamageType.Cold;
        if (descLower.Contains("lightning damage"))
            return DamageType.Lightning;
        if (descLower.Contains("chaos damage"))
            return DamageType.Chaos;
        
        return DamageType.None;
    }
    
    /// <summary>
    /// Determines the modifier type from the description
    /// </summary>
    private ModifierType DetermineModifierType(string description)
    {
        string descLower = description.ToLower();
        
        if (descLower.Contains("increased"))
            return ModifierType.Increased;
        if (descLower.Contains("more"))
            return ModifierType.More;
        if (descLower.Contains("reduced"))
            return ModifierType.Reduced;
        if (descLower.Contains("less"))
            return ModifierType.Less;
        
        // Default to Flat for "Adds" modifiers
        return ModifierType.Flat;
    }
    
    /// <summary>
    /// Determines the modifier scope based on damage type and description
    /// </summary>
    private ModifierScope DetermineModifierScope(string description, DamageType damageType)
    {
        string descLower = description.ToLower();
        
        // Flat damage modifiers should always be Local (weapon-specific)
        if (descLower.Contains("adds") || descLower.Contains("added"))
            return ModifierScope.Local;
        
        // For "increased" modifiers:
        if (descLower.Contains("increased"))
        {
            // Physical damage increased modifiers should be Local (weapon-specific)
            if (damageType == DamageType.Physical)
                return ModifierScope.Local;
            
            // Non-Physical damage increased modifiers should be Global (character-wide)
            if (damageType != DamageType.None)
                return ModifierScope.Global;
        }
        
        // Check for other local modifiers
        if (descLower.Contains("attack speed") || descLower.Contains("accuracy") || 
            descLower.Contains("critical") || descLower.Contains("weapon"))
            return ModifierScope.Local;
        
        // Default to Global for other stats
        return ModifierScope.Global;
    }
    
    /// <summary>
    /// Determines the stat name based on damage type and description
    /// </summary>
    private string DetermineStatName(string description, DamageType damageType)
    {
        string descLower = description.ToLower();
        
        // Handle increased damage modifiers
        if (descLower.Contains("increased") && damageType != DamageType.None)
        {
            return $"Increase{damageType}Damage";
        }
        
        // Handle flat damage modifiers
        if (damageType != DamageType.None)
        {
            return $"{damageType}Damage";
        }
        
        // Check for other specific stats
        if (descLower.Contains("attack speed"))
            return "AttackSpeed";
        if (descLower.Contains("accuracy"))
            return "AccuracyRating";
        if (descLower.Contains("critical"))
            return "CriticalStrikeChance";
        
        return "CustomStat";
    }
    

    
    private string DetermineCategoryName(List<string> tags, Handedness handedness)
    {
        // Check for handedness tags first
        if (handedness == Handedness.OneHand) return "OneHand";
        if (handedness == Handedness.TwoHand) return "TwoHand";
        
        // Then check for damage type tags
        if (tags.Contains("physical")) return "Physical";
        if (tags.Contains("fire") || tags.Contains("cold") || tags.Contains("lightning") || tags.Contains("chaos")) return "Elemental";
        if (tags.Contains("speed")) return "Speed";
        if (tags.Contains("attack")) return "Attack";
        if (tags.Contains("spell")) return "Spell";
        
        return "General";
    }
    
    private string DetermineSubCategoryName(List<string> tags, string description, Handedness handedness)
    {
        // Check if this is a handedness-based category
        bool isOneHand = handedness == Handedness.OneHand;
        bool isTwoHand = handedness == Handedness.TwoHand;
        
        // Determine the elemental type first
        string elementalType = "";
        if (tags.Contains("fire") || description.Contains("Fire Damage"))
            elementalType = "Fire";
        else if (tags.Contains("cold") || description.Contains("Cold Damage"))
            elementalType = "Cold";
        else if (tags.Contains("lightning") || description.Contains("Lightning Damage"))
            elementalType = "Lightning";
        else if (tags.Contains("chaos") || description.Contains("Chaos Damage"))
            elementalType = "Chaos";
        else if (tags.Contains("physical") || description.Contains("Physical Damage"))
            elementalType = "Physical";
        
        // Determine the modifier type
        string modifierType = "";
        if (description.Contains("increased"))
            modifierType = "Increased";
        else if (description.Contains("added") || description.Contains("adds"))
            modifierType = "Flat";
        else if (description.Contains("more"))
            modifierType = "More";
        else if (description.Contains("reduced"))
            modifierType = "Reduced";
        else if (description.Contains("less"))
            modifierType = "Less";
        
        // For handedness categories, combine elemental type with modifier type
        if (isOneHand || isTwoHand)
        {
            if (!string.IsNullOrEmpty(elementalType) && !string.IsNullOrEmpty(modifierType))
            {
                return $"{elementalType} {modifierType}";
            }
            else if (!string.IsNullOrEmpty(elementalType))
            {
                return elementalType;
            }
            else if (!string.IsNullOrEmpty(modifierType))
            {
                return modifierType;
            }
            else
            {
                return "General";
            }
        }
        
        // For non-handedness categories, use the existing logic
        // Combine elemental type with modifier type for granular categorization
        if (!string.IsNullOrEmpty(elementalType) && !string.IsNullOrEmpty(modifierType))
        {
            return $"{elementalType} {modifierType}";
        }
        else if (!string.IsNullOrEmpty(elementalType))
        {
            return elementalType;
        }
        else if (!string.IsNullOrEmpty(modifierType))
        {
            return modifierType;
        }
        
        return "Other";
    }
    
    private bool IsValidWeaponType(string weaponType)
    {
        var validTypes = new List<string> 
        { 
            "sword", "axe", "mace", "dagger", "claw", "ritualdagger", 
            "bow", "wand", "staff", "sceptre" 
        };
        
        return validTypes.Contains(weaponType.ToLower());
    }
    
    private AffixCategory GetOrCreateCategory(List<AffixCategory> categories, string categoryName)
    {
        var category = categories.Find(c => c.categoryName == categoryName);
        if (category == null)
        {
            category = new AffixCategory(categoryName);
            categories.Add(category);
        }
        return category;
    }
    
    private AffixSubCategory GetOrCreateSubCategory(AffixCategory category, string subCategoryName)
    {
        var subCategory = category.subCategories.Find(sc => sc.subCategoryName == subCategoryName);
        if (subCategory == null)
        {
            subCategory = new AffixSubCategory(subCategoryName);
            category.subCategories.Add(subCategory);
        }
        return subCategory;
    }
    
    private int GetTotalAffixCount(List<AffixCategory> categories)
    {
        if (categories == null) return 0;
        
        int total = 0;
        foreach (var category in categories)
        {
            total += GetTotalAffixCount(category);
        }
        return total;
    }
    
    private int GetTotalAffixCount(AffixCategory category)
    {
        if (category == null || category.subCategories == null) return 0;
        
        int total = 0;
        foreach (var subCategory in category.subCategories)
        {
            if (subCategory.affixes != null)
            {
                total += subCategory.affixes.Count;
            }
        }
        return total;
    }
    
    private bool GetExpandedState(Dictionary<string, bool> dict, string key)
    {
        if (dict.ContainsKey(key))
        {
            return dict[key];
        }
        return false;
    }
    
    private void SetExpandedState(Dictionary<string, bool> dict, string key, bool value)
    {
        dict[key] = value;
    }

    private void RemoveAllElementalCategories(AffixDatabase affixDatabase)
    {
        RemoveCategoriesOfType(affixDatabase.weaponPrefixCategories, "Elemental");
        RemoveCategoriesOfType(affixDatabase.weaponSuffixCategories, "Elemental");
        RemoveCategoriesOfType(affixDatabase.armourPrefixCategories, "Elemental");
        RemoveCategoriesOfType(affixDatabase.armourSuffixCategories, "Elemental");
        RemoveCategoriesOfType(affixDatabase.jewelleryPrefixCategories, "Elemental");
        RemoveCategoriesOfType(affixDatabase.jewellerySuffixCategories, "Elemental");
    }

    private void RemoveAllPhysicalCategories(AffixDatabase affixDatabase)
    {
        RemoveCategoriesOfType(affixDatabase.weaponPrefixCategories, "Physical");
        RemoveCategoriesOfType(affixDatabase.weaponSuffixCategories, "Physical");
        RemoveCategoriesOfType(affixDatabase.armourPrefixCategories, "Physical");
        RemoveCategoriesOfType(affixDatabase.armourSuffixCategories, "Physical");
        RemoveCategoriesOfType(affixDatabase.jewelleryPrefixCategories, "Physical");
        RemoveCategoriesOfType(affixDatabase.jewellerySuffixCategories, "Physical");
    }

    private void RemoveCategoriesOfType(List<AffixCategory> categories, string type)
    {
        categories.RemoveAll(c => c.categoryName.ToLower().Contains(type));
    }

    private void RemoveEmptyCategories(AffixDatabase affixDatabase)
    {
        RemoveEmptyCategoriesOfType(affixDatabase.weaponPrefixCategories);
        RemoveEmptyCategoriesOfType(affixDatabase.weaponSuffixCategories);
        RemoveEmptyCategoriesOfType(affixDatabase.armourPrefixCategories);
        RemoveEmptyCategoriesOfType(affixDatabase.armourSuffixCategories);
        RemoveEmptyCategoriesOfType(affixDatabase.jewelleryPrefixCategories);
        RemoveEmptyCategoriesOfType(affixDatabase.jewellerySuffixCategories);
    }

    private void RemoveEmptyCategoriesOfType(List<AffixCategory> categories)
    {
        categories.RemoveAll(c => c.subCategories == null || c.subCategories.Count == 0);
    }

    private int RemoveDuplicateAffixes(AffixDatabase affixDatabase)
    {
        int removed = 0;
        var seenAffixes = new HashSet<string>();

        foreach (var category in affixDatabase.weaponPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                subCategory.affixes.RemoveAll(affix =>
                {
                    string affixKey = $"{category.categoryName}_{subCategory.subCategoryName}_{affix.name}";
                    if (seenAffixes.Contains(affixKey))
                    {
                        removed++;
                        return true; // Remove the duplicate
                    }
                    seenAffixes.Add(affixKey);
                    return false; // Keep the original
                });
            }
        }

        foreach (var category in affixDatabase.weaponSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                subCategory.affixes.RemoveAll(affix =>
                {
                    string affixKey = $"{category.categoryName}_{subCategory.subCategoryName}_{affix.name}";
                    if (seenAffixes.Contains(affixKey))
                    {
                        removed++;
                        return true; // Remove the duplicate
                    }
                    seenAffixes.Add(affixKey);
                    return false; // Keep the original
                });
            }
        }

        return removed;
    }
    
    private void RemoveAllAffixes(AffixDatabase affixDatabase)
    {
        // Clear all weapon categories
        foreach (var category in affixDatabase.weaponPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                subCategory.affixes.Clear();
            }
        }
        
        foreach (var category in affixDatabase.weaponSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                subCategory.affixes.Clear();
            }
        }
        
        // Clear all armour categories
        foreach (var category in affixDatabase.armourPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                subCategory.affixes.Clear();
            }
        }
        
        foreach (var category in affixDatabase.armourSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                subCategory.affixes.Clear();
            }
        }
        
        // Clear all jewellery categories
        foreach (var category in affixDatabase.jewelleryPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                subCategory.affixes.Clear();
            }
        }
        
        foreach (var category in affixDatabase.jewellerySuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                subCategory.affixes.Clear();
            }
        }
    }
    
    private void SetAllFlatDamageToLocal(AffixDatabase affixDatabase)
    {
        int changedCount = 0;
        
        // Process all category lists
        var allCategories = new List<List<AffixCategory>>
        {
            affixDatabase.weaponPrefixCategories,
            affixDatabase.weaponSuffixCategories,
            affixDatabase.armourPrefixCategories,
            affixDatabase.armourSuffixCategories,
            affixDatabase.jewelleryPrefixCategories,
            affixDatabase.jewellerySuffixCategories
        };
        
        foreach (var categoryList in allCategories)
        {
            foreach (var category in categoryList)
            {
                foreach (var subCategory in category.subCategories)
                {
                    foreach (var affix in subCategory.affixes)
                    {
                        foreach (var modifier in affix.modifiers)
                        {
                            if (modifier.modifierType == ModifierType.Flat && 
                                (affix.description.ToLower().Contains("adds") || affix.description.ToLower().Contains("added")))
                            {
                                modifier.scope = ModifierScope.Local;
                                changedCount++;
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log($"Set {changedCount} flat damage modifiers to Local scope");
    }
    
    private void SetAllIncreasedToGlobal(AffixDatabase affixDatabase)
    {
        int changedCount = 0;
        
        // Process all category lists
        var allCategories = new List<List<AffixCategory>>
        {
            affixDatabase.weaponPrefixCategories,
            affixDatabase.weaponSuffixCategories,
            affixDatabase.armourPrefixCategories,
            affixDatabase.armourSuffixCategories,
            affixDatabase.jewelleryPrefixCategories,
            affixDatabase.jewellerySuffixCategories
        };
        
        foreach (var categoryList in allCategories)
        {
            foreach (var category in categoryList)
            {
                foreach (var subCategory in category.subCategories)
                {
                    foreach (var affix in subCategory.affixes)
                    {
                        foreach (var modifier in affix.modifiers)
                        {
                            if (modifier.modifierType == ModifierType.Increased && 
                                affix.description.ToLower().Contains("increased"))
                            {
                                // Skip Physical damage - it should stay Local
                                if (modifier.damageType != DamageType.Physical)
                                {
                                    modifier.scope = ModifierScope.Global;
                                    changedCount++;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log($"Set {changedCount} increased damage modifiers to Global scope (excluding Physical)");
    }
    
    /// <summary>
    /// Determines which category list to use based on affix type and item types
    /// </summary>
    private List<AffixCategory> DetermineCategoryList(AffixDatabase affixDatabase, AffixImportData data)
    {
        // Check if this is an armour affix based on item types
        if (IsArmourAffix(data.weaponTypes))
        {
            return data.affixType == AffixType.Prefix ? 
                affixDatabase.armourPrefixCategories : 
                affixDatabase.armourSuffixCategories;
        }
        
        // Check if this is a jewellery affix based on item types
        if (IsJewelleryAffix(data.weaponTypes))
        {
            return data.affixType == AffixType.Prefix ? 
                affixDatabase.jewelleryPrefixCategories : 
                affixDatabase.jewellerySuffixCategories;
        }
        
        // Check if this is an attribute affix based on tags
        if (IsAttributeAffix(data.tags))
        {
            return data.affixType == AffixType.Prefix ? 
                affixDatabase.jewelleryPrefixCategories : 
                affixDatabase.jewellerySuffixCategories;
        }
        
        // Default to weapon categories
        return data.affixType == AffixType.Prefix ? 
            affixDatabase.weaponPrefixCategories : 
            affixDatabase.weaponSuffixCategories;
    }
    
    /// <summary>
    /// Checks if an affix is an armour affix based on item types
    /// </summary>
    private bool IsArmourAffix(List<string> itemTypes)
    {
        if (itemTypes == null) return false;
        
        foreach (string itemType in itemTypes)
        {
            string typeLower = itemType.ToLower();
            if (typeLower.Contains("_armour") || typeLower.Contains("body_armour") || 
                typeLower.Contains("boots") || typeLower.Contains("gloves") || 
                typeLower.Contains("helmet") || typeLower.Contains("shield") ||
                typeLower.Contains("armour"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if an affix is a jewellery affix based on item types
    /// </summary>
    private bool IsJewelleryAffix(List<string> itemTypes)
    {
        if (itemTypes == null) return false;
        
        foreach (string itemType in itemTypes)
        {
            string typeLower = itemType.ToLower();
            if (typeLower.Contains("ring") || typeLower.Contains("amulet") || typeLower.Contains("belt"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if an affix is an attribute affix based on tags
    /// </summary>
    private bool IsAttributeAffix(List<string> tags)
    {
        if (tags == null) return false;
        
        foreach (string tag in tags)
        {
            string tagLower = tag.ToLower();
            if (tagLower.Contains("attribute") || tagLower.Contains("strength") || 
                tagLower.Contains("dexterity") || tagLower.Contains("intelligence"))
            {
                return true;
            }
        }
        
        return false;
    }
}
