using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

/// <summary>
/// CSV Importer for Monster Modifiers
/// Reads a CSV file with modifier data and creates MonsterModifier ScriptableObjects
/// </summary>
public class MonsterModifierCSVImporter : EditorWindow
{
    private TextAsset csvFile;
    private string outputFolder = "Assets/Resources/MonsterModifiers";
    private bool overwriteExisting = false;
    
    private Vector2 scrollPosition;
    private string previewText = "";
    private int previewCount = 0;
    
    [MenuItem("Tools/Dexiled/Import Monster Modifiers from CSV")]
    public static void ShowWindow()
    {
        GetWindow<MonsterModifierCSVImporter>("Monster Modifier CSV Importer");
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Monster Modifier CSV Importer", EditorStyles.boldLabel);
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
        
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);
        
        EditorGUILayout.Space();
        
        // Preview section
        if (!string.IsNullOrEmpty(previewText))
        {
            EditorGUILayout.HelpBox($"Preview: {previewCount} modifiers found", MessageType.Info);
            EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(previewText, GUILayout.Height(100));
        }
        
        EditorGUILayout.Space();
        
        // Import button
        EditorGUI.BeginDisabledGroup(csvFile == null);
        if (GUILayout.Button("Import Modifiers", GUILayout.Height(30)))
        {
            ImportModifiers();
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space();
        
        // Format help
        EditorGUILayout.HelpBox(
            "CSV Format:\n" +
            "Header row required. Columns:\n" +
            "Name, Description, Color (R,G,B), Health%, Damage%, Accuracy%, Evasion%, CritChance%, CritMultiplier%, " +
            "PhysicalRes%, FireRes%, ColdRes%, LightningRes%, ChaosRes%, " +
            "RegenHealth (true/false), RegenAmount, IsHasted (true/false), IsArmored (true/false), " +
            "ReflectsDamage (true/false), ReflectPercent%, " +
            "QuantityMultiplier, RarityMultiplier, Weight",
            MessageType.Info);
        
        EditorGUILayout.EndScrollView();
    }
    
    private void ImportModifiers()
    {
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a CSV file", "OK");
            return;
        }
        
        // Parse CSV
        List<ModifierData> modifierList = ParseCSV(csvFile.text);
        
