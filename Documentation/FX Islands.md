# FX Islands (UI + SpriteRenderer/ParticleSystem Hybrid)

FX Islands let you attach world-space SpriteRenderer/ParticleSystem effects to UI elements without converting the UI system. UI stays authoritative; FX just follows.

## Why This Exists

Unity UI (`Canvas`, `Image`, `RectTransform`) and world rendering (`SpriteRenderer`, `ParticleSystem`, shaders) do not share masking, layout, or scaling. FX Islands bridge the two with a one-way sync.

## Core Principle

- UI drives FX: `RectTransform` -> world-space effect position
- FX never drives UI: no layout or interaction changes

## Components Added

- `UIFxAnchor` (optional): attach to a UI element to provide an explicit anchor + optional offset.
- `UIFxFollower`: add to a world-space FX prefab to follow a UI `RectTransform`.

Paths:
- `Assets/Scripts/UI/FX/UIFxAnchor.cs`
- `Assets/Scripts/UI/FX/UIFxFollower.cs`

## Quick Setup (Recommended)

1) **Create a world-space FX root**

- Create an empty object in the scene (example name: `UI_FX_Root`).
- This is where world FX will live. It can be anywhere in the scene hierarchy.

2) **Add a UI anchor**

- In your card prefab (or any UI element), add a child `RectTransform`.
- Add `UIFxAnchor` to that child.
- Adjust `worldOffset` if you want the effect above or below the card center.

3) **Add the follower to your FX prefab**

- Add `UIFxFollower` to the burn VFX prefab (or any world FX prefab).
- Do not place this prefab under the Canvas. It should live under `UI_FX_Root`.

4) **Wire it in CombatDeckManager**

In the inspector (for the `CombatDeckManager`):
- `uiFxRoot` -> your `UI_FX_Root` transform
- `uiFxWorldCamera` -> optional camera to use for world-space placement
- `burnFxWorldDistance` -> depth value for world placement

When a card is burned, the burn prefab is instantiated and the follower locks to the card’s anchor.

## How the Position Sync Works

`UIFxFollower` does this each frame:

1) Convert UI world position -> screen point
2) Convert screen point -> world space using `worldCamera`
3) Apply optional offset

This keeps a world FX object visually attached to UI without UI layout rebuilds.

## Burn FX Hook Points (Combat)

In `CombatDeckManager`:

- **SFX**: `burnSfx`
- **VFX**: `burnVfxPrefab`
- **Background reveal**: `burnBackgroundSprite`, `burnBackgroundRevealSeconds`, `burnHoldSeconds`

The burn VFX is spawned at the card position and (if it has `UIFxFollower`) is anchored to the UI card or to the `UIFxAnchor` child.

## Notes & Troubleshooting

- If FX is not visible, make sure the world camera renders the FX layer and that it is not behind the UI. Screen Space Overlay draws last.
- If your card prefab does not have a child named `CardBackground`, the burn background may not be found. Ensure the background image exists and is enabled.
- If the card background stays “burnt” after pooling, it’s reset automatically when the burn completes.

## Recommended Patterns

- Use FX Islands only for **high-impact** moments: burn, flash, dissolve.
- Keep UI images as the source of truth for layout and interactivity.
- Treat FX Islands as **opt-in** and **localized** to avoid UI regressions.

