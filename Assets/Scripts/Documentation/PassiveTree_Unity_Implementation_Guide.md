# Passive Tree Unity Implementation Guide

## Overview

This guide provides step-by-step instructions for implementing the modular passive tree system in Unity, translated from the original React/TypeScript architecture.

## Architecture Translation

### TypeScript → C# Mapping

| TypeScript | C# | Purpose |
|------------|----|---------|
| `interface PassiveNode` | `class PassiveNode` | Individual node data |
| `interface PassiveBoard` | `class PassiveBoard` | Board with nodes grid |
| `interface ModularPassiveTree` | `class ModularPassiveTree` | Main tree container |
| `interface PlayerPassiveState` | `class PlayerPassiveState` | Player progress tracking |
| `interface BoardConnection` | `class BoardConnection` | Board connection metadata |

### Key Differences

1. **Serialization**: C# classes use `[System.Serializable]` for Unity inspector support
2. **Events**: Unity events instead of React state updates
3. **MonoBehaviour**: Manager classes inherit from MonoBehaviour for Unity lifecycle
4. **ScriptableObjects**: Board data can be stored as ScriptableObjects for easy editing

---

## 🎯 Phase 1: Core Data Structures (COMPLETED)

### ✅ Completed Files

1. **`PassiveNode.cs`** - Individual node with stats and allocation logic
2. **`ExtensionPoint.cs`** - Board connection points
3. **`PassiveBoard.cs`** - Complete board with 2D node grid
4. **`BoardConnection.cs`** - Board connection metadata
5. **`ModularPassiveTree.cs`** - Main tree container with limits
6. **`PlayerPassiveState.cs`** - Player progress tracking
7. **`PassiveTreeManager.cs`** - Main manager with events

### 🔧 Key Features Implemented

- **Node Allocation System**: Click-to-allocate with point costs
- **Board Connection System**: Extension points for modular boards
- **Stat Calculation**: Automatic stat aggregation from allocated nodes
- **State Management**: Player progress persistence
- **Event System**: Real-time updates for UI components
- **Performance Optimization**: Cached stats with dirty flags

---

## 🚀 Phase 2: Scene Setup & UI Foundation

### **Step 1: Create PassiveTreeScene**

1. **Create New Scene**:
   - File → New Scene → 2D
   - Save as "PassiveTreeScene"

2. **Scene Hierarchy**:
   ```
   PassiveTreeScene
   ├── PassiveTreeManager (Empty GameObject)
   ├── Canvas
   │   ├── PassiveTreeUI
   │   │   ├── CoreBoardContainer
   │   │   ├── ExtensionBoardsContainer
   │   │   ├── BoardSelectionModal
   │   │   └── StatsPanel
   │   └── EventSystem
   └── Camera
   ```

### **Step 2: Setup PassiveTreeManager**

