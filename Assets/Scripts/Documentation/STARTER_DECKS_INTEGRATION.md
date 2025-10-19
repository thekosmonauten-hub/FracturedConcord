# Starter Decks Integration - Complete Guide

## 🎴 Overview

Connect your 6 JSON starter decks to the combat system and see cards in action!

**Your Decks:**
- ✅ Apostle (Chaos/Discard mechanics)
- ✅ Brawler (Momentum system)
- ✅ Marauder (Physical/Vulnerability)
- ✅ Ranger (Bow/Evasion/Poison)
- ✅ Thief (Preparation/Dual-wield)
- ✅ Witch (Elemental spells/Combos)

---

## 🚀 Super Quick Setup (10 Minutes!)

### Step 1: Add DeckLoader (No setup needed!)

The `DeckLoader.cs` is a static utility - **no GameObject needed!** It just works! ✅

### Step 2: Setup CombatDeckManager (5 minutes)

**In Combat Scene:**

1. **Create GameObject**: "CombatDeckManager"
2. **Add Component**: `CombatDeckManager`
3. **Configure in Inspector**:
   ```
   Deck Settings:
   ├── Load Deck On Start: ✓
   ├── Initial Hand Size: 5
   └── Auto Shuffle On Start: ✓
   
   References:
   ├── Card Runtime Manager: (auto-finds)
   └── Character Manager: (auto-finds)
   ```
4. **Done!** Deck manager ready!

### Step 3: Test It! (1 minute)

**Make sure you have:**
- ✅ CardRuntimeManager in scene
- ✅ CardPrefab assigned to CardRuntimeManager
- ✅ Character created (any class)

**Test:**
1. **Press Play**
2. **Deck auto-loads** based on character's class!
3. **5 cards draw** automatically!
4. **See your cards!** 🎉

**Or test manually:**
1. **Select CombatDeckManager**
2. **Right-click component** → "Load Marauder Deck"
3. **Right-click** → "Draw Initial Hand"
4. **See 5 cards appear!**

---

## 💻 Usage in Code

### Load Deck for Current Character

```csharp
// Automatic - loads based on player's class
CombatDeckManager.Instance.LoadDeckForCurrentCharacter();

// Or specific class
CombatDeckManager.Instance.LoadDeckForClass("Marauder");
```

### Draw Cards

```csharp
// Draw cards (auto-creates visuals)
CombatDeckManager.Instance.DrawCards(5);
```

### Get Deck Info

```csharp
int cardsInHand = CombatDeckManager.Instance.GetHandCount();
int cardsInDeck = CombatDeckManager.Instance.GetDrawPileCount();
int cardsInDiscard = CombatDeckManager.Instance.GetDiscardPileCount();

Debug.Log($"Hand: {cardsInHand}, Deck: {cardsInDeck}, Discard: {cardsInDiscard}");
```

---

## 🎮 Complete Combat Integration

### Full Combat Flow Example

```csharp
public class MyCombatController : MonoBehaviour
{
    void StartCombat()
    {
        // 1. Load deck for character
        CombatDeckManager deckMgr = CombatDeckManager.Instance;
        deckMgr.LoadDeckForCurrentCharacter();
        
        // 2. Shuffle
        deckMgr.ShuffleDeck();
        
        // 3. Draw initial hand
        deckMgr.DrawInitialHand();
        
        // Cards are now visible and ready!
        Debug.Log($"Combat started with {deckMgr.GetHandCount()} cards in hand");
    }
    
    void StartPlayerTurn()
    {
        // Draw cards at turn start
        Character player = CharacterManager.Instance.GetCurrentCharacter();
        int cardsToDraw = player.cardsDrawnPerTurn; // Usually 1-2
        
        CombatDeckManager.Instance.DrawCards(cardsToDraw);
    }
    
    void EndPlayerTurn()
    {
        // Discard hand at turn end
        CombatDeckManager.Instance.DiscardHand();
    }
}
```

---

## 📋 Deck File Locations

**Make sure JSON files are in Resources:**

```
Assets/Resources/Cards/
├── starter_deck_apostle.json    ✅
├── starter_deck_brawler.json    ✅
├── starter_deck_marauder.json   ✅
├── starter_deck_ranger.json     ✅
├── starter_deck_thief.json      ✅
└── starter_deck_witch.json      ✅
```

**Path must be**: `Resources/Cards/starter_deck_{classname}.json`

The system looks for: `Resources.Load<TextAsset>("Cards/starter_deck_marauder")`

---

## 🎯 Testing Each Deck

### Method 1: Context Menu (Edit Mode)

**Select CombatDeckManager**, then right-click:

