using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    private static CharacterManager _instance;
    public static CharacterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CharacterManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CharacterManager");
                    _instance = go.AddComponent<CharacterManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Current Character")]
    public Character currentCharacter;
    
    [Header("Character Events")]
    public System.Action<Character> OnCharacterLoaded;
    public System.Action<Character> OnCharacterSaved;
    public System.Action<Character> OnCharacterLevelUp;
    public System.Action<Character> OnCharacterDamaged;
    public System.Action<Character> OnCharacterHealed;
    public System.Action<Character> OnExperienceGained;
    public System.Action<Character> OnManaUsed;
    public System.Action<Character> OnRelianceUsed;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // Load a character by name
    public void LoadCharacter(string characterName)
    {
        List<CharacterData> characterDataList = CharacterSaveSystem.Instance.LoadCharacters();
        CharacterData characterData = characterDataList.Find(c => c.characterName == characterName);
        
        if (characterData != null)
        {
            currentCharacter = Character.FromCharacterData(characterData);
            LoadCharacterFromPlayerPrefs(characterName);
            OnCharacterLoaded?.Invoke(currentCharacter);
            Debug.Log($"Loaded character: {currentCharacter.characterName}");
        }
        else
        {
            Debug.LogError($"Character {characterName} not found!");
        }
    }
    
    // Create a new character
    public void CreateCharacter(string characterName, string characterClass)
    {
        currentCharacter = new Character(characterName, characterClass);
        SaveCharacter();
        OnCharacterLoaded?.Invoke(currentCharacter);
        Debug.Log($"Created new character: {currentCharacter.characterName}");
    }
    
    // Save the current character
    public void SaveCharacter()
    {
        if (currentCharacter != null)
        {
            currentCharacter.lastSaveTime = System.DateTime.Now;
            
            // Save to CharacterSaveSystem
            CharacterData characterData = currentCharacter.ToCharacterData();
            CharacterSaveSystem.Instance.SaveCharacter(characterData);
            
            // Save additional data to PlayerPrefs
            SaveCharacterToPlayerPrefs();
            
            OnCharacterSaved?.Invoke(currentCharacter);
            Debug.Log($"Saved character: {currentCharacter.characterName}");
        }
    }
    
    // Save character data to PlayerPrefs
    private void SaveCharacterToPlayerPrefs()
    {
        string prefix = $"Character_{currentCharacter.characterName}_";
        
        PlayerPrefs.SetInt(prefix + "Experience", currentCharacter.experience);
        PlayerPrefs.SetInt(prefix + "CurrentHealth", currentCharacter.currentHealth);
        PlayerPrefs.SetInt(prefix + "Mana", currentCharacter.mana);
        PlayerPrefs.SetInt(prefix + "Reliance", currentCharacter.reliance);
        PlayerPrefs.SetInt(prefix + "Strength", currentCharacter.strength);
        PlayerPrefs.SetInt(prefix + "Dexterity", currentCharacter.dexterity);
        PlayerPrefs.SetInt(prefix + "Intelligence", currentCharacter.intelligence);
        PlayerPrefs.SetString(prefix + "CurrentScene", currentCharacter.currentScene);
        PlayerPrefs.SetString(prefix + "LastPosition", JsonUtility.ToJson(currentCharacter.lastPosition));
        
        PlayerPrefs.Save();
    }
    
    // Load character data from PlayerPrefs
    private void LoadCharacterFromPlayerPrefs(string characterName)
    {
        string prefix = $"Character_{characterName}_";
        
        currentCharacter.experience = PlayerPrefs.GetInt(prefix + "Experience", 0);
        currentCharacter.currentHealth = PlayerPrefs.GetInt(prefix + "CurrentHealth", currentCharacter.maxHealth);
        currentCharacter.mana = PlayerPrefs.GetInt(prefix + "Mana", currentCharacter.maxMana);
        currentCharacter.reliance = PlayerPrefs.GetInt(prefix + "Reliance", currentCharacter.maxReliance);
        currentCharacter.strength = PlayerPrefs.GetInt(prefix + "Strength", currentCharacter.strength);
        currentCharacter.dexterity = PlayerPrefs.GetInt(prefix + "Dexterity", currentCharacter.dexterity);
        currentCharacter.intelligence = PlayerPrefs.GetInt(prefix + "Intelligence", currentCharacter.intelligence);
        currentCharacter.currentScene = PlayerPrefs.GetString(prefix + "CurrentScene", "MainGameUI");
        
        string lastPosJson = PlayerPrefs.GetString(prefix + "LastPosition", "");
        if (!string.IsNullOrEmpty(lastPosJson))
        {
            currentCharacter.lastPosition = JsonUtility.FromJson<Vector3>(lastPosJson);
        }
        
        currentCharacter.CalculateDerivedStats();
    }
    
    // Get current character (null-safe)
    public Character GetCurrentCharacter()
    {
        return currentCharacter;
    }
    
    // Check if a character is loaded
    public bool HasCharacter()
    {
        return currentCharacter != null;
    }
    
    // Clear current character (for logout/character switch)
    public void ClearCurrentCharacter()
    {
        if (currentCharacter != null)
        {
            SaveCharacter(); // Save before clearing
        }
        currentCharacter = null;
    }
    
    // Character combat and progression methods
    public void TakeDamage(int damage)
    {
        if (currentCharacter != null)
        {
            currentCharacter.TakeDamage(damage);
            OnCharacterDamaged?.Invoke(currentCharacter);
        }
    }
    
    public void Heal(int amount)
    {
        if (currentCharacter != null)
        {
            currentCharacter.Heal(amount);
            OnCharacterHealed?.Invoke(currentCharacter);
        }
    }
    
    public void AddExperience(int exp)
    {
        if (currentCharacter != null)
        {
            int oldLevel = currentCharacter.level;
            currentCharacter.AddExperience(exp);
            
            OnExperienceGained?.Invoke(currentCharacter);
            
            if (currentCharacter.level > oldLevel)
            {
                OnCharacterLevelUp?.Invoke(currentCharacter);
            }
        }
    }
    
    public bool UseMana(int amount)
    {
        if (currentCharacter != null)
        {
            bool success = currentCharacter.UseMana(amount);
            if (success)
            {
                OnManaUsed?.Invoke(currentCharacter);
            }
            return success;
        }
        return false;
    }
    
    public void RestoreMana(int amount)
    {
        if (currentCharacter != null)
        {
            currentCharacter.RestoreMana(amount);
        }
    }
    
    public bool UseReliance(int amount)
    {
        if (currentCharacter != null)
        {
            bool success = currentCharacter.UseReliance(amount);
            if (success)
            {
                OnRelianceUsed?.Invoke(currentCharacter);
            }
            return success;
        }
        return false;
    }
    
    public void RestoreReliance(int amount)
    {
        if (currentCharacter != null)
        {
            currentCharacter.RestoreReliance(amount);
        }
    }
    
    // Utility methods
    public bool IsCharacterAlive()
    {
        return currentCharacter != null && currentCharacter.currentHealth > 0;
    }
    
    public float GetHealthPercentage()
    {
        if (currentCharacter != null)
        {
            return (float)currentCharacter.currentHealth / currentCharacter.maxHealth;
        }
        return 0f;
    }
    
    public float GetManaPercentage()
    {
        if (currentCharacter != null)
        {
            return (float)currentCharacter.mana / currentCharacter.maxMana;
        }
        return 0f;
    }
    
    public float GetReliancePercentage()
    {
        if (currentCharacter != null)
        {
            return (float)currentCharacter.reliance / currentCharacter.maxReliance;
        }
        return 0f;
    }
    
    public float GetExperiencePercentage()
    {
        if (currentCharacter != null)
        {
            return (float)currentCharacter.experience / currentCharacter.GetRequiredExperience();
        }
        return 0f;
    }
}
