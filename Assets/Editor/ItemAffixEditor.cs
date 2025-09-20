using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;

[CustomEditor(typeof(BaseItem), true)]
public class ItemAffixEditor : Editor
{
    private bool showAffixManagement = true;
    private bool showImplicitModifiers = true;
    private bool showPrefixes = true;
    private bool showSuffixes = true;
    private bool showDescription = false; // Description is minimized by default
    
    // Affix selection state
    private bool showAffixSelector = false;
    private string selectedCategory = "";
    private string selectedSubCategory = "";
    private AffixType selectedAffixType = AffixType.Prefix;
    private List<Affix> availableAffixes = new List<Affix>();
    
    // Manual Affix Creation State
    private string manualAffixName = "";
    private string manualStatName = "";
    private int manualMinValue = 1;
    private int manualMaxValue = 10;
    private float manualSelectedValue = 5;
    private AffixType selectedManualAffixType = AffixType.Prefix;
    private ModifierType selectedManualModifierType = ModifierType.Flat;
    private ModifierScope selectedManualScope = ModifierScope.Local;
    private DamageType selectedManualDamageType = DamageType.None;
    
    public override void OnInspectorGUI()
    {
        BaseItem item = (BaseItem)target;
        
        // Draw custom inspector with foldout description
        DrawCustomInspector(item);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Affix Management", EditorStyles.boldLabel);
        
        // Affix Management Section
        showAffixManagement = EditorGUILayout.Foldout(showAffixManagement, "Affix Management", true);
        
        if (showAffixManagement)
        {
            EditorGUI.indentLevel++;
            
            // Quick affix generation
            DrawQuickAffixGeneration(item);
            
            EditorGUILayout.Space();
            
            // Manual affix selection
            DrawAffixSelector(item);
            
            EditorGUILayout.Space();
            
            // Manual affix creation
            DrawManualAffixCreation(item);
            
            EditorGUILayout.Space();
            
            // Current affixes display
            DrawCurrentAffixes(item);
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawQuickAffixGeneration(BaseItem item)
    {
        EditorGUILayout.LabelField("Quick Affix Generation", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Generate Random Affixes"))
        {
            if (AffixDatabase.Instance != null)
            {
                AffixDatabase.Instance.GenerateRandomAffixes(item, item.requiredLevel, 0.3f, 0.1f);
                EditorUtility.SetDirty(item);
                Debug.Log($"Generated random affixes for {item.itemName}");
            }
            else
            {
                Debug.LogError("AffixDatabase not found! Please create it first.");
            }
        }
        
        if (GUILayout.Button("Clear All Affixes"))
        {
            if (EditorUtility.DisplayDialog("Clear Affixes", 
                $"Are you sure you want to clear all affixes from '{item.itemName}'?", "Yes", "No"))
            {
                item.ClearAffixes();
                item.implicitModifiers.Clear();
                EditorUtility.SetDirty(item);
                Debug.Log($"Cleared all affixes from {item.itemName}");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Make Magic Item"))
        {
            if (AffixDatabase.Instance != null)
            {
                item.ClearAffixes();
                AffixDatabase.Instance.GenerateRandomAffixes(item, item.requiredLevel, 0.5f, 0.0f);
                EditorUtility.SetDirty(item);
                Debug.Log($"Made {item.itemName} a Magic item");
            }
        }
        
        if (GUILayout.Button("Make Rare Item"))
        {
            if (AffixDatabase.Instance != null)
            {
                item.ClearAffixes();
                AffixDatabase.Instance.GenerateRandomAffixes(item, item.requiredLevel, 0.0f, 0.5f);
                EditorUtility.SetDirty(item);
                Debug.Log($"Made {item.itemName} a Rare item");
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawAffixSelector(BaseItem item)
    {
        EditorGUILayout.LabelField("Manual Affix Selection", EditorStyles.boldLabel);
        
        // Item Tags Management
        DrawItemTagsManagement(item);
        
        // Affix type selection
        selectedAffixType = (AffixType)EditorGUILayout.EnumPopup("Affix Type", selectedAffixType);
        
        // Check if we can add more affixes
        bool canAddPrefix = item.CanAddPrefix();
        bool canAddSuffix = item.CanAddSuffix();
        
        if ((selectedAffixType == AffixType.Prefix && !canAddPrefix) || 
            (selectedAffixType == AffixType.Suffix && !canAddSuffix))
        {
            EditorGUILayout.HelpBox($"Cannot add more {selectedAffixType}s. Maximum reached.", MessageType.Warning);
            return;
        }
        
        if (AffixDatabase.Instance == null)
        {
            EditorGUILayout.HelpBox("AffixDatabase not found! Please create it first.", MessageType.Error);
            return;
        }
        
        // Category selection
        List<string> categories = GetAvailableCategories(selectedAffixType);
        if (categories.Count > 0)
        {
            int categoryIndex = categories.IndexOf(selectedCategory);
            if (categoryIndex < 0) categoryIndex = 0;
            
            categoryIndex = EditorGUILayout.Popup("Category", categoryIndex, categories.ToArray());
            selectedCategory = categories[categoryIndex];
            
            // Sub-category selection
            if (!string.IsNullOrEmpty(selectedCategory))
            {
                List<string> subCategories = GetAvailableSubCategories(selectedAffixType, selectedCategory);
                if (subCategories.Count > 0)
                {
                    int subCategoryIndex = subCategories.IndexOf(selectedSubCategory);
                    if (subCategoryIndex < 0) subCategoryIndex = 0;
                    
                    subCategoryIndex = EditorGUILayout.Popup("Sub-Category", subCategoryIndex, subCategories.ToArray());
                    selectedSubCategory = subCategories[subCategoryIndex];
                    
                    // Affix selection
                    if (!string.IsNullOrEmpty(selectedSubCategory))
                    {
                        availableAffixes = GetAvailableAffixes(selectedAffixType, selectedCategory, selectedSubCategory, item);
                        
                        if (availableAffixes.Count > 0)
                        {
                            EditorGUILayout.LabelField("Available Affixes:", EditorStyles.boldLabel);
                            
                            foreach (var affix in availableAffixes)
                            {
                                EditorGUILayout.BeginHorizontal();
                                
                                EditorGUILayout.LabelField($"{affix.name} (Tier {affix.tier})", GUILayout.Width(200));
                                EditorGUILayout.LabelField(affix.description, GUILayout.ExpandWidth(true));
                                
                                if (GUILayout.Button("Add", GUILayout.Width(60)))
                                {
                                    if (selectedAffixType == AffixType.Prefix)
                                    {
                                        item.AddPrefix(affix);
                                    }
                                    else
                                    {
                                        item.AddSuffix(affix);
                                    }
                                    EditorUtility.SetDirty(item);
                                    Debug.Log($"Added {affix.name} to {item.itemName}");
                                }
                                
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("No compatible affixes found for this item. Check the item tags above.", MessageType.Info);
                        }
                    }
                }
            }
        }
    }
    
    private void DrawItemTagsManagement(BaseItem item)
    {
        EditorGUILayout.LabelField("Item Tags (Required for Affix Compatibility)", EditorStyles.boldLabel);
        
        // Show current tags
        if (item.itemTags != null && item.itemTags.Count > 0)
        {
            EditorGUILayout.LabelField("Current Tags:");
            EditorGUI.indentLevel++;
            for (int i = 0; i < item.itemTags.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"• {item.itemTags[i]}");
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    item.itemTags.RemoveAt(i);
                    EditorUtility.SetDirty(item);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUILayout.HelpBox("No tags set. Add tags to enable affix compatibility.", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        // Auto-set tags button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto-Set Tags"))
        {
            SetAppropriateTags(item);
            EditorUtility.SetDirty(item);
        }
        
        if (GUILayout.Button("Add Tag"))
        {
            if (item.itemTags == null)
                item.itemTags = new List<string>();
            item.itemTags.Add("new_tag");
            EditorUtility.SetDirty(item);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
    }
    
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
        
        Debug.Log($"Auto-set tags for {item.itemName}: {string.Join(", ", item.itemTags)}");
    }
    
    private void DrawManualAffixCreation(BaseItem item)
    {
        EditorGUILayout.LabelField("Manual Affix Creation", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Create custom affixes with manual value control.", MessageType.Info);
        
        // Affix type selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Affix Type:", GUILayout.Width(80));
        selectedManualAffixType = (AffixType)EditorGUILayout.EnumPopup(selectedManualAffixType);
        EditorGUILayout.EndHorizontal();
        
        // Affix name
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Affix Name:", GUILayout.Width(80));
        manualAffixName = EditorGUILayout.TextField(manualAffixName);
        EditorGUILayout.EndHorizontal();
        
        // Stat name
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stat Name:", GUILayout.Width(80));
        manualStatName = EditorGUILayout.TextField(manualStatName);
        EditorGUILayout.EndHorizontal();
        
        // Modifier type
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Modifier Type:", GUILayout.Width(80));
        selectedManualModifierType = (ModifierType)EditorGUILayout.EnumPopup(selectedManualModifierType);
        EditorGUILayout.EndHorizontal();
        
        // Scope
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scope:", GUILayout.Width(80));
        selectedManualScope = (ModifierScope)EditorGUILayout.EnumPopup(selectedManualScope);
        EditorGUILayout.EndHorizontal();
        
        // Damage type (if applicable)
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Damage Type:", GUILayout.Width(80));
        selectedManualDamageType = (DamageType)EditorGUILayout.EnumPopup(selectedManualDamageType);
        EditorGUILayout.EndHorizontal();
        
        // Value range
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Value Range:", GUILayout.Width(80));
        manualMinValue = EditorGUILayout.IntField(manualMinValue);
        EditorGUILayout.LabelField("to", GUILayout.Width(20));
        manualMaxValue = EditorGUILayout.IntField(manualMaxValue);
        EditorGUILayout.EndHorizontal();
        
        // Manual value slider
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Manual Value:", GUILayout.Width(80));
        manualSelectedValue = EditorGUILayout.Slider(manualSelectedValue, manualMinValue, manualMaxValue, GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField($"{(int)manualSelectedValue}", GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();
        
        // Random button for manual value
        if (GUILayout.Button("Randomize Manual Value", GUILayout.Width(150)))
        {
            manualSelectedValue = Random.Range(manualMinValue, manualMaxValue + 1);
        }
        
        // Create affix button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Custom Affix", GUILayout.Width(150)))
        {
            CreateManualAffix(item);
        }
        
        // Clear form button
        if (GUILayout.Button("Clear Form", GUILayout.Width(100)))
        {
            ClearManualAffixForm();
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void CreateManualAffix(BaseItem item)
    {
        if (string.IsNullOrEmpty(manualAffixName) || string.IsNullOrEmpty(manualStatName))
        {
            EditorUtility.DisplayDialog("Error", "Please fill in both Affix Name and Stat Name.", "OK");
            return;
        }
        
        if (manualMinValue > manualMaxValue)
        {
            EditorUtility.DisplayDialog("Error", "Minimum value cannot be greater than maximum value.", "OK");
            return;
        }
        
        // Create the affix
        Affix newAffix = new Affix(manualAffixName, $"{manualAffixName} affix", selectedManualAffixType, AffixTier.Tier1);
        
        // Create the modifier with the selected value
        AffixModifier modifier = new AffixModifier(
            manualStatName,
            (int)manualSelectedValue,
            (int)manualSelectedValue, // Use the same value for min/max since it's manually set
            selectedManualModifierType,
            selectedManualScope
        );
        modifier.damageType = selectedManualDamageType;
        
        newAffix.modifiers.Add(modifier);
        
        // Add to the appropriate list
        bool success = false;
        switch (selectedManualAffixType)
        {
            case AffixType.Prefix:
                success = item.AddPrefix(newAffix);
                break;
            case AffixType.Suffix:
                success = item.AddSuffix(newAffix);
                break;
        }
        
        if (success)
        {
            EditorUtility.SetDirty(item);
            Debug.Log($"Created custom {selectedManualAffixType} '{manualAffixName}' with value {(int)manualSelectedValue}");
            ClearManualAffixForm();
        }
        else
        {
            EditorUtility.DisplayDialog("Error", $"Cannot add {selectedManualAffixType}. Maximum count reached.", "OK");
        }
    }
    
    private void ClearManualAffixForm()
    {
        manualAffixName = "";
        manualStatName = "";
        manualMinValue = 1;
        manualMaxValue = 10;
        manualSelectedValue = 5;
        selectedManualAffixType = AffixType.Prefix;
        selectedManualModifierType = ModifierType.Flat;
        selectedManualScope = ModifierScope.Local;
        selectedManualDamageType = DamageType.None;
    }
    
    private void DrawCurrentAffixes(BaseItem item)
    {
        EditorGUILayout.LabelField("Current Affixes", EditorStyles.boldLabel);
        
        // Implicit Modifiers
        if (item.implicitModifiers.Count > 0)
        {
            EditorGUILayout.LabelField("Implicit Modifiers:", EditorStyles.boldLabel);
            for (int i = 0; i < item.implicitModifiers.Count; i++)
            {
                DrawAffixWithSlider(item.implicitModifiers[i], $"Implicit {i + 1}", item);
            }
        }
        
        // Prefixes
        if (item.prefixes.Count > 0)
        {
            EditorGUILayout.LabelField("Prefixes:", EditorStyles.boldLabel);
            for (int i = 0; i < item.prefixes.Count; i++)
            {
                DrawAffixWithSlider(item.prefixes[i], $"Prefix {i + 1}", item);
            }
        }
        
        // Suffixes
        if (item.suffixes.Count > 0)
        {
            EditorGUILayout.LabelField("Suffixes:", EditorStyles.boldLabel);
            for (int i = 0; i < item.suffixes.Count; i++)
            {
                DrawAffixWithSlider(item.suffixes[i], $"Suffix {i + 1}", item);
            }
        }
        
        if (item.implicitModifiers.Count == 0 && item.prefixes.Count == 0 && item.suffixes.Count == 0)
        {
            EditorGUILayout.HelpBox("No affixes currently applied to this item.", MessageType.Info);
        }
    }
    
    private void DrawAffixWithSlider(Affix affix, string affixLabel, BaseItem item)
    {
        EditorGUILayout.BeginVertical("box");
        
        // Affix header with remove button
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{affixLabel}: {affix.name}", EditorStyles.boldLabel);
        if (GUILayout.Button("Remove", GUILayout.Width(60)))
        {
            RemoveAffix(item, affix);
            return;
        }
        EditorGUILayout.EndHorizontal();
        
        // Draw each modifier with its slider
        for (int i = 0; i < affix.modifiers.Count; i++)
        {
            var modifier = affix.modifiers[i];
            DrawModifierWithSlider(modifier, i, affix, item);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawModifierWithSlider(AffixModifier modifier, int modifierIndex, Affix affix, BaseItem item)
    {
        EditorGUILayout.BeginVertical("box");
        
        // Modifier header
        string modifierTypeText = modifier.modifierType.ToString();
        string scopeText = modifier.scope == ModifierScope.Local ? "🔧" : "🌍";
        string damageTypeText = modifier.damageType != DamageType.None ? $" ({modifier.damageType})" : "";
        
        EditorGUILayout.LabelField($"{scopeText} {modifier.statName}{damageTypeText} - {modifierTypeText}", EditorStyles.boldLabel);
        
        if (modifier.isDualRange)
        {
            // Handle dual-range modifiers
            EditorGUILayout.LabelField($"Current Values: {(int)modifier.rolledFirstValue} to {(int)modifier.rolledSecondValue}");
            
            // First range slider
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min Value:", GUILayout.Width(80));
            float newFirstValue = EditorGUILayout.Slider(modifier.rolledFirstValue, modifier.firstRangeMin, modifier.firstRangeMax, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            
            // Second range slider
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max Value:", GUILayout.Width(80));
            float newSecondValue = EditorGUILayout.Slider(modifier.rolledSecondValue, modifier.secondRangeMin, modifier.secondRangeMax, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            
            // Random button for dual-range
            if (GUILayout.Button("Randomize Both Values", GUILayout.Width(150)))
            {
                newFirstValue = Random.Range((int)modifier.firstRangeMin, (int)modifier.firstRangeMax + 1);
                newSecondValue = Random.Range((int)modifier.secondRangeMin, (int)modifier.secondRangeMax + 1);
            }
            
            // Update the modifier values if changed
            if (!Mathf.Approximately(newFirstValue, modifier.rolledFirstValue) || !Mathf.Approximately(newSecondValue, modifier.rolledSecondValue))
            {
                modifier.rolledFirstValue = (int)newFirstValue;
                modifier.rolledSecondValue = (int)newSecondValue;
                modifier.minValue = (int)newFirstValue;
                modifier.maxValue = (int)newSecondValue;
                EditorUtility.SetDirty(item);
            }
            
            // Show the range info
            EditorGUILayout.LabelField($"Original Ranges: {(int)modifier.firstRangeMin}-{(int)modifier.firstRangeMax} to {(int)modifier.secondRangeMin}-{(int)modifier.secondRangeMax}", EditorStyles.miniLabel);
        }
        else
        {
            // Handle single-range modifiers (existing logic)
            EditorGUILayout.LabelField($"Current Value: {(int)modifier.minValue}");
            
            // Slider for manual value control
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Manual Value:", GUILayout.Width(80));
            
            // Use the original range for the slider
            float minRange = modifier.originalMinValue;
            float maxRange = modifier.originalMaxValue;
            
            // If original range is not set (legacy affixes), use current values
            if (Mathf.Approximately(minRange, 0) && Mathf.Approximately(maxRange, 0))
            {
                minRange = modifier.minValue;
                maxRange = modifier.maxValue;
            }
            
            // Create a slider for manual value control
            float newValue = EditorGUILayout.Slider(modifier.minValue, minRange, maxRange, GUILayout.ExpandWidth(true));
            
            // Add a "Random" button to re-roll the value within the original range
            if (GUILayout.Button("Random", GUILayout.Width(60)))
            {
                newValue = Random.Range((int)minRange, (int)maxRange + 1);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Update the modifier value if changed
            if (!Mathf.Approximately(newValue, modifier.minValue))
            {
                modifier.minValue = (int)newValue;
                modifier.maxValue = (int)newValue; // Keep min and max the same for rolled affixes
                EditorUtility.SetDirty(item);
            }
            
            // Show the range info
            EditorGUILayout.LabelField($"Original Range: {(int)minRange} - {(int)maxRange}", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void RemoveAffix(BaseItem item, Affix affix)
    {
        if (item.implicitModifiers.Contains(affix))
        {
            item.implicitModifiers.Remove(affix);
        }
        else if (item.prefixes.Contains(affix))
        {
            item.prefixes.Remove(affix);
        }
        else if (item.suffixes.Contains(affix))
        {
            item.suffixes.Remove(affix);
        }
        EditorUtility.SetDirty(item);
        Debug.Log($"Removed {affix.name} from {item.itemName}");
    }
    
    private List<string> GetAvailableCategories(AffixType affixType)
    {
        if (AffixDatabase.Instance == null) return new List<string>();
        
        List<string> categories = new List<string>();
        
        switch (affixType)
        {
            case AffixType.Prefix:
                foreach (var category in AffixDatabase.Instance.weaponPrefixCategories)
                {
                    if (!categories.Contains(category.categoryName))
                        categories.Add(category.categoryName);
                }
                break;
            case AffixType.Suffix:
                foreach (var category in AffixDatabase.Instance.weaponSuffixCategories)
                {
                    if (!categories.Contains(category.categoryName))
                        categories.Add(category.categoryName);
                }
                break;
        }
        
        return categories;
    }
    
    private List<string> GetAvailableSubCategories(AffixType affixType, string categoryName)
    {
        if (AffixDatabase.Instance == null) return new List<string>();
        
        List<string> subCategories = new List<string>();
        
        switch (affixType)
        {
            case AffixType.Prefix:
                var prefixCategory = AffixDatabase.Instance.weaponPrefixCategories.Find(c => c.categoryName == categoryName);
                if (prefixCategory != null)
                {
                    foreach (var subCategory in prefixCategory.subCategories)
                    {
                        subCategories.Add(subCategory.subCategoryName);
                    }
                }
                break;
            case AffixType.Suffix:
                var suffixCategory = AffixDatabase.Instance.weaponSuffixCategories.Find(c => c.categoryName == categoryName);
                if (suffixCategory != null)
                {
                    foreach (var subCategory in suffixCategory.subCategories)
                    {
                        subCategories.Add(subCategory.subCategoryName);
                    }
                }
                break;
        }
        
        return subCategories;
    }
    
    private List<Affix> GetAvailableAffixes(AffixType affixType, string categoryName, string subCategoryName, BaseItem item)
    {
        if (AffixDatabase.Instance == null) return new List<Affix>();
        
        List<Affix> availableAffixes = new List<Affix>();
        
        switch (affixType)
        {
            case AffixType.Prefix:
                var prefixCategory = AffixDatabase.Instance.weaponPrefixCategories.Find(c => c.categoryName == categoryName);
                if (prefixCategory != null)
                {
                    var prefixSubCategory = prefixCategory.subCategories.Find(sc => sc.subCategoryName == subCategoryName);
                    if (prefixSubCategory != null)
                    {
                        foreach (var affix in prefixSubCategory.affixes)
                        {
                            if (IsAffixCompatibleWithItem(affix, item))
                            {
                                availableAffixes.Add(affix);
                            }
                        }
                    }
                }
                break;
            case AffixType.Suffix:
                var suffixCategory = AffixDatabase.Instance.weaponSuffixCategories.Find(c => c.categoryName == categoryName);
                if (suffixCategory != null)
                {
                    var suffixSubCategory = suffixCategory.subCategories.Find(sc => sc.subCategoryName == subCategoryName);
                    if (suffixSubCategory != null)
                    {
                        foreach (var affix in suffixSubCategory.affixes)
                        {
                            if (IsAffixCompatibleWithItem(affix, item))
                            {
                                availableAffixes.Add(affix);
                            }
                        }
                    }
                }
                break;
        }
        
        return availableAffixes;
    }
    
    private bool IsAffixCompatibleWithItem(Affix affix, BaseItem item)
    {
        if (affix.requiredTags == null || affix.requiredTags.Count == 0) return true;
        if (item.itemTags == null || item.itemTags.Count == 0) return false;
        
        // Check handedness compatibility
        if (affix.handedness == Handedness.OneHand)
        {
            if (!item.itemTags.Contains("onehanded"))
            {
                return false; // OneHand affix requires one-handed weapon
            }
        }
        else if (affix.handedness == Handedness.TwoHand)
        {
            if (!item.itemTags.Contains("twohanded"))
            {
                return false; // TwoHand affix requires two-handed weapon
            }
        }
        // Handedness.Both is compatible with all weapons
        
        // Check if any of the affix's required tags match the item's tags
        foreach (var requiredTag in affix.requiredTags)
        {
            if (item.itemTags.Contains(requiredTag))
            {
                return true;
            }
        }
        
        return false;
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
    
    /// <summary>
    /// Draws a custom inspector with foldout description and equipment type sync
    /// </summary>
    private void DrawCustomInspector(BaseItem item)
    {
        SerializedObject serializedObject = new SerializedObject(item);
        serializedObject.Update();
        
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            
            // Skip the script field
            if (iterator.name == "m_Script")
                continue;
            
            // Handle description field with foldout
            if (iterator.name == "description")
            {
                showDescription = EditorGUILayout.Foldout(showDescription, "Description", true);
                if (showDescription)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(iterator, true);
                    EditorGUI.indentLevel--;
                }
                continue;
            }
            
            // Handle equipment type sync for armour items
            if (iterator.name == "equipmentType" && item is Armour)
            {
                EditorGUILayout.PropertyField(iterator, true);
                
                // Auto-sync equipment type to armour slot for specific types
                EquipmentType newEquipmentType = (EquipmentType)iterator.enumValueIndex;
                SerializedProperty armourSlotProperty = serializedObject.FindProperty("armourSlot");
                
                if (armourSlotProperty != null)
                {
                    ArmourSlot? newArmourSlot = GetArmourSlotFromEquipmentType(newEquipmentType);
                    if (newArmourSlot.HasValue)
                    {
                        armourSlotProperty.enumValueIndex = (int)newArmourSlot.Value;
                    }
                }
                continue;
            }
            
            // Handle ring equipment type for jewellery
            if (iterator.name == "equipmentType" && item is Jewellery)
            {
                EditorGUILayout.PropertyField(iterator, true);
                
                // For rings, show a note about ambiguity
                EquipmentType equipmentType = (EquipmentType)iterator.enumValueIndex;
                if (equipmentType == EquipmentType.LeftRing || equipmentType == EquipmentType.RightRing)
                {
                    EditorGUILayout.HelpBox("Ring can be equipped in either left or right ring slot", MessageType.Info);
                }
                continue;
            }
            
            // Draw all other properties normally
            EditorGUILayout.PropertyField(iterator, true);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    /// <summary>
    /// Maps EquipmentType to ArmourSlot for auto-sync
    /// </summary>
    private ArmourSlot? GetArmourSlotFromEquipmentType(EquipmentType equipmentType)
    {
        switch (equipmentType)
        {
            case EquipmentType.Helmet:
                return ArmourSlot.Helmet;
            case EquipmentType.BodyArmour:
                return ArmourSlot.BodyArmour;
            case EquipmentType.Gloves:
                return ArmourSlot.Gloves;
            case EquipmentType.Boots:
                return ArmourSlot.Boots;
            // Belt doesn't exist in ArmourSlot, so we skip it
            default:
                return null; // No auto-sync for other types
        }
    }
}
