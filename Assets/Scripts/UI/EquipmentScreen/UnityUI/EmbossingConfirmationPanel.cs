using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Confirmation panel for applying embossings to cards
    /// Shows detailed information and requires user confirmation before applying
    /// </summary>
    public class EmbossingConfirmationPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image overlayImage;
        [SerializeField] private RectTransform panelTransform;
        
        [Header("Embossing Info")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI elementText;
        [SerializeField] private TextMeshProUGUI requirementsText;
        [SerializeField] private TextMeshProUGUI effectText;
        [SerializeField] private TextMeshProUGUI manaCostText;
        
        [Header("Target Card Info")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI currentSlotsText;
        [SerializeField] private TextMeshProUGUI slotIndexText;
        
        [Header("Mana Cost Preview")]
        [SerializeField] private TextMeshProUGUI currentCostText;
        [SerializeField] private TextMeshProUGUI newCostText;
        
        [Header("Validation")]
        [SerializeField] private TextMeshProUGUI validationMessageText;
        
        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button closeButton;
        
        [Header("Animation")]
        [SerializeField] private bool useAnimation = true;
        [SerializeField] private float showDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.25f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;
        
        [Header("Colors")]
        [SerializeField] private Color validColor = Color.green;
        [SerializeField] private Color invalidColor = Color.red;
        [SerializeField] private Color warningColor = Color.yellow;
        
        [Header("Auto-Setup")]
        [SerializeField] private bool autoSetup = true;
        
        [Header("Testing")]
        [SerializeField] private bool testMode = false;
        [Tooltip("When enabled, bypasses all requirement checks for testing")]
        
        // State
        private EmbossingEffect currentEmbossing;
        private Card currentCard;
        private int targetSlotIndex;
        private bool isShowing = false;
        private CanvasGroup panelCanvasGroup;
        private Sequence currentAnimation;
        
        void Awake()
        {
            // Auto-assign panelRoot if not set
            if (panelRoot == null)
            {
                // Look for a child Canvas or Panel
                Canvas childCanvas = GetComponentInChildren<Canvas>(true);
                if (childCanvas != null && childCanvas.gameObject != gameObject)
                {
                    panelRoot = childCanvas.gameObject;
                    Debug.Log($"[EmbossingConfirmationPanel] Auto-assigned panelRoot to child Canvas: {panelRoot.name}");
                }
                else
                {
                    // Look for any child with "Panel" in the name
                    Transform panelChild = transform.Find("Panel");
                    if (panelChild == null)
                    {
                        // Try case-insensitive search
                        foreach (Transform child in transform)
                        {
                            if (child.name.ToLower().Contains("panel") || child.name.ToLower().Contains("canvas"))
                            {
                                panelChild = child;
                                break;
                            }
                        }
                    }
                    
                    if (panelChild != null)
                    {
                        panelRoot = panelChild.gameObject;
                        Debug.Log($"[EmbossingConfirmationPanel] Auto-assigned panelRoot to child: {panelRoot.name}");
                    }
                    else
                    {
                        // Keep this GameObject active, use CanvasGroup to hide/show instead
                        panelRoot = gameObject;
                        Debug.LogWarning($"[EmbossingConfirmationPanel] No child found, using self as panelRoot: {panelRoot.name}. Component GameObject must stay active!");
                    }
                }
            }
            
            if (autoSetup)
            {
                AutoSetupReferences();
            }
            
            SetupButtons();
            
            // Get or add CanvasGroup for animations BEFORE hiding
            if (panelTransform == null && panelRoot != null)
            {
                // Try to use panelRoot if panelTransform not assigned
                panelTransform = panelRoot.GetComponent<RectTransform>();
                if (panelTransform != null)
                {
                    Debug.Log("[EmbossingConfirmationPanel] Auto-assigned panelTransform from panelRoot");
                }
            }
            
            if (panelTransform != null)
            {
                panelCanvasGroup = panelTransform.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = panelTransform.gameObject.AddComponent<CanvasGroup>();
                    Debug.Log("[EmbossingConfirmationPanel] Added CanvasGroup component");
                }
            }
            
            // Hide panel initially
            // The component GameObject MUST stay active, but we disable the visual panel (panelRoot)
            if (panelRoot != null)
            {
                // ALWAYS disable panelRoot initially (it's the Canvas, not the component)
                panelRoot.SetActive(false);
                Debug.Log($"[EmbossingConfirmationPanel] Panel disabled initially: {panelRoot.name}");
                
                // Also set CanvasGroup alpha to 0 if available
                if (panelCanvasGroup != null)
                {
                    panelCanvasGroup.alpha = 0f;
                    panelCanvasGroup.interactable = false;
                    panelCanvasGroup.blocksRaycasts = false;
                }
                
                // Validate setup
                ValidateSetup();
            }
            else
            {
                Debug.LogError("[EmbossingConfirmationPanel] panelRoot is still null after Awake!");
            }
        }
        
        /// <summary>
        /// Validate the panel setup and warn about missing components
        /// </summary>
        void ValidateSetup()
        {
            // Check for overlay image (important for blocking clicks)
            if (overlayImage == null)
            {
                Image[] images = panelRoot.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img.name.ToLower().Contains("overlay"))
                    {
                        overlayImage = img;
                        Debug.Log($"[EmbossingConfirmationPanel] Found overlay: {img.name}");
                        break;
                    }
                }
                
                if (overlayImage == null)
                {
                    Debug.LogWarning("[EmbossingConfirmationPanel] No overlay Image found! Panel may not block clicks properly. Add an Image component named 'Overlay' that covers the screen.");
                }
            }
            
            // Ensure overlay blocks raycasts
            if (overlayImage != null)
            {
                overlayImage.raycastTarget = true;
                Debug.Log($"[EmbossingConfirmationPanel] Overlay raycastTarget enabled: {overlayImage.name}");
            }
            
            // Check Canvas sorting order
            Canvas canvas = panelRoot.GetComponent<Canvas>();
            if (canvas != null)
            {
                if (canvas.sortingOrder < 100)
                {
                    Debug.LogWarning($"[EmbossingConfirmationPanel] Canvas sorting order is {canvas.sortingOrder}. Consider increasing to 1000+ to appear on top of other UI.");
                }
                Debug.Log($"[EmbossingConfirmationPanel] Canvas sorting order: {canvas.sortingOrder}");
                
                // Check for GraphicRaycaster (required for UI interaction)
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    Debug.LogWarning("[EmbossingConfirmationPanel] Canvas is missing GraphicRaycaster! Adding it now...");
                    canvas.gameObject.AddComponent<GraphicRaycaster>();
                }
                else
                {
                    Debug.Log("[EmbossingConfirmationPanel] GraphicRaycaster found on Canvas");
                }
            }
        }
        
        /// <summary>
        /// Auto-setup references by finding components
        /// </summary>
        void AutoSetupReferences()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }
            
            // Find all TextMeshProUGUI components
            TextMeshProUGUI[] allText = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allText)
            {
                string textName = text.name.ToLower();
                
                if (textName.Contains("title"))
                    titleText = text;
                else if (textName.Contains("description") || textName.Contains("desc"))
                    descriptionText = text;
                else if (textName.Contains("category"))
                    categoryText = text;
                else if (textName.Contains("rarity"))
                    rarityText = text;
                else if (textName.Contains("element"))
                    elementText = text;
                else if (textName.Contains("requirement"))
                    requirementsText = text;
                else if (textName.Contains("effect") && !textName.Contains("title"))
                    effectText = text;
                else if (textName.Contains("manacost") || textName.Contains("mana_cost"))
                    manaCostText = text;
                else if (textName.Contains("cardname") || textName.Contains("card_name"))
                    cardNameText = text;
                else if (textName.Contains("currentslot") || textName.Contains("current_slot"))
                    currentSlotsText = text;
                else if (textName.Contains("slotindex") || textName.Contains("slot_index"))
                    slotIndexText = text;
                else if (textName.Contains("currentcost") || textName.Contains("current_cost"))
                    currentCostText = text;
                else if (textName.Contains("newcost") || textName.Contains("new_cost"))
                    newCostText = text;
                else if (textName.Contains("validation") || textName.Contains("message"))
                    validationMessageText = text;
            }
            
            // Find buttons
            Button[] allButtons = GetComponentsInChildren<Button>(true);
            foreach (var button in allButtons)
            {
                string buttonName = button.name.ToLower();
                
                if (buttonName.Contains("confirm"))
                    confirmButton = button;
                else if (buttonName.Contains("cancel"))
                    cancelButton = button;
                else if (buttonName.Contains("close"))
                    closeButton = button;
            }
            
            // Find images
            Image[] allImages = GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                string imgName = img.name.ToLower();
                
                if (imgName.Contains("icon") && iconImage == null)
                    iconImage = img;
                else if (imgName.Contains("overlay"))
                    overlayImage = img;
            }
            
            Debug.Log("[EmbossingConfirmationPanel] Auto-setup complete");
        }
        
        /// <summary>
        /// Setup button listeners
        /// </summary>
        void SetupButtons()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirm);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancel);
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCancel);
            }
        }
        
        /// <summary>
        /// Show confirmation panel for applying an embossing
        /// </summary>
        public void ShowConfirmation(EmbossingEffect embossing, Card card, int slotIndex)
        {
            if (embossing == null || card == null)
            {
                Debug.LogError("[EmbossingConfirmationPanel] Cannot show confirmation - embossing or card is null");
                return;
            }
            
            currentEmbossing = embossing;
            currentCard = card;
            targetSlotIndex = slotIndex;
            
            // Update UI
            UpdateEmbossingInfo();
            UpdateCardInfo();
            UpdateManaCostPreview();
            UpdateValidation();
            
            // Debug: Check if panelRoot is assigned
            if (panelRoot == null)
            {
                Debug.LogError("[EmbossingConfirmationPanel] panelRoot is NULL! Cannot show panel. Please assign 'Panel Root' in the inspector.");
                return;
            }
            
            Debug.Log($"[EmbossingConfirmationPanel] Showing confirmation for {embossing.embossingName} on {card.cardName}");
            
            // Kill any ongoing animation
            if (currentAnimation != null && currentAnimation.IsActive())
            {
                currentAnimation.Kill();
                Debug.Log("[EmbossingConfirmationPanel] Killed previous animation");
            }
            
            // Show panel
            if (useAnimation)
            {
                AnimateShow();
            }
            else
            {
                panelRoot.SetActive(true);
                isShowing = true;
                
                if (panelCanvasGroup != null)
                {
                    panelCanvasGroup.alpha = 1f;
                    panelCanvasGroup.interactable = true;
                    panelCanvasGroup.blocksRaycasts = true;
                }
                
                if (panelTransform != null)
                {
                    panelTransform.localScale = Vector3.one;
                }
            }
        }
        
        /// <summary>
        /// Hide the confirmation panel
        /// </summary>
        public void HidePanel()
        {
            Debug.Log("[EmbossingConfirmationPanel] ======== HidePanel() called ========");
            
            // Kill any ongoing animation
            if (currentAnimation != null && currentAnimation.IsActive())
            {
                currentAnimation.Kill();
                Debug.Log("[EmbossingConfirmationPanel] Killed previous animation");
            }
            
            if (useAnimation)
            {
                AnimateHide();
            }
            else
            {
                panelRoot.SetActive(false);
                isShowing = false;
                
                if (panelCanvasGroup != null)
                {
                    panelCanvasGroup.alpha = 0f;
                    panelCanvasGroup.interactable = false;
                    panelCanvasGroup.blocksRaycasts = false;
                }
                
                if (panelTransform != null)
                {
                    panelTransform.localScale = Vector3.one;
                }
            }
        }
        
        /// <summary>
        /// Animate panel showing with DOTween (zoom in + fade in)
        /// </summary>
        void AnimateShow()
        {
            Debug.Log("[EmbossingConfirmationPanel] Starting DOTween SHOW animation");
            
            // Enable panel
            panelRoot.SetActive(true);
            isShowing = true;
            
            // Enable raycasts immediately
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.interactable = true;
                panelCanvasGroup.blocksRaycasts = true;
            }
            
            // Set initial state (small and invisible)
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
            }
            
            if (panelTransform != null)
            {
                panelTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }
            
            // Create animation sequence
            currentAnimation = DOTween.Sequence();
            
            // Fade in
            if (panelCanvasGroup != null)
            {
                currentAnimation.Join(panelCanvasGroup.DOFade(1f, showDuration).SetEase(showEase));
            }
            
            // Scale up (zoom in)
            if (panelTransform != null)
            {
                currentAnimation.Join(panelTransform.DOScale(Vector3.one, showDuration).SetEase(showEase));
            }
            
            currentAnimation.OnComplete(() =>
            {
                Debug.Log("[EmbossingConfirmationPanel] DOTween SHOW animation complete");
                currentAnimation = null;
            });
            
            currentAnimation.Play();
        }
        
        /// <summary>
        /// Animate panel hiding with DOTween (zoom out/minimize + fade out)
        /// </summary>
        void AnimateHide()
        {
            Debug.Log("[EmbossingConfirmationPanel] Starting DOTween HIDE animation");
            
            // Disable raycasts immediately so panel doesn't block during hide
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }
            
            // Create animation sequence
            currentAnimation = DOTween.Sequence();
            
            // Fade out
            if (panelCanvasGroup != null)
            {
                currentAnimation.Join(panelCanvasGroup.DOFade(0f, hideDuration).SetEase(hideEase));
            }
            
            // Scale down (zoom out/minimize)
            if (panelTransform != null)
            {
                currentAnimation.Join(panelTransform.DOScale(new Vector3(0.5f, 0.5f, 1f), hideDuration).SetEase(hideEase));
            }
            
            currentAnimation.OnComplete(() =>
            {
                // Disable panel after animation
                panelRoot.SetActive(false);
                isShowing = false;
                
                // Reset to default scale for next show
                if (panelTransform != null)
                {
                    panelTransform.localScale = Vector3.one;
                }
                
                Debug.Log("[EmbossingConfirmationPanel] DOTween HIDE animation complete - Panel disabled");
                currentAnimation = null;
            });
            
            currentAnimation.Play();
        }
        
        /// <summary>
        /// Update embossing information display
        /// </summary>
        void UpdateEmbossingInfo()
        {
            if (currentEmbossing == null) return;
            
            Character character = CharacterManager.Instance?.currentCharacter;
            
            // Title
            if (titleText != null)
            {
                titleText.text = $"Apply {currentEmbossing.embossingName}?";
                titleText.color = currentEmbossing.GetRarityColor();
            }
            
            // Icon
            if (iconImage != null)
            {
                if (currentEmbossing.embossingIcon != null)
                {
                    iconImage.sprite = currentEmbossing.embossingIcon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }
            
            // Description
            if (descriptionText != null)
            {
                descriptionText.text = currentEmbossing.description;
            }
            
            // Category
            if (categoryText != null)
            {
                categoryText.text = $"<b>Category:</b> {currentEmbossing.category}";
                categoryText.color = currentEmbossing.GetTypeColor();
            }
            
            // Rarity
            if (rarityText != null)
            {
                rarityText.text = $"<b>Rarity:</b> {currentEmbossing.rarity}";
                rarityText.color = currentEmbossing.GetRarityColor();
            }
            
            // Element
            if (elementText != null)
            {
                elementText.text = $"<b>Element:</b> {currentEmbossing.elementType}";
            }
            
            // Requirements
            if (requirementsText != null)
            {
                if (character != null)
                {
                    requirementsText.text = $"<b>Requirements:</b>\n{currentEmbossing.GetRequirementsTextColored(character)}";
                }
                else
                {
                    requirementsText.text = $"<b>Requirements:</b>\n{currentEmbossing.GetRequirementsText()}";
                }
            }
            
            // Effect
            if (effectText != null)
            {
                effectText.text = $"<b>Effect:</b>\n{currentEmbossing.GetEffectDescription()}";
            }
            
            // Mana Cost
            if (manaCostText != null)
            {
                string costText = $"+{(currentEmbossing.manaCostMultiplier * 100):F0}% mana cost";
                if (currentEmbossing.flatManaCostIncrease > 0)
                {
                    costText += $" +{currentEmbossing.flatManaCostIncrease} flat";
                }
                manaCostText.text = $"<b>Mana Cost Impact:</b>\n{costText}";
            }
        }
        
        /// <summary>
        /// Update target card information display
        /// </summary>
        void UpdateCardInfo()
        {
            if (currentCard == null) return;
            
            // Card name
            if (cardNameText != null)
            {
                cardNameText.text = $"<b>Target Card:</b> {currentCard.cardName}";
            }
            
            // Current slots
            if (currentSlotsText != null)
            {
                int currentEmbossings = currentCard.appliedEmbossings != null ? currentCard.appliedEmbossings.Count : 0;
                currentSlotsText.text = $"<b>Embossing Slots:</b> {currentEmbossings}/{currentCard.embossingSlots}";
            }
            
            // Slot index
            if (slotIndexText != null)
            {
                slotIndexText.text = $"<b>Will use:</b> Slot {targetSlotIndex + 1}";
            }
        }
        
        /// <summary>
        /// Update mana cost preview
        /// </summary>
        void UpdateManaCostPreview()
        {
            if (currentCard == null || currentEmbossing == null) return;
            
            int currentCost = currentCard.manaCost;
            int newCost = currentEmbossing.CalculateNewManaCost(currentCard);
            
            // Current cost
            if (currentCostText != null)
            {
                currentCostText.text = $"<b>Current Cost:</b> {currentCost}";
            }
            
            // New cost
            if (newCostText != null)
            {
                int increase = newCost - currentCost;
                float increasePercent = currentCost > 0 ? ((float)increase / currentCost) * 100 : 0;
                newCostText.text = $"<b>New Cost:</b> {newCost} <color=red>(+{increase}, +{increasePercent:F0}%)</color>";
            }
        }
        
        /// <summary>
        /// Update validation message and button state
        /// </summary>
        void UpdateValidation()
        {
            if (currentCard == null || currentEmbossing == null) return;
            
            Character character = CharacterManager.Instance?.currentCharacter;
            string validationMessage = "";
            bool canApply = true;
            Color messageColor = validColor;
            
            // Test mode bypasses all checks
            if (testMode)
            {
                validationMessage = "<color=yellow>TEST MODE: All checks bypassed</color>";
                messageColor = warningColor;
                canApply = true;
            }
            // Check character requirements
            else if (character != null && !currentEmbossing.MeetsRequirements(character))
            {
                validationMessage = "You do not meet the requirements for this embossing.";
                canApply = false;
                messageColor = invalidColor;
            }
            // Check available slots
            else if (targetSlotIndex >= currentCard.embossingSlots)
            {
                validationMessage = "Card has no available embossing slots.";
                canApply = false;
                messageColor = invalidColor;
            }
            // Check if embossing is unique and already applied
            else if (currentEmbossing.unique && CardHasEmbossing(currentCard, currentEmbossing.embossingId))
            {
                validationMessage = "This embossing is unique and already applied to this card.";
                canApply = false;
                messageColor = invalidColor;
            }
            // Check exclusivity group
            else if (!string.IsNullOrEmpty(currentEmbossing.exclusivityGroup) && 
                     CardHasExclusivityGroup(currentCard, currentEmbossing.exclusivityGroup))
            {
                validationMessage = $"This card already has an embossing from the '{currentEmbossing.exclusivityGroup}' group.";
                canApply = false;
                messageColor = invalidColor;
            }
            else
            {
                validationMessage = "Ready to apply embossing.";
                messageColor = validColor;
            }
            
            // Update validation text
            if (validationMessageText != null)
            {
                validationMessageText.text = validationMessage;
                validationMessageText.color = messageColor;
            }
            
            // Update button state
            if (confirmButton != null)
            {
                confirmButton.interactable = canApply;
            }
        }
        
        /// <summary>
        /// Check if card already has a specific embossing
        /// </summary>
        bool CardHasEmbossing(Card card, string embossingId)
        {
            if (card.appliedEmbossings == null) return false;
            
            foreach (var instance in card.appliedEmbossings)
            {
                if (instance.embossingId == embossingId)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if card has embossing from exclusivity group
        /// </summary>
        bool CardHasExclusivityGroup(Card card, string exclusivityGroup)
        {
            if (card.appliedEmbossings == null) return false;
            
            foreach (var instance in card.appliedEmbossings)
            {
                EmbossingEffect effect = EmbossingDatabase.Instance?.GetEmbossing(instance.embossingId);
                if (effect != null && effect.exclusivityGroup == exclusivityGroup)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Handle confirm button click
        /// </summary>
        void OnConfirm()
        {
            if (currentCard == null || currentEmbossing == null)
            {
                Debug.LogError("[EmbossingConfirmationPanel] Cannot apply - card or embossing is null");
                return;
            }
            
            // Validate (unless in test mode)
            if (!testMode)
            {
                Character character = CharacterManager.Instance?.currentCharacter;
                if (character != null && !currentEmbossing.MeetsRequirements(character))
                {
                    Debug.LogWarning("[EmbossingConfirmationPanel] Character does not meet requirements");
                    return;
                }
            }
            else
            {
                Debug.Log("[EmbossingConfirmationPanel] TEST MODE: Bypassing requirement checks");
            }
            
            // Apply embossing
            bool success = ApplyEmbossingToCard();
            
            if (success)
            {
                Debug.Log($"[EmbossingConfirmationPanel] Successfully applied {currentEmbossing.embossingName} to {currentCard.cardName}");
                HidePanel();
            }
            else
            {
                Debug.LogError("[EmbossingConfirmationPanel] Failed to apply embossing");
            }
        }
        
        /// <summary>
        /// Handle cancel button click
        /// </summary>
        void OnCancel()
        {
            Debug.Log("[EmbossingConfirmationPanel] OnCancel() - Cancelled embossing application");
            HidePanel();
        }
        
        /// <summary>
        /// Apply embossing to the card
        /// </summary>
        bool ApplyEmbossingToCard()
        {
            if (currentCard == null || currentEmbossing == null)
                return false;
            
            // Create embossing instance
            EmbossingInstance newInstance = new EmbossingInstance
            {
                embossingId = currentEmbossing.embossingId,
                level = 1,
                experience = 0,
                slotIndex = targetSlotIndex
            };
            
            // Add to card
            if (currentCard.appliedEmbossings == null)
            {
                currentCard.appliedEmbossings = new System.Collections.Generic.List<EmbossingInstance>();
            }
            
            currentCard.appliedEmbossings.Add(newInstance);
            
            // Update all cards with same groupKey in the deck
            UpdateCardInActiveDeck(currentCard);
            
            // Refresh card carousel display
            RefreshCardCarousel();
            
            return true;
        }
        
        /// <summary>
        /// Update the card in the active deck
        /// Note: For runtime Card objects, embossings are tracked per instance.
        /// DeckPreset persistence is handled separately through the deck builder.
        /// </summary>
        void UpdateCardInActiveDeck(Card card)
        {
            if (card == null) return;
            
            // Get active deck preset
            if (DeckManager.Instance == null || !DeckManager.Instance.HasActiveDeck())
            {
                Debug.LogWarning("[EmbossingConfirmationPanel] No active deck found");
                return;
            }
            
            DeckPreset activeDeck = DeckManager.Instance.GetActiveDeck();
            string groupKey = card.GetGroupKey();
            
            if (!activeDeck.UpdateEmbossingsForGroup(groupKey, card.appliedEmbossings))
            {
                Debug.LogWarning($"[EmbossingConfirmationPanel] Failed to update embossings for group '{groupKey}' in active deck");
                return;
            }

            Debug.Log($"[EmbossingConfirmationPanel] Stored embossings for '{card.cardName}' (groupKey: {groupKey}) in deck '{activeDeck.deckName}'");

            // Persist deck changes so other scenes pick up the updated embossings
            DeckManager.Instance.SaveDeck(activeDeck);
            DeckManager.Instance.NotifyDeckUpdatedExternally();
        }
        
        /// <summary>
        /// Refresh card carousel display
        /// </summary>
        void RefreshCardCarousel()
        {
            CardCarouselUI carousel = FindFirstObjectByType<CardCarouselUI>();
            if (carousel != null)
            {
                carousel.ReloadDeckPreservingSelection();
                Debug.Log("[EmbossingConfirmationPanel] Refreshed card carousel");
            }
        }
        
        void OnDestroy()
        {
            // Kill any active DOTween animations when component is destroyed
            if (currentAnimation != null && currentAnimation.IsActive())
            {
                currentAnimation.Kill();
                Debug.Log("[EmbossingConfirmationPanel] Killed DOTween animation on destroy");
            }
        }
        
        /// <summary>
        /// Check if panel is currently showing
        /// </summary>
        public bool IsShowing()
        {
            return isShowing;
        }
    }
}

