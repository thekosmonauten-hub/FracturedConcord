using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class CharacterPanelEditorHelper : EditorWindow
{
    private GameObject characterPanel;
    private RectTransform mainContainer;
    private ScrollRect scrollRect;
    
    private Vector2 panelSize = new Vector2(800, 600);
    private float sectionSpacing = 10f;
    private float headerHeight = 30f;
    
    [MenuItem("Tools/Character Panel Helper")]
    public static void ShowWindow()
    {
        GetWindow<CharacterPanelEditorHelper>("Character Panel Helper");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Character Panel Layout Helper", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // Panel References
        GUILayout.Label("Panel References", EditorStyles.boldLabel);
        characterPanel = (GameObject)EditorGUILayout.ObjectField("Character Panel", characterPanel, typeof(GameObject), true);
        
        if (characterPanel != null)
        {
            mainContainer = characterPanel.GetComponent<RectTransform>();
            scrollRect = characterPanel.GetComponentInChildren<ScrollRect>();
        }
        
        EditorGUILayout.Space();
        
        // Layout Settings
        GUILayout.Label("Layout Settings", EditorStyles.boldLabel);
        panelSize = EditorGUILayout.Vector2Field("Panel Size", panelSize);
        sectionSpacing = EditorGUILayout.FloatField("Section Spacing", sectionSpacing);
        headerHeight = EditorGUILayout.FloatField("Header Height", headerHeight);
        
        EditorGUILayout.Space();
        
        // Action Buttons
        GUILayout.Label("Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Setup Basic Panel Structure"))
        {
            SetupBasicPanelStructure();
        }
        
        if (GUILayout.Button("Position All Sections"))
        {
            PositionAllSections();
        }
        
        if (GUILayout.Button("Center Panel"))
        {
            CenterPanel();
        }
        
        if (GUILayout.Button("Stretch Panel"))
        {
            StretchPanel();
        }
        
        if (GUILayout.Button("Auto Assign Components"))
        {
            AutoAssignComponents();
        }
        
        EditorGUILayout.Space();
        
        // Quick Actions
        GUILayout.Label("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Character Info Section"))
        {
            CreateSection("CharacterInfoSection", "Character Information");
        }
        
        if (GUILayout.Button("Create Resource Bars Section"))
        {
            CreateSection("ResourceBarsSection", "Resources");
        }
        
        if (GUILayout.Button("Create Attributes Section"))
        {
            CreateSection("AttributesSection", "Core Attributes");
        }
        
        if (GUILayout.Button("Create Combat Stats Section"))
        {
            CreateSection("CombatStatsSection", "Combat Stats");
        }
        
        if (GUILayout.Button("Create All Sections"))
        {
            CreateAllSections();
        }
    }
    
    private void SetupBasicPanelStructure()
    {
        if (characterPanel == null)
        {
            Debug.LogError("Please assign a Character Panel first!");
            return;
        }
        
        // Ensure it has Canvas
        Canvas canvas = characterPanel.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = characterPanel.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add CanvasScaler
        CanvasScaler scaler = characterPanel.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = characterPanel.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster
        GraphicRaycaster raycaster = characterPanel.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = characterPanel.AddComponent<GraphicRaycaster>();
        }
        
        // Create main container
        GameObject mainContainerObj = CreateChild(characterPanel, "MainContainer");
        mainContainer = mainContainerObj.GetComponent<RectTransform>();
        
        // Add VerticalLayoutGroup to main container
        VerticalLayoutGroup mainLayout = mainContainerObj.GetComponent<VerticalLayoutGroup>();
        if (mainLayout == null)
        {
            mainLayout = mainContainerObj.AddComponent<VerticalLayoutGroup>();
        }
        mainLayout.spacing = 5f;
        mainLayout.padding = new RectOffset(10, 10, 10, 10);
        mainLayout.childForceExpandWidth = true;
        mainLayout.childForceExpandHeight = false;
        
        // Create header
        GameObject header = CreateChild(mainContainerObj, "Header");
        HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = true;
        
        // Add title
        GameObject title = CreateChild(header, "Title");
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "Character Stats";
        titleText.fontSize = 24;
        titleText.color = Color.white;
        
        // Add close button
        GameObject closeButton = CreateChild(header, "CloseButton");
        Button button = closeButton.AddComponent<Button>();
        Image buttonImage = closeButton.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.2f, 0.2f);
        
        GameObject buttonText = CreateChild(closeButton, "Text");
        TextMeshProUGUI buttonTextComponent = buttonText.AddComponent<TextMeshProUGUI>();
        buttonTextComponent.text = "X";
        buttonTextComponent.fontSize = 18;
        buttonTextComponent.color = Color.white;
        
        // Create content area
        GameObject contentArea = CreateChild(mainContainerObj, "ContentArea");
        scrollRect = contentArea.AddComponent<ScrollRect>();
        
        // Create viewport
        GameObject viewport = CreateChild(contentArea, "Viewport");
        viewport.AddComponent<Image>();
        viewport.AddComponent<Mask>();
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        // Create content
        GameObject content = CreateChild(viewport, "Content");
        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = sectionSpacing;
        contentLayout.padding = new RectOffset(10, 10, 10, 10);
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        
        // Setup scroll rect
        scrollRect.viewport = viewportRect;
        scrollRect.content = content.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        
        // Create footer
        GameObject footer = CreateChild(mainContainerObj, "Footer");
        HorizontalLayoutGroup footerLayout = footer.AddComponent<HorizontalLayoutGroup>();
        footerLayout.childForceExpandWidth = true;
        footerLayout.childForceExpandHeight = true;
        
        // Add footer text
        GameObject footerText = CreateChild(footer, "FooterText");
        TextMeshProUGUI footerTextComponent = footerText.AddComponent<TextMeshProUGUI>();
        footerTextComponent.text = "Character Stats Panel";
        footerTextComponent.fontSize = 12;
        footerTextComponent.color = Color.gray;
        
        // Set panel size
        mainContainer.sizeDelta = panelSize;
        
        Debug.Log("Basic panel structure created successfully!");
    }
    
    private void CreateSection(string sectionName, string headerText)
    {
        if (scrollRect?.content == null)
        {
            Debug.LogError("ScrollRect content not found! Please setup basic structure first.");
            return;
        }
        
        GameObject section = CreateChild(scrollRect.content.gameObject, sectionName);
        VerticalLayoutGroup sectionLayout = section.AddComponent<VerticalLayoutGroup>();
        sectionLayout.spacing = 5f;
        sectionLayout.childForceExpandWidth = true;
        sectionLayout.childForceExpandHeight = false;
        
        // Add section header
        GameObject header = CreateChild(section, "SectionHeader");
        TextMeshProUGUI headerTextComponent = header.AddComponent<TextMeshProUGUI>();
        headerTextComponent.text = headerText;
        headerTextComponent.fontSize = 16;
        headerTextComponent.fontStyle = FontStyles.Bold;
        headerTextComponent.color = new Color(0.7f, 0.9f, 1.0f);
        
        // Add placeholder content
        GameObject placeholder = CreateChild(section, "Placeholder");
        TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderText.text = $"Content for {headerText} will be added here...";
        placeholderText.fontSize = 12;
        placeholderText.color = Color.gray;
        
        Debug.Log($"Created section: {sectionName}");
    }
    
    private void CreateAllSections()
    {
        CreateSection("CharacterInfoSection", "Character Information");
        CreateSection("ResourceBarsSection", "Resources");
        CreateSection("AttributesSection", "Core Attributes");
        CreateSection("CombatStatsSection", "Combat Stats");
        CreateSection("DamageModifiersSection", "Damage Modifiers");
        CreateSection("ResistancesSection", "Resistances");
        CreateSection("DefenseStatsSection", "Defense Stats");
        CreateSection("RecoveryStatsSection", "Recovery Stats");
        CreateSection("CombatMechanicsSection", "Combat Mechanics");
        CreateSection("CardSystemSection", "Card System");
        CreateSection("EquipmentSummarySection", "Equipment Summary");
        
        Debug.Log("All sections created successfully!");
    }
    
    private void PositionAllSections()
    {
        if (scrollRect?.content == null)
        {
            Debug.LogError("ScrollRect content not found!");
            return;
        }
        
        float currentY = 0f;
        Transform content = scrollRect.content;
        
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            RectTransform rect = child.GetComponent<RectTransform>();
            
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.anchoredPosition = new Vector2(0, -currentY);
                rect.sizeDelta = new Vector2(0, headerHeight);
                
                currentY += headerHeight + sectionSpacing;
            }
        }
        
        Debug.Log("All sections positioned!");
    }
    
    private void CenterPanel()
    {
        if (mainContainer != null)
        {
            mainContainer.anchorMin = new Vector2(0.5f, 0.5f);
            mainContainer.anchorMax = new Vector2(0.5f, 0.5f);
            mainContainer.anchoredPosition = Vector2.zero;
            Debug.Log("Panel centered!");
        }
    }
    
    private void StretchPanel()
    {
        if (mainContainer != null)
        {
            mainContainer.anchorMin = Vector2.zero;
            mainContainer.anchorMax = Vector2.one;
            mainContainer.offsetMin = Vector2.zero;
            mainContainer.offsetMax = Vector2.zero;
            Debug.Log("Panel stretched to fill!");
        }
    }
    
    private void AutoAssignComponents()
    {
        if (characterPanel == null)
        {
            Debug.LogError("Please assign a Character Panel first!");
            return;
        }
        
        // Find or add CharacterStatsController
        CharacterStatsController controller = characterPanel.GetComponent<CharacterStatsController>();
        if (controller == null)
        {
            controller = characterPanel.AddComponent<CharacterStatsController>();
        }
        
        // Find or add CharacterPanelLayoutManager
        CharacterPanelLayoutManager layoutManager = characterPanel.GetComponent<CharacterPanelLayoutManager>();
        if (layoutManager == null)
        {
            layoutManager = characterPanel.AddComponent<CharacterPanelLayoutManager>();
        }
        
        // Auto-assign references
        layoutManager.mainContainer = mainContainer;
        layoutManager.scrollRect = scrollRect;
        if (scrollRect != null)
        {
            layoutManager.contentArea = scrollRect.content;
        }
        
        Debug.Log("Components auto-assigned!");
    }
    
    private GameObject CreateChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        child.AddComponent<RectTransform>();
        return child;
    }
}


