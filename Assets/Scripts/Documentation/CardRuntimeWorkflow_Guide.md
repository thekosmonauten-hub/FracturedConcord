# Card Runtime Creation - Complete Workflow Guide

## 🎯 Overview

Complete guide on creating and displaying cards at runtime in your combat system.

**Workflow:**
```
Card Data → Create GameObject → Display in Hand → Animate → Play/Discard → Pool
```

---

## 🚀 Quick Start (3 Steps!)

### Step 1: Create Card Prefab (1 minute)

**Use the auto-creator:**

1. **Unity Menu**: `Tools > Combat UI > Create Card Prefab`
2. **Configure**:
   - Prefab Name: `CardPrefab`
   - Card Size: (120, 180)
   - Use CardData Format: ✓ (or uncheck for Card.cs)
3. **Click**: "Create Card Prefab"
4. **Done!** Prefab created at `Assets/Prefab/CardPrefab.prefab`

### Step 2: Setup Card Runtime Manager

1. **Create GameObject** in your Combat Scene: "CardRuntimeManager"
2. **Add Component**: `CardRuntimeManager`
3. **Assign in Inspector**:
   - Card Prefab → Drag your CardPrefab
   - Card Hand Parent → Create empty GameObject at bottom of screen
   - Deck Position → Create empty GameObject (left side)
   - Discard Position → Create empty GameObject (right side)
   - Pool Size: 15

### Step 3: Create Cards at Runtime

**From code:**

```csharp
// Get the manager
CardRuntimeManager cardManager = CardRuntimeManager.Instance;

// Create a single card
Card myCard = new Card {
    cardName = "Fire Strike",
    cardType = CardType.Attack,
    manaCost = 2,
    baseDamage = 12f,
    primaryDamageType = DamageType.Fire
};

CharacterManager charMgr = CharacterManager.Instance;
Character player = charMgr.GetCurrentCharacter();

GameObject cardObject = cardManager.CreateCardFromData(myCard, player);

// Create a full hand from a list
List<Card> hand = GetPlayerHand();
List<GameObject> cardObjects = cardManager.CreateHandFromCards(hand, player);
```

---

## 📋 Complete Workflow

### 1. Card Data Sources

**You have 2 options:**

#### Option A: Card Class (Full Featured)
```csharp
Card card = new Card {
    cardName = "Heavy Strike",
    description = "Deal {damage} physical damage",
    cardType = CardType.Attack,
    manaCost = 1,
    baseDamage = 8f,
    primaryDamageType = DamageType.Physical,
    scalesWithMeleeWeapon = true,
    damageScaling = new AttributeScaling {
        strengthScaling = 0.5f
    }
};
```

#### Option B: CardData ScriptableObject (Simpler)
```csharp
CardData cardData = ScriptableObject.CreateInstance<CardData>();
cardData.cardName = "Heavy Strike";
cardData.cardType = "Attack";
cardData.playCost = 1;
cardData.damage = 8;
```

### 2. Create Card GameObject

**Using CardRuntimeManager:**

```csharp
CardRuntimeManager cardMgr = CardRuntimeManager.Instance;

// From Card class:
GameObject cardObj = cardMgr.CreateCardFromData(card, playerCharacter);

// From CardData:
GameObject cardObj = cardMgr.CreateCardFromCardData(cardData);
```

### 3. Display in Hand

**Automatically positioned:**

```csharp
// Create full hand (auto-positioned)
List<Card> hand = new List<Card> { card1, card2, card3 };
List<GameObject> cardObjects = cardMgr.CreateHandFromCards(hand, player);

// Cards are automatically:
// - Positioned in hand with spacing
// - Animated from deck (if deck position set)
// - Ready for interaction
```

### 4. Play Card

**With animations:**

```csharp
// When card is clicked:
void OnCardClicked(GameObject cardObj, Vector3 targetPosition)
{
    cardMgr.AnimateCardPlay(cardObj, targetPosition, onComplete: () => {
        // Apply card effect
        ApplyCardEffect();
    });
}

// Card automatically:
// - Flies to target
// - Scales down
// - Fades out
// - Returns to pool
```

