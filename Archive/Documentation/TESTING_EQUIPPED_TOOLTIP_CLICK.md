# Testing Equipped Tooltip Click - Debug Guide 🔍

**Date:** December 4, 2025  
**Feature:** Click equipped slot to show tooltip  
**Status:** Ready to test with comprehensive logging

---

## 🧪 **Test Steps:**

1. **Equip a weapon** in MainHand slot
2. **Click the MainHand equipment slot**
3. **Watch Console** for detailed flow

---

## 📊 **Expected Console Output (Full Flow):**

```
[EquipmentSlotUI] OnPointerClick called for MainHand
  Equipped Item: Worn Hatchet
  Subscribers to OnSlotClicked: 1

[EquipmentScreenUI] Equipment slot clicked: MainHand
[EquipmentScreenUI] Has selection: False
[EquipmentScreenUI] Slot MainHand equipped item: Worn Hatchet
[EquipmentScreenUI] ✅ Showing equipped tooltip for Worn Hatchet
[EquipmentScreenUI] Showing equipped tooltip for Worn Hatchet in slot MainHand

[ItemTooltipManager] Using weapon container for: Worn Hatchet
[ItemTooltipManager] Activating container: WeaponTooltip_Equipped
[ItemTooltipManager] Container active: True
[ItemTooltipManager] Found WeaponTooltipView, setting data for: Worn Hatchet

[TooltipFormat] Formatting rolled affix ...
[ItemTooltipManager] ✅ Populated WeaponTooltipView with Worn Hatchet
```

**If you see all this, it's working!** ✅

---

## 🔍 **Troubleshooting by Console Output**

### **Issue 1: No OnPointerClick Message**

**Console:**
```
(Nothing when you click)
```

**Problem:** Click not detected

**Solutions:**
1. ✅ Button component added → Button.onClick wired in Awake()
2. Check Button is not disabled
3. Check EventSystem exists in scene
4. Check slot has Graphic raycastTarget enabled

---

### **Issue 2: OnPointerClick Fires But No OnSlotClicked**

**Console:**
```
[EquipmentSlotUI] OnPointerClick called for MainHand
  Subscribers to OnSlotClicked: 0
```

**Problem:** No subscribers to event

**Solution:** Check EquipmentScreenUI wired up in Initialize():
```csharp
kvp.Value.OnSlotClicked += OnEquipmentSlotClicked;
```

---

### **Issue 3: Container Not Assigned**

**Console:**
```
[ItemTooltipManager] No equipped tooltip container assigned for Weapon!
Please assign containers in ItemTooltipManager Inspector!
```

**Problem:** Container reference not set

**Solution:**
1. Find ItemTooltipManager in scene
2. Drag `WeaponTooltip_Equipped` GameObject → Weapon Tooltip Equipped Container field

---

### **Issue 4: Container Found But No Component**

**Console:**
```
[ItemTooltipManager] Container WeaponTooltip_Equipped has no tooltip view component!
Available components on WeaponTooltip_Equipped:
  - Transform
  - RectTransform
  - CanvasRenderer
```

**Problem:** Missing WeaponTooltipView component

**Solution:**
1. Select `WeaponTooltip_Equipped` in Hierarchy
2. Add Component → `WeaponTooltipView`
3. Assign UI references

---

### **Issue 5: Container Activates But Nothing Visible**

**Console:**
```
[ItemTooltipManager] Container active: True
[ItemTooltipManager] ✅ Populated WeaponTooltipView with Worn Hatchet
(But nothing visible on screen)
```

**Problem:** Container is off-screen or has no UI elements

**Solutions:**
1. Check container position in Scene view
2. Check container has TextMeshPro child objects
3. Check Canvas is enabled
4. Check CanvasGroup alpha is 1

---

## ✅ **Quick Diagnostic Checklist**

Run through these in order:

### **1. Click Detection:**
```
Click slot → See "[EquipmentSlotUI] OnPointerClick"?
✅ Yes → Click detected, continue
❌ No → Button problem, check EventSystem
```

### **2. Event Propagation:**
```
See "[EquipmentScreenUI] Equipment slot clicked"?
✅ Yes → Event fired, continue
❌ No → Not subscribed, check Initialize()
```

### **3. Item Detection:**
```
See "Slot MainHand equipped item: Worn Hatchet"?
✅ Yes → Item found, continue
❌ No → Item not equipped or slot issue
```

### **4. Container Assignment:**
```
See "Using weapon container for: Worn Hatchet"?
✅ Yes → Container assigned, continue
❌ No → Assign container in Inspector
```

### **5. Component Detection:**
```
See "Found WeaponTooltipView, setting data"?
✅ Yes → Component exists, continue
❌ No → Add WeaponTooltipView component
```

### **6. Data Population:**
```
See "✅ Populated WeaponTooltipView"?
✅ Yes → Everything worked!
❌ No → Check SetData() errors
```

---

## 🎮 **Unity Setup Verification**

### **Check 1: GameObjects Exist**

In Hierarchy, verify:
```
EquipmentNavDisplay/
├─ WeaponTooltip_Equipped ✅
└─ EquipmentTooltip_Equipped ✅
```

### **Check 2: Components Added**

Select `WeaponTooltip_Equipped`:
```
Inspector shows:
├─ Transform ✅
├─ RectTransform ✅
├─ WeaponTooltipView ✅ ← MUST HAVE THIS
└─ (other UI components)
```

### **Check 3: ItemTooltipManager References**

Select ItemTooltipManager:
```
Equipped Item Tooltip Containers:
├─ Weapon Tooltip Equipped Container: WeaponTooltip_Equipped ✅
└─ Equipment Tooltip Equipped Container: EquipmentTooltip_Equipped ✅
```

### **Check 4: Button Component**

Select equipment slot prefab (e.g., MainHand slot):
```
Components:
├─ EquipmentSlotUI ✅
├─ Button ✅ ← You added this
└─ Image (for raycast target) ✅
```

---

## 🎯 **What the Debug Logs Tell You**

The console output will **show you exactly where the flow stops**:

```
✅ Stop at step 1 → Button/click issue
✅ Stop at step 2 → Event subscription issue
✅ Stop at step 3 → Item not equipped
✅ Stop at step 4 → Container not assigned
✅ Stop at step 5 → Component missing
✅ All steps → It's working!
```

---

## 📝 **Implementation Summary**

### **Code Changes:**

1. **EquipmentSlotUI.cs**
   - Added `Button button` field
   - Auto-wires Button.onClick in Awake()
   - Works with BOTH Button and IPointerClickHandler

2. **EquipmentScreenUI.cs**
   - Checks if slot has item before showing tooltip
   - Only shows tooltip if no inventory selection
   - Calls ShowEquippedItemTooltip()

3. **ItemTooltipManager.cs**
   - Added `ShowEquippedTooltip()` public method
   - Selects correct container (weapon vs equipment)
   - Activates container and populates data

---

## 🎮 **Test Now:**

1. **Click MainHand slot** (with weapon equipped)
2. **Read Console from top to bottom**
3. **Find where it stops**
4. **Follow troubleshooting for that step**

**The debug logs are your guide!** 🔍

---

## ✅ **Success Indicators:**

When it works, you'll see:
- ✅ All 6 steps in console
- ✅ WeaponTooltip_Equipped activates in Hierarchy
- ✅ Tooltip shows weapon data
- ✅ Can press ALT to toggle ranges
- ✅ Rolled values display correctly

---

**Click that slot and share the console output!** The logs will tell us exactly what needs to be fixed. 🎯

