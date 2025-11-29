# Development Log - Dexiled Unity Deckbuilder ARPG

## Project Overview
A deckbuilder ARPG with Path of Exile influences, built in Unity.

## Development Philosophy
- **Centralized Management**: Use singleton managers for core systems
- **UI Consistency**: Connect UI elements through centralized managers rather than individual scripts
- **Step-by-Step Implementation**: Build systems incrementally with proper documentation
- **Unity Best Practices**: Follow Unity conventions and patterns

---

## Session 15: Spell Tag Focus Charges

### Date: 2025-11-14
### Goal: Ensure Spell-tagged cards fuel Focus charges even when their card type is Attack/Guard.

### Key Decisions Made

1. **Tag-First Routing for Focus Meter**
   - Added an explicit Spell tag check inside `CombatDeckManager.ProcessSpeedMeters`.
   - Rationale: Designers flag Spell cards via tags while reusing Attack card types for damage templating; the meter must follow the tag to avoid mixed signals.

2. **Helper for Case-Insensitive Tag Lookups**
   - Introduced `CardHasTag` utility plus a `SpellTagName` constant to avoid scattering string comparisons throughout combat systems.
   - Rationale: Keeps future meter hooks (e.g., Momentum, Totem) consistent and reduces typo-related bugs.

### Systems Updated

1. `CombatDeckManager`
   - Added `using System;`, `SpellTagName` constant, and `CardHasTag`.
   - `ProcessSpeedMeters` now routes Spell-tagged cards to `AddFocusProgress()` regardless of card type before falling back to the existing Attack/Skill/Power logic.
   - Prevents double dipping by returning early once a Spell tag is detected.

2. Documentation
   - Logged this change here so future developers understand why tag precedence matters for Speed Meter tuning.

---

## Session 16: Focus Effects + Card Pooling Fixes

### Date: 2025-11-14
### Goal: Make pooled card visuals stay clickable and let Focus charge bonuses affect AoE spells.

### Key Decisions Made

1. **Centralize Interaction State**
   - Reused `SetCardInteractable` whenever a card is disabled/enabled instead of raw `Button`/`CanvasGroup` tweaks.
   - Forcing the `Button` component to stay enabled prevents pooled cards from coming back with their `Button` script disabled (the "Button GameObject" issue after 8–10 draws).

2. **Reset Hover Components When Reusing Cards**
   - `CardRuntimeManager` now reassigns `CardHoverEffect.animationManager` and re-enables the component every time a pooled card is spawned.
   - Ensures hover/tooltip behaviour survives the pooling lifecycle.

3. **Apply Focus Modifiers to AoE Damage**
   - Batched AoE damage now runs through `CombatDeckManager.ApplyDamageModifier`, so Focus «DoubleDamage» (and future global multipliers) affect spell nukes just like single-target attacks.

### Systems Updated

1. `CombatDeckManager`
   - Replaced bespoke button/canvas disabling with a single `SetCardInteractable(cardObj, false)` call when a card starts its play animation.
   - `SetCardInteractable` guarantees pooled buttons remain enabled while still toggling `interactable`/raycasts, eliminating permanently-disabled buttons on future draws.

2. `CardRuntimeManager`
   - All creation paths (`CreateCardFromData`, `CreateCardFromCardData`, `CreateCardFromCardDataExtended`) now re-enable `CardHoverEffect` and refresh its animation manager on spawn.

3. `CardEffectProcessor`
   - `ApplyAoECard` pipes its calculated damage through `CombatDeckManager.ApplyDamageModifier`, so Focus charges finally double AoE spell damage (and any future shared modifiers) before hitting each enemy.

---

## Session 17: Enemy Energy & Wave Preview

### Date: 2025-11-14
### Goal: Surface the enemy energy resource on HUD, hook Chill/Slow drains, and preview wave compression on encounter buttons.

### Key Decisions Made

1. **Data-Driven Enemy Energy**
   - `EnemyData` exposes opt-in energy stats (max pool, regen, drain multipliers) so bosses/elites can toggle the system without per-enemy code.
   - `Enemy` stores the pool, raises `OnEnergyChanged`, and exposes helpers for drains/regeneration.

2. **UI Integration**
   - `EnemyCombatDisplay` auto-creates a teal bar under each enemy, subscribes to the new energy event, and hides the bar when energy isn’t used.
   - `StatusEffectManager` drains energy automatically whenever Chill or Slow is applied, so any future source of those ailments benefits without bespoke logic.

3. **Encounter Preview**
   - `EncounterButton` now renders a `WavePreview` label that shows `base → compressed` wave counts based on the character’s current movement-speed multiplier.
   - Compression clamps at a minimum of one wave, so boss previews remain honest even with absurd run speed.

### Systems Updated

1. `Enemy`/`EnemyData`
   - Added energy configuration, drain multipliers, and events.

2. `EnemyCombatDisplay`
   - Auto-builds the energy bar (fallback prefab) and subscribes/unsubscribes from energy changes alongside stack events.

