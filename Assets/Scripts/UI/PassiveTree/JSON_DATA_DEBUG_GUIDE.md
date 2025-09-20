# JSON Data Debug Guide

## 🔍 The Problem

Your tooltip system is showing:
```
🔍 [JsonPassiveTreeTooltip] NodeData for (2, 6): False
```

This means the JsonBoardDataManager can't find any JSON data for cell position (2, 6). The cell is showing default values instead of JSON data.

## 🧪 How to Debug This

### **Step 1: Add the JsonDataTester Script**

1. Create an empty GameObject in your scene
2. Add the `JsonDataTester` component to it
3. Check "Test On Start" in the Inspector

### **Step 2: Run the Test**

**Option A: Automatic**
- Just play the scene - the test will run automatically

**Option B: Manual**
- Right-click the `JsonDataTester` component
- Select "Test JSON Data"

### **Step 3: Check the Console**

The test will show you:

#### **✅ If JSON File is Assigned:**
```
✅ [JsonDataTester] JSON file assigned: CoreBoardData
   File size: 12345 characters
```

#### **❌ If JSON File is Missing:**
```
❌ [JsonDataTester] No JSON file assigned to JsonBoardDataManager!
   Please assign CoreBoardData.json in the inspector.
```

#### **📊 If Data is Loaded:**
```
✅ [JsonDataTester] Data loaded: True
📊 [JsonDataTester] Total nodes loaded: 49
📋 [JsonDataTester] First 5 nodes:
  1. Position (0, 0): 'Strength' (small)
  2. Position (1, 0): 'Dexterity' (small)
  3. Position (2, 0): 'Intelligence' (small)
  ...
```

#### **🎯 If Position (2, 6) Has Data:**
```
✅ [JsonDataTester] Found data for position (2, 6):
   - Name: 'Some Node Name'
   - Description: 'Some description'
   - Type: small
   - Position: (2, 6)
```

#### **⚠️ If Position (2, 6) Has No Data:**
```
⚠️ [JsonDataTester] No data found for position (2, 6)
   Available positions:
     - (0, 0): 'Strength'
     - (1, 0): 'Dexterity'
     - (2, 0): 'Intelligence'
     ...
```

## 🔧 Common Issues and Solutions

### **Issue 1: No JSON File Assigned**
**Problem**: `No JSON file assigned to JsonBoardDataManager!`
**Solution**: 
1. Find the JsonBoardDataManager in your scene
2. In the Inspector, assign `CoreBoardData.json` to the "Board Data Json" field

### **Issue 2: JSON File Not Loading**
**Problem**: `Total nodes loaded: 0`
**Solution**:
1. Check if the JSON file is valid
2. Right-click JsonDataTester and select "Force Reload JSON Data"
3. Check the console for JSON parsing errors

### **Issue 3: Position Mismatch**
**Problem**: Position (2, 6) exists in cells but not in JSON data
**Solution**:
1. Check if your JSON file has data for position (2, 6)
2. Verify that cell positions match JSON positions
3. Use "Show All Loaded Positions" to see what positions are available

### **Issue 4: Cell Position Wrong**
**Problem**: Cell at (2, 6) shows default values
**Solution**:
1. Check if the cell's GridPosition is actually (2, 6)
2. Verify the cell was created correctly
3. Check if the cell has the right position in the inspector

## 🎯 What to Look For

### **Expected Output (Working):**
```
✅ [JsonDataTester] JsonBoardDataManager found: JsonBoardDataManager
✅ [JsonDataTester] JSON file assigned: CoreBoardData
   File size: 12345 characters
✅ [JsonDataTester] Data loaded: True
📊 [JsonDataTester] Total nodes loaded: 49
🎯 [JsonDataTester] Testing position (2, 6)...
✅ [JsonDataTester] Found data for position (2, 6):
   - Name: 'Some Node Name'
   - Description: 'Some description'
   - Type: small
   - Position: (2, 6)
✅ [JsonDataTester] Found cell at position (2, 6):
   - Cell Name: 'Some Node Name'
   - Cell Description: 'Some description'
   - Cell Type: Small
```

### **Problem Output (Not Working):**
```
❌ [JsonDataTester] No JSON file assigned to JsonBoardDataManager!
   Please assign CoreBoardData.json in the inspector.
```

OR

```
⚠️ [JsonDataTester] No data found for position (2, 6)
   Available positions:
     - (0, 0): 'Strength'
     - (1, 0): 'Dexterity'
     - (2, 0): 'Intelligence'
     - (0, 1): 'Health'
     - (1, 1): 'Mana'
     - (2, 1): 'Energy Shield'
     - (0, 2): 'Armor'
     - (1, 2): 'Evasion'
     - (2, 2): 'Resistance'
     - (0, 3): 'Critical Strike'
     - (1, 3): 'Attack Speed'
     - (2, 3): 'Cast Speed'
     - (0, 4): 'Movement Speed'
     - (1, 4): 'Life Regeneration'
     - (2, 4): 'Mana Regeneration'
     - (0, 5): 'Life Leech'
     - (1, 5): 'Mana Leech'
     - (2, 5): 'Energy Shield Leech'
     - (0, 6): 'Block Chance'
     - (1, 6): 'Dodge Chance'
     - (2, 6): 'Spell Block Chance'
```

## 🚀 Next Steps

1. **Run the JsonDataTester** and check the console output
2. **Identify the specific issue** based on the output
3. **Fix the issue** using the solutions above
4. **Test the tooltip system** again by hovering over cells
5. **Let me know what the test shows** so I can help with the specific issue

The JsonDataTester will tell us exactly what's wrong with your JSON data loading!



