using UnityEngine;

/// <summary>
/// Controls the visibility of a GameObject (typically a button) based on tutorial completion.
/// The GameObject is hidden until the specified tutorial is completed.
/// </summary>
public class TutorialButtonUnlock : MonoBehaviour
{
    [Header("Target GameObject")]
    [Tooltip("The GameObject to control. If not assigned, will search for a GameObject with the same name as this component's GameObject.")]
    [SerializeField] private GameObject targetGameObject;

    [Header("Tutorial Check")]
    [Tooltip("The tutorial ID that must be completed to show the GameObject.")]
    [SerializeField] private string requiredTutorialId = "warrant_fusion_tutorial";

    [Header("Settings")]
    [Tooltip("If true, will check tutorial status every frame (useful for testing). Otherwise, only checks on Start and when explicitly requested.")]
    [SerializeField] private bool checkEveryFrame = false;

    private void Awake()
    {
        FindTargetGameObjectIfMissing();
    }

    private void Start()
    {
        UpdateGameObjectVisibility();
    }

    private void Update()
    {
        if (checkEveryFrame)
        {
            UpdateGameObjectVisibility();
        }
    }

    /// <summary>
    /// Finds the target GameObject if not assigned.
    /// </summary>
    private void FindTargetGameObjectIfMissing()
    {
        if (targetGameObject == null)
        {
            // Try to find by the same name as this GameObject (minus "Controller" or "Unlock" suffix)
            string searchName = gameObject.name;
            if (searchName.EndsWith("Controller") || searchName.EndsWith("Unlock"))
            {
                // Remove suffix to get the actual button/object name
                searchName = searchName.Replace("Controller", "").Replace("Unlock", "").Trim();
            }
            
            GameObject found = GameObject.Find(searchName);
            if (found != null)
            {
                targetGameObject = found;
                Debug.Log($"[TutorialButtonUnlock] Found target GameObject by name: {targetGameObject.name}");
            }
            else
            {
                // Try to find in children
                Transform child = transform.Find(searchName);
                if (child != null)
                {
                    targetGameObject = child.gameObject;
                    Debug.Log($"[TutorialButtonUnlock] Found target GameObject in children: {targetGameObject.name}");
                }
                else
                {
                    Debug.LogWarning($"[TutorialButtonUnlock] Target GameObject not found! Please assign it in the Inspector or ensure a GameObject named '{searchName}' exists in the scene.");
                }
            }
        }
    }

    /// <summary>
    /// Updates the target GameObject visibility based on tutorial completion status.
    /// </summary>
    public void UpdateGameObjectVisibility()
    {
        if (targetGameObject == null)
        {
            FindTargetGameObjectIfMissing();
            if (targetGameObject == null)
            {
                return; // Still not found, can't update
            }
        }

        bool tutorialCompleted = IsTutorialCompleted(requiredTutorialId);
        
        if (targetGameObject.activeSelf != tutorialCompleted)
        {
            targetGameObject.SetActive(tutorialCompleted);
            Debug.Log($"[TutorialButtonUnlock] Target GameObject '{targetGameObject.name}' {(tutorialCompleted ? "activated" : "deactivated")} (tutorial '{requiredTutorialId}' completed: {tutorialCompleted})");
        }
    }

    /// <summary>
    /// Checks if a specific tutorial has been completed.
    /// </summary>
    private bool IsTutorialCompleted(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
        {
            return false;
        }

        var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
        if (character != null)
        {
            return character.HasCompletedTutorial(tutorialId);
        }

        return false;
    }

    /// <summary>
    /// Manually refresh the GameObject visibility (useful to call after tutorial completion).
    /// </summary>
    public void Refresh()
    {
        UpdateGameObjectVisibility();
    }

    /// <summary>
    /// Called when the scene is enabled/loaded. Refreshes GameObject visibility.
    /// </summary>
    private void OnEnable()
    {
        // Small delay to ensure CharacterManager is ready
        Invoke(nameof(UpdateGameObjectVisibility), 0.1f);
    }
}

