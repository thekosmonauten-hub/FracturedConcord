using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Improved scene loader that uses additive loading and deferred initialization.
/// Based on Unity Scene Loading best practices.
/// </summary>
public static class ImprovedSceneLoader
{
    /// <summary>
    /// Load a scene additively with deferred initialization.
    /// This prevents blocking and allows smooth transitions.
    /// </summary>
    public static IEnumerator LoadSceneAdditive(string sceneName, bool setAsActive = true)
    {
        Debug.Log($"[ImprovedSceneLoader] Loading scene additively: {sceneName}");
        
        // Load scene additively (doesn't unload current scene)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;
        
        // Wait for scene to load
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        // Activate the scene
        asyncLoad.allowSceneActivation = true;
        
        // Wait for scene to fully activate
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Get the loaded scene
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (!loadedScene.IsValid())
        {
            Debug.LogError($"[ImprovedSceneLoader] Failed to load scene: {sceneName}");
            yield break;
        }
        
        // Set as active scene if requested
        if (setAsActive)
        {
            SceneManager.SetActiveScene(loadedScene);
        }
        
        // Initialize the scene using SceneInitializationManager
        if (SceneInitializationManager.Instance != null)
        {
            SceneInitializationManager.Instance.InitializeScene(loadedScene);
        }
        
        Debug.Log($"[ImprovedSceneLoader] ✅ Scene loaded and initialized: {sceneName}");
    }
    
    /// <summary>
    /// Unload a scene asynchronously.
    /// </summary>
    public static IEnumerator UnloadSceneAsync(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid())
        {
            Debug.LogWarning($"[ImprovedSceneLoader] Scene not found for unloading: {sceneName}");
            yield break;
        }
        
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(scene);
        
        while (!asyncUnload.isDone)
        {
            yield return null;
        }
        
        Debug.Log($"[ImprovedSceneLoader] ✅ Scene unloaded: {sceneName}");
    }
    
    /// <summary>
    /// Transition from one scene to another using additive loading.
    /// Unloads the old scene after the new one is loaded.
    /// </summary>
    public static IEnumerator TransitionScenes(string fromSceneName, string toSceneName)
    {
        // Load new scene additively
        yield return LoadSceneAdditive(toSceneName, true);
        
        // Wait a frame to ensure new scene is ready
        yield return null;
        
        // Unload old scene
        if (!string.IsNullOrEmpty(fromSceneName))
        {
            yield return UnloadSceneAsync(fromSceneName);
        }
        
        Debug.Log($"[ImprovedSceneLoader] ✅ Transitioned from {fromSceneName} to {toSceneName}");
    }
}

