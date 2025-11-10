# Character Creation - Interactive Deck Preview System

## Overview
This guide explains how the character creation screen displays interactive deck previews using a hybrid approach: **UI Toolkit** for the list interface and **UGUI** for the full card hover previews.

## System Architecture

### Hybrid UI Approach
- **UI Toolkit (UI Document)**: Main interface, deck list, class selection
- **UGUI (GameObject/Canvas)**: Full card preview on hover (uses existing card prefabs)

### Why This Approach?
1. **Leverage Existing Assets**: Reuses `CardPrefab.prefab` (full card) and `DeckCardPrefab.prefab` (compact)
2. **Best of Both Worlds**: UI Toolkit for layout, UGUI for complex card visuals
3. **Consistent Card Display**: Same card rendering as deck builder and combat
4. **Easy Maintenance**: No need to duplicate card visual logic

---

## File Structure

### UI Toolkit Files
```
Assets/UI/
├── CharacterCreationUI.uxml   # Layout and structure
└── CharacterCreationUI.uss    # Styling (USS = Unity Style Sheets, NOT CSS!)
```

### Scripts
```
Assets/Scripts/UI/
└── CharacterCreationController.cs   # Controller logic
```

### Prefabs
```
Assets/Art/CardArt/
├── DeckCardPrefab.prefab   # Compact card for deck list (not currently used, but available)
└── CardPrefab.prefab       # Full card for hover preview
```

---

## Implementation Details

### 1. UXML Structure

```xml
<ui:ScrollView name="StarterDeckPreview" class="deck-preview-scroll">
    <ui:VisualElement name="DeckCardGrid" class="deck-card-grid" />
</ui:ScrollView>
```

**Key Points:**
- `ScrollView`: Allows scrolling if deck has many cards
- `DeckCardGrid`: Flexbox container for card rows (dynamically populated)

### 2. USS Styling

```css
.deck-preview-scroll {
    width: 750px;
    height: 220px;
    background-color: rgba(0, 0, 0, 0.4);
    border-radius: 10px;
    padding: 10px;
}

.deck-card-grid {
    flex-direction: row;
    flex-wrap: wrap;
    justify-content: flex-start;
}

.deck-preview-row {
    width: 180px;
    height: 45px;
    background-color: rgba(20, 20, 20, 0.7);
    border-radius: 6px;
    margin: 4px;
    padding: 8px 10px;
    transition-duration: 0.15s;
}

.deck-preview-row:hover {
    background-color: rgba(60, 60, 60, 0.9);
    border-color: rgba(255, 255, 255, 0.6);
    scale: 1.05;
}
```

**Important USS Notes:**
- Use USS properties, NOT CSS properties!
- `scale` instead of `transform: scale()`
- `transition-duration` instead of `transition`
- `-unity-` prefix for Unity-specific properties

### 3. Controller Logic

#### Dynamic Card Row Creation
```csharp
private void UpdateStarterDeckPreview()
{
    if (deckCardGrid == null) return;
    
    // Clear existing preview
    deckCardGrid.Clear();
    ClearDeckPreviewCards();
    
    // Get starter deck definition for selected class
    var def = starterDeckManager.GetDefinitionForClass(selectedClass);
    if (def == null || def.cards == null) return;
    
    // Create interactive card entries
    foreach (var entry in def.cards)
    {
        // Create container
        VisualElement cardRow = new VisualElement();
        cardRow.AddToClassList("deck-preview-row");
        
        // Add count label (x3, x5, etc.)
        Label countLabel = new Label($"x{entry.count}");
        countLabel.AddToClassList("deck-card-count");
        cardRow.Add(countLabel);
        
        // Add card name label
        Label nameLabel = new Label(entry.card.cardName);
        nameLabel.AddToClassList("deck-card-name");
        cardRow.Add(nameLabel);
        
        // Register hover events
        CardDataExtended cardData = entry.card;
        cardRow.RegisterCallback<MouseEnterEvent>(evt => OnCardRowHoverEnter(cardData, cardRow));
        cardRow.RegisterCallback<MouseLeaveEvent>(evt => OnCardRowHoverExit());
        
        deckCardGrid.Add(cardRow);
    }
}
```

