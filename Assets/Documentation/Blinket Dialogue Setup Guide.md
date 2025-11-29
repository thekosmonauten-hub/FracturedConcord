# Blinket Dialogue Setup Guide

## Overview

This guide explains how to set up Blinket the Bargain-Bound's dialogue system for the Maze Vendor. The dialogue plays when the player clicks the vendor button, providing lore and character interaction before opening the shop.

## Dialogue Structure

The dialogue is structured as a tree with the following branches:

### Main Greeting
- **Node ID**: `greeting`
- **Choices**:
  - "Who are you?" → `who_are_you`
  - "What is this place?" → `what_is_place`
  - "Why does your bag keep… twitching?" → `bag_twitching`
  - "Let me see what you're selling." → `open_shop` (opens vendor)
  - "Goodbye." → `goodbye`

### Who Are You Branch
- Explains Blinket's background
- Leads to questions about the Maze or Blinket's nervousness
- Can branch back to shop or goodbye

### What Is This Place Branch
- Explains the Ever-Shifting Emporium
- Discusses how inventory restocks automatically
- Addresses safety concerns
- Can branch back to shop or goodbye

### Shop Opening
- **Node ID**: `open_shop`
- Triggers `OpenShop` action with value "MazeVendor"
- Automatically closes dialogue and opens vendor panel

## Setup Instructions

### Step 1: Generate Dialogue Asset

1. Open Unity Editor
2. Go to menu: **Tools → Dexiled → Create Blinket Dialogue**
3. Verify the paths:
   - **Markdown File**: `Assets/Documentation/Blinket_the_Bargain-Bound_dialogue.md`
   - **Output Path**: `Assets/Resources/Dialogues/Blinket_Dialogue.asset`
4. **Optional**: Set a **Default Speaker Portrait** sprite:
   - Drag a sprite asset to the "Portrait Sprite" field
   - This portrait will be used for all dialogue nodes by default
   - Individual nodes can override this with their own portrait if needed
5. Click **"Generate Dialogue Asset"**
6. The dialogue asset will be created with all nodes and branches

### Step 2: Assign Dialogue to Maze Hub Controller

1. Open your Maze Hub scene (or scene with `MazeHubController`)
2. Select the GameObject with `MazeHubController` component
3. In the Inspector, find the **"Dialogue (Optional)"** section
4. Assign the generated dialogue asset:
   - Drag `Blinket_Dialogue.asset` from `Assets/Resources/Dialogues/` to the **Vendor Dialogue** field
5. Ensure **Use Vendor Dialogue** is checked (default: true)

### Step 3: Ensure DialogueManager Exists

1. The `DialogueManager` will be automatically created if it doesn't exist (singleton pattern)
2. If you want to manually place it:
   - Create an empty GameObject named "DialogueManager"
   - Add the `DialogueManager` component
   - The component will handle dialogue UI automatically

### Step 4: Verify Dialogue UI

The dialogue system requires a `DialogueUI` component in the scene:
- It will be auto-detected by `DialogueManager`
- If missing, create a GameObject with `DialogueUI` component
- Typically placed in a DialogueCanvas or similar UI container

## How It Works

### Flow

1. **Player clicks Vendor Button** in Maze Hub
2. **Dialogue starts** (if `useVendorDialogue` is true and `vendorDialogue` is assigned)
3. **Player selects dialogue options** and branches through conversation
4. **Player clicks "Let me see what you're selling"**
5. **Dialogue closes** and **Vendor Panel opens** automatically

### Opening Shop from Dialogue

When the dialogue reaches the `open_shop` node:
- `DialogueManager` executes the `OpenShop` action
- The action triggers `OpenMazeVendorShop()` method
- This method:
  1. Finds `MazeHubController` in the scene
  2. Calls `ShowPanel(vendorPanel)` to open the vendor
  3. Falls back to finding `MazeVendorUI` directly if hub controller not found
  4. Falls back to finding panel by GameObject name

### Bypassing Dialogue

If you want the vendor button to open the shop directly without dialogue:
- Set **Use Vendor Dialogue** to `false` in `MazeHubController`
- Or leave **Vendor Dialogue** field empty

## Ambient Lines (Future Enhancement)

The markdown mentions ambient lines that Blinket says while browsing:
- "Careful. That one bites."
- "Blinket is 80% sure that price is correct!"
- "If you hear humming, don't hum back."
- "Blinket accepts gold, shards, secrets, and polite begging."
- "No refunds! Not since the incident."

These can be implemented as:
1. Random dialogue nodes that trigger periodically while the shop is open
2. A separate ambient dialogue system in `MazeVendorUI`
3. Tooltip or notification messages

**Note**: These are not included in the initial dialogue asset but can be added later.

## Dialogue Node Reference

### Complete Node List

