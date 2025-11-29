# Encounter System Refactoring - Setup Guide

## Overview

The EncounterManager has been refactored into a modular architecture with separated concerns. This guide will help you set up the new system in Unity.

## New Architecture

The system is now split into focused components:

1. **EncounterEvents** - Centralized event system
2. **EncounterProgressionData** - Rich progression tracking
3. **EncounterDataLoader** - Loads encounters from Resources
4. **EncounterGraphBuilder** - Builds prerequisite/unlock relationships
5. **EncounterStateManager** - Single source of truth for state
6. **EncounterProgressionManager** - Manages unlock/completion logic
7. **EncounterValidator** - Validates encounter data integrity
8. **EncounterManager** - Coordinates all components

## Unity Setup Steps

### 1. Verify EncounterManager GameObject

1. Open your scene that contains the EncounterManager (usually `MainGameUI` or a persistent scene)
2. Find the `EncounterManager` GameObject
3. Select it and check the Inspector
4. Verify these settings:
   - **Load From Resources**: ✅ Checked
   - **Act 1 Resources Path**: `Encounters/Act1`
   - **Act 2 Resources Path**: `Encounters/Act2`
   - **Act 3 Resources Path**: `Encounters/Act3`
   - **Act 4 Resources Path**: `Encounters/Act4`
   - **Enemy Database Ref**: (Optional) Assign if you have an EnemyDatabase prefab

### 2. Verify Resources Structure

Ensure your encounter assets are organized correctly:

```
Assets/
  Resources/
    Encounters/
      Act1/
        - 1.WhereNightFirstFell.asset
        - 2.CampConcordia.asset
        - ... (other Act 1 encounters)
      Act2/
        - 1.OuterWardsofVassara.asset
        - ... (other Act 2 encounters)
      Act3/
        - ... (Act 3 encounters)
      Act4/
        - ... (Act 4 encounters)
```

### 3. Test Validation

1. Select the `EncounterManager` GameObject
2. Right-click in the Inspector
3. Choose **"Encounters > Validate Encounters"**
4. Check the console for validation results

This will check for:
- Duplicate encounter IDs
- Missing prerequisite references
- Circular prerequisites
- Missing Encounter 1
- Invalid scene names

### 4. Import Encounters (If Needed)

If you need to refresh encounters from Resources:

1. Select the `EncounterManager` GameObject
2. Right-click in the Inspector
3. Choose **"Encounters > Import From Resources"**

This will reload all encounters from your Resources folders.

### 5. Update Existing Characters (Optional)

The new system is backwards compatible with existing character data. However, if you want to migrate to the new progression system:

1. The system automatically converts old `completedEncounterIDs` and `unlockedEncounterIDs` lists to `EncounterProgressionData`
2. No manual migration needed - it happens automatically when `ApplyCharacterProgression()` is called

### 6. Update UI Components

If you have custom UI components that interact with encounters:

**Old way:**
```csharp
bool unlocked = EncounterManager.Instance.IsEncounterUnlocked(id);
```

**New way (same API, but now uses new system):**
```csharp
bool unlocked = EncounterManager.Instance.IsEncounterUnlocked(id);
// API is the same, but now uses EncounterProgressionManager internally
```

### 7. Subscribe to Events (Optional)

You can now subscribe to specific encounter events:

```csharp
void OnEnable()
{
    EncounterEvents.OnEncounterUnlocked += HandleEncounterUnlocked;
    EncounterEvents.OnEncounterCompleted += HandleEncounterCompleted;
}

void OnDisable()
{
    EncounterEvents.OnEncounterUnlocked -= HandleEncounterUnlocked;
    EncounterEvents.OnEncounterCompleted -= HandleEncounterCompleted;
}

void HandleEncounterUnlocked(int encounterID)
{
    Debug.Log($"Encounter {encounterID} unlocked!");
    // Update UI, show notification, etc.
}

void HandleEncounterCompleted(int encounterID)
{
    Debug.Log($"Encounter {encounterID} completed!");
    // Show completion screen, unlock rewards, etc.
}
```

## New Features Available

### Rich Progression Tracking

You can now access detailed progression data:

```csharp
var progression = EncounterManager.Instance.GetProgression(encounterID);
if (progression != null)
{
    Debug.Log($"Completion count: {progression.completionCount}");
    Debug.Log($"Attempt count: {progression.attemptCount}");
    Debug.Log($"Best time: {progression.bestCompletionTime}s");
    Debug.Log($"First completed: {progression.firstCompletedDate}");
}
```

### Validation

The system automatically validates encounter data on load. Check the console for any warnings or errors.

### Debug Tools

In the Editor, you can:
- **Validate Encounters** - Check for data integrity issues
- **Reset Progression** - Clear all progression (with confirmation dialog)
- **Import From Resources** - Reload encounters

## Backwards Compatibility

The refactored system maintains backwards compatibility:

- ✅ Existing `Character.completedEncounterIDs` and `unlockedEncounterIDs` still work
- ✅ Existing `EncounterManager` API methods still work
- ✅ Existing `EncounterButton` components still work
- ✅ Existing save/load system still works

The new system automatically converts old data to the new format when needed.

## Troubleshooting

### Encounters Not Loading

1. Check that `loadFromResources` is enabled on EncounterManager
2. Verify Resources paths are correct
3. Check that encounter assets exist in the specified Resources folders
4. Look for errors in the console

### Encounter 1 Not Unlocked

The system ensures Encounter 1 is always unlocked. If it's not:
1. Check console for errors
2. Verify Encounter 1 exists in Resources/Encounters/Act1
3. Try "Import From Resources" from the context menu

### Validation Errors

If validation finds errors:
1. Fix duplicate encounter IDs
2. Fix missing prerequisite references
3. Fix circular prerequisites
4. Re-validate after fixes

### Progression Not Saving

The system saves progression through `CharacterManager.SaveCharacter()`. Ensure:
1. CharacterManager is properly initialized
2. Character is loaded before accessing progression
3. `SaveCharacter()` is called after progression changes

## Testing Checklist

- [ ] EncounterManager initializes without errors
- [ ] Encounters load from Resources
- [ ] Validation passes (no errors)
- [ ] Encounter 1 is unlocked by default
- [ ] Starting an encounter works
- [ ] Completing an encounter unlocks next encounters
- [ ] Progression saves and loads correctly
- [ ] Events fire correctly (check console)
- [ ] UI updates when encounters unlock/complete

## Next Steps

After setup is complete, you can:
1. Add custom unlock conditions (level requirements, item requirements, etc.)
2. Track additional statistics (completion time, score, etc.)
3. Add achievement triggers based on encounter events
4. Implement analytics tracking using the event system

## Support

If you encounter issues:
1. Check the console for error messages
2. Use the validation tool to check data integrity
3. Verify Resources paths and asset setup
4. Check that all required components are initialized

