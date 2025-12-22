# StackIcon Setup Guide

## Overview

The `StackIcon` component is designed specifically for displaying stack counts (Momentum, Agitate, Tolerance, Potential, etc.) in the combat UI. It provides a clean, dedicated solution for stack visualization.

## Quick Setup

### Option 1: Auto-Setup (Recommended)

1. Add `StackDisplayManager` component to your scene (on the same GameObject as `StatusEffectBar` or create a new GameObject)
2. Assign the `StackContainer` Transform in the Inspector
3. Right-click the `StackDisplayManager` component → **"Setup Stack GameObjects"**
4. This will automatically create all stack GameObjects with the proper visual structure

### Option 2: Manual Setup

For each stack type (Momentum, Agitate, Tolerance, Potential, etc.):

1. **Create GameObject** in `StackContainer`:
   - Name it after the stack type (e.g., "Momentum", "Agitate")
   - Add `RectTransform` component (should be automatic)
   - Set size to 48x48 pixels (or your preferred size)

2. **Add StackIcon Component**:
   - Add `StackIcon` component to the GameObject
   - The component will auto-find child components, or you can assign them manually

3. **Create Visual Structure** (if not auto-created):
   - **Background** (child GameObject):
     - Name: "Background"
     - Add `Image` component
     - Set anchors to fill parent (0,0 to 1,1)
     - Color: Green with transparency (0, 1, 0, 0.3)
   
   - **Icon** (child GameObject):
     - Name: "Icon"
     - Add `Image` component
     - Set anchors to 0.1, 0.1 to 0.9, 0.9 (10% padding)
     - This will display the stack type icon/sprite
   
   - **CountText** (child GameObject):
     - Name: "CountText"
     - Add `TextMeshProUGUI` component
     - Set anchors to fill parent (0,0 to 1,1)
     - Alignment: Center
     - Font Size: 18
     - Color: White
     - This displays the stack count number

## Prefab Structure

```
StackContainer
├── Momentum
│   ├── Background (Image)
│   ├── Icon (Image)
│   └── CountText (TextMeshProUGUI)
├── Agitate
│   ├── Background (Image)
│   ├── Icon (Image)
│   └── CountText (TextMeshProUGUI)
├── Tolerance
│   ├── Background (Image)
│   ├── Icon (Image)
│   └── CountText (TextMeshProUGUI)
└── Potential
    ├── Background (Image)
    ├── Icon (Image)
    └── CountText (TextMeshProUGUI)
```

## Component Auto-Detection

The `StackIcon` component automatically finds child components by name:
- **Background**: Looks for "Background" child or uses the root Image
- **Icon**: Looks for "Icon" child or any Image that's not the background
- **CountText**: Looks for "CountText", "Value", "Magnitude", "Amount", or "Text" child

## Customization

### Stack Sprites

Place stack type sprites in:
- `Resources/UI/Stacks/{StackType}.png` (e.g., `Resources/UI/Stacks/Momentum.png`)
- Or `Resources/Stacks/{StackType}.png`

If no sprite is found, the icon will use a colored square based on stack type:
- **Momentum**: Orange-yellow
- **Agitate**: Red
- **Tolerance**: Light blue
- **Potential**: Purple

### Colors

Modify the `stackBackgroundColor` in `StackIcon` component or `StackDisplayManager` to change the background color for all stacks.

## Adding New Stack Types

1. Add the new type to `StackType` enum in `StackType.cs`
2. Create a GameObject in `StackContainer` named after the enum value
3. Add `StackIcon` component
4. The system will automatically detect and update it!

## Troubleshooting

### Stacks not showing:
- Check that `StackDisplayManager` is in the scene
- Verify `StackContainer` is assigned
- Ensure stack GameObjects are named correctly (must match `StackType` enum)
- Check Console for `[StackDisplayManager] Found stack GameObject: ...` messages

### Visual elements missing:
- Use the "Setup Stack GameObjects" context menu option
- Or manually create the Background, Icon, and CountText children

### Count not updating:
- Ensure `StackSystem.Instance` is available
- Check that stacks are being added via `StackSystem.Instance.AddStacks()`
- Verify the `StackIcon` component is on the GameObject

