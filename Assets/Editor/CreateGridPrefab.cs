using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor tool to quickly create grid prefabs with all slots already set up.
/// This is much faster than manually creating grids or using dynamic generation.
/// </summary>
public class CreateGridPrefab : EditorWindow
{
    private GameObject slotPrefab;
    private int gridWidth = 12;
    private int gridHeight = 4;
    private Vector2 cellSize = new Vector2(60, 60);
    private Vector2 spacing = new Vector2(2, 2);
    private string prefabName = "InventoryGrid";
    private string savePath = "Assets/Prefabs/UI/Grids/";
    
    [MenuItem("FracturedConcord/Create Grid Prefab")]
    public static void ShowWindow()
    {
        GetWindow<CreateGridPrefab>("Create Grid Prefab");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Grid Prefab Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool creates a complete grid prefab with all slots already set up.\n\n" +
            "This is MUCH faster than dynamic generation:\n" +
            "- 1 Instantiate call vs. 60+ calls\n" +
            "- Instant load vs. 2+ seconds\n" +
            "- No progressive generation needed",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Slot prefab
        slotPrefab = (GameObject)EditorGUILayout.ObjectField("Slot Prefab", slotPrefab, typeof(GameObject), false);
        if (slotPrefab == null)
        {
            EditorGUILayout.HelpBox("⚠️ Slot Prefab is required!", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        // Grid settings
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Grid Width (Columns)", gridWidth);
        gridHeight = EditorGUILayout.IntField("Grid Height (Rows)", gridHeight);
        cellSize = EditorGUILayout.Vector2Field("Cell Size", cellSize);
        spacing = EditorGUILayout.Vector2Field("Spacing", spacing);
        
        EditorGUILayout.Space();
        
        // Prefab settings
        EditorGUILayout.LabelField("Prefab Settings", EditorStyles.boldLabel);
        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        
        EditorGUILayout.Space();
        
        // Preset buttons
        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("InventoryGrid (12×4)"))
        {
            gridWidth = 12;
            gridHeight = 4;
            cellSize = new Vector2(60, 60);
            spacing = new Vector2(2, 2);
            prefabName = "InventoryGridPrefab_12x4";
        }
        
        if (GUILayout.Button("StashGrid (14×4)"))
        {
            gridWidth = 14;
            gridHeight = 4;
            cellSize = new Vector2(60, 60);
            spacing = new Vector2(2, 2);
            prefabName = "StashGridPrefab_14x4";
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("EffigyGrid (6×20)"))
        {
            gridWidth = 6;
            gridHeight = 20;
            cellSize = new Vector2(60, 60);
            spacing = new Vector2(2, 2);
            prefabName = "EffigyGridPrefab_6x20";
        }
        
        if (GUILayout.Button("EffigyStorage (6×20)"))
        {
            gridWidth = 6;
            gridHeight = 20;
            cellSize = new Vector2(80, 80);
            spacing = new Vector2(10, 10);
            prefabName = "EffigyStoragePrefab_6x20";
        }
        
        if (GUILayout.Button("AuraGrid (4×10)"))
        {
            gridWidth = 4;
            gridHeight = 10;
            cellSize = new Vector2(100, 100);
            spacing = new Vector2(10, 10);
            prefabName = "AuraGridPrefab_4x10";
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Create button
        GUI.enabled = slotPrefab != null;
        if (GUILayout.Button("Create Grid Prefab", GUILayout.Height(30)))
        {
            CreatePrefab();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        
        // Info
        int totalSlots = gridWidth * gridHeight;
        EditorGUILayout.LabelField($"Total Slots: {totalSlots} ({gridWidth} × {gridHeight})", EditorStyles.helpBox);
    }
    
    private void CreatePrefab()
    {
        if (slotPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Slot Prefab is required!", "OK");
            return;
        }
        
        // Ensure directory exists
        if (!System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
            AssetDatabase.Refresh();
        }
        
        // Create root GameObject
        GameObject gridRoot = new GameObject(prefabName);
        GridLayoutGroup gridLayout = gridRoot.AddComponent<GridLayoutGroup>();
        
        // Configure GridLayoutGroup
        gridLayout.cellSize = cellSize;
        gridLayout.spacing = spacing;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridWidth;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        
        // Create all slots
        int totalSlots = gridWidth * gridHeight;
        Debug.Log($"[CreateGridPrefab] Creating {totalSlots} slots ({gridWidth}×{gridHeight})...");
        
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // Instantiate slot prefab
                GameObject slot = PrefabUtility.InstantiatePrefab(slotPrefab) as GameObject;
                slot.transform.SetParent(gridRoot.transform, false);
                slot.name = $"Slot_{x}_{y}";
            }
        }
        
        // Save as prefab
        string fullPath = savePath + prefabName + ".prefab";
        
        // Remove existing prefab if it exists
        if (System.IO.File.Exists(fullPath))
        {
            AssetDatabase.DeleteAsset(fullPath);
        }
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gridRoot, fullPath);
        DestroyImmediate(gridRoot);
        
        // Refresh asset database
        AssetDatabase.Refresh();
        
        // Select the prefab
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        
        // Show success message
        string message = $"Grid prefab created successfully!\n\n" +
                        $"Name: {prefabName}\n" +
                        $"Size: {gridWidth}×{gridHeight} = {totalSlots} slots\n" +
                        $"Path: {fullPath}\n\n" +
                        $"The prefab has been selected in the Project window.";
        
        EditorUtility.DisplayDialog("Success", message, "OK");
        
        Debug.Log($"[CreateGridPrefab] ✅ Created grid prefab: {fullPath}");
    }
}

