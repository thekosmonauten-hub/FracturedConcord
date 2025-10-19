# Card Prefab Scaling Guide

## Problem
Different scenes need different card sizes, but modifying the prefab breaks one scene or the other.

## Solution: Per-Scene Scaling

### ✅ Recommended Approach: Scale at Runtime

**Keep ONE prefab at original size, let each scene scale it dynamically.**

---

## Combat Scene (CardRuntimeManager)

**Location:** `CardRuntimeManager` component in Combat Scene

**Settings:**
```
Card Scale: (0.7, 0.7, 1)  // Adjust to fit your combat UI
```

**Where to set:**
1. Select `CardRuntimeManager` GameObject
2. Inspector → "Card Scale" field
3. Adjust X, Y, Z values
4. Cards will scale automatically when created

**Code (if needed):**
```csharp
CardRuntimeManager.Instance.cardScale = new Vector3(0.7f, 0.7f, 1f);
```

---

## DeckBuilder Scene (SimpleCombatUI)

**Location:** `SimpleCombatUI` component in DeckBuilder Scene

**Settings:**
```
Scale X: 1.0  // Horizontal scale
Scale Y: 1.0  // Vertical scale
```

**Where to set:**
1. Select `SimpleCombatUI` GameObject
2. Inspector → "Card Scaling" section
3. Adjust `Scale X` and `Scale Y` sliders
4. Enable "Auto Update Scaling" to see changes live

**Context Menu Options:**
- Right-click component → "Test Tiny Cards (0.5x)" 
- Right-click component → "Test Huge Cards (3x)"
- Right-click component → "Reset Card Size (1x)"

---

## Alternative: Prefab Variants (Advanced)

If you need **completely different** card layouts for each scene:

### Step 1: Create Variant
1. Right-click `CardPrefab.prefab`
2. Create → Prefab Variant
3. Name it `CardPrefab_Combat.prefab`
4. Modify this variant for combat

### Step 2: Assign Variants
- **Combat Scene**: Assign `CardPrefab_Combat` to `CardRuntimeManager.cardPrefab`
- **DeckBuilder Scene**: Keep original `CardPrefab` in `SimpleCombatUI.cardPrefab`

### Pros:
- Complete layout control per scene
- Can have different UI elements visible

### Cons:
- Harder to maintain (changes needed in 2+ places)
- Art updates require updating all variants

---

## Best Practices

### ✅ DO:
- Keep prefab at "base size" (e.g., 200x300)
- Use runtime scaling in each scene
- Document your scale values

### ❌ DON'T:
- Modify the prefab to fit one scene (breaks others)
- Use different scale methods in same scene (pick one!)
- Hardcode sizes (use Inspector fields instead)

---

## Common Scale Values

### Combat Scene (smaller, many cards visible)
```
Card Scale: (0.6, 0.6, 1) to (0.8, 0.8, 1)
```

### DeckBuilder Scene (larger, focus on card details)
```
Scale X: 1.0 to 1.5
Scale Y: 1.0 to 1.5
```

### Card Collection View (tiny thumbnails)
```
Card Scale: (0.3, 0.3, 1) to (0.5, 0.5, 1)
```

---

## Debugging Scale Issues

### Cards too small?
1. Check `CardRuntimeManager.cardScale` (Combat)
2. Check `SimpleCombatUI.scaleX/scaleY` (DeckBuilder)
3. Verify prefab `RectTransform.sizeDelta` is reasonable (e.g., 200x300)

### Cards overlap?
1. Check `CardRuntimeManager.cardSpacing`
2. Check `SimpleCombatUI.cardSpacing`
3. Reduce `cardScale` if needed

### Cards off-screen?
1. Check `CardHandParent` position
2. Verify `Canvas Scaler` settings
3. Adjust camera view (if using World Space)

---

## Example: Setting Up New Scene

```csharp
// In your scene's combat manager or UI controller
void Start()
{
    // Find or create CardRuntimeManager
    CardRuntimeManager cardManager = CardRuntimeManager.Instance;
    
    // Set scale for this scene
    cardManager.cardScale = new Vector3(0.7f, 0.7f, 1f);
    
    // Set spacing
    cardManager.cardSpacing = 140f;
    
    // Now create cards - they'll use these settings
    cardManager.CreateCardFromData(myCard, myCharacter);
}
```

---

## Quick Fix Checklist

Scene broken after prefab changes?

- [ ] Revert prefab to original size (200x300 or similar)
- [ ] Set `CardRuntimeManager.cardScale` in Combat Scene
- [ ] Set `SimpleCombatUI.scaleX/Y` in DeckBuilder Scene  
- [ ] Test both scenes
- [ ] Document scale values used

---

## See Also
- `CardRuntimeManager.cs` - Combat card display
- `SimpleCombatUI.cs` - DeckBuilder card display
- `CardFactory.cs` - Generic card creation

