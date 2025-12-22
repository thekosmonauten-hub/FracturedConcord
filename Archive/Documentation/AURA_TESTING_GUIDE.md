# Reliance Aura Testing Guide

## Overview

Two testing tools are available for testing Reliance Auras in-game:

1. **Editor Window** (`RelianceAuraTestingTools`) - Full-featured editor tool
2. **Runtime Component** (`AuraTestingHelper`) - In-game testing helper

---

## Method 1: Editor Window (Recommended)

### Accessing the Tool

1. In Unity Editor, go to: **Tools > Dexiled > Reliance Aura Testing Tools**
2. A window will open showing all auras and their status

### Features

#### Quick Actions Panel
- **Unlock All Auras** - Unlocks all auras for the current character
- **Activate All Auras** - Activates all owned auras
- **Set All to Level 20** - Gives enough XP to level all auras to 20
- **Set Custom Level** - Set all auras to a specific level (1-20)

#### Individual Aura Controls
- View all auras with their status (OWNED, ACTIVE, LOCKED)
- See current level and experience for each aura
- **Activate/Deactivate** buttons for each aura
- **+1000 XP** button to quickly give experience

#### Batch Operations
- **Apply Changes** - Applies all selected options at once
- **Unlock All Auras Only** - Just unlocks, doesn't activate
- **Activate All Owned Auras** - Activates all unlocked auras
- **Deactivate All Auras** - Deactivates all active auras
- **Give 10,000 XP to All Active Auras** - Quick leveling

### Usage Example

1. Load your character in-game
2. Open the testing tools window
3. Check "Unlock All Auras" and "Set All to Level 20"
4. Click "Apply Changes"
5. All auras are now unlocked and at level 20!

---

## Method 2: Runtime Component

### Setup

1. Create an empty GameObject in your scene (or use an existing one)
2. Add the `AuraTestingHelper` component
3. Configure the options in the Inspector

### Inspector Options

#### Quick Actions
- **Unlock All Auras** - Check to unlock all auras (resets to false after use)
- **Activate All Auras** - Check to activate all owned auras
- **Give Experience to Active** - Check to give XP to active auras
- **Experience Amount** - Amount of XP to give

#### Single Aura Testing
- **Test Aura Name** - Name of aura to test (e.g., "Pyreheart Mantle")
- **Give Experience to Test Aura** - Check to give XP
- **Activate Test Aura** - Check to activate
- **Deactivate Test Aura** - Check to deactivate

### Usage

#### Option 1: Inspector Toggles
1. Set the options in the Inspector
2. Check the boolean flags to trigger actions
3. They automatically reset to false after execution

#### Option 2: Context Menu
Right-click the component in Inspector:
- **Unlock All Auras**
- **Activate All Owned Auras**
- **Give Experience to Active Auras**
- **Set Test Aura to Level 20**

#### Option 3: Code
```csharp
AuraTestingHelper helper = FindObjectOfType<AuraTestingHelper>();
helper.UnlockAllAuras();
helper.ActivateAllOwnedAuras();
helper.GiveExperienceToActiveAuras(10000);
helper.SetAuraToLevel("Pyreheart Mantle", 20);
```

---

## Testing Workflow

### Basic Testing

1. **Unlock Auras:**
   - Editor Window: Check "Unlock All Auras" → Click "Apply Changes"
   - Runtime: Check "Unlock All Auras" in Inspector

2. **Activate Auras:**
   - Editor Window: Check "Activate All Auras" → Click "Apply Changes"
   - Runtime: Check "Activate All Owned Auras" in Inspector

3. **Level Up Auras:**
   - Editor Window: Click "+1000 XP" on individual auras, or "Give 10,000 XP to All Active Auras"
   - Runtime: Check "Give Experience to Active" and set amount

### Advanced Testing

1. **Test Specific Aura:**
   - Editor Window: Use individual aura controls
   - Runtime: Set "Test Aura Name" and use test aura options

2. **Test Level Scaling:**
   - Set aura to level 1, test effect
   - Set aura to level 10, test effect
   - Set aura to level 20, test effect
   - Compare values to verify scaling

3. **Test Multiple Auras:**
   - Activate multiple auras
   - Verify they all work together
   - Check reliance cost calculations

---

## Verification

### Check Aura Status

**In Editor Window:**
- View the scrollable list showing all auras
- Status indicators: `[ACTIVE]`, `[OWNED]`, `[LOCKED]`
- Level and XP displayed for each aura

**In Code:**
```csharp
Character character = CharacterManager.Instance.GetCurrentCharacter();
Debug.Log($"Owned: {character.ownedRelianceAuras.Count}");
Debug.Log($"Active: {character.activeRelianceAuras.Count}");

AuraExperienceManager expManager = AuraExperienceManager.Instance;
int level = expManager.GetAuraLevel("Pyreheart Mantle");
Debug.Log($"Pyreheart Mantle is level {level}");
```

### Check Scaled Effects

```csharp
Character character = CharacterManager.Instance.GetCurrentCharacter();
List<ModifierEffect> effects = 
    RelianceAuraModifierRegistry.Instance.GetActiveEffects(character);

foreach (var effect in effects)
{
    foreach (var action in effect.actions)
    {
        float damage = action.parameterDict.GetParameter<float>("damage");
        Debug.Log($"Scaled damage: {damage}");
    }
}
```

---

## Tips

1. **Save After Testing:** Use "Save Character" button in Editor Window to persist changes
2. **Test in Combat:** Activate auras and enter combat to see effects in action
3. **Check Console:** All actions log to console for debugging
4. **Reliance Costs:** Be aware that activating many auras may exceed max reliance
5. **Level Progression:** Use smaller XP amounts to test gradual leveling

---

## Troubleshooting

### "No character loaded"
- Make sure you've loaded a character in-game before using the tools
- The tools require an active character to work

### "RelianceAuraDatabase not found"
- Ensure `RelianceAuraDatabase` exists in the scene
- It should be created automatically, but you may need to add it manually

### "AuraExperienceManager not found"
- The manager is created automatically when first accessed
- If issues persist, manually create a GameObject with `AuraExperienceManager` component

### Auras Not Scaling
- Verify modifier definitions have `_level20` parameters
- Check that auras are actually leveling up (check level in tool)
- Ensure `GetActiveEffects()` is being used (not `GetActiveModifiers()`)

---

## Quick Reference

### Editor Window Shortcuts
- **Unlock All:** Check "Unlock All Auras" → "Apply Changes"
- **Level 20 All:** Check "Set All to Level 20" → "Apply Changes"
- **Quick XP:** Click "+1000 XP" on any aura

### Runtime Component Methods
```csharp
helper.UnlockAllAuras();
helper.ActivateAllOwnedAuras();
helper.GiveExperienceToActiveAuras(10000);
helper.SetAuraToLevel("Aura Name", 20);
```

