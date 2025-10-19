# Character-Deck Integration System

## Overview

This guide explains how cards, decks, and characters are connected, saved, and loaded in the game.

---

## System Architecture

```
Character (Save Data)
├── CharacterDeckData
│   ├── unlockedCards (List<string>) ← Cards the character owns
│   ├── activeDeckName (string) ← Currently equipped deck
│   └── savedDeckNames (List<string>) ← Character's saved deck presets
│
DeckManager (Singleton - Persists across scenes)
├── activeDeck (DeckPreset) ← Currently active deck for combat
└── savedDecks (List<DeckPreset>) ← All saved deck presets (JSON files)
│
CardDatabase (ScriptableObject - Global card pool)
└── allCards (List<CardData>) ← All cards in the game
```

---

## Data Flow

### **When Creating a New Character:**

```
1. CharacterManager.CreateCharacter("PlayerName", "Marauder")
   ↓
2. Character.deckData = new CharacterDeckData()
   ↓
3. StarterCardCollection.GetStarterCards("Marauder")
   → Returns ["Heavy Strike", "Cleave", ...]
   ↓
4. Character.deckData.InitializeStarterCollection(starterCards)
   ↓
5. DeckManager.CreateStarterDeck(character)
   → Creates "Marauder Starter" DeckPreset
   ↓
6. Character.deckData.activeDeckName = "Marauder Starter"
7. Character.deckData.savedDeckNames.Add("Marauder Starter")
```

### **When Loading a Character:**

```
1. CharacterManager.LoadCharacter("PlayerName")
   ↓
2. Load Character.deckData from save file
   ↓
3. DeckManager.LoadCharacterDecks(character)
   ↓
4. Load activeDeck from JSON based on activeDeckName
   ↓
5. Deck is ready for combat
```

### **When Building a Deck:**

```
1. Player opens Deck Builder scene
   ↓
2. DeckBuilderUI loads character.deckData.unlockedCards
   ↓
3. Filter CardDatabase.allCards to show only unlocked cards
   ↓
4. Player builds deck
   ↓
5. DeckManager.SaveDeck(deck)
   → Saves as JSON: "MyDeck.json"
   ↓
6. Character.deckData.savedDeckNames.Add("MyDeck")
   ↓
7. CharacterManager.SaveCharacter()
   → Saves character with updated deck list
```

### **When Entering Combat:**

```
1. SceneManager.LoadScene("Combat")
   ↓
2. CombatManager.InitializeCombat()
   ↓
3. DeckManager.GetActiveDeck()
   → Returns DeckPreset based on character.deckData.activeDeckName
   ↓
4. DeckManager.GetActiveDeckAsCards()
   → Converts DeckPreset (CardData) → List<Card> (for combat)
   ↓
5. Combat uses the deck
```

---

## Implementation: Character Save Integration

### **Step 1: Update CharacterManager**

Add deck loading/saving methods:

```csharp
// In CharacterManager.cs

public void CreateCharacter(string characterName, string characterClass)
{
    currentCharacter = new Character(characterName, characterClass);
    
    // Initialize starter cards
    List<string> starterCards = StarterCardCollection.GetStarterCards(characterClass);
    currentCharacter.deckData.InitializeStarterCollection(starterCards);
    
    // Create starter deck
    CreateStarterDeck(currentCharacter);
    
    SaveCharacter();
    OnCharacterLoaded?.Invoke(currentCharacter);
}

private void CreateStarterDeck(Character character)
{
    // Get starter deck from StarterDeckManager (existing system)
    Deck starterDeck = StarterDeckManager.Instance.GetStarterDeck(character.characterClass);
    
    if (starterDeck != null)
    {
        // Convert to DeckPreset
        DeckPreset deckPreset = ConvertToDeckPreset(starterDeck, $"{character.characterClass} Starter");
        
        // Save it
        DeckManager.Instance.SaveDeck(deckPreset);
        
        // Link to character
        character.deckData.activeDeckName = deckPreset.deckName;
        character.deckData.savedDeckNames.Add(deckPreset.deckName);
        
        // Set as active
        DeckManager.Instance.SetActiveDeck(deckPreset);
    }
}

private DeckPreset ConvertToDeckPreset(Deck deck, string name)
{
    DeckPreset preset = ScriptableObject.CreateInstance<DeckPreset>();
    preset.deckName = name;
    preset.description = $"Starter deck for {deck.characterClass}";
    preset.characterClass = deck.characterClass;
    
    // Convert Card → CardData and add to preset
    CardDatabase db = CardDatabase.Instance;
    foreach (Card card in deck.cards)
    {
        CardData cardData = db.allCards.Find(c => c.cardName == card.cardName);
        if (cardData != null)
        {
            preset.AddCard(cardData, 1);
        }
    }
    
    return preset;
}

public void LoadCharacter(string characterName)
{
    // ... existing load code ...
    
    // Load character's decks
    LoadCharacterDecks(currentCharacter);
}

private void LoadCharacterDecks(Character character)
{
    if (character == null || character.deckData == null) return;
    
    // Load active deck
    if (!string.IsNullOrEmpty(character.deckData.activeDeckName))
    {
        string fileName = character.deckData.activeDeckName + ".json";
        DeckPreset activeDeck = DeckManager.Instance.LoadDeck(fileName);
        
        if (activeDeck != null)
        {
            DeckManager.Instance.SetActiveDeck(activeDeck);
        }
        else
        {
            Debug.LogWarning($"Active deck '{character.deckData.activeDeckName}' not found. Creating default.");
            CreateStarterDeck(character);
        }
    }
}
```

