using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System;

/// <summary>
/// Plays a video transition within the same scene, then triggers a callback.
/// Use for UI transitions without loading new scenes.
/// </summary>
public class InSceneVideoTransition : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoClip transitionVideo;
    [Tooltip("Minimum transition duration (even if video is shorter)")]
    [SerializeField] private float minTransitionDuration = 1f;
    
    [Header("Audio")]
    [SerializeField] private bool playAudio = false;
    
    [Header("Auto-Setup (Leave Empty - Auto-Created)")]
    [SerializeField] private Canvas transitionCanvas;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture renderTexture;
    
    [Header("Optional: UI to Hide During Transition")]
    [SerializeField] private GameObject[] uiToHideDuringTransition;
    
    [Header("Optional: UI to Show After Transition")]
    [SerializeField] private GameObject[] uiToShowAfterTransition;
    
    private Action onTransitionComplete;
    
    void Awake()
    {
        SetupTransition();
    }

    void SetupTransition()
    {
        Debug.Log("[InSceneVideoTransition] Setting up transition system...");
        
        // Create RenderTexture
        renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.name = "InSceneVideoTransitionRT";
        
        // Setup VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.skipOnDrop = true;
        videoPlayer.audioOutputMode = playAudio ? VideoAudioOutputMode.Direct : VideoAudioOutputMode.None;
        
        // Create Canvas
        GameObject canvasObj = new GameObject("InSceneVideoTransitionCanvas");
        canvasObj.transform.SetParent(transform);
        
        transitionCanvas = canvasObj.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = 9999; // Very high - on top of everything
        
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
        
        Debug.Log("[InSceneVideoTransition] Setup complete!");
    }

    /// <summary>
    /// Play video transition with a callback when complete
    /// </summary>
    public void PlayTransition(Action onComplete = null, VideoClip customVideo = null)
    {
        onTransitionComplete = onComplete;
        StartCoroutine(VideoTransitionCoroutine(customVideo));
    }
    
    /// <summary>
    /// Play video transition and show specific UI containers afterward
    /// </summary>
    public void PlayTransitionAndShowUI(GameObject[] containersToShow, VideoClip customVideo = null)
    {
        StartCoroutine(VideoTransitionCoroutine(customVideo, containersToShow));
    }

    IEnumerator VideoTransitionCoroutine(VideoClip customVideo = null, GameObject[] containersToShow = null)
    {
        // Use custom video if provided, otherwise use default
        VideoClip videoToPlay = customVideo != null ? customVideo : transitionVideo;
        
        if (videoToPlay == null)
        {
            Debug.LogError("[InSceneVideoTransition] No video clip assigned! Skipping transition.");
            onTransitionComplete?.Invoke();
            ShowUIContainers(containersToShow);
            yield break;
        }
        
        Debug.Log($"<color=magenta>━━━ [InSceneVideoTransition] Playing: {videoToPlay.name} ━━━</color>");
        
        // Hide specified UI during transition
        HideUIContainers(uiToHideDuringTransition);
        
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
        
        Debug.Log("[InSceneVideoTransition] Video prepared, starting playback...");
        videoPlayer.Play();
        
        // Track video duration
        float startTime = Time.time;
        float videoDuration = (float)videoToPlay.length;
        
        Debug.Log($"[InSceneVideoTransition] Video duration: {videoDuration:F2}s, Min duration: {minTransitionDuration:F2}s");
        
        // Wait for video OR minimum duration (whichever is longer)
        float waitDuration = Mathf.Max(videoDuration, minTransitionDuration);
        
        // Wait for actual video playback to complete
        while (videoPlayer.isPlaying && (Time.time - startTime) < waitDuration + 1f)
        {
            yield return null;
        }
        
        Debug.Log($"[InSceneVideoTransition] Video finished after {(Time.time - startTime):F2}s");
        
        // Stop video and hide transition
        videoPlayer.Stop();
        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(false);
        }
        
        // Show UI containers that were specified
        ShowUIContainers(containersToShow);
        ShowUIContainers(uiToShowAfterTransition);
        
        // Trigger callback
        onTransitionComplete?.Invoke();
        
        Debug.Log("[InSceneVideoTransition] Transition complete!");
    }
    
    private void HideUIContainers(GameObject[] containers)
    {
        if (containers == null || containers.Length == 0) return;
        
        foreach (GameObject container in containers)
        {
            if (container != null)
            {
                container.SetActive(false);
                Debug.Log($"[InSceneVideoTransition] Hiding: {container.name}");
            }
        }
    }
    
    private void ShowUIContainers(GameObject[] containers)
    {
        if (containers == null || containers.Length == 0) return;
        
        foreach (GameObject container in containers)
        {
            if (container != null)
            {
                container.SetActive(true);
                Debug.Log($"[InSceneVideoTransition] Showing: {container.name}");
            }
        }
    }

    /// <summary>
    /// Test video playback
    /// </summary>
    [ContextMenu("Test Play Video")]
    public void TestPlayVideo()
    {
        if (transitionVideo == null)
        {
            Debug.LogError("[InSceneVideoTransition] No video assigned!");
            return;
        }
        
        PlayTransition(() => Debug.Log("[InSceneVideoTransition] Test complete!"));
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


