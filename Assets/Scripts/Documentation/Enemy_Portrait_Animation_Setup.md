# Enemy Portrait Animation Setup Guide

## Overview
This guide explains how to properly set up enemy portraits for animation and display in the combat scene, ensuring that the display data from `EnemyData` ScriptableObjects is correctly applied.

---

## 🎯 Problem & Solution Summary

### Issues You May Encounter:
1. **Animations not playing** - Missing or incorrectly placed Animator component
2. **Display scale not applied** - Portrait size doesn't match `displayScale` in EnemyData
3. **Animator Controller not assigned** - Missing RuntimeAnimatorController reference in EnemyData

### What Was Fixed:
✅ Enhanced Animator auto-detection (checks Image, parent, children, siblings)  
✅ Added display scale application to portrait sprites  
✅ Added comprehensive validation tool (right-click component → "Validate Enemy Display Setup")  
✅ Better debug logging to trace data flow  

---

## 📋 Required Components

### Scene Hierarchy (Example):
```
EnemyCombatDisplay (GameObject)
├── EnemyCombatDisplay (Script Component)
├── Enemy Portrait (Image)
│   └── Animator (Component) ← CRITICAL: Add this!
├── EnemyName (TextMeshProUGUI)
├── EnemyType (TextMeshProUGUI)
├── HealthBar (Slider)
└── IntentContainer (GameObject)
```

**Alternative Valid Structures:**
- Animator on the Enemy Portrait Image itself
- Animator on the parent of Enemy Portrait
- Animator on a child of Enemy Portrait  
- Animator on a sibling GameObject (will be auto-detected)

---

## 🔧 Step-by-Step Setup

### Step 1: Add Animator Component to Enemy Portrait

1. **Select the Enemy Portrait GameObject** in your scene
2. **Add Component** → Search for "Animator"
3. **Leave the Controller field EMPTY** - it will be assigned at runtime from EnemyData

**Why?** The system dynamically assigns the correct Animator Controller based on which enemy is displayed. Each enemy can have its own unique animation controller.

---

### Step 2: Configure EnemyData Asset

1. **Open your EnemyData asset** (e.g., `Assets/Resources/Enemies/BOSS_FirstToFall.asset`)
2. **Scroll to Visual section**
3. **Assign the following:**

```
[Visual]
├── Animator Controller: [Drag your RuntimeAnimatorController here]
│   (e.g., EnemySkeletonAnimator, BossFirstToFallAnimator, etc.)
│
└── Display Scale: 1.0 - 3.0
    (Controls both panel height AND sprite scale)
    
└── Base Panel Height: 280
    (Base height before scaling is applied)
```

**Example for Boss:**
- `animatorController`: `BossFirstToFallAnimator`
- `displayScale`: `1.8` (makes boss 80% larger)
- `basePanelHeight`: `320`

---

### Step 3: Create Animator Controller (if needed)

If you don't have an Animator Controller yet:

1. **Right-click in Project** → Create → Animator Controller
2. **Name it** (e.g., "SkeletonArcherAnimator")
3. **Open the Animator window** (Window → Animation → Animator)
4. **Create states:**
   - `Idle` (default state)
   - `Attack` (triggered by "Attack" Trigger parameter)
   - `Hit` (optional, triggered by "Hit" Trigger parameter)
   - `Death` (optional, controlled by "IsDead" Bool parameter)

5. **Add Parameters:**
   - `Attack` (Trigger) - **REQUIRED for attack animations**
   - `Hit` (Trigger) - Optional
   - `IsDead` (Bool) - Optional

6. **Create Transitions:**
   - `Idle → Attack` (Condition: Attack trigger)
   - `Attack → Idle` (Exit time: true, transition duration: 0.1s)

---

### Step 4: Assign Animation Clips

1. **Select each state in the Animator**
2. **In the Inspector, assign the Motion** (your animation clip)
   - Idle state → `SkeletonIdle` animation clip
   - Attack state → `SkeletonAttack` animation clip

**Where to get animation clips?**
- Create them using Unity's Animation window (Window → Animation → Animation)
- Import from sprite sheets using Unity's Sprite Editor
- Use existing clips from Asset Store

---

### Step 5: Validate Your Setup

1. **Select your EnemyCombatDisplay GameObject** in the scene
2. **Right-click the EnemyCombatDisplay component** in Inspector
3. **Select "Validate Enemy Display Setup"**
4. **Check the Console** for validation results

**What to look for:**
```
✓ Enemy Portrait: Assigned
✓ Animator: Found on 'EnemyPortrait'
✓ Enemy Data: Boss Name
  ✓ Sprite: boss_sprite
  ✓ Display Scale: 1.8
  ✓ Animator Controller: BossAnimator
```

**Common Issues:**
```
✗ NO Animator component found!
→ Add Animator component to Enemy Portrait or its children

✗ Animator Controller: NOT ASSIGNED - ANIMATIONS WILL NOT WORK!
→ Open EnemyData asset and assign an Animator Controller under Visual section
```

---

## 🎮 How It Works at Runtime

### Data Flow:
1. **CombatManager spawns enemy** → Calls `EnemyCombatDisplay.SetEnemyFromData(enemyData)`
2. **SetEnemyFromData** stores reference to `EnemyData`
3. **SetupEnemyAnimator** is called:
   - Auto-finds Animator component (if not manually assigned)
   - Assigns `enemyData.animatorController` to Animator
