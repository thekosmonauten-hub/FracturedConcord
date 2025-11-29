# Development Log - November 11, 2025 - Effigy Affix Overhaul

## Context
- Effigy ScriptableObjects now act as blueprints that should roll loot-style affixes when dropped instead of being static assets.
- Design goals: reuse the full equipment affix catalog, ensure every modifier is global, scale magnitudes down to 10%, and allow any prefix/suffix mix up to four slots while persisting rolled instances across scenes.

## Changes
- Introduced `EffigyFactory` to clone blueprint assets, deep-copy implicit modifiers, and trigger affix rolling so every drop becomes a unique runtime ScriptableObject.
- Rebuilt `EffigyAffixGenerator` to pull from the shared `AffixDatabase`, apply weighted selection, enforce 0–4 total affixes, scale numeric values by 0.1×, force global scope, and rebuild descriptions from the scaled rolls.
- Updated `Effigy` overrides to cap total explicit affixes at four and compute rarity via the new rules (`0 → Normal`, `1–2 → Magic`, `3–4 → Rare`, Unique untouched).
- Effigy storage now instantiates runtime clones using the factory, ensuring UI previews show rolled stats without mutating blueprints. Both the equipment screen and loot manager now source data from `Character.ownedEffigies`, keeping drops, storage, and equipping in sync.
- Loot tables gained an `Effigy` reward type—assign any blueprint to a row and the generator rolls/scales it automatically before persisting the runtime instance to the active character.
- Documentation expanded with blueprint workflow, scaling rules, rarity logic, and loot-table wiring so future contributors understand the entire pipeline.

## Follow-Ups
- Hook the factory into the actual loot drop system so effigy drops outside the equipment screen test harness use the same generator.
- Author implicit modifier tables per shape/size tier, then wire them into the factory before affixes roll.
- Add debug/QA tooling to print or snapshot rolled effigies for balance review (verify distribution of prefixes vs suffixes and scaling edge cases).

