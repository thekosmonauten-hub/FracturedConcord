using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI item for displaying and selecting notables in the fusion UI.
/// Only one notable can be selected at a time.
/// </summary>
public class WarrantNotableLockItem : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private Toggle selectionToggle;
    [SerializeField] private TextMeshProUGUI notableNameText;
    [SerializeField] private TextMeshProUGUI notableDescriptionText;
    [SerializeField] private TextMeshProUGUI slotLabelText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button confirmButton;
    
    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color unselectedColor = Color.white;
    [SerializeField] private Color pendingColor = Color.cyan; // Color when toggled but not confirmed
    
    private int slotIndex;
    private WarrantNotableDefinition notable;
    private WarrantFusionUI parentUI;
    private bool isInitialized = false;
    
    public void Initialize(int slotIdx, WarrantNotableDefinition notableDef, WarrantFusionUI parent)
    {
        slotIndex = slotIdx;
        notable = notableDef;
        parentUI = parent;
        
        if (notableNameText != null && notable != null)
        {
            notableNameText.text = !string.IsNullOrEmpty(notable.displayName) 
                ? notable.displayName 
                : "Unknown Notable";
        }
        
        if (notableDescriptionText != null && notable != null)
        {
            notableDescriptionText.text = !string.IsNullOrEmpty(notable.description) 
                ? notable.description 
                : "";
        }
        
        if (slotLabelText != null)
        {
            slotLabelText.text = $"Slot {slotIndex + 1}";
        }
        
        if (selectionToggle != null)
        {
            selectionToggle.onValueChanged.RemoveAllListeners();
            selectionToggle.isOn = false;
            selectionToggle.onValueChanged.AddListener(OnToggleChanged);
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
    
    private void OnToggleChanged(bool isSelected)
    {
        if (!isInitialized || notable == null || parentUI == null)
            return;
        
        // Notify parent that this notable is toggled (pending selection)
        parentUI.ToggleNotableSelection(slotIndex, notable, isSelected, this);
        UpdateVisuals();
    }
    
    private void OnConfirmClicked()
    {
        if (!isInitialized || notable == null || parentUI == null)
            return;
        
        // Confirm the selection
        parentUI.ConfirmNotableSelection(slotIndex, notable);
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (selectionToggle == null)
            return;
        
        bool isToggled = selectionToggle.isOn;
        bool isPending = isToggled && confirmButton != null && confirmButton.gameObject.activeSelf;
        
        if (backgroundImage != null)
        {
            if (isPending)
            {
                backgroundImage.color = pendingColor;
            }
            else if (isToggled)
            {
                backgroundImage.color = selectedColor;
            }
            else
            {
                backgroundImage.color = unselectedColor;
            }
        }
        
        // Show confirm button only when toggled and not yet confirmed
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(isToggled && !isPending);
        }
    }
    
    public void SetSelected(bool selected)
    {
        if (selectionToggle != null)
        {
            selectionToggle.isOn = selected;
        }
        UpdateVisuals();
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

