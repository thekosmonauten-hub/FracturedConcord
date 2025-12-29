using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor script to create and populate EmbossingDatabase from existing embossing effects in Resources.
/// </summary>
public class CreateEmbossingDatabase : EditorWindow
{
    private string embossingResourcePath = "Embossings";
    private string databaseSavePath = "Assets/Resources/EmbossingDatabase.asset";
    private int loadedCount = 0;
    private Vector2 scrollPosition;
    
    [MenuItem("FracturedConcord/Create Embossing Database")]
    public static void ShowWindow()
    {
        GetWindow<CreateEmbossingDatabase>("Create Embossing Database");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Embossing Database Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool will:\n" +
            "1. Load all EmbossingEffect assets from Resources/Embossings\n" +
            "2. Create or update EmbossingDatabase ScriptableObject\n" +
            "3. Populate it with all found embossings\n\n" +
            "This allows for faster loading via preloading instead of Resources.LoadAll.",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        embossingResourcePath = EditorGUILayout.TextField("Resource Path:", embossingResourcePath);
        databaseSavePath = EditorGUILayout.TextField("Database Save Path:", databaseSavePath);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Load Embossings and Create Database", GUILayout.Height(30)))
        {
            CreateDatabase();
        }
        
        EditorGUILayout.Space();
        
        if (loadedCount > 0)
        {
            EditorGUILayout.HelpBox($"Successfully loaded {loadedCount} embossing effects!", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Find All Embossing Effects in Project"))
        {
            FindAllEmbossings();
        }
    }
    
    private void CreateDatabase()
    {
        // Load all embossings from Resources
        Debug.Log($"[CreateEmbossingDatabase] Loading embossings from Resources/{embossingResourcePath}");
        EmbossingEffect[] loadedEmbossings = Resources.LoadAll<EmbossingEffect>(embossingResourcePath);
        
        if (loadedEmbossings == null || loadedEmbossings.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "No Embossings Found",
                $"No EmbossingEffect assets found at Resources/{embossingResourcePath}.\n\n" +
                "Please check:\n" +
                "1. The path is correct\n" +
                "2. EmbossingEffect assets exist in that folder\n" +
                "3. Assets are in a Resources folder",
                "OK");
            return;
        }
        
        Debug.Log($"[CreateEmbossingDatabase] Found {loadedEmbossings.Length} embossing effects");
        
        // Filter out null entries and validate
        List<EmbossingEffect> validEmbossings = new List<EmbossingEffect>();
        HashSet<string> seenIds = new HashSet<string>();
        int duplicateCount = 0;
        int nullCount = 0;
        int emptyIdCount = 0;
        
        foreach (EmbossingEffect embossing in loadedEmbossings)
        {
            if (embossing == null)
            {
                nullCount++;
                continue;
            }
            
            string id = embossing.embossingId;
            if (string.IsNullOrEmpty(id))
            {
                emptyIdCount++;
                Debug.LogWarning($"[CreateEmbossingDatabase] Embossing '{embossing.embossingName}' has no ID, skipping");
                continue;
            }
            
            id = id.Trim();
            if (seenIds.Contains(id))
            {
                duplicateCount++;
                Debug.LogWarning($"[CreateEmbossingDatabase] Duplicate embossing ID '{id}', skipping '{embossing.embossingName}'");
                continue;
            }
            
            seenIds.Add(id);
            validEmbossings.Add(embossing);
        }
        
        if (validEmbossings.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "No Valid Embossings",
                $"Found {loadedEmbossings.Length} embossing effects, but none were valid.\n\n" +
                $"Null entries: {nullCount}\n" +
                $"Empty IDs: {emptyIdCount}\n" +
                $"Duplicates: {duplicateCount}",
                "OK");
            return;
        }
        
        Debug.Log($"[CreateEmbossingDatabase] Valid embossings: {validEmbossings.Count} (skipped {nullCount + emptyIdCount + duplicateCount})");
        
        // Load or create database
        EmbossingDatabaseAsset database = AssetDatabase.LoadAssetAtPath<EmbossingDatabaseAsset>(databaseSavePath);
        
        if (database == null)
        {
            // Create new database
            database = ScriptableObject.CreateInstance<EmbossingDatabaseAsset>();
            AssetDatabase.CreateAsset(database, databaseSavePath);
            Debug.Log($"[CreateEmbossingDatabase] Created new database at {databaseSavePath}");
        }
        else
        {
            Debug.Log($"[CreateEmbossingDatabase] Updating existing database at {databaseSavePath}");
        }
        
        // Populate database
        database.SetEmbossings(validEmbossings);
        
        // Mark as dirty and save
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        loadedCount = validEmbossings.Count;
        
        // Show summary
        string summary = $"Database created/updated successfully!\n\n" +
                        $"Total embossings: {validEmbossings.Count}\n" +
                        $"Skipped: {nullCount + emptyIdCount + duplicateCount}\n" +
                        $"  - Null entries: {nullCount}\n" +
                        $"  - Empty IDs: {emptyIdCount}\n" +
                        $"  - Duplicates: {duplicateCount}\n\n" +
                        $"Database saved to: {databaseSavePath}";
        
        EditorUtility.DisplayDialog("Success", summary, "OK");
        
        // Select the database in project window
        Selection.activeObject = database;
        EditorGUIUtility.PingObject(database);
    }
    