### 5. Discard Card

```csharp
cardMgr.AnimateCardDiscard(cardObj, onComplete: () => {
    Debug.Log("Card discarded");
});
```

---

## 🎨 Card Prefab Structure

**What the auto-creator makes:**

```
CardPrefab (120x180)
├── Background (Image)
│   └── Color: Changes based on card type
├── Border (Image + Outline)
│   └── Color: Changes based on element
├── RarityGlow (Image)
│   └── Glow for rare/unique cards
├── CardName (Text, 16pt, bold)
│   └── Top center
├── Cost (Text, 20pt, cyan)
│   └── Top right corner
├── Type (Text, 12pt, yellow)
│   └── Below name
├── Value (Text, 36pt, bold)
│   └── Center - shows damage/block
└── Description (Text, 10pt)
    └── Bottom - card effect text

Components:
├── RectTransform (120x180)
├── CanvasGroup (for fading)
├── Button (for clicking)
├── CardDataVisualizer (updates visuals)
└── CardHoverEffect (hover animation)
```

---

## 💻 Integration Examples

### Example 1: Draw Cards from Deck

```csharp
public class MyDeckSystem : MonoBehaviour
{
    private List<Card> drawPile = new List<Card>();
    private List<GameObject> handObjects = new List<GameObject>();
    
    public void DrawCards(int count)
    {
        CardRuntimeManager cardMgr = CardRuntimeManager.Instance;
        CharacterManager charMgr = CharacterManager.Instance;
        Character player = charMgr.GetCurrentCharacter();
        
        for (int i = 0; i < count; i++)
        {
            if (drawPile.Count > 0)
            {
                // Get card data from draw pile
                Card card = drawPile[0];
                drawPile.RemoveAt(0);
                
                // Create visual card
                GameObject cardObj = cardMgr.CreateCardFromData(card, player);
                handObjects.Add(cardObj);
                
                // Position in hand
                cardObj.transform.SetParent(cardMgr.transform);
                cardMgr.PositionCardInHand(cardObj, handObjects.Count - 1, handObjects.Count);
            }
        }
    }
}
```

### Example 2: Complete Combat Card System

```csharp
public class CombatCardController : MonoBehaviour
{
    private List<Card> deck = new List<Card>();
    private List<Card> hand = new List<Card>();
    private List<Card> discard = new List<Card>();
    private List<GameObject> handVisuals = new List<GameObject>();
    
    void Start()
    {
        // Load deck
        LoadDeck();
        
        // Draw initial hand
        DrawHand(5);
    }
    
    void LoadDeck()
    {
        // Get starter cards for character
        CharacterManager charMgr = CharacterManager.Instance;
        Character player = charMgr.GetCurrentCharacter();
        
        List<string> starterCards = StarterCardCollection.GetStarterCards(player.characterClass);
        
        // Convert card names to Card objects
        foreach (string cardName in starterCards)
        {
            Card card = CreateCardByName(cardName);
            if (card != null)
            {
                deck.Add(card);
            }
        }
        
        ShuffleDeck();
    }
    
    void DrawHand(int cardCount)
    {
        // Clear existing hand
        ClearHand();
        
        // Draw cards
        for (int i = 0; i < cardCount && deck.Count > 0; i++)
        {
            Card card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);
        }
        
        // Create visuals
        UpdateHandVisuals();
    }
    
    void UpdateHandVisuals()
    {
        CardRuntimeManager cardMgr = CardRuntimeManager.Instance;
        CharacterManager charMgr = CharacterManager.Instance;
        Character player = charMgr.GetCurrentCharacter();
        
        // Create all hand cards
        handVisuals = cardMgr.CreateHandFromCards(hand, player);
        
        // Setup click handlers
        for (int i = 0; i < handVisuals.Count; i++)
        {
            int index = i; // Capture for closure
            Button btn = handVisuals[i].GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnCardClicked(index));
            }
        }
    }
    
    void OnCardClicked(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return;
        
        Card card = hand[handIndex];
        GameObject cardObj = handVisuals[handIndex];
        
        // Animate and play card
        Vector3 targetPos = GetTargetPosition();
        CardRuntimeManager.Instance.AnimateCardPlay(cardObj, targetPos, () => {
            // Apply card effect
            PlayCard(card);
            
            // Remove from hand
            hand.RemoveAt(handIndex);
            handVisuals.RemoveAt(handIndex);
            
            // Add to discard
            discard.Add(card);
            
            // Reposition remaining cards
            CardRuntimeManager.Instance.RepositionAllCards();
        });
    }
    
    void PlayCard(Card card)
    {
        // Apply card effects here
        Debug.Log($"Playing: {card.cardName}");
    }
    
    void ClearHand()
    {
        foreach (GameObject cardObj in handVisuals)
        {
            if (cardObj != null)
            {
                Destroy(cardObj);
            }
        }
        handVisuals.Clear();
    }
    
    void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Card temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }
    
    Card CreateCardByName(string cardName)
    {
        // Load from your card database or create manually
        // This is a placeholder - implement based on your system
        return new Card { cardName = cardName };
    }
    
    Vector3 GetTargetPosition()
    {
        // Get enemy position or center of screen
        return new Vector3(Screen.width * 0.7f, Screen.height * 0.6f, 0);
    }
}
```

