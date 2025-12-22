using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button controller that opens the Active Warrant Stats panel.
/// Can be attached to any button in the WarrantTree scene.
/// </summary>
public class WarrantStatsButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private ActiveWarrantStatsPanel statsPanel;

    private void Awake()
    {
        // Find button if not assigned
        if (button == null)
            button = GetComponent<Button>();

        // Find panel if not assigned
        if (statsPanel == null)
            statsPanel = FindFirstObjectByType<ActiveWarrantStatsPanel>();
    }

    private void Start()
    {
        // Wire button click
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("[WarrantStatsButton] Button component not found!");
        }
    }

    private void OnButtonClicked()
    {
        if (statsPanel == null)
        {
            Debug.LogWarning("[WarrantStatsButton] ActiveWarrantStatsPanel not found! Make sure the panel exists in the scene.");
            return;
        }

        statsPanel.TogglePanel();
    }
}

