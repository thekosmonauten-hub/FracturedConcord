# Card Damage Debugging Guide

## 🔍 How to Debug Damage Issues

If cards aren't dealing damage, follow this systematic debugging process.

---

## 🎯 **Step-by-Step Debugging:**

### **Step 1: Play a Card and Read Console**

```
1. Press Play ▶
2. Click an enemy panel (to target)
3. Click an ATTACK card (like "Heavy Strike")
4. Check console output
```

---

## 📊 **Expected Console Output (Working):**

```
Card clicked: Heavy Strike (Index: 0)
Playing card: Heavy Strike
  Animation manager found. Flying card to enemy...
  Card reached target! Applying effects...
  
╔══════════════════════════════════╗
║ APPLYING CARD EFFECT DEBUG      ║
╚══════════════════════════════════╝
✓ Card: Heavy Strike (Type: Attack)
✓ Target: CoconutCrab
✓ Target HP BEFORE: 50/50
→ Attack card detected!
  Base damage: 12
  Total calculated damage: 33
  Calling Enemy.TakeDamage(33)...
  ⚔️ Dealt 33 damage to CoconutCrab
  💔 Target HP AFTER: 17/50  ← HP decreased!
  → Updated EnemyPanel_1 health display
```

---

## 🐛 **Common Issues:**

### **Issue 1: No Debug Box Appears**
```
Console shows card clicked and animation...
(But NO "APPLYING CARD EFFECT DEBUG" box)
```

**Diagnosis:** `CardEffectProcessor.ApplyCardToEnemy()` not being called!

**Causes:**
- CardEffectProcessor doesn't exist in scene
- cardEffectProcessor reference is null
- Animation callback not firing

**Fix:**
```
1. Check: Does CardEffectProcessor GameObject exist?
2. Select CombatDeckManager → Inspector
3. References → Card Effect Processor: Should be assigned
4. If null, create CardEffectProcessor GameObject
```

---

### **Issue 2: "Target enemy is NULL!"**
```
╔══════════════════════════════════╗
✗ Target enemy is NULL! Cannot apply!
```

**Diagnosis:** No enemy targeted or enemy died

**Causes:**
- No EnemyTargetingManager in scene
- Enemy defeated but not re-targeted
- CombatDisplayManager has no active enemies

**Fix:**
```
1. Check: Does EnemyTargetingManager GameObject exist?
2. Check: Do enemy panels show enemies?
3. Right-click CombatDisplayManager → "Check Enemy Setup"
4. Should show active enemies
```

---

### **Issue 3: "Card is NULL!"**
```
╔══════════════════════════════════╗
✗ Card is NULL! Cannot apply!
```

**Diagnosis:** Card data not passed correctly

**Causes:**
- Card object doesn't have card data
- Index mismatch in hand

**Fix:**
```
Check console earlier in the log:
  Should show: "Card clicked: Heavy Strike"
  If missing, click handler not working
```

---

### **Issue 4: Wrong Card Type**
```
✓ Card: Brace (Type: Guard)
→ Skill effect applied: Brace
(No damage dealt - it's a Guard card!)
```

**Diagnosis:** Not an attack card!

**Fix:**
- Play an Attack card instead (Heavy Strike, Basic Strike, etc.)
- Guard cards don't deal damage

---

### **Issue 5: HP Changes But Bar Doesn't Update**
```
💔 Target HP AFTER: 17/50  ← HP changed in data!
(But visual health bar stays full)
```

**Diagnosis:** Enemy display not refreshing

**Causes:**
- EnemyCombatDisplay.RefreshDisplay() not called
- Health bar component not assigned
- Health bar not listening to enemy changes

**Fix:**
```
1. Check if you see: "→ Updated EnemyPanel_X health display"
2. If missing, enemy display not found
3. Select enemy panel → Check EnemyCombatDisplay references
4. Health Slider should be assigned
```

---

### **Issue 6: Animation Never Reaches Target**
```
Animation manager found. Flying card to enemy...
(Never shows "Card reached target!")
```

**Diagnosis:** AnimateCardPlay callback not firing

**Causes:**
- Card destroyed mid-animation
- Target position invalid
- LeanTween issue

**Fix:**
```
1. Check target position in log
2. Should be on-screen coordinates
3. If (0,0,0) or weird values, targeting broken
```

---

## ✅ **Damage Flow Checklist:**

Check each step appears in console:

- [ ] "Card clicked: Heavy Strike"
- [ ] "Playing card: Heavy Strike"
- [ ] "Animation manager found"
- [ ] "Card reached target!"
- [ ] "APPLYING CARD EFFECT DEBUG" box
- [ ] "✓ Card: Heavy Strike (Type: Attack)"
- [ ] "✓ Target: CoconutCrab"
- [ ] "✓ Target HP BEFORE: 50/50"
- [ ] "→ Attack card detected!"
- [ ] "Calling Enemy.TakeDamage(X)..."
- [ ] "⚔️ Dealt X damage"
- [ ] "💔 Target HP AFTER: X/50" ← Lower than before!
- [ ] "→ Updated EnemyPanel health display"

**If ANY step is missing, that's where it's breaking!**

---

## 🔧 **Quick Diagnostic Commands:**

### **Check Scene Setup:**
```
Right-click CombatDeckManager → "Check Discard Setup"
Right-click CombatDisplayManager → "Check Enemy Setup"
Right-click EnemyTargetingManager → "Show Current Target"
```

### **Check Enemy State:**
```
While playing, select an enemy panel
Check Inspector:
  EnemyCombatDisplay → Current Enemy
  Should show HP values
```

---

## 🎮 **Complete Test:**

```
1. Create CardEffectProcessor (if missing)
2. Create EnemyTargetingManager (if missing)
3. Press Play
4. Click enemy panel (yellow outline appears)
5. Click "Heavy Strike" card
6. Watch console for full debug output
7. Copy and send me ANY error or missing step!
```

---

**Play a card now and send me the FULL console output!** The enhanced debug logs will show exactly where the damage flow is breaking! 🔍✨







