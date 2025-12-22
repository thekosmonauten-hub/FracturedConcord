# Ascendancy Inspector Quick Setup

Fast copy-paste guide for setting up your Crumbling Earth asset in Unity Inspector.

---

## 🚀 Complete Inspector Setup

### **1. Enable Branch System**

```
Tree Branches (Recommended)
├─ Use Branch System: ✅
```

---

### **2. Setup Start Node**

Click the Start Node dropdown:

```
Start Node:
├─ Name: Crumbling Earth
├─ Node Type: Start
├─ Description: Your journey into the Crumbling Earth begins here.
├─ Icon: [Drag your center/core icon]
├─ Node Scale: 1.5
├─ Point Cost: 0
├─ Unlocked By Default: ✅
├─ Prerequisites: (Size: 0)
└─ Tree Position: (0, 0)
```

---

### **3. Setup Branches**

Set size to 3:
```
Branches: Size = 3
```

---

### **Branch 0: Path of Destruction (Left)**

```
Element 0
├─ Branch Name: Path of Destruction
├─ Branch Theme: Maximize damage and Crumble magnitude
├─ Branch Angle: 270
├─ Horizontal Offset: -150

Branch Nodes: Size = 4

    [0] Minor:
    ├─ Name: Attack and Crumble Magnitude
    ├─ Node Type: Minor
    ├─ Description: 10% increased Attack damage and 15% increased Crumble Magnitude
    ├─ Icon: [Drag icon]
    └─ Point Cost: 1
    
    [1] Major:
    ├─ Name: Blood Price
    ├─ Node Type: Major
    ├─ Description: Lose 3% current HP when you Attack; Attacks deal +30% more damage
    ├─ Icon: [Drag blood icon]
    ├─ Node Scale: 1.3
    └─ Point Cost: 1
    
    [2] Minor:
    ├─ Name: Spring of Rage
    ├─ Node Type: Minor
    ├─ Description: Gain +10% Maximum Mana and 30% more Crumble Magnitude when below 75% Life
    ├─ Icon: [Drag mana icon]
    └─ Point Cost: 1
    
    [3] Major:
    ├─ Name: Final Offering
    ├─ Node Type: Major
    ├─ Description: On death's door (≤25% Life), trigger all active Crumble stacks instantly
    ├─ Icon: [Drag explosion icon]
    ├─ Node Scale: 1.3
    └─ Point Cost: 1
```

---

### **Branch 1: Path of Resilience (Center)**

```
Element 1
├─ Branch Name: Path of Resilience
├─ Branch Theme: Sustain and survival
├─ Branch Angle: 0
├─ Horizontal Offset: 0

Branch Nodes: Size = 3

    [0] Minor:
    ├─ Name: Crumble Duration and Guard Effectiveness
    ├─ Node Type: Minor
    ├─ Description: 20% increased Crumble Duration and 10% increased Guard effectiveness
    ├─ Icon: [Drag shield icon]
    └─ Point Cost: 1
    
    [1] Major:
    ├─ Name: Trembling Echo
    ├─ Node Type: Major
    ├─ Description: The first Attack each turn repeats for 50% effect if the target is Crumbling
    ├─ Icon: [Drag echo icon]
    ├─ Node Scale: 1.3
    └─ Point Cost: 1
    
    [2] Major:
    ├─ Name: Seismic Hunger
    ├─ Node Type: Major
    ├─ Description: Crumble explosions heal you for 10% of damage dealt
    ├─ Icon: [Drag heart icon]
    ├─ Node Scale: 1.3
    └─ Point Cost: 1
```

---

### **Branch 2: Path of Endurance (Right)**

```
Element 2
├─ Branch Name: Path of Endurance
├─ Branch Theme: Defensive synergies
├─ Branch Angle: 90
├─ Horizontal Offset: 150

Branch Nodes: Size = 2

    [0] Minor:
    ├─ Name: Thrill of Agony
    ├─ Node Type: Minor
    ├─ Description: While affected by Bleed or Burning, Crumble damage deals +50% more
    ├─ Icon: [Drag flame icon]
    └─ Point Cost: 1
    
    [1] Major:
    ├─ Name: Stoneskin
    ├─ Node Type: Major
    ├─ Description: Unaffected by Bleed and Burning
    ├─ Icon: [Drag stone icon]
    ├─ Node Scale: 1.3
    └─ Point Cost: 1
```

---

### **4. Configure Tree Settings**

```
Tree Structure:
├─ Use Auto Generated Paths: ✅
└─ Number Of Branches: 3
```

---

### **5. Save**

Press **Ctrl+S** to save the asset.

---

## 🎨 Visual Result

```
                Crumbling Earth
                    (Start)
                       |
        ┌──────────────┼──────────────┐
        │              │              │
    Attack &       Crumble        Thrill of
   Magnitude       Duration         Agony
    (Minor)         (Minor)         (Minor)
        │              │              │
   Blood Price    Trembling        Stoneskin
    (Major)          Echo           (Major)
        │           (Major)
   Spring of          │
    Rage          Seismic
    (Minor)        Hunger
        │           (Major)
  Final Offering
    (Major)
```

---

## 📊 Node Type Guide

Looking at your current tree, here's the recommended type for each:

| Node | Current Type | Recommended | Reason |
|------|--------------|-------------|--------|
| Crumbling Earth | Start (0) | Start | ✅ Correct |
| Attack & Crumble Magnitude | Minor (1) | Minor | ✅ Correct - small stat bonus |
| Crumble Duration | Minor (1) | Minor | ✅ Correct - small stat bonus |
| Blood Price | Major (2) | Major | ✅ Correct - powerful effect |
| Trembling Echo | Major (2) | Major | ✅ Correct - build-defining |
| Seismic Hunger | Major (2) | Major | ✅ Correct - powerful heal |
| Spring of Rage | Major (2) | **Minor** | Should be Minor (small stat boost) |
| Thrill of Agony | Major (2) | **Minor** | Should be Minor (conditional bonus) |
| Stoneskin | Major (2) | Major | ✅ Correct - immunity is powerful |
| Final Offering | Major (2) | Major | ✅ Correct - build-defining |

**Suggestion:** Change Spring of Rage and Thrill of Agony to Minor nodes.

---

## 💡 Why Branch System is Better

### **Your Current Setup:**
- All 10 nodes in one list
- Prerequisites manually set between scattered nodes
- Hard to visualize the tree structure
- Confusing indices

### **With Branches:**
- Clear separation: Left (damage), Center (sustain), Right (defense)
- Sequential indices per branch: [0], [1], [2], [3]...
- Easy to add/remove nodes within a branch
- Clear themes for each path

---

## 🔧 Alternative: Keep Current Setup

If you prefer the current flat list:

1. **Set:** `Use Branch System: ☐` (disable)
2. **Fix Node Types:**
   - Change Spring of Rage to Minor
   - Change Thrill of Agony to Minor
3. **Set Tree Positions manually:**
   - Node 0 (Start): (0, 0)
   - Node 1-4: Left branch (-150, -100/-220/-340/-460)
   - Node 5-7: Center branch (0, -100/-220/-340)
   - Node 8-9: Right branch (150, -100/-220)

---

## ✅ Recommended: Use Branch System

It's much cleaner and matches your 3-branch design perfectly!

**Time to migrate:** 10-15 minutes
**Benefits:** Organized, maintainable, clear structure

---

**Last Updated:** 2024-12-19
**Ready to migrate? The branch system will make your tree much easier to manage!** 🎉


