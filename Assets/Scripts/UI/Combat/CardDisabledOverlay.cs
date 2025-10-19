using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual overlay to show when a card is disabled during animation.
/// Attach to card prefab as child GameObject with semi-transparent image.
/// </summary>
[RequireComponent(typeof(Image))]
public class CardDisabledOverlay : MonoBehaviour
{
    private Image overlayImage;
    private Button cardButton;
    
    [Header("Settings")]
    [SerializeField] private Color disabledColor = new Color(0, 0, 0, 0.5f); // Semi-transparent black
    
    private void Awake()
    {
        overlayImage = GetComponent<Image>();
        cardButton = GetComponentInParent<Button>();
        
        // Start hidden
        overlayImage.enabled = false;
    }
    
    private void Update()
    {
        // Show overlay when button is disabled
        if (cardButton != null)
        {
            bool shouldShow = !cardButton.interactable;
            
            if (overlayImage.enabled != shouldShow)
            {
                overlayImage.enabled = shouldShow;
                overlayImage.color = disabledColor;
            }
        }
    }
}

