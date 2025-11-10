using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Unity UI version of effigy grid system
/// Manages 6x4 grid for effigy placement with drag & drop
/// </summary>
public class EffigyGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    private const int GRID_WIDTH = 6;
    private const int GRID_HEIGHT = 4;
    
    [Tooltip("Size of each cell in pixels")]
    [SerializeField] private float cellSize = 60f;
    
    [Tooltip("Spacing between cells in pixels")]
    [SerializeField] private float cellSpacing = 2f;
    
    [Header("References")]
    [SerializeField] private GameObject cellPrefab;
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
    
    void Awake()
    {
        GenerateGrid();
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
    
    void GenerateGrid()
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
                    cellUI.OnCellMouseUp += OnCellMouseUp;
                    
                    gridCells.Add(cellUI);
                }
            }
        }
        
        Debug.Log($"[EffigyGridUI] Generated {gridCells.Count} cells ({GRID_WIDTH}x{GRID_HEIGHT})");
    }
    
    public void StartDragFromStorage(Effigy effigy, EffigyStorageSlotUI sourceSlot = null, EffigyStorageUI storage = null)
    {
        draggedEffigy = effigy;
        isDraggingFromStorage = true;
        currentHoveredCell = new Vector2Int(-1, -1);
        sourceStorageSlot = sourceSlot;
        sourceStorage = storage;
        
        CreateDragVisual(effigy);
        
        Debug.Log($"[EffigyGridUI] Started dragging {effigy.effigyName} from storage");
    }
    
    /// <summary>
    /// Create a visual representation of the effigy that follows the cursor
    /// Centered on the effigy's actual occupied cells
    /// </summary>
    void CreateDragVisual(Effigy effigy)
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
        
        // Calculate center of actual occupied cells (not bounding box center)
        Vector2 centerOffset = Vector2.zero;
        int occupiedCount = 0;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index < effigy.shapeMask.Length && effigy.shapeMask[index])
                {
                    centerOffset += new Vector2(x, y);
                    occupiedCount++;
                }
            }
        }
        
        if (occupiedCount > 0)
        {
            centerOffset /= occupiedCount;
        }
        
        // Set size to fit all cells
        dragRect.sizeDelta = new Vector2(width * cellSize + (width - 1) * cellSpacing, 
                                          height * cellSize + (height - 1) * cellSpacing);
        
        // Pivot at center of occupied cells
        dragRect.pivot = new Vector2(centerOffset.x / width, 
                                      1f - (centerOffset.y / height));
        
        // Position at mouse immediately
        dragRect.position = Input.mousePosition;
        
        // Make slightly transparent
        CanvasGroup canvasGroup = dragVisual.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false; // Allow mouse to pass through to grid
        
        // Create visual for each occupied cell
        Color elementColor = effigy.GetElementColor();
        Color borderColor = GetRarityColor(effigy.rarity);
        
        bool isFirstOccupiedCell = true;
        
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
                
                // Position relative to drag visual container
                float localX = x * (cellSize + cellSpacing);
                float localY = -y * (cellSize + cellSpacing);
                
                cellRect.anchorMin = new Vector2(0, 1);
                cellRect.anchorMax = new Vector2(0, 1);
                cellRect.pivot = new Vector2(0, 1);
                cellRect.anchoredPosition = new Vector2(localX, localY);
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                
                // Background
                Image cellImage = cellObj.AddComponent<Image>();
                cellImage.color = elementColor * 0.8f;
                
                // Border
                Outline outline = cellObj.AddComponent<Outline>();
                outline.effectColor = borderColor;
                outline.effectDistance = new Vector2(2, 2);
                
                // Icon (on first occupied cell only)
                if (isFirstOccupiedCell && effigy.icon != null)
                {
                    GameObject iconObj = new GameObject("Icon");
                    iconObj.transform.SetParent(cellObj.transform, false);
                    
                    RectTransform iconRect = iconObj.AddComponent<RectTransform>();
                    iconRect.anchorMin = Vector2.zero;
                    iconRect.anchorMax = Vector2.one;
                    iconRect.offsetMin = new Vector2(5, 5);
                    iconRect.offsetMax = new Vector2(-5, -5);
                    
                    Image iconImage = iconObj.AddComponent<Image>();
                    iconImage.sprite = effigy.icon;
                    iconImage.preserveAspect = true;
                    
                    isFirstOccupiedCell = false;
                }
            }
        }
        
        Debug.Log($"[EffigyGridUI] Created drag visual for {effigy.effigyName} - Center: {centerOffset}, Pivot: {dragRect.pivot}, Occupied cells: {occupiedCount}");
    }
    
    void LateUpdate()
    {
        // Update drag visual position to follow cursor (LateUpdate ensures it's last)
        if (dragVisual != null && draggedEffigy != null)
        {
            RectTransform dragRect = dragVisual.GetComponent<RectTransform>();
            if (dragRect != null)
            {
                dragRect.position = Input.mousePosition;
            }
            
            // Detect mouse release outside grid (for drag-out unequip)
            if (!isDraggingFromStorage && Input.GetMouseButtonUp(0))
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
            if (isDraggingFromStorage && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
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
        
        Vector2 mousePos = Input.mousePosition;
        
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
                CreateDragVisual(effigy);
                RemoveEffigy(effigy); // Temporarily remove so we can drag it
                Debug.Log($"[EffigyGridUI] Started dragging {effigy.effigyName} from grid at ({x}, {y})");
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
            PreviewPlacement(draggedEffigy, x, y);
        }
    }
    
    void OnCellMouseUp(int x, int y, PointerEventData eventData)
    {
        if (draggedEffigy == null) return;
        
        // OnCellMouseUp only fires when released ON a cell
        // Mouse release outside grid is handled in LateUpdate()
        
        // Try to place at hovered cell
        if (currentHoveredCell.x >= 0 && currentHoveredCell.y >= 0)
        {
            bool placed = TryPlaceEffigy(draggedEffigy, currentHoveredCell.x, currentHoveredCell.y);
            
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
        if (effigyVisuals.ContainsKey(effigy))
        {
            foreach (var visual in effigyVisuals[effigy])
            {
                Destroy(visual);
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
        
        Debug.Log($"[EffigyGridUI] ✅ Successfully unequipped {effigy.effigyName} - returned to storage");
    }
    
    void CreateEffigyVisual(Effigy effigy, int gridX, int gridY)
    {
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        List<GameObject> visuals = new List<GameObject>();
        
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            int cellIndex = worldY * GRID_WIDTH + worldX;
            if (cellIndex < 0 || cellIndex >= gridCells.Count)
                continue;
            
            EffigyGridCellUI cellUI = gridCells[cellIndex];
            
            // Create visual GameObject
            GameObject effigyVisual = new GameObject($"Effigy_{effigy.effigyName}_{cell.x}_{cell.y}");
            effigyVisual.transform.SetParent(cellUI.transform, false);
            
            RectTransform rt = effigyVisual.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            
            Image img = effigyVisual.AddComponent<Image>();
            Color elementColor = effigy.GetElementColor();
            float rarityBrightness = GetRarityBrightness(effigy.rarity);
            img.color = elementColor * rarityBrightness;
            
            if (effigy.icon != null)
                img.sprite = effigy.icon;
            
            visuals.Add(effigyVisual);
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

