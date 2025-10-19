using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Creates a grid background using Unity's Grid system
    /// Perfect for passive tree backgrounds with regular grid patterns
    /// </summary>
    public class GridBackground : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private Sprite gridTile;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private bool showDebugInfo = true;
        
        [Header("Sprite Scaling")]
        [SerializeField] private bool autoScaleSprite = true;
        [SerializeField] private float spriteScale = 1f;
        
        [Header("Gap Removal")]
        [SerializeField] private bool removeGaps = true;
        [SerializeField] private float gapOffset = 0f;
        
        [Header("Runtime Persistence")]
        [SerializeField] private bool persistInRuntime = true;
        [SerializeField] private bool preventDestruction = true;
        
        [Header("Visual Settings")]
        [SerializeField] private int sortingOrder = -100;
        [SerializeField] private Color gridColor = Color.white;
        
        private Grid grid;
        private GameObject[,] gridCells;
        private bool isInitialized = false;
        
        void Awake()
        {
            // Prevent destruction if enabled
            if (preventDestruction)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        
        /// <summary>
        /// Apply DontDestroyOnLoad to all child GameObjects
        /// </summary>
        private void ApplyDontDestroyOnLoadToChildren()
        {
            if (preventDestruction)
            {
                // Apply to all existing children
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    DontDestroyOnLoad(child.gameObject);
                }
            }
        }
        
        void Start()
        {
            if (persistInRuntime)
            {
                CreateGridBackground();
                isInitialized = true;
            }
        }
        
        void OnDestroy()
        {
            if (showDebugInfo)
            {
                Debug.Log("[GridBackground] GridBackground is being destroyed. Check if this is intended.");
            }
        }
        
        void Update()
        {
            // Monitor grid validity during runtime if persistence is enabled
            if (persistInRuntime && isInitialized)
            {
                // Check grid validity every 5 seconds
                if (Time.time % 5f < Time.deltaTime)
                {
                    CheckGridValidity();
                    
                    // Also check if children need protection
                    if (preventDestruction)
                    {
                        CheckAndProtectChildren();
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if children need protection and apply it
        /// </summary>
        private void CheckAndProtectChildren()
        {
            int unprotectedChildren = 0;
            
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                
                // Check if child is not already protected
                // Note: There's no direct way to check if DontDestroyOnLoad is applied,
                // so we'll apply it to all children to be safe
                DontDestroyOnLoad(child.gameObject);
                unprotectedChildren++;
            }
            
            if (unprotectedChildren > 0 && showDebugInfo)
            {
                Debug.Log($"[GridBackground] Protected {unprotectedChildren} child GameObjects from destruction");
            }
        }
        
        /// <summary>
        /// Create the grid background using Unity's Grid system
        /// </summary>
        [ContextMenu("Create Grid Background")]
        public void CreateGridBackground()
        {
            if (gridTile == null)
            {
                Debug.LogError("[GridBackground] No grid tile sprite assigned!");
                return;
            }
            
            // Clear existing grid if any
            ClearExistingGrid();
            
            // Add Grid component
            grid = gameObject.AddComponent<Grid>();
            grid.cellSize = new Vector3(cellSize, cellSize, 0);
            
            // Create grid cells
            gridCells = new GameObject[gridWidth, gridHeight];
            
            float startX = -(gridWidth * cellSize) / 2f;
            float startY = -(gridHeight * cellSize) / 2f;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    CreateGridCell(x, y, startX, startY);
                }
            }
            
            isInitialized = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridBackground] Created {gridWidth}x{gridHeight} grid with {gridWidth * gridHeight} cells");
            }
            
            // Apply DontDestroyOnLoad to all children AFTER grid creation is complete
            if (preventDestruction)
            {
                ApplyDontDestroyOnLoadToChildren();
                if (showDebugInfo)
                {
                    Debug.Log($"[GridBackground] All {gridWidth * gridHeight} child GameObjects are protected from destruction");
                }
            }
        }
        
        /// <summary>
        /// Check if grid is still valid and recreate if needed
        /// </summary>
        [ContextMenu("Check Grid Validity")]
        public void CheckGridValidity()
        {
            bool needsRecreation = false;
            
            // Check if grid cells array is null or empty
            if (gridCells == null)
            {
                Debug.LogWarning("[GridBackground] Grid cells array is null. Grid needs recreation.");
                needsRecreation = true;
            }
            else
            {
                // Check if any grid cells are missing
                int missingCells = 0;
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (gridCells[x, y] == null)
                        {
                            missingCells++;
                        }
                    }
                }
                
                if (missingCells > 0)
                {
                    Debug.LogWarning($"[GridBackground] {missingCells} grid cells are missing. Grid needs recreation.");
                    needsRecreation = true;
                }
            }
            
            // Check if Grid component is missing
            if (grid == null)
            {
                Debug.LogWarning("[GridBackground] Grid component is missing. Grid needs recreation.");
                needsRecreation = true;
            }
            
            if (needsRecreation)
            {
                Debug.Log("[GridBackground] Recreating grid...");
                CreateGridBackground();
            }
            else
            {
                Debug.Log("[GridBackground] Grid is valid and intact.");
            }
        }
        
        /// <summary>
        /// Force recreate grid if it's missing or corrupted
        /// </summary>
        [ContextMenu("Force Recreate Grid")]
        public void ForceRecreateGrid()
        {
            Debug.Log("[GridBackground] Force recreating grid...");
            CreateGridBackground();
        }
        
        /// <summary>
        /// Apply DontDestroyOnLoad to all existing children
        /// </summary>
        [ContextMenu("Protect All Children")]
        public void ProtectAllChildren()
        {
            if (preventDestruction)
            {
                int protectedCount = 0;
                
                // Apply to all existing children
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    DontDestroyOnLoad(child.gameObject);
                    protectedCount++;
                }
                
                if (showDebugInfo)
                {
                    Debug.Log($"[GridBackground] Protected {protectedCount} child GameObjects from destruction");
                }
            }
            else
            {
                Debug.LogWarning("[GridBackground] Prevent Destruction is disabled. Enable it first to protect children.");
            }
        }
        
        /// <summary>
        /// Verify that all grid cells were created properly
        /// </summary>
        [ContextMenu("Verify Grid Creation")]
        public void VerifyGridCreation()
        {
            if (gridCells == null)
            {
                Debug.LogError("[GridBackground] Grid cells array is null. Grid was not created properly.");
                return;
            }
            
            int expectedCells = gridWidth * gridHeight;
            int actualCells = 0;
            int nullCells = 0;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridCells[x, y] != null)
                    {
                        actualCells++;
                    }
                    else
                    {
                        nullCells++;
                    }
                }
            }
            
            Debug.Log($"[GridBackground] Grid Creation Verification:");
            Debug.Log($"  - Expected Cells: {expectedCells}");
            Debug.Log($"  - Actual Cells: {actualCells}");
            Debug.Log($"  - Null Cells: {nullCells}");
            Debug.Log($"  - Child Count: {transform.childCount}");
            
            if (actualCells != expectedCells)
            {
                Debug.LogError($"[GridBackground] Grid creation incomplete! Expected {expectedCells} cells, got {actualCells}");
            }
            else
            {
                Debug.Log("[GridBackground] ✅ Grid creation successful!");
            }
        }
        
        /// <summary>
        /// Create a single grid cell
        /// </summary>
        private void CreateGridCell(int x, int y, float startX, float startY)
        {
            GameObject tile = new GameObject($"GridCell_{x}_{y}");
            tile.transform.SetParent(transform);
            
            // Calculate position with gap removal
            float posX = startX + x * cellSize;
            float posY = startY + y * cellSize;
            
            if (removeGaps)
            {
                // Offset to remove gaps between cells
                posX += gapOffset;
                posY += gapOffset;
            }
            
            tile.transform.position = new Vector3(posX, posY, 0f);
            
            // Add SpriteRenderer
            SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
            renderer.sprite = gridTile;
            renderer.sortingOrder = sortingOrder;
            renderer.color = gridColor;
            
            // Scale the sprite to match the cell size
            if (autoScaleSprite && gridTile != null)
            {
                // Get the sprite's original size
                float spriteWidth = gridTile.bounds.size.x;
                float spriteHeight = gridTile.bounds.size.y;
                
                // Calculate scale to fit the cell size
                float scaleX = cellSize / spriteWidth;
                float scaleY = cellSize / spriteHeight;
                
                // Apply additional sprite scale if specified
                scaleX *= spriteScale;
                scaleY *= spriteScale;
                
                // Set the scale
                tile.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[GridBackground] Scaled sprite: {spriteWidth}x{spriteHeight} -> {scaleX}x{scaleY} (Cell: {cellSize})");
                }
            }
            else if (spriteScale != 1f)
            {
                // Apply manual sprite scale
                tile.transform.localScale = new Vector3(spriteScale, spriteScale, 1f);
            }
            
            // Store reference first
            gridCells[x, y] = tile;
        }
        
        /// <summary>
        /// Clear existing grid cells
        /// </summary>
        [ContextMenu("Clear Grid")]
        public void ClearExistingGrid()
        {
            // Remove existing children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
            
            // Clear grid cells array
            gridCells = null;
            
            if (showDebugInfo)
            {
                Debug.Log("[GridBackground] Cleared existing grid");
            }
        }
        
        /// <summary>
        /// Update grid settings and recreate
        /// </summary>
        [ContextMenu("Update Grid")]
        public void UpdateGrid()
        {
            ClearExistingGrid();
            CreateGridBackground();
        }
        
        /// <summary>
        /// Update sprite scaling for existing grid cells
        /// </summary>
        [ContextMenu("Update Sprite Scaling")]
        public void UpdateSpriteScaling()
        {
            if (gridCells == null)
            {
                Debug.LogWarning("[GridBackground] No grid cells to update. Create grid first.");
                return;
            }
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridCells[x, y] != null)
                    {
                        UpdateCellSpriteScale(gridCells[x, y]);
                    }
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridBackground] Updated sprite scaling for {gridWidth * gridHeight} cells");
            }
        }
        
        /// <summary>
        /// Auto-calculate optimal gap offset for seamless tiling
        /// </summary>
        [ContextMenu("Auto Calculate Gap Offset")]
        public void AutoCalculateGapOffset()
        {
            if (gridTile == null)
            {
                Debug.LogWarning("[GridBackground] No grid tile assigned. Cannot calculate gap offset.");
                return;
            }
            
            // Calculate the gap offset needed for seamless tiling
            // This is typically half the cell size to ensure perfect alignment
            gapOffset = cellSize * 0.5f;
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridBackground] Auto-calculated gap offset: {gapOffset} (Cell Size: {cellSize})");
            }
        }
        
        /// <summary>
        /// Update gap removal for existing grid cells
        /// </summary>
        [ContextMenu("Update Gap Removal")]
        public void UpdateGapRemoval()
        {
            if (gridCells == null)
            {
                Debug.LogWarning("[GridBackground] No grid cells to update. Create grid first.");
                return;
            }
            
            float startX = -(gridWidth * cellSize) / 2f;
            float startY = -(gridHeight * cellSize) / 2f;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridCells[x, y] != null)
                    {
                        // Calculate new position with gap removal
                        float posX = startX + x * cellSize;
                        float posY = startY + y * cellSize;
                        
                        if (removeGaps)
                        {
                            posX += gapOffset;
                            posY += gapOffset;
                        }
                        
                        gridCells[x, y].transform.position = new Vector3(posX, posY, 0f);
                    }
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridBackground] Updated gap removal for {gridWidth * gridHeight} cells");
            }
        }
        
        /// <summary>
        /// Update sprite scale for a single cell
        /// </summary>
        private void UpdateCellSpriteScale(GameObject cell)
        {
            if (autoScaleSprite && gridTile != null)
            {
                // Get the sprite's original size
                float spriteWidth = gridTile.bounds.size.x;
                float spriteHeight = gridTile.bounds.size.y;
                
                // Calculate scale to fit the cell size
                float scaleX = cellSize / spriteWidth;
                float scaleY = cellSize / spriteHeight;
                
                // Apply additional sprite scale if specified
                scaleX *= spriteScale;
                scaleY *= spriteScale;
                
                // Set the scale
                cell.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
            else if (spriteScale != 1f)
            {
                // Apply manual sprite scale
                cell.transform.localScale = new Vector3(spriteScale, spriteScale, 1f);
            }
        }
        
        /// <summary>
        /// Get grid cell at specific position
        /// </summary>
        public GameObject GetGridCell(int x, int y)
        {
            if (gridCells == null || x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            {
                return null;
            }
            
            return gridCells[x, y];
        }
        
        /// <summary>
        /// Get grid cell at world position
        /// </summary>
        public GameObject GetGridCellAtWorldPosition(Vector3 worldPos)
        {
            if (grid == null) return null;
            
            Vector3Int gridPos = grid.WorldToCell(worldPos);
            return GetGridCell(gridPos.x, gridPos.y);
        }
        
        /// <summary>
        /// Set grid cell color
        /// </summary>
        public void SetGridCellColor(int x, int y, Color color)
        {
            GameObject cell = GetGridCell(x, y);
            if (cell != null)
            {
                SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = color;
                }
            }
        }
        
        /// <summary>
        /// Set all grid cells to a specific color
        /// </summary>
        [ContextMenu("Set All Cells Color")]
        public void SetAllCellsColor(Color color)
        {
            if (gridCells == null) return;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    SetGridCellColor(x, y, color);
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[GridBackground] Set all {gridWidth * gridHeight} cells to color {color}");
            }
        }
        
        /// <summary>
        /// Get grid statistics
        /// </summary>
        [ContextMenu("Show Grid Stats")]
        public void ShowGridStats()
        {
            Debug.Log($"[GridBackground] Grid Statistics:");
            Debug.Log($"  - Grid Size: {gridWidth}x{gridHeight}");
            Debug.Log($"  - Cell Size: {cellSize}");
            Debug.Log($"  - Total Cells: {gridWidth * gridHeight}");
            Debug.Log($"  - Grid Component: {(grid != null ? "Present" : "Missing")}");
            Debug.Log($"  - Grid Cells Array: {(gridCells != null ? "Initialized" : "Null")}");
            
            if (gridTile != null)
            {
                Debug.Log($"  - Sprite Size: {gridTile.bounds.size.x}x{gridTile.bounds.size.y}");
                Debug.Log($"  - Auto Scale Sprite: {autoScaleSprite}");
                Debug.Log($"  - Sprite Scale: {spriteScale}");
                
                if (autoScaleSprite)
                {
                    float scaleX = cellSize / gridTile.bounds.size.x;
                    float scaleY = cellSize / gridTile.bounds.size.y;
                    Debug.Log($"  - Calculated Scale: {scaleX}x{scaleY}");
                }
            }
            
            Debug.Log($"  - Remove Gaps: {removeGaps}");
            Debug.Log($"  - Gap Offset: {gapOffset}");
            
            if (removeGaps)
            {
                Debug.Log($"  - Gap Removal Status: Active (Offset: {gapOffset})");
            }
            else
            {
                Debug.Log($"  - Gap Removal Status: Disabled");
            }
            
            Debug.Log($"  - Persist in Runtime: {persistInRuntime}");
            Debug.Log($"  - Prevent Destruction: {preventDestruction}");
            Debug.Log($"  - Is Initialized: {isInitialized}");
            
            if (persistInRuntime)
            {
                Debug.Log($"  - Runtime Persistence: Active");
            }
            else
            {
                Debug.Log($"  - Runtime Persistence: Disabled");
            }
            
            if (preventDestruction)
            {
                Debug.Log($"  - Child Protection: Active ({transform.childCount} children protected)");
            }
            else
            {
                Debug.Log($"  - Child Protection: Disabled");
            }
        }
        
        /// <summary>
        /// Validate grid settings
        /// </summary>
        [ContextMenu("Validate Grid Settings")]
        public void ValidateGridSettings()
        {
            bool isValid = true;
            
            if (gridTile == null)
            {
                Debug.LogError("[GridBackground] Grid tile sprite is not assigned!");
                isValid = false;
            }
            
            if (cellSize <= 0)
            {
                Debug.LogError("[GridBackground] Cell size must be greater than 0!");
                isValid = false;
            }
            
            if (gridWidth <= 0 || gridHeight <= 0)
            {
                Debug.LogError("[GridBackground] Grid dimensions must be greater than 0!");
                isValid = false;
            }
            
            if (isValid)
            {
                Debug.Log("[GridBackground] ✅ Grid settings are valid");
            }
        }
    }
}
