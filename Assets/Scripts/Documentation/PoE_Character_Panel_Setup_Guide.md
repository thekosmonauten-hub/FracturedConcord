# Path of Exile Character Panel Setup Guide

## Overview

This guide will help you create a character panel that visually matches Path of Exile's distinctive dark theme and layout. The PoE character panel features:

- **Dark color scheme** with brown/gold accents
- **Sectioned layout** with clear headers
- **Resource bars** for Life, Energy Shield, Mana
- **Comprehensive stat display** organized by categories
- **Scrollable content** for large stat lists

## Quick Setup

### Method 1: Using the PoE Character Panel Creator (Recommended)

1. **Open the Creator Window**
   - Go to `Tools > PoE Character Panel Creator`
   - This opens the dedicated editor window

2. **Create a New PoE Panel**
   - Click "Create PoE Character Panel"
   - This automatically creates a complete PoE-style panel with all components

3. **Customize the Style**
   - Adjust colors, fonts, and layout settings in the Style Customization section
   - Click "Apply Custom Style" to update the panel

4. **Add Functionality**
   - Click "Add CharacterStatsController" to add the stats controller
   - Click "Setup UIManager Integration" to connect to your UI system

### Method 2: Manual Setup

1. **Create the Base Panel**
   ```
   GameObject "PoECharacterPanel"
   ├── Canvas (ScreenSpaceOverlay)
   ├── CanvasScaler (ScaleWithScreenSize)
   ├── GraphicRaycaster
   └── PoECharacterPanelStyle
   ```

2. **Add the PoE Style Component**
   - Add `PoECharacterPanelStyle` script to your panel
   - Assign the main container reference
   - Call `ApplyPoEStyle()` to create the layout

## PoE Color Scheme

The default PoE color scheme includes:

```csharp
// Background Colors
poeBackgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.95f);      // Very dark blue-gray
poePanelBackgroundColor = new Color(0.08f, 0.08f, 0.12f, 0.98f); // Slightly lighter panel
poeBorderColor = new Color(0.3f, 0.25f, 0.15f, 1f);              // Brown border

// Header Colors
poeHeaderColor = new Color(0.15f, 0.12f, 0.08f, 1f);             // Dark brown header
poeSectionHeaderColor = new Color(0.8f, 0.6f, 0.2f, 1f);         // Gold section headers

// Text Colors
poeTextColor = new Color(0.9f, 0.85f, 0.7f, 1f);                 // Light cream text
poeStatValueColor = new Color(0.8f, 0.7f, 0.4f, 1f);             // Gold stat values
poePositiveStatColor = new Color(0.4f, 0.8f, 0.4f, 1f);          // Green for positive
poeNegativeStatColor = new Color(0.8f, 0.4f, 0.4f, 1f);          // Red for negative
```

## Layout Structure

The PoE panel uses this hierarchy:

```
PoECharacterPanel (Canvas)
├── PoEBackground (Image) - Dark overlay
├── PoEPanelBackground (Image) - Panel background
├── PoEHeader (Image + Text) - "CHARACTER" title
└── PoEContentArea (ScrollRect)
    ├── PoEViewport (Mask)
    └── PoEContent (VerticalLayoutGroup)
        ├── CharacterInfoSection
        ├── AttributesSection
        ├── CombatStatsSection
        ├── DefenseStatsSection
        ├── DamageModifiersSection
        ├── ResistancesSection
        ├── RecoveryStatsSection
        ├── CombatMechanicsSection
        ├── CardSystemSection
        └── EquipmentSummarySection
```

## Section Structure

Each section follows this pattern:

```
SectionName
├── SectionHeader (Image + Text) - Section title
└── SectionContent (VerticalLayoutGroup)
    ├── StatRow_StatName
    │   ├── StatName (TextMeshPro) - Stat label
    │   └── StatValue (TextMeshPro) - Stat value
    └── ... more stat rows
```

## Resource Bars

PoE-style resource bars include:

- **Life Bar** (Red) - Health points
- **Energy Shield Bar** (Blue) - Energy shield points
- **Mana Bar** (Cyan) - Mana points
- **Reliance Bar** (Yellow) - Reliance points

Each bar shows:
- Resource name on the left
- Visual bar in the center
- Current/Max values on the right

