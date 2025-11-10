# 🧪 **TESTING AFFIX GENERATION - COMPLETE GUIDE**

## 🎯 **HOW TO TEST RANDOM ITEM GENERATION WITH AFFIXES**

This guide shows you **multiple ways** to test your affix system and verify that items are being generated correctly with the right rarity rules!

---

## 🚀 **METHOD 1: USING RARITYAFFIXTESTER (EASIEST - RECOMMENDED)**

This is the **fastest and most comprehensive** way to test your affix system!

### **STEP 1: Set Up the Tester**

1. **Open Unity Editor**
2. **In Hierarchy**, create a new GameObject:
   - Right-click → Create Empty
   - Name it: `AffixTester`
3. **Add the RarityAffixTester component**:
   - Select `AffixTester` GameObject
   - In Inspector → Add Component
   - Search for: `RarityAffixTester`
   - Click to add

### **STEP 2: Configure Test Settings**

In the Inspector, you'll see:

```
RarityAffixTester Component:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Test Area Level: 80 ← Change this to test different levels
Test Items Per Rarity: 10 ← How many items to generate
Show Detailed Affix Info: ☑ ← Check this for full details
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

**Configuration Options:**
- `Test Area Level: 1` → Tests starter items (Tier 9 affixes only)
- `Test Area Level: 30` → Tests mid-game items (Tier 6-9 affixes)
- `Test Area Level: 80` → Tests endgame items (ALL tiers)

### **STEP 3: Run Tests**

**Right-click** the `RarityAffixTester` component in Inspector and select:

#### **Option A: Test All Rarities**
- Generates 10 Normal, 10 Magic, 10 Rare items
- Shows comprehensive breakdown

#### **Option B: Test Magic Items**
- Generates 10 Magic items (1-2 affixes each)
- Verifies Magic rarity rules

#### **Option C: Test Rare Items**
- Generates 10 Rare items (3-6 affixes each)
- Verifies Rare rarity rules

#### **Option D: Test Rarity Distribution**
- Tests the drop rate probabilities
- Shows % of Normal, Magic, Rare drops

### **STEP 4: Read the Console Output**

**Example Output:**
```
--- TESTING RARE RARITY ITEMS ---
Generated 10 Rare items

Rare Item #1:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Legendary Iron Sword of Power
Level: 80 | Type: Weapon | Rarity: Rare
Base Damage: 12-18
Final Damage: 376-668 Physical & Fire Damage ✅

Prefixes (3):
  ✅ PREFIX: Devastating - Adds (34-47) to (72-84) Physical Damage (Tier: Tier1, Level: 80)
     → Stat: addedPhysicalDamage [Dual-Range: True] ✅
     → Rolled: 41 to 78 ✅
     → Scope: Local ✅
     
  ✅ PREFIX: Apocalyptic Flame - Adds (89-121) to (180-210) Fire Damage (Tier: Tier1, Level: 80)
     → Stat: addedFireDamage [Dual-Range: True] ✅
     → Rolled: 105 to 195 ✅
     → Scope: Local ✅
     
  ✅ PREFIX: Tyrannical - +92% increased Physical Damage (Tier: Tier1, Level: 80)
     → Stat: increasedPhysicalDamage: 85-99 (Increased) ✅
     → Scope: Local ✅

Suffixes (3):
  ✅ SUFFIX: of the Inferno - +41-42% Fire Resistance (Tier: Tier1, Level: 80)
     → Stat: fireResistance: 41-42 (Increased) ✅
     → Scope: Global ✅
     
  ✅ SUFFIX: of the Titan - +57-60 to Strength (Tier: Tier1, Level: 80)
     → Stat: strength: 57-60 (Flat) ✅
     → Scope: Global ✅
     
  ✅ SUFFIX: of Precision - +28-32% Attack Speed (Tier: Tier1, Level: 80)
     → Stat: attackSpeed: 28-32 (Increased) ✅
     → Scope: Global ✅

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Rare Item #2:
... (9 more items)
```

### **WHAT TO LOOK FOR:**

✅ **Correct Rarity Counts:**
- Normal items: 0 affixes
- Magic items: 1-2 affixes (at least 1)
- Rare items: 3-6 affixes (at least 3)

✅ **Dual-Range Working:**
- See "Dual-Range: True" for flat damage affixes
- See "Rolled: X to Y" showing actual rolled values
- Min damage uses first roll, max damage uses second roll

✅ **Scope Correct:**
- Local: Physical/elemental damage on weapons, % armour/evasion/ES on armor
- Global: Resistances, attributes, crit, accuracy

✅ **Level Gating:**
- Level 1 items only have Tier 9 affixes (minLevel 1)
- Level 80 items can have Tier 1 affixes (minLevel 80)
- No affixes with minLevel > item level

---

## 🎮 **METHOD 2: USING AREALOOTMANAGER DIRECTLY**

Use this method if you want to integrate item generation into your game systems!

### **STEP 1: Create Test Script**

Create a new file: `Assets/Scripts/Testing/QuickItemGeneratorTest.cs`

```csharp
using UnityEngine;

