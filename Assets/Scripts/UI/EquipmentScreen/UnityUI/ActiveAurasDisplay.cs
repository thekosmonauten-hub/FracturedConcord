using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Displays all active auras as button icons in a horizontal layout
/// Clicking an aura shows its details in EffectDisplay
/// When no aura is selected, shows all active aura effects
/// </summary>
public class ActiveAurasDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform activeAurasContainer; // Container for active aura button icons (Horizontal Layout Group)
    [SerializeField] private GameObject auraButtonPrefab; // Prefab for individual active aura button
    [SerializeField] private Transform effectDisplayContainer; // Container for effect display (AuraNavDisplay/EffectDisplay)
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI headerText; // Optional header text
    
    private List<ActiveAuraButtonUI> activeAuraButtons = new List<ActiveAuraButtonUI>();
    private AuraEffectDisplay effectDisplay;
    private RelianceAura selectedAura = null; // Currently selected aura (null = show all)
    
    void Start()
    {
        // Ensure container has HorizontalLayoutGroup
        if (activeAurasContainer != null)
        {
            HorizontalLayoutGroup layout = activeAurasContainer.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = activeAurasContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 10f;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
            }
        }
        
        // Get or create effect display
        if (effectDisplayContainer != null)
        {
            effectDisplay = effectDisplayContainer.GetComponent<AuraEffectDisplay>();
            if (effectDisplay == null)
            {
                // Try to find it in children
                effectDisplay = effectDisplayContainer.GetComponentInChildren<AuraEffectDisplay>();
                if (effectDisplay == null)
                {
                    // Create it on the container
                    effectDisplay = effectDisplayContainer.gameObject.AddComponent<AuraEffectDisplay>();
                }
            }
        }
        
        Refresh();
    }
    
    /// <summary>
    /// Refresh the active auras display
    /// </summary>
    public void Refresh()
    {
        if (activeAurasContainer == null)
        {
            Debug.LogWarning("[ActiveAurasDisplay] Active auras container is null!");
            return;
        }
        
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            ClearDisplay();
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        
        if (character.activeRelianceAuras == null || character.activeRelianceAuras.Count == 0)
        {
            ClearDisplay();
            return;
        }
        
        // Clear existing buttons
        ClearButtons();
        
        // Create button icons for each active aura
        RelianceAuraDatabase database = RelianceAuraDatabase.Instance;
        
        foreach (string auraName in character.activeRelianceAuras)
        {
            RelianceAura aura = database != null ? database.GetAura(auraName) : null;
            if (aura != null)
            {
                CreateAuraButton(aura);
            }
        }
        
        // Update effect display (show selected aura or all auras)
        UpdateEffectDisplay();
    }
    
    /// <summary>
    /// Create a button icon for an active aura
    /// </summary>
    private void CreateAuraButton(RelianceAura aura)
    {
        GameObject buttonObj;
        
        if (auraButtonPrefab != null)
        {
            buttonObj = Instantiate(auraButtonPrefab, activeAurasContainer);
        }
        else
        {
            buttonObj = new GameObject($"ActiveAuraButton_{aura.auraName}");
            buttonObj.transform.SetParent(activeAurasContainer, false);
            
            // Add Button component
            Button button = buttonObj.AddComponent<Button>();
            
            // Add Image for icon
            Image iconImage = buttonObj.AddComponent<Image>();
            if (aura.icon != null)
            {
                iconImage.sprite = aura.icon;
            }
            iconImage.preserveAspect = true;
            
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(64, 64); // Icon size
        }
        
        ActiveAuraButtonUI buttonUI = buttonObj.GetComponent<ActiveAuraButtonUI>();
        if (buttonUI == null)
        {
            buttonUI = buttonObj.AddComponent<ActiveAuraButtonUI>();
        }
        
        buttonUI.Initialize(aura, this);
        activeAuraButtons.Add(buttonUI);
    }
    
    /// <summary>
    /// Clear all buttons
    /// </summary>
    private void ClearButtons()
    {
        foreach (var button in activeAuraButtons)
        {
            if (button != null && button.gameObject != null)
            {
                Destroy(button.gameObject);
            }
        }
        activeAuraButtons.Clear();
    }
    
    /// <summary>
    /// Clear the entire display
    /// </summary>
    private void ClearDisplay()
    {
        ClearButtons();
        selectedAura = null;
        
        if (effectDisplay != null)
        {
            effectDisplay.Clear();
        }
    }
    
    /// <summary>
    /// Update the effect display based on selection
    /// </summary>
    private void UpdateEffectDisplay()
    {
        if (effectDisplay == null) return;
        
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            effectDisplay.Clear();
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        
        if (selectedAura != null)
        {
            // Show details for selected aura only
            effectDisplay.RefreshSelectedAura(selectedAura);
        }
        else
        {
            // Show all active aura effects
            if (character.activeRelianceAuras != null && character.activeRelianceAuras.Count > 0)
            {
                effectDisplay.Refresh(character.activeRelianceAuras);
            }
            else
            {
                effectDisplay.Clear();
            }
        }
    }
    
    /// <summary>
    /// Select an aura (called by button click)
    /// </summary>
    public void SelectAura(RelianceAura aura)
    {
        if (selectedAura == aura)
        {
            // Deselect if clicking the same aura
            selectedAura = null;
        }
        else
        {
            selectedAura = aura;
        }
        
        // Update button visuals
        foreach (var button in activeAuraButtons)
        {
            if (button != null)
            {
                button.SetSelected(button.GetAura() == selectedAura);
            }
        }
        
        UpdateEffectDisplay();
    }
    
    /// <summary>
    /// Get the currently selected aura (null if showing all)
    /// </summary>
    public RelianceAura GetSelectedAura()
    {
        return selectedAura;
    }
}


