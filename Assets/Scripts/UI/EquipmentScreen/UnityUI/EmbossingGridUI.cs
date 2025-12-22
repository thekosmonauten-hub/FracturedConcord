using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using Dexiled.UI.EquipmentScreen;

/// <summary>
/// Embossing storage grid - works like inventory with always-visible cells
/// Displays a scrollable grid of fixed cells where embossing effects are stored
/// </summary>
public class EmbossingGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Number of columns in the embossing grid")]
    [SerializeField] private int gridColumns = 4;
    
    [Tooltip("Number of rows in the embossing grid (defines total capacity)")]
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
    
    [Header("Visual Settings")]
    [SerializeField] private Color emptyCellColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    [SerializeField] private Color borderColor = new Color(0.4f, 0.4f, 0.4f);
    
    [Header("Auto-Load Settings")]
    [SerializeField] private bool autoLoadFromResources = true;
    [SerializeField] private string resourcesPath = "Embossings";
    private static readonly string[] ResourceFallbackPaths = { "Embossings", "Embossing/Effects", "Embossing" };
    
    [Header("Callbacks")]
    public System.Action<EmbossingEffect> OnEmbossingClicked;
    
    private List<EmbossingSlotUI> cells = new List<EmbossingSlotUI>();
    private List<EmbossingEffect> storedEmbossings = new List<EmbossingEffect>();
    
    void Start()
    {
        GenerateGrid();
        
        if (autoLoadFromResources)
        {
            LoadEmbossingsFromResources();
        }
    }
    
    /// <summary>
    /// Generate the fixed grid of cells (always visible, like inventory)
    /// </summary>
    void GenerateGrid()
    {
        if (gridContainer == null)
        {
            Debug.LogError("[EmbossingGridUI] Grid container is null!");
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
                cellObj = new GameObject($"EmbossingCell_{col}_{row}");
                cellObj.transform.SetParent(gridContainer, false);
                
                // Add Image component
                Image cellImage = cellObj.AddComponent<Image>();
                cellImage.color = emptyCellColor;
                
                // Add Outline for border
                Outline outline = cellObj.AddComponent<Outline>();
                outline.effectColor = borderColor;
                outline.effectDistance = new Vector2(2, 2);
            }
            
            cellObj.name = $"EmbossingCell_{col}_{row}";
            
            // Add or get the slot component
            EmbossingSlotUI slotUI = cellObj.GetComponent<EmbossingSlotUI>();
            if (slotUI == null)
                slotUI = cellObj.AddComponent<EmbossingSlotUI>();
            
            slotUI.Initialize(col, row, emptyCellColor);
            cells.Add(slotUI);
        }
        
        Debug.Log($"[EmbossingGridUI] Generated {cells.Count} cells ({gridColumns}x{gridRows})");
    }
    
    /// <summary>
    /// Load all embossing effects from Resources folder and place them in grid
    /// </summary>
    void LoadEmbossingsFromResources()
    {
        storedEmbossings.Clear();

        // Build candidate paths starting with the serialized value then falling back to defaults.
        List<string> candidatePaths = new List<string>();
        if (!string.IsNullOrEmpty(resourcesPath))
        {
            candidatePaths.Add(resourcesPath);
        }

        foreach (string fallback in ResourceFallbackPaths)
        {
            if (!candidatePaths.Contains(fallback))
            {
                candidatePaths.Add(fallback);
            }
        }

        foreach (string path in candidatePaths)
        {
            EmbossingEffect[] results = Resources.LoadAll<EmbossingEffect>(path);
            if (results != null && results.Length > 0)
            {
                storedEmbossings.AddRange(results);
                Debug.Log($"[EmbossingGridUI] Loaded {results.Length} embossing effects from Resources/{path}");
                PopulateGrid();
                return;
            }
        }

        Debug.LogWarning($"[EmbossingGridUI] No embossing effects found in candidate resource paths: {string.Join(", ", candidatePaths)}");
    }
    
    /// <summary>
    /// Place stored embossing effects into the grid cells
    /// </summary>
    void PopulateGrid()
    {
        // Clear all cells first
        foreach (var cell in cells)
        {
            cell.ClearCell();
        }
        
        // Place embossings in cells
        for (int i = 0; i < storedEmbossings.Count && i < cells.Count; i++)
        {
            cells[i].SetEmbossing(storedEmbossings[i]);
        }
        
        Debug.Log($"[EmbossingGridUI] Populated {storedEmbossings.Count} embossing effects into grid");
    }
    
    /// <summary>
    /// Manually add embossings (for when you create the system)
    /// </summary>
    public void SetEmbossings(List<EmbossingEffect> embossings)
    {
        storedEmbossings = new List<EmbossingEffect>(embossings);
        PopulateGrid();
    }
    
    /// <summary>
    /// Add an embossing to storage
    /// </summary>
    public void AddEmbossing(EmbossingEffect embossing)
    {
        if (!storedEmbossings.Contains(embossing))
        {
            storedEmbossings.Add(embossing);
            PopulateGrid();
            Debug.Log($"[EmbossingGridUI] Added embossing: {embossing.embossingName}");
        }
    }
    
    /// <summary>
    /// Remove an embossing from storage
    /// </summary>
    public void RemoveEmbossing(EmbossingEffect embossing)
    {
        if (storedEmbossings.Remove(embossing))
        {
            PopulateGrid();
            Debug.Log($"[EmbossingGridUI] Removed embossing: {embossing.embossingName}");
        }
    }
    
    /// <summary>
    /// Clear all embossings from storage
    /// </summary>
    public void ClearStorage()
    {
        storedEmbossings.Clear();
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
    /// Get number of stored embossings
    /// </summary>
    public int GetStoredCount()
    {
        return storedEmbossings.Count;
    }
}

