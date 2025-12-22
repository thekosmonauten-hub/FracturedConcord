# Card Creation Workflow

## Quick Guide: Adding a New Card

### Step 1: Create the CardData Asset (1 minute)

1. In Unity Project window, navigate to `Assets/Resources/Cards/[Category]`
2. **Right-Click → Create → Dexiled → Cards → Card Data**
3. Name it (e.g., `Fireball`)

### Step 2: Configure the Card (2 minutes)

Select the new CardData asset and fill in the Inspector:

**Basic Information:**
- **Card Name**: "Fireball"
- **Card Type**: "Attack"
- **Play Cost**: 2
- **Description**: "Launch a blazing projectile"

**Card Properties:**
- **Rarity**: Common/Magic/Rare/Unique
- **Element**: Fire
- **Category**: Attack

**Visual Assets:**
- **Card Image**: Drag artwork from `Assets/Art/Cards/`
- **Element Frame**: (Optional) Custom frame sprite
- **Cost Bubble**: (Optional) Custom cost background
- **Rarity Frame**: (Optional) Custom rarity border

**Card Effects:**
- **Damage**: 12
- **Block**: 0
- **Is Discard Card**: False
- **Is Dual Wield**: False

**Special Effects:**
- **If Discarded Effect**: (Leave empty unless it's a discard card)
- **Dual Wield Effect**: (Leave empty unless it has dual wield)

### Step 3: Add to CardDatabase (30 seconds)

1. Select `Assets/Resources/CardDatabase.asset`
2. In the Inspector, scroll to **All Cards** list
3. Click **+** to add a new slot
4. Drag your new card into the slot
5. **(Auto-categorized)** The database will automatically sort it by category/element/rarity when you save

**OR** Let the database auto-populate (see Editor Tools section below)

### Step 4: Test the Card (1 minute)

1. Open the Deck Builder scene
2. Run the game
3. Your new card should appear in the collection
4. Try adding it to a deck and using it in combat

---

## Advanced: Creating Card Variants

### Creating Multiple Copies Quickly

1. **Duplicate** an existing card asset (Ctrl+D)
2. Rename it
3. Modify only the changed values
4. Add to CardDatabase

### Creating a Card Series (e.g., Fire Spell Levels 1-3)

```
Fireball_Lvl1.asset
├── Damage: 8
├── Cost: 1
└── Description: "Launch a small fireball"

Fireball_Lvl2.asset
├── Damage: 12
├── Cost: 2
└── Description: "Launch a blazing fireball"

Fireball_Lvl3.asset
├── Damage: 18
├── Cost: 3
└── Description: "Launch a massive fireball"
```

---

## Card Artwork Guidelines

### File Format & Size
- **Format**: PNG with transparency
- **Recommended Size**: 512x512 or 1024x1024
- **Aspect Ratio**: 1:1 (square) or 3:4 (portrait)

### Naming Convention
- Use card name: `Fireball.png`
- Keep consistent capitalization
- No spaces (use underscores): `Frost_Bolt.png`

### Organization
```
Assets/Art/Cards/
├── Attack/
├── Guard/
├── Skill/
└── Power/
```

---

## Validation Checklist

Before adding a card to the game, ensure:

- [ ] Card has a unique name
- [ ] Card has artwork assigned
- [ ] Damage/Block values are balanced
- [ ] Mana cost is appropriate for power level
- [ ] Description is clear and concise
- [ ] Rarity matches power level
- [ ] Element matches card theme
- [ ] Added to CardDatabase
- [ ] Tested in Deck Builder
- [ ] Tested in Combat

---

## Common Mistakes

❌ **Forgot to add to CardDatabase** → Card won't appear in Deck Builder  
❌ **Missing artwork** → Card shows as blank  
❌ **Wrong category** → Card appears in wrong filter  
❌ **Typo in name** → Breaks JSON deck imports  
❌ **Unbalanced stats** → Breaks game balance  

---

## Batch Card Creation (For Large Sets)

See `CardBatchImporter_EditorTool.md` for importing cards from spreadsheets.








