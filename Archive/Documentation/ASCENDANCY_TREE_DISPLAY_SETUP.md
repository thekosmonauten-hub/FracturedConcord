# Ascendancy Tree Display Setup

Guide for setting up your dynamic Ascendancy tree with splash art and passive nodes.

---

## 🎯 Overview

Your prefabs create a visual Ascendancy tree:
- **Central Container**: Displays Ascendancy splash art in a circular frame
- **Passive Nodes**: Individual unlockable passives arranged around the container
- **Dynamic System**: Uses **icons from `AscendancyPassive.icon`** (NOT index-based!)

**Why this is better than index-based:**
- ✅ Each passive stores its own icon
- ✅ Reordering passives doesn't break the display
- ✅ Missing icons are handled gracefully
- ✅ Each passive can have a unique icon

---

## 📦 System Components

### **1. AscendancyContainerController.cs**
- Controls the central splash art display
- Auto-finds Image components in prefab
- Applies theme color tinting

### **2. AscendancyPassiveNode.cs**
- Displays individual passive abilities
- Uses `AscendancyPassive.icon` sprite (stored in data!)
- Handles unlock states: locked, available, unlocked
- Hover effects and click handling

### **3. AscendancyTreeDisplay.cs**
- Main manager that coordinates everything
- Spawns container + nodes
- Arranges nodes in circular or grid layout
- Handles unlock logic and progression

---

## ✅ Setup Steps

### **Step 1: Add Scripts to Prefabs**

#### **AscendancyContainerPrefab:**
1. Open `Assets/Prefab/Ascendancy/AscendancyContainerPrefab.prefab`
2. **Select root GameObject** (`AscendancyContainerPrefab`)
3. **Add Component** → `AscendancyContainerController`
4. **Assign references** (or leave empty for auto-find):
   ```
   Splash Art Image: SplashArt/AscendancySplashArt (Image)
   Frame Overlay: FrameOverlay (Image)
   Circular Frame: CircularFrame (Image)
   ```
5. **Save prefab**

#### **AscendancyNode.prefab:**
1. Open `Assets/Prefab/Ascendancy/AscendancyNode.prefab`
2. **Select root GameObject** (`AscendancyNode`)
3. **Add Component** → `AscendancyPassiveNode`
4. **Assign references** (or leave empty for auto-find):
   ```
   Node Image: NodeArtButton (Image)
   Node Button: NodeArtButton (Button)
   ```
5. **Configure visual states:**
   ```
   Locked Color: Gray (0.3, 0.3, 0.3)
   Unlocked Color: White (1, 1, 1)
   Available Color: Yellow (1, 1, 0.5)
   Hover Scale: 1.1
   ```
6. **Save prefab**

---

### **Step 2: Create Ascendancy Tree Display**

1. **Create a new scene or use existing** (e.g., `AscendancyTreeScene`)
2. **Create UI Canvas:**
   - Right-click Hierarchy → UI → Canvas
   - Name: `AscendancyCanvas`
3. **Create display manager:**
   - Right-click Canvas → Create Empty
   - Name: `AscendancyTreeDisplay`
   - Add Component → `AscendancyTreeDisplay`

4. **Create nodes container:**
   - Right-click `AscendancyTreeDisplay` → Create Empty
   - Name: `NodesContainer`
   - This will hold all the spawned nodes

5. **Assign in Inspector:**
   ```
   Prefabs
   ├─ Container Prefab: [Drag AscendancyContainerPrefab]
   └─ Node Prefab: [Drag AscendancyNode]
   
   Layout
   ├─ Nodes Container: [Drag NodesContainer]
   ├─ Node Distance From Center: 250
   └─ Circular Layout: ✅
   ```

---

### **Step 3: Create Optional Info Panel**

For displaying passive details when clicked:

1. **Create Panel:**
   - Right-click Canvas → UI → Panel
   - Name: `PassiveInfoPanel`
   - Position: Right side or bottom of screen

