using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Helper component for buttons that need to trigger video transitions.
/// Automatically finds VideoTransitionManager singleton at runtime.
/// Attach this to any button that should play a video transition.
/// </summary>
[RequireComponent(typeof(Button))]
public class VideoTransitionButton : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("Scene to load after video transition")]
    [SerializeField] private string targetSceneName = "CharacterDisplayUI";
    
    [Tooltip("Optional: Custom video to play (leave empty to use default transition video)")]
    [SerializeField] private VideoClip customTransitionVideo = null;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Button button;
    
    void Awake()
    {
        button = GetComponent<Button>();
    }
    
    void Start()
    {
        // Add listener to button
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
            
            if (showDebugLogs)
            {
                Debug.Log($"[VideoTransitionButton] '{gameObject.name}' configured to transition to: {targetSceneName}");
            }
        }
        else
        {
            Debug.LogError($"[VideoTransitionButton] No Button component found on '{gameObject.name}'!");
        }
    }
    
    void OnButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[VideoTransitionButton] '{gameObject.name}' clicked! Triggering transition to: {targetSceneName}</color>");
        }
        
        // Find VideoTransitionManager singleton
        VideoTransitionManager transitionManager = VideoTransitionManager.Instance;
        
        if (transitionManager == null)
        {
            Debug.LogError($"[VideoTransitionButton] VideoTransitionManager singleton not found! Cannot play transition. Loading scene directly...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            return;
        }
        
        // Play transition
        if (customTransitionVideo != null)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[VideoTransitionButton] Playing custom video: {customTransitionVideo.name}");
            }
            transitionManager.PlayTransitionAndLoadScene(targetSceneName, customTransitionVideo);
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log($"[VideoTransitionButton] Playing default transition video");
            }
            transitionManager.PlayTransitionAndLoadScene(targetSceneName);
        }
    }
    
    void OnDestroy()
    {
        // Clean up listener
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
    
    /// <summary>
    /// Manually set the target scene (useful for dynamic button configuration)
    /// </summary>
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        
        if (showDebugLogs)
        {
            Debug.Log($"[VideoTransitionButton] '{gameObject.name}' target scene changed to: {targetSceneName}");
        }
    }
    
    /// <summary>
    /// Manually set the custom video (useful for dynamic button configuration)
    /// </summary>
    public void SetCustomVideo(VideoClip video)
    {
        customTransitionVideo = video;
        
        if (showDebugLogs)
        {
            Debug.Log($"[VideoTransitionButton] '{gameObject.name}' custom video set to: {(video != null ? video.name : "null")}");
        }
    }
}

