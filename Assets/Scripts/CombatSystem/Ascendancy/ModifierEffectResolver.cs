using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Resolves and executes modifier actions
/// </summary>
public static class ModifierEffectResolver
{
    /// <summary>
    /// Reset Battle Rhythm tracking for a new turn (call this on OnTurnStart for BattleRhythm_Start modifier)
    /// </summary>
    public static void ResetBattleRhythmTracking(Dictionary<string, object> modifierState)
    {
        if (modifierState != null && modifierState.ContainsKey("battleRhythmPlayedTypes"))
        {
            HashSet<CardType> playedTypes = modifierState["battleRhythmPlayedTypes"] as HashSet<CardType>;
            if (playedTypes != null)
            {
                playedTypes.Clear();
                UnityEngine.Debug.Log("[ModifierEffectResolver] Battle Rhythm: Reset card type tracking for new turn");
            }
        }
    }

    /// <summary>
    /// Resolve a modifier action and execute it
    /// </summary>
    public static void ResolveAction(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, AscendancyModifierDefinition definition)
    {
        if (action == null || character == null)
        {
            Debug.LogWarning("[ModifierEffectResolver] Cannot resolve action - null action or character");
            return;
        }

        // Get parameters dictionary
        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();

        // Special handling: Reset Battle Rhythm tracking on turn start
        // Check if this is an OnTurnStart effect with empty actions (used for resetting tracking)
        if (definition != null && definition.modifierId == "BattleRhythm_Start" && 
            eventContext != null && eventContext.ContainsKey("eventType"))
        {
            object eventTypeObj = eventContext["eventType"];
            if (eventTypeObj is ModifierEventType eventType && eventType == ModifierEventType.OnTurnStart)
            {
                ResetBattleRhythmTracking(modifierState);
                // Also reset card type tracking for Conductors Strike and Second Beat
                if (modifierState != null)
                {
                    if (modifierState.ContainsKey("conductorsStrikeCardTypes"))
                    {
                        ((HashSet<CardType>)modifierState["conductorsStrikeCardTypes"]).Clear();
                    }
                    if (modifierState.ContainsKey("secondBeatCardsPlayed"))
                    {
                        modifierState["secondBeatCardsPlayed"] = 0;
                    }
                }
                // Handle Tactical Transference and Tempered Resolve turn start
                if (definition.modifierId == "TacticalTransference")
                {
                    HandleTacticalTransference(action, eventContext, character, modifierState);
                }
                if (definition.modifierId == "TemperedResolve")
                {
                    HandleTemperedResolve(action, eventContext, character, modifierState);
                }
                if (definition.modifierId == "MartialRefrain")
                {
                    HandleMartialRefrain(action, eventContext, character, modifierState);
                }
                // Hourglass Paradox: Apply bonuses at turn start
                if (definition != null && definition.modifierId == "HourglassParadox")
                {
                    HandleHourglassParadox(action, eventContext, character, modifierState);
                }
                return; // Early return since this effect has no actions
            }
        }
        
        // Handle turn start events for other modifiers
        if (eventContext != null && eventContext.ContainsKey("eventType"))
        {
            object eventTypeObj = eventContext["eventType"];
            if (eventTypeObj is ModifierEventType eventType && eventType == ModifierEventType.OnTurnStart)
            {
                // Hourglass Paradox: Apply bonuses at turn start
                if (definition != null && definition.modifierId == "HourglassParadox")
                {
                    HandleHourglassParadox(action, eventContext, character, modifierState);
                }
            }
        }

        // Handle turn end events
        if (eventContext != null && eventContext.ContainsKey("eventType"))
        {
            object eventTypeObj = eventContext["eventType"];
            if (eventTypeObj is ModifierEventType eventType && eventType == ModifierEventType.OnTurnEnd)
            {
                // Tactical Transference: Check Battle Rhythm at turn end
                if (definition != null && definition.modifierId == "TacticalTransference")
                {
                    HandleTacticalTransference(action, eventContext, character, modifierState);
                }
                // Hourglass Paradox: Check if no mana was spent
                if (definition != null && definition.modifierId == "HourglassParadox")
                {
                    HandleHourglassParadox(action, eventContext, character, modifierState);
                }
                // Future Echo: Decrement turn counters
                if (definition != null && definition.modifierId == "FutureEcho")
                {
                    HandleFutureEchoTurnEnd(action, eventContext, character, modifierState);
                }
            }
        }

        // Handle enemy killed events
        if (eventContext != null && eventContext.ContainsKey("eventType"))
        {
            object eventTypeObj = eventContext["eventType"];
            if (eventTypeObj is ModifierEventType eventType && (eventType == ModifierEventType.OnEnemyKilled || eventType == ModifierEventType.OnEnemyDefeated))
            {
                // Feast of Pain: Heal when killing enemy with 10+ Corruption
                if (definition != null && definition.modifierId == "FeastOfPain")
                {
                    HandleFeastOfPain(action, eventContext, character, modifierState);
                }
            }
        }

        // Handle card played events for tracking
        if (eventContext != null && eventContext.ContainsKey("eventType"))
        {
            object eventTypeObj = eventContext["eventType"];
            if (eventTypeObj is ModifierEventType eventType && eventType == ModifierEventType.OnCardPlayed)
            {
                // Conductors Strike: Track card types
                if (definition != null && definition.modifierId == "ConductorsStrike")
                {
                    HandleConductorsStrike(action, eventContext, character, modifierState);
                }
                // Second Beat: Track cards played
                if (definition != null && definition.modifierId == "SecondBeat")
                {
                    HandleSecondBeat(action, eventContext, character, modifierState);
                }
                // Flow of Iron: Check for Battle Rhythm gain
                if (definition != null && definition.modifierId == "FlowOfIron")
                {
                    HandleFlowOfIron(action, eventContext, character, modifierState);
                }
                // Pulse of War: Check for Battle Rhythm gain
                if (definition != null && definition.modifierId == "PulseOfWar")
                {
                    HandlePulseOfWar(action, eventContext, character, modifierState);
                }
            }
        }

        // Handle Battle Rhythm loss (Flowbreaker, Flow of Iron, Tempered Resolve)
        if (StackSystem.Instance != null && modifierState != null && character != null)
        {
            int battleRhythmStacks = StackSystem.Instance.GetStacks(StackType.BattleRhythm);
            if (battleRhythmStacks == 0 && modifierState.ContainsKey("lastBattleRhythmStacks") && 
                Convert.ToInt32(modifierState["lastBattleRhythmStacks"]) > 0)
            {
                // Battle Rhythm was lost (regardless of how it ended - turn end, duplicate card, etc.)
                
                // Flowbreaker: Deal AoE damage for 30% of total Guard (triggers whenever Battle Rhythm ends)
                // Store the damage in event context so Flowbreaker modifier can process it
                float guardAmount = character.currentGuard;
                float damage = guardAmount * 0.3f; // 30% of total Guard

                if (eventContext != null)
                {
                    eventContext["flowbreakerDamage"] = damage;
                    eventContext["flowbreakerTargetAll"] = true;
                    eventContext["flowbreakerDamageType"] = "Physical";
                    eventContext["flowbreakerTriggered"] = true; // Mark that Battle Rhythm ended and Flowbreaker should trigger
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Battle Rhythm ended - Flowbreaker will deal {damage} AoE damage (30% of {guardAmount} Guard)");
                }
                
                // Flow of Iron: Reset Flow
                if (definition != null && definition.modifierId == "FlowOfIron")
                {
                    HandleFlowOfIron(action, eventContext, character, modifierState);
                }
                
                // Tempered Resolve: Apply damage reduction
                if (definition != null && definition.modifierId == "TemperedResolve")
                {
                    if (!modifierState.ContainsKey("temperedResolveDamageReduction"))
                    {
                        modifierState["temperedResolveDamageReduction"] = 0f;
                    }
                    float currentReduction = Convert.ToSingle(modifierState["temperedResolveDamageReduction"]);
                    modifierState["temperedResolveDamageReduction"] = Mathf.Min(20f, currentReduction + 20f);
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Tempered Resolve: Battle Rhythm lost - damage reduction set to {modifierState["temperedResolveDamageReduction"]}%");
                }
            }
            modifierState["lastBattleRhythmStacks"] = battleRhythmStacks;
        }
        
        // Special handling: Reset Trembling Echo tracking on turn start
        if (definition != null && definition.modifierId == "TremblingEcho" && 
            eventContext != null && eventContext.ContainsKey("eventType"))
        {
            object eventTypeObj = eventContext["eventType"];
            if (eventTypeObj is ModifierEventType eventType && eventType == ModifierEventType.OnTurnStart)
            {
                if (modifierState != null && modifierState.ContainsKey("tremblingEchoUsedThisTurn"))
                {
                    modifierState["tremblingEchoUsedThisTurn"] = false;
                    UnityEngine.Debug.Log("[ModifierEffectResolver] Trembling Echo: Reset for new turn");
                }
            }
        }

        switch (action.actionType)
        {
            case ModifierActionType.AddStack:
                ResolveAddStack(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.RemoveStack:
                ResolveRemoveStack(action, parameters, character);
                break;
            case ModifierActionType.ConsumeStack:
                ResolveConsumeStack(action, parameters, character);
                break;
            case ModifierActionType.SetStack:
                ResolveSetStack(action, parameters, character);
                break;
            case ModifierActionType.ClearStack:
                ResolveClearStack(action, parameters, character);
                break;
            case ModifierActionType.ModifyMaxStacks:
                ResolveModifyMaxStacks(action, parameters, character);
                break;
            case ModifierActionType.AddFlatDamage:
                ResolveAddFlatDamage(action, parameters, eventContext);
                // Check for Bulwark Pulse: Deal damage when guard is gained (OnGuardGained event)
                if (definition != null && definition.modifierId == "BulwarkPulse" && 
                    eventContext != null && eventContext.ContainsKey("guardGained"))
                {
                    HandleBulwarkPulse(action, eventContext, character);
                }
                break;
            case ModifierActionType.AddPercentDamage:
                ResolveAddPercentDamage(action, parameters, eventContext);
                break;
            case ModifierActionType.AddDamageMorePercent:
                ResolveAddDamageMorePercent(action, eventContext, character, modifierState, definition);
                // Check for Final Offering: Double damage at ≤25% HP
                if (definition != null && definition.modifierId == "FinalOffering")
                {
                    HandleFinalOfferingDamage(action, eventContext, character, modifierState);
                }
                // Check for Flow of Iron: +50% damage at 10 Flow
                if (definition != null && definition.modifierId == "FlowOfIron")
                {
                    HandleFlowOfIron(action, eventContext, character, modifierState);
                }
                // Check for Conductors Strike: +5% per card type
                if (definition != null && definition.modifierId == "ConductorsStrike")
                {
                    HandleConductorsStrike(action, eventContext, character, modifierState);
                }
                // Check for Flux Weaving: Apply damage bonus on alternation
                if (definition != null && definition.modifierId == "FluxWeaving")
                {
                    HandleFluxWeaving(action, eventContext, character, modifierState);
                }
                break;
            case ModifierActionType.AddElementalDamage:
                ResolveAddElementalDamage(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.AddExtraHit:
                ResolveAddExtraHit(action, eventContext, character, modifierState, definition);
                // Check for Trembling Echo: Repeat first attack if target has Crumble
                if (definition != null && definition.modifierId == "TremblingEcho")
                {
                    HandleTremblingEcho(action, eventContext, character, modifierState);
                }
                // Check for Guarded Motion: Extra hit on next attack after guard
                if (definition != null && definition.modifierId == "GuardedMotion")
                {
                    HandleGuardedMotion(action, eventContext, character, modifierState);
                }
                break;
            case ModifierActionType.ApplyStatus:
            case ModifierActionType.ApplyStatusToEnemy:
                ResolveApplyStatus(action, eventContext, character);
                // Check for Blood Price: Apply Pressure when attacking
                if (definition != null && definition.modifierId == "BloodPrice" && 
                    action.parameters.ContainsKey("statusEffectType") && 
                    action.parameters["statusEffectType"].ToString() == "Pressure")
                {
                    HandleBloodPrice(action, eventContext, character, modifierState);
                }
                // Check for Tormented Knowledge: Mark next card for bonus when losing life from own effects
                if (definition != null && definition.modifierId == "TormentedKnowledge" && 
                    eventContext != null && eventContext.ContainsKey("damageSource") && 
                    eventContext["damageSource"].ToString() == "Self")
                {
                    HandleTormentedKnowledge(action, eventContext, character, modifierState);
                }
                // Check for Descent of Thought: Random attribute gain when losing life from own effects
                if (definition != null && definition.modifierId == "DescentOfThought" && 
                    eventContext != null && eventContext.ContainsKey("damageSource") && 
                    eventContext["damageSource"].ToString() == "Self")
                {
                    HandleDescentOfThought(action, eventContext, character, modifierState);
                }
                break;
            case ModifierActionType.RemoveStatus:
                ResolveRemoveStatus(action, parameters, character);
                break;
            case ModifierActionType.AddGuard:
                ResolveAddGuard(action, parameters, eventContext, character);
                // Check for Guarded Motion: Mark next attack for extra hit
                if (definition != null && definition.modifierId == "GuardedMotion")
                {
                    HandleGuardedMotion(action, eventContext, character, modifierState);
                }
                // Check for Flux Weaving: Apply guard bonus on alternation
                if (definition != null && definition.modifierId == "FluxWeaving")
                {
                    HandleFluxWeaving(action, eventContext, character, modifierState);
                }
                break;
            case ModifierActionType.AddMana:
                ResolveAddMana(action, parameters, character);
                // Check for Abyssal Bargain: Recover mana and draw card at Corruption thresholds
                if (definition != null && definition.modifierId == "AbyssalBargain")
                {
                    HandleAbyssalBargain(action, eventContext, character, modifierState);
                }
                break;
            case ModifierActionType.DrawCard:
                ResolveDrawCard(action, parameters);
                // Check for Abyssal Bargain: Recover mana and draw card at Corruption thresholds
                if (definition != null && definition.modifierId == "AbyssalBargain")
                {
                    HandleAbyssalBargain(action, eventContext, character, modifierState);
                }
                break;
            case ModifierActionType.DiscardCard:
                ResolveDiscardCard(action, parameters);
                break;
            case ModifierActionType.ModifyHealthPercent:
                ResolveModifyHealthPercent(action, parameters, character);
                break;
            case ModifierActionType.MarkCardAsTemporal:
                ResolveMarkCardAsTemporal(action, eventContext, character);
                break;
            case ModifierActionType.EchoTemporalCards:
                ResolveEchoTemporalCards(action, parameters, character);
                // Check for Echoing Will: Increment echo count
                if (definition != null && definition.modifierId == "EchoingWill")
                {
                    HandleEchoingWill(action, eventContext, character, modifierState);
                }
                break;
            case ModifierActionType.TriggerChaosSurge:
                ResolveTriggerChaosSurge(action, parameters, character, modifierState, definition);
                // Check for Echo of Agony: Mark next card for duplication after Chaos Surge
                if (definition != null && definition.modifierId == "EchoOfAgony")
                {
                    HandleEchoOfAgony(action, eventContext, character, modifierState);
                }
                break;
            case ModifierActionType.ModifyStat:
                ResolveModifyStat(action, parameters, character, modifierState);
                // Check for Damage And Rhythm: Calculate crit chance
                if (definition != null && definition.modifierId == "DamageAndRhythm")
                {
                    HandleDamageAndRhythm(action, eventContext, character, modifierState);
                }
                // Check for Damage and Guard Balance: Conditional stats
                if (definition != null && definition.modifierId == "DamageAndGuardBalance")
                {
                    HandleDamageAndGuardBalance(action, eventContext, character, modifierState);
                }
                // Check for Searing Insight: Apply damage bonus based on stacks
                if (definition != null && definition.modifierId == "SearingInsight")
                {
                    HandleSearingInsight(action, eventContext, character, modifierState);
                }
                // Check for Pact of the Harvester: Apply damage bonus per Corruption threshold
                if (definition != null && definition.modifierId == "PactOfTheHarvester")
                {
                    HandlePactOfTheHarvester(action, eventContext, character, modifierState);
                }
                // Check for Descent of Thought: Random attribute gain
                if (definition != null && definition.modifierId == "DescentOfThought")
                {
                    HandleDescentOfThought(action, eventContext, character, modifierState);
                }
                // Check for Chaotic Pendulum: Apply element-specific damage bonus
                if (definition != null && definition.modifierId == "ChaoticPendulum")
                {
                    HandleChaoticPendulum(action, eventContext, character, modifierState);
                }
                // Check for Blades of Conduction: Apply weapon/spell scaling
                if (definition != null && definition.modifierId == "BladesOfConduction")
                {
                    HandleBladesOfConduction(action, eventContext, character, modifierState);
                }
                // Check for Blades Ascend Spells Break: Mark next card as hybrid
                if (definition != null && definition.modifierId == "BladesAscendSpellsBreak")
                {
                    HandleBladesAscendSpellsBreak(action, eventContext, character, modifierState);
                }
                // Check for Twin Arts Invocation: Track attacks/spells for duplication
                if (definition != null && definition.modifierId == "TwinArtsInvocation")
                {
                    HandleTwinArtsInvocation(action, eventContext, character, modifierState);
                }
                // Check for Elemental Branding: Apply subnode effects
                if (definition != null && definition.modifierId == "ElementalBranding")
                {
                    HandleElementalBranding(action, eventContext, character, modifierState);
                }
                // Check for Future Echo: Queue first card of turn
                if (definition != null && definition.modifierId == "FutureEcho")
                {
                    HandleFutureEcho(action, eventContext, character, modifierState);
                }
                // Check for Hourglass Paradox: Track mana spent
                if (definition != null && definition.modifierId == "HourglassParadox")
                {
                    HandleHourglassParadox(action, eventContext, character, modifierState);
                }
                // Check for Borrowed Power: Apply Temporal card bonuses
                if (definition != null && definition.modifierId == "BorrowedPower")
                {
                    HandleBorrowedPower(action, eventContext, character, modifierState);
                }
                // Check for Suspended Moment: Apply DoT speed multiplier
                if (definition != null && definition.modifierId == "SuspendedMoment")
                {
                    HandleSuspendedMoment(action, eventContext, character, modifierState);
                }
                // Check for Echoing Will: Apply power bonus based on echoes
                if (definition != null && definition.modifierId == "EchoingWill")
                {
                    HandleEchoingWill(action, eventContext, character, modifierState);
                }
                // Check for Chrono Collapse: Spread debuffs on enemy death
                if (definition != null && definition.modifierId == "ChronoCollapse")
                {
                    HandleChronoCollapse(action, eventContext, character, modifierState);
                }
                // Check for Tempered Resolve: Damage reduction tracking
                if (definition != null && definition.modifierId == "TemperedResolve")
                {
                    HandleTemperedResolve(action, eventContext, character, modifierState);
                }
                break;
            case ModifierActionType.ModifyDamageMultiplier:
                ResolveModifyDamageMultiplier(action, parameters, eventContext, character);
                // Check for Mirrorsteel Guard: Apply damage reduction per stack
                if (definition != null && definition.modifierId == "MirrorsteelGuard")
                {
                    HandleMirrorsteelGuard(action, eventContext, character, modifierState);
                }
                break;
            default:
                Debug.LogWarning($"[ModifierEffectResolver] Unhandled action type: {action.actionType}");
                break;
        }
    }

    private static void ResolveAddStack(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, AscendancyModifierDefinition definition)
    {
        if (!action.parameters.ContainsKey("stackType")) return;

        string stackTypeStr = action.parameters["stackType"].ToString();
        if (Enum.TryParse<StackType>(stackTypeStr, out StackType stackType))
        {
            // Special handling for Battle Rhythm: Only add stack when all three card types are played
            if (definition != null && definition.modifierId == "BattleRhythm_Start" && stackType == StackType.BattleRhythm)
            {
                HandleBattleRhythmStack(eventContext, character, modifierState);
                return;
            }

            // Special handling for Braced Assault: Track guard cards played
            if (definition != null && definition.modifierId == "BracedAssault" && stackTypeStr == "BracedAssaultStacks")
            {
                HandleBracedAssaultStack(eventContext, character, modifierState);
                return;
            }

            int amount = action.parameters.ContainsKey("amount") ? Convert.ToInt32(action.parameters["amount"]) : 1;
            StackSystem.Instance?.AddStacks(stackType, amount);

            // Special handling for Arcane Rebound: When Flow stack is added on spell cast, randomly select bonus type
            if (definition != null && definition.modifierId == "ArcaneRebound_Start" &&
                stackType == StackType.Flow &&
                eventContext != null && eventContext.ContainsKey("data") &&
                eventContext["data"] is SpellData)
            {
                // Randomly select one of three bonus types
                string[] bonusTypes = { "damage", "hits", "elemental" };
                string selectedBonus = bonusTypes[UnityEngine.Random.Range(0, bonusTypes.Length)];

                if (modifierState != null)
                {
                    modifierState["arcaneReboundBonus"] = selectedBonus;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Arcane Rebound: Spell cast detected! Selected bonus type: {selectedBonus} (stored in modifier state)");
                    ArcaneReboundIconManager.ShowBonusIcon(character, selectedBonus);
                }
            }
        }
    }

    /// <summary>
    /// Handle Battle Rhythm stack logic: Track card types played per turn, only grant stack when all three types (Attack, Guard, Skill) are played.
    /// Resets stacks if duplicate type is played before completing the set.
    /// </summary>
    private static void HandleBattleRhythmStack(Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (eventContext == null || modifierState == null)
        {
            UnityEngine.Debug.LogWarning("[ModifierEffectResolver] Battle Rhythm: Missing eventContext or modifierState");
            return;
        }

        // Get the card from event context
        CardDataExtended card = null;
        if (eventContext.ContainsKey("card") && eventContext["card"] is CardDataExtended cardData)
        {
            card = cardData;
        }
        else if (eventContext.ContainsKey("data") && eventContext["data"] is CardDataExtended cardData2)
        {
            card = cardData2;
        }

        if (card == null)
        {
            UnityEngine.Debug.LogWarning("[ModifierEffectResolver] Battle Rhythm: No card found in event context");
            return;
        }

        // Get card type (only Attack, Guard, Skill count for Battle Rhythm)
        CardType cardType = card.GetCardTypeEnum();
        if (cardType != CardType.Attack && cardType != CardType.Guard && cardType != CardType.Skill)
        {
            // Power and Aura cards don't count for Battle Rhythm
            return;
        }

        // Initialize tracking if needed
        if (!modifierState.ContainsKey("battleRhythmPlayedTypes"))
        {
            modifierState["battleRhythmPlayedTypes"] = new HashSet<CardType>();
        }

        HashSet<CardType> playedTypes = modifierState["battleRhythmPlayedTypes"] as HashSet<CardType>;
        if (playedTypes == null)
        {
            playedTypes = new HashSet<CardType>();
            modifierState["battleRhythmPlayedTypes"] = playedTypes;
        }

        // Check if this card type was already played this turn
        if (playedTypes.Contains(cardType))
        {
            // Duplicate type played - reset Battle Rhythm stacks and clear tracking
            StackSystem.Instance?.ClearStacks(StackType.BattleRhythm);
            playedTypes.Clear();
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Battle Rhythm: Duplicate {cardType} played - reset stacks to 0");
            return;
        }

        // Add this card type to the set
        playedTypes.Add(cardType);
        UnityEngine.Debug.Log($"[ModifierEffectResolver] Battle Rhythm: Played {cardType} (tracking: {string.Join(", ", playedTypes)})");

        // Check if all three types are now present
        bool hasAttack = playedTypes.Contains(CardType.Attack);
        bool hasGuard = playedTypes.Contains(CardType.Guard);
        bool hasSkill = playedTypes.Contains(CardType.Skill);

        if (hasAttack && hasGuard && hasSkill)
        {
            // All three types played - grant 1 Battle Rhythm stack
            StackSystem.Instance?.AddStacks(StackType.BattleRhythm, 1);
            int currentStacks = StackSystem.Instance != null ? StackSystem.Instance.GetStacks(StackType.BattleRhythm) : 0;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Battle Rhythm: ✓ All three types played! Gained 1 stack (total: {currentStacks})");

            // Mark that Battle Rhythm was just gained (for Flow of Iron, Pulse of War)
            if (modifierState != null)
            {
                modifierState["battleRhythmJustGained"] = true;
            }

            // Clear tracking for next cycle
            playedTypes.Clear();
        }
    }

    private static void ResolveAddDamageMorePercent(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, AscendancyModifierDefinition definition)
    {
        if (!action.parameters.ContainsKey("percent")) return;

        float percent = Convert.ToSingle(action.parameters["percent"]);

        // Special handling for Arcane Rebound: Only apply if bonus type is "damage"
        if (definition != null && definition.modifierId == "ArcaneRebound_Start")
        {
            if (modifierState != null && modifierState.ContainsKey("arcaneReboundBonus"))
            {
                string bonusType = modifierState["arcaneReboundBonus"].ToString();
                if (bonusType == "damage")
                {
                    // Apply more damage multiplier
                    if (eventContext != null && eventContext.ContainsKey("damage"))
                    {
                        float currentDamage = Convert.ToSingle(eventContext["damage"]);
                        float newDamage = currentDamage * (1f + percent / 100f);
                        eventContext["damage"] = newDamage;
                        UnityEngine.Debug.Log($"[ModifierEffectResolver] Arcane Rebound: Applied {percent}% more damage. {currentDamage} -> {newDamage}");
                    }
                    else
                    {
                        // Store in event context for later application
                        if (eventContext != null)
                        {
                            if (!eventContext.ContainsKey("moreDamagePercent"))
                            {
                                eventContext["moreDamagePercent"] = 0f;
                            }
                            eventContext["moreDamagePercent"] = Convert.ToSingle(eventContext["moreDamagePercent"]) + percent;
                        }
                    }

                    // Remove the bonus icon
                    ArcaneReboundIconManager.RemoveBonusIcon(character);
                    modifierState.Remove("arcaneReboundBonus");
                }
            }
        }
        else
        {
            // Standard more damage application
            if (eventContext != null && eventContext.ContainsKey("damage"))
            {
                float currentDamage = Convert.ToSingle(eventContext["damage"]);
                
                // Check for Braced Assault bonus
                float bracedAssaultBonus = 0f;
                if (modifierState != null && modifierState.ContainsKey("bracedAssaultStacks"))
                {
                    int stacks = Convert.ToInt32(modifierState["bracedAssaultStacks"]);
                    bracedAssaultBonus = stacks * 20f; // 20% per stack
                }
                
                float totalBonus = percent + bracedAssaultBonus;
                float newDamage = currentDamage * (1f + totalBonus / 100f);
                eventContext["damage"] = newDamage;
                
                if (bracedAssaultBonus > 0f)
                {
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Braced Assault: Applied {bracedAssaultBonus}% bonus damage ({Convert.ToInt32(modifierState["bracedAssaultStacks"])} stacks)");
                    modifierState["bracedAssaultStacks"] = 0; // Consume stacks
                }
            }
            else if (eventContext != null)
            {
                if (!eventContext.ContainsKey("moreDamagePercent"))
                {
                    eventContext["moreDamagePercent"] = 0f;
                }
                eventContext["moreDamagePercent"] = Convert.ToSingle(eventContext["moreDamagePercent"]) + percent;
            }
        }
    }

    private static void ResolveAddExtraHit(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, AscendancyModifierDefinition definition)
    {
        // Special handling for Arcane Rebound: Only apply if bonus type is "hits"
        if (definition != null && definition.modifierId == "ArcaneRebound_Start")
        {
            if (modifierState != null && modifierState.ContainsKey("arcaneReboundBonus"))
            {
                string bonusType = modifierState["arcaneReboundBonus"].ToString();
                if (bonusType == "hits")
                {
                    int extraHits = action.parameters.ContainsKey("amount") ? Convert.ToInt32(action.parameters["amount"]) : 1;

                    if (eventContext != null)
                    {
                        if (!eventContext.ContainsKey("extraHits"))
                        {
                            eventContext["extraHits"] = 0;
                        }
                        eventContext["extraHits"] = Convert.ToInt32(eventContext["extraHits"]) + extraHits;
                        UnityEngine.Debug.Log($"[ModifierEffectResolver] Arcane Rebound: Added {extraHits} extra hit(s)");
                    }

                    // Remove the bonus icon
                    ArcaneReboundIconManager.RemoveBonusIcon(character);
                    modifierState.Remove("arcaneReboundBonus");
                }
            }
        }
        else
        {
            // Standard extra hit application
            int extraHits = action.parameters.ContainsKey("amount") ? Convert.ToInt32(action.parameters["amount"]) : 1;
            if (eventContext != null)
            {
                if (!eventContext.ContainsKey("extraHits"))
                {
                    eventContext["extraHits"] = 0;
                }
                eventContext["extraHits"] = Convert.ToInt32(eventContext["extraHits"]) + extraHits;
            }
        }
    }

    private static void ResolveAddElementalDamage(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, AscendancyModifierDefinition definition)
    {
        // Special handling for Arcane Rebound: Only apply if bonus type is "elemental"
        if (definition != null && definition.modifierId == "ArcaneRebound_Start")
        {
            if (modifierState != null && modifierState.ContainsKey("arcaneReboundBonus"))
            {
                string bonusType = modifierState["arcaneReboundBonus"].ToString();
                if (bonusType == "elemental")
                {
                    float elementalDamage = action.parameters.ContainsKey("amount") ? Convert.ToSingle(action.parameters["amount"]) : 50f;

                    if (eventContext != null)
                    {
                        if (!eventContext.ContainsKey("addedElementalDamage"))
                        {
                            eventContext["addedElementalDamage"] = 0f;
                        }
                        eventContext["addedElementalDamage"] = Convert.ToSingle(eventContext["addedElementalDamage"]) + elementalDamage;
                        UnityEngine.Debug.Log($"[ModifierEffectResolver] Arcane Rebound: Added {elementalDamage} elemental damage");
                    }

                    // Remove the bonus icon
                    ArcaneReboundIconManager.RemoveBonusIcon(character);
                    modifierState.Remove("arcaneReboundBonus");
                }
            }
        }
        else
        {
            // Standard elemental damage application
            float elementalDamage = action.parameters.ContainsKey("amount") ? Convert.ToSingle(action.parameters["amount"]) : 0f;
            if (eventContext != null)
            {
                if (!eventContext.ContainsKey("addedElementalDamage"))
                {
                    eventContext["addedElementalDamage"] = 0f;
                }
                eventContext["addedElementalDamage"] = Convert.ToSingle(eventContext["addedElementalDamage"]) + elementalDamage;
            }
        }
    }

    private static void ResolveApplyStatus(ModifierAction action, Dictionary<string, object> eventContext, Character character)
    {
        if (!action.parameters.ContainsKey("statusEffectType") && !action.parameters.ContainsKey("statusType")) return;

        // Support both "statusEffectType" and "statusType" parameter names
        string statusTypeStr = action.parameters.ContainsKey("statusEffectType") 
            ? action.parameters["statusEffectType"].ToString() 
            : action.parameters["statusType"].ToString();
        
        // Special handling for Shock Absorber: Calculate magnitude from damage reduced
        if (statusTypeStr == "Pressure" && action.parameters.ContainsKey("calculateFromDamageReduced") && 
            Convert.ToBoolean(action.parameters["calculateFromDamageReduced"]) && eventContext != null)
        {
            if (eventContext.ContainsKey("damageReduced"))
            {
                float damageReduced = Convert.ToSingle(eventContext["damageReduced"]);
                // Override magnitude with damage reduced amount
                action.parameters["magnitude"] = damageReduced;
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Shock Absorber: Pressure magnitude set to {damageReduced} (from damage reduced)");
            }
        }
        
        // Special handling for Blood Price: Calculate Pressure magnitude from max HP
        if (statusTypeStr == "Pressure" && action.parameters.ContainsKey("calculateFromMaxHP") && 
            Convert.ToBoolean(action.parameters["calculateFromMaxHP"]) && character != null)
        {
            float percent = action.parameters.ContainsKey("magnitude") ? Convert.ToSingle(action.parameters["magnitude"]) : 0.03f;
            float pressureMagnitude = character.maxHealth * percent;
            action.parameters["magnitude"] = pressureMagnitude;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Blood Price: Pressure magnitude set to {pressureMagnitude} ({(percent * 100)}% of {character.maxHealth} max HP)");
        }

        if (Enum.TryParse<StatusEffectType>(statusTypeStr, out StatusEffectType statusType))
        {
            float magnitude = action.parameters.ContainsKey("magnitude") ? Convert.ToSingle(action.parameters["magnitude"]) : 0f;
            int duration = action.parameters.ContainsKey("duration") ? Convert.ToInt32(action.parameters["duration"]) : 1;

            // Special handling for Crumble: Calculate magnitude from damage if damagePercent parameter exists
            if (statusType == StatusEffectType.Crumble && action.parameters.ContainsKey("damagePercent"))
            {
                float damagePercent = Convert.ToSingle(action.parameters["damagePercent"]);

                // Get damage amount from event context
                if (eventContext != null && eventContext.ContainsKey("data") && eventContext["data"] is DamageDealtData damageData)
                {
                    magnitude = damageData.damage * damagePercent;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Crumbling Earth: Calculated Crumble magnitude = {damageData.damage} * {damagePercent} = {magnitude}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[ModifierEffectResolver] Crumbling Earth: Could not get damage amount from event context!");
                }
            }

            StatusEffect effect = new StatusEffect(statusType, statusTypeStr, magnitude, duration);

            // Determine target: Check if this is for an enemy (Crumbling Earth) or player
            StatusEffectManager statusManager = null;

            if (statusType == StatusEffectType.Crumble && eventContext != null && eventContext.ContainsKey("data") && eventContext["data"] is DamageDealtData crumbleData)
            {
                // Apply to enemy target
                if (crumbleData.targetEnemy != null)
                {
                    // Find EnemyCombatDisplay for this enemy
                    EnemyCombatDisplay[] enemyDisplays = GameObject.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                    foreach (var display in enemyDisplays)
                    {
                        if (display != null && display.GetEnemy() == crumbleData.targetEnemy)
                        {
                            statusManager = display.GetStatusEffectManager();
                            break;
                        }
                    }

                    if (statusManager == null)
                    {
                        UnityEngine.Debug.LogWarning($"[ModifierEffectResolver] Could not find StatusEffectManager for enemy: {crumbleData.targetEnemy.enemyName}");
                    }
                }
            }
            else
            {
                // Apply to player character
                var playerDisplay = GameObject.FindFirstObjectByType<PlayerCombatDisplay>();
                if (playerDisplay != null)
                {
                    statusManager = playerDisplay.GetStatusEffectManager();
                }
            }

            if (statusManager != null)
            {
                statusManager.AddStatusEffect(effect);
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Applied status effect: {statusTypeStr} (magnitude: {magnitude}, duration: {duration})");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[ModifierEffectResolver] Could not find StatusEffectManager to apply {statusTypeStr}");
            }
        }
    }

    // Helper methods for other action types
    private static void ResolveRemoveStack(ModifierAction action, Dictionary<string, object> parameters, Character character)
    {
        if (!parameters.ContainsKey("stackType")) return;
        string stackTypeStr = parameters["stackType"].ToString();
        if (Enum.TryParse<StackType>(stackTypeStr, out StackType stackType))
        {
            int amount = parameters.ContainsKey("amount") ? Convert.ToInt32(parameters["amount"]) : 1;
            StackSystem.Instance?.RemoveStacks(stackType, amount);
        }
    }

    private static void ResolveConsumeStack(ModifierAction action, Dictionary<string, object> parameters, Character character)
    {
        if (!parameters.ContainsKey("stackType")) return;
        string stackTypeStr = parameters["stackType"].ToString();
        if (Enum.TryParse<StackType>(stackTypeStr, out StackType stackType))
        {
            int amount = parameters.ContainsKey("amount") ? Convert.ToInt32(parameters["amount"]) : 1;
            // Consume = remove stacks (same as RemoveStacks)
            StackSystem.Instance?.RemoveStacks(stackType, amount);
        }
    }

    private static void ResolveSetStack(ModifierAction action, Dictionary<string, object> parameters, Character character)
    {
        if (!parameters.ContainsKey("stackType")) return;
        string stackTypeStr = parameters["stackType"].ToString();
        if (Enum.TryParse<StackType>(stackTypeStr, out StackType stackType))
        {
            int targetAmount = parameters.ContainsKey("amount") ? Convert.ToInt32(parameters["amount"]) : 0;
            // Set = clear current, then add to target amount
            int current = StackSystem.Instance?.GetStacks(stackType) ?? 0;
            int delta = targetAmount - current;
            if (delta > 0)
            {
                StackSystem.Instance?.AddStacks(stackType, delta);
            }
            else if (delta < 0)
            {
                StackSystem.Instance?.RemoveStacks(stackType, -delta);
            }
        }
    }

    private static void ResolveClearStack(ModifierAction action, Dictionary<string, object> parameters, Character character)
    {
        if (!parameters.ContainsKey("stackType")) return;
        string stackTypeStr = parameters["stackType"].ToString();
        if (Enum.TryParse<StackType>(stackTypeStr, out StackType stackType))
        {
            StackSystem.Instance?.ClearStacks(stackType);
        }
    }

    private static void ResolveModifyMaxStacks(ModifierAction action, Dictionary<string, object> parameters, Character character)
    {
        if (!parameters.ContainsKey("stackType")) return;
        string stackTypeStr = parameters["stackType"].ToString();
        if (Enum.TryParse<StackType>(stackTypeStr, out StackType stackType))
        {
            int bonusMax = parameters.ContainsKey("bonusMax") ? Convert.ToInt32(parameters["bonusMax"]) : 0;
            // Use SetBonusMaxStacks instead of ModifyMaxStacks
            StackSystem.Instance?.SetBonusMaxStacks(stackType, bonusMax);
        }
    }

    private static void ResolveAddFlatDamage(ModifierAction action, Dictionary<string, object> parameters, Dictionary<string, object> eventContext)
    {
        float damage = 0f;
        
        // Check if Flowbreaker damage is already calculated (from Battle Rhythm loss - triggers whenever Battle Rhythm ends)
        if (eventContext != null && eventContext.ContainsKey("flowbreakerTriggered") && 
            Convert.ToBoolean(eventContext["flowbreakerTriggered"]) && eventContext.ContainsKey("flowbreakerDamage"))
        {
            damage = Convert.ToSingle(eventContext["flowbreakerDamage"]);
            bool targetAll = eventContext.ContainsKey("flowbreakerTargetAll") && Convert.ToBoolean(eventContext["flowbreakerTargetAll"]);
            string damageType = eventContext.ContainsKey("flowbreakerDamageType") ? eventContext["flowbreakerDamageType"].ToString() : "Physical";
            
            // Store in event context for AoE damage processing (same format as Bulwark Pulse)
            eventContext["bulwarkPulseDamage"] = damage;
            eventContext["bulwarkPulseTargetAll"] = targetAll;
            eventContext["bulwarkPulseDamageType"] = damageType;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Flowbreaker: Processing AoE damage - {damage} {damageType} damage to all enemies");
            
            // Clear the trigger flag so it doesn't fire multiple times
            eventContext["flowbreakerTriggered"] = false;
        }
        // Check if we need to calculate damage from guard gained (Bulwark Pulse)
        else if (parameters.ContainsKey("calculateFromGuardGained") && Convert.ToBoolean(parameters["calculateFromGuardGained"]) && 
            eventContext != null && eventContext.ContainsKey("guardGained"))
        {
            float guardGained = Convert.ToSingle(eventContext["guardGained"]);
            float damagePercent = parameters.ContainsKey("amount") ? Convert.ToSingle(parameters["amount"]) : 0.1f;
            damage = guardGained * damagePercent;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Bulwark Pulse: Calculated damage from guard gained: {damage} ({(damagePercent * 100)}% of {guardGained})");
        }
        else
        {
            damage = parameters.ContainsKey("amount") ? Convert.ToSingle(parameters["amount"]) : 0f;
        }
        
        if (damage > 0f && eventContext != null)
        {
            // Check if this should target all enemies (if not already set by Flowbreaker)
            bool targetAll = eventContext.ContainsKey("bulwarkPulseTargetAll") && Convert.ToBoolean(eventContext["bulwarkPulseTargetAll"]) ||
                            (parameters.ContainsKey("targetAllEnemies") && Convert.ToBoolean(parameters["targetAllEnemies"]));
            
            if (targetAll && !eventContext.ContainsKey("bulwarkPulseDamage"))
            {
                // Store in event context for AoE damage processing (only if not already set by Flowbreaker)
                eventContext["bulwarkPulseDamage"] = damage;
                eventContext["bulwarkPulseTargetAll"] = true;
                string damageType = parameters.ContainsKey("damageType") ? parameters["damageType"].ToString() : "Physical";
                eventContext["bulwarkPulseDamageType"] = damageType;
            }
            else if (!targetAll && eventContext.ContainsKey("damage"))
            {
                // Add to existing damage
                float currentDamage = Convert.ToSingle(eventContext["damage"]);
                eventContext["damage"] = currentDamage + damage;
            }
            else if (!targetAll)
            {
                eventContext["damage"] = damage;
            }
        }
    }

    private static void ResolveAddPercentDamage(ModifierAction action, Dictionary<string, object> parameters, Dictionary<string, object> eventContext)
    {
        float percent = parameters.ContainsKey("percent") ? Convert.ToSingle(parameters["percent"]) : 0f;
        if (eventContext != null && eventContext.ContainsKey("damage"))
        {
            float currentDamage = Convert.ToSingle(eventContext["damage"]);
            eventContext["damage"] = currentDamage * (1f + percent / 100f);
        }
    }

    private static void ResolveRemoveStatus(ModifierAction action, Dictionary<string, object> parameters, Character character)
    {
        // Implementation for removing status effects
        Debug.LogWarning("[ModifierEffectResolver] RemoveStatus not yet implemented");
    }

    private static void ResolveAddGuard(ModifierAction action, Dictionary<string, object> parameters, Dictionary<string, object> eventContext, Character character)
    {
        if (character == null) return;
        
        float guard = 0f;
        
        // Check if we need to calculate guard from max HP
        if (parameters.ContainsKey("calculateFromMaxHP") && Convert.ToBoolean(parameters["calculateFromMaxHP"]))
        {
            float percent = parameters.ContainsKey("amount") ? Convert.ToSingle(parameters["amount"]) : 0f;
            guard = character.maxHealth * percent;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Calculated guard from max HP: {guard} ({(percent * 100)}% of {character.maxHealth})");
        }
        // Check if we need to calculate from attributes and armour (Immovable Object)
        else if (parameters.ContainsKey("calculateFromAttributes") && Convert.ToBoolean(parameters["calculateFromAttributes"]))
        {
            float percent = parameters.ContainsKey("amount") ? Convert.ToSingle(parameters["amount"]) : 0f;
            
            // Calculate total attributes
            int totalAttributes = character.strength + character.dexterity + character.intelligence;
            
            // Get armour (would need EquipmentManager integration - for now use 0 or a placeholder)
            float armour = 0f;
            var equipmentManager = EquipmentManager.Instance;
            if (equipmentManager != null)
            {
                // Try to get total armour from equipment
                // This would need EquipmentManager to expose armour calculation
                // For now, we'll use attributes only
            }
            
            guard = (totalAttributes + armour) * percent;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Calculated guard from attributes/armour: {guard} ({(percent * 100)}% of {totalAttributes} attributes + {armour} armour)");
        }
        else
        {
            guard = parameters.ContainsKey("amount") ? Convert.ToSingle(parameters["amount"]) : 0f;
        }
        
        if (guard > 0f)
        {
            // Store guard before adding (for Bulwark Pulse)
            float guardBefore = character.currentGuard;
            
            // Use character's AddGuard method which applies effectiveness multipliers
            character.AddGuard(guard);
            
            // Calculate actual guard gained (after effectiveness multipliers)
            float guardGained = character.currentGuard - guardBefore;
            
            // Store in event context for Bulwark Pulse
            if (eventContext != null && guardGained > 0f)
            {
                eventContext["guardGained"] = guardGained;
            }
        }
    }

    private static void ResolveAddMana(ModifierAction action, Dictionary<string, object> parameters, Character character)
    {
        int mana = parameters.ContainsKey("amount") ? Convert.ToInt32(parameters["amount"]) : 0;
        if (character != null)
        {
            character.mana = Mathf.Min(character.mana + mana, character.maxMana);
        }
    }

    private static void ResolveDrawCard(ModifierAction action, Dictionary<string, object> parameters)
    {
        int count = parameters.ContainsKey("amount") ? Convert.ToInt32(parameters["amount"]) : 1;
        CombatDeckManager.Instance?.DrawCards(count);
    }

    private static void ResolveDiscardCard(ModifierAction action, Dictionary<string, object> parameters)
    {
        int count = parameters.ContainsKey("amount") ? Convert.ToInt32(parameters["amount"]) : 1;
        // Implementation would need to integrate with card discard system
        Debug.LogWarning("[ModifierEffectResolver] DiscardCard not yet fully implemented");
    }

    private static void ResolveModifyHealthPercent(ModifierAction action, Dictionary<string, object> parameters, Character character)
    {
        if (character == null) return;
        
        float percent = parameters.ContainsKey("percent") ? Convert.ToSingle(parameters["percent"]) : 0f;
        // Negative percent means lose health (for Unstable Corruption)
        int healthChange = Mathf.RoundToInt(character.currentHealth * percent);
        character.currentHealth = Mathf.Max(0, character.currentHealth - healthChange);
        
        UnityEngine.Debug.Log($"[ModifierEffectResolver] Modified health by {percent * 100}% ({healthChange} HP). Current HP: {character.currentHealth}");
    }

    private static void ResolveMarkCardAsTemporal(ModifierAction action, Dictionary<string, object> eventContext, Character character)
    {
        // Check for chance parameter (15% for Temporal Threads)
        float chance = action.parameters.ContainsKey("chance") ? Convert.ToSingle(action.parameters["chance"]) : 0.15f;
        
        if (UnityEngine.Random.Range(0f, 1f) <= chance)
        {
            // Mark the card as Temporal
            if (eventContext != null && eventContext.ContainsKey("card"))
            {
                var card = eventContext["card"] as CardDataExtended;
                if (card != null)
                {
                    // Add "Temporal" tag to the card permanently
                    if (card.tags == null)
                    {
                        card.tags = new System.Collections.Generic.List<string>();
                    }
                    
                    if (!card.tags.Contains("Temporal"))
                    {
                        card.tags.Add("Temporal");
                        UnityEngine.Debug.Log($"[ModifierEffectResolver] Marked card '{card.cardName}' as Temporal (chance: {chance * 100}%)");
                    }
                    
                    // Also store in event context for immediate use
                    eventContext["isTemporal"] = true;
                }
                else
                {
                    // Fallback: just set event context if card is not CardDataExtended
                    eventContext["isTemporal"] = true;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Marked card as Temporal (chance: {chance * 100}%) - card type not CardDataExtended");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[ModifierEffectResolver] MarkCardAsTemporal: No card in event context");
            }
        }
    }

    private static void ResolveEchoTemporalCards(ModifierAction action, Dictionary<string, object> parameters, Character character)
    {
        // Echo all Temporal cards at turn start
        float echoPower = parameters.ContainsKey("echoPower") ? Convert.ToSingle(parameters["echoPower"]) : 1f;
        bool targetTemporalCards = parameters.ContainsKey("targetTemporalCards") ? Convert.ToBoolean(parameters["targetTemporalCards"]) : true;
        
        if (targetTemporalCards && character != null)
        {
            // This would need integration with the card system to find and echo Temporal cards
            // For now, log that it should happen
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Echoing Temporal cards with {echoPower * 100}% power");
            // TODO: Implement actual card echoing logic
        }
    }

    private static void ResolveTriggerChaosSurge(ModifierAction action, Dictionary<string, object> parameters, Character character, Dictionary<string, object> modifierState, AscendancyModifierDefinition definition)
    {
        if (character == null) return;
        
        // Get parameters
        float damagePercent = parameters.ContainsKey("damagePercent") ? Convert.ToSingle(parameters["damagePercent"]) : 0.5f;
        float loseMaxHpPercent = parameters.ContainsKey("loseMaxHpPercent") ? Convert.ToSingle(parameters["loseMaxHpPercent"]) : 0.05f;
        int gainCorruptionFlow = parameters.ContainsKey("gainCorruptionFlow") ? Convert.ToInt32(parameters["gainCorruptionFlow"]) : 1;
        
        // Deal AoE Chaos damage (would need to integrate with combat system)
        UnityEngine.Debug.Log($"[ModifierEffectResolver] Chaos Surge triggered! Damage: {damagePercent * 100}%, Lose Max HP: {loseMaxHpPercent * 100}%, Gain Corruption Flow: {gainCorruptionFlow}");
        
        // Lose max HP
        int maxHpLoss = Mathf.RoundToInt(character.maxHealth * loseMaxHpPercent);
        character.maxHealth = Mathf.Max(1, character.maxHealth - maxHpLoss);
        character.currentHealth = Mathf.Min(character.currentHealth, character.maxHealth);
        
        // Gain Corruption Flow stacks
        if (gainCorruptionFlow > 0)
        {
            StackSystem.Instance?.AddStacks(StackType.Corruption, gainCorruptionFlow);
        }
    }

    /// <summary>
    /// Modify a character stat (for Tolerance/Guard effectiveness, guard persistence, etc.)
    /// </summary>
    private static void ResolveModifyStat(ModifierAction action, Dictionary<string, object> parameters, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null) return;
        
        if (!parameters.ContainsKey("statName")) return;
        
        string statName = parameters["statName"].ToString();
        
        // Handle different stat modifications
        switch (statName.ToLower())
        {
            case "toleranceeffectiveness":
            case "tolerance_effectiveness":
                if (parameters.ContainsKey("percentIncrease"))
                {
                    float increase = Convert.ToSingle(parameters["percentIncrease"]);
                    character.toleranceEffectivenessPercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Tolerance effectiveness by {increase}% (total: {character.toleranceEffectivenessPercent}%)");
                }
                break;
                
            case "guardeffectiveness":
            case "guard_effectiveness":
                if (parameters.ContainsKey("percentIncrease"))
                {
                    float increase = Convert.ToSingle(parameters["percentIncrease"]);
                    character.guardEffectivenessPercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Guard effectiveness by {increase}% (total: {character.guardEffectivenessPercent}%)");
                }
                break;
                
            case "guardpersistencefraction":
            case "guard_persistence_fraction":
                if (parameters.ContainsKey("setValue"))
                {
                    float value = Convert.ToSingle(parameters["setValue"]);
                    character.guardPersistenceFraction = value;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Set guard persistence fraction to {value} ({value * 100}%)");
                }
                break;
                
            case "maxguardmultiplier":
            case "max_guard_multiplier":
                if (parameters.ContainsKey("setValue"))
                {
                    float multiplier = Convert.ToSingle(parameters["setValue"]);
                    character.maxGuardMultiplier = multiplier;
                    // Update max guard immediately
                    character.maxGuard = character.maxHealth * multiplier;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Set max guard multiplier to {multiplier}x (max guard: {character.maxGuard})");
                }
                break;
                
            case "attackcannotmiss":
            case "attack_cannot_miss":
                character.attackCannotMiss = true;
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Attacks cannot miss enabled");
                break;
                
            case "attackdamageincreasedpercent":
            case "attack_damage_increased_percent":
                if (parameters.ContainsKey("percentIncrease"))
                {
                    float increase = Convert.ToSingle(parameters["percentIncrease"]);
                    character.attackDamageIncreasedPercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Attack damage by {increase}% (total: {character.attackDamageIncreasedPercent}%)");
                }
                break;
                
            case "crumblemagnitudeincreasedpercent":
            case "crumble_magnitude_increased_percent":
                if (parameters.ContainsKey("percentIncrease"))
                {
                    float increase = Convert.ToSingle(parameters["percentIncrease"]);
                    character.crumbleMagnitudeIncreasedPercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Crumble Magnitude by {increase}% (total: {character.crumbleMagnitudeIncreasedPercent}%)");
                }
                break;
                
            case "crumblemagnitudemorepercent":
            case "crumble_magnitude_more_percent":
                if (parameters.ContainsKey("percentIncrease"))
                {
                    float increase = Convert.ToSingle(parameters["percentIncrease"]);
                    character.crumbleMagnitudeMorePercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] More Crumble Magnitude by {increase}% (total: {character.crumbleMagnitudeMorePercent}%)");
                }
                break;
                
            case "crumbledurationincreasedpercent":
            case "crumble_duration_increased_percent":
                if (parameters.ContainsKey("percentIncrease"))
                {
                    float increase = Convert.ToSingle(parameters["percentIncrease"]);
                    character.crumbleDurationIncreasedPercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Crumble Duration by {increase}% (total: {character.crumbleDurationIncreasedPercent}%)");
                }
                break;
                
            case "crumbleexplosionhealpercent":
            case "crumble_explosion_heal_percent":
                if (parameters.ContainsKey("setValue"))
                {
                    float value = Convert.ToSingle(parameters["setValue"]);
                    character.crumbleExplosionHealPercent = value;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Set Crumble explosion heal to {(value * 100)}%");
                }
                break;
                
            case "maxmanapercentincrease":
            case "max_mana_percent_increase":
                if (parameters.ContainsKey("percentIncrease"))
                {
                    float increase = Convert.ToSingle(parameters["percentIncrease"]);
                    character.maxManaPercentIncrease += increase;
                    // Apply to max mana immediately
                    int oldMaxMana = character.maxMana;
                    character.maxMana = Mathf.RoundToInt(character.maxMana * (1f + increase / 100f));
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Max Mana by {increase}% ({oldMaxMana} -> {character.maxMana})");
                }
                break;
                
            case "immunetobleed":
            case "immune_to_bleed":
                character.immuneToBleed = true;
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Immune to Bleed enabled");
                break;
                
            case "immunetoignite":
            case "immune_to_ignite":
                character.immuneToIgnite = true;
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Immune to Ignite enabled");
                break;
                
            // Profane Vessel stats
            case "chaosresistance":
            case "chaos_resistance":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.chaosResistancePercent += increase;
                    // Also update damageStats for actual resistance calculation
                    if (character.damageStats != null)
                    {
                        character.damageStats.AddResistance(DamageType.Chaos, increase);
                    }
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Chaos Resistance by {increase}% (total: {character.chaosResistancePercent}%)");
                }
                break;
                
