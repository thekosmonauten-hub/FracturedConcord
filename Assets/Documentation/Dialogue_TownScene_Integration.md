# Dialogue System - TownScene Integration Guide

## Overview

This guide shows how to integrate the dialogue system with your existing TownScene NPC panel system where:
- Buttons activate panels (e.g., "Peacekeepers" button → `PeacekeepersFactionPanel`)
- NPCs inside panels should be clickable to start dialogue

## Current Setup

Your TownScene structure:
```
TownScene
├── Canvas (or MainCanvas/UICanvas)
│   └── DialoguePanel (CHILD OF CANVAS - REQUIRED!)
│       └── [DialogueUI component]
├── Menus
│   ├── PeacekeepersFactionPanel (deactivated)
│   ├── SeerPanel (deactivated)
│   ├── ForgePanel (deactivated)
│   └── QuestgiverPanel (deactivated)
└── Interactables
    ├── PeacekeepersFaction
    │   └── Peacekeepers (Button) → Activates PeacekeepersFactionPanel
    ├── Seer (Button) → Activates SeerPanel
    ├── Forge (Button) → Activates ForgePanel
    └── Questgiver (Button) → Activates QuestgiverPanel
```

**IMPORTANT:** The `DialoguePanel` MUST be a direct child of `Canvas` (or a child of a child of Canvas). It cannot be a root object!

## Integration Steps

### Step 1: Add DialogueManager to Scene

1. **Create DialogueManager GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Rename to `DialogueManager`
   - Add Component: `DialogueManager`
   - The DialogueManager is a singleton with `DontDestroyOnLoad`, so it persists across scenes

2. **Auto-Find DialogueUI:**
   - DialogueManager will automatically find `DialogueUI` in the scene when dialogue starts
   - Or you can manually assign it in the inspector

### Step 2: Add DialogueUI to Scene

**CRITICAL: DialoguePanel MUST be a child of a Canvas!**

**Option A: Use Prefab (Recommended)**

1. **Find or Create a Canvas in TownScene:**
   - Look in your Hierarchy for a Canvas GameObject
   - If you don't have one, create it:
     - Right-click in Hierarchy → UI → Canvas
     - This creates: `Canvas` (with Canvas, CanvasScaler, GraphicRaycaster components)
     - Rename it to `MainCanvas` or `UICanvas` if you want

2. **Instantiate DialoguePanel Prefab:**
   - Drag `Assets/Prefab/Dialogue/DialoguePanel.prefab` into the scene
   - **IMPORTANT:** In the Hierarchy, drag the `DialoguePanel` GameObject so it becomes a **CHILD** of the Canvas
   - The hierarchy should look like:
     ```
     Canvas
     └── DialoguePanel
         ├── Background
         ├── DialogueContainer
         │   ├── Header
         │   ├── DialogueText
         │   ├── Choices
         │   └── Buttons
         └── [DialogueUI component on DialoguePanel]
     ```
   - Position it where you want (it will be hidden by default)
   - The DialogueUI component will auto-find the DialogueManager

3. **Verify Setup:**
   - Select `DialoguePanel` in Hierarchy
   - In Inspector, check that `DialoguePanel` has a parent (should show `Canvas` as parent)
   - Check that `DialoguePanel` is **active** (checkbox at top of Inspector is checked)
   - Check that the parent `Canvas` is also **active**

**Option B: Create in Scene**

1. Follow the prefab creation guide to build the UI hierarchy directly in the scene
2. **Make sure it's under a Canvas** - drag it as a child of Canvas in Hierarchy

### Step 3: Add NPCInteractable to NPCs

For each NPC inside the panels (e.g., Joreg in PeacekeepersFactionPanel):

1. **Select the NPC GameObject** (e.g., `Joreg` inside `PeacekeepersFactionPanel`)
2. **Add Component: `NPCInteractable`**
3. **Configure NPCInteractable:**
   - **NPC Id**: `joreg` (or your NPC ID)
   - **NPC Name**: `Joreg` (display name)
   - **NPC Portrait**: Assign the portrait sprite (optional)
   - **Dialogue Data**: Assign your `Peacekeeper Joreg.asset` ScriptableObject
   - **Hover Indicator**: (Optional) GameObject to show on hover
   - **Show Hover Effect**: ✓ (if you want hover feedback)

4. **Ensure NPC is Clickable:**
   - The NPC GameObject needs an `Image` component (NPCInteractable adds one automatically if missing)
   - Make sure the NPC is a child of a Canvas
   - The panel must be active when you want to click the NPC

### Step 4: Verify Panel Activation

Make sure your button click handlers activate the panels correctly:

**Example Button Script:**
```csharp
public class ActivatePanelButton : MonoBehaviour
{
    [SerializeField] private GameObject targetPanel;
    
    public void OnButtonClick()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }
}
```

Or if using Unity Button component directly:
- Assign the panel to the Button's `OnClick()` event
- Call `GameObject.SetActive(true)` on the panel

### Step 5: Test the Flow

