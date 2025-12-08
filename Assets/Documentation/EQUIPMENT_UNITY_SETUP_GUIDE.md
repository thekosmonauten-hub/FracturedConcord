# Equipment & Inventory System - Unity Editor Setup Guide

**Time Required:** 15-20 minutes  
**Difficulty:** Easy - Just follow the steps!

---

## 📋 Overview

This guide will help you configure the equipment and inventory system in Unity Editor so that:
- ✅ Items appear in inventory when picked up
- ✅ You can click items to select them
- ✅ You can drag items to equip them
- ✅ Tooltips show on hover
- ✅ Visual feedback works properly

---

## 🎯 Quick Checklist

Before starting, make sure you have:
- [ ] Equipment Scene exists
- [ ] Inventory grid visible in scene
- [ ] Equipment slots visible in scene
- [ ] Canvas with GraphicRaycaster component

---

## Part 1: Add Required Managers (5 minutes)

### **Step 1: Add ItemSelectionManager**

1. **In Hierarchy**, right-click in Equipment Scene
2. **Create Empty GameObject**
3. **Rename it:** `ItemSelectionManager`
4. **Click "Add Component"**
5. **Search for:** `ItemSelectionManager`
6. **Add it**

**Verification:** Component appears in Inspector with no errors

---

### **Step 2: Add DragVisualHelper**

1. **In Hierarchy**, right-click in Equipment Scene
2. **Create Empty GameObject**
3. **Rename it:** `DragVisualHelper`
4. **Click "Add Component"**
5. **Search for:** `DragVisualHelper`
6. **Add it**

**Optional Configuration:**
- **Drag Canvas:** Will auto-find, but you can assign manually for better performance
- **Drag Visual Prefab:** Leave empty for default (creates Image on-the-fly)
- **Drag Alpha:** 0.7 (default is good - 70% transparent)

**Verification:** Component appears, no red errors

---

## Part 2: Configure Inventory Grid (5 minutes)

### **Step 3: Find InventoryGridUI Component**

1. **In Hierarchy**, find your inventory grid GameObject
   - Usually named: `InventoryGrid`, `PlayerInventory`, or similar
2. **Select it**
3. **In Inspector**, find `InventoryGridUI` component

---

### **Step 4: Configure InventoryGridUI Settings**

**Grid Settings:**
- **Grid Width:** 10 (recommended)
- **Grid Height:** 6 (60 slots total)
- **Cell Size:** (60, 60) pixels
- **Spacing:** (2, 2) pixels

**References:**
- **Slot Prefab:** Drag your inventory slot prefab here
  - Should have `InventorySlotUI` component
  - Should have `Image` components for background and icon
- **Grid Container:** The Transform where slots spawn
  - Usually a child GameObject with GridLayoutGroup

**Character Inventory Binding:**
- **Bind To Character Inventory:** ✅ **Check this!**
- **Refresh On Enable:** ✅ **Check this!**

**Verification:** 
- Click "Play"
- Inventory slots should generate automatically
- Check Console for: `[InventoryGridUI] Generated X slots`

---

### **Step 5: Verify Slot Prefab Setup**

**Your inventory slot prefab needs:**

1. **InventorySlotUI Component:**
   - Should already be on prefab
   - No configuration needed

2. **Image Components:**
   - **Background Image** (for slot background)
   - **Item Icon Image** (for item sprite)
   - **Optional:** TextMeshProUGUI for item name

3. **Colors (in InventorySlotUI):**
   - **Normal Color:** RGB(30, 36, 40) - Dark grey
   - **Hover Color:** RGB(51, 56, 61) - Light grey
   - **Occupied Color:** RGB(77, 102, 128) - Blue-grey
   - **Selected Color:** RGB(230, 179, 51) - Gold

4. **References:**
   - **Background:** Assign background Image
   - **Item Icon:** Assign icon Image
   - **Item Label:** Assign TextMeshProUGUI (optional)

**Verification:** Select prefab in Project view, check Inspector

---

## Part 3: Configure Equipment Slots (5 minutes)

### **Step 6: Find Equipment Slots**

Your scene should have these equipment slot GameObjects:
- Helmet Slot
- Body Armour Slot
- Gloves Slot
- Boots Slot
- Amulet Slot
- Belt Slot
- Left Ring Slot
- Right Ring Slot
- Main Hand Slot (Weapon)
- Off Hand Slot (Shield/Dual Wield)

---

### **Step 7: Configure Each Equipment Slot**

**For each slot GameObject:**

