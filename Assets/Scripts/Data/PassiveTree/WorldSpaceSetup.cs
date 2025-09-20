using UnityEngine;
using System.Collections;

namespace PassiveTree
{
    /// <summary>
    /// Setup script to configure World Space passive tree system
    /// Removes Canvas dependency and sets up proper camera positioning
    /// </summary>
    public class WorldSpaceSetup : MonoBehaviour
    {
        [Header("Setup Options")]
        [SerializeField] private bool removeCanvasOnStart = true;
        [SerializeField] private bool resetCameraPosition = true;
        [SerializeField] private bool resetCellPositions = true;
        
        [Header("Target Positions")]
        [SerializeField] private Vector3 targetCameraPosition = new Vector3(0, 0, -10);
        [SerializeField] private Vector3 targetCellContainerPosition = new Vector3(0, 0, 0);
        [SerializeField] private float targetCameraSize = 5f;
        
        void Start()
        {
            // Use coroutine to ensure proper initialization order
            StartCoroutine(SetupWorldSpaceSystemDelayed());
        }
        
        /// <summary>
        /// Coroutine to setup world space system with proper timing
        /// </summary>
        private IEnumerator SetupWorldSpaceSystemDelayed()
        {
            // Wait one frame to ensure PassiveTreeManager.Start() has completed
            yield return null;
            
            // Wait a bit more to ensure all initialization is complete
            yield return new WaitForSeconds(0.1f);
            
            Debug.Log("[WorldSpaceSetup] Starting delayed world space setup...");
            SetupWorldSpaceSystem();
        }
        
        /// <summary>
        /// Configure the system for World Space operation
        /// </summary>
        [ContextMenu("Setup World Space System")]
        public void SetupWorldSpaceSystem()
        {
            Debug.Log("[WorldSpaceSetup] Setting up World Space passive tree system...");
            
            // Remove Canvas if requested
            if (removeCanvasOnStart)
            {
                RemoveCanvas();
            }
            
            // Reset camera position
            if (resetCameraPosition)
            {
                ResetCameraPosition();
            }
            
            // Reset cell positions
            if (resetCellPositions)
            {
                ResetCellPositions();
            }
            
            // Add WorldSpaceInputHandler if not present
            AddWorldSpaceInputHandler();
            
            // Reinitialize core board cells after canvas removal
            ReinitializeCoreBoardCells();
            
            Debug.Log("[WorldSpaceSetup] World Space setup complete!");
        }
        
        /// <summary>
        /// Remove Canvas components that cause coordinate issues
        /// Preserves UI canvases that are needed for the game
        /// </summary>
        private void RemoveCanvas()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                // Preserve UI canvases that are needed for the game
                if (ShouldPreserveCanvas(canvas))
                {
                    Debug.Log($"[WorldSpaceSetup] Preserving UI Canvas: {canvas.name}");
                    continue;
                }
                
