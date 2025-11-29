# Development Log - November 10, 2025 - Effigy Single-Sprite Rendering

## Context
- Multi-cell effigies were spawning one icon per occupied cell, which duplicated artwork and made the new cat/Z piece look tiled.
- Goal: render a single cohesive sprite across the footprint while keeping grid logic, colliders, and drag/drop workflow intact.

## Changes
- Refactored `EffigyGridUI` to support registering sliced sprite sets per effigy (via `EffigySpriteSetInitializer`) so multi-cell art can remain cohesive without duplicating tiles.
- Each occupied cell now pulls from the registered sprite set; otherwise it falls back to the base icon tint.
- Updated the drag ghost to use the same per-cell sprites so previews match placement.
- Added helper scripts (`EffigySpriteSet`, `EffigySpriteSetInitializer` with multi-entry support) and refreshed the documentation section on visual rendering, storage previews, and drag ghosts.

## Follow-Ups
- Rotation/mirroring support will need additional offset logic but the new root-based visuals make it easier.
- Consider adding optional masks if we want per-element tint overlays without modifying the base art.
- Evaluate whether to move effigy visuals into a dedicated overlay layer for sorting if we introduce VFX.


