using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Simple component to attach to buttons for scene transitions with curtain effect.
/// Just assign the target scene name and it will handle the transition when clicked.
/// </summary>
[RequireComponent(typeof(Button))]
public class SceneTransitionButton : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("Name of the scene to transition to (must be in Build Settings)")]
    [SerializeField] private string targetSceneName;
    
    [Tooltip("Use curtain transition (black out effect). If false, uses fade transition.")]
    [SerializeField] private bool useCurtainTransition = true;
    
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogError($"[SceneTransitionButton] No Button component found on {gameObject.name}. This component requires a Button.");
        }
    }
    
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
    
    /// <summary>
    /// Called when the button is clicked
    /// </summary>
    private void OnButtonClicked()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"[SceneTransitionButton] Target scene name is empty on button '{gameObject.name}'. Cannot transition.");
            return;
        }
        
        Debug.Log($"[SceneTransitionButton] Button '{gameObject.name}' clicked - transitioning to scene '{targetSceneName}' (curtain: {useCurtainTransition})");
        
        // Get or find TransitionManager
        TransitionManager transitionManager = TransitionManager.Instance;
        if (transitionManager == null)
        {
            Debug.LogWarning($"[SceneTransitionButton] TransitionManager not found. Loading scene '{targetSceneName}' directly without transition.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            return;
        }
        
        // Use curtain transition if requested, otherwise use regular fade transition
        if (useCurtainTransition)
        {
            transitionManager.TransitionToSceneWithCurtain(targetSceneName);
        }
        else
        {
            transitionManager.TransitionToScene(targetSceneName);
        }
    }
    
    /// <summary>
    /// Set the target scene name programmatically
    /// </summary>
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }
    
    /// <summary>
    /// Get the target scene name
    /// </summary>
    public string GetTargetScene()
    {
        return targetSceneName;
    }
    
    /// <summary>
    /// Fallback async scene loading if TransitionManager is unavailable
    /// </summary>
    private IEnumerator LoadSceneAsyncFallback(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        // Wait for scene to load
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        // Activate the scene
        asyncLoad.allowSceneActivation = true;
    }
}