    private void FindAllEmbossings()
    {
        // Find all EmbossingEffect assets in the project
        string[] guids = AssetDatabase.FindAssets("t:EmbossingEffect");
        
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Embossings Found", "No EmbossingEffect assets found in the project.", "OK");
            return;
        }
        
        // Group by folder
        Dictionary<string, List<string>> folders = new Dictionary<string, List<string>>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string folder = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            
            if (!folders.ContainsKey(folder))
            {
                folders[folder] = new List<string>();
            }
            
            folders[folder].Add(path);
        }
        
        // Display results
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField($"Found {guids.Length} EmbossingEffect assets:", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        foreach (var folder in folders.OrderBy(f => f.Key))
        {
            EditorGUILayout.LabelField($"{folder.Key} ({folder.Value.Count} assets)", EditorStyles.boldLabel);
            
            foreach (string path in folder.Value)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(System.IO.Path.GetFileName(path), GUILayout.Width(200));
                
                if (path.Contains("Resources"))
                {
                    string resourcePath = ExtractResourcePath(path);
                    EditorGUILayout.LabelField($"Resources/{resourcePath}", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("(Not in Resources folder)", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
        }
        
        EditorGUILayout.EndScrollView();
        
        // Suggest resource path
        if (folders.Count > 0)
        {
            var resourcesFolders = folders.Keys.Where(f => f.Contains("Resources")).ToList();
            if (resourcesFolders.Count > 0)
            {
                string suggestedPath = ExtractResourcePath(resourcesFolders[0]);
                embossingResourcePath = suggestedPath;
                EditorUtility.DisplayDialog(
                    "Found Embossings",
                    $"Found {guids.Length} EmbossingEffect assets.\n\n" +
                    $"Suggested resource path: {suggestedPath}\n\n" +
                    "This path has been set in the tool.",
                    "OK");
            }
        }
    }
    
    private string ExtractResourcePath(string fullPath)
    {
        // Extract path relative to Resources folder
        int resourcesIndex = fullPath.IndexOf("/Resources/");
        if (resourcesIndex >= 0)
        {
            string afterResources = fullPath.Substring(resourcesIndex + "/Resources/".Length);
            // Remove file extension
            int lastSlash = afterResources.LastIndexOf('/');
            if (lastSlash >= 0)
            {
                return afterResources.Substring(0, lastSlash);
            }
            return afterResources;
        }
        return "";
    }
}

