using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Represents a single entry in the combat log with optional hoverable loot items
/// </summary>
public class CombatLogEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public TextMeshProUGUI logText;
    public Image backgroundImage;
    
    [Header("Colors")]
    public Color normalColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color hoverColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    
    private LootReward associatedLoot;
    private bool hasLoot = false;
    private CombatLogTooltip tooltip;
    
    public void Initialize(string message, LootReward loot = null)
    {
        if (logText != null)
        {
            logText.text = message;
        }
        
        if (loot != null)
        {
            associatedLoot = loot;
            hasLoot = true;
        }
        
        // Find tooltip
        tooltip = FindFirstObjectByType<CombatLogTooltip>();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = hoverColor;
        }
        
        if (hasLoot && tooltip != null && associatedLoot != null)
        {
            tooltip.ShowLootTooltip(associatedLoot, eventData.position);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
        
        if (tooltip != null)
        {
            tooltip.HideTooltip();
        }
    }
}













