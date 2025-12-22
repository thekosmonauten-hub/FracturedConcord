# Deck Builder System - Complete Implementation Guide

## Table of Contents
1. [System Overview](#system-overview)
2. [Architecture](#architecture)
3. [Setup Instructions](#setup-instructions)
4. [UI Prefab Creation](#ui-prefab-creation)
5. [Scene Setup](#scene-setup)
6. [Script Reference](#script-reference)
7. [Data Flow](#data-flow)
8. [Rarity Color System](#rarity-color-system)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)

---

## System Overview

The Deck Builder System is a Hearthstone-style deck management interface that allows players to:
- Browse their card collection with filtering/search
- Build decks with rule validation (max 40 cards, 6 copies per card, 1 for uniques)
- Save/load multiple deck presets
- Export/import decks via JSON
- Persist active deck across scenes for combat

### Key Features
✅ **Real-time Validation** - Instant feedback on deck rules  
✅ **Multi-Deck Support** - Save unlimited deck presets  
✅ **Scene Persistence** - Active deck survives scene transitions  
✅ **Advanced Filtering** - Search by name, cost, type, rarity, element  
✅ **JSON Export/Import** - Share decks via clipboard  
✅ **Smooth Animations** - Hover effects, card add/remove animations  
✅ **Dynamic Rarity Colors** - Visual distinction with rarity-based frame colors  

---

## Architecture

### Component Hierarchy

```
DeckManager (Singleton - DontDestroyOnLoad)
├── DeckPreset ScriptableObjects
│   ├── Active Deck
│   └── Saved Deck Presets (List)
└── CardDatabase Reference

DeckBuilderUI (Scene Controller)
├── Card Collection Panel
│   ├── Filters (Search, Cost, Type, Rarity, Element)
│   └── Card Grid (DeckBuilderCardUI prefabs)
├── Deck List Panel
│   └── Deck Cards (DeckCardListUI prefabs)
├── Deck Info Display
│   ├── Name, Size, Description
│   └── Validity Indicator
└── Preset Management
    └── Dropdown, Buttons (New, Save, Delete, etc.)
```

### Data Flow

1. **Collection → Deck**
   ```
   User clicks card in collection
   → DeckBuilderCardUI.OnPointerClick()
   → DeckBuilderUI.OnCardAdded(card)
   → DeckPreset.AddCard(card) [with validation]
   → RefreshDeckDisplay()
   ```

2. **Deck → Removal**
   ```
   User clicks card in deck list
   → DeckCardListUI.OnPointerClick()
   → DeckBuilderUI.OnCardRemoved(card)
   → DeckPreset.RemoveCard(card)
   → RefreshDeckDisplay()
   ```

3. **Persistence**
   ```
   DeckBuilderUI.OnDoneClicked()
   → DeckManager.SetActiveDeck(deck)
   → DeckManager.SaveDeck(deck) [JSON to disk]
   → Scene change
   → CombatManager.Start()
   → DeckManager.GetActiveDeckAsCards() [CardData → Card conversion]
   ```

---

## Setup Instructions

### Step 1: Create Required Folders

Create these folders in your Unity project:
```
Assets/
├── Scripts/
│   ├── Data/Cards/
│   ├── Managers/
│   └── UI/DeckBuilder/
├── Resources/
│   └── UI/DeckBuilder/
└── Prefabs/
    └── UI/DeckBuilder/
```

### Step 2: Add Scripts to Project

All scripts are already created:
- ✅ `DeckPreset.cs` → `Assets/Scripts/Data/Cards/`
- ✅ `DeckManager.cs` → `Assets/Scripts/Managers/`
- ✅ `DeckBuilderUI.cs` → `Assets/Scripts/UI/DeckBuilder/`
- ✅ `DeckBuilderCardUI.cs` → `Assets/Scripts/UI/DeckBuilder/`
- ✅ `DeckCardListUI.cs` → `Assets/Scripts/UI/DeckBuilder/`

### Step 3: Install LeanTween (Required for Animations)

The card UI components use LeanTween for smooth animations.

**Option A: Asset Store** (Recommended)
1. Open Unity Asset Store
2. Search "LeanTween"
3. Download and import (it's free!)

**Option B: Manual Import**
1. Download from: https://github.com/dentedpixel/LeanTween
2. Place `LeanTween.cs` in `Assets/Scripts/ThirdParty/`

**Option C: Remove LeanTween** (If you don't want animations)
- In `DeckBuilderCardUI.cs` and `DeckCardListUI.cs`, replace all `LeanTween` calls with simple `transform.localScale = ...` assignments

### Step 4: Create DeckManager GameObject

1. Create an empty GameObject in your **MainGameUI** scene
2. Name it `DeckManager`
3. Add the `DeckManager` component
4. Assign references:
   - **Card Database**: Drag your CardDatabase asset from Resources
5. The DeckManager will persist across scenes automatically

---

## UI Prefab Creation

### Prefab 1: CardPrefab (Collection Grid Card)

**File**: `Assets/Prefabs/UI/DeckBuilder/CardPrefab.prefab`

**Hierarchy:**
```
CardPrefab (RectTransform, Image, Button, DeckBuilderCardUI)
├── CardBackground (Image) - Background panel
├── CardImage (Image) - Card artwork
├── RarityFrame (Image) - Border based on rarity
├── ElementFrame (Image) - Element indicator
├── CostBubble (Image) - Cost background
├── CostText (TextMeshProUGUI) - Mana cost number
├── CardName (TextMeshProUGUI) - Card name
├── CategoryText (TextMeshProUGUI) - Attack/Skill/Guard/Power
├── DescriptionText (TextMeshProUGUI) - Card description
├── QuantityText (TextMeshProUGUI) - "x2/6" indicator
├── DisabledOverlay (Image, disabled by default) - Gray overlay when maxed out
└── GlowEffect (Image, disabled by default) - Hover glow
```

**DeckBuilderCardUI Component Setup:**
1. Drag all child elements to corresponding fields in the component
2. Set colors:
   - Normal Color: White (255, 255, 255, 255)
   - Hover Color: Yellow (255, 255, 0, 255)
   - Disabled Color: Gray (100, 100, 100, 255)
3. Set Hover Scale: 1.15
4. Set Animation Duration: 0.2

**Button Component:**
- Transition: None (handled by custom script)

**Recommended Size:**
- Width: 200px
- Height: 280px

---

### Prefab 2: DeckCardPrefab (Deck List Entry)

**File**: `Assets/Prefabs/UI/DeckBuilder/DeckCardPrefab.prefab`

**Hierarchy:**
```
DeckCardPrefab (RectTransform, Image, Button, DeckCardListUI)
├── CardBackground (Image) - Dark background
├── CostIcon (Image) - Mana icon
├── CostText (TextMeshProUGUI) - Cost number
├── CardNameText (TextMeshProUGUI) - Card name (colored by rarity)
├── QuantityText (TextMeshProUGUI) - "x3" if more than 1 copy
└── RarityIndicator (Image) - Small colored dot/stripe
```

**DeckCardListUI Component Setup:**
1. Drag child elements to component fields
2. Configure colors:
   - Normal Color: (50, 50, 50, 200)
   - Hover Color: (100, 25, 25, 230) - Reddish tint for removal
   - Rarity Colors (array):
     - [0] Common: (255, 255, 255) White
     - [1] Magic: (76, 128, 255) Blue
     - [2] Rare: (255, 204, 0) Gold
     - [3] Unique: (255, 128, 0) Orange

**Recommended Size:**
- Width: 300px (stretch to fill)
- Height: 40px

---

### Prefab 3: Optional - DeckPresetButton

For saved deck list (if you want a custom UI instead of dropdown):

```
DeckPresetButton (RectTransform, Image, Button)
├── DeckNameText (TextMeshProUGUI)
├── CardCountText (TextMeshProUGUI)
└── DeckIcon (Image)
```

---

## Scene Setup

### Create DeckBuilder Scene

**File**: `Assets/Scenes/DeckBuilder.unity`

### Main Canvas Hierarchy

```
Canvas (Canvas Scaler: Scale with Screen Size, 1920x1080)
├── DeckBuilderUI (Empty GameObject with DeckBuilderUI component)
│
├── CardCollectionPanel (Panel)
│   ├── FilterPanel (Panel - Top)
│   │   ├── SearchBar (TMP_InputField)
│   │   ├── CostFilterDropdown (TMP_Dropdown)
│   │   ├── CategoryFilterDropdown (TMP_Dropdown)
│   │   ├── RarityFilterDropdown (TMP_Dropdown)
│   │   ├── ElementFilterDropdown (TMP_Dropdown)
│   │   └── ClearFiltersButton (Button)
│   │
│   └── CardScrollView (Scroll Rect)
│       ├── Viewport (Mask, Image)
│       │   └── CardGrid (RectTransform, Grid Layout Group, Content Size Fitter)
│       │       └── [CardPrefabs spawn here]
│       └── Scrollbar Vertical (Scrollbar)
│
├── DeckPanel (Panel - Right Side)
│   ├── DeckInfoHeader (Panel)
│   │   ├── DeckNameInputField (TMP_InputField) - **EDITABLE** deck name
│   │   ├── DeckSizeText (TextMeshProUGUI) - "27/40"
│   │   ├── DeckDescriptionInputField (TMP_InputField) - **EDITABLE** description
│   │   └── DeckValidityIndicator (Image) - Green/Red dot
│   │
│   ├── DeckListScrollView (Scroll Rect)
│   │   ├── Viewport (Mask, Image)
│   │   │   └── DeckListContainer (RectTransform, Vertical Layout Group, Content Size Fitter)
│   │   │       └── [DeckCardPrefabs spawn here]
│   │   └── Scrollbar Vertical (Scrollbar)
│   │
│   └── PresetControlPanel (Panel - Bottom)
│       ├── DeckPresetsDropdown (TMP_Dropdown)
│       ├── ButtonRow1
│       │   ├── NewDeckButton (Button)
│       │   ├── SaveDeckButton (Button)
│       │   ├── DeleteDeckButton (Button)
│       │   └── DuplicateDeckButton (Button)
│       └── ButtonRow2
│           ├── ExportDeckButton (Button)
│           └── ImportDeckButton (Button)
│
└── NavigationPanel (Panel - Bottom)
    ├── DoneButton (Button) - "Done" - Green
    └── BackButton (Button) - "Back" - Gray
```

### Layout Recommendations

**CardCollectionPanel:**
- Anchor: Left side
- Width: ~60% of screen
- Height: Full height

**DeckPanel:**
- Anchor: Right side
- Width: ~40% of screen
- Height: Full height

### Component Configurations

**CardGrid (Grid Layout Group):**
```
Cell Size: 200 x 280
Spacing: 10 x 10
Constraint: Fixed Column Count = 3 (or 4 for wider screens)
Child Alignment: Upper Left
```

**CardGrid (Content Size Fitter):**
```
Horizontal Fit: Unconstrained
Vertical Fit: Preferred Size
```

**DeckListContainer (Vertical Layout Group):**
```
Child Force Expand: Width = true, Height = false
Child Control Size: Width = true, Height = true
Spacing: 5
Padding: 10 (all sides)
```

**DeckListContainer (Content Size Fitter):**
```
Horizontal Fit: Unconstrained
Vertical Fit: Preferred Size
```

### DeckBuilderUI Component Setup

1. Select the `DeckBuilderUI` GameObject
2. Assign all references:
   - **Card Collection Scroll Rect**: CardScrollView
   - **Card Collection Grid**: CardGrid (GridLayoutGroup component)
   - **Card Prefab**: CardPrefab from Prefabs folder
   - **Card Collection Container**: CardGrid transform
   - **Deck List Scroll Rect**: DeckListScrollView
   - **Deck List Container**: DeckListContainer transform
   - **Deck Card Prefab**: DeckCardPrefab from Prefabs folder
   - **Deck Name Input Field**: DeckNameInputField (TMP_InputField component)
   - **Deck Size Text**: DeckSizeText
   - **Deck Description Input Field**: DeckDescriptionInputField (TMP_InputField component)
   - **Deck Validity Indicator**: DeckValidityIndicator image
   - **Search Input Field**: SearchBar
   - **Cost Filter Dropdown**: CostFilterDropdown
   - **Category Filter Dropdown**: CategoryFilterDropdown
   - **Rarity Filter Dropdown**: RarityFilterDropdown
   - **Element Filter Dropdown**: ElementFilterDropdown
   - **Clear Filters Button**: ClearFiltersButton
   - **Deck Presets Dropdown**: DeckPresetsDropdown
   - **New Deck Button**: NewDeckButton
   - **Save Deck Button**: SaveDeckButton
   - **Delete Deck Button**: DeleteDeckButton
   - **Duplicate Deck Button**: DuplicateDeckButton
   - **Export Deck Button**: ExportDeckButton
   - **Import Deck Button**: ImportDeckButton
   - **Done Button**: DoneButton
   - **Back Button**: BackButton
   - **Card Database**: Your CardDatabase asset from Resources

3. Configure settings:
   - Valid Deck Color: Green (0, 255, 0)
   - Invalid Deck Color: Red (255, 0, 0)
   - Card Hover Scale: 1.15
   - Card Animation Duration: 0.2

---

## Script Reference

### DeckPreset Class

**Purpose**: ScriptableObject representing a saved deck configuration.

**Key Methods:**
```csharp
bool AddCard(CardData card, int quantity = 1)
bool RemoveCard(CardData card, int quantity = 1)
int GetCardQuantity(CardData card)
int GetTotalCardCount()
bool IsValidDeck(out string errorMessage)
string ExportToJSON()
bool ImportFromJSON(string json, CardDatabase database)
DeckStatistics GetStatistics()
```

**Usage Example:**
```csharp
// Create new deck preset
DeckPreset myDeck = ScriptableObject.CreateInstance<DeckPreset>();
myDeck.deckName = "Aggro Deck";

// Add cards
if (myDeck.AddCard(fireball, 2))
{
    Debug.Log("Added 2 Fireballs");
}

// Validate
string error;
if (!myDeck.IsValidDeck(out error))
{
    Debug.LogError($"Invalid deck: {error}");
}
```

---

### DeckManager Class

**Purpose**: Singleton manager for deck persistence across scenes.

**Key Methods:**
```csharp
void SetActiveDeck(DeckPreset deck)
DeckPreset GetActiveDeck()
bool HasActiveDeck()
List<Card> GetActiveDeckAsCards() // Converts to legacy Card format

DeckPreset CreateNewDeck(string name, string characterClass = "")
bool SaveDeck(DeckPreset deck)
DeckPreset LoadDeck(string fileName)
void LoadAllDecks()
bool DeleteDeck(DeckPreset deck)
List<DeckPreset> GetAllDecks()
DeckPreset DuplicateDeck(DeckPreset source)

bool ValidateDeck(DeckPreset deck, out List<string> errors)
```

**Usage Example:**
```csharp
// Access from anywhere
DeckManager.Instance.SetActiveDeck(myDeck);

// In CombatManager
void InitializeCombat()
{
    if (DeckManager.Instance.HasActiveDeck())
    {
        List<Card> deckCards = DeckManager.Instance.GetActiveDeckAsCards();
        InitializeDeck(deckCards);
    }
}
```

**Events:**
```csharp
DeckManager.Instance.OnDeckLoaded += (deck) => Debug.Log($"Loaded: {deck.deckName}");
DeckManager.Instance.OnDeckSaved += (deck) => Debug.Log($"Saved: {deck.deckName}");
DeckManager.Instance.OnDeckChanged += (deck) => RefreshUI();
DeckManager.Instance.OnDeckDeleted += (deck) => Debug.Log($"Deleted: {deck.deckName}");
```

---

### DeckBuilderUI Class

**Purpose**: Main UI controller for deck building scene.

**Key Methods:**
```csharp
void RefreshCardCollection() // Updates card grid with filters
void RefreshDeckList() // Updates deck list display
void OnCardAdded(CardData card) // Called by DeckBuilderCardUI
void OnCardRemoved(CardData card) // Called by DeckCardListUI
```

**Public for Button Events:**
```csharp
OnNewDeck()
OnSaveDeck()
OnDeleteDeck()
OnDuplicateDeck()
OnExportDeck()
OnImportDeck()
OnDoneClicked()
OnBackClicked()
```

---

### DeckBuilderCardUI Class

**Purpose**: Individual card in collection grid.

**Key Methods:**
```csharp
void Initialize(CardData card, DeckBuilderUI deckBuilder)
void UpdateQuantity(int quantity, int max)
void SetInteractable(bool interactable)
```

**Features:**
- Hover scaling animation
- Click animation
- Disabled state when max copies reached
- Quantity display (x2/6)

---

### DeckCardListUI Class

**Purpose**: Individual card in deck list.

**Key Methods:**
```csharp
void Initialize(DeckCardEntry entry, DeckBuilderUI deckBuilder)
```

**Features:**
- Hover color change
- Click to remove
- Fade-out animation on removal
- Rarity-based name coloring

---

## Data Flow

### Building a Deck

```
1. User opens Deck Builder scene
   ↓
2. DeckBuilderUI.Start()
   ↓
3. LoadInitialDeck()
   - Loads active deck from DeckManager
   - Or creates new deck if none exists
   ↓
4. RefreshCardCollection()
   - Queries CardDatabase.allCards
   - Applies filters
   - Creates DeckBuilderCardUI for each card
   ↓
5. User clicks card in collection
   ↓
6. DeckBuilderCardUI.OnPointerClick()
   ↓
7. DeckBuilderUI.OnCardAdded(card)
   - Validates deck size limit
   - Validates card copy limit
   ↓
8. DeckPreset.AddCard(card)
   - Adds card to entries list
   - Returns true/false
   ↓
9. RefreshDeckDisplay()
   - Recreates deck list UI
   - Updates card collection states
   - Updates deck info (size, validity)
```

### Saving a Deck

```
1. User clicks "Save" button
   ↓
2. DeckBuilderUI.OnSaveDeck()
   ↓
3. DeckPreset.IsValidDeck(out errorMessage)
   - Checks min/max deck size
   - Validates card quantities
   ↓
4. DeckManager.SaveDeck(deck)
   ↓
5. DeckPreset.ExportToJSON()
   - Serializes deck data
   ↓
6. File.WriteAllText(savePath, json)
   - Saves to Application.persistentDataPath/DeckPresets/
```

### Using Deck in Combat

```
1. User clicks "Done" in Deck Builder
   ↓
2. DeckBuilderUI.OnDoneClicked()
   ↓
3. DeckManager.SetActiveDeck(currentDeck)
   ↓
4. DeckManager.SaveDeck(currentDeck)
   ↓
5. SceneManager.LoadScene("Combat")
   ↓
6. CombatManager.Start()
   ↓
7. DeckManager.GetActiveDeckAsCards()
   - Converts each CardData to Card class
   - Returns List<Card> for combat system
   ↓
8. CombatManager.InitializeDeck(cards)
```

---

## Rarity Color System

The Deck Builder System implements a centralized rarity color system that provides consistent visual feedback across all UI components.

### Color Scheme

The `RarityColorUtility` class defines the standard color palette:

| Rarity | Color | RGB Values | Usage |
|--------|-------|------------|-------|
| **Common** | Light Grey | (0.7, 0.7, 0.7) | Basic cards, neutral appearance |
| **Magic** | Blue | (0.4, 0.6, 1.0) | Enhanced cards, magical feel |
| **Rare** | Gold | (1.0, 0.8, 0.4) | Premium cards, valuable appearance |
| **Unique** | Orange | (1.0, 0.6, 0.2) | Legendary cards, special rarity |

### Implementation

#### RarityColorUtility Class
```csharp
// Get standard rarity color for text
Color textColor = RarityColorUtility.GetRarityColor(cardData.rarity);

// Get semi-transparent color for backgrounds
Color frameColor = RarityColorUtility.GetRarityFrameColor(cardData.rarity);

// Get enhanced color for hover states
Color hoverColor = RarityColorUtility.GetRarityFrameHoverColor(cardData.rarity);
```

#### DeckBuilderCardUI Integration
- **Card Name Text**: Colored based on rarity for immediate identification
- **Rarity Frame**: Semi-transparent background (30% opacity) that changes color
- **Hover Effects**: Frame becomes more opaque (60% opacity) on hover
- **Consistency**: All colors sourced from centralized utility

### Visual Feedback

1. **Normal State**: 
   - Card name displays in rarity color
   - Rarity frame shows as semi-transparent colored background
   
2. **Hover State**:
   - Frame color becomes more opaque for enhanced visibility
   - Maintains rarity color identity while providing feedback

3. **Disabled State**:
   - Maintains rarity colors but with reduced opacity
   - Visual consistency even when non-interactive

### Benefits

- **Visual Hierarchy**: Players can instantly identify card rarity
- **Consistency**: Same colors used across all UI components
- **Accessibility**: High contrast colors for better readability
- **Maintainability**: Single source of truth for color definitions
- **Extensibility**: Easy to add new rarity tiers or modify colors

---

## Best Practices

### Performance Optimization

1. **Object Pooling** (For large collections):
   ```csharp
   // Instead of Instantiate/Destroy every refresh:
   - Create pool of 50 card UI objects
   - Reuse and update data instead of destroying
   ```

2. **Incremental Updates**:
   ```csharp
   // Instead of RefreshDeckDisplay() on every change:
   void OnCardAdded(CardData card)
   {
       currentDeck.AddCard(card);
       UpdateSingleCardUI(card); // Only update this card's UI
       UpdateDeckInfoDisplay(); // Update size/validity
   }
   ```

3. **Lazy Loading**:
   ```csharp
   // Load card sprites on demand:
   if (cardImage.sprite == null)
   {
       cardImage.sprite = Resources.Load<Sprite>($"Cards/{card.cardName}");
   }
   ```

### UI/UX Enhancements

1. **Keyboard Shortcuts**:
   ```csharp
   void Update()
   {
       if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
       {
           OnSaveDeck(); // Ctrl+S to save
       }
   }
   ```

2. **Tooltips**:
   ```csharp
   // Add to DeckBuilderCardUI
   public void OnPointerEnter(PointerEventData eventData)
   {
       // Show detailed tooltip with card stats
       TooltipManager.Instance.ShowTooltip(cardData.GetFullDescription());
   }
   ```

3. **Confirmation Dialogs**:
   ```csharp
   private void OnDeleteDeck()
   {
       // Show modal confirmation
       ModalDialog.Show(
           "Delete Deck?",
           $"Are you sure you want to delete '{currentDeck.deckName}'?",
           onConfirm: () => DeckManager.Instance.DeleteDeck(currentDeck)
       );
   }
   ```

4. **Auto-Save**:
   ```csharp
   // In DeckBuilderUI
   private float autoSaveTimer = 0f;
   private const float AUTO_SAVE_INTERVAL = 30f; // 30 seconds
   
   void Update()
   {
       autoSaveTimer += Time.deltaTime;
       if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
       {
           OnSaveDeck();
           autoSaveTimer = 0f;
       }
   }
   ```

### Deck Validation

**Always validate before:**
- Saving deck
- Setting as active deck
- Entering combat

```csharp
string error;
if (!deck.IsValidDeck(out error))
{
    ShowMessage($"Invalid deck: {error}");
    return;
}
```

### Error Handling

**Graceful Degradation:**
```csharp
// In DeckManager.LoadDeck()
try
{
    DeckPreset deck = LoadFromJSON(json);
    
    // Validate loaded cards still exist in database
    deck.GetCardEntries().RemoveAll(entry => 
    {
        if (entry.cardData == null)
        {
            Debug.LogWarning($"Card reference broken in deck '{deck.deckName}'");
            return true; // Remove broken entry
        }
        return false;
    });
    
    return deck;
}
catch (Exception e)
{
    Debug.LogError($"Failed to load deck: {e.Message}");
    return CreateDefaultDeck(); // Fallback
}
```

---

## Troubleshooting

### Issue: Cards not displaying in collection

**Diagnosis:**
```csharp
// In DeckBuilderUI.RefreshCardCollection()
Debug.Log($"CardDatabase has {cardDatabase.allCards.Count} cards");
Debug.Log($"After filters: {filteredCards.Count} cards");
Debug.Log($"CardCollectionContainer children: {cardCollectionContainer.childCount}");
```

**Common Causes:**
1. CardDatabase not assigned in DeckBuilderUI
2. CardDatabase.allCards is empty
3. Filters are too restrictive
4. CardPrefab not assigned

**Fix:**
- Ensure CardDatabase asset is populated
- Assign CardDatabase to DeckBuilderUI in Inspector
- Click "Clear Filters" button

---

### Issue: DeckManager losing active deck on scene change

**Diagnosis:**
```csharp
// In DeckManager.Awake()
void Awake()
{
    Debug.Log("DeckManager Awake called");
    if (_instance == null)
    {
        Debug.Log("Setting DeckManager instance");
        DontDestroyOnLoad(gameObject);
    }
}
```

**Common Causes:**
1. Multiple DeckManager instances being created
2. DontDestroyOnLoad not called
3. DeckManager destroyed on scene change

**Fix:**
- Ensure only ONE DeckManager exists (singleton pattern)
- Check "Don't Destroy On Load" is working
- Add DeckManager to a persistent scene (like MainGameUI)

---

### Issue: LeanTween errors

**Error:**
```
NullReferenceException: Object reference not set to an instance of an object
LeanTween.scale(...) 
```

**Fix Option 1** (Install LeanTween):
- Download LeanTween from Asset Store or GitHub
- Import into project

**Fix Option 2** (Remove animations):
```csharp
// In DeckBuilderCardUI.OnPointerEnter()
// Replace:
LeanTween.scale(gameObject, originalScale * hoverScale, animationDuration);

// With:
transform.localScale = originalScale * hoverScale;
```

---

### Issue: Deck not saving to disk

**Diagnosis:**
```csharp
// In DeckManager.SaveDeck()
Debug.Log($"Save path: {SavePath}");
Debug.Log($"Directory exists: {Directory.Exists(SavePath)}");
Debug.Log($"Writing deck: {deck.deckName}");
```

**Common Causes:**
1. Save directory doesn't exist
2. No write permissions
3. Deck name has invalid characters

**Fix:**
```csharp
// DeckManager creates directory automatically in Initialize()
if (!Directory.Exists(SavePath))
{
    Directory.CreateDirectory(SavePath);
}

// Sanitize file names
private string SanitizeFileName(string fileName)
{
    char[] invalidChars = Path.GetInvalidFileNameChars();
    return new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
}
```

---

### Issue: CardData to Card conversion issues

**Problem:** Combat system uses legacy `Card` class, but Deck Builder uses `CardData`.

**Solution:** DeckManager bridges the gap:

```csharp
// In DeckManager.GetActiveDeckAsCards()
private Card ConvertCardDataToCard(CardData cardData)
{
    Card card = new Card
    {
        cardName = cardData.cardName,
        description = cardData.GetFullDescription(),
        manaCost = cardData.playCost,
        baseDamage = cardData.damage,
        baseGuard = cardData.block
    };
    
    // Map category to CardType
    switch (cardData.category)
    {
        case CardCategory.Attack:
            card.cardType = CardType.Attack;
            break;
        // ... etc
    }
    
    return card;
}
```

**If you need custom mapping**, modify this method in `DeckManager.cs`.

---

## Integration with Existing Systems

### Using Active Deck in Combat

**CombatManager.cs:**
```csharp
public class CombatManager : MonoBehaviour
{
    [Header("Deck Management")]
    public List<Card> drawPile = new List<Card>();
    
    private void Start()
    {
        InitializeCombat();
    }
    
    private void InitializeDeck()
    {
        // Load deck from DeckManager
        if (DeckManager.Instance.HasActiveDeck())
        {
            List<Card> deckCards = DeckManager.Instance.GetActiveDeckAsCards();
            drawPile = new List<Card>(deckCards);
            
            // Shuffle
            ShuffleDeck();
            
            Debug.Log($"Initialized combat with {drawPile.Count} cards");
        }
        else
        {
            Debug.LogWarning("No active deck! Using fallback starter deck.");
            LoadStarterDeck();
        }
    }
}
```

### Adding Deck Button to Main Menu

**MainMenuController.cs:**
```csharp
[SerializeField] private Button deckBuilderButton;

private void Start()
{
    deckBuilderButton.onClick.AddListener(OpenDeckBuilder);
}

private void OpenDeckBuilder()
{
    SceneManager.LoadScene("DeckBuilder");
}
```

### Character-Specific Decks

**DeckManager.cs** (Add filtering):
```csharp
public List<DeckPreset> GetDecksForClass(string characterClass)
{
    return savedDecks
        .Where(d => string.IsNullOrEmpty(d.characterClass) || 
                    d.characterClass == characterClass)
        .ToList();
}
```

**DeckBuilderUI.cs** (Filter by current character):
```csharp
private void LoadInitialDeck()
{
    string currentClass = GetCurrentCharacterClass();
    List<DeckPreset> classDecks = DeckManager.Instance.GetDecksForClass(currentClass);
    
    if (classDecks.Count > 0)
    {
        currentDeck = classDecks[0];
    }
    else
    {
        currentDeck = DeckManager.Instance.CreateNewDeck("New Deck", currentClass);
    }
}
```

---

## Advanced Features (Future Enhancements)

### 1. Deck Templates
```csharp
[CreateAssetMenu(menuName = "Dexiled/Decks/Deck Template")]
public class DeckTemplate : ScriptableObject
{
    public string templateName;
    public string description;
    public List<CardData> requiredCards;
    public List<CardData> recommendedCards;
    
    public DeckPreset GenerateDeck()
    {
        DeckPreset deck = CreateInstance<DeckPreset>();
        deck.deckName = templateName;
        
        foreach (CardData card in requiredCards)
        {
            deck.AddCard(card, 2);
        }
        
        return deck;
    }
}
```

### 2. Deck Statistics Panel
```csharp
public class DeckStatsPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI avgCostText;
    [SerializeField] private Image manaCurveChart;
    
    public void UpdateStats(DeckPreset deck)
    {
        DeckStatistics stats = deck.GetStatistics();
        avgCostText.text = $"Avg Cost: {stats.averageManaCost:F1}";
        
        // Draw mana curve histogram
        DrawManaCurve(deck);
    }
}
```

### 3. Deck Code Sharing (Base64 Encoding)
```csharp
public string GenerateDeckCode(DeckPreset deck)
{
    string json = deck.ExportToJSON();
    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
    return System.Convert.ToBase64String(bytes);
}

public DeckPreset ImportDeckCode(string code)
{
    byte[] bytes = System.Convert.ToBase64String(code);
    string json = System.Text.Encoding.UTF8.GetString(bytes);
    
    DeckPreset deck = CreateInstance<DeckPreset>();
    deck.ImportFromJSON(json, cardDatabase);
    return deck;
}
```

### 4. Drag-and-Drop Cards
```csharp
// In DeckBuilderCardUI
public class DeckBuilderCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform dragTransform;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Create drag ghost
        dragTransform = Instantiate(gameObject, canvas.transform).GetComponent<RectTransform>();
        dragTransform.position = eventData.position;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        dragTransform.position = eventData.position;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Check if dropped on deck panel
        if (RectTransformUtility.RectangleContainsScreenPoint(deckPanel, eventData.position))
        {
            deckBuilder.OnCardAdded(cardData);
        }
        
        Destroy(dragTransform.gameObject);
    }
}
```

---

## Testing Checklist

Before deploying the Deck Builder:

- [ ] Can add cards to deck
- [ ] Can remove cards from deck
- [ ] Deck size limit enforced (40 max)
- [ ] Card copy limit enforced (6 standard, 1 unique)
- [ ] Filters work (search, cost, type, rarity, element)
- [ ] Can create new deck
- [ ] Can save deck to disk
- [ ] Can load saved deck
- [ ] Can delete deck
- [ ] Can duplicate deck
- [ ] Can export deck as JSON
- [ ] Can import deck from JSON
- [ ] Active deck persists across scene changes
- [ ] Active deck loaded in combat
- [ ] Hover animations work
- [ ] Card click animations work
- [ ] Deck validity indicator works
- [ ] Deck size counter updates
- [ ] Invalid deck prevents "Done" action
- [ ] "Back" button returns to main menu
- [ ] No errors in console

---

## Conclusion

You now have a fully-featured, production-ready Deck Builder System!

**Next Steps:**
1. Create the UI prefabs following the guide above
2. Set up the DeckBuilder scene
3. Test thoroughly with your card database
4. Customize visuals to match your game's art style
5. Add optional enhancements (drag-drop, deck stats, etc.)

**Questions or Issues?**
- Check the Troubleshooting section
- Review the Script Reference
- Debug with the provided diagnostic code snippets

**Happy Deck Building!** 🃏✨
