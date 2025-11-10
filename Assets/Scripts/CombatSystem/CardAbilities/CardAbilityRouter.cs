using System;
using UnityEngine;

/// <summary>
/// Centralises bespoke card effects and combo follow-ups for card data assets.
/// </summary>
public static class CardAbilityRouter
{
    private const string FocusKey = "Focus";
    private const string DodgeKey = "Dodge";
    private const string PackHunterKey = "PackHunter";
    private const string PoisonArrowKey = "PoisonArrow";
    private const string MultiShotKey = "MultiShot";
    private const string QuickstepKey = "Quickstep";

    public static void ApplyCardPlay(CardDataExtended cardData, Card runtimeCard, Character player, Enemy primaryTarget, Vector3 targetScreenPosition)
    {
        if (cardData == null || player == null)
            return;

        string currentKey = NormalizeKey(!string.IsNullOrEmpty(cardData.groupKey) ? cardData.groupKey : cardData.cardName);
        string lastGroupKey = ComboSystem.Instance != null ? NormalizeKey(ComboSystem.Instance.GetLastPlayedGroupKey()) : string.Empty;
        string lastCardName = ComboSystem.Instance != null ? NormalizeKey(ComboSystem.Instance.GetLastPlayedName()) : string.Empty;
        string lastKey = !string.IsNullOrEmpty(lastGroupKey) ? lastGroupKey : lastCardName;

        switch (currentKey)
        {
            case FocusKey:
                HandleFocus(player);
                break;
            case DodgeKey:
                HandleDodge(player, lastKey);
                break;
            case PackHunterKey:
                HandlePackHunter(player, runtimeCard, targetScreenPosition, lastKey);
                break;
            case PoisonArrowKey:
                HandlePoisonArrow(player, lastKey);
                break;
            case MultiShotKey:
                HandleMultiShot(player, lastKey);
                break;
            case QuickstepKey:
                HandleQuickstep(player, lastKey);
                break;
        }
    }

    private static void HandleFocus(Character player)
    {
        var statusManager = TryGetPlayerStatusManager();
        var playerDisplay = TryGetPlayerDisplay();
        if (player == null) return;

        bool atFullMana = player.mana >= player.maxMana;
        if (atFullMana)
        {
            if (statusManager != null)
            {
                StatusEffect overcharge = new StatusEffect(StatusEffectType.TempMaxMana, "Focus: Overcharge", 1f, 1, false);
                statusManager.AddStatusEffect(overcharge);
            }
        }
        else
        {
            player.RestoreMana(1);
            playerDisplay?.UpdateManaDisplay();
        }

        if (statusManager != null)
        {
            StatusEffect evasionBuff = new StatusEffect(StatusEffectType.TempEvasion, "Focus: Poised", 20f, 2, false);
            statusManager.AddStatusEffect(evasionBuff);
        }
    }

    private static void HandleDodge(Character player, string lastKey)
    {
        if (player == null) return;
        float evasionGain = 15f + (player.dexterity / 2f);
        player.baseEvasionRating += evasionGain;

        TryGetPlayerDisplay()?.RefreshDisplay();

        if (Matches(lastKey, PackHunterKey))
        {
            TemporaryStatSystem.Instance?.ApplyToPlayer(TemporaryStatType.IncreasedEvasion, 25f, durationSeconds: 2f);
        }
    }

    private static void HandleQuickstep(Character player, string lastKey)
    {
        if (player == null) return;
        float evasionGain = 20f + (player.dexterity / 2f);
        if (Matches(lastKey, FocusKey))
        {
            evasionGain *= 3f;
        }

        player.baseEvasionRating += evasionGain;
        TryGetPlayerDisplay()?.RefreshDisplay();
    }

