using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using Dexiled.Data.Items;

public class EquipmentScreen : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    
    [Header("Equipment Slots")]
    public EquipmentSlot helmetSlot;
    public EquipmentSlot amuletSlot;
    public EquipmentSlot mainHandSlot;
    public EquipmentSlot bodyArmourSlot;
    public EquipmentSlot offHandSlot;
    public EquipmentSlot glovesSlot;
    public EquipmentSlot leftRingSlot;
    public EquipmentSlot rightRingSlot;
    public EquipmentSlot beltSlot;
    public EquipmentSlot bootsSlot;
    
    [Header("Inventory")]
    public int inventoryWidth = 10;
    public int inventoryHeight = 6;
    
    // UI Elements
    private VisualElement mainContainer;
    private VisualElement equipmentPanel;
    private VisualElement inventoryPanel;
    private VisualElement inventoryGrid;
    private VisualElement stashGrid;
    
    // Section elements
    private VisualElement inventorySection;
    private VisualElement stashSection;
    
    // Stash tabs
    private VisualElement stashTabsContainer;
    private VisualElement stashTabs;
    private Button addStashTabButton;
    
    // Currency system
    private VisualElement currencySection;
    private VisualElement currencyGrid;
    private List<VisualElement> currencySlots = new List<VisualElement>();
    private List<CurrencyData> playerCurrencies = new List<CurrencyData>();
    private CurrencyDatabase currencyDatabase;
    
    // Currency tab system
    private VisualElement currencyTabsContainer;
    private Button orbsTabButton;
    private Button spiritsTabButton;
    private Button sealsTabButton;
    private Button fragmentsTabButton;
    
    private VisualElement orbsTabContent;
    private VisualElement spiritsTabContent;
    private VisualElement sealsTabContent;
    private VisualElement fragmentsTabContent;
    
    private VisualElement orbsCurrencyGrid;
    private VisualElement spiritsCurrencyGrid;
    private VisualElement sealsCurrencyGrid;
    private VisualElement fragmentsCurrencyGrid;
    
    private List<VisualElement> orbsSlots = new List<VisualElement>();
    private List<VisualElement> spiritsSlots = new List<VisualElement>();
    private List<VisualElement> sealsSlots = new List<VisualElement>();
    private List<VisualElement> fragmentsSlots = new List<VisualElement>();
    
    private string currentCurrencyTab = "Orbs";
    private Button renameTabButton;
    
    // Equipment slots
    private VisualElement helmetSlotElement;
    private VisualElement amuletSlotElement;
    private VisualElement mainHandSlotElement;
    private VisualElement bodyArmourSlotElement;
    private VisualElement offHandSlotElement;
    private VisualElement glovesSlotElement;
    private VisualElement leftRingSlotElement;
    private VisualElement rightRingSlotElement;
    private VisualElement beltSlotElement;
    private VisualElement bootsSlotElement;
    
    // Buttons
    private Button returnButton;
    private Button characterButton;
    private Button skillsButton;
    private Button sortButton;
    private Button filterButton;
    private Button stashSortButton;
    private Button stashFilterButton;
    
    // Inventory slots
    private List<VisualElement> inventorySlots = new List<VisualElement>();
    private List<VisualElement> stashSlots = new List<VisualElement>();
    
    // Equipment data
    private EquipmentManager equipmentManager;
    private List<ItemData> inventoryItems = new List<ItemData>();
    
    // Drag and drop system
    private ItemData draggedItem = null;
    private int draggedItemIndex = -1;
    private string draggedItemSource = ""; // "inventory", "stash", "equipment"
    private int draggedStashTabIndex = -1;
    private VisualElement draggedElement = null;
    
    // Stash data
    private List<StashTab> stashTabsList = new List<StashTab>();
    private int currentStashTabIndex = 0;
    
    // Tooltip system
    private VisualElement tooltipElement;
    
    // Effigy system
    private EffigyGrid effigyGrid;
    private VisualElement effigyGridContainer;
    private List<Effigy> playerEffigies = new List<Effigy>();
    
    // Effigy Storage Panel
    private VisualElement effigyStoragePanel;
    private VisualElement effigyStorageContent;
    private Button closeEffigyStorageButton;
    private Button openEffigyStorageButton;
    private bool isEffigyStorageOpen = false;
    private float storagePanelWidth = 400f;
    
    private void Start()
    {
        // Ensure correct grid dimensions
        inventoryWidth = 10;
        inventoryHeight = 6;
        
        // Get or create EquipmentManager
        equipmentManager = EquipmentManager.Instance;
        
        InitializeUI();
        SetupEventHandlers();
        GenerateInventoryGrid();
        GenerateStashGrid();
        InitializeStashTabs();
        InitializeCurrencySystem();
        InitializeEffigyGrid();
        LoadEffigiesFromResources(); // Load all effigies for storage display
        
        // Debug: Log grid creation
        Debug.Log($"Inventory grid created with {inventorySlots.Count} slots ({inventoryWidth}x{inventoryHeight} = {inventoryWidth * inventoryHeight} expected) - 60x60px slots");
        Debug.Log($"Stash grid created with {stashSlots.Count} slots ({inventoryWidth}x{inventoryHeight} = {inventoryWidth * inventoryHeight} expected) - 60x60px slots");
    }
    
    /// <summary>
    /// Initialize the Effigy grid system
    /// </summary>
    private void InitializeEffigyGrid()
    {
        if (effigyGridContainer == null)
        {
            Debug.LogError("[EquipmentScreen] EffigyGridContainer not found - cannot initialize Effigy grid");
            return;
        }
        
        try
        {
            // Create the effigy grid
            effigyGrid = new EffigyGrid(effigyGridContainer);
            
            // Load player's effigies (TODO: Load from save data)
            Debug.Log("[EquipmentScreen] Effigy grid initialized (6x4 grid)");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EquipmentScreen] Error initializing Effigy grid: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    /// <summary>
    /// Initialize Effigy Storage Panel
    /// </summary>
    private void InitializeEffigyStorage()
    {
        if (effigyStoragePanel == null)
        {
            Debug.LogWarning("[EquipmentScreen] EffigyStoragePanel not found");
            return;
        }
        
        // Setup panel positioning
        effigyStoragePanel.style.position = Position.Absolute;
        effigyStoragePanel.style.top = 0;
        effigyStoragePanel.style.bottom = 0;
        effigyStoragePanel.style.right = -storagePanelWidth; // Start off-screen
        effigyStoragePanel.style.width = storagePanelWidth;
        effigyStoragePanel.style.display = DisplayStyle.None;
        
        // Ensure panel appears on top of other elements
        effigyStoragePanel.BringToFront();
        
        Debug.Log($"[EffigyStorage] Panel initialized - width: {storagePanelWidth}, initial right: {-storagePanelWidth}");
        
        // Setup close button
        if (closeEffigyStorageButton != null)
        {
            closeEffigyStorageButton.clicked += ToggleEffigyStorage;
        }
        
        Debug.Log("[EquipmentScreen] Effigy Storage Panel initialized");
    }
    
    /// <summary>
    /// Create button to open Effigy Storage
    /// </summary>
    private void CreateEffigyStorageButton(VisualElement root)
    {
        if (effigyGridContainer == null) return;
        
        // Find or create header container
        VisualElement headerLabel = effigyGridContainer.Q<Label>();
        if (headerLabel != null && headerLabel.parent != null)
        {
            // Check if button already exists
            VisualElement existingButton = headerLabel.parent.Q<Button>("OpenEffigyStorageButton");
            if (existingButton != null)
            {
                openEffigyStorageButton = existingButton as Button;
            }
            else
            {
                // Create button container
                VisualElement headerContainer = new VisualElement();
                headerContainer.name = "EffigyHeaderContainer";
                headerContainer.AddToClassList("effigy-header-container");
                
                // Create storage button
                openEffigyStorageButton = new Button();
                openEffigyStorageButton.name = "OpenEffigyStorageButton";
                openEffigyStorageButton.text = "📦 Storage";
                openEffigyStorageButton.AddToClassList("effigy-storage-button");
                openEffigyStorageButton.clicked += ToggleEffigyStorage;
                
                // Replace label with header container (button + label)
                VisualElement parent = headerLabel.parent;
                int labelIndex = parent.IndexOf(headerLabel);
                headerLabel.RemoveFromHierarchy();
                headerContainer.Add(openEffigyStorageButton);
                headerContainer.Add(headerLabel);
                parent.Insert(labelIndex, headerContainer);
            }
        }
        else
        {
            // Fallback: add directly to container
            openEffigyStorageButton = new Button();
            openEffigyStorageButton.name = "OpenEffigyStorageButton";
            openEffigyStorageButton.text = "📦 Storage";
            openEffigyStorageButton.AddToClassList("effigy-storage-button");
            openEffigyStorageButton.clicked += ToggleEffigyStorage;
            effigyGridContainer.Insert(0, openEffigyStorageButton);
        }
    }
    
    /// <summary>
    /// Toggle Effigy Storage Panel
    /// </summary>
    private void ToggleEffigyStorage()
    {
        if (effigyStoragePanel == null) return;
        
        isEffigyStorageOpen = !isEffigyStorageOpen;
        
        if (isEffigyStorageOpen)
        {
            ShowEffigyStorage();
        }
        else
        {
            HideEffigyStorage();
        }
    }
    
    /// <summary>
    /// Show Effigy Storage Panel
    /// </summary>
    private void ShowEffigyStorage()
    {
        if (effigyStoragePanel == null) return;
        
        // Build storage content if needed
        if (effigyStorageContent != null && effigyStorageContent.childCount == 0)
        {
            BuildEffigyStorageContent();
        }
        
        // Ensure panel is visible and on top
        effigyStoragePanel.style.display = DisplayStyle.Flex;
        effigyStoragePanel.BringToFront();
        
        // Use simple animation without coroutine
        effigyStoragePanel.style.right = 0;
        StartCoroutine(AnimatePanelSlide(-storagePanelWidth, 0f, 0.3f));
    }
    
    /// <summary>
    /// Hide Effigy Storage Panel
    /// </summary>
    private void HideEffigyStorage()
    {
        if (effigyStoragePanel == null) return;
        
        StartCoroutine(AnimatePanelSlide(0f, -storagePanelWidth, 0.3f, () => {
            effigyStoragePanel.style.display = DisplayStyle.None;
        }));
    }
    
    /// <summary>
    /// Animate panel slide
    /// </summary>
    private System.Collections.IEnumerator AnimatePanelSlide(float startRight, float endRight, float duration, System.Action onComplete = null)
    {
        float elapsed = 0f;
        
        while (elapsed < duration && effigyStoragePanel != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Ease out quad
            t = 1f - (1f - t) * (1f - t);
            
            float currentRight = Mathf.Lerp(startRight, endRight, t);
            effigyStoragePanel.style.right = currentRight;
            
            // Keep bringing to front during animation
            effigyStoragePanel.BringToFront();
            
            yield return null;
        }
        
        if (effigyStoragePanel != null)
        {
            effigyStoragePanel.style.right = endRight;
        }
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Load all Effigies from Resources
    /// </summary>
    private void LoadEffigiesFromResources()
    {
        playerEffigies.Clear();
        
        // Load all Effigy assets from Resources
        Effigy[] allEffigies = Resources.LoadAll<Effigy>("Items");
        playerEffigies.AddRange(allEffigies);
        
        Debug.Log($"[EquipmentScreen] Loaded {playerEffigies.Count} effigies from Resources");
    }
    
    /// <summary>
    /// Build the storage content with grouped effigies
    /// </summary>
    private void BuildEffigyStorageContent()
    {
        if (effigyStorageContent == null) return;
        
        effigyStorageContent.Clear();
        
        // Group effigies by element
        var groupedByElement = playerEffigies.GroupBy(e => e.element).OrderBy(g => g.Key);
        
        foreach (var elementGroup in groupedByElement)
        {
            // Create section header
            VisualElement sectionHeader = new VisualElement();
            sectionHeader.name = $"EffigySection_{elementGroup.Key}";
            sectionHeader.AddToClassList("effigy-storage-section-header");
            
            Label headerLabel = new Label(elementGroup.Key.ToString().ToUpper());
            headerLabel.style.fontSize = 16;
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerLabel.style.color = GetElementColor(elementGroup.Key);
            sectionHeader.Add(headerLabel);
            
            effigyStorageContent.Add(sectionHeader);
            
            // Create grid for this element
            VisualElement elementGrid = new VisualElement();
            elementGrid.name = $"EffigyGrid_{elementGroup.Key}";
            elementGrid.AddToClassList("effigy-storage-grid");
            elementGrid.style.flexDirection = FlexDirection.Row;
            elementGrid.style.flexWrap = Wrap.Wrap;
            elementGrid.style.marginBottom = 20;
            
            // Add effigies to grid
            foreach (Effigy effigy in elementGroup.OrderBy(e => e.rarity).ThenBy(e => e.effigyName))
            {
                VisualElement effigySlot = CreateEffigyStorageSlot(effigy);
                elementGrid.Add(effigySlot);
            }
            
            effigyStorageContent.Add(elementGrid);
        }
    }
    
    /// <summary>
    /// Create a visual slot for an effigy in storage
    /// </summary>
    private VisualElement CreateEffigyStorageSlot(Effigy effigy)
    {
        VisualElement slot = new VisualElement();
        slot.name = $"EffigySlot_{effigy.name}";
        slot.AddToClassList("effigy-storage-slot");
        
        // Size based on cell count
        int cellCount = effigy.GetCellCount();
        float slotSize = 60 + (cellCount - 1) * 10; // Scale slightly with size
        slot.style.width = slotSize;
        slot.style.height = slotSize;
        slot.style.minWidth = 60;
        slot.style.minHeight = 60;
        
        // Background color based on element and rarity
        Color elementColor = effigy.GetElementColor();
        float rarityBrightness = GetRarityBrightness(effigy.rarity);
        slot.style.backgroundColor = elementColor * rarityBrightness;
        slot.style.borderLeftWidth = 2;
        slot.style.borderRightWidth = 2;
        slot.style.borderTopWidth = 2;
        slot.style.borderBottomWidth = 2;
        slot.style.borderLeftColor = GetRarityColor(effigy.rarity);
        slot.style.borderRightColor = GetRarityColor(effigy.rarity);
        slot.style.borderTopColor = GetRarityColor(effigy.rarity);
        slot.style.borderBottomColor = GetRarityColor(effigy.rarity);
        slot.style.borderTopLeftRadius = 4;
        slot.style.borderTopRightRadius = 4;
        slot.style.borderBottomLeftRadius = 4;
        slot.style.borderBottomRightRadius = 4;
        slot.style.marginLeft = 5;
        slot.style.marginRight = 5;
        slot.style.marginTop = 5;
        slot.style.marginBottom = 5;
        
        // Add icon if available
        if (effigy.icon != null)
        {
            Image iconImage = new Image();
            iconImage.image = effigy.icon.texture;
            iconImage.style.width = Length.Percent(100);
            iconImage.style.height = Length.Percent(100);
            slot.Add(iconImage);
        }
        
        // Add name label
        Label nameLabel = new Label(effigy.effigyName);
        nameLabel.style.fontSize = 10;
        nameLabel.style.color = Color.white;
        nameLabel.style.unityTextAlign = TextAnchor.LowerCenter;
        nameLabel.style.position = Position.Absolute;
        nameLabel.style.bottom = 2;
        nameLabel.style.left = 2;
        nameLabel.style.right = 2;
        nameLabel.style.backgroundColor = new Color(0, 0, 0, 0.7f);
        slot.Add(nameLabel);
        
        // Add click handler to drag to grid
        slot.RegisterCallback<MouseDownEvent>(evt => OnStorageEffigyClicked(effigy, evt));
        
        return slot;
    }
    
    private void OnStorageEffigyClicked(Effigy effigy, MouseDownEvent evt)
    {
        // Start drag from storage to grid
        if (effigyGrid != null)
        {
            effigyGrid.StartDragFromStorage(effigy);
            Debug.Log($"[EquipmentScreen] Started dragging effigy from storage: {effigy.effigyName}");
        }
    }
    
    private Color GetElementColor(EffigyElement element)
    {
        switch (element)
        {
            case EffigyElement.Fire: return new Color(1f, 0.3f, 0.2f);
            case EffigyElement.Cold: return new Color(0.2f, 0.6f, 1f);
            case EffigyElement.Lightning: return new Color(1f, 0.9f, 0.2f);
            case EffigyElement.Physical: return new Color(0.7f, 0.7f, 0.7f);
            case EffigyElement.Chaos: return new Color(0.8f, 0.2f, 0.8f);
            default: return Color.white;
        }
    }
    
    /// <summary>
    /// Get rarity color for borders and text
    /// </summary>
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return Color.white;
            case ItemRarity.Magic: return new Color(0.3f, 0.6f, 1f); // Blue
            case ItemRarity.Rare: return new Color(1f, 0.8f, 0.2f); // Gold
            case ItemRarity.Unique: return new Color(1f, 0.5f, 0f); // Orange
            default: return Color.white;
        }
    }
    
    private float GetRarityBrightness(ItemRarity rarity)
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
    
    /// <summary>
    /// Try to place an effigy from inventory into the grid
    /// </summary>
    public bool TryPlaceEffigyFromInventory(Effigy effigy, int gridX, int gridY)
    {
        if (effigyGrid == null || effigy == null)
            return false;
        
        if (effigyGrid.TryPlaceEffigy(effigy, gridX, gridY))
        {
            // Remove from inventory if successful
            // TODO: Implement inventory removal
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Remove an effigy from the grid and return it to inventory
    /// </summary>
    public void RemoveEffigyFromGrid(Effigy effigy)
    {
        if (effigyGrid == null || effigy == null)
            return;
        
        effigyGrid.RemoveEffigy(effigy);
        // TODO: Add back to inventory
    }
    
    private void InitializeUI()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
            
        VisualElement root = uiDocument.rootVisualElement;
        
        // Get main containers
        mainContainer = root.Q<VisualElement>("MainContainer");
        equipmentPanel = root.Q<VisualElement>("EquipmentPanel");
        inventoryPanel = root.Q<VisualElement>("InventoryPanel");
        inventoryGrid = root.Q<VisualElement>("InventoryGrid");
        stashGrid = root.Q<VisualElement>("StashGrid");
        
        // Validate critical UI elements
        if (inventoryGrid == null)
        {
            Debug.LogError("[EquipmentScreen] InventoryGrid not found in UXML! Cannot generate inventory slots.");
        }
        
        if (stashGrid == null)
        {
            Debug.LogError("[EquipmentScreen] StashGrid not found in UXML! Cannot generate stash slots.");
        }
        
        // Initialize Effigy grid container
        effigyGridContainer = root.Q<VisualElement>("EffigyGridContainer");
        if (effigyGridContainer == null)
        {
            // Create container if it doesn't exist in UXML
            effigyGridContainer = new VisualElement();
            effigyGridContainer.name = "EffigyGridContainer";
            effigyGridContainer.style.width = Length.Percent(100);
            effigyGridContainer.style.height = 300; // Approximate height for 4 rows
            equipmentPanel.Add(effigyGridContainer);
        }
        
        // Initialize Effigy Storage Panel
        effigyStoragePanel = root.Q<VisualElement>("EffigyStoragePanel");
        effigyStorageContent = root.Q<VisualElement>("EffigyStorageContent");
        closeEffigyStorageButton = root.Q<Button>("CloseEffigyStorageButton");
        
        if (effigyStoragePanel != null)
        {
            // Move storage panel to end of root to ensure it renders on top
            effigyStoragePanel.RemoveFromHierarchy();
            root.Add(effigyStoragePanel);
            
            // Setup storage panel
            InitializeEffigyStorage();
        }
        
        // Create button to open storage (add to header or near effigy grid)
        CreateEffigyStorageButton(root);
        
        // Get section elements
        inventorySection = root.Q<VisualElement>("InventorySection");
        stashSection = root.Q<VisualElement>("StashSection");
        
        // Get stash elements
        stashTabsContainer = root.Q<VisualElement>("StashTabsContainer");
        stashTabs = root.Q<VisualElement>("StashTabs");
        addStashTabButton = root.Q<Button>("AddStashTabButton");
        
        // Get currency elements
        currencySection = root.Q<VisualElement>("CurrencySection");
        currencyGrid = root.Q<VisualElement>("CurrencyGrid");
        
        // Initialize currency tab system
        currencyTabsContainer = root.Q<VisualElement>("CurrencyTabsContainer");
        orbsTabButton = root.Q<Button>("OrbsTabButton");
        spiritsTabButton = root.Q<Button>("SpiritsTabButton");
        sealsTabButton = root.Q<Button>("SealsTabButton");
        fragmentsTabButton = root.Q<Button>("FragmentsTabButton");
        
        orbsTabContent = root.Q<VisualElement>("OrbsTabContent");
        spiritsTabContent = root.Q<VisualElement>("SpiritsTabContent");
        sealsTabContent = root.Q<VisualElement>("SealsTabContent");
        fragmentsTabContent = root.Q<VisualElement>("FragmentsTabContent");
        
        orbsCurrencyGrid = root.Q<VisualElement>("OrbsCurrencyGrid");
        spiritsCurrencyGrid = root.Q<VisualElement>("SpiritsCurrencyGrid");
        sealsCurrencyGrid = root.Q<VisualElement>("SealsCurrencyGrid");
        fragmentsCurrencyGrid = root.Q<VisualElement>("FragmentsCurrencyGrid");
        
        renameTabButton = root.Q<Button>("RenameTabButton");
        
        // Get equipment slot elements
        helmetSlotElement = root.Q<VisualElement>("HelmetSlot");
        amuletSlotElement = root.Q<VisualElement>("AmuletSlot");
        mainHandSlotElement = root.Q<VisualElement>("MainHandSlot");
        bodyArmourSlotElement = root.Q<VisualElement>("BodyArmourSlot");
        offHandSlotElement = root.Q<VisualElement>("OffHandSlot");
        glovesSlotElement = root.Q<VisualElement>("GlovesSlot");
        leftRingSlotElement = root.Q<VisualElement>("LeftRingSlot");
        rightRingSlotElement = root.Q<VisualElement>("RightRingSlot");
        beltSlotElement = root.Q<VisualElement>("BeltSlot");
        bootsSlotElement = root.Q<VisualElement>("BootsSlot");
        
        // Add click handlers for equipment slots
        helmetSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.Helmet));
        amuletSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.Amulet));
        mainHandSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.MainHand));
        bodyArmourSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.BodyArmour));
        offHandSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.OffHand));
        glovesSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.Gloves));
        leftRingSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.LeftRing));
        rightRingSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.RightRing));
        beltSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.Belt));
        bootsSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.Boots));
        
        // Get buttons
        returnButton = root.Q<Button>("ReturnButton");
        characterButton = root.Q<Button>("CharacterButton");
        skillsButton = root.Q<Button>("SkillsButton");
        sortButton = root.Q<Button>("SortButton");
        filterButton = root.Q<Button>("FilterButton");
        stashSortButton = root.Q<Button>("StashSortButton");
        stashFilterButton = root.Q<Button>("StashFilterButton");
        
        // Initialize tooltip
        InitializeTooltip();
    }
    
    private void InitializeTooltip()
    {
        // Create tooltip element
        tooltipElement = new VisualElement();
        tooltipElement.AddToClassList("tooltip");
        tooltipElement.style.display = DisplayStyle.None;
        tooltipElement.style.position = Position.Absolute;
        // Note: Unity USS doesn't support z-index, positioning handles layering
        
        // Add tooltip to root
        uiDocument.rootVisualElement.Add(tooltipElement);
    }
    
    private void SetupEventHandlers()
    {
        // Navigation buttons
        if (returnButton != null)
            returnButton.clicked += OnReturnButtonClicked;
        
        if (characterButton != null)
            characterButton.clicked += OnCharacterButtonClicked;
        
        if (skillsButton != null)
            skillsButton.clicked += OnSkillsButtonClicked;
        
        // Inventory buttons
        if (sortButton != null)
            sortButton.clicked += OnSortButtonClicked;
        
        if (filterButton != null)
            filterButton.clicked += OnFilterButtonClicked;
        
        // Stash buttons
        if (stashSortButton != null)
            stashSortButton.clicked += OnStashSortButtonClicked;
        
        if (stashFilterButton != null)
            stashFilterButton.clicked += OnStashFilterButtonClicked;
        
        if (addStashTabButton != null)
            addStashTabButton.clicked += OnAddStashTabClicked;
        
        if (renameTabButton != null)
            renameTabButton.clicked += OnRenameTabClicked;
        
        // Currency tab buttons
        if (orbsTabButton != null)
            orbsTabButton.clicked += () => SwitchCurrencyTab("Orbs");
        
        if (spiritsTabButton != null)
            spiritsTabButton.clicked += () => SwitchCurrencyTab("Spirits");
        
        if (sealsTabButton != null)
            sealsTabButton.clicked += () => SwitchCurrencyTab("Seals");
        
        if (fragmentsTabButton != null)
            fragmentsTabButton.clicked += () => SwitchCurrencyTab("Fragments");
        
        // Equipment slot events
        SetupEquipmentSlotEvents();
    }
    
    private void SetupEquipmentSlotEvents()
    {
        // Add click events to all equipment slots (with null checks)
        if (helmetSlotElement != null)
            helmetSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.Helmet));
        
        if (amuletSlotElement != null)
            amuletSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.Amulet));
        
        if (mainHandSlotElement != null)
            mainHandSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.MainHand));
        
        if (bodyArmourSlotElement != null)
            bodyArmourSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.BodyArmour));
        
        if (offHandSlotElement != null)
            offHandSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.OffHand));
        
        if (glovesSlotElement != null)
            glovesSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.Gloves));
        
        if (leftRingSlotElement != null)
            leftRingSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.LeftRing));
        
        if (rightRingSlotElement != null)
            rightRingSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.RightRing));
        
        if (beltSlotElement != null)
            beltSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.Belt));
        
        if (bootsSlotElement != null)
            bootsSlotElement.RegisterCallback<ClickEvent>(evt => OnEquipmentSlotClicked(EquipmentType.Boots));
        
        // Add hover events for tooltips
        SetupTooltipEvents();
    }
    
    private void SetupTooltipEvents()
    {
        // Add hover events to equipment slots for tooltips (with null checks)
        if (helmetSlotElement != null)
        {
            helmetSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.Helmet, evt.mousePosition));
            helmetSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }
        
        if (amuletSlotElement != null)
        {
            amuletSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.Amulet, evt.mousePosition));
            amuletSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }
        
        if (mainHandSlotElement != null)
        {
            mainHandSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.MainHand, evt.mousePosition));
            mainHandSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }
        
        if (bodyArmourSlotElement != null)
        {
            bodyArmourSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.BodyArmour, evt.mousePosition));
            bodyArmourSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }
        
        if (offHandSlotElement != null)
        {
            offHandSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.OffHand, evt.mousePosition));
            offHandSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }
        
        if (glovesSlotElement != null)
        {
            glovesSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.Gloves, evt.mousePosition));
            glovesSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }
        
        if (leftRingSlotElement != null)
        {
            leftRingSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.LeftRing, evt.mousePosition));
            leftRingSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }
        
        if (rightRingSlotElement != null)
        {
            rightRingSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.RightRing, evt.mousePosition));
            rightRingSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }
        
        if (beltSlotElement != null)
        {
            beltSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.Belt, evt.mousePosition));
            beltSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }
        
        if (bootsSlotElement != null)
        {
            bootsSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.Boots, evt.mousePosition));
            bootsSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }
    }
    
    private void GenerateInventoryGrid()
    {
        if (inventoryGrid == null)
        {
            Debug.LogError("[EquipmentScreen] Cannot generate inventory grid - InventoryGrid element is null!");
            return;
        }
        
        Debug.Log($"GenerateInventoryGrid called - inventoryWidth: {inventoryWidth}, inventoryHeight: {inventoryHeight}");
        Debug.Log($"Expected total slots: {inventoryWidth * inventoryHeight}");
        inventoryGrid.Clear();
        inventorySlots.Clear();
        
        // Create a container for proper grid layout
        VisualElement gridContainer = new VisualElement();
        gridContainer.style.flexDirection = FlexDirection.Row;
        gridContainer.style.flexWrap = Wrap.Wrap;
        gridContainer.style.width = Length.Percent(100);
        gridContainer.style.height = Length.Percent(100);
        gridContainer.style.minHeight = 400; // Ensure minimum height
        
        for (int y = 0; y < inventoryHeight; y++)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                VisualElement slot = new VisualElement();
                slot.AddToClassList("inventory-slot");
                slot.name = $"InventorySlot_{x}_{y}";
                
                // Ensure slot is visible even when empty - now 60x60
                slot.style.width = 60;
                slot.style.height = 60;
                slot.style.marginLeft = 1;
                slot.style.marginRight = 1;
                slot.style.marginTop = 1;
                slot.style.marginBottom = 1;
                slot.style.flexShrink = 0;
                
                // Add click event
                int slotIndex = y * inventoryWidth + x;
                slot.RegisterCallback<ClickEvent>(evt => OnInventorySlotClicked(slotIndex));
                
                // Add hover events for tooltips
                slot.RegisterCallback<MouseEnterEvent>(evt => ShowInventoryTooltip(slotIndex, evt.mousePosition));
                slot.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
                
                gridContainer.Add(slot);
                inventorySlots.Add(slot);
                
                // Debug: Log slot creation
                if (inventorySlots.Count % 10 == 0)
                {
                    Debug.Log($"Created {inventorySlots.Count} inventory slots so far");
                }
            }
        }
        
        inventoryGrid.Add(gridContainer);
        
        // Debug: Log actual slot creation
        Debug.Log($"Generated {inventorySlots.Count} inventory slots. Expected: {inventoryWidth * inventoryHeight}");
        Debug.Log($"Grid container has {gridContainer.childCount} children");
        Debug.Log($"Grid dimensions: {inventoryWidth}x{inventoryHeight}");
        Debug.Log($"Panel dimensions - Width: {inventoryGrid.worldBound.width}, Height: {inventoryGrid.worldBound.height}");
    }
    
    private void GenerateStashGrid()
    {
        if (stashGrid == null)
        {
            Debug.LogError("[EquipmentScreen] Cannot generate stash grid - StashGrid element is null!");
            return;
        }
        
        Debug.Log($"GenerateStashGrid called - inventoryWidth: {inventoryWidth}, inventoryHeight: {inventoryHeight}");
        Debug.Log($"Expected total slots: {inventoryWidth * inventoryHeight}");
        stashGrid.Clear();
        stashSlots.Clear();
        
        // Create a container for proper grid layout
        VisualElement gridContainer = new VisualElement();
        gridContainer.style.flexDirection = FlexDirection.Row;
        gridContainer.style.flexWrap = Wrap.Wrap;
        gridContainer.style.width = Length.Percent(100);
        gridContainer.style.height = Length.Percent(100);
        gridContainer.style.minHeight = 400; // Ensure minimum height
        
        for (int y = 0; y < inventoryHeight; y++)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                VisualElement slot = new VisualElement();
                slot.AddToClassList("stash-slot");
                slot.name = $"StashSlot_{x}_{y}";
                
                // Ensure slot is visible even when empty - now 60x60
                slot.style.width = 60;
                slot.style.height = 60;
                slot.style.marginLeft = 1;
                slot.style.marginRight = 1;
                slot.style.marginTop = 1;
                slot.style.marginBottom = 1;
                slot.style.flexShrink = 0;
                
                // Explicit styling to ensure visibility (matching inventory slots)
                slot.style.backgroundColor = new Color(0.12f, 0.14f, 0.16f, 1f); // rgb(30, 35, 40)
                slot.style.borderLeftWidth = 2;
                slot.style.borderRightWidth = 2;
                slot.style.borderTopWidth = 2;
                slot.style.borderBottomWidth = 2;
                slot.style.borderLeftColor = new Color(0.39f, 0.43f, 0.47f, 1f); // rgb(100, 110, 120)
                slot.style.borderRightColor = new Color(0.39f, 0.43f, 0.47f, 1f);
                slot.style.borderTopColor = new Color(0.39f, 0.43f, 0.47f, 1f);
                slot.style.borderBottomColor = new Color(0.39f, 0.43f, 0.47f, 1f);
                slot.style.borderTopLeftRadius = 4;
                slot.style.borderTopRightRadius = 4;
                slot.style.borderBottomLeftRadius = 4;
                slot.style.borderBottomRightRadius = 4;
                
                // Add click event
                int slotIndex = y * inventoryWidth + x;
                slot.RegisterCallback<ClickEvent>(evt => OnStashSlotClicked(slotIndex));
                
                // Add hover events for tooltips
                slot.RegisterCallback<MouseEnterEvent>(evt => ShowStashTooltip(slotIndex, evt.mousePosition));
                slot.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
                
                gridContainer.Add(slot);
                stashSlots.Add(slot);
                
                // Debug: Log slot creation
                if (stashSlots.Count % 10 == 0)
                {
                    Debug.Log($"Created {stashSlots.Count} stash slots so far");
                }
            }
        }
        
        stashGrid.Add(gridContainer);
        
        // Debug: Log actual slot creation
        Debug.Log($"Generated {stashSlots.Count} stash slots. Expected: {inventoryWidth * inventoryHeight}");
        Debug.Log($"Stash grid container has {gridContainer.childCount} children");
        Debug.Log($"Stash grid dimensions: {inventoryWidth}x{inventoryHeight}");
        Debug.Log($"Stash panel dimensions - Width: {stashGrid.worldBound.width}, Height: {stashGrid.worldBound.height}");
    }
    
    private void InitializeStashTabs()
    {
        // Create default stash tabs
        CreateStashTab("Tab 1");
        CreateStashTab("Tab 2");
        CreateStashTab("Tab 3");
        
        // Set first tab as active
        if (stashTabsList.Count > 0)
        {
            SetActiveStashTab(0);
        }
        
        // Update add button state
        UpdateAddStashTabButtonState();
    }
    
    private void CreateStashTab(string tabName)
    {
        StashTab newTab = new StashTab
        {
            tabName = tabName,
            items = new List<ItemData>()
        };
        
        stashTabsList.Add(newTab);
        CreateStashTabButton(newTab, stashTabsList.Count - 1);
    }
    
    private void CreateStashTabButton(StashTab tab, int tabIndex)
    {
        Button tabButton = new Button();
        tabButton.text = tab.tabName;
        tabButton.AddToClassList("stash-tab-button");
        tabButton.name = $"StashTab_{tabIndex}";
        
        // Add click event
        int index = tabIndex; // Capture the index
        tabButton.clicked += () => OnStashTabButtonClicked(index);
        
        // Add right-click event for renaming
        tabButton.RegisterCallback<MouseDownEvent>(evt => {
            if (evt.button == 1) // Right click
            {
                OnStashTabRightClicked(index, evt.mousePosition);
            }
        });
        
        stashTabs.Add(tabButton);
    }
    
    private void SetActiveStashTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= stashTabsList.Count) return;
        
        currentStashTabIndex = tabIndex;
        
        // Update tab button states
        for (int i = 0; i < stashTabs.childCount; i++)
        {
            Button tabButton = stashTabs.ElementAt(i) as Button;
            if (tabButton != null)
            {
                if (i == tabIndex)
                {
                    tabButton.AddToClassList("active");
                }
                else
                {
                    tabButton.RemoveFromClassList("active");
                }
            }
        }
        
        // Update stash grid display
        UpdateStashDisplay();
    }
    
    private void UpdateStashDisplay()
    {
        if (currentStashTabIndex >= 0 && currentStashTabIndex < stashTabsList.Count)
        {
            StashTab currentTab = stashTabsList[currentStashTabIndex];
            
            // Update stash slots
            for (int i = 0; i < stashSlots.Count; i++)
            {
                // Clear slot content and classes first
                stashSlots[i].Clear();
                stashSlots[i].RemoveFromClassList("occupied");
                stashSlots[i].RemoveFromClassList("common");
                stashSlots[i].RemoveFromClassList("rare");
                stashSlots[i].RemoveFromClassList("magic");
                stashSlots[i].RemoveFromClassList("unique");
                stashSlots[i].RemoveFromClassList("weapon");
                stashSlots[i].RemoveFromClassList("armour");
                stashSlots[i].RemoveFromClassList("accessory");
                stashSlots[i].RemoveFromClassList("consumable");
                stashSlots[i].RemoveFromClassList("material");
                
                if (i < currentTab.items.Count && currentTab.items[i] != null)
                {
                    // Add item visual to slot
                    stashSlots[i].AddToClassList("occupied");
                    stashSlots[i].AddToClassList(currentTab.items[i].rarity.ToString().ToLower());
                    stashSlots[i].AddToClassList(currentTab.items[i].itemType.ToString().ToLower());
                    
                    // Add item sprite if available
                    if (currentTab.items[i].itemSprite != null)
                    {
                        Image itemImage = new Image();
                        itemImage.sprite = currentTab.items[i].itemSprite;
                        itemImage.AddToClassList("item-image");
                        itemImage.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                        stashSlots[i].Add(itemImage);
                    }
                    
                    // Add item name label
                    Label itemLabel = new Label(currentTab.items[i].itemName);
                    itemLabel.AddToClassList("inventory-item-label");
                    itemLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    itemLabel.style.fontSize = 8;
                    itemLabel.style.color = GetRarityColor(currentTab.items[i].rarity);
                    stashSlots[i].Add(itemLabel);
                }
            }
        }
    }
    

    
    private void AddTestItems()
    {
        // Clear existing items
        inventoryItems.Clear();
        
        // Try to get items from the database
        if (ItemDatabase.Instance != null)
        {
            // Add the Rusted Sword specifically for testing
            WeaponItem rustedSword = ItemDatabase.Instance.weapons.Find(w => w.itemName == "Rusted Sword");
            if (rustedSword != null)
            {
                inventoryItems.Add(new ItemData { 
                    itemName = rustedSword.GetDisplayName(), 
                    itemType = ItemType.Weapon, 
                    rarity = rustedSword.rarity,
                    equipmentType = EquipmentType.MainHand,
                    itemSprite = rustedSword.itemIcon,
                    baseDamageMin = rustedSword.minDamage,
                    baseDamageMax = rustedSword.maxDamage,
                    criticalStrikeChance = rustedSword.criticalStrikeChance,
                    attackSpeed = rustedSword.attackSpeed,
                    requiredLevel = rustedSword.requiredLevel,
                    requiredStrength = rustedSword.requiredStrength,
                    requiredDexterity = rustedSword.requiredDexterity,
                    requiredIntelligence = rustedSword.requiredIntelligence,
                    implicitModifiers = ConvertAffixesToStrings(rustedSword.implicitModifiers),
                    prefixModifiers = ConvertAffixesToStrings(rustedSword.prefixes),
                    suffixModifiers = ConvertAffixesToStrings(rustedSword.suffixes),
                    stats = ConvertAffixesToStats(rustedSword.implicitModifiers, rustedSword.prefixes, rustedSword.suffixes)
                });
                Debug.Log($"Added Rusted Sword to inventory: {rustedSword.GetDisplayName()} - Sprite: {(rustedSword.itemIcon != null ? "Found" : "Missing")}");
            }
            else
            {
                Debug.LogWarning("Rusted Sword not found in ItemDatabase!");
            }
            
            // Add a random weapon as backup
            WeaponItem randomWeapon = ItemDatabase.Instance.GetRandomWeapon();
            if (randomWeapon != null && randomWeapon.itemName != "Rusted Sword")
            {
                inventoryItems.Add(new ItemData { 
                    itemName = randomWeapon.GetDisplayName(), 
                    itemType = ItemType.Weapon, 
                    rarity = randomWeapon.rarity,
                    equipmentType = EquipmentType.MainHand,
                    itemSprite = randomWeapon.itemIcon,
                    baseDamageMin = randomWeapon.minDamage,
                    baseDamageMax = randomWeapon.maxDamage,
                    criticalStrikeChance = randomWeapon.criticalStrikeChance,
                    attackSpeed = randomWeapon.attackSpeed,
                    requiredLevel = randomWeapon.requiredLevel,
                    requiredStrength = randomWeapon.requiredStrength,
                    requiredDexterity = randomWeapon.requiredDexterity,
                    requiredIntelligence = randomWeapon.requiredIntelligence,
                    implicitModifiers = ConvertAffixesToStrings(randomWeapon.implicitModifiers),
                    prefixModifiers = ConvertAffixesToStrings(randomWeapon.prefixes),
                    suffixModifiers = ConvertAffixesToStrings(randomWeapon.suffixes),
                    stats = ConvertAffixesToStats(randomWeapon.implicitModifiers, randomWeapon.prefixes, randomWeapon.suffixes)
                });
            }
            
            // Add all armour pieces for testing
            List<Armour> allArmour = ItemDatabase.Instance.armour;
            foreach (Armour armourPiece in allArmour)
            {
                if (armourPiece != null)
                {
                                    inventoryItems.Add(new ItemData { 
                    itemName = armourPiece.GetDisplayName(), 
                    itemType = ItemType.Armour, 
                    rarity = armourPiece.rarity,
                    equipmentType = ConvertArmourSlotToEquipmentType(armourPiece.armourSlot),
                    itemSprite = armourPiece.itemIcon,
                    baseArmour = armourPiece.armour,
                    baseEvasion = armourPiece.evasion,
                    baseEnergyShield = armourPiece.energyShield,
                    requiredLevel = armourPiece.requiredLevel,
                    requiredStrength = armourPiece.requiredStrength,
                    requiredDexterity = armourPiece.requiredDexterity,
                    requiredIntelligence = armourPiece.requiredIntelligence,
                    stats = ConvertAffixesToStats(armourPiece.implicitModifiers, armourPiece.prefixes, armourPiece.suffixes)
                });
                    
                    Debug.Log($"Added armour for testing: {armourPiece.itemName} (Slot: {armourPiece.armourSlot} -> EquipmentType: {ConvertArmourSlotToEquipmentType(armourPiece.armourSlot)})");
                }
            }
            
            // Add a random jewellery piece
            Jewellery randomJewellery = ItemDatabase.Instance.GetRandomJewellery();
            if (randomJewellery != null)
            {
                inventoryItems.Add(new ItemData { 
                    itemName = randomJewellery.GetDisplayName(), 
                    itemType = ItemType.Accessory, 
                    rarity = randomJewellery.rarity,
                    equipmentType = EquipmentType.Amulet,
                    itemSprite = randomJewellery.itemIcon
                });
            }
            
            // Add a random consumable
            Consumable randomConsumable = ItemDatabase.Instance.GetRandomConsumable();
            if (randomConsumable != null)
            {
                inventoryItems.Add(new ItemData { 
                    itemName = randomConsumable.GetDisplayName(), 
                    itemType = ItemType.Consumable, 
                    rarity = randomConsumable.rarity,
                    equipmentType = EquipmentType.MainHand,
                    itemSprite = randomConsumable.itemIcon
                });
            }
        }
        else
        {
            // Fallback to hardcoded items if database is not available
            inventoryItems.Add(new ItemData { 
                itemName = "Steel Axe", 
                itemType = ItemType.Weapon, 
                rarity = ItemRarity.Normal,
                equipmentType = EquipmentType.MainHand,
                itemSprite = null,
                baseDamageMin = 8,
                baseDamageMax = 15,
                criticalStrikeChance = 5.0f,
                attackSpeed = 1.10f,
                requiredLevel = 8,
                requiredStrength = 20,
                requiredDexterity = 0,
                requiredIntelligence = 0,
                implicitModifiers = new List<string>(),
                prefixModifiers = new List<string>(),
                suffixModifiers = new List<string>()
            });
            
            inventoryItems.Add(new ItemData { 
                itemName = "Leather Armour", 
                itemType = ItemType.Armour, 
                rarity = ItemRarity.Magic,
                equipmentType = EquipmentType.BodyArmour,
                itemSprite = null,
                baseArmour = 15f,
                baseEvasion = 25f,
                baseEnergyShield = 0f,
                requiredLevel = 5,
                requiredStrength = 8,
                requiredDexterity = 12,
                requiredIntelligence = 0,
                implicitModifiers = new List<string>(),
                prefixModifiers = new List<string>(),
                suffixModifiers = new List<string>(),
                stats = new Dictionary<string, float>()
            });
            
            inventoryItems.Add(new ItemData { 
                itemName = "Iron Ring", 
                itemType = ItemType.Accessory, 
                rarity = ItemRarity.Rare,
                equipmentType = EquipmentType.Amulet,
                itemSprite = null
            });
            
            inventoryItems.Add(new ItemData { 
                itemName = "Health Potion", 
                itemType = ItemType.Consumable, 
                rarity = ItemRarity.Normal,
                equipmentType = EquipmentType.MainHand,
                itemSprite = null
            });
        }
        
        // Debug: Log the number of items added
        Debug.Log($"Added {inventoryItems.Count} test items to inventory");
        
        // Update the display
        UpdateInventoryDisplay();
        
        // Add test items to stash tabs
        AddTestStashItems();
    }
    
    private void AddTestStashItems()
    {
        if (stashTabsList.Count > 0)
        {
            // Add items to first stash tab
            stashTabsList[0].items.Add(new ItemData { 
                itemName = "Stash Sword", 
                itemType = ItemType.Weapon, 
                rarity = ItemRarity.Rare,
                itemSprite = null, // No sprite for test items
                baseDamageMin = 12,
                baseDamageMax = 20,
                criticalStrikeChance = 6.5f,
                attackSpeed = 1.30f,
                requiredLevel = 12,
                requiredStrength = 25,
                requiredDexterity = 0,
                requiredIntelligence = 0,
                implicitModifiers = new List<string> { "15% INCREASED MELEE DAMAGE" },
                prefixModifiers = new List<string> { 
                    "Adds 8-12 Physical damage",
                    "25% increased physical damage"
                },
                suffixModifiers = new List<string> { 
                    "15% increased Attack Speed",
                    "+5 Life gained on hit"
                }
            });
            
            stashTabsList[0].items.Add(new ItemData { 
                itemName = "Stash Armor", 
                itemType = ItemType.Armour, 
                rarity = ItemRarity.Rare,
                itemSprite = null // No sprite for test items
            });
            
            // Add items to second stash tab
            stashTabsList[1].items.Add(new ItemData { 
                itemName = "Tab 2 Item", 
                itemType = ItemType.Accessory, 
                rarity = ItemRarity.Unique,
                itemSprite = null // No sprite for test items
            });
            
            // Update stash display
            UpdateStashDisplay();
        }
    }
    
    private void UpdateEquipmentDisplay()
    {
        // Update visual representation of equipped items using EquipmentManager
        UpdateEquipmentSlot(helmetSlotElement, equipmentManager?.GetEquippedItem(EquipmentType.Helmet));
        UpdateEquipmentSlot(mainHandSlotElement, equipmentManager?.GetEquippedItem(EquipmentType.MainHand));
        UpdateEquipmentSlot(bodyArmourSlotElement, equipmentManager?.GetEquippedItem(EquipmentType.BodyArmour));
        UpdateEquipmentSlot(amuletSlotElement, equipmentManager?.GetEquippedItem(EquipmentType.Amulet));
        UpdateEquipmentSlot(offHandSlotElement, equipmentManager?.GetEquippedItem(EquipmentType.OffHand));
        UpdateEquipmentSlot(glovesSlotElement, equipmentManager?.GetEquippedItem(EquipmentType.Gloves));
        UpdateEquipmentSlot(leftRingSlotElement, equipmentManager?.GetEquippedItem(EquipmentType.LeftRing));
        UpdateEquipmentSlot(rightRingSlotElement, equipmentManager?.GetEquippedItem(EquipmentType.RightRing));
        UpdateEquipmentSlot(beltSlotElement, equipmentManager?.GetEquippedItem(EquipmentType.Belt));
        UpdateEquipmentSlot(bootsSlotElement, equipmentManager?.GetEquippedItem(EquipmentType.Boots));
    }
    
    // Drag and Drop System Methods
    private void StartDraggingItem(ItemData item, int itemIndex, string source, int stashTabIndex)
    {
        if (item == null) return;
        
        draggedItem = item;
        draggedItemIndex = itemIndex;
        draggedItemSource = source;
        draggedStashTabIndex = stashTabIndex;
        
        Debug.Log($"Started dragging {item.itemName} from {source}");
        
        // Add visual feedback to the source slot
        AddDraggingVisualFeedback();
        
        // Create visual feedback for dragging
        CreateDragVisual(item);
        
        // Register mouse up event to handle drop
        uiDocument.rootVisualElement.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
    }
    
    private void AddDraggingVisualFeedback()
    {
        // Add dragging class to the source slot
        if (draggedItemSource == "inventory" && draggedItemIndex >= 0 && draggedItemIndex < inventorySlots.Count)
        {
            inventorySlots[draggedItemIndex].AddToClassList("dragging");
        }
        else if (draggedItemSource == "stash" && draggedItemIndex >= 0 && draggedItemIndex < stashSlots.Count)
        {
            stashSlots[draggedItemIndex].AddToClassList("dragging");
        }
        else if (draggedItemSource == "equipment")
        {
            // Find the equipment slot that contains the dragged item
            EquipmentType[] allEquipmentTypes = {
                EquipmentType.Helmet, EquipmentType.Amulet, EquipmentType.MainHand, EquipmentType.BodyArmour,
                EquipmentType.OffHand, EquipmentType.Gloves, EquipmentType.LeftRing, EquipmentType.RightRing,
                EquipmentType.Belt, EquipmentType.Boots
            };
            
            foreach (EquipmentType equipmentType in allEquipmentTypes)
            {
                ItemData equippedItem = equipmentManager?.GetEquippedItem(equipmentType);
                if (equippedItem == draggedItem)
                {
                    VisualElement equipmentSlot = GetEquipmentSlotElement(equipmentType);
                    if (equipmentSlot != null)
                    {
                        equipmentSlot.AddToClassList("dragging");
                    }
                    break;
                }
            }
        }
    }
    
    private VisualElement GetEquipmentSlotElement(EquipmentType equipmentType)
    {
        switch (equipmentType)
        {
            case EquipmentType.Helmet: return helmetSlotElement;
            case EquipmentType.Amulet: return amuletSlotElement;
            case EquipmentType.MainHand: return mainHandSlotElement;
            case EquipmentType.BodyArmour: return bodyArmourSlotElement;
            case EquipmentType.OffHand: return offHandSlotElement;
            case EquipmentType.Gloves: return glovesSlotElement;
            case EquipmentType.LeftRing: return leftRingSlotElement;
            case EquipmentType.RightRing: return rightRingSlotElement;
            case EquipmentType.Belt: return beltSlotElement;
            case EquipmentType.Boots: return bootsSlotElement;
            default: return null;
        }
    }
    
    private void CreateDragVisual(ItemData item)
    {
        // Create a visual element that follows the mouse
        draggedElement = new VisualElement();
        draggedElement.AddToClassList("drag-visual");
        
        // Set border color using Unity USS properties
        Color rarityColor = GetRarityColor(item.rarity);
        draggedElement.style.borderLeftColor = rarityColor;
        draggedElement.style.borderRightColor = rarityColor;
        draggedElement.style.borderTopColor = rarityColor;
        draggedElement.style.borderBottomColor = rarityColor;
        
        // Add item sprite if available
        if (item.itemSprite != null)
        {
            Image itemImage = new Image();
            itemImage.sprite = item.itemSprite;
            itemImage.style.width = Length.Percent(100);
            itemImage.style.height = Length.Percent(100);
            draggedElement.Add(itemImage);
        }
        
        // Add item name
        Label itemLabel = new Label(item.itemName);
        itemLabel.style.fontSize = 10;
        itemLabel.style.color = GetRarityColor(item.rarity);
        itemLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        draggedElement.Add(itemLabel);
        
        uiDocument.rootVisualElement.Add(draggedElement);
        
        // Register mouse move to update position
        uiDocument.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
    }
    
    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (draggedElement != null)
        {
            draggedElement.style.left = evt.mousePosition.x - 25;
            draggedElement.style.top = evt.mousePosition.y - 25;
        }
    }
    
    private void OnMouseUp(MouseUpEvent evt)
    {
        if (draggedItem == null) return;
        
        // Find the target slot under the mouse
        Vector2 mousePos = evt.mousePosition;
        VisualElement targetSlot = FindSlotUnderMouse(mousePos);
        
        if (targetSlot != null)
        {
            // Handle the drop
            HandleItemDrop(targetSlot);
        }
        else
        {
            // Drop failed, return item to original location
            ReturnItemToOriginalLocation();
        }
        
        // Clean up drag state
        CleanupDragState();
    }
    
    private VisualElement FindSlotUnderMouse(Vector2 mousePos)
    {
        // Check inventory slots
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (IsPointInElement(mousePos, inventorySlots[i]))
            {
                return inventorySlots[i];
            }
        }
        
        // Check stash slots
        for (int i = 0; i < stashSlots.Count; i++)
        {
            if (IsPointInElement(mousePos, stashSlots[i]))
            {
                return stashSlots[i];
            }
        }
        
        // Check equipment slots
        VisualElement[] equipmentSlots = {
            helmetSlotElement, amuletSlotElement, mainHandSlotElement, bodyArmourSlotElement,
            offHandSlotElement, glovesSlotElement, leftRingSlotElement, rightRingSlotElement,
            beltSlotElement, bootsSlotElement
        };
        
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            if (IsPointInElement(mousePos, equipmentSlots[i]))
            {
                return equipmentSlots[i];
            }
        }
        
        return null;
    }
    
    private bool IsPointInElement(Vector2 point, VisualElement element)
    {
        if (element == null) return false;
        
        Rect elementRect = element.worldBound;
        return elementRect.Contains(point);
    }
    
    private void HandleItemDrop(VisualElement targetSlot)
    {
        if (draggedItem == null) return;
        
        // Determine the target type and index
        string targetType = "";
        int targetIndex = -1;
        EquipmentType targetEquipmentType = EquipmentType.MainHand;
        
        // Check if it's an inventory slot
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] == targetSlot)
            {
                targetType = "inventory";
                targetIndex = i;
                break;
            }
        }
        
        // Check if it's a stash slot
        for (int i = 0; i < stashSlots.Count; i++)
        {
            if (stashSlots[i] == targetSlot)
            {
                targetType = "stash";
                targetIndex = i;
                break;
            }
        }
        
        // Check if it's an equipment slot
        if (helmetSlotElement == targetSlot) { targetType = "equipment"; targetEquipmentType = EquipmentType.Helmet; }
        else if (amuletSlotElement == targetSlot) { targetType = "equipment"; targetEquipmentType = EquipmentType.Amulet; }
        else if (mainHandSlotElement == targetSlot) { targetType = "equipment"; targetEquipmentType = EquipmentType.MainHand; }
        else if (bodyArmourSlotElement == targetSlot) { targetType = "equipment"; targetEquipmentType = EquipmentType.BodyArmour; }
        else if (offHandSlotElement == targetSlot) { targetType = "equipment"; targetEquipmentType = EquipmentType.OffHand; }
        else if (glovesSlotElement == targetSlot) { targetType = "equipment"; targetEquipmentType = EquipmentType.Gloves; }
        else if (leftRingSlotElement == targetSlot) { targetType = "equipment"; targetEquipmentType = EquipmentType.LeftRing; }
        else if (rightRingSlotElement == targetSlot) { targetType = "equipment"; targetEquipmentType = EquipmentType.RightRing; }
        else if (beltSlotElement == targetSlot) { targetType = "equipment"; targetEquipmentType = EquipmentType.Belt; }
        else if (bootsSlotElement == targetSlot) { targetType = "equipment"; targetEquipmentType = EquipmentType.Boots; }
        
        // Perform the drop operation
        if (targetType == "inventory")
        {
            DropToInventory(targetIndex);
        }
        else if (targetType == "stash")
        {
            DropToStash(targetIndex);
        }
        else if (targetType == "equipment")
        {
            DropToEquipment(targetEquipmentType);
        }
    }
    
    private void DropToInventory(int targetIndex)
    {
        // Remove item from original location
        RemoveItemFromOriginalLocation();
        
        // Add to inventory at target position
        if (targetIndex < inventoryItems.Count)
        {
            inventoryItems.Insert(targetIndex, draggedItem);
        }
        else
        {
            inventoryItems.Add(draggedItem);
        }
        
        UpdateInventoryDisplay();
        Debug.Log($"Dropped {draggedItem.itemName} to inventory at position {targetIndex}");
    }
    
    private void DropToStash(int targetIndex)
    {
        // Remove item from original location
        RemoveItemFromOriginalLocation();
        
        // Add to current stash tab
        if (currentStashTabIndex >= 0 && currentStashTabIndex < stashTabsList.Count)
        {
            StashTab currentTab = stashTabsList[currentStashTabIndex];
            if (targetIndex < currentTab.items.Count)
            {
                currentTab.items.Insert(targetIndex, draggedItem);
            }
            else
            {
                currentTab.items.Add(draggedItem);
            }
        }
        
        UpdateStashDisplay();
        Debug.Log($"Dropped {draggedItem.itemName} to stash at position {targetIndex}");
    }
    
    private void DropToEquipment(EquipmentType targetEquipmentType)
    {
        // Check if the item can be equipped in this slot
        if (draggedItem.equipmentType != targetEquipmentType)
        {
            Debug.Log($"Cannot equip {draggedItem.itemName} in {targetEquipmentType} slot");
            ReturnItemToOriginalLocation();
            return;
        }
        
        // Remove item from original location
        RemoveItemFromOriginalLocation();
        
        // Equip the item
        if (equipmentManager?.EquipItem(draggedItem) == true)
        {
            UpdateEquipmentDisplay();
            Debug.Log($"Equipped {draggedItem.itemName} in {targetEquipmentType} slot");
        }
        else
        {
            // Equip failed, return to original location
            ReturnItemToOriginalLocation();
        }
    }
    
    private void RemoveItemFromOriginalLocation()
    {
        switch (draggedItemSource)
        {
            case "inventory":
                if (draggedItemIndex >= 0 && draggedItemIndex < inventoryItems.Count)
                {
                    inventoryItems.RemoveAt(draggedItemIndex);
                }
                break;
            case "stash":
                if (draggedStashTabIndex >= 0 && draggedStashTabIndex < stashTabsList.Count)
                {
                    StashTab sourceTab = stashTabsList[draggedStashTabIndex];
                    if (draggedItemIndex >= 0 && draggedItemIndex < sourceTab.items.Count)
                    {
                        sourceTab.items.RemoveAt(draggedItemIndex);
                    }
                }
                break;
            case "equipment":
                // Equipment items are handled by EquipmentManager
                break;
        }
    }
    
    private void ReturnItemToOriginalLocation()
    {
        switch (draggedItemSource)
        {
            case "inventory":
                if (draggedItemIndex >= 0 && draggedItemIndex < inventoryItems.Count)
                {
                    inventoryItems.Insert(draggedItemIndex, draggedItem);
                }
                else
                {
                    inventoryItems.Add(draggedItem);
                }
                UpdateInventoryDisplay();
                break;
            case "stash":
                if (draggedStashTabIndex >= 0 && draggedStashTabIndex < stashTabsList.Count)
                {
                    StashTab sourceTab = stashTabsList[draggedStashTabIndex];
                    if (draggedItemIndex >= 0 && draggedItemIndex < sourceTab.items.Count)
                    {
                        sourceTab.items.Insert(draggedItemIndex, draggedItem);
                    }
                    else
                    {
                        sourceTab.items.Add(draggedItem);
                    }
                }
                UpdateStashDisplay();
                break;
            case "equipment":
                // Equipment items are handled by EquipmentManager
                break;
        }
    }
    
    private void CleanupDragState()
    {
        // Remove drag visual
        if (draggedElement != null)
        {
            draggedElement.RemoveFromHierarchy();
            draggedElement = null;
        }
        
        // Remove dragging visual feedback from all slots
        RemoveDraggingVisualFeedback();
        
        // Clear drag state
        draggedItem = null;
        draggedItemIndex = -1;
        draggedItemSource = "";
        draggedStashTabIndex = -1;
        
        // Unregister events
        uiDocument.rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
        uiDocument.rootVisualElement.UnregisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
    }
    
    private void RemoveDraggingVisualFeedback()
    {
        // Remove dragging class from all inventory slots
        foreach (var slot in inventorySlots)
        {
            slot.RemoveFromClassList("dragging");
        }
        
        // Remove dragging class from all stash slots
        foreach (var slot in stashSlots)
        {
            slot.RemoveFromClassList("dragging");
        }
        
        // Remove dragging class from all equipment slots
        VisualElement[] equipmentSlots = {
            helmetSlotElement, amuletSlotElement, mainHandSlotElement, bodyArmourSlotElement,
            offHandSlotElement, glovesSlotElement, leftRingSlotElement, rightRingSlotElement,
            beltSlotElement, bootsSlotElement
        };
        
        foreach (var slot in equipmentSlots)
        {
            if (slot != null)
            {
                slot.RemoveFromClassList("dragging");
            }
        }
    }
    
    private void UpdateEquipmentSlot(VisualElement slotElement, ItemData item)
    {
        // Clear existing content and classes
        slotElement.Clear();
        slotElement.RemoveFromClassList("occupied");
        slotElement.RemoveFromClassList("normal");
        slotElement.RemoveFromClassList("magic");
        slotElement.RemoveFromClassList("rare");
        slotElement.RemoveFromClassList("unique");
        slotElement.RemoveFromClassList("weapon");
        slotElement.RemoveFromClassList("armour");
        slotElement.RemoveFromClassList("accessory");
        
        if (item != null)
        {
            // Add item visual to slot
            slotElement.AddToClassList("occupied");
            slotElement.AddToClassList(item.rarity.ToString().ToLower());
            slotElement.AddToClassList(item.itemType.ToString().ToLower());
            
            // Add item sprite if available
            if (item.itemSprite != null)
            {
                Image itemImage = new Image();
                itemImage.sprite = item.itemSprite;
                itemImage.AddToClassList("equipment-item-image");
                itemImage.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                slotElement.Add(itemImage);
            }
            
            // Add item name label
            Label itemLabel = new Label(item.itemName);
            itemLabel.AddToClassList("equipment-item-label");
            itemLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            itemLabel.style.fontSize = 10;
            itemLabel.style.color = GetRarityColor(item.rarity);
            slotElement.Add(itemLabel);
        }
    }
    
    

    
    private void UpdateInventoryDisplay()
    {
        // Debug: Log the update process
        Debug.Log($"UpdateInventoryDisplay: {inventorySlots.Count} slots, {inventoryItems.Count} items");
        
        // Update visual representation of inventory items
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            // Clear slot content and classes first
            inventorySlots[i].Clear();
            inventorySlots[i].RemoveFromClassList("occupied");
            inventorySlots[i].RemoveFromClassList("common");
            inventorySlots[i].RemoveFromClassList("rare");
            inventorySlots[i].RemoveFromClassList("magic");
            inventorySlots[i].RemoveFromClassList("unique");
            inventorySlots[i].RemoveFromClassList("weapon");
            inventorySlots[i].RemoveFromClassList("armour");
            inventorySlots[i].RemoveFromClassList("accessory");
            inventorySlots[i].RemoveFromClassList("consumable");
            inventorySlots[i].RemoveFromClassList("material");
            
            if (i < inventoryItems.Count && inventoryItems[i] != null)
            {
                // Add item visual to slot
                inventorySlots[i].AddToClassList("occupied");
                inventorySlots[i].AddToClassList(inventoryItems[i].rarity.ToString().ToLower());
                inventorySlots[i].AddToClassList(inventoryItems[i].itemType.ToString().ToLower());
                
                // Add item sprite if available
                if (inventoryItems[i].itemSprite != null)
                {
                    Image itemImage = new Image();
                    itemImage.sprite = inventoryItems[i].itemSprite;
                    itemImage.AddToClassList("item-image");
                    itemImage.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                    inventorySlots[i].Add(itemImage);
                }
                
                // Add item name label
                Label itemLabel = new Label(inventoryItems[i].itemName);
                itemLabel.AddToClassList("inventory-item-label");
                itemLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                itemLabel.style.fontSize = 8;
                itemLabel.style.color = GetRarityColor(inventoryItems[i].rarity);
                inventorySlots[i].Add(itemLabel);
                
                // Debug: Log the first few items being added
                if (i < 3)
                {
                    Debug.Log($"Slot {i}: Added {inventoryItems[i].itemName} ({inventoryItems[i].rarity} {inventoryItems[i].itemType})");
                }
            }
        }
    }
    
    // Event Handlers
    private void OnReturnButtonClicked()
    {
        // Return to previous scene (likely MainGameUI)
        SceneManager.LoadScene("MainGameUI");
    }
    
    private void OnCharacterButtonClicked()
    {
        // Navigate to character screen
        SceneManager.LoadScene("CharacterScreen");
    }
    
    private void OnSkillsButtonClicked()
    {
        // Navigate to skills screen
        SceneManager.LoadScene("SkillsScreen");
    }
    

    
    private void OnSortButtonClicked()
    {
        // Sort inventory items
        SortInventory();
        UpdateInventoryDisplay();
    }
    
    private void OnFilterButtonClicked()
    {
        // Show filter options
        ShowFilterOptions();
    }
    
    private void OnStashSortButtonClicked()
    {
        // Sort current stash tab items
        if (currentStashTabIndex >= 0 && currentStashTabIndex < stashTabsList.Count)
        {
            SortStashTab(currentStashTabIndex);
            UpdateStashDisplay();
        }
    }
    
    private void OnStashFilterButtonClicked()
    {
        // Show stash filter options
        ShowStashFilterOptions();
    }
    
    private void OnAddStashTabClicked()
    {
        // Check if we've reached the maximum number of stash tabs (11)
        if (stashTabsList.Count >= 11)
        {
            Debug.Log("Maximum number of stash tabs (11) reached!");
            return;
        }
        
        // Add new stash tab
        string newTabName = $"Tab {stashTabsList.Count + 1}";
        CreateStashTab(newTabName);
        
        // Switch to the new tab
        SetActiveStashTab(stashTabsList.Count - 1);
        
        // Update add button state
        UpdateAddStashTabButtonState();
    }
    
    private void InitializeCurrencySystem()
    {
        // Load or create currency database
        currencyDatabase = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
        if (currencyDatabase == null)
        {
            currencyDatabase = ScriptableObject.CreateInstance<CurrencyDatabase>();
            currencyDatabase.InitializeDefaultCurrencies();
        }
        
        // Initialize player currencies with some test currencies
        InitializePlayerCurrencies();
        
        // Generate currency grids for all tabs
        GenerateCurrencyGrids();
        
        // Update currency display
        UpdateCurrencyDisplay();
    }
    
    private void InitializePlayerCurrencies()
    {
        playerCurrencies.Clear();
        
        // Add some test currencies
        foreach (var currency in currencyDatabase.currencies)
        {
            CurrencyData playerCurrency = new CurrencyData
            {
                currencyType = currency.currencyType,
                currencyName = currency.currencyName,
                description = currency.description,
                rarity = currency.rarity,
                currencySprite = currency.currencySprite,
                quantity = UnityEngine.Random.Range(0, 5), // Random quantity for testing
                canTargetCards = currency.canTargetCards,
                canTargetEquipment = currency.canTargetEquipment,
                validEquipmentRarities = currency.validEquipmentRarities,
                maxAffixesForTarget = currency.maxAffixesForTarget,
                isCorruption = currency.isCorruption,
                preservesLockedAffixes = currency.preservesLockedAffixes
            };
            
            playerCurrencies.Add(playerCurrency);
        }
    }
    
    private void GenerateCurrencyGrids()
    {
        // Clear all grids and slot lists
        orbsSlots.Clear();
        spiritsSlots.Clear();
        sealsSlots.Clear();
        fragmentsSlots.Clear();
        
        orbsCurrencyGrid.Clear();
        spiritsCurrencyGrid.Clear();
        sealsCurrencyGrid.Clear();
        fragmentsCurrencyGrid.Clear();
        
        // Generate slots for each tab
        GenerateCurrencyGridForTab(orbsCurrencyGrid, orbsSlots, "Orbs");
        GenerateCurrencyGridForTab(spiritsCurrencyGrid, spiritsSlots, "Spirits");
        GenerateCurrencyGridForTab(sealsCurrencyGrid, sealsSlots, "Seals");
        GenerateCurrencyGridForTab(fragmentsCurrencyGrid, fragmentsSlots, "Fragments");
    }
    
    private void GenerateCurrencyGridForTab(VisualElement grid, List<VisualElement> slots, string tabName)
    {
        // Create currency slots (3x3 grid for 9 currencies per tab)
        for (int i = 0; i < 9; i++)
        {
            VisualElement slot = new VisualElement();
            slot.AddToClassList("currency-slot");
            slot.name = $"{tabName}CurrencySlot_{i}";
            
            // Add click event for currency usage
            int index = i;
            slot.RegisterCallback<ClickEvent>(evt => OnCurrencySlotClicked(index, tabName));
            
            // Add hover events for tooltips
            slot.RegisterCallback<MouseEnterEvent>(evt => ShowCurrencyTooltip(index, evt.mousePosition, tabName));
            slot.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
            
            slots.Add(slot);
            grid.Add(slot);
        }
    }
    
    private void SwitchCurrencyTab(string tabName)
    {
        // Update current tab
        currentCurrencyTab = tabName;
        
        // Update button states
        orbsTabButton.RemoveFromClassList("active");
        spiritsTabButton.RemoveFromClassList("active");
        sealsTabButton.RemoveFromClassList("active");
        fragmentsTabButton.RemoveFromClassList("active");
        
        // Hide all tab contents
        orbsTabContent.RemoveFromClassList("active");
        spiritsTabContent.RemoveFromClassList("active");
        sealsTabContent.RemoveFromClassList("active");
        fragmentsTabContent.RemoveFromClassList("active");
        
        // Show selected tab content and activate button
        switch (tabName)
        {
            case "Orbs":
                orbsTabButton.AddToClassList("active");
                orbsTabContent.AddToClassList("active");
                break;
            case "Spirits":
                spiritsTabButton.AddToClassList("active");
                spiritsTabContent.AddToClassList("active");
                break;
            case "Seals":
                sealsTabButton.AddToClassList("active");
                sealsTabContent.AddToClassList("active");
                break;
            case "Fragments":
                fragmentsTabButton.AddToClassList("active");
                fragmentsTabContent.AddToClassList("active");
                break;
        }
        
        // Update currency display for the current tab
        UpdateCurrencyDisplay();
    }
    
    private void UpdateCurrencyDisplay()
    {
        // Get the appropriate slots for the current tab
        List<VisualElement> currentSlots = GetCurrentTabSlots();
        
        Debug.Log($"[Currency] Updating display for tab '{currentCurrencyTab}' with {currentSlots.Count} slots");
        
        for (int i = 0; i < currentSlots.Count; i++)
        {
            VisualElement slot = currentSlots[i];
            
            // Get currency for this specific slot index
            CurrencyData currency = currencyDatabase.GetCurrencyBySlotIndex(i, currentCurrencyTab);
            CurrencyData playerCurrency = GetPlayerCurrencyForSlot(i, currentCurrencyTab);
            
            // Clear slot content
            slot.Clear();
            
            // Always show the currency sprite (placeholder) if available
            if (currency != null && currency.currencySprite != null)
            {
                try
                {
                    Image currencyImage = new Image();
                    
                    // Extract the specific sprite region from the texture atlas
                    Sprite sprite = currency.currencySprite;
                    Texture2D fullTexture = sprite.texture;
                    Rect spriteRect = sprite.rect;
                    
                    // Validate rect bounds
                    if (spriteRect.width > 0 && spriteRect.height > 0 && 
                        spriteRect.x >= 0 && spriteRect.y >= 0 &&
                        spriteRect.x + spriteRect.width <= fullTexture.width &&
                        spriteRect.y + spriteRect.height <= fullTexture.height)
                    {
                        // Create a new texture with just the sprite region
                        Texture2D croppedTexture = new Texture2D((int)spriteRect.width, (int)spriteRect.height, fullTexture.format, false);
                        Color[] pixels = fullTexture.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height);
                        croppedTexture.SetPixels(pixels);
                        croppedTexture.Apply();
                        
                        currencyImage.image = croppedTexture;
                        currencyImage.AddToClassList("currency-image");
                        currencyImage.style.width = 60;
                        currencyImage.style.height = 60;
                        currencyImage.style.alignSelf = Align.Center;
                        slot.Add(currencyImage);
                        
                        Debug.Log($"[Currency] Added sprite for {currency.currencyName} at slot {i} (extracted from atlas)");
                    }
                    else
                    {
                        Debug.LogError($"[Currency] Invalid sprite rect for {currency.currencyName}: {spriteRect} (texture size: {fullTexture.width}x{fullTexture.height})");
                        // Fallback to placeholder
                        Label placeholderLabel = new Label(currency.currencyName);
                        placeholderLabel.style.fontSize = 10;
                        placeholderLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                        placeholderLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
                        slot.Add(placeholderLabel);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Currency] Error extracting sprite for {currency.currencyName}: {ex.Message}");
                    // Fallback to placeholder
                    Label placeholderLabel = new Label(currency.currencyName);
                    placeholderLabel.style.fontSize = 10;
                    placeholderLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    placeholderLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
                    slot.Add(placeholderLabel);
                }
            }
            else
            {
                // Create a placeholder image with currency name
                Label placeholderLabel = new Label(currency != null ? currency.currencyName : $"Slot {i + 1}");
                placeholderLabel.style.fontSize = 10;
                placeholderLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                placeholderLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
                slot.Add(placeholderLabel);
                
                if (currency != null)
                {
                    Debug.LogWarning($"[Currency] No sprite for {currency.currencyName} at slot {i} - showing placeholder");
                }
            }
            
            // Add currency name label
            if (currency != null)
            {
                Label nameLabel = new Label(currency.currencyName);
                nameLabel.AddToClassList("currency-item-label");
                slot.Add(nameLabel);
            }
            
            // Show quantity if player has this currency
            if (playerCurrency != null && playerCurrency.quantity > 0)
            {
                // Add occupied class and rarity class
                slot.AddToClassList("occupied");
                slot.AddToClassList(playerCurrency.rarity.ToString().ToLower());
                
                // Add quantity label
                Label quantityLabel = new Label(playerCurrency.quantity.ToString());
                quantityLabel.AddToClassList("currency-quantity-label");
                slot.Add(quantityLabel);
            }
            else
            {
                // Remove occupied and rarity classes
                slot.RemoveFromClassList("occupied");
                slot.RemoveFromClassList("normal");
                slot.RemoveFromClassList("magic");
                slot.RemoveFromClassList("rare");
                slot.RemoveFromClassList("unique");
            }
        }
    }
    
    private void UpdateAddStashTabButtonState()
    {
        if (addStashTabButton != null)
        {
            // Disable button if we've reached the maximum number of tabs
            if (stashTabsList.Count >= 11)
            {
                addStashTabButton.SetEnabled(false);
                addStashTabButton.tooltip = "Maximum number of stash tabs (11) reached";
            }
            else
            {
                addStashTabButton.SetEnabled(true);
                addStashTabButton.tooltip = "Add new stash tab";
            }
        }
    }
    
    private void OnRenameTabClicked()
    {
        // Show rename dialog for current tab
        ShowRenameTabDialog(currentStashTabIndex);
    }
    
    private void OnStashTabButtonClicked(int tabIndex)
    {
        SetActiveStashTab(tabIndex);
    }
    
    private void OnStashTabRightClicked(int tabIndex, Vector2 position)
    {
        // Show context menu for tab (rename, delete, etc.)
        ShowStashTabContextMenu(tabIndex, position);
    }
    
    private void OnEquipmentSlotClicked(EquipmentType equipmentType)
    {
        // Handle equipment slot click for drag and drop
        Debug.Log($"Equipment slot clicked: {equipmentType}");
        
        // Check if there's an equipped item in this slot
        ItemData equippedItem = equipmentManager?.GetEquippedItem(equipmentType);
        if (equippedItem != null)
        {
            // Start dragging the equipped item
            StartDraggingItem(equippedItem, -1, "equipment", -1);
        }
    }
    
    private void OnInventorySlotClicked(int slotIndex)
    {
        // Handle inventory slot click for drag and drop
        Debug.Log($"Inventory slot clicked: {slotIndex}");
        
        // Check if there's an item in this slot
        if (slotIndex < inventoryItems.Count && inventoryItems[slotIndex] != null)
        {
            // Start dragging the item
            StartDraggingItem(inventoryItems[slotIndex], slotIndex, "inventory", -1);
        }
    }
    
    private void OnStashSlotClicked(int slotIndex)
    {
        // Handle stash slot click for drag and drop
        Debug.Log($"Stash slot clicked: {slotIndex}");
        
        // Check if there's an item in this slot
        if (currentStashTabIndex >= 0 && currentStashTabIndex < stashTabsList.Count)
        {
            StashTab currentTab = stashTabsList[currentStashTabIndex];
            if (slotIndex < currentTab.items.Count && currentTab.items[slotIndex] != null)
            {
                // Start dragging the item
                StartDraggingItem(currentTab.items[slotIndex], slotIndex, "stash", currentStashTabIndex);
            }
        }
    }
    
    private List<VisualElement> GetCurrentTabSlots()
    {
        switch (currentCurrencyTab)
        {
            case "Orbs": return orbsSlots;
            case "Spirits": return spiritsSlots;
            case "Seals": return sealsSlots;
            case "Fragments": return fragmentsSlots;
            default: return orbsSlots;
        }
    }
    
    private List<CurrencyData> GetCurrentTabCurrencies()
    {
        List<CurrencyData> tabCurrencies = new List<CurrencyData>();
        
        switch (currentCurrencyTab)
        {
            case "Orbs":
                // Filter for orb currencies (first 9)
                for (int i = 0; i < Math.Min(9, playerCurrencies.Count); i++)
                {
                    if (IsOrbCurrency(playerCurrencies[i].currencyType))
                        tabCurrencies.Add(playerCurrencies[i]);
                }
                break;
            case "Spirits":
                // Filter for spirit currencies
                foreach (var currency in playerCurrencies)
                {
                    if (IsSpiritCurrency(currency.currencyType))
                        tabCurrencies.Add(currency);
                }
                break;
            case "Seals":
                // Filter for seal currencies
                foreach (var currency in playerCurrencies)
                {
                    if (IsSealCurrency(currency.currencyType))
                        tabCurrencies.Add(currency);
                }
                break;
            case "Fragments":
                // Filter for fragment currencies
                foreach (var currency in playerCurrencies)
                {
                    if (IsFragmentCurrency(currency.currencyType))
                        tabCurrencies.Add(currency);
                }
                break;
        }
        
        return tabCurrencies;
    }
    
    private bool IsOrbCurrency(CurrencyType currencyType)
    {
        return currencyType >= CurrencyType.OrbOfGeneration && currencyType <= CurrencyType.OrbOfAmnesia;
    }
    
    private bool IsSpiritCurrency(CurrencyType currencyType)
    {
        return currencyType >= CurrencyType.FireSpirit && currencyType <= CurrencyType.DivineSpirit;
    }
    
    private bool IsSealCurrency(CurrencyType currencyType)
    {
        return currencyType >= CurrencyType.TranspositionSeal && currencyType <= CurrencyType.EtchingSeal;
    }
    
    private bool IsFragmentCurrency(CurrencyType currencyType)
    {
        return currencyType >= CurrencyType.Fragment1 && currencyType <= CurrencyType.Fragment3;
    }
    
    private CurrencyData GetPlayerCurrencyForSlot(int slotIndex, string tabName)
    {
        // Get the base currency for this slot
        CurrencyData baseCurrency = currencyDatabase.GetCurrencyBySlotIndex(slotIndex, tabName);
        if (baseCurrency == null) return null;
        
        // Find the player's currency with the same type
        return playerCurrencies.Find(c => c.currencyType == baseCurrency.currencyType);
    }
    
    private void OnCurrencySlotClicked(int slotIndex, string tabName = "Orbs")
    {
        // Handle currency slot click (use currency, etc.)
        CurrencyData playerCurrency = GetPlayerCurrencyForSlot(slotIndex, tabName);
        if (playerCurrency != null && playerCurrency.quantity > 0)
        {
            Debug.Log($"Currency clicked: {playerCurrency.currencyName} (Quantity: {playerCurrency.quantity})");
            // TODO: Implement currency usage logic
            // This could open a selection mode to choose target item/card
        }
        else
        {
            CurrencyData baseCurrency = currencyDatabase.GetCurrencyBySlotIndex(slotIndex, tabName);
            if (baseCurrency != null)
            {
                Debug.Log($"Currency slot clicked: {baseCurrency.currencyName} (No quantity available)");
            }
        }
    }
    
    private void ShowCurrencyTooltip(int slotIndex, Vector2 position, string tabName = "Orbs")
    {
        CurrencyData baseCurrency = currencyDatabase.GetCurrencyBySlotIndex(slotIndex, tabName);
        CurrencyData playerCurrency = GetPlayerCurrencyForSlot(slotIndex, tabName);
        
        if (baseCurrency != null)
        {
            // Show tooltip for the base currency (always available)
            ShowCurrencyTooltip(baseCurrency, position, playerCurrency);
        }
    }
    
    private void ShowCurrencyTooltip(CurrencyData currency, Vector2 position, CurrencyData playerCurrency = null)
    {
        tooltipElement.Clear();
        tooltipElement.style.display = DisplayStyle.Flex;
        tooltipElement.style.left = position.x + 10;
        tooltipElement.style.top = position.y - 10;
        
        // Currency name
        Label nameLabel = new Label(currency.currencyName);
        nameLabel.AddToClassList("tooltip-name");
        nameLabel.AddToClassList(currency.rarity.ToString().ToLower());
        tooltipElement.Add(nameLabel);
        
        // Separator
        VisualElement separator = new VisualElement();
        separator.AddToClassList("tooltip-separator");
        tooltipElement.Add(separator);
        
        // Description
        Label descLabel = new Label(currency.description);
        descLabel.AddToClassList("tooltip-stat");
        tooltipElement.Add(descLabel);
        
        // Quantity (show player's quantity if available)
        if (playerCurrency != null && playerCurrency.quantity > 0)
        {
            Label quantityLabel = new Label($"Quantity: {playerCurrency.quantity}");
            quantityLabel.AddToClassList("tooltip-stat");
            tooltipElement.Add(quantityLabel);
        }
        else
        {
            Label quantityLabel = new Label("Quantity: 0");
            quantityLabel.AddToClassList("tooltip-stat");
            quantityLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            tooltipElement.Add(quantityLabel);
        }
        
        // Valid targets
        if (currency.canTargetEquipment)
        {
            Label targetLabel = new Label("Can target equipment");
            targetLabel.AddToClassList("tooltip-stat");
            tooltipElement.Add(targetLabel);
        }
        
        if (currency.canTargetCards)
        {
            Label targetLabel = new Label("Can target cards");
            targetLabel.AddToClassList("tooltip-stat");
            tooltipElement.Add(targetLabel);
        }
    }
    
    private void ShowEquipmentTooltip(EquipmentType equipmentType, Vector2 position)
    {
        // Show tooltip for equipped item
        ItemData equippedItem = GetEquippedItem(equipmentType);
        if (equippedItem != null)
        {
            ShowTooltip(equippedItem, position);
        }
    }
    
    private void ShowInventoryTooltip(int slotIndex, Vector2 position)
    {
        // Show tooltip for inventory item
        if (slotIndex < inventoryItems.Count && inventoryItems[slotIndex] != null)
        {
            ShowTooltip(inventoryItems[slotIndex], position);
        }
    }
    
    private void ShowStashTooltip(int slotIndex, Vector2 position)
    {
        // Show tooltip for stash item
        if (currentStashTabIndex >= 0 && currentStashTabIndex < stashTabsList.Count)
        {
            StashTab currentTab = stashTabsList[currentStashTabIndex];
            if (slotIndex < currentTab.items.Count && currentTab.items[slotIndex] != null)
            {
                ShowTooltip(currentTab.items[slotIndex], position);
            }
        }
    }
    
    private void ShowTooltip(ItemData item, Vector2 position)
    {
        if (tooltipElement == null) return;
        
        // Create tooltip content
        tooltipElement.Clear();
        
        // Item name with rarity color
        Label nameLabel = new Label(item.itemName);
        nameLabel.AddToClassList("tooltip-name");
        nameLabel.AddToClassList(item.rarity.ToString().ToLower());
        tooltipElement.Add(nameLabel);
        
        // Item type
        Label typeLabel = new Label(item.itemType.ToString().ToUpper());
        typeLabel.AddToClassList("tooltip-type");
        tooltipElement.Add(typeLabel);
        
        // Weapon stats section
        if (item.itemType == ItemType.Weapon)
        {
            // Calculate total damage including affixes
            float totalMinDamage = item.GetTotalMinDamage();
            float totalMaxDamage = item.GetTotalMaxDamage();
            int averageDamage = Mathf.CeilToInt((totalMinDamage + totalMaxDamage) / 2f);
            Label damageLabel = new Label($"Base damage: {averageDamage}");
            damageLabel.AddToClassList("tooltip-stat");
            tooltipElement.Add(damageLabel);
            
            // Critical Strike chance
            Label critLabel = new Label($"Critical Strike chance: {item.criticalStrikeChance:F2}%");
            critLabel.AddToClassList("tooltip-stat");
            tooltipElement.Add(critLabel);
            
            // Attack speed
            Label speedLabel = new Label($"Attack speed: {item.attackSpeed:F2}");
            speedLabel.AddToClassList("tooltip-stat");
            tooltipElement.Add(speedLabel);
        }
        
        // Armor stats section
        if (item.itemType == ItemType.Armour)
        {
            // Calculate total defense values including affixes
            float totalArmour = item.GetTotalArmour();
            float totalEvasion = item.GetTotalEvasion();
            float totalEnergyShield = item.GetTotalEnergyShield();
            
            // Display calculated defense values
            if (totalArmour > 0)
            {
                Label armourLabel = new Label($"Base armour: {Mathf.CeilToInt(totalArmour)}");
                armourLabel.AddToClassList("tooltip-stat");
                tooltipElement.Add(armourLabel);
            }
            
            if (totalEvasion > 0)
            {
                Label evasionLabel = new Label($"Base evasion: {Mathf.CeilToInt(totalEvasion)}");
                evasionLabel.AddToClassList("tooltip-stat");
                tooltipElement.Add(evasionLabel);
            }
            
            if (totalEnergyShield > 0)
            {
                Label esLabel = new Label($"Base energy shield: {Mathf.CeilToInt(totalEnergyShield)}");
                esLabel.AddToClassList("tooltip-stat");
                tooltipElement.Add(esLabel);
            }
        }
        
        // Requirements section
        if (item.requiredLevel > 1 || item.requiredStrength > 0 || item.requiredDexterity > 0 || item.requiredIntelligence > 0)
        {
            // Add separator
            VisualElement reqSeparator = new VisualElement();
            reqSeparator.AddToClassList("tooltip-separator");
            tooltipElement.Add(reqSeparator);
            
            Label reqHeader = new Label("Requirements:");
            reqHeader.AddToClassList("tooltip-section-header");
            tooltipElement.Add(reqHeader);
            
            if (item.requiredLevel > 1)
            {
                Label levelReq = new Label($"Level {item.requiredLevel}");
                levelReq.AddToClassList("tooltip-requirement");
                tooltipElement.Add(levelReq);
            }
            
            if (item.requiredStrength > 0)
            {
                Label strReq = new Label($"{item.requiredStrength} STR");
                strReq.AddToClassList("tooltip-requirement");
                tooltipElement.Add(strReq);
            }
            
            if (item.requiredDexterity > 0)
            {
                Label dexReq = new Label($"{item.requiredDexterity} DEX");
                dexReq.AddToClassList("tooltip-requirement");
                tooltipElement.Add(dexReq);
            }
            
            if (item.requiredIntelligence > 0)
            {
                Label intReq = new Label($"{item.requiredIntelligence} INT");
                intReq.AddToClassList("tooltip-requirement");
                tooltipElement.Add(intReq);
            }
        }
        
        // Implicit modifiers section
        if (item.implicitModifiers.Count > 0)
        {
            // Add separator
            VisualElement impSeparator = new VisualElement();
            impSeparator.AddToClassList("tooltip-separator");
            tooltipElement.Add(impSeparator);
            
            foreach (string modifier in item.implicitModifiers)
            {
                Label impLabel = new Label(modifier);
                impLabel.AddToClassList("tooltip-implicit");
                tooltipElement.Add(impLabel);
            }
        }
        
        // Prefix modifiers section
        if (item.prefixModifiers.Count > 0)
        {
            // Add separator
            VisualElement prefixSeparator = new VisualElement();
            prefixSeparator.AddToClassList("tooltip-separator");
            tooltipElement.Add(prefixSeparator);
            
            Label prefixHeader = new Label("Explicit modifiers - Prefixes:");
            prefixHeader.AddToClassList("tooltip-section-header");
            tooltipElement.Add(prefixHeader);
            
            foreach (string modifier in item.prefixModifiers)
            {
                Label prefixLabel = new Label(modifier);
                prefixLabel.AddToClassList("tooltip-prefix");
                tooltipElement.Add(prefixLabel);
            }
        }
        
        // Suffix modifiers section
        if (item.suffixModifiers.Count > 0)
        {
            // Add separator
            VisualElement suffixSeparator = new VisualElement();
            suffixSeparator.AddToClassList("tooltip-separator");
            tooltipElement.Add(suffixSeparator);
            
            Label suffixHeader = new Label("Explicit modifiers - Suffixes:");
            suffixHeader.AddToClassList("tooltip-section-header");
            tooltipElement.Add(suffixHeader);
            
            foreach (string modifier in item.suffixModifiers)
            {
                Label suffixLabel = new Label(modifier);
                suffixLabel.AddToClassList("tooltip-suffix");
                tooltipElement.Add(suffixLabel);
            }
        }
        
        // Position tooltip near mouse but ensure it stays on screen
        float tooltipX = Mathf.Min(position.x + 10, Screen.width - 300);
        float tooltipY = Mathf.Min(position.y + 10, Screen.height - 400);
        
        tooltipElement.style.left = tooltipX;
        tooltipElement.style.top = tooltipY;
        tooltipElement.style.display = DisplayStyle.Flex;
    }
    
    private void HideTooltip()
    {
        if (tooltipElement != null)
        {
            tooltipElement.style.display = DisplayStyle.None;
        }
    }
    
    private ItemData GetEquippedItem(EquipmentType equipmentType)
    {
        return equipmentManager?.GetEquippedItem(equipmentType);
    }
    
    // Helper Methods
    
    // Helper Methods
    private void SortInventory()
    {
        // Sort inventory items by type, rarity, etc.
        inventoryItems.Sort((a, b) => a.rarity.CompareTo(b.rarity));
    }
    
    private void SortStashTab(int tabIndex)
    {
        if (tabIndex >= 0 && tabIndex < stashTabsList.Count)
        {
            stashTabsList[tabIndex].items.Sort((a, b) => a.rarity.CompareTo(b.rarity));
        }
    }
    
    private void ShowFilterOptions()
    {
        // Show filter dialog
        Debug.Log("Show filter options");
    }
    
    private void ShowStashFilterOptions()
    {
        // Show stash filter dialog
        Debug.Log("Show stash filter options");
    }
    
    private void ShowRenameTabDialog(int tabIndex)
    {
        // Show rename dialog
        Debug.Log($"Show rename dialog for tab {tabIndex}");
        // TODO: Implement rename dialog
    }
    
    private void ShowStashTabContextMenu(int tabIndex, Vector2 position)
    {
        // Show context menu
        Debug.Log($"Show context menu for tab {tabIndex} at position {position}");
        // TODO: Implement context menu
    }
    
    private void OnDestroy()
    {
        // Clean up event handlers
        if (returnButton != null) returnButton.clicked -= OnReturnButtonClicked;
        if (characterButton != null) characterButton.clicked -= OnCharacterButtonClicked;
        if (skillsButton != null) skillsButton.clicked -= OnSkillsButtonClicked;
        if (sortButton != null) sortButton.clicked -= OnSortButtonClicked;
        if (filterButton != null) filterButton.clicked -= OnFilterButtonClicked;
        if (stashSortButton != null) stashSortButton.clicked -= OnStashSortButtonClicked;
        if (stashFilterButton != null) stashFilterButton.clicked -= OnStashFilterButtonClicked;
        if (addStashTabButton != null) addStashTabButton.clicked -= OnAddStashTabClicked;
        if (renameTabButton != null) renameTabButton.clicked -= OnRenameTabClicked;
    }
    
            // Helper method to convert Affix list to string list
        private List<string> ConvertAffixesToStrings(List<Affix> affixes)
        {
            List<string> result = new List<string>();
            
            foreach (var affix in affixes)
            {
                string affixText = $"{affix.name}: ";
                List<string> modifierTexts = new List<string>();
                
                foreach (var modifier in affix.modifiers)
                {
                    string modifierText = "";
                    
                    if (modifier.isDualRange)
                    {
                        // Handle dual-range modifiers
                        if (modifier.modifierType == ModifierType.Flat)
                        {
                            if (modifier.damageType != DamageType.None)
                            {
                                modifierText = $"Adds {(int)modifier.rolledFirstValue} to {(int)modifier.rolledSecondValue} {modifier.damageType} Damage";
                            }
                            else
                            {
                                modifierText = $"Adds {(int)modifier.rolledFirstValue} to {(int)modifier.rolledSecondValue} {modifier.statName}";
                            }
                        }
                        else
                        {
                            // For non-flat dual-range modifiers (if any exist)
                            modifierText = $"{(int)modifier.rolledFirstValue} to {(int)modifier.rolledSecondValue} {modifier.statName}";
                        }
                    }
                    else
                    {
                        // Handle single-range modifiers (existing logic)
                        if (modifier.modifierType == ModifierType.Flat)
                        {
                            if (modifier.damageType != DamageType.None)
                            {
                                modifierText = $"+{(int)modifier.minValue} {modifier.damageType} Damage";
                            }
                            else
                            {
                                modifierText = $"+{(int)modifier.minValue} {modifier.statName}";
                            }
                        }
                        else if (modifier.modifierType == ModifierType.Increased)
                        {
                            modifierText = $"+{(int)modifier.minValue}% increased {modifier.statName}";
                        }
                        else if (modifier.modifierType == ModifierType.More)
                        {
                            modifierText = $"{(int)modifier.minValue}% more {modifier.statName}";
                        }
                        else if (modifier.modifierType == ModifierType.Reduced)
                        {
                            modifierText = $"-{(int)modifier.minValue}% reduced {modifier.statName}";
                        }
                        else if (modifier.modifierType == ModifierType.Less)
                        {
                            modifierText = $"{(int)modifier.minValue}% less {modifier.statName}";
                        }
                    }
                    
                    // Add scope indicator
                    string scopeIndicator = modifier.scope == ModifierScope.Local ? "🔧" : "🌍";
                    modifierText = $"{scopeIndicator} {modifierText}";
                    
                    modifierTexts.Add(modifierText);
                }
                
                affixText += string.Join(", ", modifierTexts);
                result.Add(affixText);
            }
            
            return result;
        }
        
        // Helper method to convert Affix lists to stats dictionary
        private Dictionary<string, float> ConvertAffixesToStats(List<Affix> implicitModifiers, List<Affix> prefixes, List<Affix> suffixes)
        {
            Dictionary<string, float> stats = new Dictionary<string, float>();
            
            // Process all affix lists
            ProcessAffixList(implicitModifiers, stats);
            ProcessAffixList(prefixes, stats);
            ProcessAffixList(suffixes, stats);
            
            return stats;
        }
        
        // Helper method to process a single affix list
        private void ProcessAffixList(List<Affix> affixes, Dictionary<string, float> stats)
        {
            foreach (var affix in affixes)
            {
                foreach (var modifier in affix.modifiers)
                {
                    string statName = modifier.statName;
                    
                    // Handle damage type modifiers
                    if (modifier.damageType != DamageType.None)
                    {
                        if (modifier.modifierType == ModifierType.Flat)
                        {
                            // For flat damage, use the damage type as the stat name
                            statName = $"{modifier.damageType}Damage";
                        }
                        else if (modifier.modifierType == ModifierType.Increased)
                        {
                            // For increased damage, use "Increased" + damage type
                            statName = $"Increased{modifier.damageType}Damage";
                        }
                    }
                    
                    // Add or accumulate the value
                    float value = modifier.minValue; // Use minValue as the rolled value
                    if (stats.ContainsKey(statName))
                    {
                        stats[statName] += value;
                    }
                    else
                    {
                        stats[statName] = value;
                    }
                }
            }
        }
    
    // Debug method to test grid visibility
    [ContextMenu("Test Grid Visibility")]
    public void TestGridVisibility()
    {
        Debug.Log("Testing grid visibility...");
        Debug.Log($"Inventory slots count: {inventorySlots.Count}");
        Debug.Log($"Inventory items count: {inventoryItems.Count}");
        
        // Force add a test item if none exist
        if (inventoryItems.Count == 0)
        {
            inventoryItems.Add(new ItemData { 
                itemName = "Test Steel Axe", 
                itemType = ItemType.Weapon, 
                rarity = ItemRarity.Normal,
                itemSprite = null,
                baseDamageMin = 10,
                baseDamageMax = 18,
                criticalStrikeChance = 7.0f,
                attackSpeed = 1.15f,
                requiredLevel = 10,
                requiredStrength = 22,
                requiredDexterity = 0,
                requiredIntelligence = 0,
                implicitModifiers = new List<string>(),
                prefixModifiers = new List<string>(),
                suffixModifiers = new List<string>()
            });
            Debug.Log("Added test Steel Axe to inventory");
        }
        
        // Force update display
        UpdateInventoryDisplay();
        
        // Log the first few slots
        for (int i = 0; i < Math.Min(5, inventorySlots.Count); i++)
        {
            Debug.Log($"Slot {i}: Classes = {string.Join(", ", inventorySlots[i].GetClasses())}");
        }
    }
    
    /// <summary>
    /// Converts ArmourSlot to EquipmentType for proper equipment mapping
    /// </summary>
    private EquipmentType ConvertArmourSlotToEquipmentType(ArmourSlot armourSlot)
    {
        switch (armourSlot)
        {
            case ArmourSlot.Helmet:
                return EquipmentType.Helmet;
            case ArmourSlot.BodyArmour:
                return EquipmentType.BodyArmour;
            case ArmourSlot.Gloves:
                return EquipmentType.Gloves;
            case ArmourSlot.Boots:
                return EquipmentType.Boots;
            case ArmourSlot.Shield:
                return EquipmentType.OffHand;
            default:
                return EquipmentType.BodyArmour; // Fallback
        }
    }
}

