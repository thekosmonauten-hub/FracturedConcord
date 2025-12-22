# Ascendancy Tree Example: Crumbling Earth

Complete example showing how to set up a branching tree with Minor/Major nodes.

---

## 🎯 Complete Tree Structure

```
                Crumbling Earth Origin (START)
                         |
          ┌──────────────┴──────────────┐
          │                             │
      Blood Price                   Wound Echo
       (Minor)                       (Minor)
          │                             │
   Seismic Hunger                Thrill of Agony
       (Major)                       (Major)
          │                             │
   Rage Wellspring               Earth Shatter
       (Minor)                       (Minor)
          │                             │
   Final Offering               Shockwave Mastery
       (Major)                       (Major)
```

**Total Nodes:** 9
- 1 Start
- 4 Minor (Blood Price, Rage Wellspring, Wound Echo, Earth Shatter)
- 4 Major (Seismic Hunger, Final Offering, Thrill of Agony, Shockwave Mastery)

**Total Points Needed:** 8 (all except Start)

---

## 📋 Node-by-Node Setup

### **Node 1: Start (Crumbling Earth Origin)**

```
Passive Abilities → Element 0

Basic Info:
├─ Name: "Crumbling Earth Origin"
├─ Node Type: Start
├─ Description: "The ground trembles beneath your feet. Begin your path of destruction."

Visuals:
├─ Icon: [Center/Core icon]
└─ Node Scale: 1.5

Progression:
├─ Point Cost: 0
└─ Unlocked By Default: ✅

Tree Structure:
├─ Prerequisites: [] (none - it's the start)
└─ Tree Position: (0, 0)
```

---

### **LEFT BRANCH**

### **Node 2: Blood Price (Minor)**

```
Element 1

Basic Info:
├─ Name: "Blood Price"
├─ Node Type: Minor
├─ Description: "Lose 5% current HP when you Attack; deal +20% more damage this turn."

Visuals:
├─ Icon: [Blood drop icon]
└─ Node Scale: 1.0

Progression:
└─ Point Cost: 1

Tree Structure:
├─ Prerequisites: ["Crumbling Earth Origin"]
└─ Tree Position: (-100, -100)
```

---

### **Node 3: Seismic Hunger (Major)**

```
Element 2

Basic Info:
├─ Name: "Seismic Hunger"
├─ Node Type: Major ⭐
├─ Description: "Crumble explosions heal you for 10% of damage dealt."

Visuals:
├─ Icon: [Heart/healing icon]
└─ Node Scale: 1.3

Progression:
└─ Point Cost: 1

Tree Structure:
├─ Prerequisites: ["Blood Price"]
└─ Tree Position: (-100, -200)
```

---

### **Node 4: Rage Wellspring (Minor)**

```
Element 3

Basic Info:
├─ Name: "Rage Wellspring"
├─ Node Type: Minor
├─ Description: "Gain +1 Maximum Mana when below 25% Life."

Visuals:
├─ Icon: [Mana crystal icon]
└─ Node Scale: 1.0

Progression:
└─ Point Cost: 1

Tree Structure:
├─ Prerequisites: ["Seismic Hunger"]
└─ Tree Position: (-100, -300)
```

---

### **Node 5: Final Offering (Major)**

```
Element 4

Basic Info:
├─ Name: "Final Offering"
├─ Node Type: Major ⭐
├─ Description: "On death's door (≤10% Life), trigger all active Crumble stacks instantly."

Visuals:
├─ Icon: [Explosion/detonation icon]
└─ Node Scale: 1.3

Progression:
└─ Point Cost: 1

Tree Structure:
├─ Prerequisites: ["Rage Wellspring"]
└─ Tree Position: (-100, -400)
```

---

### **RIGHT BRANCH**

### **Node 6: Wound Echo (Minor)**

```
Element 5

Basic Info:
├─ Name: "Wound Echo"
├─ Node Type: Minor
├─ Description: "The first Attack each turn repeats for 50% effect if the target had Crumble."

Visuals:
├─ Icon: [Echo/repeat icon]
└─ Node Scale: 1.0

Progression:
└─ Point Cost: 1

Tree Structure:
├─ Prerequisites: ["Crumbling Earth Origin"]
└─ Tree Position: (100, -100)
```

