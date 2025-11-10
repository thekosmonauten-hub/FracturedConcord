using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for an Ascendancy selection button.
/// Displays splash art, name, and basic info.
/// Handles click events to show detailed Ascendancy information.
/// </summary>
[RequireComponent(typeof(Button))]
public class AscendancyButton : MonoBehaviour
{
    [Header("UI References (Auto-Found if not assigned)")]
    [SerializeField] private Image splashArtImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI taglineText;
    [SerializeField] private Image backgroundOverlay;
    
    [Header("Optional: Hover Effect")]
    [SerializeField] private GameObject hoverIndicator;
    [SerializeField] private float hoverScale = 1.05f;
    
    [Header("Lock State")]
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private TextMeshProUGUI lockReasonText;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Button button;
    private AscendancyData ascendancyData;
    private bool isLocked = false;
    
    // Events
    public System.Action<AscendancyData> OnAscendancyClicked;
    
    void Awake()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        
        // Auto-find Image component if not assigned
        if (splashArtImage == null && iconImage == null)
        {
            // Try to find any Image component on this GameObject or children
            Image[] images = GetComponentsInChildren<Image>();
            
            if (images.Length > 0)
            {
                // Use first Image as splash art
                splashArtImage = images[0];
                
                if (showDebugLogs)
                    Debug.Log($"[AscendancyButton] Auto-found Image component on '{gameObject.name}'");
            }
        }
        
        // Auto-find Text components if not assigned
        if (nameText == null)
        {
            // Try to find by name first (direct child)
            Transform nameTransform = transform.Find("Name");
            if (nameTransform == null)
                nameTransform = transform.Find("NameText");
            if (nameTransform == null)
                nameTransform = transform.Find("Text"); // Common Unity default
            
            if (nameTransform != null)
            {
                nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                if (showDebugLogs && nameText != null)
                    Debug.Log($"[AscendancyButton] Auto-found Name text: {nameTransform.name}");
            }
            
            // If still not found, search all children
            if (nameText == null)
            {
                TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>();
                if (allTexts.Length > 0)
                {
                    // Use first TextMeshPro found as name
                    nameText = allTexts[0];
                    if (showDebugLogs)
                        Debug.Log($"[AscendancyButton] Auto-found Name text (fallback): {allTexts[0].gameObject.name}");
                }
            }
        }
        
