using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Circular health/mana bar that drains from top to bottom (or any direction).
/// Works seamlessly with CombatAnimationManager for smooth animations.
/// </summary>
[RequireComponent(typeof(Image))]
public class CircularHealthBar : MonoBehaviour
{
    [Header("Bar Configuration")]
    [SerializeField] private BarType barType = BarType.Health;
    [SerializeField] private Image fillImage;
    
    [Header("Fill Direction")]
    [Tooltip("Which direction the bar drains from. Top = drains downward")]
    [SerializeField] private FillOrigin fillOrigin = FillOrigin.Top;
    [Tooltip("For top-to-bottom drain, use counter-clockwise (false)")]
    [SerializeField] private bool clockwise = false;
    
    [Header("Colors")]
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private Color manaColor = new Color(0.3f, 0.5f, 1f);
    
    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;
    
    [Header("Effects")]
    [SerializeField] private bool pulseOnLow = true;
    [SerializeField] private float lowThreshold = 0.25f;
    [SerializeField] private GameObject warningEffect;
    
    // State
    private float currentFillAmount = 1f;
    private float targetFillAmount = 1f;
    private int activeTweenId = -1;
    
    private void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponent<Image>();
        
        SetupRadialFill();
        InitializeGradient();
    }
    
    private void OnValidate()
    {
        // Called when values change in Inspector (Edit Mode)
        if (fillImage == null)
            fillImage = GetComponent<Image>();
        
        SetupRadialFill();
        
        // Update visual in Edit Mode
        if (!Application.isPlaying && fillImage != null)
        {
            UpdateFillVisual(fillImage.fillAmount);
        }
    }
    
    private void SetupRadialFill()
    {
        if (fillImage == null) return;
        
        // Configure as radial fill
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillOrigin = (int)fillOrigin;
        fillImage.fillClockwise = clockwise;
        fillImage.fillAmount = 1f;
    }
    
    private void InitializeGradient()
    {
        if (healthGradient == null || healthGradient.colorKeys.Length == 0)
        {
            // Create default health gradient (green → yellow → red)
            healthGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.red, 0f);      // 0% = red
            colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f); // 50% = yellow
            colorKeys[2] = new GradientColorKey(Color.green, 1f);    // 100% = green
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            
            healthGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    /// <summary>
    /// Set the fill amount (0 to 1) with smooth animation
    /// </summary>
    public void SetFillAmount(float current, float max)
    {
        float fillAmount = Mathf.Clamp01(current / max);
        SetFillAmount(fillAmount);
    }
    
    /// <summary>
    /// Set the fill amount directly (0 to 1)
    /// </summary>
    public void SetFillAmount(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount);
        targetFillAmount = fillAmount;
        
        // In Edit Mode, update instantly (LeanTween doesn't work in Edit Mode)
        if (!Application.isPlaying)
        {
            currentFillAmount = fillAmount;
            UpdateFillVisual(fillAmount);
            return;
        }
        
        // Cancel existing animation
        if (activeTweenId != -1)
        {
            LeanTween.cancel(activeTweenId);
            activeTweenId = -1;
        }
        
        // Animate fill amount
        activeTweenId = LeanTween.value(gameObject, currentFillAmount, targetFillAmount, animationDuration)
            .setEase(easeType)
            .setOnUpdate((float val) => {
                currentFillAmount = val;
                UpdateFillVisual(val);
            })
            .setOnComplete(() => {
                activeTweenId = -1;
            }).id;
        
        // Check for low health warning
        if (pulseOnLow && fillAmount <= lowThreshold && fillAmount > 0)
        {
            StartLowPulse();
        }
        else
        {
            StopLowPulse();
        }
    }
    
    /// <summary>
    /// Set fill instantly without animation
    /// </summary>
    public void SetFillInstant(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount);
        currentFillAmount = fillAmount;
        targetFillAmount = fillAmount;
        UpdateFillVisual(fillAmount);
    }
    
    private void UpdateFillVisual(float fillAmount)
    {
        if (fillImage == null) return;
        
        // Update fill amount
        fillImage.fillAmount = fillAmount;
        
        // Update color based on type
        if (barType == BarType.Health)
        {
            fillImage.color = healthGradient.Evaluate(fillAmount);
        }
        else
        {
            fillImage.color = manaColor;
        }
    }
    
    private void StartLowPulse()
    {
        if (fillImage == null) return;
        
        // Pulse effect when low
        LeanTween.cancel(gameObject, false);
        LeanTween.alpha(fillImage.rectTransform, 0.5f, 0.5f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong();
        
        // Show warning effect
        if (warningEffect != null)
            warningEffect.SetActive(true);
    }
    
    private void StopLowPulse()
    {
        if (fillImage == null) return;
        
        LeanTween.cancel(gameObject, false);
        
        Color c = fillImage.color;
        c.a = 1f;
        fillImage.color = c;
        
        // Hide warning effect
        if (warningEffect != null)
            warningEffect.SetActive(false);
    }
    
    /// <summary>
    /// Flash effect when taking damage/healing
    /// </summary>
    public void Flash(Color flashColor, float duration = 0.2f)
    {
        if (fillImage == null) return;
        
        Color originalColor = fillImage.color;
        
        LeanTween.value(gameObject, 0f, 1f, duration)
            .setOnUpdate((float t) => {
                fillImage.color = Color.Lerp(flashColor, originalColor, t);
            });
    }
    
    /// <summary>
    /// Shake effect for impact
    /// </summary>
    public void Shake(float intensity = 10f, float duration = 0.3f)
    {
        Vector3 originalPos = transform.localPosition;
        
        LeanTween.value(gameObject, 0f, 1f, duration)
            .setOnUpdate((float progress) => {
                float damping = 1f - progress;
                Vector3 shake = Random.insideUnitCircle * intensity * damping;
                transform.localPosition = originalPos + shake;
            })
            .setOnComplete(() => {
                transform.localPosition = originalPos;
            })
            .setEase(LeanTweenType.easeOutQuad);
    }
    
    private void OnDestroy()
    {
        if (activeTweenId != -1)
        {
            LeanTween.cancel(activeTweenId);
        }
        LeanTween.cancel(gameObject);
    }
    
    #region Inspector Helpers
    
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
    
    [ContextMenu("Fix Fill Direction (Top to Bottom)")]
    private void FixFillDirection()
    {
        fillOrigin = FillOrigin.Top;
        clockwise = false;
        SetupRadialFill();
        Debug.Log($"✓ Fixed {gameObject.name} to drain top-to-bottom (counter-clockwise)");
    }
    
    [ContextMenu("Set to Low")]
    private void SetToLow()
    {
        SetFillAmount(0.2f);
    }
    
    [ContextMenu("Test Flash")]
    private void TestFlash()
    {
        Flash(Color.white);
    }
    
    [ContextMenu("Test Shake")]
    private void TestShake()
    {
        Shake();
    }
    
    #endregion
}

public enum BarType
{
    Health,
    Mana
}

public enum FillOrigin
{
    Bottom = 0,
    Right = 1,
    Top = 2,
    Left = 3
}

