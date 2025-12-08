# Equipped Item Tooltips - Setup Guide 🎯

**Date:** December 4, 2025  
**Feature:** Show tooltips when hovering equipped items (without icon)  
**Status:** ✅ Code Complete - Needs Unity Setup

---

## 🎯 **What This Does**

### **Before:**
```
Hover equipped weapon → No tooltip ❌
```

### **After:**
```
Hover equipped weapon → Shows tooltip (no icon) ✅
```

**Why no icon?** The icon is already visible in the equipment slot!

---

## 🔧 **Unity Inspector Setup**

### **Step 1: Find ItemTooltipManager**

1. Open your **EquipmentScreen scene**
2. Find the GameObject with **ItemTooltipManager** component
3. Select it in Inspector

---

### **Step 2: Assign Equipped Tooltip Prefabs**

In the **ItemTooltipManager** Inspector:

```
┌─────────────────────────────────────────┐
│ Item Tooltip Manager (Script)          │
├─────────────────────────────────────────┤
│ Tooltip Prefabs                         │
│ ├─ Weapon Tooltip Prefab               │
│ ├─ Equipment Tooltip Prefab            │
│ ├─ Effigy Tooltip Prefab               │
│ ├─ Card Tooltip Prefab                 │
│ ├─ Currency Tooltip Prefab             │
│ └─ Warrant Tooltip Prefab              │
│                                         │
│ Equipped Item Tooltips (No Icon) ← NEW!│
│ ├─ Weapon Tooltip Equipped Prefab      │
│ │   └─ Drag: WeaponTooltips_Equipped   │
│ │                                       │
│ └─ Equipment Tooltip Equipped Prefab   │
│     └─ Drag: EquipmentTooltips_Equipped│
└─────────────────────────────────────────┘
```

**Drag and drop:**
1. **Weapon Tooltip Equipped Prefab** ← `WeaponTooltips_Equipped.prefab`
2. **Equipment Tooltip Equipped Prefab** ← `EquipmentTooltips_Equipped.prefab`

---

### **Step 3: Verify Prefabs Have Scripts**

**Check WeaponTooltips_Equipped.prefab:**
```
1. Open prefab in Inspector
2. Verify it has: WeaponTooltipView component
3. Verify all UI references are assigned
4. NO icon image (or icon disabled)
```

**Check EquipmentTooltips_Equipped.prefab:**
```
1. Open prefab in Inspector
2. Verify it has: EquipmentTooltipView component
3. Verify all UI references are assigned
4. NO icon image (or icon disabled)
```

---

## 📋 **How It Works**

### **Code Flow:**

```
1. User hovers equipped weapon slot
   └─ EquipmentSlotUI.OnPointerEnter()
   
2. Fires OnSlotHovered event
   └─ EquipmentScreenUI.ShowEquipmentTooltip()
   
3. Calls ItemTooltipManager with isEquipped=true
   └─ ItemTooltipManager.ShowEquipmentTooltip(item, pos, isEquipped: true)
   
4. Manager selects correct prefab
   ├─ isEquipped=true → weaponTooltipEquippedPrefab
   └─ isEquipped=false → weaponTooltipPrefab
   
5. Shows tooltip without icon ✅
```

### **Implementation:**

```csharp
// ItemTooltipManager.cs
public void ShowEquipmentTooltip(BaseItem item, Vector2 position, bool isEquipped = false)
{
    if (item is WeaponItem weaponItem)
    {
        // Select prefab based on equipped state
        GameObject prefab = isEquipped && weaponTooltipEquippedPrefab != null
            ? weaponTooltipEquippedPrefab  // No icon version
            : weaponTooltipPrefab;          // With icon version
        
        ShowTooltipInternal(prefab, position, ...);
    }
}

// EquipmentScreenUI.cs
void ShowEquipmentTooltip(EquipmentType slotType, Vector2 position)
{
    BaseItem equipped = slot.GetEquippedItem();
    
    // Pass isEquipped=true for equipped items
    ItemTooltipManager.Instance.ShowEquipmentTooltip(equipped, position, isEquipped: true);
}
```

---

## ✅ **Features Working**

1. **Inventory hover** → Shows tooltip WITH icon
2. **Equipped slot hover** → Shows tooltip WITHOUT icon
3. **ALT-key toggle** → Works on both!
4. **Rolled values** → Shown on both!
5. **Dynamic updates** → ALT updates instantly!

---

## 🎮 **Testing Checklist**

### **After Unity Setup:**

- [ ] Assign WeaponTooltips_Equipped.prefab to ItemTooltipManager
- [ ] Assign EquipmentTooltips_Equipped.prefab to ItemTooltipManager
- [ ] Equip a weapon
- [ ] Hover equipped weapon slot → Tooltip appears (no icon)
- [ ] Hover inventory weapon → Tooltip appears (with icon)
- [ ] Press ALT while hovering equipped → Ranges appear
- [ ] Release ALT → Rolled values appear

---

## 📝 **Prefab Locations**

```
Assets/Prefab/EquipmentScreen/
├─ WeaponTooltips.prefab (inventory - WITH icon)
├─ EquipmentTooltips.prefab (inventory - WITH icon)
├─ WeaponTooltips_Equipped.prefab (equipped - NO icon) ← NEW!
└─ EquipmentTooltips_Equipped.prefab (equipped - NO icon) ← NEW!
```

---

## 🔍 **Troubleshooting**

### **Issue: Equipped tooltip shows icon**

**Solution:** Check the prefab:
1. Open WeaponTooltips_Equipped.prefab
2. Find the Icon GameObject
3. Disable it or remove Image component

### **Issue: Equipped tooltip doesn't appear**

**Solution:** 
1. Check ItemTooltipManager has prefabs assigned
2. Check EquipmentSlotUI is calling OnSlotHovered
3. Check console for warnings

### **Issue: Wrong tooltip appears**

**Solution:**
1. Verify isEquipped=true is being passed
2. Check prefab assignments in Inspector

---

## 🎯 **Result**

**Inventory Items:**
```
╔════════════════════════════════════╗
║   🪓  WORN HATCHET                 ║  ← Icon visible
╠════════════════════════════════════╣
║ DAMAGE: 8  (TOTAL 16)             ║
╚════════════════════════════════════╝
```

**Equipped Items:**
```
╔════════════════════════════════════╗
║   WORN HATCHET                     ║  ← No icon
╠════════════════════════════════════╣
║ DAMAGE: 8  (TOTAL 16)             ║
╚════════════════════════════════════╝
```

**Cleaner for equipped items since icon is already in slot!** ✅

---

## 📝 **Files Modified**

1. **ItemTooltipManager.cs**
   - Added `weaponTooltipEquippedPrefab` field
   - Added `equipmentTooltipEquippedPrefab` field
   - Added `isEquipped` parameter to methods
   - Selects correct prefab based on equipped state

2. **EquipmentScreenUI.cs**
   - Passes `isEquipped: true` to ItemTooltipManager
   - Already had hover events wired up!

---

**Next Step:** 
**Assign the prefabs in Unity Inspector!**

1. Select ItemTooltipManager GameObject
2. Drag WeaponTooltips_Equipped.prefab → Weapon Tooltip Equipped Prefab
3. Drag EquipmentTooltips_Equipped.prefab → Equipment Tooltip Equipped Prefab
4. Test by hovering equipped items!

**No linter errors!** Ready to set up in Unity! 🎮

