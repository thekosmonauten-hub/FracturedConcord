# MainMenu UI Toolkit → Canvas Migration Guide

## Overview
Migrating MainMenu from UI Toolkit (UIDocument) to Canvas-based UI while preserving all existing managers and functionality.

---

## ⚠️ CRITICAL: What to Keep vs Replace

### ✅ KEEP (Don't Touch):
- **CharacterManager** GameObject & script
- **CharacterSaveSystem** GameObject & script  
- **StarterDeckManager** GameObject & script
- **TransitionManager** GameObject & script
- **EventSystem** GameObject
- **Main Camera**

### ❌ REMOVE/DISABLE:
- **MainMenuCanvas** GameObject (the UI Toolkit one)
  - Or just disable the `UIDocument` component
- **MainMenu** child GameObject with `UIDocument` component

### 🔄 REPLACE:
- **MainMenuController.cs** script (needs rewrite for Canvas)
- UI structure (UXML → Canvas hierarchy)

---

## Step-by-Step Migration

### Part 1: Scene Setup (5 minutes)

#### 1.1: Disable Old UI
1. In MainMenu scene, find **MainMenuCanvas** GameObject
2. Disable the **UIDocument** component (or delete entire MainMenu child)
3. Keep MainMenuCanvas if it has CanvasScaler/GraphicRaycaster you want to reuse

#### 1.2: Create New Canvas
1. Right-click in Hierarchy → **UI** → **Canvas**
2. Rename to **MainMenuCanvasUI**
3. Set **Render Mode** to **Screen Space - Overlay**
4. Verify it has:
   - `Canvas` component
   - `CanvasScaler` component (UI Scale Mode: Scale With Screen Size, Reference: 1920x1080)
   - `GraphicRaycaster` component

#### 1.3: Create Background Image
1. Right-click **MainMenuCanvasUI** → **UI** → **Image**
2. Rename to **Background**
3. Set to fullscreen:
   - **Anchor**: Stretch/Stretch (alt+shift+click bottom-right preset)
   - **Left/Right/Top/Bottom**: 0
4. Assign **Source Image**: `MainMenu_Dexiled.png`
5. **Image Type**: Simple
6. **Preserve Aspect**: Unchecked

---

### Part 2: Main Menu Panel (10 minutes)

#### 2.1: Create Main Menu Container
```
MainMenuCanvasUI
  └─ Background
  └─ MainMenuPanel          [Empty GameObject with RectTransform]
      └─ ContentContainer   [Vertical Layout Group]
          ├─ HeaderText
          ├─ NewGameButton
          ├─ ContinueButton
          ├─ SettingsButton
          └─ ExitButton
```

#### 2.2: MainMenuPanel Setup
- **RectTransform**: Anchors centered, Width: 412, Height: flexible
- Position: Center of screen

#### 2.3: ContentContainer Setup
- Add **Vertical Layout Group**:
  - Child Alignment: Middle Center
  - Control Child Size: ✅ Width, ✅ Height
  - Child Force Expand: ❌ Width, ❌ Height
  - Spacing: 12
  - Padding: 20 all sides

#### 2.4: HeaderText
- **TextMeshProUGUI** component
- Text: "DEXILED"
- Font: Oswald-Bold
- Font Size: 96
- Alignment: Center
- Color: White
- Add **Shadow** component (4px offset)

#### 2.5: Buttons (x4)
- Add **Button** component
- Add **Image** component:
  - Source Image: `Wide_Buttons_Sprites_Sheet_4` (normal state)
  - Image Type: Simple
  - Color: White
- **RectTransform**: 
  - Width: 220
  - Height: 60
- Add **TextMeshProUGUI** child:
  - Font: Roboto-Bold
  - Font Size: 18
  - Color: White
  - Alignment: Center
  - Text: "Start Journey", "Continue", "Settings", "Exit"

