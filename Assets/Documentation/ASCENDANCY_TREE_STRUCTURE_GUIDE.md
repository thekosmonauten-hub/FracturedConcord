# Ascendancy Tree Structure Guide

Complete guide for creating branching Ascendancy trees with Minor and Major nodes.

---

## 🎯 Tree Structure Pattern

Your requested pattern:

```
                    START (Auto-unlocked)
                       |
        ┌──────────────┴──────────────┐
        │                             │
     Minor 1                       Minor 5
        │                             │
     Major 1                       Major 3
        │                             │
     Minor 2                       Minor 6
        │                             │
     Major 2                       Major 4
        │                             │
     Minor 3                       Minor 7
        │                             │
     Minor 4                       Minor 8
   (Left Branch)                (Right Branch)
```

**Pattern:** Start → (Minor → Major → Minor → Major) x2 branches

---

## 📊 Node Types

### **Start Node:**
- Auto-unlocked when Ascendancy is chosen
- Point Cost: 0
- Visual: Largest node (1.5x scale)
- Always at center (0, 0)

### **Minor Node:**
- Small passive bonuses
- Point Cost: 1
- Visual: Normal size (1.0x scale)
- Examples: +10% damage, +5% life, small stat boosts

### **Major Node:**
- Notable passives with powerful effects
- Point Cost: 1
- Visual: Larger (1.3x scale)
- Examples: New mechanics, build-defining bonuses

---

## ✅ Setting Up an Ascendancy Tree

### **Step 1: Create the Start Node**

In your Ascendancy asset:

```
Passive Abilities → Element 0
├─ Name: "Crumbling Earth Origin"
├─ Node Type: Start
├─ Description: "Begin your path of destruction"
├─ Point Cost: 0
├─ Unlocked By Default: ✅
├─ Tree Position: (0, 0)
└─ Prerequisites: []
```

---

### **Step 2: Create Left Branch**

Pattern: Minor → Major → Minor → Major

**Minor 1:**
```
Element 1
├─ Name: "Blood Price"
├─ Node Type: Minor
├─ Description: "Lose 5% current HP when you Attack; deal +20% more damage this turn"
├─ Point Cost: 1
├─ Tree Position: (-100, -100)
└─ Prerequisites: ["Crumbling Earth Origin"]
```

**Major 1:**
```
Element 2
├─ Name: "Seismic Hunger"
├─ Node Type: Major
├─ Description: "Crumble explosions heal you for 10% of damage dealt"
├─ Point Cost: 1
├─ Node Scale: 1.3
├─ Tree Position: (-100, -200)
└─ Prerequisites: ["Blood Price"]
```

**Minor 2:**
```
Element 3
├─ Name: "Rage Wellspring"
├─ Node Type: Minor
├─ Description: "Gain +1 Maximum Mana when below 25% Life"
├─ Point Cost: 1
├─ Tree Position: (-100, -300)
└─ Prerequisites: ["Seismic Hunger"]
```

**Major 2:**
```
Element 4
├─ Name: "Final Offering"
├─ Node Type: Major
├─ Description: "On death's door (≤10% Life), trigger all active Crumble stacks instantly"
├─ Point Cost: 1
├─ Node Scale: 1.3
├─ Tree Position: (-100, -400)
└─ Prerequisites: ["Rage Wellspring"]
```

---

### **Step 3: Create Right Branch**

Same pattern on the right:

**Minor 5:**
```
Element 5
├─ Name: "Wound Echo"
├─ Node Type: Minor
├─ Description: "The first Attack each turn repeats for 50% effect if the target had Crumble"
├─ Point Cost: 1
├─ Tree Position: (100, -100)
└─ Prerequisites: ["Crumbling Earth Origin"]
```

**Major 3:**
```
Element 6
├─ Name: "Thrill of Agony"
├─ Node Type: Major
├─ Description: "While bleeding or burning, Crumble damage deals +50% more"
├─ Point Cost: 1
├─ Node Scale: 1.3
├─ Tree Position: (100, -200)
└─ Prerequisites: ["Wound Echo"]
```

Continue the pattern...

---

## 📐 Tree Position Guide

### **Coordinate System:**
```
       (-200, 0)  (0, 0)  (200, 0)
           │        │        │
       (-200,-100) (0,-100) (200,-100)
           │        │        │
       (-200,-200) (0,-200) (200,-200)
```

### **Position Formula:**
- **X axis:** Branch position
  - Left branch: -100 to -200
  - Center: 0
  - Right branch: 100 to 200
  
- **Y axis:** Depth in tree
  - Start: 0
  - Each step: -100 to -150 (depending on spacing)

### **Recommended Positions:**

