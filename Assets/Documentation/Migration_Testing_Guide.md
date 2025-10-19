# CardDataExtended Migration - Testing Guide

## ✅ Implementation Complete!

All code changes have been implemented. Now it's time to test!

---

## 📋 **What Was Changed**

### **Files Modified:**

1. **`CardDataExtended.cs`** - Added helper methods (GetCardTypeEnum, ToCard, etc.)
2. **`CombatDeckManager.cs`** - Changed from `Card` to `CardDataExtended` throughout
3. **`CardRuntimeManager.cs`** - Added `CreateCardFromCardDataExtended()` method
4. **`CombatCardAdapter.cs`** - Added `SetCardDataExtended()` method
5. **`CardDataMigrationTool.cs`** - Created new Editor tool

### **What Changed:**

- ✅ Deck piles now use `List<CardDataExtended>` instead of `List<Card>`
- ✅ All card loading methods use CardDataExtended directly
- ✅ **REMOVED** `ConvertCardDataToCards()` method (no more conversion!)
- ✅ **REMOVED** `ParseCardType()` helper method (no longer needed)
- ✅ Card display uses CardDataExtended without temp conversions
- ✅ Migration tool ready to convert existing cards

---

## 🧪 **PHASE 6: Run Migration Tool**

### **Step 1: Open Migration Tool**

1. In Unity, go to: **`Tools > Cards > Migrate to CardDataExtended`**
2. A window titled "Card Migration" should appear

### **Step 2: Find Existing Cards**

1. Click **"Find All CardData Assets"**
2. Review the list:
   - ✓ **Green = Already CardDataExtended** (ready to use)
   - ⚠ **Yellow = Needs Migration** (old CardData format)

### **Step 3: Migrate Cards**

**Option A: Migrate All (Recommended)**

1. Click **"Migrate All X Cards"** button (big cyan button)
2. Wait for migration to complete
3. Dialog will show results

**Option B: Migrate Individual Cards**

1. Find a specific card in the list
2. Click **"Migrate →"** button next to it
3. Repeat for each card

### **Step 4: Verify Migration**

After migration, you should see files like:
```
Assets/Resources/Cards/Marauder/
├── HeavyStrike.asset              (old - CardData)
├── HeavyStrike_Extended.asset     (new - CardDataExtended) ✅
├── Brace.asset                    (old)
├── Brace_Extended.asset           (new) ✅
└── ...
```

### **Step 5: Update CardDatabase**

1. Find **`SetupCardDatabase`** GameObject in your scene (or create one)
2. Add **`SetupCardDatabase`** component if not present
3. Right-click the component in Inspector
4. Click **"Setup Card Database"**
5. This will add the new CardDataExtended assets to the database

**Verify in Console:**
```
Found 6 CardDataExtended assets
✓ Added: HeavyStrike_Extended
✓ Added: Brace_Extended
...
CardDatabase created with 6 cards
```

### **Step 6: Verify Heavy Strike**

1. Navigate to: `Assets/Resources/Cards/Marauder/HeavyStrike_Extended.asset`
2. Select it in Project window
3. In Inspector, verify:
   - ✅ **Card Name**: "Heavy Strike"
   - ✅ **Card Image**: Sprite is assigned
   - ✅ **Combat Properties**: Visible (weapon scaling, AoE, etc.)
   - ✅ **Damage**: 15 (or whatever value it should be)

---

## 🎮 **PHASE 7: Test in Combat Scene**

### **Test 1: Basic Combat Test**

1. **Open Combat Scene**
2. **Play the scene**
3. **Wait for initial hand to draw**

**Expected Behavior:**
- ✅ 5 cards appear in hand
- ✅ Cards display correctly (name, cost, damage, description)
- ✅ **Card images show up** (this was the original bug!)
- ✅ Cards are clickable

**Console Logs to Look For:**
```
[CardDataExtended] Loading Marauder deck from CardDatabase...
[CardDataExtended] Found 6 Marauder cards in CardDatabase
[CardDataExtended] ✓ Found extended card: Heavy Strike
[CardDataExtended] Added 6 copies of Heavy Strike to deck
...
[CardDataExtended] Creating card: Heavy Strike
[CardDataExtended]   - Card Image: ✅ LOADED
[CardDataExtended] ✓ Set card via CombatCardAdapter
```

**Should NOT See:**
```
❌ ConvertCardDataToCards()
❌ ConvertToCardData()
❌ [CardArt] Converted CardData to Card
```

### **Test 2: Play a Card**

1. **Click on Heavy Strike card** (or any Attack card)
2. Card should:
   - ✅ Fly toward enemy
   - ✅ Apply damage
   - ✅ Fly to discard pile
   - ✅ Disappear (return to pool)

**Console Logs:**
```
Drawing card #1: Heavy Strike
[CardDataExtended] Creating card: Heavy Strike
[CardDataExtended]   - Card Image: ✅ LOADED
Playing card: Heavy Strike
Spent 1 mana to play Heavy Strike
```

### **Test 3: Verify Image Display**

1. **Draw cards until you get Heavy Strike**
2. **Verify the card image displays:**
   - ✅ Image shows in the card UI
   - ✅ Not a blank/missing sprite
   - ✅ Correct artwork for Heavy Strike

If image doesn't show:
- Check `HeavyStrike_Extended.asset` has cardImage assigned
- Check Console for errors
- Verify CardDatabase includes the Extended version

### **Test 4: Damage Calculation**

1. **Note enemy health before attack**
2. **Play Heavy Strike**
3. **Verify damage is applied correctly:**
   - ✅ Enemy health decreases
   - ✅ Damage number shows on screen
   - ✅ Damage scales with character stats (if applicable)

