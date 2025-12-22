# Dynamic Recipe System

The Forge system now supports dynamic recipes that automatically show craftable items based on the player's level.

## How It Works

Instead of creating many individual recipes, you can create **one recipe per base type** (e.g., "Body Armour", "Weapons", etc.) that dynamically shows items available for the player's current level.

### Recipe Configuration

1. **Create a Recipe** with the following settings:
   - Set `itemType` to the type you want to craft (Weapon, Armour, Accessory)
   - Enable `filterByEquipmentType` and set `equipmentType` if you want to filter by a specific equipment type (e.g., Body Armour only)
   - Set `craftRandomItem = true` to enable dynamic item selection
   - Set `levelRangeAbovePlayer` to control how many levels above the player's level to show items (default: 5)

2. **Example Recipe Settings**:
   ```
   Recipe Name: "Body Armour"
   Item Type: Armour
   Filter By Equipment Type: true
   Equipment Type: Body Armour
   Craft Random Item: true
   Level Range Above Player: 5
   ```

### Item Selection

When a recipe is selected:
1. The system queries `ItemDatabase` for items matching the recipe's criteria
2. Items are filtered by:
   - Item type (Weapon/Armour/Accessory)
   - Equipment type (if `filterByEquipmentType` is enabled)
   - Required level ≤ (player level + `levelRangeAbovePlayer`)
3. Items are displayed in a selectable list in the crafting output area
4. The player can click on an item to preview it and select it for crafting

### Crafting Process

1. Player selects a recipe from the recipe list
2. Available items appear in the crafting output area
3. Player selects an item from the list (first item is auto-selected)
4. Item preview/tooltip is shown
5. Player clicks "Craft" button
6. Selected item is crafted with the recipe's specified rarity and affixes

### Code Implementation

The key methods are:

- `CraftingRecipe.GetEligibleItemsForLevel(int characterLevel)` - Returns items available for crafting at the given level
- `ForgeCraftingOutputUI.ShowRecipePreview(CraftingRecipe recipe)` - Shows available items when recipe is selected
- `CraftingRecipe.selectedItemToCraft` - Stores the selected item for crafting

### Example: Body Armour Recipe

At **Level 1**:
- Shows: Plate Vest (requiredLevel 0), Chestplate (requiredLevel 6)

At **Level 10**:
- Shows: Chestplate (requiredLevel 6), Copper Plate (requiredLevel 17)

At **Level 17**:
- Shows: Copper Plate (requiredLevel 17), War Plate (requiredLevel 21)

This is based on the items' `requiredLevel` field and the `levelRangeAbovePlayer` setting (5 levels above player level).

### UI Setup

To enable item selection UI:

1. **ForgeCraftingOutputUI** component needs:
   - `itemSelectionScrollRect` - ScrollRect for the item list (optional)
   - `itemSelectionContainer` - Transform to parent item selection buttons
   - `itemSelectionButtonPrefab` - Prefab for item selection buttons (should have Button + TextMeshProUGUI)

2. If item selection UI components are not assigned, the system will still work but won't show the item list buttons (first item will be auto-selected).

