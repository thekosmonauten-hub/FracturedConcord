using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple vertical health/mana bar that drains from top to bottom.
/// Much simpler than CircularHealthBar - just uses Image fill vertical.
/// </summary>
[RequireComponent(typeof(Image))]
public class VerticalHealthBar : MonoBehaviour
{
    [Header("Bar Configuration")]
    [SerializeField] private BarType barType = BarType.Health;
    [SerializeField] private Image fillImage;
    
    [Header("Colors")]
    [SerializeField] private Color barColor = new Color(0.78f, 0.02f, 0f, 1f); // #C70500 red (default for health)
    [SerializeField] private bool useGradient = false;
    [SerializeField] private Gradient healthGradient;
    
    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;
    
    [Header("Effects")]
    [SerializeField] private bool flashOnDamage = true;
    [SerializeField] private Color damageFlashColor = Color.white;
    [SerializeField] private bool pulseOnLow = true;
    [SerializeField] private float lowThreshold = 0.25f;
    
    // State
    private float currentFillAmount = 1f;
    private int activeTweenId = -1;
    
    private void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponent<Image>();
        
        SetupVerticalFill();
        ForceHealthBarColor(); // Force the health bar color to prevent green override
        UpdateColor(1f);
    }
    
    private void OnValidate()
    {
        if (fillImage == null)
            fillImage = GetComponent<Image>();
        
        SetupVerticalFill();
        UpdateColor(fillImage != null ? fillImage.fillAmount : 1f);
    }
    
    private void SetupVerticalFill()
    {
        if (fillImage == null) return;
        
        // Configure as VERTICAL fill (bottom to top)
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Vertical;
        fillImage.fillOrigin = (int)Image.OriginVertical.Bottom; // Fill from bottom up
        fillImage.fillAmount = 1f;
    }
    
    /// <summary>
    /// Set health/mana amount (0-1).
    /// </summary>
    public void SetFillAmount(float fillAmount, bool animate = true)
    {
        fillAmount = Mathf.Clamp01(fillAmount);
        
        // In Edit Mode, update instantly (LeanTween doesn't work in Edit Mode)
        if (!Application.isPlaying)
        {
            currentFillAmount = fillAmount;
            UpdateFillVisual(fillAmount);
            return;
        }
        
        // In Play Mode, animate smoothly
        if (animate)
        {
            AnimateFill(fillAmount);
        }
        else
        {
            currentFillAmount = fillAmount;
            UpdateFillVisual(fillAmount);
        }
    }
    
    private void AnimateFill(float targetFillAmount)
    {
        // Cancel existing animation
        if (activeTweenId >= 0)
        {
            LeanTween.cancel(activeTweenId);
        }
        
        // Determine if damage or heal
        bool isDamage = targetFillAmount < currentFillAmount;
        
        // Animate fill amount
        activeTweenId = LeanTween.value(gameObject, currentFillAmount, targetFillAmount, animationDuration)
            .setEase(easeType)
            .setOnUpdate((float value) => {
                UpdateFillVisual(value);
            })
            .setOnComplete(() => {
                currentFillAmount = targetFillAmount;
                activeTweenId = -1;
                
                // Flash effect on damage
                if (isDamage && flashOnDamage)
                {
                    FlashDamage();
                }
            }).id;
    }
    
    private void UpdateFillVisual(float fillAmount)
    {
        if (fillImage == null) return;
        
        fillImage.fillAmount = fillAmount;
        UpdateColor(fillAmount);
    }
    
    private void UpdateColor(float fillAmount)
    {
        if (fillImage == null) return;
        
        if (useGradient && healthGradient != null)
        {
            // Use gradient
            fillImage.color = healthGradient.Evaluate(fillAmount);
        }
        else
        {
            // Use solid color - force the health bar color
            fillImage.color = barColor;
        }
        
        // Always force the color to prevent external overrides
        ForceHealthBarColor();
    }
    
    /// <summary>
    /// Forces the bar to use the correct color based on bar type and prevents color override.
    /// </summary>
    private void ForceHealthBarColor()
    {
        if (fillImage == null) return;
        
        // Set color based on bar type
        if (barType == BarType.Mana)
        {
            // Force the mana bar color to blue (#4879F3)
            fillImage.color = new Color(0.28f, 0.47f, 0.95f, 1f);
        }
        else
        {
            // Force the health bar color to red (#C70500)
            fillImage.color = new Color(0.78f, 0.02f, 0f, 1f);
        }
    }
    
    private void FlashDamage()
    {
        if (fillImage == null) return;
        
        Color originalColor = fillImage.color;
        
        LeanTween.value(gameObject, 0f, 1f, 0.2f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float t) => {
                fillImage.color = Color.Lerp(damageFlashColor, originalColor, t);
            });
    }
    
    private void Update()
    {
        // Pulse effect when low
        if (pulseOnLow && currentFillAmount <= lowThreshold)
        {
            if (fillImage != null)
            {
                float pulse = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f; // 0-1 oscillation
                float alpha = Mathf.Lerp(0.6f, 1f, pulse);
                
                // Keep the correct color based on bar type but adjust alpha only
                Color baseColor = barType == BarType.Mana 
                    ? new Color(0.28f, 0.47f, 0.95f, alpha) // Blue for mana
                    : new Color(0.78f, 0.02f, 0f, alpha);   // Red for health
                    
                fillImage.color = baseColor;
            }
        }
        else
        {
            // Ensure color stays correct when not pulsing
            ForceHealthBarColor();
        }
    }
    
    #region Context Menu
    
    [ContextMenu("Set to Full")]
    private void SetToFull()
    {
        SetFillAmount(1f);
    }
    
    [ContextMenu("Set to Half")]
    private void SetToHalf()
    {
        SetFillAmount(0.5f);
    }
    
    [ContextMenu("Set to Low")]
    private void SetToLow()
    {
        SetFillAmount(0.2f);
    }
    
    [ContextMenu("Set to Empty")]
    private void SetToEmpty()
    {
        SetFillAmount(0f);
    }
    
    [ContextMenu("Test Damage Flash")]
    private void TestDamageFlash()
    {
        SetFillAmount(Mathf.Max(0f, currentFillAmount - 0.2f));
    }
    
    [ContextMenu("Force Correct Color")]
    private void DebugForceCorrectColor()
    {
        ForceHealthBarColor();
        string colorName = barType == BarType.Mana ? "blue (#4879F3)" : "red (#C70500)";
        string barName = barType == BarType.Mana ? "Mana" : "Health";
        Debug.Log($"{barName} bar color forced to {colorName}");
    }
    
    #endregion
}

// Note: BarType enum is defined in CircularHealthBar.cs (shared)

