using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Editor tool to import Embossings from TSV file
/// Creates EmbossingEffect ScriptableObjects from the TSV data
/// </summary>
public class EmbossingTSVImporter : EditorWindow
{
    private TextAsset tsvFile;
    private string tsvFilePath = "Assets/Documentation/Embossing revisit.md";
    private string outputFolder = "Assets/Resources/Embossings";
    private bool overwriteExisting = false;
    
    [MenuItem("Tools/Dexiled/Import Embossings from TSV")]
    public static void ShowWindow()
    {
        GetWindow<EmbossingTSVImporter>("Embossing TSV Importer");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Embossing TSV Importer", EditorStyles.boldLabel);
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
            string path = EditorUtility.OpenFilePanel("Select TSV/MD File", "Assets", "md,tsv");
            if (!string.IsNullOrEmpty(path))
            {
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
        
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("TSV Format:\nName | Level | Tags | Effect", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Import Embossings"))
        {
            if (tsvFile == null && string.IsNullOrEmpty(tsvFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a TSV file or provide a file path.", "OK");
                return;
            }
            
            ImportEmbossings();
        }
    }
    
    private void ImportEmbossings()
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
            if (tsvFilePath.StartsWith("Assets/"))
            {
                string fullPath = System.IO.Path.Combine(Application.dataPath, tsvFilePath.Substring(7));
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
            Debug.LogError("[EmbossingTSVImporter] TSV file is empty or has no data rows!");
            return;
        }
        
        // Ensure output folder exists
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        
        int createdCount = 0;
        int skippedCount = 0;
        int errorCount = 0;
        
        // Skip header line
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
            
            try
            {
                string[] columns = line.Split('\t');
                if (columns.Length < 4)
                    continue;
                
                string name = columns[0].Trim();
                string levelStr = columns[1].Trim();
                string tagsStr = columns[2].Trim();
                string effect = columns[3].Trim();
                
                if (string.IsNullOrEmpty(name))
                    continue;
                
                // Parse level
                int minLevel = 1;
                if (!string.IsNullOrEmpty(levelStr) && int.TryParse(levelStr, out int parsedLevel))
                {
                    minLevel = parsedLevel;
                }
                
                if (CreateEmbossingAsset(name, minLevel, tagsStr, effect))
                {
                    createdCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EmbossingTSVImporter] Error processing line {i + 1}: {e.Message}\n{e.StackTrace}");
                errorCount++;
            }
        }
        
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Import Complete", 
            $"Created: {createdCount}\nSkipped: {skippedCount}\nErrors: {errorCount}", 
            "OK");
        
        Debug.Log($"[EmbossingTSVImporter] Import complete. Created: {createdCount}, Skipped: {skippedCount}, Errors: {errorCount}");
    }
    
    private bool CreateEmbossingAsset(string name, int minLevel, string tagsStr, string effect)
    {
        // Generate embossing ID
        string embossingId = GenerateEmbossingId(name);
        
        // Create asset name (sanitize)
        string assetName = SanitizeFileName(name);
        string assetPath = Path.Combine(outputFolder, $"{assetName}.asset");
        
        // Check if asset already exists
        if (File.Exists(assetPath) && !overwriteExisting)
        {
            Debug.Log($"[EmbossingTSVImporter] Skipping {name} - asset already exists (use 'Overwrite Existing' to replace)");
            return false;
        }
        
        // Create or load existing asset
        EmbossingEffect embossing = AssetDatabase.LoadAssetAtPath<EmbossingEffect>(assetPath);
        if (embossing == null)
        {
            embossing = ScriptableObject.CreateInstance<EmbossingEffect>();
        }
        
        // Populate embossing data
        embossing.embossingName = name;
        embossing.embossingId = embossingId;
        embossing.description = effect; // Use effect as description
        embossing.minimumLevel = minLevel;
        
        // Infer category and rarity from level and tags
        embossing.category = InferCategory(tagsStr, effect);
        embossing.rarity = InferRarity(minLevel);
        
        // Set default mana cost multiplier based on level
        embossing.manaCostMultiplier = InferManaCostMultiplier(minLevel);
        embossing.flatManaCostIncrease = 0; // Default to 0
        
        // Set default effect type (will be overridden by modifiers, but kept for backward compatibility)
        embossing.effectType = EmbossingEffectType.CustomEffect;
        embossing.effectValue = 0f;
        embossing.secondaryEffectValue = 0f;
        
        // Clear modifier IDs - they'll be linked by the linker tool
        embossing.modifierIds = new List<string>();
        
        // Save asset
        if (File.Exists(assetPath))
        {
            EditorUtility.SetDirty(embossing);
        }
        else
        {
            AssetDatabase.CreateAsset(embossing, assetPath);
        }
        
        Debug.Log($"[EmbossingTSVImporter] Created/Updated: {assetPath} (ID: {embossingId})");
        return true;
    }
    
