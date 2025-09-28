using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Controls an individual extension board
    /// Manages cells, extension points, and board-specific functionality
    /// </summary>
    public class ExtensionBoardController : MonoBehaviour
    {
        [Header("Board Configuration")]
        [SerializeField] private BoardTheme boardTheme = BoardTheme.General;
        [SerializeField] private Vector2Int boardSize = new Vector2Int(7, 7);
        [SerializeField] private string boardId;
        [SerializeField] private string boardName;
        [SerializeField] private string boardDescription;
        
        [Header("Board State")]
        [SerializeField] private bool isActive = true;
        [SerializeField] private bool isUnlocked = false;
        [SerializeField] private int allocatedPoints = 0;
        [SerializeField] private int maxPoints = 0;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        [Header("References")]
        [SerializeField] private BoardPositioningManager boardPositioningManager;
        
        // Board data
        private Dictionary<Vector2Int, CellController> cells = new Dictionary<Vector2Int, CellController>();
        private List<ExtensionPoint> extensionPoints = new List<ExtensionPoint>();
        private Vector2Int boardGridPosition = Vector2Int.zero;
        
        // Events
        public System.Action<ExtensionBoardController> OnBoardActivated;
        public System.Action<ExtensionBoardController> OnBoardDeactivated;
        
        void Start()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Start() called for {gameObject.name}");
            }
            
            // Find the BoardPositioningManager if not assigned
            if (boardPositioningManager == null)
            {
                boardPositioningManager = FindFirstObjectByType<BoardPositioningManager>();
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardController] BoardPositioningManager {(boardPositioningManager != null ? "found" : "not found")} for {gameObject.name}");
                }
            }
            
            InitializeBoard();
        }
        
        /// <summary>
        /// Initialize the extension board
        /// </summary>
        public void Initialize(BoardTheme theme, Vector2Int size)
        {
            boardTheme = theme;
            boardSize = size;
            boardId = $"extension_{theme}_{System.Guid.NewGuid().ToString("N")[..8]}";
            boardName = $"{theme} Extension Board";
            boardDescription = $"A {theme.ToString().ToLower()} themed extension board";
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Initialize() called for {gameObject.name} with theme {theme} and size {size}");
            }
            
            InitializeBoard();
        }
        
        /// <summary>
        /// Initialize board components and state
        /// </summary>
        private void InitializeBoard()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] InitializeBoard() called for {gameObject.name}");
            }
            
            // Find all cell controllers in this board (both CellController and CellController_EXT)
            CellController[] foundCells = GetComponentsInChildren<CellController>();
            CellController_EXT[] foundExtCells = GetComponentsInChildren<CellController_EXT>();
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Found {foundCells.Length} CellController and {foundExtCells.Length} CellController_EXT components in {gameObject.name}");
                
                // Debug: List all found cells
                for (int i = 0; i < foundCells.Length; i++)
                {
                    Debug.Log($"[ExtensionBoardController] Cell {i}: {foundCells[i].gameObject.name} at {foundCells[i].GridPosition}");
                }
                for (int i = 0; i < foundExtCells.Length; i++)
                {
                    Debug.Log($"[ExtensionBoardController] ExtCell {i}: {foundExtCells[i].gameObject.name} at {foundExtCells[i].GridPosition}");
                }
            }
            
            // Map regular CellController components
            foreach (CellController cell in foundCells)
            {
                if (cell.GridPosition.x >= 0 && cell.GridPosition.x < boardSize.x &&
                    cell.GridPosition.y >= 0 && cell.GridPosition.y < boardSize.y)
                {
                    cells[cell.GridPosition] = cell;
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] Mapped regular cell at {cell.GridPosition} in {gameObject.name}");
                    }
                    
                    SetupCellInteraction(cell);
                }
            }
            
            // Map CellController_EXT components (these take precedence for extension boards)
            foreach (CellController_EXT extCell in foundExtCells)
            {
                if (extCell.GridPosition.x >= 0 && extCell.GridPosition.x < boardSize.x &&
                    extCell.GridPosition.y >= 0 && extCell.GridPosition.y < boardSize.y)
                {
                    cells[extCell.GridPosition] = extCell; // CellController_EXT inherits from CellController
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] Mapped extension cell at {extCell.GridPosition} in {gameObject.name}");
                        Debug.Log($"[ExtensionBoardController] Cell name: {extCell.gameObject.name}, Type: {extCell.NodeType}");
                    }
                    
                    SetupCellInteraction(extCell);
                }
            }
            
            // Initialize board state
            InitializeBoardState();
            
            if (showDebugInfo)
                Debug.Log($"[ExtensionBoardController] Initialized {boardName} with {cells.Count} cells");
        }
        
        /// <summary>
        /// Setup cell interaction for extension board cells
        /// </summary>
        private void SetupCellInteraction(CellController cell)
        {
            // Add a custom component to handle clicks for extension boards
            var clickHandler = cell.gameObject.GetComponent<ExtensionBoardCellClickHandler>();
            if (clickHandler == null)
            {
                clickHandler = cell.gameObject.AddComponent<ExtensionBoardCellClickHandler>();
            }
            clickHandler.Initialize(this, cell.GridPosition);
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Set up cell interaction for cell at {cell.GridPosition} (Type: {cell.NodeType})");
            }
        }
        
        /// <summary>
        /// Initialize the board's initial state
        /// </summary>
        private void InitializeBoardState()
        {
            // All cells start locked except for connection points
            foreach (var kvp in cells)
            {
                CellController cell = kvp.Value;
                
                if (cell.NodeType == NodeType.Extension)
                {
                    // Extension points start locked - only the connecting extension point will be made available by BoardPositioningManager
                    cell.SetUnlocked(false);
                    cell.SetAvailable(false);
                    cell.SetPurchased(false);
                }
                else
                {
                    // All other nodes start locked
                    cell.SetUnlocked(false);
                    cell.SetAvailable(false);
                    cell.SetPurchased(false);
                }
            }
        }
        
        /// <summary>
        /// Set the grid position of this board
        /// </summary>
        public void SetBoardGridPosition(Vector2Int gridPosition)
        {
            boardGridPosition = gridPosition;
            
            if (showDebugInfo)
                Debug.Log($"[ExtensionBoardController] {boardName} positioned at grid: {gridPosition}");
        }
        
        /// <summary>
        /// Get the grid position of this board
        /// </summary>
        public Vector2Int GetBoardGridPosition()
        {
            return boardGridPosition;
        }
        
        /// <summary>
        /// Set extension points for this board
        /// </summary>
        public void SetExtensionPoints(List<ExtensionPoint> points)
        {
            extensionPoints = new List<ExtensionPoint>(points);
            
            if (showDebugInfo)
                Debug.Log($"[ExtensionBoardController] {boardName} has {extensionPoints.Count} extension points");
        }
        
        /// <summary>
        /// Get all extension points for this board
        /// </summary>
        public List<ExtensionPoint> GetExtensionPoints()
        {
            return new List<ExtensionPoint>(extensionPoints);
        }
        
        /// <summary>
        /// Get available extension points
        /// </summary>
        public List<ExtensionPoint> GetAvailableExtensionPoints()
        {
            return extensionPoints.Where(ep => ep.CanConnect()).ToList();
        }
        
        /// <summary>
        /// Activate this board
        /// </summary>
        public void ActivateBoard()
        {
            if (isActive) return;
            
            isActive = true;
            gameObject.SetActive(true);
            
            // Unlock the board's nodes
            UnlockBoardNodes();
            
            OnBoardActivated?.Invoke(this);
            
            if (showDebugInfo)
                Debug.Log($"[ExtensionBoardController] Activated {boardName}");
        }
        
        /// <summary>
        /// Deactivate this board
        /// </summary>
        public void DeactivateBoard()
        {
            if (!isActive) return;
            
            isActive = false;
            gameObject.SetActive(false);
            
            OnBoardDeactivated?.Invoke(this);
            
            if (showDebugInfo)
                Debug.Log($"[ExtensionBoardController] Deactivated {boardName}");
        }
        
        /// <summary>
        /// Unlock nodes in this board
        /// </summary>
        private void UnlockBoardNodes()
        {
            // Find extension point cells and unlock adjacent nodes
            foreach (var kvp in cells)
            {
                CellController cell = kvp.Value;
                
                if (cell.NodeType == NodeType.Extension && cell.IsPurchased)
                {
                    // Unlock adjacent nodes from this extension point
                    UnlockAdjacentNodes(cell.GridPosition);
                }
            }
        }
        
        /// <summary>
        /// Unlock adjacent nodes when a node is purchased
        /// </summary>
        private void UnlockAdjacentNodes(Vector2Int centerPosition)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] UnlockAdjacentNodes called for position {centerPosition} on board {boardName}");
                Debug.Log($"[ExtensionBoardController] Board has {cells.Count} cells mapped");
            }
            
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                centerPosition + Vector2Int.up,    // North
                centerPosition + Vector2Int.down,  // South
                centerPosition + Vector2Int.left,  // West
                centerPosition + Vector2Int.right  // East
            };
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Checking adjacent positions: {string.Join(", ", adjacentPositions.Select(p => p.ToString()))}");
            }
            
            int unlockedCount = 0;
            foreach (Vector2Int pos in adjacentPositions)
            {
                if (cells.ContainsKey(pos))
                {
                    CellController adjacentCell = cells[pos];
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] Found adjacent cell at {pos}: Purchased={adjacentCell.IsPurchased}, Type={adjacentCell.NodeType}");
                    }
                    
                    if (!adjacentCell.IsPurchased && adjacentCell.NodeType != NodeType.Extension)
                    {
                        adjacentCell.SetAdjacent(true);
                        unlockedCount++;
                        Debug.Log($"[ExtensionBoardController] ✅ Unlocked adjacent node at {pos} on board {boardName}");
                    }
                    else
                    {
                        if (showDebugInfo)
                        {
                            Debug.Log($"[ExtensionBoardController] ⏭️ Skipped adjacent node at {pos}: already purchased or is extension point");
                        }
                    }
                }
                else
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] ❌ No cell found at adjacent position {pos} on board {boardName}");
                    }
                }
            }
            
            // After unlocking regular nodes, check if any extension points should be unlocked
            CheckAndUnlockExtensionPoints();
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] UnlockAdjacentNodes complete: {unlockedCount} nodes unlocked on board {boardName}");
            }
        }
        
        /// <summary>
        /// Handle cell click within this board
        /// </summary>
        public void OnCellClicked(Vector2Int gridPosition)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] OnCellClicked called for position {gridPosition} on board {boardName}");
            }
            
            if (!cells.ContainsKey(gridPosition))
            {
                Debug.LogWarning($"[ExtensionBoardController] Cell not found at position: {gridPosition}");
                return;
            }
            
            CellController cell = cells[gridPosition];
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Found cell at {gridPosition}: Type={cell.NodeType}, Available={cell.IsAvailable}, Unlocked={cell.IsUnlocked}, Purchased={cell.IsPurchased}");
            }
            
            // Handle different node types
            switch (cell.NodeType)
            {
                case NodeType.Extension:
                    HandleExtensionNodeClick(cell);
                    break;
                case NodeType.Travel:
                case NodeType.Small:
                    HandleTravelNodeClick(cell);
                    break;
                case NodeType.Notable:
                    HandleNotableNodeClick(cell);
                    break;
                case NodeType.Keystone:
                    HandleKeystoneNodeClick(cell);
                    break;
            }
            
            // Cell click handled
        }
        
        /// <summary>
        /// Handle extension node click - should show board selection UI
        /// </summary>
        private void HandleExtensionNodeClick(CellController cell)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] HandleExtensionNodeClick called for extension point at {cell.GridPosition} on board {boardName}");
            }
            
            // Extension points should always show board selection UI, regardless of their current state
            // Skip the CanPurchaseNode check for extension points - they should always show the board selection UI
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] ✅ Extension point at {cell.GridPosition} is clickable - will show board selection UI");
            }
            
            // Extension points should show board selection UI, not be purchased directly
            if (boardPositioningManager != null)
            {
                // Find the extension point that corresponds to this cell
                ExtensionPoint targetPoint = FindExtensionPointForCell(cell);
                
                if (targetPoint != null)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] Found extension point {targetPoint.id} for cell at {cell.GridPosition}");
                    }
                    
                    // Call the BoardPositioningManager to handle the extension point click
                    // Pass this extension board as the source board
                    boardPositioningManager.HandleExtensionPointClick(targetPoint, this.gameObject);
                }
                else
                {
                    Debug.LogWarning($"[ExtensionBoardController] No extension point found for cell at {cell.GridPosition} on board {boardName}");
                }
            }
            else
            {
                Debug.LogError($"[ExtensionBoardController] BoardPositioningManager is null - cannot handle extension point click");
            }
        }
        
        /// <summary>
        /// Find the extension point that corresponds to a specific cell
        /// </summary>
        private ExtensionPoint FindExtensionPointForCell(CellController cell)
        {
            if (boardPositioningManager == null) return null;
            
            // Get all extension points from the BoardPositioningManager
            var allExtensionPoints = boardPositioningManager.GetAllExtensionPoints();
            
            // Ensure board grid position is set
            if (boardGridPosition == Vector2Int.zero)
            {
                Debug.LogWarning($"[ExtensionBoardController] Board grid position not set for {boardName}! This may cause extension point matching issues.");
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Searching for extension point for cell at {cell.GridPosition} on board {boardName} (grid position: {boardGridPosition})");
                Debug.Log($"[ExtensionBoardController] Total extension points available: {allExtensionPoints.Count}");
                Debug.Log($"[ExtensionBoardController] Looking for extension points with ID pattern: extension_{boardGridPosition.x}_{boardGridPosition.y}_*");
            }
            
            // Look for an extension point at the same position as the cell
            // We need to match both the cell position AND the board's grid position
            foreach (var point in allExtensionPoints)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardController] Checking extension point: ID={point.id}, Position={point.position}, WorldPosition={point.worldPosition}");
                    Debug.Log($"[ExtensionBoardController] Board grid position: {boardGridPosition}, Cell position: {cell.GridPosition}");
                }
                
                // Check if this extension point matches the cell position
                if (point.position == cell.GridPosition)
                {
                    // For extension boards, we need to verify this extension point belongs to this board
                    // by checking if the extension point's world position matches this board's grid position + cell position
                    Vector2Int expectedWorldPosition = boardGridPosition + cell.GridPosition;
                    Vector2Int pointWorldPosition = new Vector2Int(point.worldPosition.x, point.worldPosition.y);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] Found potential match: Extension point {point.id}");
                        Debug.Log($"  - Cell position: {cell.GridPosition}");
                        Debug.Log($"  - Board grid position: {boardGridPosition}");
                        Debug.Log($"  - Expected world position: {expectedWorldPosition}");
                        Debug.Log($"  - Extension point world position: {pointWorldPosition}");
                    }
                    
                    // For extension boards, we need to be more specific about which extension point we want
                    // The extension point should have a world position that indicates it belongs to this board
                    // Extension point ID format: extension_{gridX}_{gridY}_{directionX}_{directionY}
                    if (point.id.StartsWith($"extension_{boardGridPosition.x}_{boardGridPosition.y}_"))
                    {
                        if (showDebugInfo)
                        {
                            Debug.Log($"[ExtensionBoardController] ✅ Found extension point {point.id} for cell {cell.GridPosition} on board {boardName}");
                        }
                        return point;
                    }
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] ❌ No extension point found at position {cell.GridPosition} on board {boardName}");
                Debug.Log($"[ExtensionBoardController] Board grid position: {boardGridPosition}");
                Debug.Log($"[ExtensionBoardController] Looking for extension points with ID pattern: extension_{boardGridPosition.x}_{boardGridPosition.y}_*");
                
                // Show all available extension points for debugging
                Debug.Log($"[ExtensionBoardController] All available extension points:");
                foreach (var point in allExtensionPoints)
                {
                    Debug.Log($"  - {point.id} at position {point.position} (world: {point.worldPosition})");
                }
                
                // Show specifically extension points for this board
                Debug.Log($"[ExtensionBoardController] Extension points for this board (grid {boardGridPosition}):");
                foreach (var point in allExtensionPoints)
                {
                    if (point.id.StartsWith($"extension_{boardGridPosition.x}_{boardGridPosition.y}_"))
                    {
                        Debug.Log($"  - MATCH: {point.id} at position {point.position} (world: {point.worldPosition})");
                    }
                }
            }
            
            return null;
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
                
                // Trigger cross-board allocation if this is an extension point
                if (cell.NodeType == NodeType.Extension)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] Extension point purchased, triggering cross-board allocation for {cell.GridPosition}");
                    }
                    TriggerCrossBoardAllocation(cell);
                }
                
                if (showDebugInfo)
                    Debug.Log($"[ExtensionBoardController] Travel node purchased at {cell.GridPosition}");
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
                
                // Trigger cross-board allocation if this is an extension point
                if (cell.NodeType == NodeType.Extension)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] Extension point purchased, triggering cross-board allocation for {cell.GridPosition}");
                    }
                    TriggerCrossBoardAllocation(cell);
                }
                
                if (showDebugInfo)
                    Debug.Log($"[ExtensionBoardController] Notable node purchased at {cell.GridPosition}");
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
                UnlockAdjacentNodes(cell.GridPosition);
                
                // Trigger cross-board allocation if this is an extension point
                if (cell.NodeType == NodeType.Extension)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] Extension point purchased, triggering cross-board allocation for {cell.GridPosition}");
                    }
                    TriggerCrossBoardAllocation(cell);
                }
                
                if (showDebugInfo)
                    Debug.Log($"[ExtensionBoardController] Keystone node purchased at {cell.GridPosition}");
            }
        }
        
        /// <summary>
        /// Trigger cross-board allocation for extension points
        /// </summary>
        private void TriggerCrossBoardAllocation(CellController cell)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Triggering cross-board allocation for extension point at {cell.GridPosition}");
            }
            
            // Find the PassiveTreeManager and trigger cross-board allocation
            PassiveTreeManager treeManager = FindObjectOfType<PassiveTreeManager>();
            if (treeManager != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardController] Found PassiveTreeManager, triggering cross-board allocation");
                }
                treeManager.TriggerCrossBoardAllocation(cell);
            }
            else
            {
                Debug.LogWarning($"[ExtensionBoardController] No PassiveTreeManager found for cross-board allocation");
            }
        }
        
        /// <summary>
        /// Check if a node has at least one adjacent purchased node (for pathing validation)
        /// </summary>
        public bool HasAdjacentPurchasedNode(Vector2Int centerPosition)
        {
            if (showDebugInfo)
                Debug.Log($"[ExtensionBoardController] HasAdjacentPurchasedNode check for position: {centerPosition} on board {boardName}");
            
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
                            Debug.Log($"[ExtensionBoardController] ✅ Found adjacent purchased node at {pos} on board {boardName}");
                        return true;
                    }
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[ExtensionBoardController] ❌ No adjacent purchased nodes found for {centerPosition} on board {boardName}");
            return false;
        }
        
        /// <summary>
        /// Check if a node can be purchased
        /// </summary>
        private bool CanPurchaseNode(CellController cell)
        {
            // Can't purchase if already purchased
            if (cell.IsPurchased) return false;
            
            // Can't purchase if not unlocked
            if (!cell.IsUnlocked) return false;
            
            // Can't purchase if not available
            if (!cell.IsAvailable) return false;
            
            // Check adjacency validation - node must have at least one purchased adjacent node
            // (except for start nodes and extension points which are always available)
            if (cell.NodeType != NodeType.Start && cell.NodeType != NodeType.Extension)
            {
                if (!HasAdjacentPurchasedNode(cell.GridPosition))
                {
                    if (showDebugInfo)
                        Debug.Log($"[ExtensionBoardController] ❌ Cannot purchase node at {cell.GridPosition}: no adjacent purchased nodes");
                    return false;
                }
                if (showDebugInfo)
                    Debug.Log($"[ExtensionBoardController] ✅ Node at {cell.GridPosition} has adjacent purchased node");
            }
            
            // TODO: Add cost validation, prerequisite checks, etc.
            
            return true;
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
        /// Get all cells in this board
        /// </summary>
        public Dictionary<Vector2Int, CellController> GetAllCells()
        {
            return new Dictionary<Vector2Int, CellController>(cells);
        }
        
        /// <summary>
        /// Get board theme
        /// </summary>
        public BoardTheme GetBoardTheme()
        {
            return boardTheme;
        }
        
        /// <summary>
        /// Get board ID
        /// </summary>
        public string GetBoardId()
        {
            return boardId;
        }
        
        /// <summary>
        /// Get board name
        /// </summary>
        public string GetBoardName()
        {
            return boardName;
        }
        
        /// <summary>
        /// Get the world grid position of this board
        /// </summary>
        public Vector2Int GetWorldGridPosition()
        {
            return boardGridPosition;
        }
        
        /// <summary>
        /// Get board description
        /// </summary>
        public string GetBoardDescription()
        {
            return boardDescription;
        }
        
        /// <summary>
        /// Check if board is active
        /// </summary>
        public bool IsActive()
        {
            return isActive;
        }
        
        /// <summary>
        /// Check if this board has a cell at the specified position
        /// </summary>
        public bool HasCellAt(Vector2Int gridPosition)
        {
            return cells.ContainsKey(gridPosition);
        }
        
        /// <summary>
        /// Get a cell at the specified position
        /// </summary>
        public CellController GetCellAt(Vector2Int gridPosition)
        {
            return cells.ContainsKey(gridPosition) ? cells[gridPosition] : null;
        }
        
        /// <summary>
        /// Manually refresh the cell mapping - useful if cells are not found initially
        /// </summary>
        [ContextMenu("Refresh Cell Mapping")]
        public void RefreshCellMapping()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Manually refreshing cell mapping for {gameObject.name}");
            }
            
            // Clear existing mapping
            cells.Clear();
            
            // Re-initialize the board
            InitializeBoard();
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Cell mapping refreshed. Total cells: {cells.Count}");
                foreach (var kvp in cells)
                {
                    Debug.Log($"[ExtensionBoardController] Mapped cell at {kvp.Key}: {kvp.Value.gameObject.name}");
                }
            }
        }
        
        /// <summary>
        /// Handle when a cell is purchased on this extension board
        /// </summary>
        public void OnCellPurchased(Vector2Int gridPosition)
        {
            Debug.Log($"[ExtensionBoardController] Cell {gridPosition} purchased on {boardName}");
            
            // Unlock adjacent nodes for further progression
            UnlockAdjacentNodesFromPurchase(gridPosition);
        }
        
        /// <summary>
        /// Unlock adjacent nodes when a cell is purchased
        /// </summary>
        private void UnlockAdjacentNodesFromPurchase(Vector2Int purchasedPosition)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] UnlockAdjacentNodesFromPurchase called for position {purchasedPosition} on board {boardName}");
                Debug.Log($"[ExtensionBoardController] Board has {cells.Count} cells mapped");
            }
            
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                purchasedPosition + Vector2Int.up,    // North
                purchasedPosition + Vector2Int.down,  // South
                purchasedPosition + Vector2Int.left,  // West
                purchasedPosition + Vector2Int.right  // East
            };
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] Checking adjacent positions: {string.Join(", ", adjacentPositions.Select(p => p.ToString()))}");
            }
            
            int unlockedCount = 0;
            foreach (Vector2Int pos in adjacentPositions)
            {
                if (cells.ContainsKey(pos))
                {
                    CellController adjacentCell = cells[pos];
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] Found adjacent cell at {pos}: Purchased={adjacentCell.IsPurchased}, Type={adjacentCell.NodeType}");
                    }
                    
                    if (!adjacentCell.IsPurchased && adjacentCell.NodeType != NodeType.Extension)
                    {
                        adjacentCell.SetAdjacent(true);
                        unlockedCount++;
                        Debug.Log($"[ExtensionBoardController] ✅ Unlocked adjacent node at {pos} on board {boardName}");
                    }
                    else
                    {
                        if (showDebugInfo)
                        {
                            Debug.Log($"[ExtensionBoardController] ⏭️ Skipped adjacent node at {pos}: already purchased or is extension point");
                        }
                    }
                }
                else
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[ExtensionBoardController] ❌ No cell found at adjacent position {pos} on board {boardName}");
                    }
                }
            }
            
            // After unlocking regular nodes, check if any extension points should be unlocked
            CheckAndUnlockExtensionPoints();
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] UnlockAdjacentNodesFromPurchase complete: {unlockedCount} nodes unlocked on board {boardName}");
            }
        }
        
        /// <summary>
        /// Check and unlock extension points that have adjacent purchased nodes
        /// </summary>
        private void CheckAndUnlockExtensionPoints()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] CheckAndUnlockExtensionPoints called on board {boardName}");
            }
            
            int unlockedExtensionPoints = 0;
            
            // Check all extension points on this board
            foreach (var kvp in cells)
            {
                CellController cell = kvp.Value;
                
                if (cell.NodeType == NodeType.Extension && !cell.IsPurchased)
                {
                    // Check if this extension point has adjacent purchased nodes
                    if (HasAdjacentPurchasedNode(cell.GridPosition))
                    {
                        // Unlock this extension point
                        cell.SetAdjacent(true);
                        unlockedExtensionPoints++;
                        
                        if (showDebugInfo)
                        {
                            Debug.Log($"[ExtensionBoardController] ✅ Unlocked extension point at {cell.GridPosition} on board {boardName} - has adjacent purchased node");
                        }
                    }
                    else
                    {
                        if (showDebugInfo)
                        {
                            Debug.Log($"[ExtensionBoardController] ⏭️ Extension point at {cell.GridPosition} on board {boardName} still locked - no adjacent purchased nodes");
                        }
                    }
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardController] CheckAndUnlockExtensionPoints complete: {unlockedExtensionPoints} extension points unlocked on board {boardName}");
            }
        }
        
        /// <summary>
        /// Debug method to show the current state of all cells on this board
        /// </summary>
        [ContextMenu("Debug Extension Board Cell States")]
        public void DebugExtensionBoardCellStates()
        {
            Debug.Log($"=== DEBUGGING EXTENSION BOARD CELL STATES ===");
            Debug.Log($"Board: {boardName}");
            Debug.Log($"Total cells mapped: {cells.Count}");
            Debug.Log($"Board size: {boardSize.x}x{boardSize.y}");
            
            foreach (var kvp in cells)
            {
                Vector2Int pos = kvp.Key;
                CellController cell = kvp.Value;
                Debug.Log($"Cell at {pos}: Type={cell.NodeType}, Available={cell.IsAvailable}, Unlocked={cell.IsUnlocked}, Purchased={cell.IsPurchased}, Name={cell.gameObject.name}");
            }
        }
        
        /// <summary>
        /// Check if board is unlocked
        /// </summary>
        public bool IsUnlocked()
        {
            return isUnlocked;
        }
        
        /// <summary>
        /// Get allocated points
        /// </summary>
        public int GetAllocatedPoints()
        {
            return allocatedPoints;
        }
        
        /// <summary>
        /// Get maximum points
        /// </summary>
        public int GetMaxPoints()
        {
            return maxPoints;
        }
        
        #region Context Menu Methods
        
        /// <summary>
        /// Debug method to show board information
        /// </summary>
        [ContextMenu("Debug Board Info")]
        public void DebugBoardInfo()
        {
            Debug.Log($"=== {boardName} DEBUG INFO ===");
            Debug.Log($"Board ID: {boardId}");
            Debug.Log($"Board Theme: {boardTheme}");
            Debug.Log($"Board Size: {boardSize}");
            Debug.Log($"Grid Position: {boardGridPosition}");
            Debug.Log($"Is Active: {isActive}");
            Debug.Log($"Is Unlocked: {isUnlocked}");
            Debug.Log($"Allocated Points: {allocatedPoints}/{maxPoints}");
            Debug.Log($"Total Cells: {cells.Count}");
            Debug.Log($"Extension Points: {extensionPoints.Count}");
            Debug.Log("=== END BOARD INFO ===");
        }
        
        /// <summary>
        /// Debug method to show all cell states
        /// </summary>
        [ContextMenu("Debug Cell States")]
        public void DebugCellStates()
        {
            Debug.Log($"=== {boardName} CELL STATES ===");
            
            foreach (var kvp in cells)
            {
                CellController cell = kvp.Value;
                Debug.Log($"Cell {cell.GridPosition}: Type={cell.NodeType}, Available={cell.IsAvailable}, Unlocked={cell.IsUnlocked}, Purchased={cell.IsPurchased}");
            }
            
            Debug.Log("=== END CELL STATES ===");
        }
        
        /// <summary>
        /// Debug method to unlock all nodes
        /// </summary>
        [ContextMenu("Unlock All Nodes")]
        public void UnlockAllNodes()
        {
            foreach (var cell in cells.Values)
            {
                cell.SetAdjacent(true);
            }
            
            Debug.Log($"[ExtensionBoardController] Unlocked all nodes in {boardName}");
        }
        
        /// <summary>
        /// Debug method to test purchasing functionality
        /// </summary>
        [ContextMenu("Test Purchasing Functionality")]
        public void TestPurchasingFunctionality()
        {
            Debug.Log($"=== TESTING PURCHASING FUNCTIONALITY FOR {boardName} ===");
            
            foreach (var kvp in cells)
            {
                CellController cell = kvp.Value;
                bool canPurchase = CanPurchaseNode(cell);
                
                Debug.Log($"Cell at {cell.GridPosition}: Type={cell.NodeType}, Available={cell.IsAvailable}, Unlocked={cell.IsUnlocked}, Purchased={cell.IsPurchased}, CanPurchase={canPurchase}");
            }
            
            Debug.Log("=== END TESTING PURCHASING FUNCTIONALITY ===");
        }
        
        #endregion
    }
}
