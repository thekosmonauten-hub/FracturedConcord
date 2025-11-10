# Character Creation UI Toolkit → Canvas Migration Guide

## Overview
Migrating CharacterCreation scene from UI Toolkit to Canvas-based UI while preserving all character creation logic and starter deck preview functionality.

---

## Current Structure (UI Toolkit)
- Character Name input field
- 6 Class selection buttons (Witch, Marauder, Ranger, Thief, Apostle, Brawler)
- Starter Deck preview (scrollable grid with card hover)
- Back button (to MainMenu)
- Create Character button

---

## ⚠️ CRITICAL: What to Keep vs Replace

### ✅ KEEP (Don't Touch):
- **StarterDeckManager** GameObject & script
- **CharacterManager** GameObject & script
- **CharacterSaveSystem** GameObject & script
- **TransitionManager** GameObject & script (if present)
- **EventSystem** GameObject
- **Main Camera**

### ❌ REMOVE/DISABLE:
- **UIDocument** component on Canvas
- UXML/USS files (can keep for reference)

### 🔄 REPLACE:
- **CharacterCreationController.cs** script (needs rewrite for Canvas)
- UI structure (UXML → Canvas hierarchy)

---

## Part 1: Scene Canvas Setup (10 minutes)

### 1.1: Create New Canvas Structure

```
CharacterCreationCanvas (Canvas)
├─ Background (RawImage or Image)
├─ ContentPanel (Empty with Vertical Layout)
│   ├─ NameSection (Vertical Layout)
│   │   ├─ NameLabel (TextMeshProUGUI)
│   │   └─ NameInputField (TMP_InputField)
│   ├─ ClassSection (Vertical Layout)
│   │   ├─ ClassLabel (TextMeshProUGUI)
│   │   └─ ClassGrid (Grid Layout Group)
│   │       ├─ WitchButton
│   │       ├─ MarauderButton
│   │       ├─ RangerButton
│   │       ├─ ThiefButton
│   │       ├─ ApostleButton
│   │       └─ BrawlerButton
│   ├─ DeckPreviewSection (Vertical Layout)
│   │   ├─ DeckPreviewLabel (TextMeshProUGUI)
│   │   └─ DeckPreviewScrollView (Scroll Rect)
│   │       └─ Viewport
│   │           └─ DeckCardGrid (Grid Layout Group)
│   └─ ActionButtons (Horizontal Layout)
│       ├─ BackButton
│       └─ CreateCharacterButton
└─ CardHoverPreview (Empty - parent for hover card)
```

### 1.2: Canvas Settings
- **Render Mode**: Screen Space - Overlay
- **Canvas Scaler**:
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1920 x 1080
  - Match: 0 (or 0.5 for balanced)
- **Graphic Raycaster**: Enabled

---

## Part 2: Background Setup (2 minutes)

### 2.1: Background
1. Right-click **CharacterCreationCanvas** → **UI** → **Raw Image** (for video) or **Image** (for static)
2. Rename to **Background**
3. **RectTransform**:
   - Anchors: Stretch/Stretch
   - All offsets: 0
4. **Raw Image** (if video):
   - Texture: Your RenderTexture
   - Color: White
5. **Image** (if static):
   - Source Image: Your background sprite
   - Image Type: Simple

---

## Part 3: Content Panel (15 minutes)

### 3.1: ContentPanel Setup
1. Create Empty → Rename to **ContentPanel**
2. **RectTransform**:
   - Anchors: Center
   - Width: 1000
   - Height: 800
3. Add **Vertical Layout Group**:
   - Spacing: 30
   - Child Alignment: Upper Center
   - Control Child Size: ❌ Both unchecked
   - Padding: 40 (all sides)

---

### 3.2: Name Section

#### NameSection Container
1. Create Empty under ContentPanel → Rename to **NameSection**
2. Add **Vertical Layout Group**:
   - Spacing: 10
   - Child Alignment: Upper Left
3. **Layout Element**:
   - Preferred Height: 100

#### NameLabel
1. Right-click NameSection → **UI** → **TextMeshProUGUI**
2. Rename to **NameLabel**
3. Text: "Character Name"
4. Font Size: 24
5. Font: Roboto-Bold
6. Color: White
7. Alignment: Left

