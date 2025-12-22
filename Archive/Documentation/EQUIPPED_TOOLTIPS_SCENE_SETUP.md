# Equipped Item Tooltips - Scene Setup Guide 🎯

**Date:** December 4, 2025  
**Feature:** Show tooltips in pre-existing GameObjects when hovering equipped items  
**Status:** ✅ Code Complete - Needs Unity Scene Setup

---

## 🎯 **Architecture**

### **Two Types of Tooltips:**

1. **Inventory Tooltips** (Dynamic)
   - Instantiated when hovering inventory items
   - Follows cursor
   - Includes item icon
   - Destroyed when unhover

2. **Equipped Tooltips** (Static)
   - Pre-existing GameObjects in scene
   - Fixed position (not cursor-following)
   - **NO item icon** (icon already in slot)
   - Just activated/deactivated

---

## 🏗️ **Unity Scene Setup**

### **Step 1: Locate the Tooltip GameObjects**

In your **EquipmentScreen** scene hierarchy:

```
EquipmentNavDisplay/
├─ WeaponTooltip_Equipped      ← For weapons
└─ EquipmentTooltip_Equipped   ← For armour/accessories
```

**These should already exist!** ✅

---

### **Step 2: Add Components to Tooltip GameObjects**

#### **WeaponTooltip_Equipped:**

1. Select `EquipmentNavDisplay/WeaponTooltip_Equipped`
2. **Add Component** → `WeaponTooltipView`
3. **Assign UI references** (same as WeaponTooltips.prefab):
   - Name Label
   - Damage Label
   - Attack Speed Label
   - Critical Chance Label
   - Weapon Type Label
   - Requirements Label
   - Implicit Label
   - Prefix Labels (array)
   - Suffix Labels (array)

**Note:** NO icon references needed!

---

#### **EquipmentTooltip_Equipped:**

1. Select `EquipmentNavDisplay/EquipmentTooltip_Equipped`
2. **Add Component** → `EquipmentTooltipView`
3. **Assign UI references** (same as EquipmentTooltips.prefab):
   - Name Label
   - Base Stat Labels
   - Requirements Label
   - Implicit Label
   - Prefix Labels
   - Suffix Labels

**Note:** NO icon references needed!

---

### **Step 3: Wire Up ItemTooltipManager**

1. Find **ItemTooltipManager** GameObject in scene
2. In Inspector, find new section:

```
┌─────────────────────────────────────────┐
│ Equipped Item Tooltip Containers       │
│ (Scene Objects)                         │
├─────────────────────────────────────────┤
│ Weapon Tooltip Equipped Container      │
│ └─ Drag: EquipmentNavDisplay/          │
│           WeaponTooltip_Equipped        │
│                                         │
│ Equipment Tooltip Equipped Container   │
│ └─ Drag: EquipmentNavDisplay/          │
│           EquipmentTooltip_Equipped     │
└─────────────────────────────────────────┘
```

**Drag from Hierarchy:**
1. Drag `WeaponTooltip_Equipped` GameObject → Weapon Tooltip Equipped Container
2. Drag `EquipmentTooltip_Equipped` GameObject → Equipment Tooltip Equipped Container

---

### **Step 4: Initial State**

Make sure both tooltip GameObjects are **initially disabled**:

1. Select `WeaponTooltip_Equipped`
2. **Uncheck** the checkbox at top of Inspector (disable GameObject)
3. Select `EquipmentTooltip_Equipped`
4. **Uncheck** the checkbox (disable GameObject)

They'll be activated automatically when hovering!

---

## 📋 **How It Works**

### **Inventory Item Hover:**

```
User hovers inventory weapon
└─ ItemTooltipManager.ShowWeaponTooltip(weapon, pos, isEquipped: false)
   └─ Instantiates weaponTooltipPrefab
   └─ Positions at cursor
   └─ Shows with icon ✅
```

### **Equipped Item Hover:**

```
User hovers equipped weapon slot
└─ EquipmentScreenUI.ShowEquipmentTooltip(slotType, pos)
   └─ ItemTooltipManager.ShowWeaponTooltip(weapon, pos, isEquipped: true)
      └─ Activates weaponTooltipEquippedContainer
      └─ Populates with data
      └─ Shows at fixed position (no icon) ✅
```

---

## 🔧 **Code Implementation**

