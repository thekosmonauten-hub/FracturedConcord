using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CombatManager : MonoBehaviour
{
    [Header("Combat State")]
    public bool isPlayerTurn = true;
    public bool combatActive = false;
    public int selectedEnemyIndex = 0; // Track which enemy is targeted
    
    [Header("Characters")]
    public Character playerCharacter;
    public List<Enemy> enemies = new List<Enemy>(); // Multiple enemies
    public Enemy currentEnemy; // Keep for backward compatibility
    
    [Header("Deck Management")]
    public List<Card> drawPile = new List<Card>();
    public List<Card> discardPile = new List<Card>();
    public List<Card> currentHand = new List<Card>();
    public int handSize = 5;
    
    [Header("UI References")]
    public CombatUI combatUI;
    
    [Header("Events")]
    public System.Action<Card> OnCardPlayed;
    public System.Action OnTurnEnded;
    public System.Action OnCombatEnded;
    
    private void Start()
    {
        InitializeCombat();
    }
    
    public void InitializeCombat()
    {
        // Load current character
        if (CharacterManager.Instance.HasCharacter())
        {
            playerCharacter = CharacterManager.Instance.GetCurrentCharacter();
        }
        else
        {
            // Create a test character if none exists
            playerCharacter = new Character("TestMarauder", "Marauder");
            CharacterManager.Instance.currentCharacter = playerCharacter;
        }
        
        // Equip a test weapon
        EquipTestWeapon();
        
        // Create test enemies
        CreateTestEnemies();
        
        // Initialize deck
        InitializeDeck();
        
        // Start combat
        StartCombat();
    }
    
    private void EquipTestWeapon()
    {
        // Create a Steel Axe for testing
        Weapon steelAxe = CharacterWeapons.CreateSteelAxe();
        playerCharacter.weapons.meleeWeapon = steelAxe;
        
        Debug.Log($"Equipped {steelAxe.weaponName} to {playerCharacter.characterName}");
        Debug.Log($"Weapon damage: {steelAxe.GetDamageRangeString()}");
    }
    
    private void CreateTestEnemies()
    {
        // Create multiple test enemies
        enemies.Add(new Enemy("Goblin", 50, 8));
        enemies.Add(new Enemy("Orc", 75, 12));
        enemies.Add(new Enemy("Troll", 100, 15));
        
        // Set the first enemy as current for backward compatibility
        currentEnemy = enemies[0];
        
        Debug.Log($"Created {enemies.Count} enemies for combat");
    }
    
    private void InitializeDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        currentHand.Clear();
        
        // Get starter deck for character
        if (playerCharacter.currentDeck != null)
        {
            drawPile.AddRange(playerCharacter.currentDeck.cards);
        }
        else
        {
            // Create Marauder starter deck if none exists
            CreateMarauderStarterDeck();
        }
        
        // Shuffle deck
        ShuffleDeck();
        
        // Draw initial hand
        DrawCards(handSize);
    }
    
    private void CreateMarauderStarterDeck()
    {
        // Create Marauder starter deck using the proper system
        MarauderStarterDeck marauderDeck = ScriptableObject.CreateInstance<MarauderStarterDeck>();
        Deck deck = marauderDeck.CreateMarauderStarterDeck();
        
        // Add all cards from the Marauder starter deck
        drawPile.AddRange(deck.cards);
        
        Debug.Log($"Created Marauder starter deck with {deck.cards.Count} cards");
    }
    
    private void ShuffleDeck()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Card temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
    }
    
    public void StartCombat()
    {
        combatActive = true;
        isPlayerTurn = true;
        
        Debug.Log($"Combat started! {playerCharacter.characterName} vs {currentEnemy.enemyName}");
        
        // Update UI
        if (combatUI != null)
        {
            combatUI.UpdateCombatUI();
        }
    }
    
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (drawPile.Count > 0)
            {
                Card drawnCard = drawPile[0];
                drawPile.RemoveAt(0);
                currentHand.Add(drawnCard);
            }
            else if (discardPile.Count > 0)
            {
                // Reshuffle discard pile into draw pile
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck();
                
                if (drawPile.Count > 0)
                {
                    Card drawnCard = drawPile[0];
                    drawPile.RemoveAt(0);
                    currentHand.Add(drawnCard);
                }
            }
        }
        
        // Update UI
        if (combatUI != null)
        {
            combatUI.UpdateHand();
        }
    }
    
    public bool CanPlayCard(Card card)
    {
        // Check mana cost
        if (playerCharacter.mana < card.manaCost)
            return false;
        
        // Check requirements
        if (!card.CanUseCard(playerCharacter))
            return false;
        
        return true;
    }
    
    public void PlayCard(Card card)
    {
        if (!CanPlayCard(card))
        {
            Debug.LogWarning($"Cannot play card {card.cardName}");
            return;
        }
        
        // Remove from hand
        currentHand.Remove(card);
        
        // Spend mana
        playerCharacter.UseMana(card.manaCost);
        
        // Calculate damage
        float damage = DamageCalculator.CalculateCardDamage(card, playerCharacter, GetEquippedWeapon());
        
        // Handle AoE vs single target
        if (card.isAoE)
        {
            // AoE card hits multiple enemies
            PlayAoECard(card, damage);
        }
        else
        {
            // Single target card
            PlaySingleTargetCard(card, damage);
        }
        
        // Add to discard pile
        discardPile.Add(card);
        
        // Check if any enemies are defeated
        CheckEnemyDefeats();
        
        // Update UI
        if (combatUI != null)
        {
            combatUI.UpdateCombatUI();
        }
        
        // Trigger event
        OnCardPlayed?.Invoke(card);
    }
    
    private void PlaySingleTargetCard(Card card, float damage)
    {
        // Apply damage to selected enemy
        if (selectedEnemyIndex < enemies.Count && enemies[selectedEnemyIndex] != null)
        {
            enemies[selectedEnemyIndex].TakeDamage(damage);
            Debug.Log($"{playerCharacter.characterName} played {card.cardName} for {damage:F1} damage on {enemies[selectedEnemyIndex].enemyName}!");
        }
        else
        {
            Debug.LogWarning("No valid target for card!");
        }
    }
    
    private void PlayAoECard(Card card, float damage)
    {
        // AoE hits multiple enemies
        int targetsHit = 0;
        int maxTargets = Mathf.Min(card.aoeTargets, enemies.Count);
        
        for (int i = 0; i < maxTargets; i++)
        {
            if (enemies[i] != null && enemies[i].IsAlive())
            {
                enemies[i].TakeDamage(damage);
                targetsHit++;
                Debug.Log($"{playerCharacter.characterName} played {card.cardName} for {damage:F1} damage on {enemies[i].enemyName}!");
            }
        }
        
        if (targetsHit > 0)
        {
            Debug.Log($"{card.cardName} hit {targetsHit} enemies for {damage:F1} damage each!");
        }
        else
        {
            Debug.LogWarning("No valid targets for AoE card!");
        }
    }
    
    // Target a specific enemy
    public void SelectEnemy(int enemyIndex)
    {
        if (enemyIndex >= 0 && enemyIndex < enemies.Count)
        {
            selectedEnemyIndex = enemyIndex;
            currentEnemy = enemies[enemyIndex]; // Update for backward compatibility
            Debug.Log($"Targeted enemy: {enemies[enemyIndex].enemyName}");
            
            // Update UI
            if (combatUI != null)
            {
                combatUI.UpdateCombatUI();
            }
        }
    }
    
    // Get the currently selected enemy
    public Enemy GetSelectedEnemy()
    {
        if (selectedEnemyIndex < enemies.Count)
        {
            return enemies[selectedEnemyIndex];
        }
        return null;
    }
    
    // Check if any enemies are defeated
    private void CheckEnemyDefeats()
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (!enemies[i].IsAlive())
            {
                Debug.Log($"{enemies[i].enemyName} has been defeated!");
                enemies.RemoveAt(i);
                
                // Adjust selected enemy index if needed
                if (selectedEnemyIndex >= enemies.Count)
                {
                    selectedEnemyIndex = Mathf.Max(0, enemies.Count - 1);
                }
                
                // Update current enemy for backward compatibility
                if (enemies.Count > 0)
                {
                    currentEnemy = enemies[selectedEnemyIndex];
                }
                else
                {
                    currentEnemy = null;
                }
            }
        }
        
        // Check if all enemies are defeated
        if (enemies.Count == 0)
        {
            EndCombat(true);
        }
    }
    
    private Weapon GetEquippedWeapon()
    {
        // For now, return the melee weapon if available
        return playerCharacter.weapons.meleeWeapon;
    }
    
    public void EndTurn()
    {
        if (!isPlayerTurn) return;
        
        isPlayerTurn = false;
        
        Debug.Log("Player ended turn. Enemy's turn now.");
        
        // Update UI
        if (combatUI != null)
        {
            combatUI.UpdateCombatUI();
        }
        
        // Trigger enemy turn
        StartCoroutine(EnemyTurn());
        
        OnTurnEnded?.Invoke();
    }
    
    private System.Collections.IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f); // Brief pause
        
        // All enemies take their actions
        foreach (Enemy enemy in enemies)
        {
            if (enemy.IsAlive())
            {
                // Enemy action
                int damage = enemy.GetAttackDamage();
                playerCharacter.TakeDamage(damage, DamageType.Physical);
                
                Debug.Log($"{enemy.enemyName} attacks for {damage} damage!");
                
                // Check if player is defeated
                if (playerCharacter.currentHealth <= 0)
                {
                    EndCombat(false);
                    yield break;
                }
                
                yield return new WaitForSeconds(0.5f); // Brief pause between enemy actions
            }
        }
        
        // Start player turn
        isPlayerTurn = true;
        
        // Restore mana based on recovery per turn (not to maximum)
        int manaToRestore = playerCharacter.GetManaRecoveryPerTurn();
        playerCharacter.RestoreMana(manaToRestore);
        
        // Set new enemy intents for next turn
        foreach (Enemy enemy in enemies)
        {
            enemy.SetIntent();
        }
        
        // Draw cards based on character's draw rate
        int cardsToDraw = playerCharacter.GetCardsDrawnPerTurn();
        DrawCards(cardsToDraw);
        
        Debug.Log($"Player turn started. Mana restored by {manaToRestore} to {playerCharacter.mana}/{playerCharacter.maxMana}. Drew {cardsToDraw} cards.");
        
        // Update UI
        if (combatUI != null)
        {
            combatUI.UpdateCombatUI();
        }
    }
    
    private void EndCombat(bool playerWon)
    {
        combatActive = false;
        
        if (playerWon)
        {
            Debug.Log("Combat won! Enemy defeated.");
            // Add experience, rewards, etc.
            playerCharacter.AddExperience(50);
        }
        else
        {
            Debug.Log("Combat lost! Player defeated.");
        }
        
        OnCombatEnded?.Invoke();
    }
    
    public void DrawCard()
    {
        DrawCards(1);
    }
    
    public List<Card> GetCurrentHand()
    {
        return currentHand;
    }
    
    public int GetDrawPileCount()
    {
        return drawPile.Count;
    }
    
    public int GetDiscardPileCount()
    {
        return discardPile.Count;
    }
    
    // Test method to demonstrate mana recovery system
    [ContextMenu("Test Mana Recovery System")]
    public void TestManaRecoverySystem()
    {
        Debug.Log("=== Testing Mana Recovery and Card Drawing Systems ===");
        
        // Test 1: Level 1 character (3 max mana, 3 recovery per turn, 1 card per turn)
        Character testChar1 = new Character("TestLevel1", "Marauder");
        Debug.Log($"Level 1: Max Mana = {testChar1.maxMana}, Recovery = {testChar1.manaRecoveryPerTurn}/turn, Cards = {testChar1.cardsDrawnPerTurn}/turn");
        
        // Test 2: Level 51 character with increased max mana (8 max mana, still 3 recovery per turn, still 1 card per turn)
        Character testChar2 = new Character("TestLevel51", "Marauder");
        testChar2.maxMana = 8; // Increased through leveling/items
        Debug.Log($"Level 51: Max Mana = {testChar2.maxMana}, Recovery = {testChar2.manaRecoveryPerTurn}/turn, Cards = {testChar2.cardsDrawnPerTurn}/turn");
        
        // Test 3: Level 51 character with increased mana recovery and card draw (8 max mana, 4 recovery per turn, 2 cards per turn)
        Character testChar3 = new Character("TestLevel51Enhanced", "Marauder");
        testChar3.maxMana = 8;
        testChar3.AddManaRecoveryPerTurn(1); // Increased through passive skills
        testChar3.AddCardsDrawnPerTurn(1); // Increased through passive skills
        Debug.Log($"Level 51 Enhanced: Max Mana = {testChar3.maxMana}, Recovery = {testChar3.manaRecoveryPerTurn}/turn, Cards = {testChar3.cardsDrawnPerTurn}/turn");
        
        Debug.Log("=== Mana Recovery and Card Drawing Systems Test Complete ===");
    }
}
