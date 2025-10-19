using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Example script showing how to use the GridBackground system
    /// This demonstrates various ways to interact with the grid
    /// </summary>
    public class GridBackgroundExample : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private GridBackground gridBackground;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color defaultColor = Color.white;
        
        void Start()
        {
            // Get reference to grid background if not assigned
            if (gridBackground == null)
            {
                gridBackground = FindObjectOfType<GridBackground>();
            }
            
            if (gridBackground != null)
            {
                // Example: Highlight some grid cells
                HighlightRandomCells();
            }
        }
        
        /// <summary>
        /// Example: Highlight random grid cells
        /// </summary>
        [ContextMenu("Highlight Random Cells")]
        public void HighlightRandomCells()
        {
            if (gridBackground == null) return;
            
            // Highlight 5 random cells
            for (int i = 0; i < 5; i++)
            {
                int x = Random.Range(0, 10); // Assuming 10x10 grid
                int y = Random.Range(0, 10);
                
                gridBackground.SetGridCellColor(x, y, highlightColor);
            }
            
            Debug.Log("[GridBackgroundExample] Highlighted 5 random cells");
        }
        
        /// <summary>
        /// Example: Reset all grid cells to default color
        /// </summary>
        [ContextMenu("Reset All Cells")]
        public void ResetAllCells()
        {
            if (gridBackground == null) return;
            
            gridBackground.SetAllCellsColor(defaultColor);
            Debug.Log("[GridBackgroundExample] Reset all cells to default color");
        }
        
        /// <summary>
        /// Example: Create a pattern on the grid
        /// </summary>
        [ContextMenu("Create Checkerboard Pattern")]
        public void CreateCheckerboardPattern()
        {
            if (gridBackground == null) return;
            
            // Create a checkerboard pattern
            for (int x = 0; x < 10; x++) // Assuming 10x10 grid
            {
                for (int y = 0; y < 10; y++)
                {
                    Color cellColor = ((x + y) % 2 == 0) ? Color.white : Color.gray;
                    gridBackground.SetGridCellColor(x, y, cellColor);
                }
            }
            
            Debug.Log("[GridBackgroundExample] Created checkerboard pattern");
        }
        
        /// <summary>
        /// Example: Highlight cells in a cross pattern
        /// </summary>
        [ContextMenu("Create Cross Pattern")]
        public void CreateCrossPattern()
        {
            if (gridBackground == null) return;
            
            int centerX = 5; // Center of 10x10 grid
            int centerY = 5;
            
            // Create horizontal line
            for (int x = 0; x < 10; x++)
            {
                gridBackground.SetGridCellColor(x, centerY, highlightColor);
            }
            
            // Create vertical line
            for (int y = 0; y < 10; y++)
            {
                gridBackground.SetGridCellColor(centerX, y, highlightColor);
            }
            
            Debug.Log("[GridBackgroundExample] Created cross pattern");
        }
        
        /// <summary>
        /// Example: Get grid cell at mouse position
        /// </summary>
        void Update()
        {
            if (Input.GetMouseButtonDown(0) && gridBackground != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0; // Ensure z is 0 for 2D
            
                GameObject cell = gridBackground.GetGridCellAtWorldPosition(mousePos);
                if (cell != null)
                {
                    Debug.Log($"[GridBackgroundExample] Clicked on grid cell at world position {mousePos}");
                    
                    // Highlight the clicked cell
                    SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.color = highlightColor;
                    }
                }
            }
        }
    }
}
