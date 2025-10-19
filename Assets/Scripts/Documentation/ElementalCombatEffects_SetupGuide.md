# Elemental Combat Effects Setup Guide

## Overview
The Elemental Combat Effects system provides different visual impact effects for different damage types, making combat more visually engaging and informative.

## Supported Damage Types
- **Fire** - Burning, explosive effects
- **Cold** - Freezing, crystalline effects  
- **Lightning** - Electric, shocking effects
- **Chaos** - Toxic, corrosive, magical effects
- **Physical** - Impact, crushing effects

## Setup Instructions

### Step 1: Create CombatEffectManager
1. Create empty GameObject named "CombatEffectManager"
2. Add `CombatEffectManager` component
3. Configure basic settings:
   - **Effect Duration**: 2 seconds
   - **Effect Offset**: (0, 50, 0) - positions effects above targets
   - **Pool Size**: 10 effects per type
   - **Auto Destroy Effects**: true

### Step 2: Assign Elemental Impact Prefabs
In the "Elemental Impact Effects" section, assign your impact effect prefabs:

```
Fire Impact Prefab → Fire damage effects
Cold Impact Prefab → Cold damage effects  
Lightning Impact Prefab → Lightning damage effects
Chaos Impact Prefab → Chaos damage effects (poison, magic, dark)
Physical Impact Prefab → Physical damage effects
```

### Step 3: Impact Effect Prefab Requirements
Each elemental prefab should have:
- **ParticleSystem** component (for particle effects)
- **Animator** component (for animation effects)
- Or **Animation** component (for legacy animation)

### Step 4: Effect Prefab Guidelines

#### Fire Effects
- Colors: Red, orange, yellow
- Particles: Sparks, flames, smoke
- Animation: Explosion, burst

#### Cold Effects  
- Colors: Blue, cyan, white
- Particles: Ice crystals, frost, mist
- Animation: Freeze, shatter

#### Lightning Effects
- Colors: Yellow, white, blue
- Particles: Electric arcs, sparks
- Animation: Zap, discharge

#### Chaos Effects
- Colors: Green, purple, dark, toxic
- Particles: Bubbles, gas, acid, shadows, sparkles
- Animation: Corrosion, dissolve, absorb, enchant

#### Physical Effects
- Colors: Gray, brown, metallic
- Particles: Debris, dust, impact
- Animation: Crush, slam

## How It Works

### Automatic Element Detection
The system automatically detects damage types from:
1. **Card Effects** - Uses `damageType` from `CardEffect`
2. **Card Names** - Keyword matching in card names
3. **Fallback** - Defaults to Physical damage

### Card Name Keywords
```
Fire: "fire", "burn"
Cold: "ice", "frost", "freeze", "cold"
Lightning: "lightning", "thunder", "shock"
Chaos: "poison", "toxic", "venom", "chaos", "magic", "arcane", "spell", "dark", "shadow", "void", "light", "holy", "divine"
Physical: (default, no keywords needed)
```

### Effect Triggering
1. **Card Played** → System detects damage type
2. **Effect Selection** → Chooses appropriate elemental effect
3. **Target Positioning** → Places effect above target
4. **Effect Playback** → Plays particles/animations
5. **Auto-cleanup** → Returns to pool after duration

## Testing

### Context Menu Tests
Right-click `CombatEffectManager` for individual tests:
- "Test Fire Damage Effect"
- "Test Cold Damage Effect"  
- "Test Lightning Damage Effect"
- "Test Chaos Damage Effect"
- "Test Physical Damage Effect"
- "Test Critical Fire Damage Effect"

### In-Game Testing
1. **Fire Cards** → Fire impact effects
2. **Cold Cards** → Cold impact effects
3. **Lightning Cards** → Lightning impact effects
4. **Chaos Cards** → Chaos impact effects (poison, magic, dark, light)
5. **Physical Cards** → Physical impact effects
6. **Critical Hits** → Enhanced elemental effects

### Debug Features
- **Console Logs**: Shows which effects are played
- **Pool Debugging**: Shows effect usage statistics
- **Context Menu Tests**: Individual effect testing
- **Automatic Fallback**: Uses basic damage effects if elemental missing

## Performance Features

### Object Pooling
- **Separate Pools**: Each damage type has its own pool
- **Auto-scaling**: Creates new effects if pools empty
- **Efficient Cleanup**: Returns effects to correct pools

### Memory Management
- **Auto-destroy**: Effects automatically cleaned up
- **Pool-based**: Reduces garbage collection
- **Singleton Pattern**: Prevents duplicate managers

## Expected Behavior

### Attack Cards
- **Fire Attack** → Fire impact above enemy
- **Cold Attack** → Cold impact above enemy  
- **Lightning Attack** → Lightning impact above enemy
- **Chaos Attack** → Chaos impact above enemy
- **Physical Attack** → Physical impact above enemy

### Skill Cards
- **Heal Skills** → Heal effects on player
- **Damage Skills** → Appropriate elemental effects on target
- **Buff Skills** → Status effects on player

### Critical Hits
- **Enhanced Effects** → More intense versions of elemental effects
- **Special Feedback** → Visual indication of critical damage

## Troubleshooting

### Effects Not Showing
1. Check prefab assignments in CombatEffectManager
2. Verify prefabs have ParticleSystem/Animator components
3. Check console for error messages
4. Use context menu tests to verify individual effects

### Performance Issues
1. Reduce pool size if memory constrained
2. Check effect duration settings
3. Monitor console for pool statistics
4. Use debug context menu to check pool status

### Wrong Effects Playing
1. Check card name keywords
2. Verify card effect damage types
3. Check fallback behavior (should use Physical)
4. Use debug logs to trace effect selection

## Integration Examples

### Custom Damage Types
To add new damage types:
1. Add to `DamageType` enum
2. Add prefab field to `CombatEffectManager`
3. Update `InitializeElementalEffects()` method
4. Add keyword detection in `GetCardDamageType()`

### Card-Specific Effects
To create unique effects for specific cards:
1. Create custom effect prefab
2. Assign to card-specific effect field
3. Modify effect selection logic
4. Add custom triggering method

### Status Effect Integration
Elemental effects can be combined with status effects:
- **Fire** → Burn status effect
- **Ice** → Freeze status effect
- **Poison** → Poison status effect
- **Lightning** → Shock status effect

## Best Practices

### Effect Design
- **Consistent Style**: Match game's visual theme
- **Clear Distinction**: Make damage types visually distinct
- **Performance**: Keep particle counts reasonable
- **Duration**: 1-3 seconds for most effects

### Pool Management
- **Appropriate Sizes**: 5-15 effects per type
- **Monitor Usage**: Check pool statistics regularly
- **Auto-scaling**: Allow system to create new effects if needed
- **Cleanup**: Ensure effects return to pools properly

### Integration
- **Card Names**: Use clear elemental keywords
- **Effect Types**: Set damage types in card effects
- **Testing**: Use context menu tests during development
- **Debugging**: Monitor console logs for issues

The Elemental Combat Effects system is now ready to use with your impact assets!
