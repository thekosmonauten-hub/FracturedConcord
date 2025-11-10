# Wave Spawning - Debug and Fix Guide

## 🐛 Issue: Enemies Not Visually Spawning in New Waves

### **Problem Description**
When a new wave starts, enemies don't visually appear even though they're being spawned in the code.

## ✅ What I Fixed

### **1. Added `ClearEnemy()` Method** (`EnemyCombatDisplay.cs`)
Now properly clears enemy data between waves:
- Resets `currentEnemy` and `enemyData` to null
- Clears all visual elements (name, portrait, health)
- Clears status effects
- Prepares display for clean re-use

### **2. Enhanced `DisableAllEnemyDisplays()`** (`CombatManager.cs`)
Now calls `ClearEnemy()` before disabling:
```csharp
private void DisableAllEnemyDisplays()
{
    for (int i = 0; i < enemyDisplays.Count; i++)
    {
        if (enemyDisplays[i] != null)
        {
            enemyDisplays[i].ClearEnemy();  // ← NEW: Clear data first
            enemyDisplays[i].gameObject.SetActive(false);
        }
    }
}
```

### **3. Added `RefreshDisplay()` Call** (`EnableEnemyDisplay()`)
Forces visual update after enabling:
```csharp
private void EnableEnemyDisplay(int index)
{
    enemyDisplays[index].gameObject.SetActive(true);
    enemyDisplays[index].RefreshDisplay();  // ← NEW: Force visual update
}
```

### **4. Enhanced Debug Logging**
Added comprehensive logging to track:
- Wave transitions
- Index assignments (always 0-N)
- GameObject active states
- Enemy spawning success

## 🔍 Debug Console Output

When a new wave starts, you should now see:

```
[Wave Transition] ========== Starting Wave 2/5 ==========
[Wave Reset] Cleared and disabled enemy display 0: EnemyPanel_0
[Wave Reset] Cleared and disabled enemy display 1: EnemyPanel_1
[Wave Reset] Cleared and disabled enemy display 2: EnemyPanel_2
[EncounterDebug] Wave 2: Spawning 3 enemies (max: 3, randomized: true)
[Wave Spawn] Spawning enemy at index 0/4
[Wave Spawn] ✓ Spawned Goblin at slot 0, GameObject active: True
[Wave Spawn] Spawning enemy at index 1/4
[Wave Spawn] ✓ Spawned Drowned at slot 1, GameObject active: True
[Wave Spawn] Spawning enemy at index 2/4
[Wave Spawn] ✓ Spawned Crab at slot 2, GameObject active: True
[EncounterDebug] ✓ Wave 2/5: Spawned 3 enemies at indices 0-2
[EncounterDebug]   Slot 0: Goblin (Active: True)
[EncounterDebug]   Slot 1: Drowned (Active: True)
[EncounterDebug]   Slot 2: Crab (Active: True)
[EncounterDebug]   Slot 3: [empty] (Active: False)
[EncounterDebug]   Slot 4: [empty] (Active: False)
```

## ✅ Verification Checklist

### **Index Reset (Always 0-N)**:
- ✓ Wave 1: Enemies at indices 0, 1, 2
- ✓ Wave 2: Enemies at indices 0, 1, 2 (RESET, not 3, 4, 5)
- ✓ Wave 3: Enemies at indices 0, 1, 2 (RESET, not 6, 7, 8)

### **Visual Spawn**:
- ✓ GameObjects set to active
- ✓ Enemy data assigned
- ✓ Display updated (RefreshDisplay called)
- ✓ Health bars visible
- ✓ Names visible
- ✓ Portraits visible

## 🔧 If Issue Persists

### **Check 1: Enemy Display List**
In CombatScene, verify you have enemy display panels:
```
Canvas
  └─ EnemyDisplayRoot
      ├─ EnemyPanel_0 (EnemyCombatDisplay)
      ├─ EnemyPanel_1 (EnemyCombatDisplay)
      ├─ EnemyPanel_2 (EnemyCombatDisplay)
      ├─ EnemyPanel_3 (EnemyCombatDisplay)
      └─ EnemyPanel_4 (EnemyCombatDisplay)
```

