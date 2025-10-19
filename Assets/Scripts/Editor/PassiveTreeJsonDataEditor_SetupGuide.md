# Passive Tree JSON Data Editor - Setup Guide

## 🎯 **What This Tool Does**

The **Passive Tree JSON Data Editor** is a custom Unity editor window that allows you to easily manage `CellJsonData` on prefabs without manually scrolling through each cell. It provides a streamlined interface for adjusting JSON data efficiently.

## 🚀 **How to Access**

### **Method 1: Menu Bar**
1. Go to **Tools** → **Passive Tree** → **JSON Data Editor**
2. The editor window will open

### **Method 2: Code**
```csharp
PassiveTreeJsonDataEditor.ShowWindow();
```

## 📋 **Features Overview**

### **1. Cell Browser Tab**
- **View all cells** in a selected board
- **Quick cell information** (name, position, type, stats preview)
- **One-click editing** - click "Edit Stats" to open the stat editor
- **Real-time filtering** and search

### **2. Stat Editor Tab**
- **Comprehensive stat editing** for any selected cell
- **All stat categories** organized in sections:
  - Core Attributes (Strength, Dexterity, Intelligence)
  - Combat Resources (Health, Energy Shield, Mana, Reliance)
  - Combat Stats (Attack Power, Defense, Critical Chance, etc.)
  - Damage Modifiers (Increased Physical, Fire, Cold, etc.)
  - Added Damage (Added Physical, Fire, Cold, etc.)
  - Elemental Conversions (Physical as Fire, etc.)
  - Resistances (Physical, Fire, Cold, Lightning, Chaos, etc.)
  - Defense Stats (Armour, Evasion, Energy Shield, etc.)
  - Recovery Stats (Life Regen, Mana Regen, Leech, etc.)
  - Combat Mechanics (Attack Speed, Cast Speed, Movement Speed, etc.)
  - Card System Stats (Cards Drawn, Hand Size, etc.)
  - Legacy Stats (Armor Increase, Spell Power, etc.)

### **3. Bulk Operations Tab**
- **Select multiple cells** for batch operations
- **Apply stat templates** to multiple cells
- **Clear all stats** from selected cells
- **Bulk selection** (select all, clear selection)

### **4. Search & Filter Tab**
- **Search by name or position** to find specific cells
- **Filter by node type** (Start, Travel, Notable, Keystone, Extension)
- **Show only allocated nodes** (nodes that are purchased)
- **Show only nodes with stats** (nodes that have non-zero stats)
- **Clear all filters** to reset

## 🛠️ **How to Use**

### **Step 1: Select a Board**
1. **Open the editor** (Tools → Passive Tree → JSON Data Editor)
2. **Select a board** in the "Board" field
3. **Click "Refresh"** to load all cells from that board

### **Step 2: Browse Cells**
1. **Go to "Cell Browser" tab**
2. **See all cells** with their basic info
3. **Use search/filter** to find specific cells
4. **Click "Edit Stats"** on any cell to open the stat editor

### **Step 3: Edit Stats**
1. **Go to "Stat Editor" tab** (automatically switches when you click "Edit Stats")
2. **Modify any stat values** you want
3. **Click "Apply Changes"** to save
4. **Click "Reset"** to undo changes
5. **Click "Close"** to finish editing

### **Step 4: Bulk Operations (Optional)**
1. **Go to "Bulk Operations" tab**
2. **Enable "Bulk Mode"**
3. **Go back to "Cell Browser"** and select multiple cells
4. **Return to "Bulk Operations"** to apply changes to all selected cells

## 🔍 **Search and Filter Options**

### **Search Filter**
- **Search by cell name**: Type part of the cell's GameObject name
- **Search by node name**: Type part of the NodeName
- **Search by position**: Type coordinates like "5,2"

### **Node Type Filter**
- **All**: Show all node types
- **Start**: Show only start nodes
- **Travel**: Show only travel nodes
- **Notable**: Show only notable nodes
- **Keystone**: Show only keystone nodes
- **Extension**: Show only extension nodes

### **Additional Filters**
- **Show only allocated nodes**: Only show nodes that are purchased
- **Show only nodes with stats**: Only show nodes that have non-zero stats

## 📊 **Bulk Operations**

### **Selecting Cells**
1. **Enable "Bulk Mode"** in the Bulk Operations tab
2. **Go to Cell Browser** tab
3. **Click "Select"** on cells you want to modify
4. **Return to Bulk Operations** to see selected cells

### **Available Operations**
- **Apply Stat Template**: Apply a predefined stat template to all selected cells
- **Clear All Stats**: Set all stats to zero for selected cells
- **Select All**: Select all visible cells (after filtering)
- **Clear Selection**: Deselect all cells

## 🎨 **Stat Categories Explained**

### **Core Attributes**
- **Strength**: Physical damage and health
- **Dexterity**: Attack speed and evasion
- **Intelligence**: Spell damage and mana

### **Combat Resources**
- **Max Health Increase**: Additional maximum health
- **Max Energy Shield Increase**: Additional maximum energy shield
- **Max Mana**: Additional maximum mana
- **Max Reliance**: Additional maximum reliance

### **Combat Stats**
- **Attack Power**: Base attack damage
- **Defense**: Base defense value
- **Critical Chance**: Chance to deal critical hits
- **Critical Multiplier**: Damage multiplier for critical hits
- **Accuracy**: Chance to hit targets

