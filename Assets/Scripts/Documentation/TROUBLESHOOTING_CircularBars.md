# Troubleshooting: Circular Bars Appear as Squares

## 🐛 Problem: "I added CircularHealthBar but it's showing a square!"

This is a **very common issue** - here's why and how to fix it!

---

## ❓ Why It Happens

The `CircularHealthBar` script configures the **fill settings** (Radial 360, Top, etc.), BUT it still needs a **circular sprite** to actually look circular!

```
What you have:           What Unity shows:
┌─────────┐             ┌─────────┐
│ Square  │  +  Radial  │ Square  │  = Still looks square!
│ Sprite  │     Fill    │ Filled  │
└─────────┘             └─────────┘

What you need:           What Unity shows:
   ╭───╮                    ╭───╮
  ╱     ╲    +  Radial     ╱  ◑  ╲   = Circular!
 │ Circle │      Fill     │ Fill │
  ╲     ╱                  ╲     ╱
   ╰───╯                    ╰───╯
```

**TL;DR: You need a circular sprite image!**

---

## ✅ **Solution 1: Use Unity's Built-in Circle** (30 seconds!)

**Fastest fix:**

1. **Select your health bar Image** GameObject
2. **In Inspector**, find the **Image component**
3. **Click the circle** next to "Source Image" (currently shows "None" or a square)
4. **In the popup**, search for: `Knob`
5. **Select**: "Knob" sprite (it's a perfect circle!)
6. **Done!** Should be circular now!

**Step-by-step with Inspector:**
```
Image Component:
┌─────────────────────────────────┐
│ Source Image: [○] None (Sprite) │ ← Click this circle!
│                      ↓           │
│ Search: "knob"       ↓           │
│ Results:             ↓           │
│   → Knob ✓          ↓          │ ← Select this!
└─────────────────────────────────┘
```

**Alternative built-in sprites to try:**
- `Knob` - Perfect filled circle ⭐ Best!
- `UISprite` - Also circular
- `Background` - Soft circle
- Look in: `Assets/Unity Default Resources/`

---

## ✅ **Solution 2: Generate Perfect Circle** (1 click!)

**Use my sprite generator tool:**

1. **Unity Menu**: `Tools > Combat UI > Generate Circle Sprite`
2. **Configure**:
   - Texture Size: 512
   - Circle Color: White
   - Anti-Aliasing: ✓
3. **Click**: "Generate Circle Sprite"
4. **Sprite is created and auto-selected!**
5. **Drag to Image**: Drag the sprite to your Image's "Source Image" field
6. **Done!** Perfect circle!

**What it creates:**
- High-quality PNG (512x512)
- Smooth anti-aliased edges
- Transparent background
- Ready to use

---

## ✅ **Solution 3: Download/Create Your Own**

### Option A: Create in Image Editor

**In Photoshop/GIMP/Paint.NET:**
1. New canvas 512x512
2. Transparent background
3. Select ellipse tool
4. Hold Shift (for perfect circle)
5. Fill with white
6. Export as PNG
7. Import to Unity

### Option B: Download Free Circle

**Unity Asset Store:**
- Search: "circle sprite"
- Download free UI pack
- Import circle sprites

### Option C: Use Online Generator
- Website: `pixlr.com` or `photopea.com`
- Create circle shape
- Export PNG
- Import to Unity

---

## 🔍 Verify Your Setup

### Check These Settings

**1. Image Component:**
```
Image:
├── Source Image: [Circle sprite] ← MUST be circular!
├── Image Type: Filled
├── Fill Method: Radial 360
├── Fill Origin: Top (2)
├── Fill Amount: 1
└── Clockwise: ✓
```

**2. Sprite Import Settings:**
```
Select your circle sprite in Project window
Inspector:
├── Texture Type: Sprite (2D and UI)
├── Sprite Mode: Single
├── Pixels Per Unit: 100
└── Filter Mode: Bilinear
```

**3. RectTransform:**
```
Make sure it's square:
Width: 100
Height: 100  ← Same as width!
```

---

## 🎯 Complete Setup Checklist

Follow this exactly:

### Step 1: Get Circle Sprite
- [ ] **Option A**: Use Unity's built-in `Knob` sprite
- [ ] **Option B**: Generate using my tool (`Tools > Combat UI > Generate Circle Sprite`)
- [ ] **Option C**: Create/download your own

### Step 2: Assign Sprite
- [ ] Select your health bar Image GameObject
- [ ] Image component → Source Image → Drag circle sprite
- [ ] Verify it looks circular in Scene view

### Step 3: Configure Image Settings
- [ ] Image Type: **Filled**
- [ ] Fill Method: **Radial 360**
- [ ] Fill Origin: **Top**
- [ ] Fill Amount: **1.0**
- [ ] Clockwise: **✓ Checked**

### Step 4: Verify CircularHealthBar Component
- [ ] CircularHealthBar component is attached
- [ ] Fill Image is assigned (or auto-found)
- [ ] Bar Type: Health or Mana
- [ ] Fill Origin: Top

### Step 5: Test
- [ ] Right-click component → "Set to Half"
- [ ] Should show half-circle from top
- [ ] Try "Set to Low" - should pulse

---

## 🖼️ Visual Debugging

### What You See vs What's Wrong

**Seeing this?**
```
┌─────┐
│  ░  │  ← Square with fill
└─────┘
```
**Problem:** Square sprite
**Fix:** Change Source Image to circle sprite

**Seeing this?**
```
  ░░░
 ░   ░
  ░░░   ← Pixelated circle
```
**Problem:** Low-res sprite or no anti-aliasing
**Fix:** Use 512x512 sprite with AA

**Seeing this?**
```
  ╭─╮
 │░░│  ← Stretched/oval
  ╰─╯
```
**Problem:** RectTransform not square
**Fix:** Make Width = Height (e.g., 100x100)

---

## 🔧 Inspector Walkthrough

**Looking at your screenshot, here's what to do:**

1. **Select the left health bar** (the one showing "100/100")

2. **Find Image component** in Inspector

3. **Check Source Image field**:
   ```
   Image
   ├── Source Image: [?]  ← Is this a square or circle?
   ```

4. **If it's square or None:**
   - Click the circle button next to Source Image
   - Search "knob"
   - Select the Knob sprite
   - Should instantly become circular!

5. **If that doesn't work:**
   - Use my sprite generator (see Solution 2 above)
   - Or import a circular PNG image

---

## 🎨 Quick Test

**To verify it's working:**

1. **Select your health bar Image**
2. **In Inspector, Image component**:
   - Set Fill Amount to `0.5` (manually)
   - You should see **half a circle** from top
   - If you see **half a square**, sprite is still square!

**Visual test:**
```
Fill Amount: 1.0    Fill Amount: 0.5    Fill Amount: 0.25
      ●                  ◑                  ◕
   (full)            (half)            (quarter)

If you see: ■  or  ◧  or  ▀  ← Wrong! Still using square sprite
```

---

## 💡 Pro Tip: Verify Sprite Shape

**Check your sprite is actually circular:**

1. **Find sprite in Project window**
2. **Look at thumbnail** - is it circular?
3. **Click sprite** - Inspector shows preview
4. **If square** → You need a circular sprite!

---

## 🚀 Recommended Workflow

**For best results:**

1. **Generate circle sprite first:**
   ```
   Tools > Combat UI > Generate Circle Sprite
   → Creates WhiteCircle.png
   ```

2. **Assign to Image:**
   ```
   Drag WhiteCircle sprite to Source Image field
   ```

3. **Verify settings:**
   ```
   Image Type: Filled
   Fill Method: Radial 360
   Fill Origin: Top
   ```

4. **Test:**
   ```
   Right-click CircularHealthBar → Set to Half
   Should show half-circle from top!
   ```

---

## 📸 What It Should Look Like

**In Unity Editor:**
```
Scene View:
   ╭───╮
  ╱     ╲
 │   ◑   │  ← Circular, smooth edges
  ╲     ╱
   ╰───╯

Inspector (Image):
Source Image: WhiteCircle ✓
Image Type: Filled ✓
Fill Method: Radial 360 ✓
Fill Origin: Top ✓
```

**In Game:**
```
With health at 75%:
   ╭───╮
  ╱ ◔   ╲
 │       │  ← Filled from top, clockwise to 75%
  ╲     ╱
   ╰───╯
```

---

## ✅ Final Checklist

If still showing square, verify:

- [ ] Source Image is assigned (not None)
- [ ] Source Image is actually circular (check thumbnail)
- [ ] Image Type is set to **Filled** (not Simple/Sliced)
- [ ] Fill Method is **Radial 360** (not Horizontal/Vertical)
- [ ] RectTransform is square (width = height)
- [ ] Sprite import type is **Sprite (2D and UI)**
- [ ] CircularHealthBar component is on same GameObject as Image

**If all checked and still square:**
- The sprite itself is square! Generate a new one using my tool.

---

## 🎯 Quick Fix Script

**Can't find a circle sprite? Run this:**

```csharp
// In Unity Console, paste this:
// Creates a circle sprite programmatically

[MenuItem("Tools/Quick Fix/Add Circle to Selected Image")]
static void QuickFixCircle()
{
    GameObject selected = Selection.activeGameObject;
    Image img = selected?.GetComponent<Image>();
    if (img != null)
    {
        // Use Unity's built-in
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillOrigin = 2; // Top
        Debug.Log("✓ Applied circle sprite and radial fill!");
    }
}
```

---

*Problem: Square instead of circle*
*Solution: Assign circular sprite to Source Image*
*Tools: Built-in Knob sprite OR Generate Circle Sprite tool*

