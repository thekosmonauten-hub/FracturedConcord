using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Panel that displays aggregated warrant stats grouped by category.
/// Shows statKey and total value for each modifier.
/// </summary>
public class ActiveWarrantStatsPanel : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleLabel;

    [Header("Content References")]
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject categoryHeaderPrefab;
    [SerializeField] private GameObject statRowPrefab;

    [Header("Required References")]
    [SerializeField] private WarrantBoardStateController boardState;
    [SerializeField] private WarrantLockerGrid lockerGrid;
    [SerializeField] private WarrantBoardGraphBuilder graphBuilder;

    private WarrantBoardRuntimeGraph GetRuntimeGraph()
    {
        if (graphBuilder != null)
            return graphBuilder.RuntimeGraph;
        
        // Try to find graph builder if not assigned
        var builder = FindFirstObjectByType<WarrantBoardGraphBuilder>();
        return builder != null ? builder.RuntimeGraph : null;
    }

    private void Awake()
    {
        // Find references if not assigned
        if (boardState == null)
            boardState = FindFirstObjectByType<WarrantBoardStateController>();
        
        if (lockerGrid == null)
            lockerGrid = FindFirstObjectByType<WarrantLockerGrid>();
        
        if (graphBuilder == null)
            graphBuilder = FindFirstObjectByType<WarrantBoardGraphBuilder>();

        // Wire close button
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePanel);
        }

        // Hide panel by default
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        // Note: We subscribe in ShowPanel() instead of OnEnable()
        // because we want to ensure references are found first
    }

    private void OnDisable()
    {
        // Unsubscribe from warrant changes when panel is disabled
        UnsubscribeFromWarrantChanges();
    }

    private void OnDestroy()
    {
        // Clean up subscriptions
        UnsubscribeFromWarrantChanges();
    }

    private void SubscribeToWarrantChanges()
    {
        if (boardState != null)
        {
            boardState.OnWarrantChanged += RefreshStats;
        }
    }

    private void UnsubscribeFromWarrantChanges()
    {
        if (boardState != null)
        {
            boardState.OnWarrantChanged -= RefreshStats;
        }
    }

    /// <summary>
    /// Show the panel and populate with current warrant stats
    /// </summary>
    public void ShowPanel()
    {
        if (panelRoot == null)
        {
            Debug.LogWarning("[ActiveWarrantStatsPanel] Panel root is not assigned!");
            return;
        }

        // Ensure references are found
        if (boardState == null)
            boardState = FindFirstObjectByType<WarrantBoardStateController>();
        
        if (lockerGrid == null)
            lockerGrid = FindFirstObjectByType<WarrantLockerGrid>();
        
        if (graphBuilder == null)
            graphBuilder = FindFirstObjectByType<WarrantBoardGraphBuilder>();

        var runtimeGraph = GetRuntimeGraph();
        
        if (boardState == null || lockerGrid == null || runtimeGraph == null)
        {
            Debug.LogWarning("[ActiveWarrantStatsPanel] Required references are missing!");
            return;
        }

        // Subscribe to warrant changes for real-time updates
        SubscribeToWarrantChanges();

        panelRoot.SetActive(true);
        RefreshStats();
    }

    /// <summary>
    /// Hide the panel
    /// </summary>
    public void HidePanel()
    {
        // Unsubscribe from warrant changes when hiding
        UnsubscribeFromWarrantChanges();
        
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    /// <summary>
    /// Toggle panel visibility
    /// </summary>
    public void TogglePanel()
    {
        if (panelRoot != null && panelRoot.activeSelf)
            HidePanel();
        else
            ShowPanel();
    }

    /// <summary>
    /// Refresh the displayed stats
    /// </summary>
    public void RefreshStats()
    {
        if (contentContainer == null)
        {
            Debug.LogWarning("[ActiveWarrantStatsPanel] Content container is not assigned!");
            return;
        }

        // Clear existing content
        foreach (Transform child in contentContainer)
        {
            if (child != null)
                Destroy(child.gameObject);
        }

        // Get runtime graph
        var runtimeGraph = GetRuntimeGraph();
        if (runtimeGraph == null)
        {
            Debug.LogWarning("[ActiveWarrantStatsPanel] Cannot refresh stats: RuntimeGraph is null!");
            return;
        }

        // Collect stats
        var statsByCategory = WarrantStatsCollector.CollectActiveWarrantStats(boardState, lockerGrid, runtimeGraph);

        if (statsByCategory.Count == 0)
        {
            // Show "No active warrants" message
            CreateStatRow("No active warrants equipped", "", "");
            return;
        }

        // Display stats by category
        string[] categoryOrder = { "Damage", "Defense", "Attributes", "Ailments", "Utility", "Resistances", "Other" };

        foreach (var category in categoryOrder)
        {
            if (statsByCategory.ContainsKey(category) && statsByCategory[category].Count > 0)
            {
                CreateCategoryHeader(category);
                
                foreach (var entry in statsByCategory[category])
                {
                    string valueText = WarrantStatsCollector.FormatStatValue(entry);
                    string statKeyText = entry.statKey;
                    string displayText = !string.IsNullOrWhiteSpace(entry.displayName) 
                        ? entry.displayName 
                        : entry.statKey;
                    
                    // Pass both the formatted value and display text separately
                    CreateStatRow(displayText, statKeyText, valueText);
                }
            }
        }

        // Show any remaining categories not in the ordered list
        foreach (var kvp in statsByCategory)
        {
            if (System.Array.IndexOf(categoryOrder, kvp.Key) < 0)
            {
                CreateCategoryHeader(kvp.Key);
                
                foreach (var entry in kvp.Value)
                {
                    string valueText = WarrantStatsCollector.FormatStatValue(entry);
                    string statKeyText = entry.statKey;
                    string displayText = !string.IsNullOrWhiteSpace(entry.displayName) 
                        ? entry.displayName 
                        : entry.statKey;
                    
                    // Pass both the formatted value and display text separately
                    CreateStatRow(displayText, statKeyText, valueText);
                }
            }
        }
    }

    private void CreateCategoryHeader(string categoryName)
    {
        if (categoryHeaderPrefab != null && contentContainer != null)
        {
            var header = Instantiate(categoryHeaderPrefab, contentContainer);
            
            // Set category text
            var text = header.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = categoryName;
            }
            
            // Set category image color
            var image = header.GetComponentInChildren<Image>();
            if (image != null)
            {
                image.color = GetCategoryColor(categoryName);
            }
        }
        else
        {
            // Fallback: create a simple text object
            CreateStatRow($"--- {categoryName} ---", "", "");
        }
    }

    /// <summary>
    /// Get color for a category, aligned with game's UI color theme
    /// </summary>
    private Color GetCategoryColor(string category)
    {
        if (string.IsNullOrEmpty(category))
            return Color.white;

        switch (category.ToLower())
        {
            case "damage":
                return new Color(0.8f, 0.2f, 0.2f); // Red
            case "defense":
                return new Color(0.5f, 0.5f, 0.8f); // Purple
            case "attributes":
                return new Color(0.8f, 0.6f, 0.2f); // Gold/Orange
            case "ailments":
                return new Color(0.6f, 0.2f, 0.8f); // Violet
            case "utility":
                return new Color(0.2f, 0.5f, 0.8f); // Blue
            case "resistances":
                return new Color(0.3f, 0.7f, 0.9f); // Light Blue
            case "other":
            default:
                return new Color(0.7f, 0.7f, 0.7f); // Gray
        }
    }

    private void CreateStatRow(string displayText, string statKey, string value)
    {
        if (statRowPrefab != null && contentContainer != null)
        {
            var row = Instantiate(statRowPrefab, contentContainer);
            
            // Find all TextMeshProUGUI components in the prefab
            var allTexts = row.GetComponentsInChildren<TextMeshProUGUI>();
            
            // Try to find components by name (case-insensitive)
            TextMeshProUGUI statValueText = null;
            TextMeshProUGUI statText = null;
            TextMeshProUGUI statKeyText = null;
            
            foreach (var txt in allTexts)
            {
                string objName = txt.gameObject.name.ToLower();
                
                // Check for StatValue component
                if (objName.Contains("statvalue") || (objName.Contains("value") && !objName.Contains("text") && !objName.Contains("key")))
                {
                    statValueText = txt;
                }
                // Check for StatText component (but not StatKey)
                else if (objName.Contains("stattext") || (objName.Contains("text") && !objName.Contains("key") && !objName.Contains("value")))
                {
                    statText = txt;
                }
                // Check for StatKey component
                else if (objName.Contains("statkey") || objName.Contains("key"))
                {
                    statKeyText = txt;
                }
            }
            
            // If we have StatValue and StatText, use the split structure (preferred)
            if (statValueText != null && statText != null)
            {
                // StatValue: "16%"
                statValueText.text = !string.IsNullOrEmpty(value) ? value : "";
                
                // StatText: "Increased Elemental Damage"
                statText.text = !string.IsNullOrEmpty(displayText) ? displayText : statKey;
                
                // StatKey: "increasedElementalDamage" (if component exists)
                if (statKeyText != null && !string.IsNullOrEmpty(statKey))
                {
                    statKeyText.text = statKey;
                }
            }
            // Otherwise, use the original structure (StatText + StatKey)
            else if (statText != null && statKeyText != null)
            {
                // StatText: "16% Increased Elemental Damage" (combined)
                string combinedText = !string.IsNullOrEmpty(value) 
                    ? $"{value} {displayText}" 
                    : displayText;
                statText.text = combinedText;
                
                // StatKey: "increasedElementalDamage"
                statKeyText.text = !string.IsNullOrEmpty(statKey) ? statKey : "";
            }
            // Fallback: use first available text component
            else if (allTexts.Length > 0)
            {
                string combinedText = !string.IsNullOrEmpty(value) 
                    ? $"{value} {displayText} ({statKey})" 
                    : $"{displayText} ({statKey})";
                allTexts[0].text = combinedText;
                
                // Use second text for statKey if available
                if (allTexts.Length > 1 && !string.IsNullOrEmpty(statKey))
                {
                    allTexts[1].text = statKey;
                }
            }
        }
        else
        {
            // Fallback: create a simple text object
            var go = new GameObject("StatRow", typeof(RectTransform));
            go.transform.SetParent(contentContainer, false);
            
            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.sizeDelta = new Vector2(0, 30);

            var textComponent = go.AddComponent<TextMeshProUGUI>();
            string combinedText = !string.IsNullOrEmpty(value) 
                ? $"{value} {displayText} ({statKey})" 
                : $"{displayText} ({statKey})";
            textComponent.text = combinedText;
            textComponent.fontSize = 14;
            textComponent.color = Color.white;
        }
    }
}

