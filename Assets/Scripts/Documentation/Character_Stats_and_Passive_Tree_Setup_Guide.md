# Character Stats & Passive Tree Setup Guide

## Overview
This guide will help you set up the Character Stats page and Passive Tree system in your Unity project. These systems provide comprehensive character progression and customization features.

---

## 🎯 Character Stats Page Setup

### **Step 1: Create Character Stats Panel Structure**

1. **In MainGameUI scene**, locate the `CharacterStatsPanel` GameObject
2. **Add CharacterStatsController script** to the panel
3. **Create UI hierarchy** as follows:

```
CharacterPanel
├── Background (Image)
├── Header
│   ├── Title (TextMeshPro - "Character Stats")
│   └── CloseButton (Button)
├── CharacterInfo
│   ├── CharacterPortrait (Image)
│   ├── CharacterName (TextMeshPro)
│   ├── CharacterClass (TextMeshPro)
│   └── CharacterLevel (TextMeshPro)
├── ExperienceBar (Slider)
│   ├── Background (Image)
│   ├── Fill Area
│   │   └── Fill (Image)
│   ├── ExperienceText (TextMeshPro)
│   └── NextLevelText (TextMeshPro)
├── Attributes
│   ├── StrengthText (TextMeshPro)
│   ├── DexterityText (TextMeshPro)
│   └── IntelligenceText (TextMeshPro)
├── DerivedStats
│   ├── MaxHealthText (TextMeshPro)
│   ├── MaxManaText (TextMeshPro)
│   ├── MaxEnergyShieldText (TextMeshPro)
│   ├── AttackPowerText (TextMeshPro)
│   ├── DefenseText (TextMeshPro)
│   ├── CriticalChanceText (TextMeshPro)
│   └── CriticalMultiplierText (TextMeshPro)
├── CombatResources
│   ├── ManaRecoveryText (TextMeshPro)
│   ├── CardsDrawnText (TextMeshPro)
│   └── RelianceText (TextMeshPro)
├── DamageModifiers
│   ├── PhysicalDamageText (TextMeshPro)
│   ├── FireDamageText (TextMeshPro)
│   ├── ColdDamageText (TextMeshPro)
│   ├── LightningDamageText (TextMeshPro)
│   └── ChaosDamageText (TextMeshPro)
├── Resistances
│   ├── PhysicalResistanceText (TextMeshPro)
│   ├── FireResistanceText (TextMeshPro)
│   ├── ColdResistanceText (TextMeshPro)
│   ├── LightningResistanceText (TextMeshPro)
│   └── ChaosResistanceText (TextMeshPro)
└── EquipmentSummary
    ├── EquippedWeaponText (TextMeshPro)
    ├── EquippedArmorText (TextMeshPro)
    └── TotalEquipmentStatsText (TextMeshPro)
```

### **Step 2: Configure UI Elements**

#### **Character Info Section**
- **CharacterPortrait**: 64x64 pixels, assign character sprite
- **CharacterName**: Font size 18, bold
- **CharacterClass**: Font size 16, italic
- **CharacterLevel**: Font size 16

#### **Experience Bar**
- **Size**: 300x20 pixels
- **Fill Color**: Green gradient
- **Text**: Show current/required XP

#### **Attributes Section**
- **Strength**: Red color (0.8, 0.2, 0.2)
- **Dexterity**: Green color (0.2, 0.8, 0.2)
- **Intelligence**: Blue color (0.2, 0.2, 0.8)

#### **Stats Sections**
- **Font size**: 14
- **Layout**: Vertical list with proper spacing
- **Background**: Semi-transparent panel

### **Step 3: Assign Script References**

In the `CharacterStatsController` component:

#### **Character Information**
- **Character Portrait**: Assign CharacterPortrait Image
- **Character Name Text**: Assign CharacterName TextMeshPro
- **Character Class Text**: Assign CharacterClass TextMeshPro
- **Character Level Text**: Assign CharacterLevel TextMeshPro

#### **Experience Display**
- **Experience Slider**: Assign ExperienceBar Slider
- **Experience Text**: Assign ExperienceText TextMeshPro
- **Next Level Text**: Assign NextLevelText TextMeshPro

#### **Core Attributes**
- **Strength Text**: Assign StrengthText TextMeshPro
- **Dexterity Text**: Assign DexterityText TextMeshPro
- **Intelligence Text**: Assign IntelligenceText TextMeshPro

#### **Derived Stats**
- **Max Health Text**: Assign MaxHealthText TextMeshPro
- **Max Mana Text**: Assign MaxManaText TextMeshPro
- **Max Energy Shield Text**: Assign MaxEnergyShieldText TextMeshPro
- **Attack Power Text**: Assign AttackPowerText TextMeshPro
- **Defense Text**: Assign DefenseText TextMeshPro
- **Critical Chance Text**: Assign CriticalChanceText TextMeshPro
- **Critical Multiplier Text**: Assign CriticalMultiplierText TextMeshPro