### Example 3: Simple Test

```csharp
// Quick test in any script:
void TestCreateCard()
{
    Card testCard = new Card {
        cardName = "Test Strike",
        cardType = CardType.Attack,
        manaCost = 1,
        baseDamage = 10f
    };
    
    Character player = CharacterManager.Instance.GetCurrentCharacter();
    
    GameObject cardObj = CardRuntimeManager.Instance.CreateCardFromData(testCard, player);
    
    // Card is now created and visible!
}
```

---

## 🎮 Testing in Unity

### Method 1: Context Menu Testing

1. **Select CardRuntimeManager** in Hierarchy
2. **Right-click component** in Inspector
3. **Choose**:
   - "Create Test Card" → Creates 1 card
   - "Create Test Hand (5 cards)" → Creates 5 cards
   - "Clear All Cards" → Removes all
   - "Show Pool Stats" → Shows pooling info

### Method 2: Script Testing

Add this to any MonoBehaviour:

```csharp
[ContextMenu("Test Card Creation")]
void TestCards()
{
    // Test Card class format
    Card card1 = new Card {
        cardName = "Fire Strike",
        cardType = CardType.Attack,
        manaCost = 2,
        baseDamage = 12f,
        primaryDamageType = DamageType.Fire
    };
    
    GameObject cardObj1 = CardRuntimeManager.Instance.CreateCardFromData(
        card1, 
        CharacterManager.Instance.GetCurrentCharacter()
    );
    
    // Test CardData format
    CardData cardData = ScriptableObject.CreateInstance<CardData>();
    cardData.cardName = "Ice Shield";
    cardData.cardType = "Guard";
    cardData.playCost = 1;
    cardData.block = 8;
    
    GameObject cardObj2 = CardRuntimeManager.Instance.CreateCardFromCardData(cardData);
    
    Debug.Log("Created 2 test cards!");
}
```

---

## 📊 System Architecture

### Your Current Setup

```
Combat Scene
├── CombatDisplayManager (manages combat flow)
│   ├── PlayerCombatDisplay (health/mana display)
│   └── EnemyCombatDisplay x3 (enemy displays)
│
├── CardRuntimeManager ← NEW! (manages card visuals)
│   ├── Card Pool (reusable card GameObjects)
│   ├── Card Hand Parent (where cards appear)
│   ├── Deck Position (draw from here)
│   └── Discard Position (discard to here)
│
├── CombatAnimationManager (handles animations)
│   └── Damage numbers, effects, tweens
│
└── AnimatedCombatUI (optional - alternative UI)
```

### Data Flow

