using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Library of icons that can be randomly assigned to rolled warrant instances.
/// Also maintains backward compatibility with ID-based icon lookup for UI widgets.
/// </summary>
[CreateAssetMenu(fileName = "WarrantIconLibrary", menuName = "Dexiled/Warrants/Icon Library")]
public class WarrantIconLibrary : ScriptableObject
{
    [Header("Random Icon Pool")]
    [Tooltip("List of sprites that can be randomly assigned to rolled warrant instances. Each rolled warrant will get a random icon from this pool.")]
    [SerializeField] private List<Sprite> randomIconPool = new List<Sprite>();
    
    [Header("Legacy: ID-Based Lookup (Optional)")]
    [Tooltip("Legacy support: Maps warrant IDs to their definition assets for UI widgets. Can be empty if using random icon pool only.")]
    [SerializeField] private List<WarrantDefinition> definitions = new List<WarrantDefinition>();

    private Dictionary<string, WarrantDefinition> lookup;

    /// <summary>
    /// Gets a random icon from the icon pool. Used when rolling warrant instances from blueprints.
    /// </summary>
    public Sprite GetRandomIcon()
    {
        if (randomIconPool == null || randomIconPool.Count == 0)
        {
            Debug.LogWarning("[WarrantIconLibrary] Random icon pool is empty. Cannot get random icon.");
            return null;
        }

        // Filter out null sprites
        List<Sprite> validIcons = new List<Sprite>();
        foreach (var sprite in randomIconPool)
        {
            if (sprite != null)
            {
                validIcons.Add(sprite);
            }
        }

        if (validIcons.Count == 0)
        {
            Debug.LogWarning("[WarrantIconLibrary] No valid icons in pool.");
            return null;
        }

        // Return a random icon
        return validIcons[UnityEngine.Random.Range(0, validIcons.Count)];
    }

    /// <summary>
    /// Gets the number of icons available in the random pool.
    /// </summary>
    public int GetIconPoolCount()
    {
        if (randomIconPool == null) return 0;
        int count = 0;
        foreach (var sprite in randomIconPool)
        {
            if (sprite != null) count++;
        }
        return count;
    }
    
    /// <summary>
    /// Gets the index of a sprite in the icon pool. Returns -1 if not found.
    /// Used for saving icon persistence.
    /// </summary>
    public int GetIconIndex(Sprite sprite)
    {
        if (sprite == null || randomIconPool == null)
            return -1;
        
        for (int i = 0; i < randomIconPool.Count; i++)
        {
            if (randomIconPool[i] == sprite)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    /// <summary>
    /// Gets the sprite at the specified index in the icon pool. Returns null if index is invalid.
    /// Used for restoring icon persistence.
    /// </summary>
    public Sprite GetIconByIndex(int index)
    {
        if (randomIconPool == null || index < 0 || index >= randomIconPool.Count)
            return null;
        
        return randomIconPool[index];
    }

    /// <summary>
    /// Legacy: Attempts to retrieve the icon sprite for the provided warrant ID.
    /// Used for backward compatibility with existing UI widgets.
    /// </summary>
    public Sprite GetIcon(string warrantId)
    {
        if (string.IsNullOrWhiteSpace(warrantId))
            return null;

        EnsureLookup();
        return lookup != null && lookup.TryGetValue(warrantId, out var definition)
            ? definition?.icon
            : null;
    }

    private void EnsureLookup()
    {
        if (lookup != null)
            return;

        lookup = new Dictionary<string, WarrantDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in definitions)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.warrantId))
                continue;

            lookup[definition.warrantId] = definition;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        lookup = null;
    }
#endif
}


