# Quick Setup Guide: Character Creation Deck Preview

## Prerequisites
✅ All code changes are already implemented
✅ UXML and USS files are updated
✅ Controller script is ready

## Unity Editor Setup (5 Minutes)

### Step 1: Assign Card Prefabs to Controller
1. Open the Character Creation scene
2. Select the `CharacterCreationController` GameObject
3. In Inspector, assign:
   - **Full Card Prefab**: Drag `Assets/Art/CardArt/CardPrefab.prefab`
   - **Deck Card Prefab**: Drag `Assets/Art/CardArt/DeckCardPrefab.prefab` (optional)

### Step 2: Create Card Preview Canvas
1. Right-click in Hierarchy → **UI → Canvas**
2. Rename to: `CardPreviewCanvas`
3. Configure Canvas component:
   - **Render Mode**: `Screen Space - Overlay`
   - **Sort Order**: `100` (renders on top of UI Toolkit)
4. Select `CharacterCreationController` GameObject
5. Drag `CardPreviewCanvas` to the **Card Preview Canvas** field

### Step 3: Verify Starter Deck Definitions
Check that these exist in `Assets/Resources/StarterDecks/`:
- [ ] `MarauderStarterDeck.asset`
- [ ] `WitchStarterDeck.asset`
- [ ] `RangerStarterDeck.asset`
- [ ] `ThiefStarterDeck.asset`
- [ ] `ApostleStarterDeck.asset`
- [ ] `BrawlerStarterDeck.asset`

If any are missing, create them:
1. Right-click → **Create → Dexiled → Cards → Starter Deck Definition**
2. Set `Character Class` field to match class name exactly
3. Add cards with counts (e.g., x5 Strike, x4 Defend)
4. Save to `Assets/Resources/StarterDecks/`

### Step 4: Test
1. Enter Play Mode
2. Select each class
3. Verify deck preview appears
4. Hover over card rows
5. Verify full card preview appears
6. Move mouse away
7. Verify card preview disappears

## Expected Result

### Deck Preview
- Grid of card rows
- Each row shows: `x5 Strike` format
- Hovering highlights row
- Smooth transitions

### Card Hover Preview
- Full card appears on hover
- Shows complete card details
- Positioned to the right
- Scales to 1.2x for visibility
- Disappears smoothly on mouse leave

## Troubleshooting

### No Deck Preview?
1. Console shows: `"No starter deck definition found for class: [X]"`
2. **Fix**: Create missing StarterDeckDefinition asset
3. **Verify**: Class name matches exactly (case-sensitive)

### No Hover Card?
1. Console shows: `"Cannot show card preview: Missing prefab, parent, or card data"`
2. **Fix**: Assign `fullCardPrefab` in Inspector
3. **Fix**: Verify `CardPreviewCanvas` exists and is assigned
4. **Check**: Prefab has `DeckBuilderCardUI` component

### Card Shows at Wrong Position?
1. **Fix**: Adjust in `CharacterCreationController.cs` → `SetupUI()`:
   ```csharp
   rt.anchoredPosition = new Vector2(300f, 0f);  // Change X/Y values
   ```

### Hover Card Doesn't Disappear?
1. **Check**: Mouse leave event is registered
2. **Verify**: `OnCardRowHoverExit()` is being called
3. **Debug**: Add breakpoint in `OnCardRowHoverExit()`

## Complete!

Once you've completed these steps, your character creation screen will have a fully interactive deck preview system with hoverable cards!

---

**Next Steps:**
- Test with all 6 classes
- Adjust positioning/scaling as needed
- Consider adding fade animations
- Add sound effects on hover (optional)












