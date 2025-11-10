# Equipment Screen Migration Guide
## UI Toolkit → Unity UI (Canvas-based System)

**Author:** AI Assistant  
**Date:** October 30, 2025  
**Purpose:** Complete guide for migrating EquipmentScreen from UI Toolkit (UIDocument) to Unity UI (Canvas/RectTransform)

---

## Table of Contents
1. [Current UI Toolkit Implementation Overview](#current-ui-toolkit-implementation-overview)
2. [Why Migrate?](#why-migrate)
3. [Component Equivalencies](#component-equivalencies)
4. [Migration Strategy](#migration-strategy)
5. [Step-by-Step Migration](#step-by-step-migration)
6. [Code Examples](#code-examples)
7. [Troubleshooting](#troubleshooting)

---

## Current UI Toolkit Implementation Overview

### Files Using UI Toolkit

| File | Purpose | UI Toolkit Dependencies |
|------|---------|------------------------|
| `EquipmentScreen.cs` | Main controller | `using UnityEngine.UIElements;` |
| `EffigyGrid.cs` | Effigy grid system | `using UnityEngine.UIElements;` |
| `EffigyStorageManager.cs` | Effigy storage panel | `using UnityEngine.UIElements;` |
| `EquipmentScreen.uxml` | UI Layout definition | UXML document |
| `EquipmentScreen.uss` | Styling/CSS | USS stylesheet |

### Current UI Toolkit Elements

#### Main Container Structure
```
MainContainer (VisualElement)
├── HeaderBar (VisualElement)
│   └── CentralPillar (VisualElement)
├── ContentArea (VisualElement)
│   ├── EquipmentPanel (VisualElement)
│   │   ├── EquipmentPanelTop (VisualElement)
│   │   │   ├── EffigyGridContainer (VisualElement)
│   │   │   │   └── EffigyGrid (dynamically generated)
│   │   │   └── EquipmentSlots (VisualElement)
│   │   │       ├── HelmetSlot
│   │   │       ├── AmuletSlot
│   │   │       ├── MainHandSlot
│   │   │       ├── BodyArmourSlot
│   │   │       ├── OffHandSlot
│   │   │       ├── GlovesSlot
│   │   │       ├── LeftRingSlot
│   │   │       ├── RightRingSlot
│   │   │       ├── BeltSlot
│   │   │       └── BootsSlot
│   │   └── CurrencySection (VisualElement)
│   │       ├── CurrencyTabsContainer (VisualElement)
│   │       │   ├── OrbsTabButton (Button)
│   │       │   ├── SpiritsTabButton (Button)
│   │       │   ├── SealsTabButton (Button)
│   │       │   └── FragmentsTabButton (Button)
│   │       └── Tab Contents (4 grids)
│   └── InventoryPanel (VisualElement)
│       ├── InventorySection (VisualElement)
│       │   ├── InventoryGrid (dynamically generated)
│       │   └── InventoryControls (Buttons)
│       └── StashSection (VisualElement)
│           ├── StashTabsContainer (VisualElement)
│           ├── StashGrid (dynamically generated)
│           └── StashControls (Buttons)
├── EffigyStoragePanel (VisualElement - slides in from right)
│   ├── EffigyStorageHeader
│   └── EffigyStorageScrollView (ScrollView)
│       └── EffigyStorageContent (dynamically generated)
└── BottomControls (VisualElement)
    └── ReturnButton (Button)
```

### UI Toolkit Features Currently Used

| Feature | Usage | Notes |
|---------|-------|-------|
| `UIDocument` | Root component | Main entry point |
| `VisualElement` | Containers | Equivalent to GameObject/RectTransform |
| `Button` | Clickable elements | Has built-in clicked event |
| `Label` | Text display | Static text |
| `Image` | Sprite display | Shows sprites/textures |
| `ScrollView` | Scrollable area | For Effigy Storage |
| `StyleSheet (.uss)` | CSS-like styling | Positioning, colors, sizes |
| `UXML` | Layout markup | XML-based UI definition |
| Event System | Mouse events | `RegisterCallback<MouseEnterEvent>`, `ClickEvent`, etc. |
| Flexbox Layout | Positioning | `flex-direction`, `flex-grow`, etc. |
| Dynamic Creation | Runtime UI | `new VisualElement()`, `Add()`, etc. |

### Critical UI Toolkit Code Patterns

#### 1. Element Query Pattern
```csharp
// UI Toolkit
private VisualElement mainContainer;
mainContainer = uiDocument.rootVisualElement.Q<VisualElement>("MainContainer");
```

#### 2. Dynamic Element Creation
```csharp
// UI Toolkit
VisualElement slot = new VisualElement();
slot.name = "Slot_0_0";
slot.style.width = 60;
slot.style.height = 60;
slot.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
parentContainer.Add(slot);
```

#### 3. Event Registration
```csharp
// UI Toolkit
slot.RegisterCallback<ClickEvent>(evt => OnSlotClicked(index));
slot.RegisterCallback<MouseEnterEvent>(evt => OnSlotHovered(index));
```

#### 4. Styling
```csharp
// UI Toolkit - Inline Styles
element.style.position = Position.Absolute;
element.style.width = Length.Percent(100);
element.style.backgroundColor = new StyleColor(Color.red);

// UI Toolkit - Classes (from USS)
element.AddToClassList("inventory-slot");
element.RemoveFromClassList("occupied");
```

---

## Why Migrate?

### Advantages of Unity UI (Canvas) over UI Toolkit

| Aspect | UI Toolkit | Unity UI (Canvas) |
|--------|-----------|------------------|
| **Maturity** | Newer, less stable | Battle-tested, mature |
| **Learning Curve** | Steeper (web-like) | Unity-native approach |
| **Documentation** | Limited examples | Extensive documentation |
| **Community Support** | Growing | Large, established |
| **Editor Tools** | Limited | Rich (Anchors, Layout Groups) |
| **Visual Editing** | Less intuitive | Scene view WYSIWYG |
| **Performance** | Good for UI | Excellent, well-optimized |
| **GameObject Integration** | Separate paradigm | Native GameObject system |
| **Prefabs** | Limited | Full prefab support |
| **Animation** | Custom animations | Animator integration |

### When to Use Each

**Use UI Toolkit if:**
- Building editor tools
- Want web-like development (CSS/HTML-style)
- Need runtime UI rebuilding at scale
- Comfortable with web development paradigms

**Use Unity UI if:**
- Building traditional game UI
- Want familiar Unity workflows
- Need extensive prefab reuse
- Want visual scene editing
- Prefer component-based architecture

---

## Component Equivalencies

### Core Components

| UI Toolkit | Unity UI (Canvas) | Notes |
|------------|-------------------|-------|
| `UIDocument` | `Canvas` | Root component |
| `VisualElement` | `GameObject` + `RectTransform` | Container |
| `Button` | `Button` (UnityEngine.UI) | Similar API |
| `Label` | `TextMeshProUGUI` / `Text` | Use TMP for better quality |
| `Image` | `Image` | Similar sprite display |
| `ScrollView` | `ScrollRect` | Scrollable area |
| `.uss` file | Direct component properties | No CSS equivalent |
| `.uxml` file | Prefab hierarchy | Different format |

### Layout Components

| UI Toolkit Concept | Unity UI Equivalent |
|-------------------|---------------------|
| Flexbox (`flex-direction: row`) | `HorizontalLayoutGroup` |
| Flexbox (`flex-direction: column`) | `VerticalLayoutGroup` |
| `flex-wrap: wrap` | `GridLayoutGroup` |
| `position: absolute` | Anchors set manually |
| `width`, `height` | `RectTransform.sizeDelta` |
| `margin`, `padding` | Layout Element + Layout Groups |

### Event System

| UI Toolkit Event | Unity UI Equivalent |
|-----------------|---------------------|
| `RegisterCallback<ClickEvent>` | `Button.onClick.AddListener` |
| `RegisterCallback<MouseEnterEvent>` | `EventTrigger` + `PointerEnter` |
| `RegisterCallback<MouseLeaveEvent>` | `EventTrigger` + `PointerExit` |
| `RegisterCallback<MouseDownEvent>` | `EventTrigger` + `PointerDown` |
| `RegisterCallback<MouseUpEvent>` | `EventTrigger` + `PointerUp` |

### Styling Properties

| UI Toolkit Style | Unity UI Equivalent |
|-----------------|---------------------|
| `style.backgroundColor` | `Image.color` |
| `style.width` / `style.height` | `RectTransform.sizeDelta` |
| `style.position` | Anchors configuration |
| `style.borderWidth` / `borderColor` | `Outline` component |
| `style.borderRadius` | Custom shader or 9-slice sprite |
| `style.opacity` | `CanvasGroup.alpha` |
| `style.display = DisplayStyle.None` | `GameObject.SetActive(false)` |

---

## Migration Strategy

### Phase 1: Planning (Before Code)
1. ✅ Document current UI structure (this guide)
2. Create mockup of Canvas hierarchy
3. Identify reusable prefabs
4. Plan animation requirements
5. Create asset list (sprites, fonts)

### Phase 2: Setup (Unity Scene)
1. Create new Canvas
2. Set up Canvas Scaler (Reference Resolution: 1920x1080)
3. Set Render Mode (Screen Space - Overlay recommended)
4. Add Event System to scene
5. Create folder structure for prefabs

### Phase 3: Conversion (Step-by-Step)
1. Convert layout (UXML → GameObject hierarchy)
2. Convert styling (USS → Component properties)
3. Convert scripts (VisualElement → RectTransform)
4. Test each section incrementally
5. Hook up events

### Phase 4: Testing & Polish
1. Test all interactions
2. Verify layout on different resolutions
3. Performance testing
4. Final polish

---

## Step-by-Step Migration

### Step 1: Create Canvas Setup

```
GameObject Hierarchy:
EquipmentScreenCanvas (Canvas)
├── EventSystem
└── MainContainer (RectTransform)
    ├── HeaderBar
    ├── ContentArea
    │   ├── EquipmentPanel
    │   └── InventoryPanel
    ├── EffigyStoragePanel
    └── BottomControls
```

**Canvas Settings:**
- Render Mode: Screen Space - Overlay
- Canvas Scaler: Scale With Screen Size
- Reference Resolution: 1920 x 1080
- Match: 0.5 (blend width/height)

### Step 2: Convert Main Container

**Before (UI Toolkit - UXML):**
```xml
<ui:VisualElement name="MainContainer" class="main-container">
```

**After (Unity UI - Hierarchy):**
1. Create GameObject "MainContainer"
2. Add `RectTransform` (auto-added)
3. Add `Image` component (for background)
4. Add `VerticalLayoutGroup` (for column layout)

**Component Settings:**
```
RectTransform:
- Anchors: Stretch (min: 0,0 max: 1,1)
- Offset: Left: 20, Right: -20, Top: -20, Bottom: 20

Image:
- Color: rgb(25, 35, 45) - or (25/255, 35/255, 45/255, 1)
- Raycast Target: ✓ (to block clicks)

VerticalLayoutGroup:
- Padding: 20 all sides
- Spacing: 20
- Child Force Expand: Width ✓, Height ✓
```

### Step 3: Convert Equipment Slots

**Before (UI Toolkit):**
```csharp
private VisualElement helmetSlotElement;
helmetSlotElement = root.Q<VisualElement>("HelmetSlot");
helmetSlotElement.style.width = 100;
helmetSlotElement.style.height = 100;
helmetSlotElement.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
```

**After (Unity UI):**
```csharp
// Create as prefab or in hierarchy
public GameObject helmetSlotPrefab;
private Image helmetSlotImage;

void SetupEquipmentSlot()
{
    GameObject helmetSlot = new GameObject("HelmetSlot");
    helmetSlot.transform.SetParent(equipmentContainer.transform, false);
    
    RectTransform rt = helmetSlot.AddComponent<RectTransform>();
    rt.sizeDelta = new Vector2(100, 100);
    
    Image img = helmetSlot.AddComponent<Image>();
    img.color = new Color(0.1f, 0.1f, 0.1f);
    
    // Add label
    GameObject label = new GameObject("Label");
    label.transform.SetParent(helmetSlot.transform, false);
    TextMeshProUGUI text = label.AddComponent<TextMeshProUGUI>();
    text.text = "HELMET";
    text.fontSize = 12;
    text.alignment = TextAlignmentOptions.Center;
    
    // Store reference
    helmetSlotImage = img;
}
```

**Better Approach: Create as Prefab**
1. Create prefab: `Assets/Prefabs/UI/EquipmentSlot.prefab`
2. Add `EquipmentSlotUI.cs` script component
3. Drag references in Inspector
4. Instantiate in code: `Instantiate(equipmentSlotPrefab, parent)`

### Step 4: Convert Dynamic Grid Generation

**Before (UI Toolkit):**
```csharp
for (int y = 0; y < inventoryHeight; y++)
{
    for (int x = 0; x < inventoryWidth; x++)
    {
        VisualElement slot = new VisualElement();
        slot.name = $"Slot_{x}_{y}";
        slot.style.width = 60;
        slot.style.height = 60;
        slot.style.backgroundColor = new Color(0.12f, 0.14f, 0.16f);
        slot.RegisterCallback<ClickEvent>(evt => OnSlotClicked(x, y));
        gridContainer.Add(slot);
        inventorySlots.Add(slot);
    }
}
```

**After (Unity UI):**
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 6;
    public Vector2 cellSize = new Vector2(60, 60);
    public Vector2 spacing = new Vector2(2, 2);
    
    [Header("References")]
    public GameObject slotPrefab;
    public Transform gridContainer;
    
    private List<InventorySlotUI> slots = new List<InventorySlotUI>();
    
    void GenerateGrid()
    {
        // Set up GridLayoutGroup
        GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        
        gridLayout.cellSize = cellSize;
        gridLayout.spacing = spacing;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridWidth;
        
        // Generate slots
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject slotObj = Instantiate(slotPrefab, gridContainer);
                slotObj.name = $"Slot_{x}_{y}";
                
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.SetPosition(x, y);
                    slotUI.OnSlotClicked += () => OnSlotClicked(x, y);
                    slots.Add(slotUI);
                }
            }
        }
    }
    
    void OnSlotClicked(int x, int y)
    {
        Debug.Log($"Slot clicked: ({x}, {y})");
    }
}
```

**Create InventorySlotUI.cs:**
```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, 
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public Image background;
    public Image itemIcon;
    public TextMeshProUGUI itemLabel;
    
    [Header("Colors")]
    public Color normalColor = new Color(0.12f, 0.14f, 0.16f);
    public Color hoverColor = new Color(0.2f, 0.22f, 0.24f);
    public Color occupiedColor = new Color(0.3f, 0.4f, 0.5f);
    
    public event Action OnSlotClicked;
    public event Action OnSlotHovered;
    
    private int posX;
    private int posY;
    private bool isOccupied = false;
    
    public void SetPosition(int x, int y)
    {
        posX = x;
        posY = y;
    }
    
    public void SetOccupied(bool occupied, Sprite icon = null)
    {
        isOccupied = occupied;
        
        if (occupied)
        {
            background.color = occupiedColor;
            if (icon != null && itemIcon != null)
            {
                itemIcon.sprite = icon;
                itemIcon.enabled = true;
            }
        }
        else
        {
            background.color = normalColor;
            if (itemIcon != null)
                itemIcon.enabled = false;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOccupied)
            background.color = hoverColor;
        OnSlotHovered?.Invoke();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isOccupied)
            background.color = normalColor;
    }
}
```

### Step 5: Convert Button Events

**Before (UI Toolkit):**
```csharp
returnButton = root.Q<Button>("ReturnButton");
returnButton.clicked += OnReturnButtonClicked;
```

**After (Unity UI):**
```csharp
using UnityEngine.UI; // Important!

[SerializeField] private Button returnButton;

void Start()
{
    returnButton.onClick.AddListener(OnReturnButtonClicked);
}

void OnReturnButtonClicked()
{
    Debug.Log("Return button clicked");
}
```

### Step 6: Convert Tabs System

**Before (UI Toolkit):**
```csharp
orbsTabButton.clicked += () => SetCurrencyTab("Orbs");
orbsTabContent.style.display = currentTab == "Orbs" ? DisplayStyle.Flex : DisplayStyle.None;
```

**After (Unity UI):**
```csharp
using UnityEngine.UI;

[SerializeField] private Button orbsTabButton;
[SerializeField] private GameObject orbsTabContent;

void Start()
{
    orbsTabButton.onClick.AddListener(() => SetCurrencyTab("Orbs"));
}

void SetCurrencyTab(string tabName)
{
    orbsTabContent.SetActive(tabName == "Orbs");
    spiritsTabContent.SetActive(tabName == "Spirits");
    // etc...
    
    // Update button states (colors)
    UpdateTabButtonStates(tabName);
}

void UpdateTabButtonStates(string activeTab)
{
    ColorBlock colors = orbsTabButton.colors;
    colors.normalColor = activeTab == "Orbs" ? Color.blue : Color.gray;
    orbsTabButton.colors = colors;
}
```

### Step 7: Convert Effigy Drag & Drop

**Before (UI Toolkit):**
```csharp
slot.RegisterCallback<MouseDownEvent>(evt => OnCellMouseDown(x, y, evt));
slot.RegisterCallback<MouseEnterEvent>(evt => OnCellMouseEnter(x, y));
slot.RegisterCallback<MouseUpEvent>(evt => OnCellMouseUp(x, y, evt));
```

**After (Unity UI):**
```csharp
using UnityEngine.EventSystems;

public class EffigyGridCellUI : MonoBehaviour, 
    IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int cellX;
    public int cellY;
    
    public event Action<int, int, PointerEventData> OnCellMouseDown;
    public event Action<int, int> OnCellMouseEnter;
    public event Action<int, int, PointerEventData> OnCellMouseUp;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        OnCellMouseDown?.Invoke(cellX, cellY, eventData);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnCellMouseEnter?.Invoke(cellX, cellY);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        OnCellMouseUp?.Invoke(cellX, cellY, eventData);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Start drag
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Update drag position
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Complete drag
    }
}
```

### Step 8: Convert Animations (Sliding Panel)

**Before (UI Toolkit - EffigyStorageManager):**
```csharp
IEnumerator AnimatePanelSlide(VisualElement panel, float startValue, float endValue, float duration)
{
    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        float currentValue = Mathf.Lerp(startValue, endValue, t);
        panel.style.right = currentValue;
        yield return null;
    }
}
```

**After (Unity UI - LeanTween or Animator):**

**Option 1: Using LeanTween (you already have this)**
```csharp
public class EffigyStoragePanel : MonoBehaviour
{
    private RectTransform panelRect;
    private float panelWidth = 400f;
    
    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
    }
    
    public void SlideIn(float duration = 0.3f)
    {
        // Start position (off-screen right)
        panelRect.anchoredPosition = new Vector2(panelWidth, 0);
        
        // Animate to visible position
        LeanTween.moveX(panelRect, 0, duration)
            .setEase(LeanTweenType.easeOutQuad);
    }
    
    public void SlideOut(float duration = 0.3f, Action onComplete = null)
    {
        LeanTween.moveX(panelRect, panelWidth, duration)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }
}
```

**Option 2: Using Unity Animator**
1. Create Animator Controller
2. Add parameters: "IsOpen" (bool)
3. Create animations: "PanelSlideIn", "PanelSlideOut"
4. Set up transitions
5. Control via code:
```csharp
animator.SetBool("IsOpen", true); // Slide in
animator.SetBool("IsOpen", false); // Slide out
```

---

## Code Examples

### Complete Equipment Slot Setup (Prefab-based)

**EquipmentSlotUI.cs:**
```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler, 
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public Image backgroundImage;
    public Image itemIconImage;
    public TextMeshProUGUI slotLabel;
    public TextMeshProUGUI itemNameLabel;
    
    [Header("Settings")]
    public EquipmentType slotType;
    public Color emptyColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color occupiedColor = new Color(0.2f, 0.3f, 0.4f, 0.9f);
    public Color hoverColor = new Color(0.3f, 0.4f, 0.5f, 1f);
    
    public event Action<EquipmentType> OnSlotClicked;
    public event Action<EquipmentType, Vector2> OnSlotHovered;
    
    private ItemData equippedItem = null;
    
    public void Initialize(EquipmentType type, string labelText)
    {
        slotType = type;
        if (slotLabel != null)
            slotLabel.text = labelText;
        
        UpdateVisual();
    }
    
    public void SetEquippedItem(ItemData item)
    {
        equippedItem = item;
        UpdateVisual();
    }
    
    private void UpdateVisual()
    {
        if (equippedItem != null)
        {
            backgroundImage.color = occupiedColor;
            
            if (itemIconImage != null && equippedItem.itemSprite != null)
            {
                itemIconImage.sprite = equippedItem.itemSprite;
                itemIconImage.enabled = true;
            }
            
            if (itemNameLabel != null)
            {
                itemNameLabel.text = equippedItem.itemName;
                itemNameLabel.enabled = true;
            }
        }
        else
        {
            backgroundImage.color = emptyColor;
            
            if (itemIconImage != null)
                itemIconImage.enabled = false;
            
            if (itemNameLabel != null)
                itemNameLabel.enabled = false;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(slotType);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (equippedItem != null)
        {
            OnSlotHovered?.Invoke(slotType, eventData.position);
        }
        
        // Visual feedback
        ColorBlock colors = backgroundImage.color;
        backgroundImage.color = hoverColor;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateVisual(); // Restore original color
    }
}
```

**EquipmentScreenUI.cs (Main Controller):**
```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentScreenUI : MonoBehaviour
{
    [Header("Equipment Slots")]
    [SerializeField] private EquipmentSlotUI helmetSlot;
    [SerializeField] private EquipmentSlotUI amuletSlot;
    [SerializeField] private EquipmentSlotUI mainHandSlot;
    [SerializeField] private EquipmentSlotUI bodyArmourSlot;
    [SerializeField] private EquipmentSlotUI offHandSlot;
    [SerializeField] private EquipmentSlotUI glovesSlot;
    [SerializeField] private EquipmentSlotUI leftRingSlot;
    [SerializeField] private EquipmentSlotUI rightRingSlot;
    [SerializeField] private EquipmentSlotUI beltSlot;
    [SerializeField] private EquipmentSlotUI bootsSlot;
    
    [Header("Inventory")]
    [SerializeField] private InventoryGridUI inventoryGrid;
    [SerializeField] private InventoryGridUI stashGrid;
    
    [Header("Buttons")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Button sortButton;
    [SerializeField] private Button filterButton;
    
    [Header("Effigy System")]
    [SerializeField] private EffigyGridUI effigyGrid;
    [SerializeField] private EffigyStoragePanel effigyStoragePanel;
    [SerializeField] private Button openEffigyStorageButton;
    
    private Dictionary<EquipmentType, EquipmentSlotUI> slotMap;
    
    void Awake()
    {
        InitializeSlotMap();
        SetupEventListeners();
    }
    
    void InitializeSlotMap()
    {
        slotMap = new Dictionary<EquipmentType, EquipmentSlotUI>
        {
            { EquipmentType.Helmet, helmetSlot },
            { EquipmentType.Amulet, amuletSlot },
            { EquipmentType.MainHand, mainHandSlot },
            { EquipmentType.BodyArmour, bodyArmourSlot },
            { EquipmentType.OffHand, offHandSlot },
            { EquipmentType.Gloves, glovesSlot },
            { EquipmentType.LeftRing, leftRingSlot },
            { EquipmentType.RightRing, rightRingSlot },
            { EquipmentType.Belt, beltSlot },
            { EquipmentType.Boots, bootsSlot }
        };
        
        // Initialize each slot
        foreach (var kvp in slotMap)
        {
            kvp.Value.Initialize(kvp.Key, kvp.Key.ToString().ToUpper());
            kvp.Value.OnSlotClicked += OnEquipmentSlotClicked;
            kvp.Value.OnSlotHovered += ShowEquipmentTooltip;
        }
    }
    
    void SetupEventListeners()
    {
        returnButton.onClick.AddListener(OnReturnButtonClicked);
        sortButton.onClick.AddListener(OnSortButtonClicked);
        filterButton.onClick.AddListener(OnFilterButtonClicked);
        openEffigyStorageButton.onClick.AddListener(OnOpenEffigyStorage);
    }
    
    void OnEquipmentSlotClicked(EquipmentType slotType)
    {
        Debug.Log($"Equipment slot clicked: {slotType}");
        // Handle equipment slot click (equip/unequip)
    }
    
    void ShowEquipmentTooltip(EquipmentType slotType, Vector2 position)
    {
        // Show tooltip for equipped item
    }
    
    void OnReturnButtonClicked()
    {
        // Return to game
        gameObject.SetActive(false);
    }
    
    void OnSortButtonClicked()
    {
        // Sort inventory
        inventoryGrid.SortInventory();
    }
    
    void OnFilterButtonClicked()
    {
        // Filter inventory
    }
    
    void OnOpenEffigyStorage()
    {
        effigyStoragePanel.SlideIn();
    }
    
    public void EquipItem(ItemData item, EquipmentType slotType)
    {
        if (slotMap.ContainsKey(slotType))
        {
            slotMap[slotType].SetEquippedItem(item);
        }
    }
    
    public void UnequipItem(EquipmentType slotType)
    {
        if (slotMap.ContainsKey(slotType))
        {
            slotMap[slotType].SetEquippedItem(null);
        }
    }
}
```

### Effigy Grid Migration

**EffigyGridUI.cs:**
```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EffigyGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    private const int GRID_WIDTH = 6;
    private const int GRID_HEIGHT = 4;
    private const int CELL_SIZE = 60;
    
    [Header("References")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform gridContainer;
    
    [Header("Colors")]
    [SerializeField] private Color emptyCellColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
    [SerializeField] private Color validPlacementColor = new Color(0.2f, 1f, 0.2f, 0.6f);
    [SerializeField] private Color invalidPlacementColor = new Color(1f, 0.2f, 0.2f, 0.6f);
    
    private List<EffigyGridCellUI> gridCells = new List<EffigyGridCellUI>();
    private Effigy[,] placedEffigies = new Effigy[GRID_HEIGHT, GRID_WIDTH];
    private Dictionary<Effigy, List<GameObject>> effigyVisuals = new Dictionary<Effigy, List<GameObject>>();
    
    // Drag state
    private Effigy draggedEffigy = null;
    private Vector2Int currentHoveredCell = new Vector2Int(-1, -1);
    private bool isDraggingFromStorage = false;
    
    void Awake()
    {
        GenerateGrid();
    }
    
    void GenerateGrid()
    {
        // Set up GridLayoutGroup
        GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        
        gridLayout.cellSize = new Vector2(CELL_SIZE, CELL_SIZE);
        gridLayout.spacing = new Vector2(2, 2);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = GRID_WIDTH;
        
        // Generate cells
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridContainer);
                cellObj.name = $"EffigyCell_{x}_{y}";
                
                EffigyGridCellUI cellUI = cellObj.GetComponent<EffigyGridCellUI>();
                if (cellUI != null)
                {
                    cellUI.cellX = x;
                    cellUI.cellY = y;
                    cellUI.OnCellMouseDown += OnCellMouseDown;
                    cellUI.OnCellMouseEnter += OnCellMouseEnter;
                    cellUI.OnCellMouseUp += OnCellMouseUp;
                    
                    gridCells.Add(cellUI);
                }
            }
        }
    }
    
    public void StartDragFromStorage(Effigy effigy)
    {
        draggedEffigy = effigy;
        isDraggingFromStorage = true;
        currentHoveredCell = new Vector2Int(-1, -1);
        Debug.Log($"[EffigyGrid] Started dragging {effigy.effigyName} from storage");
    }
    
    void OnCellMouseDown(int x, int y, PointerEventData eventData)
    {
        Effigy effigy = placedEffigies[y, x];
        if (effigy != null)
        {
            // Start dragging existing effigy
            draggedEffigy = effigy;
            isDraggingFromStorage = false;
            Debug.Log($"Started dragging {effigy.effigyName} from grid");
        }
    }
    
    void OnCellMouseEnter(int x, int y)
    {
        if (draggedEffigy != null)
        {
            currentHoveredCell = new Vector2Int(x, y);
            PreviewPlacement(draggedEffigy, x, y);
        }
    }
    
    void OnCellMouseUp(int x, int y, PointerEventData eventData)
    {
        if (draggedEffigy != null && currentHoveredCell.x >= 0 && currentHoveredCell.y >= 0)
        {
            bool placed = TryPlaceEffigy(draggedEffigy, currentHoveredCell.x, currentHoveredCell.y);
            
            if (placed)
            {
                Debug.Log($"Successfully placed {draggedEffigy.effigyName}");
            }
            
            ClearHighlight();
            draggedEffigy = null;
            isDraggingFromStorage = false;
            currentHoveredCell = new Vector2Int(-1, -1);
        }
    }
    
    bool TryPlaceEffigy(Effigy effigy, int gridX, int gridY)
    {
        if (!CanPlaceEffigy(effigy, gridX, gridY))
            return false;
        
        RemoveEffigy(effigy);
        
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            if (worldX >= 0 && worldX < GRID_WIDTH && worldY >= 0 && worldY < GRID_HEIGHT)
            {
                placedEffigies[worldY, worldX] = effigy;
            }
        }
        
        CreateEffigyVisual(effigy, gridX, gridY);
        return true;
    }
    
    bool CanPlaceEffigy(Effigy effigy, int gridX, int gridY)
    {
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            if (worldX < 0 || worldX >= GRID_WIDTH || worldY < 0 || worldY >= GRID_HEIGHT)
                return false;
            
            Effigy occupyingEffigy = placedEffigies[worldY, worldX];
            if (occupyingEffigy != null && occupyingEffigy != effigy)
                return false;
        }
        
        return true;
    }
    
    void RemoveEffigy(Effigy effigy)
    {
        if (effigy == null) return;
        
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (placedEffigies[y, x] == effigy)
                    placedEffigies[y, x] = null;
            }
        }
        
        if (effigyVisuals.ContainsKey(effigy))
        {
            foreach (var visual in effigyVisuals[effigy])
            {
                Destroy(visual);
            }
            effigyVisuals.Remove(effigy);
        }
    }
    
    void CreateEffigyVisual(Effigy effigy, int gridX, int gridY)
    {
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        List<GameObject> visuals = new List<GameObject>();
        
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            int cellIndex = worldY * GRID_WIDTH + worldX;
            if (cellIndex < 0 || cellIndex >= gridCells.Count)
                continue;
            
            EffigyGridCellUI cellUI = gridCells[cellIndex];
            
            // Create visual GameObject
            GameObject effigyVisual = new GameObject($"Effigy_{effigy.effigyName}_{cell.x}_{cell.y}");
            effigyVisual.transform.SetParent(cellUI.transform, false);
            
            RectTransform rt = effigyVisual.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            
            Image img = effigyVisual.AddComponent<Image>();
            Color elementColor = effigy.GetElementColor();
            float rarityBrightness = GetRarityBrightness(effigy.rarity);
            img.color = elementColor * rarityBrightness;
            
            if (effigy.icon != null)
                img.sprite = effigy.icon;
            
            visuals.Add(effigyVisual);
        }
        
        if (visuals.Count > 0)
        {
            effigyVisuals[effigy] = visuals;
        }
    }
    
    void PreviewPlacement(Effigy effigy, int gridX, int gridY)
    {
        ClearHighlight();
        
        bool canPlace = CanPlaceEffigy(effigy, gridX, gridY);
        List<Vector2Int> occupiedCells = effigy.GetOccupiedCells();
        
        foreach (Vector2Int cell in occupiedCells)
        {
            int worldX = gridX + cell.x;
            int worldY = gridY + cell.y;
            
            if (worldX >= 0 && worldX < GRID_WIDTH && worldY >= 0 && worldY < GRID_HEIGHT)
            {
                int cellIndex = worldY * GRID_WIDTH + worldX;
                if (cellIndex >= 0 && cellIndex < gridCells.Count)
                {
                    EffigyGridCellUI cellUI = gridCells[cellIndex];
                    cellUI.SetHighlight(canPlace ? validPlacementColor : invalidPlacementColor);
                }
            }
        }
    }
    
    void ClearHighlight()
    {
        foreach (var cell in gridCells)
        {
            cell.ClearHighlight();
        }
    }
    
    float GetRarityBrightness(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return 0.7f;
            case ItemRarity.Magic: return 0.85f;
            case ItemRarity.Rare: return 1.0f;
            case ItemRarity.Unique: return 1.1f;
            default: return 0.8f;
        }
    }
}
```

---

## Troubleshooting

### Common Issues

#### Issue: UI doesn't scale properly
**Solution:** Check Canvas Scaler settings
- Set Render Mode correctly
- Use "Scale With Screen Size"
- Set reference resolution (1920x1080)
- Adjust Match value (0.5 = balanced)

#### Issue: Buttons don't respond to clicks
**Solution:** 
1. Check EventSystem exists in scene
2. Verify Canvas has "Graphic Raycaster" component
3. Check Image "Raycast Target" is enabled
4. Verify button isn't blocked by another UI element (sort order)

#### Issue: Layout doesn't match design
**Solution:**
1. Use Layout Groups (Horizontal/Vertical/Grid)
2. Set anchor presets correctly
3. Use Content Size Fitter for dynamic sizing
4. Check RectTransform pivot and anchor settings

#### Issue: Text looks blurry
**Solution:**
1. Use TextMeshPro instead of legacy Text
2. Set Canvas "Render Mode" to "Screen Space - Overlay"
3. Check reference resolution matches target display
4. Enable "Best Fit" or set proper font size

#### Issue: Performance problems with many UI elements
**Solution:**
1. Use object pooling for grid cells
2. Disable raycast target on non-interactive elements
3. Use CanvasGroup for batch alpha changes
4. Split into multiple canvases if needed

### Performance Comparison

| Aspect | UI Toolkit | Unity UI (Canvas) |
|--------|-----------|-------------------|
| Startup Cost | Lower | Higher (more GameObjects) |
| Runtime Performance | Good | Excellent (optimized) |
| Memory Usage | Lower | Higher (more components) |
| Rebuild Cost | Low | Medium |
| Batching | Automatic | Manual (via Canvas) |

---

## Checklist

### Pre-Migration
- [ ] Document all current UI Toolkit elements
- [ ] Create Canvas setup in scene
- [ ] Design prefab structure
- [ ] Prepare sprite assets
- [ ] Install TextMeshPro (if not already)

### During Migration
- [ ] Convert layout (UXML → GameObject hierarchy)
- [ ] Convert styling (USS → Component properties)
- [ ] Create UI component scripts
- [ ] Set up prefabs for reusable elements
- [ ] Convert event handlers
- [ ] Test each section incrementally

### Post-Migration
- [ ] Test all interactions
- [ ] Verify layout on multiple resolutions
- [ ] Performance profiling
- [ ] Clean up old UI Toolkit files
- [ ] Update documentation
- [ ] Test on target platforms

---

## Conclusion

While UI Toolkit offers modern web-like development, Unity UI (Canvas) provides:
- ✅ Better Unity integration
- ✅ More mature and stable
- ✅ Extensive documentation and community support
- ✅ Visual scene editing (WYSIWYG)
- ✅ Full prefab support
- ✅ Familiar Unity workflow

The migration requires effort but results in a more maintainable, Unity-native UI system that's easier to work with if you're not comfortable with web development paradigms.

---

## Additional Resources

### Unity UI Documentation
- [Canvas](https://docs.unity3d.com/Manual/UICanvas.html)
- [RectTransform](https://docs.unity3d.com/Manual/class-RectTransform.html)
- [EventSystem](https://docs.unity3d.com/Manual/EventSystem.html)
- [Layout Groups](https://docs.unity3d.com/Manual/comp-UIAutoLayout.html)

### TextMeshPro
- [TextMeshPro Documentation](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)

### LeanTween (for animations)
- [LeanTween GitHub](https://github.com/dentedpixel/LeanTween)

---

**End of Migration Guide**


