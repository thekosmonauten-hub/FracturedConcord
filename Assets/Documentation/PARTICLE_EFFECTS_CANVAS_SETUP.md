# Particle Effects Canvas - Complete Setup

## Required Components on ParticleEffectsCanvas

Your ParticleEffectsCanvas GameObject needs these 2 components:

### 1. Canvas Component ✅
You already have this! Configure:
- **Render Mode**: **Screen Space - Overlay** (recommended)
- **Pixel Perfect**: ❌ Unchecked (optional)
- **Sort Order**: **10** (must be HIGHER than your main canvas, which is usually 0)
- **Target Display**: Display 1
- **Additional Shader Channels**: Nothing selected (particles don't need extra channels)

**Note:** "Override Sorting" only appears for "Screen Space - Camera" or "World Space" render modes. For "Screen Space - Overlay", Sort Order alone controls rendering order.

### 2. Canvas Scaler Component (Recommended)
Add this for consistent particle sizing across resolutions:

1. Click **Add Component** on ParticleEffectsCanvas
2. Search for **Canvas Scaler**
3. Configure:
   - **UI Scale Mode**: **Scale With Screen Size**
   - **Reference Resolution**: **1920 x 1080** (match your main canvas)
   - **Screen Match Mode**: Match Width Or Height
   - **Match**: **0** (or 0.5 for balanced)

---

## NOT Required (But Sometimes Added)

### ❌ GraphicRaycaster - NOT needed
Particles aren't clickable, so you don't need this component. It would just waste performance.

---

## Complete Setup Checklist

**ParticleEffectsCanvas GameObject:**
- [ ] **Canvas** component
  - [ ] Render Mode: Screen Space - Overlay
  - [ ] Sort Order: **10** (or higher than main canvas)
- [ ] **CanvasScaler** component
  - [ ] UI Scale Mode: Scale With Screen Size
  - [ ] Reference Resolution: 1920 x 1080

**Each Particle System (XDeckGlow):**
- [ ] GameObject is **ACTIVE** ✅
- [ ] Particle System → **Play On Awake**: ✅ Checked
- [ ] **RectTransform** component exists
- [ ] RectTransform → Scale: (1, 1, 1) NOT zero
- [ ] ParticleSystemRenderer → **Enabled**: ✅

**Children (Particle Systems):**
- [ ] MarauderDeckGlow
- [ ] WitchDeckGlow
- [ ] RangerDeckGlow
- [ ] ThiefDeckGlow
- [ ] ApostleDeckGlow
- [ ] BrawlerDeckGlow

---

## Hierarchy Structure

Your hierarchy should look like this:

```
CharacterCreationCanvas (Sort Order: 0)
├─ Background
├─ ContentPanel
│   └─ ClassGrid
│       ├─ MarauderButton
│       ├─ WitchButton
│       ├─ etc...
└─ ...

ParticleEffectsCanvas (Sort Order: 10) ← Separate Canvas!
├─ MarauderDeckGlow
├─ WitchDeckGlow
├─ RangerDeckGlow
├─ ThiefDeckGlow
├─ ApostleDeckGlow
└─ BrawlerDeckGlow
```

**Important:** ParticleEffectsCanvas should be a **sibling** of your main canvas (at the same hierarchy level), NOT a child!

---

## Positioning Particles to Match Buttons

Since particles are now in a separate Canvas, you need to position them manually or with a script.

### Option 1: Manual Positioning
1. Enter Play Mode (or just in Scene view)
2. For each particle system, adjust **RectTransform** position to align with its button
3. Use **Anchors** to maintain position across screen sizes

### Option 2: Auto-Position Script (Recommended)

Add this component to each particle system:

```csharp
using UnityEngine;

/// <summary>
/// Positions a UI element (like particles) to follow another UI element (like a button).
/// Useful when they're in different canvases.
/// </summary>
public class UIFollowTarget : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private bool followInUpdate = false;
    
    private RectTransform rectTransform;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        UpdatePosition();
    }
    
    void Update()
    {
        if (followInUpdate)
        {
            UpdatePosition();
        }
    }
    
    void UpdatePosition()
    {
        if (target == null || rectTransform == null) return;
        
        // Get target's screen position
        Vector3 targetScreenPos = RectTransformUtility.WorldToScreenPoint(null, target.position);
        
        // Convert to local position in this canvas
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            targetScreenPos,
            null,
            out localPos
        );
        
        rectTransform.anchoredPosition = localPos + offset;
    }
}
```

**Usage:**
1. Add `UIFollowTarget` script to each XDeckGlow GameObject
2. Assign **Target** (drag the corresponding button)
3. Particles will auto-position to button location!

---

## Testing

After setup, verify:

- [ ] Particles appear in Scene view
- [ ] Particles appear in Game view
- [ ] Particles render **in front** of buttons/UI
- [ ] Particles are positioned correctly over their buttons
- [ ] No console errors

**Test visibility:**
1. Enter Play Mode
2. Particles should be visible above the class selection buttons
3. If still behind, increase ParticleEffectsCanvas Sort Order to 20 or higher

---

## 🔧 TROUBLESHOOTING: Particles Not Visible (Even with High Sort Order)

If particles don't appear even with Sort Order 10, 100, or 1000, use the **ParticleCanvasDebugger** script to diagnose:

1. **Add `ParticleCanvasDebugger` component to ParticleEffectsCanvas**
2. **Check the Console** - it will show detailed diagnostics
3. **Right-click the component → "Run Full Diagnostic"** for a complete report

### Common Causes & Fixes:

#### ❌ Canvas Sort Order Too Low
**Symptom:** Particles invisible or behind UI.

**Fix:** 
- ParticleEffectsCanvas → Canvas → **Sort Order**: **10** (or higher than main canvas)
- Check main canvas Sort Order and ensure ParticleEffectsCanvas is higher

**Note:** "Override Sorting" only exists for Camera/World Space render modes, not Screen Space - Overlay.

#### ❌ Particle Systems Not Playing
**Symptom:** Particles exist but nothing appears.

**Fix:**
- For each XDeckGlow → Particle System → **Play On Awake**: ✅ **CHECKED**
- OR call `particleSystem.Play()` in code

#### ❌ Particle Systems Disabled or Inactive
**Symptom:** No particles visible.

**Fix:**
- Check each XDeckGlow GameObject is **ACTIVE** in hierarchy
- Check Particle System component is **ENABLED** (checkbox checked)
- Check ParticleSystemRenderer component is **ENABLED**

#### ❌ Missing RectTransform on Particle Systems
**Symptom:** Particles positioned incorrectly or invisible.

**Fix:**
- Each XDeckGlow MUST have a **RectTransform** component
- Add RectTransform: Right-click XDeckGlow → Add Component → RectTransform

#### ❌ Zero Scale or Off-Screen Position
**Symptom:** Particles exist but not visible.

**Fix:**
- Check RectTransform → **Scale**: Should be (1, 1, 1), NOT (0, 0, 0)
- Check RectTransform → **Position**: Should be on-screen (check in Scene view)
- Verify particles are positioned over their buttons

#### ❌ Wrong Canvas Render Mode
**Symptom:** Particles render incorrectly or not at all.

**Fix:**
- ParticleEffectsCanvas → Canvas → **Render Mode**: **Screen Space - Overlay**
- Main canvas should also be **Screen Space - Overlay**

#### ❌ Particle System Render Mode Issue
**Symptom:** Particles render but look wrong.

**Fix:**
- XDeckGlow → Particle System Renderer → **Render Mode**: **Billboard** (default is fine)
- Avoid **Mesh** render mode for UI particles

#### ❌ Particles Using World Space
**Symptom:** Particles appear in wrong location or scale.

**Fix:**
- Ensure all XDeckGlow GameObjects are **children** of ParticleEffectsCanvas
- Ensure ParticleEffectsCanvas uses **Screen Space - Overlay**

---

## Common Issues

### Particles still behind UI?
**Fix 1:** Increase Canvas Sort Order
- ParticleEffectsCanvas → Canvas → Sort Order: **20** (or 100)
- Ensure it's higher than your main canvas Sort Order

**Fix 2:** Check if particles are actually playing
- Use the ParticleCanvasDebugger script (provided above)
- Manually verify: XDeckGlow → Particle System → Play On Awake: ✅ Checked

### Particles visible but in wrong position?
**Fix:** Use the UIFollowTarget script (provided above)

### Particles don't scale properly on different resolutions?
**Fix:** Ensure ParticleEffectsCanvas has CanvasScaler with same settings as main canvas

### Particles flicker or disappear?
**Fix:** Both canvases should use same Render Mode (both Screen Space - Overlay)

---

## Alternative: Keep Particles as Children

If you prefer particles as children of buttons, you can make it work:

1. **For each particle system:**
   - Renderer → Order in Layer: **100** (very high)
   - Renderer → Sorting Layer: Create a new layer called "UIParticles"

2. **In Project Settings:**
   - Edit → Project Settings → Tags and Layers
   - Add new Sorting Layer: "UIParticles"
   - Ensure it's ABOVE the default layer

This is more complex and can have issues, so **separate Canvas is recommended**.

---

## ⚠️ CRITICAL: Particle Sorting Layer Conflict

**If particles appear BEHIND UI even with high Canvas Sort Order:**

The issue is that **Particle System Renderers** can use custom **Sorting Layers** (like "VFX") which **OVERRIDE Canvas Sort Order**.

### The Problem:
- Your particles use **Sorting Layer: VFX** with **Order in Layer: 1000**
- This makes them render in the "VFX" sorting layer system
- Canvas Sort Order is IGNORED because particles are using a different rendering system

### The Solution:

**Option A: Use Canvas Rendering (Easiest)**

For each particle system (WitchDeckGlow, etc.):
1. Select particle GameObject
2. **Particle System Renderer** component:
   - **Sorting Layer**: **Default** (NOT "VFX")
   - **Order in Layer**: **0**
3. Particles now use Canvas Sort Order (100) for rendering ✅

**Option B: Fix VFX Sorting Layer Order**

1. **Edit → Project Settings → Tags and Layers**
2. **Sorting Layers** section
3. Ensure **VFX** is **BELOW UI** in the list (bottom renders on top):
   ```
   Default
   UI          ← Above
   VFX         ← Below = on top
   ```

**Option C: Use the Fix Script**

1. Add `FixParticleSorting.cs` component to ParticleEffectsCanvas
2. In Inspector, ensure **Reset To Canvas Rendering** is ✅ checked
3. Right-click component → **"Fix All Particle Sorting"**
4. Remove the component after fixing

---

## Summary

**Minimum Setup:**
1. ✅ ParticleEffectsCanvas has **Canvas** component (Sort Order: 10 or higher)
2. ✅ ParticleEffectsCanvas has **CanvasScaler** component
3. ✅ All XDeckGlow GameObjects are children of ParticleEffectsCanvas
4. ✅ Position particles to align with buttons (manually or with script)
5. ✅ **CRITICAL:** All particle systems use **Sorting Layer: Default** and **Order in Layer: 0** (no custom sorting layers!)

**That's all you need!** The particles will render on top of your UI. 🎆

