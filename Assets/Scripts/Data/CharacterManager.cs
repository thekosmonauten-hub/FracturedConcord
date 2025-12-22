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
    
    [Header("Inventory")]
    public List<BaseItem> inventoryItems = new List<BaseItem>();
    
    [Header("Character Events")]
    public System.Action<Character> OnCharacterLoaded;
    public System.Action<Character> OnCharacterSaved;
    public System.Action<Character> OnCharacterLevelUp;
    public System.Action<Character> OnCharacterDamaged;
    public System.Action<Character> OnCharacterHealed;
    public System.Action<Character> OnExperienceGained;
    public System.Action<Character> OnManaUsed;
    public System.Action<Character> OnRelianceUsed;
    public System.Action<string> OnCardUnlocked; // card name
    public System.Action<BaseItem> OnItemAdded; // inventory item added
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            // Attempt to restore character if not already set (scene reloads)
            if (currentCharacter == null)
            {
                EnsureCharacterLoadedFromPrefs();
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // Load a character by name
    public void LoadCharacter(string characterName)
    {
        // Clear currencies before loading new character (prevent currency bleed between characters)
        if (LootManager.Instance != null)
        {
            LootManager.Instance.ClearAllCurrencies();
        }
        
        // Clear inventory before loading new character
        inventoryItems.Clear();
        
        List<CharacterData> characterDataList = CharacterSaveSystem.Instance.LoadCharacters();
        CharacterData characterData = characterDataList.Find(c => c.characterName == characterName);
        
        if (characterData != null)
        {
            currentCharacter = Character.FromCharacterData(characterData);
            LoadCharacterFromPlayerPrefs(characterName);
            // Fallback: if no starter cards unlocked yet, initialize from StarterDeckDefinition
            try
            {
                if (currentCharacter != null && (currentCharacter.deckData == null || currentCharacter.deckData.unlockedCards.Count == 0))
                {
                    if (currentCharacter.deckData == null)
                    {
                        currentCharacter.deckData = new CharacterDeckData();
                    }
                    var sdm = StarterDeckManager.Instance;
                    if (sdm != null)
                    {
                        sdm.AssignStarterToCharacterDeckData(currentCharacter);
                    }
                }
            }
            catch { }
            
            // Load character's deck presets
            LoadCharacterDecks(currentCharacter);
            
            // Load card experience data
            if (CardExperienceManager.Instance != null)
            {
                CardExperienceManager.Instance.LoadFromCharacter(currentCharacter);
            }
            
            // Load aura experience data
            if (AuraExperienceManager.Instance != null)
            {
                AuraExperienceManager.Instance.LoadFromCharacter(currentCharacter);
            }
            
            OnCharacterLoaded?.Invoke(currentCharacter);
            Debug.Log($"Loaded character: {currentCharacter.characterName}");
            
            // Refresh warrant modifiers after loading character
            // Note: This may fail if warrant board components aren't loaded yet (e.g., not in WarrantTree scene)
            // That's okay - modifiers will be refreshed when the warrant board is accessed
            if (currentCharacter != null)
            {
                currentCharacter.RefreshWarrantModifiers();
            }
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
        
        InitializeStarterDeck(currentCharacter);
        
        // IMPORTANT: Do NOT automatically give warrant starter pack here.
        // The warrant starter pack should ONLY be given through Joreg's dialogue
        // when the player selects the "Warrants?" option in PeacekeeperJoreg.asset dialogue.
        // See DialogueManager.GiveWarrantPack() for the implementation.
        
        // For new characters, only unlock encounter 1 by default
        // Other encounters will unlock as prerequisites are completed
        // Note: We mark it unlocked here, but ApplyCharacterProgression will also ensure it's unlocked
        currentCharacter.MarkEncounterUnlocked(1);
        
        // Refresh encounter manager to apply progression
        // This will detect the new character and ensure encounter 1 is unlocked
        var encounterMgr = EncounterManager.Instance;
        if (encounterMgr != null)
        {
            // Ensure encounters are loaded and graph is initialized
            // InitializeEncounterGraph will populate encounterNodes and apply progression
            encounterMgr.InitializeEncounterGraph();
            
            // Double-check that encounter 1 is unlocked (in case graph wasn't ready)
            var encounter1 = encounterMgr.GetEncounterByID(1);
            if (encounter1 != null && !encounter1.isUnlocked)
                {
                encounter1.isUnlocked = true;
                currentCharacter.MarkEncounterUnlocked(1);
                Debug.Log($"[CharacterManager] Force-unlocked encounter 1 for new character: {encounter1.encounterName}");
            }
        }
        
        SaveCharacter();
        OnCharacterLoaded?.Invoke(currentCharacter);
        Debug.Log($"Created new character: {currentCharacter.characterName} (Encounter 1 unlocked)");
        PlayerPrefs.SetString("LastCharacterName", characterName);
        PlayerPrefs.Save();
    }
    
    // Save the current character
    public void SaveCharacter()
    {
        if (currentCharacter != null)
        {
            currentCharacter.lastSaveTime = System.DateTime.Now;
            
            // Save card experience data
            if (CardExperienceManager.Instance != null)
            {
                CardExperienceManager.Instance.SaveToCharacter(currentCharacter);
            }
            
            // Save aura experience data
            if (AuraExperienceManager.Instance != null)
            {
                AuraExperienceManager.Instance.SaveToCharacter(currentCharacter);
            }
            
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
        PlayerPrefs.SetString("LastCharacterName", currentCharacter.characterName);
        
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

    // Ensure a character is present by restoring from PlayerPrefs if needed
    public void EnsureCharacterLoadedFromPrefs()
    {
        if (currentCharacter != null) return;
        string last = PlayerPrefs.GetString("LastCharacterName", string.Empty);
        if (!string.IsNullOrWhiteSpace(last))
        {
            LoadCharacter(last);
        }
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
    
    public void AddExperience(int exp, bool shareWithCards = true)
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
            
            // Also give experience to cards in active deck
            if (shareWithCards && CardExperienceManager.Instance != null)
            {
                CardExperienceManager.Instance.ApplyCombatExperience(exp);
            }
            
            // Also give experience to active auras
            if (shareWithCards && AuraExperienceManager.Instance != null)
            {
                AuraExperienceManager.Instance.ApplyCombatExperience(exp);
            }
        }
    }

    // Unlock a card by name for the current character and notify listeners
    public void UnlockCard(string cardName)
    {
        if (currentCharacter == null || string.IsNullOrWhiteSpace(cardName)) return;
        if (currentCharacter.deckData == null) currentCharacter.deckData = new CharacterDeckData();
        currentCharacter.deckData.UnlockCard(cardName);
        OnCardUnlocked?.Invoke(cardName);
        Debug.Log($"[Cards] Unlocked card: {cardName} for {currentCharacter.characterName}");
    }

    // Add an item to the player's inventory and notify listeners
    public void AddItem(BaseItem item)
    {
        if (item == null) return;
        inventoryItems.Add(item);
        OnItemAdded?.Invoke(item);
        Debug.Log($"[Loot] Added item to inventory: {item.itemName} ({item.rarity})");
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
    
    #region Deck Management
    /// <summary>
    /// Initialize starter deck for a new character
    /// Creates a DeckPreset from the StarterDeckDefinition and sets it as active
    /// </summary>
    private void InitializeStarterDeck(Character character)
    {
        if (character == null) return;
        
        // Initialize deck data if needed
        if (character.deckData == null)
        {
            character.deckData = new CharacterDeckData();
        }
        
        // Get starter deck manager
        StarterDeckManager sdm = StarterDeckManager.Instance;
        if (sdm == null)
        {
            Debug.LogWarning("StarterDeckManager not found. Cannot initialize starter deck.");
            return;
        }
        
        // Assign starter cards to character's collection
        sdm.AssignStarterToCharacterDeckData(character);
        
        // Get the starter deck definition
        StarterDeckDefinition starterDef = sdm.GetDefinitionForClass(character.characterClass);
        if (starterDef == null)
        {
            Debug.LogWarning($"No StarterDeckDefinition found for class: {character.characterClass}");
            return;
        }
        
        // Create DeckPreset from starter definition
        DeckPreset starterDeckPreset = CreateDeckPresetFromStarterDefinition(starterDef, character.characterClass);
        if (starterDeckPreset == null)
        {
            Debug.LogError("Failed to create starter deck preset.");
            return;
        }
        
        // Save the deck preset to disk
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.SaveDeck(starterDeckPreset);
            
            // Ensure the character knows about this deck
            if (!character.deckData.savedDeckNames.Contains(starterDeckPreset.deckName))
            {
                character.deckData.AddDeck(starterDeckPreset.deckName);
            }
            character.deckData.SetActiveDeck(starterDeckPreset.deckName);
            
            // Set as active deck in DeckManager (this will also sync back to character data)
            DeckManager.Instance.SetActiveDeck(starterDeckPreset);
            
            Debug.Log($"Created and activated starter deck: {starterDeckPreset.deckName} for {character.characterName}");
        }
    }
    
    /// <summary>
    /// Create a DeckPreset from a StarterDeckDefinition
    /// </summary>
    private DeckPreset CreateDeckPresetFromStarterDefinition(StarterDeckDefinition definition, string characterClass)
    {
        if (definition == null) return null;
        
        // Create new DeckPreset
        DeckPreset preset = ScriptableObject.CreateInstance<DeckPreset>();
        preset.deckName = $"{characterClass} Starter";
        preset.description = $"Starter deck for the {characterClass} class";
        preset.characterClass = characterClass;
        preset.createdDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        preset.lastModifiedDate = preset.createdDate;
        
        // Get card database
        CardDatabase cardDatabase = CardDatabase.Instance;
        if (cardDatabase == null)
        {
            Debug.LogError("CardDatabase not found!");
            return null;
        }
        
        // Add cards from definition
        foreach (var entry in definition.cards)
        {
            if (entry == null || entry.card == null) continue;
            
            // Find the CardData in the database by name
            CardData cardData = cardDatabase.allCards.Find(c => c.cardName == entry.card.cardName);
            
            if (cardData != null)
            {
                preset.AddCard(cardData, entry.count);
            }
            else
            {
                Debug.LogWarning($"Card not found in database: {entry.card.cardName}");
            }
        }
        
        Debug.Log($"Created DeckPreset '{preset.deckName}' with {preset.GetTotalCardCount()} cards");
        return preset;
    }
    
    /// <summary>
    /// Load character's decks from disk when loading a saved character
    /// Sets the active deck based on character.deckData.activeDeckName
    /// </summary>
    public void LoadCharacterDecks(Character character)
    {
        if (character == null || character.deckData == null) return;
        
        string activeDeckName = character.deckData.activeDeckName;
        if (string.IsNullOrEmpty(activeDeckName))
        {
            Debug.Log("No active deck name set for character");
            return;
        }
        
        // Try to load the active deck from DeckManager
        if (DeckManager.Instance != null)
        {
            // Load all saved decks first
            DeckManager.Instance.LoadAllDecks();
            
            // Try to find and set the active deck
            DeckPreset activeDeck = DeckManager.Instance.GetAllDecks()
                .Find(d => d.deckName == activeDeckName);
            
            if (activeDeck != null)
            {
                DeckManager.Instance.SetActiveDeck(activeDeck);
                Debug.Log($"Loaded active deck: {activeDeckName} for {character.characterName}");
            }
            else
            {
                Debug.LogWarning($"Active deck '{activeDeckName}' not found. May need to recreate starter deck.");
                
                // If it's a starter deck that doesn't exist, recreate it
                if (activeDeckName.Contains("Starter"))
                {
                    InitializeStarterDeck(character);
                }
            }
        }
    }
    #endregion
}
