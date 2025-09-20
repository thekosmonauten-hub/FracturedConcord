using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Manages the positioning and alignment of extension boards relative to the core board
    /// Uses a hierarchical grid container system for perfect orthographic alignment
    /// </summary>
    public class BoardPositioningManager : MonoBehaviour
    {
        [Header("Board Container Settings")]
        [SerializeField] private Transform boardsContainer;
        [SerializeField] private GameObject coreBoardPrefab;
        [SerializeField] private GameObject extensionBoardPrefab;
        [SerializeField] private float boardSpacing = 7f; // Distance between board centers (7x7 grid + spacing)
        
        [Header("Grid Settings")]
        [SerializeField] private Vector2Int gridCellSize = new Vector2Int(900, 900); // Unity Grid cell size (128px × 7 cells = 896px + spacing = 900px per board)
        [SerializeField] private Vector2Int boardSize = new Vector2Int(7, 7); // 7x7 board dimensions
        [SerializeField] private int cellPixelSize = 128; // Individual cell size in pixels
        
        [Header("Core Board Position")]
        [SerializeField] private Vector3 coreBoardWorldPosition = Vector3.zero; // Actual world position of core board center
        
        [Header("Extension Board Positioning")]
        [SerializeField] private Vector3 extensionBoardOffset = new Vector3(-768, -768, 0); // Manual offset to adjust extension board positioning
        
        /// <summary>
        /// Calculate the board size in pixels based on cell size and board dimensions
        /// </summary>
        private int GetBoardPixelSize()
        {
            return cellPixelSize * boardSize.x; // Assuming square boards
        }
        
        [Header("Camera Settings")]
        [SerializeField] private bool autoCenterCameraOnCoreBoard = true;
        [SerializeField] private SimpleCameraController cameraController;
        
        [Header("Board Selection UI")]
        [SerializeField] private BoardSelectionUI boardSelectionUI;
        [SerializeField] private bool useBoardSelection = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool showGridGizmos = true;
        
        // Board tracking
        private Dictionary<Vector2Int, GameObject> positionedBoards = new Dictionary<Vector2Int, GameObject>();
        private GameObject coreBoard;
        private Vector2Int coreBoardGridPosition = Vector2Int.zero;
        
        // Extension point integration
        private List<ExtensionPoint> availableExtensionPoints = new List<ExtensionPoint>();
        
        void Start()
        {
            InitializeBoardContainer();
            SetupCoreBoard();
        }
        
        /// <summary>
        /// Initialize the boards container with proper grid setup
        /// </summary>
        private void InitializeBoardContainer()
        {
            if (boardsContainer == null)
            {
                // Create boards container if it doesn't exist
                GameObject container = new GameObject("BoardsContainer");
                boardsContainer = container.transform;
                boardsContainer.SetParent(transform);
                
                // Add Grid component for perfect alignment
                var grid = container.AddComponent<Grid>();
                grid.cellSize = new Vector3(gridCellSize.x, gridCellSize.y, 1f);
                grid.cellGap = Vector3.zero;
                grid.cellLayout = GridLayout.CellLayout.Rectangle;
                grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
                
                if (showDebugInfo)
                    Debug.Log($"[BoardPositioningManager] Created boards container with grid cell size: {gridCellSize}");
            }
        }
        
        /// <summary>
        /// Setup the core board at the center position (0,0)
        /// </summary>
        private void SetupCoreBoard()
        {
            if (coreBoardPrefab == null)
            {
                Debug.LogError("[BoardPositioningManager] Core board prefab not assigned!");
                return;
            }
            
            // Create core board at grid position (0,0)
            // Don't set parent during instantiation to avoid scaling issues
            coreBoard = Instantiate(coreBoardPrefab);
            coreBoard.name = "CoreBoard";
            
            // Position using grid system (this will set the parent correctly)
            PositionBoardAtGrid(coreBoard, coreBoardGridPosition);
            positionedBoards[coreBoardGridPosition] = coreBoard;
            
            // Fix scaling issues in the core board cells AFTER parenting
            // This ensures the cells are properly scaled regardless of prefab settings
            FixBoardCellScaling(coreBoard);
            
            // Detect and store the actual world position of the core board
            DetectCoreBoardWorldPosition();
            
            // Setup extension points for the core board
            SetupCoreBoardExtensionPoints();
            
            // Center camera on the core board's center cell (3,3)
            if (autoCenterCameraOnCoreBoard)
            {
                CenterCameraOnCoreBoard();
            }
            
            if (showDebugInfo)
                Debug.Log($"[BoardPositioningManager] Core board created at grid position: {coreBoardGridPosition}, world position: {coreBoardWorldPosition}");
        }
        
        /// <summary>
        /// Detect the actual world position of the core board
        /// </summary>
        private void DetectCoreBoardWorldPosition()
        {
            if (coreBoard == null) return;
            
            // Get the center cell of the core board (3,3 in a 7x7 grid)
            Vector2Int centerCellPosition = new Vector2Int(boardSize.x / 2, boardSize.y / 2);
            
            // Find the center cell and get its world position
            CellController[] cells = coreBoard.GetComponentsInChildren<CellController>();
            foreach (CellController cell in cells)
            {
                if (cell.GridPosition == centerCellPosition)
                {
                    coreBoardWorldPosition = cell.transform.position;
                    if (showDebugInfo)
                        Debug.Log($"[BoardPositioningManager] Detected core board world position: {coreBoardWorldPosition} (from center cell at {centerCellPosition})");
                    break;
                }
            }
        }
        
        /// <summary>
        /// Fix scaling issues in board cells and set proper BoxCollider size
        /// </summary>
        private void FixBoardCellScaling(GameObject board)
        {
            if (board == null) 
            {
                Debug.LogWarning("[BoardPositioningManager] Cannot fix scaling - board is null");
                return;
            }
            
            // Find all child objects that might be cells
            Transform[] children = board.GetComponentsInChildren<Transform>();
            int fixedCount = 0;
            int colliderFixedCount = 0;
            
            if (showDebugInfo)
                Debug.Log($"[BoardPositioningManager] Fixing scaling for {board.name} - found {children.Length} children");
            
            foreach (Transform child in children)
            {
                // Skip the board itself
                if (child == board.transform) continue;
                
                // Check if this looks like a cell (has CellController or is named like a cell)
                if (child.GetComponent<CellController>() != null || child.name.StartsWith("Cell_"))
                {
                    // Always fix scaling to 1.0 (force it regardless of current value)
                    Vector3 oldScale = child.localScale;
                    child.localScale = Vector3.one;
                    fixedCount++;
                    
                    if (showDebugInfo)
                        Debug.Log($"[BoardPositioningManager] Fixed scaling for cell: {child.name} (was {oldScale}, now {child.localScale})");
                    
                    // Always fix BoxCollider size to 1.72 on both X/Y axes
                    var boxCollider = child.GetComponent<BoxCollider2D>();
                    if (boxCollider != null)
                    {
                        Vector2 oldSize = boxCollider.size;
                        boxCollider.size = new Vector2(1.72f, 1.72f);
                        colliderFixedCount++;
                        
                        if (showDebugInfo)
                            Debug.Log($"[BoardPositioningManager] Fixed BoxCollider size for cell: {child.name} (was {oldSize}, now {boxCollider.size})");
                    }
                    else
                    {
                        if (showDebugInfo)
                            Debug.LogWarning($"[BoardPositioningManager] No BoxCollider2D found on cell: {child.name}");
                    }
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[BoardPositioningManager] Fixed {fixedCount} cell scales and {colliderFixedCount} collider sizes in {board.name}");
        }
        
        /// <summary>
        /// Center camera on the core board's center cell (3,3)
        /// </summary>
        private void CenterCameraOnCoreBoard()
        {
            if (cameraController == null)
            {
                // Try to find the camera controller automatically
                cameraController = FindObjectOfType<SimpleCameraController>();
                if (cameraController == null)
                {
                    if (showDebugInfo)
                        Debug.LogWarning("[BoardPositioningManager] No SimpleCameraController found - cannot center camera");
                    return;
                }
            }
            
            if (coreBoard == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning("[BoardPositioningManager] No core board found - cannot center camera");
                return;
            }
            
            // Calculate the center cell position (3,3 in a 7x7 grid)
            Vector2Int centerCellPosition = new Vector2Int(3, 3);
            
            // Convert grid position to world position
            Vector3 centerCellWorldPosition = boardsContainer.TransformPoint(new Vector3(centerCellPosition.x, centerCellPosition.y, 0));
            
            // Focus camera on the center cell
            cameraController.FocusOnPosition(centerCellWorldPosition);
            
            if (showDebugInfo)
                Debug.Log($"[BoardPositioningManager] Centered camera on core board center cell (3,3) at world position: {centerCellWorldPosition}");
        }
        
        /// <summary>
        /// Setup extension points for the core board
        /// </summary>
        private void SetupCoreBoardExtensionPoints()
        {
            if (coreBoard == null) return;
            
            // Create extension points at the edges of the core board
            // Note: Extension board creation coordinates are swapped for proper positioning
            Vector2Int[] extensionDirections = new Vector2Int[]
            {
                Vector2Int.up,    // North
                Vector2Int.down,  // South
                Vector2Int.right, // East
                Vector2Int.left   // West
            };
            
            foreach (Vector2Int direction in extensionDirections)
            {
                // Calculate extension board grid position with swapped coordinates
                Vector2Int extensionGridPosition;
                Vector2Int edgeCellPosition = GetBoardEdgePosition(direction);
                
                // Swap the extension board creation coordinates
                if (direction == Vector2Int.up) // North extension point
                {
                    extensionGridPosition = coreBoardGridPosition + Vector2Int.right; // Create at East position
                }
                else if (direction == Vector2Int.down) // South extension point
                {
                    extensionGridPosition = coreBoardGridPosition + Vector2Int.left; // Create at West position
                }
                else if (direction == Vector2Int.right) // East extension point
                {
                    extensionGridPosition = coreBoardGridPosition + Vector2Int.up; // Create at North position
                }
                else // West extension point
                {
                    extensionGridPosition = coreBoardGridPosition + Vector2Int.down; // Create at South position
                }
                
                // Create extension point
                ExtensionPoint extensionPoint = new ExtensionPoint
                {
                    id = $"core_extension_{direction.x}_{direction.y}",
                    position = edgeCellPosition,
                    worldPosition = new Vector3Int(extensionGridPosition.x, extensionGridPosition.y, 0),
                    availableBoards = new List<string>(), // Allow all board types
                    maxConnections = 1,
                    currentConnections = 0
                };
                
                availableExtensionPoints.Add(extensionPoint);
                
                // Mark the corresponding cell as an extension point
                MarkCellAsExtensionPoint(edgeCellPosition);
                
            if (showDebugInfo)
                Debug.Log($"[BoardPositioningManager] Created extension point: {extensionPoint.id} at cell position {edgeCellPosition}, grid position {extensionGridPosition} (swapped coordinates)");
            }
        }
        
        /// <summary>
        /// Mark a cell at the specified position as an extension point
        /// </summary>
        private void MarkCellAsExtensionPoint(Vector2Int cellPosition)
        {
            if (coreBoard == null) return;
            
            // Find the cell at the specified position
            CellController[] cells = coreBoard.GetComponentsInChildren<CellController>();
            foreach (CellController cell in cells)
            {
                if (cell.GridPosition == cellPosition)
                {
                    cell.SetAsExtensionPoint(true);
                    if (showDebugInfo)
                        Debug.Log($"[BoardPositioningManager] Marked cell at {cellPosition} as extension point");
                    break;
                }
            }
        }
        
        /// <summary>
        /// Get the edge position of a board for extension point placement
        /// This returns the actual cell position on the board edge where extension points are located
        /// </summary>
        private Vector2Int GetBoardEdgePosition(Vector2Int direction)
        {
            // Calculate the actual edge cell position based on direction
            Vector2Int edgePosition = Vector2Int.zero;
            
            if (direction == Vector2Int.up) // North
                edgePosition = new Vector2Int(boardSize.x / 2, boardSize.y - 1); // Top edge, center X
            else if (direction == Vector2Int.down) // South
                edgePosition = new Vector2Int(boardSize.x / 2, 0); // Bottom edge, center X
            else if (direction == Vector2Int.right) // East
                edgePosition = new Vector2Int(boardSize.x - 1, boardSize.y / 2); // Right edge, center Y
            else if (direction == Vector2Int.left) // West
                edgePosition = new Vector2Int(0, boardSize.y / 2); // Left edge, center Y
            
            return edgePosition;
        }
        
        /// <summary>
        /// Position a board at a specific grid position
        /// </summary>
        private void PositionBoardAtGrid(GameObject board, Vector2Int gridPosition)
        {
            if (board == null || boardsContainer == null) return;
            
            // Store original scale to preserve it
            Vector3 originalScale = board.transform.localScale;
            
            // Calculate world position using Grid component's cell size
            Vector3 worldPosition = CalculateGridWorldPosition(gridPosition);
            
            // Set parent and position
            board.transform.SetParent(boardsContainer);
            board.transform.position = worldPosition;
            
            // Restore original scale to prevent scaling issues
            board.transform.localScale = originalScale;
            
            if (showDebugInfo)
                Debug.Log($"[BoardPositioningManager] Positioned {board.name} at grid position: {gridPosition}, world position: {worldPosition} (scale: {originalScale})");
        }
        
        /// <summary>
        /// Calculate world position from grid position using Grid component settings
        /// </summary>
        private Vector3 CalculateGridWorldPosition(Vector2Int gridPosition)
        {
            if (boardsContainer == null) return Vector3.zero;
            
            // Get the Grid component
            var grid = boardsContainer.GetComponent<Grid>();
            if (grid != null)
            {
                // Use Grid component's cell size and gap
                Vector3 cellSize = grid.cellSize;
                Vector3 cellGap = grid.cellGap;
                
                // Calculate world position relative to core board with configurable offset
                Vector3 worldPos = new Vector3(
                    gridPosition.x * (cellSize.x + cellGap.x) + extensionBoardOffset.x,
                    gridPosition.y * (cellSize.y + cellGap.y) + extensionBoardOffset.y,
                    extensionBoardOffset.z
                );
                
                // Use the core board's actual world position as the base, then add grid offset
                Vector3 finalPosition = coreBoardWorldPosition + worldPos;
                
                return finalPosition;
            }
            else
            {
                // Fallback: use gridCellSize directly with configurable offset
                Vector3 worldPos = new Vector3(
                    gridPosition.x * gridCellSize.x + extensionBoardOffset.x,
                    gridPosition.y * gridCellSize.y + extensionBoardOffset.y,
                    extensionBoardOffset.z
                );
                
                // Use the core board's actual world position as the base, then add grid offset
                Vector3 finalPosition = coreBoardWorldPosition + worldPos;
                
                return finalPosition;
            }
        }
        
        /// <summary>
        /// Handle extension point click - either show board selection UI or spawn board directly
        /// </summary>
        public void HandleExtensionPointClick(ExtensionPoint extensionPoint)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] HandleExtensionPointClick called for extension point at {extensionPoint.position}");
                Debug.Log($"[BoardPositioningManager] useBoardSelection: {useBoardSelection}");
                Debug.Log($"[BoardPositioningManager] boardSelectionUI is null: {boardSelectionUI == null}");
                if (boardSelectionUI != null)
                {
                    Debug.Log($"[BoardPositioningManager] boardSelectionUI GameObject active: {boardSelectionUI.gameObject.activeInHierarchy}");
                    Debug.Log($"[BoardPositioningManager] boardSelectionUI GameObject name: {boardSelectionUI.gameObject.name}");
                }
            }
            
            if (useBoardSelection && boardSelectionUI != null)
            {
                // Calculate grid position for the extension board
                Vector2Int extensionGridPosition = new Vector2Int(extensionPoint.worldPosition.x, extensionPoint.worldPosition.y);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] About to call ShowBoardSelection for extension point at {extensionPoint.position}");
                }
                
                // Show board selection UI
                boardSelectionUI.ShowBoardSelection(extensionPoint, extensionGridPosition);
                
                // Subscribe to board selection events
                boardSelectionUI.OnBoardSelected += (boardData) => OnBoardSelected(extensionPoint, boardData);
                boardSelectionUI.OnSelectionCancelled += OnBoardSelectionCancelled;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] ShowBoardSelection called and events subscribed for extension point at {extensionPoint.position}");
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] Falling back to direct spawn - useBoardSelection: {useBoardSelection}, boardSelectionUI null: {boardSelectionUI == null}");
                }
                
                // WORKAROUND: Mark the extension point as purchased before spawning the board
                MarkExtensionPointAsPurchased(extensionPoint);
                
                // Direct spawn with default board type
                SpawnExtensionBoard(extensionPoint, "Default");
            }
        }
        
        /// <summary>
        /// Handle board selection from UI
        /// </summary>
        private void OnBoardSelected(ExtensionPoint extensionPoint, BoardData boardData)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Board selected: {boardData.BoardName}");
            }
            
            // WORKAROUND: Mark the extension point as purchased before spawning the board
            // This prevents the extension point from being unselected
            MarkExtensionPointAsPurchased(extensionPoint);
            
            // Unsubscribe from events
            if (boardSelectionUI != null)
            {
                boardSelectionUI.OnBoardSelected -= (boardData) => OnBoardSelected(extensionPoint, boardData);
                boardSelectionUI.OnSelectionCancelled -= OnBoardSelectionCancelled;
            }
            
            // Spawn the selected board
            SpawnExtensionBoard(extensionPoint, boardData);
        }
        
        /// <summary>
        /// Handle board selection cancellation
        /// </summary>
        private void OnBoardSelectionCancelled()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Board selection cancelled");
            }
        }
        
        /// <summary>
        /// WORKAROUND: Mark an extension point as purchased to prevent it from being unselected
        /// </summary>
        private void MarkExtensionPointAsPurchased(ExtensionPoint extensionPoint)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Marking extension point as purchased: {extensionPoint.id} at {extensionPoint.position}");
            }
            
            // Find the CellController for this extension point
            CellController cellController = null;
            
            // Search through all positioned boards to find the cell
            foreach (var kvp in positionedBoards)
            {
                GameObject board = kvp.Value;
                CellController[] cells = board.GetComponentsInChildren<CellController>();
                
                foreach (CellController cell in cells)
                {
                    if (cell.GridPosition == extensionPoint.position)
                    {
                        cellController = cell;
                        break;
                    }
                }
                
                if (cellController != null) break;
            }
            
            if (cellController != null)
            {
                cellController.SetPurchased(true);
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] ✅ Extension point cell marked as purchased: {cellController.GridPosition}");
                }
            }
            else
            {
                Debug.LogWarning($"[BoardPositioningManager] ❌ Could not find CellController for extension point at {extensionPoint.position}");
            }
        }
        
        /// <summary>
        /// Spawn an extension board at a specific extension point using BoardData
        /// </summary>
        public bool SpawnExtensionBoard(ExtensionPoint extensionPoint, BoardData boardData)
        {
            if (boardData == null)
            {
                Debug.LogError("[BoardPositioningManager] BoardData is null!");
                return false;
            }
            
            if (!boardData.IsValid())
            {
                Debug.LogError($"[BoardPositioningManager] BoardData '{boardData.BoardName}' is not valid!");
                return false;
            }
            
            Debug.Log($"[BoardPositioningManager] Attempting to spawn board: '{boardData.BoardName}'");
            Debug.Log($"[BoardPositioningManager] Extension point availableBoards: [{string.Join(", ", extensionPoint.availableBoards)}]");
            Debug.Log($"[BoardPositioningManager] CanConnectBoard result: {extensionPoint.CanConnectBoard(boardData.BoardName)}");
            
            return SpawnExtensionBoard(extensionPoint, boardData.BoardName, boardData.JsonDataAsset);
        }
        
        /// <summary>
        /// Spawn an extension board at a specific extension point with JSON data
        /// </summary>
        public bool SpawnExtensionBoard(ExtensionPoint extensionPoint, string boardType, TextAsset jsonDataAsset)
        {
            if (extensionBoardPrefab == null)
            {
                Debug.LogError("[BoardPositioningManager] Extension board prefab not assigned!");
                return false;
            }
            
            if (!extensionPoint.CanConnectBoard(boardType))
            {
                Debug.LogWarning($"[BoardPositioningManager] Cannot connect board type '{boardType}' to extension point '{extensionPoint.id}'");
                Debug.LogWarning($"[BoardPositioningManager] Available boards: [{string.Join(", ", extensionPoint.availableBoards)}]");
                Debug.LogWarning($"[BoardPositioningManager] Current connections: {extensionPoint.currentConnections}/{extensionPoint.maxConnections}");
                return false;
            }
            
            Debug.Log($"[BoardPositioningManager] ✅ CanConnectBoard validation passed for '{boardType}'");
            
            // Calculate grid position for the extension board
            Vector2Int extensionGridPosition = new Vector2Int(extensionPoint.worldPosition.x, extensionPoint.worldPosition.y);
            
            // Check if position is already occupied
            if (positionedBoards.ContainsKey(extensionGridPosition))
            {
                Debug.LogWarning($"[BoardPositioningManager] Grid position {extensionGridPosition} is already occupied!");
                return false;
            }
            
            // Create extension board
            // Don't set parent during instantiation to avoid scaling issues
            GameObject extensionBoard = Instantiate(extensionBoardPrefab);
            extensionBoard.name = $"ExtensionBoard_{boardType}_{extensionGridPosition.x}_{extensionGridPosition.y}";
            
            // Position the board (this will set the parent correctly)
            PositionBoardAtGrid(extensionBoard, extensionGridPosition);
            positionedBoards[extensionGridPosition] = extensionBoard;
            
            // Initialize all cells on the new board with PassiveTreeManager reference
            InitializeBoardCells(extensionBoard);
            
            // Load JSON data if provided
            if (jsonDataAsset != null)
            {
                LoadJsonDataToBoard(extensionBoard, jsonDataAsset);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] JSON data loaded, checking extension point cells...");
                    CellController[] allCells = extensionBoard.GetComponentsInChildren<CellController>();
                    foreach (CellController cell in allCells)
                    {
                        if (cell.NodeType == NodeType.Extension)
                        {
                            Debug.Log($"[BoardPositioningManager] Found extension point cell at {cell.GridPosition} with type {cell.NodeType}");
                        }
                    }
                }
            }
            
            // Update extension point
            extensionPoint.AddConnection();
            
            // Setup extension points for the new board
            SetupExtensionBoardExtensionPoints(extensionBoard, extensionGridPosition);
            
            // Wait one frame to ensure extension points are fully added to the list
            StartCoroutine(AllocateCorrespondingExtensionPointDelayed(extensionPoint, extensionBoard, extensionGridPosition));
            
            if (showDebugInfo)
            {
                Vector3 worldPos = CalculateGridWorldPosition(extensionGridPosition);
                Debug.Log($"[BoardPositioningManager] Spawned extension board '{boardType}' at grid position: {extensionGridPosition}, world position: {worldPos}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Spawn an extension board at a specific extension point (legacy method)
        /// </summary>
        public bool SpawnExtensionBoard(ExtensionPoint extensionPoint, string boardType)
        {
            if (extensionBoardPrefab == null)
            {
                Debug.LogError("[BoardPositioningManager] Extension board prefab not assigned!");
                return false;
            }
            
            if (!extensionPoint.CanConnectBoard(boardType))
            {
                Debug.LogWarning($"[BoardPositioningManager] Cannot connect board type '{boardType}' to extension point '{extensionPoint.id}'");
                return false;
            }
            
            // Calculate grid position for the extension board
            Vector2Int extensionGridPosition = new Vector2Int(extensionPoint.worldPosition.x, extensionPoint.worldPosition.y);
            
            // Check if position is already occupied
            if (positionedBoards.ContainsKey(extensionGridPosition))
            {
                Debug.LogWarning($"[BoardPositioningManager] Grid position {extensionGridPosition} is already occupied!");
                return false;
            }
            
            // Create extension board
            // Don't set parent during instantiation to avoid scaling issues
            GameObject extensionBoard = Instantiate(extensionBoardPrefab);
            extensionBoard.name = $"ExtensionBoard_{boardType}_{extensionGridPosition.x}_{extensionGridPosition.y}";
            
            // Position the board (this will set the parent correctly)
            PositionBoardAtGrid(extensionBoard, extensionGridPosition);
            positionedBoards[extensionGridPosition] = extensionBoard;
            
            // Initialize all cells on the new board with PassiveTreeManager reference
            InitializeBoardCells(extensionBoard);
            
            // Update extension point
            extensionPoint.AddConnection();
            
            // Setup extension points for the new board
            SetupExtensionBoardExtensionPoints(extensionBoard, extensionGridPosition);
            
            // Wait one frame to ensure extension points are fully added to the list
            StartCoroutine(AllocateCorrespondingExtensionPointDelayed(extensionPoint, extensionBoard, extensionGridPosition));
            
            if (showDebugInfo)
            {
                Vector3 worldPos = CalculateGridWorldPosition(extensionGridPosition);
                Debug.Log($"[BoardPositioningManager] Spawned extension board '{boardType}' at grid position: {extensionGridPosition}, world position: {worldPos}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Setup extension points for a newly created extension board
        /// </summary>
        private void SetupExtensionBoardExtensionPoints(GameObject board, Vector2Int gridPosition)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] === SETTING UP EXTENSION POINTS FOR BOARD AT {gridPosition} ===");
                Debug.Log($"[BoardPositioningManager] Current availableExtensionPoints count: {availableExtensionPoints.Count}");
            }
            
            // Create extension points for the new board (excluding the direction it came from)
            Vector2Int[] extensionDirections = new Vector2Int[]
            {
                Vector2Int.up,    // North
                Vector2Int.down,  // South
                Vector2Int.right, // East
                Vector2Int.left   // West
            };
            
            // Find the direction this board came from (towards core)
            Vector2Int fromDirection = coreBoardGridPosition - gridPosition;
            // Normalize the direction to get the unit direction
            Vector2Int fromDirectionNormalized = new Vector2Int(
                fromDirection.x != 0 ? fromDirection.x / Mathf.Abs(fromDirection.x) : 0,
                fromDirection.y != 0 ? fromDirection.y / Mathf.Abs(fromDirection.y) : 0
            );
            
            if (showDebugInfo)
                Debug.Log($"[BoardPositioningManager] Setting up extension points for board at {gridPosition}, came from direction {fromDirectionNormalized}");
            
            foreach (Vector2Int direction in extensionDirections)
            {
                // Skip the direction this board came from
                if (direction == fromDirectionNormalized) 
                {
                    if (showDebugInfo)
                        Debug.Log($"[BoardPositioningManager] Skipping direction {direction} (came from this direction)");
                    continue;
                }
                
                // Calculate extension board grid position - use normal direction (no coordinate swapping for extension boards)
                Vector2Int extensionGridPosition = gridPosition + direction;
                
                // Check if this position is already occupied
                if (positionedBoards.ContainsKey(extensionGridPosition)) 
                {
                    if (showDebugInfo)
                        Debug.Log($"[BoardPositioningManager] Skipping extension point at {direction} - position {extensionGridPosition} already occupied");
                    continue;
                }
                
                // Get the cell position for this extension point
                Vector2Int cellPosition = GetBoardEdgePosition(direction);
                CellController[] allCells = board.GetComponentsInChildren<CellController>();
                CellController extensionCell = null;
                
                foreach (CellController c in allCells)
                {
                    if (c.GridPosition == cellPosition)
                    {
                        extensionCell = c;
                        break;
                    }
                }
                
                if (extensionCell != null)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[BoardPositioningManager] Found cell at {cellPosition} with current type: {extensionCell.NodeType}");
                    }
                    
                    // ALWAYS set extension point cells to NodeType.Extension
                    // These positions (3,0), (3,6), (0,3), (6,3) are always extension points
                    extensionCell.SetNodeType(NodeType.Extension);
                    
                    // Create extension point
                    ExtensionPoint extensionPoint = new ExtensionPoint
                    {
                        id = $"extension_{gridPosition.x}_{gridPosition.y}_{direction.x}_{direction.y}",
                        position = cellPosition,
                        worldPosition = new Vector3Int(extensionGridPosition.x, extensionGridPosition.y, 0),
                        availableBoards = new List<string>(), // Allow all board types
                        maxConnections = 1,
                        currentConnections = 0
                    };
                    
                    availableExtensionPoints.Add(extensionPoint);
                    
                    if (showDebugInfo)
                        Debug.Log($"[BoardPositioningManager] Created extension point: {extensionPoint.id} at cell {extensionPoint.position} (forced to NodeType.Extension)");
                }
                else
                {
                    if (showDebugInfo)
                        Debug.Log($"[BoardPositioningManager] No cell found at position {cellPosition} for extension point");
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] === EXTENSION POINT SETUP COMPLETE ===");
                Debug.Log($"[BoardPositioningManager] Final availableExtensionPoints count: {availableExtensionPoints.Count}");
                Debug.Log($"[BoardPositioningManager] Extension points for this board:");
                foreach (ExtensionPoint ep in availableExtensionPoints)
                {
                    if (ep.id.Contains($"extension_{gridPosition.x}_{gridPosition.y}"))
                    {
                        Debug.Log($"  - {ep.id} at cell {ep.position} - Can connect: {ep.CanConnect()}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Automatically allocate the corresponding extension point on a newly created extension board
        /// Based on specific cell coordinate mapping for clean path maintenance
        /// </summary>
        private void AllocateCorrespondingExtensionPoint(ExtensionPoint sourceExtensionPoint, GameObject newBoard, Vector2Int newBoardGridPosition)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Starting automatic allocation for board at {newBoardGridPosition}");
                Debug.Log($"[BoardPositioningManager] Source extension point: {sourceExtensionPoint.id} at cell {sourceExtensionPoint.position}");
            }
            
            // Determine the corresponding cell position based on the source extension point's cell position
            Vector2Int correspondingCellPosition = GetCorrespondingCellPosition(sourceExtensionPoint.position);
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Source cell position: {sourceExtensionPoint.position}");
                Debug.Log($"[BoardPositioningManager] Looking for corresponding cell position: {correspondingCellPosition}");
                Debug.Log($"[BoardPositioningManager] Available extension points count: {availableExtensionPoints.Count}");
            }
            
            // Find the extension point on the new board that matches the corresponding cell position
            // Use more flexible matching to handle potential timing issues
            ExtensionPoint correspondingExtensionPoint = null;
            
            // First try exact match
            correspondingExtensionPoint = availableExtensionPoints.FirstOrDefault(ep => 
                ep.id.Contains($"extension_{newBoardGridPosition.x}_{newBoardGridPosition.y}") &&
                ep.position == correspondingCellPosition
            );
            
            // If not found, try to find any extension point on this board at the corresponding cell
            if (correspondingExtensionPoint == null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] Exact match failed, trying flexible match...");
                }
                
                // Get all extension points for this board
                var boardExtensionPoints = availableExtensionPoints.Where(ep => 
                    ep.id.Contains($"extension_{newBoardGridPosition.x}_{newBoardGridPosition.y}")).ToList();
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] Found {boardExtensionPoints.Count} extension points for board at {newBoardGridPosition}");
                    foreach (ExtensionPoint ep in boardExtensionPoints)
                    {
                        Debug.Log($"  - {ep.id} at cell {ep.position}");
                    }
                }
                
                // Look for extension point at the corresponding cell position
                correspondingExtensionPoint = boardExtensionPoints.FirstOrDefault(ep => 
                    ep.position == correspondingCellPosition);
            }
            
            if (correspondingExtensionPoint != null)
            {
                // Automatically allocate the corresponding extension point
                correspondingExtensionPoint.AddConnection();
                
                // Also purchase the corresponding cell on the new board
                PurchaseCorrespondingCell(newBoard, correspondingCellPosition);
                
                // Unlock adjacent nodes for orthographic interaction
                UnlockAdjacentNodesFromExtensionPoint(newBoard, correspondingCellPosition);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] ✅ Automatically allocated extension point '{correspondingExtensionPoint.id}' at cell {correspondingExtensionPoint.position} on new board at {newBoardGridPosition}");
                    Debug.Log($"[BoardPositioningManager] ✅ Automatically configured corresponding cell at {correspondingCellPosition} as ALLOCATED extension point");
                    Debug.Log($"[BoardPositioningManager] ✅ Unlocked adjacent nodes for orthographic interaction");
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[BoardPositioningManager] ❌ Could not find corresponding extension point at cell {correspondingCellPosition} on board at {newBoardGridPosition}");
                    
                    // Debug: Show all available extension points for this board
                    Debug.Log($"[BoardPositioningManager] Available extension points for board at {newBoardGridPosition}:");
                    foreach (ExtensionPoint ep in availableExtensionPoints.Where(ep => ep.id.Contains($"extension_{newBoardGridPosition.x}_{newBoardGridPosition.y}")))
                    {
                        Debug.Log($"  - {ep.id} at cell {ep.position}");
                    }
                    
                    // Also show all extension points to see what's available
                    Debug.Log($"[BoardPositioningManager] All available extension points:");
                    foreach (ExtensionPoint ep in availableExtensionPoints)
                    {
                        Debug.Log($"  - {ep.id} at cell {ep.position} (world: {ep.worldPosition})");
                    }
                }
            }
        }
        
        /// <summary>
        /// Load JSON data into a board using JsonBoardDataManager
        /// </summary>
        private void LoadJsonDataToBoard(GameObject board, TextAsset jsonDataAsset)
        {
            if (board == null || jsonDataAsset == null) return;
            
            // Find or add JsonBoardDataManager to the board
            JsonBoardDataManager jsonManager = board.GetComponent<JsonBoardDataManager>();
            if (jsonManager == null)
            {
                jsonManager = board.AddComponent<JsonBoardDataManager>();
            }
            
            // Set the JSON data asset
            var jsonManagerField = typeof(JsonBoardDataManager).GetField("boardDataJson", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (jsonManagerField != null)
            {
                jsonManagerField.SetValue(jsonManager, jsonDataAsset);
            }
            
            // Configure CellJsonData components on all cells
            CellJsonData[] cellJsonDataComponents = board.GetComponentsInChildren<CellJsonData>();
            foreach (CellJsonData cellJsonData in cellJsonDataComponents)
            {
                cellJsonData.SetJsonFile(jsonDataAsset);
                cellJsonData.SetBoardId(jsonDataAsset.name); // Use JSON file name as board ID
                
                // Force update the cell's sprite based on the new JSON data
                CellController cellController = cellJsonData.GetComponent<CellController>();
                if (cellController != null)
                {
                    cellController.UpdateSpriteForJsonData();
                }
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] Configured CellJsonData on {cellJsonData.gameObject.name} with JSON: {jsonDataAsset.name}");
                }
            }
            
            // Load the data
            jsonManager.LoadBoardData();
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Loaded JSON data '{jsonDataAsset.name}' into board '{board.name}' and configured {cellJsonDataComponents.Length} CellJsonData components");
            }
        }
        
        /// <summary>
        /// Initialize all cells on a board with the PassiveTreeManager reference
        /// </summary>
        private void InitializeBoardCells(GameObject board)
        {
            if (board == null) return;
            
            // Find the PassiveTreeManager to initialize cells
            PassiveTreeManager treeManager = FindFirstObjectByType<PassiveTreeManager>();
            if (treeManager == null)
            {
                Debug.LogError("[BoardPositioningManager] ❌ Could not find PassiveTreeManager to initialize cells");
                return;
            }
            
            // Initialize all cells on the board
            CellController[] cells = board.GetComponentsInChildren<CellController>();
            foreach (CellController cell in cells)
            {
                cell.Initialize(treeManager);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] ✅ Initialized {cells.Length} cells on board {board.name} with PassiveTreeManager reference");
            }
        }
        
        /// <summary>
        /// Configure the corresponding cell on the new board as an allocated extension point
        /// This cell should be automatically purchased/allocated to establish the connection
        /// </summary>
        private void PurchaseCorrespondingCell(GameObject board, Vector2Int cellPosition)
        {
            if (board == null) return;
            
            // Find the cell at the specified position on the board
            CellController[] cells = board.GetComponentsInChildren<CellController>();
            foreach (CellController cell in cells)
            {
                if (cell.GridPosition == cellPosition)
                {
                    // Configure the cell as an allocated extension point (purchased)
                    cell.SetAvailable(true);
                    cell.SetUnlocked(true);
                    cell.SetPurchased(true); // This is the key fix - mark as purchased
                    cell.SetAsExtensionPoint(true);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[BoardPositioningManager] ✅ Configured cell at {cellPosition} on board {board.name} as ALLOCATED extension point (purchased)");
                    }
                    return;
                }
            }
            
            if (showDebugInfo)
            {
                Debug.LogWarning($"[BoardPositioningManager] ❌ Could not find cell at {cellPosition} on board {board.name}");
            }
        }
        
        /// <summary>
        /// Unlock adjacent nodes from an extension point for orthographic interaction and purchasing
        /// </summary>
        private void UnlockAdjacentNodesFromExtensionPoint(GameObject board, Vector2Int extensionPointPosition)
        {
            if (board == null) 
            {
                Debug.LogWarning("[BoardPositioningManager] Cannot unlock adjacent nodes - board is null");
                return;
            }
            
            Debug.Log($"[BoardPositioningManager] Unlocking adjacent nodes from extension point at {extensionPointPosition} on board {board.name}");
            
            // Define orthographic directions (up, down, left, right)
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                extensionPointPosition + Vector2Int.up,    // North
                extensionPointPosition + Vector2Int.down,  // South
                extensionPointPosition + Vector2Int.left,  // West
                extensionPointPosition + Vector2Int.right  // East
            };
            
            Debug.Log($"[BoardPositioningManager] Adjacent positions to check: {string.Join(", ", adjacentPositions.Select(p => p.ToString()))}");
            
            // Find all cells on the board
            CellController[] allCells = board.GetComponentsInChildren<CellController>();
            Dictionary<Vector2Int, CellController> cellMap = new Dictionary<Vector2Int, CellController>();
            
            foreach (CellController cell in allCells)
            {
                cellMap[cell.GridPosition] = cell;
            }
            
            Debug.Log($"[BoardPositioningManager] Found {allCells.Length} total cells on board");
            
            // Unlock and make purchasable adjacent cells
            foreach (Vector2Int pos in adjacentPositions)
            {
                Debug.Log($"[BoardPositioningManager] Checking position {pos}");
                
                if (cellMap.ContainsKey(pos))
                {
                    CellController adjacentCell = cellMap[pos];
                    Debug.Log($"[BoardPositioningManager] Found cell at {pos}: Type={adjacentCell.NodeType}, IsPurchased={adjacentCell.IsPurchased}, IsUnlocked={adjacentCell.IsUnlocked}, IsAvailable={adjacentCell.IsAvailable}");
                    
                    // Only unlock if it's not already purchased and not an extension point
                    if (!adjacentCell.IsPurchased && adjacentCell.NodeType != NodeType.Extension)
                    {
                        Debug.Log($"[BoardPositioningManager] Unlocking cell at {pos}");
                        
                        // Enable full interaction and purchasing
                        adjacentCell.SetUnlocked(true);
                        adjacentCell.SetAvailable(true);
                        
                        // Ensure the cell is properly configured for purchasing
                        // This might involve additional setup depending on the CellController implementation
                        ConfigureCellForPurchasing(adjacentCell);
                        
                        Debug.Log($"[BoardPositioningManager] ✅ Unlocked and made purchasable adjacent node at {pos} (Type: {adjacentCell.NodeType}) - Final state: Unlocked={adjacentCell.IsUnlocked}, Available={adjacentCell.IsAvailable}");
                    }
                    else if (adjacentCell.NodeType == NodeType.Extension)
                    {
                        Debug.Log($"[BoardPositioningManager] Skipped extension point at {pos} - will be handled separately");
                    }
                    else if (adjacentCell.IsPurchased)
                    {
                        Debug.Log($"[BoardPositioningManager] Skipped already purchased node at {pos}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[BoardPositioningManager] No cell found at position {pos}");
                }
            }
        }
        
        /// <summary>
        /// Configure a cell for proper purchasing functionality
        /// </summary>
        private void ConfigureCellForPurchasing(CellController cell)
        {
            if (cell == null) return;
            
            // Ensure the cell has all necessary components for purchasing
            // This might include setting up click handlers, visual states, etc.
            
            // Make sure the cell is in the correct state for interaction
            cell.SetAvailable(true);
            cell.SetUnlocked(true);
            
            // Additional configuration might be needed here depending on your CellController implementation
            // For example, ensuring proper visual feedback, click handlers, etc.
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Configured cell at {cell.GridPosition} for purchasing (Type: {cell.NodeType})");
            }
        }
        
        /// <summary>
        /// Get the corresponding cell position for automatic allocation
        /// Maps specific cell coordinates to maintain clean paths between boards
        /// </summary>
        private Vector2Int GetCorrespondingCellPosition(Vector2Int sourceCellPosition)
        {
            // Specific cell coordinate mapping for clean path maintenance
            // The corresponding cell should be the OPPOSITE edge of the new board
            if (sourceCellPosition == new Vector2Int(3, 6)) // North extension point on core
            {
                return new Vector2Int(3, 0); // South cell on extension board
            }
            else if (sourceCellPosition == new Vector2Int(3, 0)) // South extension point on core
            {
                return new Vector2Int(3, 6); // North cell on extension board
            }
            else if (sourceCellPosition == new Vector2Int(0, 3)) // West extension point on core
            {
                return new Vector2Int(6, 3); // East cell on extension board
            }
            else if (sourceCellPosition == new Vector2Int(6, 3)) // East extension point on core
            {
                return new Vector2Int(0, 3); // West cell on extension board
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[BoardPositioningManager] Unknown source cell position: {sourceCellPosition}");
                }
                return sourceCellPosition; // Fallback to same position
            }
        }
        
        /// <summary>
        /// Coroutine to allocate corresponding extension point after a frame delay
        /// </summary>
        private IEnumerator AllocateCorrespondingExtensionPointDelayed(ExtensionPoint sourceExtensionPoint, GameObject newBoard, Vector2Int newBoardGridPosition)
        {
            // Wait a few frames to ensure extension points are fully added to the list
            yield return null;
            yield return null;
            yield return null;
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Attempting allocation after delay for board at {newBoardGridPosition}");
            }
            
            // Now try to allocate the corresponding extension point
            AllocateCorrespondingExtensionPoint(sourceExtensionPoint, newBoard, newBoardGridPosition);
            
            // If allocation failed, try again after a longer delay
            yield return new WaitForSeconds(0.1f);
            
            // Check if allocation was successful by looking for the corresponding extension point
            Vector2Int correspondingCellPosition = GetCorrespondingCellPosition(sourceExtensionPoint.position);
            ExtensionPoint correspondingEP = availableExtensionPoints.FirstOrDefault(ep => 
                ep.id.Contains($"extension_{newBoardGridPosition.x}_{newBoardGridPosition.y}") &&
                ep.position == correspondingCellPosition);
            
            if (correspondingEP == null || correspondingEP.CanConnect())
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] Retrying allocation after longer delay...");
                }
                AllocateCorrespondingExtensionPoint(sourceExtensionPoint, newBoard, newBoardGridPosition);
            }
        }
        
        /// <summary>
        /// Get all available extension points
        /// </summary>
        public List<ExtensionPoint> GetAvailableExtensionPoints()
        {
            return availableExtensionPoints.Where(ep => ep.CanConnect()).ToList();
        }
        
        /// <summary>
        /// Get all extension points (both available and allocated)
        /// </summary>
        public List<ExtensionPoint> GetAllExtensionPoints()
        {
            return new List<ExtensionPoint>(availableExtensionPoints);
        }
        
        /// <summary>
        /// Get extension points that can connect a specific board type
        /// </summary>
        public List<ExtensionPoint> GetExtensionPointsForBoardType(string boardType)
        {
            return availableExtensionPoints.Where(ep => ep.CanConnectBoard(boardType)).ToList();
        }
        
        /// <summary>
        /// Get the core board reference
        /// </summary>
        public GameObject GetCoreBoard()
        {
            return coreBoard;
        }
        
        /// <summary>
        /// Get all positioned boards
        /// </summary>
        public Dictionary<Vector2Int, GameObject> GetAllPositionedBoards()
        {
            return new Dictionary<Vector2Int, GameObject>(positionedBoards);
        }
        
        /// <summary>
        /// Get board at specific grid position
        /// </summary>
        public GameObject GetBoardAtPosition(Vector2Int gridPosition)
        {
            positionedBoards.TryGetValue(gridPosition, out GameObject board);
            return board;
        }
        
        /// <summary>
        /// Remove a board from the grid
        /// </summary>
        public bool RemoveBoard(Vector2Int gridPosition)
        {
            if (!positionedBoards.ContainsKey(gridPosition))
            {
                Debug.LogWarning($"[BoardPositioningManager] No board found at grid position: {gridPosition}");
                return false;
            }
            
            GameObject board = positionedBoards[gridPosition];
            
            // Don't allow removing the core board
            if (board == coreBoard)
            {
                Debug.LogWarning("[BoardPositioningManager] Cannot remove the core board!");
                return false;
            }
            
            // Remove from tracking
            positionedBoards.Remove(gridPosition);
            
            // Remove extension points associated with this board
            RemoveExtensionPointsForBoard(gridPosition);
            
            // Destroy the board
            DestroyImmediate(board);
            
            if (showDebugInfo)
                Debug.Log($"[BoardPositioningManager] Removed board at grid position: {gridPosition}");
            
            return true;
        }
        
        /// <summary>
        /// Remove extension points associated with a specific board
        /// </summary>
        private void RemoveExtensionPointsForBoard(Vector2Int gridPosition)
        {
            availableExtensionPoints.RemoveAll(ep => 
                ep.id.Contains($"{gridPosition.x}_{gridPosition.y}"));
        }
        
        /// <summary>
        /// Clear all extension boards (keep core board)
        /// </summary>
        public void ClearAllExtensionBoards()
        {
            List<Vector2Int> positionsToRemove = positionedBoards.Keys.Where(pos => pos != coreBoardGridPosition).ToList();
            
            foreach (Vector2Int position in positionsToRemove)
            {
                RemoveBoard(position);
            }
            
            if (showDebugInfo)
                Debug.Log("[BoardPositioningManager] Cleared all extension boards");
        }
        
        /// <summary>
        /// Get the grid position of a board
        /// </summary>
        public Vector2Int GetBoardGridPosition(GameObject board)
        {
            foreach (var kvp in positionedBoards)
            {
                if (kvp.Value == board)
                    return kvp.Key;
            }
            return Vector2Int.zero;
        }
        
        #region Debug Methods
        
        /// <summary>
        /// Manually center camera on the core board's center cell
        /// </summary>
        [ContextMenu("Center Camera on Core Board")]
        public void CenterCameraOnCoreBoardManual()
        {
            CenterCameraOnCoreBoard();
        }
        
        /// <summary>
        /// Debug method to spawn test extension boards
        /// </summary>
        [ContextMenu("Spawn Test Extension Boards")]
        public void SpawnTestExtensionBoards()
        {
            var availablePoints = GetAvailableExtensionPoints();
            
            if (availablePoints.Count == 0)
            {
                Debug.LogWarning("[BoardPositioningManager] No available extension points found!");
                return;
            }
            
            Debug.Log($"[BoardPositioningManager] Found {availablePoints.Count} available extension points");
            
            // Spawn one board for each available extension point
            foreach (var extensionPoint in availablePoints.Take(4)) // Limit to 4 for testing
            {
                Debug.Log($"[BoardPositioningManager] Spawning board at extension point: {extensionPoint.id} at world position: {extensionPoint.worldPosition}");
                bool success = SpawnExtensionBoard(extensionPoint, "fire_board");
                Debug.Log($"[BoardPositioningManager] Spawn result: {success}");
            }
        }
        
        /// <summary>
        /// Debug method to spawn a single extension board at a specific direction
        /// </summary>
        [ContextMenu("Spawn Single Extension Board (North)")]
        public void SpawnSingleExtensionBoardNorth()
        {
            var availablePoints = GetAvailableExtensionPoints();
            var northPoint = availablePoints.FirstOrDefault(ep => ep.id.Contains("_0_1")); // North direction
            
            if (northPoint != null)
            {
                Debug.Log($"[BoardPositioningManager] Spawning north extension board at: {northPoint.worldPosition}");
                SpawnExtensionBoard(northPoint, "fire_board");
            }
            else
            {
                Debug.LogWarning("[BoardPositioningManager] No north extension point found!");
            }
        }
        
        /// <summary>
        /// Debug method to check extension point setup
        /// </summary>
        [ContextMenu("Debug Extension Points")]
        public void DebugExtensionPoints()
        {
            Debug.Log("=== EXTENSION POINTS DEBUG ===");
            Debug.Log($"Core board grid position: {coreBoardGridPosition}");
            Debug.Log($"Board size: {boardSize}");
            Debug.Log($"Grid cell size: {gridCellSize}");
            Debug.Log($"Available extension points: {availableExtensionPoints.Count}");
            
            foreach (var ep in availableExtensionPoints)
            {
                Vector3 worldPos = CalculateGridWorldPosition(new Vector2Int(ep.worldPosition.x, ep.worldPosition.y));
                Debug.Log($"Extension Point: {ep.id} at cell position {ep.position}, grid position {ep.worldPosition}, world position {worldPos}");
            }
            
            // Check which cells are marked as extension points
            if (coreBoard != null)
            {
                CellController[] cells = coreBoard.GetComponentsInChildren<CellController>();
                Debug.Log($"Total cells in core board: {cells.Length}");
                
                foreach (CellController cell in cells)
                {
                    if (cell.IsExtensionPoint)
                    {
                        Debug.Log($"Extension Point Cell: {cell.GridPosition} - {cell.NodeType}");
                    }
                }
            }
            
            Debug.Log("=== END EXTENSION POINTS DEBUG ===");
        }
        
        /// <summary>
        /// Debug method to show grid layout visualization
        /// </summary>
        [ContextMenu("Debug Grid Layout")]
        public void DebugGridLayout()
        {
            Debug.Log("=== GRID LAYOUT DEBUG ===");
            Debug.Log($"Core board at grid position: {coreBoardGridPosition}");
            
            // Show where extension boards should be positioned
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };
            string[] directionNames = { "North", "South", "East", "West" };
            
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int extensionGridPos = coreBoardGridPosition + directions[i];
                Vector3 worldPos = CalculateGridWorldPosition(extensionGridPos);
                Debug.Log($"{directionNames[i]} extension board should be at grid: {extensionGridPos}, world: {worldPos}");
            }
            
            Debug.Log("=== END GRID LAYOUT DEBUG ===");
        }
        
        /// <summary>
        /// Debug method to fix scaling and collider size for all boards
        /// </summary>
        [ContextMenu("Fix All Board Cell Scaling & Colliders")]
        public void FixAllBoardCellScaling()
        {
            Debug.Log("=== FIXING ALL BOARD CELL SCALING & COLLIDERS ===");
            
            foreach (var kvp in positionedBoards)
            {
                GameObject board = kvp.Value;
                FixBoardCellScaling(board);
            }
            
            Debug.Log("=== SCALING & COLLIDER FIX COMPLETE ===");
        }
        
        /// <summary>
        /// Update grid cell size based on current cell pixel size and board dimensions
        /// </summary>
        [ContextMenu("Update Grid Cell Size")]
        public void UpdateGridCellSize()
        {
            int boardPixelSize = GetBoardPixelSize();
            gridCellSize = new Vector2Int(boardPixelSize, boardPixelSize);
            
            // Update the Grid component if it exists
            if (boardsContainer != null)
            {
                var grid = boardsContainer.GetComponent<Grid>();
                if (grid != null)
                {
                    grid.cellSize = new Vector3(gridCellSize.x, gridCellSize.y, 1f);
                    Debug.Log($"[BoardPositioningManager] Updated grid cell size to: {gridCellSize} pixels ({boardPixelSize}x{boardPixelSize})");
                }
            }
        }
        
        /// <summary>
        /// Debug method to show Grid component settings
        /// </summary>
        [ContextMenu("Debug Grid Component")]
        public void DebugGridComponent()
        {
            Debug.Log("=== GRID COMPONENT DEBUG ===");
            
            if (boardsContainer != null)
            {
                var grid = boardsContainer.GetComponent<Grid>();
                if (grid != null)
                {
                    Debug.Log($"Grid Cell Size: {grid.cellSize}");
                    Debug.Log($"Grid Cell Gap: {grid.cellGap}");
                    Debug.Log($"Grid Cell Layout: {grid.cellLayout}");
                    Debug.Log($"Grid Cell Swizzle: {grid.cellSwizzle}");
                }
                else
                {
                    Debug.LogWarning("No Grid component found on boardsContainer!");
                }
            }
            else
            {
                Debug.LogWarning("boardsContainer is null!");
            }
            
            Debug.Log($"Inspector Grid Cell Size: {gridCellSize}");
            Debug.Log($"Cell Pixel Size: {cellPixelSize}");
            Debug.Log($"Board Size: {boardSize}");
            Debug.Log($"Calculated Board Pixel Size: {GetBoardPixelSize()}");
            Debug.Log($"Core Board World Position: {coreBoardWorldPosition}");
            
            Debug.Log("=== END GRID COMPONENT DEBUG ===");
        }
        
        /// <summary>
        /// Debug method to detect and show core board world position
        /// </summary>
        [ContextMenu("Detect Core Board Position")]
        public void DetectCoreBoardPosition()
        {
            if (coreBoard != null)
            {
                DetectCoreBoardWorldPosition();
                Debug.Log($"[BoardPositioningManager] Core board world position detected: {coreBoardWorldPosition}");
            }
            else
            {
                Debug.LogWarning("[BoardPositioningManager] Core board is null! Cannot detect position.");
            }
        }
        
        /// <summary>
        /// Debug method to show current extension board offset settings
        /// </summary>
        [ContextMenu("Debug Extension Board Offset")]
        public void DebugExtensionBoardOffset()
        {
            Debug.Log("=== EXTENSION BOARD OFFSET DEBUG ===");
            Debug.Log($"Current Extension Board Offset: {extensionBoardOffset}");
            Debug.Log($"Core Board World Position: {coreBoardWorldPosition}");
            Debug.Log($"Grid Cell Size: {gridCellSize}");
            
            // Show expected positions with current offset
            Vector3 northPos = coreBoardWorldPosition + new Vector3(0, 900 + extensionBoardOffset.y, 0);
            Vector3 southPos = coreBoardWorldPosition + new Vector3(0, -900 + extensionBoardOffset.y, 0);
            Vector3 eastPos = coreBoardWorldPosition + new Vector3(900 + extensionBoardOffset.x, 0, 0);
            Vector3 westPos = coreBoardWorldPosition + new Vector3(-900 + extensionBoardOffset.x, 0, 0);
            
            Debug.Log($"Expected North Extension Position: {northPos}");
            Debug.Log($"Expected South Extension Position: {southPos}");
            Debug.Log($"Expected East Extension Position: {eastPos}");
            Debug.Log($"Expected West Extension Position: {westPos}");
            Debug.Log("=== END EXTENSION BOARD OFFSET DEBUG ===");
        }
        
        /// <summary>
        /// Debug method to show all extension points and their allocation status
        /// </summary>
        [ContextMenu("Debug Extension Point Allocation")]
        public void DebugExtensionPointAllocation()
        {
            Debug.Log("=== EXTENSION POINT ALLOCATION DEBUG ===");
            Debug.Log($"Total Extension Points: {availableExtensionPoints.Count}");
            
            foreach (ExtensionPoint ep in availableExtensionPoints)
            {
                string status = ep.CanConnect() ? "Available" : "Allocated";
                Debug.Log($"Extension Point: {ep.id} - Status: {status} - Connections: {ep.currentConnections}/{ep.maxConnections}");
            }
            
            Debug.Log("=== END EXTENSION POINT ALLOCATION DEBUG ===");
        }
        
        /// <summary>
        /// Debug method to test automatic allocation manually
        /// </summary>
        [ContextMenu("Test Automatic Allocation")]
        public void TestAutomaticAllocation()
        {
            Debug.Log("=== TESTING AUTOMATIC ALLOCATION ===");
            
            // Test all four core extension points
            Vector2Int[] testCellPositions = { new Vector2Int(3, 6), new Vector2Int(3, 0), new Vector2Int(0, 3), new Vector2Int(6, 3) };
            
            foreach (Vector2Int cellPos in testCellPositions)
            {
                Debug.Log($"Testing cell position: {cellPos}");
                Vector2Int correspondingPos = GetCorrespondingCellPosition(cellPos);
                Debug.Log($"  → Corresponding cell position: {correspondingPos}");
            }
            
            // Find the first available extension point
            ExtensionPoint testPoint = availableExtensionPoints.FirstOrDefault(ep => ep.CanConnect());
            
            if (testPoint != null)
            {
                Debug.Log($"Testing with extension point: {testPoint.id} at cell {testPoint.position}");
                
                // Simulate spawning a board at this extension point
                Vector2Int testGridPosition = new Vector2Int(testPoint.worldPosition.x, testPoint.worldPosition.y);
                
                // Create a dummy board for testing
                GameObject testBoard = new GameObject("TestBoard");
                
                // Test the allocation
                AllocateCorrespondingExtensionPoint(testPoint, testBoard, testGridPosition);
                
                // Clean up
                DestroyImmediate(testBoard);
            }
            else
            {
                Debug.LogWarning("No available extension points found for testing!");
            }
            
            Debug.Log("=== END TESTING AUTOMATIC ALLOCATION ===");
        }
        
        /// <summary>
        /// Comprehensive debug method to analyze the entire allocation process
        /// </summary>
        [ContextMenu("Debug Complete Allocation Process")]
        public void DebugCompleteAllocationProcess()
        {
            Debug.Log("=== COMPLETE ALLOCATION PROCESS DEBUG ===");
            
            // 1. Show core board info
            Debug.Log($"Core Board: {(coreBoard != null ? coreBoard.name : "NULL")}");
            Debug.Log($"Core Board Grid Position: {coreBoardGridPosition}");
            Debug.Log($"Core Board World Position: {coreBoardWorldPosition}");
            
            // 2. Show all available extension points
            Debug.Log($"Total Available Extension Points: {availableExtensionPoints.Count}");
            foreach (ExtensionPoint ep in availableExtensionPoints)
            {
                string status = ep.CanConnect() ? "Available" : "Allocated";
                Debug.Log($"  - {ep.id} at cell {ep.position} - Status: {status} - World: {ep.worldPosition}");
            }
            
            // 3. Show positioned boards
            Debug.Log($"Positioned Boards: {positionedBoards.Count}");
            foreach (var kvp in positionedBoards)
            {
                Debug.Log($"  - Grid {kvp.Key}: {kvp.Value.name}");
            }
            
            // 4. Test each core extension point
            Debug.Log("=== TESTING CORE EXTENSION POINTS ===");
            Vector2Int[] coreCellPositions = { new Vector2Int(3, 6), new Vector2Int(3, 0), new Vector2Int(0, 3), new Vector2Int(6, 3) };
            
            foreach (Vector2Int cellPos in coreCellPositions)
            {
                Debug.Log($"Testing core cell {cellPos}:");
                
                // Find the extension point at this cell
                ExtensionPoint coreEP = availableExtensionPoints.FirstOrDefault(ep => 
                    ep.id.StartsWith("core_extension") && ep.position == cellPos);
                
                if (coreEP != null)
                {
                    Debug.Log($"  - Found core extension point: {coreEP.id}");
                    Debug.Log($"  - World position: {coreEP.worldPosition}");
                    Debug.Log($"  - Can connect: {coreEP.CanConnect()}");
                    
                    // Calculate where the extension board would be created
                    Vector2Int extensionBoardGridPos = new Vector2Int(coreEP.worldPosition.x, coreEP.worldPosition.y);
                    Debug.Log($"  - Extension board would be at grid: {extensionBoardGridPos}");
                    
                    // Check if there are any extension points for this board
                    var extensionBoardEPs = availableExtensionPoints.Where(ep => 
                        ep.id.Contains($"extension_{extensionBoardGridPos.x}_{extensionBoardGridPos.y}"));
                    
                    Debug.Log($"  - Extension board extension points: {extensionBoardEPs.Count()}");
                    foreach (ExtensionPoint extEP in extensionBoardEPs)
                    {
                        Debug.Log($"    * {extEP.id} at cell {extEP.position}");
                    }
                    
                    // Test the corresponding cell mapping
                    Vector2Int correspondingCell = GetCorrespondingCellPosition(cellPos);
                    Debug.Log($"  - Corresponding cell would be: {correspondingCell}");
                    
                    // Check if that corresponding cell exists
                    ExtensionPoint correspondingEP = extensionBoardEPs.FirstOrDefault(ep => ep.position == correspondingCell);
                    if (correspondingEP != null)
                    {
                        Debug.Log($"  - ✅ Corresponding extension point found: {correspondingEP.id}");
                    }
                    else
                    {
                        Debug.Log($"  - ❌ Corresponding extension point NOT found for cell {correspondingCell}");
                    }
                }
                else
                {
                    Debug.Log($"  - ❌ No core extension point found at cell {cellPos}");
                }
            }
            
            Debug.Log("=== END COMPLETE ALLOCATION PROCESS DEBUG ===");
        }
        
        /// <summary>
        /// Manually test extension board creation with detailed logging
        /// </summary>
        [ContextMenu("Manual Test Extension Board Creation")]
        public void ManualTestExtensionBoardCreation()
        {
            Debug.Log("=== MANUAL TEST EXTENSION BOARD CREATION ===");
            
            // Find the first available core extension point
            ExtensionPoint testPoint = availableExtensionPoints.FirstOrDefault(ep => 
                ep.id.StartsWith("core_extension") && ep.CanConnect());
            
            if (testPoint != null)
            {
                Debug.Log($"Testing with extension point: {testPoint.id} at cell {testPoint.position}");
                Debug.Log($"Extension point world position: {testPoint.worldPosition}");
                
                // Show what would happen
                Vector2Int extensionBoardGridPos = new Vector2Int(testPoint.worldPosition.x, testPoint.worldPosition.y);
                Debug.Log($"Extension board would be created at grid: {extensionBoardGridPos}");
                
                Vector2Int correspondingCell = GetCorrespondingCellPosition(testPoint.position);
                Debug.Log($"Corresponding cell that should be allocated: {correspondingCell}");
                
                // Actually create the extension board
                Debug.Log("Creating extension board...");
                bool success = SpawnExtensionBoard(testPoint, "fire_board");
                Debug.Log($"Extension board creation result: {success}");
                
                // Wait a moment and then check the allocation
                StartCoroutine(CheckAllocationAfterDelay(extensionBoardGridPos, correspondingCell));
            }
            else
            {
                Debug.LogWarning("No available core extension points found for testing!");
            }
            
            Debug.Log("=== END MANUAL TEST EXTENSION BOARD CREATION ===");
        }
        
        /// <summary>
        /// Coroutine to check allocation after a delay
        /// </summary>
        private IEnumerator CheckAllocationAfterDelay(Vector2Int extensionBoardGridPos, Vector2Int expectedCorrespondingCell)
        {
            yield return new WaitForSeconds(0.5f); // Wait half a second
            
            Debug.Log("=== CHECKING ALLOCATION AFTER DELAY ===");
            
            // Check if the extension board was created
            if (positionedBoards.ContainsKey(extensionBoardGridPos))
            {
                GameObject board = positionedBoards[extensionBoardGridPos];
                Debug.Log($"✅ Extension board found at grid {extensionBoardGridPos}: {board.name}");
                
                // Check if the corresponding extension point was allocated
                ExtensionPoint correspondingEP = availableExtensionPoints.FirstOrDefault(ep => 
                    ep.id.Contains($"extension_{extensionBoardGridPos.x}_{extensionBoardGridPos.y}") &&
                    ep.position == expectedCorrespondingCell);
                
                if (correspondingEP != null)
                {
                    string status = correspondingEP.CanConnect() ? "Available" : "Allocated";
                    Debug.Log($"✅ Corresponding extension point found: {correspondingEP.id} - Status: {status}");
                    
                    // Check if the corresponding cell was configured correctly
                    CellController[] cells = board.GetComponentsInChildren<CellController>();
                    CellController correspondingCell = cells.FirstOrDefault(cell => cell.GridPosition == expectedCorrespondingCell);
                    
                    if (correspondingCell != null)
                    {
                        Debug.Log($"✅ Corresponding cell found at {expectedCorrespondingCell} - Available: {correspondingCell.IsAvailable}, Unlocked: {correspondingCell.IsUnlocked}, Extension Point: {correspondingCell.IsExtensionPoint}");
                    }
                    else
                    {
                        Debug.LogWarning($"❌ Corresponding cell NOT found at {expectedCorrespondingCell}");
                    }
                }
                else
                {
                    Debug.LogWarning($"❌ Corresponding extension point NOT found at cell {expectedCorrespondingCell}");
                    
                    // Show all extension points for this board
                    var boardEPs = availableExtensionPoints.Where(ep => 
                        ep.id.Contains($"extension_{extensionBoardGridPos.x}_{extensionBoardGridPos.y}"));
                    
                    Debug.Log($"Extension points for board at {extensionBoardGridPos}:");
                    foreach (ExtensionPoint ep in boardEPs)
                    {
                        Debug.Log($"  - {ep.id} at cell {ep.position}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"❌ Extension board NOT found at grid {extensionBoardGridPos}");
            }
            
            Debug.Log("=== END CHECKING ALLOCATION AFTER DELAY ===");
        }
        
        /// <summary>
        /// Debug method to show all board positions
        /// </summary>
        [ContextMenu("Debug Board Positions")]
        public void DebugBoardPositions()
        {
            Debug.Log("=== BOARD POSITIONS DEBUG ===");
            Debug.Log($"Grid Cell Size: {gridCellSize}");
            Debug.Log($"Board Size: {boardSize}");
            Debug.Log($"Core Board Grid Position: {coreBoardGridPosition}");
            
            foreach (var kvp in positionedBoards)
            {
                Vector2Int gridPos = kvp.Key;
                GameObject board = kvp.Value;
                
                Debug.Log($"Board: {board.name} at Grid Position: {gridPos}, World Position: {board.transform.position}");
            }
            
            Debug.Log($"Total boards: {positionedBoards.Count}");
            Debug.Log($"Available extension points: {GetAvailableExtensionPoints().Count}");
            
            // Debug extension points
            foreach (var ep in availableExtensionPoints)
            {
                Debug.Log($"Extension Point: {ep.id} at World Position: {ep.worldPosition}");
            }
            
            Debug.Log("=== END BOARD POSITIONS DEBUG ===");
        }
        
        /// <summary>
        /// Debug method to check cell states on extension boards
        /// </summary>
        [ContextMenu("Debug Extension Board Cell States")]
        public void DebugExtensionBoardCellStates()
        {
            Debug.Log("=== DEBUGGING EXTENSION BOARD CELL STATES ===");
            
            // Find all extension boards (not core board)
            foreach (var kvp in positionedBoards)
            {
                if (kvp.Key != coreBoardGridPosition)
                {
                    GameObject extensionBoard = kvp.Value;
                    Vector2Int gridPos = kvp.Key;
                    
                    Debug.Log($"Extension Board: {extensionBoard.name} at grid {gridPos}");
                    
                    // Get all cells on this board
                    CellController[] cells = extensionBoard.GetComponentsInChildren<CellController>();
                    foreach (CellController cell in cells)
                    {
                        Debug.Log($"  Cell at {cell.GridPosition}: Type={cell.NodeType}, Available={cell.IsAvailable}, Unlocked={cell.IsUnlocked}, Purchased={cell.IsPurchased}");
                    }
                }
            }
            
            Debug.Log("=== END DEBUGGING EXTENSION BOARD CELL STATES ===");
        }
        
        /// <summary>
        /// Debug method to manually unlock orthographic nodes on extension boards
        /// </summary>
        [ContextMenu("Manually Unlock Orthographic Nodes")]
        public void ManuallyUnlockOrthographicNodes()
        {
            Debug.Log("=== MANUALLY UNLOCKING ORTHOGRAPHIC NODES ===");
            
            // Find all extension boards (not core board)
            foreach (var kvp in positionedBoards)
            {
                if (kvp.Key != coreBoardGridPosition)
                {
                    GameObject extensionBoard = kvp.Value;
                    Vector2Int gridPos = kvp.Key;
                    
                    Debug.Log($"Processing extension board: {extensionBoard.name} at grid {gridPos}");
                    
                    // Find the allocated extension point (should be at 3,0 for south extension)
                    CellController[] cells = extensionBoard.GetComponentsInChildren<CellController>();
                    CellController extensionPointCell = null;
                    
                    foreach (CellController cell in cells)
                    {
                        if (cell.NodeType == NodeType.Extension && cell.IsPurchased)
                        {
                            extensionPointCell = cell;
                            break;
                        }
                    }
                    
                    if (extensionPointCell != null)
                    {
                        Debug.Log($"Found allocated extension point at {extensionPointCell.GridPosition}");
                        
                        // Unlock adjacent nodes
                        UnlockAdjacentNodesFromExtensionPoint(extensionBoard, extensionPointCell.GridPosition);
                    }
                    else
                    {
                        Debug.LogWarning($"No allocated extension point found on board {extensionBoard.name}");
                    }
                }
            }
            
            Debug.Log("=== END MANUALLY UNLOCKING ORTHOGRAPHIC NODES ===");
        }
        
        /// <summary>
        /// Debug method to test purchasing functionality on extension boards
        /// </summary>
        [ContextMenu("Test Extension Board Purchasing")]
        public void TestExtensionBoardPurchasing()
        {
            Debug.Log("=== TESTING EXTENSION BOARD PURCHASING ===");
            
            // Find the first extension board (not core board)
            GameObject extensionBoard = null;
            Vector2Int extensionBoardGridPos = Vector2Int.zero;
            
            foreach (var kvp in positionedBoards)
            {
                if (kvp.Key != coreBoardGridPosition)
                {
                    extensionBoard = kvp.Value;
                    extensionBoardGridPos = kvp.Key;
                    break;
                }
            }
            
            if (extensionBoard != null)
            {
                Debug.Log($"Testing purchasing on extension board: {extensionBoard.name} at grid {extensionBoardGridPos}");
                
                // Find the ExtensionBoardController
                ExtensionBoardController boardController = extensionBoard.GetComponent<ExtensionBoardController>();
                if (boardController != null)
                {
                    Debug.Log("Found ExtensionBoardController, testing cell purchasing...");
                    
                    // Get all cells and test purchasing
                    CellController[] cells = extensionBoard.GetComponentsInChildren<CellController>();
                    foreach (CellController cell in cells)
                    {
                        if (cell.NodeType != NodeType.Extension && !cell.IsPurchased)
                        {
                            bool canPurchase = cell.IsAvailable && cell.IsUnlocked;
                            Debug.Log($"Cell at {cell.GridPosition}: Type={cell.NodeType}, Available={cell.IsAvailable}, Unlocked={cell.IsUnlocked}, CanPurchase={canPurchase}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No ExtensionBoardController found on extension board!");
                }
            }
            else
            {
                Debug.LogWarning("No extension boards found! Create one first using 'Test Fixed Extension Point Allocation'");
            }
            
            Debug.Log("=== END TESTING EXTENSION BOARD PURCHASING ===");
        }
        
        /// <summary>
        /// Debug method to test the fixed extension point allocation
        /// </summary>
        [ContextMenu("Test Fixed Extension Point Allocation")]
        public void TestFixedExtensionPointAllocation()
        {
            Debug.Log("=== TESTING FIXED EXTENSION POINT ALLOCATION ===");
            
            // Find the first available core extension point
            ExtensionPoint testPoint = availableExtensionPoints.FirstOrDefault(ep => 
                ep.id.StartsWith("core_extension") && ep.CanConnect());
            
            if (testPoint != null)
            {
                Debug.Log($"Testing with extension point: {testPoint.id} at cell {testPoint.position}");
                Debug.Log($"Extension point world position: {testPoint.worldPosition}");
                
                // Show what would happen
                Vector2Int extensionBoardGridPos = new Vector2Int(testPoint.worldPosition.x, testPoint.worldPosition.y);
                Debug.Log($"Extension board would be created at grid: {extensionBoardGridPos}");
                
                Vector2Int correspondingCell = GetCorrespondingCellPosition(testPoint.position);
                Debug.Log($"Corresponding cell that should be allocated: {correspondingCell}");
                
                // Actually create the extension board
                Debug.Log("Creating extension board...");
                bool success = SpawnExtensionBoard(testPoint, "fire_board");
                Debug.Log($"Extension board creation result: {success}");
                
                if (success)
                {
                    // Wait a moment and then check the allocation
                    StartCoroutine(VerifyAllocationAfterDelay(extensionBoardGridPos, correspondingCell));
                }
            }
            else
            {
                Debug.LogWarning("No available core extension points found for testing!");
            }
            
            Debug.Log("=== END TESTING FIXED EXTENSION POINT ALLOCATION ===");
        }
        
        /// <summary>
        /// Coroutine to verify allocation after a delay
        /// </summary>
        private IEnumerator VerifyAllocationAfterDelay(Vector2Int extensionBoardGridPos, Vector2Int expectedCorrespondingCell)
        {
            yield return new WaitForSeconds(0.5f); // Wait half a second
            
            Debug.Log("=== VERIFYING ALLOCATION AFTER DELAY ===");
            
            // Check if the extension board was created
            if (positionedBoards.ContainsKey(extensionBoardGridPos))
            {
                GameObject board = positionedBoards[extensionBoardGridPos];
                Debug.Log($"✅ Extension board found at grid {extensionBoardGridPos}: {board.name}");
                
                // Check if the corresponding extension point was allocated
                ExtensionPoint correspondingEP = availableExtensionPoints.FirstOrDefault(ep => 
                    ep.id.Contains($"extension_{extensionBoardGridPos.x}_{extensionBoardGridPos.y}") &&
                    ep.position == expectedCorrespondingCell);
                
                if (correspondingEP != null)
                {
                    string status = correspondingEP.CanConnect() ? "Available" : "Allocated";
                    Debug.Log($"✅ Corresponding extension point found: {correspondingEP.id} - Status: {status}");
                    
                    // Check if the corresponding cell was configured correctly
                    CellController[] cells = board.GetComponentsInChildren<CellController>();
                    CellController correspondingCell = cells.FirstOrDefault(cell => cell.GridPosition == expectedCorrespondingCell);
                    
                    if (correspondingCell != null)
                    {
                        Debug.Log($"✅ Corresponding cell found at {expectedCorrespondingCell} - Available: {correspondingCell.IsAvailable}, Unlocked: {correspondingCell.IsUnlocked}, Purchased: {correspondingCell.IsPurchased}, Extension Point: {correspondingCell.IsExtensionPoint}");
                        
                        // Check if adjacent nodes are unlocked for orthographic interaction
                        Vector2Int[] adjacentPositions = new Vector2Int[]
                        {
                            expectedCorrespondingCell + Vector2Int.up,    // North
                            expectedCorrespondingCell + Vector2Int.down,  // South
                            expectedCorrespondingCell + Vector2Int.left,  // West
                            expectedCorrespondingCell + Vector2Int.right  // East
                        };
                        
                        Debug.Log("Checking adjacent nodes for orthographic interaction and purchasing:");
                        foreach (Vector2Int pos in adjacentPositions)
                        {
                            CellController adjacentCell = cells.FirstOrDefault(cell => cell.GridPosition == pos);
                            if (adjacentCell != null)
                            {
                                // Check if the node can be purchased using the ExtensionBoardController logic
                                bool canPurchase = !adjacentCell.IsPurchased && adjacentCell.IsUnlocked && adjacentCell.IsAvailable;
                                Debug.Log($"  - Node at {pos}: Available={adjacentCell.IsAvailable}, Unlocked={adjacentCell.IsUnlocked}, Purchased={adjacentCell.IsPurchased}, CanPurchase={canPurchase}, Type={adjacentCell.NodeType}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"❌ Corresponding cell NOT found at {expectedCorrespondingCell}");
                    }
                }
                else
                {
                    Debug.LogWarning($"❌ Corresponding extension point NOT found at cell {expectedCorrespondingCell}");
                }
            }
            else
            {
                Debug.LogWarning($"❌ Extension board NOT found at grid {extensionBoardGridPos}");
            }
            
            Debug.Log("=== END VERIFYING ALLOCATION AFTER DELAY ===");
        }
        
        /// <summary>
        /// Debug method to analyze extension point allocation issues
        /// </summary>
        [ContextMenu("Debug Extension Point Allocation Issues")]
        public void DebugExtensionPointAllocationIssues()
        {
            Debug.Log("=== EXTENSION POINT ALLOCATION ISSUES DEBUG ===");
            
            // 1. Show all extension points
            Debug.Log($"Total Extension Points: {availableExtensionPoints.Count}");
            foreach (ExtensionPoint ep in availableExtensionPoints)
            {
                string status = ep.CanConnect() ? "Available" : "Allocated";
                Debug.Log($"  - {ep.id} at cell {ep.position} (world: {ep.worldPosition}) - Status: {status}");
            }
            
            // 2. Show all positioned boards
            Debug.Log($"Positioned Boards: {positionedBoards.Count}");
            foreach (var kvp in positionedBoards)
            {
                Debug.Log($"  - Grid {kvp.Key}: {kvp.Value.name}");
            }
            
            // 3. Test allocation logic for each extension board
            foreach (var kvp in positionedBoards)
            {
                Vector2Int gridPos = kvp.Key;
                GameObject board = kvp.Value;
                
                // Skip core board
                if (gridPos == coreBoardGridPosition) continue;
                
                Debug.Log($"=== Testing allocation for board at {gridPos} ===");
                
                // Find extension points for this board
                var boardExtensionPoints = availableExtensionPoints.Where(ep => 
                    ep.id.Contains($"extension_{gridPos.x}_{gridPos.y}")).ToList();
                
                Debug.Log($"Extension points for board at {gridPos}: {boardExtensionPoints.Count}");
                foreach (ExtensionPoint ep in boardExtensionPoints)
                {
                    Debug.Log($"  - {ep.id} at cell {ep.position} - Can connect: {ep.CanConnect()}");
                }
                
                // Test the allocation logic
                // Find the source extension point that created this board
                Vector2Int fromDirection = coreBoardGridPosition - gridPos;
                Vector2Int fromDirectionNormalized = new Vector2Int(
                    fromDirection.x != 0 ? fromDirection.x / Mathf.Abs(fromDirection.x) : 0,
                    fromDirection.y != 0 ? fromDirection.y / Mathf.Abs(fromDirection.y) : 0
                );
                
                Debug.Log($"Board at {gridPos} came from direction: {fromDirectionNormalized}");
                
                // Find the source extension point
                ExtensionPoint sourceEP = availableExtensionPoints.FirstOrDefault(ep => 
                    ep.id.StartsWith("core_extension") && 
                    ep.worldPosition == new Vector3Int(gridPos.x, gridPos.y, 0));
                
                if (sourceEP != null)
                {
                    Debug.Log($"Source extension point: {sourceEP.id} at cell {sourceEP.position}");
                    
                    // Calculate corresponding cell position
                    Vector2Int correspondingCell = GetCorrespondingCellPosition(sourceEP.position);
                    Debug.Log($"Corresponding cell position: {correspondingCell}");
                    
                    // Check if there's an extension point at that position
                    ExtensionPoint correspondingEP = boardExtensionPoints.FirstOrDefault(ep => 
                        ep.position == correspondingCell);
                    
                    if (correspondingEP != null)
                    {
                        Debug.Log($"✅ Corresponding extension point found: {correspondingEP.id} at cell {correspondingEP.position}");
                        Debug.Log($"   Status: {(correspondingEP.CanConnect() ? "Available" : "Allocated")}");
                    }
                    else
                    {
                        Debug.LogWarning($"❌ No corresponding extension point found at cell {correspondingCell}");
                        Debug.Log($"Available cells on this board:");
                        foreach (ExtensionPoint ep in boardExtensionPoints)
                        {
                            Debug.Log($"  - Cell {ep.position}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"❌ No source extension point found for board at {gridPos}");
                }
            }
            
            Debug.Log("=== END EXTENSION POINT ALLOCATION ISSUES DEBUG ===");
        }
        
        #endregion
        
        #region Gizmos
        
        void OnDrawGizmos()
        {
            if (!showGridGizmos || boardsContainer == null) return;
            
            // Draw grid lines
            Gizmos.color = Color.gray;
            
            // Draw board boundaries
            for (int x = -5; x <= 5; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    Vector3 worldPos = boardsContainer.TransformPoint(new Vector3(x, y, 0));
                    Gizmos.DrawWireCube(worldPos, new Vector3(gridCellSize.x, gridCellSize.y, 0.1f));
                }
            }
            
            // Draw positioned boards
            Gizmos.color = Color.green;
            foreach (var kvp in positionedBoards)
            {
                Vector2Int gridPos = kvp.Key;
                Vector3 worldPos = boardsContainer.TransformPoint(new Vector3(gridPos.x, gridPos.y, 0));
                Gizmos.DrawCube(worldPos, new Vector3(gridCellSize.x * 0.8f, gridCellSize.y * 0.8f, 0.1f));
            }
            
            // Draw extension points
            Gizmos.color = Color.yellow;
            foreach (var extensionPoint in availableExtensionPoints)
            {
                Vector3 worldPos = boardsContainer.TransformPoint(new Vector3(extensionPoint.worldPosition.x, extensionPoint.worldPosition.y, 0));
                Gizmos.DrawWireSphere(worldPos, 0.5f);
            }
        }
        
        #endregion
    }
}
