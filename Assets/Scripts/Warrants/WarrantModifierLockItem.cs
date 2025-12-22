using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI item for displaying and toggling modifier locks in the fusion UI.
/// </summary>
public class WarrantModifierLockItem : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private Toggle lockToggle;
    [SerializeField] private TextMeshProUGUI modifierNameText;
    [SerializeField] private TextMeshProUGUI modifierValueText;
    [SerializeField] private TextMeshProUGUI slotLabelText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button confirmButton;
    
    [Header("Colors")]
    [SerializeField] private Color lockedColor = new Color(1f, 0.84f, 0f); // Gold/Yellow for locked/sealed affixes
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color pendingColor = Color.cyan; // Color when toggled but not confirmed
    [SerializeField] private Color fusedColor = new Color(1f, 0.84f, 0f); // Gold/Yellow for locked/sealed affixes (same as lockedColor)
    
    private int slotIndex;
    private WarrantModifier modifier;
    private WarrantFusionUI parentUI;
    private bool isInitialized = false;
    private bool isFused = false; // True if this affix is from a previously fused warrant (locked/permanent)
    
    public void Initialize(int slotIdx, WarrantModifier mod, WarrantFusionUI parent, bool fused = false)
    {
        slotIndex = slotIdx;
        modifier = mod;
        parentUI = parent;
        isFused = fused;
        
        if (modifierNameText != null && modifier != null)
        {
            modifierNameText.text = !string.IsNullOrEmpty(modifier.displayName) 
                ? modifier.displayName 
                : modifier.modifierId;
        }
        
        if (modifierValueText != null && modifier != null)
        {
            modifierValueText.text = !string.IsNullOrEmpty(modifier.description) 
                ? modifier.description 
                : $"{modifier.value:+#;-#;0}";
        }
        
        if (slotLabelText != null)
        {
            slotLabelText.text = $"Slot {slotIndex + 1}";
        }
        
        if (lockToggle != null)
        {
            lockToggle.onValueChanged.RemoveAllListeners();
            lockToggle.isOn = false;
            lockToggle.interactable = !isFused; // Disable toggle if this is a fused affix
            lockToggle.onValueChanged.AddListener(OnToggleChanged);
        }
        
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.gameObject.SetActive(false); // Hidden by default
        }
        
        UpdateVisuals();
        isInitialized = true;
    }
    
    private void OnToggleChanged(bool isLocked)
    {
        if (!isInitialized || modifier == null || parentUI == null)
            return;
        
        // Prevent toggling if this is a fused affix
        if (isFused)
        {
            if (lockToggle != null)
            {
                lockToggle.isOn = false;
            }
            return;
        }
        
        // Notify parent that this affix is toggled (pending selection)
        parentUI.ToggleAffixSelection(slotIndex, modifier.modifierId, isLocked, this);
        UpdateVisuals();
    }
    
    private void OnConfirmClicked()
    {
        if (!isInitialized || modifier == null || parentUI == null)
            return;
        
        // Confirm the selection
        parentUI.ConfirmAffixSelection(slotIndex, modifier);
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (lockToggle == null)
            return;
        
        bool isToggled = lockToggle.isOn;
        bool isPending = isToggled && confirmButton != null && confirmButton.gameObject.activeSelf;
        
        if (backgroundImage != null)
        {
            if (isFused)
            {
                // Fused affixes are permanently locked and cannot be selected
                backgroundImage.color = fusedColor;
            }
            else if (isPending)
            {
                backgroundImage.color = pendingColor;
            }
            else if (isToggled)
            {
                backgroundImage.color = lockedColor;
            }
            else
            {
                backgroundImage.color = unlockedColor;
            }
        }
        
        // Show confirm button only when toggled and not yet confirmed (and not fused)
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(isToggled && !isPending && !isFused);
        }
    }
    
    public void SetConfirmed(bool confirmed)
    {
        // Hide confirm button when confirmed
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(false);
        }
        UpdateVisuals();
    }
}