2. **Add UI elements inside panel:**
   ```
   PassiveInfoPanel
   ├─ PassiveName (TextMeshPro)
   ├─ PassiveDescription (TextMeshPro)
   ├─ PassiveCost (TextMeshPro)
   └─ UnlockButton (Button)
       └─ Text: "Unlock"
   ```

3. **Assign to AscendancyTreeDisplay:**
   ```
   Optional: Info Panel
   ├─ Info Panel: [Drag PassiveInfoPanel]
   ├─ Passive Name Text: [Drag PassiveName]
   ├─ Passive Description Text: [Drag PassiveDescription]
   ├─ Passive Cost Text: [Drag PassiveCost]
   └─ Unlock Button: [Drag UnlockButton]
   ```

---

### **Step 4: Add Icons to Passive Abilities**

**IMPORTANT:** Each passive needs an icon sprite!

1. **Open an Ascendancy asset** (e.g., `MarauderCrumblingEarth`)
2. **For each passive ability:**
   ```
   Passive Abilities → Element 0
   ├─ Name: "Blood Price"
   ├─ Description: "Lose 5% current HP..."
   ├─ Icon: [Drag icon sprite] ← ADD THIS!
   ├─ Point Cost: 1
   └─ Prerequisites: []
   ```
3. **Repeat for all passives**

**Icon Tips:**
- **Size:** 64x64 or 128x128 pixels
- **Style:** Simple, recognizable symbols
- **Format:** PNG with transparency
- **Examples:**
  - Blood Price: Blood drop icon
  - Wound Echo: Echo/repeat symbol
  - Seismic Hunger: Healing/heart icon
  - Rage Wellspring: Mana/energy icon

---

## 🧪 Testing

### **Test Display:**

1. **Create test script:**

```csharp
using UnityEngine;

public class AscendancyTreeTest : MonoBehaviour
{
    [SerializeField] private AscendancyTreeDisplay treeDisplay;
    [SerializeField] private string ascendancyClass = "Marauder";
    
    void Start()
    {
        // Get Ascendancy data
        AscendancyDatabase db = AscendancyDatabase.Instance;
        var ascendancies = db.GetAscendanciesForClass(ascendancyClass);
        
        if (ascendancies.Count > 0)
        {
            // Create test progression (with some points)
            CharacterAscendancyProgress progress = new CharacterAscendancyProgress();
            progress.ChooseAscendancy(ascendancies[0].ascendancyName);
            progress.AwardPoints(5); // Give 5 points for testing
            
            // Display tree
            treeDisplay.DisplayAscendancy(ascendancies[0], progress);
        }
    }
}
```

2. **Add to scene:**
   - Select `AscendancyTreeDisplay` GameObject
   - Add Component → `AscendancyTreeTest`
   - Assign Tree Display field

3. **Press Play**

### **Expected Results:**
- ✅ Central splash art appears
- ✅ Passive nodes arranged in circle
- ✅ Each node shows its icon (if assigned)
- ✅ Locked nodes are greyed out
- ✅ Available nodes are yellowish
- ✅ Hover scales nodes up
- ✅ Click shows info panel
- ✅ Unlock button works

---

## 🎨 Customization

### **Layout Options:**

**Circular (Default):**
```
Circular Layout: ✅
Node Distance From Center: 250
```
- Nodes arranged in circle
- Good for 5-10 passives
- Clean, symmetric look

**Grid Layout:**
```
Circular Layout: ☐
```
- Nodes arranged in grid
- Good for many passives
- Compact layout

### **Visual States:**

**In AscendancyPassiveNode:**
```
Locked Color: (0.3, 0.3, 0.3) - Dark grey
Unlocked Color: (1, 1, 1) - Full color
Available Color: (1, 1, 0.5) - Yellow highlight
```

**Hover Effect:**
```
Hover Scale: 1.1 - Grows 10% on hover
```

### **Node Distance:**

