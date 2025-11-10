using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

/// <summary>
/// CSV Importer for Armor
/// Reads a CSV file with armor data and creates Armour ScriptableObjects
/// </summary>
public class ArmorCSVImporter : EditorWindow
{
    private TextAsset csvFile;
    private string outputFolder = "Assets/Resources/Items/Armour";
    private bool createSubfolders = true;
    private bool separateBySlot = false;
    private bool overwriteExisting = false;
    
    // Armor slot filters
    private bool filterBySlot = false;
    private bool importHelmets = true;
    private bool importBodyArmour = true;
    private bool importGloves = true;
    private bool importBoots = true;
    private bool importShields = true;
    
    // Defense type filters  
    private bool filterByDefenseType = false;
    private bool importArmour = true;
    private bool importEvasion = true;
    private bool importEnergyShield = true;
    private bool importHybrid = true; // Multiple defense types
    
    private Dictionary<ArmourSlot, int> armorSlotCounts = new Dictionary<ArmourSlot, int>();
    private Dictionary<string, int> defenseTypeCounts = new Dictionary<string, int>();
    
    [MenuItem("Tools/Dexiled/Import Armor from CSV")]
    public static void ShowWindow()
    {
        GetWindow<ArmorCSVImporter>("Armor CSV Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Armor CSV Importer", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // CSV File Selection
        EditorGUILayout.BeginHorizontal();
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Select CSV File", "Assets/Resources/CSV", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative path from Assets folder
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Output Settings
        EditorGUILayout.LabelField("Output Settings", EditorStyles.boldLabel);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        
        createSubfolders = EditorGUILayout.Toggle("Create Subfolders by Slot", createSubfolders);
        
        EditorGUI.BeginDisabledGroup(!createSubfolders);
        separateBySlot = EditorGUILayout.Toggle("  Separate by Slot Type", separateBySlot);
        EditorGUI.EndDisabledGroup();
        
        if (separateBySlot && !createSubfolders)
        {
            separateBySlot = false;
        }
        
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);
        
        EditorGUILayout.Space();
        
        // Armor Slot Filters
        GUILayout.Label("Armor Slot Filters", EditorStyles.boldLabel);
        filterBySlot = EditorGUILayout.Toggle("Filter by Armor Slot", filterBySlot);
        
        if (filterBySlot)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                importHelmets = importBodyArmour = importGloves = importBoots = importShields = true;
            }
            if (GUILayout.Button("Deselect All"))
            {
                importHelmets = importBodyArmour = importGloves = importBoots = importShields = false;
            }
            EditorGUILayout.EndHorizontal();
            
            importHelmets = EditorGUILayout.Toggle($"Helmets ({GetArmorSlotCount(ArmourSlot.Helmet)})", importHelmets);
            importBodyArmour = EditorGUILayout.Toggle($"Body Armour ({GetArmorSlotCount(ArmourSlot.BodyArmour)})", importBodyArmour);
            importGloves = EditorGUILayout.Toggle($"Gloves ({GetArmorSlotCount(ArmourSlot.Gloves)})", importGloves);
            importBoots = EditorGUILayout.Toggle($"Boots ({GetArmorSlotCount(ArmourSlot.Boots)})", importBoots);
            importShields = EditorGUILayout.Toggle($"Shields ({GetArmorSlotCount(ArmourSlot.Shield)})", importShields);
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Defense Type Filters
        GUILayout.Label("Defense Type Filters", EditorStyles.boldLabel);
        filterByDefenseType = EditorGUILayout.Toggle("Filter by Defense Type", filterByDefenseType);
        
        if (filterByDefenseType)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");
            importArmour = EditorGUILayout.Toggle($"Pure Armour ({GetDefenseTypeCount("Armour")})", importArmour);
            importEvasion = EditorGUILayout.Toggle($"Pure Evasion ({GetDefenseTypeCount("Evasion")})", importEvasion);
            importEnergyShield = EditorGUILayout.Toggle($"Pure Energy Shield ({GetDefenseTypeCount("EnergyShield")})", importEnergyShield);
            importHybrid = EditorGUILayout.Toggle($"Hybrid Defense ({GetDefenseTypeCount("Hybrid")})", importHybrid);
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Action Buttons
        if (GUILayout.Button("Preview Import"))
        {
            PreviewImport();
        }
        
        if (GUILayout.Button("Import Armor"))
        {
            ImportArmor();
        }
        
        EditorGUILayout.Space();
        
