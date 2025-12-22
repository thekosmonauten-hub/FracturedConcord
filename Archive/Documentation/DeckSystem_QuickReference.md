# Deck System - Quick Reference

## 🎯 TL;DR - Quick Answers

### **Where do I store cards?**
```
Assets/Resources/Cards/
├── Attack/
│   ├── Fireball.asset
│   ├── SwordStrike.asset
│   └── ...
├── Guard/
├── Skill/
└── Power/
```

### **How do I add a new card?**
1. Right-Click in `Assets/Resources/Cards/Attack/` → **Create → Dexiled → Cards → Card Data**
2. Name it (e.g., "Fireball")
3. Fill in the Inspector (name, cost, damage, artwork, etc.)
4. Add to `CardDatabase.asset` → **All Cards** list
5. Done! It appears in Deck Builder automatically

### **How are cards tied to character saves?**
```
Character.deckData.unlockedCards  ← List of card names the character owns
Character.deckData.activeDeckName ← Name of currently equipped deck
Character.deckData.savedDeckNames ← List of deck presets this character saved
```

When you save the character, all this data is saved with it.

---

## 📁 File System Overview

```
Unity Project
│
├── Assets/Resources/
│   ├── CardDatabase.asset ← All cards in the game
│   └── Cards/
│       ├── Attack/
│       │   └── [CardData assets]
│       ├── Guard/
│       ├── Skill/
│       └── Power/
│
└── Application.persistentDataPath/
    ├── DeckPresets/
    │   ├── Marauder_Starter.json
    │   ├── My_Deck.json
    │   └── ...
    └── characters.json ← Character save data
```

---

## ⚙️ System Components

| Component | Purpose | Location |
|-----------|---------|----------|
| **CardData** | Individual card definition | `Assets/Resources/Cards/` |
| **CardDatabase** | Central card pool | `Assets/Resources/CardDatabase.asset` |
| **DeckPreset** | Saved deck configuration | Runtime, saved to JSON |
| **DeckManager** | Manages active deck, persists across scenes | Singleton (DontDestroyOnLoad) |
| **CharacterDeckData** | Character's card collection & deck list | Part of Character save data |
| **StarterCardCollection** | Defines starter cards per class | Static helper class |

---

## 🔄 Data Flow Summary

### **Character Creation**
```
New Character Created
→ Unlock starter cards (StarterCardCollection)
→ Create starter deck (DeckPreset)
→ Save character (includes deckData)
```

### **Building a Deck**
```
Open Deck Builder
→ Load character.deckData.unlockedCards
→ Show only owned cards in collection
→ Build deck
→ Save as JSON to DeckPresets folder
→ Add deck name to character.deckData.savedDeckNames
→ Save character
```

### **Entering Combat**
```
Load Combat Scene
→ DeckManager.GetActiveDeck() (based on character.deckData.activeDeckName)
→ Convert DeckPreset → List<Card>
→ Combat system uses the deck
```

---

## 🛠️ Common Tasks

### **Task: Add 10 New Cards**

1. Create 10 CardData assets in `Assets/Resources/Cards/[Category]/`
2. Fill in details for each
3. Open `CardDatabase.asset`
4. Drag all 10 cards into the **All Cards** list
5. Done!

**Time:** ~5 minutes per card = 50 minutes

---

### **Task: Give Character a New Card**

```csharp
Character character = CharacterManager.Instance.GetCurrentCharacter();
character.deckData.UnlockCard("Fireball");
CharacterManager.Instance.SaveCharacter();
```

**Result:** "Fireball" now appears in Deck Builder for this character.

---

### **Task: Create Starter Deck for New Class**

1. Open `StarterCardCollection.cs`
2. Add a new method:
   ```csharp
   private static List<string> GetMyClassStarters()
   {
       return new List<string>
       {
           "Card 1",
           "Card 2",
           ...
       };
   }
   ```
3. Add case to `GetStarterCards()` switch statement
4. Done! New characters of this class get these cards

---

### **Task: Debug - Unlock All Cards for Testing**

```csharp
// In Unity Editor, add this to a script with [ContextMenu]
[ContextMenu("Debug: Unlock All Cards")]
public void DebugUnlockAllCards()
{
    Character character = CharacterManager.Instance.GetCurrentCharacter();
    if (character != null)
    {
        character.deckData.UnlockAllCards();
        CharacterManager.Instance.SaveCharacter();
    }
}
```

**Or manually:**
- Select character in CharacterManager
- Inspector → Deck Data → Has All Cards = ✅ Checked

---

## 🎮 Player Workflow

### **As a Player:**

1. **Create Character** → Get starter cards
2. **Play Game** → Unlock more cards (boss rewards, shops, etc.)
3. **Open Deck Builder** → Build decks from unlocked cards
4. **Save Deck** → Deck saved to character
5. **Select Deck** → Set as active
6. **Enter Combat** → Fight with active deck
7. **Unlock More Cards** → Repeat!

---

## 📊 Character Save Data Structure

```json
{
  "characterName": "MyHero",
  "characterClass": "Marauder",
  "level": 10,
  "deckData": {
    "activeDeckName": "My Best Deck",
    "savedDeckNames": [
      "Marauder Starter",
      "My Best Deck",
      "Boss Killer"
    ],
    "unlockedCards": [
      "Heavy Strike",
      "Cleave",
      "Fireball",
      ...50 more cards...
    ],
    "hasAllCards": false,
    "deckSlotsUnlocked": 5
  }
}
```

**Key Points:**
- `unlockedCards` = What cards the character owns
- `activeDeckName` = Which deck is equipped for combat
- `savedDeckNames` = Which decks this character has built
- Each character has their own separate card collection

---

## 🔍 Finding Specific Info

| Question | Answer Location |
|----------|----------------|
| How to create cards? | `CardCreation_Workflow.md` |
| How to integrate with character saves? | `CharacterDeck_Integration_Guide.md` |
| How does DeckBuilder UI work? | `DeckBuilder_System_Guide.md` |
| What are starter cards per class? | `StarterCardCollection.cs` |
| How to unlock cards for a character? | `CharacterDeckData.cs` → `UnlockCard()` |

---

## ✅ Final Checklist

**For Adding Cards:**
- [ ] CardData asset created
- [ ] Artwork assigned
- [ ] Stats configured
- [ ] Added to CardDatabase
- [ ] (Optional) Added to starter collection

**For Character Integration:**
- [ ] Character.deckData exists
- [ ] Starter cards initialized on character creation
- [ ] Unlocked cards filter in Deck Builder
- [ ] Deck names saved with character
- [ ] Active deck loaded in combat

---

## 🚀 You're All Set!

- **Cards are stored:** `Assets/Resources/Cards/`
- **New cards are added:** Create asset → Add to database
- **Character owns cards:** `character.deckData.unlockedCards`
- **Decks are saved:** With character save data + JSON files

Read the full guides for detailed implementation! 🎮✨








