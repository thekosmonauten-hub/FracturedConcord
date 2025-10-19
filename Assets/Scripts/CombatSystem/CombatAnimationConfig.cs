using UnityEngine;

/// <summary>
/// ScriptableObject configuration for combat animations.
/// Allows designers to tweak animation timing, easing, and appearance without code changes.
/// Create via: Assets > Create > Combat > Animation Config
/// </summary>
[CreateAssetMenu(fileName = "CombatAnimationConfig", menuName = "Combat/Animation Config", order = 1)]
public class CombatAnimationConfig : ScriptableObject
{
    [Header("Damage Numbers")]
    [Tooltip("Base font size for damage numbers")]
    [Range(20, 100)]
    public int baseDamageFontSize = 48;
    
    [Tooltip("How high damage numbers float")]
    [Range(20f, 200f)]
    public float damageFloatHeight = 100f;
    
    [Tooltip("Horizontal spread of damage numbers")]
    [Range(0f, 100f)]
    public float damageFloatHorizontalRange = 30f;
    
    [Tooltip("Duration of damage pop-in animation")]
    [Range(0.1f, 1f)]
    public float damagePopDuration = 0.2f;
    
    [Tooltip("Duration of damage float animation")]
    [Range(0.3f, 2f)]
    public float damageFloatDuration = 0.8f;
    
    [Tooltip("Duration of damage fade-out animation")]
    [Range(0.1f, 1f)]
    public float damageFadeDuration = 0.3f;
    
    [Tooltip("Easing curve for damage pop")]
    public LeanTweenType damagePopEase = LeanTweenType.easeOutBack;
    
    [Tooltip("Easing curve for damage float")]
    public LeanTweenType damageFloatEase = LeanTweenType.easeOutQuad;
    
    [Header("Damage Colors")]
    public Color normalDamageColor = new Color(1f, 0.3f, 0.3f); // Red
    public Color criticalDamageColor = new Color(1f, 0.8f, 0f); // Yellow/Orange
    public Color healColor = new Color(0.2f, 1f, 0.2f); // Green
    
    [Header("Health/Mana Bars")]
    [Tooltip("Duration of health bar fill animation")]
    [Range(0.1f, 1f)]
    public float healthBarDuration = 0.4f;
    
    [Tooltip("Duration of mana bar fill animation")]
    [Range(0.1f, 1f)]
    public float manaBarDuration = 0.3f;
    
    [Tooltip("Easing for health bar animations")]
    public LeanTweenType healthBarEase = LeanTweenType.easeOutQuad;
    
    [Tooltip("Easing for mana bar animations")]
    public LeanTweenType manaBarEase = LeanTweenType.easeInOutQuad;
    
    [Header("Health Bar Colors")]
    public Color healthColorHigh = new Color(0f, 0.8f, 0.4f); // Green
    public Color healthColorMedium = new Color(1f, 0.8f, 0f); // Yellow
    public Color healthColorLow = new Color(1f, 0.2f, 0.2f); // Red
    
    [Header("Card Animations")]
    [Tooltip("Duration of card draw animation")]
    [Range(0.2f, 1f)]
    public float cardDrawDuration = 0.5f;
    
    [Tooltip("Duration of card play animation")]
    [Range(0.2f, 1f)]
    public float cardPlayDuration = 0.6f;
    
    [Tooltip("Duration of card discard animation")]
    [Range(0.1f, 0.5f)]
    public float cardDiscardDuration = 0.3f;
    
    [Tooltip("Duration of card hover animation")]
    [Range(0.05f, 0.3f)]
    public float cardHoverDuration = 0.15f;
    
    [Tooltip("Scale multiplier when hovering over card")]
    [Range(1f, 1.5f)]
    public float cardHoverScale = 1.1f;
    
    [Tooltip("How much card lifts when hovered")]
    [Range(0f, 50f)]
    public float cardHoverLift = 20f;
    
    [Tooltip("Easing for card draw")]
    public LeanTweenType cardDrawEase = LeanTweenType.easeOutQuad;
    
    [Tooltip("Easing for card play")]
    public LeanTweenType cardPlayEase = LeanTweenType.easeInQuad;
    
    [Tooltip("Easing for card discard")]
    public LeanTweenType cardDiscardEase = LeanTweenType.easeInQuad;
    
    [Header("Screen Effects")]
    [Tooltip("Duration of screen shake")]
    [Range(0.1f, 1f)]
    public float screenShakeDuration = 0.3f;
    
    [Tooltip("Intensity of screen shake")]
    [Range(0.1f, 5f)]
    public float screenShakeMagnitude = 0.5f;
    
    [Header("Turn Transitions")]
    [Tooltip("Duration of turn transition animation")]
    [Range(0.3f, 2f)]
    public float turnTransitionDuration = 0.8f;
    
    [Tooltip("Easing for turn transitions")]
    public LeanTweenType turnTransitionEase = LeanTweenType.easeOutBack;
    
    [Header("Timing")]
    [Tooltip("Delay between sequential animations (card draws, etc.)")]
    [Range(0f, 0.5f)]
    public float sequenceDelay = 0.1f;
    
    [Tooltip("Delay before enemy turn starts")]
    [Range(0.5f, 2f)]
    public float enemyTurnDelay = 1f;
    
    [Header("Presets")]
    [Tooltip("Apply preset animation speeds")]
    public AnimationSpeedPreset speedPreset = AnimationSpeedPreset.Normal;
    
