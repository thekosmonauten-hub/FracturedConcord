# Reliance Aura Experience & Leveling System

## Overview

Reliance Auras gain experience and level up from 1 to 20, similar to the card leveling system. Active auras gain experience when the character gains experience in combat.

**Created:** Based on Card Experience System pattern  
**Purpose:** Allow auras to scale their effects from Level 1 to Level 20

---

## System Architecture

```
Player gains XP in combat
    ↓
CharacterManager.AddExperience(xp, shareWithCards: true)
    ↓
    ├─→ Character levels up (existing system)
    ├─→ CardExperienceManager.ApplyCombatExperience(xp)
    └─→ AuraExperienceManager.ApplyCombatExperience(xp)
            ↓
            Active auras gain XP (by aura name)
            ↓
            Aura levels up (1-20)
            ↓
            Modifier effects scale based on level
```

---

## Core Components

### 1. AuraExperienceManager

**Location:** `Assets/Scripts/CombatSystem/RelianceAura/AuraExperienceManager.cs`

**Responsibilities:**
- Tracks experience per aura name (all instances share experience)
- Handles level ups (1-20)
- Saves/loads with character data
- Applies combat experience to active auras

**Key Methods:**
```csharp
void AddAuraExperience(string auraName, int amount)
int GetAuraLevel(string auraName)
int GetAuraExperience(string auraName)
AuraExperienceData GetAuraExperienceData(string auraName)
void ApplyCombatExperience(int baseExperience) // Gives XP to all active auras
void SaveToCharacter(Character character)
void LoadFromCharacter(Character character)
```

### 2. AuraExperienceData

**Location:** `Assets/Scripts/CombatSystem/RelianceAura/AuraExperienceManager.cs`

**Structure:**
```csharp
[System.Serializable]
public class AuraExperienceData
{
    public string auraName;
    public int level = 1;        // 1-20
    public int experience = 0;
    
    int GetRequiredExperienceForNextLevel()
    bool CanLevelUp()
    float GetLevelProgress() // 0.0 to 1.0
}
```

**Experience Formula:**
- Base XP: 100
- Scaling: 1.15x per level (exponential, same as cards)
- XP for next level = `100 * (1.15^(level-1))`

**Example Progression:**
| Level | Required XP | Cumulative XP |
|-------|-------------|---------------|
| 1 | 0 | 0 |
| 2 | 100 | 100 |
| 5 | 175 | 644 |
| 10 | 314 | 2,030 |
| 15 | 563 | 5,819 |
| 20 | 1,011 | 16,398 |

---

## Integration Points

### Character Experience Gain

**Location:** `Assets/Scripts/Data/CharacterManager.cs`

```csharp
public void AddExperience(int exp, bool shareWithCards = true)
{
    // ... character gains XP ...
    
    // Also give experience to cards in active deck
    if (shareWithCards && CardExperienceManager.Instance != null)
    {
        CardExperienceManager.Instance.ApplyCombatExperience(exp);
    }
    
    // Also give experience to active auras
    if (shareWithCards && AuraExperienceManager.Instance != null)
    {
        AuraExperienceManager.Instance.ApplyCombatExperience(exp);
    }
}
```

### Save/Load System

**Character.cs:**
```csharp
[Header("Reliance Auras")]
public List<string> ownedRelianceAuras = new List<string>();
public List<string> activeRelianceAuras = new List<string>();
public List<AuraExperienceData> auraExperienceData = new List<AuraExperienceData>(); // NEW
```

**CharacterManager.cs:**
```csharp
// On Save
if (AuraExperienceManager.Instance != null)
{
    AuraExperienceManager.Instance.SaveToCharacter(currentCharacter);
}

// On Load
if (AuraExperienceManager.Instance != null)
{
    AuraExperienceManager.Instance.LoadFromCharacter(currentCharacter);
}
```

**CharacterData (Serialization):**
```csharp
public List<AuraExperienceData> auraExperienceData = new List<AuraExperienceData>();
```