---

### **Step 2: Update DeckBuilderUI**

Filter cards based on character ownership:

```csharp
// In DeckBuilderUI.cs

private void ApplyFilters()
{
    filteredCards.Clear();
    
    if (cardDatabase == null || cardDatabase.allCards == null)
    {
        return;
    }
    
    // Get current character
    Character currentCharacter = CharacterManager.Instance.GetCurrentCharacter();
    
    foreach (CardData card in cardDatabase.allCards)
    {
        // Check if character owns this card
        if (currentCharacter != null && !currentCharacter.deckData.OwnsCard(card))
        {
            continue; // Skip locked cards
        }
        
        if (PassesFilters(card))
        {
            filteredCards.Add(card);
        }
    }
    
    // Sort by cost, then by name
    filteredCards = filteredCards
        .OrderBy(c => c.playCost)
        .ThenBy(c => c.cardName)
        .ToList();
}
```

---

### **Step 3: Update DeckManager Save/Load**

Tie decks to characters:

```csharp
// In DeckManager.cs

public bool SaveDeck(DeckPreset deck, string customFileName = "")
{
    if (deck == null)
    {
        Debug.LogError("Cannot save null deck.");
        return false;
    }
    
    try
    {
        string fileName = string.IsNullOrEmpty(customFileName) 
            ? SanitizeFileName(deck.deckName) + ".json"
            : customFileName;
        
        string filePath = Path.Combine(SavePath, fileName);
        string json = deck.ExportToJSON();
        
        File.WriteAllText(filePath, json);
        
        // Add to saved decks if not already present
        if (!savedDecks.Contains(deck))
        {
            savedDecks.Add(deck);
        }
        
        // Update character's deck list
        Character currentCharacter = CharacterManager.Instance.GetCurrentCharacter();
        if (currentCharacter != null)
        {
            currentCharacter.deckData.AddDeck(deck.deckName);
            CharacterManager.Instance.SaveCharacter(); // Save character with updated deck list
        }
        
        OnDeckSaved?.Invoke(deck);
        Debug.Log($"Deck saved: {filePath}");
        return true;
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to save deck '{deck.deckName}': {e.Message}");
        return false;
    }
}

public void SetActiveDeck(DeckPreset deck)
{
    if (deck == null)
    {
        Debug.LogWarning("Cannot set null deck as active.");
        return;
    }
    
    string errorMessage;
    if (!deck.IsValidDeck(out errorMessage))
    {
        Debug.LogError($"Cannot activate invalid deck: {errorMessage}");
        return;
    }
    
    activeDeck = deck;
    
    // Update character's active deck
    Character currentCharacter = CharacterManager.Instance.GetCurrentCharacter();
    if (currentCharacter != null)
    {
        currentCharacter.deckData.SetActiveDeck(deck.deckName);
        CharacterManager.Instance.SaveCharacter();
    }
    
    OnDeckLoaded?.Invoke(activeDeck);
    Debug.Log($"Active deck set to: {deck.deckName}");
}
```

---

## Card Unlocking System

### **Unlocking Cards (Rewards, Shop, etc.)**

```csharp
// Example: Unlock a card after defeating a boss
public class BossRewardSystem : MonoBehaviour
{
    public void GrantBossReward(string cardName)
    {
        Character character = CharacterManager.Instance.GetCurrentCharacter();
        
        if (character != null)
        {
            character.deckData.UnlockCard(cardName);
            CharacterManager.Instance.SaveCharacter();
            
            // Show UI notification
            ShowCardUnlockedPopup(cardName);
        }
    }
    
    public void UnlockMultipleCards(List<string> cardNames)
    {
        Character character = CharacterManager.Instance.GetCurrentCharacter();
        
        if (character != null)
        {
            character.deckData.UnlockCards(cardNames);
            CharacterManager.Instance.SaveCharacter();
        }
    }
}
```

### **Debug: Unlock All Cards**

