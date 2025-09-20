# StatsPanel Text Color Customization Guide

## Overview
This guide covers all the ways to customize text colors inside MultiColumnListView cells in the StatsPanel.

## 🎨 Color Customization Approaches

### **1. CSS-Based Column-Specific Colors (Static)**

#### **How It Works**
- Uses CSS selectors to target specific columns
- Colors are applied based on column position
- Static colors that don't change based on data content

#### **Current Implementation**
```css
/* Attribute Columns */
#AttributeColumns .unity-multi-column-list-view__cell:nth-child(1) {
    color: rgb(255, 100, 100); /* Strength - Red */
}
#AttributeColumns .unity-multi-column-list-view__cell:nth-child(2) {
    color: rgb(100, 255, 100); /* Dexterity - Green */
}
#AttributeColumns .unity-multi-column-list-view__cell:nth-child(3) {
    color: rgb(100, 150, 255); /* Intelligence - Blue */
}

/* Resource Columns */
#ResourceColumns .unity-multi-column-list-view__cell:nth-child(1) {
    color: rgb(255, 120, 120); /* Health - Red */
}
#ResourceColumns .unity-multi-column-list-view__cell:nth-child(2) {
    color: rgb(120, 180, 255); /* Mana - Blue */
}
#ResourceColumns .unity-multi-column-list-view__cell:nth-child(3) {
    color: rgb(200, 120, 255); /* Energy Shield - Purple */
}
#ResourceColumns .unity-multi-column-list-view__cell:nth-child(4) {
    color: rgb(255, 255, 120); /* Reliance - Yellow */
}

/* Damage Columns */
#DamageColumns .unity-multi-column-list-view__cell:nth-child(1) {
    color: rgb(200, 200, 200); /* Type - Gray */
}
#DamageColumns .unity-multi-column-list-view__cell:nth-child(2) {
    color: rgb(255, 150, 50); /* Flat - Orange */
}
#DamageColumns .unity-multi-column-list-view__cell:nth-child(3) {
    color: rgb(100, 200, 255); /* Increased - Light Blue */
}
#DamageColumns .unity-multi-column-list-view__cell:nth-child(4) {
    color: rgb(255, 255, 100); /* More - Yellow */
}

/* Resistance Columns */
#ResistanceColumns .unity-multi-column-list-view__cell:nth-child(1) {
    color: rgb(255, 100, 100); /* Fire - Red */
}
#ResistanceColumns .unity-multi-column-list-view__cell:nth-child(2) {
    color: rgb(100, 255, 255); /* Cold - Cyan */
}
#ResistanceColumns .unity-multi-column-list-view__cell:nth-child(3) {
    color: rgb(255, 255, 100); /* Lightning - Yellow */
}
#ResistanceColumns .unity-multi-column-list-view__cell:nth-child(4) {
    color: rgb(200, 100, 255); /* Chaos - Purple */
}
```

### **2. CSS Classes for Dynamic Styling**

#### **Available CSS Classes**
```css
/* Bonus Value Styling */
.bonus-positive {
    color: rgb(100, 255, 100) !important; /* Green for positive */
    -unity-font-style: bold;
}

.bonus-negative {
    color: rgb(255, 100, 100) !important; /* Red for negative */
    -unity-font-style: bold;
}

.bonus-zero {
    color: rgb(150, 150, 150) !important; /* Gray for zero */
}

/* Percentage Styling */
.percentage-value {
    color: rgb(255, 200, 100) !important; /* Orange for percentages */
    -unity-font-style: bold;
}
```

### **3. C# Code-Based Dynamic Colors**

#### **How It Works**
- Colors are applied programmatically based on data content
- Dynamic styling that changes based on values
- Uses CSS classes for styling

#### **Implementation in StatsPanelRuntime.cs**
```csharp
private void ApplyAttributeStyling(Label label, string columnName, string data)
{
    // Remove existing classes
    label.RemoveFromClassList("bonus-positive");
    label.RemoveFromClassList("bonus-negative");
    label.RemoveFromClassList("bonus-zero");

    // Apply dynamic styling
    if (data.StartsWith("+"))
    {
        label.AddToClassList("bonus-positive"); // Green
    }
    else if (data.StartsWith("-"))
    {
        label.AddToClassList("bonus-negative"); // Red
    }
    else if (data == "+0" || data == "0")
    {
        label.AddToClassList("bonus-zero"); // Gray
    }
}
```

## 🎯 Expected Color Results

