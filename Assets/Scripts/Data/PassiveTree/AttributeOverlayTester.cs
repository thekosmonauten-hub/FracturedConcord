using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Simple test script to verify the attribute overlay system works correctly
    /// </summary>
    public class AttributeOverlayTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool testOnStart = false;
        [SerializeField] private bool showDebugInfo = true;

        void Start()
        {
            if (testOnStart)
            {
                TestAttributeOverlays();
            }
        }

        /// <summary>
        /// Test the attribute overlay system
        /// </summary>
        [ContextMenu("Test Attribute Overlays")]
        public void TestAttributeOverlays()
        {
            if (showDebugInfo) Debug.Log("🧪 [AttributeOverlayTester] Testing attribute overlay system...");

            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            int cellsWithOverlays = 0;
            int cellsWithJsonData = 0;
            int totalCells = cellControllers.Length;

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                // Check if cell has JSON data
                var jsonData = cell.GetComponent<CellJsonData>();
                if (jsonData != null && jsonData.HasJsonData())
                {
                    cellsWithJsonData++;

                    // Check if cell is showing an overlay
                    if (cell.IsShowingAttributeOverlay())
                    {
                        cellsWithOverlays++;
                        if (showDebugInfo)
                        {
                            Debug.Log($"  ✅ Cell {cell.GridPosition}: {cell.GetOverlayInfo()}");
                        }
                    }
                    else
                    {
                        if (showDebugInfo)
                        {
                            Debug.Log($"  ⚠️ Cell {cell.GridPosition}: No overlay (may not have attributes)");
                        }
                    }
                }
            }

            Debug.Log($"📊 [AttributeOverlayTester] Test Results:");
            Debug.Log($"  - Total Cells: {totalCells}");
            Debug.Log($"  - Cells with JSON Data: {cellsWithJsonData}");
            Debug.Log($"  - Cells Showing Overlays: {cellsWithOverlays}");
        }

        /// <summary>
        /// Force refresh all attribute overlays
        /// </summary>
        [ContextMenu("Force Refresh All Overlays")]
        public void ForceRefreshAllOverlays()
        {
            if (showDebugInfo) Debug.Log("🔄 [AttributeOverlayTester] Force refreshing all attribute overlays...");

            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            int refreshedCount = 0;

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                cell.RefreshAttributeOverlay();
                refreshedCount++;
            }

            Debug.Log($"📊 [AttributeOverlayTester] Refreshed {refreshedCount} cells");
        }

        /// <summary>
        /// Show detailed information about all cells
        /// </summary>
        [ContextMenu("Show Detailed Cell Info")]
        public void ShowDetailedCellInfo()
        {
            Debug.Log("📋 [AttributeOverlayTester] Detailed Cell Information:");

            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            
            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                var jsonData = cell.GetComponent<CellJsonData>();
                
                Debug.Log($"Cell {cell.GridPosition}:");
                Debug.Log($"  - Has JsonData: {jsonData != null}");
                Debug.Log($"  - Is Extension Point: {cell.IsExtensionPoint}");
                Debug.Log($"  - Extension Point Info: {cell.GetExtensionPointInfo()}");
                
                if (jsonData != null && jsonData.HasJsonData())
                {
                    var stats = jsonData.NodeStats;
                    Debug.Log($"  - Node Type: {jsonData.NodeType}");
                    Debug.Log($"  - Strength: {stats.strength}");
                    Debug.Log($"  - Dexterity: {stats.dexterity}");
                    Debug.Log($"  - Intelligence: {stats.intelligence}");
                    Debug.Log($"  - Overlay Info: {cell.GetOverlayInfo()}");
                    Debug.Log($"  - Is Showing Overlay: {cell.IsShowingAttributeOverlay()}");
                }
                else
                {
                    Debug.Log($"  - No JSON data available");
                }
                
                Debug.Log("  ---");
            }
        }

        /// <summary>
        /// Enable overlays on all cells
        /// </summary>
        [ContextMenu("Enable Overlays on All Cells")]
        public void EnableOverlaysOnAllCells()
        {
            if (showDebugInfo) Debug.Log("✅ [AttributeOverlayTester] Enabling overlays on all cells...");

            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            int enabledCount = 0;

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                cell.SetAttributeOverlaysEnabled(true);
                enabledCount++;
            }

            Debug.Log($"📊 [AttributeOverlayTester] Enabled overlays on {enabledCount} cells");
        }

        /// <summary>
        /// Disable overlays on all cells
        /// </summary>
        [ContextMenu("Disable Overlays on All Cells")]
        public void DisableOverlaysOnAllCells()
        {
            if (showDebugInfo) Debug.Log("❌ [AttributeOverlayTester] Disabling overlays on all cells...");

            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            int disabledCount = 0;

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                cell.SetAttributeOverlaysEnabled(false);
                disabledCount++;
            }

            Debug.Log($"📊 [AttributeOverlayTester] Disabled overlays on {disabledCount} cells");
        }

        /// <summary>
        /// Test extension points specifically
        /// </summary>
        [ContextMenu("Test Extension Points")]
        public void TestExtensionPoints()
        {
            if (showDebugInfo) Debug.Log("🔗 [AttributeOverlayTester] Testing extension points...");

            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            int extensionPoints = 0;
            int totalCells = cellControllers.Length;

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                if (cell.IsExtensionPoint)
                {
                    extensionPoints++;
                    if (showDebugInfo)
                    {
                        Debug.Log($"  🔗 Extension Point: Cell {cell.GridPosition} - {cell.GetExtensionPointInfo()}");
                    }
                }
            }

            Debug.Log($"📊 [AttributeOverlayTester] Extension Point Results:");
            Debug.Log($"  - Total Cells: {totalCells}");
            Debug.Log($"  - Extension Points: {extensionPoints}");
        }
    }
}
