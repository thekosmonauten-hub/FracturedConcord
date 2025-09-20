# Passive Tree Visual Setup Guide

## **🎨 OVERVIEW**

This guide will help you create the visual representation of your passive tree board in Unity. You'll learn how to set up the UI components, create prefabs, and connect everything to display your board visually.

---

## **🚀 STEP 1: Create the Scene Structure**

### **Set up the UI Hierarchy**

1. **Create Canvas**:
   - Right-click in Hierarchy → UI → Canvas
   - Name it "PassiveTreeCanvas"
   - Set Canvas Scaler to "Scale With Screen Size"
   - Reference Resolution: 1920x1080

2. **Create Board Container**:
   - Right-click on Canvas → UI → Panel
   - Name it "BoardContainer"
   - Set Anchor to center
   - Set size to 800x600 (or your preferred size)

3. **Add Board UI Component**:
   - Select BoardContainer
   - Add Component → PassiveTreeBoardUI

---

## **🎮 STEP 2: Create Node Prefab**

### **Create the Node GameObject**

1. **Create Node Base**:
   - Right-click in Hierarchy → UI → Image
   - Name it "NodePrefab"
   - Set size to 60x60
   - Set Image type to "Simple"
   - Add a border or outline for visibility

2. **Add Node Components**:
   - Add Component → PassiveTreeNodeUI
   - Add Component → Button (for click handling)

3. **Create Node UI Elements**:
   ```
   NodePrefab
   ├── Background (Image)
   ├── Icon (Image) - Optional
   ├── NameText (TextMeshPro)
   ├── CostText (TextMeshPro)
   └── TooltipPanel (Panel)
       └── TooltipText (TextMeshPro)
   ```

4. **Configure Node Elements**:
   - **NameText**: Position at top, small font
   - **CostText**: Position at bottom-right, small font
   - **TooltipPanel**: Position above node, initially disabled
   - **TooltipText**: Configure with appropriate font size

5. **Save as Prefab**:
   - Drag NodePrefab to Project window
   - Save in `Assets/Prefabs/PassiveTree/`
   - Delete from scene

---

## **🔗 STEP 3: Create Connection Line Prefab**

### **Create the Connection Line**

1. **Create Line GameObject**:
   - Right-click in Hierarchy → Create Empty
   - Name it "ConnectionLinePrefab"

2. **Add LineRenderer**:
   - Add Component → LineRenderer
   - Set Material to "Sprites/Default"
   - Set Width to 2
   - Set Color to white

3. **Add ConnectionLineUI Component**:
   - Add Component → ConnectionLineUI

4. **Save as Prefab**:
   - Drag to Project window
   - Save in `Assets/Prefabs/PassiveTree/`
   - Delete from scene

---

## **⚙️ STEP 4: Configure Board UI**

### **Set up the PassiveTreeBoardUI Component**

1. **Select BoardContainer** in the scene

2. **Configure Board UI Settings**:
   ```
   Board Configuration:
   - Node Spacing: 80, 80 (adjust as needed)
   - Board Offset: 0, 0
   
   UI References:
   - Node Prefab: Assign your NodePrefab
   - Connection Line Prefab: Assign your ConnectionLinePrefab
   
   Visual Settings:
   - Allocated Node Color: Green
   - Unallocated Node Color: Gray
   - Unavailable Node Color: Red
   - Connection Line Color: White
   - Connection Line Width: 2
   
   Debug:
   - Show Debug Info: True (for development)
   ```

3. **Connect to Tree Manager**:
   - Right-click on PassiveTreeBoardUI component
   - Choose "Set Board Data from Manager"
   - This will automatically load your assigned CoreBoard

---

## **🎯 STEP 5: Test the Visual Setup**

### **Verify Everything Works**

1. **Run the Scene**:
   - Press Play
   - Check console for initialization messages

2. **Check Visual Elements**:
   - Nodes should appear in their grid positions
   - Connection lines should draw between connected nodes
   - Starting node should be green (allocated)

3. **Test Interactions**:
   - Hover over nodes (should show tooltip)
   - Click on available nodes (should allocate)
   - Click on allocated nodes (should deallocate)

4. **Add Test Points**:
   ```csharp
   // In console or test script
   PassiveTreeManager.Instance.AddSkillPoints(5);
   ```

