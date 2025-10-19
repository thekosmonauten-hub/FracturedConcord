using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// REFACTORED: SimpleCombatUI now uses CardRuntimeManager for all card display.
/// This ensures consistent card appearance and behavior across all scenes.
/// 
/// Migration from old SimpleCombatUI:
/// - Removed custom card creation logic
/// - Uses CardRuntimeManager.Instance for pooling
/// - Consistent scaling with Combat Scene
/// - Same public API for easy drop-in replacement
/// </summary>
public class SimpleCombatUI_Refactored : MonoBehaviour
{
    [Header("UI References")]
    public Canvas combatCanvas;
    public Text deckCountText;
    public Text discardCountText;
    public Button drawCardButton;
    public Button endTurnButton;
    
    [Header("Card Display - Uses CardRuntimeManager")]
    [Tooltip("CardRuntimeManager handles all card creation and pooling")]
    public CardRuntimeManager cardRuntimeManager;
    
    [Header("Card Settings")]
    public int maxCardsInHand = 5;
    
    [Header("Test Configuration")]
    public bool loadTestCardsOnStart = true;
    public bool shuffleDeckOnStart = true;
    
    // Card collections
    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    
    // Card instances in hand (managed by CardRuntimeManager)
    private List<GameObject> cardInstances = new List<GameObject>();
    
    private void Start()
    {
        InitializeUI();
        
        // Auto-find CardRuntimeManager if not assigned
        if (cardRuntimeManager == null)
        {
            cardRuntimeManager = CardRuntimeManager.Instance;
            if (cardRuntimeManager == null)
            {
                Debug.LogError("SimpleCombatUI: CardRuntimeManager not found! Please add CardRuntimeManager to the scene.");
                return;
            }
        }
        
        if (loadTestCardsOnStart)
        {
            LoadTestDeck();
        }
        
        if (shuffleDeckOnStart)
        {
            ShuffleDeck();
        }
        
        // Draw initial hand
        DrawInitialHand();
    }
    
    private void InitializeUI()
    {
        if (drawCardButton != null)
        {
            drawCardButton.onClick.AddListener(OnDrawCardClicked);
        }
        
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }
        
        UpdateDeckCount();
    }
    
    private void LoadTestDeck()
    {
        // Load test cards from CardDatabase
        CardDatabase database = CardDatabase.Instance;
        if (database == null)
        {
            Debug.LogWarning("CardDatabase not found! Creating simple test deck.");
            CreateSimpleTestDeck();
            return;
        }
        
        // Get 10 random cards from database
        for (int i = 0; i < 10; i++)
        {
            CardData randomCard = database.GetRandomCard();
            if (randomCard != null)
            {
                deck.Add(randomCard);
            }
        }
        
        Debug.Log($"Loaded test deck with {deck.Count} cards");
    }
    
    private void CreateSimpleTestDeck()
    {
        // Fallback: Create basic cards if no database
        for (int i = 0; i < 10; i++)
        {
            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = $"Test Card {i + 1}";
            card.description = "A test card for development";
            card.playCost = Random.Range(1, 5);
            card.damage = Random.Range(5, 15);
            card.block = Random.Range(0, 10);
            deck.Add(card);
        }
    }
    
    private void ShuffleDeck()
    {
        // Fisher-Yates shuffle
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardData temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
        
        Debug.Log("Deck shuffled");
    }
    
    private void DrawInitialHand()
    {
        // Draw initial cards
        for (int i = 0; i < Mathf.Min(3, maxCardsInHand); i++)
        {
            DrawCard();
        }
    }
    
    private void DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.Log("Deck is empty! Cannot draw card.");
            return;
        }
        
        if (hand.Count >= maxCardsInHand)
        {
            Debug.Log("Hand is full! Cannot draw more cards.");
            return;
        }
        
        // Draw card from deck
        CardData drawnCard = deck[0];
        deck.RemoveAt(0);
        hand.Add(drawnCard);
        
        // Create card using CardRuntimeManager (with pooling!)
        GameObject cardInstance = cardRuntimeManager.CreateCardFromCardData(drawnCard);
        if (cardInstance != null)
        {
            cardInstances.Add(cardInstance);
            Debug.Log($"Drew card: {drawnCard.cardName}");
        }
        else
        {
            Debug.LogError($"Failed to create card visual for: {drawnCard.cardName}");
        }
        
        // CardRuntimeManager handles repositioning automatically
        UpdateDeckCount();
    }
    
    private void OnDrawCardClicked()
    {
        DrawCard();
    }
    
    private void OnEndTurnClicked()
    {
        Debug.Log("End turn clicked - discarding hand and redrawing");
        
        // Discard current hand
        foreach (GameObject cardInstance in cardInstances)
        {
            if (cardInstance != null)
            {
                cardRuntimeManager.ReturnCardToPool(cardInstance);
            }
        }
        cardInstances.Clear();
        
        // Move hand to discard pile
        discardPile.AddRange(hand);
        hand.Clear();
        
        // If deck is empty, reshuffle discard pile
        if (deck.Count == 0 && discardPile.Count > 0)
        {
            deck.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }
        
        // Draw new hand
        DrawInitialHand();
        UpdateDeckCount();
    }
    
    private void OnCardClicked(CardData cardData)
    {
        Debug.Log($"Card clicked: {cardData.cardName}");
        
        // Find and remove the card
        int cardIndex = hand.IndexOf(cardData);
        if (cardIndex >= 0)
        {
            // Remove from hand
            hand.RemoveAt(cardIndex);
            
            // Return visual to pool
            if (cardIndex < cardInstances.Count)
            {
                GameObject cardInstance = cardInstances[cardIndex];
                cardRuntimeManager.ReturnCardToPool(cardInstance);
                cardInstances.RemoveAt(cardIndex);
            }
            
            // Add to discard pile
            discardPile.Add(cardData);
            
            Debug.Log($"Played card: {cardData.cardName}");
            UpdateDeckCount();
        }
    }
    
    private void UpdateDeckCount()
    {
        if (deckCountText != null)
        {
            deckCountText.text = $"Deck: {deck.Count}";
        }
        
        if (discardCountText != null)
        {
            discardCountText.text = $"Discard: {discardPile.Count}";
        }
    }
    
    #region Context Menu Debug Commands
    
    [ContextMenu("Draw Card")]
    public void DebugDrawCard()
    {
        DrawCard();
    }
    
    [ContextMenu("Clear Hand")]
    public void DebugClearHand()
    {
        foreach (GameObject cardInstance in cardInstances)
        {
            if (cardInstance != null)
            {
                cardRuntimeManager.ReturnCardToPool(cardInstance);
            }
        }
        cardInstances.Clear();
        hand.Clear();
        UpdateDeckCount();
    }
    
    [ContextMenu("Reshuffle Deck")]
    public void DebugReshuffle()
    {
        // Return all cards to deck
        deck.AddRange(hand);
        deck.AddRange(discardPile);
        hand.Clear();
        discardPile.Clear();
        
        // Clear visuals
        foreach (GameObject cardInstance in cardInstances)
        {
            if (cardInstance != null)
            {
                cardRuntimeManager.ReturnCardToPool(cardInstance);
            }
        }
        cardInstances.Clear();
        
        ShuffleDeck();
        UpdateDeckCount();
    }
    
    [ContextMenu("Show Deck Stats")]
    public void ShowDeckStats()
    {
        Debug.Log($"Deck: {deck.Count} cards\nHand: {hand.Count} cards\nDiscard: {discardPile.Count} cards\nTotal: {deck.Count + hand.Count + discardPile.Count} cards");
    }
    
    #endregion
}

