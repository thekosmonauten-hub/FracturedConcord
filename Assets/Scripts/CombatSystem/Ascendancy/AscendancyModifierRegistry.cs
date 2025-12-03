using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Registry that automatically loads all AscendancyModifierDefinition assets from Resources
/// </summary>
public class AscendancyModifierRegistry : MonoBehaviour
{
    private static AscendancyModifierRegistry _instance;
    public static AscendancyModifierRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AscendancyModifierRegistry>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("AscendancyModifierRegistry");
                    _instance = go.AddComponent<AscendancyModifierRegistry>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Settings")]
    [Tooltip("Resources folder path for AscendancyModifierDefinitions (relative to Resources/). Loads recursively from subfolders.")]
    [SerializeField] private string resourcesPath = "AscendancyModifiers";
    
    [Header("Debug Info (Read-Only)")]
    [SerializeField] private List<AscendancyModifierDefinition> loadedModifiers = new List<AscendancyModifierDefinition>();

    private Dictionary<string, AscendancyModifierDefinition> modifiersById = new Dictionary<string, AscendancyModifierDefinition>();
    private Dictionary<string, List<AscendancyModifierDefinition>> modifiersByPassiveName = new Dictionary<string, List<AscendancyModifierDefinition>>();
    
    void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadModifiers();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    [ContextMenu("Reload Modifiers")]
    public void ReloadModifiers()
    {
        LoadModifiers();
    }

    private void LoadModifiers()
    {
        modifiersById.Clear();
        modifiersByPassiveName.Clear();
        loadedModifiers.Clear();

        // Resources.LoadAll loads recursively from subfolders, so organizing modifiers into subfolders (e.g., "AscendancyModifiers/BastionOfTolerance") works fine
        AscendancyModifierDefinition[] loadedAssets = Resources.LoadAll<AscendancyModifierDefinition>(resourcesPath);

        if (loadedAssets != null && loadedAssets.Length > 0)
        {
            foreach (var modifier in loadedAssets)
            {
                if (modifier == null || string.IsNullOrEmpty(modifier.modifierId))
                {
                    Debug.LogWarning($"[AscendancyModifierRegistry] Found null or invalid modifier asset in {resourcesPath}. Skipping.");
                    continue;
                }

                if (modifiersById.ContainsKey(modifier.modifierId))
                {
                    Debug.LogWarning($"[AscendancyModifierRegistry] Duplicate modifierId found: {modifier.modifierId}. Overwriting with the last one loaded.");
                }
                modifiersById[modifier.modifierId] = modifier;
                loadedModifiers.Add(modifier);

                if (!string.IsNullOrEmpty(modifier.linkedPassiveName))
                {
                    if (!modifiersByPassiveName.ContainsKey(modifier.linkedPassiveName))
                    {
                        modifiersByPassiveName[modifier.linkedPassiveName] = new List<AscendancyModifierDefinition>();
                    }
                    modifiersByPassiveName[modifier.linkedPassiveName].Add(modifier);
                }
            }
            Debug.Log($"[AscendancyModifierRegistry] Loaded {loadedModifiers.Count} Ascendancy Modifier Definitions from Resources/{resourcesPath}");
        }
        else
        {
            Debug.LogWarning($"[AscendancyModifierRegistry] No Ascendancy Modifier Definitions found in Resources/{resourcesPath}");
        }
    }
    
    /// <summary>
    /// Get a specific modifier by its unique ID.
    /// </summary>
    public AscendancyModifierDefinition GetModifier(string modifierId)
    {
        if (modifiersById.TryGetValue(modifierId, out AscendancyModifierDefinition modifier))
        {
            return modifier;
        }
        Debug.LogWarning($"[AscendancyModifierRegistry] Modifier with ID '{modifierId}' not found.");
        return null;
    }

    /// <summary>
    /// Get all modifiers linked to a specific passive ability name.
    /// Supports partial matching for numbered variants (e.g., "Attack and Crumble Magnitude_1" matches "Attack and Crumble Magnitude").
    /// </summary>
    public List<AscendancyModifierDefinition> GetModifiersForPassive(string passiveName)
    {
        if (string.IsNullOrEmpty(passiveName))
        {
            return new List<AscendancyModifierDefinition>();
        }

        // First try exact match
        if (modifiersByPassiveName.TryGetValue(passiveName, out List<AscendancyModifierDefinition> modifiers))
        {
            return new List<AscendancyModifierDefinition>(modifiers);
        }

        // If no exact match, try partial matching for numbered variants
        // Check if any modifier's linkedPassiveName matches the start of the passiveName
        // (e.g., passiveName = "Attack and Crumble Magnitude_1", linkedPassiveName = "Attack and Crumble Magnitude")
        List<AscendancyModifierDefinition> matchedModifiers = new List<AscendancyModifierDefinition>();
        
        foreach (var modifier in loadedModifiers)
        {
            if (modifier == null || string.IsNullOrEmpty(modifier.linkedPassiveName))
                continue;

            // Check if passiveName starts with linkedPassiveName (for numbered variants)
            if (passiveName.StartsWith(modifier.linkedPassiveName, System.StringComparison.OrdinalIgnoreCase))
            {
                // Verify it's a numbered variant (ends with _number) or exact match
                string remainder = passiveName.Substring(modifier.linkedPassiveName.Length);
                if (string.IsNullOrEmpty(remainder) || remainder.StartsWith("_"))
                {
                    matchedModifiers.Add(modifier);
                }
            }
            // Also check reverse: if linkedPassiveName starts with passiveName
            else if (modifier.linkedPassiveName.StartsWith(passiveName, System.StringComparison.OrdinalIgnoreCase))
            {
                matchedModifiers.Add(modifier);
            }
        }

        return matchedModifiers;
    }

    /// <summary>
    /// Get all loaded modifier definitions.
    /// </summary>
    public List<AscendancyModifierDefinition> GetAllModifiers()
    {
        return loadedModifiers;
    }

    /// <summary>
    /// Check if a modifier with the given ID exists.
    /// </summary>
    public bool HasModifier(string modifierId)
    {
        return modifiersById.ContainsKey(modifierId);
    }
    
    /// <summary>
    /// Get all active modifiers for a character based on their ascendancy progress.
    /// </summary>
    public List<AscendancyModifierDefinition> GetActiveModifiers(Character character)
    {
        if (character == null || character.ascendancyProgress == null)
        {
            return new List<AscendancyModifierDefinition>();
        }
        
        var activeModifiers = new List<AscendancyModifierDefinition>();
        var unlockedPassiveNames = character.ascendancyProgress.unlockedPassives;
        
        if (unlockedPassiveNames == null || unlockedPassiveNames.Count == 0)
        {
            return activeModifiers;
        }
        
        // Get modifiers for each unlocked passive name
        foreach (var passiveName in unlockedPassiveNames)
        {
            if (string.IsNullOrEmpty(passiveName))
                continue;
            
            var modifiers = GetModifiersForPassive(passiveName);
            activeModifiers.AddRange(modifiers);
        }
        
        return activeModifiers;
    }
}

