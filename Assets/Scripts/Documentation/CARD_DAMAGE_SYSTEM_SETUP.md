# Card Damage System - Setup Guide

## 🎯 Overview

Cards now **actually deal damage** to enemies! When you click a card, it will:
1. ✨ Fly to the enemy
2. 💥 Deal calculated damage
3. 💔 Show damage numbers
4. 🗑️ Fly to discard pile

---

## 🛠️ Setup (5 minutes)

### Step 1: Add CardEffectProcessor to Scene

```
1. Create empty GameObject: "CardEffectProcessor"
2. Add Component: CardEffectProcessor
3. All references will auto-find (no manual assignment needed!)
```

### Step 2: Verify Components Exist

Make sure your scene has:
- ✓ `CombatDisplayManager` (manages enemies)
- ✓ `CombatDeckManager` (manages cards)
- ✓ `CardRuntimeManager` (displays cards)
- ✓ `CombatAnimationManager` (animations)
- ✓ `CardEffectProcessor` (NEW - applies damage!)

### Step 3: Setup Enemy Displays

Your `CombatDisplayManager` should already have this, but verify:
```
CombatDisplayManager
├── Create Test Enemies: ✓ (checked)
├── Test Enemy Count: 2
└── Enemy Displays: [List of EnemyCombatDisplay components]
```

---

## 🎮 How It Works

### When You Click a Card:

```
1. Card click detected
   ↓
2. GetTargetEnemy() → Gets first available enemy
   ↓
3. Card flies to enemy position
   ↓
4. CardEffectProcessor.ApplyCardToEnemy()
   ↓
5. Calculate damage (base + weapon + attributes)
   ↓
6. Enemy.TakeDamage(calculatedDamage)
   ↓
7. Show damage number animation
   ↓
8. Check if enemy is defeated
   ↓
9. Card flies to discard pile
```

---

## 💥 Damage Calculation

### Attack Card Example: "Heavy Strike"
```json
{
  "cardName": "Heavy Strike",
  "baseDamage": 12,
  "damageScaling": {
    "strengthScaling": 1.5
  },
  "weaponScaling": {
    "scalesWithMeleeWeapon": true
  }
}
```

**Total Damage:**
```
Base:     12
Weapon:   +10 (if you have a weapon with 10 damage)
STR:      +15 (if you have 10 STR × 1.5 scaling)
───────────
Total:    37 damage!
```

---

## 📊 Console Output (What You'll See)

### When Drawing Cards:
```
TEST MODE: Loading Marauder starter deck...
✓ Loaded Marauder deck: 18 cards
Drawing card #1: Heavy Strike
✓ Card GameObject created: PooledCard_0
...
```

### When Playing a Card:
```
Card clicked! Hand index: 0, Target pos: (1200, 600, 0)
Playing card: Heavy Strike
═══ Applying Heavy Strike to Goblin Scout ═══
  • Melee weapon bonus: +10
  • Attribute scaling: +15.0
  ⚔️ Dealt 37 damage to Goblin Scout
  💔 Goblin Scout HP: 0/30
💀 Goblin Scout has been defeated!
  → Card effect triggered: Heavy Strike
  → Animating Heavy Strike to discard pile...
  → Heavy Strike discarded!
```

---

## 🎨 Card Types Supported

### Attack Cards (Deal Damage)
```
CardType.Attack
  → Deals damage to enemy
  → Scales with weapon + attributes
  → Shows damage numbers
```

### Guard Cards (Block)
```
CardType.Guard
  → Adds guard to player
  → Currently logs only (guard system TODO)
```

### Skill Cards (Hybrid)
```
CardType.Skill
  → Can have damage AND guard
  → Can have special effects
```

### Power Cards (Buffs)
```
CardType.Power
  → Buffs player stats
  → Currently logs only (buff system TODO)
```

---

## 🧪 Testing

### Quick Test:
1. **Check "Test Load Marauder Deck On Start"** on CombatDeckManager
2. **Press Play** ▶
3. **Wait for cards to draw** (5 cards appear)
4. **Click any card** 
5. **Watch:**
   - Card flies to enemy ✓
   - Damage number appears ✓
   - Enemy HP decreases ✓
   - Card flies to discard ✓

