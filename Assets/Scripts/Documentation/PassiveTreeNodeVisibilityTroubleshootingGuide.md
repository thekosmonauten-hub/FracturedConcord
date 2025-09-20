# Passive Tree Node Visibility Troubleshooting Guide

## **🔍 QUICK DIAGNOSIS**

If nodes aren't appearing at runtime, follow this systematic approach:

### **Step 1: Run the Diagnostic Script**
1. Add `PassiveTreeNodeVisibilityDebugger` component to any GameObject in your scene
2. Run the scene and check the console output
3. Look for ❌ errors and follow the specific instructions

### **Step 2: Common Issues & Solutions**

## **❌ ISSUE 1: PassiveTreeManager Not Found**
**Symptoms**: "PassiveTreeManager not found in scene"
**Solution**:
1. Create a new GameObject named "PassiveTreeManager"
2. Add `PassiveTreeManager` component to it
3. Assign your CoreBoard ScriptableObject to the "Core Board Asset" field

## **❌ ISSUE 2: CoreBoard Has 0 Nodes**
**Symptoms**: "CoreBoard has 0 nodes"
**Solution**:
1. Select your CoreBoard ScriptableObject in the Project window
2. Right-click → "Create Complete Core Board"
3. Check console for confirmation
4. Or use the PassiveTreeManager's "Force Initialize CoreBoard" context menu

## **❌ ISSUE 3: PassiveTreeBoardUI Not Found**
**Symptoms**: "PassiveTreeBoardUI not found in scene"
**Solution**:
1. Create a GameObject named "BoardContainer"
2. Add `PassiveTreeBoardUI` component to it
3. Assign your NodePrefab to the "Node Prefab" field
4. Make sure it's a child of a Canvas

## **❌ ISSUE 4: Node Prefab Not Assigned**
**Symptoms**: "Node prefab not assigned to BoardUI"
**Solution**:
1. Create a NodePrefab if you haven't already:
   - Create UI → Image
   - Add `PassiveTreeNodeUI` component
   - Set Image Type to "Simple"
   - Assign a source image (white sprite works)
   - Save as prefab
2. Assign the prefab to BoardUI's "Node Prefab" field

## **❌ ISSUE 5: No Canvas Found**
**Symptoms**: "No Canvas found in scene"
**Solution**:
1. Create UI → Canvas
2. Set Render Mode to "Screen Space - Overlay"
3. Make sure BoardContainer is a child of the Canvas

## **❌ ISSUE 6: Nodes Created But Not Visible**
**Symptoms**: "Found X PassiveTreeNodeUI instances" but nodes not visible
**Possible Causes**:
1. **Positioning Issue**: Nodes might be off-screen
   - Check `nodeSpacing` and `boardOffset` in PassiveTreeBoardUI
   - Try setting `boardOffset` to (0,0) and `nodeSpacing` to (100,100)
2. **Image Component Issue**: 
   - Check if Image component has a source image
   - Check if Image color is not transparent
   - Check if Image Type is set to "Simple"
3. **Canvas Scale Issue**:
   - Check Canvas Scaler settings
   - Try setting UI Scale Mode to "Scale With Screen Size"

## **🔧 STEP-BY-STEP FIX PROCESS**

### **Phase 1: Basic Setup**
1. **Create PassiveTreeManager**:
   ```
   GameObject → Create Empty → Name: "PassiveTreeManager"
   Add Component → PassiveTreeManager
   Assign CoreBoard ScriptableObject
   ```

2. **Create Canvas Structure**:
   ```
   UI → Canvas → Name: "PassiveTreeCanvas"
   Set Render Mode: Screen Space - Overlay
   Add EventSystem (if not auto-created)
   ```

3. **Create BoardContainer**:
   ```
   Right-click Canvas → Create Empty → Name: "BoardContainer"
   Add Component → PassiveTreeBoardUI
   ```

### **Phase 2: Create Node Prefab**
1. **Create Base Node**:
   ```
   UI → Image → Name: "NodePrefab"
   Set Image Type: Simple
   Assign source image (UI Sprite or White Sprite)
   Set Color: White (or any visible color)
   Set Size: 50x50 (or desired size)
   ```

