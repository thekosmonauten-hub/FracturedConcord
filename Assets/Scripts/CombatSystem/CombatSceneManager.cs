using UnityEngine;
using UnityEngine.UI;
using Dexiled.MazeSystem;

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
    
    [Header("Combat Background")]
    [Tooltip("Background image component for combat scene (Unity UI Image)")]
    public UnityEngine.UI.Image combatBackgroundImage;
    [Tooltip("If true, will try to find background image automatically if not assigned")]
    public bool autoFindBackgroundImage = true;
    
    private void Start()
    {
        InitializeCombatScene();
        SetupUI();
        SetMazeCombatBackground();
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
            Debug.Log($"Combat Scene initialized for: {currentEncounter.encounterName} (ID: {currentEncounter.encounterID}) AreaLevel={currentEncounter.areaLevel}");
            
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
        
        // Check if this is a maze combat encounter
        bool isMazeCombat = PlayerPrefs.HasKey("MazeCombatContext") || PlayerPrefs.HasKey("MazeRunId");
        
        if (isMazeCombat)
        {
            // This is a maze combat - notify MazeRunManager to handle the return
            if (MazeRunManager.Instance != null)
            {
                Debug.Log("[CombatSceneManager] Maze combat victory detected. Notifying MazeRunManager...");
                MazeRunManager.Instance.OnCombatVictory();
                return; // MazeRunManager will handle scene transition
            }
            else
            {
                Debug.LogWarning("[CombatSceneManager] Maze combat detected but MazeRunManager.Instance is null!");
            }
        }
        
        // Regular encounter completion
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.CompleteCurrentEncounter();
        }
        else
        {
            Debug.LogWarning("EncounterManager.Instance is null! Cannot complete encounter.");
        }
        
        // Check if LootRewardsUI exists - if so, it will handle showing the victory panel
        // Search for inactive GameObjects too, as LootRewardsUI might be on a disabled GameObject
        LootRewardsUI lootRewardsUI = FindFirstObjectByType<LootRewardsUI>();
        if (lootRewardsUI == null)
        {
            // Try to find inactive GameObjects
            var allLootRewards = FindObjectsByType<LootRewardsUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allLootRewards != null && allLootRewards.Length > 0)
            {
                lootRewardsUI = allLootRewards[0];
                Debug.Log($"[CombatSceneManager] Found LootRewardsUI on inactive GameObject: {lootRewardsUI.gameObject.name}");
            }
        }
        
        if (lootRewardsUI != null)
        {
            Debug.Log("[CombatSceneManager] LootRewardsUI found - it will handle showing AreaClearedPanel. Hiding combat UI only.");
            // Hide combat UI, but let LootRewardsUI show the AreaClearedPanel
            if (combatUI != null) combatUI.SetActive(false);
            
            // Ensure LootRewardsUI GameObject is active so it can receive events and show the panel
            if (!lootRewardsUI.gameObject.activeInHierarchy)
            {
                lootRewardsUI.gameObject.SetActive(true);
                Debug.Log($"[CombatSceneManager] Activated LootRewardsUI GameObject: {lootRewardsUI.gameObject.name}");
            }
            
            // Call ShowAreaClearedPanel directly (don't wait for event, as it may have already fired)
            // Also set up delayed fallback in case the direct call doesn't work
            lootRewardsUI.ShowAreaClearedPanel();
            StartCoroutine(DelayedShowLootRewards(lootRewardsUI));
        }
        else
        {
            // Fallback: Show victory UI if LootRewardsUI doesn't exist
            Debug.Log("[CombatSceneManager] LootRewardsUI not found - using fallback victoryUI");
            if (combatUI != null) combatUI.SetActive(false);
            if (victoryUI != null) victoryUI.SetActive(true);
        }
    }
    
    private System.Collections.IEnumerator DelayedShowLootRewards(LootRewardsUI lootRewardsUI)
    {
        // Wait a bit longer than LootRewardsUI's showDelaySeconds (1.1s) to ensure event has time to fire
        yield return new WaitForSeconds(1.5f);
        
        // Fallback: If event subscription failed, call directly
        // This ensures the panel shows even if the event wasn't received
        if (lootRewardsUI != null && lootRewardsUI.gameObject.activeInHierarchy)
        {
            lootRewardsUI.ShowAreaClearedPanel();
            Debug.Log("[CombatSceneManager] Called ShowAreaClearedPanel() directly as fallback");
        }
        else if (lootRewardsUI == null)
        {
            Debug.LogWarning("[CombatSceneManager] LootRewardsUI became null during delay");
        }
        else
        {
            Debug.LogWarning($"[CombatSceneManager] LootRewardsUI GameObject is not active: {lootRewardsUI.gameObject.activeInHierarchy}");
        }
    }
    
    public void FailEncounter()
    {
        Debug.Log("Encounter failed!");
        
        // Show defeat UI
        if (combatUI != null) combatUI.SetActive(false);
        if (defeatUI != null) defeatUI.SetActive(true);
    }
    
    /// <summary>
    /// Sets the background for maze combat encounters.
    /// Uses the randomized background selected when entering combat.
    /// </summary>
    private void SetMazeCombatBackground()
    {
        // Check if this is a maze combat encounter
        bool isMazeCombat = PlayerPrefs.HasKey("MazeCombatContext") || PlayerPrefs.HasKey("MazeRunId");
        
        if (!isMazeCombat)
        {
            // Not a maze combat - use default background or don't change
            return;
        }
        
        // Try to find background image if not assigned
        if (combatBackgroundImage == null && autoFindBackgroundImage)
        {
            // Try to find by name
            GameObject bgObj = GameObject.Find("Background");
            if (bgObj != null)
            {
                combatBackgroundImage = bgObj.GetComponent<UnityEngine.UI.Image>();
            }
            
            // Try to find by tag
            if (combatBackgroundImage == null)
            {
                bgObj = GameObject.FindGameObjectWithTag("Background");
                if (bgObj != null)
                {
                    combatBackgroundImage = bgObj.GetComponent<UnityEngine.UI.Image>();
                }
            }
            
            // Try to find any UI Image component with "background" in name
            if (combatBackgroundImage == null)
            {
                var allImages = FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None);
                foreach (var img in allImages)
                {
                    if (img.name.ToLower().Contains("background"))
                    {
                        combatBackgroundImage = img;
                        break;
                    }
                }
            }
            
            if (combatBackgroundImage != null)
            {
                Debug.Log($"[CombatSceneManager] Auto-found combat background image: {combatBackgroundImage.name}");
            }
        }
        
        if (combatBackgroundImage == null)
        {
            Debug.LogWarning("[CombatSceneManager] No combat background image found! Maze combat backgrounds will not be applied.");
            return;
        }
        
        // Get the maze run manager to access background config
        if (MazeRunManager.Instance != null && MazeRunManager.Instance.mazeConfig != null)
        {
            // Get floor number for background selection
            int mazeFloor = PlayerPrefs.GetInt("MazeCombatFloor", 1);
            
            // Get random background for this floor
            Sprite backgroundSprite = MazeRunManager.Instance.mazeConfig.GetRandomBackgroundForFloor(mazeFloor);
            
            if (backgroundSprite != null)
            {
                combatBackgroundImage.sprite = backgroundSprite;
                Debug.Log($"[CombatSceneManager] Applied maze combat background: {backgroundSprite.name} (Floor {mazeFloor})");
            }
            else
            {
                Debug.LogWarning($"[CombatSceneManager] No background images available for floor {mazeFloor} in MazeConfig!");
            }
        }
        else
        {
            Debug.LogWarning("[CombatSceneManager] MazeRunManager or MazeConfig not available! Cannot set maze combat background.");
        }
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
