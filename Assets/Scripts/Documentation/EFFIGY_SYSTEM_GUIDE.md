# Effigy System Guide

## Overview
The Effigy system is similar to Last Epoch's Idols or Path of Exile's Cluster Jewels. It's a 6x4 grid where you place puzzle-piece-shaped effigies that provide passive bonuses.

## System Architecture

### Core Components

1. **Effigy.cs** - ScriptableObject for effigy data
   - Defines shape (width, height, mask)
   - Contains modifiers/affixes
   - Stores requirements

2. **EffigyGrid.cs** - Manages the 6x4 grid
   - Handles placement validation
   - Visual representation
   - Drag-and-drop logic

3. **EquipmentScreen.cs** - Integration point
   - Initializes EffigyGrid
   - Connects to inventory system

## Creating an Effigy

### Step 1: Create Effigy Asset
1. Right-click in Project window
2. `Create > Dexiled > Items > Effigy`
3. Name it (e.g., "Small_2x1_Health")

### Step 2: Configure Shape
The shape system uses a 2D boolean mask:
- `shapeWidth` / `shapeHeight`: Dimensions (e.g., 2x1, 3x2)
- `shapeMask`: Boolean array (true = occupied cell)

**Example Shapes:**

**2x1 (Horizontal line):**
```
Width: 2, Height: 1
Mask: [true, true]
Shape:
[X][X]
```

**L-Shape (2x2):**
```
Width: 2, Height: 2
Mask: [true, false,
       true, true]
Shape:
[X][ ]
[X][X]
```

**T-Shape (3x2):**
```
Width: 3, Height: 2
Mask: [false, true, false,
       true,  true,  true]
Shape:
[ ][X][ ]
[X][X][X]
```

### Step 3: Add Modifiers
- Add Affix instances to the `modifiers` list
- These provide the passive bonuses

### Step 4: Set Requirements
- `requiredLevel`: Minimum level to use

## UI Integration

### Adding to UXML (Optional)
You can add this to your `EquipmentScreen.uxml`:

```xml
<ui:VisualElement name="EffigyGridContainer" class="effigy-container">
    <ui:Label text="EFFIGIES" class="section-label" />
</ui:VisualElement>
```

If the container doesn't exist in UXML, it will be created automatically.

### Manual Placement from Code

```csharp
// Get EquipmentScreen reference
EquipmentScreen equipmentScreen = FindObjectOfType<EquipmentScreen>();

// Place effigy at position (0, 0) in grid
Effigy myEffigy = // Load from Resources or asset reference
equipmentScreen.TryPlaceEffigyFromInventory(myEffigy, 0, 0);
```

## Grid System Details

### Grid Coordinates
- **Origin**: Top-left is (0, 0)
- **X-axis**: Left to right (0-5)
- **Y-axis**: Top to bottom (0-3)

### Placement Rules
1. Effigy must fit entirely within 6x4 bounds
2. No overlapping with existing effigies
3. All cells in shape mask must be unoccupied

### Drag and Drop
- **Click and drag**: Moves existing effigy
- **Release**: Validates and places (or returns to original position)
- **Visual feedback**: Placement preview highlights occupied cells; the drag ghost reuses the full effigy sprite with an outline for rarity feedback.

### Visual Rendering (UGUI)
- Each occupied cell gets its own `Image` child. By default the effigy icon is reused for every cell, but you can slice a sprite sheet and register per-cell sprites via `EffigySpriteSetInitializer` (supports multiple entries per component) to paint unique art (e.g., the Z-piece cat).
- Cell visuals disable raycasts so `EffigyGridCellUI` keeps handling pointer events for drag/unequip operations.
- Rarity color is communicated through an `Outline`; if no sprite is assigned we fall back to a solid element tint.
- Drag ghost visuals and storage previews use the same sprite set so everything stays consistent.

### Affix Generation & Scaling
- Effigy assets act as blueprints. `EffigyFactory.CreateInstance` clones the ScriptableObject, deep-copies implicit modifiers, and calls `EffigyAffixGenerator.RollAffixes` so every drop is a fresh roll.
- Use the `displayAlias` field on the blueprint to control the player-facing item name (e.g., `Shape_Fire`); when left empty the system falls back to `effigyName`.
- Affix pools reuse the entire equipment/weapon/jewellery catalog—there are no exclusions—but every modifier is forced to `ModifierScope.Global`.
- Rolled values are scaled to 10% of their item counterparts (including dual-range stats). Descriptions are rebuilt to reflect the scaled numeric values.
- Up to four explicit affixes can appear per effigy with any prefix/suffix mix. Total affix count drives rarity: `0 → Normal`, `1-2 → Magic`, `3-4 → Rare`. Unique effigies skip the generator and rely on their predefined modifiers.
- All runtime effigies retain the same `effigyName` / visuals as their blueprint but store their own affix lists so they persist correctly across scenes/inventory.

### Loot Table Integration
- `RewardType` now includes `Effigy`. When you add a loot row, set `rewardType = Effigy` and assign the blueprint in the `effigyBlueprint` field. The generator clones and rolls it with `EffigyFactory`.
- Loot rewards flagged as effigies are stored on the active character (`Character.ownedEffigies`) so equipment/storage screens can rebuild their UI directly from the save state.
- Existing boss/encounter tables only need their drop entries updated—no extra scripts required. Configure the drop chance like any other `LootEntry`; rarities are decided at roll time.
- For area-driven drops, `AreaLootManager` now exposes a ready-made Act binding list (Act 1/2/3) and an effigy drop array inside each `AreaLootTable`. Fill the act loot tables and set per-act effigy chances—area levels automatically lerp between the min/max drop rates you provide.
- Encounters can now include an `actNumber` inside `EncounterDataAsset`, making it easy to categorize drops/UI by story progression or to feed act-specific loot tables.

## Current Limitations & TODOs

1. **Visual Feedback**: Placement preview while dragging needs enhancement
2. **Rotation**: Shapes cannot be rotated (future feature)
3. **Inventory Integration**: Need to connect to inventory/stash system
4. **Save/Load**: Effigy placements not persisted yet
5. **Tooltips**: Need tooltips showing effigy stats

## Switching to UGUI

If you want to switch to GameObject-based UI:

### Pros:
- More tutorials/examples available
- Better editor support for complex interactions
- Easier drag-and-drop implementation

### Cons:
- More GameObjects (performance)
- Less flexible styling

### Migration Path:
1. Create Canvas for Effigy grid
2. Use `GridLayoutGroup` for 6x4 layout
3. Implement `IDragHandler`, `IDropHandler` interfaces
4. Follow similar patterns to your inventory drag-drop

## Next Steps

1. **Create Test Effigies**: Make a few different shapes to test
2. **Add Visual Polish**: Improve colors, borders, hover effects
3. **Connect Inventory**: Allow dragging from inventory to grid
4. **Add Validation UI**: Show why placement failed
5. **Implement Save System**: Persist effigy placements

## Example: Creating a Simple Health Effigy

1. Create Effigy asset: "Small_Health_Boost"
2. Set `shapeWidth = 1`, `shapeHeight = 1`
3. Set `shapeMask = [true]` (single cell)
4. Create Affix:
   - Name: "Health Boost"
   - Modifier: `maximumLife` (+20 to +50)
5. Add Affix to `modifiers` list
6. Set `requiredLevel = 1`
7. Place in grid at (0, 0)

The system is now ready to use! Start by creating a few test effigies to see how the puzzle mechanics work.