3. `StatusEffectManager`
   - New Chill/Slow cases call `Enemy.ApplyEnergyDrainFromStatus`, keeping the logic close to where status effects are applied.

4. `EncounterButton`
   - Adds `wavePreviewLabel`, runtime creation helper, and `UpdateWavePreview()` so the main UI shows how movement speed compresses encounters.

5. Documentation
   - `COMBAT_SPEED_METERS.md` now reflects charge-consumption rules, energy UI, and wave preview behavior.

---

## Session 18: Warrant Board Loadouts

### Date: 2025-11-14
### Goal: Give players multiple warrant board "pages" with save/load helpers so they can swap layouts freely.

### Key Decisions Made

1. **Page-Based State Controller**
   - Introduced `WarrantBoardStateController` to own a list of board pages (loadouts) and expose helpers for assigning/removing warrants per socket.
   - Rationale: Keeps board state logic decoupled from the visual builder while enabling designers to add future UI flows (e.g., tabs, thumbnails) without reworking data.

2. **Serializable Save Slots**
   - Added `WarrantBoardSaveData` + JSON helpers so the entire set of pages can be stored in PlayerPrefs (or any persistence layer) with a single call.
   - Rationale: Designers requested multi-page boards per character; having a ready-to-serialize structure keeps integration with the broader save system straightforward.

3. **Socket Validation via Graph Definition**
   - The controller inspects the assigned `WarrantBoardGraphDefinition` and only accepts assignments for `Socket` / `SpecialSocket` nodes.
   - Rationale: Prevents mistakenly binding warrants to effect nodes or anchors, aligning with the "3-node influence" rule.

### Systems Updated

1. `WarrantBoardStateController`
   - Fields for default page count, naming, and PlayerPrefs key.
   - APIs: `EnsurePageCount`, `SetActivePage`, `TryAssignWarrant`, `TryRemoveWarrant`, `DuplicateActivePage`, `ToJson`, `LoadFromJson`, plus context-menu save/load.
   - Nested serializable classes for page data and socket assignments.

2. Documentation
   - `MasterChecklist.md` now marks board geometry + multi-page state as complete to keep high-level tracking accurate.

---

## Session 19: Warrant Drag-and-Drop Prefabs

### Date: 2025-11-15
### Goal: Implement the prefab + scripting work outlined in `WarrantPrefabSetup.md` so sockets support Option 1 (CombatSceneManager-aligned) drag/drop with the existing `WarrantBoardStateController`.

### Key Decisions Made

1. **Dedicated Socket View**
   - Authored `WarrantSocketView` (extends `WarrantNodeView`) to own highlight toggles, drag ghost spawning, and assignment calls into `WarrantBoardStateController`.
   - Rationale: Keeps drag/drop logic localized to a reusable component that any board builder can spawn, mirroring the Option 1 integration pattern.

2. **Payload Contract for Future Sources**
   - Introduced `IWarrantDragPayload` so lockers, rewards, or sockets can exchange warrant IDs + icons without direct type checks.
   - Rationale: Prevents tight coupling and makes it trivial for future inventory sources to participate in the same drag/drop flow.

3. **Icon Lookup via ScriptableObject**
   - Added `WarrantIconLibrary` to map warrant IDs to sprites; sockets render icons when present and fall back gracefully when definitions are missing.
   - Rationale: Designers can curate libraries per scene/board without hardcoding resources in scripts.

### Systems Updated

1. `WarrantNode_Socket.prefab`
   - Swapped to `WarrantSocketView`, added highlight/icon references, and bundled a `CanvasGroup` needed for drag transparency.

2. `WarrantBoardGraphBuilder`
   - New serialized references for `boardStateController`, `iconLibrary`, and a distinct special-socket color; sockets now receive dependencies immediately after instantiation.

3. Documentation
   - `WarrantPrefabSetup.md` captures the current prefab/script state plus icon-library guidance.
   - `MasterChecklist.md` marks the prefab + drag/drop tasks complete.

4. Shared Contracts
   - `IWarrantDragPayload` defines the drag payload API.
   - `WarrantIconLibrary` centralizes warrant→sprite lookups for any UI in the scene.

---

## Session 20: Warrant Locker Inventory & Free Swapping

### Date: 2025-11-15
### Goal: Implement a sliding locker panel with grid layout, filtering, and free swapping between locker and sockets.

### Key Decisions Made

1. **Sliding Panel Pattern**
   - Authored `WarrantLockerPanelManager` following the `CharacterStatsPanelManager` pattern for consistency.
   - Uses LeanTween for smooth slide animations from off-screen (configurable direction: left/right/top/bottom).
   - Rationale: Reuses established UI patterns, keeps code maintainable, and provides smooth UX.

2. **Grid-Based Inventory Display**
   - Created `WarrantLockerGrid` using Unity's `GridLayoutGroup` for dynamic warrant item generation.
   - `WarrantLockerItem` implements `IWarrantDragPayload` so locker items can be dragged directly into sockets.
   - Rationale: Consistent with existing inventory grid patterns in the codebase, leverages Unity's built-in layout system.

