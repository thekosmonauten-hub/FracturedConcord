using UnityEngine;

/// <summary>
/// Damage effect that scales based on a dynamic variable (cards played, turns elapsed, etc.)
/// Used for abilities like Black Dawn Crash: "Deals damage based on how many cards you played last turn"
/// </summary>
[CreateAssetMenu(fileName = "ScalingDamageEffect", menuName = "Dexiled/Enemies/Effects/Scaling Damage")]
public class ScalingDamageEffect : AbilityEffect
{
    [Header("Base Damage")]
    [Tooltip("Base flat damage before scaling")]
    public int baseDamage = 15;
    
    [Tooltip("Damage type to deal")]
    public DamageType damageType = DamageType.Physical;
    
    [Header("Scaling Variable")]
    [Tooltip("What variable to scale damage with")]
    public ScalingVariable scalingVariable = ScalingVariable.CardsPlayedLastTurn;
    
    [Tooltip("Damage added per unit of the scaling variable")]
    public int damagePerUnit = 8;
    
    [Header("Limits")]
    [Tooltip("Maximum scaling bonus (0 = unlimited)")]
    public int maxScalingBonus = 0;
    
    public override void Execute(AbilityContext ctx)
    {
        if (ctx == null) return;
        
        int scalingValue = GetScalingValue(ctx);
        int scalingBonus = scalingValue * damagePerUnit;
        
        // Apply max limit if set
        if (maxScalingBonus > 0)
        {
            scalingBonus = Mathf.Min(scalingBonus, maxScalingBonus);
        }
        
        int totalDamage = baseDamage + scalingBonus;
        
        Debug.Log($"[ScalingDamage] Base: {baseDamage}, Scaling: {scalingBonus} ({scalingValue} × {damagePerUnit}), Total: {totalDamage}");
        
        // Apply damage to player (most scaling abilities target player)
        if (ctx.target == AbilityTarget.Player || ctx.target == AbilityTarget.AllPlayers)
        {
            ApplyDamageToPlayer(totalDamage, ctx);
        }
        else
        {
            Debug.LogWarning($"[ScalingDamage] Target {ctx.target} not yet supported for scaling damage");
        }
    }
    
    private int GetScalingValue(AbilityContext ctx)
    {
        switch (scalingVariable)
        {
            case ScalingVariable.CardsPlayedLastTurn:
                // Get cards played last turn from combat state
                var combatManager = UnityEngine.Object.FindFirstObjectByType<CombatDisplayManager>();
                if (combatManager != null)
                {
                    // Store this value in enemy custom data at end of player turn
                    if (ctx.enemyRuntime != null)
                    {
                        object stored = ctx.enemyRuntime.GetBossData("cardsPlayedLastTurn");
                        if (stored != null && stored is int)
                        {
                            return (int)stored;
                        }
                    }
                }
                return 0;
            
            case ScalingVariable.CurrentTurn:
                var cm = UnityEngine.Object.FindFirstObjectByType<CombatDisplayManager>();
                return cm != null ? cm.currentTurn : 1;
            
            case ScalingVariable.EnemyMissingHealthPercent:
                if (ctx.enemyRuntime != null)
                {
                    float missingPercent = 100f * (1f - ((float)ctx.enemyRuntime.currentHealth / ctx.enemyRuntime.maxHealth));
                    return Mathf.FloorToInt(missingPercent / 10f); // Per 10% missing
                }
                return 0;
            
            default:
                return 0;
        }
    }
    
    private void ApplyDamageToPlayer(int damage, AbilityContext ctx)
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.TakeDamage(damage);
            
            // Show floating damage
            var floatingDamageManager = UnityEngine.Object.FindFirstObjectByType<FloatingDamageManager>();
            var playerDisplay = UnityEngine.Object.FindFirstObjectByType<PlayerCombatDisplay>();
            if (floatingDamageManager != null && playerDisplay != null)
            {
                floatingDamageManager.ShowDamage(damage, false, playerDisplay.transform);
            }
            
            Debug.Log($"[ScalingDamage] Dealt {damage} {damageType} damage to player");
        }
    }
}

public enum ScalingVariable
{
    CardsPlayedLastTurn,      // Black Dawn Crash
    CurrentTurn,               // Turn-based scaling
    EnemyMissingHealthPercent, // Enrage mechanics
    PlayerBuffCount,           // Buff-based scaling
    CardsInHand,              // Hand size scaling
}

