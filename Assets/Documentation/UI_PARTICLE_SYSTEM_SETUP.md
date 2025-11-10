# UI Particle System Setup - Rendering Above UI Elements

## Problem
Particle Systems appear behind UI elements even though they're in the Canvas hierarchy.

## Solution: Configure Particle System Renderer

### Quick Fix (2 minutes)

For each particle system (MarauderDeckGlow, WitchDeckGlow, etc.):

1. Select the particle system GameObject in Hierarchy
2. Find **Particle System** component → Expand **Renderer** section
3. Set these properties:

**Renderer Settings:**
- **Render Mode**: **Billboard** (default is fine)
- **Render Alignment**: **View**
- **Sorting Layer**: **Default** (or your UI sorting layer)
- **Order in Layer**: **1** or higher (increase until particles show above UI)

**Material:**
- Must be a **UI-compatible material** (uses UI shader)
- Default particle material often doesn't work with UI

---

## Method 1: Use Canvas Sorting Order (Recommended)

### Step 1: Create Separate Canvas for Particles

Instead of putting particles directly under buttons, use a dedicated Canvas:

```
CharacterCreationCanvas
├─ ContentPanel
│   └─ ClassGrid
│       └─ MarauderButton
└─ ParticleEffectsCanvas  ← NEW Canvas, higher sorting order
    ├─ MarauderDeckGlow
    ├─ WitchDeckGlow
    ├─ RangerDeckGlow
    ├─ ThiefDeckGlow
    ├─ ApostleDeckGlow
    └─ BrawlerDeckGlow
```

### Step 2: Setup ParticleEffectsCanvas

1. Right-click **CharacterCreationCanvas** → **Create Empty**
2. Rename to **ParticleEffectsCanvas**
3. Add **Canvas** component:
   - Render Mode: **Screen Space - Overlay**
   - Sort Order: **10** (higher than main canvas)
4. Add **CanvasScaler** component (same settings as main canvas)
5. **IMPORTANT**: Move it to be a **sibling** of your main canvas (not a child!)

### Step 3: Move Particle Systems

1. Drag all your XDeckGlow GameObjects under **ParticleEffectsCanvas**
2. Position them to align with their respective buttons

### Step 4: Position Particles to Match Buttons

For each particle system, you'll need to position it to match its button:

```csharp
// You can use a script to auto-position particles to buttons
// See "Auto-Positioning Script" below
```

---

## Method 2: Use World Space Particles (Alternative)

### Step 1: Change Canvas Render Mode

If your main canvas is **Screen Space - Overlay**, particles won't render in front easily.

**Solution A: Switch to Screen Space - Camera**
1. Select **CharacterCreationCanvas**
2. **Render Mode**: Screen Space - Camera
3. **Render Camera**: Drag your Main Camera
4. **Plane Distance**: 10

Particles now work with Z-depth positioning!

**Solution B: Keep Overlay, Use Particle Sorting**
Keep your canvas as Overlay, but use sorting layers (Method 1 is better).

---

## Method 3: Auto-Position Particles to Buttons (Helper Script)

Create this script to automatically position particles above their buttons:

```csharp
using UnityEngine;

/// <summary>
/// Positions a particle system to follow a UI button.
/// Useful for button glow effects.
/// </summary>
public class UIParticlePositioner : MonoBehaviour
{
    [SerializeField] private RectTransform targetButton;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private bool updateEveryFrame = false;
    
    private RectTransform rectTransform;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        PositionToButton();
    }
    
    void Update()
    {
        if (updateEveryFrame)
        {
            PositionToButton();
        }
    }
    
    void PositionToButton()
    {
        if (targetButton == null || rectTransform == null) return;
        
        // Match position
        rectTransform.position = targetButton.position;
        
        // Apply offset
        rectTransform.anchoredPosition += offset;
        
        // Optional: Match size
        rectTransform.sizeDelta = targetButton.sizeDelta;
    }
}
```

**Usage:**
1. Add this script to each XDeckGlow GameObject
2. Assign **Target Button** (drag the MarauderButton, WitchButton, etc.)
3. Particles will auto-position over the button!

---

## Method 4: Keep Particles as Children (Simplest if it works)

If you want particles to stay as children of buttons:

### For Each XDeckGlow GameObject:

1. Select the particle system (e.g., MarauderDeckGlow)
2. **Particle System** component:
   - Renderer → **Render Mode**: Billboard
   - Renderer → **Sorting Layer**: Default
   - Renderer → **Order in Layer**: **2** or higher
3. **RectTransform** (if it has one):
   - Position: (0, 0, 0) - centered on parent
   - Anchors: Center
4. **Material**: 
   - Use a UI-compatible particle material
   - Try: Default-Particle (built-in) or create one

---

## Recommended Setup (Cleanest)

Use **Method 1** (Separate Canvas):

```
Main Canvas (Sort Order: 0)
  └─ All your UI elements

ParticleEffectsCanvas (Sort Order: 10)
  └─ All particle systems
```

**Advantages:**
- ✅ Particles always render on top
- ✅ Easy to toggle all particles on/off
- ✅ Better organization
- ✅ No z-fighting issues

---

## Quick Debug

### Check Current Setup:
1. Select one of your XDeckGlow GameObjects
2. Look at **Particle System Renderer** component
3. Check **Order in Layer** value

**If particles are behind:**
- Increase **Order in Layer** to 5, 10, or higher

**If particles are still behind:**
- Use Method 1 (separate Canvas with higher sort order)

---

## Common Issues

### Particles invisible?
- **Check**: Material is assigned
- **Check**: Start Color alpha is > 0
- **Check**: Size is large enough to see

### Particles flicker?
- **Fix**: Use Screen Space - Camera instead of Overlay
- **Or**: Ensure all particles use same Order in Layer

### Particles too bright/dark?
- **Adjust**: Particle System → Renderer → Material
- **Or**: Particle System → Start Color

---

## Summary

**Simplest solution:**
1. Select each XDeckGlow
2. Particle System → Renderer → **Order in Layer**: **5**
3. Done!

If that doesn't work, use separate Canvas with higher Sort Order (Method 1).

Let me know if you need help with the positioning! 🎆