// Data structures for equipment system
[System.Serializable]
public class EquipmentData
{
    public ItemData helmet;
    public ItemData amulet;
    public ItemData mainHand;
    public ItemData bodyArmour;
    public ItemData offHand;
    public ItemData gloves;
    public ItemData leftRing;
    public ItemData rightRing;
    public ItemData belt;
    public ItemData boots;
}

[System.Serializable]
public class ItemData
{
    public string itemName;
    public ItemType itemType;
    public EquipmentType equipmentType;
    public ItemRarity rarity;
    public int level;
    public Sprite itemSprite;
    public Dictionary<string, float> stats = new Dictionary<string, float>();
    
    // Weapon-specific stats
    public float baseDamageMin;
    public float baseDamageMax;
    public float criticalStrikeChance;
    public float attackSpeed;
    
    // Armor stats
    public float baseArmour;
    public float baseEvasion;
    public float baseEnergyShield;
    
    // Requirements
    public int requiredLevel;
    public int requiredStrength;
    public int requiredDexterity;
    public int requiredIntelligence;
    
    // Modifiers
    public List<string> implicitModifiers = new List<string>();
    public List<string> prefixModifiers = new List<string>();
    public List<string> suffixModifiers = new List<string>();
    
    // Damage calculation methods
    public float GetTotalMinDamage()
    {
        if (itemType != ItemType.Weapon) return 0f;
        return CalculateTotalDamage(baseDamageMin);
    }
    
