# Panel Navigation Setup Guide
## Step-by-Step Instructions for PanelNavigationController

**Script:** `PanelNavigationController.cs`  
**Purpose:** Switch between Equipment, Effigy, Character, etc. views with one navigation bar (Inventory/Stash stay visible)

---

## 📋 Your Scenario

You have:
- **Navigation Buttons** → Equipment, Effigy, Character, etc.
- **Display Window Panels** → EquipmentNavDisplay, EffigyNavDisplay, CharacterNavDisplay (overlapping)
- **Dynamic Area Panels** → DynamicArea/Equipment, DynamicArea/Effigy, DynamicArea/Character (children)
- **Always Visible** → Inventory and Stash (NOT controlled by navigation)

You want: Clicking "Equipment" button shows both Equipment panels, clicking "Effigy" shows both Effigy panels, etc., while Inventory/Stash remain visible.

---

## 🎯 Step 1: Hierarchy Setup

Your hierarchy should look like this:

```
EquipmentScreenCanvas
├── NavigationBar (with PanelNavigationController.cs attached)
│   ├── EquipmentButton
│   ├── EffigyButton
│   ├── CharacterButton
│   └── ... other navigation buttons
├── DisplayWindow
│   ├── EquipmentNavDisplay (positioned at same location)
│   ├── EffigyNavDisplay (positioned at same location)
│   ├── CharacterNavDisplay (positioned at same location)
│   └── ... other display panels (NOT Inventory/Stash)
├── DynamicArea
│   ├── Equipment
│   ├── Effigy
│   ├── Character
│   └── ... other dynamic panels (NOT Inventory/Stash)
├── InventoryPanel (ALWAYS VISIBLE - not controlled by navigation)
└── StashPanel (ALWAYS VISIBLE - not controlled by navigation)
```

**Important:** 
- All Display Window panels should be positioned at the same location (overlapping). Only one will be active at a time.
- **Inventory and Stash are always visible** and should NOT be added to Navigation Items.

---

## 🛠️ Step 2: Attach Script

1. Select your **NavigationBar** GameObject (or create a new empty GameObject for navigation)
2. Click **Add Component** → `PanelNavigationController`
3. You should now see the component in the Inspector

---

## ⚙️ Step 3: Configure in Inspector

### Navigation Items List

Click **"+"** button to add entries. For each navigation button you have:

#### Example: Equipment Navigation Item
```
Element 0 (Equipment):
  Navigation Button: [Drag EquipmentButton here]
  Display Panel: [Drag EquipmentNavDisplay here]
  Dynamic Panel: [Drag DynamicArea/Equipment here]
  
  Visual States (Optional):
    Active Button Sprite: [Your selected equipment button sprite]
    Inactive Button Sprite: [Your unselected equipment button sprite]
    Active Button Color: White (255, 255, 255)
    Inactive Button Color: Gray (180, 180, 180)
```

#### Example: Effigy Navigation Item
```
Element 1 (Effigy):
  Navigation Button: [Drag EffigyButton here]
  Display Panel: [Drag EffigyNavDisplay here]
  Dynamic Panel: [Drag DynamicArea/Effigy here]
  
  Visual States (Optional):
    Active Button Sprite: [Your selected effigy button sprite]
    Inactive Button Sprite: [Your unselected effigy button sprite]
    Active Button Color: White (255, 255, 255)
    Inactive Button Color: Gray (180, 180, 180)
```

#### Example: Character Navigation Item
```
Element 2 (Character):
  Navigation Button: [Drag CharacterButton here]
  Display Panel: [Drag CharacterNavDisplay here]
  Dynamic Panel: [Drag DynamicArea/Character here]
  
  Visual States (Optional):
    Active Button Sprite: [Your selected character button sprite]
    Inactive Button Sprite: [Your unselected character button sprite]
    Active Button Color: White (255, 255, 255)
    Inactive Button Color: Gray (180, 180, 180)
```

**Repeat for all your navigation modes!**

**Note:** Do NOT add Inventory or Stash to Navigation Items - they're always visible!

---

## 🎨 Step 4: Settings Configuration

Below the Navigation Items List:

### Settings Section
- **Starting Index:** `0` (which panel to show on start)
  - `0` = First item (Equipment)
  - `1` = Second item (Effigy)
  - etc.

- **Use Sprite Swapping:** ✓ Check if you want button sprites to change
  - ✓ = Uses Active/Inactive Button Sprites
  - ☐ = Uses color-only mode

### Optional Transitions
- **Enable Fade Transitions:** ☐ Check for smooth fading
  - ✓ = Panels fade in/out smoothly
  - ☐ = Instant switching (default)
  
