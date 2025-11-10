# Complete Ascendancy System Guide

How to implement all 18 Ascendancies from your design document.

---

## 🎯 System Overview

### **What's Implemented**

✅ **Core Ascendancy System:**
- `AscendancyData.cs` - Fully featured ScriptableObject system
- `AscendancyDatabase.cs` - Loads and manages all Ascendancies
- `AscendancyButton.cs` - UI button for displaying Ascendancies
- `CharacterAscendancyProgress.cs` - Tracks progression and unlocks

✅ **Progression System:**
- Ascendancy Point tracking
- Individual passive unlock system
- Prerequisite requirements
- Signature card unlocking

✅ **UI Integration:**
- CharacterDisplayUI shows 3 Ascendancies per class
- Splash art display
- Click to view detailed info

---

## 📊 Your Design Document Structure

Each Ascendancy in your document has:

1. **Theme & Fantasy Hook** → `description` + `tagline`
2. **Core Mechanic** → `coreMechanicName` + `coreMechanicDescription`
3. **Passive Abilities (5-8)** → `passiveAbilities` list
4. **Specific Card Unlock** → `signatureCard`
5. **Playstyle Summary** → `playstyleKeywords`

---

## 📝 All 18 Ascendancies To Create

### **⚔️ MARAUDER (3)**
1. **Crumbling Earth** - Detonation & Self-Damage
   - Core: Crumble mechanic
   - Card: 3x Earthquake

2. **Iron Vanguard / Bastion of Tolerance** - Tank & Retaliation
   - Core: Tolerance mechanic  
   - Card: 3x Indomitable Roar

3. **Disciple of War** - Tactical & Combo Flow
   - Core: Battle Rhythm mechanic
   - Card: 3x Drums of War

### **🔮 WITCH (3)**
4. **Herald of the Abyss** - Chaos & Life as Fuel
   - Core: Corruption mechanic
   - Card: 3x Chaotic Eruption

5. **Temporal Savant** - Time Control & Foresight
   - Core: Delay mechanic
   - Card: 3x Rewind

6. **Architect of Entropy** - Deck Manipulation & Discard
   - Core: Entropy mechanic
   - Card: 3x Collapse

### **🏹 RANGER (3)**
7. **Marksman** - Precision & Crits
   - Core: Chain mechanic
   - Card: 3x Ricochet

8. **Agitated Duelist** - Melee Speed & Momentum
   - Core: Agitate mechanic
   - Card: 3x Mantra of Calm

9. **Effigy Keeper** - Totems & Adaptability
   - Core: Effigy mechanic
   - Card: 3x Nature's Wrath

### **🥊 BRAWLER (3)**
10. **Warmonger** - Momentum Predator
    - Core: Predatory Momentum
    - Card: Crimson Lunge

11. **Iron Duelist** - Unshakable Defender
    - Core: Bolster mechanic
    - Card: Stand Your Ground

12. **Bloodguard** - Dual-Wield Bleed
    - Core: Hemorrhage Flow
    - Card: Reprisal Cleave

### **📖 APOSTLE (3)**
13. **Battlemind** - Arcane Bulwark
    - Core: Mana Shielding
    - Card: Arcane Retribution

14. **Elemental Arbiter** - Crit & Ailments
    - Core: Fluid Motion
    - Card: Tri-Element Nova

15. **Sanctifier** - Divine Auras
    - Core: Radiant Auras
    - Card: Divine Convergence

### **🗡️ THIEF (3)**
16. **Toxicologist** - Poison & Crits
    - Core: Potential mechanic
    - Card: Venom Gambit

17. **Choreographer** - Conversion & Evasion
    - Core: Afterimage mechanic
    - Card: Phase Reversal

18. **Mastermind** - Preparation & Planning
    - Core: Preparation & Unleash
    - Card: Perfect Setup

---

## 🛠️ Quick Creation Workflow

### **Per Ascendancy (15-20 min each):**

1. **Create Asset** (1 min)
   - Right-click Resources/Ascendancies/
   - Create → Dexiled → Ascendancy Data
   - Name: `{Class}{AscendancyName}` (e.g., `MarauderCrumblingEarth`)

2. **Basic Info** (2 min)
   - Ascendancy Name
   - Base Class (MUST match exactly!)
   - Tagline (from "Fantasy Hook")
   - Splash art + icon
   - Theme color

3. **Description & Keywords** (2 min)
   - Copy "Theme" text to Description
   - Add 3-4 playstyle keywords

4. **Core Mechanic** (3 min)
   - Name: e.g., "Crumble"
   - Description: Copy from "Mechanics" section

5. **Passive Abilities** (8 min)
   - Add each passive from document
   - Name + Description from document
   - Point Cost: 1 (for all)
   - Set prerequisites if logical

6. **Signature Card** (4 min)
   - Copies: 3 (usually)
   - Name + Type + Cost
   - Description from "Specific Card Unlock"
   - Leave Card Data empty for now

7. **Save & Test** (1 min)
   - Ctrl+S
   - Test in-game if ready

---

## 🎨 Asset Creation Tips

### **Naming Convention:**
```
{BaseClass}{AscendancyName}.asset

Examples:
- MarauderCrumblingEarth.asset
- WitchHeraldOfTheAbyss.asset
- RangerMarksman.asset
- BrawlerWarmonger.asset
- ApostleBattlemind.asset
- ThiefToxicologist.asset
```

