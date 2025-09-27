using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        [SerializeField] private ExtensionBoardGenerator extensionBoardGenerator;
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
        [SerializeField] private GameObject selectionContainer;
        [SerializeField] private Transform boardButtonContainer;
        [SerializeField] private GameObject boardButtonPrefab;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool showGridGizmos = true;
        
        // Track current extension point for grid positioning
        private Vector2Int currentExtensionPointPosition = Vector2Int.zero;
        private GameObject currentExtensionPointBoard = null; // Track which board the extension point click came from
        
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
            
            // Auto-find the selection container if not assigned
            if (selectionContainer == null)
            {
                selectionContainer = GameObject.Find("SelectionContainer");
                if (selectionContainer == null && showDebugInfo)
                {
                    Debug.LogWarning("[BoardPositioningManager] SelectionContainer not found in scene - extension point clicks will not work");
                }
                else if (showDebugInfo)
                {
                    Debug.Log("[BoardPositioningManager] Auto-found SelectionContainer");
                }
            }
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
            
            // Initialize all cells on the core board with PassiveTreeManager reference
            InitializeBoardCells(coreBoard);
            
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
        /// Get the extension point name from a direction
        /// </summary>
        private string GetExtensionPointNameFromDirection(Vector2Int direction)
        {
            if (direction == Vector2Int.up)    // North
                return "Extension_North";
            else if (direction == Vector2Int.down)  // South
                return "Extension_South";
            else if (direction == Vector2Int.right) // East
                return "Extension_East";
            else if (direction == Vector2Int.left)  // West
                return "Extension_West";
            else
                return "Extension_Unknown";
        }
        
        /// <summary>
        /// Get the corresponding extension point name for a new board based on the clicked extension point
        /// </summary>
        private string GetCorrespondingExtensionPointNameForNewBoard(Vector2Int direction, Vector2Int boardGridPosition)
        {
            // Get the direction this board came from (towards core)
            Vector2Int fromDirection = coreBoardGridPosition - boardGridPosition;
            Vector2Int fromDirectionNormalized = new Vector2Int(
                fromDirection.x != 0 ? fromDirection.x / Mathf.Abs(fromDirection.x) : 0,
                fromDirection.y != 0 ? fromDirection.y / Mathf.Abs(fromDirection.y) : 0
            );
            
            // Get the clicked extension point name
            string clickedExtensionPointName = GetClickedExtensionPointName();
            if (string.IsNullOrEmpty(clickedExtensionPointName))
            {
                Debug.LogWarning("[BoardPositioningManager] ❌ Could not determine clicked extension point name for corresponding mapping");
                return GetExtensionPointNameFromDirection(direction); // Fallback to local direction
            }
            
            // Check if this is the extension point that should connect back to the core board
            // The extension point that connects back to core should be the one in the opposite direction of fromDirection
            Vector2Int oppositeDirection = new Vector2Int(-fromDirectionNormalized.x, -fromDirectionNormalized.y);
            
            if (direction == oppositeDirection)
            {
                // This is the extension point that connects back to the core board
                // It should have the corresponding name to the clicked extension point
                string correspondingName = GetCorrespondingExtensionPointName(clickedExtensionPointName);
                Debug.Log($"[BoardPositioningManager] 🎯 Setting corresponding extension point name: {correspondingName} for direction {direction} (opposite of {fromDirectionNormalized})");
                return correspondingName;
            }
            else
            {
                // For other directions, use the local direction name
                string localName = GetExtensionPointNameFromDirection(direction);
                Debug.Log($"[BoardPositioningManager] 🎯 Setting local extension point name: {localName} for direction {direction}");
                return localName;
            }
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
            
            // If this is an extension board, set the grid position in the controller
            ExtensionBoardController extensionController = board.GetComponent<ExtensionBoardController>();
            if (extensionController != null)
            {
                extensionController.SetBoardGridPosition(gridPosition);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] Set grid position {gridPosition} for extension board controller {extensionController.GetBoardName()}");
                }
            }
            
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
            HandleExtensionPointClick(extensionPoint, null);
        }
        
        /// <summary>
        /// Handle extension point click with board information - either show board selection UI or spawn board directly
        /// </summary>
        public void HandleExtensionPointClick(ExtensionPoint extensionPoint, GameObject sourceBoard)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] HandleExtensionPointClick called for extension point:");
                Debug.Log($"  - Extension Point ID: {extensionPoint.id}");
                Debug.Log($"  - Extension Point Position: {extensionPoint.position}");
                Debug.Log($"  - Extension Point World Position: {extensionPoint.worldPosition}");
                Debug.Log($"  - Source Board: {(sourceBoard != null ? sourceBoard.name : "Core Board")}");
                Debug.Log($"[BoardPositioningManager] Using prefab-based selection system");
            }
            
            // Store the source board information
            currentExtensionPointBoard = sourceBoard;
            
            // Determine if this is a core board extension point or extension board extension point
            bool isCoreBoardExtensionPoint = extensionPoint.id.StartsWith("core_extension");
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Extension point type: {(isCoreBoardExtensionPoint ? "Core Board" : "Extension Board")}");
            }
            
            // Store the extension point position for grid calculation
            // Use the local cell position, not the board's world position
            currentExtensionPointPosition = extensionPoint.position;
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Stored extension point position: {currentExtensionPointPosition}");
                Debug.Log($"[BoardPositioningManager] Stored source board: {(currentExtensionPointBoard != null ? currentExtensionPointBoard.name : "Core Board")}");
            }
            
            // Calculate grid position for the extension board
            Vector2Int extensionGridPosition = new Vector2Int(extensionPoint.worldPosition.x, extensionPoint.worldPosition.y);
            
            // Find the current board name from the extension point ID
            string currentBoardName = GetCurrentBoardNameFromExtensionPoint(extensionPoint);
            
            // Enable the selection container for extension point selection
            if (selectionContainer != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] Enabling SelectionContainer for extension point at {extensionPoint.position}");
                    Debug.Log($"[BoardPositioningManager] Current board name: {currentBoardName}");
                }
                
                // Populate the selection container with available boards
                PopulateSelectionContainer(currentBoardName);
                
                // Enable the selection container
                selectionContainer.SetActive(true);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] ✅ SelectionContainer enabled: {selectionContainer.activeInHierarchy}");
                }
            }
            else
            {
                Debug.LogError($"[BoardPositioningManager] SelectionContainer is null - cannot show selection UI for extension point");
            }
        }
        
        /// <summary>
        /// Populate the selection container with available board prefabs
        /// </summary>
        private void PopulateSelectionContainer(string currentBoardName)
        {
            if (boardButtonContainer == null || boardButtonPrefab == null)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning("[BoardPositioningManager] Cannot populate selection - missing boardButtonContainer or boardButtonPrefab references");
                }
                return;
            }
            
            // Clear existing buttons
            ClearBoardButtons();
            
            // Get available boards from ExtensionBoardGenerator
            ExtensionBoardGenerator generator = FindFirstObjectByType<ExtensionBoardGenerator>();
            if (generator == null)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning("[BoardPositioningManager] No ExtensionBoardGenerator found in scene!");
                }
                return;
            }
            
            ExtensionBoardPrefabData[] availableBoards = generator.GetAvailableBoardPrefabs();
            if (availableBoards == null || availableBoards.Length == 0)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning("[BoardPositioningManager] No available board prefabs found!");
                }
                CreateNoBoardsMessage();
                return;
            }
            
            // Filter out the current board to prevent duplicates
            var selectableBoards = availableBoards.Where(board => 
                board != null && 
                board.boardPrefab != null && 
                !board.boardName.Equals(currentBoardName, System.StringComparison.OrdinalIgnoreCase)
            ).ToArray();
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Creating {selectableBoards.Length} board selection buttons");
                Debug.Log($"[BoardPositioningManager] Available boards: {string.Join(", ", selectableBoards.Select(b => b.boardName))}");
            }
            
            // Create buttons for each available board
            foreach (var boardData in selectableBoards)
            {
                CreateBoardSelectionButton(boardData);
            }
            
            // If no boards are available, show a message
            if (selectableBoards.Length == 0)
            {
                CreateNoBoardsMessage();
            }
        }
        
        /// <summary>
        /// Clear all existing board buttons
        /// </summary>
        private void ClearBoardButtons()
        {
            if (boardButtonContainer == null) return;
            
            foreach (Transform child in boardButtonContainer)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
        
        /// <summary>
        /// Create a button for a specific board prefab
        /// </summary>
        private void CreateBoardSelectionButton(ExtensionBoardPrefabData boardData)
        {
            if (boardButtonPrefab == null) return;
            
            // Instantiate button
            GameObject buttonObj = Instantiate(boardButtonPrefab, boardButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            if (button == null)
            {
                Debug.LogError($"[BoardPositioningManager] Board button prefab doesn't have a Button component!");
                Destroy(buttonObj);
                return;
            }
            
            // Setup button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = boardData.boardName;
            }
            
            // Setup button image/preview
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null && boardData.boardPreview != null)
            {
                buttonImage.sprite = boardData.boardPreview;
            }
            
            // Setup button color
            if (buttonImage != null)
            {
                buttonImage.color = boardData.boardColor;
            }
            
            // Setup button click event
            button.onClick.AddListener(() => OnBoardSelected(boardData));
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Created button for board: {boardData.boardName}");
            }
        }
        
        /// <summary>
        /// Create a message when no boards are available
        /// </summary>
        private void CreateNoBoardsMessage()
        {
            if (boardButtonPrefab == null) return;
            
            // Create a disabled button to show the message
            GameObject messageObj = Instantiate(boardButtonPrefab, boardButtonContainer);
            Button button = messageObj.GetComponent<Button>();
            
            if (button != null)
            {
                button.interactable = false;
            }
            
            // Setup message text
            TextMeshProUGUI messageText = messageObj.GetComponentInChildren<TextMeshProUGUI>();
            if (messageText != null)
            {
                messageText.text = "No additional boards available";
                messageText.color = Color.gray;
            }
            
            // Setup button appearance
            Image buttonImage = messageObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Semi-transparent gray
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] No additional boards available for selection");
            }
        }
        
        /// <summary>
        /// Handle board selection
        /// </summary>
        private void OnBoardSelected(ExtensionBoardPrefabData boardData)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] 🎯 BOARD SELECTION STARTED");
                Debug.Log($"[BoardPositioningManager] Board selected: {boardData.boardName}");
                Debug.Log($"[BoardPositioningManager] Current extension point position: {currentExtensionPointPosition}");
                Debug.Log($"[BoardPositioningManager] Source board: {(currentExtensionPointBoard != null ? currentExtensionPointBoard.name : "Core Board")}");
            }
            
            // Spawn the selected board first
            SpawnExtensionBoardPrefab(boardData);
            
            // Only mark the extension point as purchased if it came from the core board
            // Extension points on extension boards should not be purchased immediately
            if (currentExtensionPointBoard == null)
            {
                // This is a core board extension point - mark it as purchased
                Debug.Log($"[BoardPositioningManager] 🔒 MARKING CORE BOARD EXTENSION POINT AS PURCHASED");
                MarkExtensionPointAsPurchased();
            }
            else
            {
                // This is an extension board extension point - don't mark it as purchased
                // The extension point on the extension board should remain unpurchased
                Debug.Log($"[BoardPositioningManager] ⏭️ SKIPPING EXTENSION BOARD EXTENSION POINT PURCHASE - Extension point on extension board should remain unpurchased");
            }
            
            // Hide the selection container after spawning
            if (selectionContainer != null)
            {
                selectionContainer.SetActive(false);
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] SelectionContainer hidden: {!selectionContainer.activeInHierarchy}");
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] ✅ BOARD SELECTION COMPLETED");
            }
        }

        /// <summary>
        /// Automatic extension point allocation coroutine for a specific board
        /// Ensures extension points are allocated immediately after board creation
        /// </summary>
        private IEnumerator AutomaticExtensionPointAllocation(GameObject targetBoard = null)
        {
            Debug.Log($"[BoardPositioningManager] 🔄 AUTOMATIC ALLOCATION: Starting automatic extension point allocation");
            
            // Wait a few frames for the board to be fully initialized
            yield return null;
            yield return null;
            yield return null;
            
            ExtensionBoardController targetBoardController = null;
            
            if (targetBoard != null)
            {
                // Use the specific board that was passed in
                targetBoardController = targetBoard.GetComponent<ExtensionBoardController>();
                if (targetBoardController == null)
                {
                    Debug.LogWarning($"[BoardPositioningManager] ⚠️ Target board {targetBoard.name} doesn't have ExtensionBoardController");
                    yield break;
                }
                Debug.Log($"[BoardPositioningManager] 🔄 AUTOMATIC ALLOCATION: Using specific target board {targetBoardController.GetBoardName()} at {targetBoardController.GetBoardGridPosition()}");
            }
            else
            {
                // Fallback: Find the most recently created extension board
                ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
                
                if (extensionBoards.Length == 0)
                {
                    Debug.LogWarning("[BoardPositioningManager] ⚠️ No extension boards found for automatic allocation");
                    yield break;
                }
                
                // Get the most recently created board (should be the last one)
                targetBoardController = extensionBoards[extensionBoards.Length - 1];
                Debug.Log($"[BoardPositioningManager] 🔄 AUTOMATIC ALLOCATION: Using fallback newest board {targetBoardController.GetBoardName()} at {targetBoardController.GetBoardGridPosition()}");
            }
            
            // Determine which extension point should be allocated
            Vector2Int targetExtensionPointPosition = GetCorrespondingExtensionPointPositionForBoard(targetBoardController.gameObject);
            
            if (targetExtensionPointPosition == Vector2Int.zero)
            {
                Debug.LogWarning($"[BoardPositioningManager] ⚠️ Could not determine extension point position for board {targetBoardController.GetBoardName()}");
                yield break;
            }
            
            Debug.Log($"[BoardPositioningManager] 🔄 AUTOMATIC ALLOCATION: Looking for extension point at {targetExtensionPointPosition}");
            
            // Find all cells in the board
            CellController_EXT[] extCells = targetBoardController.GetComponentsInChildren<CellController_EXT>();
            Debug.Log($"[BoardPositioningManager] 🔄 AUTOMATIC ALLOCATION: Found {extCells.Length} cells in board");
            
            foreach (CellController_EXT cell in extCells)
            {
                if (cell.GridPosition == targetExtensionPointPosition)
                {
                    Debug.Log($"[BoardPositioningManager] 🔄 AUTOMATIC ALLOCATION: Found extension point at {targetExtensionPointPosition} - setting as purchased");
                    
                    // Set as purchased and adjacent
                    cell.SetAdjacent(true);
                    cell.SetPurchased(true);
                    cell.SetAsExtensionPoint(true);
                    
                    // Force update visual state
                    cell.UpdateVisualState();
                    
                    // Unlock adjacent cells using board-specific logic
                    UnlockAdjacentNodesFromExtensionPoint(targetBoardController.gameObject, cell.GridPosition);
                    
                    Debug.Log($"[BoardPositioningManager] ✅ AUTOMATIC ALLOCATION: Extension point at {cell.GridPosition} successfully allocated");
                    Debug.Log($"[BoardPositioningManager] Final state - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
                    break;
                }
            }
            
            Debug.Log($"[BoardPositioningManager] ✅ AUTOMATIC ALLOCATION: Complete for board {targetBoardController.GetBoardName()}");
        }

        /// <summary>
        /// Test automatic extension point allocation on all extension boards
        /// </summary>
        [ContextMenu("Test Automatic Extension Point Allocation")]
        public void TestAutomaticExtensionPointAllocation()
        {
            Debug.Log("=== TESTING AUTOMATIC EXTENSION POINT ALLOCATION ===");
            StartCoroutine(AutomaticExtensionPointAllocation());
        }
        
        /// <summary>
        /// Mark the extension point that initiated the board selection as purchased
        /// </summary>
        private void MarkExtensionPointAsPurchased()
        {
            Debug.Log($"[BoardPositioningManager] 🔒 MARKING EXTENSION POINT AS PURCHASED - START");
            
            if (currentExtensionPointPosition == Vector2Int.zero)
            {
                Debug.LogError("[BoardPositioningManager] ❌ No extension point position stored - cannot mark as purchased");
                return;
            }
            
            Debug.Log($"[BoardPositioningManager] 🔍 Looking for extension point at stored position: {currentExtensionPointPosition}");
            
            // Find the specific extension point cell that was clicked
            CellController extensionCell = FindExtensionPointCell(currentExtensionPointPosition);
            
            if (extensionCell != null)
            {
                Debug.Log($"[BoardPositioningManager] ✅ Found extension point cell: {extensionCell.gameObject.name}");
                MarkCellAsPurchased(extensionCell);
            }
            else
            {
                Debug.LogError($"[BoardPositioningManager] ❌ Could not find extension point cell at {currentExtensionPointPosition}");
                Debug.LogError($"[BoardPositioningManager] This means the extension point purchasing is NOT working!");
            }
            
            Debug.Log($"[BoardPositioningManager] 🔒 MARKING EXTENSION POINT AS PURCHASED - END");
        }
        
        /// <summary>
        /// Mark a specific cell as purchased
        /// </summary>
        private void MarkCellAsPurchased(CellController cell)
        {
            Debug.Log($"[BoardPositioningManager] 📊 BEFORE - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
            
            // Mark the extension point as purchased and adjacent
            cell.SetPurchased(true);
            cell.SetAdjacent(true);
            
            // Force visual update
            cell.UpdateVisualState();
            
            // Update the data manager
            var pointData = ExtensionBoardDataManager.Instance.FindExtensionPointByPosition(cell.GridPosition);
            if (pointData != null)
            {
                ExtensionBoardDataManager.Instance.UpdateExtensionPointState(
                    pointData.pointId, 
                    true, // isPurchased
                    true, // isUnlocked
                    true  // isAvailable
                );
            }
            
            Debug.Log($"[BoardPositioningManager] 📊 AFTER - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
            Debug.Log($"[BoardPositioningManager] ✅ Extension point marked as purchased successfully!");
        }
        
        /// <summary>
        /// Find the extension point cell at the specified position
        /// </summary>
        private CellController FindExtensionPointCell(Vector2Int position)
        {
            Debug.Log($"[BoardPositioningManager] 🔍 FINDING EXTENSION POINT CELL at {position}");
            Debug.Log($"[BoardPositioningManager] 🔍 Source board: {(currentExtensionPointBoard != null ? currentExtensionPointBoard.name : "Core Board")}");
            
            // If we have a specific source board, search it first
            if (currentExtensionPointBoard != null)
            {
                Debug.Log($"[BoardPositioningManager] 🔍 Searching source board first: {currentExtensionPointBoard.name}");
                
                CellController[] cells = currentExtensionPointBoard.GetComponentsInChildren<CellController>();
                Debug.Log($"[BoardPositioningManager] 🔍 Found {cells.Length} cells on source board");
                
                foreach (CellController cell in cells)
                {
                    Debug.Log($"[BoardPositioningManager] 🔍 Checking source board cell: {cell.gameObject.name} at {cell.GridPosition}, NodeType: {cell.NodeType}");
                    
                    if (cell.GridPosition == position && cell.NodeType == NodeType.Extension)
                    {
                        Debug.Log($"[BoardPositioningManager] ✅ Found extension point cell on source board at {position}: {cell.gameObject.name}");
                        return cell;
                    }
                }
            }
            
            // If not found on source board or no source board specified, search the core board
            if (coreBoard != null)
            {
                Debug.Log($"[BoardPositioningManager] 🔍 Searching core board: {coreBoard.name}");
                
                CellController[] cells = coreBoard.GetComponentsInChildren<CellController>();
                Debug.Log($"[BoardPositioningManager] 🔍 Found {cells.Length} cells on core board");
                
                foreach (CellController cell in cells)
                {
                    Debug.Log($"[BoardPositioningManager] 🔍 Checking core board cell: {cell.gameObject.name} at {cell.GridPosition}, NodeType: {cell.NodeType}");
                    
                    if (cell.GridPosition == position && cell.NodeType == NodeType.Extension)
                    {
                        Debug.Log($"[BoardPositioningManager] ✅ Found extension point cell on core board at {position}: {cell.gameObject.name}");
                        return cell;
                    }
                }
            }
            
            // If not found on core board, search extension boards
            Debug.Log($"[BoardPositioningManager] 🔍 Extension point not found on core board, searching extension boards");
            
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            Debug.Log($"[BoardPositioningManager] 🔍 Found {extensionBoards.Length} extension boards");
            
            foreach (ExtensionBoardController board in extensionBoards)
            {
                if (board == null) continue;
                
                Debug.Log($"[BoardPositioningManager] 🔍 Searching extension board: {board.GetBoardName()}");
                
                CellController[] cells = board.GetAllCells().Values.ToArray();
                Debug.Log($"[BoardPositioningManager] 🔍 Found {cells.Length} cells on extension board {board.GetBoardName()}");
                
                foreach (CellController cell in cells)
                {
                    Debug.Log($"[BoardPositioningManager] 🔍 Checking extension board cell: {cell.gameObject.name} at {cell.GridPosition}, NodeType: {cell.NodeType}");
                    
                    if (cell.GridPosition == position && cell.NodeType == NodeType.Extension)
                    {
                        Debug.Log($"[BoardPositioningManager] ✅ Found extension point cell on extension board at {position}: {cell.gameObject.name}");
                        return cell;
                    }
                }
            }
            
            Debug.LogError($"[BoardPositioningManager] ❌ No extension point cell found at {position} on any board");
            Debug.LogError($"[BoardPositioningManager] This is why the extension point purchasing is not working!");
            return null;
        }
        
        /// <summary>
        /// Spawn an extension board prefab
        /// </summary>
        private void SpawnExtensionBoardPrefab(ExtensionBoardPrefabData boardData)
        {
            if (boardData?.boardPrefab == null)
            {
                Debug.LogError("[BoardPositioningManager] Cannot spawn board - boardData or boardPrefab is null!");
                return;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Spawning extension board: {boardData.boardName}");
            }
            
            // Ensure core board position is detected before positioning
            if (coreBoardWorldPosition == Vector3.zero)
            {
                DetectCoreBoardWorldPosition();
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] Detected core board position: {coreBoardWorldPosition}");
                }
            }
            
            // Instantiate the board prefab
            GameObject newBoard = Instantiate(boardData.boardPrefab);
            
            // Calculate the grid position for this extension board
            Vector2Int extensionGridPosition = CalculateExtensionGridPosition();
            
            // Check if a board already exists at this grid position
            if (positionedBoards.ContainsKey(extensionGridPosition))
            {
                Debug.LogError($"[BoardPositioningManager] ❌ Board already exists at grid position {extensionGridPosition}!");
                Debug.LogError($"[BoardPositioningManager] Cannot create duplicate board. Destroying new board.");
                Destroy(newBoard);
                return;
            }
            
            // Set the board name with proper grid position
            newBoard.name = $"ExtensionBoard_{boardData.boardName}_{extensionGridPosition.x}_{extensionGridPosition.y}";
            
            // Ensure the board has an ExtensionBoardController component
            ExtensionBoardController boardController = newBoard.GetComponent<ExtensionBoardController>();
            if (boardController == null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] ⚠️ Board prefab {boardData.boardName} doesn't have ExtensionBoardController component, adding it");
                }
                boardController = newBoard.AddComponent<ExtensionBoardController>();
            }
            
            // Set the board controller properties
            if (boardController != null)
            {
                boardController.SetBoardGridPosition(extensionGridPosition);
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] 📝 Set board grid position to {extensionGridPosition} for {boardData.boardName}");
                }
            }
            
            // Position the board using proper offset system
            Vector3 boardPosition = CalculateBoardPosition();
            newBoard.transform.position = boardPosition;
            
            // Add to the boards container
            if (boardsContainer != null)
            {
                newBoard.transform.SetParent(boardsContainer, false);
            }
            
            // Register the board in the positionedBoards dictionary
            positionedBoards[extensionGridPosition] = newBoard;
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] 📝 Registered board in positionedBoards at {extensionGridPosition}");
            }
            
            // Register the board in the ExtensionBoardDataManager
            string boardId = $"board_{extensionGridPosition.x}_{extensionGridPosition.y}";
            ExtensionBoardDataManager.Instance.RegisterExtensionBoard(boardId, boardData.boardName, extensionGridPosition, newBoard.transform.position);
            
            // Extension point purchasing is now handled in OnBoardSelected method
            // to distinguish between core board and extension board extension points
            
            // Setup the board with extension point auto-purchase
            SetupExtensionPointDirectly(newBoard);
            
            // Setup extension points for the new board
            SetupExtensionBoardExtensionPoints(newBoard, extensionGridPosition);
            
            // Ensure extension points on the new board are properly unlocked and available
            UnlockExtensionPointsOnNewBoard(newBoard);
            
            // AUTOMATIC EXTENSION POINT ALLOCATION - Pass the newly created board
            Debug.Log($"[BoardPositioningManager] 🚀 STARTING AUTOMATIC ALLOCATION COROUTINE for board: {newBoard.name}");
            StartCoroutine(AutomaticExtensionPointAllocation(newBoard));
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] ✅ Successfully spawned extension board: {boardData.boardName} at {boardPosition}");
                Debug.Log($"[BoardPositioningManager] Core board position: {coreBoardWorldPosition}");
                Debug.Log($"[BoardPositioningManager] Extension board offset: {extensionBoardOffset}");
                Debug.Log($"[BoardPositioningManager] 📊 Total positioned boards: {positionedBoards.Count}");
            }
        }
        
        /// <summary>
        /// Unlock extension points on a newly created board and allocate the corresponding extension point
        /// </summary>
        private void UnlockExtensionPointsOnNewBoard(GameObject board)
        {
            Debug.Log($"[BoardPositioningManager] 🔓 UNLOCKING extension points on new board: {board.name}");
            Debug.Log($"[BoardPositioningManager] 🔓 AUTOMATIC ALLOCATION - START");
            
            // Get all cells on the board
            CellController[] cells = board.GetComponentsInChildren<CellController>();
            Debug.Log($"[BoardPositioningManager] 🔓 Found {cells.Length} cells on board {board.name}");
            
            int unlockedCount = 0;
            CellController correspondingExtensionPoint = null;
            
            foreach (CellController cell in cells)
            {
                // Check if this is an extension point
                if (cell.NodeType == NodeType.Extension && cell.IsExtensionPoint)
                {
                    Debug.Log($"[BoardPositioningManager] 🔓 Unlocking extension point: {cell.gameObject.name} at {cell.GridPosition}");
                    
                    // Make adjacent but not purchased yet
                    cell.SetAdjacent(true);
                    cell.SetPurchased(false); // Not purchased yet
                    
                    // Force visual update
                    cell.UpdateVisualState();
                    
                    // Update data manager
                    var pointData = ExtensionBoardDataManager.Instance.FindExtensionPointByPosition(cell.GridPosition);
                    if (pointData != null)
                    {
                        ExtensionBoardDataManager.Instance.UpdateExtensionPointState(
                            pointData.pointId,
                            false, // isPurchased
                            true,  // isUnlocked
                            true   // isAvailable
                        );
                    }
                    
                    // Check if this is the corresponding extension point that should be purchased
                    if (IsCorrespondingExtensionPoint(cell))
                    {
                        correspondingExtensionPoint = cell;
                        Debug.Log($"[BoardPositioningManager] 🎯 Found corresponding extension point: {cell.gameObject.name} at {cell.GridPosition}");
                    }
                    
                    unlockedCount++;
                }
            }
            
            // Allocate the corresponding extension point if found
            if (correspondingExtensionPoint != null)
            {
                Debug.Log($"[BoardPositioningManager] 🎯 AUTOMATIC ALLOCATION - Found corresponding extension point: {correspondingExtensionPoint.gameObject.name}");
                AllocateCorrespondingExtensionPointOnNewBoard(correspondingExtensionPoint);
                Debug.Log($"[BoardPositioningManager] 🎯 AUTOMATIC ALLOCATION - COMPLETED");
            }
            else
            {
                Debug.LogWarning($"[BoardPositioningManager] ❌ AUTOMATIC ALLOCATION - NO CORRESPONDING EXTENSION POINT FOUND for board {board.name}");
            }
            
            Debug.Log($"[BoardPositioningManager] ✅ Unlocked {unlockedCount} extension points on board {board.name}");
            Debug.Log($"[BoardPositioningManager] 🔓 AUTOMATIC ALLOCATION - END");
        }
        
        /// <summary>
        /// Check if this extension point is the corresponding one that should be purchased
        /// </summary>
        private bool IsCorrespondingExtensionPoint(CellController cell)
        {
            Debug.Log($"[BoardPositioningManager] 🔍 CHECKING IF CORRESPONDING EXTENSION POINT: {cell.gameObject.name} at {cell.GridPosition}");
            
            // Get the extension point name from the cell
            string extensionPointName = GetExtensionPointNameFromCell(cell);
            if (string.IsNullOrEmpty(extensionPointName))
            {
                Debug.Log($"[BoardPositioningManager] ❌ No extension point name found for {cell.gameObject.name}");
                return false;
            }
            
            Debug.Log($"[BoardPositioningManager] 🔍 Cell extension point name: {extensionPointName}");
            
            // Get the extension point name that was clicked on the core board
            string clickedExtensionPointName = GetClickedExtensionPointName();
            if (string.IsNullOrEmpty(clickedExtensionPointName))
            {
                Debug.LogWarning("[BoardPositioningManager] ❌ Could not determine clicked extension point name");
                return false;
            }
            
            // Get the corresponding extension point name for the new board
            string correspondingName = GetCorrespondingExtensionPointName(clickedExtensionPointName);
            
            Debug.Log($"[BoardPositioningManager] 🔍 Checking if {extensionPointName} corresponds to {correspondingName} (clicked: {clickedExtensionPointName})");
            
            bool isCorresponding = extensionPointName == correspondingName;
            Debug.Log($"[BoardPositioningManager] 🔍 Result: {isCorresponding}");
            
            return isCorresponding;
        }
        
        /// <summary>
        /// Get the extension point name that was clicked on the core board
        /// </summary>
        private string GetClickedExtensionPointName()
        {
            Debug.Log($"[BoardPositioningManager] 🔍 GETTING CLICKED EXTENSION POINT NAME");
            Debug.Log($"[BoardPositioningManager] 🔍 Current extension point position: {currentExtensionPointPosition}");
            
            // Find the extension point cell that was clicked on the core board
            CellController clickedCell = FindExtensionPointCell(currentExtensionPointPosition);
            if (clickedCell == null)
            {
                Debug.LogWarning($"[BoardPositioningManager] ❌ Could not find clicked extension point cell at {currentExtensionPointPosition}");
                return null;
            }
            
            string extensionPointName = GetExtensionPointNameFromCell(clickedCell);
            Debug.Log($"[BoardPositioningManager] 🔍 Found clicked extension point name: {extensionPointName}");
            
            return extensionPointName;
        }
        
        /// <summary>
        /// Get the extension point name from a cell
        /// </summary>
        private string GetExtensionPointNameFromCell(CellController cell)
        {
            // First try to get from CellJsonData (for extension boards)
            var cellJsonData = cell.GetComponent<CellJsonData>();
            if (cellJsonData != null && !string.IsNullOrEmpty(cellJsonData.NodeName))
            {
                return cellJsonData.NodeName;
            }
            
            // Fallback to base class node name
            if (!string.IsNullOrEmpty(cell.NodeName))
            {
                return cell.NodeName;
            }
            
            return null;
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
        /// Allocate the corresponding extension point on the new board
        /// </summary>
        private void AllocateCorrespondingExtensionPointOnNewBoard(CellController extensionCell)
        {
            if (extensionCell == null)
            {
                Debug.LogWarning("[BoardPositioningManager] ❌ Extension cell is null");
                return;
            }
            
            Debug.Log($"[BoardPositioningManager] 🎯 ALLOCATING extension point: {extensionCell.gameObject.name} at {extensionCell.GridPosition}");
            Debug.Log($"[BoardPositioningManager] 🎯 BEFORE ALLOCATION - IsPurchased: {extensionCell.IsPurchased}, IsUnlocked: {extensionCell.IsUnlocked}, IsAvailable: {extensionCell.IsAvailable}");
            
            // Mark as purchased, unlocked, and available
            extensionCell.SetPurchased(true);
            extensionCell.SetAdjacent(true);
            extensionCell.SetAsExtensionPoint(true);
            
            Debug.Log($"[BoardPositioningManager] 🎯 AFTER ALLOCATION - IsPurchased: {extensionCell.IsPurchased}, IsUnlocked: {extensionCell.IsUnlocked}, IsAvailable: {extensionCell.IsAvailable}");
            
            // Force visual update
            extensionCell.UpdateVisualState();
            
            // Notify the extension board controller if this is a CellController_EXT
            if (extensionCell is CellController_EXT extCell)
            {
                var boardController = extCell.GetComponentInParent<ExtensionBoardController>();
                if (boardController != null)
                {
                    Debug.Log($"[BoardPositioningManager] 🎯 Notifying ExtensionBoardController of purchase for {extensionCell.GridPosition}");
                    boardController.OnCellPurchased(extensionCell.GridPosition);
                    
                    // Check state after ExtensionBoardController notification
                    Debug.Log($"[BoardPositioningManager] 🎯 AFTER EXTENSIONBOARDCONTROLLER - IsPurchased: {extensionCell.IsPurchased}, IsUnlocked: {extensionCell.IsUnlocked}, IsAvailable: {extensionCell.IsAvailable}");
                }
                else
                {
                    Debug.LogWarning($"[BoardPositioningManager] ❌ Could not find ExtensionBoardController for {extensionCell.gameObject.name}");
                }
            }
            
            // Update data manager
            var pointData = ExtensionBoardDataManager.Instance.FindExtensionPointByPosition(extensionCell.GridPosition);
            if (pointData != null)
            {
                ExtensionBoardDataManager.Instance.UpdateExtensionPointState(
                    pointData.pointId,
                    true,  // isPurchased
                    true,  // isUnlocked
                    true   // isAvailable
                );
            }
            
            Debug.Log($"[BoardPositioningManager] ✅ Extension point allocated successfully: {extensionCell.gameObject.name}");
            
            // Final state check
            Debug.Log($"[BoardPositioningManager] 🎯 FINAL STATE CHECK - IsPurchased: {extensionCell.IsPurchased}, IsUnlocked: {extensionCell.IsUnlocked}, IsAvailable: {extensionCell.IsAvailable}");
        }
        
        /// <summary>
        /// Calculate position for the new extension board using the proper offset system
        /// </summary>
        private Vector3 CalculateBoardPosition()
        {
            // Ensure core board position is detected
            if (coreBoardWorldPosition == Vector3.zero)
            {
                DetectCoreBoardWorldPosition();
            }
            
            // Get the grid position for this extension board
            Vector2Int gridPosition = CalculateExtensionGridPosition();
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Calculating board position for grid: {gridPosition}");
                Debug.Log($"[BoardPositioningManager] Core board world position: {coreBoardWorldPosition}");
                Debug.Log($"[BoardPositioningManager] Grid cell size: {gridCellSize}");
            }
            
            // Calculate position based on grid position
            // Each grid cell represents a board-sized area
            Vector3 gridOffset = new Vector3(
                gridPosition.x * gridCellSize.x,
                gridPosition.y * gridCellSize.y, // Y-axis positioning
                0 // Keep Z at same level
            );
            
            Vector3 boardPosition = coreBoardWorldPosition + gridOffset;
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Grid offset: {gridOffset}");
                Debug.Log($"[BoardPositioningManager] Final board position: {boardPosition}");
            }
            
            return boardPosition;
        }
        
        /// <summary>
        /// Calculate the grid position for the new extension board based on the extension point clicked
        /// </summary>
        private Vector2Int CalculateExtensionGridPosition()
        {
            // Get the current extension point that was clicked
            // This should be stored when the extension point click is handled
            Vector2Int extensionPointPosition = GetCurrentExtensionPointPosition();
            
            // Calculate grid position relative to core board (0,0)
            // Core board is at (0,0), extension boards go to adjacent positions
            Vector2Int gridPosition = CalculateGridPositionFromExtensionPoint(extensionPointPosition);
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Extension point at {extensionPointPosition} -> Grid position: {gridPosition}");
            }
            
            return gridPosition;
        }
        
        /// <summary>
        /// Get the position of the extension point that was clicked
        /// </summary>
        private Vector2Int GetCurrentExtensionPointPosition()
        {
            return currentExtensionPointPosition;
        }
        
        /// <summary>
        /// Calculate grid position based on which extension point was clicked
        /// Takes into account the source board's position for extension board extension points
        /// </summary>
        private Vector2Int CalculateGridPositionFromExtensionPoint(Vector2Int extensionPointPosition)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] 🔍 CALCULATING GRID POSITION FROM EXTENSION POINT");
                Debug.Log($"[BoardPositioningManager] Extension point position: {extensionPointPosition}");
                Debug.Log($"[BoardPositioningManager] Source board: {(currentExtensionPointBoard != null ? currentExtensionPointBoard.name : "Core Board")}");
            }
            
            // Get the source board's grid position
            Vector2Int sourceBoardPosition = Vector2Int.zero; // Default to core board position
            if (currentExtensionPointBoard != null)
            {
                ExtensionBoardController sourceBoardController = currentExtensionPointBoard.GetComponent<ExtensionBoardController>();
                if (sourceBoardController != null)
                {
                    sourceBoardPosition = sourceBoardController.GetBoardGridPosition();
                    if (showDebugInfo)
                    {
                        Debug.Log($"[BoardPositioningManager] Source board grid position: {sourceBoardPosition}");
                    }
                }
            }
            
            // Determine grid position based on which extension point was clicked
            // Extension points are at specific positions: (3,0), (0,3), (3,6), (6,3)
            Vector2Int relativeOffset = Vector2Int.zero; // Offset relative to source board
            
            if (extensionPointPosition == new Vector2Int(3, 0)) // South extension point
            {
                relativeOffset = new Vector2Int(0, -1); // Place board to the South of source board
                if (showDebugInfo) Debug.Log($"[BoardPositioningManager] South extension point -> Relative offset: {relativeOffset}");
            }
            else if (extensionPointPosition == new Vector2Int(0, 3)) // West extension point
            {
                relativeOffset = new Vector2Int(-1, 0); // Place board to the West of source board
                if (showDebugInfo) Debug.Log($"[BoardPositioningManager] West extension point -> Relative offset: {relativeOffset}");
            }
            else if (extensionPointPosition == new Vector2Int(3, 6)) // North extension point
            {
                relativeOffset = new Vector2Int(0, 1); // Place board to the North of source board
                if (showDebugInfo) Debug.Log($"[BoardPositioningManager] North extension point -> Relative offset: {relativeOffset}");
            }
            else if (extensionPointPosition == new Vector2Int(6, 3)) // East extension point
            {
                relativeOffset = new Vector2Int(1, 0); // Place board to the East of source board
                if (showDebugInfo) Debug.Log($"[BoardPositioningManager] East extension point -> Relative offset: {relativeOffset}");
            }
            else
            {
                Debug.LogError($"[BoardPositioningManager] ❌ Unknown extension point position: {extensionPointPosition}");
                Debug.LogError($"[BoardPositioningManager] Expected positions: (3,0), (0,3), (3,6), (6,3)");
                return sourceBoardPosition; // Default to source board position
            }
            
            // Calculate final grid position: source board position + relative offset
            Vector2Int gridPosition = sourceBoardPosition + relativeOffset;
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] ✅ Final grid position: {gridPosition}");
            }
            
            return gridPosition;
        }
        
        /// <summary>
        /// Get the actual center position of the core board
        /// </summary>
        private Vector2Int GetCoreBoardCenter()
        {
            // Try to get the core board center from the core board grid position
            if (coreBoard != null)
            {
                // The core board grid position should be the center
                return coreBoardGridPosition;
            }
            
            // Fallback: assume core board is at (0,0) in the grid
            return Vector2Int.zero;
        }
        
        
        /// <summary>
        /// Manually find and assign the SelectionContainer
        /// </summary>
        [ContextMenu("Find Selection Container")]
        public void FindSelectionContainer()
        {
            selectionContainer = GameObject.Find("SelectionContainer");
            if (selectionContainer != null)
            {
                Debug.Log("[BoardPositioningManager] ✅ Found SelectionContainer");
            }
            else
            {
                Debug.LogError("[BoardPositioningManager] ❌ SelectionContainer not found in scene!");
            }
        }
        
        
        
        /// <summary>
        /// Get the current board name from an extension point ID
        /// </summary>
        private string GetCurrentBoardNameFromExtensionPoint(ExtensionPoint extensionPoint)
        {
            // For extension board extension points, the ID format is: extension_{gridX}_{gridY}_{directionX}_{directionY}
            // We need to find the board that corresponds to this grid position
            if (extensionPoint.id.StartsWith("extension_"))
            {
                // Extract grid position from the extension point ID
                string[] idParts = extensionPoint.id.Split('_');
                if (idParts.Length >= 3)
                {
                    // Parse grid position
                    if (int.TryParse(idParts[1], out int gridX) && int.TryParse(idParts[2], out int gridY))
                    {
                        Vector2Int boardGridPosition = new Vector2Int(gridX, gridY);
                        
                        // Find the board at this grid position
                        if (positionedBoards.ContainsKey(boardGridPosition))
                        {
                            GameObject board = positionedBoards[boardGridPosition];
                            ExtensionBoardController controller = board.GetComponent<ExtensionBoardController>();
                            if (controller != null)
                            {
                                return controller.GetBoardName();
                            }
                        }
                    }
                }
            }
            
            // Fallback: return a generic name
            return "Unknown Board";
        }
        
        
        /// <summary>
        /// Handle board prefab selection from the prefab selection UI
        /// </summary>
        private void OnBoardPrefabSelected(ExtensionPoint extensionPoint, GameObject boardPrefab)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Board prefab selected: {boardPrefab.name} for extension point at {extensionPoint.position}");
            }
            
            // Mark the extension point as purchased
            MarkExtensionPointAsPurchased(extensionPoint);
            
            // Note: Event handling removed since we're using SelectionContainer directly
            
            // Spawn the selected board prefab
            SpawnExtensionBoardPrefab(extensionPoint, boardPrefab);
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
        /// Spawn an extension board prefab directly
        /// </summary>
        public bool SpawnExtensionBoardPrefab(ExtensionPoint extensionPoint, GameObject boardPrefab)
        {
            if (boardPrefab == null)
            {
                Debug.LogError("[BoardPositioningManager] Board prefab is null!");
                return false;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Attempting to spawn board prefab: '{boardPrefab.name}'");
            }
            
            // Calculate grid position for the extension board
            Vector2Int extensionGridPosition = new Vector2Int(extensionPoint.worldPosition.x, extensionPoint.worldPosition.y);
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] SpawnExtensionBoardPrefab - Extension Point Details:");
                Debug.Log($"  - Extension Point ID: {extensionPoint.id}");
                Debug.Log($"  - Extension Point Position: {extensionPoint.position}");
                Debug.Log($"  - Extension Point World Position: {extensionPoint.worldPosition}");
                Debug.Log($"  - Calculated Grid Position: {extensionGridPosition}");
                Debug.Log($"  - Board Prefab: {boardPrefab.name}");
            }
            
            // Check if position is already occupied
            if (positionedBoards.ContainsKey(extensionGridPosition))
            {
                Debug.LogWarning($"[BoardPositioningManager] Grid position {extensionGridPosition} is already occupied!");
                return false;
            }
            
            // Instantiate the board prefab
            GameObject extensionBoard = Instantiate(boardPrefab);
            extensionBoard.name = $"ExtensionBoard_{boardPrefab.name}_{extensionGridPosition.x}_{extensionGridPosition.y}";
            
            if (extensionBoard == null)
            {
                Debug.LogError($"[BoardPositioningManager] Failed to instantiate board prefab: {boardPrefab.name}");
                return false;
            }
            
            // Position the board
            Vector3 worldPosition = new Vector3(extensionGridPosition.x * gridCellSize.x, extensionGridPosition.y * gridCellSize.y, 0);
            extensionBoard.transform.position = worldPosition;
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] ✅ Board prefab instantiated and positioned at {worldPosition}");
            }
            
            // Register the board
            positionedBoards[extensionGridPosition] = extensionBoard;
            
            // Setup extension points for the new board
            SetupExtensionBoardExtensionPoints(extensionBoard, extensionGridPosition);
            
            // Allocate the corresponding extension point
            StartCoroutine(AllocateCorrespondingExtensionPointDelayed(extensionPoint, extensionBoard, extensionGridPosition));
            
            // NEW: Directly set up extension point at (0, 3) as purchased
            StartCoroutine(SetupExtensionPointDirectly(extensionBoard));
            
            return true;
        }
        
        
        /// <summary>
        /// Setup extension points for a newly created extension board
        /// </summary>
        private void SetupExtensionBoardExtensionPoints(GameObject board, Vector2Int gridPosition)
        {
            Debug.Log($"[BoardPositioningManager] 🔧 SETUP: Creating extension points for board at {gridPosition} (showDebugInfo: {showDebugInfo})");
            Debug.Log($"[BoardPositioningManager] 🔧 SETUP: Board name: {board.name}, Board active: {board.activeInHierarchy}");
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] 🔧 SETUP: Creating extension points for board at {gridPosition}");
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
            {
                Debug.Log($"[BoardPositioningManager] Setting up extension points for board at {gridPosition}");
                Debug.Log($"[BoardPositioningManager] Core board at {coreBoardGridPosition}");
                Debug.Log($"[BoardPositioningManager] From direction: {fromDirection}");
                Debug.Log($"[BoardPositioningManager] From direction normalized: {fromDirectionNormalized}");
            }
            
            foreach (Vector2Int direction in extensionDirections)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] Processing direction {direction}");
                }
                
                // Skip the direction this board came from
                if (direction == fromDirectionNormalized) 
                {
                    if (showDebugInfo)
                        Debug.Log($"[BoardPositioningManager] Skipping direction {direction} (came from this direction)");
                    continue;
                }
                
                // Calculate extension board grid position - use coordinate swapping for proper orthographic positioning
                Vector2Int extensionGridPosition;
                
                // Apply the same coordinate swapping logic used for core board extension points
                if (direction == Vector2Int.up) // North extension point
                {
                    extensionGridPosition = gridPosition + Vector2Int.right; // Create at East position
                }
                else if (direction == Vector2Int.down) // South extension point
                {
                    extensionGridPosition = gridPosition + Vector2Int.left; // Create at West position
                }
                else if (direction == Vector2Int.right) // East extension point
                {
                    extensionGridPosition = gridPosition + Vector2Int.up; // Create at North position
                }
                else // West extension point
                {
                    extensionGridPosition = gridPosition + Vector2Int.down; // Create at South position
                }
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] Extension board coordinate swapping: Direction {direction} → Grid position {extensionGridPosition} (from base {gridPosition})");
                }
                
                // Check if this position is already occupied
                if (positionedBoards.ContainsKey(extensionGridPosition)) 
                {
                    if (showDebugInfo)
                        Debug.Log($"[BoardPositioningManager] Skipping extension point at {direction} - position {extensionGridPosition} already occupied");
                    continue;
                }
                
                // Get the cell position for this extension point
                Vector2Int cellPosition = GetBoardEdgePosition(direction);
                Debug.Log($"[BoardPositioningManager] 🔍 SETUP: Checking for extension point at direction {direction}, cell position {cellPosition}");
                
                CellController[] allCells = board.GetComponentsInChildren<CellController>();
                Debug.Log($"[BoardPositioningManager] 🔍 SETUP: Found {allCells.Length} cells in board {board.name}");
                
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
                    Debug.Log($"[BoardPositioningManager] ✅ SETUP: Found cell at {cellPosition}, creating extension point");
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[BoardPositioningManager] Found cell at {cellPosition} with current type: {extensionCell.NodeType}");
                    }
                    
                    // ALWAYS set extension point cells to NodeType.Extension
                    // These positions (3,0), (3,6), (0,3), (6,3) are always extension points
                    extensionCell.SetNodeType(NodeType.Extension);
                    
                        // Set the correct NodeName for the extension point
                        string extensionPointName = GetCorrespondingExtensionPointNameForNewBoard(direction, gridPosition);
                        extensionCell.SetNodeName(extensionPointName);
                        Debug.Log($"[BoardPositioningManager] ✅ SETUP: Set NodeName to {extensionPointName} for cell at {cellPosition} (direction: {direction})");
                        
                        // Register the extension point with the data manager
                        string pointId = $"point_{gridPosition.x}_{gridPosition.y}_{direction.x}_{direction.y}";
                        string boardId = $"board_{gridPosition.x}_{gridPosition.y}";
                        ExtensionBoardDataManager.Instance.RegisterExtensionPoint(
                            pointId, 
                            extensionPointName, 
                            cellPosition, // Use local cell position, not board grid position
                            extensionCell.transform.position, 
                            boardId
                        );
                    
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
                    
                    Debug.Log($"[BoardPositioningManager] ✅ SETUP: Created extension point {extensionPoint.id} at {extensionPoint.position} (showDebugInfo: {showDebugInfo})");
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[BoardPositioningManager] ✅ SETUP: Created extension point at {extensionPoint.position}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[BoardPositioningManager] ❌ SETUP: No cell found at position {cellPosition} for extension point");
                    
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
            Debug.Log($"[BoardPositioningManager] 🔄 ALLOCATION: Starting for board at {newBoardGridPosition} from source {sourceExtensionPoint.id} (showDebugInfo: {showDebugInfo})");
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] 🔄 ALLOCATION: Starting for board at {newBoardGridPosition} from source {sourceExtensionPoint.id}");
            }
            
            // Determine the corresponding cell position based on the source extension point's cell position
            Vector2Int correspondingCellPosition = GetCorrespondingCellPosition(sourceExtensionPoint.position);
            
            Debug.Log($"[BoardPositioningManager] 🔄 ALLOCATION: Source extension point at {sourceExtensionPoint.position}, looking for cell {correspondingCellPosition} on board {newBoardGridPosition} (showDebugInfo: {showDebugInfo})");
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] 🔄 ALLOCATION: Looking for cell {correspondingCellPosition} on board {newBoardGridPosition}");
            }
            
            // Find the extension point on the new board that matches the corresponding cell position
            // Use more flexible matching to handle potential timing issues
            ExtensionPoint correspondingExtensionPoint = null;
            
            // First try exact match
            correspondingExtensionPoint = availableExtensionPoints.FirstOrDefault(ep => 
                ep.id.Contains($"extension_{newBoardGridPosition.x}_{newBoardGridPosition.y}") &&
                ep.position == correspondingCellPosition
            );
            
            Debug.Log($"[BoardPositioningManager] 🔍 ALLOCATION: Exact match result - Found: {correspondingExtensionPoint != null}, Total extension points: {availableExtensionPoints.Count}");
            
            if (showDebugInfo && correspondingExtensionPoint != null)
            {
                Debug.Log($"[BoardPositioningManager] ✅ ALLOCATION: Found extension point {correspondingExtensionPoint.id}");
            }
            
            // If not found, try to find any extension point on this board at the corresponding cell
            if (correspondingExtensionPoint == null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] ⚠️ ALLOCATION: Exact match failed, trying flexible match...");
                }
                
                // Get all extension points for this board
                var boardExtensionPoints = availableExtensionPoints.Where(ep => 
                    ep.id.Contains($"extension_{newBoardGridPosition.x}_{newBoardGridPosition.y}")).ToList();
                
                // Look for extension point at the corresponding cell position
                correspondingExtensionPoint = boardExtensionPoints.FirstOrDefault(ep => 
                    ep.position == correspondingCellPosition);
                    
                if (showDebugInfo && correspondingExtensionPoint != null)
                {
                    Debug.Log($"[BoardPositioningManager] ✅ ALLOCATION: Found via flexible match: {correspondingExtensionPoint.id}");
                }
            }
            
            if (correspondingExtensionPoint != null)
            {
                // Automatically allocate the corresponding extension point
                correspondingExtensionPoint.AddConnection();
                
                // Also purchase the corresponding cell on the new board
                PurchaseCorrespondingCell(newBoard, correspondingCellPosition);
                
                // Unlock adjacent nodes for orthographic interaction
                UnlockAdjacentNodesFromExtensionPoint(newBoard, correspondingCellPosition);
                
                Debug.Log($"[BoardPositioningManager] ✅ ALLOCATION: SUCCESS - Extension point {correspondingExtensionPoint.id} allocated and cell purchased");
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] ✅ ALLOCATION: SUCCESS - Extension point {correspondingExtensionPoint.id} allocated and cell purchased");
                }
            }
            else
            {
                // NEW: Try to unlock the extension point first, then retry allocation
                Debug.Log($"[BoardPositioningManager] ⚠️ ALLOCATION: Extension point not found, attempting to unlock first...");
                
                // First, unlock the corresponding cell to make it available
                UnlockCorrespondingCell(newBoard, correspondingCellPosition);
                
                // Wait a frame for the system to update
                StartCoroutine(RetryAllocationAfterUnlock(sourceExtensionPoint, newBoard, newBoardGridPosition, correspondingCellPosition));
            }
        }
        
        /// <summary>
        /// Unlock the corresponding cell to make it available for allocation
        /// </summary>
        private void UnlockCorrespondingCell(GameObject board, Vector2Int cellPosition)
        {
            if (board == null) return;
            
            Debug.Log($"[BoardPositioningManager] 🔓 UNLOCKING: Attempting to unlock cell at {cellPosition} on board {board.name}");
            
            // Find the cell at the specified position on the board
            CellController[] cells = board.GetComponentsInChildren<CellController>();
            CellController_EXT[] extCells = board.GetComponentsInChildren<CellController_EXT>();
            
            // First check CellController_EXT components (for extension boards)
            foreach (CellController_EXT extCell in extCells)
            {
                if (extCell.GridPosition == cellPosition)
                {
                    // Make the cell adjacent
                    extCell.SetAdjacent(true);
                    
                    Debug.Log($"[BoardPositioningManager] ✅ UNLOCKED: Cell at {cellPosition} is now unlocked and available");
                    return;
                }
            }
            
            // Fallback to regular CellController components
            foreach (CellController cell in cells)
            {
                if (cell.GridPosition == cellPosition)
                {
                    // Make the cell adjacent
                    cell.SetAdjacent(true);
                    
                    Debug.Log($"[BoardPositioningManager] ✅ UNLOCKED: Cell at {cellPosition} is now unlocked and available");
                    return;
                }
            }
            
            Debug.LogWarning($"[BoardPositioningManager] ❌ UNLOCK FAILED: Could not find cell at {cellPosition} on board {board.name}");
        }
        
        /// <summary>
        /// Retry allocation after unlocking the extension point
        /// </summary>
        private IEnumerator RetryAllocationAfterUnlock(ExtensionPoint sourceExtensionPoint, GameObject newBoard, Vector2Int newBoardGridPosition, Vector2Int correspondingCellPosition)
        {
            // Wait a frame for the system to update
            yield return null;
            
            Debug.Log($"[BoardPositioningManager] 🔄 RETRY: Attempting allocation after unlock for cell {correspondingCellPosition}");
            
            // Try to find the extension point again
            ExtensionPoint correspondingExtensionPoint = availableExtensionPoints.FirstOrDefault(ep => 
                ep.id.Contains($"extension_{newBoardGridPosition.x}_{newBoardGridPosition.y}") &&
                ep.position == correspondingCellPosition
            );
            
            if (correspondingExtensionPoint != null)
            {
                Debug.Log($"[BoardPositioningManager] ✅ RETRY SUCCESS: Found extension point {correspondingExtensionPoint.id}");
                
                // Automatically allocate the corresponding extension point
                correspondingExtensionPoint.AddConnection();
                
                // Also purchase the corresponding cell on the new board
                PurchaseCorrespondingCell(newBoard, correspondingCellPosition);
                
                // Unlock adjacent nodes for orthographic interaction
                UnlockAdjacentNodesFromExtensionPoint(newBoard, correspondingCellPosition);
                
                Debug.Log($"[BoardPositioningManager] ✅ RETRY ALLOCATION: SUCCESS - Extension point {correspondingExtensionPoint.id} allocated and cell purchased");
            }
            else
            {
                Debug.LogWarning($"[BoardPositioningManager] ❌ RETRY FAILED: Still could not find extension point at cell {correspondingCellPosition} on board {newBoardGridPosition}");
                
                // Show available extension points for debugging
                var boardExtensionPoints = availableExtensionPoints.Where(ep => ep.id.Contains($"extension_{newBoardGridPosition.x}_{newBoardGridPosition.y}")).ToList();
                Debug.Log($"[BoardPositioningManager] Available extension points for this board: {boardExtensionPoints.Count}");
                foreach (ExtensionPoint ep in boardExtensionPoints)
                {
                    Debug.Log($"  - {ep.id} at cell {ep.position}");
                }
            }
        }
        
        /// <summary>
        /// Load JSON data with a delay to ensure cells are fully initialized
        /// </summary>
        private IEnumerator LoadJsonDataDelayed(GameObject board, TextAsset jsonDataAsset)
        {
            // Wait one frame to ensure all cells are fully initialized
            yield return new WaitForEndOfFrame();
            
            // Verify that cells exist before attempting to load JSON data
            if (!VerifyCellsExist(board))
            {
                Debug.LogWarning($"[BoardPositioningManager] Cells not ready for board '{board.name}', retrying in next frame...");
                yield return new WaitForEndOfFrame();
                
                // Try one more time
                if (!VerifyCellsExist(board))
                {
                    Debug.LogError($"[BoardPositioningManager] Failed to find cells for board '{board.name}' after retry");
                    yield break;
                }
            }
            
            // Now load the JSON data
            LoadJsonDataToBoard(board, jsonDataAsset);
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] JSON data loaded, checking extension point cells...");
                
                // Check both CellController and CellController_EXT components
                CellController[] allCells = board.GetComponentsInChildren<CellController>();
                CellController_EXT[] allExtCells = board.GetComponentsInChildren<CellController_EXT>();
                
                foreach (CellController cell in allCells)
                {
                    if (cell.NodeType == NodeType.Extension)
                    {
                        Debug.Log($"[BoardPositioningManager] Found extension point cell at {cell.GridPosition} with type {cell.NodeType}");
                    }
                }
                
                foreach (CellController_EXT extCell in allExtCells)
                {
                    if (extCell.NodeType == NodeType.Extension)
                    {
                        Debug.Log($"[BoardPositioningManager] Found extension point CellController_EXT at {extCell.GridPosition} with type {extCell.NodeType}");
                    }
                }
            }
        }

        /// <summary>
        /// Verify that cells exist and are properly initialized on the board
        /// </summary>
        private bool VerifyCellsExist(GameObject board)
        {
            if (board == null) return false;
            
            // Check if ExtensionBoardCells child exists
            Transform extensionBoardCells = board.transform.Find("ExtensionBoardCells");
            if (extensionBoardCells == null)
            {
                Debug.LogWarning($"[BoardPositioningManager] ExtensionBoardCells child not found on board '{board.name}'");
                return false;
            }
            
            // Check if there are any cells in the ExtensionBoardCells child
            CellController[] cells = extensionBoardCells.GetComponentsInChildren<CellController>();
            CellController_EXT[] extCells = extensionBoardCells.GetComponentsInChildren<CellController_EXT>();
            
            int totalCells = cells.Length + extCells.Length;
            
            if (totalCells == 0)
            {
                Debug.LogWarning($"[BoardPositioningManager] No cells found in ExtensionBoardCells child of board '{board.name}'");
                return false;
            }
            
            // Verify that cells have valid GridPosition values
            int validCells = 0;
            foreach (CellController cell in cells)
            {
                if (cell != null && cell.GridPosition != Vector2Int.zero)
                {
                    validCells++;
                }
            }
            foreach (CellController_EXT extCell in extCells)
            {
                if (extCell != null && extCell.GridPosition != Vector2Int.zero)
                {
                    validCells++;
                }
            }
            
            if (validCells == 0)
            {
                Debug.LogWarning($"[BoardPositioningManager] No cells with valid GridPosition found on board '{board.name}'");
                return false;
            }
            
            Debug.Log($"[BoardPositioningManager] ✅ Verified {validCells} valid cells exist on board '{board.name}'");
            return true;
        }

        /// <summary>
        /// Load JSON data into a board using the centralized JsonBoardDataManager
        /// </summary>
        private void LoadJsonDataToBoard(GameObject board, TextAsset jsonDataAsset)
        {
            if (board == null || jsonDataAsset == null)
            {
                Debug.LogError("[BoardPositioningManager] Cannot load JSON data - board or JSON asset is null");
                return;
            }
            
            // Get the centralized JsonBoardDataManager (should be on the scene, not the board)
            JsonBoardDataManager jsonManager = FindFirstObjectByType<JsonBoardDataManager>();
            if (jsonManager == null)
            {
                Debug.LogError("[BoardPositioningManager] No JsonBoardDataManager found in scene! Please add one to manage JSON data.");
                return;
            }
            
            // Determine board ID from the JSON asset name
            string boardId = ExtractBoardIdFromJsonName(jsonDataAsset.name);
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Loading JSON data for board '{board.name}'");
                Debug.Log($"[BoardPositioningManager] JSON Asset Name: '{jsonDataAsset.name}'");
                Debug.Log($"[BoardPositioningManager] Extracted Board ID: '{boardId}'");
            }
            
            // Try to load using the configured board ID first
            bool success = jsonManager.LoadExtensionBoardData(board, boardId);
            
            // If that fails, try loading directly from the TextAsset
            if (!success)
            {
                Debug.Log($"[BoardPositioningManager] Board ID '{boardId}' not found in configured data, trying direct TextAsset loading...");
                success = jsonManager.LoadExtensionBoardDataFromTextAsset(board, jsonDataAsset);
            }
            
            Debug.Log($"[BoardPositioningManager] JSON loading result: {success} for board {board.name}");
            
            if (success)
            {
                Debug.Log($"[BoardPositioningManager] ✅ Successfully loaded JSON data for extension board '{board.name}' using centralized system");
                
                // Verify that extension point cells have correct NodeType
                CellController[] allCells = board.GetComponentsInChildren<CellController>();
                int extensionCells = 0;
                foreach (CellController cell in allCells)
                {
                    if (cell.NodeType == NodeType.Extension)
                    {
                        extensionCells++;
                        Debug.Log($"[BoardPositioningManager] Found extension cell at {cell.GridPosition}");
                    }
                }
                Debug.Log($"[BoardPositioningManager] Total extension cells found: {extensionCells}");
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardPositioningManager] ✅ Successfully loaded JSON data for extension board '{board.name}' using centralized system");
                }
            }
            else
            {
                Debug.LogError($"[BoardPositioningManager] ❌ Failed to load JSON data for extension board '{board.name}' with board ID: {boardId}");
            }
        }
        
        /// <summary>
        /// Extract board ID from JSON file name
        /// </summary>
        private string ExtractBoardIdFromJsonName(string jsonFileName)
        {
            // Remove .json extension
            string boardId = jsonFileName;
            if (boardId.EndsWith(".json"))
            {
                boardId = boardId.Substring(0, boardId.Length - 5);
            }
            
            // The board ID should match the JSON filename exactly
            // Examples: "T1FireBoard.json" -> "T1Fire_board", "T1ColdBoard.json" -> "T1Cold_board"
            // Convert "Board" to "_board" to match the expected format
            if (boardId.EndsWith("Board"))
            {
                boardId = boardId.Substring(0, boardId.Length - 5) + "_board";
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] Extracted board ID '{boardId}' from JSON filename '{jsonFileName}'");
            }
            
            return boardId;
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
            
            // Initialize all cells on the board (handle both CellController and CellController_EXT)
            CellController[] cells = board.GetComponentsInChildren<CellController>();
            CellController_EXT[] extCells = board.GetComponentsInChildren<CellController_EXT>();
            
            int totalCells = 0;
            
            // Initialize regular CellController components
            foreach (CellController cell in cells)
            {
                cell.Initialize(treeManager);
                totalCells++;
            }
            
            // Initialize CellController_EXT components
            foreach (CellController_EXT extCell in extCells)
            {
                // For extension board cells, initialize with the ExtensionBoardController
                ExtensionBoardController extensionController = board.GetComponent<ExtensionBoardController>();
                if (extensionController != null)
                {
                    extCell.Initialize(extensionController);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[BoardPositioningManager] Initialized CellController_EXT at {extCell.GridPosition} with ExtensionBoardController");
                    }
                }
                else
                {
                    Debug.LogError($"[BoardPositioningManager] No ExtensionBoardController found for CellController_EXT at {extCell.GridPosition}");
                }
                totalCells++;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] ✅ Initialized {totalCells} cells on board {board.name} ({cells.Length} regular, {extCells.Length} extension)");
            }
        }
        
        /// <summary>
        /// Configure the corresponding cell on the new board as an allocated extension point
        /// This cell should be automatically purchased/allocated to establish the connection
        /// </summary>
        private void PurchaseCorrespondingCell(GameObject board, Vector2Int cellPosition)
        {
            if (board == null) return;
            
            // Find the cell at the specified position on the board (check both CellController and CellController_EXT)
            CellController[] cells = board.GetComponentsInChildren<CellController>();
            CellController_EXT[] extCells = board.GetComponentsInChildren<CellController_EXT>();
            
            // First check CellController_EXT components (for extension boards)
            foreach (CellController_EXT extCell in extCells)
            {
                if (extCell.GridPosition == cellPosition)
                {
                    // Configure the cell as an allocated extension point (purchased)
                    extCell.SetAdjacent(true);
                    extCell.SetPurchased(true); // This is the key fix - mark as purchased
                    extCell.SetAsExtensionPoint(true);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[BoardPositioningManager] ✅ ALLOCATION: Cell at {cellPosition} configured as purchased");
                    }
                    return;
                }
            }
            
            // Fallback to regular CellController components
            foreach (CellController cell in cells)
            {
                if (cell.GridPosition == cellPosition)
                {
                    // Configure the cell as an allocated extension point (purchased)
                    cell.SetAdjacent(true);
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
            
            // Find all cells on the board (both CellController and CellController_EXT)
            CellController[] allCells = board.GetComponentsInChildren<CellController>();
            CellController_EXT[] allExtCells = board.GetComponentsInChildren<CellController_EXT>();
            Dictionary<Vector2Int, CellController> cellMap = new Dictionary<Vector2Int, CellController>();
            
            // Map regular CellController components
            foreach (CellController cell in allCells)
            {
                cellMap[cell.GridPosition] = cell;
            }
            
            // Map CellController_EXT components (these take precedence for extension boards)
            foreach (CellController_EXT extCell in allExtCells)
            {
                cellMap[extCell.GridPosition] = extCell; // CellController_EXT inherits from CellController
            }
            
            Debug.Log($"[BoardPositioningManager] Found {allCells.Length} CellController and {allExtCells.Length} CellController_EXT components on board");
            
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
                        adjacentCell.SetAdjacent(true);
                        
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
            cell.SetAdjacent(true);
            
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
            else if (sourceCellPosition == new Vector2Int(0, 1)) // West extension point on core (actual position)
            {
                return new Vector2Int(6, 3); // East cell on extension board
            }
            else if (sourceCellPosition == new Vector2Int(0, 3)) // West extension point on core (alternative position)
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
            Debug.Log($"[BoardPositioningManager] 🚀 ALLOCATION: Starting coroutine for board at {newBoardGridPosition} (showDebugInfo: {showDebugInfo})");
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] 🚀 ALLOCATION: Starting coroutine for board at {newBoardGridPosition}");
            }
            
            // Wait a few frames to ensure extension points are fully added to the list
            yield return null;
            yield return null;
            yield return null;
            
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
                Debug.Log($"[BoardPositioningManager] Core board grid position: {coreBoardGridPosition}");
                Debug.Log($"[BoardPositioningManager] Core board center: {GetCoreBoardCenter()}");
            }
            else
            {
                Debug.LogWarning("[BoardPositioningManager] Core board is null! Cannot detect position.");
            }
        }
        
        /// <summary>
        /// Debug extension point positioning
        /// </summary>
        [ContextMenu("Debug Extension Point Positioning")]
        public void DebugExtensionPointPositioning()
        {
            Debug.Log("=== EXTENSION POINT POSITIONING DEBUG ===");
            Debug.Log($"Core board center: {GetCoreBoardCenter()}");
            Debug.Log($"Current extension point position: {currentExtensionPointPosition}");
            
            if (currentExtensionPointPosition != Vector2Int.zero)
            {
                Vector2Int direction = currentExtensionPointPosition - GetCoreBoardCenter();
                Vector2Int gridPosition = CalculateGridPositionFromExtensionPoint(currentExtensionPointPosition);
                
                Debug.Log($"Direction vector: {direction}");
                Debug.Log($"Calculated grid position: {gridPosition}");
                
                // Test different extension point positions
                Debug.Log("=== TESTING DIFFERENT EXTENSION POINTS ===");
                TestExtensionPoint(new Vector2Int(3, 6), "North");
                TestExtensionPoint(new Vector2Int(6, 3), "East");
                TestExtensionPoint(new Vector2Int(3, 0), "South");
                TestExtensionPoint(new Vector2Int(0, 3), "West");
            }
            else
            {
                Debug.LogWarning("No extension point position stored. Click an extension point first.");
            }
        }
        
        /// <summary>
        /// Debug core board positioning and fix if needed
        /// </summary>
        [ContextMenu("Debug Core Board Positioning")]
        public void DebugCoreBoardPositioning()
        {
            Debug.Log("=== CORE BOARD POSITIONING DEBUG ===");
            
            if (coreBoard != null)
            {
                Vector3 currentPosition = coreBoard.transform.position;
                Debug.Log($"Core board current position: {currentPosition}");
                Debug.Log($"Core board world position stored: {coreBoardWorldPosition}");
                Debug.Log($"Core board grid position: {coreBoardGridPosition}");
                
                // Check if core board is at origin
                if (currentPosition != Vector3.zero)
                {
                    Debug.LogWarning($"Core board is not at origin! Current position: {currentPosition}");
                    Debug.Log("This will cause extension boards to be positioned incorrectly.");
                    Debug.Log("Consider moving the core board to (0,0,0) or adjusting the positioning logic.");
                }
                else
                {
                    Debug.Log("✅ Core board is at origin - positioning should work correctly.");
                }
                
                // Show what extension board positions would be
                Debug.Log("=== EXTENSION BOARD POSITIONS ===");
                TestExtensionBoardPosition(new Vector2Int(0, 1), "North");
                TestExtensionBoardPosition(new Vector2Int(1, 0), "East");
                TestExtensionBoardPosition(new Vector2Int(0, -1), "South");
                TestExtensionBoardPosition(new Vector2Int(-1, 0), "West");
            }
            else
            {
                Debug.LogError("Core board is null! Cannot debug positioning.");
            }
        }
        
        /// <summary>
        /// Test extension board position calculation
        /// </summary>
        private void TestExtensionBoardPosition(Vector2Int gridPos, string direction)
        {
            Vector3 gridOffset = new Vector3(
                gridPos.x * gridCellSize.x,
                gridPos.y * gridCellSize.y, // Y-axis positioning
                0 // Keep Z at same level
            );
            Vector3 finalPosition = coreBoardWorldPosition + gridOffset;
            Debug.Log($"{direction} board at grid {gridPos} -> World position: {finalPosition}");
        }
        
        /// <summary>
        /// Test extension point positioning
        /// </summary>
        private void TestExtensionPoint(Vector2Int position, string direction)
        {
            Vector2Int gridPos = CalculateGridPositionFromExtensionPoint(position);
            Debug.Log($"{direction} extension point at {position} -> Grid position: {gridPos}");
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
        /// Debug method to show board positioning information
        /// </summary>
        [ContextMenu("Debug Board Positioning")]
        public void DebugBoardPositioning()
        {
            Debug.Log("=== BOARD POSITIONING DEBUG ===");
            
            // Core board info
            Debug.Log($"Core Board Grid Position: {coreBoardGridPosition}");
            Debug.Log($"Core Board World Position: {coreBoardWorldPosition}");
            Debug.Log($"Grid Cell Size: {gridCellSize}");
            
            // Show positioned boards
            Debug.Log($"Positioned Boards Count: {positionedBoards.Count}");
            foreach (var kvp in positionedBoards)
            {
                Vector3 worldPos = kvp.Value.transform.position;
                Debug.Log($"Board at grid {kvp.Key}: {kvp.Value.name} at world position {worldPos}");
            }
            
            // Test extension board positions
            Debug.Log("=== TESTING EXTENSION BOARD POSITIONS ===");
            TestExtensionBoardPosition(new Vector2Int(1, 0), "East");
            TestExtensionBoardPosition(new Vector2Int(-1, 0), "West");
            TestExtensionBoardPosition(new Vector2Int(0, 1), "North");
            TestExtensionBoardPosition(new Vector2Int(0, -1), "South");
            
            Debug.Log("=== BOARD POSITIONING DEBUG COMPLETE ===");
        }
        
        
        /// <summary>
        /// Debug method to show all positioned boards and their grid positions
        /// </summary>
        [ContextMenu("Debug All Positioned Boards")]
        public void DebugAllPositionedBoards()
        {
            Debug.Log("=== DEBUGGING ALL POSITIONED BOARDS ===");
            Debug.Log($"Total positioned boards: {positionedBoards.Count}");
            
            foreach (var kvp in positionedBoards)
            {
                Vector2Int gridPos = kvp.Key;
                GameObject board = kvp.Value;
                Vector3 worldPos = board.transform.position;
                
                Debug.Log($"Board: {board.name}");
                Debug.Log($"  Grid Position: {gridPos}");
                Debug.Log($"  World Position: {worldPos}");
                Debug.Log($"  Active: {board.activeInHierarchy}");
            }
            
            Debug.Log("=== POSITIONED BOARDS DEBUG COMPLETE ===");
        }
        
        /// <summary>
        /// Debug method to show the current extension point position and search for it
        /// </summary>
        [ContextMenu("Debug Extension Point Allocation")]
        public void DebugExtensionPointAllocation()
        {
            Debug.Log("=== DEBUGGING EXTENSION POINT ALLOCATION ===");
            Debug.Log($"Current extension point position: {currentExtensionPointPosition}");
            
            if (currentExtensionPointPosition == Vector2Int.zero)
            {
                Debug.LogWarning("No extension point position stored. Click an extension point first.");
                return;
            }
            
            // Get the clicked extension point name
            string clickedName = GetClickedExtensionPointName();
            Debug.Log($"Clicked extension point name: {clickedName}");
            
            if (!string.IsNullOrEmpty(clickedName))
            {
                string correspondingName = GetCorrespondingExtensionPointName(clickedName);
                Debug.Log($"Corresponding extension point name: {correspondingName}");
            }
            
            Debug.Log("=== EXTENSION POINT ALLOCATION DEBUG COMPLETE ===");
        }

        [ContextMenu("Debug Current Extension Point Position")]
        public void DebugCurrentExtensionPointPosition()
        {
            Debug.Log("=== DEBUGGING CURRENT EXTENSION POINT POSITION ===");
            Debug.Log($"Current extension point position: {currentExtensionPointPosition}");
            
            if (currentExtensionPointPosition == Vector2Int.zero)
            {
                Debug.LogWarning("No extension point position stored. Click an extension point first.");
                return;
            }
            
            // Search for the extension point at this position
            CellController foundCell = FindExtensionPointCell(currentExtensionPointPosition);
            
            if (foundCell != null)
            {
                Debug.Log($"✅ Found extension point cell: {foundCell.gameObject.name}");
                Debug.Log($"  - Grid Position: {foundCell.GridPosition}");
                Debug.Log($"  - Node Type: {foundCell.NodeType}");
                Debug.Log($"  - Is Purchased: {foundCell.IsPurchased}");
            }
            else
            {
                Debug.LogError($"❌ Could not find extension point cell at {currentExtensionPointPosition}");
                
                // Show all extension points on all boards
                DebugAllBoardsExtensionPoints();
            }
            
            Debug.Log("=== CURRENT EXTENSION POINT POSITION DEBUG COMPLETE ===");
        }
        
        /// <summary>
        /// Debug method to show extension points on all boards (core and extension)
        /// </summary>
        [ContextMenu("Debug All Boards Extension Points")]
        public void DebugAllBoardsExtensionPoints()
        {
            Debug.Log("=== DEBUGGING ALL BOARDS EXTENSION POINTS ===");
            
            // Check core board
            if (coreBoard != null)
            {
                Debug.Log($"--- CORE BOARD: {coreBoard.name} ---");
                CellController[] cells = coreBoard.GetComponentsInChildren<CellController>();
                int extensionCount = 0;
                
                foreach (CellController cell in cells)
                {
                    if (cell.NodeType == NodeType.Extension)
                    {
                        extensionCount++;
                        Debug.Log($"Core Extension Point {extensionCount}: {cell.gameObject.name} at {cell.GridPosition}");
                        Debug.Log($"  - IsPurchased: {cell.IsPurchased}");
                        Debug.Log($"  - IsUnlocked: {cell.IsUnlocked}");
                        Debug.Log($"  - IsAvailable: {cell.IsAvailable}");
                    }
                }
                Debug.Log($"Core board has {extensionCount} extension points");
            }
            
            // Check extension boards
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            Debug.Log($"--- EXTENSION BOARDS: {extensionBoards.Length} found ---");
            
            foreach (ExtensionBoardController board in extensionBoards)
            {
                if (board == null) continue;
                
                Debug.Log($"Extension Board: {board.GetBoardName()}");
                var cells = board.GetAllCells();
                int extensionCount = 0;
                
                foreach (var kvp in cells)
                {
                    CellController cell = kvp.Value;
                    if (cell.NodeType == NodeType.Extension)
                    {
                        extensionCount++;
                        Debug.Log($"  Extension Point {extensionCount}: {cell.gameObject.name} at {cell.GridPosition}");
                        Debug.Log($"    - IsPurchased: {cell.IsPurchased}");
                        Debug.Log($"    - IsUnlocked: {cell.IsUnlocked}");
                        Debug.Log($"    - IsAvailable: {cell.IsAvailable}");
                    }
                }
                Debug.Log($"  Board {board.GetBoardName()} has {extensionCount} extension points");
            }
            
            Debug.Log($"Current extension point position: {currentExtensionPointPosition}");
            Debug.Log("=== ALL BOARDS EXTENSION POINTS DEBUG COMPLETE ===");
        }
        
        /// <summary>
        /// Force mark all extension points as purchased (emergency method)
        /// </summary>
        [ContextMenu("Force Mark All Extension Points as Purchased")]
        public void ForceMarkAllExtensionPointsAsPurchased()
        {
            Debug.Log("=== FORCE MARKING ALL EXTENSION POINTS AS PURCHASED ===");
            
            if (coreBoard == null)
            {
                Debug.LogError("❌ Core board is null!");
                return;
            }
            
            CellController[] cells = coreBoard.GetComponentsInChildren<CellController>();
            int markedCount = 0;
            
            foreach (CellController cell in cells)
            {
                if (cell.NodeType == NodeType.Extension)
                {
                    Debug.Log($"Marking extension point: {cell.gameObject.name} at {cell.GridPosition}");
                    cell.SetPurchased(true);
                    cell.SetAdjacent(true);
                    cell.UpdateVisualState();
                    markedCount++;
                }
            }
            
            Debug.Log($"✅ Marked {markedCount} extension points as purchased");
            Debug.Log("=== FORCE MARKING COMPLETE ===");
        }
        
        /// <summary>
        /// Debug method to check all extension points on the core board
        /// </summary>
        [ContextMenu("Debug All Extension Points")]
        public void DebugAllExtensionPoints()
        {
            Debug.Log("=== DEBUGGING ALL EXTENSION POINTS ===");
            
            if (coreBoard == null)
            {
                Debug.LogError("❌ Core board is null!");
                return;
            }
            
            CellController[] cells = coreBoard.GetComponentsInChildren<CellController>();
            Debug.Log($"Found {cells.Length} cells on core board");
            
            int extensionPointCount = 0;
            foreach (CellController cell in cells)
            {
                if (cell.NodeType == NodeType.Extension)
                {
                    extensionPointCount++;
                    Debug.Log($"Extension Point {extensionPointCount}: {cell.gameObject.name} at {cell.GridPosition}");
                    Debug.Log($"  - IsPurchased: {cell.IsPurchased}");
                    Debug.Log($"  - IsUnlocked: {cell.IsUnlocked}");
                    Debug.Log($"  - IsAvailable: {cell.IsAvailable}");
                    Debug.Log($"  - NodeType: {cell.NodeType}");
                }
            }
            
            Debug.Log($"Total extension points found: {extensionPointCount}");
            Debug.Log($"Current extension point position: {currentExtensionPointPosition}");
            Debug.Log("=== EXTENSION POINTS DEBUG COMPLETE ===");
        }
        
        /// <summary>
        /// Test the complete board creation and extension point purchasing flow
        /// </summary>
        [ContextMenu("Test Complete Board Creation Flow")]
        public void TestCompleteBoardCreationFlow()
        {
            Debug.Log("=== TESTING COMPLETE BOARD CREATION FLOW ===");
            
            // Check current extension points
            DebugAllExtensionPoints();
            
            // Test marking extension point as purchased
            Debug.Log("Testing extension point purchasing...");
            MarkExtensionPointAsPurchased();
            
            // Check extension points again
            Debug.Log("Checking extension points after purchasing...");
            DebugAllExtensionPoints();
            
            Debug.Log("=== COMPLETE BOARD CREATION FLOW TEST COMPLETE ===");
        }
        
        /// <summary>
        /// Test marking the current extension point as purchased
        /// </summary>
        [ContextMenu("Test Mark Extension Point as Purchased")]
        public void TestMarkExtensionPointAsPurchased()
        {
            Debug.Log("=== TESTING EXTENSION POINT PURCHASING ===");
            
            if (currentExtensionPointPosition == Vector2Int.zero)
            {
                Debug.LogWarning("No extension point position stored. Click an extension point first.");
                return;
            }
            
            Debug.Log($"Current extension point position: {currentExtensionPointPosition}");
            
            // Find the extension point cell
            CellController extensionCell = FindExtensionPointCell(currentExtensionPointPosition);
            
            if (extensionCell != null)
            {
                Debug.Log($"Found extension point cell: {extensionCell.gameObject.name}");
                Debug.Log($"Current state - IsPurchased: {extensionCell.IsPurchased}, IsUnlocked: {extensionCell.IsUnlocked}, IsAvailable: {extensionCell.IsAvailable}");
                
                // Mark as purchased
                MarkExtensionPointAsPurchased();
                
                Debug.Log($"After marking - IsPurchased: {extensionCell.IsPurchased}, IsUnlocked: {extensionCell.IsUnlocked}, IsAvailable: {extensionCell.IsAvailable}");
                Debug.Log("✅ Extension point marked as purchased successfully!");
            }
            else
            {
                Debug.LogError($"❌ Could not find extension point cell at {currentExtensionPointPosition}");
            }
            
            Debug.Log("=== EXTENSION POINT PURCHASING TEST COMPLETE ===");
        }
        
        /// <summary>
        /// Debug method to show all extension points and their allocation status
        /// </summary>
        [ContextMenu("Debug Extension Point Status")]
        public void DebugExtensionPointStatus()
        {
            Debug.Log("=== EXTENSION POINT STATUS DEBUG ===");
            Debug.Log($"Total Extension Points: {availableExtensionPoints.Count}");
            
            foreach (ExtensionPoint ep in availableExtensionPoints)
            {
                string status = ep.CanConnect() ? "Available" : "Allocated";
                Debug.Log($"Extension Point: {ep.id} - Status: {status} - Connections: {ep.currentConnections}/{ep.maxConnections}");
            }
            
            Debug.Log("=== END EXTENSION POINT STATUS DEBUG ===");
        }
        
        [ContextMenu("Debug Extension Point Allocation Process")]
        public void DebugExtensionPointAllocationProcess()
        {
            Debug.Log("=== DEBUGGING EXTENSION POINT ALLOCATION PROCESS ===");
            
            // Get the clicked extension point name
            string clickedName = GetClickedExtensionPointName();
            Debug.Log($"Clicked extension point name: {clickedName}");
            
            if (!string.IsNullOrEmpty(clickedName))
            {
                string correspondingName = GetCorrespondingExtensionPointName(clickedName);
                Debug.Log($"Corresponding extension point name: {correspondingName}");
                
                // Find all extension boards and check their extension points
                var extensionBoards = FindObjectsOfType<ExtensionBoardController>();
                Debug.Log($"Found {extensionBoards.Length} extension boards");
                
                foreach (var board in extensionBoards)
                {
                    Debug.Log($"Checking board: {board.name}");
                    var cells = board.GetAllCells();
                    
                    foreach (var kvp in cells)
                    {
                        var cell = kvp.Value;
                        if (cell.NodeType == NodeType.Extension && cell.IsExtensionPoint)
                        {
                            string cellExtensionName = GetExtensionPointNameFromCell(cell);
                            bool isCorresponding = IsCorrespondingExtensionPoint(cell);
                            
                            Debug.Log($"Extension point: {cell.gameObject.name} at {cell.GridPosition} - " +
                                     $"Name: {cellExtensionName}, IsCorresponding: {isCorresponding}, " +
                                     $"Purchased: {cell.IsPurchased}, Unlocked: {cell.IsUnlocked}, Available: {cell.IsAvailable}");
                        }
                    }
                }
            }
            
            Debug.Log("=== EXTENSION POINT ALLOCATION PROCESS DEBUG COMPLETE ===");
        }
        
        [ContextMenu("Force Allocate Extension Point at (0,3)")]
        public void ForceAllocateExtensionPointAtZeroThree()
        {
            Debug.Log("=== FORCE ALLOCATING EXTENSION POINT AT (0,3) ===");
            
            // Find all extension boards
            var extensionBoards = FindObjectsOfType<ExtensionBoardController>();
            Debug.Log($"Found {extensionBoards.Length} extension boards");
            
            foreach (var board in extensionBoards)
            {
                Debug.Log($"Checking board: {board.name}");
                var cells = board.GetAllCells();
                
                foreach (var kvp in cells)
                {
                    var cell = kvp.Value;
                    if (cell.GridPosition == new Vector2Int(0, 3) && cell.NodeType == NodeType.Extension)
                    {
                        Debug.Log($"Found extension point at (0,3): {cell.gameObject.name}");
                        Debug.Log($"BEFORE - Purchased: {cell.IsPurchased}, Unlocked: {cell.IsUnlocked}, Available: {cell.IsAvailable}");
                        
                        // Force allocation
                        AllocateCorrespondingExtensionPointOnNewBoard(cell);
                        
                        Debug.Log($"AFTER - Purchased: {cell.IsPurchased}, Unlocked: {cell.IsUnlocked}, Available: {cell.IsAvailable}");
                        return;
                    }
                }
            }
            
            Debug.LogWarning("No extension point found at (0,3) on any extension board");
            Debug.Log("=== FORCE ALLOCATION COMPLETE ===");
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
        /// Manually set up extension point as purchased and unlock adjacent cells
        /// This is a simple fallback method to ensure extension points work
        /// </summary>
        [ContextMenu("Force Extension Point Setup")]
        public void ForceExtensionPointSetup()
        {
            Debug.Log("=== FORCING EXTENSION POINT SETUP ===");
            
            // Find all extension boards
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            Debug.Log($"[BoardPositioningManager] Found {extensionBoards.Length} extension boards");
            
            foreach (ExtensionBoardController boardController in extensionBoards)
            {
                Debug.Log($"[BoardPositioningManager] Processing extension board: {boardController.gameObject.name}");
                
                // Find all cells in this board
                CellController_EXT[] extCells = boardController.gameObject.GetComponentsInChildren<CellController_EXT>();
                Debug.Log($"[BoardPositioningManager] Found {extCells.Length} cells in board");
                
                foreach (CellController_EXT cell in extCells)
                {
                    Debug.Log($"[BoardPositioningManager] Checking cell at {cell.GridPosition}: Type={cell.NodeType}, Name={cell.gameObject.name}, NodeName={cell.NodeName}");
                    
                    // Check if this is an extension point by node type OR by name
                    bool isExtensionPoint = (cell.NodeType == NodeType.Extension) || 
                                          (cell.NodeName != null && cell.NodeName.Contains("Extension"));
                    
                    if (isExtensionPoint)
                    {
                        Debug.Log($"[BoardPositioningManager] Setting up extension point at {cell.GridPosition} (Type: {cell.NodeType}, Name: {cell.NodeName})");
                        
                        // Set as purchased
                        cell.SetAdjacent(true);
                        cell.SetPurchased(true);
                        cell.SetAsExtensionPoint(true);
                        
                        // Unlock adjacent cells
                        UnlockAdjacentNodesFromExtensionPoint(boardController.gameObject, cell.GridPosition);
                        
                        Debug.Log($"[BoardPositioningManager] ✅ Extension point at {cell.GridPosition} set up successfully");
                    }
                    else
                    {
                        Debug.Log($"[BoardPositioningManager] Skipping cell at {cell.GridPosition} - not an extension point");
                    }
                }
            }
            
            Debug.Log("=== EXTENSION POINT SETUP COMPLETE ===");
        }
        
        /// <summary>
        /// Coroutine to directly set up the corresponding extension point after board creation
        /// Works with all orientations (North, South, East, West)
        /// </summary>
        private IEnumerator SetupExtensionPointDirectly(GameObject extensionBoard)
        {
            // Wait a few frames for the board to be fully initialized
            yield return null;
            yield return null;
            yield return null;
            
            Debug.Log($"[BoardPositioningManager] 🔧 DIRECT SETUP: Setting up corresponding extension point on {extensionBoard.name}");
            
            // Determine which extension point to allocate based on the board's position
            Vector2Int targetExtensionPointPosition = GetCorrespondingExtensionPointPositionForBoard(extensionBoard);
            
            if (targetExtensionPointPosition == Vector2Int.zero)
            {
                Debug.LogWarning($"[BoardPositioningManager] ⚠️ Could not determine extension point position for board {extensionBoard.name}");
                yield break;
            }
            
            Debug.Log($"[BoardPositioningManager] 🔧 DIRECT SETUP: Looking for extension point at {targetExtensionPointPosition} on {extensionBoard.name}");
            
            // Find all cells in this board
            CellController_EXT[] extCells = extensionBoard.GetComponentsInChildren<CellController_EXT>();
            Debug.Log($"[BoardPositioningManager] Found {extCells.Length} cells in board");
            
            foreach (CellController_EXT cell in extCells)
            {
                Debug.Log($"[BoardPositioningManager] Checking cell at {cell.GridPosition}: Type={cell.NodeType}, Name={cell.gameObject.name}, NodeName={cell.NodeName}");
                
                // Target the dynamically determined extension point position
                if (cell.GridPosition == targetExtensionPointPosition)
                {
                    Debug.Log($"[BoardPositioningManager] Found extension point at {targetExtensionPointPosition} - setting as purchased");
                    
                    // Set as purchased and adjacent
                    cell.SetAdjacent(true);
                    cell.SetPurchased(true);
                    cell.SetAsExtensionPoint(true);
                    
                    // Unlock adjacent cells
                    UnlockAdjacentNodesFromExtensionPoint(extensionBoard, cell.GridPosition);
                    
                    Debug.Log($"[BoardPositioningManager] ✅ Cell at {cell.GridPosition} set as purchased");
                    Debug.Log($"[BoardPositioningManager] Final state - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
                    break; // Found and set up the extension point, we're done
                }
            }
            
            Debug.Log($"[BoardPositioningManager] 🔧 DIRECT SETUP: Complete for {extensionBoard.name}");
        }

        /// <summary>
        /// Get the corresponding extension point position for a newly created board
        /// Determines which extension point should be allocated based on the board's position relative to core
        /// </summary>
        private Vector2Int GetCorrespondingExtensionPointPositionForBoard(GameObject extensionBoard)
        {
            // Get the board controller to determine the board's grid position
            ExtensionBoardController boardController = extensionBoard.GetComponent<ExtensionBoardController>();
            if (boardController == null)
            {
                Debug.LogError($"[BoardPositioningManager] ❌ Board {extensionBoard.name} doesn't have ExtensionBoardController");
                return Vector2Int.zero;
            }

            Vector2Int boardGridPosition = boardController.GetBoardGridPosition();
            
            // Get the source extension point position that was clicked to create this board
            Vector2Int sourceExtensionPointPosition = GetCurrentExtensionPointPosition();
            
            // Use the existing GetCorrespondingCellPosition logic to determine which extension point to allocate
            Vector2Int correspondingExtensionPointPosition = GetCorrespondingCellPosition(sourceExtensionPointPosition);
            
            if (showDebugInfo)
            {
                Debug.Log($"[BoardPositioningManager] 🔧 Board at {boardGridPosition} created from extension point at {sourceExtensionPointPosition}");
                Debug.Log($"[BoardPositioningManager] 🔧 Corresponding extension point position: {correspondingExtensionPointPosition}");
            }
            
            return correspondingExtensionPointPosition;
        }
        
        /// <summary>
        /// Directly purchase extension point at (0,3) on all extension boards
        /// This bypasses the complex allocation system and just sets it as purchased
        /// </summary>
        [ContextMenu("Direct Purchase Extension Point (0,3)")]
        public void DirectPurchaseExtensionPoint()
        {
            Debug.Log("=== DIRECT PURCHASE EXTENSION POINT (0,3) ===");
            
            // Find all extension boards
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            Debug.Log($"[BoardPositioningManager] Found {extensionBoards.Length} extension boards");
            
            foreach (ExtensionBoardController boardController in extensionBoards)
            {
                Debug.Log($"[BoardPositioningManager] Processing extension board: {boardController.gameObject.name}");
                
                // Find all cells in this board
                CellController_EXT[] extCells = boardController.gameObject.GetComponentsInChildren<CellController_EXT>();
                Debug.Log($"[BoardPositioningManager] Found {extCells.Length} cells in board");
                
                foreach (CellController_EXT cell in extCells)
                {
                    Debug.Log($"[BoardPositioningManager] Checking cell at {cell.GridPosition}: Type={cell.NodeType}, Name={cell.gameObject.name}, NodeName={cell.NodeName}");
                    
                // Target cell at grid position (0,3) - this should be the extension point
                if (cell.GridPosition == new Vector2Int(0, 3))
                {
                    Debug.Log($"[BoardPositioningManager] Found extension point at (0,3) - setting as purchased");
                        
                        // Set as purchased
                        cell.SetAdjacent(true);
                        cell.SetPurchased(true);
                        cell.SetAsExtensionPoint(true);
                        
                        // Unlock adjacent cells
                        UnlockAdjacentNodesFromExtensionPoint(boardController.gameObject, cell.GridPosition);
                        
                        Debug.Log($"[BoardPositioningManager] ✅ Cell at {cell.GridPosition} set as purchased");
                        Debug.Log($"[BoardPositioningManager] Final state - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
                    }
                }
            }
            
            Debug.Log("=== DIRECT PURCHASE COMPLETE ===");
        }
        
        /// <summary>
        /// Specifically target the extension point at (0,3) and set it as purchased
        /// </summary>
        [ContextMenu("Force Extension_West Setup")]
        public void ForceExtensionWestSetup()
        {
            Debug.Log("=== FORCING EXTENSION_WEST SETUP ===");
            
            // Find all extension boards
            ExtensionBoardController[] extensionBoards = FindObjectsByType<ExtensionBoardController>(FindObjectsSortMode.None);
            Debug.Log($"[BoardPositioningManager] Found {extensionBoards.Length} extension boards");
            
            foreach (ExtensionBoardController boardController in extensionBoards)
            {
                Debug.Log($"[BoardPositioningManager] Processing extension board: {boardController.gameObject.name}");
                
                // Find all cells in this board
                CellController_EXT[] extCells = boardController.gameObject.GetComponentsInChildren<CellController_EXT>();
                Debug.Log($"[BoardPositioningManager] Found {extCells.Length} cells in board");
                
                foreach (CellController_EXT cell in extCells)
                {
                    Debug.Log($"[BoardPositioningManager] Checking cell at {cell.GridPosition}: Type={cell.NodeType}, Name={cell.gameObject.name}, NodeName={cell.NodeName}");
                    
                    // Specifically look for extension point at (0,3)
                    if (cell.GridPosition == new Vector2Int(0, 3) && 
                        (cell.NodeName == "Extension_West" || cell.gameObject.name.Contains("Extension_West")))
                    {
                        Debug.Log($"[BoardPositioningManager] Found extension point (Extension_West) at (0,3) - setting up");
                        
                        // Set as purchased
                        cell.SetAdjacent(true);
                        cell.SetPurchased(true);
                        cell.SetAsExtensionPoint(true);
                        
                        // Unlock adjacent cells
                        UnlockAdjacentNodesFromExtensionPoint(boardController.gameObject, cell.GridPosition);
                        
                        Debug.Log($"[BoardPositioningManager] ✅ Extension point (Extension_West) at (0,3) set up successfully");
                        Debug.Log($"[BoardPositioningManager] Final state - IsPurchased: {cell.IsPurchased}, IsUnlocked: {cell.IsUnlocked}, IsAvailable: {cell.IsAvailable}");
                    }
                }
            }
            
            Debug.Log("=== EXTENSION_WEST SETUP COMPLETE ===");
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
        
        #region Utility Methods
        
        /// <summary>
        /// Convert board type string to BoardTheme enum
        /// </summary>
        private BoardTheme ConvertBoardTypeToTheme(string boardType)
        {
            if (string.IsNullOrEmpty(boardType))
                return BoardTheme.General;
                
            string lowerType = boardType.ToLower();
            
            if (lowerType.Contains("fire") || lowerType.Contains("ember"))
                return BoardTheme.Fire;
            else if (lowerType.Contains("cold") || lowerType.Contains("ice"))
                return BoardTheme.Cold;
            else if (lowerType.Contains("lightning") || lowerType.Contains("storm"))
                return BoardTheme.Lightning;
            else if (lowerType.Contains("physical") || lowerType.Contains("melee"))
                return BoardTheme.Physical;
            else
                return BoardTheme.General;
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
