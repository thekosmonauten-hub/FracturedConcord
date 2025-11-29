using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Dexiled.MazeSystem;

/// <summary>
/// Editor tool to automatically assign maze enemies to MazeConfig tier pools.
/// Scans Resources/Enemies/Maze of Broken Clauses and assigns enemies based on their tier.
/// </summary>
public class MazeConfigEnemyAssigner : EditorWindow
{
    private MazeConfig targetConfig;
    private string mazeEnemiesPath = "Assets/Resources/Enemies/Maze of Broken Clauses";
    
    [MenuItem("Tools/Maze Enemies/Assign Enemies to MazeConfig")]
    public static void ShowWindow()
    {
        GetWindow<MazeConfigEnemyAssigner>("Maze Config Enemy Assigner");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Maze Config Enemy Assigner", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        targetConfig = (MazeConfig)EditorGUILayout.ObjectField("Maze Config:", targetConfig, typeof(MazeConfig), false);
        mazeEnemiesPath = EditorGUILayout.TextField("Enemies Path:", mazeEnemiesPath);
        
        GUILayout.Space(10);
        
        if (targetConfig == null)
        {
            EditorGUILayout.HelpBox("Please assign a MazeConfig ScriptableObject.", MessageType.Warning);
            return;
        }
        
        if (GUILayout.Button("Auto-Assign Enemies to Tiers", GUILayout.Height(30)))
        {
            AssignEnemiesToTiers();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Clear All Tier Pools", GUILayout.Height(25)))
        {
            ClearAllTiers();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("This will:\n" +
            "• Scan for all EnemyData assets in the maze enemies folder\n" +
            "• Add all enemies to the maze pool (mazeEnemies)\n" +
            "• All enemies can spawn at any tier\n" +
            "• Rarity is rolled per spawn based on tier weights\n" +
            "• Higher tiers have higher chance of rare enemies", MessageType.Info);
    }
    
    private void AssignEnemiesToTiers()
    {
        if (targetConfig == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a MazeConfig first!", "OK");
            return;
        }
        
        if (!Directory.Exists(mazeEnemiesPath))
        {
            EditorUtility.DisplayDialog("Error", $"Path not found: {mazeEnemiesPath}", "OK");
            return;
        }
        
        // Clear existing pool
        targetConfig.mazeEnemies.Clear();
        
        // Find all EnemyData assets in the maze folder
        string[] guids = AssetDatabase.FindAssets("t:EnemyData", new[] { mazeEnemiesPath });
        
        int assignedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            
            if (enemy == null) continue;
            
            // Skip if it's in an Abilities subfolder (those are not enemy assets)
            if (path.Contains("/Abilities/")) continue;
            
            // Add all maze enemies to the pool (they can spawn at any tier with different rarities)
            if (!targetConfig.mazeEnemies.Contains(enemy))
            {
                targetConfig.mazeEnemies.Add(enemy);
                assignedCount++;
            }
        }
        
        // Mark config as dirty and save
        EditorUtility.SetDirty(targetConfig);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Success", 
            $"Assigned {assignedCount} enemies to maze pool.\n\n" +
            $"All enemies can spawn at any tier with different rarities based on tier weights:\n" +
            $"• Tier 1: {targetConfig.tier1NormalWeight}% Normal, {targetConfig.tier1MagicWeight}% Magic, {targetConfig.tier1RareWeight}% Rare\n" +
            $"• Tier 2: {targetConfig.tier2NormalWeight}% Normal, {targetConfig.tier2MagicWeight}% Magic, {targetConfig.tier2RareWeight}% Rare\n" +
            $"• Tier 3: {targetConfig.tier3NormalWeight}% Normal, {targetConfig.tier3MagicWeight}% Magic, {targetConfig.tier3RareWeight}% Rare\n" +
            $"• Tier 4: {targetConfig.tier4NormalWeight}% Normal, {targetConfig.tier4MagicWeight}% Magic, {targetConfig.tier4RareWeight}% Rare", 
            "OK");
    }
    
    private void ClearAllTiers()
    {
        if (targetConfig == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a MazeConfig first!", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Clear Maze Pool", 
            "Are you sure you want to clear the maze enemy pool?", 
            "Yes", "Cancel"))
        {
            targetConfig.mazeEnemies.Clear();
            
            EditorUtility.SetDirty(targetConfig);
            AssetDatabase.SaveAssets();
            
            EditorUtility.DisplayDialog("Success", "All tier pools cleared.", "OK");
        }
    }
}