3. **Client-Side Filtering**
   - `WarrantLockerFilterController` filters by rarity (toggle group) and modifiers (TMP_Dropdown).
   - Auto-populates modifier dropdown from available warrants in inventory.
   - Rationale: Fast, responsive filtering without server calls; modifier list is built dynamically from player's actual warrants.

4. **Free Swapping Flow**
   - Warrants can be dragged from locker → socket (assigns) or socket → locker (clears assignment).
   - `WarrantLockerDropZone` handles returns from sockets; `WarrantSocketView.ClearAssignmentForLockerReturn()` clears socket state.
   - Rationale: No consumption model - warrants are freely swappable, making experimentation easy for players.

### Systems Updated

1. **New Components**
   - `WarrantLockerPanelManager`: Sliding panel animation and toggle management.
   - `WarrantLockerItem`: Individual warrant item with drag/drop support.
   - `WarrantLockerGrid`: Grid layout manager for warrant items.
   - `WarrantLockerFilterController`: Rarity and modifier filtering.
   - `WarrantLockerDropZone`: Drop zone for returning warrants from sockets.

2. **WarrantSocketView**
   - Added `ClearAssignmentForLockerReturn()` method for free swapping support.
   - Sockets can now accept drops from locker items and return warrants to locker.

3. **Documentation**
   - `WarrantPrefabSetup.md` now includes complete locker panel setup guide with hierarchy and integration steps.
   - `MasterChecklist.md` marks locker inventory task as complete.

### Integration Notes

- Locker panel prefab structure documented in `WarrantPrefabSetup.md` with full hierarchy.
- Filter controller uses TMP_Dropdown (consistent with DeckBuilderUI pattern).
- Rarity colors configurable per `WarrantLockerItem` instance.
- Panel slides in from left by default (configurable in inspector).
- Drop zone should cover entire locker panel area for easy dropping.

---

## Session 21: Warrant Tooltips & Effect Node Surfacing

### Date: 2025-11-15
### Goal: Hook warrant sockets/effect nodes into the shared tooltip stack so hovering any node reveals its active modifiers (supporting 1–8 affixes dynamically).

### Key Decisions Made

1. **Reuse ItemTooltipManager**
   - Added warrant-aware overloads instead of inventing a parallel tooltip system, keeping Option 1 integrations consistent.
   - Provides a fallback runtime tooltip if designers haven’t authored a bespoke prefab yet.

2. **Data-Driven Tooltip Rendering**
   - Authored `WarrantTooltipData`, `WarrantTooltipUtility`, and `WarrantTooltipView` to convert `WarrantDefinition` + modifiers into sections/lines.
   - Supports any number of modifiers per section with automatic truncation + “…and more” messaging past eight entries.

3. **Effect Nodes Gain Their Own View**
   - Replaced the base `WarrantNodeView` on effect prefabs with `WarrantEffectView`.
   - Effect nodes inspect runtime graph connections + `WarrantBoardStateController` assignments to aggregate nearby warrant definitions and display the combined impact.

### Systems Updated

1. `ItemTooltipManager`
   - New `warrantTooltipPrefab` slot, runtime fallback factory, and public `ShowWarrantTooltip` APIs (definition/data + pointer overloads).
   - All tooltip spawners now funnel through `HideTooltip()` before instantiating to prevent duplicates.

2. `WarrantSocketView`
   - Receives `ItemTooltipManager` + `WarrantLockerGrid` references from the graph builder.
   - Hovering shows the assigned warrant’s modifiers; drag/drop/clear events auto-hide or refresh the tooltip to avoid stale ghosts.

3. `WarrantEffectView`
   - Extends `WarrantNodeView` and surfaces aggregated modifiers when hovering effect nodes.
   - Uses the runtime graph binding plus locker definition lookup so tooltips keep working even after a warrant leaves the locker inventory.

4. `WarrantBoardGraphBuilder`
   - Binds each spawned node to its runtime graph entry and injects locker + tooltip references into sockets/effect nodes.
   - Adds a serialized `tooltipManager` field; assign it in scenes alongside `boardStateController`, `lockerGrid`, and `iconLibrary`.

5. `WarrantLockerGrid`
   - Maintains a definition catalog/lookup so tooltips can resolve any warrant ID at runtime (even if it’s not currently listed in the locker UI).
   - Provides helpers to remove consumed warrants and re-add them when sockets return items.

### Integration Notes

- `WarrantPrefabSetup.md` now documents the tooltip workflow, prefab expectations, and catalog requirements.
- Designers can optionally supply a custom `warrantTooltipPrefab`; otherwise the runtime-generated view provides a functional baseline.
- Make sure every scene wiring the board assigns the new `tooltipManager` field on `WarrantBoardGraphBuilder` to enable hover behavior.

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
