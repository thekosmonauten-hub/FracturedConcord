using UnityEngine;
using TMPro;
using PassiveTree;

/// <summary>
/// Static tooltip panel that updates content based on hovered passive
/// </summary>
public class StaticTooltipPanel : MonoBehaviour
{
    [Header("Tooltip Panel References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Settings")]
    [SerializeField] private bool showOnStart = false;
    [SerializeField] private string defaultNameText = "Select a Passive";
    [SerializeField] private string defaultDescriptionText = "Hover over a passive node to see its details";
    
    [Header("References")]
    [SerializeField] private JsonBoardDataManager dataManager;
    
    // Runtime
    private CellController currentHoveredCell;
    
    void Start()
    {
        // Find required components
        if (dataManager == null)
        {
            dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        }
        
        // Set default content
        SetDefaultContent();
        
        // Show or hide panel based on settings
        gameObject.SetActive(showOnStart);
    }
    
    /// <summary>
    /// Update tooltip content for a hovered cell
    /// </summary>
    public void UpdateTooltipContent(CellController cell)
    {
        PassiveTreeLogger.LogCategory($"UpdateTooltipContent called for cell at {cell?.GridPosition}", "tooltip");
        
        if (cell == null) 
        {
            PassiveTreeLogger.LogWarning("Cell is null!");
            SetDefaultContent();
            return;
        }

        currentHoveredCell = cell;
        
        // Get node data from CellJsonData component
        var cellJsonData = cell.GetComponent<CellJsonData>();
        
        if (cellJsonData != null && cellJsonData.HasJsonData())
        {
            PassiveTreeLogger.LogCategory($"Found node data: {cellJsonData.NodeName} ({cellJsonData.NodeType})", "tooltip");
            UpdateContentWithCellData(cellJsonData);
        }
        else
        {
            PassiveTreeLogger.LogCategory($"Using basic cell data for {cell.GetNodeName()}", "tooltip");
            UpdateContentWithCellBasicData(cell);
        }
        
        // Show the panel
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Hide tooltip panel
    /// </summary>
    public void HideTooltip()
    {
        PassiveTreeLogger.LogCategory("Hiding tooltip panel", "tooltip");
        gameObject.SetActive(false);
        currentHoveredCell = null;
    }
    
    /// <summary>
    /// Show tooltip panel with default content
    /// </summary>
    public void ShowTooltip()
    {
        PassiveTreeLogger.LogCategory("Showing tooltip panel with default content", "tooltip");
        SetDefaultContent();
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Update content using CellJsonData
    /// </summary>
    private void UpdateContentWithCellData(CellJsonData cellJsonData)
    {
        if (nameText != null)
        {
            nameText.text = cellJsonData.NodeName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = cellJsonData.GetFormattedDescription();
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
        return gameObject.activeInHierarchy;
    }
    
    /// <summary>
    /// Set default text content
    /// </summary>
    public void SetDefaultTexts(string name, string description)
    {
        defaultNameText = name;
        defaultDescriptionText = description;
        SetDefaultContent();
    }
}




