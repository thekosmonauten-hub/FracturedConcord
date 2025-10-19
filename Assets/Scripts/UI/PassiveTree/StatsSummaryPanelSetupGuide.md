# Stats Summary Panel Setup Guide

## Overview
The Stats Summary Panel is a static UI element that displays all accumulated stats from allocated passive tree nodes. It appears on the right side of the screen and updates in real-time as nodes are allocated or deallocated.

## Features
- **Real-time stats tracking** from all allocated nodes
- **Categorized display** (Damage, Attributes, Resistances, etc.)
- **Color-coded stats** for easy identification
- **Toggle button** to show/hide the panel
- **Scrollable content** for large stat lists
- **Automatic updates** every 0.5 seconds

## Quick Setup

### Method 1: Automatic Setup (Recommended)
1. **Add StatsSummaryPanelSetupHelper** to any GameObject in your scene
2. **Right-click** on the component and select **"Setup Stats Summary Panel"**
3. **Test** by right-clicking and selecting **"Test Stats Panel"**

### Method 2: Simple Setup
1. **Add StatsSummaryPanelSetupHelper** to any GameObject
2. **Right-click** and select **"Create Simple Stats Panel"**
3. **Assign** the created components to the StatsSummaryPanel

## Manual Setup

### 1. Create Canvas
```
StatsSummaryCanvas (Canvas)
├── StatsSummaryPanel (GameObject with StatsSummaryPanel component)
│   ├── StatsContent (ScrollRect)
│   │   └── Content (VerticalLayoutGroup)
│   │       ├── TitleText (TextMeshProUGUI) - "Passive Tree Stats"
│   │       └── StatsText (TextMeshProUGUI) - Stats content
└── ToggleButton (Button)
    └── Text (TextMeshProUGUI) - "Show Stats"
```

### 2. Component Configuration

#### StatsSummaryPanel Component
- **statsPanel**: Reference to the main panel GameObject
- **toggleButton**: Reference to the toggle button
- **toggleButtonText**: Reference to the button text
- **statsContentText**: Reference to the stats display text
- **statsScrollRect**: Reference to the scroll rect (optional)
- **titleText**: Reference to the title text (optional)

#### Styling Options
- **titleColor**: Color for the title text
- **statColor**: Default color for stats
- **categoryColor**: Color for category headers
- **valueColor**: Color for stat values
- **titleFontSize**: Font size for title (default: 24)
- **statFontSize**: Font size for stats (default: 18)
- **categoryFontSize**: Font size for categories (default: 20)

#### Configuration
- **startVisible**: Whether panel starts visible (default: false)
- **updateInterval**: How often to update stats (default: 0.5 seconds)
- **toggleButtonTextShow**: Text when panel is hidden
- **toggleButtonTextHide**: Text when panel is visible

### 3. Positioning
The panel is positioned in the **top-right corner** of the screen:
- **Anchor**: Top-right (1, 1)
- **Pivot**: Top-right (1, 1)
- **Position**: (-10, -10) from top-right corner
- **Size**: 300x600 pixels (configurable)

### 4. Toggle Button
The toggle button is positioned **below the panel**:
- **Anchor**: Top-right (1, 1)
- **Pivot**: Top-right (1, 1)
- **Position**: (-10, -620) from top-right corner
- **Size**: 120x40 pixels (configurable)

## Integration

### With PassiveTreeManager
The panel automatically finds and integrates with:
- **PassiveTreeManager**: For core board node tracking
- **BoardPositioningManager**: For extension board tracking
- **ExtensionBoardController**: For individual board node tracking

### Stat Categories
Stats are automatically categorized into:
- **Damage**: Fire, Cold, Lightning, Physical damage
- **Attributes**: Intelligence, Strength, Dexterity
- **Resistances**: Fire, Cold, Lightning, Physical resistance
- **Magnitudes**: Ignite, Freeze, Shock magnitude
- **Speed**: Movement, Attack, Cast speed
- **Other**: Any uncategorized stats

### Color Coding
Stats are color-coded for easy identification:
- **Fire**: Red
- **Cold**: Cyan
- **Lightning**: Yellow
- **Physical**: Gray
- **Intelligence**: Blue
- **Strength**: Red
- **Dexterity**: Green
- **Default**: White

## Usage

### Basic Usage
1. **Toggle Panel**: Click the toggle button to show/hide
2. **View Stats**: Panel automatically updates with current stats
3. **Scroll**: Use scroll wheel if stats exceed panel height

### Programmatic Control
```csharp
// Get reference to the panel
StatsSummaryPanel statsPanel = FindObjectOfType<StatsSummaryPanel>();

// Show/hide the panel
statsPanel.ShowPanel();
statsPanel.HidePanel();
statsPanel.TogglePanel();

// Force update stats
statsPanel.ForceUpdateStats();
```

### Testing
Use the **"Test Stats Display"** context menu to show sample stats:
- Fire Damage: +25.5%
- Intelligence: +15
- Health: +100
- Ignite Magnitude: +30%
- Movement Speed: +10%

## Troubleshooting

### Panel Not Showing
- Check that **statsPanel** is assigned in the component
- Verify the panel GameObject is active
- Check Canvas sorting order (should be higher than tooltip)

### Stats Not Updating
- Verify **PassiveTreeManager** and **BoardPositioningManager** are found
- Check that nodes have proper JSON data
- Use **"Test Stats Display"** to verify formatting

### Performance Issues
- Increase **updateInterval** to reduce update frequency
- Check for excessive node allocation
- Verify no memory leaks in stat calculation

### Styling Issues
- Adjust **panelSize** and **buttonSize** for different screen resolutions
- Modify colors in the component inspector
- Check font sizes for readability

## Advanced Configuration

### Custom Stat Patterns
To add new stat types, modify the **statPatterns** dictionary in `ExtractStatsFromNode()`:
```csharp
var statPatterns = new Dictionary<string, string>
{
    { "Custom Stat", "\"custom_stat\":\\s*(\\d+\\.?\\d*)" },
    // Add more patterns here
};
```

### Custom Categories
To add new categories, modify the **categories** dictionary in `FormatStatsText()`:
```csharp
var categories = new Dictionary<string, List<KeyValuePair<string, float>>>
{
    { "Custom Category", new List<KeyValuePair<string, float>>() },
    // Add more categories here
};
```

### Custom Colors
To add new colors, modify the **GetStatColor()** method:
```csharp
private string GetStatColor(string statName)
{
    if (statName.Contains("Custom")) return ColorUtility.ToHtmlStringRGB(Color.magenta);
    // Add more color conditions here
}
```

## Best Practices

1. **Positioning**: Keep the panel in the top-right to avoid blocking gameplay
2. **Updates**: Use reasonable update intervals to balance performance and responsiveness
3. **Styling**: Use consistent colors and fonts with your game's UI theme
4. **Testing**: Always test with sample data before using with real stats
5. **Performance**: Monitor update frequency if you have many allocated nodes

## Notes

- The panel automatically integrates with the existing passive tree system
- Stats are calculated from all allocated nodes across all boards
- The panel respects the existing tooltip system and doesn't interfere
- All stat calculations are cached to improve performance
- The system is designed to be extensible for future stat types

