using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages visual display of player stacks (Momentum, Agitate, Tolerance, Potential, Flow, etc.)
/// Supports separate containers for regular stacks and Ascendancy-related stacks.
/// 
/// Regular Stacks (displayed in StackContainer):
/// - Momentum, Agitate, Tolerance, Potential, Flow
/// 
/// Ascendancy Stacks (displayed in AscendancyStacks container):
/// - Corruption (Profane Vessel)
/// - BattleRhythm (Disciple of War)
/// 
/// Uses existing GameObjects in containers instead of instantiating new ones
/// </summary>
public class StackDisplayManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Container that holds regular stack GameObjects (e.g., Momentum, Agitate, Tolerance, Potential, Flow)")]
    public Transform stackContainer;
    
    [Tooltip("Container that holds Ascendancy-related stack GameObjects (e.g., Corruption, BattleRhythm)")]
    public Transform ascendancyStacksContainer;
    
    [Header("Settings")]
    [Tooltip("Hide stack GameObjects when stack count is 0")]
    public bool hideZeroStacks = true;
    
    [Tooltip("Optional: Prefab for creating stack icons dynamically (if stack GameObjects don't exist)")]
    public GameObject stackIconPrefab;
    
    [Header("Visual Settings")]
    [Tooltip("Background color for stack icons (stacks are treated as buffs/resources)")]
    public Color stackBackgroundColor = new Color(0f, 1f, 0f, 0.3f); // Green with transparency
    
    // Cache for stack GameObjects and sprites
    private Dictionary<StackType, GameObject> stackGameObjectLookup = new Dictionary<StackType, GameObject>();
    private Dictionary<StackType, Sprite> stackSpriteCache = new Dictionary<StackType, Sprite>();
    
    private void Awake()
    {
        // Auto-find regular stack container if not assigned
        if (stackContainer == null)
        {
            Transform container = transform.Find("StackContainer");
            if (container != null)
            {
                stackContainer = container;
            }
            else
            {
                // Try to find in parent or scene
                StackDisplayManager[] managers = FindObjectsByType<StackDisplayManager>(FindObjectsSortMode.None);
                foreach (var mgr in managers)
                {
                    if (mgr != this && mgr.stackContainer != null)
                    {
                        stackContainer = mgr.stackContainer;
                        break;
                    }
                }
                
                if (stackContainer == null)
                {
                    Debug.LogWarning("[StackDisplayManager] StackContainer not found! Please assign it in the inspector or create a child GameObject named 'StackContainer'.");
                }
            }
        }
        
        // Auto-find Ascendancy stacks container if not assigned
        if (ascendancyStacksContainer == null)
        {
            Transform container = transform.Find("AscendancyStacks");
            if (container != null)
            {
                ascendancyStacksContainer = container;
            }
            else
            {
                // Try to find in parent or scene
                StackDisplayManager[] managers = FindObjectsByType<StackDisplayManager>(FindObjectsSortMode.None);
                foreach (var mgr in managers)
                {
                    if (mgr != this && mgr.ascendancyStacksContainer != null)
                    {
                        ascendancyStacksContainer = mgr.ascendancyStacksContainer;
                        break;
                    }
                }
                
                if (ascendancyStacksContainer == null)
                {
                    Debug.LogWarning("[StackDisplayManager] AscendancyStacks container not found! Ascendancy stacks will not be displayed. Please create a child GameObject named 'AscendancyStacks'.");
                }
            }
        }
        
        // Initialize stack GameObject lookup
        InitializeStackLookup();
    }
    
    private void Start()
    {
        // Subscribe to stack changes for real-time updates
        if (StackSystem.Instance != null)
        {
            // Note: StackSystem might not have events yet, so we'll use Update() for now
        }
    }
    
    private void Update()
    {
        UpdateAllStacks();
    }
    
    /// <summary>
    /// Check if a stack type is Ascendancy-related (should be displayed in AscendancyStacks container)
    /// Public method for external use
    /// </summary>
    public static bool IsAscendancyStack(StackType stackType)
    {
        switch (stackType)
        {
            case StackType.Corruption:
            case StackType.BattleRhythm:
                return true;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Get the appropriate container for a stack type
    /// </summary>
    private Transform GetContainerForStack(StackType stackType)
    {
        if (IsAscendancyStack(stackType))
        {
            return ascendancyStacksContainer;
        }
        return stackContainer;
    }
    
    /// <summary>
    /// Initialize lookup dictionary by finding all stack GameObjects in appropriate containers
    /// </summary>
    private void InitializeStackLookup()
    {
        stackGameObjectLookup.Clear();
        
        // Get all stack types from enum
        StackType[] allStackTypes = System.Enum.GetValues(typeof(StackType)) as StackType[];
        
        foreach (StackType stackType in allStackTypes)
        {
            GameObject stackObj = FindStackGameObject(stackType);
            if (stackObj != null)
            {
                stackGameObjectLookup[stackType] = stackObj;
                Transform container = GetContainerForStack(stackType);
                string containerName = container != null ? container.name : "Unknown";
                Debug.Log($"[StackDisplayManager] Found stack GameObject: {stackType} -> {stackObj.name} (in {containerName})");
            }
        }
    }
    
    /// <summary>
    /// Update all stack displays
    /// </summary>
    private void UpdateAllStacks()
    {
        if (StackSystem.Instance == null || stackContainer == null)
        {
            return;
        }
        
        // Get all stack types from enum (automatically includes future stack types)
        StackType[] allStackTypes = System.Enum.GetValues(typeof(StackType)) as StackType[];
        
        foreach (StackType stackType in allStackTypes)
        {
            int currentStacks = StackSystem.Instance.GetStacks(stackType);
            
            // Get or find stack GameObject
            GameObject stackObj = GetOrFindStackGameObject(stackType);
            
            if (stackObj == null)
            {
                // Stack GameObject doesn't exist - skip it (will be created in scene if needed)
                continue;
            }
            
            // Update the stack GameObject
            UpdateStackIcon(stackObj, stackType, currentStacks);
            
            // Show/hide based on stack count
            if (hideZeroStacks)
            {
                stackObj.SetActive(currentStacks > 0);
            }
            else
            {
                stackObj.SetActive(true); // Always show, even at 0
            }
        }
    }
    
    /// <summary>
    /// Get or find stack GameObject for a given stack type
    /// </summary>
    private GameObject GetOrFindStackGameObject(StackType stackType)
    {
        // Check cache first
        if (stackGameObjectLookup.TryGetValue(stackType, out GameObject cached))
        {
            if (cached != null)
            {
                return cached;
            }
            else
            {
                // GameObject was destroyed, remove from cache
                stackGameObjectLookup.Remove(stackType);
            }
        }
        
        // Try to find it
        GameObject found = FindStackGameObject(stackType);
        if (found != null)
        {
            stackGameObjectLookup[stackType] = found;
        }
        
        return found;
    }
    
    /// <summary>
    /// Find existing stack GameObject in appropriate container by StackType name
    /// Supports multiple naming conventions for flexibility
    /// </summary>
    private GameObject FindStackGameObject(StackType stackType)
    {
        Transform container = GetContainerForStack(stackType);
        if (container == null) return null;
        
        string stackName = stackType.ToString();
        
        // Try exact name match first (e.g., "Momentum", "Agitate", "Corruption")
        Transform stackTransform = container.Find(stackName);
        if (stackTransform != null)
        {
            return stackTransform.gameObject;
        }
        
        // Try alternative naming conventions
        string[] nameVariations = {
            $"Stack_{stackName}",
            $"{stackName}Stack",
            $"Stack{stackName}",
            stackName.ToLower(),
            stackName.ToUpper()
        };
        
        foreach (string name in nameVariations)
        {
            stackTransform = container.Find(name);
            if (stackTransform != null)
            {
                return stackTransform.gameObject;
            }
        }
        
        // Search all children for name containing the stack type (case-insensitive)
        foreach (Transform child in container)
        {
            if (child.name.Contains(stackName, System.StringComparison.OrdinalIgnoreCase))
            {
                return child.gameObject;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Update a stack icon GameObject with current stack value
    /// Works with StackIcon component, StatusEffectIcon prefab, or custom stack UI
    /// </summary>
    private void UpdateStackIcon(GameObject stackObj, StackType stackType, int stackCount)
    {
        if (stackObj == null) return;
        
        // Try StackIcon component first (preferred - dedicated for stacks)
        StackIcon stackIconComponent = stackObj.GetComponent<StackIcon>();
        if (stackIconComponent != null)
        {
            stackIconComponent.SetupStack(stackType, stackCount);
            return;
        }
        
        // Try StatusEffectIcon component (backwards compatibility)
        StatusEffectIcon iconComponent = stackObj.GetComponent<StatusEffectIcon>();
        if (iconComponent != null)
        {
            // Update magnitude text with stack count
            if (iconComponent.magnitudeText != null)
            {
                iconComponent.magnitudeText.text = stackCount.ToString();
                iconComponent.magnitudeText.gameObject.SetActive(true);
            }
            
            // Hide duration text for stacks (stacks don't have duration, they're resources)
            if (iconComponent.durationText != null)
            {
                iconComponent.durationText.gameObject.SetActive(false);
            }
            
            // Update icon sprite
            if (iconComponent.iconImage != null)
            {
                Sprite stackSprite = LoadStackSprite(stackType);
                if (stackSprite != null)
                {
                    iconComponent.iconImage.sprite = stackSprite;
                    iconComponent.iconImage.color = Color.white; // Reset to white so sprite colors show
                }
                else
                {
                    // Fallback: use a colored square based on stack type
                    iconComponent.iconImage.color = GetStackColor(stackType);
                }
            }
            
            // Update background color (stacks are treated as buffs/resources)
            if (iconComponent.backgroundImage != null)
            {
                iconComponent.backgroundImage.color = stackBackgroundColor;
            }
            return;
        }
        
        // Fallback: Try to find UI components directly (for custom stack UI structures)
        // Look for TextMeshProUGUI components for stack count
        TextMeshProUGUI[] textComponents = stackObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        bool foundText = false;
        foreach (var text in textComponents)
        {
            // Look for magnitude/value/count/amount text
            string textName = text.name.ToLower();
            if (textName.Contains("magnitude") ||
                textName.Contains("value") ||
                textName.Contains("count") ||
                textName.Contains("amount") ||
                textName == stackType.ToString().ToLower())
            {
                text.text = stackCount.ToString();
                text.gameObject.SetActive(true);
                foundText = true;
                break; // Update first matching text component
            }
        }
        
        // If no specific text found, try to update any TextMeshProUGUI as fallback
        if (!foundText && textComponents.Length > 0)
        {
            // Use the first TextMeshProUGUI found (assuming it's the count display)
            textComponents[0].text = stackCount.ToString();
            textComponents[0].gameObject.SetActive(true);
        }
        
        // Try to find and update icon image
        Image[] images = stackObj.GetComponentsInChildren<Image>(true);
        bool foundIcon = false;
        foreach (var img in images)
        {
            string imgName = img.name.ToLower();
            // Look for icon image (but not background)
            if (imgName.Contains("icon") && !imgName.Contains("background"))
            {
                Sprite stackSprite = LoadStackSprite(stackType);
                if (stackSprite != null)
                {
                    img.sprite = stackSprite;
                    img.color = Color.white;
                }
                else
                {
                    img.color = GetStackColor(stackType);
                }
                foundIcon = true;
                break; // Update first matching icon
            }
        }
        
        // If no icon found but we have images, use the first non-background image
        if (!foundIcon && images.Length > 0)
        {
            foreach (var img in images)
            {
                if (!img.name.ToLower().Contains("background"))
                {
                    Sprite stackSprite = LoadStackSprite(stackType);
                    if (stackSprite != null)
                    {
                        img.sprite = stackSprite;
                        img.color = Color.white;
                    }
                    else
                    {
                        img.color = GetStackColor(stackType);
                    }
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Load sprite for a stack type from Resources
    /// </summary>
    private Sprite LoadStackSprite(StackType stackType)
    {
        if (stackSpriteCache.TryGetValue(stackType, out Sprite cached))
        {
            return cached;
        }
        
        // Try to load from Resources folder
        Sprite sprite = Resources.Load<Sprite>($"UI/Stacks/{stackType}");
        if (sprite == null)
        {
            // Try alternative path
            sprite = Resources.Load<Sprite>($"Stacks/{stackType}");
        }
        
        if (sprite != null)
        {
            stackSpriteCache[stackType] = sprite;
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Get color for a stack type (fallback when sprite is missing)
    /// </summary>
    private Color GetStackColor(StackType stackType)
    {
        switch (stackType)
        {
            case StackType.Momentum:
                return new Color(1f, 0.8f, 0f); // Orange-yellow
            case StackType.Agitate:
                return new Color(1f, 0.3f, 0.3f); // Red
            case StackType.Tolerance:
                return new Color(0.3f, 0.8f, 1f); // Light blue
            case StackType.Potential:
                return new Color(0.8f, 0.3f, 1f); // Purple
            case StackType.Flow:
                return new Color(0.5f, 0.9f, 1f); // Cyan-blue
            case StackType.Corruption:
                return new Color(0.8f, 0.2f, 0.8f); // Purple-pink (chaos/corruption theme)
            case StackType.BattleRhythm:
                return new Color(1f, 0.6f, 0.2f); // Orange (war/rhythm theme)
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Force refresh all stack displays (useful for debugging or after scene changes)
    /// </summary>
    [ContextMenu("Refresh Stack Display")]
    public void RefreshStackDisplay()
    {
        InitializeStackLookup();
        UpdateAllStacks();
    }
    
    /// <summary>
    /// Get the GameObject for a specific stack type (for external access)
    /// </summary>
    public GameObject GetStackGameObject(StackType stackType)
    {
        return GetOrFindStackGameObject(stackType);
    }
    
    /// <summary>
    /// Auto-create visual structure for a stack GameObject if it doesn't have StackIcon component
    /// This is a helper method to set up stack GameObjects in the scene
    /// </summary>
    [ContextMenu("Setup Stack GameObjects")]
    public void SetupStackGameObjects()
    {
        StackType[] allStackTypes = System.Enum.GetValues(typeof(StackType)) as StackType[];
        
        foreach (StackType stackType in allStackTypes)
        {
            GameObject stackObj = FindStackGameObject(stackType);
            Transform container = GetContainerForStack(stackType);
            
            if (container == null)
            {
                Debug.LogWarning($"[StackDisplayManager] Container not found for {stackType}! Skipping.");
                continue;
            }
            
            if (stackObj == null)
            {
                // Create new GameObject for this stack type in the appropriate container
                stackObj = new GameObject(stackType.ToString());
                stackObj.transform.SetParent(container, false);
                
                // Add RectTransform
                RectTransform rect = stackObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(48f, 48f);
                
                Debug.Log($"[StackDisplayManager] Created GameObject for {stackType}");
            }
            
            // Ensure it has StackIcon component
            StackIcon stackIcon = stackObj.GetComponent<StackIcon>();
            if (stackIcon == null)
            {
                stackIcon = stackObj.AddComponent<StackIcon>();
                
                // Create background
                GameObject bgObj = new GameObject("Background");
                bgObj.transform.SetParent(stackObj.transform, false);
                RectTransform bgRect = bgObj.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                Image bgImage = bgObj.AddComponent<Image>();
                bgImage.color = stackBackgroundColor;
                stackIcon.backgroundImage = bgImage;
                
                // Create icon
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(stackObj.transform, false);
                RectTransform iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.1f, 0.1f);
                iconRect.anchorMax = new Vector2(0.9f, 0.9f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
                Image iconImage = iconObj.AddComponent<Image>();
                stackIcon.iconImage = iconImage;
                
                // Create count text
                GameObject textObj = new GameObject("CountText");
                textObj.transform.SetParent(stackObj.transform, false);
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                TextMeshProUGUI countText = textObj.AddComponent<TextMeshProUGUI>();
                countText.alignment = TextAlignmentOptions.Center;
                countText.fontSize = 18f;
                countText.color = Color.white;
                countText.raycastTarget = false;
                stackIcon.countText = countText;
                
                Debug.Log($"[StackDisplayManager] Added StackIcon component and visual structure to {stackType}");
            }
        }
        
        // Refresh the display
        RefreshStackDisplay();
    }
}

