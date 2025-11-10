using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Finds all canvases and their rendering order to diagnose particle visibility issues.
/// Attach to any GameObject and run in Play Mode.
/// </summary>
public class CanvasHierarchyDebugger : MonoBehaviour
{
    [ContextMenu("Debug All Canvases")]
    void DebugAllCanvases()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true); // Include inactive
        
        Debug.Log($"<color=cyan>═══ Found {allCanvases.Length} Canvas(es) ═══</color>");
        
        System.Array.Sort(allCanvases, (a, b) => a.sortingOrder.CompareTo(b.sortingOrder));
        
        foreach (Canvas canvas in allCanvases)
        {
            Debug.Log($"\n<color=yellow>━━━ {canvas.name} ━━━</color>");
            Debug.Log($"  Render Mode: {canvas.renderMode}");
            Debug.Log($"  Sort Order: {canvas.sortingOrder}");
            Debug.Log($"  Override Sorting: {canvas.overrideSorting}");
            Debug.Log($"  Enabled: {canvas.enabled}");
            Debug.Log($"  Active: {canvas.gameObject.activeInHierarchy}");
            
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
            {
                Debug.Log($"  World Camera: {(canvas.worldCamera != null ? canvas.worldCamera.name : "NULL")}");
                Debug.Log($"  Plane Distance: {canvas.planeDistance}");
            }
            
            // Check for Outline components
            Outline[] outlines = canvas.GetComponentsInChildren<Outline>(true);
            if (outlines.Length > 0)
            {
                Debug.Log($"  <color=orange>⚠️ Contains {outlines.Length} Outline component(s)</color>");
                foreach (Outline outline in outlines)
                {
                    Debug.Log($"    - {outline.gameObject.name}: {outline.effectColor}");
                }
            }
            
            // Check for Images/Graphics
            Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(true);
            Debug.Log($"  Contains {graphics.Length} Graphic component(s) (Images, Text, etc.)");
            
            // Check for particle systems
            ParticleSystem[] particles = canvas.GetComponentsInChildren<ParticleSystem>(true);
            if (particles.Length > 0)
            {
                Debug.Log($"  <color=lime>✓ Contains {particles.Length} Particle System(s)</color>");
            }
        }
        
        Debug.Log($"\n<color=cyan>═══ Rendering Order (lowest to highest) ═══</color>");
        for (int i = 0; i < allCanvases.Length; i++)
        {
            Debug.Log($"{i + 1}. {allCanvases[i].name} (Sort: {allCanvases[i].sortingOrder})");
        }
    }
    
    [ContextMenu("Find Card GameObjects")]
    void FindCardObjects()
    {
        // Try to find objects with "Deck" or "Card" in name
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        
        Debug.Log($"<color=cyan>═══ Looking for Card/Deck Objects ═══</color>");
        
        int found = 0;
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Deck") || obj.name.Contains("Card") || obj.name.Contains("Button"))
            {
                Canvas parentCanvas = obj.GetComponentInParent<Canvas>();
                string canvasName = parentCanvas != null ? parentCanvas.name : "NO CANVAS";
                int sortOrder = parentCanvas != null ? parentCanvas.sortingOrder : -999;
                
                Debug.Log($"  {obj.name} → Canvas: {canvasName} (Sort: {sortOrder})");
                
                // Check for Outline
                Outline[] outlines = obj.GetComponents<Outline>();
                if (outlines.Length > 0)
                {
                    Debug.Log($"    <color=orange>Has {outlines.Length} Outline(s)</color>");
                }
                
                found++;
            }
        }
        
        Debug.Log($"<color=lime>Found {found} card/deck/button object(s)</color>");
    }
    
    void Start()
    {
        // Auto-run in Play Mode
        Invoke(nameof(DebugAllCanvases), 0.5f);
        Invoke(nameof(FindCardObjects), 0.6f);
    }
}


