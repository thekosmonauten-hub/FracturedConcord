using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Displays status effect icons in separate horizontal bars for buffs and debuffs.
/// NOTE: Stack displays (Momentum, Agitate, etc.) are now handled by StackDisplayManager.
/// </summary>
public class StatusEffectBar : MonoBehaviour
{
    [Header("UI References - Separate Containers")]
    public Transform buffContainer; // Parent transform for buff icons
    public Transform debuffContainer; // Parent transform for debuff icons
    [Tooltip("DEPRECATED: Stack displays are now handled by StackDisplayManager. This field is kept for backwards compatibility only.")]
    public Transform stackContainer; // Parent transform for stack icons (Momentum, Agitate, etc.) - DEPRECATED
    public GameObject statusEffectIconPrefab; // Prefab for individual status effect icons
    
    [Header("Momentum Display (Legacy - kept for backwards compatibility)")]
    [Tooltip("Optional: Text element to display current momentum value (deprecated - use stack icons instead)")]
    public TextMeshProUGUI momentumText;
    [Tooltip("Optional: GameObject container for momentum display (deprecated - use stack icons instead)")]
    public GameObject momentumDisplayContainer;
    
    [Header("Layout Settings")]
    public float iconSpacing = 5f;
    public int maxIconsPerRow = 8;
    
    [Header("Visual Settings")]
    public Color buffBackgroundColor = new Color(0f, 1f, 0f, 0.3f); // Green with transparency
    public Color debuffBackgroundColor = new Color(1f, 0f, 0f, 0.3f); // Red with transparency
    
    private StatusEffectManager statusEffectManager;
    private List<GameObject> activeBuffIcons = new List<GameObject>();
    private List<GameObject> activeDebuffIcons = new List<GameObject>();
    private Dictionary<StackType, GameObject> activeStackIcons = new Dictionary<StackType, GameObject>();
    private CharacterManager characterManager;
    private Dictionary<StackType, Sprite> stackSpriteCache = new Dictionary<StackType, Sprite>();
    
    private void Awake()
    {
        // Auto-find buff container if not assigned
        if (buffContainer == null)
        {
            Transform buffs = transform.Find("BuffContainer");
            if (buffs != null)
                buffContainer = buffs;
            else
                Debug.LogWarning("BuffContainer not found! Please assign it in the inspector.");
        }
        
        // Auto-find debuff container if not assigned
        if (debuffContainer == null)
        {
            Transform debuffs = transform.Find("DebuffContainer");
            if (debuffs != null)
                debuffContainer = debuffs;
            else
                Debug.LogWarning("DebuffContainer not found! Please assign it in the inspector.");
        }
        
        // Auto-find momentum text if not assigned
        if (momentumText == null)
        {
            Transform momentumTransform = transform.Find("MomentumText");
            if (momentumTransform != null)
                momentumText = momentumTransform.GetComponent<TextMeshProUGUI>();
        }
        
        // Auto-find momentum container if not assigned (legacy)
        if (momentumDisplayContainer == null)
        {
            Transform momentumContainer = transform.Find("MomentumDisplay");
            if (momentumContainer != null)
                momentumDisplayContainer = momentumContainer.gameObject;
        }
        
        // Auto-find stack container if not assigned
        if (stackContainer == null)
        {
            Transform stacks = transform.Find("StackContainer");
            if (stacks != null)
                stackContainer = stacks;
            else
            {
                // Try to use buffContainer as fallback for stacks
                stackContainer = buffContainer;
                Debug.LogWarning("StackContainer not found! Using BuffContainer as fallback for stack display.");
            }
        }
        
        // Get CharacterManager reference
        characterManager = CharacterManager.Instance;
    }
    
    private void Update()
    {
        UpdateStatusEffectIcons();
        // Stack displays are now handled by StackDisplayManager
        // UpdateMomentumDisplay(); // Legacy method - kept for backwards compatibility
    }
    
    /// <summary>
    /// Set the StatusEffectManager to monitor
    /// </summary>
    public void SetStatusEffectManager(StatusEffectManager manager)
    {
        statusEffectManager = manager;
    }
    
    /// <summary>
    /// Update the status effect icons display for both buffs and debuffs
    /// </summary>
    private void UpdateStatusEffectIcons()
    {
        if (statusEffectManager == null) return;
        
        // Get buffs and debuffs separately
        var activeBuffs = statusEffectManager.GetActiveBuffs();
        var activeDebuffs = statusEffectManager.GetActiveDebuffs();
        
        // Update buff bar
        UpdateEffectBar(activeBuffs, activeBuffIcons, buffContainer, false);
        
        // Update debuff bar
        UpdateEffectBar(activeDebuffs, activeDebuffIcons, debuffContainer, true);
    }
    
    /// <summary>
    /// Update a single effect bar (either buffs or debuffs)
    /// </summary>
    private void UpdateEffectBar(List<StatusEffect> effects, List<GameObject> icons, Transform container, bool isDebuffBar)
    {
        if (container == null) return;
        
        // Remove icons for effects that no longer exist
        CleanupInactiveIcons(effects, icons);
        
        // Add icons for new effects
        AddNewEffectIcons(effects, icons, container, isDebuffBar);
        
        // Update existing icons
        UpdateExistingIcons(icons);
    }
    
