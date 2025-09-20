using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace PassiveTree
{
    /// <summary>
    /// Simple, minimal passive tree manager
    /// Handles the core functionality without complex dependencies
    /// </summary>
    public class PassiveTreeManager : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private int boardSize = 7; // 7x7 grid
        [SerializeField] private float cellSpacing = 1f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        [Header("UI Integration")]
        [SerializeField] private PassiveTreeUI uiManager;
        [SerializeField] private bool requireUI = false; // Set to true if UI is required
        
        [Header("Tooltip System")]
        [SerializeField] private bool autoSetupTooltip = true;
        [SerializeField] private PassiveTreeStaticTooltip staticTooltip;
        
        [Header("Extension Board System")]
        [SerializeField] private BoardPositioningManager boardPositioningManager;
        [SerializeField] private ExtensionBoardGenerator boardGenerator;
        [SerializeField] private bool enableExtensionBoards = true;
        
        // Simple state tracking
        private Dictionary<Vector2Int, CellController> cells = new Dictionary<Vector2Int, CellController>();
        private Vector2Int selectedCell = new Vector2Int(-1, -1);
        
        void Start()
        {
            InitializeTooltipSystem();
            InitializeBoard();
            InitializeExtensionBoardSystem();
        }
        
        /// <summary>
        /// Initialize the tooltip system
        /// </summary>
        private void InitializeTooltipSystem()
        {
            if (showDebugInfo)
                PassiveTreeLogger.LogInfo("Initializing tooltip system...", "manager");
            
            if (autoSetupTooltip)
            {
                // Find or create static tooltip
                if (staticTooltip == null)
                {
                    staticTooltip = FindFirstObjectByType<PassiveTreeStaticTooltip>();
                }
                
                if (staticTooltip == null)
                {
                    // Try to find setup helper and create tooltip
                    var setupHelper = FindFirstObjectByType<PassiveTreeStaticTooltipSetup>();
                    if (setupHelper != null)
                    {
                        setupHelper.SetupStaticTooltip();
                        staticTooltip = setupHelper.GetTooltipComponent();
                    }
                }
                
                if (staticTooltip != null)
                {
                    if (showDebugInfo)
                        PassiveTreeLogger.LogInfo("Static tooltip system initialized", "manager");
                }
                else
                {
                    PassiveTreeLogger.LogWarning("Could not initialize static tooltip system", "manager");
                }
            }
        }
        
        /// <summary>
        /// Initialize the passive tree board
        /// </summary>
        private void InitializeBoard()
        {
            if (showDebugInfo)
                PassiveTreeLogger.LogInfo("Initializing board...", "manager");
            
            // Find all cell controllers in the scene
            CellController[] foundCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            
            foreach (CellController cell in foundCells)
            {
                if (cell.GridPosition.x >= 0 && cell.GridPosition.x < boardSize &&
                    cell.GridPosition.y >= 0 && cell.GridPosition.y < boardSize)
                {
                    cells[cell.GridPosition] = cell;
                    cell.Initialize(this);
                }
            }
            
            // Initialize node states
            InitializeNodeStates();
            
            if (showDebugInfo)
                PassiveTreeLogger.LogInfo($"Found {cells.Count} cells", "manager");
        }
        
        /// <summary>
        /// Initialize node states (locked/unlocked, available, etc.)
        /// </summary>
        private void InitializeNodeStates()
        {
            foreach (var kvp in cells)
            {
                CellController cell = kvp.Value;
                
                // Start node is always unlocked, available, and automatically purchased
                if (cell.NodeType == NodeType.Start)
                {
                    cell.SetUnlocked(true);
                    cell.SetAvailable(true);
                    cell.SetPurchased(true); // Auto-allocate the start node
                    
                    if (showDebugInfo)
                        PassiveTreeLogger.LogInfo($"Start node auto-allocated at {cell.GridPosition}", "manager");
                }
                else
                {
                    // All other nodes start locked
                    cell.SetUnlocked(false);
                    cell.SetAvailable(true);
                    cell.SetPurchased(false);
                }
            }
            
            // After initializing all nodes, unlock adjacent nodes for the start node
            UnlockAdjacentNodesForStartNode();
            
            // Auto-load extension point sprites
            AutoLoadExtensionPointSprites();
            
            if (showDebugInfo)
                PassiveTreeLogger.LogInfo("Node states initialized", "manager");
        }
        
        /// <summary>
        /// Unlock adjacent nodes for the start node after auto-allocation
        /// </summary>
        private void UnlockAdjacentNodesForStartNode()
        {
            // Find the start node
            CellController startNode = null;
            foreach (var kvp in cells)
            {
                if (kvp.Value.NodeType == NodeType.Start)
                {
                    startNode = kvp.Value;
                    break;
                }
            }
            
            if (startNode != null)
            {
                UnlockAdjacentNodes(startNode.GridPosition);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Unlocked adjacent nodes for start node at {startNode.GridPosition}");
            }
        }
        
        /// <summary>
        /// Auto-load extension point sprites for all cells
        /// </summary>
        private void AutoLoadExtensionPointSprites()
        {
            if (showDebugInfo)
                PassiveTreeLogger.LogInfo("Auto-loading extension point sprites...", "manager");
            
            int extensionPointsFound = 0;
            int extensionPointsLoaded = 0;
            
            foreach (var kvp in cells)
            {
                CellController cell = kvp.Value;
                
                // Check if this cell is an extension point
                if (cell.IsExtensionPoint || cell.NodeType == NodeType.Extension)
                {
                    extensionPointsFound++;
                    
                    // Force assign extension point sprite
                    cell.SetAsExtensionPoint(true);
                    extensionPointsLoaded++;
                    
                    if (showDebugInfo)
                        PassiveTreeLogger.LogInfo($"Auto-loaded extension point sprite for {cell.GridPosition}", "manager");
                }
            }
            
            if (showDebugInfo)
                PassiveTreeLogger.LogInfo($"Auto-loaded {extensionPointsLoaded}/{extensionPointsFound} extension point sprites", "manager");
        }
        
        /// <summary>
        /// Find a cell in extension boards by grid position
        /// </summary>
        private CellController FindCellInExtensionBoards(Vector2Int gridPosition)
        {
            Debug.Log($"[PassiveTreeManager] Searching for cell at {gridPosition} in extension boards...");
            
            // Find all extension board controllers in the scene
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            Debug.Log($"[PassiveTreeManager] Found {extensionBoards.Length} extension boards in scene");
            
            foreach (ExtensionBoardController boardController in extensionBoards)
            {
                Debug.Log($"[PassiveTreeManager] Checking extension board: {boardController.GetBoardName()}");
                if (boardController.HasCellAt(gridPosition))
                {
                    Debug.Log($"[PassiveTreeManager] ✅ Found cell at {gridPosition} in extension board: {boardController.GetBoardName()}");
                    return boardController.GetCellAt(gridPosition);
                }
                else
                {
                    Debug.Log($"[PassiveTreeManager] ❌ No cell at {gridPosition} in extension board: {boardController.GetBoardName()}");
                }
            }
            
            Debug.Log($"[PassiveTreeManager] ❌ No extension board cell found at {gridPosition}");
            return null;
        }
        
        /// <summary>
        /// Handle cell click (purchase/selection)
        /// </summary>
        public void OnCellClicked(Vector2Int gridPosition)
        {
            Debug.Log($"[PassiveTreeManager] Cell clicked: {gridPosition}");
            
            CellController cell = null;
            
            // First check if it's an extension board cell (priority over core board)
            cell = FindCellInExtensionBoards(gridPosition);
            if (cell != null)
            {
                Debug.Log($"[PassiveTreeManager] Found cell in extension board at {gridPosition}");
            }
            else if (cells.ContainsKey(gridPosition))
            {
                // Fall back to core board cell if no extension board cell found
                cell = cells[gridPosition];
                Debug.Log($"[PassiveTreeManager] Found cell in core board at {gridPosition}");
            }
            else
            {
                Debug.LogError($"[PassiveTreeManager] Cell not found at position: {gridPosition}");
                Debug.LogError($"[PassiveTreeManager] Available core cells: {string.Join(", ", cells.Keys)}");
                return;
            }
            
            // Notify UI of cell selection
            NotifyUICellSelected(gridPosition);
            
            Debug.Log($"[PassiveTreeManager] Processing click for {cell.NodeType} node at {gridPosition}");
            
            // Handle different node types
            switch (cell.NodeType)
            {
                case NodeType.Start:
                    HandleStartNodeClick(cell);
                    break;
                case NodeType.Travel:
                case NodeType.Small:
                    HandleTravelNodeClick(cell);
                    break;
                case NodeType.Notable:
                    HandleNotableNodeClick(cell);
                    break;
                case NodeType.Extension:
                    Debug.Log($"[PassiveTreeManager] Calling HandleExtensionNodeClick for {gridPosition}");
                    HandleExtensionNodeClick(cell);
                    break;
                case NodeType.Keystone:
                    HandleKeystoneNodeClick(cell);
                    break;
            }
        }
        
        /// <summary>
        /// Handle start node click
        /// </summary>
        private void HandleStartNodeClick(CellController cell)
        {
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] HandleStartNodeClick called for {cell.GridPosition}, currently purchased: {cell.IsPurchased}");
            
            // Start node is always available and can be toggled
            if (cell.IsPurchased)
            {
                cell.SetPurchased(false);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Start node unpurchased: {cell.GridPosition}");
            }
            else
            {
                cell.SetPurchased(true);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Start node purchased: {cell.GridPosition}");
            }
        }
        
        /// <summary>
        /// Handle travel/small node click
        /// </summary>
        private void HandleTravelNodeClick(CellController cell)
        {
            if (CanPurchaseNode(cell))
            {
                cell.SetPurchased(true);
                UnlockAdjacentNodes(cell.GridPosition);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Travel node purchased: {cell.GridPosition}");
            }
        }
        
        /// <summary>
        /// Handle notable node click
        /// </summary>
        private void HandleNotableNodeClick(CellController cell)
        {
            if (CanPurchaseNode(cell))
            {
                cell.SetPurchased(true);
                UnlockAdjacentNodes(cell.GridPosition);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Notable node purchased: {cell.GridPosition}");
            }
        }
        
        /// <summary>
        /// Handle extension node click
        /// </summary>
        private void HandleExtensionNodeClick(CellController cell)
        {
            Debug.Log($"[PassiveTreeManager] HandleExtensionNodeClick called for {cell.GridPosition}");
            Debug.Log($"[PassiveTreeManager] enableExtensionBoards: {enableExtensionBoards}");
            Debug.Log($"[PassiveTreeManager] boardPositioningManager: {(boardPositioningManager != null ? "Found" : "Null")}");
            
            // Extension nodes now follow the same validation rules as other nodes
            // They must have an adjacent purchased node to be clickable
            if (CanPurchaseNode(cell) && enableExtensionBoards && boardPositioningManager != null)
            {
                Debug.Log($"[PassiveTreeManager] Calling HandleExtensionBoardSpawn for {cell.GridPosition}");
                HandleExtensionBoardSpawn(cell);
            }
            else
            {
                Debug.LogWarning($"[PassiveTreeManager] Extension board spawning disabled or boardPositioningManager is null");
            }
        }
        
        /// <summary>
        /// Handle keystone node click
        /// </summary>
        private void HandleKeystoneNodeClick(CellController cell)
        {
            if (CanPurchaseNode(cell))
            {
                cell.SetPurchased(true);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Keystone node purchased: {cell.GridPosition}");
            }
        }
        
        /// <summary>
        /// Check if a node can be purchased
        /// </summary>
        private bool CanPurchaseNode(CellController cell)
        {
            Debug.Log($"[PassiveTreeManager] CanPurchaseNode check for {cell.GridPosition}:");
            Debug.Log($"  - IsPurchased: {cell.IsPurchased}");
            Debug.Log($"  - IsUnlocked: {cell.IsUnlocked}");
            Debug.Log($"  - IsAvailable: {cell.IsAvailable}");
            
            // Can't purchase if already purchased
            if (cell.IsPurchased) 
            {
                Debug.Log($"  - ❌ Cannot purchase: already purchased");
                return false;
            }
            
            // Can't purchase if not unlocked
            if (!cell.IsUnlocked) 
            {
                Debug.Log($"  - ❌ Cannot purchase: not unlocked");
                return false;
            }
            
            // Can't purchase if not available
            if (!cell.IsAvailable) 
            {
                Debug.Log($"  - ❌ Cannot purchase: not available");
                return false;
            }
            
            // Check adjacency validation - node must have at least one purchased adjacent node
            // (except for start nodes which are always available)
            if (cell.NodeType != NodeType.Start)
            {
                if (!HasAdjacentPurchasedNode(cell.GridPosition))
                {
                    Debug.Log($"  - ❌ Cannot purchase: no adjacent purchased nodes");
                    return false;
                }
                Debug.Log($"  - ✅ Has adjacent purchased node");
            }
            
            // TODO: Add cost validation, prerequisite checks, etc.
            
            Debug.Log($"  - ✅ Can purchase node");
            return true;
        }
        
        /// <summary>
        /// Check if a node has at least one adjacent purchased node (for pathing validation)
        /// </summary>
        private bool HasAdjacentPurchasedNode(Vector2Int centerPosition)
        {
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] HasAdjacentPurchasedNode check for position: {centerPosition}");
            
            // Define orthographic directions (up, down, left, right)
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                centerPosition + Vector2Int.up,    // North
                centerPosition + Vector2Int.down,  // South
                centerPosition + Vector2Int.left,  // West
                centerPosition + Vector2Int.right  // East
            };
            
            // Check each adjacent position for a purchased node
            foreach (Vector2Int pos in adjacentPositions)
            {
                if (cells.ContainsKey(pos))
                {
                    CellController adjacentCell = cells[pos];
                    if (adjacentCell.IsPurchased)
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ✅ Found adjacent purchased node at {pos}");
                        return true;
                    }
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] ❌ No adjacent purchased nodes found for {centerPosition}");
            return false;
        }
        
        /// <summary>
        /// Unlock adjacent nodes when a node is purchased
        /// </summary>
        private void UnlockAdjacentNodes(Vector2Int centerPosition)
        {
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] UnlockAdjacentNodes called for position: {centerPosition}");
            
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                centerPosition + Vector2Int.up,    // North
                centerPosition + Vector2Int.down,  // South
                centerPosition + Vector2Int.left,  // West
                centerPosition + Vector2Int.right  // East
            };
            
            int unlockedCount = 0;
            
            foreach (Vector2Int pos in adjacentPositions)
            {
                if (cells.ContainsKey(pos))
                {
                    CellController adjacentCell = cells[pos];
                    
                    if (showDebugInfo)
                        Debug.Log($"[PassiveTreeManager] Checking adjacent node at {pos}: Unlocked={adjacentCell.IsUnlocked}, Purchased={adjacentCell.IsPurchased}");
                    
                    if (!adjacentCell.IsUnlocked && !adjacentCell.IsPurchased)
                    {
                        adjacentCell.SetUnlocked(true);
                        unlockedCount++;
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ✅ Unlocked adjacent node: {pos} (Type: {adjacentCell.NodeType})");
                    }
                    else
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ⏭️ Skipped adjacent node: {pos} (already unlocked or purchased)");
                    }
                }
                else
                {
                    if (showDebugInfo)
                        Debug.Log($"[PassiveTreeManager] ⚠️ Adjacent position {pos} not found in cells dictionary");
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] UnlockAdjacentNodes complete: {unlockedCount} nodes unlocked from {centerPosition}");
        }
        
        /// <summary>
        /// Get cell at specific grid position
        /// </summary>
        public CellController GetCell(Vector2Int gridPosition)
        {
            cells.TryGetValue(gridPosition, out CellController cell);
            return cell;
        }
        
        /// <summary>
        /// Get all cells
        /// </summary>
        public Dictionary<Vector2Int, CellController> GetAllCells()
        {
            return new Dictionary<Vector2Int, CellController>(cells);
        }
        
        /// <summary>
        /// Get currently selected cell
        /// </summary>
        public Vector2Int GetSelectedCell()
        {
            return selectedCell;
        }
        
        /// <summary>
        /// Debug method to log all node states
        /// </summary>
        [ContextMenu("Debug All Node States")]
        public void DebugAllNodeStates()
        {
            Debug.Log("=== PASSIVE TREE NODE STATES ===");
            foreach (var kvp in cells)
            {
                CellController cell = kvp.Value;
                Debug.Log($"Node {cell.GridPosition}: Type={cell.NodeType}, Available={cell.IsAvailable}, Unlocked={cell.IsUnlocked}, Purchased={cell.IsPurchased}");
            }
            Debug.Log("=== END NODE STATES ===");
        }
        
        /// <summary>
        /// Debug method to manually unlock adjacent nodes for start node
        /// </summary>
        [ContextMenu("Force Unlock Start Adjacents")]
        public void ForceUnlockStartAdjacents()
        {
            UnlockAdjacentNodesForStartNode();
        }
        
        /// <summary>
        /// Public method to reinitialize the passive tree system
        /// Used by WorldSpaceSetup to restore proper initialization after canvas changes
        /// </summary>
        public void ReinitializePassiveTreeSystem()
        {
            Debug.Log("[PassiveTreeManager] Reinitializing passive tree system...");
            Debug.Log($"[PassiveTreeManager] Current cells count: {cells.Count}");
            
            // Rebuild the cells dictionary to ensure all cells are registered
            RebuildCellsDictionary();
            
            // Check start node before reinitialization
            CellController startNodeBefore = null;
            foreach (var kvp in cells)
            {
                if (kvp.Value.NodeType == NodeType.Start)
                {
                    startNodeBefore = kvp.Value;
                    Debug.Log($"[PassiveTreeManager] Start node before reinit: {startNodeBefore.GridPosition}, purchased: {startNodeBefore.IsPurchased}");
                    break;
                }
            }
            
            // Reinitialize node states (start point purchase, orthogonal allocation, etc.)
            InitializeNodeStates();
            
            // Check start node after reinitialization
            if (startNodeBefore != null)
            {
                Debug.Log($"[PassiveTreeManager] Start node after reinit: {startNodeBefore.GridPosition}, purchased: {startNodeBefore.IsPurchased}");
            }
            
            Debug.Log("[PassiveTreeManager] ✅ Passive tree system reinitialized");
        }
        
        /// <summary>
        /// Rebuild the cells dictionary by finding all CellController components in the scene
        /// </summary>
        public void RebuildCellsDictionary()
        {
            Debug.Log("[PassiveTreeManager] Rebuilding cells dictionary...");
            
            // Clear existing dictionary
            cells.Clear();
            
            // Find all cell controllers in the scene
            CellController[] foundCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            Debug.Log($"[PassiveTreeManager] Found {foundCells.Length} cell controllers in scene");
            
            foreach (CellController cell in foundCells)
            {
                if (cell != null && cell.GridPosition.x >= 0 && cell.GridPosition.x < boardSize &&
                    cell.GridPosition.y >= 0 && cell.GridPosition.y < boardSize)
                {
                    cells[cell.GridPosition] = cell;
                    Debug.Log($"[PassiveTreeManager] Registered cell at {cell.GridPosition} (Type: {cell.NodeType})");
                }
                else if (cell != null)
                {
                    Debug.LogWarning($"[PassiveTreeManager] Skipped cell at {cell.GridPosition} - outside board bounds (boardSize: {boardSize})");
                }
            }
            
            Debug.Log($"[PassiveTreeManager] Rebuilt cells dictionary with {cells.Count} cells");
        }
        
        /// <summary>
        /// Debug method to check EventSystem and UI setup
        /// </summary>
        [ContextMenu("Debug EventSystem Setup")]
        public void DebugEventSystemSetup()
        {
            Debug.Log("=== EVENT SYSTEM DEBUG ===");
            
            // Check for EventSystem
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
            {
                Debug.Log($"✅ EventSystem found: {eventSystem.name} (enabled: {eventSystem.enabled})");
            }
            else
            {
                Debug.LogError("❌ No EventSystem found in scene!");
            }
            
            // Check for GraphicRaycaster (usually on Canvas)
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"✅ Canvas found: {canvas.name} (renderMode: {canvas.renderMode})");
                Debug.Log($"  - Canvas sortingOrder: {canvas.sortingOrder}");
                Debug.Log($"  - Canvas sortingLayerID: {canvas.sortingLayerID}");
                Debug.Log($"  - Canvas worldCamera: {canvas.worldCamera?.name ?? "None"}");
                Debug.Log($"  - Canvas planeDistance: {canvas.planeDistance}");
                
                var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster != null)
                {
                    Debug.Log($"✅ GraphicRaycaster found on canvas (enabled: {raycaster.enabled})");
                }
                else
                {
                    Debug.LogWarning("⚠️ No GraphicRaycaster found on canvas");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No Canvas found in scene");
            }
            
            // Check for Physics2DRaycaster (for 2D colliders)
            var physics2DRaycaster = FindFirstObjectByType<UnityEngine.EventSystems.Physics2DRaycaster>();
            if (physics2DRaycaster != null)
            {
                Debug.Log($"✅ Physics2DRaycaster found: {physics2DRaycaster.name} (enabled: {physics2DRaycaster.enabled})");
            }
            else
            {
                Debug.LogWarning("⚠️ No Physics2DRaycaster found - 2D colliders may not work for UI events");
            }
            
            // Check camera setup
            var camera = Camera.main;
            if (camera != null)
            {
                Debug.Log($"✅ Main Camera found: {camera.name}");
                Debug.Log($"  - Camera cullingMask: {camera.cullingMask}");
                Debug.Log($"  - Camera layer: {camera.gameObject.layer}");
                Debug.Log($"  - Camera position: {camera.transform.position}");
                Debug.Log($"  - Camera orthographic: {camera.orthographic}");
                Debug.Log($"  - Camera orthographicSize: {camera.orthographicSize}");
                
                var cameraRaycaster = camera.GetComponent<UnityEngine.EventSystems.Physics2DRaycaster>();
                if (cameraRaycaster != null)
                {
                    Debug.Log($"✅ Physics2DRaycaster on camera (enabled: {cameraRaycaster.enabled})");
                }
                else
                {
                    Debug.LogWarning("⚠️ No Physics2DRaycaster on main camera");
                }
            }
            else
            {
                Debug.LogError("❌ No main camera found!");
            }
            
            // Check cell setup
            Debug.Log("=== CELL SETUP DEBUG ===");
            if (cells.Count > 0)
            {
                var firstCell = cells.Values.First();
                Debug.Log($"✅ First cell found: {firstCell.GridPosition}");
                Debug.Log($"  - Cell layer: {firstCell.gameObject.layer}");
                Debug.Log($"  - Cell position: {firstCell.transform.position}");
                Debug.Log($"  - Cell scale: {firstCell.transform.localScale}");
                Debug.Log($"  - Cell active: {firstCell.gameObject.activeInHierarchy}");
                
                var collider = firstCell.GetComponent<Collider2D>();
                if (collider != null)
                {
                    Debug.Log($"  - Collider2D enabled: {collider.enabled}");
                    Debug.Log($"  - Collider2D isTrigger: {collider.isTrigger}");
                    Debug.Log($"  - Collider2D bounds: {collider.bounds}");
                }
                
                var spriteRenderer = firstCell.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Debug.Log($"  - SpriteRenderer enabled: {spriteRenderer.enabled}");
                    Debug.Log($"  - SpriteRenderer sortingOrder: {spriteRenderer.sortingOrder}");
                    Debug.Log($"  - SpriteRenderer sortingLayerID: {spriteRenderer.sortingLayerID}");
                }
            }
            
            Debug.Log("=== END EVENT SYSTEM DEBUG ===");
        }
        
        /// <summary>
        /// Debug method to test mouse position and raycasting
        /// </summary>
        [ContextMenu("Test Mouse Raycasting")]
        public void TestMouseRaycasting()
        {
            Debug.Log("=== MOUSE RAYCASTING TEST ===");
            
            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("❌ No main camera found for raycasting test!");
                return;
            }
            
            // Get mouse position using new Input System
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Debug.Log($"🖱️ Mouse position (screen): {mousePos}");
            
            // Convert to world position with different Z values
            Vector3 worldPosNear = camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camera.nearClipPlane));
            Vector3 worldPosFar = camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camera.farClipPlane));
            Vector3 worldPosZero = camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
            
            Debug.Log($"🌍 Mouse position (world near): {worldPosNear}");
            Debug.Log($"🌍 Mouse position (world far): {worldPosFar}");
            Debug.Log($"🌍 Mouse position (world zero): {worldPosZero}");
            
            // Test Physics2D raycast with different positions
            Vector2 mouseWorldPos2D = new Vector2(worldPosZero.x, worldPosZero.y);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos2D, Vector2.zero, 0f);
            
            if (hit.collider != null)
            {
                Debug.Log($"✅ Physics2D Raycast HIT: {hit.collider.name} at {hit.point}");
                var cellController = hit.collider.GetComponent<CellController>();
                if (cellController != null)
                {
                    Debug.Log($"  - CellController found: {cellController.GridPosition}");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Physics2D Raycast MISSED - no collider found at mouse position");
                
                // Check nearby cells
                Debug.Log("🔍 Checking nearby cells...");
                foreach (var cell in cells.Values)
                {
                    Vector3 cellPos = cell.transform.position;
                    float distance = Vector2.Distance(mouseWorldPos2D, new Vector2(cellPos.x, cellPos.y));
                    if (distance < 2f) // Within 2 units
                    {
                        Debug.Log($"  - Nearby cell: {cell.GridPosition} at {cellPos} (distance: {distance:F2})");
                    }
                }
            }
            
            // Test EventSystem raycast
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
            {
                var pointerEventData = new UnityEngine.EventSystems.PointerEventData(eventSystem)
                {
                    position = mousePos
                };
                
                var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                eventSystem.RaycastAll(pointerEventData, results);
                
                Debug.Log($"🎯 EventSystem Raycast Results: {results.Count} hits");
                foreach (var result in results)
                {
                    Debug.Log($"  - Hit: {result.gameObject.name} (layer: {result.gameObject.layer})");
                }
            }
            
            Debug.Log("=== END MOUSE RAYCASTING TEST ===");
        }
        
        /// <summary>
        /// Debug method to show cell positions and bounds
        /// </summary>
        [ContextMenu("Debug Cell Positions and Bounds")]
        public void DebugCellPositionsAndBounds()
        {
            Debug.Log("=== CELL POSITIONS AND BOUNDS DEBUG ===");
            
            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("❌ No main camera found!");
                return;
            }
            
            Debug.Log($"📷 Camera position: {camera.transform.position}");
            Debug.Log($"📷 Camera orthographic size: {camera.orthographicSize}");
            Debug.Log($"📷 Camera near/far: {camera.nearClipPlane}/{camera.farClipPlane}");
            
            // Get mouse position for reference using new Input System
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
            Debug.Log($"🖱️ Current mouse world position: {worldPos}");
            
            Debug.Log("📋 Cell positions and bounds:");
            foreach (var cell in cells.Values)
            {
                Vector3 cellPos = cell.transform.position;
                var collider = cell.GetComponent<Collider2D>();
                var spriteRenderer = cell.GetComponent<SpriteRenderer>();
                
                Debug.Log($"  Cell {cell.GridPosition}:");
                Debug.Log($"    - Position: {cellPos}");
                Debug.Log($"    - Scale: {cell.transform.localScale}");
                Debug.Log($"    - Active: {cell.gameObject.activeInHierarchy}");
                
                if (collider != null)
                {
                    Debug.Log($"    - Collider bounds: {collider.bounds}");
                    Debug.Log($"    - Collider enabled: {collider.enabled}");
                    Debug.Log($"    - Collider isTrigger: {collider.isTrigger}");
                }
                
                if (spriteRenderer != null)
                {
                    Debug.Log($"    - Sprite bounds: {spriteRenderer.bounds}");
                    Debug.Log($"    - Sprite enabled: {spriteRenderer.enabled}");
                }
                
                // Calculate distance from mouse
                float distance = Vector2.Distance(worldPos, new Vector2(cellPos.x, cellPos.y));
                Debug.Log($"    - Distance from mouse: {distance:F2}");
            }
            
            Debug.Log("=== END CELL POSITIONS DEBUG ===");
        }

        /// <summary>
        /// Notify UI of cell selection
        /// </summary>
        private void NotifyUICellSelected(Vector2Int gridPosition)
        {
            // Find UI manager if not assigned
            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<PassiveTreeUI>();
            }

            // Notify UI
            if (uiManager != null)
            {
                uiManager.OnCellSelected(gridPosition);
            }
            else if (requireUI && showDebugInfo)
            {
                Debug.LogWarning("[PassiveTreeManager] No PassiveTreeUI found to notify of cell selection");
            }
            else if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] Cell {gridPosition} selected (UI notification disabled)");
            }
        }

        /// <summary>
        /// Set the UI manager reference
        /// </summary>
        public void SetUIManager(PassiveTreeUI ui)
        {
            uiManager = ui;
        }

        /// <summary>
        /// Get the current UI manager
        /// </summary>
        public PassiveTreeUI GetUIManager()
        {
            return uiManager;
        }
        
        /// <summary>
        /// Get the static tooltip reference
        /// </summary>
        public PassiveTreeStaticTooltip GetStaticTooltip()
        {
            return staticTooltip;
        }
        
        /// <summary>
        /// Set the static tooltip reference
        /// </summary>
        public void SetStaticTooltip(PassiveTreeStaticTooltip tooltip)
        {
            staticTooltip = tooltip;
        }
        
        #region Context Menu Methods
        
        /// <summary>
        /// Manually initialize tooltip system (for debugging)
        /// </summary>
        [ContextMenu("Initialize Tooltip System")]
        public void ManualInitializeTooltipSystem()
        {
            InitializeTooltipSystem();
        }
        
        /// <summary>
        /// Manually load extension point sprites (for debugging)
        /// </summary>
        [ContextMenu("Load Extension Point Sprites")]
        public void ManualLoadExtensionPointSprites()
        {
            AutoLoadExtensionPointSprites();
        }
        
        /// <summary>
        /// Test tooltip system
        /// </summary>
        [ContextMenu("Test Tooltip System")]
        public void TestTooltipSystem()
        {
            if (staticTooltip != null)
            {
                staticTooltip.TestTooltip();
            }
            else
            {
                Debug.LogWarning("[PassiveTreeManager] No static tooltip found. Initialize tooltip system first.");
            }
        }
        
        #endregion
        
        #region Extension Board System
        
        /// <summary>
        /// Initialize the extension board system
        /// </summary>
        private void InitializeExtensionBoardSystem()
        {
            if (!enableExtensionBoards) return;
            
            // Find or create board positioning manager
            if (boardPositioningManager == null)
            {
                boardPositioningManager = FindFirstObjectByType<BoardPositioningManager>();
                
                if (boardPositioningManager == null)
                {
                    GameObject managerObj = new GameObject("BoardPositioningManager");
                    boardPositioningManager = managerObj.AddComponent<BoardPositioningManager>();
                    managerObj.transform.SetParent(transform);
                }
            }
            
            // Find or create board generator
            if (boardGenerator == null)
            {
                boardGenerator = FindFirstObjectByType<ExtensionBoardGenerator>();
                
                if (boardGenerator == null)
                {
                    GameObject generatorObj = new GameObject("ExtensionBoardGenerator");
                    boardGenerator = generatorObj.AddComponent<ExtensionBoardGenerator>();
                    generatorObj.transform.SetParent(transform);
                }
            }
            
            if (showDebugInfo)
                Debug.Log("[PassiveTreeManager] Extension board system initialized");
        }
        
        /// <summary>
        /// Handle extension board spawning when an extension node is purchased
        /// </summary>
        private void HandleExtensionBoardSpawn(CellController extensionCell)
        {
            if (boardPositioningManager == null || boardGenerator == null)
            {
                Debug.LogWarning("[PassiveTreeManager] Extension board system not properly initialized");
                return;
            }
            
            // First, try to find the extension point that corresponds to this cell
            ExtensionPoint targetPoint = null;
            
            // Get ALL extension points (not just available ones) to find the one at this position
            var allExtensionPoints = boardPositioningManager.GetAllExtensionPoints();
            
            foreach (var point in allExtensionPoints)
            {
                if (point.position == extensionCell.GridPosition)
                {
                    targetPoint = point;
                    break;
                }
            }
            
            if (targetPoint == null)
            {
                Debug.Log($"[PassiveTreeManager] No extension point found for cell at {extensionCell.GridPosition} - this cell is not an extension point");
                return;
            }
            
            // Check if this extension point can connect (is available)
            if (!targetPoint.CanConnect())
            {
                Debug.Log($"[PassiveTreeManager] Extension point at {extensionCell.GridPosition} is already allocated and cannot accept new connections");
                return;
            }
            
            // Use the new board selection system
            boardPositioningManager.HandleExtensionPointClick(targetPoint);
        }
        
        /// <summary>
        /// Get the board positioning manager
        /// </summary>
        public BoardPositioningManager GetBoardPositioningManager()
        {
            return boardPositioningManager;
        }
        
        /// <summary>
        /// Get the board generator
        /// </summary>
        public ExtensionBoardGenerator GetBoardGenerator()
        {
            return boardGenerator;
        }
        
        /// <summary>
        /// Enable or disable extension boards
        /// </summary>
        public void SetExtensionBoardsEnabled(bool enabled)
        {
            enableExtensionBoards = enabled;
            
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] Extension boards {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Spawn a test extension board
        /// </summary>
        [ContextMenu("Spawn Test Extension Board")]
        public void SpawnTestExtensionBoard()
        {
            if (boardPositioningManager != null)
            {
                boardPositioningManager.SpawnTestExtensionBoards();
            }
            else
            {
                Debug.LogWarning("[PassiveTreeManager] Board positioning manager not available");
            }
        }
        
        #endregion
    }
}
