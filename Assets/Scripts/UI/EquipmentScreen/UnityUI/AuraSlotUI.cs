using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// Individual aura slot in the owned auras grid
/// Displays aura icon, name, and activation state
/// </summary>
public class AuraSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image auraIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI auraNameText;
    [SerializeField] private GameObject activeIndicator;
    [SerializeField] private GameObject lockedIndicator;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color activeColor = new Color(0.2f, 0.6f, 0.2f, 0.8f);
    [SerializeField] private Color hoverColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    [SerializeField] private Color lockedColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    
    private RelianceAura aura;
    private bool isActive = false;
    private bool isOwned = false;
    private AuraStorageUI parentStorage;
    
    public event Action<RelianceAura> OnAuraClicked;
    public event Action<RelianceAura> OnAuraHovered;
    public event Action OnAuraUnhovered;
    
    /// <summary>
    /// Initialize the aura slot with an aura
    /// </summary>
    public void Initialize(RelianceAura auraData, AuraStorageUI storage)
    {
        aura = auraData;
        parentStorage = storage;
        
        if (aura == null)
        {
            ClearSlot();
            return;
        }
        
        // Check if owned and active
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager != null && characterManager.HasCharacter())
        {
            Character character = characterManager.GetCurrentCharacter();
            isOwned = character.ownedRelianceAuras != null && character.ownedRelianceAuras.Contains(aura.auraName);
            isActive = character.activeRelianceAuras != null && character.activeRelianceAuras.Contains(aura.auraName);
        }
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// Update the visual display of the slot
    /// </summary>
    private void UpdateDisplay()
    {
        if (aura == null)
        {
            ClearSlot();
            return;
        }
        
        // Set icon
        if (auraIcon != null && aura.icon != null)
        {
            auraIcon.sprite = aura.icon;
            auraIcon.color = isOwned ? Color.white : Color.gray;
        }
        
        // Set name
        if (auraNameText != null)
        {
            auraNameText.text = aura.auraName;
            auraNameText.color = isOwned ? Color.white : Color.gray;
        }
        
        // Set background color
        if (backgroundImage != null)
        {
            if (!isOwned)
            {
                backgroundImage.color = lockedColor;
            }
            else if (isActive)
            {
                backgroundImage.color = activeColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }
        
        // Show/hide active indicator
        if (activeIndicator != null)
        {
            activeIndicator.SetActive(isActive && isOwned);
        }
        
        // Show/hide locked indicator
        if (lockedIndicator != null)
        {
            lockedIndicator.SetActive(!isOwned);
        }
    }
    
    /// <summary>
    /// Clear the slot (show empty state)
    /// </summary>
    public void ClearSlot()
    {
        aura = null;
        isActive = false;
        isOwned = false;
        
        if (auraIcon != null)
        {
            auraIcon.sprite = null;
            auraIcon.color = Color.clear;
        }
        
        if (auraNameText != null)
        {
            auraNameText.text = "";
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
        
        if (activeIndicator != null)
        {
            activeIndicator.SetActive(false);
        }
        
        if (lockedIndicator != null)
        {
            lockedIndicator.SetActive(false);
        }
    }
    
    /// <summary>
    /// Refresh the slot display (update active/owned state)
    /// </summary>
    public void Refresh()
    {
        if (aura == null) return;
        
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager != null && characterManager.HasCharacter())
        {
            Character character = characterManager.GetCurrentCharacter();
            isOwned = character.ownedRelianceAuras != null && character.ownedRelianceAuras.Contains(aura.auraName);
            isActive = character.activeRelianceAuras != null && character.activeRelianceAuras.Contains(aura.auraName);
        }
        
        UpdateDisplay();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (aura == null || !isOwned) return;
        
        OnAuraClicked?.Invoke(aura);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (aura == null) return;
        
        // Highlight on hover
        if (backgroundImage != null && isOwned && !isActive)
        {
            backgroundImage.color = hoverColor;
        }
        
        // Show tooltip
        if (ItemTooltipManager.Instance != null)
        {
            Character character = CharacterManager.Instance?.GetCurrentCharacter();
            ItemTooltipManager.Instance.ShowAuraTooltip(aura, eventData.position, character);
        }
        
        OnAuraHovered?.Invoke(aura);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Restore normal color
        UpdateDisplay();
        
        // Hide tooltip
        if (ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.HideTooltip();
        }
        
        OnAuraUnhovered?.Invoke();
    }
    
    /// <summary>
    /// Get the aura in this slot
    /// </summary>
    public RelianceAura GetAura()
    {
        return aura;
    }
    
    /// <summary>
    /// Check if this slot is active
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Check if this slot is owned
    /// </summary>
    public bool IsOwned()
    {
        return isOwned;
    }
}

