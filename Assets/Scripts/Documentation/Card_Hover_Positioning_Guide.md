# 🎯 Card Hover Positioning Guide

## Problem Fixed
The hover card was appearing in the center of the screen instead of positioned relative to the hovered deck card.

## ✅ Solution Implemented

### **Relative Positioning System**
The hover card now appears positioned relative to the deck card being hovered, not centered on screen.

### **How It Works**
1. **Gets hovered card position** - World position of the deck card
2. **Converts to screen space** - Transforms world position to screen coordinates  
3. **Converts to canvas space** - Transforms screen position to hover canvas local position
4. **Applies offset** - Positions above/beside the hovered card with configurable offset

## 🎛️ New Inspector Settings

### **Card Hover Settings**
Located in `CharacterCreationController` Inspector:

#### **Hover Offset Y** (Range: -200 to 200, Default: 100)
- **Positive values**: Position above the hovered card
- **Negative values**: Position below the hovered card
- **Example**: 100 = 100 pixels above, -50 = 50 pixels below

#### **Hover Offset X** (Range: -200 to 200, Default: 0)
- **Positive values**: Position to the right of the hovered card
- **Negative values**: Position to the left of the hovered card
- **Example**: 50 = 50 pixels to the right, -30 = 30 pixels to the left

#### **Hover Card Scale** (Range: 0.5 to 2.0, Default: 1.2)
- **Controls size** of the hover card preview
- **1.0** = Same size as original card
- **1.2** = 20% larger (default)
- **0.8** = 20% smaller

## 🎮 Expected Behavior

### **Before (Center Positioning):**
```
[Deck Card] → Hover → [Hover Card appears in center of screen]
```

### **After (Relative Positioning):**
```
[Deck Card] → Hover → [Hover Card appears above the deck card]
```

## 🔧 Configuration Examples

### **Above and Centered:**
```
Hover Offset Y: 120
Hover Offset X: 0
Hover Card Scale: 1.2
```
**Result**: Hover card appears 120 pixels above the deck card, centered horizontally

### **Above and to the Right:**
```
Hover Offset Y: 100
Hover Offset X: 50
Hover Card Scale: 1.0
```
**Result**: Hover card appears 100 pixels above and 50 pixels to the right

### **Below and to the Left:**
```
Hover Offset Y: -80
Hover Offset X: -30
Hover Card Scale: 1.5
```
**Result**: Hover card appears 80 pixels below and 30 pixels to the left, 50% larger

## 🚀 Benefits

- ✅ **Intuitive positioning** - Hover card appears near the source
- ✅ **Configurable offsets** - Adjust position to your preference
- ✅ **Scalable size** - Control how large the hover card appears
- ✅ **Fallback system** - Centers if positioning fails
- ✅ **Debug logging** - Shows positioning calculations in console

## 🔍 Debug Information

Console will show positioning details:
```
Positioned hover card at: (150, 200, 0) (hovered card at: (100, 100, 0))
```

This helps verify the positioning is working correctly and troubleshoot any issues.

Perfect for creating a professional hover experience! 🎯











