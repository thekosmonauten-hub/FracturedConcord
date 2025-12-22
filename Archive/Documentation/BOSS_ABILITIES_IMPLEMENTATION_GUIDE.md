# Boss Abilities Implementation Guide - Act 1

## Overview

This document provides a complete implementation plan for all Act 1 boss abilities, following a registry/handler pattern similar to the Ascendancy modifier system.

---

## Architecture Design

### **Proposed System: Boss Ability Registry**

Similar to `ModifierEffectType`, we'll create a `BossAbilityType` enum and `BossAbilityHandler` static class.

```
BossAbilityType (Enum) 
  ↓
BossAbilityHandler (Static Handler Class)
  ↓
Individual Ability Effects (Scriptable Objects)
```

**Key Files to Create:**
1. `BossAbilityType.cs` - Enum of all unique boss mechanics
2. `BossAbilityHandler.cs` - Centralized handler for complex abilities
3. Individual `AbilityEffect` subclasses for reusable components

---

## Act 1 Boss Abilities Analysis

### **Complexity Tiers:**

**🟢 Tier 1: Simple (Use Existing Systems)**
- Standard damage, status effects, summons
- Can be implemented with current `DamageEffect`, `StatusEffectEffect`, `SummonEffect`

**🟡 Tier 2: Moderate (New AbilityEffect Subclass)**
- Requires new effect types but simple logic
- Example: Conditional damage, scaling effects

**🔴 Tier 3: Complex (Registry + Event Handlers)**
- Requires tracking player actions across turns
- Needs deep integration with card/combat systems
- Best implemented via `BossAbilityHandler` registry

---

## Boss-by-Boss Implementation Plan

### **1. The First to Fall (Encounter 1)** 🔴🟡

**Location:** `Resources/Enemies/Act1/WhereNightFirstFell/BOSS_FirstToFall.asset`

**Current Status:** ✅ Boss exists with 4 abilities

**Abilities to Implement:**

#### **Sundering Echo** 🔴 TIER 3
> "Your first card each turn is duplicated with corrupted values."

**Implementation Type:** `BossAbilityType.SunderingEcho`

**Requirements:**
- Track first card played each turn
- Duplicate card to player's hand
- Corrupt values (damage reduced, cost increased, or negative effects)
- Reset tracker at turn start

**Handler Method:**
```csharp
public static void ProcessSunderingEcho(Card firstCard, Character player)
{
    // Create corrupted copy
    Card corruptedCard = firstCard.Clone();
    corruptedCard.baseDamage = Mathf.FloorToInt(firstCard.baseDamage * 0.5f); // 50% damage
    corruptedCard.manaCost += 1; // +1 cost
    corruptedCard.cardName = "Corrupted " + firstCard.cardName;
    
    // Add to hand
    CombatDeckManager.Instance.AddCardToHand(corruptedCard);
}
```

**Trigger:** OnCardPlayed (first card only)
**Cooldown:** None (every turn)
**Energy Cost:** 0 (passive effect)

---

#### **Black Dawn Crash** 🟡 TIER 2
> "Deals damage based on how many cards you played last turn."

**Implementation Type:** New `ScalingDamageEffect : AbilityEffect`

**Requirements:**
- Track cards played in player's last turn
- Store count in Enemy or combat state
- Calculate damage: `baseDamage + (cardsPlayed × scalingPerCard)`

**Parameters:**
```csharp
public int baseDamage = 15;
public int damagePerCardPlayed = 8;
```

**Formula:** `Damage = 15 + (cardsPlayedLastTurn × 8)`

**Trigger:** OnAttack
**Cooldown:** 3 turns
**Energy Cost:** 50

---

### **3. Orchard-Bound Widow (Encounter 3)** 🟢🟡

**Boss:** The Orchard-Bound Widow
**Folder:** `Resources/Enemies/Act1/WhisperingOrchard/`

#### **Root Lash** 🟡 TIER 2
> "Applies bind (can't block next turn)."

**Implementation Type:** New status effect `StatusEffectType.Bind`

**Requirements:**
- Add `Bind` to status effect enum
- Prevent Guard-granting cards from working
- Duration: 1 turn

