using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class AffixCSVImporter : EditorWindow
{
    [Header("CSV Import Settings")]
    private string csvFilePath = "Assets/Scripts/Documentation/Comprehensive_Mods.csv";
    private string outputFolderPath = "Assets/Resources/Affixes";
    private AffixDatabase_Modern affixDatabase;
    
    [Header("Import Filters")]
    private bool filterByPrefix = false;
    private bool onlyImportPrefixes = true;
    private bool onlyImportSuffixes = true;
    
    [Header("Category Filters")]
    private bool importCoreAttributes = true;
    private bool importCombatResources = true;
    private bool importPhysicalDamage = true;
    private bool importElementalDamage = true;
    private bool importSpellDamage = true;
    private bool importAttackSpeed = true;
    private bool importCriticalStrikes = true;
    private bool importResistances = true;
    private bool importDefenseStats = true;
    private bool importAilments = true;
    private bool importRecovery = true;
    private bool importMovement = true;
    private bool importCardSystem = true;
    private bool importHybridMods = true;
    private bool importLegendaryMods = true;
    
    [Header("Import Results")]
    private List<AffixData> previewAffixes = new List<AffixData>();
    private bool showPreview = false;
    private Vector2 scrollPosition;
    
    [System.Serializable]
    public class AffixData
    {
        public string category;
        public string prefixSuffix;
        public string name;
        public string statText;
        public float minValue;
        public float maxValue;
        public string itemTypes;
        public string statName;
        public int tier;
        public int minLevel;
        public string scope; // Local or Global
        
        public bool IsPrefix => prefixSuffix.Equals("Prefix", System.StringComparison.OrdinalIgnoreCase);
        public bool IsSuffix => prefixSuffix.Equals("Suffix", System.StringComparison.OrdinalIgnoreCase);
        public ModifierScope GetModifierScope() => scope.Equals("Local", System.StringComparison.OrdinalIgnoreCase) ? ModifierScope.Local : ModifierScope.Global;
    }
    
    [MenuItem("Dexiled/Import Affixes from CSV")]
    public static void ShowWindow()
    {
        AffixCSVImporter window = GetWindow<AffixCSVImporter>("Affix CSV Importer");
        window.minSize = new Vector2(500, 700);
    }
    
    void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Affix CSV Importer", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Import affixes from CSV with Local vs Global modifier system and smart compatibility.\n" +
            "CSV Format: Category, Prefix/Suffix, Name, Stat Text, Min, Max, Item Types, Stat Name, Tier, Min Level, Scope\n" +
            "Scope: 'Local' (affects item stats), 'Global' (affects character stats)\n" +
            "Tiers: 1 (Endgame 80+), 2 (Late 70+), 3 (High 60+), 4 (Mid-Late 50+), 5 (Mid 40+), 6 (Early-Mid 30+), 7 (Early 20+), 8 (Starter 10+), 9 (Beginning 1+)\n" +
            "Local Examples: Physical Damage % (weapons), Armour % (armor). Global Examples: Resistances, Accuracy, Crit Multiplier.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        // File Selection
        EditorGUILayout.LabelField("File Selection", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("CSV File:", GUILayout.Width(70));
        csvFilePath = EditorGUILayout.TextField(csvFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("Select CSV File", "Assets", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                csvFilePath = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Output Folder Selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Folder:", GUILayout.Width(85));
        outputFolderPath = EditorGUILayout.TextField(outputFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                outputFolderPath = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Affix Database Selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Affix Database:", GUILayout.Width(100));
        affixDatabase = (AffixDatabase_Modern)EditorGUILayout.ObjectField(affixDatabase, typeof(AffixDatabase_Modern), false);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Import Filters
        EditorGUILayout.LabelField("Import Filters", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        filterByPrefix = EditorGUILayout.Toggle("Filter by Type:", filterByPrefix, GUILayout.Width(120));
        if (filterByPrefix)
        {
            onlyImportPrefixes = EditorGUILayout.Toggle("Prefixes", onlyImportPrefixes, GUILayout.Width(80));
            onlyImportSuffixes = EditorGUILayout.Toggle("Suffixes", onlyImportSuffixes);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Category Filters
        EditorGUILayout.LabelField("Category Filters", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        importCoreAttributes = EditorGUILayout.Toggle("Attributes", importCoreAttributes, GUILayout.Width(120));
        importCombatResources = EditorGUILayout.Toggle("Resources", importCombatResources, GUILayout.Width(120));
        importPhysicalDamage = EditorGUILayout.Toggle("Physical", importPhysicalDamage);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        importElementalDamage = EditorGUILayout.Toggle("Elemental", importElementalDamage, GUILayout.Width(120));
        importSpellDamage = EditorGUILayout.Toggle("Spell Damage", importSpellDamage, GUILayout.Width(120));
        importAttackSpeed = EditorGUILayout.Toggle("Speed", importAttackSpeed);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        importCriticalStrikes = EditorGUILayout.Toggle("Critical", importCriticalStrikes, GUILayout.Width(120));
        importResistances = EditorGUILayout.Toggle("Resistances", importResistances, GUILayout.Width(120));
        importDefenseStats = EditorGUILayout.Toggle("Defense", importDefenseStats);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        importAilments = EditorGUILayout.Toggle("Ailments", importAilments, GUILayout.Width(120));
        importRecovery = EditorGUILayout.Toggle("Recovery", importRecovery, GUILayout.Width(120));
        importMovement = EditorGUILayout.Toggle("Movement", importMovement);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        importCardSystem = EditorGUILayout.Toggle("Card System", importCardSystem, GUILayout.Width(120));
        importHybridMods = EditorGUILayout.Toggle("Hybrid Mods", importHybridMods, GUILayout.Width(120));
        importLegendaryMods = EditorGUILayout.Toggle("Legendary", importLegendaryMods);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Action Buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Preview Import", GUILayout.Height(30)))
        {
            PreviewImport();
        }
        if (GUILayout.Button("Import Affixes", GUILayout.Height(30)))
        {
            ImportAffixes();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Preview Results
        if (showPreview && previewAffixes.Count > 0)
        {
            EditorGUILayout.LabelField($"Preview Results ({previewAffixes.Count} affixes)", EditorStyles.boldLabel);
            
            DisplayCategoryCounts();
            
            EditorGUILayout.Space(5);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            foreach (var affix in previewAffixes.Take(20)) // Show first 20 for preview
            {
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField($"{affix.name}", EditorStyles.boldLabel, GUILayout.Width(120));
                EditorGUILayout.LabelField($"({affix.prefixSuffix})", GUILayout.Width(60));
                EditorGUILayout.LabelField($"T{affix.tier} L{affix.minLevel}", GUILayout.Width(60));
                EditorGUILayout.LabelField($"[{affix.scope}]", GUILayout.Width(50)); // Show Local/Global scope
                EditorGUILayout.LabelField(affix.statText, GUILayout.Width(180));
                EditorGUILayout.LabelField($"{affix.itemTypes}", GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();
            }
            
            if (previewAffixes.Count > 20)
            {
                EditorGUILayout.LabelField($"... and {previewAffixes.Count - 20} more affixes");
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
    
    private void PreviewImport()
    {
        if (!File.Exists(csvFilePath))
        {
            EditorUtility.DisplayDialog("Error", $"CSV file not found: {csvFilePath}", "OK");
            return;
        }
        
        previewAffixes.Clear();
        
        string[] lines = File.ReadAllLines(csvFilePath);
        bool isFirstLine = true;
        
        foreach (string line in lines)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                continue; // Skip header
            }
            
            if (string.IsNullOrEmpty(line.Trim()) || line.Trim().StartsWith("#"))
                continue; // Skip empty lines and comments
            
            AffixData affixData = ParseAffixLine(line);
            if (affixData != null && ShouldIncludeAffix(affixData))
            {
                previewAffixes.Add(affixData);
            }
        }
        
        showPreview = true;
        Debug.Log($"Preview completed: {previewAffixes.Count} affixes ready for import");
    }
    
    private void ImportAffixes()
    {
        if (affixDatabase == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign an Affix Database before importing!", "OK");
            return;
        }
        
        if (previewAffixes.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No affixes to import. Run Preview Import first!", "OK");
            return;
        }
        
        if (!Directory.Exists(outputFolderPath))
        {
            Directory.CreateDirectory(outputFolderPath);
        }
        
        int importedCount = 0;
        
        int prefixCount = 0;
        int suffixCount = 0;
        
        foreach (var affixData in previewAffixes)
        {
            Affix affix = CreateAffix(affixData);
            if (affix != null)
            {
                // Add to appropriate category in database
                AddAffixToDatabase(affixDatabase, affix, affixData);
                importedCount++;
                
                if (affixData.IsPrefix)
                    prefixCount++;
                else
                    suffixCount++;
            }
        }
        
        // Mark database as dirty and save
        EditorUtility.SetDirty(affixDatabase);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Successfully imported {importedCount} affixes to {affixDatabase.name}");
        EditorUtility.DisplayDialog("Import Complete", 
            $"Successfully imported {importedCount} affixes to the database!\n" +
            $"Prefixes: {prefixCount}\n" +
            $"Suffixes: {suffixCount}", "OK");
    }
    
    private AffixData ParseAffixLine(string line)
    {
        try
        {
            string[] columns = ParseCSVLine(line);
            
            if (columns.Length < 8)
            {
                Debug.LogWarning($"Insufficient columns in line: {line}");
                return null;
            }
            
            AffixData data = new AffixData
            {
                category = columns[0].Trim(),
                prefixSuffix = columns[1].Trim(),
                name = columns[2].Trim(),
                statText = columns[3].Trim(),
                minValue = float.Parse(columns[4].Trim()),
                maxValue = float.Parse(columns[5].Trim()),
                itemTypes = columns[6].Trim(),
                statName = columns[7].Trim(),
                tier = columns.Length > 8 && !string.IsNullOrEmpty(columns[8].Trim()) ? int.Parse(columns[8].Trim()) : 5, // Default to Tier 5 if missing
                minLevel = columns.Length > 9 && !string.IsNullOrEmpty(columns[9].Trim()) ? int.Parse(columns[9].Trim()) : 1, // Default to Level 1 if missing
                scope = columns.Length > 10 && !string.IsNullOrEmpty(columns[10].Trim()) ? columns[10].Trim() : "Global" // Default to Global for safety
            };
            
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to parse line: {line}. Error: {e.Message}");
            return null;
        }
    }
    
    private string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        fields.Add(currentField); // Add the last field
        return fields.ToArray();
    }
    
    private bool ShouldIncludeAffix(AffixData affixData)
    {
        // Filter by prefix/suffix type
        if (filterByPrefix)
        {
            if (affixData.IsPrefix && !onlyImportPrefixes) return false;
            if (affixData.IsSuffix && !onlyImportSuffixes) return false;
        }
        
        // Filter by category
        string category = affixData.category.ToLower();
        
        if (category.Contains("strength") || category.Contains("dexterity") || category.Contains("intelligence"))
            return importCoreAttributes;
        if (category.Contains("life") || category.Contains("mana") || category.Contains("energy") || category.Contains("reliance"))
            return importCombatResources;
        if (category.Contains("physical damage"))
            return importPhysicalDamage;
        if (category.Contains("fire") || category.Contains("cold") || category.Contains("lightning") || category.Contains("chaos"))
            return importElementalDamage;
        if (category.Contains("spell damage"))
            return importSpellDamage;
        if (category.Contains("speed"))
            return importAttackSpeed;
        if (category.Contains("critical"))
            return importCriticalStrikes;
        if (category.Contains("resistance"))
            return importResistances;
        if (category.Contains("armour") || category.Contains("evasion") || category.Contains("block") || category.Contains("dodge"))
            return importDefenseStats;
        if (category.Contains("ignite") || category.Contains("shock") || category.Contains("chill") || category.Contains("freeze") || 
            category.Contains("bleed") || category.Contains("poison"))
            return importAilments;
        if (category.Contains("regeneration") || category.Contains("leech"))
            return importRecovery;
        if (category.Contains("movement") || category.Contains("accuracy") || category.Contains("area") || category.Contains("projectile"))
            return importMovement;
        if (category.Contains("card"))
            return importCardSystem;
        if (category.Contains("damage & speed") || category.Contains("damage & crit") || category.Contains("life & mana") || 
            category.Contains("defense & speed") || category.Contains("spell & cast"))
            return importHybridMods;
        if (category.Contains("legendary"))
            return importLegendaryMods;
            
        return true; // Default include
    }
    
    private Affix CreateAffix(AffixData data)
    {
        // Create the Affix object (it's a serializable class, not a ScriptableObject)
        Affix affix = new Affix(
            data.name,
            data.statText,
            data.IsPrefix ? AffixType.Prefix : AffixType.Suffix,
            ConvertTierNumber(data.tier)
        );
        
        // Set additional properties
        affix.minLevel = data.minLevel;
        
        // Create modifiers
        affix.modifiers = new List<AffixModifier>();
        
        // Handle hybrid modifiers (contains |)
        if (data.statName.Contains("|"))
        {
            string[] statNames = data.statName.Split('|');
            // For hybrid mods, we'll need to parse the min/max values differently
            // For now, split the values equally
            foreach (string statName in statNames)
            {
                AffixModifier modifier = new AffixModifier(
                    MapStatName(statName.Trim()),
                    data.minValue / statNames.Length,
                    data.maxValue / statNames.Length,
                    DetermineModifierType(data.statText)
                );
                modifier.scope = data.GetModifierScope(); // Set Local vs Global scope
                affix.modifiers.Add(modifier);
            }
        }
        else
        {
            AffixModifier modifier = new AffixModifier(
                MapStatName(data.statName),
                data.minValue,
                data.maxValue,
                DetermineModifierType(data.statText)
            );
            modifier.scope = data.GetModifierScope(); // Set Local vs Global scope
            
            // Check for dual-range format: "Adds (X-Y) to (Z-W) Damage"
            if (IsDualRangeFormat(data.statText, out float firstMin, out float firstMax, out float secondMin, out float secondMax))
            {
                modifier.isDualRange = true;
                modifier.firstRangeMin = firstMin;
                modifier.firstRangeMax = firstMax;
                modifier.secondRangeMin = secondMin;
                modifier.secondRangeMax = secondMax;
                Debug.Log($"Dual-range detected: ({firstMin}-{firstMax}) to ({secondMin}-{secondMax})");
            }
            
            affix.modifiers.Add(modifier);
        }
        
        // Set item type compatibility tags
        affix.compatibleTags = GetCompatibilityTags(data.itemTypes);
        affix.requiredTags = new List<string>(affix.compatibleTags); // Also set legacy requiredTags for Inspector visibility
        
        Debug.Log($"Created affix: {data.name} (Tier {data.tier}, Level {data.minLevel})");
        Debug.Log($"  Compatible Tags: [{string.Join(", ", affix.compatibleTags)}]");
        return affix;
    }
    
    private void AddAffixToDatabase(AffixDatabase_Modern database, Affix affix, AffixData data)
    {
        // Determine which category lists to use based on item types
        List<List<AffixCategory>> targetCategoryLists = GetTargetCategoriesForAffix(database, data);
        
        foreach (var categoryList in targetCategoryLists)
        {
            // Find or create category for this affix
            AffixCategory category = categoryList.FirstOrDefault(c => c.categoryName == data.category);
            if (category == null)
            {
                category = new AffixCategory(data.category);
                categoryList.Add(category);
            }
            
            // Find or create subcategory (use category name as subcategory for now)
            AffixSubCategory subCategory = category.subCategories.FirstOrDefault(sc => sc.subCategoryName == data.category);
            if (subCategory == null)
            {
                subCategory = new AffixSubCategory(data.category);
                category.subCategories.Add(subCategory);
            }
            
            // Check if affix already exists (by name)
            if (!subCategory.affixes.Any(a => a.name == affix.name))
            {
                subCategory.affixes.Add(affix);
                Debug.Log($"Added {affix.name} to {category.categoryName}/{subCategory.subCategoryName}");
            }
        }
    }
    
    private List<List<AffixCategory>> GetTargetCategoriesForAffix(AffixDatabase_Modern database, AffixData data)
    {
        List<List<AffixCategory>> targets = new List<List<AffixCategory>>();
        
        // Check if affix is compatible with weapons, armour, or jewelry based on tags
        string itemTypes = data.itemTypes.ToLower();
        bool isWeapon = itemTypes.Contains("weapon");
        bool isArmour = itemTypes.Contains("armour");
        bool isJewelry = itemTypes.Contains("jewelry");
        
        if (data.IsPrefix)
        {
            if (isWeapon) targets.Add(database.weaponPrefixCategories);
            if (isArmour) targets.Add(database.armourPrefixCategories);
            if (isJewelry) targets.Add(database.jewelleryPrefixCategories);
        }
        else // Suffix
        {
            if (isWeapon) targets.Add(database.weaponSuffixCategories);
            if (isArmour) targets.Add(database.armourSuffixCategories);
            if (isJewelry) targets.Add(database.jewellerySuffixCategories);
        }
        
        // If no specific type, add to all
        if (targets.Count == 0)
        {
            if (data.IsPrefix)
            {
                targets.Add(database.weaponPrefixCategories);
                targets.Add(database.armourPrefixCategories);
                targets.Add(database.jewelleryPrefixCategories);
            }
            else
            {
                targets.Add(database.weaponSuffixCategories);
                targets.Add(database.armourSuffixCategories);
                targets.Add(database.jewellerySuffixCategories);
            }
        }
        
        return targets;
    }
    
    private AffixTier ConvertTierNumber(int tierNumber)
    {
        // Convert CSV tier number (1-9) to AffixTier enum
        switch (tierNumber)
        {
            case 1: return AffixTier.Tier1;  // Endgame (80+)
            case 2: return AffixTier.Tier2;  // Late game (70+)
            case 3: return AffixTier.Tier3;  // High level (60+)
            case 4: return AffixTier.Tier4;  // Mid-late (50+)
            case 5: return AffixTier.Tier5;  // Mid game (40+)
            case 6: return AffixTier.Tier6;  // Early-mid (30+)
            case 7: return AffixTier.Tier7;  // Early (20+)
            case 8: return AffixTier.Tier8;  // Starter (10+)
            case 9: return AffixTier.Tier9;  // Beginning (1+)
            default:
                Debug.LogWarning($"Invalid tier number: {tierNumber}, defaulting to Tier5");
                return AffixTier.Tier5;
        }
    }
    
    private AffixTier DetermineTier(float minValue, float maxValue, string statName)
    {
        float avgValue = (minValue + maxValue) / 2f;
        
        // Fallback tier determination based on average value ranges
        // This is a simplified heuristic - adjust based on your game balance
        
        if (avgValue <= 10) return AffixTier.Tier9;
        if (avgValue <= 15) return AffixTier.Tier8;
        if (avgValue <= 20) return AffixTier.Tier7;
        if (avgValue <= 25) return AffixTier.Tier6;
        if (avgValue <= 30) return AffixTier.Tier5;
        if (avgValue <= 35) return AffixTier.Tier4;
        if (avgValue <= 45) return AffixTier.Tier3;
        if (avgValue <= 55) return AffixTier.Tier2;
        return AffixTier.Tier1;
    }
    
    private ModifierType DetermineModifierType(string statText)
    {
        string lower = statText.ToLower();
        
        if (lower.Contains("increased") || lower.Contains("%"))
            return ModifierType.Increased;
        if (lower.Contains("reduced") || lower.Contains("decreased"))
            return ModifierType.Reduced;
        
        return ModifierType.Flat; // Default for flat additions
    }
    
    /// <summary>
    /// Detects and parses dual-range format: "Adds (X-Y) to (Z-W) Damage"
    /// </summary>
    private bool IsDualRangeFormat(string statText, out float firstMin, out float firstMax, out float secondMin, out float secondMax)
    {
        firstMin = firstMax = secondMin = secondMax = 0;
        
        // Pattern: "Adds (X-Y) to (Z-W) [Type] Damage"
        System.Text.RegularExpressions.Regex dualRangeRegex = new System.Text.RegularExpressions.Regex(
            @"Adds\s+\((\d+)–(\d+)\)\s+to\s+\((\d+)–(\d+)\)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        
        System.Text.RegularExpressions.Match match = dualRangeRegex.Match(statText);
        if (match.Success)
        {
            firstMin = float.Parse(match.Groups[1].Value);
            firstMax = float.Parse(match.Groups[2].Value);
            secondMin = float.Parse(match.Groups[3].Value);
            secondMax = float.Parse(match.Groups[4].Value);
            return true;
        }
        
        return false;
    }
    
    private string MapStatName(string csvStatName)
    {
        // Map CSV stat names to actual CharacterStatsData field names
        Dictionary<string, string> statMapping = new Dictionary<string, string>
        {
            // Core Attributes
            {"strength", "strength"},
            {"dexterity", "dexterity"},
            {"intelligence", "intelligence"},
            
            // Resources
            {"maxHealth", "maxHealth"},
            {"maxMana", "maxMana"},
            {"maxReliance", "maxReliance"},
            {"energyShield", "energyShield"},
            
            // Damage
            {"addedPhysicalDamage", "addedPhysicalDamage"},
            {"increasedPhysicalDamage", "increasedPhysicalDamage"},
            {"addedFireDamage", "addedFireDamage"},
            {"increasedFireDamage", "increasedFireDamage"},
            {"addedColdDamage", "addedColdDamage"},
            {"increasedColdDamage", "increasedColdDamage"},
            {"addedLightningDamage", "addedLightningDamage"},
            {"increasedLightningDamage", "increasedLightningDamage"},
            {"addedChaosDamage", "addedChaosDamage"},
            {"increasedChaosDamage", "increasedChaosDamage"},
            {"increasedSpellDamage", "increasedSpellDamage"},
            {"increasedAttackDamage", "increasedAttackDamage"},
            {"increasedElementalAttackDamage", "increasedElementalAttackDamage"},
            
            // Speed & Crit
            {"attackSpeed", "attackSpeed"},
            {"castSpeed", "castSpeed"},
            {"criticalChance", "criticalChance"},
            {"criticalMultiplier", "criticalMultiplier"},
            
            // Resistances
            {"fireResistance", "fireResistance"},
            {"coldResistance", "coldResistance"},
            {"lightningResistance", "lightningResistance"},
            {"chaosResistance", "chaosResistance"},
            {"allResistance", "allResistance"},
            {"elementalResistance", "elementalResistance"},
            
            // Defense
            {"armour", "armour"},
            {"evasion", "evasion"},
            {"blockChance", "blockChance"},
            {"dodgeChance", "dodgeChance"},
            {"spellDodgeChance", "spellDodgeChance"},
            {"spellBlockChance", "spellBlockChance"},
            
            // Ailments
            {"chanceToIgnite", "chanceToIgnite"},
            {"chanceToShock", "chanceToShock"},
            {"chanceToChill", "chanceToChill"},
            {"chanceToFreeze", "chanceToFreeze"},
            {"chanceToBleed", "chanceToBleed"},
            {"chanceToPoison", "chanceToPoison"},
            {"increasedIgniteMagnitude", "increasedIgniteMagnitude"},
            {"increasedShockMagnitude", "increasedShockMagnitude"},
            {"increasedChillMagnitude", "increasedChillMagnitude"},
            {"increasedFreezeMagnitude", "increasedFreezeMagnitude"},
            {"increasedBleedMagnitude", "increasedBleedMagnitude"},
            {"increasedPoisonMagnitude", "increasedPoisonMagnitude"},
            
            // Recovery
            {"lifeRegeneration", "lifeRegeneration"},
            {"manaRegeneration", "manaRegeneration"},
            {"energyShieldRegeneration", "energyShieldRegeneration"},
            {"lifeLeech", "lifeLeech"},
            {"manaLeech", "manaLeech"},
            
            // Movement & Mechanics
            {"movementSpeed", "movementSpeed"},
            {"accuracy", "accuracy"},
            {"areaOfEffect", "areaOfEffect"},
            {"projectileSpeed", "projectileSpeed"},
            
            // Card System
            {"cardsDrawnPerTurn", "cardsDrawnPerTurn"},
            {"cardsDrawnPerWave", "cardsDrawnPerWave"},
            {"maxHandSize", "maxHandSize"},
            {"cardRetentionChance", "cardRetentionChance"},
            {"cardUpgradeChance", "cardUpgradeChance"},
            {"discardPower", "discardPower"}
        };
        
        return statMapping.ContainsKey(csvStatName) ? statMapping[csvStatName] : csvStatName;
    }
    
    private List<string> GetCompatibilityTags(string itemTypes)
    {
        List<string> tags = new List<string>();
        string lower = itemTypes.ToLower();
        
        // Parse explicit slot tags from CSV format like "Helmet Body Armour Gloves Boots Shield"
        string[] slotKeywords = { "helmet", "body armour", "gloves", "boots", "shield" };
        
        bool hasExplicitSlots = false;
        foreach (string keyword in slotKeywords)
        {
            if (lower.Contains(keyword))
            {
                // Convert to tag format
                string tag = keyword.Replace(" ", "_");
                tags.Add(tag);
                hasExplicitSlots = true;
            }
        }
        
        // If "Armour" or "Armor" is mentioned WITHOUT explicit slots, expand to ALL armor slots
        if (!hasExplicitSlots && (lower.Contains("armour") || lower.Contains("armor")))
        {
            // Expand generic "Armour" to all armor slots
            tags.Add("helmet");
            tags.Add("body_armour");
            tags.Add("gloves");
            tags.Add("boots");
            tags.Add("shield");
            Debug.Log($"  Expanded generic 'Armour' to all armor slots");
        }
        
        // Handle special base stat requirements
        if (lower.Contains("es base") || lower.Contains("energy shield base"))
        {
            tags.Add("energyshield_base");
        }
        if (lower.Contains("armour base") || lower.Contains("armor base"))
        {
            tags.Add("armour_base");
        }
        if (lower.Contains("evasion base"))
        {
            tags.Add("evasion_base");
        }
        
        // Handle jewelry
        if (lower.Contains("jewelry"))
        {
            tags.Add("jewelry");
        }
        
        // Handle weapons
        if (lower.Contains("weapon"))
        {
            tags.Add("weapon");
            
            // Handle weapon subtypes
            if (lower.Contains("caster weapon"))
            {
                tags.Add("caster");
            }
            else if (lower.Contains("ranged weapon"))
            {
                tags.Add("ranged");
            }
        }
        
        return tags;
    }
    
    /// <summary>
    /// Check if an affix is compatible with a specific item based on the item's base stats and tags
    /// </summary>
    public static bool IsAffixCompatibleWithItem(Affix affix, BaseItem item)
    {
        if (affix.compatibleTags == null || affix.compatibleTags.Count == 0)
            return true; // No restrictions
        
        foreach (string requiredTag in affix.compatibleTags)
        {
            if (requiredTag == "energyshield_base")
            {
                // Only compatible with items that have Energy Shield as base stat
                if (item is Armour armour && armour.energyShield > 0)
                    continue;
                else
                    return false;
            }
            else if (requiredTag == "armour_base")
            {
                // Only compatible with items that have Armour as base stat
                if (item is Armour armour && armour.armour > 0)
                    continue;
                else
                    return false;
            }
            else if (requiredTag == "evasion_base")
            {
                // Only compatible with items that have Evasion as base stat
                if (item is Armour armour && armour.evasion > 0)
                    continue;
                else
                    return false;
            }
            else
            {
                // Check item tags
                if (item.itemTags != null && item.itemTags.Contains(requiredTag))
                    continue;
                else
                    return false;
            }
        }
        
        return true;
    }
    
    private void DisplayCategoryCounts()
    {
        var categoryCounts = previewAffixes
            .GroupBy(a => a.category)
            .ToDictionary(g => g.Key, g => g.Count());
        
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Category Breakdown:", EditorStyles.miniBoldLabel);
        
        foreach (var kvp in categoryCounts.OrderByDescending(x => x.Value))
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"• {kvp.Key}:", GUILayout.Width(180));
            EditorGUILayout.LabelField($"{kvp.Value} affixes");
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private string SanitizeFolderName(string folderName)
    {
        return Regex.Replace(folderName, @"[^a-zA-Z0-9\s]", "").Replace(" ", "_");
    }
    
    private string SanitizeFileName(string fileName)
    {
        return Regex.Replace(fileName, @"[^a-zA-Z0-9\s]", "").Replace(" ", "_");
    }
}