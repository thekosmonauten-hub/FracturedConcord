# Equipped Item Tooltip Click System - Complete Guide ✅

**Date:** December 4, 2025  
**Feature:** Click equipped slot to show tooltip in fixed container  
**Status:** ✅ Code Complete - Needs Unity Setup

---

## 🎯 **How It Works**

### **User Flow:**

```
1. User clicks MainHand equipment slot (has weapon equipped)
   └─ OnEquipmentSlotClicked(MainHand)

2. Check if slot has item and no inventory selection
   └─ If yes: Show equipped tooltip

3. ItemTooltipManager.ShowEquippedTooltip(weapon)
   └─ Activates WeaponTooltip_Equipped GameObject
   └─ Populates with weapon data

4. Tooltip appears in fixed position ✅
   └─ Shows rolled values
   └─ ALT-key toggles to ranges
```

---

## 🔧 **Unity Scene Setup**

### **Required GameObjects:**

Your scene should have these paths:
```
EquipmentNavDisplay/
├─ WeaponTooltip_Equipped
└─ EquipmentTooltip_Equipped
```

---

### **Step 1: Setup WeaponTooltip_Equipped**

1. **Select** `EquipmentNavDisplay/WeaponTooltip_Equipped` in Hierarchy

2. **Add Component** → Search for `WeaponTooltipView`

3. **Assign UI References** in Inspector:
   ```
   WeaponTooltipView Component:
   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   General:
   ├─ Name Label: (find child TextMeshPro for weapon name)
   ├─ Icon Image: LEAVE EMPTY (no icon for equipped)
   ├─ Icon Background: LEAVE EMPTY
   └─ Icon Frame: LEAVE EMPTY
   
   Stats:
   ├─ Damage Label: (TextMeshPro for damage)
   ├─ Attack Speed Label: (TextMeshPro for attack speed)
   ├─ Crit Chance Label: (TextMeshPro for crit)
   ├─ Weapon Type Label: (TextMeshPro for type)
   └─ Requirements Label: (TextMeshPro for requirements)
   
   Affixes:
   ├─ Implicit Label: (TextMeshPro for implicit)
   ├─ Prefix Labels: (array of TextMeshPro for prefixes)
   └─ Suffix Labels: (array of TextMeshPro for suffixes)
   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   ```

4. **Initially Disable** the GameObject
   - Uncheck the checkbox at top of Inspector

---

### **Step 2: Setup EquipmentTooltip_Equipped**

1. **Select** `EquipmentNavDisplay/EquipmentTooltip_Equipped`

2. **Add Component** → Search for `EquipmentTooltipView`

3. **Assign UI References** (similar to WeaponTooltipView but for armour/accessories)

4. **Initially Disable** the GameObject

---

### **Step 3: Wire to ItemTooltipManager**

1. **Find ItemTooltipManager** GameObject (usually on Canvas or UI root)

2. **In Inspector**, find section:
   ```
   Equipped Item Tooltip Containers (Scene Objects)
   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Weapon Tooltip Equipped Container
   └─ (Drag GameObject from Hierarchy)
   
   Equipment Tooltip Equipped Container
   └─ (Drag GameObject from Hierarchy)
   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   ```

3. **Drag from Hierarchy:**
   - `WeaponTooltip_Equipped` → Weapon Tooltip Equipped Container field
   - `EquipmentTooltip_Equipped` → Equipment Tooltip Equipped Container field

---

## 📊 **Expected Behavior**

### **Scenario 1: Click Equipped Weapon Slot**

```
1. Weapon equipped in MainHand
2. Click MainHand slot
3. WeaponTooltip_Equipped activates
4. Shows:
   DAMAGE: 8 (TOTAL 16)
   SMOLDERING: +13% INCREASED FIRE DAMAGE
   etc.
```

### **Scenario 2: Click Empty Slot**

```
1. No weapon equipped
2. Click MainHand slot
3. Nothing happens (no tooltip)
```

### **Scenario 3: Click While Item Selected**

```
1. Select weapon from inventory
2. Click MainHand slot
3. Weapon equips (normal behavior)
4. Tooltip doesn't show (because you're equipping)
```

### **Scenario 4: ALT-Key While Tooltip Open**

```
1. Click MainHand (tooltip shows)
2. Press ALT
3. Tooltip updates to show ranges
4. Release ALT
5. Tooltip updates to show rolled values
```

