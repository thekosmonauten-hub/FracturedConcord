# Unity UI Equipment Screen Scripts
## Ready-to-Use Components

**Created:** October 30, 2025  
**Purpose:** Complete Unity UI (Canvas-based) implementation for Equipment Screen

---

## 📁 Files Created

All scripts are ready to use! Just create the UI in Unity and attach these components.

### ✅ Core Scripts (14 total)

| Script | Purpose | Attach To |
|--------|---------|-----------|
| `EquipmentScreenUI.cs` | Main controller | Root Canvas GameObject |
| `InventoryGridUI.cs` | Inventory grid manager | Inventory grid container |
| `InventorySlotUI.cs` | Single inventory slot | Inventory slot prefab |
| `EquipmentSlotUI.cs` | Equipment slot | Equipment slot prefab |
| `EffigyGridUI.cs` | Effigy grid manager (6x4) | Effigy grid container |
| `EffigyGridCellUI.cs` | Single effigy cell | Effigy cell prefab |
| `EffigyStoragePanel.cs` | ~~Sliding storage panel~~ | ~~Not needed if using PanelNavigationController~~ |
| `EffigyStorageUI.cs` | Effigy storage grid display | Effigy storage content area |
| `EmbossingGridUI.cs` | Embossing storage grid | Embossing content area |
| `EmbossingEffect.cs` | Embossing data (ScriptableObject) | Create as assets in Resources |
| `CardCarouselUI.cs` | Card carousel for embossing | EmbossingCardDisplay GameObject |
| `CardDisplay.cs` | Individual card display | Card prefab |
| `StashTabManager.cs` | Dynamic stash tabs | Stash section container |
| `PanelNavigationController.cs` | Multi-panel navigation | Navigation parent GameObject |

---

## 🎯 Quick Start Guide

### Step 1: Create Canvas
1. **Hierarchy** → Right-click → **UI** → **Canvas**
2. Name it: `EquipmentScreenCanvas`
3. **Canvas Scaler** settings:
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 x 1080**
   - Match: **0.5**

### Step 2: Create Main Structure
```
EquipmentScreenCanvas
├── MainContainer (Empty GameObject with RectTransform)
│   ├── HeaderBar
│   ├── ContentArea
│   │   ├── EquipmentPanel
│   │   │   ├── EffigyGridContainer ← Attach EffigyGridUI.cs
│   │   │   ├── EquipmentSlots (10 slots)
│   │   │   └── CurrencySection
│   │   └── InventoryPanel
│   │       ├── InventoryGrid ← Attach InventoryGridUI.cs
│   │       └── StashSection ← Attach StashTabManager.cs
│   │           ├── StashTabsContainer (HorizontalLayoutGroup)
│   │           │   ├── (Tab buttons created dynamically)
│   │           │   ├── AddTabButton (+)
│   │           │   ├── RenameTabButton (Rename)
│   │           │   └── DeleteTabButton (Delete)
│   │           └── StashGrid ← Attach InventoryGridUI.cs
│   ├── EffigyStoragePanel ← Attach EffigyStoragePanel.cs
│   └── BottomControls
└── EventSystem (auto-created)
```

### Step 3: Create Prefabs

#### A) Inventory Slot Prefab
1. Create **UI** → **Image** named "InventorySlot"
2. Attach `InventorySlotUI.cs`
3. Add child **Image** for item icon (disable by default)
4. Add child **TextMeshProUGUI** for item name (disable by default)
5. Wire up references in Inspector:
   - `background` → parent Image
   - `itemIcon` → icon Image
   - `itemLabel` → TextMeshProUGUI
6. Save as prefab: `Assets/Prefabs/UI/InventorySlot.prefab`

