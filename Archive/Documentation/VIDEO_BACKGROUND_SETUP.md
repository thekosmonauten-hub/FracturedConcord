# Video Background Setup for Canvas UI

## Overview
Display a looping video as the MainMenu background using VideoPlayer + RawImage.

---

## Method: RenderTexture + RawImage (Recommended for Canvas)

### Step 1: Create RenderTexture (one-time setup)

1. In **Project** window, navigate to `Assets/RenderTextures/` (create folder if needed)
2. Right-click → **Create** → **Render Texture**
3. Rename to **VideoBackgroundRT**
4. Select it and configure in Inspector:
   - **Size**: 1920 x 1080 (match your video resolution)
   - **Depth Buffer**: None (videos don't need depth)
   - **Anti-aliasing**: None
   - **Filter Mode**: Bilinear
   - **Wrap Mode**: Clamp

### Step 2: Setup VideoPlayer GameObject

**Option A: Keep Existing VideoPlayer (if you already have one)**

Looking at your MainMenu scene, you already have a VideoPlayer! Just update its settings:

1. Find **Video Player** GameObject in Hierarchy (child of MainMenuCanvas)
2. Select it and update the **VideoPlayer** component:
   - **Source**: Video Clip
   - **Video Clip**: Assign your video file (drag from Assets)
   - **Play On Awake**: ✅ Checked
   - **Wait For First Frame**: ✅ Checked
   - **Loop**: ✅ Checked
   - **Playback Speed**: 0.2 (or adjust to taste - yours is already set to this)
   - **Render Mode**: **Render Texture**
   - **Target Texture**: **VideoBackgroundRT** (the one you just created)
   - **Audio Output Mode**: None (or Direct if you want audio)

**Option B: Create New VideoPlayer (if starting fresh)**

1. Right-click in Hierarchy → **Create Empty**
2. Rename to **VideoPlayerController**
3. Add **VideoPlayer** component
4. Configure as shown in Option A above

### Step 3: Add Video Background to Canvas

Now integrate the video into your Canvas UI:

```
MainMenuCanvasUI
  └─ VideoBackground      [RawImage - FIRST CHILD, renders behind everything]
  └─ Background           [Image - static fallback/overlay, optional]
  └─ MainMenuPanel
  └─ CharacterSidebar
  └─ etc...
```

#### 3.1: Create VideoBackground
1. Right-click **MainMenuCanvasUI** → **UI** → **Raw Image**
2. Rename to **VideoBackground**
3. **IMPORTANT**: Drag it to be the **FIRST child** (top of list) so it renders behind everything
4. Configure **RectTransform**:
   - **Anchors**: Stretch/Stretch (alt+shift+click bottom-right preset)
   - **Left/Right/Top/Bottom**: all 0 (fills entire Canvas)
5. Configure **Raw Image** component:
   - **Texture**: **VideoBackgroundRT** (the RenderTexture you created)
   - **Color**: White (or tint if desired)
   - **UV Rect**: X:0, Y:0, W:1, H:1
   - **Material**: None (default UI material is fine)

#### 3.2: Optional Static Background Overlay
If you want a static image overlay on top of the video (for darkening or adding frame):

1. Right-click **MainMenuCanvasUI** → **UI** → **Image**
2. Rename to **BackgroundOverlay**
3. Place as **SECOND child** (after VideoBackground)
4. Configure:
   - **Anchors**: Stretch/Stretch
   - **Source Image**: None or your overlay image
   - **Color**: Black with alpha (e.g., rgba(0, 0, 0, 0.3) for 30% darkening)
   - **Material**: None

### Step 4: Hierarchy Order (Critical!)

Your Canvas hierarchy should look like this (order matters!):

```
MainMenuCanvasUI
  1. VideoBackground       ← Renders FIRST (back layer)
  2. BackgroundOverlay     ← Optional darkening/tint
  3. MainMenuPanel         ← Your UI content
  4. CharacterSidebar
  5. SidebarToggleButton
```

**Why order matters**: Unity renders UI elements in order from top to bottom in the Hierarchy. First child renders first (back), last child renders last (front).

---

## Testing Checklist

- [ ] Video plays automatically on scene load
- [ ] Video loops seamlessly
- [ ] Video fills entire screen without stretching/distortion
- [ ] UI elements render on top of video
- [ ] Video performance is acceptable (check FPS)
- [ ] Video doesn't block UI raycasts (RawImage shouldn't need Raycast Target enabled)

