# Ascendancy Tooltip Setup

Guide for setting up node hover tooltips using your AscendancyTooltip prefab.

---

## 🎯 Overview

When hovering over a node in the Ascendancy tree:
- ✅ Tooltip appears with node name and description
- ✅ Auto-positions near the node
- ✅ Clamps to screen (won't go off-screen)
- ✅ Hides when mouse leaves node

---

## ✅ Quick Setup (5 Minutes)

### **Step 1: Add Tooltip Controller to Container**

1. **Open:** `AscendancyContainerPrefab.prefab`
2. **Select:** Root GameObject (`AscendancyContainerPrefab`)
3. **Add Component:** `AscendancyTooltipController`
4. **Configure:**
   ```
   Tooltip Prefab: [Drag AscendancyTooltip.prefab]
   Tooltip Container: AscendancyContainerPrefab (self)
   Tooltip Offset: (120, 0) - Offset from node
   Follow Mouse: ☐ - Fixed position (or ✅ for follow)
   Show Debug Logs: ✅
   ```

---

### **Step 2: Wire Up TreeDisplay**

1. **Select:** `TreeDisplayContainer` (child of prefab)
2. **In AscendancyTreeDisplay component:**
   ```
   Tooltip System:
   ├─ Tooltip Controller: [Drag AscendancyContainerPrefab]
   │   (The root with AscendancyTooltipController component)
   └─ Enable Tooltips: ✅
   ```

---

### **Step 3: Save Prefab**

Press **Ctrl+S**

---

## 🧪 Testing

1. **Press Play**
2. **Select Marauder**
3. **Click Crumbling Earth button**
4. **Hover over any node**
5. **Verify:**
   - ✅ Tooltip appears
   - ✅ Shows node name (e.g., "Blood Price")
   - ✅ Shows description
   - ✅ Positioned to the right of node
   - ✅ Move mouse away → tooltip disappears

**Console shows:**
```
[AscendancyTooltip] Showing tooltip for: Blood Price
[AscendancyTooltip] Set name: Blood Price
[AscendancyTooltip] Set description: Lose 3% current HP when you Attack...
```

---

## 🎨 Tooltip Positioning

### **Your Prefab Structure:**

Looking at `AscendancyTooltip.prefab`:
```
AscendancyTooltip (RectTransform: 774x247)
├─ Background (Image)
├─ Header
│   └─ AscendancyName (TextMeshPro) ← Auto-populated
└─ Content
    └─ AscendancyNodeDescription (TextMeshPro) ← Auto-populated
```

**Auto-finds and populates:**
- `AscendancyName` → Node name
- `AscendancyNodeDescription` → Node description

---

### **Offset Settings:**

```
Tooltip Offset:
├─ X: 120 (to the right of node)
└─ Y: 0 (same vertical level)
```

**Try different offsets:**

**Right of node:**
```
Tooltip Offset: (120, 0)
```

**Above node:**
```
Tooltip Offset: (0, 150)
```

**Top-right:**
```
Tooltip Offset: (80, 80)
```

**Mouse cursor:**
```
Follow Mouse: ✅
Tooltip Offset: (20, 20)
```

---

## 🔧 Advanced: Follow Mouse

For tooltip that follows cursor:

```
AscendancyTooltipController:
├─ Follow Mouse: ✅
├─ Follow Speed: 10 (smooth follow)
└─ Tooltip Offset: (20, 20) (cursor offset)
```

**Behavior:** Tooltip smoothly follows mouse cursor while hovering

---

## 🎨 Tooltip Prefab Customization

### **Your Prefab Already Has:**
- ✅ Background image
- ✅ Header section with name
- ✅ Content section with description
- ✅ Proper layout (774x247)

**Perfect for tooltips!** No changes needed.

---

### **Optional Enhancements:**

**Add Node Type Indicator:**

1. **Open AscendancyTooltip.prefab**
2. **Add TextMeshPro in Header:**
   ```
   Name: NodeType
   Text: "MAJOR NODE"
   Font Size: 12
   Color: Gold
   Position: Top-right of header
   ```

**Add Point Cost:**

Add TextMeshPro:
```
Name: PointCost
Text: "Cost: 1 point"
Position: Bottom of content
```

The system will auto-populate these if found!

---

## 📋 Setup Checklist

- [ ] Open AscendancyContainerPrefab
- [ ] Add AscendancyTooltipController to root
- [ ] Assign AscendancyTooltip.prefab
- [ ] Set tooltip offset (120, 0)
- [ ] Assign controller to TreeDisplayContainer
- [ ] Enable tooltips in TreeDisplay
- [ ] Save prefab
- [ ] Test hover functionality

---

## 🐛 Troubleshooting

### **❌ Tooltip doesn't appear:**

**Check 1: Controller assigned?**
```
AscendancyContainerPrefab → AscendancyTooltipController component exists
TreeDisplayContainer → Tooltip Controller field assigned
```

**Check 2: Prefab assigned?**
```
AscendancyTooltipController → Tooltip Prefab: AscendancyTooltip
```

**Check 3: Tooltips enabled?**
```
TreeDisplayContainer → Enable Tooltips: ✅
```

**Check 4: Nodes have hover events?**
```
Console should show:
[AscendancyTreeDisplay] Hovering: Blood Price
```

---

### **❌ Tooltip appears at wrong position:**

**Adjust offset:**
```
Tooltip Offset:
├─ Increase X (move right): 150
├─ Increase Y (move up): 50
```

**Or enable Follow Mouse:**
```
Follow Mouse: ✅
```

---

### **❌ Tooltip goes off-screen:**

The system auto-clamps, but you can adjust:
- Reduce tooltip size in prefab
- Reduce offset values
- Check parent canvas settings

---

## 💡 Tooltip Behavior

**On Hover Enter:**
```
Mouse enters node
    ↓
OnNodeHoverEnter() triggered
    ↓
tooltipController.ShowTooltip(passive, nodePosition)
    ↓
Tooltip spawned and positioned
    ↓
Text populated with node data
    ↓
Tooltip appears!
```

**On Hover Exit:**
```
Mouse leaves node
    ↓
OnNodeHoverExit() triggered
    ↓
tooltipController.HideTooltip()
    ↓
Tooltip destroyed
```

---

## 🎮 What Gets Auto-Populated

Looking at your tooltip prefab, it has:
- `AscendancyName` (TextMeshPro) → Gets `passive.name`
- `AscendancyNodeDescription` (TextMeshPro) → Gets `passive.description`

**System searches for these names and auto-populates them!**

---

**Last Updated:** 2024-12-19
**Status:** ✅ Tooltip System Implemented
**Next:** Wire up the controller in your prefab and test!


