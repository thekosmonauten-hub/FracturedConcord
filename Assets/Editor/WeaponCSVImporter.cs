using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// CSV Importer for Weapons
/// Reads a CSV file with weapon data and creates WeaponItem ScriptableObjects
/// </summary>
public class WeaponCSVImporter : EditorWindow
{
    private TextAsset csvFile;
    private string outputFolder = "Assets/Resources/Items/Weapons";
    private bool createSubfolders = true;
    private bool separateByHandedness = false;
    private bool overwriteExisting = false;
    
    // Weapon type filters
    private bool filterByType = false;
    private bool importMaces = true;
    private bool importSceptres = true;
    private bool importWands = true;
    private bool importDaggers = true;
    private bool importClaws = true;
    private bool importBows = true;
    private bool importAxes = true;
    private bool importSwords = true;
    private bool importStaffs = true;
    
    // Handedness filters
    private bool filterByHandedness = false;
    private bool importOneHanded = true;
    private bool importTwoHanded = true;
    
    private Vector2 scrollPosition;
    private string previewText = "";
    private int previewCount = 0;
    private System.Collections.Generic.Dictionary<WeaponItemType, int> weaponTypeCounts = new System.Collections.Generic.Dictionary<WeaponItemType, int>();
    
    [MenuItem("Tools/Dexiled/Import Weapons from CSV")]
    public static void ShowWindow()
    {
        GetWindow<WeaponCSVImporter>("Weapon CSV Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Weapon CSV Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // CSV File Selection
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        
        EditorGUILayout.Space();
        GUILayout.Label("Import Settings", EditorStyles.boldLabel);
        
        // Output folder
        EditorGUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative path
                if (path.StartsWith(Application.dataPath))
                {
                    outputFolder = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        createSubfolders = EditorGUILayout.Toggle("Create Subfolders by Type", createSubfolders);
        
        EditorGUI.BeginDisabledGroup(!createSubfolders);
        separateByHandedness = EditorGUILayout.Toggle("  Separate by Handedness", separateByHandedness);
        EditorGUI.EndDisabledGroup();
        
        if (separateByHandedness && !createSubfolders)
        {
            separateByHandedness = false; // Auto-disable if subfolders are disabled
        }
        
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);
        
        EditorGUILayout.Space();
        
        // Weapon Type Filters
        GUILayout.Label("Weapon Type Filters", EditorStyles.boldLabel);
        filterByType = EditorGUILayout.Toggle("Filter by Weapon Type", filterByType);
        
        if (filterByType)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All", GUILayout.Width(100)))
            {
                importMaces = importSceptres = importWands = importDaggers = true;
                importClaws = importBows = importAxes = importSwords = importStaffs = true;
            }
            if (GUILayout.Button("Deselect All", GUILayout.Width(100)))
            {
                importMaces = importSceptres = importWands = importDaggers = false;
                importClaws = importBows = importAxes = importSwords = importStaffs = false;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginVertical("box");
            importMaces = EditorGUILayout.Toggle($"Maces ({GetWeaponTypeCount(WeaponItemType.Mace)})", importMaces);
            importSceptres = EditorGUILayout.Toggle($"Sceptres ({GetWeaponTypeCount(WeaponItemType.Sceptre)})", importSceptres);
            importWands = EditorGUILayout.Toggle($"Wands ({GetWeaponTypeCount(WeaponItemType.Wand)})", importWands);
            importDaggers = EditorGUILayout.Toggle($"Daggers ({GetWeaponTypeCount(WeaponItemType.Dagger)})", importDaggers);
            importClaws = EditorGUILayout.Toggle($"Claws ({GetWeaponTypeCount(WeaponItemType.Claw)})", importClaws);
            importBows = EditorGUILayout.Toggle($"Bows ({GetWeaponTypeCount(WeaponItemType.Bow)})", importBows);
            importAxes = EditorGUILayout.Toggle($"Axes ({GetWeaponTypeCount(WeaponItemType.Axe)})", importAxes);
            importSwords = EditorGUILayout.Toggle($"Swords ({GetWeaponTypeCount(WeaponItemType.Sword)})", importSwords);
            importStaffs = EditorGUILayout.Toggle($"Staves ({GetWeaponTypeCount(WeaponItemType.Staff)})", importStaffs);
            EditorGUILayout.EndVertical();
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Handedness Filters
        GUILayout.Label("Handedness Filters", EditorStyles.boldLabel);
        filterByHandedness = EditorGUILayout.Toggle("Filter by Handedness", filterByHandedness);
        
        if (filterByHandedness)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");
            importOneHanded = EditorGUILayout.Toggle($"One Handed ({GetHandednessCount(WeaponHandedness.OneHanded)})", importOneHanded);
            importTwoHanded = EditorGUILayout.Toggle($"Two Handed ({GetHandednessCount(WeaponHandedness.TwoHanded)})", importTwoHanded);
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Preview Button
        if (GUILayout.Button("Preview Import", GUILayout.Height(30)))
        {
            PreviewImport();
        }
        
        // Preview Area
        if (!string.IsNullOrEmpty(previewText))
        {
            EditorGUILayout.Space();
            GUILayout.Label($"Preview ({previewCount} weapons found):", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            EditorGUILayout.TextArea(previewText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
        
        EditorGUILayout.Space();
        
        // Import Button
        GUI.enabled = csvFile != null;
        if (GUILayout.Button("Import Weapons", GUILayout.Height(40)))
        {
            ImportWeapons();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        
        // Help Box
        EditorGUILayout.HelpBox(
            "CSV Format:\n" +
            "Name, Damage, Critical Strike Chance, Attack Speed, Requirements, Implicit 1, Implicit 2, Weapon Type, Handedness\n\n" +
            "Example:\n" +
            "Weathered Club, Physical Damage: 6-8, Critical Strike Chance: 5%, Attack Speed: 1.45, \"Requires 14 Str\", 10% reduced Enemy Stagger Threshold, , Mace, One Handed\n\n" +
            "IMPORTANT: Wrap Requirements in quotes if it contains commas!",
            MessageType.Info);
    }
    
    private void PreviewImport()
    {
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a CSV file", "OK");
            return;
        }
        
        List<WeaponData> weapons = ParseCSV(csvFile.text);
        
        // Update weapon type counts
        UpdateWeaponTypeCounts(weapons);
        
        // Filter weapons if needed
        if (filterByType)
        {
            weapons = FilterWeaponsByType(weapons);
        }
        
        if (filterByHandedness)
        {
            weapons = FilterWeaponsByHandedness(weapons);
        }
        
        previewCount = weapons.Count;
        
        previewText = "";
        
        // Show summary by weapon type
        previewText += "=== WEAPON TYPE BREAKDOWN ===\n";
        foreach (var kvp in weaponTypeCounts.OrderByDescending(x => x.Value))
        {
            bool willImport = filterByType ? IsWeaponTypeEnabled(kvp.Key) : true;
            string status = willImport ? "[WILL IMPORT]" : "[SKIPPED]";
            previewText += $"{status} {kvp.Key}: {kvp.Value} weapons\n";
        }
        
        previewText += "\n=== HANDEDNESS BREAKDOWN ===\n";
        int oneHandedCount = GetHandednessCount(WeaponHandedness.OneHanded);
        int twoHandedCount = GetHandednessCount(WeaponHandedness.TwoHanded);
        bool willImportOneHanded = filterByHandedness ? importOneHanded : true;
        bool willImportTwoHanded = filterByHandedness ? importTwoHanded : true;
        previewText += $"{(willImportOneHanded ? "[WILL IMPORT]" : "[SKIPPED]")} One Handed: {oneHandedCount} weapons\n";
        previewText += $"{(willImportTwoHanded ? "[WILL IMPORT]" : "[SKIPPED]")} Two Handed: {twoHandedCount} weapons\n";
        
        previewText += $"\nTotal to import: {weapons.Count}\n\n";
        previewText += "=== WEAPON PREVIEW (First 10) ===\n";
        
        foreach (var weapon in weapons.Take(10))
        {
            previewText += $"\nName: {weapon.name}\n";
            previewText += $"  Type: {weapon.weaponType} ({weapon.handedness})\n";
            previewText += $"  Damage: {weapon.minDamage}-{weapon.maxDamage}\n";
            previewText += $"  Attack Speed: {weapon.attackSpeed}\n";
            previewText += $"  Crit Chance: {weapon.critChance}%\n";
            if (weapon.requiredLevel > 1) previewText += $"  Required Level: {weapon.requiredLevel}\n";
            if (weapon.requiredStr > 0) previewText += $"  Required Str: {weapon.requiredStr}\n";
            if (weapon.requiredDex > 0) previewText += $"  Required Dex: {weapon.requiredDex}\n";
            if (weapon.requiredInt > 0) previewText += $"  Required Int: {weapon.requiredInt}\n";
            if (weapon.implicits.Count > 0) previewText += $"  Implicits: {string.Join(", ", weapon.implicits)}\n";
        }
        
        if (weapons.Count > 10)
        {
            previewText += $"\n... and {weapons.Count - 10} more weapons\n";
        }
    }
    
    private void ImportWeapons()
    {
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a CSV file", "OK");
            return;
        }
        
        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string[] folders = outputFolder.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
        
        List<WeaponData> weapons = ParseCSV(csvFile.text);
        
        // Update weapon type counts
        UpdateWeaponTypeCounts(weapons);
        
        // Filter weapons if needed
        if (filterByType)
        {
            weapons = FilterWeaponsByType(weapons);
        }
        
        if (filterByHandedness)
        {
            weapons = FilterWeaponsByHandedness(weapons);
        }
        
        int imported = 0;
        int skipped = 0;
        
        foreach (var weaponData in weapons)
        {
            string folderPath = outputFolder;
            
            // Create subfolder by weapon type if enabled
            if (createSubfolders)
            {
                folderPath = $"{outputFolder}/{weaponData.weaponType}s";
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder(outputFolder, $"{weaponData.weaponType}s");
                }
                
                // Create handedness subfolder if enabled
                if (separateByHandedness)
                {
                    string handednessFolder = weaponData.handedness == WeaponHandedness.OneHanded ? "OneHanded" : "TwoHanded";
                    string handednessFolderPath = $"{folderPath}/{handednessFolder}";
                    
                    if (!AssetDatabase.IsValidFolder(handednessFolderPath))
                    {
                        AssetDatabase.CreateFolder(folderPath, handednessFolder);
                    }
                    
                    folderPath = handednessFolderPath;
                }
            }
            
            string assetPath = $"{folderPath}/{weaponData.name}.asset";
            
            // Check if asset already exists
            if (!overwriteExisting && File.Exists(assetPath))
            {
                skipped++;
                continue;
            }
            
            // Create WeaponItem
            WeaponItem weapon = CreateWeaponItem(weaponData);
            
            // Save asset
            if (File.Exists(assetPath))
            {
                // Overwrite existing
                AssetDatabase.DeleteAsset(assetPath);
            }
            
            AssetDatabase.CreateAsset(weapon, assetPath);
            imported++;
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        string message = $"Import Complete!\n\nImported: {imported} weapons\nSkipped: {skipped} weapons";
        EditorUtility.DisplayDialog("Import Complete", message, "OK");
        
        Debug.Log($"[WeaponCSVImporter] {message}");
    }
    
    private List<WeaponData> ParseCSV(string csvText)
    {
        List<WeaponData> weapons = new List<WeaponData>();
        string[] lines = csvText.Split('\n');
        
        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            WeaponData weapon = ParseWeaponLine(line);
            if (weapon != null)
            {
                weapons.Add(weapon);
            }
        }
        
        return weapons;
    }
    
    private WeaponData ParseWeaponLine(string line)
    {
        // Split by comma, but handle commas within quotes
        string[] parts = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
        
        if (parts.Length < 6)
        {
            Debug.LogWarning($"[WeaponCSVImporter] Invalid line (not enough columns): {line}");
            return null;
        }
        
        WeaponData weapon = new WeaponData();
        
        // Parse Name (Column 0)
        weapon.name = parts[0].Trim();
        
        // Parse Damage (Column 1) - e.g., "Physical Damage: 6-8"
        string damageStr = parts[1].Trim();
        ParseDamage(damageStr, out weapon.minDamage, out weapon.maxDamage);
        
        // Parse Critical Strike Chance (Column 2) - e.g., "Critical Strike Chance: 5%"
        string critStr = parts[2].Trim();
        weapon.critChance = ParsePercentage(critStr);
        
        // Parse Attack Speed (Column 3) - e.g., "Attack Speed: 1.45"
        string attackSpeedStr = parts[3].Trim();
        weapon.attackSpeed = ParseFloat(attackSpeedStr);
        
        // Parse Requirements (Column 4) - e.g., "Requires Level 5, 26 Str"
        string requirementsStr = parts[4].Trim();
        ParseRequirements(requirementsStr, weapon);
        
        // Parse Implicit 1 (Column 5)
        if (parts.Length > 5 && !string.IsNullOrEmpty(parts[5].Trim()))
        {
            weapon.implicits.Add(parts[5].Trim());
        }
        
        // Parse Implicit 2 (Column 6)
        if (parts.Length > 6 && !string.IsNullOrEmpty(parts[6].Trim()))
        {
            weapon.implicits.Add(parts[6].Trim());
        }
        
        // Parse Weapon Type (Column 7) - e.g., "Mace", "Bow", "Wand"
        if (parts.Length > 7 && !string.IsNullOrEmpty(parts[7].Trim()))
        {
            weapon.weaponType = ParseWeaponType(parts[7].Trim());
        }
        else
        {
            // Fall back to auto-detection from name
            weapon.weaponType = DetermineWeaponType(weapon.name);
        }
        
        // Parse Handedness (Column 8) - e.g., "One Handed", "Two Handed"
        if (parts.Length > 8 && !string.IsNullOrEmpty(parts[8].Trim()))
        {
            weapon.handedness = ParseHandedness(parts[8].Trim());
        }
        else
        {
            // Fall back to auto-detection from weapon type
            weapon.handedness = DetermineHandedness(weapon.weaponType);
        }
        
        return weapon;
    }
    
    private void ParseDamage(string damageStr, out float minDamage, out float maxDamage)
    {
        minDamage = 0;
        maxDamage = 0;
        
        // Extract damage range: "Physical Damage: 6-8" -> "6-8"
        Match match = Regex.Match(damageStr, @"(\d+)-(\d+)");
        if (match.Success)
        {
            float.TryParse(match.Groups[1].Value, out minDamage);
            float.TryParse(match.Groups[2].Value, out maxDamage);
        }
    }
    
    private float ParsePercentage(string str)
    {
        // Extract percentage: "Critical Strike Chance: 5%" -> 5
        Match match = Regex.Match(str, @"([\d.]+)%");
        if (match.Success)
        {
            float.TryParse(match.Groups[1].Value, out float value);
            return value;
        }
        return 5.0f; // Default
    }
    
    private float ParseFloat(string str)
    {
        // Extract float: "Attack Speed: 1.45" -> 1.45
        Match match = Regex.Match(str, @"([\d.]+)");
        if (match.Success)
        {
            float.TryParse(match.Groups[1].Value, out float value);
            return value;
        }
        return 1.0f; // Default
    }
    
    private void ParseRequirements(string requirementsStr, WeaponData weapon)
    {
        // Parse level: "Requires Level 5"
        Match levelMatch = Regex.Match(requirementsStr, @"Level\s+(\d+)");
        if (levelMatch.Success)
        {
            int.TryParse(levelMatch.Groups[1].Value, out weapon.requiredLevel);
        }
        
        // Parse Str: "26 Str"
        Match strMatch = Regex.Match(requirementsStr, @"(\d+)\s+Str");
        if (strMatch.Success)
        {
            int.TryParse(strMatch.Groups[1].Value, out weapon.requiredStr);
        }
        
        // Parse Dex: "26 Dex"
        Match dexMatch = Regex.Match(requirementsStr, @"(\d+)\s+Dex");
        if (dexMatch.Success)
        {
            int.TryParse(dexMatch.Groups[1].Value, out weapon.requiredDex);
        }
        
        // Parse Int: "26 Int"
        Match intMatch = Regex.Match(requirementsStr, @"(\d+)\s+Int");
        if (intMatch.Success)
        {
            int.TryParse(intMatch.Groups[1].Value, out weapon.requiredInt);
        }
    }
    
    private WeaponItemType DetermineWeaponType(string name)
    {
        // Check weapon type based on name keywords
        string nameLower = name.ToLower();
        
        if (nameLower.Contains("bow")) return WeaponItemType.Bow;
        if (nameLower.Contains("claw")) return WeaponItemType.Claw;
        if (nameLower.Contains("dagger") || nameLower.Contains("knife") || nameLower.Contains("stiletto")) return WeaponItemType.Dagger;
        if (nameLower.Contains("mace") || nameLower.Contains("club") || nameLower.Contains("maul") || 
            nameLower.Contains("hammer") || nameLower.Contains("mallet") || nameLower.Contains("gavel")) return WeaponItemType.Mace;
        if (nameLower.Contains("sceptre") || nameLower.Contains("rod") || nameLower.Contains("baton") || 
            nameLower.Contains("sekhem") || nameLower.Contains("fetish") || nameLower.Contains("idol")) return WeaponItemType.Sceptre;
        if (nameLower.Contains("wand") || nameLower.Contains("fang") && !nameLower.Contains("claw")) return WeaponItemType.Wand;
        if (nameLower.Contains("staff")) return WeaponItemType.Staff;
        if (nameLower.Contains("sword") || nameLower.Contains("blade")) return WeaponItemType.Sword;
        if (nameLower.Contains("axe")) return WeaponItemType.Axe;
        
        // Default to Sword
        return WeaponItemType.Sword;
    }
    
    private WeaponItem CreateWeaponItem(WeaponData data)
    {
        WeaponItem weapon = ScriptableObject.CreateInstance<WeaponItem>();
        
        // Basic properties
        weapon.itemName = data.name;
        weapon.description = $"A {data.weaponType.ToString().ToLower()} weapon.";
        weapon.weaponType = data.weaponType;
        weapon.itemType = ItemType.Weapon;
        weapon.equipmentType = EquipmentType.MainHand;
        
        // Damage
        weapon.minDamage = data.minDamage;
        weapon.maxDamage = data.maxDamage;
        weapon.primaryDamageType = DamageType.Physical;
        
        // Stats
        weapon.attackSpeed = data.attackSpeed;
        weapon.criticalStrikeChance = data.critChance;
        weapon.criticalStrikeMultiplier = 150f; // Default
        
        // Requirements
        weapon.requiredLevel = data.requiredLevel;
        weapon.requiredStrength = data.requiredStr;
        weapon.requiredDexterity = data.requiredDex;
        weapon.requiredIntelligence = data.requiredInt;
        
        // Use handedness from CSV data
        weapon.handedness = data.handedness;
        
        // Add proper tags
        AddWeaponTags(weapon);
        
        // Rarity
        weapon.rarity = ItemRarity.Normal;
        
        // Create implicit modifiers
        weapon.implicitModifiers = new List<Affix>();
        foreach (string implicitDesc in data.implicits)
        {
            if (!string.IsNullOrEmpty(implicitDesc))
            {
                Affix implicitAffix = CreateImplicitAffix(implicitDesc);
                weapon.implicitModifiers.Add(implicitAffix);
            }
        }
        
        return weapon;
    }
    
    // Add proper tags to weapon based on type and properties
    private void AddWeaponTags(WeaponItem weapon)
    {
        if (weapon.itemTags == null)
            weapon.itemTags = new List<string>();
        
        weapon.itemTags.Clear();
        
        // Base weapon tag
        weapon.itemTags.Add("weapon");
        
        // Weapon type tag
        weapon.itemTags.Add(weapon.weaponType.ToString().ToLower());
        
        // Handedness tag
        weapon.itemTags.Add(weapon.handedness.ToString().ToLower());
        
        // Combat style tags based on weapon type
        switch (weapon.weaponType)
        {
            case WeaponItemType.Sword:
            case WeaponItemType.Axe:
            case WeaponItemType.Mace:
            case WeaponItemType.Dagger:
            case WeaponItemType.Claw:
                weapon.itemTags.Add("melee");
                weapon.itemTags.Add("attack");
                break;
            case WeaponItemType.Bow:
                weapon.itemTags.Add("ranged");
                weapon.itemTags.Add("attack");
                break;
            case WeaponItemType.Wand:
                weapon.itemTags.Add("ranged");
                weapon.itemTags.Add("spell");
                break;
            case WeaponItemType.Staff:
            case WeaponItemType.Sceptre:
                weapon.itemTags.Add("spell");
                break;
        }
        
        // Add requirement-based tags
        if (weapon.requiredStrength > 0)
            weapon.itemTags.Add("strength");
        if (weapon.requiredDexterity > 0)
            weapon.itemTags.Add("dexterity");
        if (weapon.requiredIntelligence > 0)
            weapon.itemTags.Add("intelligence");
            
        // Add effect tags based on implicit modifiers
        foreach (var affix in weapon.implicitModifiers)
        {
            foreach (var modifier in affix.modifiers)
            {
                AddEffectTags(weapon.itemTags, modifier.statName);
            }
        }
    }
    
    // Add effect tags based on stat names
    private void AddEffectTags(List<string> tags, string statName)
    {
        string lowerStat = statName.ToLower();
        
        // Ailment tags
        if (lowerStat.Contains("bleed"))
            tags.Add("bleed");
        if (lowerStat.Contains("shock"))
            tags.Add("shock");
        if (lowerStat.Contains("chill"))
            tags.Add("chill");
        if (lowerStat.Contains("freeze"))
            tags.Add("freeze");
        if (lowerStat.Contains("ignite"))
            tags.Add("ignite");
        if (lowerStat.Contains("poison"))
            tags.Add("poison");
        if (lowerStat.Contains("crumble"))
            tags.Add("crumble");
        if (lowerStat.Contains("vulnerable"))
            tags.Add("vulnerable");
            
        // Damage type tags
        if (lowerStat.Contains("physical"))
            tags.Add("physical");
        if (lowerStat.Contains("fire"))
            tags.Add("fire");
        if (lowerStat.Contains("cold"))
            tags.Add("cold");
        if (lowerStat.Contains("lightning"))
            tags.Add("lightning");
        if (lowerStat.Contains("chaos"))
            tags.Add("chaos");
            
        // Card system tags
        if (lowerStat.Contains("carddraw") || lowerStat.Contains("carddrawn"))
            tags.Add("carddraw");
    }
    
    private WeaponHandedness DetermineHandedness(WeaponItemType weaponType)
    {
        // Most weapons are one-handed by default
        // Two-handed: Bows, Staves, some large weapons
        switch (weaponType)
        {
            case WeaponItemType.Bow:
            case WeaponItemType.Staff:
                return WeaponHandedness.TwoHanded;
            default:
                return WeaponHandedness.OneHanded;
        }
    }
    
    private WeaponItemType ParseWeaponType(string typeStr)
    {
        // Parse weapon type from CSV string
        typeStr = typeStr.Replace(" ", "").ToLower();
        
        if (typeStr.Contains("bow")) return WeaponItemType.Bow;
        if (typeStr.Contains("claw")) return WeaponItemType.Claw;
        if (typeStr.Contains("dagger")) return WeaponItemType.Dagger;
        if (typeStr.Contains("ritualdagger")) return WeaponItemType.RitualDagger;
        if (typeStr.Contains("mace")) return WeaponItemType.Mace;
        if (typeStr.Contains("sceptre")) return WeaponItemType.Sceptre;
        if (typeStr.Contains("wand")) return WeaponItemType.Wand;
        if (typeStr.Contains("staff")) return WeaponItemType.Staff;
        if (typeStr.Contains("sword")) return WeaponItemType.Sword;
        if (typeStr.Contains("axe")) return WeaponItemType.Axe;
        
        // Default to Sword
        return WeaponItemType.Sword;
    }
    
    private WeaponHandedness ParseHandedness(string handednessStr)
    {
        // Parse handedness from CSV string
        handednessStr = handednessStr.Replace(" ", "").ToLower();
        
        if (handednessStr.Contains("two") || handednessStr.Contains("2"))
        {
            return WeaponHandedness.TwoHanded;
        }
        
        return WeaponHandedness.OneHanded;
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
        
        Debug.Log($"Parsing implicit: {description}");
        
        // Comprehensive pattern matching for your character stat system
        
        // Pattern: "+(1-2) Wave card draw" or "+(1-2) End Turn Card Draw"
        Match cardDrawMatch = Regex.Match(description, @"\+\((\d+)-(\d+)\)\s+(Wave card draw|End Turn Card Draw)", RegexOptions.IgnoreCase);
        if (cardDrawMatch.Success)
        {
            float.TryParse(cardDrawMatch.Groups[1].Value, out float minValue);
            float.TryParse(cardDrawMatch.Groups[2].Value, out float maxValue);
            string drawType = cardDrawMatch.Groups[3].Value.ToLower();
            
            if (drawType.Contains("wave"))
                return new AffixModifier("cardsDrawnPerWave", minValue, maxValue, ModifierType.Flat);
            else
                return new AffixModifier("cardsDrawnPerTurn", minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "20% chance to apply Bleed on hit"
        Match applyAilmentMatch = Regex.Match(description, @"(\d+)%\s+chance\s+to\s+apply\s+(\w+)\s+on\s+hit", RegexOptions.IgnoreCase);
        if (applyAilmentMatch.Success)
        {
            float.TryParse(applyAilmentMatch.Groups[1].Value, out float value);
            string ailment = applyAilmentMatch.Groups[2].Value.ToLower();
            
            return new AffixModifier($"chanceTo{char.ToUpper(ailment[0])}{ailment.Substring(1)}", value, value, ModifierType.Flat);
        }
        
        // Pattern: "20% chance to cause enemies hit to Crumble"
        Match causeAilmentMatch = Regex.Match(description, @"(\d+)%\s+chance\s+to\s+cause\s+enemies\s+hit\s+to\s+(\w+)", RegexOptions.IgnoreCase);
        if (causeAilmentMatch.Success)
        {
            float.TryParse(causeAilmentMatch.Groups[1].Value, out float value);
            string ailment = causeAilmentMatch.Groups[2].Value.ToLower();
            
            return new AffixModifier($"chanceTo{char.ToUpper(ailment[0])}{ailment.Substring(1)}", value, value, ModifierType.Flat);
        }
        
        // Pattern: "20% chance to consume Crumble on hit"
        Match consumeAilmentMatch = Regex.Match(description, @"(\d+)%\s+chance\s+to\s+consume\s+(\w+)\s+on\s+hit", RegexOptions.IgnoreCase);
        if (consumeAilmentMatch.Success)
        {
            float.TryParse(consumeAilmentMatch.Groups[1].Value, out float value);
            string ailment = consumeAilmentMatch.Groups[2].Value.ToLower();
            
            return new AffixModifier($"chanceToConsume{char.ToUpper(ailment[0])}{ailment.Substring(1)}", value, value, ModifierType.Flat);
        }
        
        // Pattern: "10% increased Stagger Duration on Enemies"
        Match staggerDurationMatch = Regex.Match(description, @"(\d+)%\s+increased\s+Stagger\s+Duration\s+on\s+Enemies", RegexOptions.IgnoreCase);
        if (staggerDurationMatch.Success)
        {
            float.TryParse(staggerDurationMatch.Groups[1].Value, out float value);
            return new AffixModifier("increasedStaggerDuration", value, value, ModifierType.Increased);
        }
        
        // Pattern: "12% reduced Enemy Stagger Threshold"
        Match staggerThresholdMatch = Regex.Match(description, @"(\d+)%\s+reduced\s+Enemy\s+Stagger\s+Threshold", RegexOptions.IgnoreCase);
        if (staggerThresholdMatch.Success)
        {
            float.TryParse(staggerThresholdMatch.Groups[1].Value, out float value);
            return new AffixModifier("reducedEnemyStaggerThreshold", value, value, ModifierType.Reduced);
        }
        
        // Pattern: "35% increased damage to Staggered enemies"
        Match staggeredDamageMatch =  Regex.Match(description, @"(\d+)%\s+increased\s+damage\s+to\s+Staggered\s+enemies", RegexOptions.IgnoreCase);
        if (staggeredDamageMatch.Success)
        {
            float.TryParse(staggeredDamageMatch.Groups[1].Value, out float value);
            return new AffixModifier("increasedDamageToStaggered", value, value, ModifierType.Increased);
        }
        
        // Pattern: "40% increased Crumble magnitude on Staggered enemies"
        Match ailmentMagnitudeMatch = Regex.Match(description, @"(\d+)%\s+increased\s+(\w+)\s+magnitude\s+on\s+Staggered\s+enemies", RegexOptions.IgnoreCase);
        if (ailmentMagnitudeMatch.Success)
        {
            float.TryParse(ailmentMagnitudeMatch.Groups[1].Value, out float value);
            string ailment = ailmentMagnitudeMatch.Groups[2].Value.ToLower();
            
            return new AffixModifier($"increased{char.ToUpper(ailment[0])}{ailment.Substring(1)}Magnitude", value, value, ModifierType.Increased);
        }
        
        // Pattern: "50% increased Critical Strike Chance"
        Match critChanceMatch = Regex.Match(description, @"(\d+)%\s+increased\s+Critical\s+Strike\s+Chance", RegexOptions.IgnoreCase);
        if (critChanceMatch.Success)
        {
            float.TryParse(critChanceMatch.Groups[1].Value, out float value);
            return new AffixModifier("criticalStrikeChance", value, value, ModifierType.Increased);
        }
        
        // Pattern: "10% increased Physical Damage"
        Match increasedDamageMatch = Regex.Match(description, @"(\d+)%\s+increased\s+(\w+)\s+Damage", RegexOptions.IgnoreCase);
        if (increasedDamageMatch.Success)
        {
            float.TryParse(increasedDamageMatch.Groups[1].Value, out float value);
            string damageType = increasedDamageMatch.Groups[2].Value.ToLower();
            
            return new AffixModifier($"increased{char.ToUpper(damageType[0])}{damageType.Substring(1)}Damage", value, value, ModifierType.Increased);
        }
        
        // Pattern: "10% increased Attack Speed"
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
        
        // Pattern: "+(5-15) to Strength"
        Match flatStatMatch = Regex.Match(description, @"\+\((\d+)-(\d+)\)\s+to\s+(.+)", RegexOptions.IgnoreCase);
        if (flatStatMatch.Success)
        {
            float.TryParse(flatStatMatch.Groups[1].Value, out float minValue);
            float.TryParse(flatStatMatch.Groups[2].Value, out float maxValue);
            string statName = MapStatName(flatStatMatch.Groups[3].Value.Trim());
            
            return new AffixModifier(statName, minValue, maxValue, ModifierType.Flat);
        }
        
        // Pattern: "+15 to Strength"
        Match flatSingleMatch = Regex.Match(description, @"\+(\d+)\s+to\s+(.+)", RegexOptions.IgnoreCase);
        if (flatSingleMatch.Success)
        {
            float.TryParse(flatSingleMatch.Groups[1].Value, out float value);
            string statName = MapStatName(flatSingleMatch.Groups[2].Value.Trim());
            
            return new AffixModifier(statName, value, value, ModifierType.Flat);
        }
        
        // Fallback: create a descriptive modifier
        Debug.LogWarning($"Could not parse implicit modifier: {description}");
        return new AffixModifier(SanitizeStatName(description), 1f, 1f, ModifierType.Flat);
    }
    
    /// <summary>
    /// Map stat descriptions to actual CharacterStatsData field names
    /// </summary>
    private string MapStatName(string statDescription)
    {
        string lower = statDescription.ToLower().Replace(" ", "").Replace("-", "");
        
        // Core attributes
        if (lower.Contains("strength")) return "strength";
        if (lower.Contains("dexterity")) return "dexterity"; 
        if (lower.Contains("intelligence")) return "intelligence";
        
        // Attack/Cast Speed
        if (lower.Contains("attackspeed")) return "attackSpeed";
        if (lower.Contains("castspeed")) return "castSpeed";
        if (lower.Contains("movementspeed")) return "movementSpeed";
        
        // Critical Strike
        if (lower.Contains("criticalstrikechance")) return "criticalChance";
        if (lower.Contains("criticalstrikemultiplier")) return "criticalMultiplier";
        
        // Damage types
        if (lower.Contains("physicaldamage")) return "increasedPhysicalDamage";
        if (lower.Contains("firedamage")) return "increasedFireDamage";
        if (lower.Contains("colddamage")) return "increasedColdDamage";
        if (lower.Contains("lightningdamage")) return "increasedLightningDamage";
        if (lower.Contains("chaosdamage")) return "increasedChaosDamage";
        if (lower.Contains("elementaldamage")) return "increasedElementalDamage";
        if (lower.Contains("spelldamage")) return "increasedSpellDamage";
        if (lower.Contains("attackdamage")) return "increasedAttackDamage";
        if (lower.Contains("projectiledamage")) return "increasedProjectileDamage";
        if (lower.Contains("areadamage")) return "increasedAreaDamage";
        if (lower.Contains("meleedamage")) return "increasedMeleeDamage";
        if (lower.Contains("rangeddamage")) return "increasedRangedDamage";
        
        // Resistances
        if (lower.Contains("physicalresistance")) return "physicalResistance";
        if (lower.Contains("fireresistance")) return "fireResistance";
        if (lower.Contains("coldresistance")) return "coldResistance";
        if (lower.Contains("lightningresistance")) return "lightningResistance";
        if (lower.Contains("chaosresistance")) return "chaosResistance";
        if (lower.Contains("elementalresistance")) return "elementalResistance";
        if (lower.Contains("allresistance")) return "allResistance";
        
        // Defense
        if (lower.Contains("armour")) return "armour";
        if (lower.Contains("evasion")) return "evasion";
        if (lower.Contains("energyshield")) return "energyShield";
        if (lower.Contains("blockchance")) return "blockChance";
        if (lower.Contains("dodgechance")) return "dodgeChance";
        if (lower.Contains("spelldodgechance")) return "spellDodgeChance";
        if (lower.Contains("spellblockchance")) return "spellBlockChance";
        
        // Ailment chances
        if (lower.Contains("chancetoshock")) return "chanceToShock";
        if (lower.Contains("chancetochill")) return "chanceToChill";
        if (lower.Contains("chancetofreeze")) return "chanceToFreeze";
        if (lower.Contains("chancetoignite")) return "chanceToIgnite";
        if (lower.Contains("chancetobleed")) return "chanceToBleed";
        if (lower.Contains("chancetopoison")) return "chanceToPoison";
        
        // Recovery
        if (lower.Contains("liferegeneration")) return "lifeRegeneration";
        if (lower.Contains("energyshieldregeneration")) return "energyShieldRegeneration";
        if (lower.Contains("manaregeneration")) return "manaRegeneration";
        if (lower.Contains("relianceregeneration")) return "relianceRegeneration";
        if (lower.Contains("lifeleech")) return "lifeLeech";
        if (lower.Contains("manaleech")) return "manaLeech";
        if (lower.Contains("energyshieldleech")) return "energyShieldLeech";
        
        // Card system
        if (lower.Contains("cardsdrawnperturn")) return "cardsDrawnPerTurn";
        if (lower.Contains("cardsdrawnperwave")) return "cardsDrawnPerWave";
        if (lower.Contains("maxhandsize")) return "maxHandSize";
        if (lower.Contains("carddrawchance")) return "cardDrawChance";
        if (lower.Contains("cardretentionchance")) return "cardRetentionChance";
        if (lower.Contains("cardupgradechance")) return "cardUpgradeChance";
        
        // Combat mechanics
        if (lower.Contains("attackrange")) return "attackRange";
        if (lower.Contains("projectilespeed")) return "projectileSpeed";
        if (lower.Contains("areaofeffect")) return "areaOfEffect";
        if (lower.Contains("skilleffectduration")) return "skillEffectDuration";
        if (lower.Contains("statuseffectduration")) return "statusEffectDuration";
        
        // Fallback: sanitize and return
        return SanitizeStatName(statDescription);
    }
    
    /// <summary>
    /// Sanitize stat name for use as a field name
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
    
    private void UpdateWeaponTypeCounts(List<WeaponData> weapons)
    {
        weaponTypeCounts.Clear();
        
        foreach (var weapon in weapons)
        {
            if (!weaponTypeCounts.ContainsKey(weapon.weaponType))
            {
                weaponTypeCounts[weapon.weaponType] = 0;
            }
            weaponTypeCounts[weapon.weaponType]++;
        }
    }
    
    private int GetWeaponTypeCount(WeaponItemType weaponType)
    {
        if (weaponTypeCounts.ContainsKey(weaponType))
        {
            return weaponTypeCounts[weaponType];
        }
        return 0;
    }
    
    private int GetHandednessCount(WeaponHandedness handedness)
    {
        if (csvFile == null) return 0;
        
        List<WeaponData> weapons = ParseCSV(csvFile.text);
        
        // Apply weapon type filter first if enabled
        if (filterByType)
        {
            weapons = FilterWeaponsByType(weapons);
        }
        
        return weapons.Count(w => w.handedness == handedness);
    }
    
    private bool IsWeaponTypeEnabled(WeaponItemType weaponType)
    {
        switch (weaponType)
        {
            case WeaponItemType.Mace:
                return importMaces;
            case WeaponItemType.Sceptre:
                return importSceptres;
            case WeaponItemType.Wand:
                return importWands;
            case WeaponItemType.Dagger:
            case WeaponItemType.RitualDagger:
                return importDaggers;
            case WeaponItemType.Claw:
                return importClaws;
            case WeaponItemType.Bow:
                return importBows;
            case WeaponItemType.Axe:
                return importAxes;
            case WeaponItemType.Sword:
                return importSwords;
            case WeaponItemType.Staff:
                return importStaffs;
            default:
                return true;
        }
    }
    
    private List<WeaponData> FilterWeaponsByType(List<WeaponData> weapons)
    {
        List<WeaponData> filtered = new List<WeaponData>();
        
        foreach (var weapon in weapons)
        {
            if (IsWeaponTypeEnabled(weapon.weaponType))
            {
                filtered.Add(weapon);
            }
        }
        
        return filtered;
    }
    
    private List<WeaponData> FilterWeaponsByHandedness(List<WeaponData> weapons)
    {
        List<WeaponData> filtered = new List<WeaponData>();
        
        foreach (var weapon in weapons)
        {
            bool shouldImport = false;
            
            if (weapon.handedness == WeaponHandedness.OneHanded && importOneHanded)
            {
                shouldImport = true;
            }
            else if (weapon.handedness == WeaponHandedness.TwoHanded && importTwoHanded)
            {
                shouldImport = true;
            }
            
            if (shouldImport)
            {
                filtered.Add(weapon);
            }
        }
        
        return filtered;
    }
    
    // Helper class to store weapon data during parsing
    private class WeaponData
    {
        public string name;
        public float minDamage;
        public float maxDamage;
        public float critChance = 5.0f;
        public float attackSpeed = 1.0f;
        public int requiredLevel = 1;
        public int requiredStr = 0;
        public int requiredDex = 0;
        public int requiredInt = 0;
        public WeaponItemType weaponType = WeaponItemType.Sword;
        public WeaponHandedness handedness = WeaponHandedness.OneHanded;
        public List<string> implicits = new List<string>();
    }
}

