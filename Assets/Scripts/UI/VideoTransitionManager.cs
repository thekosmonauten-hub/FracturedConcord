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
    [Tooltip("Wait for video to finish before loading scene")]
    [SerializeField] private bool waitForVideoToFinish = true;
    [Tooltip("Minimum transition duration (even if video is shorter)")]
    [SerializeField] private float minTransitionDuration = 2f;
    
    [Header("Audio")]
    [SerializeField] private bool playAudio = false;
    
    [Header("Auto-Setup (Leave Empty - Auto-Created)")]
    [SerializeField] private Canvas transitionCanvas;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture renderTexture;
    
    // Store last used video for reverse transitions
    private VideoClip lastUsedVideo;
    
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
        Debug.Log("[VideoTransitionManager] Setting up transition system...");
        
        // Create RenderTexture
        renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.name = "VideoTransitionRT";
        
        // Setup VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.skipOnDrop = true;
        videoPlayer.audioOutputMode = playAudio ? VideoAudioOutputMode.Direct : VideoAudioOutputMode.None;
        
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
        videoDisplay.color = Color.white;
        
        RectTransform rect = videoObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        // Hide initially
        canvasObj.SetActive(false);
        
        // Disable raycasting on transition canvas so it doesn't block UI
        var raycaster = canvasObj.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            raycaster.enabled = false;
        }
        
        Debug.Log("[VideoTransitionManager] Setup complete!");
    }

    /// <summary>
    /// Get the last used video clip (useful for reverse transitions)
    /// </summary>
    public VideoClip GetLastUsedVideo()
    {
        return lastUsedVideo;
    }
    
    /// <summary>
    /// Play video transition and load scene (Inspector-friendly)
    /// </summary>
    public void PlayTransitionAndLoadScene(string sceneName)
    {
        PlayTransitionAndLoadScene(sceneName, null);
    }
    
    /// <summary>
    /// Play video transition and load scene with custom video
    /// </summary>
    public void PlayTransitionAndLoadScene(string sceneName, VideoClip customVideo)
    {
        PlayTransitionAndLoadScene(sceneName, customVideo, 1f, false);
    }
    
    /// <summary>
    /// Play video transition with speed and direction control
    /// </summary>
    /// <param name="sceneName">Scene to load after transition</param>
    /// <param name="customVideo">Optional custom video (uses default if null)</param>
    /// <param name="playbackSpeed">Playback speed multiplier (1.0 = normal, 1.5 = 1.5x faster, etc.)</param>
    /// <param name="playInReverse">If true, plays video in reverse</param>
    public void PlayTransitionAndLoadScene(string sceneName, VideoClip customVideo, float playbackSpeed, bool playInReverse)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[VideoTransitionManager] Scene name is null or empty!");
            return;
        }
        
        StartCoroutine(VideoTransitionCoroutine(sceneName, customVideo, playbackSpeed, playInReverse));
    }

    IEnumerator VideoTransitionCoroutine(string sceneName, VideoClip customVideo = null, float playbackSpeed = 1f, bool playInReverse = false)
    {
        // Determine which video to use
        VideoClip videoToPlay = null;
        
        if (customVideo != null)
        {
            videoToPlay = customVideo;
            Debug.Log($"[VideoTransitionManager] Using custom video: {videoToPlay.name}");
        }
        else if (lastUsedVideo != null)
        {
            videoToPlay = lastUsedVideo;
            Debug.Log($"[VideoTransitionManager] Using last used video: {videoToPlay.name}");
        }
        else if (transitionVideo != null)
        {
            videoToPlay = transitionVideo;
            Debug.Log($"[VideoTransitionManager] Using default transition video: {videoToPlay.name}");
        }
        
        if (videoToPlay == null)
        {
            Debug.LogError("[VideoTransitionManager] ❌ NO VIDEO CLIP AVAILABLE!");
            Debug.LogError($"  - customVideo: {(customVideo != null ? customVideo.name : "NULL")}");
            Debug.LogError($"  - lastUsedVideo: {(lastUsedVideo != null ? lastUsedVideo.name : "NULL")}");
            Debug.LogError($"  - transitionVideo: {(transitionVideo != null ? transitionVideo.name : "NULL")}");
            Debug.LogError("Loading scene directly without transition.");
            SceneManager.LoadScene(sceneName);
            yield break;
        }
        
        // Store this video as last used (for reverse transitions)
        if (!playInReverse)
        {
            lastUsedVideo = videoToPlay;
            Debug.Log($"<color=lime>[VideoTransitionManager] ✓ Stored video for reverse: {videoToPlay.name}</color>");
        }
        
        string directionText = playInReverse ? "REVERSE" : "FORWARD";
        Debug.Log($"<color=magenta>━━━ [VideoTransition] Playing: {videoToPlay.name} [{directionText} @ {playbackSpeed}x] → {sceneName} ━━━</color>");
        
        // Show transition canvas
        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(true);
            
            // Enable raycasting during transition to block all input
            var raycaster = transitionCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.enabled = true;
            }
        }
        
        // Setup and play video
        videoPlayer.clip = videoToPlay;
        
        // For forward playback, set speed now
        if (!playInReverse)
        {
            videoPlayer.playbackSpeed = playbackSpeed;
        }
        
        videoPlayer.Prepare();
        
        // Wait for video to be prepared
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        // AFTER video is prepared, set up reverse playback
        if (playInReverse)
        {
            // Seek to the end FIRST
            videoPlayer.time = videoToPlay.length - 0.01; // Start at end (minus a tiny bit to avoid edge case)
            Debug.Log($"[VideoTransition] Seeking to end of video ({videoPlayer.time:F2}s) for reverse playback");
            
            // Wait one frame for seek to complete
            yield return null;
            
            // NOW set negative playback speed
            videoPlayer.playbackSpeed = -playbackSpeed; // Negative = reverse
            Debug.Log($"[VideoTransition] Set playback speed to {videoPlayer.playbackSpeed}x (REVERSE)");
        }
        
        Debug.Log($"[VideoTransition] Video prepared (speed: {videoPlayer.playbackSpeed}x), starting playback...");
        
        videoPlayer.Play();
        
        // Track video duration
        float startTime = Time.time;
        float videoDuration = (float)videoToPlay.length;
        
        // Account for playback speed in duration calculation
        float adjustedDuration = videoDuration / Mathf.Abs(playbackSpeed);
        
        Debug.Log($"[VideoTransition] Video duration: {videoDuration:F2}s (adjusted for speed: {adjustedDuration:F2}s), Min duration: {minTransitionDuration:F2}s");
        
        // Wait for video OR minimum duration (whichever is longer)
        float waitDuration = Mathf.Max(adjustedDuration, minTransitionDuration);
        
        if (waitForVideoToFinish)
        {
            // Wait for actual video playback to complete
            // For reverse playback, check if time is reaching 0
            if (playInReverse)
            {
                while (videoPlayer.isPlaying && videoPlayer.time > 0.1f && (Time.time - startTime) < waitDuration + 1f)
                {
                    yield return null;
                }
            }
            else
            {
                while (videoPlayer.isPlaying && (Time.time - startTime) < waitDuration + 1f)
                {
                    yield return null;
                }
            }
            
            Debug.Log($"[VideoTransition] Video finished after {(Time.time - startTime):F2}s");
        }
        else
        {
            // Just wait minimum duration
            yield return new WaitForSeconds(minTransitionDuration);
            Debug.Log($"[VideoTransition] Minimum duration reached");
        }
        
        Debug.Log($"<color=yellow>[VideoTransition] Loading scene: {sceneName}</color>");
        
        // Load scene
        SceneManager.LoadScene(sceneName);
        
        // Wait a frame for scene to start loading
        yield return new WaitForSeconds(0.1f);
        
        // Stop video and hide transition
        videoPlayer.Stop();
        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(false);
            
            // Disable raycasting so it doesn't block UI in next scene
            var raycaster = transitionCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.enabled = false;
            }
        }
        
        Debug.Log("[VideoTransition] Transition hidden, scene loaded!");
    }

    /// <summary>
    /// Test video playback without loading a scene
    /// </summary>
    [ContextMenu("Test Play Video")]
    public void TestPlayVideo()
    {
        if (transitionVideo == null)
        {
            Debug.LogError("[VideoTransition] No video assigned!");
            return;
        }
        
        StartCoroutine(TestVideoCoroutine());
    }

    IEnumerator TestVideoCoroutine()
    {
        Debug.Log($"<color=cyan>[VideoTransition] Testing video: {transitionVideo.name}</color>");
        
        transitionCanvas.gameObject.SetActive(true);
        videoPlayer.clip = transitionVideo;
        videoPlayer.Prepare();
        
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        videoPlayer.Play();
        
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);
        
        transitionCanvas.gameObject.SetActive(false);
        Debug.Log("[VideoTransition] Test complete!");
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}