#### 2.6: Button Hover Effect (Optional)
- Add **HoverImageSwap.cs** script (see Part 5) to each button
- Assign hover sprite: `Wide_Buttons_Sprites_Sheet_5`

---

### Part 3: Character Sidebar (15 minutes)

#### 3.1: Create Sidebar Structure
```
MainMenuCanvasUI
  └─ CharacterSidebar           [RectTransform + Image]
      ├─ SidebarHeader          [Empty with Horizontal Layout]
      │   ├─ TitleText
      │   └─ CloseButton
      ├─ CharacterScrollView    [Scroll Rect]
      │   └─ Viewport
      │       └─ CharacterList  [Vertical Layout Group]
      └─ CreateCharacterButton
```

#### 3.2: CharacterSidebar Setup
- **RectTransform**:
  - Anchors: Top-Right stretch (right side of screen)
  - Pivot: (1, 0.5)
  - Width: 400
  - Left: -400 (starts off-screen)
  - Top: 0, Bottom: 0
- **Image** component:
  - Source Image: `CharacterSelectionPillar.png`
  - Image Type: Sliced or Simple
  - Color: Slightly transparent black
- **CanvasGroup** component (for fade animations):
  - Alpha: 1
  - Interactable: ✅
  - Block Raycasts: ✅

#### 3.3: CharacterScrollView Setup
- Add **Scroll Rect** component:
  - Content: CharacterList RectTransform
  - Viewport: Viewport RectTransform
  - Vertical: ✅
  - Horizontal: ❌
  - Movement Type: Clamped
  - Scroll Sensitivity: 20
- **RectTransform**: Stretch to fill (except top 80px for header, bottom 80px for button)
  - Anchors: Stretch/Stretch
  - Top: 80, Bottom: 80, Left: 10, Right: 10
- Add **Image** (for background):
  - Source Image: None
  - Color: Transparent or slight tint

#### 3.4: CharacterList Setup
- **Vertical Layout Group**:
  - Child Alignment: Upper Center
  - Control Child Size: ✅ Width, ❌ Height
  - Child Force Expand: ✅ Width, ❌ Height
  - Spacing: 10
  - Padding: 10 all sides
- **Content Size Fitter**:
  - Vertical Fit: Preferred Size

---

### Part 4: Character Card Prefab (10 minutes)

#### 4.1: Create Prefab Structure
1. In Hierarchy: Right-click → Create Empty → rename "CharacterCardPrefab"
2. Add **RectTransform** component
3. Structure:
```
CharacterCardPrefab
  ├─ BackgroundImage
  ├─ InfoSection
  │   ├─ NameText
  │   └─ DetailsText
  └─ ButtonsSection
      ├─ PlayButton
      └─ DeleteButton
```

#### 4.2: CharacterCardPrefab Root
- **RectTransform**:
  - Width: 360 (fills scroll view with padding)
  - Height: 120 (or use Layout Element with Preferred Height: 120)
- Add **Layout Element** component:
  - Preferred Height: 120
  - Flexible Width: 1

#### 4.3: BackgroundImage
- **Image** component:
  - Source Image: `CharacterSelectionBackground`
  - Image Type: Sliced (if has border) or Simple
  - Color: White
- **RectTransform**: Stretch to fill parent (Anchors: Stretch/Stretch, all offsets: 0)

#### 4.4: InfoSection
- **RectTransform**: 
  - Anchors: Top-Left stretch
  - Height: 60
  - Padding: 15px
- NameText (TextMeshProUGUI):
  - Font: Roboto-Bold, Size: 18, Color: White
- DetailsText (TextMeshProUGUI):
  - Font: Roboto-Bold, Size: 14, Color: Light Gray

#### 4.5: ButtonsSection
- **Horizontal Layout Group**:
  - Spacing: 10
  - Child Alignment: Middle Center
- **RectTransform**:
  - Anchors: Bottom stretch
  - Height: 40
  - Padding from sides: 15px
