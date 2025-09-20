# StatsPanel Integration Guide - MainGameUI Sliding Panel

## 🎯 Overview

This guide will help you integrate the StatsPanel into your MainGameUI as a toggleable sliding panel. The solution includes:
- **StatsPanel prefab** with smooth sliding animations
- **StatsPanelController** script for panel management
- **UIManager integration** for seamless control
- **Backward compatibility** with existing systems

## 🏗️ Step-by-Step Setup

### **Step 1: Create the StatsPanel Prefab**

#### **1.1 Create the Prefab Structure**
1. **Create a new GameObject** in your scene:
   - Right-click in Hierarchy → Create Empty
   - Name it "StatsPanelPrefab"

2. **Add required components**:
   - **RectTransform** (should be auto-added)
   - **UIDocument** component
   - **StatsPanelController** script
   - **StatsPanelRuntime** script

#### **1.2 Configure the RectTransform**
```
StatsPanelPrefab (RectTransform)
├── Anchor: Top-Right (for right-side sliding)
├── Pivot: (1, 1) - Top-Right corner
├── Position: (0, 0, 0) - Screen edge
├── Size: Width = 400, Height = Screen Height
└── Scale: (1, 1, 1)
```

#### **1.3 Configure UIDocument**
- **Panel Settings**: Set to "Screen Space - Overlay"
- **Sort Order**: Set to a high value (e.g., 100) to appear above other UI
- **Visual Tree Asset**: Assign `Assets/UI/CharacterStats/StatsPanel.uxml`
- **Style Sheets**: Add `Assets/UI/CharacterStats/StatsPanel.uss`

#### **1.4 Configure StatsPanelController**
```
Animation Settings:
├── Slide Duration: 0.3 seconds
├── Slide Curve: EaseInOut
├── Slide From Right: true
└── Slide Distance: 400 pixels

Panel Settings:
├── UIDocument: Auto-assigned
├── Panel RectTransform: Auto-assigned
├── Close Button: (assign if you have one)
└── Toggle Button: (assign if you have one)
```

### **Step 2: Create the Prefab Asset**

1. **Drag the configured GameObject** from Hierarchy to your `Assets/Prefabs/UI/` folder
2. **Name it** "StatsPanelPrefab"
3. **Delete the scene instance** (keep the prefab)

### **Step 3: Integrate with MainGameUI Scene**

#### **3.1 Add to MainGameUI Scene**
1. **Open MainGameUI scene**
2. **Drag StatsPanelPrefab** from Project window to scene
3. **Position it** at the right edge of the screen
4. **Ensure it's a child** of your main UI Canvas

#### **3.2 Configure Canvas Settings**
```
Canvas (Canvas component)
├── Render Mode: Screen Space - Overlay
├── Sort Order: 0 (or appropriate value)
└── Additional Shader Channels: None

Canvas Scaler (CanvasScaler component)
├── UI Scale Mode: Scale With Screen Size
├── Reference Resolution: 1920 x 1080
├── Screen Match Mode: Match Width Or Height
└── Match: 0.5 (or your preference)
```

### **Step 4: Update UIManager Integration**

#### **4.1 Assign StatsPanelController**
1. **Select UIManager GameObject** in MainGameUI scene
2. **In Inspector**, find the "Stats Panel Controller" field
3. **Drag the StatsPanelPrefab** from scene to this field

#### **4.2 Verify Integration**
The UIManager will automatically:
- ✅ Find the StatsPanelController if not assigned
- ✅ Use sliding animations when available
- ✅ Fall back to legacy system if needed
- ✅ Provide new methods for stats management

### **Step 5: Add Toggle Button (Optional)**

#### **5.1 Create Toggle Button**
1. **Add a Button** to your UI (e.g., in NavBar)
2. **Configure button appearance**:
   ```
   Button Settings:
   ├── Text: "Stats" or "Character"
   ├── Icon: Character/Stats icon
   └── Style: Match your UI theme
   ```

#### **5.2 Connect Button to UIManager**
1. **Select the button** in Inspector
2. **In OnClick() section**:
   - Click "+" to add new event
   - Drag UIManager GameObject to field
   - Select function: `UIManager.ToggleCharacterStats()`

#### **5.3 Alternative: Direct Controller Connection**
If you want direct control:
1. **Select the button**
2. **In OnClick() section**:
   - Drag StatsPanelPrefab to field
   - Select function: `StatsPanelController.TogglePanel()`

## 🎮 Usage Examples

### **Basic Toggle**
```csharp
// From any script
UIManager uiManager = FindObjectOfType<UIManager>();
uiManager.ToggleCharacterStats();
```

### **Show/Hide Control**
```csharp
// Show the panel
uiManager.ShowCharacterStats();

// Hide the panel
uiManager.HideCharacterStats();

// Check if visible
bool isVisible = uiManager.IsCharacterStatsVisible();
```