public class QuickItemGeneratorTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public int testAreaLevel = 80;
    public ItemRarity forcedRarity = ItemRarity.Rare;
    public int itemsToGenerate = 5;
    
    [ContextMenu("Generate Random Items")]
    public void GenerateRandomItems()
    {
        Debug.Log($"=== GENERATING {itemsToGenerate} ITEMS (Area Level {testAreaLevel}) ===\n");
        
        for (int i = 0; i < itemsToGenerate; i++)
        {
            // Generate item with random rarity
            BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel);
            
            if (item != null)
            {
                LogItemDetails(item, i + 1);
            }
            else
            {
                Debug.LogWarning($"Item {i + 1}: Failed to generate (null result)");
            }
        }
        
        Debug.Log("=== GENERATION COMPLETE ===");
    }
    
    [ContextMenu("Generate Rare Items (Forced)")]
    public void GenerateRareItems()
    {
        Debug.Log($"=== GENERATING {itemsToGenerate} RARE ITEMS (Area Level {testAreaLevel}) ===\n");
        
        for (int i = 0; i < itemsToGenerate; i++)
        {
            // Generate item with forced Rare rarity
            BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel, ItemRarity.Rare);
            
            if (item != null)
            {
                LogItemDetails(item, i + 1);
            }
        }
        
        Debug.Log("=== GENERATION COMPLETE ===");
    }
    
    [ContextMenu("Generate Magic Items (Forced)")]
    public void GenerateMagicItems()
    {
        Debug.Log($"=== GENERATING {itemsToGenerate} MAGIC ITEMS (Area Level {testAreaLevel}) ===\n");
        
        for (int i = 0; i < itemsToGenerate; i++)
        {
            BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(testAreaLevel, ItemRarity.Magic);
            
            if (item != null)
            {
                LogItemDetails(item, i + 1);
            }
        }
        
        Debug.Log("=== GENERATION COMPLETE ===");
    }
    
    private void LogItemDetails(BaseItem item, int index)
    {
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log($"Item #{index}: {item.GetDisplayName()}");
        Debug.Log($"Type: {item.itemType} | Rarity: {item.GetCalculatedRarity()} | Level: {item.requiredLevel}");
        
        // Log weapon damage if applicable
        if (item is WeaponItem weapon)
        {
            Debug.Log($"Damage: {weapon.GetTotalMinDamage()}-{weapon.GetTotalMaxDamage()}");
            Debug.Log($"Crit Chance: {weapon.criticalStrikeChance}% | Attack Speed: {weapon.attackSpeed}");
        }
        
        // Log armor defense if applicable
        if (item is Armour armor)
        {
            if (armor.armour > 0) Debug.Log($"Armour: {armor.armour}");
            if (armor.evasion > 0) Debug.Log($"Evasion: {armor.evasion}");
            if (armor.energyShield > 0) Debug.Log($"Energy Shield: {armor.energyShield}");
        }
        
        // Log affixes
        Debug.Log($"Affixes: {item.prefixes.Count} Prefix(es) + {item.suffixes.Count} Suffix(es)");
        
        foreach (var affix in item.prefixes)
        {
            Debug.Log($"  ✅ PREFIX: {affix.name} - {affix.description} (Tier: {affix.tier}, MinLevel: {affix.minLevel})");
        }
        
        foreach (var affix in item.suffixes)
        {
            Debug.Log($"  ✅ SUFFIX: {affix.name} - {affix.description} (Tier: {affix.tier}, MinLevel: {affix.minLevel})");
        }
        
        Debug.Log("");
    }
}
```

### **STEP 2: Add to Scene**

1. In Hierarchy → Create Empty → Name: `ItemGeneratorTest`
2. Add Component → `QuickItemGeneratorTest`
3. Configure in Inspector:
   - Test Area Level: `80` (or `1`, `30`, etc.)
   - Items To Generate: `5`

### **STEP 3: Run Tests**

Right-click the component in Inspector:
- `Generate Random Items` → Mix of Normal/Magic/Rare based on drop weights
- `Generate Rare Items (Forced)` → All Rare items
- `Generate Magic Items (Forced)` → All Magic items

---

## 🔬 **METHOD 3: USING PLAY MODE (IN-GAME TESTING)**

Test item generation during actual gameplay!

### **STEP 1: Set Up Debug Key**

Add this to an existing MonoBehaviour (or create a new one):

```csharp
using UnityEngine;

