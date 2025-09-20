using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Simple setup script for JSON-based passive tree system
    /// Much easier than the complex ScriptableObject approach
    /// </summary>
    public class JsonPassiveTreeSetup : MonoBehaviour
    {
        [Header("JSON Data")]
        [SerializeField] private TextAsset boardDataJson;
        
        [Header("Components")]
        [SerializeField] private JsonBoardDataManager dataManager;
        [SerializeField] private JsonPassiveTreeTooltip tooltipManager;
        [SerializeField] private PassiveTreeManager passiveTreeManager;

        /// <summary>
        /// Complete setup for JSON-based passive tree system
        /// </summary>
        [ContextMenu("Setup JSON Passive Tree System")]
        public void SetupJsonPassiveTreeSystem()
        {
            Debug.Log("[JsonPassiveTreeSetup] Setting up JSON-based passive tree system...");

            // 1. Setup data manager
            SetupDataManager();

            // 2. Setup tooltip system
            SetupTooltipSystem();

            // 3. Setup passive tree manager
            SetupPassiveTreeManager();

            // 4. Connect components
            ConnectComponents();

            Debug.Log("[JsonPassiveTreeSetup] JSON-based passive tree system setup complete!");
        }

        /// <summary>
        /// Setup JSON data manager
        /// </summary>
        private void SetupDataManager()
        {
            if (dataManager == null)
            {
                dataManager = FindFirstObjectByType<JsonBoardDataManager>();
                if (dataManager == null)
                {
                    GameObject managerGO = new GameObject("JsonBoardDataManager");
                    dataManager = managerGO.AddComponent<JsonBoardDataManager>();
                    Debug.Log("[JsonPassiveTreeSetup] Created JsonBoardDataManager");
                }
            }

            // Set JSON data source
            if (boardDataJson != null)
            {
                dataManager.SetJsonDataSource(boardDataJson);
                Debug.Log("[JsonPassiveTreeSetup] Set JSON data source");
            }
            else
            {
                Debug.LogWarning("[JsonPassiveTreeSetup] No JSON data file assigned!");
            }
        }

        /// <summary>
        /// Setup tooltip system
        /// </summary>
        private void SetupTooltipSystem()
        {
            if (tooltipManager == null)
            {
                tooltipManager = FindFirstObjectByType<JsonPassiveTreeTooltip>();
                if (tooltipManager == null)
                {
                    GameObject tooltipGO = new GameObject("JsonPassiveTreeTooltip");
                    tooltipManager = tooltipGO.AddComponent<JsonPassiveTreeTooltip>();
                    Debug.Log("[JsonPassiveTreeSetup] Created JsonPassiveTreeTooltip");
                }
            }
        }

        /// <summary>
        /// Setup passive tree manager
        /// </summary>
        private void SetupPassiveTreeManager()
        {
            if (passiveTreeManager == null)
            {
                passiveTreeManager = FindFirstObjectByType<PassiveTreeManager>();
                if (passiveTreeManager == null)
                {
                    Debug.LogWarning("[JsonPassiveTreeSetup] No PassiveTreeManager found in scene!");
                }
            }
        }

        /// <summary>
        /// Connect all components
        /// </summary>
        private void ConnectComponents()
        {
            // Connect tooltip to data manager
            if (tooltipManager != null && dataManager != null)
            {
                tooltipManager.SetDataManager(dataManager);
                Debug.Log("[JsonPassiveTreeSetup] Connected tooltip to data manager");
            }

            // Load data
            if (dataManager != null)
            {
                dataManager.LoadBoardData();
                Debug.Log("[JsonPassiveTreeSetup] Loaded board data from JSON");
            }
        }

        /// <summary>
        /// Set JSON data source
        /// </summary>
        public void SetJsonDataSource(TextAsset jsonFile)
        {
            boardDataJson = jsonFile;
            if (dataManager != null)
            {
                dataManager.SetJsonDataSource(jsonFile);
            }
        }

        /// <summary>
        /// Get the data manager
        /// </summary>
        public JsonBoardDataManager GetDataManager()
        {
            return dataManager;
        }

        /// <summary>
        /// Get the tooltip manager
        /// </summary>
        public JsonPassiveTreeTooltip GetTooltipManager()
        {
            return tooltipManager;
        }
    }
}
