using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using System;

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
        
        [Header("Stats Integration")]
        [SerializeField] private bool enableStatsIntegration = true;
        [SerializeField] private PassiveTreeStatsIntegration statsIntegration;
        
        [Header("Allocation Confirmation")]
        [SerializeField] private bool requireConfirmToAllocate = true;
        
        // Pending selection buffer for confirm/cancel flow
        private HashSet<Vector2Int> pendingAllocations = new HashSet<Vector2Int>();
        
        // Events for stats integration
        public static event Action<Vector2Int, CellController> OnNodeAllocated;
        public static event Action<Vector2Int, CellController> OnNodeDeallocated;
        public static event Action<Vector2Int, CellController> OnNodeClicked;
        
        // UI can subscribe to update pending/points display
        public static event Action OnPendingAllocationsChanged;

        /// <summary>
        /// Get current count of pending allocations
        /// </summary>
        public int GetPendingAllocationsCount()
        {
            return pendingAllocations != null ? pendingAllocations.Count : 0;
        }

        /// <summary>
        /// Confirm and finalize all pending allocations. Returns number of nodes allocated.
        /// </summary>
        public int ConfirmPendingAllocations()
        {
            if (!requireConfirmToAllocate || pendingAllocations == null || pendingAllocations.Count == 0) return 0;

            int allocated = 0;
            // Copy to avoid modification during iteration
            var toAllocate = new List<Vector2Int>(pendingAllocations);
            foreach (var pos in toAllocate)
            {
                CellController cell = null;
                if (cells.ContainsKey(pos))
                {
                    cell = cells[pos];
                }
                else
                {
                    cell = FindCellInExtensionBoards(pos);
                }
                if (cell == null) continue;
                if (cell.IsPurchased) continue;
                if (!cell.IsAdjacent) continue;

                cell.SetSelected(false);
                cell.SetPurchased(true);
                TriggerNodeAllocatedEvent(pos, cell);
                UnlockAdjacentNodes(pos);
                allocated++;
            }

            pendingAllocations.Clear();
            OnPendingAllocationsChanged?.Invoke();
            return allocated;
        }

        /// <summary>
        /// Cancel any pending allocations and clear selection state.
        /// </summary>
        public void CancelPendingAllocations()
        {
            if (pendingAllocations == null || pendingAllocations.Count == 0) return;
            foreach (var pos in pendingAllocations)
            {
                if (cells.ContainsKey(pos))
                {
                    cells[pos].SetSelected(false);
                }
                else
                {
                    var extCell = FindCellInExtensionBoards(pos);
                    if (extCell != null) extCell.SetSelected(false);
                }
            }
            pendingAllocations.Clear();
            OnPendingAllocationsChanged?.Invoke();
        }
        
        // Simple state tracking
        private Dictionary<Vector2Int, CellController> cells = new Dictionary<Vector2Int, CellController>();
        private Vector2Int selectedCell = new Vector2Int(-1, -1);
        
        void Start()
        {
            InitializeTooltipSystem();
            InitializeBoard();
            InitializeExtensionBoardSystem();
            InitializeStatsIntegration();
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
                
                // Start node is always adjacent and automatically purchased
                if (cell.NodeType == NodeType.Start)
                {
                    cell.SetAdjacent(true);
                    cell.SetPurchased(true); // Auto-allocate the start node
                    
                    if (showDebugInfo)
                        PassiveTreeLogger.LogInfo($"Start node auto-allocated at {cell.GridPosition}", "manager");
                }
                else
                {
                    // All other nodes start non-adjacent
                    cell.SetAdjacent(false);
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
            OnCellClicked(gridPosition, null);
        }
        
        /// <summary>
        /// Handle cell click with explicit board selection
        /// </summary>
        public void OnCellClicked(Vector2Int gridPosition, ExtensionBoardController preferredBoard)
        {
            Debug.Log($"[PassiveTreeManager] Cell clicked: {gridPosition}");
            
            CellController cell = null;
            
            // Check if there are multiple boards with cells at this position
            bool hasCoreBoardCell = cells.ContainsKey(gridPosition);
            CellController extensionBoardCell = FindCellInExtensionBoards(gridPosition);
            bool hasExtensionBoardCell = extensionBoardCell != null;
            
            if (hasCoreBoardCell && hasExtensionBoardCell)
            {
                // Multiple boards have cells at this position - need to choose which one
                Debug.Log($"[PassiveTreeManager] ⚠️ Multiple boards have cells at {gridPosition} - need to choose which board to interact with");
                Debug.Log($"[PassiveTreeManager] Core board cell: {cells[gridPosition].NodeType}, Extension board cell: {extensionBoardCell.NodeType}");
                
                // If a preferred board is specified, use it
                if (preferredBoard != null)
                {
                    if (preferredBoard.HasCellAt(gridPosition))
                    {
                        cell = preferredBoard.GetCellAt(gridPosition);
                        Debug.Log($"[PassiveTreeManager] Using preferred extension board cell at {gridPosition}");
                    }
                    else
                    {
                        Debug.LogWarning($"[PassiveTreeManager] Preferred board {preferredBoard.GetBoardName()} doesn't have a cell at {gridPosition}, falling back to core board");
                        cell = cells[gridPosition];
                    }
                }
                else
                {
                    // Smart routing: prioritize core board for non-extension nodes, extension board for extension nodes
                    if (cells[gridPosition].NodeType == NodeType.Extension)
                    {
                        // Core board extension point - handle extension board creation
                        cell = cells[gridPosition];
                        Debug.Log($"[PassiveTreeManager] Using core board extension point at {gridPosition}");
                    }
                    else
                    {
                        // Regular core board node - use core board
                        cell = cells[gridPosition];
                        Debug.Log($"[PassiveTreeManager] Using core board cell at {gridPosition}");
                    }
                }
            }
            else if (hasCoreBoardCell)
            {
                // Only core board has a cell at this position
                cell = cells[gridPosition];
                Debug.Log($"[PassiveTreeManager] Found cell in core board at {gridPosition}");
            }
            else if (hasExtensionBoardCell)
            {
                // Only extension board has a cell at this position
                cell = extensionBoardCell;
                Debug.Log($"[PassiveTreeManager] Found cell in extension board at {gridPosition}");
            }
            else
            {
                Debug.LogError($"[PassiveTreeManager] Cell not found at position: {gridPosition}");
                Debug.LogError($"[PassiveTreeManager] Available core cells: {string.Join(", ", cells.Keys)}");
                return;
            }
            
            // Notify UI of cell selection
            NotifyUICellSelected(gridPosition);
            
            // If confirm flow is enabled, toggle pending selection instead of immediate allocation
            if (requireConfirmToAllocate && cell != null)
            {
                // Already purchased? ignore
                if (cell.IsPurchased)
                {
                    if (showDebugInfo) Debug.Log($"[PassiveTreeManager] Click ignored - already purchased {gridPosition}");
                    return;
                }
                // Must be adjacent to be selectable
                if (!cell.IsAdjacent)
                {
                    if (showDebugInfo) Debug.Log($"[PassiveTreeManager] Click ignored - not adjacent {gridPosition}");
                    return;
                }
                
                if (pendingAllocations.Contains(gridPosition))
                {
                    pendingAllocations.Remove(gridPosition);
                    cell.SetSelected(false);
                }
                else
                {
                    pendingAllocations.Add(gridPosition);
                    cell.SetSelected(true);
                }
                OnPendingAllocationsChanged?.Invoke();
                return;
            }

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
        /// Handle extension board cell click (for cells routed through PassiveTreeManager)
        /// </summary>
        public void OnExtensionBoardCellClicked(CellController_EXT extensionCell)
        {
            Debug.Log($"[PassiveTreeManager] Extension board cell clicked: {extensionCell.GridPosition}");
            
            // Check if this is an extension point
            if (extensionCell.NodeType == NodeType.Extension)
            {
                Debug.Log($"[PassiveTreeManager] Processing extension point click for {extensionCell.GridPosition}");
                HandleExtensionNodeClick(extensionCell);
            }
            else
            {
                // Handle as a regular node purchase
                Debug.Log($"[PassiveTreeManager] Processing regular node click for {extensionCell.GridPosition}");
                HandleRegularNodeClick(extensionCell);
            }
        }
        
        /// <summary>
        /// Handle regular node click (non-extension nodes)
        /// </summary>
        private void HandleRegularNodeClick(CellController cell)
        {
            if (requireConfirmToAllocate)
            {
                // In confirm mode, ignore immediate purchase flow
                return;
            }
            // Use the appropriate overload based on cell type
            bool canPurchase = false;
            if (cell is CellController_EXT extCell)
            {
                canPurchase = CanPurchaseNode(extCell);
            }
            else
            {
                canPurchase = CanPurchaseNode(cell);
            }
            
            if (canPurchase)
            {
                cell.SetPurchased(true);
                
                // Get the correct position based on cell type
                Vector2Int correctPosition = cell.GridPosition;
                if (cell is CellController_EXT extensionCell)
                {
                    correctPosition = extensionCell.GridPosition; // Use the overridden property
                }
                
                // Check if this is an extension board cell and use board-specific logic
                ExtensionBoardController extensionBoard = cell.GetComponentInParent<ExtensionBoardController>();
                if (extensionBoard != null)
                {
                    // Use board-specific adjacency logic for extension board cells
                    UnlockAdjacentNodesWithinBoard(correctPosition, extensionBoard);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[PassiveTreeManager] ✅ Used board-specific adjacency for extension board cell at {correctPosition} (base GridPosition: {cell.GridPosition})");
                    }
                }
                else
                {
                    // Use cross-board logic for core board cells
                    UnlockAdjacentNodes(correctPosition);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[PassiveTreeManager] ✅ Used cross-board adjacency for core board cell at {correctPosition} (base GridPosition: {cell.GridPosition})");
                    }
                }
                
                // Trigger cross-board allocation if this is an extension point
                if (cell.NodeType == NodeType.Extension)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[PassiveTreeManager] Extension point purchased, triggering cross-board allocation for {cell.GridPosition}");
                    }
                    HandleCrossBoardExtensionAllocation(cell);
                }
                
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Node purchased: {cell.GridPosition}");
            }
            else
            {
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Cannot purchase node at {cell.GridPosition}");
            }
        }
        
        /// <summary>
        /// Handle start node click
        /// </summary>
        private void HandleStartNodeClick(CellController cell)
        {
            if (requireConfirmToAllocate)
            {
                return;
            }
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] HandleStartNodeClick called for {cell.GridPosition}, currently purchased: {cell.IsPurchased}");
            
            // Start node is always available and can be toggled
            if (cell.IsPurchased)
            {
                cell.SetPurchased(false);
                TriggerNodeDeallocatedEvent(cell.GridPosition, cell);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Start node unpurchased: {cell.GridPosition}");
            }
            else
            {
                cell.SetPurchased(true);
                TriggerNodeAllocatedEvent(cell.GridPosition, cell);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Start node purchased: {cell.GridPosition}");
            }
        }
        
        /// <summary>
        /// Handle travel/small node click
        /// </summary>
        private void HandleTravelNodeClick(CellController cell)
        {
            if (requireConfirmToAllocate)
            {
                return;
            }
            if (CanPurchaseNode(cell))
            {
                Debug.Log($"[PassiveTreeManager] ✅ Can purchase node - proceeding with purchase");
                cell.SetPurchased(true);
                Debug.Log($"[PassiveTreeManager] ✅ Node marked as purchased");
                TriggerNodeAllocatedEvent(cell.GridPosition, cell);
                Debug.Log($"[PassiveTreeManager] ✅ Node allocated event triggered");
                UnlockAdjacentNodes(cell.GridPosition);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Travel node purchased: {cell.GridPosition}");
            }
            else
            {
                Debug.Log($"[PassiveTreeManager] ❌ Cannot purchase node - purchase blocked");
            }
        }
        
        /// <summary>
        /// Handle notable node click
        /// </summary>
        private void HandleNotableNodeClick(CellController cell)
        {
            if (requireConfirmToAllocate)
            {
                return;
            }
            if (CanPurchaseNode(cell))
            {
                Debug.Log($"[PassiveTreeManager] ✅ Can purchase notable node - proceeding with purchase");
                cell.SetPurchased(true);
                Debug.Log($"[PassiveTreeManager] ✅ Notable node marked as purchased");
                TriggerNodeAllocatedEvent(cell.GridPosition, cell);
                Debug.Log($"[PassiveTreeManager] ✅ Notable node allocated event triggered");
                UnlockAdjacentNodes(cell.GridPosition);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Notable node purchased: {cell.GridPosition}");
            }
            else
            {
                Debug.Log($"[PassiveTreeManager] ❌ Cannot purchase notable node - purchase blocked");
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
            
            // Extension points should show board selection UI, not be purchased like regular nodes
            // Skip the CanPurchaseNode check for extension points - they should always show the board selection UI
            if (enableExtensionBoards && boardPositioningManager != null)
            {
                Debug.Log($"[PassiveTreeManager] Extension point clicked - showing board selection UI for {cell.GridPosition}");
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
            if (requireConfirmToAllocate)
            {
                return;
            }
            if (CanPurchaseNode(cell))
            {
                cell.SetPurchased(true);
                TriggerNodeAllocatedEvent(cell.GridPosition, cell);
                if (showDebugInfo)
                    Debug.Log($"[PassiveTreeManager] Keystone node purchased: {cell.GridPosition}");
            }
        }
        
        /// <summary>
        /// Check if a position can be allocated using the ExtensionBoardDataManager
        /// </summary>
        public bool CanAllocatePosition(Vector2Int gridPosition, string boardId = null)
        {
            if (ExtensionBoardDataManager.Instance != null)
            {
                return ExtensionBoardDataManager.Instance.CanAllocatePosition(gridPosition, boardId);
            }
            return false;
        }
        
        /// <summary>
        /// Check if a node can be purchased
        /// </summary>
        private bool CanPurchaseNode(CellController cell)
        {
            Debug.Log($"[PassiveTreeManager] CanPurchaseNode check for {cell.GridPosition}:");
            Debug.Log($"  - IsPurchased: {cell.IsPurchased}");
            Debug.Log($"  - IsAdjacent: {cell.IsAdjacent} (Available: {cell.IsAvailable}, Unlocked: {cell.IsUnlocked})");
            
            // Can't purchase if already purchased
            if (cell.IsPurchased) 
            {
                Debug.Log($"  - ❌ Cannot purchase: already purchased");
                return false;
            }
            
            // Can't purchase if not adjacent (combines available and unlocked check)
            if (!cell.IsAdjacent) 
            {
                Debug.Log($"  - ❌ Cannot purchase: not adjacent (not available or not unlocked)");
                return false;
            }
            
            // Check adjacency validation - node must have at least one purchased adjacent node
            // (except for start nodes which are always available)
            if (cell.NodeType != NodeType.Start)
            {
                // Check if this is an extension board cell and use board-specific logic
                ExtensionBoardController extensionBoard = cell.GetComponentInParent<ExtensionBoardController>();
                bool hasAdjacentPurchasedNode;
                
                if (extensionBoard != null)
                {
                    // Use board-specific adjacency logic for extension board cells
                    hasAdjacentPurchasedNode = HasAdjacentPurchasedNodeWithinBoard(cell.GridPosition, extensionBoard);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"  - 🔍 Using board-specific adjacency check for extension board cell");
                    }
                }
                else
                {
                    // Use cross-board logic for core board cells
                    hasAdjacentPurchasedNode = HasAdjacentPurchasedNode(cell.GridPosition);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"  - 🔍 Using cross-board adjacency check for core board cell");
                    }
                }
                
                if (!hasAdjacentPurchasedNode)
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
        /// Check if an extension board node can be purchased (overload for CellController_EXT)
        /// </summary>
        private bool CanPurchaseNode(CellController_EXT cell)
        {
            Vector2Int correctPosition = cell.GridPosition;
            Debug.Log($"[PassiveTreeManager] CanPurchaseNode (EXT) check for {correctPosition} (base GridPosition: {((CellController)cell).GridPosition}):");
            Debug.Log($"  - IsPurchased: {cell.IsPurchased}");
            Debug.Log($"  - IsAdjacent: {cell.IsAdjacent} (Available: {cell.IsAvailable}, Unlocked: {cell.IsUnlocked})");
            
            // Can't purchase if already purchased
            if (cell.IsPurchased) 
            {
                Debug.Log($"  - ❌ Cannot purchase: already purchased");
                return false;
            }
            
            // Can't purchase if not adjacent (combines available and unlocked check)
            if (!cell.IsAdjacent) 
            {
                Debug.Log($"  - ❌ Cannot purchase: not adjacent (not available or not unlocked)");
                return false;
            }
            
            // Check adjacency validation - node must have at least one purchased adjacent node
            // (except for start nodes which are always available)
            if (cell.NodeType != NodeType.Start)
            {
                // Use board-specific adjacency logic for extension board cells
                ExtensionBoardController extensionBoard = cell.GetComponentInParent<ExtensionBoardController>();
                bool hasAdjacentPurchasedNode;
                
                if (extensionBoard != null)
                {
                    // Use board-specific adjacency logic for extension board cells
                    hasAdjacentPurchasedNode = HasAdjacentPurchasedNodeWithinBoard(correctPosition, extensionBoard);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"  - 🔍 Using board-specific adjacency check for extension board cell");
                    }
                }
                else
                {
                    // Use cross-board logic for core board cells
                    hasAdjacentPurchasedNode = HasAdjacentPurchasedNode(correctPosition);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"  - 🔍 Using cross-board adjacency check for core board cell");
                    }
                }
                
                if (!hasAdjacentPurchasedNode)
                {
                    Debug.Log($"  - ❌ Cannot purchase: no adjacent purchased nodes");
                    return false;
                }
                Debug.Log($"  - ✅ Has adjacent purchased node");
            }
            
            // TODO: Add cost validation, prerequisite checks, etc.
            
            Debug.Log($"  - ✅ Can purchase node (EXT)");
            return true;
        }
        
        /// <summary>
        /// Check if a node has at least one adjacent purchased node (for pathing validation)
        /// Checks both core board and extension boards
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
                // First check core board
                if (cells.ContainsKey(pos))
                {
                    CellController adjacentCell = cells[pos];
                    if (adjacentCell.IsPurchased)
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ✅ Found adjacent purchased node at {pos} in core board");
                        return true;
                    }
                }
                
                // Then check extension boards
                CellController extensionBoardCell = FindCellInExtensionBoards(pos);
                if (extensionBoardCell != null && extensionBoardCell.IsPurchased)
                {
                    if (showDebugInfo)
                        Debug.Log($"[PassiveTreeManager] ✅ Found adjacent purchased node at {pos} in extension board");
                    return true;
                }
            }
            
            // NEW: Check for cross-board extension point connections
            // If this is an extension point on an extension board, check if the corresponding
            // extension point on the core board is purchased
            if (IsExtensionPointOnExtensionBoard(centerPosition))
            {
                if (HasCorrespondingCoreBoardExtensionPointPurchased(centerPosition))
                {
                    if (showDebugInfo)
                        Debug.Log($"[PassiveTreeManager] ✅ Found cross-board extension point connection for {centerPosition}");
                    return true;
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] ❌ No adjacent purchased nodes found for {centerPosition}");
            return false;
        }

        /// <summary>
        /// Check if a node has at least one adjacent purchased node within a specific board only
        /// (prevents cross-board contamination, but allows extension point connections)
        /// </summary>
        private bool HasAdjacentPurchasedNodeWithinBoard(Vector2Int centerPosition, ExtensionBoardController targetBoard)
        {
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] HasAdjacentPurchasedNodeWithinBoard check for position: {centerPosition} on board: {targetBoard?.GetBoardName()}");
            
            // Define orthographic directions (up, down, left, right)
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                centerPosition + Vector2Int.up,    // North
                centerPosition + Vector2Int.down,  // South
                centerPosition + Vector2Int.left,  // West
                centerPosition + Vector2Int.right  // East
            };
            
            // Check each adjacent position for a purchased node within the specified board only
            foreach (Vector2Int pos in adjacentPositions)
            {
                if (targetBoard != null)
                {
                    CellController adjacentCell = targetBoard.GetCellAt(pos);
                    if (adjacentCell != null && adjacentCell.IsPurchased)
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ✅ Found adjacent purchased node at {pos} in extension board");
                        return true;
                    }
                }
            }
            
            // SPECIAL CASE: Check for extension point connections
            // If this is an extension point on an extension board, check if the corresponding
            // extension point on the core board is purchased (this allows pathing from extension points)
            if (IsExtensionPointOnExtensionBoard(centerPosition, targetBoard))
            {
                if (HasCorrespondingCoreBoardExtensionPointPurchased(centerPosition))
                {
                    if (showDebugInfo)
                        Debug.Log($"[PassiveTreeManager] ✅ Found extension point connection for {centerPosition}");
                    return true;
                }
            }
            
            // SPECIAL CASE: If this cell is adjacent to an extension point on the same board, 
            // check if that extension point is purchased (this allows pathing to extension points)
            foreach (Vector2Int pos in adjacentPositions)
            {
                if (targetBoard != null)
                {
                    CellController adjacentCell = targetBoard.GetCellAt(pos);
                    if (adjacentCell != null && adjacentCell.NodeType == NodeType.Extension && adjacentCell.IsPurchased)
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ✅ Found adjacent purchased extension point at {pos} in extension board");
                        return true;
                    }
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] ❌ No adjacent purchased nodes found for {centerPosition} within board");
            return false;
        }
        
        /// <summary>
        /// Check if a position is an extension point on an extension board
        /// </summary>
        private bool IsExtensionPointOnExtensionBoard(Vector2Int position)
        {
            // Find the cell in extension boards
            CellController extensionCell = FindCellInExtensionBoards(position);
            if (extensionCell == null) return false;
            
            // Check if it's an extension point
            return extensionCell.NodeType == NodeType.Extension;
        }
        
        /// <summary>
        /// Check if a position is an extension point within a specific extension board
        /// </summary>
        private bool IsExtensionPointOnExtensionBoard(Vector2Int position, ExtensionBoardController targetBoard)
        {
            if (targetBoard == null) return false;
            
            // Check if the target board has a cell at this position
            if (!targetBoard.HasCellAt(position)) return false;
            
            // Get the cell and check if it's an extension point
            CellController cell = targetBoard.GetCellAt(position);
            return cell != null && cell.NodeType == NodeType.Extension;
        }
        
        /// <summary>
        /// Manually refresh all extension board controllers - useful for debugging
        /// </summary>
        [ContextMenu("Refresh All Extension Board Controllers")]
        public void RefreshAllExtensionBoardControllers()
        {
            Debug.Log("[PassiveTreeManager] Manually refreshing all extension board controllers...");
            
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            Debug.Log($"[PassiveTreeManager] Found {extensionBoards.Length} extension boards to refresh");
            
            foreach (ExtensionBoardController boardController in extensionBoards)
            {
                Debug.Log($"[PassiveTreeManager] Refreshing extension board: {boardController.gameObject.name}");
                boardController.RefreshCellMapping();
            }
            
            Debug.Log("[PassiveTreeManager] All extension board controllers refreshed");
        }
        
        /// <summary>
        /// Debug method to show board overlap information
        /// </summary>
        [ContextMenu("Show Board Overlap Information")]
        public void ShowBoardOverlapInformation()
        {
            Debug.Log("=== BOARD OVERLAP ANALYSIS ===");
            
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            Debug.Log($"[PassiveTreeManager] Found {extensionBoards.Length} extension boards");
            
            // Check for overlapping positions
            var overlappingPositions = new Dictionary<Vector2Int, List<string>>();
            
            // Add core board positions
            foreach (var kvp in cells)
            {
                if (!overlappingPositions.ContainsKey(kvp.Key))
                {
                    overlappingPositions[kvp.Key] = new List<string>();
                }
                overlappingPositions[kvp.Key].Add($"Core Board ({kvp.Value.NodeType})");
            }
            
            // Add extension board positions
            foreach (var board in extensionBoards)
            {
                var boardCells = board.GetAllCells();
                foreach (var kvp in boardCells)
                {
                    if (!overlappingPositions.ContainsKey(kvp.Key))
                    {
                        overlappingPositions[kvp.Key] = new List<string>();
                    }
                    overlappingPositions[kvp.Key].Add($"Extension Board: {board.GetBoardName()} ({kvp.Value.NodeType})");
                }
            }
            
            // Report overlaps
            int overlapCount = 0;
            foreach (var kvp in overlappingPositions)
            {
                if (kvp.Value.Count > 1)
                {
                    overlapCount++;
                    Debug.Log($"⚠️ Position {kvp.Key} has {kvp.Value.Count} boards: {string.Join(", ", kvp.Value)}");
                }
            }
            
            if (overlapCount == 0)
            {
                Debug.Log("✅ No board overlaps detected - all positions are unique to one board");
            }
            else
            {
                Debug.Log($"⚠️ Found {overlapCount} overlapping positions - this may cause input routing issues");
            }
            
            Debug.Log("=== BOARD OVERLAP ANALYSIS COMPLETE ===");
        }
        
        /// <summary>
        /// Check if the corresponding extension point on the core board is purchased
        /// This checks if the core board extension point that connects to the current extension board is purchased
        /// </summary>
        private bool HasCorrespondingCoreBoardExtensionPointPurchased(Vector2Int extensionBoardPosition)
        {
            // This method is called when checking if a cell on an extension board is adjacent to an extension point
            // The extensionBoardPosition is the position of the cell being checked, not the extension point itself
            // We need to determine which core board extension point connects to this extension board
            
            // Find all extension boards to determine which core board extension point connects to the current board
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            
            foreach (ExtensionBoardController board in extensionBoards)
            {
                Vector2Int boardGridPosition = board.GetBoardGridPosition();
                
                // Determine which core board extension point connects to this extension board
                Vector2Int coreBoardExtensionPoint = Vector2Int.zero;
                
                if (boardGridPosition == new Vector2Int(1, 0)) // East extension board
                {
                    coreBoardExtensionPoint = new Vector2Int(6, 3); // Core board East extension point
                }
                else if (boardGridPosition == new Vector2Int(-1, 0)) // West extension board
                {
                    coreBoardExtensionPoint = new Vector2Int(0, 3); // Core board West extension point
                }
                else if (boardGridPosition == new Vector2Int(0, 1)) // North extension board
                {
                    coreBoardExtensionPoint = new Vector2Int(3, 6); // Core board North extension point
                }
                else if (boardGridPosition == new Vector2Int(0, -1)) // South extension board
                {
                    coreBoardExtensionPoint = new Vector2Int(3, 0); // Core board South extension point
                }
                
                // Check if this extension board contains the position we're checking
                if (board.HasCellAt(extensionBoardPosition))
                {
                    // Check if the corresponding core board extension point is purchased
                    if (cells.ContainsKey(coreBoardExtensionPoint))
                    {
                        CellController coreBoardCell = cells[coreBoardExtensionPoint];
                        if (coreBoardCell.NodeType == NodeType.Extension && coreBoardCell.IsPurchased)
                        {
                            if (showDebugInfo)
                                Debug.Log($"[PassiveTreeManager] ✅ Found corresponding core board extension point at {coreBoardExtensionPoint} is purchased for board at {boardGridPosition}");
                            return true;
                        }
                    }
                    
                    if (showDebugInfo)
                        Debug.Log($"[PassiveTreeManager] ❌ Core board extension point at {coreBoardExtensionPoint} is not purchased for board at {boardGridPosition}");
                    return false;
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] ❌ No extension board found containing position {extensionBoardPosition}");
            return false;
        }
        
        /// <summary>
        /// Unlock adjacent nodes when a node is purchased
        /// Unlocks nodes in both core board and extension boards
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
                // First check core board
                if (cells.ContainsKey(pos))
                {
                    CellController adjacentCell = cells[pos];
                    
                    if (showDebugInfo)
                        Debug.Log($"[PassiveTreeManager] Checking core board adjacent node at {pos}: Unlocked={adjacentCell.IsUnlocked}, Purchased={adjacentCell.IsPurchased}");
                    
                    if (!adjacentCell.IsAdjacent && !adjacentCell.IsPurchased)
                    {
                        adjacentCell.SetAdjacent(true);
                        unlockedCount++;
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ✅ Made adjacent core board node: {pos} (Type: {adjacentCell.NodeType})");
                    }
                    else
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ⏭️ Skipped core board adjacent node: {pos} (already unlocked or purchased)");
                    }
                }
                else
                {
                    // Check extension boards
                    CellController extensionBoardCell = FindCellInExtensionBoards(pos);
                    if (extensionBoardCell != null)
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] Checking extension board adjacent node at {pos}: Unlocked={extensionBoardCell.IsUnlocked}, Purchased={extensionBoardCell.IsPurchased}");
                        
                        if (!extensionBoardCell.IsAdjacent && !extensionBoardCell.IsPurchased)
                        {
                            extensionBoardCell.SetAdjacent(true);
                            unlockedCount++;
                            if (showDebugInfo)
                                Debug.Log($"[PassiveTreeManager] ✅ Made adjacent extension board node: {pos} (Type: {extensionBoardCell.NodeType})");
                        }
                        else
                        {
                            if (showDebugInfo)
                                Debug.Log($"[PassiveTreeManager] ⏭️ Skipped extension board adjacent node: {pos} (already unlocked or purchased)");
                        }
                    }
                    else
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ⚠️ Adjacent position {pos} not found in core board or extension boards");
                    }
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] UnlockAdjacentNodes complete: {unlockedCount} nodes unlocked from {centerPosition}");
        }

        /// <summary>
        /// Unlock adjacent nodes within a specific board only (prevents cross-board contamination)
        /// </summary>
        private void UnlockAdjacentNodesWithinBoard(Vector2Int centerPosition, ExtensionBoardController targetBoard)
        {
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] UnlockAdjacentNodesWithinBoard called for position: {centerPosition} on board: {targetBoard?.GetBoardName()}");
            
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
                // Only check within the specified extension board
                if (targetBoard != null)
                {
                    CellController adjacentCell = targetBoard.GetCellAt(pos);
                    if (adjacentCell != null)
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] Checking extension board adjacent node at {pos}: Unlocked={adjacentCell.IsUnlocked}, Purchased={adjacentCell.IsPurchased}");
                        
                        if (!adjacentCell.IsAdjacent && !adjacentCell.IsPurchased)
                        {
                            adjacentCell.SetAdjacent(true);
                            unlockedCount++;
                            if (showDebugInfo)
                                Debug.Log($"[PassiveTreeManager] ✅ Made adjacent extension board node: {pos} (Type: {adjacentCell.NodeType})");
                        }
                        else
                        {
                            if (showDebugInfo)
                                Debug.Log($"[PassiveTreeManager] ⏭️ Skipped extension board adjacent node: {pos} (already unlocked or purchased)");
                        }
                    }
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] UnlockAdjacentNodesWithinBoard complete: {unlockedCount} nodes unlocked from {centerPosition}");
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
        /// Initialize the stats integration system
        /// </summary>
        private void InitializeStatsIntegration()
        {
            if (!enableStatsIntegration) return;
            
            if (showDebugInfo)
                PassiveTreeLogger.LogInfo("Initializing stats integration...", "manager");
            
            // Find or create stats integration component
            if (statsIntegration == null)
            {
                statsIntegration = FindFirstObjectByType<PassiveTreeStatsIntegration>();
                
                if (statsIntegration == null)
                {
                    GameObject integrationObj = new GameObject("PassiveTreeStatsIntegration");
                    statsIntegration = integrationObj.AddComponent<PassiveTreeStatsIntegration>();
                    integrationObj.transform.SetParent(transform);
                }
            }
            
            // Set up the integration
            if (statsIntegration != null)
            {
                statsIntegration.SetPassiveTreeManager(this);
                statsIntegration.SetupIntegration();
                
                if (showDebugInfo)
                    PassiveTreeLogger.LogInfo("Stats integration initialized", "manager");
            }
            else
            {
                PassiveTreeLogger.LogWarning("Could not initialize stats integration", "manager");
            }
        }
        
        /// <summary>
        /// Trigger node allocation event for stats integration
        /// </summary>
        private void TriggerNodeAllocatedEvent(Vector2Int position, CellController cell)
        {
            Debug.Log($"[PassiveTreeManager] TriggerNodeAllocatedEvent called for {position}, enableStatsIntegration: {enableStatsIntegration}");
            
            if (enableStatsIntegration)
            {
                Debug.Log($"[PassiveTreeManager] Invoking OnNodeAllocated event for {position}");
                OnNodeAllocated?.Invoke(position, cell);
                
                if (showDebugInfo)
                    PassiveTreeLogger.LogInfo($"Node allocated event triggered for {position}", "manager");
            }
            else
            {
                Debug.Log($"[PassiveTreeManager] Stats integration disabled - not triggering event");
            }
        }
        
        /// <summary>
        /// Trigger node deallocation event for stats integration
        /// </summary>
        private void TriggerNodeDeallocatedEvent(Vector2Int position, CellController cell)
        {
            if (enableStatsIntegration)
            {
                OnNodeDeallocated?.Invoke(position, cell);
                
                if (showDebugInfo)
                    PassiveTreeLogger.LogInfo($"Node deallocated event triggered for {position}", "manager");
            }
        }
        
        /// <summary>
        /// Handle extension board spawning when an extension node is purchased
        /// </summary>
        private void HandleExtensionBoardSpawn(CellController extensionCell)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 🚀 HandleExtensionBoardSpawn called for cell at {extensionCell.GridPosition}");
            }
            
            if (boardPositioningManager == null || boardGenerator == null)
            {
                Debug.LogWarning("[PassiveTreeManager] ❌ Extension board system not properly initialized");
                return;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] ✅ Board positioning manager and board generator found");
            }
            
            // First, try to find the extension point that corresponds to this cell
            ExtensionPoint targetPoint = null;
            
            // Get ALL extension points (not just available ones) to find the one at this position
            var allExtensionPoints = boardPositioningManager.GetAllExtensionPoints();
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 📊 Found {allExtensionPoints.Count} extension points from board positioning manager");
            }
            
            foreach (var point in allExtensionPoints)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[PassiveTreeManager] 🔍 Checking extension point: {point.id} at position {point.position}");
                }
                
                if (point.position == extensionCell.GridPosition)
                {
                    targetPoint = point;
                    if (showDebugInfo)
                    {
                        Debug.Log($"[PassiveTreeManager] ✅ Found matching extension point: {point.id} at {point.position}");
                    }
                    break;
                }
            }
            
            if (targetPoint == null)
            {
                Debug.LogWarning($"[PassiveTreeManager] ❌ No extension point found for cell at {extensionCell.GridPosition} - this cell is not an extension point");
                return;
            }
            
            // Check if this extension point can connect (is available)
            if (!targetPoint.CanConnect())
            {
                Debug.Log($"[PassiveTreeManager] ❌ Extension point at {extensionCell.GridPosition} is already allocated and cannot accept new connections");
                return;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] ✅ Extension point can connect, proceeding with board creation");
            }
            
            // Get the source board from the extension cell
            GameObject sourceBoard = null;
            ExtensionBoardController extensionBoardController = extensionCell.GetComponentInParent<ExtensionBoardController>();
            if (extensionBoardController != null)
            {
                sourceBoard = extensionBoardController.gameObject;
                if (showDebugInfo)
                {
                    Debug.Log($"[PassiveTreeManager] 🏗️ Extension point click from extension board: {sourceBoard.name}");
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[PassiveTreeManager] 🏗️ Extension point click from core board");
                }
            }
            
            // FIRST: Create the extension board
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 🏗️ Calling boardPositioningManager.HandleExtensionPointClick for {targetPoint.id}");
            }
            
            boardPositioningManager.HandleExtensionPointClick(targetPoint, sourceBoard);
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 🏗️ Board creation call completed, starting cross-board allocation coroutine");
            }
            
            // NOTE: Cross-board allocation is now handled by BoardPositioningManager
            // The extension point purchasing is handled in OnBoardSelected method
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 🏗️ Board creation completed, extension point purchasing handled by BoardPositioningManager");
            }
        }
        
        
        /// <summary>
        /// Public method to trigger cross-board allocation from external controllers
        /// </summary>
        public void TriggerCrossBoardAllocation(CellController extensionCell)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] TriggerCrossBoardAllocation called for {extensionCell.GridPosition}");
            }
            HandleCrossBoardExtensionAllocation(extensionCell);
        }
        
        /// <summary>
        /// Handle cross-board extension point allocation
        /// When an extension point is allocated on one board, allocate the corresponding point on connected boards
        /// </summary>
        private void HandleCrossBoardExtensionAllocation(CellController extensionCell)
        {
            Debug.Log($"[PassiveTreeManager] 🔗 Handling cross-board extension allocation for {extensionCell.GridPosition}");
            
            // Get the extension point name from the cell (e.g., "Extension_East")
            string extensionPointName = GetExtensionPointName(extensionCell);
            if (string.IsNullOrEmpty(extensionPointName))
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[PassiveTreeManager] ❌ Could not determine extension point name for cell at {extensionCell.GridPosition}");
                }
                return;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 📍 Extension point name: {extensionPointName}");
            }
            
            // Find the corresponding extension point on other boards
            string correspondingPointName = GetCorrespondingExtensionPointName(extensionPointName);
            if (string.IsNullOrEmpty(correspondingPointName))
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[PassiveTreeManager] ❌ No corresponding extension point found for {extensionPointName}");
                }
                return;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 🔄 Looking for corresponding extension point: {correspondingPointName}");
            }
            
            // Find and allocate the corresponding extension point
            AllocateCorrespondingExtensionPoint(correspondingPointName);
        }
        
        /// <summary>
        /// Get the extension point name from a cell (e.g., "Extension_East")
        /// Uses GameObject name to determine extension point type
        /// </summary>
        private string GetExtensionPointName(CellController cell)
        {
            if (cell == null) return null;
            
            Debug.Log($"[PassiveTreeManager] 🔍 Getting extension point name for cell at {cell.GridPosition} (GameObject: {cell.gameObject.name})");
            
            // First try to get from CellJsonData (for extension boards)
            var cellJsonData = cell.GetComponent<CellJsonData>();
            if (cellJsonData != null && !string.IsNullOrEmpty(cellJsonData.NodeName))
            {
                Debug.Log($"[PassiveTreeManager] 🔍 CellJsonData NodeName: {cellJsonData.NodeName}");
                return cellJsonData.NodeName;
            }
            
            // Check if this is an extension point by looking at the GameObject name
            string gameObjectName = cell.gameObject.name;
            
            Debug.Log($"[PassiveTreeManager] 🔍 Checking GameObject name: {gameObjectName} for cell at {cell.GridPosition}");
            
            // Map GameObject names to extension point names
            switch (gameObjectName)
            {
                case "Cell_0_3":  // Extension_North (top)
                    Debug.Log($"[PassiveTreeManager] ✅ Mapped {gameObjectName} to Extension_North");
                    return "Extension_North";
                case "Cell_3_0":  // Extension_West (left)
                    Debug.Log($"[PassiveTreeManager] ✅ Mapped {gameObjectName} to Extension_West");
                    return "Extension_West";
                case "Cell_3_6":  // Extension_East (right)
                    Debug.Log($"[PassiveTreeManager] ✅ Mapped {gameObjectName} to Extension_East");
                    return "Extension_East";
                case "Cell_6_3":  // Extension_South (bottom)
                    Debug.Log($"[PassiveTreeManager] ✅ Mapped {gameObjectName} to Extension_South");
                    return "Extension_South";
                default:
                    Debug.Log($"[PassiveTreeManager] 🔍 No direct mapping for {gameObjectName}, checking NodeName");
            // Fallback to base class node name
            if (!string.IsNullOrEmpty(cell.NodeName))
            {
                        Debug.Log($"[PassiveTreeManager] ✅ Using NodeName: {cell.NodeName}");
                return cell.NodeName;
            }
                    Debug.Log($"[PassiveTreeManager] ❌ No extension point name found for {gameObjectName}");
            return null;
            }
        }
        
        /// <summary>
        /// Get the corresponding extension point name for cross-board allocation
        /// </summary>
        private string GetCorrespondingExtensionPointName(string extensionPointName)
        {
            // Return the same extension point name - no swapping needed
            // When Extension_West is clicked, allocate Extension_West on the new board
            switch (extensionPointName)
            {
                case "Extension_East":
                    return "Extension_East";
                case "Extension_West":
                    return "Extension_West";
                case "Extension_North":
                    return "Extension_North";
                case "Extension_South":
                    return "Extension_South";
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Find and allocate the corresponding extension point on other boards
        /// </summary>
        private void AllocateCorrespondingExtensionPoint(string correspondingPointName)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 🔍 Looking for corresponding extension point: {correspondingPointName}");
            }
            
            // Find all extension boards in the scene
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 📊 Found {extensionBoards.Length} extension boards in scene");
            }
            
            foreach (ExtensionBoardController board in extensionBoards)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[PassiveTreeManager] 🔍 Checking board: {board.GetBoardName()} at position {board.GetBoardGridPosition()}");
                }
                
                // Find cells with the corresponding extension point name
                CellController[] cells = board.GetComponentsInChildren<CellController>();
                
                if (showDebugInfo)
                {
                    Debug.Log($"[PassiveTreeManager] 📊 Board {board.GetBoardName()} has {cells.Length} cells");
                }
                
                foreach (CellController cell in cells)
                {
                    string cellExtensionName = GetExtensionPointName(cell);
                    if (showDebugInfo)
                    {
                        Debug.Log($"[PassiveTreeManager] 🔍 Checking cell {cell.gameObject.name} at {cell.GridPosition}: ExtensionName={cellExtensionName}, Target={correspondingPointName}");
                    }
                    
                    if (cellExtensionName == correspondingPointName)
                    {
                        if (showDebugInfo)
                        {
                            Debug.Log($"[PassiveTreeManager] ✅ Found corresponding extension point {correspondingPointName} at {cell.GridPosition} on board {board.GetBoardName()}");
                            Debug.Log($"[PassiveTreeManager] 📊 Cell state before allocation - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
                        }
                        
                        // Allocate the corresponding extension point
                        AllocateExtensionPoint(cell);
                        
                        // Handle world board adjacency logic (only for new board creation)
                        // This should only be called when creating new boards, not when allocating on existing boards
                        if (IsNewBoardCreation(cell))
                        {
                            HandleWorldBoardAdjacency(cell);
                        }
                        
                        if (showDebugInfo)
                        {
                            Debug.Log($"[PassiveTreeManager] 📊 Cell state after allocation - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
                        }
                        
                        return; // Only allocate the first matching point
                    }
                }
            }
            
            if (showDebugInfo)
            {
                Debug.LogWarning($"[PassiveTreeManager] ❌ No corresponding extension point {correspondingPointName} found on any extension board");
            }
        }
        
        /// <summary>
        /// Allocate an extension point (mark as purchased and unlocked)
        /// </summary>
        public void AllocateExtensionPoint(CellController extensionCell)
        {
            if (extensionCell == null) return;
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 🎯 Allocating extension point {extensionCell.gameObject.name} at {extensionCell.GridPosition}");
                Debug.Log($"[PassiveTreeManager] 📊 Cell type: {extensionCell.GetType().Name}");
            }
            
            // Mark the extension point as purchased and adjacent
            extensionCell.SetPurchased(true);
            extensionCell.SetAdjacent(true);
            
            // IMPORTANT: Set this cell as an extension point
            extensionCell.SetAsExtensionPoint(true);
            
            // Force update visual state
            extensionCell.UpdateVisualState();
            
            // If this is a CellController_EXT, force a refresh
            if (extensionCell is CellController_EXT extCell)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[PassiveTreeManager] 🔄 Forcing CellController_EXT refresh for {extCell.gameObject.name}");
                }
                
                // Force the extension cell to refresh its state
                extCell.UpdateVisualState();
                
                // Also try to force the extension board controller to refresh
                var boardController = extCell.GetComponentInParent<ExtensionBoardController>();
                if (boardController != null)
                {
            if (showDebugInfo)
            {
                        Debug.Log($"[PassiveTreeManager] 🔄 Notifying extension board controller of cell purchase");
                    }
                    boardController.OnCellPurchased(extCell.GridPosition);
                }
            }
            
            // For cross-board extension points, we need to unlock the correct adjacent nodes
            // based on the world grid position, not the local board position
            UnlockCrossBoardAdjacentNodes(extensionCell);
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] ✅ Successfully allocated extension point {extensionCell.gameObject.name} at {extensionCell.GridPosition}");
                Debug.Log($"[PassiveTreeManager] 📊 Final state - IsPurchased: {extensionCell.IsPurchased}, IsUnlocked: {extensionCell.IsUnlocked}, IsAvailable: {extensionCell.IsAvailable}, IsExtensionPoint: {extensionCell.IsExtensionPoint}");
            }
        }
        
        /// <summary>
        /// Check if this is a new board creation (not just allocating on existing board)
        /// </summary>
        private bool IsNewBoardCreation(CellController cell)
        {
            // Adjacency logic should run when:
            // 1. The cell is an extension point
            // 2. The cell is being allocated for the first time
            // 3. There are adjacent boards that need corresponding extension points allocated
            
            // For now, enable adjacency logic for extension points
            // This will handle the case where Extension_North on (0,-1) should allocate Extension_South on (0,0)
            return cell.IsExtensionPoint;
        }
        
        /// <summary>
        /// Handle world board adjacency logic when an extension point is allocated
        /// </summary>
        private void HandleWorldBoardAdjacency(CellController extensionCell)
        {
            Debug.Log($"[PassiveTreeManager] 🌍 ADJACENCY: Starting adjacency logic for cell at {extensionCell.GridPosition}");
            
            if (WorldBoardAdjacencyManager.Instance == null)
            {
                Debug.LogWarning($"[PassiveTreeManager] ❌ ADJACENCY: WorldBoardAdjacencyManager not available, skipping adjacency logic");
                return;
            }
            
            // Get the board controller to find the world position
            ExtensionBoardController boardController = extensionCell.GetComponentInParent<ExtensionBoardController>();
            if (boardController == null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[PassiveTreeManager] Could not find ExtensionBoardController for adjacency logic");
                }
                return;
            }
            
            // Get the board's world position (this needs to be implemented in ExtensionBoardController)
            Vector2Int boardWorldPosition = GetBoardWorldPosition(boardController);
            Vector2Int cellPosition = extensionCell.GridPosition;
            string extensionPointName = GetExtensionPointName(extensionCell);
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 🌍 Handling adjacency for extension point '{extensionPointName}' at cell {cellPosition} on board at world position {boardWorldPosition}");
            }
            
            // Delegate to the WorldBoardAdjacencyManager
            WorldBoardAdjacencyManager.Instance.HandleExtensionPointAllocation(boardWorldPosition, cellPosition, extensionPointName);
        }
        
        /// <summary>
        /// Get the world position of a board
        /// </summary>
        private Vector2Int GetBoardWorldPosition(ExtensionBoardController boardController)
        {
            return boardController.GetWorldGridPosition();
        }
        
        
        /// <summary>
        /// Allocate an extension point within a specific board only (prevents cross-board contamination)
        /// </summary>
        private void AllocateExtensionPointWithinBoard(CellController extensionCell, ExtensionBoardController targetBoard)
        {
            if (extensionCell == null)
            {
                Debug.LogError("[PassiveTreeManager] Cannot allocate extension point - cell is null");
                return;
            }
            
            if (targetBoard == null)
            {
                Debug.LogError("[PassiveTreeManager] Cannot allocate extension point - target board is null");
                return;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] 🎯 Allocating extension point within board: {extensionCell.gameObject.name} at {extensionCell.GridPosition} on board {targetBoard.GetBoardName()}");
                Debug.Log($"[PassiveTreeManager] 📊 Cell type: {extensionCell.GetType().Name}");
            }
            
            // Mark the extension point as purchased and adjacent
            extensionCell.SetPurchased(true);
            extensionCell.SetAdjacent(true);
            
            // IMPORTANT: Set this cell as an extension point
            extensionCell.SetAsExtensionPoint(true);
            
            // Force update visual state
            extensionCell.UpdateVisualState();
            
            // If this is a CellController_EXT, force a refresh
            if (extensionCell is CellController_EXT extCell)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[PassiveTreeManager] 🔄 Forcing CellController_EXT refresh for {extCell.gameObject.name}");
                }
                
                // Force the extension cell to refresh its state
                extCell.UpdateVisualState();
                
                // Also try to force the extension board controller to refresh
                var boardController = extCell.GetComponentInParent<ExtensionBoardController>();
                if (boardController != null)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[PassiveTreeManager] 🔄 Notifying extension board controller of cell purchase");
                    }
                    boardController.OnCellPurchased(extCell.GridPosition);
                }
            }
            
            // Unlock adjacent nodes only within the target board
            UnlockAdjacentNodesWithinBoard(extensionCell.GridPosition, targetBoard);
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] ✅ Successfully allocated extension point within board: {extensionCell.gameObject.name} at {extensionCell.GridPosition}");
                Debug.Log($"[PassiveTreeManager] 📊 Final state - IsPurchased: {extensionCell.IsPurchased}, IsUnlocked: {extensionCell.IsUnlocked}, IsAvailable: {extensionCell.IsAvailable}, IsExtensionPoint: {extensionCell.IsExtensionPoint}");
            }
        }
        
        /// <summary>
        /// Unlock adjacent nodes for cross-board extension points
        /// This handles the world grid positioning correctly for extension boards
        /// </summary>
        private void UnlockCrossBoardAdjacentNodes(CellController extensionCell)
        {
            if (extensionCell == null) return;
            
            // Get the extension board controller to find the world position
            ExtensionBoardController boardController = extensionCell.GetComponentInParent<ExtensionBoardController>();
            if (boardController == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[PassiveTreeManager] ❌ Could not find ExtensionBoardController for extension cell at {extensionCell.GridPosition}");
                return;
            }
            
            // Get the world grid position of the board
            Vector2Int boardWorldPosition = boardController.GetBoardGridPosition();
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] 📍 Board world position: {boardWorldPosition}, Extension cell local position: {extensionCell.GridPosition}");
            
            // Calculate the world position of the extension point
            Vector2Int extensionWorldPosition = boardWorldPosition + extensionCell.GridPosition;
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] 🌍 Extension point world position: {extensionWorldPosition}");
            
            // Cross-board adjacency is now handled by board-specific logic
            // No need to unlock nodes in the world grid as this causes cross-board contamination
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeManager] ⏭️ Skipping world grid adjacency unlock to prevent cross-board contamination");
            }
        }
        
        /// <summary>
        /// Check if a cell is an extension point by GameObject name
        /// </summary>
        private bool IsExtensionPointByGameObjectName(CellController cell)
        {
            if (cell == null) return false;
            
            string gameObjectName = cell.gameObject.name;
            return gameObjectName == "Cell_0_3" ||  // Extension_North (top)
                   gameObjectName == "Cell_3_0" ||  // Extension_West (left)
                   gameObjectName == "Cell_3_6" ||  // Extension_East (right)
                   gameObjectName == "Cell_6_3";    // Extension_South (bottom)
        }
        
        /// <summary>
        /// Unlock adjacent nodes at a specific world grid position
        /// This ensures we're unlocking the correct nodes in the world grid
        /// </summary>
        private void UnlockAdjacentNodesAtWorldPosition(Vector2Int worldPosition)
        {
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] UnlockAdjacentNodesAtWorldPosition called for world position: {worldPosition}");
            
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                worldPosition + Vector2Int.up,    // North
                worldPosition + Vector2Int.down,  // South
                worldPosition + Vector2Int.left,  // West
                worldPosition + Vector2Int.right  // East
            };
            
            int unlockedCount = 0;
            
            foreach (Vector2Int pos in adjacentPositions)
            {
                // First check core board
                if (cells.ContainsKey(pos))
                {
                    CellController adjacentCell = cells[pos];
                    
                    if (showDebugInfo)
                        Debug.Log($"[PassiveTreeManager] Checking core board adjacent node at {pos}: Unlocked={adjacentCell.IsUnlocked}, Purchased={adjacentCell.IsPurchased}");
                    
                    if (!adjacentCell.IsAdjacent && !adjacentCell.IsPurchased)
                    {
                        adjacentCell.SetAdjacent(true);
                        unlockedCount++;
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ✅ Made adjacent core board node: {pos} (Type: {adjacentCell.NodeType})");
                    }
                    else
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ⏭️ Skipped core board adjacent node: {pos} (already unlocked or purchased)");
                    }
                }
                else
                {
                    // Check extension boards for adjacent nodes
                    CellController extensionBoardCell = FindCellInExtensionBoards(pos);
                    if (extensionBoardCell != null)
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] Checking extension board adjacent node at {pos}: Unlocked={extensionBoardCell.IsUnlocked}, Purchased={extensionBoardCell.IsPurchased}");
                        
                        if (!extensionBoardCell.IsAdjacent && !extensionBoardCell.IsPurchased)
                        {
                            extensionBoardCell.SetAdjacent(true);
                            unlockedCount++;
                            if (showDebugInfo)
                                Debug.Log($"[PassiveTreeManager] ✅ Made adjacent extension board node: {pos} (Type: {extensionBoardCell.NodeType})");
                        }
                        else
                        {
                            if (showDebugInfo)
                                Debug.Log($"[PassiveTreeManager] ⏭️ Skipped extension board adjacent node: {pos} (already unlocked or purchased)");
                        }
                    }
                    else
                    {
                        if (showDebugInfo)
                            Debug.Log($"[PassiveTreeManager] ⚠️ Adjacent position {pos} not found in core board or extension boards");
                    }
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[PassiveTreeManager] UnlockAdjacentNodesAtWorldPosition complete: {unlockedCount} nodes unlocked from {worldPosition}");
        }
        
        /// <summary>
        /// Test cross-board extension point allocation
        /// This method can be used to test the cross-board allocation system
        /// </summary>
        [ContextMenu("Test Cross-Board Extension Allocation")]
        public void TestCrossBoardExtensionAllocation()
        {
            Debug.Log("=== TESTING CROSS-BOARD EXTENSION ALLOCATION ===");
            
            // Find all extension boards
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            Debug.Log($"[PassiveTreeManager] Found {extensionBoards.Length} extension boards");
            
            foreach (ExtensionBoardController board in extensionBoards)
            {
                Debug.Log($"[PassiveTreeManager] Testing board: {board.GetBoardName()} at position {board.GetBoardGridPosition()}");
                
                // Find extension points on this board
                CellController[] cells = board.GetComponentsInChildren<CellController>();
                foreach (CellController cell in cells)
                {
                    // Check if this is an extension point by GameObject name or NodeType
                    if (IsExtensionPointByGameObjectName(cell) || cell.NodeType == NodeType.Extension)
                    {
                        string extensionName = GetExtensionPointName(cell);
                        if (!string.IsNullOrEmpty(extensionName))
                        {
                            Debug.Log($"[PassiveTreeManager] 🎯 Found extension point: {extensionName} at {cell.GridPosition} (GameObject: {cell.gameObject.name})");
                            
                            // Test the cross-board allocation
                            HandleCrossBoardExtensionAllocation(cell);
                        }
                    }
                }
            }
            
            Debug.Log("=== CROSS-BOARD EXTENSION ALLOCATION TEST COMPLETE ===");
        }
        
        /// <summary>
        /// Test coordinate system fixes
        /// This method validates that coordinates are correctly mapped
        /// </summary>
        [ContextMenu("Test Coordinate System Fixes")]
        public void TestCoordinateSystemFixes()
        {
            Debug.Log("=== TESTING COORDINATE SYSTEM FIXES ===");
            
            // Test Extension_West should be at (0,3) not (3,0)
            Debug.Log("[PassiveTreeManager] 🎯 Testing Extension_West coordinate mapping...");
            
            // Find Extension_West on core board
            if (cells.ContainsKey(new Vector2Int(0, 3)))
            {
                CellController extensionWest = cells[new Vector2Int(0, 3)];
                Debug.Log($"[PassiveTreeManager] ✅ Found Extension_West at correct position (0,3): {extensionWest.gameObject.name}");
                
                if (extensionWest.NodeType == NodeType.Extension)
                {
                    Debug.Log($"[PassiveTreeManager] ✅ Extension_West has correct NodeType: Extension");
                }
                else
                {
                    Debug.LogWarning($"[PassiveTreeManager] ❌ Extension_West has incorrect NodeType: {extensionWest.NodeType}");
                }
            }
            else
            {
                Debug.LogWarning("[PassiveTreeManager] ❌ Extension_West not found at position (0,3)");
            }
            
            // Test other extension points
            Vector2Int[] expectedExtensionPositions = {
                new Vector2Int(0, 3), // Extension_West
                new Vector2Int(3, 0), // Extension_South  
                new Vector2Int(3, 6), // Extension_East
                new Vector2Int(6, 3)  // Extension_North
            };
            
            foreach (Vector2Int pos in expectedExtensionPositions)
            {
                if (cells.ContainsKey(pos))
                {
                    CellController cell = cells[pos];
                    Debug.Log($"[PassiveTreeManager] ✅ Found extension point at {pos}: {cell.gameObject.name} (Type: {cell.NodeType})");
                }
                else
                {
                    Debug.LogWarning($"[PassiveTreeManager] ❌ No extension point found at {pos}");
                }
            }
            
            Debug.Log("=== COORDINATE SYSTEM FIXES TEST COMPLETE ===");
        }
        
        /// <summary>
        /// Check extension board creation status
        /// </summary>
        [ContextMenu("Check Extension Board Status")]
        public void CheckExtensionBoardStatus()
        {
            Debug.Log("=== CHECKING EXTENSION BOARD STATUS ===");
            
            // Check if board positioning manager exists
            if (boardPositioningManager == null)
            {
                Debug.LogWarning("[PassiveTreeManager] ❌ Board positioning manager is null");
                return;
            }
            
            // Check if board generator exists
            if (boardGenerator == null)
            {
                Debug.LogWarning("[PassiveTreeManager] ❌ Board generator is null");
                return;
            }
            
            Debug.Log("[PassiveTreeManager] ✅ Board positioning manager and board generator found");
            
            // Check extension points
            var allExtensionPoints = boardPositioningManager.GetAllExtensionPoints();
            Debug.Log($"[PassiveTreeManager] 📊 Found {allExtensionPoints.Count} extension points");
            
            foreach (var point in allExtensionPoints)
            {
                Debug.Log($"[PassiveTreeManager] 🔍 Extension point: {point.id} at {point.position}, CanConnect: {point.CanConnect()}");
            }
            
            // Check for existing extension boards
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            Debug.Log($"[PassiveTreeManager] 📊 Found {extensionBoards.Length} extension boards in scene");
            
            foreach (ExtensionBoardController board in extensionBoards)
            {
                Debug.Log($"[PassiveTreeManager] 🎯 Extension board: {board.GetBoardName()} at position {board.GetBoardGridPosition()}");
            }
            
            Debug.Log("=== EXTENSION BOARD STATUS CHECK COMPLETE ===");
        }
        
        /// <summary>
        /// Test core board extension points
        /// </summary>
        [ContextMenu("Test Core Board Extension Points")]
        public void TestCoreBoardExtensionPoints()
        {
            Debug.Log("=== TESTING CORE BOARD EXTENSION POINTS ===");
            
            int extensionPointsFound = 0;
            
            foreach (var kvp in cells)
            {
                CellController cell = kvp.Value;
                if (cell.NodeType == NodeType.Extension || IsExtensionPointByGameObjectName(cell))
                {
                    extensionPointsFound++;
                    string extensionName = GetExtensionPointName(cell);
                    Debug.Log($"[PassiveTreeManager] 🎯 Found core board extension point: {extensionName} at {cell.GridPosition} (GameObject: {cell.gameObject.name})");
                    Debug.Log($"[PassiveTreeManager] 📊 State - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
                }
            }
            
            Debug.Log($"[PassiveTreeManager] 📊 Total core board extension points found: {extensionPointsFound}");
            Debug.Log("=== CORE BOARD EXTENSION POINTS TEST COMPLETE ===");
        }
        
        /// <summary>
        /// Manually allocate a specific extension point for testing
        /// </summary>
        [ContextMenu("Manually Allocate Extension Points")]
        public void ManuallyAllocateAllExtensionPoints()
        {
            Debug.Log("=== MANUALLY ALLOCATING ALL EXTENSION POINTS ===");
            
            // Find all extension boards
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            
            if (extensionBoards.Length == 0)
            {
                Debug.LogWarning("[PassiveTreeManager] ❌ No extension boards found in scene. You need to create an extension board first.");
                Debug.Log("[PassiveTreeManager] 💡 To create an extension board:");
                Debug.Log("   1. Click on an extension point (Extension_East, Extension_West, etc.) on the core board");
                Debug.Log("   2. Select a board from the board selection UI");
                Debug.Log("   3. The extension board will be created and then you can test allocation");
                return;
            }
            
            Debug.Log($"[PassiveTreeManager] 📊 Found {extensionBoards.Length} extension boards in scene");
            
            foreach (ExtensionBoardController board in extensionBoards)
            {
                Debug.Log($"[PassiveTreeManager] 🔍 Checking board: {board.GetBoardName()} at position {board.GetBoardGridPosition()}");
                
                CellController[] cells = board.GetComponentsInChildren<CellController>();
                Debug.Log($"[PassiveTreeManager] 📊 Board has {cells.Length} cells");
                
                // Find all extension points on this board
                foreach (CellController cell in cells)
                {
                    if (cell.NodeType == NodeType.Extension && cell.IsExtensionPoint)
                    {
                        if (showDebugInfo)
                        {
                            Debug.Log($"[PassiveTreeManager] 🔍 Found extension point: {cell.gameObject.name} at {cell.GridPosition}");
                        }
                        
                        if (!cell.IsPurchased)
                        {
                            Debug.Log($"[PassiveTreeManager] 🎯 Allocating extension point at {cell.GridPosition} on board {board.GetBoardName()}");
                            Debug.Log($"[PassiveTreeManager] 📊 Before allocation - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
                            
                            // Use board-specific allocation to prevent cross-board contamination
                            AllocateExtensionPointWithinBoard(cell, board);
                            
                            Debug.Log($"[PassiveTreeManager] 📊 After allocation - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
                        }
                        else
                        {
                            Debug.Log($"[PassiveTreeManager] ⏭️ Extension point at {cell.GridPosition} already purchased");
                        }
                    }
                }
            }
            
            Debug.Log("[PassiveTreeManager] ✅ Manual extension point allocation complete");
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
        
        
        #endregion
    }
}
