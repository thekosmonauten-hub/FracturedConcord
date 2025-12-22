using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Editor tool to import Reliance Auras from TSV file
/// Creates RelianceAura ScriptableObjects from the TSV data
/// </summary>
public class RelianceAuraTSVImporter : EditorWindow
{
    private TextAsset tsvFile;
    private string tsvFilePath = "Assets/Documentation/RelianceAuras.tsv";
    private string outputFolder = "Assets/Resources/RelianceAuras";
    private bool createSubfoldersByCategory = true;
    private bool overwriteExisting = false;
    
    [MenuItem("Tools/Dexiled/Import Reliance Auras from TSV")]
    public static void ShowWindow()
    {
        GetWindow<RelianceAuraTSVImporter>("Reliance Aura TSV Importer");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Reliance Aura TSV Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("TSV File Selection", EditorStyles.boldLabel);
        tsvFile = (TextAsset)EditorGUILayout.ObjectField("TSV File (TextAsset)", tsvFile, typeof(TextAsset), false);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("OR", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("TSV File Path (Alternative)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        tsvFilePath = EditorGUILayout.TextField("File Path", tsvFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("Select TSV File", "Assets", "tsv");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative path if it's within the project
                if (path.StartsWith(Application.dataPath))
                {
                    tsvFilePath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    tsvFilePath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        
        createSubfoldersByCategory = EditorGUILayout.Toggle("Create Subfolders by Category", createSubfoldersByCategory);
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("TSV Format:\nCategory | Aura Name | Description | Effect Level 1 | Effect Level 20 | Reliance | Modifier IDs | ...", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Import Auras"))
        {
            if (tsvFile == null && string.IsNullOrEmpty(tsvFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a TSV file or provide a file path.", "OK");
                return;
            }
            
            ImportAuras();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Show Format Example"))
        {
            ShowFormatExample();
        }
    }
    
    private void ImportAuras()
    {
        string tsvContent = null;
        
        // Try to get content from TextAsset first
        if (tsvFile != null)
        {
            tsvContent = tsvFile.text;
        }
        // Otherwise, try to read from file path
        else if (!string.IsNullOrEmpty(tsvFilePath))
        {
            // Check if it's a relative path (starts with Assets/)
            if (tsvFilePath.StartsWith("Assets/"))
            {
                string fullPath = System.IO.Path.Combine(Application.dataPath, tsvFilePath.Substring(7)); // Remove "Assets/"
                if (System.IO.File.Exists(fullPath))
                {
                    tsvContent = System.IO.File.ReadAllText(fullPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", $"File not found: {fullPath}", "OK");
                    return;
                }
            }
            else if (System.IO.File.Exists(tsvFilePath))
            {
                tsvContent = System.IO.File.ReadAllText(tsvFilePath);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", $"File not found: {tsvFilePath}", "OK");
                return;
            }
        }
        
        if (string.IsNullOrEmpty(tsvContent))
        {
            EditorUtility.DisplayDialog("Error", "Could not read TSV file. Please check the file path or select a TextAsset.", "OK");
            return;
        }
        
        string[] lines = tsvContent.Split('\n');
        if (lines.Length < 2)
        {
            Debug.LogError("[RelianceAuraTSVImporter] TSV file is empty or has no data rows!");
            return;
        }
        
        // Skip header line
        int importedCount = 0;
        int skippedCount = 0;
        int errorCount = 0;
        
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
            
            try
            {
                RelianceAuraData data = ParseTSVLine(line);
                if (data == null)
                {
                    errorCount++;
                    continue;
                }
                
                if (CreateRelianceAuraAsset(data))
                {
                    importedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RelianceAuraTSVImporter] Error processing line {i + 1}: {e.Message}");
                errorCount++;
            }
        }
        
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Import Complete", 
            $"Imported: {importedCount}\nSkipped: {skippedCount}\nErrors: {errorCount}", 
            "OK");
        
        Debug.Log($"[RelianceAuraTSVImporter] Import complete. Imported: {importedCount}, Skipped: {skippedCount}, Errors: {errorCount}");
    }
    
    private RelianceAuraData ParseTSVLine(string line)
    {
        // TSV uses tab-separated values
        string[] columns = line.Split('\t');
        
        if (columns.Length < 6)
        {
            Debug.LogWarning($"[RelianceAuraTSVImporter] Insufficient columns in line: {line}");
            return null;
        }
        
        RelianceAuraData data = new RelianceAuraData
        {
            category = columns[0].Trim(),
            auraName = columns[1].Trim(),
            description = columns[2].Trim(),
            effectLevel1 = columns[3].Trim(),
            effectLevel20 = columns[4].Trim(),
            relianceCost = ParseReliance(columns[5].Trim())
        };
        
        // Parse Modifier IDs (column 6, if it exists)
        if (columns.Length > 6 && !string.IsNullOrEmpty(columns[6].Trim()))
        {
            string modifierIdsStr = columns[6].Trim();
            data.modifierIds = new List<string>();
            foreach (string id in modifierIdsStr.Split(','))
            {
                string trimmedId = id.Trim();
                if (!string.IsNullOrEmpty(trimmedId))
                {
                    data.modifierIds.Add(trimmedId);
                }
            }
        }
        
        return data;
    }
    
    private int ParseReliance(string relianceStr)
    {
        if (string.IsNullOrEmpty(relianceStr))
            return 100; // Default
        
        if (int.TryParse(relianceStr, out int value))
            return value;
        
        Debug.LogWarning($"[RelianceAuraTSVImporter] Could not parse reliance value: {relianceStr}, using default 100");
        return 100;
    }
    
    private bool CreateRelianceAuraAsset(RelianceAuraData data)
    {
        if (string.IsNullOrEmpty(data.auraName))
        {
            Debug.LogWarning("[RelianceAuraTSVImporter] Skipping aura with empty name");
            return false;
        }
        
        // Determine output path
        string folderPath = outputFolder;
        if (createSubfoldersByCategory && !string.IsNullOrEmpty(data.category))
        {
            folderPath = Path.Combine(outputFolder, data.category);
        }
        
        // Ensure folder exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        // Create asset name (sanitize)
        string assetName = SanitizeFileName(data.auraName);
        string assetPath = Path.Combine(folderPath, $"{assetName}.asset");
        
        // Check if asset already exists
        if (File.Exists(assetPath) && !overwriteExisting)
        {
            Debug.Log($"[RelianceAuraTSVImporter] Skipping {data.auraName} - asset already exists (use 'Overwrite Existing' to replace)");
            return false;
        }
        
        // Create or load existing asset
        RelianceAura aura = AssetDatabase.LoadAssetAtPath<RelianceAura>(assetPath);
        if (aura == null)
        {
            aura = ScriptableObject.CreateInstance<RelianceAura>();
        }
        
        // Populate aura data
        aura.auraName = data.auraName;
        aura.category = data.category;
        aura.description = data.description;
        aura.effectLevel1 = data.effectLevel1;
        aura.effectLevel20 = data.effectLevel20;
        aura.relianceCost = data.relianceCost;
        aura.embossingSlots = 1; // Default to 1 slot, can be adjusted manually
        aura.requiredLevel = 1; // Default to level 1, can be adjusted manually
        
        // Populate modifier IDs (from TSV or auto-link from generated modifiers)
        if (data.modifierIds != null && data.modifierIds.Count > 0)
        {
            // Use modifier IDs from TSV
            aura.modifierIds = new List<string>(data.modifierIds);
        }
        else
        {
            // Auto-link: Find modifiers that have this aura's name as linkedAuraName
            aura.modifierIds = AutoLinkModifiers(data.auraName);
        }
        
        // Set default theme color based on category
        aura.themeColor = GetCategoryColor(data.category);
        
        // Save asset
        if (File.Exists(assetPath))
        {
            EditorUtility.SetDirty(aura);
        }
        else
        {
            AssetDatabase.CreateAsset(aura, assetPath);
        }
        
        Debug.Log($"[RelianceAuraTSVImporter] Created/Updated: {assetPath}");
        return true;
    }
    
    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters for file names
        string sanitized = Regex.Replace(fileName, @"[<>:""/\\|?*]", "");
        return sanitized.Trim();
    }
    
    /// <summary>
    /// Auto-link modifier IDs by finding RelianceAuraModifierDefinition assets with matching linkedAuraName
    /// </summary>
    private List<string> AutoLinkModifiers(string auraName)
    {
        List<string> modifierIds = new List<string>();
        
        // Find all RelianceAuraModifierDefinition assets in Resources
        string[] guids = AssetDatabase.FindAssets("t:RelianceAuraModifierDefinition", new[] { "Assets/Resources" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RelianceAuraModifierDefinition modifier = AssetDatabase.LoadAssetAtPath<RelianceAuraModifierDefinition>(path);
            
            if (modifier != null && modifier.linkedAuraName == auraName)
            {
                if (!string.IsNullOrEmpty(modifier.modifierId))
                {
                    modifierIds.Add(modifier.modifierId);
                }
            }
        }
        
        if (modifierIds.Count > 0)
        {
            Debug.Log($"[RelianceAuraTSVImporter] Auto-linked {modifierIds.Count} modifier(s) for {auraName}: {string.Join(", ", modifierIds)}");
        }
        
        return modifierIds;
    }
    
    private Color GetCategoryColor(string category)
    {
        if (string.IsNullOrEmpty(category))
            return Color.white;
        
        switch (category.ToLower())
        {
            case "fire":
                return new Color(1f, 0.3f, 0.1f); // Red-orange
            case "cold":
                return new Color(0.3f, 0.7f, 1f); // Light blue
            case "lightning":
                return new Color(0.9f, 0.9f, 0.2f); // Yellow
            case "physical":
                return new Color(0.7f, 0.7f, 0.7f); // Gray
            case "ailment":
                return new Color(0.5f, 0.2f, 0.8f); // Purple
            case "speed":
                return new Color(0.2f, 1f, 0.5f); // Green
            case "critical":
                return new Color(1f, 0.2f, 0.5f); // Pink
            case "spell":
                return new Color(0.5f, 0.8f, 1f); // Light blue
            case "mana":
                return new Color(0.2f, 0.5f, 1f); // Blue
            case "armor":
                return new Color(0.6f, 0.4f, 0.2f); // Brown
            case "evasion":
                return new Color(0.3f, 0.8f, 0.3f); // Green
            case "energy shield":
                return new Color(0.8f, 0.5f, 1f); // Light purple
            case "life regen":
                return new Color(1f, 0.4f, 0.4f); // Light red
            case "resist":
                return new Color(0.5f, 0.5f, 0.8f); // Light purple-blue
            case "card draw":
                return new Color(0.9f, 0.7f, 0.3f); // Gold
            case "prep":
                return new Color(0.4f, 0.9f, 0.9f); // Cyan
            case "discard":
                return new Color(0.8f, 0.3f, 0.3f); // Dark red
            default:
                return Color.white;
        }
    }
    
    private void ShowFormatExample()
    {
        string example = @"TSV Format (Tab-separated):
Category	Aura Name	Description	Effect Level 1	Effect Level 20	Reliance
Fire	Pyreheart Mantle	Casts an aura that adds fire damage to attacks and spells.	90 flat Fire damage	160 flat Fire damage	100
Cold	Frostgraft Veil	Casts an aura that causes your physical damage to deal extra Cold damage.	15% of Physical damage as Extra cold damage	25% of Physical damage as Extra cold damage	100

Columns:
1. Category (Fire, Cold, Lightning, etc.)
2. Aura Name
3. Description
4. Effect Level 1
5. Effect Level 20
6. Reliance Cost";
        
        EditorUtility.DisplayDialog("TSV Format", example, "OK");
    }
    
    private class RelianceAuraData
    {
        public string category;
        public string auraName;
        public string description;
        public string effectLevel1;
        public string effectLevel20;
        public int relianceCost;
        public List<string> modifierIds = new List<string>();
    }
}

