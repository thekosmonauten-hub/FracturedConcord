using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// UI component for individual cards in the deck list panel.
/// Simplified display showing name, cost, and quantity.
/// Click to remove from deck.
/// </summary>
public class DeckCardListUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Elements")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image costIcon;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image rarityIndicator;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color hoverColor = new Color(0.4f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color[] rarityColors = new Color[4]
    {
        Color.white,                        // Common
        new Color(0.3f, 0.5f, 1f),         // Magic (Blue)
        new Color(1f, 0.8f, 0f),           // Rare (Gold)
        new Color(1f, 0.5f, 0f)            // Unique (Orange)
    };
    
    [Header("Background Art Settings (Center-anchored)")]
    [SerializeField] private bool centerAnchorArt = true;
    [SerializeField] private bool clipToRow = true; // add RectMask2D to crop overflow
    [SerializeField] private bool deferMaskUntilLayout = true; // add mask after first layout frame

    private DeckCardEntry cardEntry;
    private DeckBuilderUI deckBuilder;
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        
        // Disable Button component if it exists - we use IPointerClickHandler instead
        // This prevents the Button from consuming click events
        if (button != null)
        {
            button.transition = Selectable.Transition.None; // Disable visual transitions
            // Don't set interactable to false - that blocks ALL pointer events!
        }
    }

    private void Start()
    {
        if (clipToRow && deferMaskUntilLayout)
        {
            StartCoroutine(AddMaskAfterLayout());
        }
    }
    
    /// <summary>
    /// Initialize the deck card UI with card entry data.
    /// </summary>
    public void Initialize(DeckCardEntry entry, DeckBuilderUI deckBuilderUI)
    {
        cardEntry = entry;
        deckBuilder = deckBuilderUI;
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// Update the card's visual display.
    /// </summary>
    private void UpdateDisplay()
    {
        if (cardEntry == null || cardEntry.cardData == null) return;
        
        CardData card = cardEntry.cardData;
        
        // Update card name
        if (cardNameText != null)
        {
            cardNameText.text = card.cardName;
            
            // Color based on rarity
            if ((int)card.rarity < rarityColors.Length)
            {
                cardNameText.color = rarityColors[(int)card.rarity];
            }
        }
        
        // Update cost
        if (costText != null)
        {
            costText.text = card.playCost.ToString();
        }
        
        // Update quantity
        if (quantityText != null)
        {
            if (cardEntry.quantity > 1)
            {
                quantityText.text = $"x{cardEntry.quantity}";
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }
        
        // Update rarity indicator
        if (rarityIndicator != null)
        {
            if ((int)card.rarity < rarityColors.Length)
            {
                rarityIndicator.color = rarityColors[(int)card.rarity];
            }
        }
        
        // Set background to card art when available; center-anchored and scalable
        if (cardBackground != null)
        {
            // Use thumbnail sprite for list view (falls back to cardImage if not assigned)
            Sprite displaySprite = card.GetCardSprite(CardSpriteContext.Thumbnail);
            
            if (displaySprite != null)
            {
                cardBackground.sprite = displaySprite;
                cardBackground.preserveAspect = false; // we will control scaling
                cardBackground.color = Color.white; // ensure art isn't tinted

                if (centerAnchorArt)
                {
                    var bgRt = cardBackground.rectTransform;
                    // Center anchors and pivot
                    bgRt.anchorMin = new Vector2(0.5f, 0.5f);
                    bgRt.anchorMax = new Vector2(0.5f, 0.5f);
                    bgRt.pivot = new Vector2(0.5f, 0.5f);
                    bgRt.anchoredPosition = Vector2.zero;
                    // Native size, no additional scaling
                    cardBackground.SetNativeSize();
                    bgRt.localScale = Vector3.one;
                }
            }
            else
            {
                cardBackground.sprite = null;
                cardBackground.color = normalColor;
                // Reset any scaling if previously applied
                var bgRt = cardBackground.rectTransform;
                bgRt.localScale = Vector3.one;
            }
        }
    }

    private System.Collections.IEnumerator AddMaskAfterLayout()
    {
        // Wait for end of frame to allow layout to finalize sizes
        yield return new WaitForEndOfFrame();
        var mask = GetComponent<UnityEngine.UI.RectMask2D>();
        if (mask == null)
        {
            gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        }
    }
    
    #region Interaction Handlers
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardBackground != null)
        {
            // If showing art, avoid tinting it; otherwise use hover color
            if (cardBackground.sprite != null)
            {
                cardBackground.color = Color.white;
            }
            else
            {
                cardBackground.color = hoverColor;
            }
        }
        
        // Slight scale animation
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.one * 1.05f, 0.15f)
            .setEase(LeanTweenType.easeOutQuad);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (cardBackground != null)
        {
            // Restore proper base color depending on whether art is used
            if (cardBackground.sprite != null)
            {
                cardBackground.color = Color.white;
            }
            else
            {
                cardBackground.color = normalColor;
            }
        }
        
        // Scale back
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.one, 0.15f)
            .setEase(LeanTweenType.easeInQuad);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Only respond to left-click to remove cards
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Check for Shift key using new Input System
            bool shiftPressed = Keyboard.current != null && 
                               (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
            
            // Shift + Left-Click: Remove 2 cards
            if (shiftPressed)
            {
                OnCardClicked(); // Remove first card
                OnCardClicked(); // Remove second card
            }
            else
            {
                OnCardClicked(); // Remove 1 card
            }
        }
        
        // Right-click could be used for other actions (remove all copies, etc.)
        // Currently disabled - only left-click removes cards
    }
    
    private void OnCardClicked()
    {
        if (cardEntry == null || cardEntry.cardData == null || deckBuilder == null) return;
        
        // Play remove animation
        PlayRemoveAnimation();
        
        // Notify deck builder
        deckBuilder.OnCardRemoved(cardEntry.cardData);
    }
    
    private void PlayRemoveAnimation()
    {
        // Fade out and slide animation
        LeanTween.cancel(gameObject);
        LeanTween.alpha(GetComponent<RectTransform>(), 0f, 0.3f)
            .setEase(LeanTweenType.easeInQuad);
        
        LeanTween.moveLocalX(gameObject, -200f, 0.3f)
            .setEase(LeanTweenType.easeInBack);
    }
    #endregion
    
    private void OnDestroy()
    {
        // Clean up tweens
        LeanTween.cancel(gameObject);
    }
}
