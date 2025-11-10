# Equipment Screen - UI Toolkit References
## Current Implementation (Before Migration)

**Last Updated:** October 30, 2025

---

## File Structure

```
Assets/
├── Scripts/
│   └── UI/
│       └── EquipmentScreen/
│           ├── EquipmentScreen.cs ............... Main controller (3,119 lines)
│           ├── EffigyGrid.cs .................... Effigy grid system (681 lines)
│           └── EffigyStorageManager.cs .......... Sliding panel manager (123 lines)
└── UI/
    └── EquipmentScreen/
        ├── EquipmentScreen.uxml ................. Layout definition (119 lines)
        └── EquipmentScreen.uss .................. Styling sheet (1,118 lines)
```

---

## EquipmentScreen.cs - UI Toolkit References

### Namespaces
```csharp
using UnityEngine.UIElements;  // PRIMARY UI TOOLKIT IMPORT
```

### UIDocument Reference
```csharp
[Header("UI References")]
public UIDocument uiDocument;  // ROOT COMPONENT - Key entry point
```

### VisualElement Fields (Containers)
```csharp
// Main containers
private VisualElement mainContainer;
private VisualElement equipmentPanel;
private VisualElement inventoryPanel;
private VisualElement inventoryGrid;
private VisualElement stashGrid;

// Sections
private VisualElement inventorySection;
private VisualElement stashSection;

// Stash tabs
private VisualElement stashTabsContainer;
private VisualElement stashTabs;

// Currency system
private VisualElement currencySection;
private VisualElement currencyGrid;
private VisualElement currencyTabsContainer;

// Equipment slots (10 total)
private VisualElement helmetSlotElement;
private VisualElement amuletSlotElement;
private VisualElement mainHandSlotElement;
private VisualElement bodyArmourSlotElement;
private VisualElement offHandSlotElement;
private VisualElement glovesSlotElement;
private VisualElement leftRingSlotElement;
private VisualElement rightRingSlotElement;
private VisualElement beltSlotElement;
private VisualElement bootsSlotElement;

// Currency tab contents
private VisualElement orbsTabContent;
private VisualElement spiritsTabContent;
private VisualElement sealsTabContent;
private VisualElement fragmentsTabContent;

// Currency grids
private VisualElement orbsCurrencyGrid;
private VisualElement spiritsCurrencyGrid;
private VisualElement sealsCurrencyGrid;
private VisualElement fragmentsCurrencyGrid;

// Effigy system
private VisualElement effigyGridContainer;
private VisualElement effigyStoragePanel;
private VisualElement effigyStorageContent;
private ScrollView effigyStorageScrollView;
```

### Button Fields
```csharp
// Navigation buttons
private Button returnButton;
private Button characterButton;
private Button skillsButton;

// Inventory controls
private Button sortButton;
private Button filterButton;
private Button stashSortButton;
private Button stashFilterButton;

// Stash tabs
private Button addStashTabButton;
private Button renameTabButton;

// Currency tabs (4 buttons)
private Button orbsTabButton;
private Button spiritsTabButton;
private Button sealsTabButton;
private Button fragmentsTabButton;

// Effigy storage
private Button closeEffigyStorageButton;
private Button openEffigyStorageButton;
```

### Lists of UI Elements
```csharp
private List<VisualElement> inventorySlots = new List<VisualElement>();
private List<VisualElement> stashSlots = new List<VisualElement>();
private List<VisualElement> currencySlots = new List<VisualElement>();

private List<VisualElement> orbsSlots = new List<VisualElement>();
private List<VisualElement> spiritsSlots = new List<VisualElement>();
private List<VisualElement> sealsSlots = new List<VisualElement>();
private List<VisualElement> fragmentsSlots = new List<VisualElement>();
```

### UI Toolkit Methods Used

#### Element Query (Q<T>)
```csharp
// Line ~142-180 in InitializeUIReferences()
mainContainer = uiDocument.rootVisualElement.Q<VisualElement>("MainContainer");
equipmentPanel = mainContainer.Q<VisualElement>("EquipmentPanel");
inventoryPanel = mainContainer.Q<VisualElement>("InventoryPanel");
inventoryGrid = mainContainer.Q<VisualElement>("InventoryGrid");
stashGrid = mainContainer.Q<VisualElement>("StashGrid");

// Equipment slots
helmetSlotElement = mainContainer.Q<VisualElement>("HelmetSlot");
amuletSlotElement = mainContainer.Q<VisualElement>("AmuletSlot");
// ... 8 more equipment slots

// Buttons
returnButton = mainContainer.Q<Button>("ReturnButton");
sortButton = mainContainer.Q<Button>("SortButton");
filterButton = mainContainer.Q<Button>("FilterButton");
// ... etc

// Effigy system
effigyGridContainer = mainContainer.Q<VisualElement>("EffigyGridContainer");
effigyStoragePanel = mainContainer.Q<VisualElement>("EffigyStoragePanel");
effigyStorageContent = effigyStoragePanel.Q<VisualElement>("EffigyStorageContent");
effigyStorageScrollView = effigyStoragePanel.Q<ScrollView>("EffigyStorageScrollView");
```

