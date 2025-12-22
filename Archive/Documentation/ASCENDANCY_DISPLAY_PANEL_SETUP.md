# Ascendancy Display Panel Setup

Complete guide for setting up the full Ascendancy tree panel that opens when clicking Ascendancy buttons.

---

## 🎯 Overview

When you click an Ascendancy button (Ascendancy1, 2, or 3), a detailed panel opens showing:
- Splash art and description
- Core mechanic explanation
- Interactive passive tree with all nodes
- Branch structure
- Unlock progression

---

## ✅ Setup Steps

### **Step 1: Create the Panel Root**

1. **In CharacterDisplayUI scene**
2. **Right-click Canvas → UI → Panel**
3. **Name:** `AscendancyDisplayPanel`
4. **Configure:**
   ```
   Rect Transform:
   ├─ Anchor: Stretch both (0,0 to 1,1)
   ├─ Offset: (0, 0, 0, 0)
   └─ Full screen overlay
   
   Image (Background):
   ├─ Color: Black (0, 0, 0, 0.8) - Semi-transparent
   └─ Raycast Target: ✅ (blocks clicks behind panel)
   ```

5. **Add Component:** `AscendancyDisplayPanel`

---

### **Step 2: Create Panel Structure**

Inside `AscendancyDisplayPanel`, create this hierarchy:

```
AscendancyDisplayPanel
├── Background (Image) - Already exists from Panel
├── CloseButton (Button) - Top-right X button
├── InfoSection (VerticalLayoutGroup)
│   ├── NameText (TextMeshPro) - "Crumbling Earth"
│   ├── TaglineText (TextMeshPro) - "Every strike chips the world..."
│   ├── DescriptionText (TextMeshPro) - Full description
│   ├── CoreMechanicSection
│   │   ├── CoreMechanicNameText (TextMeshPro) - "Crumble"
│   │   └── CoreMechanicDescriptionText (TextMeshPro) - How it works
│   └── SignatureCardText (TextMeshPro) - "3x Earthquake..."
├── TreeDisplayContainer (Empty GameObject)
│   └── [Tree spawns here at runtime]
└── ProgressionInfo
    ├── AvailablePointsText (TextMeshPro)
    └── SpentPointsText (TextMeshPro)
```

---

### **Step 3: Create Info Section**

1. **Right-click AscendancyDisplayPanel → Create Empty**
2. **Name:** `InfoSection`
3. **Add Component:** Vertical Layout Group
   ```
   Padding: 20, 20, 20, 20
   Spacing: 10
   Child Alignment: Upper Center
   ```

4. **Position:**
   ```
   Rect Transform:
   ├─ Anchor: Top stretch (0, 1 to 1, 1)
   ├─ Height: 250
   └─ Position: Top of screen
   ```

---

### **Step 4: Add Text Components**

Inside `InfoSection`:

**Name Text:**
```
Right-click InfoSection → UI → Text - TextMeshPro
Name: NameText
Font Size: 32
Style: Bold
Color: Gold
Alignment: Center
```

**Tagline Text:**
```
Font Size: 18
Style: Italic
Color: Light Grey
Alignment: Center
```

**Description Text:**
```
Font Size: 14
Color: White
Alignment: Center
Auto Size: Enable (min 10, max 14)
```

**Core Mechanic Name:**
```
Font Size: 20
Style: Bold
Color: Yellow
Alignment: Center
```

**Core Mechanic Description:**
```
Font Size: 14
Color: White
Alignment: Left
Wrapping: Enabled
```

**Signature Card:**
```
Font Size: 14
Color: Magenta
Alignment: Center
```

---

### **Step 5: Create Tree Display Container**

1. **Right-click AscendancyDisplayPanel → Create Empty**
2. **Name:** `TreeDisplayContainer`
3. **Add Component:** `AscendancyTreeDisplay`
4. **Position:**
   ```
   Rect Transform:
   ├─ Anchor: Center (0.5, 0.5)
   ├─ Size: 800 x 600
   └─ Position: Center of screen (below info section)
   ```

5. **Create child:** `NodesContainer`
   ```
   Right-click TreeDisplayContainer → Create Empty
   Name: NodesContainer
   Rect Transform: Fill parent
   ```

6. **Configure AscendancyTreeDisplay:**
   ```
   Prefabs:
   ├─ Container Prefab: AscendancyContainerPrefab
   └─ Node Prefab: AscendancyNode
   
   Layout:
   ├─ Nodes Container: NodesContainer
   ├─ Use Manual Positions: ✅
   ├─ Node Spacing: 100
   └─ Branch Spacing: 200
   
   Connection Lines:
   ├─ Draw Connections: ✅
   ├─ Connection Color: White
   └─ Connection Width: 3
   ```

---

### **Step 6: Create Close Button**

1. **Right-click AscendancyDisplayPanel → UI → Button**
2. **Name:** `CloseButton`
3. **Position:**
   ```
   Rect Transform:
   ├─ Anchor: Top-right (1, 1)
   ├─ Size: 60 x 60
   └─ Position: (-30, -30)
   ```
4. **Add Text child:** "X"

---

### **Step 7: Create Progression Info**

1. **Right-click AscendancyDisplayPanel → Create Empty**
2. **Name:** `ProgressionInfo`
3. **Add Component:** Horizontal Layout Group
4. **Position:** Bottom of panel

**Add two text components:**
```
AvailablePointsText:
└─ "Available Points: 0"

SpentPointsText:
└─ "Spent: 0/8"
```

---

### **Step 8: Wire Up AscendancyDisplayPanel Component**

