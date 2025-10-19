# Passive Tree JSON Data Editor - Template Preset System

## ✅ **New Feature: Stat Template Presets!**

The editor now includes a comprehensive preset system for stat templates, making it easy to save, load, and manage common stat configurations for repeat use.

## 🎯 **What Are Template Presets?**

Template presets are **pre-configured stat templates** that you can save, load, and reuse. Instead of manually setting up stats every time, you can:

- ✅ **Save** your current stat template as a named preset
- ✅ **Load** any saved preset to quickly apply common stat sets
- ✅ **Delete** presets you no longer need
- ✅ **Use built-in presets** for common character archetypes

## 🚀 **Built-in Presets**

The system comes with **8 ready-to-use presets**:

### **1. Warrior**
- **Strength**: 10
- **Attack Power**: 15
- **Increased Physical Damage**: 20%
- **Armour**: 25

### **2. Mage**
- **Intelligence**: 15
- **Increased Spell Damage**: 25%
- **Max Mana**: 50
- **Increased Fire Damage**: 15%

### **3. Rogue**
- **Dexterity**: 12
- **Critical Chance**: 10%
- **Critical Multiplier**: 25%
- **Evasion**: 20

### **4. Tank**
- **Max Health Increase**: 30%
- **Armour**: 40
- **Physical Resistance**: 15%
- **Life Regeneration**: 5

### **5. Fire Elemental**
- **Increased Fire Damage**: 30%
- **Added Fire Damage**: 20
- **Fire Resistance**: 25%
- **Chance to Ignite**: 15%

### **6. Cold Elemental**
- **Increased Cold Damage**: 30%
- **Added Cold Damage**: 20
- **Cold Resistance**: 25%
- **Chance to Freeze**: 15%

### **7. Lightning Elemental**
- **Increased Lightning Damage**: 30%
- **Added Lightning Damage**: 20
- **Lightning Resistance**: 25%
- **Chance to Shock**: 15%

### **8. Card System**
- **Cards Drawn Per Turn**: 2
- **Max Hand Size**: 3
- **Card Draw Chance**: 25%
- **Mana Per Turn**: 5

## 🎮 **How to Use Template Presets**

### **Step 1: Open the Editor**
1. Go to **Tools → Passive Tree → JSON Data Editor**
2. Select your board prefab
3. Go to the **"Bulk Operations"** tab

### **Step 2: Load a Preset**
1. **Select a preset** from the dropdown menu
2. Click **"Load Preset"** to apply it to your current template
3. The stat template will be updated with the preset's values

### **Step 3: Apply to Cells**
1. **Select cells** in the grid view or individual cell list
2. Click **"Apply Stat Template"** to apply the loaded preset
3. **Node names will be auto-generated** if enabled

### **Step 4: Save Your Own Presets**
1. **Edit the stat template** manually or load an existing preset
2. **Modify the stats** to your desired values
3. **Enter a name** in the "Save as:" field
4. Click **"Save Preset"** to store it for future use

## 🛠️ **Preset Management Features**

### **Load Preset**
- **Select** from dropdown menu
- **Click "Load Preset"** to apply to current template
- **Instant application** of all preset stats

### **Save Preset**
- **Enter name** in "Save as:" field
- **Click "Save Preset"** to store current template
- **Automatic selection** of newly saved preset

### **Delete Preset**
- **Select preset** to delete
- **Click "Delete Preset"** to remove it
- **Automatic index adjustment** after deletion

### **Export Presets**
- **Tools → Passive Tree → Export Template Presets**
- **Console output** showing all presets and their stats
- **Useful for sharing** preset configurations

### **Reset Presets**
- **Tools → Passive Tree → Reset Template Presets**
- **Restores default presets** if you've modified them
- **Clears custom presets** and reloads built-ins

## 🎨 **Workflow Examples**

### **Example 1: Creating Fire Nodes**
1. **Load "Fire Elemental" preset**
2. **Select multiple cells** in a fire-themed area
3. **Apply template** to create consistent fire nodes
4. **Generated names**: "Cell_3_4_Increased Fire Damage & Added Fire Damage"

### **Example 2: Creating Warrior Nodes**
1. **Load "Warrior" preset**
2. **Modify stats** if needed (e.g., increase strength to 15)
3. **Save as "Strong Warrior"** for future use
4. **Apply to warrior-themed cells**

### **Example 3: Creating Card System Nodes**
1. **Load "Card System" preset**
2. **Select cells** in card-themed areas
3. **Apply template** to create card-related nodes
4. **Generated names**: "Cell_2_1_Cards Drawn Per Turn & Max Hand Size"

## 🔧 **Advanced Features**

### **Custom Preset Creation**
1. **Start with any preset** or blank template
2. **Modify stats** to your specifications
3. **Save with descriptive name** (e.g., "High Crit Rogue")
4. **Reuse anytime** for similar node types

### **Preset Modification**
1. **Load existing preset**
2. **Edit stats** as needed
3. **Save with new name** to preserve original
4. **Or overwrite** by saving with same name

### **Bulk Operations with Presets**
1. **Load preset** for desired node type
2. **Select multiple cells** in grid view
3. **Apply template** to all selected cells
4. **Consistent stat application** across multiple nodes

## 📋 **Best Practices**

### **Preset Naming**
- **Use descriptive names**: "High Damage Warrior", "Tank Support", "Fire Mage"
- **Include stat focus**: "Crit Rogue", "Health Tank", "Mana Mage"
- **Keep names short** but clear

### **Preset Organization**
- **Group by theme**: Warrior, Mage, Rogue, Elemental, Card System
- **Create variations**: "Basic Warrior", "Advanced Warrior", "Berserker"
- **Delete unused presets** to keep list clean

### **Workflow Efficiency**
- **Create presets** for common node types first
- **Use presets** for bulk operations on similar nodes
- **Modify presets** rather than starting from scratch
- **Export presets** to share with team members

## 🎉 **Benefits**

### **Time Saving**
- ✅ **No manual stat entry** for common configurations
- ✅ **Quick application** of proven stat sets
- ✅ **Consistent results** across similar nodes

### **Organization**
- ✅ **Named presets** for easy identification
- ✅ **Categorized by function** (Warrior, Mage, etc.)
- ✅ **Easy management** with load/save/delete

### **Flexibility**
- ✅ **Custom presets** for project-specific needs
- ✅ **Modify existing presets** for variations
- ✅ **Share presets** with team members

### **Quality Assurance**
- ✅ **Tested stat combinations** in built-in presets
- ✅ **Consistent naming** with auto-generation
- ✅ **Easy verification** with export functionality

## 🧪 **Testing Your Presets**

### **Test 1: Load and Apply**
1. **Load "Warrior" preset**
2. **Select a cell** at position (3,4)
3. **Apply template** and verify stats
4. **Check generated name**: "Cell_3_4_Strength & Attack Power & Increased Physical Damage"

### **Test 2: Create Custom Preset**
1. **Load "Mage" preset**
2. **Modify intelligence** to 20
3. **Save as "Smart Mage"**
4. **Load "Smart Mage"** and verify changes

### **Test 3: Bulk Application**
1. **Load "Fire Elemental" preset**
2. **Select 3-4 cells** in grid view
3. **Apply template** to all selected cells
4. **Verify consistent stats** across all cells

## 🎯 **Result**

You now have a **powerful preset system** that makes stat template management efficient and organized! 

**No more repetitive stat entry - just load, apply, and go!** 🎉

