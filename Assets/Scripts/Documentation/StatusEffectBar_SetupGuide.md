# StatusEffectBar Setup Guide

## Overview
The StatusEffectBar displays visual icons for active status effects separated into two distinct bars:
- **Buff Bar**: Displays positive effects (buffs) with green background tint
- **Debuff Bar**: Displays negative effects (debuffs) with red background tint

Each icon shows the effect type, duration, and magnitude, providing clear visual feedback during combat.

## Required Sprites

### Sprite Organization
Create a folder structure in your project:
```
Assets/
├── Resources/
│   └── StatusEffectIcons/
│       ├── Poison.aseprite
│       ├── Ignite.aseprite
│       ├── Chilled.aseprite
│       ├── Stunned.aseprite
│       ├── Vulnerability.aseprite
│       ├── TempStrength.aseprite
│       ├── TempAgility.aseprite
│       ├── TempIntelligence.aseprite
│       ├── Shielded.aseprite
│       ├── TempProjectileDamage.aseprite
│       └── ... (all other status effects)
```

### Sprite Specifications
- **Size**: 32x32 or 64x64 pixels
- **Format**: Aseprite files with transparent background (converted to PNG by Unity)
- **Style**: Pixel art or flat design
- **Naming**: Must match the `iconName` in `StatusEffect.cs`

### Current Sprite Mappings
- **Poison** → `Poison.aseprite`
- **Ignite** → `Ignite.aseprite` (for Burn effects)
- **Chilled** → `Chilled.aseprite` (for Freeze effects)
- **Stunned** → `Stunned.aseprite`
- **Vulnerability** → `Vulnerability.aseprite` (for Vulnerable effects)
- **TempStrength** → `TempStrength.aseprite` (for Strength buffs)
- **TempAgility** → `TempAgility.aseprite` (for Dexterity buffs)
- **TempIntelligence** → `TempIntelligence.aseprite` (for Intelligence buffs)
- **Shielded** → `Shielded.aseprite` (for Shield effects)
- **TempProjectileDamage** → `TempProjectileDamage.aseprite`

## Setup Instructions

### 1. Create StatusEffectIcon Prefab

1. **Create GameObject Hierarchy**:
   ```
   StatusEffectIcon (GameObject)
   ├── Background (Image)
   ├── Icon (Image)
   ├── DurationText (TextMeshPro)
   └── MagnitudeText (TextMeshPro)
   ```

2. **Component Setup**:
   - Add `StatusEffectIcon` script to the root GameObject
   - Assign references in the inspector:
     - `Background Image`: Background Image component
     - `Icon Image`: Icon Image component
     - `Duration Text`: Duration Text component
     - `Magnitude Text`: Magnitude Text component

3. **Visual Settings**:
   - Set `Buff Color`: Green with transparency (0, 1, 0, 0.3)
   - Set `Debuff Color`: Red with transparency (1, 0, 0, 0.3)

4. **Save as Prefab**: Save this GameObject as `StatusEffectIconPrefab.prefab`

### 2. Create StatusEffectBar GameObject

1. **Create GameObject Hierarchy**:
   ```
   StatusEffectBar (GameObject)
   ├── BuffContainer (Empty GameObject with HorizontalLayoutGroup)
   └── DebuffContainer (Empty GameObject with HorizontalLayoutGroup)
   ```

2. **Component Setup**:
   - Add `StatusEffectBar` script to the root GameObject
   - Assign `BuffContainer` reference (for positive effects)
   - Assign `DebuffContainer` reference (for negative effects)
   - Assign `StatusEffectIconPrefab` reference

3. **Layout Settings**:
   - Set `Icon Spacing`: 5f
   - Set `Max Icons Per Row`: 8
   - Adjust `Buff Background Color` (default: green with 30% transparency)
   - Adjust `Debuff Background Color` (default: red with 30% transparency)

4. **Container Layout** (Optional but Recommended):
   - Add `HorizontalLayoutGroup` component to BuffContainer:
     - Child Alignment: Middle Left
     - Spacing: 5
     - Child Force Expand: Width/Height unchecked
   - Add `HorizontalLayoutGroup` component to DebuffContainer:
     - Child Alignment: Middle Left
     - Spacing: 5
     - Child Force Expand: Width/Height unchecked

### 3. Integration with Combat UI

#### For Player Status Effects:
1. Find your `PlayerCombatDisplay` GameObject
2. Add `StatusEffectBar` as a child
3. Position it above or below the player character
4. In `PlayerCombatDisplay.cs`, add:
   ```csharp
   [Header("Status Effect Display")]
   public StatusEffectBar statusEffectBar;
   
   private void Start()
   {
       if (statusEffectBar != null && statusEffectManager != null)
       {
           statusEffectBar.SetStatusEffectManager(statusEffectManager);
       }
   }
   ```

#### For Enemy Status Effects:
1. Find your `EnemyCombatDisplay` GameObject
2. Add `StatusEffectBar` as a child
3. Position it above or below the enemy character
4. In `EnemyCombatDisplay.cs`, add:
   ```csharp
   [Header("Status Effect Display")]
   public StatusEffectBar statusEffectBar;
   
   private void Start()
   {
       if (statusEffectBar != null && statusEffectManager != null)
       {
           statusEffectBar.SetStatusEffectManager(statusEffectManager);
       }
   }
   ```

### 4. Sprite Implementation