### **ItemTooltipManager.cs - New Method:**

```csharp
private void ShowTooltipInContainer(GameObject container, BaseItem item)
{
    // Hide dynamic tooltips
    if (activeTooltip != null)
    {
        Destroy(activeTooltip);
        activeTooltip = null;
    }
    
    // Hide all equipped containers
    HideEquippedTooltipContainers();
    
    // Activate target container
    container.SetActive(true);
    
    // Populate with data
    var weaponView = container.GetComponent<WeaponTooltipView>();
    if (weaponView != null && item is WeaponItem weapon)
    {
        weaponView.SetData(weapon);
    }
    
    var equipmentView = container.GetComponent<EquipmentTooltipView>();
    if (equipmentView != null)
    {
        equipmentView.SetData(item);
    }
}
```

---

## ✅ **Benefits**

1. **Performance**
   - No instantiation for equipped tooltips
   - Reuse same GameObjects
   - Faster!

2. **Fixed Position**
   - Tooltips stay in designated area
   - Don't follow cursor
   - Better for equipped items

3. **Clean Design**
   - No icon duplication
   - Icon already visible in slot

4. **Consistent Behavior**
   - Same data display as inventory
   - Same ALT-key toggle
   - Same rolled values

---

## 🎮 **Testing**

After Unity setup:

1. **Equip a weapon**
2. **Hover the MainHand equipment slot**
3. **Check:**
   - ✅ Tooltip appears in WeaponTooltip_Equipped GameObject
   - ✅ Shows weapon stats
   - ✅ Shows rolled affix values
   - ✅ NO icon displayed
4. **Press ALT while hovering**
   - ✅ Tooltip switches to ranges
5. **Unhover**
   - ✅ Tooltip disappears

---

## 🔍 **Troubleshooting**

### **Issue: Nothing appears when hovering**

**Check Console:**
```
[ItemTooltipManager] Showing equipped tooltip in container: WeaponTooltip_Equipped
```

If you see this, container is activating but might be invisible.

**Solutions:**
1. Check container has Canvas/CanvasGroup enabled
2. Check container position is on screen
3. Check container has UI elements (TextMeshPro, etc)

---

### **Issue: Wrong data shown**

**Check:**
1. Container has correct component (WeaponTooltipView or EquipmentTooltipView)
2. Component UI references are assigned
3. Console for "✅ Populated" messages

---

### **Issue: Tooltip doesn't hide**

**Check:**
1. EquipmentSlotUI.OnPointerExit() calls HideTooltip()
2. ItemTooltipManager.HideEquippedTooltipContainers() is called
3. Containers are being deactivated

---

## 📝 **Files Modified**

1. **ItemTooltipManager.cs**
   - Changed equipped fields from prefabs to containers
   - Added `ShowTooltipInContainer()` method
   - Added `HideEquippedTooltipContainers()` method
   - Updated `ShowWeaponTooltip()` and `ShowEquipmentTooltip()`

2. **EquipmentScreenUI.cs**
   - Already passes `isEquipped: true` ✅

3. **EquipmentSlotUI.cs**
   - Already calls HideTooltip() on exit ✅

---

## 🎯 **Result**

**Inventory:**
```
[Hover] → Tooltip spawns at cursor (with icon)
[Unhover] → Tooltip destroyed
```

**Equipped Slot:**
```
[Hover] → Container activates (no icon)
[Unhover] → Container deactivates
```

**Both support:**
- ✅ Rolled values
- ✅ ALT-key toggle
- ✅ Dynamic updates

---

## 📋 **Quick Setup Checklist**

- [ ] WeaponTooltip_Equipped has WeaponTooltipView component
- [ ] EquipmentTooltip_Equipped has EquipmentTooltipView component
- [ ] Both have UI references assigned
- [ ] Both are initially disabled
- [ ] ItemTooltipManager has both containers assigned
- [ ] Test: Hover equipped slot → Tooltip appears
- [ ] Test: Press ALT → Ranges appear
- [ ] Test: Unhover → Tooltip disappears

---

**Ready to set up in Unity!** 🎮

**Path to containers:**
```
EquipmentNavDisplay/WeaponTooltip_Equipped
EquipmentNavDisplay/EquipmentTooltip_Equipped
```

Just add the components and assign them to ItemTooltipManager! 🔧

