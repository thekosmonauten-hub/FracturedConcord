# Card Multi-Sprite System

This guide explains how to use different sprites for the same card in different UI contexts.

---

## 🎨 Overview

Cards can now have **three different sprites** for different display contexts:

1. **Card Image** (Full) - High-res artwork for detailed card display
2. **Card Thumbnail** (Medium) - Optimized for card rows/lists
3. **Card Icon** (Small) - Compact icon for inventory/minimal display

**Fallback System:**
- If `cardThumbnail` is empty → uses `cardImage`
- If `cardIcon` is empty → uses `cardThumbnail` or `cardImage`

---

## 📐 Sprite Sizes & Use Cases

### **1. Card Image (Full)**
- **Recommended Size:** 512×768 px (portrait) or 1024×1536 px (high-res)
- **Used By:**
  - `DeckBuilderCardUI` (full card display)
  - Combat card display
  - Card tooltips/previews
- **Examples:**
  - Deck builder card grid
  - Combat hand cards
  - Card detail view

### **2. Card Thumbnail**
- **Recommended Size:** 256×384 px or 512×512 px
- **Used By:**
  - `DeckCardListUI` (simplified card rows)
  - CharacterScreenDeckCard (character creation)
  - Deck list panels
- **Examples:**
  - Deck builder side panel (card list)
  - Character screen starter deck preview
  - Quick card references

### **3. Card Icon**
- **Recommended Size:** 64×64 px or 128×128 px
- **Used By:**
  - Inventory systems
  - Reward screens
  - Card notifications
- **Examples:**
  - Card obtained notification
  - Inventory slots
  - Quest rewards

---

## 🛠️ How to Assign Sprites

### **Method 1: Manual Assignment**

1. **Select a CardDataExtended asset** (e.g., `Strike.asset`)
2. **In Inspector → Visual Assets:**
   ```
   Card Image: [Strike_Full.png]      ← High-res full artwork
   Card Thumbnail: [Strike_Thumb.png]  ← Medium-sized thumbnail
   Card Icon: [Strike_Icon.png]        ← Small icon
   ```
3. **Repeat for each card**

### **Method 2: Bulk Assignment Tool**

Use the **Card Sprite Assigner** tool:

1. **Organize sprites with naming convention:**
   ```
   Assets/Art/CardArt/CardSprites/
   ├── Full/
   │   ├── Strike.png
   │   ├── Bash.png
   │   └── ...
   ├── Thumbnails/
   │   ├── Strike_Thumb.png
   │   ├── Bash_Thumb.png
   │   └── ...
   └── Icons/
       ├── Strike_Icon.png
       ├── Bash_Icon.png
       └── ...
   ```

2. **Run the tool multiple times:**
   - First run: Assign full images from `Full/` folder
   - Second run: Assign thumbnails from `Thumbnails/` folder
   - Third run: Assign icons from `Icons/` folder

---

## 💡 Usage Examples

### **Example 1: Only Full Sprite (Simplest)**

```
Strike.asset:
├── Card Image: Strike.png
├── Card Thumbnail: (empty)
├── Card Icon: (empty)
```

**Result:**
- `DeckBuilderCardUI` uses `Strike.png`
- `DeckCardListUI` uses `Strike.png` (fallback)
- All contexts use the same sprite

### **Example 2: Full + Thumbnail (Recommended)**

```
Strike.asset:
├── Card Image: Strike_Full.png (1024×1536 high-res)
├── Card Thumbnail: Strike_Thumb.png (512×512 optimized)
├── Card Icon: (empty)
```

**Result:**
- `DeckBuilderCardUI` uses `Strike_Full.png`
- `DeckCardListUI` uses `Strike_Thumb.png`
- Icon contexts use `Strike_Thumb.png` (fallback)

### **Example 3: All Three (Complete)**

```
Strike.asset:
├── Card Image: Strike_Full.png (1024×1536)
├── Card Thumbnail: Strike_Thumb.png (512×512)
├── Card Icon: Strike_Icon.png (128×128)
```

**Result:**
- Full card display: `Strike_Full.png`
- Card rows/lists: `Strike_Thumb.png`
- Inventory/notifications: `Strike_Icon.png`

---

## 🔧 Component Sprite Usage

