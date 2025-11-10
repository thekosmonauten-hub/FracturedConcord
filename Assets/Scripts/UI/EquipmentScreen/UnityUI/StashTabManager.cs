using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages dynamic stash tabs with separate inventories per tab
/// Handles tab creation, switching, renaming, and deletion
/// </summary>
public class StashTabManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tabButtonPrefab;
    [SerializeField] private Transform tabButtonContainer;
    [SerializeField] private InventoryGridUI stashGrid;
    [SerializeField] private Button addTabButton;
    [SerializeField] private Button renameTabButton;
    [SerializeField] private Button deleteTabButton;
    
    [Header("Settings")]
    [SerializeField] private int maxTabs = 10;
    [SerializeField] private string defaultTabNamePrefix = "Tab ";
    
    [Header("Visual Style")]
    [Tooltip("Sprite for active (selected) tab. Leave null to use color-based approach.")]
    [SerializeField] private Sprite activeTabSprite;
    
    [Tooltip("Sprite for inactive (unselected) tab. Leave null to use color-based approach.")]
    [SerializeField] private Sprite inactiveTabSprite;
    
    [Tooltip("Color tint for active tab (used if sprites are set, or as base color if not)")]
    [SerializeField] private Color activeTabColor = new Color(0.4f, 0.6f, 1f);
    
    [Tooltip("Color tint for inactive tab (used if sprites are set, or as base color if not)")]
    [SerializeField] private Color inactiveTabColor = new Color(0.3f, 0.3f, 0.3f);
    
    // Data structure for each stash tab
    [System.Serializable]
    public class StashTab
    {
        public string tabName;
        public List<ItemData> items = new List<ItemData>();
    }
    
    private List<StashTab> stashTabs = new List<StashTab>();
    private List<Button> tabButtons = new List<Button>();
    private int currentTabIndex = 0;
    
    void Start()
    {
        SetupEventListeners();
        InitializeDefaultTabs();
    }
    
    void SetupEventListeners()
    {
        if (addTabButton != null)
            addTabButton.onClick.AddListener(OnAddTabClicked);
        
        if (renameTabButton != null)
            renameTabButton.onClick.AddListener(OnRenameTabClicked);
        
        if (deleteTabButton != null)
            deleteTabButton.onClick.AddListener(OnDeleteTabClicked);
    }
    
    void InitializeDefaultTabs()
    {
        // Create initial tabs (e.g., 3 default tabs)
        for (int i = 0; i < 3; i++)
        {
            CreateTab($"{defaultTabNamePrefix}{i + 1}");
        }
        
        // Select first tab
        if (stashTabs.Count > 0)
        {
            SwitchToTab(0);
        }
        
        UpdateAddButtonState();
    }
    
    public void CreateTab(string tabName = null)
    {
        // Check max tabs
        if (stashTabs.Count >= maxTabs)
        {
            Debug.LogWarning($"[StashTabManager] Cannot create more than {maxTabs} tabs");
            return;
        }
        
        // Create tab data
        StashTab newTab = new StashTab
        {
            tabName = tabName ?? $"{defaultTabNamePrefix}{stashTabs.Count + 1}",
            items = new List<ItemData>()
        };
        
        stashTabs.Add(newTab);
        
        // Create tab button
        CreateTabButton(newTab, stashTabs.Count - 1);
        
        // Update add button state
        UpdateAddButtonState();
        
        Debug.Log($"[StashTabManager] Created tab: {newTab.tabName}");
    }
    
    void CreateTabButton(StashTab tab, int index)
    {
        if (tabButtonPrefab == null || tabButtonContainer == null)
        {
            Debug.LogError("[StashTabManager] Tab button prefab or container is null!");
            return;
        }
        
        GameObject buttonObj = Instantiate(tabButtonPrefab, tabButtonContainer);
        buttonObj.name = $"StashTabButton_{index}";
        
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            // Set button text
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = tab.tabName;
            }
            
            // Set initial sprite (inactive by default)
            if (inactiveTabSprite != null)
            {
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = inactiveTabSprite;
                }
            }
            
            // Add click listener
            int capturedIndex = index;
            button.onClick.AddListener(() => SwitchToTab(capturedIndex));
            
            tabButtons.Add(button);
        }
    }
    
    public void SwitchToTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= stashTabs.Count)
        {
            Debug.LogWarning($"[StashTabManager] Invalid tab index: {tabIndex}");
            return;
        }
        
        currentTabIndex = tabIndex;
        
        // Update button states
        UpdateTabButtonStates();
        
        // Load tab contents into grid
        LoadTabContents(stashTabs[currentTabIndex]);
        
        Debug.Log($"[StashTabManager] Switched to tab: {stashTabs[currentTabIndex].tabName}");
    }
    
    void UpdateTabButtonStates()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            if (tabButtons[i] == null) continue;
            
            bool isActive = (i == currentTabIndex);
            
            // If sprites are provided, use sprite-based approach
            if (activeTabSprite != null && inactiveTabSprite != null)
            {
                Image buttonImage = tabButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = isActive ? activeTabSprite : inactiveTabSprite;
                    buttonImage.color = isActive ? activeTabColor : inactiveTabColor;
                }
            }
            else
            {
                // Fall back to color-only approach
                ColorBlock colors = tabButtons[i].colors;
                colors.normalColor = isActive ? activeTabColor : inactiveTabColor;
                colors.highlightedColor = isActive ? activeTabColor : new Color(0.4f, 0.4f, 0.4f);
                tabButtons[i].colors = colors;
            }
        }
    }
    
    void LoadTabContents(StashTab tab)
    {
        if (stashGrid == null)
        {
            Debug.LogWarning("[StashTabManager] StashGrid is null!");
            return;
        }
        
        // TODO: Populate stash grid with items from tab.items
        // For now, just log
        Debug.Log($"[StashTabManager] Loading {tab.items.Count} items into stash grid");
        
        // Clear existing items in grid
        // Then populate with tab.items
        // You'll implement this based on your InventoryGridUI implementation
    }
    
    void OnAddTabClicked()
    {
        CreateTab();
    }
    
    void OnRenameTabClicked()
    {
        if (currentTabIndex < 0 || currentTabIndex >= stashTabs.Count)
            return;
        
        // TODO: Show input dialog for new name
        // For now, use a simple naming scheme
        string newName = $"Renamed Tab {currentTabIndex + 1}";
        RenameTab(currentTabIndex, newName);
    }
    
    public void RenameTab(int tabIndex, string newName)
    {
        if (tabIndex < 0 || tabIndex >= stashTabs.Count)
        {
            Debug.LogWarning($"[StashTabManager] Invalid tab index for rename: {tabIndex}");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning("[StashTabManager] Cannot rename tab to empty name");
            return;
        }
        
        stashTabs[tabIndex].tabName = newName;
        
        // Update button text
        if (tabIndex < tabButtons.Count && tabButtons[tabIndex] != null)
        {
            TextMeshProUGUI buttonText = tabButtons[tabIndex].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = newName;
            }
        }
        
        Debug.Log($"[StashTabManager] Renamed tab {tabIndex} to: {newName}");
    }
    
    void OnDeleteTabClicked()
    {
        // Don't allow deleting if only one tab remains
        if (stashTabs.Count <= 1)
        {
            Debug.LogWarning("[StashTabManager] Cannot delete the last remaining tab");
            return;
        }
        
        DeleteTab(currentTabIndex);
    }
    
    public void DeleteTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= stashTabs.Count)
        {
            Debug.LogWarning($"[StashTabManager] Invalid tab index for delete: {tabIndex}");
            return;
        }
        
        if (stashTabs.Count <= 1)
        {
            Debug.LogWarning("[StashTabManager] Cannot delete the last tab");
            return;
        }
        
        // Remove tab data
        string deletedTabName = stashTabs[tabIndex].tabName;
        stashTabs.RemoveAt(tabIndex);
        
        // Remove and destroy button
        if (tabIndex < tabButtons.Count)
        {
            if (tabButtons[tabIndex] != null)
                Destroy(tabButtons[tabIndex].gameObject);
            tabButtons.RemoveAt(tabIndex);
        }
        
        // Adjust current tab index if necessary
        if (currentTabIndex >= stashTabs.Count)
        {
            currentTabIndex = stashTabs.Count - 1;
        }
        
        // Switch to adjusted tab
        SwitchToTab(currentTabIndex);
        
        // Update add button state
        UpdateAddButtonState();
        
        Debug.Log($"[StashTabManager] Deleted tab: {deletedTabName}");
    }
    
    void UpdateAddButtonState()
    {
        if (addTabButton != null)
        {
            addTabButton.interactable = stashTabs.Count < maxTabs;
        }
    }
    
    public StashTab GetCurrentTab()
    {
        if (currentTabIndex >= 0 && currentTabIndex < stashTabs.Count)
            return stashTabs[currentTabIndex];
        return null;
    }
    
    public int GetCurrentTabIndex()
    {
        return currentTabIndex;
    }
    
    public List<StashTab> GetAllTabs()
    {
        return new List<StashTab>(stashTabs);
    }
    
    public void AddItemToCurrentTab(ItemData item)
    {
        StashTab currentTab = GetCurrentTab();
        if (currentTab != null)
        {
            currentTab.items.Add(item);
            LoadTabContents(currentTab);
            Debug.Log($"[StashTabManager] Added item to tab: {currentTab.tabName}");
        }
    }
    
    public void RemoveItemFromCurrentTab(ItemData item)
    {
        StashTab currentTab = GetCurrentTab();
        if (currentTab != null && currentTab.items.Contains(item))
        {
            currentTab.items.Remove(item);
            LoadTabContents(currentTab);
            Debug.Log($"[StashTabManager] Removed item from tab: {currentTab.tabName}");
        }
    }
    
    /// <summary>
    /// Save all stash tabs to persistent data
    /// TODO: Implement serialization
    /// </summary>
    public void SaveTabs()
    {
        Debug.Log($"[StashTabManager] Saving {stashTabs.Count} stash tabs");
        // TODO: Implement save to PlayerPrefs or file
    }
    
    /// <summary>
    /// Load stash tabs from persistent data
    /// TODO: Implement deserialization
    /// </summary>
    public void LoadTabs()
    {
        Debug.Log("[StashTabManager] Loading stash tabs");
        // TODO: Implement load from PlayerPrefs or file
    }
}

