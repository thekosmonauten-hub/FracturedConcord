# Floating Damage Numbers - Setup Guide

## 🎯 Overview

Floating damage numbers appear above enemies when they take damage, providing instant visual feedback with:
- ✅ **Animated numbers** that float upward and fade out
- ✅ **Critical hit highlighting** (orange, larger, bold)
- ✅ **Heal numbers** (green, with + prefix)
- ✅ **Object pooling** for performance
- ✅ **Customizable animations** with curves

---

## 📋 Setup Instructions

### **Step 1: Create Floating Damage Prefab**

#### **1.1: Create UI GameObject**
1. In Hierarchy, **right-click** your Combat Canvas → Create Empty
2. **Name it**: `FloatingDamageText`
3. **Add Component**: `RectTransform` (should auto-add)

#### **1.2: Add Text Component**
1. **Select** `FloatingDamageText`
2. **Add Component** → UI → **TextMeshPro - Text (UI)**
3. **Configure TextMeshPro:**
   - **Text**: "999" (placeholder)
   - **Font**: Choose your game font
   - **Font Size**: 36
   - **Alignment**: Center (both horizontal and vertical)
   - **Color**: White
   - **Vertex Color**: White (alpha 255)
   - **Extra Settings** → **Enable Rich Text**: ✓

#### **1.3: Add CanvasGroup**
1. **Select** `FloatingDamageText`
2. **Add Component** → **CanvasGroup**
3. This allows fade-in/out animations

#### **1.4: Add FloatingDamageText Component**
1. **Select** `FloatingDamageText`
2. **Add Component** → `FloatingDamageText`
3. **In Inspector**, configure:
   - **Damage Text**: Drag the TextMeshProUGUI component
   - **Canvas Group**: Drag the CanvasGroup component
   - **Float Distance**: `100` (how far up it floats)
   - **Float Duration**: `1.5` seconds
   - **Normal Damage Color**: White
   - **Critical Damage Color**: Orange (R:255, G:153, B:0)
   - **Heal Color**: Green
   - **Critical Size Multiplier**: `1.5`