#### NameInputField
1. Right-click NameSection → **UI** → **InputField - TextMeshPro**
2. Rename to **NameInputField**
3. **TMP_InputField** component:
   - Character Limit: 20
   - Placeholder Text: "Enter Character Name"
4. **RectTransform**:
   - Height: 50
5. Style the background (Image component):
   - Color: Semi-transparent dark
   - Add border if desired

---

### 3.3: Class Section

#### ClassSection Container
1. Create Empty under ContentPanel → Rename to **ClassSection**
2. Add **Vertical Layout Group**:
   - Spacing: 15
   - Child Alignment: Upper Center
3. **Layout Element**:
   - Flexible Height: 1

#### ClassLabel
1. Right-click ClassSection → **UI** → **TextMeshProUGUI**
2. Rename to **ClassLabel**
3. Text: "Choose Your Class"
4. Font Size: 28
5. Font: Roboto-Bold or Oswald-Bold
6. Color: White
7. Alignment: Center

#### ClassGrid
1. Right-click ClassSection → **UI** → **Empty**
2. Rename to **ClassGrid**
3. Add **Grid Layout Group**:
   - Cell Size: (280, 120) or adjust to fit
   - Spacing: (15, 15)
   - Start Corner: Upper Left
   - Start Axis: Horizontal
   - Child Alignment: Upper Center
   - Constraint: Fixed Column Count
   - Constraint Count: 2 (2 columns = 3 rows of classes)
4. **RectTransform**:
   - Height: Auto (let Grid Layout control it)
   - Or set explicit height: 400

---

### 3.4: Class Buttons (x6)

Create 6 class buttons inside ClassGrid. For each button:

#### Structure for Each Class Button:
```
WitchButton (Button)
├─ BackgroundImage (Image - optional)
├─ ClassNameText (TextMeshProUGUI)
└─ ClassDescriptionText (TextMeshProUGUI)
```

**Or simpler structure:**
```
WitchButton (Button + Image)
└─ ButtonText (TextMeshProUGUI)
    ├─ ClassName: "WITCH"
    ├─ ClassDesc: "Intelligence-based spellcaster"
```

#### Button Setup (repeat for all 6):
1. Right-click **ClassGrid** → **UI** → **Button**
2. Rename: **WitchButton**, **MarauderButton**, **RangerButton**, **ThiefButton**, **ApostleButton**, **BrawlerButton**
3. **Button** component:
   - Interactable: ✅
   - Transition: Color Tint
4. **Image** component:
   - Source Image: Your class button background
   - Color: Semi-transparent or styled
5. Add **Layout Element**:
   - Ignore Layout: ❌ (let Grid Layout control size)

#### Button Text (for each):
1. Right-click button → **UI** → **TextMeshProUGUI**
2. Rename to **ClassNameText**
3. Text: "WITCH", "MARAUDER", etc.
4. Font Size: 20-24
5. Font: Roboto-Bold
6. Color: White
7. Alignment: Center
8. **RectTransform**: Anchors Stretch/Stretch, all offsets 0

#### Optional: Class Description Text
1. Add another TextMeshProUGUI below ClassNameText
2. Text: "Intelligence-based spellcaster", etc.
3. Font Size: 12-14
4. Color: Light gray

**Class Button Quick Reference:**
- **Witch**: "Intelligence-based spellcaster"
- **Marauder**: "Strength-based warrior"
- **Ranger**: "Dexterity-based archer"
- **Thief**: "Dexterity/Intelligence hybrid"
- **Apostle**: "Strength/Intelligence hybrid"
- **Brawler**: "Strength/Dexterity hybrid"

---

### 3.5: Deck Preview Section

#### DeckPreviewSection Container
1. Create Empty under ContentPanel → Rename to **DeckPreviewSection**
2. Add **Vertical Layout Group**:
   - Spacing: 10
   - Child Alignment: Upper Center
3. **Layout Element**:
   - Flexible Height: 1
   - Min Height: 200

#### DeckPreviewLabel
1. Right-click DeckPreviewSection → **UI** → **TextMeshProUGUI**
2. Rename to **DeckPreviewLabel**
3. Text: "Starter Deck Preview"
4. Font Size: 24
5. Font: Roboto-Bold
6. Color: White
7. Alignment: Center

