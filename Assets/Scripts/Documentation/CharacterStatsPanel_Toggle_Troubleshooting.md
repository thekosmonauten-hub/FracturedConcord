# CharacterStatsPanel Toggle Troubleshooting Guide

## Issue: Button Doesn't Hide Panel When Pressed Again

If the CharacterStatsPanel shows when you press the button but doesn't hide when you press it again, follow these troubleshooting steps:

---

## 🔍 **Step 1: Verify Button Connection**

### **Check Button Assignment**
1. **Select the CharacterStatsPanelManager** in the scene
2. **In the Inspector**, verify the "Toggle Button" field is assigned
3. **If not assigned**: Drag your CharacterStatsButton to this field

### **Check Button Event Connection**
1. **Select your CharacterStatsButton** in the scene
2. **In the Inspector**, look at the Button component
3. **Check the OnClick() section**:
   - Should have at least 1 event
   - The event should call `CharacterStatsPanelManager.TogglePanel()`
   - If not connected, drag the CharacterStatsPanelManager to the OnClick field and select `TogglePanel()`

---

## 🔧 **Step 2: Use Debug Tools**

### **Context Menu Debugging**
1. **Select CharacterStatsPanelManager** in the scene
2. **Right-click** on the component
3. **Use these context menu options**:

#### **"Debug Manager State"**
- Shows current state of all components
- Check if button is assigned and connected
- Verify panel visibility state

#### **"Test Toggle Panel"**
- Manually tests the toggle functionality
- Should show/hide the panel
- Check console for toggle messages

#### **"Connect Toggle Button"**
- Manually connects the assigned button
- Use if button connection is missing

#### **"Check Button Events"**
- Analyzes button event listeners
- Identifies multiple event connections
- Shows all components on the button

---

## 🛠️ **Step 3: Manual Button Setup**

### **If Button Connection is Missing**
1. **Select your CharacterStatsButton**
2. **In the Button component's OnClick() section**:
   - Click the **+** button to add an event
   - Drag the **CharacterStatsPanelManager** GameObject to the field
   - Select **CharacterStatsPanelManager → TogglePanel()**

### **Alternative: Use UIManager Integration**
If the direct connection doesn't work, you can use the UIManager:
1. **In the Button's OnClick() section**:
   - Add event
   - Drag **UIManager** to the field
   - Select **UIManager → ToggleCharacterStats()**

---

## 🔍 **Step 4: Check Console Messages**

### **Expected Console Output**
When you press the button, you should see:
```
[CharacterStatsPanelManager] TogglePanel() called - Current state: Hidden
[CharacterStatsPanelManager] Panel toggled: Visible - Panel active: True
```

When you press it again:
```
[CharacterStatsPanelManager] TogglePanel() called - Current state: Visible
[CharacterStatsPanelManager] Panel toggled: Hidden - Panel active: False
```

### **If No Messages Appear**
- Button is not connected to the TogglePanel method
- Check button assignment and OnClick events

### **If Only Show Messages Appear**
- Panel might be getting reactivated elsewhere
- Check if other scripts are calling ShowPanel()

### **If Multiple Toggle Calls Appear**
- Button might have multiple event listeners
- Use "Check Button Events" context menu to analyze
- Remove duplicate OnClick events in the button inspector
- Check for other scripts that might be calling TogglePanel()

---

## 🎯 **Step 5: Common Solutions**

### **Solution 1: Reconnect Button**
1. **Select CharacterStatsPanelManager**
2. **Right-click → "Connect Toggle Button"**
3. **Test the button**

### **Solution 2: Clear and Reassign**
1. **Clear the "Toggle Button" field** in CharacterStatsPanelManager
2. **Drag the button again** to reassign
3. **Use "Connect Toggle Button"** context menu

### **Solution 3: Check for Conflicts**
1. **Look for other scripts** that might be controlling the panel
2. **Check UIManager** for conflicting calls
3. **Ensure only one script** is managing panel visibility

### **Solution 4: Fix Multiple Event Listeners**
1. **Use "Check Button Events"** context menu
2. **Remove duplicate OnClick events** in button inspector
3. **Clear all OnClick events** and reconnect using "Connect Toggle Button"
4. **Check for other scripts** that might be adding listeners to the same button

### **Solution 4: Verify Panel Hierarchy**
1. **Check that the panel GameObject** is the correct one being controlled
2. **Ensure the panel has the CharacterStatsController** component
3. **Verify the panel is a child** of the correct parent

---

## ✅ **Step 6: Verification Checklist**

- [ ] Button is assigned in CharacterStatsPanelManager
- [ ] Button OnClick event calls TogglePanel()
- [ ] Console shows toggle messages when button is pressed
- [ ] Panel shows when button is pressed first time
- [ ] Panel hides when button is pressed second time
- [ ] No other scripts are interfering with panel visibility

---

## 🆘 **If Still Not Working**

### **Emergency Fix: Manual Toggle**
Add this script to your button temporarily:

```csharp
using UnityEngine;
using UnityEngine.UI;

public class ManualToggle : MonoBehaviour
{
    public GameObject characterStatsPanel;
    private bool isVisible = false;
    
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Toggle);
    }
    
    void Toggle()
    {
        isVisible = !isVisible;
        characterStatsPanel.SetActive(isVisible);
        Debug.Log($"Panel toggled: {(isVisible ? "Visible" : "Hidden")}");
    }
}
```

1. **Add this script** to your CharacterStatsButton
2. **Assign the panel** to the characterStatsPanel field
3. **Test the toggle**
4. **Remove this script** once the main system is working

---

## 📞 **Need More Help?**

If the issue persists after following these steps:
1. **Check the console** for any error messages
2. **Use "Debug Manager State"** and share the output
3. **Verify the scene hierarchy** matches the expected structure
4. **Test with a simple button** to isolate the issue
