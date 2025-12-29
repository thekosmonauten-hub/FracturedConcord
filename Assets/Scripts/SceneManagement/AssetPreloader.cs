using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Preloads commonly used assets at game startup to prevent loading delays during scene transitions.
/// Based on Unity Scene Loading best practices - preload assets that are reused constantly.
/// </summary>
public class AssetPreloader : MonoBehaviour
{
    private static AssetPreloader _instance;
    public static AssetPreloader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AssetPreloader>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AssetPreloader");
                    _instance = go.AddComponent<AssetPreloader>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Preload Settings")]
    [Tooltip("Whether to preload assets on startup")]
    [SerializeField] private bool preloadOnStart = true;
    
    [Tooltip("Maximum time per frame to spend loading assets (prevents freezing)")]
    [SerializeField] private float maxLoadTimePerFrame = 0.016f; // ~1 frame at 60fps
    
    // Cached asset references
    private Dictionary<string, Object> cachedAssets = new Dictionary<string, Object>();
    private bool isPreloading = false;
    private bool preloadComplete = false;
    
    /// <summary>
    /// Whether asset preloading has completed
    /// </summary>
    public bool IsPreloadComplete => preloadComplete;
    
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
        
        if (preloadOnStart)
        {
            StartCoroutine(PreloadCommonAssets());
        }
    }
    
    /// <summary>
    /// Preload commonly used assets that are accessed frequently.
    /// This should include: Card databases, Warrant databases, Item databases, etc.
    /// </summary>
    private IEnumerator PreloadCommonAssets()
    {
        if (isPreloading || preloadComplete)
            yield break;
        
        isPreloading = true;
        Debug.Log("[AssetPreloader] Starting asset preload...");
        
        float frameStartTime = Time.realtimeSinceStartup;
        int assetsLoaded = 0;
        
        // List of common assets to preload
        // Priority: Most frequently accessed assets first
        string[] commonAssets = new string[]
        {
            // EquipmentScreen critical dependencies (loaded immediately on scene load)
            "CurrencyDatabase",           // ⚠️ CRITICAL - Loaded in CurrencyManager, LootManager, etc.
            "CardVisualAssets",           // ⚠️ CRITICAL - Loaded in CardCarouselUI, CardDisplay
            "ItemDatabase",               // Used by inventory grids
            "EmbossingDatabase",          // ⚠️ CRITICAL - Replaces slow Resources.LoadAll for embossings
            
            // Warrant system (already partially preloaded, but ensure completeness)
            "WarrantDatabase",
            "WarrantNotableDatabase",
            "WarrantAffixDatabase",
            "WarrantIconLibrary",
            
            // Affix databases (used for item generation/display)
            "AffixDatabase",
            "AffixDatabase_Modern",
            "EffigyAffixDatabase",
            
            // Other common databases
            "CardDatabase",
            "ForgeMaterialDatabase",
            "MazeForgeAffixDatabase",
            "StatusDatabase",
            "EffectsDatabase",
            
            // Add more common assets here
        };
        
        foreach (string assetName in commonAssets)
        {
            // Check if already cached
            if (cachedAssets.ContainsKey(assetName))
                continue;
            
            // Load asset
            float loadStart = Time.realtimeSinceStartup;
            Object asset = Resources.Load(assetName);
            
            if (asset != null)
            {
                cachedAssets[assetName] = asset;
                assetsLoaded++;
                Debug.Log($"[AssetPreloader] Preloaded: {assetName}");
            }
            else
            {
                Debug.LogWarning($"[AssetPreloader] Failed to preload: {assetName} (not found in Resources)");
            }
            
            // Check if we've spent too much time this frame
            float elapsed = Time.realtimeSinceStartup - frameStartTime;
            if (elapsed >= maxLoadTimePerFrame)
            {
                yield return null; // Wait a frame before continuing
                frameStartTime = Time.realtimeSinceStartup;
            }
        }
        
        preloadComplete = true;
        isPreloading = false;
        Debug.Log($"[AssetPreloader] ✅ Preload complete! Loaded {assetsLoaded} assets.");
    }
    
    /// <summary>
    /// Get a preloaded asset by name (faster than Resources.Load).
    /// </summary>
    public T GetPreloadedAsset<T>(string assetName) where T : Object
    {
        if (cachedAssets.TryGetValue(assetName, out Object asset))
        {
            return asset as T;
        }
        
        // Fallback to Resources.Load if not preloaded
        return Resources.Load<T>(assetName);
    }
    
    /// <summary>
    /// Check if an asset has been preloaded.
    /// </summary>
    public bool IsPreloaded(string assetName)
    {
        return cachedAssets.ContainsKey(assetName);
    }
    
    /// <summary>
    /// Manually trigger preloading (useful if preloadOnStart is false).
    /// </summary>
    public void StartPreload()
    {
        if (!isPreloading && !preloadComplete)
        {
            StartCoroutine(PreloadCommonAssets());
        }
    }
}

