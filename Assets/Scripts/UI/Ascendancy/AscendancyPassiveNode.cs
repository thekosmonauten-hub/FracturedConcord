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
        // Auto-find references
        if (nodeImage == null)
            nodeImage = GetComponentInChildren<Image>();
        
        if (nodeButton == null)
            nodeButton = GetComponentInChildren<Button>();
        
        if (nodeButton != null)
        {
            nodeButton.onClick.AddListener(OnButtonClicked);
        }
        
        originalScale = transform.localScale;
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
        
        // Update visual state
        UpdateVisualState();
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyPassiveNode] Initialized: {passive.name} (Unlocked: {unlocked})");
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
        if (nodeImage == null) return;
        
        if (isUnlocked)
        {
            // Unlocked: Full color, interactable
            nodeImage.color = unlockedColor;
            if (nodeButton != null)
                nodeButton.interactable = true;
        }
        else if (isAvailable)
        {
            // Available to unlock: Highlighted, interactable
            nodeImage.color = availableColor;
            if (nodeButton != null)
                nodeButton.interactable = true;
        }
        else
        {
            // Locked: Greyed out, not interactable
            nodeImage.color = lockedColor;
            if (nodeButton != null)
                nodeButton.interactable = false;
        }
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
    
    void OnDestroy()
    {
        if (nodeButton != null)
        {
            nodeButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}