```
Context Menu Options:
├── Load Marauder Deck   ← Test Marauder
├── Load Witch Deck      ← Test Witch
├── Load Ranger Deck     ← Test Ranger
├── Load Brawler Deck    ← Test Brawler
├── Load Thief Deck      ← Test Thief
├── Load Apostle Deck    ← Test Apostle
├── Shuffle Deck
├── Draw Initial Hand    ← Draw 5 cards
├── Draw 1 Card
├── Clear Hand
└── Show Deck Stats      ← See deck composition
```

**Workflow:**
1. Load a deck (e.g., "Load Marauder Deck")
2. Check console → See "Loaded Marauder deck: 18 cards"
3. "Draw Initial Hand"
4. See 5 cards appear!
5. "Clear Hand"
6. Try another class!

### Method 2: Auto-Load (Play Mode)

**Just press Play!**

- CombatDeckManager auto-loads deck for current character
- Auto-shuffles
- Auto-draws 5 cards
- **Done!**

---

## 🎨 What Each Deck Looks Like

### Marauder (18 cards)
```
Loaded Marauder deck: 18 cards
Deck Composition:
  - Heavy Strike x6      (Physical attack, STR scaling)
  - Brace x4            (Guard, STR scaling)
  - Ground Slam x2      (AoE, Vulnerability)
  - Intimidating Shout x2 (Vulnerability to all)
  - Cleave x2           (AoE physical)
  - Endure x2           (Guard + Draw)
```

### Witch (16 cards)
```
Loaded Witch deck: 16 cards
Deck Composition:
  - Arcane Bolt x6      (Lightning, combo)
  - Arcane Barrier x4   (Guard, INT scaling)
  - Fireball x2         (Fire AoE, Ignite)
  - Frost Nova x2       (Cold AoE, Chill)
  - Chaos Bolt x2       (Chaos, Poison)
  - Arcane Focus x2     (Draw, INT buff)
```

### Brawler (15 cards)
```
Loaded Brawler deck: 15 cards
Deck Composition:
  - One-Two Punch x6    (Momentum builder)
  - Momentum Spike x1   (Spender, AoE)
  - Guardbreaker x2     (Guard, Momentum)
  - Rapid Strike x2     (Fast attack)
  - Devastating Blow x2 (Heavy attack)
  - Berserker's Fury x1 (Power buff)
  - Steadfast Guard x2  (Guard)
  - Adrenaline Rush x2  (Momentum + Draw)
```

### Ranger (13 cards)
```
Loaded Ranger deck: 13 cards
Deck Composition:
  - Pack Hunter x6      (Bow attack, combo)
  - Dodge x4            (Evasion)
  - Multi-Shot x3       (AoE, combo)
  - Poison Arrow x2     (Poison, combo)
  - Quickstep x2        (Evasion buff)
  - Focus x1            (Mana recovery)
```

### Thief (12 cards)
```
Loaded Thief deck: 12 cards
Deck Composition:
  - Twin Strike x6      (Dual-wield, prepare)
  - Shadow Step x4      (Guard, dual-wield)
  - Ambush x3           (Preparation synergy)
  - Perfect Strike x2   (Preparation consumer)
  - Feint x2            (Prepare cards)
  - Poisoned Blade x1   (Poison, dual-wield)
```

### Apostle (12 cards)
```
Loaded Apostle deck: 12 cards
Deck Composition:
  - Sacred Strike x6    (Physical/Chaos, discard)
  - Divine Ward x4      (Guard, discard)
  - Divine Wrath x2     (Chaos AoE, discard)
  - Scripture Burn x2   (Discard synergy)
  - Divine Favor x2     (Discard power)
  - Forbidden Prayer x2 (Draw, discard)
```

---

## 🔧 Integration with CombatDisplayManager

### Connect Combat Flow

```csharp
public class CombatFlowIntegrator : MonoBehaviour
{
    private CombatDisplayManager combatDisplay;
    private CombatDeckManager deckManager;
    
    void Start()
    {
        combatDisplay = FindFirstObjectByType<CombatDisplayManager>();
        deckManager = CombatDeckManager.Instance;
        
        // Subscribe to combat events
        if (combatDisplay != null)
        {
            combatDisplay.OnTurnTypeChanged += OnTurnChanged;
        }
    }
    
    void OnTurnChanged(bool isPlayerTurn)
    {
        if (isPlayerTurn)
        {
            // Player turn starts - draw cards
            Character player = CharacterManager.Instance.GetCurrentCharacter();
            int cardsToDraw = player != null ? player.cardsDrawnPerTurn : 1;
            
            deckManager.DrawCards(cardsToDraw);
        }
        else
        {
            // Enemy turn starts - optionally discard hand
            // (depends on your game rules)
        }
    }
}
```

