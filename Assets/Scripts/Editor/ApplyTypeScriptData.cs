using UnityEngine;
using UnityEditor;
using PassiveTree;

/// <summary>
/// Simple script to apply TypeScript data to the existing CorePassiveTreeData asset
/// </summary>
public class ApplyTypeScriptData
{
    [MenuItem("Tools/Passive Tree/Apply TypeScript Data to Core Asset")]
    public static void ApplyTypeScriptDataToCoreAsset()
    {
        // Find the existing CorePassiveTreeData asset
        string[] guids = AssetDatabase.FindAssets("CorePassiveTreeData t:PassiveTreeBoardData");
        
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "CorePassiveTreeData asset not found. Please create it first.", "OK");
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        PassiveTreeBoardData existingAsset = AssetDatabase.LoadAssetAtPath<PassiveTreeBoardData>(assetPath);

        if (existingAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not load CorePassiveTreeData asset.", "OK");
            return;
        }

        if (EditorUtility.DisplayDialog("Apply TypeScript Data", 
            $"This will overwrite the current data in {assetPath} with TypeScript data. Continue?", 
            "Yes", "Cancel"))
        {
            // Create converted data
            PassiveTreeBoardData convertedData = TypeScriptDataConverter.ConvertCoreBoardData();
            
            // Copy the converted data to the existing asset
            CopyBoardDataToAsset(convertedData, existingAsset);
            
            // Mark as dirty and save
            EditorUtility.SetDirty(existingAsset);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[ApplyTypeScriptData] Successfully applied TypeScript data to {assetPath}");
            EditorUtility.DisplayDialog("Success", "TypeScript data has been applied to the CorePassiveTreeData asset!", "OK");
        }
    }

    /// <summary>
    /// Copy board data from source to target asset
    /// </summary>
    private static void CopyBoardDataToAsset(PassiveTreeBoardData source, PassiveTreeBoardData target)
    {
        // Copy all serialized fields
        var fields = typeof(PassiveTreeBoardData).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            // Check if field has SerializeField attribute
            if (field.GetCustomAttributes(typeof(SerializeField), false).Length > 0)
            {
                object value = field.GetValue(source);
                field.SetValue(target, value);
            }
        }
        
        Debug.Log("[ApplyTypeScriptData] Copied all fields from converted data to existing asset");
    }
}
