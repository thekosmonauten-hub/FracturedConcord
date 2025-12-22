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
    [SerializeField] private Transform effectContainer; // Container for effect entries (should be ScrollView Content)
    [SerializeField] private GameObject effectEntryPrefab; // Prefab for individual effect display
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI headerText;
    
    private List<GameObject> effectEntries = new List<GameObject>();
    
    void Awake()
    {
        // Auto-detect effectContainer if not set
        if (effectContainer == null)
        {
            // Try to find a ScrollView and use its Content
            UnityEngine.UI.ScrollRect scrollRect = GetComponentInParent<UnityEngine.UI.ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
            {
                effectContainer = scrollRect.content;
                Debug.Log($"[AuraEffectDisplay] Auto-detected effectContainer from ScrollView: {effectContainer.name}");
            }
            else
            {
                // Try to find a child named "Content" or "EffectsScrollView/Content"
                Transform contentTransform = transform.Find("Content");
                if (contentTransform == null)
                {
                    Transform scrollView = transform.Find("EffectsScrollView");
                    if (scrollView != null)
                    {
                        contentTransform = scrollView.Find("Content");
                    }
                }
                if (contentTransform != null)
                {
                    effectContainer = contentTransform;
                    Debug.Log($"[AuraEffectDisplay] Auto-detected effectContainer: {effectContainer.name}");
                }
            }
        }
        
        // Ensure container has VerticalLayoutGroup for proper layout
        if (effectContainer != null)
        {
            VerticalLayoutGroup layout = effectContainer.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = effectContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 5f;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            }
            
            // Add ContentSizeFitter to auto-resize
            ContentSizeFitter fitter = effectContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = effectContainer.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
    }
    
    /// <summary>
    /// Refresh the effect display with all active auras
    /// </summary>
    public void Refresh(List<string> activeAuraNames)
    {
        if (effectContainer == null)
        {
            Debug.LogError("[AuraEffectDisplay] Cannot refresh - effectContainer is null! Please assign it in the Inspector or ensure a ScrollView Content exists.");
            return;
        }
        
        Debug.Log($"[AuraEffectDisplay] Refreshing with {activeAuraNames?.Count ?? 0} active auras");
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
        
        // Display effects for this aura (combine all modifiers into one entry)
        if (selectedAura.modifierIds != null && selectedAura.modifierIds.Count > 0)
        {
            // Collect all modifiers (excluding RollDamagePerTurn)
            List<RelianceAuraModifierDefinition> displayModifiers = new List<RelianceAuraModifierDefinition>();
            
            foreach (string modifierId in selectedAura.modifierIds)
            {
                RelianceAuraModifierDefinition modifierDef = modifierRegistry.GetModifier(modifierId);
                if (modifierDef != null)
                {
                    // Filter out RollDamagePerTurn modifiers
                    bool shouldExclude = false;
                    if (modifierDef.effects != null)
                    {
                        foreach (var effect in modifierDef.effects)
                        {
                            if (effect.actions != null)
                            {
                                foreach (var action in effect.actions)
                                {
                                    if (action.actionType == ModifierActionType.RollDamagePerTurn)
                                    {
                                        shouldExclude = true;
                                        break;
                                    }
                                }
                            }
                            if (shouldExclude) break;
                        }
                    }
                    
                    if (!shouldExclude)
                    {
                        displayModifiers.Add(modifierDef);
                    }
                }
            }
            
            // Create a single combined effect entry
            if (displayModifiers.Count > 0)
            {
                CreateCombinedEffectEntry(selectedAura, displayModifiers);
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
                // Collect all modifiers (excluding RollDamagePerTurn)
                List<RelianceAuraModifierDefinition> displayModifiers = new List<RelianceAuraModifierDefinition>();
                
                foreach (string modifierId in selectedAura.modifierIds)
                {
                    RelianceAuraModifierDefinition modifierDef = modifierRegistry.GetModifier(modifierId);
                    if (modifierDef != null)
                    {
                        // Filter out RollDamagePerTurn modifiers
                        bool shouldExclude = false;
                        if (modifierDef.effects != null)
                        {
                            foreach (var effect in modifierDef.effects)
                            {
                                if (effect.actions != null)
                                {
                                    foreach (var action in effect.actions)
                                    {
                                        if (action.actionType == ModifierActionType.RollDamagePerTurn)
                                        {
                                            shouldExclude = true;
                                            break;
                                        }
                                    }
                                }
                                if (shouldExclude) break;
                            }
                        }
                        
                        if (!shouldExclude)
                        {
                            displayModifiers.Add(modifierDef);
                        }
                    }
                }
                
                // Create a single combined effect entry
                if (displayModifiers.Count > 0)
                {
                    CreateCombinedEffectEntry(selectedAura, displayModifiers);
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
        
        Debug.Log($"[AuraEffectDisplay] Found {auraMap.Count} auras in database");
        
        // Display effects grouped by aura (one entry per aura, combining all modifiers)
        foreach (string auraName in activeAuraNames)
        {
            if (!auraMap.ContainsKey(auraName))
            {
                Debug.LogWarning($"[AuraEffectDisplay] Aura '{auraName}' not found in database!");
                continue;
            }
            
            RelianceAura aura = auraMap[auraName];
            CreateAuraSection(aura);
            
            // Get and display effects for this aura (combine all modifiers into one entry)
            if (aura.modifierIds == null || aura.modifierIds.Count == 0)
            {
                Debug.LogWarning($"[AuraEffectDisplay] Aura '{auraName}' has no modifier IDs! Re-import the auras to link modifiers.");
            }
            else
            {
                Debug.Log($"[AuraEffectDisplay] Aura '{auraName}' has {aura.modifierIds.Count} modifier IDs: {string.Join(", ", aura.modifierIds)}");
                
                // Collect all modifiers for this aura (excluding RollDamagePerTurn)
                List<RelianceAuraModifierDefinition> displayModifiers = new List<RelianceAuraModifierDefinition>();
                
                foreach (string modifierId in aura.modifierIds)
                {
                    RelianceAuraModifierDefinition modifierDef = modifierRegistry.GetModifier(modifierId);
                    if (modifierDef != null)
                    {
                        // Filter out RollDamagePerTurn modifiers (they're just mechanics, not display info)
                        bool shouldExclude = false;
                        if (modifierDef.effects != null)
                        {
                            foreach (var effect in modifierDef.effects)
                            {
                                if (effect.actions != null)
                                {
                                    foreach (var action in effect.actions)
                                    {
                                        if (action.actionType == ModifierActionType.RollDamagePerTurn)
                                        {
                                            shouldExclude = true;
                                            Debug.Log($"[AuraEffectDisplay] Excluding RollDamagePerTurn modifier: {modifierId}");
                                            break;
                                        }
                                    }
                                }
                                if (shouldExclude) break;
                            }
                        }
                        
                        if (!shouldExclude)
                        {
                            displayModifiers.Add(modifierDef);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[AuraEffectDisplay] Modifier '{modifierId}' not found in registry!");
                    }
                }
                
                // Create a single combined effect entry for this aura
                if (displayModifiers.Count > 0)
                {
                    CreateCombinedEffectEntry(aura, displayModifiers);
                }
            }
        }
        
        Debug.Log($"[AuraEffectDisplay] Created {effectEntries.Count} effect entries total");
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
    /// Create a combined effect entry for an aura (combines all modifiers into one display)
    /// </summary>
    private void CreateCombinedEffectEntry(RelianceAura aura, List<RelianceAuraModifierDefinition> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
            return;
        
        // Use the first modifier's description as base, or combine them
        string combinedDescription = "";
        
        if (modifiers.Count == 1)
        {
            combinedDescription = modifiers[0].description;
        }
        else
        {
            // Combine descriptions (remove duplicates and combine intelligently)
            List<string> descriptions = new List<string>();
            foreach (var mod in modifiers)
            {
                if (!string.IsNullOrEmpty(mod.description))
                {
                    descriptions.Add(mod.description);
                }
            }
            combinedDescription = string.Join(". ", descriptions);
        }
        
        // Create entry using the combined description
        GameObject entryObj;
        
        if (effectEntryPrefab != null)
        {
            entryObj = Instantiate(effectEntryPrefab, effectContainer);
        }
        else
        {
            entryObj = new GameObject($"EffectEntry_{aura.auraName}");
            entryObj.transform.SetParent(effectContainer, false);
            
            // Add background
            Image bg = entryObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        }
        
        // Find or create text component
        TextMeshProUGUI descText = entryObj.GetComponent<TextMeshProUGUI>();
        
        if (descText == null)
        {
            descText = entryObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (descText == null)
            {
                // Create a child GameObject for the text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(entryObj.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
                
                descText = textObj.AddComponent<TextMeshProUGUI>();
            }
        }
        
        if (descText != null)
        {
            descText.text = combinedDescription;
            descText.fontSize = 12;
            descText.color = Color.white;
        }
        
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
    /// Create an effect entry (legacy method - kept for backwards compatibility)
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
        
        // Find or create text component (prefab might already have Image, so create child for text)
        TextMeshProUGUI descText = entryObj.GetComponent<TextMeshProUGUI>();
        
        if (descText == null)
        {
            // Check if prefab has a child with TextMeshProUGUI
            descText = entryObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (descText == null)
            {
                // Create a child GameObject for the text (since entryObj might have Image)
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(entryObj.transform, false);
                
                // Set up RectTransform to fill parent
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                if (textRect == null)
                {
                    textRect = textObj.AddComponent<RectTransform>();
                }
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
                
                descText = textObj.AddComponent<TextMeshProUGUI>();
            }
        }
        
        if (descText != null)
        {
            descText.text = modifierDef.description;
            descText.fontSize = 12;
            descText.color = Color.white;
        }
        else
        {
            Debug.LogError($"[AuraEffectDisplay] Failed to create text component for effect entry: {modifierDef.modifierId}");
        }
        
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