### **Damage Modifiers (Increased)**
- **Physical Damage**: Increased physical damage percentage
- **Fire Damage**: Increased fire damage percentage
- **Cold Damage**: Increased cold damage percentage
- **Lightning Damage**: Increased lightning damage percentage
- **Chaos Damage**: Increased chaos damage percentage
- **Elemental Damage**: Increased elemental damage percentage
- **Spell Damage**: Increased spell damage percentage
- **Attack Damage**: Increased attack damage percentage

### **Added Damage**
- **Added Physical Damage**: Flat physical damage added
- **Added Fire Damage**: Flat fire damage added
- **Added Cold Damage**: Flat cold damage added
- **Added Lightning Damage**: Flat lightning damage added
- **Added Chaos Damage**: Flat chaos damage added
- **Added Elemental Damage**: Flat elemental damage added
- **Added Spell Damage**: Flat spell damage added
- **Added Attack Damage**: Flat attack damage added

### **Elemental Conversions**
- **Physical Damage as Fire**: Convert physical damage to fire
- **Physical Damage as Cold**: Convert physical damage to cold
- **Physical Damage as Lightning**: Convert physical damage to lightning
- **Fire Damage as Cold**: Convert fire damage to cold
- **Cold Damage as Fire**: Convert cold damage to fire
- **Lightning Damage as Fire**: Convert lightning damage to fire

### **Resistances**
- **Physical Resistance**: Resistance to physical damage
- **Fire Resistance**: Resistance to fire damage
- **Cold Resistance**: Resistance to cold damage
- **Lightning Resistance**: Resistance to lightning damage
- **Chaos Resistance**: Resistance to chaos damage
- **Elemental Resistance**: Resistance to all elemental damage
- **All Resistance**: Resistance to all damage types

### **Defense Stats**
- **Armour**: Physical damage reduction
- **Evasion**: Chance to avoid attacks
- **Energy Shield**: Additional protective barrier
- **Block Chance**: Chance to block attacks
- **Dodge Chance**: Chance to dodge attacks
- **Spell Dodge Chance**: Chance to dodge spells
- **Spell Block Chance**: Chance to block spells

### **Recovery Stats**
- **Life Regeneration**: Health regeneration per second
- **Energy Shield Regeneration**: Energy shield regeneration per second
- **Mana Regeneration**: Mana regeneration per second
- **Reliance Regeneration**: Reliance regeneration per second
- **Life Leech**: Life gained from dealing damage
- **Mana Leech**: Mana gained from dealing damage
- **Energy Shield Leech**: Energy shield gained from dealing damage

### **Combat Mechanics**
- **Attack Speed**: Attacks per second
- **Cast Speed**: Spells per second
- **Movement Speed**: Movement speed multiplier
- **Attack Range**: Range of attacks
- **Projectile Speed**: Speed of projectiles
- **Area of Effect**: Size of area effects
- **Skill Effect Duration**: Duration of skill effects
- **Status Effect Duration**: Duration of status effects

### **Card System Stats**
- **Cards Drawn Per Turn**: Additional cards drawn each turn
- **Max Hand Size**: Maximum number of cards in hand
- **Card Draw Chance**: Chance to draw extra cards
- **Card Retention Chance**: Chance to keep cards
- **Card Upgrade Chance**: Chance to upgrade cards
- **Discard Power**: Power of discard effects
- **Mana Per Turn**: Additional mana gained each turn

### **Legacy Stats**
- **Armor Increase**: Legacy armor stat
- **Increased Evasion**: Legacy evasion stat
- **Elemental Resist**: Legacy elemental resistance
- **Spell Power Increase**: Legacy spell power
- **Crit Chance Increase**: Legacy critical chance
- **Crit Multiplier Increase**: Legacy critical multiplier

## 🚨 **Important Notes**

### **Saving Changes**
- **Always click "Apply Changes"** to save your modifications
- **Use "Reset"** to undo changes before applying
- **The editor automatically marks objects as dirty** for saving

### **Performance**
- **Refresh the cell data** if you add/remove cells from boards
- **Use filters** to work with smaller subsets of cells
- **Bulk operations** are more efficient for large changes

### **Best Practices**
1. **Start with a small test** - edit one cell first
2. **Use search/filter** to find specific cells quickly
3. **Use bulk operations** for repetitive changes
4. **Save your work** regularly (Ctrl+S)

## 🎯 **Quick Start Workflow**

1. **Open the editor** (Tools → Passive Tree → JSON Data Editor)
2. **Select your board** in the Board field
3. **Click "Refresh"** to load cells
4. **Use search** to find the cell you want to edit
5. **Click "Edit Stats"** on that cell
6. **Modify the stats** you want to change
7. **Click "Apply Changes"** to save
8. **Repeat** for other cells as needed

## 🔧 **Troubleshooting**

### **No cells found**
- **Check if the board is selected** correctly
- **Click "Refresh"** to reload cell data
- **Check if the board has CellJsonData components**

### **Changes not saving**
- **Make sure to click "Apply Changes"** in the stat editor
- **Check if the cell is selected** properly
- **Try refreshing the cell data**

### **Performance issues**
- **Use filters** to reduce the number of visible cells
- **Work with smaller batches** of cells
- **Close the editor** when not in use

This tool should make managing your passive tree JSON data much more efficient! 🎉

