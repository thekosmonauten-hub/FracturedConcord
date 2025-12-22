# Monster Modifier CSV Format

## Overview

This document describes the CSV format for batch importing Monster Modifiers into the game.

## CSV File Structure

The CSV file must have a header row with column names, followed by data rows. Column names are case-insensitive and can use various formats (see examples below).

## Required Columns

### Basic Information
- **Name** (required): The modifier name (e.g., "Hasted", "Armored")
- **Description** (required): Description text for the modifier

### Color
- **Color**: Modifier color for UI display. Can be:
  - RGB format: `255,200,0` (values 0-255)
  - RGBA format: `255,200,0,255` (with alpha)
  - Hex format: `#FFC800` or `#FFC800FF`
  - Named color: `yellow`, `red`, `green`, `blue`, `cyan`, `magenta`, `white`, `black`, `gray`
  - Default: `yellow` if not specified

## Optional Columns (Stat Modifications)

All stat modification columns are percentages (e.g., `30` = 30% increase).

- **Health%**: Percentage increase to maximum health
- **Damage%**: Percentage increase to damage
- **Accuracy%**: Percentage increase to accuracy rating
- **Evasion%**: Percentage increase to evasion rating
- **CritChance%**: Percentage increase to critical strike chance
- **CritMultiplier%**: Percentage increase to critical strike multiplier

## Optional Columns (Resistances)

All resistance columns are percentages (e.g., `25` = +25% resistance).

- **PhysicalRes%**: Physical resistance
- **FireRes%**: Fire resistance
- **ColdRes%**: Cold resistance
- **LightningRes%**: Lightning resistance
- **ChaosRes%**: Chaos resistance

## Optional Columns (Special Effects)

- **RegenHealth**: Boolean (`true`/`false`, `1`/`0`, `yes`/`no`, `y`/`n`)
- **RegenAmount**: Health regenerated per turn (if RegenHealth is true)
- **IsHasted**: Boolean - Enemy is hasted (moves faster)
- **IsArmored**: Boolean - Enemy has increased defenses
- **ReflectsDamage**: Boolean - Enemy reflects damage
- **ReflectPercent%**: Percentage of damage reflected (if ReflectsDamage is true)

## Optional Columns (Loot Modifiers)

- **QuantityMultiplier**: Multiplier for quantity of items dropped (e.g., `1.5` = 50% more items). Default: `1.0`
- **RarityMultiplier**: Multiplier for rarity of items dropped (e.g., `2.0` = 2x better rarity). Default: `1.0`

## Optional Columns (Modifier Weight)

- **Weight**: Weight for rarity-based selection. Higher weight = more common. Lower weight = rarer but more rewarding. Default: `100`
  - Common modifiers: `100` (default)
  - Uncommon modifiers: `50`
  - Rare modifiers: `25`
  - Very rare modifiers: `10`

## Optional Columns (Modifier Effects)

- **EffectTypes**: Comma-separated list of effect types (e.g., `ShockOnHit,NoExtraCritDamage`). Available options:
  - `ShockOnHit` - Apply status effect on hit
  - `ChillOnHit` - Apply Chill on hit
  - `BurnOnHit` - Apply Burn on hit
  - `PoisonOnHit` - Apply Poison on hit
  - `VulnerableOnHit` - Apply Vulnerable on hit
  - `NoExtraCritDamage` - Take no extra damage from critical strikes
  - `ReducedCritDamage` - Take reduced damage from critical strikes
  - `GrantStackOnTurnStart` - Grant stacks at turn start
  - `GrantStackOnHit` - Grant stacks when hitting

- **OnHitStatusEffect**: Status effect type to apply on hit (if using ShockOnHit, etc.). Default: `Vulnerable`
- **OnHitStatusMagnitude**: Magnitude for on-hit status effects. Default: `1`
- **OnHitStatusDuration**: Duration (turns) for on-hit status effects. Default: `1`

- **TurnStartStackType**: Stack type to grant on turn start (`Tolerance`, `Agitate`, `Potential`). Default: `Tolerance`
- **TurnStartStackAmount**: Number of stacks to grant. Default: `1`
- **TurnStartStackRequiresHitRecently**: Boolean - only grant stacks if hit recently. Default: `false`

## Column Name Variations

The importer is flexible with column names. It matches column headers case-insensitively and supports variations:

- `Health%`, `health%`, `Health`, `health`, `Health Multiplier`, etc.
- `FireRes%`, `fireres%`, `Fire Resistance`, `fire resistance`, etc.
- `IsHasted`, `ishasted`, `Is Hasted`, `is hasted`, `Hasted`, etc.

## Example CSV

