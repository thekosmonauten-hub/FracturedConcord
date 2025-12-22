# Level Scaling Column Proposal

## Current System
- **Effect Level 1**: Shows effect at level 1
- **Effect Level 20**: Shows effect at level 20
- Generator creates modifiers with `_level20` suffix parameters
- Registry interpolates linearly between level 1 and 20

## Proposed Addition: "Scaling Type" Column

### Option 1: Simple Scaling Type (Recommended)
Add a column that specifies how the effect scales:

**Values:**
- `linear` (default) - Linear interpolation between level 1 and 20
- `none` - No scaling, effect stays at level 1 value
- `exponential` - Exponential scaling (for future use)
- `step` - Only applies at certain level thresholds (for future use)

**Example:**
```
Category | Aura Name | ... | Effect Level 1 | Effect Level 20 | Scaling Type
Fire | Pyreheart | ... | 90 flat Fire damage | 160 flat Fire damage | linear
```

### Option 2: Scaling Formula Column
More flexible but more complex:

**Format:** `"linear"`, `"exponential:2.0"`, `"custom:formula"`, etc.

### Option 3: Per-Parameter Scaling
Most flexible but most complex - specify scaling for each parameter separately.

## Recommendation

**Use Option 1** - Simple scaling type column:
- Easy to understand
- Covers 99% of use cases (linear scaling)
- Leaves room for future expansion
- Defaults to "linear" if empty

## Implementation

The generator would:
1. Parse Effect Level 1 and Effect Level 20 (as before)
2. Check Scaling Type column
3. If "linear" or empty: Create modifiers with `_level20` parameters (current behavior)
4. If "none": Create modifiers without scaling (use level 1 value only)
5. Future: Support other scaling types

## TSV Structure

```
Category | Aura Name | Description | Effect Level 1 | Effect Level 20 | Reliance | Modifier IDs | Embossing Slots | Required Level | Unlock Requirement | Icon Name | Theme Color | Scaling Type
```

## Benefits

1. **Explicit**: Makes it clear how each aura scales
2. **Flexible**: Can disable scaling for certain auras if needed
3. **Future-proof**: Easy to add new scaling types
4. **Backwards compatible**: Defaults to "linear" if column is empty

