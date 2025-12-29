using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Dexiled.UI.EquipmentScreen;
using TMPro;

/// <summary>
/// Unity UI version of Equipment Screen main controller
/// Manages all equipment screen UI elements and interactions
/// </summary>
public class EquipmentScreenUI : MonoBehaviour
{
    [Header("Player Details")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private TextMeshProUGUI playerClassText;
    
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
    
    [Header("Currency")]
    [SerializeField] private CurrencyManager currencyManager;
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
        // Defer heavy initialization to spread load across frames
        StartCoroutine(DeferredInitialization());
    }
    
    /// <summary>
    /// Spread initialization across multiple frames to prevent freezing
    /// </summary>
    private System.Collections.IEnumerator DeferredInitialization()
    {
        // Critical: Set initial tab state (lightweight)
        SetCurrencyTab("Orbs");
        yield return null; // Wait one frame
        
        // Update player details (lightweight)
        UpdatePlayerDetails();
        yield return null; // Wait one frame
        
        // Configure grids (may be heavier)
        ConfigureGrids();
        yield return null; // Wait one frame
        
        // Refresh inventory grid (potentially heavy)
        if (inventoryGrid != null)
        {
            inventoryGrid.RefreshFromDataSource();
            yield return null; // Wait one frame
        }
        
        // Refresh stash grid (potentially heavy)
        if (stashGrid != null)
        {
            stashGrid.RefreshFromDataSource();
            yield return null; // Wait one frame
        }
        
        // Refresh equipment slots (lightweight)
        RefreshEquipmentSlots();
        yield return null; // Wait one frame
        
        // Refresh currency display (may load from resources)
        RefreshCurrencyDisplay();
    }
    
    /// <summary>
    /// Configure inventory and stash grids with correct modes
    /// </summary>
    private void ConfigureGrids()
    {
        if (inventoryGrid != null)
        {
            inventoryGrid.gridMode = InventoryGridUI.GridMode.CharacterInventory;
            Debug.Log("[EquipmentScreenUI] Configured inventoryGrid as CharacterInventory");
        }
        
        if (stashGrid != null)
        {
            stashGrid.gridMode = InventoryGridUI.GridMode.GlobalStash;
            Debug.Log("[EquipmentScreenUI] Configured stashGrid as GlobalStash");
        }
    }
    
