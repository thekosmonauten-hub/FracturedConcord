# 🎯 UI Toolkit vs Canvas Integration Guide

## The Problem
You're using **UI Toolkit** for the main character creation interface, but trying to use **Canvas (UGUI)** for card hover previews. This can cause conflicts and issues.

## 🎮 Three Integration Approaches

### **Option 1: Pure UI Toolkit (Recommended)**
**Best for**: Full UI Toolkit integration, no Canvas conflicts

**Setup:**
```
Inspector Settings:
✅ Full Card Prefab → [Leave Empty]
❌ Deck Card Prefab → [Leave Empty]  
❌ Card Preview Canvas → [Leave Empty]
```

**How it works:**
- Uses UI Toolkit VisualElements for deck preview
- Hover shows full card details in UI Toolkit
- No Canvas/UGUI conflicts
- Perfect integration with existing UI

**Pros:**
- ✅ No rendering conflicts
- ✅ Consistent with main UI
- ✅ Better performance
- ✅ No Canvas setup needed

**Cons:**
- ❌ Less visual fidelity than UGUI cards
- ❌ Limited to UI Toolkit styling

### **Option 2: UI Toolkit + UGUI Hover (Hybrid)**
**Best for**: UI Toolkit deck preview with UGUI card hover

**Setup:**
```
Inspector Settings:
✅ Full Card Prefab → CardPrefab.prefab
❌ Deck Card Prefab → [Leave Empty]
❌ Card Preview Canvas → [Leave Empty]
```

**How it works:**
- UI Toolkit VisualElements for deck preview
- UGUI Canvas for hover card preview
- Minimal Canvas usage (only for hover)

**Pros:**
- ✅ UI Toolkit deck preview (consistent)
- ✅ UGUI card hover (high fidelity)
- ✅ Minimal Canvas conflicts
- ✅ Best of both worlds

**Cons:**
- ⚠️ Still some Canvas/UI Toolkit mixing
- ⚠️ Requires Canvas setup for hover

### **Option 3: Full UGUI (Current)**
**Best for**: Maximum visual fidelity, willing to handle Canvas conflicts

**Setup:**
```
Inspector Settings:
✅ Full Card Prefab → CardPrefab.prefab
✅ Deck Card Prefab → DeckCardPrefab.prefab
✅ Card Preview Canvas → [Your Canvas]
```

**How it works:**
- UGUI card prefabs for deck preview
- UGUI Canvas for hover preview
- Full visual fidelity

**Pros:**
- ✅ Maximum visual fidelity
- ✅ Rich card interactions
- ✅ Full UGUI features

**Cons:**
- ❌ Canvas/UI Toolkit conflicts
- ❌ Event system issues
- ❌ Complex setup
- ❌ Performance overhead

## 🔧 **Recommended Solution**

For your UI Toolkit-based character creation, I recommend **Option 2 (Hybrid)**:

### **Why Hybrid is Best:**
1. **UI Toolkit deck preview** - consistent with main UI
2. **UGUI hover preview** - high-quality card display
3. **Minimal Canvas usage** - reduces conflicts
4. **Easy setup** - just assign Full Card Prefab

### **Setup Steps:**
1. **Assign only `Full Card Prefab`** in Inspector
2. **Leave other fields empty** (Deck Card Prefab, Canvas)
3. **System auto-detects** and uses UI Toolkit + UGUI hover
4. **Test hover functionality** - should work smoothly

## 🎯 **Canvas Sorting Order Fix**

If you must use Canvas, ensure proper sorting:

```
Canvas Hierarchy:
├── Main UI Canvas (Sort Order: 0) - UI Toolkit
├── Card Hover Canvas (Sort Order: 100) - UGUI Hover
└── Other UI Elements
```

## 🚀 **Quick Test**

**To test which mode you're using:**
1. Check console for: `"Creating UI Toolkit deck preview with hover"`
2. If you see this message, you're using the hybrid approach
3. Hover should work with minimal Canvas conflicts

The hybrid approach gives you the best of both worlds! 🎯











