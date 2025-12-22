# Curved and Bent Branch Paths

Guide for creating branches that change direction mid-path.

---

## 🎯 Overview

Create branches that curve or bend at specific nodes:

```
START
  |
  | (210°)
Node 1
  |
Node 2
  \ (Change to 270°)
   \
  Node 3
    |
  Node 4
```

**Use cases:**
- Wrap branches around obstacles
- Create flowing, organic tree shapes
- Maximize space utilization
- Create artistic layouts

---

## ✅ How to Add Direction Changes

### **Example: L-Shaped Branch**

In your Ascendancy asset (e.g., `MarauderCrumblingEarth`):

```
Branches → Element 0 (Path of Destruction)

Branch Angle: 210 (starting direction: bottom-left)

Direction Changes: Size = 1

    Element 0:
    ├─ At Node Index: 2 (change direction after node 2)
    ├─ New Angle: 270 (turn to go straight left)
    └─ New Spacing: 80 (optional, 0 = use default)
```

**Result:**
```
START
  |
  ↙ (210°)
Node 0
  |
Node 1
  |
  ← (270°) Direction change!
Node 2
  |
Node 3
```

---

## 🎨 Common Patterns

### **Pattern 1: L-Shape (Corner Turn)**

```
Direction Changes: Size = 1

Element 0:
├─ At Node Index: 2
└─ New Angle: 270 (turn left)
```

**Visual:**
```
START → Node 0 → Node 1
                    |
                    ↓
              Node 2 → Node 3
```

---

### **Pattern 2: S-Curve (Double Turn)**

```
Direction Changes: Size = 2

Element 0:
├─ At Node Index: 2
└─ New Angle: 90 (turn up)

Element 1:
├─ At Node Index: 4
└─ New Angle: 0 (turn right)
```

**Visual:**
```
    Node 4 → Node 5
       ↑
    Node 3
       ↑
START → Node 0 → Node 1 → Node 2
```

---

### **Pattern 3: Spiral**

```
Direction Changes: Size = 3

Element 0:
├─ At Node Index: 2
└─ New Angle: 90 (turn up)

Element 1:
├─ At Node Index: 4
└─ New Angle: 180 (turn left)

Element 2:
├─ At Node Index: 6
└─ New Angle: 270 (turn down)
```

**Visual:**
```
Node 4 ← Node 5
  |         ↓
Node 3   Node 6
  ↑
Node 2
  ↑
Node 1
  ↑
START
```

---

### **Pattern 4: Arc/Curve**

Instead of sharp turns, make gradual curves:

```
Direction Changes: Size = 3

Element 0: At Node Index: 1, New Angle: 220
Element 1: At Node Index: 2, New Angle: 240
Element 2: At Node Index: 3, New Angle: 260
```

**Result:** Smooth arc instead of straight line

---

## 📐 Angle Reference

```
        90° (Up)
         |
         |
180° ────┼──── 0° (Right)
(Left)   |
         |
       270° (Down)
```

**Examples:**
- 0° = Right →
- 45° = Up-right ↗
- 90° = Up ↑
- 135° = Up-left ↖
- 180° = Left ←
- 225° = Down-left ↙
- 270° = Down ↓
- 315° = Down-right ↘

---

## 🔧 Advanced: Spacing Changes

You can also change spacing at direction changes:

```
Direction Changes → Element 0:
├─ At Node Index: 2
├─ New Angle: 270
└─ New Spacing: 120 (wider spacing after turn)
```

**Use case:** Nodes spread out more after a turn

---

## 💡 Practical Example: Crumbling Earth

Let's make the left branch curve:

```
Branch 0 (Path of Destruction):
├─ Branch Angle: 210 (start bottom-left)
├─ Branch Nodes: Size = 4
│
└─ Direction Changes: Size = 1
    └─ Element 0:
        ├─ At Node Index: 2 (after Blood Price)
        └─ New Angle: 180 (turn to go left)
```

**Result:**
```
         START
           |
          ↙ (210°)
    Node 0 (Attack & Magnitude)
          |
    Node 1 (Blood Price)
          |
          ← (180°) Turn left!
    Node 2 (Spring of Rage)
          |
    Node 3 (Final Offering)
```

---

## 🧪 Testing Direction Changes

1. **Open Ascendancy asset**
2. **Add Direction Changes to a branch**
3. **Save asset**
4. **Press Play**
5. **Click Ascendancy button**
6. **Check Console:**
   ```
   [AscendancyBranch] Direction change at node 2: new angle = 270°
   [AscendancyBranch] Spring of Rage positioned at (-120, -50)
   ```
7. **Verify:** Branch curves at the specified node

---

## 📋 Setup Checklist

- [ ] Open Ascendancy asset
- [ ] Expand a branch
- [ ] Set Direction Changes size
- [ ] Add direction change entries
- [ ] Set At Node Index (where to turn)
- [ ] Set New Angle (which direction)
- [ ] Save asset
- [ ] Test tree display

---

## 🎨 Design Tips

### **Use Direction Changes For:**
- ✅ Creating L-shaped paths
- ✅ Wrapping branches around center
- ✅ Making tree fit better in container
- ✅ Creating artistic, flowing layouts
- ✅ Avoiding node overlap

### **Avoid:**
- ❌ Too many direction changes (looks chaotic)
- ❌ Sharp 180° turns (visually jarring)
- ❌ Changes every node (defeats the purpose)

### **Best Practices:**
- 1-2 direction changes per branch
- Smooth angle transitions (210° → 240° → 270°)
- Use at Major nodes (visual weight indicates turn)
- Test to ensure nodes don't overlap

---

## 🔄 Multiple Branches Example

**3-Branch Tree with Curves:**

```
Branch 0 (Left):
├─ Start Angle: 210°
└─ Direction Changes:
    └─ At Node 2: Turn to 180° (curve left)

Branch 1 (Right):
├─ Start Angle: 330°
└─ Direction Changes:
    └─ At Node 2: Turn to 0° (curve right)

Branch 2 (Top):
├─ Start Angle: 90°
└─ No direction changes (straight up)
```

**Visual:**
```
         [Node]
         [Node]
            |
         START
        /     \
    [Curve]  [Curve]
      /         \
   Left       Right
  Branch     Branch
```

---

**Last Updated:** 2024-12-19
**Status:** ✅ Direction Changes Implemented!
**Try it:** Add a direction change to one branch and see the curve!


