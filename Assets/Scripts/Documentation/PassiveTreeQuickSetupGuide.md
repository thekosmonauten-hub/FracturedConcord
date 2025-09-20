# Passive Tree Quick Setup Guide

## **🚨 IMMEDIATE FIXES NEEDED**

Based on your debug output, here are the exact steps to fix your issues:

---

## **Step 1: Add PassiveTreeManager to Scene**

1. **Create PassiveTreeManager GameObject**:
   - Right-click in Hierarchy → Create Empty
   - Name it "PassiveTreeManager"

2. **Add PassiveTreeManager Component**:
   - Select the "PassiveTreeManager" GameObject
   - In Inspector, click "Add Component"
   - Search for "PassiveTreeManager" and add it

3. **Verify it's working**:
   - The component should show fields for Core Board Asset, Extension Board Assets, etc.
   - If you see empty text fields, the component wasn't added correctly

---

## **Step 2: Assign Your CoreBoard Asset**

1. **Select the PassiveTreeManager** GameObject
2. **In the Inspector**, find the PassiveTreeManager component
3. **Drag your CoreBoard asset** to the "Core Board Asset" field
4. **Right-click the component** → "Reinitialize Passive Tree"

---

## **Step 3: Add Test Nodes to Your CoreBoard**

Your CoreBoard asset appears to be empty. Let's add some test nodes:

1. **Select your CoreBoard asset** in the Project window
2. **Right-click the component** → "Add Test Node"
3. **Repeat a few times** to add multiple test nodes
4. **Right-click** → "Validate Board" to check everything is correct

---

## **Step 4: Connect Board Data to UI**

1. **Select your BoardContainer** in the scene
2. **Right-click PassiveTreeBoardUI component** → "Set Board Data from Manager"
3. **Check console** for confirmation message
4. **Right-click** → "Refresh Board Visual"

---

## **Step 5: Test the Setup**

1. **Run the scene**
2. **Check console** for the debug report
3. **Look for all ✅ checks** instead of ❌ errors
4. **You should see nodes** appear in the scene

---

## **🔧 TROUBLESHOOTING**

### **If PassiveTreeManager Still Shows as NULL**:
- Make sure you added the component to a GameObject
- Check that the GameObject is active in the hierarchy
- Verify the component is enabled in the Inspector

### **If Board Data Still Shows as NULL**:
- Make sure your CoreBoard asset is assigned to the PassiveTreeManager
- Use "Reinitialize Passive Tree" context menu option
- Check that your CoreBoard asset has nodes (use "Add Test Node")

### **If Nodes Still Don't Appear**:
- Use "Refresh Board Visual" context menu option
- Check that your NodePrefab is assigned to PassiveTreeBoardUI
- Verify the NodePrefab has a PassiveTreeNodeUI component

---

## **✅ EXPECTED DEBUG OUTPUT**

After fixing, your debug output should look like this:

```
=== PASSIVE TREE DEBUG REPORT ===
--- PassiveTreeManager Check ---
✅ PassiveTreeManager found
✅ Core Board Asset: [YourBoardName]
✅ Passive Tree data exists
✅ Core Board has [X] nodes
--- Board UI Check ---
✅ PassiveTreeBoardUI found
✅ Node Prefab: [YourNodePrefab]
✅ NodePrefab has PassiveTreeNodeUI component
✅ Board Data: [YourBoardName]
✅ Board has [X] nodes
--- Prefab Check ---
Found [X] PassiveTreeNodeUI instances in scene
--- Canvas Check ---
✅ Canvas found
✅ EventSystem found
=== END DEBUG REPORT ===
```

---

## **🎯 NEXT STEPS**

Once everything is working:
1. **Test node interactions** (click, hover)
2. **Add skill points** using "Add Test Skill Points"
3. **Verify connections** are drawn between nodes
4. **Customize your board** with real passive nodes

---

## **💡 TIPS**

- **Save your scene** after making changes
- **Use the context menu options** for quick testing
- **Check the console** for any error messages
- **Start with a simple board** (few nodes) for testing
