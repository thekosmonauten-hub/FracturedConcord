using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyTooltipUI : MonoBehaviour
{
    private static EnemyTooltipUI instance;
    public static EnemyTooltipUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<EnemyTooltipUI>();
                if (instance == null)
                {
                    var go = new GameObject("EnemyTooltipUI");
                    instance = go.AddComponent<EnemyTooltipUI>();
                }
            }
            return instance;
        }
    }

    [Header("Layout")]
    public Vector2 screenOffset = new Vector2(180f, 40f);
    public Vector2 padding = new Vector2(12f, 10f);

    [Header("Styling")]
    public Color backgroundColor = new Color(0.08f, 0.1f, 0.14f, 0.95f);
    public Color textColor = new Color(0.95f, 0.95f, 0.95f, 1f);
    public float fontSize = 15f;

    private RectTransform root;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI text;
    private Canvas hostCanvas;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureUI();
    }

    private void EnsureUI()
    {
        if (root != null)
            return;

        hostCanvas = FindFirstObjectByType<Canvas>();
        if (hostCanvas == null)
            return;

        var go = new GameObject("EnemyTooltip", typeof(RectTransform));
        go.transform.SetParent(hostCanvas.transform, false);
        root = go.GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0f, 1f);

        var bg = go.AddComponent<Image>();
        bg.color = backgroundColor;

        canvasGroup = go.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(go.transform, false);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(padding.x, padding.y);
        textRect.offsetMax = new Vector2(-padding.x, -padding.y);

        text = textGO.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = textColor;
        text.enableWordWrapping = true;
    }

    public void Show(EnemyCombatDisplay display)
    {
        if (display == null)
            return;

        EnsureUI();
        if (root == null || hostCanvas == null)
            return;

        text.text = BuildText(display);
        canvasGroup.alpha = 1f;

        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, display.transform.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            hostCanvas.transform as RectTransform,
            screenPos,
            hostCanvas.worldCamera,
            out Vector2 localPoint))
        {
            root.anchoredPosition = localPoint + screenOffset;
        }
    }

    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private string BuildText(EnemyCombatDisplay display)
    {
        Enemy enemy = display.GetEnemy();
        EnemyData data = display.GetEnemyData();
        if (enemy == null)
            return "Enemy";

        var sb = new StringBuilder();
        sb.AppendLine(enemy.enemyName);
        sb.AppendLine($"HP: {enemy.currentHealth}/{enemy.maxHealth}  Guard: {enemy.currentGuard:F0}/{enemy.maxGuard:F0}");
        sb.AppendLine($"Damage: {enemy.GetAttackDamage()}");

        if (data != null)
        {
            sb.AppendLine($"Resist: Phys {data.physicalResistance:F0}% | Fire {data.fireResistance:F0}% | Cold {data.coldResistance:F0}% | Lightning {data.lightningResistance:F0}% | Chaos {data.chaosResistance:F0}%");
        }

        AppendThreat(sb, "Threat", enemy.primaryThreat);
        AppendThreat(sb, "Secondary", enemy.secondaryThreat);

        return sb.ToString();
    }

    private void AppendThreat(StringBuilder sb, string label, ThreatWord word)
    {
        if (word == ThreatWord.None)
            return;

        var def = ThreatBehaviorTable.Get(word);
        if (def != null && !string.IsNullOrWhiteSpace(def.EffectSummary))
            sb.AppendLine($"{label}: {word} - {def.EffectSummary}");
        else
            sb.AppendLine($"{label}: {word}");
    }
}