Select `AscendancyDisplayPanel` GameObject:

```
Panel References:
├─ Panel Root: AscendancyDisplayPanel (self)
├─ Close Button: CloseButton
└─ Back Button: [Optional]

Ascendancy Info:
├─ Splash Art Image: [Create Image for splash art]
├─ Name Text: NameText
├─ Tagline Text: TaglineText
├─ Description Text: DescriptionText
├─ Core Mechanic Name Text: CoreMechanicNameText
├─ Core Mechanic Description Text: CoreMechanicDescriptionText
└─ Signature Card Text: SignatureCardText

Tree Display:
└─ Tree Display: TreeDisplayContainer (AscendancyTreeDisplay component)

Progression Info:
├─ Available Points Text: AvailablePointsText
└─ Spent Points Text: SpentPointsText

Settings:
└─ Show Debug Logs: ✅
```

---

### **Step 9: Wire Up CharacterDisplayController**

Select `CharacterDisplayController` GameObject:

```
Ascendancy Display:
├─ Ascendancy1 Button: [Already assigned]
├─ Ascendancy2 Button: [Already assigned]
├─ Ascendancy3 Button: [Already assigned]
└─ Ascendancy Display Panel: AscendancyDisplayPanel ← ADD THIS
```

---

### **Step 10: Hide Panel Initially**

1. **Select `AscendancyDisplayPanel` GameObject**
2. **Uncheck Active** in Inspector (panel starts hidden)
3. **Save scene**

---

## 🧪 Testing

1. **Press Play**
2. **Select Marauder**
3. **Go to CharacterDisplayUI**
4. **Click "Crumbling Earth" button**
5. **Verify:**
   - ✅ Panel opens
   - ✅ Shows Ascendancy name, tagline, description
   - ✅ Shows core mechanic (Crumble)
   - ✅ Shows tree with Start node + 3 branches
   - ✅ Nodes properly positioned and connected
   - ✅ Click X to close panel

**Console output:**
```
[CharacterDisplayController] Ascendancy clicked: Crumbling Earth
[AscendancyDisplayPanel] Showing: Crumbling Earth
[AscendancyTreeDisplay] Displayed Crumbling Earth with 10 passives
✓ Opened Ascendancy panel: Crumbling Earth
```

---

## 🎨 Optional Enhancements

### **Add Splash Art Display:**

```
Right-click InfoSection → UI → Image (at index 0, before Name)
Name: SplashArt
Size: 200 x 200
Preserve Aspect: ✅
```

Assign to:
```
AscendancyDisplayPanel → Splash Art Image
```

---

### **Add Panel Animation:**

In `AscendancyDisplayPanel.cs`, enhance `ShowAscendancy()`:

```csharp
// Fade in animation
panelRoot.SetActive(true);
CanvasGroup canvasGroup = panelRoot.GetComponent<CanvasGroup>();
if (canvasGroup == null)
    canvasGroup = panelRoot.AddComponent<CanvasGroup>();

canvasGroup.alpha = 0;
LeanTween.alphaCanvas(canvasGroup, 1f, 0.3f).setEaseOutQuad();
```

---

### **Add Background Blur:**

1. **Duplicate Background Image**
2. **Name:** `BlurBackground`
3. **Add Component:** UI → Effects → Blur (if available)
4. **Set alpha:** 0.9

---

## 📋 Quick Setup Checklist

- [ ] Create AscendancyDisplayPanel (UI Panel)
- [ ] Add AscendancyDisplayPanel component
- [ ] Create InfoSection with text components
- [ ] Create TreeDisplayContainer with AscendancyTreeDisplay
- [ ] Create NodesContainer for tree nodes
- [ ] Create CloseButton
- [ ] Assign all references in AscendancyDisplayPanel component
- [ ] Assign panel to CharacterDisplayController
- [ ] Hide panel (uncheck Active)
- [ ] Assign prefabs to AscendancyTreeDisplay
- [ ] Test!

---

## 🔧 Panel Layout Example

```
┌─────────────────────────────────────┐
│  CRUMBLING EARTH           [X]      │ ← Name + Close
│  "Every strike chips the world..."  │ ← Tagline
│                                     │
│  The destructive rhythm of...       │ ← Description
│                                     │
│  CORE MECHANIC: Crumble             │ ← Mechanic
│  Enemies gain Crumble stacks...     │
│                                     │
│  SIGNATURE CARD:                    │
│  3x Earthquake (Attack - 3 Mana)    │
│                                     │
│  ┌──────PASSIVE TREE───────┐       │
│  │       [START]            │       │
│  │          |               │       │
│  │    ┌─────┼─────┐        │       │
│  │  [Minor] [Minor] [Minor] │       │
│  │    |       |       |     │       │
│  │  [Major] [Major] [Major] │       │
│  └─────────────────────────┘       │
│                                     │
│  Points: 0/8 available              │ ← Progression
└─────────────────────────────────────┘
```

---

## 🐛 Troubleshooting

### ❌ **Panel doesn't open**
**Fix:**
- Check `AscendancyDisplayPanel` assigned in CharacterDisplayController
- Check Console for: "AscendancyDisplayPanel not assigned!"

### ❌ **Tree doesn't display**
**Fix:**
- Check TreeDisplay component has prefabs assigned
- Check NodesContainer created
- Check Console for errors

### ❌ **No nodes appear**
**Fix:**
- Verify Ascendancy has branches or passiveAbilities
- Check `Use Branch System` setting
- Enable debug logs and check Console

---

**Last Updated:** 2024-12-19
**Status:** ✅ Full Panel System Complete - Ready to Build!


