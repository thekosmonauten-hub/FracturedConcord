using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Unity UI version of effigy grid system
/// Manages 6x4 grid for effigy placement with drag & drop
/// </summary>
public class EffigyGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    private const int GRID_WIDTH = 6;
    private const int GRID_HEIGHT = 20; // Updated to 20 rows
    
    [Tooltip("Size of each cell in pixels")]
    [SerializeField] private float cellSize = 60f;
    
    [Tooltip("Spacing between cells in pixels")]
    [SerializeField] private float cellSpacing = 2f;
    
    [Header("References")]
    [Tooltip("Prefab for individual cells (used if gridPrefab is not set)")]
    [SerializeField] private GameObject cellPrefab;
    
    [Tooltip("Complete grid prefab with all cells already set up (FASTER - recommended)")]
    [SerializeField] private GameObject gridPrefab;
    
    [SerializeField] private Transform gridContainer;
    [SerializeField] private EffigyStorageUI effigyStorage; // Reference to return unequipped effigies
    
    [Header("Colors")]
    [SerializeField] private Color emptyCellColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
    [SerializeField] private Color validPlacementColor = new Color(0.2f, 1f, 0.2f, 0.6f);
    [SerializeField] private Color invalidPlacementColor = new Color(1f, 0.2f, 0.2f, 0.6f);
    
    private List<EffigyGridCellUI> gridCells = new List<EffigyGridCellUI>();
    private Effigy[,] placedEffigies = new Effigy[GRID_HEIGHT, GRID_WIDTH];
    private Dictionary<Effigy, List<GameObject>> effigyVisuals = new Dictionary<Effigy, List<GameObject>>();
    
    // Drag state
    private Effigy draggedEffigy = null;
    private Vector2Int currentHoveredCell = new Vector2Int(-1, -1);
    private bool isDraggingFromStorage = false;
    private GameObject dragVisual = null;
    private Canvas dragCanvas = null;
    private EffigyStorageSlotUI sourceStorageSlot = null; // Track source to restore or remove
    private EffigyStorageUI sourceStorage = null; // Track storage to remove effigy from
    private Vector2Int originalGridPosition = new Vector2Int(-1, -1); // Track original position for repositioning
    private Vector2 dragCentroidOffset = Vector2.zero; // Store the centroid offset in pixel space for proper centering
    private Vector2Int pickupPoint = new Vector2Int(-1, -1); // Store the pickup point cell coordinates for placement calculation
    
    void Awake()
    {
        // Defer grid generation to prevent blocking scene load
        StartCoroutine(DeferredGridGeneration());
    }
    
    /// <summary>
    /// Generate grid progressively across multiple frames
    /// </summary>
    private System.Collections.IEnumerator DeferredGridGeneration()
    {
        yield return null; // Wait a frame
        yield return StartCoroutine(GenerateGridProgressive());
    }
    
    void Start()
    {
        // Verify critical reference
        if (effigyStorage == null)
        {
            Debug.LogError("[EffigyGridUI] ⚠️ EFFIGY STORAGE REFERENCE IS MISSING! Unequip will not work. Please assign EffigyStorageUI in Inspector.");
        }
        else
        {
            Debug.Log($"[EffigyGridUI] ✓ Effigy Storage reference set: {effigyStorage.name}");
        }
    }
    
    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    void GenerateGrid()
    {
        // This method is kept for backward compatibility but should use DeferredGridGeneration
        StartCoroutine(GenerateGridProgressive());
    }
    
    /// <summary>
    /// Generate grid progressively to prevent blocking
    /// Uses prefab grid if available (much faster), otherwise generates dynamically
    /// </summary>
    private System.Collections.IEnumerator GenerateGridProgressive()
    {
        // Clear existing cells
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        gridCells.Clear();
        
        // FAST PATH: Use prefab grid if available (instantiate once, much faster)
        if (gridPrefab != null)
        {
            yield return null; // Wait one frame
            
            // Instantiate the prefab as a single object - it already has GridLayoutGroup and all cells laid out
            GameObject gridInstance = Instantiate(gridPrefab, gridContainer);
            gridInstance.name = "EffigyGrid";
            
            // Ensure the prefab grid fills the container properly
            RectTransform gridRect = gridInstance.GetComponent<RectTransform>();
            if (gridRect != null)
            {
                gridRect.anchoredPosition = Vector2.zero;
                gridRect.anchorMin = Vector2.zero;
                gridRect.anchorMax = Vector2.one;
                gridRect.sizeDelta = Vector2.zero;
                gridRect.localScale = Vector3.one;
            }
            
            // Disable container's GridLayoutGroup if it exists (prefab has its own)
            GridLayoutGroup containerLayout = gridContainer.GetComponent<GridLayoutGroup>();
            if (containerLayout != null)
            {
                containerLayout.enabled = false;
            }
            
            // Collect all cells from the prefab (they're already laid out by the prefab's GridLayoutGroup)
            EffigyGridCellUI[] prefabCells = gridInstance.GetComponentsInChildren<EffigyGridCellUI>();
            gridCells.AddRange(prefabCells);
            
            // Set up event handlers for all cells
            for (int i = 0; i < gridCells.Count; i++)
            {
                EffigyGridCellUI cellUI = gridCells[i];
                if (cellUI != null)
                {
                    // Calculate position from index (assuming left-to-right, top-to-bottom)
                    int x = i % GRID_WIDTH;
                    int y = i / GRID_WIDTH;
                    cellUI.cellX = x;
                    cellUI.cellY = y;
                    cellUI.OnCellMouseDown += OnCellMouseDown;
                    cellUI.OnCellMouseEnter += OnCellMouseEnter;
                    cellUI.OnCellMouseExit += OnCellMouseExit;
                    cellUI.OnCellMouseUp += OnCellMouseUp;
                }
            }
            
            Debug.Log($"[EffigyGridUI] Loaded {gridCells.Count} cells from prefab grid ({GRID_WIDTH}x{GRID_HEIGHT}) - FAST PATH (1 instantiate)");
            yield break; // Done!
        }
        
        // SLOW PATH: Generate dynamically (fallback if no prefab)
        // Set up GridLayoutGroup on container for dynamic generation
        GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        
        gridLayout.enabled = true; // Re-enable if it was disabled
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
        gridLayout.spacing = new Vector2(cellSpacing, cellSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = GRID_WIDTH;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.MiddleCenter; // Center the grid cells
        
        // SLOW PATH: Generate dynamically (fallback if no prefab)
        if (cellPrefab == null)
        {
            Debug.LogError("[EffigyGridUI] Neither gridPrefab nor cellPrefab is set! Cannot generate grid.");
            yield break;
        }
        
        // Generate cells progressively (5 per frame)
        const int cellsPerFrame = 5;
        int cellsGenerated = 0;
        int totalCells = GRID_WIDTH * GRID_HEIGHT;
        
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridContainer);
                cellObj.name = $"EffigyCell_{x}_{y}";
                
                EffigyGridCellUI cellUI = cellObj.GetComponent<EffigyGridCellUI>();
                if (cellUI != null)
                {
                    cellUI.cellX = x;
                    cellUI.cellY = y;
                    cellUI.OnCellMouseDown += OnCellMouseDown;
                    cellUI.OnCellMouseEnter += OnCellMouseEnter;
                    cellUI.OnCellMouseExit += OnCellMouseExit;
                    cellUI.OnCellMouseUp += OnCellMouseUp;
                    
                    gridCells.Add(cellUI);
                }
                
                cellsGenerated++;
                
                // Yield every few cells to prevent blocking
                if (cellsGenerated % cellsPerFrame == 0)
                {
                    yield return null;
                }
            }
        }
        
        Debug.Log($"[EffigyGridUI] Generated {gridCells.Count} cells ({GRID_WIDTH}x{GRID_HEIGHT}) progressively - SLOW PATH");
    }
    
    /// <summary>
    /// Legacy GenerateGrid implementation (for backward compatibility)
    /// </summary>
    private void GenerateGridLegacy()
    {
        // Clear existing cells
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        gridCells.Clear();
        
        // Set up GridLayoutGroup
        GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
        gridLayout.spacing = new Vector2(cellSpacing, cellSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = GRID_WIDTH;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.MiddleCenter; // Center the grid cells
        
        // Generate cells
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridContainer);
                cellObj.name = $"EffigyCell_{x}_{y}";
                
                EffigyGridCellUI cellUI = cellObj.GetComponent<EffigyGridCellUI>();
                if (cellUI != null)
                {
                    cellUI.cellX = x;
                    cellUI.cellY = y;
                    cellUI.OnCellMouseDown += OnCellMouseDown;
                    cellUI.OnCellMouseEnter += OnCellMouseEnter;
                    cellUI.OnCellMouseExit += OnCellMouseExit;
                    cellUI.OnCellMouseUp += OnCellMouseUp;
                    
                    gridCells.Add(cellUI);
                }
            }
        }
        
        Debug.Log($"[EffigyGridUI] Generated {gridCells.Count} cells ({GRID_WIDTH}x{GRID_HEIGHT})");
    }
    
    public void StartDragFromStorage(Effigy effigy, EffigyStorageSlotUI sourceSlot = null, EffigyStorageUI storage = null, Vector2Int clickedCell = default)
    {
        draggedEffigy = effigy;
        isDraggingFromStorage = true;
        currentHoveredCell = new Vector2Int(-1, -1);
        sourceStorageSlot = sourceSlot;
        sourceStorage = storage;
        
        // Use clicked cell as pickup point if provided and valid, otherwise use effigy's ghostPickupPoint or centroid
        CreateDragVisual(effigy, clickedCell);
        
        Debug.Log($"[EffigyGridUI] Started dragging {effigy.effigyName} from storage, clicked cell: ({clickedCell.x}, {clickedCell.y})");
    }
    
    /// <summary>
    /// Create a visual representation of the effigy that follows the cursor
    /// Uses the clicked cell position as the pickup point if provided, otherwise uses centroid or ghostPickupPoint
    /// </summary>
    void CreateDragVisual(Effigy effigy, Vector2Int clickedCell = default)
    {
        if (effigy == null || effigy.shapeMask == null || effigy.shapeMask.Length == 0) return;
        
        // Find or create a canvas for drag visuals (high sort order to appear above everything)
        if (dragCanvas == null)
        {
            GameObject canvasObj = new GameObject("DragCanvas");
            dragCanvas = canvasObj.AddComponent<Canvas>();
            dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dragCanvas.sortingOrder = 1000; // Very high to appear above everything
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create container for drag visual
        dragVisual = new GameObject($"DragVisual_{effigy.effigyName}");
        dragVisual.transform.SetParent(dragCanvas.transform, false);
        
        RectTransform dragRect = dragVisual.AddComponent<RectTransform>();
        
        int width = effigy.shapeWidth;
        int height = effigy.shapeHeight;
        
        // Count occupied cells first (needed for debug logging)
        int occupiedCount = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index < effigy.shapeMask.Length && effigy.shapeMask[index])
                {
                    occupiedCount++;
                }
            }
        }
        
        // Determine the pickup point (centroid) to use for positioning the ghost
        // Priority: 1) effigy.ghostPickupPoint (if set and occupied - highest priority for designer control),
        // 2) clickedCell (if valid and occupied), 3) first occupied cell in reading order, 4) calculated centroid
        Vector2 centroid = Vector2.zero;
        
        // First check if effigy has an explicit ghostPickupPoint set (designer's explicit choice takes priority)
        Vector2Int pickupPoint = effigy.ghostPickupPoint;
        bool usePickupPoint = pickupPoint.x >= 0 && pickupPoint.y >= 0 &&
                              pickupPoint.x < width && pickupPoint.y < height &&
                              effigy.IsCellOccupied(pickupPoint.x, pickupPoint.y);
        
        if (usePickupPoint)
        {
            // Use the explicitly set pickup point as the centroid (designer's choice)
            centroid = new Vector2(pickupPoint.x, pickupPoint.y);
            this.pickupPoint = pickupPoint; // Store for placement calculation
            Debug.Log($"[EffigyGridUI] Using explicit ghost pickup point: ({pickupPoint.x}, {pickupPoint.y})");
        }
        else
        {
            // Check if a clicked cell was provided and is valid
            bool useClickedCell = clickedCell.x >= 0 && clickedCell.y >= 0 &&
                                  clickedCell.x < width && clickedCell.y < height &&
                                  effigy.IsCellOccupied(clickedCell.x, clickedCell.y);
            
            if (useClickedCell)
            {
                // Use the clicked cell as the pickup point
                centroid = new Vector2(clickedCell.x, clickedCell.y);
                this.pickupPoint = clickedCell; // Store for placement calculation
                Debug.Log($"[EffigyGridUI] Using clicked cell as pickup point: ({clickedCell.x}, {clickedCell.y})");
            }
            else
            {
                // Get all occupied cells and find the first one in reading order (top-to-bottom, left-to-right)
                // This ensures we always use an occupied cell, even for shapes like Cross where (0,0) is empty
                List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
                
                if (occupiedCells.Count > 0)
                {
                    // Sort by Y first (top to bottom), then by X (left to right)
                    occupiedCells.Sort((a, b) => 
                    {
                        if (a.y != b.y) return a.y.CompareTo(b.y);
                        return a.x.CompareTo(b.x);
                    });
                    
                    // Use the first occupied cell as the pickup point
                    Vector2Int firstOccupied = occupiedCells[0];
                    centroid = new Vector2(firstOccupied.x, firstOccupied.y);
                    this.pickupPoint = firstOccupied; // Store for placement calculation
                    Debug.Log($"[EffigyGridUI] Using first occupied cell in reading order as pickup point: ({firstOccupied.x}, {firstOccupied.y})");
                }
                else
                {
                    // Fallback: Calculate centroid: average of all occupied cell positions
                    // Formula: centroid = (1/N) * sum of all tile positions
                    List<Vector2> occupiedPositions = new List<Vector2>();
                    
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int index = y * width + x;
                            if (index < effigy.shapeMask.Length && effigy.shapeMask[index])
                            {
                                occupiedPositions.Add(new Vector2(x, y));
                            }
                        }
                    }
                    
                    if (occupiedPositions.Count > 0)
                    {
                        // Sum all positions
                        foreach (var pos in occupiedPositions)
                        {
                            centroid += pos;
                        }
                        // Divide by count to get average (centroid)
                        centroid /= occupiedPositions.Count;
                    }
                    else
                    {
                    // Fallback to bounding box center if no occupied cells found
                    centroid = new Vector2((width - 1) / 2f, (height - 1) / 2f);
                    this.pickupPoint = new Vector2Int(Mathf.RoundToInt(centroid.x), Mathf.RoundToInt(centroid.y)); // Store for placement calculation
                }
                
                Debug.Log($"[EffigyGridUI] Calculated centroid from {occupiedPositions.Count} occupied cells: ({centroid.x:F2}, {centroid.y:F2})");
            }
        }
        
        // If pickup point wasn't set (shouldn't happen, but safety), use centroid rounded to nearest cell
        if (pickupPoint.x < 0 || pickupPoint.y < 0)
        {
            pickupPoint = new Vector2Int(Mathf.RoundToInt(centroid.x), Mathf.RoundToInt(centroid.y));
        }
        }
        
        // Set size to fit all cells
        float totalWidth = width * cellSize + (width - 1) * cellSpacing;
        float totalHeight = height * cellSize + (height - 1) * cellSpacing;
        
        // Keep top-left anchor to match cell positioning system
        dragRect.anchorMin = new Vector2(0, 1);
        dragRect.anchorMax = new Vector2(0, 1);
        dragRect.pivot = new Vector2(0.5f, 0.5f);  // Center pivot for easier offset calculations
        dragRect.sizeDelta = new Vector2(totalWidth, totalHeight);
        
        // Convert centroid from tile-space to pixel-space offset
        // With top-left anchor (0,1), cells are positioned as:
        //   cellTopLeftX = x * (cellSize + cellSpacing)
        //   cellTopLeftY = -y * (cellSize + cellSpacing)  (negative because Y goes down from top)
        //   cellCenterX = cellTopLeftX + cellSize/2
        //   cellCenterY = cellTopLeftY - cellSize/2
        // The centroid cell center in pixel space (relative to dragRect top-left):
        float centroidCellCenterX = centroid.x * (cellSize + cellSpacing) + cellSize * 0.5f;
        float centroidCellCenterY = -centroid.y * (cellSize + cellSpacing) - cellSize * 0.5f;
        
        // Calculate offset from dragRect center (pivot at 0.5, 0.5) to centroid cell center
        // dragRect center in local space (relative to top-left anchor) is at (totalWidth/2, -totalHeight/2)
        // Note: Y is negative because with top-left anchor, positive Y goes down
        float rectCenterX = totalWidth * 0.5f;
        float rectCenterY = -totalHeight * 0.5f;
        
        dragCentroidOffset = new Vector2(
            centroidCellCenterX - rectCenterX,      // X: centroid X - rect center X
            centroidCellCenterY - rectCenterY      // Y: centroid Y - rect center Y
        );
        
        // Get canvas scale for proper screen space conversion
        Canvas canvas = dragCanvas != null ? dragCanvas : dragRect.GetComponentInParent<Canvas>();
        float canvasScale = canvas != null ? canvas.scaleFactor : 1f;
        
        // Position at mouse, offset by centroid to center the shape under cursor
        // Formula: ghost.position = mouseWorldPosition - (centroidOffset * canvasScale)
        Vector3 mousePos = GetMousePosition();
        Vector3 offset = new Vector3(dragCentroidOffset.x * canvasScale, dragCentroidOffset.y * canvasScale, 0);
        dragRect.position = mousePos - offset;
        
        Debug.Log($"[EffigyGridUI] Ghost positioning - Centroid: ({centroid.x:F2}, {centroid.y:F2}), " +
                  $"CellCenter: ({centroidCellCenterX:F2}, {centroidCellCenterY:F2}), " +
                  $"RectCenter: ({rectCenterX:F2}, {rectCenterY:F2}), " +
                  $"Offset: ({dragCentroidOffset.x:F2}, {dragCentroidOffset.y:F2}), " +
                  $"MousePos: ({mousePos.x:F0}, {mousePos.y:F0}), " +
                  $"FinalPos: ({dragRect.position.x:F0}, {dragRect.position.y:F0})");
        
        // Force update to ensure positioning is applied
        Canvas.ForceUpdateCanvases();
        
        // Make slightly transparent
        CanvasGroup canvasGroup = dragVisual.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0.9f; // 70% opacity for ghost effect
        canvasGroup.blocksRaycasts = false; // Allow mouse to pass through to grid
        
        // Ghost transparency value (70% opacity)
        float ghostAlpha = 0.30f;
        
        Sprite[] spriteSet = EffigySpriteSet.GetSprites(effigy.effigyName);
        int spriteIndex = 0;
        Color elementColor = effigy.GetElementColor();
        Color borderColor = GetRarityColor(effigy.rarity);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index >= effigy.shapeMask.Length || !effigy.shapeMask[index])
                    continue;
                
                GameObject cellObj = new GameObject($"Cell_{x}_{y}");
                cellObj.transform.SetParent(dragVisual.transform, false);
                
                RectTransform cellRect = cellObj.AddComponent<RectTransform>();
                float localX = x * (cellSize + cellSpacing);
                float localY = -y * (cellSize + cellSpacing);
                cellRect.anchorMin = new Vector2(0, 1);
                cellRect.anchorMax = new Vector2(0, 1);
                cellRect.pivot = new Vector2(0, 1);
                cellRect.anchoredPosition = new Vector2(localX, localY);
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                
                Image cellImage = cellObj.AddComponent<Image>();
                cellImage.preserveAspect = false;
                cellImage.raycastTarget = false;
                
                Sprite selectedSprite = spriteSet != null && spriteIndex < spriteSet.Length ? spriteSet[spriteIndex] : effigy.icon;
                if (selectedSprite != null)
                {
                    cellImage.sprite = selectedSprite;
                    // Apply transparency to white color
                    cellImage.color = new Color(1f, 1f, 1f, ghostAlpha);
                }
                else
                {
                    float rarityBrightness = GetRarityBrightness(effigy.rarity);
                    // Apply transparency to element color
                    Color colorWithAlpha = elementColor * rarityBrightness;
                    colorWithAlpha.a = ghostAlpha;
                    cellImage.color = colorWithAlpha;
                }
                
                Outline outline = cellObj.AddComponent<Outline>();
                // Apply transparency to outline color as well
                Color outlineColorWithAlpha = borderColor;
                outlineColorWithAlpha.a = ghostAlpha;
                outline.effectColor = outlineColorWithAlpha;
                outline.effectDistance = new Vector2(2f, -2f);
                
                spriteIndex++;
            }
        }
        
        Debug.Log($"[EffigyGridUI] Created drag visual for {effigy.effigyName} - Centroid: ({centroid.x:F2}, {centroid.y:F2}), Offset: ({dragCentroidOffset.x:F2}, {dragCentroidOffset.y:F2}), Pivot: {dragRect.pivot}, Occupied cells: {occupiedCount}, Size: {dragRect.sizeDelta}");
    }
    
    /// <summary>
    /// Get mouse position using the new Input System
    /// </summary>
    Vector3 GetMousePosition()
    {
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            return new Vector3(mousePos.x, mousePos.y, 0);
        }
        // Fallback to screen center if mouse is not available
        return new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
    }
    
    void LateUpdate()
    {
        // Update drag visual position to follow cursor (LateUpdate ensures it's last)
        // Apply centroid offset to keep the shape centered under the mouse
        if (dragVisual != null && draggedEffigy != null)
        {
            RectTransform dragRect = dragVisual.GetComponent<RectTransform>();
            if (dragRect != null)
            {
                // Get canvas scale for proper screen space conversion
                Canvas canvas = dragCanvas != null ? dragCanvas : dragRect.GetComponentInParent<Canvas>();
                float canvasScale = canvas != null ? canvas.scaleFactor : 1f;
                
                // Position at mouse, offset by centroid to center the shape under cursor
                // Formula: ghost.position = mouseWorldPosition - (centroidOffset * canvasScale)
                Vector3 mousePos = GetMousePosition();
                dragRect.position = mousePos - new Vector3(dragCentroidOffset.x * canvasScale, dragCentroidOffset.y * canvasScale, 0);
            }
            
            // Detect mouse release outside grid (for drag-out unequip)
            if (!isDraggingFromStorage && Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                bool isOutside = IsMouseOutsideGrid();
                Debug.Log($"[EffigyGridUI] Mouse released (left-click), dragging: {draggedEffigy.effigyName}, outside grid: {isOutside}");
                
                if (isOutside)
                {
                    Debug.Log($"[EffigyGridUI] 🗑️ Released {draggedEffigy.effigyName} outside grid - unequipping");
                    UnequipEffigy(draggedEffigy);
                    ClearHighlight();
                    DestroyDragVisual();
                    draggedEffigy = null;
                    currentHoveredCell = new Vector2Int(-1, -1);
                    originalGridPosition = new Vector2Int(-1, -1);
                }
            }
            
            // Cancel drag with ESC or right-click (only for storage drags)
            bool escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            bool rightClickPressed = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
            if (isDraggingFromStorage && (escapePressed || rightClickPressed))
            {
                Debug.Log("[EffigyGridUI] Drag cancelled");
                ClearHighlight();
                DestroyDragVisual();
                RestoreSourceSlot();
                draggedEffigy = null;
                isDraggingFromStorage = false;
                sourceStorage = null;
                sourceStorageSlot = null;
                currentHoveredCell = new Vector2Int(-1, -1);
            }
        }
    }
    
    /// <summary>
    /// Check if mouse is outside grid bounds
    /// </summary>
    bool IsMouseOutsideGrid()
    {
        if (gridContainer == null) return true;
        
        RectTransform gridRect = gridContainer.GetComponent<RectTransform>();
        if (gridRect == null) return true;
        
        // Convert grid rect to screen space
        Vector3[] corners = new Vector3[4];
        gridRect.GetWorldCorners(corners);
        
        Vector2 min = corners[0];
        Vector2 max = corners[2];
        
        Vector2 mousePos = GetMousePosition();
        
        // Check if mouse is outside grid bounds
        return mousePos.x < min.x || mousePos.x > max.x || 
               mousePos.y < min.y || mousePos.y > max.y;
    }
    
    void OnDestroy()
    {
        // Cleanup drag canvas when this component is destroyed
        if (dragCanvas != null)
        {
            Destroy(dragCanvas.gameObject);
        }
    }
    
    Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return new Color(0.5f, 0.5f, 0.5f);
            case ItemRarity.Magic: return new Color(0.2f, 0.6f, 1f);
            case ItemRarity.Rare: return new Color(1f, 0.8f, 0.2f);
            case ItemRarity.Unique: return new Color(1f, 0.6f, 0.2f);
            default: return Color.white;
        }
    }
    
    void OnCellMouseDown(int x, int y, PointerEventData eventData)
    {
        Debug.Log($"[EffigyGridUI] OnCellMouseDown at ({x}, {y}), button: {eventData.button}");
        
        Effigy effigy = placedEffigies[y, x];
        if (effigy != null)
        {
            Debug.Log($"[EffigyGridUI] Found effigy: {effigy.effigyName}");
            
            // Right-click = unequip
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log($"[EffigyGridUI] RIGHT-CLICK detected on {effigy.effigyName} - attempting unequip");
                UnequipEffigy(effigy);
                return;
            }
            
            // Left-click = start dragging existing effigy
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                draggedEffigy = effigy;
                isDraggingFromStorage = false;
                originalGridPosition = new Vector2Int(x, y); // Remember original position
                CreateDragVisual(effigy); // This will calculate and store the pickup point
                RemoveEffigy(effigy); // Temporarily remove so we can drag it
                Debug.Log($"[EffigyGridUI] Started dragging {effigy.effigyName} from grid at ({x}, {y}), pickup point: ({pickupPoint.x}, {pickupPoint.y})");
            }
        }
        else
        {
            Debug.Log($"[EffigyGridUI] No effigy at ({x}, {y})");
        }
    }
    
    void OnCellMouseEnter(int x, int y)
    {
        if (draggedEffigy != null)
        {
            currentHoveredCell = new Vector2Int(x, y);
            // Calculate placement position accounting for pickup point offset
            // If pickup point is (px, py) and we want it at grid cell (x, y),
            // then effigy origin should be at (x - px, y - py)
            int placementX = x - pickupPoint.x;
            int placementY = y - pickupPoint.y;
            PreviewPlacement(draggedEffigy, placementX, placementY);
            return;
        }

        Effigy effigy = placedEffigies != null ? placedEffigies[y, x] : null;
        if (effigy != null && ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.ShowEffigyTooltip(effigy, GetMousePosition());
        }
        else if (ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.HideTooltip();
        }
    }

    void OnCellMouseExit(int x, int y)
    {
        if (draggedEffigy != null)
            return;

        if (ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.HideTooltip();
        }
    }
    
    void OnCellMouseUp(int x, int y, PointerEventData eventData)
    {
        if (draggedEffigy == null) return;
        
        // OnCellMouseUp only fires when released ON a cell
        // Mouse release outside grid is handled in LateUpdate()
        
        // Try to place at hovered cell, accounting for pickup point offset
        if (currentHoveredCell.x >= 0 && currentHoveredCell.y >= 0)
        {
            // Calculate placement position accounting for pickup point offset
            // If pickup point is (px, py) and we want it at grid cell (x, y),
            // then effigy origin should be at (x - px, y - py)
            int placementX = currentHoveredCell.x - pickupPoint.x;
            int placementY = currentHoveredCell.y - pickupPoint.y;
            bool placed = TryPlaceEffigy(draggedEffigy, placementX, placementY);
            
            if (placed)
            {
                Debug.Log($"[EffigyGridUI] ✓ Successfully placed {draggedEffigy.effigyName}");
                
                // Remove from storage since it's now equipped/placed (only for storage drags)
                if (isDraggingFromStorage && sourceStorage != null && draggedEffigy != null)
                {
                    sourceStorage.RemoveEffigy(draggedEffigy);
                    Debug.Log($"[EffigyGridUI] Removed {draggedEffigy.effigyName} from storage (now equipped)");
                }
                // If dragging from grid (repositioning), it's already placed, nothing more needed
            }
            else
            {
                Debug.LogWarning($"[EffigyGridUI] ✗ Failed to place {draggedEffigy.effigyName}");
                
                // If dragging from grid and placement failed, restore to original position
                if (!isDraggingFromStorage)
                {
                    if (originalGridPosition.x >= 0 && originalGridPosition.y >= 0)
                    {
                        // Restore to original position
                        bool restored = TryPlaceEffigy(draggedEffigy, originalGridPosition.x, originalGridPosition.y);
                        if (!restored)
                        {
                            // If can't restore, unequip (shouldn't happen, but safety)
                            if (effigyStorage != null)
                            {
                                UnequipEffigy(draggedEffigy);
                            }
                        }
                    }
                    else
                    {
                        // No original position, unequip
                        if (effigyStorage != null)
                        {
                            UnequipEffigy(draggedEffigy);
                        }
                    }
                }
                else
                {
                    // Restore slot visual since placement failed (storage drag)
                    RestoreSourceSlot();
                }
            }
            
            ClearHighlight();
            DestroyDragVisual();
            
            // Only restore if placement failed and from storage (already handled above)
            if (!placed && isDraggingFromStorage)
            {
                RestoreSourceSlot();
            }
            
            draggedEffigy = null;
            isDraggingFromStorage = false;
            sourceStorage = null;
            sourceStorageSlot = null;
            originalGridPosition = new Vector2Int(-1, -1);
            currentHoveredCell = new Vector2Int(-1, -1);
        }
    }
    
    /// <summary>
    /// Restore the source storage slot's visual after drag
    /// </summary>
    void RestoreSourceSlot()
    {
        if (sourceStorageSlot != null)
        {
            sourceStorageSlot.SetDragging(false);
            sourceStorageSlot = null;
        }
    }
    
    /// <summary>
    /// Destroy the drag visual
    /// </summary>
    void DestroyDragVisual()
    {
        if (dragVisual != null)
        {
            Destroy(dragVisual);
            dragVisual = null;
        }
    }
    
    public bool TryPlaceEffigy(Effigy effigy, int gridX, int gridY)
    {
        if (!CanPlaceEffigy(effigy, gridX, gridY))
            return false;
        
        RemoveEffigy(effigy);
        
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            if (worldX >= 0 && worldX < GRID_WIDTH && worldY >= 0 && worldY < GRID_HEIGHT)
            {
                placedEffigies[worldY, worldX] = effigy;
            }
        }
        
        CreateEffigyVisual(effigy, gridX, gridY);
        
        // Sync equipped effigies to Character
        SyncEquippedEffigiesToCharacter();
        
        // Notify EquipmentManager to recalculate and apply stats
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.CalculateTotalEquipmentStats();
            EquipmentManager.Instance.ApplyEquipmentStats();
        }
        
        return true;
    }
    
    public bool CanPlaceEffigy(Effigy effigy, int gridX, int gridY)
    {
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            // Check bounds
            if (worldX < 0 || worldX >= GRID_WIDTH || worldY < 0 || worldY >= GRID_HEIGHT)
                return false;
            
            // Check if cell is already occupied by a DIFFERENT effigy
            Effigy occupyingEffigy = placedEffigies[worldY, worldX];
            if (occupyingEffigy != null && occupyingEffigy != effigy)
                return false;
        }
        
        return true;
    }
    
    public void RemoveEffigy(Effigy effigy)
    {
        if (effigy == null) return;
        
        // Clear grid data
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (placedEffigies[y, x] == effigy)
                    placedEffigies[y, x] = null;
            }
        }
        
        // Remove visuals
        if (effigyVisuals.TryGetValue(effigy, out List<GameObject> visuals))
        {
            foreach (var visual in visuals)
            {
                if (visual != null)
                {
                    Destroy(visual);
                }
            }
            effigyVisuals.Remove(effigy);
        }
    }
    
    /// <summary>
    /// Unequip an effigy and return it to storage
    /// </summary>
    public void UnequipEffigy(Effigy effigy)
    {
        Debug.Log($"[EffigyGridUI] UnequipEffigy called for: {(effigy != null ? effigy.effigyName : "NULL")}");
        
        if (effigy == null)
        {
            Debug.LogWarning("[EffigyGridUI] ❌ Cannot unequip null effigy");
            return;
        }
        
        if (effigyStorage == null)
        {
            Debug.LogError("[EffigyGridUI] ❌ NO EFFIGY STORAGE REFERENCE! Cannot unequip effigy. PLEASE ASSIGN IN INSPECTOR!");
            return;
        }
        
        Debug.Log($"[EffigyGridUI] Removing {effigy.effigyName} from grid...");
        
        // Remove from grid
        RemoveEffigy(effigy);
        
        Debug.Log($"[EffigyGridUI] Adding {effigy.effigyName} back to storage...");
        
        // Add back to storage
        effigyStorage.AddEffigy(effigy);
        
        // Sync equipped effigies to Character (effigy is already removed from grid by RemoveEffigy)
        SyncEquippedEffigiesToCharacter();
        
        // Notify EquipmentManager to recalculate and apply stats
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.CalculateTotalEquipmentStats();
            EquipmentManager.Instance.ApplyEquipmentStats();
        }
        
        Debug.Log($"[EffigyGridUI] ✅ Successfully unequipped {effigy.effigyName} - returned to storage");
    }
    
    /// <summary>
    /// Sync equipped effigies from grid to Character.equippedEffigies
    /// </summary>
    private void SyncEquippedEffigiesToCharacter()
    {
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        if (character == null)
        {
            return;
        }
        
        if (character.equippedEffigies == null)
        {
            character.equippedEffigies = new List<Effigy>();
        }
        
        // Get unique placed effigies
        HashSet<Effigy> uniqueEffigies = new HashSet<Effigy>();
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (placedEffigies[y, x] != null)
                    uniqueEffigies.Add(placedEffigies[y, x]);
            }
        }
        
        // Update Character.equippedEffigies
        character.equippedEffigies.Clear();
        character.equippedEffigies.AddRange(uniqueEffigies);
        
        Debug.Log($"[EffigyGridUI] Synced {character.equippedEffigies.Count} equipped effigies to Character");
    }
    
    void CreateEffigyVisual(Effigy effigy, int gridX, int gridY)
    {
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        Sprite[] spriteSet = EffigySpriteSet.GetSprites(effigy.effigyName);
        Color elementColor = effigy.GetElementColor();
        float rarityBrightness = GetRarityBrightness(effigy.rarity);
        Color borderColor = GetRarityColor(effigy.rarity);
        
        List<GameObject> visuals = new List<GameObject>();
        int spriteIndex = 0;
        
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            if (worldX < 0 || worldX >= GRID_WIDTH || worldY < 0 || worldY >= GRID_HEIGHT)
                continue;
            
            int cellIndex = worldY * GRID_WIDTH + worldX;
            if (cellIndex < 0 || cellIndex >= gridCells.Count)
                continue;
            
            EffigyGridCellUI cellUI = gridCells[cellIndex];
            GameObject effigyVisual = new GameObject($"Effigy_{effigy.effigyName}_{cell.x}_{cell.y}");
            effigyVisual.transform.SetParent(cellUI.transform, false);
            
            RectTransform rt = effigyVisual.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            
            Image img = effigyVisual.AddComponent<Image>();
            img.preserveAspect = false;
            img.raycastTarget = false;
            
            Sprite selectedSprite = spriteSet != null && spriteIndex < spriteSet.Length ? spriteSet[spriteIndex] : effigy.icon;
            if (selectedSprite != null)
            {
                img.sprite = selectedSprite;
                img.color = Color.white;
            }
            else
            {
                img.color = elementColor * rarityBrightness;
            }
            
            Outline outline = effigyVisual.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(2f, -2f);
            
            visuals.Add(effigyVisual);
            spriteIndex++;
        }
        
        if (visuals.Count > 0)
        {
            effigyVisuals[effigy] = visuals;
            Debug.Log($"[EffigyGridUI] Created {visuals.Count} visual elements for {effigy.effigyName}");
        }
    }
    
    void PreviewPlacement(Effigy effigy, int gridX, int gridY)
    {
        ClearHighlight();
        
        bool canPlace = CanPlaceEffigy(effigy, gridX, gridY);
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            if (worldX >= 0 && worldX < GRID_WIDTH && worldY >= 0 && worldY < GRID_HEIGHT)
            {
                int cellIndex = worldY * GRID_WIDTH + worldX;
                if (cellIndex >= 0 && cellIndex < gridCells.Count)
                {
                    EffigyGridCellUI cellUI = gridCells[cellIndex];
                    cellUI.SetHighlight(canPlace ? validPlacementColor : invalidPlacementColor);
                }
            }
        }
    }
    
    void ClearHighlight()
    {
        foreach (var cell in gridCells)
        {
            cell.ClearHighlight();
        }
    }
    
    float GetRarityBrightness(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return 0.7f;
            case ItemRarity.Magic: return 0.85f;
            case ItemRarity.Rare: return 1.0f;
            case ItemRarity.Unique: return 1.1f;
            default: return 0.8f;
        }
    }
    
    public List<Effigy> GetPlacedEffigies()
    {
        HashSet<Effigy> uniqueEffigies = new HashSet<Effigy>();
        
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (placedEffigies[y, x] != null)
                    uniqueEffigies.Add(placedEffigies[y, x]);
            }
        }
        
        return new List<Effigy>(uniqueEffigies);
    }
}