#### Hover Preview System
```csharp
private void OnCardRowHoverEnter(CardDataExtended cardData, VisualElement cardRow)
{
    // Hide previous hover card
    if (currentHoverCard != null)
    {
        Destroy(currentHoverCard);
    }
    
    // Instantiate full card prefab
    currentHoverCard = Instantiate(fullCardPrefab, cardHoverPreviewParent);
    
    // Setup card visual
    DeckBuilderCardUI cardUI = currentHoverCard.GetComponent<DeckBuilderCardUI>();
    if (cardUI != null)
    {
        Character tempChar = new Character("Preview", selectedClass);
        cardUI.Initialize(cardData, null, tempChar);
    }
    
    // Position and scale
    RectTransform rt = currentHoverCard.GetComponent<RectTransform>();
    if (rt != null)
    {
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one * 1.2f;
    }
    
    currentHoverCard.SetActive(true);
    currentHoverCard.transform.SetAsLastSibling();
}

private void OnCardRowHoverExit()
{
    if (currentHoverCard != null)
    {
        Destroy(currentHoverCard);
        currentHoverCard = null;
    }
}
```

---

## Unity Editor Setup

### 1. Character Creation Scene Setup

#### UIDocument GameObject
1. Create GameObject: `CharacterCreationUI`
2. Add Component: `UI Document`
3. Assign:
   - **Source Asset**: `CharacterCreationUI.uxml`
   - **Panel Settings**: Create or assign existing

#### Controller GameObject
1. Create GameObject: `CharacterCreationController`
2. Add Component: `CharacterCreationController`
3. Assign:
   - **UI Document**: Reference the UIDocument component
   - **Deck Card Prefab**: `Assets/Art/CardArt/DeckCardPrefab.prefab`
   - **Full Card Prefab**: `Assets/Art/CardArt/CardPrefab.prefab`
   - **Starter Deck Manager**: Auto-finds or assign manually

#### Canvas Setup for Card Previews
1. Create GameObject: `CardPreviewCanvas`
2. Add Component: `Canvas`
3. Configure Canvas:
   - **Render Mode**: Screen Space - Overlay
   - **Sort Order**: 100 (to render on top of UI Toolkit)
4. Assign to Controller:
   - **Card Preview Canvas**: Reference this Canvas

### 2. Prefab Requirements

#### Full Card Prefab (`CardPrefab.prefab`)
Must have:
- `DeckBuilderCardUI` component
- All required UI references (cardNameText, descriptionText, etc.)
- `RectTransform` for positioning

#### Deck Card Prefab (`DeckCardPrefab.prefab`)
Must have:
- `DeckCardListUI` or similar component
- Compact layout for list view
- *(Currently not used, but available for future enhancements)*

---

## Workflow

### User Flow
1. User opens Character Creation screen
2. User selects a class (Witch, Marauder, Ranger, etc.)
3. Deck preview auto-updates with that class's starter deck
4. User hovers over a card row (e.g., "x5 Strike")
5. Full card preview appears showing:
   - Card art
   - Full description
   - Damage/guard values
   - All card details
6. User moves mouse away → card preview disappears
7. User can hover any other card in the list
8. User creates character → deck is assigned

### Technical Flow
1. `OnClassSelected(string className)` called
2. `UpdateStarterDeckPreview()` executed
3. `StarterDeckManager.GetDefinitionForClass()` retrieves deck
4. For each card in deck:
   - Create `VisualElement` row
   - Register mouse enter/exit callbacks
5. On hover enter:
   - Instantiate `fullCardPrefab` as UGUI GameObject
   - Initialize with `CardDataExtended` data
   - Display on `cardPreviewCanvas`
6. On hover exit:
   - Destroy hover card GameObject

---

## Class-Specific Deck Definitions

### Required for Each Class
Each class must have a `StarterDeckDefinition` ScriptableObject asset:

```
Assets/Resources/StarterDecks/
├── MarauderStarterDeck.asset
├── WitchStarterDeck.asset
├── RangerStarterDeck.asset
├── ThiefStarterDeck.asset
├── ApostleStarterDeck.asset
└── BrawlerStarterDeck.asset
```

### Creating a Starter Deck Definition

