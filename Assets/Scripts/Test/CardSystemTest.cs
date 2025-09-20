using UnityEngine;

public class CardSystemTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestOnStart = true;
    public Transform testParent;
    
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
        Debug.Log("=== Testing Card System ===");
        
        // Test creating a random card
        GameObject randomCard = CardFactory.CreateRandomCard(testParent);
        if (randomCard != null)
        {
            Debug.Log($"Created random card: {randomCard.name}");
        }
        
        // Test creating cards by category
        TestCardByCategory(CardCategory.Attack);
        TestCardByCategory(CardCategory.Skill);
        TestCardByCategory(CardCategory.Power);
        TestCardByCategory(CardCategory.Guard);
        
        // Test creating cards by element
        TestCardByElement(CardElement.Fire);
        TestCardByElement(CardElement.Cold);
        TestCardByElement(CardElement.Lightning);
        TestCardByElement(CardElement.Physical);
        TestCardByElement(CardElement.Chaos);
        
        // Test creating cards by rarity
        TestCardByRarity(CardRarity.Common);
        TestCardByRarity(CardRarity.Magic);
        TestCardByRarity(CardRarity.Rare);
        TestCardByRarity(CardRarity.Unique);
        
        Debug.Log("=== Card System Test Complete ===");
    }
    
    private void TestCardByCategory(CardCategory category)
    {
        GameObject card = CardFactory.CreateCardByCategory(category, testParent);
        if (card != null)
        {
            CardData cardData = CardFactory.GetCardDataFromInstance(card);
            Debug.Log($"Created {category} card: {cardData?.cardName ?? "Unknown"}");
        }
        else
        {
            Debug.LogWarning($"No {category} cards available");
        }
    }
    
    private void TestCardByElement(CardElement element)
    {
        GameObject card = CardFactory.CreateCardByElement(element, testParent);
        if (card != null)
        {
            CardData cardData = CardFactory.GetCardDataFromInstance(card);
            Debug.Log($"Created {element} card: {cardData?.cardName ?? "Unknown"}");
        }
        else
        {
            Debug.LogWarning($"No {element} cards available");
        }
    }
    
    private void TestCardByRarity(CardRarity rarity)
    {
        GameObject card = CardFactory.CreateCardByRarity(rarity, testParent);
        if (card != null)
        {
            CardData cardData = CardFactory.GetCardDataFromInstance(card);
            Debug.Log($"Created {rarity} card: {cardData?.cardName ?? "Unknown"}");
        }
        else
        {
            Debug.LogWarning($"No {rarity} cards available");
        }
    }
    
    [ContextMenu("Test Card Database")]
    public void TestCardDatabase()
    {
        Debug.Log("=== Testing Card Database ===");
        
        CardDatabase database = CardDatabase.Instance;
        if (database == null)
        {
            Debug.LogError("CardDatabase not found!");
            return;
        }
        
        Debug.Log($"Total cards in database: {database.allCards.Count}");
        Debug.Log($"Attack cards: {database.attackCards.Count}");
        Debug.Log($"Skill cards: {database.skillCards.Count}");
        Debug.Log($"Power cards: {database.powerCards.Count}");
        Debug.Log($"Guard cards: {database.guardCards.Count}");
        
        Debug.Log($"Fire cards: {database.fireCards.Count}");
        Debug.Log($"Cold cards: {database.coldCards.Count}");
        Debug.Log($"Lightning cards: {database.lightningCards.Count}");
        Debug.Log($"Physical cards: {database.physicalCards.Count}");
        Debug.Log($"Chaos cards: {database.chaosCards.Count}");
        
        Debug.Log($"Common cards: {database.commonCards.Count}");
        Debug.Log($"Magic cards: {database.magicCards.Count}");
        Debug.Log($"Rare cards: {database.rareCards.Count}");
        Debug.Log($"Unique cards: {database.uniqueCards.Count}");
        
        Debug.Log("=== Card Database Test Complete ===");
    }
}
