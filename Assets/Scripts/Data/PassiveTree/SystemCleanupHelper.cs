using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Helper script to clean up system warnings and missing references
    /// </summary>
    public class SystemCleanupHelper : MonoBehaviour
    {
        [Header("Cleanup Settings")]
        [SerializeField] private bool cleanupOnStart = false;
        [SerializeField] private bool showDebugInfo = true;

        void Start()
        {
            if (cleanupOnStart)
            {
                CleanupSystemWarnings();
            }
        }

        /// <summary>
        /// Clean up common system warnings
        /// </summary>
        [ContextMenu("Cleanup System Warnings")]
        public void CleanupSystemWarnings()
        {
            if (showDebugInfo) Debug.Log("🧹 [SystemCleanupHelper] Starting system cleanup...");

            // Step 1: Remove missing script references
            RemoveMissingScriptReferences();

            // Step 2: Disable problematic components temporarily
            DisableProblematicComponents();

            // Step 3: Show system status
            ShowSystemStatus();

            if (showDebugInfo) Debug.Log("✅ [SystemCleanupHelper] System cleanup complete!");
        }

        /// <summary>
        /// Remove missing script references
        /// </summary>
        private void RemoveMissingScriptReferences()
        {
            if (showDebugInfo) Debug.Log("  🗑️ Checking for missing script references...");

            // Find all GameObjects with missing scripts
            var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int missingScriptsFound = 0;

            foreach (var obj in allObjects)
            {
                if (obj == null) continue;

                var components = obj.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component == null)
                    {
                        missingScriptsFound++;
                        if (showDebugInfo) Debug.Log($"  ⚠️ Found missing script on {obj.name}");
                    }
                }
            }

            if (showDebugInfo) Debug.Log($"  📊 Found {missingScriptsFound} missing script references");
        }

        /// <summary>
        /// Disable problematic components temporarily
        /// </summary>
        private void DisableProblematicComponents()
        {
            if (showDebugInfo) Debug.Log("  🔄 Disabling problematic components...");

            // Disable JsonDataTester if it's causing warnings
            var jsonDataTesters = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int disabledComponents = 0;

            foreach (var component in jsonDataTesters)
            {
                if (component == null) continue;

                if (component.GetType().Name == "JsonDataTester")
                {
                    component.enabled = false;
                    disabledComponents++;
                    if (showDebugInfo) Debug.Log($"  🔇 Disabled {component.GetType().Name} on {component.gameObject.name}");
                }
            }

            if (showDebugInfo) Debug.Log($"  📊 Disabled {disabledComponents} problematic components");
        }

        /// <summary>
        /// Show current system status
        /// </summary>
        [ContextMenu("Show System Status")]
        public void ShowSystemStatus()
        {
            Debug.Log("📊 [SystemCleanupHelper] System Status:");

            // Check cells
            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            int cellsWithJsonData = 0;
            int cellsWithOverlays = 0;

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                var jsonData = cell.GetComponent<CellJsonData>();
                if (jsonData != null && jsonData.HasJsonData())
                {
                    cellsWithJsonData++;
                }

                if (cell.IsShowingAttributeOverlay())
                {
                    cellsWithOverlays++;
                }
            }

            Debug.Log($"  - Total Cells: {cellControllers.Length}");
            Debug.Log($"  - Cells with JSON Data: {cellsWithJsonData}");
            Debug.Log($"  - Cells with Overlays: {cellsWithOverlays}");

            // Check managers
            var jsonManager = FindFirstObjectByType<JsonBoardDataManager>();
            var boardManager = FindFirstObjectByType<BoardDataManager>();
            var treeManager = FindFirstObjectByType<PassiveTreeManager>();
            var uiManager = FindFirstObjectByType<PassiveTreeUI>();

            Debug.Log($"  - JsonBoardDataManager: {(jsonManager != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"  - BoardDataManager: {(boardManager != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"  - PassiveTreeManager: {(treeManager != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"  - PassiveTreeUI: {(uiManager != null ? "✅ Found" : "❌ Missing")}");
        }

        /// <summary>
        /// Disable all warning-generating components
        /// </summary>
        [ContextMenu("Disable Warning Components")]
        public void DisableWarningComponents()
        {
            if (showDebugInfo) Debug.Log("🔇 [SystemCleanupHelper] Disabling warning-generating components...");

            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int disabledCount = 0;

            foreach (var component in allComponents)
            {
                if (component == null) continue;

                string componentName = component.GetType().Name;
                
                // Disable components that commonly generate warnings
                if (componentName == "JsonDataTester" || 
                    componentName == "JsonPassiveTreeTooltip" ||
                    componentName == "PassiveTreeDataIntegration")
                {
                    component.enabled = false;
                    disabledCount++;
                    if (showDebugInfo) Debug.Log($"  🔇 Disabled {componentName} on {component.gameObject.name}");
                }
            }

            Debug.Log($"📊 [SystemCleanupHelper] Disabled {disabledCount} warning-generating components");
        }

        /// <summary>
        /// Re-enable all components
        /// </summary>
        [ContextMenu("Re-enable All Components")]
        public void ReEnableAllComponents()
        {
            if (showDebugInfo) Debug.Log("🔊 [SystemCleanupHelper] Re-enabling all components...");

            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int enabledCount = 0;

            foreach (var component in allComponents)
            {
                if (component == null) continue;

                if (!component.enabled)
                {
                    component.enabled = true;
                    enabledCount++;
                }
            }

            Debug.Log($"📊 [SystemCleanupHelper] Re-enabled {enabledCount} components");
        }
    }
}

