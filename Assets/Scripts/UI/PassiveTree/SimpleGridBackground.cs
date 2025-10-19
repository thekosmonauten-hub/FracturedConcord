using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Simple grid background creator using individual quads
    /// Creates a grid of quads for a true grid pattern
    /// </summary>
    public class SimpleGridBackground : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private Material gridMaterial;
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private bool showDebugInfo = true;
        
        [Header("Visual Settings")]
        [SerializeField] private int sortingOrder = -100;
        [SerializeField] private Color gridColor = Color.white;
        
        private GameObject[,] gridCells;
        
        void Start()
        {
            CreateGrid();
        }
        
        /// <summary>
        /// Create the grid of quads
        /// </summary>
        [ContextMenu("Create Grid")]
        public void CreateGrid()
        {
            if (gridMaterial == null)
            {
                Debug.LogError("[SimpleGridBackground] No grid material assigned!");
                return;
            }
            
            // Clear existing grid
            ClearGrid();
            
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
            
            if (showDebugInfo)
            {
                Debug.Log($"[SimpleGridBackground] Created {gridWidth}x{gridHeight} grid with {gridWidth * gridHeight} cells");
            }
        }
        
        /// <summary>
        /// Create a single grid cell
        /// </summary>
        private void CreateGridCell(int x, int y, float startX, float startY)
        {
            GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cell.name = $"GridCell_{x}_{y}";
            cell.transform.SetParent(transform);
            cell.transform.position = new Vector3(
                startX + x * cellSize, 
                startY + y * cellSize, 
                0f
            );
            cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
            
            // Apply material
            Renderer renderer = cell.GetComponent<Renderer>();
            renderer.material = gridMaterial;
            renderer.sortingOrder = sortingOrder;
            
            // Store reference
            gridCells[x, y] = cell;
        }
        
        /// <summary>
        /// Clear existing grid
        /// </summary>
        [ContextMenu("Clear Grid")]
        public void ClearGrid()
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
            
            gridCells = null;
            
            if (showDebugInfo)
            {
                Debug.Log("[SimpleGridBackground] Cleared existing grid");
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
        /// Set grid cell color
        /// </summary>
        public void SetGridCellColor(int x, int y, Color color)
        {
            GameObject cell = GetGridCell(x, y);
            if (cell != null)
            {
                Renderer renderer = cell.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
        }
        
        /// <summary>
        /// Show grid statistics
        /// </summary>
        [ContextMenu("Show Grid Stats")]
        public void ShowGridStats()
        {
            Debug.Log($"[SimpleGridBackground] Grid Statistics:");
            Debug.Log($"  - Grid Size: {gridWidth}x{gridHeight}");
            Debug.Log($"  - Cell Size: {cellSize}");
            Debug.Log($"  - Total Cells: {gridWidth * gridHeight}");
            Debug.Log($"  - Grid Cells Array: {(gridCells != null ? "Initialized" : "Null")}");
            Debug.Log($"  - Child Count: {transform.childCount}");
        }
    }
}

