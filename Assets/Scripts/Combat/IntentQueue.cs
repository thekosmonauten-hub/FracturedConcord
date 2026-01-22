using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Threat tier for intent vocabulary. Used for readability and future UI (color, emphasis).
/// </summary>
public enum ThreatTier
{
    Minor,
    Major,
    Lethal
}

/// <summary>
/// A single entry in an enemy's intent queue. Supports 1–3 upcoming intents per enemy.
/// </summary>
[Serializable]
public struct EnemyIntentEntry
{
    public EnemyIntent Type;
    public int Damage;
    /// <summary>Turns until execution (0 = this turn).</summary>
    public int Timing;
    public ThreatTier Tier;
    public bool IsAbility;
    public string AbilityId;
    public string AbilityName;
    public int AbilityValue;
    public Sprite AbilityIcon;

    public EnemyIntentEntry(EnemyIntent type, int damage, int timing = 0, ThreatTier tier = ThreatTier.Minor)
    {
        Type = type;
        Damage = damage;
        Timing = timing;
        Tier = tier;
        IsAbility = false;
        AbilityId = null;
        AbilityName = null;
        AbilityValue = 0;
        AbilityIcon = null;
    }

    public EnemyIntentEntry(string abilityId, string abilityName, int abilityValue, Sprite abilityIcon, int timing = 0)
    {
        Type = EnemyIntent.Attack;
        Damage = 0;
        Timing = timing;
        Tier = ThreatTier.Minor;
        IsAbility = true;
        AbilityId = abilityId;
        AbilityName = abilityName;
        AbilityValue = abilityValue;
        AbilityIcon = abilityIcon;
    }
}

/// <summary>
/// Mutable queue of 1–3 intents per enemy. Source of truth for "what this enemy will do."
/// Supports insert, remove, delay; used for reactive combat hooks (Phase 3).
/// </summary>
public class IntentQueue
{
    public const int MaxIntents = 3;

    private readonly List<EnemyIntentEntry> _entries = new List<EnemyIntentEntry>();

    public int Count => _entries.Count;
    public bool IsEmpty => _entries.Count == 0;

    /// <summary>Next intent to execute (head of queue). Null if empty.</summary>
    public EnemyIntentEntry? Peek()
    {
        if (_entries.Count == 0) return null;
        return _entries[0];
    }

    /// <summary>All queued intents (read-only).</summary>
    public IReadOnlyList<EnemyIntentEntry> All => _entries;

    public void Clear()
    {
        _entries.Clear();
    }

    /// <summary>Add intent to back of queue. Respects MaxIntents.</summary>
    public void Enqueue(EnemyIntentEntry entry)
    {
        if (_entries.Count >= MaxIntents) return;
        _entries.Add(entry);
    }

    /// <summary>Remove and return head. Returns null if empty.</summary>
    public EnemyIntentEntry? Dequeue()
    {
        if (_entries.Count == 0) return null;
        var head = _entries[0];
        _entries.RemoveAt(0);
        return head;
    }

    /// <summary>Remove intent at index. Returns false if out of range.</summary>
    public bool RemoveAt(int index)
    {
        if (index < 0 || index >= _entries.Count) return false;
        _entries.RemoveAt(index);
        return true;
    }

    /// <summary>Insert intent at index. If full, drops the last entry.</summary>
    public void InsertAt(int index, EnemyIntentEntry entry)
    {
        if (_entries.Count >= MaxIntents)
            _entries.RemoveAt(_entries.Count - 1);
        index = Mathf.Clamp(index, 0, _entries.Count);
        _entries.Insert(index, entry);
    }

    /// <summary>Find the first index matching predicate, or -1.</summary>
    public int FindIndex(Predicate<EnemyIntentEntry> predicate)
    {
        return _entries.FindIndex(predicate);
    }

    /// <summary>Remove the first entry matching predicate. Returns true if removed.</summary>
    public bool RemoveFirst(Predicate<EnemyIntentEntry> predicate)
    {
        int index = _entries.FindIndex(predicate);
        if (index < 0) return false;
        _entries.RemoveAt(index);
        return true;
    }

    /// <summary>Delay head intent by N turns (increment its Timing). No-op if empty.</summary>
    public void DelayHead(int turns)
    {
        if (_entries.Count == 0 || turns <= 0) return;
        var e = _entries[0];
        e.Timing += turns;
        _entries[0] = e;
    }

    /// <summary>Decrement Timing for all intents (called each turn). Remove any that reach execution (Timing 0).</summary>
    public void TickTurn()
    {
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            var e = _entries[i];
            e.Timing = Math.Max(0, e.Timing - 1);
            _entries[i] = e;
        }
    }
}
