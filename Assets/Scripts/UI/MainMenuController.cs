using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuControllerClean : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    
    [Header("Scene Names")]
    public string characterCreationSceneName = "CharacterCreation";
    
    // UI Elements
    private VisualElement mainMenuPanel;
    private VisualElement characterSelectionPanel;
    
    // Main Menu Buttons
    private Button newGameButton;
    private Button continueButton;
    private Button settingsButton;
    private Button exitButton;
    
    // Character Selection Elements
    private ListView characterListView;
    private Button createNewCharacterButton;
    private Button backToMainMenuButton;
    
    // Add these new fields to your existing MainMenuController class
    private VisualElement characterSidebar;
    private Button sidebarToggleButton;
    private Button toggleSidebarButton;
    private VisualElement characterList;
    private Button sidebarCreateNewCharacterButton;
    
    private void Start()
    {
        SetupUI();
        SetupButtonEvents();
        ShowMainMenu(); // Start with main menu visible
    }
    
    private void SetupUI()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        if (uiDocument == null)
        {
            Debug.LogError("MainMenuController: No UIDocument found!");
            return;
        }
        
        var root = uiDocument.rootVisualElement;
        
        // Get panel references
        mainMenuPanel = root.Q<VisualElement>("MainMenuPanel");
        characterSelectionPanel = root.Q<VisualElement>("CharacterSelectionPanel");
        
        // Get main menu button references
        newGameButton = root.Q<Button>("NewGameButton");
        continueButton = root.Q<Button>("ContinueButton");
        settingsButton = root.Q<Button>("SettingsButton");
        exitButton = root.Q<Button>("ExitButton");
        
        // Get sidebar references
        characterSidebar = root.Q<VisualElement>("CharacterSidebar");
        sidebarToggleButton = root.Q<Button>("SidebarToggleButton");
        toggleSidebarButton = root.Q<Button>("ToggleSidebarButton");
        characterList = root.Q<VisualElement>("CharacterList");
        sidebarCreateNewCharacterButton = root.Q<Button>("CreateNewCharacterButton");
        
        // Get character selection references (keep for backward compatibility)
        characterListView = root.Q<ListView>("CharacterListView");
        createNewCharacterButton = root.Q<Button>("CreateNewCharacterButton");
        backToMainMenuButton = root.Q<Button>("BackToMainMenuButton");
        
        // Check if save exists and enable/disable continue button
        UpdateContinueButton();
        
        // Initialize sidebar
        InitializeSidebar();
    }
    
    private void SetupButtonEvents()
    {
        // Main Menu Button Events
        if (newGameButton != null)
        {
            newGameButton.clicked += OnNewGameClicked;
        }
        
        if (continueButton != null)
        {
            continueButton.clicked += OnContinueClicked;
        }
        
        if (settingsButton != null)
        {
            settingsButton.clicked += OnSettingsClicked;
        }
        
        if (exitButton != null)
        {
            exitButton.clicked += OnExitClicked;
        }
        
        // Character Selection Button Events
        if (createNewCharacterButton != null)
        {
            createNewCharacterButton.clicked += OnCreateNewCharacter;
        }
        
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.clicked += OnBackToMainMenu;
        }
        
        // Set up character list view
        if (characterListView != null)
        {
            characterListView.makeItem = MakeCharacterItem;
            characterListView.bindItem = BindCharacterItem;
        }
    }
    
// Button Event Handlers
private void OnNewGameClicked()
{
    // Use direct scene loading instead of transition
    SceneManager.LoadScene(characterCreationSceneName);
}

