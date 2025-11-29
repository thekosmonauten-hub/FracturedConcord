using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class WarrantNodeView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private CanvasGroup lockOverlay;

    public string NodeId { get; private set; }
    public WarrantNodeType NodeType { get; private set; }
    public RectTransform RectTransform { get; private set; }
    public Button Button { get; private set; }
    protected Image BackgroundImage => background;
    public WarrantBoardRuntimeGraph.Node RuntimeNode { get; private set; }
    
    private bool isLocked = true;
    private Color baseColor;
    private const float LockedAlpha = 0.4f;
    private const float LockedSaturation = 0.3f;

    protected virtual void Awake()
    {
        CacheComponents();
    }

    public void BindRuntimeNode(WarrantBoardRuntimeGraph.Node node)
    {
        RuntimeNode = node;
    }

    public virtual void Initialize(string nodeId, WarrantNodeType type, Color color)
    {
        CacheComponents();
        NodeId = nodeId;
        NodeType = type;
        baseColor = color;
        background.color = color;
        
        // Anchor is always unlocked
        if (nodeId == "Anchor")
        {
            SetLocked(false);
        }
        else
        {
            SetLocked(true); // All other nodes start locked
        }
    }

    /// <summary>
    /// Updates the visual locked/unlocked state of the node.
    /// </summary>
	public virtual void SetLocked(bool locked)
    {
        isLocked = locked;
        
        if (background != null)
        {
            if (locked)
            {
                // Grey out and desaturate locked nodes
                var greyColor = baseColor;
                greyColor.r *= LockedSaturation;
                greyColor.g *= LockedSaturation;
                greyColor.b *= LockedSaturation;
                greyColor.a *= LockedAlpha;
                background.color = greyColor;
            }
            else
            {
                background.color = baseColor;
            }
        }
        
        // Show/hide lock icon
        if (lockIcon != null)
        {
            lockIcon.SetActive(locked);
        }
        
        // Adjust overlay alpha
        if (lockOverlay != null)
        {
            lockOverlay.alpha = locked ? 0.6f : 0f;
            lockOverlay.blocksRaycasts = false; // Don't block clicks
        }
        
        // Disable button interactions on locked nodes (except for unlock attempts)
        if (Button != null)
        {
            // Button stays enabled so unlock clicks can work
            // But visual feedback shows locked state
        }
    }

    public bool IsLocked => isLocked;

    protected void CacheComponents()
    {
        if (RectTransform == null)
        {
            RectTransform = GetComponent<RectTransform>();
        }

        if (background == null)
        {
            background = GetComponent<Image>();
            if (background == null)
            {
                background = gameObject.AddComponent<Image>();
            }
        }

        if (Button == null)
        {
            Button = GetComponent<Button>();
            if (Button == null)
            {
                Button = gameObject.AddComponent<Button>();
            }
        }
    }
}