**Effect:**
- New `StatusEffectEffect` with `statusType = Bind`
- Hook into card validation: `CanPlayCard()` checks for Bind
- Block only Guard-type cards, allow attacks

**Trigger:** OnAttack
**Cooldown:** 2 turns
**Energy Cost:** 30

---

#### **Memory Sap** 🟢 TIER 1
> "Exhausts a random card."

**Implementation Type:** New `ExhaustCardEffect : AbilityEffect`

**Requirements:**
- Access player's hand
- Select random card
- Move to exhaust pile (or remove from game)

**Pseudo-code:**
```csharp
var hand = CombatDeckManager.Instance.GetHand();
if (hand.Count > 0)
{
    int randomIndex = Random.Range(0, hand.Count);
    Card exhaustedCard = hand[randomIndex];
    CombatDeckManager.Instance.ExhaustCard(exhaustedCard);
}
```

**Trigger:** OnTurnStart
**Cooldown:** 4 turns
**Energy Cost:** 60

---

### **4. Husk Stalker (Encounter 4)** 🟡🟢

**Boss:** Husk Stalker  
**Folder:** `Resources/Enemies/Act1/HollowGrainfields/`

#### **Shatterflail** 🟡 TIER 2
> "Breaks armor-type defenses."

**Implementation Type:** `BreakGuardEffect : AbilityEffect`

**Requirements:**
- Remove all Guard from player
- Optionally prevent Guard for 1 turn

**Effect:**
```csharp
player.currentGuard = 0f;
// Apply status: "Shattered Defense" - Can't gain Guard next turn
```

**Trigger:** OnAttack (replaces normal attack)
**Cooldown:** 3 turns
**Energy Cost:** 40

---

#### **Hollow Drawl** 🟡 TIER 2
> "Reduces card draw next turn."

**Implementation Type:** New status effect `StatusEffectType.DrawReduction`

**Requirements:**
- Add status that modifies `cardsDrawnPerTurn`
- Apply debuff: -1 or -2 card draw
- Duration: 1 turn (affects next turn only)

**Effect:**
```csharp
StatusEffect drawReduction = new StatusEffect(
    StatusEffectType.DrawReduction,
    "Hollow Drawl",
    1f, // -1 card
    1    // 1 turn
);
```

**Trigger:** OnTurnEnd
**Cooldown:** 2 turns
**Energy Cost:** 25

---

### **5. Bridge Warden Remnant (Encounter 5)** 🔴🟡

**Boss:** Bridge Warden Remnant  
**Folder:** `Resources/Enemies/Act1/SplinteredBridge/`

#### **Judgment Loop** 🔴 TIER 3
> "Repeats its last attack if you repeat a card type."

**Implementation Type:** `BossAbilityType.JudgmentLoop`

**Requirements:**
- Track last 2 cards played by player
- If same CardType, trigger ability
- Store boss's last attack damage
- Re-execute attack

**Handler Method:**
```csharp
public static void CheckJudgmentLoop(Card lastCard, Card previousCard, Enemy boss)
{
    if (lastCard.cardType == previousCard.cardType)
    {
        // Repeat boss's last attack
        int repeatDamage = boss.lastAttackDamage;
        CharacterManager.Instance.TakeDamage(repeatDamage);
        LogMessage($"Judgment Loop! {boss.enemyName} repeats attack for {repeatDamage} damage!");
    }
}
```

**Trigger:** OnCardPlayed (reactive)
**Cooldown:** None (passive)
**Energy Cost:** 0

---

#### **Crossing Denied** 🟡 TIER 2
> "Cancels your next buff."

**Implementation Type:** `BossAbilityType.BuffCancellation`

**Requirements:**
- Apply status: "Buff Denial"
- When player plays buff card, prevent it and remove status
- Duration: Until consumed or 2 turns

**Effect:**
```csharp
StatusEffect buffDenial = new StatusEffect(
    StatusEffectType.BuffDenial,
    "Crossing Denied",
    1f,
    2
);
```

**Trigger:** OnTurnStart
**Cooldown:** 4 turns
**Energy Cost:** 50

---

### **6. River-Sworn Rotmass (Encounter 6)** 🔴🟡

**Boss:** The River-Sworn Rotmass  
**Folder:** `Resources/Enemies/Act1/RotfallCreek/`