private void OnContinueClicked()
{
    // Load the most recent character and continue
    var characters = CharacterSaveSystem.Instance.LoadCharacters();
    if (characters.Count > 0)
    {
        var mostRecentCharacter = characters[0]; // Assuming first is most recent
        OnCharacterSelected(mostRecentCharacter);
            }
            else
            {
        Debug.LogWarning("No saved characters found!");
    }
}
    
    private void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked - not implemented yet");
    }
    
    private void OnExitClicked()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }
    
    private void OnCreateNewCharacter()
    {
    // Use direct scene loading instead of transition
    SceneManager.LoadScene(characterCreationSceneName);
}
    
    private void OnBackToMainMenu()
    {
        ShowMainMenu();
    }
    
    // Panel Management
    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.style.display = DisplayStyle.Flex;
        }
        
        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.style.display = DisplayStyle.None;
        }
        
        UpdateContinueButton();
    }
    
    private void ShowCharacterSelection()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.style.display = DisplayStyle.None;
        }
        
        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.style.display = DisplayStyle.Flex;
        }
        
        LoadAndRefreshCharacterList();
    }
    
    private void UpdateContinueButton()
    {
        if (continueButton != null)
        {
            var characters = CharacterSaveSystem.Instance.LoadCharacters();
            continueButton.SetEnabled(characters.Count > 0);
        }
    }
    
    private void LoadAndRefreshCharacterList()
    {
        if (characterListView != null)
        {
            var characters = CharacterSaveSystem.Instance.LoadCharacters();
            characterListView.itemsSource = characters;
            characterListView.RefreshItems();
        }
    }
    
    // ListView Item Management
    private VisualElement MakeCharacterItem()
    {
        var container = new VisualElement();
        container.AddToClassList("character-item");
        return container;
    }
    
    private void BindCharacterItem(VisualElement element, int index)
    {
        if (characterListView?.itemsSource is List<CharacterData> characters && 
            index >= 0 && index < characters.Count)
        {
            var character = characters[index];
            
            // Clear existing content
            element.Clear();
            
            // Character info container
            var characterInfo = new VisualElement();
            characterInfo.AddToClassList("character-info");
            
            // Character details
            var characterDetails = new VisualElement();
            characterDetails.AddToClassList("character-details");
            
            var nameLabel = new Label(character.characterName);
            nameLabel.AddToClassList("character-name");
            characterDetails.Add(nameLabel);
            
            var levelLabel = new Label($"Level {character.level}");
            levelLabel.AddToClassList("character-level");
            characterDetails.Add(levelLabel);
            
            var classLabel = new Label(character.characterClass);
            classLabel.AddToClassList("character-class");
            characterDetails.Add(classLabel);
            
            characterInfo.Add(characterDetails);
            
            // Action buttons container
            var actionButtons = new VisualElement();
            actionButtons.AddToClassList("character-actions");
            
            // Load button
            var loadButton = new Button();
            loadButton.text = "Load Character";
            loadButton.AddToClassList("load-button");
            loadButton.clicked += () => OnCharacterSelected(character);
            actionButtons.Add(loadButton);
            
            // Delete button
            var deleteButton = new Button();
            deleteButton.text = "Delete";
            deleteButton.AddToClassList("delete-button");
            deleteButton.clicked += () => OnDeleteCharacter(character);
            actionButtons.Add(deleteButton);
            
            characterInfo.Add(actionButtons);
            element.Add(characterInfo);
        }
    }
    
    private void OnDeleteCharacter(CharacterData character)
    {
        // Show confirmation dialog
        ShowDeleteConfirmation(character);
    }

    private void ShowDeleteConfirmation(CharacterData character)
    {
        // Create confirmation dialog
        var dialog = new VisualElement();
        dialog.AddToClassList("confirmation-dialog");
        
        var dialogContent = new VisualElement();
        dialogContent.AddToClassList("dialog-content");
        
        var message = new Label($"Are you sure you want to delete '{character.characterName}'?");
        message.AddToClassList("dialog-message");
        dialogContent.Add(message);
        
        var buttonContainer = new VisualElement();
        buttonContainer.AddToClassList("dialog-buttons");
        
        var confirmButton = new Button();
        confirmButton.text = "Delete";
        confirmButton.AddToClassList("confirm-button");
        confirmButton.clicked += () => {
            ConfirmDeleteCharacter(character);
            dialog.RemoveFromHierarchy();
        };
        
        var cancelButton = new Button();
        cancelButton.text = "Cancel";
        cancelButton.AddToClassList("cancel-button");
        cancelButton.clicked += () => dialog.RemoveFromHierarchy();
        
        buttonContainer.Add(confirmButton);
        buttonContainer.Add(cancelButton);
        dialogContent.Add(buttonContainer);
        dialog.Add(dialogContent);
        
        // Add dialog to root
        var root = uiDocument.rootVisualElement;
        root.Add(dialog);
    }

    private void ConfirmDeleteCharacter(CharacterData character)
    {
        // Delete the character
        CharacterSaveSystem.Instance.DeleteCharacter(character.characterName);
        
        // Refresh both the sidebar and the old character list
        RefreshSidebarCharacters();
        LoadAndRefreshCharacterList();
        
        // Show feedback
        Debug.Log($"Deleted character: {character.characterName}");
        
        // Update continue button state
        UpdateContinueButton();
    }

    private void OnCharacterSelected(CharacterData character)
    {
        Debug.Log($"Loading character: {character.characterName}");
        
        // Load character data
                LoadCharacterData(character);
        
        // Use direct scene loading instead of transition
                SceneManager.LoadScene("MainGameUI");
    }
    
    private void LoadCharacterData(CharacterData character)
    {
        // Set current character data
        PlayerPrefs.SetString("CurrentCharacter", character.characterName);
        PlayerPrefs.SetInt("CurrentCharacterLevel", character.level);
        PlayerPrefs.SetInt("CurrentCharacterAct", character.act);
        PlayerPrefs.SetString("CurrentCharacterClass", character.characterClass);
        
        // Save the current character data
        PlayerPrefs.Save();
        
        Debug.Log($"Character data loaded: {character.characterName}");
    }
    
    private void OnDestroy()
    {
        // Cleanup - No selectionChanged to remove since we're using individual buttons
    }

    // Add this new method
    private void InitializeSidebar()
    {
        // Set up sidebar toggle button
        if (sidebarToggleButton != null)
        {
            sidebarToggleButton.clicked += ToggleSidebar;
        }
        
        // Set up sidebar close button
        if (toggleSidebarButton != null)
        {
            toggleSidebarButton.clicked += CloseSidebar;
        }
        
        // Set up create new character button in sidebar
        if (sidebarCreateNewCharacterButton != null)
        {
            sidebarCreateNewCharacterButton.clicked += OnCreateNewCharacter;
        }
        
        // Load characters into sidebar
        RefreshSidebarCharacters();
    }

    // Add these new methods
    private void ToggleSidebar()
    {
        if (characterSidebar != null)
        {
            bool isOpen = characterSidebar.ClassListContains("open");
            if (isOpen)
            {
                CloseSidebar();
            }
            else
            {
                OpenSidebar();
            }
        }
    }

    private void OpenSidebar()
    {
        if (characterSidebar != null)
        {
            characterSidebar.AddToClassList("open");
            RefreshSidebarCharacters();
        }
    }

    private void CloseSidebar()
    {
        if (characterSidebar != null)
        {
            characterSidebar.RemoveFromClassList("open");
        }
    }

    private void RefreshSidebarCharacters()
    {
        if (characterList == null) return;
        
        // Clear existing characters
        characterList.Clear();
        
        // Load characters
        var characters = CharacterSaveSystem.Instance.LoadCharacters();
        
        foreach (var character in characters)
        {
            var characterCard = CreateCharacterCard(character);
            characterList.Add(characterCard);
        }
    }

    private VisualElement CreateCharacterCard(CharacterData character)
    {
        var card = new VisualElement();
        card.AddToClassList("character-card");
        
        // Character info section
        var infoSection = new VisualElement();
        infoSection.AddToClassList("character-info");
        
        var nameLabel = new Label(character.characterName);
        nameLabel.AddToClassList("character-name");
        infoSection.Add(nameLabel);
        
        var detailsLabel = new Label($"Level {character.level} {character.characterClass} - Act {character.act}");
        detailsLabel.AddToClassList("character-details");
        infoSection.Add(detailsLabel);
        
        card.Add(infoSection);
        
        // Action buttons section
        var actionSection = new VisualElement();
        actionSection.AddToClassList("character-actions");
        
        var playButton = new Button();
        playButton.text = "Play";
        playButton.AddToClassList("character-action-button");
        playButton.AddToClassList("primary");
        playButton.clicked += () => OnCharacterSelected(character);
        actionSection.Add(playButton);
        
        var deleteButton = new Button();
        deleteButton.text = "Delete";
        deleteButton.AddToClassList("character-action-button");
        deleteButton.AddToClassList("danger");
        deleteButton.clicked += () => OnDeleteCharacter(character);
        actionSection.Add(deleteButton);
        
        card.Add(actionSection);
        
        return card;
    }
}
