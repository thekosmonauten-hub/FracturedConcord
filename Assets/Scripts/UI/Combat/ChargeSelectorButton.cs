using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unified selector that exposes Aggression/Focus charge effects to the player.
/// Shows one button when only one charge type is ready, a split button when both are ready.
/// </summary>
public class ChargeSelectorButton : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button aggressionButton;
    [SerializeField] private Button focusButton;
    [SerializeField] private GameObject combinedButtonRoot;
    [SerializeField] private Button combinedAggressionButton;
    [SerializeField] private Button combinedFocusButton;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI aggressionText;
    [SerializeField] private TextMeshProUGUI focusText;
    [SerializeField] private TextMeshProUGUI combinedAggressionText;
    [SerializeField] private TextMeshProUGUI combinedFocusText;

    private CombatDeckManager deckManager;
    private CombatDeckManager.AggressionChargeEffect currentAggressionEffect = CombatDeckManager.AggressionChargeEffect.None;
    private CombatDeckManager.FocusChargeEffect currentFocusEffect = CombatDeckManager.FocusChargeEffect.None;

    private void Awake()
    {
        aggressionButton ??= transform.Find("AggressionButton")?.GetComponent<Button>();
        focusButton ??= transform.Find("FocusButton")?.GetComponent<Button>();
        if (combinedButtonRoot == null)
            combinedButtonRoot = transform.Find("CombinedButton")?.gameObject;

        if (combinedAggressionButton == null && combinedButtonRoot != null)
            combinedAggressionButton = combinedButtonRoot.transform.Find("AggressionButton")?.GetComponent<Button>();
        if (combinedFocusButton == null && combinedButtonRoot != null)
            combinedFocusButton = combinedButtonRoot.transform.Find("FocusButton")?.GetComponent<Button>();

        if (aggressionText == null && aggressionButton != null)
            aggressionText = aggressionButton.GetComponentInChildren<TextMeshProUGUI>();
        if (focusText == null && focusButton != null)
            focusText = focusButton.GetComponentInChildren<TextMeshProUGUI>();
        if (combinedAggressionText == null && combinedAggressionButton != null)
            combinedAggressionText = combinedAggressionButton.GetComponentInChildren<TextMeshProUGUI>();
        if (combinedFocusText == null && combinedFocusButton != null)
            combinedFocusText = combinedFocusButton.GetComponentInChildren<TextMeshProUGUI>();

        aggressionButton?.onClick.AddListener(OnAggressionClicked);
        focusButton?.onClick.AddListener(OnFocusClicked);
        combinedAggressionButton?.onClick.AddListener(OnCombinedAggressionClicked);
        combinedFocusButton?.onClick.AddListener(OnCombinedFocusClicked);
    }

    private void Start()
    {
        deckManager = CombatDeckManager.Instance;
        if (deckManager != null)
        {
            deckManager.OnSpeedMeterChanged += HandleSpeedMeterChanged;
            deckManager.OnAggressionModifierSelected += HandleAggressionSelected;
            deckManager.OnFocusModifierSelected += HandleFocusSelected;
            deckManager.OnChargeModifiersCleared += HandleModifiersCleared;

            currentAggressionEffect = deckManager.GetAggressionChargeEffect();
            currentFocusEffect = deckManager.GetFocusChargeEffect();
        }

        UpdateButtonState();
    }

    private void OnDestroy()
    {
        if (deckManager != null)
        {
            deckManager.OnSpeedMeterChanged -= HandleSpeedMeterChanged;
            deckManager.OnAggressionModifierSelected -= HandleAggressionSelected;
            deckManager.OnFocusModifierSelected -= HandleFocusSelected;
            deckManager.OnChargeModifiersCleared -= HandleModifiersCleared;
        }
    }

    private void HandleSpeedMeterChanged(CombatDeckManager.SpeedMeterState state) => UpdateButtonState();

    private void HandleAggressionSelected(CombatDeckManager.AggressionChargeEffect effect)
    {
        currentAggressionEffect = effect;
        UpdateButtonState();
    }

    private void HandleFocusSelected(CombatDeckManager.FocusChargeEffect effect)
    {
        currentFocusEffect = effect;
        UpdateButtonState();
    }

    private void HandleModifiersCleared()
    {
        currentAggressionEffect = CombatDeckManager.AggressionChargeEffect.None;
        currentFocusEffect = CombatDeckManager.FocusChargeEffect.None;
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (deckManager == null) return;

        var meterState = deckManager.GetSpeedMeterState();
        bool hasAggression = meterState.aggressionCharges > 0;
        bool hasFocus = meterState.focusCharges > 0;
        bool hasBoth = hasAggression && hasFocus;

        aggressionButton?.gameObject.SetActive(hasAggression && !hasBoth);
        focusButton?.gameObject.SetActive(hasFocus && !hasBoth);
        if (combinedButtonRoot != null)
            combinedButtonRoot.SetActive(hasBoth);

        if (hasAggression && !hasBoth && aggressionText != null)
        {
            aggressionText.text = currentAggressionEffect != CombatDeckManager.AggressionChargeEffect.None
                ? $"<color=yellow>{currentAggressionEffect}</color>"
                : "Aggression";
        }

        if (hasFocus && !hasBoth && focusText != null)
        {
            focusText.text = currentFocusEffect != CombatDeckManager.FocusChargeEffect.None
                ? $"<color=yellow>{currentFocusEffect}</color>"
                : "Focus";
        }

        if (hasBoth)
        {
            if (combinedAggressionText != null)
            {
                combinedAggressionText.text = currentAggressionEffect != CombatDeckManager.AggressionChargeEffect.None
                    ? $"<color=yellow>{currentAggressionEffect}</color>"
                    : "Aggression";
            }

            if (combinedFocusText != null)
            {
                combinedFocusText.text = currentFocusEffect != CombatDeckManager.FocusChargeEffect.None
                    ? $"<color=yellow>{currentFocusEffect}</color>"
                    : "Focus";
            }
        }
    }

    private void OnAggressionClicked() => CycleAggressionEffect();
    private void OnFocusClicked() => CycleFocusEffect();

    private void OnCombinedAggressionClicked() => CycleAggressionEffect();
    private void OnCombinedFocusClicked() => CycleFocusEffect();

    private void CycleAggressionEffect()
    {
        if (deckManager == null) return;

        var current = deckManager.GetAggressionChargeEffect();
        var next = current switch
        {
            CombatDeckManager.AggressionChargeEffect.None => CombatDeckManager.AggressionChargeEffect.HitsTwice,
            CombatDeckManager.AggressionChargeEffect.HitsTwice => CombatDeckManager.AggressionChargeEffect.HitsAllEnemies,
            CombatDeckManager.AggressionChargeEffect.HitsAllEnemies => CombatDeckManager.AggressionChargeEffect.AlwaysCrit,
            CombatDeckManager.AggressionChargeEffect.AlwaysCrit => CombatDeckManager.AggressionChargeEffect.IgnoresGuardArmor,
            CombatDeckManager.AggressionChargeEffect.IgnoresGuardArmor => CombatDeckManager.AggressionChargeEffect.None,
            _ => CombatDeckManager.AggressionChargeEffect.HitsTwice
        };

        if (next != CombatDeckManager.AggressionChargeEffect.None)
        {
            deckManager.SelectAggressionChargeEffect(next);
        }
        else
        {
            deckManager.ClearAggressionChargeEffect();
        }
    }

    private void CycleFocusEffect()
    {
        if (deckManager == null) return;

        var current = deckManager.GetFocusChargeEffect();
        var next = current switch
        {
            CombatDeckManager.FocusChargeEffect.None => CombatDeckManager.FocusChargeEffect.DoubleDamage,
            CombatDeckManager.FocusChargeEffect.DoubleDamage => CombatDeckManager.FocusChargeEffect.HalfManaCost,
            CombatDeckManager.FocusChargeEffect.HalfManaCost => CombatDeckManager.FocusChargeEffect.None,
            _ => CombatDeckManager.FocusChargeEffect.DoubleDamage
        };

        if (next != CombatDeckManager.FocusChargeEffect.None)
        {
            deckManager.SelectFocusChargeEffect(next);
        }
        else
        {
            deckManager.ClearFocusChargeEffect();
        }
    }
}