2. **Add Components**:
   ```
   Add Component → PassiveTreeNodeUI
   Add Component → Button (optional, for interactions)
   ```

3. **Save as Prefab**:
   ```
   Drag to Project window → Save as "NodePrefab"
   Delete from scene
   ```

### **Phase 3: Connect Everything**
1. **Assign Prefab**:
   ```
   Select BoardContainer
   Drag NodePrefab to "Node Prefab" field in PassiveTreeBoardUI
   ```

2. **Configure BoardUI**:
   ```
   Set Node Spacing: (100, 100)
   Set Board Offset: (0, 0)
   Enable "Show Debug Info"
   ```

3. **Initialize CoreBoard**:
   ```
   Select CoreBoard ScriptableObject
   Right-click → "Create Complete Core Board"
   ```

### **Phase 4: Test and Debug**
1. **Run Scene**:
   ```
   Enter Play Mode
   Check console for diagnostic output
   ```

2. **Use Diagnostic Tools**:
   ```
   Select PassiveTreeNodeVisibilityDebugger
   Right-click → "Run Full Diagnostic"
   ```

3. **Force Refresh**:
   ```
   Select PassiveTreeManager
   Right-click → "Force Initialize CoreBoard"
   Select BoardContainer
   Right-click → "Refresh Board Visual"
   ```

## **🎯 ADVANCED TROUBLESHOOTING**

### **Check Node Positioning**
If nodes exist but aren't visible, they might be positioned off-screen:

```csharp
// In PassiveTreeBoardUI, temporarily modify GetNodePosition:
private Vector2 GetNodePosition(Vector2Int gridPos)
{
    // Force all nodes to center for testing
    return Vector2.zero;
}
```

### **Check Image Visibility**
Ensure the Image component is properly configured:

```csharp
// In PassiveTreeNodeUI, add this to Start():
void Start()
{
    var image = GetComponent<Image>();
    if (image != null)
    {
        image.color = Color.red; // Force visible color
        Debug.Log($"Node image color: {image.color}, enabled: {image.enabled}");
    }
}
```

### **Check Canvas Hierarchy**
Ensure proper parent-child relationships:

```
Canvas (Screen Space - Overlay)
├── BoardContainer (PassiveTreeBoardUI)
│   ├── Node_0_0 (PassiveTreeNodeUI)
│   ├── Node_0_1 (PassiveTreeNodeUI)
│   └── ...
└── Other UI Elements
```

## **🚨 EMERGENCY FIXES**

### **Quick Test Node Creation**
Use the diagnostic script's "Check Single Node Creation" to test if the basic setup works:

1. Add `PassiveTreeNodeVisibilityDebugger` to scene
2. Right-click → "Check Single Node Creation"
3. Look for a red test node appearing at (0,0)

### **Force Complete Reset**
If nothing works, try this nuclear option:

1. Delete all PassiveTree-related GameObjects
2. Create fresh setup following Phase 1-4 above
3. Use a simple white sprite for node prefab
4. Set all colors to bright red for visibility

## **✅ VERIFICATION CHECKLIST**

- [ ] PassiveTreeManager exists in scene
- [ ] CoreBoard ScriptableObject is assigned to manager
- [ ] CoreBoard has nodes (use "Create Complete Core Board")
- [ ] Canvas exists with Screen Space - Overlay
- [ ] BoardContainer is child of Canvas
- [ ] PassiveTreeBoardUI component is on BoardContainer
- [ ] NodePrefab is assigned to BoardUI
- [ ] NodePrefab has Image component with source image
- [ ] NodePrefab has PassiveTreeNodeUI component
- [ ] EventSystem exists in scene
- [ ] Console shows no errors
- [ ] Diagnostic script shows all ✅ checks

## **📞 GETTING HELP**

If you're still having issues:

1. **Run the diagnostic script** and share the console output
2. **Check the verification checklist** above
3. **Share your scene hierarchy** (screenshot of Hierarchy window)
4. **Share your PassiveTreeBoardUI inspector** (screenshot)
5. **Share any error messages** from the console

The diagnostic script will identify the exact issue and provide specific solutions!
