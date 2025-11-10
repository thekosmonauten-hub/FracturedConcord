# Combat Manager Migration Status

## ✅ What's Been Done:

1. **Added EnemySpawner reference** to CombatDisplayManager
2. **Removed static enemy displays list**
3. **Updated SpawnWaveInternal** to use dynamic spawning
4. **Removed old display management methods**
5. **Updated InitializeCombat** to validate spawner

##❌ What Still Needs Fixing:

### Remaining `enemyDisplays` References:

These lines still reference the old `enemyDisplays` list and need updating:

**Line 446:** `EnemyDoActionCoroutine`
```csharp
// OLD:
EnemyCombatDisplay enemyDisplay = (enemyIndex >= 0 && enemyIndex < enemyDisplays.Count) ? enemyDisplays[enemyIndex] : null;

// NEW:
var activeDisplays = enemySpawner.GetActiveEnemies();
EnemyCombatDisplay enemyDisplay = (enemyIndex >= 0 && enemyIndex < activeDisplays.Count) ? activeDisplays[enemyIndex] : null;
```

**Lines 524-534:** `PlayerAttackEnemy`
```csharp
// OLD:
if (combatEffectManager != null && enemyIndex < enemyDisplays.Count)
{
    combatEffectManager.PlayElementalDamageEffectOnTarget(enemyDisplays[enemyIndex].transform, DamageType.Physical, wasCritical);
}

if (enemyIndex < enemyDisplays.Count)
{
    enemyDisplays[enemyIndex].TakeDamage(damage);
    enemyDisplays[enemyIndex].PlayDamageAnimation();
}

// NEW:
var activeDisplays = enemySpawner.GetActiveEnemies();
if (combatEffectManager != null && enemyIndex < activeDisplays.Count)
{
    combatEffectManager.PlayElementalDamageEffectOnTarget(activeDisplays[enemyIndex].transform, DamageType.Physical, wasCritical);
}

if (enemyIndex < activeDisplays.Count)
{
    activeDisplays[enemyIndex].TakeDamage(damage);
    activeDisplays[enemyIndex].PlayDamageAnimation();
}
```

**Line 541:** Enemy loot check
```csharp
// OLD:
var disp = (enemyIndex < enemyDisplays.Count) ? enemyDisplays[enemyIndex] : null;

// NEW:
var activeDisplays = enemySpawner.GetActiveEnemies();
var disp = (enemyIndex < activeDisplays.Count) ? activeDisplays[enemyIndex] : null;
```

**Line 563:** When enemy is defeated
```csharp
// OLD:
DisableEnemyDisplay(enemyIndex);

// NEW:
DespawnEnemyAtIndex(enemyIndex);
```

**Line 777:** `RefreshAllDisplays`
```csharp
// OLD:
foreach (EnemyCombatDisplay enemyDisplay in enemyDisplays)
{
    if (enemyDisplay != null)
    {
        enemyDisplay.RefreshDisplay();
    }
}

// NEW:
if (enemySpawner != null)
{
    foreach (EnemyCombatDisplay enemyDisplay in enemySpawner.GetActiveEnemies())
    {
        if (enemyDisplay != null)
        {
            enemyDisplay.RefreshDisplay();
        }
    }
}
```

**Lines 810-816:** `AddEnemy` (legacy method, consider removing)
```csharp
// This whole method can likely be removed or rewritten to use spawner
```

**Line 827:** `RemoveEnemy` (legacy method, consider removing)
```csharp
// This whole method can likely be removed or rewritten to use spawner
```

**Lines 156-158:** `StartFirstWave`
```csharp
// OLD:
DisableAllEnemyDisplays();

// NEW:
if (enemySpawner != null)
{
    enemySpawner.DespawnAllEnemies();
}
```

**Lines 184-187:** `StartNextWaveCoroutine`
```csharp
// OLD:
DisableAllEnemyDisplays();

// NEW:
if (enemySpawner != null)
{
    enemySpawner.DespawnAllEnemies();
}
```

**Line 647-648:** `AdvanceAllStatusEffects`
```csharp
// This is actually fine - it creates a local variable with the same name
// But should be renamed to avoid confusion:
EnemyCombatDisplay[] activeEnemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
foreach (var enemyDisplay in activeEnemyDisplays)
```

---

## 🎯 Next Steps:

### Option 1: I Can Fix It
Let me know and I'll update all these references to use the spawner system.

###Option 2: Manual Fix
Follow the patterns above to update each reference manually.

### Option 3: Hybrid Approach
Keep a cached reference to active displays in CombatManager:
```csharp
private List<EnemyCombatDisplay> GetActiveEnemyDisplays()
{
    return enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
}
```

Then replace `enemyDisplays` with `GetActiveEnemyDisplays()` throughout the code.

---

## 📊 Migration Progress:

- [x] Remove static enemy displays list
- [x] Add EnemySpawner reference
- [x] Update SpawnWaveInternal
- [x] Remove old display management methods
- [ ] Update EnemyDoActionCoroutine (Line 446)
- [ ] Update PlayerAttackEnemy (Lines 524-534)
- [ ] Update loot check (Line 541)
- [ ] Update defeat handling (Line 563)
- [ ] Update RefreshAllDisplays (Line 777)
- [ ] Update/Remove AddEnemy (Line 810)
- [ ] Update/Remove RemoveEnemy (Line 827)
- [ ] Update StartFirstWave (Line 156)
- [ ] Update StartNextWaveCoroutine (Line 184)
- [ ] Rename variable in AdvanceAllStatusEffects (Line 647)

---

## ⚠️ Important Notes:

1. **Don't forget Unity setup**: After code changes, you need to:
   - Create EnemyDisplayPrefab
   - Setup spawn points in scene
   - Assign spawner reference in Inspector

2. **Test incrementally**: Fix compilation errors one at a time

3. **Backup first**: Commit current changes before continuing

---

**Would you like me to finish the migration by updating all these references?**












