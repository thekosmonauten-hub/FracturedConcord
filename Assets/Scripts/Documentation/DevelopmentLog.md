# Development Log - Dexiled Unity Deckbuilder ARPG

## Project Overview
A deckbuilder ARPG with Path of Exile influences, built in Unity.

## Development Philosophy
- **Centralized Management**: Use singleton managers for core systems
- **UI Consistency**: Connect UI elements through centralized managers rather than individual scripts
- **Step-by-Step Implementation**: Build systems incrementally with proper documentation
- **Unity Best Practices**: Follow Unity conventions and patterns

---

## Session 12: Channeling Mechanic Tracking

### Date: 2025-11-07
### Goal: Introduce character-level channeling tracking and expose hooks for repeated-cast bonuses.

### Key Decisions Made

#### 1. **Treat Channeling as Character Combat State**
**Decision**: Store channeling streaks on `Character.Channeling` instead of per-card tags.
**Why**:
- Keeps streak logic independent from asset mutations
- Allows any combat system to query the same state
- Enables future mechanics (AI, status effects) to react uniformly

#### 2. **Centralize Updates in CombatDeckManager**
**Decision**: Let `CombatDeckManager` register casts, fire `OnChannelingStateChanged`, and reset state on deck load.
**Why**:
- Single point of truth during card play
- Ensures events fire before damage/effects resolve
- Provides utility function `ResetChannelingState()` for turn/scene boundaries

#### 3. **Attach Channeling Metadata to Runtime Cards**
**Decision**: Enrich the temporary `Card` instance with channeling flags and stack counts.
**Why**:
- Effect processors and combo logic can read channeling without re-querying the tracker
- Supports “start/stop” triggers by exposing `channelingStartedThisCast` / `channelingStoppedThisCast`

### Systems Implemented

1. `ChannelingTracker` with `RegisterCast`, `BreakChannel`, and snapshot struct.
2. `CombatDeckManager` updates streaks for both animated and instant plays and surfaces `OnChannelingStateChanged`.
3. Runtime `Card` metadata so effect logic can branch on channeling state.
4. Documentation: `CHANNELING_MECHANIC_GUIDE.md` describing designer & engineer workflow.

---

## Session 13: Speed Meter Foundations

### Date: 2025-11-07
### Goal: Capture attack/cast speed progress into visible meters without overwhelming the HUD.

### Key Decisions Made

#### 1. **One Ring, Dual Purpose**
**Decision**: Render Aggression (attack) and Focus (cast) as paired fills around the existing player display instead of adding new bars.
**Why**:
- Keeps the HUD lightweight
- Reuses the floating combat text system for “Meter ready” popups
- Easy to expand with charge-based effects later

#### 2. **Event-first Implementation**
**Decision**: `CombatDeckManager` raises a consolidated `SpeedMeterState`; UI simply listens and updates. No gameplay discounts yet.
**Why**:
- Lets us tune pacing before hooking real discounts/combos
- Keeps future consumer systems (mana discounts, free combos) decoupled

### Systems Implemented

1. Character helpers (`GetAttackSpeedMultiplier`, etc.) for lightweight speed multipliers.
2. Meter tracking in `CombatDeckManager` with overflow → charges, reset hooks, and UI events.
3. `SpeedMeterRing` UI component plus subscriptions from `PlayerCombatDisplay`.
4. Documentation: `COMBAT_SPEED_METERS.md` capturing design intent and TODOs.

---

## Session 14: Ranger Starter Deck & Stack Systems

### Date: 2025-11-08
### Goal: Bring the Ranger starter deck online with bespoke combo logic and introduce long-term stack mechanics.

### Key Decisions Made

1. **Centralise Class-Specific Logic**
   - Created `CardAbilityRouter` so card-specific rules live in one module rather than scattered across effect processors.
2. **Reusable Stack Infrastructure**
   - Implemented `StackSystem` to own Agitate/Tolerance/Potential with helper multipliers instead of hard-coding bonuses per mechanic.
