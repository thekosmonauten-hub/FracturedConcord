using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Manages enemy targeting for card play.
/// Highlights selected enemy and provides target for card effects.
/// </summary>
public class EnemyTargetingManager : MonoBehaviour
{
    public static EnemyTargetingManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private CombatDisplayManager combatManager;
    
    [Header("Targeting Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color targetedColor = Color.yellow;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.7f); // Light yellow
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showTargetArrow = true;
    [SerializeField] private GameObject targetArrowPrefab;
    
    private int selectedEnemyIndex = 0;
    private Enemy selectedEnemy = null;
    private EnemyCombatDisplay selectedDisplay = null;
    private GameObject targetArrow;
    
    // Events
    public System.Action<Enemy> OnTargetChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (combatManager == null)
        {
            combatManager = FindFirstObjectByType<CombatDisplayManager>();
        }
    }
    
    private void Start()
    {
        SetupEnemyClickHandlers();
        
        // Subscribe to enemy defeated events
        if (combatManager != null)
        {
            combatManager.OnEnemyDefeated += OnEnemyDefeated;
        }
        
        // Select first enemy by default
        SelectFirstAvailableEnemy();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe
        if (combatManager != null)
        {
            combatManager.OnEnemyDefeated -= OnEnemyDefeated;
        }
        
        // Clean up
        ClearTargetHighlight();
    }
    
    /// <summary>
    /// Setup click handlers on all enemy displays.
    /// Note: EnemyCombatDisplay now handles its own clickable areas on visual elements,
    /// so we don't need to add buttons here. This method is kept for compatibility.
    /// </summary>
    private void SetupEnemyClickHandlers()
    {
        // EnemyCombatDisplay.SetupClickableArea() now handles making visual elements clickable
        // This ensures clicks work on the actual sprites/UI elements, not just the root bounds
        // No action needed here - EnemyCombatDisplay handles it in Start()
        
        if (combatManager == null || combatManager.GetActiveEnemyDisplays() == null) return;
        
        Debug.Log($"EnemyTargetingManager: {combatManager.GetActiveEnemyDisplays().Count} enemy displays found. Click handling is managed by EnemyCombatDisplay components.");
    }
    
    /// <summary>
    /// Called when an enemy panel is clicked.
    /// </summary>
    private void OnEnemyClicked(int enemyIndex)
    {
        SelectEnemy(enemyIndex);
    }
    
    /// <summary>
    /// Select an enemy as the target.
    /// </summary>
    public void SelectEnemy(int enemyIndex)
    {
        if (combatManager == null || combatManager.GetActiveEnemyDisplays() == null) return;
        
        if (enemyIndex < 0 || enemyIndex >= combatManager.GetActiveEnemyDisplays().Count)
        {
            Debug.LogWarning($"Invalid enemy index: {enemyIndex}");
            return;
        }
        
        EnemyCombatDisplay display = combatManager.GetActiveEnemyDisplays()[enemyIndex];
        Enemy enemy = display.GetCurrentEnemy();
        
        // Can't target dead enemies
        if (enemy == null || enemy.currentHealth <= 0)
        {
            Debug.Log($"Enemy {enemyIndex} is dead or null, selecting next available...");
            SelectNextAvailableEnemy();
            return;
        }
        
        // Clear previous selection
        ClearTargetHighlight();
        
        // Set new selection
        selectedEnemyIndex = enemyIndex;
        selectedEnemy = enemy;
        selectedDisplay = display;
        
        // Apply highlight
        ApplyTargetHighlight();
        
        // Trigger event
        OnTargetChanged?.Invoke(selectedEnemy);
        
        Debug.Log($"<color=yellow>Target selected: {enemy.enemyName} (Index: {enemyIndex})</color>");
    }
    
    /// <summary>
    /// Get currently targeted enemy.
    /// </summary>
    public Enemy GetTargetedEnemy()
    {
        // Validate target is still alive
        if (selectedEnemy != null && selectedEnemy.currentHealth > 0)
        {
            return selectedEnemy;
        }
        
        // Target is dead, select next
        SelectNextAvailableEnemy();
        return selectedEnemy;
    }

    public int GetTargetedEnemyIndex()
    {
        return selectedEnemyIndex;
    }

    /// <summary>
    /// Simple hit-roll based on player's accuracy and enemy evasion (if added later).
    /// </summary>
    public bool TryHitRoll(Character attacker, Enemy target)
    {
        float accuracy = Mathf.Max(0f, attacker != null ? attacker.accuracyRating : 0f);
        float evasion = 0f; // Placeholder until Enemy has evasion
        float chanceToHit = accuracy > 0f ? (accuracy / (accuracy + Mathf.Max(1f, evasion))) : 0.8f;
        return Random.value < Mathf.Clamp01(chanceToHit);
    }
    
    /// <summary>
    /// Get screen position of targeted enemy (for card animations).
    /// </summary>
    public Vector3 GetTargetedEnemyPosition()
    {
        if (selectedDisplay != null)
        {
            RectTransform rect = selectedDisplay.GetComponent<RectTransform>();
            if (rect != null)
            {
                return rect.position;
            }
        }
        
        // Fallback
        return new Vector3(Screen.width * 0.7f, Screen.height * 0.6f, 0);
    }
    
    /// <summary>
    /// Select first available (alive) enemy.
    /// </summary>
    public void SelectFirstAvailableEnemy()
    {
        if (combatManager == null) return;
        
        for (int i = 0; i < combatManager.GetActiveEnemyDisplays().Count; i++)
        {
            Enemy enemy = combatManager.GetActiveEnemyDisplays()[i].GetCurrentEnemy();
            if (enemy != null && enemy.currentHealth > 0)
            {
                SelectEnemy(i);
                return;
            }
        }
        
        Debug.LogWarning("No available enemies to target!");
    }
    
    /// <summary>
    /// Select next available enemy (cycle through).
    /// </summary>
    public void SelectNextAvailableEnemy()
    {
        if (combatManager == null) return;

        var displays = combatManager.GetActiveEnemyDisplays();
        int displayCount = displays != null ? displays.Count : 0;

        if (displayCount == 0)
        {
            // No enemies remain – clear selection safely
            Debug.Log("No enemy displays available to target. Clearing selection.");
            ClearTargetHighlight();
            selectedEnemyIndex = -1;
            selectedEnemy = null;
            selectedDisplay = null;
            OnTargetChanged?.Invoke(null);
            return;
        }

        // Ensure starting index is within range
        int startIndex = Mathf.Clamp(selectedEnemyIndex, 0, displayCount - 1);
        int checkIndex = (startIndex + 1) % displayCount;

        // Scan through all displays once to find a living enemy
        for (int i = 0; i < displayCount; i++)
        {
            int currentIndex = (checkIndex + i) % displayCount;
            var display = displays[currentIndex];
            Enemy enemy = display != null ? display.GetCurrentEnemy() : null;

            if (enemy != null && enemy.currentHealth > 0)
            {
                SelectEnemy(currentIndex);
                return;
            }
        }

        // No living enemies were found; clear current selection
        Debug.Log("EnemyTargetingManager: No alive enemies found when cycling targets. Clearing selection.");
        ClearTargetHighlight();
        selectedEnemyIndex = -1;
        selectedEnemy = null;
        selectedDisplay = null;
        OnTargetChanged?.Invoke(null);
    }
    
    /// <summary>
    /// Apply visual highlight to selected enemy.
    /// </summary>
    private void ApplyTargetHighlight()
    {
        if (selectedDisplay == null) return;

        // Change enemy NAME text color instead of slider/border
        bool changedAny = false;

        // Unity UI Text
        Text[] uiTexts = selectedDisplay.GetComponentsInChildren<Text>(true);
        foreach (var t in uiTexts)
        {
            if (t != null && t.name.IndexOf("Name", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                t.color = targetedColor;
                changedAny = true;
            }
        }

        // TMP Text
        TMP_Text[] tmpTexts = selectedDisplay.GetComponentsInChildren<TMP_Text>(true);
        foreach (var tt in tmpTexts)
        {
            if (tt != null && tt.name.IndexOf("Name", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                tt.color = targetedColor;
                changedAny = true;
            }
        }

        if (!changedAny)
        {
            Debug.LogWarning($"EnemyTargetingManager: Could not find a Name text under {selectedDisplay.name} to highlight.");
        }

        Debug.Log($"Applied targeting highlight to {selectedDisplay.name} (name color)");
    }
    
    /// <summary>
    /// Clear visual highlight from previously selected enemy.
    /// </summary>
    private void ClearTargetHighlight()
    {
        if (selectedDisplay == null) return;

        // Get the enemy to restore its rarity color
        Enemy enemy = selectedDisplay.GetCurrentEnemy();
        EnemyData enemyData = selectedDisplay.GetEnemyData();
        Color rarityColor = normalColor; // Default fallback
        
        if (enemy != null && enemyData != null)
        {
            // Get the proper rarity color from EnemyCombatDisplay
            rarityColor = selectedDisplay.GetEnemyNameColor(enemy.rarity, enemyData.tier);
        }
        else if (enemy != null)
        {
            // Fallback: calculate rarity color manually
            EnemyTier tier = enemyData != null ? enemyData.tier : EnemyTier.Normal;
            if (tier == EnemyTier.Boss || tier == EnemyTier.Miniboss)
            {
                rarityColor = new Color(1f, 0.65f, 0f); // Orange
            }
            else
            {
                switch (enemy.rarity)
                {
                    case EnemyRarity.Normal:
                        rarityColor = Color.white;
                        break;
                    case EnemyRarity.Magic:
                        rarityColor = new Color(0.3f, 0.6f, 1f); // Blue
                        break;
                    case EnemyRarity.Rare:
                        rarityColor = new Color(1f, 0.9f, 0.2f); // Yellow
                        break;
                    case EnemyRarity.Unique:
                        rarityColor = new Color(1f, 0.65f, 0f); // Orange
                        break;
                }
            }
        }

        // Reset only the enemy NAME text color to rarity color
        Text[] uiTexts = selectedDisplay.GetComponentsInChildren<Text>(true);
        foreach (var t in uiTexts)
        {
            if (t != null && t.name.IndexOf("Name", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                t.color = rarityColor;
            }
        }

        TMP_Text[] tmpTexts = selectedDisplay.GetComponentsInChildren<TMP_Text>(true);
        foreach (var tt in tmpTexts)
        {
            if (tt != null && tt.name.IndexOf("Name", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                tt.color = rarityColor;
            }
        }
    }
    
    /// <summary>
    /// Called when an enemy is defeated (auto-select next).
    /// </summary>
    public void OnEnemyDefeated(Enemy defeatedEnemy)
    {
        if (defeatedEnemy == selectedEnemy)
        {
            Debug.Log($"Targeted enemy defeated! Selecting next...");
            ClearTargetHighlight();
            SelectNextAvailableEnemy();
        }
    }
    
    #region Input Handling
    
    private void Update()
    {
        // Use new Input System
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Tab key to cycle targets
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            SelectNextAvailableEnemy();
        }
        
        // Number keys (1-3) to select specific enemy
        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            SelectEnemy(0);
        }
        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            SelectEnemy(1);
        }
        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
        {
            SelectEnemy(2);
        }
    }
    
    #endregion
    
    #region Debug
    
    [ContextMenu("Select Next Enemy")]
    private void DebugSelectNext()
    {
        SelectNextAvailableEnemy();
    }
    
    [ContextMenu("Show Current Target")]
    private void DebugShowCurrentTarget()
    {
        if (selectedEnemy != null)
        {
            Debug.Log($"Current Target: {selectedEnemy.enemyName} (Index: {selectedEnemyIndex})");
            Debug.Log($"  HP: {selectedEnemy.currentHealth}/{selectedEnemy.maxHealth}");
        }
        else
        {
            Debug.LogWarning("No enemy targeted!");
        }
    }
    
    #endregion
}

