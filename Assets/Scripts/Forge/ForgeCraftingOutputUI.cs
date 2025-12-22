using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

/// <summary>
/// Displays tooltip prefabs for crafted items in the crafting output area
/// Can show a list of available items to craft from a recipe
/// </summary>
public class ForgeCraftingOutputUI : MonoBehaviour
{
    [Header("Tooltip Prefabs")]
    [SerializeField] private GameObject weaponTooltipPrefab;
    [SerializeField] private GameObject equipmentTooltipPrefab;
    [SerializeField] private GameObject effigyTooltipPrefab;
    [SerializeField] private GameObject warrantTooltipPrefab;
    
    [Header("Container")]
    [SerializeField] private Transform tooltipContainer;
    
    [Header("Item Selection UI")]
    [SerializeField] private ScrollRect itemSelectionScrollRect;
    [SerializeField] private Transform itemSelectionContainer;
    [SerializeField] private GameObject itemSelectionButtonPrefab;
    
    private GameObject currentTooltip;
    private CraftingRecipe currentRecipe;
    private BaseItem selectedItem;
    private List<GameObject> itemSelectionButtons = new List<GameObject>();
    private Dictionary<GameObject, BaseItem> buttonToItemMap = new Dictionary<GameObject, BaseItem>();
    
    public System.Action<BaseItem> OnItemSelected;
    
    private void Awake()
    {
        if (tooltipContainer == null)
        {
            tooltipContainer = transform;
        }
    }
    
    /// <summary>
    /// Display available items to craft from a recipe
    /// Shows a list of items the player can craft based on their level
    /// </summary>
    public void ShowRecipePreview(CraftingRecipe recipe)
    {
        currentRecipe = recipe;
        selectedItem = null;
        
        if (recipe == null)
        {
            ClearPreview();
            ClearItemSelection();
            return;
        }
        
        // If recipe has a specific item or we're not doing dynamic selection, show that item
        if (recipe.specificItem != null && !recipe.craftRandomItem)
        {
            ClearItemSelection();
            ShowItemPreview(recipe.specificItem);
            selectedItem = recipe.specificItem;
            recipe.selectedItemToCraft = recipe.specificItem;
            OnItemSelected?.Invoke(selectedItem);
            return;
        }
        
        // Show list of available items for dynamic crafting
        ShowAvailableItemsList(recipe);
    }
    
    /// <summary>
    /// Show a list of available items that can be crafted from this recipe
    /// </summary>
    private void ShowAvailableItemsList(CraftingRecipe recipe)
    {
        ClearPreview();
        ClearItemSelection();
        
        var character = CharacterManager.Instance?.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogWarning("[ForgeCraftingOutputUI] No character found, cannot show available items.");
            return;
        }
        
        // Get eligible items for the character's level
        List<BaseItem> eligibleItems = recipe.GetEligibleItemsForLevel(character.level);
        
        if (eligibleItems.Count == 0)
        {
            Debug.LogWarning($"[ForgeCraftingOutputUI] No eligible items found for recipe {recipe.recipeName} at level {character.level}");
            return;
        }
        
        // Show item selection UI if we have the components
        if (itemSelectionContainer != null && itemSelectionButtonPrefab != null)
        {
            ShowItemSelectionButtons(eligibleItems);
        }
        