1. **Start the game in TownScene**
2. **Click "Peacekeepers" button** → `PeacekeepersFactionPanel` activates
3. **Click Joreg NPC** inside the panel → Dialogue should start
4. **Verify:**
   - Dialogue panel appears
   - Speaker name shows "Joreg"
   - Dialogue text displays
   - Continue/Close buttons work

## Troubleshooting

### Dialogue doesn't start when clicking NPC

**Check:**
1. ✅ Is `NPCInteractable` component on the NPC GameObject?
2. ✅ Is `DialogueData` assigned in NPCInteractable?
3. ✅ Is the panel active? (NPCInteractable needs the GameObject to be active)
4. ✅ Is there a Canvas in the hierarchy? (needed for UI raycasting)
5. ✅ Is `DialogueManager` in the scene?
6. ✅ Is `DialogueUI` in the scene (or prefab instantiated)?

**Debug:**
- Check Console for errors
- Add Debug.Log in `NPCInteractable.StartDialogue()` to verify it's being called
- Check if `DialogueManager.Instance` is null

### Dialogue panel doesn't appear

**Check:**
1. ✅ Is `DialogueUI` component in the scene?
2. ✅ Is the DialoguePanel GameObject active?
3. ✅ **Is DialoguePanel a CHILD of Canvas?** (Check Hierarchy - DialoguePanel should be indented under Canvas)
4. ✅ Is the Canvas GameObject active?
5. ✅ Are all UI references assigned in DialogueUI inspector?
6. ✅ Check Console for "DialogueUI not found" warnings

**Debug:**
- DialogueManager logs warnings if DialogueUI is missing
- Check `DialogueManager.dialogueUI` field in inspector
- **If you see "NO CANVAS FOUND IN PARENT CHAIN!"** → DialoguePanel is NOT a child of Canvas!

**Fix:**
1. Select `DialoguePanel` in Hierarchy
2. Drag it so it becomes a child of `Canvas`
3. The Hierarchy should show:
   ```
   Canvas
   └── DialoguePanel  ← Should be indented under Canvas
   ```
4. NOT like this (wrong):
   ```
   Canvas
   DialoguePanel  ← Same level as Canvas (WRONG!)
   ```

### NPC not clickable

**Check:**
1. ✅ Does NPC have `Image` component? (NPCInteractable adds one automatically)
2. ✅ Is `Image.raycastTarget` enabled?
3. ✅ Is the NPC's panel active?
4. ✅ Is there an EventSystem in the scene? (required for UI clicks)
5. ✅ Is the NPC behind other UI elements? (check Canvas sorting order)
6. ✅ Is the NPC under a Canvas?
7. ✅ Does the Canvas have a GraphicRaycaster component?

**Debug:**
- Check Console for debug messages from NPCInteractable
- NPCInteractable now logs warnings if EventSystem/Canvas/GraphicRaycaster are missing
- Look for `[NPCInteractable] OnPointerClick called` in console when clicking

**Quick Fix - Use Button Component Instead:**

If IPointerClickHandler isn't working, you can use a Button:

1. **Add Button Component:**
   - Select PeacekeeperJoreg GameObject
   - Add Component: `Button`
   - Set Button's Transition to "None" (if you don't want button visual effects)

2. **Wire Up Button:**
   - In Button's `OnClick()` event (bottom of Button component)
   - Click the **+** button to add a listener
   - Drag PeacekeeperJoreg GameObject to the object field
   - Select: `NPCInteractable` → `OnButtonClick()`

3. **Remove Image if Needed:**
   - If you have a visible Image component you don't want, you can remove it
   - The Button will still work (Button has its own Graphic component)
   - Or keep the Image and set Button's Target Graphic to it

### Panel activation conflicts with dialogue

If panels are deactivating when dialogue starts:

**Solution:**
- Dialogue panel should be separate from NPC panels
- NPC panels can stay active while dialogue is open
- Dialogue panel appears on top (higher Canvas sorting order)

## Example: Complete Joreg Setup

```
PeacekeepersFactionPanel (GameObject)
├── Background (Image)
├── Header (TextMeshProUGUI) - "Peacekeepers Faction"
└── Joreg (GameObject)
    ├── Image (Component) - NPC sprite/portrait
    ├── NPCInteractable (Component)
    │   ├── NPC Id: "joreg"
    │   ├── NPC Name: "Joreg"
    │   ├── NPC Portrait: [Joreg Portrait Sprite]
    │   └── Dialogue Data: [Peacekeeper Joreg.asset]
    └── HoverIndicator (GameObject, optional)
        └── Image - Highlight effect
```

## Next Steps

1. ✅ Set up DialogueManager in TownScene
2. ✅ Add DialogueUI (prefab or in-scene)
3. ✅ Add NPCInteractable to Joreg
4. ✅ Assign Joreg's DialogueData asset
5. ✅ Test clicking Joreg → Dialogue starts
6. ✅ Repeat for other NPCs (Seer, Forge, Questgiver)

## Integration with Existing Systems

The dialogue system is designed to work alongside your existing panel system:

- **Panels activate** → NPCs become visible
- **Click NPC** → Dialogue starts (panel can stay open)
- **Dialogue appears** → On top of everything (high Canvas order)
- **Close dialogue** → Returns to panel view

No changes needed to your existing panel activation system!

