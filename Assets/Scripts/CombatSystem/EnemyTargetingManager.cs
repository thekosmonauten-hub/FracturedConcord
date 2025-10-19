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
    /// </summary>
    private void SetupEnemyClickHandlers()
    {
        if (combatManager == null || combatManager.enemyDisplays == null) return;
        
        for (int i = 0; i < combatManager.enemyDisplays.Count; i++)
        {
            EnemyCombatDisplay display = combatManager.enemyDisplays[i];
            if (display != null)
            {
                // Add button if not present
                Button button = display.GetComponent<Button>();
                if (button == null)
                {
                    button = display.gameObject.AddComponent<Button>();
                    button.transition = Selectable.Transition.None; // We'll handle visuals ourselves
                }
                
                // Setup click handler
                int enemyIndex = i; // Capture for lambda
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnEnemyClicked(enemyIndex));
                
                Debug.Log($"Setup targeting for enemy panel {i}");
            }
        }
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
        if (combatManager == null || combatManager.enemyDisplays == null) return;
        
        if (enemyIndex < 0 || enemyIndex >= combatManager.enemyDisplays.Count)
        {
            Debug.LogWarning($"Invalid enemy index: {enemyIndex}");
            return;
        }
        
        EnemyCombatDisplay display = combatManager.enemyDisplays[enemyIndex];
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
        
        for (int i = 0; i < combatManager.enemyDisplays.Count; i++)
        {
            Enemy enemy = combatManager.enemyDisplays[i].GetCurrentEnemy();
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
        
        int startIndex = selectedEnemyIndex;
        int checkIndex = (startIndex + 1) % combatManager.enemyDisplays.Count;
        
        // Search for next alive enemy
        while (checkIndex != startIndex)
        {
            Enemy enemy = combatManager.enemyDisplays[checkIndex].GetCurrentEnemy();
            if (enemy != null && enemy.currentHealth > 0)
            {
                SelectEnemy(checkIndex);
                return;
            }
            
            checkIndex = (checkIndex + 1) % combatManager.enemyDisplays.Count;
        }
        
        // Check starting index as last resort
        Enemy startEnemy = combatManager.enemyDisplays[startIndex].GetCurrentEnemy();
        if (startEnemy != null && startEnemy.currentHealth > 0)
        {
            SelectEnemy(startIndex);
        }
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

        // Reset only the enemy NAME text color
        Text[] uiTexts = selectedDisplay.GetComponentsInChildren<Text>(true);
        foreach (var t in uiTexts)
        {
            if (t != null && t.name.IndexOf("Name", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                t.color = normalColor;
            }
        }

        TMP_Text[] tmpTexts = selectedDisplay.GetComponentsInChildren<TMP_Text>(true);
        foreach (var tt in tmpTexts)
        {
            if (tt != null && tt.name.IndexOf("Name", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                tt.color = normalColor;
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