        // Auto-select first item and show its preview
        if (eligibleItems.Count > 0)
        {
            SelectItem(eligibleItems[0]);
        }
    }
    
    /// <summary>
    /// Show buttons for selecting items to craft
    /// </summary>
    private void ShowItemSelectionButtons(List<BaseItem> items)
    {
        buttonToItemMap.Clear();
        
        foreach (var item in items)
        {
            GameObject buttonObj = Instantiate(itemSelectionButtonPrefab, itemSelectionContainer);
            itemSelectionButtons.Add(buttonObj);
            buttonToItemMap[buttonObj] = item;
            
            // Try to use ForgeItemSelectionButton component if available
            var itemButton = buttonObj.GetComponent<ForgeItemSelectionButton>();
            if (itemButton != null)
            {
                itemButton.SetItemData(item, false);
                itemButton.OnItemClicked += SelectItem;
            }
            else
            {
                // Fallback to simple text setup
                var text = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{item.itemName} (Lv. {item.requiredLevel})";
                }
                
                // Setup button click
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    BaseItem itemCopy = item; // Capture for closure
                    button.onClick.AddListener(() => SelectItem(itemCopy));
                }
            }
        }
    }
    
    /// <summary>
    /// Select an item to craft
    /// </summary>
    private void SelectItem(BaseItem item)
    {
        if (item == null || currentRecipe == null)
        {
            return;
        }
        
        selectedItem = item;
        currentRecipe.selectedItemToCraft = item;
        
        // Show preview of selected item
        ShowItemPreview(item);
        
        // Update button appearances
        UpdateItemSelectionButtons();
        
        OnItemSelected?.Invoke(item);
        Debug.Log($"[ForgeCraftingOutputUI] Selected item to craft: {item.itemName} (Level {item.requiredLevel})");
    }
    
    /// <summary>
    /// Update button appearances to show which item is selected
    /// </summary>
    private void UpdateItemSelectionButtons()
    {
        foreach (var buttonObj in itemSelectionButtons)
        {
            if (buttonObj == null) continue;
            
            // Try to use ForgeItemSelectionButton component if available
            var itemButton = buttonObj.GetComponent<ForgeItemSelectionButton>();
            if (itemButton != null)
            {
                // Get the item data from the button using our mapping
                BaseItem buttonItem = GetItemFromButton(buttonObj);
                bool isSelected = buttonItem != null && buttonItem == selectedItem;
                itemButton.SetSelected(isSelected);
            }
            else
            {
                // Fallback to simple button color update
                var button = buttonObj.GetComponent<Button>();
                if (button == null) continue;
                
                // Check if this button corresponds to the selected item
                // Try to match by comparing the item from our mapping
                BaseItem buttonItem = GetItemFromButton(buttonObj);
                bool isSelected = buttonItem != null && buttonItem == selectedItem;
                
                // If mapping doesn't work, try text comparison as fallback
                if (!isSelected && selectedItem != null)
                {
                    var text = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (text != null)
                    {
                        string expectedText = $"{selectedItem.itemName} (Lv. {selectedItem.requiredLevel})";
                        isSelected = text.text == expectedText || text.text.Contains(selectedItem.itemName);
                    }
                }
                
                // Update button color
                var colors = button.colors;
                colors.normalColor = isSelected ? new Color(0.5f, 0.7f, 0.5f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f);
                button.colors = colors;
            }
        }
    }
    
    /// <summary>
    /// Helper to get item data from a button (for comparison)
    /// </summary>
    private BaseItem GetItemFromButton(GameObject buttonObj)
    {
        if (buttonToItemMap.TryGetValue(buttonObj, out BaseItem item))
        {
            return item;
        }
        return null;
    }
    
    /// <summary>
    /// Clear the item selection UI
    /// </summary>
    private void ClearItemSelection()
    {
        foreach (var button in itemSelectionButtons)
        {
            if (button != null)
            {
                // Unsubscribe from events
                var itemButton = button.GetComponent<ForgeItemSelectionButton>();
                if (itemButton != null)
                {
                    itemButton.OnItemClicked -= SelectItem;
                }
                
                Destroy(button);
            }
        }
        itemSelectionButtons.Clear();
        buttonToItemMap.Clear();
        
        if (currentRecipe != null)
        {
            currentRecipe.selectedItemToCraft = null;
        }
        selectedItem = null;
    }
    
    /// <summary>
    /// Display preview for a random item (shows item type info)
    /// </summary>
    private void ShowRandomItemPreview(CraftingRecipe recipe)
    {
        ClearPreview();
        
        // Create a placeholder tooltip showing item type and rarity
        // In a full implementation, you might want to generate a preview item
        Debug.Log($"[ForgeCraftingOutputUI] Preview: Random {recipe.itemType} ({recipe.craftedRarity})");
        
        // For now, we'll just show a text placeholder
        // In production, you might want to generate a temporary preview item
    }
    
    /// <summary>
    /// Display preview for a specific item
    /// </summary>
    public void ShowItemPreview(BaseItem item)
    {
        if (item == null)
        {
            ClearPreview();
            return;
        }
        
        ClearPreview();
        
        GameObject prefab = GetTooltipPrefabForItem(item);
        if (prefab == null)
        {
            Debug.LogWarning($"[ForgeCraftingOutputUI] No tooltip prefab found for item type: {item.itemType}");
            return;
        }
        
        currentTooltip = Instantiate(prefab, tooltipContainer);
        
        // Set up the tooltip view component
        SetupTooltipView(item);
    }
    
    /// <summary>
    /// Get the appropriate tooltip prefab for an item type
    /// </summary>
    private GameObject GetTooltipPrefabForItem(BaseItem item)
    {
        if (item is WeaponItem)
        {
            return weaponTooltipPrefab;
        }
        else if (item is Effigy)
        {
            return effigyTooltipPrefab;
        }
        else if (item.itemType == ItemType.Warrant)
        {
            return warrantTooltipPrefab;
        }
        else
        {
            return equipmentTooltipPrefab;
        }
    }
    
    /// <summary>
    /// Setup the tooltip view component with item data
    /// </summary>
    private void SetupTooltipView(BaseItem item)
    {
        if (currentTooltip == null)
        {
            return;
        }
        
        // Try WeaponTooltipView
        var weaponView = currentTooltip.GetComponent<WeaponTooltipView>();
        if (weaponView != null && item is WeaponItem weapon)
        {
            weaponView.SetData(weapon);
            return;
        }
        
        // Try EquipmentTooltipView
        var equipmentView = currentTooltip.GetComponent<EquipmentTooltipView>();
        if (equipmentView != null)
        {
            equipmentView.SetData(item);
            return;
        }
        
        // Try EffigyTooltipView
        var effigyView = currentTooltip.GetComponent<EffigyTooltipView>();
        if (effigyView != null && item is Effigy effigy)
        {
            effigyView.SetData(effigy);
            return;
        }
        
        // Try WarrantTooltipView
        var warrantView = currentTooltip.GetComponent<WarrantTooltipView>();
        if (warrantView != null && item.itemType == ItemType.Warrant)
        {
            // For warrants, we need to get the WarrantDefinition
            // Since warrants are BaseItem instances, we need to find the definition
            // Try to load from WarrantDatabase using the item name
            WarrantDefinition warrantDef = null;
            
            // Try to find via WarrantDatabase
            var warrantDb = Resources.Load<WarrantDatabase>("WarrantDatabase");
            if (warrantDb != null)
            {
                warrantDef = warrantDb.GetAll().FirstOrDefault(w => 
                    w != null && (w.warrantId == item.itemName || w.displayName == item.itemName));
            }
            
            if (warrantDef != null)
            {
                var tooltipData = WarrantTooltipUtility.BuildSingleWarrantData(warrantDef);
                if (tooltipData != null)
                {
                    warrantView.SetData(tooltipData);
                    return;
                }
            }
            
            // Fallback: Create a basic tooltip from BaseItem data
            Debug.LogWarning($"[ForgeCraftingOutputUI] Could not find WarrantDefinition for {item.itemName}. Warrant tooltip may not display correctly.");
        }
        
        Debug.LogWarning($"[ForgeCraftingOutputUI] Could not find appropriate tooltip view component for {item.itemType}");
    }
    
    /// <summary>
    /// Display the actual crafted item after crafting
    /// </summary>
    public void ShowCraftedItem(BaseItem craftedItem)
    {
        ShowItemPreview(craftedItem);
    }
    
    /// <summary>
    /// Clear the current preview
    /// </summary>
    public void ClearPreview()
    {
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
        }
    }
    
    /// <summary>
    /// Get the currently selected item to craft
    /// </summary>
    public BaseItem GetSelectedItem()
    {
        return selectedItem;
    }
    
    private void OnDestroy()
    {
        ClearPreview();
        ClearItemSelection();
    }
}