| Node ID | Speaker | Description |
|---------|---------|-------------|
| `greeting` | Blinket | Initial greeting with 5 choices |
| `who_are_you` | Blinket | Explains who Blinket is |
| `from_maze` | Blinket | Response about not being from the Maze |
| `nervous` | Blinket | Response about being cautiously optimistic |
| `what_is_place` | Blinket | Explains the Ever-Shifting Emporium |
| `restocks` | Blinket | Explains automatic restocking |
| `is_safe` | Blinket | Discusses shop safety |
| `bag_twitching` | Blinket | Explains the twitching bag |
| `open_shop` | Blinket | Opens the vendor shop (end node with action) |
| `goodbye` | Blinket | Farewell message (end node) |

## Customization

### Adding More Dialogue Nodes

To add custom dialogue nodes to the generated asset:

1. Select the `Blinket_Dialogue.asset` in Unity
2. In the Inspector, expand the **Dialogue Tree** section
3. Expand the **Nodes** list
4. Add new nodes by:
   - Clicking the "+" button
   - Setting `nodeId`, `speakerName`, `paragraphs`
   - Adding choices that link to other nodes

### Modifying Dialogue Text

Edit the dialogue text directly in the asset:
1. Select `Blinket_Dialogue.asset`
2. Expand the node you want to edit
3. Modify the `paragraphs` list or `dialogueText` field
4. Changes are saved automatically

### Setting Speaker Portraits

#### Default Speaker Portrait

The dialogue asset has a **Default Speaker Portrait** field:
1. Select `Blinket_Dialogue.asset`
2. In the **Speaker Portrait** section, find **Default Speaker Portrait**
3. Drag a sprite to assign it
4. This portrait will be used for all dialogue nodes that don't have their own portrait

#### Node-Specific Portraits

To override the default portrait for specific nodes:
1. Expand the node in the dialogue asset
2. Find the **Speaker Portrait** field in that node
3. Drag a sprite to assign it
4. This node will use its own portrait instead of the default

**Example Use Cases:**
- Use default portrait for all of Blinket's dialogue
- Override for specific nodes where Blinket looks different (surprised, worried, etc.)
- Use different portraits for multi-speaker dialogues

### Adding Conditions

You can add conditions to dialogue choices:
- Only show certain options if quests are completed
- Unlock new dialogue based on player level
- Hide options after they've been seen

Example: Add a `DialogueCondition` to a choice to check if a tutorial has been completed.

## Troubleshooting

### Dialogue Not Starting

- **Check**: Is `DialogueManager` in the scene?
- **Check**: Is `vendorDialogue` assigned in `MazeHubController`?
- **Check**: Is `useVendorDialogue` enabled?
- **Check**: Does `DialogueUI` component exist in the scene?

### Shop Not Opening After Dialogue

- **Check**: Does the `open_shop` node have the `OpenShop` action with value "MazeVendor"?
- **Check**: Does `MazeHubController` exist in the scene?
- **Check**: Is the `vendorPanel` field assigned in `MazeHubController`?
- **Check Console**: Look for `[DialogueManager]` log messages

### Dialogue UI Not Appearing

- **Check**: Does `DialogueUI` component exist in the scene?
- **Check**: Is the dialogue panel GameObject active?
- **Check**: Is the dialogue panel assigned to a Canvas?
- **Check Console**: Look for `[DialogueManager]` warnings about missing UI

## Integration Points

### With MazeVendorUI

The dialogue system integrates seamlessly with the vendor:
- Dialogue opens first (if enabled)
- Shop opens automatically after "Let me see what you're selling"
- Both systems work independently if dialogue is disabled

### With MazeHubController

The hub controller now supports:
- Optional dialogue before opening vendor
- Direct vendor opening if dialogue is disabled
- Consistent overlay panel system (dialogue → shop → deadzone backdrop)

## Files Created/Modified

### Created Files:
- `Assets/Editor/CreateBlinketDialogue.cs` - Editor tool to generate dialogue asset
- `Assets/Documentation/Blinket Dialogue Setup Guide.md` - This guide
- `Assets/Resources/Dialogues/Blinket_Dialogue.asset` - Generated dialogue asset (created by tool)

### Modified Files:
- `Assets/Scripts/Dialogue/DialogueManager.cs` - Added `OpenShop()` and `OpenMazeVendorShop()` methods
- `Assets/Scripts/MazeSystem/MazeHubController.cs` - Added dialogue support to vendor button

## Next Steps

1. **Generate the dialogue asset** using the editor tool
2. **Assign it** to `MazeHubController` in your Maze Hub scene
3. **Test** the dialogue flow by clicking the vendor button
4. **Customize** dialogue text or add new nodes as needed
5. **(Optional)** Implement ambient lines system for while browsing

