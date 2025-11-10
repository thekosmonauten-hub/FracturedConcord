# Character Display Scene Setup Guide

This guide shows how to set up the flow: **CharacterCreation** → Video Transition → **CharacterDisplayUI**

---

## Architecture Overview

### Persistent Managers (DontDestroyOnLoad):
- ✅ **StarterDeckManager** - Provides starter decks
- ✅ **CharacterManager** - Creates characters
- ✅ **ClassSelectionData** (NEW) - Passes selected class between scenes
- ✅ **VideoTransitionManager** - Plays transition videos

### Scene-Specific Controllers:
- **CharacterCreationController** - CharacterCreation scene only
- **CharacterDisplayController** (NEW) - CharacterDisplayUI scene only

---

## Step 1: Update CharacterCreationController

Add this to your `CharacterCreationController.cs`:

```csharp
[Header("Scene Transition")]
public VideoTransitionManager videoTransitionManager;
public string characterDisplaySceneName = "CharacterDisplayUI";

private void OnClassSelected(string className)
{
    selectedClass = className;
    
    // Save selection to persistent data
    ClassSelectionData.Instance.SetSelectedClass(className);
    
    // Play video and transition to CharacterDisplayUI scene
    if (videoTransitionManager != null)
    {
        videoTransitionManager.PlayTransitionAndLoadScene(
            characterDisplaySceneName, 
            null // or specify class-specific video
        );
    }
    else
    {
        // No video, load scene directly
        Debug.LogWarning("[CharacterCreationController] No VideoTransitionManager assigned!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(characterDisplaySceneName);
    }
}
```

**In Unity:**
1. Select your CharacterCreationController GameObject
2. Find **Video Transition Manager** field
3. Drag your **VideoTransitionManager** GameObject here

---

## Step 2: Setup CharacterDisplayUI Scene

### A. Scene Hierarchy

Create this structure in **CharacterDisplayUI** scene:

```
CharacterDisplayUI Scene
├── Canvas
│   ├── Header
│   │   └── PathOfTheArchetype (TextMeshPro) - "Path of the X"
│   ├── ClassInfoPanel
│   │   ├── ClassNameText (TextMeshPro)
│   │   ├── ClassDescriptionText (TextMeshPro)
│   │   └── ClassIconImage (Image)
│   ├── AttributePanel
│   │   ├── StrengthText (TextMeshPro)
│   │   ├── DexterityText (TextMeshPro)
│   │   ├── IntelligenceText (TextMeshPro)
│   │   ├── HealthText (TextMeshPro) [Optional]
│   │   ├── ManaText (TextMeshPro) [Optional]
│   │   └── RelianceText (TextMeshPro) [Optional]
│   ├── CardPreviewPanel
│   │   └── CardGridContainer (Empty GameObject with RectTransform)
│   ├── CharacterInputPanel
│   │   ├── NameInputField (TMP_InputField)
│   │   ├── ConfirmButton (Button)
│   │   └── BackButton (Button)
└── CharacterDisplayController (Empty GameObject with script)
```

### B. Add CharacterDisplayController Script

1. Create empty GameObject: **"CharacterDisplayController"**
2. **Add Component** → `CharacterDisplayController`
3. Assign references in Inspector:
   
   **UI References:**
   - **Path Of The Archetype Text**: PathOfTheArchetype (from Header)
   - **Class Name Text**: ClassNameText
   - **Class Description Text**: ClassDescriptionText
   - **Class Icon Image**: ClassIconImage
   
   **Attribute Display:**
   - **Strength Text**: StrengthText
   - **Dexterity Text**: DexterityText
   - **Intelligence Text**: IntelligenceText
   
   **Resource Display (Optional):**
   - **Health Text**: HealthText
   - **Mana Text**: ManaText
   - **Reliance Text**: RelianceText
   
   **Card Preview:**
   - **Card Prefab**: Your card prefab (DeckCardPrefab or CardPrefab)
   - **Card Grid Container**: CardGridContainer
   
   **Character Input:**
   - **Character Name Input**: NameInputField
   - **Confirm Button**: ConfirmButton
   - **Back Button**: BackButton

### C. Optional: CardGridContainer Layout

For automatic card positioning:

1. Select **CardGridContainer**
2. **Add Component** → **Grid Layout Group**
3. Configure:
   - **Cell Size**: 120 x 160 (adjust to your card size)
   - **Spacing**: 10, 10
   - **Start Corner**: Upper Left
   - **Start Axis**: Horizontal
   - **Child Alignment**: Upper Left
   - **Constraint**: Fixed Column Count
   - **Constraint Count**: 6 (cards per row)

---

## Step 3: Manager Setup

### Ensure Managers Exist

These should already be in your project as singletons:

**In CharacterCreation Scene:**
- ✅ StarterDeckManager (persists)
- ✅ VideoTransitionManager (persists)

**Auto-Created (DontDestroyOnLoad):**
- ✅ ClassSelectionData (created automatically when accessed)
- ✅ CharacterManager (created automatically when accessed)

**You don't need to add them to CharacterDisplayUI scene** - they persist automatically!

---

## Complete Flow

### User Journey:

1. **CharacterCreation Scene**
   - User clicks class button (e.g., "Witch")
   - `OnClassSelected("Witch")` called
   - Saves to `ClassSelectionData.Instance`
   - `VideoTransitionManager` plays video
   - Loads **CharacterDisplayUI** scene

