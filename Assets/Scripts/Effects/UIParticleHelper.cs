using UnityEngine;
using Coffee.UIExtensions;

/// <summary>
/// Helper to automatically setup UIParticle on effect prefabs
/// </summary>
public static class UIParticleHelper
{
    /// <summary>
    /// Ensure effect has UIParticle component and is properly configured
    /// </summary>
    public static GameObject SetupUIParticle(GameObject effect, EffectData effectData, Canvas targetCanvas)
    {
        if (effect == null || effectData == null) return effect;
        
        // Check if UIParticle already exists (prefab may already have it configured)
        UIParticle existingUIParticle = effect.GetComponent<UIParticle>();
        if (existingUIParticle != null)
        {
            // UIParticle already exists - just update scale if needed and ensure canvas parent
            if (effectData.useUIParticle && effectData.uiParticleScale > 0)
            {
                existingUIParticle.scale = effectData.uiParticleScale;
            }
            
            // CRITICAL: Disable raycastTarget on UIParticle to prevent red border
            existingUIParticle.raycastTarget = false;
            
            // Ensure RectTransform exists and has proper scale
            RectTransform existingRect = effect.GetComponent<RectTransform>();
            if (existingRect == null)
            {
                existingRect = effect.AddComponent<RectTransform>();
                existingRect.sizeDelta = new Vector2(1, 1);
            }
            
            // Fix scale if it's zero (can cause rendering issues)
            if (effect.transform.localScale == Vector3.zero)
            {
                effect.transform.localScale = Vector3.one;
            }
            
            // Disable any Image components that might show a red border
            UnityEngine.UI.Image existingImage = effect.GetComponent<UnityEngine.UI.Image>();
            if (existingImage != null)
            {
                existingImage.raycastTarget = false;
                // Make it invisible if it exists
                var color = existingImage.color;
                color.a = 0f;
                existingImage.color = color;
            }
            
            // Parent to canvas if provided and not already parented
            if (targetCanvas != null && effect.transform.parent != targetCanvas.transform)
            {
                effect.transform.SetParent(targetCanvas.transform, false);
            }
            
            return effect;
        }
        
        // Only setup if useUIParticle is true
        if (!effectData.useUIParticle) return effect;
        
        // Check if ParticleSystem exists
        ParticleSystem particles = effect.GetComponent<ParticleSystem>();
        if (particles == null)
        {
            Debug.LogWarning($"Effect {effectData.effectName} has useUIParticle=true but no ParticleSystem!");
            return effect;
        }
        
        // Add UIParticle component
        UIParticle uiParticle = effect.AddComponent<UIParticle>();
        uiParticle.scale = effectData.uiParticleScale;
        
        // CRITICAL: Disable raycastTarget on UIParticle to prevent red border
        uiParticle.raycastTarget = false;
        
        // Ensure RectTransform exists
        RectTransform rect = effect.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = effect.AddComponent<RectTransform>();
        }
        
        // Set parent to canvas
        if (targetCanvas != null)
        {
            effect.transform.SetParent(targetCanvas.transform, false);
        }
        
        // Fix scale if it's zero (can cause rendering issues)
        if (effect.transform.localScale == Vector3.zero)
        {
            effect.transform.localScale = Vector3.one;
        }
        
        // Set initial size (small, just enough for particles - not visible)
        rect.sizeDelta = new Vector2(1, 1);
        
        // Disable any Image components that might show a red border
        UnityEngine.UI.Image effectImage = effect.GetComponent<UnityEngine.UI.Image>();
        if (effectImage != null)
        {
            effectImage.raycastTarget = false;
            // Make it invisible if it exists
            var color = effectImage.color;
            color.a = 0f;
            effectImage.color = color;
        }
        