#### Button Event Registration
```csharp
// Line ~196-230
returnButton.clicked += OnReturnButtonClicked;
sortButton.clicked += OnSortInventory;
filterButton.clicked += OnFilterInventory;
stashSortButton.clicked += OnSortStash;
stashFilterButton.clicked += OnFilterStash;
addStashTabButton.clicked += OnAddStashTabClicked;
renameTabButton.clicked += OnRenameTabClicked;

// Currency tabs
orbsTabButton.clicked += () => SetCurrencyTab("Orbs");
spiritsTabButton.clicked += () => SetCurrencyTab("Spirits");
sealsTabButton.clicked += () => SetCurrencyTab("Seals");
fragmentsTabButton.clicked += () => SetCurrencyTab("Fragments");

// Effigy storage
closeEffigyStorageButton.clicked += OnCloseEffigyStorage;
openEffigyStorageButton.clicked += OnOpenEffigyStorage;
```

#### Mouse Event Registration (Callbacks)
```csharp
// Equipment slots - hover tooltips (Line ~765-842)
helmetSlotElement.RegisterCallback<MouseEnterEvent>(evt => ShowEquipmentTooltip(EquipmentType.Helmet, evt.mousePosition));
helmetSlotElement.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());

// Inventory slots (Line ~885-890)
slot.RegisterCallback<ClickEvent>(evt => OnInventorySlotClicked(slotIndex));
slot.RegisterCallback<MouseEnterEvent>(evt => ShowInventoryTooltip(slotIndex, evt.mousePosition));
slot.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());

// Stash slots (Line ~966-970)
slot.RegisterCallback<ClickEvent>(evt => OnStashSlotClicked(slotIndex));
slot.RegisterCallback<MouseEnterEvent>(evt => ShowStashTooltip(slotIndex, evt.mousePosition));
slot.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());

// Effigy storage slots (Line ~473)
slot.RegisterCallback<MouseDownEvent>(evt => OnStorageEffigyClicked(effigy, evt));
```

#### Dynamic Element Creation
```csharp
// Inventory Grid Generation (Line ~846-909)
for (int y = 0; y < inventoryHeight; y++)
{
    for (int x = 0; x < inventoryWidth; x++)
    {
        VisualElement slot = new VisualElement();
        slot.name = $"Slot_{x}_{y}";
        slot.style.width = 60;
        slot.style.height = 60;
        slot.style.marginLeft = 1;
        // ... more styling
        
        gridContainer.Add(slot);
        inventorySlots.Add(slot);
    }
}

// Effigy Storage Slot Creation (Line ~422-476)
VisualElement slot = new VisualElement();
slot.name = $"EffigySlot_{i}";
slot.style.width = 80;
slot.style.height = 80;
slot.style.backgroundColor = elementColor;

Image iconImage = new Image();
iconImage.image = effigy.icon.texture;
slot.Add(iconImage);

Label nameLabel = new Label(effigy.effigyName);
slot.Add(nameLabel);
```

#### Style Manipulation
```csharp
// Inline styles (throughout file)
element.style.width = 60;
element.style.height = 60;
element.style.backgroundColor = new Color(0.12f, 0.14f, 0.16f, 1f);
element.style.borderLeftWidth = 2;
element.style.borderLeftColor = new Color(0.39f, 0.43f, 0.47f, 1f);
element.style.borderTopLeftRadius = 4;
element.style.opacity = 0.9f;
element.style.position = Position.Absolute;
element.style.display = DisplayStyle.None;

// USS Classes (Line ~1080-1090)
stashSlots[i].AddToClassList("occupied");
stashSlots[i].AddToClassList("common");
stashSlots[i].AddToClassList("weapon");
stashSlots[i].RemoveFromClassList("occupied");
```

