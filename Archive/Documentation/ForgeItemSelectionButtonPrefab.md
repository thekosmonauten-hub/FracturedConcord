# Forge Item Selection Button Prefab Guide

This guide shows how to create the `itemSelectionButtonPrefab` for the Forge crafting system. This prefab displays items available for crafting with detailed information.

## Required Component

The prefab **must** have a `Button` component and **should** have a `ForgeItemSelectionButton` component for full functionality.

## Recommended Prefab Structure

```
ItemSelectionButton (GameObject)
├── Button Component
├── Image Component (Background)
├── ForgeItemSelectionButton Component
│
├── IconImage (Image)
│   └── Item icon sprite
│
├── NameText (TextMeshProUGUI)
│   └── Item name (colored by rarity)
│
├── TypeText (TextMeshProUGUI) [Optional]
│   └── Equipment type (e.g., "Body Armour", "Helmet", "Sword")
│
├── StatsText (TextMeshProUGUI) [Optional]
│   └── Defense/Damage values
│   └── For Armour: "Armour: 100 | Evasion: 50 | ES: 25"
│   └── For Weapons: "Damage: 50"
│
├── RequirementsText (TextMeshProUGUI) [Optional]
│   └── Attribute requirements: "Str: 25 / Dex: 10 / Int: 5"
│   └── Or "No Req." if no requirements
│
├── LevelText (TextMeshProUGUI) [Optional]
│   └── "Lv. 10" (required level)
│
├── ImplicitText (TextMeshProUGUI) [Optional]
│   └── Implicit modifiers: "+15% increased Physical Damage"
│   └── Multiple implicits joined with " | " separator
│
└── SelectedHighlight (Image) [Optional]
    └── Visual indicator when item is selected
```

## Component Setup

### ForgeItemSelectionButton Component

The `ForgeItemSelectionButton` component automatically populates the UI elements if they follow the naming conventions above. You can also manually assign references in the inspector.

**Inspector Fields:**
- `Item Icon` - Image component for the item icon
- `Item Name Text` - TextMeshProUGUI for item name
- `Equipment Type Text` - TextMeshProUGUI for equipment type
- `Stats Text` - TextMeshProUGUI for defense/damage values
- `Requirements Text` - TextMeshProUGUI for attribute requirements
- `Level Text` - TextMeshProUGUI for required level
- `Implicit Text` - TextMeshProUGUI for implicit modifiers
- `Background Image` - Image component for button background
- `Selected Highlight` - Image component for selection indicator

### Visual Settings

The component includes color settings:
- `Normal Color` - Default button color (default: Dark gray)
- `Selected Color` - Color when item is selected (default: Light green)
- `Unselectable Color` - Color when item cannot be selected (default: Dark gray, semi-transparent)

## Example Layout (Recommended)

### Simple Layout (Minimal)

```
ItemSelectionButton
├── Background (Image) - Stretch to fill
├── Horizontal Layout Group
│   ├── IconImage (40x40 pixels)
│   ├── Vertical Layout Group
│   │   ├── NameText
│   │   └── LevelText (smaller font)
```

### Detailed Layout (Recommended)

```
ItemSelectionButton (Width: 200-300px, Height: 80-100px)
├── Background (Image)
├── Layout Group (Horizontal or Vertical)
│   ├── IconImage (60x60px, left side)
│   ├── Content (Vertical Layout Group, flexible width)
│   │   ├── NameText (Bold, 16-18pt)
│   │   ├── TypeText (Regular, 12pt, gray)
│   │   ├── StatsText (Regular, 12pt, white)
│   │   ├── RequirementsText (Regular, 11pt, light gray)
│   │   └── ImplicitText (Regular, 11pt, light blue/cyan)
│   └── LevelText (Right side, 14pt, yellow/gold)
└── SelectedHighlight (Image, Stretch to fill, semi-transparent overlay)
```

## Displayed Information

### For Armour Items

- **Icon**: Item icon sprite
- **Name**: Item name (color coded by rarity)
  - Normal: White
  - Magic: Blue
  - Rare: Yellow/Gold
  - Unique: Purple
- **Equipment Type**: Armour slot (Body Armour, Helmet, Gloves, Boots, etc.)
- **Stats**: Defense values
  - "Armour: 100 | Evasion: 50 | ES: 25" (shows only non-zero values)
- **Requirements**: Attribute requirements
  - "Str: 25 / Dex: 10 / Int: 5" or "No Req."
- **Level**: Required level (e.g., "Lv. 10")
- **Implicit Modifiers**: Item implicit modifiers
  - "+15% increased Physical Damage"
  - Multiple implicits: "+15% Physical | +10% Attack Speed"

### For Weapon Items

- **Icon**: Item icon sprite
- **Name**: Item name (color coded by rarity)
- **Equipment Type**: Weapon type (Sword, Axe, Bow, etc.)
- **Stats**: Average damage
  - "Damage: 50"
- **Requirements**: Attribute requirements
- **Level**: Required level

### For Jewellery Items

- **Icon**: Item icon sprite
- **Name**: Item name (color coded by rarity)
- **Equipment Type**: Jewellery type (Ring, Amulet, etc.)
- **Stats**: Usually empty (no base stats)
- **Requirements**: Attribute requirements (if any)
- **Level**: Required level
- **Implicit Modifiers**: Item implicit modifiers (if any)

## Auto-Assign Feature

The `ForgeItemSelectionButton` component includes an auto-assign feature (Editor context menu):
1. Right-click the component in the Inspector
2. Select "Auto-Assign References"
3. The component will try to find child objects with common names:
   - `IconImage` or `Icon` → Item icon
   - `NameText` or `ItemName` → Item name
   - `TypeText` or `EquipmentType` → Equipment type
   - `StatsText` or `Stats` → Stats
   - `RequirementsText` or `Requirements` → Requirements
   - `LevelText` or `Level` → Level
   - `ImplicitText` or `Implicit` or `ImplicitModifiers` → Implicit modifiers
   - `SelectedHighlight` → Selection highlight

## Fallback Behavior

If the prefab doesn't have a `ForgeItemSelectionButton` component, the system will fall back to simple text display:
- Finds first `TextMeshProUGUI` in children
- Sets text to: `"{itemName} (Lv. {requiredLevel})"`
- Button click still works for item selection

## Usage in Forge Scene

1. Create the prefab following the structure above
2. Add the `ForgeItemSelectionButton` component
3. Assign the prefab to `ForgeCraftingOutputUI.itemSelectionButtonPrefab`
4. The system will automatically populate and display items when a recipe is selected

## Tips

- Use a consistent button size (recommended: 200-300px wide, 60-100px tall)
- Ensure the button has enough height to display all information clearly
- Use a clear hierarchy with layout groups for easy positioning
- Make the background slightly transparent or use a border for visual separation
- Consider adding hover effects or animations for better UX
- Test with items of different rarities to ensure text colors are visible

