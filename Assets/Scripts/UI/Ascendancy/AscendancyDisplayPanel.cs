using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel that displays the full Ascendancy tree when an Ascendancy button is clicked.
/// Uses AscendancyContainerPrefab to display the tree.
/// </summary>
public class AscendancyDisplayPanel : MonoBehaviour
{
    [Header("Prefab Mode")]
    [Tooltip("Use prefab for display (recommended - uses AscendancyContainerPrefab)")]
    [SerializeField] private bool usePrefabMode = true;
    [SerializeField] private GameObject ascendancyContainerPrefab;
    [SerializeField] private Transform contentContainer;
    
    [Header("Panel Mode")]
    [Tooltip("Preview mode (view only) or Selection mode (can allocate points)")]
    [SerializeField] private bool isPreviewMode = true; // True for CharacterDisplayUI
    [SerializeField] private bool allowPointAllocation = false; // False for character creation
    
    [Header("Panel References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backButton;
    
    [Header("Manual Mode (Legacy - use Prefab Mode instead)")]
    [SerializeField] private Image splashArtImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI taglineText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI coreMechanicNameText;
    [SerializeField] private TextMeshProUGUI coreMechanicDescriptionText;
    [SerializeField] private TextMeshProUGUI signatureCardText;
    [SerializeField] private AscendancyTreeDisplay treeDisplay;
    [SerializeField] private TextMeshProUGUI availablePointsText;
    [SerializeField] private TextMeshProUGUI spentPointsText;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugLogs = true;
    
    private AscendancyData currentAscendancy;
    private CharacterAscendancyProgress currentProgress;
    private GameObject spawnedContainer;
    
    void Awake()
    {
        // Setup buttons
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        
        if (backButton != null)
            backButton.onClick.AddListener(ClosePanel);
        
        // Hide panel initially
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
    
    /// <summary>
    /// Show the panel with Ascendancy data
    /// </summary>
    public void ShowAscendancy(AscendancyData ascendancy, CharacterAscendancyProgress progress = null, bool allowAllocation = false)
    {
        if (ascendancy == null)
        {
            Debug.LogError("[AscendancyDisplayPanel] Cannot show null Ascendancy!");
            return;
        }
        
        currentAscendancy = ascendancy;
        currentProgress = progress ?? new CharacterAscendancyProgress();
        
        // Set allocation mode
        allowPointAllocation = allowAllocation;
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyDisplayPanel] Showing: {ascendancy.ascendancyName} (Allow Allocation: {allowAllocation})");
        
        // Clear previous container if any
        if (spawnedContainer != null)
        {
            Destroy(spawnedContainer);
            spawnedContainer = null;
        }
        
        if (usePrefabMode)
        {
            // Spawn prefab and populate it
            DisplayWithPrefab(ascendancy);
        }
        else
        {
            // Use manually assigned components
            DisplayInfo(ascendancy);
            
            if (treeDisplay != null)
            {
                treeDisplay.DisplayAscendancy(ascendancy, currentProgress, allowAllocation);
            }
            
            UpdateProgressionInfo();
        }
        
        // Show panel
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }
    
    /// <summary>
    /// Set whether point allocation is allowed (for external control)
    /// </summary>
    public void SetAllowPointAllocation(bool allow)
    {
        allowPointAllocation = allow;
    }
    
    /// <summary>
    /// Display using AscendancyContainerPrefab
    /// </summary>
    void DisplayWithPrefab(AscendancyData ascendancy)
    {
        if (ascendancyContainerPrefab == null)
        {
            Debug.LogError("[AscendancyDisplayPanel] AscendancyContainerPrefab not assigned!");
            return;
        }
        
        Transform parent = contentContainer != null ? contentContainer : transform;
        
        // Spawn container prefab
        spawnedContainer = Instantiate(ascendancyContainerPrefab, parent);
        spawnedContainer.name = $"AscendancyContainer_{ascendancy.ascendancyName}";
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyDisplayPanel] Spawned container: {spawnedContainer.name}");
        
        // Find and populate components in the prefab
        PopulatePrefabComponents(spawnedContainer, ascendancy);
        
        // Find and setup tree display
        AscendancyTreeDisplay prefabTreeDisplay = spawnedContainer.GetComponentInChildren<AscendancyTreeDisplay>();
        if (prefabTreeDisplay != null)
        {
            prefabTreeDisplay.DisplayAscendancy(ascendancy, currentProgress, allowPointAllocation);
            
            // Disable node interaction if in preview mode
            if (isPreviewMode || !allowPointAllocation)
            {
                DisableTreeInteraction(prefabTreeDisplay);
            }
            
            if (showDebugLogs)
                Debug.Log($"[AscendancyDisplayPanel] ✓ Tree display initialized (Preview Mode: {isPreviewMode})");
        }
        else
        {
            Debug.LogWarning("[AscendancyDisplayPanel] No AscendancyTreeDisplay found in prefab!");
        }
    }
    
    /// <summary>
    /// Disable node interaction in preview mode
    /// </summary>
    void DisableTreeInteraction(AscendancyTreeDisplay treeDisplay)
    {
        // Find all passive nodes
        AscendancyPassiveNode[] nodes = treeDisplay.GetComponentsInChildren<AscendancyPassiveNode>();
        
        foreach (var node in nodes)
        {
            // Disable button interaction (but keep hover for tooltip)
            Button nodeButton = node.GetComponentInChildren<Button>();
            if (nodeButton != null)
            {
                nodeButton.interactable = false;
            }
        }
        
        // Disable unlock button if present
        Button unlockButton = treeDisplay.GetComponentInChildren<Button>();
        if (unlockButton != null && unlockButton.gameObject.name.Contains("Unlock"))
        {
            unlockButton.gameObject.SetActive(false);
        }
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyDisplayPanel] Disabled tree interaction (preview mode)");
    }
    
