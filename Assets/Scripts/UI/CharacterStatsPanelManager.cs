using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Centralized manager for the Character Stats Panel following the established architecture pattern.
/// Handles panel visibility, data synchronization, and integration with UIManager and CharacterManager.
/// </summary>
public class CharacterStatsPanelManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject characterStatsPanel;
    public CharacterStatsController statsController;
	private RectTransform panelRect;
	private CanvasGroup panelCanvasGroup;
    
    [Header("Toggle Button")]
    public Button toggleButton;
    
    [Header("Panel State")]
    public bool isPanelVisible = false;
	[Header("Slide Settings")]
	[SerializeField] private bool useSlideAnimation = true;
	[SerializeField] private float slideDuration = 0.25f;
	[SerializeField] private AnimationCurve slideEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField] private float offscreenPadding = 40f;
	private Vector2 visibleAnchoredPos;
	private Vector2 hiddenAnchoredPos;
    
    [Header("Toggle Protection")]
    [SerializeField] private float toggleCooldown = 0.1f; // Prevent rapid successive toggles
    private float lastToggleTime = 0f;
    private bool isToggling = false; // Prevent multiple simultaneous toggles
    
    // Singleton pattern following project conventions
    public static CharacterStatsPanelManager Instance { get; private set; }
    
    // References to other managers
    private UIManager uiManager;
    private CharacterManager characterManager;
    
    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
		// Initialize panel state
		if (characterStatsPanel != null)
		{
			panelRect = characterStatsPanel.GetComponent<RectTransform>();
			panelCanvasGroup = characterStatsPanel.GetComponent<CanvasGroup>();
			if (panelCanvasGroup == null) panelCanvasGroup = characterStatsPanel.AddComponent<CanvasGroup>();
			if (panelRect != null)
			{
				visibleAnchoredPos = panelRect.anchoredPosition;
				// Compute hidden position to the LEFT by panel width + padding
				float width = panelRect.rect.width;
				hiddenAnchoredPos = visibleAnchoredPos - new Vector2(width + offscreenPadding, 0f);
				// Start hidden
				panelRect.anchoredPosition = hiddenAnchoredPos;
			}
			panelCanvasGroup.blocksRaycasts = false; // don't intercept when hidden
			panelCanvasGroup.interactable = false;
			panelCanvasGroup.alpha = 0f;
			// Start disabled so it cannot intercept any raycasts
			characterStatsPanel.SetActive(false);
			isPanelVisible = false;
		}
    }
    
    private void Start()
    {
        // Get references to other managers
        uiManager = FindFirstObjectByType<UIManager>();
        characterManager = CharacterManager.Instance;
        
        // Auto-setup panel references if not assigned
        if (characterStatsPanel == null || statsController == null)
        {
            SetupPanelReferences();
        }
        
        // Setup toggle button if assigned
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePanel);
            Debug.Log($"[CharacterStatsPanelManager] Toggle button connected: {toggleButton.name}");
        }
        else
        {
            Debug.LogWarning("[CharacterStatsPanelManager] No toggle button assigned!");
        }
        
        // Subscribe to character data changes
        if (characterManager != null)
        {
            // Note: CharacterManager doesn't have events yet, so we'll update on toggle
            Debug.Log("[CharacterStatsPanelManager] Connected to CharacterManager");
        }
        else
        {
            Debug.LogWarning("[CharacterStatsPanelManager] CharacterManager not found!");
        }
        
        Debug.Log($"[CharacterStatsPanelManager] Initialized successfully - Panel: {(characterStatsPanel != null ? "Assigned" : "NOT ASSIGNED")}, Controller: {(statsController != null ? "Assigned" : "NOT ASSIGNED")}");
    }
    
    private void OnDestroy()
    {
        // Clean up button listeners
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(TogglePanel);
        }
    }
    
    /// <summary>
    /// Toggle the character stats panel visibility
    /// </summary>
    public void TogglePanel()
    {
        // Prevent rapid successive toggles
        if (Time.time - lastToggleTime < toggleCooldown)
        {
            Debug.Log($"[CharacterStatsPanelManager] Toggle blocked - too soon since last toggle ({(Time.time - lastToggleTime):F3}s)");
            return;
        }
        
        // Prevent multiple simultaneous toggles
        if (isToggling)
        {
            Debug.Log("[CharacterStatsPanelManager] Toggle blocked - already toggling");
            return;
        }
        
        isToggling = true;
        lastToggleTime = Time.time;
        
        Debug.Log($"[CharacterStatsPanelManager] TogglePanel() called - Current state: {(isPanelVisible ? "Visible" : "Hidden")}");
        
        if (characterStatsPanel == null)
        {
            Debug.LogWarning("[CharacterStatsPanelManager] CharacterStatsPanel is not assigned!");
            isToggling = false;
            return;
        }
        
		// Toggle the visibility state
		isPanelVisible = !isPanelVisible;
		
		if (!useSlideAnimation || panelRect == null || panelCanvasGroup == null)
		{
			characterStatsPanel.SetActive(isPanelVisible);
			panelCanvasGroup.blocksRaycasts = isPanelVisible;
			panelCanvasGroup.interactable = isPanelVisible;
			panelCanvasGroup.alpha = isPanelVisible ? 1f : 0f;
			if (isPanelVisible) UpdatePanelData();
		}
		else
		{
			// Animate slide and raycast state
			Vector2 target = isPanelVisible ? visibleAnchoredPos : hiddenAnchoredPos;
			if (isPanelVisible)
			{
				// Ensure active before animating in
				characterStatsPanel.SetActive(true);
				// Start from hidden position and 0 alpha if this is a fresh open
				panelRect.anchoredPosition = hiddenAnchoredPos;
				panelCanvasGroup.alpha = 0f;
				UpdatePanelData();
				panelCanvasGroup.blocksRaycasts = true;
				panelCanvasGroup.interactable = true;
			}
			LeanTween.cancel(characterStatsPanel);
			LeanTween.value(characterStatsPanel, panelRect.anchoredPosition, target, slideDuration)
				.setOnUpdate((Vector2 v) => panelRect.anchoredPosition = v)
				.setEase(slideEase);
			LeanTween.value(characterStatsPanel, panelCanvasGroup.alpha, isPanelVisible ? 1f : 0f, slideDuration)
				.setOnUpdate((float a) => panelCanvasGroup.alpha = a)
				.setEase(slideEase)
				.setOnComplete(() => {
					if (!isPanelVisible)
					{
						panelCanvasGroup.blocksRaycasts = false;
						panelCanvasGroup.interactable = false;
						// Finally disable so it cannot intercept any clicks
						characterStatsPanel.SetActive(false);
					}
				});
		}
		
		Debug.Log($"[CharacterStatsPanelManager] Panel toggled: {(isPanelVisible ? "Visible" : "Hidden")}" );
        
        // Reset toggle protection
        isToggling = false;
    }
    
    /// <summary>
    /// Show the character stats panel
    /// </summary>
    public void ShowPanel()
    {
        if (characterStatsPanel == null)
        {
            Debug.LogWarning("[CharacterStatsPanelManager] CharacterStatsPanel is not assigned!");
            return;
        }
        
        isPanelVisible = true;
        characterStatsPanel.SetActive(true);
        UpdatePanelData();
        
        Debug.Log("[CharacterStatsPanelManager] Panel shown");
    }
    
    /// <summary>
    /// Hide the character stats panel
    /// </summary>
    public void HidePanel()
    {
        if (characterStatsPanel == null)
        {
            Debug.LogWarning("[CharacterStatsPanelManager] CharacterStatsPanel is not assigned!");
            return;
        }
        
        isPanelVisible = false;
        characterStatsPanel.SetActive(false);
        
        Debug.Log("[CharacterStatsPanelManager] Panel hidden");
    }
    
    /// <summary>
    /// Check if the panel is currently visible
    /// </summary>
    public bool IsPanelVisible()
    {
        return isPanelVisible && characterStatsPanel != null && characterStatsPanel.activeSelf;
    }
    
    /// <summary>
    /// Update the panel with current character data
    /// </summary>
    public void UpdatePanelData()
    {
        if (characterManager == null)
        {
            Debug.LogWarning("[CharacterStatsPanelManager] CharacterManager not found!");
            return;
        }
        
        if (!characterManager.HasCharacter())
        {
            Debug.LogWarning("[CharacterStatsPanelManager] No character loaded!");
            return;
        }
        
        Character currentCharacter = characterManager.GetCurrentCharacter();
        if (currentCharacter == null)
        {
            Debug.LogWarning("[CharacterStatsPanelManager] Current character is null!");
            return;
        }
        
        // Update the stats controller if available
        if (statsController != null)
        {
            statsController.UpdateCharacterStats(currentCharacter);
        }
        else
        {
            Debug.LogWarning("[CharacterStatsPanelManager] StatsController not assigned!");
        }
        
        Debug.Log($"[CharacterStatsPanelManager] Updated panel data for {currentCharacter.characterName}");
    }
    
    /// <summary>
    /// Force refresh of panel data (useful when character stats change)
    /// </summary>
    public void RefreshPanelData()
    {
        if (IsPanelVisible())
        {
            UpdatePanelData();
        }
    }
    
    /// <summary>
    /// Debug method to check the current state of the manager
    /// </summary>
    [ContextMenu("Debug Manager State")]
    public void DebugManagerState()
    {
        Debug.Log("=== CharacterStatsPanelManager Debug Info ===");
        Debug.Log($"Instance exists: {Instance != null}");
        Debug.Log($"Panel assigned: {characterStatsPanel != null}");
        Debug.Log($"Controller assigned: {statsController != null}");
        Debug.Log($"Toggle button assigned: {toggleButton != null}");
        Debug.Log($"CharacterManager exists: {characterManager != null}");
        Debug.Log($"UIManager exists: {uiManager != null}");
        Debug.Log($"Panel visible: {isPanelVisible}");
        
        if (characterStatsPanel != null)
        {
            Debug.Log($"Panel active: {characterStatsPanel.activeSelf}");
            Debug.Log($"Panel name: {characterStatsPanel.name}");
        }
        
        if (toggleButton != null)
        {
            Debug.Log($"Toggle button name: {toggleButton.name}");
            Debug.Log($"Toggle button active: {toggleButton.gameObject.activeSelf}");
            Debug.Log($"Toggle button interactable: {toggleButton.interactable}");
            Debug.Log($"Toggle button onClick event count: {toggleButton.onClick.GetPersistentEventCount()}");
        }
        
        if (characterManager != null)
        {
            Debug.Log($"Has character: {characterManager.HasCharacter()}");
            if (characterManager.HasCharacter())
            {
                Character character = characterManager.GetCurrentCharacter();
                Debug.Log($"Character name: {character.characterName}");
            }
        }
        
        Debug.Log("=== End Debug Info ===");
    }
    
    /// <summary>
    /// Test toggle functionality from context menu
    /// </summary>
    [ContextMenu("Test Toggle Panel")]
    public void TestTogglePanel()
    {
        Debug.Log("[CharacterStatsPanelManager] Testing toggle panel from context menu...");
        TogglePanel();
    }
    
    /// <summary>
    /// Setup the panel with required references
    /// </summary>
    [ContextMenu("Setup Panel References")]
    public void SetupPanelReferences()
    {
        // Try to find the panel if not assigned
        if (characterStatsPanel == null)
        {
            // First try to find by exact name
            characterStatsPanel = GameObject.Find("CharacterStatsPanel");
            
            // If not found, look for a parent with that name and find the prefab underneath
            if (characterStatsPanel == null)
            {
                GameObject parentPanel = GameObject.Find("CharacterStatsPanel");
                if (parentPanel != null)
                {
                    // Look for the actual prefab underneath the parent
                    CharacterStatsController controller = parentPanel.GetComponentInChildren<CharacterStatsController>();
                    if (controller != null)
                    {
                        characterStatsPanel = controller.gameObject;
                        Debug.Log($"[CharacterStatsPanelManager] Found panel under parent: {characterStatsPanel.name}");
                    }
                    else
                    {
                        // If no controller found, use the parent itself
                        characterStatsPanel = parentPanel;
                        Debug.Log($"[CharacterStatsPanelManager] Using parent as panel: {characterStatsPanel.name}");
                    }
                }
            }
            
            if (characterStatsPanel == null)
            {
                Debug.LogWarning("[CharacterStatsPanelManager] Could not find CharacterStatsPanel in scene!");
            }
            else
            {
                Debug.Log($"[CharacterStatsPanelManager] Found panel: {characterStatsPanel.name}");
            }
        }
        
        // Try to find the stats controller if not assigned
        if (statsController == null)
        {
            if (characterStatsPanel != null)
            {
                statsController = characterStatsPanel.GetComponent<CharacterStatsController>();
                if (statsController == null)
                {
                    // Try to find it in children
                    statsController = characterStatsPanel.GetComponentInChildren<CharacterStatsController>();
                }
                
                if (statsController == null)
                {
                    Debug.LogWarning("[CharacterStatsPanelManager] CharacterStatsController not found on panel or children!");
                }
                else
                {
                    Debug.Log($"[CharacterStatsPanelManager] Found controller: {statsController.name}");
                }
            }
        }
        
        // Try to find the toggle button if not assigned
        if (toggleButton == null)
        {
            // Look for a button with "Character" or "Stats" in the name
            Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button button in buttons)
            {
                if (button.name.ToLower().Contains("character") || 
                    button.name.ToLower().Contains("stats") ||
                    button.name.ToLower().Contains("char"))
                {
                    toggleButton = button;
                    Debug.Log($"[CharacterStatsPanelManager] Found toggle button: {button.name}");
                    break;
                }
            }
        }
        
        Debug.Log($"[CharacterStatsPanelManager] Panel references setup complete - Panel: {(characterStatsPanel != null ? "✓" : "✗")}, Controller: {(statsController != null ? "✓" : "✗")}, Button: {(toggleButton != null ? "✓" : "✗")}");
    }
    
    /// <summary>
    /// Manually connect a button to the toggle functionality
    /// </summary>
    [ContextMenu("Connect Toggle Button")]
    public void ConnectToggleButton()
    {
        if (toggleButton == null)
        {
            Debug.LogWarning("[CharacterStatsPanelManager] No toggle button assigned! Please assign a button first.");
            return;
        }
        
        // Remove any existing listeners to avoid duplicates
        toggleButton.onClick.RemoveListener(TogglePanel);
        
        // Add the toggle listener
        toggleButton.onClick.AddListener(TogglePanel);
        
        Debug.Log($"[CharacterStatsPanelManager] Toggle button connected: {toggleButton.name}");
        Debug.Log($"[CharacterStatsPanelManager] Button onClick event count: {toggleButton.onClick.GetPersistentEventCount()}");
    }
    
    /// <summary>
    /// Check for multiple event listeners on the button
    /// </summary>
    [ContextMenu("Check Button Events")]
    public void CheckButtonEvents()
    {
        if (toggleButton == null)
        {
            Debug.LogWarning("[CharacterStatsPanelManager] No toggle button assigned!");
            return;
        }
        
        Debug.Log("=== Button Event Analysis ===");
        Debug.Log($"Button name: {toggleButton.name}");
        Debug.Log($"Persistent event count: {toggleButton.onClick.GetPersistentEventCount()}");
        
        // Check for any other scripts that might be listening to this button
        var listeners = toggleButton.onClick.GetPersistentEventCount();
        if (listeners > 1)
        {
            Debug.LogWarning($"[CharacterStatsPanelManager] Button has {listeners} event listeners! This might cause multiple calls.");
            Debug.LogWarning("Check the button's OnClick() events in the inspector and remove duplicates.");
        }
        
        // Check if the button has any other components that might interfere
        var components = toggleButton.GetComponents<MonoBehaviour>();
        Debug.Log($"Button has {components.Length} MonoBehaviour components:");
        foreach (var component in components)
        {
            Debug.Log($"  - {component.GetType().Name}");
        }
        
        Debug.Log("=== End Button Analysis ===");
    }
}