### **DeckBuilderCardUI** (Full Card Display)
```csharp
cardImage.sprite = cardData.GetCardSprite(CardSpriteContext.Full);
```
- Used in: Deck builder grid, combat hand
- Shows: Full high-res artwork

### **DeckCardListUI** (Simplified Rows)
```csharp
cardBackground.sprite = card.GetCardSprite(CardSpriteContext.Thumbnail);
```
- Used in: Deck list panel, character screen
- Shows: Optimized thumbnail

### **Future: Inventory/Icons**
```csharp
icon.sprite = card.GetCardSprite(CardSpriteContext.Icon);
```
- Used in: Inventory, rewards, notifications
- Shows: Small compact icon

---

## 📝 Best Practices

### **1. Always Assign Card Image (Required)**
- This is the base sprite
- All contexts fall back to this
- Minimum requirement for cards to display

### **2. Add Thumbnail for Performance**
- Smaller file size = better performance
- Especially important for lists with many cards
- Recommended for production builds

### **3. Add Icon for UI Polish**
- Optional but improves visual clarity
- Good for inventory systems
- Can be simple/stylized versions

---

## 🎯 Sprite Optimization Tips

### **Full Card Image:**
- Use PNG with transparency
- High resolution (1024×1536 or higher)
- Detailed artwork with effects
- Import settings: Max Size 2048, High Quality

### **Thumbnail:**
- Simplified version (remove small details)
- Medium resolution (512×512)
- Import settings: Max Size 1024, Normal Quality
- Can be square aspect ratio for better compatibility

### **Icon:**
- Very simplified/stylized
- Small resolution (128×128 or 64×64)
- Import settings: Max Size 256, Compressed
- Focus on recognizable silhouette

---

## 🔄 Migration Guide

### **If You Already Have Sprites Assigned:**

Your existing `cardImage` assignments will continue to work! The new fields are **optional**:

1. **Leave as is:** All contexts use `cardImage` (works fine!)
2. **Gradual upgrade:** Add thumbnails for commonly displayed cards first
3. **Full upgrade:** Create all three sprite types for best performance

### **If Starting Fresh:**

1. **Start with Card Image only** (required)
2. **Add Thumbnails** when you notice performance issues with large card lists
3. **Add Icons** when building inventory/notification systems

---

## ✅ Updated Components

The following components now use `GetCardSprite()`:

- ✅ `DeckBuilderCardUI` - Uses `CardSpriteContext.Full`
- ✅ `DeckCardListUI` - Uses `CardSpriteContext.Thumbnail`
- ✅ Fallback system ensures backward compatibility

---

## 🎨 Your Current Setup

For **CharacterScreenDeckCard.prefab**:
- Uses `DeckCardListUI` component
- Now requests: `CardSpriteContext.Thumbnail`
- You can assign optimized sprites to `cardThumbnail` field
- If `cardThumbnail` is empty, uses `cardImage` automatically

For **CardPrefab.prefab** (DeckBuilderCardUI):
- Uses `DeckBuilderCardUI` component
- Now requests: `CardSpriteContext.Full`
- Uses high-res `cardImage` sprite
- Best quality for detailed display

---

## 📋 Quick Assignment Workflow

### **Option A: Same Sprite Everywhere (Easy)**
1. Assign only `Card Image`
2. Leave `Card Thumbnail` and `Card Icon` empty
3. All contexts use the same sprite ✅

### **Option B: Optimized Sprites (Best)**
1. Assign `Card Image` (high-res)
2. Assign `Card Thumbnail` (medium-res, optimized for lists)
3. Leave `Card Icon` empty (uses thumbnail) ✅

### **Option C: Full Optimization (Professional)**
1. Create three versions of each card art
2. Assign all three fields
3. Best performance and visual quality ✅

---

## 🚀 Summary

**You can now:**
- ✅ Assign different sprites for different prefabs
- ✅ Use high-res for DeckBuilderCardUI (full display)
- ✅ Use optimized thumbnails for DeckCardListUI (rows)
- ✅ Maintain backward compatibility (existing sprites still work)
- ✅ Gradually upgrade sprites as needed

**Next Steps:**
1. Assign `cardThumbnail` to your CardDataExtended assets
2. Use thumbnails optimized for CharacterScreenDeckCard size
3. Use full images for CardPrefab size
4. Test in Unity to verify correct sprites appear

---

**Last Updated:** 2024-12-19
**Status:** ✅ Ready to Use


