# Warrant Prefab Setup Checklist

Follow these steps the next time we resume prefab work for the Warrant Tree. Each step builds on the previous one; stop after any step if you need sign-off.

1. **Audit Existing Node View**
   - Review `WarrantNodeView` to confirm it covers shared needs (RectTransform, Image, Button).
   - Decide whether to extend it (e.g., `WarrantSocketView`) for drag/drop specifics instead of duplicating logic.

2. **Define Prefab Set**
   - Plan for at least four prefabs: `Socket`, `SpecialSocket`, `Effect`, `Anchor`.
   - Only sockets need interactable layers; effect/anchor prefabs can remain static visuals.

3. **Author Socket Visual Prefab**
   - Create `Assets/Prefabs/Warrants/WarrantNode_Socket.prefab`.
   - Base hierarchy:
     ```
     WarrantNode_Socket (RectTransform, WarrantNodeView, Button, Image)
       ├── Highlight (Image, disabled by default)
       └── IconHolder (RectTransform, optional Image)
     ```
   - Tune colors/sizes to 28px nodes; duplicate/tweak for effect/special/anchor versions.

4. **Interaction Script**
   - Create `WarrantSocketView : WarrantNodeView` implementing the Unity drag/drop interfaces (`IPointerEnterHandler`, `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`, `IDropHandler`).
   - Responsibilities:
     - Highlight on hover/selection.
     - Request assignments from `WarrantBoardStateController` on drop.
     - Spawn/cleanup drag ghosts (defer to locker integration later if needed).

5. **Update Graph Builder**
   - Assign the new prefabs in `WarrantBoardGraphBuilder` inspector fields (`socketPrefab`, `effectPrefab`).
   - Rebuild the graph to ensure every node uses the prefab visuals without manual placement.

6. **State Controller Hook-Up**
   - Give each `WarrantSocketView` a reference to `WarrantBoardStateController`.
   - On `Initialize`, read current assignments via `GetWarrantAtNode(nodeId)` to update icon/tooltip.
   - When dropping/clearing, call `TryAssignWarrant` / `TryRemoveWarrant` and refresh the visuals.

7. **Test Loop**
   - In `WarrantTree.scene`, regenerate the graph and ensure:
     - Sockets highlight, read state, and swap visuals per page.
     - Saving/loading via the controller keeps assignments intact.
   - Log edge cases (invalid IDs, missing prefabs) for future polish.

Keep this document updated as workflows evolve (e.g., once Peacekeeper fusion or locker drag/drop is implemented).



## Implementation Notes — 2025-11-15

- `WarrantNode_Socket.prefab` now uses `WarrantSocketView` plus a `CanvasGroup`, highlight, and icon holder. Assign it to `WarrantBoardGraphBuilder.socketPrefab` so sockets gain drag/drop automatically.
- `WarrantSocketView` implements Unity's pointer + drag interfaces, highlights on hover, spawns a drag ghost (when an icon exists), and syncs assignments through `WarrantBoardStateController` (Option 1 flow).
- The graph builder exposes `boardStateController` and `iconLibrary` references so freshly spawned sockets call `GetWarrantAtNode` immediately. Wire both fields after dropping the builder into a scene.
- `WarrantIconLibrary` (Project → Create → Dexiled → Warrants → Icon Library) maps warrant IDs to sprites. Populate it with `WarrantDefinition` assets to surface icons inside sockets/drag ghosts; sockets fall back to the empty icon when lookups miss.
- External sources (locker, rewards, etc.) can integrate by implementing `IWarrantDragPayload`. Dropping payloads onto sockets swaps assignments and keeps `WarrantBoardStateController` authoritative.
- Log notable prefab/script tweaks in `DevelopmentLog.md` as you iterate so future passes know which responsibilities already landed in `WarrantSocketView`.

## Locker Inventory Implementation — 2025-11-15

### Components Created

1. **WarrantLockerPanelManager**
   - Manages sliding panel animation (slides in from off-screen)
   - Uses LeanTween for smooth transitions
   - Supports left/right/top/bottom slide directions
   - Toggle button integration

2. **WarrantLockerItem**
   - Individual warrant item in the grid
   - Implements `IWarrantDragPayload` for drag/drop
   - Shows warrant icon and rarity-colored background
   - Spawns drag ghost during drag operations

3. **WarrantLockerGrid**
   - Manages grid layout using Unity's `GridLayoutGroup`
   - Dynamically generates warrant items from available warrants list
   - Supports filtering via `ApplyFilter()` method

4. **WarrantLockerFilterController**
   - Filters warrants by rarity (toggle group)
   - Filters warrants by modifier (TMP_Dropdown)
   - Auto-populates modifier dropdown from available warrants
   - Clear filters button

5. **WarrantLockerDropZone**
   - Drop zone component for the locker panel
   - Handles dropping warrants back from sockets (free swapping)
   - Clears socket assignments when warrant is returned

### Prefab Setup Guide

**Locker Panel Hierarchy:**
```
WarrantLockerPanel (RectTransform, WarrantLockerPanelManager, CanvasGroup)
├── Header (RectTransform)
│   ├── Title (Text/TMP_Text)
│   └── ToggleButton (Button)
├── FilterSection (RectTransform, WarrantLockerFilterController)
│   ├── RarityToggleGroup (ToggleGroup)
│   │   ├── CommonToggle (Toggle)
│   │   ├── MagicToggle (Toggle)
│   │   ├── RareToggle (Toggle)
│   │   └── UniqueToggle (Toggle)
│   ├── ModifierDropdown (TMP_Dropdown)
│   └── ClearFiltersButton (Button)
├── ScrollView (ScrollRect)
│   └── Content (RectTransform, WarrantLockerGrid, GridLayoutGroup)
│       └── [WarrantLockerItem instances spawned here]
└── DropZone (RectTransform, Image, WarrantLockerDropZone)
```

