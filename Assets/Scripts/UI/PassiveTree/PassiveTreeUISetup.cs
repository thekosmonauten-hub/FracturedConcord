using UnityEngine;
using TMPro;
using PassiveTree;

/// <summary>
/// Simple setup script to automatically connect TextMeshPro components to the passive tree UI system
/// Add this to any GameObject with a TextMeshPro component to display cell information
/// </summary>
public class PassiveTreeUISetup : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool debugMode = false;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI mainInfoText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI statusText;

    private PassiveTreeUI uiManager;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupUI();
        }
    }

    /// <summary>
    /// Set up the UI system
    /// </summary>
    [ContextMenu("Setup UI")]
    public void SetupUI()
    {
        Debug.Log("[PassiveTreeUISetup] Setting up passive tree UI...");

        // Find or create UI manager
        uiManager = FindFirstObjectByType<PassiveTreeUI>();
        if (uiManager == null)
        {
            GameObject uiGO = new GameObject("PassiveTreeUI");
            uiManager = uiGO.AddComponent<PassiveTreeUI>();
            Debug.Log("[PassiveTreeUISetup] Created PassiveTreeUI component");
        }

        // Auto-detect TextMeshPro components if not assigned
        if (mainInfoText == null)
        {
            mainInfoText = GetComponent<TextMeshProUGUI>();
        }

        if (mainInfoText == null)
        {
            // Try to find TextMeshPro components in children
            mainInfoText = GetComponentInChildren<TextMeshProUGUI>();
        }

        // Set up the UI manager with our components
        if (uiManager != null)
        {
            if (mainInfoText != null)
            {
                uiManager.SetCellInfoText(mainInfoText);
                Debug.Log("[PassiveTreeUISetup] Connected main info text");
            }

            if (nameText != null || descriptionText != null || typeText != null || costText != null || statusText != null)
            {
                uiManager.SetTextComponents(nameText, descriptionText, typeText, costText, statusText);
                Debug.Log("[PassiveTreeUISetup] Connected individual text components");
            }
        }

        // Connect to PassiveTreeManager
        ConnectToPassiveTree();

        Debug.Log("[PassiveTreeUISetup] UI setup complete!");
    }

    /// <summary>
    /// Connect to the passive tree system
    /// </summary>
    private void ConnectToPassiveTree()
    {
        PassiveTreeManager treeManager = FindFirstObjectByType<PassiveTreeManager>();
        if (treeManager != null)
        {
            treeManager.SetUIManager(uiManager);
            Debug.Log("[PassiveTreeUISetup] Connected to PassiveTreeManager");
        }
        else
        {
            Debug.LogWarning("[PassiveTreeUISetup] No PassiveTreeManager found in scene!");
        }
    }

    /// <summary>
    /// Manually set the main info text component
    /// </summary>
    public void SetMainInfoText(TextMeshProUGUI textComponent)
    {
        mainInfoText = textComponent;
        if (uiManager != null)
        {
            uiManager.SetCellInfoText(textComponent);
        }
    }

    /// <summary>
    /// Manually set individual text components
    /// </summary>
    public void SetTextComponents(TextMeshProUGUI name, TextMeshProUGUI desc, TextMeshProUGUI type, TextMeshProUGUI cost, TextMeshProUGUI status)
    {
        nameText = name;
        descriptionText = desc;
        typeText = type;
        costText = cost;
        statusText = status;

        if (uiManager != null)
        {
            uiManager.SetTextComponents(name, desc, type, cost, status);
        }
    }

    /// <summary>
    /// Test the UI by simulating a cell selection
    /// </summary>
    [ContextMenu("Test UI")]
    public void TestUI()
    {
        if (uiManager != null)
        {
            // Simulate selecting a cell at position (3, 3)
            uiManager.OnCellSelected(new Vector2Int(3, 3));
            Debug.Log("[PassiveTreeUISetup] Tested UI with cell (3, 3)");
        }
        else
        {
            Debug.LogWarning("[PassiveTreeUISetup] No UI manager found. Please run Setup UI first.");
        }
    }

    /// <summary>
    /// Clear the UI display
    /// </summary>
    [ContextMenu("Clear UI")]
    public void ClearUI()
    {
        if (uiManager != null)
        {
            uiManager.ClearUI();
            Debug.Log("[PassiveTreeUISetup] Cleared UI display");
        }
    }

    /// <summary>
    /// Get the UI manager
    /// </summary>
    public PassiveTreeUI GetUIManager()
    {
        return uiManager;
    }
}