3. **Hybrid Temporary Stat Handling**
   - Added `TemporaryStatSystem` for real-time duration buffs (seconds) while reserving `StatusEffectManager` for turn-based icons and adjustments.

### Systems Implemented

- `StackSystem` singleton with Agitate/Tolerance/Potential tracking, speed/damage/mitigation helpers, and lazy initialisation.
- `TemporaryStatSystem` singleton supporting second- or turn-based stat shifts plus player UI refreshes.
- New status effect types (`Bleed`, `TempMaxMana`, `TempEvasion`) with runtime application hooks inside `StatusEffectManager`.
- `CardAbilityRouter` coverage for Focus, Dodge, Pack Hunter, Poison Arrow, Multi-Shot, and Quickstep including combo outcomes.
- Combo metadata & asset updates for Ranger starter cards (AOE flags, group keys, combo text).
- Documentation update (this entry) describing architecture choices for card ability routing and stack systems.

---

## Session 1: Encounter System & UI Navigation

### Date: [Current Date]
### Goal: Implement scene navigation from MainGameUI to CombatScene with encounter data

### Key Decisions Made

#### 1. **UI Management Approach: Option 1 (Centralized)**
**Decision**: Use centralized managers (CombatSceneManager) instead of individual button scripts
**Why**: 
- Better maintainability
- Consistent architecture
- Easier debugging
- Single source of truth for UI logic

**Alternative Considered**: Individual scripts per button (Option 2)
**Rejected Because**: Would lead to scattered logic and harder maintenance

#### 2. **Singleton Pattern for EncounterManager**
**Decision**: Use singleton pattern with DontDestroyOnLoad
**Why**:
- Persists data between scenes
- Single instance ensures consistency
- Easy access from anywhere in the game

### Systems Implemented

#### 1. **EncounterData Class**
```csharp
[System.Serializable]
public class EncounterData
{
    public int encounterID;
    public string encounterName;
    public string sceneName;
    public bool isUnlocked = true;
    public bool isCompleted = false;
}
```
**Purpose**: Data structure for encounter information
**Unity Concept**: `[System.Serializable]` allows Unity to serialize this data in the Inspector

#### 2. **EncounterManager (Singleton)**
**Key Features**:
- Manages all encounter data
- Handles scene transitions
- Tracks completion status
- Unlocks subsequent encounters

**Unity Concepts Used**:
- `DontDestroyOnLoad()`: Keeps object alive during scene changes
- `SceneManager.LoadScene()`: Unity's scene loading system
- Singleton pattern: Ensures single instance

#### 3. **EncounterButton Component**
**Purpose**: Reusable component for any encounter button
**Features**:
- Automatic button setup
- Dynamic text updates
- Unlock/completion state handling

#### 4. **CombatSceneManager**
**Purpose**: Manages combat scene state and UI
**Key Methods**:
- `InitializeCombatScene()`: Sets up encounter data
- `ReturnToMainUI()`: Navigation back to main scene
- `CompleteEncounter()`: Handles victory state
- `FailEncounter()`: Handles defeat state

### Implementation Steps

#### Step 1: Create Data Structure
1. Created `EncounterData.cs` for encounter information
2. Made it serializable for Unity Inspector support

#### Step 2: Create Manager System
1. Created `EncounterManager.cs` with singleton pattern
2. Implemented encounter initialization
3. Added scene transition methods

#### Step 3: Create UI Components
1. Created `EncounterButton.cs` for reusable button logic
2. Created `CombatSceneManager.cs` for combat scene management
3. Updated existing `WretchedShore.cs` to use new system

#### Step 4: UI Integration
1. Added `returnToMapButton` field to CombatSceneManager
2. Connected button events in `SetupUI()` method
3. Added proper event cleanup in `OnDestroy()`

### Unity Concepts Explained

#### 1. **Scene Management**
```csharp
SceneManager.LoadScene("SceneName");
```
- Unity's built-in scene loading system
- Scenes must be added to Build Settings
- Scene names must match exactly

