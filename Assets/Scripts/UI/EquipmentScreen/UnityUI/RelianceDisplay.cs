using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Displays current and max Reliance with a filled image bar
/// Shows in DynamicArea/Auras header
/// </summary>
public class RelianceDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI relianceText; // "Reliance" label
    [SerializeField] private TextMeshProUGUI relianceCounterText; // "CurrentReliance/MaxReliance" counter
    [SerializeField] private Image relianceFillImage; // Filled image type bound to CurrentReliance
    
    [Header("Settings")]
    [SerializeField] private bool updateOnEnable = true;
    [SerializeField] private float updateInterval = 0.1f; // Update every 0.1 seconds
    
    [Header("Animation Settings")]
    [SerializeField] private bool animateFillBar = true;
    [SerializeField] private float animationDuration = 0.5f; // Time to animate from current to target fill
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private CharacterManager characterManager;
    private float lastUpdateTime = 0f;
    private float targetFillAmount = 0f;
    private float currentFillAmount = 0f;
    private Coroutine fillAnimationCoroutine;
    
    void Start()
    {
        characterManager = CharacterManager.Instance;
        
        // Ensure fill image is set to Filled type
        if (relianceFillImage != null)
        {
            relianceFillImage.type = Image.Type.Filled;
            relianceFillImage.fillMethod = Image.FillMethod.Horizontal; // Or Vertical, depending on design
            currentFillAmount = relianceFillImage.fillAmount;
            targetFillAmount = currentFillAmount;
        }
        
        UpdateDisplay();
    }
    
    void OnEnable()
    {
        if (updateOnEnable)
        {
            UpdateDisplay();
        }
    }
    
    void Update()
    {
        // Update periodically (not every frame for performance)
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// Update the reliance display
    /// </summary>
    public void UpdateDisplay()
    {
        if (characterManager == null)
        {
            characterManager = CharacterManager.Instance;
        }
        
        if (characterManager == null || !characterManager.HasCharacter())
        {
            // Show default/empty values
            if (relianceCounterText != null)
            {
                relianceCounterText.text = "0/0";
            }
            
            if (relianceFillImage != null)
            {
                relianceFillImage.fillAmount = 0f;
            }
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        int currentReliance = character.reliance;
        int maxReliance = character.maxReliance;
        
        // Update counter text
        if (relianceCounterText != null)
        {
            relianceCounterText.text = $"{currentReliance}/{maxReliance}";
        }
        
        // Update fill image
        if (relianceFillImage != null && maxReliance > 0)
        {
            float fillAmount = (float)currentReliance / (float)maxReliance;
            targetFillAmount = Mathf.Clamp01(fillAmount);
            
            if (animateFillBar && animationDuration > 0f)
            {
                // Animate to target fill amount
                if (fillAnimationCoroutine != null)
                {
                    StopCoroutine(fillAnimationCoroutine);
                }
                fillAnimationCoroutine = StartCoroutine(AnimateFillBar(targetFillAmount));
            }
            else
            {
                // Update immediately without animation
                currentFillAmount = targetFillAmount;
                relianceFillImage.fillAmount = targetFillAmount;
            }
        }
    }
    
    /// <summary>
    /// Force an immediate update
    /// </summary>
    public void Refresh()
    {
        UpdateDisplay();
    }
    
    /// <summary>
    /// Animate the fill bar smoothly to the target amount
    /// </summary>
    private IEnumerator AnimateFillBar(float targetAmount)
    {
        float startAmount = currentFillAmount;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            
            // Apply animation curve for smooth easing
            float curveValue = animationCurve.Evaluate(t);
            
            currentFillAmount = Mathf.Lerp(startAmount, targetAmount, curveValue);
            relianceFillImage.fillAmount = currentFillAmount;
            
            yield return null;
        }
        
        // Ensure we end exactly at the target
        currentFillAmount = targetAmount;
        relianceFillImage.fillAmount = targetAmount;
        fillAnimationCoroutine = null;
    }
}