#### B) Equipment Slot Prefab
1. Create **UI** → **Image** named "EquipmentSlot"
2. Set size based on slot type (e.g., 100x100 for helmet)
3. Attach `EquipmentSlotUI.cs`
4. Add child **Image** for item icon
5. Add child **TextMeshProUGUI** for slot label ("HELMET")
6. Add child **TextMeshProUGUI** for item name
7. Wire up references in Inspector
8. Save as prefab: `Assets/Prefabs/UI/EquipmentSlot.prefab`

#### C) Effigy Cell Prefab
1. Create **UI** → **Image** named "EffigyCell"
2. Set size: **60 x 60**
3. Attach `EffigyGridCellUI.cs`
4. Component auto-wires background Image
5. Save as prefab: `Assets/Prefabs/UI/EffigyCell.prefab`

#### D) Stash Tab Button Prefab
1. Create **UI** → **Button** named "StashTabButton"
2. Set size: **80 x 30** (or to taste)
3. Ensure it has a child **TextMeshProUGUI** for the tab name
4. **For Sprite-Based Approach:**
   - Set the Button's **Image** component to your default (inactive) sprite
   - StashTabManager will swap sprites based on active/inactive state
5. **For Color-Only Approach:**
   - Leave sprites blank in StashTabManager
   - Style button colors in Inspector (normal, hover, pressed)
6. Save as prefab: `Assets/Prefabs/UI/StashTabButton.prefab`

### Step 4: Wire Up Main Controller
1. Select `EquipmentScreenCanvas`
2. Attach `EquipmentScreenUI.cs`
3. Drag all references in Inspector:
   - **Equipment Slots:** 10 equipment slot GameObjects
   - **Inventory:** InventoryGridUI and StashGridUI references
   - **Buttons:** All button references
   - **Effigy System:** EffigyGridUI, EffigyStoragePanel, button
   - **Currency Tabs:** 4 tab buttons + 4 tab content panels

### Step 5: Configure Grid Components

#### InventoryGridUI Settings:
- Grid Width: `10`
- Grid Height: `6`
- Cell Size: `60 x 60`
- Spacing: `2 x 2`
- Slot Prefab: Drag your InventorySlot prefab
- Grid Container: Drag the grid's container GameObject

#### EffigyGridUI Settings:
**Grid Settings:**
- Cell Size: `60` (default, adjustable for bigger cells)
- Cell Spacing: `2` (default, adjustable for more spacing)

**References:**
- Cell Prefab: Drag your EffigyCell prefab
- Grid Container: Drag the effigy grid container

**Colors:**
- Empty Cell Color: Dark gray (configurable)
- Valid Placement Color: Green (configurable)
- Invalid Placement Color: Red (configurable)

**Pro Tip:** Try Cell Size: 70 and Spacing: 5 for a more spacious feel!

#### EffigyStorageUI Settings:
**Grid Settings:**
- Grid Columns: `4` (number of columns)
- Grid Rows: `20` (number of rows - defines total capacity: 4×20 = 80 cells)
- Cell Size: `80` (size of each cell in pixels)
- Cell Spacing: `10` (spacing between cells)
- Grid Padding: `10` (padding around grid edges)

**References:**
- **Cell Prefab:** (Optional) Custom cell prefab - leave empty to auto-generate
- **Grid Container:** Transform where cells are created (usually Content in ScrollView)
- **Effigy Grid:** Reference to main EffigyGridUI for drag functionality

**Visual Settings:**
- Empty Cell Color: Dark gray with 50% opacity (for empty cells)
- Border Color: Medium gray (for cell borders)

**How It Works:**
- ✅ **ALL cells are ALWAYS visible** (like inventory grid)
- ✅ Fixed grid size (e.g., 4 columns × 20 rows = 80 cells total capacity)
- ✅ Cells show effigy when occupied, or empty when not
- ✅ ScrollView allows scrolling through all cells
- ✅ Drag effigies from cells to main effigy grid
- ✅ Auto-loads effigies from `Resources/Items/Effigies/` on start

**Note:** Works exactly like inventory grid - all cells always visible, empty or not!

