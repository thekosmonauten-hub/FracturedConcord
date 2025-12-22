using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

/// <summary>
/// Component for recipe buttons in the Forge recipe list.
/// Displays recipe information: name, item type, equipment type, and requirements.
/// </summary>
public class ForgeRecipeButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statsText; // Material requirements summary
    [SerializeField] private TextMeshProUGUI requirementsText; // Equipment type filter
    [SerializeField] private TextMeshProUGUI implicitText; // Implicit modifiers
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image selectedHighlight;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.5f, 0.7f, 0.5f, 1f);
    [SerializeField] private Color canCraftColor = new Color(0.3f, 0.5f, 0.3f, 1f);
    [SerializeField] private Color cannotCraftColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    private CraftingRecipe recipeData;
    private Button button;
    
    public System.Action<CraftingRecipe> OnRecipeClicked;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnRecipeClicked?.Invoke(recipeData));
        }
    }
    
    /// <summary>
    /// Initialize the button with recipe data
    /// </summary>
    public void SetRecipeData(CraftingRecipe recipe, bool isSelected = false, bool canCraft = false)
    {
        recipeData = recipe;
        
        if (recipe == null)
        {
            Debug.LogWarning("[ForgeRecipeButton] Cannot set null recipe data.");
            return;
        }
        
        UpdateDisplay(isSelected, canCraft);
    }
    
    /// <summary>
    /// Update the visual display based on recipe data
    /// </summary>
    private void UpdateDisplay(bool isSelected, bool canCraft)
    {
        // Recipe/Item Name - prefer item name if available, otherwise recipe name
        if (nameText != null)
        {
            string displayName = GetDisplayName(recipeData);
            nameText.text = displayName;
        }
        
        // Item Type - show more detailed type information
        if (typeText != null)
        {
            typeText.text = GetDetailedTypeString(recipeData);
        }
        
        // Level Range
        if (levelText != null)
        {
            string levelStr = $"Lv. {recipeData.minItemLevel}";
            if (recipeData.maxItemLevel > 0 && recipeData.maxItemLevel != recipeData.minItemLevel)
            {
                levelStr += $"-{recipeData.maxItemLevel}";
            }
            else if (recipeData.maxItemLevel == -1)
            {
                levelStr += "+";
            }
            levelText.text = levelStr;
        }
        
        // Material Requirements Summary
        if (statsText != null)
        {
            statsText.text = GetMaterialRequirementsString(recipeData);
        }
        
        // Equipment Type Filter (if applicable)
        if (requirementsText != null)
        {
            if (recipeData.filterByEquipmentType)
            {
                requirementsText.text = recipeData.equipmentType.ToString();
            }
            else
            {
                requirementsText.text = "Any";
            }
        }
        
        // Implicit Modifiers
        if (implicitText != null)
        {
            implicitText.text = GetImplicitModifiersString(recipeData);
        }
        
        // Item Icon - try to get from first eligible item or specific item
        if (itemIcon != null)
        {
            Sprite icon = GetRecipeIcon(recipeData);
            if (icon != null)
            {
                itemIcon.sprite = icon;
                itemIcon.enabled = true;
            }
            else
            {
                itemIcon.enabled = false;
            }
        }
        
        // Visual State
        SetSelected(isSelected, canCraft);
    }
    
    /// <summary>
    /// Set the selected state of this button
    /// </summary>
    public void SetSelected(bool isSelected, bool canCraft = false)
    {
        if (backgroundImage != null)
        {
            if (isSelected)
            {
                backgroundImage.color = selectedColor;
            }
            else if (canCraft)
            {
                backgroundImage.color = canCraftColor;
            }
            else
            {
                backgroundImage.color = cannotCraftColor;
            }
        }
        
        if (selectedHighlight != null)
        {
            selectedHighlight.enabled = isSelected;
        }
        
        if (button != null)
        {
            button.interactable = true;
        }
    }
    
    /// <summary>
    /// Get display name - prefer item name if available, otherwise recipe name
    /// </summary>
    private string GetDisplayName(CraftingRecipe recipe)
    {
        // If recipe has a specific item, use its display name
        if (recipe.specificItem != null)
        {
            return recipe.specificItem.GetDisplayName();
        }
        
        // For dynamic recipes, try to get name from first eligible item
        var character = CharacterManager.Instance?.GetCurrentCharacter();
        if (character != null)
        {
            var eligibleItems = recipe.GetEligibleItemsForLevel(character.level);
            if (eligibleItems != null && eligibleItems.Count > 0)
            {
                var firstItem = eligibleItems[0];
                if (firstItem != null)
                {
                    // For dynamic recipes, show a representative item name
                    return firstItem.GetDisplayName();
                }
            }
        }
        
        // Fallback to recipe name
        return recipe.recipeName;
    }
    
    /// <summary>
    /// Get item type as display string
    /// </summary>
    private string GetItemTypeString(CraftingRecipe recipe)
    {
        return recipe.itemType.ToString();
    }
    
    /// <summary>
    /// Get detailed type string including filters
    /// </summary>
    private string GetDetailedTypeString(CraftingRecipe recipe)
    {
        List<string> typeParts = new List<string>();
        
        // Base item type
        typeParts.Add(recipe.itemType.ToString());
        
        // Add weapon-specific filters
        if (recipe.itemType == ItemType.Weapon)
        {
            if (recipe.filterByWeaponType)
            {
                typeParts.Add(recipe.weaponType.ToString());
            }
            if (recipe.filterByWeaponHandedness)
            {
                typeParts.Add(recipe.weaponHandedness.ToString());
            }
        }
        
        // Add armour-specific filters
        if (recipe.itemType == ItemType.Armour)
        {
            if (recipe.filterByArmourSlot)
            {
                typeParts.Add(recipe.armourSlot.ToString());
            }
            if (recipe.filterByDefenseType)
            {
                typeParts.Add(recipe.defenseType.ToString());
            }
        }
        
        // If no specific filters, just return base type
        if (typeParts.Count == 1)
        {
            return typeParts[0];
        }
        
        // Join with spaces for readability
        return string.Join(" ", typeParts);
    }
    
    /// <summary>
    /// Get material requirements as a summary string
    /// </summary>
    private string GetMaterialRequirementsString(CraftingRecipe recipe)
    {
        if (recipe.requiredMaterials == null || recipe.requiredMaterials.Count == 0)
        {
            return "No materials";
        }
        
        List<string> materials = new List<string>();
        foreach (var mat in recipe.requiredMaterials)
        {
            if (mat.quantity > 0)
            {
                materials.Add($"{mat.materialType} x{mat.quantity}");
            }
        }
        
        if (materials.Count == 0)
        {
            return "No materials";
        }
        
        // Show first 2-3 materials, or all if few
        if (materials.Count <= 3)
        {
            return string.Join(", ", materials);
        }
        else
        {
            return string.Join(", ", materials.Take(2)) + "...";
        }
    }
    
    /// <summary>
    /// Get an icon for the recipe (from specific item, first eligible item, or placeholder)
    /// </summary>
    private Sprite GetRecipeIcon(CraftingRecipe recipe)
    {
        // If recipe has a specific item, use its icon
        if (recipe.specificItem != null && recipe.specificItem.itemIcon != null)
        {
            return recipe.specificItem.itemIcon;
        }
        
        // Otherwise, try to get icon from first eligible item
        var character = CharacterManager.Instance?.GetCurrentCharacter();
        if (character != null)
        {
            var eligibleItems = recipe.GetEligibleItemsForLevel(character.level);
            if (eligibleItems != null && eligibleItems.Count > 0)
            {
                var firstItem = eligibleItems[0];
                if (firstItem != null && firstItem.itemIcon != null)
                {
                    return firstItem.itemIcon;
                }
            }
        }
        
        // Fallback to placeholder icons for Warrant and Effigy recipes
        return GetPlaceholderIcon(recipe);
    }
    
    /// <summary>
    /// Get placeholder icon for recipes that don't have item icons yet
    /// </summary>
    private Sprite GetPlaceholderIcon(CraftingRecipe recipe)
    {
        switch (recipe.itemType)
        {
            case ItemType.Warrant:
                return GetWarrantPlaceholderIcon();
            
            case ItemType.Effigy:
                return GetEffigyPlaceholderIcon();
            
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Get a random placeholder icon from WarrantIconLibrary
    /// </summary>
    private Sprite GetWarrantPlaceholderIcon()
    {
        // Try to load WarrantIconLibrary from Resources
        var iconLibrary = Resources.Load<WarrantIconLibrary>("WarrantIconLibrary");
        if (iconLibrary != null)
        {
            Sprite icon = iconLibrary.GetRandomIcon();
            if (icon != null)
            {
                return icon;
            }
        }
        
        // Try alternative paths
        iconLibrary = Resources.Load<WarrantIconLibrary>("Warrants/WarrantIconLibrary");
        if (iconLibrary != null)
        {
            Sprite icon = iconLibrary.GetRandomIcon();
            if (icon != null)
            {
                return icon;
            }
        }
        
        Debug.LogWarning("[ForgeRecipeButton] Could not find WarrantIconLibrary for placeholder icon.");
        return null;
    }
    
    /// <summary>
    /// Get a placeholder icon from Effigies folder
    /// </summary>
    private Sprite GetEffigyPlaceholderIcon()
    {
        // Try to load a random effigy from Resources/Items/Effigies
        // Load all effigies and pick one's icon
        var allEffigies = Resources.LoadAll<Effigy>("Items/Effigies");
        if (allEffigies != null && allEffigies.Length > 0)
        {
            // Filter out null effigies and those without icons
            List<Effigy> validEffigies = new List<Effigy>();
            foreach (var effigy in allEffigies)
            {
                if (effigy != null && effigy.icon != null)
                {
                    validEffigies.Add(effigy);
                }
            }
            
            if (validEffigies.Count > 0)
            {
                // Return a random effigy icon
                int randomIndex = Random.Range(0, validEffigies.Count);
                return validEffigies[randomIndex].icon;
            }
        }
        
        Debug.LogWarning("[ForgeRecipeButton] Could not find any Effigy assets with icons for placeholder.");
        return null;
    }
    
    /// <summary>
    /// Get implicit modifiers string from recipe's items
    /// </summary>
    private string GetImplicitModifiersString(CraftingRecipe recipe)
    {
        BaseItem itemToCheck = null;
        
        // Prefer specific item if available
        if (recipe.specificItem != null)
        {
            itemToCheck = recipe.specificItem;
        }
        else
        {
            // Otherwise, get from first eligible item
            var character = CharacterManager.Instance?.GetCurrentCharacter();
            if (character != null)
            {
                var eligibleItems = recipe.GetEligibleItemsForLevel(character.level);
                if (eligibleItems != null && eligibleItems.Count > 0)
                {
                    itemToCheck = eligibleItems[0];
                }
            }
        }
        
        if (itemToCheck == null || itemToCheck.implicitModifiers == null || itemToCheck.implicitModifiers.Count == 0)
        {
            return "";
        }
        
        List<string> implicitStrings = new List<string>();
        foreach (var affix in itemToCheck.implicitModifiers)
        {
            if (affix != null && !string.IsNullOrEmpty(affix.description))
            {
                implicitStrings.Add(affix.description);
            }
        }
        
        // Join with separator, or show first one if space is limited
        if (implicitStrings.Count == 0)
        {
            return "";
        }
        else if (implicitStrings.Count == 1)
        {
            return implicitStrings[0];
        }
        else
        {
            // For multiple implicits, join with " | " separator
            return string.Join(" | ", implicitStrings);
        }
    }
    
#if UNITY_EDITOR
    [ContextMenu("Auto-Assign References")]
    private void AutoAssignReferences()
    {
        // Try to find common child names
        if (itemIcon == null)
        {
            itemIcon = transform.Find("IconImage")?.GetComponent<Image>() ??
                       transform.Find("Icon")?.GetComponent<Image>();
        }
        
        if (nameText == null)
        {
            nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>() ??
                       transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (typeText == null)
        {
            typeText = transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>() ??
                       transform.Find("Type")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (levelText == null)
        {
            levelText = transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>() ??
                        transform.Find("Level")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (statsText == null)
        {
            statsText = transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>() ??
                        transform.Find("Stats")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (requirementsText == null)
        {
            requirementsText = transform.Find("RequirementsText")?.GetComponent<TextMeshProUGUI>() ??
                              transform.Find("Requirements")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (implicitText == null)
        {
            implicitText = transform.Find("ImplicitText")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("Implicit")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("ImplicitModifiers")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
        
        if (selectedHighlight == null)
        {
            selectedHighlight = transform.Find("Selected")?.GetComponent<Image>();
        }
        
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}

