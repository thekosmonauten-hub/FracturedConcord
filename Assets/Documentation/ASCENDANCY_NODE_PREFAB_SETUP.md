# Setting Up Node Spawning in TreeDisplayContainer

Quick guide to get nodes spawning in your Ascendancy tree.

---

## 🎯 What You Need

For nodes to spawn, `AscendancyTreeDisplay` needs:
1. ✅ Node Prefab (AscendancyNode.prefab)
2. ✅ NodesContainer (you already have this)
3. ✅ Ascendancy data with branches

---

## ✅ Step 1: Assign Node Prefab

### **Open Your Prefab:**
1. **In Project window:** `Assets/Prefab/Ascendancy/AscendancyContainerPrefab.prefab`
2. **Double-click** to open in Prefab mode

### **Find TreeDisplayContainer:**
1. **Select** `TreeDisplayContainer` in Hierarchy
2. **Look at** `AscendancyTreeDisplay` component in Inspector

### **Assign Node Prefab:**
```
AscendancyTreeDisplay component:
├─ Container Prefab: [Leave empty or assign if you have one]
├─ Node Prefab: [ASSIGN: Drag AscendancyNode.prefab] ← REQUIRED!
├─ Nodes Container: NodesContainer ← Already set ✅
├─ Use Manual Positions: ☐ (use branch auto-positioning)
├─ Node Spacing: 120
├─ Branch Spacing: 200
└─ Draw Connections: ✅
```

### **Save Prefab:**
Press **Ctrl+S**

---

## 📦 Step 2: Check Node Prefab Exists

### **If AscendancyNode.prefab doesn't exist:**

Create it:

1. **Right-click in Hierarchy → Create Empty**
2. **Name:** `AscendancyNode`
3. **Add Components:**
   ```
   GameObject: AscendancyNode
   ├─ RectTransform (size: 80x80)
   ├─ Image (circle or square sprite)
   ├─ Button
   └─ AscendancyPassiveNode (script)
   ```

4. **Add child for Icon:**
   ```
   Right-click AscendancyNode → UI → Image
   Name: Icon
   Size: 50x50
   Position: Center
   ```

5. **Add child for Name text:**
   ```
   Right-click AscendancyNode → UI → Text - TextMeshPro
   Name: Name
   Position: Below icon
   ```

6. **Drag to Project:**
   ```
   Drag AscendancyNode from Hierarchy to:
   Assets/Prefab/Ascendancy/AscendancyNode.prefab
   ```

7. **Delete from Hierarchy**

---

## 🔧 Step 3: Configure AscendancyNode Prefab

### **Open AscendancyNode.prefab:**

1. **Select in Project:** `Assets/Prefab/Ascendancy/AscendancyNode.prefab`
2. **Double-click** to open

### **Configure AscendancyPassiveNode component:**

```
AscendancyPassiveNode:
├─ Icon Image: Icon (Image component)
├─ Name Text: Name (TextMeshPro component)
├─ Button: AscendancyNode (Button component)
├─ Normal Color: White (1, 1, 1, 1)
├─ Locked Color: Grey (0.5, 0.5, 0.5, 1)
├─ Available Color: Yellow (1, 1, 0, 1)
├─ Unlocked Color: Green (0, 1, 0, 1)
└─ Show Debug Logs: ✅
```

### **Save Prefab**

---

## 🎮 Step 4: Test

1. **Press Play**
2. **Select Marauder**
3. **Go to CharacterDisplayUI**
4. **Click "Crumbling Earth" button**
5. **Panel opens:**

**Console should show:**
```
[AscendancyDisplayPanel] Showing: Crumbling Earth
[AscendancyDisplayPanel] Spawned container: AscendancyContainer_Crumbling Earth
[AscendancyDisplayPanel] ✓ Container controller initialized
[AscendancyTreeDisplay] DisplayAscendancy: Crumbling Earth
[AscendancyTreeDisplay] Spawned 10 passive nodes ← SHOULD SEE THIS!
```

**Verify:**
- ✅ Start node at center
- ✅ 3 branches extending out
- ✅ Nodes connected with lines
- ✅ Nodes have correct icons/names

---

## 🐛 Troubleshooting

### **❌ No nodes appear:**

**Check 1: Node Prefab assigned?**
```
Open AscendancyContainerPrefab
→ TreeDisplayContainer
→ AscendancyTreeDisplay component
→ Node Prefab: Should have AscendancyNode assigned
```

**Check 2: Ascendancy has branches?**
```
Open MarauderCrumblingEarth asset
→ Use Branch System: ✅
→ Branches: Size = 3
→ Each branch has nodes
```

**Check 3: Enable debug logs:**
```
AscendancyTreeDisplay component:
└─ Show Debug Logs: ✅
```

Then check Console for error messages.

---

### **❌ Nodes spawn but are invisible:**

**Check Canvas settings:**
```
Select TreeDisplayContainer
→ Check parent Canvas Sort Order
→ Should be high (e.g., 100)
```

**Check node prefab:**
```
Open AscendancyNode.prefab
→ Image component has sprite assigned
→ Color is not transparent
→ Size is reasonable (80x80)
```

---

### **❌ Nodes spawn in wrong position:**

**Check branch setup:**
```
MarauderCrumblingEarth asset:
├─ Use Branch System: ✅
├─ Use Auto Generated Paths: ✅
└─ Number Of Branches: 3
```

**Check tree display settings:**
```
TreeDisplayContainer → AscendancyTreeDisplay:
├─ Use Manual Positions: ☐ (for auto-layout)
├─ Node Spacing: 100-150
└─ Branch Spacing: 150-250
```

---

## 📋 Quick Checklist

- [ ] Node Prefab exists (AscendancyNode.prefab)
- [ ] Node Prefab assigned to TreeDisplayContainer
- [ ] NodesContainer is assigned
- [ ] Ascendancy has branches set up
- [ ] Use Branch System enabled
- [ ] Debug logs enabled
- [ ] Test in Play mode

---

## 🎨 Expected Result

When panel opens:

```
         [START]
           |
    ┌──────┼──────┐
    |      |      |
  [●]    [●]    [●]  ← Minor nodes
    |      |      |
  [●]    [●]    [●]  ← Major nodes (larger)
    |      |
  [●]    [●]        ← More nodes
    |
  [●]               ← Final node

With connection lines between them!
```

---

## 💡 Alternative: Simple Node Prefab

If you don't have a node prefab yet, here's the simplest version:

```
AscendancyNode (GameObject)
├─ RectTransform (60x60)
├─ Image (white circle)
├─ Button
└─ AscendancyPassiveNode (script)
    └─ All fields can auto-find
```

**Create it:**
1. Create Empty → Add components
2. Save as Prefab
3. Assign to TreeDisplayContainer
4. Done!

---

**Once Node Prefab is assigned, nodes will spawn automatically when panel opens!** 🎉


