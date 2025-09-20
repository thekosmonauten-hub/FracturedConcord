using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CombatDisplayManager : MonoBehaviour
{
    [Header("Combat Participants")]
    public PlayerCombatDisplay playerDisplay;
    public List<EnemyCombatDisplay> enemyDisplays = new List<EnemyCombatDisplay>();
    
    [Header("Combat Settings")]
    public int maxEnemies = 3;
    public bool autoStartCombat = true;
    public float turnDelay = 1f;
    
    [Header("Combat State")]
    public CombatState currentState = CombatState.Setup;
    public int currentTurn = 0;
    public bool isPlayerTurn = true;
    
    [Header("Test Configuration")]
    public bool createTestEnemies = true;
    public int testEnemyCount = 2;
    
    // Combat events
    public System.Action<CombatState> OnCombatStateChanged;
    public System.Action<int> OnTurnChanged;
    public System.Action<bool> OnTurnTypeChanged;
    public System.Action<Enemy> OnEnemyDefeated;
    public System.Action OnCombatEnded;
    
    private List<Enemy> activeEnemies = new List<Enemy>();
    private CharacterManager characterManager;
    private SimpleCombatUI combatUI;
    
    public enum CombatState
    {
        Setup,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }
    
    private void Start()
    {
        InitializeCombat();
    }
    
    private void InitializeCombat()
    {
        // Get references
        characterManager = CharacterManager.Instance;
        combatUI = FindFirstObjectByType<SimpleCombatUI>();
        
        // Auto-find player display if not assigned
        if (playerDisplay == null)
        {
            playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        }
        
        // Auto-find enemy displays if not assigned
        if (enemyDisplays.Count == 0)
        {
            EnemyCombatDisplay[] foundEnemies = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            enemyDisplays.AddRange(foundEnemies);
        }
        
        // Create test enemies if enabled
        if (createTestEnemies)
        {
            CreateTestEnemies();
        }
        
        // Start combat if auto-start is enabled
        if (autoStartCombat)
        {
            StartCombat();
        }
    }
    
    private void CreateTestEnemies()
    {
        // Create test enemies for combat
        List<Enemy> testEnemies = new List<Enemy>
        {
            new Enemy("Goblin Scout", 30, 6),
            new Enemy("Orc Warrior", 45, 8),
            new Enemy("Dark Mage", 35, 10)
        };
        
        // Assign enemies to displays
        for (int i = 0; i < Mathf.Min(testEnemyCount, enemyDisplays.Count); i++)
        {
            if (i < testEnemies.Count)
            {
                enemyDisplays[i].SetEnemy(testEnemies[i]);
                activeEnemies.Add(testEnemies[i]);
            }
        }
        
        Debug.Log($"Created {activeEnemies.Count} test enemies for combat");
    }
    
    public void StartCombat()
    {
        currentState = CombatState.Setup;
        currentTurn = 1;
        isPlayerTurn = true;
        
        // Set initial enemy intents
        foreach (Enemy enemy in activeEnemies)
        {
            enemy.SetIntent();
        }
        
        // Update all displays
        RefreshAllDisplays();
        
        // Start player turn
        StartPlayerTurn();
        
        Debug.Log("Combat started!");
    }
    
    private void StartPlayerTurn()
    {
        currentState = CombatState.PlayerTurn;
        isPlayerTurn = true;
        
        // Restore player resources
        if (characterManager != null && characterManager.HasCharacter())
        {
            Character character = characterManager.GetCurrentCharacter();
            character.RestoreMana(character.manaRecoveryPerTurn);
            
            // Draw cards (if combat UI is available)
            if (combatUI != null)
            {
                // This would integrate with the card system
                // combatUI.DrawCards(character.cardsDrawnPerTurn);
            }
        }
        
        // Update displays
        RefreshAllDisplays();
        
        OnCombatStateChanged?.Invoke(currentState);
        OnTurnChanged?.Invoke(currentTurn);
        OnTurnTypeChanged?.Invoke(isPlayerTurn);
        
        Debug.Log($"Player turn {currentTurn} started");
    }
    
    private void StartEnemyTurn()
    {
        currentState = CombatState.EnemyTurn;
        isPlayerTurn = false;
        
        // Update displays
        RefreshAllDisplays();
        
        OnCombatStateChanged?.Invoke(currentState);
        OnTurnTypeChanged?.Invoke(isPlayerTurn);
        
        Debug.Log($"Enemy turn {currentTurn} started");
        
        // Execute enemy actions
        StartCoroutine(ExecuteEnemyActions());
    }
    
    private IEnumerator ExecuteEnemyActions()
    {
        // Execute each enemy's action
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            Enemy enemy = activeEnemies[i];
            if (enemy.currentHealth > 0)
            {
                yield return ExecuteEnemyAction(enemy, i);
                yield return new WaitForSeconds(turnDelay);
            }
        }
        
        // End enemy turn and start next player turn
        EndEnemyTurn();
    }
    
    private IEnumerator ExecuteEnemyAction(Enemy enemy, int enemyIndex)
    {
        Debug.Log($"{enemy.enemyName} is taking action...");
        
        switch (enemy.currentIntent)
        {
            case EnemyIntent.Attack:
                // Attack the player
                if (characterManager != null && characterManager.HasCharacter())
                {
                    int damage = enemy.GetAttackDamage();
                    characterManager.TakeDamage(damage);
                    Debug.Log($"{enemy.enemyName} attacks for {damage} damage!");
                }
                break;
                
            case EnemyIntent.Defend:
                // Enemy defends (could add block/armor system)
                Debug.Log($"{enemy.enemyName} is defending!");
                break;
        }
        
        // Set new intent for next turn
        enemy.SetIntent();
        
        // Update enemy display
        if (enemyIndex < enemyDisplays.Count)
        {
            enemyDisplays[enemyIndex].UpdateIntent();
        }
        
        yield return null;
    }
    
    private void EndEnemyTurn()
    {
        currentTurn++;
        StartPlayerTurn();
    }
    
    public void EndPlayerTurn()
    {
        // This would be called when the player finishes their turn
        // (e.g., after playing cards or clicking "End Turn")
        StartEnemyTurn();
    }
    
    public void PlayerAttackEnemy(int enemyIndex, float damage)
    {
        if (enemyIndex >= 0 && enemyIndex < activeEnemies.Count)
        {
            Enemy targetEnemy = activeEnemies[enemyIndex];
            targetEnemy.TakeDamage(damage);
            
            // Update enemy display
            if (enemyIndex < enemyDisplays.Count)
            {
                enemyDisplays[enemyIndex].TakeDamage(damage);
                enemyDisplays[enemyIndex].PlayDamageAnimation();
            }
            
            // Check if enemy is defeated
            if (targetEnemy.currentHealth <= 0)
            {
                OnEnemyDefeated?.Invoke(targetEnemy);
                activeEnemies.RemoveAt(enemyIndex);
                
                // Check for victory
                if (activeEnemies.Count == 0)
                {
                    EndCombat(true);
                }
            }
        }
    }
    
    public void PlayerHeal(int amount)
    {
        if (characterManager != null)
        {
            characterManager.Heal(amount);
        }
    }
    
    private void EndCombat(bool victory)
    {
        currentState = victory ? CombatState.Victory : CombatState.Defeat;
        
        OnCombatStateChanged?.Invoke(currentState);
        OnCombatEnded?.Invoke();
        
        Debug.Log($"Combat ended: {(victory ? "Victory!" : "Defeat!")}");
    }
    
    private void RefreshAllDisplays()
    {
        if (playerDisplay != null)
        {
            playerDisplay.RefreshDisplay();
        }
        
        foreach (EnemyCombatDisplay enemyDisplay in enemyDisplays)
        {
            if (enemyDisplay != null)
            {
                enemyDisplay.RefreshDisplay();
            }
        }
    }
    
    // Public methods for external control
    public void AddEnemy(Enemy enemy)
    {
        if (activeEnemies.Count < maxEnemies)
        {
            activeEnemies.Add(enemy);
            
            // Find available enemy display
            for (int i = 0; i < enemyDisplays.Count; i++)
            {
                if (enemyDisplays[i].GetCurrentEnemy() == null)
                {
                    enemyDisplays[i].SetEnemy(enemy);
                    break;
                }
            }
        }
    }
    
    public void RemoveEnemy(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        
        // Clear enemy display
        foreach (EnemyCombatDisplay enemyDisplay in enemyDisplays)
        {
            if (enemyDisplay.GetCurrentEnemy() == enemy)
            {
                enemyDisplay.SetEnemy(null);
                break;
            }
        }
    }
    
    public List<Enemy> GetActiveEnemies()
    {
        return new List<Enemy>(activeEnemies);
    }
    
    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }
    
    public CombatState GetCombatState()
    {
        return currentState;
    }
    
    // Debug methods
    [ContextMenu("Start Combat")]
    public void DebugStartCombat()
    {
        StartCombat();
    }
    
    [ContextMenu("End Player Turn")]
    public void DebugEndPlayerTurn()
    {
        EndPlayerTurn();
    }
    
    [ContextMenu("Test Player Attack")]
    public void DebugPlayerAttack()
    {
        if (activeEnemies.Count > 0)
        {
            PlayerAttackEnemy(0, 15);
        }
    }
    
    [ContextMenu("Test Player Heal")]
    public void DebugPlayerHeal()
    {
        PlayerHeal(10);
    }
    
    [ContextMenu("Create Test Enemies")]
    public void DebugCreateEnemies()
    {
        CreateTestEnemies();
    }
}
