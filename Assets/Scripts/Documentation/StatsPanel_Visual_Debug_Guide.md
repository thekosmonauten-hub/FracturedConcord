# StatsPanel Visual Movement Debug Guide

## 🐛 Issue Description

**Problem**: The StatsPanel reports movement in logs but shows no visual change. The panel should slide in/out from the left side but remains visually static despite animation code executing.

## 🔍 Debugging Steps

### **Step 1: Check Console Logs**

Look for these debug messages in the Console:

```
StatsPanelController: Original position = (X, Y)
StatsPanelController: Visible position = (X, Y), Hidden position = (X, Y)
StatsPanelController: Set initial position to hidden = (X, Y)
StatsPanelController: Starting animation - show=true
StatsPanelController: Animation from (X, Y) to (X, Y)
StatsPanelController: Animation progress 10% - Position: (X, Y)
```

**Expected Behavior**: 
- Original position should be around (0, 0) or your panel's starting position
- Hidden position should be (-400, 0) for left-side sliding
- Visible position should be (0, 0) or your panel's target position
- Animation should show position changes during progress

### **Step 2: Check RectTransform Settings**

In the Inspector, select your StatsPanel GameObject and check:

#### **RectTransform Component**
```
Anchor: Should be Top-Left (0, 1)
Pivot: Should be (0, 1)
Position: Should change during animation
Size: Should be appropriate (e.g., 400 width)
Scale: Should be (1, 1, 1)
```

#### **Common Issues**:
- **Wrong Anchor**: If anchored to center, movement might not be visible
- **Wrong Pivot**: If pivot is center, sliding might appear from wrong direction
- **Parent Constraints**: Parent RectTransform might be constraining movement

### **Step 3: Check Parent Hierarchy**

1. **Select StatsPanel GameObject** in Hierarchy
2. **Check its parent** - should be Canvas or UI container
3. **Verify parent RectTransform** isn't constraining the child
4. **Check for Layout Groups** that might interfere

### **Step 4: Check Canvas Settings**

1. **Select Canvas GameObject**
2. **Check Canvas component settings**:
   - Render Mode: Screen Space - Overlay (recommended)
   - Pixel Perfect: Enabled
   - Sort Order: Appropriate value
3. **Check Canvas Scaler**:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: Appropriate (e.g., 1920x1080)

### **Step 5: Check for UI Toolkit Issues**

Since you're using UI Toolkit (UIDocument), check:

1. **UIDocument component**:
   - Visual Tree Asset: Should be assigned
   - Panel Settings: Should be appropriate
   - Sort Order: Should be high enough to be visible

2. **UI Toolkit Panel Settings**:
   - Render Mode: Screen Space - Overlay
   - Sort Order: Higher than other UI elements

## 🚨 Common Causes & Solutions

### **Issue 1: Panel is Moving Off-Screen**
**Symptoms**: Panel moves but you can't see it
**Solution**: 
- Check if panel is moving beyond screen bounds
- Verify slideDistance isn't too large
- Check if panel is behind other UI elements

### **Issue 2: Parent RectTransform Constraints**
**Symptoms**: Panel tries to move but parent prevents it
**Solution**:
- Check parent's RectTransform settings
- Ensure parent isn't using Layout Groups
- Verify parent's anchor and pivot settings

### **Issue 3: Canvas/UI Scale Issues**
**Symptoms**: Movement is too small to see
**Solution**:
- Check Canvas Scaler settings
- Verify UI scale mode is appropriate
- Test with larger slideDistance values

### **Issue 4: Z-Position Issues**
**Symptoms**: Panel moves but is behind other elements
**Solution**:
- Check Z position in RectTransform
- Verify Canvas sort order
- Check UIDocument sort order

### **Issue 5: Animation Speed Too Fast**
**Symptoms**: Movement happens but is barely visible
**Solution**:
- Increase slideDuration (try 1.0 seconds)
- Check slideCurve settings
- Add more debug logging

## 🔧 Quick Fixes to Try

### **Fix 1: Increase Animation Duration**
```csharp
// In StatsPanelController Inspector
Slide Duration: 1.0 (instead of 0.3)
```

### **Fix 2: Increase Slide Distance**
```csharp
// In StatsPanelController Inspector
Slide Distance: 600 (instead of 400)
```

### **Fix 3: Check RectTransform in Play Mode**
1. **Enter Play Mode**
2. **Select StatsPanel GameObject**
3. **Watch RectTransform position** in Inspector
4. **Verify it changes** during animation

### **Fix 4: Test with Instant Movement**
```csharp
// Temporarily disable animation
SetPanelVisible(true, false); // No animation
```

## 📋 Debug Checklist

### **Before Testing**
- [ ] StatsPanel GameObject is active
- [ ] RectTransform is properly configured
- [ ] Parent hierarchy is correct
- [ ] Canvas settings are appropriate
- [ ] UIDocument is properly assigned

### **During Testing**
- [ ] Console shows debug messages
- [ ] Position values are reasonable
- [ ] Animation progress is logged
- [ ] RectTransform position changes in Inspector
- [ ] No error messages in Console

### **Visual Verification**
- [ ] Panel starts off-screen (hidden)
- [ ] Panel slides in from left when toggled
- [ ] Panel slides out to left when hidden
- [ ] Movement is smooth and visible
- [ ] Panel doesn't disappear behind other UI

## 🎮 Testing Commands

```csharp
// Test instant movement (no animation)
StatsPanelController controller = FindObjectOfType<StatsPanelController>();
controller.SetPanelVisible(true, false); // Should be instant

// Test with longer animation
controller.slideDuration = 2.0f; // 2 seconds
controller.TogglePanel(); // Should be very slow and visible
```

## 🎯 Expected Debug Output

When working correctly, you should see:
```
StatsPanelController: Original position = (0, 0)
StatsPanelController: Visible position = (0, 0), Hidden position = (-400, 0)
StatsPanelController: Set initial position to hidden = (-400, 0)
StatsPanelController: Starting animation - show=true
StatsPanelController: Animation from (-400, 0) to (0, 0)
StatsPanelController: Animation progress 10% - Position: (-360, 0)
StatsPanelController: Animation progress 20% - Position: (-320, 0)
...
StatsPanelController: Animation complete - Final position: (0, 0)
```

**If you see this output but no visual movement, the issue is likely with RectTransform settings or parent constraints.**

## 🚀 Next Steps

1. **Run the debug version** and check Console output
2. **Verify RectTransform settings** in Inspector
3. **Check parent hierarchy** for constraints
4. **Test with instant movement** to isolate animation vs. positioning
5. **Report back** with Console output and RectTransform settings

**This will help us identify exactly why the panel isn't moving visually!** 🔍









