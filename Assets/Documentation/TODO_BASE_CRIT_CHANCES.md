# TODO: Base Critical Strike Chances

**Date Created:** December 3, 2025  
**Date Completed:** December 4, 2025  
**Priority:** Medium  
**Status:** ✅ COMPLETE

---

## 📋 Requirement

### **Base Critical Strike Chances (No Weapon):**
- **Attack Cards:** 2% base crit chance
- **Spell Cards:** 5% base crit chance

**Current State:**
- ✅ Removed attribute scaling (Dex no longer gives crit)
- ❌ Base crit is 0% for all cards
- ⏳ Need to add card-type-based base crit

---

## 🔧 Implementation Plan

### **Approach 1: In DamageCalculator (Recommended)**

**File:** `Assets/Scripts/Combat/DamageCalculation.cs`

**Method:** `CalculateCardDamage(Card card, Character character, Weapon equippedWeapon)`

**Add before crit roll:**
```csharp
// Base critical strike chance by card type
float baseCritChance = 0f;
if (card.cardType == CardType.Attack)
{
    baseCritChance = 2f; // 2% for attacks
}
else if (card.cardType == CardType.Skill || ctx.isSpell)
{
    baseCritChance = 5f; // 5% for spells
}

// Add base crit to modifiers
modifiers.criticalStrikeChance += baseCritChance;
```

**Where to add:** Around line 176-180 (before the crit roll logic)

---

### **Approach 2: In Card Class**

**File:** `Assets/Scripts/Data/Cards/CardDataExtended.cs`

**Add field:**
```csharp
[Header("Critical Strike")]
public float baseCriticalChance = 0f; // Overridden by card type if 0
```

**In damage calculation:**
```csharp
float critChance = card.baseCriticalChance;
if (critChance == 0f)
{
    // Use card type defaults
    critChance = (card.cardType == CardType.Attack) ? 2f : 5f;
}
```

---

### **Approach 3: Card Type Constants**

**Create:** `Assets/Scripts/Data/Cards/CardTypeConstants.cs`

```csharp
public static class CardTypeConstants
{
    public const float ATTACK_BASE_CRIT = 2f;
    public const float SPELL_BASE_CRIT = 5f;
    public const float POWER_BASE_CRIT = 5f;
    
    public static float GetBaseCritChance(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Attack:
                return ATTACK_BASE_CRIT;
            case CardType.Skill:
            case CardType.Power:
                return SPELL_BASE_CRIT;
            default:
                return 0f;
        }
    }
}
```

**Then use in DamageCalculator:**
```csharp
modifiers.criticalStrikeChance += CardTypeConstants.GetBaseCritChance(card.cardType);
```

---

## 💡 Recommendation

**Use Approach 3** (Constants class) because:
- ✅ Centralized values (easy to balance)
- ✅ Clean separation of concerns
- ✅ Easy to extend (more card types in future)
- ✅ No magic numbers scattered in code
- ✅ Easy to test different values

---

## 🧪 Testing Checklist

After implementation:
- [ ] Attack card with no weapon → 2% crit chance
- [ ] Spell card with no weapon → 5% crit chance
- [ ] Attack card with weapon → 2% + weapon crit
- [ ] Equipment crit modifiers stack with base
- [ ] Temporary buffs stack with base
- [ ] Console shows correct crit chance calculation

---

## 📊 Expected Behavior

### **Scenario 1: No Equipment**
- Attack card: 2% crit
- Spell card: 5% crit

### **Scenario 2: With Weapon (No Crit)**
- Attack card: 2% crit (base only)
- Spell card: 5% crit (base only)

### **Scenario 3: With Weapon (+10% Crit)**
- Attack card: 2% + 10% = 12% crit
- Spell card: 5% + 10% = 15% crit

### **Scenario 4: With Equipment (+15% Total Crit)**
- Attack card: 2% + 15% = 17% crit
- Spell card: 5% + 15% = 20% crit

---

## 🔗 Related Files

**Will need to modify:**
- `DamageCalculation.cs` (add base crit logic)
- Optionally: `CardTypeConstants.cs` (new file for constants)

**Won't need to modify:**
- Character.cs ✅ (already removed Dex scaling)
- EquipmentManager.cs ✅ (already working)
- CardDataExtended.cs (unless using Approach 2)

---

## ⏰ Estimated Time

**Implementation:** 10-15 minutes  
**Testing:** 5-10 minutes  
**Total:** ~20 minutes

---

## 📝 Notes

- Current crit system uses equipment modifiers correctly
- Just missing the card-type-based baseline
- Should be quick to add once we resume
- All the infrastructure is already in place

---

**Saved for tomorrow's session!** 🌙

We've accomplished a lot today:
- ✅ Boss system (15 bosses, 30 abilities)
- ✅ Equipment system (click + drag)
- ✅ Weapon damage scaling
- ✅ Item replacement bug fix
- ✅ Crit attribute scaling removed

**Tomorrow:** Just add base crit chances and we're golden! 🎯

---

## ✅ IMPLEMENTATION COMPLETE (December 4, 2025)

**Implemented:** Full base critical strike system with card-type-based crit chances

**Files Created:**
- `CardTypeConstants.cs` - Centralized constants for card type properties

**Files Modified:**
- `CombatManager.cs` (UI) - Added proper crit calculation in PlayerAttackEnemy
- `CombatManager.cs` (Combat) - Updated card play methods to pass card
- `CardEffectProcessor.cs` - Updated 6 PlayerAttackEnemy calls
- `DamageCalculation.cs` - Removed hardcoded 5% base crit

**Implementation Details:**
See `IMPLEMENTATION_BASE_CRIT_CHANCES.md` for complete documentation.

**Ready for Testing!** 🎮

