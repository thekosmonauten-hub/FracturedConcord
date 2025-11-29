using UnityEngine;

/// <summary>
/// Helper to register a sprite set for an effigy via the Inspector.
/// </summary>
public class EffigySpriteSetInitializer : MonoBehaviour
{
    [System.Serializable]
    private class EffigySpriteEntry
    {
        public string effigyName;
        public Sprite[] sprites;
    }

    [SerializeField] private EffigySpriteEntry[] entries;

    void Awake()
    {
        if (entries == null)
            return;

        foreach (var entry in entries)
        {
            if (entry == null || entry.sprites == null || entry.sprites.Length == 0 || string.IsNullOrWhiteSpace(entry.effigyName))
                continue;

            EffigySpriteSet.Register(entry.effigyName, entry.sprites);
        }
    }
}

