# Migrating to Branch System

How to reorganize your Crumbling Earth tree from flat list to organized branches.

---

## 🎯 Current vs New Structure

### **Current (Flat List):**
```
Passive Abilities (10 nodes)
├─ [0] Crumbling Earth (Start)
├─ [1] Blood Price (Major)
├─ [2] Trembling Echo (Major)
├─ [3] Seismic Hunger (Major)
├─ [4] Spring of Rage (Major)
├─ [5] Thrill of Agony (Major)
├─ [6] Stoneskin (Major)
├─ [7] Final Offering (Major)
├─ [8] Attack and Crumble Magnitude (Minor)
└─ [9] Crumble Duration and Guard Effectiveness (Minor)
```
**Problem:** All nodes in one list, hard to see tree structure

---

### **New (Branch System):**
```
Start Node:
└─ Crumbling Earth (Start)

Branches (3):
├─ Branch 1 (Left)
│   ├─ [0] Attack and Crumble Magnitude (Minor)
│   ├─ [1] Blood Price (Major)
│   ├─ [2] Spring of Rage (Minor)
│   └─ [3] Final Offering (Major)
│
├─ Branch 2 (Center)
│   ├─ [0] Crumble Duration (Minor)
│   ├─ [1] Trembling Echo (Major)
│   └─ [2] Seismic Hunger (Major)
│
└─ Branch 3 (Right)
    ├─ [0] Thrill of Agony (Minor)
    └─ [1] Stoneskin (Major)
```
**Benefits:** Clear paths, easy to understand, sequential indices per branch

---

## ✅ Migration Steps

### **Step 1: Enable Branch System**

1. **Open `MarauderCrumblingEarth` asset**
2. **Find Tree Branches section:**
   ```
   Tree Branches (Recommended)
   └─ Use Branch System: ✅ (enable this)
   ```

---

### **Step 2: Setup Start Node**

1. **Expand Start Node field**
2. **Copy from Passive Abilities [0]:**
   ```
   Start Node:
   ├─ Name: "Crumbling Earth"
   ├─ Node Type: Start
   ├─ Description: "Your journey into the Crumbling Earth now."
   ├─ Icon: [Copy icon]
   ├─ Node Scale: 1.5
   ├─ Point Cost: 0
   └─ Unlocked By Default: ✅
   ```

---

### **Step 3: Create 3 Branches**

1. **Set Branches size to 3:**
   ```
   Branches
   └─ Size: 3
   ```

---

### **Step 4: Setup Branch 1 (Left - Damage Focus)**

```
Branches → Element 0

Branch Info:
├─ Branch Name: "Path of Destruction"
├─ Branch Theme: "Offensive focus - maximize Crumble damage"
└─ Branch Angle: 270 (left)

Branch Nodes:
├─ Size: 4
│
├─ [0] Attack and Crumble Magnitude (Minor)
│   ├─ Node Type: Minor
│   ├─ Description: "10% increased Attack damage and 15% increased Crumble Magnitude"
│   ├─ Icon: [Copy from Element 8]
│   └─ Point Cost: 1
│
├─ [1] Blood Price (Major)
│   ├─ Node Type: Major
│   ├─ Description: "Lose 3% current HP when you Attack; Attacks deal +30% more damage"
│   ├─ Icon: [Copy from Element 1]
│   ├─ Node Scale: 1.3
│   └─ Point Cost: 1
│
├─ [2] Spring of Rage (Minor)
│   ├─ Node Type: Minor
│   ├─ Description: "Gain +10% Maximum Mana and 30% more Crumble Magnitude when below 75% Life"
│   ├─ Icon: [Copy from Element 4]
│   └─ Point Cost: 1
│
└─ [3] Final Offering (Major)
    ├─ Node Type: Major
    ├─ Description: "On death's door (≤25% Life), trigger all active Crumble stacks instantly"
    ├─ Icon: [Copy from Element 7]
    ├─ Node Scale: 1.3
    └─ Point Cost: 1
```

---

### **Step 5: Setup Branch 2 (Center - Sustain Focus)**

