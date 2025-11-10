# Ascendancy Template: Crumbling Earth (Marauder)

Example of how to set up the "Crumbling Earth" Ascendancy from your design document.

---

## 📋 Ascendancy Asset Configuration

### **Basic Info**
```
Ascendancy Name: Crumbling Earth
Base Class: Marauder
Tagline: The Destructive Rhythm of Violence
Theme Color: Brown/Earth (#8B4513)
```

### **Description**
```
The destructive rhythm of violence and decay. Damage compounds and detonates.
Every strike chips the world — and your enemies — until they shatter.
```

### **Playstyle Keywords**
- Detonation
- Self-Damage
- Area of Effect
- High Risk High Reward

---

## ⚙️ Core Mechanic

### **Crumble Mechanic**
```
Core Mechanic Name: Crumble

Core Mechanic Description:
Enemies gain Crumble stacks equal to 10% of damage taken (max 300% of your Strength). 
When Crumble expires (after 3 turns) or the enemy dies, it explodes, dealing stored damage 
to nearby enemies.
```

---

## 🔓 Passive Abilities (7 total)

### **1. Blood Price**
```
Name: Blood Price
Description: Lose 5% current HP when you Attack; deal +20% more damage this turn.
Point Cost: 1
Unlocked By Default: No
Prerequisites: None
```

### **2. Wound Echo**
```
Name: Wound Echo
Description: The first Attack each turn repeats for 50% effect if the target had Crumble.
Point Cost: 1
Unlocked By Default: No
Prerequisites: None
```

### **3. Seismic Hunger**
```
Name: Seismic Hunger
Description: Crumble explosions heal you for 10% of damage dealt.
Point Cost: 1
Unlocked By Default: No
Prerequisites: Blood Price
```

### **4. Rage Wellspring**
```
Name: Rage Wellspring
Description: Gain +1 Maximum Mana when below 25% Life.
Point Cost: 1
Unlocked By Default: No
Prerequisites: None
```

### **5. Thrill of Agony**
```
Name: Thrill of Agony
Description: While bleeding or burning, Crumble damage deals +50% more.
Point Cost: 1
Unlocked By Default: No
Prerequisites: Blood Price
```

### **6. Final Offering**
```
Name: Final Offering
Description: On death's door (≤10% Life), trigger all active Crumble stacks instantly.
Point Cost: 1
Unlocked By Default: No
Prerequisites: Thrill of Agony
```

### **7. Unstable Corruption** (From Witch ideas - example)
```
Name: Unstable Corruption
Description: Each Spell cast adds 1 Corruption (max 10). Each stack grants +5% Spell Damage 
and reduces your max HP by 2% for this combat.
Point Cost: 1
Unlocked By Default: No
Prerequisites: None
```

---

## 🃏 Signature Card

### **Earthquake (3x)**
```
Copies: 3
Card Name: Earthquake
Card Type: Attack
Mana Cost: 3 (example)

Description:
High damage, 5 target, both rows AoE.
Causes enemies hit to Crumble with 100% of damage taken, refreshes Crumble duration.

Card Data: [Leave empty until card is created]
```

---

## 📊 Unlock Requirements

```
Required Level: 15
Unlock Requirement: Complete the Labyrinth
Max Ascendancy Points: 8
```

---

## 🎮 Playstyle Summary

**Risk vs Reward:**
- Constant trading of HP for devastating detonations
- Build up Crumble on enemies
- Trigger massive AoE explosions when they die
- High-risk playstyle rewards aggressive play

**Synergies:**
- Self-damage passives amplify Crumble damage
- Low HP triggers bonus effects
- AoE explosions chain for screen-clearing potential

---

## 🎨 Visual Design Notes

**Splash Art Ideas:**
- Marauder standing in cracked earth
- Ground breaking apart beneath feet
- Enemies crumbling into rubble
- Earth/brown color palette
- Visual cracks/fissures

**Icon:**
- Cracked earth symbol
- Shattered stone
- Seismic waves

---

## 💾 Unity Inspector Setup

### **Step 1: Create Asset**
1. Right-click in `Assets/Resources/Ascendancies/`
2. Create → Dexiled → Ascendancy Data
3. Name: `MarauderCrumblingEarth`

### **Step 2: Fill Basic Info**
```
Ascendancy Name: Crumbling Earth
Base Class: Marauder
Tagline: The Destructive Rhythm of Violence
```

### **Step 3: Assign Visuals**
- Drag splash art to Splash Art field
- Drag icon to Icon field
- Set Theme Color to brown (#8B4513)

### **Step 4: Add Description**
Paste the description from above

### **Step 5: Add Keywords**
Click + to add each keyword:
- Detonation
- Self-Damage
- Area of Effect
- High Risk High Reward

### **Step 6: Setup Core Mechanic**
```
Core Mechanic Name: Crumble
Core Mechanic Description: [Paste from above]
```

### **Step 7: Add Passive Abilities**
Click + 7 times to add 7 passives
Fill each one with:
- Name
- Description  
- Point Cost (1 each)
- Unlocked By Default (No for all)
- Prerequisites (as listed above)

### **Step 8: Setup Signature Card**
```
Copies: 3
Card Name: Earthquake
Card Type: Attack
Mana Cost: 3
Description: [Paste from above]
Card Data: [Leave empty for now]
```

### **Step 9: Set Unlock Requirements**
```
Required Level: 15
Unlock Requirement: Complete the Labyrinth
Max Ascendancy Points: 8
```

### **Step 10: Save**
Ctrl+S to save the asset

---

## 🧪 Testing

1. **Press Play** in Unity
2. **Select Marauder class**
3. **Go to CharacterDisplayUI**
4. **Click "Crumbling Earth" button**
5. **Check Console** - should display:
   ```
   ━━━ Ascendancy clicked: Crumbling Earth ━━━
   Tagline: The Destructive Rhythm of Violence
   
   ━━ Core Mechanic: Crumble ━━
   [Mechanic description]
   
   ━━ Passive Abilities (7) ━━
   • Blood Price: Lose 5% current HP...
   • Wound Echo: The first Attack...
   [etc.]
   
   ━━ Signature Card ━━
   3x Earthquake (Attack - 3 Mana)
   [Card description]
   ```

---

## 📝 Checklist

- [ ] Create asset in Resources/Ascendancies/
- [ ] Set Ascendancy Name + Base Class (MUST match "Marauder")
- [ ] Add splash art and icon
- [ ] Set theme color
- [ ] Add description
- [ ] Add 4 playstyle keywords
- [ ] Set Core Mechanic (name + description)
- [ ] Add 7 Passive Abilities
- [ ] Set up Signature Card (3x Earthquake)
- [ ] Set unlock requirements
- [ ] Save asset
- [ ] Test in-game

---

**Status:** Template Ready - Fill in Unity Inspector
**Next:** Create actual card assets for Earthquake

