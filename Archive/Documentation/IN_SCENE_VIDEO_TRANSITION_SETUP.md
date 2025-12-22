# In-Scene Video Transition Setup Guide

Play a video transition when selecting a class, then show character customization containers.

---

## Quick Setup (3 Steps)

### Step 1: Add the Component

1. Open your **CharacterCreation** scene
2. Create an empty GameObject: **Right-click Hierarchy → Create Empty**
3. Rename it to **"VideoTransitionManager"**
4. **Add Component** → Search "InSceneVideoTransition"
5. In Inspector:
   - **Transition Video**: Drag your video file here
   - **Min Transition Duration**: **1** (seconds)
   - **Play Audio**: ✅ Check if video has sound

---

### Step 2: Configure UI Visibility

In the **InSceneVideoTransition** component:

#### UI to Hide During Transition:
Add GameObjects you want to hide while video plays:
- Class selection buttons/panel
- Any UI that should be hidden during transition

#### UI to Show After Transition:
Add GameObjects you want to show when video finishes:
- Character name input field
- Character customization panel
- Create character button
- Any containers for next step

**Example:**
```
UI to Hide During Transition:
  [0] ClassSelectionPanel
  [1] ClassGrid

UI to Show After Transition:
  [0] CharacterCustomizationPanel
  [1] CharacterNameInput
  [2] CreateCharacterButton
```

---

### Step 3: Integrate with Class Selection

You have **two options** for triggering the video:

#### **Option A: Call from CharacterCreationController (Code)**

Open `CharacterCreationController.cs` and modify the `OnClassSelected` method:

```csharp
[Header("Video Transition")]
public InSceneVideoTransition videoTransition;

private void OnClassSelected(string className)
{
    selectedClass = className;
    
    // Play video transition
    if (videoTransition != null)
    {
        videoTransition.PlayTransition(() => 
        {
            // This runs after video finishes
            UpdateClassSelection();
            UpdateCreateButtonState();
            UpdateCharacterPreview();
            Debug.Log($"Class selected: {className}");
        });
    }
    else
    {
        // No video, run immediately
        UpdateClassSelection();
        UpdateCreateButtonState();
        UpdateCharacterPreview();
    }
}
```

Then in Unity:
1. Select your **CharacterCreationController** GameObject
2. In Inspector, find **Video Transition** field
3. Drag the **VideoTransitionManager** GameObject here

#### **Option B: Call Directly from Button Events (No Code)**

For each class button (Witch, Marauder, etc.):

1. Select the button in Hierarchy
2. In Inspector, find **Button** component (or your button event handler)
3. Add a new event:
   - Click **+** to add event
   - Drag **VideoTransitionManager** to the object field
   - Select **InSceneVideoTransition → PlayTransition**
4. Add another event for class selection logic (your existing method)

**This approach:** Video plays first, then your existing class selection logic runs.

---

## Advanced Usage

### Different Videos for Each Class

```csharp
[Header("Class-Specific Videos")]
public VideoClip witchTransitionVideo;
public VideoClip marauderTransitionVideo;
public VideoClip rangerTransitionVideo;
// etc...

private void OnClassSelected(string className)
{
    selectedClass = className;
    
    // Get video for selected class
    VideoClip classVideo = GetVideoForClass(className);
    
    // Play class-specific video
    if (videoTransition != null && classVideo != null)
    {
        videoTransition.PlayTransition(() => 
        {
            UpdateClassSelection();
            UpdateCreateButtonState();
            UpdateCharacterPreview();
        }, classVideo);
    }
}

private VideoClip GetVideoForClass(string className)
{
    switch (className)
    {
        case "Witch": return witchTransitionVideo;
        case "Marauder": return marauderTransitionVideo;
        case "Ranger": return rangerTransitionVideo;
        case "Thief": return thiefTransitionVideo;
        case "Apostle": return apostleTransitionVideo;
        case "Brawler": return brawlerTransitionVideo;
        default: return null;
    }
}
```

### Show Specific Containers Based on Class

```csharp
[Header("UI Containers")]
public GameObject generalCustomizationPanel;
public GameObject spellcasterPanel; // For Witch, Apostle
public GameObject meleePanel; // For Marauder, Brawler

private void OnClassSelected(string className)
{
    selectedClass = className;
    
    // Determine which panels to show
    GameObject[] panelsToShow = GetPanelsForClass(className);
    
    // Play video and show panels
    if (videoTransition != null)
    {
        videoTransition.PlayTransitionAndShowUI(panelsToShow);
    }
}

private GameObject[] GetPanelsForClass(string className)
{
    List<GameObject> panels = new List<GameObject>();
    panels.Add(generalCustomizationPanel);
    
    if (className == "Witch" || className == "Apostle")
    {
        panels.Add(spellcasterPanel);
    }
    else if (className == "Marauder" || className == "Brawler")
    {
        panels.Add(meleePanel);
    }
    
    return panels.ToArray();
}
```

---

## Testing

1. **Test Video Playback:**
   - Select **VideoTransitionManager** in Hierarchy
   - Right-click **InSceneVideoTransition** component
   - Click **"Test Play Video"**
   - Video should play full-screen and disappear when done

2. **Test Class Selection:**
   - Enter Play Mode
   - Click a class button
   - Video should play
   - After video: customization UI should appear

---

## Troubleshooting

### Video doesn't play
**Fix:** 
- Check video is assigned in Inspector
- Check Console for errors
- Try right-click component → "Test Play Video"

### UI doesn't hide/show correctly
**Fix:**
- Verify GameObjects are assigned in Inspector arrays
- Check GameObjects are not disabled in Hierarchy manually
- Enable debug logs to see what's being shown/hidden

### Video plays but UI update doesn't happen
**Fix:**
- Ensure callback method is being called
- Check `onTransitionComplete?.Invoke()` is in the code
- Add `Debug.Log()` statements to track execution

### Video is too short/long
**Fix:**
- Adjust **Min Transition Duration** in Inspector
- Import a different video with desired length
- Video will play at actual length OR min duration (whichever is longer)

---

## Summary

**Setup:**
1. ✅ Add `InSceneVideoTransition` component
2. ✅ Assign video and configure UI visibility
3. ✅ Integrate with class selection (code or button events)

**Result:**
- Click class button → Video plays → Customization UI appears

**That's it!** The video transition creates a cinematic feel between class selection and character customization. 🎬


