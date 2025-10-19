# CardDataExtended Migration Guide

## 🎯 **Goal**
Eliminate circular Card → CardData → Card conversion by using CardDataExtended as the single source of truth.

---

## 📋 **Migration Checklist**

- [ ] Phase 1: Complete CardDataExtended (5 min)
- [ ] Phase 2: Update CombatDeckManager (15 min)
- [ ] Phase 3: Update CardRuntimeManager (10 min)
- [ ] Phase 4: Update DeckBuilderCardUI (10 min)
- [ ] Phase 5: Create Migration Utility (15 min)
- [ ] Phase 6: Migrate Existing Cards (20 min)
- [ ] Phase 7: Test & Validate (15 min)

**Total Time**: ~90 minutes

---

## **PHASE 1: Complete CardDataExtended** ✅

CardDataExtended is already created, but needs helper methods.

### **File**: `Assets/Scripts/Data/Cards/CardDataExtended.cs`

Add these methods at the end of the class (before closing brace):

```csharp
/// <summary>
/// Get CardType enum from string cardType
/// </summary>
public CardType GetCardTypeEnum()
{
    switch (cardType.ToLower())
    {
        case "attack": return CardType.Attack;
        case "guard": return CardType.Guard;
        case "skill": return CardType.Skill;
        case "power": return CardType.Power;
        case "aura": return CardType.Aura;
        default: return CardType.Attack;
    }
}

/// <summary>
/// Get mana cost (alias for compatibility)
/// </summary>
public int GetManaCost()
{
    return playCost;
}

/// <summary>
/// Get base damage as float (for combat calculations)
/// </summary>
public float GetBaseDamage()
{
    return (float)damage;
}

/// <summary>
/// Get base guard as float (for combat calculations)
/// </summary>
public float GetBaseGuard()
{
    return (float)block;
}

/// <summary>
/// Convert this CardDataExtended to a Card object (for backward compatibility during migration)
/// TEMPORARY - will be removed after full migration
/// </summary>
[System.Obsolete("Use CardDataExtended directly instead of converting to Card")]
public Card ToCard()
{
    return new Card
    {
        cardName = this.cardName,
        description = this.description,
        cardType = GetCardTypeEnum(),
        manaCost = this.playCost,
        baseDamage = (float)this.damage,
        baseGuard = (float)this.block,
        primaryDamageType = this.primaryDamageType,
        cardArt = this.cardImage,
        cardArtName = this.cardName,
        scalesWithMeleeWeapon = this.scalesWithMeleeWeapon,
        scalesWithProjectileWeapon = this.scalesWithProjectileWeapon,
        scalesWithSpellWeapon = this.scalesWithSpellWeapon,
        isAoE = this.isAoE,
        aoeTargets = this.aoeTargets,
        requirements = this.requirements,
        tags = this.tags != null ? new List<string>(this.tags) : new List<string>(),
        additionalDamageTypes = this.additionalDamageTypes != null ? new List<DamageType>(this.additionalDamageTypes) : new List<DamageType>(),
        damageScaling = this.damageScaling,
        guardScaling = this.guardScaling,
        effects = this.effects != null ? new List<CardEffect>(this.effects) : new List<CardEffect>(),
        comboWith = this.comboWith,
        comboDescription = this.comboDescription,
        comboEffect = this.comboEffect,
        comboHighlightType = this.comboHighlightType
    };
}
```

**Testing**:
- Open Unity
- Check Console for compilation errors
- Should compile successfully

---

## **PHASE 2: Update CombatDeckManager**

### **File**: `Assets/Scripts/CombatSystem/CombatDeckManager.cs`

#### **Step 2.1: Update DrawPile to use CardDataExtended**

Find line 49:
```csharp
private List<Card> drawPile = new List<Card>();
```

Change to:
```csharp
// NEW: Use CardDataExtended directly (no conversion!)
private List<CardDataExtended> drawPile = new List<CardDataExtended>();
```

Also update lines 50-51:
```csharp
private List<CardDataExtended> hand = new List<CardDataExtended>();
private List<CardDataExtended> discardPile = new List<CardDataExtended>();
```

#### **Step 2.2: Update Events**