### **Attributes Section**
```
Strength    | Dexterity  | Intelligence
🔴 50       | 🟢 12      | 🔵 40
🔴 23       | 🟢 14      | 🔵 23
🟢 +27      | 🔴 -2      | 🟢 +17
```

### **Resources Section**
```
Health      | Mana       | Energy Shield | Reliance
🔴 108/108  | 🔵 3/3     | 🟣 0/0        | 🟡 75%
🔴 +50      | 🔵 +10     | 🟣 +15        | 🟡 +5%
```

### **Damage Section**
```
Type        | Flat       | Increased     | More
⚪ Physical  | 🟠 50      | 🔵 110%       | 🟡 30%
⚪ Fire      | 🟠 50      | 🔵 110%       | 🟡 30%
⚪ Cold      | 🟠 50      | 🔵 110%       | 🟡 30%
⚪ Lightning | 🟠 50      | 🔵 110%       | 🟡 30%
⚪ Chaos     | 🟠 50      | 🔵 110%       | 🟡 30%
```

### **Resistances Section**
```
Fire        | Cold       | Lightning     | Chaos
🔴 0/0      | 🔵 0/0     | 🟡 0/0        | 🟣 0/0
🟢 +10      | 🟢 +15     | 🟢 +20        | 🟢 +5
```

## 🔧 Customization Options

### **1. Change Column Colors**
To change a specific column color, modify the CSS:

```css
/* Change Strength column to blue */
#AttributeColumns .unity-multi-column-list-view__cell:nth-child(1) {
    color: rgb(100, 150, 255); /* Blue instead of red */
}
```

### **2. Add New CSS Classes**
To add new color schemes:

```css
/* Critical values - bright red */
.critical-value {
    color: rgb(255, 50, 50) !important;
    -unity-font-style: bold;
    font-size: 14px;
}

/* Legendary values - gold */
.legendary-value {
    color: rgb(255, 215, 0) !important;
    -unity-font-style: bold;
    text-shadow: 0 0 5px rgba(255, 215, 0, 0.5);
}
```

### **3. Dynamic Color Logic**
To add new dynamic color logic in C#:

```csharp
private void ApplyCustomStyling(Label label, string data)
{
    // Remove existing classes
    label.RemoveFromClassList("critical-value");
    label.RemoveFromClassList("legendary-value");

    // Parse numeric value
    if (int.TryParse(data.Replace("+", "").Replace("-", ""), out int value))
    {
        if (value >= 100)
        {
            label.AddToClassList("legendary-value");
        }
        else if (value <= 0)
        {
            label.AddToClassList("critical-value");
        }
    }
}
```

## 🎨 Color Palette Reference

### **Standard Colors**
- **Red**: `rgb(255, 100, 100)` - Health, Fire, Negative values
- **Green**: `rgb(100, 255, 100)` - Dexterity, Positive values
- **Blue**: `rgb(100, 150, 255)` - Intelligence, Mana, Increased damage
- **Yellow**: `rgb(255, 255, 100)` - Lightning, More damage, Reliance
- **Purple**: `rgb(200, 100, 255)` - Energy Shield, Chaos
- **Orange**: `rgb(255, 150, 50)` - Flat damage, Percentages
- **Cyan**: `rgb(100, 255, 255)` - Cold resistance
- **Gray**: `rgb(200, 200, 200)` - Type labels, Zero values

### **Special Colors**
- **Critical**: `rgb(255, 50, 50)` - Very low values
- **Legendary**: `rgb(255, 215, 0)` - Very high values
- **Neutral**: `rgb(150, 150, 150)` - Zero or neutral values

## 🧪 Testing Your Color Changes

### **Step 1: Test CSS Changes**
1. Modify colors in USS files
2. Save and let Unity recompile
3. Check both Editor Window and Runtime

### **Step 2: Test Dynamic Colors**
1. Modify C# styling methods
2. Add/remove CSS classes
3. Test with different data values

### **Step 3: Verify Color Consistency**
1. Check that colors match across sections
2. Verify dynamic colors work correctly
3. Test edge cases (zero values, negative values)

## 🚨 Troubleshooting

### **Colors Not Applying**
1. Check CSS selector specificity
2. Verify USS files are saved and compiled
3. Ensure no conflicting styles

### **Dynamic Colors Not Working**
1. Check that C# methods are being called
2. Verify CSS classes exist
3. Debug data parsing logic

### **Performance Issues**
1. Avoid too many dynamic color changes
2. Use CSS classes instead of inline styles
3. Cache color calculations when possible

The color system provides both static column-based colors and dynamic content-based colors for a rich, informative visual experience! 🎨









