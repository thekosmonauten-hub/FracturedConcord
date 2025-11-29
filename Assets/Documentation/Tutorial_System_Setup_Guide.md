# Tutorial System Setup Guide

## Overview

The Tutorial System allows you to create guided tutorials that can:
- Highlight UI elements (panels, buttons, sockets, etc.)
- Display tutorial text
- Progress through multiple steps
- **Persist across scene transitions** (e.g., start in Town Scene, continue in WarrantTree scene)
- Wait for player interaction or automatically advance

## Quick Start: Cross-Scene Tutorial

### Step 1: Create Tutorial Data Asset

1. In Unity, right-click in `Assets/Resources/Tutorials/` (create folder if needed)
2. Select **Create > Dexiled > Tutorial Data**
3. Name it `WarrantTutorial` (or your preferred name)
4. Set the **Tutorial ID** to `warrant_tutorial` (this is what you'll reference in dialogue)

### Step 2: Configure Tutorial Steps

For your Joreg tutorial that transitions from Town Scene to WarrantTree:

#### Step 1: Highlight Warrant Locker (in Town Scene)
- **Step ID**: `highlight_locker`
- **Tutorial Text**: "This is your Warrant Locker. Joreg has given you 3 warrants to start with."
- **Target Object Path**: `WarrantLockerGrid` (or the actual GameObject name)
- **Completion Type**: `WaitForTime` (2 seconds)
- **On Start Action**: None
- **On Complete Action**: None

#### Step 2: Transition to WarrantTree Scene
- **Step ID**: `transition_scene`
- **Tutorial Text**: "Let's move to the Warrant Tree to see how to socket warrants."
- **Target Object**: None (no highlight)
- **Completion Type**: `WaitForTime` (1 second)
- **On Start Action**: 
  - **Action Type**: `Custom`
  - **Action Value**: `transition_to_warrant_tree`
- **On Complete Action**: None

> **Note**: The actual scene transition will be handled by the dialogue action, not the tutorial step.

#### Step 3: Highlight First Socket (in WarrantTree Scene)
- **Step ID**: `highlight_socket_1`
- **Tutorial Text**: "This is a Warrant Socket. Drag a warrant from your locker to socket it here."
- **Target Object Path**: Find the first socket GameObject name (e.g., `Socket_Anchor` or similar)
- **Completion Type**: `ClickTarget` (wait for player to socket a warrant)
- **Click Target Path**: Same as target object
- **On Start Action**: None
- **On Complete Action**: None

#### Step 4: Highlight More Sockets
- **Step ID**: `highlight_socket_2`
- **Tutorial Text**: "You can socket warrants in multiple sockets. Each warrant provides different modifiers."
- **Target Object Path**: Another socket GameObject
- **Completion Type**: `Manual` (show "Next" button)
- **On Start Action**: None
- **On Complete Action**: None

#### Step 5: Highlight Warrant Tree Panel
- **Step ID**: `highlight_tree_panel`
- **Tutorial Text**: "The Warrant Tree shows all available sockets. Unlock more by progressing through the game."
- **Target Object Path**: The main Warrant Tree panel GameObject
- **Completion Type**: `Manual`
- **On Start Action**: None
- **On Complete Action**: None

### Step 3: Set Up Dialogue Action

In your Joreg dialogue asset (`Peacekeeper Joreg.asset`):

1. Find the node `warrant_tutorial_start`
2. Set the **On Exit Actions** (list):
   - Click the **+** button to add actions to the list
   - **Action 1**:
     - **Action Type**: `TransitionScene`
     - **Action Value**: `WarrantTree` (the scene name)
   - **Action 2**:
     - **Action Type**: `StartTutorial`
     - **Action Value**: `warrant_tutorial` (matches your Tutorial Data asset ID)
   
   > **Note**: Actions in the list execute in order. The scene transition happens first, then the tutorial starts automatically in the new scene.
   
   **Alternative (if you prefer single action)**: You can still use the single `On Exit Action` field, but you'll need to create a separate dialogue node for the tutorial start, or handle it differently.

> **Important**: The scene transition happens first, then the tutorial resumes in the new scene.

### Step 4: Create Tutorial UI in Both Scenes

You need a Tutorial Panel in both Town Scene and WarrantTree scene:

1. **Create Tutorial Panel**:
   - Create a Canvas if one doesn't exist (or use existing UI Canvas)
   - Create a GameObject: `TutorialPanel`
   - Add a `RectTransform` (full screen or positioned as desired)
   - Add a `CanvasGroup` component
   - Add a background `Image` (semi-transparent or solid)

2. **Add Tutorial Text**:
   - Create child GameObject: `TutorialText`
   - Add `TextMeshProUGUI` component
   - Set font, size, color as desired
   - Position it (e.g., bottom center of screen)

3. **Add Buttons**:
   - Create child GameObject: `NextButton`
   - Add `Button` component
   - Add `TextMeshProUGUI` child for button text ("Next" or "Continue")
   - Create child GameObject: `SkipButton` (optional)
   - Add `Button` component
   - Add `TextMeshProUGUI` child for button text ("Skip")

4. **Assign to TutorialManager**:
   - Find or create `TutorialManager` GameObject in scene
   - Assign `TutorialPanel` to **Tutorial Panel** field
   - Assign `TutorialText` to **Tutorial Text** field
   - Assign `NextButton` to **Next Button** field
   - Assign `SkipButton` to **Skip Button** field (if created)

### Step 5: Finding GameObject Paths

To find the correct paths for warrant sockets and panels:

#### For Warrant Sockets (Recommended Method)

Since all warrant sockets are dynamically created with the same name ("WarrantNode_Socket(clone)"), you should use the **node ID from the graph definition**:

1. **Open `MasterWarrantGraph.asset`** (or your graph asset)
2. **Find the node ID** you want to highlight (e.g., `Anchor`, `TopLeft`, `Anchor_BottomLeft_Section1_Socket`)
3. **Use the format**: `WarrantNodeView:NodeId`
   - Example: `WarrantNodeView:Anchor`
   - Example: `WarrantNodeView:Anchor_BottomLeft_Section1_Socket`
   - Example: `WarrantSocketView:Center` (also works)

The system will automatically find the GameObject with a `WarrantNodeView` component that has a matching `NodeId`.

#### For Regular GameObjects

1. **In Unity Editor**:
   - Open the scene
   - Select the GameObject you want to highlight
   - Note its name in the Hierarchy
   - Use that name as the `targetObjectPath` (e.g., `MyButton`)

2. **For Hierarchical Paths**:
   - Use forward slashes: `Canvas/Panel/Button`
   - The system will search down the hierarchy

#### Supported Path Formats

- **Component Property**: `WarrantNodeView:Anchor` (finds by component property)
- **GameObject Name**: `MyButton` (finds by GameObject name)
- **Hierarchical Path**: `Canvas/Panel/Button` (finds by path)

### Step 6: Testing

1. Start the game in Town Scene
2. Talk to Joreg
3. Trigger the `warrant_tutorial_start` dialogue node
4. Verify:
   - Scene transitions to WarrantTree
   - Tutorial panel appears
   - First step highlights the correct socket
   - Tutorial text displays correctly
   - Clicking/advancing moves to next step

## Advanced: Custom Highlight Overlay

If you want a custom highlight overlay (instead of the default):

1. Create a prefab with:
   - `RectTransform` (anchored to fill parent)
   - `Image` component with your highlight sprite/color
   - Optional: Animation components for pulse effects

2. Assign the prefab to `TutorialManager` > **Highlight Overlay Prefab**

## Troubleshooting

### Tutorial doesn't resume after scene transition
- Check that `TutorialManager` has `DontDestroyOnLoad` (it should automatically)
- Verify the tutorial ID matches between dialogue action and TutorialData asset
- Check console for errors about missing TutorialData

### Highlight doesn't appear
- Verify the `targetObjectPath` matches the GameObject name exactly
- Check that the GameObject exists in the scene
- Ensure the GameObject has a `RectTransform` (for UI elements)

### Tutorial text doesn't show
- Verify `TutorialPanel` is assigned in TutorialManager
- Check that `TutorialText` is assigned and is a child of `TutorialPanel`
- Ensure the panel is active (TutorialManager activates it automatically)

### Scene transition doesn't work
- Verify scene name in dialogue action matches the actual scene name in Build Settings
- Check that `TransitionManager` exists in the scene (optional, but recommended)
- Ensure `DialogueManager` is set up correctly

## Example: Complete Tutorial Data Setup

```
Tutorial ID: warrant_tutorial
Tutorial Name: Warrant System Tutorial
Can Skip: false
Auto Start: false

Steps:
1. Step ID: intro_locker
   Text: "This is your Warrant Locker..."
   Target: WarrantLockerGrid
   Completion: WaitForTime (2s)

2. Step ID: transition_note
   Text: "Now let's see how to use warrants..."
   Target: None
   Completion: WaitForTime (1s)

3. Step ID: highlight_socket
   Text: "Drag a warrant here to socket it..."
   Target: Socket_Anchor
   Completion: ClickTarget

4. Step ID: explain_modifiers
   Text: "Each warrant provides modifiers..."
   Target: None
   Completion: Manual
```

## Integration with Dialogue System

The tutorial system integrates seamlessly with the dialogue system:

1. **Start Tutorial from Dialogue**:
   - Use `StartTutorial` action type
   - Set `actionValue` to tutorial ID

2. **Transition Scene from Dialogue**:
   - Use `TransitionScene` action type
   - Set `actionValue` to scene name
   - Tutorial will automatically resume in new scene

3. **Give Warrants from Dialogue**:
   - Use `GiveItem` action (when implemented)
   - Or use custom action to call `WarrantRollingUtility`

## Next Steps

- Create your `WarrantTutorial` TutorialData asset
- Set up tutorial steps for your specific UI
- Test the cross-scene flow
- Customize highlight colors and animations as needed

