using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Menu button for MainGameUI that opens a menu panel with Save & Quit options.
/// </summary>
public class MainGameMenuButton : MonoBehaviour
{
    [Header("Menu Button")]
    [Tooltip("The button that opens the menu (if null, will try to find on this GameObject)")]
    [SerializeField] private Button menuButton;
    
    [Header("Menu Panel")]
    [Tooltip("The menu panel GameObject that contains the menu options (should be inactive by default)")]
    [SerializeField] private GameObject menuPanel;
    
    [Header("Menu Panel Buttons")]
    [Tooltip("Button to save and quit to main menu")]
    [SerializeField] private Button saveAndQuitButton;
    
    [Tooltip("Button to quit without saving (optional)")]
    [SerializeField] private Button quitWithoutSavingButton;
    
    [Tooltip("Button to cancel/close the menu")]
    [SerializeField] private Button cancelButton;
    
    [Header("Scene Settings")]
    [Tooltip("Name of the main menu scene to load")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [Header("Confirmation Dialog (Optional)")]
    [Tooltip("Optional confirmation dialog panel for quit actions")]
    [SerializeField] private GameObject confirmationDialog;
    
    [Tooltip("Confirmation dialog message text")]
    [SerializeField] private TextMeshProUGUI confirmationMessageText;
    
    [Tooltip("Confirm button in confirmation dialog")]
    [SerializeField] private Button confirmButton;
    
    [Tooltip("Cancel button in confirmation dialog")]
    [SerializeField] private Button confirmCancelButton;
    
    private bool isMenuOpen = false;
    private System.Action pendingAction = null;
    
    private void Awake()
    {
        // Find menu button if not assigned
        if (menuButton == null)
        {
            menuButton = GetComponent<Button>();
        }
        
        // Find menu panel if not assigned (search by name)
        if (menuPanel == null)
        {
            GameObject found = GameObject.Find("MenuPanel");
            if (found != null)
            {
                menuPanel = found;
            }
        }
    }
    
    private void Start()
    {
        SetupButtons();
        
        // Ensure menu panel starts closed and sync state
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            isMenuOpen = false; // Explicitly set state to match panel
        }
        
        // Ensure confirmation dialog starts closed
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
    }
    
    private void SetupButtons()
    {
        // Menu button
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(ToggleMenu);
        }
        else
        {
            Debug.LogWarning("[MainGameMenuButton] Menu button not found. Please assign it in Inspector.");
        }
        
        // Save & Quit button
        if (saveAndQuitButton != null)
        {
            saveAndQuitButton.onClick.RemoveAllListeners();
            saveAndQuitButton.onClick.AddListener(OnSaveAndQuitClicked);
        }
        else
        {
            Debug.LogWarning("[MainGameMenuButton] Save & Quit button not found. Please assign it in Inspector.");
        }
        
        // Quit without saving button (optional)
        if (quitWithoutSavingButton != null)
        {
            quitWithoutSavingButton.onClick.RemoveAllListeners();
            quitWithoutSavingButton.onClick.AddListener(OnQuitWithoutSavingClicked);
        }
        
