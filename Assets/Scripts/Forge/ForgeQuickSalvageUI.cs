using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

/// <summary>
/// Handles quick salvage functionality with rarity and type filters
/// </summary>
public class ForgeQuickSalvageUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button quickSalvageButton;
    [SerializeField] private TMP_Dropdown rarityDropdown;
    [SerializeField] private TMP_Dropdown typeDropdown;
    
    [Header("Salvage Settings")]
    [Tooltip("Salvage from inventory, stash, or both")]
    [SerializeField] private SalvageSource salvageSource = SalvageSource.Both;
    
    private enum SalvageSource
    {
        Inventory,
        Stash,
        Both
    }
    
    private void Start()
    {
        SetupDropdowns();
        SetupButton();
    }
    
    /// <summary>
    /// Setup rarity and type dropdowns
    /// </summary>
    private void SetupDropdowns()
    {
        // Setup Rarity Dropdown
        if (rarityDropdown != null)
        {
            rarityDropdown.ClearOptions();
            var rarityOptions = new List<string>
            {
                "Common",
                "Magic",
                "Rare",
                "Unique"
            };
            rarityDropdown.AddOptions(rarityOptions);
            rarityDropdown.value = 0; // Default to Common
        }
        
        // Setup Type Dropdown
        if (typeDropdown != null)
        {
            typeDropdown.ClearOptions();
            var typeOptions = new List<string>
            {
                "All",
                "Weapons",
                "Equipment",
                "Warrants",
                "Effigies"
            };
            typeDropdown.AddOptions(typeOptions);
            typeDropdown.value = 0; // Default to All
        }
    }
    
    /// <summary>
    /// Setup quick salvage button
    /// </summary>
    private void SetupButton()
    {
        if (quickSalvageButton != null)
        {
            quickSalvageButton.onClick.RemoveAllListeners();
            quickSalvageButton.onClick.AddListener(PerformQuickSalvage);
        }
    }
    
    /// <summary>
    /// Perform quick salvage based on selected filters
    /// </summary>
    public void PerformQuickSalvage()
    {
        if (CharacterManager.Instance == null)
        {
            Debug.LogWarning("[ForgeQuickSalvageUI] CharacterManager not found.");
            return;
        }
        
        var character = CharacterManager.Instance.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogWarning("[ForgeQuickSalvageUI] No current character.");
            return;
        }
        
        // Get selected filters
        ItemRarity selectedRarity = GetSelectedRarity();
        ItemTypeFilter selectedTypeFilter = GetSelectedTypeFilter();
        
        // Get items to salvage
        List<BaseItem> itemsToSalvage = GetItemsToSalvage(selectedRarity, selectedTypeFilter);
        
        if (itemsToSalvage.Count == 0)
        {
            Debug.Log("[ForgeQuickSalvageUI] No items found matching filters.");
            return;
        }
        
        // Confirm with user (optional - you might want to add a confirmation dialog)
        int salvagedCount = 0;
        foreach (var item in itemsToSalvage)
        {
            if (ForgeSalvageSystem.SalvageItem(item, character))
            {
                // Remove item from inventory or stash
                RemoveItemFromSource(item);
                salvagedCount++;
            }
        }
        
        Debug.Log($"[ForgeQuickSalvageUI] Salvaged {salvagedCount} items.");
        
        // Refresh UI
        RefreshUI();
    }
    
    /// <summary>
    /// Get selected rarity from dropdown
    /// </summary>
    private ItemRarity GetSelectedRarity()
    {
        if (rarityDropdown == null)
        {
            return ItemRarity.Normal;
        }
        
        int index = rarityDropdown.value;
        switch (index)
        {
            case 0: return ItemRarity.Normal;
            case 1: return ItemRarity.Magic;
            case 2: return ItemRarity.Rare;
            case 3: return ItemRarity.Unique;
            default: return ItemRarity.Normal;
        }
    }
    
    /// <summary>
    /// Get selected type filter from dropdown
    /// </summary>
    private ItemTypeFilter GetSelectedTypeFilter()
    {
        if (typeDropdown == null)
        {
            return ItemTypeFilter.All;
        }
        
        int index = typeDropdown.value;
        switch (index)
        {
            case 0: return ItemTypeFilter.All;
            case 1: return ItemTypeFilter.Weapons;
            case 2: return ItemTypeFilter.Equipment;
            case 3: return ItemTypeFilter.Warrants;
            case 4: return ItemTypeFilter.Effigies;
            default: return ItemTypeFilter.All;
        }
    }
    
    /// <summary>
    /// Get items matching the filters from inventory/stash
    /// </summary>
    private List<BaseItem> GetItemsToSalvage(ItemRarity rarity, ItemTypeFilter typeFilter)
    {
        List<BaseItem> items = new List<BaseItem>();
        
        // Get from inventory
        if (salvageSource == SalvageSource.Inventory || salvageSource == SalvageSource.Both)
        {
            if (CharacterManager.Instance != null && CharacterManager.Instance.inventoryItems != null)
            {
                items.AddRange(CharacterManager.Instance.inventoryItems);
            }
        }
        
        // Get from stash
        if (salvageSource == SalvageSource.Stash || salvageSource == SalvageSource.Both)
        {
            if (StashManager.Instance != null && StashManager.Instance.stashItems != null)
            {
                items.AddRange(StashManager.Instance.stashItems);
            }
        }
        
        // Filter by rarity
        items = items.Where(item => item != null && item.rarity == rarity).ToList();
        
        // Filter by type
        items = FilterByType(items, typeFilter);
        
        return items;
    }
    
    /// <summary>
    /// Filter items by type
    /// </summary>
    private List<BaseItem> FilterByType(List<BaseItem> items, ItemTypeFilter filter)
    {
        switch (filter)
        {
            case ItemTypeFilter.All:
                return items;
            case ItemTypeFilter.Weapons:
                return items.Where(item => item.itemType == ItemType.Weapon).ToList();
            case ItemTypeFilter.Equipment:
                return items.Where(item => 
                    item.itemType == ItemType.Armour || 
                    item.itemType == ItemType.Armor ||
                    item.itemType == ItemType.Accessory).ToList();
            case ItemTypeFilter.Warrants:
                return items.Where(item => item.itemType == ItemType.Warrant).ToList();
            case ItemTypeFilter.Effigies:
                return items.Where(item => item.itemType == ItemType.Effigy).ToList();
            default:
                return items;
        }
    }
    
    /// <summary>
    /// Remove item from its source (inventory or stash)
    /// </summary>
    private void RemoveItemFromSource(BaseItem item)
    {
        // Try to remove from inventory first
        if (CharacterManager.Instance != null && CharacterManager.Instance.inventoryItems != null)
        {
            if (CharacterManager.Instance.inventoryItems.Contains(item))
            {
                CharacterManager.Instance.inventoryItems.Remove(item);
                return;
            }
        }
        
        // Try to remove from stash
        if (StashManager.Instance != null && StashManager.Instance.stashItems != null)
        {
            if (StashManager.Instance.stashItems.Contains(item))
            {
                StashManager.Instance.stashItems.Remove(item);
                return;
            }
        }
    }
    
    /// <summary>
    /// Refresh UI after salvage
    /// </summary>
    private void RefreshUI()
    {
        // Refresh material display
        var materialDisplay = FindFirstObjectByType<ForgeMaterialDisplayUI>();
        if (materialDisplay != null)
        {
            materialDisplay.RefreshDisplay();
        }
        
        // Refresh inventory/stash grids
        var forgeManager = FindFirstObjectByType<ForgeManager>();
        if (forgeManager != null)
        {
            forgeManager.RefreshGrids();
        }
    }
    
    private enum ItemTypeFilter
    {
        All,
        Weapons,
        Equipment,
        Warrants,
        Effigies
    }
}