#### EmbossingGridUI Settings:
**Grid Settings:**
- Grid Columns: `4` (number of columns)
- Grid Rows: `20` (number of rows - capacity: 4×20 = 80 cells)
- Cell Size: `80` (size of each cell in pixels)
- Cell Spacing: `10` (spacing between cells)
- Grid Padding: `10` (padding around grid edges)

**References:**
- **Cell Prefab:** (Optional) Leave empty to auto-generate
- **Grid Container:** Transform where cells are created (Content in ScrollView)

**Visual Settings:**
- Empty Cell Color: Dark gray with 50% opacity
- Border Color: Medium gray

**Auto-Load Settings:**
- **Auto Load From Resources:** ✓ (loads embossing effects on Start)
- **Resources Path:** `"Embossing/Effects"` (folder path in Resources)

**How It Works:**
- ✅ **ALL cells ALWAYS visible** (exactly like inventory/effigy storage)
- ✅ Fixed grid size (e.g., 4×20 = 80 cells)
- ✅ Scrollable to see all cells
- ✅ Auto-loads from Resources folder (or set manually)
- ✅ Color-coded by embossing type (Offensive/Defensive/Utility/Special)
- ✅ Ready for future implementation (click to apply, tooltips, etc.)

**Note:** Grid shows even if no embossing effects exist yet - perfect for UI work-in-progress!

#### StashTabManager Settings:
**References:**
- Tab Button Prefab: Drag your StashTabButton prefab
- Tab Button Container: Drag the horizontal container for tab buttons
- Stash Grid: Drag the InventoryGridUI component attached to stash
- Add Tab Button: Drag the "+" button
- Rename Tab Button: Drag the "Rename" button (optional)
- Delete Tab Button: Drag the "Delete" button (optional)

**Visual Style (Sprite Mode):**
- Active Tab Sprite: Your sprite for selected/active tab
- Inactive Tab Sprite: Your sprite for unselected tab
- Active Tab Color: Color tint for active tab (applies to sprite or color mode)
- Inactive Tab Color: Color tint for inactive tab

**Settings:**
- Max Tabs: `10` (or adjust as needed)
- Default Tab Name Prefix: "Tab " (used when creating new tabs)

**Note:** If both Active and Inactive Tab Sprites are assigned, the system uses sprite swapping. If sprites are null, it falls back to color-only mode (ColorBlock).

#### PanelNavigationController Settings:
**Navigation Items List:**
For each navigation button (Equipment, Effigy, etc.):
- **Navigation Button:** The button to click
- **Display Panel:** GameObject in Display Window area
- **Dynamic Panel:** GameObject in DynamicArea
- **Active Button Sprite:** (Optional) Sprite when selected
- **Inactive Button Sprite:** (Optional) Sprite when unselected
- **Active Button Color:** Color when selected (default: white)
- **Inactive Button Color:** Color when unselected (default: light gray)

**Settings:**
- **Starting Index:** Which panel to show on start (0 = first)
- **Use Sprite Swapping:** Enable sprite-based button visuals
- **Enable Fade Transitions:** Smooth fade between panels
- **Fade Duration:** Transition speed (default: 0.2s)

**Visual Style (Sprite-Based):**
- Active Tab Sprite: Your active/selected tab sprite
- Inactive Tab Sprite: Your inactive/unselected tab sprite
- Active Tab Color: Color tint for active tabs (default: light blue)
- Inactive Tab Color: Color tint for inactive tabs (default: gray)

*Note: If sprites are provided, the system uses sprite swapping. If left null, it uses color-only approach.*

---

## 🔌 Component Dependencies

### Required Unity Packages:
- ✅ **TextMeshPro** (Window → TextMeshPro → Import TMP Essential Resources)
- ✅ **LeanTween** (Already in your project!)

### Required Namespaces (Already Added):
```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
```

---

## 📋 Component Reference

### EquipmentScreenUI.cs
**Purpose:** Main controller for entire equipment screen