        if (modifierList.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "No modifiers found in CSV", "OK");
            return;
        }
        
        // Create output directory
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        
        int imported = 0;
        int skipped = 0;
        int errors = 0;
        
        foreach (ModifierData data in modifierList)
        {
            try
            {
                string assetPath = $"{outputFolder}/{data.name}.asset";
                
                // Check if exists
                if (File.Exists(assetPath) && !overwriteExisting)
                {
                    skipped++;
                    continue;
                }
                
                // Create modifier
                MonsterModifier modifier = ScriptableObject.CreateInstance<MonsterModifier>();
                modifier.modifierName = data.name;
                modifier.description = data.description;
                modifier.modifierColor = data.color;
                
                // Stat modifications
                modifier.healthMultiplier = data.healthMultiplier;
                modifier.damageMultiplier = data.damageMultiplier;
                modifier.accuracyMultiplier = data.accuracyMultiplier;
                modifier.evasionMultiplier = data.evasionMultiplier;
                modifier.critChanceMultiplier = data.critChanceMultiplier;
                modifier.critMultiplierMultiplier = data.critMultiplierMultiplier;
                
                // Resistances
                modifier.physicalResistance = data.physicalResistance;
                modifier.fireResistance = data.fireResistance;
                modifier.coldResistance = data.coldResistance;
                modifier.lightningResistance = data.lightningResistance;
                modifier.chaosResistance = data.chaosResistance;
                
                // Special effects
                modifier.regeneratesHealth = data.regeneratesHealth;
                modifier.healthRegenPerTurn = data.healthRegenPerTurn;
                modifier.isHasted = data.isHasted;
                modifier.isArmored = data.isArmored;
                modifier.reflectsDamage = data.reflectsDamage;
                modifier.reflectPercentage = data.reflectPercentage;
                
                // Loot modifiers
                modifier.quantityMultiplier = data.quantityMultiplier;
                modifier.rarityMultiplier = data.rarityMultiplier;
                
                // Modifier weight
                modifier.weight = data.weight;
                
                // Save asset
                AssetDatabase.CreateAsset(modifier, assetPath);
                imported++;
                
                Debug.Log($"Imported: {data.name}");
            }
            catch (System.Exception ex)
            {
                errors++;
                Debug.LogError($"Error importing {data.name}: {ex.Message}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        string message = $"Import Complete!\n\nImported: {imported} modifiers\nSkipped: {skipped} modifiers\nErrors: {errors}";
        EditorUtility.DisplayDialog("Import Complete", message, "OK");
        
        Debug.Log($"[MonsterModifierCSVImporter] {message}");
        
        // Clear preview
        previewText = "";
        previewCount = 0;
    }
    
    private List<ModifierData> ParseCSV(string csvText)
    {
        List<ModifierData> modifierList = new List<ModifierData>();
        string[] lines = csvText.Split('\n');
        
        if (lines.Length < 2)
        {
            Debug.LogWarning("[MonsterModifierCSVImporter] CSV file is empty or has no data rows");
            return modifierList;
        }
        
        // Parse header to determine column indices
        string[] headerColumns = ParseCSVLine(lines[0]);
        Dictionary<string, int> columnIndices = new Dictionary<string, int>();
        
        for (int i = 0; i < headerColumns.Length; i++)
        {
            string header = headerColumns[i].Trim().ToLower();
            columnIndices[header] = i;
        }
        
        // Parse data rows
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            ModifierData modifier = ParseModifierLine(line, columnIndices);
            if (modifier != null)
            {
                modifierList.Add(modifier);
            }
        }
        
        // Update preview
        previewCount = modifierList.Count;
        previewText = $"Found {modifierList.Count} modifiers:\n";
        foreach (var mod in modifierList)
        {
            previewText += $"- {mod.name}\n";
        }
        
        return modifierList;
    }
    
    private ModifierData ParseModifierLine(string line, Dictionary<string, int> columnIndices)
    {
        string[] columns = ParseCSVLine(line);
        
        if (columns.Length < 2)
        {
            Debug.LogWarning($"[MonsterModifierCSVImporter] Invalid line (not enough columns): {line}");
            return null;
        }
        
        ModifierData data = new ModifierData();
        
        try
        {
            // Required fields
            data.name = GetColumnValue(columns, columnIndices, "name", "");
            data.description = GetColumnValue(columns, columnIndices, "description", "");
            
            // Color (R,G,B format or hex)
            string colorStr = GetColumnValue(columns, columnIndices, "color", "");
            data.color = ParseColor(colorStr);
            
            // Stat modifications (percentages)
            data.healthMultiplier = ParseFloat(GetColumnValue(columns, columnIndices, "health%", "0"));
            data.damageMultiplier = ParseFloat(GetColumnValue(columns, columnIndices, "damage%", "0"));
            data.accuracyMultiplier = ParseFloat(GetColumnValue(columns, columnIndices, "accuracy%", "0"));
            data.evasionMultiplier = ParseFloat(GetColumnValue(columns, columnIndices, "evasion%", "0"));
            data.critChanceMultiplier = ParseFloat(GetColumnValue(columns, columnIndices, "critchance%", "0"));
            data.critMultiplierMultiplier = ParseFloat(GetColumnValue(columns, columnIndices, "critmultiplier%", "0"));
            
            // Resistances (percentages)
            data.physicalResistance = ParseFloat(GetColumnValue(columns, columnIndices, "physicalres%", "0"));
            data.fireResistance = ParseFloat(GetColumnValue(columns, columnIndices, "fireres%", "0"));
            data.coldResistance = ParseFloat(GetColumnValue(columns, columnIndices, "coldres%", "0"));
            data.lightningResistance = ParseFloat(GetColumnValue(columns, columnIndices, "lightningres%", "0"));
            data.chaosResistance = ParseFloat(GetColumnValue(columns, columnIndices, "chaosres%", "0"));
            
            // Special effects
            data.regeneratesHealth = ParseBool(GetColumnValue(columns, columnIndices, "regenhealth", "false"));
            data.healthRegenPerTurn = ParseFloat(GetColumnValue(columns, columnIndices, "regenamount", "0"));
            data.isHasted = ParseBool(GetColumnValue(columns, columnIndices, "ishasted", "false"));
            data.isArmored = ParseBool(GetColumnValue(columns, columnIndices, "isarmored", "false"));
            data.reflectsDamage = ParseBool(GetColumnValue(columns, columnIndices, "reflectsdamage", "false"));
            data.reflectPercentage = ParseFloat(GetColumnValue(columns, columnIndices, "reflectpercent%", "0"));
            
            // Loot modifiers
            data.quantityMultiplier = ParseFloat(GetColumnValue(columns, columnIndices, "quantitymultiplier", "1"));
            data.rarityMultiplier = ParseFloat(GetColumnValue(columns, columnIndices, "raritymultiplier", "1"));
            
            // Modifier weight
            string weightStr = GetColumnValue(columns, columnIndices, "weight", "100");
            data.weight = ParseInt(weightStr);
            
            // Effect types (comma-separated)
            string effectTypesStr = GetColumnValue(columns, columnIndices, "effecttypes", "");
            data.effectTypes = ParseEffectTypes(effectTypesStr);
            
            // Effect parameters
            data.onHitStatusEffect = ParseStatusEffectType(GetColumnValue(columns, columnIndices, "onhitstatuseffect", "Vulnerable"));
            data.onHitStatusMagnitude = ParseFloat(GetColumnValue(columns, columnIndices, "onhitstatusmagnitude", "1"));
            data.onHitStatusDuration = ParseInt(GetColumnValue(columns, columnIndices, "onhitstatusduration", "1"));
            data.turnStartStackType = ParseStackType(GetColumnValue(columns, columnIndices, "turnstartstacktype", "Tolerance"));
            data.turnStartStackAmount = ParseInt(GetColumnValue(columns, columnIndices, "turnstartstackamount", "1"));
            data.turnStartStackRequiresHitRecently = ParseBool(GetColumnValue(columns, columnIndices, "turnstartstackrequireshitrecently", "false"));
            
            return data;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing modifier line: {line}\nError: {ex.Message}");
            return null;
        }
    }
    
    private string GetColumnValue(string[] columns, Dictionary<string, int> columnIndices, string columnName, string defaultValue)
    {
        if (columnIndices.ContainsKey(columnName))
        {
            int index = columnIndices[columnName];
            if (index < columns.Length)
            {
                string value = columns[index].Trim();
                return string.IsNullOrEmpty(value) ? defaultValue : value;
            }
        }
        return defaultValue;
    }
    
    private Color ParseColor(string colorStr)
    {
        if (string.IsNullOrEmpty(colorStr))
        {
            return Color.yellow; // Default color
        }
        
        colorStr = colorStr.Trim();
        
        // Try RGB format (R,G,B) or (R,G,B,A)
        if (colorStr.Contains(","))
        {
            string[] parts = colorStr.Split(',');
            if (parts.Length >= 3)
            {
                float r = ParseFloat(parts[0].Trim()) / 255f;
                float g = ParseFloat(parts[1].Trim()) / 255f;
                float b = ParseFloat(parts[2].Trim()) / 255f;
                float a = parts.Length > 3 ? ParseFloat(parts[3].Trim()) / 255f : 1f;
                return new Color(r, g, b, a);
            }
        }
        
        // Try hex format (#RRGGBB or #RRGGBBAA)
        if (colorStr.StartsWith("#"))
        {
            colorStr = colorStr.Substring(1);
            if (colorStr.Length == 6 || colorStr.Length == 8)
            {
                int r = int.Parse(colorStr.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(colorStr.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(colorStr.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                int a = colorStr.Length == 8 ? int.Parse(colorStr.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) : 255;
                return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }
        }
        
        // Try named colors
        switch (colorStr.ToLower())
        {
            case "red": return Color.red;
            case "green": return Color.green;
            case "blue": return Color.blue;
            case "yellow": return Color.yellow;
            case "cyan": return Color.cyan;
            case "magenta": return Color.magenta;
            case "white": return Color.white;
            case "black": return Color.black;
            case "gray": case "grey": return Color.gray;
            default: return Color.yellow;
        }
    }
    
    private float ParseFloat(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0f;
        
        value = value.Trim();
        float result;
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
        {
            return result;
        }
        return 0f;
    }
    
    private bool ParseBool(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
        
        value = value.Trim().ToLower();
        return value == "true" || value == "1" || value == "yes" || value == "y";
    }
    
    private int ParseInt(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 100; // Default weight
        
        value = value.Trim();
        int result;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
        {
            return Mathf.Max(1, result); // Ensure weight is at least 1
        }
        return 100; // Default weight
    }
    
    private List<ModifierEffectType> ParseEffectTypes(string value)
    {
        List<ModifierEffectType> result = new List<ModifierEffectType>();
        if (string.IsNullOrEmpty(value)) return result;
        
        string[] parts = value.Split(',');
        foreach (var part in parts)
        {
            string trimmed = part.Trim();
            if (System.Enum.TryParse<ModifierEffectType>(trimmed, true, out ModifierEffectType effectType))
            {
                if (effectType != ModifierEffectType.None)
                {
                    result.Add(effectType);
                }
            }
        }
        return result;
    }
    
    private StatusEffectType ParseStatusEffectType(string value)
    {
        if (string.IsNullOrEmpty(value)) return StatusEffectType.Vulnerable;
        
        value = value.Trim();
        if (System.Enum.TryParse<StatusEffectType>(value, true, out StatusEffectType result))
        {
            return result;
        }
        return StatusEffectType.Vulnerable;
    }
    
    private StackType ParseStackType(string value)
    {
        if (string.IsNullOrEmpty(value)) return StackType.Tolerance;
        
        value = value.Trim();
        if (System.Enum.TryParse<StackType>(value, true, out StackType result))
        {
            return result;
        }
        return StackType.Tolerance;
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
    
    private class ModifierData
    {
        public string name;
        public string description;
        public Color color = Color.yellow;
        
        // Stat modifications
        public float healthMultiplier;
        public float damageMultiplier;
        public float accuracyMultiplier;
        public float evasionMultiplier;
        public float critChanceMultiplier;
        public float critMultiplierMultiplier;
        
        // Resistances
        public float physicalResistance;
        public float fireResistance;
        public float coldResistance;
        public float lightningResistance;
        public float chaosResistance;
        
        // Special effects
        public bool regeneratesHealth;
        public float healthRegenPerTurn;
        public bool isHasted;
        public bool isArmored;
        public bool reflectsDamage;
        public float reflectPercentage;
        
        // Loot modifiers
        public float quantityMultiplier;
        public float rarityMultiplier;
        
        // Modifier weight
        public int weight;
        
        // Effect types
        public List<ModifierEffectType> effectTypes;
        
        // Effect parameters
        public StatusEffectType onHitStatusEffect;
        public float onHitStatusMagnitude;
        public int onHitStatusDuration;
        public StackType turnStartStackType;
        public int turnStartStackAmount;
        public bool turnStartStackRequiresHitRecently;
    }
}

