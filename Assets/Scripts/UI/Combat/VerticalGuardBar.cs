using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Vertical guard bar that overlays on top of energy shield and health bars.
/// Guard provides temporary protection that reduces incoming damage.
/// </summary>
[RequireComponent(typeof(Image))]
public class VerticalGuardBar : MonoBehaviour
{
    [Header("Bar Configuration")]
    [SerializeField] private Image fillImage;
    
    [Header("Colors")]
    [SerializeField] private Color barColor = new Color(1f, 1f, 0f, 0.6f); // Yellow with semi-transparency
    [SerializeField] private bool useGradient = false;
    [SerializeField] private Gradient guardGradient;
    
    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;
    
    [Header("Effects")]
    [SerializeField] private bool flashOnDamage = true;
    [SerializeField] private Color damageFlashColor = Color.yellow;
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
    /// Set guard amount (0-1).
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
        
        if (useGradient && guardGradient != null)
        {
            // Use gradient
            fillImage.color = guardGradient.Evaluate(fillAmount);
        }
        else
        {
            // Use solid color
            fillImage.color = barColor;
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
                Color c = fillImage.color;
                c.a = alpha;
                fillImage.color = c;
            }
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
    
    #endregion
}
