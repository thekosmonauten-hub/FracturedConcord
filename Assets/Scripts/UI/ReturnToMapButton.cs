using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReturnToMapButton : MonoBehaviour
{
    private Button button;
    
    private void Start()
    {
        // Get the button component
        button = GetComponent<Button>();
        
        if (button != null)
        {
            // Add click listener
            button.onClick.AddListener(ReturnToMainUI);
        }
        else
        {
            Debug.LogError("ReturnToMapButton: No Button component found on this GameObject!");
        }
    }
    
    private void ReturnToMainUI()
    {
        Debug.Log("ReturnToMap button clicked - returning to MainGameUI");
        
        // Use EncounterManager if available, otherwise use SceneManager directly
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.ReturnToMainUI();
        }
        else
        {
            // Fallback to direct scene loading
            SceneManager.LoadScene("MainGameUI");
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event listener
        if (button != null)
        {
            button.onClick.RemoveListener(ReturnToMainUI);
        }
    }
}