```
1. Game Start
   └→ Load deck (Card data from JSON/ScriptableObjects)

2. Combat Start
   └→ Shuffle deck
   └→ Draw initial hand (5 cards)
   └→ CardRuntimeManager creates GameObjects
   └→ Animate cards from deck to hand

3. Player Turn
   └→ Cards display in hand
   └→ Player hovers → Card lifts and scales
   └→ Player clicks → Card plays with animation
   └→ Card effect applies
   └→ Card returns to pool

4. Turn End
   └→ Discard hand
   └→ Draw new cards
   └→ Repeat
```

---

## 🔧 Setup Checklist

### In Scene Hierarchy

Create these GameObjects:

```
Canvas
└── CombatUI
    ├── CardHandParent (Transform)
    │   └── Position: (Screen.width/2, 100, 0)
    ├── DeckPile (Transform)
    │   └── Position: (50, 100, 0)
    └── DiscardPile (Transform)
        └── Position: (Screen.width - 50, 100, 0)

GameObjects (Root Level)
├── CardRuntimeManager
│   └── Component: CardRuntimeManager
└── CombatAnimationManager
    └── Component: CombatAnimationManager
```

### In Inspector (CardRuntimeManager)

Assign these references:

```
Card Runtime Manager
├── Card Prefab: [CardPrefab] ← Drag from Project
├── Card Hand Parent: [CardHandParent] ← Drag from Hierarchy
├── Card Spacing: 140
├── Card Y Offset: 0
├── Card Scale: (1, 1, 1)
├── Pool Size: 15
├── Pool Parent: (auto-created)
├── Deck Position: [DeckPile] ← Drag from Hierarchy
└── Discard Position: [DiscardPile] ← Drag from Hierarchy
```

---

## 💻 API Reference

### CardRuntimeManager Methods

```csharp
// Create single card from Card class
GameObject CreateCardFromData(Card cardData, Character ownerCharacter)

// Create single card from CardData ScriptableObject
GameObject CreateCardFromCardData(CardData cardData)

// Create entire hand (multiple cards)
List<GameObject> CreateHandFromCards(List<Card> cards, Character owner)

// Position card in hand
void PositionCardInHand(GameObject cardObj, int index, int totalCards)

// Reposition all cards
void RepositionAllCards()

// Animate card play
void AnimateCardPlay(GameObject cardObj, Vector3 targetPos, Action onComplete)

// Animate card discard
void AnimateCardDiscard(GameObject cardObj, Action onComplete)

// Remove card from display
void RemoveCard(GameObject cardObj)

// Clear all cards
void ClearAllCards()

// Get active card count
int GetActiveCardCount()
```

---

## 🎯 Integration with Existing Systems

### With CombatDisplayManager

```csharp
public class CombatIntegration : MonoBehaviour
{
    private CombatDisplayManager combatMgr;
    private CardRuntimeManager cardMgr;
    
    void Start()
    {
        combatMgr = FindFirstObjectByType<CombatDisplayManager>();
        cardMgr = CardRuntimeManager.Instance;
        
        // Subscribe to combat events
        combatMgr.OnTurnTypeChanged += OnTurnChanged;
    }
    
    void OnTurnChanged(bool isPlayerTurn)
    {
        if (isPlayerTurn)
        {
            // Draw cards at start of player turn
            DrawCards(5);
        }
        else
        {
            // Discard hand at end of turn
            DiscardHand();
        }
    }
    
    void DrawCards(int count)
    {
        // Get cards from deck
        List<Card> cardsToDrawList = GetCardsFromDeck(count);
        
        // Create visuals
        Character player = CharacterManager.Instance.GetCurrentCharacter();
        cardMgr.CreateHandFromCards(cardsToDraw, player);
    }
    
    void DiscardHand()
    {
        cardMgr.ClearAllCards();
    }
}
```

### With CharacterManager

```csharp
// Always get player character for card scaling:
CharacterManager charMgr = CharacterManager.Instance;
Character player = charMgr.GetCurrentCharacter();

// Create card with proper scaling values:
GameObject cardObj = cardMgr.CreateCardFromData(card, player);
```

---

## 🔄 Complete Combat Turn Example

