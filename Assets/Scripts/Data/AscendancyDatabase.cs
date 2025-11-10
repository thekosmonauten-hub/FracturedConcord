using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton database that stores and retrieves Ascendancy data.
/// Loads Ascendancy definitions from Resources folder.
/// </summary>
public class AscendancyDatabase : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Load Ascendancy definitions from Resources folder")]
    [SerializeField] private bool loadFromResources = true;
    
    [Tooltip("Resources folder path (relative to Resources/)")]
    [SerializeField] private string resourcesPath = "Ascendancies";
    
    [Header("Manual Assignment (if not loading from Resources)")]
    [Tooltip("Manually assigned Ascendancy definitions")]
    [SerializeField] private List<AscendancyData> ascendancies = new List<AscendancyData>();
    
    private static AscendancyDatabase _instance;
    public static AscendancyDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AscendancyDatabase>();
                
                if (_instance == null)
                {
                    // Create new instance if none exists
                    GameObject go = new GameObject("AscendancyDatabase");
                    _instance = go.AddComponent<AscendancyDatabase>();
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
            LoadAscendancies();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Load all Ascendancy definitions
    /// </summary>
    void LoadAscendancies()
    {
        if (loadFromResources)
        {
            AscendancyData[] loadedAscendancies = Resources.LoadAll<AscendancyData>(resourcesPath);
            
            if (loadedAscendancies != null && loadedAscendancies.Length > 0)
            {
                ascendancies = new List<AscendancyData>(loadedAscendancies);
                Debug.Log($"[AscendancyDatabase] Loaded {ascendancies.Count} Ascendancies from Resources/{resourcesPath}");
            }
            else
            {
                Debug.LogWarning($"[AscendancyDatabase] No Ascendancies found in Resources/{resourcesPath}");
            }
        }
        else
        {
            Debug.Log($"[AscendancyDatabase] Using {ascendancies.Count} manually assigned Ascendancies");
        }
    }
    
    /// <summary>
    /// Get all Ascendancies for a specific base class
    /// </summary>
    /// <param name="baseClassName">Base class name (e.g., "Witch", "Marauder")</param>
    /// <returns>List of Ascendancies for this class (should be 3)</returns>
    public List<AscendancyData> GetAscendanciesForClass(string baseClassName)
    {
        if (ascendancies == null || ascendancies.Count == 0)
        {
            Debug.LogWarning($"[AscendancyDatabase] No Ascendancies loaded!");
            return new List<AscendancyData>();
        }
        
        var classAscendancies = ascendancies
            .Where(a => a != null && a.baseClass.Equals(baseClassName, System.StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        if (classAscendancies.Count == 0)
        {
            Debug.LogWarning($"[AscendancyDatabase] No Ascendancies found for class: {baseClassName}");
        }
        else
        {
            Debug.Log($"[AscendancyDatabase] Found {classAscendancies.Count} Ascendancies for class: {baseClassName}");
        }
        
        return classAscendancies;
    }
    
    /// <summary>
    /// Get a specific Ascendancy by name
    /// </summary>
    public AscendancyData GetAscendancy(string ascendancyName)
    {
        return ascendancies?.FirstOrDefault(a => 
            a != null && a.ascendancyName.Equals(ascendancyName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Get all loaded Ascendancies
    /// </summary>
    public List<AscendancyData> GetAllAscendancies()
    {
        return ascendancies ?? new List<AscendancyData>();
    }
    
    /// <summary>
    /// Check if an Ascendancy exists
    /// </summary>
    public bool HasAscendancy(string ascendancyName)
    {
        return GetAscendancy(ascendancyName) != null;
    }
}

