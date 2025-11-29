# MazeNodeButton Prefab Restructure Guide

## Goal
Restructure the prefab so the **Button component is on the root GameObject** instead of nested in a child. This will fix click detection issues.

## New Prefab Structure

```
MazeNodeButton (Root GameObject)
├── Button Component (on root) ← NEW: Button here instead of in child
├── RectTransform
├── Background (Child GameObject)
│   ├── Image Component
│   └── RectTransform
│   └── Sprite: Grid Tile (from MazeConfig)
└── Icon (Child GameObject)
    ├── Image Component
    └── RectTransform
    └── Sprite: Node Icon (Start, Combat, etc.)
```

## Steps to Restructure

### Option 1: Create New Prefab (Recommended)

1. **Create a new GameObject** in the scene (not from prefab)
   - Name it: `MazeNodeButton_New`

2. **Add Components to Root:**
   - Add `RectTransform` (should be automatic)
   - Add `Button` component
   - Set Button's `Target Graphic` to the Background Image (see step 3)

3. **Create Background Child:**
   - Create child GameObject named `Background`
   - Add `Image` component
   - Set `Raycast Target` = **FALSE** (so clicks pass through to Button)
   - This will display the grid tile sprite

4. **Create Icon Child:**
   - Create child GameObject named `Icon`
   - Add `Image` component
   - Set `Raycast Target` = **FALSE** (so clicks pass through to Button)
   - This will display the node type icon

5. **Set up RectTransforms:**
   - Root: Size (100, 100) or your desired size
   - Background: Anchor to stretch (0,0,0,0), sizeDelta (0,0)
   - Icon: Anchor to center (0.5, 0.5), size (95, 95) or similar

6. **Configure Button:**
   - Set `Target Graphic` = Background's Image component
   - Set `Interactable` = true
   - Configure colors/transitions as needed

7. **Save as Prefab:**
   - Drag `MazeNodeButton_New` to `Assets/Prefab/Maze/` folder
   - Delete the instance from scene
   - Update `MazeMinimapUI.nodeButtonPrefab` to use the new prefab

### Option 2: Modify Existing Prefab

1. **Open the existing prefab** (`Assets/Prefab/Maze/MazeNodeButton.prefab`)

2. **Move Button Component:**
   - Select the root GameObject (`MazeNodeButton`)
   - Add `Button` component to root
   - Copy settings from the Button child's Button component
   - Delete the Button child GameObject (or remove its Button component)

3. **Update Background:**
   - Ensure `Background` child exists
   - Set its Image's `Raycast Target` = **FALSE**

4. **Update Icon:**
   - Ensure `Icon` child exists (may need to move from Button/Icon to root/Icon)
   - Set its Image's `Raycast Target` = **FALSE**

5. **Set Button Target Graphic:**
   - On root's Button component, set `Target Graphic` = Background's Image

6. **Save the prefab**

## Code Support

The code has been updated to support **both structures**:
- **New structure**: Button on root → Code will find it automatically
- **Old structure**: Button in child → Code will fall back to finding it in "Button" child

## Testing

After restructuring:
1. Assign the new prefab to `MazeMinimapUI.nodeButtonPrefab`
2. Run the scene
3. Click on selectable nodes
4. You should see console logs:
   - `[MazeMinimapUI] Button onClick triggered...`
   - `[MazeNodeButtonDebugger] OnPointerClick detected...`

## Troubleshooting

If clicks still don't work:
1. Check that `Background` Image has `Raycast Target` = **FALSE**
2. Check that `Icon` Image has `Raycast Target` = **FALSE**
3. Check that Button's `Target Graphic` is set to Background's Image
4. Check that Button's Image (Background) has alpha > 0.01
5. Verify EventSystem exists in scene
6. Verify Canvas has GraphicRaycaster component



