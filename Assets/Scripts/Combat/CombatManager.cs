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

	// Optional bridge to UI-driven combat display manager (UGUI path)
	private CombatDisplayManager cachedDisplayManager;
    
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
        
        // Priority 1: Check DeckManager for active deck (from DeckBuilder)
        if (DeckManager.Instance != null && DeckManager.Instance.HasActiveDeck())
        {
            List<Card> deckCards = DeckManager.Instance.GetActiveDeckAsCards();
            drawPile.AddRange(deckCards);
            Debug.Log($"Loaded active deck from DeckManager with {deckCards.Count} cards");
        }
        // Priority 2: Fall back to Character's current deck (legacy)
        else if (playerCharacter.currentDeck != null)
        {
            drawPile.AddRange(playerCharacter.currentDeck.cards);
            Debug.Log($"Loaded deck from Character.currentDeck with {playerCharacter.currentDeck.cards.Count} cards");
        }
        // Priority 3: Create starter deck if none exists
        else
        {
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
        
        // Check if this card should be prepared instead of played immediately
        CardDataExtended extendedCard = card.sourceCardData;
        if (extendedCard != null && extendedCard.canPrepare)
        {
            // Check if player wants to prepare (could add UI prompt here)
            // For now, check for "Prepare" tag or automatically prepare
            if (card.tags != null && card.tags.Contains("Prepare"))
            {
                var prepManager = PreparationManager.Instance;
                if (prepManager != null && prepManager.CanPrepareCard(extendedCard))
                {
                    // Remove from hand
                    currentHand.Remove(card);
                    
                    // Spend mana
                    if (!playerCharacter.UseMana(card.manaCost))
                    {
                        Debug.LogWarning($"[CombatManager] Failed to spend mana for preparing {card.cardName}");
                        currentHand.Add(card);
                        return;
                    }
                    
                    // Prepare the card
                    bool prepared = prepManager.PrepareCard(extendedCard, playerCharacter);
                    
                    if (prepared)
                    {
                        Debug.Log($"<color=cyan>[CombatManager] Prepared card: {card.cardName}</color>");
                        
                        // Update UI
                        if (combatUI != null)
                        {
                            combatUI.UpdateCombatUI();
                        }
                        
                        // Trigger event
                        OnCardPlayed?.Invoke(card);
                        return;
                    }
                    else
                    {
                        // Failed to prepare, refund mana and return card to hand
                        playerCharacter.RestoreMana(card.manaCost);
                        currentHand.Add(card);
                        Debug.LogWarning($"[CombatManager] Failed to prepare {card.cardName}");
                        return;
                    }
                }
            }
        }
        
        // Check if this is an "Unleash" card that triggers prepared cards
        if (card.tags != null && card.tags.Contains("Unleash"))
        {
            var prepManager = PreparationManager.Instance;
            if (prepManager != null)
            {
                // Remove from hand
                currentHand.Remove(card);
                
                // Spend mana
                if (!playerCharacter.UseMana(card.manaCost))
                {
                    Debug.LogWarning($"[CombatManager] Failed to spend mana for unleash card {card.cardName}");
                    currentHand.Add(card);
                    return;
                }
                
                // Unleash all prepared cards (or specific ones based on card effect)
                // For now, unleash all
                var preparedCards = prepManager.GetPreparedCards();
                foreach (var prepared in preparedCards.ToList())
                {
                    prepManager.UnleashCardFree(prepared, playerCharacter, false);
                }
                
                Debug.Log($"<color=yellow>[CombatManager] Unleashed {preparedCards.Count} prepared cards!</color>");
                
                // Add to discard pile
                discardPile.Add(card);
                
                // Update UI
                if (combatUI != null)
                {
                    combatUI.UpdateCombatUI();
                }
                
                // Trigger event
                OnCardPlayed?.Invoke(card);
                return;
            }
        }
        
        // Normal card play logic
        // Remove from hand
        currentHand.Remove(card);
        
        // Spend mana
        playerCharacter.UseMana(card.manaCost);
        
        // Calculate damage
        float damage = DamageCalculator.CalculateCardDamage(card, playerCharacter, GetEquippedWeapon());
        
        // Handle AoE vs single target
        Debug.Log($"[PlayCard] {card.cardName} isAoE={card.isAoE} aoeTargets={card.aoeTargets}");
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
        // Prefer CombatDisplayManager if present (UGUI path), otherwise use internal enemies list
		var dm = GetDisplayManager();
		if (dm != null)
		{
            int targetIdx = selectedEnemyIndex;
            var targetingMgr = EnemyTargetingManager.Instance;
            var active = dm.GetActiveEnemies();
            int total = active != null ? active.Count : 0;
            if (targetingMgr != null)
            {
                targetIdx = Mathf.Clamp(targetingMgr.GetTargetedEnemyIndex(), 0, Mathf.Max(0, total - 1));
            }
            if (targetIdx >= 0 && targetIdx < total && active[targetIdx] != null && active[targetIdx].currentHealth > 0)
            {
                dm.PlayerAttackEnemy(targetIdx, damage);
                Debug.Log($"{playerCharacter.characterName} played {card.cardName} for {damage:F1} damage on enemy index {targetIdx} (DisplayManager)");
            }
            else
            {
                Debug.LogWarning($"No valid target index {targetIdx} for card (DisplayManager)! total={total}");
            }
			return;
		}

        // Apply damage to selected enemy (internal path)
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
        var dm = GetDisplayManager();
        int targetsHit = 0;

        if (dm != null)
        {
            // Use display manager's active enemies and apply adjacency/all logic
            var active = dm.GetActiveEnemies();
            int total = active != null ? active.Count : 0;
            if (total == 0)
            {
                Debug.LogWarning("No enemies available in DisplayManager for AoE.");
                return;
            }

            // Build alive flags
            System.Func<int, bool> isAliveAt = (idx) => idx >= 0 && idx < total && active[idx] != null && active[idx].currentHealth > 0;

            System.Collections.Generic.List<int> indices = new System.Collections.Generic.List<int>();
            if (card.aoeTargets <= 0)
            {
                for (int i = 0; i < total; i++) if (isAliveAt(i)) indices.Add(i);
            }
            else
            {
                // adjacency starting from targeted enemy index (sync with EnemyTargetingManager)
                int start = selectedEnemyIndex;
                var targetingMgr = EnemyTargetingManager.Instance;
                if (targetingMgr != null)
                {
                    start = Mathf.Clamp(targetingMgr.GetTargetedEnemyIndex(), 0, Mathf.Max(0, total - 1));
                }
                if (!isAliveAt(start))
                {
                    // find nearest alive
                    int best = -1; int bestDist = int.MaxValue;
                    for (int i = 0; i < total; i++)
                    {
                        if (!isAliveAt(i)) continue;
                        int d = Mathf.Abs(i - selectedEnemyIndex);
                        if (d < bestDist) { best = i; bestDist = d; }
                    }
                    start = best >= 0 ? best : 0;
                }
                if (isAliveAt(start)) indices.Add(start);

                for (int d = 1; indices.Count < card.aoeTargets; d++)
                {
                    bool any = false;
                    int left = start - d;
                    if (isAliveAt(left) && !indices.Contains(left)) { indices.Add(left); any = true; if (indices.Count == card.aoeTargets) break; }
                    int right = start + d;
                    if (isAliveAt(right) && !indices.Contains(right)) { indices.Add(right); any = true; }
                    if (!any && left < 0 && right >= total) break;
                }
                for (int i = 0; indices.Count < card.aoeTargets && i < total; i++)
                {
                    if (isAliveAt(i) && !indices.Contains(i)) indices.Add(i);
                }
            }

            Debug.Log($"[AoE] Using DisplayManager with total={total}, selected={selectedEnemyIndex}, targetsResolved=[{string.Join(",", indices)}]");

            // Apply via display manager so UI updates correctly
            for (int n = 0; n < indices.Count; n++)
            {
                int idx = indices[n];
                if (isAliveAt(idx))
                {
                    dm.PlayerAttackEnemy(idx, damage);
                    targetsHit++;
                }
            }
        }
        else
        {
            // Internal enemies list path
            // Determine AoE targets using adjacency around selected enemy.
            List<int> targetIndices = GetAoETargetIndices(card.aoeTargets, selectedEnemyIndex);
            Debug.Log($"[AoE] Internal path, targetsResolved=[{string.Join(",", targetIndices)}]");
            for (int t = 0; t < targetIndices.Count; t++)
            {
                int i = targetIndices[t];
                if (i >= 0 && i < enemies.Count && enemies[i] != null && enemies[i].IsAlive())
                {
                    enemies[i].TakeDamage(damage);
                    targetsHit++;
                    Debug.Log($"{playerCharacter.characterName} played {card.cardName} for {damage:F1} damage on {enemies[i].enemyName}!");
                }
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

    /// <summary>
    /// Compute which enemies should be hit by an AoE effect.
    /// If requestedTargets <= 0, returns all alive enemies.
    /// Otherwise returns a set chosen by adjacency around centerIndex (center, -1, +1, -2, +2, ...).
    /// </summary>
    private List<int> GetAoETargetIndices(int requestedTargets, int centerIndex)
    {
        List<int> result = new List<int>();

        // Collect alive enemy indices
        List<int> alive = new List<int>();
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null && enemies[i].IsAlive())
            {
                alive.Add(i);
            }
        }

        if (alive.Count == 0)
        {
            return result;
        }

        // Non-positive means hit all in scope
        if (requestedTargets <= 0)
        {
            result.AddRange(alive);
            return result;
        }

        // Ensure starting index is alive; if not, find nearest alive
        int start = centerIndex;
        if (!alive.Contains(start))
        {
            int best = alive[0];
            int bestDist = Mathf.Abs(best - centerIndex);
            for (int j = 1; j < alive.Count; j++)
            {
                int idx = alive[j];
                int d = Mathf.Abs(idx - centerIndex);
                if (d < bestDist)
                {
                    best = idx;
                    bestDist = d;
                }
            }
            start = best;
        }

        result.Add(start);

        // Expand outwards: left, right, left2, right2, ...
        for (int d = 1; result.Count < requestedTargets; d++)
        {
            bool addedAny = false;

            int left = start - d;
            if (left >= 0 && alive.Contains(left) && !result.Contains(left))
            {
                result.Add(left);
                addedAny = true;
                if (result.Count == requestedTargets) break;
            }

            int right = start + d;
            if (right < enemies.Count && alive.Contains(right) && !result.Contains(right))
            {
                result.Add(right);
                addedAny = true;
            }

            if (!addedAny && left < 0 && right >= enemies.Count)
            {
                break;
            }
        }

        // If still short, fill remaining from alive left-to-right
        for (int k = 0; result.Count < requestedTargets && k < alive.Count; k++)
        {
            if (!result.Contains(alive[k]))
            {
                result.Add(alive[k]);
            }
        }

        return result;
    }

    /// <summary>
    /// Enemy row classification helper for future use.
    /// For 4–5 enemies: indices 0..2 Back, 3..4 Front. For ≤3, treat as Both.
    /// </summary>
    public enum EnemyRow { Front, Back, Both }

    public static EnemyRow GetEnemyRow(int zeroBasedIndex, int totalEnemies)
    {
        if (totalEnemies >= 4)
        {
            if (zeroBasedIndex <= 2) return EnemyRow.Back;
            return EnemyRow.Front;
        }
        return EnemyRow.Both;
    }
    
    private Weapon GetEquippedWeapon()
    {
        // For now, return the melee weapon if available
        return playerCharacter.weapons.meleeWeapon;
    }

	private CombatDisplayManager GetDisplayManager()
	{
		if (cachedDisplayManager == null)
		{
			cachedDisplayManager = FindFirstObjectByType<CombatDisplayManager>();
		}
		return cachedDisplayManager;
	}
    
    public void EndTurn()
    {
        if (!isPlayerTurn) return;
        
        isPlayerTurn = false;
        
        Debug.Log("Player ended turn. Enemy's turn now.");
        
        // Update prepared cards (accumulate bonuses, check for auto-unleash)
        var prepManager = PreparationManager.Instance;
        if (prepManager != null)
        {
            prepManager.OnTurnEnd();
        }
        
        // Update UI
        if (combatUI != null)
        {
            combatUI.UpdateCombatUI();
        }
        
        // Trigger enemy turn
        StartCoroutine(EnemyTurn());
        
        OnTurnEnded?.Invoke();
        TemporaryStatSystem.Instance?.AdvanceTurn();
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
            
            // Add experience to character and cards
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.AddExperience(50, shareWithCards: true);
                
                // Auto-save after victory
                CharacterManager.Instance.SaveCharacter();
                Debug.Log("[Auto-Save] Character saved after combat victory.");
            }
        }
        else
        {
            Debug.Log("Combat lost! Player defeated.");
            // Auto-save after defeat (preserve progress up to this point)
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.SaveCharacter();
                Debug.Log("[Auto-Save] Character saved after combat defeat.");
            }
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
