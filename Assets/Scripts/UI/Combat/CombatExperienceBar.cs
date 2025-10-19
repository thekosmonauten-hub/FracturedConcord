using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatExperienceBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider xpSlider;              // 0..1
    [SerializeField] private TextMeshProUGUI xpText;       // "XP: 23/100" (optional)

    private void OnEnable()
    {
        // Subscribe to character events
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharacterLoaded += HandleCharacterChanged;
            CharacterManager.Instance.OnExperienceGained += HandleCharacterChanged;
            CharacterManager.Instance.OnCharacterLevelUp += HandleCharacterChanged;
        }

        // Initial update
        Refresh();
    }

    private void OnDisable()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharacterLoaded -= HandleCharacterChanged;
            CharacterManager.Instance.OnExperienceGained -= HandleCharacterChanged;
            CharacterManager.Instance.OnCharacterLevelUp -= HandleCharacterChanged;
        }
    }

    private void HandleCharacterChanged(Character _)
    {
        Refresh();
    }

    private void Refresh()
    {
        var cm = CharacterManager.Instance;
        if (cm == null || !cm.HasCharacter()) return;

        var c = cm.GetCurrentCharacter();
        int needed = Mathf.Max(1, c.GetRequiredExperience());   // avoid div by zero
        int current = Mathf.Clamp(c.experience, 0, needed);

        float t = Mathf.Clamp01((float)current / needed);
        if (xpSlider != null) xpSlider.value = t;

        if (xpText != null)
        {
            xpText.text = $"XP: {current}/{needed}  Lv {c.level}";
        }
    }
}