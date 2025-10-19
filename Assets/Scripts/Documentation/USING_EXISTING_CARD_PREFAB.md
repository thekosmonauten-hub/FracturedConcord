# Using Your Existing Card Prefab in Combat

## ✅ You Already Have a Card Prefab!

Location: `Assets/Art/CardArt/CardPrefab.prefab`

Your prefab has:
- ✅ DeckBuilderCardUI component
- ✅ TextMeshPro text elements
- ✅ Button component
- ✅ Nice visual layout (160x220)

**Let's use it in combat!**

---

## 🚀 Setup (3 Steps - 5 Minutes!)

### Step 1: Add Adapter to Your Prefab (2 minutes)

1. **Open prefab**: Double-click `CardPrefab.prefab` in Project window
2. **Select root** GameObject (CardPrefab)
3. **Add Component**: `CombatCardAdapter`
4. **Add Component**: `CardHoverEffect`
5. **Save prefab** (Ctrl+S or File → Save)

**That's it!** Your prefab is now combat-ready!

---

### Step 2: Setup CardRuntimeManager (2 minutes)

1. **Create GameObject** in Combat Scene: "CardRuntimeManager"
2. **Add Component**: `CardRuntimeManager`
3. **Assign in Inspector**:
   ```
   Card Prefab: [Drag CardPrefab.prefab here]
   Card Hand Parent: [Create empty GameObject, bottom center screen]
   Deck Position: [Create empty GameObject, bottom left]
   Discard Position: [Create empty GameObject, bottom right]
   Pool Size: 15
   Card Spacing: 140
   Card Scale: (0.7, 0.7, 1)  ← Smaller for combat (your prefab is 160x220)
   ```

---

### Step 3: Test It! (1 minute)

1. **Select CardRuntimeManager**
2. **Right-click component**
3. **Click**: "Create Test Hand (5 cards)"
4. **See your cards appear!** 🎉

---

## 💻 Using in Code

### Create Cards from Your Deck

```csharp
// Get your deck (from JSON, ScriptableObjects, etc.)
List<Card> myDeck = LoadDeckSomehow();

// Draw 5 cards
List<Card> hand = myDeck.GetRange(0, 5);

// Get player character
Character player = CharacterManager.Instance.GetCurrentCharacter();

// CREATE CARDS WITH YOUR PREFAB!
CardRuntimeManager cardMgr = CardRuntimeManager.Instance;
List<GameObject> cardObjects = cardMgr.CreateHandFromCards(hand, player);

// Cards spawn using your beautiful prefab! ✨
```

### Single Card

```csharp
Card myCard = new Card {
    cardName = "Heavy Strike",
    cardType = CardType.Attack,
    manaCost = 1,
    baseDamage = 8f
};

Character player = CharacterManager.Instance.GetCurrentCharacter();

GameObject cardObj = CardRuntimeManager.Instance.CreateCardFromData(myCard, player);

// Card created with your prefab!
```

---

## 🎯 How the Adapter Works

**Your Prefab Structure:**
```
CardPrefab
├── DeckBuilderCardUI ← Your existing component
├── CombatCardAdapter ← NEW! Bridges to combat system
└── CardHoverEffect ← NEW! Adds hover animations
```

**What CombatCardAdapter Does:**
1. Takes Card or CardData
2. Converts to format DeckBuilderCardUI expects
3. Calls DeckBuilderCardUI.Initialize()
4. Your card displays correctly!

---

## ⚙️ Configuration

### Card Scale Recommendation

Your prefab is 160x220 which is larger than typical combat cards. Adjust scale:

```
Card Scale in CardRuntimeManager:
- (0.6, 0.6, 1) - Smaller cards
- (0.7, 0.7, 1) - Good balance ← Recommended
- (0.8, 0.8, 1) - Larger cards
- (1.0, 1.0, 1) - Full size (may be too big)
```

### Card Spacing

```
Card Spacing: 
- 100 - Tight (cards close together)
- 140 - Normal ← Good default
- 180 - Loose (more space between cards)
- Adjust based on your scaled card width!
```