#### Option A: Individual Aseprite Files (Current Setup)
1. Create individual Aseprite files for each status effect
2. Place them in `Assets/Resources/StatusEffectIcons/`
3. Name them exactly as specified in `StatusEffect.cs` (e.g., `Poison.aseprite`, `Ignite.aseprite`)
4. Unity will automatically convert them to sprites for use in the game

#### Option B: Sprite Atlas (Recommended for Performance)
1. Create a Sprite Atlas containing all status effect sprites
2. Modify `LoadStatusEffectSprite()` in `StatusEffectIcon.cs` to use the atlas
3. This improves performance when many status effects are active

### 5. Testing

#### Debug Methods Available:
- **Test Add Poison Effect**: Adds a 3-turn poison effect
- **Test Add Strength Buff**: Adds a 5-turn strength buff
- **Clear All Effects**: Removes all status effects

#### Manual Testing:
1. Play the game and enter combat
2. Use cards that apply status effects (e.g., "Ground Slam" for Vulnerability)
3. Verify icons appear with correct sprites, colors, and text
4. Check that duration counts down correctly
5. Verify magnitude is displayed for effects with values

## Expected Behavior

### Visual Display:
- **Buff Bar**: 
  - Green background tint for positive effects
  - Displays all active buffs (Strength, Shield, etc.)
  - Empty when no buffs are active
- **Debuff Bar**: 
  - Red background tint for negative effects
  - Displays all active debuffs (Poison, Vulnerable, etc.)
  - Empty when no debuffs are active
- **Duration**: Shows remaining turns (or ∞ for permanent effects)
- **Magnitude**: Shows effect strength (e.g., "5" for 5 poison damage)

### Icon Layout:
- Each bar arranges icons horizontally in its container
- Spacing between icons is configurable
- Icons automatically appear/disappear as effects are added/removed
- Buffs and debuffs are visually separated for clarity

### Turn-Based Duration:
- Status effect durations decrease by 1 turn when "End Turn" is clicked
- The visual countdown updates automatically
- Effects are removed when duration reaches 0
- Permanent effects (∞) never expire

### Fallback Behavior:
- If a sprite is missing, a colored square is generated
- Color matches the effect's default color
- Warning is logged to console for missing sprites

## Troubleshooting

### Common Issues:

1. **Icons Not Appearing**:
   - Check that `StatusEffectManager` is assigned
   - Verify `StatusEffectIconPrefab` is set
   - Ensure `BuffContainer` and `DebuffContainer` are assigned
   - Check that status effects are being added to the manager

2. **Wrong Sprites**:
   - Verify sprite names match `iconName` in `StatusEffect.cs`
   - Check that sprites are in the correct Resources folder
   - Ensure sprites are imported as Sprites (not Textures)

3. **Layout Issues**:
   - Adjust `Icon Spacing` for better visual spacing
   - Check that BuffContainer and DebuffContainer have proper RectTransform settings
   - Consider adding HorizontalLayoutGroup for automatic spacing
   - Verify icon prefab size matches expected dimensions

4. **Duration Not Counting Down**:
   - Ensure `SimpleCombatUI` has `CombatDisplayManager` reference assigned
   - Check that "End Turn" button is properly wired to `OnEndTurnClicked()`
   - Verify `CombatDisplayManager.EndPlayerTurn()` is being called
   - Look for debug logs: "AdvanceTurn: [EffectName] - Duration remaining: X/Y"
   - Context menu test methods bypass UI and directly call turn advancement

5. **Buffs/Debuffs in Wrong Bar**:
   - Check `isDebuff` property in StatusEffect creation
   - Buffs should have `isDebuff = false`
   - Debuffs should have `isDebuff = true`
   - Verify StatusEffect.SetDefaultProperties() for effect type

6. **Performance Issues**:
   - Consider using a Sprite Atlas for better performance
   - Limit the number of active status effects
   - Optimize icon prefab complexity
   - Consider disabling Update() loop in StatusEffectBar if visual updates are too frequent

## Advanced Features

### Custom Status Effects:
1. Add new `StatusEffectType` enum values
2. Update `SetDefaultProperties()` in `StatusEffect.cs`
3. Create corresponding sprite files
4. Test with the debug methods

### Tooltip Integration:
- Add tooltip system to show detailed effect information on hover
- Display effect description, source, and remaining duration
- Consider using UI Toolkit tooltips for better integration

### Animation Effects:
- Add entrance/exit animations for status effect icons
- Implement pulsing effects for important status effects
- Add visual feedback when effects tick (e.g., poison damage)

---

## Recent Updates

### Version 2.0 - Dual Bar System (Current)

**Major Changes:**
1. **Separated Buff and Debuff Bars**:
   - StatusEffectBar now uses two separate containers
   - Buffs display in their own bar with green tint
   - Debuffs display in their own bar with red tint
   - Improved visual clarity and organization

2. **Fixed Turn-Based Duration Countdown**:
   - SimpleCombatUI now properly calls `CombatDisplayManager.EndPlayerTurn()`
   - Status effect durations correctly decrease when "End Turn" is clicked
   - Added debug logging to track turn advancement
   - Context menu tests and actual gameplay now work consistently

**Migration from Version 1.0:**
- Replace single `iconContainer` with `buffContainer` and `debuffContainer`
- Update any existing StatusEffectBar GameObjects to use the new dual-container structure
- Ensure SimpleCombatUI references CombatDisplayManager (auto-finds if not assigned)
- No changes needed to StatusEffect or StatusEffectManager code

**Benefits:**
- Clear visual separation between positive and negative effects
- Easier to track buff/debuff status at a glance
- Better UI organization and scalability
- Proper turn-based gameplay integration
