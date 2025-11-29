using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Floating damage number that animates upward and fades out
/// </summary>
public class FloatingDamageText : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI damageText;
    public CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    public float floatDistance = 100f; // How far it floats up
    public float floatDuration = 1.5f; // How long the animation takes
    public AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);
    
    [Header("Visual Settings")]
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = new Color(1f, 0.6f, 0f); // Orange
    public Color healColor = Color.green;
    public float criticalSizeMultiplier = 1.5f;
    
    private RectTransform rectTransform;
    private Vector3 startPosition;
    private bool isAnimating = false;
    private Action onComplete;
    private float baseFontSize;
    
    private void Awake()
    {
        if (damageText == null)
            damageText = GetComponent<TextMeshProUGUI>();
        
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        rectTransform = GetComponent<RectTransform>();
        
        if (damageText != null)
            baseFontSize = damageText.fontSize;
    }
    
    /// <summary>
    /// Display damage number with animation
    /// </summary>
    public void Show(float damage, bool isCritical, Vector3 worldPosition, Action onCompleteCallback = null)
    {
        // Stop any existing animations
        LeanTween.cancel(gameObject);
        
        // Setup
        onComplete = onCompleteCallback;
        isAnimating = true;
        
        // Set damage text
        int displayDamage = Mathf.RoundToInt(damage);
        damageText.fontSize = baseFontSize;
        damageText.text = displayDamage.ToString();
        
        // Set color and size based on critical
        if (isCritical)
        {
            damageText.color = criticalDamageColor;
            damageText.fontSize = baseFontSize * criticalSizeMultiplier;
            damageText.text = $"<b>{displayDamage}!</b>"; // Bold with exclamation
        }
        else
        {
            damageText.color = normalDamageColor;
        }
        
        // Position at world position (convert to screen space)
        rectTransform.position = worldPosition;
        startPosition = rectTransform.anchoredPosition;
        
        // Reset alpha
        canvasGroup.alpha = 1f;
        
        // Animate upward float
        Vector3 endPosition = startPosition + new Vector3(UnityEngine.Random.Range(-20f, 20f), floatDistance, 0);
        
        LeanTween.value(gameObject, 0f, 1f, floatDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float t) => 
            {
                if (rectTransform != null)
                {
                    // Animate position
                    rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, floatCurve.Evaluate(t));
                    
                    // Animate fade
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = fadeCurve.Evaluate(t);
                    }
                }
            })
            .setOnComplete(() => 
            {
                isAnimating = false;
                onComplete?.Invoke();
                gameObject.SetActive(false);
            });
    }
    
    /// <summary>
    /// Display healing number (green, no critical)
    /// </summary>
    public void ShowHeal(float healAmount, Vector3 worldPosition, Action onCompleteCallback = null)
    {
        // Stop any existing animations
        LeanTween.cancel(gameObject);
        
        // Setup
        onComplete = onCompleteCallback;
        isAnimating = true;
        
        // Set heal text
        int displayHeal = Mathf.RoundToInt(healAmount);
        damageText.fontSize = baseFontSize;
        damageText.text = $"+{displayHeal}";
        damageText.color = healColor;
        
        // Position at world position
        rectTransform.position = worldPosition;
        startPosition = rectTransform.anchoredPosition;
        
        // Reset alpha
        canvasGroup.alpha = 1f;
        
        // Animate upward float
        Vector3 endPosition = startPosition + new Vector3(0, floatDistance, 0);
        
        LeanTween.value(gameObject, 0f, 1f, floatDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float t) => 
            {
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, floatCurve.Evaluate(t));
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = fadeCurve.Evaluate(t);
                    }
                }
            })
            .setOnComplete(() => 
            {
                isAnimating = false;
                onComplete?.Invoke();
                gameObject.SetActive(false);
            });
    }
    
    public void ShowAbilityText(string abilityName, Color textColor, Vector3 worldPosition, Action onCompleteCallback = null)
    {
        LeanTween.cancel(gameObject);
        
        onComplete = onCompleteCallback;
        isAnimating = true;
        
        damageText.fontSize = baseFontSize;
        damageText.text = abilityName;
        damageText.color = textColor;
        
        rectTransform.position = worldPosition;
        startPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 1f;
        
        Vector3 endPosition = startPosition + new Vector3(0, floatDistance, 0);
        
        LeanTween.value(gameObject, 0f, 1f, floatDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float t) =>
            {
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, floatCurve.Evaluate(t));
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = fadeCurve.Evaluate(t);
                    }
                }
            })
            .setOnComplete(() =>
            {
                isAnimating = false;
                onComplete?.Invoke();
                gameObject.SetActive(false);
            });
    }
    
    /// <summary>
    /// Stop animation and hide immediately
    /// </summary>
    public void Hide()
    {
        LeanTween.cancel(gameObject);
        isAnimating = false;
        gameObject.SetActive(false);
    }
}












