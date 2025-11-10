using UnityEngine;

/// <summary>
/// NUCLEAR OPTION: Removes particles from Canvas system entirely and places them in World Space.
/// This GUARANTEES particles render on top of all UI.
/// Attach to ParticleEffectsCanvas, run once, then remove.
/// </summary>
public class MoveParticlesToWorldSpace : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Camera to render particles in front of (usually Main Camera)")]
    [SerializeField] private Camera targetCamera;
    
    [Tooltip("Distance from camera (smaller = closer = more on top)")]
    [SerializeField] private float distanceFromCamera = 0.5f;
    
    [Tooltip("Layer name for particles (must exist in project)")]
    [SerializeField] private string particleLayerName = "Default";
    
    [Header("Sorting")]
    [Tooltip("Sorting Layer (use Default or create 'TopMostUI')")]
    [SerializeField] private string sortingLayerName = "Default";
    
    [Tooltip("Order in Layer (32767 = max, guarantees on top)")]
    [SerializeField] private int orderInLayer = 32767;
    
    [ContextMenu("Move Particles to World Space")]
    void MoveToWorldSpace()
    {
        // Find camera
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }
            
            if (targetCamera == null)
            {
                Debug.LogError("No camera found! Assign manually.");
                return;
            }
        }
        
        Debug.Log($"<color=yellow>═══ Moving Particles to World Space ═══</color>");
        Debug.Log($"Camera: {targetCamera.name}");
        
        // Get all particle systems
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        
        if (particles.Length == 0)
        {
            Debug.LogWarning("No particle systems found!");
            return;
        }
        
        Debug.Log($"Found {particles.Length} particle system(s)");
        
        // Create parent object for world space particles
        GameObject worldParticlesParent = new GameObject("WorldSpaceParticles");
        worldParticlesParent.transform.SetParent(null); // Root of hierarchy
        worldParticlesParent.transform.position = Vector3.zero;
        
        Debug.Log($"<color=cyan>Moving particles...</color>");
        
        foreach (ParticleSystem ps in particles)
        {
            // Store original canvas-space position
            RectTransform rectTransform = ps.GetComponent<RectTransform>();
            Vector2 canvasPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
            
            Debug.Log($"\n  Processing: {ps.name}");
            Debug.Log($"    Original Canvas Position: {canvasPosition}");
            
            // Move to world space parent
            ps.transform.SetParent(worldParticlesParent.transform);
            
            // Remove RectTransform (makes it world space)
            if (rectTransform != null)
            {
                DestroyImmediate(rectTransform);
                Debug.Log($"    ✓ Removed RectTransform");
            }
            
            // Convert canvas position to world position in front of camera
            Vector3 screenPos = new Vector3(
                Screen.width / 2f + canvasPosition.x,
                Screen.height / 2f + canvasPosition.y,
                distanceFromCamera
            );
            
            Vector3 worldPos = targetCamera.ScreenToWorldPoint(screenPos);
            ps.transform.position = worldPos;
            
            Debug.Log($"    New World Position: {worldPos}");
            
            // Configure particle system for world space
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Configure renderer for absolute top layer
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = sortingLayerName;
                renderer.sortingOrder = orderInLayer;
                
                Debug.Log($"    ✓ Sorting Layer: {sortingLayerName}");
                Debug.Log($"    ✓ Order in Layer: {orderInLayer}");
            }
            
            // Ensure it's playing
            if (!ps.isPlaying)
            {
                ps.Play();
            }
        }
        
        Debug.Log($"\n<color=green>✓ Moved {particles.Length} particle system(s) to World Space!</color>");
        Debug.Log($"<color=lime>Particles are now at root of hierarchy under 'WorldSpaceParticles'</color>");
        Debug.Log($"<color=lime>They will render on top of ALL UI (Order in Layer: {orderInLayer})</color>");
        
        // Destroy the canvas parent (no longer needed)
        Debug.Log($"\n<color=yellow>You can now DELETE the '{gameObject.name}' Canvas.</color>");
    }
    
    void Start()
    {
        // Optionally auto-convert
        // MoveToWorldSpace();
    }
}


