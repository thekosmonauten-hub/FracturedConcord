# EncounterManager & Progression System - Improvement Recommendations

## Current System Analysis

### Strengths
- ✅ Clean separation of encounter data (Resources-based)
- ✅ Prerequisite-based unlocking system
- ✅ Character-specific progression tracking
- ✅ Event system for graph changes
- ✅ Support for multiple acts

### Areas for Improvement

---

## 1. **Separation of Concerns**

### Current Issue
`EncounterManager` handles too many responsibilities:
- Data loading from Resources
- Graph building and traversal
- Progression state management
- Scene loading
- Character synchronization

### Recommendation: Split into Focused Components

```csharp
// Proposed Structure:
EncounterDataLoader      // Handles loading from Resources
EncounterGraphBuilder    // Builds prerequisite/unlock relationships
EncounterProgressionManager  // Manages unlock/completion state
EncounterSceneLoader     // Handles scene transitions
```

**Benefits:**
- Easier to test individual components
- Clearer responsibilities
- Better maintainability
- Easier to extend with new features

---

## 2. **Enhanced Progression Tracking**

### Current Limitation
Only tracks basic completion/unlock state:
```csharp
List<int> completedEncounterIDs
List<int> unlockedEncounterIDs
```

### Recommendation: Rich Progression Data

```csharp
[System.Serializable]
public class EncounterProgressionData
{
    public int encounterID;
    public bool isCompleted;
    public bool isUnlocked;
    public int completionCount;
    public int attemptCount;
    public float bestCompletionTime; // seconds
    public System.DateTime firstCompletedDate;
    public System.DateTime lastCompletedDate;
    public int highestScore; // if you add scoring
    public Dictionary<string, object> customStats; // extensible
}

// In Character:
Dictionary<int, EncounterProgressionData> encounterProgression = new Dictionary<int, EncounterProgressionData>();
```

**Benefits:**
- Track replay statistics
- Enable achievement systems
- Support leaderboards
- Better analytics

---

## 3. **Flexible Unlock Conditions**

### Current Limitation
Only supports prerequisite-based unlocking:
```csharp
List<int> prerequisiteEncounterIDs
```

### Recommendation: Unlock Condition System

```csharp
public enum UnlockConditionType
{
    Prerequisite,      // Complete X encounters
    LevelRequirement,  // Reach character level X
    ItemRequirement,   // Own specific item
    QuestRequirement,  // Complete quest
    Custom             // Scriptable condition
}

[System.Serializable]
public class UnlockCondition
{
    public UnlockConditionType type;
    public int[] prerequisiteEncounterIDs; // for Prerequisite type
    public int requiredLevel;              // for LevelRequirement
    public string requiredItemName;        // for ItemRequirement
    public string customConditionScript;   // for Custom
}

// In EncounterData:
public List<UnlockCondition> unlockConditions = new List<UnlockCondition>();
```

**Benefits:**
- Support multiple unlock paths
- Level-gated content
- Item-based unlocks
- Quest integration
- Future extensibility

---

## 4. **Improved State Management**

### Current Issue
State is scattered across:
- `EncounterData.isUnlocked` (runtime state)
- `EncounterData.isCompleted` (runtime state)
- `Character.unlockedEncounterIDs` (persistent state)
- `Character.completedEncounterIDs` (persistent state)

### Recommendation: Single Source of Truth

```csharp
public class EncounterStateManager
{
    // Runtime state (derived from Character + EncounterData)
    private Dictionary<int, EncounterState> encounterStates = new Dictionary<int, EncounterState>();
    
    public struct EncounterState
    {
        public bool isUnlocked;
        public bool isCompleted;
        public bool isAvailable; // unlocked AND prerequisites met
        public string lockReason; // why it's locked
    }
    
    public void RefreshState(Character character)
    {
        // Rebuild all states from character progression + encounter data
        // Single method that ensures consistency
    }
}
```

