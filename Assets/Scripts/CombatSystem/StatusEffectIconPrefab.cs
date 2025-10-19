using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple prefab script for status effect icons
/// This can be used as a template for creating actual prefabs
/// </summary>
public class StatusEffectIconPrefab : MonoBehaviour
{
    [Header("UI Components")]
    public Image backgroundImage;
    public Image iconImage;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI magnitudeText;
    
    [Header("Visual Settings")]
    public Color buffColor = Color.green;
    public Color debuffColor = Color.red;
    
    private StatusEffect statusEffect;
    
    private void Awake()
    {
        // Auto-find components if not assigned
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
        
        if (durationText == null)
            durationText = transform.Find("DurationText")?.GetComponent<TextMeshProUGUI>();
        
        if (magnitudeText == null)
            magnitudeText = transform.Find("MagnitudeText")?.GetComponent<TextMeshProUGUI>();
        
        // Set up size
        GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
    }
    
    /// <summary>
    /// Setup this icon for a status effect
    /// </summary>
    public void SetupStatusEffect(StatusEffect effect)
    {
        statusEffect = effect;
        
        if (effect == null) return;
        
        // Set background color based on buff/debuff
        if (backgroundImage != null)
        {
            backgroundImage.color = effect.isDebuff ? debuffColor : buffColor;
        }
        
        // Set icon color
        if (iconImage != null)
        {
            iconImage.color = effect.effectColor;
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
        
        // Set duration text
        if (durationText != null)
        {
            if (effect.duration > 0)
            {
                int remainingTurns = Mathf.CeilToInt(effect.timeRemaining);
                durationText.text = remainingTurns.ToString();
                durationText.gameObject.SetActive(true);
            }
            else
            {
                durationText.gameObject.SetActive(false);
            }
        }
    }
    
    private void Update()
    {
        if (statusEffect != null && statusEffect.isActive)
        {
            UpdateVisuals();
        }
    }
    
    /// <summary>
    /// Update visual elements
    /// </summary>
    private void UpdateVisuals()
    {
        // Update duration text
        if (durationText != null && durationText.gameObject.activeInHierarchy)
        {
            int remainingTurns = Mathf.CeilToInt(statusEffect.timeRemaining);
            durationText.text = remainingTurns.ToString();
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
            string tooltip = $"{statusEffect.effectName}\n";
            tooltip += $"{statusEffect.description}\n";
            
            if (statusEffect.duration > 0)
            {
                tooltip += $"Duration: {Mathf.CeilToInt(statusEffect.timeRemaining)} turns\n";
            }
            else
            {
                tooltip += "Duration: Permanent\n";
            }
            
            if (statusEffect.magnitude > 0)
            {
                tooltip += $"Magnitude: {statusEffect.magnitude}";
            }
            
            Debug.Log($"Status Effect Tooltip: {tooltip}");
        }
    }
}
