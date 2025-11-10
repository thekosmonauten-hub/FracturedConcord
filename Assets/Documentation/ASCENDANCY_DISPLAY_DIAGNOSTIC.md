# Ascendancy Display Diagnostic Guide

Step-by-step troubleshooting for when Ascendancies don't display on CharacterDisplayUI.

---

## 🔍 Diagnostic Checklist

### **Step 1: Check Console Output**

Press Play and look for these messages in Console:

#### **✅ SUCCESS - Should See:**
```
[AscendancyDatabase] Loaded 3 Ascendancies from Resources/Ascendancies
[CharacterDisplayController] Displaying 3 Ascendancies for Marauder
━━━ [AscendancyButton] Initializing 'Ascendancy1' with Crumbling Earth ━━━
  Splash Art: CrumblingEarth_Splash
  Icon: CrumblingEarth_Icon
  splashArtImage component: Found
  ✓ Set splash art on splashArtImage: CrumblingEarth_Splash
  ✓ Initialized: Crumbling Earth (Locked: true)
```

#### **❌ ERROR - Common Issues:**

**Issue 1: No Ascendancies Loaded**
```
[AscendancyDatabase] No Ascendancies found in Resources/Ascendancies
[CharacterDisplayController] No Ascendancies found for class: Marauder
```
**Fix:** Assets not in correct folder (see Step 2)

**Issue 2: Wrong Base Class**
```
[AscendancyDatabase] Loaded 3 Ascendancies
[CharacterDisplayController] No Ascendancies found for class: Marauder
```
**Fix:** Base Class field doesn't match (see Step 3)

**Issue 3: No Image Components**
```
[AscendancyButton] 'Ascendancy1' has no Image components assigned!
```
**Fix:** Button needs Image component (see Step 4)

---

### **Step 2: Verify Asset Location** ⭐ **MOST COMMON**

1. **Open Project window**
2. **Navigate to:** `Assets/Resources/Ascendancies/`
3. **Verify folder structure:**
   ```
   Assets/
   └── Resources/           ← MUST be named "Resources"
       └── Ascendancies/    ← MUST be named "Ascendancies"
           ├── MarauderCrumblingEarth.asset
           ├── MarauderIronVanguard.asset
           └── MarauderDiscipleOfWar.asset
   ```

**Common Mistakes:**
- ❌ `Assets/Ascendancies/` - Missing "Resources" folder
- ❌ `Assets/Resources/Ascendancy/` - Wrong folder name (needs "Ascendancies")
- ❌ Files in subfolder like `Resources/Ascendancies/Marauder/` - Should be directly in Ascendancies

---

### **Step 3: Verify Base Class Field** ⭐ **CRITICAL**

1. **Select an Ascendancy asset** (e.g., `MarauderCrumblingEarth`)
2. **Check Inspector → Basic Info:**
   ```
   Ascendancy Name: Crumbling Earth
   Base Class: Marauder  ← CHECK THIS!
   ```

**MUST MATCH EXACTLY (case-sensitive):**
- ✅ `"Marauder"` - Correct
- ❌ `"marauder"` - Wrong (lowercase)
- ❌ `"MARAUDER"` - Wrong (uppercase)
- ❌ `" Marauder"` - Wrong (extra space)
- ❌ Empty - Wrong (not set)

**Quick Fix:**
1. Delete the text in Base Class field
2. Type exactly: `Marauder`
3. Save asset (Ctrl+S)
4. Repeat for all 3 Marauder Ascendancies

---

### **Step 4: Verify AscendancyDatabase Exists**

1. **Search Hierarchy:** "AscendancyDatabase"
2. **Should find:** GameObject named `AscendancyDatabase`
3. **Check component:**
   ```
   AscendancyDatabase (Component)
   ├─ Load From Resources: ✅
   └─ Resources Path: "Ascendancies"
   ```

**If missing:**
1. Create GameObject → Name: `AscendancyDatabase`
2. Add Component → `AscendancyDatabase`
3. Set Load From Resources = ✅
4. Set Resources Path = "Ascendancies"
5. Save scene

---

### **Step 5: Verify Button Structure**

Your `Ascendancy1`, `Ascendancy2`, `Ascendancy3` GameObjects need **Image components**.

**Check each button:**

1. **Select `Ascendancy1` in Hierarchy**
2. **Check if it has:**
   - ✅ Button component
   - ✅ Image component (for displaying sprite)

**If missing Image:**
1. Select Ascendancy1
2. Add Component → UI → Image
3. Set RectTransform to fill button area
4. Repeat for Ascendancy2, Ascendancy3

---

### **Step 6: Verify Icon/Splash Art in Assets**

1. **Select an Ascendancy asset** (e.g., `MarauderCrumblingEarth`)
2. **Check Visual Assets section:**
   ```
   Visual Assets
   ├─ Splash Art: [Should have a sprite assigned]
   ├─ Icon: [Should have a sprite assigned]
   └─ Theme Color: [Any color]
   ```

**If empty:**
- Assign **at least** the `Icon` field
- Can use placeholder images initially

---

## 🧪 Complete Test Procedure

### **Test 1: Check Database Loading**

1. **Press Play**
2. **Immediately check Console** (before doing anything)
3. **Look for:**
   ```
   [AscendancyDatabase] Loaded X Ascendancies from Resources/Ascendancies
   ```

