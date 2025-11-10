using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Debug tool to diagnose particle rendering issues in Canvas UI.
/// Attach this to your ParticleEffectsCanvas GameObject.
/// </summary>
public class ParticleCanvasDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool logCanvasInfo = true;
    [SerializeField] private bool logParticleInfo = true;
    [SerializeField] private bool checkParticlePositions = true;
    [SerializeField] private bool autoFixCommonIssues = false;
    
    [Header("Manual Checks")]
    [SerializeField] private Canvas particleCanvas;
    [SerializeField] private ParticleSystem[] particleSystems;
    
    void Start()
    {
        if (logCanvasInfo)
        {
            CheckCanvasSetup();
        }
        
        if (logParticleInfo)
        {
            CheckParticleSystems();
        }
        
        if (checkParticlePositions)
        {
            CheckParticlePositions();
        }
        
        if (autoFixCommonIssues)
        {
            AutoFixIssues();
        }
    }
    
    void CheckCanvasSetup()
    {
        if (particleCanvas == null)
        {
            particleCanvas = GetComponent<Canvas>();
        }
        
        if (particleCanvas == null)
        {
            Debug.LogError("❌ ParticleCanvasDebugger: No Canvas component found on this GameObject!");
            return;
        }
        
        Debug.Log($"<color=cyan>=== Particle Canvas Debug Info ===</color>");
        Debug.Log($"Canvas Name: {particleCanvas.name}");
        Debug.Log($"Render Mode: {particleCanvas.renderMode}");
        Debug.Log($"Sort Order: {particleCanvas.sortingOrder}");
        Debug.Log($"Override Sorting: {particleCanvas.overrideSorting}");
        Debug.Log($"Pixel Perfect: {particleCanvas.pixelPerfect}");
        
        // Check for GraphicRaycaster (not needed but shouldn't cause issues)
        GraphicRaycaster raycaster = particleCanvas.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            Debug.Log($"GraphicRaycaster: ✅ Present (not needed but OK)");
        }
        
        // Check for CanvasScaler
        CanvasScaler scaler = particleCanvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            Debug.Log($"CanvasScaler: ✅ Present");
            Debug.Log($"  Scale Mode: {scaler.uiScaleMode}");
        }
        else
        {
            Debug.LogWarning($"CanvasScaler: ⚠️ Missing (recommended for consistent scaling)");
        }
        
        // Check if canvas is active
        if (!particleCanvas.gameObject.activeInHierarchy)
        {
            Debug.LogError("❌ Canvas GameObject is NOT ACTIVE! Enable it in hierarchy.");
        }
        
        // Check if canvas is enabled
        if (!particleCanvas.enabled)
        {
            Debug.LogError("❌ Canvas component is DISABLED! Enable it in Inspector.");
        }
        
        // Check main canvas for conflicts
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        Canvas mainCanvas = null;
        int highestSortOrder = int.MinValue;
        
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas != particleCanvas && canvas.sortingOrder > highestSortOrder)
            {
                highestSortOrder = canvas.sortingOrder;
                mainCanvas = canvas;
            }
        }
        
        if (mainCanvas != null)
        {
            Debug.Log($"Main Canvas Found: {mainCanvas.name} (Sort Order: {mainCanvas.sortingOrder})");
            
            if (particleCanvas.sortingOrder <= mainCanvas.sortingOrder)
            {
                Debug.LogError($"❌ Particle Canvas Sort Order ({particleCanvas.sortingOrder}) is NOT HIGHER than Main Canvas ({mainCanvas.sortingOrder})!");
                Debug.LogError($"   Fix: Set Particle Canvas Sort Order to {mainCanvas.sortingOrder + 1} or higher.");
            }
            else
            {
                Debug.Log($"✅ Particle Canvas Sort Order ({particleCanvas.sortingOrder}) is higher than Main Canvas ({mainCanvas.sortingOrder})");
            }
            
            if (mainCanvas.overrideSorting && mainCanvas.sortingOrder >= particleCanvas.sortingOrder)
            {
                Debug.LogWarning($"⚠️ Main Canvas has Override Sorting enabled - this might cause issues!");
            }
        }
    }
    
    void CheckParticleSystems()
    {
        if (particleSystems == null || particleSystems.Length == 0)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }
        
        if (particleSystems == null || particleSystems.Length == 0)
        {
            Debug.LogWarning("⚠️ No Particle Systems found as children of ParticleEffectsCanvas!");
            return;
        }
        
        Debug.Log($"<color=cyan>=== Particle Systems Debug Info ===</color>");
        Debug.Log($"Found {particleSystems.Length} particle system(s)");
        
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps == null) continue;
            
            Debug.Log($"\n--- {ps.name} ---");
            
            // Check if GameObject is active
            if (!ps.gameObject.activeInHierarchy)
            {
                Debug.LogError($"❌ {ps.name} GameObject is NOT ACTIVE!");
            }
            
            // Check if ParticleSystem is enabled
            if (!ps.gameObject.activeSelf)
            {
                Debug.LogError($"❌ {ps.name} is disabled!");
            }
            
            // Check if it's playing
            bool isPlaying = ps.isPlaying;
            bool isPaused = ps.isPaused;
            bool isStopped = ps.isStopped;
            
            Debug.Log($"Playing: {isPlaying} | Paused: {isPaused} | Stopped: {isStopped}");
            
            if (!isPlaying && !isPaused)
            {
                Debug.LogWarning($"⚠️ {ps.name} is NOT PLAYING! Enable 'Play On Awake' or call Play() manually.");
            }
            
            // Check particle count
            int particleCount = ps.particleCount;
            Debug.Log($"Active Particles: {particleCount}");
            
            if (particleCount == 0 && isPlaying)
            {
                Debug.LogWarning($"⚠️ {ps.name} is playing but has 0 particles! Check emission settings.");
            }
            
            // Check Renderer settings
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                Debug.Log($"Renderer Enabled: {renderer.enabled}");
                Debug.Log($"Sorting Layer: {renderer.sortingLayerName}");
                Debug.Log($"Order in Layer: {renderer.sortingOrder}");
                Debug.Log($"Render Mode: {renderer.renderMode}");
                
                // Check if using World Space (bad for UI Canvas)
                if (renderer.renderMode == ParticleSystemRenderMode.Mesh)
                {
                    Debug.LogWarning($"⚠️ {ps.name} is using Mesh render mode - might not work well in Canvas!");
                }
            }
            else
            {
                Debug.LogError($"❌ {ps.name} has NO ParticleSystemRenderer component!");
            }
            
            // Check RectTransform
            RectTransform rect = ps.GetComponent<RectTransform>();
            if (rect != null)
            {
                Debug.Log($"RectTransform Position: {rect.anchoredPosition}");
                Debug.Log($"RectTransform Size: {rect.sizeDelta}");
                Debug.Log($"RectTransform Scale: {rect.localScale}");
                
                if (rect.localScale == Vector3.zero)
                {
                    Debug.LogError($"❌ {ps.name} has ZERO SCALE! Particles won't be visible.");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ {ps.name} has NO RectTransform! This is required for UI Canvas.");
            }
            
            // Check if it's a child of Canvas
            Canvas parentCanvas = ps.GetComponentInParent<Canvas>();
            if (parentCanvas == null || parentCanvas != particleCanvas)
            {
                Debug.LogError($"❌ {ps.name} is NOT a child of the ParticleEffectsCanvas!");
            }
        }
    }
    
    void CheckParticlePositions()
    {
        if (particleSystems == null || particleSystems.Length == 0) return;
        
        Debug.Log($"<color=cyan>=== Particle Position Check ===</color>");
        
        Camera uiCamera = null;
        if (particleCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            uiCamera = particleCanvas.worldCamera;
        }
        
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps == null) continue;
            
            RectTransform rect = ps.GetComponent<RectTransform>();
            if (rect == null) continue;
            
            // Convert UI position to screen position
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                rect.position
            );
            
            Debug.Log($"{ps.name} Screen Position: {screenPos}");
            
            // Check if position is reasonable (within screen bounds or close)
            bool isOnScreen = screenPos.x >= -1000 && screenPos.x <= Screen.width + 1000 &&
                             screenPos.y >= -1000 && screenPos.y <= Screen.height + 1000;
            
            if (!isOnScreen)
            {
                Debug.LogWarning($"⚠️ {ps.name} appears to be OFF-SCREEN! Position: {screenPos}");
            }
        }
    }
    
    void AutoFixIssues()
    {
        Debug.Log("<color=yellow>=== Auto-Fixing Common Issues ===</color>");
        
        // Fix Canvas
        if (particleCanvas != null)
        {
            if (particleCanvas.renderMode != RenderMode.ScreenSpaceOverlay &&
                particleCanvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                Debug.Log("Fixing: Setting Canvas Render Mode to Screen Space - Overlay");
                particleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            
            if (!particleCanvas.overrideSorting)
            {
                Debug.Log("Fixing: Enabling Override Sorting on Canvas");
                particleCanvas.overrideSorting = true;
            }
        }
        
        // Fix Particle Systems
        if (particleSystems == null || particleSystems.Length == 0)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }
        
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps == null) continue;
            
            // Ensure RectTransform exists
            RectTransform rect = ps.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.Log($"Fixing: Adding RectTransform to {ps.name}");
                rect = ps.gameObject.AddComponent<RectTransform>();
            }
            
            // Ensure scale is not zero
            if (rect.localScale == Vector3.zero)
            {
                Debug.Log($"Fixing: Setting scale to 1 for {ps.name}");
                rect.localScale = Vector3.one;
            }
            
            // Ensure particle system is playing
            if (!ps.isPlaying && !ps.isPaused)
            {
                Debug.Log($"Fixing: Starting particle system {ps.name}");
                ps.Play();
            }
            
            // Ensure renderer exists and is enabled
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
            {
                Debug.LogError($"Cannot fix: {ps.name} has no ParticleSystemRenderer!");
            }
            else if (!renderer.enabled)
            {
                Debug.Log($"Fixing: Enabling renderer for {ps.name}");
                renderer.enabled = true;
            }
        }
        
        Debug.Log("<color=green>✓ Auto-fix complete!</color>");
    }
    
    [ContextMenu("Run Full Diagnostic")]
    void RunFullDiagnostic()
    {
        CheckCanvasSetup();
        CheckParticleSystems();
        CheckParticlePositions();
    }
    
    [ContextMenu("Auto-Fix Issues")]
    void RunAutoFix()
    {
        autoFixCommonIssues = true;
        AutoFixIssues();
    }
}


