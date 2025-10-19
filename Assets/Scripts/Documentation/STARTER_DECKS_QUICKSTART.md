# Starter Decks - Quick Start ⚡

## 🎴 "How do I use my JSON decks in combat?"

**3-step answer:**

---

## Step 1: Add to Your Prefab (2 minutes)

**Open**: `Assets/Art/CardArt/CardPrefab.prefab`

**Add 2 Components**:
1. `CombatCardAdapter`
2. `CardHoverEffect`

**Save** prefab (Ctrl+S)

---

## Step 2: Add to Scene (3 minutes)

**Create GameObject**: "CombatDeckManager"

**Add Component**: `CombatDeckManager`

**That's it!** Auto-finds everything else!

---

## Step 3: Test! (30 seconds)

**Right-click CombatDeckManager** → Pick a deck:

- "Load Marauder Deck"
- "Load Witch Deck"
- "Load Ranger Deck"
- "Load Brawler Deck"
- "Load Thief Deck"
- "Load Apostle Deck"

**Then**: "Draw Initial Hand"

**See 5 cards appear!** 🎉

---

## 💻 In Code

```csharp
// Auto-loads deck for current character
CombatDeckManager.Instance.LoadDeckForCurrentCharacter();
CombatDeckManager.Instance.DrawInitialHand();

// Cards appear automatically!
```

---

## ✅ All 6 Decks Ready

Your JSON decks:
- ✅ Apostle (Chaos/Discard) - 12 cards
- ✅ Brawler (Momentum) - 15 cards
- ✅ Marauder (Physical/STR) - 18 cards
- ✅ Ranger (Bow/Evasion) - 13 cards
- ✅ Thief (Preparation) - 12 cards
- ✅ Witch (Spells/Combos) - 16 cards

**All load automatically based on character class!**

---

## 🎮 What You Get

When you load a deck:
- ✅ Cards loaded from JSON
- ✅ Visuals created from your CardPrefab
- ✅ Auto-shuffled (optional)
- ✅ Auto-drawn to hand
- ✅ Animations work (draw, play, hover)
- ✅ Click to play
- ✅ Object pooling (performance)

---

## 📦 Files Created

```
Assets/Scripts/Cards/
└── DeckLoader.cs (loads JSON → Card objects)

Assets/Scripts/CombatSystem/
└── CombatDeckManager.cs (manages combat deck)

Assets/Scripts/UI/Combat/
└── CombatCardAdapter.cs (adapts to your prefab)

Assets/Scripts/Documentation/
├── STARTER_DECKS_INTEGRATION.md (complete guide)
└── STARTER_DECKS_QUICKSTART.md (this file)
```

---

*Setup Time: 5 minutes*
*Works with all 6 starter decks!*

