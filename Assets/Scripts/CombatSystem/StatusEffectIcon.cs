using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    
    private StatusEffect statusEffect;
    private StatusEffectManager statusEffectManager;
    
    
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
        
        // Update icon sprite
        if (iconImage != null)
        {
            Sprite iconSprite = LoadStatusEffectSprite(statusEffect.iconName);
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
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
    /// </summary>
    private Sprite LoadStatusEffectSprite(string iconName)
    {
        // Try to load from Resources folder
        string spritePath = $"StatusEffectIcons/{iconName}";
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        
        if (sprite == null)
        {
            Debug.LogWarning($"Status effect sprite not found: {spritePath}");
            // Return a default sprite or create a colored square
            return CreateDefaultSprite(statusEffect.effectColor);
        }
        
        return sprite;
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
        
        // Load and set the actual sprite
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