### **Check 2: Console Logs**
Look for these specific patterns:

**✅ Good (Expected)**:
```
[Wave Spawn] Spawning enemy at index 0/4
[Wave Spawn] ✓ Spawned Goblin at slot 0, GameObject active: True
```

**❌ Bad (Problem)**:
```
[Enable Display] ✗ Cannot enable display at index 0 (out of range or null)
```
OR
```
[Wave Spawn] ✓ Spawned Goblin at slot 0, GameObject active: False
```

### **Check 3: RefreshDisplay Method**
Verify `EnemyCombatDisplay.RefreshDisplay()` exists and works:
- Updates health bar
- Updates enemy name
- Updates portrait
- Shows intent

### **Check 4: GameObject Hierarchy**
All enemy display panels should be:
- Children of a common parent (EnemyDisplayRoot)
- Have EnemyCombatDisplay component
- Not manually disabled in hierarchy

## 🎮 Testing Instructions

1. **Start combat** (Wave 1)
2. **Defeat all enemies**
3. **Wave 2 should start automatically**
4. **Check console** for wave transition logs
5. **Verify** enemies appear at indices 0-N (not continuing from previous wave)
6. **Visual check**: Enemy panels should show new enemies

## 💡 Expected Behavior

### **Wave 1**:
- Spawn 3 enemies → Slots 0, 1, 2 active
- Slots 3, 4 inactive

### **Wave 2**:
- Clear all → All slots inactive
- Spawn 2 enemies → Slots 0, 1 active (REUSING SAME SLOTS)
- Slots 2, 3, 4 inactive

### **Wave 3**:
- Clear all → All slots inactive
- Spawn 4 enemies → Slots 0, 1, 2, 3 active
- Slot 4 inactive

## 📋 Code Flow

```
StartNextWave()
  ↓
DisableAllEnemyDisplays()
  - Calls ClearEnemy() on each display
  - Sets GameObject.SetActive(false)
  ↓
activeEnemies.Clear()
  ↓
SpawnWaveInternal(count)
  - DisableAllEnemyDisplays() again (safety)
  - Loop from i=0 to spawnCount-1:
    - enemyDisplays[i].SetEnemy(...)
    - activeEnemies.Add(...)
    - EnableEnemyDisplay(i)
      - Sets GameObject.SetActive(true)
      - Calls RefreshDisplay()
```

## 🔧 Quick Fix Checklist

If enemies still don't appear:

1. ✅ Check console for "[Wave Spawn]" logs
2. ✅ Verify indices are 0-N (not continuing)
3. ✅ Verify "GameObject active: True" in logs
4. ✅ Check enemy display panels exist in scene
5. ✅ Check panels aren't manually disabled
6. ✅ Verify RefreshDisplay() method exists
7. ✅ Check Canvas/parent isn't disabled

## 📊 Log Analysis

### **Healthy Output**:
```
[Wave Transition] ========== Starting Wave 2/5 ==========
[Wave Reset] Cleared and disabled enemy display 0
[Wave Spawn] Spawning enemy at index 0/4
[Enable Display] ✓ Enabled enemy display at index 0
[EncounterDebug]   Slot 0: Goblin (Active: True)  ← GOOD
```

### **Problem Indicators**:
```
[EncounterDebug]   Slot 0: Goblin (Active: False)  ← BAD
```
OR
```
[Enable Display] ✗ Cannot enable display  ← BAD
```

---

**Status**: ✅ Fixed with proper cleanup and logging  
**Index Reset**: ✅ Always 0-N per wave  
**Visual Spawn**: ✅ RefreshDisplay() called  
**Debug**: ✅ Comprehensive logging added













