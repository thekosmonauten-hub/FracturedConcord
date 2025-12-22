# StatusDatabase Setup Guide

## Overview

The `StatusDatabase` provides a centralized way to manage status effect sprites, colors, and metadata. It works seamlessly with sprite sheets and maintains backward compatibility with the existing `Resources.Load` approach.

## Benefits

✅ **Centralized Management** - All status effect data in one ScriptableObject  
✅ **Sprite Sheet Support** - Works with sprite sheets when sprites are properly named  
✅ **Backward Compatible** - Falls back to `Resources.Load` if database not used  
✅ **Easy Updates** - Change sprite references without touching code  
✅ **Consistent Pattern** - Follows the same pattern as `CurrencyDatabase`

## Setup Instructions

### 1. Create StatusDatabase Asset

1. In Unity, right-click in Project window → **Create** → **Dexiled** → **Status Database**
2. Name it `StatusDatabase` and save it to `Assets/Resources/`
3. The asset will be automatically loaded at runtime

### 2. Configure Status Effect Entries

For each status effect type, add an entry with:

- **Effect Type**: The `StatusEffectType` enum value
- **Effect Name**: Display name (e.g., "Poison", "TempStrength")
- **Description**: Tooltip description
- **Icon Sprite**: Drag sprite from sprite sheet (or individual sprite)
- **Effect Color**: Color for fallback/default sprite
- **Is Debuff**: Whether it's a debuff (red) or buff (green)
- **Tooltip Description**: Optional detailed tooltip text

### 3. Sprite Sheet Configuration

#### Option A: Using Sprite Sheets (Recommended)

1. Import sprite sheets with **Texture Type**: `Sprite (2D and UI)`
2. Set **Sprite Mode**: `Multiple`
3. Use **Sprite Editor** to slice sprites
4. **Name each sprite** to match the `iconName` in `StatusEffect.cs`:
   - `Poison` → "Poison"
   - `Bleed` → "Bleed"
   - `TempStrength` → "TempStrength"
   - etc.

5. In `StatusDatabase`, drag the appropriate sprite from the sprite sheet to each entry's **Icon Sprite** field

#### Option B: Individual Sprites

1. Place individual sprite files in `Assets/Resources/StatusEffectIcons/`
2. Name them to match `iconName` values
3. System will auto-load via `Resources.Load` if `StatusDatabase` not configured

### 4. Using StatusDatabase in Code

The `StatusEffectIcon` component automatically uses `StatusDatabase` if available:

```csharp
// StatusDatabase is automatically loaded from Resources
// No code changes needed - it's backward compatible!
```

**Optional**: Assign `StatusDatabase` directly to `StatusEffectIcon` component in Inspector for explicit reference.

## Current Status Effect Mappings

Based on `StatusEffect.cs`, these are the current `iconName` values:

| StatusEffectType | iconName | Description |
|-----------------|----------|-------------|
| Poison | `Poison` | Chaos DoT |
| Burn | `Ignite` | Fire DoT |
| Bleed | `Bleed` | Physical DoT |
| Chill | `Chilled` | Action speed reduction |
| Freeze | `Chilled` | (reuses Chilled icon) |
| Stun | `Stunned` | Skip turn |
| Vulnerable | `Vulnerability` | Take increased damage |
| Strength | `TempStrength` | Physical damage buff |
| Dexterity | `TempAgility` | Accuracy/evasion buff |
| Intelligence | `TempIntelligence` | Spell damage buff |
| Shield | `Shielded` | Damage absorption |
| Crumble | `Crumble` | Physical damage storage |
| Bolster | `Bolster` | Damage reduction |
| TempMaxMana | `TempMana` | Max mana increase |
| TempEvasion | `TempEvasion` | Evasion buff |
| SpellPower | `SpellPower` | Spell damage multiplier |

## How It Works

### Loading Priority

1. **StatusDatabase** (if assigned/available)
   - Tries to find sprite by `iconName` first
   - Falls back to `StatusEffectType` lookup
   
2. **Resources.Load** (fallback)
   - Loads from `Resources/StatusEffectIcons/{iconName}`
   - Works with sprite sheets if sprites are named correctly
   
3. **Default Sprite** (last resort)
   - Creates a colored square using `effectColor`

### Backward Compatibility

✅ **Existing code works without changes**  
✅ **Gradual migration** - Can use database for some effects, Resources for others  
✅ **No breaking changes** - System falls back gracefully

## Migration from Resources.Load

If you're currently using `Resources.Load` and want to migrate to `StatusDatabase`:

1. Create `StatusDatabase` asset
2. Add entries for all status effects
3. Assign sprites from your sprite sheets
4. System will automatically use database when available
5. Old `Resources.Load` code continues to work as fallback

## Troubleshooting

### Sprites Not Showing

1. **Check sprite names match `iconName`** in `StatusEffect.cs`
2. **Verify sprite sheet import settings**:
   - Texture Type: `Sprite (2D and UI)`
   - Sprite Mode: `Multiple`
   - Sprites are properly sliced
3. **Check StatusDatabase asset**:
   - Is it in `Assets/Resources/`?
   - Are sprites assigned to entries?
   - Do effect types match?

### Using Both Systems

The system supports using both:
- `StatusDatabase` for effects you've configured
- `Resources.Load` for effects not yet in database

This allows gradual migration without breaking existing functionality.

## Example: Setting Up Poison Effect

1. Open `StatusDatabase` asset
2. Click **+** to add new entry
3. Set:
   - **Effect Type**: `Poison`
   - **Effect Name**: `Poison`
   - **Icon Sprite**: Drag "Poison" sprite from sprite sheet
   - **Effect Color**: Green
   - **Is Debuff**: ✓ (checked)
   - **Description**: "Deals chaos damage over time"
4. Save asset

The system will now use this sprite for all Poison status effects!

