        if (taglineText == null)
        {
            // Try to find by name
            Transform taglineTransform = transform.Find("Tagline");
            if (taglineTransform == null)
                taglineTransform = transform.Find("TaglineText");
            if (taglineTransform == null)
                taglineTransform = transform.Find("Subtitle");
            
            if (taglineTransform != null)
            {
                taglineText = taglineTransform.GetComponent<TextMeshProUGUI>();
                if (showDebugLogs && taglineText != null)
                    Debug.Log($"[AscendancyButton] Auto-found Tagline text: {taglineTransform.name}");
            }
            
            // If still not found, try second TextMeshPro (if exists)
            if (taglineText == null)
            {
                TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>();
                if (allTexts.Length > 1)
                {
                    // Use second TextMeshPro as tagline
                    taglineText = allTexts[1];
                    if (showDebugLogs)
                        Debug.Log($"[AscendancyButton] Auto-found Tagline text (fallback): {allTexts[1].gameObject.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Initialize this button with Ascendancy data
    /// </summary>
    public void Initialize(AscendancyData data, bool locked = false, string lockReason = "")
    {
        ascendancyData = data;
        isLocked = locked;
        
        if (data == null)
        {
            Debug.LogError("[AscendancyButton] Cannot initialize with null AscendancyData!");
            gameObject.SetActive(false);
            return;
        }
        
        // Re-check for text components in case Awake() didn't find them
        if (nameText == null)
        {
            TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>();
            if (allTexts.Length > 0)
            {
                nameText = allTexts[0];
                if (showDebugLogs)
                    Debug.Log($"[AscendancyButton] Found TextMeshPro during Initialize: {allTexts[0].gameObject.name}");
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"━━━ [AscendancyButton] Initializing '{gameObject.name}' with {data.ascendancyName} ━━━");
            Debug.Log($"  Splash Art: {(data.splashArt != null ? data.splashArt.name : "NULL")}");
            Debug.Log($"  Icon: {(data.icon != null ? data.icon.name : "NULL")}");
            Debug.Log($"  splashArtImage component: {(splashArtImage != null ? "Found" : "NULL")}");
            Debug.Log($"  iconImage component: {(iconImage != null ? "Found" : "NULL")}");
            Debug.Log($"  nameText component: {(nameText != null ? $"Found ({nameText.gameObject.name})" : "NULL")}");
            Debug.Log($"  taglineText component: {(taglineText != null ? $"Found ({taglineText.gameObject.name})" : "NULL")}");
        }
        
        // For buttons, prioritize Icon over SplashArt
        // SplashArt is for full tree display, Icon is for button previews
        
        // If we have separate iconImage and splashArtImage components
        if (iconImage != null && splashArtImage != null)
        {
            // Set icon on iconImage
            if (data.icon != null)
            {
                iconImage.sprite = data.icon;
                if (showDebugLogs)
                    Debug.Log($"  ✓ Set icon on iconImage: {data.icon.name}");
            }
            
            // Set splash art on splashArtImage
            if (data.splashArt != null)
            {
                splashArtImage.sprite = data.splashArt;
                if (showDebugLogs)
                    Debug.Log($"  ✓ Set splash art on splashArtImage: {data.splashArt.name}");
            }
        }
        // If only one Image component exists (most common for buttons), use Icon
        else if (splashArtImage != null && iconImage == null)
        {
            // Single Image component - use Icon for button preview (not SplashArt)
            if (data.icon != null)
            {
                splashArtImage.sprite = data.icon;
                if (showDebugLogs)
                    Debug.Log($"  ✓ Set icon on splashArtImage (button preview): {data.icon.name}");
            }
            else if (data.splashArt != null)
            {
                // Fallback to splash art if no icon available
                splashArtImage.sprite = data.splashArt;
                if (showDebugLogs)
                    Debug.Log($"  ⚠️ Set splash art on splashArtImage (no icon available): {data.splashArt.name}");
            }
        }
        // If only iconImage exists (uncommon)
        else if (iconImage != null && splashArtImage == null)
        {
            if (data.icon != null)
            {
                iconImage.sprite = data.icon;
                if (showDebugLogs)
                    Debug.Log($"  ✓ Set icon on iconImage: {data.icon.name}");
            }
        }
        
        // If we have neither splashArtImage nor iconImage, log error
        if (splashArtImage == null && iconImage == null)
        {
            Debug.LogError($"[AscendancyButton] '{gameObject.name}' has no Image components assigned! Cannot display Ascendancy art.");
            
            // Try to find any Image component as last resort
            Image anyImage = GetComponentInChildren<Image>();
            if (anyImage != null)
            {
                anyImage.sprite = data.icon != null ? data.icon : data.splashArt;
                if (showDebugLogs)
                    Debug.Log($"  ⚠️ Using fallback Image component: {anyImage.gameObject.name}");
            }
        }
        
        // Set name
        if (nameText != null)
        {
            nameText.text = data.ascendancyName;
            if (showDebugLogs)
                Debug.Log($"  ✓ Set name text to: {data.ascendancyName}");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning($"  ⚠️ No name text component found on '{gameObject.name}' - cannot display name");
        }
        
        // Set tagline
        if (taglineText != null)
        {
            taglineText.text = data.tagline;
            if (showDebugLogs)
                Debug.Log($"  ✓ Set tagline text to: {data.tagline}");
        }
        
        // Set theme color for background
        if (backgroundOverlay != null)
        {
            backgroundOverlay.color = new Color(data.themeColor.r, data.themeColor.g, data.themeColor.b, 0.3f);
        }
        
        // Handle locked state (for visual indication)
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(isLocked);
        }
        
        if (lockReasonText != null && isLocked)
        {
            lockReasonText.text = string.IsNullOrEmpty(lockReason) ? 
                $"Unlocks at Level {data.requiredLevel}" : lockReason;
        }
        
        // Keep button interactable even when locked (allows clicking to preview)
        // The actual selection/allocation will be handled in the Ascendancy panel
        if (button != null)
        {
            button.interactable = true; // Always clickable for preview
            
            if (showDebugLogs)
                Debug.Log($"  Button set to interactable (locked={isLocked}, but clickable for preview)");
        }
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyButton] ✓ Initialized: {data.ascendancyName} (Locked: {isLocked})");
    }
    
    void OnButtonClicked()
    {
        if (ascendancyData == null)
        {
            Debug.LogWarning($"[AscendancyButton] Button clicked but has no data!");
            return;
        }
        
        // Always trigger event (even if locked) - panel will handle preview vs selection
        Debug.Log($"[AscendancyButton] Clicked: {ascendancyData.ascendancyName} (Locked: {isLocked})");
        
        // Trigger event
        OnAscendancyClicked?.Invoke(ascendancyData);
    }
    
    /// <summary>
    /// Set locked state
    /// </summary>
    public void SetLocked(bool locked, string reason = "")
    {
        isLocked = locked;
        
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(locked);
        }
        
        if (lockReasonText != null && locked && !string.IsNullOrEmpty(reason))
        {
            lockReasonText.text = reason;
        }
        
        if (button != null)
        {
            button.interactable = !locked;
        }
    }
    
    /// <summary>
    /// Get the Ascendancy data this button represents
    /// </summary>
    public AscendancyData GetAscendancyData()
    {
        return ascendancyData;
    }
    
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}

