using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the Effigy Storage Panel - sliding container for all effigies
/// </summary>
public static class EffigyStorageManager
{
    /// <summary>
    /// Slide panel in from the right using UI Toolkit animation
    /// </summary>
    public static void SlideInPanel(VisualElement panel, float width, float duration = 0.3f)
    {
        if (panel == null) return;
        
        // Ensure panel is on top
        panel.BringToFront();
        
        // Stop any existing coroutines
        CoroutineRunner.Instance.StopAllCoroutines();
        
        // Set initial position (off-screen to the right)
        panel.style.position = Position.Absolute;
        panel.style.top = 0;
        panel.style.bottom = 0;
        panel.style.right = 0;
        panel.style.width = width;
        panel.style.display = DisplayStyle.Flex;
        
        Debug.Log($"[EffigyStorage] Sliding in panel from right={0f} to right={-width}");
        
        // Use CoroutineRunner for animation
        CoroutineRunner.Instance.StartCoroutine(AnimatePanelSlide(panel, 0f, -width, duration, () => {
            Debug.Log($"[EffigyStorage] Panel slide in complete, final right={-width}");
        }));
    }
    
    /// <summary>
    /// Slide panel out to the right (off-screen)
    /// </summary>
    public static void SlideOutPanel(VisualElement panel, float width, float duration = 0.3f, System.Action onComplete = null)
    {
        if (panel == null) return;
        
        CoroutineRunner.Instance.StartCoroutine(AnimatePanelSlide(panel, -width, 0f, duration, () => {
            Debug.Log($"[EffigyStorage] Panel slide out complete, hiding panel");
            panel.style.display = DisplayStyle.None;
            onComplete?.Invoke();
        }));
    }
    
    private static IEnumerator AnimatePanelSlide(VisualElement panel, float startValue, float endValue, float duration, System.Action onComplete = null)
    {
        // Ensure panel is visible during animation
        if (panel != null)
        {
            panel.style.display = DisplayStyle.Flex;
            panel.BringToFront();
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration && panel != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Ease out quad for slide in, ease in quad for slide out
            if (startValue < endValue) // Sliding in
            {
                t = 1f - (1f - t) * (1f - t); // Ease out
            }
            else // Sliding out
            {
                t = t * t; // Ease in
            }
            
            float currentValue = Mathf.Lerp(startValue, endValue, t);
            panel.style.right = currentValue;
            
            yield return null;
        }
        
        if (panel != null)
        {
            panel.style.right = endValue;
        }
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Simple coroutine runner for UI Toolkit animations
    /// </summary>
    private class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        private Dictionary<VisualElement, Coroutine> activeAnimations = new Dictionary<VisualElement, Coroutine>();
        
        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("EffigyStorageCoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        public new void StopAllCoroutines()
        {
            base.StopAllCoroutines();
        }
    }
}