public class DebugItemGenerator : MonoBehaviour
{
    void Update()
    {
        // Press F5 to generate a random item
        if (Input.GetKeyDown(KeyCode.F5))
        {
            GenerateDebugItem();
        }
        
        // Press F6 to generate a Rare item
        if (Input.GetKeyDown(KeyCode.F6))
        {
            GenerateDebugRareItem();
        }
    }
    
    void GenerateDebugItem()
    {
        int areaLevel = 80; // Or use your current area level
        BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(areaLevel);
        
        if (item != null)
        {
            Debug.Log($"Generated: {item.GetDisplayName()} | Rarity: {item.GetCalculatedRarity()} | Affixes: {item.prefixes.Count}P + {item.suffixes.Count}S");
            
            // Add item to player inventory here
            // InventoryManager.Instance.AddItem(item);
        }
    }
    
    void GenerateDebugRareItem()
    {
        int areaLevel = 80;
        BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(areaLevel, ItemRarity.Rare);
        
        if (item != null)
        {
            Debug.Log($"Generated Rare: {item.GetDisplayName()} | Affixes: {item.prefixes.Count}P + {item.suffixes.Count}S");
        }
    }
}
```

### **STEP 2: Play and Test**

1. Press **Play** in Unity
2. Press **F5** to generate random items (mixed rarities)
3. Press **F6** to generate Rare items (forced)
4. Check Console for results

---

## 📊 **METHOD 4: TIER SYSTEM VALIDATOR (LEVEL PROGRESSION)**

Test that the tier/level system works correctly!

### **STEP 1: Use TierSystemValidator**

1. In Hierarchy → Create Empty → Name: `TierValidator`
2. Add Component → `TierSystemValidator`
3. Right-click component → Select test:
   - `Test Level 1 Generation` → Should only get Tier 9 affixes
   - `Test Level 30 Generation` → Should get Tier 6-9 affixes
   - `Test Level 80 Generation` → Should get ALL tier affixes

### **STEP 2: Verify Tier Distribution**

**Expected Results:**

**Level 1 Items:**
```
✅ Should ONLY see:
- Tier 9 affixes (minLevel 1)

❌ Should NEVER see:
- Tier 1 affixes (minLevel 80)
- Tier 2 affixes (minLevel 70)
- etc.
```

**Level 80 Items:**
```
✅ Should see:
- Mix of ALL tiers (Tier 9 through Tier 1)
- Higher tiers more common (due to recency)
```

---

## 🎯 **QUICK VERIFICATION CHECKLIST**

After running tests, verify these critical points:

### **✅ RARITY RULES:**
```
☑ Normal items have 0 affixes
☑ Magic items have 1-2 affixes (never 0)
☑ Rare items have 3-6 affixes (never < 3)
☑ At least 1 prefix OR 1 suffix (for Magic)
☑ At least 1 prefix AND 1 suffix (for Rare)
```

### **✅ DUAL-RANGE DAMAGE:**
```
☑ Flat damage affixes show "isDualRange: true"
☑ Two separate values rolled (e.g., "41 to 78")
☑ First value added to weapon minDamage
☑ Second value added to weapon maxDamage
☑ Final damage range makes sense (min < max)
```

### **✅ LOCAL VS GLOBAL:**
```
☑ % Physical Damage on WEAPON = Local ✅
☑ % Fire Damage on WEAPON = Local ✅
☑ % Armour on ARMOR = Local ✅
☑ Fire Resistance on ANY item = Global ✅
☑ Attributes on ANY item = Global ✅
☑ Critical Strike on ANY item = Global ✅
```

### **✅ LEVEL GATING:**
```
☑ Level 1 items ONLY have Tier 9 affixes (minLevel 1)
☑ Level 30 items have Tier 6-9 affixes (minLevel 1-30)
☑ Level 80 items have ALL tier affixes (minLevel 1-80)
☑ NO affixes with minLevel > item level
```

### **✅ SMART COMPATIBILITY:**
```
☑ +% Energy Shield ONLY on ES base armor (energyShield > 0)
☑ +% Armour ONLY on Armour base armor (armour > 0)
☑ +% Evasion ONLY on Evasion base armor (evasion > 0)
☑ Block Chance ONLY on Shields
☑ Weapon damage affixes ONLY on weapons
```

---

## 🚨 **TROUBLESHOOTING**

### **Problem: All items are Normal (0 affixes)**

**Cause:** AffixDatabase might not be loaded or have affixes

**Solution:**
1. Check that you've imported affixes from CSV
2. Verify `AffixDatabase.asset` has populated categories
3. Check Console for "AffixDatabase.Instance is null" warnings

---

### **Problem: "No drop (failed base chance)"**

**Cause:** AreaLootTable.baseDropChance is too low

**Solution:**
1. Open your AreaLootTable asset
2. Set `baseDropChance` to `1.0` for testing
3. Items will now always drop

---

### **Problem: Items generate but have wrong affixes**

**Cause:** Affix compatibility or level filtering issues

**Solution:**
1. Check item tags (use `Debug.Log(string.Join(", ", item.itemTags))`)
2. Check affix compatibleTags match item tags
3. Verify affix minLevel <= item level

---

### **Problem: Dual-range not working**

**Cause:** Affixes weren't imported with dual-range format

**Solution:**
1. Re-import affixes from updated CSV
2. Check CSV has format: "Adds (X-Y) to (Z-W) Damage"
3. Verify `isDualRange = true` in generated affixes

---

## 📈 **EXPECTED TEST RESULTS**

Here's what **good** test output looks like:

### **Test Level 80 Rare Items:**
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Generated 10 Rare items

Item #1: Legendary Sword of the Titan
- Prefixes: 3 (Devastating, Apocalyptic Flame, Tyrannical)
- Suffixes: 3 (of the Inferno, of the Titan, of Precision)
- Damage: 376-668 ✅
- All affixes Tier 1 (Level 80) ✅

Item #2: Mythic Axe of Devastation
- Prefixes: 2 (Glacial, Tyrannical)
- Suffixes: 4 (of Frost, of the Colossus, of the Cheetah, of Accuracy)
- Damage: 298-512 ✅
- Mix of Tier 1-3 affixes ✅

... (8 more items)

✅ All items have 3-6 affixes
✅ All items have at least 1 prefix AND 1 suffix
✅ Dual-range damage working
✅ Affixes appropriate for item level
```

