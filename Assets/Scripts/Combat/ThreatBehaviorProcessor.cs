using System;
using UnityEngine;

public static class ThreatBehaviorProcessor
{
    public static void OnIntentGenerated(Enemy enemy, ref EnemyIntentEntry entry)
    {
        if (enemy == null) return;
        NormalizeThreatsByBinding(ref entry);
        ApplyCharging(enemy, ref entry);
    }

    public static bool OnTurnStart(Enemy enemy)
    {
        if (enemy == null || enemy.intentQueue == null || enemy.intentQueue.IsEmpty)
            return false;

        bool changed = false;
        if (HasThreat(enemy, ThreatWord.Primed))
        {
            changed |= TryTriggerPrimed(enemy);
        }
        if (HasThreat(enemy, ThreatWord.Escalating))
        {
            enemy.escalatingTurns = Mathf.Max(0, enemy.escalatingTurns + 1);
        }
        for (int i = 0; i < enemy.intentQueue.Count; i++)
        {
            var entry = enemy.intentQueue.All[i];
            if (ShouldApply(entry, ThreatWord.Escalating, ThreatHook.OnTurnStart))
            {
                int bonus = Math.Max(0, enemy.escalatingDamagePerTurn);
                float percent = Mathf.Max(0f, enemy.escalatingDamagePercentPerTurn);
                if (bonus > 0 || percent > 0f)
                {
                    if (entry.IsAbility)
                    {
                        if (percent > 0f)
                            entry.AbilityValue = Mathf.RoundToInt(entry.AbilityValue * (1f + percent));
                        entry.AbilityValue += bonus;
                    }
                    else if (entry.Type == EnemyIntent.Attack)
                    {
                        if (percent > 0f)
                            entry.Damage = Mathf.RoundToInt(entry.Damage * (1f + percent));
                        entry.Damage += bonus;
                    }
                    enemy.intentQueue.SetAt(i, entry);
                    changed = true;
                }
            }
        }

        if (HasThreat(enemy, ThreatWord.Escalating) && enemy.escalatingAoEDamagePerTurn > 0)
        {
            int aoeDamage = Mathf.RoundToInt(enemy.escalatingAoEDamagePerTurn * Mathf.Max(1, enemy.escalatingTurns));
            if (aoeDamage > 0)
            {
                var cm = CharacterManager.Instance;
                if (cm != null)
                    cm.TakeDamage(aoeDamage);

                var playerDisplay = UnityEngine.Object.FindFirstObjectByType<PlayerCombatDisplay>();
                var floating = UnityEngine.Object.FindFirstObjectByType<FloatingDamageManager>();
                if (playerDisplay != null && floating != null)
                    floating.ShowDamage(aoeDamage, false, playerDisplay.transform);
            }
        }

        if (changed)
        {
            enemy.SyncFromQueue();
            enemy.NotifyIntentChanged();
        }

        return changed;
    }

    public static bool OnDamaged(Enemy enemy, float incomingDamage, float preGuard)
    {
        if (enemy == null || enemy.intentQueue == null)
            return false;

        if (!HasThreat(enemy, ThreatWord.Retaliating))
            return false;

        if (!IsDefensiveEnemy(enemy))
            return false;

        if (preGuard <= 0f)
            return false;

        // Convert incoming damage into guard (10% default).
        float guardGain = incomingDamage * Mathf.Max(0f, enemy.retaliateGuardGainPercent);
        if (guardGain > 0f)
            enemy.AddGuard(guardGain);

        if (enemy.currentGuard <= 0f)
            return false;

        // Thorns damage based on current guard (50% default).
        float thornsDamage = enemy.currentGuard * Mathf.Max(0f, enemy.retaliateThornsPercent);
        int finalThorns = Mathf.RoundToInt(thornsDamage);
        if (finalThorns <= 0)
            return false;

        var cm = CharacterManager.Instance;
        if (cm != null)
            cm.TakeDamage(finalThorns);

        var playerDisplay = UnityEngine.Object.FindFirstObjectByType<PlayerCombatDisplay>();
        var floating = UnityEngine.Object.FindFirstObjectByType<FloatingDamageManager>();
        if (playerDisplay != null && floating != null)
            floating.ShowDamage(finalThorns, false, playerDisplay.transform);

        return true;
    }

    public static void OnExecuteIntent(Enemy enemy, EnemyIntentEntry entry)
    {
        if (enemy == null)
            return;

        // Reserved for Suppressing/Leeching/Converting hooks.
    }

