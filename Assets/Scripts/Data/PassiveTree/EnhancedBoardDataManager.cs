using UnityEngine;
using System.Collections.Generic;
using System.IO;
using PassiveTree;

/// <summary>
/// Enhanced BoardDataManager that works with PassiveTreeBoardData ScriptableObject
/// Manages loading and mapping of cell data to CellController instances
/// </summary>
public class EnhancedBoardDataManager : MonoBehaviour
{
    [Header("Data Sources")]
    [SerializeField] private PassiveTreeBoardData boardData;
    [SerializeField] private string nodeDataFolderPath = "Assets/Scripts/Data/PassiveTree/NodeData";
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool debugMode = false;

    [Header("Fallback Options")]
    [SerializeField] private bool useLegacyNodeData = true;
    [SerializeField] private bool generateMissingData = true;
    [SerializeField] private bool createAssetFiles = false;

    // Runtime data
    private Dictionary<Vector2Int, PassiveNodeData> nodeDataMap;
    private List<PassiveNodeData> allNodeData;
    private bool isDataLoaded = false;

    void Start()
    {
        if (autoLoadOnStart)
        {
            LoadAllNodeData();
        }
    }

    /// <summary>
    /// Load all node data from the board data ScriptableObject
    /// </summary>
    [ContextMenu("Load All Node Data")]
    public void LoadAllNodeData()
    {
        nodeDataMap = new Dictionary<Vector2Int, PassiveNodeData>();
        allNodeData = new List<PassiveNodeData>();

        // Try to load from board data first
        if (boardData != null)
        {
            LoadFromBoardData();
        }
        else if (useLegacyNodeData)
        {
            LoadFromLegacyNodeData();
        }

        if (generateMissingData)
        {
            GenerateMissingNodeData();
        }

        isDataLoaded = true;
        Debug.Log($"[EnhancedBoardDataManager] Loaded {nodeDataMap.Count} node data entries");
    }

    /// <summary>
    /// Load data from PassiveTreeBoardData ScriptableObject
    /// </summary>
    private void LoadFromBoardData()
    {
        if (boardData == null)
        {
            Debug.LogWarning("[EnhancedBoardDataManager] No board data ScriptableObject assigned");
            return;
        }

        Dictionary<Vector2Int, PassiveNodeData> boardDataMap = boardData.GetAllCellData();
        
        foreach (var kvp in boardDataMap)
        {
            nodeDataMap[kvp.Key] = kvp.Value;
            allNodeData.Add(kvp.Value);
            
            if (debugMode)
            {
                Debug.Log($"[EnhancedBoardDataManager] Loaded from board data: {kvp.Key} -> {kvp.Value.NodeName}");
            }
        }

        Debug.Log($"[EnhancedBoardDataManager] Loaded {boardDataMap.Count} entries from board data");
    }

