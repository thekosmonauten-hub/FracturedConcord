# Card Runtime - Quick Start ⚡

## 🃏 "How do I create cards at runtime?"

**3-step answer:**

---

## Step 1: Create Card Prefab (1 minute)

**Unity Menu**: `Tools > Combat UI > Create Card Prefab`
**Click**: "Create Card Prefab"
**Done!** → Prefab saved to `Assets/Prefab/CardPrefab.prefab`

---

## Step 2: Setup Manager (2 minutes)

**In Scene:**
1. Create GameObject: `CardRuntimeManager`
2. Add Component: `CardRuntimeManager`
3. Create 3 empty GameObjects:
   - `CardHandParent` (bottom center screen)
   - `DeckPile` (bottom left)
   - `DiscardPile` (bottom right)

**In Inspector (CardRuntimeManager):**
- Card Prefab → Drag `CardPrefab.prefab`
- Card Hand Parent → Drag `CardHandParent`
- Deck Position → Drag `DeckPile`
- Discard Position → Drag `DiscardPile`
- Pool Size: 15

---

## Step 3: Create Cards (1 line of code!)

**From code:**

```csharp
// Get manager
CardRuntimeManager cardMgr = CardRuntimeManager.Instance;

// Create a card
Card myCard = new Card {
    cardName = "Fire Strike",
    cardType = CardType.Attack,
    baseDamage = 12f
};

Character player = CharacterManager.Instance.GetCurrentCharacter();

// THIS LINE CREATES THE CARD!
GameObject cardObj = cardMgr.CreateCardFromData(myCard, player);

// Card is now visible in the scene!
```

---

## 🎮 Quick Test

**No code needed - use context menu:**

1. **Select** CardRuntimeManager in Hierarchy
2. **Right-click** component in Inspector
3. **Click**: "Create Test Hand (5 cards)"
4. **See** 5 cards appear in hand!
5. **Try**: "Clear All Cards" to remove them

---

## 💡 Common Usage Patterns

### Pattern 1: Draw from Deck

```csharp
// You have a deck list
List<Card> deck = LoadDeck();

// Draw 5 cards
List<Card> hand = deck.GetRange(0, 5);
deck.RemoveRange(0, 5);

// Create visuals
Character player = CharacterManager.Instance.GetCurrentCharacter();
CardRuntimeManager.Instance.CreateHandFromCards(hand, player);

// Cards animate from deck to hand!
```

### Pattern 2: Single Card

```csharp
// Create one card
Card card = new Card { cardName = "Strike", baseDamage = 6f };

GameObject cardObj = CardRuntimeManager.Instance.CreateCardFromData(
    card, 
    playerCharacter
);

// Position it
cardMgr.PositionCardInHand(cardObj, 0, 1);
```

### Pattern 3: From JSON

```csharp
// Load from your JSON deck
TextAsset json = Resources.Load<TextAsset>("CardJSON/MarauderStarterDeck");
// Parse JSON to List<Card>
// Create hand from list

cardMgr.CreateHandFromCards(parsedCards, player);
```

---

## 🎨 What You Get

**Created card will have:**
- ✅ Visual display (name, cost, damage, description)
- ✅ Colored by card type (red = attack, blue = guard)
- ✅ Hover effect (lift and scale)
- ✅ Click handler (ready to connect)
- ✅ Animations (draw, play, discard)
- ✅ Pooled (performance optimized)

---

## 📁 Files You Got

```
Assets/Scripts/UI/Combat/
├── CardRuntimeManager.cs        (Main card manager)
├── CardVisualizer.cs            (For Card.cs format)
└── CardDataVisualizer.cs        (For CardData format)

Assets/Scripts/Editor/
└── CardPrefabCreator.cs         (Auto-create tool)

Assets/Scripts/Documentation/
└── CardRuntimeWorkflow_Guide.md (Complete guide)
```

---

## ⚡ Super Quick Example

**Copy-paste this to test:**

```csharp
using UnityEngine;

public class QuickCardTest : MonoBehaviour
{
    [ContextMenu("Create 3 Test Cards")]
    void Test()
    {
        var cardMgr = CardRuntimeManager.Instance;
        var player = CharacterManager.Instance.GetCurrentCharacter();
        
        List<Card> cards = new List<Card> {
            new Card { cardName = "Strike", cardType = CardType.Attack, baseDamage = 6f },
            new Card { cardName = "Block", cardType = CardType.Guard, baseGuard = 5f },
            new Card { cardName = "Fireball", cardType = CardType.Attack, baseDamage = 12f }
        };
        
        cardMgr.CreateHandFromCards(cards, player);
    }
}
```

**Usage:**
1. Add this script to any GameObject
2. Right-click component → "Create 3 Test Cards"
3. Cards appear!

---

## 🎯 Next Steps

1. ✅ Create card prefab (Tools menu)
2. ✅ Setup CardRuntimeManager in scene
3. ✅ Test with context menu
4. ✅ Integrate with your combat system
5. ✅ Create cards from your deck data

**That's it!** 🎉

---

*Quick Start Guide*
*Setup Time: 5 minutes*
*Difficulty: Easy*