#### 2. **Singleton Pattern**
```csharp
public static EncounterManager Instance { get; private set; }
```
- Ensures only one instance exists
- Global access point
- Common Unity pattern for managers

#### 3. **DontDestroyOnLoad**
```csharp
DontDestroyOnLoad(gameObject);
```
- Keeps GameObject alive during scene changes
- Essential for persistent data
- Use sparingly to avoid memory issues

#### 4. **UI Button Events**
```csharp
button.onClick.AddListener(MethodName);
button.onClick.RemoveListener(MethodName);
```
- Unity's event system for UI
- Always remove listeners to prevent memory leaks
- Use in Start() and cleanup in OnDestroy()

#### 5. **Component References**
```csharp
public Button returnToMapButton;
```
- Inspector-visible fields
- Drag-and-drop assignment in Unity Editor
- Null checks required for safety

### File Structure
```
Assets/Scripts/
├── EncounterSystem/
│   ├── EncounterData.cs
│   ├── EncounterManager.cs
│   ├── EncounterButton.cs
│   └── EncounterSetup.cs
├── CombatSystem/
│   └── CombatSceneManager.cs
├── UI/
│   ├── UIManager.cs
│   ├── WretchedShore.cs
│   └── ReturnToMapButton.cs
└── Documentation/
    └── DevelopmentLog.md
```

### Testing Checklist
- [ ] MainGameUI scene loads correctly
- [ ] Encounter buttons transition to CombatScene
- [ ] CombatScene displays correct encounter information
- [ ] ReturnToMap button returns to MainGameUI
- [ ] Encounter completion unlocks next encounter
- [ ] No console errors during scene transitions

### Future Considerations
1. **Save System**: Need to persist encounter completion status
2. **Scene Transitions**: Add fade effects or loading screens
3. **Error Handling**: More robust error handling for missing scenes
4. **UI Polish**: Add visual feedback for button states
5. **Performance**: Monitor for memory leaks with DontDestroyOnLoad

### Lessons Learned
1. **Centralized Management**: Always prefer centralized systems over scattered scripts
2. **Event Cleanup**: Always remove event listeners in OnDestroy()
3. **Null Checks**: Always check for null references in Unity
4. **Documentation**: Document decisions and implementations as we go
5. **Testing**: Test each component individually before integration

---

## Session 2: Main Menu & Character Selection System

### Date: [Current Date]
### Goal: Implement dynamic character selection within the main menu using UI Toolkit

### Key Decisions Made

#### 1. **Panel-based Navigation vs Scene-based**
**Decision**: Use panel switching within single scene instead of separate scenes
**Why**: 
- Better performance (no scene loading)
- Smoother user experience
- Easier state management
- More responsive UI

**Alternative Considered**: Separate Character Selection scene
**Rejected Because**: Would require scene loading and state management complexity

#### 2. **UI Toolkit vs Legacy UI**
**Decision**: Use UI Toolkit for modern, responsive design
**Why**:
- Better styling capabilities
- More modern approach
- Better performance for complex UI
- Future-proof technology

**Alternative Considered**: Legacy UI (Canvas-based)
**Rejected Because**: Less flexible styling and older technology

#### 3. **Character Save System Architecture**
**Decision**: JSON-based file storage with singleton manager
**Why**:
- Human-readable save files
- Easy debugging and manual editing
- Cross-platform compatibility
- Simple implementation

### Systems Implemented

#### 1. **CharacterData Class**
```csharp
[System.Serializable]
public class CharacterData
{
    public string characterName;
    public string characterClass;
    public int level;
    public int act;
    public string saveDate;
}
```
**Purpose**: Data structure for character information
**Unity Concept**: Serializable for JSON persistence

#### 2. **CharacterSaveSystem (Singleton)**
**Key Features**:
- JSON file persistence
- Character CRUD operations
- Error handling and logging
- Singleton pattern for global access

**Unity Concepts Used**:
- `Application.persistentDataPath`: Cross-platform save location
- `JsonUtility`: Unity's built-in JSON serialization
- `File.WriteAllText()` / `File.ReadAllText()`: File I/O operations

