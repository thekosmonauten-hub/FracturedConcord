using UnityEngine;
using UnityEngine.SceneManagement;
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
        StartCoroutine(InitializeSceneCoroutine(scene));
    }
}