### **Base Class Names (Must Match Exactly!):**
- `Marauder`
- `Witch`
- `Ranger`
- `Brawler`
- `Apostle`
- `Thief`

### **Theme Colors by Archetype:**
- **Marauder:** Browns, reds, earth tones
- **Witch:** Purples, dark colors, chaos
- **Ranger:** Greens, natural colors
- **Brawler:** Blues, metallics
- **Apostle:** Golds, holy colors
- **Thief:** Dark greens, poisons, shadows

---

## 📋 Bulk Creation Checklist

### **Phase 1: Marauder (3 Ascendancies)**
- [ ] Crumbling Earth
- [ ] Iron Vanguard
- [ ] Disciple of War

### **Phase 2: Witch (3 Ascendancies)**
- [ ] Herald of the Abyss
- [ ] Temporal Savant
- [ ] Architect of Entropy

### **Phase 3: Ranger (3 Ascendancies)**
- [ ] Marksman
- [ ] Agitated Duelist
- [ ] Effigy Keeper

### **Phase 4: Brawler (3 Ascendancies)**
- [ ] Warmonger
- [ ] Iron Duelist
- [ ] Bloodguard

### **Phase 5: Apostle (3 Ascendancies)**
- [ ] Battlemind
- [ ] Elemental Arbiter
- [ ] Sanctifier

### **Phase 6: Thief (3 Ascendancies)**
- [ ] Toxicologist
- [ ] Choreographer
- [ ] Mastermind

---

## 🧪 Testing Each Class

1. **Create 3 Ascendancies for one class**
2. **Press Play**
3. **Select that class**
4. **Go to CharacterDisplayUI**
5. **Verify:** 3 buttons appear with splash art
6. **Click each** and check Console for complete info

### **Expected Console Output:**
```
━━━ Ascendancy clicked: Crumbling Earth ━━━
Tagline: The Destructive Rhythm of Violence
Description: [Your description]
Keywords: Detonation • Self-Damage • Area of Effect • High Risk

━━ Core Mechanic: Crumble ━━
Enemies gain Crumble stacks equal to 10% of damage taken...

━━ Passive Abilities (7) ━━
• Blood Price: Lose 5% current HP when you Attack...
• Wound Echo: The first Attack each turn repeats...
[etc.]

━━ Signature Card ━━
3x Earthquake (Attack - 3 Mana)
High damage, 5 target, both rows AoE...

Unlock Requirements: Level 15
Max Ascendancy Points: 8
```

---

## 🔮 Future: Implementing Mechanics

Once assets are created, implement the actual mechanics:

### **Step 1: Create Signature Cards**
- Create CardDataExtended assets for each signature card
- Assign to `signatureCard.cardData` field

### **Step 2: Implement Core Mechanics**
Example for Crumble:
```csharp
public class CrumbleMechanic : MonoBehaviour
{
    Dictionary<Enemy, float> crumbleStacks = new Dictionary<Enemy, float>();
    
    public void ApplyCrumble(Enemy enemy, float damage)
    {
        float crumbleAmount = damage * 0.1f; // 10% of damage
        float maxCrumble = characterStats.strength * 3f; // 300% of STR
        
        if (!crumbleStacks.ContainsKey(enemy))
            crumbleStacks[enemy] = 0;
        
        crumbleStacks[enemy] = Mathf.Min(crumbleStacks[enemy] + crumbleAmount, maxCrumble);
    }
    
    public void TriggerCrumbleExplosion(Enemy enemy)
    {
        if (crumbleStacks.ContainsKey(enemy))
        {
            float damage = crumbleStacks[enemy];
            // Deal AoE damage to nearby enemies
            DealAoEDamage(enemy.position, damage);
            crumbleStacks.Remove(enemy);
        }
    }
}
```

### **Step 3: Implement Passive Effects**
Check `CharacterAscendancyProgress` for unlocked passives:
```csharp
if (character.ascendancyProgress.IsPassiveUnlocked("Blood Price"))
{
    // Apply Blood Price effect
    character.LosePercentHP(0.05f);
    damageModifier *= 1.2f;
}
```

---

## 💡 Pro Tips

1. **Start with one class** - Create all 3, test thoroughly
2. **Copy-paste descriptions** from your document
3. **Placeholder splash art** is fine initially
4. **Focus on data first** - implement mechanics later
5. **Use templates** - Copy MarauderCrumblingEarth and modify

---

## 🎯 Current Implementation Status

**✅ Completed:**
- Full data structure
- UI display system
- Progression tracking
- Click-to-view details

**📝 Your Next Steps:**
1. Create 18 AscendancyData assets (3-4 hours)
2. Add splash art when available
3. Create signature card assets (later)
4. Implement core mechanics (in-game systems)
5. Implement passive effects (in-game systems)

**⏱️ Estimated Time:**
- Assets creation: 3-4 hours (all 18)
- Splash art creation: Separate effort
- Mechanic implementation: Per mechanic as needed
- Passive implementation: Per passive as needed

---

**Last Updated:** 2024-12-19
**Status:** ✅ System Complete - Ready for Asset Creation
**Document Reference:** AscendancyIdeas.md