    public float GetTotalMaxDamage()
    {
        if (itemType != ItemType.Weapon) return 0f;
        return CalculateTotalDamage(baseDamageMax);
    }
    
    public float GetCalculatedTotalDamage()
    {
        if (itemType != ItemType.Weapon) return 0f;
        return (GetTotalMinDamage() + GetTotalMaxDamage()) / 2f;
    }
    
    // Calculate total damage using PoE-style formula: (Base + Added) * (1 + Increased)
    private float CalculateTotalDamage(float baseDamage)
    {
        // Start with base damage
        float totalDamage = baseDamage;
        
        // Add flat damage from modifiers (simplified calculation)
        float addedDamage = GetModifierValue("PhysicalDamage") + GetModifierValue("FireDamage") + 
                           GetModifierValue("ColdDamage") + GetModifierValue("LightningDamage") + 
                           GetModifierValue("ChaosDamage");
        totalDamage += addedDamage;
        
        // Apply increased damage multipliers
        float increasedDamage = GetModifierValue("IncreasedPhysicalDamage") + GetModifierValue("IncreasedFireDamage") + 
                               GetModifierValue("IncreasedColdDamage") + GetModifierValue("IncreasedLightningDamage") + 
                               GetModifierValue("IncreasedChaosDamage");
        totalDamage *= (1f + increasedDamage / 100f); // Convert percentage to multiplier
        
        // Round up to nearest whole number
        return Mathf.Ceil(totalDamage);
    }
    
