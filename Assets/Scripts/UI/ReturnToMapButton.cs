using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReturnToMapButton : MonoBehaviour
{
    private Button button;
    
    private void Start()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(ReturnToMainUI);
        }
        else
        {
            Debug.LogError("ReturnToMapButton: No Button component found on this GameObject!");
        }
    }
    
    private void ReturnToMainUI()
    {
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.ReturnToMainUI();
        }
        else
        {
            SceneManager.LoadScene("MainGameUI");
        }
    }
    
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(ReturnToMainUI);
        }
    }
}
