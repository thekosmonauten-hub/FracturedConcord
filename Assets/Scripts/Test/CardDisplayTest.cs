using UnityEngine;

public class CardDisplayTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool createCardsOnStart = true;
    public Transform cardParent;
    public Vector3 cardSpacing = new Vector3(2f, 0f, 0f);
    
    [Header("Test Cards")]
    public CardData[] testCards;
    
    void Start()
    {
        if (createCardsOnStart)
        {
            CreateTestCards();
        }
    }
    
    [ContextMenu("Check Database Status")]
    public void CheckDatabaseStatus()
    {
        CardDatabase database = CardDatabase.Instance;
        if (database != null)
        {
            Debug.Log($"CardDatabase found with {database.allCards.Count} cards");
            Debug.Log($"Categories: Attack={database.attackCards.Count}, Skill={database.skillCards.Count}, Power={database.powerCards.Count}, Guard={database.guardCards.Count}");
            Debug.Log($"Elements: Basic={database.basicCards.Count}, Fire={database.fireCards.Count}, Cold={database.coldCards.Count}, Lightning={database.lightningCards.Count}, Physical={database.physicalCards.Count}, Chaos={database.chaosCards.Count}");
            Debug.Log($"Rarities: Common={database.commonCards.Count}, Magic={database.magicCards.Count}, Rare={database.rareCards.Count}, Unique={database.uniqueCards.Count}");
        }
        else
        {
            Debug.LogError("CardDatabase.Instance is null!");
        }
    }
    
    [ContextMenu("Refresh Card Database")]
    public void RefreshCardDatabase()
    {
        // Find the existing CardDatabase in Resources
        CardDatabase existingDatabase = Resources.Load<CardDatabase>("CardDatabase");
        if (existingDatabase != null)
        {
            // Clear existing cards
            existingDatabase.allCards.Clear();
            
            // Find all CardData assets in the project
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CardData");
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                CardData cardData = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (cardData != null)
                {
                    existingDatabase.allCards.Add(cardData);
                    Debug.Log($"Added card to database: {cardData.cardName}");
                }
            }
            
            // Auto-categorize the cards
            existingDatabase.CategorizeCards();
            
            // Mark as dirty and save
            UnityEditor.EditorUtility.SetDirty(existingDatabase);
            UnityEditor.AssetDatabase.SaveAssets();
            
            Debug.Log($"CardDatabase refreshed with {existingDatabase.allCards.Count} cards");
        }
        else
        {
            Debug.LogError("CardDatabase not found in Resources folder!");
        }
    }
    
    [ContextMenu("Create Test Cards")]
    public void CreateTestCards()
    {
        // Clear existing cards
        if (cardParent != null)
        {
            foreach (Transform child in cardParent)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        // Create cards from database
        CardDatabase database = CardDatabase.Instance;
        if (database != null && database.allCards.Count > 0)
        {
            Debug.Log($"Creating {database.allCards.Count} cards from database...");
            
            for (int i = 0; i < database.allCards.Count; i++)
            {
                CardData cardData = database.allCards[i];
                GameObject cardInstance = CardFactory.CreateCard(cardData, cardParent);
                
                if (cardInstance != null)
                {
                    // Position the card
                    Vector3 position = cardSpacing * i;
                    cardInstance.transform.localPosition = position;
                    
                    Debug.Log($"Created card {i + 1}: {cardData.cardName} at position {position}");
                }
            }
        }
        else
        {
            Debug.LogWarning("No cards found in database! Try running 'Refresh Card Database' first.");
            Debug.LogWarning($"Database instance: {(database != null ? "Found" : "Null")}");
            if (database != null)
            {
                Debug.LogWarning($"Cards in database: {database.allCards.Count}");
            }
        }
    }
    
    [ContextMenu("Create Random Cards")]
    public void CreateRandomCards()
    {
        if (cardParent == null) return;
        
        // Clear existing cards
        foreach (Transform child in cardParent)
        {
            DestroyImmediate(child.gameObject);
        }
        
        // Create 5 random cards
        for (int i = 0; i < 5; i++)
        {
            GameObject cardInstance = CardFactory.CreateRandomCard(cardParent);
            if (cardInstance != null)
            {
                Vector3 position = cardSpacing * i;
                cardInstance.transform.localPosition = position;
                
                CardData cardData = CardFactory.GetCardDataFromInstance(cardInstance);
                Debug.Log($"Created random card {i + 1}: {cardData?.cardName ?? "Unknown"}");
            }
        }
    }
    
    [ContextMenu("Test Card Elements")]
    public void TestCardElements()
    {
        if (cardParent == null) return;
        
        // Clear existing cards
        foreach (Transform child in cardParent)
        {
            DestroyImmediate(child.gameObject);
        }
        
        // Create one card of each element
        CardElement[] elements = { CardElement.Basic, CardElement.Fire, CardElement.Cold, CardElement.Lightning, CardElement.Physical, CardElement.Chaos };
        
        for (int i = 0; i < elements.Length; i++)
        {
            GameObject cardInstance = CardFactory.CreateCardByElement(elements[i], cardParent);
            if (cardInstance != null)
            {
                Vector3 position = cardSpacing * i;
                cardInstance.transform.localPosition = position;
                
                CardData cardData = CardFactory.GetCardDataFromInstance(cardInstance);
                Debug.Log($"Created {elements[i]} card: {cardData?.cardName ?? "Unknown"}");
            }
        }
    }
    
    [ContextMenu("Test Card Rarities")]
    public void TestCardRarities()
    {
        if (cardParent == null) return;
        
        // Clear existing cards
        foreach (Transform child in cardParent)
        {
            DestroyImmediate(child.gameObject);
        }
        
        // Create one card of each rarity
        CardRarity[] rarities = { CardRarity.Common, CardRarity.Magic, CardRarity.Rare, CardRarity.Unique };
        
        for (int i = 0; i < rarities.Length; i++)
        {
            GameObject cardInstance = CardFactory.CreateCardByRarity(rarities[i], cardParent);
            if (cardInstance != null)
            {
                Vector3 position = cardSpacing * i;
                cardInstance.transform.localPosition = position;
                
                CardData cardData = CardFactory.GetCardDataFromInstance(cardInstance);
                Debug.Log($"Created {rarities[i]} card: {cardData?.cardName ?? "Unknown"}");
            }
        }
    }
}
