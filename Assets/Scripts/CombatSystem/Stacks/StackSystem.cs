using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralised manager for repeatable stack mechanics (Agitate, Tolerance, Potential).
/// Lives across scene loads and exposes helper multipliers for combat systems.
/// </summary>
[DefaultExecutionOrder(-50)]
public class StackSystem : MonoBehaviour
{
    private static StackSystem _instance;
    public static StackSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindFirstObjectByType<StackSystem>();
                if (_instance == null)
                {
                    var go = new GameObject("StackSystem");
                    _instance = go.AddComponent<StackSystem>();
                }
            }
            return _instance;
        }
    }

    [Serializable]
    private class StackState
    {
        public StackType type;
        public int baseMaxStacks = 10;
        public int bonusMaxStacks = 0;
        public int currentStacks = 0;
    }

    private readonly Dictionary<StackType, StackState> stackStates = new Dictionary<StackType, StackState>();

    /// <summary>
    /// Fired whenever a stack value changes (post-clamp). Provides new stack count.
    /// </summary>
    public event Action<StackType, int> OnStacksChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeStates();
    }

    private void InitializeStates()
    {
        foreach (StackType type in Enum.GetValues(typeof(StackType)))
        {
            if (!stackStates.ContainsKey(type))
            {
                stackStates[type] = new StackState
                {
                    type = type
                };
            }
        }
    }

    public int GetStacks(StackType type)
    {
        return stackStates.TryGetValue(type, out var state) ? state.currentStacks : 0;
    }

    public int GetMaxStacks(StackType type)
    {
        return stackStates.TryGetValue(type, out var state) ? state.baseMaxStacks + state.bonusMaxStacks : 0;
    }

    public void SetBonusMaxStacks(StackType type, int bonusStacks)
    {
        if (!stackStates.TryGetValue(type, out var state)) return;
        state.bonusMaxStacks = Mathf.Max(0, bonusStacks);
        ClampStacks(state);
    }

    public void AddStacks(StackType type, int amount)
    {
        ModifyStacks(type, Mathf.Abs(amount));
    }

    public void RemoveStacks(StackType type, int amount)
    {
        ModifyStacks(type, -Mathf.Abs(amount));
    }

    public void ClearStacks(StackType type)
    {
        if (!stackStates.TryGetValue(type, out var state)) return;
        if (state.currentStacks == 0) return;
        state.currentStacks = 0;
        OnStacksChanged?.Invoke(type, 0);
    }

    private void ModifyStacks(StackType type, int delta)
    {
        if (!stackStates.TryGetValue(type, out var state)) return;

        int previous = state.currentStacks;
        int maxStacks = state.baseMaxStacks + state.bonusMaxStacks;
        state.currentStacks = Mathf.Clamp(previous + delta, 0, maxStacks);

        if (state.currentStacks != previous)
        {
            OnStacksChanged?.Invoke(type, state.currentStacks);
        }
    }

    private void ClampStacks(StackState state)
    {
        int maxStacks = state.baseMaxStacks + state.bonusMaxStacks;
        if (state.currentStacks > maxStacks)
        {
            state.currentStacks = maxStacks;
            OnStacksChanged?.Invoke(state.type, state.currentStacks);
        }
    }

    #region Helper Multipliers

    /// <summary>
    /// Returns multiplicative damage bonus from Agitate stacks (1f when zero stacks).
    /// </summary>
    public float GetDamageMoreMultiplier()
    {
        int stacks = GetStacks(StackType.Agitate);
        return 1f + (0.02f * stacks);
    }

    /// <summary>
    /// Returns additive percent bonuses to attack/cast/move speed from Agitate stacks.
    /// </summary>
    public (float attack, float cast, float move) GetSpeedBonuses()
    {
        float percent = 2f * GetStacks(StackType.Agitate);
        return (percent, percent, percent);
    }

    /// <summary>
    /// Returns multiplicative damage-taken multiplier from Tolerance stacks (clamped to >= 0).
    /// </summary>
    public float GetToleranceDamageMultiplier()
    {
        float reduction = 0.03f * GetStacks(StackType.Tolerance);
        return Mathf.Clamp01(1f - reduction);
    }

    /// <summary>
    /// Returns additive crit chance bonus in percentage points from Potential stacks.
    /// </summary>
    public float GetCritChanceBonus()
    {
        return 2f * GetStacks(StackType.Potential);
    }

    /// <summary>
    /// Returns multiplicative crit multiplier bonus from Potential stacks (1f when zero stacks).
    /// </summary>
    public float GetCritMultiplierBonus()
    {
        return 1f + (0.02f * GetStacks(StackType.Potential));
    }

    #endregion
}

