# Monster Rarity and Modifier System

## Overview

The Monster Rarity system allows enemies to spawn with different rarities (Normal, Magic, Rare, Unique) and receive modifiers that affect their stats, loot drops, and experience rewards.

## Rarity Tiers

### Normal Enemies
- **Modifiers**: None
- **Stats**: Base stats only
- **Loot**: Standard drops

### Magic Enemies
- **Visible Modifiers**: 1 random modifier
- **Hidden Modifiers**:
  - 250% increased Experience (2.5x multiplier)
  - 148% more Maximum Life (2.48x total)
  - 600% increased Quantity of Items Dropped (6x multiplier)
  - 200% increased Rarity of Items Dropped (2x multiplier)
  - Drops gear 1 level above its own level

### Rare Enemies
- **Visible Modifiers**: 2-4 random modifiers
- **Hidden Modifiers**:
  - 750% increased Experience (7.5x multiplier)
  - 390% more Maximum Life (4.9x total)
  - 1400% increased Quantity of Items Dropped (14x multiplier)
  - 1000% increased Rarity of Items Dropped (10x multiplier)
  - 10% increased Character Size (1.1x size multiplier)
  - Drops gear 2 levels above its own level

### Unique Enemies
- **Visible Modifiers**: Typically none (scripted/bosses)
- **Hidden Modifiers**:
  - 450% increased Experience (4.5x multiplier)
  - 698% more Maximum Life (7.98x total)
  - 2850% increased Quantity of Items Dropped (28.5x multiplier)
  - 1000% increased Rarity of Items Dropped (10x multiplier)

## Implementation Details

### Files Created

1. **`MonsterModifier.cs`** - ScriptableObject for visible modifiers
   - Stat modifications (health, damage, accuracy, etc.)
   - Resistance modifications
   - Special effects (regen, haste, armor, reflect)

2. **`MonsterModifierDatabase.cs`** - Database for all modifiers
   - Singleton pattern
   - Methods to get random modifiers

3. **`MonsterRarityModifiers.cs`** - Static class with hidden base modifiers
   - Constants for each rarity tier

### Files Modified

1. **`Enemy.cs`**
   - Added `modifiers` list for visible modifiers
   - Added hidden stats (experienceMultiplier, quantityMultiplier, etc.)
   - Rewrote `ApplyRarityScaling()` to apply all modifiers

2. **`EnemySpawner.cs`**
   - Updated to roll rarity for all encounters
   - Uses `RollRarityForEncounter()` method

3. **`CardEffectProcessor.cs`**
   - Updated experience calculation to use `enemy.experienceMultiplier`

4. **`CombatManager.cs`**
   - Tracks defeated Enemy instances for rarity modifiers
   - Ready for loot multiplier integration

## Creating Monster Modifiers

### Using the Editor Tool

1. Go to **Dexiled > Create Monster Modifier** in the Unity menu
2. Fill in the modifier details:
   - Name and description
   - Stat modifications (percentages)
   - Resistances
   - Special effects
3. Click "Create Modifier Asset"
4. The asset will be created in `Assets/Resources/MonsterModifiers/`

### Preset Buttons

The editor tool includes preset buttons for common modifiers:
- **Hasted**: +20% Accuracy, +15% Evasion, Hasted effect
- **Armored**: +30% Health, +25% Physical Resistance, Armored effect
- **Resistant**: +30% to all elemental resistances

## Setting Up the Database

1. Create a `MonsterModifierDatabase` asset:
   - Right-click in Project window
   - **Create > Dexiled > Combat > Monster Modifier Database**
   - Save it as `Assets/Resources/MonsterModifierDatabase.asset`

2. Assign modifiers to the database:
   - Select the database asset
   - Drag all your MonsterModifier assets into the "All Modifiers" list

## Integration Status

### ✅ Completed
- Rarity rolling system
- Hidden base modifiers applied
- Visible modifier rolling (Magic: 1, Rare: 2-4)
- Experience multiplier integration
- Editor tool for creating modifiers

### 🔄 In Progress
- Loot quantity/rarity multiplier integration
- Item level bonus integration

### 📋 TODO
- UI display for modifiers (EnemyCombatDisplay)
- Loot generation to use quantityMultiplier and rarityMultiplier
- Item level calculation to use itemLevelBonus
- Size multiplier visual effect for Rare enemies

## Usage Example

```csharp
// When an enemy spawns, rarity is automatically rolled
Enemy enemy = enemyData.CreateEnemyWithRarity(EnemyRarity.Magic, areaLevel);

// Access modifiers
foreach (var modifier in enemy.modifiers)
{
    Debug.Log($"Enemy has modifier: {modifier.modifierName}");
}

// Access hidden stats
float xpMultiplier = enemy.experienceMultiplier; // 2.5x for Magic
float lootMultiplier = enemy.quantityMultiplier; // 6x for Magic
int itemLevelBonus = enemy.itemLevelBonus; // +1 for Magic
```

## Modifier Weight System

Modifiers have a **weight** value that determines how often they appear:
- **Higher weight** = More common (e.g., 100 = common)
- **Lower weight** = Rarer but typically more rewarding (e.g., 10 = very rare)

The system uses weighted random selection, so a modifier with weight 10 is 10x rarer than one with weight 100.

### Weight Guidelines
- **100** (Common): Standard modifiers
- **50** (Uncommon): Moderately rare with good rewards
- **25** (Rare): Rare modifiers with significant bonuses
- **10** (Very Rare): Extremely rare with powerful effects

## Loot Modifiers

Modifiers can now affect loot drops:
- **QuantityMultiplier**: Increases quantity of items dropped (e.g., 2.0 = 2x items)
- **RarityMultiplier**: Increases rarity of items dropped (e.g., 2.0 = 2x better rarity)

These multipliers stack multiplicatively with the base rarity multipliers, so a Magic enemy with a "Loot-Rich" modifier (2.0x quantity) will drop 12x items total (6x base × 2x modifier).

## Notes

- Hidden modifiers are automatically applied and don't count toward modifier cap
- Unique enemies typically don't get random modifiers (they're scripted)
- Modifiers are applied after base stats, so they stack multiplicatively
- Loot multipliers from modifiers stack multiplicatively with rarity base multipliers
- The system is designed to be extensible - add new modifiers easily via ScriptableObjects

