using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PassiveTree;

/// <summary>
/// Setup script for passive tree tooltip system
/// Creates tooltip prefabs and connects them to the JSON data system
/// </summary>
public class PassiveTreeTooltipSetup : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createTooltipPrefab = true;
    
    [Header("Tooltip Prefab Settings")]
    [SerializeField] private Vector2 tooltipSize = new Vector2(300, 200);
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int titleFontSize = 16;
    [SerializeField] private int descriptionFontSize = 14;
    
    [Header("References")]
    [SerializeField] private JsonPassiveTreeTooltip jsonTooltip;
    [SerializeField] private JsonBoardDataManager dataManager;
    [SerializeField] private GameObject tooltipPrefab;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupTooltipSystem();
        }
    }

    /// <summary>
    /// Complete setup for the tooltip system
    /// </summary>
    [ContextMenu("Setup Tooltip System")]
    public void SetupTooltipSystem()
    {
        Debug.Log("[PassiveTreeTooltipSetup] Setting up tooltip system...");

        // 1. Find or create data manager
        SetupDataManager();

        // 2. Find or create JSON tooltip manager
        SetupJsonTooltipManager();

        // 3. Create tooltip prefab if needed
        if (createTooltipPrefab)
        {
            CreateTooltipPrefab();
        }

        // 4. Connect components
        ConnectComponents();

        Debug.Log("[PassiveTreeTooltipSetup] Tooltip system setup complete!");
    }

    /// <summary>
    /// Setup the JSON data manager
    /// </summary>
    private void SetupDataManager()
    {
        if (dataManager == null)
        {
            dataManager = FindFirstObjectByType<JsonBoardDataManager>();
            if (dataManager == null)
            {
                GameObject managerGO = new GameObject("JsonBoardDataManager");
                dataManager = managerGO.AddComponent<JsonBoardDataManager>();
                Debug.Log("[PassiveTreeTooltipSetup] Created JsonBoardDataManager");
            }
        }

        // Load the CoreBoardData.json if not already loaded
        if (dataManager.GetBoardData() == null)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("PassiveTree/CoreBoardData");
            if (jsonFile != null)
            {
                dataManager.SetJsonDataSource(jsonFile);
                dataManager.LoadBoardData();
                Debug.Log("[PassiveTreeTooltipSetup] Loaded CoreBoardData.json");
            }
            else
            {
                Debug.LogWarning("[PassiveTreeTooltipSetup] CoreBoardData.json not found in Resources/PassiveTree/");
            }
        }
    }

    /// <summary>
    /// Setup the JSON tooltip manager
    /// </summary>
    private void SetupJsonTooltipManager()
    {
        if (jsonTooltip == null)
        {
            jsonTooltip = FindFirstObjectByType<JsonPassiveTreeTooltip>();
            if (jsonTooltip == null)
            {
                GameObject tooltipGO = new GameObject("JsonPassiveTreeTooltip");
                jsonTooltip = tooltipGO.AddComponent<JsonPassiveTreeTooltip>();
                Debug.Log("[PassiveTreeTooltipSetup] Created JsonPassiveTreeTooltip");
            }
        }
    }

    /// <summary>
    /// Create a tooltip prefab
    /// </summary>
    private void CreateTooltipPrefab()
    {
        if (tooltipPrefab != null)
        {
            Debug.Log("[PassiveTreeTooltipSetup] Tooltip prefab already exists");
            return;
        }

        // Create tooltip prefab
        tooltipPrefab = new GameObject("PassiveTreeTooltipPrefab");
        
        // Add RectTransform
        RectTransform rectTransform = tooltipPrefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = tooltipSize;
        
        // Add Image component for background
        Image backgroundImage = tooltipPrefab.AddComponent<Image>();
        backgroundImage.color = backgroundColor;
        
        // Add ContentSizeFitter
        ContentSizeFitter sizeFitter = tooltipPrefab.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Add VerticalLayoutGroup
        VerticalLayoutGroup layoutGroup = tooltipPrefab.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.spacing = 5;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;

        // Create title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(tooltipPrefab.transform);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Node Name";
        titleText.fontSize = titleFontSize;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = textColor;
        titleText.alignment = TextAlignmentOptions.Center;
        
        // Add LayoutElement to title
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = titleFontSize + 10;
        
        // Create description text
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(tooltipPrefab.transform);
        
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = "Node description and stats will appear here";
        descText.fontSize = descriptionFontSize;
        descText.color = textColor;
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.textWrappingMode = TextWrappingModes.Normal;
        
        // Add LayoutElement to description
        LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
        descLayout.flexibleHeight = 1;
        descLayout.minHeight = 50;

        // Initially hide the tooltip
        tooltipPrefab.SetActive(false);

        Debug.Log("[PassiveTreeTooltipSetup] Created tooltip prefab");
    }

    /// <summary>
    /// Connect all components
    /// </summary>
    private void ConnectComponents()
    {
        // Connect tooltip to data manager
        if (jsonTooltip != null && dataManager != null)
        {
            jsonTooltip.SetDataManager(dataManager);
            Debug.Log("[PassiveTreeTooltipSetup] Connected tooltip to data manager");
        }

        // Set tooltip prefab
        if (jsonTooltip != null && tooltipPrefab != null)
        {
            // Use reflection to set the private tooltipPrefab field
            var field = typeof(JsonPassiveTreeTooltip).GetField("tooltipPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(jsonTooltip, tooltipPrefab);
                Debug.Log("[PassiveTreeTooltipSetup] Set tooltip prefab on JsonPassiveTreeTooltip");
            }
        }
    }

    /// <summary>
    /// Test the tooltip system
    /// </summary>
    [ContextMenu("Test Tooltip System")]
    public void TestTooltipSystem()
    {
        if (jsonTooltip == null)
        {
            Debug.LogError("[PassiveTreeTooltipSetup] No tooltip manager found!");
            return;
        }

        // Find a cell to test with
        CellController[] cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        if (cells.Length > 0)
        {
            Debug.Log($"[PassiveTreeTooltipSetup] Testing tooltip with cell at {cells[0].GridPosition}");
            jsonTooltip.ShowTooltip(cells[0]);
        }
        else
        {
            Debug.LogWarning("[PassiveTreeTooltipSetup] No cells found to test with!");
        }
    }

    /// <summary>
    /// Get the tooltip prefab
    /// </summary>
    public GameObject GetTooltipPrefab()
    {
        return tooltipPrefab;
    }

    /// <summary>
    /// Get the JSON tooltip manager
    /// </summary>
    public JsonPassiveTreeTooltip GetJsonTooltipManager()
    {
        return jsonTooltip;
    }

    /// <summary>
    /// Get the data manager
    /// </summary>
    public JsonBoardDataManager GetDataManager()
    {
        return dataManager;
    }
}

