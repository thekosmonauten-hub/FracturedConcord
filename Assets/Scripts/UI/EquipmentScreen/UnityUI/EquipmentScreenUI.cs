using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Unity UI version of Equipment Screen main controller
/// Manages all equipment screen UI elements and interactions
/// </summary>
public class EquipmentScreenUI : MonoBehaviour
{
    [Header("Equipment Slots")]
    [SerializeField] private EquipmentSlotUI helmetSlot;
    [SerializeField] private EquipmentSlotUI amuletSlot;
    [SerializeField] private EquipmentSlotUI mainHandSlot;
    [SerializeField] private EquipmentSlotUI bodyArmourSlot;
    [SerializeField] private EquipmentSlotUI offHandSlot;
    [SerializeField] private EquipmentSlotUI glovesSlot;
    [SerializeField] private EquipmentSlotUI leftRingSlot;
    [SerializeField] private EquipmentSlotUI rightRingSlot;
    [SerializeField] private EquipmentSlotUI beltSlot;
    [SerializeField] private EquipmentSlotUI bootsSlot;
    
    [Header("Inventory")]
    [SerializeField] private InventoryGridUI inventoryGrid;
    [SerializeField] private InventoryGridUI stashGrid;
    
    [Header("Buttons")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Button sortButton;
    [SerializeField] private Button filterButton;
    [SerializeField] private Button stashSortButton;
    [SerializeField] private Button stashFilterButton;
    
    [Header("Effigy System")]
    [SerializeField] private EffigyGridUI effigyGrid;
    [SerializeField] private EffigyStoragePanel effigyStoragePanel;
    [SerializeField] private Button openEffigyStorageButton;
    
    [Header("Currency Tabs")]
    [SerializeField] private Button orbsTabButton;
    [SerializeField] private Button spiritsTabButton;
    [SerializeField] private Button sealsTabButton;
    [SerializeField] private Button fragmentsTabButton;
    [SerializeField] private GameObject orbsTabContent;
    [SerializeField] private GameObject spiritsTabContent;
    [SerializeField] private GameObject sealsTabContent;
    [SerializeField] private GameObject fragmentsTabContent;
    
    private Dictionary<EquipmentType, EquipmentSlotUI> slotMap;
    private string currentCurrencyTab = "Orbs";
    
    void Awake()
    {
        InitializeSlotMap();
        SetupEventListeners();
    }
    
    void Start()
    {
        // Set initial tab state
        SetCurrencyTab("Orbs");
    }
    
    void InitializeSlotMap()
    {
        slotMap = new Dictionary<EquipmentType, EquipmentSlotUI>
        {
            { EquipmentType.Helmet, helmetSlot },
            { EquipmentType.Amulet, amuletSlot },
            { EquipmentType.MainHand, mainHandSlot },
            { EquipmentType.BodyArmour, bodyArmourSlot },
            { EquipmentType.OffHand, offHandSlot },
            { EquipmentType.Gloves, glovesSlot },
            { EquipmentType.LeftRing, leftRingSlot },
            { EquipmentType.RightRing, rightRingSlot },
            { EquipmentType.Belt, beltSlot },
            { EquipmentType.Boots, bootsSlot }
        };
        
        // Initialize each slot
        foreach (var kvp in slotMap)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Initialize(kvp.Key, kvp.Key.ToString().ToUpper());
                kvp.Value.OnSlotClicked += OnEquipmentSlotClicked;
                kvp.Value.OnSlotHovered += ShowEquipmentTooltip;
            }
        }
    }
    
    void SetupEventListeners()
    {
        if (returnButton != null)
            returnButton.onClick.AddListener(OnReturnButtonClicked);
        
        if (sortButton != null)
            sortButton.onClick.AddListener(OnSortButtonClicked);
        
        if (filterButton != null)
            filterButton.onClick.AddListener(OnFilterButtonClicked);
        
        if (stashSortButton != null)
            stashSortButton.onClick.AddListener(OnStashSortButtonClicked);
        
        if (stashFilterButton != null)
            stashFilterButton.onClick.AddListener(OnStashFilterButtonClicked);
        
        if (openEffigyStorageButton != null)
            openEffigyStorageButton.onClick.AddListener(OnOpenEffigyStorage);
        
        // Currency tabs
        if (orbsTabButton != null)
            orbsTabButton.onClick.AddListener(() => SetCurrencyTab("Orbs"));
        
        if (spiritsTabButton != null)
            spiritsTabButton.onClick.AddListener(() => SetCurrencyTab("Spirits"));
        
        if (sealsTabButton != null)
            sealsTabButton.onClick.AddListener(() => SetCurrencyTab("Seals"));
        
        if (fragmentsTabButton != null)
            fragmentsTabButton.onClick.AddListener(() => SetCurrencyTab("Fragments"));
    }
    
    void OnEquipmentSlotClicked(EquipmentType slotType)
    {
        Debug.Log($"[EquipmentScreenUI] Equipment slot clicked: {slotType}");
        // TODO: Handle equipment slot click (equip/unequip)
    }
    
    void ShowEquipmentTooltip(EquipmentType slotType, Vector2 position)
    {
        Debug.Log($"[EquipmentScreenUI] Show tooltip for: {slotType}");
        // TODO: Show tooltip for equipped item
    }
    
    void OnReturnButtonClicked()
    {
        Debug.Log("[EquipmentScreenUI] Return button clicked");
        // Return to game
        gameObject.SetActive(false);
    }
    
    void OnSortButtonClicked()
    {
        Debug.Log("[EquipmentScreenUI] Sort inventory");
        if (inventoryGrid != null)
            inventoryGrid.SortInventory();
    }
    
    void OnFilterButtonClicked()
    {
        Debug.Log("[EquipmentScreenUI] Filter inventory");
        // TODO: Implement filter UI
    }
    
    void OnStashSortButtonClicked()
    {
        Debug.Log("[EquipmentScreenUI] Sort stash");
        if (stashGrid != null)
            stashGrid.SortInventory();
    }
    
    void OnStashFilterButtonClicked()
    {
        Debug.Log("[EquipmentScreenUI] Filter stash");
        // TODO: Implement stash filter UI
    }
    
    void OnOpenEffigyStorage()
    {
        Debug.Log("[EquipmentScreenUI] Open effigy storage");
        if (effigyStoragePanel != null)
            effigyStoragePanel.SlideIn();
    }
    
    void SetCurrencyTab(string tabName)
    {
        currentCurrencyTab = tabName;
        
        // Show/hide tab contents
        if (orbsTabContent != null)
            orbsTabContent.SetActive(tabName == "Orbs");
        
        if (spiritsTabContent != null)
            spiritsTabContent.SetActive(tabName == "Spirits");
        
        if (sealsTabContent != null)
            sealsTabContent.SetActive(tabName == "Seals");
        
        if (fragmentsTabContent != null)
            fragmentsTabContent.SetActive(tabName == "Fragments");
        
        // Update button states (colors)
        UpdateTabButtonStates(tabName);
    }
    
    void UpdateTabButtonStates(string activeTab)
    {
        UpdateButtonState(orbsTabButton, activeTab == "Orbs");
        UpdateButtonState(spiritsTabButton, activeTab == "Spirits");
        UpdateButtonState(sealsTabButton, activeTab == "Seals");
        UpdateButtonState(fragmentsTabButton, activeTab == "Fragments");
    }
    
    void UpdateButtonState(Button button, bool isActive)
    {
        if (button == null) return;
        
        ColorBlock colors = button.colors;
        colors.normalColor = isActive ? new Color(0.4f, 0.6f, 1f) : new Color(0.3f, 0.3f, 0.3f);
        button.colors = colors;
    }
    
    public void EquipItem(ItemData item, EquipmentType slotType)
    {
        if (slotMap.ContainsKey(slotType) && slotMap[slotType] != null)
        {
            slotMap[slotType].SetEquippedItem(item);
            Debug.Log($"[EquipmentScreenUI] Equipped {item.itemName} to {slotType}");
        }
    }
    
    public void UnequipItem(EquipmentType slotType)
    {
        if (slotMap.ContainsKey(slotType) && slotMap[slotType] != null)
        {
            slotMap[slotType].SetEquippedItem(null);
            Debug.Log($"[EquipmentScreenUI] Unequipped item from {slotType}");
        }
    }
    
    public ItemData GetEquippedItem(EquipmentType slotType)
    {
        if (slotMap.ContainsKey(slotType) && slotMap[slotType] != null)
        {
            return slotMap[slotType].GetEquippedItem();
        }
        return null;
    }
}