Find lines 55-57:
```csharp
public System.Action<Card> OnCardDrawn;
public System.Action<Card> OnCardPlayed;
public System.Action<Card> OnCardDiscarded;
```

Change to:
```csharp
public System.Action<CardDataExtended> OnCardDrawn;
public System.Action<CardDataExtended> OnCardPlayed;
public System.Action<CardDataExtended> OnCardDiscarded;
```

#### **Step 2.3: Update LoadDeckForClass Method**

Find the `LoadDeckForClass` method (around line 191). Replace the entire method:

```csharp
/// <summary>
/// Load deck for a specific class
/// </summary>
public void LoadDeckForClass(string characterClass)
{
    Debug.Log($"<color=yellow>=== Loading deck for: {characterClass} (CardDataExtended) ===</color>");
    
    // Clear existing deck
    drawPile.Clear();
    hand.Clear();
    discardPile.Clear();
    
    // Clear visual cards
    if (cardRuntimeManager != null)
    {
        cardRuntimeManager.ClearAllCards();
    }
    handVisuals.Clear();
    
    // Load deck from CardDatabase as CardDataExtended
    List<CardDataExtended> cardDataDeck = LoadDeckFromCardDatabase(characterClass);
    
    if (cardDataDeck != null && cardDataDeck.Count > 0)
    {
        // NO CONVERSION NEEDED! Use CardDataExtended directly!
        foreach (CardDataExtended card in cardDataDeck)
        {
            int copies = GetCardCopies(card.cardName);
            for (int i = 0; i < copies; i++)
            {
                drawPile.Add(card);
            }
            Debug.Log($"[CardDataExtended] Added {copies} copies of {card.cardName} to deck");
        }
        
        Debug.Log($"<color=green>✓ Loaded {characterClass} deck from CardDatabase:</color> {drawPile.Count} cards");
        LogDeckComposition();
    }
    else
    {
        Debug.LogError($"<color=red>✗ Failed to load deck for {characterClass}!</color>");
    }
}
```

#### **Step 2.4: Update LoadDeckFromCardDatabase Method**

Find the method (around line 269). Replace it:

```csharp
/// <summary>
/// Load deck from CardDatabase as CardDataExtended (no conversion!)
/// </summary>
private List<CardDataExtended> LoadDeckFromCardDatabase(string characterClass)
{
    Debug.Log($"<color=cyan>[CardDataExtended] Loading {characterClass} deck from CardDatabase...</color>");
    
    CardDatabase database = CardDatabase.Instance;
    if (database == null)
    {
        Debug.LogError($"[CardDataExtended] CardDatabase not found! Cannot load {characterClass} deck.");
        return null;
    }
    
    Debug.Log($"[CardDataExtended] CardDatabase loaded with {database.allCards.Count} total cards");
    
    // Get cards for this class
    List<CardDataExtended> classCards = GetClassCardsFromDatabase(database, characterClass);
    
    if (classCards.Count == 0)
    {
        Debug.LogWarning($"[CardDataExtended] No {characterClass} cards found in CardDatabase.");
        return null;
    }
    
    Debug.Log($"[CardDataExtended] Found {classCards.Count} {characterClass} cards in CardDatabase");
    return classCards;
}
```

#### **Step 2.5: Add GetClassCardsFromDatabase Method**

Add this new method:

