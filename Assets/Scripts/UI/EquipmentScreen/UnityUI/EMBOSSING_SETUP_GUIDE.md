# Embossing Grid Setup Guide
## Scrollable Grid for Embossing Effects

**Component:** `EmbossingGridUI.cs`  
**Works Like:** Inventory/Effigy Storage - all cells always visible, scrollable

---

## 📖 What is Embossing?

**Embossing System Overview:**
- Cards can have up to 5 embossing slots
- Slots are either innate or crafted with "Inscription Seals"
- Embossing effects are permanent buffs applied to cards
- Effects can be added, removed, or replaced (if requirements are met)
- Requirements: Stat or level based

---

## 🎯 What You're Building

A scrollable embossing storage grid:
- ✅ All cells always visible (empty cells show as empty, not hidden)
- ✅ Fixed grid size (e.g., 4 columns × 20 rows = 80 cells)
- ✅ Scrollable to see all cells
- ✅ Embossing effects occupy cells when available
- ✅ Click on embossing to view/apply (TODO: implement later)

**Visual:**
```
┌─────────────────────────┐  ← Scroll top
│ [OFF] [DEF] [UTL] [SPC] │  Row 0: Embossings
│ [OFF] [DEF] [░] [░]     │  Row 1: Partial
│ [░] [░] [░] [░]         │  Row 2: Empty (visible!)
│ [░] [░] [░] [░]         │  Row 3: Empty (visible!)
│      ... scroll ...     │
│ [░] [░] [░] [░]         │  Row 19: Empty
└─────────────────────────┘  ← Scroll bottom

Legend:
  OFF = Offensive (Red)
  DEF = Defensive (Blue)
  UTL = Utility (Green)
  SPC = Special (Purple)
  [░] = Empty cell (dimmed, visible)
```

---

## 🏗️ Hierarchy Structure

```
DynamicArea/Embossing (Shown/hidden by PanelNavigationController)
└── ScrollView (ScrollRect component)
    └── Viewport (with Mask)
        └── Content (Attach EmbossingGridUI.cs)
            └── (Grid cells created automatically - all visible)
                ├── EmbossingCell_0_0
                ├── EmbossingCell_1_0
                ├── ... (80 cells total if 4×20 grid)
```

---

## 🛠️ Step 1: Create ScrollView

1. **In DynamicArea** → Right-click → UI → Scroll View
2. Name it: `Embossing` (or EmbossingScrollView)
3. **Configure ScrollRect component:**
   - Horizontal: ☐ (unchecked)
   - Vertical: ✓ (checked)
   - Movement Type: Elastic
   - Scroll Sensitivity: 20
4. **Delete** Horizontal Scrollbar
5. Keep Vertical Scrollbar (optional)

---

## 🛠️ Step 2: Configure Content

1. **Select** Content GameObject (child of Viewport)
2. **Attach** `EmbossingGridUI.cs`
3. This is where all cells will be generated!

---

## 🛠️ Step 3: Configure EmbossingGridUI in Inspector

Select the **Content** GameObject:

### Grid Settings
- **Grid Columns:** `4` (how many columns wide)
- **Grid Rows:** `20` (how many rows tall - capacity: 4×20 = 80 cells)
- **Cell Size:** `80` (pixels - try 70-100)
- **Cell Spacing:** `10` (pixels - try 8-15)
- **Grid Padding:** `10` (padding around edges)

### References
- **Cell Prefab:** (Optional) Leave empty to auto-generate
- **Grid Container:** Drag Content GameObject (self-reference)

### Visual Settings
- **Empty Cell Color:** RGB(25, 25, 25) Alpha 128 (dimmed for empty)
- **Border Color:** RGB(100, 100, 100) (cell borders)

### Auto-Load Settings
- **Auto Load From Resources:** ✓ (checked - loads on Start)
- **Resources Path:** `"Embossing/Effects"` (where to find embossing ScriptableObjects)

---

## 🛠️ Step 4: Add to Navigation

**Select** NavigationBar (with PanelNavigationController.cs):

1. **Add Navigation Item** in Inspector
2. **Configure:**
```
Navigation Item X (Embossing):
  - Navigation Button: EmbossingButton
  - Display Panel: (Optional) EmbossingNavDisplay
  - Dynamic Panel: DynamicArea/Embossing
  - Active/Inactive Sprites: Your button sprites
```

---

## 🎨 Step 5: Create Embossing Effects (Later)

When you're ready to implement embossing:

1. **Create folder:** `Assets/Resources/Embossing/Effects/`
2. **Create embossing effects:**
   - Right-click → Create → Dexiled → Embossing → Embossing Effect
3. **Configure each effect:**
   - Name, icon, description
   - Requirements (level, stats)
   - Rarity, type (Offensive/Defensive/Utility/Special)
   - Modifiers (stat bonuses)

