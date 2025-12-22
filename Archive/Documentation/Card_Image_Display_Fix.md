# Card Image Display Fix - Combat Scene

## 🎯 **Problem Statement**

Card images added to ScriptableObjects (like Heavy Strike) were not displaying in the combat scene, even though the images were properly assigned in the Unity Inspector.

---

## 🔍 **Root Cause Analysis**

### **Current System Architecture**

The combat scene uses a **hybrid approach** combining CardData (ScriptableObjects) and Card (JSON-based runtime objects):

```
CardDatabase (ScriptableObjects)
    ↓
CombatDeckManager loads CardData
    ↓
ConvertCardDataToCards() → Converts to Card objects ✅
    ↓ 
    cardData.cardImage → card.cardArt (LINE 391) ✅ Sprite copied!
    ↓
CardRuntimeManager creates visual GameObjects
    ↓
CombatCardAdapter.SetCard(card, character)
    ↓
CombatCardAdapter.ConvertToCardData(card) ❌ BUG HERE!
    ↓
Creates NEW temporary CardData
    ↓
❌ MISSING: cardData.cardImage = card.cardArt
    ↓
DeckBuilderCardUI.Initialize(cardData, null)
    ↓
DeckBuilderCardUI tries to display cardData.cardImage
    ↓
❌ NULL - No image displays!
```

### **The Bug**

**File**: `Assets/Scripts/UI/Combat/CombatCardAdapter.cs`  
**Method**: `ConvertToCardData(Card card, Character character)`  
**Lines**: 58-107

The method converted Card → CardData but **never copied the sprite**:

```csharp
private CardData ConvertToCardData(Card card, Character character)
{
    CardData cardData = ScriptableObject.CreateInstance<CardData>();
    
    cardData.cardName = card.cardName;      // ✅ Copied
    cardData.cardType = card.cardType;      // ✅ Copied
    cardData.playCost = card.manaCost;      // ✅ Copied
    cardData.description = card.description; // ✅ Copied
    
    // ❌ MISSING: cardData.cardImage = card.cardArt;
    
    return cardData;
}
```

---

## ✅ **Solution Applied**

### **Fix #1: Copy Card Artwork in Primary Path**

**File**: `CombatCardAdapter.cs`  
**Line**: 85-86

```csharp
// Visual Assets - CRITICAL: Copy card artwork sprite!
cardData.cardImage = card.cardArt;
Debug.Log($"[CardArt] CombatCardAdapter: Converted {card.cardName} - cardImage: {(cardData.cardImage != null ? "✅ SET" : "❌ NULL")}");
```

### **Fix #2: Handle Card Artwork in Fallback Path**

**File**: `CombatCardAdapter.cs`  
**Lines**: 122-165

```csharp
// Find card art image - CRITICAL for displaying artwork!
Image cardArtImage = transform.Find("CardImage")?.GetComponent<Image>();
if (cardArtImage == null)
    cardArtImage = transform.Find("Card Image")?.GetComponent<Image>();

// Update card artwork - CRITICAL!
if (cardArtImage != null && card.cardArt != null)
{
    cardArtImage.sprite = card.cardArt;
    cardArtImage.enabled = true;
    Debug.Log($"[CardArt] ✓ Fallback path: Card art displayed for {card.cardName}!");
}
```

---

## 🧪 **Testing Instructions**

### **1. Verify Heavy Strike Has Card Image**