1. **Select the slot** in Hierarchy
2. **Add component** if missing: `EquipmentSlotUI`
3. **Configure References:**
   - **Background Image:** Assign Image component
   - **Item Icon Image:** Assign Image for item sprite
   - **Slot Label:** Assign TextMeshProUGUI (e.g., "HELMET")
   - **Item Name Label:** Assign TextMeshProUGUI for equipped item name

4. **Set Slot Type:**
   - **Slot Type:** Select from dropdown:
     - Helmet (1)
     - BodyArmour (2)
     - Gloves (3)
     - Boots (4)
     - Amulet (5)
     - Belt (6)
     - LeftRing (7)
     - RightRing (8)
     - MainHand (9)
     - OffHand (10)

5. **Set Colors:**
   - **Empty Color:** RGB(26, 26, 26, 204)
   - **Occupied Color:** RGB(51, 77, 102, 230)
   - **Hover Color:** RGB(77, 102, 128, 255)

**Verification:** All 10 equipment slots configured with unique slot types

---

### **Step 8: Link Equipment Slots to EquipmentScreenUI**

1. **Find your main Equipment Screen manager** GameObject
   - Usually named: `EquipmentScreenUI`, `EquipmentManager`, or similar
2. **Select it**
3. **In Inspector**, find `EquipmentScreenUI` component
4. **Assign all equipment slot references:**
   - Helmet Slot → helmetSlot field
   - Amulet Slot → amuletSlot field
   - Main Hand Slot → mainHandSlot field
   - Body Armour Slot → bodyArmourSlot field
   - Off Hand Slot → offHandSlot field
   - Gloves Slot → glovesSlot field
   - Left Ring Slot → leftRingSlot field
   - Right Ring Slot → rightRingSlot field
   - Belt Slot → beltSlot field
   - Boots Slot → bootsSlot field

5. **Assign Inventory Grid:**
   - **Inventory Grid:** Drag your InventoryGridUI GameObject here

**Verification:** All fields filled, no "None (GameObject)" entries

---

## Part 4: Canvas & Input Configuration (2 minutes)

### **Step 9: Verify Canvas Settings**

1. **Select your Canvas** in Hierarchy
2. **Check these components exist:**
   - Canvas (Render Mode: Screen Space - Overlay)
   - Canvas Scaler
   - **GraphicRaycaster** ⚠️ **REQUIRED FOR CLICKS!**

**If GraphicRaycaster is missing:**
- Click "Add Component"
- Search "Graphic Raycaster"
- Add it

3. **Verify EventSystem exists:**
   - Look for "EventSystem" GameObject in Hierarchy
   - If missing, Create → UI → Event System

**Verification:** 
- Canvas has GraphicRaycaster
- EventSystem exists in scene

---

### **Step 10: Configure Image Components for Raycasting**

**For each inventory slot and equipment slot:**

1. **Select the slot GameObject**
2. **Find the Background Image component**
3. **Ensure "Raycast Target" is checked:**
   - ✅ **Raycast Target** (must be enabled for clicks!)

**Do this for:**
- All inventory slot backgrounds
- All equipment slot backgrounds

**Tip:** If using a prefab, you only need to do this once on the prefab!

---

## Part 5: Test the System (3 minutes)

### **Step 11: Basic Functionality Test**

1. **Press Play**
2. **Pick up an item** (use existing drop system)
3. **Check inventory:**
   - Item appears? ✅
   - Click item? → Should highlight gold ✅
   - Drag item? → Ghost image appears? ✅

4. **Test equipping:**
   - **Method 1 (Click):** Click item → Click equipment slot
   - **Method 2 (Drag):** Drag item → Drop on equipment slot

5. **Check character stats:**
   - Stats updated? ✅
   - Item shows in equipment slot? ✅

6. **Test unequipping:**
   - Click equipped item → Returns to inventory? ✅

---

### **Step 12: Troubleshooting**

**Problem: Items don't respond to clicks**

**Solutions:**
- Check GraphicRaycaster on Canvas ✅
- Check EventSystem exists ✅
- Check Image "Raycast Target" enabled ✅
- Check InventoryGridUI has slotPrefab assigned ✅

---

**Problem: Can't drag items**

**Solutions:**
- Check DragVisualHelper exists in scene ✅
- Check InventorySlotUI has drag event subscriptions ✅
- Check itemIcon Image component exists ✅

---

**Problem: Items equip but stats don't update**

**Solutions:**
- Check EquipmentManager exists ✅
- Check EquipmentManager.CalculateTotalEquipmentStats() is called ✅
- Check Character reference is set ✅

---

**Problem: Ghost image doesn't appear**

