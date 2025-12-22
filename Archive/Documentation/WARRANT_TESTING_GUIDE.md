# Warrant Stat Implementation Testing Guide

This guide explains how to use the `WarrantStatImplementationTester` script to verify all warrant stat implementations.

## Setup

1. **Add the Test Script to a GameObject:**
   - Create an empty GameObject in your scene (e.g., "WarrantTester")
   - Add the `WarrantStatImplementationTester` component to it
   - The script will automatically use the current character from `CharacterManager`

2. **Configure Test Settings:**
   - **Run Tests On Start**: Enable to automatically run tests when the scene starts
   - **Verbose Logging**: Enable to see detailed test results with expected vs actual values

## Running Tests

### Method 1: Context Menu (Recommended)
1. Select the GameObject with the `WarrantStatImplementationTester` component
2. Right-click the component in the Inspector
3. Click **"Run All Tests"**

### Method 2: Enable "Run Tests On Start"
1. Enable the **"Run Tests On Start"** checkbox in the Inspector
2. Play the scene
3. Tests will run automatically on Start

### Method 3: Code
```csharp
var tester = FindObjectOfType<WarrantStatImplementationTester>();
tester.RunAllTests();
```

## What Gets Tested

### Phase 1 Tests

#### 1.1 Card Tag-Based Damage Modifiers
- Tests `increasedProjectileDamage` with Projectile tag
- Tests `increasedAreaDamage` with AoE tag
- Tests `increasedMeleeDamage` with Melee scaling
- Verifies modifiers only apply when appropriate tags/scaling are present

#### 1.2 Guard Effectiveness
- Tests `guardEffectivenessIncreased` modifier
- Verifies guard calculation: `finalGuard = baseGuard * (1 + modifier / 100)`

#### 1.3 Flat vs Increased Modifiers
- Tests `maxHealthFlat` and `maxManaFlat` (applied first)
- Tests `maxHealthIncreased` and `maxManaIncreased` (applied after flat)
- Verifies order: `(base + flat) * (1 + increased / 100)`

#### 1.4 Status Effect Duration
- Tests `statusEffectDuration` modifier
- Verifies duration calculation with rounding up: `Ceil(baseDuration * (1 + modifier / 100))`

### Phase 2 Tests

#### 2.1 Defense Modifiers Scaling
- Tests `evasionIncreased`, `armourIncreased`, `energyShieldIncreased`
- Verifies modifiers apply to base values from items: `baseValue * (1 + increased / 100)`
- Tests that base values are tracked separately from modifiers

#### 2.2 Weapon Type Modifiers
- Tests `increasedAxeDamage`, `increasedBowDamage`, `increasedSwordDamage`, `increasedDaggerDamage`
- Verifies modifiers apply based on actual weapon item type
- Tests that weapon type is correctly identified from equipped weapon

#### 2.3 Ailment Damage Modifiers
- Tests `increasedPoisonMagnitude`, `increasedPoisonDamage`, `increasedDamageOverTime` for Poison
- Tests `increasedIgniteMagnitude` + `increasedDamageOverTime` for Ignite
- Tests `increasedBleedMagnitude` + `increasedDamageOverTime` for Bleed
- Verifies multiplicative application: `base * (1 + mod1/100) * (1 + mod2/100) * ...`

#### 2.4 Conditional Damage
- Tests `increasedDamageVsChilled` when target has Chill status
- Tests `increasedDamageVsShocked` when target has Shocked status
- Tests `increasedDamageVsIgnited` when target has Burn status
- Verifies modifiers stack when multiple conditions are met

## Test Output

### Success
```
✓ PASS - Test Name: Expected: X, Got: X
```

### Failure
```
✗ FAIL - Test Name: Expected: X, Got: Y
```

### Example Output
```
========================================
WARRANT STAT IMPLEMENTATION TEST SUITE
========================================

[Setup] Using character: TestCharacter (Level 10)

=== Phase 1.1: Card Tag-Based Damage Modifiers ===
✓ PASS - Projectile Damage Modifier: Expected: 20%, Got: 20%
✓ PASS - AoE Damage Modifier: Expected: 15%, Got: 15%
✓ PASS - Melee Damage Modifier: Expected: 25%, Got: 25%

=== Phase 1.2: Guard Effectiveness ===
✓ PASS - Guard Effectiveness: Base: 100, Expected: 130, Got: 130

...

========================================
ALL TESTS COMPLETE
========================================
```

## Clearing Test Data

To clear all test modifiers from the character:
1. Right-click the component in the Inspector
2. Click **"Clear Test Modifiers"**

This will:
- Clear all `warrantStatModifiers`
- Clear all `warrantFlatModifiers`
- Reset base defense values to 0

## Troubleshooting

### "CharacterManager.Instance is null"
- Make sure `CharacterManager` exists in the scene
- Ensure a character is loaded before running tests

### "No character loaded"
- The script will try to create a basic test character
- For best results, load a character through the normal game flow first

### Tests Fail Unexpectedly
- Check that warrant modifiers are being applied correctly
- Verify `CharacterManager.RefreshWarrantModifiers()` is being called
- Ensure equipment is properly equipped and stats are calculated

## Notes

- Tests use the **current character** from `CharacterManager.Instance`
- Tests modify the character's warrant modifiers temporarily
- Use **"Clear Test Modifiers"** to clean up after testing
- Tests are **non-destructive** - they only read and verify values, don't modify game state permanently

## Integration with Actual Warrants

To test with actual warrant assets:
1. Assign a `Warrant` asset to the **"Test Warrant"** field
2. Equip the warrant in the Warrant Tree
3. Allocate nodes to activate modifiers
4. Run tests to verify modifiers are applied correctly

The test script will automatically use any modifiers currently applied to the character, so you can test with real warrants by:
1. Equipping warrants in-game
2. Running the test script
3. Verifying the modifiers are working as expected

