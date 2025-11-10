# Video Transition - Quick Setup Instructions

## 🎬 3-Minute Setup

### Step 1: Add VideoTransitionManager to Scene

1. In your **MainMenu** scene, create an **Empty GameObject**
2. Rename to **VideoTransitionManager**
3. Add the `VideoTransitionManager` script component

---

### Step 2: Assign Your Video

Select **VideoTransitionManager** GameObject and assign:

**Video Settings:**
- **Transition Video**: Drag your transition video from Project window
- **Wait For Video To Finish**: ✅ Checked
- **Min Transition Duration**: `2` (or video length, whichever you prefer)
- **Play Audio**: ✅ Checked (if video has audio) or ❌ Unchecked

**Auto-Setup Fields:**
- Leave all empty - they auto-create on startup!

---

### Step 3: Test It!

#### Test Video Playback (without loading scene):
1. Select **VideoTransitionManager** in Hierarchy
2. Right-click the component header → **Test Play Video**
3. Video should play full-screen and then hide

#### Test Scene Transition:
1. Enter Play Mode
2. Click **Start Journey** button
3. You should see:
   - Video plays full-screen
   - After video finishes, scene loads
   - Console shows:
     ```
     [MainMenu] Loading scene GameScene with VIDEO transition
     ━━━ [VideoTransition] Playing: YourVideo.mp4 → GameScene ━━━
     [VideoTransition] Video prepared, starting playback...
     [VideoTransition] Video duration: 3.50s
     [VideoTransition] Video finished after 3.52s
     [VideoTransition] Loading scene: GameScene
     [VideoTransition] Transition hidden, scene loaded!
     ```

---

## How It Works

The `MainMenuController` now automatically detects and uses transitions in this priority:

1. **VideoTransitionManager** (if present) ← Uses video
2. **TransitionManager** (if present) ← Uses fade/frame transition
3. **Direct load** (if neither) ← Instant scene change

**No extra code needed!** Just add VideoTransitionManager to scene and assign your video.

---

## Customization

### Make Video Shorter/Longer

Adjust in Inspector:
- **Min Transition Duration**: Controls minimum time before loading scene
- **Wait For Video To Finish**: 
  - ✅ Checked = waits for full video
  - ❌ Unchecked = loads after min duration

### Add Audio to Video

- **Play Audio**: ✅ Check this
- Ensure your video file has an audio track

### Different Videos for Different Scenes

Add this to `VideoTransitionManager.cs`:

```csharp
[System.Serializable]
public class SceneTransitionVideo
{
    public string sceneName;
    public VideoClip video;
}

[Header("Multi-Video Support")]
[SerializeField] private List<SceneTransitionVideo> sceneSpecificVideos;
```

Then in `MainMenuController.LoadScene()`, pass the scene name and it'll pick the right video.

---

## Troubleshooting

### Video doesn't play?
- **Check**: Transition Video is assigned in Inspector
- **Check**: Console shows "Playing: YourVideo.mp4" message
- **Fix**: Verify video codec (H.264 MP4 recommended)

### Scene loads instantly (video skipped)?
- **Check**: VideoTransitionManager exists in scene
- **Check**: Console shows "with VIDEO transition" message
- **Fix**: Ensure VideoTransitionManager script is enabled

### Video plays but scene doesn't load?
- **Check**: Scene name is correct (case-sensitive!)
- **Check**: Scene is in Build Settings (File → Build Settings)
- **Fix**: Add scene to build settings or fix scene name spelling

### Video is upside down?
Add this to `VideoTransitionManager.SetupTransition()` after creating videoDisplay:
```csharp
videoDisplay.uvRect = new Rect(0, 1, 1, -1); // Flip vertically
```

---

## Video File Recommendations

**Format**: MP4 (H.264)
**Resolution**: 1920x1080 or match your screen resolution
**Length**: 2-5 seconds (keep it snappy!)
**Framerate**: 30 or 60 fps
**Bitrate**: Medium (don't need ultra high quality)
**Audio**: Optional (toggle with Play Audio checkbox)

---

## Complete! 🎉

Your video transition is now ready. Every time you click "Start Journey" (or any button that loads a scene), it will:
1. ✅ Play your transition video full-screen
2. ✅ Wait for video to finish
3. ✅ Load the target scene
4. ✅ Hide the video

No additional setup needed - works automatically! 🎬


