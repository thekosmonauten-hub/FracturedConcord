using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Aura Storage grid - displays owned auras in a scrollable grid
/// Similar to EffigyStorageUI but for Reliance Auras
/// </summary>
public class AuraStorageUI : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Number of columns in the storage grid")]
    [SerializeField] private int gridColumns = 4;
    
    [Tooltip("Number of rows in the storage grid (defines total capacity)")]
    [SerializeField] private int gridRows = 10;
    
    [Tooltip("Size of each cell in pixels")]
    [SerializeField] private float cellSize = 100f;
    
    [Tooltip("Spacing between cells in pixels")]
    [SerializeField] private float cellSpacing = 10f;
    
    [Tooltip("Padding around the grid edges")]
    [SerializeField] private float gridPadding = 10f;
    
    [Header("References")]
    [SerializeField] private GameObject slotPrefab; // Prefab for individual aura slots
    [SerializeField] private Transform gridContainer; // Container where slots are created
    
    [Header("Visual Settings")]
    [SerializeField] private Color emptyCellColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    
    private List<AuraSlotUI> slots = new List<AuraSlotUI>();
    private List<RelianceAura> allAuras = new List<RelianceAura>();
    private AuraManagerUI auraManager;
    
    public event System.Action<RelianceAura> OnAuraClicked;
    public event System.Action<RelianceAura> OnAuraHovered;
    public event System.Action OnAuraUnhovered;
    
    void Start()
    {
        GenerateGrid();
        LoadAuras();
    }
    
    /// <summary>
    /// Generate the fixed grid of slots (always visible, like inventory)
    /// </summary>
    void GenerateGrid()
    {
        if (gridContainer == null)
        {
            Debug.LogError("[AuraStorageUI] Grid container is null!");
            return;
        }
        
        // Clear existing slots
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();
        
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
        
        // Generate all slots (total capacity)
        int totalSlots = gridColumns * gridRows;
        
        for (int i = 0; i < totalSlots; i++)
        {
            int col = i % gridColumns;
            int row = i / gridColumns;
            
            GameObject slotObj;
            
            if (slotPrefab != null)
            {
                // Use provided prefab
                slotObj = Instantiate(slotPrefab, gridContainer);
            }
            else
            {
                // Create slot dynamically
                slotObj = new GameObject($"AuraSlot_{col}_{row}");
                slotObj.transform.SetParent(gridContainer, false);
                
                // Add Image component
                Image slotImage = slotObj.AddComponent<Image>();
                slotImage.color = emptyCellColor;
            }
            
            slotObj.name = $"AuraSlot_{col}_{row}";
            
            // Add or get the slot component
            AuraSlotUI slotUI = slotObj.GetComponent<AuraSlotUI>();
            if (slotUI == null)
                slotUI = slotObj.AddComponent<AuraSlotUI>();
            
            // Subscribe to events
            slotUI.OnAuraClicked += HandleAuraClicked;
            slotUI.OnAuraHovered += HandleAuraHovered;
            slotUI.OnAuraUnhovered += HandleAuraUnhovered;
            
            slots.Add(slotUI);
        }
        
        Debug.Log($"[AuraStorageUI] Generated {slots.Count} slots ({gridColumns}x{gridRows})");
    }
    
    /// <summary>
    /// Load all auras from the database
    /// </summary>
    public void LoadAuras()
    {
        allAuras.Clear();
        
        // Load from RelianceAuraDatabase
        if (RelianceAuraDatabase.Instance != null)
        {
            allAuras = RelianceAuraDatabase.Instance.GetAllAuras();
            Debug.Log($"[AuraStorageUI] Loaded {allAuras.Count} auras from database");
        }
        else
        {
            Debug.LogWarning("[AuraStorageUI] RelianceAuraDatabase.Instance is null!");
        }
        
        PopulateGrid();
    }
    
    /// <summary>
    /// Place auras into the grid slots
    /// </summary>
    void PopulateGrid()
    {
        // Clear all slots first
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
        
        // Place auras in slots
        for (int i = 0; i < allAuras.Count && i < slots.Count; i++)
        {
            slots[i].Initialize(allAuras[i], this);
        }
        
        Debug.Log($"[AuraStorageUI] Populated {allAuras.Count} auras into grid");
    }
    
    /// <summary>
    /// Refresh all slots (update active/owned states)
    /// </summary>
    public void Refresh()
    {
        foreach (var slot in slots)
        {
            slot.Refresh();
        }
    }
    
    /// <summary>
    /// Set the aura manager reference
    /// </summary>
    public void SetAuraManager(AuraManagerUI manager)
    {
        auraManager = manager;
    }
    
    private void HandleAuraClicked(RelianceAura aura)
    {
        OnAuraClicked?.Invoke(aura);
        
        if (auraManager != null)
        {
            auraManager.ToggleAura(aura);
        }
    }
    
    private void HandleAuraHovered(RelianceAura aura)
    {
        OnAuraHovered?.Invoke(aura);
    }
    
    private void HandleAuraUnhovered()
    {
        OnAuraUnhovered?.Invoke();
    }
}

