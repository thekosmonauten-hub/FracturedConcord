using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles hover detection for cards in CharacterDisplayUI scene.
/// Triggers full card preview when hovering over simplified card rows.
/// Attach this to CharacterScreenDeckCard prefab instances.
/// </summary>
public class CharacterScreenCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private CardDataExtended cardData;
    
    [Header("Settings")]
    public bool showDebugLogs = false;
    
    // Event delegates for hover
    public System.Action<CardDataExtended> OnCardHoverEnter;
    public System.Action OnCardHoverExit;
    
    /// <summary>
    /// Set the card data for this hover component
    /// </summary>
    public void SetCardData(CardDataExtended card)
    {
        cardData = card;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardData == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("[CharacterScreenCardHover] Card data is null!");
            return;
        }
        
        if (showDebugLogs)
            Debug.Log($"[CharacterScreenCardHover] Hovering over: {cardData.cardName}");
        
        // Trigger hover event
        OnCardHoverEnter?.Invoke(cardData);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (showDebugLogs)
            Debug.Log($"[CharacterScreenCardHover] Stopped hovering");
        
        // Trigger exit event
        OnCardHoverExit?.Invoke();
    }
}