4. **UpdateDisplay** is called:
   - Sets sprite from `enemyData.enemySprite`
   - Applies `displayScale` to portrait's RectTransform.localScale
   - Applies `basePanelHeight * displayScale` to panel's LayoutElement
5. **During combat:**
   - `PlayAttackAnimation()` → Sets "Attack" trigger
   - `PlayHitAnimation()` → Sets "Hit" trigger  
   - `PlayDeathAnimation()` → Sets "IsDead" bool to true

---

## 🐛 Troubleshooting

### Problem: Animations don't play

**Diagnosis:**
1. Run validation (right-click component → Validate Enemy Display Setup)
2. Check Console for warnings

**Solutions:**

**If "NO Animator component found":**
- Add Animator component to Enemy Portrait GameObject
- Or add it to a parent/child/sibling (auto-detection will find it)

**If "Animator Controller: NOT ASSIGNED":**
- Open the EnemyData asset in Inspector
- Under `Visual > Animator Controller`, assign your RuntimeAnimatorController

**If Animator exists but animations still don't play:**
- Check the Animator Controller has an "Attack" Trigger parameter
- Verify the Attack state has an animation clip assigned
- Ensure there's a transition from Idle → Attack with "Attack" as condition

---

### Problem: Enemy portrait is the wrong size

**Diagnosis:**
- Check `EnemyData.displayScale` value
- Check if RectTransform was manually resized (this will be overridden)

**Solutions:**
1. **Open the EnemyData asset**
2. **Adjust `displayScale`:**
   - `0.5` = 50% size (small enemies)
   - `1.0` = 100% size (normal)
   - `1.8` = 180% size (bosses)
3. **Adjust `basePanelHeight`** if the entire panel needs to be taller
4. **DO NOT manually resize the portrait in the scene** - it will be overridden at runtime

---

### Problem: Sprite doesn't appear

**Diagnosis:**
- Validation shows "Sprite: ✗ MISSING"

**Solutions:**
1. Open EnemyData asset
2. Under `Basic Info > Enemy Sprite`, drag in a sprite
3. Ensure the sprite is set to Sprite (2D/UI) type in Import Settings

---

## 📝 Quick Reference Checklist

**Before Testing:**
- [ ] Animator component exists on Enemy Portrait (or nearby GameObject)
- [ ] EnemyData has Animator Controller assigned (Visual section)
- [ ] EnemyData has Enemy Sprite assigned
- [ ] Animator Controller has "Attack" Trigger parameter
- [ ] Attack state has an animation clip assigned
- [ ] Idle → Attack transition exists with "Attack" trigger condition
- [ ] Display scale is set appropriately (0.5 - 3.0 range)

**Run Validation:**
- [ ] Right-click EnemyCombatDisplay component
- [ ] Select "Validate Enemy Display Setup"
- [ ] Fix any warnings/errors shown in Console

---

## 🎨 Example: Setting Up a Boss Enemy

Let's set up "The First to Fall" boss as an example:

### 1. Create Animator Controller
```
Assets/Animations/Enemies/
└── BossFirstToFallAnimator.controller
```

### 2. Configure Animator States
- **Idle** (default): `boss_idle_anim` clip
- **Attack**: `boss_attack_anim` clip  
- **Death**: `boss_death_anim` clip

### 3. Add Parameters
- `Attack` (Trigger)
- `IsDead` (Bool)

### 4. Configure EnemyData
```
Assets/Resources/Enemies/BOSS_FirstToFall.asset

Basic Info:
- Enemy Name: "The First to Fall"
- Enemy Sprite: boss_firsttofall_sprite
- Rarity: Unique
- Tier: Boss

Display:
- Display Scale: 1.8 (80% larger than normal)
- Base Panel Height: 320

Visual:
- Animator Controller: BossFirstToFallAnimator
- Health Bar Color: Red
```

### 5. Validate
- Select EnemyCombatDisplay in scene
- Right-click component → Validate Enemy Display Setup
- Confirm all checkmarks are green

### 6. Test
- Enter Play Mode
- Enemy should appear with correct size
- Attack animations should play when enemy attacks

---

## 🚀 Best Practices

1. **Keep Animator Controllers Reusable**
   - One controller per enemy type (not per enemy instance)
   - Example: "HumanoidEnemyAnimator" used by Skeleton, Zombie, Guard

2. **Use Consistent Parameter Names**
   - Always use "Attack" trigger for attacks
   - Always use "Hit" trigger for taking damage
   - Always use "IsDead" bool for death

3. **Test with Validation Tool**
   - Run validation before committing changes
   - Validation catches 90% of setup issues

4. **Use Display Scale, Not Manual Sizing**
   - Configure size in EnemyData.displayScale
   - Avoids scene-specific sizes that get overridden
   - Makes it easy to adjust all instances at once

5. **Separate Sprite from Animation**
   - EnemyData.enemySprite = Static display sprite
   - Animator Controller = Handles animation playback
   - Both work together for animated portraits

---

## 📞 Need Help?

If you're still encountering issues:

1. **Run Validation** and share Console output
2. **Check these common mistakes:**
   - Animator component in wrong location (must be on/near portrait)
   - AnimatorController not assigned in EnemyData
   - Missing "Attack" trigger parameter
   - No transition from Idle to Attack state
3. **Review the logs** - Debug.Log statements will show exactly what's happening

---

**Last Updated:** 2025-10-21  
**Compatibility:** Unity 2022.3+, Dexiled Combat System v2.x














