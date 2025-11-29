using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the 6x4 Effigy grid system in the Equipment Screen
/// Handles placement, validation, and visual representation of effigies
/// </summary>
public class EffigyGrid
{
    private const int GRID_WIDTH = 6;
    private const int GRID_HEIGHT = 4;
    private const int CELL_SIZE = 60; // Match inventory slot size
    
    private VisualElement gridContainer;
    private List<VisualElement> gridCells = new List<VisualElement>();
    private Effigy[,] placedEffigies = new Effigy[GRID_HEIGHT, GRID_WIDTH];
    private Dictionary<Effigy, List<VisualElement>> effigyVisuals = new Dictionary<Effigy, List<VisualElement>>();
    
    // Drag and drop
    private Effigy draggedEffigy = null;
    private Vector2Int dragStartPosition = Vector2Int.zero;
    private Vector2Int currentHoveredCell = new Vector2Int(-1, -1); // Track currently hovered cell
    private HashSet<VisualElement> highlightedCells = new HashSet<VisualElement>();
    private HashSet<VisualElement> occupiedCellHighlights = new HashSet<VisualElement>();
    private bool isDraggingFromStorage = false;
    
    public event Action<IReadOnlyCollection<Effigy>> EffigiesChanged;
    
    public EffigyGrid(VisualElement parent)
    {
        CreateGrid(parent);
    }
    
    /// <summary>
    /// Create the 6x4 grid visual
    /// </summary>
    private void CreateGrid(VisualElement parent)
    {
        gridContainer = new VisualElement();
        gridContainer.name = "EffigyGrid";
        gridContainer.style.flexDirection = FlexDirection.Column;
        
        // Calculate total width needed: 6 cells * (60px + 2px margin) = 372px
        float totalWidth = GRID_WIDTH * CELL_SIZE + (GRID_WIDTH - 1) * 2; // 6*60 + 5*2 = 370px
        float totalHeight = GRID_HEIGHT * CELL_SIZE + (GRID_HEIGHT - 1) * 2; // 4*60 + 3*2 = 246px
        
        gridContainer.style.width = totalWidth;
        gridContainer.style.height = totalHeight;
        gridContainer.style.minWidth = totalWidth;
        gridContainer.style.maxWidth = totalWidth;
        
        // Create rows
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            VisualElement row = new VisualElement();
            row.name = $"EffigyRow_{y}";
            row.style.flexDirection = FlexDirection.Row;
            row.style.width = Length.Percent(100);
            row.style.height = CELL_SIZE;
            
            // Create cells in this row
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                VisualElement cell = new VisualElement();
                cell.name = $"EffigyCell_{x}_{y}";
                cell.style.width = CELL_SIZE;
                cell.style.height = CELL_SIZE;
                cell.style.flexShrink = 0;
                cell.style.flexGrow = 0;
                
                // Only add left margin if not first cell in row
                if (x > 0)
                {
                    cell.style.marginLeft = 2;
                }
                // Only add top margin if not first row
                if (y > 0)
                {
                    row.style.marginTop = 2;
                }
                
                // Style the cell (border/background to show grid)
                cell.style.borderLeftWidth = 1;
                cell.style.borderRightWidth = 1;
                cell.style.borderTopWidth = 1;
                cell.style.borderBottomWidth = 1;
                cell.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
                cell.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
                cell.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
                cell.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
                cell.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
                
                // Add drag and drop handlers
                int cellX = x;
                int cellY = y;
                cell.RegisterCallback<MouseDownEvent>(evt => OnCellMouseDown(cellX, cellY, evt));
                cell.RegisterCallback<MouseEnterEvent>(evt => OnCellMouseEnter(cellX, cellY));
                cell.RegisterCallback<MouseUpEvent>(evt => OnCellMouseUp(cellX, cellY, evt));
                
                // Also handle click for direct placement
                cell.RegisterCallback<ClickEvent>(evt => {
                    Debug.Log($"[EffigyGrid] Cell ({cellX}, {cellY}) clicked");
                    OnCellClick(cellX, cellY);
                });
                
                row.Add(cell);
                gridCells.Add(cell);
            }
            
