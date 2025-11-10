using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cleans up persistent PassiveTree canvases when leaving the PassiveTreeScene
/// to prevent them from blocking backgrounds in other scenes.
/// </summary>
public class PassiveTreeSceneCleanup : MonoBehaviour
{
    [Header("Cleanup Settings")]
    [SerializeField] private bool destroyTooltipCanvasOnExit = true;
    [SerializeField] private bool destroyPersistentCanvasesOnExit = true;
    
    [Header("Canvas Names to Clean Up")]
    [SerializeField] private string[] canvasNamesToDestroy = new string[]
    {
        "PassiveTreeTooltipCanvas",
        "WorldSpaceTooltipCanvas",
        "PersistentCanvas"
    };
    
    private void Awake()
    {
        // Subscribe to scene unload event
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    
    /// <summary>
    /// Called when any scene is unloaded
    /// </summary>
    private void OnSceneUnloaded(Scene scene)
    {
        // Only clean up when leaving PassiveTreeScene
        if (scene.name == "PassiveTreeScene" || scene.name == "PassiveTreeScene_new")
        {
            CleanupPersistentCanvases();
        }
    }
    
    /// <summary>
    /// Clean up persistent canvases that may block other scenes
    /// </summary>
    private void CleanupPersistentCanvases()
    {
        if (!destroyPersistentCanvasesOnExit) return;
        
        foreach (string canvasName in canvasNamesToDestroy)
        {
            GameObject persistentCanvas = GameObject.Find(canvasName);
            if (persistentCanvas != null)
            {
                // Check if it's in the DontDestroyOnLoad scene
                if (persistentCanvas.scene.name == "DontDestroyOnLoad")
                {
                    Destroy(persistentCanvas);
                    Debug.Log($"[PassiveTreeSceneCleanup] Destroyed persistent canvas: {canvasName}");
                }
            }
        }
    }
    
    /// <summary>
    /// Manually trigger cleanup (can be called from UI buttons)
    /// </summary>
    [ContextMenu("Manual Cleanup Persistent Canvases")]
    public void ManualCleanup()
    {
        CleanupPersistentCanvases();
    }
    
    /// <summary>
    /// Find and list all DontDestroyOnLoad canvases for debugging
    /// </summary>
    [ContextMenu("Debug: List Persistent Canvases")]
    public void DebugListPersistentCanvases()
    {
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"[PassiveTreeSceneCleanup] Found {allCanvases.Length} total canvases");
        
        int persistentCount = 0;
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.gameObject.scene.name == "DontDestroyOnLoad")
            {
                persistentCount++;
                Debug.Log($"[PassiveTreeSceneCleanup] Persistent Canvas: {canvas.gameObject.name} | SortOrder: {canvas.sortingOrder} | RenderMode: {canvas.renderMode}");
            }
        }
        
        Debug.Log($"[PassiveTreeSceneCleanup] Total persistent canvases: {persistentCount}");
    }
}




















