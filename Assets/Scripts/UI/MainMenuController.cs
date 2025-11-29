using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Canvas-based MainMenu controller for the new UI structure.
/// Manages main menu buttons, character slot panel, and scene transitions.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Center Panel Buttons")]
    [SerializeField] private Button mainContinueButton;
    [SerializeField] private Button startJourneyButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Character Slot Panel")]
    [SerializeField] private RectTransform characterSlotPanel;
    [SerializeField] private Transform characterSlotContainer;
    [SerializeField] private Button sidebarToggleButton;
    [SerializeField] private RectTransform sidebarToggleButtonRect; // RectTransform of the toggle button for animation

    [Header("Character Slot Prefab")]
    [SerializeField] private GameObject characterSlotPrefab;
    
    [Header("Scene Names")]
    [SerializeField] private string characterCreationSceneName = "CharacterCreation";
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Animation Settings")]
    [SerializeField] private float panelAnimDuration = 0.3f;
    [SerializeField] private Ease panelAnimEase = Ease.OutCubic;
    [SerializeField] private float panelClosedX = 600f; // Panel X position when closed (off-screen right)
    [SerializeField] private float panelOpenX = -600f; // Panel X position when open (fully visible - NEGATIVE pulls it left)
    [SerializeField] private float toggleButtonClosedX = -20f; // Toggle button X position when panel closed
    [SerializeField] private float toggleButtonOpenX = -620f; // Toggle button X position when panel open (moves with panel)

    // State
    private bool isPanelOpen = false;
    private List<GameObject> characterSlotInstances = new List<GameObject>();
    private CharacterData selectedCharacter = null;

    // References (auto-found)
    private CharacterManager characterManager;
    private CharacterSaveSystem characterSaveSystem;
    private TransitionManager transitionManager;
    private VideoTransitionManager videoTransitionManager;

    void Start()
    {
        // Find existing managers in scene
        characterManager = FindObjectOfType<CharacterManager>();
        characterSaveSystem = FindObjectOfType<CharacterSaveSystem>();
        transitionManager = FindObjectOfType<TransitionManager>();
        videoTransitionManager = FindObjectOfType<VideoTransitionManager>();

        if (characterManager == null)
            Debug.LogError("[MainMenu] CharacterManager not found in scene!");
        if (characterSaveSystem == null)
            Debug.LogError("[MainMenu] CharacterSaveSystem not found in scene!");
        if (videoTransitionManager == null)
            Debug.LogWarning("[MainMenu] VideoTransitionManager not found - using standard transitions");

        // Auto-assign toggle button RectTransform if not set
        if (sidebarToggleButton != null && sidebarToggleButtonRect == null)
        {
            sidebarToggleButtonRect = sidebarToggleButton.GetComponent<RectTransform>();
        }

        // Setup button listeners
        SetupButtons();

        // Load characters
        RefreshCharacterList();

        // Initialize panel (closed)
        InitializePanel();
        
        Debug.Log("[MainMenu] Initialization complete!");
    }

    void SetupButtons()
    {
        // Center Panel Buttons
        if (mainContinueButton != null)
        {
            mainContinueButton.onClick.AddListener(OnMainContinueClicked);
            Debug.Log("[MainMenu] Main Continue Button listener added");
        }
        else
        {
            Debug.LogError("[MainMenu] Main Continue Button is NULL! Assign it in Inspector.");
        }
        
        if (startJourneyButton != null)
        {
            startJourneyButton.onClick.AddListener(OnStartJourneyClicked);
            Debug.Log("[MainMenu] Start Journey Button listener added");
        }
        else
        {
            Debug.LogError("[MainMenu] Start Journey Button is NULL! Assign it in Inspector.");
        }
        
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
            Debug.Log("[MainMenu] Settings Button listener added");
        }
        else
        {
            Debug.LogError("[MainMenu] Settings Button is NULL! Assign it in Inspector.");
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
            Debug.Log("[MainMenu] Exit Button listener added");
        }
        else
        {
            Debug.LogError("[MainMenu] Exit Button is NULL! Assign it in Inspector.");
        }
        
        // Sidebar Toggle Button
        if (sidebarToggleButton != null)
        {
            sidebarToggleButton.onClick.AddListener(ToggleCharacterPanel);
            Debug.Log("[MainMenu] Sidebar Toggle Button listener added");
        }
        else
        {
            Debug.LogError("[MainMenu] Sidebar Toggle Button is NULL! Assign it in Inspector.");
        }
    }

    void InitializePanel()
    {
        if (characterSlotPanel == null)
        {
            Debug.LogError("[MainMenu] CharacterSlotPanel is NULL!");
            return;
        }

        // Position panel off-screen to the right
        Vector2 startPos = characterSlotPanel.anchoredPosition;
        startPos.x = panelClosedX;
        characterSlotPanel.anchoredPosition = startPos;
        
        // Position toggle button at closed position
        if (sidebarToggleButtonRect != null)
        {
            Vector2 togglePos = sidebarToggleButtonRect.anchoredPosition;
            togglePos.x = toggleButtonClosedX;
            sidebarToggleButtonRect.anchoredPosition = togglePos;
        }
        
        isPanelOpen = false;
        Debug.Log($"[MainMenu] Panel initialized at X: {panelClosedX} (closed)");
    }

    #region Button Handlers

    void OnMainContinueClicked()
    {
        Debug.Log("<color=cyan>━━━ [MainMenu] Main Continue Button CLICKED ━━━</color>");
        
        // Load most recent character
        if (characterSaveSystem != null)
        {
            var characters = characterSaveSystem.LoadCharacters();
            Debug.Log($"[MainMenu] Found {characters.Count} saved characters");
            
            if (characters.Count > 0)
            {
                Debug.Log($"[MainMenu] Loading most recent character: {characters[0].characterName}");
                LoadCharacter(characters[0], useVideoTransition:false);
            }
            else
            {
                Debug.LogWarning("[MainMenu] No characters found! Opening character selection panel.");
                ToggleCharacterPanel(); // Open character selection
            }
        }
        else
        {
            Debug.LogError("[MainMenu] CharacterSaveSystem is NULL! Cannot load characters.");
        }
    }

    void OnStartJourneyClicked()
    {
        Debug.Log("<color=cyan>━━━ [MainMenu] Start Journey Button CLICKED ━━━</color>");
        Debug.Log("[MainMenu] Start Journey launches character creation.");
        GoToCharacterCreation();
    }

    void OnSettingsClicked()
    {
        Debug.Log("[MainMenu] Settings clicked (not implemented)");
        // TODO: Open settings menu
    }

    void OnExitClicked()
    {
        Debug.Log("[MainMenu] Exit clicked");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    #endregion

    #region Character Management

    void RefreshCharacterList()
    {
        // Clear existing slots
        foreach (var slot in characterSlotInstances)
        {
            if (slot != null) Destroy(slot);
        }
        characterSlotInstances.Clear();

        if (characterSaveSystem == null || characterSlotContainer == null)
        {
            Debug.LogWarning("[MainMenu] CharacterSaveSystem or CharacterSlotContainer is null!");
            return;
        }

        // Get all saved characters
        List<CharacterData> characters = characterSaveSystem.LoadCharacters();
        
        Debug.Log($"[MainMenu] Loading {characters.Count} characters");

        // Create slot for each character
        foreach (var character in characters)
        {
            CreateCharacterSlot(character);
        }

        // Update main continue button state
        if (mainContinueButton != null)
        {
            mainContinueButton.interactable = characters.Count > 0;
        }
    }

    void CreateCharacterSlot(CharacterData character)
    {
        if (characterSlotPrefab == null)
        {
            Debug.LogError("[MainMenu] Character slot prefab not assigned!");
            return;
        }

        GameObject slotObj = Instantiate(characterSlotPrefab, characterSlotContainer);
        characterSlotInstances.Add(slotObj);

        // Get references to slot components
        var characterNameText = slotObj.transform.Find("CharacterName")?.GetComponent<TextMeshProUGUI>();
        var characterLevelText = slotObj.transform.Find("CharacterLevel")?.GetComponent<TextMeshProUGUI>();
        var characterProgressionText = slotObj.transform.Find("CharacterProgression")?.GetComponent<TextMeshProUGUI>();
        var continueButton = slotObj.transform.Find("ContinueButton")?.GetComponent<Button>();
        var deleteButton = slotObj.transform.Find("DeleteButton")?.GetComponent<Button>();

        // Set text values
        if (characterNameText != null)
            characterNameText.text = character.characterName;
        
        if (characterLevelText != null)
            characterLevelText.text = $"Level {character.level}";
        
        if (characterProgressionText != null)
            characterProgressionText.text = $"{character.characterClass} - Act {character.act}";

        // Setup button listeners
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(() => OnCharacterContinueClicked(character));
        }
        else
        {
            Debug.LogWarning($"[MainMenu] ContinueButton not found on slot for {character.characterName}");
        }
        
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(() => OnCharacterDeleteClicked(character));
        }
        else
        {
            Debug.LogWarning($"[MainMenu] DeleteButton not found on slot for {character.characterName}");
        }
    }

    void OnCharacterContinueClicked(CharacterData character)
    {
        Debug.Log($"[MainMenu] Continue clicked for character: {character.characterName}");
        LoadCharacter(character);
    }

    void OnCharacterDeleteClicked(CharacterData character)
    {
        Debug.Log($"[MainMenu] Delete clicked for character: {character.characterName}");
        
        // TODO: Add confirmation dialog
        DeleteCharacter(character);
    }

    void LoadCharacter(CharacterData character, bool useVideoTransition = true)
    {
        Debug.Log($"<color=lime>[MainMenu] Loading character: {character.characterName}</color>");
        
        if (characterManager != null)
        {
            Debug.Log($"[MainMenu] Calling CharacterManager.LoadCharacter({character.characterName})");
            characterManager.LoadCharacter(character.characterName);
            
            Debug.Log($"[MainMenu] Character loaded, now transitioning to scene: {gameSceneName}");
            if (!useVideoTransition && transitionManager != null)
            {
                transitionManager.TransitionToSceneWithCurtain(gameSceneName);
            }
            else
            {
                LoadScene(gameSceneName);
            }
        }
        else
        {
            Debug.LogError("[MainMenu] Cannot load character - CharacterManager is null!");
        }
    }

    void DeleteCharacter(CharacterData character)
    {
        if (characterSaveSystem != null)
        {
            characterSaveSystem.DeleteCharacter(character.characterName);
            RefreshCharacterList();
            Debug.Log($"[MainMenu] Character {character.characterName} deleted");
        }
        else
        {
            Debug.LogError("[MainMenu] Cannot delete character - CharacterSaveSystem is null!");
        }
    }

    #endregion

    #region Panel Animation

    void ToggleCharacterPanel()
    {
        isPanelOpen = !isPanelOpen;
        AnimatePanel(isPanelOpen);
    }

    void AnimatePanel(bool open)
    {
        if (characterSlotPanel == null)
        {
            Debug.LogWarning("[MainMenu] CharacterSlotPanel is null!");
            return;
        }

        float panelTargetX = open ? panelOpenX : panelClosedX; // panelOpenX (negative) pulls panel left to show it fully
        float toggleTargetX = open ? toggleButtonOpenX : toggleButtonClosedX;

        Debug.Log($"[MainMenu] Animating panel to: {(open ? "OPEN" : "CLOSED")} (Panel X: {panelTargetX}, Toggle X: {toggleTargetX})");

        // Animate panel with DOTween
        characterSlotPanel.DOAnchorPosX(panelTargetX, panelAnimDuration)
            .SetEase(panelAnimEase)
            .OnComplete(() => {
                Debug.Log($"[MainMenu] Panel animation complete. Open: {open}, Final X: {characterSlotPanel.anchoredPosition.x}");
            });

        // Animate toggle button along with the panel
        if (sidebarToggleButtonRect != null)
        {
            sidebarToggleButtonRect.DOAnchorPosX(toggleTargetX, panelAnimDuration)
                .SetEase(panelAnimEase);
            
            Debug.Log($"[MainMenu] Animating toggle button to X: {toggleTargetX}");
        }
        else
        {
            Debug.LogWarning("[MainMenu] SidebarToggleButtonRect is null! Cannot animate toggle button.");
        }
    }

    /// <summary>
    /// Public method to close the panel (can be called from UI button)
    /// </summary>
    public void CloseCharacterPanel()
    {
        if (isPanelOpen)
        {
            ToggleCharacterPanel();
        }
    }

    /// <summary>
    /// Public method to open the panel (can be called from UI button)
    /// </summary>
    public void OpenCharacterPanel()
    {
        if (!isPanelOpen)
        {
            ToggleCharacterPanel();
        }
    }

    #endregion

    #region Scene Loading

    void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[MainMenu] Scene name is NULL or EMPTY! Cannot load scene.");
            return;
        }

        Debug.Log($"<color=yellow>[MainMenu] Attempting to load scene: {sceneName}</color>");
        
        // Check if scene exists in build settings
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
        if (sceneIndex < 0)
        {
            // Try finding by name
            bool sceneExists = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneNameFromPath == sceneName)
                {
                    sceneExists = true;
                    sceneIndex = i;
                    break;
                }
            }
            
            if (!sceneExists)
            {
                Debug.LogError($"[MainMenu] Scene '{sceneName}' NOT FOUND in build settings! Available scenes:");
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    Debug.Log($"  - {sceneNameFromPath}");
                }
                return;
            }
        }

        // Priority: VideoTransition → TransitionManager → Direct Load
        // Only use VideoTransitionManager for character creation; skip for direct gameplay scenes to avoid looping video
        if (!string.Equals(sceneName, characterCreationSceneName, System.StringComparison.OrdinalIgnoreCase) && transitionManager != null)
        {
            Debug.Log($"[MainMenu] Loading scene {sceneName} via TransitionManager curtain");
            transitionManager.TransitionToSceneWithCurtain(sceneName);
            return;
        }
        
        if (videoTransitionManager != null)
        {
            Debug.Log($"[MainMenu] Loading scene {sceneName} with VIDEO transition");
            videoTransitionManager.PlayTransitionAndLoadScene(sceneName);
        }
        else if (transitionManager != null)
        {
            Debug.Log($"[MainMenu] Loading scene {sceneName} via TransitionManager");
            transitionManager.TransitionToScene(sceneName);
        }
        else
        {
            Debug.Log($"[MainMenu] No transition managers found, loading scene {sceneName} directly");
            SceneManager.LoadScene(sceneName);
        }
    }

    #endregion

    #region Public Helper Methods

    /// <summary>
    /// Force refresh the character list (useful after creating a new character)
    /// </summary>
    public void ForceRefreshCharacterList()
    {
        RefreshCharacterList();
    }

    /// <summary>
    /// Navigate to Character Creation scene
    /// </summary>
    public void GoToCharacterCreation()
    {
        LoadScene(characterCreationSceneName);
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Debug: Print Character Count")]
    void DebugPrintCharacterCount()
    {
        if (characterSaveSystem != null)
        {
            var characters = characterSaveSystem.LoadCharacters();
            Debug.Log($"[MainMenu Debug] Found {characters.Count} saved characters");
        foreach (var character in characters)
            {
                Debug.Log($"  - {character.characterName} (Level {character.level} {character.characterClass})");
            }
        }
        else
        {
            Debug.LogError("[MainMenu Debug] CharacterSaveSystem is null!");
        }
    }

    [ContextMenu("Debug: Toggle Panel")]
    void DebugTogglePanel()
    {
        ToggleCharacterPanel();
    }

    #endregion
}
