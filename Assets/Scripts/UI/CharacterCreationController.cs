using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class CharacterCreationController : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    
    // UI Elements - these will be connected to your UXML elements
    private TextField characterNameInput;
    private Button createCharacterButton;
    private Button backButton;
    private VisualElement characterPreview;
    private VisualElement deckPreview;
    
    // Class Buttons - connected to your class selection buttons
    private Button witchButton;
    private Button marauderButton;
    private Button rangerButton;
    private Button thiefButton;
    private Button apostleButton;
    private Button brawlerButton;
    
    // Current selection state
    private string selectedClass = "";
    private string characterName = "";
    
    // Add this field to the CharacterCreationController class
    [Header("Deck Management")]
    public StarterDeckManager starterDeckManager;
    
    private void Start()
    {
        SetupUI();
        SetupButtonEvents();
        UpdateCreateButtonState();
    }
    
    private void SetupUI()
    {
        // Get UIDocument if not assigned
        if (uiDocument == null)
        {
            uiDocument = GetComponentInParent<UIDocument>();
        }
        
        if (uiDocument == null)
        {
            Debug.LogError("CharacterCreationController: No UIDocument found!");
            return;
        }
        
        var root = uiDocument.rootVisualElement;
        
        // Connect to UXML elements by name
        characterNameInput = root.Q<TextField>("CharacterNameInput");
        createCharacterButton = root.Q<Button>("CreateCharacterButton");
        backButton = root.Q<Button>("BackButton");
        characterPreview = root.Q<VisualElement>("CharacterPreview");
        deckPreview = root.Q<VisualElement>("StarterDeckPreview");
        
        // Connect to class buttons
        witchButton = root.Q<Button>("WitchButton");
        marauderButton = root.Q<Button>("MarauderButton");
        rangerButton = root.Q<Button>("RangerButton");
        thiefButton = root.Q<Button>("ThiefButton");
        apostleButton = root.Q<Button>("ApostleButton");
        brawlerButton = root.Q<Button>("BrawlerButton");
        
        // Verify connections
        if (characterNameInput == null) Debug.LogError("CharacterNameInput not found!");
        if (createCharacterButton == null) Debug.LogError("CreateCharacterButton not found!");
        if (backButton == null) Debug.LogError("BackButton not found!");
    }
    
    private void SetupButtonEvents()
    {
        // Name input event
        if (characterNameInput != null)
        {
            characterNameInput.RegisterValueChangedCallback(OnNameChanged);
        }
        
        // Action buttons
        if (createCharacterButton != null)
        {
            createCharacterButton.clicked += OnCreateCharacter;
        }
        
        if (backButton != null)
        {
            backButton.clicked += OnBackToMainMenu;
        }
        
        // Class selection buttons
        if (witchButton != null)
        {
            witchButton.clicked += () => OnClassSelected("Witch");
        }
        
        if (marauderButton != null)
        {
            marauderButton.clicked += () => OnClassSelected("Marauder");
        }
        
        if (rangerButton != null)
        {
            rangerButton.clicked += () => OnClassSelected("Ranger");
        }
        
        if (thiefButton != null)
        {
            thiefButton.clicked += () => OnClassSelected("Thief");
        }
        
        if (apostleButton != null)
        {
            apostleButton.clicked += () => OnClassSelected("Apostle");
        }
        
        if (brawlerButton != null)
        {
            brawlerButton.clicked += () => OnClassSelected("Brawler");
        }
    }
    
    // Event handlers
    private void OnNameChanged(ChangeEvent<string> evt)
    {
        characterName = evt.newValue;
        UpdateCreateButtonState();
    }
    
    private void OnClassSelected(string className)
    {
        selectedClass = className;
        UpdateClassSelection();
        UpdateCreateButtonState();
        UpdateCharacterPreview();
    }
    
    private void UpdateClassSelection()
    {
        // Reset all button styles
        ResetClassButtonStyles();
        
        // Highlight selected button
        Button selectedButton = GetClassButton(selectedClass);
        if (selectedButton != null)
        {
            selectedButton.AddToClassList("selected");
        }
    }
    
    private void ResetClassButtonStyles()
    {
        Button[] classButtons = { witchButton, marauderButton, rangerButton, 
                                 thiefButton, apostleButton, brawlerButton };
        
        foreach (var button in classButtons)
        {
            if (button != null)
            {
                button.RemoveFromClassList("selected");
            }
        }
    }
    
    private Button GetClassButton(string className)
    {
        switch (className)
        {
            case "Witch": return witchButton;
            case "Marauder": return marauderButton;
            case "Ranger": return rangerButton;
            case "Thief": return thiefButton;
            case "Apostle": return apostleButton;
            case "Brawler": return brawlerButton;
            default: return null;
        }
    }
    
    private void UpdateCharacterPreview()
    {
        if (characterPreview != null)
        {
            // Update preview based on selected class
            // You can add preview images or styling here
            characterPreview.style.backgroundColor = GetClassColor(selectedClass);
        }

        // Update starter deck preview
        UpdateStarterDeckPreview();
    }

    private void UpdateStarterDeckPreview()
    {
        if (deckPreview == null) return;
        deckPreview.Clear();

        if (string.IsNullOrEmpty(selectedClass)) return;

        var sdm = starterDeckManager != null ? starterDeckManager : StarterDeckManager.Instance;
        var def = sdm != null ? sdm.GetDefinitionForClass(selectedClass) : null;
        if (def == null || def.cards == null || def.cards.Count == 0) return;

        // Simple list: "xN Card Name" entries
        foreach (var entry in def.cards)
        {
            if (entry == null || entry.card == null || entry.count <= 0) continue;
            var row = new Label($"x{entry.count}  {entry.card.cardName}");
            row.AddToClassList("deck-preview-row");
            deckPreview.Add(row);
        }
    }
    
    private Color GetClassColor(string className)
    {
        switch (className)
        {
            case "Witch": return new Color(0.5f, 0.2f, 0.8f, 0.3f); // Purple
            case "Marauder": return new Color(0.8f, 0.2f, 0.2f, 0.3f); // Red
            case "Ranger": return new Color(0.2f, 0.8f, 0.2f, 0.3f); // Green
            case "Thief": return new Color(0.2f, 0.2f, 0.8f, 0.3f); // Blue
            case "Apostle": return new Color(0.8f, 0.8f, 0.2f, 0.3f); // Yellow
            case "Brawler": return new Color(0.8f, 0.5f, 0.2f, 0.3f); // Orange
            default: return new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gray
        }
    }
    
    private void UpdateCreateButtonState()
    {
        if (createCharacterButton != null)
        {
            bool canCreate = !string.IsNullOrEmpty(characterName) && 
                           !string.IsNullOrEmpty(selectedClass);
            
            createCharacterButton.SetEnabled(canCreate);
            
            if (canCreate)
            {
                createCharacterButton.style.opacity = 1f;
            }
            else
            {
                createCharacterButton.style.opacity = 0.5f;
            }
        }
    }
    
    private void OnCreateCharacter()
    {
        if (string.IsNullOrEmpty(characterName) || string.IsNullOrEmpty(selectedClass))
        {
            Debug.LogWarning("Cannot create character: Missing name or class selection");
            return;
        }
        
        // Check if character name already exists
        if (CharacterSaveSystem.Instance.CharacterExists(characterName))
        {
            Debug.LogWarning($"Character name '{characterName}' already exists!");
            // TODO: Show error message to user
            return;
        }
        
        // Create new character using CharacterManager
        CharacterManager.Instance.CreateCharacter(characterName, selectedClass);
        
        // Initialize starter collection/deckData using CardDataExtended-only definitions
        var sdm = starterDeckManager != null ? starterDeckManager : StarterDeckManager.Instance;
        if (sdm != null)
        {
            sdm.AssignStarterToCharacterDeckData(CharacterManager.Instance.GetCurrentCharacter());
        }
        
        Debug.Log($"Created new character: {characterName} ({selectedClass})");
        
        // Persist last character for scene continuity
        PlayerPrefs.SetString("LastCharacterName", characterName);
        PlayerPrefs.Save();
        // Transition to main game
        SceneManager.LoadScene("MainGameUI");
    }
    
    private void OnBackToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        // Use direct scene loading instead of transition
        SceneManager.LoadScene("MainMenu");
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners to prevent memory leaks
        if (characterNameInput != null)
        {
            characterNameInput.UnregisterValueChangedCallback(OnNameChanged);
        }
        
        if (createCharacterButton != null)
        {
            createCharacterButton.clicked -= OnCreateCharacter;
        }
        
        if (backButton != null)
        {
            backButton.clicked -= OnBackToMainMenu;
        }
        
        // Clean up class button events
        if (witchButton != null) witchButton.clicked -= () => OnClassSelected("Witch");
        if (marauderButton != null) marauderButton.clicked -= () => OnClassSelected("Marauder");
        if (rangerButton != null) rangerButton.clicked -= () => OnClassSelected("Ranger");
        if (thiefButton != null) thiefButton.clicked -= () => OnClassSelected("Thief");
        if (apostleButton != null) apostleButton.clicked -= () => OnClassSelected("Apostle");
        if (brawlerButton != null) brawlerButton.clicked -= () => OnClassSelected("Brawler");
    }
}