**Example Embossing Effects:**
- "Swift Strike" (Offensive) - +10% Attack Speed
- "Iron Skin" (Defensive) - +5 Armor
- "Essence Flow" (Utility) - +20% Mana Regeneration
- "Critical Mastery" (Special) - +5% Critical Strike Chance

---

## 🎯 Recommended Settings

### Standard Setup (80 cells):
```
Grid Columns: 4
Grid Rows: 20
Cell Size: 80
Cell Spacing: 10
Grid Padding: 10

Total Capacity: 80 embossing effects
```

### Compact Setup (100 cells):
```
Grid Columns: 5
Grid Rows: 20
Cell Size: 70
Cell Spacing: 8
Grid Padding: 10

Total Capacity: 100 embossing effects
```

### Spacious Setup (60 cells):
```
Grid Columns: 3
Grid Rows: 20
Cell Size: 100
Cell Spacing: 15
Grid Padding: 15

Total Capacity: 60 embossing effects
```

---

## 🧪 Testing

1. **Play the game**
2. **Click Embossing navigation button**
3. **Check:**
   - [ ] ALL cells are visible (including empty ones)
   - [ ] Grid looks like inventory grid
   - [ ] Can scroll vertically through all cells
   - [ ] Empty cells are dimmed but visible
   - [ ] When you create embossing effects, they appear in first X cells
   - [ ] Can click on embossing cells (logs to console for now)
   - [ ] Hover shows visual feedback

---

## 🎨 Color Coding

### Embossing Types (Auto-colored):
- **Offensive** - Red `(1, 0.3, 0.2)` - Damage, crit, attack speed
- **Defensive** - Blue `(0.3, 0.6, 1)` - Armor, resistances, health
- **Utility** - Green `(0.3, 1, 0.3)` - Resources, cooldown, movement
- **Special** - Purple `(0.8, 0.3, 0.8)` - Unique effects

### Rarity Colors (For text):
- **Normal** - Gray
- **Magic** - Blue
- **Rare** - Gold
- **Unique** - Orange

---

## 🚀 For Later Implementation

When you build the actual embossing system, you'll want to:

### Phase 1: Data
- [x] EmbossingEffect ScriptableObject (DONE!)
- [ ] Create actual embossing effects
- [ ] Define stat modifiers for each
- [ ] Set requirements (level/stats)

### Phase 2: Card Integration
- [ ] Add embossing slots to card data structure
- [ ] Card UI shows embossing slots
- [ ] Apply embossing bonuses to card stats

### Phase 3: Application System
- [ ] Click embossing to select
- [ ] Click card to apply embossing
- [ ] Check requirements before applying
- [ ] Remove/replace embossing logic
- [ ] Inscription Seal crafting system

### Phase 4: UI/UX
- [ ] Tooltip showing embossing details
- [ ] Visual feedback when applying
- [ ] Confirmation dialogs for removal
- [ ] Filter/sort embossings by type/rarity

---

## 💡 Pro Tips

### Tip 1: Start Small
For initial testing, create just 5-10 embossing effects to see the grid working.

### Tip 2: Color Coding Helps
The type colors (red/blue/green/purple) help players quickly identify effect categories.

### Tip 3: Resources Folder Structure
```
Assets/Resources/
└── Embossing/
    └── Effects/
        ├── Offensive/
        │   ├── SwiftStrike.asset
        │   └── CriticalEdge.asset
        ├── Defensive/
        │   ├── IronSkin.asset
        │   └── FortifiedWall.asset
        ├── Utility/
        │   └── EssenceFlow.asset
        └── Special/
            └── MasteryCrest.asset
```

### Tip 4: Capacity Planning
80 cells is usually more than enough. Adjust Grid Rows based on how many embossing effects your game will have.

---

## 🚨 Common Issues

### ❌ No cells visible
**Problem:** Grid Container reference not set  
**Solution:** Drag Content GameObject to "Grid Container" field

### ❌ Grid shows but no embossings
**Problem:** No embossing effects created yet OR wrong Resources path  
**Solution:** 
- This is expected if embossing system isn't implemented yet!
- Grid will show all empty (dimmed) cells
- When you create embossing ScriptableObjects in Resources/Embossing/Effects/, they'll appear automatically

### ❌ Can't scroll
**Problem:** ContentSizeFitter not working  
**Solution:** Script auto-adds it, but verify Content is taller than Viewport

---

## ✅ Quick Verification

After setup:
- [ ] EmbossingGridUI.cs attached to Content
- [ ] Grid Container reference set (self-reference)
- [ ] Grid Columns and Rows configured
- [ ] Cell Size and Spacing set
- [ ] **ALL cells visible** (empty ones are dimmed)
- [ ] Can scroll through all cells
- [ ] Added to PanelNavigationController
- [ ] Embossing button switches to this panel

---

**Perfect!** Your embossing grid is ready for when you implement the actual embossing system! 🎨

For now, you'll see a grid of empty cells. When you create EmbossingEffect ScriptableObjects in `Resources/Embossing/Effects/`, they'll automatically appear!


