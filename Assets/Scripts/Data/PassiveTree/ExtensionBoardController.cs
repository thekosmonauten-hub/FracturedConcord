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
        
        // Board data
        private Dictionary<Vector2Int, CellController> cells = new Dictionary<Vector2Int, CellController>();
        private List<ExtensionPoint> extensionPoints = new List<ExtensionPoint>();
        private Vector2Int boardGridPosition = Vector2Int.zero;
        
        // Events
        public System.Action<ExtensionBoardController> OnBoardActivated;
        public System.Action<ExtensionBoardController> OnBoardDeactivated;
        
        void Start()
        {
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
            
            InitializeBoard();
        }
        
        /// <summary>
        /// Initialize board components and state
        /// </summary>
        private void InitializeBoard()
        {
            // Find all cell controllers in this board
            CellController[] foundCells = GetComponentsInChildren<CellController>();
            
            foreach (CellController cell in foundCells)
            {
                if (cell.GridPosition.x >= 0 && cell.GridPosition.x < boardSize.x &&
                    cell.GridPosition.y >= 0 && cell.GridPosition.y < boardSize.y)
                {
                    cells[cell.GridPosition] = cell;
                    // Note: CellController.Initialize() expects PassiveTreeManager, 
                    // but we'll handle cell interactions through our own methods
                    SetupCellInteraction(cell);
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
                    // Extension points are always available for connection
                    // Note: The BoardPositioningManager will override this for the connecting extension point
                    cell.SetUnlocked(true);
                    cell.SetAvailable(true);
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
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                centerPosition + Vector2Int.up,    // North
                centerPosition + Vector2Int.down,  // South
                centerPosition + Vector2Int.left,  // West
                centerPosition + Vector2Int.right  // East
            };
            
            foreach (Vector2Int pos in adjacentPositions)
            {
                if (cells.ContainsKey(pos))
                {
                    CellController adjacentCell = cells[pos];
                    
                    if (!adjacentCell.IsUnlocked && !adjacentCell.IsPurchased)
                    {
                        adjacentCell.SetUnlocked(true);
                        adjacentCell.SetAvailable(true);
                        
                        if (showDebugInfo)
                            Debug.Log($"[ExtensionBoardController] Unlocked node at {pos} in {boardName}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Handle cell click within this board
        /// </summary>
        public void OnCellClicked(Vector2Int gridPosition)
        {
            if (!cells.ContainsKey(gridPosition))
            {
                Debug.LogWarning($"[ExtensionBoardController] Cell not found at position: {gridPosition}");
                return;
            }
            
            CellController cell = cells[gridPosition];
            
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
        /// Handle extension node click
        /// </summary>
        private void HandleExtensionNodeClick(CellController cell)
        {
            if (CanPurchaseNode(cell))
            {
                cell.SetPurchased(true);
                UnlockAdjacentNodes(cell.GridPosition);
                
                if (showDebugInfo)
                    Debug.Log($"[ExtensionBoardController] Extension node purchased at {cell.GridPosition}");
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
                
                if (showDebugInfo)
                    Debug.Log($"[ExtensionBoardController] Keystone node purchased at {cell.GridPosition}");
            }
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
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                purchasedPosition + Vector2Int.up,    // North
                purchasedPosition + Vector2Int.down,  // South
                purchasedPosition + Vector2Int.left,  // West
                purchasedPosition + Vector2Int.right  // East
            };
            
            foreach (Vector2Int pos in adjacentPositions)
            {
                if (cells.ContainsKey(pos))
                {
                    CellController adjacentCell = cells[pos];
                    if (!adjacentCell.IsPurchased && adjacentCell.NodeType != NodeType.Extension)
                    {
                        adjacentCell.SetUnlocked(true);
                        adjacentCell.SetAvailable(true);
                        Debug.Log($"[ExtensionBoardController] Unlocked adjacent node at {pos}");
                    }
                }
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
                cell.SetUnlocked(true);
                cell.SetAvailable(true);
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
