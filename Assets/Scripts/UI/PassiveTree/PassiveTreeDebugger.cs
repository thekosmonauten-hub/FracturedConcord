using UnityEngine;
using System.Linq;
using PassiveTree;

/// <summary>
/// Comprehensive debug script for passive tree tooltip system
/// Use this to diagnose tooltip issues and verify JSON data loading
/// </summary>
public class PassiveTreeDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool debugOnStart = true;
    [SerializeField] private bool enableVerboseLogging = true;
    [SerializeField] private bool testAllCells = false;
    
    [Header("Test Settings")]
    [SerializeField] private Vector2Int testCellPosition = new Vector2Int(0, 0);
    [SerializeField] private bool simulateHover = false;

    void Start()
    {
        if (debugOnStart)
        {
            Debug.Log("🔧 [PassiveTreeDebugger] Starting comprehensive debug...");
            RunFullDiagnostic();
        }
    }

    /// <summary>
    /// Run complete diagnostic of the passive tree system
    /// </summary>
    [ContextMenu("Run Full Diagnostic")]
    public void RunFullDiagnostic()
    {
        Debug.Log("🔧 [PassiveTreeDebugger] ===== FULL DIAGNOSTIC START =====");
        
        // 1. Check JSON data loading
        CheckJsonDataLoading();
        
        // 2. Check cell creation and data assignment
        CheckCellDataAssignment();
        
        // 3. Check tooltip system components
        CheckTooltipSystemComponents();
        
        // 4. Check event system
        CheckEventSystem();
        
        // 5. Test specific cell if requested
        if (testAllCells)
        {
            TestAllCells();
        }
        else
        {
            TestSpecificCell(testCellPosition);
        }
        
        Debug.Log("🔧 [PassiveTreeDebugger] ===== FULL DIAGNOSTIC COMPLETE =====");
    }

    /// <summary>
    /// Check if JSON data is properly loaded
    /// </summary>
    private void CheckJsonDataLoading()
    {
        Debug.Log("📊 [PassiveTreeDebugger] Checking JSON data loading...");
        
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (dataManager == null)
        {
            Debug.LogError("❌ [PassiveTreeDebugger] JsonBoardDataManager not found!");
            return;
        }
        
        Debug.Log($"✅ [PassiveTreeDebugger] JsonBoardDataManager found: {dataManager.name}");
        Debug.Log($"  - Is Data Loaded: {dataManager.IsDataLoaded}");
        Debug.Log($"  - JSON File: {dataManager.BoardDataJson?.name ?? "None"}");
        
        if (dataManager.IsDataLoaded)
        {
            var allNodeData = dataManager.GetAllNodeData();
            Debug.Log($"  - Total Nodes Loaded: {allNodeData.Count}");
            
            // Show first few nodes as examples
            int count = 0;
            foreach (var kvp in allNodeData.Take(5))
            {
                var node = kvp.Value;
                Debug.Log($"    Node {count + 1}: '{node.name}' at {kvp.Key} - Type: {node.type}");
                count++;
            }
            
            if (allNodeData.Count > 5)
            {
                Debug.Log($"    ... and {allNodeData.Count - 5} more nodes");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ [PassiveTreeDebugger] JSON data is not loaded!");
        }
    }

    /// <summary>
    /// Check if cells are created and have proper data
    /// </summary>
    private void CheckCellDataAssignment()
    {
        Debug.Log("🔲 [PassiveTreeDebugger] Checking cell data assignment...");
        
        var cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        Debug.Log($"✅ [PassiveTreeDebugger] Found {cells.Length} CellController components");
        
        if (cells.Length == 0)
        {
            Debug.LogError("❌ [PassiveTreeDebugger] No CellController components found!");
            return;
        }
        
        // Check first few cells
        int cellsToCheck = Mathf.Min(5, cells.Length);
        for (int i = 0; i < cellsToCheck; i++)
        {
            var cell = cells[i];
            Debug.Log($"  Cell {i + 1}: Position {cell.GridPosition}");
            Debug.Log($"    - Node Name: '{cell.NodeName}'");
            Debug.Log($"    - Description: '{cell.NodeDescription}'");
            Debug.Log($"    - Node Type: {cell.NodeType}");
            Debug.Log($"    - Is Available: {cell.IsAvailable}");
            Debug.Log($"    - Is Unlocked: {cell.IsUnlocked}");
        }
        
        if (cells.Length > 5)
        {
            Debug.Log($"  ... and {cells.Length - 5} more cells");
        }
    }

    /// <summary>
    /// Check tooltip system components
    /// </summary>
    private void CheckTooltipSystemComponents()
    {
        Debug.Log("💬 [PassiveTreeDebugger] Checking tooltip system components...");
        
        // Check JsonPassiveTreeTooltip
        var jsonTooltip = FindFirstObjectByType<JsonPassiveTreeTooltip>();
        Debug.Log($"  - JsonPassiveTreeTooltip: {(jsonTooltip != null ? "✅ Found" : "❌ Missing")}");
        
        if (jsonTooltip != null)
        {
            Debug.Log($"    - Tooltip Prefab: {(jsonTooltip.TooltipPrefab != null ? "✅ Assigned" : "❌ Not assigned")}");
            Debug.Log($"    - Data Manager: {(jsonTooltip.DataManager != null ? "✅ Connected" : "❌ Not connected")}");
        }
        
        // Check PassiveTreeTooltip
        var passiveTooltip = FindFirstObjectByType<PassiveTreeTooltip>();
        Debug.Log($"  - PassiveTreeTooltip: {(passiveTooltip != null ? "✅ Found" : "❌ Missing")}");
        
        // Check EventSystem
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        Debug.Log($"  - EventSystem: {(eventSystem != null ? "✅ Found" : "❌ Missing")}");
        
        // Check Canvas
        var canvas = FindFirstObjectByType<Canvas>();
        Debug.Log($"  - Canvas: {(canvas != null ? "✅ Found" : "❌ Missing")}");
        
        if (canvas != null)
        {
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log($"    - GraphicRaycaster: {(raycaster != null ? "✅ Found" : "❌ Missing")}");
        }
    }

    /// <summary>
    /// Check event system setup
    /// </summary>
    private void CheckEventSystem()
    {
        Debug.Log("🎯 [PassiveTreeDebugger] Checking event system...");
        
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("❌ [PassiveTreeDebugger] No EventSystem found! Tooltips won't work without this.");
            return;
        }
        
        Debug.Log($"✅ [PassiveTreeDebugger] EventSystem found: {eventSystem.name}");
        
        // Check for GraphicRaycaster
        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"  - Found {canvases.Length} Canvas components");
        
        foreach (var canvas in canvases)
        {
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log($"    - Canvas '{canvas.name}': GraphicRaycaster {(raycaster != null ? "✅" : "❌")}");
        }
    }

    /// <summary>
    /// Test a specific cell
    /// </summary>
    private void TestSpecificCell(Vector2Int position)
    {
        Debug.Log($"🧪 [PassiveTreeDebugger] Testing cell at {position}...");
        
        var cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        var targetCell = cells.FirstOrDefault(c => c.GridPosition == position);
        
        if (targetCell == null)
        {
            Debug.LogWarning($"⚠️ [PassiveTreeDebugger] No cell found at position {position}");
            return;
        }
        
        Debug.Log($"✅ [PassiveTreeDebugger] Found cell at {position}:");
        Debug.Log($"  - Node Name: '{targetCell.NodeName}'");
        Debug.Log($"  - Description: '{targetCell.NodeDescription}'");
        Debug.Log($"  - Node Type: {targetCell.NodeType}");
        
        // Test tooltip data
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (dataManager != null)
        {
            var nodeData = dataManager.GetNodeData(position);
            if (nodeData != null)
            {
                Debug.Log($"  - JSON Data Found:");
                Debug.Log($"    - Name: '{nodeData.name}'");
                Debug.Log($"    - Description: '{nodeData.description}'");
                Debug.Log($"    - Type: {nodeData.type}");
                if (nodeData.stats != null)
                {
                    Debug.Log($"    - Stats:");
                    // Log some key stats if they have values
                    if (nodeData.stats.strength != 0) Debug.Log($"      - Strength: {nodeData.stats.strength}");
                    if (nodeData.stats.dexterity != 0) Debug.Log($"      - Dexterity: {nodeData.stats.dexterity}");
                    if (nodeData.stats.intelligence != 0) Debug.Log($"      - Intelligence: {nodeData.stats.intelligence}");
                    if (nodeData.stats.maxHealthIncrease != 0) Debug.Log($"      - Max Health: +{nodeData.stats.maxHealthIncrease}");
                    if (nodeData.stats.armorIncrease != 0) Debug.Log($"      - Armor: +{nodeData.stats.armorIncrease}");
                }
            }
            else
            {
                Debug.LogWarning($"  - ❌ No JSON data found for position {position}");
            }
        }
        
        if (simulateHover)
        {
            Debug.Log($"🖱️ [PassiveTreeDebugger] Simulating hover on cell {position}...");
            // Note: We can't actually simulate pointer events, but we can call the tooltip directly
            var jsonTooltip = FindFirstObjectByType<JsonPassiveTreeTooltip>();
            if (jsonTooltip != null)
            {
                jsonTooltip.ShowTooltip(targetCell);
            }
        }
    }

    /// <summary>
    /// Test all cells in the grid
    /// </summary>
    private void TestAllCells()
    {
        Debug.Log("🧪 [PassiveTreeDebugger] Testing all cells...");
        
        var cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        
        Debug.Log($"Found {cells.Length} cells, checking JSON data for each...");
        
        int cellsWithData = 0;
        int cellsWithoutData = 0;
        
        foreach (var cell in cells)
        {
            var nodeData = dataManager?.GetNodeData(cell.GridPosition);
            if (nodeData != null)
            {
                cellsWithData++;
                if (enableVerboseLogging)
                {
                    Debug.Log($"  ✅ Cell {cell.GridPosition}: '{nodeData.name}' - {nodeData.type}");
                }
            }
            else
            {
                cellsWithoutData++;
                if (enableVerboseLogging)
                {
                    Debug.Log($"  ❌ Cell {cell.GridPosition}: No JSON data");
                }
            }
        }
        
        Debug.Log($"📊 [PassiveTreeDebugger] Cell Data Summary:");
        Debug.Log($"  - Cells with JSON data: {cellsWithData}");
        Debug.Log($"  - Cells without JSON data: {cellsWithoutData}");
        Debug.Log($"  - Total cells: {cells.Length}");
    }

    /// <summary>
    /// Force enable debug mode on all components
    /// </summary>
    [ContextMenu("Enable Debug Mode Everywhere")]
    public void EnableDebugModeEverywhere()
    {
        Debug.Log("🔧 [PassiveTreeDebugger] Enabling debug mode on all components...");
        
        // Enable debug on JsonBoardDataManager
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (dataManager != null)
        {
            // Use reflection to set debugMode if it exists
            var debugField = dataManager.GetType().GetField("debugMode", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (debugField != null)
            {
                debugField.SetValue(dataManager, true);
                Debug.Log("✅ [PassiveTreeDebugger] Enabled debug mode on JsonBoardDataManager");
            }
        }
        
        Debug.Log("🔧 [PassiveTreeDebugger] Debug mode setup complete");
    }

    /// <summary>
    /// Test tooltip system directly
    /// </summary>
    [ContextMenu("Test Tooltip System")]
    public void TestTooltipSystem()
    {
        Debug.Log("🧪 [PassiveTreeDebugger] Testing tooltip system...");
        
        var cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        if (cells.Length == 0)
        {
            Debug.LogError("❌ [PassiveTreeDebugger] No cells found to test!");
            return;
        }
        
        var testCell = cells[0];
        Debug.Log($"Testing tooltip with cell at {testCell.GridPosition}");
        
        var jsonTooltip = FindFirstObjectByType<JsonPassiveTreeTooltip>();
        if (jsonTooltip != null)
        {
            jsonTooltip.ShowTooltip(testCell);
            Debug.Log("✅ [PassiveTreeDebugger] Tooltip test completed");
        }
        else
        {
            Debug.LogError("❌ [PassiveTreeDebugger] No JsonPassiveTreeTooltip found!");
        }
    }
}
