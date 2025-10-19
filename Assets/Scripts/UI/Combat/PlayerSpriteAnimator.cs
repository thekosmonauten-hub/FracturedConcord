using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles player sprite animations for combat actions like attack nudge and guard sheen.
/// </summary>
public class PlayerSpriteAnimator : MonoBehaviour
{
    [Header("Player Sprite References")]
    [SerializeField] private Image playerSprite;
    [SerializeField] private RectTransform playerSpriteTransform;
    
    [Header("Attack Animation Settings")]
    [SerializeField] private float attackNudgeDistance = 30f;
    [SerializeField] private float attackNudgeDuration = 0.3f;
    [SerializeField] private LeanTweenType attackEaseType = LeanTweenType.easeOutQuad;
    
    [Header("Guard Animation Settings")]
    [SerializeField] private GameObject sheenEffect;
    [SerializeField] private float sheenDuration = 1f;
    [SerializeField] private float sheenFadeInDuration = 0.2f;
    [SerializeField] private float sheenFadeOutDuration = 0.8f;
    [SerializeField] private LeanTweenType sheenEaseType = LeanTweenType.easeOutQuad;
    
    [Header("Animation Colors")]
    [SerializeField] private Color sheenColor = new Color(1f, 1f, 1f, 0.8f);
    
    // State
    private Vector3 originalPosition;
    private bool isAnimating = false;
    
    private void Awake()
    {
        // Auto-find components if not assigned
        if (playerSprite == null)
            playerSprite = GetComponent<Image>();
            
        if (playerSpriteTransform == null)
            playerSpriteTransform = GetComponent<RectTransform>();
            
        if (sheenEffect == null)
            sheenEffect = transform.Find("SheenEffect")?.gameObject;
        
        // Store original position
        if (playerSpriteTransform != null)
        {
            originalPosition = playerSpriteTransform.anchoredPosition;
        }
        
        // Initialize sheen effect
        InitializeSheenEffect();
    }
    
