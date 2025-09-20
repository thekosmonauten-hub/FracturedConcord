# Passive Tree System - Implementation Guide

## 🚀 Quick Start Implementation

### **Prerequisites**
- ✅ ScriptableObject assets in `Resources/PassiveTree/`
- ✅ All C# scripts compiled without errors
- ✅ Character stats system integrated
- ✅ UI system ready for integration

---

## 📋 Implementation Steps

### **Step 1: Create Main GameObject Structure**

#### **1.1 Create Passive Tree Manager GameObject**
```
PassiveTreeManager (Empty GameObject)
├── PassiveTreeOrchestrator (Script)
├── PassiveTreeInputHandler (Script)
├── PassiveTreeSelectionManager (Script)
├── PassiveTreeInfoDisplay (Script)
├── PassiveTreeStatsIntegrator (Script)
├── PassiveTreeBoardFactory (Script)
└── PassiveTreeBoardSelector (Script)
```

#### **1.2 Create Board Container**
```
PassiveTreeManager
└── BoardContainer (Empty GameObject)
    └── CoreBoard (Will be created dynamically)
```

#### **1.3 Create UI Container**
```
PassiveTreeManager
└── UIContainer (Empty GameObject)
    ├── InfoText (Text/TextMeshPro)
    └── BoardSelectionPanel (Panel - initially inactive)
        ├── BoardOptionsContainer (Vertical Layout Group)
        ├── SelectButton (Button)
        └── CancelButton (Button)
```

---

### **Step 2: Configure Core Components**

#### **2.1 PassiveTreeOrchestrator Setup**
- **Script**: `PassiveTreeOrchestrator`
- **Auto-Initialize**: ✅ (finds other components automatically)
- **No additional configuration needed**

#### **2.2 PassiveTreeInputHandler Setup**
- **Script**: `PassiveTreeInputHandler`
- **Input System**: Ensure Input System package is installed
- **Camera Reference**: Will auto-find Main Camera
- **No additional configuration needed**

#### **2.3 PassiveTreeSelectionManager Setup**
- **Script**: `PassiveTreeSelectionManager`
- **Passive Points**: Set initial passive points (e.g., 10)
- **Debug Mode**: Enable for testing
- **No additional configuration needed**

#### **2.4 PassiveTreeInfoDisplay Setup**
- **Script**: `PassiveTreeInfoDisplay`
- **Info Text Reference**: Drag InfoText UI element
- **No additional configuration needed**

#### **2.5 PassiveTreeStatsIntegrator Setup**
- **Script**: `PassiveTreeStatsIntegrator`
- **Character Stats Controller**: Will auto-find
- **No additional configuration needed**

---

### **Step 3: Configure Dynamic Board System**

#### **3.1 PassiveTreeBoardFactory Setup**
- **Script**: `PassiveTreeBoardFactory`
- **Board Container**: Drag BoardContainer GameObject
- **Connection Prefab**: Create connection line prefab (optional)
- **Debug Mode**: Enable for testing

#### **3.2 PassiveTreeBoardSelector Setup**
- **Script**: `PassiveTreeBoardSelector`
- **Board Selection Panel**: Drag BoardSelectionPanel
- **Board Option Prefab**: Create board option button prefab
- **Board Options Container**: Drag BoardOptionsContainer
- **Select Button**: Drag SelectButton
- **Cancel Button**: Drag CancelButton
- **Debug Mode**: Enable for testing

---

### **Step 4: Create Board Prefabs**

#### **4.1 Core Board Prefab**
```
CoreBoardPrefab
├── Grid (Grid)
│   └── Tilemap (Tilemap)
│       └── TilemapRenderer (TilemapRenderer)
├── BoardTilemapManager_Simplified (Script)
├── PassiveBoard (Script)
└── PassiveNode (Prefab) - for each node
```

#### **4.2 Extension Board Prefab**
```
ExtensionBoardPrefab
├── Grid (Grid)
│   └── Tilemap (Tilemap)
│       └── TilemapRenderer (TilemapRenderer)
├── BoardTilemapManager_Simplified (Script)
├── PassiveBoard (Script)
└── PassiveNode (Prefab) - for each node
```

#### **4.3 Board Option Button Prefab**
```
BoardOptionButton
├── Button (Button)
├── Image (Image) - for board icon
├── Text (Text/TextMeshPro) - for board name
└── BoardOptionButton (Script) - custom script
```

