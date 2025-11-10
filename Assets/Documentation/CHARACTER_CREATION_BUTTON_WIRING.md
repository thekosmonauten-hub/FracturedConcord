# Character Creation Button Wiring Guide

Wire up class selection buttons to trigger video transition and load CharacterDisplayUI scene.

---

## Option 1: Inspector-Only (Recommended for Quick Setup)

**Pros:** No code changes, visual setup, easy to modify
**Cons:** Repetitive for 6 buttons

### For Each Class Button:

1. Select button (e.g., **WitchButton**)
2. **Button** component → **On Click ()**
3. **Add Event 1: Save Class Selection**
   - Click **+**
   - Drag any GameObject (or create "Managers" empty GameObject)
   - Dropdown: **ClassSelectionData → SetSelectedClass (string)**
   - Text field: **"Witch"** (class name in quotes)
4. **Add Event 2: Play Video Transition**
   - Click **+**
   - Drag **VideoTransitionManager** GameObject
   - Dropdown: **VideoTransitionManager → PlayTransitionAndLoadScene (string)**
   - Text field: **"CharacterDisplayUI"** (scene name)

**Result:** Click button → Saves class → Plays video → Loads CharacterDisplayUI

### Complete Button Setup:

| Button | Class Name | Scene Name |
|--------|-----------|------------|
| WitchButton | "Witch" | "CharacterDisplayUI" |
| MarauderButton | "Marauder" | "CharacterDisplayUI" |
| RangerButton | "Ranger" | "CharacterDisplayUI" |
| ThiefButton | "Thief" | "CharacterDisplayUI" |
| ApostleButton | "Apostle" | "CharacterDisplayUI" |
| BrawlerButton | "Brawler" | "CharacterDisplayUI" |

---

## Option 2: Code-Based (Recommended for Flexibility)

**Pros:** Centralized logic, easier to maintain, can add custom behavior
**Cons:** Requires code modification

### Modify CharacterCreationController

Add these fields:

```csharp
[Header("Scene Transition")]
public VideoTransitionManager videoTransitionManager;
public string characterDisplaySceneName = "CharacterDisplayUI";

[Header("Video Clips (Optional)")]
public VideoClip defaultTransitionVideo;
public VideoClip witchTransitionVideo;
public VideoClip marauderTransitionVideo;
public VideoClip rangerTransitionVideo;
public VideoClip thiefTransitionVideo;
public VideoClip apostleTransitionVideo;
public VideoClip brawlerTransitionVideo;
```

Update the `OnClassSelected` method:

```csharp
private void OnClassSelected(string className)
{
    selectedClass = className;
    
    // Save selection to persistent data
    ClassSelectionData.Instance.SetSelectedClass(className);
    
    Debug.Log($"[CharacterCreation] Class selected: {className}");
    
    // Get class-specific video (optional)
    VideoClip classVideo = GetVideoForClass(className);
    
    // Play video transition and load CharacterDisplayUI scene
    if (videoTransitionManager != null)
    {
        videoTransitionManager.PlayTransitionAndLoadScene(
            characterDisplaySceneName, 
            classVideo // null uses default video
        );
    }
    else
    {
        // No video manager, load scene directly
        Debug.LogWarning("[CharacterCreation] No VideoTransitionManager assigned! Loading scene directly.");
        UnityEngine.SceneManagement.SceneManager.LoadScene(characterDisplaySceneName);
    }
}

private VideoClip GetVideoForClass(string className)
{
    switch (className.ToLower())
    {
        case "witch": return witchTransitionVideo;
        case "marauder": return marauderTransitionVideo;
        case "ranger": return rangerTransitionVideo;
        case "thief": return thiefTransitionVideo;
        case "apostle": return apostleTransitionVideo;
        case "brawler": return brawlerTransitionVideo;
        default: return defaultTransitionVideo;
    }
}
```

### In Unity Inspector:

1. Select **CharacterCreationController** GameObject
2. Find **Video Transition Manager** field
3. Drag your **VideoTransitionManager** GameObject here
4. Set **Character Display Scene Name**: "CharacterDisplayUI"
5. (Optional) Assign class-specific videos

**That's it!** Your existing button setup will now trigger the video transition.

---

## Option 3: Hybrid Approach (Best of Both Worlds)

Use code for logic, but add a helper method callable from Inspector:

```csharp
// Add this method to CharacterCreationController
public void OnClassButtonClicked_Witch() { OnClassSelected("Witch"); }
public void OnClassButtonClicked_Marauder() { OnClassSelected("Marauder"); }
public void OnClassButtonClicked_Ranger() { OnClassSelected("Ranger"); }
public void OnClassButtonClicked_Thief() { OnClassSelected("Thief"); }
public void OnClassButtonClicked_Apostle() { OnClassSelected("Apostle"); }
public void OnClassButtonClicked_Brawler() { OnClassSelected("Brawler"); }
```

Then in Unity:
1. Select button (e.g., WitchButton)
2. **On Click ()** → **+**
3. Drag **CharacterCreationController** GameObject
4. Select: **CharacterCreationController → OnClassButtonClicked_Witch**

Repeat for all 6 buttons with their respective methods.

---

## Troubleshooting

### Video doesn't play
**Fix:**
- Verify VideoTransitionManager exists in scene
- Check video clip is assigned in VideoTransitionManager
- Ensure video file is imported correctly (Assets/Videos folder)
- Check Console for error messages

### Scene doesn't load
**Fix:**
- Verify "CharacterDisplayUI" scene exists
- Check scene is added to Build Settings (**File → Build Settings**)
- Ensure scene name matches exactly (case-sensitive)

### Class selection not saved
**Fix:**
- Verify ClassSelectionData.Instance.SetSelectedClass() is called
- Check Console for debug logs
- Add debug: `Debug.Log(ClassSelectionData.Instance.SelectedClass);`

### Buttons don't respond
**Fix:**
- Check Button component is enabled
- Verify EventSystem exists in scene
- Check Canvas has GraphicRaycaster component
- Ensure button isn't behind another UI element

---

## Testing

1. **Enter Play Mode**
2. **Click a class button** (e.g., Witch)
3. **Expected behavior:**
   - Console log: "Class selected: Witch"
   - Video plays full-screen
   - After video: CharacterDisplayUI scene loads
   - CharacterDisplayUI displays: "Path of the Witch"

---

## Summary

**Inspector-Only:**
- 2 events per button
- Event 1: ClassSelectionData → SetSelectedClass ("ClassName")
- Event 2: VideoTransitionManager → PlayTransitionAndLoadScene ("CharacterDisplayUI")

**Code-Based:**
- Modify `OnClassSelected()` method
- Assign VideoTransitionManager in Inspector
- Buttons work automatically via existing code

**Choose based on preference!** Inspector-only is fastest for prototyping, code-based is better for production.


