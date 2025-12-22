# Adjusting Ascendancy Node Spacing

Quick guide to control node spacing and prevent overflow.

---

## 🎯 Quick Fix: Adjust in Inspector

### **Option 1: Adjust in TreeDisplayContainer (Recommended)**

1. **Open:** `AscendancyContainerPrefab.prefab`
2. **Select:** `TreeDisplayContainer`
3. **In AscendancyTreeDisplay component:**

```
Layout:
├─ Node Spacing: 60-80 (default: 80) ← Reduce for tighter spacing
├─ Branch Spacing: 120-150 (default: 150) ← Reduce for closer branches
└─ Max Distance From Center: 250-300 (default: 300) ← Prevents overflow
```

**Recommended values for smaller container:**
```
Node Spacing: 60
Branch Spacing: 120
Max Distance From Center: 250
```

---

### **Option 2: Adjust in Code (For All Ascendancies)**

**In AscendancyBranch.cs:**
```csharp
float baseSpacing = 60f; // Change from 80f to 60f (or lower)
```

**Or adjust per branch in Ascendancy asset:**
- Each branch can have different spacing
- Modify `GenerateBranchStructure()` to use a configurable spacing value

---

## 📐 Understanding the Settings

### **Node Spacing:**
- **Distance between nodes in the same branch**
- **Default:** 80 pixels
- **Smaller = tighter:** 60 = closer nodes
- **Larger = wider:** 100 = more spread

### **Branch Spacing:**
- **Distance between different branches**
- **Default:** 150 pixels
- **Smaller = closer branches:** 120 = branches closer together
- **Larger = wider spread:** 200 = more separation

### **Max Distance From Center:**
- **Maximum distance any node can be from center**
- **Default:** 300 pixels
- **Prevents overflow:** Nodes beyond this are scaled down
- **Match to container size:** If container is 500x500, use ~250

---

## 🎨 Calculate Container Size

### **Check Your Container Size:**

1. **Open:** `AscendancyContainerPrefab.prefab`
2. **Select:** `TreeDisplayContainer`
3. **Check RectTransform:**
   ```
   Size Delta: (507, 468) ← Your current size
   ```

### **Calculate Safe Distance:**

```
Max Distance = (Container Width / 2) - Node Size
Max Distance = (507 / 2) - 40 = 213.5

Recommended: 200-250 (with padding)
```

---

## 🔧 Quick Adjustments

### **For Tighter Layout (Smaller Container):**
```
Node Spacing: 50
Branch Spacing: 100
Max Distance From Center: 200
```

### **For Medium Layout (Default):**
```
Node Spacing: 80
Branch Spacing: 150
Max Distance From Center: 300
```

### **For Wider Layout (Larger Container):**
```
Node Spacing: 100
Branch Spacing: 200
Max Distance From Center: 400
```

---

## 🧪 Test After Adjusting

1. **Save prefab** (Ctrl+S)
2. **Press Play**
3. **Click Crumbling Earth**
4. **Verify:**
   - ✅ Nodes fit within container
   - ✅ No overflow/clipping
   - ✅ Spacing looks good
   - ✅ Connection lines visible

---

## 📋 Quick Checklist

- [ ] Open AscendancyContainerPrefab
- [ ] Select TreeDisplayContainer
- [ ] Adjust Node Spacing (60-80)
- [ ] Adjust Branch Spacing (120-150)
- [ ] Set Max Distance From Center (200-300)
- [ ] Save prefab
- [ ] Test in Play mode

---

## 💡 Pro Tips

**If nodes still overflow:**
1. Reduce Node Spacing to 50
2. Reduce Max Distance to 200
3. Check container has proper size/mask

**If nodes are too close:**
1. Increase Node Spacing to 100
2. Increase Max Distance to 350
3. Increase Branch Spacing to 180

**For different container sizes:**
- Small (400x400): Node Spacing 50, Max Distance 180
- Medium (500x500): Node Spacing 70, Max Distance 230
- Large (600x600): Node Spacing 90, Max Distance 280

---

**Adjust these values in Inspector to get perfect spacing for your container!** 🎯