#### DeckPreviewScrollView
1. Right-click DeckPreviewSection → **UI** → **Scroll View**
2. Rename to **DeckPreviewScrollView**
3. Delete default "Scrollbar Vertical" and "Scrollbar Horizontal" children (optional)
4. **Scroll Rect** component:
   - Content: DeckCardGrid (will assign after creating it)
   - Viewport: Viewport (auto-created)
   - Horizontal: ❌ (or ✅ if you want)
   - Vertical: ✅
   - Movement Type: Clamped
   - Scroll Sensitivity: 20
5. **RectTransform**:
   - Height: 250 or flexible

#### Viewport (auto-created by Scroll View)
- Usually doesn't need changes
- Has **Mask** component
- Has **Image** component (transparent)

#### DeckCardGrid
1. Select **Viewport** → Right-click → **UI** → **Empty**
2. Rename to **DeckCardGrid**
3. Add **Grid Layout Group**:
   - Cell Size: (120, 40) - matches your settings
   - Spacing: (10, 10) - matches your settings
   - Start Corner: Upper Left
   - Start Axis: Horizontal
   - Child Alignment: Upper Center
   - Constraint: Fixed Column Count
   - Constraint Count: 6 (maxCardsPerRow)
4. Add **Content Size Fitter**:
   - Horizontal Fit: Preferred Size (or Unconstrained)
   - Vertical Fit: Preferred Size
5. **RectTransform**:
   - Anchors: Top-Left
   - Pivot: (0.5, 1) or (0, 1)
6. **IMPORTANT**: Assign this to ScrollRect → Content

---

### 3.6: Action Buttons

#### ActionButtons Container
1. Create Empty under ContentPanel → Rename to **ActionButtons**
2. Add **Horizontal Layout Group**:
   - Spacing: 20
   - Child Alignment: Middle Center
   - Child Force Expand Width: ❌
3. **Layout Element**:
   - Preferred Height: 60

#### BackButton
1. Right-click ActionButtons → **UI** → **Button**
2. Rename to **BackButton**
3. Add TextMeshProUGUI child: "Back to Main Menu"
4. **RectTransform**: Width: 200, Height: 50
5. Style with your button sprite/colors

#### CreateCharacterButton
1. Right-click ActionButtons → **UI** → **Button**
2. Rename to **CreateCharacterButton**
3. Add TextMeshProUGUI child: "Create Character"
4. **RectTransform**: Width: 200, Height: 50
5. Style with your primary button sprite/colors (green or highlighted)

---

### 3.7: Card Hover Preview (Important!)

This is for showing a larger card preview on hover:

1. Right-click **CharacterCreationCanvas** → **Create Empty**
2. Rename to **CardHoverPreview**
3. **RectTransform**:
   - Anchors: Bottom-Left (or wherever you want hover card to appear)
   - Width: 250 (or your card width × hoverCardScale)
   - Height: 350 (or your card height × hoverCardScale)
   - Position: Adjust based on where you want it

**Note**: The controller will instantiate card prefabs here dynamically when hovering deck cards.

---

## Part 4: New Controller Script

