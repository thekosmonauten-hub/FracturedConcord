using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dexiled.Data.Items;
using System.Collections.Generic;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Manages adding embossing slots to cards using Inscription Seals
    /// Handles button interaction, cost validation, and card updates
    /// </summary>
    public class EmbossingSlotManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button addSlotButton;
        [SerializeField] private InscriptionSealDisplay currencyDisplay;
        [SerializeField] private CardCarouselUI cardCarousel;
        
        [Header("Cost Settings")]
        [Tooltip("Cost per embossing slot (increases with each slot)")]
        [SerializeField] private int[] slotCosts = { 5, 10, 20, 40, 80 }; // Cost for slots 1-5
        
        [Header("UI Feedback")]
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI slotInfoText;
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color unaffordableColor = Color.red;
        
        private Card selectedCard;
        private int currentCost = 0;
        
        void Start()
        {
            // Wire up button
            if (addSlotButton != null)
            {
                addSlotButton.onClick.AddListener(OnAddSlotClicked);
            }
            
            // Auto-find references if not assigned
            if (currencyDisplay == null)
            {
                currencyDisplay = GetComponentInChildren<InscriptionSealDisplay>();
            }
            
            if (cardCarousel == null)
            {
                cardCarousel = FindFirstObjectByType<CardCarouselUI>();
            }
            
            UpdateUI();
        }
        
        void Update()
        {
            // Update UI when selected card changes
            Card newSelectedCard = GetSelectedCard();
            if (newSelectedCard != selectedCard)
            {
                selectedCard = newSelectedCard;
                UpdateUI();
            }
        }
        
        /// <summary>
        /// Handle add slot button click
        /// </summary>
        private void OnAddSlotClicked()
        {
            if (selectedCard == null)
            {
                Debug.LogWarning("[EmbossingSlotManager] No card selected");
                ShowFeedback("No card selected!");
                return;
            }
            
            // Check if card can have more slots
            if (selectedCard.embossingSlots >= 5)
            {
                Debug.LogWarning("[EmbossingSlotManager] Card already has maximum embossing slots (5)");
                ShowFeedback("Card has maximum slots!");
                return;
            }
            
            // Calculate cost
            int cost = GetCostForNextSlot(selectedCard);
            
            // Check if player has enough currency
            if (LootManager.Instance != null)
            {
                int playerCurrency = LootManager.Instance.GetCurrencyAmount(CurrencyType.InscriptionSeal);
                
                if (playerCurrency < cost)
                {
                    Debug.LogWarning($"[EmbossingSlotManager] Not enough Inscription Seals. Need {cost}, have {playerCurrency}");
                    ShowFeedback($"Need {cost} Inscription Seals!");
                    return;
                }
                
                // Deduct currency
                bool success = LootManager.Instance.RemoveCurrency(CurrencyType.InscriptionSeal, cost);
                if (!success)
                {
                    Debug.LogError("[EmbossingSlotManager] Failed to remove currency");
                    ShowFeedback("Transaction failed!");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("[EmbossingSlotManager] LootManager not available, adding slot anyway");
            }
            
            // Add the slot
            selectedCard.embossingSlots++;
            
            Debug.Log($"[EmbossingSlotManager] Added embossing slot to '{selectedCard.cardName}'. Now has {selectedCard.embossingSlots} slots");
            ShowFeedback($"Added slot! ({selectedCard.embossingSlots}/5)");
            
            // Update active deck with new card data
            UpdateCardInActiveDeck(selectedCard);
            
            // Update UI
            UpdateUI();
            
            // Refresh currency display
            if (currencyDisplay != null)
            {
                currencyDisplay.UpdateDisplay();
            }
            
            // Refresh card display
            RefreshCardDisplay();
        }
        
        /// <summary>
        /// Get the cost for the next embossing slot
        /// </summary>
        private int GetCostForNextSlot(Card card)
        {
            if (card == null) return 0;
            
            int currentSlots = card.embossingSlots;
            
            // Cap at 5 slots
            if (currentSlots >= 5) return int.MaxValue;
            
            // Return cost for next slot
            if (currentSlots < slotCosts.Length)
            {
                return slotCosts[currentSlots];
            }
            
            // Fallback: exponential cost
            return 5 * (int)Mathf.Pow(2, currentSlots);
        }
        
        /// <summary>
        /// Update UI based on selected card and currency
        /// </summary>
        private void UpdateUI()
        {
            if (selectedCard == null)
            {
                // No card selected
                if (addSlotButton != null)
                    addSlotButton.interactable = false;
                
                if (costText != null)
                    costText.text = "-";
                
                if (slotInfoText != null)
                    slotInfoText.text = "No card selected";
                
                return;
            }
            
            // Check if card is at max slots
            if (selectedCard.embossingSlots >= 5)
            {
                if (addSlotButton != null)
                    addSlotButton.interactable = false;
                
                if (costText != null)
                    costText.text = "MAX";
                
                if (slotInfoText != null)
                    slotInfoText.text = $"{selectedCard.embossingSlots}/5 (Max)";
                
                return;
            }
            
            // Calculate cost
            int cost = GetCostForNextSlot(selectedCard);
            currentCost = cost;
            
            // Check affordability
            bool canAfford = false;
            if (LootManager.Instance != null)
            {
                int playerCurrency = LootManager.Instance.GetCurrencyAmount(CurrencyType.InscriptionSeal);
                canAfford = playerCurrency >= cost;
            }
            
            // Update button
            if (addSlotButton != null)
                addSlotButton.interactable = canAfford;
            
            // Update cost text
            if (costText != null)
            {
                costText.text = cost.ToString();
                costText.color = canAfford ? affordableColor : unaffordableColor;
            }
            
            // Update slot info
            if (slotInfoText != null)
            {
                slotInfoText.text = $"{selectedCard.embossingSlots}/5 (+1 for {cost})";
            }
            
            // Update currency display with requirement
            if (currencyDisplay != null)
            {
                currencyDisplay.UpdateDisplayWithRequirement(cost);
            }
        }
        
        /// <summary>
        /// Get currently selected card from carousel
        /// </summary>
        private Card GetSelectedCard()
        {
            if (cardCarousel == null) return null;
            
            // Get selected card from carousel
            int selectedIndex = cardCarousel.GetCurrentCardIndex();
            if (selectedIndex < 0) return null;
            
            Card card = cardCarousel.GetCardAtIndex(selectedIndex);
            return card;
        }
        
        /// <summary>
        /// Update the card in the active deck
        /// </summary>
        private void UpdateCardInActiveDeck(Card card)
        {
            if (card == null) return;
            
            // Get DeckManager and update the card in active deck
            if (DeckManager.Instance != null)
            {
                // Get all cards from active deck
                List<Card> deckCards = DeckManager.Instance.GetActiveDeckAsCards();
                
                if (deckCards != null && deckCards.Count > 0)
                {
                    // Find and update all cards with matching groupKey
                    string groupKey = card.GetGroupKey();
                    bool updated = false;
                    
                    foreach (Card deckCard in deckCards)
                    {
                        if (deckCard.GetGroupKey() == groupKey)
                        {
                            deckCard.embossingSlots = card.embossingSlots;
                            updated = true;
                        }
                    }
                    
                    if (updated)
                    {
                        // Note: Cards are references, changes are already saved in the active deck
                        // Force save the active deck
                        DeckPreset activeDeck = DeckManager.Instance.GetActiveDeck();
                        if (activeDeck != null)
                        {
                            DeckManager.Instance.SaveDeck(activeDeck);
                        }
                        Debug.Log($"[EmbossingSlotManager] Updated {groupKey} cards in active deck");
                    }
                }
            }
        }
        
        /// <summary>
        /// Refresh card display in carousel
        /// </summary>
        private void RefreshCardDisplay()
        {
            if (cardCarousel != null)
            {
                // Force carousel to refresh the card display
                // This will update the embossing slot visuals
                cardCarousel.RefreshCurrentCard();
            }
        }
        
        /// <summary>
        /// Show feedback message to player
        /// </summary>
        private void ShowFeedback(string message)
        {
            Debug.Log($"[EmbossingSlotManager] Feedback: {message}");
            // TODO: Show UI feedback (toast, popup, etc.)
        }
        
        #region Public Accessors
        
        public Card GetCurrentSelectedCard()
        {
            return selectedCard;
        }
        
        public int GetCurrentCost()
        {
            return currentCost;
        }
        
        public bool CanAddSlot()
        {
            if (selectedCard == null) return false;
            if (selectedCard.embossingSlots >= 5) return false;
            
            int cost = GetCostForNextSlot(selectedCard);
            if (LootManager.Instance != null)
            {
                return LootManager.Instance.GetCurrencyAmount(CurrencyType.InscriptionSeal) >= cost;
            }
            
            return false;
        }
        
        #endregion
    }
}

