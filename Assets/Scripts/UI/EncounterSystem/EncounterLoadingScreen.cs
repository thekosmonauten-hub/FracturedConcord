using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncounterLoadingScreen : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;
    public Slider progressSlider;
    public TextMeshProUGUI statusText;

    [Header("Display")]
    public bool autoCreateIfMissing = true;
    public float fadeDuration = 0.15f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        EnsureUI();
        _ = EncounterManager.Instance;
        HideImmediate();
    }

    private void OnEnable()
    {
        EncounterEvents.OnEncounterSystemLoading += HandleLoading;
        EncounterEvents.OnEncounterSystemInitialized += HandleLoaded;
        var manager = EncounterManager.Instance;
        if (manager != null && manager.IsInitialized)
            HideImmediate();
        else
            ShowImmediate();

        if (manager != null && !manager.IsInitialized)
            manager.EnsureInitialized();
    }

    private void OnDisable()
    {
        EncounterEvents.OnEncounterSystemLoading -= HandleLoading;
        EncounterEvents.OnEncounterSystemInitialized -= HandleLoaded;
    }

    private void HandleLoading(float progress, string message)
    {
        ShowImmediate();
        if (progressSlider != null)
            progressSlider.value = Mathf.Clamp01(progress);
        if (statusText != null)
            statusText.text = message;
    }

    private void HandleLoaded()
    {
        Hide();
    }

    private void EnsureUI()
    {
        if (panelRoot != null && progressSlider != null && statusText != null)
        {
            EnsureCanvasGroup();
            return;
        }

        if (!autoCreateIfMissing)
            return;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        panelRoot = new GameObject("EncounterLoadingPanel", typeof(RectTransform));
        panelRoot.transform.SetParent(canvas.transform, false);
        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var image = panelRoot.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.6f);

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(panelRoot.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(420f, 140f);

        var textGO = new GameObject("StatusText", typeof(RectTransform));
        textGO.transform.SetParent(content.transform, false);
        statusText = textGO.AddComponent<TextMeshProUGUI>();
        statusText.text = "Loading...";
        statusText.fontSize = 20f;
        statusText.alignment = TextAlignmentOptions.Center;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.6f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var sliderGO = new GameObject("Progress", typeof(RectTransform));
        sliderGO.transform.SetParent(content.transform, false);
        progressSlider = sliderGO.AddComponent<Slider>();
        var sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.1f, 0f);
        sliderRect.anchorMax = new Vector2(0.9f, 0.4f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        var bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(sliderGO.transform, false);
        var bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(1f, 1f, 1f, 0.15f);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(sliderGO.transform, false);
        var fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.8f, 1f, 0.9f);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        progressSlider.targetGraphic = fillImage;
        progressSlider.fillRect = fillRect;
        progressSlider.direction = Slider.Direction.LeftToRight;
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.value = 0f;

        EnsureCanvasGroup();
    }

    private void EnsureCanvasGroup()
    {
        if (panelRoot == null)
            return;
        canvasGroup = panelRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panelRoot.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
    }

    private void ShowImmediate()
    {
        if (panelRoot == null)
            return;
        panelRoot.SetActive(true);
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    private void HideImmediate()
    {
        if (panelRoot == null)
            return;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        panelRoot.SetActive(false);
    }

    private void Hide()
    {
        if (panelRoot == null)
            return;
        if (fadeDuration <= 0f || canvasGroup == null)
        {
            HideImmediate();
            return;
        }
        LeanTween.cancel(panelRoot);
        panelRoot.SetActive(true);
        canvasGroup.alpha = 1f;
        LeanTween.value(panelRoot, 1f, 0f, fadeDuration)
            .setOnUpdate(v => canvasGroup.alpha = v)
            .setOnComplete(() => panelRoot.SetActive(false));
    }
}