**If you see 0 Ascendancies:**
- Assets not in Resources/Ascendancies folder
- Check Step 2

**If you see 3+ Ascendancies:**
- ✓ Database loaded correctly
- Continue to Test 2

---

### **Test 2: Check Class Matching**

1. **Continue in Play Mode**
2. **Select Marauder class**
3. **Go to CharacterDisplayUI**
4. **Check Console:**
   ```
   [CharacterDisplayController] Displaying X Ascendancies for Marauder
   ```

**If X = 0:**
- Base Class field doesn't match
- Check Step 3

**If X = 3:**
- ✓ Matching works
- Continue to Test 3

---

### **Test 3: Check Button Initialization**

Still in Play Mode, check Console for:

```
━━━ [AscendancyButton] Initializing 'Ascendancy1' with Crumbling Earth ━━━
  Splash Art: MySprite
  Icon: MyIcon
  splashArtImage component: Found
  ✓ Set splash art on splashArtImage: MySprite
```

**If you see:**
```
splashArtImage component: NULL
iconImage component: NULL
[AscendancyButton] 'Ascendancy1' has no Image components!
```

**Then:**
- Your button doesn't have an Image component
- Check Step 5

---

## 🔧 Quick Fixes

### **Fix 1: Assets Not Loading**

```
Problem: [AscendancyDatabase] No Ascendancies found

Solution:
1. Move assets to: Assets/Resources/Ascendancies/
2. Verify folder is named exactly "Resources"
3. Verify subfolder is named exactly "Ascendancies"
```

---

### **Fix 2: Wrong Base Class**

```
Problem: Ascendancies load but don't match class

Solution:
1. Open each Marauder Ascendancy asset
2. Set Base Class = "Marauder" (exact spelling)
3. Save (Ctrl+S)
4. Test again
```

---

### **Fix 3: Missing Image Components**

```
Problem: [AscendancyButton] has no Image components

Solution:
1. Select Ascendancy1 GameObject
2. Add Component → UI → Image
3. Set RectTransform to fill area:
   - Anchor: Stretch/Stretch (0,0 to 1,1)
   - Offset: 0,0,0,0
4. Repeat for Ascendancy2, Ascendancy3
```

---

### **Fix 4: No Sprites Assigned**

```
Problem: Sprites show as empty/white

Solution:
1. Open Ascendancy asset
2. Assign Icon field (minimum)
3. Assign Splash Art field (optional)
4. Save asset
```

---

## 📊 Expected Hierarchy

Your CharacterDisplayUI should have:

```
CharacterDisplayUI
└── Background
    └── RightPage (or wherever you placed them)
        ├── Ascendancy1 (GameObject)
        │   ├─ Button component ✅
        │   ├─ Image component ✅ ← Shows the sprite
        │   └─ AscendancyButton ← Added at runtime
        ├── Ascendancy2 (GameObject)
        │   ├─ Button component ✅
        │   ├─ Image component ✅
        │   └─ AscendancyButton ← Added at runtime
        └── Ascendancy3 (GameObject)
            ├─ Button component ✅
            ├─ Image component ✅
            └─ AscendancyButton ← Added at runtime
```

---

## 🎯 Step-by-Step Verification

Run through this checklist in order:

### **✅ Checklist:**

- [ ] **Folder exists:** `Assets/Resources/Ascendancies/`
- [ ] **3 assets created** for Marauder
- [ ] **Base Class set** to "Marauder" in all 3 assets
- [ ] **Icon assigned** in all 3 assets
- [ ] **AscendancyDatabase GameObject exists** in scene
- [ ] **AscendancyDatabase component** configured:
  - Load From Resources: ✅
  - Resources Path: "Ascendancies"
- [ ] **Ascendancy1 has Button + Image** components
- [ ] **Ascendancy2 has Button + Image** components
- [ ] **Ascendancy3 has Button + Image** components
- [ ] **CharacterDisplayController** has all 3 buttons assigned
- [ ] **Press Play** and check Console output

---

## 💡 What To Look For

**When you Press Play, you should see this sequence:**

```
1. [AscendancyDatabase] Loaded 3 Ascendancies...
   ↓
2. [CharacterDisplayController] Displaying 3 Ascendancies for Marauder
   ↓
3. ━━━ [AscendancyButton] Initializing 'Ascendancy1' with Crumbling Earth ━━━
   ↓
4. ✓ Set splash art/icon on Image
   ↓
5. [AscendancyButton] ✓ Initialized: Crumbling Earth
```

**If any step fails, the diagnostic logs will tell you exactly what's wrong!**

---

## 🚨 Emergency Fix

If nothing works, try this:

1. **Exit Play Mode**
2. **Delete and recreate one asset:**
   - Right-click in `Assets/Resources/Ascendancies/`
   - Create → Dexiled → Ascendancy Data
   - Name: `TestAscendancy`
   - Set:
     ```
     Ascendancy Name: Test
     Base Class: Marauder
     Icon: [Any sprite]
     ```
3. **Save**
4. **Press Play**
5. **Check Console** - should see "Loaded 1 Ascendancies..."

If this works, the issue was with your original assets.

---

**Last Updated:** 2024-12-19
**Status:** ✅ Enhanced Debug Logging - Console Will Tell You Exactly What's Wrong