#### **Bile Torrent** 🔴 TIER 3
> "Adds 'Rot' cards to your deck."

**Implementation Type:** `BossAbilityType.AddCurseCards`

**Requirements:**
- Create "Rot" curse card (negative effect, costs mana, does nothing or hurts)
- Add to player's draw pile or discard pile
- Number of cards: 2-3

**Rot Card Properties:**
```json
{
  "cardName": "Rot",
  "cardType": "Curse",
  "manaCost": 1,
  "description": "Curse. When played, take 5 chaos damage. Exhausts.",
  "effects": [
    {"effectType": "Damage", "value": 5, "targetsSelf": true}
  ]
}
```

**Handler Method:**
```csharp
public static void AddRotCards(int count)
{
    Card rotCard = CreateRotCurseCard();
    for (int i = 0; i < count; i++)
    {
        CombatDeckManager.Instance.AddCardToDiscardPile(rotCard.Clone());
    }
}
```

**Trigger:** OnTurnStart
**Cooldown:** 5 turns
**Energy Cost:** 70

---

#### **Seep** 🔴 TIER 3
> "Deals damage whenever you gain Block."

**Implementation Type:** `BossAbilityType.ReactiveSeep`

**Requirements:**
- Hook into player Guard gain events
- Track boss state: is Seep active?
- Deal damage each time Guard is gained

**Event Hook:**
```csharp
// In BossAbilityHandler
public static void OnPlayerGainedGuard(float guardAmount, Enemy rotmass)
{
    if (rotmass.HasAbility(BossAbilityType.Seep))
    {
        int damage = Mathf.FloorToInt(guardAmount * 0.5f); // 50% of guard as damage
        CharacterManager.Instance.TakeDamage(damage);
        Log($"Seep! Rotmass deals {damage} damage (you gained {guardAmount} guard)");
    }
}
```

**Trigger:** Passive (reactive to player actions)
**Cooldown:** None
**Energy Cost:** 0

---

### **7. Weeper-of-Bark (Encounter 7)** 🟢🔴

**Boss:** Weeper-of-Bark  
**Folder:** `Resources/Enemies/Act1/SighingWoods/`

#### **Splinter Cry** 🟢 TIER 1
> "Multihit physical."

**Implementation Type:** Existing `DamageEffect` with multi-hit parameter

**Requirements:**
- Use `DamageEffect` 
- Add parameter: `hitCount = 3`
- Each hit: 30% of baseDamage

**Configuration:**
```
DamageEffect:
  flatDamage: 12
  hitCount: 3
  damageType: Physical
```

**Trigger:** OnAttack
**Cooldown:** 0 (can use anytime)
**Energy Cost:** 40

---

#### **Echo of Breaking** 🔴 TIER 3
> "When you play a skill, it deals retaliation."

**Implementation Type:** `BossAbilityType.RetaliationOnSkill`

**Requirements:**
- Hook into OnCardPlayed event
- Check if card is Skill type
- Deal retribution damage

**Handler Method:**
```csharp
public static void CheckEchoOfBreaking(Card playedCard, Enemy boss)
{
    if (playedCard.cardType == CardType.Skill)
    {
        int damage = boss.baseDamage;
        CharacterManager.Instance.TakeDamage(damage);
        Log($"Echo of Breaking! You played a Skill, {boss.enemyName} retaliates for {damage}!");
    }
}
```

**Trigger:** OnCardPlayed (reactive)
**Cooldown:** None (passive)
**Energy Cost:** 0

---

### **8. Entropic Traveler (Encounter 8)** 🔴🔴

**Boss:** Entropic Traveler  
**Folder:** `Resources/Enemies/Act1/WoeMilestonePass/`

#### **Empty Footfalls** 🔴 TIER 3
> "Avoids your first attack each turn."

**Implementation Type:** `BossAbilityType.AvoidFirstAttack`

**Requirements:**
- Track attacks per turn (reset at turn start)
- First attack deals 0 damage (100% evasion)
- Apply evasion status for first hit only

**Handler Method:**
```csharp
public static bool ShouldEvadeAttack(Enemy boss)
{
    if (boss.GetCustomData("attacksThisTurn") == 0)
    {
        boss.SetCustomData("attacksThisTurn", 1);
        return true; // Evade
    }
    return false; // Don't evade subsequent attacks
}
```

