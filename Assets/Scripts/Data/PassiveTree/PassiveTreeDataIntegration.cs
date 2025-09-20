using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Simple integration script that connects Board data to the passive tree system
    /// Add this to your scene to automatically set up data integration
    /// </summary>
    public class PassiveTreeDataIntegration : MonoBehaviour
    {
        [Header("Integration Settings")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private string boardId = "core_board";
        [SerializeField] private bool debugMode = true;

        [Header("Components")]
        [SerializeField] private BoardDataManager dataManager;
        [SerializeField] private PassiveTreeManager treeManager;

        void Start()
        {
            if (autoSetupOnStart)
            {
                SetupDataIntegration();
            }
        }

        /// <summary>
        /// Set up the complete data integration system
        /// </summary>
        [ContextMenu("Setup Data Integration")]
        public void SetupDataIntegration()
        {
            Debug.Log("[PassiveTreeDataIntegration] Setting up data integration...");

            // Find or create BoardDataManager
            if (dataManager == null)
            {
                dataManager = FindFirstObjectByType<BoardDataManager>();
                if (dataManager == null)
                {
                    GameObject managerGO = new GameObject("BoardDataManager");
                    dataManager = managerGO.AddComponent<BoardDataManager>();
                    Debug.Log("[PassiveTreeDataIntegration] Created BoardDataManager");
                }
            }

            // Find PassiveTreeManager
            if (treeManager == null)
            {
                treeManager = FindFirstObjectByType<PassiveTreeManager>();
                if (treeManager == null)
                {
                    Debug.LogWarning("[PassiveTreeDataIntegration] No PassiveTreeManager found in scene!");
                    return;
                }
            }

            // Set up the data manager
            SetupBoardDataManager();

            // Connect events
            ConnectEvents();

            // Load initial data
            LoadInitialData();

            Debug.Log("[PassiveTreeDataIntegration] Data integration setup complete!");
        }

        /// <summary>
        /// Set up the BoardDataManager with proper settings
        /// </summary>
        private void SetupBoardDataManager()
        {
            if (dataManager == null) return;

            // Set board ID using reflection
            var boardIdField = typeof(BoardDataManager).GetField("boardId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (boardIdField != null)
            {
                boardIdField.SetValue(dataManager, boardId);
            }

            // Set debug mode
            var debugField = typeof(BoardDataManager).GetField("debugMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (debugField != null)
            {
                debugField.SetValue(dataManager, debugMode);
            }

            // Set auto-load
            var autoLoadField = typeof(BoardDataManager).GetField("autoLoadOnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (autoLoadField != null)
            {
                autoLoadField.SetValue(dataManager, true);
            }

            Debug.Log($"[PassiveTreeDataIntegration] Configured BoardDataManager with board ID: {boardId}");
        }

        /// <summary>
        /// Connect events between components
        /// </summary>
        private void ConnectEvents()
        {
            if (dataManager == null || treeManager == null) return;

            // Connect data manager events
            dataManager.OnNodeDataAssigned += OnNodeDataAssigned;
            dataManager.OnBoardDataLoaded += OnBoardDataLoaded;

            Debug.Log("[PassiveTreeDataIntegration] Connected events between components");
        }

        /// <summary>
        /// Load initial data
        /// </summary>
        private void LoadInitialData()
        {
            if (dataManager == null) return;

            // Load the board data
            dataManager.LoadBoardData();

            Debug.Log("[PassiveTreeDataIntegration] Loaded initial board data");
        }

        /// <summary>
        /// Handle node data assignment events
        /// </summary>
        private void OnNodeDataAssigned(Vector2Int position, PassiveNodeData nodeData)
        {
            if (debugMode)
            {
                Debug.Log($"[PassiveTreeDataIntegration] Node data assigned to {position}: {nodeData.NodeName}");
            }

            // You can add additional logic here, such as:
            // - Updating UI elements
            // - Triggering animations
            // - Updating game state
        }

        /// <summary>
        /// Handle board data loaded events
        /// </summary>
        private void OnBoardDataLoaded()
        {
            Debug.Log("[PassiveTreeDataIntegration] Board data loading complete!");

            // You can add additional logic here, such as:
            // - Initializing UI
            // - Setting up initial state
            // - Triggering start animations
        }

        /// <summary>
        /// Get the BoardDataManager instance
        /// </summary>
        public BoardDataManager GetDataManager()
        {
            return dataManager;
        }

        /// <summary>
        /// Get the PassiveTreeManager instance
        /// </summary>
        public PassiveTreeManager GetTreeManager()
        {
            return treeManager;
        }

        /// <summary>
        /// Reload board data
        /// </summary>
        [ContextMenu("Reload Board Data")]
        public void ReloadBoardData()
        {
            if (dataManager != null)
            {
                dataManager.LoadBoardData();
                Debug.Log("[PassiveTreeDataIntegration] Reloaded board data");
            }
        }

        /// <summary>
        /// Debug the current integration state
        /// </summary>
        [ContextMenu("Debug Integration State")]
        public void DebugIntegrationState()
        {
            Debug.Log("=== PASSIVE TREE DATA INTEGRATION STATE ===");
            Debug.Log($"Board ID: {boardId}");
            Debug.Log($"Data Manager: {(dataManager != null ? dataManager.name : "NULL")}");
            Debug.Log($"Tree Manager: {(treeManager != null ? treeManager.name : "NULL")}");
            Debug.Log($"Debug Mode: {debugMode}");

            if (dataManager != null)
            {
                Debug.Log($"Data Loaded: {dataManager.IsDataLoaded}");
                var allData = dataManager.GetAllNodeData();
                Debug.Log($"Node Data Count: {allData.Count}");
            }

            Debug.Log("=== END INTEGRATION STATE ===");
        }

        void OnDestroy()
        {
            // Clean up events
            if (dataManager != null)
            {
                dataManager.OnNodeDataAssigned -= OnNodeDataAssigned;
                dataManager.OnBoardDataLoaded -= OnBoardDataLoaded;
            }
        }
    }
}
