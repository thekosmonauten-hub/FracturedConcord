# Weeper-of-Bark Boss Setup Guide

## Overview
This guide will help you configure the Weeper-of-Bark boss in Unity Editor to test the Boss Ability System.

---

## Step 1: Configure Splinter Cry Ability

**File:** `Assets/Resources/Enemies/Act1/SighingWoods/Abilities/WeeperOfBark_SplinterCry.asset`

### In Unity Editor:

1. Select the `WeeperOfBark_SplinterCry` asset
2. Verify the settings:
   - **Display Name:** "Splinter Cry"
   - **Trigger:** OnAttack (3)
   - **Cooldown Turns:** 2
   - **Consumes Turn:** ✅ True
   - **Target:** Player (1)
   - **Energy Cost:** 40

3. Verify the **Effects** array has 3 DamageEffect entries:
   - Each with **Damage Type:** Physical (0)
   - Each with **Flat Damage:** 12
   - (This creates a 3-hit attack: 12 + 12 + 12 = 36 total damage)

---

## Step 2: Configure Weeper-of-Bark Boss

**File:** `Assets/Resources/Enemies/Act1/SighingWoods/BOSS_WeeperOfBark.asset`

### In Unity Editor:

1. Select the `BOSS_WeeperOfBark` asset
2. Verify **Basic Stats:**
   - **Enemy Name:** "Weeper-of-Bark"
   - **Tier:** Boss (2)
   - **Rarity:** Rare (2)
   - **Min Health:** 250
   - **Max Health:** 300
   - **Base Damage:** 15

3. Set **Abilities (Scriptable):**
   - **Size:** 1
   - **Element 0:** Drag `WeeperOfBark_SplinterCry` asset here

4. Set **Boss Abilities (Complex):**
   - **Size:** 1
   - **Element 0:** Select `RetaliationOnSkill` from dropdown
   - (This enables Echo of Breaking passive ability)

5. Verify **Other Settings:**
   - **Display Scale:** 1.3 (larger than normal enemies)
   - **Exclude From Random:** ✅ True (boss-only)
   - **Attack Preference:** 0.85 (aggressive)

---

## Step 3: Create Test Encounter

You have two options for testing:

### Option A: Add to Existing Encounter
1. Open `Assets/Resources/Encounters/Act1/1.WhereNightFirstFell.asset`
2. Set **Unique Enemy (Final Wave):** Drag `BOSS_WeeperOfBark` asset here
3. This will spawn Weeper-of-Bark as the boss in the final wave

### Option B: Create New Encounter (SighingWoods)
Create a new encounter asset for Encounter 7:

**File to create:** `Assets/Resources/Encounters/Act1/7.SighingWoods.asset`

**Settings:**
- **Encounter ID:** 7
- **Encounter Name:** "Sighing Woods"
- **Scene Name:** "CombatScene"
- **Act Number:** 1
- **Area Level:** 7
- **Total Waves:** 1
- **Max Enemies Per Wave:** 3
- **Unique Enemy (Final Wave):** BOSS_WeeperOfBark
- **Encounter Enemy Pool:** BrambleSkitterer, WindSwayedShade
- **Use Exclusive Enemy Pool:** ✅ True

---

## Step 4: Test in Combat

### Expected Behavior:

**Splinter Cry (Active Ability):**
- Boss uses on Attack intent
- Deals 3 hits of 12 physical damage each
- Shows individual damage numbers
- Cooldown: 2 turns

**Echo of Breaking (Passive Ability):**
- Triggers whenever you play a **Skill** card
- Boss retaliates for 15 damage immediately
- Shows combat log: "Echo of Breaking! Skill retaliation: 15 damage"
- No cooldown (passive)

### Testing Steps:

1. Start the encounter with Weeper-of-Bark
2. Play an **Attack** card → Should work normally
3. Play a **Skill** card → Should trigger retaliation damage
4. Watch boss's turn → If cooldown is up, uses Splinter Cry (3 hits)
5. Check console for debug logs:
   ```
   [Boss Ability] Registered RetaliationOnSkill on Weeper-of-Bark
   [Echo of Breaking] Weeper-of-Bark retaliates for 15 damage!
   ```

---

## Troubleshooting

### Boss ability not triggering?
- Check console for "[Boss Ability] Registered..." message
- Verify `bossAbilities` list has `RetaliationOnSkill` set
- Ensure you're playing a **Skill** card (not Attack)

### Splinter Cry not working?
- Verify ability is in the `abilities` list
- Check boss has enough energy (40 required)
- Check cooldown hasn't been triggered

### No damage from retaliation?
- Check console for event hook debug logs
- Verify `OnPlayerCardPlayed` is being called
- Check if boss is alive when you play the card

---

## Success Criteria

✅ Boss spawns with correct stats  
✅ RetaliationOnSkill registered automatically  
✅ Playing Skill card triggers Echo of Breaking  
✅ Boss uses Splinter Cry ability (3 hits)  
✅ No compilation or runtime errors  

Once this works, we'll know the entire Boss Ability System is functional!

---

## Next Steps After Successful Test

If the test is successful:
1. Implement remaining status effect processing (Bind, Blind, etc.)
2. Create more bosses with their abilities
3. Implement Tier 1 simple abilities
4. Move on to complex Tier 3 abilities

If there are issues:
1. Debug the specific failure point
2. Check console logs
3. Fix integration issues
4. Re-test

---

Ready to test! Let me know how it goes! 🎮

