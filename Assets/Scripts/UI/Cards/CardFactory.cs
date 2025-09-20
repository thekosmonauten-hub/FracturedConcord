using UnityEngine;
using UnityEngine.UI;

public static class CardFactory
{
    private static GameObject cardPrefab;
    private static CardVisualAssets visualAssets;
    
    static CardFactory()
    {
        LoadResources();
    }
    
    private static void LoadResources()
    {
        // Load the card prefab
        cardPrefab = Resources.Load<GameObject>("CardPrefab");
        if (cardPrefab == null)
        {
            Debug.LogWarning("CardPrefab not found in Resources folder! Make sure to move your card prefab to a Resources folder.");
        }
        
        // Load visual assets
        visualAssets = Resources.Load<CardVisualAssets>("CardVisualAssets");
        if (visualAssets == null)
        {
            Debug.LogWarning("CardVisualAssets not found in Resources folder! Run SetupCardDatabase.CreateCardVisualAssets() to create it.");
        }
    }
    
    public static GameObject CreateCard(CardData cardData, Transform parent = null)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card prefab not loaded!");
            return null;
        }
        
        if (cardData == null)
        {
            Debug.LogError("Card data is null!");
            return null;
        }
        
        // Instantiate the card prefab
        GameObject cardInstance = Object.Instantiate(cardPrefab, parent);
        
        // Get the CardVisualManager component
        CardVisualManager visualManager = cardInstance.GetComponent<CardVisualManager>();
        if (visualManager == null)
        {
            // Add the component if it doesn't exist
            visualManager = cardInstance.AddComponent<CardVisualManager>();
        }
        
        // Update the card visuals
        visualManager.UpdateCardVisuals(cardData);
        
        // Set the card name for identification
        cardInstance.name = $"Card_{cardData.cardName}";
        
        return cardInstance;
    }
    
    public static GameObject CreateRandomCard(Transform parent = null)
    {
        CardData randomCard = CardDatabase.Instance.GetRandomCard();
        if (randomCard == null)
        {
            Debug.LogWarning("No cards available in database!");
            return null;
        }
        
        return CreateCard(randomCard, parent);
    }
    
    public static GameObject CreateCardByCategory(CardCategory category, Transform parent = null)
    {
        CardData card = CardDatabase.Instance.GetRandomCardByCategory(category);
        if (card == null)
        {
            Debug.LogWarning($"No cards available for category: {category}");
            return null;
        }
        
        return CreateCard(card, parent);
    }
    
    public static GameObject CreateCardByElement(CardElement element, Transform parent = null)
    {
        CardData card = CardDatabase.Instance.GetRandomCardByElement(element);
        if (card == null)
        {
            Debug.LogWarning($"No cards available for element: {element}");
            return null;
        }
        
        return CreateCard(card, parent);
    }
    
    public static GameObject CreateCardByRarity(CardRarity rarity, Transform parent = null)
    {
        CardData card = CardDatabase.Instance.GetRandomCardByRarity(rarity);
        if (card == null)
        {
            Debug.LogWarning($"No cards available for rarity: {rarity}");
            return null;
        }
        
        return CreateCard(card, parent);
    }
    
    public static void UpdateCardVisuals(GameObject cardInstance, CardData cardData)
    {
        if (cardInstance == null || cardData == null) return;
        
        CardVisualManager visualManager = cardInstance.GetComponent<CardVisualManager>();
        if (visualManager != null)
        {
            visualManager.UpdateCardVisuals(cardData);
        }
    }
    
    public static CardData GetCardDataFromInstance(GameObject cardInstance)
    {
        if (cardInstance == null) return null;
        
        CardVisualManager visualManager = cardInstance.GetComponent<CardVisualManager>();
        if (visualManager != null)
        {
            return visualManager.GetCurrentCardData();
        }
        
        return null;
    }
}
