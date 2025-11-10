using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the central Ascendancy splash art container.
/// Displays the Ascendancy's main visual with circular frame.
/// </summary>
public class AscendancyContainerController : MonoBehaviour
{
    [Header("References (Auto-Found if not assigned)")]
    [SerializeField] private Image splashArtImage;
    [SerializeField] private Image frameOverlay;
    [SerializeField] private Image circularFrame;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugLogs = false;
    
    private AscendancyData currentAscendancy;
    
    void Awake()
    {
        // Auto-find references if not assigned
        if (splashArtImage == null)
        {
            // Find child named "AscendancySplashArt"
            Transform splashArtTransform = transform.Find("SplashArt/AscendancySplashArt");
            if (splashArtTransform != null)
                splashArtImage = splashArtTransform.GetComponent<Image>();
        }
        
        if (frameOverlay == null)
        {
            Transform frameTransform = transform.Find("FrameOverlay");
            if (frameTransform != null)
                frameOverlay = frameTransform.GetComponent<Image>();
        }
        
        if (circularFrame == null)
        {
            Transform circleTransform = transform.Find("CircularFrame");
            if (circleTransform != null)
                circularFrame = circleTransform.GetComponent<Image>();
        }
    }
    
    /// <summary>
    /// Initialize with Ascendancy data
    /// </summary>
    public void Initialize(AscendancyData ascendancy)
    {
        if (ascendancy == null)
        {
            Debug.LogError("[AscendancyContainer] Cannot initialize with null AscendancyData!");
            return;
        }
        
        currentAscendancy = ascendancy;
        
        // Set splash art
        if (splashArtImage != null && ascendancy.splashArt != null)
        {
            splashArtImage.sprite = ascendancy.splashArt;
            if (showDebugLogs)
                Debug.Log($"[AscendancyContainer] Set splash art: {ascendancy.splashArt.name}");
        }
        
        // Apply theme color to frame overlay (subtle tint)
        if (frameOverlay != null)
        {
            Color tintColor = ascendancy.themeColor;
            tintColor.a = 0.3f; // Semi-transparent
            frameOverlay.color = tintColor;
        }
        
        // Optional: Apply theme color to circular frame
        if (circularFrame != null)
        {
            circularFrame.color = ascendancy.themeColor;
        }
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyContainer] Initialized with {ascendancy.ascendancyName}");
    }
    
    /// <summary>
    /// Get current Ascendancy data
    /// </summary>
    public AscendancyData GetAscendancy()
    {
        return currentAscendancy;
    }
    
    /// <summary>
    /// Set frame color (for visual variety or state indication)
    /// </summary>
    public void SetFrameColor(Color color)
    {
        if (circularFrame != null)
        {
            circularFrame.color = color;
        }
    }
}