#### 3. **MainMenuController (Unified UI Controller)**
**Purpose**: Manages both main menu and character selection panels
**Key Features**:
- Panel switching logic
- Dynamic character list generation
- Button event handling
- Scene navigation

**Unity Concepts Used**:
- `UIDocument`: UI Toolkit document reference
- `VisualElement.Q<T>()`: Query UI elements by name
- `DisplayStyle.Flex`/`DisplayStyle.None`: Show/hide panels
- `ListView`: Dynamic list component for characters

#### 4. **UI Toolkit Structure**
**UXML Structure**:
```xml
Container (main-container)
├── MainMenuPanel (main-menu-panel)
│   └── ContentContainer (content-container)
│       ├── Header ("DEXILED")
│       ├── NewGameButton
│       ├── ContinueButton
│       ├── SettingsButton
│       └── ExitButton
└── CharacterSelectionPanel (character-selection-panel)
    └── CharacterSelectionContent
        ├── CharacterSelectionHeader
        ├── CharacterListView
        └── CharacterSelectionButtons
```

**USS Styling**:
- Responsive design with flexbox
- Panel visibility control via `display` property
- Consistent button styling with hover effects
- Dark theme with transparency for character selection

### Implementation Steps

#### Step 1: Create Data and Save System
1. Created `CharacterData.cs` for character information
2. Created `CharacterSaveSystem.cs` with JSON persistence
3. Implemented singleton pattern for global access

#### Step 2: Create UI Structure
1. Created `MainMenuUI.uxml` with both panels
2. Created `MainMenuUI.uss` with comprehensive styling
3. Used only supported CSS properties for compatibility

#### Step 3: Implement Controller Logic
1. Created `MainMenuController.cs` with unified management
2. Implemented panel switching with `ShowMainMenu()` and `ShowCharacterSelection()`
3. Added dynamic ListView setup with `makeItem` and `bindItem` callbacks

#### Step 4: Character List Management
1. Implemented `LoadAndRefreshCharacterList()` method
2. Added sample character creation for testing
3. Connected character selection to scene navigation

### Unity Concepts Explained

#### 1. **UI Toolkit Panel Management**
```csharp
// Show main menu
mainMenuPanel.style.display = DisplayStyle.Flex;
characterSelectionPanel.style.display = DisplayStyle.None;

// Show character selection
mainMenuPanel.style.display = DisplayStyle.None;
characterSelectionPanel.style.display = DisplayStyle.Flex;
```
- `DisplayStyle.Flex`: Shows the element
- `DisplayStyle.None`: Hides the element
- No scene loading required

#### 2. **ListView with Dynamic Content**
```csharp
characterListView.makeItem = MakeCharacterItem;
characterListView.bindItem = BindCharacterItem;
characterListView.itemsSource = savedCharacters;
characterListView.Rebuild();
```
- `makeItem`: Creates new UI elements for list items
- `bindItem`: Populates existing elements with data
- `itemsSource`: Data source for the list
- `Rebuild()`: Refreshes the list display

#### 3. **UI Element Querying**
```csharp
var button = root.Q<Button>("ButtonName");
```
- `Q<T>()`: Query element by name and type
- Type-safe element retrieval
- Returns null if not found

#### 4. **Event Handling in UI Toolkit**
```csharp
button.clicked += OnButtonClicked;
button.clicked -= OnButtonClicked; // Cleanup
```
- `clicked` event for UI Toolkit buttons
- Always remove event listeners to prevent memory leaks
- Use in Start() and cleanup in OnDestroy()

#### 5. **JSON File Persistence**
```csharp
string json = JsonUtility.ToJson(data, true);
File.WriteAllText(filePath, json);
```
- `JsonUtility.ToJson()`: Converts objects to JSON string
- `File.WriteAllText()`: Writes to file system
- `Application.persistentDataPath`: Cross-platform save location

