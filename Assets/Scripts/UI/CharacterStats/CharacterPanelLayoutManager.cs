using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterPanelLayoutManager : MonoBehaviour
{
    [Header("Panel References")]
    public RectTransform mainContainer;
    public RectTransform contentArea;
    public ScrollRect scrollRect;
    
    [Header("Section References")]
    public RectTransform characterInfoSection;
    public RectTransform resourceBarsSection;
    public RectTransform attributesSection;
    public RectTransform combatStatsSection;
    public RectTransform damageModifiersSection;
    public RectTransform resistancesSection;
    public RectTransform defenseStatsSection;
    public RectTransform recoveryStatsSection;
    public RectTransform combatMechanicsSection;
    public RectTransform cardSystemSection;
    public RectTransform equipmentSummarySection;
    
    [Header("Layout Settings")]
    public float sectionSpacing = 10f;
    public float headerHeight = 30f;
    public float statRowHeight = 25f;
    public Vector2 panelSize = new Vector2(800, 600);
    public Vector2 contentPadding = new Vector2(20, 20);
    
    [Header("Auto Setup")]
    public bool autoSetupOnStart = true;
    public bool autoPositionSections = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPanelLayout();
        }
    }
    
    [ContextMenu("Setup Panel Layout")]
    public void SetupPanelLayout()
    {
        if (mainContainer == null)
        {
            Debug.LogError("MainContainer not assigned!");
            return;
        }
        
        // Set panel size
        mainContainer.sizeDelta = panelSize;
        
        // Setup content area
        if (contentArea != null)
        {
            SetupContentArea();
        }
        
        // Position sections
        if (autoPositionSections)
        {
            PositionAllSections();
        }
        
        // Setup scroll rect
        if (scrollRect != null)
        {
            SetupScrollRect();
        }
    }
    
    private void SetupContentArea()
    {
        // Set content area to fill the available space
        contentArea.anchorMin = Vector2.zero;
        contentArea.anchorMax = Vector2.one;
        contentArea.offsetMin = contentPadding;
        contentArea.offsetMax = -contentPadding;
    }
    
    private void SetupScrollRect()
    {
        if (scrollRect.content != null)
        {
            // Ensure content has proper layout group
            var layoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = scrollRect.content.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            
            layoutGroup.spacing = sectionSpacing;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
        }
    }
    
    [ContextMenu("Position All Sections")]
    public void PositionAllSections()
    {
        float currentY = 0f;
        
        // Position each section vertically
        PositionSection(characterInfoSection, ref currentY, "Character Info");
        PositionSection(resourceBarsSection, ref currentY, "Resource Bars");
        PositionSection(attributesSection, ref currentY, "Attributes");
        PositionSection(combatStatsSection, ref currentY, "Combat Stats");
        PositionSection(damageModifiersSection, ref currentY, "Damage Modifiers");
        PositionSection(resistancesSection, ref currentY, "Resistances");
        PositionSection(defenseStatsSection, ref currentY, "Defense Stats");
        PositionSection(recoveryStatsSection, ref currentY, "Recovery Stats");
        PositionSection(combatMechanicsSection, ref currentY, "Combat Mechanics");
        PositionSection(cardSystemSection, ref currentY, "Card System");
        PositionSection(equipmentSummarySection, ref currentY, "Equipment Summary");
    }
    
    private void PositionSection(RectTransform section, ref float currentY, string sectionName)
    {
        if (section == null)
        {
            Debug.LogWarning($"Section {sectionName} not assigned!");
            return;
        }
        
        // Set anchor to top
        section.anchorMin = new Vector2(0, 1);
        section.anchorMax = new Vector2(1, 1);
        
        // Position at current Y
        section.anchoredPosition = new Vector2(0, -currentY);
        
        // Set size
        section.sizeDelta = new Vector2(0, headerHeight);
        
        // Add spacing for next section
        currentY += headerHeight + sectionSpacing;
        
        Debug.Log($"Positioned {sectionName} at Y: {-currentY}");
    }
    
    [ContextMenu("Center Panel")]
    public void CenterPanel()
    {
        if (mainContainer != null)
        {
            mainContainer.anchorMin = new Vector2(0.5f, 0.5f);
            mainContainer.anchorMax = new Vector2(0.5f, 0.5f);
            mainContainer.anchoredPosition = Vector2.zero;
        }
    }
    
    [ContextMenu("Stretch Panel")]
    public void StretchPanel()
    {
        if (mainContainer != null)
        {
            mainContainer.anchorMin = Vector2.zero;
            mainContainer.anchorMax = Vector2.one;
            mainContainer.offsetMin = Vector2.zero;
            mainContainer.offsetMax = Vector2.zero;
        }
    }
    
    [ContextMenu("Reset Panel Position")]
    public void ResetPanelPosition()
    {
        if (mainContainer != null)
        {
            mainContainer.anchorMin = new Vector2(0.5f, 0.5f);
            mainContainer.anchorMax = new Vector2(0.5f, 0.5f);
            mainContainer.anchoredPosition = Vector2.zero;
            mainContainer.sizeDelta = panelSize;
        }
    }
    
    // Helper method to find and assign sections automatically
    [ContextMenu("Auto Assign Sections")]
    public void AutoAssignSections()
    {
        Transform content = scrollRect?.content;
        if (content == null)
        {
            Debug.LogError("ScrollRect content not found!");
            return;
        }
        
        // Find sections by name
        characterInfoSection = FindSection(content, "CharacterInfoSection");
        resourceBarsSection = FindSection(content, "ResourceBarsSection");
        attributesSection = FindSection(content, "AttributesSection");
        combatStatsSection = FindSection(content, "CombatStatsSection");
        damageModifiersSection = FindSection(content, "DamageModifiersSection");
        resistancesSection = FindSection(content, "ResistancesSection");
        defenseStatsSection = FindSection(content, "DefenseStatsSection");
        recoveryStatsSection = FindSection(content, "RecoveryStatsSection");
        combatMechanicsSection = FindSection(content, "CombatMechanicsSection");
        cardSystemSection = FindSection(content, "CardSystemSection");
        equipmentSummarySection = FindSection(content, "EquipmentSummarySection");
        
        Debug.Log("Auto-assigned sections completed!");
    }
    
    private RectTransform FindSection(Transform parent, string sectionName)
    {
        Transform found = parent.Find(sectionName);
        if (found != null)
        {
            return found.GetComponent<RectTransform>();
        }
        
        // Try to find by partial name match
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.ToLower().Contains(sectionName.ToLower()))
            {
                return child.GetComponent<RectTransform>();
            }
        }
        
        Debug.LogWarning($"Section {sectionName} not found!");
        return null;
    }
}