---

### **Step 5: Configure Visual Assets**

#### **5.1 Tile Sprites**
- **Available Tile**: Theme-based sprite
- **Unavailable Tile**: Transparent grey overlay
- **Preview Tile**: Transparent white overlay
- **Allocated Tile**: `Selected_cell.aseprite`
- **Start Tile**: Special start node sprite
- **Extension Point Tile**: Special extension point sprite

#### **5.2 Board Themes**
- **Core Board**: Default theme
- **Fire Board**: Fire theme sprites
- **Cold Board**: Cold theme sprites
- **Life Board**: Life theme sprites
- **Discard Board**: Discard theme sprites

---

### **Step 6: Test and Debug**

#### **6.1 Basic Functionality Test**
1. **Start Game**: Verify core board loads
2. **Mouse Input**: Test node highlighting
3. **Selection**: Test preview selection
4. **Allocation**: Test permanent selection
5. **Stats**: Verify stat changes

#### **6.2 Dynamic Board Test**
1. **Extension Point**: Click extension point
2. **Board Selection**: Verify selection UI appears
3. **Board Creation**: Test board creation
4. **Connection**: Verify visual connections
5. **Navigation**: Test movement between boards

---

## 📝 Detailed To-Do List

### **🔴 High Priority (Must Complete)**

#### **Unity Setup**
- [ ] **Create PassiveTreeManager GameObject**
  - [ ] Add PassiveTreeOrchestrator script
  - [ ] Add PassiveTreeInputHandler script
  - [ ] Add PassiveTreeSelectionManager script
  - [ ] Add PassiveTreeInfoDisplay script
  - [ ] Add PassiveTreeStatsIntegrator script
  - [ ] Add PassiveTreeBoardFactory script
  - [ ] Add PassiveTreeBoardSelector script

- [ ] **Create BoardContainer GameObject**
  - [ ] Set as child of PassiveTreeManager
  - [ ] Configure as empty GameObject

- [ ] **Create UIContainer GameObject**
  - [ ] Set as child of PassiveTreeManager
  - [ ] Add InfoText UI element
  - [ ] Add BoardSelectionPanel (initially inactive)
  - [ ] Add BoardOptionsContainer with Vertical Layout Group
  - [ ] Add SelectButton and CancelButton

#### **Prefab Creation**
- [ ] **Create CoreBoardPrefab**
  - [ ] Add Grid component
  - [ ] Add Tilemap and TilemapRenderer
  - [ ] Add BoardTilemapManager_Simplified script
  - [ ] Add PassiveBoard script
  - [ ] Configure node prefabs

- [ ] **Create ExtensionBoardPrefab**
  - [ ] Copy CoreBoardPrefab structure
  - [ ] Modify for extension board use
  - [ ] Test with different themes

- [ ] **Create BoardOptionButton Prefab**
  - [ ] Add Button component
  - [ ] Add Image for board icon
  - [ ] Add Text for board name
  - [ ] Create custom BoardOptionButton script

#### **Script Configuration**
- [ ] **Configure PassiveTreeBoardFactory**
  - [ ] Set BoardContainer reference
  - [ ] Set CoreBoardPrefab reference
  - [ ] Set ExtensionBoardPrefab reference
  - [ ] Enable debug mode

- [ ] **Configure PassiveTreeBoardSelector**
  - [ ] Set BoardSelectionPanel reference
  - [ ] Set BoardOptionButton prefab reference
  - [ ] Set BoardOptionsContainer reference
  - [ ] Set SelectButton and CancelButton references
  - [ ] Enable debug mode

- [ ] **Configure PassiveTreeInfoDisplay**
  - [ ] Set InfoText reference
  - [ ] Test text display functionality

---

### **🟡 Medium Priority (Should Complete)**

#### **Visual Assets**
- [ ] **Create Tile Sprites**
  - [ ] Available tile (theme-based)
  - [ ] Unavailable tile (grey overlay)
  - [ ] Preview tile (white overlay)
  - [ ] Allocated tile (Selected_cell.aseprite)
  - [ ] Start tile (special sprite)
  - [ ] Extension point tile (special sprite)

- [ ] **Configure Board Themes**
  - [ ] Core board theme
  - [ ] Fire board theme
  - [ ] Cold board theme
  - [ ] Life board theme
  - [ ] Discard board theme

