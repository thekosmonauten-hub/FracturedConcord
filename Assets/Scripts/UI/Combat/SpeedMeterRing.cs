using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the Aggression/Focus meter ring visual on the player combat display.
/// </summary>
public class SpeedMeterRing : MonoBehaviour
{
    [Header("Ring Images")]
    [SerializeField] private Image aggressionFill;
    [SerializeField] private Image focusFill;

    [Header("Charge Labels")]
    [SerializeField] private TextMeshProUGUI aggressionChargesText;
    [SerializeField] private TextMeshProUGUI focusChargesText;

    [Header("Ready Indicators")]
    [SerializeField] private GameObject aggressionReadyIndicator;
    [SerializeField] private GameObject focusReadyIndicator;

    private void Awake()
    {
        // Attempt to auto-wire children if not assigned
        if (aggressionFill == null)
            aggressionFill = transform.Find("AggressionFill")?.GetComponent<Image>();
        if (focusFill == null)
            focusFill = transform.Find("FocusFill")?.GetComponent<Image>();
        if (aggressionChargesText == null)
            aggressionChargesText = transform.Find("AggressionCharges")?.GetComponent<TextMeshProUGUI>();
        if (focusChargesText == null)
            focusChargesText = transform.Find("FocusCharges")?.GetComponent<TextMeshProUGUI>();
        if (aggressionReadyIndicator == null)
            aggressionReadyIndicator = transform.Find("AggressionReady")?.gameObject;
        if (focusReadyIndicator == null)
            focusReadyIndicator = transform.Find("FocusReady")?.gameObject;

        EnsureFilledImage(aggressionFill);
        EnsureFilledImage(focusFill);
    }

    public void UpdateMeterState(CombatDeckManager.SpeedMeterState state)
    {
        if (aggressionFill != null)
            aggressionFill.fillAmount = Mathf.Clamp01(state.aggressionProgress);

        if (focusFill != null)
            focusFill.fillAmount = Mathf.Clamp01(state.focusProgress);

        if (aggressionChargesText != null)
        {
            aggressionChargesText.text = state.aggressionCharges > 0 ? state.aggressionCharges.ToString() : string.Empty;
        }

        if (focusChargesText != null)
        {
            focusChargesText.text = state.focusCharges > 0 ? state.focusCharges.ToString() : string.Empty;
        }

        if (aggressionReadyIndicator != null)
            aggressionReadyIndicator.SetActive(state.IsAggressionReady);

        if (focusReadyIndicator != null)
            focusReadyIndicator.SetActive(state.IsFocusReady);
    }

    private void EnsureFilledImage(Image img)
    {
        if (img == null) return;
        if (img.type != Image.Type.Filled)
        {
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
        }
    }
}

