# Card Art Integration Guide for JSON Cards

## Overview
This guide explains how to add card artwork to your JSON-based card system. Card art is loaded dynamically from the Resources folder when cards are parsed from JSON files.

---

## Quick Start

### 1. Add Artwork to Resources Folder

Create your card art folder structure:
```
Assets/
└── Resources/
    └── CardArt/
        ├── HeavyStrike.png
        ├── Brace.png
        ├── GroundSlam.png
        └── ... (all your card art files)
```

**Requirements:**
- Art must be in the `Resources` folder (or any subfolder within Resources)
- Supported formats: `.png`, `.jpg`, `.psd`, `.tga`, `.tiff`, `.gif`
- Recommended size: 512x512 pixels or 256x256 pixels
- Import settings: Set Texture Type to "Sprite (2D and UI)"

---

### 2. Update Your JSON Files

Add the `cardArtName` field to each card in your JSON:

```json
{
  "cardName": "Heavy Strike",
  "count": 6,
  "data": {
    "cardName": "Heavy Strike",
    "description": "Deal {damage} physical damage.",
    "cardType": "Attack",
    "manaCost": 1,
    "baseDamage": 8.0,
    "baseGuard": 0.0,
    "primaryDamageType": "Physical",
    "cardArtName": "CardArt/HeavyStrike",
    "weaponScaling": { ... },
    "aoe": { ... },
    ...
  }
}
```

**Path Format:**
- Relative to Resources folder
- Don't include file extension
- Use forward slashes for subfolders
- Examples:
  - `"CardArt/HeavyStrike"` → Loads from `Resources/CardArt/HeavyStrike.png`
  - `"Cards/Physical/Strike"` → Loads from `Resources/Cards/Physical/Strike.png`
  - `"MyCardArt"` → Loads from `Resources/MyCardArt.png`

---

### 3. Test in Unity

1. **Load your deck**: Cards will automatically load their art when the JSON is parsed
2. **Check console**: Look for messages:
   - `<color=green>Loaded card art: CardArt/HeavyStrike</color>` ✅ Success
   - `Failed to load card art: CardArt/HeavyStrike` ❌ Missing sprite
3. **Verify display**: Card prefabs should now show the loaded artwork

---

## Implementation Details

### Architecture Flow

```
JSON File → DeckLoader.LoadStarterDeck()
         → ConvertJSONToCard()
         → LoadCardArt()
         → Resources.Load<Sprite>()
         → Card.cardArt (set)
         → CardVisualManager.UpdateCardVisuals(Card)
         → Display on card prefab
```

### File Structure

**Modified Files:**
1. `Assets/Scripts/Cards/Card.cs` - Added `cardArt` and `cardArtName` fields
2. `Assets/Scripts/Cards/CardJSONFormat.cs` - Added `cardArtName` field
3. `Assets/Scripts/Cards/DeckLoader.cs` - Added sprite loading logic
4. `Assets/Scripts/UI/Cards/CardVisualManager.cs` - Added Card overload method

### Code Reference

**In Card.cs:**
```csharp
[Header("Visual Assets")]
public Sprite cardArt; // Loaded sprite
public string cardArtName; // Reference for loading
```

**In CardJSONFormat.cs:**
```csharp
// Visual Assets - Optional, loaded from Resources folder
// Example: "CardArt/HeavyStrike" loads from Resources/CardArt/HeavyStrike.png
public string cardArtName;
```

**In DeckLoader.cs:**
```csharp
// Load card art from Resources if specified
cardArtName = jsonCard.cardArtName,
cardArt = LoadCardArt(jsonCard.cardArtName),
```

---

## Best Practices

### Naming Convention

Use consistent naming for easy management:

```
CardArt/
├── Attack/
│   ├── HeavyStrike.png
│   ├── QuickSlash.png
│   └── GroundSlam.png
├── Guard/
│   ├── Brace.png
│   ├── Endure.png
│   └── IronWill.png
└── Skill/
    ├── IntimidatingShout.png
    └── BattleCry.png
```

**JSON References:**
```json
"cardArtName": "CardArt/Attack/HeavyStrike"
"cardArtName": "CardArt/Guard/Brace"
"cardArtName": "CardArt/Skill/IntimidatingShout"
```

### Art Specifications

**Recommended Settings:**
- **Resolution**: 512x512 pixels (power of 2)
- **Format**: PNG with transparency
- **Style**: Consistent across all cards
- **File Size**: Under 500KB per sprite

**Unity Import Settings:**
- Texture Type: `Sprite (2D and UI)`
- Sprite Mode: `Single`
- Pixels Per Unit: `100`
- Filter Mode: `Bilinear`
- Compression: `Normal Quality`

### Performance Tips

1. **Use Sprite Atlases** for production (pack multiple sprites into one texture):
   ```
   Create → 2D → Sprite Atlas
   Add your CardArt folder to the atlas
   ```

2. **Optimize Resolution**: Use 256x256 for mobile, 512x512 for PC
3. **Compress Textures**: Use appropriate compression settings
4. **Lazy Loading**: Art loads only when needed (already implemented)

---

## Troubleshooting

### Problem: Card Art Not Displaying

**Symptoms:**
- Cards show without artwork
- Console shows: `Failed to load card art: CardArt/HeavyStrike`