### File Structure
```
Assets/
├── Scripts/
│   ├── UI/
│   │   ├── MainMenuController.cs
│   │   ├── CharacterSelectionUI.cs (Legacy UI version)
│   │   └── CharacterSelectionUIToolkit.cs (UI Toolkit version)
│   └── Data/
│       └── CharacterSaveSystem.cs
└── UI/
    ├── MainMenuUI.uxml
    └── MainMenuUI.uss
```

### Testing Checklist
- [ ] Main menu loads with correct styling
- [ ] Continue button shows character selection panel
- [ ] Character list displays saved characters
- [ ] Character selection loads game scene
- [ ] Back button returns to main menu
- [ ] Create New Character button works
- [ ] Save system persists characters between sessions
- [ ] No console errors during panel switching

### Future Considerations
1. **Character Creation**: Implement character creation scene and flow
2. **Character Deletion**: Add delete functionality to character list
3. **Character Portraits**: Add visual character representation
4. **Save Encryption**: Consider encrypting save files for security
5. **Auto-save**: Implement automatic save functionality

### Lessons Learned
1. **Panel Switching**: More efficient than scene loading for UI transitions
2. **UI Toolkit Compatibility**: Use only supported CSS properties
3. **Event Cleanup**: Essential for UI Toolkit as well as Legacy UI
4. **Data Binding**: ListView provides efficient dynamic content
5. **Error Handling**: Always handle file I/O operations safely

---

## Session 3: Combat System & Card Game Foundation

### Date: [Current Date]
### Goal: Implement comprehensive combat system with card mechanics, damage calculations, and UI integration

### Key Decisions Made

#### 1. **Card System Architecture**
**Decision**: Modular card system with separate data and behavior classes
**Why**: 
- Separation of concerns (data vs logic)
- Easy to extend with new card types
- Reusable card components
- Clear data structure for serialization

**Alternative Considered**: Monolithic card class
**Rejected Because**: Would become unwieldy as system grows

#### 2. **Damage Calculation System**
**Decision**: Centralized DamageCalculator with stat-based scaling
**Why**:
- Consistent damage calculations across all cards
- Easy to balance and modify
- Supports multiple damage types and scaling sources
- Clear separation from card data

#### 3. **UI Rendering Approach**
**Decision**: UI Toolkit with custom card components
**Why**:
- Modern, responsive design
- Better performance for complex layouts
- Consistent with main menu implementation
- Future-proof technology

#### 4. **Combat Scene Integration**
**Decision**: UI Toolkit button integration over GameObject-based buttons
**Why**:
- Consistent rendering within UI system
- Better z-index control
- Integrated with existing UI structure
- Easier maintenance

### Systems Implemented

#### 1. **Card Data Structure**
```csharp
[System.Serializable]
public class Card
{
    public string cardName;
    public int manaCost;
    public CardType cardType;
    public float baseDamage;
    public float baseGuard;
    public bool isAoE;
    public int aoeTargets;
    public DamageScaling damageScaling;
    public bool scalesWithMeleeWeapon;
    public bool scalesWithProjectileWeapon;
    public bool scalesWithSpellWeapon;
    public string ifDiscarded;
    public string dualWieldEffect;
}
```
**Purpose**: Complete card data structure with all necessary properties
**Unity Concept**: Serializable for potential save system integration

#### 2. **Damage Calculation System**
```csharp
public static class DamageCalculator
{
    public static float CalculateCardDamage(Card card, Character character, Weapon weapon);
    public static float CalculateGuardAmount(Card card, Character character);
    public static float CalculateWeaponDamage(Weapon weapon, Character character);
}
```
**Purpose**: Centralized damage calculations with stat scaling
**Features**:
- Strength, Dexterity, Intelligence scaling
- Weapon damage integration
- Critical strike calculations
- Damage type support (Physical, Fire, Cold, Lightning, Chaos)

#### 3. **Combat Manager (Singleton)**
**Key Features**:
- Turn management
- Card playing logic
- Enemy targeting
- Combat state tracking
- Event system for UI updates

**Unity Concepts Used**:
- `DontDestroyOnLoad()`: Persists combat state
- Event system: `OnCardPlayed`, `OnTurnEnded`, `OnCombatEnded`
- Singleton pattern: Global combat access