**Benefits:**
- No state synchronization issues
- Easier debugging
- Clear state derivation logic

---

## 5. **Event System Enhancement**

### Current Events
```csharp
event System.Action EncounterGraphChanged;
```

### Recommendation: Comprehensive Event System

```csharp
public class EncounterEvents
{
    // Progression events
    public static event System.Action<int> OnEncounterUnlocked;
    public static event System.Action<int> OnEncounterCompleted;
    public static event System.Action<int> OnEncounterEntered;
    public static event System.Action<int> OnEncounterFailed;
    
    // State change events
    public static event System.Action<int, bool> OnEncounterLockStateChanged;
    
    // Graph events
    public static event System.Action OnEncounterGraphBuilt;
    public static event System.Action OnProgressionApplied;
}
```

**Benefits:**
- UI can react to specific events
- Analytics hooks
- Achievement triggers
- Better decoupling

---

## 6. **Data Validation & Integrity**

### Current Issue
No validation of:
- Duplicate encounter IDs
- Circular prerequisites
- Missing prerequisite encounters
- Invalid encounter references

### Recommendation: Validation System

```csharp
public class EncounterValidator
{
    public ValidationResult ValidateEncounterData(List<EncounterData> encounters)
    {
        var result = new ValidationResult();
        
        // Check for duplicate IDs
        var ids = encounters.Select(e => e.encounterID).ToList();
        var duplicates = ids.GroupBy(id => id).Where(g => g.Count() > 1);
        foreach (var dup in duplicates)
            result.AddError($"Duplicate encounter ID: {dup.Key}");
        
        // Check for circular prerequisites
        ValidatePrerequisiteCycles(encounters, result);
        
        // Check for missing prerequisites
        ValidatePrerequisiteReferences(encounters, result);
        
        return result;
    }
}
```

**Benefits:**
- Catch errors early
- Better debugging
- Data integrity guarantees

---

## 7. **Performance Optimizations**

### Current Issues
- Multiple dictionary lookups in `GetEncounterByID`
- Linear searches through act lists
- Repeated graph traversal

### Recommendations

```csharp
// Single unified lookup
private Dictionary<int, EncounterData> allEncounters = new Dictionary<int, EncounterData>();

// Cache unlock state
private Dictionary<int, bool> unlockCache = new Dictionary<int, bool>();

// Batch operations
public void BatchUpdateProgression(List<int> completedIDs)
{
    // Update multiple encounters at once
    // Single graph refresh
}
```

**Benefits:**
- Faster lookups
- Reduced CPU usage
- Better scalability

---

## 8. **Save/Load System Consistency**

### Current Issue
Mix of PlayerPrefs and Character data:
- `PlayerPrefs.GetInt("PendingEncounterID")` - temporary state
- `Character.completedEncounterIDs` - persistent state
- `EncounterData.isUnlocked` - runtime state

### Recommendation: Unified Save System

```csharp
[System.Serializable]
public class EncounterSaveData
{
    public Dictionary<int, EncounterProgressionData> progression;
    public int currentEncounterID;
    public System.DateTime lastSaveTime;
}

// Save/Load through Character or dedicated SaveManager
public void SaveEncounterProgression(Character character)
{
    var saveData = new EncounterSaveData
    {
        progression = character.encounterProgression,
        currentEncounterID = EncounterManager.Instance.currentEncounterID,
        lastSaveTime = System.DateTime.Now
    };
    // Save to character or dedicated save file
}
```

**Benefits:**
- Consistent save format
- Easier to debug saves
- Support for multiple save slots
- Better error recovery

---

## 9. **Testing & Debugging Tools**

### Recommendation: Debug/Test Utilities