---

### **Node 7: Thrill of Agony (Major)**

```
Element 6

Basic Info:
├─ Name: "Thrill of Agony"
├─ Node Type: Major ⭐
├─ Description: "While bleeding or burning, Crumble damage deals +50% more."

Visuals:
├─ Icon: [Flame/bleed icon]
└─ Node Scale: 1.3

Progression:
└─ Point Cost: 1

Tree Structure:
├─ Prerequisites: ["Wound Echo"]
└─ Tree Position: (100, -200)
```

---

### **Node 8: Earth Shatter (Minor)**

```
Element 7

Basic Info:
├─ Name: "Earth Shatter"
├─ Node Type: Minor
├─ Description: "Crumble explosions have 25% increased Area of Effect."

Visuals:
├─ Icon: [Shatter/AoE icon]
└─ Node Scale: 1.0

Progression:
└─ Point Cost: 1

Tree Structure:
├─ Prerequisites: ["Thrill of Agony"]
└─ Tree Position: (100, -300)
```

---

### **Node 9: Shockwave Mastery (Major)**

```
Element 8

Basic Info:
├─ Name: "Shockwave Mastery"
├─ Node Type: Major ⭐
├─ Description: "Crumble explosions can chain to other Crumbled enemies."

Visuals:
├─ Icon: [Chain/shockwave icon]
└─ Node Scale: 1.3

Progression:
└─ Point Cost: 1

Tree Structure:
├─ Prerequisites: ["Earth Shatter"]
└─ Tree Position: (100, -400)
```

---

## 🎨 Visual Result

When displayed, the tree will look like:

```
                    [START]
                   1.5x size
                       |
          ┌────────────┴────────────┐
          │                         │
       [Minor]                   [Minor]
       1.0x size                 1.0x size
          │                         │
       [MAJOR]                   [MAJOR]
       1.3x size                 1.3x size
          │                         │
       [Minor]                   [Minor]
          │                         │
       [MAJOR]                   [MAJOR]
```

**With connection lines showing the path!**

---

## ⚙️ Settings in Ascendancy Data

```
Tree Structure:
├─ Use Auto Generated Paths: ☐ (manual positioning)
└─ Number Of Branches: 2
```

**OR for automatic:**
```
Tree Structure:
├─ Use Auto Generated Paths: ✅
└─ Number Of Branches: 2
```
(Just mark nodes as Minor/Major, system positions them automatically!)

---

## 🔄 Alternative: Auto-Generated Positions

If you don't want to manually set positions:

1. **Mark node types only:**
   - 1x Start
   - 4x Minor
   - 4x Major

2. **Set prerequisites to chain them**

3. **Enable:** `Use Auto Generated Paths = ✅`

4. **System automatically:**
   - Positions Start at (0, 0)
   - Creates 2 branches
   - Alternates Minor/Major pattern
   - Draws connection lines

---

## 📊 Point Progression Example

**Player journey:**

```
Points: 0 → Start unlocked (free)
Points: 1 → Unlock Blood Price (Minor, left)
Points: 2 → Unlock Seismic Hunger (Major, left)
Points: 3 → Unlock Rage Wellspring (Minor, left)
Points: 4 → Unlock Wound Echo (Minor, right) - Switching branch!
Points: 5 → Unlock Thrill of Agony (Major, right)
Points: 6 → Unlock Earth Shatter (Minor, right)
Points: 7 → Unlock Final Offering (Major, left) - Back to left!
Points: 8 → Unlock Shockwave Mastery (Major, right) - Complete!
```

**Allows flexible pathing - don't have to complete one branch first!**

---

## 🎯 Benefits of This System

✅ **Clear Progression:** Minor → Major pattern guides players
✅ **Branching Choice:** 2+ paths to specialize
✅ **Visual Clarity:** Size indicates power (Major > Minor > Start)
✅ **Flexible Unlocking:** Can switch branches mid-way
✅ **Connection Lines:** Shows required path
✅ **Scalable:** Easy to add more nodes or branches

---

**Last Updated:** 2024-12-19
**Status:** Complete Example - Copy This Structure!


