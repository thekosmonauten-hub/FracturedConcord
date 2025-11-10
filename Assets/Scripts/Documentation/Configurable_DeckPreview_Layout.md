# 🎛️ Configurable Deck Preview Layout Settings

## Overview
The Character Creation deck preview now supports fully configurable layout parameters that can be adjusted directly in the Unity Inspector.

## New Inspector Settings

### Deck Preview Layout Settings
Located in the `CharacterCreationController` Inspector under the **"Deck Preview Layout Settings"** header:

#### 1. Max Cards Per Row
- **Field**: `maxCardsPerRow`
- **Range**: 1-10
- **Default**: 6
- **Description**: Maximum number of cards per row before wrapping to next row
- **Example**: Set to 4 for tighter layout, 8 for wider layout

#### 2. Card Spacing X (Horizontal)
- **Field**: `cardSpacingX`
- **Range**: 0-50 pixels
- **Default**: 10
- **Description**: Horizontal spacing between cards
- **Example**: 5 for tight spacing, 20 for loose spacing

#### 3. Card Spacing Y (Vertical)
- **Field**: `cardSpacingY`
- **Range**: 0-50 pixels
- **Default**: 10
- **Description**: Vertical spacing between card rows
- **Example**: 5 for compact rows, 15 for spacious rows

#### 4. Card Width
- **Field**: `cardWidth`
- **Range**: 80-200 pixels
- **Default**: 120
- **Description**: Fixed width for each card display
- **Example**: 100 for smaller cards, 150 for larger cards

#### 5. Card Height
- **Field**: `cardHeight`
- **Range**: 30-80 pixels
- **Default**: 40
- **Description**: Fixed height for each card display
- **Example**: 35 for compact cards, 50 for taller cards

#### 6. Card Alignment
- **Field**: `cardAlignment`
- **Options**: Left, Center, Right
- **Default**: Center
- **Description**: How to align cards within the preview panel
- **Example**: Center for balanced look, Left for compact layout

## Dynamic Features

### Automatic Height Calculation
The container height automatically adjusts based on:
- Number of cards in the deck
- Cards per row setting
- Card height and vertical spacing
- Formula: `(rows × cardHeight) + ((rows-1) × cardSpacingY) + 40px padding`

### Real-time Layout Updates
Changes to these settings take effect immediately when:
- Switching between character classes
- The deck preview is regenerated

## Example Configurations

### Compact Layout
```
Max Cards Per Row: 8
Card Spacing X: 5
Card Spacing Y: 5
Card Width: 100
Card Height: 35
```
**Result**: More cards per row, tighter spacing, smaller cards

### Spacious Layout
```
Max Cards Per Row: 4
Card Spacing X: 20
Card Spacing Y: 15
Card Width: 150
Card Height: 50
```
**Result**: Fewer cards per row, generous spacing, larger cards

### Balanced Layout (Default)
```
Max Cards Per Row: 6
Card Spacing X: 10
Card Spacing Y: 10
Card Width: 120
Card Height: 40
Card Alignment: Center
```
**Result**: Good balance of cards per row and spacing, centered alignment

### Centered Layout
```
Max Cards Per Row: 4
Card Spacing X: 15
Card Spacing Y: 15
Card Width: 140
Card Height: 45
Card Alignment: Center
```
**Result**: Fewer cards per row, centered within the panel, generous spacing

## Debug Information
The console will show layout calculations:
```
Deck Preview Layout: 18 cards, 3 rows, 6 per row, height: 180, alignment: Center
```

## Benefits
- ✅ **No code changes needed** - adjust in Inspector
- ✅ **Real-time preview** - see changes immediately
- ✅ **Responsive design** - container height adjusts automatically
- ✅ **Flexible layouts** - support for different screen sizes and preferences
- ✅ **Range validation** - Inspector sliders prevent invalid values

Perfect for fine-tuning the deck preview appearance without touching code! 🎛️