        return effect;
    }
    
    /// <summary>
    /// Setup projectile with ParticleAttractor (if available) or manual animation
    /// </summary>
    public static void SetupProjectile(GameObject projectile, EffectData effectData, 
        RectTransform startRect, RectTransform endRect, Canvas targetCanvas, System.Action onArrival = null)
    {
        if (projectile == null || effectData == null || !effectData.isProjectile) return;
        
        // Setup UIParticle first
        SetupUIParticle(projectile, effectData, targetCanvas);
        
        // Try to use ParticleAttractor via reflection (from ParticleEffectForUGUI package)
        System.Type attractorType = System.Type.GetType("Coffee.UIExtensions.ParticleAttractor, Assembly-CSharp");
        if (attractorType == null)
        {
            // Try alternative namespace/assembly
            attractorType = System.Type.GetType("Coffee.UIExtensions.ParticleAttractor");
        }
        
        if (attractorType != null)
        {
            // ParticleAttractor is available - use it
            Debug.Log($"[UIParticleHelper] ParticleAttractor found! Setting up for {projectile.name}");
            try
            {
                Component attractor = projectile.GetComponent(attractorType);
                if (attractor == null)
                {
                    attractor = projectile.AddComponent(attractorType);
                    Debug.Log($"[UIParticleHelper] Added ParticleAttractor component to {projectile.name}");
                }
                
                // Configure attractor using reflection
                // Try both property and field access
                var targetProperty = attractorType.GetProperty("target");
                var targetField = attractorType.GetField("target");
                var speedProperty = attractorType.GetProperty("speed");
                var speedField = attractorType.GetField("speed");
                var destroyOnArrivalProperty = attractorType.GetProperty("destroyOnArrival");
                var destroyOnArrivalField = attractorType.GetField("destroyOnArrival");
                
                // Set target (RectTransform) - this is the enemy's "Default" point
                if (targetProperty != null)
                {
                    targetProperty.SetValue(attractor, endRect);
                    Debug.Log($"[UIParticleHelper] Set ParticleAttractor.target (via property) to {endRect.name} at position {endRect.anchoredPosition}");
                }
                else if (targetField != null)
                {
                    targetField.SetValue(attractor, endRect);
                    Debug.Log($"[UIParticleHelper] Set ParticleAttractor.target (via field) to {endRect.name} at position {endRect.anchoredPosition}");
                }
                else
                {
                    Debug.LogWarning("[UIParticleHelper] Could not find 'target' property/field on ParticleAttractor!");
                }
                
                // Set speed
                if (speedProperty != null)
                {
                    speedProperty.SetValue(attractor, effectData.projectileSpeed);
                    Debug.Log($"[UIParticleHelper] Set ParticleAttractor.speed (via property) to {effectData.projectileSpeed}");
                }
                else if (speedField != null)
                {
                    speedField.SetValue(attractor, effectData.projectileSpeed);
                    Debug.Log($"[UIParticleHelper] Set ParticleAttractor.speed (via field) to {effectData.projectileSpeed}");
                }
                
                // Set destroyOnArrival
                if (destroyOnArrivalProperty != null)
                {
                    destroyOnArrivalProperty.SetValue(attractor, true);
                }
                else if (destroyOnArrivalField != null)
                {
                    destroyOnArrivalField.SetValue(attractor, true);
                }
                
                // Verify the setup
                if (targetProperty != null || targetField != null)
                {
                    object currentTarget = targetProperty != null ? targetProperty.GetValue(attractor) : targetField.GetValue(attractor);
                    if (currentTarget == endRect)
                    {
                        Debug.Log($"[UIParticleHelper] ✓ ParticleAttractor configured successfully! Target verified: {endRect.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[UIParticleHelper] ⚠ ParticleAttractor target verification failed! Expected {endRect.name}, got {currentTarget}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[UIParticleHelper] Could not configure ParticleAttractor: {e.Message}. Using manual animation fallback.");
                Debug.LogException(e);
                SetupManualProjectileAnimation(projectile, effectData, startRect, endRect, targetCanvas, onArrival);
            }
        }
        else
        {
            // ParticleAttractor not found - use manual animation
            Debug.LogWarning("ParticleAttractor not found in ParticleEffectForUGUI package. Using manual animation fallback.");
            SetupManualProjectileAnimation(projectile, effectData, startRect, endRect, targetCanvas, onArrival);
        }
        
        // Position at start - use anchoredPosition directly since projectile will be parented to canvas
        RectTransform projectileRect = projectile.GetComponent<RectTransform>();
        if (projectileRect != null && startRect != null)
        {
            // Convert startRect's position to canvas root space
            Vector2 startPos = GetAnchoredPositionInCanvas(startRect, targetCanvas);
            projectileRect.anchoredPosition = startPos;
            Debug.Log($"[UIParticleHelper] Positioned projectile at start: {startPos} (from startRect.anchoredPosition: {startRect.anchoredPosition})");
        }
        
        // Play particles
        ParticleSystem particles = projectile.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
        }
    }
    
    /// <summary>
    /// Setup manual projectile animation using coroutine (fallback when ParticleAttractor unavailable)
    /// </summary>
    private static void SetupManualProjectileAnimation(GameObject projectile, EffectData effectData,
        RectTransform startRect, RectTransform endRect, Canvas targetCanvas, System.Action onArrival = null)
    {
        Debug.Log($"[UIParticleHelper] Setting up manual projectile animation for {projectile.name}");
        
        // Ensure projectile has RectTransform
        RectTransform projectileRect = projectile.GetComponent<RectTransform>();
        if (projectileRect == null)
        {
            Debug.LogWarning($"[UIParticleHelper] Projectile {projectile.name} has no RectTransform! Adding one...");
            projectileRect = projectile.AddComponent<RectTransform>();
            projectileRect.sizeDelta = new Vector2(100, 100);
        }
        
        // Ensure projectile is active
        if (!projectile.activeSelf)
        {
            Debug.LogWarning($"[UIParticleHelper] Projectile {projectile.name} is inactive! Activating...");
            projectile.SetActive(true);
        }
        
        // Add a MonoBehaviour component to handle the animation coroutine
        var animator = projectile.GetComponent<ProjectileAnimator>();
        if (animator == null)
        {
            animator = projectile.AddComponent<ProjectileAnimator>();
            Debug.Log($"[UIParticleHelper] Added ProjectileAnimator to {projectile.name}");
        }
        
        Vector2 startPos = GetAnchoredPositionInCanvas(startRect, targetCanvas);
        Vector2 endPos = GetAnchoredPositionInCanvas(endRect, targetCanvas);
        
        Debug.Log($"[UIParticleHelper] Position conversion - startRect.anchoredPosition: {startRect.anchoredPosition}, converted: {startPos}");
        Debug.Log($"[UIParticleHelper] Position conversion - endRect.anchoredPosition: {endRect.anchoredPosition}, converted: {endPos}");
        
        float distance = Vector2.Distance(startPos, endPos);
        float duration = distance / effectData.projectileSpeed;
        
        Debug.Log($"[UIParticleHelper] Starting animation: start={startPos}, end={endPos}, distance={distance:F1}, speed={effectData.projectileSpeed}, duration={duration:F2}s");
        
        animator.StartAnimation(startPos, endPos, effectData.projectileSpeed, effectData.useArcTrajectory, effectData.arcHeight, onArrival);
    }
    
    /// <summary>
    /// Helper MonoBehaviour for manual projectile animation
    /// </summary>
    private class ProjectileAnimator : MonoBehaviour
    {
        private System.Action onArrival;
        
        public void StartAnimation(Vector2 startPos, Vector2 endPos, float speed, bool useArc, float arcHeight, System.Action onArrivalCallback = null)
        {
            onArrival = onArrivalCallback;
            StartCoroutine(AnimateProjectile(startPos, endPos, speed, useArc, arcHeight));
        }
        
        private System.Collections.IEnumerator AnimateProjectile(Vector2 startPos, Vector2 endPos, float speed, bool useArc, float arcHeight)
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("[ProjectileAnimator] No RectTransform found! Cannot animate.");
                yield break;
            }
            
            Debug.Log($"[ProjectileAnimator] Starting animation coroutine for {gameObject.name}");
            
            // Set initial position
            rect.anchoredPosition = startPos;
            Debug.Log($"[ProjectileAnimator] Initial position set: {startPos}");
            
            float distance = Vector2.Distance(startPos, endPos);
            float duration = distance / speed;
            float elapsed = 0f;
            
            // Distance threshold for "arrival" (trigger impact when close enough)
            // This is more reliable than pure timing, especially with arc trajectories
            float arrivalThreshold = Mathf.Max(10f, distance * 0.05f); // 5% of distance or 10px minimum
            bool hasArrived = false;
            
            Debug.Log($"[ProjectileAnimator] Animation params: distance={distance:F1}, speed={speed}, duration={duration:F2}s, useArc={useArc}, arrivalThreshold={arrivalThreshold:F1}");
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                Vector2 currentPos;
                
                if (useArc)
                {
                    // Curved arc trajectory
                    currentPos = Vector2.Lerp(startPos, endPos, t);
                    float arcOffset = Mathf.Sin(t * Mathf.PI) * arcHeight;
                    currentPos.y += arcOffset;
                    rect.anchoredPosition = currentPos;
                    
                    // Rotate to face direction
                    Vector2 direction = (endPos - startPos).normalized;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    rect.localEulerAngles = new Vector3(0, 0, angle);
                }
                else
                {
                    // Straight line
                    currentPos = Vector2.Lerp(startPos, endPos, t);
                    rect.anchoredPosition = currentPos;
                    
                    // Rotate to face direction
                    Vector2 direction = (endPos - startPos).normalized;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    rect.localEulerAngles = new Vector3(0, 0, angle);
                }
                
                // Check if we're close enough to the target (distance-based detection)
                float distanceToTarget = Vector2.Distance(currentPos, endPos);
                if (!hasArrived && distanceToTarget <= arrivalThreshold)
                {
                    hasArrived = true;
                    Debug.Log($"[ProjectileAnimator] ✓ Projectile arrived at target! Distance: {distanceToTarget:F1} (threshold: {arrivalThreshold:F1})");
                    
                    // Trigger arrival callback immediately when close enough
                    if (onArrival != null)
                    {
                        onArrival.Invoke();
                    }
                }
                
                yield return null;
            }
            
            // Ensure we're exactly at end position
            rect.anchoredPosition = endPos;
            
            // Trigger arrival callback if we haven't already (safety fallback)
            if (!hasArrived && onArrival != null)
            {
                Debug.Log($"[ProjectileAnimator] Triggering arrival callback as fallback (animation complete)");
                onArrival.Invoke();
            }
            
            // Destroy after brief delay
            yield return new WaitForSeconds(0.1f);
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }
    
    private static Vector2 GetAnchoredPositionInCanvas(RectTransform targetRect, Canvas canvas)
    {
        if (canvas == null || targetRect == null) 
        {
            return targetRect != null ? targetRect.anchoredPosition : Vector2.zero;
        }
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null) return targetRect.anchoredPosition;
        
        // Since both RectTransforms are in the Canvas hierarchy, we need to convert
        // their position to the canvas root's coordinate space.
        // The most reliable way is to use RectTransformUtility with the world position.
        
        Vector3 worldPos = targetRect.position;
        Camera cam = canvas.worldCamera;
        
        // Convert world position to screen point
        Vector2 screenPoint;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // For overlay canvas, worldPos is already in screen coordinates
            screenPoint = new Vector2(worldPos.x, worldPos.y);
            cam = null; // No camera for overlay
        }
        else
        {
            // For Camera or World Space canvas, convert through camera
            if (cam == null) cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError($"[UIParticleHelper] No camera found for canvas {canvas.name} (renderMode: {canvas.renderMode})");
                return targetRect.anchoredPosition;
            }
            screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        }
        
        // Convert screen point to canvas root's local space
        Vector2 canvasLocalPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            cam,
            out canvasLocalPos))
        {
            Debug.Log($"[UIParticleHelper] Converted {targetRect.name}: worldPos={worldPos}, screenPoint={screenPoint}, canvasLocalPos={canvasLocalPos}, anchoredPos={targetRect.anchoredPosition}");
            return canvasLocalPos;
        }
        
        // If conversion failed, try alternative method: traverse hierarchy
        Debug.LogWarning($"[UIParticleHelper] ScreenPointToLocalPointInRectangle failed for {targetRect.name}. Trying hierarchy traversal...");
        
        // Alternative: Calculate position by traversing up to canvas root
        Vector2 accumulatedPos = targetRect.anchoredPosition;
        RectTransform current = targetRect.parent as RectTransform;
        RectTransform parentRect = null;
        
        while (current != null && current != canvasRect)
        {
            accumulatedPos += current.anchoredPosition;
            parentRect = current;
            current = current.parent as RectTransform;
        }
        
        if (current == canvasRect)
        {
            Debug.Log($"[UIParticleHelper] Hierarchy traversal successful: {targetRect.name} -> {accumulatedPos}");
            return accumulatedPos;
        }
        
        // Final fallback
        Debug.LogError($"[UIParticleHelper] All conversion methods failed for {targetRect.name}. Canvas: {canvas.renderMode}, Camera: {(cam != null ? cam.name : "null")}. Using anchoredPosition: {targetRect.anchoredPosition}");
        return targetRect.anchoredPosition;
    }
}

