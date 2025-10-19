using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles automatic saving of character data at key moments:
/// - Scene transitions
/// - Periodic auto-save every N minutes
/// - On application quit/pause
/// </summary>
public class AutoSaveManager : MonoBehaviour
{
    private static AutoSaveManager _instance;
    public static AutoSaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AutoSaveManager");
                _instance = go.AddComponent<AutoSaveManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    [Header("Periodic Auto-Save Settings")]
    [SerializeField] private bool enablePeriodicAutoSave = true;
    [SerializeField] private float autoSaveIntervalMinutes = 5f;
    private float timeSinceLastSave = 0f;
    
    [Header("Scene Transition Settings")]
    [SerializeField] private bool saveOnSceneTransition = true;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Subscribe to scene change events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void Update()
    {
        // Periodic auto-save timer
        if (enablePeriodicAutoSave)
        {
            timeSinceLastSave += Time.deltaTime;
            
            if (timeSinceLastSave >= autoSaveIntervalMinutes * 60f)
            {
                PeriodicAutoSave();
                timeSinceLastSave = 0f;
            }
        }
    }
    
    /// <summary>
    /// Called when a scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (saveOnSceneTransition)
        {
            SaveOnSceneTransition(scene.name);
        }
    }
    
    /// <summary>
    /// Save character data when transitioning between scenes
    /// </summary>
    private void SaveOnSceneTransition(string sceneName)
    {
        if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
        {
            CharacterManager.Instance.SaveCharacter();
            Debug.Log($"[Auto-Save] Character saved on scene transition to: {sceneName}");
        }
    }
    
    /// <summary>
    /// Periodic auto-save every N minutes
    /// </summary>
    private void PeriodicAutoSave()
    {
        if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
        {
            CharacterManager.Instance.SaveCharacter();
            Debug.Log($"[Auto-Save] Periodic auto-save completed ({autoSaveIntervalMinutes} minute interval).");
        }
    }
    
    /// <summary>
    /// Save on application quit
    /// </summary>
    private void OnApplicationQuit()
    {
        if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
        {
            CharacterManager.Instance.SaveCharacter();
            Debug.Log("[Auto-Save] Character saved on application quit.");
        }
    }
    
    /// <summary>
    /// Save on application pause (mobile/console)
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) // Pausing
        {
            if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
            {
                CharacterManager.Instance.SaveCharacter();
                Debug.Log("[Auto-Save] Character saved on application pause.");
            }
        }
    }
    
    /// <summary>
    /// Manually trigger a save (can be called from UI buttons, etc.)
    /// </summary>
    public void ManualSave()
    {
        if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
        {
            CharacterManager.Instance.SaveCharacter();
            Debug.Log("[Auto-Save] Manual save completed.");
            
            // Reset periodic timer
            timeSinceLastSave = 0f;
        }
        else
        {
            Debug.LogWarning("[Auto-Save] No character loaded to save.");
        }
    }
    
    /// <summary>
    /// Enable or disable periodic auto-save
    /// </summary>
    public void SetPeriodicAutoSave(bool enabled)
    {
        enablePeriodicAutoSave = enabled;
        Debug.Log($"[Auto-Save] Periodic auto-save {(enabled ? "enabled" : "disabled")}.");
    }
    
    /// <summary>
    /// Set the auto-save interval in minutes
    /// </summary>
    public void SetAutoSaveInterval(float minutes)
    {
        autoSaveIntervalMinutes = Mathf.Max(1f, minutes); // Minimum 1 minute
        Debug.Log($"[Auto-Save] Auto-save interval set to {autoSaveIntervalMinutes} minutes.");
    }
}



