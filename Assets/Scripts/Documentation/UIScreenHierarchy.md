# UI Screen Hierarchy - Unity Migration

## Screen Navigation Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                        MAIN MENU                                │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │   NEW GAME  │  │  CONTINUE   │  │  SETTINGS   │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    CHARACTER CREATION                           │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │    WITCH    │  │  MARAUDER   │  │   RANGER    │            │
│  │   (INT)     │  │   (STR)     │  │   (DEX)     │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │    THIEF    │  │   APOSTLE   │  │   BRAWLER   │            │
│  │ (DEX/INT)   │  │ (STR/INT)   │  │ (STR/DEX)   │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      MAIN GAME UI                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │  CHARACTER  │  │   EQUIPMENT │  │ DECK MANAGER│            │
│  │    STATS    │  │             │  │             │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │  PASSIVE    │  │   WORLD     │  │   ENCOUNTER │            │
│  │    TREE     │  │    MAP      │  │   BUTTONS   │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      COMBAT SCENE                              │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    COMBAT HUD                               │ │
│  │  [Health] [Energy Shield] [Turn] [Momentum] [Status Effects]│ │
│  └─────────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    PLAYER HAND                              │ │
│  │  [Card1] [Card2] [Card3] [Card4] [Card5] [End Turn]        │ │
│  └─────────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    ENEMY AREA                               │ │
│  │  [Enemy1] [Enemy2] [Enemy3] [Enemy4] [Enemy5]              │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Detailed Screen Breakdown

### 1. Main Menu Screen
**Purpose**: Entry point to the game
**UI Elements**:
- Game title/logo
- New Game button
- Continue Game button (if save exists)
- Settings button
- Exit button
- Background art

**Navigation**:
- New Game → Character Creation
- Continue → Main Game UI
- Settings → Settings Screen
- Exit → Quit game

### 2. Character Creation Screen
**Purpose**: Create new character with class selection
**UI Elements**:
- Class selection grid (6 classes)
- Character name input field
- Attribute preview (STR/DEX/INT distribution)
- Starter deck preview
- Character portrait preview
- Create character button
- Back button

**Navigation**:
- Create Character → Main Game UI
- Back → Main Menu

### 3. Main Game UI Screen
**Purpose**: Central hub for all game systems
**UI Elements**:
- Character stats panel
- Equipment panel
- Deck manager panel
- Passive tree panel
- World map panel
- Encounter buttons (Where Night First Fell, etc.)
- Settings button
- Save button

**Navigation**:
- Character Stats → Character Stats Screen
- Equipment → Equipment Screen
- Deck Manager → Deck Management Screen
- Passive Tree → Passive Tree Screen
- World Map → World Map Screen
- Encounter Buttons → Combat Scene

### 4. Character Stats Screen
**Purpose**: Display and manage character statistics
**UI Elements**:
- Character portrait
- Basic info (name, class, level)
- Attribute display (STR/DEX/INT)
- Derived stats (Health, Damage, etc.)
- Experience bar
- Attribute point allocation (if applicable)
- Equipment summary
- Back button

**Navigation**:
- Back → Main Game UI

### 5. Equipment Screen
**Purpose**: Manage character equipment and inventory
**UI Elements**:
- Equipment slots (weapon, armor, accessories)
- Inventory grid
- Item details panel
- Stat changes preview
- Equip/unequip buttons
- Item comparison
- Back button

**Navigation**:
- Back → Main Game UI

### 6. Deck Management Screen
**Purpose**: Build and manage card deck
**UI Elements**:
- Current deck display
- Card collection
- Card details panel
- Add/remove card buttons
- Deck size indicator
- Filter options (by type, rarity)
- Search functionality
- Back button

**Navigation**:
- Back → Main Game UI

### 7. Passive Tree Screen
**Purpose**: Allocate passive skill points
**UI Elements**:
- Board grid display
- Node connections
- Point allocation interface
- Stat preview
- Board selection
- Keystone display
- Back button

**Navigation**:
- Back → Main Game UI

### 8. World Map Screen
**Purpose**: Navigate game world and areas
**UI Elements**:
- Area nodes
- Connection lines
- Completion indicators
- Area information panel
- Progression tracking
- Back button

**Navigation**:
- Area Nodes → Combat Scene
- Back → Main Game UI

### 9. Combat Scene
**Purpose**: Turn-based card combat
**UI Elements**:
- Combat HUD (health, turn, status effects)
- Player hand (cards)
- Enemy display
- Action buttons (end turn)
- Card play interface
- Victory/defeat screens
- Return to map button

**Navigation**:
- Victory/Defeat → Loot Screen
- Return to Map → Main Game UI

## UI Component Hierarchy

### Main Menu Components
```
MainMenuCanvas
├── Background
├── Title
├── ButtonContainer
│   ├── NewGameButton
│   ├── ContinueButton
│   ├── SettingsButton
│   └── ExitButton
└── VersionInfo
```

### Character Creation Components
```
CharacterCreationCanvas
├── Background
├── Title
├── ClassSelectionGrid
│   ├── WitchButton
│   ├── MarauderButton
│   ├── RangerButton
│   ├── ThiefButton
│   ├── ApostleButton
│   └── BrawlerButton
├── CharacterInfoPanel
│   ├── NameInput
│   ├── AttributePreview
│   └── StarterDeckPreview
└── ButtonContainer
    ├── CreateButton
    └── BackButton
```

### Main Game UI Components
```
MainGameUICanvas
├── Background
├── TopPanel
│   ├── CharacterStatsButton
│   ├── EquipmentButton
│   └── DeckManagerButton
├── CenterPanel
│   ├── PassiveTreeButton
│   ├── WorldMapButton
│   └── EncounterButtons
├── BottomPanel
│   ├── SettingsButton
│   └── SaveButton
└── StatusPanel
    ├── HealthBar
    ├── ExperienceBar
    └── LevelDisplay
```

### Combat Scene Components
```
CombatCanvas
├── CombatHUD
│   ├── PlayerHealthBar
│   ├── EnergyShieldBar
│   ├── TurnIndicator
│   ├── MomentumDisplay
│   └── StatusEffectsPanel
├── EnemyArea
│   ├── Enemy1
│   ├── Enemy2
│   ├── Enemy3
│   └── Enemy4
├── PlayerHand
│   ├── Card1
│   ├── Card2
│   ├── Card3
│   ├── Card4
│   └── Card5
├── ActionButtons
│   ├── EndTurnButton
│   └── ReturnToMapButton
└── VictoryDefeatPanel
    ├── VictoryScreen
    └── DefeatScreen
```

## Unity UI Implementation Notes

### Canvas Setup
- Use **Screen Space - Overlay** for main UI
- Use **Screen Space - Camera** for combat UI
- Set proper sorting order for layering

### UI Toolkit vs Legacy UI
- **UI Toolkit**: For complex, data-driven UI (deck management, inventory)
- **Legacy UI**: For simple, static UI (buttons, basic panels)

### Responsive Design
- Use **Anchor Presets** for responsive layouts
- Implement **Canvas Scaler** for different screen sizes
- Test on various aspect ratios

### Performance Considerations
- Use **Object Pooling** for frequently created UI elements
- Implement **Lazy Loading** for large lists
- Optimize **UI updates** to minimize redraws

### Navigation System
- Use **SceneManager** for major screen transitions
- Use **Panel switching** for sub-screens within main UI
- Implement **Back button** functionality consistently

This hierarchy provides a clear structure for implementing your UI system in Unity while maintaining the complexity and depth of your original React implementation.