---

## **🔧 STEP 6: Customize Visuals**

### **Node Visual Customization**

1. **Node Colors by Type**:
   - **Main**: Green (starting point)
   - **Notable**: Blue (powerful nodes)
   - **Keystone**: Magenta (build-defining)
   - **Small**: White (basic stats)
   - **Travel**: Cyan (attribute nodes)
   - **Extension**: Yellow (connection points)

2. **Node Sizes**:
   - **Main/Keystone**: 80x80 (larger)
   - **Notable**: 70x70 (medium)
   - **Small/Travel**: 60x60 (standard)
   - **Extension**: 50x50 (smaller)

3. **Visual Effects**:
   - Add glow effects for allocated nodes
   - Add pulsing animation for available nodes
   - Add particle effects for keystone nodes

### **Connection Line Customization**

1. **Line Styles**:
   - Solid lines for basic connections
   - Dashed lines for extension connections
   - Glowing lines for allocated paths

2. **Line Colors**:
   - White for basic connections
   - Yellow for extension points
   - Green for allocated paths

---

## **📱 STEP 7: Responsive Design**

### **Make it Work on Different Screens**

1. **Canvas Scaler Settings**:
   ```
   UI Scale Mode: Scale With Screen Size
   Reference Resolution: 1920x1080
   Screen Match Mode: Match Width or Height
   Match: 0.5 (or adjust as needed)
   ```

2. **Board Container Scaling**:
   - Use anchors to keep board centered
   - Set minimum and maximum sizes
   - Adjust node spacing for different screen sizes

3. **Mobile Considerations**:
   - Increase node sizes for touch
   - Add touch-friendly tooltips
   - Consider zoom/pan functionality

---

## **🎨 STEP 8: Advanced Visual Features**

### **Add Polish and Effects**

1. **Animations**:
   - Node allocation/deallocation animations
   - Connection line drawing animations
   - Hover effects and transitions

2. **Visual Feedback**:
   - Glow effects for available nodes
   - Pulse animations for new allocations
   - Color transitions for state changes

3. **Information Display**:
   - Points remaining counter
   - Total stats display
   - Board connection status

4. **Accessibility**:
   - High contrast mode
   - Colorblind-friendly palettes
   - Keyboard navigation support

---

## **🔍 STEP 9: Troubleshooting**

### **Common Issues and Solutions**

#### **Nodes Not Appearing**
- Check that NodePrefab is assigned
- Verify board data is loaded
- Check console for error messages
- Use "Refresh Board Visual" context menu

#### **Nodes in Wrong Positions**
- Adjust Node Spacing in Board UI
- Check node position data in ScriptableObject
- Verify grid coordinates are correct

#### **Connections Not Drawing**
- Check that ConnectionLinePrefab is assigned
- Verify LineRenderer component is present
- Check connection data in board

#### **Click Interactions Not Working**
- Ensure PassiveTreeNodeUI component is present
- Check that Button component is added
- Verify event system is in scene

#### **Tooltips Not Showing**
- Check TooltipPanel is assigned
- Verify TooltipText component exists
- Check pointer events are working

---

## **📋 SETUP CHECKLIST**

- [ ] Canvas created with proper settings
- [ ] BoardContainer created and positioned
- [ ] PassiveTreeBoardUI component added
- [ ] NodePrefab created with all components
- [ ] ConnectionLinePrefab created
- [ ] Prefabs assigned to Board UI
- [ ] Board data loaded from manager
- [ ] Nodes appear in correct positions
- [ ] Connection lines draw properly
- [ ] Click interactions work
- [ ] Tooltips display correctly
- [ ] Visual states update properly
- [ ] Responsive design implemented
- [ ] Visual effects added
- [ ] Tested on different screen sizes

---

## **🚀 NEXT STEPS**

Once your visual setup is working:

1. **Add More Boards**: Create extension boards and connect them
2. **Enhance Visuals**: Add animations, effects, and polish
3. **Add Sound**: Implement audio feedback for interactions
4. **Save/Load**: Implement persistence for player progress
5. **Performance**: Optimize for large boards and many nodes
6. **Mobile**: Add touch controls and mobile-specific features

---

This guide provides everything you need to create a beautiful, functional visual representation of your passive tree board. The modular design allows you to easily customize and extend the system as your game grows!
