using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

/// <summary>
/// Displays a scrollable list of crafting recipes with single selection
/// </summary>
public class ForgeRecipeListUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject recipeButtonPrefab;
    
    [Header("Recipe Source")]
    [Tooltip("If empty, will load all recipes from Resources/Forge/Recipes")]
    [SerializeField] private List<CraftingRecipe> availableRecipes = new List<CraftingRecipe>();
    
    [Header("Filtering")]
    [Tooltip("Filter recipes by character level")]
    [SerializeField] private bool filterByCharacterLevel = true;
    [Tooltip("Level range around character level to show recipes")]
    [SerializeField] private int levelRange = 5;
    
    private CraftingRecipe selectedRecipe;
    private Dictionary<CraftingRecipe, GameObject> recipeButtons = new Dictionary<CraftingRecipe, GameObject>();
    
    public System.Action<CraftingRecipe> OnRecipeSelected;
    
    private void Start()
    {
        LoadRecipes();
        PopulateRecipeList();
    }
    
    /// <summary>
    /// Load recipes from Resources or use assigned list
    /// Supports subfolders - Resources.LoadAll recursively searches all subfolders under the specified path
    /// </summary>
    private void LoadRecipes()
    {
        if (availableRecipes == null || availableRecipes.Count == 0)
        {
            // Resources.LoadAll recursively searches all subfolders under the specified path
            // So "Forge/Recipes" will find recipes in:
            // - Resources/Forge/Recipes/
            // - Resources/Forge/Recipes/Weapons/
            // - Resources/Forge/Recipes/Armour/
            // - Resources/Forge/Recipes/Armour/Helmets/
            // - etc.
            var loadedRecipes = Resources.LoadAll<CraftingRecipe>("Forge/Recipes");
            if (loadedRecipes != null && loadedRecipes.Length > 0)
            {
                availableRecipes = loadedRecipes.ToList();
                Debug.Log($"[ForgeRecipeListUI] Loaded {availableRecipes.Count} recipes from Resources/Forge/Recipes (including all subfolders).");
            }
            else
            {
                Debug.LogWarning("[ForgeRecipeListUI] No recipes found. Assign recipes in inspector or create them in Resources/Forge/Recipes (subfolders are supported).");
            }
        }
    }
    
    /// <summary>
    /// Populate the recipe list UI
    /// </summary>
    public void PopulateRecipeList()
    {
        if (contentContainer == null)
        {
            Debug.LogError("[ForgeRecipeListUI] Content container not assigned!");
            return;
        }
        
        // Clear existing buttons
        foreach (var button in recipeButtons.Values)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
        recipeButtons.Clear();
        
        // Get filtered recipes
        var filteredRecipes = GetFilteredRecipes();
        
        if (filteredRecipes.Count == 0)
        {
            Debug.LogWarning("[ForgeRecipeListUI] No recipes available after filtering.");
            return;
        }
        
        // Create buttons for each recipe
        foreach (var recipe in filteredRecipes)
        {
            CreateRecipeButton(recipe);
        }
    }
    
    /// <summary>
    /// Get recipes filtered by character level
    /// Shows recipes where the recipe's level range overlaps with (characterLevel - levelRange) to (characterLevel + levelRange)
    /// This means recipes can show items both below and above the character's current level
    /// </summary>
    private List<CraftingRecipe> GetFilteredRecipes()
    {
        if (!filterByCharacterLevel || CharacterManager.Instance == null)
        {
            return availableRecipes;
        }
        
        var character = CharacterManager.Instance.GetCurrentCharacter();
        if (character == null)
        {
            return availableRecipes;
        }
        
        int characterLevel = character.level;
        // Calculate level range - allow looking both ways (below and above character level)
        int minLevel = Mathf.Max(0, characterLevel - levelRange); // Allow level 0 items
        int maxLevel = characterLevel + levelRange;
        
        return availableRecipes.Where(recipe =>
        {
            // Check if recipe's item level range overlaps with character level range
            int recipeMin = recipe.minItemLevel;
            // For unlimited max level (-1), use a very high number to ensure it overlaps
            // This allows recipes with unlimited max to show items at any level
            int recipeMax = recipe.maxItemLevel == -1 ? int.MaxValue : recipe.maxItemLevel;
            
            // Check if ranges overlap: recipe range [recipeMin, recipeMax] overlaps with [minLevel, maxLevel]
            // Ranges overlap if: recipeMax >= minLevel AND recipeMin <= maxLevel
            // This works both ways - shows recipes with items below AND above character level
            bool overlaps = recipeMax >= minLevel && recipeMin <= maxLevel;
            
            return overlaps;
        }).ToList();
    }
    
    /// <summary>
    /// Create a button for a recipe
    /// </summary>
    private void CreateRecipeButton(CraftingRecipe recipe)
    {
        GameObject buttonObj;
        
        if (recipeButtonPrefab != null)
        {
            buttonObj = Instantiate(recipeButtonPrefab, contentContainer);
        }
        else
        {
            // Create a simple button if no prefab
            buttonObj = new GameObject($"Recipe_{recipe.recipeName}");
            buttonObj.transform.SetParent(contentContainer);
            
            var button = buttonObj.AddComponent<Button>();
            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            var rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = recipe.recipeName;
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Left;
            text.overflowMode = TextOverflowModes.Ellipsis;
        }
        
        // Setup button click
        var btn = buttonObj.GetComponent<Button>();
        if (btn == null)
        {
            btn = buttonObj.AddComponent<Button>();
        }
        
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SelectRecipe(recipe));
        
        // Try to use ForgeRecipeButton component if available
        var recipeButton = buttonObj.GetComponent<ForgeRecipeButton>();
        if (recipeButton != null)
        {
            var character = CharacterManager.Instance?.GetCurrentCharacter();
            bool canCraft = character != null && recipe.CanCraft(character);
            recipeButton.SetRecipeData(recipe, false, canCraft);
            recipeButton.OnRecipeClicked += SelectRecipe;
        }
        
        // Store reference
        recipeButtons[recipe] = buttonObj;
        
        // Update button appearance based on can craft status
        UpdateRecipeButtonAppearance(recipe, buttonObj);
    }
    
    /// <summary>
    /// Update button appearance based on whether recipe can be crafted
    /// </summary>
    private void UpdateRecipeButtonAppearance(CraftingRecipe recipe, GameObject buttonObj)
    {
        var character = CharacterManager.Instance?.GetCurrentCharacter();
        bool canCraft = character != null && recipe.CanCraft(character);
        bool isSelected = selectedRecipe == recipe;
        
        // Try to use ForgeRecipeButton component if available
        var recipeButton = buttonObj.GetComponent<ForgeRecipeButton>();
        if (recipeButton != null)
        {
            recipeButton.SetSelected(isSelected, canCraft);
            return;
        }
        
        // Fallback to simple button color update
        var button = buttonObj.GetComponent<Button>();
        if (button == null) return;
        
        var colors = button.colors;
        
        if (isSelected)
        {
            colors.normalColor = new Color(0.5f, 0.7f, 0.5f, 1f); // Selected green
        }
        else if (canCraft)
        {
            colors.normalColor = new Color(0.3f, 0.5f, 0.3f, 1f); // Green tint
        }
        else
        {
            colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Gray
        }
        
        button.colors = colors;
        
        // Update text color if it's a TextMeshProUGUI
        var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = canCraft ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
        }
    }
    
    /// <summary>
    /// Select a recipe
    /// </summary>
    public void SelectRecipe(CraftingRecipe recipe)
    {
        if (recipe == null)
        {
            return;
        }
        
        // Deselect previous
        if (selectedRecipe != null && recipeButtons.ContainsKey(selectedRecipe))
        {
            UpdateRecipeButtonAppearance(selectedRecipe, recipeButtons[selectedRecipe]);
        }
        
        selectedRecipe = recipe;
        
        // Highlight selected
        if (recipeButtons.ContainsKey(recipe))
        {
            UpdateRecipeButtonAppearance(recipe, recipeButtons[recipe]);
        }
        
        OnRecipeSelected?.Invoke(recipe);
        Debug.Log($"[ForgeRecipeListUI] Selected recipe: {recipe.recipeName}");
    }
    
    /// <summary>
    /// Get the currently selected recipe
    /// </summary>
    public CraftingRecipe GetSelectedRecipe()
    {
        return selectedRecipe;
    }
    
    /// <summary>
    /// Refresh the recipe list (e.g., after materials change)
    /// </summary>
    public void RefreshList()
    {
        PopulateRecipeList();
        
        // Reselect current recipe if still available
        if (selectedRecipe != null)
        {
            SelectRecipe(selectedRecipe);
        }
    }
}

