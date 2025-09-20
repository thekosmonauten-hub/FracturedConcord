using UnityEngine;
using PassiveTree;

/// <summary>
/// Simple test script to verify JSON data loading
/// Add this to any GameObject and use the context menu to test
/// </summary>
public class JsonDataTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private Vector2Int testPosition = new Vector2Int(2, 6);
    [SerializeField] private bool testOnStart = true;

    void Start()
    {
        if (testOnStart)
        {
            TestJsonData();
        }
    }

    /// <summary>
    /// Test JSON data loading and retrieval
    /// </summary>
    [ContextMenu("Test JSON Data")]
    public void TestJsonData()
    {
        Debug.Log("🧪 [JsonDataTester] Starting JSON data test...");

        // 1. Find JsonBoardDataManager
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (dataManager == null)
        {
            Debug.LogError("❌ [JsonDataTester] JsonBoardDataManager not found!");
            return;
        }

        Debug.Log($"✅ [JsonDataTester] JsonBoardDataManager found: {dataManager.name}");

        // 2. Check if JSON file is assigned
        var jsonFile = dataManager.BoardDataJson;
        if (jsonFile == null)
        {
            Debug.LogError("❌ [JsonDataTester] No JSON file assigned to JsonBoardDataManager!");
            Debug.LogError("   Please assign CoreBoardData.json in the inspector.");
            return;
        }

        Debug.Log($"✅ [JsonDataTester] JSON file assigned: {jsonFile.name}");
        Debug.Log($"   File size: {jsonFile.text.Length} characters");

        // 3. Check if data is loaded
        if (!dataManager.IsDataLoaded)
        {
            Debug.LogWarning("⚠️ [JsonDataTester] JSON data is not loaded. Attempting to load...");
            dataManager.LoadBoardData();
        }

        Debug.Log($"✅ [JsonDataTester] Data loaded: {dataManager.IsDataLoaded}");

        // 4. Get all node data
        var allNodeData = dataManager.GetAllNodeData();
        Debug.Log($"📊 [JsonDataTester] Total nodes loaded: {allNodeData.Count}");

        if (allNodeData.Count == 0)
        {
            Debug.LogError("❌ [JsonDataTester] No nodes loaded from JSON!");
            return;
        }

        // 5. Show first few nodes
        Debug.Log("📋 [JsonDataTester] First 5 nodes:");
        int count = 0;
        foreach (var kvp in allNodeData)
        {
            if (count >= 5) break;
            var node = kvp.Value;
            Debug.Log($"  {count + 1}. Position {kvp.Key}: '{node.name}' ({node.type})");
            count++;
        }

        // 6. Test specific position
        Debug.Log($"🎯 [JsonDataTester] Testing position {testPosition}...");
        var testNodeData = dataManager.GetNodeData(testPosition);
        
        if (testNodeData != null)
        {
            Debug.Log($"✅ [JsonDataTester] Found data for position {testPosition}:");
            Debug.Log($"   - Name: '{testNodeData.name}'");
            Debug.Log($"   - Description: '{testNodeData.description}'");
            Debug.Log($"   - Type: {testNodeData.type}");
            Debug.Log($"   - Position: ({testNodeData.position.column}, {testNodeData.position.row})");
        }
        else
        {
            Debug.LogWarning($"⚠️ [JsonDataTester] No data found for position {testPosition}");
            Debug.Log("   Available positions:");
            foreach (var kvp in allNodeData)
            {
                Debug.Log($"     - {kvp.Key}: '{kvp.Value.name}'");
            }
        }

        // 7. Test cell at that position
        var cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        var testCell = System.Array.Find(cells, c => c.GridPosition == testPosition);
        
        if (testCell != null)
        {
            Debug.Log($"✅ [JsonDataTester] Found cell at position {testPosition}:");
            Debug.Log($"   - Cell Name: '{testCell.NodeName}'");
            Debug.Log($"   - Cell Description: '{testCell.NodeDescription}'");
            Debug.Log($"   - Cell Type: {testCell.NodeType}");
        }
        else
        {
            Debug.LogWarning($"⚠️ [JsonDataTester] No cell found at position {testPosition}");
            Debug.Log("   Available cell positions:");
            foreach (var cell in cells)
            {
                Debug.Log($"     - {cell.GridPosition}: '{cell.NodeName}'");
            }
        }

        Debug.Log("🧪 [JsonDataTester] JSON data test complete!");
    }

    /// <summary>
    /// Force reload JSON data
    /// </summary>
    [ContextMenu("Force Reload JSON Data")]
    public void ForceReloadJsonData()
    {
        Debug.Log("🔄 [JsonDataTester] Force reloading JSON data...");
        
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (dataManager != null)
        {
            dataManager.LoadBoardData();
            Debug.Log("✅ [JsonDataTester] JSON data reloaded!");
            
            // Test again after reload
            TestJsonData();
        }
        else
        {
            Debug.LogError("❌ [JsonDataTester] JsonBoardDataManager not found!");
        }
    }

    /// <summary>
    /// Show all loaded positions
    /// </summary>
    [ContextMenu("Show All Loaded Positions")]
    public void ShowAllLoadedPositions()
    {
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (dataManager == null)
        {
            Debug.LogError("❌ [JsonDataTester] JsonBoardDataManager not found!");
            return;
        }

        var allNodeData = dataManager.GetAllNodeData();
        Debug.Log($"📋 [JsonDataTester] All {allNodeData.Count} loaded positions:");
        
        foreach (var kvp in allNodeData)
        {
            var node = kvp.Value;
            Debug.Log($"  - {kvp.Key}: '{node.name}' ({node.type}) - ({node.position.column}, {node.position.row})");
        }
    }
}
