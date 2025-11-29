using UnityEngine;

/// <summary>
/// Represents a delayed action that will execute in the future (enemy or player)
/// </summary>
[System.Serializable]
public class DelayedAction
{
    public enum ActionType
    {
        EnemyAction,
        PlayerCard
    }
    
    public ActionType actionType;
    
    // Enemy action fields
    public EnemyIntent intent;
    public int intentDamage;
    [UnityEngine.SerializeReference]
    public Enemy sourceEnemy; // Reference to the enemy that queued this action
    
    // Player card fields
    public CardDataExtended delayedCard; // The card to play
    [UnityEngine.SerializeReference]
    public Enemy targetEnemy; // Target enemy for the card
    public Vector3 targetPosition; // Target position for the card
    public bool isDelayed = true; // Flag to indicate this is a delayed card (for bonus application)
    
    public int turnsRemaining; // How many turns until this action executes
    
    // Constructor for enemy actions
    public DelayedAction(EnemyIntent intent, int damage, int delayTurns, Enemy enemy)
    {
        this.actionType = ActionType.EnemyAction;
        this.intent = intent;
        this.intentDamage = damage;
        this.turnsRemaining = delayTurns;
        this.sourceEnemy = enemy;
    }
    
    // Constructor for player cards
    public DelayedAction(CardDataExtended card, int delayTurns, Enemy targetEnemy = null, Vector3 targetPos = default)
    {
        this.actionType = ActionType.PlayerCard;
        this.delayedCard = card;
        this.targetEnemy = targetEnemy;
        this.targetPosition = targetPos;
        this.turnsRemaining = delayTurns;
    }
    
    /// <summary>
    /// Decrement the turn counter and return true if ready to execute
    /// </summary>
    public bool Tick()
    {
        turnsRemaining--;
        return turnsRemaining <= 0;
    }
}