---

## 🎯 Scene Setup Checklist

Complete combat scene setup:

### GameObjects Needed

```
Combat Scene
├── CombatDisplayManager ✓ (already exists)
├── CombatAnimationManager ← Add this
├── CardRuntimeManager ← Add this
├── CombatDeckManager ← Add this
└── UI
    ├── CardHandParent ← Create empty GameObject
    ├── DeckPile ← Create empty GameObject
    └── DiscardPile ← Create empty GameObject
```

### Prefab Setup

```
CardPrefab (Assets/Art/CardArt/CardPrefab.prefab)
├── DeckBuilderCardUI ✓ (already has)
├── CombatCardAdapter ← Add component
├── CardHoverEffect ← Add component
└── Button ✓ (already has)
```

### Assignments

**CardRuntimeManager:**
- Card Prefab → CardPrefab.prefab
- Card Hand Parent → CardHandParent GameObject
- Deck Position → DeckPile GameObject
- Discard Position → DiscardPile GameObject

**CombatDeckManager:**
- (Auto-finds CardRuntimeManager and CharacterManager)

---

## 🧪 Testing Workflow

### Test 1: Load Specific Deck

```
1. Select CombatDeckManager
2. Right-click → "Load Marauder Deck"
3. Console shows: "Loaded Marauder deck: 18 cards"
4. Console shows deck composition
5. Right-click → "Draw Initial Hand"
6. See 5 Marauder cards appear!
```

### Test 2: Try Different Classes

```
1. Right-click → "Clear Hand"
2. Right-click → "Load Witch Deck"
3. Right-click → "Draw Initial Hand"
4. See 5 Witch spell cards!
```

### Test 3: Auto-Load Based on Character

```
1. Make sure you have a character created
2. Character's class is "Ranger"
3. Press Play
4. Ranger deck auto-loads!
5. 5 cards auto-draw!
```

---

## 🐛 Troubleshooting

### "Could not find starter deck JSON"

**Check:**
- ✅ JSON files in `Assets/Resources/Cards/` folder
- ✅ Files named exactly: `starter_deck_marauder.json` (lowercase)
- ✅ Using Resources folder (not anywhere else)

**Fix:**
```
Move files to: Assets/Resources/Cards/
Ensure lowercase: starter_deck_{class}.json
```

### "Loaded deck but cards don't appear"

**Check:**
- ✅ CardRuntimeManager exists in scene
- ✅ Card Prefab assigned to CardRuntimeManager
- ✅ CardHandParent assigned
- ✅ Called DrawInitialHand() or DrawCards()

**Fix:**
```
Right-click CombatDeckManager → "Draw Initial Hand"
```

### "Cards appear but are huge/tiny"

**Check:**
- ✅ Card Scale in CardRuntimeManager

**Fix:**
```
CardRuntimeManager → Card Scale: (0.7, 0.7, 1)
Your prefab is 160x220, so scale down for combat
```

### "JSON parse error"

**Check:**
- ✅ JSON syntax valid (use jsonlint.com)
- ✅ All quotes are straight quotes (not curly)
- ✅ No trailing commas

**Fix:**
```
Validate JSON at jsonlint.com
Check console for specific error message
```

---

## 📊 System Architecture

**How everything connects:**

```
Character (Marauder) 
    ↓
CombatDeckManager.LoadDeckForCurrentCharacter()
    ↓
DeckLoader.LoadStarterDeck("Marauder")
    ↓
Loads: Resources/Cards/starter_deck_marauder.json
    ↓
Parses JSON → List<Card>
    ↓
CombatDeckManager stores in drawPile
    ↓
CombatDeckManager.DrawCards(5)
    ↓
CardRuntimeManager.CreateCardFromData() x5
    ↓
Cards appear using CardPrefab.prefab
    ↓
DeckBuilderCardUI displays card info
    ↓
Player sees 5 Marauder cards in hand! ✨
```

---

## 🎮 Complete Combat Example