    private void InitializeSheenEffect()
    {
        if (sheenEffect == null)
        {
            // Create sheen effect if it doesn't exist
            GameObject sheen = new GameObject("SheenEffect");
            sheen.transform.SetParent(transform);
            sheen.transform.SetAsLastSibling(); // Put it IN FRONT of the player sprite
            
            RectTransform sheenRect = sheen.AddComponent<RectTransform>();
            sheenRect.anchorMin = Vector2.zero;
            sheenRect.anchorMax = Vector2.one;
            sheenRect.offsetMin = Vector2.zero;
            sheenRect.offsetMax = Vector2.zero;
            
            Image sheenImage = sheen.AddComponent<Image>();
            sheenImage.color = sheenColor;
            sheenImage.sprite = playerSprite.sprite; // Use same sprite as player
            sheenImage.raycastTarget = false; // Don't block clicks
            
            sheenEffect = sheen;
            Debug.Log("Created SheenEffect GameObject");
        }
        
        // Hide sheen effect initially
        if (sheenEffect != null)
        {
            sheenEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// Triggers attack nudge animation towards enemies.
    /// </summary>
    public void PlayAttackNudge()
    {
        if (isAnimating || playerSpriteTransform == null) return;
        
        isAnimating = true;
        
        // Calculate nudge direction (towards enemies - typically to the right)
        Vector3 nudgePosition = originalPosition + new Vector3(attackNudgeDistance, 0, 0);
        
        // Animate nudge forward
        LeanTween.move(playerSpriteTransform, nudgePosition, attackNudgeDuration * 0.5f)
            .setEase(attackEaseType)
            .setOnComplete(() => {
                // Animate back to original position
                LeanTween.move(playerSpriteTransform, originalPosition, attackNudgeDuration * 0.5f)
                    .setEase(attackEaseType)
                    .setOnComplete(() => {
                        isAnimating = false;
                    });
            });
    }
    
    /// <summary>
    /// Triggers guard sheen animation over the sprite.
    /// </summary>
    public void PlayGuardSheen()
    {
        if (sheenEffect == null) 
        {
            Debug.LogWarning("SheenEffect is null! Cannot play guard sheen animation.");
            return;
        }
        
        Debug.Log("Starting guard sheen animation");
        
        // Stop any existing sheen animation
        LeanTween.cancel(sheenEffect);
        
        // Reset sheen effect
        sheenEffect.SetActive(true);
        Image sheenImage = sheenEffect.GetComponent<Image>();
        if (sheenImage != null)
        {
            Color startColor = sheenColor;
            startColor.a = 0f;
            sheenImage.color = startColor;
            Debug.Log($"Sheen effect activated with initial color: {startColor}");
            Debug.Log($"Sheen effect GameObject active: {sheenEffect.activeInHierarchy}");
            Debug.Log($"Sheen effect Transform position: {sheenEffect.transform.position}");
            Debug.Log($"Sheen effect Transform localPosition: {sheenEffect.transform.localPosition}");
        }
        
        // Simple test - just make it fully visible for 1 second
        Debug.Log("Testing simple sheen visibility...");
        
        // First, make it fully visible to test
        if (sheenImage != null)
        {
            Color testColor = sheenColor;
            testColor.a = 1f;
            sheenImage.color = testColor;
            Debug.Log($"Set sheen to full opacity: {testColor}");
            
            // Wait 1 second, then fade out
            LeanTween.delayedCall(1f, () => {
                Debug.Log("Starting fade out...");
                LeanTween.value(sheenEffect, 1f, 0f, 0.5f)
                    .setOnUpdate((float alpha) => {
                        if (sheenImage != null)
                        {
                            Color currentColor = sheenImage.color;
                            currentColor.a = alpha;
                            sheenImage.color = currentColor;
                            Debug.Log($"Alpha: {alpha}");
                        }
                    })
                    .setOnComplete(() => {
                        Debug.Log("Fade out complete, hiding effect");
                        sheenEffect.SetActive(false);
                    });
            });
        }
    }
    
    /// <summary>
    /// Triggers a damage shake animation.
    /// </summary>
    public void PlayDamageShake()
    {
        if (isAnimating || playerSpriteTransform == null) return;
        
        isAnimating = true;
        
        // Small shake effect
        float shakeIntensity = 5f;
        float shakeDuration = 0.3f;
        
        LeanTween.moveX(playerSpriteTransform, originalPosition.x + shakeIntensity, shakeDuration * 0.25f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => {
                LeanTween.moveX(playerSpriteTransform, originalPosition.x - shakeIntensity, shakeDuration * 0.25f)
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnComplete(() => {
                        LeanTween.moveX(playerSpriteTransform, originalPosition.x + shakeIntensity, shakeDuration * 0.25f)
                            .setEase(LeanTweenType.easeInOutQuad)
                            .setOnComplete(() => {
                                LeanTween.moveX(playerSpriteTransform, originalPosition.x, shakeDuration * 0.25f)
                                    .setEase(LeanTweenType.easeInOutQuad)
                                    .setOnComplete(() => {
                                        isAnimating = false;
                                    });
                            });
                    });
            });
    }
    
    /// <summary>
    /// Triggers a heal glow animation.
    /// </summary>
    public void PlayHealGlow()
    {
        if (sheenEffect == null) return;
        
        // Stop any existing sheen animation
        LeanTween.cancel(sheenEffect);
        
        // Set heal glow color (green tint)
        Color healColor = new Color(0.5f, 1f, 0.5f, 0.6f);
        
        // Reset sheen effect
        sheenEffect.SetActive(true);
        Image sheenImage = sheenEffect.GetComponent<Image>();
        if (sheenImage != null)
        {
            sheenImage.color = healColor;
        }
        
        // Fade in and out heal glow
        LeanTween.alpha(sheenEffect.GetComponent<RectTransform>(), healColor.a, 0.2f)
            .setEase(sheenEaseType)
            .setOnComplete(() => {
                LeanTween.alpha(sheenEffect.GetComponent<RectTransform>(), 0f, 0.8f)
                    .setEase(sheenEaseType)
                    .setOnComplete(() => {
                        sheenEffect.SetActive(false);
                        // Reset to original sheen color
                        if (sheenImage != null)
                        {
                            sheenImage.color = sheenColor;
                        }
                    });
            });
    }
    
    #region Context Menu Debug Methods
    
    [ContextMenu("Test Attack Nudge")]
    private void TestAttackNudge()
    {
        PlayAttackNudge();
        Debug.Log("Playing attack nudge animation");
    }
    
    [ContextMenu("Test Guard Sheen")]
    private void TestGuardSheen()
    {
        PlayGuardSheen();
        Debug.Log("Playing guard sheen animation");
    }
    
    [ContextMenu("Debug Sheen Effect")]
    private void DebugSheenEffect()
    {
        Debug.Log("=== SHEEN EFFECT DEBUG ===");
        Debug.Log($"SheenEffect GameObject: {(sheenEffect != null ? sheenEffect.name : "NULL")}");
        Debug.Log($"SheenEffect Active: {(sheenEffect != null ? sheenEffect.activeInHierarchy.ToString() : "NULL")}");
        
        if (sheenEffect != null)
        {
            Image sheenImage = sheenEffect.GetComponent<Image>();
            Debug.Log($"SheenEffect Image: {(sheenImage != null ? "FOUND" : "MISSING")}");
            if (sheenImage != null)
            {
                Debug.Log($"SheenEffect Color: {sheenImage.color}");
                Debug.Log($"SheenEffect Sprite: {(sheenImage.sprite != null ? sheenImage.sprite.name : "NULL")}");
                Debug.Log($"SheenEffect RaycastTarget: {sheenImage.raycastTarget}");
            }
            
            RectTransform sheenRect = sheenEffect.GetComponent<RectTransform>();
            Debug.Log($"SheenEffect RectTransform: {(sheenRect != null ? "FOUND" : "MISSING")}");
            if (sheenRect != null)
            {
                Debug.Log($"SheenEffect Position: {sheenRect.anchoredPosition}");
                Debug.Log($"SheenEffect Size: {sheenRect.sizeDelta}");
                Debug.Log($"SheenEffect LocalPosition: {sheenRect.localPosition}");
                Debug.Log($"SheenEffect AnchoredPosition: {sheenRect.anchoredPosition}");
            }
            
            // Check parent hierarchy
            Transform parent = sheenEffect.transform.parent;
            Debug.Log($"SheenEffect Parent: {(parent != null ? parent.name : "NULL")}");
            Debug.Log($"SheenEffect Sibling Index: {sheenEffect.transform.GetSiblingIndex()}");
            
            // Check if there are any Canvas components
            Canvas canvas = sheenEffect.GetComponentInParent<Canvas>();
            Debug.Log($"Canvas Found: {(canvas != null ? canvas.name : "NULL")}");
            if (canvas != null)
            {
                Debug.Log($"Canvas Sort Order: {canvas.sortingOrder}");
                Debug.Log($"Canvas Render Mode: {canvas.renderMode}");
            }
        }
        
        Debug.Log($"PlayerSprite: {(playerSprite != null ? playerSprite.name : "NULL")}");
        Debug.Log($"PlayerSpriteTransform: {(playerSpriteTransform != null ? playerSpriteTransform.name : "NULL")}");
        Debug.Log($"SheenColor: {sheenColor}");
        Debug.Log("=== END DEBUG ===");
    }
    
    [ContextMenu("Force Sheen Visible")]
    private void ForceSheenVisible()
    {
        if (sheenEffect == null)
        {
            Debug.LogWarning("SheenEffect is null! Creating it now...");
            InitializeSheenEffect();
        }
        
        if (sheenEffect != null)
        {
            sheenEffect.SetActive(true);
            Image sheenImage = sheenEffect.GetComponent<Image>();
            if (sheenImage != null)
            {
                // Try a bright, obvious color to test visibility
                Color visibleColor = Color.red; // Bright red for testing
                visibleColor.a = 1f; // Force full opacity
                sheenImage.color = visibleColor;
                
                // Also ensure it's on top
                sheenEffect.transform.SetAsLastSibling();
                
                Debug.Log($"Forced sheen to be visible with BRIGHT RED color: {visibleColor}");
                Debug.Log($"SheenEffect sibling index: {sheenEffect.transform.GetSiblingIndex()}");
                Debug.Log($"SheenEffect active: {sheenEffect.activeInHierarchy}");
                Debug.Log($"SheenEffect position: {sheenEffect.transform.position}");
                Debug.Log($"SheenEffect localPosition: {sheenEffect.transform.localPosition}");
            }
        }
    }
    
    [ContextMenu("Create Test Sheen")]
    private void CreateTestSheen()
    {
        // Create a completely separate GameObject for testing
        GameObject testSheen = new GameObject("TestSheen");
        testSheen.transform.SetParent(transform.parent); // Put it at the same level as player sprite
        
        RectTransform testRect = testSheen.AddComponent<RectTransform>();
        testRect.anchorMin = Vector2.zero;
        testRect.anchorMax = Vector2.one;
        testRect.offsetMin = Vector2.zero;
        testRect.offsetMax = Vector2.zero;
        
        Image testImage = testSheen.AddComponent<Image>();
        testImage.color = Color.yellow; // Bright yellow
        testImage.raycastTarget = false;
        
        // Position it over the player sprite area
        if (playerSpriteTransform != null)
        {
            testRect.position = playerSpriteTransform.position;
            testRect.sizeDelta = playerSpriteTransform.sizeDelta;
        }
        
        Debug.Log("Created TestSheen GameObject with bright yellow color");
        Debug.Log($"TestSheen position: {testRect.position}");
        Debug.Log($"TestSheen size: {testRect.sizeDelta}");
        
        // Auto-destroy after 3 seconds
        Destroy(testSheen, 3f);
    }
    
    [ContextMenu("Test Damage Shake")]
    private void TestDamageShake()
    {
        PlayDamageShake();
        Debug.Log("Playing damage shake animation");
    }
    
    [ContextMenu("Test Heal Glow")]
    private void TestHealGlow()
    {
        PlayHealGlow();
        Debug.Log("Playing heal glow animation");
    }
    
    #endregion
}
