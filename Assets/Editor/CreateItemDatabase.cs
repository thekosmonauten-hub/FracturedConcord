using UnityEngine;
using UnityEditor;

public class CreateItemDatabase : EditorWindow
{
    [MenuItem("Dexiled/Create Item Database")]
    public static void CreateItemDatabaseAsset()
    {
        // Create the ItemDatabase asset
        ItemDatabase itemDatabase = ScriptableObject.CreateInstance<ItemDatabase>();
        
        // Save it to the Resources folder
        string path = "Assets/Resources/ItemDatabase.asset";
        
        // Create the asset
        AssetDatabase.CreateAsset(itemDatabase, path);
        
        // Save the asset
        AssetDatabase.SaveAssets();
        
        // Refresh the asset database
        AssetDatabase.Refresh();
        
        // Select the created asset
        Selection.activeObject = itemDatabase;
        
        Debug.Log($"ItemDatabase created at: {path}");
    }
}