**Solutions:**
1. **Check File Location**:
   - File must be in a Resources folder
   - Path in JSON must match folder structure
   - Don't include file extension in JSON

2. **Check Import Settings**:
   - Open sprite in Inspector
   - Verify Texture Type is "Sprite (2D and UI)"
   - Click "Apply" if you made changes

3. **Check Naming**:
   - File name must match JSON reference exactly (case-sensitive)
   - Use forward slashes in paths, not backslashes
   - Don't use special characters in file names

### Problem: Sprite Loads But Doesn't Display

**Symptoms:**
- Console shows: `Loaded card art: CardArt/HeavyStrike`
- But card prefab still blank

**Solutions:**
1. **Check Card Prefab**:
   - Open card prefab
   - Find `CardImage` GameObject
   - Verify it has an Image component
   - Check that CardVisualManager is attached

2. **Check CardVisualManager**:
   - Ensure `cardImage` field is assigned
   - Verify the Image component is not disabled
   - Check that sprite is not being overwritten

### Problem: Wrong Art Displayed

**Symptoms:**
- Art loads but shows wrong image

**Solutions:**
1. Check JSON has correct `cardArtName` for each card
2. Verify no duplicate file names in Resources folder
3. Clear Unity cache: `Edit → Preferences → Cache Server → Clean Cache`

---

## Optional Card Art

**Card art is completely optional!** If you don't specify `cardArtName`:
- Card will display without artwork
- Card frames, text, and other elements still work
- Fallback: Shows placeholder or empty Image component

**Example without art:**
```json
{
  "cardName": "Basic Strike",
  "data": {
    "cardName": "Basic Strike",
    "description": "Deal 6 damage.",
    "cardType": "Attack",
    "manaCost": 1,
    "baseDamage": 6.0,
    ...
    // No cardArtName field - card works fine without art
  }
}
```

---

## Advanced: Dynamic Art Loading

For advanced use cases, you can modify art loading:

### Load from AssetBundles

```csharp
// In DeckLoader.cs - LoadCardArt()
private static Sprite LoadCardArt(string artName)
{
    // Try Resources first
    Sprite sprite = Resources.Load<Sprite>(artName);
    
    // Fallback to AssetBundle
    if (sprite == null)
    {
        sprite = YourAssetBundleManager.LoadSprite(artName);
    }
    
    return sprite;
}
```

### Load from Addressables

```csharp
using UnityEngine.AddressableAssets;

private static async Task<Sprite> LoadCardArtAsync(string artName)
{
    var handle = Addressables.LoadAssetAsync<Sprite>(artName);
    await handle.Task;
    return handle.Result;
}
```

---

## Migration Guide

### From ScriptableObject Cards

If you were using `CardData` ScriptableObjects before:

**Before (CardData):**
```csharp
public CardData myCard; // Had cardImage assigned in Inspector
```

**After (JSON + Card):**
```json
{
  "cardName": "My Card",
  "data": {
    "cardArtName": "CardArt/MyCard", // References sprite file
    ...
  }
}
```

**Compatibility:**
- ScriptableObject cards (`CardData`) still work as before
- JSON cards (`Card`) now support art via Resources folder
- Both systems can coexist

---

## Example: Complete Card with Art

```json
{
  "cardName": "Heavy Strike",
  "count": 6,
  "data": {
    "cardName": "Heavy Strike",
    "description": "Deal {damage} physical damage. Scales with melee weapon and Strength.",
    "cardType": "Attack",
    "manaCost": 1,
    "baseDamage": 8.0,
    "baseGuard": 0.0,
    "primaryDamageType": "Physical",
    "cardArtName": "CardArt/HeavyStrike",
    "weaponScaling": {
      "scalesWithMeleeWeapon": true,
      "scalesWithProjectileWeapon": false,
      "scalesWithSpellWeapon": false
    },
    "aoe": {
      "isAoE": false,
      "aoeTargets": 1
    },
    "requirements": {
      "requiredStrength": 25,
      "requiredDexterity": 0,
      "requiredIntelligence": 0,
      "requiredLevel": 1,
      "requiredWeaponTypes": []
    },
    "tags": ["Attack", "Physical", "Combo"],
    "additionalDamageTypes": [],
    "damageScaling": {
      "strengthScaling": 0.5,
      "dexterityScaling": 0.0,
      "intelligenceScaling": 0.0
    },
    "guardScaling": {
      "strengthScaling": 0.0,
      "dexterityScaling": 0.0,
      "intelligenceScaling": 0.0
    },
    "effects": [],
    "rarity": "Common",
    "element": "Physical",
    "category": "Attack"
  }
}
```

---

## Conclusion

You now have a complete card art system integrated with your JSON cards! The system:
- ✅ Loads sprites dynamically from Resources
- ✅ Supports optional art (cards work without it)
- ✅ Gracefully handles missing sprites
- ✅ Works alongside existing CardData system
- ✅ Easy to use - just add `cardArtName` to JSON

**Next Steps:**
1. Create your card art assets
2. Place them in `Resources/CardArt/`
3. Update your JSON files with `cardArtName` fields
4. Test in Unity!

For questions or issues, refer to the Troubleshooting section above.




