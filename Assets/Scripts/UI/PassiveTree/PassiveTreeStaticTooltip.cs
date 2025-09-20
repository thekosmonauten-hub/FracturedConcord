using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PassiveTree;

/// <summary>
/// Static tooltip system for passive tree that displays in bottom-left corner
/// Integrates with CellController hover events
/// </summary>
public class PassiveTreeStaticTooltip : MonoBehaviour
{
    [Header("Tooltip UI References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;
    
    [Header("Positioning")]
    [SerializeField] private Vector2 bottomLeftOffset = new Vector2(20, 20);
    [SerializeField] private bool stayInBottomLeft = true;
    
    [Header("Dynamic Size Configuration")]
    [SerializeField] private bool useDynamicHeight = true;
    [SerializeField] private float fixedWidth = 400f; // Fixed width for consistent appearance
    [SerializeField] private float minHeight = 150f; // Minimum height to ensure readability
    [SerializeField] private float maxHeight = 600f; // Increased maximum height for larger content
    [SerializeField] private bool enableTextWrapping = true;
    [SerializeField] private int maxDescriptionLines = 6; // Increased for dynamic height
    [SerializeField] private int maxStatsLines = 8; // Increased for dynamic height
    
    [Header("Background Configuration")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private bool useBackgroundSprite = false;
    
    [Header("Default Content")]
    [SerializeField] private string defaultNameText = "Select a Passive";
    [SerializeField] private string defaultDescriptionText = "Hover over a passive node to see its details";
    [SerializeField] private string defaultStatsText = "";
    
    [Header("Settings")]
    [SerializeField] private bool showOnStart = false;
    [SerializeField] private bool enableDebugLogging = true;
    
    // Runtime
    private CellController currentHoveredCell;
    private RectTransform tooltipRectTransform;
    private Canvas parentCanvas;
    
    void Start()
    {
        InitializeTooltip();
        EnsureTooltipEnabled();
    }
    
    void OnEnable()
    {
        // Ensure tooltip is properly enabled when the GameObject becomes active
        EnsureTooltipEnabled();
    }
    
    /// <summary>
    /// Initialize the tooltip system
    /// </summary>
    private void InitializeTooltip()
    {
        // Find tooltip panel if not assigned
        if (tooltipPanel == null)
        {
            tooltipPanel = gameObject;
        }
        
        // Get RectTransform
        tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();
        if (tooltipRectTransform == null)
        {
            Debug.LogError("[PassiveTreeStaticTooltip] Tooltip panel needs a RectTransform component!");
            return;
        }
        
        // Find parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("[PassiveTreeStaticTooltip] Tooltip must be a child of a Canvas!");
            return;
        }
        
        // Find text components if not assigned
        if (nameText == null || descriptionText == null)
        {
            FindTextComponents();
        }
        
        // Set initial position to bottom-left
        SetBottomLeftPosition();
        
        // Apply static size configuration
        ApplyDynamicSizeConfiguration();
        
        // Set default content
        SetDefaultContent();
        
        // Show or hide based on settings
        tooltipPanel.SetActive(showOnStart);
        
        if (enableDebugLogging)
        {
            Debug.Log("[PassiveTreeStaticTooltip] Static tooltip system initialized");
        }
    }
    
    /// <summary>
    /// Ensure the tooltip GameObject is enabled and ready for use
    /// </summary>
    private void EnsureTooltipEnabled()
    {
        // Ensure the tooltip GameObject itself is enabled
        if (!gameObject.activeInHierarchy)
        {
            if (enableDebugLogging)
            {
                // Debug.Log("[PassiveTreeStaticTooltip] Enabling tooltip GameObject");
            }
            gameObject.SetActive(true);
        }
        
        // Ensure the tooltip panel is properly set up
        if (tooltipPanel == null)
        {
            tooltipPanel = gameObject;
        }
        
        if (enableDebugLogging)
        {
            // Debug.Log("[PassiveTreeStaticTooltip] Tooltip GameObject is enabled and ready");
        }
    }
    
    /// <summary>
    /// Apply static size configuration to prevent tooltip flickering
    /// </summary>
    private void ApplyDynamicSizeConfiguration()
    {
        if (tooltipRectTransform == null) return;
        
        if (useDynamicHeight)
        {
            // Set fixed width and dynamic height
            tooltipRectTransform.sizeDelta = new Vector2(fixedWidth, minHeight);
            
            // Configure text components for dynamic sizing
            ConfigureTextComponentsForDynamicSize();
        }
        else
        {
            // Fallback to static sizing for backward compatibility
            tooltipRectTransform.sizeDelta = new Vector2(fixedWidth, minHeight);
            ConfigureTextComponentsForStaticSize();
        }
        
        // Configure background
        ConfigureBackground();
        
        // Configure VerticalLayoutGroup for proper child control
        ConfigureVerticalLayoutGroup();
        
        if (enableDebugLogging)
        {
            // Debug.Log($"[PassiveTreeStaticTooltip] Applied dynamic size configuration - Width: {fixedWidth}, Min Height: {minHeight}");
        }
    }
    
    /// <summary>
    /// Configure VerticalLayoutGroup for proper child control
    /// </summary>
    private void ConfigureVerticalLayoutGroup()
    {
        VerticalLayoutGroup layoutGroup = GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            // Enable child size control for both width and height
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            
            // Enable child force expand for both width and height
            layoutGroup.childForceExpandHeight = true;
            layoutGroup.childForceExpandWidth = true;
            
            if (enableDebugLogging)
            {
                // Debug.Log("[PassiveTreeStaticTooltip] Configured VerticalLayoutGroup - Child Control: Width=True, Height=True, Force Expand: Width=True, Height=True");
            }
        }
        else
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("[PassiveTreeStaticTooltip] VerticalLayoutGroup component not found!");
            }
        }
    }
    
    /// <summary>
    /// Calculate and apply dynamic height based on content
    /// </summary>
    private void CalculateAndApplyDynamicHeight()
    {
        if (!useDynamicHeight || tooltipRectTransform == null) return;
        
        // Force text to update their preferred height
        if (nameText != null) nameText.SetAllDirty();
        if (descriptionText != null) descriptionText.SetAllDirty();
        if (statsText != null) statsText.SetAllDirty();
        
        // Calculate required height based on content using GetPreferredValues
        float requiredHeight = 0;
        
        // Add padding (8px top + 8px bottom = 16px)
        requiredHeight += 16;
        
        // Add height for name section
        if (nameText != null && !string.IsNullOrEmpty(nameText.text))
        {
            Vector2 nameSize = nameText.GetPreferredValues(nameText.text, fixedWidth - 16, 0); // Account for padding
            requiredHeight += nameSize.y + 2; // Add small spacing
        }
        
        // Add height for description section
        if (descriptionText != null && !string.IsNullOrEmpty(descriptionText.text))
        {
            Vector2 descSize = descriptionText.GetPreferredValues(descriptionText.text, fixedWidth - 16, 0); // Account for padding
            requiredHeight += descSize.y + 2; // Add small spacing
        }
        
        // Add height for stats section
        if (statsText != null && !string.IsNullOrEmpty(statsText.text))
        {
            Vector2 statsSize = statsText.GetPreferredValues(statsText.text, fixedWidth - 16, 0); // Account for padding
            requiredHeight += statsSize.y + 2; // Add small spacing
        }
        
        // Ensure minimum height
        requiredHeight = Mathf.Max(requiredHeight, minHeight);
        
        // Clamp height between min and max
        float finalHeight = Mathf.Clamp(requiredHeight, minHeight, maxHeight);
        
        // Apply the calculated height
        tooltipRectTransform.sizeDelta = new Vector2(fixedWidth, finalHeight);
        
        if (enableDebugLogging)
        {
            // Debug.Log($"[PassiveTreeStaticTooltip] Calculated dynamic height: {finalHeight} (required: {requiredHeight}, clamped between {minHeight}-{maxHeight})");
            if (nameText != null && !string.IsNullOrEmpty(nameText.text))
            {
                Vector2 nameSize = nameText.GetPreferredValues(nameText.text, fixedWidth - 16, 0);
                // Debug.Log($"[PassiveTreeStaticTooltip] Name size: {nameSize}");
            }
            if (descriptionText != null && !string.IsNullOrEmpty(descriptionText.text))
            {
                Vector2 descSize = descriptionText.GetPreferredValues(descriptionText.text, fixedWidth - 16, 0);
                // Debug.Log($"[PassiveTreeStaticTooltip] Description size: {descSize}");
            }
            if (statsText != null && !string.IsNullOrEmpty(statsText.text))
            {
                Vector2 statsSize = statsText.GetPreferredValues(statsText.text, fixedWidth - 16, 0);
                // Debug.Log($"[PassiveTreeStaticTooltip] Stats size: {statsSize}");
            }
        }
    }
    
    /// <summary>
    /// Coroutine to calculate height after text has been updated
    /// </summary>
    private System.Collections.IEnumerator CalculateHeightAfterFrame()
    {
        // Wait for text to update properly
        yield return null;
        yield return null; // Wait an additional frame
        yield return null; // Wait one more frame for complete text update
        
        // Calculate and apply dynamic height
        CalculateAndApplyDynamicHeight();
        
        // Recalculate after a short delay to ensure accuracy
        yield return new WaitForSeconds(0.1f);
        CalculateAndApplyDynamicHeight();
    }
    
    /// <summary>
    /// Force height recalculation after tooltip is shown
    /// </summary>
    private System.Collections.IEnumerator ForceHeightRecalculation()
    {
        // Wait for tooltip to be fully visible
        yield return new WaitForSeconds(0.05f);
        
        // Force text components to recalculate
        if (nameText != null) nameText.SetAllDirty();
        if (descriptionText != null) descriptionText.SetAllDirty();
        if (statsText != null) statsText.SetAllDirty();
        
        // Wait one frame for dirty flag to process
        yield return null;
        
        // Recalculate height
        CalculateAndApplyDynamicHeight();
        
    }
    
    /// <summary>
    /// Configure text components for dynamic sizing
    /// </summary>
    private void ConfigureTextComponentsForDynamicSize()
    {
        // Configure name text
        if (nameText != null)
        {
            nameText.enableWordWrapping = enableTextWrapping;
            nameText.overflowMode = TextOverflowModes.Overflow;
        }
        
        // Configure description text for dynamic sizing
        if (descriptionText != null)
        {
            descriptionText.enableWordWrapping = enableTextWrapping;
            descriptionText.overflowMode = TextOverflowModes.Overflow;
            // Set a reasonable height for description
            descriptionText.rectTransform.sizeDelta = new Vector2(descriptionText.rectTransform.sizeDelta.x, 50);
        }
        
        // Configure stats text for dynamic sizing
        if (statsText != null)
        {
            statsText.enableWordWrapping = enableTextWrapping;
            statsText.overflowMode = TextOverflowModes.Overflow;
            // Set a reasonable height for stats
            statsText.rectTransform.sizeDelta = new Vector2(statsText.rectTransform.sizeDelta.x, 60);
        }
    }
    
    /// <summary>
    /// Configure text components for static sizing (backward compatibility)
    /// </summary>
    private void ConfigureTextComponentsForStaticSize()
    {
        // Configure name text
        if (nameText != null)
        {
            nameText.enableWordWrapping = enableTextWrapping;
            nameText.overflowMode = TextOverflowModes.Overflow;
        }
        
        // Configure description text
        if (descriptionText != null)
        {
            descriptionText.enableWordWrapping = enableTextWrapping;
            descriptionText.overflowMode = TextOverflowModes.Overflow;
            // Set preferred height based on max lines (adjusted for proper fit)
            if (maxDescriptionLines > 0)
            {
                descriptionText.rectTransform.sizeDelta = new Vector2(descriptionText.rectTransform.sizeDelta.x, maxDescriptionLines * 18); // Reduced from 20 to 18
            }
        }
        
        // Configure stats text
        if (statsText != null)
        {
            statsText.enableWordWrapping = enableTextWrapping;
            statsText.overflowMode = TextOverflowModes.Overflow;
            // Set preferred height based on max lines (adjusted for proper fit)
            if (maxStatsLines > 0)
            {
                statsText.rectTransform.sizeDelta = new Vector2(statsText.rectTransform.sizeDelta.x, maxStatsLines * 18); // Reduced from 20 to 18
            }
        }
    }
    
    /// <summary>
    /// Configure the tooltip background
    /// </summary>
    private void ConfigureBackground()
    {
        if (tooltipPanel == null) return;
        
        // Get or add Image component for background
        Image backgroundImage = tooltipPanel.GetComponent<Image>();
        if (backgroundImage == null)
        {
            backgroundImage = tooltipPanel.AddComponent<Image>();
        }
        
        // Configure background based on settings
        if (useBackgroundSprite && backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = Color.white; // Use sprite's original colors
            backgroundImage.type = Image.Type.Sliced; // Use sliced for better scaling
            if (enableDebugLogging)
            {
                // Debug.Log($"[PassiveTreeStaticTooltip] Applied background sprite: {backgroundSprite.name}");
            }
        }
        else
        {
            backgroundImage.sprite = null;
            backgroundImage.color = backgroundColor;
            if (enableDebugLogging)
            {
                Debug.Log($"[PassiveTreeStaticTooltip] Applied background color: {backgroundColor}");
            }
        }
    }
    
    /// <summary>
    /// Find text components automatically
    /// </summary>
    private void FindTextComponents()
    {
        TextMeshProUGUI[] allTexts = tooltipPanel.GetComponentsInChildren<TextMeshProUGUI>();
        
        foreach (var text in allTexts)
        {
            string textName = text.name.ToLower();
            
            if (textName.Contains("name") || textName.Contains("title"))
            {
                nameText = text;
                if (enableDebugLogging)
                {
                    Debug.Log($"[PassiveTreeStaticTooltip] Found name text: {text.name}");
                }
            }
            else if (textName.Contains("description") || textName.Contains("desc"))
            {
                descriptionText = text;
                if (enableDebugLogging)
                {
                    Debug.Log($"[PassiveTreeStaticTooltip] Found description text: {text.name}");
                }
            }
            else if (textName.Contains("stats") || textName.Contains("stat"))
            {
                statsText = text;
                if (enableDebugLogging)
                {
                    Debug.Log($"[PassiveTreeStaticTooltip] Found stats text: {text.name}");
                }
            }
        }
        
        if (nameText == null || descriptionText == null)
        {
            Debug.LogWarning("[PassiveTreeStaticTooltip] Could not find all required text components!");
        }
    }
    
    /// <summary>
    /// Update tooltip content for a hovered cell
    /// </summary>
    public void UpdateTooltipContent(CellController cell)
    {
        if (enableDebugLogging)
        {
            // Debug.Log($"[PassiveTreeStaticTooltip] UpdateTooltipContent called for cell at {cell?.GridPosition}");
        }
        
        if (cell == null)
        {
            SetDefaultContent();
            return;
        }
        
        currentHoveredCell = cell;
        
        // Ensure the tooltip GameObject is enabled first
        if (!gameObject.activeInHierarchy)
        {
            if (enableDebugLogging)
            {
                // Debug.Log("[PassiveTreeStaticTooltip] Enabling tooltip GameObject");
            }
            gameObject.SetActive(true);
        }
        
        // Get node data from CellJsonData component
        var cellJsonData = cell.GetComponent<CellJsonData>();
        
        if (cellJsonData != null && cellJsonData.HasJsonData())
        {
            if (enableDebugLogging)
            {
                // Debug.Log($"[PassiveTreeStaticTooltip] Found JSON data: {cellJsonData.NodeName} ({cellJsonData.NodeType})");
            }
            UpdateContentWithCellData(cellJsonData, cell);
        }
        else
        {
            if (enableDebugLogging)
            {
                // Debug.Log($"[PassiveTreeStaticTooltip] Using basic cell data for {cell.GetNodeName()}");
            }
            UpdateContentWithCellBasicData(cell);
        }
        
        // Apply dynamic size configuration
        ApplyDynamicSizeConfiguration();
        
        // Calculate and apply dynamic height based on content
        if (useDynamicHeight)
        {
            // Wait one frame for text to update, then calculate height
            StartCoroutine(CalculateHeightAfterFrame());
        }
        
        // Show the tooltip panel
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            if (enableDebugLogging)
            {
                // Debug.Log("[PassiveTreeStaticTooltip] Tooltip panel activated");
            }
        }
        
        // Force height recalculation after showing
        if (useDynamicHeight)
        {
            StartCoroutine(ForceHeightRecalculation());
        }
        else
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("[PassiveTreeStaticTooltip] Tooltip panel is null!");
            }
        }
    }
    
    /// <summary>
    /// Hide tooltip
    /// </summary>
    public void HideTooltip()
    {
        if (enableDebugLogging)
        {
            // Debug.Log("[PassiveTreeStaticTooltip] Hiding tooltip");
        }
        
        // Only hide the tooltip panel, don't disable the GameObject
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        
        currentHoveredCell = null;
        
        // Note: We don't disable the GameObject itself, just the panel
        // This allows the tooltip system to remain active for future use
    }
    
    /// <summary>
    /// Clear tooltip content but keep it visible (for debugging)
    /// </summary>
    public void ClearTooltipContent()
    {
        if (enableDebugLogging)
        {
            Debug.Log("[PassiveTreeStaticTooltip] Clearing tooltip content");
        }
        
        SetDefaultContent();
        currentHoveredCell = null;
        
        // Keep the tooltip visible but with default content
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Show tooltip with default content
    /// </summary>
    public void ShowTooltip()
    {
        if (enableDebugLogging)
        {
            Debug.Log("[PassiveTreeStaticTooltip] Showing tooltip with default content");
        }
        
        // Ensure the tooltip GameObject is enabled
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        
        SetDefaultContent();
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Update content using CellJsonData
    /// </summary>
    private void UpdateContentWithCellData(CellJsonData cellJsonData, CellController cell)
    {
        if (nameText != null)
        {
            nameText.text = cellJsonData.NodeName;
        }
        
        if (descriptionText != null)
        {
            // Use raw description without stats to avoid duplication
            descriptionText.text = cellJsonData.NodeDescription;
        }
        
        if (statsText != null)
        {
            statsText.text = FormatStatsFromJsonData(cellJsonData, cell);
        }
    }
    
    /// <summary>
    /// Update content using cell's basic data
    /// </summary>
    private void UpdateContentWithCellBasicData(CellController cell)
    {
        if (nameText != null)
        {
            nameText.text = cell.GetNodeName();
        }
        
        if (descriptionText != null)
        {
            string description = cell.NodeDescription;
            
            // Add basic info if no description
            if (string.IsNullOrEmpty(description))
            {
                description = $"Node Type: {cell.GetNodeType()}\nPosition: {cell.GetGridPosition()}";
            }
            
            descriptionText.text = description;
        }
        
        if (statsText != null)
        {
            statsText.text = FormatBasicStats(cell);
        }
    }
    
    /// <summary>
    /// Format stats from JSON data
    /// </summary>
    private string FormatStatsFromJsonData(CellJsonData cellJsonData, CellController cell)
    {
        string statsText = "";
        
        if (cellJsonData.NodeStats != null)
        {
            var stats = cellJsonData.NodeStats;
            var statsList = new System.Collections.Generic.List<string>();
            
            if (stats.strength != 0) statsList.Add($"+{stats.strength} Strength");
            if (stats.dexterity != 0) statsList.Add($"+{stats.dexterity} Dexterity");
            if (stats.intelligence != 0) statsList.Add($"+{stats.intelligence} Intelligence");
            if (stats.maxHealthIncrease != 0) statsList.Add($"+{stats.maxHealthIncrease} Max Health");
            if (stats.maxEnergyShieldIncrease != 0) statsList.Add($"+{stats.maxEnergyShieldIncrease} Max Energy Shield");
            if (stats.armorIncrease != 0) statsList.Add($"+{stats.armorIncrease} Armor");
            if (stats.increasedEvasion != 0) statsList.Add($"+{stats.increasedEvasion} Evasion");
            if (stats.accuracy != 0) statsList.Add($"+{stats.accuracy} Accuracy");
            if (stats.spellPowerIncrease != 0) statsList.Add($"+{stats.spellPowerIncrease} Spell Power");
            if (stats.increasedProjectileDamage != 0) statsList.Add($"+{stats.increasedProjectileDamage} Projectile Damage");
            if (stats.elementalResist != 0) statsList.Add($"+{stats.elementalResist} Elemental Resistance");
            
            if (statsList.Count > 0)
            {
                statsText = "Stats:\n" + string.Join("\n", statsList);
            }
        }
        
        // Cost removed - all passives cost one skill point
        
        return statsText;
    }
    
    /// <summary>
    /// Format basic stats for cells without JSON data
    /// </summary>
    private string FormatBasicStats(CellController cell)
    {
        string statsText = "";
        
        // Add node type
        statsText += $"Type: {cell.GetNodeType()}";
        
        // Add extension point info if applicable
        if (cell.IsExtensionPoint)
        {
            statsText += "\n[EXT] Extension Point";
        }
        
        return statsText;
    }
    
    /// <summary>
    /// Set default content
    /// </summary>
    private void SetDefaultContent()
    {
        if (nameText != null)
        {
            nameText.text = defaultNameText;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = defaultDescriptionText;
        }
        
        if (statsText != null)
        {
            statsText.text = defaultStatsText;
        }
    }
    
    /// <summary>
    /// Set tooltip position to bottom-left corner
    /// </summary>
    private void SetBottomLeftPosition()
    {
        if (tooltipRectTransform == null || parentCanvas == null) return;
        
        // Set anchor to bottom-left
        tooltipRectTransform.anchorMin = new Vector2(0, 0);
        tooltipRectTransform.anchorMax = new Vector2(0, 0);
        tooltipRectTransform.pivot = new Vector2(0, 0);
        
        // Set position with offset
        tooltipRectTransform.anchoredPosition = bottomLeftOffset;
    }
    
    /// <summary>
    /// Get the currently hovered cell
    /// </summary>
    public CellController GetCurrentHoveredCell()
    {
        return currentHoveredCell;
    }
    
    /// <summary>
    /// Check if tooltip is currently showing
    /// </summary>
    public bool IsTooltipShowing()
    {
        return tooltipPanel.activeInHierarchy;
    }
    
    /// <summary>
    /// Set default text content
    /// </summary>
    public void SetDefaultTexts(string name, string description, string stats = "")
    {
        defaultNameText = name;
        defaultDescriptionText = description;
        defaultStatsText = stats;
        SetDefaultContent();
    }
    
    /// <summary>
    /// Update tooltip position (called by external systems if needed)
    /// </summary>
    public void UpdatePosition()
    {
        if (stayInBottomLeft)
        {
            SetBottomLeftPosition();
        }
    }
    
    #region Context Menu Methods
    
    /// <summary>
    /// Test tooltip with sample data
    /// </summary>
    [ContextMenu("Test Tooltip")]
    public void TestTooltip()
    {
        if (nameText != null)
        {
            nameText.text = "Test Node";
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = "This is a test tooltip to verify the system is working correctly.";
        }
        
        if (statsText != null)
        {
            statsText.text = "Stats:\n+10 Strength\n+5 Dexterity";
        }
        
        tooltipPanel.SetActive(true);
        
        Debug.Log("[PassiveTreeStaticTooltip] Test tooltip displayed");
    }
    
    /// <summary>
    /// Reset tooltip to default content
    /// </summary>
    [ContextMenu("Reset to Default")]
    public void ResetToDefault()
    {
        SetDefaultContent();
        Debug.Log("[PassiveTreeStaticTooltip] Reset to default content");
    }
    
    /// <summary>
    /// Manually ensure tooltip is enabled
    /// </summary>
    [ContextMenu("Ensure Tooltip Enabled")]
    public void ManualEnsureTooltipEnabled()
    {
        EnsureTooltipEnabled();
        Debug.Log("[PassiveTreeStaticTooltip] Manually ensured tooltip is enabled");
    }
    
    /// <summary>
    /// Test tooltip with a random cell (for debugging)
    /// </summary>
    [ContextMenu("Test Tooltip with Random Cell")]
    public void TestTooltipWithRandomCell()
    {
        // Find a random CellController
        var allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        if (allCells.Length > 0)
        {
            var randomCell = allCells[Random.Range(0, allCells.Length)];
            UpdateTooltipContent(randomCell);
            Debug.Log($"[PassiveTreeStaticTooltip] Testing tooltip with random cell at {randomCell.GridPosition}");
        }
        else
        {
            Debug.LogWarning("[PassiveTreeStaticTooltip] No CellController found for testing");
        }
    }
    
    /// <summary>
    /// Apply dynamic size configuration manually
    /// </summary>
    [ContextMenu("Apply Dynamic Size Configuration")]
    public void ManualApplyDynamicSizeConfiguration()
    {
        ApplyDynamicSizeConfiguration();
        Debug.Log($"[PassiveTreeStaticTooltip] Manually applied dynamic size configuration - Width: {fixedWidth}, Min Height: {minHeight}");
    }
    
    /// <summary>
    /// Toggle dynamic height on/off
    /// </summary>
    [ContextMenu("Toggle Dynamic Height")]
    public void ToggleDynamicHeight()
    {
        useDynamicHeight = !useDynamicHeight;
        ApplyDynamicSizeConfiguration();
        Debug.Log($"[PassiveTreeStaticTooltip] Dynamic height toggled: {useDynamicHeight}");
    }
    
    /// <summary>
    /// Apply background configuration manually
    /// </summary>
    [ContextMenu("Apply Background Configuration")]
    public void ManualApplyBackgroundConfiguration()
    {
        ConfigureBackground();
        Debug.Log($"[PassiveTreeStaticTooltip] Manually applied background configuration");
    }
    
    /// <summary>
    /// Configure VerticalLayoutGroup manually
    /// </summary>
    [ContextMenu("Configure VerticalLayoutGroup")]
    public void ManualConfigureVerticalLayoutGroup()
    {
        ConfigureVerticalLayoutGroup();
        Debug.Log("[PassiveTreeStaticTooltip] Manually configured VerticalLayoutGroup");
    }
    
    /// <summary>
    /// Toggle background sprite on/off
    /// </summary>
    [ContextMenu("Toggle Background Sprite")]
    public void ToggleBackgroundSprite()
    {
        useBackgroundSprite = !useBackgroundSprite;
        ConfigureBackground();
        Debug.Log($"[PassiveTreeStaticTooltip] Background sprite toggled: {useBackgroundSprite}");
    }
    
    #endregion
}
