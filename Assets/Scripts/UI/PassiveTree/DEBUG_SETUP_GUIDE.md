# Passive Tree Tooltip Debug Setup Guide

## 🔧 How to Debug Your Tooltip System

I've added comprehensive debug logging to help you identify why tooltips aren't showing. Here's how to use it:

### **Step 1: Add the Debug Script**

1. Create an empty GameObject in your scene
2. Add the `PassiveTreeDebugger` component to it
3. In the Inspector, check "Debug On Start" to run diagnostics automatically

### **Step 2: Run the Diagnostic**

**Option A: Automatic (Recommended)**
- Just play the scene - the debugger will run automatically

**Option B: Manual**
- Right-click the `PassiveTreeDebugger` component
- Select "Run Full Diagnostic"

### **Step 3: Check the Console**

The debugger will show you:

#### **📊 JSON Data Loading**
- Whether your `CoreBoardData.json` is loaded
- How many nodes were loaded
- Sample node data (name, description, stats)

#### **🔲 Cell Data Assignment**
- How many CellController components exist
- What data each cell has (name, description, type)
- Whether cells are properly configured

#### **💬 Tooltip System Components**
- Whether `JsonPassiveTreeTooltip` exists
- Whether tooltip prefab is assigned
- Whether data manager is connected
- Whether EventSystem and Canvas are set up

#### **🎯 Event System**
- Whether EventSystem exists (required for hover events)
- Whether Canvas has GraphicRaycaster (required for UI events)

### **Step 4: Test Specific Cells**

1. Set `Test Cell Position` to a specific cell (e.g., 0,0 or 3,3)
2. Check "Simulate Hover" to test tooltip directly
3. Right-click and select "Test Tooltip System"

### **Step 5: Enable Verbose Logging**

1. Check "Enable Verbose Logging" for detailed output
2. Check "Test All Cells" to see data for every cell

## 🔍 What to Look For

### **Common Issues and Solutions:**

#### **❌ "No JSON data found for cell"**
- **Problem**: JSON file not loaded or cell position doesn't match JSON data
- **Solution**: Check if `JsonBoardDataManager` has the correct JSON file assigned

#### **❌ "No tooltip manager found"**
- **Problem**: Missing `JsonPassiveTreeTooltip` component
- **Solution**: Add `JsonPassiveTreeTooltip` to your scene

#### **❌ "No EventSystem found"**
- **Problem**: Missing EventSystem (required for hover events)
- **Solution**: Add EventSystem to your scene (GameObject → UI → Event System)

#### **❌ "No GraphicRaycaster found"**
- **Problem**: Canvas missing GraphicRaycaster
- **Solution**: Add GraphicRaycaster component to your Canvas

#### **❌ "Cell is null" or "No cells found"**
- **Problem**: CellController components not created or positioned incorrectly
- **Solution**: Check your cell creation system

## 🧪 Manual Testing

### **Test Hover Events:**
1. Play the scene
2. Hover over cells in your passive tree
3. Check console for hover event logs
4. Look for tooltip creation logs

### **Test JSON Data:**
1. Look for logs showing JSON data loading
2. Check if node names, descriptions, and stats are displayed
3. Verify cell positions match JSON positions

### **Test Tooltip Creation:**
1. Look for logs showing tooltip manager found/not found
2. Check for tooltip creation success/failure
3. Verify tooltip positioning and content

## 📋 Expected Console Output

When working correctly, you should see:

```
🔧 [PassiveTreeDebugger] ===== FULL DIAGNOSTIC START =====
📊 [PassiveTreeDebugger] Checking JSON data loading...
✅ [PassiveTreeDebugger] JsonBoardDataManager found: JsonBoardDataManager
  - Is Data Loaded: True
  - JSON File: CoreBoardData
  - Total Nodes Loaded: 49
    Node 1: 'Strength' at (0, 0) - Type: small
    Node 2: 'Dexterity' at (1, 0) - Type: small
    ...

🔲 [PassiveTreeDebugger] Checking cell data assignment...
✅ [PassiveTreeDebugger] Found 49 CellController components
  Cell 1: Position (0, 0)
    - Node Name: 'Strength'
    - Description: 'Increases physical damage'
    - Node Type: Small
    - Is Available: True
    - Is Unlocked: True
  ...

💬 [PassiveTreeDebugger] Checking tooltip system components...
  - JsonPassiveTreeTooltip: ✅ Found
    - Tooltip Prefab: ✅ Assigned
    - Data Manager: ✅ Connected
  - EventSystem: ✅ Found
  - Canvas: ✅ Found
    - Canvas 'Canvas': GraphicRaycaster ✅

🧪 [PassiveTreeDebugger] Testing cell at (0, 0)...
✅ [PassiveTreeDebugger] Found cell at (0, 0):
  - Node Name: 'Strength'
  - Description: 'Increases physical damage'
  - Node Type: Small
  - JSON Data Found:
    - Name: 'Strength'
    - Description: 'Increases physical damage'
    - Type: small
    - Stats:
      - Physical Damage: 5
      - Health: 10

🔧 [PassiveTreeDebugger] ===== FULL DIAGNOSTIC COMPLETE =====
```

## 🚀 Next Steps

1. **Run the diagnostic** and check the console output
2. **Identify any missing components** or data issues
3. **Fix the issues** based on the diagnostic results
4. **Test hover events** to see if tooltips now appear
5. **Let me know what the diagnostic shows** so I can help with specific issues

The debug system will tell us exactly what's wrong with your tooltip setup!