    private string GenerateEmbossingId(string embossingName)
    {
        if (string.IsNullOrEmpty(embossingName))
            return "";
        
        // Remove "of " prefix if present
        string id = embossingName.Replace("of ", "").Replace("Of ", "");
        // Convert to lowercase and replace spaces with underscores
        id = id.ToLower().Replace(" ", "_");
        // Remove special characters
        id = Regex.Replace(id, @"[^a-z0-9_]", "");
        return id;
    }
    
    private string SanitizeFileName(string fileName)
    {
        return Regex.Replace(fileName, @"[<>:""/\\|?*]", "").Trim();
    }
    
    private EmbossingCategory InferCategory(string tagsStr, string effect)
    {
        string tagsLower = tagsStr.ToLower();
        string effectLower = effect.ToLower();
        
        // Check for specific categories based on tags and effect
        if (tagsLower.Contains("bleed") || tagsLower.Contains("poison") || tagsLower.Contains("ignite") || 
            tagsLower.Contains("shock") || tagsLower.Contains("chill") || tagsLower.Contains("status"))
        {
            return EmbossingCategory.Ailment;
        }
        
        if (tagsLower.Contains("fire") || tagsLower.Contains("cold") || tagsLower.Contains("lightning") || 
            tagsLower.Contains("elemental") || effectLower.Contains("fire") || effectLower.Contains("cold") || 
            effectLower.Contains("lightning"))
        {
            return EmbossingCategory.Conversion;
        }
        
        if (tagsLower.Contains("chaos") || effectLower.Contains("chaos"))
        {
            return EmbossingCategory.Chaos;
        }
        
        if (tagsLower.Contains("spell") || effectLower.Contains("spell"))
        {
            return EmbossingCategory.Utility;
        }
        
        if (tagsLower.Contains("attack") || tagsLower.Contains("melee") || tagsLower.Contains("projectile"))
        {
            return EmbossingCategory.Damage;
        }
        
        // Default to Damage
        return EmbossingCategory.Damage;
    }
    
    private EmbossingRarity InferRarity(int minLevel)
    {
        if (minLevel >= 31)
            return EmbossingRarity.Legendary;
        else if (minLevel >= 18)
            return EmbossingRarity.Epic;
        else if (minLevel >= 8)
            return EmbossingRarity.Rare;
        else if (minLevel >= 4)
            return EmbossingRarity.Uncommon;
        else
            return EmbossingRarity.Common;
    }
    
    private float InferManaCostMultiplier(int minLevel)
    {
        // Higher level embossings have higher mana cost multipliers
        if (minLevel >= 31)
            return 0.50f; // +50% for legendary
        else if (minLevel >= 18)
            return 0.40f; // +40% for epic
        else if (minLevel >= 8)
            return 0.30f; // +30% for rare
        else if (minLevel >= 4)
            return 0.25f; // +25% for uncommon
        else
            return 0.20f; // +20% for common
    }
}

