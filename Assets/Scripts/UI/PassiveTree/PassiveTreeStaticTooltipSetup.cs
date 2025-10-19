using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Setup helper for the static tooltip system
/// Creates the tooltip UI structure automatically
/// </summary>
public class PassiveTreeStaticTooltipSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    [SerializeField] private bool createTooltipOnStart = true;
    [SerializeField] private bool createUIElements = true;
    
    [Header("Tooltip Settings")]
    [SerializeField] private Vector2 tooltipSize = new Vector2(400, 150); // Fixed width, minimum height for dynamic sizing
    [SerializeField] private Vector2 bottomLeftOffset = new Vector2(20, 20);
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private bool useBackgroundSprite = false;
    
    [Header("Text Settings")]
    [SerializeField] private int nameFontSize = 16;
    [SerializeField] private int descriptionFontSize = 14;
    [SerializeField] private int statsFontSize = 12;
    
    [Header("References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private PassiveTreeStaticTooltip tooltipComponent;
    
    void Start()
    {
        if (createTooltipOnStart)
        {
            SetupStaticTooltip();
        }
        
        // Make tooltip persistent when game starts (if not already persistent)
        MakeTooltipPersistent();
    }
    
    /// <summary>
    /// Setup the static tooltip system
    /// </summary>
    [ContextMenu("Setup Static Tooltip")]
    public void SetupStaticTooltip()
    {
        Debug.Log("[PassiveTreeStaticTooltipSetup] Setting up static tooltip system...");
        
        // Create or find a dedicated tooltip canvas
        Canvas tooltipCanvas = CreateOrFindTooltipCanvas();
        
        if (tooltipCanvas == null)
        {
            Debug.LogError("[PassiveTreeStaticTooltipSetup] Failed to create tooltip canvas!");
            return;
        }
        
        // Create tooltip GameObject
        GameObject tooltipObject = CreateTooltipObject(tooltipCanvas);
        
        // Add tooltip component
        if (tooltipComponent == null)
        {
            tooltipComponent = tooltipObject.GetComponent<PassiveTreeStaticTooltip>();
            if (tooltipComponent == null)
            {
                tooltipComponent = tooltipObject.AddComponent<PassiveTreeStaticTooltip>();
            }
        }
        
        // Create UI elements if requested
        if (createUIElements)
        {
            CreateTooltipUIElements(tooltipObject);
        }
        
        // Ensure tooltip is properly configured
        if (tooltipComponent != null)
        {
            // Make sure the tooltip GameObject is active in hierarchy (but not visible until hover)
            if (!tooltipComponent.gameObject.activeInHierarchy)
            {
                tooltipComponent.gameObject.SetActive(true);
            }
        }
        
        Debug.Log("[PassiveTreeStaticTooltipSetup] Static tooltip system setup complete!");
    }
    
    /// <summary>
    /// Create or find a dedicated tooltip canvas
    /// </summary>
    private Canvas CreateOrFindTooltipCanvas()
    {
        // First, look for existing tooltip canvas
        GameObject existingTooltipCanvas = GameObject.Find("PassiveTreeTooltipCanvas");
        if (existingTooltipCanvas != null)
        {
            Canvas canvas = existingTooltipCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                Debug.Log("[PassiveTreeStaticTooltipSetup] Found existing tooltip canvas");
                return canvas;
            }
        }
        
        // Create a new dedicated tooltip canvas
        GameObject tooltipCanvasObject = new GameObject("PassiveTreeTooltipCanvas");
        
        // Add Canvas component
        Canvas canvasComponent = tooltipCanvasObject.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComponent.sortingOrder = 1000; // High sorting order to appear on top
        
        // Add CanvasScaler
        UnityEngine.UI.CanvasScaler scaler = tooltipCanvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster
        tooltipCanvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // DO NOT make canvas persistent - it should be destroyed when leaving the scene
        // Otherwise it blocks backgrounds in other scenes with its high sortingOrder (1000)
        Debug.Log("[PassiveTreeStaticTooltipSetup] Created tooltip canvas (scene-local, will be cleaned up on scene exit)");
        
        return canvasComponent;
    }
    
    /// <summary>
    /// Make the tooltip persistent when the game starts
    /// DISABLED: Persistent tooltip canvases block backgrounds in other scenes
    /// </summary>
    private void MakeTooltipPersistent()
    {
        // DO NOT make tooltip persistent - it blocks other scene backgrounds
        // The tooltip will be recreated if needed when returning to PassiveTreeScene
        Debug.Log("[PassiveTreeStaticTooltipSetup] MakeTooltipPersistent() disabled to prevent background blocking.");
    }
    
    /// <summary>
    /// Find the target canvas for the tooltip
    /// </summary>
    private Canvas FindTargetCanvas()
    {
        // Look for PassiveScreen canvas first
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.name.ToLower().Contains("passive") || canvas.name.ToLower().Contains("passivescreen"))
            {
                Debug.Log($"[PassiveTreeStaticTooltipSetup] Found PassiveScreen canvas: {canvas.name}");
                return canvas;
            }
        }
        
        // Look for a persistent UI canvas
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.name.ToLower().Contains("ui") || canvas.name.ToLower().Contains("hud") || canvas.name.ToLower().Contains("overlay"))
            {
                Debug.Log($"[PassiveTreeStaticTooltipSetup] Found UI canvas: {canvas.name}");
                return canvas;
            }
        }
        
        // Fallback to any canvas that's not a world space canvas
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                Debug.Log($"[PassiveTreeStaticTooltipSetup] Using fallback canvas: {canvas.name}");
                return canvas;
            }
        }
        
        // Last resort - use any canvas
        if (allCanvases.Length > 0)
        {
            Debug.Log($"[PassiveTreeStaticTooltipSetup] Using last resort canvas: {allCanvases[0].name}");
            return allCanvases[0];
        }
        
        return null;
    }
    
    /// <summary>
    /// Create the tooltip GameObject
    /// </summary>
    private GameObject CreateTooltipObject(Canvas parentCanvas)
    {
        GameObject tooltipObject = new GameObject("PassiveTreeStaticTooltip");
        tooltipObject.transform.SetParent(parentCanvas.transform, false);
        
        // Add RectTransform
        RectTransform rectTransform = tooltipObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = tooltipSize;
        
        // Set anchor to bottom-left
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0, 0);
        rectTransform.anchoredPosition = bottomLeftOffset;
        
        // Add Image component for background
        Image backgroundImage = tooltipObject.AddComponent<Image>();
        
        // Configure background based on settings
        if (useBackgroundSprite && backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = Color.white; // Use sprite's original colors
            backgroundImage.type = Image.Type.Sliced; // Use sliced for better scaling
        }
        else
        {
            backgroundImage.sprite = null;
            backgroundImage.color = backgroundColor;
        }
        
        // Add ContentSizeFitter (disabled for manual height control)
        ContentSizeFitter sizeFitter = tooltipObject.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        
        // Add VerticalLayoutGroup
        VerticalLayoutGroup layoutGroup = tooltipObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(8, 8, 8, 8); // Reduced padding for more compact layout
        layoutGroup.spacing = 0; // No spacing for maximum compactness
        layoutGroup.childControlHeight = true; // Enable child size control for height
        layoutGroup.childControlWidth = true; // Enable child size control for width
        layoutGroup.childForceExpandHeight = true; // Enable child force expand for height
        layoutGroup.childForceExpandWidth = true; // Enable child force expand for width
        
        // Initially hide the tooltip (it will be shown on hover)
        tooltipObject.SetActive(false);
        
        // Tooltip canvas is already persistent (created with DontDestroyOnLoad)
        
        Debug.Log("[PassiveTreeStaticTooltipSetup] Created persistent tooltip GameObject");
        return tooltipObject;
    }
    
    /// <summary>
    /// Create UI elements for the tooltip
    /// </summary>
    private void CreateTooltipUIElements(GameObject tooltipObject)
    {
        // Create name text
        GameObject nameObj = CreateTextObject("Name", tooltipObject, nameFontSize, FontStyles.Bold);
        TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
        nameText.text = "Select a Passive";
        nameText.alignment = TextAlignmentOptions.Center;
        
        // Add LayoutElement to name
        LayoutElement nameLayout = nameObj.AddComponent<LayoutElement>();
        nameLayout.preferredHeight = nameFontSize + 4; // Reduced for more compact layout
        
        // Create description text
        GameObject descObj = CreateTextObject("Description", tooltipObject, descriptionFontSize, FontStyles.Normal);
        TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
        descText.text = "Hover over a passive node to see its details";
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.textWrappingMode = TextWrappingModes.Normal;
        
        // Add LayoutElement to description (dynamic sizing)
        LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
        descLayout.preferredHeight = 50; // Set a reasonable default height
        descLayout.flexibleHeight = 0; // No flexible height
        
        // Create stats text
        GameObject statsObj = CreateTextObject("Stats", tooltipObject, statsFontSize, FontStyles.Normal);
        TextMeshProUGUI statsText = statsObj.GetComponent<TextMeshProUGUI>();
        statsText.text = "";
        statsText.alignment = TextAlignmentOptions.TopLeft;
        statsText.textWrappingMode = TextWrappingModes.Normal;
        
        // Add LayoutElement to stats (dynamic sizing)
        LayoutElement statsLayout = statsObj.AddComponent<LayoutElement>();
        statsLayout.preferredHeight = 60; // Set a reasonable default height
        statsLayout.flexibleHeight = 0; // No flexible height
        
        Debug.Log("[PassiveTreeStaticTooltipSetup] Created tooltip UI elements");
    }
    
    /// <summary>
    /// Create a text object with TextMeshPro
    /// </summary>
    private GameObject CreateTextObject(string name, GameObject parent, int fontSize, FontStyles fontStyle)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = name;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.color = textColor;
        
        return textObj;
    }
    
    
    /// <summary>
    /// Make tooltip persistent (play mode only)
    /// </summary>
    [ContextMenu("Make Tooltip Persistent")]
    public void ManualMakeTooltipPersistent()
    {
        if (Application.isPlaying)
        {
            MakeTooltipPersistent();
        }
        else
        {
            Debug.LogWarning("[PassiveTreeStaticTooltipSetup] Can only make tooltip persistent in play mode. Start the game first.");
        }
    }
    
    /// <summary>
    /// Test the tooltip system
    /// </summary>
    [ContextMenu("Test Tooltip System")]
    public void TestTooltipSystem()
    {
        if (tooltipComponent != null)
        {
            tooltipComponent.TestTooltip();
        }
        else
        {
            Debug.LogWarning("[PassiveTreeStaticTooltipSetup] No tooltip component found. Run setup first.");
        }
    }
    
    /// <summary>
    /// Get the tooltip component
    /// </summary>
    public PassiveTreeStaticTooltip GetTooltipComponent()
    {
        return tooltipComponent;
    }
}
