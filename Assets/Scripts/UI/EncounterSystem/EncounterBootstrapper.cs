using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterBootstrapper : MonoBehaviour
{
    [Tooltip("Only auto-initialize in this scene.")]
    public string targetSceneName = "MainGameUI";

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == targetSceneName)
        {
            StartCoroutine(EnsureEncounterInit());
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == targetSceneName)
        {
            StartCoroutine(EnsureEncounterInit());
        }
    }

    private System.Collections.IEnumerator EnsureEncounterInit()
    {
        yield return null;
        var manager = EncounterManager.Instance;
        if (manager != null)
            manager.EnsureInitialized();
    }
}