    private static void HandlePoisonArrow(Character player, string lastKey)
    {
        bool focusCombo = Matches(lastKey, FocusKey);
        int baseDuration = 3;
        int duration = focusCombo ? baseDuration * 3 : baseDuration;
        int stacks = 2;
        float perStackDamage = Mathf.Max(1f, player.dexterity * 0.1f);
        float magnitude = stacks * perStackDamage;

        var enemyDisplays = UnityEngine.Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var display in enemyDisplays)
        {
            if (display == null || !display.gameObject.activeInHierarchy) continue;
            var enemy = display.GetCurrentEnemy();
            if (enemy == null || enemy.currentHealth <= 0) continue;

            var statusManager = display.GetStatusEffectManager();
            if (statusManager == null) continue;

            StatusEffect poison = new StatusEffect(StatusEffectType.Poison, "Poison Arrow", magnitude, duration, true);
            statusManager.AddStatusEffect(poison);
        }
    }

    private static void HandleMultiShot(Character player, string lastKey)
    {
        bool focusCombo = Matches(lastKey, FocusKey);
        if (!focusCombo) return;

        var enemyDisplays = UnityEngine.Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var display in enemyDisplays)
        {
            if (display == null || !display.gameObject.activeInHierarchy) continue;
            var enemy = display.GetCurrentEnemy();
            if (enemy == null || enemy.currentHealth <= 0) continue;

            var statusManager = display.GetStatusEffectManager();
            if (statusManager == null) continue;

            float magnitude = Mathf.Max(1f, player.dexterity * 0.15f);
            StatusEffect bleed = new StatusEffect(StatusEffectType.Bleed, "Multi-Shot Bleed", magnitude, 3, true);
            statusManager.AddStatusEffect(bleed);
        }

        StackSystem.Instance?.AddStacks(StackType.Agitate, 1);
    }

    private static void HandlePackHunter(Character player, Card runtimeCard, Vector3 targetScreenPosition, string lastKey)
    {
        if (!Matches(lastKey, PackHunterKey) || player == null || runtimeCard == null)
            return;

        float repeatDamage = DamageCalculator.CalculateCardDamage(runtimeCard, player);
        var enemyDisplays = UnityEngine.Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        var animationManager = CombatAnimationManager.Instance;

        foreach (var display in enemyDisplays)
        {
            if (display == null || !display.gameObject.activeInHierarchy) continue;
            var enemy = display.GetCurrentEnemy();
            if (enemy == null || enemy.currentHealth <= 0) continue;

            float adjustedDamage = repeatDamage;
            var statusManager = display.GetStatusEffectManager();
            if (statusManager != null)
            {
                float vulnStacks = statusManager.GetTotalMagnitude(StatusEffectType.Vulnerable);
                if (vulnStacks > 0f)
                {
                    adjustedDamage *= (1f + 0.15f * vulnStacks);
                }
                float bolsterStacks = Mathf.Min(10f, statusManager.GetTotalMagnitude(StatusEffectType.Bolster));
                if (bolsterStacks > 0f)
                {
                    adjustedDamage *= Mathf.Clamp01(1f - 0.02f * bolsterStacks);
                }
            }

            display.TakeDamage(adjustedDamage);

            if (animationManager != null)
            {
                var position = display.transform.position;
                animationManager.ShowDamageNumber(adjustedDamage, position, ConvertDamageType(runtimeCard.primaryDamageType));
            }
        }
    }

    #region Helpers

    private static string NormalizeKey(string raw)
    {
        return string.IsNullOrWhiteSpace(raw)
            ? string.Empty
            : raw.Replace(" ", string.Empty, StringComparison.Ordinal)
                  .Replace("-", string.Empty, StringComparison.Ordinal)
                  .Trim();
    }

    private static bool Matches(string value, string expected)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(expected)) return false;
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static PlayerCombatDisplay TryGetPlayerDisplay()
    {
        return UnityEngine.Object.FindFirstObjectByType<PlayerCombatDisplay>();
    }

    private static StatusEffectManager TryGetPlayerStatusManager()
    {
        var display = TryGetPlayerDisplay();
        return display != null ? display.GetStatusEffectManager() : null;
    }

    private static DamageNumberType ConvertDamageType(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Fire:
                return DamageNumberType.Fire;
            case DamageType.Cold:
                return DamageNumberType.Cold;
            case DamageType.Lightning:
                return DamageNumberType.Lightning;
            default:
                return DamageNumberType.Normal;
        }
    }

    #endregion
}