---

## Troubleshooting

### Video not appearing?
- **Check RenderTexture is assigned** to both VideoPlayer and RawImage
- **Verify VideoPlayer Target Texture** is set to your RenderTexture (not "Camera Far/Near Plane")
- **Check Render Mode** is "Render Texture" (not "Camera Far Plane")
- **Verify video clip** is assigned and supported format (.mp4, .mov, .webm)

### Video appears but UI is invisible?
- **Check hierarchy order**: VideoBackground should be FIRST child
- **Verify CanvasGroup alpha**: Make sure other UI elements aren't transparent
- **Check Canvas sorting**: Canvas should be in "Screen Space - Overlay" mode

### Video is stretched/distorted?
- **Match RenderTexture size** to your video resolution
- **Check RawImage UV Rect**: Should be (0,0,1,1)
- **Aspect ratio**: Use AspectRatioFitter component on RawImage if needed:
  - Add **Aspect Ratio Fitter** component to VideoBackground
  - **Aspect Mode**: Fit In Parent or Envelope Parent
  - **Aspect Ratio**: Calculate from video (width/height, e.g., 1920/1080 = 1.778)

### Video upside down?
Some video formats render upside down. Fix:
- Set RawImage **UV Rect** to: X:0, Y:1, W:1, H:-1 (negative height flips it)

### Performance issues / Low FPS?
- **Reduce RenderTexture resolution**: Try 1280x720 instead of 1920x1080
- **Reduce video quality**: Use lower bitrate/compression
- **Disable audio**: Set Audio Output Mode to None
- **Reduce playback speed**: Lower than 1.0 (you already have 0.2, which is good)

### Video doesn't loop smoothly?
- **Check Loop setting**: Must be enabled on VideoPlayer
- **Verify video file**: Some codecs have loop issues - try re-encoding to H.264 MP4

---

## Alternative: Video as Camera Render Texture

If you want more control or need to apply effects:

1. Create a **new Camera** (call it "VideoCamera")
2. Set **Clear Flags** to Solid Color
3. Set **Culling Mask** to Nothing
4. Set **Target Texture** to your RenderTexture
5. Position VideoPlayer in world space for this camera to see
6. Use the RenderTexture in Canvas RawImage as before

This method gives you more flexibility for camera effects, but is more complex.

---

## Performance Tips

1. **RenderTexture resolution**: Use lowest acceptable quality (720p often sufficient)
2. **Video codec**: H.264 MP4 is well-supported and efficient
3. **Audio**: Disable if not needed
4. **Playback speed**: Slower = better performance (you're using 0.2x which is great)
5. **Mobile**: Consider static image fallback on low-end devices

---

## Example: Optional Video Controller Script

If you want runtime control (pause, play, etc.):

```csharp
using UnityEngine;
using UnityEngine.Video;

public class VideoBackgroundController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float defaultSpeed = 0.2f;
    
    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
            
        if (videoPlayer != null)
        {
            videoPlayer.playbackSpeed = defaultSpeed;
            videoPlayer.Play();
        }
    }
    
    public void PauseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Pause();
    }
    
    public void ResumeVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
            videoPlayer.Play();
    }
    
    public void SetPlaybackSpeed(float speed)
    {
        if (videoPlayer != null)
            videoPlayer.playbackSpeed = speed;
    }
}
```

Attach this to your VideoPlayer GameObject for easy control.

---

## Summary

**Quick Setup:**
1. Create RenderTexture (1920x1080)
2. Assign to VideoPlayer → Target Texture
3. Add RawImage as first child of Canvas
4. Assign RenderTexture to RawImage → Texture
5. Play!

**Your Existing Setup:**
You already have a VideoPlayer in your scene! Just:
1. Create the RenderTexture
2. Set VideoPlayer Render Mode to "Render Texture"
3. Assign the RenderTexture
4. Add RawImage to your new Canvas pointing to the RenderTexture

**Result:** Smooth looping video background with all UI elements rendering perfectly on top! 🎬


