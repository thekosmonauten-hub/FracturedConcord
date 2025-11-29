using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EncounterConnectionsRenderer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform connectionContainer;
    [SerializeField] private Image connectionPrefab;

    [Header("Appearance")]
    [SerializeField] private float lineThickness = 6f;
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
    [SerializeField] private Color unlockedColor = Color.white;

    private readonly List<Image> activeConnections = new List<Image>();
    private EncounterManager encounterManager;

    private void OnEnable()
    {
        encounterManager = EncounterManager.Instance;
        if (encounterManager != null)
        {
            encounterManager.EncounterGraphChanged += HandleGraphChanged;
        }
        RefreshConnections();
    }

    private void OnDisable()
    {
        if (encounterManager != null)
        {
            encounterManager.EncounterGraphChanged -= HandleGraphChanged;
        }
        ClearConnections();
    }

    private void HandleGraphChanged()
    {
        RefreshConnections();
    }

    public void RefreshConnections()
    {
        ClearConnections();

        if (connectionContainer == null || connectionPrefab == null || encounterManager == null)
            return;

        EncounterButton[] buttons = GetComponentsInChildren<EncounterButton>(true);
        if (buttons == null || buttons.Length == 0)
            return;

        Dictionary<int, EncounterButton> buttonLookup = new Dictionary<int, EncounterButton>();
        foreach (var button in buttons)
        {
            if (button != null && button.encounterID > 0 && !buttonLookup.ContainsKey(button.encounterID))
            {
                buttonLookup.Add(button.encounterID, button);
            }
        }

        foreach (var button in buttons)
        {
            if (button == null || button.encounterID <= 0)
                continue;

            var prerequisites = encounterManager.GetPrerequisitesForEncounter(button.encounterID);
            if (prerequisites == null)
                continue;

            foreach (int prereqId in prerequisites)
            {
                if (!buttonLookup.TryGetValue(prereqId, out var prereqButton))
                    continue;

                CreateConnection(prereqButton, button);
            }
        }
    }

    private void CreateConnection(EncounterButton from, EncounterButton to)
    {
        if (from == null || to == null)
            return;

        RectTransform fromRect = from.transform as RectTransform;
        RectTransform toRect = to.transform as RectTransform;
        if (fromRect == null || toRect == null)
            return;

        Image connection = Instantiate(connectionPrefab, connectionContainer);
        RectTransform lineRect = connection.rectTransform;

        Vector3 startWorld = fromRect.TransformPoint(fromRect.rect.center);
        Vector3 endWorld = toRect.TransformPoint(toRect.rect.center);

        Vector3 start = connectionContainer.InverseTransformPoint(startWorld);
        Vector3 end = connectionContainer.InverseTransformPoint(endWorld);
        Vector3 direction = end - start;

        float length = direction.magnitude;
        lineRect.sizeDelta = new Vector2(length, lineThickness);
        lineRect.anchoredPosition = (start + end) * 0.5f;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0f, 0f, angle);

        bool unlocked = encounterManager.IsEncounterUnlocked(to.encounterID);
        connection.color = unlocked ? unlockedColor : lockedColor;

        activeConnections.Add(connection);
    }

    private void ClearConnections()
    {
        foreach (var connection in activeConnections)
        {
            if (connection != null)
            {
                Destroy(connection.gameObject);
            }
        }
        activeConnections.Clear();
    }
}

















