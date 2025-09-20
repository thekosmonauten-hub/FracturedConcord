# CellJsonData Usage Guide

## 🎯 **Enhanced CellJsonData Component**

The `CellJsonData` component now includes JSON file assignment and context menu options for easy data loading.

## 🔧 **Setup Instructions:**

### **Step 1: Add CellJsonData Component**
1. Select any Cell game object (e.g., "Cell_0_0")
2. Add the `CellJsonData` component to it

### **Step 2: Assign JSON File**
1. In the Inspector, find the "JSON Data Source" section
2. Assign your `CoreBoardData.json` file to the "JSON File" field
3. Set the "Board Id" field (default: "core_board")

### **Step 3: Load JSON Data**
Right-click the `CellJsonData` component and choose from these context menu options:

## 📋 **Context Menu Options:**

### **"Load JSON Data for This Cell"**
- Finds JSON data by matching the cell's position (x, y coordinates)
- Example: Cell_2_3 will look for JSON data at position (2, 3)

### **"Load JSON Data by ID Pattern"** ⭐ **Recommended**
- Finds JSON data by matching the expected ID pattern
- Example: Cell_0_0 will look for ID "Core_0_0"
- Example: Cell_2_3 will look for ID "Core_2_3"
- This is the most reliable method for your JSON structure

### **"Show Available JSON IDs"**
- Lists all available IDs in the JSON file for debugging
- Helps you see what IDs are available to match

### **"Clear JSON Data"**
- Removes all JSON data from the cell
- Useful for testing or resetting

## 🎯 **ID Pattern Matching:**

The system automatically generates expected IDs based on:
- **Board ID**: "core_board" → "Core"
- **Cell Position**: (0, 0) → "0_0"
- **Final ID**: "Core_0_0"

### **For Future Extension Boards:**
- Change the "Board Id" field to match your new board
- Example: "combat_board" → "Combat_0_0"
- Example: "magic_board" → "Magic_0_0"

## 🧪 **Testing:**

1. **Assign JSON file** to a CellJsonData component
2. **Right-click** the component
3. **Select "Load JSON Data by ID Pattern"**
4. **Check the console** for success/error messages
5. **Hover over the cell** to test the tooltip

## 📊 **Expected Results:**

### **Success:**
```
[CellJsonData] Successfully loaded JSON data for Cell_0_0 with ID 'Core_0_0': 'Intelligence & Strength'
```

### **Error (ID not found):**
```
[CellJsonData] No JSON data found with ID 'Core_0_0' in CoreBoardData
[CellJsonData] Available IDs: Cell_0_0, Cell_1_0, Cell_2_0, ...
```

### **Error (No JSON file):**
```
[CellJsonData] No JSON file assigned to Cell_0_0. Please assign a JSON file first.
```

## 🔍 **Debugging:**

### **Check Available IDs:**
1. Right-click CellJsonData component
2. Select "Show Available JSON IDs"
3. Look for the ID pattern you expect

### **Verify Cell Names:**
- Make sure your Cell game objects are named correctly: "Cell_0_0", "Cell_1_0", etc.
- The system reads the position from the game object name

### **Check JSON Structure:**
- Ensure your JSON has the correct ID format: "Core_0_0", "Core_1_0", etc.
- Verify the JSON file is assigned correctly

## 🚀 **Benefits:**

- **✅ Manual Control** - You decide which cells get JSON data
- **✅ Easy Assignment** - Right-click context menu options
- **✅ Future-Proof** - Works with any extension board
- **✅ Debug-Friendly** - Shows available IDs and error messages
- **✅ Flexible** - Can assign different JSON files to different cells

## 🎯 **Workflow:**

1. **Add CellJsonData** to cells you want to have JSON data
2. **Assign JSON file** to each component
3. **Right-click and select** "Load JSON Data by ID Pattern"
4. **Test tooltips** by hovering over cells
5. **Repeat** for other cells as needed

This approach gives you complete control while making the assignment process quick and easy!



