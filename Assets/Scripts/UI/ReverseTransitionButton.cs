using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Helper component for buttons that trigger reverse video transitions.
/// Useful for "Back" or "Cancel" buttons that should play the transition in reverse.
/// </summary>
[RequireComponent(typeof(Button))]
public class ReverseTransitionButton : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("Scene to load after reverse transition")]
    [SerializeField] private string targetSceneName = "";
    
    [Tooltip("Optional: Custom video to play (leave empty to use VideoTransitionManager's default)")]
    [SerializeField] private VideoClip customTransitionVideo = null;
    
    [Tooltip("Playback speed multiplier (1.5 = 1.5x faster)")]
    [SerializeField] private float playbackSpeed = 1.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Button button;
    
    void Awake()
    {
        button = GetComponent<Button>();
    }
    
    void Start()
    {
        // Validate
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"[ReverseTransitionButton] targetSceneName is not set on '{gameObject.name}'! Please assign it in Inspector.");
            return;
        }
        
        // Add listener to button
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
            
            if (showDebugLogs)
            {
                Debug.Log($"[ReverseTransitionButton] '{gameObject.name}' configured for reverse transition to: {targetSceneName} @ {playbackSpeed}x");
            }
        }
        else
        {
            Debug.LogError($"[ReverseTransitionButton] No Button component found on '{gameObject.name}'!");
        }
    }
    
    void OnButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=yellow>━━━ [ReverseTransitionButton] '{gameObject.name}' clicked! ━━━</color>");
        }
        
        // Find VideoTransitionManager singleton
        VideoTransitionManager transitionManager = VideoTransitionManager.Instance;
        
        if (transitionManager == null)
        {
            Debug.LogError($"[ReverseTransitionButton] VideoTransitionManager singleton not found! Loading scene directly...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            return;
        }
        
        // Play transition in reverse
        if (showDebugLogs)
        {
            string videoName = customTransitionVideo != null ? customTransitionVideo.name : "default";
            Debug.Log($"[ReverseTransitionButton] Playing video '{videoName}' in REVERSE @ {playbackSpeed}x → {targetSceneName}");
        }
        
        transitionManager.PlayTransitionAndLoadScene(
            targetSceneName,
            customTransitionVideo,
            playbackSpeed,
            true // playInReverse = true
        );
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
    /// Manually set the target scene name (useful for dynamic button configuration)
    /// </summary>
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        
        if (showDebugLogs)
        {
            Debug.Log($"[ReverseTransitionButton] '{gameObject.name}' target scene set to: {targetSceneName}");
        }
    }
    
    /// <summary>
    /// Manually set the playback speed
    /// </summary>
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        
        if (showDebugLogs)
        {
            Debug.Log($"[ReverseTransitionButton] '{gameObject.name}' playback speed set to: {playbackSpeed}x");
        }
    }
}

