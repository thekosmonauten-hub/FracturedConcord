# Effigy Storage Setup Guide
## Scrollable Grid with Always-Visible Cells

**Component:** `EffigyStorageUI.cs`  
**Works Like:** Inventory grid - all cells always visible, empty or not

---

## 🎯 What You're Building

A scrollable effigy storage grid that works EXACTLY like your inventory:
- ✅ All cells always visible (empty cells show as empty, not hidden)
- ✅ Fixed grid size (e.g., 4 columns × 20 rows = 80 cells)
- ✅ Scrollable to see all cells
- ✅ Effigies occupy cells when stored
- ✅ Drag from storage cells to main effigy grid

**Visual Comparison:**
```
GOOD (Like this):           BAD (Not this):
┌─────────────────┐         ┌─────────────────┐
│ [🔥] [❄️] [⚡] │         │                 │
│ [░] [░] [░]     │  ←YES   │  (blank space)  │  ←NO
│ [░] [░] [░]     │         │                 │
└─────────────────┘         └─────────────────┘
```

---

## 🏗️ Hierarchy Structure

```
ScrollView (ScrollRect component)
└── Viewport (with Mask)
    └── Content (Attach EffigyStorageUI.cs)
        └── (Grid cells created automatically - all visible)
            ├── StorageCell_0_0
            ├── StorageCell_1_0
            ├── ... (80 cells total if 4×20 grid)
```

---

## 🛠️ Step 1: Create ScrollView

1. **Right-click in Hierarchy** → UI → Scroll View
2. Name it: `EffigyStorageScrollView`
3. **Configure ScrollRect component:**
   - Horizontal: ☐ (unchecked - no horizontal scroll)
   - Vertical: ✓ (checked - vertical scroll only)
   - Movement Type: Elastic or Clamped
   - Scroll Sensitivity: 20
4. **Delete** the Horizontal Scrollbar (if created)
5. Keep the Vertical Scrollbar (or hide it if you want invisible scrolling)

---

## 🛠️ Step 2: Select Content (Child of Viewport)

1. **Find** the Content GameObject (child of Viewport)
2. **Attach** `EffigyStorageUI.cs` to it
3. This is where all the cells will be generated!

---

## 🛠️ Step 3: Configure EffigyStorageUI in Inspector

Select the **Content** GameObject:

### Grid Settings
- **Grid Columns:** `4` (how many columns wide)
- **Grid Rows:** `20` (how many rows tall - total capacity = 4×20 = 80 cells)
- **Cell Size:** `80` (pixels - try 70-100)
- **Cell Spacing:** `10` (pixels - try 8-15)
- **Grid Padding:** `10` (padding around edges)

### References
- **Cell Prefab:** (Optional) Leave empty to auto-generate cells
- **Grid Container:** Drag the Content GameObject itself (self-reference)
- **Effigy Grid:** Drag your main EffigyGridUI component (for drag functionality)

### Visual Settings
- **Empty Cell Color:** RGB(25, 25, 25) with Alpha 128 (dimmed gray for empty cells)
- **Border Color:** RGB(100, 100, 100) (medium gray for cell borders)

---

## 🎯 Recommended Settings

### Standard Setup (80 cells):
```
Grid Columns: 4
Grid Rows: 20
Cell Size: 80
Cell Spacing: 10
Grid Padding: 10

Total Capacity: 4 × 20 = 80 effigies
```

### Compact Setup (100 cells):
```
Grid Columns: 5
Grid Rows: 20
Cell Size: 70
Cell Spacing: 8
Grid Padding: 10

Total Capacity: 5 × 20 = 100 effigies
```

### Spacious Setup (60 cells):
```
Grid Columns: 3
Grid Rows: 20
Cell Size: 100
Cell Spacing: 15
Grid Padding: 15

Total Capacity: 3 × 20 = 60 effigies
```

---

## 🧪 Testing

1. **Play the game**
2. **Open effigy storage** (via PanelNavigationController)
3. **Check:**
   - [ ] ALL cells are visible (even empty ones)
   - [ ] Grid looks like inventory grid
   - [ ] Can scroll vertically through all cells
   - [ ] Effigies appear in first X cells (where X = number of effigies)
   - [ ] Remaining cells are empty but visible (dimmed)
   - [ ] Can drag effigy from storage cell to main grid
   - [ ] Empty cells are not draggable

---

## 🎨 Visual Comparison

### What You'll See:

**With 10 Effigies in 4×20 Grid:**
```
┌─────────────────────────┐  ← ScrollView top
│ [🔥] [❄️] [⚡] [💀]     │  Row 0: Occupied
│ [🌿] [🔥] [❄️] [⚡]     │  Row 1: Occupied
│ [💀] [🌿] [░] [░]       │  Row 2: Partial
│ [░] [░] [░] [░]         │  Row 3: Empty
│ [░] [░] [░] [░]         │  Row 4: Empty
│      ... scroll ...     │
│ [░] [░] [░] [░]         │  Row 19: Empty
└─────────────────────────┘  ← ScrollView bottom
```

- 🔥 = Effigy (bright, draggable)
- [░] = Empty cell (dimmed, not draggable)
- All cells always visible, just scroll to see more!

---

## 🚨 Common Issues

### ❌ No cells visible
**Problem:** Grid Container reference not set  
**Solution:** Drag the Content GameObject to "Grid Container" field in Inspector

### ❌ Can't scroll
**Problem:** ContentSizeFitter not working or ScrollRect disabled  
**Solution:** 
- Script auto-adds ContentSizeFitter, but check it's there
- Make sure ScrollRect's Vertical is ✓ checked
- Check Content is taller than Viewport

### ❌ Cells don't show empty slots
**Problem:** This is the expected behavior now!  
**Solution:** All cells are always visible - empty ones appear dimmed

### ❌ Grid doesn't fill width
**Problem:** GridLayoutGroup alignment  
**Solution:** Script sets childAlignment to UpperCenter, check Grid Container RectTransform is properly sized

---

## ✅ Quick Verification

After setup:
- [ ] EffigyStorageUI.cs attached to Content (child of ScrollView/Viewport)
- [ ] Grid Container reference points to Content (self-reference)
- [ ] Effigy Grid reference points to main EffigyGridUI
- [ ] Grid Columns and Rows set (e.g., 4×20)
- [ ] Cell Size and Spacing configured
- [ ] **ALL cells are visible** (empty ones are dimmed)
- [ ] Can scroll through cells vertically
- [ ] Effigies appear in first cells
- [ ] Can drag effigies to main grid

---

## 💡 Pro Tips

### Tip 1: Capacity Planning
Calculate total capacity: `Grid Columns × Grid Rows`
- 4×20 = 80 effigies
- 3×30 = 90 effigies
- 5×15 = 75 effigies

Choose based on how many effigies your game will have!

### Tip 2: Visual Balance
Storage cells are usually bigger than main grid cells:
- **Main Grid:** 60-70px cells
- **Storage:** 80-100px cells
- Makes it easier to browse and select

### Tip 3: Scroll Performance
If you have MANY effigies (100+), consider:
- Using object pooling (advanced)
- Reducing grid rows to what you actually need
- Most games need 60-100 storage slots max

---

**Perfect!** Your effigy storage now works exactly like an inventory grid! 🎉

All cells always visible, scrollable, and ready to use!