```csharp
#if UNITY_EDITOR
public class EncounterDebugTools
{
    [MenuItem("Tools/Encounters/Unlock All")]
    public static void UnlockAllEncounters()
    {
        // Debug utility
    }
    
    [MenuItem("Tools/Encounters/Reset Progression")]
    public static void ResetProgression()
    {
        // Debug utility
    }
    
    [MenuItem("Tools/Encounters/Validate Graph")]
    public static void ValidateGraph()
    {
        // Validation utility
    }
    
    [MenuItem("Tools/Encounters/Export Progression Report")]
    public static void ExportProgressionReport()
    {
        // Analytics utility
    }
}
#endif
```

**Benefits:**
- Faster iteration
- Easier debugging
- Better testing

---

## 10. **Extensibility for Future Features**

### Recommendations

#### A. Encounter Variants
```csharp
public class EncounterVariant
{
    public string variantName;
    public int difficultyModifier;
    public LootTable variantLootTable;
}

// In EncounterData:
public List<EncounterVariant> variants = new List<EncounterVariant>();
```

#### B. Dynamic Encounters
```csharp
public interface IEncounterGenerator
{
    EncounterData GenerateEncounter(int areaLevel, Character character);
}
```

#### C. Seasonal/Event Encounters
```csharp
public class SeasonalEncounter : EncounterData
{
    public System.DateTime startDate;
    public System.DateTime endDate;
    public bool isActive => DateTime.Now >= startDate && DateTime.Now <= endDate;
}
```

---

## Implementation Priority

### Phase 1: Foundation (High Priority)
1. ✅ Separation of Concerns (split EncounterManager)
2. ✅ Enhanced Progression Tracking
3. ✅ Improved State Management

### Phase 2: Features (Medium Priority)
4. ✅ Flexible Unlock Conditions
5. ✅ Event System Enhancement
6. ✅ Data Validation

### Phase 3: Polish (Lower Priority)
7. ✅ Performance Optimizations
8. ✅ Save/Load Consistency
9. ✅ Debug Tools
10. ✅ Extensibility Features

---

## Quick Wins (Can Implement Now)

1. **Add validation method** to catch duplicate IDs
2. **Enhance events** - add OnEncounterUnlocked, OnEncounterCompleted
3. **Add completion tracking** - track attempt count, completion time
4. **Unified lookup** - single dictionary instead of searching acts
5. **Debug menu** - unlock all, reset progression utilities

---

## Example: Improved EncounterProgressionManager

```csharp
public class EncounterProgressionManager : MonoBehaviour
{
    private Dictionary<int, EncounterProgressionData> progression = new Dictionary<int, EncounterProgressionData>();
    
    public void MarkCompleted(int encounterID)
    {
        var data = GetOrCreateProgression(encounterID);
        data.isCompleted = true;
        data.completionCount++;
        data.lastCompletedDate = System.DateTime.Now;
        
        if (data.firstCompletedDate == default)
            data.firstCompletedDate = System.DateTime.Now;
        
        EncounterEvents.OnEncounterCompleted?.Invoke(encounterID);
        
        // Auto-unlock next encounters
        UnlockNextEncounters(encounterID);
    }
    
    public bool IsUnlocked(int encounterID)
    {
        if (encounterID == 1) return true; // Always unlocked
        
        var data = GetOrCreateProgression(encounterID);
        if (data.isUnlocked) return true;
        
        // Check unlock conditions
        return CheckUnlockConditions(encounterID);
    }
    
    private bool CheckUnlockConditions(int encounterID)
    {
        var encounter = EncounterManager.Instance.GetEncounterByID(encounterID);
        if (encounter == null) return false;
        
        foreach (var condition in encounter.unlockConditions)
        {
            if (!EvaluateCondition(condition))
                return false;
        }
        
        return true;
    }
}
```

---

## Summary

The current system is functional but could benefit from:
- **Better organization** (separation of concerns)
- **Richer data** (enhanced progression tracking)
- **More flexibility** (multiple unlock conditions)
- **Better state management** (single source of truth)
- **Improved tooling** (validation, debugging, testing)

These improvements would make the system more maintainable, extensible, and robust for future features.

