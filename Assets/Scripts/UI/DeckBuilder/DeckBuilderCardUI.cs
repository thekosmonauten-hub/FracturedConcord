using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// UI component for individual cards in the collection grid.
/// Displays card information and handles click/hover interactions.
/// </summary>
public class DeckBuilderCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Structure")]
    [SerializeField] private RectTransform visualRoot; // scale this instead of the layout root to avoid layout jitter
    [Header("Card Visual Elements")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image cardImage;
    [SerializeField] private Image rarityFrame;
    [SerializeField] private Image elementFrame;
    [SerializeField] private Image costBubble;
    
    [Header("Card Text Elements")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI categoryText;
    
    [Header("State Visuals")]
    [SerializeField] private GameObject disabledOverlay;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color disabledColor = Color.gray;
    
    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float animationDuration = 0.2f;
    
    private CardData cardData;
    private DeckBuilderUI deckBuilder;
    private Character ownerCharacter; // For dynamic descriptions in combat
    private int currentQuantity;
    private int maxQuantity;
    private bool isInteractable = true;
    private Vector3 originalScale;
    private RectTransform rectTransform;
    private Button button;
    private Transform scaleTarget;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        // Decide which transform to scale on hover
        scaleTarget = visualRoot != null ? (Transform)visualRoot : transform;
        originalScale = scaleTarget.localScale;
        
        // Disable Button component if it exists - we use IPointerClickHandler instead
        // This prevents the Button from consuming click events
        if (button != null)
        {
            button.transition = Selectable.Transition.None; // Disable visual transitions
            // Don't set interactable to false - that blocks ALL pointer events!
        }

        // Ensure hover visuals do not block raycasts
        if (glowEffect != null)
        {
            var graphics = glowEffect.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].raycastTarget = false;
            }
        }
    }
    
    /// <summary>
    /// Initialize the card UI with card data.
    /// </summary>
    public void Initialize(CardData card, DeckBuilderUI deckBuilderUI)
    {
        cardData = card;
        deckBuilder = deckBuilderUI;
        ownerCharacter = null; // No character in deck builder
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// Initialize the card UI with card data and character (for combat with dynamic descriptions).
    /// </summary>
    public void Initialize(CardData card, DeckBuilderUI deckBuilderUI, Character character)
    {
        cardData = card;
        deckBuilder = deckBuilderUI;
        ownerCharacter = character;
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// Update the card's visual display.
    /// </summary>
    private void UpdateDisplay()
    {
        if (cardData == null) return;
        
        // Update card image
        if (cardImage != null && cardData.cardImage != null)
        {
            cardImage.sprite = cardData.cardImage;
        }

        // Background remains styled by color/frames for collection grid; no sprite assignment here
        
        // Update text elements
        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }
        
        if (costText != null)
        {
            costText.text = cardData.playCost.ToString();
        }
        
        if (descriptionText != null)
        {
            // Use dynamic description if we have a character and the card supports it
            if (ownerCharacter != null && cardData is CardDataExtended)
            {
                CardDataExtended extendedCard = cardData as CardDataExtended;
                descriptionText.text = extendedCard.GetDynamicDescription(ownerCharacter);
                Debug.Log($"<color=cyan>[DynamicDesc] Using dynamic description for {cardData.cardName}</color>");
            }
            else
            {
                // Use static description (deck builder mode or regular CardData)
                descriptionText.text = cardData.description;
            }
        }
        
        if (categoryText != null)
        {
            categoryText.text = cardData.category.ToString();
        }
        
        // Update visual assets
        if (rarityFrame != null && cardData.rarityFrame != null)
        {
            rarityFrame.sprite = cardData.rarityFrame;
        }
        
        if (elementFrame != null && cardData.elementFrame != null)
        {
            elementFrame.sprite = cardData.elementFrame;
        }
        
        if (costBubble != null && cardData.costBubble != null)
        {
            costBubble.sprite = cardData.costBubble;
        }
        
        // Update rarity-based styling
        UpdateRarityVisuals();
    }
    
    /// <summary>
    /// Update quantity display and interactability.
    /// </summary>
    public void UpdateQuantity(int quantity, int max)
    {
        currentQuantity = quantity;
        maxQuantity = max;
        
        // Update quantity text
        if (quantityText != null)
        {
            if (quantity > 0)
            {
                quantityText.text = $"x{quantity}/{max}";
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }
        
        // Update visual state
        UpdateVisualState();
    }
    
    /// <summary>
    /// Set whether the card is interactable (can be added to deck).
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        UpdateVisualState();
    }
    
    private void UpdateVisualState()
    {
        if (disabledOverlay != null)
        {
            disabledOverlay.SetActive(!isInteractable);
        }
        
        if (cardBackground != null)
        {
            cardBackground.color = isInteractable ? normalColor : disabledColor;
        }
        
        // Note: Don't set button.interactable - it blocks IPointerClickHandler events
        // We handle interactability manually in OnPointerClick/OnPointerEnter
    }
    
    private void UpdateRarityVisuals()
    {
        if (cardData == null) return;
        
        // Update rarity frame background color
        UpdateRarityFrameColor();
    }
    
    /// <summary>
    /// Updates the rarity frame background color based on card rarity.
    /// Maintains semi-transparency while providing distinct colors for each rarity tier.
    /// </summary>
    private void UpdateRarityFrameColor()
    {
        if (rarityFrame == null || cardData == null) return;
        
        rarityFrame.color = RarityColorUtility.GetRarityFrameColor(cardData.rarity);
    }
    
    #region Interaction Handlers
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        // Scale up animation on visual root only (keeps layout root stable)
        LeanTween.cancel(scaleTarget.gameObject);
        LeanTween.scale(scaleTarget.gameObject, originalScale * hoverScale, animationDuration)
            .setEase(LeanTweenType.easeOutBack);
        
        // Highlight effect
        if (cardBackground != null)
        {
            cardBackground.color = hoverColor;
        }
        
        // Enhance rarity frame color on hover (make it more opaque)
        if (rarityFrame != null && cardData != null)
        {
            rarityFrame.color = RarityColorUtility.GetRarityFrameHoverColor(cardData.rarity);
        }
        
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
        }
        
        // Note: SetAsLastSibling() removed because it causes grid recalculation issues
        // Cards will scale up on hover but won't overlap other cards
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        // Scale down animation on visual root only
        LeanTween.cancel(scaleTarget.gameObject);
        LeanTween.scale(scaleTarget.gameObject, originalScale, animationDuration)
            .setEase(LeanTweenType.easeInBack);
        
        // Remove highlight
        if (cardBackground != null)
        {
            cardBackground.color = normalColor;
        }
        
        // Restore normal rarity frame color
        if (rarityFrame != null && cardData != null)
        {
            rarityFrame.color = RarityColorUtility.GetRarityFrameColor(cardData.rarity);
        }
        
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"OnPointerClick received! Button: {eventData.button}, Card: {cardData?.cardName}");
        
        // Only respond to left-click to add cards
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Check for Shift key using new Input System
            bool shiftPressed = Keyboard.current != null && 
                               (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
            
            // Shift + Left-Click: Add 2 cards
            if (shiftPressed)
            {
                Debug.Log("Shift+Click detected - adding 2 cards");
                OnCardClicked(); // Add first card
                OnCardClicked(); // Add second card
            }
            else
            {
                Debug.Log("Regular click - adding 1 card");
                OnCardClicked(); // Add 1 card
            }
        }
        
        // Right-click could be used for other actions (preview, info, etc.)
        // Currently disabled - only left-click adds cards
    }
    
    private void OnCardClicked()
    {
        if (!isInteractable || cardData == null || deckBuilder == null) return;
        
        // Play click animation
        PlayClickAnimation();
        
        // Notify deck builder
        deckBuilder.OnCardAdded(cardData);
    }
    
    private void PlayClickAnimation()
    {
        // Quick punch animation on visual root only; ensure it doesn't block clicks
        LeanTween.cancel(scaleTarget.gameObject);
        LeanTween.scale(scaleTarget.gameObject, originalScale * 1.1f, 0.1f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(scaleTarget.gameObject, originalScale, 0.1f)
                    .setEase(LeanTweenType.easeInQuad);
            });
    }
    #endregion
    
    private void OnDestroy()
    {
        // Clean up tweens
        LeanTween.cancel(gameObject);
    }

	/// <summary>
	/// Expose the current CardData for consumers (e.g., combat adapters/tooltips).
	/// </summary>
	public CardData GetCardData()
	{
		return cardData;
	}
}