#### Parent/Child Hierarchy
```csharp
// Adding children
parentContainer.Add(childElement);
slot.Add(iconImage);
slot.Add(nameLabel);

// Clearing
element.Clear();
slot.Clear();

// Removing
element.RemoveFromHierarchy();
```

---

## EffigyGrid.cs - UI Toolkit References

### Namespaces
```csharp
using UnityEngine.UIElements;  // PRIMARY UI TOOLKIT IMPORT
```

### Fields
```csharp
private VisualElement gridContainer;
private List<VisualElement> gridCells = new List<VisualElement>();
private Dictionary<Effigy, List<VisualElement>> effigyVisuals = new Dictionary<Effigy, List<VisualElement>>();
private HashSet<VisualElement> highlightedCells = new HashSet<VisualElement>();
private HashSet<VisualElement> occupiedCellHighlights = new HashSet<VisualElement>();
```

### Key Methods

#### Grid Creation (Line ~36-115)
```csharp
private void CreateGrid(VisualElement parent)
{
    gridContainer = new VisualElement();
    gridContainer.name = "EffigyGrid";
    gridContainer.style.flexDirection = FlexDirection.Column;
    gridContainer.style.width = totalWidth;
    gridContainer.style.height = totalHeight;
    
    for (int y = 0; y < GRID_HEIGHT; y++)
    {
        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            VisualElement cell = new VisualElement();
            cell.name = $"EffigyCell_{x}_{y}";
            cell.style.width = CELL_SIZE;
            cell.style.height = CELL_SIZE;
            
            // Borders
            cell.style.borderLeftWidth = 1;
            cell.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
            cell.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
            
            // Event handlers
            cell.RegisterCallback<MouseDownEvent>(evt => OnCellMouseDown(x, y, evt));
            cell.RegisterCallback<MouseEnterEvent>(evt => OnCellMouseEnter(x, y));
            cell.RegisterCallback<MouseUpEvent>(evt => OnCellMouseUp(x, y, evt));
            cell.RegisterCallback<ClickEvent>(evt => OnCellClick(x, y));
            
            row.Add(cell);
            gridCells.Add(cell);
        }
        
        gridContainer.Add(row);
    }
    
    parent.Add(gridContainer);
}
```

#### Visual Creation (Line ~238-328)
```csharp
private void CreateEffigyVisual(Effigy effigy, int gridX, int gridY)
{
    foreach (Vector2Int cell in occupiedCells)
    {
        VisualElement effigyCell = new VisualElement();
        effigyCell.name = $"Effigy_{effigy.effigyName}_{cell.x}_{cell.y}";
        effigyCell.style.width = Length.Percent(100);
        effigyCell.style.height = Length.Percent(100);
        effigyCell.style.position = Position.Relative;
        effigyCell.style.backgroundColor = finalColor;
        effigyCell.style.opacity = 0.9f;
        
        // Icon
        if (effigy.icon != null)
        {
            effigyCell.style.backgroundImage = new StyleBackground(effigy.icon);
        }
        
        // Label (only on first cell)
        if (cell.x == 0 && cell.y == 0)
        {
            Label nameLabel = new Label(effigy.effigyName);
            nameLabel.style.fontSize = 9;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            effigyCell.Add(nameLabel);
        }
        
        cellElement.Add(effigyCell);
        visuals.Add(effigyCell);
    }
}
```

#### Highlight System (Line ~468-585)
```csharp
private void PreviewPlacement(Effigy effigy, int gridX, int gridY)
{
    // Valid placement - green
    cellElement.style.backgroundColor = new Color(0.2f, 1f, 0.2f, 0.6f);
    cellElement.style.borderLeftColor = new Color(0f, 1f, 0f);
    cellElement.style.borderLeftWidth = 3;
    
    // Invalid placement - red
    cellElement.style.backgroundColor = new Color(1f, 0.2f, 0.2f, 0.6f);
    cellElement.style.borderLeftColor = new Color(1f, 0f, 0f);
}

private void ClearHighlight()
{
    cell.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
    cell.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
    cell.style.borderLeftWidth = 1;
}
```

#### Event Registration (Line ~600-605)
```csharp
public void StartDragFromStorage(Effigy effigy)
{
    gridContainer.RegisterCallback<MouseUpEvent>(OnGlobalMouseUp, TrickleDown.TrickleDown);
    gridContainer.RegisterCallback<ClickEvent>(OnGridClick);
}

// Unregister
gridContainer.UnregisterCallback<MouseUpEvent>(OnGlobalMouseUp, TrickleDown.TrickleDown);
gridContainer.UnregisterCallback<ClickEvent>(OnGridClick);
```

