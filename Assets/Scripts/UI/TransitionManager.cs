using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public Image transitionFrame;
    public float transitionDuration = 1.5f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Optional curve controlling curtain motion. Defaults to fadeCurve when null.")]
    public AnimationCurve curtainCurve = null;
    
    [Header("Transition Frame")]
    public Sprite transitionFrameSprite;
    
    [Header("Background Image")]
    public Sprite backgroundSprite; // Add this new field
    
    private GameObject transitionCanvas;
    private CanvasGroup transitionCanvasGroup;
    private Image backgroundImageOverlay;
    private RectTransform topCurtainRect;
    private RectTransform bottomCurtainRect;
    private RectTransform leftCurtainRect;
    private RectTransform rightCurtainRect;
    
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
        
        transitionCanvas = canvasGO;
        transitionCanvasGroup = canvasGO.GetComponent<CanvasGroup>();
        if (transitionCanvasGroup == null)
        {
            transitionCanvasGroup = canvasGO.AddComponent<CanvasGroup>();
        }
        transitionCanvasGroup.alpha = 0f;
        
        // Create background image that fills the entire canvas
        GameObject backgroundImageGO = new GameObject("BackgroundImage");
        backgroundImageGO.transform.SetParent(canvasGO.transform);
        
        Image bgImage = backgroundImageGO.AddComponent<Image>();
        if (transitionFrameSprite != null)
        {
            bgImage.sprite = transitionFrameSprite; // Use the same sprite as transition frame
        }
        bgImage.color = Color.black;
        bgImage.preserveAspect = false; // Allow it to stretch to fill
        bgImage.type = Image.Type.Sliced; // Or use Simple if you prefer
        backgroundImageOverlay = bgImage;
        
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
        
        // Create curtains for optional transitions
        GameObject topCurtainGO = new GameObject("TopCurtain");
        topCurtainGO.transform.SetParent(canvasGO.transform);
        Image topCurtainImage = topCurtainGO.AddComponent<Image>();
        topCurtainImage.color = Color.black;
        topCurtainRect = topCurtainGO.GetComponent<RectTransform>();
        topCurtainRect.anchorMin = new Vector2(0f, 1f);
        topCurtainRect.anchorMax = new Vector2(1f, 1f);
        topCurtainRect.pivot = new Vector2(0.5f, 1f);
        topCurtainRect.offsetMin = Vector2.zero;
        topCurtainRect.offsetMax = Vector2.zero;
        topCurtainGO.SetActive(false);

        GameObject bottomCurtainGO = new GameObject("BottomCurtain");
        bottomCurtainGO.transform.SetParent(canvasGO.transform);
        Image bottomCurtainImage = bottomCurtainGO.AddComponent<Image>();
        bottomCurtainImage.color = Color.black;
        bottomCurtainRect = bottomCurtainGO.GetComponent<RectTransform>();
        bottomCurtainRect.anchorMin = new Vector2(0f, 0f);
        bottomCurtainRect.anchorMax = new Vector2(1f, 0f);
        bottomCurtainRect.pivot = new Vector2(0.5f, 0f);
        bottomCurtainRect.offsetMin = Vector2.zero;
        bottomCurtainRect.offsetMax = Vector2.zero;
        bottomCurtainGO.SetActive(false);

        GameObject leftCurtainGO = new GameObject("LeftCurtain");
        leftCurtainGO.transform.SetParent(canvasGO.transform);
        Image leftCurtainImage = leftCurtainGO.AddComponent<Image>();
        leftCurtainImage.color = Color.black;
        leftCurtainRect = leftCurtainGO.GetComponent<RectTransform>();
        leftCurtainRect.anchorMin = new Vector2(0f, 0f);
        leftCurtainRect.anchorMax = new Vector2(0f, 1f);
        leftCurtainRect.pivot = new Vector2(0f, 0.5f);
        leftCurtainRect.offsetMin = Vector2.zero;
        leftCurtainRect.offsetMax = Vector2.zero;
        leftCurtainGO.SetActive(false);
        
        GameObject rightCurtainGO = new GameObject("RightCurtain");
        rightCurtainGO.transform.SetParent(canvasGO.transform);
        Image rightCurtainImage = rightCurtainGO.AddComponent<Image>();
        rightCurtainImage.color = Color.black;
        rightCurtainRect = rightCurtainGO.GetComponent<RectTransform>();
        rightCurtainRect.anchorMin = new Vector2(1f, 0f);
        rightCurtainRect.anchorMax = new Vector2(1f, 1f);
        rightCurtainRect.pivot = new Vector2(1f, 0.5f);
        rightCurtainRect.offsetMin = Vector2.zero;
        rightCurtainRect.offsetMax = Vector2.zero;
        rightCurtainGO.SetActive(false);
        
        // Initially hide the transition
        canvasGO.SetActive(false);
    }
    
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName));
    }
    
    private IEnumerator TransitionCoroutine(string sceneName)
    {
        if (transitionCanvas == null)
        {
            SetupTransitionUI();
        }
        
        transitionCanvas.SetActive(true);
        if (transitionCanvasGroup == null)
        {
            transitionCanvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
            if (transitionCanvasGroup == null)
                transitionCanvasGroup = transitionCanvas.AddComponent<CanvasGroup>();
        }
        transitionCanvasGroup.alpha = 0f;
    if (leftCurtainRect != null) leftCurtainRect.gameObject.SetActive(false);
    if (rightCurtainRect != null) rightCurtainRect.gameObject.SetActive(false);
        if (backgroundImageOverlay != null) backgroundImageOverlay.enabled = true;
        
        // Fade in the transition frame
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (transitionDuration * 0.5f);
            float curveValue = fadeCurve.Evaluate(progress);
            if (curtainCurve != null)
            {
                curveValue = curtainCurve.Evaluate(progress);
            }
            
            transitionCanvasGroup.alpha = curveValue;
            yield return null;
        }
        
        transitionCanvasGroup.alpha = 1f;
        
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
            if (curtainCurve != null)
            {
                curveValue = curtainCurve.Evaluate(1f - progress);
            }
            
            transitionCanvasGroup.alpha = curveValue;
            yield return null;
        }
        
        transitionCanvasGroup.alpha = 0f;
        
        // Hide transition UI
        transitionCanvas.SetActive(false);
    }
    
    // Alternative transition with scale effect
    public void TransitionToSceneWithScale(string sceneName)
    {
        StartCoroutine(TransitionWithScaleCoroutine(sceneName));
    }
    
    private IEnumerator TransitionWithScaleCoroutine(string sceneName)
    {
        // Show transition UI
        if (transitionCanvas == null)
        {
            SetupTransitionUI();
        }
        
        transitionCanvas.SetActive(true);
        if (transitionCanvasGroup == null)
        {
            transitionCanvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
            if (transitionCanvasGroup == null)
                transitionCanvasGroup = transitionCanvas.AddComponent<CanvasGroup>();
        }
        transitionCanvasGroup.alpha = 0f;
        if (topCurtainRect != null) topCurtainRect.gameObject.SetActive(false);
        if (bottomCurtainRect != null) bottomCurtainRect.gameObject.SetActive(false);
        if (leftCurtainRect != null) leftCurtainRect.gameObject.SetActive(false);
        if (rightCurtainRect != null) rightCurtainRect.gameObject.SetActive(false);
        if (topCurtainRect != null) topCurtainRect.gameObject.SetActive(false);
        if (bottomCurtainRect != null) bottomCurtainRect.gameObject.SetActive(false);
        if (leftCurtainRect != null) leftCurtainRect.gameObject.SetActive(false);
        if (rightCurtainRect != null) rightCurtainRect.gameObject.SetActive(false);
        if (backgroundImageOverlay != null) backgroundImageOverlay.enabled = true;
        
        RectTransform frameRect = transitionFrame.GetComponent<RectTransform>();
        
        // Scale in effect
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (transitionDuration * 0.5f);
            float curveValue = fadeCurve.Evaluate(progress);
            if (curtainCurve != null)
            {
                curveValue = curtainCurve.Evaluate(progress);
            }
            
            transitionCanvasGroup.alpha = curveValue;
            frameRect.localScale = Vector3.one * (0.5f + curveValue * 0.5f);
            yield return null;
        }
        
        transitionCanvasGroup.alpha = 1f;
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
            if (curtainCurve != null)
            {
                curveValue = curtainCurve.Evaluate(1f - progress);
            }
            
            transitionCanvasGroup.alpha = curveValue;
            frameRect.localScale = Vector3.one * (0.5f + curveValue * 0.5f);
            yield return null;
        }
        
        transitionCanvasGroup.alpha = 0f;
        frameRect.localScale = Vector3.one * 0.5f;
        
        // Hide transition UI
        transitionCanvas.SetActive(false);
    }

    public void TransitionToSceneWithCurtain(string sceneName)
    {
        StartCoroutine(TransitionCurtainCoroutine(sceneName));
    }

    private IEnumerator TransitionCurtainCoroutine(string sceneName)
    {
        if (transitionCanvas == null)
        {
            SetupTransitionUI();
        }

        bool frameWasActive = transitionFrame != null && transitionFrame.gameObject.activeSelf;
        if (transitionFrame != null)
        {
            transitionFrame.gameObject.SetActive(false);
        }

        transitionCanvas.SetActive(true);
        if (transitionCanvasGroup == null)
        {
            transitionCanvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
            if (transitionCanvasGroup == null)
                transitionCanvasGroup = transitionCanvas.AddComponent<CanvasGroup>();
        }
        transitionCanvasGroup.alpha = 0f;

        RectTransform canvasRect = transitionCanvas.GetComponent<RectTransform>();
        if (backgroundImageOverlay != null)
        {
            backgroundImageOverlay.enabled = true;
            Color overlayColor = backgroundImageOverlay.color;
            overlayColor.a = 0f;
            backgroundImageOverlay.color = overlayColor;
        }

        ActivateCurtains(true);

        float closeDuration = transitionDuration * 0.5f;
        Sequence closeSequence = DOTween.Sequence();
        closeSequence.Append(transitionCanvasGroup.DOFade(1f, closeDuration).SetEase(fadeCurve));

        if (topCurtainRect != null)
        {
            topCurtainRect.anchorMin = new Vector2(0f, 1f);
            topCurtainRect.anchorMax = new Vector2(1f, 1f);
            closeSequence.Join(topCurtainRect.DOAnchorMin(new Vector2(0f, 0.5f), closeDuration).SetEase(fadeCurve));
            closeSequence.Join(topCurtainRect.DOAnchorMax(new Vector2(1f, 0.5f), closeDuration).SetEase(fadeCurve));
        }
        if (bottomCurtainRect != null)
        {
            bottomCurtainRect.anchorMin = new Vector2(0f, 0f);
            bottomCurtainRect.anchorMax = new Vector2(1f, 0f);
            closeSequence.Join(bottomCurtainRect.DOAnchorMin(new Vector2(0f, 0.5f), closeDuration).SetEase(fadeCurve));
            closeSequence.Join(bottomCurtainRect.DOAnchorMax(new Vector2(1f, 0.5f), closeDuration).SetEase(fadeCurve));
        }
        if (leftCurtainRect != null)
        {
            leftCurtainRect.anchorMin = new Vector2(0f, 0f);
            leftCurtainRect.anchorMax = new Vector2(0f, 1f);
            closeSequence.Join(leftCurtainRect.DOAnchorMin(new Vector2(0.5f, 0f), closeDuration).SetEase(fadeCurve));
            closeSequence.Join(leftCurtainRect.DOAnchorMax(new Vector2(0.5f, 1f), closeDuration).SetEase(fadeCurve));
        }
        if (rightCurtainRect != null)
        {
            rightCurtainRect.anchorMin = new Vector2(1f, 0f);
            rightCurtainRect.anchorMax = new Vector2(1f, 1f);
            closeSequence.Join(rightCurtainRect.DOAnchorMin(new Vector2(0.5f, 0f), closeDuration).SetEase(fadeCurve));
            closeSequence.Join(rightCurtainRect.DOAnchorMax(new Vector2(0.5f, 1f), closeDuration).SetEase(fadeCurve));
        }
        if (backgroundImageOverlay != null)
        {
            closeSequence.Join(backgroundImageOverlay.DOFade(1f, closeDuration).SetEase(fadeCurve));
        }

        yield return closeSequence.WaitForCompletion();

        SceneManager.LoadScene(sceneName);
        yield return null;

        float openDuration = transitionDuration * 0.5f;
        Sequence openSequence = DOTween.Sequence();
        openSequence.Append(transitionCanvasGroup.DOFade(0f, openDuration).SetEase(fadeCurve));

        if (topCurtainRect != null)
        {
            openSequence.Join(topCurtainRect.DOAnchorMin(new Vector2(0f, 1f), openDuration).SetEase(fadeCurve));
            openSequence.Join(topCurtainRect.DOAnchorMax(new Vector2(1f, 1f), openDuration).SetEase(fadeCurve));
        }
        if (bottomCurtainRect != null)
        {
            openSequence.Join(bottomCurtainRect.DOAnchorMin(new Vector2(0f, 0f), openDuration).SetEase(fadeCurve));
            openSequence.Join(bottomCurtainRect.DOAnchorMax(new Vector2(1f, 0f), openDuration).SetEase(fadeCurve));
        }
        if (leftCurtainRect != null)
        {
            openSequence.Join(leftCurtainRect.DOAnchorMin(new Vector2(0f, 0f), openDuration).SetEase(fadeCurve));
            openSequence.Join(leftCurtainRect.DOAnchorMax(new Vector2(0f, 1f), openDuration).SetEase(fadeCurve));
        }
        if (rightCurtainRect != null)
        {
            openSequence.Join(rightCurtainRect.DOAnchorMin(new Vector2(1f, 0f), openDuration).SetEase(fadeCurve));
            openSequence.Join(rightCurtainRect.DOAnchorMax(new Vector2(1f, 1f), openDuration).SetEase(fadeCurve));
        }
        if (backgroundImageOverlay != null)
        {
            openSequence.Join(backgroundImageOverlay.DOFade(0f, openDuration).SetEase(fadeCurve));
        }

        yield return openSequence.WaitForCompletion();

        ActivateCurtains(false);
        if (backgroundImageOverlay != null)
        {
            backgroundImageOverlay.enabled = false;
        }
        transitionCanvas.SetActive(false);

        if (transitionFrame != null)
        {
            transitionFrame.gameObject.SetActive(frameWasActive);
        }
    }

    private void ActivateCurtains(bool active)
    {
        if (topCurtainRect != null) topCurtainRect.gameObject.SetActive(active);
        if (bottomCurtainRect != null) bottomCurtainRect.gameObject.SetActive(active);
        if (leftCurtainRect != null) leftCurtainRect.gameObject.SetActive(active);
        if (rightCurtainRect != null) rightCurtainRect.gameObject.SetActive(active);
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
