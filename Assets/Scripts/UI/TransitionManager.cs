using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public Image transitionFrame;
    public float transitionDuration = 1.5f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Transition Frame")]
    public Sprite transitionFrameSprite;

    [Header("Background Image")]
    public Sprite backgroundSprite; // Add this new field
    
    private static TransitionManager _instance;
    public static TransitionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TransitionManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("TransitionManager");
                    _instance = go.AddComponent<TransitionManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SetupTransitionUI();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupTransitionUI()
    {
        // Create Canvas for transition overlay
        GameObject canvasGO = new GameObject("TransitionCanvas");
        canvasGO.transform.SetParent(transform);
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Ensure it's on top
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Create background panel (black overlay)
        GameObject backgroundGO = new GameObject("TransitionBackground");
        backgroundGO.transform.SetParent(canvasGO.transform);
        
        Image backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = Color.black;
        
        RectTransform backgroundRect = backgroundGO.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        
        // Create background image that fills the entire canvas
        GameObject backgroundImageGO = new GameObject("BackgroundImage");
        backgroundImageGO.transform.SetParent(canvasGO.transform);
        
        Image bgImage = backgroundImageGO.AddComponent<Image>();
        if (transitionFrameSprite != null)
        {
            bgImage.sprite = transitionFrameSprite; // Use the same sprite as transition frame
        }
        bgImage.color = Color.white;
        bgImage.preserveAspect = false; // Allow it to stretch to fill
        bgImage.type = Image.Type.Sliced; // Or use Simple if you prefer
        
        RectTransform bgImageRect = backgroundImageGO.GetComponent<RectTransform>();
        bgImageRect.anchorMin = Vector2.zero;
        bgImageRect.anchorMax = Vector2.one;
        bgImageRect.offsetMin = Vector2.zero;
        bgImageRect.offsetMax = Vector2.zero;
        
        // Create transition frame (on top of background)
        GameObject frameGO = new GameObject("TransitionFrame");
        frameGO.transform.SetParent(canvasGO.transform);
        
        transitionFrame = frameGO.AddComponent<Image>();
        if (transitionFrameSprite != null)
        {
            transitionFrame.sprite = transitionFrameSprite;
        }
        transitionFrame.color = Color.white;
        transitionFrame.preserveAspect = true;
        
        RectTransform frameRect = frameGO.GetComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;  // Anchor to all corners
        frameRect.anchorMax = Vector2.one;   // Anchor to all corners
        frameRect.offsetMin = Vector2.zero;  // No offset
        frameRect.offsetMax = Vector2.zero;  // No offset
        
        // Initially hide the transition
        canvasGO.SetActive(false);
    }
    
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName));
    }
    
    private IEnumerator TransitionCoroutine(string sceneName)
    {
        // Show transition UI
        transform.GetChild(0).gameObject.SetActive(true);
        
        // Fade in the transition frame
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = transform.GetChild(0).gameObject.AddComponent<CanvasGroup>();
        }
        
        // Fade in
        while (elapsedTime < transitionDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (transitionDuration * 0.5f);
            float curveValue = fadeCurve.Evaluate(progress);
            
            canvasGroup.alpha = curveValue;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        
        // Load the new scene
        SceneManager.LoadScene(sceneName);
        
        // Wait a frame to ensure scene is loaded
        yield return null;
        
        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < transitionDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (transitionDuration * 0.5f);
            float curveValue = fadeCurve.Evaluate(1f - progress);
            
            canvasGroup.alpha = curveValue;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        
        // Hide transition UI
        transform.GetChild(0).gameObject.SetActive(false);
    }
    
    // Alternative transition with scale effect
    public void TransitionToSceneWithScale(string sceneName)
    {
        StartCoroutine(TransitionWithScaleCoroutine(sceneName));
    }
    
    private IEnumerator TransitionWithScaleCoroutine(string sceneName)
    {
        // Show transition UI
        GameObject transitionCanvas = transform.GetChild(0).gameObject;
        transitionCanvas.SetActive(true);
        
        CanvasGroup canvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = transitionCanvas.AddComponent<CanvasGroup>();
        }
        
        RectTransform frameRect = transitionFrame.GetComponent<RectTransform>();
        
        // Scale in effect
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (transitionDuration * 0.5f);
            float curveValue = fadeCurve.Evaluate(progress);
            
            canvasGroup.alpha = curveValue;
            frameRect.localScale = Vector3.one * (0.5f + curveValue * 0.5f);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        frameRect.localScale = Vector3.one;
        
        // Load the new scene
        SceneManager.LoadScene(sceneName);
        
        // Wait a frame to ensure scene is loaded
        yield return null;
        
        // Scale out effect
        elapsedTime = 0f;
        while (elapsedTime < transitionDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (transitionDuration * 0.5f);
            float curveValue = fadeCurve.Evaluate(1f - progress);
            
            canvasGroup.alpha = curveValue;
            frameRect.localScale = Vector3.one * (0.5f + curveValue * 0.5f);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        frameRect.localScale = Vector3.one * 0.5f;
        
        // Hide transition UI
        transitionCanvas.SetActive(false);
    }

    // Comment out or remove these methods for now:
    /*
    public void TransitionToSceneWithZoomOut(string sceneName)
    {
        StartCoroutine(TransitionWithZoomOutCoroutine(sceneName));
    }

    private IEnumerator TransitionWithZoomOutCoroutine(string sceneName)
    {
        // Show transition UI
        GameObject transitionCanvas = transform.GetChild(0).gameObject;
        transitionCanvas.SetActive(true);
        
        CanvasGroup canvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = transitionCanvas.AddComponent<CanvasGroup>();
        }
        
        // Get the transition frame and background image
        RectTransform frameRect = transitionFrame.GetComponent<RectTransform>();
        Image backgroundImage = transitionCanvas.transform.Find("BackgroundImage")?.GetComponent<Image>();
        
        // Set initial state - frame starts invisible, background visible
        canvasGroup.alpha = 0f;
        frameRect.localScale = Vector3.one * 1.2f; // Start slightly zoomed in
        
        // Make background image visible immediately
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
        }
        
        // Phase 1: Fade in the transition frame
        float elapsedTime = 0f;
        float fadeInDuration = transitionDuration * 0.3f; // 30% of total time for fade in
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeInDuration;
            float curveValue = fadeCurve.Evaluate(progress);
            
            canvasGroup.alpha = curveValue;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        
        // Phase 2: Zoom out effect
        elapsedTime = 0f;
        float zoomDuration = transitionDuration * 0.4f; // 40% of total time for zoom
        float startScale = 1.2f;
        float endScale = 0.8f;
        
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / zoomDuration;
            float curveValue = fadeCurve.Evaluate(progress);
            
            // Zoom out effect
            float currentScale = Mathf.Lerp(startScale, endScale, curveValue);
            frameRect.localScale = Vector3.one * currentScale;
            
            yield return null;
        }
        
        frameRect.localScale = Vector3.one * endScale;
        
        // Load the new scene
        SceneManager.LoadScene(sceneName);
        
        // Wait a frame to ensure scene is loaded
        yield return null;
        
        // Phase 3: Fade out
        elapsedTime = 0f;
        float fadeOutDuration = transitionDuration * 0.3f; // 30% of total time for fade out
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeOutDuration;
            float curveValue = fadeCurve.Evaluate(1f - progress);
            
            canvasGroup.alpha = curveValue;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        
        // Reset scale
        frameRect.localScale = Vector3.one;
        
        // Hide transition UI
        transitionCanvas.SetActive(false);
    }

    // Alternative method that zooms the background instead of the frame
    public void TransitionToSceneWithBackgroundZoom(string sceneName, Image backgroundImage)
    {
        StartCoroutine(TransitionWithBackgroundZoomCoroutine(sceneName, backgroundImage));
    }

    private IEnumerator TransitionWithBackgroundZoomCoroutine(string sceneName, Image backgroundImage)
    {
        if (backgroundImage == null)
        {
            // Fallback to regular transition
            TransitionToScene(sceneName);
            yield break;
        }
        
        RectTransform backgroundRect = backgroundImage.GetComponent<RectTransform>();
        
        // Phase 1: Zoom out the current background
        float elapsedTime = 0f;
        float zoomOutDuration = transitionDuration * 0.5f;
        Vector3 startScale = backgroundRect.localScale;
        Vector3 endScale = startScale * 0.8f; // Zoom out to 80%
        
        while (elapsedTime < zoomOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / zoomOutDuration;
            float curveValue = fadeCurve.Evaluate(progress);
            
            backgroundRect.localScale = Vector3.Lerp(startScale, endScale, curveValue);
            yield return null;
        }
        
        backgroundRect.localScale = endScale;
        
        // Load the new scene
        SceneManager.LoadScene(sceneName);
        
        // Wait a frame to ensure scene is loaded
        yield return null;
        
        // Phase 2: Zoom in the new background (if it exists)
        // This would need to be handled in the new scene's Start method
    }

    public void TransitionToSceneWithCinematicZoom(string sceneName)
    {
        StartCoroutine(CinematicZoomTransitionCoroutine(sceneName));
    }

    private IEnumerator CinematicZoomTransitionCoroutine(string sceneName)
    {
        GameObject transitionCanvas = transform.GetChild(0).gameObject;
        transitionCanvas.SetActive(true);
        
        CanvasGroup canvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = transitionCanvas.AddComponent<CanvasGroup>();
        }
        
        RectTransform frameRect = transitionFrame.GetComponent<RectTransform>();
        
        // Initial state
        canvasGroup.alpha = 0f;
        frameRect.localScale = Vector3.one * 1.5f; // Start more zoomed in
        
        // Phase 1: Quick fade in
        float elapsedTime = 0f;
        float fadeInDuration = transitionDuration * 0.2f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeInDuration;
            canvasGroup.alpha = progress;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        
        // Phase 2: Smooth zoom out
        elapsedTime = 0f;
        float zoomDuration = transitionDuration * 0.6f;
        float startScale = 1.5f;
        float endScale = 0.7f;
        
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / zoomDuration;
            
            // Use a custom curve for more natural zoom
            float curveValue = Mathf.SmoothStep(0f, 1f, progress);
            float currentScale = Mathf.Lerp(startScale, endScale, curveValue);
            
            frameRect.localScale = Vector3.one * currentScale;
            yield return null;
        }
        
        frameRect.localScale = Vector3.one * endScale;
        
        // Load scene
        SceneManager.LoadScene(sceneName);
        yield return null;
        
        // Phase 3: Quick fade out
        elapsedTime = 0f;
        float fadeOutDuration = transitionDuration * 0.2f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeOutDuration;
            canvasGroup.alpha = 1f - progress;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        frameRect.localScale = Vector3.one;
        transitionCanvas.SetActive(false);
    }
    */
}