---

## EffigyStorageManager.cs - UI Toolkit References

### Namespaces
```csharp
using UnityEngine.UIElements;  // PRIMARY UI TOOLKIT IMPORT
```

### Animation Methods

#### Slide Animation (Line ~14-52)
```csharp
public static void SlideInPanel(VisualElement panel, float width, float duration = 0.3f)
{
    panel.BringToFront();
    panel.style.position = Position.Absolute;
    panel.style.top = 0;
    panel.style.bottom = 0;
    panel.style.right = 0;
    panel.style.width = width;
    panel.style.display = DisplayStyle.Flex;
    
    // Coroutine animates panel.style.right from 0 to -width
}

public static void SlideOutPanel(VisualElement panel, float width, float duration = 0.3f)
{
    // Coroutine animates panel.style.right from -width to 0
    // Then: panel.style.display = DisplayStyle.None;
}
```

#### Coroutine Animation (Line ~54-92)
```csharp
private static IEnumerator AnimatePanelSlide(VisualElement panel, float startValue, float endValue, float duration)
{
    float elapsed = 0f;
    while (elapsed < duration && panel != null)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        float currentValue = Mathf.Lerp(startValue, endValue, t);
        panel.style.right = currentValue;  // KEY LINE: Animates style property
        yield return null;
    }
}
```

---

## UXML Structure (EquipmentScreen.uxml)

### Root Element
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="EquipmentScreen.uss" />
    <ui:VisualElement name="MainContainer" class="main-container">