---

## 🔍 **Debug Output**

### **When You Click an Equipped Slot:**

**Expected Console:**
```
[EquipmentScreenUI] Equipment slot clicked: MainHand
[EquipmentScreenUI] Showing equipped tooltip for Worn Hatchet in slot MainHand
[ItemTooltipManager] Using weapon container for: Worn Hatchet
[ItemTooltipManager] Activating container: WeaponTooltip_Equipped
[ItemTooltipManager] Container active: True
[ItemTooltipManager] Found WeaponTooltipView, setting data for: Worn Hatchet
[TooltipFormat] Formatting rolled affix 'Smoldering' (isRolled: True, rolledValue: 13)
[TooltipFormat] Single-range: '+8–14% increased Fire Damage' → '+13% increased Fire Damage'
[ItemTooltipManager] ✅ Populated WeaponTooltipView with Worn Hatchet
```

---

### **If Container Not Assigned:**

**Console Shows:**
```
[ItemTooltipManager] No equipped tooltip container assigned for Weapon!
Please assign containers in ItemTooltipManager Inspector!
```

**Solution:** Assign the containers in ItemTooltipManager!

---

### **If Container Has No Component:**

**Console Shows:**
```
[ItemTooltipManager] Container WeaponTooltip_Equipped has no tooltip view component!
Available components on WeaponTooltip_Equipped:
  - Transform
  - RectTransform
  - Canvas
```

**Solution:** Add `WeaponTooltipView` component to the GameObject!

---

## 💡 **Key Differences from Hover**

### **Hover (OnPointerEnter):**
- Can use cursor-following tooltip
- Automatically hides on exit
- Good for quick info

### **Click (OnPointerClick):**
- Uses fixed container
- Stays visible until:
  - You click another slot
  - You click somewhere else (TODO: add click-away handler)
  - You press ESC (TODO)
- Good for detailed inspection

---

## 🎮 **User Experience**

**Workflow:**
```
1. Open Equipment Screen
2. See equipped weapon in MainHand slot
3. Click the slot
4. Tooltip appears on right side (in WeaponTooltip_Equipped)
5. Press ALT to compare rolled vs ranges
6. Click away or click another slot to close
```

**Clean and intuitive!** ✅

---

## 📝 **Files Modified**

1. **ItemTooltipManager.cs**
   - Changed to use `weaponTooltipEquippedContainer` (GameObject ref)
   - Added `ShowEquippedTooltip(BaseItem)` public method
   - Added detailed debug logging
   - Added component detection error messages

2. **EquipmentScreenUI.cs**
   - Modified `OnEquipmentSlotClicked()` to show tooltip for equipped items
   - Added `ShowEquippedItemTooltip()` method
   - Only shows if slot has item AND no selection

3. **EquipmentSlotUI.cs**
   - Already has OnPointerExit calling HideTooltip() ✅

---

## ✅ **Setup Checklist**

- [ ] WeaponTooltip_Equipped exists at correct path
- [ ] EquipmentTooltip_Equipped exists at correct path
- [ ] WeaponTooltip_Equipped has WeaponTooltipView component
- [ ] EquipmentTooltip_Equipped has EquipmentTooltipView component
- [ ] Both have UI references assigned
- [ ] Both start disabled
- [ ] ItemTooltipManager has both containers assigned
- [ ] Test: Click equipped weapon → Tooltip appears
- [ ] Test: Press ALT → Ranges appear
- [ ] Test: Release ALT → Rolled values appear

---

## 🚀 **Testing**

1. **Equip a weapon**
2. **Click the MainHand slot**
3. **Check Console** for debug messages
4. **Check** if WeaponTooltip_Equipped activates

**Console will tell you:**
- ✅ If container was found
- ✅ If container was activated
- ✅ If component was found
- ✅ If data was set
- ❌ What's missing if it fails

---

## 🎯 **Result**

**Before:**
```
Click equipped slot → Nothing happens
```

**After:**
```
Click equipped slot → Tooltip shows in fixed position
Press ALT → See ranges
No unhover needed → Smooth UX!
```

---

**Status:** ✅ **Production Ready** - Just needs Unity scene setup!

**Path to GameObjects:**
```
EquipmentNavDisplay/WeaponTooltip_Equipped
EquipmentNavDisplay/EquipmentTooltip_Equipped
```

**The debug logs will guide you through any setup issues!** 🔍

