using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ensures only one EventSystem exists when loading scenes.
/// Attach to a GameObject in each scene that uses Unity UI.
/// </summary>
public class SceneInitializer : MonoBehaviour
{
    [Header("Scene Setup")]
    [SerializeField] private bool checkEventSystem = true;
    [SerializeField] private bool checkForBlockingCanvases = true;
    
    void Awake()
    {
        if (checkEventSystem)
        {
            EnsureSingleEventSystem();
        }
        
        if (checkForBlockingCanvases)
        {
            CheckForBlockingCanvases();
        }
    }
    
    void EnsureSingleEventSystem()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>(true);
        
        if (eventSystems.Length == 0)
        {
            Debug.LogWarning("[SceneInitializer] No EventSystem found! Creating one.");
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }
        else if (eventSystems.Length > 1)
        {
            Debug.LogWarning($"[SceneInitializer] Found {eventSystems.Length} EventSystems! Keeping only one.");
            
            // Keep the first one, destroy the rest
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Debug.Log($"[SceneInitializer] Destroying duplicate EventSystem: {eventSystems[i].name}");
                Destroy(eventSystems[i].gameObject);
            }
        }
        else
        {
            Debug.Log($"[SceneInitializer] EventSystem found: {eventSystems[0].name}");
        }
    }
    
    void CheckForBlockingCanvases()
    {
        // Find all canvases that might be blocking input
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);
        
        foreach (Canvas canvas in allCanvases)
        {
            // Check for inactive canvases with enabled raycasters
            if (!canvas.gameObject.activeInHierarchy)
            {
                var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster != null && raycaster.enabled)
                {
                    Debug.LogWarning($"[SceneInitializer] Found inactive canvas with enabled raycaster: {canvas.name}. This might block input.");
                }
            }
            
            // Check for DontDestroyOnLoad canvases
            if (canvas.gameObject.scene.name == "DontDestroyOnLoad")
            {
                // Check if it's active but shouldn't be
                if (canvas.gameObject.activeInHierarchy && 
                    (canvas.name.Contains("Transition") || canvas.name.Contains("Video")))
                {
                    var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                    if (raycaster != null && raycaster.enabled)
                    {
                        Debug.LogWarning($"[SceneInitializer] Persistent transition canvas has enabled raycaster: {canvas.name}. Disabling.");
                        raycaster.enabled = false;
                    }
                }
            }
        }
    }
    
    [ContextMenu("Debug: List All Canvases")]
    void DebugListCanvases()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);
        Debug.Log($"<color=cyan>━━━ Found {allCanvases.Length} Canvas(es) ━━━</color>");
        
        foreach (Canvas canvas in allCanvases)
        {
            string sceneName = canvas.gameObject.scene.name;
            bool isActive = canvas.gameObject.activeInHierarchy;
            int sortOrder = canvas.sortingOrder;
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            bool hasRaycaster = raycaster != null;
            bool raycasterEnabled = hasRaycaster && raycaster.enabled;
            
            Debug.Log($"  {canvas.name} (Scene: {sceneName}, Active: {isActive}, Sort: {sortOrder}, Raycaster: {hasRaycaster}/{raycasterEnabled})");
        }
    }
    
    [ContextMenu("Debug: List All EventSystems")]
    void DebugListEventSystems()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>(true);
        Debug.Log($"<color=cyan>━━━ Found {eventSystems.Length} EventSystem(s) ━━━</color>");
        
        foreach (EventSystem eventSystem in eventSystems)
        {
            string sceneName = eventSystem.gameObject.scene.name;
            bool isActive = eventSystem.gameObject.activeInHierarchy;
            bool isEnabled = eventSystem.enabled;
            
            Debug.Log($"  {eventSystem.name} (Scene: {sceneName}, Active: {isActive}, Enabled: {isEnabled})");
        }
    }
}


