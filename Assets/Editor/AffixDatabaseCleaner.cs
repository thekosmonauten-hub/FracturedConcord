using UnityEngine;
using UnityEditor;

public class AffixDatabaseCleaner : EditorWindow
{
    private AffixDatabase affixDatabase;
    
    [MenuItem("Dexiled/Clear Affix Database")]
    public static void ShowWindow()
    {
        AffixDatabaseCleaner window = GetWindow<AffixDatabaseCleaner>("Clear Affix Database");
        window.minSize = new Vector2(400, 250);
    }
    
    void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Clear Affix Database", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool will CLEAR ALL AFFIXES from the database.\n\n" +
            "Use this before re-importing affixes from CSV to ensure old affixes with incorrect tags are removed.\n\n" +
            "⚠️ WARNING: This action cannot be undone!",
            MessageType.Warning
        );
        
        EditorGUILayout.Space(10);
        
        // Affix Database Selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Affix Database:", GUILayout.Width(100));
        affixDatabase = (AffixDatabase)EditorGUILayout.ObjectField(affixDatabase, typeof(AffixDatabase), false);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        if (affixDatabase != null)
        {
            int totalAffixes = CountAffixes();
            EditorGUILayout.LabelField($"Current affix count: {totalAffixes}", EditorStyles.boldLabel);
            
            EditorGUILayout.Space(10);
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("⚠️ CLEAR ALL AFFIXES ⚠️", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog(
                    "Confirm Clear",
                    $"Are you sure you want to delete all {totalAffixes} affixes from {affixDatabase.name}?\n\n" +
                    "This action cannot be undone!",
                    "Yes, Clear All",
                    "Cancel"))
                {
                    ClearAllAffixes();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("Please select an Affix Database to clear.", MessageType.Info);
        }
    }
    
    private int CountAffixes()
    {
        int count = 0;
        
        foreach (var category in affixDatabase.weaponPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                count += subCategory.affixes.Count;
            }
        }
        
        foreach (var category in affixDatabase.weaponSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                count += subCategory.affixes.Count;
            }
        }
        
        foreach (var category in affixDatabase.armourPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                count += subCategory.affixes.Count;
            }
        }
        
        foreach (var category in affixDatabase.armourSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                count += subCategory.affixes.Count;
            }
        }
        
        foreach (var category in affixDatabase.jewelleryPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                count += subCategory.affixes.Count;
            }
        }
        
        foreach (var category in affixDatabase.jewellerySuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                count += subCategory.affixes.Count;
            }
        }
        
        return count;
    }
    
    private void ClearAllAffixes()
    {
        int clearedCount = 0;
        
        // Clear weapon prefixes
        foreach (var category in affixDatabase.weaponPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                clearedCount += subCategory.affixes.Count;
                subCategory.affixes.Clear();
            }
            category.subCategories.Clear();
        }
        affixDatabase.weaponPrefixCategories.Clear();
        
        // Clear weapon suffixes
        foreach (var category in affixDatabase.weaponSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                clearedCount += subCategory.affixes.Count;
                subCategory.affixes.Clear();
            }
            category.subCategories.Clear();
        }
        affixDatabase.weaponSuffixCategories.Clear();
        
        // Clear armour prefixes
        foreach (var category in affixDatabase.armourPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                clearedCount += subCategory.affixes.Count;
                subCategory.affixes.Clear();
            }
            category.subCategories.Clear();
        }
        affixDatabase.armourPrefixCategories.Clear();
        
        // Clear armour suffixes
        foreach (var category in affixDatabase.armourSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                clearedCount += subCategory.affixes.Count;
                subCategory.affixes.Clear();
            }
            category.subCategories.Clear();
        }
        affixDatabase.armourSuffixCategories.Clear();
        
        // Clear jewellery prefixes
        foreach (var category in affixDatabase.jewelleryPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                clearedCount += subCategory.affixes.Count;
                subCategory.affixes.Clear();
            }
            category.subCategories.Clear();
        }
        affixDatabase.jewelleryPrefixCategories.Clear();
        
        // Clear jewellery suffixes
        foreach (var category in affixDatabase.jewellerySuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                clearedCount += subCategory.affixes.Count;
                subCategory.affixes.Clear();
            }
            category.subCategories.Clear();
        }
        affixDatabase.jewellerySuffixCategories.Clear();
        
        // Mark as dirty and save
        EditorUtility.SetDirty(affixDatabase);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>✅ Cleared {clearedCount} affixes from {affixDatabase.name}</color>");
        EditorUtility.DisplayDialog("Clear Complete", 
            $"Successfully cleared {clearedCount} affixes from the database!\n\n" +
            "You can now re-import affixes from CSV with the updated importer.", "OK");
    }
}








