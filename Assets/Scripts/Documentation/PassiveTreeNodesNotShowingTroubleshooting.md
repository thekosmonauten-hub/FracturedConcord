# Passive Tree Nodes Not Showing - Troubleshooting Guide

## **🔍 PROBLEM: Nodes Not Appearing at Runtime**

### **Symptoms**
- PassiveTreeBoardUI is configured correctly
- Prefabs are assigned
- No compilation errors
- But nodes don't appear in the scene

---

## **🚀 QUICK DIAGNOSIS**

### **Step 1: Use the Debug Script**
1. **Add PassiveTreeDebugger** to any GameObject in your scene
2. **Run the scene**
3. **Check console** for the detailed debug report
4. **Look for ❌ errors** in the output

### **Step 2: Common Issues & Fixes**

#### **Issue 1: Board Data Not Loaded**
**Symptoms**: "❌ Board Data is NULL!" in debug output
**Fix**:
1. Select your BoardContainer
2. Right-click PassiveTreeBoardUI component
3. Choose "Set Board Data from Manager"
4. Check console for confirmation

#### **Issue 2: CoreBoard Not Assigned to Manager**
**Symptoms**: "❌ Core Board Asset is NOT ASSIGNED!" in debug output
**Fix**:
1. Select PassiveTreeManager in scene
2. Drag your CoreBoard asset to "Core Board Asset" field
3. Right-click → "Reinitialize Passive Tree"

#### **Issue 3: Prefabs Not Assigned**
**Symptoms**: "❌ Node Prefab is NOT ASSIGNED!" in debug output
**Fix**:
1. Select BoardContainer
2. In PassiveTreeBoardUI component
3. Drag your NodePrefab to "Node Prefab" field
4. Drag your ConnectionLinePrefab to "Connection Line Prefab" field

#### **Issue 4: Board Not Refreshing**
**Symptoms**: No error messages but nodes still don't appear
**Fix**:
1. Select BoardContainer
2. Right-click PassiveTreeBoardUI component
3. Choose "Refresh Board Visual"

---

## **🔧 MANUAL VERIFICATION STEPS**

### **Check 1: PassiveTreeManager Setup**
```
✅ PassiveTreeManager found
✅ Core Board Asset: [YourBoardName]
✅ Passive Tree data exists
✅ Core Board has [X] nodes
```

### **Check 2: Board UI Setup**
```
✅ PassiveTreeBoardUI found
✅ Node Prefab: [YourNodePrefab]
✅ NodePrefab has PassiveTreeNodeUI component
✅ Board Data: [YourBoardName]
✅ Board has [X] nodes
```

### **Check 3: Scene Objects**
```
Found [X] PassiveTreeNodeUI instances in scene
✅ Canvas found
✅ EventSystem found
```

---

## **🎯 STEP-BY-STEP FIX PROCESS**

### **Phase 1: Verify Manager Setup**
1. **Select PassiveTreeManager** in scene
2. **Check "Core Board Asset"** field has your board assigned
3. **Right-click → "Check Board Assignments"**
4. **If not assigned**: Drag your CoreBoard asset to the field
5. **Right-click → "Reinitialize Passive Tree"**

### **Phase 2: Verify Board UI Setup**
1. **Select BoardContainer** in scene
2. **Check PassiveTreeBoardUI component**:
   - Node Prefab assigned
   - Connection Line Prefab assigned (optional)
3. **Right-click → "Set Board Data from Manager"**
4. **Check console** for confirmation message

### **Phase 3: Force Refresh**
1. **Select BoardContainer**
2. **Right-click PassiveTreeBoardUI → "Refresh Board Visual"**
3. **Check console** for any error messages

### **Phase 4: Add Test Data**
1. **Select PassiveTreeManager**
2. **Right-click → "Add Test Skill Points"**
3. **Check if nodes become interactive**

---

## **🔍 ADVANCED TROUBLESHOOTING**

### **If Debug Script Shows No Errors But Nodes Still Don't Appear**

#### **Check 1: Canvas Settings**
- **Render Mode**: Should be "Screen Space - Overlay"
- **Canvas Scaler**: Should be "Scale With Screen Size"
- **Reference Resolution**: Set to your target resolution

#### **Check 2: Board Container Position**
- **Select BoardContainer**
- **Check RectTransform**:
  - Anchors: Should be set appropriately
  - Position: Should be visible on screen
  - Size: Should have non-zero width/height

#### **Check 3: Node Prefab Setup**
- **Select NodePrefab** in Project window
- **Verify components**:
  - ✅ Image component
  - ✅ PassiveTreeNodeUI component
  - ✅ RectTransform
  - ✅ Source Image assigned to Image component

#### **Check 4: Board Data Content**
- **Select your CoreBoard asset**
- **Check if it has nodes**:
  - Right-click → "Add Test Node" if empty
  - Verify nodes have valid positions

---

## **🚨 EMERGENCY FIXES**

### **If Nothing Works: Complete Reset**
1. **Delete BoardContainer** from scene
2. **Create new GameObject** named "BoardContainer"
3. **Add PassiveTreeBoardUI component**
4. **Assign prefabs** to the component
5. **Right-click → "Set Board Data from Manager"**
6. **Right-click → "Refresh Board Visual"**

### **If Still No Nodes: Manual Creation**
1. **Select BoardContainer**
2. **Right-click → "Create Board Visual"**
3. **Check console** for any error messages
4. **If errors**: Follow the error messages to fix the issue

---

## **📋 VERIFICATION CHECKLIST**

- [ ] PassiveTreeManager exists in scene
- [ ] CoreBoard asset is assigned to manager
- [ ] BoardContainer exists in scene
- [ ] PassiveTreeBoardUI component is on BoardContainer
- [ ] NodePrefab is assigned to PassiveTreeBoardUI
- [ ] Board data is loaded (use "Set Board Data from Manager")
- [ ] Board visual is created (use "Refresh Board Visual")
- [ ] Canvas is properly configured
- [ ] EventSystem exists in scene
- [ ] No compilation errors
- [ ] Debug script shows all ✅ checks

---

## **🎯 NEXT STEPS**

Once nodes are showing:
1. **Test node interactions** (click, hover)
2. **Add skill points** to test allocation
3. **Verify connections** are drawn
4. **Test board expansion** with extension boards

---

## **💡 TIPS**

- **Always check the console** for error messages
- **Use the debug script** to identify issues quickly
- **Context menu options** are your friends for testing
- **Start with a simple board** (few nodes) for testing
- **Save your scene** before making major changes