            gridContainer.Add(row);
        }
        
        parent.Add(gridContainer);
        
        Debug.Log($"Created Effigy grid: {GRID_WIDTH}x{GRID_HEIGHT} = {gridCells.Count} cells");
    }
    
    /// <summary>
    /// Try to place an effigy at grid position (x, y)
    /// Returns true if placement was successful
    /// </summary>
    public bool TryPlaceEffigy(Effigy effigy, int gridX, int gridY)
    {
        if (effigy == null) return false;
        
        // Check if effigy can fit at this position (excluding its own current position if already placed)
        if (!CanPlaceEffigy(effigy, gridX, gridY))
        {
            Debug.Log($"Cannot place {effigy.effigyName} at ({gridX}, {gridY}) - shape doesn't fit or overlaps");
            return false;
        }
        
        // Remove effigy from old position if it was already placed (before placing at new position)
        RemoveEffigy(effigy);
        
        // Place effigy in grid data
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
        
        // Create visual representation
        CreateEffigyVisual(effigy, gridX, gridY);
        
        Debug.Log($"[EffigyGrid] Successfully placed {effigy.effigyName} at ({gridX}, {gridY}) - Occupying {occupiedCells.Count} cells");
        
        // Log grid state for debugging
        int placedCount = 0;
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (placedEffigies[y, x] == effigy)
                    placedCount++;
            }
        }
        Debug.Log($"[EffigyGrid] Grid state: {effigy.effigyName} occupies {placedCount} cells in grid data");

        NotifyEffigiesChanged();
        
        return true;
    }
    
    /// <summary>
    /// Check if an effigy can be placed at the given grid position
    /// This method ignores the effigy's own current position if it's already placed
    /// </summary>
    public bool CanPlaceEffigy(Effigy effigy, int gridX, int gridY)
    {
        if (effigy == null) return false;
        
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            // Check bounds
            if (worldX < 0 || worldX >= GRID_WIDTH || worldY < 0 || worldY >= GRID_HEIGHT)
            {
                Debug.Log($"Cannot place at ({gridX}, {gridY}): cell ({worldX}, {worldY}) is out of bounds");
                return false;
            }
            
            // Check if cell is already occupied by a DIFFERENT effigy
            Effigy occupyingEffigy = placedEffigies[worldY, worldX];
            if (occupyingEffigy != null && occupyingEffigy != effigy)
            {
                Debug.Log($"Cannot place at ({gridX}, {gridY}): cell ({worldX}, {worldY}) is occupied by {occupyingEffigy.effigyName}");
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Remove an effigy from the grid
    /// </summary>
    public void RemoveEffigy(Effigy effigy)
    {
        if (effigy == null) return;
        
        // Clear preview highlights if removing
        ClearHighlight();
        
        // Clear grid data
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (placedEffigies[y, x] == effigy)
                {
                    placedEffigies[y, x] = null;
                }
            }
        }
        
        // Remove visual
        if (effigyVisuals.ContainsKey(effigy))
        {
            foreach (var visual in effigyVisuals[effigy])
            {
                visual.RemoveFromHierarchy();
            }
            effigyVisuals.Remove(effigy);
        }

        NotifyEffigiesChanged();
    }
    
    /// <summary>
    /// Create visual representation of effigy
    /// </summary>
    private void CreateEffigyVisual(Effigy effigy, int gridX, int gridY)
    {
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        List<VisualElement> visuals = new List<VisualElement>();
        
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            int cellIndex = worldY * GRID_WIDTH + worldX;
            if (cellIndex < 0 || cellIndex >= gridCells.Count)
                continue;
            
            VisualElement cellElement = gridCells[cellIndex];
            
            // Check if cell is already occupied by another visual
            // Clear any existing children that are effigy visuals
            for (int i = cellElement.childCount - 1; i >= 0; i--)
            {
                VisualElement child = cellElement[i];
                if (child.name.StartsWith("Effigy_"))
                {
                    cellElement.RemoveAt(i);
                }
            }
            
            // Create effigy visual
            VisualElement effigyCell = new VisualElement();
            effigyCell.name = $"Effigy_{effigy.effigyName}_{cell.x}_{cell.y}";
            effigyCell.style.width = Length.Percent(100);
            effigyCell.style.height = Length.Percent(100);
            effigyCell.style.position = Position.Relative;
            
            // Use element color with rarity brightness adjustment
            Color elementColor = effigy.GetElementColor();
            float rarityBrightness = GetRarityBrightness(effigy.rarity);
            Color finalColor = elementColor * rarityBrightness;
            effigyCell.style.backgroundColor = new StyleColor(finalColor);
            effigyCell.style.opacity = 0.9f;
            
            // Add border color based on rarity
            Color borderColor = GetRarityColor(effigy.rarity);
            effigyCell.style.borderLeftColor = new StyleColor(borderColor);
            effigyCell.style.borderRightColor = new StyleColor(borderColor);
            effigyCell.style.borderTopColor = new StyleColor(borderColor);
            effigyCell.style.borderBottomColor = new StyleColor(borderColor);
            effigyCell.style.borderLeftWidth = 3;
            effigyCell.style.borderRightWidth = 3;
            effigyCell.style.borderTopWidth = 3;
            effigyCell.style.borderBottomWidth = 3;
            
            // Add icon if available (as background image)
            if (effigy.icon != null)
            {
                effigyCell.style.backgroundImage = new StyleBackground(effigy.icon);
                effigyCell.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
            }
            
            // Only show name label on the first cell (top-left) to avoid duplication
            if (cell.x == 0 && cell.y == 0)
            {
                Label nameLabel = new Label(effigy.effigyName);
                nameLabel.style.fontSize = 9;
                nameLabel.style.color = new StyleColor(Color.white);
                nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                nameLabel.style.position = Position.Absolute;
                nameLabel.style.width = Length.Percent(100);
                nameLabel.style.height = Length.Percent(100);
                nameLabel.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.6f));
                nameLabel.style.paddingLeft = 2;
                nameLabel.style.paddingRight = 2;
                effigyCell.Add(nameLabel);
            }
            
            cellElement.Add(effigyCell);
            visuals.Add(effigyCell);
            
            Debug.Log($"[EffigyGrid] Added visual to cell ({worldX}, {worldY}) for {effigy.effigyName}");
        }
        
        if (visuals.Count > 0)
        {
            effigyVisuals[effigy] = visuals;
            Debug.Log($"Created {visuals.Count} visual elements for {effigy.effigyName} at ({gridX}, {gridY})");
        }
        else
        {
            Debug.LogWarning($"Failed to create visuals for {effigy.effigyName} at ({gridX}, {gridY}) - no valid cells!");
        }
    }

    public IReadOnlyCollection<Effigy> GetPlacedEffigies()
    {
        return CollectPlacedEffigies();
    }

    private HashSet<Effigy> CollectPlacedEffigies()
    {
        HashSet<Effigy> result = new HashSet<Effigy>();
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                Effigy effigy = placedEffigies[y, x];
                if (effigy != null)
                {
                    result.Add(effigy);
                }
            }
        }
        return result;
    }

    private void NotifyEffigiesChanged()
    {
        var snapshot = CollectPlacedEffigies();
        EffigiesChanged?.Invoke(snapshot);
    }
    
    /// <summary>
    /// Get rarity color for borders (distinct from element colors)
    /// </summary>
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return new Color(0.5f, 0.5f, 0.5f); // Gray
            case ItemRarity.Magic: return new Color(0.2f, 0.6f, 1f); // Blue
            case ItemRarity.Rare: return new Color(1f, 0.8f, 0.2f); // Gold/Yellow
            case ItemRarity.Unique: return new Color(1f, 0.6f, 0.2f); // Orange
            default: return Color.white;
        }
    }
    
    /// <summary>
    /// Get brightness multiplier based on rarity (for element color tinting)
    /// </summary>
    private float GetRarityBrightness(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return 0.7f; // Dimmer for normal
            case ItemRarity.Magic: return 0.85f; // Slightly dimmer for magic
            case ItemRarity.Rare: return 1.0f; // Full brightness for rare
            case ItemRarity.Unique: return 1.1f; // Brighter for unique
            default: return 0.8f;
        }
    }
    
    /// <summary>
    /// Mouse down handler for grid cells
    /// </summary>
    private void OnCellMouseDown(int x, int y, MouseDownEvent evt)
    {
        Effigy effigy = placedEffigies[y, x];
        if (effigy != null)
        {
            // Start dragging this effigy
            draggedEffigy = effigy;
            dragStartPosition = new Vector2Int(x, y);
            Debug.Log($"Started dragging {effigy.effigyName} from ({x}, {y})");
        }
    }
    
    /// <summary>
    /// Mouse enter handler for preview placement
    /// </summary>
    private void OnCellMouseEnter(int x, int y)
    {
        if (draggedEffigy != null)
        {
            currentHoveredCell = new Vector2Int(x, y); // Track hovered cell
            PreviewPlacement(draggedEffigy, x, y);
        }
    }
    
    /// <summary>
    /// Cell click handler for direct placement
    /// </summary>
    private void OnCellClick(int x, int y)
    {
        if (draggedEffigy != null)
        {
            Debug.Log($"[EffigyGrid] Cell clicked with dragged effigy: {draggedEffigy.effigyName}");
            OnCellMouseUp(x, y, null);
        }
    }
    
    /// <summary>
    /// Mouse up handler - place or remove effigy
    /// </summary>
    private void OnCellMouseUp(int x, int y, MouseUpEvent evt)
    {
        if (draggedEffigy != null)
        {
            // Clear preview highlight
            ClearHighlight();
            
            // Calculate grid position based on effigy shape
            Vector2Int newPosition = CalculatePlacementPosition(draggedEffigy, x, y);
            
            // Try to place at new position
            Debug.Log($"[EffigyGrid] Attempting to place {draggedEffigy.effigyName} at ({newPosition.x}, {newPosition.y})");
            bool placed = TryPlaceEffigy(draggedEffigy, newPosition.x, newPosition.y);
            
            if (placed)
            {
                Debug.Log($"[EffigyGrid] ✓ Successfully placed {draggedEffigy.effigyName}");
            }
            else
            {
                Debug.LogWarning($"[EffigyGrid] ✗ Failed to place {draggedEffigy.effigyName} at ({newPosition.x}, {newPosition.y})");
            }
            
            if (!placed)
            {
                // If can't place and not from storage, put back at original position
                if (!isDraggingFromStorage && dragStartPosition.x >= 0 && dragStartPosition.y >= 0)
                {
                    // Temporarily remove to avoid "already placed" conflicts
                    RemoveEffigy(draggedEffigy);
                    bool restored = TryPlaceEffigy(draggedEffigy, dragStartPosition.x, dragStartPosition.y);
                    if (!restored)
                    {
                        Debug.LogWarning($"Failed to restore {draggedEffigy.effigyName} to original position ({dragStartPosition.x}, {dragStartPosition.y})");
                    }
                }
                else
                {
                    Debug.Log($"Could not place {draggedEffigy.effigyName} at ({newPosition.x}, {newPosition.y}) - location occupied or invalid");
                }
            }
            
            // Unregister global mouse up if from storage
            if (isDraggingFromStorage && gridContainer != null)
            {
                gridContainer.UnregisterCallback<MouseUpEvent>(OnGlobalMouseUp, TrickleDown.TrickleDown);
                gridContainer.UnregisterCallback<ClickEvent>(OnGridClick);
            }
            
            // Clean up state
            draggedEffigy = null;
            isDraggingFromStorage = false;
            currentHoveredCell = new Vector2Int(-1, -1);
        }
    }
    
    /// <summary>
    /// Calculate where to place effigy based on click position
    /// Places so the clicked cell aligns with top-left of effigy shape
    /// </summary>
    private Vector2Int CalculatePlacementPosition(Effigy effigy, int clickX, int clickY)
    {
        // Simple: place so clicked cell becomes top-left of effigy
        // TODO: Better logic to find valid placement near click
        return new Vector2Int(clickX, clickY);
    }
    
    /// <summary>
    /// Preview placement by highlighting cells
    /// </summary>
    private void PreviewPlacement(Effigy effigy, int gridX, int gridY)
    {
        if (effigy == null) return;
        
        ClearHighlight();
        
        bool canPlace = CanPlaceEffigy(effigy, gridX, gridY);
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        
        // First, highlight all cells that are occupied by other effigies
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (placedEffigies[y, x] != null && placedEffigies[y, x] != effigy)
                {
                    int cellIndex = y * GRID_WIDTH + x;
                    if (cellIndex >= 0 && cellIndex < gridCells.Count)
                    {
                        VisualElement cellElement = gridCells[cellIndex];
                        occupiedCellHighlights.Add(cellElement);
                        // Yellow/orange highlight for occupied cells
                        cellElement.style.backgroundColor = new Color(1f, 0.8f, 0.2f, 0.4f); // Yellow/Orange
                        cellElement.style.borderLeftColor = new Color(1f, 0.6f, 0f); // Orange border
                        cellElement.style.borderRightColor = new Color(1f, 0.6f, 0f);
                        cellElement.style.borderTopColor = new Color(1f, 0.6f, 0f);
                        cellElement.style.borderBottomColor = new Color(1f, 0.6f, 0f);
                        cellElement.style.borderLeftWidth = 2;
                        cellElement.style.borderRightWidth = 2;
                        cellElement.style.borderTopWidth = 2;
                        cellElement.style.borderBottomWidth = 2;
                    }
                }
            }
        }
        
        // Then highlight where the effigy would be placed
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            if (worldX >= 0 && worldX < GRID_WIDTH && worldY >= 0 && worldY < GRID_HEIGHT)
            {
                int cellIndex = worldY * GRID_WIDTH + worldX;
                if (cellIndex >= 0 && cellIndex < gridCells.Count)
                {
                    VisualElement cellElement = gridCells[cellIndex];
                    highlightedCells.Add(cellElement);
                    
                    // Check if this cell is occupied by another effigy
                    bool isOccupiedByOther = placedEffigies[worldY, worldX] != null && placedEffigies[worldY, worldX] != effigy;
                    
                    // Highlight based on whether placement is valid
                    if (canPlace && !isOccupiedByOther)
                    {
                        cellElement.style.backgroundColor = new Color(0.2f, 1f, 0.2f, 0.6f); // Green - valid placement
                        cellElement.style.borderLeftColor = new Color(0f, 1f, 0f); // Green border
                        cellElement.style.borderRightColor = new Color(0f, 1f, 0f);
                        cellElement.style.borderTopColor = new Color(0f, 1f, 0f);
                        cellElement.style.borderBottomColor = new Color(0f, 1f, 0f);
                        cellElement.style.borderLeftWidth = 3;
                        cellElement.style.borderRightWidth = 3;
                        cellElement.style.borderTopWidth = 3;
                        cellElement.style.borderBottomWidth = 3;
                    }
                    else
                    {
                        cellElement.style.backgroundColor = new Color(1f, 0.2f, 0.2f, 0.6f); // Red - invalid placement
                        cellElement.style.borderLeftColor = new Color(1f, 0f, 0f); // Red border
                        cellElement.style.borderRightColor = new Color(1f, 0f, 0f);
                        cellElement.style.borderTopColor = new Color(1f, 0f, 0f);
                        cellElement.style.borderBottomColor = new Color(1f, 0f, 0f);
                        cellElement.style.borderLeftWidth = 3;
                        cellElement.style.borderRightWidth = 3;
                        cellElement.style.borderTopWidth = 3;
                        cellElement.style.borderBottomWidth = 3;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Clear all highlighted cells
    /// </summary>
    private void ClearHighlight()
    {
        // Clear placement preview highlights
        foreach (var cell in highlightedCells)
        {
            cell.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
            cell.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
            cell.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
            cell.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            cell.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
            cell.style.borderLeftWidth = 1;
            cell.style.borderRightWidth = 1;
            cell.style.borderTopWidth = 1;
            cell.style.borderBottomWidth = 1;
        }
        highlightedCells.Clear();
        
        // Clear occupied cell highlights
        foreach (var cell in occupiedCellHighlights)
        {
            cell.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
            cell.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
            cell.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
            cell.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            cell.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
            cell.style.borderLeftWidth = 1;
            cell.style.borderRightWidth = 1;
            cell.style.borderTopWidth = 1;
            cell.style.borderBottomWidth = 1;
        }
        occupiedCellHighlights.Clear();
    }
    
    /// <summary>
    /// Start dragging an effigy from storage
    /// </summary>
    public void StartDragFromStorage(Effigy effigy)
    {
        draggedEffigy = effigy;
        isDraggingFromStorage = true;
        dragStartPosition = new Vector2Int(-1, -1);
        currentHoveredCell = new Vector2Int(-1, -1); // Initialize hovered cell
        Debug.Log($"[EffigyGrid] Started dragging {effigy.effigyName} from storage");
        
        // Register global mouse up to place effigy (only for storage drags)
        if (gridContainer != null)
        {
            gridContainer.RegisterCallback<MouseUpEvent>(OnGlobalMouseUp, TrickleDown.TrickleDown);
            // Also register click event on the grid container
            gridContainer.RegisterCallback<ClickEvent>(OnGridClick);
        }
    }
    
    /// <summary>
    /// Handle clicks anywhere on the grid
    /// </summary>
    private void OnGridClick(ClickEvent evt)
    {
        if (draggedEffigy != null)
        {
            // Find which cell was clicked
            VisualElement target = evt.target as VisualElement;
            if (target != null)
            {
                // Try to extract cell coordinates from the target's name
                string cellName = target.name;
                if (cellName.StartsWith("EffigyCell_"))
                {
                    // Parse coordinates from name like "EffigyCell_2_3"
                    string[] parts = cellName.Split('_');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                    {
                        Debug.Log($"[EffigyGrid] Grid clicked at cell ({x}, {y})");
                        OnCellClick(x, y);
                        evt.StopPropagation();
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Global mouse up handler to place effigy at hovered cell or cancel drag
    /// </summary>
    private void OnGlobalMouseUp(MouseUpEvent evt)
    {
        if (isDraggingFromStorage && draggedEffigy != null)
        {
            Debug.Log($"[EffigyGrid] Global mouse up - dragged: {draggedEffigy.effigyName}, hovered cell: ({currentHoveredCell.x}, {currentHoveredCell.y})");
            
            // If we're hovering over a valid cell, try to place the effigy
            if (currentHoveredCell.x >= 0 && currentHoveredCell.y >= 0)
            {
                Debug.Log($"[EffigyGrid] Attempting to place {draggedEffigy.effigyName} at hovered cell ({currentHoveredCell.x}, {currentHoveredCell.y})");
                bool placed = TryPlaceEffigy(draggedEffigy, currentHoveredCell.x, currentHoveredCell.y);
                
                if (placed)
                {
                    Debug.Log($"[EffigyGrid] ✓ Successfully placed {draggedEffigy.effigyName} from storage");
                }
                else
                {
                    Debug.LogWarning($"[EffigyGrid] ✗ Failed to place {draggedEffigy.effigyName} at ({currentHoveredCell.x}, {currentHoveredCell.y})");
                }
            }
            else
            {
                Debug.Log($"[EffigyGrid] No valid cell hovered, canceling drag");
            }
            
            // Clean up
            ClearHighlight();
            draggedEffigy = null;
            isDraggingFromStorage = false;
            currentHoveredCell = new Vector2Int(-1, -1);
            
            if (gridContainer != null)
            {
                gridContainer.UnregisterCallback<MouseUpEvent>(OnGlobalMouseUp, TrickleDown.TrickleDown);
                gridContainer.UnregisterCallback<ClickEvent>(OnGridClick);
            }
        }
    }
    
    /// <summary>
    /// Get VisualElement for the grid container (for adding to UI)
    /// </summary>
    public VisualElement GetVisualElement()
    {
        return gridContainer;
    }
}