```csharp
/// <summary>
/// Get class cards from database as CardDataExtended
/// </summary>
private List<CardDataExtended> GetClassCardsFromDatabase(CardDatabase database, string characterClass)
{
    List<CardDataExtended> classCards = new List<CardDataExtended>();
    
    // Define class card names
    string[] cardNames = GetCardNamesForClass(characterClass);
    
    foreach (string cardName in cardNames)
    {
        // Try to find CardDataExtended first
        CardDataExtended cardExtended = database.allCards.Find(c => c.cardName == cardName && c is CardDataExtended) as CardDataExtended;
        
        if (cardExtended != null)
        {
            classCards.Add(cardExtended);
            Debug.Log($"[CardDataExtended] Found extended card: {cardExtended.cardName}");
        }
        else
        {
            // Fallback: Try regular CardData and log warning
            CardData cardData = database.allCards.Find(c => c.cardName == cardName);
            if (cardData != null)
            {
                Debug.LogWarning($"[CardDataExtended] Card '{cardName}' is CardData, not CardDataExtended! Migration needed.");
                // For now, you could create a temporary CardDataExtended wrapper
                // Or just skip it and force migration
            }
            else
            {
                Debug.LogError($"[CardDataExtended] Card '{cardName}' not found in database!");
            }
        }
    }
    
    return classCards;
}

/// <summary>
/// Get card names for a specific class
/// </summary>
private string[] GetCardNamesForClass(string characterClass)
{
    switch (characterClass)
    {
        case "Marauder":
            return new string[] {
                "Heavy Strike",
                "Brace",
                "Ground Slam",
                "Cleave",
                "Endure",
                "Intimidating Shout"
            };
        case "Witch":
            return new string[] { /* Add Witch cards */ };
        case "Ranger":
            return new string[] { /* Add Ranger cards */ };
        default:
            return new string[] { };
    }
}
```

#### **Step 2.6: Remove ConvertCardDataToCards Method**

Find and DELETE this entire method (around line 371):
```csharp
/// <summary>
/// Convert CardData ScriptableObjects to Card runtime objects
/// </summary>
private List<Card> ConvertCardDataToCards(List<CardData> cardDataList)
{
    // DELETE THIS ENTIRE METHOD!
}
```

#### **Step 2.7: Update DrawCards Method**

Find the `DrawCards` method (around line 514). Update the key parts:

```csharp
public void DrawCards(int count)
{
    Debug.Log($"<color=yellow>=== DrawCards called: Drawing {count} cards ===</color>");
    Debug.Log($"Current state - Draw pile: {drawPile.Count}, Hand: {hand.Count}, Discard: {discardPile.Count}");
    
    // ... existing manager checks ...
    
    for (int i = 0; i < count; i++)
    {
        if (drawPile.Count == 0)
        {
            // ... reshuffle logic ...
        }
        
        if (drawPile.Count > 0)
        {
            // Draw card from deck
            CardDataExtended drawnCard = drawPile[0]; // Changed from Card to CardDataExtended
            drawPile.RemoveAt(0);
            hand.Add(drawnCard);
            
            Debug.Log($"<color=cyan>Drawing card #{i+1}: {drawnCard.cardName}</color>");
            
            // Create visual card - NO CONVERSION!
            Debug.Log($"Creating visual for: {drawnCard.cardName}...");
            GameObject cardObj = CreateAnimatedCard(drawnCard, player, i);
            
            // ... rest of the method
        }
    }
    
    // ... rest of the method
}
```

#### **Step 2.8: Update CreateAnimatedCard Method**

Find `CreateAnimatedCard` method (around line 1043):

```csharp
/// <summary>
/// Create a card with draw animation from deck pile.
/// </summary>
private GameObject CreateAnimatedCard(CardDataExtended cardData, Character player, int cardIndex)
{
    // Create the card visual - NO CONVERSION!
    GameObject cardObj = cardRuntimeManager.CreateCardFromCardDataExtended(cardData, player);
    if (cardObj == null) return null;
    
    // ... rest of the method stays the same
}
```

#### **Step 2.9: Update PlayCard Method**

Update the PlayCard signature (around line 646):

```csharp
public void PlayCard(int handIndex, Vector3 targetPosition)
{
    if (handIndex < 0 || handIndex >= hand.Count)
    {
        Debug.LogWarning($"Invalid hand index: {handIndex}");
        return;
    }
    
    CardDataExtended card = hand[handIndex]; // Changed from Card to CardDataExtended
    GameObject cardObj = handVisuals[handIndex];
    
    // ... rest of the method, update card references from Card to CardDataExtended
}
```

**Testing**:
- Save file
- Open Unity
- Check for compilation errors
- Fix any remaining Card → CardDataExtended references

---

## **PHASE 3: Update CardRuntimeManager**

### **File**: `Assets/Scripts/UI/Combat/CardRuntimeManager.cs`

#### **Step 3.1: Add New Method CreateCardFromCardDataExtended**

Add this method after the existing `CreateCardFromCardData` method (around line 220):

