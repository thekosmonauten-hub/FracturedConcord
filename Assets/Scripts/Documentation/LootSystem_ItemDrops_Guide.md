# Loot System - Item Drops Quick Add Guide

## ✨ New Feature: Quick Add Item Drops

You can now easily add items from your ItemDatabase to loot tables using the custom editor!

## 🎯 How to Use

### **Step 1: Assign Databases**

When you open a Loot Table, assign both:
1. **Currency Database** (for currency drops)
2. **Item Database** (for item drops)

Both reference fields are at the top of the inspector under "Database References".

### **Step 2: Use the Item Quick Add Tool**

You'll see a new section:

```
┌─────────────────────────────────┐
│ Add Item Drop                   │
├─────────────────────────────────┤
│ [Refresh Item List] 15 items... │
│ Select Item: [Dropdown ▼]       │
│ Drop Chance %: [slider 0-100]   │
│ [Add Item Drop] Button          │
└─────────────────────────────────┘
```

### **Step 3: Select and Add Items**

1. **Click dropdown** to see all items from your ItemDatabase
2. **Items are categorized**:
   - `[Weapon] Steel Axe`
   - `[Armor] Leather Chest`
   - `[Jewellery] Ruby Ring`
3. **Set drop chance** (0-100%)
4. **Click "Add Item Drop"**
5. Item is added to the loot table! ✅

## 📦 What Gets Added

When you add an item, a LootEntry is created with:
- ✅ **Item name** and **type** (Weapon/Armor/Jewellery)
- ✅ **Item sprite** for UI display
- ✅ **Rarity** and **level** from the database
- ✅ **Drop chance** you specified
- ✅ **Quantity**: Always 1 (items don't stack)

## 🔄 Refreshing the Item List

If you add new items to your ItemDatabase:
1. Click **"Refresh Item List"** button
2. New items will appear in the dropdown
3. No need to close/reopen the loot table

## 💡 Example Workflow

### **Add a Rare Weapon Drop to Boss Loot:**

1. Open your boss encounter's loot table
2. Assign ItemDatabase
3. Find "Add Item Drop" section
4. Select `[Weapon] Legendary Sword` from dropdown
5. Set drop chance to `15%`
6. Click "Add Item Drop"
7. Done! Boss has 15% chance to drop Legendary Sword

### **Add Multiple Armor Pieces:**

1. Select `[Armor] Magic Helmet` → 25% → Add
2. Select `[Armor] Rare Chestplate` → 10% → Add
3. Select `[Armor] Common Boots` → 40% → Add
4. Now 3 different armor pieces can drop!

## ✅ Verification

After adding items, check the "Item Drops" list in the default inspector:
- Should show your added items
- Each with configured drop chance
- RewardType = Item
- ItemData properly assigned

## 🎮 In-Game Results

When player defeats enemies:
1. Loot system rolls for each item
2. If RNG succeeds (within drop %), item is awarded
3. Console shows: `[LootManager] Awarded item: Steel Axe`
4. (Future) Item appears in player inventory

## 📋 Item Categories

Your ItemDatabase includes:

### **Weapons**
- Main-hand weapons
- Off-hand weapons
- Two-handed weapons

### **Armor**
- Helmet
- Body Armor
- Gloves
- Boots
- Belts

### **Jewellery**
- Rings
- Amulets

All show in the dropdown with `[Category]` prefix!

## 🔧 Tips & Best Practices

### ✅ DO:
- Use lower drop % for rare/powerful items (5-15%)
- Use higher drop % for common items (30-60%)
- Add variety - multiple items per loot table
- Match item level to encounter difficulty
- Refresh list after adding items to database

### ❌ DON'T:
- Set everything to 100% (too much loot!)
- Add only one item type (variety is fun!)
- Forget to assign ItemDatabase first
- Add same item twice (just adjust the chance)

## 🎲 Drop Chance Examples

**Common Items** (40-60%)
- Basic weapons
- White/Normal quality gear
- Consumables

**Magic Items** (20-35%)
- Blue quality gear
- Enhanced weapons
- Useful accessories

**Rare Items** (10-20%)
- Yellow quality gear
- Powerful weapons
- Strong accessories

**Unique Items** (1-10%)
- Boss-specific drops
- Legendary weapons
- Build-defining gear

## 🚀 Advanced: Mix Currency + Items

Create balanced loot tables:

```
Guaranteed:
  - 2-4x Orb of Generation
  
Random Currency:
  - 25% Orb of Infusion
  - 15% Fire Spirit
  
Random Items:
  - 30% [Weapon] Steel Sword
  - 20% [Armor] Magic Helmet
  - 10% [Jewellery] Ruby Ring
```

This gives players consistent currency rewards + chance for equipment upgrades!

---

**Status**: ✅ Item Drops Fully Integrated  
**Workflow**: Super fast with dropdowns!  
**Categories**: Weapons, Armor, Jewellery













