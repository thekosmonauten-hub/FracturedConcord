using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Visual representation of a stack icon (Momentum, Agitate, Tolerance, Potential, etc.)
/// Designed specifically for displaying stack counts as resources/buffs
/// </summary>
public class StackIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [Tooltip("Background image for the stack icon")]
    public Image backgroundImage;
    
    [Tooltip("Icon image showing the stack type")]
    public Image iconImage;
    
    [Tooltip("Text displaying the stack count")]
    public TextMeshProUGUI countText;
    
    [Header("Visual Settings")]
    [Tooltip("Background color for stack icons (stacks are treated as buffs/resources)")]
    public Color stackBackgroundColor = new Color(0f, 1f, 0f, 0.3f); // Green with transparency
    
    [Tooltip("Default icon color if no sprite is loaded")]
    public Color defaultIconColor = Color.white;
    
    [Header("Tooltip Settings")]
    [Tooltip("Enable/disable hover tooltips for this stack icon")]
    public bool enableTooltips = true;
    [Tooltip("Tooltip prefab to use (optional, will create simple tooltip if not set)")]
    public GameObject tooltipPrefab;
    
    private StackType stackType;
    private int currentCount = 0;
    private GameObject tooltipInstance;
    private bool isHovering = false;
    private RectTransform tooltipRect;
    
    private void Awake()
    {
        // Ensure this GameObject can receive pointer events (needed for IPointerEnterHandler)
        Graphic graphic = GetComponent<Graphic>();
        if (graphic == null)
        {
            // Add an Image component if none exists (invisible, just for raycasting)
            Image raycastImage = gameObject.AddComponent<Image>();
            raycastImage.color = new Color(1f, 1f, 1f, 0f); // Transparent
            raycastImage.raycastTarget = true;
        }
        else
        {
            // Ensure existing graphic can receive raycasts
            graphic.raycastTarget = true;
        }
        
        // Auto-find components if not assigned
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                // Try to find Background child
                Transform bgTransform = transform.Find("Background");
                if (bgTransform != null)
                {
                    backgroundImage = bgTransform.GetComponent<Image>();
                }
            }
        }
        
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }
            else
            {
                // Try to find any Image that's not the background
                Image[] images = GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img != backgroundImage && !img.name.Contains("Background"))
                    {
                        iconImage = img;
                        break;
                    }
                }
            }
        }
        
        if (countText == null)
        {
            Transform textTransform = transform.Find("CountText");
            if (textTransform != null)
            {
                countText = textTransform.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                // Try alternative names
                string[] textNames = { "Value", "Magnitude", "Amount", "Text" };
                foreach (string name in textNames)
                {
                    Transform t = transform.Find(name);
                    if (t != null)
                    {
                        countText = t.GetComponent<TextMeshProUGUI>();
                        if (countText != null) break;
                    }
                }
                
                // Last resort: find any TextMeshProUGUI
                if (countText == null)
                {
                    countText = GetComponentInChildren<TextMeshProUGUI>(true);
                }
            }
        }
        
        // Set up default background if it exists
        if (backgroundImage != null)
        {
            backgroundImage.color = stackBackgroundColor;
        }
    }
    
    /// <summary>
    /// Set the stack type for this icon
    /// </summary>
    public void SetStackType(StackType type)
    {
        stackType = type;
        UpdateIconSprite();
    }
    
    /// <summary>
    /// Update the stack count display
    /// </summary>
    public void UpdateCount(int count)
    {
        currentCount = count;
        
        if (countText != null)
        {
            countText.text = count.ToString();
            StackDisplayManager manager = GetComponentInParent<StackDisplayManager>();
            bool shouldShow = count > 0 || (manager == null || !manager.hideZeroStacks);
            countText.gameObject.SetActive(shouldShow);
            
            // Update text color based on proximity to cap
            UpdateCountTextColor(count);
        }
    }
    
    /// <summary>
    /// Update the count text color based on proximity to cap (10 stacks)
    /// Green at 1, Yellow at 5 (halfway), Red at 10 (cap)
    /// If cap is higher than 10, yellow is at halfway point
    /// </summary>
    private void UpdateCountTextColor(int count)
    {
        if (countText == null) return;
        
        const int defaultCap = 10;
        int cap = defaultCap; // Could be made configurable per stack type in the future
        
        // Calculate color based on proximity to cap
        Color textColor;
        
        if (count <= 0)
        {
            // Default color for zero or negative
            textColor = Color.white;
        }
        else if (count >= cap)
        {
            // At or above cap: Red
            textColor = Color.red;
        }
        else
        {
            // Calculate halfway point (yellow)
            float halfwayPoint = cap / 2f;
            
            if (count <= 1)
            {
                // At 1: Green
                textColor = Color.green;
            }
            else if (count >= halfwayPoint)
            {
                // Between halfway and cap: Interpolate from Yellow to Red
                float t = (count - halfwayPoint) / (cap - halfwayPoint);
                textColor = Color.Lerp(Color.yellow, Color.red, t);
            }
            else
            {
                // Between 1 and halfway: Interpolate from Green to Yellow
                float t = (count - 1f) / (halfwayPoint - 1f);
                textColor = Color.Lerp(Color.green, Color.yellow, t);
            }
        }
        
        countText.color = textColor;
    }
    
    /// <summary>
    /// Get the current stack type
    /// </summary>
    public StackType GetStackType()
    {
        return stackType;
    }
    
    /// <summary>
    /// Get the current stack count
    /// </summary>
    public int GetCount()
    {
        return currentCount;
    }
    
    /// <summary>
    /// Update the icon sprite based on stack type
    /// </summary>
    private void UpdateIconSprite()
    {
        if (iconImage == null) return;
        
        // Try to load sprite from Resources
        Sprite stackSprite = LoadStackSprite(stackType);
        if (stackSprite != null)
        {
            iconImage.sprite = stackSprite;
            iconImage.color = Color.white; // Reset to white so sprite colors show
        }
        else
        {
            // Fallback: use colored square based on stack type
            iconImage.color = GetStackColor(stackType);
        }
    }
    
    /// <summary>
    /// Load sprite for a stack type from Resources
    /// </summary>
    private Sprite LoadStackSprite(StackType stackType)
    {
        // Try to load from Resources folder
        Sprite sprite = Resources.Load<Sprite>($"UI/Stacks/{stackType}");
        if (sprite == null)
        {
            // Try alternative path
            sprite = Resources.Load<Sprite>($"Stacks/{stackType}");
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Get color for a stack type (fallback when sprite is missing)
    /// </summary>
    private Color GetStackColor(StackType stackType)
    {
        switch (stackType)
        {
            case StackType.Momentum:
                return new Color(1f, 0.8f, 0f); // Orange-yellow
            case StackType.Agitate:
                return new Color(1f, 0.3f, 0.3f); // Red
            case StackType.Tolerance:
                return new Color(0.3f, 0.8f, 1f); // Light blue
            case StackType.Potential:
                return new Color(0.8f, 0.3f, 1f); // Purple
            default:
                return defaultIconColor;
        }
    }
    
    /// <summary>
    /// Setup this icon for a specific stack type and count
    /// </summary>
    public void SetupStack(StackType type, int count)
    {
        SetStackType(type);
        UpdateCount(count);
        // Color is updated in UpdateCount
    }
    
    /// <summary>
    /// Get tooltip description for a stack type
    /// </summary>
    private string GetStackTooltipDescription(StackType type)
    {
        switch (type)
        {
            case StackType.Momentum:
                return "Momentum Charges:\nIf you have momentum threshold higher or equal to the required effect when playing the card, it will play that effect.\n\nMomentum Spenders:\nIf you spend Momentum, the highest threshold will always be triggered.";
            
            case StackType.Agitate:
                return "Agitate:\n+2% more damage per stack (multiplicative).\n+2% attack speed, cast speed, and move speed per stack.\nMaximum stacks: 10";
            
            case StackType.Tolerance:
                return "Tolerance:\n-3% damage taken per stack (multiplicative reduction).\nMaximum stacks: 10";
            
            case StackType.Potential:
                return "Potential:\n+2% critical strike chance per stack.\n+2% critical strike multiplier per stack (multiplicative).\nMaximum stacks: 10";
            
            default:
                return $"{type}:\nA combat resource that can be accumulated and spent for various effects.";
        }
    }
    
    /// <summary>
    /// Get tooltip title for a stack type
    /// </summary>
    private string GetStackTooltipTitle(StackType type)
    {
        return $"{type} ({currentCount})";
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enableTooltips) return;
        isHovering = true;
        ShowTooltip();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enableTooltips) return;
        isHovering = false;
        HideTooltip();
    }
    
    private void Update()
    {
        // Update tooltip position to follow mouse
        if (tooltipInstance != null && tooltipRect != null)
        {
            Vector2 mousePos = Input.mousePosition;
            tooltipRect.position = new Vector3(mousePos.x + 20f, mousePos.y - 20f, 0f);
        }
    }
    
    private void OnDisable()
    {
        if (isHovering)
        {
            isHovering = false;
            HideTooltip();
        }
    }
    
    /// <summary>
    /// Show tooltip for this stack icon
    /// </summary>
    private void ShowTooltip()
    {
        if (tooltipInstance != null) return;
        
        // Try to use CardHoverTooltip component if available
        CardHoverTooltip hoverTooltip = GetComponent<CardHoverTooltip>();
        if (hoverTooltip != null)
        {
            string[] lines = {
                GetStackTooltipTitle(stackType),
                GetStackTooltipDescription(stackType)
            };
            hoverTooltip.Show(lines);
            return;
        }
        
        // Otherwise, create a simple tooltip
        CreateSimpleTooltip();
    }
    
    /// <summary>
    /// Hide tooltip for this stack icon
    /// </summary>
    private void HideTooltip()
    {
        // Try CardHoverTooltip first
        CardHoverTooltip hoverTooltip = GetComponent<CardHoverTooltip>();
        if (hoverTooltip != null)
        {
            hoverTooltip.Hide();
            return;
        }
        
        // Destroy simple tooltip
        if (tooltipInstance != null)
        {
            Destroy(tooltipInstance);
            tooltipInstance = null;
            tooltipRect = null;
        }
    }
    
    /// <summary>
    /// Create a simple tooltip GameObject
    /// </summary>
    private void CreateSimpleTooltip()
    {
        // Find canvas to parent tooltip to
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        if (canvas == null)
        {
            Debug.LogWarning("[StackIcon] No Canvas found for tooltip!");
            return;
        }
        
        // Create tooltip GameObject
        tooltipInstance = new GameObject("StackTooltip");
        tooltipInstance.transform.SetParent(canvas.transform, false);
        
        tooltipRect = tooltipInstance.AddComponent<RectTransform>();
        tooltipRect.sizeDelta = new Vector2(300f, 150f);
        
        // Position tooltip near mouse/cursor (will be updated in Update())
        Vector2 mousePos = Input.mousePosition;
        tooltipRect.position = new Vector3(mousePos.x + 20f, mousePos.y - 20f, 0f);
        
        // Add background
        Image bg = tooltipInstance.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        
        // Add outline
        Outline outline = tooltipInstance.AddComponent<Outline>();
        outline.effectColor = new Color(0.8f, 0.6f, 0.2f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(tooltipInstance.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 10f);
        textRect.offsetMax = new Vector2(-10f, -10f);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = $"<b>{GetStackTooltipTitle(stackType)}</b>\n\n{GetStackTooltipDescription(stackType)}";
        text.fontSize = 14f;
        text.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        text.textWrappingMode = TextWrappingModes.Normal;
        text.alignment = TextAlignmentOptions.TopLeft;
        
        // Add outline to text for readability
        text.outlineWidth = 2f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.8f);
    }
}

