using System;
using System.Collections.Generic;
using UnityEngine;

public enum TemporaryStatType
{
    MaxMana,
    IncreasedEvasion
}

/// <summary>
/// Handles temporary stat adjustments (time or turn based) and ensures they revert correctly.
/// </summary>
[DefaultExecutionOrder(-45)]
public class TemporaryStatSystem : MonoBehaviour
{
    private class TemporaryStatInstance
    {
        public string id;
        public TemporaryStatType statType;
        public float magnitude;
        public float? expiresAtTime;
        public int remainingTurns;
    }

    private static TemporaryStatSystem _instance;
    public static TemporaryStatSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindFirstObjectByType<TemporaryStatSystem>();
                if (_instance == null)
                {
                    var go = new GameObject("TemporaryStatSystem");
                    _instance = go.AddComponent<TemporaryStatSystem>();
                }
            }
            return _instance;
        }
    }

    private readonly List<TemporaryStatInstance> activeInstances = new List<TemporaryStatInstance>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (activeInstances.Count == 0) return;

        float now = Time.time;
        bool anyRemoved = false;

        for (int i = activeInstances.Count - 1; i >= 0; i--)
        {
            var instance = activeInstances[i];
            if (instance.expiresAtTime.HasValue && now >= instance.expiresAtTime.Value)
            {
                RevertInstance(instance);
                activeInstances.RemoveAt(i);
                anyRemoved = true;
            }
        }

        if (anyRemoved)
        {
            RefreshPlayerDisplay();
        }
    }

    /// <summary>
    /// Apply a temporary stat effect to the current player.
    /// </summary>
    public void ApplyToPlayer(TemporaryStatType statType, float magnitude, int durationTurns = 0, float durationSeconds = 0f)
    {
        var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
        if (character == null)
        {
            Debug.LogWarning("[TemporaryStatSystem] No character available when trying to apply temporary stat.");
            return;
        }

        var instance = new TemporaryStatInstance
        {
            id = Guid.NewGuid().ToString("N"),
            statType = statType,
            magnitude = magnitude,
            expiresAtTime = durationSeconds > 0f ? Time.time + durationSeconds : (float?)null,
            remainingTurns = Mathf.Max(0, durationTurns)
        };

        ApplyInstance(character, instance);
        activeInstances.Add(instance);
        RefreshPlayerDisplay();
    }

    /// <summary>
    /// Called at the end of a player turn to progress turn-based buffs.
    /// </summary>
    public void AdvanceTurn()
    {
        if (activeInstances.Count == 0) return;

        var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
        if (character == null) return;

        bool anyRemoved = false;
        for (int i = activeInstances.Count - 1; i >= 0; i--)
        {
            var instance = activeInstances[i];
            if (instance.remainingTurns <= 0) continue;

            instance.remainingTurns--;
            if (instance.remainingTurns <= 0)
            {
                RevertInstance(instance);
                activeInstances.RemoveAt(i);
                anyRemoved = true;
            }
        }

        if (anyRemoved)
        {
            RefreshPlayerDisplay();
        }
    }

    public void ClearAll()
    {
        var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
        if (character == null)
        {
            activeInstances.Clear();
            return;
        }

        foreach (var instance in activeInstances)
        {
            RevertInstance(instance);
        }

        activeInstances.Clear();
        RefreshPlayerDisplay();
    }

    private void ApplyInstance(Character character, TemporaryStatInstance instance)
    {
        float sign = 1f;
        switch (instance.statType)
        {
            case TemporaryStatType.MaxMana:
                {
                    int delta = Mathf.RoundToInt(instance.magnitude * sign);
                    character.maxMana = Mathf.Max(0, character.maxMana + delta);
                    character.mana = Mathf.Min(character.maxMana, character.mana + delta);
                    break;
                }
            case TemporaryStatType.IncreasedEvasion:
                {
                    float delta = (instance.magnitude / 100f) * sign;
                    character.increasedEvasion = Mathf.Max(0f, character.increasedEvasion + delta);
                    break;
                }
        }
    }

    private void RevertInstance(TemporaryStatInstance instance)
    {
        var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
        if (character == null) return;

        float sign = -1f;
        switch (instance.statType)
        {
            case TemporaryStatType.MaxMana:
                {
                    int delta = Mathf.RoundToInt(instance.magnitude * sign);
                    character.maxMana = Mathf.Max(0, character.maxMana + delta);
                    character.mana = Mathf.Min(character.mana, character.maxMana);
                    break;
                }
            case TemporaryStatType.IncreasedEvasion:
                {
                    float delta = (instance.magnitude / 100f) * sign;
                    character.increasedEvasion = Mathf.Max(0f, character.increasedEvasion + delta);
                    break;
                }
        }
    }

    private void RefreshPlayerDisplay()
    {
        var display = FindFirstObjectByType<PlayerCombatDisplay>();
        if (display != null)
        {
            display.RefreshDisplay();
        }
    }
}