1. Right-click in Project window
2. **Create → Dexiled → Cards → Starter Deck Definition**
3. Name it: `[ClassName]StarterDeck`
4. Configure:
   ```
   Character Class: "Marauder"  (must match exactly!)
   Cards:
     - Card: Strike (CardDataExtended asset)
       Count: 5
     - Card: Defend (CardDataExtended asset)
       Count: 4
     - Card: Bash (CardDataExtended asset)
       Count: 1
   ```
5. Save to: `Assets/Resources/StarterDecks/`

### Class Names (Must Match Exactly)
- `"Witch"`
- `"Marauder"`
- `"Ranger"`
- `"Thief"`
- `"Apostle"`
- `"Brawler"`

---

## Troubleshooting

### Issue: Deck Preview Not Showing

**Possible Causes:**
1. No `StarterDeckDefinition` found for selected class
2. Definition not in `Resources/StarterDecks/` folder
3. `characterClass` field doesn't match exactly

**Solution:**
- Check console for: `"No starter deck definition found for class: [ClassName]"`
- Verify definition exists and `characterClass` matches exactly (case-sensitive!)
- Ensure definition is in `Resources/StarterDecks/` or assigned to `StarterDeckManager`

### Issue: Card Hover Preview Not Showing

**Possible Causes:**
1. `fullCardPrefab` not assigned
2. `cardPreviewCanvas` not assigned or inactive
3. `cardHoverPreviewParent` not created

**Solution:**
- Check Inspector: Ensure prefabs are assigned
- Verify Canvas exists with Sort Order 100+
- Check console for warnings about missing components

### Issue: Card Preview Shows Wrong Data

**Possible Causes:**
1. `DeckBuilderCardUI` not on prefab
2. Card data not properly passed to Initialize()

**Solution:**
- Verify `CardPrefab.prefab` has `DeckBuilderCardUI` component
- Check that `Initialize()` is being called with valid `CardDataExtended`

### Issue: Cards Overlapping or Wrong Position

**Possible Causes:**
1. `cardHoverPreviewParent` positioned incorrectly
2. Multiple hover cards not cleaned up

**Solution:**
- Adjust `cardHoverPreviewParent` RectTransform position in `SetupUI()`
- Verify `OnCardRowHoverExit()` is properly destroying old cards

---

## Styling Tips

### Adjusting Deck Preview Size
In `CharacterCreationUI.uss`:
```css
.deck-preview-scroll {
    width: 750px;    /* Total width */
    height: 220px;   /* Total height (adjust for more/fewer visible rows) */
}
```

### Changing Card Row Size
```css
.deck-preview-row {
    width: 180px;    /* Row width */
    height: 45px;    /* Row height */
    margin: 4px;     /* Spacing between rows */
}
```

### Hover Effect Customization
```css
.deck-preview-row:hover {
    background-color: rgba(60, 60, 60, 0.9);  /* Hover background */
    scale: 1.05;                               /* Hover scale */
    transition-duration: 0.1s;                 /* Animation speed */
}
```

### Card Preview Position
In `CharacterCreationController.cs` → `SetupUI()`:
```csharp
rt.anchoredPosition = new Vector2(300f, 0f);  // X = right/left, Y = up/down
rt.sizeDelta = new Vector2(160, 220);          // Card size
```

---

## Performance Considerations

### Efficient Card Management
- **Object Pooling**: Consider pooling hover cards if users hover frequently
- **Lazy Instantiation**: Cards only created on hover, not preloaded
- **Automatic Cleanup**: `OnDestroy()` ensures no memory leaks

### UI Toolkit vs UGUI
- **UI Toolkit**: Lightweight, great for lists and layout
- **UGUI**: Better for complex visuals like cards with images/effects
- **Hybrid**: Combines strengths of both systems

---

## Extension Ideas

### Future Enhancements
1. **Animated Transitions**: Fade in/out for hover card
2. **Sound Effects**: Play sound on hover
3. **Card Statistics**: Show total deck stats (avg cost, damage, etc.)
4. **Deck Comparison**: Compare multiple class decks side-by-side
5. **Card Filtering**: Filter by type (Attack, Guard, Skill)
6. **Rarity Indicators**: Color-code cards by rarity

