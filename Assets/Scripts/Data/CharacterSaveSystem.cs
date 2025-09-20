using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class CharacterSaveSystem : MonoBehaviour
{
    private static CharacterSaveSystem _instance;
    public static CharacterSaveSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CharacterSaveSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CharacterSaveSystem");
                    _instance = go.AddComponent<CharacterSaveSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "characters.json");
    
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
    
    public void SaveCharacters(List<CharacterData> characters)
    {
        try
        {
            // Convert list to JSON
            string json = JsonUtility.ToJson(new CharacterListWrapper { characters = characters }, true);
            
            // Write to file
            File.WriteAllText(SaveFilePath, json);
            
            Debug.Log($"Saved {characters.Count} characters to {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save characters: {e.Message}");
        }
    }
    
    public List<CharacterData> LoadCharacters()
    {
        List<CharacterData> characters = new List<CharacterData>();
        
        try
        {
            if (File.Exists(SaveFilePath))
            {
                string json = File.ReadAllText(SaveFilePath);
                CharacterListWrapper wrapper = JsonUtility.FromJson<CharacterListWrapper>(json);
                characters = wrapper.characters;
                
                Debug.Log($"Loaded {characters.Count} characters from {SaveFilePath}");
            }
            else
            {
                Debug.Log("No character save file found, starting with empty list");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load characters: {e.Message}");
        }
        
        return characters;
    }
    
    public void SaveCharacter(CharacterData character)
    {
        List<CharacterData> characters = LoadCharacters();
        
        // Check if character already exists
        int existingIndex = characters.FindIndex(c => c.characterName == character.characterName);
        
        if (existingIndex >= 0)
        {
            // Update existing character
            characters[existingIndex] = character;
        }
        else
        {
            // Add new character
            characters.Add(character);
        }
        
        SaveCharacters(characters);
    }
    
    public void DeleteCharacter(string characterName)
    {
        List<CharacterData> characters = LoadCharacters();
        int removedCount = characters.RemoveAll(c => c.characterName == characterName);
        
        if (removedCount > 0)
        {
            SaveCharacters(characters);
            Debug.Log($"Deleted character: {characterName}");
        }
        else
        {
            Debug.LogWarning($"Character not found for deletion: {characterName}");
        }
    }
    
    public CharacterData GetCharacter(string characterName)
    {
        List<CharacterData> characters = LoadCharacters();
        return characters.Find(c => c.characterName == characterName);
    }
    
    public bool CharacterExists(string characterName)
    {
        return GetCharacter(characterName) != null;
    }
}

[System.Serializable]
public class CharacterListWrapper
{
    public List<CharacterData> characters = new List<CharacterData>();
}
