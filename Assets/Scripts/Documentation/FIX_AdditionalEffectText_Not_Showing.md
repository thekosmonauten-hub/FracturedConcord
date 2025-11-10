# 🔧 Fix: AdditionalEffectText Not Showing Combo Descriptions

## Problem
The "AdditionalEffectText" field on cards is not being populated with combo descriptions from `CardDataExtended.comboDescription`.

## 🔍 Debugging Steps

### 1. Check Console Messages
Look for these debug messages when cards are displayed:

**Expected Messages:**
```
[CombatCardAdapter] Card 'Heavy Strike' combo description: 'Deals +2 damage if you played an Attack card this turn' (dynamic: '', static: 'Deals +2 damage if you played an Attack card this turn')
[CombatCardAdapter] Searching for AdditionalEffectText on CardPrefab(Clone). Found 5 TextMeshProUGUI components:
  - GameObject: 'CardName' (Active: True)
  - GameObject: 'CardDescription' (Active: True)
  - GameObject: 'AdditionalEffectText' (Active: True)
  - GameObject: 'ManaCost' (Active: True)
  - GameObject: 'Damage' (Active: True)
[CombatCardAdapter] Found matching GameObject: 'AdditionalEffectText'
[CombatCardAdapter] CardPrefab(Clone): AdditionalEffectText set to: "Deals +2 damage if you played an Attack card this turn"
```

### 2. Common Issues & Solutions

#### Issue 1: GameObject Name Mismatch
**Symptoms:** "AdditionalEffectText GameObject not found"
**Causes:**
- GameObject in prefab has different name
- GameObject is nested under different parent

**Solutions:**
1. **Check prefab hierarchy** - find the TextMeshProUGUI for additional effects
2. **Rename GameObject** to match expected names:
   - `AdditionalEffectText`
   - `Additional Effects` 
   - `AdditionalEffect`
3. **Check console** for "Available GameObjects with TextMeshProUGUI" list

#### Issue 2: Missing TextMeshProUGUI Component
**Symptoms:** GameObject found but no TextMeshProUGUI component
**Causes:**
- GameObject exists but has wrong component
- Component was removed or replaced

**Solutions:**
1. **Add TextMeshProUGUI component** to the GameObject
2. **Check component type** - must be `TMPro.TextMeshProUGUI`
3. **Verify it's enabled** and not disabled

#### Issue 3: Card Not CardDataExtended
**Symptoms:** "Card is not CardDataExtended, clearing additional effect text"
**Causes:**
- Using old `Card` class instead of `CardDataExtended`
- Card data not properly converted

**Solutions:**
1. **Check card data type** - must be `CardDataExtended`
2. **Verify card creation** uses extended data
3. **Update card instantiation** to use `CardDataExtended`

#### Issue 4: Empty Combo Description
**Symptoms:** Card is CardDataExtended but combo description is empty
**Causes:**
- `comboDescription` field not set in card data
- Dynamic combo description returns empty

**Solutions:**
1. **Check card asset** - ensure `comboDescription` is filled
2. **Verify dynamic description** - check `GetDynamicComboDescription()` method
3. **Test with static text** - set a simple combo description

### 3. Prefab Setup Checklist

**Required GameObject Structure:**
```
CardPrefab
├── VisualRoot
│   ├── CardName (TextMeshProUGUI)
│   ├── CardDescription (TextMeshProUGUI)
│   ├── AdditionalEffectText (TextMeshProUGUI) ← MUST EXIST
│   ├── ManaCost (TextMeshProUGUI)
│   └── Damage (TextMeshProUGUI)
```

**Required Components:**
- ✅ GameObject named exactly: `AdditionalEffectText`
- ✅ TextMeshProUGUI component attached
- ✅ GameObject active in hierarchy
- ✅ TextMeshProUGUI component enabled

### 4. Testing Steps

1. **Open a card in combat** or deck builder
2. **Check console** for debug messages
3. **Verify card data type** - should be CardDataExtended
4. **Check combo description** - should not be empty
5. **Verify GameObject found** - should find AdditionalEffectText
6. **Check text assignment** - should set the text content

### 5. Quick Fixes

**If GameObject not found:**
```csharp
// Add to UpdateAdditionalEffectText method
Debug.LogWarning($"[CombatCardAdapter] Available GameObjects:");
foreach (Transform child in transform)
{
    Debug.LogWarning($"  - '{child.name}'");
}
```

**If card not CardDataExtended:**
```csharp
// Check card data type
Debug.Log($"Card data type: {cardData.GetType()}");
Debug.Log($"Is CardDataExtended: {cardData is CardDataExtended}");
```

**If combo description empty:**
```csharp
// Check combo description values
Debug.Log($"Static combo: '{ext.comboDescription}'");
Debug.Log($"Dynamic combo: '{dyn}'");
Debug.Log($"Final text: '{comboText}'");
```

## 🎯 Expected Behavior

**Working correctly:**
- Console shows combo description being found and set
- AdditionalEffectText GameObject is found and activated
- Text content is set to the combo description
- Text is visible on the card

**Not working:**
- Console shows "GameObject not found" warnings
- No combo description in debug messages
- AdditionalEffectText remains hidden or empty

The enhanced debugging should help identify exactly where the issue is occurring! 🔧