### Expected Console:
```
Heavy Strike
═══ Applying Heavy Strike to Goblin Scout ═══
  ⚔️ Dealt 12 damage to Goblin Scout
  💔 Goblin Scout HP: 18/30
```

---

## 🎯 Enemy Targeting

### Current System (Auto-Target)
- Automatically targets **first available enemy**
- No targeting UI needed yet
- Perfect for testing!

### Future (TODO - Enemy Targeting UI)
```
1. Click card
2. Enemy panels highlight
3. Click enemy to target
4. Card flies to selected enemy
```

**For now:** Cards automatically target the first enemy!

---

## 🔧 Customization

### Change Damage Calculation

Edit `CardEffectProcessor.cs` → `CalculateDamage()`:
```csharp
// Example: Add critical hit chance
if (Random.Range(0f, 1f) < 0.1f) // 10% crit
{
    baseDamage *= 2f;
    Debug.Log("    • CRITICAL HIT!");
}
```

### Change Effect Duration

Edit `CombatDeckManager.cs` → `PlayCard()`:
```csharp
float effectDuration = 0.3f; // Time card stays at enemy
                             // Increase for more impact!
```

### Add Sound Effects

Hook into events in `CardEffectProcessor.cs`:
```csharp
private void ApplyAttackCard(...)
{
    // Play hit sound
    AudioManager.PlaySound("CardHit");
    
    // Apply damage
    targetEnemy.TakeDamage(totalDamage);
}
```

---

## 🐛 Troubleshooting

### "Cannot apply card: No target enemy"
- ✓ Check `CombatDisplayManager` exists
- ✓ Check "Create Test Enemies" is checked
- ✓ Check Test Enemy Count > 0

### "CardEffectProcessor is NULL"
- ✓ Create CardEffectProcessor GameObject in scene
- ✓ Add CardEffectProcessor component
- ✓ Restart play mode

### Damage number doesn't show
- ✓ Check `CombatAnimationManager` exists
- ✓ Check damage number prefab is assigned
- ✓ Check damage number pool is initialized

### Card doesn't fly to enemy
- ✓ Check enemy displays have RectTransform
- ✓ Check enemy displays are positioned on screen
- ✓ Check GetEnemyScreenPosition() is finding the display

---

## 📋 Scene Hierarchy Checklist

```
Combat Scene
├── CardEffectProcessor ✓ NEW!
├── CombatDeckManager ✓
├── CombatDisplayManager ✓
│   └── Has test enemies setup
├── CardRuntimeManager ✓
├── CombatAnimationManager ✓
├── DeckPilePosition ✓
├── DiscardPilePosition ✓
├── Enemy Displays ✓
│   ├── EnemyDisplay_1
│   ├── EnemyDisplay_2
│   └── EnemyDisplay_3
└── Player Display ✓
```

---

## ✅ Success Criteria

After setup, you should see:

1. ✅ Cards draw from deck pile
2. ✅ Cards appear in hand with hover effects
3. ✅ Click card → flies to enemy
4. ✅ Damage number appears on enemy
5. ✅ Enemy HP decreases
6. ✅ Console shows damage calculation
7. ✅ Card flies to discard pile
8. ✅ Card disappears (returns to pool)
9. ✅ If enemy HP = 0, "defeated" message shows
10. ✅ Next card click targets next available enemy

---

## 🚀 Next Steps (TODO)

### Immediate:
- [ ] Add guard/block system to Character
- [ ] Add buff/debuff system
- [ ] Enemy selection/targeting UI
- [ ] Card effect icons/particles

### Future:
- [ ] Multi-target cards (AoE)
- [ ] Status effects (poison, burn, etc.)
- [ ] Card combos
- [ ] Enemy AI reactions
- [ ] Victory/defeat conditions

---

## 💡 Pro Tips

### Want to see damage values?
Enable detailed logs:
```
Select CardEffectProcessor
Inspector → Settings
  ✓ Show Detailed Logs
```

### Want different starting enemies?
Edit `CombatDisplayManager.CreateTestEnemies()`:
```csharp
new Enemy("Dragon", 100, 20),
new Enemy("Slime", 10, 2),
```

### Want to test specific cards?
Use context menu:
```
Right-click CombatDeckManager
  → Load Marauder Deck
  → Draw Initial Hand
Then click cards to test!
```

---

**Your cards now ACTUALLY deal damage! Test it out!** 🎴💥