    // Get modifier value from stats dictionary
    private float GetModifierValue(string statName)
    {
        return stats.ContainsKey(statName) ? stats[statName] : 0f;
    }
    
    // Defense calculation methods for armor items
    public float GetTotalArmour()
    {
        if (itemType != ItemType.Armour) return 0f;
        return CalculateTotalDefense("Armour", baseArmour);
    }
    
    public float GetTotalEvasion()
    {
        if (itemType != ItemType.Armour) return 0f;
        return CalculateTotalDefense("Evasion", baseEvasion);
    }
    
    public float GetTotalEnergyShield()
    {
        if (itemType != ItemType.Armour) return 0f;
        return CalculateTotalDefense("EnergyShield", baseEnergyShield);
    }
    
    // Calculate total defense using PoE-style formula: (Base + Added) * (1 + Increased)
    private float CalculateTotalDefense(string defenseType, float baseDefense)
    {
        // Start with base defense
        float totalDefense = baseDefense;
        
        // Add flat defense from modifiers
        float addedDefense = GetModifierValue(defenseType);
        totalDefense += addedDefense;
        
        // Apply increased defense multipliers
        float increasedDefense = GetModifierValue($"Increased{defenseType}");
        totalDefense *= (1f + increasedDefense / 100f); // Convert percentage to multiplier
        
        // Round up to nearest whole number
        return Mathf.Ceil(totalDefense);
    }
}

[System.Serializable]
public class StashTab
{
    public string tabName;
    public List<ItemData> items = new List<ItemData>();
    public bool isLocked = false;
    public string customColor = "";
}

public enum EquipmentType
{
    Helmet,
    Amulet,
    MainHand,
    BodyArmour,
    OffHand,
    Gloves,
    LeftRing,
    RightRing,
    Belt,
    Boots
}

public enum ItemType
{
    Weapon,
    Armour,
    Accessory,
    Consumable,
    Material
}

// ItemRarity enum moved to ItemRarity.cs

[System.Serializable]
public class EquipmentSlot
{
    public EquipmentType slotType;
    public ItemData equippedItem;
    public bool isOccupied => equippedItem != null;
}
