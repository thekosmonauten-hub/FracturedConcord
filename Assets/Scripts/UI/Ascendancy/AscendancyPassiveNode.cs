using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a single passive ability node in the Ascendancy tree.
/// Displays the passive's icon and handles unlock state visualization.
/// </summary>
public class AscendancyPassiveNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References (Auto-Found if not assigned)")]
    [SerializeField] private Image nodeImage;
    [SerializeField] private Button nodeButton;
    
    [Header("Visual States")]
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color availableColor = new Color(1f, 1f, 0.5f, 1f); // Yellowish for can-unlock
    [SerializeField] private float hoverScale = 1.1f;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugLogs = false;
    
    // Data
    private AscendancyPassive passiveData;
    private AscendancyData ascendancyData;
    private bool isUnlocked = false;
    private bool isAvailable = false;
    private Vector3 originalScale;
    
    // Events
    public System.Action<AscendancyPassiveNode> OnNodeClicked;
    public System.Action<AscendancyPassiveNode> OnNodeHoverEnter;
    public System.Action<AscendancyPassiveNode> OnNodeHoverExit;
    
    void Awake()
    {
        // Ensure EventSystem exists (required for UI clicks)
        EnsureEventSystem();
        
        originalScale = transform.localScale;
    }
    
    void Start()
    {
        // Setup button and image after prefab is fully instantiated
        // This ensures child objects are available
        SetupButtonAndImage();
        
        // Update visual state if already initialized
        if (passiveData != null)
        {
            UpdateVisualState();
        }
    }
    
    /// <summary>
    /// Ensure EventSystem exists in the scene (required for UI button clicks)
    /// </summary>
    void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("[AscendancyPassiveNode] Created EventSystem - required for UI clicks");
        }
    }
    
    /// <summary>
    /// Setup button and image with proper settings
    /// </summary>
    void SetupButtonAndImage()
    {
        // Find Image - search in children first (prefab has it on child "NodeArtButton")
        if (nodeImage == null)
        {
            nodeImage = GetComponentInChildren<Image>(true); // Include inactive
            if (nodeImage == null)
                nodeImage = GetComponent<Image>();
        }
        
        // Find Button - search in children first (prefab has it on child "NodeArtButton")
        if (nodeButton == null)
        {
            nodeButton = GetComponentInChildren<Button>(true); // Include inactive
            if (nodeButton == null)
                nodeButton = GetComponent<Button>();
            
            // If no button found, add one to root
            if (nodeButton == null)
            {
                nodeButton = gameObject.AddComponent<Button>();
                Debug.Log($"[AscendancyPassiveNode] Added Button component to {gameObject.name}");
            }
        }
        
        // Ensure Image has raycastTarget = true (required for button clicks)
        if (nodeImage != null)
        {
            nodeImage.raycastTarget = true;
            if (showDebugLogs)
                Debug.Log($"[AscendancyPassiveNode] Found Image: {nodeImage.gameObject.name}, raycastTarget={nodeImage.raycastTarget}");
        }
        else
        {
            // If no image, add one for visual feedback
            nodeImage = gameObject.AddComponent<Image>();
            nodeImage.raycastTarget = true;
            Debug.Log($"[AscendancyPassiveNode] Added Image component to {gameObject.name}");
        }
        
        // Setup button
        if (nodeButton != null)
        {
            // Remove any existing listeners to avoid duplicates
            nodeButton.onClick.RemoveAllListeners();
            nodeButton.onClick.AddListener(OnButtonClicked);
            
            // Set button to use the image as target graphic
            if (nodeImage != null)
            {
                nodeButton.targetGraphic = nodeImage;
            }
            
            // Ensure button is interactable (will be updated by UpdateVisualState)
            nodeButton.interactable = true;
            
            // Make button colors transparent (we're using image for visuals)
            var colors = nodeButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            nodeButton.colors = colors;
            
            if (showDebugLogs)
                Debug.Log($"[AscendancyPassiveNode] Button setup complete on {gameObject.name} (Button: {nodeButton.gameObject.name}, Image: {nodeImage?.gameObject.name ?? "NULL"})");
        }
        else
        {
            Debug.LogError($"[AscendancyPassiveNode] Could not create or find Button on {gameObject.name}!");
        }
    }
    
    /// <summary>
    /// Initialize node with passive ability data
    /// </summary>
    public void Initialize(AscendancyPassive passive, AscendancyData ascendancy, bool unlocked = false)
    {
        if (passive == null)
        {
            Debug.LogError("[AscendancyPassiveNode] Cannot initialize with null passive!");
            return;
        }
        
        // Ensure button and image are set up before initialization
        if (nodeButton == null || nodeImage == null)
        {
            SetupButtonAndImage();
        }
        
        passiveData = passive;
        ascendancyData = ascendancy;
        isUnlocked = unlocked;
        
        // Set icon from passive data
        if (nodeImage != null && passive.icon != null)
        {
            nodeImage.sprite = passive.icon;
        }
        else if (nodeImage != null && passive.icon == null)
        {
            // No icon assigned - could use a default placeholder
            Debug.LogWarning($"[AscendancyPassiveNode] Passive '{passive.name}' has no icon assigned!");
        }
        
        // Update visual state (this will set colors and interactability)
        UpdateVisualState();
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyPassiveNode] Initialized: {passive.name} (Unlocked: {unlocked}, Image: {nodeImage?.gameObject.name ?? "NULL"}, Button: {nodeButton?.gameObject.name ?? "NULL"})");
    }
    
    /// <summary>
    /// Set unlock state
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        UpdateVisualState();
    }
    
    /// <summary>
    /// Set available state (can be unlocked - has points and prerequisites met)
    /// </summary>
    public void SetAvailable(bool available)
    {
        isAvailable = available;
        UpdateVisualState();
    }
    
    /// <summary>
    /// Update visual appearance based on state
    /// </summary>
    void UpdateVisualState()
    {
        // Ensure button and image are set up
        if (nodeButton == null || nodeImage == null)
        {
            SetupButtonAndImage();
        }
        
        if (nodeImage == null) return;
        
        // Ensure image can receive raycasts
        nodeImage.raycastTarget = true;
        
        if (isUnlocked)
        {
            // Unlocked: Full color, interactable
            nodeImage.color = unlockedColor;
            if (nodeButton != null)
            {
                nodeButton.interactable = true;
            }
        }
        else if (isAvailable)
        {
            // Available to unlock: Highlighted, interactable
            nodeImage.color = availableColor;
            if (nodeButton != null)
            {
                nodeButton.interactable = true;
            }
        }
        else
        {
            // Locked: Greyed out, not interactable
            nodeImage.color = lockedColor;
            if (nodeButton != null)
            {
                nodeButton.interactable = false;
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyPassiveNode] Updated visual state: Unlocked={isUnlocked}, Available={isAvailable}, Interactable={nodeButton?.interactable ?? false}");
    }
    
    void OnButtonClicked()
    {
        if (passiveData == null) return;
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyPassiveNode] Clicked: {passiveData.name}");
        
        OnNodeClicked?.Invoke(this);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Hover scale effect
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, originalScale * hoverScale, 0.2f).setEaseOutBack();
        
        OnNodeHoverEnter?.Invoke(this);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Return to original scale
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, originalScale, 0.2f).setEaseInBack();
        
        OnNodeHoverExit?.Invoke(this);
    }
    
    /// <summary>
    /// Get the passive data this node represents
    /// </summary>
    public AscendancyPassive GetPassiveData()
    {
        return passiveData;
    }
    
    /// <summary>
    /// Get the ascendancy this passive belongs to
    /// </summary>
    public AscendancyData GetAscendancyData()
    {
        return ascendancyData;
    }
    
    /// <summary>
    /// Check if this node is unlocked
    /// </summary>
    public bool IsUnlocked()
    {
        return isUnlocked;
    }
    
    /// <summary>
    /// Check if this node is available to unlock
    /// </summary>
    public bool IsAvailable()
    {
        return isAvailable;
    }
    
    /// <summary>
    /// Update the node's icon
    /// </summary>
    public void UpdateIcon(Sprite newIcon)
    {
        if (nodeImage != null && newIcon != null)
        {
            nodeImage.sprite = newIcon;
            if (showDebugLogs)
                Debug.Log($"[AscendancyPassiveNode] Updated icon for {passiveData?.name ?? "unknown"}");
        }
    }
    
    /// <summary>
    /// Update the node's description (for display purposes)
    /// Note: This doesn't modify the passive data, just stores it for tooltip/info display
    /// </summary>
    private string overrideDescription = null;
    
    public void UpdateDescription(string newDescription)
    {
        overrideDescription = newDescription;
        if (showDebugLogs)
            Debug.Log($"[AscendancyPassiveNode] Updated description override for {passiveData?.name ?? "unknown"}");
    }
    
    /// <summary>
    /// Get the effective description (uses override if set, otherwise passive description)
    /// </summary>
    public string GetEffectiveDescription()
    {
        if (!string.IsNullOrEmpty(overrideDescription))
            return overrideDescription;
        return passiveData != null ? passiveData.description : "";
    }
    
    /// <summary>
    /// Force refresh visual state (useful for debugging)
    /// </summary>
    [ContextMenu("Refresh Visual State")]
    public void RefreshVisualState()
    {
        SetupButtonAndImage();
        UpdateVisualState();
        Debug.Log($"[AscendancyPassiveNode] Refreshed visual state: Unlocked={isUnlocked}, Available={isAvailable}, Button={nodeButton != null}, Image={nodeImage != null}");
    }
    
    /// <summary>
    /// Debug button setup (context menu)
    /// </summary>
    [ContextMenu("Debug Button Setup")]
    public void DebugButtonSetup()
    {
        Debug.Log("=== AscendancyPassiveNode Debug ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Active: {gameObject.activeSelf}, ActiveInHierarchy: {gameObject.activeInHierarchy}");
        Debug.Log($"NodeImage: {(nodeImage != null ? nodeImage.gameObject.name : "NULL")}");
        Debug.Log($"NodeButton: {(nodeButton != null ? nodeButton.gameObject.name : "NULL")}");
        Debug.Log($"Button Interactable: {nodeButton?.interactable ?? false}");
        Debug.Log($"Image RaycastTarget: {nodeImage?.raycastTarget ?? false}");
        Debug.Log($"IsUnlocked: {isUnlocked}");
        Debug.Log($"IsAvailable: {isAvailable}");
        Debug.Log($"EventSystem: {(EventSystem.current != null ? "EXISTS" : "MISSING")}");
        
        // Check for buttons in children
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        Debug.Log($"Buttons in children: {allButtons.Length}");
        foreach (var btn in allButtons)
        {
            Debug.Log($"  - {btn.gameObject.name} (Active: {btn.gameObject.activeSelf}, Interactable: {btn.interactable})");
        }
        
        // Check for images in children
        Image[] allImages = GetComponentsInChildren<Image>(true);
        Debug.Log($"Images in children: {allImages.Length}");
        foreach (var img in allImages)
        {
            Debug.Log($"  - {img.gameObject.name} (RaycastTarget: {img.raycastTarget})");
        }
        
        Debug.Log("=== End Debug ===");
    }
    
    void OnDestroy()
    {
        if (nodeButton != null)
        {
            nodeButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}

