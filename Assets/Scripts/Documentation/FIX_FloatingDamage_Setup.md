# 🚨 FIX: Floating Damage Numbers Not Showing

## **Problem:**
Floating damage numbers are not appearing because the `FloatingDamageManager` GameObject doesn't exist in your scene yet.

## **Quick Fix (2 minutes):**

### **Step 1: Create FloatingDamageManager in Scene**

1. **Open your Combat Scene** (the one with the combat UI)
2. **In Hierarchy**, right-click your **"CombatScene"** GameObject → **Create Empty**
3. **Name it**: `FloatingDamageManager`
4. **Add Component** → `FloatingDamageManager`

### **Step 2: Create FloatingDamageText Prefab**

1. **Right-click** "CombatScene" GameObject → **Create Empty** → Name: `FloatingDamageText`
2. **Add Component** → **UI** → **TextMeshPro - Text (UI)**
   - **Text**: "999"
   - **Font Size**: 36
   - **Alignment**: Center (both)
   - **Color**: White
3. **Add Component** → **CanvasGroup**
4. **Add Component** → `FloatingDamageText`
5. **In Inspector**, assign:
   - **Damage Text**: Drag the TextMeshPro component
   - **Canvas Group**: Drag the CanvasGroup component
6. **Drag** `FloatingDamageText` from Hierarchy → **Project window** (create prefab)
7. **Delete** the original from Hierarchy

### **Step 3: Configure Manager**

1. **Select** `FloatingDamageManager` in Hierarchy
2. **In Inspector**:
   - **Floating Damage Prefab**: Drag your `FloatingDamageText` prefab
   - **Damage Number Container**: Drag your **"CombatScene"** GameObject
   - **Spawn Offset**: 50
   - **Use Pooling**: ✓ Checked
   - **Pool Size**: 20

### **Step 4: Test**

1. **Enter Play Mode**
2. **Attack an enemy**
3. **You should see floating damage numbers!** 🎯

---

## **If Still Not Working:**

Check the **Console** for this message:
```
[CombatDisplayManager] FloatingDamageManager not found. Damage numbers will not display.
```

If you see this, the manager wasn't found. Make sure:
- [ ] `FloatingDamageManager` GameObject exists in scene
- [ ] It has the `FloatingDamageManager` component
- [ ] It's a child of the same Canvas as your combat UI

---

## **Expected Result:**

When you attack an enemy, you should see:
- **White numbers** floating up and fading out
- **Orange, larger numbers** for critical hits
- **Green numbers with "+"** for healing

---

**This should fix the floating damage numbers issue!** ✨