        // Help box
        EditorGUILayout.HelpBox(
            "CSV Format: Name, Defence, Defence 2, Defence 3, Requirements, Implicit 1, Implicit 2, Item Type\n\n" +
            "Defense columns can contain:\n" +
            "• Armour: 50-60 (or just Armour: 55)\n" +
            "• Evasion Rating: 100-120\n" +
            "• Energy Shield: 25-30\n\n" +
            "Item Type: Helmet, Body Armour, Gloves, Boots, Shield\n\n" +
            "Requirements: \"Requires Level 20, 30 Str, 25 Dex\"\n" +
            "Implicit modifiers: \"-1 Card draw\", \"+1 to Level of all Void Cards\"",
            MessageType.Info);
    }
    
    private void PreviewImport()
    {
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a CSV file first.", "OK");
            return;
        }
        
        List<ArmorData> armors = ParseCSV(csvFile.text);
        UpdateArmorSlotCounts(armors);
        UpdateDefenseTypeCounts(armors);
        
        // Apply filters
        List<ArmorData> filteredArmors = armors;
        if (filterBySlot)
        {
            filteredArmors = FilterArmorsBySlot(filteredArmors);
        }
        if (filterByDefenseType)
        {
            filteredArmors = FilterArmorsByDefenseType(filteredArmors);
        }
        
        string preview = $"Found {armors.Count} total armor pieces, {filteredArmors.Count} will be imported.\n\n";
        
        // Add slot breakdown
        preview += "SLOT BREAKDOWN:\n";
        foreach (var kvp in armorSlotCounts)
        {
            preview += $"• {kvp.Key}: {kvp.Value}\n";
        }
        preview += "\n";
        
        // Add defense type breakdown
        preview += "DEFENSE TYPE BREAKDOWN:\n";
        foreach (var kvp in defenseTypeCounts)
        {
            preview += $"• {kvp.Key}: {kvp.Value}\n";
        }
        preview += "\n";
        
        // Show first few items
        preview += "PREVIEW (first 5 items):\n";
        for (int i = 0; i < Mathf.Min(5, filteredArmors.Count); i++)
        {
            var armor = filteredArmors[i];
            preview += $"\n{i + 1}. {armor.name}\n";
            preview += $"   Slot: {armor.armorSlot}\n";
            preview += $"   Defense: Armour={armor.armour}, Evasion={armor.evasion}, Energy Shield={armor.energyShield}\n";
            preview += $"   Requirements: Lv{armor.requiredLevel}, {armor.requiredStr} Str, {armor.requiredDex} Dex, {armor.requiredInt} Int\n";
            preview += $"   Implicits: [{string.Join(", ", armor.implicits)}]\n";
        }
        
        EditorUtility.DisplayDialog("Import Preview", preview, "OK");
    }
    
    private void ImportArmor()
    {
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a CSV file first.", "OK");
            return;
        }
        
        List<ArmorData> armors = ParseCSV(csvFile.text);
        
        // Apply filters
        if (filterBySlot)
        {
            armors = FilterArmorsBySlot(armors);
        }
        if (filterByDefenseType)
        {
            armors = FilterArmorsByDefenseType(armors);
        }
        
        if (armors.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "No armor pieces match the current filters.", "OK");
            return;
        }
        
        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string[] pathParts = outputFolder.Split('/');
            string currentPath = pathParts[0];
            
            for (int i = 1; i < pathParts.Length; i++)
            {
                string newPath = currentPath + "/" + pathParts[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, pathParts[i]);
                }
                currentPath = newPath;
            }
        }
        
        int imported = 0;
        int skipped = 0;
        
        foreach (var armorData in armors)
        {
            string folderPath = outputFolder;
            
            // Create subfolder by armor slot if enabled
            if (createSubfolders)
            {
                folderPath = $"{outputFolder}/{armorData.armorSlot}s";
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder(outputFolder, $"{armorData.armorSlot}s");
                }
                
                // Create slot type subfolder if enabled
                if (separateBySlot)
                {
                    string defenseTypeFolder = GetDefenseTypeString(armorData);
                    string defenseTypeFolderPath = $"{folderPath}/{defenseTypeFolder}";
                    
                    if (!AssetDatabase.IsValidFolder(defenseTypeFolderPath))
                    {
                        AssetDatabase.CreateFolder(folderPath, defenseTypeFolder);
                    }
                    
                    folderPath = defenseTypeFolderPath;
                }
            }
            
            string assetPath = $"{folderPath}/{armorData.name}.asset";
            
            // Check if asset already exists
            if (!overwriteExisting && File.Exists(assetPath))
            {
                skipped++;
                continue;
            }
            
            // Create the armor item
            Armour armor = CreateArmorItem(armorData);
            
            // Save as asset
            AssetDatabase.CreateAsset(armor, assetPath);
            imported++;
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Import Complete", 
            $"Armor import completed!\n\nImported: {imported}\nSkipped: {skipped}", "OK");
    }
    
    private List<ArmorData> ParseCSV(string csvContent)
    {
        List<ArmorData> armors = new List<ArmorData>();
        string[] lines = csvContent.Split('\n');
        
        if (lines.Length < 2) return armors; // Need header + at least one data line
        
        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            ArmorData armor = ParseArmorLine(line);
            if (armor != null)
            {
                armors.Add(armor);
            }
        }
        
        return armors;
    }
    
    private ArmorData ParseArmorLine(string line)
    {
        // Parse CSV line properly handling empty fields
        string[] columns = ParseCSVLine(line);
        
        if (columns.Length < 8) // Need at least 8 columns
        {
            Debug.LogWarning($"Invalid CSV line (insufficient columns - got {columns.Length}, expected 8+): {line}");
            return null;
        }
        
        ArmorData armor = new ArmorData();
        
        try
        {
            // Extract values from columns
            armor.name = columns[0].Trim();
            string defence1 = columns[1].Trim();
            string defence2 = columns[2].Trim();  
            string defence3 = columns[3].Trim();
            string requirements = columns[4].Trim();
            string implicit1 = columns[5].Trim();
            string implicit2 = columns[6].Trim();
            
            // Handle different CSV formats - Item Type can be in column 7 or 8
            string itemType;
            if (columns.Length >= 9 && !string.IsNullOrEmpty(columns[8].Trim()))
            {
                // 9-column format (shields CSV) - Item Type in column 8
                itemType = columns[8].Trim();
                Debug.Log($"Using 9-column format, Item Type from column 8: '{itemType}'");
            }
            else
            {
                // 8-column format (regular armor CSV) - Item Type in column 7  
                itemType = columns[7].Trim();
                Debug.Log($"Using 8-column format, Item Type from column 7: '{itemType}'");
            }
            
            // Parse defense values
            ParseDefenseValues(defence1, ref armor);
            ParseDefenseValues(defence2, ref armor);
            ParseDefenseValues(defence3, ref armor);
            
            // Parse requirements
            ParseRequirements(requirements, ref armor);
            
            // Parse implicits
            armor.implicits = new List<string>();
            if (!string.IsNullOrEmpty(implicit1)) armor.implicits.Add(implicit1);
            if (!string.IsNullOrEmpty(implicit2)) armor.implicits.Add(implicit2);
            
            // Parse item type to armor slot
            armor.armorSlot = ParseArmorSlot(itemType);
            
            // Determine armor type based on defense values
            armor.armorType = DetermineArmorType(armor);
            
            Debug.Log($"Parsed armor: {armor.name} - {armor.armorSlot} - Armour:{armor.armour} Evasion:{armor.evasion} ES:{armor.energyShield} Block:{armor.blockChance}%");
            
            return armor;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing armor line: {line}\nError: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Parse a CSV line properly handling quoted fields and empty fields
    /// </summary>
    private string[] ParseCSVLine(string line)
    {
        List<string> columns = new List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Double quote - add single quote to field
                    currentField += '"';
                    i++; // Skip next quote
                }
                else
                {
                    // Toggle quote state
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                // End of field
                columns.Add(currentField);
                currentField = "";
            }
            else
            {
                // Add character to current field
                currentField += c;
            }
        }
        
        // Add the last field
        columns.Add(currentField);
        
        return columns.ToArray();
    }
    
    private void ParseDefenseValues(string defenseStr, ref ArmorData armor)
    {
        if (string.IsNullOrEmpty(defenseStr)) return;
        
        // Parse "Chance to Block: 24%" (for shields)
        Match blockMatch = Regex.Match(defenseStr, @"Chance to Block:\s*(\d+)%", RegexOptions.IgnoreCase);
        if (blockMatch.Success)
        {
            float.TryParse(blockMatch.Groups[1].Value, out float blockChance);
            armor.blockChance = blockChance;
            Debug.Log($"Found block chance: {blockChance}%");
            return;
        }
        
        // Parse "Armour: 50-60" or "Armour: 55"
        Match armorMatch = Regex.Match(defenseStr, @"Armour:\s*(\d+)(?:-(\d+))?", RegexOptions.IgnoreCase);
        if (armorMatch.Success)
        {
            float.TryParse(armorMatch.Groups[1].Value, out float minArmor);
            float maxArmor = minArmor;
            if (armorMatch.Groups[2].Success)
            {
                float.TryParse(armorMatch.Groups[2].Value, out maxArmor);
            }
            armor.armour = (minArmor + maxArmor) / 2f; // Use average for now
            return;
        }
        
        // Parse "Evasion Rating: 100-120" or "Evasion Rating: 110"
        Match evasionMatch = Regex.Match(defenseStr, @"Evasion Rating:\s*(\d+)(?:-(\d+))?", RegexOptions.IgnoreCase);
        if (evasionMatch.Success)
        {
            float.TryParse(evasionMatch.Groups[1].Value, out float minEvasion);
            float maxEvasion = minEvasion;
            if (evasionMatch.Groups[2].Success)
            {
                float.TryParse(evasionMatch.Groups[2].Value, out maxEvasion);
            }
            armor.evasion = (minEvasion + maxEvasion) / 2f;
            return;
        }
        
        // Parse "Energy Shield: 25-30" or "Energy Shield: 28"
        Match esMatch = Regex.Match(defenseStr, @"Energy Shield:\s*(\d+)(?:-(\d+))?", RegexOptions.IgnoreCase);
        if (esMatch.Success)
        {
            float.TryParse(esMatch.Groups[1].Value, out float minES);
            float maxES = minES;
            if (esMatch.Groups[2].Success)
            {
                float.TryParse(esMatch.Groups[2].Value, out maxES);
            }
            armor.energyShield = (minES + maxES) / 2f;
            return;
        }
    }
    
    private void ParseRequirements(string requirementsStr, ref ArmorData armor)
    {
        if (string.IsNullOrEmpty(requirementsStr)) return;
        
        // Parse level requirement
        Match levelMatch = Regex.Match(requirementsStr, @"Level\s+(\d+)", RegexOptions.IgnoreCase);
        if (levelMatch.Success)
        {
            int.TryParse(levelMatch.Groups[1].Value, out armor.requiredLevel);
        }
        
        // Parse strength requirement
        Match strMatch = Regex.Match(requirementsStr, @"(\d+)\s+Str", RegexOptions.IgnoreCase);
        if (strMatch.Success)
        {
            int.TryParse(strMatch.Groups[1].Value, out armor.requiredStr);
        }
        
        // Parse dexterity requirement
        Match dexMatch = Regex.Match(requirementsStr, @"(\d+)\s+Dex", RegexOptions.IgnoreCase);
        if (dexMatch.Success)
        {
            int.TryParse(dexMatch.Groups[1].Value, out armor.requiredDex);
        }
        
        // Parse intelligence requirement
        Match intMatch = Regex.Match(requirementsStr, @"(\d+)\s+Int", RegexOptions.IgnoreCase);
        if (intMatch.Success)
        {
            int.TryParse(intMatch.Groups[1].Value, out armor.requiredInt);
        }
    }
    
    private ArmourSlot ParseArmorSlot(string itemTypeStr)
    {
        string lower = itemTypeStr.ToLower().Trim();
        
        Debug.Log($"Parsing armor slot for: '{itemTypeStr}' -> '{lower}'");
        
        // Check specific types first before general patterns
        if (lower.Contains("shield")) return ArmourSlot.Shield;
        if (lower.Contains("helmet")) return ArmourSlot.Helmet;
        if (lower.Contains("gloves")) return ArmourSlot.Gloves;
        if (lower.Contains("boots")) return ArmourSlot.Boots;
        
        // Check for body armour last (and be more specific)
        if (lower.Contains("body") || 
            (lower.Contains("armour") && !lower.Contains("shield")) || 
            (lower.Contains("armor") && !lower.Contains("shield")))
        {
            return ArmourSlot.BodyArmour;
        }
        
        // Default fallback
        Debug.LogWarning($"Could not determine armor slot for '{itemTypeStr}', defaulting to Body Armour");
        return ArmourSlot.BodyArmour;
    }
    
    private ArmourType DetermineArmorType(ArmorData armor)
    {
        // Determine based on dominant defense type and name patterns
        bool hasArmor = armor.armour > 0;
        bool hasEvasion = armor.evasion > 0;
        bool hasES = armor.energyShield > 0;
        
        string nameLower = armor.name.ToLower();
        
        // Name-based detection first
        if (nameLower.Contains("plate") || nameLower.Contains("steel") || nameLower.Contains("iron"))
            return ArmourType.Plate;
        if (nameLower.Contains("chain") || nameLower.Contains("mail"))
            return ArmourType.Chain;
        if (nameLower.Contains("leather") || nameLower.Contains("hide"))
            return ArmourType.Leather;
        if (nameLower.Contains("cloth") || nameLower.Contains("robe"))
            return ArmourType.Cloth;
        if (nameLower.Contains("silk"))
            return ArmourType.Silk;
        if (nameLower.Contains("bone"))
            return ArmourType.Bone;
        if (nameLower.Contains("scale"))
            return ArmourType.Scale;
        
        // Defense-based detection
        if (hasArmor && !hasEvasion && !hasES)
            return ArmourType.Plate;
        if (hasEvasion && !hasArmor && !hasES)
            return ArmourType.Leather;
        if (hasES && !hasArmor && !hasEvasion)
            return ArmourType.Cloth;
        if (hasArmor && hasEvasion)
            return ArmourType.Scale;
        
        // Default
        return ArmourType.Leather;
    }
    
    private Armour CreateArmorItem(ArmorData data)
    {
        Armour armor = ScriptableObject.CreateInstance<Armour>();
        
        // Basic properties
        armor.itemName = data.name;
        armor.description = $"A {data.armorSlot.ToString().ToLower()} providing defense.";
        armor.itemType = ItemType.Armour;
        armor.armourSlot = data.armorSlot;
        armor.armourType = data.armorType;
        
        // Defense values
        armor.armour = data.armour;
        armor.evasion = data.evasion;
        armor.energyShield = data.energyShield;
        
        // Requirements
        armor.requiredLevel = data.requiredLevel;
        armor.requiredStrength = data.requiredStr;
        armor.requiredDexterity = data.requiredDex;
        armor.requiredIntelligence = data.requiredInt;
        
        // Determine equipment type
        armor.equipmentType = ConvertArmorSlotToEquipmentType(data.armorSlot);
        
        // Add proper tags
        AddArmorTags(armor);
        
        // Rarity
        armor.rarity = ItemRarity.Normal;
        
        // Create implicit modifiers
        armor.implicitModifiers = new List<Affix>();
        foreach (string implicitDesc in data.implicits)
        {
            if (!string.IsNullOrEmpty(implicitDesc))
            {
                Affix implicitAffix = CreateImplicitAffix(implicitDesc);
                armor.implicitModifiers.Add(implicitAffix);
            }
        }
        
        // For shields, add block chance as an implicit modifier
        if (data.blockChance > 0)
        {
            Affix blockAffix = new Affix("Block Chance", $"{data.blockChance}% Chance to Block", AffixType.Prefix, AffixTier.Tier1);
            AffixModifier blockModifier = new AffixModifier("blockChance", data.blockChance, data.blockChance, ModifierType.Flat);
            blockAffix.modifiers.Add(blockModifier);
            armor.implicitModifiers.Add(blockAffix);
            Debug.Log($"Added block chance {data.blockChance}% as implicit modifier");
        }
        
        Debug.Log($"Created Armor: {data.name}");
        return armor;
    }
    
    private EquipmentType ConvertArmorSlotToEquipmentType(ArmourSlot armourSlot)
    {
        switch (armourSlot)
        {
            case ArmourSlot.Helmet:
                return EquipmentType.Helmet;
            case ArmourSlot.BodyArmour:
                return EquipmentType.BodyArmour;
            case ArmourSlot.Gloves:
                return EquipmentType.Gloves;
            case ArmourSlot.Boots:
                return EquipmentType.Boots;
            case ArmourSlot.Shield:
                return EquipmentType.OffHand;
            default:
                return EquipmentType.BodyArmour;
        }
    }
    
    // Add proper tags to armor based on slot and properties
    private void AddArmorTags(Armour armor)
    {
        if (armor.itemTags == null)
            armor.itemTags = new List<string>();
        
        armor.itemTags.Clear();
        
        // Base armor tag
        armor.itemTags.Add("armour");
        armor.itemTags.Add("defence");
        
        // Armor slot tag
        armor.itemTags.Add(armor.armourSlot.ToString().ToLower());
        
        // Armor type tag
        armor.itemTags.Add(armor.armourType.ToString().ToLower());
        
        // Defense type tags
        if (armor.armour > 0)
            armor.itemTags.Add("armor");
        if (armor.evasion > 0)
            armor.itemTags.Add("evasion");
        if (armor.energyShield > 0)
            armor.itemTags.Add("energyshield");
            
        // Hybrid tag if has multiple defense types
        int defenseTypes = 0;
        if (armor.armour > 0) defenseTypes++;
        if (armor.evasion > 0) defenseTypes++;
        if (armor.energyShield > 0) defenseTypes++;
        
        if (defenseTypes > 1)
            armor.itemTags.Add("hybrid");
        
        // Add requirement-based tags
        if (armor.requiredStrength > 0)
            armor.itemTags.Add("strength");
        if (armor.requiredDexterity > 0)
            armor.itemTags.Add("dexterity");
        if (armor.requiredIntelligence > 0)
            armor.itemTags.Add("intelligence");
            
        // Add effect tags based on implicit modifiers
        foreach (var affix in armor.implicitModifiers)
        {
            foreach (var modifier in affix.modifiers)
            {
                AddEffectTags(armor.itemTags, modifier.statName);
            }
        }
    }
    
    // Add effect tags based on stat names (reuse from weapon importer)
    private void AddEffectTags(List<string> tags, string statName)
    {
        string lowerStat = statName.ToLower();
        
        // Card system tags
        if (lowerStat.Contains("carddraw") || lowerStat.Contains("carddrawn"))
            tags.Add("carddraw");
        if (lowerStat.Contains("cardlevel") || lowerStat.Contains("level"))
            tags.Add("cardlevel");
        if (lowerStat.Contains("discard"))
            tags.Add("discard");
        if (lowerStat.Contains("void"))
            tags.Add("void");
            
        // Defense tags
        if (lowerStat.Contains("resistance"))
            tags.Add("resistance");
        if (lowerStat.Contains("block"))
            tags.Add("block");
        if (lowerStat.Contains("dodge"))
            tags.Add("dodge");
    }
    
    private Affix CreateImplicitAffix(string description)
    {
        Affix affix = new Affix("Implicit", description, AffixType.Prefix, AffixTier.Tier1);
        
        // Parse the description to determine modifier type
        AffixModifier modifier = ParseImplicitModifier(description);
        if (modifier != null)
        {
            affix.modifiers.Add(modifier);
        }
        
        return affix;
    }
    
    private AffixModifier ParseImplicitModifier(string description)
    {
        if (string.IsNullOrEmpty(description))
            return new AffixModifier("Unknown", 0f, 0f, ModifierType.Flat);
        
        Debug.Log($"Parsing armor implicit: {description}");
        
        // Armor-specific pattern matching
        
        // Pattern: "-1 Card draw" or "+2 Card draw"
        Match cardDrawMatch = Regex.Match(description, @"([+-]?\d+)\s+Card\s+draw", RegexOptions.IgnoreCase);
        if (cardDrawMatch.Success)
        {
            float.TryParse(cardDrawMatch.Groups[1].Value, out float value);
            return new AffixModifier("cardsDrawnPerTurn", value, value, ModifierType.Flat);
        }
        
        // Pattern: "Select (1-2) additional Card to Discard"
        Match selectDiscardMatch = Regex.Match(description, @"Select\s+\((\d+)-(\d+)\)\s+additional\s+Card\s+to\s+Discard", RegexOptions.IgnoreCase);
        if (selectDiscardMatch.Success)
        {
            float.TryParse(selectDiscardMatch.Groups[1].Value, out float minValue);
            float.TryParse(selectDiscardMatch.Groups[2].Value, out float maxValue);
            return new AffixModifier("additionalCardDiscardSelection", minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "+1 to Level of all Void Cards"
        Match cardLevelMatch = Regex.Match(description, @"([+-]?\d+)\s+to\s+Level\s+of\s+all\s+(\w+)\s+Cards", RegexOptions.IgnoreCase);
        if (cardLevelMatch.Success)
        {
            float.TryParse(cardLevelMatch.Groups[1].Value, out float value);
            string cardType = cardLevelMatch.Groups[2].Value.ToLower();
            return new AffixModifier($"{cardType}CardLevelBonus", value, value, ModifierType.Flat);
        }
        
        // Pattern: "+(10-15)% to all Resistances"
        Match resistanceMatch = Regex.Match(description, @"\+\((\d+)-(\d+)\)%\s+to\s+all\s+Resistances", RegexOptions.IgnoreCase);
        if (resistanceMatch.Success)
        {
            float.TryParse(resistanceMatch.Groups[1].Value, out float minValue);
            float.TryParse(resistanceMatch.Groups[2].Value, out float maxValue);
            return new AffixModifier("allResistance", minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "10% increased Movement Speed"
        Match increasedStatMatch = Regex.Match(description, @"(\d+)%\s+increased\s+(.+)", RegexOptions.IgnoreCase);
        if (increasedStatMatch.Success)
        {
            float.TryParse(increasedStatMatch.Groups[1].Value, out float value);
            string statName = MapStatName(increasedStatMatch.Groups[2].Value.Trim());
            return new AffixModifier(statName, value, value, ModifierType.Increased);
        }
        
        // Pattern: "10% reduced Movement Speed"
        Match reducedStatMatch = Regex.Match(description, @"(\d+)%\s+reduced\s+(.+)", RegexOptions.IgnoreCase);
        if (reducedStatMatch.Success)
        {
            float.TryParse(reducedStatMatch.Groups[1].Value, out float value);
            string statName = MapStatName(reducedStatMatch.Groups[2].Value.Trim());
            return new AffixModifier(statName, value, value, ModifierType.Reduced);
        }
        
        // Pattern: "+(15-25) to Strength"
        Match flatStatMatch = Regex.Match(description, @"\+\((\d+)-(\d+)\)\s+to\s+(.+)", RegexOptions.IgnoreCase);
        if (flatStatMatch.Success)
        {
            float.TryParse(flatStatMatch.Groups[1].Value, out float minValue);
            float.TryParse(flatStatMatch.Groups[2].Value, out float maxValue);
            string statName = MapStatName(flatStatMatch.Groups[3].Value.Trim());
            return new AffixModifier(statName, minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "+15 to Strength"
        Match flatSingleMatch = Regex.Match(description, @"([+-]?\d+)\s+to\s+(.+)", RegexOptions.IgnoreCase);
        if (flatSingleMatch.Success)
        {
            float.TryParse(flatSingleMatch.Groups[1].Value, out float value);
            string statName = MapStatName(flatSingleMatch.Groups[2].Value.Trim());
            return new AffixModifier(statName, value, value, ModifierType.Flat);
        }
        
        // Fallback: create a descriptive modifier
        Debug.LogWarning($"Could not parse armor implicit modifier: {description}");
        return new AffixModifier(SanitizeStatName(description), 1f, 1f, ModifierType.Flat);
    }
    
    /// <summary>
    /// Map stat descriptions to actual CharacterStatsData field names (reuse from weapon importer)
    /// </summary>
    private string MapStatName(string statDescription)
    {
        string lower = statDescription.ToLower().Replace(" ", "").Replace("-", "");
        
        // Core attributes
        if (lower.Contains("strength")) return "strength";
        if (lower.Contains("dexterity")) return "dexterity"; 
        if (lower.Contains("intelligence")) return "intelligence";
        
        // Movement and speed
        if (lower.Contains("movementspeed")) return "movementSpeed";
        if (lower.Contains("attackspeed")) return "attackSpeed";
        if (lower.Contains("castspeed")) return "castSpeed";
        
        // Defense
        if (lower.Contains("armour")) return "armour";
        if (lower.Contains("evasion")) return "evasion";
        if (lower.Contains("energyshield")) return "energyShield";
        if (lower.Contains("blockchance")) return "blockChance";
        if (lower.Contains("dodgechance")) return "dodgeChance";
        
        // Resistances
        if (lower.Contains("allresistance") || lower.Contains("allresistances")) return "allResistance";
        if (lower.Contains("physicalresistance")) return "physicalResistance";
        if (lower.Contains("fireresistance")) return "fireResistance";
        if (lower.Contains("coldresistance")) return "coldResistance";
        if (lower.Contains("lightningresistance")) return "lightningResistance";
        if (lower.Contains("chaosresistance")) return "chaosResistance";
        if (lower.Contains("elementalresistance")) return "elementalResistance";
        
        // Card system
        if (lower.Contains("cardsdrawnperturn")) return "cardsDrawnPerTurn";
        if (lower.Contains("cardsdrawnperwave")) return "cardsDrawnPerWave";
        if (lower.Contains("maxhandsize")) return "maxHandSize";
        
        // Fallback: sanitize and return
        return SanitizeStatName(statDescription);
    }
    
    /// <summary>
    /// Sanitize stat name for use as a field name (reuse from weapon importer)
    /// </summary>
    private string SanitizeStatName(string statName)
    {
        // Remove special characters and spaces, convert to camelCase
        string sanitized = Regex.Replace(statName, @"[^a-zA-Z0-9\s]", "");
        sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
        
        if (string.IsNullOrEmpty(sanitized))
            return "unknownStat";
        
        // Convert to camelCase
        string[] words = sanitized.Split(' ');
        string result = words[0].ToLower();
        
        for (int i = 1; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
            {
                result += char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }
        
        return result;
    }
    
    private void UpdateArmorSlotCounts(List<ArmorData> armors)
    {
        armorSlotCounts.Clear();
        
        foreach (var armor in armors)
        {
            if (armorSlotCounts.ContainsKey(armor.armorSlot))
            {
                armorSlotCounts[armor.armorSlot]++;
            }
            else
            {
                armorSlotCounts[armor.armorSlot] = 1;
            }
        }
    }
    
    private void UpdateDefenseTypeCounts(List<ArmorData> armors)
    {
        defenseTypeCounts.Clear();
        defenseTypeCounts["Armour"] = 0;
        defenseTypeCounts["Evasion"] = 0;
        defenseTypeCounts["EnergyShield"] = 0;
        defenseTypeCounts["Hybrid"] = 0;
        
        foreach (var armor in armors)
        {
            bool hasArmor = armor.armour > 0;
            bool hasEvasion = armor.evasion > 0;
            bool hasES = armor.energyShield > 0;
            
            int defenseTypes = 0;
            if (hasArmor) defenseTypes++;
            if (hasEvasion) defenseTypes++;
            if (hasES) defenseTypes++;
            
            if (defenseTypes > 1)
            {
                defenseTypeCounts["Hybrid"]++;
            }
            else if (hasArmor)
            {
                defenseTypeCounts["Armour"]++;
            }
            else if (hasEvasion)
            {
                defenseTypeCounts["Evasion"]++;
            }
            else if (hasES)
            {
                defenseTypeCounts["EnergyShield"]++;
            }
        }
    }
    
    private int GetArmorSlotCount(ArmourSlot slot)
    {
        return armorSlotCounts.ContainsKey(slot) ? armorSlotCounts[slot] : 0;
    }
    
    private int GetDefenseTypeCount(string defenseType)
    {
        return defenseTypeCounts.ContainsKey(defenseType) ? defenseTypeCounts[defenseType] : 0;
    }
    
    private bool IsArmorSlotEnabled(ArmourSlot slot)
    {
        switch (slot)
        {
            case ArmourSlot.Helmet:
                return importHelmets;
            case ArmourSlot.BodyArmour:
                return importBodyArmour;
            case ArmourSlot.Gloves:
                return importGloves;
            case ArmourSlot.Boots:
                return importBoots;
            case ArmourSlot.Shield:
                return importShields;
            default:
                return true;
        }
    }
    
    private List<ArmorData> FilterArmorsBySlot(List<ArmorData> armors)
    {
        List<ArmorData> filtered = new List<ArmorData>();
        
        foreach (var armor in armors)
        {
            if (IsArmorSlotEnabled(armor.armorSlot))
            {
                filtered.Add(armor);
            }
        }
        
        return filtered;
    }
    
    private List<ArmorData> FilterArmorsByDefenseType(List<ArmorData> armors)
    {
        List<ArmorData> filtered = new List<ArmorData>();
        
        foreach (var armor in armors)
        {
            bool hasArmor = armor.armour > 0;
            bool hasEvasion = armor.evasion > 0;
            bool hasES = armor.energyShield > 0;
            
            int defenseTypes = 0;
            if (hasArmor) defenseTypes++;
            if (hasEvasion) defenseTypes++;
            if (hasES) defenseTypes++;
            
            bool shouldImport = false;
            
            if (defenseTypes > 1 && importHybrid)
            {
                shouldImport = true;
            }
            else if (hasArmor && !hasEvasion && !hasES && importArmour)
            {
                shouldImport = true;
            }
            else if (hasEvasion && !hasArmor && !hasES && importEvasion)
            {
                shouldImport = true;
            }
            else if (hasES && !hasArmor && !hasEvasion && importEnergyShield)
            {
                shouldImport = true;
            }
            
            if (shouldImport)
            {
                filtered.Add(armor);
            }
        }
        
        return filtered;
    }
    
    private string GetDefenseTypeString(ArmorData armor)
    {
        bool hasArmor = armor.armour > 0;
        bool hasEvasion = armor.evasion > 0;
        bool hasES = armor.energyShield > 0;
        
        int defenseTypes = 0;
        if (hasArmor) defenseTypes++;
        if (hasEvasion) defenseTypes++;
        if (hasES) defenseTypes++;
        
        if (defenseTypes > 1)
            return "Hybrid";
        else if (hasArmor)
            return "Armour";
        else if (hasEvasion)
            return "Evasion";
        else if (hasES)
            return "EnergyShield";
        else
            return "Unknown";
    }
}

// Data structure for parsed armor data
[System.Serializable]
public class ArmorData
{
    public string name;
    public float armour;
    public float evasion;
    public float energyShield;
    public float blockChance; // For shields
    public int requiredLevel;
    public int requiredStr;
    public int requiredDex;
    public int requiredInt;
    public List<string> implicits;
    public ArmourSlot armorSlot;
    public ArmourType armorType;
}
