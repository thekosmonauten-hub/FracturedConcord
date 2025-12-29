using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the loading screen UI shown during bootstrap initialization.
/// Displays progress bar and status messages while the game initializes.
/// </summary>
public class LoadingScreenManager : MonoBehaviour
{
    private static LoadingScreenManager _instance;
    public static LoadingScreenManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<LoadingScreenManager>();
            }
            return _instance;
        }
    }
    
    [Header("UI References")]
    [Tooltip("Root canvas for loading screen (should be on Bootstrap scene)")]
    [SerializeField] private Canvas loadingCanvas;
    
    [Tooltip("Loading screen panel")]
    [SerializeField] private GameObject loadingPanel;
    
    [Tooltip("Progress bar fill image")]
    [SerializeField] private Image progressBarFill;
    
    [Tooltip("Progress text (e.g., 'Loading... 45%')")]
    [SerializeField] private TextMeshProUGUI progressText;
    
    [Tooltip("Status text (e.g., 'Loading assets...')")]
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Tooltip("Loading icon/spinner (optional)")]
    [SerializeField] private GameObject loadingSpinner;
    
    [Header("Settings")]
    [Tooltip("Show loading screen on start")]
    [SerializeField] private bool showOnStart = true;
    
    [Tooltip("Smooth progress bar animation speed")]
    [SerializeField] private float progressSmoothSpeed = 2f;
    
    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private bool isVisible = false;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Setup if not assigned
        if (loadingCanvas == null)
        {
            loadingCanvas = GetComponentInChildren<Canvas>();
        }
        
        if (loadingPanel == null && loadingCanvas != null)
        {
            loadingPanel = loadingCanvas.transform.Find("LoadingPanel")?.gameObject;
        }
        
        // Hide by default
        SetVisible(false);
    }
    
    private void Start()
    {
        if (showOnStart)
        {
            Show();
        }
    }
    
    private void Update()
    {
        // Smooth progress bar animation
        if (Mathf.Abs(currentProgress - targetProgress) > 0.01f)
        {
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * progressSmoothSpeed);
            UpdateProgressBar();
        }
    }
    
    /// <summary>
    /// Show the loading screen
    /// </summary>
    public void Show()
    {
        isVisible = true;
        if (loadingCanvas != null)
        {
            loadingCanvas.gameObject.SetActive(true);
        }
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(true);
        }
        
        SetProgress(0f);
        SetStatus("Initializing...");
    }
    
    /// <summary>
    /// Hide the loading screen
    /// </summary>
    public void Hide()
    {
        if (!isVisible)
            return; // Already hidden
            
        isVisible = false;
        SetProgress(1f);
        
        // Fade out animation
        StartCoroutine(FadeOutCoroutine());
    }
    
    /// <summary>
    /// Hide the loading screen and wait for it to complete (coroutine-friendly)
    /// </summary>
    public IEnumerator HideAndWait()
    {
        if (!isVisible)
            yield break; // Already hidden
            
        isVisible = false;
        SetProgress(1f);
        
        // Wait for progress bar to reach 100% (smooth animation)
        while (currentProgress < 0.99f)
        {
            yield return null;
        }
        
        // Ensure it's exactly 100%
        currentProgress = 1f;
        UpdateProgressBar();
        yield return null;
        
        // Now fade out
        yield return StartCoroutine(FadeOutCoroutine());
    }
    
    /// <summary>
    /// Check if loading screen is currently visible
    /// </summary>
    public bool IsVisible => isVisible;
    
    /// <summary>
    /// Get current progress (0-1)
    /// </summary>
    public float CurrentProgress => currentProgress;
    
    /// <summary>
    /// Set loading progress (0-1)
    /// </summary>
    public void SetProgress(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
    }
    
    /// <summary>
    /// Set status message
    /// </summary>
    public void SetStatus(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
    }
    
    /// <summary>
    /// Update progress bar visual
    /// </summary>
    private void UpdateProgressBar()
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = currentProgress;
        }
        
        if (progressText != null)
        {
            progressText.text = $"Loading... {Mathf.RoundToInt(currentProgress * 100)}%";
        }
    }
    
    /// <summary>
    /// Fade out the loading screen
    /// </summary>
    private IEnumerator FadeOutCoroutine()
    {
        // Wait for progress to reach 100%
        while (currentProgress < 0.99f)
        {
            yield return null;
        }
        
        // Fade out
        CanvasGroup canvasGroup = loadingCanvas?.GetComponent<CanvasGroup>();
        if (canvasGroup == null && loadingCanvas != null)
        {
            canvasGroup = loadingCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        if (canvasGroup != null)
        {
            float fadeTime = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / fadeTime);
                yield return null;
            }
        }
        
        // Hide completely
        if (loadingCanvas != null)
        {
            loadingCanvas.gameObject.SetActive(false);
        }
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(false);
        }
    }
    
    /// <summary>
    /// Set visibility (internal)
    /// </summary>
    private void SetVisible(bool visible)
    {
        if (loadingCanvas != null)
        {
            loadingCanvas.gameObject.SetActive(visible);
        }
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(visible);
        }
    }
}