**Key Methods:**
- `EquipItem(ItemData, EquipmentType)` - Equip item to slot
- `UnequipItem(EquipmentType)` - Remove item from slot
- `GetEquippedItem(EquipmentType)` - Get currently equipped item

**Events Handled:**
- All button clicks
- Equipment slot interactions
- Tab switching
- Effigy storage open/close

---

### InventoryGridUI.cs
**Purpose:** Manages dynamic inventory/stash grid

**Key Methods:**
- `GenerateGrid()` - Creates all slots (auto-called in Start)
- `SortInventory()` - Sort items (TODO: implement logic)
- `FilterInventory(string)` - Filter items (TODO: implement logic)
- `GetSlot(int x, int y)` - Get specific slot

**Auto-generates:** Uses GridLayoutGroup for automatic layout

---

### InventorySlotUI.cs
**Purpose:** Individual inventory slot behavior

**Key Methods:**
- `SetPosition(int x, int y)` - Set grid position
- `SetOccupied(bool, Sprite, string)` - Show/hide item

**Events:**
- `OnSlotClicked` - Fired when clicked
- `OnSlotHovered` - Fired when hovered

**Implements:** IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler

---

### EquipmentSlotUI.cs
**Purpose:** Equipment slot display and interaction

**Key Methods:**
- `Initialize(EquipmentType, string)` - Setup slot type and label
- `SetEquippedItem(ItemData)` - Display equipped item
- `GetEquippedItem()` - Get current item

**Events:**
- `OnSlotClicked(EquipmentType)` - Fired when clicked
- `OnSlotHovered(EquipmentType, Vector2)` - Fired when hovered

**Implements:** IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler

---

### EffigyGridUI.cs
**Purpose:** 6x4 effigy placement grid with drag & drop

**Key Methods:**
- `StartDragFromStorage(Effigy)` - Begin drag from storage
- `CreateDragVisual(Effigy)` - Create visual that follows cursor (smart centering!)
- `TryPlaceEffigy(Effigy, int, int)` - Place effigy at position
- `CanPlaceEffigy(Effigy, int, int)` - Check if placement is valid
- `RemoveEffigy(Effigy)` - Remove effigy from grid
- `GetPlacedEffigies()` - Get all placed effigies

**Features:**
- ✅ **Smart drag visual** - centers on effigy's occupied cells, not bounding box!
- ✅ **Cursor-following preview** - see exactly what you're placing
- ✅ Visual placement preview (green = valid, red = invalid)
- ✅ Drag from storage to grid
- ✅ Drag within grid to reposition
- ✅ Shape-based collision detection
- ✅ **ESC key** to cancel drag operation

**Drag Improvements:**
- Effigy visual follows cursor precisely
- Pivot point calculated from **center of actual occupied cells**
- Works perfectly for L-shapes, T-shapes, and irregular shapes
- No more fighting with top-left corner pickup!
- **Smart storage management** - effigy removed from storage on successful placement
- Storage slot dims during drag, restores on cancel

**Placement Logic:**
- **Success:** Effigy placed in grid → removed from storage (now equipped!)
- **Failure:** Invalid placement → effigy stays in storage, slot restored
- **Cancel:** ESC or right-click → effigy stays in storage, slot restored

**Unequip Methods:**
- ✅ **Right-click on effigy** → instantly unequips and returns to storage
- ✅ **Drag effigy outside grid** → releases outside bounds = unequip and return to storage
- ✅ **Repositioning:** Drag within grid to move effigies around
- ✅ **Failed repositioning:** Returns to original position (or unequips if impossible)

---

### EffigyGridCellUI.cs
**Purpose:** Individual effigy grid cell

**Key Methods:**
- `SetHighlight(Color)` - Highlight during drag preview
- `ClearHighlight()` - Remove highlight

**Events:**
- `OnCellMouseDown(int, int, PointerEventData)`
- `OnCellMouseEnter(int, int)`
- `OnCellMouseUp(int, int, PointerEventData)`