## Font Settings

Default PoE font settings:

```csharp
poeHeaderFontSize = 20;        // Main "CHARACTER" title
poeSectionFontSize = 16;       // Section headers
poeStatFontSize = 14;          // Stat names and values

poeHeaderFontStyle = FontStyles.Bold;
poeSectionFontStyle = FontStyles.Bold;
poeStatFontStyle = FontStyles.Normal;
```

## Integration with CharacterStatsController

To connect the PoE panel with your character stats system:

1. **Add CharacterStatsController**
   - The PoE panel automatically assigns the ScrollRect and Content references
   - The controller will populate the sections with actual character data

2. **Customize Stat Display**
   - Modify the `AddStatRow()` method in `CharacterStatsController` to use PoE colors
   - Use `poePositiveStatColor` for positive modifiers
   - Use `poeNegativeStatColor` for negative modifiers
   - Use `poeStatValueColor` for neutral values

## Customization Options

### Colors
- Modify the color fields in the `PoECharacterPanelStyle` component
- Use the editor window for real-time color adjustments
- Colors are applied to all text and background elements

### Layout
- Adjust `poePanelSize` for different panel dimensions
- Modify `poeSectionSpacing` for tighter/looser layouts
- Change `poeHeaderHeight` for different header sizes

### Fonts
- Customize font sizes for different text elements
- Adjust font styles (Bold, Italic, Normal)
- Font changes apply to all text elements

## Context Menu Commands

The `PoECharacterPanelStyle` component includes these context menu options:

- **Apply PoE Style** - Reapply the complete PoE styling
- **Create PoE Resource Bars** - Add resource bars to the character info section
- **Center Panel** - Center the panel on screen
- **Stretch Panel** - Make the panel fill the entire screen
- **Reset Panel Position** - Reset to default centered position

## Troubleshooting

### Panel Not Visible
- Check that the Canvas is set to `ScreenSpaceOverlay`
- Ensure the panel has a proper RectTransform
- Verify the background images are not transparent

### Text Not Styled
- Run "Apply PoE Style" from the context menu
- Check that the `PoECharacterPanelStyle` component is attached
- Verify font settings are not overridden by other components

### Sections Not Appearing
- Ensure the ScrollRect is properly configured
- Check that the content area has a VerticalLayoutGroup
- Verify section GameObjects are children of the content area

### Resource Bars Missing
- Use "Create PoE Resource Bars" from the context menu
- Ensure the CharacterInfoSection exists
- Check that the section content area is properly set up

## Performance Considerations

- The PoE panel uses efficient LayoutGroups for automatic positioning
- Text elements are optimized with TextMeshPro
- ScrollRect provides smooth scrolling for large stat lists
- Consider using object pooling for dynamically created stat rows

## Advanced Customization

### Custom Sections
To add custom sections beyond the default ones:

```csharp
// In PoECharacterPanelStyle
private void CreateCustomSection(Transform parent, string sectionName, string headerText)
{
    CreatePoESection(parent, sectionName, headerText);
}
```

### Custom Stat Rows
To create custom stat row layouts:

```csharp
// In PoECharacterPanelStyle
private void CreateCustomStatRow(GameObject parent, string statName, string statValue, Color valueColor)
{
    // Custom implementation for special stat displays
}
```

### Animation Integration
To add PoE-style animations:

```csharp
// Add smooth transitions when stats change
// Add hover effects for interactive elements
// Add fade-in animations when the panel opens
```

## Best Practices

1. **Consistent Naming** - Use the established naming conventions for sections and elements
2. **Color Consistency** - Stick to the PoE color palette for authenticity
3. **Responsive Design** - Test the panel at different screen resolutions
4. **Performance** - Use efficient layout groups and avoid unnecessary UI updates
5. **Accessibility** - Ensure text is readable and contrast is sufficient

## Integration Checklist

- [ ] PoE Character Panel created
- [ ] PoE style applied and customized
- [ ] CharacterStatsController added and configured
- [ ] UIManager integration set up
- [ ] Resource bars created and functional
- [ ] All stat sections populated with data
- [ ] Colors and fonts match PoE aesthetic
- [ ] Panel positioning and sizing optimized
- [ ] Scrolling and responsiveness tested
- [ ] Performance verified with large stat lists











