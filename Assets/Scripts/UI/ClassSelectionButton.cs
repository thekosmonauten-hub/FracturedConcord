using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Specialized button for class selection in CharacterCreation scene.
/// Handles:
/// 1. Setting the selected class in ClassSelectionData
/// 2. Triggering video transition to CharacterDisplayUI
/// </summary>
[RequireComponent(typeof(Button))]
public class ClassSelectionButton : MonoBehaviour
{
    [Header("Class Settings")]
    [Tooltip("The class name this button represents (e.g., 'Witch', 'Marauder')")]
    [SerializeField] private string className = "";
    
    [Header("Transition Settings")]
    [Tooltip("Scene to load after video transition")]
    [SerializeField] private string targetSceneName = "CharacterDisplayUI";
    
    [Tooltip("Optional: Custom video to play (leave empty to use default)")]
    [SerializeField] private VideoClip customTransitionVideo = null;
    
    [Tooltip("Playback speed multiplier (1.0 = normal speed, 1.5 = 1.5x faster, 0.5 = half speed)")]
    [Range(0.1f, 3.0f)]
    [SerializeField] private float playbackSpeed = 1.0f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Button button;
    
    void Awake()
    {
        button = GetComponent<Button>();
        
        // Auto-detect class name from GameObject name if not set
        if (string.IsNullOrEmpty(className))
        {
            // Try to extract class name from GameObject name
            // E.g., "WitchStartingDeck" -> "Witch"
            string goName = gameObject.name;
            
            if (goName.Contains("Witch")) className = "Witch";
            else if (goName.Contains("Marauder")) className = "Marauder";
            else if (goName.Contains("Ranger")) className = "Ranger";
            else if (goName.Contains("Thief")) className = "Thief";
            else if (goName.Contains("Apostle")) className = "Apostle";
            else if (goName.Contains("Brawler")) className = "Brawler";
            
            if (!string.IsNullOrEmpty(className) && showDebugLogs)
            {
                Debug.Log($"[ClassSelectionButton] Auto-detected class name: {className} from GameObject: {goName}");
            }
        }
    }
    
    void Start()
    {
        // Validate
        if (string.IsNullOrEmpty(className))
        {
            Debug.LogError($"[ClassSelectionButton] className is not set on '{gameObject.name}'! Please assign it in Inspector.");
            return;
        }
        
        // Add listener to button
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
            
            if (showDebugLogs)
            {
                Debug.Log($"[ClassSelectionButton] '{gameObject.name}' configured for class: {className}");
            }
        }
        else
        {
            Debug.LogError($"[ClassSelectionButton] No Button component found on '{gameObject.name}'!");
        }
    }
    
    void OnButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=yellow>━━━ [ClassSelectionButton] '{className}' button clicked! ━━━</color>");
        }
        
        // 1. Store selected class in ClassSelectionData singleton
        ClassSelectionData.Instance.SetSelectedClass(className);
        
        if (showDebugLogs)
        {
            Debug.Log($"[ClassSelectionButton] Stored class '{className}' in ClassSelectionData");
        }
        
        // 2. Find VideoTransitionManager singleton
        VideoTransitionManager transitionManager = VideoTransitionManager.Instance;
        
        if (transitionManager == null)
        {
            Debug.LogError($"[ClassSelectionButton] VideoTransitionManager singleton not found! Loading scene directly...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            return;
        }
        
        // 3. Play transition with custom speed
        if (customTransitionVideo != null)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[ClassSelectionButton] Playing custom video: {customTransitionVideo.name} @ {playbackSpeed}x speed");
            }
            transitionManager.PlayTransitionAndLoadScene(targetSceneName, customTransitionVideo, playbackSpeed, false);
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log($"[ClassSelectionButton] Playing default transition video @ {playbackSpeed}x speed");
            }
            transitionManager.PlayTransitionAndLoadScene(targetSceneName, null, playbackSpeed, false);
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
    /// Manually set the class name (useful for dynamic button configuration)
    /// </summary>
    public void SetClassName(string newClassName)
    {
        className = newClassName;
        
        if (showDebugLogs)
        {
            Debug.Log($"[ClassSelectionButton] '{gameObject.name}' class name set to: {className}");
        }
    }
    
    /// <summary>
    /// Set the playback speed for the transition video
    /// </summary>
    /// <param name="speed">Playback speed multiplier (1.0 = normal, 1.5 = faster, 0.5 = slower)</param>
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = Mathf.Clamp(speed, 0.1f, 3.0f);
        
        if (showDebugLogs)
        {
            Debug.Log($"[ClassSelectionButton] '{gameObject.name}' playback speed set to: {playbackSpeed}x");
        }
    }
    
    /// <summary>
    /// Get the current playback speed
    /// </summary>
    public float GetPlaybackSpeed()
    {
        return playbackSpeed;
    }
}

