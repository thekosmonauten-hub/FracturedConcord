using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Script to fix cell positioning issues and ensure cells are properly activated
    /// </summary>
    public class CellPositioningFixer : MonoBehaviour
    {
        [Header("Fix Settings")]
        [SerializeField] private bool fixOnStart = true;
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool disableOverlaysDuringFix = true;

        void Start()
        {
            if (fixOnStart)
            {
                FixCellPositioning();
            }
        }

        /// <summary>
        /// Fix cell positioning and activation issues
        /// </summary>
        [ContextMenu("Fix Cell Positioning")]
        public void FixCellPositioning()
        {
            if (showDebugInfo) Debug.Log("🔧 [CellPositioningFixer] Starting cell positioning fix...");

            // Step 1: Find all cells
            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            if (showDebugInfo) Debug.Log($"  📊 Found {cellControllers.Length} cells");

            // Step 2: Disable overlays temporarily if requested
            if (disableOverlaysDuringFix)
            {
                DisableOverlaysTemporarily(cellControllers);
            }

            // Step 3: Ensure all cells are active and properly positioned
            FixCellActivationAndPositioning(cellControllers);

            // Step 4: Verify the fix
            VerifyCellFix(cellControllers);

            if (showDebugInfo) Debug.Log("✅ [CellPositioningFixer] Cell positioning fix complete!");
        }

        /// <summary>
        /// Disable overlays temporarily to avoid interference
        /// </summary>
        private void DisableOverlaysTemporarily(CellController[] cellControllers)
        {
            if (showDebugInfo) Debug.Log("  🔄 Temporarily disabling overlays...");

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;
                cell.SetAttributeOverlaysEnabled(false);
            }
        }

        /// <summary>
        /// Fix cell activation and positioning
        /// </summary>
        private void FixCellActivationAndPositioning(CellController[] cellControllers)
        {
            if (showDebugInfo) Debug.Log("  🔄 Fixing cell activation and positioning...");

            int fixedCount = 0;
            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                // Ensure cell is active
                if (!cell.gameObject.activeInHierarchy)
                {
                    cell.gameObject.SetActive(true);
                    fixedCount++;
                }

                // Ensure cell component is enabled
                if (!cell.enabled)
                {
                    cell.enabled = true;
                }

                // Check if cell has proper components
                var spriteRenderer = cell.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    if (showDebugInfo) Debug.LogWarning($"  ⚠️ Cell {cell.GridPosition} missing SpriteRenderer!");
                }
                else if (!spriteRenderer.enabled)
                {
                    spriteRenderer.enabled = true;
                    fixedCount++;
                }

                // Force update visual state
                cell.UpdateVisualState();
            }

            if (showDebugInfo) Debug.Log($"  ✅ Fixed {fixedCount} cells");
        }

        /// <summary>
        /// Verify that the fix worked
        /// </summary>
        private void VerifyCellFix(CellController[] cellControllers)
        {
            if (showDebugInfo) Debug.Log("  🔍 Verifying fix...");

            int activeCells = 0;
            int cellsWithSpriteRenderer = 0;
            int cellsWithSprites = 0;

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                if (cell.gameObject.activeInHierarchy)
                {
                    activeCells++;
                }

                var spriteRenderer = cell.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    cellsWithSpriteRenderer++;
                    if (spriteRenderer.sprite != null)
                    {
                        cellsWithSprites++;
                    }
                }
            }

            if (showDebugInfo)
            {
                Debug.Log($"  📊 Verification Results:");
                Debug.Log($"    - Active Cells: {activeCells}/{cellControllers.Length}");
                Debug.Log($"    - Cells with SpriteRenderer: {cellsWithSpriteRenderer}/{cellControllers.Length}");
                Debug.Log($"    - Cells with Sprites: {cellsWithSprites}/{cellControllers.Length}");
            }
        }

        /// <summary>
        /// Show current cell status
        /// </summary>
        [ContextMenu("Show Current Cell Status")]
        public void ShowCurrentCellStatus()
        {
            Debug.Log("📋 [CellPositioningFixer] Current Cell Status:");

            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            int activeCells = 0;
            int cellsWithSprites = 0;
            int cellsWithOverlays = 0;

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                bool isActive = cell.gameObject.activeInHierarchy;
                if (isActive) activeCells++;

                var spriteRenderer = cell.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    cellsWithSprites++;
                }

                if (cell.IsShowingAttributeOverlay())
                {
                    cellsWithOverlays++;
                }
            }

            Debug.Log($"  - Total Cells: {cellControllers.Length}");
            Debug.Log($"  - Active Cells: {activeCells}");
            Debug.Log($"  - Cells with Sprites: {cellsWithSprites}");
            Debug.Log($"  - Cells with Overlays: {cellsWithOverlays}");
        }

        /// <summary>
        /// Force refresh all cells
        /// </summary>
        [ContextMenu("Force Refresh All Cells")]
        public void ForceRefreshAllCells()
        {
            if (showDebugInfo) Debug.Log("🔄 [CellPositioningFixer] Force refreshing all cells...");

            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            int refreshedCount = 0;

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                cell.UpdateVisualState();
                cell.UpdateSpriteForJsonData();
                refreshedCount++;
            }

            Debug.Log($"📊 [CellPositioningFixer] Refreshed {refreshedCount} cells");
        }

        /// <summary>
        /// Remove all overlay objects to clean up
        /// </summary>
        [ContextMenu("Remove All Overlay Objects")]
        public void RemoveAllOverlayObjects()
        {
            if (showDebugInfo) Debug.Log("🗑️ [CellPositioningFixer] Removing all overlay objects...");

            var cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            int removedCount = 0;

            foreach (var cell in cellControllers)
            {
                if (cell == null) continue;

                // Find and remove any overlay objects
                Transform overlayObject = cell.transform.Find("AttributeOverlay");
                if (overlayObject != null)
                {
                    DestroyImmediate(overlayObject.gameObject);
                    removedCount++;
                }
            }

            Debug.Log($"📊 [CellPositioningFixer] Removed {removedCount} overlay objects");
        }
    }
}
