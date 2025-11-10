using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateFreshAffixDatabase : EditorWindow
{
    private string databaseName = "AffixDatabase_Fresh";
    private string outputPath = "Assets/Resources";
    
    [MenuItem("Dexiled/Create Fresh Affix Database")]
    public static void ShowWindow()
    {
        CreateFreshAffixDatabase window = GetWindow<CreateFreshAffixDatabase>("Create Fresh Affix Database");
        window.minSize = new Vector2(450, 300);
    }
    
    void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Create Fresh Affix Database", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This creates a brand new AffixDatabase ScriptableObject with no old data.\n\n" +
            "✅ Clean slate - no old affixes with wrong tags\n" +
            "✅ Ready to import from CSV with updated importer\n" +
            "✅ Won't affect your old database (safe!)\n\n" +
            "After creating, use the Affix CSV Importer to populate it!",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        // Database Name
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Database Name:", GUILayout.Width(120));
        databaseName = EditorGUILayout.TextField(databaseName);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Output Path
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Path:", GUILayout.Width(120));
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                outputPath = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            $"Will create: {outputPath}/{databaseName}.asset",
            MessageType.None
        );
        
        EditorGUILayout.Space(10);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("✅ Create Fresh Database", GUILayout.Height(40)))
        {
            CreateDatabase();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "NEXT STEPS:\n" +
            "1. Create this fresh database\n" +
            "2. Dexiled → Import Affixes from CSV\n" +
            "3. Select your NEW database\n" +
            "4. Import from Comprehensive_Mods.csv\n" +
            "5. Update AreaLootManager to use NEW database",
            MessageType.Info
        );
    }
    
    private void CreateDatabase()
    {
        if (string.IsNullOrEmpty(databaseName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a database name!", "OK");
            return;
        }
        
        // Ensure output path exists
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        string fullPath = $"{outputPath}/{databaseName}.asset";
        
        // Check if file already exists
        if (File.Exists(fullPath))
        {
            if (!EditorUtility.DisplayDialog(
                "File Exists",
                $"A file already exists at {fullPath}\n\nOverwrite it?",
                "Yes, Overwrite",
                "Cancel"))
            {
                return;
            }
        }
        
        // Create fresh MODERN AffixDatabase
        AffixDatabase_Modern freshDatabase = ScriptableObject.CreateInstance<AffixDatabase_Modern>();
        
        // Initialize empty lists (they should already be initialized, but just to be safe)
        if (freshDatabase.weaponPrefixCategories == null)
            freshDatabase.weaponPrefixCategories = new System.Collections.Generic.List<AffixCategory>();
        if (freshDatabase.weaponSuffixCategories == null)
            freshDatabase.weaponSuffixCategories = new System.Collections.Generic.List<AffixCategory>();
        if (freshDatabase.armourPrefixCategories == null)
            freshDatabase.armourPrefixCategories = new System.Collections.Generic.List<AffixCategory>();
        if (freshDatabase.armourSuffixCategories == null)
            freshDatabase.armourSuffixCategories = new System.Collections.Generic.List<AffixCategory>();
        if (freshDatabase.jewelleryPrefixCategories == null)
            freshDatabase.jewelleryPrefixCategories = new System.Collections.Generic.List<AffixCategory>();
        if (freshDatabase.jewellerySuffixCategories == null)
            freshDatabase.jewellerySuffixCategories = new System.Collections.Generic.List<AffixCategory>();
        
        // Create the asset
        AssetDatabase.CreateAsset(freshDatabase, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select it in the project view
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = freshDatabase;
        EditorGUIUtility.PingObject(freshDatabase);
        
        Debug.Log($"<color=green>✅ Created fresh AffixDatabase at: {fullPath}</color>");
        Debug.Log($"<color=yellow>NEXT: Use 'Dexiled → Import Affixes from CSV' and select this new database!</color>");
        
        EditorUtility.DisplayDialog(
            "Success!",
            $"Created fresh AffixDatabase at:\n{fullPath}\n\n" +
            "NEXT STEPS:\n" +
            "1. Open: Dexiled → Import Affixes from CSV\n" +
            "2. Select THIS new database\n" +
            "3. Import from Comprehensive_Mods.csv\n" +
            "4. Update AreaLootManager to use this database",
            "OK"
        );
    }
}