#### 4. **Custom Card UI Component**
```csharp
public class CustomCard : VisualElement
{
    public Card cardData { get; private set; }
    public bool isUsable { get; private set; }
    public bool isSelected { get; private set; }
    
    public void SetCardData(Card card, Character character);
    public void UpdateCardDisplay(Character character);
    public void SetSelected(bool selected);
}
```
**Purpose**: Reusable card UI component with dynamic updates
**Features**:
- Dynamic damage/guard value calculation
- Usability checking
- Hover effects and selection states
- Real-time updates based on character stats

#### 5. **Combat UI System**
**Key Components**:
- **CombatUI.cs**: Main UI controller with event handling
- **CombatSceneUI.uxml**: UI structure with status bars, character sections, card hand
- **CombatSceneUI.uss**: Comprehensive styling with card fanning effects
- **CardTemplate.uxml/.uss**: Individual card styling and layout

**UI Features**:
- Real-time health/mana bars
- Dynamic enemy targeting
- Card hand with fanning effect
- Turn management buttons
- Combat log display

#### 6. **Scene Management Integration**
**CombatSceneManager**:
- Encounter data integration
- Scene navigation (ReturnToMap functionality)
- Victory/defeat state management
- UI state coordination

### Implementation Steps

#### Step 1: Core Card System
1. Created comprehensive `Card.cs` data structure
2. Implemented `DamageCalculator.cs` with stat scaling
3. Added weapon integration and damage type support
4. Created card usability checking system

#### Step 2: Combat Management
1. Created `CombatManager.cs` with singleton pattern
2. Implemented turn-based combat logic
3. Added card playing and targeting systems
4. Integrated event system for UI updates

#### Step 3: UI Toolkit Integration
1. Created `CustomCard.cs` component for dynamic card rendering
2. Implemented `CombatUI.cs` with comprehensive event handling
3. Designed `CombatSceneUI.uxml` with proper layout structure
4. Added `CombatSceneUI.uss` with card fanning and hover effects

#### Step 4: Scene Integration
1. Updated `CombatSceneManager.cs` with ReturnToMap functionality
2. Integrated UI Toolkit button over GameObject-based approach
3. Added proper scene flow testing and debugging

### Unity Concepts Explained

#### 1. **UI Toolkit Custom Components**
```csharp
public class CustomCard : VisualElement
{
    public CustomCard()
    {
        var cardTemplate = Resources.Load<VisualTreeAsset>("UI/Combat/CardTemplate");
        cardTemplate.CloneTree(this);
        InitializeElements();
    }
}
```
- Extend `VisualElement` for custom UI components
- Load UXML templates from Resources folder
- Use `CloneTree()` to instantiate template
- Query child elements with `Q<T>()`

#### 2. **Event System Integration**
```csharp
public System.Action<CustomCard> OnCardClicked;
cardBackground.RegisterCallback<ClickEvent>(OnCardClick);
```
- Custom events for component communication
- Unity's event system for input handling
- Proper event cleanup to prevent memory leaks

#### 3. **Dynamic UI Updates**
```csharp
public void UpdateCardDisplay(Character character)
{
    cardName.text = cardData.cardName;
    manaCost.text = cardData.manaCost.ToString();
    // ... dynamic calculations
}
```
- Real-time UI updates based on game state
- Character stat integration
- Damage calculation display

#### 4. **CSS-like Styling in UI Toolkit**
```css
.card-template {
    width: 200px;
    height: 280px;
    transition: all 0.3s ease;
    position: relative;
}

.card-hovered {
    margin-top: -20px;
    scale: 1.1;
}
```
- CSS-like styling with Unity-specific properties
- Transition effects for smooth animations
- Hover states and visual feedback

#### 5. **Singleton Pattern with Events**
```csharp
public static CombatManager Instance { get; private set; }
public event System.Action<Card> OnCardPlayed;
public event System.Action OnTurnEnded;
```
- Global access to combat state
- Event-driven architecture for loose coupling
- UI updates triggered by combat events