- **Fade Duration:** `0.2` (seconds for fade effect)

---

## 🎮 Step 5: Test!

1. **Play the game**
2. Click different navigation buttons
3. Watch as both Display Window and Dynamic Area panels switch together!

**Expected behavior:**
- Clicking "Equipment" → Shows EquipmentNavDisplay + DynamicArea/Equipment
- Clicking "Effigy" → Shows EffigyNavDisplay + DynamicArea/Effigy
- Clicking "Character" → Shows CharacterNavDisplay + DynamicArea/Character
- Only one set of panels visible at a time
- Button visuals update (sprite/color changes)
- **Inventory and Stash remain visible the entire time** ✓

---

## 🔧 Advanced: Visual States

### Option 1: Sprite Swapping (Recommended for your tabs)
1. Set **Use Sprite Swapping** to ✓
2. For each Navigation Item:
   - **Active Button Sprite** → Your "selected" sprite
   - **Inactive Button Sprite** → Your "unselected" sprite
3. The button Image component will swap sprites automatically

### Option 2: Color-Only
1. Set **Use Sprite Swapping** to ☐
2. Only Active/Inactive Button Colors will be used
3. Unity's ColorBlock system handles the tinting

### Option 3: Both (Sprite + Color)
1. Set **Use Sprite Swapping** to ✓
2. Assign sprites AND set colors
3. Sprites swap AND get color tinted

---

## 📝 Common Setup Mistakes

### ❌ Inventory/Stash disappear when switching
**Problem:** Accidentally added Inventory or Stash to Navigation Items  
**Solution:** 
- Remove Inventory/Stash from Navigation Items list
- They should be separate GameObjects, always active
- Only add panels that should switch (Equipment, Effigy, Character, etc.)

### ❌ Panels don't switch
**Problem:** Buttons don't have click listeners  
**Solution:** Make sure PanelNavigationController is attached and Navigation Items are configured. The script auto-adds listeners in Start().

### ❌ Both panels show at once
**Problem:** Panels aren't properly overlapped or disabled  
**Solution:** 
- Manually disable all Display Panels except the starting one
- Make sure all panels are in the same position (overlapping)

### ❌ Sprites don't change
**Problem:** Use Sprite Swapping is unchecked OR sprites not assigned  
**Solution:**
- Check **Use Sprite Swapping** ✓
- Assign both Active and Inactive Button Sprites for each item
- Make sure button has an Image component

### ❌ Wrong panel shows on start
**Problem:** Starting Index is incorrect  
**Solution:** Set **Starting Index** to match the panel you want:
- `0` = First Navigation Item
- `1` = Second Navigation Item
- etc.

---

## 💡 Pro Tips

### Tip 1: Keep Display Panels Aligned
All Display Window panels should have:
- **Same anchors** (e.g., stretch or centered)
- **Same position/size**
- Only differs in content

This ensures smooth visual consistency when switching.

### Tip 2: Organize Dynamic Area
Create a parent GameObject "DynamicArea" with:
- Child for each navigation mode (Equipment, Effigy, Character, etc.)
- Each child contains the specific content for that mode
- **Do NOT** put Inventory or Stash in DynamicArea - they're separate and always visible

### Tip 3: Button Visual Feedback
For best UX:
- Use distinct sprites for active/inactive states
- Or use contrasting colors (white vs gray)
- Consider hover states (Unity's Button component handles this)

### Tip 4: Keyboard Shortcuts
You can call these methods from code:
```csharp
navigationController.NextPanel();      // Cycle to next
navigationController.PreviousPanel();  // Cycle to previous
navigationController.SwitchToPanel(2); // Jump to specific panel
```

Bind these to keyboard shortcuts if desired!

---

## 🎬 Quick Reference: Full Setup

1. ✅ Create NavigationBar GameObject
2. ✅ Attach PanelNavigationController script
3. ✅ Add Navigation Items (one per button)
4. ✅ Drag button references
5. ✅ Drag display panel references
6. ✅ Drag dynamic panel references
7. ✅ (Optional) Assign sprite states
8. ✅ Set Starting Index
9. ✅ Enable sprite swapping if using sprites
10. ✅ Test!

---

## 🚀 Result

You now have a clean, flexible navigation system that:
- ✅ Controls multiple panel groups with one script
- ✅ Supports sprite-based or color-based button states
- ✅ Optional fade transitions
- ✅ Easy to extend (just add more Navigation Items)
- ✅ Clean, organized code

**No custom code needed!** Just configure in Inspector and go! 🎉

---

**End of Setup Guide**