**Implements:** IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler

---

### EffigyStoragePanel.cs ⚠️ DEPRECATED
**Status:** NOT NEEDED if using `PanelNavigationController.cs`

**Original Purpose:** Sliding panel animation controller for effigy storage

**Why Not Needed:**
- `PanelNavigationController` already handles showing/hiding the effigy storage panel
- Use PanelNavigationController for cleaner integration with your navigation system
- Only use this if you want a separate slide-in animation independent of navigation

**If You Still Want Slide Animation:**
You can combine both by having PanelNavigationController activate a panel that then uses EffigyStoragePanel for slide effects. But for most cases, just use PanelNavigationController.

---

### EffigyStorageUI.cs
**Purpose:** Scrollable effigy storage grid - works like inventory with always-visible cells

**Key Methods:**
- `GenerateGrid()` - Creates all cells (auto-called on Start)
- `AddEffigy(Effigy)` - Add effigy to storage
- `RemoveEffigy(Effigy)` - Remove effigy from storage
- `ClearStorage()` - Clear all effigies (cells remain visible)
- `GetCapacity()` - Get total storage capacity
- `GetStoredCount()` - Get number of stored effigies

**Features:**
- ✅ **ALL cells ALWAYS visible** (exactly like inventory grid)
- ✅ Fixed grid size (e.g., 4 columns × 20 rows = 80 cells)
- ✅ Scrollable via ScrollView parent
- ✅ Empty cells remain visible (dimmed color)
- ✅ Occupied cells show effigy icon and name
- ✅ Drag effigies from storage to main grid
- ✅ Auto-loads effigies from Resources folder
- ✅ Automatic ContentSizeFitter for vertical scrolling

**How It Works:**
1. Generates fixed grid of cells on Start (all visible)
2. Loads effigies from Resources
3. Places effigies in cells (first X cells are occupied)
4. Remaining cells stay empty but visible
5. User can scroll to see all cells

**Includes Helper:**
- `EffigyStorageSlotUI` component on each cell for display and drag functionality

---

### EmbossingGridUI.cs
**Purpose:** Scrollable embossing storage grid - works like inventory with always-visible cells

**Key Methods:**
- `GenerateGrid()` - Creates all cells (auto-called on Start)
- `SetEmbossings(List<EmbossingEffect>)` - Manually set embossing list
- `AddEmbossing(EmbossingEffect)` - Add embossing to storage
- `RemoveEmbossing(EmbossingEffect)` - Remove embossing from storage
- `ClearStorage()` - Clear all embossings (cells remain visible)
- `GetCapacity()` - Get total storage capacity
- `GetStoredCount()` - Get number of stored embossings

**Features:**
- ✅ **ALL cells ALWAYS visible** (exactly like inventory grid)
- ✅ Fixed grid size (e.g., 4 columns × 20 rows = 80 cells)
- ✅ Scrollable via ScrollView parent
- ✅ Empty cells remain visible (dimmed color)
- ✅ Occupied cells show embossing icon and name
- ✅ Color-coded by embossing type (Red/Blue/Green/Purple)
- ✅ Auto-loads from Resources folder (optional)
- ✅ Automatic ContentSizeFitter for vertical scrolling

**How It Works:**
1. Generates fixed grid of cells on Start (all visible)
2. Loads embossing effects from Resources (if any exist)
3. Places embossings in cells (first X cells are occupied)
4. Remaining cells stay empty but visible
5. User can scroll to see all cells

**Includes Helper:**
- `EmbossingSlotUI` component on each cell for display and interaction

**For Later:**
- TODO: Tooltip system showing embossing details
- TODO: Click to select/apply embossing to card
- TODO: Requirement validation UI
- TODO: Filter/sort by type or rarity

---

### CardCarouselUI.cs
**Purpose:** Smooth snapping carousel for deck card selection in embossing UI

