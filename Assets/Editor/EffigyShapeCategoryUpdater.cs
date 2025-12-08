using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Editor tool to update all effigy assets with their shape categories
/// </summary>
public class EffigyShapeCategoryUpdater : EditorWindow
{
    [MenuItem("Tools/Effigy/Update All Shape Categories")]
    public static void ShowWindow()
    {
        GetWindow<EffigyShapeCategoryUpdater>("Effigy Shape Category Updater");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Effigy Shape Category Updater", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool will update all effigy assets in the project to set their shapeCategory field " +
            "based on their name. This ensures effigies can find the correct affix pools.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Update All Effigies", GUILayout.Height(30)))
        {
            UpdateAllEffigies();
        }
    }
    
    private void UpdateAllEffigies()
    {
        // Find all effigy assets
        string[] guids = AssetDatabase.FindAssets("t:Effigy");
        int updatedCount = 0;
        int skippedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Effigy effigy = AssetDatabase.LoadAssetAtPath<Effigy>(path);
            
            if (effigy == null)
                continue;
            
            // Get current shape category
            EffigyShapeCategory currentCategory = effigy.shapeCategory;
            
            // Force auto-detect from name (bypasses GetShapeCategory's check)
            EffigyShapeCategory detectedCategory = effigy.DetectShapeCategoryFromName();
            
            if (currentCategory == EffigyShapeCategory.Unknown && detectedCategory != EffigyShapeCategory.Unknown)
            {
                effigy.shapeCategory = detectedCategory;
                EditorUtility.SetDirty(effigy);
                updatedCount++;
                Debug.Log($"[EffigyShapeCategoryUpdater] Updated '{effigy.effigyName}' ({path}): {detectedCategory}");
            }
            else if (currentCategory != EffigyShapeCategory.Unknown)
            {
                // Optionally update if detected differs (for fixing incorrect assignments)
                if (detectedCategory != EffigyShapeCategory.Unknown && detectedCategory != currentCategory)
                {
                    Debug.LogWarning($"[EffigyShapeCategoryUpdater] '{effigy.effigyName}' has category {currentCategory} but name suggests {detectedCategory}. Keeping current value.");
                }
                skippedCount++;
            }
            else
            {
                skippedCount++;
                Debug.LogWarning($"[EffigyShapeCategoryUpdater] Could not detect shape for '{effigy.effigyName}' ({path})");
            }
        }
        
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog(
            "Update Complete",
            $"Updated {updatedCount} effigies.\nSkipped {skippedCount} effigies (already set or unknown).",
            "OK"
        );
        
        Debug.Log($"[EffigyShapeCategoryUpdater] Update complete: {updatedCount} updated, {skippedCount} skipped");
    }
}

