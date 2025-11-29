using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the Warrant Locker sliding panel UI. Handles panel visibility, slide animations,
/// and integration with the warrant inventory system.
/// </summary>
public class WarrantLockerPanelManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject lockerPanel;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    
    [Header("Toggle Button")]
    [SerializeField] private Button toggleButton;
    
    [Header("Panel State")]
    [SerializeField] private bool isPanelVisible = false;
    
    [Header("Slide Settings")]
    [SerializeField] private bool useSlideAnimation = true;
    [SerializeField] private float slideDuration = 0.25f;
    [SerializeField] private AnimationCurve slideEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float offscreenPadding = 40f;
    [SerializeField] private SlideDirection slideDirection = SlideDirection.Left;
    
    [Header("Toggle Protection")]
    [SerializeField] private float toggleCooldown = 0.1f;
    
    private Vector2 visibleAnchoredPos;
    private Vector2 hiddenAnchoredPos;
    private float lastToggleTime = 0f;
    private bool isToggling = false;
    
    public bool IsPanelVisible => isPanelVisible;
    
    private enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }
    
    private void Awake()
    {
        InitializePanel();
    }
    
    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePanel);
        }
    }
    
    private void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(TogglePanel);
        }
    }
    
    private void InitializePanel()
    {
        if (lockerPanel == null)
        {
            Debug.LogWarning("[WarrantLockerPanelManager] Locker panel is not assigned!");
            return;
        }
        
        if (panelRect == null)
        {
            panelRect = lockerPanel.GetComponent<RectTransform>();
        }
        
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = lockerPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = lockerPanel.AddComponent<CanvasGroup>();
            }
        }
        
        if (panelRect != null)
        {
            visibleAnchoredPos = panelRect.anchoredPosition;
            CalculateHiddenPosition();
            
            // Start hidden
            panelRect.anchoredPosition = hiddenAnchoredPos;
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.alpha = 0f;
            lockerPanel.SetActive(false);
            isPanelVisible = false;
        }
    }
    
    private void CalculateHiddenPosition()
    {
        if (panelRect == null) return;
        
        float width = panelRect.rect.width;
        float height = panelRect.rect.height;
        
        switch (slideDirection)
        {
            case SlideDirection.Left:
                hiddenAnchoredPos = visibleAnchoredPos - new Vector2(width + offscreenPadding, 0f);
                break;
            case SlideDirection.Right:
                hiddenAnchoredPos = visibleAnchoredPos + new Vector2(width + offscreenPadding, 0f);
                break;
            case SlideDirection.Top:
                hiddenAnchoredPos = visibleAnchoredPos + new Vector2(0f, height + offscreenPadding);
                break;
            case SlideDirection.Bottom:
                hiddenAnchoredPos = visibleAnchoredPos - new Vector2(0f, height + offscreenPadding);
                break;
        }
    }
    
    public void TogglePanel()
    {
        if (Time.time - lastToggleTime < toggleCooldown || isToggling)
        {
            return;
        }
        
        isToggling = true;
        lastToggleTime = Time.time;
        
        if (lockerPanel == null)
        {
            Debug.LogWarning("[WarrantLockerPanelManager] Locker panel is not assigned!");
            isToggling = false;
            return;
        }
        
        isPanelVisible = !isPanelVisible;
        
        if (!useSlideAnimation || panelRect == null || panelCanvasGroup == null)
        {
            lockerPanel.SetActive(isPanelVisible);
            panelCanvasGroup.blocksRaycasts = isPanelVisible;
            panelCanvasGroup.interactable = isPanelVisible;
            panelCanvasGroup.alpha = isPanelVisible ? 1f : 0f;
        }
        else
        {
            Vector2 target = isPanelVisible ? visibleAnchoredPos : hiddenAnchoredPos;
            
            if (isPanelVisible)
            {
                lockerPanel.SetActive(true);
                panelRect.anchoredPosition = hiddenAnchoredPos;
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.blocksRaycasts = true;
                panelCanvasGroup.interactable = true;
            }
            
            LeanTween.cancel(lockerPanel);
            LeanTween.value(lockerPanel, panelRect.anchoredPosition, target, slideDuration)
                .setOnUpdate((Vector2 v) => panelRect.anchoredPosition = v)
                .setEase(slideEase);
            
            LeanTween.value(lockerPanel, panelCanvasGroup.alpha, isPanelVisible ? 1f : 0f, slideDuration)
                .setOnUpdate((float a) => panelCanvasGroup.alpha = a)
                .setEase(slideEase)
                .setOnComplete(() =>
                {
                    if (!isPanelVisible)
                    {
                        panelCanvasGroup.blocksRaycasts = false;
                        panelCanvasGroup.interactable = false;
                        lockerPanel.SetActive(false);
                    }
                    isToggling = false;
                });
        }
        
        if (!useSlideAnimation)
        {
            isToggling = false;
        }
    }
    
    public void ShowPanel()
    {
        if (!isPanelVisible)
        {
            TogglePanel();
        }
    }
    
    public void HidePanel()
    {
        if (isPanelVisible)
        {
            TogglePanel();
        }
    }
}

