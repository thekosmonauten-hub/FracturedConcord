using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Editor tool to bulk import all effigies from Resources/Items/Effigies to all AreaLootTable assets.
/// This tool will:
/// 1. Find all effigies in Resources/Items/Effigies
/// 2. Find all AreaLootTable assets in Resources/LootTables
/// 3. Add all effigies to each AreaLootTable's effigyDrops array with configurable settings
/// </summary>
public class AreaLootTableEffigyImporter : EditorWindow
{
    private string effigyPath = "Assets/Resources/Items/Effigies";
    private string lootTablePath = "Assets/Resources/LootTables";
    
    [Header("Drop Settings")]
    [SerializeField] private int minAreaLevel = 1;
    [SerializeField] private int maxAreaLevel = 100;
    [SerializeField] private float dropChanceAtMinLevel = 0f;
    [SerializeField] private float dropChanceAtMaxLevel = 0.05f;
    [SerializeField] private int maxQuantity = 1;
    
    [Header("Options")]
    [SerializeField] private bool clearExistingEffigies = false;
    [SerializeField] private bool onlyAddMissingEffigies = true;
    
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Dexiled/Bulk Import Effigies to Area Loot Tables")]
    public static void ShowWindow()
    {
        GetWindow<AreaLootTableEffigyImporter>("Effigy Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Bulk Import Effigies to Area Loot Tables", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool will find all effigies in Resources/Items/Effigies and add them to all AreaLootTable assets in Resources/LootTables.\n\n" +
            "The effigy drop rate is controlled by the 'Effigy' weight in ItemTypeWeights. All effigies share this weight equally.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // Paths
        EditorGUILayout.LabelField("Paths", EditorStyles.boldLabel);
        effigyPath = EditorGUILayout.TextField("Effigy Path:", effigyPath);
        lootTablePath = EditorGUILayout.TextField("Loot Table Path:", lootTablePath);
        
        EditorGUILayout.Space();
        
        // Drop settings
        EditorGUILayout.LabelField("Drop Settings", EditorStyles.boldLabel);
        minAreaLevel = EditorGUILayout.IntField("Min Area Level:", minAreaLevel);
        maxAreaLevel = EditorGUILayout.IntField("Max Area Level:", maxAreaLevel);
        dropChanceAtMinLevel = EditorGUILayout.Slider("Drop Chance at Min Level:", dropChanceAtMinLevel, 0f, 1f);
        dropChanceAtMaxLevel = EditorGUILayout.Slider("Drop Chance at Max Level:", dropChanceAtMaxLevel, 0f, 1f);
        maxQuantity = EditorGUILayout.IntField("Max Quantity:", maxQuantity);
        
        EditorGUILayout.Space();
        
        // Options
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        clearExistingEffigies = EditorGUILayout.Toggle("Clear Existing Effigies:", clearExistingEffigies);
        onlyAddMissingEffigies = EditorGUILayout.Toggle("Only Add Missing Effigies:", onlyAddMissingEffigies);
        
        EditorGUILayout.Space();
        
        // Info
        int effigyCount = CountEffigies();
        int lootTableCount = CountLootTables();
        
        EditorGUILayout.LabelField($"Found {effigyCount} effigies", EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Found {lootTableCount} AreaLootTable assets", EditorStyles.helpBox);
        
        EditorGUILayout.Space();
        
        // Import button
        GUI.enabled = effigyCount > 0 && lootTableCount > 0;
        if (GUILayout.Button("Import All Effigies to All Area Loot Tables", GUILayout.Height(30)))
        {
            ImportEffigies();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        
        // Preview button
        if (GUILayout.Button("Preview Effigies"))
        {
            PreviewEffigies();
        }
    }
    
    private int CountEffigies()
    {
        if (!Directory.Exists(effigyPath))
            return 0;
        
        string[] guids = AssetDatabase.FindAssets("t:Effigy", new[] { effigyPath });
        return guids.Length;
    }
    
    private int CountLootTables()
    {
        if (!Directory.Exists(lootTablePath))
            return 0;
        
        string[] guids = AssetDatabase.FindAssets("t:AreaLootTable", new[] { lootTablePath });
        return guids.Length;
    }
    
    private List<Effigy> GetAllEffigies()
    {
        List<Effigy> effigies = new List<Effigy>();
        
        if (!Directory.Exists(effigyPath))
        {
            Debug.LogError($"[EffigyImporter] Effigy path does not exist: {effigyPath}");
            return effigies;
        }
        
        string[] guids = AssetDatabase.FindAssets("t:Effigy", new[] { effigyPath });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Effigy effigy = AssetDatabase.LoadAssetAtPath<Effigy>(path);
            if (effigy != null)
            {
                effigies.Add(effigy);
            }
        }
        
        // Sort by name for consistency
        effigies = effigies.OrderBy(e => e.effigyName).ToList();
        
        return effigies;
    }
    
    private List<AreaLootTable> GetAllLootTables()
    {
        List<AreaLootTable> lootTables = new List<AreaLootTable>();
        
        if (!Directory.Exists(lootTablePath))
        {
            Debug.LogError($"[EffigyImporter] Loot table path does not exist: {lootTablePath}");
            return lootTables;
        }
        
        string[] guids = AssetDatabase.FindAssets("t:AreaLootTable", new[] { lootTablePath });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AreaLootTable lootTable = AssetDatabase.LoadAssetAtPath<AreaLootTable>(path);
            if (lootTable != null)
            {
                lootTables.Add(lootTable);
            }
        }
        
        return lootTables;
    }
    
    private void PreviewEffigies()
    {
        List<Effigy> effigies = GetAllEffigies();
        
        Debug.Log($"[EffigyImporter] Found {effigies.Count} effigies:");
        foreach (var effigy in effigies)
        {
            Debug.Log($"  - {effigy.effigyName} (Level {effigy.requiredLevel})");
        }
    }
    
    private void ImportEffigies()
    {
        List<Effigy> effigies = GetAllEffigies();
        List<AreaLootTable> lootTables = GetAllLootTables();
        
        if (effigies.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No effigies found!", "OK");
            return;
        }
        
        if (lootTables.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No AreaLootTable assets found!", "OK");
            return;
        }
        
        if (!EditorUtility.DisplayDialog(
            "Confirm Import",
            $"This will import {effigies.Count} effigies to {lootTables.Count} AreaLootTable assets.\n\n" +
            $"Settings:\n" +
            $"  Min Area Level: {minAreaLevel}\n" +
            $"  Max Area Level: {maxAreaLevel}\n" +
            $"  Drop Chance: {dropChanceAtMinLevel:F3} → {dropChanceAtMaxLevel:F3}\n" +
            $"  Max Quantity: {maxQuantity}\n" +
            $"  Clear Existing: {clearExistingEffigies}\n" +
            $"  Only Add Missing: {onlyAddMissingEffigies}\n\n" +
            $"Continue?",
            "Yes", "Cancel"))
        {
            return;
        }
        
        int totalAdded = 0;
        int totalSkipped = 0;
        
        foreach (var lootTable in lootTables)
        {
            if (lootTable == null)
                continue;
            
            Undo.RecordObject(lootTable, "Import Effigies to AreaLootTable");
            
            // Clear existing if requested
            if (clearExistingEffigies)
            {
                lootTable.effigyDrops = new EffigyDropWeight[0];
            }
            
            // Get existing effigy IDs (if only adding missing)
            HashSet<string> existingEffigyIds = new HashSet<string>();
            if (onlyAddMissingEffigies && lootTable.effigyDrops != null)
            {
                foreach (var drop in lootTable.effigyDrops)
                {
                    if (drop != null && drop.effigyBlueprint != null)
                    {
                        existingEffigyIds.Add(drop.effigyBlueprint.effigyName);
                    }
                }
            }
            
            // Create list of drops
            List<EffigyDropWeight> drops = new List<EffigyDropWeight>();
            
            // Add existing drops if not clearing
            if (!clearExistingEffigies && lootTable.effigyDrops != null)
            {
                drops.AddRange(lootTable.effigyDrops);
            }
            
            // Add new effigies
            foreach (var effigy in effigies)
            {
                // Skip if already exists and we're only adding missing
                if (onlyAddMissingEffigies && existingEffigyIds.Contains(effigy.effigyName))
                {
                    totalSkipped++;
                    continue;
                }
                
                EffigyDropWeight drop = new EffigyDropWeight
                {
                    effigyBlueprint = effigy,
                    minAreaLevel = minAreaLevel,
                    maxAreaLevel = maxAreaLevel,
                    dropChanceAtMinLevel = dropChanceAtMinLevel,
                    dropChanceAtMaxLevel = dropChanceAtMaxLevel,
                    maxQuantity = maxQuantity
                };
                
                drops.Add(drop);
                totalAdded++;
            }
            
            // Assign back to loot table
            lootTable.effigyDrops = drops.ToArray();
            
            EditorUtility.SetDirty(lootTable);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog(
            "Import Complete",
            $"Successfully imported effigies!\n\n" +
            $"  Added: {totalAdded}\n" +
            $"  Skipped (already exists): {totalSkipped}\n" +
            $"  Loot Tables Updated: {lootTables.Count}",
            "OK"
        );
        
        Debug.Log($"[EffigyImporter] Import complete: {totalAdded} effigies added, {totalSkipped} skipped, {lootTables.Count} loot tables updated");
    }
}












