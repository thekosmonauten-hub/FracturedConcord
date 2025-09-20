using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CharacterSelectionUIToolkit : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    
    private ListView characterListView;
    private Button createNewCharacterButton;
    private Button backButton;
    
    private List<CharacterData> savedCharacters = new List<CharacterData>();
    
    private void Start()
    {
        SetupUI();
        LoadSavedCharacters();
        RefreshCharacterList();
    }
    
    private void SetupUI()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        if (uiDocument == null)
        {
            Debug.LogError("No UIDocument found!");
            return;
        }
        
        var root = uiDocument.rootVisualElement;
        
        // Get UI elements by name (you'll need to set these in your UXML)
        characterListView = root.Q<ListView>("CharacterListView");
        createNewCharacterButton = root.Q<Button>("CreateNewCharacterButton");
        backButton = root.Q<Button>("BackButton");
        
        // Set up button events
        if (createNewCharacterButton != null)
        {
            createNewCharacterButton.clicked += OnCreateNewCharacter;
        }
        
        if (backButton != null)
        {
            backButton.clicked += OnBackToMainMenu;
        }
        
        // Set up ListView
        if (characterListView != null)
        {
            characterListView.makeItem = MakeCharacterItem;
            characterListView.bindItem = BindCharacterItem;
            characterListView.selectionChanged += OnCharacterSelected;
        }
    }
    
    private VisualElement MakeCharacterItem()
    {
        // Create a button for each character
        var button = new Button();
        button.AddToClassList("character-button");
        return button;
    }
    
    private void BindCharacterItem(VisualElement element, int index)
    {
        if (index < savedCharacters.Count)
        {
            var character = savedCharacters[index];
            var button = element as Button;
            
            if (button != null)
            {
                button.text = $"{character.characterName} - Level {character.level} (Act {character.act})";
                button.userData = character; // Store character data
            }
        }
    }
    
    private void LoadSavedCharacters()
    {
        savedCharacters = CharacterSaveSystem.Instance.LoadCharacters();
        
        // For testing, add sample characters if none exist
        if (savedCharacters.Count == 0)
        {
            savedCharacters.Add(new CharacterData("ShadowWalker", "Rogue", 15, 2));
            savedCharacters.Add(new CharacterData("FireMage", "Mage", 8, 1));
            CharacterSaveSystem.Instance.SaveCharacters(savedCharacters);
        }
    }
    
    private void RefreshCharacterList()
    {
        if (characterListView != null)
        {
            characterListView.itemsSource = savedCharacters;
            characterListView.Rebuild();
        }
    }
    
    private void OnCharacterSelected(IEnumerable<object> selectedItems)
    {
        foreach (var item in selectedItems)
        {
            if (item is CharacterData character)
            {
                LoadCharacterData(character);
                SceneManager.LoadScene("MainGameUI");
                break;
            }
        }
    }
    
    private void LoadCharacterData(CharacterData character)
    {
        PlayerPrefs.SetString("CurrentCharacter", character.characterName);
        PlayerPrefs.SetInt("CurrentCharacterLevel", character.level);
        PlayerPrefs.SetInt("CurrentCharacterAct", character.act);
    }
    
    private void OnCreateNewCharacter()
    {
        SceneManager.LoadScene("CharacterCreation");
    }
    
    private void OnBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    private void OnDestroy()
    {
        if (createNewCharacterButton != null)
        {
            createNewCharacterButton.clicked -= OnCreateNewCharacter;
        }
        
        if (backButton != null)
        {
            backButton.clicked -= OnBackToMainMenu;
        }
    }
}
