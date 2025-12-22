# Ascendancy Panel - Prefab Mode Setup

Simple guide for using your AscendancyContainerPrefab with the panel system.

---

## 🎯 Overview

**Prefab Mode** is much simpler than manual setup:
- ✅ Just spawn your prefab
- ✅ Auto-populates all components
- ✅ No manual wiring needed
- ✅ Uses your existing prefab structure

---

## ✅ Quick Setup (3 Steps)

### **Step 1: Create Panel Background**

1. **In CharacterDisplayUI scene**
2. **Right-click Canvas → UI → Panel**
3. **Name:** `AscendancyDisplayPanel`
4. **Configure:**
   ```
   Rect Transform:
   └─ Fill screen (Anchor: 0,0 to 1,1)
   
   Image (Background):
   ├─ Color: Black (0, 0, 0, 0.9) - Dark overlay
   └─ Raycast Target: ✅
   ```
5. **Uncheck Active** (starts hidden)

---

### **Step 2: Add Close Button**

1. **Right-click AscendancyDisplayPanel → UI → Button**
2. **Name:** `CloseButton`
3. **Position:** Top-right corner
   ```
   Rect Transform:
   ├─ Anchor: Top-right (1, 1)
   ├─ Position: (-40, -40)
   └─ Size: 60 x 60
   ```
4. **Add Text child:** "X" or "Close"

---

### **Step 3: Configure AscendancyDisplayPanel Component**

1. **Select `AscendancyDisplayPanel` GameObject**
2. **Add Component:** `AscendancyDisplayPanel`
3. **Configure in Inspector:**
   ```
   Prefab Mode:
   ├─ Use Prefab Mode: ✅
   ├─ Ascendancy Container Prefab: [Drag AscendancyContainerPrefab]
   └─ Content Container: AscendancyDisplayPanel (self)
   
   Panel References:
   ├─ Panel Root: AscendancyDisplayPanel (self)
   └─ Close Button: CloseButton
   
   Settings:
   └─ Show Debug Logs: ✅
   ```

4. **That's it!** No need to create text components manually.

---

### **Step 4: Assign to CharacterDisplayController**

1. **Select `CharacterDisplayController` GameObject**
2. **In Inspector → Ascendancy Display:**
   ```
   Ascendancy Display Panel: [Drag AscendancyDisplayPanel]
   ```

---

## 🎮 How It Works

When you click an Ascendancy button:

```
Click Ascendancy1
    ↓
OnAscendancyClicked(ascendancy)
    ↓
ascendancyDisplayPanel.ShowAscendancy(ascendancy)
    ↓
Panel spawns AscendancyContainerPrefab
    ↓
Auto-finds and populates:
  - NameText → "Crumbling Earth"
  - TagLine → "Every strike chips..."
  - AscendancySplashArt → Splash image
  - AvailablePointsText → "Points: 0/8"
    ↓
Calls TreeDisplayContainer.DisplayAscendancy()
    ↓
Spawns passive nodes in tree
    ↓
Panel opens with complete Ascendancy tree!
```

---

## 📊 Your Prefab Structure

Looking at your prefab, it has:

```
AscendancyContainerPrefab
├─ SplashArt (Mask)
│   └─ AscendancySplashArt (Image) ← Auto-populated
├─ FrameOverlay (Image)
├─ CircularFrame (Image) ← Auto-tinted with theme color
├─ NameText (TextMeshPro) ← Auto-populated
├─ TagLine (TextMeshPro) ← Auto-populated
├─ TreeDisplayContainer (AscendancyTreeDisplay) ← Auto-populated with tree
│   └─ NodesContainer
└─ ProgressionInfo
    └─ AvailablePointsText (TextMeshPro) ← Auto-populated
```

**All components are auto-found and populated!** ✨

---

## 🔧 What Gets Auto-Populated

### **By AscendancyContainerController:**
- Splash Art Image → `ascendancy.splashArt`
- Frame color → `ascendancy.themeColor`

### **By PopulatePrefabComponents:**
- `NameText` → `ascendancy.ascendancyName`
- `TagLine` → `ascendancy.tagline`
- `AvailablePointsText` → Points info

### **By AscendancyTreeDisplay:**
- Start node
- All branch nodes
- Connection lines
- Node states (locked/available/unlocked)

---

## 🧪 Testing

1. **Press Play**
2. **Select Marauder**
3. **Go to CharacterDisplayUI**
4. **Click "Crumbling Earth" button**
5. **Verify:**
   - ✅ Panel opens
   - ✅ Shows splash art in circular frame
   - ✅ Shows "Crumbling Earth" name
   - ✅ Shows tagline
   - ✅ Shows "Points: 0/8"
   - ✅ Shows Start node at center
   - ✅ Shows 3 branches with nodes
   - ✅ Nodes connected with lines
   - ✅ Click X to close

**Console output:**
```
[AscendancyDisplayPanel] Showing: Crumbling Earth
[AscendancyDisplayPanel] Spawned container: AscendancyContainer_Crumbling Earth
[AscendancyDisplayPanel] ✓ Container controller initialized
[AscendancyDisplayPanel] ✓ Set name: Crumbling Earth
[AscendancyDisplayPanel] ✓ Set tagline: Every strike chips...
[AscendancyDisplayPanel] ✓ Tree display initialized
[AscendancyTreeDisplay] Displayed Crumbling Earth with 10 passives
✓ Opened Ascendancy panel: Crumbling Earth
```

---

## 🎨 Customizing the Panel

### **Add Background Blur/Dim:**

The panel background already dims the screen. To blur:

1. **Select AscendancyDisplayPanel (root)**
2. **Image component → Material:** Assign blur material

---

### **Adjust Container Size:**

Your prefab has:
```
Size Delta: (700, 700)
```

To change:
1. Open `AscendancyContainerPrefab.prefab`
2. Adjust root RectTransform size
3. Save prefab
4. Changes apply automatically

---

### **Change Tree Layout:**

In your prefab's `TreeDisplayContainer`:
```
AscendancyTreeDisplay component:
├─ Node Spacing: 120 (increase for more spread)
├─ Branch Spacing: 200 (increase for wider branches)
├─ Connection Width: 5 (thicker lines)
└─ Connection Color: Gold (theme color)
```

---

## 📋 Setup Checklist

- [ ] Create Panel background (dark overlay)
- [ ] Add AscendancyDisplayPanel component
- [ ] Assign AscendancyContainerPrefab
- [ ] Create CloseButton
- [ ] Assign panel to CharacterDisplayController
- [ ] Hide panel (uncheck Active)
- [ ] Test!

---

## 💡 Benefits of Prefab Mode

✅ **Simple Setup:** Just assign prefab + close button
✅ **Auto-Population:** All components found automatically
✅ **Consistent:** Uses your designed prefab structure
✅ **Easy Updates:** Change prefab, all instances update
✅ **No Manual Wiring:** System finds components by name

---

**Last Updated:** 2024-12-19
**Status:** ✅ Prefab Mode Implemented - Simplest Setup!