                Debug.Log($"[WorldSpaceSetup] Removing Canvas: {canvas.name}");
                DestroyImmediate(canvas.gameObject);
            }
        }
        
        /// <summary>
        /// Check if a canvas should be preserved (not destroyed)
        /// </summary>
        private bool ShouldPreserveCanvas(Canvas canvas)
        {
            // Preserve canvases with specific UI components
            if (canvas.GetComponent<BoardSelectionUI>() != null)
            {
                return true; // Preserve board selection UI
            }
            
            if (canvas.GetComponent<PassiveTreeStaticTooltip>() != null)
            {
                // Configure tooltip canvas for screen space overlay
                ConfigureTooltipCanvas(canvas);
                return true; // Preserve tooltip UI
            }
            
            // Preserve canvases with "tooltip" in the name
            if (canvas.name.ToLower().Contains("tooltip"))
            {
                // Configure tooltip canvas for screen space overlay
                ConfigureTooltipCanvas(canvas);
                return true;
            }
            
            // Preserve canvases with "selection" in the name
            if (canvas.name.ToLower().Contains("selection"))
            {
                return true;
            }
            
            // Preserve canvases with "boardselection" in the name
            if (canvas.name.ToLower().Contains("boardselection"))
            {
                return true;
            }
            
            // Preserve canvases with "persistent" in the name
            if (canvas.name.ToLower().Contains("persistent"))
            {
                return true;
            }
            
            // Don't preserve main UI canvas - it should be removed for world space
            // The core board cells will be reinitialized by the PassiveTreeManager
            return false; // Allow destruction of other canvases
        }
        
        /// <summary>
        /// Configure a canvas for proper tooltip display (screen space overlay)
        /// </summary>
        private void ConfigureTooltipCanvas(Canvas canvas)
        {
            if (canvas == null) return;
            
            Debug.Log($"[WorldSpaceSetup] Configuring tooltip canvas: {canvas.name}");
            
            // Set to screen space overlay for fixed position
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.sortingOrder = 10; // High sort order to appear on top
            
            // Ensure it's not affected by world space transformations
            canvas.worldCamera = null;
            
            Debug.Log($"[WorldSpaceSetup] Tooltip canvas configured for screen space overlay");
        }
        
        /// <summary>
        /// Reset camera to proper position for World Space
        /// </summary>
        private void ResetCameraPosition()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Debug.Log($"[WorldSpaceSetup] Resetting camera position from {mainCamera.transform.position} to {targetCameraPosition}");
                mainCamera.transform.position = targetCameraPosition;
                mainCamera.orthographicSize = targetCameraSize;
            }
        }
        
        /// <summary>
        /// Reset cell container position to origin
        /// </summary>
        private void ResetCellPositions()
        {
            // Find cell container
            GameObject cellContainer = GameObject.Find("CellContainer");
            if (cellContainer != null)
            {
                Debug.Log($"[WorldSpaceSetup] Resetting cell container position from {cellContainer.transform.position} to {targetCellContainerPosition}");
                cellContainer.transform.position = targetCellContainerPosition;
            }
            else
            {
                Debug.LogWarning("[WorldSpaceSetup] No CellContainer found - cells may be positioned incorrectly");
            }
        }
        
        /// <summary>
        /// Add WorldSpaceInputHandler to the scene
        /// </summary>
        private void AddWorldSpaceInputHandler()
        {
            // Check if already exists
            WorldSpaceInputHandler existingHandler = FindFirstObjectByType<WorldSpaceInputHandler>();
            if (existingHandler != null)
            {
                Debug.Log("[WorldSpaceSetup] WorldSpaceInputHandler already exists");
                return;
            }
            
            // Create new GameObject with WorldSpaceInputHandler
            GameObject inputHandlerGO = new GameObject("WorldSpaceInputHandler");
            inputHandlerGO.AddComponent<WorldSpaceInputHandler>();
            
            Debug.Log("[WorldSpaceSetup] Added WorldSpaceInputHandler to scene");
        }
        
        /// <summary>
        /// Reinitialize core board cells with PassiveTreeManager reference
        /// </summary>
        private void ReinitializeCoreBoardCells()
        {
            // Find the PassiveTreeManager
            PassiveTreeManager treeManager = FindFirstObjectByType<PassiveTreeManager>();
            if (treeManager == null)
            {
                Debug.LogWarning("[WorldSpaceSetup] No PassiveTreeManager found - cannot reinitialize cells");
                return;
            }
            
            Debug.Log($"[WorldSpaceSetup] Found PassiveTreeManager: {treeManager.name}");
            
            // Find all cell controllers in the scene
            CellController[] cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            Debug.Log($"[WorldSpaceSetup] Found {cellControllers.Length} cell controllers to reinitialize");
            
            // Check if we have a start node before reinitialization
            CellController startNode = null;
            foreach (CellController cell in cellControllers)
            {
                if (cell != null && cell.NodeType == NodeType.Start)
                {
                    startNode = cell;
                    Debug.Log($"[WorldSpaceSetup] Found start node at {cell.GridPosition}, currently purchased: {cell.IsPurchased}");
                    break;
                }
            }
            
            int reinitializedCount = 0;
            foreach (CellController cellController in cellControllers)
            {
                if (cellController != null)
                {
                    // Reinitialize the cell with the tree manager reference
                    cellController.Initialize(treeManager);
                    reinitializedCount++;
                }
            }
            
            Debug.Log($"[WorldSpaceSetup] Reinitialized {reinitializedCount} core board cells");
            
            // Trigger full passive tree reinitialization to restore start point purchase and orthogonal allocation
            ReinitializePassiveTreeSystem(treeManager);
            
            // Check start node after reinitialization
            if (startNode != null)
            {
                Debug.Log($"[WorldSpaceSetup] After reinitialization - start node at {startNode.GridPosition}, purchased: {startNode.IsPurchased}");
            }
        }
        
        /// <summary>
        /// Reinitialize the passive tree system to restore start point purchase and orthogonal allocation
        /// </summary>
        private void ReinitializePassiveTreeSystem(PassiveTreeManager treeManager)
        {
            Debug.Log("[WorldSpaceSetup] Reinitializing passive tree system...");
            
            // Call the public reinitialization method
            treeManager.ReinitializePassiveTreeSystem();
            
            Debug.Log("[WorldSpaceSetup] ✅ Passive tree system reinitialized - start point should be purchased and adjacent nodes unlocked");
        }
        
        /// <summary>
        /// Debug method to show current system state
        /// </summary>
        [ContextMenu("Debug System State")]
        public void DebugSystemState()
        {
            Debug.Log("=== WORLD SPACE SYSTEM STATE ===");
            
            // Camera info
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Debug.Log($"📷 Camera: {mainCamera.name} at {mainCamera.transform.position}, size: {mainCamera.orthographicSize}");
            }
            else
            {
                Debug.LogError("❌ No main camera found!");
            }
            
            // Canvas info
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Debug.Log($"🖼️ Canvas count: {canvases.Length}");
            foreach (Canvas canvas in canvases)
            {
                Debug.Log($"  - {canvas.name} (renderMode: {canvas.renderMode})");
            }
            
            // Cell container info
            GameObject cellContainer = GameObject.Find("CellContainer");
            if (cellContainer != null)
            {
                Debug.Log($"📦 CellContainer: {cellContainer.name} at {cellContainer.transform.position}");
            }
            else
            {
                Debug.LogWarning("⚠️ No CellContainer found");
            }
            
            // Input handler info
            WorldSpaceInputHandler inputHandler = FindFirstObjectByType<WorldSpaceInputHandler>();
            if (inputHandler != null)
            {
                Debug.Log($"🖱️ WorldSpaceInputHandler: {inputHandler.name} (enabled: {inputHandler.enabled})");
            }
            else
            {
                Debug.LogWarning("⚠️ No WorldSpaceInputHandler found");
            }
            
            Debug.Log("=== END SYSTEM STATE ===");
        }
    }
}
