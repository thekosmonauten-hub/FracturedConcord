# Fix: Converting Enemy Portrait to SpriteRenderer

## 🎯 Problem Identified

Your animation clips target `SpriteRenderer` (classID: 212) but your Enemy Portrait uses UI `Image` component.

**Result:** Animation plays but sprite never changes visually.

---

## ✅ Solution: Use SpriteRenderer

### Step 1: Modify Enemy Portrait GameObject

1. **Open Combat Scene**

2. **Find Enemy Portrait GameObject in hierarchy**
   - Usually under: `EnemyCombatDisplay > Enemy Portrait`

3. **Select it**

4. **In Inspector:**
   - Find the `Image` component
   - Click the gear icon → **Remove Component**
   - (Or just uncheck it to disable)

---

### Step 2: Add SpriteRenderer

1. **With Enemy Portrait still selected:**

2. **Add Component → Rendering → Sprite Renderer**

3. **Configure SpriteRenderer:**
   ```
   Sprite: (leave empty - will be set at runtime)
   Color: White (255, 255, 255, 255)
   Flip: None
   Material: Sprites-Default
   
   Sorting Layer: UI (or create one called "UI")
   Order in Layer: 100 (high enough to be above other UI)
   ```

4. **Important:** Check "Additional Settings"
   - **Mask Interaction: None**

---

### Step 3: Verify Animator Targets SpriteRenderer

1. **Check if Animator component exists on this GameObject**
   - Should show the Animator component
   - If not, Add Component → Animator

2. **Configure Animator:**
   ```
   Controller: (leave empty - assigned at runtime)
   Avatar: None
   Apply Root Motion: False
   Update Mode: Normal
   Culling Mode: Always Animate ← IMPORTANT!
   ```

---

### Step 4: Update EnemyCombatDisplay Script Reference

**⚠️ CRITICAL:** The script references `Image enemyPortrait` but now you're using SpriteRenderer.

**Option A: Update the Script (Recommended)**

I'll create an updated version that supports both Image and SpriteRenderer:















