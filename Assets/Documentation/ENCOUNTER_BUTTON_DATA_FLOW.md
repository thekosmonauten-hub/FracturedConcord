# EncounterButton Data Flow - How It Loads Data from EncounterManager

## Overview

`EncounterButton` loads encounter data from `EncounterManager` through several mechanisms:

1. **Direct API calls** - Queries `EncounterManager.Instance` for encounter data
2. **Event subscriptions** - Listens for graph changes to refresh
3. **Auto-sync** - Automatically syncs data when `autoSyncEncounterData` is enabled

## Data Loading Flow

### 1. Initialization (OnEnable)

When the button is enabled, it:

```csharp
private void OnEnable()
{
    TryAssignReferences();
    
    // Step 1: Load from EncounterDataAsset (if assigned)
    if (autoSyncEncounterData)
    {
        ResolveEncounterFromAsset(); // Loads from ScriptableObject asset
    }
    
    WireClickHandler();
    EnsureEventSystemAndRaycaster();
    HookManagerEvents();
    
    // Step 2: Subscribe to EncounterManager events
    if (EncounterManager.Instance != null)
    {
        EncounterManager.Instance.EncounterGraphChanged += HandleEncounterGraphChanged;
    }
    
    // Step 3: Refresh visuals (which loads from EncounterManager)
    RefreshVisuals();
}
```

### 2. Refresh Visuals (Loads from EncounterManager)

`RefreshVisuals()` triggers a chain that loads data:

```csharp
private void RefreshVisuals()
{
    UpdateButtonText();
    UpdateButtonState();      // ← Calls EvaluateAvailability() which loads from EncounterManager
    UpdateAreaLevelText();
    UpdateEncounterSprite();
    UpdateWavePreview();      // ← Also loads from EncounterManager
}
```

### 3. Evaluate Availability (Primary Data Loading Point)

This is where `EncounterButton` **actively loads data from EncounterManager**:

```374:437:Assets/Scripts/EncounterSystem/EncounterButton.cs
    private bool EvaluateAvailability(out string reason)
    {
        StringBuilder sb = new StringBuilder();
        bool allGood = true;

        EncounterManager encounterManager = EncounterManager.Instance;
        EncounterData data = encounterManager != null ? encounterManager.GetEncounterByID(encounterID) : null;
        // Only sync data from EncounterManager if autoSyncEncounterData is enabled
        if (data != null && autoSyncEncounterData)
        {
            ApplyEncounterData(data);
        }

        if (requireUnlockedEncounter)
        {
            if (data == null)
            {
                sb.AppendLine("Encounter data missing.");
                allGood = false;
            }
            else
            {
                // Encounter 1 is always unlocked - bypass check
                bool unlocked;
                if (encounterID == 1)
                {
                    unlocked = true;
                }
                else
                {
                    unlocked = encounterManager == null || encounterManager.IsEncounterUnlocked(encounterID);
                }
                
                if (!unlocked)
                {
                    sb.AppendLine("Complete the prerequisite encounters first.");
                    allGood = false;
                }
            }
        }

        if (requireSelectedCharacter)
        {
            var charMgr = CharacterManager.Instance;
            if (charMgr == null || charMgr.currentCharacter == null)
            {
                sb.AppendLine("Select a character first.");
                allGood = false;
            }
        }

        if (requireActiveDeck)
        {
            var deckMgr = DeckManager.Instance;
            if (deckMgr == null || !deckMgr.HasActiveDeck())
            {
                sb.AppendLine("Activate a deck in Deck Builder.");
                allGood = false;
            }
        }

        reason = sb.ToString().Trim();
        return allGood;
    }
```

**Key Points:**
- **Line 380**: `encounterManager.GetEncounterByID(encounterID)` - Fetches `EncounterData` from EncounterManager
- **Line 382-384**: If `autoSyncEncounterData` is true, calls `ApplyEncounterData(data)` to sync the button's local data
- **Line 404**: `encounterManager.IsEncounterUnlocked(encounterID)` - Checks unlock state from EncounterManager

### 4. Apply Encounter Data (Syncs Button State)

When data is loaded, it syncs to the button's local fields:

```488:505:Assets/Scripts/EncounterSystem/EncounterButton.cs
    private void ApplyEncounterData(EncounterData data)
    {
        if (data == null)
            return;

        encounterName = string.IsNullOrWhiteSpace(data.encounterName) ? encounterName : data.encounterName;
        encounterID = data.encounterID;
        sceneName = string.IsNullOrWhiteSpace(data.sceneName) ? sceneName : data.sceneName;
        areaLevel = data.areaLevel;
        if (data.encounterSprite != null)
        {
            encounterSprite = data.encounterSprite;
        }

        UpdateButtonText();
        UpdateAreaLevelText();
        UpdateEncounterSprite();
    }
```

This updates:
- `encounterName` - Button text
- `encounterID` - Encounter identifier
- `sceneName` - Scene to load
- `areaLevel` - Area level display
- `encounterSprite` - Icon/sprite