    void OnEnable()
    {
        // Update player details when screen is shown
        UpdatePlayerDetails();
        
        // Refresh currency display from LootManager
        RefreshCurrencyDisplay();
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
        Debug.Log($"<color=cyan>[EquipmentScreenUI] Equipment slot clicked: {slotType}</color>");
        
        var selectionManager = ItemSelectionManager.Instance;
        if (selectionManager == null)
        {
            Debug.LogWarning("[EquipmentScreenUI] ItemSelectionManager not found!");
            return;
        }
        
        // Get the equipment manager
        var equipmentManager = EquipmentManager.Instance;
        if (equipmentManager == null)
        {
            Debug.LogWarning("[EquipmentScreenUI] EquipmentManager not found!");
            return;
        }
        
        bool hasSelection = selectionManager.HasSelection();
        Debug.Log($"<color=cyan>[EquipmentScreenUI] Has selection: {hasSelection}</color>");
        
        // First, check if this slot has an equipped item and no selection
        // If so, show the equipped tooltip
        if (!hasSelection)
        {
            if (slotMap.TryGetValue(slotType, out var slot) && slot != null)
            {
                BaseItem equippedItem = slot.GetEquippedItem();
                Debug.Log($"<color=cyan>[EquipmentScreenUI] Slot {slotType} equipped item: {(equippedItem != null ? equippedItem.itemName : "NULL")}</color>");
                
                if (equippedItem != null)
                {
                    Debug.Log($"<color=lime>[EquipmentScreenUI] ✅ Showing equipped tooltip for {equippedItem.itemName}</color>");
                    // Show equipped tooltip in the fixed container
                    ShowEquippedItemTooltip(equippedItem, slotType);
                    return;
                }
                else
                {
                    Debug.Log($"<color=yellow>[EquipmentScreenUI] No item equipped in {slotType}, nothing to show</color>");
                }
            }
            else
            {
                Debug.LogWarning($"<color=red>[EquipmentScreenUI] Slot {slotType} not found in slotMap!</color>");
            }
        }
        else
        {
            Debug.Log($"<color=cyan>[EquipmentScreenUI] Has selection, proceeding with equip/unequip logic</color>");
        }
        
        // Check if we have a selected item
        if (selectionManager.HasSelection())
        {
            BaseItem selectedItem = selectionManager.GetSelectedItem();
            
            // Check if item can go in this slot
            if (selectionManager.CanEquipToSlot(selectedItem, slotType))
            {
                // Equip the item (pass target slot for 1-handed weapons)
                bool success = equipmentManager.EquipItem(selectedItem, slotType);
                    
                if (success)
                {
                    Debug.Log($"[EquipmentScreenUI] Successfully equipped {selectedItem.itemName} to {slotType}");
                        
                    // Remove from inventory if it was from inventory
                    if (selectionManager.IsFromInventory())
                    {
                        int inventoryIndex = selectionManager.GetSelectedInventoryIndex();
                        var charManager = CharacterManager.Instance;
                        if (charManager != null && inventoryIndex >= 0 && inventoryIndex < charManager.inventoryItems.Count)
                        {
                            charManager.inventoryItems.RemoveAt(inventoryIndex);
                        }
                    }
                    
                    // Clear selection and refresh
                    selectionManager.ClearSelection();
                    RefreshAllDisplays();
                }
                else
                {
                    Debug.LogWarning($"[EquipmentScreenUI] Failed to equip {selectedItem.itemName}");
                }
            }
            else
            {
                Debug.LogWarning($"[EquipmentScreenUI] {selectedItem.GetDisplayName()} cannot be equipped in {slotType} slot!");
            }
        }
        else
        {
            // No selection - unequip item in this slot
            if (!slotMap.TryGetValue(slotType, out var slot) || slot == null)
                return;
            
            BaseItem equippedItem = slot.GetEquippedItem();
            if (equippedItem)
            {
                // Unequip and move to inventory
                BaseItem unequippedItem = equipmentManager.UnequipItem(slotType);
                
                if (unequippedItem != null)
                {
                    // Special case: If MainHand was unequipped, OffHand weapon may have been moved to MainHand
                    // In that case, unequippedItem is the MainHand item, and OffHand is now empty
                    // We need to refresh to show the moved weapon in MainHand
                    
                    // Add to inventory
                    var charManager = CharacterManager.Instance;
                    if (charManager != null)
                    {
                        charManager.inventoryItems.Add(unequippedItem);
                        charManager.OnItemAdded?.Invoke(unequippedItem);
                    }
                    
                    Debug.Log($"[EquipmentScreenUI] Unequipped {unequippedItem.itemName} from {slotType}");
                    RefreshAllDisplays();
                }
            }
        }
    }
    
    private void RefreshAllDisplays()
    {
        // Refresh inventory grid (character-specific)
        if (inventoryGrid != null)
        {
            inventoryGrid.RefreshFromDataSource();
        }
        
        // Refresh stash grid (global)
        if (stashGrid != null)
        {
            stashGrid.RefreshFromDataSource();
        }
        
        // Refresh equipment slots
        RefreshEquipmentSlots();
        
        // Update player details
        UpdatePlayerDetails();
        
        // Refresh currency display
        RefreshCurrencyDisplay();
    }
    
    /// <summary>
    /// Refresh currency display from LootManager
    /// </summary>
    private void RefreshCurrencyDisplay()
    {
        if (currencyManager != null)
        {
            currencyManager.Refresh(); // This will call SyncFromLootManager() internally
            Debug.Log("[EquipmentScreenUI] Currency display refreshed from LootManager");
        }
        else
        {
            Debug.LogWarning("[EquipmentScreenUI] CurrencyManager not assigned! Currencies will not display.");
        }
    }
    
    /// <summary>
    /// Transfer item from inventory to stash
    /// </summary>
    public void TransferToStash(BaseItem item, int inventoryIndex)
    {
        if (item == null || StashManager.Instance == null || CharacterManager.Instance == null)
        {
            Debug.LogWarning("[EquipmentScreenUI] Cannot transfer item to stash - missing managers or item");
            return;
        }
        
        // Remove from inventory
        if (inventoryIndex >= 0 && inventoryIndex < CharacterManager.Instance.inventoryItems.Count)
        {
            CharacterManager.Instance.inventoryItems.RemoveAt(inventoryIndex);
        }
        
        // Add to stash
        StashManager.Instance.AddItem(item);
        
        Debug.Log($"[EquipmentScreenUI] Transferred {item.GetDisplayName()} from inventory to stash");
        RefreshAllDisplays();
    }
    
