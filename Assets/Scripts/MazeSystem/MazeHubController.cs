using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using Dexiled.Data.Items;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Main controller for the Maze Hub - a centralized location for maze-related activities.
    /// Handles navigation between different hub panels (Start Run, Vendor, Forge, etc.)
    /// </summary>
    public class MazeHubController : MonoBehaviour
    {
        [Header("Scene Names")]
        [Tooltip("Name of the maze scene to load when starting a run")]
        public string mazeSceneName = "MazeScene";
        
        [Header("UI Panels")]
        [Tooltip("Main hub panel with navigation buttons")]
        public GameObject mainHubPanel;
        
        [Tooltip("Start run panel with difficulty selection")]
        public GameObject startRunPanel;
        
        [Tooltip("Vendor/shop panel")]
        public GameObject vendorPanel;
        
        [Tooltip("Forge panel")]
        public GameObject forgePanel;
        
        [Tooltip("Currency display panel")]
        public GameObject currencyPanel;
        
        [Tooltip("Statistics/progress panel")]
        public GameObject statisticsPanel;
        
        [Header("Navigation Buttons")]
        public Button startRunButton;
        public Button vendorButton;
        public Button forgeButton;
        public Button statisticsButton;
        public Button exitButton;
        
        [Header("Dialogue (Optional)")]
        [Tooltip("Dialogue to play before opening vendor. If null, vendor opens directly.")]
        public DialogueData vendorDialogue;
        
        [Tooltip("If true, dialogue will play before opening vendor. If false or dialogue is null, vendor opens directly.")]
        public bool useVendorDialogue = true;
        
        [Header("Back Buttons")]
        public Button backToHubButton;
        
        [Header("Deadzone/Backdrop")]
        [Tooltip("Deadzone backdrop panel that appears behind overlay panels. Created automatically if not assigned.")]
        public GameObject deadzoneBackdrop;
        
        [Tooltip("Color of the deadzone backdrop (semi-transparent overlay)")]
        public Color deadzoneColor = new Color(0, 0, 0, 0.5f);
        
        [Header("Difficulty Selection")]
        [Tooltip("Container for difficulty option buttons")]
        public Transform difficultyContainer;
        
        [Tooltip("Prefab for difficulty selection button")]
        public GameObject difficultyButtonPrefab;
        
        [Tooltip("List of available difficulty configurations")]
        public List<MazeDifficultyConfig> availableDifficulties = new List<MazeDifficultyConfig>();
        
        [Header("Currency Display")]
        [Tooltip("Text displaying current maze currency")]
        public TextMeshProUGUI mazeCurrencyText;
        
        [Tooltip("Currency type used for maze activities")]
        public CurrencyType mazeCurrencyType = CurrencyType.OrbOfGeneration; // Default, can be customized
        
        private MazeRunManager mazeRunManager;
        private MazeCurrencyManager currencyManager;
        private GameObject currentOverlayPanel; // Track which overlay is currently open
        
        private void Awake()
        {
            mazeRunManager = MazeRunManager.Instance;
            if (mazeRunManager == null)
            {
                Debug.LogError("[MazeHubController] MazeRunManager.Instance is null! Maze hub will not function correctly.");
            }
            
            // Try to find or create currency manager
            currencyManager = FindFirstObjectByType<MazeCurrencyManager>();
            if (currencyManager == null)
            {
                GameObject currencyObj = new GameObject("MazeCurrencyManager");
                currencyManager = currencyObj.AddComponent<MazeCurrencyManager>();
            }
        }
        
        private void Start()
        {
            InitializeHub();
            SetupNavigation();
            SetupDifficultySelection();
            UpdateCurrencyDisplay();
        }
        
        private void InitializeHub()
        {
            // Main hub panel is always active
            if (mainHubPanel != null) mainHubPanel.SetActive(true);
            
            // Overlay panels start hidden
            if (startRunPanel != null) startRunPanel.SetActive(false);
            if (vendorPanel != null) vendorPanel.SetActive(false);
            if (forgePanel != null) forgePanel.SetActive(false);
            if (statisticsPanel != null) statisticsPanel.SetActive(false);
            
            // Deadzone backdrop starts hidden
            if (deadzoneBackdrop != null) deadzoneBackdrop.SetActive(false);
            
            currentOverlayPanel = null;
        }
        
        private void SetupNavigation()
        {
            // Main hub buttons
            if (startRunButton != null)
                startRunButton.onClick.AddListener(() => ShowPanel(startRunPanel));
            
            if (vendorButton != null)
                vendorButton.onClick.AddListener(OnVendorButtonClicked);
            
            if (forgeButton != null)
                forgeButton.onClick.AddListener(() => ShowPanel(forgePanel));
            
            if (statisticsButton != null)
                statisticsButton.onClick.AddListener(() => ShowPanel(statisticsPanel));
            
            if (exitButton != null)
                exitButton.onClick.AddListener(ExitToMainMenu);
            
            // Back button closes overlay
            if (backToHubButton != null)
                backToHubButton.onClick.AddListener(CloseCurrentOverlay);
        }
        
        /// <summary>
        /// Handles vendor button click - shows dialogue first if enabled, then opens vendor panel.
        /// </summary>
        private void OnVendorButtonClicked()
        {
            // Check if we should show dialogue first
            if (useVendorDialogue && vendorDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(vendorDialogue);
            }
            else
            {
                // Open vendor directly
                ShowPanel(vendorPanel);
            }
        }
        
        /// <summary>
        /// Shows an overlay panel on top of the main hub panel.
        /// Creates and shows the deadzone backdrop if needed.
        /// Public so DialogueManager can open the vendor panel.
        /// </summary>
        public void ShowPanel(GameObject panel)
        {
            // Don't show main hub panel as an overlay (it's always visible)
            if (panel == mainHubPanel)
            {
                CloseCurrentOverlay();
                return;
            }
            
            // Close any existing overlay first
            CloseCurrentOverlay();
            
            // Show the requested overlay panel
            if (panel != null)
            {
                panel.SetActive(true);
                currentOverlayPanel = panel;
                
                // Show deadzone backdrop
                ShowDeadzoneBackdrop();
            }
        }
        
        /// <summary>
        /// Closes the current overlay panel and hides the deadzone backdrop.
        /// </summary>
        public void CloseCurrentOverlay()
        {
            if (currentOverlayPanel != null)
            {
                currentOverlayPanel.SetActive(false);
                currentOverlayPanel = null;
            }
            
            // Hide deadzone backdrop
            HideDeadzoneBackdrop();
        }
        
        /// <summary>
        /// Creates the deadzone backdrop if it doesn't exist, then shows it.
        /// </summary>
        private void ShowDeadzoneBackdrop()
        {
            // Create deadzone if it doesn't exist
            if (deadzoneBackdrop == null)
            {
                CreateDeadzoneBackdrop();
            }
            
            if (deadzoneBackdrop != null)
            {
                deadzoneBackdrop.SetActive(true);
                // Ensure overlay panel is on top, then deadzone, then main hub
                // First set deadzone position
                deadzoneBackdrop.transform.SetAsLastSibling();
                // Then put overlay on top of deadzone
                if (currentOverlayPanel != null)
                {
                    currentOverlayPanel.transform.SetAsLastSibling();
                }
            }
        }
        
        /// <summary>
        /// Hides the deadzone backdrop.
        /// </summary>
        private void HideDeadzoneBackdrop()
        {
            if (deadzoneBackdrop != null)
            {
                deadzoneBackdrop.SetActive(false);
            }
        }
        
        /// <summary>
        /// Creates a deadzone backdrop panel that covers the screen and closes overlays when clicked.
        /// </summary>
        private void CreateDeadzoneBackdrop()
        {
            // Find the Canvas to parent the deadzone to
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }
            
            if (canvas == null)
            {
                Debug.LogError("[MazeHubController] Cannot create deadzone backdrop: No Canvas found!");
                return;
            }
            
            // Create deadzone GameObject
            GameObject deadzoneObj = new GameObject("DeadzoneBackdrop");
            deadzoneObj.transform.SetParent(canvas.transform, false);
            
            // Add Image component for visual backdrop
            Image backdropImage = deadzoneObj.AddComponent<Image>();
            backdropImage.color = deadzoneColor;
            
            // Add Button component for click handling
            Button backdropButton = deadzoneObj.AddComponent<Button>();
            backdropButton.onClick.AddListener(CloseCurrentOverlay);
            
            // Set up RectTransform to cover full screen
            RectTransform rectTransform = deadzoneObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Deadzone will be positioned behind overlay panels when shown
            // (overlay panels will be set as last sibling when opened)
            
            deadzoneBackdrop = deadzoneObj;
            
            Debug.Log("[MazeHubController] Created deadzone backdrop panel");
        }
        
        private void SetupDifficultySelection()
        {
            if (difficultyContainer == null || difficultyButtonPrefab == null)
            {
                Debug.LogWarning("[MazeHubController] Difficulty container or prefab not assigned. Difficulty selection will not work.");
                return;
            }
            
            // Clear existing buttons
            foreach (Transform child in difficultyContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create buttons for each difficulty
            foreach (MazeDifficultyConfig difficulty in availableDifficulties)
            {
                if (difficulty == null) continue;
                
                GameObject buttonObj = Instantiate(difficultyButtonPrefab, difficultyContainer);
                
                // Configure RectTransform for Layout Group compatibility
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    // Reset anchors and pivot for proper layout behavior
                    buttonRect.anchorMin = new Vector2(0, 0.5f);
                    buttonRect.anchorMax = new Vector2(0, 0.5f);
                    buttonRect.pivot = new Vector2(0, 0.5f);
                    
                    // Set initial size (layout group will handle spacing)
                    // If prefab has a preferred size, keep it; otherwise set a default
                    if (buttonRect.sizeDelta == Vector2.zero)
                    {
                        buttonRect.sizeDelta = new Vector2(200, 100); // Default size
                    }
                    
                    // Reset position - layout group will position it
                    buttonRect.anchoredPosition = Vector2.zero;
                }
                
                // Set up button
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    // Store difficulty reference
                    MazeDifficultyConfig difficultyRef = difficulty;
                    button.onClick.AddListener(() => StartMazeRun(difficultyRef));
                }
                
                // Try to set text/image from prefab children
                TextMeshProUGUI nameText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = difficulty.difficultyName;
                }
                
                Image iconImage = buttonObj.GetComponentInChildren<Image>();
                if (iconImage != null && difficulty.difficultyIcon != null)
                {
                    iconImage.sprite = difficulty.difficultyIcon;
                }
                
                // Add difficulty info component if needed
                MazeDifficultyButton difficultyButton = buttonObj.GetComponent<MazeDifficultyButton>();
                if (difficultyButton == null)
                {
                    difficultyButton = buttonObj.AddComponent<MazeDifficultyButton>();
                }
                difficultyButton.SetDifficulty(difficulty);
            }
            
            // Force layout rebuild to ensure buttons are positioned correctly
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(difficultyContainer as RectTransform);
            
            // Also try to get the HorizontalLayoutGroup and force update
            HorizontalLayoutGroup layoutGroup = difficultyContainer.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.enabled = false;
                layoutGroup.enabled = true;
            }
        }
        
        /// <summary>
        /// Starts a maze run with the selected difficulty configuration.
        /// </summary>
        public void StartMazeRun(MazeDifficultyConfig difficulty)
        {
            if (difficulty == null)
            {
                Debug.LogError("[MazeHubController] Cannot start maze run: difficulty is null!");
                return;
            }
            
            if (mazeRunManager == null)
            {
                Debug.LogError("[MazeHubController] Cannot start maze run: MazeRunManager is null!");
                return;
            }
            
            // Check entry requirements
            if (!CanStartDifficulty(difficulty))
            {
                Debug.LogWarning($"[MazeHubController] Cannot start difficulty {difficulty.difficultyName}: requirements not met!");
                return;
            }
            
            // Check entry cost
            if (difficulty.entryCost > 0)
            {
                if (currencyManager == null || !currencyManager.HasCurrency(difficulty.entryCurrencyType, difficulty.entryCost))
                {
                    Debug.LogWarning($"[MazeHubController] Cannot start maze run: insufficient currency! Required: {difficulty.entryCost} {difficulty.entryCurrencyType}");
                    // TODO: Show error message to player
                    return;
                }
                
                // Deduct entry cost
                currencyManager.SpendCurrency(difficulty.entryCurrencyType, difficulty.entryCost);
                UpdateCurrencyDisplay();
            }
            
            // Set difficulty config in MazeRunManager
            if (difficulty.mazeConfig != null)
            {
                mazeRunManager.mazeConfig = difficulty.mazeConfig;
            }
            
            // Store difficulty modifiers for the run
            PlayerPrefs.SetFloat("MazeDifficulty_EnemyMultiplier", difficulty.enemyLevelMultiplier);
            PlayerPrefs.SetFloat("MazeDifficulty_ExperienceMultiplier", difficulty.experienceMultiplier);
            PlayerPrefs.SetFloat("MazeDifficulty_LootMultiplier", difficulty.lootMultiplier);
            PlayerPrefs.SetFloat("MazeDifficulty_CurrencyMultiplier", difficulty.currencyMultiplier);
            PlayerPrefs.SetString("MazeDifficulty_Name", difficulty.difficultyName);
            
            // Store first clear bonus values for potential application on completion
            PlayerPrefs.SetInt($"MazeDifficulty_FirstClear_Attunement_{difficulty.difficultyName}", difficulty.firstClearAttunementPoints);
            PlayerPrefs.SetInt($"MazeDifficulty_FirstClear_Currency_{difficulty.difficultyName}", difficulty.firstClearCurrencyBonus);
            PlayerPrefs.SetString($"MazeDifficulty_FirstClear_CurrencyType_{difficulty.difficultyName}", difficulty.firstClearCurrencyType.ToString());
            
            PlayerPrefs.Save();
            
            Debug.Log($"[MazeHubController] Starting maze run with difficulty: {difficulty.difficultyName}");
            
            // Start the run (this will load the maze scene)
            mazeRunManager.StartRun();
        }
        
        /// <summary>
        /// Checks if the player can start a run at the given difficulty.
        /// </summary>
        private bool CanStartDifficulty(MazeDifficultyConfig difficulty)
        {
            // Check level requirement
            var characterManager = CharacterManager.Instance;
            if (characterManager != null && characterManager.HasCharacter())
            {
                var character = characterManager.GetCurrentCharacter();
                if (character.level < difficulty.requiredLevel)
                {
                    return false;
                }
            }
            
            // Check if a run is already active
            if (mazeRunManager != null && mazeRunManager.HasActiveRun())
            {
                Debug.LogWarning("[MazeHubController] Cannot start new run: a run is already active!");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Updates the currency display on the hub UI.
        /// </summary>
        public void UpdateCurrencyDisplay()
        {
            if (mazeCurrencyText == null || currencyManager == null)
                return;
            
            int currencyAmount = currencyManager.GetCurrencyAmount(mazeCurrencyType);
            mazeCurrencyText.text = currencyAmount.ToString();
        }
        
        /// <summary>
        /// Exits to the main menu.
        /// </summary>
        private void ExitToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
        
        private void OnDestroy()
        {
            // Clean up button listeners
            if (startRunButton != null)
                startRunButton.onClick.RemoveAllListeners();
            
            if (vendorButton != null)
                vendorButton.onClick.RemoveAllListeners();
            
            if (forgeButton != null)
                forgeButton.onClick.RemoveAllListeners();
            
            if (statisticsButton != null)
                statisticsButton.onClick.RemoveAllListeners();
            
            if (exitButton != null)
                exitButton.onClick.RemoveAllListeners();
            
            if (backToHubButton != null)
                backToHubButton.onClick.RemoveAllListeners();
        }
    }
}


