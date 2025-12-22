# MainGameMenuButton Setup Guide

This guide explains how to set up the Menu button in the MainGameUI scene.

## Overview

The `MainGameMenuButton` script provides a menu system that allows players to:
- Save their character and return to the main menu
- Quit without saving (optional)
- Cancel and return to the game

## Setup Instructions

### 1. Create the Menu Button

1. In the MainGameUI scene, create a new Button GameObject (e.g., "MenuButton")
2. Position it where you want the menu button to appear (typically top-right corner)
3. Set the button text to "Menu" or use an icon
4. Add the `MainGameMenuButton` component to this button

### 2. Create the Menu Panel

1. Create a new GameObject (e.g., "MenuPanel") as a child of your main Canvas
2. Add a `Canvas Group` or `Image` component for the background
3. Set the panel to be **inactive** by default (uncheck the checkbox in Inspector)
4. Position and style the panel as desired (typically centered or anchored to a corner)

### 3. Add Menu Panel Buttons

Inside the MenuPanel, create the following buttons:

#### Required Buttons:
- **Save & Quit Button**: Button that saves the character and returns to main menu
- **Cancel Button**: Button that closes the menu without doing anything

#### Optional Buttons:
- **Quit Without Saving Button**: Button that quits without saving (use with caution)

### 4. Optional: Confirmation Dialog

If you want a confirmation dialog before quitting:

1. Create a new GameObject (e.g., "ConfirmationDialog") as a child of your Canvas
2. Add a background panel and text element
3. Add two buttons: "Confirm" and "Cancel"
4. Add a `TextMeshProUGUI` component for the message text
5. Set the dialog to be **inactive** by default

### 5. Configure the Script

In the Inspector for the MenuButton GameObject with `MainGameMenuButton`:

1. **Menu Button**: Assign the button component (auto-detected if on same GameObject)
2. **Menu Panel**: Assign the MenuPanel GameObject
3. **Save & Quit Button**: Assign the Save & Quit button
4. **Quit Without Saving Button**: (Optional) Assign if you have this button
5. **Cancel Button**: Assign the Cancel button
6. **Main Menu Scene Name**: Set to "MainMenu" (default)
7. **Confirmation Dialog**: (Optional) Assign the confirmation dialog GameObject
8. **Confirmation Message Text**: (Optional) Assign the TextMeshProUGUI for the message
9. **Confirm Button**: (Optional) Assign the confirm button in the dialog
10. **Confirm Cancel Button**: (Optional) Assign the cancel button in the dialog

## UI Hierarchy Example

```
Canvas
├── MenuButton (with MainGameMenuButton component)
├── MenuPanel (inactive by default)
│   ├── Background (Image)
│   ├── SaveAndQuitButton
│   ├── QuitWithoutSavingButton (optional)
│   └── CancelButton
└── ConfirmationDialog (inactive by default, optional)
    ├── Background (Image)
    ├── MessageText (TextMeshProUGUI)
    ├── ConfirmButton
    └── CancelButton
```

## Features

- **Escape Key Support**: Press Escape to close the menu
- **Auto-Save**: Automatically saves the character when "Save & Quit" is clicked
- **Confirmation Dialog**: Optional confirmation before quitting
- **Scene Transition**: Smoothly transitions to MainMenu scene

## Usage

1. Player clicks the Menu button
2. Menu panel opens
3. Player selects an option:
   - **Save & Quit**: Saves character and returns to main menu
   - **Quit Without Saving**: Returns to main menu without saving (if enabled)
   - **Cancel**: Closes menu and returns to game
4. If confirmation dialog is enabled, player must confirm before quitting

## Notes

- The menu panel should start **inactive** (unchecked in Inspector)
- The confirmation dialog should also start **inactive** if used
- The script will automatically find buttons by name if not assigned in Inspector
- Character saving uses `CharacterManager.Instance.SaveCharacter()`
- Scene transition uses `SceneManager.LoadScene("MainMenu")`