```csharp
/// <summary>
/// Create a card GameObject from CardDataExtended ScriptableObject (PREFERRED METHOD)
/// </summary>
public GameObject CreateCardFromCardDataExtended(CardDataExtended cardData, Character character)
{
    GameObject cardObj = GetCardFromPool();
    if (cardObj == null) return null;
    
    // IMPORTANT: Reparent to hand (not pool!)
    if (cardHandParent != null)
    {
        cardObj.transform.SetParent(cardHandParent, false);
    }
    
    Debug.Log($"<color=cyan>[CardDataExtended] Creating card: {cardData.cardName}</color>");
    Debug.Log($"<color=cyan>[CardDataExtended]   - Card Image: {(cardData.cardImage != null ? "✅ LOADED" : "❌ NULL")}</color>");
    
    // Try to use CombatCardAdapter
    CombatCardAdapter adapter = cardObj.GetComponent<CombatCardAdapter>();
    if (adapter != null)
    {
        adapter.SetCardDataExtended(cardData, character);
    }
    else
    {
        // Fallback to DeckBuilderCardUI directly
        DeckBuilderCardUI deckBuilderCard = cardObj.GetComponent<DeckBuilderCardUI>();
        if (deckBuilderCard != null)
        {
            deckBuilderCard.Initialize(cardData, null);
        }
        else
        {
            Debug.LogWarning($"[CardDataExtended] No adapter or DeckBuilderCardUI found on card prefab!");
        }
    }
    
    // Setup hover effect
    CardHoverEffect hover = cardObj.GetComponent<CardHoverEffect>();
    if (hover == null)
    {
        hover = cardObj.AddComponent<CardHoverEffect>();
        hover.animationManager = animManager;
    }
    
    // Scale
    cardObj.transform.localScale = cardScale;
    
    // Store base scale for hover effect
    hover.StoreBaseScale();
    
    // Add to active cards list
    if (!activeCards.Contains(cardObj))
    {
        activeCards.Add(cardObj);
    }
    
    return cardObj;
}
```

**Testing**:
- Save file
- Check for compilation errors

---

## **PHASE 4: Update CombatCardAdapter**

### **File**: `Assets/Scripts/UI/Combat/CombatCardAdapter.cs`

#### **Step 4.1: Add New Method SetCardDataExtended**

Add this method after the existing `SetCardData` method (around line 54):

```csharp
/// <summary>
/// Set card from CardDataExtended (PREFERRED METHOD - no conversion!)
/// </summary>
public void SetCardDataExtended(CardDataExtended cardData, Character character)
{
    Debug.Log($"<color=lime>[CardDataExtended] CombatCardAdapter: Setting card {cardData.cardName}</color>");
    Debug.Log($"<color=lime>[CardDataExtended]   - Card Image: {(cardData.cardImage != null ? "✅ PRESENT" : "❌ NULL")}</color>");
    
    if (deckBuilderCard != null)
    {
        // Initialize DeckBuilderCardUI with CardDataExtended directly!
        deckBuilderCard.Initialize(cardData, null);
        
        Debug.Log($"<color=lime>[CardDataExtended] ✓ Card initialized through DeckBuilderCardUI!</color>");
    }
    else
    {
        Debug.LogError($"[CardDataExtended] DeckBuilderCardUI component not found!");
    }
}
```

**Testing**:
- Save file
- Check for compilation errors

---

## **PHASE 5: Create Migration Utility**

Create a tool to convert existing CardData to CardDataExtended.

### **File**: `Assets/Scripts/Editor/CardDataMigrationTool.cs` (NEW FILE)

