using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
        
        // Check for shift-click (left-click + shift key) using new Input System
        bool isShiftClick = false;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Use new Input System to check for shift keys
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                isShiftClick = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            }
            else
            {
                // Fallback: If keyboard is null (shouldn't happen in normal gameplay), log warning
                Debug.LogWarning("[CardClickHandler] Keyboard.current is null - cannot detect shift key");
            }
        }
        
        // If right-click or shift-click, prepare the card
        if (isRightClick || isShiftClick)
        {
            deckManager.OnCardClicked(cardObject, isRightClick, isShiftClick);
        }
        // Left-click without shift is handled by Button component
    }
}