```

### Key Elements by Name (for Q<T> queries)

| Element Name | Type | Line | Parent | Purpose |
|-------------|------|------|--------|---------|
| MainContainer | VisualElement | 3 | root | Top-level container |
| HeaderBar | VisualElement | 4 | MainContainer | Header section |
| CentralPillar | VisualElement | 5 | HeaderBar | Decorative element |
| ContentArea | VisualElement | 7 | MainContainer | Main content wrapper |
| EquipmentPanel | VisualElement | 8 | ContentArea | Left panel |
| EquipmentPanelTop | VisualElement | 9 | EquipmentPanel | Top section of equipment |
| EffigyGridContainer | VisualElement | 10 | EquipmentPanelTop | Effigy grid wrapper |
| EquipmentSlots | VisualElement | 13 | EquipmentPanelTop | Equipment slots container |
| HelmetSlot | VisualElement | 15 | TopRow | Helmet equipment |
| AmuletSlot | VisualElement | 18 | TopRow | Amulet equipment |
| MainHandSlot | VisualElement | 23 | MiddleRow | Main hand weapon |
| BodyArmourSlot | VisualElement | 26 | MiddleRow | Body armour |
| OffHandSlot | VisualElement | 29 | MiddleRow | Off hand |
| LeftRingSlot | VisualElement | 34 | RingsContainer | Left ring |
| BeltSlot | VisualElement | 37 | RingsContainer | Belt |
| RightRingSlot | VisualElement | 40 | RingsContainer | Right ring |
| GlovesSlot | VisualElement | 45 | LowerRow | Gloves |
| BootsSlot | VisualElement | 49 | LowerRow | Boots |
| CurrencySection | VisualElement | 55 | EquipmentPanel | Currency panel |
| CurrencyTabsContainer | VisualElement | 57 | CurrencySection | Tab buttons wrapper |
| OrbsTabButton | Button | 58 | CurrencyTabsContainer | Orbs tab |
| SpiritsTabButton | Button | 59 | CurrencyTabsContainer | Spirits tab |
| SealsTabButton | Button | 60 | CurrencyTabsContainer | Seals tab |
| FragmentsTabButton | Button | 61 | CurrencyTabsContainer | Fragments tab |
| OrbsTabContent | VisualElement | 63 | CurrencySection | Orbs grid content |
| SpiritsTabContent | VisualElement | 66 | CurrencySection | Spirits grid content |
| SealsTabContent | VisualElement | 69 | CurrencySection | Seals grid content |
| FragmentsTabContent | VisualElement | 72 | CurrencySection | Fragments grid content |
| OrbsCurrencyGrid | VisualElement | 64 | OrbsTabContent | Orbs grid |
| SpiritsCurrencyGrid | VisualElement | 67 | SpiritsTabContent | Spirits grid |
| SealsCurrencyGrid | VisualElement | 70 | SealsTabContent | Seals grid |
| FragmentsCurrencyGrid | VisualElement | 73 | FragmentsTabContent | Fragments grid |
| InventoryPanel | VisualElement | 77 | ContentArea | Right panel |
| InventorySection | VisualElement | 78 | InventoryPanel | Inventory wrapper |
| InventoryGrid | VisualElement | 80 | InventorySection | Inventory grid |
| InventoryControls | VisualElement | 81 | InventorySection | Buttons wrapper |
| SortButton | Button | 82 | InventoryControls | Sort inventory |
| FilterButton | Button | 83 | InventoryControls | Filter inventory |
| StashSection | VisualElement | 86 | InventoryPanel | Stash wrapper |
| StashTabsContainer | VisualElement | 88 | StashSection | Stash tabs wrapper |
| StashTabs | VisualElement | 89 | StashTabsContainer | Tab buttons container |
| AddStashTabButton | Button | 90 | StashTabsContainer | Add new tab |
| StashGrid | VisualElement | 92 | StashSection | Stash grid |
| StashControls | VisualElement | 93 | StashSection | Stash buttons |
| StashSortButton | Button | 94 | StashControls | Sort stash |
| StashFilterButton | Button | 95 | StashControls | Filter stash |
| RenameTabButton | Button | 96 | StashControls | Rename tab |
| EffigyStoragePanel | VisualElement | 103 | MainContainer | Sliding panel |
| EffigyStorageHeader | VisualElement | 104 | EffigyStoragePanel | Header |
| CloseEffigyStorageButton | Button | 106 | EffigyStorageHeader | Close button |
| EffigyStorageScrollView | ScrollView | 108 | EffigyStoragePanel | Scrollable area |
| EffigyStorageContent | VisualElement | 109 | EffigyStorageScrollView | Content container |
| BottomControls | VisualElement | 115 | MainContainer | Bottom bar |
| ReturnButton | Button | 116 | BottomControls | Return to game |

---

## USS Classes (EquipmentScreen.uss)

### Major Classes Used

| Class Name | Element Count | Purpose |
|-----------|---------------|---------|
| `.main-container` | 1 | Root container styling |
| `.header-bar` | 1 | Header styling |
| `.content-area` | 1 | Main content layout |
| `.equipment-panel` | 1 | Left panel styling |
| `.equipment-slot` | 10 | Equipment slot base style |
| `.inventory-panel` | 1 | Right panel styling |
| `.inventory-grid` | 1 | Grid container |
| `.stash-grid` | 1 | Stash grid container |
| `.inventory-slot` | ~60+ | Individual slots |
| `.effigy-grid-container` | 1 | Effigy grid wrapper |
| `.effigy-storage-panel` | 1 | Sliding panel |
| `.currency-section` | 1 | Currency area |
| `.currency-tab-button` | 4 | Tab buttons |
| `.currency-tab-content` | 4 | Tab content areas |
| `.inventory-button` | 6 | Action buttons |

### Modifiers (applied dynamically)
```csharp
// Rarity classes
.common
.magic
.rare
.unique

// Item type classes
.weapon
.armour
.accessory
.consumable
.material

// State classes
.occupied
.active
.highlighted
```

---

## Summary Statistics

### Total UI Toolkit Usage

| Metric | Count |
|--------|-------|
| Files using UIElements | 3 |
| Total VisualElement fields | ~45 |
| Total Button fields | ~16 |
| Total List<VisualElement> | ~7 |
| Named elements in UXML | ~50 |
| USS classes defined | ~80+ |
| Event registrations | ~100+ |
| Dynamic element creation loops | ~5 major grids |

### Conversion Priorities

**High Priority (Core Functionality):**
1. Equipment Slots (10 slots)
2. Inventory Grid (60 slots)
3. Stash Grid (60 slots)
4. Buttons (16 total)

**Medium Priority (Complex Systems):**
1. Effigy Grid (24 cells + drag/drop)
2. Currency System (4 tabs, 40+ slots)
3. Stash Tabs (dynamic tabs)

**Low Priority (Visual/Polish):**
1. Effigy Storage Panel (sliding animation)
2. Header/Footer layout
3. Tooltips

---

## Next Steps for Migration

1. **Start with simplest elements:** Buttons and labels
2. **Move to static containers:** Equipment slots, headers
3. **Tackle dynamic grids:** Inventory, stash
4. **Complex interactions:** Effigy drag & drop
5. **Polish:** Animations, transitions

---

**End of Reference Document**


