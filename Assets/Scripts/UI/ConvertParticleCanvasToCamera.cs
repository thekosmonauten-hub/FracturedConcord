using UnityEngine;

/// <summary>
/// Converts ParticleEffectsCanvas from Screen Space - Overlay to Screen Space - Camera
/// This fixes rendering order issues where particles appear behind UI elements.
/// Attach to ParticleEffectsCanvas, run once, then remove.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class ConvertParticleCanvasToCamera : MonoBehaviour
{
    [Header("Conversion Settings")]
    [Tooltip("Leave empty to auto-find main camera")]
    [SerializeField] private Camera targetCamera;
    
    [Tooltip("Distance from camera (lower = closer to camera = renders on top)")]
    [SerializeField] private float planeDistance = 1f;
    
    [Header("Sorting")]
    [SerializeField] private int sortingOrder = 100;
    
    [ContextMenu("Convert to Camera Mode")]
    void ConvertToCameraMode()
    {
        Canvas canvas = GetComponent<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("No Canvas component found!");
            return;
        }
        
        // Find main camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }
            
            if (targetCamera == null)
            {
                Debug.LogError("No camera found! Assign a camera manually.");
                return;
            }
        }
        
        Debug.Log($"<color=yellow>Converting Canvas to Screen Space - Camera mode...</color>");
        Debug.Log($"Using camera: {targetCamera.name}");
        
        // Convert to Camera mode
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = targetCamera;
        canvas.planeDistance = planeDistance;
        canvas.sortingOrder = sortingOrder;
        
        // Ensure override sorting is enabled (required for Camera mode)
        if (!canvas.overrideSorting)
        {
            canvas.overrideSorting = true;
            Debug.Log("Enabled Override Sorting");
        }
        
        Debug.Log($"<color=green>✓ Conversion complete!</color>");
        Debug.Log($"Render Mode: {canvas.renderMode}");
        Debug.Log($"World Camera: {canvas.worldCamera.name}");
        Debug.Log($"Plane Distance: {canvas.planeDistance}");
        Debug.Log($"Sort Order: {canvas.sortingOrder}");
        Debug.Log($"Override Sorting: {canvas.overrideSorting}");
        
        // Check particle renderers
        ParticleSystemRenderer[] renderers = GetComponentsInChildren<ParticleSystemRenderer>();
        Debug.Log($"<color=cyan>Checking {renderers.Length} particle renderer(s)...</color>");
        
        foreach (ParticleSystemRenderer renderer in renderers)
        {
            Debug.Log($"  {renderer.gameObject.name}: Layer={renderer.sortingLayerName}, Order={renderer.sortingOrder}");
        }
        
        Debug.Log("<color=lime>Test the scene now. Particles should render on top!</color>");
    }
    
    void Start()
    {
        // Optionally auto-convert on start
        // ConvertToCameraMode();
    }
}


