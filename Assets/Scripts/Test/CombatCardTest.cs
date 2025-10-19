using UnityEngine;

public class CombatCardTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestOnStart = false;
    public AnimatedCombatUI cardManager; // Updated to use AnimatedCombatUI
    
    void Start()
    {
        if (runTestOnStart)
        {
            TestCardSystem();
        }
    }
    
    [ContextMenu("Test Card System")]
    public void TestCardSystem()
    {
        if (cardManager == null)
        {
            cardManager = FindFirstObjectByType<AnimatedCombatUI>();
        }
        
        if (cardManager != null)
        {
            Debug.Log("=== Testing Combat Card System ===");
            Debug.Log("AnimatedCombatUI found");
            Debug.Log("Note: AnimatedCombatUI uses different API than SimpleCombatUI");
            Debug.Log("=== Test Complete ===");
        }
        else
        {
            Debug.LogError("AnimatedCombatUI not found! Make sure it's added to the scene.");
        }
    }
    
    [ContextMenu("Draw Test Card")]
    public void DrawTestCard()
    {
        Debug.LogWarning("Draw card functionality moved to AnimatedCombatUI - use combat system directly");
    }
    
    [ContextMenu("Shuffle Deck")]
    public void ShuffleDeck()
    {
        Debug.LogWarning("Shuffle functionality managed by CombatDeckManager");
    }
    
    [ContextMenu("Check Database Status")]
    public void CheckDatabaseStatus()
    {
        CardDatabase database = CardDatabase.Instance;
        if (database != null)
        {
            Debug.Log($"CardDatabase found with {database.allCards.Count} cards");
        }
        else
        {
            Debug.LogError("CardDatabase not found! Make sure it's in the Resources folder.");
        }
    }
}
