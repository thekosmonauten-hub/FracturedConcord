# Video Transition Setup Guide

## Overview
Play a video transition when moving between scenes (e.g., clicking "Start Journey" button).

---

## Two Approaches

### **Approach A: Add Video to Existing TransitionManager** (Recommended - Simpler)
Extends your current TransitionManager with video support

### **Approach B: Standalone Video Transition System** (More Control)
Creates a separate video transition manager

---

# APPROACH A: Extend TransitionManager with Video (Recommended)

## Step 1: Add Video Support to TransitionManager

Add these fields to your existing `TransitionManager.cs`:

```csharp
[Header("Video Transition")]
[SerializeField] private VideoClip transitionVideoClip;
[SerializeField] private RawImage videoTransitionDisplay;
[SerializeField] private VideoPlayer videoPlayer;
[SerializeField] private RenderTexture videoRenderTexture;
[SerializeField] private bool useVideoTransition = false;
```

Add this new method to TransitionManager:

```csharp
/// <summary>
/// Transition with video playback
/// </summary>
public void TransitionToSceneWithVideo(string sceneName)
{
    StartCoroutine(VideoTransitionCoroutine(sceneName));
}

private IEnumerator VideoTransitionCoroutine(string sceneName)
{
    if (transitionVideoClip == null || videoPlayer == null || videoRenderTexture == null)
    {
        Debug.LogWarning("[TransitionManager] Video transition not configured, using default transition");
        TransitionToScene(sceneName);
        yield break;
    }

    // Show transition canvas
    GameObject transitionCanvas = transform.GetChild(0).gameObject;
    transitionCanvas.SetActive(true);
    
    // Setup video player
    videoPlayer.clip = transitionVideoClip;
    videoPlayer.targetTexture = videoRenderTexture;
    videoPlayer.isLooping = false;
    videoPlayer.playOnAwake = false;
    videoPlayer.renderMode = VideoRenderMode.RenderTexture;
    
    // Show video display
    if (videoTransitionDisplay != null)
    {
        videoTransitionDisplay.gameObject.SetActive(true);
        videoTransitionDisplay.texture = videoRenderTexture;
    }
    
    // Play video
    videoPlayer.Play();
    Debug.Log($"[TransitionManager] Playing transition video ({transitionVideoClip.name})");
    
    // Wait for video to finish
    while (videoPlayer.isPlaying)
    {
        yield return null;
    }
    
    Debug.Log("[TransitionManager] Video finished, loading scene...");
    
    // Load new scene
    SceneManager.LoadScene(sceneName);
    
    // Hide video after scene loads
    yield return new WaitForSeconds(0.1f);
    
    if (videoTransitionDisplay != null)
    {
        videoTransitionDisplay.gameObject.SetActive(false);
    }
    
    transitionCanvas.SetActive(false);
}
```

Update the main TransitionToScene method to support video:

```csharp
public void TransitionToScene(string sceneName)
{
    if (useVideoTransition && transitionVideoClip != null)
    {
        StartCoroutine(VideoTransitionCoroutine(sceneName));
    }
    else
    {
        StartCoroutine(TransitionCoroutine(sceneName));
    }
}
```

---

## Step 2: Setup in Unity (Approach A)

### 2.1: Create RenderTexture for Video

1. Project window → **Assets/RenderTextures** (create folder if needed)
2. Right-click → **Create** → **Render Texture**
3. Rename to **TransitionVideoRT**
4. Settings:
   - Size: 1920 x 1080 (match video resolution)
   - Depth Buffer: None
   - Anti-aliasing: None

### 2.2: Add Video Components to TransitionManager

Your TransitionManager GameObject should have these components:

1. **TransitionManager** (script) - already exists
2. **VideoPlayer** component:
   - Click **Add Component** → Search "Video Player"
   - Source: **Video Clip**
   - Video Clip: Drag your transition video
   - Render Mode: **Render Texture**
   - Target Texture: **TransitionVideoRT**
   - Play On Awake: ❌ Unchecked
   - Loop: ❌ Unchecked
   - Skip On Drop: ✅ Checked

### 2.3: Update TransitionCanvas Hierarchy

The TransitionManager creates a TransitionCanvas on `Awake()`. Modify `SetupTransitionUI()` to add video support:

**After TransitionCanvas is created, add:**

