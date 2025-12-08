using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Effigy Storage grid - works like inventory with always-visible cells
/// Displays a scrollable grid of fixed cells where effigies can be stored
/// </summary>
public class EffigyStorageUI : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Number of columns in the storage grid")]
    [SerializeField] private int gridColumns = 4;
    
    [Tooltip("Number of rows in the storage grid (defines total capacity)")]
    [SerializeField] private int gridRows = 20;
    
    [Tooltip("Size of each cell in pixels")]
    [SerializeField] private float cellSize = 80f;
    
    [Tooltip("Spacing between cells in pixels")]
    [SerializeField] private float cellSpacing = 10f;
    
    [Tooltip("Padding around the grid edges")]
    [SerializeField] private float gridPadding = 10f;
    
    [Header("References")]
    [SerializeField] private GameObject cellPrefab; // Prefab for individual cells
    [SerializeField] private Transform gridContainer; // Container where cells are created
    [SerializeField] private EffigyGridUI effigyGrid; // Reference to main effigy grid for drag
    
    [Header("Visual Settings")]
    [SerializeField] private Color emptyCellColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    [SerializeField] private Color borderColor = new Color(0.4f, 0.4f, 0.4f);
    
    private List<EffigyStorageSlotUI> cells = new List<EffigyStorageSlotUI>();
    private List<Effigy> storedEffigies = new List<Effigy>();
    
    void Start()
    {
        GenerateGrid();
        LoadEffigiesFromResources();
    }
    
    /// <summary>
    /// Generate the fixed grid of cells (always visible, like inventory)
    /// </summary>
    void GenerateGrid()
    {
        if (gridContainer == null)
        {
            Debug.LogError("[EffigyStorageUI] Grid container is null!");
            return;
        }
        
        // Clear existing cells
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        cells.Clear();
        
        // Set up GridLayoutGroup
        GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
        gridLayout.spacing = new Vector2(cellSpacing, cellSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridColumns;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.padding = new RectOffset(
            (int)gridPadding, 
            (int)gridPadding, 
            (int)gridPadding, 
            (int)gridPadding
        );
        
        // Add ContentSizeFitter for vertical scrolling
        ContentSizeFitter sizeFitter = gridContainer.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
            sizeFitter = gridContainer.gameObject.AddComponent<ContentSizeFitter>();
        
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Generate all cells (total capacity)
        int totalCells = gridColumns * gridRows;
        
        for (int i = 0; i < totalCells; i++)
        {
            int row = i / gridColumns;
            int col = i % gridColumns;
            
            GameObject cellObj;
            
            if (cellPrefab != null)
            {
                // Use provided prefab
                cellObj = Instantiate(cellPrefab, gridContainer);
            }
            else
            {
                // Create cell dynamically
                cellObj = new GameObject($"StorageCell_{col}_{row}");
                cellObj.transform.SetParent(gridContainer, false);
                
                // Add Image component
                Image cellImage = cellObj.AddComponent<Image>();
                cellImage.color = emptyCellColor;
                
                // Add Outline for border
                Outline outline = cellObj.AddComponent<Outline>();
                outline.effectColor = borderColor;
                outline.effectDistance = new Vector2(2, 2);
            }
            
            cellObj.name = $"StorageCell_{col}_{row}";
            
            // Add or get the slot component
            EffigyStorageSlotUI slotUI = cellObj.GetComponent<EffigyStorageSlotUI>();
            if (slotUI == null)
                slotUI = cellObj.AddComponent<EffigyStorageSlotUI>();
            
            slotUI.Initialize(col, row, emptyCellColor, effigyGrid);
            cells.Add(slotUI);
        }
        
        Debug.Log($"[EffigyStorageUI] Generated {cells.Count} cells ({gridColumns}x{gridRows})");
    }
    
    /// <summary>
    /// Load effigies from character's owned effigies (no blueprints for new characters)
    /// </summary>
    public void LoadEffigiesFromResources()
    {
        storedEffigies.Clear();
        
        CharacterManager characterManager = CharacterManager.Instance;
        Character character = characterManager != null && characterManager.HasCharacter()
            ? characterManager.GetCurrentCharacter()
            : null;
        
        if (character != null && character.ownedEffigies != null && character.ownedEffigies.Count > 0)
        {
            storedEffigies.AddRange(character.ownedEffigies);
            Debug.Log($"[EffigyStorageUI] Loaded {storedEffigies.Count} effigies from character inventory");
        }
        else
        {
            // New characters start with empty effigy storage - no blueprints
            Debug.Log("[EffigyStorageUI] Character has no owned effigies - storage will be empty");
        }
        
        PopulateGrid();
    }
    
    /// <summary>
    /// Place stored effigies into the grid cells
    /// </summary>
    void PopulateGrid()
    {
        // Clear all cells first
        foreach (var cell in cells)
        {
            cell.ClearCell();
        }
        
        // Place effigies in cells
        for (int i = 0; i < storedEffigies.Count && i < cells.Count; i++)
        {
            cells[i].SetEffigy(storedEffigies[i]);
        }
        
        Debug.Log($"[EffigyStorageUI] Populated {storedEffigies.Count} effigies into grid");
    }
    
    /// <summary>
    /// Add an effigy to storage
    /// </summary>
    public void AddEffigy(Effigy effigy)
    {
        if (!storedEffigies.Contains(effigy))
        {
            storedEffigies.Add(effigy);
            
            CharacterManager characterManager = CharacterManager.Instance;
            if (characterManager != null && characterManager.HasCharacter())
            {
                var owned = characterManager.GetCurrentCharacter().ownedEffigies;
                if (owned != null && !owned.Contains(effigy))
                {
                    owned.Add(effigy);
                }
            }
            
            PopulateGrid();
            Debug.Log($"[EffigyStorageUI] Added effigy: {effigy.effigyName}");
        }
    }
    
    /// <summary>
    /// Remove an effigy from storage
    /// </summary>
    public void RemoveEffigy(Effigy effigy)
    {
        if (storedEffigies.Remove(effigy))
        {
            PopulateGrid();
            Debug.Log($"[EffigyStorageUI] Removed effigy: {effigy.effigyName}");
        }
    }
    
    /// <summary>
    /// Clear all effigies from storage
    /// </summary>
    public void ClearStorage()
    {
        storedEffigies.Clear();
        PopulateGrid();
    }
    
    /// <summary>
    /// Get total storage capacity
    /// </summary>
    public int GetCapacity()
    {
        return gridColumns * gridRows;
    }
    
    /// <summary>
    /// Get number of stored effigies
    /// </summary>
    public int GetStoredCount()
    {
        return storedEffigies.Count;
    }
}