Create `Assets/Scripts/UI/CharacterCreationCanvasController.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Canvas-based Character Creation controller.
/// Manages class selection, name input, deck preview, and character creation.
/// </summary>
public class CharacterCreationCanvasController : MonoBehaviour
{
    [Header("Name Input")]
    [SerializeField] private TMP_InputField nameInputField;
    
    [Header("Class Selection Buttons")]
    [SerializeField] private Button witchButton;
    [SerializeField] private Button marauderButton;
    [SerializeField] private Button rangerButton;
    [SerializeField] private Button thiefButton;
    [SerializeField] private Button apostleButton;
    [SerializeField] private Button brawlerButton;
    
    [Header("Deck Preview")]
    [SerializeField] private Transform deckCardGrid;
    [SerializeField] private GameObject deckCardPrefab;
    
    [Header("Card Hover Preview")]
    [SerializeField] private Transform cardHoverPreviewParent;
    [SerializeField] private GameObject fullCardPrefab;
    [SerializeField] private float hoverOffsetY = 100f;
    [SerializeField] private float hoverOffsetX = 0f;
    [SerializeField] private float hoverCardScale = 1.2f;
    
    [Header("Action Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button createCharacterButton;
    
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "MainGameUI";
    
    [Header("Deck Management")]
    [SerializeField] private StarterDeckManager starterDeckManager;
    
    // State
    private string selectedClass = "";
    private string characterName = "";
    private List<GameObject> deckPreviewCards = new List<GameObject>();
    private GameObject currentHoverCard = null;
    
    // References (auto-found)
    private CharacterManager characterManager;
    private CharacterSaveSystem characterSaveSystem;
    private TransitionManager transitionManager;
    
    // Class button references for highlighting
    private Button currentlySelectedButton = null;

    void Start()
    {
        // Find existing managers
        characterManager = FindObjectOfType<CharacterManager>();
        characterSaveSystem = FindObjectOfType<CharacterSaveSystem>();
        transitionManager = FindObjectOfType<TransitionManager>();
        starterDeckManager = FindObjectOfType<StarterDeckManager>();

        if (characterManager == null)
            Debug.LogError("[CharacterCreation] CharacterManager not found!");
        if (characterSaveSystem == null)
            Debug.LogError("[CharacterCreation] CharacterSaveSystem not found!");
        if (starterDeckManager == null)
            Debug.LogError("[CharacterCreation] StarterDeckManager not found!");

        // Setup buttons
        SetupButtons();
        
        // Initialize UI state
        UpdateCreateButtonState();
        
        Debug.Log("[CharacterCreation] Initialization complete!");
    }

    void SetupButtons()
    {
        // Class selection buttons
        if (witchButton != null)
            witchButton.onClick.AddListener(() => OnClassSelected("Witch", witchButton));
        
        if (marauderButton != null)
            marauderButton.onClick.AddListener(() => OnClassSelected("Marauder", marauderButton));
        
        if (rangerButton != null)
            rangerButton.onClick.AddListener(() => OnClassSelected("Ranger", rangerButton));
        
        if (thiefButton != null)
            thiefButton.onClick.AddListener(() => OnClassSelected("Thief", thiefButton));
        
        if (apostleButton != null)
            apostleButton.onClick.AddListener(() => OnClassSelected("Apostle", apostleButton));
        
        if (brawlerButton != null)
            brawlerButton.onClick.AddListener(() => OnClassSelected("Brawler", brawlerButton));
        
        // Name input
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }
        
        // Action buttons
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        
        if (createCharacterButton != null)
            createCharacterButton.onClick.AddListener(OnCreateCharacterClicked);
    }

    #region Event Handlers

    void OnClassSelected(string className, Button selectedButton)
    {
        Debug.Log($"<color=cyan>[CharacterCreation] Class selected: {className}</color>");
        selectedClass = className;
        
        // Update button highlights
        UpdateClassButtonHighlight(selectedButton);
        
        // Load and display starter deck for this class
        LoadStarterDeckPreview(className);
        
        // Update create button state
        UpdateCreateButtonState();
    }

    void OnNameChanged(string newName)
    {
        characterName = newName;
        Debug.Log($"[CharacterCreation] Name changed to: {characterName}");
        UpdateCreateButtonState();
    }

    void OnBackClicked()
    {
        Debug.Log("[CharacterCreation] Back button clicked");
        LoadScene(mainMenuSceneName);
    }

    void OnCreateCharacterClicked()
    {
        Debug.Log("<color=lime>[CharacterCreation] Create Character button clicked</color>");
        
        // Validate inputs
        if (string.IsNullOrWhiteSpace(characterName))
        {
            Debug.LogWarning("[CharacterCreation] Character name is empty!");
            return;
        }
        
        if (string.IsNullOrEmpty(selectedClass))
        {
            Debug.LogWarning("[CharacterCreation] No class selected!");
            return;
        }
        
        // Check if character name already exists
        if (characterSaveSystem != null && characterSaveSystem.CharacterExists(characterName))
        {
            Debug.LogWarning($"[CharacterCreation] Character name '{characterName}' already exists!");
            // TODO: Show error message to player
            return;
        }
        
        // Create character
        CreateCharacter();
    }

    #endregion

    #region Character Creation

    void CreateCharacter()
    {
        Debug.Log($"<color=lime>[CharacterCreation] Creating character: {characterName} ({selectedClass})</color>");
        
        if (characterManager == null)
        {
            Debug.LogError("[CharacterCreation] CharacterManager is null!");
            return;
        }
        
        // Create new character through CharacterManager
        characterManager.CreateCharacter(characterName, selectedClass);
        
        // Load starter deck
        if (starterDeckManager != null)
        {
            Debug.Log($"[CharacterCreation] Loading starter deck for {selectedClass}");
            starterDeckManager.LoadStarterDeck(selectedClass);
        }
        
        // Save the character
        characterManager.SaveCharacter();
        
        Debug.Log($"[CharacterCreation] Character created and saved! Transitioning to game...");
        
        // Transition to game scene
        LoadScene(gameSceneName);
    }

    #endregion

    #region Deck Preview

    void LoadStarterDeckPreview(string className)
    {
        Debug.Log($"[CharacterCreation] Loading deck preview for {className}");
        
        // Clear existing preview
        ClearDeckPreview();
        
        if (starterDeckManager == null || deckCardGrid == null)
        {
            Debug.LogWarning("[CharacterCreation] StarterDeckManager or DeckCardGrid is null!");
            return;
        }
        
        // Get starter deck for this class
        DeckPreset starterDeck = starterDeckManager.GetStarterDeck(className);
        
        if (starterDeck == null)
        {
            Debug.LogWarning($"[CharacterCreation] No starter deck found for {className}");
            return;
        }
        
        Debug.Log($"[CharacterCreation] Starter deck has {starterDeck.cards.Count} cards");
        
        // Create card preview for each card in deck
        foreach (var deckEntry in starterDeck.cards)
        {
            CreateDeckCardPreview(deckEntry);
        }
    }

    void CreateDeckCardPreview(DeckCardEntry deckEntry)
    {
        if (deckCardPrefab == null)
        {
            Debug.LogError("[CharacterCreation] Deck card prefab is null!");
            return;
        }
        
        GameObject cardObj = Instantiate(deckCardPrefab, deckCardGrid);
        deckPreviewCards.Add(cardObj);
        
        // Setup card display
        DeckCardListUI cardUI = cardObj.GetComponent<DeckCardListUI>();
        if (cardUI != null)
        {
            cardUI.Initialize(deckEntry, null); // No deck builder UI reference
        }
        else
        {
            Debug.LogWarning($"[CharacterCreation] Card prefab missing DeckCardListUI component!");
        }
        
        // Setup hover behavior (optional - shows larger preview)
        SetupCardHover(cardObj, deckEntry.cardData);
    }

    void SetupCardHover(GameObject cardObj, CardData cardData)
    {
        // Add event trigger for hover
        UnityEngine.EventSystems.EventTrigger trigger = cardObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = cardObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // Pointer Enter
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => { OnCardHoverEnter(cardData, cardObj); });
        trigger.triggers.Add(pointerEnter);
        
        // Pointer Exit
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => { OnCardHoverExit(); });
        trigger.triggers.Add(pointerExit);
    }

    void OnCardHoverEnter(CardData cardData, GameObject hoveredCard)
    {
        if (fullCardPrefab == null || cardHoverPreviewParent == null) return;
        
        // Destroy previous hover card
        if (currentHoverCard != null)
        {
            Destroy(currentHoverCard);
        }
        
        // Create hover preview
        currentHoverCard = Instantiate(fullCardPrefab, cardHoverPreviewParent);
        currentHoverCard.transform.localScale = Vector3.one * hoverCardScale;
        
        // Initialize card
        DeckBuilderCardUI cardUI = currentHoverCard.GetComponent<DeckBuilderCardUI>();
        if (cardUI != null)
        {
            cardUI.Initialize(cardData, null);
        }
        
        // Position near hovered card (optional - or keep at fixed position)
        // RectTransform rect = currentHoverCard.GetComponent<RectTransform>();
        // if (rect != null)
        // {
        //     // Position logic here if you want it to follow the hovered card
        // }
    }

    void OnCardHoverExit()
    {
        if (currentHoverCard != null)
        {
            Destroy(currentHoverCard);
            currentHoverCard = null;
        }
    }

    void ClearDeckPreview()
    {
        foreach (var card in deckPreviewCards)
        {
            if (card != null) Destroy(card);
        }
        deckPreviewCards.Clear();
        
        // Also clear hover card
        if (currentHoverCard != null)
        {
            Destroy(currentHoverCard);
            currentHoverCard = null;
        }
    }

    #endregion

    #region UI State Management

    void UpdateClassButtonHighlight(Button selectedButton)
    {
        // Reset all class buttons to normal color
        ResetClassButtonColors();
        
        // Highlight selected button
        if (selectedButton != null)
        {
            ColorBlock colors = selectedButton.colors;
            colors.normalColor = new Color(0.3f, 1f, 0.3f, 1f); // Green tint for selected
            selectedButton.colors = colors;
            currentlySelectedButton = selectedButton;
        }
    }

    void ResetClassButtonColors()
    {
        Button[] classButtons = { witchButton, marauderButton, rangerButton, thiefButton, apostleButton, brawlerButton };
        
        foreach (var btn in classButtons)
        {
            if (btn != null)
            {
                ColorBlock colors = btn.colors;
                colors.normalColor = Color.white; // Reset to default
                btn.colors = colors;
            }
        }
    }

    void UpdateCreateButtonState()
    {
        if (createCharacterButton == null) return;
        
        bool canCreate = !string.IsNullOrWhiteSpace(characterName) && !string.IsNullOrEmpty(selectedClass);
        createCharacterButton.interactable = canCreate;
        
        Debug.Log($"[CharacterCreation] Create button state: {(canCreate ? "ENABLED" : "DISABLED")} (Name: '{characterName}', Class: '{selectedClass}')");
    }

    #endregion

    #region Scene Loading

    void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[CharacterCreation] Scene name is NULL or EMPTY!");
            return;
        }

        Debug.Log($"<color=yellow>[CharacterCreation] Loading scene: {sceneName}</color>");
        
        if (transitionManager != null)
        {
            transitionManager.TransitionToScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Debug: Test Create Character")]
    void DebugTestCreateCharacter()
    {
        characterName = "TestHero";
        selectedClass = "Marauder";
        CreateCharacter();
    }

    [ContextMenu("Debug: Print Selected State")]
    void DebugPrintState()
    {
        Debug.Log($"[CharacterCreation Debug]");
        Debug.Log($"  Name: '{characterName}'");
        Debug.Log($"  Class: '{selectedClass}'");
        Debug.Log($"  Can Create: {(!string.IsNullOrWhiteSpace(characterName) && !string.IsNullOrEmpty(selectedClass))}");
    }

    #endregion
}
```

