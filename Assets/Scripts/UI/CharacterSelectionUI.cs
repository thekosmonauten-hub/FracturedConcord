using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect characterScrollView;
    public Transform characterListContent; // The content area inside the ScrollView
    public GameObject characterButtonPrefab; // Prefab for individual character buttons
    public Button createNewCharacterButton;
    public Button backToMainMenuButton;
    
    [Header("Character Data")]
    public List<CharacterData> savedCharacters = new List<CharacterData>();
    
    private void Start()
    {
        SetupUI();
        LoadSavedCharacters();
        RefreshCharacterList();
    }
    
    private void SetupUI()
    {
        // Set up button events
        if (createNewCharacterButton != null)
        {
            createNewCharacterButton.onClick.AddListener(OnCreateNewCharacter);
        }
        
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenu);
        }
    }
    
    private void LoadSavedCharacters()
    {
        // Load saved characters using the save system
        savedCharacters = CharacterSaveSystem.Instance.LoadCharacters();
        
        // For testing, add some sample characters if none exist
        if (savedCharacters.Count == 0)
        {
            savedCharacters.Add(new CharacterData("ShadowWalker", "Rogue", 15, 2));
            savedCharacters.Add(new CharacterData("FireMage", "Mage", 8, 1));
            
            // Save the sample characters
            CharacterSaveSystem.Instance.SaveCharacters(savedCharacters);
        }
    }
    
    private void RefreshCharacterList()
    {
        // Clear existing character buttons
        ClearCharacterList();
        
        // Create buttons for each saved character
        foreach (CharacterData character in savedCharacters)
        {
            CreateCharacterButton(character);
        }
        
        // Update scroll view content size
        UpdateScrollViewSize();
    }
    
    private void ClearCharacterList()
    {
        if (characterListContent != null)
        {
            // Destroy all child objects (character buttons)
            for (int i = characterListContent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(characterListContent.GetChild(i).gameObject);
            }
        }
    }
    
    private void CreateCharacterButton(CharacterData character)
    {
        if (characterButtonPrefab == null || characterListContent == null)
        {
            Debug.LogError("Character button prefab or content area not assigned!");
            return;
        }
        
        // Instantiate the character button
        GameObject buttonObj = Instantiate(characterButtonPrefab, characterListContent);
        Button characterButton = buttonObj.GetComponent<Button>();
        
        // Set up the button text and data
        SetupCharacterButton(characterButton, character);
        
        // Add click event
        characterButton.onClick.AddListener(() => OnCharacterSelected(character));
    }
    
    private void SetupCharacterButton(Button button, CharacterData character)
    {
        // Find and update the text component
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = $"{character.characterName} - Level {character.level} (Act {character.act})";
        }
        
        // You can also set up additional UI elements like:
        // - Character class icon
        // - Character portrait
        // - Progress indicators
        // - Delete button (if needed)
    }
    
    private void UpdateScrollViewSize()
    {
        if (characterListContent != null && characterScrollView != null)
        {
            // Calculate the total height needed for all character buttons
            float totalHeight = savedCharacters.Count * 80f; // Assuming 80px per button
            
            // Set the content size fitter or manually adjust the content height
            ContentSizeFitter sizeFitter = characterListContent.GetComponent<ContentSizeFitter>();
            if (sizeFitter != null)
            {
                sizeFitter.SetLayoutVertical();
            }
        }
    }
    
    private void OnCharacterSelected(CharacterData character)
    {
        Debug.Log($"Selected character: {character.characterName}");
        
        // Load the character's save data
        LoadCharacterData(character);
        
        // Transition to the main game scene
        SceneManager.LoadScene("MainGameUI");
    }
    
    private void LoadCharacterData(CharacterData character)
    {
        // Load the character's specific save data
        PlayerPrefs.SetString("CurrentCharacter", character.characterName);
        PlayerPrefs.SetInt("CurrentCharacterLevel", character.level);
        PlayerPrefs.SetInt("CurrentCharacterAct", character.act);
        
        // Load additional character data (deck, equipment, etc.)
        // This will depend on your save system implementation
    }
    
    private void OnCreateNewCharacter()
    {
        Debug.Log("Creating new character...");
        
        // Transition to character creation scene
        SceneManager.LoadScene("CharacterCreation");
    }
    
    private void OnBackToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        
        // Return to the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (createNewCharacterButton != null)
        {
            createNewCharacterButton.onClick.RemoveListener(OnCreateNewCharacter);
        }
        
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.RemoveListener(OnBackToMainMenu);
        }
    }
}

[System.Serializable]
public class CharacterData
{
    public string characterName;
    public string characterClass;
    public int level;
    public int act;
    public string saveDate;
    
    // Core Stats
    public int experience;
    public int skillPoints;
    public int strength;
    public int dexterity;
    public int intelligence;
    
    // Resources
    public int currentHealth;
    public int maxHealth;
    public int currentEnergyShield;
    public int maxEnergyShield;
    public int mana;
    public int maxMana;
    public int manaRecoveryPerTurn;
    public int cardsDrawnPerTurn;
    public int reliance;
    public int maxReliance;
    
    // Game State
    public string currentScene;
    public float lastPosX, lastPosY, lastPosZ;
    
    public CharacterData(string name, string characterClass, int level, int act)
    {
        this.characterName = name;
        this.characterClass = characterClass;
        this.level = level;
        this.act = act;
        this.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }
}
