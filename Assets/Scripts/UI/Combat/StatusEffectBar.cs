using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Displays status effect icons in separate horizontal bars for buffs and debuffs
/// </summary>
public class StatusEffectBar : MonoBehaviour
{
    [Header("UI References - Separate Containers")]
    public Transform buffContainer; // Parent transform for buff icons
    public Transform debuffContainer; // Parent transform for debuff icons
    public GameObject statusEffectIconPrefab; // Prefab for individual status effect icons
    
    [Header("Layout Settings")]
    public float iconSpacing = 5f;
    public int maxIconsPerRow = 8;
    
    [Header("Visual Settings")]
    public Color buffBackgroundColor = new Color(0f, 1f, 0f, 0.3f); // Green with transparency
    public Color debuffBackgroundColor = new Color(1f, 0f, 0f, 0.3f); // Red with transparency
    
    private StatusEffectManager statusEffectManager;
    private List<GameObject> activeBuffIcons = new List<GameObject>();
    private List<GameObject> activeDebuffIcons = new List<GameObject>();
    
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
    }
    
    private void Update()
    {
        UpdateStatusEffectIcons();
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