**Trigger:** OnDamaged (reactive, first hit only)
**Cooldown:** None (passive, resets each turn)
**Energy Cost:** 0

---

#### **Dust Trail** 🔴 TIER 3
> "Gives you temporary 'Wither' cards."

**Implementation Type:** `BossAbilityType.AddTemporaryCurse`

**Requirements:**
- Create "Wither" curse card
- Add to player's hand directly (not deck)
- Cards vanish at end of turn (temporary)

**Wither Card:**
```json
{
  "cardName": "Wither",
  "cardType": "Curse",
  "manaCost": 0,
  "description": "Curse. Vanishes at end of turn. Reduces your damage by 10% this turn.",
  "tags": ["Temporary", "Curse"]
}
```

**Handler Method:**
```csharp
public static void AddTemporaryWitherCards(int count)
{
    Card witherCard = CreateWitherCurseCard();
    for (int i = 0; i < count; i++)
    {
        CombatDeckManager.Instance.AddCardToHand(witherCard);
    }
}
```

**Trigger:** OnTurnStart
**Cooldown:** 3 turns
**Energy Cost:** 55

---

### **9. Fieldborn Aberrant (Encounter 9)** 🟢🔴

**Boss:** Fieldborn Aberrant  
**Folder:** `Resources/Enemies/Act1/BlightTilledMeadow/`

#### **Verdant Collapse** 🟢 TIER 1
> "Deals AoE-type damage."

**Implementation Type:** Existing `DamageEffect` targeting `AllPlayers`

**Configuration:**
```
DamageEffect:
  flatDamage: 18
  target: AllPlayers (or just Player since single player game)
  damageType: Chaos
```

**Trigger:** OnAttack
**Cooldown:** 2 turns
**Energy Cost:** 65

---

#### **Bloom of Ruin** 🔴 TIER 3
> "Places a weak zone that triggers when you block."

**Implementation Type:** `BossAbilityType.BloomOfRuin`

**Requirements:**
- Apply status: "Bloom of Ruin" (tracks zone)
- Hook into player Guard gain event
- When Guard gained, deal damage and remove zone

**Handler Method:**
```csharp
public static void OnPlayerGainedGuardWithBloom(float guardGained, Enemy boss)
{
    if (HasActiveBloom(boss))
    {
        int damage = 20;
        CharacterManager.Instance.TakeDamage(damage);
        RemoveBloom(boss);
        Log($"Bloom of Ruin triggered! {damage} damage dealt.");
    }
}
```

**Trigger:** OnTurnStart (places zone)
**Cooldown:** 4 turns
**Energy Cost:** 60

---

### **10. Bandit Reclaimer (Encounter 10)** 🟢🔴

**Boss:** Bandit Reclaimer  
**Folder:** `Resources/Enemies/Act1/ThornedPalisade/`

#### **Bestial Graft** 🟢 TIER 1
> "Gains random buffs."

**Implementation Type:** Existing `StatusEffectEffect` with randomization

**Configuration:**
```csharp
// Randomly select from:
- Strength (+3, 2 turns)
- Bolster (2 stacks, 3 turns)
- Shield (20, 2 turns)
```

**Trigger:** OnTurnStart
**Cooldown:** 2 turns
**Energy Cost:** 40

---

#### **Predation Lineage** 🔴 TIER 3
> "Learns one of your cards mid-fight."

**Implementation Type:** `BossAbilityType.LearnPlayerCard`

**Requirements:**
- Select random card from player's discard pile
- Store as boss's "learned card"
- Boss can cast this card during combat
- Requires card execution from enemy side

**Handler Method:**
```csharp
public static void LearnRandomCard(Enemy boss)
{
    var discardPile = CombatDeckManager.Instance.GetDiscardPile();
    if (discardPile.Count > 0)
    {
        Card learnedCard = discardPile[Random.Range(0, discardPile.Count)];
        boss.SetCustomData("learnedCard", learnedCard);
        Log($"{boss.enemyName} learned {learnedCard.cardName}!");
    }
}

public static void CastLearnedCard(Enemy boss)
{
    Card learned = boss.GetCustomData("learnedCard") as Card;
    if (learned != null)
    {
        // Execute card effect on player
        ExecuteCardAsEnemy(learned, boss);
    }
}
```

