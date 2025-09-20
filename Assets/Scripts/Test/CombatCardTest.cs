using UnityEngine;

public class CombatCardTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestOnStart = false;
    public SimpleCombatUI cardManager;
    
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
            cardManager = FindFirstObjectByType<SimpleCombatUI>();
        }
        
        if (cardManager != null)
        {
            Debug.Log("=== Testing Combat Card System ===");
            Debug.Log($"Deck count: {cardManager.GetDeckCount()}");
            Debug.Log($"Hand count: {cardManager.GetHandCount()}");
            Debug.Log($"Discard count: {cardManager.GetDiscardCount()}");
            Debug.Log("=== Test Complete ===");
        }
        else
        {
            Debug.LogError("SimpleCombatUI not found! Make sure it's added to the scene.");
        }
    }
    
    [ContextMenu("Draw Test Card")]
    public void DrawTestCard()
    {
        if (cardManager == null)
        {
            cardManager = FindFirstObjectByType<SimpleCombatUI>();
        }
        
        if (cardManager != null)
        {
            cardManager.DrawCardPublic();
        }
        else
        {
            Debug.LogError("SimpleCombatUI not found!");
        }
    }
    
    [ContextMenu("Shuffle Deck")]
    public void ShuffleDeck()
    {
        if (cardManager == null)
        {
            cardManager = FindFirstObjectByType<SimpleCombatUI>();
        }
        
        if (cardManager != null)
        {
            cardManager.ShuffleDeckPublic();
        }
        else
        {
            Debug.LogError("SimpleCombatUI not found!");
        }
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