```csharp
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Tool to migrate CardData assets to CardDataExtended
/// </summary>
public class CardDataMigrationTool : EditorWindow
{
    private List<CardData> cardsToMigrate = new List<CardData>();
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Cards/Migrate to CardDataExtended")]
    public static void ShowWindow()
    {
        GetWindow<CardDataMigrationTool>("Card Migration");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Card Data Migration Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool will find all CardData assets and help you migrate them to CardDataExtended.\n\n" +
            "CardDataExtended includes all CardData features PLUS combat features (weapon scaling, AoE, effects, etc.)",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Find All CardData Assets", GUILayout.Height(30)))
        {
            FindCardDataAssets();
        }
        
        EditorGUILayout.Space();
        
        if (cardsToMigrate.Count > 0)
        {
            EditorGUILayout.LabelField($"Found {cardsToMigrate.Count} CardData assets:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            foreach (CardData card in cardsToMigrate)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(card, typeof(CardData), false);
                
                bool isExtended = card is CardDataExtended;
                if (isExtended)
                {
                    GUI.color = Color.green;
                    GUILayout.Label("✓ Already Extended", GUILayout.Width(150));
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label("⚠ Needs Migration", GUILayout.Width(150));
                    GUI.color = Color.white;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Migrate All to CardDataExtended", GUILayout.Height(40)))
            {
                MigrateAllCards();
            }
            GUI.backgroundColor = Color.white;
        }
    }
    
    private void FindCardDataAssets()
    {
        cardsToMigrate.Clear();
        
        // Find all CardData assets
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (card != null)
            {
                cardsToMigrate.Add(card);
            }
        }
        
        Debug.Log($"Found {cardsToMigrate.Count} CardData assets");
    }
    
    private void MigrateAllCards()
    {
        int migratedCount = 0;
        int skippedCount = 0;
        
        foreach (CardData oldCard in cardsToMigrate)
        {
            // Skip if already extended
            if (oldCard is CardDataExtended)
            {
                skippedCount++;
                continue;
            }
            
            // Create new CardDataExtended asset
            CardDataExtended newCard = ScriptableObject.CreateInstance<CardDataExtended>();
            
            // Copy all CardData fields
            newCard.cardName = oldCard.cardName;
            newCard.cardType = oldCard.cardType;
            newCard.playCost = oldCard.playCost;
            newCard.description = oldCard.description;
            newCard.rarity = oldCard.rarity;
            newCard.element = oldCard.element;
            newCard.category = oldCard.category;
            newCard.cardImage = oldCard.cardImage;
            newCard.elementFrame = oldCard.elementFrame;
            newCard.costBubble = oldCard.costBubble;
            newCard.rarityFrame = oldCard.rarityFrame;
            newCard.damage = oldCard.damage;
            newCard.block = oldCard.block;
            newCard.isDiscardCard = oldCard.isDiscardCard;
            newCard.isDualWield = oldCard.isDualWield;
            newCard.ifDiscardedEffect = oldCard.ifDiscardedEffect;
            newCard.dualWieldEffect = oldCard.dualWieldEffect;
            
            // Set default combat properties
            newCard.primaryDamageType = GetDamageTypeFromElement(oldCard.element);
            newCard.scalesWithMeleeWeapon = (oldCard.category == CardCategory.Attack && oldCard.element == CardElement.Physical);
            newCard.isAoE = false;
            newCard.aoeTargets = 1;
            
            // Initialize scaling
            newCard.damageScaling = new AttributeScaling();
            if (oldCard.category == CardCategory.Attack)
            {
                newCard.damageScaling.strengthScaling = 1.0f; // Default STR scaling for attacks
            }
            
            newCard.guardScaling = new AttributeScaling();
            newCard.requirements = new CardRequirements();
            newCard.effects = new List<CardEffect>();
            newCard.tags = new List<string> { oldCard.category.ToString() };
            
            // Get original path and create new one
            string oldPath = AssetDatabase.GetAssetPath(oldCard);
            string directory = Path.GetDirectoryName(oldPath);
            string fileName = Path.GetFileNameWithoutExtension(oldPath);
            string newPath = Path.Combine(directory, fileName + "_Extended.asset");
            
            // Create the new asset
            AssetDatabase.CreateAsset(newCard, newPath);
            
            Debug.Log($"✓ Migrated: {oldCard.cardName} → {newPath}");
            migratedCount++;
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog(
            "Migration Complete",
            $"Migrated {migratedCount} cards to CardDataExtended.\n" +
            $"Skipped {skippedCount} cards (already extended).\n\n" +
            $"New assets created with '_Extended' suffix.\n" +
            $"Review and delete old CardData assets when ready.",
            "OK"
        );
        
        // Refresh the list
        FindCardDataAssets();
    }
    
    private DamageType GetDamageTypeFromElement(CardElement element)
    {
        switch (element)
        {
            case CardElement.Fire: return DamageType.Fire;
            case CardElement.Cold: return DamageType.Cold;
            case CardElement.Lightning: return DamageType.Lightning;
            case CardElement.Physical: return DamageType.Physical;
            case CardElement.Chaos: return DamageType.Chaos;
            default: return DamageType.Physical;
        }
    }
}
```