2. **CharacterDisplayUI Scene**
   - `CharacterDisplayController.Start()` runs
   - Gets `ClassSelectionData.Instance.SelectedClass` → "Witch"
   - Gets `StarterDeckManager.Instance` → loads Witch starter deck
   - Displays class info (name, description, icon)
   - Displays starting attributes (STR: 14, DEX: 14, INT: 32)
   - Displays derived resources (Health: 156, Mana: 3, Reliance: 200)
   - Spawns starter deck cards in grid
   - User enters character name
   - User clicks Confirm
   - Creates character via `CharacterManager`
   - Loads next scene (MainMenu or GameMap)

---

## Alternative: Direct Button Integration (No Code Changes)

If you want class buttons to directly trigger scene transition:

### For Each Class Button (in CharacterCreation):

1. Select button (e.g., WitchButton)
2. In Inspector → **Button** component → **On Click ()**
3. Click **+** to add event
4. **Event 1: Save Class**
   - Drag **ClassSelectionData** GameObject
   - Select **ClassSelectionData → SetSelectedClass**
   - In text field, enter: **"Witch"**
5. **Event 2: Play Video Transition**
   - Drag **VideoTransitionManager** GameObject
   - Select **VideoTransitionManager → PlayTransitionAndLoadScene**
   - In text field, enter: **"CharacterDisplayUI"**

Repeat for all 6 class buttons (Witch, Marauder, Ranger, Thief, Apostle, Brawler).

---

## Managers Location - Where to Keep Them

### ✅ CORRECT Approach:

**StarterDeckManager:**
- Place in **FIRST** scene (CharacterCreation)
- It will persist to all scenes automatically via DontDestroyOnLoad
- **DO NOT** add to CharacterDisplayUI scene

**CharacterCreationController:**
- Only in **CharacterCreation** scene
- Will be destroyed when loading CharacterDisplayUI

**CharacterDisplayController:**
- Only in **CharacterDisplayUI** scene
- New instance created when scene loads

### ❌ WRONG Approach:

- ❌ Adding StarterDeckManager to CharacterDisplayUI scene (causes duplicates)
- ❌ Adding CharacterCreationController to CharacterDisplayUI scene (wrong scene)
- ❌ Expecting CharacterCreationController to persist (it won't)

---

## Troubleshooting

### "No class selected" error in CharacterDisplayUI
**Fix:**
- Ensure `ClassSelectionData.Instance.SetSelectedClass()` is called before scene transition
- Add debug log to verify: `Debug.Log(ClassSelectionData.Instance.SelectedClass);`

### Cards not displaying
**Fix:**
- Check **Card Prefab** is assigned in CharacterDisplayController
- Check **CardGridContainer** is assigned
- Verify StarterDeckManager has deck definitions assigned

### Multiple StarterDeckManagers exist
**Fix:**
- Remove StarterDeckManager from CharacterDisplayUI scene
- It should only exist in CharacterCreation scene (will persist automatically)

### Video doesn't play
**Fix:**
- Ensure VideoTransitionManager is assigned in CharacterCreationController
- Check video is assigned in VideoTransitionManager Inspector
- Verify scene name "CharacterDisplayUI" matches exactly

---

## Summary

**Data Flow:**
```
CharacterCreation Scene:
  OnClassSelected("Witch")
    → ClassSelectionData.SetSelectedClass("Witch")  [Persists]
    → VideoTransitionManager.PlayTransition()       [Persists]
    → Load CharacterDisplayUI Scene

CharacterDisplayUI Scene:
  CharacterDisplayController.Start()
    → Get ClassSelectionData.SelectedClass         [Still "Witch"]
    → Get StarterDeckManager.Instance              [Still exists]
    → Display class info and cards
    → User confirms
    → CharacterManager.CreateCharacter()           [Persists]
    → Load next scene
```

**That's it!** The persistent managers handle all the data, while scene-specific controllers only exist in their own scenes. 🎮

---

## Appendix: Starting Attributes by Class

### Primary Classes (Single Attribute Focus)

**Marauder** (Strength)
- Strength: **32**
- Dexterity: 14
- Intelligence: 14
- Health: **228** (100 + 32 × 4)
- Mana: 3
- Reliance: 200

**Ranger** (Dexterity)
- Strength: 14
- Dexterity: **32**
- Intelligence: 14
- Health: **156** (100 + 14 × 4)
- Mana: 3
- Reliance: 200

**Witch** (Intelligence)
- Strength: 14
- Dexterity: 14
- Intelligence: **32**
- Health: **156** (100 + 14 × 4)
- Mana: 3
- Reliance: 200

### Hybrid Classes (Dual Attribute Focus)

**Brawler** (Strength + Dexterity)
- Strength: **23**
- Dexterity: **23**
- Intelligence: 14
- Health: **192** (100 + 23 × 4)
- Mana: 3
- Reliance: 200

**Thief** (Dexterity + Intelligence)
- Strength: 14
- Dexterity: **23**
- Intelligence: **23**
- Health: **156** (100 + 14 × 4)
- Mana: 3
- Reliance: 200

**Apostle** (Strength + Intelligence)
- Strength: **23**
- Dexterity: 14
- Intelligence: **23**
- Health: **192** (100 + 23 × 4)
- Mana: 3
- Reliance: 200

**Note:** Health is calculated as `100 + (Strength × 4)`. All classes start with 3 Mana and 200 Reliance.