**Key Methods:**
- `LoadDeckCards()` - Auto-loads from DeckManager.Instance
- `NavigateCard(int direction)` - Navigate previous/next
- `SnapToCard(int index, bool animated)` - Snap to specific card
- `GetSelectedCard()` - Get currently centered card
- `RefreshCarousel()` - Reload deck (call when deck changes)

**Features:**
- ✅ **Smooth snapping** to center card (Ease Out Cubic animation)
- ✅ **Auto-scaling** - center card large (1.0x), sides small (0.7x)
- ✅ **Auto-dimming** - side cards at 60% opacity
- ✅ **Button navigation** - Previous/Next buttons
- ✅ **Swipe navigation** - Drag/swipe to navigate
- ✅ **Smart button states** - Disabled at first/last card
- ✅ **Test mode** - Generates dummy cards when DeckManager not available
- ✅ **DeckManager integration** - Uses singleton pattern

**DeckManager Integration:**
- Uses `DeckManager.Instance.GetActiveDeckAsCards()`
- Fallback to test cards if DeckManager not found
- Perfect for testing in Equipment Screen scene!

**Test Mode Settings:**
- Use Test Cards: ✓ (generates 5 test cards like "Flame Strike", "Ice Barrier")
- Test Card Count: 5 (adjustable)
- Disable when DeckManager is ready

---

### CardDisplay.cs
**Purpose:** Individual card visual in carousel

**Key Methods:**
- `SetCard(Card card)` - Display a card's data
- `GetCard()` - Get the displayed card
- `DisplayEmbossingSlots(Card)` - Show embossing slots (TODO)

**Displays:**
- Card name (TextMeshProUGUI)
- Card description (TextMeshProUGUI)
- Card art (Image)
- Embossing slot indicators (placeholder)

**Auto-Setup:**
- Finds components automatically if not assigned
- Uses default sprite if card has no art

---

### EmbossingEffect.cs (ScriptableObject)
**Purpose:** Data class for embossing effects (not a UI component)

**Properties:**
- `embossingName` - Display name
- `icon` - Visual icon
- `description` - Effect description
- `requiredLevel` - Min level to apply
- `requiredStrength/Dexterity/Intelligence` - Stat requirements
- `rarity` - Normal/Magic/Rare/Unique
- `embossingType` - Offensive/Defensive/Utility/Special
- `modifiers` - List of stat bonuses (Affix)

**Methods:**
- `GetRarityColor()` - Returns color based on rarity
- `GetTypeColor()` - Returns color based on type
- `MeetsRequirements(...)` - Checks if player meets requirements

**Create In Unity:**
- Right-click → Create → Dexiled → Embossing → Embossing Effect
- Save in `Resources/Embossing/Effects/` for auto-loading

---

### StashTabManager.cs
**Purpose:** Dynamic stash tab management

**Key Methods:**
- `CreateTab(string name)` - Create new stash tab
- `SwitchToTab(int index)` - Switch to specific tab
- `RenameTab(int index, string newName)` - Rename a tab
- `DeleteTab(int index)` - Delete a tab (keeps at least 1)
- `GetCurrentTab()` - Get active tab data
- `AddItemToCurrentTab(ItemData)` - Add item to current tab
- `RemoveItemFromCurrentTab(ItemData)` - Remove item from current tab
- `SaveTabs()` - Save all tabs (TODO: implement serialization)
- `LoadTabs()` - Load saved tabs (TODO: implement deserialization)

**Features:**
- ✅ Dynamic tab creation (up to max limit)
- ✅ Tab switching with visual feedback
- ✅ **Dual visual modes:** Sprite swapping OR color tinting
- ✅ Per-tab item storage (List<ItemData>)
- ✅ Rename tabs
- ✅ Delete tabs (protects last tab)
- ✅ Auto-initializes with 3 default tabs

