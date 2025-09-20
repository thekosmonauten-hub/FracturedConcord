using UnityEngine;
using UnityEngine.UI;

public class CombatSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public Text encounterNameText;
    public Text encounterIDText;
    public Button returnToMainButton;
    public Button returnToMapButton; // Add this for your new button
    
    [Header("Combat UI")]
    public GameObject combatUI;
    public GameObject victoryUI;
    public GameObject defeatUI;
    
    private void Start()
    {
        InitializeCombatScene();
        SetupUI();
    }
    
    private void InitializeCombatScene()
    {
        // Check if EncounterManager exists
        if (EncounterManager.Instance == null)
        {
            Debug.LogError("EncounterManager.Instance is null! Creating a test encounter for development.");
            
            // For development/testing, create a simple test encounter
            CreateTestEncounter();
            return;
        }
        
        // Get current encounter data
        EncounterData currentEncounter = EncounterManager.Instance.GetCurrentEncounter();
        
        if (currentEncounter != null)
        {
            Debug.Log($"Combat Scene initialized for: {currentEncounter.encounterName} (ID: {currentEncounter.encounterID})");
            
            // Update UI with encounter info
            if (encounterNameText != null)
                encounterNameText.text = currentEncounter.encounterName;
                
            if (encounterIDText != null)
                encounterIDText.text = $"Encounter {currentEncounter.encounterID}";
        }
        else
        {
            Debug.LogWarning("No current encounter found! Creating a test encounter for development.");
            CreateTestEncounter();
        }
    }
    
    private void CreateTestEncounter()
    {
        // Create a simple test encounter for development
        Debug.Log("Creating test encounter for development...");
        
        if (encounterNameText != null)
            encounterNameText.text = "Test Encounter";
            
        if (encounterIDText != null)
            encounterIDText.text = "Encounter 001";
    }
    
    private void SetupUI()
    {
        // Set up return buttons
        if (returnToMainButton != null)
        {
            returnToMainButton.onClick.AddListener(ReturnToMainUI);
        }
        
        // Set up return to map button
        if (returnToMapButton != null)
        {
            returnToMapButton.onClick.AddListener(ReturnToMainUI);
        }
        
        // Initialize UI states
        if (combatUI != null) combatUI.SetActive(true);
        if (victoryUI != null) victoryUI.SetActive(false);
        if (defeatUI != null) defeatUI.SetActive(false);
    }
    
    public void ReturnToMainUI()
    {
        Debug.Log("Returning to Main Game UI");
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.ReturnToMainUI();
        }
        else
        {
            Debug.LogWarning("EncounterManager.Instance is null! Cannot return to main UI.");
        }
    }
    
    public void ReturnToMap()
    {
        Debug.Log("Returning to Map");
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.ReturnToMainUI(); // For now, this goes to main UI, but could be changed to go to map
        }
        else
        {
            Debug.LogWarning("EncounterManager.Instance is null! Cannot return to map.");
        }
    }
    
    public void CompleteEncounter()
    {
        Debug.Log("Encounter completed!");
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.CompleteCurrentEncounter();
        }
        else
        {
            Debug.LogWarning("EncounterManager.Instance is null! Cannot complete encounter.");
        }
        
        // Show victory UI
        if (combatUI != null) combatUI.SetActive(false);
        if (victoryUI != null) victoryUI.SetActive(true);
    }
    
    public void FailEncounter()
    {
        Debug.Log("Encounter failed!");
        
        // Show defeat UI
        if (combatUI != null) combatUI.SetActive(false);
        if (defeatUI != null) defeatUI.SetActive(true);
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (returnToMainButton != null)
        {
            returnToMainButton.onClick.RemoveListener(ReturnToMainUI);
        }
        
        if (returnToMapButton != null)
        {
            returnToMapButton.onClick.RemoveListener(ReturnToMainUI);
        }
    }
}