        // Cancel button
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CloseMenu);
        }
        else
        {
            Debug.LogWarning("[MainGameMenuButton] Cancel button not found. Please assign it in Inspector.");
        }
        
        // Confirmation dialog buttons
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
        
        if (confirmCancelButton != null)
        {
            confirmCancelButton.onClick.RemoveAllListeners();
            confirmCancelButton.onClick.AddListener(OnConfirmCancelClicked);
        }
    }
    
    private void Update()
    {
        // Close menu with Escape key (using new Input System)
        if (isMenuOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseMenu();
        }
    }
    
    /// <summary>
    /// Toggle the menu panel open/closed
    /// </summary>
    public void ToggleMenu()
    {
        if (menuPanel == null)
        {
            Debug.LogWarning("[MainGameMenuButton] Menu panel not assigned. Cannot toggle menu.");
            return;
        }
        
        // Toggle state
        isMenuOpen = !isMenuOpen;
        
        // Update panel visibility to match state
        menuPanel.SetActive(isMenuOpen);
        
        // Verify state is correct (safety check)
        if (menuPanel.activeSelf != isMenuOpen)
        {
            Debug.LogWarning($"[MainGameMenuButton] State mismatch detected! Panel active: {menuPanel.activeSelf}, isMenuOpen: {isMenuOpen}. Syncing...");
            isMenuOpen = menuPanel.activeSelf;
        }
        
        // Pause game time when menu is open (optional)
        // Time.timeScale = isMenuOpen ? 0f : 1f;
        
        Debug.Log($"[MainGameMenuButton] Menu {(isMenuOpen ? "opened" : "closed")}");
    }
    
    /// <summary>
    /// Close the menu panel
    /// </summary>
    public void CloseMenu()
    {
        if (menuPanel != null)
        {
            isMenuOpen = false;
            menuPanel.SetActive(false);
            // Time.timeScale = 1f;
            Debug.Log("[MainGameMenuButton] Menu closed");
        }
        
        // Also close confirmation dialog if open
        if (confirmationDialog != null && confirmationDialog.activeSelf)
        {
            confirmationDialog.SetActive(false);
            pendingAction = null;
        }
    }
    
    /// <summary>
    /// Handle Save & Quit button click
    /// </summary>
    private void OnSaveAndQuitClicked()
    {
        if (confirmationDialog != null)
        {
            // Show confirmation dialog
            ShowConfirmationDialog("Save and return to main menu?", () => ExecuteSaveAndQuit());
        }
        else
        {
            // Execute directly if no confirmation dialog
            ExecuteSaveAndQuit();
        }
    }
    
    /// <summary>
    /// Execute save and quit to main menu
    /// </summary>
    private void ExecuteSaveAndQuit()
    {
        Debug.Log("[MainGameMenuButton] Saving character and quitting to main menu...");
        
        // Save character
        if (CharacterManager.Instance != null && CharacterManager.Instance.GetCurrentCharacter() != null)
        {
            CharacterManager.Instance.SaveCharacter();
            Debug.Log("[MainGameMenuButton] Character saved successfully.");
        }
        else
        {
            Debug.LogWarning("[MainGameMenuButton] No character to save.");
        }
        
        // Close menu
        CloseMenu();
        
        // Load main menu scene
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("[MainGameMenuButton] Main menu scene name is not set!");
        }
    }
    
    /// <summary>
    /// Handle Quit Without Saving button click
    /// </summary>
    private void OnQuitWithoutSavingClicked()
    {
        if (confirmationDialog != null)
        {
            // Show confirmation dialog
            ShowConfirmationDialog("Quit without saving? All unsaved progress will be lost.", () => ExecuteQuitWithoutSaving());
        }
        else
        {
            // Execute directly if no confirmation dialog
            ExecuteQuitWithoutSaving();
        }
    }
    
    /// <summary>
    /// Execute quit without saving
    /// </summary>
    private void ExecuteQuitWithoutSaving()
    {
        Debug.Log("[MainGameMenuButton] Quitting to main menu without saving...");
        
        // Close menu
        CloseMenu();
        
        // Load main menu scene
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("[MainGameMenuButton] Main menu scene name is not set!");
        }
    }
    
    /// <summary>
    /// Show confirmation dialog with a message and action
    /// </summary>
    private void ShowConfirmationDialog(string message, System.Action onConfirm)
    {
        if (confirmationDialog == null)
        {
            // No confirmation dialog, execute action directly
            onConfirm?.Invoke();
            return;
        }
        
        // Set message text
        if (confirmationMessageText != null)
        {
            confirmationMessageText.text = message;
        }
        
        // Store pending action
        pendingAction = onConfirm;
        
        // Show dialog
        confirmationDialog.SetActive(true);
    }
    
    /// <summary>
    /// Handle confirm button in confirmation dialog
    /// </summary>
    private void OnConfirmClicked()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
        
        // Execute pending action
        pendingAction?.Invoke();
        pendingAction = null;
    }
    
    /// <summary>
    /// Handle cancel button in confirmation dialog
    /// </summary>
    private void OnConfirmCancelClicked()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
        
        pendingAction = null;
    }
    
    private void OnDestroy()
    {
        // Clean up button listeners
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
        }
        
        if (saveAndQuitButton != null)
        {
            saveAndQuitButton.onClick.RemoveAllListeners();
        }
        
        if (quitWithoutSavingButton != null)
        {
            quitWithoutSavingButton.onClick.RemoveAllListeners();
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
        }
        
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
        }
        
        if (confirmCancelButton != null)
        {
            confirmCancelButton.onClick.RemoveAllListeners();
        }
    }
}

