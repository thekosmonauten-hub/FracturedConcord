# Boss Ability Debugging Guide - Echo of Breaking

## Issue: RetaliationOnSkill Not Triggering

---

## Debug Checklist

### 1. **Check Console Logs When Boss Spawns**

You should see:
```
[Boss Ability] Registered RetaliationOnSkill on Weeper-of-Bark
```

**If NOT showing:**
- Boss ability wasn't added to `bossAbilities` list in Unity
- Open `BOSS_WeeperOfBark.asset` in Inspector
- Expand "Boss Abilities (Complex)"
- Set size to 1
- Element 0: Select "RetaliationOnSkill"

---

### 2. **Check Console Logs When Playing Card**

You should see:
```
[BossAbilityHandler] OnPlayerCardPlayed: {CardName}, Type: {Type}, Count: 1
[BossAbilityHandler] Found {N} active enemies
[BossAbilityHandler] Checking enemy: Weeper-of-Bark
[BossAbilityHandler] Weeper-of-Bark has RetaliationOnSkill: true
[BossAbilityHandler] Weeper-of-Bark registered abilities: RetaliationOnSkill
```

**If "Found 0 active enemies":**
- Boss didn't spawn correctly
- Check enemy spawner
- Verify boss is in encounter

**If "has RetaliationOnSkill: false":**
- Ability not registered
- Check step 1 above
- Verify `bossAbilities` list has RetaliationOnSkill

**If "registered abilities: " is empty:**
- `EnemyData.CreateEnemy()` didn't register abilities
- Check that boss uses `CreateEnemy()` or `CreateEnemyWithRarity()`
- Verify `bossAbilities` list is populated in asset

---

### 3. **Card Type Verification**

**CRITICAL:** Echo of Breaking only triggers on **Skill** cards, NOT Guard cards!

**Card Types:**
- `CardType.Attack` = Damage cards (Strike, Cleave, etc.) → ❌ **No retaliation**
- `CardType.Guard` = Defense cards (Block, Brace, Shield) → ❌ **No retaliation**
- `CardType.Skill` = Utility cards (Draw, Rallying Cry, Feint) → ✅ **TRIGGERS retaliation**
- `CardType.Power` = Permanent buffs → ❌ **No retaliation**
- `CardType.Aura` = Ongoing effects → ❌ **No retaliation**

---

### 4. **Skill Cards to Test With**

**Marauder Starter Deck:**
- ❌ Heavy Strike (Attack)
- ❌ Brace (Guard)
- ✅ **Rallying Cry** (Skill) ← **USE THIS**
- ✅ **War Cry** (Skill) ← **USE THIS**
- ❌ Ground Slam (Attack)
- ❌ Enduring Guard (Guard)

**Thief Class:**
- ✅ **Feint** (Skill)

**Ranger Class:**
- ✅ **Focus** (Skill)

**Apostle Class:**
- ✅ **Scripture Burn** (Skill)

---

### 5. **Expected Behavior When Playing Skill Card**

**Steps:**
1. Play a **Skill** card (e.g., Rallying Cry)
2. Console should show:
   ```
   [BossAbilityHandler] Processing Echo of Breaking for Rallying Cry (type: Skill)
   [Echo of Breaking] Weeper-of-Bark retaliates for 15 damage!
   ```
3. You should take 15 damage
4. Combat log should show: "Echo of Breaking! Skill retaliation: 15 damage."

---

## Common Issues & Solutions

### Issue 1: "No damage from retaliation"
**Possible Causes:**
- Playing wrong card type (Attack/Guard instead of Skill)
- Boss not alive
- CharacterManager not found

**Solution:**
- Verify card type with debug logs
- Check boss HP > 0
- Check console for error messages

---

### Issue 2: "Ability not registered"
**Console:** `registered abilities: ` (empty)

**Solution:**
1. Open `BOSS_WeeperOfBark.asset`
2. Find "Boss Abilities (Complex)"
3. Make sure it shows:
   ```
   Boss Abilities (Complex)
   Size: 1
   Element 0: RetaliationOnSkill
   ```

---

### Issue 3: "OnPlayerCardPlayed not called"
**Console:** No `[BossAbilityHandler]` logs at all

**Solution:**
- Check that `CombatManager.PlayCard()` is being called
- Verify event hook wasn't removed
- Make sure you're using the correct CombatManager (Combat/ folder)

---

### Issue 4: "Card is NULL"
**Console:** `Card: NULL`

**Solution:**
- Card object is null when passed
- Check card play logic
- Verify card is valid before PlayCard()

---

## Testing Script

Create this test sequence:

1. **Start encounter with Weeper-of-Bark**
2. **Check console** for ability registration
3. **Play Attack card** (Heavy Strike)
   - ❌ Should NOT trigger retaliation
   - Console: Card type shows "Attack"
4. **Play Guard card** (Brace)
   - ❌ Should NOT trigger retaliation
   - Console: Card type shows "Guard"
5. **Play Skill card** (Rallying Cry or War Cry)
   - ✅ SHOULD trigger retaliation
   - Console: Card type shows "Skill"
   - Console: "Echo of Breaking!" message
   - Take 15 damage

---

## Full Console Log Example (Success)

```
// On boss spawn:
[Boss Ability] Registered RetaliationOnSkill on Weeper-of-Bark

// When playing Skill card:
[BossAbilityHandler] OnPlayerCardPlayed: Rallying Cry, Type: Skill, Count: 1
[BossAbilityHandler] Found 1 active enemies
[BossAbilityHandler] Checking enemy: Weeper-of-Bark
[BossAbilityHandler] Weeper-of-Bark has RetaliationOnSkill: true
[BossAbilityHandler] Weeper-of-Bark registered abilities: RetaliationOnSkill
[BossAbilityHandler] Processing Echo of Breaking for Rallying Cry (type: Skill)
[Echo of Breaking] Weeper-of-Bark retaliates for 15 damage!
```

---

## Quick Fix Checklist

- [ ] Boss ability configured in Unity Editor
- [ ] `bossAbilities` list has RetaliationOnSkill
- [ ] Testing with **Skill** card (not Guard or Attack)
- [ ] Boss is alive and spawned
- [ ] Console shows "[Boss Ability] Registered..." message
- [ ] Console shows "[BossAbilityHandler] OnPlayerCardPlayed..." messages

---

## Next Steps

1. **Check console logs** - Are the debug messages showing?
2. **Try a Skill card** - Use Rallying Cry or War Cry
3. **Paste console output** - So I can diagnose the issue

Let me know what you see in the console!