```csharp
// Add RawImage for video display (in SetupTransitionUI method)
GameObject videoDisplayGO = new GameObject("VideoTransitionDisplay");
videoDisplayGO.transform.SetParent(canvasGO.transform);

RawImage videoRawImage = videoDisplayGO.AddComponent<RawImage>();
videoRawImage.texture = videoRenderTexture;

RectTransform videoRect = videoDisplayGO.GetComponent<RectTransform>();
videoRect.anchorMin = Vector2.zero;
videoRect.anchorMax = Vector2.one;
videoRect.offsetMin = Vector2.zero;
videoRect.offsetMax = Vector2.zero;

videoTransitionDisplay = videoRawImage;
videoDisplayGO.SetActive(false); // Hidden initially

// Move to be behind transition frame
videoDisplayGO.transform.SetSiblingIndex(1); // After background, before frame
```

### 2.4: Assign References in Inspector

Select **TransitionManager** GameObject and assign:

**Video Transition (new fields):**
- **Transition Video Clip**: Drag your video file
- **Video Transition Display**: Will be auto-created (or manually assign)
- **Video Player**: Drag the VideoPlayer component
- **Video Render Texture**: Drag **TransitionVideoRT**
- **Use Video Transition**: ✅ Checked (if you want video by default)

---

## Step 3: Update MainMenuController to Use Video Transition

No changes needed! Your `MainMenuController.LoadScene()` already uses `TransitionManager.TransitionToScene()`, which now supports video!

**Optional**: If you want to force video transition for specific scenes:

```csharp
void LoadScene(string sceneName)
{
    if (transitionManager != null)
    {
        // Force video transition for game scene
        if (sceneName == gameSceneName && transitionManager.GetComponent<VideoPlayer>() != null)
        {
            transitionManager.TransitionToSceneWithVideo(sceneName);
        }
        else
        {
            transitionManager.TransitionToScene(sceneName);
        }
    }
    else
    {
        SceneManager.LoadScene(sceneName);
    }
}
```

---

# APPROACH B: Standalone Video Transition System

If you want more control or different videos for different transitions:

