# Inscription Seal Display & Add Embossing Slot - Setup Guide

## ✅ What's Been Implemented

### Components Created:

1. **InscriptionSealDisplay** - Displays Inscription Seal currency quantity
2. **EmbossingSlotManager** - Handles adding embossing slots to cards
3. **CardCarouselUI Extensions** - Added accessor methods for card selection

---

## 🎯 Hierarchy Setup

Your existing hierarchy should be:
```
EmbossingNavDisplay
└── EmbossingBackground
    └── Footer
        └── AddEmbossingSlot (Button)
            └── CurrencyQuantity (TextMeshProUGUI)
```

---

## 🔧 Component Setup Instructions

### Step 1: Add InscriptionSealDisplay Component

**On GameObject:** `EmbossingNavDisplay/EmbossingBackground/Footer/AddEmbossingSlot/CurrencyQuantity`

1. Select the `CurrencyQuantity` GameObject
2. Add Component: `InscriptionSealDisplay`
3. **Settings:**
   - ✅ Auto Find Text: `true` (will find TextMeshProUGUI automatically)
   - Format: `"{0}"` (shows just the number)
   - Normal Color: White
   - Insufficient Color: Red

**What it does:**
- Automatically finds and updates the TextMeshProUGUI
- Displays current Inscription Seal count from LootManager
- Changes color based on affordability

---

### Step 2: Add EmbossingSlotManager Component

**On GameObject:** `EmbossingNavDisplay/EmbossingBackground/Footer/AddEmbossingSlot` (or parent)

1. Create empty GameObject: `EmbossingSlotManager` (or add to existing)
2. Add Component: `EmbossingSlotManager`
3. **Assign References:**
   - **Add Slot Button**: Drag `AddEmbossingSlot` Button
   - **Currency Display**: Drag `CurrencyQuantity` (with InscriptionSealDisplay)
   - **Card Carousel**: Auto-finds CardCarouselUI in scene
   
4. **Slot Costs** (default):
   - Slot 1: `5` Inscription Seals
   - Slot 2: `10` Inscription Seals
   - Slot 3: `20` Inscription Seals
   - Slot 4: `40` Inscription Seals
   - Slot 5: `80` Inscription Seals

5. **Optional UI References:**
   - **Cost Text**: TextMeshProUGUI showing cost
   - **Slot Info Text**: TextMeshProUGUI showing "2/5 (+1 for 20)"

**What it does:**
- Monitors selected card from carousel
- Calculates cost for next embossing slot
- Validates player has enough Inscription Seals
- Deducts currency and adds slot to card
- Updates all cards with same groupKey
- Saves to active deck

---

## 🎨 UI Layout Suggestion

### Minimal Setup (Current):
```
AddEmbossingSlot (Button)
└── CurrencyQuantity (Text) ← Shows "10" or "5" (red if can't afford)
```

### Enhanced Setup (Recommended):
```
AddEmbossingSlot (Button)
├── Icon (Image) ← Inscription Seal icon
├── CurrencyQuantity (Text) ← "10"
├── CostDisplay (Text) ← "Cost: 5"
└── SlotInfo (Text) ← "Slots: 2/5"
```

---

## 🧪 Testing

### Test Currency Display:

1. **Start Play Mode**
2. **Navigate to EquipmentScreen**
3. **Check Console:**
```
[InscriptionSealDisplay] Auto-assigned TextMeshProUGUI: CurrencyQuantity
[InscriptionSealDisplay] LootManager not found, returning 0
```

4. **Add Test Currency** (in code or debug):
```csharp
// Give player 50 Inscription Seals for testing
LootManager.Instance.AddCurrency(CurrencyType.InscriptionSeal, 50);
```

5. **Currency should update automatically**

---

### Test Add Slot Functionality:

1. **Give player currency:**
```csharp
LootManager.Instance.AddCurrency(CurrencyType.InscriptionSeal, 100);
```

2. **Select a card in carousel**
3. **Check button state:**
   - If card has < 5 slots and player has enough currency: Button enabled
   - If card has 5 slots or insufficient currency: Button disabled

4. **Click "Add Embossing Slot" button**
5. **Verify:**
   - Currency deducted (e.g., 100 → 95)
   - Card slot count increased (e.g., 1 → 2)
   - Visual slots update on card prefab
   - All copies of card in deck updated

6. **Check Console:**
```
[EmbossingSlotManager] Added embossing slot to 'Heavy Strike'. Now has 2 slots
[EmbossingSlotManager] Updated Heavy Strike cards in active deck
[CardCarousel] Refreshed card display for: Heavy Strike
```

---

## 🔢 Slot Cost Progression

