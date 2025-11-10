# 🐛 Preparation System - Bug Fixes Applied

## Issues Fixed

All compilation errors have been resolved! Here's what was fixed:

---

### **1. Card to CardDataExtended Conversion Error**

**Error:**
```
Cannot convert type 'Card' to 'CardDataExtended' via a reference conversion
```

**Problem:**
- `Card` is a `[Serializable]` class used for gameplay
- `CardDataExtended` is a `ScriptableObject` used for card definitions
- These are separate types with no inheritance relationship

**Solution:**
Added a source reference field to the `Card` class:

```csharp
// In Card.cs
[Header("Source Reference")]
[System.NonSerialized]
public CardDataExtended sourceCardData;
```

Updated `CardDataExtended.ToCard()` to set this reference:

```csharp
// In CardDataExtended.cs
public Card ToCard()
{
    var card = new Card { /* ... */ };
    card.sourceCardData = this; // Set reference
    return card;
}
```

Updated CombatManager to use the reference:

```csharp
// In CombatManager.cs
CardDataExtended extendedCard = card.sourceCardData; // Instead of casting
```

---

### **2. baseDamage Field Not Found**

**Error:**
```
CardDataExtended does not contain a definition for 'baseDamage'
```

**Problem:**
- `CardDataExtended` inherits from `CardData`
- `CardData` has a field named `damage`, not `baseDamage`

**Solution:**
Changed all references from `baseDamage` to `damage`:

```csharp
// In PreparedCard.cs
storedBaseDamage = card.damage; // Was: card.baseDamage
```

---

### **3. GetSpellPower() Method Not Found**

**Error:**
```
Character does not contain a definition for 'GetSpellPower'
```

**Problem:**
- `Character` class doesn't have a `GetSpellPower()` method
- Spell power is calculated from intelligence

**Solution:**
Calculate spell power directly from intelligence:

```csharp
// In PreparedCard.cs & PreparationManager.cs
float spellPower = player.intelligence * 0.1f; // 10% of INT
```

---

### **4. SpendMana() Method Not Found**

**Error:**
```
Character does not contain a definition for 'SpendMana'
```

**Problem:**
- Method is named `UseMana()`, not `SpendMana()`
- `UseMana()` returns `bool` (success/failure)

**Solution:**
Changed to use `UseMana()` with proper error handling:

```csharp
// In PreparationManager.cs
if (!player.UseMana(energyCost))
{
    Debug.LogWarning($"Failed to spend {energyCost} mana!");
    return false;
}
```

---

### **5. CombatManager.Instance Not Found**

**Error:**
```
CombatManager does not contain a definition for 'Instance'
```

**Problem:**
- `CombatManager` is not a singleton
- Need to find it using `FindFirstObjectByType<>()`

**Solution:**
Use Unity's object finding methods:

```csharp
// In PreparationManager.cs
var combatManager = FindFirstObjectByType<CombatManager>();
var combatDisplay = FindFirstObjectByType<CombatDisplayManager>();

if (combatDisplay != null)
{
    // Use CombatDisplayManager
}
else if (combatManager != null)
{
    // Fallback to CombatManager
}
```

---

### **6. MomentumManager Not Found**

**Error:**
```
The name 'MomentumManager' does not exist in the current context
```

**Problem:**
- `MomentumManager` was referenced but doesn't exist in the project
- This was an example synergy that may not be implemented yet

**Solution:**
Commented out the MomentumManager code with instructions:

```csharp
// In PreparationManager.cs
// Momentum synergy example (optional)
// Uncomment if you have MomentumManager:
/*
var momentumManager = FindFirstObjectByType<MomentumManager>();
if (momentumManager != null && card.tags.Contains("Momentum"))
{
    momentumManager.GainMomentum(prepared.turnsPrepared);
}
*/
```

---

## Verification

All errors fixed! Run these checks:

1. **Compile**: No errors in Console
2. **Test Preparation**: Play a card with "Prepare" tag
3. **Test Unleash**: Click prepared card to unleash
4. **Test Stats**: Verify INT/DEX/STR bonuses apply

---

## Key Architectural Changes

### **Card ↔ CardDataExtended Bridge**

The system now properly bridges the two card systems:

```
CardDataExtended (ScriptableObject)
    ↓ ToCard()
    ↓ sets sourceCardData reference
Card (Serializable class)
    ↓ Used in gameplay
    ↓ sourceCardData field
    ↓ references back to
CardDataExtended (for preparation fields)
```

This allows:
- Gameplay to use `Card` objects (fast, serializable)
- Preparation system to access `CardDataExtended` fields (via `sourceCardData`)
- No breaking changes to existing code

---

## Future Improvements

Consider:
1. **Eliminate Card class entirely**: Use CardDataExtended everywhere (long-term migration)
2. **Add MomentumManager**: Implement if you want momentum synergies
3. **Spell Power System**: Create a proper spell power calculation method in Character
4. **Better Enemy Targeting**: Integrate with your targeting system

---

## Testing Checklist

Before deploying:

- [ ] Cards with "Prepare" tag prepare correctly
- [ ] Turn counter increments each turn
- [ ] Manual unleash costs correct energy
- [ ] Unleash cards trigger for free
- [ ] DEX stat speeds up charging (30/50+ DEX)
- [ ] INT stat extends max turns
- [ ] STR stat adds flat damage
- [ ] No console errors during gameplay

---

**Status**: ✅ All compilation errors fixed! System ready for testing.











