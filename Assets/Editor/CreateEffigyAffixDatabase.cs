using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to create the EffigyAffixDatabase asset
/// </summary>
public class CreateEffigyAffixDatabase : EditorWindow
{
    [MenuItem("Dexiled/Create Effigy Affix Database")]
    public static void CreateEffigyAffixDatabaseAsset()
    {
        // Check if database already exists
        EffigyAffixDatabase existing = Resources.Load<EffigyAffixDatabase>("EffigyAffixDatabase");
        if (existing != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Effigy Affix Database Already Exists",
                $"An EffigyAffixDatabase already exists at:\n{AssetDatabase.GetAssetPath(existing)}\n\nDo you want to overwrite it?",
                "Overwrite",
                "Cancel"
            );

            if (!overwrite)
            {
                Debug.Log("[CreateEffigyAffixDatabase] Operation cancelled.");
                return;
            }
        }

        // Create new database
        EffigyAffixDatabase database = ScriptableObject.CreateInstance<EffigyAffixDatabase>();

        // Ensure Resources folder exists
        string resourcesPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        // Create asset
        string assetPath = "Assets/Resources/EffigyAffixDatabase.asset";
        AssetDatabase.CreateAsset(database, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[CreateEffigyAffixDatabase] Created EffigyAffixDatabase at: {assetPath}");
        Debug.Log("[CreateEffigyAffixDatabase] Next steps:");
        Debug.Log("  1. Select the asset in the Project window");
        Debug.Log("  2. Add AffixCategory objects to each shape's prefix/suffix lists");
        Debug.Log("  3. Each category should contain AffixSubCategory objects with Affix assets");
        Debug.Log("  4. See EFFIGY_AFFIX_SYSTEM_ANALYSIS.md for shape-specific affix themes");

        // Select the created asset
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = database;
    }
}