**Trigger:** PhaseGate at 70% HP (learns), then OnAttack (casts)
**Cooldown:** Learns once, can cast every 2 turns
**Energy Cost:** 80 (for casting)

---

### **11. The Lantern Wretch (Encounter 11)** 🟡🟡

**Boss:** The Lantern Wretch  
**Folder:** `Resources/Enemies/Act1/HalfLitRoad/`

#### **Blindflare** 🟡 TIER 2
> "Reduces accuracy (or applies misplay)."

**Implementation Type:** New status `StatusEffectType.Blind` or `Accuracy Reduction`

**Requirements:**
- Reduce player accuracy by 30%
- Miss chance on attacks: 30%
- Duration: 2 turns

**Effect:**
```csharp
StatusEffect blind = new StatusEffect(
    StatusEffectType.Blind,
    "Blindflare",
    30f, // 30% accuracy reduction
    2
);
```

**Trigger:** OnTurnStart
**Cooldown:** 3 turns
**Energy Cost:** 35

---

#### **Broken Lens** 🟡 TIER 2
> "Reflects 50% of next damage."

**Implementation Type:** New `StatusEffectType.DamageReflection`

**Requirements:**
- Apply buff to boss
- On next damage taken, reflect 50%
- Consumed after one hit (vulnerabilityConsumed pattern)

**Effect:**
```csharp
StatusEffect reflection = new StatusEffect(
    StatusEffectType.DamageReflection,
    "Broken Lens",
    0.5f, // 50% reflection
    1     // Until next hit
);
```

**Trigger:** OnDamaged (applies after taking damage)
**Cooldown:** 4 turns
**Energy Cost:** 50

---

### **12. Charred Homesteader (Encounter 12)** 🟢🔴

**Boss:** Charred Homesteader  
**Folder:** `Resources/Enemies/Act1/AsheslopeRidge/`

#### **Coalburst** 🟢 TIER 1
> "Burn stacks."

**Implementation Type:** Existing `StatusEffectEffect` for Burn

**Configuration:**
```
StatusEffectEffect:
  statusType: Burn
  magnitude: 15 (calculated from fire damage)
  durationTurns: 4
  target: Player
```

**Trigger:** OnAttack
**Cooldown:** 2 turns
**Energy Cost:** 45

---

#### **Cooling Regret** 🔴 TIER 3
> "Heals if you don't hit him each turn."

**Implementation Type:** `BossAbilityType.ConditionalHeal`

**Requirements:**
- Track if boss was hit this turn
- Reset flag at turn start
- If flag still false at turn end, heal

**Handler Method:**
```csharp
public static void CheckCoolingRegret(Enemy boss)
{
    bool wasHitThisTurn = boss.GetCustomData("hitThisTurn") as bool? ?? false;
    
    if (!wasHitThisTurn)
    {
        int healAmount = Mathf.FloorToInt(boss.maxHealth * 0.15f); // 15% max HP
        boss.Heal(healAmount);
        Log($"Cooling Regret! {boss.enemyName} heals for {healAmount}.");
    }
    
    // Reset flag for next turn
    boss.SetCustomData("hitThisTurn", false);
}
```

**Trigger:** OnTurnEnd (check), OnDamaged (set flag)
**Cooldown:** None (passive)
**Energy Cost:** 0

---

### **13. Concordial Echo-Beast (Encounter 13)** 🟡🔴

**Boss:** Concordial Echo-Beast  
**Folder:** `Resources/Enemies/Act1/FoldingVale/`

#### **Twin Snarl** 🟡 TIER 2
> "Strikes twice with different damage types."

**Implementation Type:** Multiple `DamageEffect` instances

**Configuration:**
```
Ability: Twin Snarl
  Effect 1: DamageEffect (Fire, 15 damage)
  Effect 2: DamageEffect (Lightning, 12 damage)
```

**Trigger:** OnAttack
**Cooldown:** 0 (can use often)
**Energy Cost:** 50

---

#### **Afterbite** 🔴 TIER 3
> "Delayed damage triggers 2 turns later."

**Implementation Type:** `BossAbilityType.DelayedDamageTrigger`

