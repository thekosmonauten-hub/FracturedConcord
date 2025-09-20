# Passive Tree Board Creation Guide

## Overview

This guide explains how to create passive tree boards as ScriptableObjects in Unity, allowing you to design and configure boards in the inspector.

---

## 🎯 Step 1: Understanding ScriptableObject Boards

### **What is a ScriptableObject Board?**

A ScriptableObject board is a Unity asset that contains:
- **Board Configuration**: Size, theme, limits
- **Node Data**: All nodes with their positions, stats, and requirements
- **Extension Points**: Connection points for other boards
- **Visual Settings**: Colors, backgrounds, etc.

### **Benefits of ScriptableObject Boards**

1. **Inspector Editing**: Configure boards visually in Unity
2. **Asset Management**: Boards are saved as .asset files
3. **Reusability**: Same board can be used in multiple places
4. **Version Control**: Boards can be tracked in git
5. **Runtime Loading**: Boards can be loaded dynamically

---

## 🚀 Step 2: Creating Your First Board

### **Method 1: Using the Create Menu**

1. **Right-click in Project window**
2. **Select Create → Passive Tree → Passive Board**
3. **Name your board** (e.g., "CoreBoard")
4. **Save in Assets/Resources/PassiveTree/**

### **Method 2: Using Context Menu**

1. **Select your board ScriptableObject**
2. **Right-click on the component**
3. **Choose "Setup Core Board"** (for core boards)
4. **Or configure manually** in the inspector

---

## ⚙️ Step 3: Board Configuration

### **Basic Board Settings**

```csharp
// In the inspector, set these values:
Board Data:
- ID: "core_board"
- Name: "Core Board"
- Description: "The central starting board"
- Theme: Utility
- Size: 7x7 (rows x columns)
- Max Points: 15

Board Configuration:
- Is Core Board: true (for starting board)
- Is Keystone Board: false
```

### **Board Themes**

Choose from these themes:
- **Fire**: Fire damage and burning effects
- **Cold**: Cold damage and freezing effects
- **Lightning**: Lightning damage and shock effects
- **Chaos**: Chaos damage and corruption
- **Physical**: Physical damage and strength
- **Life**: Health and regeneration
- **Armor**: Armor and defense
- **Evasion**: Evasion and dodge
- **Critical**: Critical strikes and accuracy
- **Speed**: Movement and attack speed
- **Utility**: General utility effects
- **Elemental**: Mixed elemental effects
- **Keystone**: Build-defining effects

---

## 🎮 Step 4: Adding Nodes to Your Board

### **Using Context Menu (Quick Method)**

1. **Select your board ScriptableObject**
2. **Right-click on the component**
3. **Choose "Add Test Node"**
4. **This adds a sample node at position (4, 3)**

### **Manual Node Configuration**

In the inspector, expand the **Board Data** section:

```csharp
// Example node configuration:
Node Identity:
- ID: "strength_node"
- Name: "Strength"
- Description: "+2 Strength"

Position:
- Row: 2
- Column: 3

Node Type:
- Type: Small (1 cost)

Stats:
- Key: "strength"
- Value: 2.0

Progression:
- Max Rank: 3
- Current Rank: 0
- Cost: 1

Visual:
- Node Color: Red
- Node Icon: (assign sprite)
```

### **Node Types and Costs**

| Type | Cost | Description | Use Case |
|------|------|-------------|----------|
| **Main** | 0 | Starting point | Core board center |
| **Extension** | 0 | Board connection | Extension points |
| **Notable** | 2 | Powerful passive | Major bonuses |
| **Travel** | 1 | Attribute nodes | Basic stats |
| **Small** | 1 | Minor bonuses | Small stat increases |
| **Keystone** | 5 | Build-defining | Major gameplay changes |

---

## 🔗 Step 5: Configuring Extension Points

### **Extension Point Setup**

Extension points allow boards to connect to other boards:

```csharp
// Example extension point configuration:
Identity:
- ID: "core_ext_right"
- Position: Row 3, Column 6 (right edge)

Connection Settings:
- Available Boards: ["fire_board", "cold_board", "lightning_board"]
- Max Connections: 1
- Current Connections: 0

Visual:
- Connection Color: Yellow
- Connection Icon: (assign sprite)
```

### **Common Extension Point Positions**

For a 7x7 board:
- **Right**: (3, 6) - Connects to elemental boards
- **Left**: (3, 0) - Connects to physical/chaos boards
- **Top**: (0, 3) - Connects to life/defense boards
- **Bottom**: (6, 3) - Connects to speed/critical boards
- **Corners**: (0, 0), (0, 6), (6, 0), (6, 6) - Special connections

---

## 🎨 Step 6: Visual Configuration

### **Board Visuals**

```csharp
Board Visual Settings:
- Board Color: White (or theme color)
- Board Background: (assign sprite)
```

### **Node Visuals**

```csharp
Node Visual Settings:
- Node Color: Based on type/theme
- Node Icon: (assign sprite)
```

### **Color Coding Suggestions**

- **Main Nodes**: Green
- **Extension Points**: Yellow
- **Fire Nodes**: Red/Orange
- **Cold Nodes**: Blue/Cyan
- **Lightning Nodes**: Yellow/White
- **Physical Nodes**: Brown/Gray
- **Life Nodes**: Green
- **Armor Nodes**: Gray/Silver
- **Critical Nodes**: Purple
- **Utility Nodes**: White

---

## 🔧 Step 7: Advanced Board Creation

### **Creating Extension Boards**

1. **Create new ScriptableObject board**
2. **Set theme** (e.g., Fire, Cold, etc.)
3. **Configure size** (usually 7x7)
4. **Add themed nodes**
5. **Set up extension points** for further connections

### **Creating Keystone Boards**

1. **Create new ScriptableObject board**
2. **Set theme to Keystone**
3. **Set size to 1x3** (extension + keystone + notable)
4. **Add powerful keystone node**
5. **Add supporting notable node**

### **Example: Fire Board Creation**

```csharp
// Fire Board Configuration:
Board Data:
- ID: "fire_board"
- Name: "Infernal Mastery"
- Description: "Master the destructive power of fire"
- Theme: Fire
- Size: 7x7
- Max Points: 15

// Add fire-themed nodes:
- Fire Damage Node: +10% Fire Damage
- Burning Node: +15% Burning Damage
- Ignite Node: +20% Chance to Ignite
- Fire Penetration: +10% Fire Penetration
- Elemental Focus: +15% Elemental Damage
```

---

## 🧪 Step 8: Testing Your Board

### **Validation Tools**

1. **Select your board ScriptableObject**
2. **Right-click → "Validate Board"**
3. **Check console for validation results**

### **Common Validation Checks**

- ✅ Board ID is set
- ✅ Board name is set
- ✅ Board is properly initialized
- ✅ Starting node exists (for core boards)
- ✅ Extension points are configured
- ✅ Node positions are valid

### **Testing in Scene**

1. **Create PassiveTreeScene**
2. **Add PassiveTreeManager**
3. **Assign your board to the manager**
4. **Run the scene and test node allocation**

---

## 📁 Step 9: File Organization

### **Recommended Structure**

```
Assets/
├── Resources/
│   └── PassiveTree/
│       ├── CoreBoard.asset
│       ├── FireBoard.asset
│       ├── ColdBoard.asset
│       ├── LightningBoard.asset
│       ├── PhysicalBoard.asset
│       ├── LifeBoard.asset
│       ├── ArmorBoard.asset
│       ├── EvasionBoard.asset
│       ├── CriticalBoard.asset
│       ├── SpeedBoard.asset
│       ├── UtilityBoard.asset
│       └── KeystoneBoards/
│           ├── FireKeystoneBoard.asset
│           ├── ColdKeystoneBoard.asset
│           └── ...
└── Scripts/
    └── Data/
        └── PassiveTree/
            ├── PassiveBoardScriptableObject.cs
            ├── PassiveNode.cs
            ├── ExtensionPoint.cs
            └── ...
```

---

## 🎯 Step 10: Integration with PassiveTreeManager

### **Assigning Boards to Manager**

1. **Select PassiveTreeManager in scene**
2. **In inspector, find "Passive Tree Data"**
3. **Assign your core board to "Core Board"**
4. **Add extension boards to "Extension Boards"**
5. **Add keystone boards to "Keystone Boards"**

### **Runtime Board Loading**

```csharp
// Load boards at runtime
var coreBoard = Resources.Load<PassiveBoardScriptableObject>("PassiveTree/CoreBoard");
var fireBoard = Resources.Load<PassiveBoardScriptableObject>("PassiveTree/FireBoard");

// Assign to manager
passiveTreeManager.passiveTree.coreBoard = coreBoard.GetBoardData();
passiveTreeManager.passiveTree.extensionBoards["fire_board"] = fireBoard.GetBoardData();
```

---

## 🆘 Troubleshooting

### **Common Issues**

1. **Board not showing in inspector**:
   - Check that the ScriptableObject inherits from ScriptableObject
   - Verify the CreateAssetMenu attribute is correct

2. **Nodes not appearing**:
   - Ensure board is initialized (call InitializeBoard())
   - Check node positions are within board bounds
   - Verify nodes are properly added to the board

3. **Extension points not working**:
   - Check extension point positions are valid
   - Verify available boards list is populated
   - Ensure max connections is set correctly

4. **Stats not calculating**:
   - Verify node stats dictionary is populated
   - Check that nodes are properly allocated
   - Ensure PassiveTreeManager is recalculating stats

### **Debug Tools**

- **Validate Board**: Right-click → "Validate Board"
- **Setup Core Board**: Right-click → "Setup Core Board"
- **Add Test Node**: Right-click → "Add Test Node"
- **Console Logs**: Check for validation and setup messages

---

## 📋 Board Creation Checklist

- [ ] Create ScriptableObject board asset
- [ ] Set board ID, name, and description
- [ ] Choose appropriate theme
- [ ] Set board size (usually 7x7)
- [ ] Configure max points
- [ ] Add starting node (for core boards)
- [ ] Add themed nodes with appropriate stats
- [ ] Configure extension points
- [ ] Set visual colors and icons
- [ ] Validate board configuration
- [ ] Test in scene with PassiveTreeManager
- [ ] Save board asset in Resources/PassiveTree/

---

This guide provides everything you need to create and configure passive tree boards as ScriptableObjects in Unity. The ScriptableObject approach makes board creation visual and intuitive while maintaining the flexibility of the modular system.