---

## Part 5: Setup Instructions

### 5.1: Add Controller to Scene
1. In CharacterCreation scene, create empty GameObject
2. Rename to **CharacterCreationController**
3. Add `CharacterCreationCanvasController` script

### 5.2: Assign References in Inspector

**Name Input:**
- Name Input Field ← `ContentPanel/NameSection/NameInputField`

**Class Selection Buttons (6):**
- Witch Button ← `ContentPanel/ClassSection/ClassGrid/WitchButton`
- Marauder Button ← `ContentPanel/ClassSection/ClassGrid/MarauderButton`
- Ranger Button ← `ContentPanel/ClassSection/ClassGrid/RangerButton`
- Thief Button ← `ContentPanel/ClassSection/ClassGrid/ThiefButton`
- Apostle Button ← `ContentPanel/ClassSection/ClassGrid/ApostleButton`
- Brawler Button ← `ContentPanel/ClassSection/ClassGrid/BrawlerButton`

**Deck Preview:**
- Deck Card Grid ← `ContentPanel/DeckPreviewSection/DeckPreviewScrollView/Viewport/DeckCardGrid`
- Deck Card Prefab ← Drag from Project (your small card prefab)

**Card Hover Preview:**
- Card Hover Preview Parent ← `CharacterCreationCanvas/CardHoverPreview`
- Full Card Prefab ← Drag from Project (your full-size card prefab)
- Hover Offset Y: `100`
- Hover Offset X: `0`
- Hover Card Scale: `1.2`