    /// <summary>
    /// Populate components in the spawned prefab
    /// </summary>
    void PopulatePrefabComponents(GameObject container, AscendancyData ascendancy)
    {
        // Find AscendancyContainerController (on root)
        AscendancyContainerController controller = container.GetComponent<AscendancyContainerController>();
        if (controller != null)
        {
            controller.Initialize(ascendancy);
            if (showDebugLogs)
                Debug.Log($"[AscendancyDisplayPanel] ✓ Container controller initialized");
        }
        
        // Find and populate text components
        TextMeshProUGUI[] allTexts = container.GetComponentsInChildren<TextMeshProUGUI>(true);
        
        foreach (var text in allTexts)
        {
            string objName = text.gameObject.name;
            
            if (objName == "NameText" || objName == "Name")
            {
                text.text = ascendancy.ascendancyName;
                if (showDebugLogs)
                    Debug.Log($"[AscendancyDisplayPanel] ✓ Set name: {ascendancy.ascendancyName}");
            }
            else if (objName == "TagLine" || objName == "Tagline" || objName == "TaglineText")
            {
                text.text = ascendancy.tagline;
                if (showDebugLogs)
                    Debug.Log($"[AscendancyDisplayPanel] ✓ Set tagline: {ascendancy.tagline}");
            }
            else if (objName == "AvailablePointsText" || objName == "PointsText")
            {
                text.text = $"Points: {currentProgress.availableAscendancyPoints}/{currentProgress.totalAscendancyPoints}";
                if (showDebugLogs)
                    Debug.Log($"[AscendancyDisplayPanel] ✓ Set points text");
            }
        }
    }
    
    /// <summary>
    /// Display Ascendancy info text
    /// </summary>
    void DisplayInfo(AscendancyData ascendancy)
    {
        // Splash art
        if (splashArtImage != null && ascendancy.splashArt != null)
        {
            splashArtImage.sprite = ascendancy.splashArt;
        }
        
        // Name and tagline
        if (nameText != null)
            nameText.text = ascendancy.ascendancyName;
        
        if (taglineText != null)
            taglineText.text = ascendancy.tagline;
        
        // Description
        if (descriptionText != null)
            descriptionText.text = ascendancy.description;
        
        // Core mechanic
        if (coreMechanicNameText != null)
            coreMechanicNameText.text = ascendancy.coreMechanicName;
        
        if (coreMechanicDescriptionText != null)
            coreMechanicDescriptionText.text = ascendancy.coreMechanicDescription;
        
        // Signature card
        if (signatureCardText != null && ascendancy.signatureCard != null)
        {
            signatureCardText.text = ascendancy.signatureCard.GetCardInfo();
        }
    }
    
    /// <summary>
    /// Update progression info display
    /// </summary>
    void UpdateProgressionInfo()
    {
        if (currentProgress == null) return;
        
        if (availablePointsText != null)
        {
            availablePointsText.text = $"Available: {currentProgress.availableAscendancyPoints}";
        }
        
        if (spentPointsText != null)
        {
            spentPointsText.text = $"Spent: {currentProgress.spentAscendancyPoints}/{currentProgress.totalAscendancyPoints}";
        }
    }
    
    /// <summary>
    /// Close the panel
    /// </summary>
    public void ClosePanel()
    {
        if (showDebugLogs)
            Debug.Log("[AscendancyDisplayPanel] Closing panel");
        
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
    
    /// <summary>
    /// Check if panel is currently open
    /// </summary>
    public bool IsOpen()
    {
        return panelRoot != null && panelRoot.activeSelf;
    }
    
    void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);
        
        if (backButton != null)
            backButton.onClick.RemoveListener(ClosePanel);
    }
}