### **Refresh Data**
```csharp
// Refresh stats when character data changes
uiManager.RefreshStatsPanel();

// Update with current character data
uiManager.UpdateStatsPanel();
```

### **Direct Controller Access**
```csharp
StatsPanelController controller = FindObjectOfType<StatsPanelController>();
if (controller != null)
{
    controller.ShowPanel();
    controller.RefreshStats();
}
```

## 🎨 Customization Options

### **Animation Customization**
```csharp
// In StatsPanelController Inspector
Animation Settings:
├── Slide Duration: 0.2-0.5 seconds (recommended)
├── Slide Curve: Customize for different effects
├── Slide From Right: true/false (left/right sliding)
└── Slide Distance: 300-500 pixels (adjust to content)
```

### **Position Customization**
```
RectTransform Settings:
├── Anchor: Top-Right, Top-Left, Bottom-Right, Bottom-Left
├── Pivot: Match anchor for smooth sliding
├── Size: Adjust width/height as needed
└── Position: Fine-tune starting position
```

### **Styling Customization**
- **USS File**: Modify `Assets/UI/CharacterStats/StatsPanel.uss`
- **UXML File**: Modify `Assets/UI/CharacterStats/StatsPanel.uxml`
- **Colors**: Update the enhanced header styling
- **Animations**: Add custom CSS transitions

## 🔧 Troubleshooting

### **Panel Not Sliding**
1. **Check RectTransform**: Ensure proper anchor and pivot
2. **Verify Controller**: Ensure StatsPanelController is attached
3. **Check Animation**: Verify slideDistance > 0
4. **Test in Play Mode**: Animations only work in play mode

### **Panel Not Visible**
1. **Check Canvas**: Ensure proper render mode and sort order
2. **Verify UIDocument**: Ensure Visual Tree Asset is assigned
3. **Check USS**: Ensure styles are properly applied
4. **Test Positioning**: Verify panel isn't off-screen

### **UIManager Not Finding Controller**
1. **Check Assignment**: Ensure StatsPanelController is assigned in UIManager
2. **Verify Scene**: Ensure prefab is in the same scene as UIManager
3. **Check Scripts**: Ensure all scripts are compiled without errors
4. **Restart Scene**: Sometimes Unity needs a scene restart

### **Performance Issues**
1. **Reduce Animation Duration**: Use shorter slide times
2. **Simplify Curves**: Use simpler animation curves
3. **Check Updates**: Ensure RefreshStats() isn't called too frequently
4. **Profile**: Use Unity Profiler to identify bottlenecks

## 📋 Integration Checklist

### **Prefab Setup**
- [ ] StatsPanelPrefab created with all components
- [ ] RectTransform properly configured
- [ ] UIDocument assigned with UXML/USS
- [ ] StatsPanelController configured
- [ ] Prefab saved to Assets folder

### **Scene Integration**
- [ ] Prefab added to MainGameUI scene
- [ ] Positioned correctly on screen
- [ ] Child of appropriate Canvas
- [ ] Canvas settings optimized

### **UIManager Integration**
- [ ] StatsPanelController assigned in UIManager
- [ ] Toggle methods working correctly
- [ ] Show/Hide methods functional
- [ ] State tracking accurate

### **Testing**
- [ ] Panel slides in/out smoothly
- [ ] Data displays correctly
- [ ] Colors and styling applied
- [ ] Performance acceptable
- [ ] No console errors

## 🚀 Advanced Features

### **Keyboard Shortcuts**
```csharp
// Add to any script
void Update()
{
    if (Input.GetKeyDown(KeyCode.C)) // 'C' for Character
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        uiManager.ToggleCharacterStats();
    }
}
```

### **Auto-Refresh on Data Changes**
```csharp
// Subscribe to character data changes
void OnCharacterDataChanged()
{
    UIManager uiManager = FindObjectOfType<UIManager>();
    uiManager.RefreshStatsPanel();
}
```

### **Custom Animation Curves**
```csharp
// Create custom animation curve in StatsPanelController
public AnimationCurve customSlideCurve = new AnimationCurve(
    new Keyframe(0, 0, 0, 2),    // Start fast
    new Keyframe(0.5f, 0.5f, 0, 0), // Slow in middle
    new Keyframe(1, 1, 2, 0)     // End fast
);
```

## 🎯 Success Criteria

The integration is successful when:
- ✅ Panel slides in/out smoothly with animation
- ✅ Data displays correctly with proper styling
- ✅ UIManager controls work seamlessly
- ✅ Backward compatibility maintained
- ✅ Performance is acceptable
- ✅ No console errors or warnings
- ✅ Panel integrates well with existing UI

Your StatsPanel is now fully integrated as a professional sliding panel in your MainGameUI! 🎉









