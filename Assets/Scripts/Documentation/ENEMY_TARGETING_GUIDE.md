# Enemy Targeting System - Quick Guide

## 🎯 Overview

Click enemy panels to select targets for your cards!

---

## 🛠️ Setup (2 minutes)

### Step 1: Create EnemyTargetingManager

```
1. Combat Scene → Create GameObject: "EnemyTargetingManager"
2. Add Component: EnemyTargetingManager
3. All references auto-find!
```

**Done! Targeting is ready!** ✓

---

## 🎮 How to Use

### **Click Enemy Panels to Target**
```
1. Press Play ▶
2. Enemy panels appear
3. Click an enemy panel → Yellow outline appears! ⭐
4. That enemy is now targeted!
5. Play a card → Flies to targeted enemy! 🎴→🦀
```

### **Keyboard Shortcuts**
```
Tab      → Cycle to next enemy
1, 2, 3  → Select enemy 1, 2, or 3
```

---

## 🎨 Visual Feedback

### **Targeted Enemy:**
```
🦀 [Yellow Outline]  ← Selected!
   CoconutCrab
   HP: 45/50
   Intent: Attack
```

### **Other Enemies:**
```
🦀 [White/Normal]
   CoconutCrab
   HP: 50/50
```

---

## 🎬 Full Workflow

```
1. Combat starts
   → First enemy auto-selected (yellow outline)
   
2. Click different enemy panel
   → Selection changes (outline moves)
   
3. Click a card
   → Card flies to selected enemy ✓
   → Deals damage to selected enemy ✓
   
4. Enemy defeated
   → Auto-selects next alive enemy ✓
```

---

## ⚙️ Features

### **Auto-Target on Defeat**
```
Targeted enemy dies → Automatically selects next alive enemy ✓
```

### **Can't Target Dead Enemies**
```
Click dead enemy → Ignores click, stays on current target
```

### **Keyboard Shortcuts**
```
Tab → Cycle through enemies (1 → 2 → 3 → 1)
1/2/3 → Jump to specific enemy
```

### **Visual Highlight**
```
Selected enemy gets:
  ✓ Yellow outline
  ✓ Color tint (if configured)
  ✓ Clear visual feedback
```

---

## 🔧 Customization

**Select EnemyTargetingManager, Inspector:**

```
Targeting Settings
├── Normal Color: White        ← Non-targeted
├── Targeted Color: Yellow     ← Selected enemy
└── Hover Color: Light Yellow  ← (Future: hover preview)
```

---

## 🎯 How It Works

### **Before (Auto-Target):**
```
Play card → Targets first enemy always ❌
Can't choose which enemy to hit!
```

### **After (Manual Target):**
```
1. Click Enemy 2 panel → Enemy 2 highlighted
2. Play card → Flies to Enemy 2! ✓
3. Enemy 2 takes damage ✓
4. Click Enemy 1 panel → Enemy 1 highlighted
5. Play next card → Flies to Enemy 1! ✓
```

---

## 🐛 Troubleshooting

### Enemy panel not clickable?
```
✓ Check enemy panel has Collider or is within Canvas
✓ EnemyTargetingManager adds Button component automatically
✓ Check console for "Setup targeting for enemy panel X"
```

### No visual highlight?
```
✓ Make sure enemy panel has Image components
✓ Check if Outline component is added (auto-added)
✓ Adjust colors in EnemyTargetingManager settings
```

### Target not changing?
```
Right-click EnemyTargetingManager → "Show Current Target"
Console shows which enemy is targeted
```

---

## 🎮 Testing

### Quick Test:
```
1. Press Play
2. Wait for cards to draw
3. Click enemy panel 2 → Should highlight
4. Play a card → Should hit enemy 2
5. Press Tab → Should cycle to next enemy
6. Play another card → Should hit new target
```

### Debug Commands:
```
Right-click EnemyTargetingManager:
  → "Select Next Enemy" (cycles target)
  → "Show Current Target" (shows current target in console)
```

---

## 📊 Integration

**Works with:**
- ✅ CombatDeckManager (card play)
- ✅ CardEffectProcessor (damage application)
- ✅ CombatDisplayManager (enemy panels)
- ✅ All card types (Attack, Skill, etc.)

---

## ✅ Setup Checklist

- [ ] Create EnemyTargetingManager GameObject
- [ ] Add EnemyTargetingManager component
- [ ] Press Play
- [ ] Click enemy panels to test
- [ ] Play cards to test targeting
- [ ] Press Tab to test cycling

---

**That's it! Click enemy panels to target them!** 🎯✨