**Requirements:**
- Apply status: "Afterbite Mark"
- Store damage amount
- Track turns remaining: 2
- At 0, deal stored damage

**Handler Method:**
```csharp
public static void ApplyAfterbite(Enemy boss, int damage, int delayTurns)
{
    StatusEffect afterbite = new StatusEffect(
        StatusEffectType.DelayedDamage,
        "Afterbite",
        damage,
        delayTurns
    );
    // Apply to player
    playerDisplay.AddStatusEffect(afterbite);
}
```

**Trigger:** OnAttack
**Cooldown:** 3 turns
**Energy Cost:** 60

---

### **14. Shadow Shepherd (Encounter 14)** 🟢🟢

**Boss:** Shadow Shepherd  
**Folder:** `Resources/Enemies/Act1/PathOfFailingLight/`

#### **Mournful Toll** 🟢 TIER 1
> "Summons adds."

**Implementation Type:** Existing `SummonEffect`

**Configuration:**
```
SummonEffect:
  enemiesToSummon: [Gloom-Touched Peasant, Shuddering Crow]
  summonCount: 2
  randomSelection: true
```

**Trigger:** PhaseGate at 60% HP, then every 4 turns
**Cooldown:** 4 turns
**Energy Cost:** 80

---

#### **Cloak of Dusk** 🔴 TIER 3
> "Gains stealth for 1 turn after being hit with 3 cards."

**Implementation Type:** `BossAbilityType.ConditionalStealth`

**Requirements:**
- Track cards that hit boss this turn
- If >= 3, apply Stealth status next turn
- Stealth: Cannot be targeted

**Handler Method:**
```csharp
public static void CheckCloakOfDusk(Enemy boss)
{
    int cardsHitThisTurn = boss.GetCustomData("cardsHitCount") as int? ?? 0;
    
    if (cardsHitThisTurn >= 3)
    {
        StatusEffect stealth = new StatusEffect(
            StatusEffectType.Invisible,
            "Cloak of Dusk",
            1f,
            1
        );
        ApplyToSelf(boss, stealth);
        Log($"Cloak of Dusk! {boss.enemyName} becomes untargetable!");
        
        // Reset counter
        boss.SetCustomData("cardsHitCount", 0);
    }
}
```

**Trigger:** OnTurnEnd (check counter)
**Cooldown:** None (passive conditional)
**Energy Cost:** 0

---

### **15. Gate Warden of Vassara (Encounter 15)** 🔴🔴

**Boss:** Gate Warden of Vassara  
**Folder:** `Resources/Enemies/Act1/ShatteredGate/`

#### **Last Article** 🔴 TIER 3
> "Negates your strongest buff."

**Implementation Type:** `BossAbilityType.NegateStrongestBuff`

**Requirements:**
- Find player's strongest buff (highest magnitude or duration)
- Remove it
- Show message

**Handler Method:**
```csharp
public static void NegateStrongestBuff(Enemy boss)
{
    var playerStatusMgr = GetPlayerStatusManager();
    var buffs = playerStatusMgr.GetActiveBuffs(); // Filter: !isDebuff
    
    if (buffs.Count == 0) return;
    
    // Find strongest (by magnitude)
    StatusEffect strongest = buffs.OrderByDescending(b => b.magnitude).First();
    playerStatusMgr.RemoveStatusEffect(strongest);
    
    Log($"Last Article! {boss.enemyName} negates your {strongest.effectName}!");
}
```

**Trigger:** OnTurnStart
**Cooldown:** 4 turns
**Energy Cost:** 70

---

#### **Barrier of Dissent** 🔴 TIER 3
> "Every 3 turns becomes invulnerable but loses health."

**Implementation Type:** `BossAbilityType.BarrierOfDissent`

**Requirements:**
- Track turn counter mod 3
- On turn % 3 == 0, apply invulnerability
- Lose 10% max HP
- Duration: 1 turn

**Handler Method:**
```csharp
public static void CheckBarrierOfDissent(Enemy boss, int currentTurn)
{
    if (currentTurn % 3 == 0)
    {
        // Apply invulnerability
        StatusEffect barrier = new StatusEffect(
            StatusEffectType.Invulnerable,
            "Barrier of Dissent",
            1f,
            1
        );
        ApplyToSelf(boss, barrier);
        
        // Lose health
        int healthLoss = Mathf.FloorToInt(boss.maxHealth * 0.10f);
        boss.TakeDamage(healthLoss);
        
        Log($"Barrier of Dissent! {boss.enemyName} becomes invulnerable but loses {healthLoss} HP!");
    }
}
```

