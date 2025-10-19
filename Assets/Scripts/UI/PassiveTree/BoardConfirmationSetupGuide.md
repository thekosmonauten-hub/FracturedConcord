# Board Confirmation System Setup Guide

This guide explains how to set up the board confirmation system that shows a "Place Board" button instead of instant board selection.

## Overview

The board confirmation system adds a confirmation step before placing boards:

1. **User clicks on a board** → Shows confirmation UI
2. **User reviews board details** → Stats, notables, description
3. **User clicks "Place Board"** → Actually places the board
4. **User clicks "Cancel" or "Back"** → Returns to board selection

## Components

### BoardConfirmationUI.cs
- **Main confirmation UI component**
- **Shows board details, stats, and notables**
- **Handles "Place Board", "Cancel", and "Back" buttons**
- **Displays board preview image**

### BoardPositioningManager.cs (Updated)
- **Modified OnBoardSelected()** to show confirmation instead of instant placement
- **Added confirmation flow methods**
- **Handles board data conversion**
- **Manages UI state transitions**

## Setup Instructions

### Step 1: Create Board Confirmation UI

1. **Create a new GameObject** called "BoardConfirmationUI"
2. **Add the BoardConfirmationUI component** to it
3. **Set up the UI elements** (see detailed setup below)

### Step 2: Configure BoardPositioningManager

1. **Open the BoardPositioningManager** in the inspector
2. **Assign the BoardConfirmationUI** to the "Board Confirmation UI" field
3. **The system will auto-find it** if not assigned

### Step 3: UI Setup

Create a UI Canvas with the following structure:

```
BoardConfirmationUI (GameObject)
├── Canvas (Canvas)
├── CanvasScaler (CanvasScaler)
├── GraphicRaycaster (GraphicRaycaster)
└── ConfirmationPanel (GameObject)
    ├── Image (Image) - Background panel
    ├── BoardNameText (TextMeshProUGUI) - Board name
    ├── BoardDescriptionText (TextMeshProUGUI) - Board description
    ├── BoardPreviewImage (Image) - Board preview sprite
    ├── StatsText (TextMeshProUGUI) - Board stats
    ├── NotablesText (TextMeshProUGUI) - Notable effects
    ├── PlaceBoardButton (Button) - Confirm placement
    ├── CancelButton (Button) - Cancel selection
    └── BackButton (Button) - Return to selection
```

## UI Element Configuration

### Required UI Elements

1. **ConfirmationPanel** - Main container (GameObject)
2. **BoardNameText** - Shows board name (TextMeshProUGUI)
3. **BoardDescriptionText** - Shows board description (TextMeshProUGUI)
4. **BoardPreviewImage** - Shows board preview sprite (Image)
5. **StatsText** - Shows calculated stats (TextMeshProUGUI)
6. **NotablesText** - Shows notable effects (TextMeshProUGUI)
7. **PlaceBoardButton** - Confirms board placement (Button)
8. **CancelButton** - Cancels selection (Button)
9. **BackButton** - Returns to board selection (Button)

### Optional UI Elements

1. **StatsScrollRect** - Scrollable stats area (ScrollRect)
2. **NotablesScrollRect** - Scrollable notables area (ScrollRect)

## Button Configuration

### Place Board Button
- **Color**: Green (confirmButtonColor)
- **Action**: Confirms board placement
- **Text**: "Place Board" or "Confirm"

### Cancel Button
- **Color**: Red (cancelButtonColor)
- **Action**: Cancels selection and closes UI
- **Text**: "Cancel"

### Back Button
- **Color**: Gray (backButtonColor)
- **Action**: Returns to board selection
- **Text**: "Back" or "← Back"

## Flow Diagram

```
User clicks extension point
         ↓
Show board selection UI
         ↓
User clicks on a board
         ↓
Show board confirmation UI
         ↓
User reviews board details
         ↓
User clicks "Place Board" → Actually place board
User clicks "Cancel" → Close UI
User clicks "Back" → Return to board selection
```

## Code Integration

### BoardPositioningManager Changes

The `OnBoardSelected()` method now shows confirmation instead of instant placement:

```csharp
private void OnBoardSelected(ExtensionBoardPrefabData boardData)
{
    // Show board confirmation instead of instant placement
    ShowBoardConfirmationForPrefab(boardData);
}
```

### Board Data Conversion

The system converts `ExtensionBoardPrefabData` to `BoardData` for the confirmation UI:

```csharp
private BoardData ConvertPrefabDataToBoardData(ExtensionBoardPrefabData prefabData)
{
    // Finds matching BoardData asset by name
    // Falls back to first available BoardData if no match found
}
```

## Testing

### Manual Testing

1. **Click on an extension point** to open board selection
2. **Click on a board** to see confirmation UI
3. **Review board details** (name, description, stats, notables)
4. **Click "Place Board"** to confirm placement
5. **Click "Cancel" or "Back"** to return to selection

### Debug Commands

- **Check console logs** for confirmation flow
- **Verify UI elements** are properly assigned
- **Test button functionality** with different boards

## Customization

### Styling

Modify the button colors in the inspector:

```csharp
[Header("Styling")]
[SerializeField] private Color confirmButtonColor = Color.green;
[SerializeField] private Color cancelButtonColor = Color.red;
[SerializeField] private Color backButtonColor = Color.gray;
```

### Board Data Matching

The system tries to match `ExtensionBoardPrefabData` with existing `BoardData` assets:

1. **Finds BoardData by name match**
2. **Falls back to first available BoardData**
3. **Returns null if no BoardData found**

### Stat Calculation

The confirmation UI calculates stats from board JSON data:

```csharp
private Dictionary<string, float> CalculateTotalStats(BoardData boardData)
{
    // Parses JSON data to extract stat values
    // Returns dictionary of stat name → total value
}
```

## Troubleshooting

### Common Issues

1. **Confirmation UI not showing**: Check that BoardConfirmationUI is assigned
2. **Board data not found**: Verify BoardData assets exist and names match
3. **Stats not calculating**: Check that board JSON data is valid
4. **Buttons not working**: Verify button onClick events are assigned

### Debug Logs

Look for these console messages:

- `"Showing confirmation for [BoardName]"`
- `"Board confirmed: [BoardName]"`
- `"Board confirmation cancelled"`
- `"No BoardConfirmationUI found"`

## Integration with Existing Systems

The board confirmation system integrates with:

- **BoardPositioningManager**: Main integration point
- **BoardSummaryWindow**: Shared stat calculation logic
- **ExtensionBoardPrefabData**: Source data for board selection
- **BoardData**: Target data for confirmation UI
- **Passive Tree System**: Board placement and adjacency logic

## Performance Considerations

- **UI State Management**: Efficient show/hide operations
- **Data Conversion**: Cached BoardData lookups
- **Stat Calculation**: Optimized JSON parsing
- **Memory Usage**: Minimal overhead with proper cleanup

## Future Enhancements

Potential improvements:

1. **Animated Transitions**: Smooth UI transitions
2. **Board Comparison**: Compare multiple boards side-by-side
3. **Preview Mode**: Show board in world before placement
4. **Undo Functionality**: Allow undoing board placement
5. **Board Filtering**: Filter boards by stats or theme