    /// <summary>
    /// Remove icons for effects that are no longer active
    /// </summary>
    private void CleanupInactiveIcons(List<StatusEffect> activeEffects, List<GameObject> icons)
    {
        for (int i = icons.Count - 1; i >= 0; i--)
        {
            var icon = icons[i];
            var iconComponent = icon.GetComponent<StatusEffectIcon>();
            
            if (iconComponent == null || !activeEffects.Contains(iconComponent.GetStatusEffect()))
            {
                Destroy(icon);
                icons.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Add icons for new status effects
    /// </summary>
    private void AddNewEffectIcons(List<StatusEffect> activeEffects, List<GameObject> icons, Transform container, bool isDebuffBar)
    {
        foreach (var effect in activeEffects)
        {
            // Check if we already have an icon for this effect
            bool hasIcon = icons.Any(icon => 
            {
                var iconComponent = icon.GetComponent<StatusEffectIcon>();
                return iconComponent != null && iconComponent.GetStatusEffect() == effect;
            });
            
            if (!hasIcon)
            {
                CreateStatusEffectIcon(effect, icons, container, isDebuffBar);
            }
        }
    }
    
    /// <summary>
    /// Update existing icons with current effect data
    /// </summary>
    private void UpdateExistingIcons(List<GameObject> icons)
    {
        foreach (var icon in icons)
        {
            var iconComponent = icon.GetComponent<StatusEffectIcon>();
            if (iconComponent != null)
            {
                iconComponent.UpdateIcon();
            }
        }
    }
    
    /// <summary>
    /// Create a new status effect icon
    /// </summary>
    private void CreateStatusEffectIcon(StatusEffect effect, List<GameObject> icons, Transform container, bool isDebuffBar)
    {
        if (statusEffectIconPrefab == null)
        {
            Debug.LogWarning("StatusEffectIconPrefab is not assigned!");
            return;
        }
        
        if (container == null)
        {
            Debug.LogWarning($"Container is null for {(isDebuffBar ? "debuff" : "buff")} bar!");
            return;
        }
        
        // Instantiate the icon
        GameObject iconObj = Instantiate(statusEffectIconPrefab, container);
        icons.Add(iconObj);
        
        // Set up the icon
        StatusEffectIcon iconComponent = iconObj.GetComponent<StatusEffectIcon>();
        if (iconComponent != null)
        {
            iconComponent.SetStatusEffect(effect);
            iconComponent.SetStatusEffectManager(statusEffectManager);
        }
        
        // Position the icon
        PositionIcon(iconObj, icons.Count - 1);
        
        Debug.Log($"Created {(isDebuffBar ? "debuff" : "buff")} icon for {effect.effectName}");
    }
    
    /// <summary>
    /// Position an icon in the bar
    /// </summary>
    private void PositionIcon(GameObject iconObj, int index)
    {
        RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        // Calculate position based on index
        float xPosition = index * (rectTransform.rect.width + iconSpacing);
        
        // Reset position
        rectTransform.anchoredPosition = new Vector2(xPosition, 0f);
        rectTransform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// Clear all status effect icons (both buffs and debuffs)
    /// </summary>
    public void ClearAllIcons()
    {
        // Clear buff icons
        foreach (var icon in activeBuffIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        activeBuffIcons.Clear();
        
        // Clear debuff icons
        foreach (var icon in activeDebuffIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        activeDebuffIcons.Clear();
    }
    
    /// <summary>
    /// Get the total number of active status effect icons
    /// </summary>
    public int GetActiveIconCount()
    {
        return activeBuffIcons.Count + activeDebuffIcons.Count;
    }
    
    /// <summary>
    /// Get the number of active buff icons
    /// </summary>
    public int GetActiveBuffIconCount()
    {
        return activeBuffIcons.Count;
    }
    
    /// <summary>
    /// Get the number of active debuff icons
    /// </summary>
    public int GetActiveDebuffIconCount()
    {
        return activeDebuffIcons.Count;
    }
    
    /// <summary>
    /// DEPRECATED: Stack displays are now handled by StackDisplayManager.
    /// This method is kept for backwards compatibility but is no longer called.
    /// </summary>
    [System.Obsolete("Stack displays are now handled by StackDisplayManager. This method is deprecated.")]
    private void UpdateStackDisplays()
    {
        if (StackSystem.Instance == null || stackContainer == null)
        {
            return;
        }
        
        // Get all stack types from enum
        StackType[] allStackTypes = System.Enum.GetValues(typeof(StackType)) as StackType[];
        
        foreach (StackType stackType in allStackTypes)
        {
            int currentStacks = StackSystem.Instance.GetStacks(stackType);
            
            // Find existing stack GameObject by name (e.g., "Momentum", "Agitate", etc.)
            GameObject stackObj = FindStackGameObject(stackType);
            
            if (stackObj == null)
            {
                // Stack GameObject doesn't exist yet - skip it (will be created in scene if needed)
                continue;
            }
            
            // Update the existing stack GameObject
            UpdateStackIcon(stackObj, stackType, currentStacks);
            
            // Show/hide based on stack count
            // Note: Stacks are resources, so you might want to show them even at 0
            // Change this to always show if you want empty stacks visible
            stackObj.SetActive(currentStacks > 0);
            
            // Debug logging (can be removed in production)
            if (currentStacks > 0)
            {
                Debug.Log($"[StatusEffectBar] Updated {stackType}: {currentStacks} stacks");
            }
        }
    }
    
    /// <summary>
    /// Find existing stack GameObject in StackContainer by StackType name
    /// </summary>
    private GameObject FindStackGameObject(StackType stackType)
    {
        if (stackContainer == null) return null;
        
        // Try to find by exact name match (e.g., "Momentum", "Agitate")
        string stackName = stackType.ToString();
        Transform stackTransform = stackContainer.Find(stackName);
        
        if (stackTransform != null)
        {
            return stackTransform.gameObject;
        }
        
        // Try alternative naming (e.g., "Stack_Momentum", "MomentumStack")
        stackTransform = stackContainer.Find($"Stack_{stackName}");
        if (stackTransform != null)
        {
            return stackTransform.gameObject;
        }
        
        stackTransform = stackContainer.Find($"{stackName}Stack");
        if (stackTransform != null)
        {
            return stackTransform.gameObject;
        }
        
        // Search all children for name containing the stack type
        foreach (Transform child in stackContainer)
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
    /// Works with StatusEffectIcon prefab structure or custom stack UI
    /// </summary>
    private void UpdateStackIcon(GameObject stackObj, StackType stackType, int stackCount)
    {
        if (stackObj == null) return;
        
        // Try StatusEffectIcon component first (if using StatusEffectIcon prefab)
        StatusEffectIcon iconComponent = stackObj.GetComponent<StatusEffectIcon>();
        if (iconComponent != null)
        {
            // Update magnitude text with stack count
            if (iconComponent.magnitudeText != null)
            {
                iconComponent.magnitudeText.text = stackCount.ToString();
                iconComponent.magnitudeText.gameObject.SetActive(true);
            }
            
            // Hide duration text for stacks (stacks don't have duration)
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
                iconComponent.backgroundImage.color = buffBackgroundColor;
            }
        }
        else
        {
            // Fallback: Try to find TextMeshProUGUI components directly (for custom stack UI)
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
        
        // Store reference for quick lookup
        if (!activeStackIcons.ContainsKey(stackType))
        {
            activeStackIcons[stackType] = stackObj;
        }
    }
    
    /// <summary>
    /// Load sprite for a stack type
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
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Update momentum display (legacy method - kept for backwards compatibility)
    /// </summary>
    private void UpdateMomentumDisplay()
    {
        if (StackSystem.Instance == null)
        {
            // Hide momentum display if StackSystem not available
            if (momentumDisplayContainer != null)
                momentumDisplayContainer.SetActive(false);
            return;
        }
        
        int currentMomentum = StackSystem.Instance.GetStacks(StackType.Momentum);
        
        // Update momentum text (legacy)
        if (momentumText != null)
        {
            momentumText.text = $"Momentum: {currentMomentum}";
            
            // Optional: Change color based on momentum thresholds
            if (currentMomentum >= 10)
                momentumText.color = Color.yellow; // Max momentum
            else if (currentMomentum >= 7)
                momentumText.color = Color.cyan; // High momentum
            else if (currentMomentum >= 5)
                momentumText.color = Color.green; // Medium-high momentum
            else if (currentMomentum >= 3)
                momentumText.color = Color.white; // Medium momentum
            else if (currentMomentum > 0)
                momentumText.color = Color.gray; // Low momentum
            else
                momentumText.color = Color.gray; // No momentum
        }
        
        // Show/hide momentum container based on whether momentum exists (legacy)
        if (momentumDisplayContainer != null)
        {
            momentumDisplayContainer.SetActive(currentMomentum > 0);
        }
    }
    
    #region Context Menu Debug Methods
    
    [ContextMenu("Test Add Poison Effect")]
    private void TestAddPoisonEffect()
    {
        if (statusEffectManager != null)
        {
            var poisonEffect = new StatusEffect(StatusEffectType.Poison, "Poison", 5f, 3, true);
            statusEffectManager.AddStatusEffect(poisonEffect);
        }
    }
    
    [ContextMenu("Test Add Strength Buff")]
    private void TestAddStrengthBuff()
    {
        if (statusEffectManager != null)
        {
            var strengthEffect = new StatusEffect(StatusEffectType.Strength, "Strength", 3f, 5, false);
            statusEffectManager.AddStatusEffect(strengthEffect);
        }
    }
    
    [ContextMenu("Clear All Effects")]
    private void TestClearAllEffects()
    {
        if (statusEffectManager != null)
        {
            statusEffectManager.ClearAllStatusEffects();
        }
    }
    
    #endregion
}