            case "reducedselfinflicteddamagepercent":
            case "reduced_self_inflicted_damage_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.reducedSelfInflictedDamagePercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Reduced Self-Inflicted Damage by {increase}% (total: {character.reducedSelfInflictedDamagePercent}%)");
                }
                break;
                
            case "increasedselfinflicteddamagepercent":
            case "increased_self_inflicted_damage_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.increasedSelfInflictedDamagePercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Self-Inflicted Damage by {increase}% (total: {character.increasedSelfInflictedDamagePercent}%)");
                }
                break;
                
            case "increasedchaosdamagepercent":
            case "increased_chaos_damage_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.increasedChaosDamagePercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Chaos Damage by {increase}% (total: {character.increasedChaosDamagePercent}%)");
                }
                break;
                
            case "corruptiongainratepercent":
            case "corruption_gain_rate_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.corruptionGainRatePercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Corruption Gain Rate by {increase}% (total: {character.corruptionGainRatePercent}%)");
                }
                break;
                
            // Archanum Bladeweaver stats
            case "increasedelementaldamagewithattackspercent":
            case "increased_elemental_damage_with_attacks_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.increasedElementalDamageWithAttacksPercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Elemental Damage with Attacks by {increase}% (total: {character.increasedElementalDamageWithAttacksPercent}%)");
                }
                break;
                
            case "increasedattackspeedpercent":
            case "increased_attack_speed_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.attackSpeedIncreasedPercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Attack Speed by {increase}% (total: {character.attackSpeedIncreasedPercent}%)");
                }
                break;
                
            case "increasedcastspeedpercent":
            case "increased_cast_speed_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.castSpeedIncreasedPercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Cast Speed by {increase}% (total: {character.castSpeedIncreasedPercent}%)");
                }
                break;
                
            case "weaponattacksscalewithspellpowerpercent":
            case "weapon_attacks_scale_with_spell_power_percent":
                if (parameters.ContainsKey("setValue"))
                {
                    float value = Convert.ToSingle(parameters["setValue"]);
                    character.weaponAttacksScaleWithSpellPowerPercent = value;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Set Weapon Attacks Scale with Spell Power to {value}%");
                }
                break;
                
            case "spellsgaindamagefromweaponpercent":
            case "spells_gain_damage_from_weapon_percent":
                if (parameters.ContainsKey("setValue"))
                {
                    float value = Convert.ToSingle(parameters["setValue"]);
                    character.spellsGainDamageFromWeaponPercent = value;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Set Spells Gain Damage from Weapon to {value}%");
                }
                break;
                
            // Temporal Savant stats
            case "temporalcarddrawchancepercent":
            case "temporal_card_draw_chance_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.temporalCardDrawChancePercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Temporal Card Draw Chance by {increase}% (total: {character.temporalCardDrawChancePercent}%)");
                }
                break;
                
            case "increasedenergyshieldpercent":
            case "increased_energy_shield_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.increasedEnergyShieldPercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Energy Shield by {increase}% (total: {character.increasedEnergyShieldPercent}%)");
                }
                break;
                
            case "increasedtemporalcardeffectivenesspercent":
            case "increased_temporal_card_effectiveness_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.increasedTemporalCardEffectivenessPercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Temporal Card Effectiveness by {increase}% (total: {character.increasedTemporalCardEffectivenessPercent}%)");
                }
                break;
                
            case "temporalcardeffectbonuspercent":
            case "temporal_card_effect_bonus_percent":
                if (parameters.ContainsKey("setValue"))
                {
                    float value = Convert.ToSingle(parameters["setValue"]);
                    character.temporalCardEffectBonusPercent = value;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Set Temporal Card Effect Bonus to {value}%");
                }
                break;
                
            case "temporalcardmanacostincrease":
            case "temporal_card_mana_cost_increase":
                if (parameters.ContainsKey("setValue"))
                {
                    int value = Convert.ToInt32(parameters["setValue"]);
                    character.temporalCardManaCostIncrease = value;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Set Temporal Card Mana Cost Increase to {value}");
                }
                break;
                
            // General damage stats
            case "increaseddamagepercent":
            case "increased_damage_percent":
                if (parameters.ContainsKey("increasePercent"))
                {
                    float increase = Convert.ToSingle(parameters["increasePercent"]);
                    character.increasedDamagePercent += increase;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Damage by {increase}% (total: {character.increasedDamagePercent}%)");
                }
                break;
                
            case "criticalstrikechance":
            case "critical_strike_chance":
                if (parameters.ContainsKey("amount"))
                {
                    float amount = Convert.ToSingle(parameters["amount"]);
                    character.criticalStrikeChance += amount;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Increased Critical Strike Chance by {amount}% (total: {character.criticalStrikeChance}%)");
                }
                break;
                
            default:
                UnityEngine.Debug.LogWarning($"[ModifierEffectResolver] Unknown stat name: {statName}");
                break;
        }
    }

    /// <summary>
    /// Modify damage multiplier (for damage reduction like Shock Absorber)
    /// </summary>
    private static void ResolveModifyDamageMultiplier(ModifierAction action, Dictionary<string, object> parameters, Dictionary<string, object> eventContext, Character character)
    {
        if (character == null || eventContext == null) return;
        
        // Check if this is for damage reduction (Shock Absorber)
        if (parameters.ContainsKey("damageReductionPercent"))
        {
            float reductionPercent = Convert.ToSingle(parameters["damageReductionPercent"]);
            
            // Get current damage from event context
            if (eventContext.ContainsKey("damage"))
            {
                float currentDamage = Convert.ToSingle(eventContext["damage"]);
                float damageReduced = currentDamage * reductionPercent;
                float newDamage = currentDamage - damageReduced;
                
                eventContext["damage"] = newDamage;
                eventContext["damageReduced"] = damageReduced; // Store for Pressure status
                
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Reduced damage by {reductionPercent * 100}%: {currentDamage} -> {newDamage} (reduced: {damageReduced})");
            }
        }
    }

    /// <summary>
    /// Handle Braced Assault: Track guard cards played and store stacks for damage bonus
    /// </summary>
    private static void HandleBracedAssaultStack(Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (modifierState == null) return;
        
        // Initialize tracking if needed
        if (!modifierState.ContainsKey("bracedAssaultStacks"))
        {
            modifierState["bracedAssaultStacks"] = 0;
        }
        
        int currentStacks = Convert.ToInt32(modifierState["bracedAssaultStacks"]);
        int maxStacks = 5; // Max 100% damage (5 stacks * 20% each)
        
        // Add one stack (capped at max)
        int newStacks = Mathf.Min(currentStacks + 1, maxStacks);
        modifierState["bracedAssaultStacks"] = newStacks;
        
        UnityEngine.Debug.Log($"[ModifierEffectResolver] Braced Assault: Guard card played. Stacks: {currentStacks} -> {newStacks} (max {maxStacks})");
    }

    /// <summary>
    /// Handle Bulwark Pulse: Deal damage when guard is gained
    /// </summary>
    private static void HandleBulwarkPulse(ModifierAction action, Dictionary<string, object> eventContext, Character character)
    {
        if (eventContext == null || character == null) return;
        
        // Get guard amount gained from event context
        float guardGained = 0f;
        if (eventContext.ContainsKey("guardGained"))
        {
            guardGained = Convert.ToSingle(eventContext["guardGained"]);
        }
        
        if (guardGained > 0f)
        {
            float damagePercent = action.parameters.ContainsKey("amount") ? Convert.ToSingle(action.parameters["amount"]) : 0.1f;
            float damage = guardGained * damagePercent;
            
            // Store in event context for combat system to process as AoE damage
            if (!eventContext.ContainsKey("bulwarkPulseDamage"))
            {
                eventContext["bulwarkPulseDamage"] = damage;
                eventContext["bulwarkPulseTargetAll"] = true;
                eventContext["bulwarkPulseDamageType"] = "Physical";
                
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Bulwark Pulse: Dealt {damage} Physical damage to all enemies ({(damagePercent * 100)}% of {guardGained} guard gained)");
            }
        }
    }

    /// <summary>
    /// Handle Final Offering: Double damage and trigger all Crumble stacks at ≤25% HP
    /// </summary>
    private static void HandleFinalOfferingDamage(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || eventContext == null) return;
        
        // Check if character is at ≤25% HP
        float healthPercent = (float)character.currentHealth / character.maxHealth;
        if (healthPercent <= 0.25f)
        {
            // Double damage (already handled by the +100% more damage in the modifier)
            // But we also need to trigger all Crumble stacks
            if (!eventContext.ContainsKey("triggerAllCrumbleStacks"))
            {
                eventContext["triggerAllCrumbleStacks"] = true;
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Final Offering: Character at {healthPercent * 100}% HP - triggering all Crumble stacks");
            }
        }
    }

    /// <summary>
    /// Handle Trembling Echo: Repeat first attack if target has Crumble
    /// </summary>
    private static void HandleTremblingEcho(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (modifierState == null || eventContext == null || character == null) return;
        
        // Initialize tracking for first attack per turn
        if (!modifierState.ContainsKey("tremblingEchoUsedThisTurn"))
        {
            modifierState["tremblingEchoUsedThisTurn"] = false;
        }
        
        bool alreadyUsed = Convert.ToBoolean(modifierState["tremblingEchoUsedThisTurn"]);
        if (alreadyUsed) return; // Only first attack per turn
        
        // Check if target has Crumble status
        // This would need to check the enemy's status effects
        // For now, we'll store in event context for the combat system to process
        if (!eventContext.ContainsKey("tremblingEchoRepeat"))
        {
            eventContext["tremblingEchoRepeat"] = true;
            eventContext["tremblingEchoDamagePercent"] = 0.5f; // 50% effect
            modifierState["tremblingEchoUsedThisTurn"] = true;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Trembling Echo: First attack this turn will repeat for 50% effect if target is Crumbling");
        }
    }

    /// <summary>
    /// Handle Blood Price: Apply Pressure status when attacking
    /// </summary>
    private static void HandleBloodPrice(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || eventContext == null) return;
        
        // Pressure magnitude is 3% of max HP (already calculated in ResolveApplyStatus)
        // This method is called after the status is applied, so we can do any additional processing here
        UnityEngine.Debug.Log($"[ModifierEffectResolver] Blood Price: Pressure applied to player from attack");
    }

    // ========== DISCIPLE OF WAR MODIFIER HANDLERS ==========

    /// <summary>
    /// Handle Damage And Rhythm: Calculate crit chance based on Battle Rhythm stacks (per 2 stacks)
    /// </summary>
    private static void HandleDamageAndRhythm(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || StackSystem.Instance == null) return;

        int battleRhythmStacks = StackSystem.Instance.GetStacks(StackType.BattleRhythm);
        int critChanceBonus = (battleRhythmStacks / 2) * 1; // +1% per 2 stacks

        if (eventContext != null && eventContext.ContainsKey("criticalStrikeChance"))
        {
            float currentCrit = Convert.ToSingle(eventContext["criticalStrikeChance"]);
            eventContext["criticalStrikeChance"] = currentCrit + critChanceBonus;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Damage And Rhythm: Added {critChanceBonus}% crit chance (from {battleRhythmStacks} Battle Rhythm stacks)");
        }
    }

    /// <summary>
    /// Handle Damage and Guard Balance: Conditional damage/guard effectiveness based on guard percentage
    /// </summary>
    private static void HandleDamageAndGuardBalance(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null) return;

        float guardPercent = character.maxGuard > 0 ? character.currentGuard / character.maxGuard : 0f;
        float threshold = 0.5f; // 50% guard threshold

        if (guardPercent > threshold)
        {
            // Above 50% guard: +5% increased damage
            if (eventContext != null && eventContext.ContainsKey("damage"))
            {
                float currentDamage = Convert.ToSingle(eventContext["damage"]);
                eventContext["damage"] = currentDamage * 1.05f;
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Damage and Guard Balance: +5% damage (guard: {guardPercent * 100}%)");
            }
        }
        else
        {
            // Below 50% guard: +5% guard effectiveness
            character.guardEffectivenessPercent += 5f;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Damage and Guard Balance: +5% guard effectiveness (guard: {guardPercent * 100}%)");
        }
    }

    /// <summary>
    /// Handle Flow of Iron: Track Flow stacks, trigger at 10, reset when Battle Rhythm lost
    /// </summary>
    private static void HandleFlowOfIron(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || StackSystem.Instance == null || modifierState == null) return;

        // Check if Battle Rhythm was just gained
        if (modifierState.ContainsKey("battleRhythmJustGained") && Convert.ToBoolean(modifierState["battleRhythmJustGained"]))
        {
            // Gain 2 Flow stacks
            StackSystem.Instance?.AddStacks(StackType.Flow, 2);
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Flow of Iron: Gained 2 Flow stacks (total: {StackSystem.Instance.GetStacks(StackType.Flow)})");
            modifierState["battleRhythmJustGained"] = false; // Clear flag
        }

        // Check if Battle Rhythm was lost (Flow resets)
        int battleRhythmStacks = StackSystem.Instance.GetStacks(StackType.BattleRhythm);
        if (battleRhythmStacks == 0)
        {
            StackSystem.Instance?.ClearStacks(StackType.Flow);
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Flow of Iron: Battle Rhythm lost - Flow stacks reset");
        }
    }

    /// <summary>
    /// Handle Guarded Motion: Track guard usage, add extra hit to next attack
    /// </summary>
    private static void HandleGuardedMotion(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null) return;

        // When guard is gained, mark next attack for extra hit
        if (eventContext != null && eventContext.ContainsKey("guardGained"))
        {
            modifierState["guardedMotionNextAttackExtraHit"] = true;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Guarded Motion: Next attack will gain +1 hit");
        }

        // When attack is used, check if extra hit should be applied
        if (eventContext != null && eventContext.ContainsKey("cardType") && 
            modifierState.ContainsKey("guardedMotionNextAttackExtraHit") && 
            Convert.ToBoolean(modifierState["guardedMotionNextAttackExtraHit"]))
        {
            CardType cardType = (CardType)eventContext["cardType"];
            if (cardType == CardType.Attack)
            {
                // Add extra hit
                if (!eventContext.ContainsKey("extraHits"))
                {
                    eventContext["extraHits"] = 0;
                }
                int extraHits = Convert.ToInt32(eventContext["extraHits"]);
                eventContext["extraHits"] = extraHits + 1;
                eventContext["extraHitDamagePercent"] = 0.4f; // 40% damage

                // Clear the flag
                modifierState["guardedMotionNextAttackExtraHit"] = false;
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Guarded Motion: Applied +1 extra hit (40% damage)");
            }
        }
    }

    /// <summary>
    /// Handle Tempered Resolve: Track Battle Rhythm loss, apply damage reduction, fade per turn
    /// </summary>
    private static void HandleTemperedResolve(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null) return;

        // Track damage reduction in modifier state
        if (!modifierState.ContainsKey("temperedResolveDamageReduction"))
        {
            modifierState["temperedResolveDamageReduction"] = 0f;
        }

        float currentReduction = Convert.ToSingle(modifierState["temperedResolveDamageReduction"]);

        // On turn start, fade 5% per turn
        if (eventContext != null && eventContext.ContainsKey("eventType") && 
            eventContext["eventType"] is ModifierEventType eventType && eventType == ModifierEventType.OnTurnStart)
        {
            currentReduction = Mathf.Max(0f, currentReduction - 5f);
            modifierState["temperedResolveDamageReduction"] = currentReduction;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Tempered Resolve: Damage reduction faded to {currentReduction}%");
        }

        // Apply damage reduction to damage events
        if (eventContext != null && eventContext.ContainsKey("damage") && currentReduction > 0f)
        {
            float currentDamage = Convert.ToSingle(eventContext["damage"]);
            float reducedDamage = currentDamage * (1f - currentReduction / 100f);
            eventContext["damage"] = reducedDamage;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Tempered Resolve: Reduced damage by {currentReduction}% ({currentDamage} -> {reducedDamage})");
        }
    }

    /// <summary>
    /// Handle Conductors Strike: Track different card types played, apply damage bonus, crit bonus at 3 types
    /// </summary>
    private static void HandleConductorsStrike(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null || eventContext == null) return;

        // Track card types played this turn
        if (!modifierState.ContainsKey("conductorsStrikeCardTypes"))
        {
            modifierState["conductorsStrikeCardTypes"] = new HashSet<CardType>();
        }

        HashSet<CardType> cardTypes = modifierState["conductorsStrikeCardTypes"] as HashSet<CardType>;
        if (cardTypes == null)
        {
            cardTypes = new HashSet<CardType>();
            modifierState["conductorsStrikeCardTypes"] = cardTypes;
        }

        // Get card type from event context
        if (eventContext.ContainsKey("cardType"))
        {
            CardType cardType = (CardType)eventContext["cardType"];
            if (!cardTypes.Contains(cardType))
            {
                cardTypes.Add(cardType);
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Conductors Strike: Tracked card type {cardType} (total: {cardTypes.Count} types)");
            }
        }

        // Apply damage bonus per card type on attack
        if (eventContext.ContainsKey("cardType") && (CardType)eventContext["cardType"] == CardType.Attack)
        {
            int uniqueTypes = cardTypes.Count;
            float damageBonus = uniqueTypes * 5f; // +5% per type

            if (eventContext.ContainsKey("damage"))
            {
                float currentDamage = Convert.ToSingle(eventContext["damage"]);
                eventContext["damage"] = currentDamage * (1f + damageBonus / 100f);
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Conductors Strike: +{damageBonus}% damage ({uniqueTypes} card types played)");
            }

            // At 3 types, critical strikes deal +100% more damage
            if (uniqueTypes >= 3 && eventContext.ContainsKey("isCritical") && Convert.ToBoolean(eventContext["isCritical"]))
            {
                if (eventContext.ContainsKey("damage"))
                {
                    float currentDamage = Convert.ToSingle(eventContext["damage"]);
                    eventContext["damage"] = currentDamage * 2f; // Double damage on crit
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Conductors Strike: Critical strike at 3+ types - doubled damage!");
                }
            }
        }
    }

    /// <summary>
    /// Handle Tactical Transference: Track Battle Rhythm at turn end, apply Momentum next turn
    /// </summary>
    private static void HandleTacticalTransference(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || StackSystem.Instance == null || modifierState == null) return;

        // On turn end, check if Battle Rhythm is active
        if (eventContext != null && eventContext.ContainsKey("eventType") && 
            eventContext["eventType"] is ModifierEventType eventType && eventType == ModifierEventType.OnTurnEnd)
        {
            int battleRhythmStacks = StackSystem.Instance.GetStacks(StackType.BattleRhythm);
            if (battleRhythmStacks > 0)
            {
                modifierState["tacticalTransferenceMomentumNextTurn"] = 2; // 2 Momentum stacks next turn
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Tactical Transference: Will grant 2 Momentum stacks next turn");
            }
        }

        // On turn start, apply Momentum if flagged
        if (eventContext != null && eventContext.ContainsKey("eventType") && 
            eventContext["eventType"] is ModifierEventType eventType2 && eventType2 == ModifierEventType.OnTurnStart)
        {
            if (modifierState.ContainsKey("tacticalTransferenceMomentumNextTurn"))
            {
                int momentumAmount = Convert.ToInt32(modifierState["tacticalTransferenceMomentumNextTurn"]);
                StackSystem.Instance?.AddStacks(StackType.Momentum, momentumAmount);
                modifierState.Remove("tacticalTransferenceMomentumNextTurn");
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Tactical Transference: Granted {momentumAmount} Momentum stacks");
            }
        }
    }

    /// <summary>
    /// Handle Flowbreaker: Trigger whenever Battle Rhythm ends (regardless of how), deal AoE damage for 30% of total Guard
    /// </summary>
    private static void HandleFlowbreaker(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || eventContext == null) return;

        // Check if Flowbreaker was triggered (Battle Rhythm ended - can happen from turn end, duplicate card, etc.)
        if (eventContext.ContainsKey("flowbreakerTriggered") && Convert.ToBoolean(eventContext["flowbreakerTriggered"]))
        {
            // Damage is already calculated in the Battle Rhythm loss handler (30% of total Guard)
            // The damage is stored in eventContext["flowbreakerDamage"] and will be processed by ResolveAddFlatDamage
            if (eventContext.ContainsKey("flowbreakerDamage"))
            {
                float damage = Convert.ToSingle(eventContext["flowbreakerDamage"]);
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Flowbreaker: Triggered! Dealing {damage} AoE damage to all enemies (30% of {character.currentGuard} Guard)");
            }
            
            // Clear the trigger flag so it doesn't fire multiple times
            eventContext["flowbreakerTriggered"] = false;
        }
    }

    /// <summary>
    /// Handle Martial Refrain: Track Battle Rhythm reset, duplicate first card next turn
    /// </summary>
    private static void HandleMartialRefrain(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null) return;

        // Track if Battle Rhythm was reset last turn
        if (eventContext != null && eventContext.ContainsKey("eventType") && 
            eventContext["eventType"] is ModifierEventType eventType && eventType == ModifierEventType.OnTurnStart)
        {
            if (modifierState.ContainsKey("battleRhythmResetLastTurn") && Convert.ToBoolean(modifierState["battleRhythmResetLastTurn"]))
            {
                modifierState["martialRefrainDuplicateFirstCard"] = true;
                modifierState["battleRhythmResetLastTurn"] = false;
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Martial Refrain: Will duplicate first card this turn");
            }
        }

        // On first card played, duplicate it
        if (eventContext != null && modifierState.ContainsKey("martialRefrainDuplicateFirstCard") && 
            Convert.ToBoolean(modifierState["martialRefrainDuplicateFirstCard"]))
        {
            eventContext["martialRefrainDuplicate"] = true;
            eventContext["martialRefrainEffectiveness"] = 0.5f; // 50% effectiveness

            // If it's an Attack, add crit chance
            if (eventContext.ContainsKey("cardType") && (CardType)eventContext["cardType"] == CardType.Attack)
            {
                if (!eventContext.ContainsKey("criticalStrikeChance"))
                {
                    eventContext["criticalStrikeChance"] = 0f;
                }
                float currentCrit = Convert.ToSingle(eventContext["criticalStrikeChance"]);
                eventContext["criticalStrikeChance"] = currentCrit + 30f; // +30% crit chance
            }

            modifierState["martialRefrainDuplicateFirstCard"] = false;
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Martial Refrain: Duplicated first card at 50% effectiveness");
        }
    }

    /// <summary>
    /// Handle Second Beat: Track cards played, echo every 3rd card
    /// </summary>
    private static void HandleSecondBeat(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null || eventContext == null) return;

        // Track cards played this turn
        if (!modifierState.ContainsKey("secondBeatCardsPlayed"))
        {
            modifierState["secondBeatCardsPlayed"] = 0;
        }

        int cardsPlayed = Convert.ToInt32(modifierState["secondBeatCardsPlayed"]);
        cardsPlayed++;
        modifierState["secondBeatCardsPlayed"] = cardsPlayed;

        // Every 3rd card (3, 6, 9, etc.)
        if (cardsPlayed % 3 == 0)
        {
            eventContext["secondBeatEcho"] = true;
            eventContext["secondBeatEffectiveness"] = 0.3f; // 30% effectiveness

            // Get card type
            if (eventContext.ContainsKey("cardType"))
            {
                CardType cardType = (CardType)eventContext["cardType"];
                if (cardType == CardType.Attack)
                {
                    eventContext["secondBeatAttackRepeatSameTarget"] = true;
                }
                else if (cardType == CardType.Guard)
                {
                    eventContext["secondBeatGuardGainPercent"] = 0.3f; // 30% of guard value
                }
            }

            UnityEngine.Debug.Log($"[ModifierEffectResolver] Second Beat: Echoing {cardsPlayed}th card at 30% effectiveness");
        }
    }

    /// <summary>
    /// Handle Pulse of War: Track Battle Rhythm gains, deal AoE damage, count pulses
    /// </summary>
    private static void HandlePulseOfWar(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || StackSystem.Instance == null || modifierState == null) return;

        // Track pulse count
        if (!modifierState.ContainsKey("pulseOfWarCount"))
        {
            modifierState["pulseOfWarCount"] = 0;
        }

        // Check if Battle Rhythm was just gained
        if (modifierState.ContainsKey("battleRhythmJustGained") && Convert.ToBoolean(modifierState["battleRhythmJustGained"]))
        {
            // Deal AoE damage for 10% of Strength
            float damage = character.strength * 0.1f;

            if (eventContext != null)
            {
                eventContext["pulseOfWarDamage"] = damage;
                eventContext["pulseOfWarTargetAll"] = true;
                eventContext["pulseOfWarDamageType"] = "Physical";
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Pulse of War: Dealt {damage} AoE damage (10% of {character.strength} Strength)");
            }

            // Increment pulse count
            int pulseCount = Convert.ToInt32(modifierState["pulseOfWarCount"]);
            pulseCount++;
            modifierState["pulseOfWarCount"] = pulseCount;

            // Every 5 pulses, grant +1 Flow and restore 10% Mana
            if (pulseCount % 5 == 0)
            {
                StackSystem.Instance?.AddStacks(StackType.Flow, 1);
                float manaRestore = character.maxMana * 0.1f;
                character.mana = Mathf.Min(character.mana + Mathf.RoundToInt(manaRestore), character.maxMana);
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Pulse of War: 5th pulse - granted 1 Flow stack and restored {Mathf.RoundToInt(manaRestore)} Mana");
            }
        }
    }

    // ========== PROFANE VESSEL HANDLERS ==========

    /// <summary>
    /// Feast of Pain: Heal 10% of Missing HP when killing an enemy with 10+ Corruption
    /// </summary>
    private static void HandleFeastOfPain(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || StackSystem.Instance == null) return;

        int corruptionStacks = StackSystem.Instance.GetStacks(StackType.Corruption);
        if (corruptionStacks >= 10)
        {
            float missingHP = character.maxHealth - character.currentHealth;
            float healAmount = missingHP * 0.1f;
            character.currentHealth = Mathf.Min(character.currentHealth + Mathf.RoundToInt(healAmount), character.maxHealth);
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Feast of Pain: Healed {healAmount} HP (10% of {missingHP} missing HP)");
        }
    }

    /// <summary>
    /// Searing Insight: Lose 2% Max HP when playing Fire/Chaos card, gain +8% More Damage (stacks to 5)
    /// </summary>
    private static void HandleSearingInsight(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null || eventContext == null) return;

        // Check if this is a Fire or Chaos card
        CardDataExtended card = null;
        if (eventContext.ContainsKey("card") && eventContext["card"] is CardDataExtended cardData)
        {
            card = cardData;
        }
        else if (eventContext.ContainsKey("data") && eventContext["data"] is CardDataExtended cardData2)
        {
            card = cardData2;
        }

        if (card == null) return;

        // Check if card is Fire or Chaos (would need to check card's damage type)
        // For now, assume this is handled by the condition in the modifier definition
        // Apply damage bonus based on stacks
        int stacks = StackSystem.Instance?.GetStacks(StackType.SearingInsightStacks) ?? 0;
        if (stacks > 0)
        {
            float damageBonus = stacks * 8f; // +8% per stack
            if (eventContext.ContainsKey("damageMultiplier"))
            {
                eventContext["damageMultiplier"] = Convert.ToSingle(eventContext["damageMultiplier"]) * (1f + damageBonus / 100f);
            }
            else
            {
                eventContext["damageMultiplier"] = 1f + (damageBonus / 100f);
            }
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Searing Insight: Applied {damageBonus}% more damage from {stacks} stacks");
        }
    }

    /// <summary>
    /// Pact of the Harvester: 40% more damage per 20 Corruption, Profane Harvest at 50 Corruption
    /// </summary>
    private static void HandlePactOfTheHarvester(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || StackSystem.Instance == null || eventContext == null) return;

        int corruptionStacks = StackSystem.Instance.GetStacks(StackType.Corruption);
        
        // Apply damage bonus per 20 Corruption
        int thresholds = corruptionStacks / 20;
        if (thresholds > 0)
        {
            float damageBonus = thresholds * 40f; // +40% per 20 stacks
            if (eventContext.ContainsKey("damageMultiplier"))
            {
                eventContext["damageMultiplier"] = Convert.ToSingle(eventContext["damageMultiplier"]) * (1f + damageBonus / 100f);
            }
            else
            {
                eventContext["damageMultiplier"] = 1f + (damageBonus / 100f);
            }
            UnityEngine.Debug.Log($"[ModifierEffectResolver] Pact of the Harvester: Applied {damageBonus}% more damage from {thresholds} thresholds ({corruptionStacks} Corruption)");
        }

        // Check for Profane Harvest trigger (handled in ResolveAction when Corruption reaches 50)
        if (eventContext.ContainsKey("triggerProfaneHarvest") && Convert.ToBoolean(eventContext["triggerProfaneHarvest"]))
        {
            HandleProfaneHarvest(action, eventContext, character, modifierState);
        }
    }

    /// <summary>
    /// Profane Harvest: Release all Corruption, Heal 1% Missing HP per Corruption, +20% Max Mana for 1 turn, Next 3 cards deal extra Chaos damage with +30% more base damage
    /// </summary>
    private static void HandleProfaneHarvest(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || StackSystem.Instance == null) return;

        int corruptionStacks = StackSystem.Instance.GetStacks(StackType.Corruption);
        
        // Release all Corruption
        StackSystem.Instance.ClearStacks(StackType.Corruption);
        
        // Heal 1% of Missing HP per Corruption
        float missingHP = character.maxHealth - character.currentHealth;
        float healAmount = missingHP * 0.01f * corruptionStacks;
        character.currentHealth = Mathf.Min(character.currentHealth + Mathf.RoundToInt(healAmount), character.maxHealth);
        
        // Gain +20% Max Mana for 1 turn (store in modifierState)
        if (modifierState != null)
        {
            modifierState["profaneHarvestManaBonus"] = character.maxMana * 0.2f;
            modifierState["profaneHarvestManaTurns"] = 1;
        }
        
        // Mark next 3 cards for extra Chaos damage
        if (modifierState != null)
        {
            modifierState["profaneHarvestCardsRemaining"] = 3;
            modifierState["profaneHarvestChaosDamageBonus"] = 0.3f; // +30% more base damage
        }
        
        UnityEngine.Debug.Log($"[ModifierEffectResolver] Profane Harvest: Released {corruptionStacks} Corruption, healed {healAmount} HP, gained +20% Max Mana for 1 turn, next 3 cards deal extra Chaos damage");
    }

    /// <summary>
    /// Abyssal Bargain: At 20 Corruption thresholds: Recover 25% Mana and draw 1 card
    /// </summary>
    private static void HandleAbyssalBargain(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || StackSystem.Instance == null) return;

        int corruptionStacks = StackSystem.Instance.GetStacks(StackType.Corruption);
        
        // Check if we just crossed a 20-stack threshold
        if (modifierState != null && modifierState.ContainsKey("lastAbyssalBargainThreshold"))
        {
            int lastThreshold = Convert.ToInt32(modifierState["lastAbyssalBargainThreshold"]);
            int currentThreshold = corruptionStacks / 20;
            
            if (currentThreshold > lastThreshold)
            {
                // Recover 25% Mana
                int manaRecovered = Mathf.RoundToInt(character.maxMana * 0.25f);
                character.mana = Mathf.Min(character.mana + manaRecovered, character.maxMana);
                
                // Draw 1 card (handled by DrawCard action)
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Abyssal Bargain: Recovered {manaRecovered} Mana and drew 1 card at {corruptionStacks} Corruption");
                
                modifierState["lastAbyssalBargainThreshold"] = currentThreshold;
            }
        }
        else if (modifierState != null)
        {
            modifierState["lastAbyssalBargainThreshold"] = corruptionStacks / 20;
        }
    }

    /// <summary>
    /// Tormented Knowledge: When losing life from own effects, next card has +30% More effect
    /// </summary>
    private static void HandleTormentedKnowledge(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null) return;

        // Mark next card for bonus
        if (modifierState.ContainsKey("tormentedKnowledgeNextCardBonus"))
        {
            modifierState["tormentedKnowledgeNextCardBonus"] = true;
            UnityEngine.Debug.Log("[ModifierEffectResolver] Tormented Knowledge: Next card will have +30% More effect");
        }
        else
        {
            modifierState["tormentedKnowledgeNextCardBonus"] = true;
        }
    }

    /// <summary>
    /// Descent of Thought: When losing life from own effects: Randomly gain +1 to STR/DEX/INT (33/33/33)
    /// </summary>
    private static void HandleDescentOfThought(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null) return;

        // Randomly choose attribute (33/33/33 chance)
        int roll = UnityEngine.Random.Range(0, 3);
        switch (roll)
        {
            case 0:
                character.strength += 1;
                UnityEngine.Debug.Log("[ModifierEffectResolver] Descent of Thought: Gained +1 Strength");
                break;
            case 1:
                character.dexterity += 1;
                UnityEngine.Debug.Log("[ModifierEffectResolver] Descent of Thought: Gained +1 Dexterity");
                break;
            case 2:
                character.intelligence += 1;
                UnityEngine.Debug.Log("[ModifierEffectResolver] Descent of Thought: Gained +1 Intelligence");
                break;
        }
        
        // Recalculate derived stats
        character.CalculateDerivedStats();
    }

    /// <summary>
    /// Blood Pact: Start of combat: Lose 10% Max HP, gain +25% Damage and +1 Draw for 3 turns
    /// </summary>
    private static void HandleBloodPact(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null) return;

        // Lose 10% Max HP (handled by ModifyHealthPercent action)
        // Apply +25% Damage and +1 Draw for 3 turns (handled by status effects)
        // This is already handled by the modifier definition's status effects
        UnityEngine.Debug.Log("[ModifierEffectResolver] Blood Pact: Applied +25% Damage and +1 Draw for 3 turns");
    }

    /// <summary>
    /// Echo of Agony: After Chaos Surge triggers, next card is duplicated as Chaos damage
    /// </summary>
    private static void HandleEchoOfAgony(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null) return;

        // Mark next card for duplication
        modifierState["echoOfAgonyNextCardDuplicate"] = true;
        modifierState["echoOfAgonyAsChaos"] = true;
        UnityEngine.Debug.Log("[ModifierEffectResolver] Echo of Agony: Next card will be duplicated as Chaos damage");
    }

    /// <summary>
    /// Chaotic Pendulum: At wave start, gain alternating Chaos/Fire buff (+25% more damage), playing that element grants +1 Corruption
    /// </summary>
    private static void HandleChaoticPendulum(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null || eventContext == null) return;

        // Initialize pendulum state if needed
        if (!modifierState.ContainsKey("chaoticPendulumCurrentElement"))
        {
            // Start with Chaos
            modifierState["chaoticPendulumCurrentElement"] = "Chaos";
        }

        // Toggle element each wave
        string currentElement = modifierState["chaoticPendulumCurrentElement"].ToString();
        string nextElement = currentElement == "Chaos" ? "Fire" : "Chaos";
        modifierState["chaoticPendulumCurrentElement"] = nextElement;

        // Apply +25% more damage for current element (stored in modifierState for damage calculation)
        modifierState["chaoticPendulumDamageBonus"] = 0.25f;
        modifierState["chaoticPendulumElement"] = nextElement;

        UnityEngine.Debug.Log($"[ModifierEffectResolver] Chaotic Pendulum: Wave started, current element is {nextElement} (+25% more damage)");

        // Check if played card matches pendulum element (grants +1 Corruption)
        CardDataExtended card = null;
        if (eventContext.ContainsKey("card") && eventContext["card"] is CardDataExtended cardData)
        {
            card = cardData;
        }

        if (card != null)
        {
            // Check if card's damage type matches pendulum element
            // This would need to check the card's damage type
            // For now, we'll handle this in the OnCardPlayed event
        }
    }

    // ========== ARCANUM BLADEWEAVER HANDLERS ==========

    /// <summary>
    /// Mirrorsteel Guard: Start with 3 stacks, consume 1 when hit (40% damage reduction per stack), refresh 1 when playing Guard after Spell
    /// </summary>
    private static void HandleMirrorsteelGuard(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || StackSystem.Instance == null || eventContext == null) return;

        // On damage taken: Consume 1 stack and apply damage reduction
        if (eventContext.ContainsKey("eventType") && (ModifierEventType)eventContext["eventType"] == ModifierEventType.OnDamageTaken)
        {
            int stacks = StackSystem.Instance.GetStacks(StackType.Mirrorsteel);
            if (stacks > 0)
            {
                // Consume 1 stack
                StackSystem.Instance.RemoveStacks(StackType.Mirrorsteel, 1);
                
                // Apply 40% damage reduction per stack (before consumption)
                float reductionPercent = stacks * 40f;
                if (eventContext.ContainsKey("damage"))
                {
                    float currentDamage = Convert.ToSingle(eventContext["damage"]);
                    float reducedDamage = currentDamage * (1f - reductionPercent / 100f);
                    eventContext["damage"] = reducedDamage;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Mirrorsteel Guard: Reduced damage by {reductionPercent}% ({currentDamage} -> {reducedDamage}) using {stacks} stacks");
                }
            }
        }
    }

    /// <summary>
    /// Prismatic Arsenal: When you Attack after casting a Spell, convert 50% of weapon damage to that spell's element
    /// </summary>
    private static void HandlePrismaticArsenal(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null || eventContext == null) return;

        // Check if last action was a Spell and current action is an Attack
        if (modifierState.ContainsKey("lastActionWasSpell") && Convert.ToBoolean(modifierState["lastActionWasSpell"]) &&
            eventContext.ContainsKey("cardType") && (CardType)eventContext["cardType"] == CardType.Attack)
        {
            // Get last spell's element from modifierState
            if (modifierState.ContainsKey("lastSpellElement"))
            {
                string spellElement = modifierState["lastSpellElement"].ToString();
                
                // Convert 50% of weapon damage to spell's element
                if (eventContext.ContainsKey("damage"))
                {
                    float weaponDamage = Convert.ToSingle(eventContext["damage"]);
                    float convertedDamage = weaponDamage * 0.5f;
                    
                    // Store conversion info in event context
                    eventContext["prismaticArsenalConvertedDamage"] = convertedDamage;
                    eventContext["prismaticArsenalElement"] = spellElement;
                    eventContext["prismaticArsenalOriginalDamage"] = weaponDamage * 0.5f; // Remaining physical
                    
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Prismatic Arsenal: Converted 50% of weapon damage ({convertedDamage}) to {spellElement}");
                }
            }
            
            // Clear the flag
            modifierState["lastActionWasSpell"] = false;
        }
    }

    /// <summary>
    /// Twin Arts Invocation: Every 3rd Attack casts your last Spell, Every 3rd Spell performs your last Attack
    /// </summary>
    private static void HandleTwinArtsInvocation(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null || eventContext == null) return;

        // Track attacks
        if (eventContext.ContainsKey("cardType") && (CardType)eventContext["cardType"] == CardType.Attack)
        {
            int currentCount = StackSystem.Instance?.GetStacks(StackType.TwinArtsAttackCount) ?? 0;
            int targetCount = currentCount + 1;
            // Set stacks by clearing and adding
            StackSystem.Instance?.ClearStacks(StackType.TwinArtsAttackCount);
            StackSystem.Instance?.AddStacks(StackType.TwinArtsAttackCount, targetCount);
            int attackCount = targetCount;
            
            // Every 3rd attack
            if (attackCount % 3 == 0 && modifierState.ContainsKey("lastSpellCard"))
            {
                // Cast last spell
                eventContext["twinArtsCastLastSpell"] = true;
                UnityEngine.Debug.Log("[ModifierEffectResolver] Twin Arts Invocation: 3rd attack - casting last spell");
            }
        }
        
        // Track spells - check if card has "Spell" tag instead of CardType.Spell
        bool isSpell = false;
        if (eventContext.ContainsKey("card") && eventContext["card"] is CardDataExtended cardData)
        {
            isSpell = cardData.tags != null && cardData.tags.Contains("Spell");
        }
        
        if (isSpell)
        {
            int currentCount = StackSystem.Instance?.GetStacks(StackType.TwinArtsSpellCount) ?? 0;
            int targetCount = currentCount + 1;
            // Set stacks by clearing and adding
            StackSystem.Instance?.ClearStacks(StackType.TwinArtsSpellCount);
            StackSystem.Instance?.AddStacks(StackType.TwinArtsSpellCount, targetCount);
            int spellCount = targetCount;
            
            // Store last spell
            if (eventContext.ContainsKey("card") && eventContext["card"] is CardDataExtended card)
            {
                modifierState["lastSpellCard"] = card;
            }
            
            // Every 3rd spell
            if (spellCount % 3 == 0 && modifierState.ContainsKey("lastAttackCard"))
            {
                // Perform last attack
                eventContext["twinArtsPerformLastAttack"] = true;
                UnityEngine.Debug.Log("[ModifierEffectResolver] Twin Arts Invocation: 3rd spell - performing last attack");
            }
        }
        
        // Store last attack
        if (eventContext.ContainsKey("cardType") && (CardType)eventContext["cardType"] == CardType.Attack &&
            eventContext.ContainsKey("card") && eventContext["card"] is CardDataExtended attackCard)
        {
            modifierState["lastAttackCard"] = attackCard;
        }
    }

    /// <summary>
    /// Flux Weaving: Whenever you alternate actions (Attack → Spell or Spell → Attack): Gain +15% More Damage and +15% More Guard for your next action
    /// </summary>
    private static void HandleFluxWeaving(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null || eventContext == null) return;

        // Track last action type
        if (!modifierState.ContainsKey("fluxWeavingLastAction"))
        {
            modifierState["fluxWeavingLastAction"] = "None";
        }

        string lastAction = modifierState["fluxWeavingLastAction"].ToString();
        string currentAction = "None";

        if (eventContext.ContainsKey("cardType"))
        {
            CardType cardType = (CardType)eventContext["cardType"];
            if (cardType == CardType.Attack)
            {
                currentAction = "Attack";
            }
            // Check if card is a spell by checking tags
            else if (eventContext.ContainsKey("card") && eventContext["card"] is CardDataExtended cardData && 
                     cardData.tags != null && cardData.tags.Contains("Spell"))
            {
                currentAction = "Spell";
            }
        }

        // Check for alternation
        bool isAlternating = (lastAction == "Attack" && currentAction == "Spell") || 
                            (lastAction == "Spell" && currentAction == "Attack");

        if (isAlternating)
        {
            // Apply +15% More Damage
            if (eventContext.ContainsKey("damageMultiplier"))
            {
                eventContext["damageMultiplier"] = Convert.ToSingle(eventContext["damageMultiplier"]) * 1.15f;
            }
            else
            {
                eventContext["damageMultiplier"] = 1.15f;
            }

            // Apply +15% More Guard (if this is a Guard card)
            if (eventContext.ContainsKey("cardType") && (CardType)eventContext["cardType"] == CardType.Guard)
            {
                if (eventContext.ContainsKey("guardMultiplier"))
                {
                    eventContext["guardMultiplier"] = Convert.ToSingle(eventContext["guardMultiplier"]) * 1.15f;
                }
                else
                {
                    eventContext["guardMultiplier"] = 1.15f;
                }
            }

            UnityEngine.Debug.Log($"[ModifierEffectResolver] Flux Weaving: Alternated {lastAction} → {currentAction}, applied +15% More Damage and Guard");
        }

        // Update last action
        if (currentAction != "None")
        {
            modifierState["fluxWeavingLastAction"] = currentAction;
        }
    }

    /// <summary>
    /// Blades Ascend Spells Break: Every 4 cards you play in a turn (Attack or Spell): Your next damaging card is considered both an Attack and a Spell
    /// </summary>
    private static void HandleBladesAscendSpellsBreak(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null || eventContext == null) return;

        // Track cards played this turn
        int currentCount = StackSystem.Instance?.GetStacks(StackType.BladesAscendCardCount) ?? 0;
        int cardCount = currentCount + 1;
        // Set stacks by clearing and adding
        StackSystem.Instance?.ClearStacks(StackType.BladesAscendCardCount);
        StackSystem.Instance?.AddStacks(StackType.BladesAscendCardCount, cardCount);

        // Every 4th card
        if (cardCount % 4 == 0)
        {
            // Mark next damaging card as hybrid
            modifierState["bladesAscendNextCardHybrid"] = true;
            UnityEngine.Debug.Log("[ModifierEffectResolver] Blades Ascend Spells Break: 4th card played, next damaging card will scale with both weapon and spell damage");
        }

        // On turn start: Reset counter
        if (eventContext.ContainsKey("eventType") && (ModifierEventType)eventContext["eventType"] == ModifierEventType.OnTurnStart)
        {
            StackSystem.Instance?.ClearStacks(StackType.BladesAscendCardCount);
        }
    }

    /// <summary>
    /// Blades of Conduction: Weapon attacks scale with 20% of Spell Power, Spells gain damage equal to 20% of weapon damage
    /// </summary>
    private static void HandleBladesOfConduction(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || eventContext == null) return;

        // This is handled statically via character stats, but we can add dynamic scaling here if needed
        // The modifier definition sets the stat values, and the damage calculation system should use them
        UnityEngine.Debug.Log("[ModifierEffectResolver] Blades of Conduction: Active - weapon attacks scale with 20% Spell Power, spells gain 20% weapon damage");
    }

    /// <summary>
    /// Elemental Branding: Apply effects based on selected subnode (Frost Brand, Storm Brand, or Flame Brand)
    /// </summary>
    private static void HandleElementalBranding(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null || eventContext == null) return;

        // Get selected subnode from progression (would need to check CharacterAscendancyProgress)
        // For now, we'll check modifierState for the selected brand
        if (!modifierState.ContainsKey("elementalBrandingSelected"))
        {
            // Default to Flame Brand if not set
            modifierState["elementalBrandingSelected"] = "FlameBrand";
        }

        string selectedBrand = modifierState["elementalBrandingSelected"].ToString();

        // Apply brand-specific effects
        switch (selectedBrand)
        {
            case "FrostBrand":
                // Every 3rd attack releases a cold wave that applies "Weak" to all enemies
                if (eventContext.ContainsKey("cardType") && (CardType)eventContext["cardType"] == CardType.Attack)
                {
                    int currentCount = StackSystem.Instance?.GetStacks(StackType.FrostBrandAttackCount) ?? 0;
                    int attackCount = currentCount + 1;
                    // Set stacks by clearing and adding
                    StackSystem.Instance?.ClearStacks(StackType.FrostBrandAttackCount);
                    StackSystem.Instance?.AddStacks(StackType.FrostBrandAttackCount, attackCount);
                    
                    if (attackCount % 3 == 0)
                    {
                        eventContext["frostBrandApplyWeak"] = true;
                        eventContext["frostBrandTargetAll"] = true;
                        UnityEngine.Debug.Log("[ModifierEffectResolver] Elemental Branding (Frost Brand): 3rd attack - applying Weak to all enemies");
                    }
                }
                break;
                
            case "StormBrand":
                // Every 3rd attack releases a lightning wave (similar to Frost Brand)
                if (eventContext.ContainsKey("cardType") && (CardType)eventContext["cardType"] == CardType.Attack)
                {
                    int currentCount = StackSystem.Instance?.GetStacks(StackType.StormBrandAttackCount) ?? 0;
                    int attackCount = currentCount + 1;
                    // Set stacks by clearing and adding
                    StackSystem.Instance?.ClearStacks(StackType.StormBrandAttackCount);
                    StackSystem.Instance?.AddStacks(StackType.StormBrandAttackCount, attackCount);
                    
                    if (attackCount % 3 == 0)
                    {
                        eventContext["stormBrandLightningWave"] = true;
                        eventContext["stormBrandTargetAll"] = true;
                        UnityEngine.Debug.Log("[ModifierEffectResolver] Elemental Branding (Storm Brand): 3rd attack - releasing lightning wave");
                    }
                }
                break;
                
            case "FlameBrand":
                // Attacks mark enemies with a burning sigil for 3s, detonates on expiry/death dealing Fire Spell damage
                if (eventContext.ContainsKey("cardType") && (CardType)eventContext["cardType"] == CardType.Attack)
                {
                    eventContext["flameBrandApplySigil"] = true;
                    eventContext["flameBrandSigilDuration"] = 3;
                    UnityEngine.Debug.Log("[ModifierEffectResolver] Elemental Branding (Flame Brand): Attack applied burning sigil");
                }
                break;
        }
    }

    // ========== TEMPORAL SAVANT HANDLERS ==========

    /// <summary>
    /// Future Echo: The first card you play each turn automatically repeats itself after 2 turns with 75% power
    /// </summary>
    private static void HandleFutureEcho(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null || eventContext == null) return;

        // Track if this is the first card of the turn
        if (!modifierState.ContainsKey("futureEchoCardsPlayedThisTurn"))
        {
            modifierState["futureEchoCardsPlayedThisTurn"] = 0;
        }

        int cardsPlayed = Convert.ToInt32(modifierState["futureEchoCardsPlayedThisTurn"]);
        
        if (cardsPlayed == 0) // First card of the turn
        {
            CardDataExtended card = null;
            if (eventContext.ContainsKey("card") && eventContext["card"] is CardDataExtended cardData)
            {
                card = cardData;
            }

            if (card != null)
            {
                // Queue the card to echo after 2 turns
                if (!modifierState.ContainsKey("futureEchoQueue"))
                {
                    modifierState["futureEchoQueue"] = new List<FutureEchoEntry>();
                }

                List<FutureEchoEntry> queue = modifierState["futureEchoQueue"] as List<FutureEchoEntry>;
                queue.Add(new FutureEchoEntry { card = card, turnsRemaining = 2, powerPercent = 0.75f });
                
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Future Echo: Queued '{card.cardName}' to echo after 2 turns at 75% power");
            }
        }

        modifierState["futureEchoCardsPlayedThisTurn"] = cardsPlayed + 1;
    }

    /// <summary>
    /// Future Echo: Process turn end to decrement counters and trigger echoes
    /// </summary>
    private static void HandleFutureEchoTurnEnd(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null) return;

        // Decrement turn counters and trigger echoes
        if (modifierState.ContainsKey("futureEchoQueue"))
        {
            List<FutureEchoEntry> queue = modifierState["futureEchoQueue"] as List<FutureEchoEntry>;
            if (queue != null)
            {
                for (int i = queue.Count - 1; i >= 0; i--)
                {
                    queue[i].turnsRemaining--;
                    if (queue[i].turnsRemaining <= 0)
                    {
                        // Trigger echo
                        eventContext["futureEchoTrigger"] = true;
                        eventContext["futureEchoCard"] = queue[i].card;
                        eventContext["futureEchoPower"] = queue[i].powerPercent;
                        queue.RemoveAt(i);
                        UnityEngine.Debug.Log($"[ModifierEffectResolver] Future Echo: Triggering echo at {queue[i].powerPercent * 100}% power");
                    }
                }
            }
        }

        // Reset cards played counter
        modifierState["futureEchoCardsPlayedThisTurn"] = 0;
    }

    /// <summary>
    /// Hourglass Paradox: If you spend No mana on your turn, gain +2 Draw, +10% increased Mana next turn, and your first card costs 0
    /// </summary>
    private static void HandleHourglassParadox(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null) return;

        // On turn end: Check if no mana was spent
        if (eventContext != null && eventContext.ContainsKey("eventType") && 
            (ModifierEventType)eventContext["eventType"] == ModifierEventType.OnTurnEnd)
        {
            if (modifierState.ContainsKey("hourglassParadoxManaSpentThisTurn"))
            {
                int manaSpent = Convert.ToInt32(modifierState["hourglassParadoxManaSpentThisTurn"]);
                
                if (manaSpent == 0)
                {
                    // Gain +2 Draw next turn
                    if (!modifierState.ContainsKey("hourglassParadoxDrawBonus"))
                    {
                        modifierState["hourglassParadoxDrawBonus"] = 2;
                    }
                    else
                    {
                        modifierState["hourglassParadoxDrawBonus"] = Convert.ToInt32(modifierState["hourglassParadoxDrawBonus"]) + 2;
                    }

                    // Gain +10% increased Mana next turn
                    if (!modifierState.ContainsKey("hourglassParadoxManaBonus"))
                    {
                        modifierState["hourglassParadoxManaBonus"] = 0.1f;
                    }

                    // First card costs 0 next turn
                    modifierState["hourglassParadoxFirstCardFree"] = true;

                    UnityEngine.Debug.Log("[ModifierEffectResolver] Hourglass Paradox: No mana spent this turn - will gain +2 Draw, +10% Mana, and first card free next turn");
                }
            }

            // Reset mana tracking
            modifierState["hourglassParadoxManaSpentThisTurn"] = 0;
        }

        // On turn start: Apply bonuses
        if (eventContext != null && eventContext.ContainsKey("eventType") && 
            (ModifierEventType)eventContext["eventType"] == ModifierEventType.OnTurnStart)
        {
            if (modifierState.ContainsKey("hourglassParadoxDrawBonus"))
            {
                int drawBonus = Convert.ToInt32(modifierState["hourglassParadoxDrawBonus"]);
                // Draw cards (handled by DrawCard action or combat system)
                eventContext["hourglassParadoxDrawCards"] = drawBonus;
                modifierState["hourglassParadoxDrawBonus"] = 0;
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Hourglass Paradox: Drawing {drawBonus} extra cards");
            }

            if (modifierState.ContainsKey("hourglassParadoxManaBonus"))
            {
                float manaBonus = Convert.ToSingle(modifierState["hourglassParadoxManaBonus"]);
                character.maxManaPercentIncrease += manaBonus * 100f; // Convert to percent
                modifierState["hourglassParadoxManaBonus"] = 0f;
                UnityEngine.Debug.Log($"[ModifierEffectResolver] Hourglass Paradox: Gained +10% increased Mana");
            }
        }

        // Track mana spent
        if (eventContext != null && eventContext.ContainsKey("manaSpent"))
        {
            int manaSpent = Convert.ToInt32(eventContext["manaSpent"]);
            if (modifierState.ContainsKey("hourglassParadoxManaSpentThisTurn"))
            {
                modifierState["hourglassParadoxManaSpentThisTurn"] = Convert.ToInt32(modifierState["hourglassParadoxManaSpentThisTurn"]) + manaSpent;
            }
            else
            {
                modifierState["hourglassParadoxManaSpentThisTurn"] = manaSpent;
            }
        }
    }

    /// <summary>
    /// Borrowed Power: Temporal cards gain +25% effect but cost +1 more
    /// </summary>
    private static void HandleBorrowedPower(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || eventContext == null) return;

        // Check if card is Temporal
        CardDataExtended card = null;
        if (eventContext.ContainsKey("card") && eventContext["card"] is CardDataExtended cardData)
        {
            card = cardData;
        }

        // Check if card is Temporal - either from event context flag or tags
        bool isTemporal = false;
        if (eventContext != null && eventContext.ContainsKey("isTemporal"))
        {
            isTemporal = Convert.ToBoolean(eventContext["isTemporal"]);
        }
        else if (card != null && card.tags != null)
        {
            isTemporal = card.tags.Contains("Temporal");
        }
        
        if (card != null && isTemporal)
        {
            // Apply +25% effect
            if (eventContext.ContainsKey("damageMultiplier"))
            {
                eventContext["damageMultiplier"] = Convert.ToSingle(eventContext["damageMultiplier"]) * 1.25f;
            }
            else
            {
                eventContext["damageMultiplier"] = 1.25f;
            }

            // Increase mana cost by +1 (handled by mana cost calculation system)
            if (eventContext.ContainsKey("manaCost"))
            {
                eventContext["manaCost"] = Convert.ToInt32(eventContext["manaCost"]) + 1;
            }
            else
            {
                eventContext["manaCost"] = 1; // Base cost increase
            }

            UnityEngine.Debug.Log("[ModifierEffectResolver] Borrowed Power: Temporal card gains +25% effect but costs +1 more");
        }
    }

    /// <summary>
    /// Suspended Moment: Damage over time effects tick twice as fast for 2 turns after you use a Delayed card
    /// </summary>
    private static void HandleSuspendedMoment(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || modifierState == null) return;

        // Check if card is Delayed (would need to check card properties)
        // For now, assume this is handled by the condition in the modifier definition
        // Apply DoT tick speed multiplier
        if (modifierState.ContainsKey("suspendedMomentActive"))
        {
            bool isActive = Convert.ToBoolean(modifierState["suspendedMomentActive"]);
            if (isActive)
            {
                // DoT effects tick twice as fast (handled by status effect system)
                eventContext["dotTickSpeedMultiplier"] = 2f;
                UnityEngine.Debug.Log("[ModifierEffectResolver] Suspended Moment: DoT effects tick twice as fast");
            }
        }
    }

    /// <summary>
    /// Echoing Will: Temporal cards gain +10% base power per card echoed this combat (80% Cap)
    /// </summary>
    private static void HandleEchoingWill(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || StackSystem.Instance == null || eventContext == null) return;

        // Get echo count
        int echoCount = StackSystem.Instance.GetStacks(StackType.EchoingWillEchoCount);
        
        // Calculate power bonus (capped at 80%)
        float powerBonus = Mathf.Min(echoCount * 10f, 80f);

        // Check if card is Temporal
        CardDataExtended card = null;
        if (eventContext.ContainsKey("card") && eventContext["card"] is CardDataExtended cardData)
        {
            card = cardData;
        }

        // Check if card is Temporal - either from event context flag or tags
        bool isTemporal = false;
        if (eventContext != null && eventContext.ContainsKey("isTemporal"))
        {
            isTemporal = Convert.ToBoolean(eventContext["isTemporal"]);
        }
        else if (card != null && card.tags != null)
        {
            isTemporal = card.tags.Contains("Temporal");
        }
        
        if (card != null && isTemporal && powerBonus > 0f)
        {
            // Apply power bonus
            if (eventContext.ContainsKey("damageMultiplier"))
            {
                eventContext["damageMultiplier"] = Convert.ToSingle(eventContext["damageMultiplier"]) * (1f + powerBonus / 100f);
            }
            else
            {
                eventContext["damageMultiplier"] = 1f + (powerBonus / 100f);
            }

            UnityEngine.Debug.Log($"[ModifierEffectResolver] Echoing Will: Temporal card gains +{powerBonus}% base power from {echoCount} echoes");
        }
    }

    /// <summary>
    /// Chrono Collapse: When an enemy dies, all their debuffs spread to a random enemy
    /// </summary>
    private static void HandleChronoCollapse(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        if (character == null || eventContext == null) return;

        // On enemy killed: Spread debuffs to random enemy
        if (eventContext.ContainsKey("eventType") && 
            ((ModifierEventType)eventContext["eventType"] == ModifierEventType.OnEnemyKilled || 
             (ModifierEventType)eventContext["eventType"] == ModifierEventType.OnEnemyDefeated))
        {
            if (eventContext.ContainsKey("killedEnemy"))
            {
                Enemy killedEnemy = eventContext["killedEnemy"] as Enemy;
                if (killedEnemy != null)
                {
                    // Mark for debuff spreading
                    eventContext["chronoCollapseSpreadDebuffs"] = true;
                    eventContext["chronoCollapseSourceEnemy"] = killedEnemy;
                    UnityEngine.Debug.Log($"[ModifierEffectResolver] Chrono Collapse: Enemy '{killedEnemy.enemyName}' died - spreading debuffs to random enemy");
                }
            }
        }
    }
}

/// <summary>
/// Helper class for Future Echo queue entries
/// </summary>
public class FutureEchoEntry
{
    public CardDataExtended card;
    public int turnsRemaining;
    public float powerPercent;
}

/// <summary>
/// Data structure for damage dealt events
/// </summary>
public class DamageDealtData
{
    public float damage;
    public Enemy targetEnemy;
    public DamageType damageType;
    public bool isCritical;

    public DamageDealtData(float dmg, Enemy target, DamageType type, bool crit = false)
    {
        damage = dmg;
        targetEnemy = target;
        damageType = type;
        isCritical = crit;
    }
}

/// <summary>
/// Data structure for spell cast events
/// </summary>
public class SpellData
{
    public string spellName;
    public float manaCost;
    public DamageType damageType;

    public SpellData(string name, float cost, DamageType type)
    {
        spellName = name;
        manaCost = cost;
        damageType = type;
    }
}