### 5. Update Wave Preview (Additional Data Loading)

The wave preview also loads from EncounterManager:

```285:319:Assets/Scripts/EncounterSystem/EncounterButton.cs
    private void UpdateWavePreview()
    {
        if (wavePreviewLabel == null)
            return;

        EncounterData data = EncounterManager.Instance != null
            ? EncounterManager.Instance.GetEncounterByID(encounterID)
            : null;

        if (data == null || data.totalWaves <= 0)
        {
            wavePreviewLabel.text = string.Empty;
            wavePreviewLabel.gameObject.SetActive(false);
            return;
        }

        var character = CharacterManager.Instance?.GetCurrentCharacter();
        float moveMultiplier = character != null ? character.GetMovementSpeedMultiplier() : 1f;
        moveMultiplier = Mathf.Max(1f, moveMultiplier); // only compress when faster

        int baseWaves = Mathf.Max(1, data.totalWaves);
        int compressedWaves = Mathf.Max(1, Mathf.CeilToInt(baseWaves / moveMultiplier));
        float compressionPercent = 1f - (float)compressedWaves / baseWaves;
        float movePercent = (moveMultiplier - 1f) * 100f;

        wavePreviewLabel.gameObject.SetActive(true);
        if (compressionPercent > 0.0001f)
        {
            wavePreviewLabel.text = $"Waves {baseWaves} → {compressedWaves} ({compressionPercent * 100f:+0;-0;0}% via MS +{movePercent:0}%)";
        }
        else
        {
            wavePreviewLabel.text = $"Waves {baseWaves} (MS +{movePercent:0}% )";
        }
    }
```

**Line 290-292**: Gets `EncounterData` from EncounterManager to read `totalWaves`

## Event-Driven Updates

### EncounterGraphChanged Event

The button subscribes to graph changes:

```507:510:Assets/Scripts/EncounterSystem/EncounterButton.cs
    private void HandleEncounterGraphChanged()
    {
        RefreshVisuals();
    }
```

When `EncounterManager` raises `EncounterGraphChanged`, the button automatically refreshes, which triggers:
- `EvaluateAvailability()` → Loads fresh data from EncounterManager
- `UpdateWavePreview()` → Loads fresh wave data
- UI updates with new state

## Data Sources Priority

`EncounterButton` can get data from multiple sources, in this order:

1. **EncounterDataAsset** (ScriptableObject) - If assigned in Inspector
   - Loaded via `ResolveEncounterFromAsset()` in `OnEnable()`
   
2. **EncounterManager** (Runtime Data) - Primary source
   - Loaded via `GetEncounterByID()` in `EvaluateAvailability()`
   - Only syncs if `autoSyncEncounterData = true`
   
3. **Manual Assignment** - If set directly in Inspector
   - `encounterID`, `encounterName`, `areaLevel`, etc. can be set manually
   - Used if `autoSyncEncounterData = false`

## When Data is Loaded

Data is loaded from EncounterManager at these times:

1. **OnEnable** → `RefreshVisuals()` → `EvaluateAvailability()` → Loads data
2. **Character Changed** → `HandleCharacterChanged()` → `RefreshVisuals()` → Loads data
3. **Deck Changed** → `HandleDeckChanged()` → `RefreshVisuals()` → Loads data
4. **Graph Changed** → `HandleEncounterGraphChanged()` → `RefreshVisuals()` → Loads data
5. **Manual Refresh** → `RefreshVisualsFromOutside()` → `RefreshVisuals()` → Loads data

## API Methods Used

`EncounterButton` uses these `EncounterManager` methods:

1. **`GetEncounterByID(int encounterID)`**
   - Returns `EncounterData` for the given ID
   - Used in `EvaluateAvailability()` and `UpdateWavePreview()`

2. **`IsEncounterUnlocked(int encounterID)`**
   - Returns `bool` indicating if encounter is unlocked
   - Used in `EvaluateAvailability()` to check availability

3. **`EncounterGraphChanged` event**
   - Event raised when encounter graph changes
   - Subscribed in `OnEnable()`, unsubscribed in `OnDisable()`

## Summary Flow Diagram

```
OnEnable()
  ↓
ResolveEncounterFromAsset() [if autoSyncEncounterData]
  ↓
RefreshVisuals()
  ↓
UpdateButtonState()
  ↓
EvaluateAvailability()
  ↓
EncounterManager.Instance.GetEncounterByID(encounterID) ← LOADS DATA
  ↓
ApplyEncounterData(data) [if autoSyncEncounterData]
  ↓
Update UI Elements (text, sprite, area level, etc.)
```

## Key Configuration

**`autoSyncEncounterData`** (bool, default: true)
- When `true`: Button automatically syncs data from EncounterManager
- When `false`: Button uses manually assigned values (encounterID, encounterName, etc.)

**Recommendation**: Keep `autoSyncEncounterData = true` for most use cases to ensure buttons stay in sync with EncounterManager's state.

