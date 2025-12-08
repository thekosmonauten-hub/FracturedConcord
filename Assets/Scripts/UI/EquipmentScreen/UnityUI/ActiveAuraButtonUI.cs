using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Button icon for an active aura in the horizontal layout
/// Clicking selects/deselects the aura to show details
/// </summary>
public class ActiveAuraButtonUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject selectedIndicator;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color selectedColor = new Color(0.4f, 0.8f, 1f, 1f);
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 1f);
    
    private RelianceAura aura;
    private ActiveAurasDisplay parentDisplay;
    private bool isSelected = false;
    
    /// <summary>
    /// Initialize the button with an aura
    /// </summary>
    public void Initialize(RelianceAura auraData, ActiveAurasDisplay display)
    {
        aura = auraData;
        parentDisplay = display;
        
        if (aura == null) return;
        
        // Set icon
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
        
        if (iconImage != null && aura.icon != null)
        {
            iconImage.sprite = aura.icon;
            iconImage.color = normalColor;
        }
        
        // Set background if available
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
        
        // Ensure button component exists
        Button button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
    }
    
    /// <summary>
    /// Set the selected state
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (iconImage != null)
        {
            iconImage.color = selected ? selectedColor : normalColor;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
        
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(selected);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (aura == null || parentDisplay == null) return;
        
        parentDisplay.SelectAura(aura);
    }
    
    /// <summary>
    /// Get the aura in this button
    /// </summary>
    public RelianceAura GetAura()
    {
        return aura;
    }
}

