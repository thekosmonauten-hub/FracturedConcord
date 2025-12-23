using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages all combat animations using LeanTween.
/// Provides smooth, professional animations for damage, effects, UI, and more.
/// 
/// Phase 2: Animation Kill Switch - Set disableAllAnimations to true to disable all animations.
/// This validates that game logic is independent of visuals (best practice).
/// If gameplay breaks when animations are disabled, logic is still tied to visuals.
/// </summary>
public class CombatAnimationManager : MonoBehaviour
{
    public static CombatAnimationManager Instance { get; private set; }
    
    [Header("Animation Config")]
    public CombatAnimationConfig config;
    
    [Header("Phase 2: Animation Kill Switch (Debug)")]
    [Tooltip("When enabled, all animations are skipped and callbacks fire immediately. " +
             "Use this to validate that game logic works independently of animations. " +
             "If gameplay breaks when this is enabled, logic is still tied to visuals - fix before adding more content.")]
    public static bool disableAllAnimations = false;
    
    [Header("Damage Number Pool")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Transform damageNumberParent;
    [SerializeField] private int damageNumberPoolSize = 20;
    private Queue<GameObject> damageNumberPool = new Queue<GameObject>();
    
    [Header("Particle Pool")]
    [SerializeField] private GameObject impactParticlePrefab;
    [SerializeField] private GameObject healParticlePrefab;
    [SerializeField] private int particlePoolSize = 10;
    private Queue<GameObject> impactParticlePool = new Queue<GameObject>();
    private Queue<GameObject> healParticlePool = new Queue<GameObject>();
    
    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;
    
    // Active animations tracking
    private List<int> activeTweens = new List<int>();
    private Dictionary<GameObject, int> objectTweens = new Dictionary<GameObject, int>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Auto-find camera if not set
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Load default config if none provided
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<CombatAnimationConfig>();
            Debug.LogWarning("No CombatAnimationConfig assigned! Using default values.");
        }
        
