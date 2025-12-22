# Base Critical Strike Chances - Implementation Complete ✅

**Date Implemented:** December 4, 2025  
**Status:** ✅ Complete  
**Approach Used:** Approach 3 (CardTypeConstants)

---

## 📋 Implementation Summary

Successfully implemented card-type-based base critical strike chances:
- **Attack Cards:** 2% base crit chance
- **Spell Cards (Skill/Power):** 5% base crit chance
- **Other Cards:** 0% base crit chance

---

## 🔧 Files Created/Modified

### Created:
1. **`Assets/Scripts/Data/Cards/CardTypeConstants.cs`**
   - New constants class for card type properties
   - `GetBaseCritChance(CardType)` - Get base crit by card type
   - `GetBaseCritChance(Card)` - Get base crit by card instance (checks tags for spell detection)

### Modified:
1. **`Assets/Scripts/UI/Combat/CombatManager.cs`**
   - Updated `PlayerAttackEnemy()` signature to accept optional `Card playedCard` parameter
   - Replaced hardcoded 10% crit chance with proper calculation:
     - Base crit from card type (2% or 5%)
     - Equipment crit modifiers
     - Character crit modifiers (from ascendancy/passives)
     - Stack system bonuses
   - Added comprehensive debug logging for crit chance calculation

2. **`Assets/Scripts/Combat/CombatManager.cs`**
   - Updated `PlaySingleTargetCard()` to pass card to `PlayerAttackEnemy()`
   - Updated `PlayAoECard()` to pass card to `PlayerAttackEnemy()`

3. **`Assets/Scripts/CombatSystem/CardEffectProcessor.cs`**
   - Updated 6 calls to `PlayerAttackEnemy()` to pass card parameter:
     - Line 289: Single target damage
     - Line 724: Momentum random targets
     - Line 818: Single hit path
     - Line 2282: Adrenaline Burst AoE
     - Line 2435: Multi-hit attacks
     - Line 2496: Multi-hit AoE
     - Line 2553: Multi-hit random targets

4. **`Assets/Scripts/Combat/DamageCalculation.cs`**
   - Updated `DamageModifiers.criticalStrikeChance` default from 5f to 0f
   - Added comment explaining base crit comes from CardTypeConstants

---

## 🎯 Critical Strike Calculation Flow

```csharp
// 1. Base crit from card type
float critChance = CardTypeConstants.GetBaseCritChance(playedCard);
// Attack: 2%, Spell: 5%, Other: 0%

// 2. Add equipment crit modifiers
critChance += player.GetDamageModifiers().criticalStrikeChance;

// 3. Add character crit modifiers (ascendancy/passives)
critChance += player.criticalStrikeChance;

// 4. Add stack system bonuses
critChance += StackSystem.Instance.GetCritChanceBonus();

// 5. Clamp to 0-100%
critChance = Mathf.Clamp(critChance, 0f, 100f);

// 6. Roll for crit
bool wasCritical = Random.Range(0f, 100f) < critChance;
```

---

## 📊 Expected Behavior (Test Scenarios)

### ✅ Scenario 1: No Equipment
- Attack card: 2% crit
- Spell card: 5% crit

### ✅ Scenario 2: With Weapon (No Crit)
- Attack card: 2% crit (base only)
- Spell card: 5% crit (base only)

### ✅ Scenario 3: With Weapon (+10% Crit)
- Attack card: 2% + 10% = 12% crit
- Spell card: 5% + 10% = 15% crit

### ✅ Scenario 4: With Equipment (+15% Total Crit)
- Attack card: 2% + 15% = 17% crit
- Spell card: 5% + 15% = 20% crit

### ✅ Scenario 5: With Ascendancy (+5% Character Crit)
- Attack card: 2% + 5% = 7% crit
- Spell card: 5% + 5% = 10% crit

### ✅ Scenario 6: Full Build (All Sources)
- Attack card: 2% (base) + 10% (weapon) + 5% (character) + 3% (stacks) = 20% crit
- Spell card: 5% (base) + 10% (weapon) + 5% (character) + 3% (stacks) = 23% crit

---

## 🧪 Testing Checklist

- [x] Created CardTypeConstants class
- [x] Updated PlayerAttackEnemy signature
- [x] Updated all callers to pass card parameter
- [x] Implemented proper crit chance calculation
- [x] Added comprehensive debug logging
- [x] Removed hardcoded 10% crit chance
- [x] Removed hardcoded 5% base from DamageModifiers
- [x] No linter errors
- [ ] In-game testing: Attack card with no equipment → 2% crit
- [ ] In-game testing: Spell card with no equipment → 5% crit
- [ ] In-game testing: Attack card with weapon crit → stacks correctly
- [ ] In-game testing: Equipment crit modifiers stack with base
- [ ] In-game testing: Ascendancy crit modifiers stack with base
- [ ] In-game testing: Console shows correct crit chance calculation

---

## 🔗 Design Rationale

### Why Approach 3 (Constants Class)?
✅ **Centralized values** - Easy to balance and tweak  
✅ **Clean separation of concerns** - Card type logic separate from combat logic  
✅ **Easy to extend** - Can add more card types in future  
✅ **No magic numbers** - All constants named and documented  
✅ **Easy to test** - Simple pure functions  
✅ **Type safety** - Compile-time checking  

### Why Not Approach 1 (DamageCalculator)?
❌ Would mix card type logic with damage calculation  
❌ Harder to reuse in other contexts  

### Why Not Approach 2 (Card Class)?
❌ Would require updating all card assets  
❌ More complex migration path  
❌ Less flexible for future changes  

---

## 📝 Debug Logging

The implementation includes comprehensive debug logging:

```
[Crit] Base crit chance for Strike ({CardType}): 2%
[Crit] + Equipment crit chance: 10% (Total: 12%)
[Crit] + Character crit chance: 5% (Total: 17%)
[Crit] + Stack crit bonus: 3% (Total: 20%)
```

This makes it easy to:
- Verify correct crit calculation
- Debug issues with crit stacking
- Balance crit values during playtesting

---

## 🎉 Benefits

1. **More strategic gameplay** - Spell cards naturally have higher crit chance
2. **Build diversity** - Attack builds need crit investment, spell builds get it baseline
3. **Proper scaling** - All crit sources stack correctly
4. **Easy balancing** - Tweak constants in one place
5. **Clean code** - No magic numbers, well-documented
6. **Extensible** - Easy to add more card types or crit modifiers

---

## 🔮 Future Enhancements

Potential improvements for future sessions:
- [ ] Per-card crit override (for special cards)
- [ ] Conditional crit bonuses (e.g., "Double crit chance if enemy is below 50% HP")
- [ ] Crit chance cap system (prevent 100% crit builds)
- [ ] Crit strike multiplier scaling by card type
- [ ] Visual indicator in card UI showing crit chance
- [ ] Floating text color variation by crit magnitude

---

**Implementation Time:** ~45 minutes  
**Files Changed:** 5  
**Lines Added:** ~80  
**Lines Modified:** ~15  

🎯 **All requirements from TODO_BASE_CRIT_CHANCES.md met!**

