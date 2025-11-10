using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

/// <summary>
/// CSV Importer for Jewelry (Amulets, Rings, Belts)
/// Reads a CSV file with jewelry data and creates Jewellery ScriptableObjects
/// </summary>
public class JewelryCSVImporter : EditorWindow
{
    private TextAsset csvFile;
    private string outputFolder = "Assets/Resources/Items/Jewellery";
    private bool createSubfolders = true;
    private bool overwriteExisting = false;
    
    // Jewelry type filters
    private bool filterByType = false;
    private bool importAmulets = true;
    private bool importRings = true;
    private bool importBelts = true;
    
    private Dictionary<JewelleryType, int> jewelryTypeCounts = new Dictionary<JewelleryType, int>();
    
    [MenuItem("Tools/Dexiled/Import Jewelry from CSV")]
    public static void ShowWindow()
    {
        GetWindow<JewelryCSVImporter>("Jewelry CSV Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Jewelry CSV Importer", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // CSV File Selection
        EditorGUILayout.BeginHorizontal();
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Select CSV File", "Assets/Resources/CSV", "csv");
            if (!string.IsNullOrEmpty(path))
            {
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
        createSubfolders = EditorGUILayout.Toggle("Create Subfolders by Type", createSubfolders);
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);
        
        EditorGUILayout.Space();
        
        // Type Filters
        GUILayout.Label("Jewelry Type Filters", EditorStyles.boldLabel);
        filterByType = EditorGUILayout.Toggle("Filter by Type", filterByType);
        
        if (filterByType)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                importAmulets = importRings = importBelts = true;
            }
            if (GUILayout.Button("Deselect All"))
            {
                importAmulets = importRings = importBelts = false;
            }
            EditorGUILayout.EndHorizontal();
            importAmulets = EditorGUILayout.Toggle("Import Amulets", importAmulets);
            importRings = EditorGUILayout.Toggle("Import Rings", importRings);
            importBelts = EditorGUILayout.Toggle("Import Belts", importBelts);
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Import Button
        EditorGUI.BeginDisabledGroup(csvFile == null);
        if (GUILayout.Button("Import Jewelry", GUILayout.Height(30)))
        {
            ImportJewelry();
        }
        EditorGUI.EndDisabledGroup();
        
        // Show counts
        if (jewelryTypeCounts.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Import Summary", EditorStyles.boldLabel);
            foreach (var kvp in jewelryTypeCounts)
            {
                EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value}");
            }
        }
    }
    
    private void ImportJewelry()
    {
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a CSV file", "OK");
            return;
        }
        
        // Reset counts
        jewelryTypeCounts.Clear();
        jewelryTypeCounts[JewelleryType.Amulet] = 0;
        jewelryTypeCounts[JewelleryType.Ring] = 0;
        jewelryTypeCounts[JewelleryType.Belt] = 0;
        
        // Parse CSV
        List<JewelryData> jewelryList = ParseCSV(csvFile.text);
        
        if (jewelryList.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "No jewelry items found in CSV", "OK");
            return;
        }
        
        // Create output directory
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        
        int imported = 0;
        int skipped = 0;
        
        foreach (JewelryData data in jewelryList)
        {
            // Filter by type if enabled
            if (filterByType)
            {
                bool shouldImport = false;
                switch (data.jewelleryType)
                {
                    case JewelleryType.Amulet:
                        shouldImport = importAmulets;
                        break;
                    case JewelleryType.Ring:
                        shouldImport = importRings;
                        break;
                    case JewelleryType.Belt:
                        shouldImport = importBelts;
                        break;
                }
                
                if (!shouldImport)
                {
                    skipped++;
                    continue;
                }
            }
            
            // Determine subfolder if needed
            string saveFolder = outputFolder;
            if (createSubfolders)
            {
                string typeFolder = data.jewelleryType.ToString();
                saveFolder = Path.Combine(outputFolder, typeFolder);
                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }
            }
            
            // Create asset path (ensure forward slashes for Unity)
            string fileName = data.name.Replace(" ", "_").Replace("/", "_") + ".asset";
            string assetPath = Path.Combine(saveFolder, fileName).Replace('\\', '/');
            
            // Remove quotes from path if any
            assetPath = assetPath.Replace("\"", "");
            
            // Check if already exists
            if (File.Exists(assetPath) && !overwriteExisting)
            {
                Debug.Log($"Skipping {data.name} - already exists");
                skipped++;
                continue;
            }
            
            // Create jewelry item
            Jewellery jewelry = CreateJewelryItem(data);
            
            // Save asset
            AssetDatabase.CreateAsset(jewelry, assetPath);
            imported++;
            jewelryTypeCounts[data.jewelleryType]++;
            
            Debug.Log($"Imported: {data.name} ({data.jewelleryType})");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        string message = $"Import Complete!\n\nImported: {imported} jewelry items\nSkipped: {skipped} items";
        EditorUtility.DisplayDialog("Import Complete", message, "OK");
        
        Debug.Log($"[JewelryCSVImporter] {message}");
    }
    
    private List<JewelryData> ParseCSV(string csvText)
    {
        List<JewelryData> jewelryList = new List<JewelryData>();
        string[] lines = csvText.Split('\n');
        
        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            JewelryData jewelry = ParseJewelryLine(line);
            if (jewelry != null)
            {
                jewelryList.Add(jewelry);
            }
        }
        
        return jewelryList;
    }
    
    private JewelryData ParseJewelryLine(string line)
    {
        // Split by comma, but handle commas within quotes
        string[] columns = ParseCSVLine(line);
        
        if (columns.Length < 3)
        {
            Debug.LogWarning($"[JewelryCSVImporter] Invalid line (not enough columns): {line}");
            return null;
        }
        
        JewelryData jewelry = new JewelryData();
        
        try
        {
            // Column 0: Name
            jewelry.name = columns[0].Trim();
            
            // Column 1: Requirements
            string requirements = columns[1].Trim();
            ParseRequirements(requirements, ref jewelry);
            
            // Column 2: Implicit
            string implicitDesc = columns[2].Trim();
            jewelry.implicitText = implicitDesc;
            
            // Determine jewelry type from name
            jewelry.jewelleryType = DetermineJewelryType(jewelry.name);
            jewelry.jewellerySlot = DetermineJewelrySlot(jewelry.jewelleryType);
            
            Debug.Log($"Parsed jewelry: {jewelry.name} - {jewelry.jewelleryType} (Level {jewelry.requiredLevel})");
            
            return jewelry;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing jewelry line: {line}\nError: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Parse a CSV line properly handling quoted fields
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
                    // Escaped quote
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
                currentField += c;
            }
        }
        
        // Add last field
        columns.Add(currentField);
        
        return columns.ToArray();
    }
    
    private void ParseRequirements(string requirementsStr, ref JewelryData jewelry)
    {
        if (string.IsNullOrEmpty(requirementsStr)) return;
        
        // Parse level requirement (e.g., "Requires Level 20")
        Match levelMatch = Regex.Match(requirementsStr, @"Level\s+(\d+)", RegexOptions.IgnoreCase);
        if (levelMatch.Success)
        {
            int.TryParse(levelMatch.Groups[1].Value, out jewelry.requiredLevel);
        }
        
        // Jewelry typically only has level requirements, but we can parse stats if needed
        // (Not in the current CSV format, but keeping for future compatibility)
    }
    
    private JewelleryType DetermineJewelryType(string name)
    {
        string lower = name.ToLower();
        
        if (lower.Contains("amulet"))
            return JewelleryType.Amulet;
        else if (lower.Contains("ring"))
            return JewelleryType.Ring;
        else if (lower.Contains("belt") || lower.Contains("sash"))
            return JewelleryType.Belt;
        
        // Default based on common patterns
        if (lower.Contains("amule") || lower.EndsWith("amulet"))
            return JewelleryType.Amulet;
        if (lower.EndsWith("ring"))
            return JewelleryType.Ring;
        
        Debug.LogWarning($"Could not determine jewelry type for: {name}, defaulting to Ring");
        return JewelleryType.Ring;
    }
    
    private JewellerySlot DetermineJewelrySlot(JewelleryType type)
    {
        switch (type)
        {
            case JewelleryType.Amulet:
                return JewellerySlot.Amulet;
            case JewelleryType.Ring:
                return JewellerySlot.LeftRing; // Default to left ring, can be changed manually
            case JewelleryType.Belt:
                return JewellerySlot.Belt;
            default:
                return JewellerySlot.LeftRing;
        }
    }
    
    private EquipmentType ConvertJewelrySlotToEquipmentType(JewellerySlot slot)
    {
        switch (slot)
        {
            case JewellerySlot.Amulet:
                return EquipmentType.Amulet;
            case JewellerySlot.LeftRing:
                return EquipmentType.LeftRing;
            case JewellerySlot.RightRing:
                return EquipmentType.RightRing;
            case JewellerySlot.Belt:
                return EquipmentType.Belt;
            default:
                return EquipmentType.Amulet;
        }
    }
    
    private Jewellery CreateJewelryItem(JewelryData data)
    {
        Jewellery jewelry = ScriptableObject.CreateInstance<Jewellery>();
        
        // Basic properties
        jewelry.itemName = data.name;
        jewelry.description = $"A {data.jewelleryType.ToString().ToLower()} with special properties.";
        jewelry.itemType = ItemType.Accessory;
        jewelry.equipmentType = ConvertJewelrySlotToEquipmentType(data.jewellerySlot);
        jewelry.jewelleryType = data.jewelleryType;
        jewelry.jewellerySlot = data.jewellerySlot;
        
        // Requirements
        jewelry.requiredLevel = data.requiredLevel;
        jewelry.requiredStrength = 0;
        jewelry.requiredDexterity = 0;
        jewelry.requiredIntelligence = 0;
        
        // Add proper tags
        AddJewelryTags(jewelry);
        
        // Rarity
        jewelry.rarity = ItemRarity.Normal;
        
        // Create implicit modifier
        jewelry.implicitModifiers = new List<Affix>();
        if (!string.IsNullOrEmpty(data.implicitText))
        {
            Affix implicitAffix = CreateImplicitAffix(data.implicitText);
            jewelry.implicitModifiers.Add(implicitAffix);
        }
        
        Debug.Log($"Created Jewelry: {data.name}");
        return jewelry;
    }
    
    private void AddJewelryTags(Jewellery jewelry)
    {
        if (jewelry.itemTags == null)
            jewelry.itemTags = new List<string>();
        
        jewelry.itemTags.Clear();
        
        // Base jewelry tags
        jewelry.itemTags.Add("jewelry");
        jewelry.itemTags.Add("accessory");
        jewelry.itemTags.Add(jewelry.jewelleryType.ToString().ToLower());
        
        // Add tags based on implicit modifiers
        foreach (var affix in jewelry.implicitModifiers)
        {
            foreach (var modifier in affix.modifiers)
            {
                string statName = modifier.statName.ToLower();
                
                // Add relevant tags based on stat type
                if (statName.Contains("strength")) jewelry.itemTags.Add("strength");
                if (statName.Contains("dexterity")) jewelry.itemTags.Add("dexterity");
                if (statName.Contains("intelligence")) jewelry.itemTags.Add("intelligence");
                if (statName.Contains("life") || statName.Contains("health")) jewelry.itemTags.Add("life");
                if (statName.Contains("mana")) jewelry.itemTags.Add("mana");
                if (statName.Contains("resistance")) jewelry.itemTags.Add("resistance");
                if (statName.Contains("damage")) jewelry.itemTags.Add("damage");
                if (statName.Contains("critical")) jewelry.itemTags.Add("critical");
            }
        }
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
        
        Debug.Log($"Parsing jewelry implicit: {description}");
        
        // Pattern: "+(16–24) to Strength and Dexterity" or "+(16-24) to Strength and Dexterity"
        Match dualStatMatch = Regex.Match(description, @"\+\((\d+)[–-](\d+)\)\s+to\s+(.+)\s+and\s+(.+)", RegexOptions.IgnoreCase);
        if (dualStatMatch.Success)
        {
            // For dual stats, we'll create a composite modifier or split into two
            // For now, we'll use the first stat and note both in the description
            float.TryParse(dualStatMatch.Groups[1].Value, out float minValue);
            float.TryParse(dualStatMatch.Groups[2].Value, out float maxValue);
            string stat1Name = MapStatName(dualStatMatch.Groups[3].Value.Trim());
            string stat2Name = MapStatName(dualStatMatch.Groups[4].Value.Trim());
            
            // Use a combined stat name or pick the first one
            // Note: This creates one modifier, you might want to create two separate ones
            return new AffixModifier(stat1Name, minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "+(20–30) to maximum Life" or "+(20-30) to maximum Life"
        Match lifeMatch = Regex.Match(description, @"\+\((\d+)[–-](\d+)\)\s+to\s+maximum\s+Life", RegexOptions.IgnoreCase);
        if (lifeMatch.Success)
        {
            float.TryParse(lifeMatch.Groups[1].Value, out float minValue);
            float.TryParse(lifeMatch.Groups[2].Value, out float maxValue);
            return new AffixModifier("maximumLife", minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "+(20–30) to maximum Mana"
        Match manaMatch = Regex.Match(description, @"\+\((\d+)[–-](\d+)\)\s+to\s+maximum\s+Mana", RegexOptions.IgnoreCase);
        if (manaMatch.Success)
        {
            float.TryParse(manaMatch.Groups[1].Value, out float minValue);
            float.TryParse(manaMatch.Groups[2].Value, out float maxValue);
            return new AffixModifier("maximumMana", minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "+(20–30)% to Fire Resistance"
        Match resistanceMatch = Regex.Match(description, @"\+\((\d+)[–-](\d+)\)%\s+to\s+(\w+)\s+Resistance", RegexOptions.IgnoreCase);
        if (resistanceMatch.Success)
        {
            float.TryParse(resistanceMatch.Groups[1].Value, out float minValue);
            float.TryParse(resistanceMatch.Groups[2].Value, out float maxValue);
            string element = resistanceMatch.Groups[3].Value.ToLower();
            string statName = element + "Resistance";
            return new AffixModifier(statName, minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "+(8–10)% to all Elemental Resistances"
        Match allResistanceMatch = Regex.Match(description, @"\+\((\d+)[–-](\d+)\)%\s+to\s+all\s+Elemental\s+Resistances", RegexOptions.IgnoreCase);
        if (allResistanceMatch.Success)
        {
            float.TryParse(allResistanceMatch.Groups[1].Value, out float minValue);
            float.TryParse(allResistanceMatch.Groups[2].Value, out float maxValue);
            return new AffixModifier("allResistance", minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "Adds 1 to 4 Physical Damage to Attacks"
        Match damageMatch = Regex.Match(description, @"Adds\s+(\d+)\s+to\s+(\d+)\s+(\w+)\s+Damage\s+to\s+Attacks", RegexOptions.IgnoreCase);
        if (damageMatch.Success)
        {
            float.TryParse(damageMatch.Groups[1].Value, out float minValue);
            float.TryParse(damageMatch.Groups[2].Value, out float maxValue);
            string damageType = damageMatch.Groups[3].Value.ToLower();
            string statName = "added" + damageType + "Damage";
            return new AffixModifier(statName, minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "+(16–24) to Strength" or "+(16-24) to Strength"
        Match singleStatMatch = Regex.Match(description, @"\+\((\d+)[–-](\d+)\)\s+to\s+(.+)", RegexOptions.IgnoreCase);
        if (singleStatMatch.Success)
        {
            float.TryParse(singleStatMatch.Groups[1].Value, out float minValue);
            float.TryParse(singleStatMatch.Groups[2].Value, out float maxValue);
            string statName = MapStatName(singleStatMatch.Groups[3].Value.Trim());
            return new AffixModifier(statName, minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "(20–30)% increased Global Critical Strike Chance" or "(20-30)% increased Global Critical Strike Chance"
        Match increasedMatch = Regex.Match(description, @"\((\d+)[–-](\d+)\)%\s+increased\s+(.+)", RegexOptions.IgnoreCase);
        if (increasedMatch.Success)
        {
            float.TryParse(increasedMatch.Groups[1].Value, out float minValue);
            float.TryParse(increasedMatch.Groups[2].Value, out float maxValue);
            string statName = MapStatName(increasedMatch.Groups[3].Value.Trim());
            return new AffixModifier(statName, minValue, maxValue, ModifierType.Increased);
        }
        
        // Pattern: "Regenerate (1.2–1.6)% of Life per turn"
        Match regenMatch = Regex.Match(description, @"Regenerate\s+\((\d+\.?\d*)[–-](\d+\.?\d*)\)%\s+of\s+Life\s+per\s+turn", RegexOptions.IgnoreCase);
        if (regenMatch.Success)
        {
            float.TryParse(regenMatch.Groups[1].Value, out float minValue);
            float.TryParse(regenMatch.Groups[2].Value, out float maxValue);
            return new AffixModifier("lifeRegenerationPercent", minValue, maxValue, ModifierType.Flat);
        }
        
        Debug.LogWarning($"Could not parse implicit modifier: {description}");
        return new AffixModifier("unknown", 0f, 0f, ModifierType.Flat);
    }
    
    private string MapStatName(string statName)
    {
        string lower = statName.ToLower();
        
        // Attributes
        if (lower.Contains("strength")) return "strength";
        if (lower.Contains("dexterity")) return "dexterity";
        if (lower.Contains("intelligence")) return "intelligence";
        if (lower.Contains("all attributes") || lower.Contains("all stats")) return "allAttributes";
        
        // Resistances
        if (lower.Contains("fire resistance")) return "fireResistance";
        if (lower.Contains("cold resistance")) return "coldResistance";
        if (lower.Contains("lightning resistance")) return "lightningResistance";
        if (lower.Contains("chaos resistance")) return "chaosResistance";
        
        // Critical
        if (lower.Contains("critical strike chance")) return "criticalStrikeChance";
        if (lower.Contains("critical strike multiplier")) return "criticalStrikeMultiplier";
        
        // Damage
        if (lower.Contains("global physical damage")) return "globalPhysicalDamage";
        if (lower.Contains("physical damage")) return "physicalDamage";
        
        // Rarity
        if (lower.Contains("rarity of items found")) return "itemRarity";
        
        // Default - return as-is (cleaned up)
        return lower.Replace(" ", "");
    }
    
    private class JewelryData
    {
        public string name;
        public int requiredLevel = 1;
        public string implicitText;
        public JewelleryType jewelleryType;
        public JewellerySlot jewellerySlot;
    }
}

