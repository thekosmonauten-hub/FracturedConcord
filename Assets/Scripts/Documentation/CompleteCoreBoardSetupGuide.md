# Complete CoreBoard Setup Guide

## **🎯 OVERVIEW**

This guide shows you how to create a complete CoreBoard that matches your original TypeScript project, with all 49 nodes (7x7 grid) including small nodes, notable nodes, extension points, and the starting node.

---

## **📋 COMPLETE BOARD STRUCTURE**

### **Board Specifications:**
- **Size**: 7x7 grid (49 total positions)
- **Total Nodes**: 49 nodes
- **Extension Points**: 4 (top, left, right, bottom)
- **Notable Nodes**: 4 (Path of the Warrior, Path of the Mage, Path of the Sentinel, Path of the Huntress)
- **Starting Node**: 1 (at position 3,3)
- **Small Nodes**: 40 (various stat bonuses)

### **Node Types:**
- **Small**: Basic stat nodes (+10 Strength, +10 Intelligence, etc.)
- **Notable**: Major nodes with multiple effects
- **Main**: Starting node (already allocated)
- **Extension**: Connection points for other boards

---

## **🚀 SETUP METHODS**

### **Method 1: Using the ScriptableObject (Recommended)**

1. **Select your CoreBoard ScriptableObject** in the Project window
2. **Right-click on the component** in the Inspector
3. **Click "Create Complete Core Board"**
4. **Check the console** for confirmation:
   ```
   Complete core board created! Total nodes: 49
   Extension points: 4
   Starting node: START
   ```

### **Method 2: Using the PassiveTreeManager**

1. **Select your PassiveTreeManager GameObject** in the scene
2. **Right-click on the component** in the Inspector
3. **Click "Force Initialize CoreBoard"**
4. **Check the console** for confirmation

### **Method 3: Programmatic Setup**

```csharp
// Create the complete board programmatically
var completeBoard = CoreBoardSetup.CreateCompleteCoreBoard();
```

---

## **📊 NODE LAYOUT**

### **Row 0 (Top Row):**
```
[Int+Str] [Int+Str] [Int] [EXT_TOP] [Int] [Int+Dex] [Int+Dex]
```

### **Row 1:**
```
[Int+Str] [WARRIOR] [Int] [Int] [Int] [MAGE] [Int+Dex]
```

### **Row 2:**
```
[Str] [Str] [Int+Str] [Int] [Int+Dex] [Dex] [Dex]
```

### **Row 3 (Center Row):**
```
[EXT_LEFT] [Str] [Str] [START] [Dex] [Dex] [EXT_RIGHT]
```

### **Row 4:**
```
[Str] [Str] [Health] [All+3] [Evasion] [Dex] [Dex]
```

### **Row 5:**
```
[Str+Dex] [SENTINEL] [Str+Dex] [All+3] [Str+Dex] [HUNTRESS] [Str+Dex]
```

### **Row 6 (Bottom Row):**
```
[Health] [ES] [ES] [EXT_BOTTOM] [ES] [Evasion] [Evasion]
```

---

## **🎨 NOTABLE NODES**

### **Path of the Warrior** (Position 1,1)
- **Effects**: +8% Max Health, +50 Armour, +20 Strength
- **Type**: Notable

### **Path of the Mage** (Position 1,5)
- **Effects**: +16% Spell Damage, +8% Energy Shield, +20 Intelligence
- **Type**: Notable

### **Path of the Sentinel** (Position 5,1)
- **Effects**: +36% Armour and Evasion, +10% Elemental Resistances
- **Type**: Notable

### **Path of the Huntress** (Position 5,5)
- **Effects**: +100 Accuracy, +16% Projectile Damage, +20 Dexterity
- **Type**: Notable

---

## **🔗 EXTENSION POINTS**

### **Extension Points Available:**
1. **Top** (Position 0,3) - Connect to life/armor boards
2. **Left** (Position 3,0) - Connect to physical/chaos boards
3. **Right** (Position 3,6) - Connect to fire/cold/lightning boards
4. **Bottom** (Position 6,3) - Connect to evasion/critical boards

---

## **✅ VERIFICATION STEPS**

### **After Setup, Verify:**

1. **Check Console Output**:
   ```
   Complete core board created! Total nodes: 49
   Extension points: 4
   Starting node: START
   ```

2. **Test in Scene**:
   - Navigate to PassiveTreeScene
   - Verify all nodes appear in the UI
   - Check that the starting node is pre-allocated
   - Verify extension points are visible

3. **Test Node Interactions**:
   - Hover over nodes to see tooltips
   - Click on available nodes to allocate them
   - Verify stat changes when nodes are allocated

---

## **🔧 TROUBLESHOOTING**

### **If Nodes Don't Appear:**

1. **Check Board Data**:
   - Right-click PassiveTreeBoardUI → "Check UI Setup"
   - Verify board data is assigned

2. **Force Refresh**:
   - Right-click PassiveTreeBoardUI → "Refresh Board Visual"
   - Right-click PassiveTreeManager → "Force Initialize CoreBoard"

3. **Check Console**:
   - Look for any error messages
   - Verify the board creation was successful

### **If Stats Don't Work:**

1. **Check Node Stats**:
   - Verify nodes have proper stat dictionaries
   - Check that stat names match your character system

2. **Test Allocation**:
   - Try allocating a simple node first
   - Check if the allocation system works

---

## **🎯 EXPECTED RESULT**

After following this guide, you should have:

- ✅ **49 nodes** in a 7x7 grid
- ✅ **4 notable nodes** with major effects
- ✅ **4 extension points** for board connections
- ✅ **1 starting node** (pre-allocated)
- ✅ **40 small nodes** with various stat bonuses
- ✅ **Proper node connections** (adjacent nodes connected)
- ✅ **Working UI** that displays all nodes
- ✅ **Functional interactions** (hover, click, allocate)

The CoreBoard should now function exactly like your original TypeScript project!