**Action Buttons:**
- Back Button ← `ContentPanel/ActionButtons/BackButton`
- Create Character Button ← `ContentPanel/ActionButtons/CreateCharacterButton`

**Scene Names:**
- Main Menu Scene Name: `MainMenu`
- Game Scene Name: `MainGameUI` (or your game scene)

**Deck Management:**
- Starter Deck Manager ← Will auto-find, but can manually assign

---

## Part 6: Class Button Prefab/Template (Optional)

To save time, create one class button and duplicate it:

### 6.1: Create ClassButtonTemplate

1. Create **WitchButton** with full styling
2. Duplicate 5 times for other classes
3. Update text labels for each

### 6.2: Visual States (ColorBlock)

Set button **ColorBlock** in Inspector:
- **Normal**: White or light color
- **Highlighted**: Lighter (hover)
- **Pressed**: Slightly darker
- **Selected**: Green tint (script will set this)
- **Disabled**: Gray

---

## Part 7: Styling Guide

### Color Scheme Recommendations

**Background:**
- Dark with transparency: `rgba(0, 0, 0, 0.8)`

**Name Input:**
- Background: `rgba(255, 255, 255, 0.1)`
- Text: White
- Placeholder: Light gray

**Class Buttons:**
- Normal: `rgba(255, 255, 255, 0.2)`
- Hover: `rgba(255, 255, 255, 0.3)`
- Selected: `rgba(0, 255, 0, 0.3)` (green tint)
- Border: `rgba(255, 255, 255, 0.3)`

