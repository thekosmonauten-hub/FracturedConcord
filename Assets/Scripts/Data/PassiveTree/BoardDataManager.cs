using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Manages Board data and connects it to individual cells
    /// Handles loading, assignment, and synchronization of Board ScriptableObject data
    /// </summary>
    public class BoardDataManager : MonoBehaviour
    {
        [Header("Board Data")]
        [SerializeField] private string boardId = "core_board";
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private bool debugMode = false;

        [Header("Data Sources")]
        [SerializeField] private PassiveNodeData[] nodeDataAssets;
        [SerializeField] private TextAsset boardDataJson;

        // Runtime data
        private Dictionary<Vector2Int, PassiveNodeData> nodeDataMap = new Dictionary<Vector2Int, PassiveNodeData>();
        private Dictionary<Vector2Int, CellController> cellMap = new Dictionary<Vector2Int, CellController>();
        private bool isDataLoaded = false;

        // Events
        public System.Action<Vector2Int, PassiveNodeData> OnNodeDataAssigned;
        public System.Action OnBoardDataLoaded;

        void Start()
        {
            if (autoLoadOnStart)
            {
                LoadBoardData();
            }
        }

        /// <summary>
        /// Load board data from ScriptableObjects and assign to cells
        /// </summary>
        [ContextMenu("Load Board Data")]
        public void LoadBoardData()
        {
            Debug.Log($"[BoardDataManager] Loading board data for board: {boardId}");

            // Clear existing data
            nodeDataMap.Clear();
            cellMap.Clear();

            // Find all CellController components
            CellController[] cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            Debug.Log($"[BoardDataManager] Found {cells.Length} CellController components");

            // Build cell map
            foreach (CellController cell in cells)
            {
                cellMap[cell.GridPosition] = cell;
                if (debugMode)
                {
                    Debug.Log($"[BoardDataManager] Mapped cell at {cell.GridPosition}");
                }
            }

            // Load node data from ScriptableObjects
            LoadNodeDataFromAssets();

            // Assign data to cells
            AssignDataToCells();

            isDataLoaded = true;
            OnBoardDataLoaded?.Invoke();

            Debug.Log($"[BoardDataManager] Board data loading complete. {nodeDataMap.Count} nodes loaded.");
        }

        /// <summary>
        /// Load node data from ScriptableObject assets
        /// </summary>
        private void LoadNodeDataFromAssets()
        {
            if (nodeDataAssets == null || nodeDataAssets.Length == 0)
            {
                Debug.Log("[BoardDataManager] No node data assets assigned. Creating default data.");
                CreateDefaultNodeData();
                return;
            }

            foreach (PassiveNodeData nodeData in nodeDataAssets)
            {
                if (nodeData == null) continue;

                // For now, we'll create a mapping based on node type and position
                // In a full implementation, you'd have position data in the ScriptableObject
                AssignNodeDataByType(nodeData);
            }
        }

        /// <summary>
        /// Create default node data based on cell positions and types
        /// </summary>
        private void CreateDefaultNodeData()
        {
            Debug.Log("[BoardDataManager] Creating default node data based on cell types...");

            foreach (var cellPair in cellMap)
            {
                Vector2Int position = cellPair.Key;
                CellController cell = cellPair.Value;

                // Create default node data based on cell's current type
                PassiveNodeData defaultData = CreateDefaultNodeDataForCell(cell);
                nodeDataMap[position] = defaultData;

                if (debugMode)
                {
                    Debug.Log($"[BoardDataManager] Created default data for {position}: {defaultData.NodeName}");
                }
            }
        }

        /// <summary>
        /// Create default node data for a specific cell
        /// </summary>
        private PassiveNodeData CreateDefaultNodeDataForCell(CellController cell)
        {
            // Create a runtime instance of PassiveNodeData
            PassiveNodeData nodeData = ScriptableObject.CreateInstance<PassiveNodeData>();

            // Set basic properties based on cell's current state
            string nodeName = GetDefaultNodeName(cell.GridPosition, cell.NodeType);
            string description = GetDefaultDescription(cell.NodeType);
            int cost = GetDefaultCost(cell.NodeType);

            // Use reflection to set private fields
            SetNodeDataField(nodeData, "_nodeName", nodeName);
            SetNodeDataField(nodeData, "_description", description);
            SetNodeDataField(nodeData, "_nodeType", cell.NodeType);
            SetNodeDataField(nodeData, "_skillPointsCost", cost);
            SetNodeDataField(nodeData, "_isStartNode", cell.NodeType == NodeType.Start);
            SetNodeDataField(nodeData, "_isUnlocked", cell.IsUnlocked);

            return nodeData;
        }

        /// <summary>
        /// Assign node data by type (for when you have ScriptableObject assets)
        /// </summary>
        private void AssignNodeDataByType(PassiveNodeData nodeData)
        {
            // Find cells that match this node type
            var matchingCells = cellMap.Where(pair => pair.Value.NodeType == nodeData.NodeType).ToList();

            if (matchingCells.Count == 0)
            {
                Debug.LogWarning($"[BoardDataManager] No cells found for node type: {nodeData.NodeType}");
                return;
            }

            // Assign to the first matching cell (you might want more sophisticated logic)
            var firstMatch = matchingCells.First();
            nodeDataMap[firstMatch.Key] = nodeData;

            if (debugMode)
            {
                Debug.Log($"[BoardDataManager] Assigned {nodeData.NodeName} to cell at {firstMatch.Key}");
            }
        }

        /// <summary>
        /// Assign loaded data to all cells
        /// </summary>
        private void AssignDataToCells()
        {
            foreach (var dataPair in nodeDataMap)
            {
                Vector2Int position = dataPair.Key;
                PassiveNodeData nodeData = dataPair.Value;

                if (cellMap.TryGetValue(position, out CellController cell))
                {
                    AssignDataToCell(cell, nodeData);
                    OnNodeDataAssigned?.Invoke(position, nodeData);
                }
                else
                {
                    Debug.LogWarning($"[BoardDataManager] No cell found at position {position} for node data {nodeData.NodeName}");
                }
            }
        }

        /// <summary>
        /// Assign node data to a specific cell
        /// </summary>
        public void AssignDataToCell(CellController cell, PassiveNodeData nodeData)
        {
            if (cell == null || nodeData == null)
            {
                Debug.LogWarning("[BoardDataManager] Cannot assign data: cell or nodeData is null");
                return;
            }

            // Update cell properties based on node data
            cell.SetNodeData(nodeData);

            if (debugMode)
            {
                Debug.Log($"[BoardDataManager] Assigned {nodeData.NodeName} to cell at {cell.GridPosition}");
            }
        }

        /// <summary>
        /// Get node data for a specific position
        /// </summary>
        public PassiveNodeData GetNodeData(Vector2Int position)
        {
            nodeDataMap.TryGetValue(position, out PassiveNodeData data);
            return data;
        }

        /// <summary>
        /// Get all loaded node data
        /// </summary>
        public Dictionary<Vector2Int, PassiveNodeData> GetAllNodeData()
        {
            return new Dictionary<Vector2Int, PassiveNodeData>(nodeDataMap);
        }

        /// <summary>
        /// Check if data is loaded
        /// </summary>
        public bool IsDataLoaded => isDataLoaded;

        // Helper methods for default data creation
        private string GetDefaultNodeName(Vector2Int position, NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.Start:
                    return "Starting Point";
                case NodeType.Travel:
                    return $"Travel Node ({position.x},{position.y})";
                case NodeType.Extension:
                    return $"Extension Point ({position.x},{position.y})";
                case NodeType.Notable:
                    return $"Notable Passive ({position.x},{position.y})";
                case NodeType.Small:
                    return $"Small Node ({position.x},{position.y})";
                case NodeType.Keystone:
                    return $"Keystone ({position.x},{position.y})";
                default:
                    return $"Node ({position.x},{position.y})";
            }
        }

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

        private int GetDefaultCost(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.Start:
                    return 0;
                case NodeType.Travel:
                    return 1;
                case NodeType.Extension:
                    return 0;
                case NodeType.Notable:
                    return 2;
                case NodeType.Small:
                    return 1;
                case NodeType.Keystone:
                    return 1;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Use reflection to set private fields on PassiveNodeData
        /// </summary>
        private void SetNodeDataField(PassiveNodeData nodeData, string fieldName, object value)
        {
            var field = typeof(PassiveNodeData).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(nodeData, value);
            }
            else
            {
                Debug.LogWarning($"[BoardDataManager] Could not find field: {fieldName}");
            }
        }

        [ContextMenu("Debug Board Data")]
        public void DebugBoardData()
        {
            Debug.Log("=== BOARD DATA DEBUG ===");
            Debug.Log($"Board ID: {boardId}");
            Debug.Log($"Data Loaded: {isDataLoaded}");
            Debug.Log($"Node Data Count: {nodeDataMap.Count}");
            Debug.Log($"Cell Count: {cellMap.Count}");

            foreach (var dataPair in nodeDataMap)
            {
                Debug.Log($"Position {dataPair.Key}: {dataPair.Value.NodeName} ({dataPair.Value.NodeType})");
            }

            Debug.Log("=== END BOARD DATA DEBUG ===");
        }
    }
}