#### **Combat Resources**
- **Mana Recovery Text**: Assign ManaRecoveryText TextMeshPro
- **Cards Drawn Text**: Assign CardsDrawnText TextMeshPro
- **Reliance Text**: Assign RelianceText TextMeshPro

#### **Damage Modifiers**
- **Physical Damage Text**: Assign PhysicalDamageText TextMeshPro
- **Fire Damage Text**: Assign FireDamageText TextMeshPro
- **Cold Damage Text**: Assign ColdDamageText TextMeshPro
- **Lightning Damage Text**: Assign LightningDamageText TextMeshPro
- **Chaos Damage Text**: Assign ChaosDamageText TextMeshPro

#### **Resistances**
- **Physical Resistance Text**: Assign PhysicalResistanceText TextMeshPro
- **Fire Resistance Text**: Assign FireResistanceText TextMeshPro
- **Cold Resistance Text**: Assign ColdResistanceText TextMeshPro
- **Lightning Resistance Text**: Assign LightningResistanceText TextMeshPro
- **Chaos Resistance Text**: Assign ChaosResistanceText TextMeshPro

#### **Equipment Summary**
- **Equipped Weapon Text**: Assign EquippedWeaponText TextMeshPro
- **Equipped Armor Text**: Assign EquippedArmorText TextMeshPro
- **Total Equipment Stats Text**: Assign TotalEquipmentStatsText TextMeshPro

### **Step 4: Configure Colors**

Set the color values in the `CharacterStatsController` script:
- **Strength Color**: Red (0.8, 0.2, 0.2)
- **Dexterity Color**: Green (0.2, 0.8, 0.2)
- **Intelligence Color**: Blue (0.2, 0.2, 0.8)

---

## 🎯 Passive Tree Setup

### **Step 1: Create Passive Tree Panel Structure**

1. **In MainGameUI scene**, locate the `PassiveTreePanel` GameObject
2. **Add PassiveTreeController script** to the panel
3. **Add PassiveTreeManager script** to a new GameObject named "PassiveTreeManager"
4. **Create UI hierarchy** as follows:

```
PassiveTreePanel
├── Background (Image)
├── Header
│   ├── Title (TextMeshPro - "Passive Tree")
│   └── CloseButton (Button)
├── TreeInfo
│   ├── AvailablePointsText (TextMeshPro)
│   ├── TotalAllocatedText (TextMeshPro)
│   └── BoardCountText (TextMeshPro)
├── BoardContainer (Transform)
│   └── [Boards will be created dynamically]
├── InfoPanel (GameObject)
│   ├── Background (Image)
│   ├── NodeNameText (TextMeshPro)
│   ├── NodeDescriptionText (TextMeshPro)
│   ├── NodeEffectsText (TextMeshPro)
│   ├── NodeRequirementsText (TextMeshPro)
│   └── PointCostText (TextMeshPro)
└── Controls
    ├── ResetTreeButton (Button)
    ├── AddPointsButton (Button)
    ├── SaveTreeButton (Button)
    └── LoadTreeButton (Button)
```

### **Step 2: Create Prefabs**

#### **Board Prefab**
Create a new GameObject named "BoardPrefab":
```
BoardPrefab
├── Background (Image)
├── NodeContainer (Transform)
└── LineContainer (Transform)
```

#### **Node Prefab**
Create a new GameObject named "NodePrefab":
```
NodePrefab
├── Background (Image) - Circle shape
├── Icon (Image) - Optional
├── NodeText (TextMeshPro)
└── Button (Button)
```

#### **Connection Line Prefab**
Create a new GameObject named "ConnectionLinePrefab":
```
ConnectionLinePrefab
└── LineRenderer
```

### **Step 3: Configure UI Elements**

#### **Tree Info Section**
- **Available Points**: Font size 16, bold
- **Total Allocated**: Font size 14
- **Board Count**: Font size 14

#### **Info Panel**
- **Background**: Semi-transparent panel
- **Node Name**: Font size 18, bold
- **Node Description**: Font size 14
- **Node Effects**: Font size 12, bullet points
- **Node Requirements**: Font size 12
- **Point Cost**: Font size 14, bold

#### **Control Buttons**
- **Reset Tree**: Red color, "Reset Tree" text
- **Add Points**: Green color, "Add 5 Points" text
- **Save Tree**: Blue color, "Save Tree" text
- **Load Tree**: Blue color, "Load Tree" text

### **Step 4: Assign Script References**

In the `PassiveTreeController` component:

#### **UI References**
- **Board Container**: Assign BoardContainer Transform
- **Board Prefab**: Assign BoardPrefab GameObject
- **Node Prefab**: Assign NodePrefab GameObject
- **Connection Line Prefab**: Assign ConnectionLinePrefab GameObject

