using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles card clicks with support for right-click and shift-click detection.
/// Used in combat to enable preparation via right-click or shift-click.
/// </summary>
public class CardClickHandler : MonoBehaviour, IPointerClickHandler
{
    private CombatDeckManager deckManager;
    private GameObject cardObject;
    
    public void Initialize(CombatDeckManager manager, GameObject cardObj)
    {
        deckManager = manager;
        cardObject = cardObj;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (deckManager == null || cardObject == null) return;
        
        // Check for right-click
        bool isRightClick = eventData.button == PointerEventData.InputButton.Right;
        
        // Check for shift-click (left-click + shift key)
        bool isShiftClick = eventData.button == PointerEventData.InputButton.Left &&
                           (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        
        // If right-click or shift-click, prepare the card
        if (isRightClick || isShiftClick)
        {
            deckManager.OnCardClicked(cardObject, isRightClick, isShiftClick);
        }
        // Left-click without shift is handled by Button component
    }
}