**2-Branch Tree:**
```
Start: (0, 0)

Left Branch:
- Minor 1: (-100, -100)
- Major 1: (-100, -200)
- Minor 2: (-100, -300)
- Major 2: (-100, -400)

Right Branch:
- Minor 5: (100, -100)
- Major 3: (100, -200)
- Minor 6: (100, -300)
- Major 4: (100, -400)
```

**3-Branch Tree (if desired):**
```
Start: (0, 0)

Left: (-150, -100), (-150, -200)...
Center: (0, -100), (0, -200)...
Right: (150, -100), (150, -200)...
```

---

## 🎨 Visual Hierarchy

### **Node Sizes:**
- **Start:** 1.5x (Node Scale = 1.5)
- **Major:** 1.3x (Node Scale = 1.3)
- **Minor:** 1.0x (Node Scale = 1.0 or leave default)

### **Connection Lines:**
- Width: 3-5 pixels
- Color: White or theme color
- Drawn automatically based on prerequisites

---

## 🔧 Example: Crumbling Earth Tree

### **Complete Tree Setup:**

```
Passive Abilities (9 nodes total)

1. Crumbling Earth Origin (Start)
   - Position: (0, 0)
   - Auto-unlocked

LEFT BRANCH:
2. Blood Price (Minor)
   - Position: (-100, -100)
   - Prerequisite: Crumbling Earth Origin

3. Seismic Hunger (Major)
   - Position: (-100, -200)
   - Prerequisite: Blood Price
   - Scale: 1.3

4. Rage Wellspring (Minor)
   - Position: (-100, -300)
   - Prerequisite: Seismic Hunger

5. Final Offering (Major)
   - Position: (-100, -400)
   - Prerequisite: Rage Wellspring
   - Scale: 1.3

RIGHT BRANCH:
6. Wound Echo (Minor)
   - Position: (100, -100)
   - Prerequisite: Crumbling Earth Origin

7. Thrill of Agony (Major)
   - Position: (100, -200)
   - Prerequisite: Wound Echo
   - Scale: 1.3

8. [Another Minor] (Minor)
   - Position: (100, -300)
   - Prerequisite: Thrill of Agony

9. [Final Major] (Major)
   - Position: (100, -400)
   - Prerequisite: [Previous node]
   - Scale: 1.3
```

---

## 🔄 Auto-Generated Paths (Alternative)

If you don't want to manually set positions:

```
Ascendancy Data Settings:
├─ Use Auto Generated Paths: ✅
└─ Number Of Branches: 2

System will automatically:
- Position Start at center
- Create 2 branches
- Alternate Minor/Major pattern
- Set prerequisites automatically
```

**Just mark nodes as Minor or Major, and the system handles the rest!**

---

## 📋 Quick Setup Checklist

For each Ascendancy:

- [ ] Create 1 Start node (point cost 0, unlocked by default)
- [ ] Create 4-6 Minor nodes (small bonuses)
- [ ] Create 2-4 Major nodes (powerful bonuses)
- [ ] Set Node Type for each
- [ ] Set Tree Position for each
- [ ] Set Prerequisites to chain them
- [ ] Set Node Scale (1.3 for Major, 1.0 for Minor)
- [ ] Test tree display

---

## 🧪 Testing

1. **Create one complete tree** (9 nodes: 1 Start + 4 Minor + 4 Major)
2. **Press Play**
3. **View Ascendancy tree**
4. **Verify:**
   - ✅ Start node at center (largest)
   - ✅ 2 branches extending out
   - ✅ Minor nodes between Major nodes
   - ✅ Major nodes are larger
   - ✅ Connection lines drawn
   - ✅ Can only unlock nodes with prerequisites met

---

## 💡 Design Tips

### **Minor Node Bonuses (Simpler):**
- +10% to specific damage type
- +5% to a stat
- Small resistances
- Minor utility

### **Major Node Bonuses (Build-Defining):**
- New mechanics (Crumble, Corruption, etc.)
- Large damage multipliers (+50%+)
- Unique interactions
- Build-enabling features

### **Branching Strategy:**
- **Left branch:** Offensive/damage focus
- **Right branch:** Defensive/utility focus
- **Both meet at top:** Ultimate power nodes

---

## 🎮 Progression Flow

```
Player chooses Ascendancy
    ↓
Start node unlocked (free)
    ↓
Player earns Ascendancy points (quests/challenges)
    ↓
Unlock Minor 1 (left OR right branch)
    ↓
Unlock Major 1 (powerful bonus)
    ↓
Continue down one branch or switch
    ↓
Eventually unlock both branches fully (8 points total)
```

---

**Last Updated:** 2024-12-19
**Status:** ✅ Minor/Major Node System Implemented
**Next:** Create your Ascendancy trees with the new structure!