#### **Info Panel**
- **Info Panel**: Assign InfoPanel GameObject
- **Node Name Text**: Assign NodeNameText TextMeshPro
- **Node Description Text**: Assign NodeDescriptionText TextMeshPro
- **Node Effects Text**: Assign NodeEffectsText TextMeshPro
- **Node Requirements Text**: Assign NodeRequirementsText TextMeshPro
- **Point Cost Text**: Assign PointCostText TextMeshPro

#### **Tree Info**
- **Available Points Text**: Assign AvailablePointsText TextMeshPro
- **Total Allocated Text**: Assign TotalAllocatedText TextMeshPro
- **Board Count Text**: Assign BoardCountText TextMeshPro

#### **Controls**
- **Reset Tree Button**: Assign ResetTreeButton Button
- **Add Points Button**: Assign AddPointsButton Button
- **Save Tree Button**: Assign SaveTreeButton Button
- **Load Tree Button**: Assign LoadTreeButton Button

### **Step 5: Configure Visual Settings**

Set the color values in the `PassiveTreeController` script:
- **Allocated Node Color**: Green (0, 1, 0)
- **Available Node Color**: Yellow (1, 1, 0)
- **Unavailable Node Color**: Gray (0.5, 0.5, 0.5)
- **Starting Node Color**: Blue (0, 0, 1)
- **Keystone Node Color**: Red (1, 0, 0)

---

## 🎯 Integration with Main Game UI

### **Step 1: Update UIManager**

The `UIManager` already has the necessary methods:
- `ShowCharacterStats()` - Shows Character Stats panel
- `ShowPassiveTree()` - Shows Passive Tree panel

### **Step 2: Button Connections**

Ensure the buttons in MainGameUI are connected:
- **Character Stats Button** → `UIManager.ShowCharacterStats()`
- **Passive Tree Button** → `UIManager.ShowPassiveTree()`

---

## 🎯 Testing and Debugging

### **Character Stats Testing**

1. **Test Level Up**: Right-click CharacterStatsController → Test Level Up
2. **Test Add Experience**: Right-click CharacterStatsController → Test Add Experience
3. **Test Attribute Increase**: Right-click CharacterStatsController → Test Attribute Increase

### **Passive Tree Testing**

1. **Add Points**: Right-click PassiveTreeManager → Add 5 Points
2. **Reset Tree**: Right-click PassiveTreeManager → Reset Tree
3. **Save Tree State**: Right-click PassiveTreeManager → Save Tree State
4. **Load Tree State**: Right-click PassiveTreeManager → Load Tree State

### **Integration Testing**

1. **Character Stats Integration**:
   - Equip items in Equipment Screen
   - Check if stats update in Character Stats
   - Level up character and verify stat changes

2. **Passive Tree Integration**:
   - Allocate nodes and check character stat changes
   - Save/load tree state
   - Test board connections and extensions

---

## 🎯 Advanced Features

### **Character Stats Features**

1. **Real-time Updates**: Stats update automatically when equipment changes
2. **Equipment Integration**: Shows equipped items and their stat bonuses
3. **Experience Tracking**: Visual experience bar with level progression
4. **Attribute Colors**: Color-coded attributes for easy identification

### **Passive Tree Features**

1. **Modular Board System**: Each board is self-contained with extension points
2. **Node Connections**: Visual connection lines between allocated nodes
3. **Requirement Validation**: Nodes check level and attribute requirements
4. **Tree Integrity**: Prevents deallocating nodes that would break the tree
5. **Save/Load System**: Persistent tree state across game sessions
6. **Visual Feedback**: Color-coded nodes based on availability and type

---

## 🎯 Best Practices

### **Performance Optimization**

1. **Event-Driven Updates**: Use events to update UI only when needed
2. **Object Pooling**: Consider pooling for frequently created/destroyed elements
3. **Batch Updates**: Update multiple UI elements in single frame

### **User Experience**

1. **Clear Visual Hierarchy**: Use consistent spacing and typography
2. **Intuitive Controls**: Click to allocate/deallocate nodes
3. **Informative Tooltips**: Show detailed node information on hover
4. **Visual Feedback**: Immediate response to user actions

### **Maintenance**

1. **Modular Design**: Each system is self-contained
2. **Clear Naming**: Use descriptive names for all UI elements
3. **Documentation**: Keep this guide updated
4. **Version Control**: Track changes in git

---

## 🎯 Troubleshooting

### **Common Issues**

1. **UI Not Updating**:
   - Check if CharacterManager exists in scene
   - Verify event subscriptions
   - Ensure prefab references are assigned

2. **Nodes Not Allocating**:
   - Check available points
   - Verify node requirements
   - Ensure node is connected to allocated node

3. **Stats Not Updating**:
   - Check EquipmentManager integration
   - Verify character data loading
   - Ensure event handlers are connected

### **Debug Tools**

- Use context menu options for testing
- Check Console for error messages
- Use Debug.Log statements for tracking
- Verify prefab assignments in inspector

---

*This guide should be updated as the systems evolve.*











