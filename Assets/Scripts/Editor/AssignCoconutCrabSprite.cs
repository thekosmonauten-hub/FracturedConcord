using UnityEngine;
using UnityEditor;

/// <summary>
/// Helper to assign Coconut Crab sprite.
/// Menu: Tools > Enemy Data > Assign Coconut Crab Sprite
/// </summary>
public class AssignCoconutCrabSprite : EditorWindow
{
    [MenuItem("Tools/Enemy Data/Assign Coconut Crab Sprite")]
    public static void AssignSprite()
    {
        // Load the Coconut Crab enemy data
        EnemyData coconutCrab = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/Resources/Enemies/CoconutCrab.asset");
        
        if (coconutCrab == null)
        {
            Debug.LogError("CoconutCrab.asset not found at Assets/Resources/Enemies/CoconutCrab.asset");
            return;
        }
        
        // Try to find sprite in Art/Enemies folder
        string[] spritePaths = new string[]
        {
            "Assets/Art/Enemies/CoconutCrab.png",
            "Assets/Art/Enemies/coconut_crab.png",
            "Assets/Art/Enemies/Coconut Crab.png",
            "Assets/Sprites/Enemies/CoconutCrab.png",
            "Assets/Sprites/CoconutCrab.png"
        };
        
        Sprite sprite = null;
        foreach (string path in spritePaths)
        {
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                Debug.Log($"Found sprite at: {path}");
                break;
            }
        }
        
        if (sprite != null)
        {
            coconutCrab.enemySprite = sprite;
            EditorUtility.SetDirty(coconutCrab);
            AssetDatabase.SaveAssets();
            Debug.Log($"✓ Assigned sprite to CoconutCrab!");
        }
        else
        {
            Debug.LogWarning("Coconut Crab sprite not found! Please export CoconutCrab.aseprite to PNG first.");
            Debug.Log("Steps:");
            Debug.Log("1. Open CoconutCrab.aseprite in Aseprite");
            Debug.Log("2. File → Export Sprite Sheet");
            Debug.Log("3. Save as: Assets/Art/Enemies/CoconutCrab.png");
            Debug.Log("4. Run this tool again");
        }
    }
    
    [MenuItem("Tools/Enemy Data/Check Coconut Crab Setup")]
    public static void CheckSetup()
    {
        Debug.Log("=== COCONUT CRAB SETUP CHECK ===");
        
        // Check asset
        EnemyData coconutCrab = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/Resources/Enemies/CoconutCrab.asset");
        if (coconutCrab != null)
        {
            Debug.Log($"✓ CoconutCrab.asset found");
            Debug.Log($"  Name: {coconutCrab.enemyName}");
            Debug.Log($"  HP: {coconutCrab.minHealth}-{coconutCrab.maxHealth}");
            Debug.Log($"  Damage: {coconutCrab.baseDamage}");
            
            if (coconutCrab.enemySprite != null)
            {
                Debug.Log($"  ✓ Sprite assigned: {coconutCrab.enemySprite.name}");
            }
            else
            {
                Debug.LogWarning("  ✗ No sprite assigned!");
            }
        }
        else
        {
            Debug.LogError("✗ CoconutCrab.asset not found!");
        }
        
        // Check sprite file
        Object aseprite = AssetDatabase.LoadAssetAtPath<Object>("Assets/Art/Enemies/CoconutCrab.aseprite");
        if (aseprite != null)
        {
            Debug.Log($"✓ CoconutCrab.aseprite found (needs to be exported to PNG)");
        }
        
        // Check if EnemyDatabase has it
        if (EnemyDatabase.Instance != null)
        {
            EnemyData fromDB = EnemyDatabase.Instance.GetEnemyByName("CoconutCrab");
            if (fromDB != null)
            {
                Debug.Log($"✓ CoconutCrab in EnemyDatabase");
            }
            else
            {
                Debug.LogWarning("CoconutCrab not in database - run 'Reload and Organize'");
            }
        }
    }
}

