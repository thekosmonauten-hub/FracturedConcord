using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton database that stores and retrieves Reliance Aura data.
/// Loads Reliance Aura definitions from Resources folder.
/// </summary>
public class RelianceAuraDatabase : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Load Reliance Aura definitions from Resources folder")]
    [SerializeField] private bool loadFromResources = true;
    
    [Tooltip("Resources folder path (relative to Resources/)")]
    [SerializeField] private string resourcesPath = "RelianceAuras";
    
    [Header("Manual Assignment (if not loading from Resources)")]
    [Tooltip("Manually assigned Reliance Aura definitions")]
    [SerializeField] private List<RelianceAura> auras = new List<RelianceAura>();
    
    private static RelianceAuraDatabase _instance;
    public static RelianceAuraDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<RelianceAuraDatabase>();
                
                if (_instance == null)
                {
                    // Create new instance if none exists
                    GameObject go = new GameObject("RelianceAuraDatabase");
                    _instance = go.AddComponent<RelianceAuraDatabase>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAuras();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Load all Reliance Aura definitions
    /// </summary>
    void LoadAuras()
    {
        if (loadFromResources)
        {
            RelianceAura[] loadedAuras = Resources.LoadAll<RelianceAura>(resourcesPath);
            
            if (loadedAuras != null && loadedAuras.Length > 0)
            {
                auras = new List<RelianceAura>(loadedAuras);
                Debug.Log($"[RelianceAuraDatabase] Loaded {auras.Count} Reliance Auras from Resources/{resourcesPath}");
            }
            else
            {
                Debug.LogWarning($"[RelianceAuraDatabase] No Reliance Auras found in Resources/{resourcesPath}");
            }
        }
        else
        {
            Debug.Log($"[RelianceAuraDatabase] Using {auras.Count} manually assigned Reliance Auras");
        }
    }
    
    /// <summary>
    /// Get all Reliance Auras for a specific category
    /// </summary>
    public List<RelianceAura> GetAurasForCategory(string category)
    {
        if (auras == null || auras.Count == 0)
        {
            Debug.LogWarning($"[RelianceAuraDatabase] No Reliance Auras loaded!");
            return new List<RelianceAura>();
        }
        
        var categoryAuras = auras
            .Where(a => a != null && a.category.Equals(category, System.StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        if (categoryAuras.Count == 0)
        {
            Debug.LogWarning($"[RelianceAuraDatabase] No Reliance Auras found for category: {category}");
        }
        
        return categoryAuras;
    }
    
    /// <summary>
    /// Get a specific Reliance Aura by name
    /// </summary>
    public RelianceAura GetAura(string auraName)
    {
        return auras?.FirstOrDefault(a => 
            a != null && a.auraName.Equals(auraName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Get all loaded Reliance Auras
    /// </summary>
    public List<RelianceAura> GetAllAuras()
    {
        return auras ?? new List<RelianceAura>();
    }
    
    /// <summary>
    /// Check if a Reliance Aura exists
    /// </summary>
    public bool HasAura(string auraName)
    {
        return GetAura(auraName) != null;
    }
    
    /// <summary>
    /// Get all unique categories
    /// </summary>
    public List<string> GetAllCategories()
    {
        if (auras == null || auras.Count == 0)
            return new List<string>();
        
        return auras
            .Where(a => a != null && !string.IsNullOrEmpty(a.category))
            .Select(a => a.category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }
}