```csv
Name,Description,Color,Health%,Damage%,Accuracy%,Evasion%,CritChance%,CritMultiplier%,PhysicalRes%,FireRes%,ColdRes%,LightningRes%,ChaosRes%,RegenHealth,RegenAmount,IsHasted,IsArmored,ReflectsDamage,ReflectPercent%,QuantityMultiplier,RarityMultiplier,Weight,EffectTypes,OnHitStatusEffect,OnHitStatusMagnitude,OnHitStatusDuration,TurnStartStackType,TurnStartStackAmount,TurnStartStackRequiresHitRecently
Hasted,"This enemy moves and acts faster",cyan,0,0,20,15,0,0,0,0,0,0,0,false,0,true,false,false,0,1.0,1.0,100,,,1,1,Tolerance,1,false
Armored,"This enemy has increased defenses",gray,30,0,0,0,0,0,25,0,0,0,0,false,0,false,true,false,0,1.0,1.0,100,,,1,1,Tolerance,1,false
Resistant,"This enemy resists elemental damage",magenta,0,0,0,0,0,0,0,30,30,30,0,false,0,false,false,false,0,1.0,1.0,100,,,1,1,Tolerance,1,false
Regenerating,"This enemy regenerates health each turn",green,20,0,0,0,0,0,0,0,0,0,0,true,10,false,false,false,0,1.0,1.0,100,,,1,1,Tolerance,1,false
Reflective,"This enemy reflects damage back",red,0,0,0,0,0,0,0,0,0,0,0,false,0,false,false,true,25,1.0,1.0,100,,,1,1,Tolerance,1,false
Loot-Rich,"This enemy drops more items",yellow,0,0,0,0,0,0,0,0,0,0,0,false,0,false,false,false,0,2.0,1.5,50,,,1,1,Tolerance,1,false
Treasure-Hunter,"This enemy drops rarer items",255,215,0,0,0,0,0,0,0,0,0,0,0,false,0,false,false,false,0,1.5,3.0,25,,,1,1,Tolerance,1,false
of Shocking,"All hits apply Shock",0,255,255,0,0,0,0,0,0,0,0,0,0,false,0,false,false,false,0,1.0,1.0,100,ShockOnHit,Vulnerable,1,1,Tolerance,1,false
Toughness,"Take no extra damage from critical strikes",128,128,128,0,0,0,0,0,0,0,0,0,0,false,0,false,false,false,0,1.0,1.0,100,NoExtraCritDamage,,1,1,Tolerance,1,false
of Tolerance,"Gain 1 Tolerance stack every turn if hit recently",0,255,0,0,0,0,0,0,0,0,0,0,0,false,0,false,false,false,0,1.0,1.0,100,GrantStackOnTurnStart,,1,1,Tolerance,1,true
```

## Minimal Example

You can create modifiers with just the required fields:

```csv
Name,Description,Color
Hasted,"This enemy moves and acts faster",cyan
Armored,"This enemy has increased defenses",gray
```

All other fields will default to 0 or false.

## Using the Importer

1. Create your CSV file with the format above
2. In Unity, go to **Tools > Dexiled > Import Monster Modifiers from CSV**
3. Select your CSV file (must be a TextAsset in the project)
4. Choose output folder (default: `Assets/Resources/MonsterModifiers`)
5. Click "Import Modifiers"
6. The modifiers will be created as ScriptableObject assets

## Notes

- Empty cells default to 0 for numbers and false for booleans
- Column order doesn't matter - the importer matches by header name
- Column names are case-insensitive
- The importer handles quoted fields (e.g., descriptions with commas)
- If a modifier with the same name already exists, it will be skipped unless "Overwrite Existing Assets" is checked

## Color Reference

Common color values:

| Color | RGB | Hex |
|-------|-----|-----|
| Red | `255,0,0` | `#FF0000` |
| Green | `0,255,0` | `#00FF00` |
| Blue | `0,0,255` | `#0000FF` |
| Yellow | `255,255,0` | `#FFFF00` |
| Cyan | `0,255,255` | `#00FFFF` |
| Magenta | `255,0,255` | `#FF00FF` |
| White | `255,255,255` | `#FFFFFF` |
| Black | `0,0,0` | `#000000` |
| Gray | `128,128,128` | `#808080` |

## Template CSV

You can copy this template and fill it in:

```csv
Name,Description,Color,Health%,Damage%,Accuracy%,Evasion%,CritChance%,CritMultiplier%,PhysicalRes%,FireRes%,ColdRes%,LightningRes%,ChaosRes%,RegenHealth,RegenAmount,IsHasted,IsArmored,ReflectsDamage,ReflectPercent%,QuantityMultiplier,RarityMultiplier,Weight
```

## Weight Guidelines

- **100** (Common): Standard modifiers that should appear frequently
- **50** (Uncommon): Moderately rare modifiers with good rewards
- **25** (Rare): Rare modifiers with significant bonuses
- **10** (Very Rare): Extremely rare modifiers with powerful effects

Lower weight = rarer but typically more rewarding. The system uses weighted random selection, so a modifier with weight 10 is 10x rarer than one with weight 100.