    private static bool TryTriggerPrimed(Enemy enemy)
    {
        if (enemy == null || enemy.primedTriggered)
            return false;

        bool conditionMet = false;
        switch (enemy.primedTriggerType)
        {
            case PrimedTriggerType.PlayerStatusStacks:
            {
                var playerDisplay = UnityEngine.Object.FindFirstObjectByType<PlayerCombatDisplay>();
                if (playerDisplay != null)
                {
                    var status = playerDisplay.GetStatusEffectManager();
                    if (status != null)
                    {
                        float total = status.GetTotalMagnitude(enemy.primedStatusType);
                        conditionMet = total >= enemy.primedStatusThreshold;
                    }
                }
                break;
            }
            case PrimedTriggerType.EnemyHealthPercent:
            {
                conditionMet = enemy.GetHealthPercentage() <= Mathf.Clamp01(enemy.primedHealthThreshold);
                break;
            }
        }

        if (!conditionMet)
            return false;

        int damage = Mathf.RoundToInt(enemy.GetAttackDamage() * Mathf.Max(1f, enemy.primedDamageMultiplier));
        var primedEntry = new EnemyIntentEntry(
            EnemyIntent.Attack,
            damage,
            0,
            ThreatTier.Major,
            ThreatWord.Primed,
            ThreatWord.None);

        enemy.intentQueue.InsertAt(0, primedEntry);
        enemy.primedTriggered = true;
        enemy.SyncFromQueue();
        enemy.NotifyIntentChanged();
        FlashPrimed(enemy);
        return true;
    }

    private static void FlashPrimed(Enemy enemy)
    {
        if (enemy == null)
            return;

        var displays = UnityEngine.Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var display in displays)
        {
            if (display != null && display.GetEnemy() == enemy)
            {
                display.FlashThreatIcons(0.2f);
                break;
            }
        }
    }

    private static void ApplyCharging(Enemy enemy, ref EnemyIntentEntry entry)
    {
        if (entry.IsCharged)
            return;
        if (entry.Type != EnemyIntent.Attack && !entry.IsAbility)
            return;
        if (enemy.chargingDelayTurns <= 0 || enemy.chargingChance <= 0f || enemy.chargingDamageMultiplier <= 1f)
            return;

        if (!HasThreat(entry, ThreatWord.Charging))
            return;

        if (UnityEngine.Random.value > enemy.chargingChance)
            return;

        entry.IsCharged = true;
        entry.ChargedMultiplier = enemy.chargingDamageMultiplier;
        entry.ChargedDelayTurns = enemy.chargingDelayTurns;
        entry.Timing = Mathf.Max(entry.Timing, enemy.chargingDelayTurns);
        if (entry.IsAbility)
            entry.AbilityValue = Mathf.RoundToInt(entry.AbilityValue * enemy.chargingDamageMultiplier);
        else
            entry.Damage = Mathf.RoundToInt(entry.Damage * enemy.chargingDamageMultiplier);
    }

    private static bool ShouldApply(EnemyIntentEntry entry, ThreatWord word, ThreatHook hook)
    {
        if (!HasThreat(entry, word))
            return false;

        var def = ThreatBehaviorTable.Get(word);
        if (!Contains(def.Hooks, hook))
            return false;

        if (def.AppliesTo != null && def.AppliesTo.Length > 0)
        {
            return Contains(def.AppliesTo, entry.Type);
        }

        return true;
    }

    private static bool HasThreat(Enemy enemy, ThreatWord word)
    {
        if (enemy == null) return false;
        return enemy.primaryThreat == word || enemy.secondaryThreat == word;
    }

    private static bool IsDefensiveEnemy(Enemy enemy)
    {
        if (enemy == null) return false;
        return enemy.aiPattern == EnemyAIPattern.Defensive || enemy.category == EnemyCategory.Tank;
    }

    private static bool HasThreat(EnemyIntentEntry entry, ThreatWord word)
    {
        return entry.PrimaryThreat == word || entry.SecondaryThreat == word;
    }

    private static void NormalizeThreatsByBinding(ref EnemyIntentEntry entry)
    {
        entry.PrimaryThreat = NormalizeThreat(entry.PrimaryThreat, entry.IsAbility);
        entry.SecondaryThreat = NormalizeThreat(entry.SecondaryThreat, entry.IsAbility);

        if (entry.PrimaryThreat == ThreatWord.None && entry.SecondaryThreat != ThreatWord.None)
        {
            entry.PrimaryThreat = entry.SecondaryThreat;
            entry.SecondaryThreat = ThreatWord.None;
        }
    }

    private static ThreatWord NormalizeThreat(ThreatWord word, bool isAbility)
    {
        if (word == ThreatWord.None)
            return ThreatWord.None;

        var def = ThreatBehaviorTable.Get(word);
        if (def.Binding == ThreatBindingScope.Removed)
            return ThreatWord.None;

        if (isAbility)
        {
            if (def.Binding == ThreatBindingScope.EnemyBound)
                return ThreatWord.None;
        }
        else
        {
            if (def.Binding == ThreatBindingScope.AbilityBound)
                return ThreatWord.None;
        }

        return word;
    }

    private static bool Contains<T>(T[] values, T target)
    {
        if (values == null) return false;
        for (int i = 0; i < values.Length; i++)
        {
            if (Equals(values[i], target))
                return true;
        }
        return false;
    }
}