**Solutions:**
- Check DragVisualHelper has Canvas reference ✅
- Check dragAlpha isn't 0 (use 0.7) ✅
- Check sprite is assigned to item ✅

---

## Part 6: Boss Ability Setup (Bonus - 10 minutes)

Since you're in Unity, here's how to configure the bosses we created:

### **Step 13: Configure Boss Abilities**

**For each boss** (e.g., `BOSS_WeeperOfBark`):

1. **Navigate to:** `Assets/Resources/Enemies/Act1/{Folder}/`
2. **Select** the BOSS asset
3. **In Inspector:**

**A) Link Scriptable Abilities:**
- Expand "Abilities (Scriptable)"
- Set **Size** to number of abilities
- Drag ability assets into each element

**B) Set Boss Ability Enums:**
- Expand "Boss Abilities (Complex)"
- Set **Size** to number of complex abilities
- Select enum values from dropdown:
  - 0 = SunderingEcho
  - 1 = JudgmentLoop
  - 2 = AddCurseCards
  - 3 = ReactiveSeep
  - 4 = RetaliationOnSkill
  - 5 = AvoidFirstAttack
  - 6 = ConditionalHeal
  - 7 = ConditionalStealth
  - 8 = NegateStrongestBuff
  - 9 = BarrierOfDissent
  - 10 = BloomOfRuin
  - 11 = LearnPlayerCard
  - 12 = BuffCancellation
  - 13 = AddTemporaryCurse

**C) Set Summon Pool (if applicable):**
- Expand "Summon Pool"
- Add minion enemies for bosses that summon

---

### **Quick Reference: Boss Configurations**

**Weeper-of-Bark:**
- Abilities: WeeperOfBark_SplinterCry
- Boss Abilities: RetaliationOnSkill (4)

**Shadow Shepherd:**
- Abilities: ShadowShepherd_MournfulToll
- Boss Abilities: ConditionalStealth (7)

**Charred Homesteader:**
- Abilities: CharredHomesteader_Coalburst
- Boss Abilities: ConditionalHeal (6)

**And so on for all 15 bosses...**

---

## 🎮 Final Testing Checklist

### Equipment System:
- [ ] Open Equipment Scene
- [ ] Pick up an item (existing drop system)
- [ ] Item appears in inventory
- [ ] Click item → Highlights gold
- [ ] Drag item → Ghost appears
- [ ] Drop on equipment slot → Equips
- [ ] Stats update properly
- [ ] Click equipped item → Unequips

### Boss System:
- [ ] Start combat with any configured boss
- [ ] Boss abilities registered (check console)
- [ ] Play Skill card → Retaliation works (if Weeper-of-Bark)
- [ ] Status effects apply correctly
- [ ] No runtime errors

---

## 🆘 Need Help?

If something doesn't work:

1. **Check Console** for error messages
2. **Verify all references assigned** (no "None" fields)
3. **Check EventSystem exists**
4. **Verify GraphicRaycaster on Canvas**
5. **Make sure components are enabled**

---

## 📚 Additional Documentation

- `EQUIPMENT_INTERACTION_SYSTEM.md` - System architecture
- `EQUIPMENT_DRAG_DROP_COMPLETE.md` - Drag & drop details
- `ACT1_BOSSES_COMPLETE.md` - Boss system overview
- `BOSS_ABILITIES_IMPLEMENTATION_GUIDE.md` - Complete boss guide

---

## ✅ Success Criteria

You'll know it's working when:
- ✅ Items appear in inventory after pickup
- ✅ Clicking items highlights them gold
- ✅ Dragging shows ghost image
- ✅ Dropping on equipment slots equips items
- ✅ Stats update immediately
- ✅ Tooltips show on hover
- ✅ No console errors

---

## 🎉 That's It!

Once these steps are complete:
- Equipment system fully functional
- Boss system ready to test
- Professional-grade interactions
- Ready for players to use!

**Total Setup Time:** ~15-20 minutes for everything!

---

## Pro Tips 💡

**Speed Up Testing:**
- Use Unity's Play Mode to test quickly
- Keep Console visible to see debug messages
- Use Debug.Log to trace interactions
- Test one feature at a time

**Organization:**
- Group managers under "--- Managers ---" in Hierarchy
- Name slots clearly (e.g., "EquipmentSlot_Helmet")
- Use prefabs for inventory slots (easier to update)

**Performance:**
- Managers are singletons (only need one)
- Drag visual creates/destroys on demand
- No performance impact when idle

---

**Setup Guide Complete!** Follow these steps whenever you're ready and the system will be fully operational! 🚀