### Example: Adding Fade Animation
```csharp
private void OnCardRowHoverEnter(CardDataExtended cardData, VisualElement cardRow)
{
    // ... existing code ...
    
    // Add fade-in animation
    CanvasGroup canvasGroup = currentHoverCard.GetComponent<CanvasGroup>();
    if (canvasGroup == null)
    {
        canvasGroup = currentHoverCard.AddComponent<CanvasGroup>();
    }
    
    canvasGroup.alpha = 0f;
    LeanTween.alphaCanvas(canvasGroup, 1f, 0.2f).setEaseOutQuad();
}
```

---

## Testing Checklist

- [x] All 6 classes have deck preview data
- [x] Deck preview updates when class is selected
- [x] Cards display with correct count (x5, x4, etc.)
- [x] Hovering a card shows full preview
- [x] Leaving a card hides full preview
- [x] Multiple hover/unhover cycles work correctly
- [x] No memory leaks (cards cleaned up on destroy)
- [x] Preview canvas renders on top of UI Toolkit
- [x] Card data displays correctly (name, description, stats)

---

## Common Mistakes to Avoid

### ❌ DON'T: Use CSS Properties in USS
```css
/* WRONG */
.deck-preview-row {
    transform: scale(1.05);    /* CSS - Won't work! */
}

/* CORRECT */
.deck-preview-row {
    scale: 1.05;               /* USS - Works! */
}
```

### ❌ DON'T: Forget to Clean Up GameObjects
```csharp
// WRONG - Memory leak!
private void OnCardRowHoverExit()
{
    currentHoverCard.SetActive(false);  // Still exists in memory!
}

// CORRECT
private void OnCardRowHoverExit()
{
    if (currentHoverCard != null)
    {
        Destroy(currentHoverCard);      // Properly destroyed
        currentHoverCard = null;
    }
}
```

### ❌ DON'T: Use Wrong Canvas Sort Order
```
// WRONG - UI Toolkit covers UGUI
Canvas Sort Order: 0

// CORRECT - UGUI covers UI Toolkit
Canvas Sort Order: 100
```

---

## Debug Logging

### Useful Debug Messages
```
<color=cyan>Building deck preview for Marauder with 3 unique cards</color>
  Added: x5 Strike
  Added: x4 Defend
  Added: x1 Bash
<color=green>✓ Deck preview created with 3 card types</color>

<color=yellow>Showing hover preview for: Strike</color>
<color=yellow>Hidden hover preview</color>
```

### Debug Checklist
If cards aren't showing:
1. Check console for "No starter deck definition found for class: [X]"
2. Verify `DeckCardGrid` element exists in UXML
3. Ensure `StarterDeckDefinition` assets are in `Resources/StarterDecks/`
4. Check that `characterClass` field matches exactly

---

## Integration with Existing Systems

### StarterDeckManager
- **Location**: `Assets/Scripts/Cards/StarterDeckManager.cs`
- **Method**: `GetDefinitionForClass(string className)`
- **Returns**: `StarterDeckDefinition` with list of cards and counts

### StarterDeckDefinition
- **Type**: ScriptableObject
- **Location**: `Assets/Resources/StarterDecks/`
- **Contains**: `List<CardEntry>` where each entry has `card` and `count`

### CardDataExtended
- **Type**: ScriptableObject
- **Location**: Various (referenced by StarterDeckDefinition)
- **Contains**: All card data (name, description, damage, etc.)

### DeckBuilderCardUI
- **Location**: `Assets/Scripts/UI/Cards/DeckBuilderCardUI.cs`
- **Method**: `Initialize(CardDataExtended cardData, DeckData deck, Character character)`
- **Purpose**: Renders card visual from data

---

## Related Documentation

- **UI Toolkit Guide**: Unity Manual → UI Toolkit
- **USS Reference**: [Unity USS Properties](https://docs.unity3d.com/Manual/UIE-USS-Properties-Reference.html)
- **Card System**: `Assets/Scripts/Documentation/CardSystem.md`
- **Deck Builder**: `Assets/Scripts/Documentation/DeckBuilder_Guide.md`

---

## Version History

### v1.0 - Initial Implementation
- Created hybrid UI Toolkit + UGUI system
- Implemented deck preview with hover cards
- Added support for all 6 classes
- Integrated with existing card prefabs and systems