| Slot # | Cost | Total Spent |
|--------|------|-------------|
| 1 → 2  | 5    | 5           |
| 2 → 3  | 10   | 15          |
| 3 → 4  | 20   | 35          |
| 4 → 5  | 40   | 75          |
| 5 (Max)| 80   | 155         |

**Total cost to max out a card:** 155 Inscription Seals

---

## 🎮 Player Flow

**Step-by-Step:**

1. **Player enters EquipmentScreen**
2. **Sees Inscription Seal count** (e.g., "25")
3. **Selects card from carousel** (e.g., Heavy Strike)
4. **Sees Add Embossing Slot button:**
   - If afford: Green/enabled with cost "5"
   - If can't afford: Red/disabled with cost "5"
5. **Clicks button** (if enabled)
6. **Currency deducted:** 25 → 20
7. **Card updated:** Heavy Strike now has 2 slots
8. **Visual feedback:** Embossing slot indicator shows 2 slots
9. **All copies updated:** All 6 Heavy Strike cards in deck now have 2 slots

---

## 🛠️ Advanced Customization

### Change Slot Costs:

**In EmbossingSlotManager Inspector:**
```
Slot Costs:
Size: 5
Element 0: 5    ← Cost for 1st slot
Element 1: 10   ← Cost for 2nd slot
Element 2: 20   ← Cost for 3rd slot
Element 3: 40   ← Cost for 4th slot
Element 4: 80   ← Cost for 5th slot
```

**Example - Cheaper Progression:**
```
5, 5, 10, 10, 15 = Total 45 to max
```

**Example - Linear Progression:**
```
10, 20, 30, 40, 50 = Total 150 to max
```

**Example - Exponential:**
```
10, 20, 40, 80, 160 = Total 310 to max
```

### Add Visual Feedback:

**In EmbossingSlotManager:**

1. **Cost Text:**
   - Shows cost for next slot
   - Changes color if unaffordable

2. **Slot Info Text:**
   - Shows "2/5 (+1 for 10)"
   - Updates dynamically

**To wire up:**
```csharp
[SerializeField] private TextMeshProUGUI costText;
[SerializeField] private TextMeshProUGUI slotInfoText;
```

Then assign in Inspector.

---

## 🐛 Troubleshooting

### "LootManager not found"
- **Solution:** Ensure LootManager GameObject exists in scene
- LootManager should be persistent (DontDestroyOnLoad)
- Check singleton initialization

### Currency not displaying
- **Solution:** Check InscriptionSealDisplay component is on CurrencyQuantity
- Verify TextMeshProUGUI exists and is assigned
- Check LootManager has Inscription Seals in currency dictionary

### Button not working
- **Solution:** Ensure EmbossingSlotManager is attached
- Verify Button reference is assigned
- Check OnClick listener is added in Start()

### Slots not updating visually
- **Solution:** Card prefab must have embossing slot GameObjects
- See `EMBOSSING_SLOTS_SYSTEM.md` for prefab setup
- CardDisplay.DisplayEmbossingSlots() must be working

### All cards not updating
- **Solution:** Cards are updated by `groupKey`
- Verify cards have matching groupKey values
- Check DeckManager.SaveDeck() is called

---

## 📋 Checklist

Setup:
- [ ] Add InscriptionSealDisplay to CurrencyQuantity
- [ ] Add EmbossingSlotManager to scene
- [ ] Assign Button reference
- [ ] Assign Currency Display reference
- [ ] (Optional) Assign Cost/Info text references

Testing:
- [ ] Give player test currency (50-100 Seals)
- [ ] Select card in carousel
- [ ] Button enables/disables correctly
- [ ] Click button adds slot
- [ ] Currency deducted
- [ ] Card slot count increases
- [ ] Visual slots update on card
- [ ] Console shows success messages

Polish:
- [ ] Add Inscription Seal icon to button
- [ ] Add cost display text
- [ ] Add slot info text (X/5)
- [ ] Add button hover/click animations
- [ ] Add success feedback (toast/popup)

---

## ✅ Result

**Working Flow:**

```
Player: [Has 50 Inscription Seals]
        ↓
Selects: Heavy Strike (1 slot)
        ↓
Button: "Add Slot - Cost: 5"  [ENABLED]
        ↓
Clicks Button
        ↓
Result:
- Currency: 50 → 45 Seals
- Card Slots: 1 → 2 slots
- Deck Updated: All Heavy Strike cards now have 2 slots
- UI Refreshed: Visual shows 2 embossing slots
```

**System is fully functional and ready to use!** 🎉

---

## 🔗 Related Documentation

- `EMBOSSING_SLOTS_SYSTEM.md` - Visual slot setup on prefabs
- `EMBOSSING_SYSTEM.md` - Full embossing mechanics
- `EMBOSSING_SETUP_GUIDE.md` - Core system setup

**Next:** Apply embossings to cards using the new slots!

