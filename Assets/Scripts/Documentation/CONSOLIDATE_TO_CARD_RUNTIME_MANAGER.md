# Consolidating to CardRuntimeManager

## Why Consolidate?

You currently have **3 systems** creating cards:

| System | Location | Issues |
|--------|----------|--------|
| `CardRuntimeManager` | Combat Scene | ✅ Modern, pooled, animation-ready |
| `SimpleCombatUI` | DeckBuilder Scene | ❌ Custom scaling, no pooling, duplicate code |
| `CardFactory` | Various | ❌ Static methods, no pooling, inconsistent |

### Problems This Causes:
- ❌ Different card appearance in different scenes
- ❌ Different scaling systems = prefab conflicts
- ❌ No pooling = performance issues
- ❌ Duplicate code = maintenance nightmare
- ❌ Inconsistent behavior = bugs

### Benefits of Using CardRuntimeManager Everywhere:
- ✅ **One source of truth** - Cards look the same everywhere
- ✅ **Object pooling** - Better performance
- ✅ **Consistent scaling** - No more prefab conflicts!
- ✅ **Animation-ready** - Integrates with CombatAnimationManager
- ✅ **Less code to maintain** - DRY principle

---

## Migration Guide

### Step 1: DeckBuilder Scene

#### Before (Old):
```csharp
public class SimpleCombatUI : MonoBehaviour
{
    public GameObject cardPrefab;
    public float scaleX = 1f;
    public float scaleY = 1f;
    
    private GameObject CreateCardInstance(CardData cardData)
    {
        GameObject cardInstance = Instantiate(cardPrefab, cardHandParent);
        // Custom scaling logic...
        cardInstance.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        return cardInstance;
    }
}
```

#### After (New):
```csharp
public class SimpleCombatUI_Refactored : MonoBehaviour
{
    public CardRuntimeManager cardRuntimeManager; // Use existing manager!
    
    private GameObject CreateCardInstance(CardData cardData)
    {
        // CardRuntimeManager handles creation, pooling, and scaling
        return cardRuntimeManager.CreateCardFromCardData(cardData);
    }
}
```

**Files:**
- ✅ Created: `SimpleCombatUI_Refactored.cs`
- 📝 Keep: `SimpleCombatUI.cs` (backup, can delete later)

**How to migrate:**
1. Add `CardRuntimeManager` to DeckBuilder Scene
2. Replace `SimpleCombatUI` with `SimpleCombatUI_Refactored`
3. Assign `CardRuntimeManager` reference
4. Set desired scale in `CardRuntimeManager.cardScale`
5. Test!

---

### Step 2: Replace CardFactory Calls

#### Before (Old):
```csharp
GameObject card = CardFactory.CreateCard(cardData, parentTransform);
```

#### After (New):
```csharp
GameObject card = CardRuntimeManager.Instance.CreateCardFromCardData(cardData);
```

**Search and Replace:**
```bash
Find:    CardFactory.CreateCard(
Replace: CardRuntimeManager.Instance.CreateCardFromCardData(
```

**Benefits:**
- Now uses object pooling
- Consistent with rest of game
- Can animate cards easily

---

### Step 3: Set Per-Scene Scaling

Now ALL scenes use `CardRuntimeManager`, so set scale per scene:

#### Combat Scene
```csharp
// In scene setup or manager
CardRuntimeManager combatCards = FindObjectOfType<CardRuntimeManager>();
combatCards.cardScale = new Vector3(0.7f, 0.7f, 1f); // Smaller cards
```

#### DeckBuilder Scene
```csharp
// In scene setup
CardRuntimeManager deckCards = FindObjectOfType<CardRuntimeManager>();
deckCards.cardScale = new Vector3(1.2f, 1.2f, 1f); // Larger cards
```

#### Collection/Gallery Scene
```csharp
// In scene setup
CardRuntimeManager galleryCards = FindObjectOfType<CardRuntimeManager>();
galleryCards.cardScale = new Vector3(0.5f, 0.5f, 1f); // Tiny thumbnails
```

**Now the PREFAB stays at base size, each scene scales it!**

---

## Implementation Checklist

### Phase 1: DeckBuilder Scene
- [ ] Add `CardRuntimeManager` GameObject to scene
- [ ] Assign `cardPrefab` to CardRuntimeManager
- [ ] Set `cardHandParent` in CardRuntimeManager
- [ ] Set `cardScale` for DeckBuilder (e.g., `1.2, 1.2, 1`)
- [ ] Replace `SimpleCombatUI` with `SimpleCombatUI_Refactored`
- [ ] Assign `cardRuntimeManager` reference
- [ ] Test drawing cards
- [ ] Test card interactions

### Phase 2: Replace CardFactory
- [ ] Search project for `CardFactory.CreateCard`
- [ ] Replace with `CardRuntimeManager.Instance.CreateCardFromCardData`
- [ ] Search for `CardFactory.CreateRandomCard`
- [ ] Replace with manual random + CardRuntimeManager
- [ ] Test all card creation points

### Phase 3: Cleanup
- [ ] Test all scenes with cards
- [ ] Verify scaling is correct everywhere
- [ ] Remove old `SimpleCombatUI.cs` (keep as backup first)
- [ ] Mark `CardFactory` as `[Obsolete]` or delete
- [ ] Update documentation

