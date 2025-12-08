using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual active aura entry UI
/// Displays a single active aura with deactivate button
/// </summary>
public class ActiveAuraEntryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image auraIcon;
    [SerializeField] private TextMeshProUGUI auraNameText;
    [SerializeField] private TextMeshProUGUI relianceCostText;
    [SerializeField] private Button deactivateButton;
    
    private RelianceAura aura;
    
    public event System.Action OnDeactivateClicked;
    
    /// <summary>
    /// Initialize the entry with an aura
    /// </summary>
    public void Initialize(RelianceAura auraData)
    {
        aura = auraData;
        
        if (aura == null) return;
        
        // Set icon
        if (auraIcon != null && aura.icon != null)
        {
            auraIcon.sprite = aura.icon;
        }
        
        // Set name
        if (auraNameText != null)
        {
            auraNameText.text = aura.auraName;
        }
        
        // Set cost
        if (relianceCostText != null)
        {
            relianceCostText.text = $"Cost: {aura.relianceCost}";
        }
        
        // Setup deactivate button
        if (deactivateButton != null)
        {
            deactivateButton.onClick.RemoveAllListeners();
            deactivateButton.onClick.AddListener(() => OnDeactivateClicked?.Invoke());
        }
    }
    
    /// <summary>
    /// Get the aura in this entry
    /// </summary>
    public RelianceAura GetAura()
    {
        return aura;
    }
}

