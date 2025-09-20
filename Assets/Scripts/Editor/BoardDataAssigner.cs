using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PassiveTree
{
    /// <summary>
    /// Editor tool for assigning Board data to cells
    /// </summary>
    public class BoardDataAssigner : EditorWindow
    {
        [MenuItem("Tools/Passive Tree/Board Data Assigner")]
        public static void ShowWindow()
        {
            GetWindow<BoardDataAssigner>("Board Data Assigner");
        }

        [Header("Board Data Assignment")]
        [SerializeField] private string boardId = "core_board";
        [SerializeField] private bool autoDetectCells = true;
        [SerializeField] private bool createDefaultData = true;
        [SerializeField] private bool debugMode = true;

        [Header("Data Sources")]
        [SerializeField] private PassiveNodeData[] nodeDataAssets = new PassiveNodeData[0];

        private BoardDataManager dataManager;
        private CellController[] allCells;
        private Vector2 scrollPosition;

        void OnGUI()
        {
            GUILayout.Label("Board Data Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Board ID
            boardId = EditorGUILayout.TextField("Board ID", boardId);
            
            // Auto-detect cells
            autoDetectCells = EditorGUILayout.Toggle("Auto-detect Cells", autoDetectCells);
            
            // Create default data
            createDefaultData = EditorGUILayout.Toggle("Create Default Data", createDefaultData);
            
            // Debug mode
            debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode);

            GUILayout.Space(10);

            // Node Data Assets
            GUILayout.Label("Node Data Assets", EditorStyles.boldLabel);
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty nodeDataProperty = so.FindProperty("nodeDataAssets");
            EditorGUILayout.PropertyField(nodeDataProperty, true);
            so.ApplyModifiedProperties();

            GUILayout.Space(10);

            // Action buttons
            if (GUILayout.Button("Find Board Data Manager"))
            {
                FindBoardDataManager();
            }

            if (GUILayout.Button("Detect All Cells"))
            {
                DetectAllCells();
            }

            if (GUILayout.Button("Assign Board Data to Cells"))
            {
                AssignBoardDataToCells();
            }

            if (GUILayout.Button("Create Default Node Data"))
            {
                CreateDefaultNodeData();
            }

            GUILayout.Space(10);

            // Debug information
            if (allCells != null)
            {
                GUILayout.Label($"Found {allCells.Length} cells", EditorStyles.helpBox);
            }

            if (dataManager != null)
            {
                GUILayout.Label($"Board Data Manager: {dataManager.name}", EditorStyles.helpBox);
            }

            // Cell list
            if (allCells != null && allCells.Length > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("Detected Cells:", EditorStyles.boldLabel);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                
                foreach (CellController cell in allCells)
                {
                    if (cell != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Cell {cell.GridPosition}", GUILayout.Width(100));
                        EditorGUILayout.LabelField($"Type: {cell.NodeType}", GUILayout.Width(120));
                        EditorGUILayout.LabelField($"Unlocked: {cell.IsUnlocked}", GUILayout.Width(80));
                        
                        if (GUILayout.Button("Select", GUILayout.Width(60)))
                        {
                            Selection.activeGameObject = cell.gameObject;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                
                EditorGUILayout.EndScrollView();
            }
        }

        private void FindBoardDataManager()
        {
            dataManager = FindObjectOfType<BoardDataManager>();
            if (dataManager == null)
            {
                Debug.LogWarning("[BoardDataAssigner] No BoardDataManager found in scene. Creating one...");
                
                GameObject managerGO = new GameObject("BoardDataManager");
                dataManager = managerGO.AddComponent<BoardDataManager>();
                
                // Set the board ID
                var boardIdField = typeof(BoardDataManager).GetField("boardId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (boardIdField != null)
                {
                    boardIdField.SetValue(dataManager, boardId);
                }
                
                Debug.Log($"[BoardDataAssigner] Created BoardDataManager with ID: {boardId}");
            }
            else
            {
                Debug.Log($"[BoardDataAssigner] Found existing BoardDataManager: {dataManager.name}");
            }
        }

        private void DetectAllCells()
        {
            allCells = FindObjectsOfType<CellController>();
            Debug.Log($"[BoardDataAssigner] Detected {allCells.Length} CellController components");
            
            if (debugMode)
            {
                foreach (CellController cell in allCells)
                {
                    Debug.Log($"[BoardDataAssigner] Cell at {cell.GridPosition}: {cell.NodeType} (Unlocked: {cell.IsUnlocked})");
                }
            }
        }

        private void AssignBoardDataToCells()
        {
            if (dataManager == null)
            {
                Debug.LogError("[BoardDataAssigner] No BoardDataManager found. Please find or create one first.");
                return;
            }

            if (allCells == null || allCells.Length == 0)
            {
                Debug.LogError("[BoardDataAssigner] No cells detected. Please detect cells first.");
                return;
            }

            Debug.Log("[BoardDataAssigner] Assigning board data to cells...");

            // Set the node data assets on the data manager
            var nodeDataField = typeof(BoardDataManager).GetField("nodeDataAssets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (nodeDataField != null)
            {
                nodeDataField.SetValue(dataManager, nodeDataAssets);
            }

            // Set debug mode
            var debugField = typeof(BoardDataManager).GetField("debugMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (debugField != null)
            {
                debugField.SetValue(dataManager, debugMode);
            }

            // Load the board data
            dataManager.LoadBoardData();

            Debug.Log("[BoardDataAssigner] Board data assignment complete!");
        }

        private void CreateDefaultNodeData()
        {
            if (allCells == null || allCells.Length == 0)
            {
                Debug.LogError("[BoardDataAssigner] No cells detected. Please detect cells first.");
                return;
            }

            Debug.Log("[BoardDataAssigner] Creating default node data assets...");

            List<PassiveNodeData> createdAssets = new List<PassiveNodeData>();

            foreach (CellController cell in allCells)
            {
                if (cell == null) continue;

                // Create a new ScriptableObject asset
                PassiveNodeData nodeData = ScriptableObject.CreateInstance<PassiveNodeData>();
                
                // Set basic properties
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

                // Save as asset
                string assetPath = $"Assets/Resources/PassiveTree/NodeData_{cell.GridPosition.x}_{cell.GridPosition.y}.asset";
                AssetDatabase.CreateAsset(nodeData, assetPath);
                
                createdAssets.Add(nodeData);

                if (debugMode)
                {
                    Debug.Log($"[BoardDataAssigner] Created node data asset: {assetPath}");
                }
            }

            // Update the node data assets array
            nodeDataAssets = createdAssets.ToArray();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[BoardDataAssigner] Created {createdAssets.Count} default node data assets!");
        }

        // Helper methods
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

        private void SetNodeDataField(PassiveNodeData nodeData, string fieldName, object value)
        {
            var field = typeof(PassiveNodeData).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(nodeData, value);
            }
        }
    }
}
