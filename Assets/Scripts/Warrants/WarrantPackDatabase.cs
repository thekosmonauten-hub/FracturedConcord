using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Database of warrant packs (starter packs, reward packs, etc.)
/// </summary>
[CreateAssetMenu(fileName = "WarrantPackDatabase", menuName = "Dexiled/Warrants/Pack Database", order = 2)]
public class WarrantPackDatabase : ScriptableObject
{
    [Header("Warrant Pack Database")]
    [Tooltip("List of all warrant packs available in the game")]
    [SerializeField] private List<WarrantPack> packs = new List<WarrantPack>();
    
    [Header("Warrant System Reference")]
    [Tooltip("Reference to WarrantDatabase for rolling warrants. Can be assigned in Inspector or will use default when rolling.")]
    [SerializeField] private WarrantDatabase warrantDatabase;
    
    private Dictionary<string, WarrantPack> lookup;
    
    /// <summary>
    /// Get the WarrantDatabase reference (for rolling warrants from packs)
    /// </summary>
    public WarrantDatabase GetWarrantDatabase()
    {
        return warrantDatabase;
    }
    
    public IReadOnlyList<WarrantPack> Packs => packs;
    
    /// <summary>
    /// Get a warrant pack by its ID
    /// </summary>
    public WarrantPack GetPackById(string packId)
    {
        if (string.IsNullOrEmpty(packId))
            return null;
        
        EnsureLookup();
        lookup.TryGetValue(packId, out var pack);
        return pack;
    }
    
    /// <summary>
    /// Check if a pack with the given ID exists
    /// </summary>
    public bool Contains(string packId)
    {
        if (string.IsNullOrEmpty(packId))
            return false;
        
        EnsureLookup();
        return lookup.ContainsKey(packId);
    }
    
    /// <summary>
    /// Get all packs
    /// </summary>
    public IEnumerable<WarrantPack> GetAll()
    {
        return packs?.Where(p => p != null) ?? Enumerable.Empty<WarrantPack>();
    }
    
    private void EnsureLookup()
    {
        if (lookup != null)
            return;
        
        lookup = new Dictionary<string, WarrantPack>();
        if (packs == null)
            return;
        
        foreach (var pack in packs)
        {
            if (pack == null || string.IsNullOrEmpty(pack.packId))
                continue;
            
            lookup[pack.packId] = pack;
        }
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        lookup = null;
    }
#endif
}