    /// <summary>
    /// Transfer item from stash to inventory
    /// </summary>
    public void TransferToInventory(BaseItem item, int stashIndex)
    {
        if (item == null || StashManager.Instance == null || CharacterManager.Instance == null)
        {
            Debug.LogWarning("[EquipmentScreenUI] Cannot transfer item to inventory - missing managers or item");
            return;
        }
        
        // Remove from stash
        if (StashManager.Instance.RemoveItemAt(stashIndex))
        {
            // Add to inventory
            CharacterManager.Instance.AddItem(item);
            
            Debug.Log($"[EquipmentScreenUI] Transferred {item.GetDisplayName()} from stash to inventory");
            RefreshAllDisplays();
        }
    }
    
    /// <summary>
    /// Update player name, level, and class from CharacterManager
    /// </summary>
    private void UpdatePlayerDetails()
    {
        var characterManager = CharacterManager.Instance;
        if (characterManager == null || characterManager.currentCharacter == null)
        {
            Debug.LogWarning("[EquipmentScreenUI] CharacterManager or currentCharacter not found!");
            
            // Set default values if no character
            if (playerNameText != null)
                playerNameText.text = "No Character";
            
            if (playerLevelText != null)
                playerLevelText.text = "Level: -";
            
            if (playerClassText != null)
                playerClassText.text = "-";
            
            return;
        }
        
        Character character = characterManager.currentCharacter;
        
        // Update player name
        if (playerNameText != null)
        {
            playerNameText.text = character.characterName;
        }
        
        // Update player level
        if (playerLevelText != null)
        {
            playerLevelText.text = $"Level: {character.level}";
        }
        
        // Update player class
        if (playerClassText != null)
        {
            playerClassText.text = character.characterClass;
        }
        
        Debug.Log($"[EquipmentScreenUI] Updated player details: {character.characterName}, Level {character.level}, {character.characterClass}");
    }
    
    private void RefreshEquipmentSlots()
    {
        var equipmentManager = EquipmentManager.Instance;
        if (equipmentManager == null)
        {
            Debug.LogWarning("[EquipmentScreenUI] EquipmentManager not found during refresh!");
            return;
        }
        
        Debug.Log("[EquipmentScreenUI] Refreshing all equipment slots...");
        
        foreach (var kvp in slotMap)
        {
            EquipmentType slotType = kvp.Key;
            EquipmentSlotUI slotUI = kvp.Value;
            
            if (slotUI != null)
            {
                BaseItem equippedItem = equipmentManager.GetEquippedItem(slotType);
                slotUI.SetEquippedItem(equippedItem);
                
                Debug.Log($"[EquipmentScreenUI] Refreshed {slotType}: {(equippedItem != null ? equippedItem.itemName : "Empty")}");
            }
        }
    }
    
    /// <summary>
    /// Public method to refresh equipment displays (can be called from other scripts)
    /// </summary>
    public void RefreshEquipmentDisplay()
    {
        RefreshAllDisplays();
    }
    
    
    void ShowEquipmentTooltip(EquipmentType slotType, Vector2 position)
    {
        if (ItemTooltipManager.Instance == null)
            return;

        if (!slotMap.TryGetValue(slotType, out var slot) || slot == null)
            return;

        BaseItem equipped = slot.GetEquippedItem();
        if (equipped == null)
        {
            ItemTooltipManager.Instance.HideTooltip();
            return;
        }

        // Pass isEquipped=true to use the equipped tooltip variant (no icon)
        ItemTooltipManager.Instance.ShowEquipmentTooltip(equipped, position, isEquipped: true);
    }
    
    /// <summary>
    /// Show equipped item tooltip in fixed container (called on click)
    /// </summary>
    void ShowEquippedItemTooltip(BaseItem item, EquipmentType slotType)
    {
        if (ItemTooltipManager.Instance == null || item == null)
            return;
        
        Debug.Log($"[EquipmentScreenUI] Showing equipped tooltip for {item.itemName} in slot {slotType}");
        
        // Use the container-based tooltip display
        ItemTooltipManager.Instance.ShowEquippedTooltip(item);
    }
    
    void OnReturnButtonClicked()
    {
        Debug.Log("[EquipmentScreenUI] Return button clicked");
        
        // Hide equipped tooltips when closing the screen
        if (ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.HideEquippedTooltip();
        }
        
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
    
    public void EquipItem(BaseItem item, EquipmentType slotType)
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
    
    public BaseItem GetEquippedItem(EquipmentType slotType)
    {
        if (slotMap.ContainsKey(slotType) && slotMap[slotType] != null)
        {
            return slotMap[slotType].GetEquippedItem();
        }
        return null;
    }
}