```csharp
public class CompleteCombatExample : MonoBehaviour
{
    private List<Card> deck;
    private List<Card> hand;
    private List<Card> discard;
    
    void StartCombat()
    {
        // 1. Load deck
        LoadDeckFromJSON("MarauderStarterDeck");
        
        // 2. Shuffle
        ShuffleDeck();
        
        // 3. Draw initial hand
        DrawInitialHand();
    }
    
    void LoadDeckFromJSON(string deckName)
    {
        // Load from Resources
        TextAsset jsonFile = Resources.Load<TextAsset>($"CardJSON/{deckName}");
        // Parse JSON to get card list
        // Add to deck list
    }
    
    void DrawInitialHand()
    {
        // Draw 5 cards
        for (int i = 0; i < 5 && deck.Count > 0; i++)
        {
            Card card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);
        }
        
        // Create visuals
        Character player = CharacterManager.Instance.GetCurrentCharacter();
        CardRuntimeManager.Instance.CreateHandFromCards(hand, player);
    }
    
    void PlayCard(int handIndex, Vector3 targetPos)
    {
        Card card = hand[handIndex];
        
        // Get card GameObject (need to track this)
        GameObject cardObj = GetCardObjectByIndex(handIndex);
        
        // Animate play
        CardRuntimeManager.Instance.AnimateCardPlay(cardObj, targetPos, () => {
            // Apply card effect
            ApplyCardEffect(card);
            
            // Move to discard
            discard.Add(card);
            hand.RemoveAt(handIndex);
            
            // Reposition remaining cards
            CardRuntimeManager.Instance.RepositionAllCards();
        });
    }
}
```

---

## 🎨 Customizing Cards

### Change Card Size

```csharp
// In Inspector:
Card Scale: (0.8, 0.8, 1) // Smaller cards
Card Scale: (1.2, 1.2, 1) // Larger cards
```

### Change Card Spacing

```csharp
Card Spacing: 100 // Tighter spacing
Card Spacing: 160 // Wider spacing
```

### Modify Card Prefab

1. Open CardPrefab in Prefab Mode
2. Adjust text sizes, colors, positions
3. Add new visual elements
4. Save prefab
5. Changes apply to all cards automatically!

---

## 🐛 Troubleshooting

### Cards Don't Appear

**Check:**
- ✅ CardRuntimeManager exists in scene
- ✅ Card Prefab is assigned
- ✅ Card Hand Parent is assigned
- ✅ Called CreateCardFromData() or CreateHandFromCards()

**Test:**
```csharp
// Right-click CardRuntimeManager → "Create Test Card"
// Should see a card appear
```

### Cards Appear in Wrong Position

**Check:**
- ✅ Card Hand Parent position (should be bottom center)
- ✅ Card Y Offset (try 0, -300, or adjust)
- ✅ Card Spacing (140 is good default)

### Cards Don't Animate

**Check:**
- ✅ CombatAnimationManager exists in scene
- ✅ Deck/Discard positions assigned
- ✅ Playing in Play Mode (not Edit Mode)

### Pool Runs Out

**Check console for:**
```
"Card pool exhausted! Created new card."
```

**Fix:**
- Increase Pool Size to 20 or 30
- Or implement better card recycling

---

## 📦 Complete Setup Summary

**1. Create Prefab:**
```
Tools > Combat UI > Create Card Prefab
```

**2. Add Manager:**
```
Create GameObject → Add CardRuntimeManager
```

**3. Setup Scene:**
```
Create: CardHandParent, DeckPile, DiscardPile
```

**4. Assign References:**
```
Drag prefab and transforms to CardRuntimeManager
```

**5. Test:**
```
Right-click component → Create Test Hand
```

**6. Integrate:**
```
Call CreateHandFromCards() from your combat code
```

---

## ✅ You're Ready!

Everything you need to create cards at runtime:
- ✅ CardRuntimeManager (manages card lifecycle)
- ✅ Card prefab creator tool
- ✅ Object pooling (performance)
- ✅ Animation integration
- ✅ Both Card and CardData support
- ✅ Complete examples

**Next:** Set up the scene objects and test with "Create Test Hand"! 🃏

---

*Card Runtime System v1.0*
*October 2, 2025*