---

## 🎨 Visual Updates

### The adapter automatically:
- ✅ Shows card name
- ✅ Shows mana cost
- ✅ Shows description
- ✅ Shows damage/block values
- ✅ Colors background by card type
- ✅ Colors border by element
- ✅ Shows rarity glow for rare cards

### Dynamic Values

If you pass a Character:
```csharp
cardMgr.CreateCardFromData(card, playerCharacter);
```

The card will show:
- ✅ **Calculated damage** (base + scaling + weapon)
- ✅ **Dynamic description** with {damage} replaced
- ✅ **Actual values** the player will deal

---

## 🔧 Customization

### Want Different Colors?

Edit `CombatCardAdapter.cs`:

```csharp
private Color GetCardTypeColor(CardType cardType)
{
    switch (cardType)
    {
        case CardType.Attack: 
            return new Color(1f, 0.2f, 0.2f); // Brighter red
        // ... customize other colors
    }
}
```

### Want to Show More Info?

Add to `CombatCardAdapter.cs`:

```csharp
// Show weapon scaling icon
Image weaponIcon = transform.Find("WeaponIcon")?.GetComponent<Image>();
if (weaponIcon != null && card.scalesWithMeleeWeapon)
{
    weaponIcon.enabled = true;
}
```

---

## 📊 Complete Example

```csharp
public class MyCardSystem : MonoBehaviour
{
    void DrawStartingHand()
    {
        // 1. Load deck from JSON
        TextAsset jsonFile = Resources.Load<TextAsset>("CardJSON/MarauderStarterDeck");
        // ... parse JSON to List<Card>
        
        // 2. Shuffle
        ShuffleDeck(deck);
        
        // 3. Draw 5 cards
        List<Card> hand = deck.GetRange(0, 5);
        deck.RemoveRange(0, 5);
        
        // 4. Get player
        Character player = CharacterManager.Instance.GetCurrentCharacter();
        
        // 5. CREATE CARDS WITH YOUR PREFAB!
        CardRuntimeManager cardMgr = CardRuntimeManager.Instance;
        List<GameObject> cardObjects = cardMgr.CreateHandFromCards(hand, player);
        
        // 6. Setup click handlers
        for (int i = 0; i < cardObjects.Count; i++)
        {
            int index = i;
            Button btn = cardObjects[i].GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnCardClicked(hand[index], cardObjects[index]));
            }
        }
        
        Debug.Log($"Drew {hand.Count} cards with your prefab!");
    }
    
    void OnCardClicked(Card card, GameObject cardObj)
    {
        // Play card with animation
        Vector3 targetPos = GetEnemyPosition();
        
        CardRuntimeManager.Instance.AnimateCardPlay(cardObj, targetPos, () => {
            ApplyCardEffect(card);
        });
    }
}
```

---

## ✅ Checklist

Setup your existing prefab for combat:

- [ ] Open CardPrefab.prefab
- [ ] Add CombatCardAdapter component
- [ ] Add CardHoverEffect component
- [ ] Save prefab
- [ ] Create CardRuntimeManager in scene
- [ ] Assign CardPrefab to Card Prefab field
- [ ] Create CardHandParent, DeckPile, DiscardPile
- [ ] Assign transforms to manager
- [ ] Set Card Scale to 0.7
- [ ] Test with "Create Test Hand"
- [ ] Cards appear using your prefab!

---

## 🎬 What You'll See

**When you test:**

1. Your cards appear with your custom design
2. Cards animate from deck to hand
3. Cards lift and scale on hover
4. Cards fly to target when played
5. All using YOUR existing prefab!

**No need to create a new prefab!** ✨

---

## 📝 Summary

**To use YOUR existing CardPrefab:**

1. **Add to prefab**: `CombatCardAdapter` + `CardHoverEffect`
2. **Assign to manager**: Drag prefab to Card Prefab field
3. **Use in code**: `CreateCardFromData(card, player)`

**That's it!** Your prefab works in combat! 🎮

---

*Setup Time: 5 minutes*
*Works with your existing CardPrefab.prefab*