1. **Add PassiveTreeManager**:
   - Create empty GameObject named "PassiveTreeManager"
   - Add `PassiveTreeManager.cs` component
   - Assign passive tree data (we'll create this next)

2. **Configure Settings**:
   - Enable "Auto Recalculate Stats"
   - Enable "Debug Logging" for development

### **Step 3: Create Board Data ScriptableObjects**

1. **Create Core Board**:
   ```
   Assets/Resources/PassiveTree/
   ├── CoreBoard.asset
   ├── FireBoard.asset
   ├── ColdBoard.asset
   └── ... (other boards)
   ```

2. **Board Creation Process**:
   - Right-click in Project → Create → ScriptableObject
   - Select "PassiveBoard" type
   - Configure nodes, extension points, and theme

---

## 🎨 Phase 3: UI Components

### **Core Board UI Component**

```csharp
public class BoardUI : MonoBehaviour
{
    [Header("Board Data")]
    public PassiveBoard boardData;
    
    [Header("UI References")]
    public Transform nodeContainer;
    public GameObject nodePrefab;
    public GameObject extensionPointPrefab;
    
    [Header("Visual Settings")]
    public float nodeSpacing = 50f;
    public Color boardColor = Color.white;
    
    private Dictionary<Vector2Int, NodeUI> nodeUIs = new Dictionary<Vector2Int, NodeUI>();
    private Dictionary<string, ExtensionPointUI> extensionPointUIs = new Dictionary<string, ExtensionPointUI>();
    
    public void InitializeBoard(PassiveBoard board)
    {
        boardData = board;
        CreateNodeGrid();
        CreateExtensionPoints();
        UpdateVisuals();
    }
    
    private void CreateNodeGrid()
    {
        // Create UI for each node in the board
        for (int row = 0; row < boardData.size.x; row++)
        {
            for (int col = 0; col < boardData.size.y; col++)
            {
                var node = boardData.GetNode(row, col);
                if (node != null)
                {
                    CreateNodeUI(node, row, col);
                }
            }
        }
    }
    
    private void CreateNodeUI(PassiveNode node, int row, int col)
    {
        var nodeGO = Instantiate(nodePrefab, nodeContainer);
        var nodeUI = nodeGO.GetComponent<NodeUI>();
        
        // Position the node
        Vector2 position = new Vector2(col * nodeSpacing, -row * nodeSpacing);
        nodeGO.GetComponent<RectTransform>().anchoredPosition = position;
        
        // Initialize the node UI
        nodeUI.Initialize(node);
        nodeUIs[new Vector2Int(row, col)] = nodeUI;
    }
    
    public void UpdateVisuals()
    {
        foreach (var nodeUI in nodeUIs.Values)
        {
            nodeUI.UpdateVisual();
        }
    }
}
```

### **Node UI Component**

```csharp
public class NodeUI : MonoBehaviour
{
    [Header("UI References")]
    public Image nodeImage;
    public TextMeshProUGUI nodeText;
    public Button nodeButton;
    
    [Header("Visual States")]
    public Color unallocatedColor = Color.gray;
    public Color allocatedColor = Color.green;
    public Color availableColor = Color.yellow;
    public Color unavailableColor = Color.red;
    
    private PassiveNode nodeData;
    
    public void Initialize(PassiveNode node)
    {
        nodeData = node;
        nodeText.text = node.name;
        
        // Setup button click
        nodeButton.onClick.AddListener(OnNodeClicked);
        
        UpdateVisual();
    }
    
    private void OnNodeClicked()
    {
        if (PassiveTreeManager.Instance.CanAllocateNode(nodeData.id))
        {
            PassiveTreeManager.Instance.AllocateNode(nodeData.id);
        }
        else if (PassiveTreeManager.Instance.IsNodeAllocated(nodeData.id))
        {
            PassiveTreeManager.Instance.DeallocateNode(nodeData.id);
        }
    }
    
    public void UpdateVisual()
    {
        if (nodeData == null) return;
        
        // Update text
        nodeText.text = $"{nodeData.name}\n({nodeData.currentRank}/{nodeData.maxRank})";
        
        // Update color based on state
        if (PassiveTreeManager.Instance.IsNodeAllocated(nodeData.id))
        {
            nodeImage.color = allocatedColor;
        }
        else if (PassiveTreeManager.Instance.CanAllocateNode(nodeData.id))
        {
            nodeImage.color = availableColor;
        }
        else
        {
            nodeImage.color = unavailableColor;
        }
    }
}
```

---

## 🔗 Phase 4: Integration with Existing Systems

### **Character Stats Integration**

```csharp
// In CharacterStatsController.cs - Add passive tree stats
public void UpdateCharacterStats(Character character)
{
    // ... existing stat updates ...
    
    // Add passive tree stats
    var passiveStats = PassiveTreeManager.Instance.GetCurrentStats();
    foreach (var stat in passiveStats)
    {
        // Apply passive stats to character stats
        ApplyPassiveStat(stat.Key, stat.Value);
    }
}

private void ApplyPassiveStat(string statName, float value)
{
    // Map passive stat names to character stat fields
    switch (statName.ToLower())
    {
        case "fireincrease":
            // Update fire damage increase
            break;
        case "coldincrease":
            // Update cold damage increase
            break;
        // ... other stat mappings
    }
}
```

### **Save/Load Integration**

```csharp
// In your save system
public void SavePassiveTreeState()
{
    var passiveState = PassiveTreeManager.Instance.playerState;
    var saveData = new PassiveTreeSaveData
    {
        allocatedNodes = passiveState.GetAllocatedNodeIds(),
        connectedBoards = passiveState.GetConnectedBoardIds(),
        boardConnections = passiveState.GetBoardConnections(),
        availablePoints = passiveState.GetAvailablePoints()
    };
    
    // Save to PlayerPrefs or file
    string json = JsonUtility.ToJson(saveData);
    PlayerPrefs.SetString("PassiveTreeState", json);
}

public void LoadPassiveTreeState()
{
    if (PlayerPrefs.HasKey("PassiveTreeState"))
    {
        string json = PlayerPrefs.GetString("PassiveTreeState");
        var saveData = JsonUtility.FromJson<PassiveTreeSaveData>(json);
        
        // Restore state
        PassiveTreeManager.Instance.playerState.allocatedNodes = saveData.allocatedNodes;
        PassiveTreeManager.Instance.playerState.connectedBoards = saveData.connectedBoards;
        PassiveTreeManager.Instance.playerState.boardConnections = saveData.boardConnections;
        PassiveTreeManager.Instance.playerState.availablePoints = saveData.availablePoints;
        
        // Recalculate stats
        PassiveTreeManager.Instance.RecalculateStats();
    }
}
```

---

## 🎮 Phase 5: Board Creation & Data Setup

### **Creating Individual Boards**

1. **Core Board Setup**:
   ```csharp
   // Example: Create a 7x7 core board with starting nodes
   var coreBoard = ScriptableObject.CreateInstance<PassiveBoard>();
   coreBoard.id = "core_board";
   coreBoard.name = "Core Board";
   coreBoard.theme = BoardTheme.Utility;
   coreBoard.size = new Vector2Int(7, 7);
   coreBoard.maxPoints = 15;
   
   // Add starting nodes
   var startNode = new PassiveNode
   {
       id = "core_start",
       name = "Starting Point",
       type = NodeType.Main,
       cost = 0,
       position = new Vector2Int(3, 3)
   };
   coreBoard.SetNode(3, 3, startNode);
   ```

2. **Extension Board Setup**:
   ```csharp
   // Example: Fire-themed extension board
   var fireBoard = ScriptableObject.CreateInstance<PassiveBoard>();
   fireBoard.id = "fire_board";
   fireBoard.name = "Infernal Mastery";
   fireBoard.theme = BoardTheme.Fire;
   fireBoard.size = new Vector2Int(7, 7);
   
   // Add fire damage nodes
   var fireNode = new PassiveNode
   {
       id = "fire_damage_1",
       name = "Fire Damage",
       type = NodeType.Small,
       cost = 1,
       stats = new Dictionary<string, float> { { "fireIncrease", 10f } },
       position = new Vector2Int(2, 2)
   };
   fireBoard.SetNode(2, 2, fireNode);
   ```

### **Board Connection System**

```csharp
// Extension points for board connections
var extensionPoint = new ExtensionPoint
{
    id = "core_ext_right",
    position = new Vector2Int(3, 6),
    availableBoards = new List<string> { "fire_board", "cold_board" },
    maxConnections = 1
};
coreBoard.extensionPoints.Add(extensionPoint);
```

---

## 🚀 Next Steps

### **Immediate Actions**

1. **Create PassiveTreeScene** with proper hierarchy
2. **Set up PassiveTreeManager** in the scene
3. **Create sample board ScriptableObjects** for testing
4. **Build basic UI components** (BoardUI, NodeUI)
5. **Test node allocation** functionality

### **Development Phases**

1. **Phase 1**: ✅ Core data structures (COMPLETED)
2. **Phase 2**: Scene setup and basic UI
3. **Phase 3**: Board rendering and node interaction
4. **Phase 4**: Board connection system
5. **Phase 5**: Integration with character stats
6. **Phase 6**: Save/load functionality
7. **Phase 7**: Performance optimization

### **Testing Strategy**

1. **Unit Tests**: Test individual components
2. **Integration Tests**: Test board connections
3. **UI Tests**: Test node allocation and visual feedback
4. **Performance Tests**: Test with large numbers of boards

---

## 📋 Implementation Checklist

- [x] Core data structures (PassiveNode, PassiveBoard, etc.)
- [x] State management (PlayerPassiveState)
- [x] Main manager (PassiveTreeManager)
- [ ] Scene setup (PassiveTreeScene)
- [ ] Board ScriptableObjects creation
- [ ] Basic UI components (BoardUI, NodeUI)
- [ ] Node allocation system
- [ ] Board connection system
- [ ] Character stats integration
- [ ] Save/load system
- [ ] Performance optimization
- [ ] Testing and debugging

---

## 🆘 Troubleshooting

### **Common Issues**

1. **Nodes not showing**: Check board initialization and node positioning
2. **Stats not updating**: Verify event subscriptions and stat calculation
3. **Board connections not working**: Check extension point configuration
4. **Performance issues**: Implement object pooling for large boards

### **Debug Tools**

- Use PassiveTreeManager's debug logging
- Check node allocation state in inspector
- Verify board connections in runtime
- Monitor stat calculation performance

---

This implementation provides a solid foundation for the modular passive tree system in Unity, maintaining the flexibility and performance of the original React/TypeScript version while leveraging Unity's strengths for game development.