- PlayButton / DeleteButton:
  - Width: Flexible (equal split)
  - Height: 35
  - Colors: Green for Play, Red for Delete

#### 4.6: Save as Prefab
1. Drag **CharacterCardPrefab** from Hierarchy to **Assets/Prefabs/UI/**
2. Delete from Hierarchy (or keep for reference)

---

### Part 5: Sidebar Toggle Button

#### 5.1: Create Toggle Button
1. Right-click **MainMenuCanvasUI** → **UI** → **Button**
2. Rename to **SidebarToggleButton**
3. **RectTransform**:
   - Anchors: Top-Right
   - Pivot: (1, 1)
   - Position: X: -20, Y: -20 (from top-right corner)
   - Width: 120, Height: 40

#### 5.2: Style Toggle Button
- **Image**: `Wide_Buttons_Sprites_Sheet_5`
- **TextMeshProUGUI** child: "Characters"
- Font: Roboto-Bold, Size: 14

---

### Part 6: Updated Controller Script

#### 6.1: Create New Script
Create `Assets/Scripts/UI/MainMenuCanvasController.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Canvas-based MainMenu controller.
/// Manages main menu buttons, character sidebar, and scene transitions.
/// </summary>
public class MainMenuCanvasController : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Character Sidebar")]
    [SerializeField] private RectTransform characterSidebar;
    [SerializeField] private Button sidebarToggleButton;
    [SerializeField] private Button sidebarCloseButton;
    [SerializeField] private Transform characterListContainer;
    [SerializeField] private GameObject characterCardPrefab;
    [SerializeField] private Button createCharacterButton;

    [Header("Scene Names")]
    [SerializeField] private string characterCreationSceneName = "CharacterCreation";
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Animation Settings")]
    [SerializeField] private float sidebarAnimDuration = 0.3f;
    [SerializeField] private Ease sidebarAnimEase = Ease.OutCubic;

    // State
    private bool isSidebarOpen = false;
    private List<GameObject> characterCardInstances = new List<GameObject>();
    private CharacterData selectedCharacter = null;

    // References (auto-found)
    private CharacterManager characterManager;
    private CharacterSaveSystem characterSaveSystem;
    private TransitionManager transitionManager;

    void Start()
    {
        // Find existing managers in scene
        characterManager = FindObjectOfType<CharacterManager>();
        characterSaveSystem = FindObjectOfType<CharacterSaveSystem>();
        transitionManager = FindObjectOfType<TransitionManager>();

        if (characterManager == null)
            Debug.LogError("[MainMenu] CharacterManager not found in scene!");
        if (characterSaveSystem == null)
            Debug.LogError("[MainMenu] CharacterSaveSystem not found in scene!");

        // Setup button listeners
        SetupButtons();

        // Load characters
        RefreshCharacterList();

        // Initialize sidebar (closed)
        if (characterSidebar != null)
        {
            characterSidebar.anchoredPosition = new Vector2(400, 0); // Off-screen right
        }
    }

    void SetupButtons()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);
        
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
        
        if (sidebarToggleButton != null)
            sidebarToggleButton.onClick.AddListener(ToggleSidebar);
        
        if (sidebarCloseButton != null)
            sidebarCloseButton.onClick.AddListener(() => ToggleSidebar());
        
        if (createCharacterButton != null)
            createCharacterButton.onClick.AddListener(OnCreateCharacterClicked);
    }

    #region Button Handlers

    void OnNewGameClicked()
    {
        Debug.Log("[MainMenu] New Game clicked");
        ToggleSidebar(); // Open character selection
    }

    void OnContinueClicked()
    {
        Debug.Log("[MainMenu] Continue clicked");
        
        // Load most recent character
        if (characterSaveSystem != null)
        {
            var characters = characterSaveSystem.GetAllCharacters();
            if (characters.Count > 0)
            {
                LoadCharacter(characters[0]);
            }
            else
            {
                Debug.LogWarning("[MainMenu] No characters found!");
                ToggleSidebar(); // Open character selection
            }
        }
    }

    void OnSettingsClicked()
    {
        Debug.Log("[MainMenu] Settings clicked (not implemented)");
    }

    void OnExitClicked()
    {
        Debug.Log("[MainMenu] Exit clicked");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void OnCreateCharacterClicked()
    {
        Debug.Log("[MainMenu] Create Character clicked");
        LoadScene(characterCreationSceneName);
    }

    #endregion

    #region Character Management

    void RefreshCharacterList()
    {
        // Clear existing cards
        foreach (var card in characterCardInstances)
        {
            if (card != null) Destroy(card);
        }
        characterCardInstances.Clear();

        if (characterSaveSystem == null || characterListContainer == null)
            return;

        // Get all saved characters
        List<CharacterData> characters = characterSaveSystem.GetAllCharacters();
        
        Debug.Log($"[MainMenu] Loading {characters.Count} characters");

        // Create card for each character
        foreach (var character in characters)
        {
            CreateCharacterCard(character);
        }

        // Update continue button state
        if (continueButton != null)
        {
            continueButton.interactable = characters.Count > 0;
        }
    }

    void CreateCharacterCard(CharacterData character)
    {
        if (characterCardPrefab == null)
        {
            Debug.LogError("[MainMenu] Character card prefab not assigned!");
            return;
        }

        GameObject cardObj = Instantiate(characterCardPrefab, characterListContainer);
        characterCardInstances.Add(cardObj);

        // Get references to card components
        var nameText = cardObj.transform.Find("InfoSection/NameText")?.GetComponent<TextMeshProUGUI>();
        var detailsText = cardObj.transform.Find("InfoSection/DetailsText")?.GetComponent<TextMeshProUGUI>();
        var playButton = cardObj.transform.Find("ButtonsSection/PlayButton")?.GetComponent<Button>();
        var deleteButton = cardObj.transform.Find("ButtonsSection/DeleteButton")?.GetComponent<Button>();

        // Set text
        if (nameText != null)
            nameText.text = character.characterName;
        
        if (detailsText != null)
            detailsText.text = $"Level {character.level} {character.characterClass} - Act {character.act}";

        // Setup buttons
        if (playButton != null)
            playButton.onClick.AddListener(() => LoadCharacter(character));
        
        if (deleteButton != null)
            deleteButton.onClick.AddListener(() => DeleteCharacter(character));
    }

    void LoadCharacter(CharacterData character)
    {
        Debug.Log($"[MainMenu] Loading character: {character.characterName}");
        
        if (characterManager != null)
        {
            characterManager.LoadCharacter(character);
            LoadScene(gameSceneName);
        }
    }

    void DeleteCharacter(CharacterData character)
    {
        Debug.Log($"[MainMenu] Deleting character: {character.characterName}");
        
        // TODO: Add confirmation dialog
        if (characterSaveSystem != null)
        {
            characterSaveSystem.DeleteCharacter(character.characterName);
            RefreshCharacterList();
        }
    }

    #endregion

    #region Sidebar Animation

    void ToggleSidebar()
    {
        isSidebarOpen = !isSidebarOpen;
        AnimateSidebar(isSidebarOpen);
    }

    void AnimateSidebar(bool open)
    {
        if (characterSidebar == null) return;

        float targetX = open ? 0 : 400; // 0 = visible, 400 = off-screen

        // Animate with DOTween
        characterSidebar.DOAnchorPosX(targetX, sidebarAnimDuration)
            .SetEase(sidebarAnimEase);

        // Update toggle button visibility (optional)
        if (sidebarToggleButton != null)
        {
            sidebarToggleButton.gameObject.SetActive(!open);
        }
    }

    #endregion

    #region Scene Loading

    void LoadScene(string sceneName)
    {
        if (transitionManager != null)
        {
            transitionManager.LoadScene(sceneName);
        }
        else
        {
            SceneManagement.LoadScene(sceneName);
        }
    }

    #endregion
}
```

#### 6.2: Add Script to Scene
1. Find or create an empty GameObject named **MainMenuCanvasController**
2. Add the `MainMenuCanvasController` script
3. Assign all references in Inspector:
   - Main Menu Buttons (drag from Hierarchy)
   - Character Sidebar components
   - Character Card Prefab (drag from Assets)
   - Scene names

---

### Part 7: Optional Helper Scripts

#### 7.1: Button Hover Effect Script
Create `Assets/Scripts/UI/HoverImageSwap.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Swaps button sprite on hover.
/// </summary>
[RequireComponent(typeof(Button))]
public class HoverImageSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    
    private Image buttonImage;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage != null && normalSprite == null)
        {
            normalSprite = buttonImage.sprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null && hoverSprite != null)
        {
            buttonImage.sprite = hoverSprite;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null && normalSprite != null)
        {
            buttonImage.sprite = normalSprite;
        }
    }
}
```

---

## Critical References Checklist

### ✅ What MainMenuCanvasController Needs:
1. **CharacterManager** - found via `FindObjectOfType` (already in scene)
2. **CharacterSaveSystem** - found via `FindObjectOfType` (already in scene)
3. **TransitionManager** - found via `FindObjectOfType` (already in scene)
4. **UI References** - assigned manually in Inspector

### ✅ Scene Dependencies:
- **EventSystem** - must exist for buttons to work (already in scene)
- **CharacterManager** GameObject - must persist (already in scene)
- **CharacterSaveSystem** GameObject - must persist (already in scene)

### ⚠️ Important Notes:
1. **Don't delete existing managers** - they're referenced by other systems
2. **Keep EventSystem** - required for all UI interaction
3. **Character data flows**: 
   - CharacterSaveSystem → loads saved data
   - MainMenuCanvasController → displays & manages selection
   - CharacterManager → receives selected character & carries to next scene
4. **Scene transitions**: Use TransitionManager if available, otherwise fallback to direct loading

---

## Testing Checklist

After migration, verify:

- [ ] Main menu buttons appear and are clickable
- [ ] "Start Journey" opens character sidebar
- [ ] Character sidebar animates in from right
- [ ] Saved characters appear in list
- [ ] Character cards show correct names/details
- [ ] Background images display correctly (no tiling!)
- [ ] "Play" button loads character and transitions to game
- [ ] "Delete" button removes character
- [ ] "Create Character" loads character creation scene
- [ ] "Continue" button loads most recent character
- [ ] Existing managers (CharacterManager, etc.) still work
- [ ] No console errors

---

## Troubleshooting

### Background images still tiling?
- Check Image component **Image Type**: use "Simple" or "Sliced" (not "Tiled")
- Verify sprite import settings: **Mesh Type** should be "Full Rect"

### Buttons not clickable?
- Verify **EventSystem** exists in scene
- Check Canvas has **GraphicRaycaster** component
- Ensure no UI elements are blocking (check sorting order)

### Character list not populating?
- Add debug logs in `RefreshCharacterList()`
- Verify CharacterSaveSystem.GetAllCharacters() returns data
- Check characterCardPrefab is assigned in Inspector

### Sidebar not animating?
- Verify DOTween is installed (or remove DOTween code, use LeanTween/coroutines)
- Check RectTransform anchors are correct (right-side anchored)
- Ensure characterSidebar reference is assigned

---

## Estimated Time: ~40 minutes total
- Scene setup: 5 min
- Main menu panel: 10 min
- Character sidebar: 15 min
- Character card prefab: 10 min
- Script & wiring: 10 min
- Testing: 10 min

## Result
A fully functional Canvas-based MainMenu with reliable background images, smooth animations, and all original functionality preserved!