## Step 1: Create VideoTransitionManager Script

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Standalone video transition system.
/// Plays a video clip when transitioning between scenes.
/// </summary>
public class VideoTransitionManager : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoClip transitionVideo;
    [SerializeField] private bool waitForVideoToFinish = true;
    [SerializeField] private float minTransitionDuration = 2f;
    
    [Header("Auto-Setup (Leave Empty)")]
    [SerializeField] private Canvas transitionCanvas;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture renderTexture;
    
    private static VideoTransitionManager _instance;
    public static VideoTransitionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<VideoTransitionManager>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SetupTransition();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void SetupTransition()
    {
        // Create RenderTexture
        renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.name = "VideoTransitionRT";
        
        // Setup VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // Or Direct if you want audio
        
        // Create Canvas
        GameObject canvasObj = new GameObject("VideoTransitionCanvas");
        canvasObj.transform.SetParent(transform);
        
        transitionCanvas = canvasObj.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = 10000; // Very high - on top of everything
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create RawImage for video display
        GameObject videoObj = new GameObject("VideoDisplay");
        videoObj.transform.SetParent(canvasObj.transform);
        
        videoDisplay = videoObj.AddComponent<RawImage>();
        videoDisplay.texture = renderTexture;
        
        RectTransform rect = videoObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        // Hide initially
        canvasObj.SetActive(false);
        
        Debug.Log("[VideoTransitionManager] Setup complete!");
    }

    /// <summary>
    /// Play video transition and load scene
    /// </summary>
    public void PlayTransitionAndLoadScene(string sceneName, VideoClip customVideo = null)
    {
        StartCoroutine(VideoTransitionCoroutine(sceneName, customVideo));
    }

    IEnumerator VideoTransitionCoroutine(string sceneName, VideoClip customVideo = null)
    {
        // Use custom video if provided, otherwise use default
        VideoClip videoToPlay = customVideo != null ? customVideo : transitionVideo;
        
        if (videoToPlay == null)
        {
            Debug.LogError("[VideoTransitionManager] No video clip assigned!");
            SceneManager.LoadScene(sceneName);
            yield break;
        }
        
        Debug.Log($"<color=magenta>[VideoTransition] Playing: {videoToPlay.name} → {sceneName}</color>");
        
        // Show transition canvas
        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(true);
        }
        
        // Setup and play video
        videoPlayer.clip = videoToPlay;
        videoPlayer.Prepare();
        
        // Wait for video to be prepared
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        videoPlayer.Play();
        
        // Track video duration
        float startTime = Time.time;
        float videoDuration = (float)videoToPlay.length;
        
        Debug.Log($"[VideoTransition] Video duration: {videoDuration}s, Min duration: {minTransitionDuration}s");
        
        // Wait for video OR minimum duration (whichever is longer)
        float waitDuration = Mathf.Max(videoDuration, minTransitionDuration);
        
        if (waitForVideoToFinish)
        {
            // Wait for actual video playback
            while (videoPlayer.isPlaying && (Time.time - startTime) < waitDuration + 1f)
            {
                yield return null;
            }
        }
        else
        {
            // Just wait minimum duration
            yield return new WaitForSeconds(minTransitionDuration);
        }
        
        Debug.Log($"[VideoTransition] Transition complete, loading scene: {sceneName}");
        
        // Load scene
        SceneManager.LoadScene(sceneName);
        
        // Wait for scene to load
        yield return new WaitForSeconds(0.1f);
        
        // Hide transition
        videoPlayer.Stop();
        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(false);
        }
        
        Debug.Log("[VideoTransition] Transition hidden");
    }

    /// <summary>
    /// Quick play without scene load (for testing)
    /// </summary>
    [ContextMenu("Test Play Video")]
    public void TestPlayVideo()
    {
        StartCoroutine(TestVideoCoroutine());
    }

    IEnumerator TestVideoCoroutine()
    {
        if (transitionVideo == null)
        {
            Debug.LogError("[VideoTransition] No video assigned!");
            yield break;
        }

        transitionCanvas.gameObject.SetActive(true);
        videoPlayer.clip = transitionVideo;
        videoPlayer.Play();
        
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }
        
        transitionCanvas.gameObject.SetActive(false);
    }
}
```

## Step 2: Setup VideoTransitionManager in Scene

### 2.1: Add to MainMenu Scene

1. Create empty GameObject in MainMenu scene
2. Rename to **VideoTransitionManager**
3. Add `VideoTransitionManager` script
4. Set to **DontDestroyOnLoad** (script handles this)

### 2.2: Assign Video in Inspector

Select **VideoTransitionManager** and assign:

**Video Settings:**
- **Transition Video**: Drag your video file from Project
- **Wait For Video To Finish**: ✅ Checked
- **Min Transition Duration**: `2` (seconds - ensures transition isn't too quick)

**Auto-Setup fields** will be created automatically, but you can verify:
- All fields will auto-populate on `Awake()`

---

## Step 3: Update MainMenuController to Use Video Transition

### Option 1: Always Use Video (Simple)

```csharp
void LoadScene(string sceneName)
{
    // Check for video transition manager first
    VideoTransitionManager videoTransition = FindObjectOfType<VideoTransitionManager>();
    
    if (videoTransition != null)
    {
        Debug.Log($"[MainMenu] Loading scene {sceneName} with VIDEO transition");
        videoTransition.PlayTransitionAndLoadScene(sceneName);
    }
    else if (transitionManager != null)
    {
        Debug.Log($"[MainMenu] Loading scene {sceneName} via TransitionManager");
        transitionManager.TransitionToScene(sceneName);
    }
    else
    {
        Debug.Log($"[MainMenu] Loading scene {sceneName} directly");
        SceneManager.LoadScene(sceneName);
    }
}
```

### Option 2: Video Only for Specific Scenes

```csharp
void LoadScene(string sceneName)
{
    VideoTransitionManager videoTransition = FindObjectOfType<VideoTransitionManager>();
    
    // Use video transition only for game scene
    if (sceneName == gameSceneName && videoTransition != null)
    {
        Debug.Log($"[MainMenu] Loading {sceneName} with VIDEO transition");
        videoTransition.PlayTransitionAndLoadScene(sceneName);
    }
    else if (transitionManager != null)
    {
        Debug.Log($"[MainMenu] Loading {sceneName} via standard transition");
        transitionManager.TransitionToScene(sceneName);
    }
    else
    {
        SceneManager.LoadScene(sceneName);
    }
}
```

---

## Step 4: Testing

### 4.1: Test Video Playback
1. Select **VideoTransitionManager** in Hierarchy
2. Right-click component → **Test Play Video**
3. Video should play full-screen and then hide

### 4.2: Test Scene Transition
1. Enter Play Mode
2. Click **Start Journey** button
3. Should see:
   ```
   [MainMenu] Loading scene GameScene with VIDEO transition
   [VideoTransition] Playing: YourVideo.mp4 → GameScene
   [VideoTransition] Video duration: 3.5s
   [VideoTransition] Transition complete, loading scene: GameScene
   ```

---

## Advanced: Multiple Videos for Different Transitions

### Create Video Transition Configuration

```csharp
[System.Serializable]
public class SceneTransitionConfig
{
    public string fromScene;
    public string toScene;
    public VideoClip transitionVideo;
}

