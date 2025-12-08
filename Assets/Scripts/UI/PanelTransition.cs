using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Handles smooth transitions when activating/deactivating panels.
/// Can be attached to buttons or called directly to animate panel activation.
/// Supports fade and scale animations.
/// </summary>
[RequireComponent(typeof(Button))]
public class PanelTransition : MonoBehaviour
{
    [Header("Panel Reference")]
    [Tooltip("The panel GameObject to animate when this button is clicked")]
    [SerializeField] private GameObject targetPanel;
    
    [Header("Animation Settings")]
    [Tooltip("Duration of the transition animation in seconds")]
    [SerializeField] private float transitionDuration = 0.3f;
    
    [Tooltip("Enable fade animation (alpha transition)")]
    [SerializeField] private bool useFadeAnimation = true;
    
    [Tooltip("Enable scale animation")]
    [SerializeField] private bool useScaleAnimation = true;
    
    [Tooltip("Starting scale for scale animation (e.g., 0.9 for slight shrink, 1.1 for slight grow)")]
    [SerializeField] private Vector3 startScale = new Vector3(0.95f, 0.95f, 1f);
    
    [Tooltip("Easing type for the animation")]
    [SerializeField] private Ease animationEase = Ease.OutQuad;
    
    [Header("Auto Setup")]
    [Tooltip("If true, will try to find the panel automatically by name")]
    [SerializeField] private bool autoFindPanel = false;
    
    [Tooltip("Panel name to search for if autoFindPanel is true")]
    [SerializeField] private string panelNameToFind = "";
    
    private CanvasGroup canvasGroup;
    private RectTransform panelRectTransform;
    private bool isAnimating = false;
    
    private void Awake()
    {
        // Try to find panel if not assigned
        if (targetPanel == null && autoFindPanel)
        {
            if (!string.IsNullOrEmpty(panelNameToFind))
            {
                targetPanel = GameObject.Find(panelNameToFind);
                if (targetPanel != null)
                {
                    Debug.Log($"[PanelTransition] Auto-found panel: {panelNameToFind}");
                }
            }
        }
        
        // Setup button click handler
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        
        // Initialize panel components
        if (targetPanel != null)
        {
            InitializePanelComponents();
        }
    }
    
    /// <summary>
    /// Initialize CanvasGroup and RectTransform for the target panel
    /// </summary>
    private void InitializePanelComponents()
    {
        if (targetPanel == null) return;
        
        // Get or add CanvasGroup
        canvasGroup = targetPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = targetPanel.AddComponent<CanvasGroup>();
        }
        
        // Get RectTransform for scale animation
        panelRectTransform = targetPanel.GetComponent<RectTransform>();
        
        // Ensure panel starts in correct state if it's active
        if (targetPanel.activeSelf)
        {
            if (useFadeAnimation && canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
            if (useScaleAnimation && panelRectTransform != null)
            {
                panelRectTransform.localScale = Vector3.one;
            }
        }
    }
    
    /// <summary>
    /// Called when the button is clicked
    /// </summary>
    private void OnButtonClicked()
    {
        if (targetPanel == null)
        {
            Debug.LogWarning($"[PanelTransition] Target panel is null on button '{gameObject.name}'");
            return;
        }
        
        // Toggle panel visibility
        bool shouldShow = !targetPanel.activeSelf;
        ShowPanel(shouldShow);
    }
    
    /// <summary>
    /// Show or hide the panel with animation
    /// </summary>
    public void ShowPanel(bool show)
    {
        if (targetPanel == null)
        {
            Debug.LogWarning("[PanelTransition] Cannot show panel - target panel is null");
            return;
        }
        
        if (isAnimating)
        {
            Debug.Log("[PanelTransition] Animation already in progress, ignoring request");
            return;
        }
        
        // Initialize components if not already done
        if (canvasGroup == null || panelRectTransform == null)
        {
            InitializePanelComponents();
        }
        
        if (show)
        {
            StartCoroutine(AnimatePanelIn());
        }
        else
        {
            StartCoroutine(AnimatePanelOut());
        }
    }
    
    /// <summary>
    /// Animate panel fading/scaling in
    /// </summary>
    private IEnumerator AnimatePanelIn()
    {
        isAnimating = true;
        
        // Activate panel first (but keep it invisible/at start scale)
        targetPanel.SetActive(true);
        
        // Ensure CanvasGroup is set up
        if (canvasGroup == null)
        {
            canvasGroup = targetPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = targetPanel.AddComponent<CanvasGroup>();
            }
        }
        
        // Set initial state
        if (useFadeAnimation)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        
        if (useScaleAnimation && panelRectTransform != null)
        {
            panelRectTransform.localScale = startScale;
        }
        
        // Create animation sequence
        Sequence animSequence = DOTween.Sequence();
        
        // Fade in
        if (useFadeAnimation && canvasGroup != null)
        {
            animSequence.Join(
                canvasGroup.DOFade(1f, transitionDuration)
                    .SetEase(animationEase)
            );
        }
        
        // Scale in
        if (useScaleAnimation && panelRectTransform != null)
        {
            animSequence.Join(
                panelRectTransform.DOScale(Vector3.one, transitionDuration)
                    .SetEase(animationEase)
            );
        }
        
        // Wait for animation to complete
        yield return animSequence.WaitForCompletion();
        
        // Ensure final state
        if (useFadeAnimation && canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        if (useScaleAnimation && panelRectTransform != null)
        {
            panelRectTransform.localScale = Vector3.one;
        }
        
        isAnimating = false;
        Debug.Log($"[PanelTransition] Panel '{targetPanel.name}' animated in");
    }
    
    /// <summary>
    /// Animate panel fading/scaling out
    /// </summary>
    private IEnumerator AnimatePanelOut()
    {
        isAnimating = true;
        
        if (canvasGroup == null)
        {
            canvasGroup = targetPanel.GetComponent<CanvasGroup>();
        }
        
        // Create animation sequence
        Sequence animSequence = DOTween.Sequence();
        
        // Fade out
        if (useFadeAnimation && canvasGroup != null)
        {
            animSequence.Join(
                canvasGroup.DOFade(0f, transitionDuration)
                    .SetEase(animationEase)
            );
        }
        
        // Scale out
        if (useScaleAnimation && panelRectTransform != null)
        {
            animSequence.Join(
                panelRectTransform.DOScale(startScale, transitionDuration)
                    .SetEase(animationEase)
            );
        }
        
        // Wait for animation to complete
        yield return animSequence.WaitForCompletion();
        
        // Disable panel after animation
        if (useFadeAnimation && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        
        targetPanel.SetActive(false);
        
        isAnimating = false;
        Debug.Log($"[PanelTransition] Panel '{targetPanel.name}' animated out");
    }
    
    /// <summary>
    /// Set the target panel programmatically
    /// </summary>
    public void SetTargetPanel(GameObject panel)
    {
        targetPanel = panel;
        if (panel != null)
        {
            InitializePanelComponents();
        }
    }
    
    /// <summary>
    /// Get the target panel
    /// </summary>
    public GameObject GetTargetPanel() => targetPanel;
    
    private void OnDestroy()
    {
        // Kill any active animations
        if (targetPanel != null)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
            }
            if (panelRectTransform != null)
            {
                panelRectTransform.DOKill();
            }
        }
    }
}












