# Fixing Ascendancy Node Spacing

Guide to fix nodes clustering together and properly space branches.

---

## 🎯 The Issue

Nodes are all bunched together because:
1. Branch angles not set (all at 0°)
2. Horizontal offsets not configured
3. Position calculation needs angles

---

## ✅ Quick Fix: Set Branch Angles

### **Open Your Ascendancy Asset:**

1. **In Project:** `Assets/Resources/Ascendancies/MarauderCrumblingEarth.asset`
2. **Double-click** to open in Inspector

### **Configure Each Branch:**

```
Branches (Size: 3)

Element 0 (Left Branch):
├─ Branch Name: "Path of Destruction"
├─ Branch Angle: 210 (or -150) ← Bottom-left
└─ Horizontal Offset: -150

Element 1 (Right Branch):
├─ Branch Name: "Path of Resilience"
├─ Branch Angle: 330 (or -30) ← Bottom-right
└─ Horizontal Offset: 150

Element 2 (Top Branch):
├─ Branch Name: "Path of Endurance"
├─ Branch Angle: 90 ← Up/Top
└─ Horizontal Offset: 0
```

---

## 🎨 Branch Angle Guide

### **Standard 3-Branch Layout:**

```
           90° (Top)
              |
         [Branch 2]
              |
              |
          [START] ────────────── 0° (Right)
            /   \
           /     \
          /       \
    210° /         \ 330°
   (Bottom-left)  (Bottom-right)
   [Branch 0]     [Branch 1]
```

### **Alternative: Side Branches**

```
    270° (Left)
        |
   [Branch 0]
        |
        |
    [START] ───────── 90° (Up)
        |              |
        |          [Branch 2]
        |
    [Branch 1]
        |
    180° (Down)
```

---

## 🔧 Angle Reference

**Angles in degrees (0° = Right, counter-clockwise):**

```
        90° (Up)
         |
         |
270° ────┼──── 90°
(Left)   |   (Right)
         |
         |
       180° (Down)
```

**Common Configurations:**

**2 Branches:**
- Branch 0: 180° (down-left)
- Branch 1: 0° (down-right)

**3 Branches (Y-shape):**
- Branch 0: 210° (bottom-left)
- Branch 1: 330° (bottom-right)
- Branch 2: 90° (top)

**3 Branches (T-shape):**
- Branch 0: 180° (down)
- Branch 1: 270° (left)
- Branch 2: 90° (up/right)

**4 Branches (Cross):**
- Branch 0: 0° (right)
- Branch 1: 90° (up)
- Branch 2: 180° (left)
- Branch 3: 270° (down)

---

## 📐 Spacing Settings

### **In MarauderCrumblingEarth asset:**

```
Tree Structure:
├─ Use Auto Generated Paths: ✅
└─ Number Of Branches: 3
```

### **In TreeDisplayContainer (prefab):**

```
AscendancyTreeDisplay:
├─ Node Spacing: 120 (distance between nodes)
├─ Branch Spacing: 200 (distance between branches)
└─ Draw Connections: ✅
```

---

## 🧪 Test After Setting Angles

1. **Save** MarauderCrumblingEarth asset
2. **Press Play**
3. **Click Crumbling Earth button**
4. **Check Console for:**
   ```
   [AscendancyBranch] [NodeName] positioned at (x, y) (branch: Path of Destruction, angle: 210°)
   [AscendancyTreeDisplay] Positioned [NodeName] at (x, y)
   ```

5. **Verify:**
   - ✅ Start node at center (0, 0)
   - ✅ Branch 0 extends bottom-left
   - ✅ Branch 1 extends bottom-right
   - ✅ Branch 2 extends up
   - ✅ Nodes evenly spaced along each branch
   - ✅ Connection lines drawn

---

## 🎨 Expected Result

**With Angles: 210°, 330°, 90°:**

```
            [Node]
            [Node]
              |
           [START]
           /     \
          /       \
      [Node]     [Node]
      [Node]     [Node]
```

**Each branch extends in its own direction!**

---

## 📋 Full Branch Setup Example

```yaml
Branch 0 (Path of Destruction):
  Branch Name: Path of Destruction
  Branch Theme: Maximize damage
  Branch Angle: 210
  Horizontal Offset: -150
  Branch Nodes: [Size: 4]
    [0] Attack & Magnitude (Minor)
    [1] Blood Price (Major)
    [2] Spring of Rage (Minor)
    [3] Final Offering (Major)

Branch 1 (Path of Resilience):
  Branch Name: Path of Resilience
  Branch Angle: 330
  Horizontal Offset: 150
  Branch Nodes: [Size: 3]
    [0] Crumble Duration (Minor)
    [1] Trembling Echo (Major)
    [2] Seismic Hunger (Major)

Branch 2 (Path of Endurance):
  Branch Name: Path of Endurance
  Branch Angle: 90
  Horizontal Offset: 0
  Branch Nodes: [Size: 2]
    [0] Thrill of Agony (Minor)
    [1] Stoneskin (Major)
```

---

## 🐛 Still Clustered?

**Check Console for positions:**
```
If all show (0, 0) or (150, 0):
→ Angles not set or not calculating correctly
→ Set Branch Angle for each branch
→ Save asset and test again
```

**Enable full debug:**
```
MarauderCrumblingEarth:
└─ Show Debug Logs: ✅

TreeDisplayContainer:
└─ Show Debug Logs: ✅
```

---

## 💡 Quick Summary

**The fix:**
1. Open MarauderCrumblingEarth asset
2. Set Branch Angle for each branch (210°, 330°, 90°)
3. Save asset
4. Test → Nodes spread out properly!

---

**After setting branch angles, nodes will spread out into a proper tree structure!** 🌳