**Trigger:** OnTurnStart (every 3rd turn)
**Cooldown:** None (automatic cycle)
**Energy Cost:** 0

---

## Summary Tables

### **Ability Complexity Breakdown**

| Tier | Count | Description |
|------|-------|-------------|
| 🟢 Tier 1 (Simple) | 6 | Use existing AbilityEffect types |
| 🟡 Tier 2 (Moderate) | 8 | Need new AbilityEffect subclasses |
| 🔴 Tier 3 (Complex) | 14 | Need BossAbilityHandler registry |

### **New Systems Required**

#### **1. Status Effect Types to Add:**
```csharp
Bind,                    // Can't play Guard cards next turn
DrawReduction,           // Reduces cards drawn
BuffDenial,              // Negates next buff
DelayedDamage,           // Damage triggers after N turns
Blind,                   // Accuracy reduction
DamageReflection         // Already exists - verify implementation
```

#### **2. New AbilityEffect Subclasses:**
```csharp
ScalingDamageEffect      // Damage based on variable (cards played, etc.)
BreakGuardEffect         // Remove all Guard
ExhaustCardEffect        // Remove card from combat
ConditionalHealEffect    // Heal based on condition
MultiHitDamageEffect     // Multiple hits in one ability (or extend DamageEffect)
```

#### **3. BossAbilityType Enum:**
```csharp
public enum BossAbilityType
{
    // Complex reactive abilities
    SunderingEcho,           // Duplicate first card with corruption
    JudgmentLoop,            // Repeat attack on card type repeat
    AddCurseCards,           // Add curse cards to deck (Bile Torrent, Dust Trail)
    ReactiveSeep,            // Damage on Guard gain
    RetaliationOnSkill,      // Damage when Skill played
    AvoidFirstAttack,        // Evade first hit each turn
    ConditionalHeal,         // Heal based on condition
    ConditionalStealth,      // Stealth after X hits
    NegateStrongestBuff,     // Remove best buff
    BarrierOfDissent,        // Invuln + health loss on cycle
    BloomOfRuin,             // Reactive zone trigger
    LearnPlayerCard,         // Copy and cast player card
}
```

#### **4. BossAbilityHandler Static Class:**
```csharp
public static class BossAbilityHandler
{
    // Event hooks
    public static void OnPlayerCardPlayed(Card card, int turnCardCount);
    public static void OnPlayerGainedGuard(float guardAmount);
    public static void OnPlayerDrawCards(int count);
    
    // Ability processors
    public static void ProcessSunderingEcho(Card firstCard, Enemy boss);
    public static void ProcessJudgmentLoop(Card card, Card previousCard, Enemy boss);
    public static void AddRotCards(int count, Enemy boss);
    public static void ProcessSeep(float guardAmount, Enemy boss);
    public static void ProcessEchoOfBreaking(Card card, Enemy boss);
    public static bool ShouldEvadeAttack(Enemy boss);
    public static void AddTemporaryWitherCards(int count);
    public static void CheckCloakOfDusk(Enemy boss);
    public static void NegateStrongestBuff(Enemy boss);
    public static void CheckBarrierOfDissent(Enemy boss, int turn);
    public static void ProcessBloomOfRuin(float guardAmount, Enemy boss);
    public static void LearnRandomCard(Enemy boss);
    public static void CastLearnedCard(Enemy boss);
    
    // Helper methods
    public static bool HasAbility(this Enemy enemy, BossAbilityType abilityType);
    public static void SetCustomData(this Enemy enemy, string key, object value);
    public static object GetCustomData(this Enemy enemy, string key);
}
```

---

## Implementation Phases

### **Phase 1: Foundation (Priority 1)** ⭐
1. Create `BossAbilityType.cs` enum
2. Create `BossAbilityHandler.cs` static class with base structure
3. Add new `StatusEffectType` values (Bind, DrawReduction, etc.)
4. Add custom data dictionary to `Enemy.cs`