/// <summary>
/// Individual cell in the embossing storage grid
/// Displays embossing effect or remains empty - always visible
/// </summary>
public class EmbossingSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private EmbossingEffect storedEmbossing = null;
    private Image backgroundImage;
    private Image iconImage;
    private TextMeshProUGUI nameLabel;
    private int cellX;
    private int cellY;
    private Color emptyColor;
    private Color hoverColor;
    
    public void Initialize(int x, int y, Color empty)
    {
        cellX = x;
        cellY = y;
        emptyColor = empty;
        hoverColor = emptyColor * 1.3f; // Slightly brighter for hover
        
        // Get or create components
        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null)
            backgroundImage = gameObject.AddComponent<Image>();
        
        backgroundImage.color = emptyColor;
    }
    
    public void SetEmbossing(EmbossingEffect embossing)
    {
        storedEmbossing = embossing;
        
        if (embossing != null)
        {
            // Show embossing visual
            Color typeColor = embossing.GetTypeColor();
            backgroundImage.color = typeColor * 0.5f;
            
            // Create or update icon
            if (iconImage == null)
            {
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(transform, false);
                
                RectTransform iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(5, 15);
                iconRect.offsetMax = new Vector2(-5, -5);
                
                iconImage = iconObj.AddComponent<Image>();
                iconImage.preserveAspect = true;
            }
            
            if (embossing.icon != null)
                iconImage.sprite = embossing.icon;
            iconImage.enabled = true;
            
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
                nameLabel.color = embossing.GetRarityColor();
                nameLabel.fontStyle = FontStyles.Bold;
            }
            
            nameLabel.text = embossing.embossingName;
            nameLabel.color = embossing.GetRarityColor();
            nameLabel.enabled = true;
        }
        else
        {
            // Clear embossing visual (show empty cell)
            backgroundImage.color = emptyColor;
            
            if (iconImage != null)
                iconImage.enabled = false;
            
            if (nameLabel != null)
                nameLabel.enabled = false;
        }
    }
    
    public void ClearCell()
    {
        SetEmbossing(null);
    }
    
    public EmbossingEffect GetEmbossing()
    {
        return storedEmbossing;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (storedEmbossing != null)
        {
            Debug.Log($"[EmbossingSlotUI] Clicked embossing: {storedEmbossing.embossingName}");
            
            // Notify parent grid of click
            EmbossingGridUI parentGrid = GetComponentInParent<EmbossingGridUI>();
            if (parentGrid != null && parentGrid.OnEmbossingClicked != null)
            {
                parentGrid.OnEmbossingClicked.Invoke(storedEmbossing);
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (storedEmbossing != null)
        {
            Debug.Log($"[EmbossingSlotUI] Hovering: {storedEmbossing.embossingName}");
            
            // Show tooltip using ItemTooltipManager
            if (ItemTooltipManager.Instance != null)
            {
                Character character = CharacterManager.Instance?.GetCurrentCharacter();
                ItemTooltipManager.Instance.ShowEmbossingTooltip(storedEmbossing, eventData.position, character);
            }
            else
            {
                Debug.LogWarning("[EmbossingSlotUI] ItemTooltipManager.Instance not found in scene!");
            }
        }
        
        // Visual feedback
        if (backgroundImage != null && storedEmbossing == null)
        {
            backgroundImage.color = hoverColor;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide tooltip
        if (storedEmbossing != null)
        {
            if (ItemTooltipManager.Instance != null)
            {
                ItemTooltipManager.Instance.HideTooltip();
            }
        }
        
        // Restore original color
        if (storedEmbossing != null)
        {
            backgroundImage.color = storedEmbossing.GetTypeColor() * 0.5f;
        }
        else
        {
            backgroundImage.color = emptyColor;
        }
    }
}


