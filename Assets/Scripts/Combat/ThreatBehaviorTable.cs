using System.Collections.Generic;

public enum ThreatHook
{
    OnIntentGenerated,
    OnTurnStart,
    OnTurnEnd,
    OnDamaged,
    OnExecuteIntent,
    OnAbilityQueued
}

public enum ThreatBindingScope
{
    EnemyBound,
    AbilityBound,
    Either,
    Removed
}

public enum PrimedTriggerType
{
    PlayerStatusStacks,
    EnemyHealthPercent
}

public enum ThreatCounterType
{
    CardTag,
    Status,
    StackConsume,
    Resource,
    Action,
    Other
}

public struct ThreatCounter
{
    public ThreatCounterType Type;
    public string Id;
    public string Description;

    public ThreatCounter(ThreatCounterType type, string id, string description)
    {
        Type = type;
        Id = id;
        Description = description;
    }
}

public sealed class ThreatBehaviorDefinition
{
    public ThreatWord Word;
    public ThreatBindingScope Binding;
    public string EffectSummary;
    public EnemyIntent[] AppliesTo;
    public ThreatHook[] Hooks;
    public ThreatCounter[] Counters;

    public ThreatBehaviorDefinition(
        ThreatWord word,
        ThreatBindingScope binding,
        string effectSummary,
        EnemyIntent[] appliesTo,
        ThreatHook[] hooks,
        ThreatCounter[] counters)
    {
        Word = word;
        Binding = binding;
        EffectSummary = effectSummary;
        AppliesTo = appliesTo;
        Hooks = hooks;
        Counters = counters;
    }
}

