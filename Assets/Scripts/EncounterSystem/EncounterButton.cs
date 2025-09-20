using UnityEngine;
using UnityEngine.UI;

public class EncounterButton : MonoBehaviour
{
    [Header("Encounter Settings")]
    public int encounterID = 1;
    public string encounterName = "Where Night First Fell";
    
    [Header("UI References")]
    public Button button;
    public Text buttonText;
    
    private void Start()
    {
        // Get button component if not assigned
        if (button == null)
            button = GetComponent<Button>();
            
        // Get text component if not assigned
        if (buttonText == null)
            buttonText = GetComponentInChildren<Text>();
            
        // Set up button click event
        if (button != null)
        {
            button.onClick.AddListener(OnEncounterButtonClick);
        }
        
        // Update button text
        UpdateButtonText();
        
        // Update button interactability based on unlock status
        UpdateButtonState();
    }
    
    private void OnEncounterButtonClick()
    {
        Debug.Log($"Starting encounter: {encounterName} (ID: {encounterID})");
        EncounterManager.Instance.StartEncounter(encounterID);
    }
    
    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = encounterName;
        }
    }
    
    private void UpdateButtonState()
    {
        if (button != null)
        {
            EncounterData encounter = EncounterManager.Instance.GetEncounterByID(encounterID);
            if (encounter != null)
            {
                button.interactable = encounter.isUnlocked;
                
                // Visual feedback for completed encounters
                if (encounter.isCompleted)
                {
                    // You can add visual indicators here (checkmark, different color, etc.)
                    Debug.Log($"Encounter {encounterID} is completed!");
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event listener
        if (button != null)
        {
            button.onClick.RemoveListener(OnEncounterButtonClick);
        }
    }
}
