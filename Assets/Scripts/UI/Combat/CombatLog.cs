using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Enhanced combat log with scrollable entries and hoverable loot tooltips
/// </summary>
public class CombatLog : MonoBehaviour
{
    private static CombatLog _instance;
    public static CombatLog Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CombatLog>();
            }
            return _instance;
        }
    }

    [Header("UI References")]
    [Tooltip("Scroll view for log entries")]
    public ScrollRect scrollView;
    
    [Tooltip("Content container for log entries")]
    public Transform logContent;
    
    [Tooltip("Prefab for log entries")]
    public GameObject logEntryPrefab;
    
    [Header("Settings")]
    [Tooltip("Maximum number of log entries to keep")]
    public int maxEntries = 50;
    
    [Tooltip("Auto-scroll to bottom when new entry added")]
    public bool autoScroll = true;
    
    private List<GameObject> logEntries = new List<GameObject>();
    
    private void Awake()
    {
        _instance = this;
    }
    
    /// <summary>
    /// Add a simple text message to the log
    /// </summary>
    public void AddMessage(string message, Color? textColor = null)
    {
        CreateLogEntry(message, null, textColor);
    }
    
    /// <summary>
    /// Add a loot drop message to the log (hoverable)
    /// </summary>
    public void AddLootDrop(string enemyName, LootReward loot)
    {
        if (loot == null)
            return;
        
        // Format message based on loot type
        string message = FormatLootMessage(enemyName, loot);
        Color messageColor = GetLootColor(loot);
        
        CreateLogEntry(message, loot, messageColor);
    }
    
    /// <summary>
    /// Add multiple loot drops from same enemy
    /// </summary>
    public void AddEnemyLootDrops(string enemyName, List<LootReward> loots)
    {
        if (loots == null || loots.Count == 0)
            return;
        
        foreach (var loot in loots)
        {
            AddLootDrop(enemyName, loot);
        }
    }
    
    private void CreateLogEntry(string message, LootReward loot = null, Color? textColor = null)
    {
        if (logContent == null)
        {
            Debug.LogWarning("[CombatLog] Log content not assigned!");
            return;
        }
        
        GameObject entry;
        
        if (logEntryPrefab != null)
        {
            // Use prefab
            entry = Instantiate(logEntryPrefab, logContent);
        }
        else
        {
            // Create simple text entry
            entry = CreateSimpleEntry();
        }
        
        // Setup the entry
        CombatLogEntry logEntryComponent = entry.GetComponent<CombatLogEntry>();
        if (logEntryComponent == null)
        {
            logEntryComponent = entry.AddComponent<CombatLogEntry>();
        }
        
        logEntryComponent.Initialize(message, loot);
        
        // Set text color
        if (textColor.HasValue)
        {
            TextMeshProUGUI textComponent = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.color = textColor.Value;
            }
        }
        
        // Add to list
        logEntries.Add(entry);
        
        // Enforce max entries
        while (logEntries.Count > maxEntries)
        {
            GameObject oldEntry = logEntries[0];
            logEntries.RemoveAt(0);
            Destroy(oldEntry);
        }
        
        // Auto-scroll to bottom
        if (autoScroll && scrollView != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollView.verticalNormalizedPosition = 0f;
        }
    }
    
    private GameObject CreateSimpleEntry()
    {
        GameObject entry = new GameObject("LogEntry");
        entry.transform.SetParent(logContent);
        entry.transform.localScale = Vector3.one;
        
        // Add RectTransform
        RectTransform rt = entry.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 30);
        
        // Add background
        Image bg = entry.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(entry.transform);
        textObj.transform.localScale = Vector3.one;
        
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 14;
        text.alignment = TextAlignmentOptions.Left;
        text.margin = new Vector4(5, 5, 5, 5);
        
        return entry;
    }
    
    private string FormatLootMessage(string enemyName, LootReward loot)
    {
        switch (loot.rewardType)
        {
            case RewardType.Currency:
                if (loot.currencyAmount > 1)
                    return $"{enemyName} dropped [{loot.currencyType}] x{loot.currencyAmount}";
                else
                    return $"{enemyName} dropped [{loot.currencyType}]";
                    
            case RewardType.Item:
                return $"{enemyName} dropped [{loot.itemData?.itemName ?? "Unknown Item"}]";
                
            case RewardType.Experience:
                return $"{enemyName} granted {loot.experienceAmount} experience";
                
            case RewardType.Card:
                return $"{enemyName} dropped [{loot.cardName}]";
                
            default:
                return $"{enemyName} dropped something";
        }
    }
    
    private Color GetLootColor(LootReward loot)
    {
        switch (loot.rewardType)
        {
            case RewardType.Currency:
                return new Color(1f, 0.84f, 0f); // Gold
            case RewardType.Item:
                return new Color(0.8f, 0.8f, 1f); // Light blue
            case RewardType.Experience:
                return new Color(0.5f, 1f, 0.5f); // Light green
            case RewardType.Card:
                return new Color(0.9f, 0.7f, 1f); // Purple
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Clear all log entries
    /// </summary>
    public void ClearLog()
    {
        foreach (var entry in logEntries)
        {
            if (entry != null)
                Destroy(entry);
        }
        logEntries.Clear();
    }
}













