using UnityEngine;

/// <summary>
/// One-time script to fix particle sorting issues in UI Canvas.
/// Attach to ParticleEffectsCanvas, run once, then remove.
/// </summary>
public class FixParticleSorting : MonoBehaviour
{
    [Header("Fix Options")]
    [SerializeField] private bool resetToCanvasRendering = true;
    [SerializeField] private bool logChanges = true;
    
    [Header("Manual Settings (if not using Canvas rendering)")]
    [SerializeField] private string customSortingLayer = "Default";
    [SerializeField] private int customOrderInLayer = 0;
    
    [ContextMenu("Fix All Particle Sorting")]
    void FixAllParticles()
    {
        ParticleSystemRenderer[] allRenderers = GetComponentsInChildren<ParticleSystemRenderer>();
        
        if (allRenderers.Length == 0)
        {
            Debug.LogWarning("No Particle System Renderers found!");
            return;
        }
        
        Debug.Log($"<color=yellow>Fixing {allRenderers.Length} particle system(s)...</color>");
        
        foreach (ParticleSystemRenderer renderer in allRenderers)
        {
            string oldLayer = renderer.sortingLayerName;
            int oldOrder = renderer.sortingOrder;
            
            if (resetToCanvasRendering)
            {
                // Use Canvas rendering (no custom sorting)
                renderer.sortingLayerName = "Default";
                renderer.sortingOrder = 0;
            }
            else
            {
                // Use custom sorting
                renderer.sortingLayerName = customSortingLayer;
                renderer.sortingOrder = customOrderInLayer;
            }
            
            if (logChanges)
            {
                Debug.Log($"<color=cyan>{renderer.gameObject.name}:</color> " +
                         $"Sorting Layer: {oldLayer} → {renderer.sortingLayerName}, " +
                         $"Order: {oldOrder} → {renderer.sortingOrder}");
            }
        }
        
        Debug.Log($"<color=green>✓ Fixed {allRenderers.Length} particle system(s)!</color>");
        
        if (resetToCanvasRendering)
        {
            Debug.Log("<color=lime>Particles now use Canvas Sort Order for rendering.</color>");
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"<color=lime>Canvas Sort Order: {canvas.sortingOrder}</color>");
            }
        }
    }
    
    void Start()
    {
        // Auto-fix on start if desired
        // FixAllParticles();
    }
}


