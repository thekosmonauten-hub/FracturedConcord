using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the grid layout for warrant items in the locker. Handles dynamic
/// generation of warrant items and integration with filtering.
/// </summary>
public class WarrantLockerGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private Vector2 cellSize = new Vector2(80, 80);
    [SerializeField] private Vector2 spacing = new Vector2(5, 5);
    
    [Header("References")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private ScrollRect scrollRect;
    
    [Header("Warrant Inventory")]
    [SerializeField] private WarrantDatabase warrantDatabase;
    [SerializeField] private WarrantNotableDatabase notableDatabase;
    [SerializeField] private WarrantIconLibrary iconLibrary;
    [SerializeField] private bool autoPopulateFromDatabase = true;
    
    private readonly List<WarrantDefinition> availableWarrants = new List<WarrantDefinition>();
    private readonly List<WarrantLockerItem> lockerItems = new List<WarrantLockerItem>();
    private readonly Dictionary<string, WarrantDefinition> definitionLookup = new Dictionary<string, WarrantDefinition>();
    private GridLayoutGroup gridLayout;
    private bool isDragging = false;
    
    private void Awake()
    {
        InitializeGrid();
        
        // Load warrants from Character immediately in Awake() so they're available
        // even if the locker panel is inactive. This ensures socket views can display
        // socketed warrants when the warrant board scene loads.
        LoadWarrantsFromCharacter();
        
        // Configure scroll rect to only allow scrolling down (prevent scrolling above top)
        ConfigureScrollRestriction();
    }
    
    /// <summary>
    /// Configures the scroll rect to only allow scrolling down (negative values).
    /// Prevents scrolling above the top of the content.
    /// </summary>
    private void ConfigureScrollRestriction()
    {
        if (scrollRect != null)
        {
            // Subscribe to value changes to clamp scroll position
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            
            // Set initial clamp
            ClampScrollPosition();
        }
    }
    
    /// <summary>
    /// Called when scroll value changes. Clamps the vertical position to prevent scrolling above top.
    /// </summary>
    private void OnScrollValueChanged(Vector2 scrollPosition)
    {
        ClampScrollPosition();
    }
    
    private void LateUpdate()
    {
        // Continuously clamp during dragging to prevent scrolling past the top
        // Using LateUpdate ensures this runs after ScrollRect has updated its position
        // This prevents scrolling past the top even during manual dragging
        if (scrollRect != null && scrollRect.vertical)
        {
            ClampScrollPosition();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event listener
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }
    }
    
    /// <summary>
    /// Clamps the scroll position to only allow downward scrolling.
    /// verticalNormalizedPosition: 1.0 = top (content starts here), 0.0 = bottom
    /// We clamp it to <= 1.0 to prevent scrolling above the top (scrolling up).
    /// This means content can only scroll down from the top, never scroll up past it.
    /// </summary>
    private void ClampScrollPosition()
    {
        if (scrollRect != null && scrollRect.vertical && scrollRect.content != null)
        {
            // Clamp vertical position to prevent scrolling above top (only allow values <= 1.0)
            // This means content can only scroll down (toward 0.0), not up (past 1.0)
            if (scrollRect.verticalNormalizedPosition > 1.0f)
            {
                scrollRect.verticalNormalizedPosition = 1.0f;
            }
            
            // Also clamp the content's anchored position directly as a backup
            // This provides an additional layer of protection during dragging
            RectTransform contentRect = scrollRect.content;
            Vector2 anchoredPos = contentRect.anchoredPosition;
            
            // If content is scrolled above the top (positive Y for top-anchored content),
            // clamp it back to the top position
            // For top-anchored content, Y should not be positive (which would move it up)
            if (anchoredPos.y > 0f)
            {
                anchoredPos.y = 0f;
                contentRect.anchoredPosition = anchoredPos;
            }
        }
    }

    private void Start()
    {
        // If warrants weren't loaded in Awake (shouldn't happen, but safety check),
        // load them here as well. This ensures warrants are loaded regardless of
        // initialization order or panel state.
        if (availableWarrants.Count == 0)
        {
            LoadWarrantsFromCharacter();
        }
        
        // After loading warrants, refresh all socket views so they can display socketed warrants
        RefreshAllSocketViews();
        
        // Refresh character warrant modifiers after warrants are loaded
        // This ensures stats from socketed warrants are applied to the character
        RefreshCharacterWarrantModifiers();
    }
    
    /// <summary>
    /// Refreshes character warrant modifiers after warrants are loaded.
    /// This ensures stats from socketed warrants are applied to the character.
    /// </summary>
    private void RefreshCharacterWarrantModifiers()
    {
        var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
        if (charManager != null && charManager.HasCharacter())
        {
            Character character = charManager.GetCurrentCharacter();
            if (character != null)
            {
                character.RefreshWarrantModifiers();
                Debug.Log("[WarrantLockerGrid] Refreshed character warrant modifiers after loading warrants");
            }
        }
    }
    
    /// <summary>
    /// Refreshes all WarrantSocketView components in the scene to sync their assigned warrants.
    /// Called after loading warrants to ensure socketed warrants are displayed correctly.
    /// </summary>
    private void RefreshAllSocketViews()
    {
        WarrantSocketView[] socketViews = FindObjectsByType<WarrantSocketView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int refreshedCount = 0;
        foreach (var socketView in socketViews)
        {
            if (socketView != null)
            {
                socketView.SyncFromState();
                refreshedCount++;
            }
        }
        if (refreshedCount > 0)
        {
            Debug.Log($"[WarrantLockerGrid] Refreshed {refreshedCount} socket views after loading warrants");
        }
    }
    
    private void InitializeGrid()
    {
        if (gridContainer == null)
        {
            gridContainer = transform;
        }
        
        gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        }
        
        gridLayout.cellSize = cellSize;
        gridLayout.spacing = spacing;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridWidth;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
    }
    
    public void SetAvailableWarrants(IEnumerable<WarrantDefinition> warrants)
    {
        availableWarrants.Clear();
        if (warrants != null)
        {
            foreach (var warrant in warrants)
            {
                if (warrant == null)
                    continue;
                availableWarrants.Add(warrant);
            }
            RegisterDefinitions(availableWarrants);
        }
        RefreshGrid();
    }
    
    public void RefreshGrid()
    {
        ClearGrid();
        
        if (itemPrefab == null)
        {
            Debug.LogWarning("[WarrantLockerGrid] Item prefab is not assigned!");
            return;
        }
        
        foreach (var warrant in availableWarrants)
        {
            CreateItem(warrant);
        }
        
        Debug.Log($"[WarrantLockerGrid] Refreshed grid with {lockerItems.Count} items");
    }
    
    public void ApplyFilter(List<WarrantDefinition> filteredWarrants)
    {
        ClearGrid();
        
        if (itemPrefab == null) return;
        if (filteredWarrants == null || filteredWarrants.Count == 0)
        {
            filteredWarrants = availableWarrants;
        }
        
        foreach (var warrant in filteredWarrants)
        {
            CreateItem(warrant);
        }
    }
    
    public void HandleWarrantAssigned(WarrantLockerItem item)
    {
        if (item == null || item.Definition == null)
            return;

        availableWarrants.Remove(item.Definition);
        lockerItems.Remove(item);
        Destroy(item.gameObject);
    }

    public void ReturnWarrantToLocker(string warrantId)
    {
        if (string.IsNullOrEmpty(warrantId))
            return;

        var definition = GetDefinition(warrantId);
        if (definition == null)
        {
            Debug.LogWarning($"[WarrantLockerGrid] Unknown warrantId '{warrantId}' when returning to locker");
            return;
        }

        if (availableWarrants.Contains(definition))
            return;

        availableWarrants.Add(definition);
        CreateItem(definition);
    }

    /// <summary>
    /// Adds a rolled warrant instance to the locker. Used for blueprint rolling and quest rewards.
    /// Also saves to Character for persistence.
    /// </summary>
    public void AddWarrantInstance(WarrantDefinition warrantInstance)
    {
        if (warrantInstance == null)
        {
            Debug.LogWarning("[WarrantLockerGrid] Attempted to add null warrant instance to locker.");
            return;
        }

        // Check if this instance is already in the locker (by ID)
        if (availableWarrants.Any(w => w != null && w.warrantId == warrantInstance.warrantId))
        {
            Debug.LogWarning($"[WarrantLockerGrid] Warrant instance '{warrantInstance.warrantId}' already exists in locker. Skipping.");
            return;
        }

        // Register the definition in our lookup
        RegisterDefinitions(new[] { warrantInstance });

        // Add to available warrants and create UI item
        availableWarrants.Add(warrantInstance);
        CreateItem(warrantInstance);

        // Save to Character for persistence
        SaveWarrantToCharacter(warrantInstance);

        Debug.Log($"[WarrantLockerGrid] Added rolled warrant instance '{warrantInstance.warrantId}' to locker.");
    }
    
    /// <summary>
    /// Load warrants from Character's owned warrants list.
    /// </summary>
    private void LoadWarrantsFromCharacter()
    {
        if (CharacterManager.Instance == null || CharacterManager.Instance.currentCharacter == null)
        {
            Debug.Log("[WarrantLockerGrid] No Character loaded. Cannot load warrants from Character.");
            ClearGrid(); // Ensure grid is empty if no character
            return;
        }

        Character character = CharacterManager.Instance.currentCharacter;
        if (character.ownedWarrants == null || character.ownedWarrants.Count == 0)
        {
            Debug.Log("[WarrantLockerGrid] Character has no owned warrants. Locker will be empty.");
            ClearGrid(); // Ensure grid is empty if character has no warrants
            return;
        }

        if (warrantDatabase == null)
        {
            Debug.LogWarning("[WarrantLockerGrid] No WarrantDatabase assigned. Cannot convert WarrantInstanceData to WarrantDefinition.");
            return;
        }

        availableWarrants.Clear();
        definitionLookup.Clear();

        foreach (var warrantData in character.ownedWarrants)
        {
            if (warrantData == null || string.IsNullOrEmpty(warrantData.warrantId))
                continue;

            // Convert WarrantInstanceData back to WarrantDefinition
            WarrantDefinition warrant = warrantData.ToWarrantDefinition(warrantDatabase, iconLibrary);
            if (warrant != null)
            {
                // If icon is still null (iconIndex was -1 or icon library changed), get a random one from icon library
                if (warrant.icon == null && iconLibrary != null)
                {
                    Sprite randomIcon = iconLibrary.GetRandomIcon();
                    if (randomIcon != null)
                    {
                        warrant.icon = randomIcon;
                        // Save the icon index so it persists next time
                        int newIconIndex = iconLibrary.GetIconIndex(randomIcon);
                        if (newIconIndex >= 0)
                        {
                            warrantData.iconIndex = newIconIndex;
                            // Save character to persist the updated iconIndex
                            if (CharacterManager.Instance != null)
                            {
                                CharacterManager.Instance.SaveCharacter();
                            }
                            Debug.Log($"[WarrantLockerGrid] Assigned random icon (index {newIconIndex}) to warrant '{warrant.warrantId}' and saved iconIndex.");
                        }
                        else
                        {
                            Debug.Log($"[WarrantLockerGrid] Assigned random icon from library to warrant '{warrant.warrantId}' (iconIndex was {warrantData.iconIndex}, couldn't get index).");
                        }
                    }
                }
                
                availableWarrants.Add(warrant);
                RegisterDefinitions(new[] { warrant });
            }
        }

        if (availableWarrants.Count > 0)
        {
            RefreshGrid();
            Debug.Log($"[WarrantLockerGrid] Loaded {availableWarrants.Count} warrants from Character.");
        }
    }
    
    /// <summary>
    /// Save a warrant instance to Character's owned warrants list.
    /// </summary>
    private void SaveWarrantToCharacter(WarrantDefinition warrant)
    {
        if (CharacterManager.Instance == null || CharacterManager.Instance.currentCharacter == null)
        {
            Debug.LogWarning("[WarrantLockerGrid] No Character loaded. Cannot save warrant to Character.");
            return;
        }

        Character character = CharacterManager.Instance.currentCharacter;
        
        // Check if warrant already exists in character's list
        if (character.ownedWarrants != null && character.ownedWarrants.Any(w => w != null && w.warrantId == warrant.warrantId))
        {
            Debug.Log($"[WarrantLockerGrid] Warrant '{warrant.warrantId}' already saved to Character.");
            return;
        }

        // Don't save blueprints to Character - only rolled instances should be saved
        if (warrant.isBlueprint)
        {
            Debug.LogWarning($"[WarrantLockerGrid] Attempted to save blueprint '{warrant.warrantId}' to Character. Blueprints are templates and should not be saved as owned warrants.");
            return;
        }

        // Create WarrantInstanceData from WarrantDefinition (with icon library for icon persistence)
        WarrantInstanceData warrantData = WarrantInstanceData.FromWarrantDefinition(warrant, warrant.warrantId, iconLibrary);
        
        if (character.ownedWarrants == null)
        {
            character.ownedWarrants = new List<WarrantInstanceData>();
        }

        character.ownedWarrants.Add(warrantData);
        
        // Save character to persist changes
        CharacterManager.Instance.SaveCharacter();
        
        Debug.Log($"[WarrantLockerGrid] Saved warrant '{warrant.warrantId}' to Character.");
    }

    public WarrantDefinition GetDefinition(string warrantId)
    {
        if (string.IsNullOrEmpty(warrantId))
            return null;

        if (definitionLookup.TryGetValue(warrantId, out var definition) && definition != null)
        {
            return definition;
        }

        if (warrantDatabase != null)
        {
            definition = warrantDatabase.GetById(warrantId);
            if (definition != null)
            {
                definitionLookup[warrantId] = definition;
            }
        }

        return definition;
    }
    
    /// <summary>
    /// Gets the NotableDatabase reference for tooltip lookups.
    /// </summary>
    public WarrantNotableDatabase GetNotableDatabase()
    {
        return notableDatabase;
    }

    public void RegisterDefinitions(IEnumerable<WarrantDefinition> definitions)
    {
        if (definitions == null)
            return;

        foreach (var def in definitions)
        {
            if (def == null || string.IsNullOrEmpty(def.warrantId))
                continue;

            if (!definitionLookup.ContainsKey(def.warrantId))
            {
                definitionLookup.Add(def.warrantId, def);
            }
        }
    }

    private void CreateItem(WarrantDefinition warrant)
    {
        if (warrant == null)
            return;

        if (itemPrefab == null)
        {
            Debug.LogWarning("[WarrantLockerGrid] Item prefab is not assigned!");
            return;
        }

        // Ensure warrant has an icon - use icon library as fallback
        if (warrant.icon == null && iconLibrary != null)
        {
            Sprite randomIcon = iconLibrary.GetRandomIcon();
            if (randomIcon != null)
            {
                warrant.icon = randomIcon;
                Debug.Log($"[WarrantLockerGrid] Assigned random icon from library to warrant '{warrant.warrantId}'.");
            }
        }

        GameObject itemObj = Instantiate(itemPrefab, gridContainer);
        WarrantLockerItem item = itemObj.GetComponent<WarrantLockerItem>();

        if (item != null)
        {
            item.Initialize(warrant, this);
            lockerItems.Add(item);
        }
    }

    private void RebuildInventoryFromDatabase()
    {
        availableWarrants.Clear();
        definitionLookup.Clear();

        if (warrantDatabase == null)
        {
            Debug.LogWarning("[WarrantLockerGrid] No WarrantDatabase assigned. Locker inventory will be empty.");
            ClearGrid();
            return;
        }

        RegisterDefinitions(warrantDatabase.Definitions);

        if (autoPopulateFromDatabase)
        {
            // Only add non-blueprint warrants (rolled instances) to locker
            // Blueprints are templates for rolling and should not appear in player's locker
            foreach (var def in warrantDatabase.Definitions)
            {
                if (def != null && !def.isBlueprint)
                {
                    availableWarrants.Add(def);
                }
            }
        }

        if (availableWarrants.Count == 0)
        {
            Debug.LogWarning("[WarrantLockerGrid] Database contains no warrants to populate the locker.");
            ClearGrid();
            return;
        }

        RefreshGrid();
    }

    public IReadOnlyList<WarrantDefinition> GetInventorySnapshot()
    {
        return new List<WarrantDefinition>(availableWarrants);
    }

    public IReadOnlyList<WarrantDefinition> GetAllKnownDefinitions()
    {
        if (warrantDatabase != null && warrantDatabase.Definitions != null && warrantDatabase.Definitions.Count > 0)
        {
            return warrantDatabase.Definitions;
        }
        return GetInventorySnapshot();
    }

    [ContextMenu("Reload Inventory From Database")]
    private void ReloadFromDatabaseContextMenu()
    {
        RebuildInventoryFromDatabase();
    }
    
    private void ClearGrid()
    {
        foreach (var item in lockerItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        lockerItems.Clear();
        
        // Also clear any orphaned children
        foreach (Transform child in gridContainer)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
}

