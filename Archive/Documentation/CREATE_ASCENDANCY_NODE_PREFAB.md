# Creating AscendancyNode Prefab (UI Element)

Step-by-step guide to create a proper UI node prefab.

---

## 🎯 The Issue

The error you saw:
```
MissingComponentException: There is no 'RectTransform' attached to "Node_Start_Crumbling Earth"
```

**Cause:** Node prefab is not a UI element - it needs `RectTransform` instead of `Transform`.

---

## ✅ Solution: Create UI Node Prefab

### **Step 1: Create UI GameObject**

1. **In Hierarchy:**
   - Right-click **Canvas** (important!)
   - **UI → Image** (this creates a UI element with RectTransform)
   - **Name:** `AscendancyNode`

2. **Configure RectTransform:**
   ```
   Width: 80
   Height: 80
   Anchor: Center (0.5, 0.5)
   ```

3. **Configure Image:**
   ```
   Color: White (1, 1, 1, 1)
   Sprite: Circle or your node sprite
   Raycast Target: ✅
   ```

---

### **Step 2: Add Button Component**

1. **Select `AscendancyNode`**
2. **Add Component:** Button
3. **Configure Button:**
   ```
   Interactable: ✅
   Transition: Color Tint
   Normal Color: White
   Highlighted Color: Yellow
   Pressed Color: Green
   ```

---

### **Step 3: Add Icon (Optional)**

1. **Right-click AscendancyNode → UI → Image**
2. **Name:** `Icon`
3. **Configure:**
   ```
   Rect Transform:
   └─ Size: 50x50
   
   Image:
   ├─ Color: White
   └─ Raycast Target: ☐
   ```

---

### **Step 4: Add Name Text (Optional)**

1. **Right-click AscendancyNode → UI → Text - TextMeshPro**
2. **Name:** `NameText`
3. **Configure:**
   ```
   Rect Transform:
   ├─ Anchor: Bottom center
   ├─ Position: (0, -50)
   └─ Size: (100, 30)
   
   TextMeshPro:
   ├─ Text: [Leave empty]
   ├─ Font Size: 12
   ├─ Alignment: Center
   └─ Color: White
   ```

---

### **Step 5: Add AscendancyPassiveNode Component**

1. **Select `AscendancyNode`**
2. **Add Component:** `AscendancyPassiveNode`
3. **Auto-assign references** (or leave blank to auto-find):
   ```
   Icon Image: Icon
   Name Text: NameText
   Button: AscendancyNode (self)
   ```

---

### **Step 6: Save as Prefab**

1. **Drag `AscendancyNode` from Hierarchy** to Project:
   ```
   Assets/Prefab/Ascendancy/AscendancyNode.prefab
   ```

2. **Delete from Hierarchy** (prefab is saved)

---

### **Step 7: Assign to TreeDisplayContainer**

1. **Open:** `AscendancyContainerPrefab.prefab`
2. **Select:** `TreeDisplayContainer`
3. **In AscendancyTreeDisplay component:**
   ```
   Node Prefab: [Drag AscendancyNode.prefab]
   ```
4. **Save prefab**

---

## 🧪 Test Again

1. **Press Play**
2. **Select Marauder**
3. **Click Crumbling Earth**
4. **Nodes should appear!**

---

## 🎨 Final Structure

Your node prefab should look like:

```
AscendancyNode (GameObject)
├─ RectTransform ← MUST HAVE THIS!
├─ Image (background/circle)
├─ Button
├─ AscendancyPassiveNode (script)
├─ Icon (Image) - optional
└─ NameText (TextMeshPro) - optional
```

---

## 💡 Quick Version

**Fastest way:**

1. Canvas → Right-click → UI → Image → Name: AscendancyNode
2. Add Component → Button
3. Add Component → AscendancyPassiveNode
4. Save as Prefab
5. Assign to TreeDisplayContainer → Node Prefab
6. Done!

---

## 🐛 Still Getting Errors?

**Check:**
- ✅ Prefab created under Canvas (UI element)
- ✅ Has RectTransform (not Transform)
- ✅ Assigned to Node Prefab field
- ✅ NodesContainer exists and is assigned

**Enable debug logs:**
```
TreeDisplayContainer → AscendancyTreeDisplay:
└─ Show Debug Logs: ✅
```

---

**After creating a proper UI node prefab, nodes will spawn correctly!** 🎉