        InitializePools();
    }
    
    private void InitializePools()
    {
        // Initialize damage number pool
        if (damageNumberPrefab != null)
        {
            if (damageNumberParent == null)
            {
                GameObject poolParent = new GameObject("DamageNumberPool");
                poolParent.transform.SetParent(transform);
                damageNumberParent = poolParent.transform;
            }
            
            for (int i = 0; i < damageNumberPoolSize; i++)
            {
                GameObject damageNumber = Instantiate(damageNumberPrefab, damageNumberParent);
                damageNumber.SetActive(false);
                damageNumberPool.Enqueue(damageNumber);
            }
            
            Debug.Log($"Damage number pool initialized with {damageNumberPoolSize} objects");
        }
        else
        {
            Debug.LogWarning("Damage number prefab not assigned! Damage numbers will be created procedurally.");
        }
        
        // Initialize particle pools
        if (impactParticlePrefab != null)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                GameObject particle = Instantiate(impactParticlePrefab, transform);
                particle.SetActive(false);
                impactParticlePool.Enqueue(particle);
            }
        }
        
        if (healParticlePrefab != null)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                GameObject particle = Instantiate(healParticlePrefab, transform);
                particle.SetActive(false);
                healParticlePool.Enqueue(particle);
            }
        }
    }
    
    #region Damage Number Animations
    
    /// <summary>
    /// Show floating damage number at world or screen position
    /// </summary>
    public void ShowDamageNumber(float damage, Vector3 worldPosition, DamageNumberType type = DamageNumberType.Normal)
    {
        // Convert world position to screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        
        // Get damage number from pool or create new one
        GameObject damageObj = GetDamageNumber();
        Text damageText = damageObj.GetComponent<Text>();
        
        if (damageText == null)
        {
            damageText = damageObj.AddComponent<Text>();
            damageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            damageText.alignment = TextAnchor.MiddleCenter;
        }
        
        // Configure appearance based on type
        ConfigureDamageNumber(damageText, damage, type);
        
        // Position
        RectTransform rectTransform = damageObj.GetComponent<RectTransform>();
        rectTransform.position = screenPos;
        
        // Animate
        AnimateDamageNumber(damageObj, rectTransform, type);
    }

    /// <summary>
    /// Show custom floating combat text (non-numeric)
    /// </summary>
    public void ShowFloatingText(string message, Vector3 worldPosition, Color color, int fontSize = 36, DamageNumberType animationType = DamageNumberType.Normal)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);

        GameObject textObj = GetDamageNumber();
        Text textComponent = textObj.GetComponent<Text>();
        if (textComponent == null)
        {
            textComponent = textObj.AddComponent<Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.alignment = TextAnchor.MiddleCenter;
        }

        textComponent.text = message;
        textComponent.fontSize = fontSize;
        textComponent.color = color;

        Outline outline = textObj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = textObj.AddComponent<Outline>();
        }
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.position = screenPos;

        AnimateDamageNumber(textObj, rectTransform, animationType);
    }
    
    private void ConfigureDamageNumber(Text damageText, float damage, DamageNumberType type)
    {
        damageText.text = Mathf.RoundToInt(damage).ToString();
        damageText.fontSize = GetDamageFontSize(damage, type);
        damageText.color = GetDamageColor(type);
        
        // Add outline for better visibility
        Outline outline = damageText.GetComponent<Outline>();
        if (outline == null)
        {
            outline = damageText.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
    }
    
    private int GetDamageFontSize(float damage, DamageNumberType type)
    {
        int baseSize = config.baseDamageFontSize;
        
        if (type == DamageNumberType.Critical)
        {
            baseSize = Mathf.RoundToInt(baseSize * 1.5f);
        }
        
        // Scale font size based on damage magnitude
        if (damage > 50)
        {
            baseSize = Mathf.RoundToInt(baseSize * 1.2f);
        }
        
        return baseSize;
    }
    
    private Color GetDamageColor(DamageNumberType type)
    {
        switch (type)
        {
            case DamageNumberType.Normal:
                return config.normalDamageColor;
            case DamageNumberType.Critical:
                return config.criticalDamageColor;
            case DamageNumberType.Fire:
                return new Color(1f, 0.5f, 0f); // Orange
            case DamageNumberType.Cold:
                return new Color(0.3f, 0.7f, 1f); // Light blue
            case DamageNumberType.Lightning:
                return new Color(0.5f, 0.5f, 1f); // Purple-blue
            case DamageNumberType.Heal:
                return config.healColor;
            case DamageNumberType.Block:
                return new Color(0.5f, 0.5f, 0.5f); // Gray
            default:
                return Color.white;
        }
    }
    
    private void AnimateDamageNumber(GameObject damageObj, RectTransform rectTransform, DamageNumberType type)
    {
        // Start slightly below final position
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 endPos = startPos + new Vector3(
            Random.Range(-config.damageFloatHorizontalRange, config.damageFloatHorizontalRange),
            config.damageFloatHeight,
            0
        );
        
        // Reset scale and alpha
        damageObj.transform.localScale = Vector3.zero;
        Text damageText = damageObj.GetComponent<Text>();
        Color textColor = damageText.color;
        textColor.a = 1f;
        damageText.color = textColor;
        
        // Pop in animation (scale up with bounce)
        int tweenId = LeanTween.scale(damageObj, Vector3.one, config.damagePopDuration)
            .setEase(config.damagePopEase)
            .setOnComplete(() => {
                // Float up animation
                LeanTween.move(rectTransform, endPos, config.damageFloatDuration)
                    .setEase(config.damageFloatEase);
                
                // Fade out animation (starts halfway through float)
                LeanTween.value(damageObj, 1f, 0f, config.damageFadeDuration)
                    .setDelay(config.damageFloatDuration * 0.5f)
                    .setOnUpdate((float alpha) => {
                        Color color = damageText.color;
                        color.a = alpha;
                        damageText.color = color;
                    })
                    .setOnComplete(() => ReturnDamageNumber(damageObj));
            }).id;
        
        // Track tween
        activeTweens.Add(tweenId);
        
        // Critical hit extra effect - scale pulse
        if (type == DamageNumberType.Critical)
        {
            LeanTween.scale(damageObj, Vector3.one * 1.2f, 0.15f)
                .setDelay(config.damagePopDuration)
                .setEase(LeanTweenType.easeOutQuad)
                .setLoopPingPong(1);
        }
    }
    
    private GameObject GetDamageNumber()
    {
        GameObject damageObj;
        
        if (damageNumberPool.Count > 0)
        {
            damageObj = damageNumberPool.Dequeue();
        }
        else
        {
            // Create new one if pool is empty
            if (damageNumberPrefab != null)
            {
                damageObj = Instantiate(damageNumberPrefab, damageNumberParent);
            }
            else
            {
                // Procedurally create damage number
                damageObj = new GameObject("DamageNumber");
                damageObj.transform.SetParent(damageNumberParent);
                RectTransform rectTransform = damageObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(100, 50);
            }
        }
        
        damageObj.SetActive(true);
        return damageObj;
    }
    
    private void ReturnDamageNumber(GameObject damageObj)
    {
        damageObj.SetActive(false);
        damageNumberPool.Enqueue(damageObj);
    }
    
    #endregion
    
    #region Health and Mana Bar Animations
    
    /// <summary>
    /// Smoothly animate health bar fill
    /// </summary>
    public void AnimateHealthBar(Image fillImage, float currentHealth, float maxHealth)
    {
        float targetFillAmount = currentHealth / maxHealth;
        
        // Cancel existing tween on this object
        CancelObjectTween(fillImage.gameObject);
        
        // Animate fill amount
        int tweenId = LeanTween.value(fillImage.gameObject, fillImage.fillAmount, targetFillAmount, config.healthBarDuration)
            .setEase(config.healthBarEase)
            .setOnUpdate((float value) => {
                fillImage.fillAmount = value;
            }).id;
        
        TrackObjectTween(fillImage.gameObject, tweenId);
        
        // Color transition based on health percentage
        Color targetColor = GetHealthBarColor(targetFillAmount);
        LeanTween.value(fillImage.gameObject, fillImage.color, targetColor, config.healthBarDuration)
            .setEase(config.healthBarEase)
            .setOnUpdate((Color color) => {
                fillImage.color = color;
            });
    }
    
    /// <summary>
    /// Smoothly animate mana bar fill
    /// </summary>
    public void AnimateManaBar(Image fillImage, float currentMana, float maxMana)
    {
        float targetFillAmount = currentMana / maxMana;
        
        // Cancel existing tween on this object
        CancelObjectTween(fillImage.gameObject);
        
        // Animate fill amount
        int tweenId = LeanTween.value(fillImage.gameObject, fillImage.fillAmount, targetFillAmount, config.manaBarDuration)
            .setEase(config.manaBarEase)
            .setOnUpdate((float value) => {
                fillImage.fillAmount = value;
            }).id;
        
        TrackObjectTween(fillImage.gameObject, tweenId);
    }
    
    private Color GetHealthBarColor(float healthPercentage)
    {
        if (healthPercentage > 0.5f)
            return config.healthColorHigh;
        else if (healthPercentage > 0.25f)
            return config.healthColorMedium;
        else
            return config.healthColorLow;
    }
    
    #endregion
    
    #region Card Animations
    
    /// <summary>
    /// Animate card being drawn from deck
    /// </summary>
    public void AnimateCardDraw(GameObject cardObject, Vector3 startPosition, Vector3 endPosition, Vector3 targetScale, System.Action onComplete = null)
    {
        // Phase 2: Animation Kill Switch - skip animation if disabled
        if (disableAllAnimations)
        {
            Debug.Log($"<color=yellow>[Animation Kill Switch] Skipping AnimateCardDraw for {cardObject.name} - instant positioning</color>");
            cardObject.transform.position = endPosition;
            cardObject.transform.localScale = targetScale;
            cardObject.transform.rotation = Quaternion.identity;
            onComplete?.Invoke();
            return;
        }
        
        cardObject.transform.position = startPosition;
        cardObject.transform.localScale = targetScale * 0.3f; // Start at 30% of target scale
        cardObject.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
        
        // Disable raycasting during animation so card doesn't interfere with mouse
        SetCardRaycastTargets(cardObject, false);
        
        // Disable hover effect and button interactions during animation
        var hoverEffect = cardObject.GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
        {
            hoverEffect.enabled = false;
        }
        
        var button = cardObject.GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.interactable = false;
        }
        
        // Move directly to hand position (no detour - use linear ease for straight path)
        LeanTween.move(cardObject, endPosition, config.cardDrawDuration)
            .setEase(LeanTweenType.easeOutQuad); // Changed from config.cardDrawEase to ensure direct path
        
        // Scale up to TARGET scale (not just Vector3.one!)
        LeanTween.scale(cardObject, targetScale, config.cardDrawDuration)
            .setEase(LeanTweenType.easeOutBack);
        
        // Rotate to upright
        LeanTween.rotate(cardObject, Vector3.zero, config.cardDrawDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => 
            {
                // CRITICAL: Ensure scale is exactly at target (in case animation was slightly off)
                cardObject.transform.localScale = targetScale;
                
                // Re-enable raycasting when animation completes
                SetCardRaycastTargets(cardObject, true);
                
                // Re-enable hover effect and button
                if (hoverEffect != null)
                {
                    hoverEffect.enabled = true;
                    // Store the correct scale for hover effects (now that animation is complete)
                    hoverEffect.StoreBaseScale();
                }
                
                if (button != null)
                {
                    button.interactable = true;
                }
                
                onComplete?.Invoke();
            });
    }
    
    /// <summary>
    /// Enable or disable raycasting on all graphics in a card GameObject
    /// </summary>
    private void SetCardRaycastTargets(GameObject cardObject, bool enabled)
    {
        if (cardObject == null) return;
        
        // Get all Graphic components (Image, TextMeshProUGUI, etc.) in the card and its children
        var graphics = cardObject.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        foreach (var graphic in graphics)
        {
            if (graphic != null)
            {
                graphic.raycastTarget = enabled;
            }
        }
    }
    
    /// <summary>
    /// Legacy overload for backward compatibility (uses Vector3.one as default scale)
    /// </summary>
    public void AnimateCardDraw(GameObject cardObject, Vector3 startPosition, Vector3 endPosition, System.Action onComplete = null)
    {
        AnimateCardDraw(cardObject, startPosition, endPosition, Vector3.one, onComplete);
    }
    
    /// <summary>
    /// Animate card being played - goes to center, shrinks, then callback triggers discard
    /// </summary>
    public void AnimateCardPlay(GameObject cardObject, Vector3 targetPosition, System.Action onComplete = null)
    {
        // Phase 2: Animation Kill Switch - skip animation if disabled
        if (disableAllAnimations)
        {
            Debug.Log($"<color=yellow>[Animation Kill Switch] Skipping AnimateCardPlay for {cardObject.name} - callback firing immediately</color>");
            onComplete?.Invoke();
            return;
        }
        
        Debug.Log($"<color=cyan>▶ AnimateCardPlay START</color>");
        Debug.Log($"  Card: {cardObject.name}");
        Debug.Log($"  From: {cardObject.transform.position}");
        Debug.Log($"  To center, then discard");
        Debug.Log($"  Duration: {config.cardPlayDuration}s");
        Debug.Log($"  Has callback: {onComplete != null}");
        
        Vector3 startPosition = cardObject.transform.position;
        
        // Get screen center position
        Vector3 screenCenter = GetScreenCenterPosition(cardObject);
        Debug.Log($"  Screen center: {screenCenter}");
        
        // Get CanvasGroup for fade effects
        CanvasGroup canvasGroup = cardObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = cardObject.AddComponent<CanvasGroup>();
            Debug.Log($"  Added CanvasGroup to {cardObject.name}");
        }
        
        RectTransform rectTransform = cardObject.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError($"Card {cardObject.name} has no RectTransform! Cannot animate.");
            onComplete?.Invoke();
            return;
        }
        
        // Step 1: Move to screen center
        float moveToCenterDuration = config.cardPlayDuration * 0.4f; // 40% of total time to reach center
        LeanTween.move(rectTransform, screenCenter, moveToCenterDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => {
                Debug.Log($"  Card reached center, starting shrink...");
                
                // Step 2: Shrink at center
                float shrinkDuration = config.cardPlayDuration * 0.3f; // 30% of total time to shrink
                Vector3 currentScale = cardObject.transform.localScale;
                LeanTween.scale(cardObject, currentScale * 0.3f, shrinkDuration)
                    .setEase(LeanTweenType.easeInQuad)
                    .setOnComplete(() => {
                        Debug.Log($"<color=green>▶ AnimateCardPlay COMPLETE - Callback firing!</color>");
                        Debug.Log($"  Card still exists: {cardObject != null}");
                        Debug.Log($"  Card active: {(cardObject != null ? cardObject.activeInHierarchy.ToString() : "null")}");
                        
                        // Reset alpha to full for next animation phase
                        if (canvasGroup != null)
                            canvasGroup.alpha = 1f;
                            
                        // DON'T destroy the card - let the caller (CombatDeckManager) handle it!
                        Debug.Log($"  About to invoke callback...");
                        onComplete?.Invoke();
                        Debug.Log($"  Callback invoked!");
                    });
            });
        
        // Scale down as it moves to center (parallel animation)
        Vector3 currentScale = cardObject.transform.localScale;
        LeanTween.scale(cardObject, currentScale * 0.7f, moveToCenterDuration)
            .setEase(LeanTweenType.easeInQuad);
        
        // Slight rotation for effect (parallel animation)
        LeanTween.rotate(cardObject, new Vector3(0, 0, Random.Range(-15f, 15f)), moveToCenterDuration)
            .setEase(LeanTweenType.easeOutQuad);
        
        // Fade animation (parallel) - slight fade as it moves to center
        LeanTween.value(cardObject, 1f, 0.9f, moveToCenterDuration)
            .setOnUpdate((float alpha) => {
                if (canvasGroup != null)
                    canvasGroup.alpha = alpha;
            });
    }
    
    /// <summary>
    /// Get the screen center position in the same coordinate space as the card
    /// </summary>
    private Vector3 GetScreenCenterPosition(GameObject cardObject)
    {
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        if (cardRect == null)
        {
            Debug.LogWarning($"Card {cardObject.name} has no RectTransform, using fallback center position");
            return new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        }
        
        // Get the Canvas that the card belongs to
        Canvas canvas = cardObject.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning($"Could not find Canvas for card {cardObject.name}, using fallback center position");
            return new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        }
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogWarning($"Canvas {canvas.name} has no RectTransform, using fallback center position");
            return new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        }
        
        // Get center of canvas in world space
        Vector3[] corners = new Vector3[4];
        canvasRect.GetWorldCorners(corners);
        Vector3 centerWorld = (corners[0] + corners[2]) * 0.5f;
        
        // Convert world position to screen point
        Camera eventCamera = canvas.worldCamera;
        if (eventCamera == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = Camera.main;
        }
        
        Vector2 screenPoint;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // For overlay, world position is already in screen coordinates
            screenPoint = new Vector2(centerWorld.x, centerWorld.y);
        }
        else
        {
            // Convert world to screen
            screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, centerWorld);
        }
        
        // Convert screen point to local position relative to card's parent
        if (cardRect.parent != null)
        {
            RectTransform parentRect = cardRect.parent as RectTransform;
            if (parentRect != null)
            {
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect, 
                    screenPoint,
                    eventCamera,
                    out localPoint))
                {
                    return localPoint;
                }
            }
        }
        
        // Fallback: return world position (should work for most cases)
        return centerWorld;
    }
    
    /// <summary>
    /// Animate card discard
    /// </summary>
    public void AnimateCardDiscard(GameObject cardObject, Vector3 discardPosition, System.Action onComplete = null)
    {
        // Phase 2: Animation Kill Switch - skip animation if disabled
        if (disableAllAnimations)
        {
            Debug.Log($"<color=yellow>[Animation Kill Switch] Skipping AnimateCardDiscard for {cardObject.name} - callback firing immediately</color>");
            cardObject.SetActive(false);
            onComplete?.Invoke();
            return;
        }
        
        // Quick movement to discard pile
        LeanTween.move(cardObject, discardPosition, config.cardDiscardDuration)
            .setEase(config.cardDiscardEase);
        
        // Scale down
        LeanTween.scale(cardObject, Vector3.one * 0.2f, config.cardDiscardDuration)
            .setEase(LeanTweenType.easeInQuad);
        
        // Rotate
        LeanTween.rotate(cardObject, new Vector3(0, 0, Random.Range(-180f, 180f)), config.cardDiscardDuration)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => {
                onComplete?.Invoke();
                cardObject.SetActive(false);
            });
    }
    
    /// <summary>
    /// Highlight card on hover
    /// </summary>
    public void AnimateCardHover(GameObject cardObject, Vector3 originalPosition, bool isHovering)
    {
        // Get the card's BASE scale (from CardHoverEffect)
        CardHoverEffect hoverEffect = cardObject.GetComponent<CardHoverEffect>();
        Vector3 baseScale = hoverEffect != null ? hoverEffect.GetBaseScale() : cardObject.transform.localScale;
        
        // Scale RELATIVE to base (multiply, don't replace!)
        float scaleMultiplier = isHovering ? config.cardHoverScale : 1f;
        Vector3 targetScale = baseScale * scaleMultiplier;
        
        Vector3 targetPosition = originalPosition; // Use the STORED original position!
        
        if (isHovering)
        {
            targetPosition.y += config.cardHoverLift;
        }
        
        // Cancel existing tweens
        LeanTween.cancel(cardObject);
        
        // Scale animation (RELATIVE to base scale!)
        LeanTween.scale(cardObject, targetScale, config.cardHoverDuration)
            .setEase(LeanTweenType.easeOutQuad);
        
        // Lift animation
        LeanTween.moveLocal(cardObject, targetPosition, config.cardHoverDuration)
            .setEase(LeanTweenType.easeOutQuad);
    }
    
    /// <summary>
    /// Legacy overload for backward compatibility - auto-detects original position
    /// </summary>
    public void AnimateCardHover(GameObject cardObject, bool isHovering)
    {
        CardHoverEffect hoverEffect = cardObject.GetComponent<CardHoverEffect>();
        Vector3 originalPosition = hoverEffect != null ? hoverEffect.GetOriginalPosition() : cardObject.transform.localPosition;
        AnimateCardHover(cardObject, originalPosition, isHovering);
    }
    
    #endregion
    
    #region Screen Effects
    
    /// <summary>
    /// Shake the camera for impact effect
    /// </summary>
    public void ShakeCamera(float intensity = 1f)
    {
        if (mainCamera == null) return;
        
        Vector3 originalPosition = mainCamera.transform.localPosition;
        float duration = config.screenShakeDuration * intensity;
        float magnitude = config.screenShakeMagnitude * intensity;
        
        LeanTween.cancel(mainCamera.gameObject);
        
        LeanTween.value(mainCamera.gameObject, 0f, 1f, duration)
            .setOnUpdate((float progress) => {
                float damping = 1f - progress;
                Vector3 shakeOffset = Random.insideUnitSphere * magnitude * damping;
                shakeOffset.z = 0; // Keep Z unchanged for 2D
                mainCamera.transform.localPosition = originalPosition + shakeOffset;
            })
            .setOnComplete(() => {
                mainCamera.transform.localPosition = originalPosition;
            })
            .setEase(LeanTweenType.easeOutQuad);
    }
    
    /// <summary>
    /// Zoom camera for emphasis
    /// </summary>
    public void ZoomCamera(float zoomAmount, float duration)
    {
        if (mainCamera == null) return;
        
        float originalSize = mainCamera.orthographicSize;
        float targetSize = originalSize * zoomAmount;
        
        LeanTween.value(mainCamera.gameObject, originalSize, targetSize, duration * 0.5f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float size) => {
                mainCamera.orthographicSize = size;
            })
            .setOnComplete(() => {
                // Zoom back out
                LeanTween.value(mainCamera.gameObject, targetSize, originalSize, duration * 0.5f)
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnUpdate((float size) => {
                        mainCamera.orthographicSize = size;
                    });
            });
    }
    
    #endregion
    
    #region Turn Transition Animations
    
    /// <summary>
    /// Animate turn indicator
    /// </summary>
    public void AnimateTurnTransition(GameObject turnIndicator, string turnText, Color turnColor, System.Action onComplete = null)
    {
        Text indicatorText = turnIndicator.GetComponent<Text>();
        if (indicatorText != null)
        {
            indicatorText.text = turnText;
        }
        
        // Scale pulse
        LeanTween.cancel(turnIndicator);
        turnIndicator.transform.localScale = Vector3.one * 0.5f;
        
        LeanTween.scale(turnIndicator, Vector3.one * 1.2f, 0.3f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {
                LeanTween.scale(turnIndicator, Vector3.one, 0.2f)
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnComplete(() => onComplete?.Invoke());
            });
        
        // Color flash
        if (indicatorText != null)
        {
            LeanTween.value(turnIndicator, Color.white, turnColor, 0.5f)
                .setOnUpdate((Color color) => {
                    indicatorText.color = color;
                });
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Cancel tween associated with a specific GameObject
    /// </summary>
    private void CancelObjectTween(GameObject obj)
    {
        if (objectTweens.ContainsKey(obj))
        {
            LeanTween.cancel(obj, objectTweens[obj]);
            objectTweens.Remove(obj);
        }
    }
    
    /// <summary>
    /// Track tween for a GameObject
    /// </summary>
    private void TrackObjectTween(GameObject obj, int tweenId)
    {
        if (objectTweens.ContainsKey(obj))
        {
            objectTweens[obj] = tweenId;
        }
        else
        {
            objectTweens.Add(obj, tweenId);
        }
        
        activeTweens.Add(tweenId);
    }
    
    /// <summary>
    /// Cancel all active animations
    /// </summary>
    public void CancelAllAnimations()
    {
        foreach (int tweenId in activeTweens)
        {
            LeanTween.cancel(tweenId);
        }
        
        activeTweens.Clear();
        objectTweens.Clear();
    }
    
    private void OnDestroy()
    {
        CancelAllAnimations();
    }
    
    #endregion
}

/// <summary>
/// Types of damage numbers for visual differentiation
/// </summary>
public enum DamageNumberType
{
    Normal,
    Critical,
    Fire,
    Cold,
    Lightning,
    Heal,
    Block
}

