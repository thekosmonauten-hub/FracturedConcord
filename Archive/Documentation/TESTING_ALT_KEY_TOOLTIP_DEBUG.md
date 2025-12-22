# Testing ALT-Key Tooltip Updates - Debug Guide 🔍

**Date:** December 4, 2025  
**Issue:** ALT key press not updating tooltip while hovering  
**Status:** Debugging in progress

---

## 🧪 **Test Steps:**

1. **Hover over weapon** in inventory
2. **Watch Console** for debug messages
3. **Press ALT** (keep hovering)
4. **Check Console** for ALT detection

---

## 📊 **What to Look For:**

### **Scenario 1: Update() is Running**

**Expected Console Output:**
```
[WeaponTooltip] Input System ALT detected: True
[WeaponTooltip] ALT state changed: True (showRanges: True)
[TooltipFormat] Formatting rolled affix 'Devastating' (isRolled: True, rolledValue: 63)
[TooltipFormat] Single-range: '+8–14% increased Fire Damage' → '+8–14% increased Fire Damage'
```

**This means:**
- ✅ Update() is running
- ✅ ALT key is detected
- ✅ Tooltip is refreshing
- ✅ Should be working!

---

### **Scenario 2: Update() Not Running**

**Console Output:**
```
(Nothing when you press ALT)
```

**This means:**
- ❌ Update() loop not executing
- **Problem:** Tooltip MonoBehaviour might be disabled
- **Solution:** Check if component is on active GameObject

---

### **Scenario 3: Keyboard.current is Null**

**Console Output:**
```
[WeaponTooltip] Keyboard.current is null!
```

**This means:**
- ❌ New Input System not initialized
- **Problem:** No keyboard device available
- **Solution:** Check Input System settings

---

### **Scenario 4: ALT Detected But Tooltip Not Updating**

**Console Output:**
```
[WeaponTooltip] Input System ALT detected: True
[WeaponTooltip] ALT state changed: True (showRanges: True)
(Tooltip text changes in console but not on screen)
```

**This means:**
- ✅ Everything working in code
- ❌ Canvas not redrawing
- **Solution:** Need manual canvas refresh (already added)

---

## 🔧 **Debugging Checklist:**

### **Check 1: Is Update() Running?**
```
Hover weapon → Press ALT → Check console

✅ See "[WeaponTooltip] ALT state changed"
   → Update() is running!

❌ See nothing
   → Update() not running, component disabled?
```

### **Check 2: Is Keyboard Detected?**
```
✅ See "[WeaponTooltip] Input System ALT detected"
   → Keyboard working!

❌ See "Keyboard.current is null!"
   → Input System problem
```

### **Check 3: Is SetData() Being Called?**
```
✅ See multiple "[TooltipFormat]" messages
   → SetData() is refreshing!

❌ See "[ALT state changed]" but no "[TooltipFormat]"
   → SetData() not being called properly
```

### **Check 4: Are Text Values Changing?**
```
Look at the console logs for affix transformations:

✅ showRanges: True → Should see original description
   Example: '+8–14% increased Fire Damage' → '+8–14% increased Fire Damage'

✅ showRanges: False → Should see rolled value
   Example: '+8–14% increased Fire Damage' → '+13% increased Fire Damage'
```

---

## 🎯 **Expected Full Console Output:**

```
(Hover weapon)
[TooltipFormat] Formatting rolled affix 'Devastating' (isRolled: True, rolledValue: 63)
[TooltipFormat] Single-range: '+8–14% increased Fire' → '+13% increased Fire'

(Press ALT)
[WeaponTooltip] Input System ALT detected: True
[WeaponTooltip] ALT state changed: True (showRanges: True)
[TooltipFormat] Formatting rolled affix 'Devastating' (isRolled: True, rolledValue: 63)
(No transformation - showing original range)

(Release ALT)
[WeaponTooltip] Input System ALT detected: False
[WeaponTooltip] ALT state changed: False (showRanges: False)
[TooltipFormat] Formatting rolled affix 'Devastating' (isRolled: True, rolledValue: 63)
[TooltipFormat] Single-range: '+8–14% increased Fire' → '+13% increased Fire'
```

---

## 🔍 **Possible Issues:**

### **Issue 1: Canvas Doesn't Refresh**

**Symptom:** Console shows updates but UI doesn't change

**Added Solution:**
```csharp
Canvas.ForceUpdateCanvases();
LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
```

**If still not working:**
- Tooltip might be in a nested Canvas with pixel perfect enabled
- Try: `GetComponentInParent<Canvas>().enabled = false; enabled = true;`

---

### **Issue 2: Tooltip Recreated Each Frame**

**Symptom:** Tooltip flickers when ALT pressed

**Check:** Is `ItemTooltipManager` creating a new tooltip each time?  
**Solution:** Manager should reuse existing tooltip, not destroy/recreate

---

### **Issue 3: Different Tooltip Component**

**Symptom:** Debug logs never appear

**Check:** Is a different tooltip script being used?  
**Solution:** Search for other tooltip components in the scene

---

## 📝 **Next Steps:**

1. **Hover weapon in inventory**
2. **Press ALT**
3. **Check Console**
4. **Share output** to diagnose

**The debug logs will tell us exactly what's happening!** 🔍

---

**Current Status:**
- ✅ Update() loop added
- ✅ Canvas refresh added
- ✅ Debug logging added
- ✅ Input System support added
- ✅ No linter errors

**Ready to test and debug!** 🎮