**Estimated Effort:** 2-3 hours

---

### **Phase 2: Simple Abilities (Priority 2)**
Implement Tier 1 abilities using existing systems:
- Splinter Cry (multi-hit)
- Verdant Collapse (AoE damage)
- Mournful Toll (summons)
- Bestial Graft (random buffs)
- Coalburst (burn stacks)
- Memory Sap (exhaust card)

**Estimated Effort:** 1-2 hours

---

### **Phase 3: Moderate Abilities (Priority 3)**
Create new AbilityEffect subclasses:
- `ScalingDamageEffect` (Black Dawn Crash)
- `BreakGuardEffect` (Shatterflail)
- `ExhaustCardEffect` (Memory Sap)
- Conditional effects (Hollow Drawl, Blindflare, Broken Lens)

**Estimated Effort:** 3-4 hours

---

### **Phase 4: Complex Abilities (Priority 4)**
Implement BossAbilityHandler methods for:
- Sundering Echo (card duplication)
- Judgment Loop (reactive repeat)
- Bile Torrent & Dust Trail (curse cards)
- Seep (reactive damage)
- Echo of Breaking (skill retaliation)
- All other Tier 3 abilities

**Estimated Effort:** 6-8 hours

---

### **Phase 5: Testing & Balancing (Priority 5)**
- Test each boss ability in combat
- Balance damage values, cooldowns
- Polish visual effects and messaging
- Create ability preview tooltips

**Estimated Effort:** 2-3 hours

---

## Integration Points

### **Events to Hook Into:**

1. **CombatDeckManager:**
   - `OnCardPlayed(Card card)` - For Sundering Echo, Judgment Loop, Echo of Breaking
   - `OnCardDrawn(int count)` - For tracking card draws

2. **Character / CharacterManager:**
   - `OnGuardGained(float amount)` - For Seep, Bloom of Ruin
   - `OnBuffApplied(StatusEffect buff)` - For Crossing Denied

3. **CombatDisplayManager:**
   - `OnTurnStart` / `OnTurnEnd` - For turn-based triggers
   - `OnEnemyDamaged(Enemy enemy)` - For tracking hits

4. **Enemy:**
   - Add `Dictionary<string, object> customData` for tracking state
   - Add `int lastAttackDamage` for Judgment Loop
   - Add `bool hitThisTurn` for Cooling Regret

---

## File Structure

```
Assets/Scripts/Combat/Abilities/
  ├── BossAbilityType.cs              [NEW]
  ├── BossAbilityHandler.cs           [NEW]
  ├── Effects/
  │   ├── ScalingDamageEffect.cs      [NEW]
  │   ├── BreakGuardEffect.cs         [NEW]
  │   ├── ExhaustCardEffect.cs        [NEW]
  │   ├── ConditionalEffect.cs        [NEW]
  │   ├── CurseCardEffect.cs          [NEW]
  │   └── [existing effects...]
  └── [existing files...]

Assets/Resources/Cards/Curses/        [NEW FOLDER]
  ├── Rot.asset
  ├── Wither.asset
  └── Thesis.asset (for Act 2)

Assets/Resources/Enemies/Act1/
  ├── WhereNightFirstFell/
  │   └── Abilities/FirstToFall/
  │       ├── SunderingEcho.asset     [UPDATE]
  │       └── BlackDawnCrash.asset    [UPDATE]
  ├── WhisperingOrchard/
  │   └── Abilities/OrchardWidow/     [NEW]
  ├── [etc...]
```

---

## Next Steps

1. **Review this document** - Confirm approach and mechanics
2. **Prioritize bosses** - Which ones are most important?
3. **Create foundation** - Implement Phase 1 (BossAbilityType, Handler, custom data)
4. **Implement incrementally** - One boss at a time, testing as you go

---

## Notes

- Many abilities require tracking player actions (card played, guard gained, etc.)
- Some abilities need curse card creation (Rot, Wither, Thesis)
- Complex abilities like "Learn Player Card" require card execution from enemy side
- Visual feedback and intent display should show ability names
- All abilities should have proper energy costs and cooldowns for balance

---

**Ready to proceed?** Let me know which phase you'd like to start with, or if you'd like me to refine any specific ability mechanics!