1. In Unity, navigate to: `Assets/Resources/Cards/Marauder/HeavyStrike` (or wherever it's located)
2. Select the **Heavy Strike** ScriptableObject
3. In Inspector, check the **Visual Assets** section
4. Ensure **Card Image** field has a sprite assigned (e.g., `HeavyStrike.png`)

### **2. Test in Combat Scene**

1. Play the Combat scene
2. Draw cards (should draw Heavy Strike since Marauder deck has 6 copies)
3. Check Console for debug logs:
   ```
   [CardArt] CombatDeckManager: Converted CardData to Card: Heavy Strike
   [CardArt]   - CardData.cardImage: ✅ LOADED
   [CardArt]   - Card.cardArt: ✅ LOADED
   
   [CardArt] CombatCardAdapter: Converted Heavy Strike - cardImage: ✅ SET
   ```
4. **Expected Result**: Heavy Strike card should now display its artwork!

### **3. Verify Other Cards**

Test with other Marauder cards:
- Brace
- Ground Slam
- Cleave
- Endure
- Intimidating Shout

All cards with assigned `cardImage` in their ScriptableObjects should now display correctly.

---

## 📊 **System Overview**

### **Which System is Being Used?**

**Answer**: **BOTH**, in a hybrid approach:

| Stage | System | Data Type |
|-------|--------|-----------|
| **Storage** | CardData (ScriptableObjects) | CardDatabase |
| **Combat Runtime** | Card (JSON-style objects) | Converted from CardData |
| **Visualization** | Card → CardData (temp) | Adapter converts back |

### **Why This Approach?**

1. **CardData (ScriptableObjects)**: 
   - Easy to edit in Unity Inspector
   - Visual asset management
   - Persistent storage

2. **Card (Runtime Objects)**:
   - Full-featured combat system
   - Dynamic calculations
   - Effect system
   - Weapon scaling

3. **Adapter Pattern**:
   - Bridges CardData and Card systems
   - Allows using DeckBuilderCardUI prefab in combat
   - Provides character-specific calculations

---

## 🚨 **Potential Issues & Edge Cases**

### **Issue #1: CardData Not in Database**

**Symptom**: Card loads but has no image  
**Cause**: CardData exists in Resources but not added to CardDatabase  
**Solution**: Run `Tools > Cards > Setup Card Database` in Unity

### **Issue #2: Sprite Not Assigned**

**Symptom**: Debug shows "cardImage: ❌ NULL"  
**Cause**: CardData ScriptableObject missing cardImage assignment  
**Solution**: Manually assign sprite in Inspector

### **Issue #3: Card Prefab Missing "CardImage" Component**

**Symptom**: Fallback path activates but no image displays  
**Cause**: Card prefab doesn't have Image component named "CardImage"  
**Solution**: Add Image component to card prefab hierarchy

### **Issue #4: Wrong Visualizer Being Used**

**Symptom**: CardVisualizer used instead of CombatCardAdapter  
**Cause**: Card prefab doesn't have DeckBuilderCardUI component  
**Debug**: Check which visualizer is used in Console logs  
**Solution**: Ensure card prefab has proper components

---

## 🎨 **Architecture Recommendations**

### **Short-Term**

✅ **DONE**: Fixed CombatCardAdapter to copy cardImage  
✅ **DONE**: Added debug logging for troubleshooting  
✅ **DONE**: Enhanced fallback path to handle card art  

### **Medium-Term Improvements**

1. **Consolidate to Single System**
   - Choose either CardData OR Card as the primary system
   - Reduces conversion overhead and potential bugs

2. **Add Validation**
   - Create Editor script to validate all CardData have card images
   - Warn when cardImage is NULL

3. **Improve DeckBuilderCardUI Integration**
   - Make DeckBuilderCardUI work natively with Card objects
   - Eliminate need for conversion

### **Long-Term Architecture**

Consider migrating to **pure ScriptableObject workflow**:

```
CardData (ScriptableObjects only)
    ↓
CardRuntimeManager creates visuals directly from CardData
    ↓
No conversion needed!
```

**Benefits**:
- Single source of truth
- No conversion bugs
- Simpler codebase
- Better Unity Editor integration

**Challenges**:
- Need to extend CardData with all Card features
- Migrate existing Card-based systems
- Update all references

---

## 📝 **Files Modified**

| File | Changes | Lines |
|------|---------|-------|
| `Assets/Scripts/UI/Combat/CombatCardAdapter.cs` | Added cardImage copying | 85-86 |
| `Assets/Scripts/UI/Combat/CombatCardAdapter.cs` | Enhanced fallback path | 122-165 |
| `Assets/Documentation/Card_Image_Display_Fix.md` | Created this documentation | All |

---

## 🎯 **Summary**

### **What Was Broken**
Card images from ScriptableObjects weren't displaying because `CombatCardAdapter` didn't copy the sprite when converting Card → CardData.

### **What Was Fixed**
Added `cardData.cardImage = card.cardArt;` to copy the sprite during conversion, ensuring images display correctly in combat.

### **How to Verify**
1. Check Heavy Strike ScriptableObject has cardImage assigned
2. Play Combat scene
3. Draw cards and verify images display
4. Check Console for confirmation logs

### **Next Steps**
1. Test all Marauder cards
2. Assign card images to other class decks
3. Consider architecture consolidation for future

---

**Status**: ✅ **FIXED**  
**Date**: October 14, 2025  
**Tested**: Pending user verification