[Header("Multi-Video Configuration")]
[SerializeField] private List<SceneTransitionConfig> transitionConfigs;

public void PlayTransitionForScenes(string fromScene, string toScene)
{
    // Find matching config
    var config = transitionConfigs.Find(c => 
        c.fromScene == fromScene && c.toScene == toScene);
    
    if (config != null && config.transitionVideo != null)
    {
        PlayTransitionAndLoadScene(toScene, config.transitionVideo);
    }
    else
    {
        // Use default video
        PlayTransitionAndLoadScene(toScene);
    }
}
```

Usage:
```csharp
videoTransition.PlayTransitionForScenes("MainMenu", "GameScene");
```

---

## Troubleshooting

### Video doesn't play?
**Check:**
- Video clip is assigned in Inspector
- RenderTexture exists and is assigned
- VideoPlayer component exists on VideoTransitionManager
- Console shows "Playing transition video" message

**Common fixes:**
- Verify video codec is supported (H.264 MP4 recommended)
- Check video is in Assets folder (not StreamingAssets)
- Ensure RenderTexture dimensions match video

### Video plays but scene doesn't load?
**Check:**
- Scene name is correct (case-sensitive)
- Scene is in Build Settings
- Console shows "loading scene" message

**Fix:**
- Add scene to Build Settings: File → Build Settings → Add Open Scenes
- Verify scene name matches exactly

### Video is choppy or laggy?
**Optimize:**
- Reduce RenderTexture resolution (1280x720 instead of 1920x1080)
- Lower video quality/bitrate
- Disable audio if not needed
- Use VideoPlayer.skipOnDrop = true

### Scene loads too quickly (video cut off)?
**Fix:**
- Increase `minTransitionDuration`
- Check `waitForVideoToFinish` is enabled
- Verify video is actually playing (check isPlaying)

### Video appears upside down?
**Fix:**
- Flip the RawImage UV Rect:
  ```csharp
  videoDisplay.uvRect = new Rect(0, 1, 1, -1); // Negative height flips it
  ```

---

## Performance Tips

1. **Video Resolution**: Match or slightly exceed screen resolution (1920x1080)
2. **Video Length**: Keep under 5 seconds for snappy feel
3. **Codec**: H.264 MP4 is most reliable
4. **Audio**: Disable if not needed (saves processing)
5. **Preload**: Consider preparing video on scene start for instant playback

---

## Quick Setup Summary (Approach A)

**5-Minute Setup:**
1. ✅ Create RenderTexture (TransitionVideoRT)
2. ✅ Add VideoPlayer component to TransitionManager GameObject
3. ✅ Add video transition code to TransitionManager.cs (provided above)
4. ✅ Assign video clip and render texture in Inspector
5. ✅ Enable "Use Video Transition" checkbox
6. ✅ Test!

**Result:** Smooth video transitions between scenes! 🎬

---

## Alternative: Quick and Dirty (No Code Changes)

If you don't want to modify TransitionManager:

1. Create **VideoTransitionManager** as separate GameObject
2. In MainMenuController, check for VideoTransitionManager before TransitionManager
3. Use VideoTransitionManager if found, otherwise fallback to TransitionManager

This keeps your existing systems untouched!

---

## Which Approach to Use?

**Use Approach A (Extend TransitionManager) if:**
- ✅ You want one unified transition system
- ✅ You're comfortable modifying existing code
- ✅ You want video as the default transition

**Use Approach B (Standalone) if:**
- ✅ You want to keep existing TransitionManager untouched
- ✅ You want different videos for different transitions
- ✅ You want more control and flexibility
- ✅ You might disable video transitions later without code changes

---

## Recommended: Approach B (Standalone)

I recommend **Approach B** because:
- No risk of breaking existing transitions
- Easy to toggle on/off
- Can use different videos per transition
- Cleaner separation of concerns

Just create the `VideoTransitionManager` script, add it to a GameObject, assign your video, and update `MainMenuController.LoadScene()` to check for it!