```
Branches → Element 1

Branch Info:
├─ Branch Name: "Path of Resilience"
├─ Branch Theme: "Sustain and survival through Crumble"
└─ Branch Angle: 0 (up/center)

Branch Nodes:
├─ Size: 3
│
├─ [0] Crumble Duration and Guard Effectiveness (Minor)
│   ├─ Node Type: Minor
│   ├─ Description: "20% increased Crumble Duration and 10% increased Guard effectiveness"
│   ├─ Icon: [Copy from Element 9]
│   └─ Point Cost: 1
│
├─ [1] Trembling Echo (Major)
│   ├─ Node Type: Major
│   ├─ Description: "The first Attack each turn repeats for 50% effect if the target is Crumbling"
│   ├─ Icon: [Copy from Element 2]
│   ├─ Node Scale: 1.3
│   └─ Point Cost: 1
│
└─ [2] Seismic Hunger (Major)
    ├─ Node Type: Major
    ├─ Description: "Crumble explosions heal you for 10% of damage dealt"
    ├─ Icon: [Copy from Element 3]
    ├─ Node Scale: 1.3
    └─ Point Cost: 1
```

---

### **Step 6: Setup Branch 3 (Right - Defensive Focus)**

```
Branches → Element 2

Branch Info:
├─ Branch Name: "Path of Endurance"
├─ Branch Theme: "Defensive synergies and immunity"
└─ Branch Angle: 90 (right)

Branch Nodes:
├─ Size: 2
│
├─ [0] Thrill of Agony (Minor)
│   ├─ Node Type: Minor
│   ├─ Description: "While affected by Bleed or Burning, Crumble damage deals +50% more"
│   ├─ Icon: [Copy from Element 5]
│   └─ Point Cost: 1
│
└─ [1] Stoneskin (Major)
    ├─ Node Type: Major
    ├─ Description: "Unaffected by Bleed and Burning"
    ├─ Icon: [Copy from Element 6]
    ├─ Node Scale: 1.3
    └─ Point Cost: 1
```

---

### **Step 7: Configure Auto-Generation**

```
Tree Structure:
├─ Use Auto Generated Paths: ✅
└─ Number Of Branches: 3
```

This will automatically:
- Position Start at (0, 0)
- Create 3 branches at angles: 270° (left), 0° (up), 90° (right)
- Set all prerequisites automatically
- Calculate positions

---

### **Step 8: Clean Up Legacy List**

Once branches are set up:
1. Keep the old `passiveAbilities` list (for backwards compatibility)
2. OR clear it (system uses branches if `useBranchSystem = true`)

---

## 🎨 Final Structure

Your tree will look like:

```
                    Crumbling Earth
                       (START)
                          |
      ┌───────────────────┼───────────────────┐
      │                   │                   │
  Attack &            Crumble                Thrill
  Magnitude           Duration              of Agony
   (Minor)             (Minor)               (Minor)
      │                   │                   │
  Blood Price        Trembling Echo        Stoneskin
   (Major)             (Major)               (Major)
      │                   │
  Spring of           Seismic Hunger
   Rage                (Major)
   (Minor)
      │
  Final Offering
   (Major)
```

**3 distinct paths with clear themes!**

---

## 💡 Benefits of Branch System

✅ **Organization:** Each branch is its own list
✅ **Sequential Indices:** Branch 1 [0,1,2,3], Branch 2 [0,1,2], etc.
✅ **Clear Themes:** Each branch has a focus (Damage, Sustain, Defense)
✅ **Easy Management:** Add/remove nodes per branch
✅ **Auto-Connection:** System chains nodes automatically
✅ **Auto-Positioning:** No manual coordinates needed

---

## 🔧 Quick Migration Checklist

- [ ] Enable `Use Branch System = ✅`
- [ ] Move "Crumbling Earth" to Start Node
- [ ] Create 3 branches
- [ ] Distribute nodes across branches by theme
- [ ] Set Node Types (Minor/Major)
- [ ] Enable `Use Auto Generated Paths = ✅`
- [ ] Save asset
- [ ] Test tree display

---

## 🧪 Testing

After migration:

1. **Press Play**
2. **View Ascendancy tree**
3. **Verify:**
   - ✅ Start node at center
   - ✅ 3 branches extending out
   - ✅ Nodes properly sized (Major > Minor)
   - ✅ Connection lines drawn
   - ✅ All nodes accessible

---

**This structure is MUCH cleaner and matches your design better!** 🎉


