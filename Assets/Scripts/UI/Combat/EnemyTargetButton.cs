using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to each enemy's UGUI Button (on the EnemyPanel) to set the active target.
/// Configure enemyIndex as zero-based (Enemy1 = 0, Enemy2 = 1, ...).
/// </summary>
[RequireComponent(typeof(Button))]
public sealed class EnemyTargetButton : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Zero-based index of this enemy (Enemy1 = 0, Enemy2 = 1, ...)")]
	private int enemyIndex;

	[SerializeField]
    private CombatManager combatManager;

	private Button cachedButton;

	private void Reset()
	{
		combatManager = FindFirstObjectByType<CombatManager>();
		cachedButton = GetComponent<Button>();
	}

	private void Awake()
	{
		if (combatManager == null)
		{
			combatManager = FindFirstObjectByType<CombatManager>();
		}

		cachedButton = cachedButton != null ? cachedButton : GetComponent<Button>();
		if (cachedButton != null)
		{
			cachedButton.onClick.RemoveAllListeners();
			cachedButton.onClick.AddListener(OnClicked);
		}
	}

    private void OnClicked()
    {
        // Refresh references in case objects were loaded later
        if (combatManager == null)
        {
            combatManager = FindFirstObjectByType<CombatManager>();
        }

        var targeting = EnemyTargetingManager.Instance;

        // 1) Update display-side targeting (highlight) if available
        if (targeting != null)
        {
            targeting.SelectEnemy(enemyIndex);
        }

        // 2) Update combat logic target if available (used by PlayCard single-target resolution)
        if (combatManager != null)
        {
            combatManager.SelectEnemy(enemyIndex);
        }
        else if (targeting == null)
        {
            Debug.LogWarning($"EnemyTargetButton: No CombatManager or EnemyTargetingManager found for index {enemyIndex}.");
        }
    }
}


