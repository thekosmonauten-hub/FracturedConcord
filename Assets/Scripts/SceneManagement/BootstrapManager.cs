using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages the Bootstrap scene and initial game setup.
/// Loads once at game start and never unloads.
/// Handles initialization of persistent managers and loads the initial scene.
/// </summary>
public class BootstrapManager : MonoBehaviour
{
    [Header("Initial Scene")]
    [Tooltip("Scene to load after bootstrap initialization")]
    [SerializeField] private string initialSceneName = "MainMenu";
    
    [Header("Bootstrap Settings")]
    [Tooltip("Wait for asset preloading before loading initial scene")]
    [SerializeField] private bool waitForAssetPreload = true;
    
    [Tooltip("Minimum time to wait before loading initial scene (ensures managers are ready)")]
    [SerializeField] private float minInitializationTime = 0.5f;
    
    private static BootstrapManager _instance;
    public static BootstrapManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<BootstrapManager>();
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        // Ensure this is the only BootstrapManager
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
    }
    
    private void Start()
    {
        StartCoroutine(InitializeBootstrap());
    }
    
    /// <summary>
    /// Initialize bootstrap systems and load initial scene
    /// </summary>
    private IEnumerator InitializeBootstrap()
    {
        Debug.Log("[BootstrapManager] Starting bootstrap initialization...");
        
        // Show loading screen
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.Show();
            LoadingScreenManager.Instance.SetProgress(0.1f);
            LoadingScreenManager.Instance.SetStatus("Initializing systems...");
        }
        
        float startTime = Time.realtimeSinceStartup;
        
        // Initialize core managers (10-30%)
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.SetProgress(0.1f);
            LoadingScreenManager.Instance.SetStatus("Loading managers...");
        }
        yield return null;
        
        // Wait for asset preloader to finish (if enabled) (30-70%)
        if (waitForAssetPreload)
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.SetProgress(0.3f);
                LoadingScreenManager.Instance.SetStatus("Preloading assets...");
            }
            
            // Wait a frame for preloader to start
            yield return null;
            
            // Wait for preloading to complete with progress updates
            if (AssetPreloader.Instance != null)
            {
                Debug.Log("[BootstrapManager] Waiting for asset preloading...");
                while (!AssetPreloader.Instance.IsPreloadComplete)
                {
                    // Update progress while waiting (30% -> 70%)
                    if (LoadingScreenManager.Instance != null)
                    {
                        float preloadProgress = 0.3f + (Time.realtimeSinceStartup - startTime) * 0.4f / minInitializationTime;
                        LoadingScreenManager.Instance.SetProgress(Mathf.Min(preloadProgress, 0.7f));
                    }
                    yield return null;
                }
                Debug.Log("[BootstrapManager] Asset preloading complete!");
            }
        }
        
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.SetProgress(0.7f);
            LoadingScreenManager.Instance.SetStatus("Preparing game...");
        }
        
        // Ensure minimum initialization time
        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < minInitializationTime)
        {
            yield return new WaitForSeconds(minInitializationTime - elapsed);
        }
        
        // Wait one more frame to ensure all managers are ready
        yield return null;
        
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.SetProgress(0.9f);
            LoadingScreenManager.Instance.SetStatus("Preparing to load scene...");
        }
        
        // Wait a moment to show 90%
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log($"[BootstrapManager] Bootstrap ready, completing loading screen before transition");
        
        // Complete loading screen first - reach 100% and wait for it to fully complete
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.SetProgress(1f);
            LoadingScreenManager.Instance.SetStatus("Complete!");
            
            // Wait for progress bar to animate to 100% (smooth animation)
            while (LoadingScreenManager.Instance.CurrentProgress < 0.99f)
            {
                yield return null;
            }
            
            // Wait a bit more to show "Complete!" message at 100%
            yield return new WaitForSeconds(0.3f);
            
            // Now hide the loading screen and wait for fade-out to complete
            yield return LoadingScreenManager.Instance.HideAndWait();
        }
        
        // Wait a frame to ensure loading screen is fully hidden
        yield return null;
        
        Debug.Log($"[BootstrapManager] Loading screen hidden, starting transition to: {initialSceneName}");
        
        // Load initial scene additively (Bootstrap scene stays loaded) with black curtain transition
        if (TransitionManager.Instance != null)
        {
            // Use curtain transition (black) instead of regular fade
            TransitionManager.Instance.TransitionToSceneWithCurtain(initialSceneName);
        }
        else
        {
            // Fallback to regular loading
            Debug.LogWarning("[BootstrapManager] TransitionManager not available, using regular scene load");
            SceneManager.LoadScene(initialSceneName);
        }
        
        Debug.Log("[BootstrapManager] ✅ Bootstrap initialization complete!");
    }
}

