using UnityEngine;
using UnityEditor;

/// <summary>
/// Helper script to create a WarrantNotableDatabase asset.
/// Menu: Tools > Warrants > Create Warrant Notable Database
/// </summary>
public class CreateWarrantNotableDatabase : EditorWindow
{
    [MenuItem("Tools/Warrants/Create Warrant Notable Database")]
    public static void CreateDatabase()
    {
        // Default save path
        string savePath = "Assets/Resources/Warrants/WarrantNotableDatabase.asset";
        
        // Check if directory exists
        string directory = System.IO.Path.GetDirectoryName(savePath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }
        
        // Check if asset already exists
        var existing = AssetDatabase.LoadAssetAtPath<WarrantNotableDatabase>(savePath);
        if (existing != null)
        {
            Debug.LogWarning($"Warrant Notable Database already exists at: {savePath}");
            Selection.activeObject = existing;
            EditorGUIUtility.PingObject(existing);
            return;
        }
        
        // Create new database
        WarrantNotableDatabase database = ScriptableObject.CreateInstance<WarrantNotableDatabase>();
        
        // Save the asset
        AssetDatabase.CreateAsset(database, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>✅ Created Warrant Notable Database at: {savePath}</color>");
        
        // Select the new asset
        Selection.activeObject = database;
        EditorGUIUtility.PingObject(database);
    }
}

