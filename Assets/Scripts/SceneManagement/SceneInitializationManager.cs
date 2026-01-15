using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages deferred scene initialization to prevent blocking during scene loads.
/// Finds all ISceneInitializable components and initializes them across multiple frames.
/// </summary>
public class SceneInitializationManager : MonoBehaviour
{
    private static SceneInitializationManager _instance;
    public static SceneInitializationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SceneInitializationManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SceneInitializationManager");
                    _instance = go.AddComponent<SceneInitializationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Initialization Settings")]
    [Tooltip("Maximum time per frame to spend on initialization (in seconds). Lower = smoother but slower.")]
    [SerializeField] private float maxTimePerFrame = 0.016f; // ~1 frame at 60fps
    
    [Tooltip("Number of components to initialize per frame. Higher = faster but may cause stuttering.")]
    [SerializeField] private int componentsPerFrame = 1;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Subscribe to scene loaded events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    /// <summary>
    /// Called when a new scene is loaded. Starts deferred initialization.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Clean up duplicate EventSystems and AudioListeners when using additive loading
        if (mode == LoadSceneMode.Additive)
        {
            CleanupDuplicateComponents(scene);
        }
        
        // Only initialize if this is the active scene (not additive loading)
        if (scene == SceneManager.GetActiveScene())
        {
            StartCoroutine(InitializeSceneCoroutine(scene));
        }
    }
    
    /// <summary>
    /// Initialize all ISceneInitializable components in the scene across multiple frames.
    /// </summary>
    private IEnumerator InitializeSceneCoroutine(Scene scene)
    {
        // Wait a frame to ensure all objects are fully instantiated
        yield return null;
        
        // Clean up duplicate EventSystems and AudioListeners (handles both additive and single scene loading)
        CleanupDuplicateComponents(scene);
        
        // Find all ISceneInitializable components in the scene
        var initializables = new List<ISceneInitializable>();
        var rootObjects = scene.GetRootGameObjects();
        
        foreach (var rootObj in rootObjects)
        {
            initializables.AddRange(rootObj.GetComponentsInChildren<ISceneInitializable>(true));
        }
        
        if (initializables.Count == 0)
        {
            Debug.Log($"[SceneInitializationManager] No ISceneInitializable components found in scene: {scene.name}");
            yield break;
        }
        
        Debug.Log($"[SceneInitializationManager] Found {initializables.Count} components to initialize in scene: {scene.name}");
        
        // Initialize components across multiple frames
        int initialized = 0;
        float frameStartTime = Time.realtimeSinceStartup;
        
        foreach (var initializable in initializables)
        {
            if (initializable == null || initializable.IsInitialized)
                continue;
            
            // Start initialization coroutine
            StartCoroutine(InitializeComponentCoroutine(initializable));
            initialized++;
            
            // Check if we've spent too much time this frame
            float elapsed = Time.realtimeSinceStartup - frameStartTime;
            if (elapsed >= maxTimePerFrame || initialized >= componentsPerFrame)
            {
                yield return null; // Wait a frame before continuing
                frameStartTime = Time.realtimeSinceStartup;
                initialized = 0;
            }
        }
        
        Debug.Log($"[SceneInitializationManager] Started initialization for {initializables.Count} components in scene: {scene.name}");
    }
    
    /// <summary>
    /// Initialize a single component and wait for it to complete.
    /// </summary>
    private IEnumerator InitializeComponentCoroutine(ISceneInitializable initializable)
    {
        if (initializable == null || initializable.IsInitialized)
            yield break;
        
        string componentName = (initializable as MonoBehaviour)?.name ?? "Unknown";
        Debug.Log($"[SceneInitializationManager] Initializing: {componentName}");
        
        yield return initializable.Initialize();
        
        if (initializable.IsInitialized)
        {
            Debug.Log($"[SceneInitializationManager] ✅ Initialized: {componentName}");
        }
        else
        {
            Debug.LogWarning($"[SceneInitializationManager] ⚠️ Component {componentName} reports not initialized after Initialize() completed");
        }
    }
    
    /// <summary>
    /// Manually trigger initialization for a specific scene (useful for additive loading).
    /// </summary>
    public void InitializeScene(Scene scene)
    {
        // Clean up duplicates before initializing
        CleanupDuplicateComponents(scene);
        StartCoroutine(InitializeSceneCoroutine(scene));
    }
    
    /// <summary>
    /// Ensures only one EventSystem and one AudioListener exist across all loaded scenes.
    /// Prefers components from the newly loaded scene over the Bootstrap scene.
    /// </summary>
    private void CleanupDuplicateComponents(Scene newlyLoadedScene)
    {
        // Check if Bootstrap scene is loaded (indicates additive loading)
        Scene bootstrapScene = SceneManager.GetSceneByName("BootstrapScene");
        bool isAdditiveLoading = bootstrapScene.IsValid() && newlyLoadedScene != bootstrapScene;
        
        if (!isAdditiveLoading)
        {
            // Single scene loading - just ensure one of each exists
            EnsureSingleEventSystem(null);
            EnsureSingleAudioListener(null);
            return;
        }
        
        // Additive loading - prefer components from newly loaded scene, remove duplicates from Bootstrap
        EnsureSingleEventSystem(newlyLoadedScene);
        EnsureSingleAudioListener(newlyLoadedScene);
    }
    
    /// <summary>
    /// Ensures only one EventSystem exists. If preferredScene is provided, keeps the one from that scene.
    /// </summary>
    private void EnsureSingleEventSystem(Scene? preferredScene)
    {
        EventSystem[] allEventSystems = FindObjectsOfType<EventSystem>(true);
        
        if (allEventSystems.Length == 0)
        {
            Debug.LogWarning("[SceneInitializationManager] No EventSystem found! Creating one.");
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            return;
        }
        
        if (allEventSystems.Length == 1)
        {
            // Already only one - perfect
            return;
        }
        
        // Multiple EventSystems found - need to clean up
        Debug.Log($"[SceneInitializationManager] Found {allEventSystems.Length} EventSystems. Cleaning up duplicates...");
        
        EventSystem eventSystemToKeep = null;
        List<EventSystem> eventSystemsToDestroy = new List<EventSystem>();
        
        // If we have a preferred scene, try to keep the EventSystem from that scene
        if (preferredScene.HasValue && preferredScene.Value.IsValid())
        {
            Scene preferredSceneValue = preferredScene.Value;
            foreach (var es in allEventSystems)
            {
                if (es.gameObject.scene == preferredSceneValue)
                {
                    eventSystemToKeep = es;
                    break;
                }
            }
        }
        
        // If no preferred scene or didn't find one in preferred scene, keep the first active one
        if (eventSystemToKeep == null)
        {
            eventSystemToKeep = allEventSystems.FirstOrDefault(es => es.gameObject.activeInHierarchy) ?? allEventSystems[0];
        }
        
        // Mark all others for destruction
        foreach (var es in allEventSystems)
        {
            if (es != eventSystemToKeep)
            {
                eventSystemsToDestroy.Add(es);
            }
        }
        
        // Destroy duplicates
        foreach (var es in eventSystemsToDestroy)
        {
            string sceneName = es.gameObject.scene.name;
            Debug.Log($"[SceneInitializationManager] Destroying duplicate EventSystem '{es.name}' from scene '{sceneName}'");
            Destroy(es.gameObject);
        }
        
        Debug.Log($"[SceneInitializationManager] Kept EventSystem '{eventSystemToKeep.name}' from scene '{eventSystemToKeep.gameObject.scene.name}'");
    }
    
    /// <summary>
    /// Ensures only one AudioListener exists. If preferredScene is provided, keeps the one from that scene.
    /// </summary>
    private void EnsureSingleAudioListener(Scene? preferredScene)
    {
        AudioListener[] allAudioListeners = FindObjectsOfType<AudioListener>(true);
        
        if (allAudioListeners.Length == 0)
        {
            Debug.LogWarning("[SceneInitializationManager] No AudioListener found!");
            return;
        }
        
        if (allAudioListeners.Length == 1)
        {
            // Already only one - perfect
            return;
        }
        
        // Multiple AudioListeners found - need to clean up
        Debug.Log($"[SceneInitializationManager] Found {allAudioListeners.Length} AudioListeners. Cleaning up duplicates...");
        
        AudioListener listenerToKeep = null;
        List<AudioListener> listenersToDestroy = new List<AudioListener>();
        
        // If we have a preferred scene, try to keep the AudioListener from that scene
        if (preferredScene.HasValue && preferredScene.Value.IsValid())
        {
            Scene preferredSceneValue = preferredScene.Value;
            foreach (var al in allAudioListeners)
            {
                if (al.gameObject.scene == preferredSceneValue)
                {
                    listenerToKeep = al;
                    break;
                }
            }
        }
        
        // If no preferred scene or didn't find one in preferred scene, keep the first active one
        if (listenerToKeep == null)
        {
            listenerToKeep = allAudioListeners.FirstOrDefault(al => al.gameObject.activeInHierarchy) ?? allAudioListeners[0];
        }
        
        // Mark all others for destruction
        foreach (var al in allAudioListeners)
        {
            if (al != listenerToKeep)
            {
                listenersToDestroy.Add(al);
            }
        }
        
        // Destroy duplicates
        foreach (var al in listenersToDestroy)
        {
            string sceneName = al.gameObject.scene.name;
            Debug.Log($"[SceneInitializationManager] Destroying duplicate AudioListener on '{al.name}' from scene '{sceneName}'");
            Destroy(al);
        }
        
        Debug.Log($"[SceneInitializationManager] Kept AudioListener on '{listenerToKeep.name}' from scene '{listenerToKeep.gameObject.scene.name}'");
    }
}