---

## 🎮 **INTEGRATION WITH YOUR GAME**

Once testing is complete, integrate into your game systems:

### **Enemy Drops:**
```csharp
void OnEnemyDeath(Enemy enemy)
{
    int areaLevel = GetCurrentAreaLevel();
    BaseItem drop = AreaLootManager.Instance.GenerateSingleItemForArea(areaLevel);
    
    if (drop != null)
    {
        DropItemAtPosition(drop, enemy.transform.position);
    }
}
```

### **Chest Loot:**
```csharp
void OpenChest(Chest chest)
{
    int areaLevel = GetCurrentAreaLevel();
    List<BaseItem> loot = new List<BaseItem>();
    
    for (int i = 0; i < 3; i++) // 3 items per chest
    {
        BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(areaLevel);
        if (item != null) loot.Add(item);
    }
    
    ShowLootWindow(loot);
}
```

### **Rare Drop from Boss:**
```csharp
void OnBossDeath(Boss boss)
{
    int areaLevel = GetCurrentAreaLevel();
    
    // Boss always drops a Rare item
    BaseItem rareItem = AreaLootManager.Instance.GenerateSingleItemForArea(areaLevel, ItemRarity.Rare);
    
    if (rareItem != null)
    {
        DropItemAtPosition(rareItem, boss.transform.position);
    }
}
```

---

## 🏆 **YOU'RE READY TO TEST!**

### **RECOMMENDED TESTING ORDER:**

1. ✅ **Import your 434 affixes** (if not done yet)
2. ✅ **Method 1: Use RarityAffixTester** → Comprehensive verification
3. ✅ **Method 4: Use TierSystemValidator** → Verify level progression
4. ✅ **Method 2: Create QuickItemGeneratorTest** → Custom testing
5. ✅ **Method 3: Add DebugItemGenerator** → In-game testing

### **EXPECTED TIME:**
- Import affixes: **30 seconds**
- Run RarityAffixTester: **5 seconds**
- Verify output: **2 minutes**
- **Total: 3 minutes to full verification!**

---

## 🎯 **SUMMARY**

You now have **4 complete methods** to test your affix system:

1. **RarityAffixTester** → Most comprehensive, best for verification
2. **QuickItemGeneratorTest** → Custom script, flexible testing
3. **DebugItemGenerator** → In-game runtime testing
4. **TierSystemValidator** → Level/tier progression validation

**Pick the method that fits your workflow and start generating legendary loot!** 🎮⚔️🛡️👑🔥

**ALL SYSTEMS READY - START TESTING NOW!** 🚀✅







