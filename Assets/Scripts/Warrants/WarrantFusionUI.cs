using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for the Peacekeeper 3→1 warrant fusion interface.
/// Uses Unity's legacy UI system (uGUI) to match the existing Warrant UI.
/// </summary>
public class WarrantFusionUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject fusionPanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    
    [Header("Input Slots (3 warrants to fuse)")]
    [SerializeField] private WarrantFusionSlot slot1;
    [SerializeField] private WarrantFusionSlot slot2;
    [SerializeField] private WarrantFusionSlot slot3;
    
    [Header("Output Slot (fused result)")]
    [SerializeField] private WarrantFusionSlot outputSlot;
    
    [Header("Modifier Locking UI")]
    [SerializeField] private Transform modifierLockContainer;
    [SerializeField] private GameObject modifierLockItemPrefab;
    [SerializeField] private ScrollRect modifierScrollRect;
    
    [Header("Notable & Affix Selection UI")]
    [Tooltip("The entire Notable section GameObject (parent of notableContainer). If null, will use notableContainer's parent.")]
    [SerializeField] private GameObject notableSection;
    [Tooltip("The entire Affix section GameObject (parent of affixContainer). If null, will use affixContainer's parent.")]
    [SerializeField] private GameObject affixSection;
    [SerializeField] private Transform notableContainer;
    [SerializeField] private Transform affixContainer;
    [Tooltip("Prefab for displaying notable selection items")]
    [SerializeField] private GameObject notableItemPrefab;
    [Tooltip("Prefab for displaying affix selection items (can reuse modifierLockItemPrefab)")]
    [SerializeField] private GameObject affixItemPrefab;
    
    [Header("Action Buttons")]
    [SerializeField] private Button fuseButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button closeButton;
    [Tooltip("Button to collect the fused warrant from the output slot. Should be a child of OutputSlot GameObject.")]
    [SerializeField] private Button collectButton;
    
    [Header("Status/Info Text")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI infoText;
    
    [Header("References")]
    [SerializeField] private WarrantDatabase warrantDatabase;
    [SerializeField] private WarrantLockerGrid lockerGrid;
    [Tooltip("If lockerGrid is not assigned, will try to find it in the scene. Fusion will still work for adding results.")]
    [SerializeField] private bool autoFindLockerGrid = true;
    [SerializeField] private WarrantLockerPanelManager lockerPanelManager;
    [Tooltip("If lockerPanelManager is not assigned, will try to find it in the scene.")]
    [SerializeField] private bool autoFindLockerPanel = true;
    [SerializeField] private WarrantIconLibrary iconLibrary;
    [Tooltip("If iconLibrary is not assigned, will try to find it from WarrantLockerGrid or WarrantDatabase.")]
    [SerializeField] private bool autoFindIconLibrary = true;
    
    // Public property for access from slots
    public WarrantLockerGrid lockerGridRef => GetLockerGrid();
    
    /// <summary>
    /// Get the output slot for tooltip subtitle
    /// </summary>
    public WarrantFusionSlot GetOutputSlot()
    {
        return outputSlot;
    }
    
    /// <summary>
    /// Get the NotableDatabase for tooltips
    /// </summary>
    public WarrantNotableDatabase GetNotableDatabase()
    {
        // Try to get from locker grid
        var grid = GetLockerGrid();
        if (grid != null)
        {
            return grid.GetNotableDatabase();
        }
        
        // Try to get from warrant database
        if (warrantDatabase != null)
        {
            // WarrantDatabase might have a reference - check via reflection if needed
            var dbType = typeof(WarrantDatabase);
            var notableDbField = dbType.GetField("notableDatabase", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (notableDbField != null)
            {
                return notableDbField.GetValue(warrantDatabase) as WarrantNotableDatabase;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the icon library for assigning icons to warrants
    /// </summary>
    public WarrantIconLibrary GetIconLibrary()
    {
        if (iconLibrary == null && autoFindIconLibrary)
        {
            // Try to get from warrant database
            if (warrantDatabase != null)
            {
                // WarrantDatabase has iconLibrary as a private field, try reflection
                var dbType = typeof(WarrantDatabase);
                var iconLibraryField = dbType.GetField("iconLibrary", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (iconLibraryField != null)
                {
                    iconLibrary = iconLibraryField.GetValue(warrantDatabase) as WarrantIconLibrary;
                }
            }
            
            // Try to get from locker grid (it also has iconLibrary)
            if (iconLibrary == null)
            {
                var grid = GetLockerGrid();
                if (grid != null)
                {
                    var gridType = typeof(WarrantLockerGrid);
                    var iconLibraryField = gridType.GetField("iconLibrary", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (iconLibraryField != null)
                    {
                        iconLibrary = iconLibraryField.GetValue(grid) as WarrantIconLibrary;
                    }
                }
            }
            
            // Last resort: find in scene or Resources
            if (iconLibrary == null)
            {
                iconLibrary = FindFirstObjectByType<WarrantIconLibrary>();
                if (iconLibrary == null)
                {
                    // Try to load from Resources
                    iconLibrary = Resources.Load<WarrantIconLibrary>("WarrantIconLibrary");
                }
            }
            
            if (iconLibrary != null)
            {
                Debug.Log($"[WarrantFusionUI] Auto-found WarrantIconLibrary: {iconLibrary.name} (pool size: {iconLibrary.GetIconPoolCount()})");
            }
        }
        
        return iconLibrary;
    }
    
    private Dictionary<int, List<string>> lockedModifiers = new Dictionary<int, List<string>>();
    private List<WarrantModifierLockItem> lockItems = new List<WarrantModifierLockItem>();
    private WarrantFusionSlot currentlySelectingSlot = null;
    
    // Selected notable and affix for fusion
    private WarrantNotableDefinition selectedNotable = null;
    private WarrantModifier selectedAffix = null;
    private int selectedNotableSlotIndex = -1;
    private int selectedAffixSlotIndex = -1;
    private WarrantModifierLockItem selectedAffixItem = null; // Track the selected affix UI item
    
    // Blueprint for the fused warrant (shown in output slot)
    private WarrantDefinition fusionBlueprint = null;
    
    // Store the fused warrant until user collects it
    private WarrantDefinition currentFusedWarrant = null;
    
    // Selection state
    private bool notableConfirmed = false;
    private bool affixConfirmed = false;
    private WarrantNotableLockItem pendingNotableItem = null; // Currently toggled but not confirmed
    private WarrantModifierLockItem pendingAffixItem = null; // Currently toggled but not confirmed
    
    private void Awake()
    {
        InitializeUI();
    }
    
    private void Start()
    {
        if (fusionPanel != null)
        {
            fusionPanel.SetActive(false);
        }
        
        // Auto-find locker grid if not assigned
        if (lockerGrid == null && autoFindLockerGrid)
        {
            lockerGrid = FindFirstObjectByType<WarrantLockerGrid>();
            if (lockerGrid == null)
            {
                Debug.LogWarning("[WarrantFusionUI] No WarrantLockerGrid found in scene. Fusion will work, but warrant selection won't be available.");
            }
        }
        
        // Auto-find locker panel manager if not assigned
        if (lockerPanelManager == null && autoFindLockerPanel)
        {
            lockerPanelManager = FindFirstObjectByType<WarrantLockerPanelManager>();
        }
        
        // Initialize icon library (will auto-find if not assigned)
        if (iconLibrary == null && autoFindIconLibrary)
        {
            iconLibrary = GetIconLibrary(); // This will find it automatically
        }
        
        // Set up warrant selection callback
        SetupWarrantSelectionCallback();
    }
    
    private void SetupWarrantSelectionCallback()
    {
        // We'll set up the callback when showing the locker
        // This will be handled in ShowLockerForSelection
    }
    
    private WarrantLockerGrid GetLockerGrid()
    {
        if (lockerGrid == null && autoFindLockerGrid)
        {
            lockerGrid = FindFirstObjectByType<WarrantLockerGrid>();
        }
        return lockerGrid;
    }
    
    private void InitializeUI()
    {
        if (fuseButton != null)
        {
            fuseButton.onClick.AddListener(OnFuseButtonClicked);
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(OnClearButtonClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        
        // Auto-find CollectButton if not assigned (should be child of OutputSlot)
        if (collectButton == null && outputSlot != null)
        {
            collectButton = outputSlot.transform.Find("CollectButton")?.GetComponent<Button>();
            if (collectButton == null)
            {
                // Try searching in children
                collectButton = outputSlot.GetComponentInChildren<Button>(true);
                if (collectButton != null && !collectButton.name.ToLower().Contains("collect"))
                {
                    collectButton = null; // Only use if name contains "collect"
                }
            }
        }
        
        if (collectButton != null)
        {
            collectButton.onClick.AddListener(OnCollectButtonClicked);
            collectButton.gameObject.SetActive(false); // Hidden by default
        }
        
        // Initialize slots
        if (slot1 != null)
        {
            slot1.Initialize(0, this);
            slot1.OnWarrantChanged += OnSlotWarrantChanged;
        }
        
        if (slot2 != null)
        {
            slot2.Initialize(1, this);
            slot2.OnWarrantChanged += OnSlotWarrantChanged;
        }
        
        if (slot3 != null)
        {
            slot3.Initialize(2, this);
            slot3.OnWarrantChanged += OnSlotWarrantChanged;
        }
        
        if (outputSlot != null)
        {
            outputSlot.Initialize(-1, this); // Output slot has index -1
            // Output slot should be interactable for tooltips, but not for drops/clicks
            // We'll handle this in SetInteractable by checking slotIndex == -1
            outputSlot.SetInteractable(true); // Allow tooltips
        }
        
        UpdateFuseButtonState();
    }
    
    private void OnDestroy()
    {
        if (fuseButton != null)
        {
            fuseButton.onClick.RemoveListener(OnFuseButtonClicked);
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.RemoveListener(OnClearButtonClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
        
        if (slot1 != null) slot1.OnWarrantChanged -= OnSlotWarrantChanged;
        if (slot2 != null) slot2.OnWarrantChanged -= OnSlotWarrantChanged;
        if (slot3 != null) slot3.OnWarrantChanged -= OnSlotWarrantChanged;
    }
    
    public void ShowPanel()
    {
        if (fusionPanel != null)
        {
            fusionPanel.SetActive(true);
        }
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.interactable = true;
        }
        
        // Ensure locker panel is hidden when fusion panel opens
        // It will only show when a slot is clicked
        HideLockerPanel();
        currentlySelectingSlot = null;
        DisableSelectionModeOnLockerItems();
        
        RefreshModifierLockUI();
    }
    
    public void HidePanel()
    {
        if (fusionPanel != null)
        {
            fusionPanel.SetActive(false);
        }
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.interactable = false;
        }
        
        // Hide locker panel when fusion panel closes
        HideLockerPanel();
        
        // Clear any selection state
        currentlySelectingSlot = null;
        DisableSelectionModeOnLockerItems();
    }
    
    public void TogglePanel()
    {
        if (fusionPanel != null && fusionPanel.activeSelf)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }
    
    /// <summary>
    /// Called when a fusion slot is clicked. Shows the locker grid for warrant selection.
    /// </summary>
    public void OnSlotClicked(WarrantFusionSlot slot)
    {
        if (slot == null || slot == outputSlot)
            return;
        
        currentlySelectingSlot = slot;
        ShowLockerForSelection();
    }
    
    /// <summary>
    /// Shows the locker grid and sets up selection mode.
    /// </summary>
    private void ShowLockerForSelection()
    {
        var grid = GetLockerGrid();
        if (grid == null)
        {
            SetStatusText("No warrant locker available. Please set up WarrantLockerGrid in the scene.", Color.red);
            return;
        }
        
        // Show the locker panel
        if (lockerPanelManager != null)
        {
            lockerPanelManager.ShowPanel();
        }
        else
        {
            // Try to find and show the locker panel directly
            var lockerPanel = grid.gameObject;
            if (lockerPanel != null)
            {
                lockerPanel.SetActive(true);
            }
        }
        
        // Enable selection mode on all locker items
        EnableSelectionModeOnLockerItems();
        
        SetStatusText("Select a warrant from the locker to place in the slot.", Color.yellow);
    }
    
    /// <summary>
    /// Enables click-to-select mode on all warrant locker items.
    /// </summary>
    private void EnableSelectionModeOnLockerItems()
    {
        var grid = GetLockerGrid();
        if (grid == null)
            return;
        
        // Find all WarrantLockerItem components in the grid
        WarrantLockerItem[] items = grid.GetComponentsInChildren<WarrantLockerItem>();
        foreach (var item in items)
        {
            if (item != null)
            {
                // Set up click handler for selection
                item.SetSelectionCallback(OnWarrantSelectedFromLocker);
            }
        }
    }
    
    /// <summary>
    /// Called when a warrant is selected from the locker grid.
    /// </summary>
    private void OnWarrantSelectedFromLocker(WarrantDefinition warrant, WarrantLockerItem lockerItem)
    {
        if (currentlySelectingSlot == null)
            return;
        
        if (warrant == null || lockerItem == null)
            return;
        
        // Assign the warrant to the slot
        currentlySelectingSlot.AssignWarrantFromSelection(warrant, lockerItem);
        
        // Hide the locker panel
        HideLockerPanel();
        
        // Clear selection state
        currentlySelectingSlot = null;
        
        // Disable selection mode on locker items
        DisableSelectionModeOnLockerItems();
        
        // Refresh UI
        RefreshModifierLockUI();
        UpdateFuseButtonState();
        UpdateStatusText();
    }
    
    /// <summary>
    /// Hides the locker panel.
    /// </summary>
    public void HideLockerPanel()
    {
        if (lockerPanelManager != null)
        {
            lockerPanelManager.HidePanel();
        }
        else
        {
            var grid = GetLockerGrid();
            if (grid != null && grid.gameObject != null)
            {
                grid.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Disables selection mode on all warrant locker items.
    /// </summary>
    private void DisableSelectionModeOnLockerItems()
    {
        var grid = GetLockerGrid();
        if (grid == null)
            return;
        
        WarrantLockerItem[] items = grid.GetComponentsInChildren<WarrantLockerItem>();
        foreach (var item in items)
        {
            if (item != null)
            {
                item.ClearSelectionCallback();
            }
        }
    }
    
    private void OnSlotWarrantChanged(int slotIndex, WarrantDefinition warrant)
    {
        // Check if all 3 input slots are filled
        bool allSlotsFilled = slot1?.CurrentWarrant != null && slot2?.CurrentWarrant != null && slot3?.CurrentWarrant != null;
        
        if (allSlotsFilled)
        {
            // Create blank blueprint in output slot (if not already created)
            if (fusionBlueprint == null)
            {
                CreateFusionBlueprint();
            }
        }
        else
        {
            // Clear blueprint if slots are not all filled
            ClearFusionBlueprint();
        }
        
        // Always refresh UI when slots change (this will resolve notables if needed)
        RefreshModifierLockUI();
        UpdateFuseButtonState();
        UpdateStatusText();
    }
    
    /// <summary>
    /// Creates a blank warrant blueprint and displays it in the output slot
    /// </summary>
    private void CreateFusionBlueprint()
    {
        if (warrantDatabase == null)
        {
            Debug.LogWarning("[WarrantFusionUI] Cannot create blueprint: WarrantDatabase not assigned");
            return;
        }
        
        // Create a blank warrant instance
        fusionBlueprint = ScriptableObject.CreateInstance<WarrantDefinition>();
        fusionBlueprint.warrantId = $"fusion_blueprint_{System.DateTime.Now.Ticks}";
        fusionBlueprint.displayName = "Fused Warrant (Preview)";
        fusionBlueprint.isBlueprint = false; // This is a preview instance, not a template
        fusionBlueprint.rarity = WarrantRarity.Common; // Will be upgraded after fusion
        
        // Copy base properties from first warrant
        if (slot1?.CurrentWarrant != null)
        {
            fusionBlueprint.rangeDirection = slot1.CurrentWarrant.rangeDirection;
            fusionBlueprint.rangeDepth = slot1.CurrentWarrant.rangeDepth;
            fusionBlueprint.affectDiagonals = slot1.CurrentWarrant.affectDiagonals;
        }
        
        // Initialize empty lists
        fusionBlueprint.modifiers = new List<WarrantModifier>();
        fusionBlueprint.notable = null;
        fusionBlueprint.notableId = null;
        fusionBlueprint.icon = null; // No icon initially - blank blueprint
        
        // Reset selection state
        notableConfirmed = false;
        affixConfirmed = false;
        selectedNotable = null;
        selectedAffix = null;
        selectedNotableSlotIndex = -1;
        selectedAffixSlotIndex = -1;
        
        // Display in output slot
        if (outputSlot != null)
        {
            outputSlot.SetWarrant(fusionBlueprint);
        }
        
        Debug.Log("[WarrantFusionUI] Created blank fusion blueprint");
    }
    
    /// <summary>
    /// Clears the fusion blueprint
    /// </summary>
    private void ClearFusionBlueprint()
    {
        if (fusionBlueprint != null)
        {
            Destroy(fusionBlueprint);
            fusionBlueprint = null;
        }
        
        // Don't clear output slot if there's a fused warrant waiting to be collected
        if (outputSlot != null && currentFusedWarrant == null)
        {
            outputSlot.ClearSlot();
        }
        
        notableConfirmed = false;
        affixConfirmed = false;
        selectedNotable = null;
        selectedAffix = null;
        
        Debug.Log("[WarrantFusionUI] Cleared fusion blueprint");
    }
    
    private void RefreshModifierLockUI()
    {
        // Clear existing lock items
        foreach (var item in lockItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        lockItems.Clear();
        lockedModifiers.Clear();
        
        // Reset selections
        selectedNotable = null;
        selectedAffix = null;
        selectedNotableSlotIndex = -1;
        selectedAffixSlotIndex = -1;
        selectedAffixItem = null;
        
        // Clear notable and affix containers
        if (notableContainer != null)
        {
            foreach (Transform child in notableContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        if (affixContainer != null)
        {
            foreach (Transform child in affixContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        // Check if we have all three warrants
        if (slot1?.CurrentWarrant == null || slot2?.CurrentWarrant == null || slot3?.CurrentWarrant == null)
        {
            return;
        }
        
        // Collect all notables and affixes from all three slots
        List<NotableSource> allNotables = new List<NotableSource>();
        List<ModifierSource> allAffixes = new List<ModifierSource>();
        
        CollectNotablesFromSlot(slot1, 0, allNotables);
        CollectNotablesFromSlot(slot2, 1, allNotables);
        CollectNotablesFromSlot(slot3, 2, allNotables);
        
        CollectModifiersFromSlot(slot1, 0, allAffixes);
        CollectModifiersFromSlot(slot2, 1, allAffixes);
        CollectModifiersFromSlot(slot3, 2, allAffixes);
        
        Debug.Log($"[WarrantFusionUI] Collected {allNotables.Count} notables and {allAffixes.Count} affixes from 3 warrants");
        
        // Get the section GameObjects (use explicit references or find from containers)
        GameObject notableSectionObj = notableSection;
        if (notableSectionObj == null && notableContainer != null)
        {
            // Try to find a parent GameObject that represents the section
            // Look for a parent with "Notable" or "Section" in the name, or use the container's parent
            Transform parent = notableContainer.parent;
            while (parent != null)
            {
                if (parent.name.ToLower().Contains("notable") || parent.name.ToLower().Contains("section"))
                {
                    notableSectionObj = parent.gameObject;
                    break;
                }
                parent = parent.parent;
            }
            // Fallback to direct parent if no named section found
            if (notableSectionObj == null && notableContainer.parent != null)
            {
                notableSectionObj = notableContainer.parent.gameObject;
            }
        }
        
        GameObject affixSectionObj = affixSection;
        if (affixSectionObj == null && affixContainer != null)
        {
            // Try to find a parent GameObject that represents the section
            Transform parent = affixContainer.parent;
            while (parent != null)
            {
                if (parent.name.ToLower().Contains("affix") || parent.name.ToLower().Contains("section"))
                {
                    affixSectionObj = parent.gameObject;
                    break;
                }
                parent = parent.parent;
            }
            // Fallback to direct parent if no named section found
            if (affixSectionObj == null && affixContainer.parent != null)
            {
                affixSectionObj = affixContainer.parent.gameObject;
            }
        }
        
        // Show Notable section first (only if notable is not yet confirmed)
        if (notableContainer != null && notableItemPrefab != null && !notableConfirmed)
        {
            // Activate the entire Notable section
            if (notableSectionObj != null)
            {
                bool wasActive = notableSectionObj.activeSelf;
                notableSectionObj.SetActive(true);
                Debug.Log($"[WarrantFusionUI] Notable section activated (was {wasActive}, now active: {notableSectionObj.activeSelf})");
            }
            
            // Ensure container is also active
            bool containerWasActive = notableContainer.gameObject.activeSelf;
            notableContainer.gameObject.SetActive(true);
            Debug.Log($"[WarrantFusionUI] Notable container activated (was {containerWasActive}, now active: {notableContainer.gameObject.activeSelf}) with {allNotables.Count} notables");
            
            if (allNotables.Count == 0)
            {
                Debug.LogWarning("[WarrantFusionUI] No notables found in any of the 3 input warrants! Check that warrants have notables assigned.");
                Debug.LogWarning($"[WarrantFusionUI] Slot1 warrant: {slot1?.CurrentWarrant?.warrantId ?? "null"}, notable: {slot1?.CurrentWarrant?.notable?.displayName ?? "null"}, notableId: {slot1?.CurrentWarrant?.notableId ?? "null"}");
                Debug.LogWarning($"[WarrantFusionUI] Slot2 warrant: {slot2?.CurrentWarrant?.warrantId ?? "null"}, notable: {slot2?.CurrentWarrant?.notable?.displayName ?? "null"}, notableId: {slot2?.CurrentWarrant?.notableId ?? "null"}");
                Debug.LogWarning($"[WarrantFusionUI] Slot3 warrant: {slot3?.CurrentWarrant?.warrantId ?? "null"}, notable: {slot3?.CurrentWarrant?.notable?.displayName ?? "null"}, notableId: {slot3?.CurrentWarrant?.notableId ?? "null"}");
            }
            else
            {
                Debug.Log($"[WarrantFusionUI] Creating {allNotables.Count} notable items in Notable section");
            }
            
            foreach (var notableSource in allNotables)
            {
                GameObject notableItemObj = Instantiate(notableItemPrefab, notableContainer);
                WarrantNotableLockItem notableItem = notableItemObj.GetComponent<WarrantNotableLockItem>();
                
                if (notableItem == null)
                {
                    notableItem = notableItemObj.AddComponent<WarrantNotableLockItem>();
                }
                
                if (notableItem != null)
                {
                    notableItem.Initialize(notableSource.slotIndex, notableSource.notable, this);
                    Debug.Log($"[WarrantFusionUI] Created notable item for '{notableSource.notable.displayName}' from slot {notableSource.slotIndex + 1}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[WarrantFusionUI] Cannot show Notable section: notableContainer={notableContainer != null}, notableItemPrefab={notableItemPrefab != null}");
        }
        
        // Show Affix section only after notable is confirmed
        if (affixContainer != null)
        {
            // Activate/deactivate the entire Affix section
            if (affixSectionObj != null)
            {
                bool wasActive = affixSectionObj.activeSelf;
                affixSectionObj.SetActive(notableConfirmed);
                Debug.Log($"[WarrantFusionUI] Affix section {(notableConfirmed ? "activated" : "deactivated")} (was {wasActive}, now active: {affixSectionObj.activeSelf})");
            }
            
            // Ensure container matches section state
            affixContainer.gameObject.SetActive(notableConfirmed);
            
            if (notableConfirmed)
            {
                GameObject prefabToUse = affixItemPrefab != null ? affixItemPrefab : modifierLockItemPrefab;
                if (prefabToUse != null)
                {
                    foreach (var modSource in allAffixes)
                    {
                        GameObject affixItemObj = Instantiate(prefabToUse, affixContainer);
                        WarrantModifierLockItem affixItem = affixItemObj.GetComponent<WarrantModifierLockItem>();
                        
                        if (affixItem != null)
                        {
                            affixItem.Initialize(modSource.slotIndex, modSource.modifier, this);
                            lockItems.Add(affixItem);
                        }
                    }
                }
            }
        }
    }
    
    private void CollectNotablesFromSlot(WarrantFusionSlot slot, int slotIndex, List<NotableSource> output)
    {
        if (slot == null || slot.CurrentWarrant == null)
        {
            Debug.LogWarning($"[WarrantFusionUI] CollectNotablesFromSlot: Slot {slotIndex} is null or has no warrant");
            return;
        }
        
        var warrant = slot.CurrentWarrant;
        Debug.Log($"[WarrantFusionUI] Checking warrant '{warrant.warrantId}' in slot {slotIndex} for notable. Has notable: {warrant.notable != null}, notableId: {warrant.notableId}");
        
        if (warrant.notable != null)
        {
            output.Add(new NotableSource
            {
                slotIndex = slotIndex,
                notable = warrant.notable
            });
            Debug.Log($"[WarrantFusionUI] Added notable '{warrant.notable.displayName}' from slot {slotIndex}");
        }
        else if (!string.IsNullOrEmpty(warrant.notableId))
        {
            // Try to resolve notable by ID if we have a NotableDatabase
            var notableDb = GetNotableDatabase();
            if (notableDb != null)
            {
                // Get NotableEntry from database
                var notableEntry = notableDb.GetById(warrant.notableId);
                if (notableEntry != null)
                {
                    // Convert NotableEntry to WarrantNotableDefinition (create runtime instance)
                    WarrantNotableDefinition notableDef = CreateNotableDefinitionFromEntry(notableEntry);
                    
                    if (notableDef != null)
                    {
                        // Found the notable! Add it to the list
                        output.Add(new NotableSource
                        {
                            slotIndex = slotIndex,
                            notable = notableDef
                        });
                        
                        // Also assign it back to the warrant so it's available for future use
                        warrant.notable = notableDef;
                        
                        Debug.Log($"[WarrantFusionUI] Resolved notable '{notableDef.displayName}' (ID: {warrant.notableId}) from NotableDatabase and assigned to warrant");
                    }
                }
                else
                {
                    Debug.LogWarning($"[WarrantFusionUI] Warrant has notableId '{warrant.notableId}' but NotableDatabase.GetById() returned null. Notable may not exist in database.");
                }
            }
            else
            {
                Debug.LogWarning($"[WarrantFusionUI] Warrant '{warrant.warrantId}' has notableId '{warrant.notableId}' but notable is null AND NotableDatabase is not available!");
            }
        }
    }
    
    /// <summary>
    /// Creates a runtime WarrantNotableDefinition from a NotableEntry.
    /// This is needed because warrants expect WarrantNotableDefinition ScriptableObjects,
    /// but the database stores NotableEntry objects.
    /// </summary>
    private WarrantNotableDefinition CreateNotableDefinitionFromEntry(WarrantNotableDatabase.NotableEntry entry)
    {
        if (entry == null)
            return null;
        
        // Create a runtime ScriptableObject instance
        WarrantNotableDefinition notableDef = ScriptableObject.CreateInstance<WarrantNotableDefinition>();
        
        // Copy data from NotableEntry to WarrantNotableDefinition
        notableDef.notableId = entry.notableId;
        notableDef.displayName = entry.displayName;
        notableDef.description = entry.description;
        notableDef.tags = entry.tags != null ? new List<string>(entry.tags) : new List<string>();
        notableDef.weight = entry.weight;
        
        // Convert NotableModifiers to WarrantModifiers
        notableDef.modifiers = new List<WarrantModifier>();
        if (entry.modifiers != null)
        {
            foreach (var notableMod in entry.modifiers)
            {
                if (notableMod == null || string.IsNullOrEmpty(notableMod.statKey))
                    continue;
                
                var warrantMod = new WarrantModifier
                {
                    modifierId = notableMod.statKey, // Use statKey as modifierId
                    displayName = !string.IsNullOrEmpty(notableMod.displayName) ? notableMod.displayName : notableMod.statKey,
                    value = notableMod.value,
                    description = notableMod.displayName
                };
                
                notableDef.modifiers.Add(warrantMod);
            }
        }
        
        return notableDef;
    }
    
    private void CollectModifiersFromSlot(WarrantFusionSlot slot, int slotIndex, List<ModifierSource> output)
    {
        if (slot == null || slot.CurrentWarrant == null)
            return;
        
        var warrant = slot.CurrentWarrant;
        
        // Get the notable for this warrant (check both notable and notableId)
        WarrantNotableDefinition notable = warrant.notable;
        if (notable == null && !string.IsNullOrEmpty(warrant.notableId))
        {
            // Try to resolve from database
            var notableDatabase = GetNotableDatabase();
            if (notableDatabase != null)
            {
                var entry = notableDatabase.GetById(warrant.notableId);
                if (entry != null)
                {
                    // Convert NotableEntry to WarrantNotableDefinition
                    notable = CreateNotableDefinitionFromEntry(entry);
                }
            }
        }
        
        // Collect notable modifiers to exclude
        // We'll match by multiple criteria to be robust
        HashSet<string> notableModifierIds = new HashSet<string>();
        HashSet<string> notableModifierNames = new HashSet<string>();
        HashSet<string> notableModifierStatKeys = new HashSet<string>();
        
        if (notable != null && notable.modifiers != null)
        {
            foreach (var notableMod in notable.modifiers)
            {
                // Match by modifierId (if set)
                if (!string.IsNullOrEmpty(notableMod.modifierId))
                {
                    notableModifierIds.Add(notableMod.modifierId);
                }
                
                // Match by displayName + operation (for UI display matching)
                if (!string.IsNullOrEmpty(notableMod.displayName))
                {
                    string key = $"{notableMod.displayName}_{notableMod.operation}";
                    notableModifierNames.Add(key);
                    // Also add just the displayName (case-insensitive) for loose matching
                    notableModifierNames.Add(notableMod.displayName.ToLowerInvariant());
                }
                
                // Match by statKey (from NotableModifier) - this is often the most reliable
                // When we convert NotableModifier to WarrantModifier, statKey becomes modifierId
                // But we also check if modifierId might be the statKey
                if (!string.IsNullOrEmpty(notableMod.modifierId))
                {
                    notableModifierStatKeys.Add(notableMod.modifierId.ToLowerInvariant());
                }
            }
        }
        
        // Collect only modifiers that are NOT from the notable
        foreach (var modifier in warrant.modifiers)
        {
            if (modifier == null)
                continue;
            
            // Check if this modifier belongs to the notable
            bool isNotableModifier = false;
            
            // Method 1: Check by modifierId (exact match)
            if (!string.IsNullOrEmpty(modifier.modifierId))
            {
                string modIdLower = modifier.modifierId.ToLowerInvariant();
                if (notableModifierIds.Contains(modifier.modifierId) || 
                    notableModifierStatKeys.Contains(modIdLower))
                {
                    isNotableModifier = true;
                }
            }
            
            // Method 2: Check by displayName + operation (for UI matching)
            if (!isNotableModifier && !string.IsNullOrEmpty(modifier.displayName))
            {
                string key = $"{modifier.displayName}_{modifier.operation}";
                string displayNameLower = modifier.displayName.ToLowerInvariant();
                
                if (notableModifierNames.Contains(key) || 
                    notableModifierNames.Contains(displayNameLower))
                {
                    isNotableModifier = true;
                }
            }
            
            // Only add if it's NOT a notable modifier
            if (!isNotableModifier)
            {
                output.Add(new ModifierSource
                {
                    slotIndex = slotIndex,
                    modifier = modifier
                });
            }
            else
            {
                Debug.Log($"[WarrantFusionUI] Excluding notable modifier from affixes: '{modifier.displayName}' (ID: {modifier.modifierId}, slot {slotIndex})");
            }
        }
    }
    
    // Legacy method - now redirects to ToggleAffixSelection
    public void ToggleModifierLock(int slotIndex, string modifierId, bool isLocked, WarrantModifierLockItem item = null)
    {
        ToggleAffixSelection(slotIndex, modifierId, isLocked, item);
    }
    
    /// <summary>
    /// Called when a notable is toggled (pending selection). Shows confirm button.
    /// </summary>
    public void ToggleNotableSelection(int slotIndex, WarrantNotableDefinition notable, bool isToggled, WarrantNotableLockItem item)
    {
        if (isToggled)
        {
            // If another notable is already toggled, deselect it
            if (pendingNotableItem != null && pendingNotableItem != item)
            {
                pendingNotableItem.SetSelected(false);
            }
            
            pendingNotableItem = item;
        }
        else
        {
            if (pendingNotableItem == item)
            {
                pendingNotableItem = null;
            }
        }
        
        UpdateStatusText();
    }
    
    /// <summary>
    /// Called when a notable is confirmed. Applies it to the blueprint and activates affix section.
    /// </summary>
    public void ConfirmNotableSelection(int slotIndex, WarrantNotableDefinition notable)
    {
        if (notable == null || fusionBlueprint == null)
            return;
        
        // Apply notable to blueprint
        fusionBlueprint.notable = notable;
        fusionBlueprint.notableId = !string.IsNullOrEmpty(notable.notableId) ? notable.notableId : notable.name;
        
        // Mark as confirmed
        notableConfirmed = true;
        selectedNotable = notable;
        selectedNotableSlotIndex = slotIndex;
        
        // Hide confirm button on the item
        if (pendingNotableItem != null)
        {
            pendingNotableItem.SetConfirmed(true);
            pendingNotableItem = null;
        }
        
        // Update output slot to show the blueprint with notable
        if (outputSlot != null)
        {
            outputSlot.SetWarrant(fusionBlueprint);
        }
        
        // Hide Notable section BEFORE refreshing UI (so RefreshModifierLockUI doesn't reactivate it)
        GameObject notableSectionObj = GetNotableSectionObject();
        if (notableSectionObj != null)
        {
            notableSectionObj.SetActive(false);
            Debug.Log($"[WarrantFusionUI] Hiding Notable section after confirmation. GameObject: {notableSectionObj.name}, Active: {notableSectionObj.activeSelf}");
        }
        else
        {
            Debug.LogWarning("[WarrantFusionUI] Could not find Notable section GameObject to hide!");
        }
        
        // Activate affix section (this will now skip Notable section activation because notableConfirmed is true)
        RefreshModifierLockUI();
        
        UpdateStatusText();
        Debug.Log($"[WarrantFusionUI] Confirmed notable: {notable.displayName}");
    }
    
    /// <summary>
    /// Helper method to get the Notable section GameObject
    /// </summary>
    private GameObject GetNotableSectionObject()
    {
        if (notableSection != null)
            return notableSection;
        
        if (notableContainer != null)
        {
            // Try to find a parent GameObject that represents the section
            Transform parent = notableContainer.parent;
            while (parent != null)
            {
                if (parent.name.ToLower().Contains("notable") || parent.name.ToLower().Contains("section"))
                {
                    return parent.gameObject;
                }
                parent = parent.parent;
            }
            // Fallback to direct parent if no named section found
            if (notableContainer.parent != null)
            {
                return notableContainer.parent.gameObject;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Helper method to get the Affix section GameObject
    /// </summary>
    private GameObject GetAffixSectionObject()
    {
        if (affixSection != null)
            return affixSection;
        
        if (affixContainer != null)
        {
            // Try to find a parent GameObject that represents the section
            Transform parent = affixContainer.parent;
            while (parent != null)
            {
                if (parent.name.ToLower().Contains("affix") || parent.name.ToLower().Contains("section"))
                {
                    return parent.gameObject;
                }
                parent = parent.parent;
            }
            // Fallback to direct parent if no named section found
            if (affixContainer.parent != null)
            {
                return affixContainer.parent.gameObject;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Called when an affix is toggled (pending selection). Shows confirm button.
    /// </summary>
    public void ToggleAffixSelection(int slotIndex, string modifierId, bool isToggled, WarrantModifierLockItem item)
    {
        if (!notableConfirmed)
        {
            // Can't select affix until notable is confirmed
            if (item != null && item.GetComponent<Toggle>() != null)
            {
                item.GetComponent<Toggle>().isOn = false;
            }
            return;
        }
        
        if (isToggled)
        {
            // If another affix is already toggled, deselect it
            if (pendingAffixItem != null && pendingAffixItem != item)
            {
                pendingAffixItem.GetComponent<Toggle>().isOn = false;
            }
            
            pendingAffixItem = item;
        }
        else
        {
            if (pendingAffixItem == item)
            {
                pendingAffixItem = null;
            }
        }
        
        UpdateStatusText();
    }
    
    /// <summary>
    /// Called when an affix is confirmed. Applies it to the blueprint.
    /// </summary>
    public void ConfirmAffixSelection(int slotIndex, WarrantModifier modifier)
    {
        if (modifier == null || fusionBlueprint == null || !notableConfirmed)
            return;
        
        // Apply affix to blueprint
        if (fusionBlueprint.modifiers == null)
        {
            fusionBlueprint.modifiers = new List<WarrantModifier>();
        }
        
        // Remove any existing affix (only one allowed)
        fusionBlueprint.modifiers.Clear();
        
        // Add the confirmed affix
        fusionBlueprint.modifiers.Add(new WarrantModifier
        {
            modifierId = modifier.modifierId,
            displayName = modifier.displayName,
            operation = modifier.operation,
            value = modifier.value,
            description = modifier.description
        });
        
        // Mark as confirmed
        affixConfirmed = true;
        selectedAffix = modifier;
        selectedAffixSlotIndex = slotIndex;
        
        // Hide confirm button on the item
        if (pendingAffixItem != null)
        {
            pendingAffixItem.SetConfirmed(true);
            pendingAffixItem = null;
        }
        
        // Update output slot to show the blueprint with notable and affix
        if (outputSlot != null)
        {
            outputSlot.SetWarrant(fusionBlueprint);
        }
        
        UpdateStatusText();
        UpdateFuseButtonState();
        Debug.Log($"[WarrantFusionUI] Confirmed affix: {modifier.displayName ?? modifier.modifierId}");
    }
    
    private void OnFuseButtonClicked()
    {
        if (!CanFuse())
        {
            if (!notableConfirmed || !affixConfirmed)
            {
                SetStatusText("Cannot fuse: Please confirm both Notable and Affix selections.", Color.red);
            }
            else
            {
                SetStatusText("Cannot fuse: All three slots must have warrants.", Color.red);
            }
            return;
        }
        
        WarrantDefinition warrant1 = slot1.CurrentWarrant;
        WarrantDefinition warrant2 = slot2.CurrentWarrant;
        WarrantDefinition warrant3 = slot3.CurrentWarrant;
        
        if (warrantDatabase == null)
        {
            SetStatusText("Error: WarrantDatabase not assigned.", Color.red);
            return;
        }
        
        // Build locked modifiers dictionary with selected affix
        Dictionary<int, List<string>> fusionLockedModifiers = new Dictionary<int, List<string>>();
        
        // Add selected affix to locked modifiers
        if (selectedAffix != null && selectedAffixSlotIndex >= 0)
        {
            if (!fusionLockedModifiers.ContainsKey(selectedAffixSlotIndex))
            {
                fusionLockedModifiers[selectedAffixSlotIndex] = new List<string>();
            }
            fusionLockedModifiers[selectedAffixSlotIndex].Add(selectedAffix.modifierId);
        }
        
        // Perform fusion
        var result = WarrantFusionLogic.FuseWarrants(
            warrant1,
            warrant2,
            warrant3,
            fusionLockedModifiers,
            warrantDatabase,
            selectedNotable  // Pass selected notable
        );
        
        if (result.success && result.fusedWarrant != null)
        {
            // Ensure fused warrant has an icon
            if (result.fusedWarrant.icon == null)
            {
                var iconLib = GetIconLibrary();
                if (iconLib != null)
                {
                    // Try to get icon by ID first, then random
                    Sprite icon = iconLib.GetIcon(result.fusedWarrant.warrantId);
                    if (icon == null)
                    {
                        icon = iconLib.GetRandomIcon();
                    }
                    if (icon != null)
                    {
                        result.fusedWarrant.icon = icon;
                        Debug.Log($"[WarrantFusionUI] Assigned icon to fused warrant '{result.fusedWarrant.warrantId}'");
                    }
                }
            }
            
            // Store the fused warrant (don't add to locker yet)
            currentFusedWarrant = result.fusedWarrant;
            
            // Display result in output slot
            if (outputSlot != null)
            {
                outputSlot.SetWarrant(result.fusedWarrant);
            }
            
            // Show CollectButton
            if (collectButton != null)
            {
                collectButton.gameObject.SetActive(true);
            }
            
            // Remove consumed warrants from locker (they're already used in fusion)
            var grid = GetLockerGrid();
            if (grid != null)
            {
                var item1 = slot1 != null ? slot1.GetLockerItem() : null;
                var item2 = slot2 != null ? slot2.GetLockerItem() : null;
                var item3 = slot3 != null ? slot3.GetLockerItem() : null;
                
                if (item1 != null) grid.HandleWarrantAssigned(item1);
                if (item2 != null) grid.HandleWarrantAssigned(item2);
                if (item3 != null) grid.HandleWarrantAssigned(item3);
            }
            
            SetStatusText($"Successfully fused warrants! Click 'Collect' to save {result.fusedWarrant.displayName} to your locker.", Color.green);
            
            // Clear input slots manually (don't call ClearSlots() as it might interfere with output slot)
            // Cancel any ongoing selection
            CancelSelection();
            
            if (slot1 != null) slot1.ClearSlot();
            if (slot2 != null) slot2.ClearSlot();
            if (slot3 != null) slot3.ClearSlot();
            
            // Clear blueprint state (but don't clear output slot - it has the fused warrant)
            if (fusionBlueprint != null)
            {
                Destroy(fusionBlueprint);
                fusionBlueprint = null;
            }
            
            // Reset selection state for next fusion
            notableConfirmed = false;
            affixConfirmed = false;
            selectedNotable = null;
            selectedAffix = null;
            selectedNotableSlotIndex = -1;
            selectedAffixSlotIndex = -1;
            
            lockedModifiers.Clear();
            RefreshModifierLockUI();
            UpdateFuseButtonState();
        }
        else
        {
            SetStatusText($"Fusion failed: {result.errorMessage}", Color.red);
        }
    }
    
    /// <summary>
    /// Called when the Collect button is clicked. Adds the fused warrant to the locker.
    /// </summary>
    private void OnCollectButtonClicked()
    {
        if (currentFusedWarrant == null)
        {
            Debug.LogWarning("[WarrantFusionUI] CollectButton clicked but no fused warrant to collect!");
            return;
        }
        
        // Add fused warrant to locker (if available)
        var grid = GetLockerGrid();
        if (grid != null)
        {
            grid.AddWarrantInstance(currentFusedWarrant);
            SetStatusText($"Collected {currentFusedWarrant.displayName} to your locker!", Color.green);
            Debug.Log($"[WarrantFusionUI] Collected fused warrant '{currentFusedWarrant.displayName}' to locker");
        }
        else
        {
            Debug.LogWarning("[WarrantFusionUI] No WarrantLockerGrid found. Cannot collect warrant.");
            SetStatusText("Error: No warrant locker available. Cannot collect warrant.", Color.red);
            return;
        }
        
        // Clear the output slot and hide CollectButton
        if (outputSlot != null)
        {
            outputSlot.ClearSlot();
        }
        
        if (collectButton != null)
        {
            collectButton.gameObject.SetActive(false);
        }
        
        // Clear the stored fused warrant
        currentFusedWarrant = null;
    }
    
    private void OnClearButtonClicked()
    {
        ClearSlots();
        SetStatusText("Slots cleared.", Color.white);
    }
    
    private void OnCloseButtonClicked()
    {
        // Cancel any ongoing selection
        CancelSelection();
        HidePanel();
    }
    
    private void ClearSlots()
    {
        // Cancel any ongoing selection
        CancelSelection();
        
        if (slot1 != null) slot1.ClearSlot();
        if (slot2 != null) slot2.ClearSlot();
        if (slot3 != null) slot3.ClearSlot();
        
        // Clear blueprint (but don't clear output slot if there's a fused warrant waiting to be collected)
        if (currentFusedWarrant == null)
        {
            ClearFusionBlueprint();
        }
        
        lockedModifiers.Clear();
        RefreshModifierLockUI();
        UpdateFuseButtonState();
        UpdateStatusText();
    }
    
    /// <summary>
    /// Cancels any ongoing warrant selection and hides the locker panel.
    /// </summary>
    private void CancelSelection()
    {
        if (currentlySelectingSlot != null)
        {
            currentlySelectingSlot = null;
            HideLockerPanel();
            DisableSelectionModeOnLockerItems();
        }
    }
    
    private bool CanFuse()
    {
        // Can fuse only when all slots are filled AND both notable and affix are confirmed
        return slot1 != null && slot1.CurrentWarrant != null &&
               slot2 != null && slot2.CurrentWarrant != null &&
               slot3 != null && slot3.CurrentWarrant != null &&
               notableConfirmed && affixConfirmed;
    }
    
    private void UpdateFuseButtonState()
    {
        if (fuseButton != null)
        {
            bool canFuse = CanFuse();
            fuseButton.interactable = canFuse;
            
            // Add pulsing animation when ready to fuse
            // Note: You may need to add DOTween if not already in the project
            // For now, we'll just enable/disable the button
            // You can add visual feedback (color change, scale pulse) in the UI prefab
        }
    }
    
    private void UpdateStatusText()
    {
        // Check if all 3 slots are filled
        bool allSlotsFilled = slot1?.CurrentWarrant != null && 
                             slot2?.CurrentWarrant != null && 
                             slot3?.CurrentWarrant != null;
        
        if (!allSlotsFilled)
        {
            SetStatusText("Place 3 warrants in the input slots to begin fusion.", Color.gray);
            return;
        }
        
        // Build status message based on selection progress
        if (!notableConfirmed)
        {
            if (pendingNotableItem != null)
            {
                SetStatusText("Notable selected - Click 'Confirm' to proceed to Affix selection.", Color.cyan);
            }
            else
            {
                SetStatusText("Step 1: Select a Notable from the list above, then click 'Confirm'.", Color.yellow);
            }
        }
        else if (!affixConfirmed)
        {
            if (pendingAffixItem != null)
            {
                SetStatusText("Affix selected - Click 'Confirm' to finalize selection.", Color.cyan);
            }
            else
            {
                SetStatusText("Step 2: Select an Affix from the list above, then click 'Confirm'.", Color.yellow);
            }
        }
        else
        {
            SetStatusText($"Ready to fuse! Notable: {selectedNotable?.displayName} | Affix: {selectedAffix?.displayName ?? selectedAffix?.modifierId}", Color.green);
        }
    }
    
    private void SetStatusText(string text, Color color)
    {
        if (statusText != null)
        {
            statusText.text = text;
            statusText.color = color;
        }
    }
    
    private class ModifierSource
    {
        public int slotIndex;
        public WarrantModifier modifier;
    }
    
    private class NotableSource
    {
        public int slotIndex;
        public WarrantNotableDefinition notable;
    }
}

