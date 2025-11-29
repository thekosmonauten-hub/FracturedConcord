using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Maps occupied cells in an effigy shape to specific sprites.
/// </summary>
public static class EffigySpriteSet
{
    private static readonly Dictionary<string, Sprite[]> spriteLookup = new Dictionary<string, Sprite[]>();

    public static void Register(string effigyName, Sprite[] sprites)
    {
        if (string.IsNullOrWhiteSpace(effigyName) || sprites == null)
            return;

        spriteLookup[effigyName] = sprites;
    }

    public static Sprite[] GetSprites(string effigyName)
    {
        if (string.IsNullOrWhiteSpace(effigyName))
            return null;

        spriteLookup.TryGetValue(effigyName, out Sprite[] sprites);
        return sprites;
    }
}