    /// <summary>
    /// Load data from legacy individual ScriptableObject files
    /// </summary>
    private void LoadFromLegacyNodeData()
    {
        if (string.IsNullOrEmpty(nodeDataFolderPath))
        {
            Debug.LogWarning("[EnhancedBoardDataManager] No node data folder path specified");
            return;
        }

        // Load all PassiveNodeData assets from the folder
        PassiveNodeData[] nodeDataAssets = Resources.LoadAll<PassiveNodeData>("NodeData");
        
        foreach (PassiveNodeData nodeData in nodeDataAssets)
        {
            if (nodeData != null)
            {
                // Try to determine grid position from the asset name
                Vector2Int gridPosition = ParseGridPositionFromName(nodeData.name);
                
                if (gridPosition != Vector2Int.zero || nodeData.name.Contains("Cell_0_0"))
                {
                    nodeDataMap[gridPosition] = nodeData;
                    allNodeData.Add(nodeData);
                    
                    if (debugMode)
                    {
                        Debug.Log($"[EnhancedBoardDataManager] Loaded legacy node data: {nodeData.name} -> {gridPosition}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get node data for a specific grid position
    /// </summary>
    public PassiveNodeData GetNodeData(Vector2Int gridPosition)
    {
        if (!isDataLoaded)
        {
            LoadAllNodeData();
        }

        if (nodeDataMap.TryGetValue(gridPosition, out PassiveNodeData data))
        {
            return data;
        }

        if (debugMode)
        {
            Debug.LogWarning($"[EnhancedBoardDataManager] No node data found for position {gridPosition}");
        }

        return null;
    }

    /// <summary>
    /// Get all loaded node data
    /// </summary>
    public Dictionary<Vector2Int, PassiveNodeData> GetAllNodeData()
    {
        if (!isDataLoaded)
        {
            LoadAllNodeData();
        }

        return new Dictionary<Vector2Int, PassiveNodeData>(nodeDataMap);
    }

    /// <summary>
    /// Parse grid position from asset name (e.g., "Cell_3_4" -> (3,4))
    /// </summary>
    private Vector2Int ParseGridPositionFromName(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return Vector2Int.zero;

        // Look for pattern like "Cell_X_Y" or "Node_X_Y"
        string[] parts = assetName.Split('_');
        if (parts.Length >= 3)
        {
            if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
            {
                return new Vector2Int(x, y);
            }
        }

        return Vector2Int.zero;
    }

    /// <summary>
    /// Generate missing node data for all cells
    /// </summary>
    [ContextMenu("Generate Missing Node Data")]
    public void GenerateMissingNodeData()
    {
        // Find all CellController components in the scene
        CellController[] cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        
        foreach (CellController cell in cells)
        {
            Vector2Int gridPosition = cell.GridPosition;
            
            // Check if we already have data for this position
            if (!nodeDataMap.ContainsKey(gridPosition))
            {
                PassiveNodeData nodeData = CreateDefaultNodeData(cell);
                nodeDataMap[gridPosition] = nodeData;
                
                if (createAssetFiles)
                {
                    CreateNodeDataAsset(nodeData, gridPosition);
                }
                
                if (debugMode)
                {
                    Debug.Log($"[EnhancedBoardDataManager] Generated node data for {gridPosition}: {nodeData.NodeName}");
                }
            }
        }
    }

    /// <summary>
    /// Create default node data for a cell
    /// </summary>
    private PassiveNodeData CreateDefaultNodeData(CellController cell)
    {
        PassiveNodeData nodeData = ScriptableObject.CreateInstance<PassiveNodeData>();
        
        // Set basic properties using reflection
        SetNodeDataField(nodeData, "_nodeName", $"Cell ({cell.GridPosition.x},{cell.GridPosition.y})");
        SetNodeDataField(nodeData, "_description", GetDefaultDescription(cell.NodeType));
        SetNodeDataField(nodeData, "_nodeType", cell.NodeType);
        SetNodeDataField(nodeData, "_skillPointsCost", GetDefaultCost(cell.NodeType));
        SetNodeDataField(nodeData, "_isUnlocked", cell.NodeType == NodeType.Start);
        SetNodeDataField(nodeData, "_nodeSprite", null); // Will use auto-assignment
        
        // Set default colors
        SetNodeDataField(nodeData, "_nodeColor", GetDefaultColor(cell.NodeType));
        SetNodeDataField(nodeData, "_highlightColor", Color.yellow);
        SetNodeDataField(nodeData, "_selectedColor", Color.cyan);
        
        // Set empty stats
        SetNodeDataField(nodeData, "_stats", new List<string>());

        return nodeData;
    }

    /// <summary>
    /// Set a field on PassiveNodeData using reflection
    /// </summary>
    private void SetNodeDataField(PassiveNodeData nodeData, string fieldName, object value)
    {
        var field = typeof(PassiveNodeData).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(nodeData, value);
        }
    }

    /// <summary>
    /// Get default description for node type
    /// </summary>
    private string GetDefaultDescription(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return "Your starting point in the passive tree";
            case NodeType.Travel:
                return "A basic travel node";
            case NodeType.Extension:
                return "An extension point for connecting other boards";
            case NodeType.Notable:
                return "A notable passive with significant effects";
            case NodeType.Small:
                return "A small passive node";
            case NodeType.Keystone:
                return "A powerful keystone passive";
            default:
                return "A passive tree node";
        }
    }

    /// <summary>
    /// Get default cost for node type
    /// </summary>
    private int GetDefaultCost(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
            case NodeType.Extension:
                return 0;
            case NodeType.Travel:
            case NodeType.Small:
                return 1;
            case NodeType.Notable:
                return 2;
            case NodeType.Keystone:
                return 1;
            default:
                return 1;
        }
    }

    /// <summary>
    /// Get default color for node type
    /// </summary>
    private Color GetDefaultColor(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return Color.green;
            case NodeType.Travel:
                return Color.white;
            case NodeType.Small:
                return Color.gray;
            case NodeType.Notable:
                return Color.blue;
            case NodeType.Extension:
                return Color.magenta;
            case NodeType.Keystone:
                return Color.red;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Create a ScriptableObject asset file for node data
    /// </summary>
    private void CreateNodeDataAsset(PassiveNodeData nodeData, Vector2Int gridPosition)
    {
        #if UNITY_EDITOR
        string fileName = $"Cell_{gridPosition.x}_{gridPosition.y}.asset";
        string fullPath = Path.Combine(nodeDataFolderPath, fileName);
        
        // Ensure directory exists
        Directory.CreateDirectory(nodeDataFolderPath);
        
        UnityEditor.AssetDatabase.CreateAsset(nodeData, fullPath);
        UnityEditor.AssetDatabase.SaveAssets();
        
        Debug.Log($"[EnhancedBoardDataManager] Created asset: {fullPath}");
        #endif
    }

    /// <summary>
    /// Assign node data to all cells in the scene
    /// </summary>
    [ContextMenu("Assign Data to All Cells")]
    public void AssignDataToAllCells()
    {
        if (!isDataLoaded)
        {
            LoadAllNodeData();
        }

        CellController[] cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        int assignedCount = 0;

        foreach (CellController cell in cells)
        {
            PassiveNodeData nodeData = GetNodeData(cell.GridPosition);
            if (nodeData != null)
            {
                cell.SetNodeData(nodeData);
                assignedCount++;
            }
        }

        Debug.Log($"[EnhancedBoardDataManager] Assigned data to {assignedCount} cells");
    }

    /// <summary>
    /// Set the board data ScriptableObject
    /// </summary>
    public void SetBoardData(PassiveTreeBoardData newBoardData)
    {
        boardData = newBoardData;
        isDataLoaded = false; // Force reload
    }

    /// <summary>
    /// Get the current board data
    /// </summary>
    public PassiveTreeBoardData GetBoardData()
    {
        return boardData;
    }

    /// <summary>
    /// Get the number of loaded node data entries
    /// </summary>
    public int GetNodeDataCount()
    {
        return nodeDataMap?.Count ?? 0;
    }

    /// <summary>
    /// Check if data is loaded for a specific position
    /// </summary>
    public bool HasNodeData(Vector2Int gridPosition)
    {
        return nodeDataMap?.ContainsKey(gridPosition) ?? false;
    }

    /// <summary>
    /// Get board information if available
    /// </summary>
    public BoardInfo GetBoardInfo()
    {
        if (boardData != null)
        {
            return boardData.GetBoardInfo();
        }
        return null;
    }
}
