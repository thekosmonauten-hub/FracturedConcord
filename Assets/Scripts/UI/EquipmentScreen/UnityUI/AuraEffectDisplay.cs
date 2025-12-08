using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Displays all effects from active auras
/// Shows in AuraNavDisplay/EffectDisplay
/// </summary>
public class AuraEffectDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform effectContainer; // Container for effect entries
    [SerializeField] private GameObject effectEntryPrefab; // Prefab for individual effect display
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI headerText;
    
    private List<GameObject> effectEntries = new List<GameObject>();
    
    /// <summary>
    /// Refresh the effect display with all active auras
    /// </summary>
    public void Refresh(List<string> activeAuraNames)
    {
        RefreshAuras(activeAuraNames, null);
    }
    
    /// <summary>
    /// Refresh the effect display with a single selected aura
    /// </summary>
    public void RefreshSelectedAura(RelianceAura selectedAura)
    {
        if (selectedAura == null)
        {
            Clear();
            return;
        }
        
        // Show only the selected aura's effects
        Clear();
        
        if (headerText != null)
        {
            headerText.text = $"{selectedAura.auraName} Effects";
        }
        
        RelianceAuraModifierRegistry modifierRegistry = RelianceAuraModifierRegistry.Instance;
        if (modifierRegistry == null)
        {
            Debug.LogWarning("[AuraEffectDisplay] Missing modifier registry!");
            return;
        }
        
        // Create section for selected aura
        CreateAuraSection(selectedAura);
        
        // Display effects for this aura
        if (selectedAura.modifierIds != null && selectedAura.modifierIds.Count > 0)
        {
            foreach (string modifierId in selectedAura.modifierIds)
            {
                RelianceAuraModifierDefinition modifierDef = modifierRegistry.GetModifier(modifierId);
                if (modifierDef != null)
                {
                    CreateEffectEntry(modifierDef);
                }
            }
        }
    }
    
    /// <summary>
    /// Refresh with multiple auras (internal method)
    /// </summary>
    private void RefreshAuras(List<string> activeAuraNames, RelianceAura selectedAura)
    {
        if (effectContainer == null)
        {
            Debug.LogWarning("[AuraEffectDisplay] Effect container is null!");
            return;
        }
        
        Clear();
        
        if (activeAuraNames == null || activeAuraNames.Count == 0)
        {
            if (headerText != null)
            {
                headerText.text = "No Active Auras";
            }
            return;
        }
        
        if (headerText != null)
        {
            if (selectedAura != null)
            {
                headerText.text = $"{selectedAura.auraName} Effects";
            }
            else
            {
                headerText.text = $"Active Aura Effects ({activeAuraNames.Count})";
            }
        }
        
        // Get all effects from active auras
        RelianceAuraDatabase auraDatabase = RelianceAuraDatabase.Instance;
        RelianceAuraModifierRegistry modifierRegistry = RelianceAuraModifierRegistry.Instance;
        
        if (auraDatabase == null || modifierRegistry == null)
        {
            Debug.LogWarning("[AuraEffectDisplay] Missing database or registry!");
            return;
        }
        
        // If a specific aura is selected, only show that one
        if (selectedAura != null)
        {
            CreateAuraSection(selectedAura);
            if (selectedAura.modifierIds != null && selectedAura.modifierIds.Count > 0)
            {
                foreach (string modifierId in selectedAura.modifierIds)
                {
                    RelianceAuraModifierDefinition modifierDef = modifierRegistry.GetModifier(modifierId);
                    if (modifierDef != null)
                    {
                        CreateEffectEntry(modifierDef);
                    }
                }
            }
            return;
        }
        
        // Display effects grouped by aura (all active auras)
        Dictionary<string, RelianceAura> auraMap = new Dictionary<string, RelianceAura>();
        
        foreach (string auraName in activeAuraNames)
        {
            RelianceAura aura = auraDatabase.GetAura(auraName);
            if (aura != null)
            {
                auraMap[auraName] = aura;
            }
        }
        
        // Display effects grouped by aura
        foreach (string auraName in activeAuraNames)
        {
            if (!auraMap.ContainsKey(auraName)) continue;
            
            RelianceAura aura = auraMap[auraName];
            CreateAuraSection(aura);
            
            // Get and display effects for this aura
            if (aura.modifierIds != null && aura.modifierIds.Count > 0)
            {
                foreach (string modifierId in aura.modifierIds)
                {
                    RelianceAuraModifierDefinition modifierDef = modifierRegistry.GetModifier(modifierId);
                    if (modifierDef != null)
                    {
                        CreateEffectEntry(modifierDef);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Create a section header for an aura
    /// </summary>
    private void CreateAuraSection(RelianceAura aura)
    {
        GameObject sectionObj = new GameObject($"AuraSection_{aura.auraName}");
        sectionObj.transform.SetParent(effectContainer, false);
        
        // Add layout component
        HorizontalLayoutGroup layout = sectionObj.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        
        // Add icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(sectionObj.transform, false);
        Image iconImage = iconObj.AddComponent<Image>();
        if (aura.icon != null)
        {
            iconImage.sprite = aura.icon;
        }
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(32, 32);
        
        // Add name text
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(sectionObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = aura.auraName;
        nameText.fontSize = 16;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = aura.themeColor;
        
        effectEntries.Add(sectionObj);
    }
    
    /// <summary>
    /// Create an effect entry
    /// </summary>
    private void CreateEffectEntry(RelianceAuraModifierDefinition modifierDef)
    {
        GameObject entryObj;
        
        if (effectEntryPrefab != null)
        {
            entryObj = Instantiate(effectEntryPrefab, effectContainer);
        }
        else
        {
            entryObj = new GameObject($"EffectEntry_{modifierDef.modifierId}");
            entryObj.transform.SetParent(effectContainer, false);
            
            // Add background
            Image bg = entryObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        }
        
        // Add description text
        TextMeshProUGUI descText = entryObj.GetComponent<TextMeshProUGUI>();
        if (descText == null)
        {
            descText = entryObj.AddComponent<TextMeshProUGUI>();
        }
        
        descText.text = modifierDef.description;
        descText.fontSize = 12;
        descText.color = Color.white;
        
        // Add layout
        LayoutElement layoutElement = entryObj.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = entryObj.AddComponent<LayoutElement>();
        }
        layoutElement.preferredHeight = 30f;
        
        effectEntries.Add(entryObj);
    }
    
    /// <summary>
    /// Clear all effect entries
    /// </summary>
    public void Clear()
    {
        foreach (var entry in effectEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        effectEntries.Clear();
        
        if (headerText != null)
        {
            headerText.text = "No Active Auras";
        }
    }
}