### **Test 5: Multiple Cards**

1. **Draw 5+ cards**
2. **Play several cards**
3. **Verify:**
   - ✅ All cards display correctly
   - ✅ Card images show for all cards
   - ✅ Mana cost deducts properly
   - ✅ Cards with insufficient mana show correctly

---

## 🔍 **Verification Checklist**

### **Migration Verification**

- [ ] Migration tool opens successfully (`Tools > Cards > Migrate to CardDataExtended`)
- [ ] All cards found and listed
- [ ] Migration creates `_Extended.asset` files
- [ ] CardDatabase includes extended cards
- [ ] Heavy Strike extended card has image assigned

### **Combat Scene Verification**

- [ ] Cards load without errors
- [ ] 5 cards draw on combat start
- [ ] **Card images display** (main bug fix!)
- [ ] Card text displays correctly (name, cost, damage, description)
- [ ] Cards are clickable/playable
- [ ] Damage applies correctly
- [ ] Mana costs work
- [ ] Cards animate (draw, play, discard)

### **Console Log Verification**

- [ ] See `[CardDataExtended]` logs, not `[CardArt]` logs
- [ ] NO `ConvertCardDataToCards()` logs
- [ ] NO `ConvertToCardData()` logs
- [ ] Card Image shows as `✅ LOADED`, not `❌ NULL`

---

## 🚨 **Troubleshooting**

### **Issue 1: "No CardDataExtended cards found"**

**Symptoms:**
```
[CardDataExtended] No Marauder cards found in CardDatabase
```

**Solutions:**
1. Run migration tool: `Tools > Cards > Migrate to CardDataExtended`
2. Click "Migrate All Cards"
3. Run `Tools > Cards > Setup Card Database` to update database

---

### **Issue 2: "Card Image: ❌ NULL"**

**Symptoms:**
- Cards display but no images show
- Console shows `Card Image: ❌ NULL`

**Solutions:**
1. Open the CardDataExtended asset in Inspector
2. Check if **Card Image** field has a sprite assigned
3. If not, drag the sprite into the field
4. Save the asset

---

### **Issue 3: Compilation Errors**

**Symptoms:**
- Unity shows errors in Console
- Can't enter Play mode

**Solutions:**
1. Check Console for specific error messages
2. Common issues:
   - Missing `using` statements
   - Typos in method names
   - Missing method implementations
3. Share error message for help

---

### **Issue 4: Cards Don't Draw**

**Symptoms:**
- Combat starts but hand is empty
- No cards visible

**Solutions:**
1. Check Console for errors
2. Verify CardDatabase has cards:
   - Select CardDatabase asset
   - Check `allCards` list has entries
3. Verify `testLoadMarauderDeckOnStart` is enabled in CombatDeckManager

---

### **Issue 5: Migration Tool Doesn't Appear**

**Symptoms:**
- `Tools > Cards` menu exists but no "Migrate to CardDataExtended" option

**Solutions:**
1. Check `CardDataMigrationTool.cs` is in `Assets/Scripts/Editor/` folder
2. Check for compilation errors
3. Restart Unity Editor
4. Check file has correct `[MenuItem]` attribute

---

## 🎯 **Success Criteria**

Migration is successful when ALL of these are true:

- ✅ Combat scene loads without errors
- ✅ Cards draw correctly (5 cards on start)
- ✅ **Card images display** (Heavy Strike shows artwork)
- ✅ Card text is correct (name, cost, damage, description)
- ✅ Cards are playable (click to use)
- ✅ Damage applies correctly
- ✅ Mana costs work
- ✅ Console shows `[CardDataExtended]` logs, not conversion logs
- ✅ No circular conversion happening

---

## 📊 **Before vs After**

### **BEFORE (Circular Conversion)**
```
CardData → Card → temp CardData → Display
   ↑________↓
  Excessive!
```

**Console Logs:**
```
[CardArt] Converted CardData to Card: Heavy Strike
[CardArt]   - CardData.cardImage: ✅ LOADED
[CardArt]   - Card.cardArt: ✅ LOADED
[CardArt] CombatCardAdapter: Converted Heavy Strike - cardImage: ✅ SET
```

### **AFTER (Direct Usage)**
```
CardDataExtended → Display
      ↓
   Simple!
```

**Console Logs:**
```
[CardDataExtended] Found extended card: Heavy Strike
[CardDataExtended] Creating card: Heavy Strike
[CardDataExtended]   - Card Image: ✅ LOADED
[CardDataExtended] ✓ Set card via CombatCardAdapter
```

---

## 📝 **Next Steps After Testing**

### **If Everything Works:**

1. ✅ **Clean up old CardData assets** (optional):
   - Delete `HeavyStrike.asset` (keep `HeavyStrike_Extended.asset`)
   - Only do this after thorough testing!

2. ✅ **Migrate other class decks:**
   - Run migration tool for Witch, Ranger, Brawler, etc.
   - Update `GetCardNamesForClass()` in CombatDeckManager

3. ✅ **Update documentation:**
   - Mark Card class as `[Obsolete]` (optional)
   - Update any README files

### **If Something Doesn't Work:**

1. 🐛 **Check Console for errors**
2. 🐛 **Review this troubleshooting guide**
3. 🐛 **Share error messages for help**

---

## 🎉 **Migration Complete!**

Once all tests pass, you've successfully:

- ✅ Eliminated circular conversion
- ✅ Fixed card image display bug
- ✅ Improved performance (no runtime conversions)
- ✅ Simplified architecture (single source of truth)
- ✅ Made system easier to maintain

**Congrats! Your card system is now production-ready!** 🚀

---

**Questions? Issues? Check the troubleshooting section or ask for help!**



