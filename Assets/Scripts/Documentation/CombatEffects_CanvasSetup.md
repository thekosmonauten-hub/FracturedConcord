# Combat Effects Canvas Setup Guide

## Issue: Effects Not Showing
If your impact effects (like Heavy Strike) aren't appearing, it's likely because the `CombatEffectManager` is not properly connected to a Canvas where UI elements can render.

## Problem
- UI elements (like GameObject impact effects) need to be under a Canvas to render properly
- The `CombatEffectManager` might be spawned outside the UI hierarchy
- Effects are created but not visible because they're not in the Canvas

## Solution

### Step 1: Move CombatEffectManager to Canvas
1. **Find your main combat Canvas**
   - Look for a GameObject with "Canvas" in the name
   - Usually named "CombatCanvas", "UI Canvas", or similar

2. **Move CombatEffectManager**
   - Drag the `CombatEffectManager` GameObject from wherever it is
   - Drop it as a **child** of your main combat Canvas
   - This ensures it's in the UI hierarchy

### Step 2: Configure Canvas Settings
1. **Open CombatEffectManager component**
2. **In "Canvas Settings" section:**
   - **Target Canvas**: Assign your main combat Canvas (optional)
   - **Auto Find Canvas**: Check this box (recommended)

### Step 3: Verify Setup
1. **Right-click CombatEffectManager** → "Debug Canvas Setup"
2. **Check console** for canvas information:
   ```
   Target Canvas: CombatCanvas
   Auto Find Canvas: True
   Total Canvases Found: 1
   Main Combat Canvas: CombatCanvas
   ```

### Step 4: Test Effects
1. **Right-click CombatEffectManager** → "Test Fire Damage Effect"
2. **Verify the effect appears** on screen
3. **Play Heavy Strike card** and check for impact effects

## Alternative Setup Methods

### Method 1: Auto-Find (Recommended)
```csharp
// CombatEffectManager automatically finds the combat canvas
[SerializeField] private bool autoFindCanvas = true;
```
- ✅ No manual configuration needed
- ✅ Works across different scene setups
- ✅ Automatically adapts to canvas changes

### Method 2: Manual Assignment
```csharp
// Manually assign the target canvas
[SerializeField] private Canvas targetCanvas;
```
- ✅ Explicit control over canvas selection
- ❌ Requires manual setup for each scene
- ❌ Breaks if canvas hierarchy changes

### Method 3: Parent Canvas (Legacy)
```csharp
// Move CombatEffectManager under Canvas in hierarchy
// Effects will automatically use parent canvas
```
- ✅ Simple hierarchy setup
- ❌ Less flexible for complex scenes
- ❌ Requires manual hierarchy management

## Canvas Requirements

### Canvas Settings
- **Render Mode**: Screen Space - Overlay (recommended)
- **Sort Order**: Higher than other canvases (if multiple)
- **Pixel Perfect**: Optional but recommended

### Canvas Hierarchy
```
Canvas (Main Combat UI)
├── CombatEffectManager
├── PlayerCombatDisplay
├── EnemyCombatDisplay
├── CardHand
└── Other UI Elements
```

## Troubleshooting

### Effects Still Not Showing
1. **Check Canvas Sort Order**
   - Ensure combat canvas has higher sort order
   - Other canvases might be covering effects

2. **Verify Effect Prefabs**
   - Ensure impact effect prefabs are assigned
   - Check prefabs have proper components (ParticleSystem, etc.)

3. **Debug Canvas Setup**
   - Use "Debug Canvas Setup" context menu
   - Check console for canvas information

4. **Test Individual Effects**
   - Use context menu tests for each damage type
   - Verify effects play individually

### Performance Issues
1. **Reduce Pool Size**
   - Lower pool size if memory constrained
   - Monitor effect count in debug

2. **Check Effect Duration**
   - Reduce effect duration if too long
   - Ensure auto-destroy is enabled

### Canvas Not Found
1. **Manual Assignment**
   - Drag canvas to "Target Canvas" field
   - Disable "Auto Find Canvas"

2. **Check Canvas Name**
   - Ensure canvas has "Combat" in name
   - Or rename canvas to include "Combat"

## Expected Behavior

### After Proper Setup
- **Heavy Strike** → Physical impact effect above enemy
- **Fire Cards** → Fire impact effect above enemy
- **Ice Cards** → Cold impact effect above enemy
- **Lightning Cards** → Lightning impact effect above enemy
- **Chaos Cards** → Chaos impact effect above enemy

### Console Messages
```
CombatEffectManager auto-found target canvas: CombatCanvas
Playing Physical Damage effect at (x, y, z)
Moved effect PhysicalEffect to target canvas: CombatCanvas
```

## Best Practices

### Canvas Management
- **Single Combat Canvas**: Use one main canvas for combat UI
- **Proper Hierarchy**: Keep all combat UI elements under main canvas
- **Sort Order**: Ensure combat canvas renders above game world

### Effect Management
- **Auto-Find Enabled**: Let system find canvas automatically
- **Pool Sizes**: Use appropriate pool sizes (5-15 per type)
- **Auto-Destroy**: Keep auto-destroy enabled for cleanup

### Performance
- **Monitor Usage**: Use debug tools to check effect usage
- **Appropriate Duration**: 1-3 seconds for most effects
- **Efficient Cleanup**: Ensure effects return to pools

## Integration with CombatUI

### AnimatedCombatUI Integration
```csharp
// CombatEffectManager should be a child of the same canvas as AnimatedCombatUI
Canvas (CombatCanvas)
├── AnimatedCombatUI
├── CombatEffectManager  // ← Should be here
└── Other Combat UI
```

### CombatDisplayManager Integration
```csharp
// CombatEffectManager works with CombatDisplayManager
// Effects target player and enemy displays automatically
combatEffectManager.PlayElementalDamageEffectOnTarget(enemyDisplay.transform, DamageType.Physical);
```

## Testing Checklist

- [ ] CombatEffectManager is under Canvas
- [ ] Auto Find Canvas is enabled
- [ ] Target Canvas is assigned or auto-found
- [ ] Impact effect prefabs are assigned
- [ ] "Debug Canvas Setup" shows correct canvas
- [ ] Individual effect tests work
- [ ] Heavy Strike triggers impact effect
- [ ] Console shows effect creation messages

The Canvas integration should resolve the Heavy Strike impact effect visibility issue!