---

## Scene Setup Template

**Every scene that displays cards needs:**

```
SceneName
├── CardRuntimeManager
│   ├── Card Prefab: CardPrefab.prefab
│   ├── Card Hand Parent: CardHandParent
│   ├── Card Scale: (Adjust per scene)
│   └── Pool Size: 15
├── CardHandParent (Empty Transform)
│   └── Position: (Center-bottom or desired location)
└── Your UI Manager
    └── Reference to CardRuntimeManager
```

---

## Code Examples

### Example 1: Scene Manager Using CardRuntimeManager

```csharp
public class MySceneManager : MonoBehaviour
{
    private CardRuntimeManager cardManager;
    
    void Start()
    {
        // Find or get CardRuntimeManager
        cardManager = CardRuntimeManager.Instance;
        
        // Configure for this scene
        cardManager.cardScale = new Vector3(0.8f, 0.8f, 1f);
        cardManager.cardSpacing = 120f;
        
        // Now create cards
        CreatePlayerHand();
    }
    
    void CreatePlayerHand()
    {
        List<Card> playerCards = GetPlayerDeck();
        
        foreach (Card card in playerCards)
        {
            GameObject cardObj = cardManager.CreateCardFromData(card, playerCharacter);
            // Card is automatically pooled, positioned, and scaled!
        }
        
        // Reposition all cards
        cardManager.RepositionAllCards();
    }
    
    void ReturnCardWhenPlayed(GameObject cardObj)
    {
        // Return to pool when done
        cardManager.ReturnCardToPool(cardObj);
    }
}
```

### Example 2: Replacing CardFactory

```csharp
// OLD WAY ❌
public class OldCardDisplay
{
    void ShowCard(CardData data)
    {
        GameObject card = CardFactory.CreateCard(data, cardParent);
        // No pooling, no scaling control, no animations
    }
}

// NEW WAY ✅
public class NewCardDisplay
{
    void ShowCard(CardData data)
    {
        GameObject card = CardRuntimeManager.Instance.CreateCardFromCardData(data);
        // Pooled, scaled, animation-ready!
    }
    
    void HideCard(GameObject card)
    {
        CardRuntimeManager.Instance.ReturnCardToPool(card);
        // Returns to pool for reuse!
    }
}
```

### Example 3: Per-Scene Scaling Configuration

```csharp
public class SceneCardScaler : MonoBehaviour
{
    [Header("Card Scale for This Scene")]
    [SerializeField] private Vector3 cardScale = new Vector3(1f, 1f, 1f);
    
    void Awake()
    {
        CardRuntimeManager manager = CardRuntimeManager.Instance;
        if (manager != null)
        {
            manager.cardScale = cardScale;
            Debug.Log($"Set card scale for {gameObject.scene.name}: {cardScale}");
        }
    }
}
```

---

## Testing

### Test Each Scene:
1. ✅ Cards appear at correct size
2. ✅ Cards are properly positioned
3. ✅ Card interactions work (hover, click)
4. ✅ Cards pool correctly (watch Hierarchy - objects get reused)
5. ✅ Prefab is unchanged between scenes

### Performance Test:
```csharp
[ContextMenu("Stress Test Pooling")]
void TestPooling()
{
    // Create 100 cards
    for (int i = 0; i < 100; i++)
    {
        GameObject card = CardRuntimeManager.Instance.CreateCardFromCardData(testCard);
    }
    
    // Return all to pool
    CardRuntimeManager.Instance.ClearAllCards();
    
    // Create 100 more - should reuse pooled objects!
    for (int i = 0; i < 100; i++)
    {
        GameObject card = CardRuntimeManager.Instance.CreateCardFromCardData(testCard);
    }
    
    // Check Hierarchy - should only have ~100 total card objects (pooled)
}
```

---

## Rollback Plan

If something breaks:

1. **Keep old scripts as backup:**
   - `SimpleCombatUI.cs` → `SimpleCombatUI_OLD.cs`
   - Don't delete immediately!

2. **Revert scene:**
   - Use version control (Git)
   - Or manually re-add old components

3. **Test incrementally:**
   - Migrate one scene at a time
   - Test before moving to next

---

## FAQ

### Q: What about scenes that need different card layouts?
**A:** Use prefab variants for layout, CardRuntimeManager for scaling.

### Q: Can I still customize individual cards?
**A:** Yes! CardRuntimeManager creates the base, then you can modify.

### Q: What if I need special card behavior in one scene?
**A:** Add scene-specific components after creation:
```csharp
GameObject card = cardManager.CreateCardFromCardData(data);
card.AddComponent<MyCustomBehavior>();
```

### Q: Performance difference?
**A:** CardRuntimeManager uses object pooling = **much better performance!**

---

## Summary

**Before:**
- 3 different card creation systems ❌
- Inconsistent scaling ❌
- No pooling ❌
- Hard to maintain ❌

**After:**
- 1 unified card system ✅
- Consistent appearance ✅
- Object pooling ✅
- Easy to maintain ✅
- Per-scene scaling ✅

**Result:** No more prefab scaling conflicts! 🎉