```csharp
// For testing - give character all cards
[ContextMenu("Debug: Unlock All Cards")]
public void DebugUnlockAllCards()
{
    Character character = CharacterManager.Instance.GetCurrentCharacter();
    if (character != null)
    {
        character.deckData.UnlockAllCards();
        CharacterManager.Instance.SaveCharacter();
        Debug.Log("All cards unlocked for testing");
    }
}
```

---

## Save File Structure

### **Character Save (JSON)**

```json
{
  "characterName": "MyCharacter",
  "characterClass": "Marauder",
  "level": 5,
  "deckData": {
    "activeDeckName": "Berserker Build",
    "savedDeckNames": [
      "Marauder Starter",
      "Berserker Build",
      "Tank Build"
    ],
    "unlockedCards": [
      "Heavy Strike",
      "Cleave",
      "Ground Slam",
      "Fireball",
      "Ice Shield",
      ...
    ],
    "hasAllCards": false,
    "deckSlotsUnlocked": 5
  }
}
```

### **Deck Preset (JSON)**

```json
{
  "deckName": "Berserker Build",
  "description": "High damage melee deck",
  "characterClass": "Marauder",
  "version": 1,
  "cardEntries": [
    {
      "cardName": "Heavy Strike",
      "quantity": 6
    },
    {
      "cardName": "Cleave",
      "quantity": 4
    },
    ...
  ]
}
```

---

## Common Workflows

### **Adding Cards to the Game**

1. Create CardData asset in Unity
2. Add artwork
3. Configure stats
4. Add to CardDatabase
5. (Optional) Add to starter collection for a class

### **Character Gets New Cards**

1. Defeat boss / complete quest / buy from shop
2. Call `character.deckData.UnlockCard(cardName)`
3. Call `CharacterManager.Instance.SaveCharacter()`
4. Card now appears in Deck Builder for that character

### **Character Builds a Deck**

1. Open Deck Builder
2. UI shows only `character.deckData.unlockedCards`
3. Player builds deck
4. Save deck → adds to `character.deckData.savedDeckNames`
5. Character save file updated

### **Character Switches Deck**

1. In Deck Builder, select different deck from dropdown
2. Click "Done"
3. `DeckManager.SetActiveDeck(deck)`
4. Updates `character.deckData.activeDeckName`
5. Character save file updated

### **Character Enters Combat**

1. Load combat scene
2. `DeckManager.GetActiveDeck()` → loads based on `character.deckData.activeDeckName`
3. Convert to `List<Card>` for combat system
4. Combat uses the deck

---

## Troubleshooting

### **Issue: Character can see all cards in Deck Builder**

**Fix:** Character.deckData.hasAllCards is set to true. Set to false or properly filter cards:

```csharp
if (currentCharacter != null && !currentCharacter.deckData.OwnsCard(card))
{
    continue; // Skip locked cards
}
```

### **Issue: Deck not persisting across scenes**

**Fix:** Ensure DeckManager has DontDestroyOnLoad and SetActiveDeck is called before scene change.

### **Issue: Cards missing after character load**

**Fix:** Ensure cardNames in `unlockedCards` match exactly (case-sensitive). Validate with:

```csharp
StarterCardCollection.ValidateStarterCards(characterClass, CardDatabase.Instance);
```

### **Issue: Deck file corrupted/not found**

**Fix:** DeckManager checks for missing decks and creates default:

```csharp
if (activeDeck == null)
{
    CreateStarterDeck(character);
}
```

---

## Testing Checklist

- [ ] Create new character → Has starter cards unlocked
- [ ] Create new character → Has starter deck created
- [ ] Build a deck → Saves to character
- [ ] Switch characters → Decks are separate
- [ ] Unlock new card → Appears in Deck Builder
- [ ] Save/Load character → Deck data persists
- [ ] Enter combat → Active deck loads correctly
- [ ] Delete deck → Removed from character's saved list

---

## Future Enhancements

1. **Card Collection UI** - Show all cards (locked/unlocked) with unlock conditions
2. **Deck Slots Progression** - Unlock more deck slots as character levels up
3. **Deck Templates** - Premade decks players can import
4. **Card Trading** - Between characters (if multiplayer)
5. **Deck Statistics** - Win rate tracking per deck
6. **Auto-Deck Builder** - AI suggests optimal decks based on unlocked cards

---

## Summary

**Card Storage:** CardData ScriptableObjects in `Assets/Resources/Cards/`  
**Card Unlocking:** `Character.deckData.unlockedCards` (character-specific)  
**Deck Storage:** JSON files in `Application.persistentDataPath/DeckPresets/`  
**Active Deck:** `DeckManager.activeDeck` (persists across scenes)  
**Character Link:** `Character.deckData.activeDeckName` + `savedDeckNames`  
**Save Integration:** CharacterManager saves deckData with character  

Everything is tied together through the character save system! 🎮✨