**WarrantLockerItem Prefab:**
```
WarrantLockerItem (RectTransform, WarrantLockerItem, CanvasGroup, Image)
├── Background (Image) - Rarity-colored background
└── Icon (Image) - Warrant icon sprite
```

### Integration Steps

1. **Create Locker Panel Prefab**
   - Set up hierarchy as shown above
   - Configure `WarrantLockerPanelManager` with slide direction and duration
   - Assign toggle button reference

2. **Create WarrantLockerItem Prefab**
   - Create prefab with `WarrantLockerItem` component
   - Assign background and icon Image references
   - Set rarity colors in inspector

3. **Wire Filter Controller**
   - Assign `WarrantLockerGrid` reference
   - Create rarity toggles (Common, Magic, Rare, Unique)
   - Create modifier dropdown (TMP_Dropdown)
   - Assign clear filters button

4. **Populate Warrant Inventory**
   - Create a list/scriptable object of available `WarrantDefinition` assets
   - Call `WarrantLockerGrid.SetAvailableWarrants()` or `WarrantLockerFilterController.SetAvailableWarrants()`
   - Grid will auto-populate and filter controller will build modifier dropdown

5. **Free Swapping Flow**
   - Drag from locker item → drop on socket: assigns warrant to socket
   - Drag from socket → drop on locker drop zone: clears socket, warrant returns to locker
   - No consumption - warrants are freely swappable

### Notes

- The locker uses the same `IWarrantDragPayload` interface as sockets for consistency
- Filtering is done client-side on the available warrants list
- The drop zone should cover the entire locker panel area for easy dropping
- Panel slides in from off-screen (default: left side) when toggled

### Warrant Database (New)

1. **Create the Database Asset**
   - Right-click in the Project window → `Create → Dexiled → Warrants → Database`.
   - Name it something like `WarrantDatabase.asset` and populate the list with every `WarrantDefinition` you intend to ship (rarity sorting optional).
2. **Locker Wiring**
   - Assign the database asset to `WarrantLockerGrid.warrantDatabase`.
   - Enable `autoPopulateFromDatabase` to have the locker clone every definition into the runtime inventory on `Start`. Disable if you need to feed a filtered list via script.
3. **Filters & Tooltips**
   - `WarrantLockerFilterController` now queries the locker at runtime, so the modifier dropdown always reflects the database.
   - `WarrantIconLibrary` can keep using the same definitions (database guarantees every warrant is referenced in one place).
4. **Maintaining the Database**
   - Any new warrant only needs to be added to the database; the locker grid, filters, and tooltip lookup all rehydrate from that source automatically.

## Tooltip Integration — 2025-11-15

To keep momentum with Option 1 (CombatSceneManager) we now surface warrant data through the shared `ItemTooltipManager`.

### New Runtime Pieces

1. **`WarrantTooltipView` & `WarrantTooltipData/Utility`**
   - Rendering layer that builds a dynamic list (1–8+ lines) for any warrant.
   - Automatically fabricates a minimal layout if you haven’t authored a dedicated prefab yet.
2. **`ItemTooltipManager`**
   - Exposes `warrantTooltipPrefab` plus `ShowWarrantTooltip(...)` overloads.
   - Falls back to a runtime-generated tooltip if you leave the prefab field empty.
3. **`WarrantEffectView`**
   - Replaces the base `WarrantNodeView` on `WarrantNode_Effect.prefab`.
   - Uses runtime graph connections to determine which sockets (and modifiers) currently affect each effect node.
4. **Socket Enhancements**
   - `WarrantSocketView` now receives `WarrantLockerGrid` + `ItemTooltipManager` references from `WarrantBoardGraphBuilder`.
   - Hovering a filled socket shows its definition/affixes; right-click clearing or drag-drop hides the tooltip automatically.

### Setup Checklist

1. **Assign Tooltip Manager**
   - In `WarrantBoardGraphBuilder`, wire the new `tooltipManager` field to the scene’s `ItemTooltipManager`.
   - Optional: drop a bespoke `warrantTooltipPrefab` onto the manager; otherwise it will render a default look.
2. **Populate Definition Catalog**
   - `WarrantLockerGrid` now keeps a definition lookup so sockets/effect nodes can resolve tooltip data even after an item leaves the locker.
   - Add every `WarrantDefinition` you plan to use to `catalog` (serialized list) so runtime lookups never miss.
3. **Prefab Updates**
   - Ensure `WarrantNode_Effect.prefab` references `WarrantEffectView`.
   - Confirm `WarrantNode_Socket.prefab` uses the latest `WarrantSocketView` (no inspector references needed—the builder injects scene objects).
4. **Hover Behavior**
   - Sockets: show the assigned warrant’s modifiers; empty sockets hide the tooltip.
   - Effect nodes: aggregate nearby socket assignments and list their modifiers (labels + bullet lines) so QA can verify propagation visually.

Document any stylistic adjustments (fonts, colors, prefab swaps) here so future sessions know how the tooltip skinning evolved.