**Visual Modes:**
- **Sprite Mode:** Assign both `activeTabSprite` and `inactiveTabSprite` to swap sprites on tab state change
- **Color Mode:** Leave sprites null to use ColorBlock color tinting instead
- Both modes support color tinting for additional visual distinction
- ✅ Sprite-based visual states (active/inactive)
- ✅ Fallback to color-only mode if sprites not provided

---

### PanelNavigationController.cs
**Purpose:** Multi-panel navigation system for switching between different screen modes (Equipment, Effigy, Character, etc.) while keeping Inventory/Stash always visible

**Key Methods:**
- `SwitchToPanel(int index)` - Switch to specific panel set
- `NextPanel()` - Cycle to next panel
- `PreviousPanel()` - Cycle to previous panel
- `GetCurrentIndex()` - Get active panel index
- `AddNavigationItem(...)` - Add navigation item at runtime

**Features:**
- ✅ Controls multiple panel groups simultaneously
- ✅ Example: Equipment button shows Equipment Display Panel + Equipment Dynamic Panel
- ✅ **Does NOT control always-visible panels** (Inventory/Stash stay visible)
- ✅ Sprite swapping for navigation buttons
- ✅ Color tinting support
- ✅ Optional fade transitions between panels
- ✅ Easy setup with NavigationItem list in Inspector

**How It Works:**
Each `NavigationItem` contains:
- **Navigation Button** - The button to click
- **Display Panel** - Panel in the "Display Window" (e.g., EquipmentNavDisplay)
- **Dynamic Panel** - Panel in the "Dynamic Area" (e.g., Equipment child)
- **Visual States** - Optional sprites and colors for button states

When you click a button, it:
1. Deactivates previous Display Panel + Dynamic Panel
2. Activates new Display Panel + Dynamic Panel
3. Updates button visuals (sprite/color)

**Setup Example:**
```
Navigation Item 0 (Equipment):
  - Navigation Button: EquipmentButton
  - Display Panel: EquipmentNavDisplay
  - Dynamic Panel: DynamicArea/Equipment
  
Navigation Item 1 (Effigy):
  - Navigation Button: EffigyButton
  - Display Panel: EffigyNavDisplay
  - Dynamic Panel: DynamicArea/Effigy

Navigation Item 2 (Character):
  - Navigation Button: CharacterButton
  - Display Panel: CharacterNavDisplay
  - Dynamic Panel: DynamicArea/Character
```

**Important:** Inventory and Stash are always visible - do NOT add them to Navigation Items!

---

## 🎨 Visual Setup Tips

### Colors
All components have sensible default colors. Adjust in Inspector:
- **Empty slots:** Dark gray `(0.12, 0.14, 0.16)`
- **Hover:** Lighter gray `(0.2, 0.22, 0.24)`
- **Occupied:** Blue tint `(0.3, 0.4, 0.5)`
- **Valid placement:** Green `(0.2, 1, 0.2, 0.6)`
- **Invalid placement:** Red `(1, 0.2, 0.2, 0.6)`

### Layout Groups
Components use these Unity layout components:
- **GridLayoutGroup** - For inventory/stash/effigy grids
- **VerticalLayoutGroup** - For column layouts
- **HorizontalLayoutGroup** - For row layouts

### Anchors
Set proper anchors for responsive UI:
- **Stretch** for full-screen containers
- **Top-Left** for fixed-position elements
- **Center** for centered elements

---

## ⚠️ Important Notes

### EventSystem Required
Unity UI needs an EventSystem in the scene. It's auto-created with the first Canvas.

### Raycast Targets
- **Enable** on interactive elements (buttons, slots)
- **Disable** on decorative elements (backgrounds, labels)

### Prefab Setup
Create prefabs BEFORE generating grids. The grid scripts instantiate from prefabs.

### LeanTween
EffigyStoragePanel uses LeanTween for animations. Already in your project!

---

## 🔧 TODO Items for You

These scripts are complete and functional, but you'll want to implement:

