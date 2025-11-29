using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dexiled.Data.Status;

/// <summary>
/// Visual representation of a status effect icon
/// </summary>
public class StatusEffectIcon : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public Image backgroundImage;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI magnitudeText;
    
    [Header("Visual Settings")]
    public Color buffColor = Color.green;
    public Color debuffColor = Color.red;
    
    [Header("Database (Optional)")]
    [Tooltip("If assigned, will use StatusDatabase for sprites. Otherwise uses Resources.Load with iconName.")]
    public StatusDatabase statusDatabase;
    
    private StatusEffect statusEffect;
    private StatusEffectManager statusEffectManager;
    private static StatusDatabase cachedStatusDatabase;
    
    
    /// <summary>
    /// Set the status effect for this icon
    /// </summary>
    public void SetStatusEffect(StatusEffect effect)
    {
        statusEffect = effect;
        UpdateIcon();
    }
    
    /// <summary>
    /// Set the status effect manager
    /// </summary>
    public void SetStatusEffectManager(StatusEffectManager manager)
    {
        statusEffectManager = manager;
    }
    
    /// <summary>
    /// Update the icon display
    /// </summary>
    public void UpdateIcon()
    {
        if (statusEffect == null) return;
        
        // Update icon sprite (automatically uses positive/negative icons if available)
        if (iconImage != null)
        {
            Sprite iconSprite = LoadStatusEffectSprite(statusEffect.iconName);
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.color = Color.white; // Reset to white so sprite colors show through
            }
        }
        
        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = statusEffect.isDebuff ? debuffColor : buffColor;
        }
        
        // Update duration text (show turns remaining)
        if (durationText != null)
        {
            if (statusEffect.duration > 0)
            {
                int remainingTurns = Mathf.CeilToInt(statusEffect.timeRemaining);
                durationText.text = remainingTurns.ToString(); // Shows turns remaining
                // Debug.Log($"UpdateIcon: {statusEffect.effectName} - Displaying duration: {remainingTurns}");
            }
            else
            {
                durationText.text = "∞"; // Permanent effect
            }
        }
        
        // Update magnitude text
        if (magnitudeText != null)
        {
            if (statusEffect.magnitude > 0)
            {
                magnitudeText.text = statusEffect.magnitude.ToString("F0");
            }
            else
            {
                magnitudeText.text = "";
            }
        }
    }
    
    /// <summary>
    /// Load sprite for status effect icon
    /// Supports both StatusDatabase (preferred) and Resources.Load (fallback)
    /// Automatically selects positive/negative icons based on magnitude when available
    /// </summary>
    private Sprite LoadStatusEffectSprite(string iconName)
    {
        // Try StatusDatabase first (if available)
        StatusDatabase db = GetStatusDatabase();
        if (db != null && statusEffect != null)
        {
            // Try to get sprite with magnitude consideration (for positive/negative icons)
            Sprite sprite = db.GetStatusEffectSprite(statusEffect.effectType, statusEffect.magnitude);
            if (sprite != null)
            {
                return sprite;
            }
            
            // Fallback: Try to get sprite by iconName (for backward compatibility)
            sprite = db.GetStatusEffectSpriteByName(iconName);
            if (sprite != null)
            {
                return sprite;
            }
        }
        
        // Fallback: Try to load from Resources folder (works with sprite sheets if named correctly)
        // For positive/negative icons, try to load based on magnitude
        if (statusEffect != null && statusEffect.magnitude != 0f)
        {
            // Check if this effect type uses positive/negative icons
            StatusEffectData effectData = db != null ? db.GetStatusEffect(statusEffect.effectType) : null;
            if (effectData != null && effectData.usePositiveNegativeIcons)
            {
                // Try effect-specific positive/negative icons first
                string magnitudeSuffix = statusEffect.magnitude > 0f ? "Positive" : "Negative";
                string spritePath = $"StatusEffectIcons/{iconName}{magnitudeSuffix}";
                Sprite spriteFromResources = Resources.Load<Sprite>(spritePath);
                if (spriteFromResources != null)
                {
                    return spriteFromResources;
                }
                
                // Try global positive/negative icons
                string globalPath = statusEffect.magnitude > 0f ? "StatusEffectIcons/Positive" : "StatusEffectIcons/Negative";
                Sprite globalSprite = Resources.Load<Sprite>(globalPath);
                if (globalSprite != null)
                {
                    return globalSprite;
                }
            }
        }
        
        // Standard Resources.Load fallback
        string standardPath = $"StatusEffectIcons/{iconName}";
        Sprite standardSprite = Resources.Load<Sprite>(standardPath);
        if (standardSprite != null)
        {
            return standardSprite;
        }
        
        // Last resort: Create default colored sprite
        Debug.LogWarning($"Status effect sprite not found: {standardPath}. Using default colored sprite.");
        return CreateDefaultSprite(statusEffect != null ? statusEffect.effectColor : Color.white);
    }
    
    /// <summary>
    /// Get StatusDatabase reference (cached for performance)
    /// </summary>
    private StatusDatabase GetStatusDatabase()
    {
        // Use instance reference first
        if (statusDatabase != null)
        {
            return statusDatabase;
        }
        
        // Use cached static reference
        if (cachedStatusDatabase != null)
        {
            return cachedStatusDatabase;
        }
        
        // Try to load from Resources
        cachedStatusDatabase = Resources.Load<StatusDatabase>("StatusDatabase");
        return cachedStatusDatabase;
    }
    
    /// <summary>
    /// Create a default colored sprite when the actual sprite is missing
    /// </summary>
    private Sprite CreateDefaultSprite(Color color)
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    private void Start()
    {
        // Auto-find components if not assigned
        if (iconImage == null)
            iconImage = GetComponent<Image>();
        
        if (backgroundImage == null)
            backgroundImage = transform.Find("Background")?.GetComponent<Image>();
        
        if (durationText == null)
            durationText = transform.Find("DurationText")?.GetComponent<TextMeshProUGUI>();
        
        if (magnitudeText == null)
            magnitudeText = transform.Find("MagnitudeText")?.GetComponent<TextMeshProUGUI>();
        
        // Find status effect manager (should be on parent)
        statusEffectManager = GetComponentInParent<StatusEffectManager>();
    }
    
    private void Update()
    {
        if (statusEffect != null && statusEffect.isActive)
        {
            UpdateVisuals();
        }
    }
    
    /// <summary>
    /// Setup this icon for a specific status effect
    /// </summary>
    public void SetupStatusEffect(StatusEffect effect)
    {
        statusEffect = effect;
        
        if (effect == null) return;
        
        // Load and set the actual sprite (will automatically use positive/negative icons if available)
        if (iconImage != null)
        {
            Sprite iconSprite = LoadStatusEffectSprite(effect.iconName);
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.color = Color.white; // Reset to white so sprite colors show through
            }
            else
            {
                // Fallback to colored square if sprite not found
                iconImage.color = effect.effectColor;
                Debug.LogWarning($"Status effect sprite not found: {effect.iconName}");
            }
        }
        
        // Set background color based on buff/debuff
        if (backgroundImage != null)
        {
            backgroundImage.color = effect.isDebuff ? debuffColor : buffColor;
        }
        
        // Set magnitude text
        if (magnitudeText != null)
        {
            if (effect.magnitude > 0)
            {
                magnitudeText.text = effect.magnitude.ToString("F0");
                magnitudeText.gameObject.SetActive(true);
            }
            else
            {
                magnitudeText.gameObject.SetActive(false);
            }
        }
        
        UpdateVisuals();
    }
    
    /// <summary>
    /// Update the visual elements of this icon
    /// </summary>
    private void UpdateVisuals()
    {
        if (statusEffect == null) return;
        
        // Update duration text
        if (durationText != null)
        {
            if (statusEffect.duration > 0) // Not permanent
            {
                int remainingTurns = Mathf.CeilToInt(statusEffect.timeRemaining);
                durationText.text = remainingTurns.ToString();
                durationText.gameObject.SetActive(true);
            }
            else
            {
                durationText.gameObject.SetActive(false);
            }
        }
        
        // Update magnitude text if it changed
        if (magnitudeText != null && magnitudeText.gameObject.activeInHierarchy)
        {
            magnitudeText.text = statusEffect.magnitude.ToString("F0");
        }
        
        // Update icon sprite if magnitude changed (for positive/negative icons)
        if (iconImage != null)
        {
            Sprite newSprite = LoadStatusEffectSprite(statusEffect.iconName);
            if (newSprite != null && iconImage.sprite != newSprite)
            {
                iconImage.sprite = newSprite;
            }
        }
    }
    
    /// <summary>
    /// Handle click on this status effect icon
    /// </summary>
    public void OnIconClicked()
    {
        if (statusEffect != null)
        {
            // Show tooltip or detailed information
            ShowStatusEffectTooltip();
        }
    }
    
    /// <summary>
    /// Show detailed information about this status effect
    /// </summary>
    private void ShowStatusEffectTooltip()
    {
        if (statusEffect == null) return;
        
        string tooltipText = $"{statusEffect.effectName}\n";
        tooltipText += $"{statusEffect.description}\n";
        
        if (statusEffect.duration > 0)
        {
            tooltipText += $"Duration: {Mathf.CeilToInt(statusEffect.timeRemaining)} turns\n";
        }
        else
        {
            tooltipText += "Duration: Permanent\n";
        }
        
        if (statusEffect.magnitude > 0)
        {
            tooltipText += $"Magnitude: {statusEffect.magnitude}";
        }
        
        Debug.Log($"Status Effect Tooltip: {tooltipText}");
        
        // In a real implementation, you'd show this in a UI tooltip
        // TooltipManager.Instance.ShowTooltip(tooltipText, transform.position);
    }
    
    /// <summary>
    /// Remove this status effect when clicked (for testing)
    /// </summary>
    public void RemoveStatusEffect()
    {
        if (statusEffect != null && statusEffectManager != null)
        {
            statusEffectManager.RemoveStatusEffect(statusEffect);
        }
    }
    
    /// <summary>
    /// Get the current status effect
    /// </summary>
    public StatusEffect GetStatusEffect()
    {
        return statusEffect;
    }
}
