using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Dexiled.Data.Items;

/// <summary>
/// Main manager for Forge Scene
/// Handles UI initialization, inventory/stash access, and scene setup
/// </summary>
public class ForgeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InventoryGridUI inventoryGrid;
    [SerializeField] private InventoryGridUI stashGrid;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject stashPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button toggleInventoryButton;
    [SerializeField] private Button toggleStashButton;
    [SerializeField] private Button returnToTownButton;
    
    [Header("Forge UI Components")]
    [SerializeField] private ForgeMaterialDisplayUI materialDisplay;
    [SerializeField] private ForgeRecipeListUI recipeList;
    [SerializeField] private ForgeCraftingOutputUI craftingOutput;
    [SerializeField] private Button craftButton;
    [SerializeField] private ForgeQuickSalvageUI quickSalvage;
    
    private void Start()
    {
        VerifyManagers();
        InitializeGrids();
        SetupButtons();
        SetupForgeUI();
    }
    
    /// <summary>
    /// Verify required managers are available
    /// </summary>
    private void VerifyManagers()
    {
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("[ForgeManager] CharacterManager.Instance is null! Inventory will not work.");
        }
        else
        {
            var character = CharacterManager.Instance.GetCurrentCharacter();
            Debug.Log($"[ForgeManager] CharacterManager found. Character: {(character != null ? character.characterName : "NULL")}");
        }
        
        if (StashManager.Instance == null)
        {
            Debug.LogError("[ForgeManager] StashManager.Instance is null! Stash will not work.");
        }
        else
        {
            Debug.Log($"[ForgeManager] StashManager found. Items: {StashManager.Instance.stashItems.Count}");
        }
    }
    
    /// <summary>
    /// Configure inventory and stash grids with correct modes
    /// </summary>
    private void InitializeGrids()
    {
        // Configure Inventory Grid
        if (inventoryGrid != null)
        {
            inventoryGrid.gridMode = InventoryGridUI.GridMode.CharacterInventory;
            Debug.Log("[ForgeManager] Configured inventoryGrid as CharacterInventory");
        }
        else
        {
            Debug.LogWarning("[ForgeManager] InventoryGridUI not assigned!");
        }
        
        // Configure Stash Grid
        if (stashGrid != null)
        {
            stashGrid.gridMode = InventoryGridUI.GridMode.GlobalStash;
            Debug.Log("[ForgeManager] Configured stashGrid as GlobalStash");
        }
        else
        {
            Debug.LogWarning("[ForgeManager] StashGridUI not assigned!");
        }
    }
    
    /// <summary>
    /// Setup button event listeners
    /// </summary>
    private void SetupButtons()
    {
        if (toggleInventoryButton != null)
        {
            toggleInventoryButton.onClick.RemoveAllListeners();
            toggleInventoryButton.onClick.AddListener(ToggleInventory);
        }
        
        if (toggleStashButton != null)
        {
            toggleStashButton.onClick.RemoveAllListeners();
            toggleStashButton.onClick.AddListener(ToggleStash);
        }
        
        if (returnToTownButton != null)
        {
            returnToTownButton.onClick.RemoveAllListeners();
            returnToTownButton.onClick.AddListener(ReturnToTown);
        }
    }
    
    /// <summary>
    /// Toggle inventory panel visibility
    /// </summary>
    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            Debug.Log($"[ForgeManager] Inventory panel toggled: {inventoryPanel.activeSelf}");
        }
    }
    
    /// <summary>
    /// Toggle stash panel visibility
    /// </summary>
    public void ToggleStash()
    {
        if (stashPanel != null)
        {
            stashPanel.SetActive(!stashPanel.activeSelf);
            Debug.Log($"[ForgeManager] Stash panel toggled: {stashPanel.activeSelf}");
        }
    }
    
    /// <summary>
    /// Return to town scene
    /// </summary>
    private void ReturnToTown()
    {
        SceneManager.LoadScene("MainGameUI");
    }
    
    /// <summary>
    /// Setup Forge UI components
    /// </summary>
    private void SetupForgeUI()
    {
        // Setup material display
        if (materialDisplay != null)
        {
            materialDisplay.RefreshDisplay();
        }
        
        // Setup recipe list
        if (recipeList != null)
        {
            recipeList.OnRecipeSelected += OnRecipeSelected;
        }
        
        // Setup craft button
        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }
    }
    
    /// <summary>
    /// Handle recipe selection
    /// </summary>
    private void OnRecipeSelected(CraftingRecipe recipe)
    {
        if (craftingOutput != null)
        {
            craftingOutput.ShowRecipePreview(recipe);
        }
    }
    
    /// <summary>
    /// Handle craft button click (public for Unity event system)
    /// </summary>
    public void OnCraftButtonClicked()
    {
        if (recipeList == null)
        {
            Debug.LogWarning("[ForgeManager] Recipe list not assigned.");
            return;
        }
        
        var selectedRecipe = recipeList.GetSelectedRecipe();
        if (selectedRecipe == null)
        {
            Debug.LogWarning("[ForgeManager] No recipe selected.");
            return;
        }
        
        var character = CharacterManager.Instance?.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogWarning("[ForgeManager] No current character.");
            return;
        }
        
        // Craft the item
        var craftedItem = ForgeCraftingSystem.CraftItem(selectedRecipe, character);
        
        if (craftedItem != null)
        {
            Debug.Log($"[ForgeManager] Successfully crafted: {craftedItem.GetDisplayName()}");
            
            // Show crafted item in output
            if (craftingOutput != null)
            {
                craftingOutput.ShowCraftedItem(craftedItem);
            }
            
            // Refresh material display
            if (materialDisplay != null)
            {
                materialDisplay.RefreshDisplay();
            }
            
            // Refresh recipe list (materials changed, can craft status may have changed)
            if (recipeList != null)
            {
                recipeList.RefreshList();
            }
            
            // Refresh grids
            RefreshGrids();
        }
        else
        {
            Debug.LogWarning("[ForgeManager] Failed to craft item.");
        }
    }
    
    /// <summary>
    /// Refresh both inventory and stash grids
    /// </summary>
    public void RefreshGrids()
    {
        if (inventoryGrid != null)
        {
            inventoryGrid.RefreshFromDataSource();
        }
        
        if (stashGrid != null)
        {
            stashGrid.RefreshFromDataSource();
        }
    }
    
    /// <summary>
    /// Get items from character inventory (for salvaging)
    /// </summary>
    public System.Collections.Generic.List<BaseItem> GetInventoryItems()
    {
        if (CharacterManager.Instance != null && CharacterManager.Instance.inventoryItems != null)
        {
            return CharacterManager.Instance.inventoryItems;
        }
        return new System.Collections.Generic.List<BaseItem>();
    }
    
    /// <summary>
    /// Get items from stash (for salvaging)
    /// </summary>
    public System.Collections.Generic.List<BaseItem> GetStashItems()
    {
        if (StashManager.Instance != null && StashManager.Instance.stashItems != null)
        {
            return StashManager.Instance.stashItems;
        }
        return new System.Collections.Generic.List<BaseItem>();
    }
}