### High Priority:
1. **Item drag & drop logic** in InventorySlotUI
2. **Tooltip system** for item hover (referenced but not implemented)
3. **Sorting logic** in InventoryGridUI.SortInventory()
4. **Filter UI** for inventory filtering

### Medium Priority:
5. **Currency system** integration
6. **Stash tab save/load** - StashTabManager.SaveTabs() / LoadTabs()
7. **Save/Load** for equipment and effigy placement

### Low Priority:
8. **Animation polish** (fade-ins, scale effects)
9. **Sound effects** on clicks and placements
10. **Contextual menus** (right-click options)

---

## 🧪 Testing Checklist

### Equipment Slots
- [ ] All 10 slots visible
- [ ] Labels display correctly
- [ ] Click events fire
- [ ] Hover changes color
- [ ] Can equip/unequip items

### Inventory Grid
- [ ] 60 slots generated (10x6)
- [ ] Grid layout correct
- [ ] Click events work
- [ ] Hover feedback works

### Effigy Grid
- [ ] 24 cells generated (6x4)
- [ ] Drag from storage works
- [ ] Placement preview shows
- [ ] Green for valid, red for invalid
- [ ] Effigy visual appears on placement
- [ ] Can drag to reposition

### Effigy Storage
- [ ] Panel slides in smoothly
- [ ] Panel slides out smoothly
- [ ] Close button works
- [ ] Storage items display

### Currency Tabs
- [ ] All 4 tabs visible
- [ ] Tab switching works
- [ ] Only active tab content visible
- [ ] Button state updates (color)

### Stash Tabs
- [ ] Default 3 tabs created
- [ ] Tab buttons appear
- [ ] Click tab to switch
- [ ] Active tab highlighted
- [ ] Add tab button works (up to 10 tabs)
- [ ] Add button disabled at max tabs
- [ ] Rename tab works
- [ ] Delete tab works (keeps 1 minimum)
- [ ] Each tab stores separate items

---

## 🚀 Next Steps

1. **Open Unity**
2. **Create Canvas** following Step 1 above
3. **Build hierarchy** following Step 2
4. **Create prefabs** following Step 3
5. **Wire up references** following Step 4
6. **Hit Play** and test!

All the code is ready - you just need to build the UI structure in Unity!

---

## 📖 Related Documentation

- **Full Migration Guide:** `EQUIPMENT_SCREEN_MIGRATION_GUIDE.md`
- **UI Toolkit Reference:** `EQUIPMENT_SCREEN_UITOOLKIT_REFERENCES.md`
- **Panel Navigation Setup:** `PANEL_NAVIGATION_SETUP_GUIDE.md` ⭐
- **Effigy Grid Centering:** `EFFIGY_GRID_CENTERING_GUIDE.md` ⭐
- **Effigy Storage Setup:** `EFFIGY_STORAGE_SETUP_GUIDE.md` ⭐
- **Embossing Grid Setup:** `EMBOSSING_SETUP_GUIDE.md` ⭐
- **Card Carousel Setup:** `CARD_CAROUSEL_SETUP_GUIDE.md` ⭐ NEW!
- **Unity UI Docs:** https://docs.unity3d.com/Manual/UISystem.html

---

**Happy UI Building! 🎨**

Need help? Check the migration guide for detailed examples and troubleshooting.

**Setting up navigation?** See `PANEL_NAVIGATION_SETUP_GUIDE.md` for step-by-step instructions!  
**Need to center the Effigy Grid?** See `EFFIGY_GRID_CENTERING_GUIDE.md` for quick setup!  
**Setting up Effigy Storage?** See `EFFIGY_STORAGE_SETUP_GUIDE.md` for complete guide!  
**Adding Embossing Grid?** See `EMBOSSING_SETUP_GUIDE.md` for embossing system setup!  
**Creating Card Carousel?** See `CARD_CAROUSEL_SETUP_GUIDE.md` for smooth card selection! ⭐ NEW!

