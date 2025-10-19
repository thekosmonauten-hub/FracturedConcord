using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool to quickly create enemy data assets.
/// Menu: Tools > Enemy Data > Create Enemy
/// </summary>
public class EnemyDataCreator : EditorWindow
{
    private string enemyName = "New Enemy";
    private EnemyTier tier = EnemyTier.Normal;
    private EnemyCategory category = EnemyCategory.Melee;
    private int health = 30;
    private int damage = 6;
    
    [MenuItem("Tools/Enemy Data/Create Enemy")]
    public static void ShowWindow()
    {
        var window = GetWindow<EnemyDataCreator>("Create Enemy Data");
        window.minSize = new Vector2(400, 500);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Create Enemy Data", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        enemyName = EditorGUILayout.TextField("Enemy Name", enemyName);
        tier = (EnemyTier)EditorGUILayout.EnumPopup("Tier", tier);
        category = (EnemyCategory)EditorGUILayout.EnumPopup("Category", category);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
        health = EditorGUILayout.IntSlider("Health", health, 10, 200);
        damage = EditorGUILayout.IntSlider("Damage", damage, 2, 30);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Enemy Data", GUILayout.Height(40)))
        {
            CreateEnemyAsset();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will create an EnemyData ScriptableObject at:\n" +
            $"Assets/Resources/Enemies/{enemyName}.asset",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Quick Test Enemies", GUILayout.Height(30)))
        {
            CreateTestEnemies();
        }
    }
    
    private void CreateEnemyAsset()
    {
        // Create folder if it doesn't exist
        string folderPath = "Assets/Resources/Enemies";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateFolder("Assets/Resources", "Enemies");
        }
        
        // Create enemy data
        EnemyData enemyData = ScriptableObject.CreateInstance<EnemyData>();
        enemyData.enemyName = enemyName;
        enemyData.tier = tier;
        enemyData.category = category;
        enemyData.minHealth = health - 5;
        enemyData.maxHealth = health + 5;
        enemyData.baseDamage = damage;
        
        // Set defaults based on tier
        switch (tier)
        {
            case EnemyTier.Elite:
                enemyData.minHealth = Mathf.RoundToInt(health * 1.5f);
                enemyData.maxHealth = Mathf.RoundToInt(health * 1.5f) + 10;
                enemyData.experienceReward = 25;
                enemyData.cardDropChance = 0.25f;
                break;
            case EnemyTier.Boss:
                enemyData.minHealth = health * 3;
                enemyData.maxHealth = health * 3 + 20;
                enemyData.experienceReward = 100;
                enemyData.cardDropChance = 1.0f;
                break;
        }
        
        // Save asset
        string assetPath = $"{folderPath}/{enemyName}.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(enemyData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = enemyData;
        
        Debug.Log($"Created enemy data: {assetPath}");
    }
    
    private void CreateTestEnemies()
    {
        // Create a variety of test enemies
        CreateTestEnemy("Goblin Scout", EnemyTier.Normal, EnemyCategory.Melee, 25, 5);
        CreateTestEnemy("Orc Warrior", EnemyTier.Normal, EnemyCategory.Tank, 45, 8);
        CreateTestEnemy("Dark Mage", EnemyTier.Normal, EnemyCategory.Caster, 30, 10);
        CreateTestEnemy("Skeleton Archer", EnemyTier.Normal, EnemyCategory.Ranged, 20, 7);
        CreateTestEnemy("Elite Guard", EnemyTier.Elite, EnemyCategory.Tank, 80, 12);
        CreateTestEnemy("Shadow Assassin", EnemyTier.Elite, EnemyCategory.Melee, 50, 15);
        CreateTestEnemy("Dragon", EnemyTier.Boss, EnemyCategory.Caster, 200, 25);
        
        Debug.Log("Created 7 test enemies!");
        
        // Refresh database if it exists
        if (EnemyDatabase.Instance != null)
        {
            EnemyDatabase.Instance.ReloadDatabase();
        }
    }
    
    private void CreateTestEnemy(string name, EnemyTier tier, EnemyCategory category, int hp, int dmg)
    {
        string folderPath = "Assets/Resources/Enemies";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateFolder("Assets/Resources", "Enemies");
        }
        
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = name;
        enemy.tier = tier;
        enemy.category = category;
        enemy.minHealth = hp - 5;
        enemy.maxHealth = hp + 5;
        enemy.baseDamage = dmg;
        
        // Set tier-based rewards
        switch (tier)
        {
            case EnemyTier.Normal:
                enemy.experienceReward = 10;
                enemy.minGoldDrop = 5;
                enemy.maxGoldDrop = 15;
                enemy.cardDropChance = 0.1f;
                break;
            case EnemyTier.Elite:
                enemy.experienceReward = 30;
                enemy.minGoldDrop = 15;
                enemy.maxGoldDrop = 40;
                enemy.cardDropChance = 0.3f;
                break;
            case EnemyTier.Boss:
                enemy.experienceReward = 100;
                enemy.minGoldDrop = 50;
                enemy.maxGoldDrop = 150;
                enemy.cardDropChance = 1.0f;
                break;
        }
        
        string assetPath = $"{folderPath}/{name}.asset";
        AssetDatabase.CreateAsset(enemy, assetPath);
    }
}

