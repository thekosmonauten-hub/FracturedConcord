# Centering the Effigy Grid
## Quick Guide for EffigyGridUI Positioning

**Component:** `EffigyGridUI.cs`  
**Grid Size:** 6x4 cells (370px x 246px)

---

## 🎯 Method 1: Manual Centering (Recommended)

### Step-by-Step in Unity Inspector

1. **Select** the GameObject with `EffigyGridUI.cs` attached
   - This should be your `EffigyGridContainer` GameObject

2. **Set RectTransform Anchors to Center:**
   - Click the **Anchor Preset** box (top-left of RectTransform)
   - Hold **Alt + Shift** and click **Center** preset
   - This sets both anchors AND pivot to center (0.5, 0.5)

   **OR manually set:**
   - Anchors Min: `(0.5, 0.5)`
   - Anchors Max: `(0.5, 0.5)`
   - Pivot: `(0.5, 0.5)`

3. **Set Position to Zero:**
   - Pos X: `0`
   - Pos Y: `0`
   - Pos Z: `0`

4. **The grid will auto-size itself!**
   - EffigyGridUI sets its own width/height based on cells
   - Width: 370px (6 cells × 60px + 5 spaces × 2px)
   - Height: 246px (4 cells × 60px + 3 spaces × 2px)

### Visual Result
```
┌─────────────────────────────┐
│      Parent Container       │
│                             │
│         ┌─────┐             │
│         │Grid │             │  ← Grid perfectly centered
│         └─────┘             │
│                             │
└─────────────────────────────┘
```

---

## 🎨 Method 2: Using Layout Groups (Alternative)

If you want more layout control or multiple centered elements:

### Setup

1. **Select Parent Container** (e.g., EffigyGridContainer's parent)

2. **Add Layout Group:**
   - **Vertical Layout Group** (if grid is only element)
   - OR **Horizontal Layout Group** (if multiple elements in a row)

3. **Configure Layout Group:**
   - Child Alignment: `Middle Center`
   - Child Force Expand: Width ☐, Height ☐
   - Padding: Adjust to taste

4. **Add Content Size Fitter to Grid:**
   - Select GameObject with EffigyGridUI
   - Add Component → `Content Size Fitter`
   - Horizontal Fit: `Preferred Size`
   - Vertical Fit: `Preferred Size`

### When to Use This
- ✅ Multiple UI elements need centering
- ✅ Want responsive padding/spacing
- ✅ Building a larger layout system

---

## 🛠️ Method 3: Using Anchors for Specific Positioning

For precise control over offset from center:

### Example: Center but Slightly Offset

```
RectTransform Settings:
  Anchors Min: (0.5, 0.5)
  Anchors Max: (0.5, 0.5)
  Pivot: (0.5, 0.5)
  
  Pos X: -50  (50 pixels left of center)
  Pos Y: 20   (20 pixels up from center)
  Pos Z: 0
  
  Width: 370  (auto-set by script)
  Height: 246 (auto-set by script)
```

---

## 📏 Understanding the Grid Size

The EffigyGridUI creates:
- **6 columns × 4 rows** = 24 cells
- **Cell size:** 60px × 60px
- **Spacing:** 2px between cells

**Total dimensions:**
```
Width  = (6 cells × 60px) + (5 gaps × 2px) = 360 + 10 = 370px
Height = (4 cells × 60px) + (3 gaps × 2px) = 240 + 6  = 246px
```

These dimensions are automatically set by `EffigyGridUI.cs` in the `CreateGrid()` method.

---

## 🎯 Quick Reference: Anchor Presets

Click the **Anchor Preset** box in RectTransform Inspector:

| Preset | Anchors | Result |
|--------|---------|--------|
| **Center** (Alt+Shift+Click) | Min: (0.5, 0.5), Max: (0.5, 0.5) | Centered, fixed size |
| **Top Center** | Min: (0.5, 1), Max: (0.5, 1) | Top edge, centered horizontally |
| **Middle Left** | Min: (0, 0.5), Max: (0, 0.5) | Left edge, centered vertically |
| **Stretch** | Min: (0, 0), Max: (1, 1) | Fills parent (not recommended for grid) |

**Pro Tip:** Hold **Alt** to set pivot, **Shift** to set position simultaneously!

---

## 🚨 Common Issues

### ❌ Grid is off-center
**Problem:** Pivot is not set to (0.5, 0.5)  
**Solution:** Set Pivot to `(0.5, 0.5)` in RectTransform

### ❌ Grid is cut off or overlapping other elements
**Problem:** Parent container is too small or grid is in wrong layer  
**Solution:** 
- Check parent container's size
- Ensure grid isn't overlapping other UI elements
- Check sorting order if using multiple Canvases

### ❌ Grid doesn't appear
**Problem:** Grid is behind other UI elements  
**Solution:**
- Check hierarchy order (elements lower in hierarchy render on top)
- Or adjust Canvas sorting if using multiple Canvases

### ❌ Grid size is wrong
**Problem:** Manually set Width/Height instead of letting script set it  
**Solution:**
- Let `EffigyGridUI.cs` set the size automatically
- Don't use `Content Size Fitter` on the grid GameObject itself
- The script calculates and sets the size in `CreateGrid()`

---

## 💡 Recommended Setup for Your Case

Based on your setup with Display Window and Dynamic Area:

```
EffigyGridContainer (in Display Window or Dynamic Area)
├── RectTransform Settings:
│   ├── Anchors: Center (0.5, 0.5) to (0.5, 0.5)
│   ├── Pivot: (0.5, 0.5)
│   ├── Position: (0, 0, 0)
│   └── Size: Auto-set by EffigyGridUI (370 x 246)
├── EffigyGridUI.cs (attached)
└── Children: (created dynamically by script)
    ├── EffigyGrid
    │   └── Row containers and cells (auto-generated)
    └── (24 cells total)
```

---

## 🎬 Visual Setup Steps

1. **Select `EffigyGridContainer`** in Hierarchy
2. **In Inspector → RectTransform:**
   - Click Anchor Preset box (top-left square icon)
   - **Hold Alt + Shift**
   - Click **center** preset (middle of the 3×3 grid)
3. **Set Position:**
   - Pos X = `0`
   - Pos Y = `0`
4. **Done!** Grid is centered ✓

---

## 📖 Alternative: Using Code

If you want to center it via code (not recommended, but possible):

```csharp
// In EffigyGridUI.cs, after gridContainer is created:

RectTransform containerRect = gridContainer.GetComponent<RectTransform>();
containerRect.anchorMin = new Vector2(0.5f, 0.5f);
containerRect.anchorMax = new Vector2(0.5f, 0.5f);
containerRect.pivot = new Vector2(0.5f, 0.5f);
containerRect.anchoredPosition = Vector2.zero;
```

But it's much easier to just set this in the Inspector once!

---

## ✅ Verification

After setup, check:
- [ ] Grid appears in center of parent container
- [ ] All 24 cells (6×4) are visible
- [ ] Grid doesn't overlap other UI elements
- [ ] Grid stays centered when resizing Game view

**Perfect!** Your Effigy Grid is now centered! 🎯

---

**Need more help?** Check Unity's RectTransform documentation:  
https://docs.unity3d.com/Manual/class-RectTransform.html