#### **UI Styling**
- [ ] **Style Board Selection Panel**
  - [ ] Background and borders
  - [ ] Button styling
  - [ ] Text formatting
  - [ ] Layout optimization

- [ ] **Style Info Text**
  - [ ] Font and size
  - [ ] Color scheme
  - [ ] Positioning
  - [ ] Animation effects

#### **Testing and Debugging**
- [ ] **Basic Functionality Tests**
  - [ ] Core board loading
  - [ ] Mouse input accuracy
  - [ ] Node selection
  - [ ] Path validation
  - [ ] Stat integration

- [ ] **Dynamic Board Tests**
  - [ ] Extension point detection
  - [ ] Board selection UI
  - [ ] Board creation
  - [ ] Board connections
  - [ ] Cross-board navigation

---

### **🟢 Low Priority (Nice to Have)**

#### **Advanced Features**
- [ ] **Animation System**
  - [ ] Node selection animations
  - [ ] Board creation animations
  - [ ] Connection line animations
  - [ ] UI transition animations

- [ ] **Sound Integration**
  - [ ] Selection sounds
  - [ ] Allocation sounds
  - [ ] Board creation sounds
  - [ ] Error sounds

- [ ] **Visual Effects**
  - [ ] Particle effects for selections
  - [ ] Glow effects for active nodes
  - [ ] Connection line effects
  - [ ] Board transition effects

#### **Performance Optimization**
- [ ] **Object Pooling**
  - [ ] Board prefab pooling
  - [ ] Connection line pooling
  - [ ] UI element pooling

- [ ] **Memory Management**
  - [ ] Efficient cleanup
  - [ ] Garbage collection optimization
  - [ ] Resource unloading

#### **User Experience**
- [ ] **Accessibility Features**
  - [ ] Keyboard navigation
  - [ ] Screen reader support
  - [ ] High contrast mode
  - [ ] Colorblind support

- [ ] **Advanced UI**
  - [ ] Board preview system
  - [ ] Stat comparison tooltips
  - [ ] Search and filter
  - [ ] Undo/redo system

---

## 🐛 Common Issues and Solutions

### **Issue 1: Mouse Input Not Working**
**Symptoms**: No node highlighting or selection
**Solutions**:
- Check Input System package is installed
- Verify camera reference in PassiveTreeInputHandler
- Check coordinate correction values
- Enable debug logging

### **Issue 2: Boards Not Loading**
**Symptoms**: Core board doesn't appear
**Solutions**:
- Verify ScriptableObject assets in Resources/PassiveTree/
- Check BoardContainer reference in PassiveTreeBoardFactory
- Verify prefab references
- Check debug logs for loading errors

### **Issue 3: Stats Not Updating**
**Symptoms**: Character stats don't change
**Solutions**:
- Verify CharacterStatsController exists in scene
- Check PassiveTreeStatsIntegrator configuration
- Verify stat property names match
- Check debug logs for stat changes

### **Issue 4: Board Selection UI Not Appearing**
**Symptoms**: Extension point click doesn't show selection
**Solutions**:
- Check BoardSelectionPanel is inactive initially
- Verify PassiveTreeBoardSelector configuration
- Check extension point detection
- Verify event system is working

---

## 📊 Success Criteria

### **Functional Requirements**
- [ ] Core board loads automatically
- [ ] Mouse input works accurately
- [ ] Node selection and deselection works
- [ ] Path validation prevents invalid selections
- [ ] Stats update in real-time
- [ ] Extension points trigger board selection
- [ ] Dynamic boards create and connect properly
- [ ] UI displays correct information

### **Performance Requirements**
- [ ] No frame rate impact
- [ ] Smooth input response (<16ms)
- [ ] Efficient memory usage
- [ ] No memory leaks

### **User Experience Requirements**
- [ ] Intuitive controls
- [ ] Clear visual feedback
- [ ] Responsive UI
- [ ] Error handling

---

## 🎯 Next Steps After Implementation

1. **User Testing**: Test with real users
2. **Performance Profiling**: Optimize if needed
3. **Feature Expansion**: Add new board types
4. **Integration**: Connect with other game systems
5. **Polish**: Add animations and effects

---

*Last Updated: December 2024*  
*Status: Ready for Implementation*

