using UnityEngine;
using System.Collections.Generic;

namespace PassiveTree
{
    /// <summary>
    /// Generates extension board prefabs using the CellContainer.prefab system
    /// Creates properly configured boards that work with the BoardPositioningManager
    /// </summary>
    public class ExtensionBoardGenerator : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] private GameObject cellContainerPrefab;
        [SerializeField] private Sprite[] nodeSprites;
        [SerializeField] private Sprite extensionPointSprite;
        
        [Header("Board Configuration")]
        [SerializeField] private Vector2Int boardSize = new Vector2Int(7, 7);
        [SerializeField] private float cellSpacing = 1f;
        [SerializeField] private Vector2 cellSize = new Vector2(1f, 1f);
        
        [Header("Board Themes")]
        [SerializeField] private BoardThemeData[] availableThemes;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        /// <summary>
        /// Generate an extension board prefab for a specific theme
        /// </summary>
        public GameObject GenerateExtensionBoard(BoardTheme theme, string boardName)
        {
            if (cellContainerPrefab == null)
            {
                Debug.LogError("[ExtensionBoardGenerator] CellContainer prefab not assigned!");
                return null;
            }
            
            // Create the board GameObject
            GameObject board = new GameObject(boardName);
            
            // Add Grid component for cell alignment
            var grid = board.AddComponent<Grid>();
            grid.cellSize = new Vector3(cellSize.x, cellSize.y, 1f);
            grid.cellGap = new Vector3(cellSpacing, cellSpacing, 0f);
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
            grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
            
            // Add BoardController component
            var boardController = board.AddComponent<ExtensionBoardController>();
            boardController.Initialize(theme, boardSize);
            
            // Generate cells for the board
            GenerateBoardCells(board, theme);
            
            // Setup extension points
            SetupBoardExtensionPoints(board, boardController);
            
            if (showDebugInfo)
                Debug.Log($"[ExtensionBoardGenerator] Generated extension board: {boardName} with theme: {theme}");
            
            return board;
        }
        
        /// <summary>
        /// Generate all cells for a board
        /// </summary>
        private void GenerateBoardCells(GameObject board, BoardTheme theme)
        {
            for (int x = 0; x < boardSize.x; x++)
            {
                for (int y = 0; y < boardSize.y; y++)
                {
                    Vector2Int cellPosition = new Vector2Int(x, y);
                    
                    // Create cell using CellContainer prefab
                    // Don't set parent during instantiation to avoid scaling issues
                    GameObject cell = Instantiate(cellContainerPrefab);
                    cell.name = $"Cell_{x}_{y}";
                    
                    // Set parent and position
                    cell.transform.SetParent(board.transform, false);
                    cell.transform.localPosition = new Vector3(x, y, 0);
                    
                    // Ensure proper scale
                    cell.transform.localScale = Vector3.one;
                    
                    // Fix BoxCollider size to 1.72 on both X/Y axes
                    var boxCollider = cell.GetComponent<BoxCollider2D>();
                    if (boxCollider != null)
                    {
                        boxCollider.size = new Vector2(1.72f, 1.72f);
                    }
                    
                    // Configure the cell based on position and theme
                    ConfigureCell(cell, cellPosition, theme);
                }
            }
        }
        
        /// <summary>
        /// Configure a cell based on its position and theme
        /// </summary>
        private void ConfigureCell(GameObject cell, Vector2Int position, BoardTheme theme)
        {
            // Get CellController component
            var cellController = cell.GetComponent<CellController>();
            if (cellController == null)
            {
                Debug.LogWarning($"[ExtensionBoardGenerator] CellController not found on cell at {position}");
                return;
            }
            
            // Set grid position
            cellController.SetGridPosition(position);
            
            // Determine node type based on position and theme
            NodeType nodeType = DetermineNodeType(position, theme);
            cellController.SetNodeType(nodeType);
            
            // Set appropriate sprite
            SetCellSprite(cell, nodeType, theme);
            
            // Configure extension points at board edges
            if (IsEdgePosition(position))
            {
                ConfigureEdgeCell(cell, position, theme);
            }
        }
        
        /// <summary>
        /// Determine the node type for a cell based on position and theme
        /// </summary>
        private NodeType DetermineNodeType(Vector2Int position, BoardTheme theme)
        {
            // Center cell is usually a notable or keystone
            Vector2Int center = new Vector2Int(boardSize.x / 2, boardSize.y / 2);
            if (position == center)
            {
                return theme == BoardTheme.Keystone ? NodeType.Keystone : NodeType.Notable;
            }
            
            // Edge cells are extension points
            if (IsEdgePosition(position))
            {
                return NodeType.Extension;
            }
            
            // Corner cells are usually small nodes
            if (IsCornerPosition(position))
            {
                return NodeType.Small;
            }
            
            // Most other cells are travel nodes
            return NodeType.Travel;
        }
        
        /// <summary>
        /// Check if a position is on the edge of the board
        /// </summary>
        private bool IsEdgePosition(Vector2Int position)
        {
            return position.x == 0 || position.x == boardSize.x - 1 || 
                   position.y == 0 || position.y == boardSize.y - 1;
        }
        
        /// <summary>
        /// Check if a position is a corner of the board
        /// </summary>
        private bool IsCornerPosition(Vector2Int position)
        {
            return (position.x == 0 || position.x == boardSize.x - 1) && 
                   (position.y == 0 || position.y == boardSize.y - 1);
        }
        
        /// <summary>
        /// Set the appropriate sprite for a cell
        /// </summary>
        private void SetCellSprite(GameObject cell, NodeType nodeType, BoardTheme theme)
        {
            var spriteRenderer = cell.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
            
            // Get theme-specific sprite
            Sprite sprite = GetThemeSprite(nodeType, theme);
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }
        
        /// <summary>
        /// Get a theme-specific sprite for a node type
        /// </summary>
        private Sprite GetThemeSprite(NodeType nodeType, BoardTheme theme)
        {
            // Find theme data
            BoardThemeData themeData = null;
            foreach (var data in availableThemes)
            {
                if (data.theme == theme)
                {
                    themeData = data;
                    break;
                }
            }
            
            if (themeData == null)
            {
                // Use default sprites if theme not found
                return GetDefaultSprite(nodeType);
            }
            
            // Return theme-specific sprite
            switch (nodeType)
            {
                case NodeType.Start:
                    return themeData.startSprite;
                case NodeType.Travel:
                    return themeData.travelSprite;
                case NodeType.Small:
                    return themeData.smallSprite;
                case NodeType.Notable:
                    return themeData.notableSprite;
                case NodeType.Keystone:
                    return themeData.keystoneSprite;
                case NodeType.Extension:
                    return extensionPointSprite;
                default:
                    return GetDefaultSprite(nodeType);
            }
        }
        
        /// <summary>
        /// Get default sprite for a node type
        /// </summary>
        private Sprite GetDefaultSprite(NodeType nodeType)
        {
            if (nodeSprites == null || nodeSprites.Length == 0) return null;
            
            // Use array index based on node type
            int spriteIndex = (int)nodeType;
            if (spriteIndex < nodeSprites.Length)
            {
                return nodeSprites[spriteIndex];
            }
            
            return nodeSprites[0]; // Fallback to first sprite
        }
        
        /// <summary>
        /// Configure edge cells as extension points
        /// </summary>
        private void ConfigureEdgeCell(GameObject cell, Vector2Int position, BoardTheme theme)
        {
            var cellController = cell.GetComponent<CellController>();
            if (cellController == null) return;
            
            // Mark as extension point
            cellController.SetAsExtensionPoint(true);
            
            // Set extension point properties
            var extensionPoint = new ExtensionPoint
            {
                id = $"board_{theme}_{position.x}_{position.y}",
                position = position,
                worldPosition = new Vector3Int(position.x, position.y, 0),
                availableBoards = GetCompatibleBoards(theme),
                maxConnections = 1,
                currentConnections = 0
            };
            
            // Store extension point data in cell controller
            cellController.SetExtensionPoint(extensionPoint);
        }
        
        /// <summary>
        /// Get boards compatible with a specific theme
        /// </summary>
        private List<string> GetCompatibleBoards(BoardTheme theme)
        {
            List<string> compatibleBoards = new List<string>();
            
            // Define compatibility rules
            switch (theme)
            {
                case BoardTheme.Fire:
                    compatibleBoards.AddRange(new[] { "fire_board", "general_board", "life_board" });
                    break;
                case BoardTheme.Cold:
                    compatibleBoards.AddRange(new[] { "cold_board", "general_board", "life_board" });
                    break;
                case BoardTheme.Life:
                    compatibleBoards.AddRange(new[] { "life_board", "fire_board", "cold_board", "general_board" });
                    break;
                case BoardTheme.General:
                    compatibleBoards.AddRange(new[] { "general_board", "fire_board", "cold_board", "life_board" });
                    break;
                default:
                    compatibleBoards.Add("general_board");
                    break;
            }
            
            return compatibleBoards;
        }
        
        /// <summary>
        /// Setup extension points for the board
        /// </summary>
        private void SetupBoardExtensionPoints(GameObject board, ExtensionBoardController boardController)
        {
            List<ExtensionPoint> extensionPoints = new List<ExtensionPoint>();
            
            // Find all extension point cells
            var cellControllers = board.GetComponentsInChildren<CellController>();
            foreach (var cellController in cellControllers)
            {
                if (cellController.IsExtensionPoint)
                {
                    var extensionPoint = cellController.GetExtensionPoint();
                    if (extensionPoint != null)
                    {
                        extensionPoints.Add(extensionPoint);
                    }
                }
            }
            
            // Set extension points in board controller
            boardController.SetExtensionPoints(extensionPoints);
        }
        
        #region Context Menu Methods
        
        /// <summary>
        /// Generate a test fire board
        /// </summary>
        [ContextMenu("Generate Test Fire Board")]
        public void GenerateTestFireBoard()
        {
            GameObject fireBoard = GenerateExtensionBoard(BoardTheme.Fire, "TestFireBoard");
            if (fireBoard != null)
            {
                Debug.Log("[ExtensionBoardGenerator] Generated test fire board");
            }
        }
        
        /// <summary>
        /// Generate a test cold board
        /// </summary>
        [ContextMenu("Generate Test Cold Board")]
        public void GenerateTestColdBoard()
        {
            GameObject coldBoard = GenerateExtensionBoard(BoardTheme.Cold, "TestColdBoard");
            if (coldBoard != null)
            {
                Debug.Log("[ExtensionBoardGenerator] Generated test cold board");
            }
        }
        
        /// <summary>
        /// Generate all test boards
        /// </summary>
        [ContextMenu("Generate All Test Boards")]
        public void GenerateAllTestBoards()
        {
            BoardTheme[] themes = { BoardTheme.Fire, BoardTheme.Cold, BoardTheme.Life, BoardTheme.General };
            
            foreach (var theme in themes)
            {
                GameObject board = GenerateExtensionBoard(theme, $"Test{theme}Board");
                if (board != null)
                {
                    Debug.Log($"[ExtensionBoardGenerator] Generated test {theme} board");
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Theme data for board generation
    /// </summary>
    [System.Serializable]
    public class BoardThemeData
    {
        public BoardTheme theme;
        public Sprite startSprite;
        public Sprite travelSprite;
        public Sprite smallSprite;
        public Sprite notableSprite;
        public Sprite keystoneSprite;
        public Color themeColor = Color.white;
    }
    
}
