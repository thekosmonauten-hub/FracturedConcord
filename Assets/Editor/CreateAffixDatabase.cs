using UnityEngine;
using UnityEditor;

public class CreateAffixDatabase
{
    [MenuItem("Dexiled/Create Affix Database")]
    public static void CreateAffixDatabaseAsset()
    {
        // Check if Resources folder exists, create if not
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        // Check if AffixDatabase already exists
        AffixDatabase existingDatabase = Resources.Load<AffixDatabase>("AffixDatabase");
        if (existingDatabase != null)
        {
            Debug.Log("AffixDatabase already exists at Assets/Resources/AffixDatabase.asset");
            Selection.activeObject = existingDatabase;
            return;
        }
        
        // Create new AffixDatabase
        AffixDatabase affixDatabase = ScriptableObject.CreateInstance<AffixDatabase>();
        
        // Save the asset
        AssetDatabase.CreateAsset(affixDatabase, "Assets/Resources/AffixDatabase.asset");
        AssetDatabase.SaveAssets();
        
        // Select the created asset
        Selection.activeObject = affixDatabase;
        
        Debug.Log("AffixDatabase created at Assets/Resources/AffixDatabase.asset");
    }
}