public static class ThreatBehaviorTable
{
    private static readonly ThreatBehaviorDefinition[] Definitions = new[]
    {
        new ThreatBehaviorDefinition(
            ThreatWord.Charging,
            ThreatBindingScope.AbilityBound,
            "Delay intent and boost its damage/effect.",
            new[] { EnemyIntent.Attack },
            new[] { ThreatHook.OnIntentGenerated, ThreatHook.OnTurnStart },
            new[]
            {
                new ThreatCounter(ThreatCounterType.Action, "Interrupt", "Interrupt or force early execution to prevent the charge."),
                new ThreatCounter(ThreatCounterType.Status, "Freeze/Stun", "Disable the enemy while charging to skip the action.")
            }),
        new ThreatBehaviorDefinition(
            ThreatWord.Primed,
            ThreatBindingScope.EnemyBound,
            "Arms a payoff that triggers once a condition is met.",
            new[] { EnemyIntent.Attack, EnemyIntent.Defend },
            new[] { ThreatHook.OnTurnStart, ThreatHook.OnDamaged },
            new[]
            {
                new ThreatCounter(ThreatCounterType.StackConsume, "ReduceStacks", "Lower the triggering stacks/condition."),
                new ThreatCounter(ThreatCounterType.Action, "Dispel", "Remove the primed state before it fires.")
            }),
        new ThreatBehaviorDefinition(
            ThreatWord.Anchoring,
            ThreatBindingScope.EnemyBound,
            "Protects allies or redirects damage while alive.",
            new[] { EnemyIntent.Defend },
            new[] { ThreatHook.OnTurnStart, ThreatHook.OnDamaged },
            new[]
            {
                new ThreatCounter(ThreatCounterType.Action, "BreakGuard", "Break guard or bypass protection."),
                new ThreatCounter(ThreatCounterType.Action, "FocusFire", "Eliminate the anchor first.")
            }),
        new ThreatBehaviorDefinition(
            ThreatWord.Leeching,
            ThreatBindingScope.EnemyBound,
            "Converts your gains into their resources.",
            new[] { EnemyIntent.Attack },
            new[] { ThreatHook.OnExecuteIntent },
            new[]
            {
                new ThreatCounter(ThreatCounterType.Status, "Cleanse", "Cleanse or prevent the drain."),
                new ThreatCounter(ThreatCounterType.Action, "Guard", "Block the on-hit conversion.")
            }),
        new ThreatBehaviorDefinition(
            ThreatWord.Suppressing,
            ThreatBindingScope.EnemyBound,
            "Applies play restrictions or debuffs on hit.",
            new[] { EnemyIntent.Attack },
            new[] { ThreatHook.OnExecuteIntent },
            new[]
            {
                new ThreatCounter(ThreatCounterType.Status, "Cleanse", "Remove suppression debuffs."),
                new ThreatCounter(ThreatCounterType.CardTag, "CleanseTag", "Play cleanse-tagged cards.")
            }),
        new ThreatBehaviorDefinition(
            ThreatWord.Escalating,
            ThreatBindingScope.EnemyBound,
            "Intent grows stronger each turn it remains queued.",
            new[] { EnemyIntent.Attack },
            new[] { ThreatHook.OnTurnStart },
            new[]
            {
                new ThreatCounter(ThreatCounterType.Action, "DelayIntent", "Push the intent back to reset/slow growth."),
                new ThreatCounter(ThreatCounterType.Action, "Interrupt", "Cancel before it scales too high.")
            }),
        new ThreatBehaviorDefinition(
            ThreatWord.Volatile,
            ThreatBindingScope.AbilityBound,
            "Unstable, with splash or random targeting.",
            new[] { EnemyIntent.Attack },
            new[] { ThreatHook.OnExecuteIntent },
            new[]
            {
                new ThreatCounter(ThreatCounterType.Status, "Stun", "Stop the volatile trigger."),
                new ThreatCounter(ThreatCounterType.Action, "SpreadOut", "Mitigate multi-target impacts.")
            }),
        new ThreatBehaviorDefinition(
            ThreatWord.Retaliating,
            ThreatBindingScope.EnemyBound,
            "Queues a counter-attack when damaged.",
            new[] { EnemyIntent.Attack },
            new[] { ThreatHook.OnDamaged },
            new[]
            {
                new ThreatCounter(ThreatCounterType.Action, "Guard", "Soak the counter-attack."),
                new ThreatCounter(ThreatCounterType.Action, "Burst", "Defeat before retaliation matters.")
            }),
        new ThreatBehaviorDefinition(
            ThreatWord.Channeling,
            ThreatBindingScope.AbilityBound,
            "Locks into a delayed, board-warping effect.",
            new[] { EnemyIntent.Attack },
            new[] { ThreatHook.OnIntentGenerated, ThreatHook.OnTurnStart, ThreatHook.OnExecuteIntent },
            new[]
            {
                new ThreatCounter(ThreatCounterType.Action, "Interrupt", "Break the channel."),
                new ThreatCounter(ThreatCounterType.Status, "Silence", "Prevent channel effects.")
            }),
        new ThreatBehaviorDefinition(
            ThreatWord.Converting,
            ThreatBindingScope.Removed,
            "Removed (refocus).",
            new EnemyIntent[0],
            new ThreatHook[0],
            new ThreatCounter[0]),
        new ThreatBehaviorDefinition(
            ThreatWord.Shielded,
            ThreatBindingScope.EnemyBound,
            "Protected until a condition is met.",
            new[] { EnemyIntent.Defend },
            new[] { ThreatHook.OnIntentGenerated, ThreatHook.OnDamaged },
            new[]
            {
                new ThreatCounter(ThreatCounterType.Action, "BreakShield", "Meet the condition to remove the shield."),
                new ThreatCounter(ThreatCounterType.CardTag, "Pierce", "Use pierce-tagged cards.")
            }),
        new ThreatBehaviorDefinition(
            ThreatWord.Terminal,
            ThreatBindingScope.AbilityBound,
            "If unanswered, ends the fight or causes catastrophic effect.",
            new[] { EnemyIntent.Attack },
            new[] { ThreatHook.OnIntentGenerated, ThreatHook.OnExecuteIntent },
            new[]
            {
                new ThreatCounter(ThreatCounterType.Action, "HardInterrupt", "Stop the intent entirely."),
                new ThreatCounter(ThreatCounterType.Resource, "Burst", "Remove the enemy before it fires.")
            })
    };

    private static readonly Dictionary<ThreatWord, ThreatBehaviorDefinition> DefinitionLookup =
        BuildLookup();

    private static Dictionary<ThreatWord, ThreatBehaviorDefinition> BuildLookup()
    {
        var map = new Dictionary<ThreatWord, ThreatBehaviorDefinition>();
        foreach (var def in Definitions)
        {
            map[def.Word] = def;
        }
        return map;
    }

    public static ThreatBehaviorDefinition Get(ThreatWord word)
    {
        if (DefinitionLookup.TryGetValue(word, out var def))
            return def;
        return new ThreatBehaviorDefinition(word, ThreatBindingScope.Either, string.Empty, new EnemyIntent[0], new ThreatHook[0], new ThreatCounter[0]);
    }

    public static IEnumerable<ThreatBehaviorDefinition> All => Definitions;
}
