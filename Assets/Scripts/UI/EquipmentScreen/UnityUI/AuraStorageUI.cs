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
    [Tooltip("Prefab for individual slots (used if gridPrefab is not set)")]
    [SerializeField] private GameObject slotPrefab; // Prefab for individual aura slots
    
    [Tooltip("Complete grid prefab with all slots already set up (FASTER - recommended)")]
    [SerializeField] private GameObject gridPrefab;
    
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
        
        // Load auras after grid is ready
        LoadAuras();
    }
    
    /// <summary>
    /// Generate the fixed grid of slots (always visible, like inventory)
    /// Uses prefab grid if available (much faster), otherwise generates dynamically
    /// </summary>
    private System.Collections.IEnumerator GenerateGridProgressive()
    {
        if (gridContainer == null)
        {
            Debug.LogError("[AuraStorageUI] Grid container is null!");
            yield break;
        }
        
        // Clear existing slots
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();
        
        // FAST PATH: Use prefab grid if available (instantiate once, much faster)
        if (gridPrefab != null)
        {
            yield return null; // Wait one frame
            
            // Instantiate the prefab as a single object - it already has GridLayoutGroup and all slots laid out
            GameObject gridInstance = Instantiate(gridPrefab, gridContainer);
            gridInstance.name = "AuraGrid";
            
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
            
            // Collect all slots from the prefab (they're already laid out by the prefab's GridLayoutGroup)
            AuraSlotUI[] prefabSlots = gridInstance.GetComponentsInChildren<AuraSlotUI>();
            slots.AddRange(prefabSlots);
            
            // Set up event handlers for all slots
            for (int i = 0; i < slots.Count; i++)
            {
                AuraSlotUI slotUI = slots[i];
                if (slotUI != null)
                {
                    // Subscribe to events
                    slotUI.OnAuraClicked += HandleAuraClicked;
                    slotUI.OnAuraHovered += HandleAuraHovered;
                    slotUI.OnAuraUnhovered += HandleAuraUnhovered;
                }
            }
            
            Debug.Log($"[AuraStorageUI] Loaded {slots.Count} slots from prefab grid ({gridColumns}x{gridRows}) - FAST PATH (1 instantiate)");
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
        int totalSlots = gridColumns * gridRows;
        const int slotsPerFrame = 5;
        int slotsGenerated = 0;
        
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
            
            slotsGenerated++;
            
            // Yield every few slots to prevent blocking
            if (slotsGenerated % slotsPerFrame == 0)
            {
                yield return null;
            }
        }
        
        Debug.Log($"[AuraStorageUI] Generated {slots.Count} slots ({gridColumns}x{gridRows}) progressively - SLOW PATH");
    }
    
    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    void GenerateGrid()
    {
        StartCoroutine(GenerateGridProgressive());
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