```
Node Distance From Center: 
- 200: Closer to center (compact)
- 250: Default (balanced)
- 300: Further out (spacious)
```

---

## 🔧 Advanced Features

### **Connecting Lines Between Nodes:**

To show prerequisites visually:

```csharp
// In AscendancyTreeDisplay, after spawning nodes:
void DrawConnectionLines()
{
    foreach (var node in spawnedNodes)
    {
        AscendancyPassive passive = node.GetPassiveData();
        
        if (passive.prerequisitePassives != null)
        {
            foreach (string prereqName in passive.prerequisitePassives)
            {
                // Find prerequisite node
                var prereqNode = spawnedNodes.Find(n => 
                    n.GetPassiveData().name == prereqName);
                
                if (prereqNode != null)
                {
                    // Draw line between nodes
                    DrawLineBetween(prereqNode.transform.position, 
                                   node.transform.position);
                }
            }
        }
    }
}
```

### **Glow Effect for Available Nodes:**

```csharp
// In AscendancyPassiveNode:
[SerializeField] private Image glowImage;

void UpdateVisualState()
{
    // ... existing code ...
    
    if (glowImage != null)
    {
        glowImage.gameObject.SetActive(isAvailable && !isUnlocked);
    }
}
```

### **Animation When Unlocking:**

```csharp
// In AscendancyTreeDisplay:
void OnUnlockButtonClicked()
{
    // ... existing code ...
    
    if (unlocked)
    {
        // Play unlock animation
        LeanTween.scale(selectedNode.gameObject, Vector3.one * 1.3f, 0.3f)
            .setEaseOutBack()
            .setOnComplete(() => {
                LeanTween.scale(selectedNode.gameObject, Vector3.one, 0.2f);
            });
    }
}
```

---

## 📋 Setup Checklist

**Prefabs:**
- [ ] Add `AscendancyContainerController` to container prefab
- [ ] Add `AscendancyPassiveNode` to node prefab
- [ ] Test prefabs in isolation

**Scene Setup:**
- [ ] Create Canvas with `AscendancyTreeDisplay`
- [ ] Create NodesContainer
- [ ] Assign prefabs to tree display
- [ ] Set layout parameters

**Data Setup:**
- [ ] Create Ascendancy assets
- [ ] Add **icons to every passive ability**
- [ ] Test with one Ascendancy first

**Testing:**
- [ ] Add test script
- [ ] Press Play
- [ ] Verify nodes display correctly
- [ ] Test hover and click
- [ ] Test unlock functionality

---

## 🐛 Troubleshooting

### ❌ **"Nodes have no icons"**
**Fix:** Assign icons to `AscendancyPassive.icon` field in your Ascendancy assets

### ❌ **"Nodes not spawning"**
**Fix:** 
1. Check prefabs are assigned
2. Check Ascendancy has passive abilities
3. Check Console for errors

### ❌ **"Nodes all in same position"**
**Fix:** Check `Node Distance From Center` is not 0

### ❌ **"Can't unlock nodes"**
**Fix:** 
1. Check progression has points (`progress.AwardPoints(5)`)
2. Check prerequisites are met

---

## 💡 Why This System is Better

**vs Index-Based:**
- ✅ **Flexible**: Reorder passives without breaking anything
- ✅ **Data-Driven**: Icons stored with passive data
- ✅ **Maintainable**: No magic number indices
- ✅ **Scalable**: Easy to add/remove passives

**Features:**
- ✅ **Visual States**: Locked, available, unlocked
- ✅ **Hover Effects**: Scale animation
- ✅ **Click to Unlock**: Interactive unlocking
- ✅ **Progression Tracking**: Integrates with `CharacterAscendancyProgress`
- ✅ **Prerequisites**: Automatically checks requirements
- ✅ **Circular/Grid Layout**: Flexible positioning

---

**Last Updated:** 2024-12-19
**Status:** ✅ Complete Dynamic System - Ready to Use