```csharp
public class CompleteCombatSetup : MonoBehaviour
{
    private CombatDeckManager deckManager;
    private CombatDisplayManager combatDisplay;
    private CharacterManager characterManager;
    
    void Start()
    {
        InitializeCombat();
    }
    
    void InitializeCombat()
    {
        // Get managers
        deckManager = CombatDeckManager.Instance;
        combatDisplay = FindFirstObjectByType<CombatDisplayManager>();
        characterManager = CharacterManager.Instance;
        
        // Subscribe to events
        if (deckManager != null)
        {
            deckManager.OnCardPlayed += OnCardPlayed;
            deckManager.OnCardDrawn += OnCardDrawn;
        }
        
        // Load deck
        deckManager.LoadDeckForCurrentCharacter();
        deckManager.ShuffleDeck();
        deckManager.DrawInitialHand();
        
        // Start combat
        if (combatDisplay != null)
        {
            combatDisplay.StartCombat();
        }
        
        Debug.Log("Combat initialized with starter deck!");
    }
    
    void OnCardPlayed(Card card)
    {
        Debug.Log($"Card played: {card.cardName}");
        
        // Apply card effects here
        ApplyCardEffects(card);
    }
    
    void OnCardDrawn(Card card)
    {
        Debug.Log($"Card drawn: {card.cardName}");
    }
    
    void ApplyCardEffects(Card card)
    {
        Character player = characterManager.GetCurrentCharacter();
        
        // Apply damage
        if (card.cardType == CardType.Attack)
        {
            float damage = DamageCalculator.CalculateCardDamage(
                card, 
                player, 
                player.weapons.meleeWeapon
            );
            
            // Deal damage to enemy
            Debug.Log($"Dealing {damage} damage!");
            
            // Show damage number
            CombatAnimationManager.Instance?.ShowDamageNumber(
                damage,
                GetEnemyPosition(),
                DamageNumberType.Normal
            );
            
            // Screen shake
            CombatAnimationManager.Instance?.ShakeCamera(0.5f);
        }
        
        // Apply guard
        if (card.cardType == CardType.Guard)
        {
            float guard = DamageCalculator.CalculateGuardAmount(card, player);
            player.AddGuard(guard);
            Debug.Log($"Gained {guard} guard!");
        }
        
        // Apply effects
        foreach (CardEffect effect in card.effects)
        {
            effect.ApplyEffect(player);
        }
    }
    
    Vector3 GetEnemyPosition()
    {
        // Get from CombatDisplayManager or AnimatedCombatUI
        return new Vector3(Screen.width * 0.7f, Screen.height * 0.6f, 0);
    }
}
```

---

## 🔍 Verifying Deck Loads

### Check Console for These Messages:

```
✅ Good Messages:
"Loaded marauder starter deck: Marauder Starter Deck (18 cards)"
"Deck Composition:"
"  - Heavy Strike x6"
"  - Brace x4"
"  etc..."
"Drew card: Heavy Strike (1 cards in hand)"

❌ Error Messages:
"Could not find starter deck JSON for Marauder..."
→ Check file location

"Failed to parse deck JSON for Marauder..."
→ Check JSON syntax

"No character loaded! Cannot load deck."
→ Create a character first
```

---

## 📖 API Reference

### DeckLoader (Static Utility)

```csharp
// Load deck for a class
List<Card> deck = DeckLoader.LoadStarterDeck("Marauder");

// Load all decks
Dictionary<string, List<Card>> allDecks = DeckLoader.LoadAllStarterDecks();
```

### CombatDeckManager (Instance)

```csharp
CombatDeckManager deckMgr = CombatDeckManager.Instance;

// Load deck
deckMgr.LoadDeckForCurrentCharacter();
deckMgr.LoadDeckForClass("Witch");

// Deck operations
deckMgr.ShuffleDeck();
deckMgr.DrawCards(5);
deckMgr.DrawInitialHand();
deckMgr.DiscardHand();

// Play/discard
deckMgr.PlayCard(handIndex, targetPosition);
deckMgr.DiscardCard(handIndex);

// Info
int hand = deckMgr.GetHandCount();
int draw = deckMgr.GetDrawPileCount();
int discard = deckMgr.GetDiscardPileCount();
```

---

## ✅ Complete Setup Checklist

- [ ] JSON files in `Assets/Resources/Cards/` folder
- [ ] Files named `starter_deck_{class}.json` (lowercase)
- [ ] CardPrefab has CombatCardAdapter component
- [ ] CardPrefab has CardHoverEffect component  
- [ ] CardRuntimeManager in scene
- [ ] CardPrefab assigned to CardRuntimeManager
- [ ] CardHandParent created and assigned
- [ ] CombatDeckManager in scene
- [ ] Character created (any class)
- [ ] Tested with "Load X Deck" context menu
- [ ] Tested with "Draw Initial Hand"
- [ ] Cards appear correctly
- [ ] No console errors

---

## 🎉 You're Ready!

Once setup complete:
1. **Press Play**
2. **Deck auto-loads** for character's class
3. **5 cards auto-draw**
4. **Click cards** to play them
5. **See animations** (draw, play, hover)
6. **Full combat** ready to go!

---

*Starter Decks Integration v1.0*
*October 2, 2025*
*Supports: All 6 classes with JSON deck files*

