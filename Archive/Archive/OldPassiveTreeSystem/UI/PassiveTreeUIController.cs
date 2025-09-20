using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Controller for the passive tree interface
/// Manages buttons and controls for the passive tree
/// </summary>
public class PassiveTreeUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button resetViewButton;
    [SerializeField] private Button fitToViewButton;
    [SerializeField] private Button zoomInButton;
    [SerializeField] private Button zoomOutButton;
    [SerializeField] private TextMeshProUGUI zoomLevelText;
    [SerializeField] private Slider zoomSlider;
    
    [Header("Pan Zoom Reference")]
    [SerializeField] private PassiveTreePanZoom panZoom;
    
    [Header("Settings")]
    [SerializeField] private float zoomIncrement = 0.2f;
    
    private void Start()
    {
        // Find pan zoom if not assigned
        if (panZoom == null)
        {
            panZoom = FindFirstObjectByType<PassiveTreePanZoom>();
        }
        
        // Setup button listeners
        SetupButtonListeners();
        
        // Setup zoom slider
        SetupZoomSlider();
        
        // Update UI
        UpdateZoomUI();
    }
    
    private void Update()
    {
        // Update zoom level text
        if (zoomLevelText != null && panZoom != null)
        {
            UpdateZoomUI();
        }
    }
    
    /// <summary>
    /// Setup button click listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        if (resetViewButton != null)
        {
            resetViewButton.onClick.AddListener(OnResetViewClicked);
        }
        
        if (fitToViewButton != null)
        {
            fitToViewButton.onClick.AddListener(OnFitToViewClicked);
        }
        
        if (zoomInButton != null)
        {
            zoomInButton.onClick.AddListener(OnZoomInClicked);
        }
        
        if (zoomOutButton != null)
        {
            zoomOutButton.onClick.AddListener(OnZoomOutClicked);
        }
    }
    
    /// <summary>
    /// Setup zoom slider
    /// </summary>
    private void SetupZoomSlider()
    {
        if (zoomSlider != null && panZoom != null)
        {
            zoomSlider.minValue = panZoom.MinZoom;
            zoomSlider.maxValue = panZoom.MaxZoom;
            zoomSlider.value = 1f;
            zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
        }
    }
    
    /// <summary>
    /// Update zoom UI elements
    /// </summary>
    private void UpdateZoomUI()
    {
        if (panZoom == null) return;
        
        // Update zoom level text
        if (zoomLevelText != null)
        {
            float currentZoom = panZoom.CurrentZoom;
            zoomLevelText.text = $"{(currentZoom * 100):F0}%";
        }
        
        // Update zoom slider
        if (zoomSlider != null)
        {
            zoomSlider.SetValueWithoutNotify(panZoom.CurrentZoom);
        }
    }
    
    #region Button Event Handlers
    
    private void OnResetViewClicked()
    {
        if (panZoom != null)
        {
            panZoom.ResetView();
            Debug.Log("[PassiveTreeUIController] Reset view clicked");
        }
    }
    
    private void OnFitToViewClicked()
    {
        if (panZoom != null)
        {
            panZoom.FitToView();
            Debug.Log("[PassiveTreeUIController] Fit to view clicked");
        }
    }
    
    private void OnZoomInClicked()
    {
        if (panZoom != null)
        {
            float newZoom = panZoom.CurrentZoom + zoomIncrement;
            panZoom.SetTargetZoom(newZoom);
            Debug.Log($"[PassiveTreeUIController] Zoom in clicked, new zoom: {newZoom}");
        }
    }
    
    private void OnZoomOutClicked()
    {
        if (panZoom != null)
        {
            float newZoom = panZoom.CurrentZoom - zoomIncrement;
            panZoom.SetTargetZoom(newZoom);
            Debug.Log($"[PassiveTreeUIController] Zoom out clicked, new zoom: {newZoom}");
        }
    }
    
    private void OnZoomSliderChanged(float value)
    {
        if (panZoom != null)
        {
            panZoom.SetTargetZoom(value);
        }
    }
    
    #endregion
    
    /// <summary>
    /// Show current pan/zoom state
    /// </summary>
    [ContextMenu("Show Pan/Zoom State")]
    public void ShowPanZoomState()
    {
        if (panZoom != null)
        {
            panZoom.ShowCurrentState();
        }
        else
        {
            Debug.LogWarning("[PassiveTreeUIController] No PanZoom component found!");
        }
    }
}