**Create Button:**
- Background: Green tint `rgba(76, 175, 80, 0.8)`
- Hover: Brighter green
- Disabled: Gray

**Back Button:**
- Background: Neutral `rgba(150, 150, 150, 0.5)`
- Hover: Lighter

---

## Part 8: Testing Checklist

After migration, verify:

- [ ] Scene loads without errors
- [ ] Character name input field works
- [ ] All 6 class buttons are clickable
- [ ] Clicking a class button:
  - [ ] Highlights that button (green tint)
  - [ ] Loads starter deck preview below
  - [ ] Deck cards appear in grid
- [ ] Hovering a deck card shows larger preview
- [ ] Create button:
  - [ ] Disabled when name empty or class not selected
  - [ ] Enabled when both name and class are set
  - [ ] Creates character and loads game scene
- [ ] Back button returns to MainMenu
- [ ] No console errors

---

## Part 9: Troubleshooting

### Deck preview cards not appearing?
- Verify **StarterDeckManager** exists and has starter decks assigned
- Check **DeckCardGrid** is assigned as ScrollRect Content
- Verify **deckCardPrefab** is assigned
- Check console for "Starter deck has X cards" message

### Hover preview not showing?
- Verify **cardHoverPreviewParent** is assigned
- Check **fullCardPrefab** is assigned
- Ensure CardHoverPreview GameObject is active

### Create button always disabled?
- Check name input field is assigned
- Verify you've entered a name AND selected a class
- Check console for "Create button state" messages

### Character not created?
- Verify CharacterManager.CreateCharacter() is being called
- Check CharacterSaveSystem exists
- Verify game scene name is correct

### Class buttons not highlighting?
- ColorBlock changes might not be visible with custom sprites
- Consider adding a checkmark or border instead
- Or add an Image overlay that shows/hides

---

## Part 10: Optional Enhancements

### 10.1: Character Name Validation
Add to `OnNameChanged()`:
```csharp
// Remove invalid characters
newName = System.Text.RegularExpressions.Regex.Replace(newName, @"[^a-zA-Z0-9_\- ]", "");
nameInputField.text = newName;
```

### 10.2: Character Already Exists Warning
Add a TextMeshProUGUI for error messages:
```csharp
[SerializeField] private TextMeshProUGUI errorMessageText;

// In OnCreateCharacterClicked():
if (characterSaveSystem.CharacterExists(characterName))
{
    errorMessageText.text = "Character name already exists!";
    errorMessageText.color = Color.red;
    return;
}
```

### 10.3: Class Selection Visual Feedback
Instead of color tint, add a checkmark or border:
```csharp
[SerializeField] private GameObject selectionIndicator; // Place on selected button
```

### 10.4: Deck Card Count Display
Show total card count:
```csharp
[SerializeField] private TextMeshProUGUI deckCountText;

// After loading deck:
deckCountText.text = $"{starterDeck.GetTotalCardCount()} Cards";
```

---

## Estimated Time: ~45 minutes
- Canvas setup: 10 min
- Name section: 5 min
- Class section + 6 buttons: 15 min
- Deck preview section: 10 min
- Action buttons: 3 min
- Script wiring: 5 min
- Testing: 10 min

---

## Summary

**Key Differences from MainMenu:**
- More complex UI (input field, multiple buttons, deck preview)
- Grid layouts for class selection and deck cards
- Hover preview system for cards
- Input validation
- Button highlighting for selected class

**Result:**
A fully functional Canvas-based Character Creation screen with class selection, deck preview, and character creation! 🎮


