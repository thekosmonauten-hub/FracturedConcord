# Board Summary Window Setup Guide

This guide explains how to set up the board summary window that shows when hovering over board selection buttons.

## Overview

The board summary system consists of several components:

1. **BoardSummaryWindow** - Main UI component that displays board stats and notables
2. **BoardSelectionButtonHover** - Handles hover detection on board buttons
3. **BoardSelectionButton** - Enhanced button component with board data
4. **BoardSelectionUISetup** - Helper script for automatic setup
5. **BoardPositioningManager** - Integration with existing board selection system

## Quick Setup

### Step 1: Create Board Summary Window

1. **Create a new GameObject** in your scene called "BoardSummaryWindow"
2. **Add the BoardSummaryWindow component** to it
3. **Set up the UI elements** (see detailed setup below)

### Step 2: Configure BoardPositioningManager

1. **Open the BoardPositioningManager** in the inspector
2. **Assign the BoardSummaryWindow** to the "Board Summary Window" field
3. **The system will auto-find it** if not assigned

### Step 3: Add Hover Detection to Board Buttons

1. **Add BoardSelectionButtonHover component** to your board selection buttons
2. **Assign the BoardSummaryWindow** reference
3. **Set the BoardData** for each button

## Detailed Setup

### BoardSummaryWindow UI Setup

Create a UI Canvas with the following structure:

```
BoardSummaryWindow (GameObject)
├── Canvas (Canvas)
├── CanvasScaler (CanvasScaler)
├── GraphicRaycaster (GraphicRaycaster)
└── SummaryPanel (GameObject)
    ├── Image (Image) - Background panel
    ├── BoardNameText (TextMeshProUGUI) - Board name
    ├── BoardTierText (TextMeshProUGUI) - Board tier
    ├── StatsText (TextMeshProUGUI) - Overall stats
    ├── NotablesText (TextMeshProUGUI) - Notable effects
    └── DescriptionText (TextMeshProUGUI) - Board description
```

### Board Button Setup

For each board selection button:

1. **Add BoardSelectionButton component**
2. **Add BoardSelectionButtonHover component**
3. **Set the BoardData** reference
4. **Configure hover delay** (default: 0.5 seconds)

### Automatic Setup

Use the **BoardSelectionUISetup** component for automatic setup:

1. **Add BoardSelectionUISetup** to a GameObject in your scene
2. **Assign references** (BoardPositioningManager, BoardSummaryWindow, etc.)
3. **Click "Setup Board Selection UI"** in the context menu
4. **Test with "Test Board Summary"** to verify functionality

## Configuration

### BoardSummaryWindow Settings

- **Hover Delay**: Time before showing summary (default: 0.5s)
- **Show Summary On Hover**: Enable/disable hover functionality
- **Stat Colors**: Customize colors for different text elements

### Board Data Structure

The system expects BoardData with the following structure:

```csharp
// BoardData is a ScriptableObject with these properties:
public class BoardData : ScriptableObject
{
    public string BoardName { get; }           // "T1 Fire Board"
    public string BoardDescription { get; }    // Board description
    public int RequiredLevel { get; }          // 1, 2, 3, etc.
    public string GetJsonDataText() { get; }   // JSON data with node stats
    public Sprite BoardPreview { get; }        // Optional board icon
    public BoardTheme BoardTheme { get; }      // Fire, Cold, Lightning, etc.
}
```

### JSON Structure for Stats

The system parses board JSON to calculate total stats. Expected format:

```json
{
  "nodes": [
    {
      "id": "fire_damage_1",
      "stats": {
        "fire_damage": 15,
        "fire_damage_percent": 10
      }
    },
    {
      "id": "intelligence_1", 
      "stats": {
        "intelligence": 10
      }
    }
  ]
}
```

## Usage

### Manual Usage

```csharp
// Show board summary
boardSummaryWindow.ShowBoardSummary(boardData);

// Hide board summary
boardSummaryWindow.HideBoardSummary();
```

### Through BoardPositioningManager

```csharp
// Show summary for a board
boardPositioningManager.ShowBoardSummary(boardData);

// Hide summary
boardPositioningManager.HideBoardSummary();
```

## Example: T1 Fire Board

Here's an example of what the summary might show for a T1 Fire Board:

```
T1 Fire Board
Tier 1

Overall Stats:
Fire Damage: +25%
Intelligence: +10
Ignite Magnitude: +15%
Health: +50

Notables:
• Enhanced fire-based abilities
• Improved elemental resistance  
• Increased spell damage
• Better mana efficiency

A powerful fire-based passive board with enhanced elemental damage and resistance.
```

## Troubleshooting

### Common Issues

1. **Summary not showing**: Check that BoardSummaryWindow is assigned and active
2. **Hover not working**: Verify BoardSelectionButtonHover component is added
3. **Stats not calculating**: Check that boardJson contains valid JSON with stats
4. **UI not visible**: Ensure Canvas is set to Screen Space - Overlay mode

### Debug Commands

- **Test Board Summary**: Right-click on BoardSelectionUISetup → "Test Board Summary"
- **Setup Board Selection UI**: Right-click on BoardSelectionUISetup → "Setup Board Selection UI"
- **Test Show Summary**: Right-click on BoardSelectionButtonHover → "Test Show Summary"

## Customization

### Adding New Stat Types

To add support for new stat types, modify the `statPatterns` dictionary in `BoardSummaryWindow.CalculateTotalStats()`:

```csharp
var statPatterns = new Dictionary<string, string>
{
    { "Fire Damage", "fire_damage|fireDamage|fire_damage_percent" },
    { "Intelligence", "intelligence|int|int_stat" },
    { "Your New Stat", "your_new_stat|yourNewStat|your_new_stat_percent" }
};
```

### Customizing Notable Effects

Modify the `ExtractNotableEffects()` method to parse your specific notable format:

```csharp
private List<string> ExtractNotableEffects(BoardData boardData)
{
    // Your custom notable parsing logic here
}
```

## Integration with Existing Systems

The board summary system integrates seamlessly with:

- **BoardPositioningManager**: Automatic initialization and management
- **BoardSelectionUI**: Hover detection on selection buttons
- **PassiveTreeManager**: Board data and selection events
- **WorldBoardAdjacencyManager**: Board placement and adjacency logic

## Performance Considerations

- **Hover Delay**: Prevents excessive summary showing/hiding
- **JSON Parsing**: Cached results to avoid repeated parsing
- **UI Updates**: Only updates when board data changes
- **Memory Usage**: Minimal overhead with efficient text formatting

## Future Enhancements

Potential improvements:

1. **Animated Transitions**: Smooth fade-in/out effects
2. **Tooltip Positioning**: Smart positioning to avoid screen edges
3. **Stat Comparisons**: Compare with currently selected boards
4. **Visual Indicators**: Icons for different stat types
5. **Search/Filter**: Find boards by stat type or name