**Testing**:
- Save file
- Open Unity
- Check `Tools > Cards > Migrate to CardDataExtended` menu appears

---

## **PHASE 6: Migrate Existing Cards**

### **Steps**:

1. **Run Migration Tool**:
   - In Unity: `Tools > Cards > Migrate to CardDataExtended`
   - Click "Find All CardData Assets"
   - Review the list
   - Click "Migrate All to CardDataExtended"

2. **Update CardDatabase**:
   - Find `SetupCardDatabase` GameObject in scene
   - Select it
   - Right-click `SetupCardDatabase` component
   - Click "Setup Card Database"
   - This will add the new CardDataExtended assets to the database

3. **Verify Heavy Strike Migration**:
   - Navigate to `Assets/Resources/Cards/Marauder/`
   - You should see `HeavyStrike_Extended.asset`
   - Select it
   - In Inspector, verify:
     - ✅ Card Image is assigned
     - ✅ All properties copied
     - ✅ Combat properties available (weapon scaling, etc.)

4. **Update References** (if needed):
   - If any scripts reference specific CardData assets, update them to use CardDataExtended versions

---

## **PHASE 7: Test & Validate**

### **Test 1: Combat Scene**

1. Open Combat scene
2. Play
3. Draw cards
4. **Verify**:
   - ✅ Cards display correctly
   - ✅ Card images show up
   - ✅ No conversion logs in Console
   - ✅ Damage calculations work
   - ✅ Card costs display correctly

### **Test 2: Check Console Logs**

Look for:
```
✅ [CardDataExtended] Creating card: Heavy Strike
✅ [CardDataExtended]   - Card Image: ✅ LOADED
✅ [CardDataExtended] ✓ Card initialized through DeckBuilderCardUI!
```

**Should NOT see**:
```
❌ ConvertCardDataToCards()
❌ ConvertToCardData()
❌ CombatCardAdapter: Converted
```

### **Test 3: Validate Features**

1. Play an attack card
2. Check damage calculation
3. Verify weapon scaling works
4. Test mana costs
5. Test card effects

---

## **PHASE 8: Cleanup (Optional)**

After successful testing, you can:

1. **Mark Card class as Obsolete**:
   ```csharp
   [System.Obsolete("Use CardDataExtended instead")]
   public class Card { ... }
   ```

2. **Delete old CardData assets**:
   - After verifying migration, delete original CardData files
   - Keep only CardDataExtended versions

3. **Remove conversion methods**:
   - Delete `ConvertToCardData()` from CombatCardAdapter
   - Delete `ConvertCardDataToCards()` from CombatDeckManager

---

## **✅ Success Criteria**

Migration is complete when:

- [ ] Combat uses CardDataExtended directly
- [ ] No Card class conversions happen
- [ ] All card images display correctly
- [ ] All combat features work (damage, scaling, effects)
- [ ] Console shows CardDataExtended logs, not conversion logs
- [ ] Heavy Strike and other cards work in combat

---

## **🚨 Troubleshooting**

### **Issue**: Cards don't display images

**Solution**: 
1. Check CardDataExtended asset in Inspector
2. Verify `cardImage` field is assigned
3. Re-run migration tool if needed

### **Issue**: Compilation errors about Card vs CardDataExtended

**Solution**:
1. Find all references to `Card` in CombatDeckManager
2. Replace with `CardDataExtended`
3. Update method signatures

### **Issue**: CardDatabase doesn't include CardDataExtended

**Solution**:
1. Run `Tools > Cards > Setup Card Database`
2. Or manually drag CardDataExtended assets to database

---

## **📊 Before vs After**

### **Before (Circular Conversion)**
```
CardData → Card → temp CardData → Display
   ↑________↓
  Excessive!
```

### **After (Direct)**
```
CardDataExtended → Display
      ↓
   Simple!
```

---

**Ready to begin? Start with Phase 1!** 🚀