#### **1.5: Create Prefab**
1. **Drag** `FloatingDamageText` from Hierarchy → Project window (e.g., `Assets/Prefabs/`)
2. **Delete** the original from Hierarchy (we'll spawn it dynamically)

---

### **Step 2: Setup Floating Damage Manager**

#### **2.1: Create Manager GameObject**
1. In Hierarchy, **right-click** your Combat Canvas → Create Empty
2. **Name it**: `FloatingDamageManager`

#### **2.2: Add Component**
1. **Select** `FloatingDamageManager`
2. **Add Component** → `FloatingDamageManager`

#### **2.3: Configure Manager**
In Inspector:
- **Floating Damage Prefab**: Drag `FloatingDamageText` prefab from Project
- **Damage Number Container**: Drag your Combat Canvas (or a dedicated container)
- **Spawn Offset**: `50` (offset above enemy)
- **Use Pooling**: ✓ (checked)
- **Pool Size**: `20` (adjust based on expected damage frequency)

---

### **Step 3: Verify Integration**

The code integration is already complete! Just verify:

1. **CombatDisplayManager** has reference to `FloatingDamageManager`
2. Damage numbers spawn when enemies take damage
3. Critical hits show orange/larger numbers

---

## 🎨 Customization

### **Colors**

Edit in `FloatingDamageText` component:

| Type | Suggested Color | RGB |
|------|----------------|-----|
| **Normal Damage** | White | (255, 255, 255) |
| **Critical** | Orange | (255, 153, 0) |
| **Heal** | Green | (0, 255, 0) |

### **Animation Curves**

**Float Curve** (how damage number moves):
- Default: `EaseInOut` (smooth motion)
- Alternative: `EaseOut` (starts fast, slows down)

**Fade Curve** (how damage number fades):
- Default: `Linear` (steady fade)
- Alternative: Custom curve for delayed fade

### **Timing**

| Setting | Default | Description |
|---------|---------|-------------|
| **Float Distance** | 100 | How far up it floats (pixels) |
| **Float Duration** | 1.5s | Animation length |
| **Critical Size** | 1.5x | Critical hit size multiplier |

---

## 🧪 Testing

### **Test 1: Normal Damage**
1. Enter Play Mode
2. Attack an enemy with a normal hit
3. **Verify**: White number floats up and fades

### **Test 2: Critical Hit**
1. Attack until you get a critical hit
2. **Verify**: Orange, larger, bold number with "!" appears

### **Test 3: Multiple Hits**
1. Attack multiple enemies rapidly
2. **Verify**: Multiple damage numbers appear simultaneously
3. **Check**: No performance issues

### **Test 4: Pool Limit**
1. Generate 20+ damage numbers rapidly
2. **Verify**: Old numbers disappear, new ones spawn
3. **Check console**: No error messages

---

## 🎨 Visual Enhancements (Optional)

### **Add Outline/Shadow**

1. Select `FloatingDamageText` prefab
2. In TextMeshPro component:
   - **Material Preset**: Choose one with outline
   - **OR** Add outline manually:
     - **Extra Settings** → **Outline**
     - **Outline Color**: Black
     - **Outline Width**: 0.2

### **Add Scale Animation**

Modify `FloatingDamageText.cs` Show() method:
```csharp
// Add scale punch effect
LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.2f)
    .setEase(LeanTweenType.easeOutBack)
    .setOnComplete(() => 
    {
        LeanTween.scale(gameObject, Vector3.one, 0.1f);
    });
```

### **Add Random Horizontal Drift**

Already included! Numbers drift slightly left/right:
```csharp
Vector3 endPosition = startPosition + new Vector3(
    Random.Range(-20f, 20f), // Random horizontal drift
    floatDistance, 
    0
);
```

---

## 🎮 Advanced Features

### **Show Damage Type Icons**

Add an Image component to the prefab for elemental damage icons:
```
FloatingDamageText
├─ DamageNumber (TextMeshPro)
└─ ElementIcon (Image) - Shows fire/cold/lightning icon
```

### **Show Multiple Damage Types**

For attacks with multiple damage types (e.g., 10 physical + 5 fire):
```csharp
floatingDamageManager.ShowDamage(physicalDamage, isCrit, enemyTransform);
floatingDamageManager.ShowDamage(fireDamage, false, enemyTransform + offset);
```

### **Show Blocked/Evaded**

Add methods to `FloatingDamageManager`:
```csharp
public void ShowBlocked(Transform target)
{
    // Show "BLOCKED" text instead of damage
}

public void ShowEvaded(Transform target)
{
    // Show "EVADED" text
}
```

---

## 🐛 Troubleshooting

### **Issue: Numbers don't appear**

**Check:**
- [ ] `FloatingDamageManager` exists in scene
- [ ] Prefab is assigned to manager
- [ ] Damage number container is assigned
- [ ] Container is a child of a Canvas
- [ ] Canvas render mode is set correctly

**Solution:**
```
Right-click CombatDisplayManager → Check Enemy Setup
Check console for: "FloatingDamageManager not found"
```

### **Issue: Numbers appear in wrong position**

**Check:**
- [ ] Container is parented to correct Canvas
- [ ] Canvas scaler settings (match game resolution)
- [ ] Spawn offset value (adjust up/down)

**Solution:**
Adjust `spawnOffset` in FloatingDamageManager (50 is default).

### **Issue: Numbers overlap**

**Solutions:**
- Reduce `floatDuration` (faster fade)
- Increase random horizontal drift range
- Add stagger delay between multiple hits
- Increase pool size (so old ones despawn faster)

### **Issue: Performance problems**

**Solutions:**
- Reduce pool size if you have many numbers
- Disable pooling for testing
- Check for memory leaks (numbers not returning to pool)
- Use simpler font/material

---

## 📊 Performance

### **Memory Usage:**
- **Pool Size 20**: ~200 KB (with TextMeshPro)
- **Pool Size 50**: ~500 KB

### **Spawn Cost:**
- **With Pooling**: ~0.1ms per number
- **Without Pooling**: ~2-5ms per number (Instantiate cost)

### **Recommended Settings:**

| Game Type | Pool Size | Duration |
|-----------|-----------|----------|
| **Slow Combat** | 10 | 2.0s |
| **Normal Combat** | 20 | 1.5s |
| **Fast Combat** | 30-50 | 1.0s |

---

## 🎯 Integration Checklist

- [x] Create `FloatingDamageText.cs` component
- [x] Create `FloatingDamageManager.cs` singleton
- [x] Integrate with `CombatDisplayManager`
- [ ] Create FloatingDamageText prefab in Unity
- [ ] Setup FloatingDamageManager in scene
- [ ] Assign prefab reference
- [ ] Test normal damage
- [ ] Test critical damage
- [ ] Test multiple simultaneous hits
- [ ] Adjust colors/timing to preference

---

## 🎨 Example Configurations

### **Minimal (Subtle)**
```
Float Distance: 50
Float Duration: 1.0s
Critical Size: 1.2x
Normal Color: Light gray
Critical Color: Yellow
```

### **Standard (Balanced)**
```
Float Distance: 100
Float Duration: 1.5s
Critical Size: 1.5x
Normal Color: White
Critical Color: Orange
```

### **Dramatic (Eye-catching)**
```
Float Distance: 150
Float Duration: 2.0s
Critical Size: 2.0x
Normal Color: White
Critical Color: Red
Add outline effect
Add scale punch animation
```

---

## 📝 Code Integration Complete

The following has already been integrated:

✅ `CombatManager.cs`:
- FloatingDamageManager reference added
- Auto-find in `InitializeCombat()`
- `ShowDamage()` called in `PlayerAttackEnemy()`
- Passes damage amount, critical flag, and enemy position

✅ Components Created:
- `FloatingDamageText.cs` - Individual damage number
- `FloatingDamageManager.cs` - Spawning & pooling system

---

## 🎮 Next Steps

1. **Follow Steps 1-2** to create prefab and manager in Unity
2. **Enter Play Mode** and attack enemies
3. **Watch** for floating damage numbers
4. **Adjust** colors/timing to your preference
5. **Enjoy** the visual feedback! 🎯

---

**Floating damage numbers add great visual feedback to your combat system!** ✨