/// <summary>
/// Individual cell in the effigy storage grid
/// Displays effigy or remains empty - always visible
/// </summary>
public class EffigyStorageSlotUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Effigy storedEffigy = null;
    private Image backgroundImage;
    private TextMeshProUGUI nameLabel;
    private RectTransform previewRoot;
    private readonly List<GameObject> previewCells = new List<GameObject>();
    private EffigyGridUI targetGrid;
    private int cellX;
    private int cellY;
    private Color emptyColor;
    
    public void Initialize(int x, int y, Color empty, EffigyGridUI grid)
    {
        cellX = x;
        cellY = y;
        emptyColor = empty;
        targetGrid = grid;
        
        // Get or create components
        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null)
            backgroundImage = gameObject.AddComponent<Image>();
        
        backgroundImage.color = emptyColor;
    }
    
    public void SetEffigy(Effigy effigy)
    {
        storedEffigy = effigy;
        
        if (effigy != null)
        {
            // Show effigy visual
            Color elementColor = effigy.GetElementColor();
            backgroundImage.color = elementColor * 0.6f;
            
            RenderPreview(effigy);
            
            // Create or update name label
            if (nameLabel == null)
            {
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(transform, false);
                
                RectTransform labelRect = labelObj.AddComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0, 0);
                labelRect.anchorMax = new Vector2(1, 0);
                labelRect.pivot = new Vector2(0.5f, 0);
                labelRect.offsetMin = new Vector2(2, 2);
                labelRect.offsetMax = new Vector2(-2, 18);
                
                nameLabel = labelObj.AddComponent<TextMeshProUGUI>();
                nameLabel.fontSize = 9;
                nameLabel.alignment = TextAlignmentOptions.Center;
                nameLabel.color = Color.white;
                nameLabel.fontStyle = FontStyles.Bold;
            }
            
            nameLabel.text = effigy.effigyName;
            nameLabel.enabled = true;
        }
        else
        {
            // Clear effigy visual (show empty cell)
            backgroundImage.color = emptyColor;
            
            ClearPreview();
            
            if (nameLabel != null)
                nameLabel.enabled = false;
        }
    }
    
    public void ClearCell()
    {
        SetEffigy(null);
    }
    
    void RenderPreview(Effigy effigy)
    {
        if (previewRoot == null)
        {
            GameObject previewObj = new GameObject("Preview");
            previewObj.transform.SetParent(transform, false);
            previewRoot = previewObj.AddComponent<RectTransform>();
            previewRoot.anchorMin = new Vector2(0.5f, 0.5f);
            previewRoot.anchorMax = new Vector2(0.5f, 0.5f);
            previewRoot.pivot = new Vector2(0.5f, 0.5f);
        }
        
        ClearPreview();
        previewRoot.gameObject.SetActive(true);
        
        RectTransform slotRect = transform as RectTransform;
        if (slotRect == null)
            return;
        
        float padding = 6f;
        float labelReserve = 18f;
        float spacing = 2f;
        
        float availableWidth = Mathf.Max(0f, slotRect.rect.width - padding * 2f);
        float availableHeight = Mathf.Max(0f, slotRect.rect.height - padding * 2f - labelReserve);
        
        int shapeWidth = Mathf.Max(1, effigy.shapeWidth);
        int shapeHeight = Mathf.Max(1, effigy.shapeHeight);
        
        float cellWidth = (availableWidth - spacing * (shapeWidth - 1)) / shapeWidth;
        float cellHeight = (availableHeight - spacing * (shapeHeight - 1)) / shapeHeight;
        float miniCellSize = Mathf.Max(0f, Mathf.Min(cellWidth, cellHeight));
        
        float totalWidth = shapeWidth * miniCellSize + Mathf.Max(0, shapeWidth - 1) * spacing;
        float totalHeight = shapeHeight * miniCellSize + Mathf.Max(0, shapeHeight - 1) * spacing;
        
        previewRoot.sizeDelta = new Vector2(totalWidth, totalHeight);
        previewRoot.anchoredPosition = new Vector2(0f, labelReserve * 0.5f);
        
        Sprite[] spriteSet = EffigySpriteSet.GetSprites(effigy.effigyName);
        Color elementColor = effigy.GetElementColor();
        float rarityBrightness = GetRarityBrightness(effigy.rarity);
        Color borderColor = GetRarityColor(effigy.rarity);
        
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        int spriteIndex = 0;
        
        foreach (Vector2Int cell in occupiedCells)
        {
            GameObject cellObj = new GameObject($"PreviewCell_{cell.x}_{cell.y}");
            cellObj.transform.SetParent(previewRoot, false);
            previewCells.Add(cellObj);
            
            RectTransform cellRect = cellObj.AddComponent<RectTransform>();
            cellRect.anchorMin = new Vector2(0, 1);
            cellRect.anchorMax = new Vector2(0, 1);
            cellRect.pivot = new Vector2(0, 1);
            
            float localX = cell.x * (miniCellSize + spacing);
            float localY = cell.y * (miniCellSize + spacing);
            cellRect.anchoredPosition = new Vector2(localX, -localY);
            cellRect.sizeDelta = new Vector2(miniCellSize, miniCellSize);
            
            Image cellImage = cellObj.AddComponent<Image>();
            cellImage.preserveAspect = false;
            cellImage.raycastTarget = false;
            
            Sprite selectedSprite = spriteSet != null && spriteIndex < spriteSet.Length ? spriteSet[spriteIndex] : effigy.icon;
            if (selectedSprite != null)
            {
                cellImage.sprite = selectedSprite;
                cellImage.color = Color.white;
            }
            else
            {
                cellImage.color = elementColor * rarityBrightness;
            }
            
            Outline outline = cellObj.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(2f, -2f);
            
            spriteIndex++;
        }
    }
    
    void ClearPreview()
    {
        foreach (var cell in previewCells)
        {
            if (cell != null)
            {
                Destroy(cell);
            }
        }
        previewCells.Clear();
        
        if (previewRoot != null)
        {
            previewRoot.gameObject.SetActive(false);
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
    
    /// <summary>
    /// Temporarily hide this slot's visual during drag
    /// </summary>
    public void SetDragging(bool isDragging)
    {
        if (backgroundImage != null)
        {
            Color currentColor = backgroundImage.color;
            currentColor.a = isDragging ? 0.3f : 1f; // Dim to 30% when dragging
            backgroundImage.color = currentColor;
        }
        
        if (previewRoot != null)
            previewRoot.gameObject.SetActive(!isDragging);
        
        if (nameLabel != null)
        {
            nameLabel.enabled = !isDragging;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (storedEffigy != null && targetGrid != null)
        {
            // Get parent storage reference
            EffigyStorageUI storage = GetComponentInParent<EffigyStorageUI>();
            
            // Detect which cell within the effigy shape was clicked
            Vector2Int clickedCell = DetectClickedCell(eventData);
            
            SetDragging(true); // Dim the slot while dragging
            targetGrid.StartDragFromStorage(storedEffigy, this, storage, clickedCell);
            Debug.Log($"[EffigyStorageSlotUI] Started dragging {storedEffigy.effigyName} from storage, clicked cell: ({clickedCell.x}, {clickedCell.y})");
        }
    }
    
    /// <summary>
    /// Detect which cell within the effigy shape was clicked based on mouse position
    /// Returns the cell coordinates relative to the effigy's shape (0,0) origin
    /// Returns (-1, -1) if no occupied cell was clicked or preview is not available
    /// </summary>
    Vector2Int DetectClickedCell(PointerEventData eventData)
    {
        if (storedEffigy == null || previewRoot == null || !previewRoot.gameObject.activeSelf)
        {
            return new Vector2Int(-1, -1);
        }
        
        // Convert mouse position to local coordinates relative to the preview root
        RectTransform previewRect = previewRoot;
        RectTransform slotRect = transform as RectTransform;
        
        if (previewRect == null || slotRect == null)
            return new Vector2Int(-1, -1);
        
        // Convert mouse position directly to preview root's local space
        Vector2 mouseInPreview;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            previewRect, 
            eventData.position, 
            eventData.pressEventCamera, 
            out mouseInPreview
        );
        
        // Preview root uses center anchor (0.5, 0.5), but preview cells use top-left anchor (0, 1)
        // So we need to convert from center-anchored coordinates to top-left-anchored coordinates
        // In center-anchored: (0,0) is center, positive X is right, positive Y is up
        // In top-left-anchored: (0,0) is top-left, positive X is right, positive Y is down
        float previewWidth = previewRect.rect.width;
        float previewHeight = previewRect.rect.height;
        
        // Convert from center-anchored to top-left-anchored coordinates
        float previewLocalX = mouseInPreview.x + previewWidth * 0.5f;
        float previewLocalY = previewHeight * 0.5f - mouseInPreview.y; // Invert Y (up becomes down)
        
        // Get the preview cell size and spacing (same calculation as in RenderPreview)
        float padding = 6f;
        float labelReserve = 18f;
        float spacing = 2f;
        float availableWidth = Mathf.Max(0f, slotRect.rect.width - padding * 2f);
        float availableHeight = Mathf.Max(0f, slotRect.rect.height - padding * 2f - labelReserve);
        
        int shapeWidth = Mathf.Max(1, storedEffigy.shapeWidth);
        int shapeHeight = Mathf.Max(1, storedEffigy.shapeHeight);
        
        float cellWidth = (availableWidth - spacing * (shapeWidth - 1)) / shapeWidth;
        float cellHeight = (availableHeight - spacing * (shapeHeight - 1)) / shapeHeight;
        float miniCellSize = Mathf.Max(0f, Mathf.Min(cellWidth, cellHeight));
        
        // Calculate which cell the mouse is over
        // In preview space with top-left origin:
        // Cell (x, y) position: x = cellX * (miniCellSize + spacing), y = cellY * (miniCellSize + spacing)
        int cellX = Mathf.FloorToInt(previewLocalX / (miniCellSize + spacing));
        int cellY = Mathf.FloorToInt(previewLocalY / (miniCellSize + spacing));
        
        // Clamp to valid range
        cellX = Mathf.Clamp(cellX, 0, shapeWidth - 1);
        cellY = Mathf.Clamp(cellY, 0, shapeHeight - 1);
        
        // Check if this cell is actually occupied
        if (storedEffigy.IsCellOccupied(cellX, cellY))
        {
            return new Vector2Int(cellX, cellY);
        }
        
        // If the clicked cell is not occupied, we need to find a valid occupied cell
        // Since storage shows effigies in a single cell, the click position doesn't map precisely
        // to individual shape cells. We'll use a smart fallback strategy.
        List<Vector2Int> occupiedCells = storedEffigy.GetOccupiedCells();
        if (occupiedCells.Count == 0)
            return new Vector2Int(-1, -1);
        
        // Strategy: Find the first occupied cell in reading order (top-to-bottom, left-to-right)
        // This ensures consistent behavior for shapes like Cross, S, T where (0,0) might be empty
        // Sort by Y first (top to bottom), then by X (left to right)
        occupiedCells.Sort((a, b) => 
        {
            if (a.y != b.y) return a.y.CompareTo(b.y);
            return a.x.CompareTo(b.x);
        });
        
        // Return the first occupied cell in reading order
        // This is predictable and works for all shapes, including Cross where center is occupied but corners are empty
        return occupiedCells[0];
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (storedEffigy != null && ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.ShowEffigyTooltip(storedEffigy, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.HideTooltip();
        }
    }
}