### File Structure
```
Assets/
├── Scripts/
│   ├── Combat/
│   │   ├── CombatManager.cs
│   │   ├── DamageCalculator.cs
│   │   └── ActiveAilment.cs
│   ├── Cards/
│   │   ├── Card.cs
│   │   ├── CardDamage.cs
│   │   └── TestMarauderDeck.cs
│   ├── UI/
│   │   ├── CombatUI.cs
│   │   └── CustomCard.cs
│   └── CombatSystem/
│       └── CombatSceneManager.cs
├── UI/
│   └── Combat/
│       ├── CombatSceneUI.uxml
│       ├── CombatSceneUI.uss
│       ├── CardTemplate.uxml
│       └── CardTemplate.uss
└── Resources/
    └── UI/Combat/
        └── CardTemplate.uxml
```

### Testing Checklist
- [ ] Combat scene loads with proper UI layout
- [ ] Cards display with correct damage/guard values
- [ ] Card hover effects work properly
- [ ] Card selection and targeting functions
- [ ] Turn management (End Turn, Draw Card) works
- [ ] Enemy targeting system functions
- [ ] ReturnToMap button navigates correctly
- [ ] Damage calculations scale with character stats
- [ ] Card usability checking works (mana cost, requirements)
- [ ] Combat log updates properly
- [ ] Health/mana bars update in real-time

### Future Considerations
1. **Card Effects**: Implement complex card effects and interactions
2. **Enemy AI**: Add intelligent enemy behavior and targeting
3. **Status Effects**: Implement buffs, debuffs, and ailment systems
4. **Card Animations**: Add visual feedback for card playing
5. **Sound Effects**: Integrate audio for card interactions
6. **Save System**: Persist combat state and deck composition
7. **Card Collection**: Implement deck building and card unlocking
8. **Balance Testing**: Extensive testing of damage calculations and card costs

### Lessons Learned
1. **Modular Design**: Separate data, logic, and presentation for maintainability
2. **Event-Driven Architecture**: Use events for loose coupling between systems
3. **UI Toolkit Performance**: Efficient for complex layouts but requires proper structure
4. **Singleton Management**: Essential for global state but requires careful event cleanup
5. **Dynamic Calculations**: Real-time updates based on character stats improve user experience
6. **Scene Integration**: Test scene flows from proper entry points (world map) rather than direct scene loading
7. **CSS-like Styling**: UI Toolkit provides powerful styling but requires Unity-specific syntax
8. **Component Reusability**: Custom UI components can be reused across different contexts

### Development Guidelines for Combat System

#### Card System Design
- Keep card data separate from behavior logic
- Use centralized calculation systems for consistency
- Implement proper scaling with character stats
- Design for extensibility (new card types, effects)

#### UI Integration
- Use UI Toolkit for modern, responsive design
- Implement proper event handling and cleanup
- Create reusable components for common UI elements
- Test UI flows thoroughly, especially scene transitions

#### Combat Logic
- Use singleton pattern for global combat state
- Implement event-driven architecture for loose coupling
- Separate turn management from card playing logic
- Design for easy testing and debugging

#### Performance Considerations
- Cache frequently accessed data (character stats, weapon info)
- Use object pooling for frequently created/destroyed objects
- Optimize UI updates to only refresh when necessary
- Monitor memory usage with singleton patterns

---

## Development Guidelines for Future Sessions

### Code Style
- Use centralized managers for system-wide functionality
- Always include proper error handling and null checks
- Document complex Unity concepts in comments
- Follow Unity naming conventions

### UI Management
- Connect UI elements through managers, not individual scripts
- Use Inspector fields for component references
- Implement proper event cleanup
- Test UI flows thoroughly

### Scene Management
- Use EncounterManager for all scene transitions
- Ensure scenes are added to Build Settings
- Test scene transitions in both directions
- Handle edge cases (missing scenes, null references)

### Documentation Standards
- Update this log after each significant feature
- Include Unity concepts explanations
- Document decisions and alternatives considered
- Maintain testing checklists