    /// <summary>
    /// Apply preset values based on selected speed
    /// </summary>
    [ContextMenu("Apply Speed Preset")]
    public void ApplySpeedPreset()
    {
        switch (speedPreset)
        {
            case AnimationSpeedPreset.VeryFast:
                ApplyVeryFastPreset();
                break;
            case AnimationSpeedPreset.Fast:
                ApplyFastPreset();
                break;
            case AnimationSpeedPreset.Normal:
                ApplyNormalPreset();
                break;
            case AnimationSpeedPreset.Slow:
                ApplySlowPreset();
                break;
            case AnimationSpeedPreset.Cinematic:
                ApplyCinematicPreset();
                break;
        }
        
        Debug.Log($"Applied {speedPreset} animation preset");
    }
    
    private void ApplyVeryFastPreset()
    {
        damagePopDuration = 0.1f;
        damageFloatDuration = 0.4f;
        damageFadeDuration = 0.2f;
        healthBarDuration = 0.2f;
        manaBarDuration = 0.15f;
        cardDrawDuration = 0.3f;
        cardPlayDuration = 0.3f;
        cardDiscardDuration = 0.15f;
        cardHoverDuration = 0.08f;
        screenShakeDuration = 0.15f;
        turnTransitionDuration = 0.4f;
        sequenceDelay = 0.05f;
        enemyTurnDelay = 0.5f;
    }
    
    private void ApplyFastPreset()
    {
        damagePopDuration = 0.15f;
        damageFloatDuration = 0.6f;
        damageFadeDuration = 0.25f;
        healthBarDuration = 0.3f;
        manaBarDuration = 0.2f;
        cardDrawDuration = 0.4f;
        cardPlayDuration = 0.4f;
        cardDiscardDuration = 0.2f;
        cardHoverDuration = 0.1f;
        screenShakeDuration = 0.2f;
        turnTransitionDuration = 0.6f;
        sequenceDelay = 0.08f;
        enemyTurnDelay = 0.8f;
    }
    
    private void ApplyNormalPreset()
    {
        damagePopDuration = 0.2f;
        damageFloatDuration = 0.8f;
        damageFadeDuration = 0.3f;
        healthBarDuration = 0.4f;
        manaBarDuration = 0.3f;
        cardDrawDuration = 0.5f;
        cardPlayDuration = 0.6f;
        cardDiscardDuration = 0.3f;
        cardHoverDuration = 0.15f;
        screenShakeDuration = 0.3f;
        turnTransitionDuration = 0.8f;
        sequenceDelay = 0.1f;
        enemyTurnDelay = 1f;
    }
    
    private void ApplySlowPreset()
    {
        damagePopDuration = 0.3f;
        damageFloatDuration = 1.2f;
        damageFadeDuration = 0.4f;
        healthBarDuration = 0.6f;
        manaBarDuration = 0.4f;
        cardDrawDuration = 0.7f;
        cardPlayDuration = 0.8f;
        cardDiscardDuration = 0.4f;
        cardHoverDuration = 0.2f;
        screenShakeDuration = 0.4f;
        turnTransitionDuration = 1.2f;
        sequenceDelay = 0.15f;
        enemyTurnDelay = 1.5f;
    }
    
    private void ApplyCinematicPreset()
    {
        damagePopDuration = 0.4f;
        damageFloatDuration = 1.5f;
        damageFadeDuration = 0.5f;
        healthBarDuration = 0.8f;
        manaBarDuration = 0.6f;
        cardDrawDuration = 1f;
        cardPlayDuration = 1.2f;
        cardDiscardDuration = 0.5f;
        cardHoverDuration = 0.25f;
        screenShakeDuration = 0.6f;
        turnTransitionDuration = 1.5f;
        sequenceDelay = 0.2f;
        enemyTurnDelay = 2f;
    }
    
    /// <summary>
    /// Validate values to ensure they're within reasonable ranges
    /// </summary>
    private void OnValidate()
    {
        // Ensure positive durations
        damagePopDuration = Mathf.Max(0.1f, damagePopDuration);
        damageFloatDuration = Mathf.Max(0.1f, damageFloatDuration);
        damageFadeDuration = Mathf.Max(0.1f, damageFadeDuration);
        healthBarDuration = Mathf.Max(0.1f, healthBarDuration);
        manaBarDuration = Mathf.Max(0.1f, manaBarDuration);
        cardDrawDuration = Mathf.Max(0.1f, cardDrawDuration);
        cardPlayDuration = Mathf.Max(0.1f, cardPlayDuration);
        cardDiscardDuration = Mathf.Max(0.1f, cardDiscardDuration);
        cardHoverDuration = Mathf.Max(0.05f, cardHoverDuration);
        screenShakeDuration = Mathf.Max(0.1f, screenShakeDuration);
        turnTransitionDuration = Mathf.Max(0.1f, turnTransitionDuration);
        
        // Ensure positive values
        damageFloatHeight = Mathf.Max(10f, damageFloatHeight);
        damageFloatHorizontalRange = Mathf.Max(0f, damageFloatHorizontalRange);
        baseDamageFontSize = Mathf.Max(20, baseDamageFontSize);
        screenShakeMagnitude = Mathf.Max(0.1f, screenShakeMagnitude);
        cardHoverScale = Mathf.Max(1f, cardHoverScale);
        cardHoverLift = Mathf.Max(0f, cardHoverLift);
    }
}

/// <summary>
/// Predefined animation speed presets
/// </summary>
public enum AnimationSpeedPreset
{
    VeryFast,   // Snappy, arcade-style
    Fast,       // Quick but readable
    Normal,     // Balanced for most players
    Slow,       // Deliberate, strategic feel
    Cinematic   // Dramatic, movie-like
}