---

## Effect Scaling

### Implementation

Aura modifier effects automatically scale based on aura level (1-20). The scaling system:

1. **Stores Level 1 and Level 20 values** in modifier parameter dictionaries
2. **Interpolates between values** when retrieving active effects
3. **Applies scaled values** during combat

### Parameter Naming Convention

To enable scaling, modifier definitions should store parameters with this convention:

- **Level 1 value:** `parameterName` (e.g., `fireDamage`, `lightningDamage`)
- **Level 20 value:** `parameterName_level20` (e.g., `fireDamage_level20`, `lightningDamage_level20`)

**Example:**
```
fireDamage = 90 (Level 1 value)
fireDamage_level20 = 160 (Level 20 value)
```

At level 10, the scaled value would be: `Mathf.Lerp(90, 160, 0.474) ≈ 123`

### Scaling Logic

**Location:** `RelianceAuraModifierRegistry.ScaleActionParameters()`

```csharp
// Interpolation factor (0.0 at level 1, 1.0 at level 20)
float t = (auraLevel - 1) / 19f;

// For numeric parameters
if (parameters.ContainsKey("fireDamage_level20"))
{
    float level1Value = parameters.GetParameter<float>("fireDamage");
    float level20Value = parameters.GetParameter<float>("fireDamage_level20");
    float scaledValue = Mathf.Lerp(level1Value, level20Value, t);
}
```

### Backwards Compatibility

If a parameter doesn't have a `_level20` version:
- The base value is used as-is (assumed to be level 1)
- No scaling is applied
- This maintains compatibility with existing modifiers

### Usage

**Getting Scaled Effects:**
```csharp
// Automatically scales based on aura levels
List<ModifierEffect> scaledEffects = 
    RelianceAuraModifierRegistry.Instance.GetActiveEffects(character);
```

**Processing Events:**
```csharp
// RelianceAuraModifierEventProcessor automatically uses scaled effects
RelianceAuraModifierEventProcessor.Instance.ProcessEvent(
    ModifierEventType.OnCombatStart,
    eventContext
);
```

---

## Usage Examples

### Get Aura Level
```csharp
int level = AuraExperienceManager.Instance.GetAuraLevel("Pyreheart Mantle");
// Returns: 1-20
```

### Check Level Progress
```csharp
AuraExperienceData data = AuraExperienceManager.Instance.GetAuraExperienceData("Pyreheart Mantle");
float progress = data.GetLevelProgress(); // 0.0 to 1.0
int currentXP = data.experience;
int requiredXP = data.GetRequiredExperienceForNextLevel();
```

### Manually Add Experience
```csharp
AuraExperienceManager.Instance.AddAuraExperience("Pyreheart Mantle", 500);
// Automatically handles level ups if enough XP
```

### Listen to Level Up Events
```csharp
AuraExperienceManager.Instance.OnAuraLevelUp += (auraName, newLevel) =>
{
    Debug.Log($"{auraName} reached level {newLevel}!");
};
```

---

## Future Enhancements

1. **UI Display:**
   - Show aura level in Aura UI
   - Display XP progress bar
   - Show level-up notifications

2. **Effect Scaling:**
   - Implement parameter interpolation in modifier system
   - Support different scaling curves (linear, exponential, etc.)
   - Visual feedback for scaled values in tooltips

3. **Experience Sources:**
   - Currently: Combat experience (shared with cards)
   - Future: Aura-specific quests, achievements, etc.

4. **Level Caps:**
   - Currently: Fixed at level 20
   - Future: Configurable per aura, or unlockable higher levels

---

## Notes

- **Only Active Auras Gain XP:** Auras must be in `character.activeRelianceAuras` to gain experience
- **Shared Experience:** All instances of the same aura name share the same experience/level
- **Persistence:** Aura experience is saved with character data and persists across sessions
- **Default Level:** New auras start at level 1 with 0 experience

