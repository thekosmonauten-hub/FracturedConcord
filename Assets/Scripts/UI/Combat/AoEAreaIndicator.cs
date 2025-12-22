using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Predefined area marker for AoE (Area of Effect) effects.
/// Effects with "Effect Type: Area" will play at this GameObject's position.
/// Attach this to a GameObject in the CombatScene where Area effects should appear.
/// </summary>
public class AoEAreaIndicator : MonoBehaviour
{
    [Header("Positioning")]
    [Tooltip("Target area to cover (usually enemy area container). If not assigned, will auto-find.")]
    [SerializeField] private RectTransform targetArea;
    
    [Tooltip("If true, automatically position at center of target area. If false, use manual positioning.")]
    [SerializeField] private bool autoPosition = true;
    
    [Header("Auto-Find Settings")]
    [Tooltip("Auto-find enemy area container if not assigned")]
    [SerializeField] private bool autoFindEnemyArea = true;
    
    [Tooltip("Names to search for enemy area container")]
    [SerializeField] private string[] enemyAreaNames = { "EnemyArea", "EnemyContainer", "EnemyRow", "Enemies" };
    
    private RectTransform rectTransform;
    
    private void Awake()
    {
        // Get RectTransform
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("[AoEAreaIndicator] GameObject must have a RectTransform component!");
            return;
        }
        
        // Auto-find target area if needed
        if (targetArea == null && autoFindEnemyArea)
        {
            FindEnemyArea();
        }
        
        // Position at the center of the target area (if auto-positioning is enabled)
        if (autoPosition)
        {
            UpdatePosition();
        }
        else
        {
            Debug.Log($"[AoEAreaIndicator] Auto-positioning disabled. Using manual position: {rectTransform.anchoredPosition}");
        }
    }
    
    /// <summary>
    /// Find the enemy area container automatically
    /// </summary>
    private void FindEnemyArea()
    {
        foreach (string name in enemyAreaNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                targetArea = found.GetComponent<RectTransform>();
                if (targetArea != null)
                {
                    Debug.Log($"[AoEAreaIndicator] Auto-found enemy area: {name}");
                    return;
                }
            }
        }
        
        // Fallback: try to find EnemyCombatDisplay parent
        EnemyCombatDisplay[] displays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        if (displays.Length > 0 && displays[0].transform.parent != null)
        {
            targetArea = displays[0].transform.parent.GetComponent<RectTransform>();
            if (targetArea != null)
            {
                Debug.Log($"[AoEAreaIndicator] Auto-found enemy area from EnemyCombatDisplay parent");
            }
        }
        
        if (targetArea == null)
        {
            Debug.LogWarning("[AoEAreaIndicator] Could not auto-find enemy area. Please assign targetArea manually in Inspector.");
        }
    }
    
    /// <summary>
    /// Get the position where Area effects should play
    /// </summary>
    public Vector3 GetEffectPosition()
    {
        if (rectTransform != null)
        {
            return rectTransform.position;
        }
        return transform.position;
    }
    
    /// <summary>
    /// Get the RectTransform for positioning effects
    /// </summary>
    public RectTransform GetRectTransform()
    {
        return rectTransform;
    }
    
    /// <summary>
    /// Update the indicator position to center on the target area or active enemies
    /// </summary>
    public void UpdatePosition()
    {
        if (rectTransform == null)
        {
            Debug.LogError("[AoEAreaIndicator] GameObject needs RectTransform!");
            return;
        }
        
        // Get the canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[AoEAreaIndicator] Cannot find parent Canvas! Please ensure AoEAreaIndicator is a child of a Canvas.");
            return;
        }
        
        // Try to calculate center from active enemy displays first (more reliable)
        EnemyCombatDisplay[] activeDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        List<EnemyCombatDisplay> activeEnemies = new System.Collections.Generic.List<EnemyCombatDisplay>();
        foreach (var display in activeDisplays)
        {
            if (display != null && display.gameObject.activeInHierarchy)
            {
                var enemy = display.GetCurrentEnemy();
                if (enemy != null && enemy.currentHealth > 0)
                {
                    activeEnemies.Add(display);
                }
            }
        }
        
        if (activeEnemies.Count > 0)
        {
            // Calculate center from active enemy positions
            Vector3 totalPos = Vector3.zero;
            foreach (var display in activeEnemies)
            {
                RectTransform enemyRect = display.GetComponent<RectTransform>();
                if (enemyRect != null)
                {
                    totalPos += enemyRect.position;
                }
            }
            Vector3 averageWorldPos = totalPos / activeEnemies.Count;
            
            // Convert to canvas local space
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera ?? Camera.main, averageWorldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPoint, canvas.worldCamera, out Vector2 localCenter);
            
            rectTransform.anchoredPosition = localCenter;
            
            Debug.Log($"[AoEAreaIndicator] Positioned at center of {activeEnemies.Count} active enemies. World center: {averageWorldPos}, Canvas local: {localCenter}");
            return;
        }
        
        // Fallback: use target area if no active enemies
        if (targetArea == null)
        {
            if (autoFindEnemyArea)
            {
                FindEnemyArea();
            }
            
            if (targetArea == null)
            {
                Debug.LogWarning("[AoEAreaIndicator] Cannot update position: No active enemies found and targetArea is null! Please position manually or assign targetArea in Inspector.");
                return;
            }
        }
        
        // Use GetWorldCorners to get the actual center of the target area
        Vector3[] corners = new Vector3[4];
        targetArea.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;
        
        // Convert world center to canvas local space
        RectTransform canvasRect2 = canvas.GetComponent<RectTransform>();
        Vector2 screenPoint2 = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera ?? Camera.main, worldCenter);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect2, screenPoint2, canvas.worldCamera, out Vector2 localCenter2);
        
        rectTransform.anchoredPosition = localCenter2;
        
        Debug.Log($"[AoEAreaIndicator] Positioned at center of target area '{targetArea.name}'. World center: {worldCenter}, Canvas local: {localCenter2}");
    }
    
    /// <summary>
    /// Set the target area manually
    /// </summary>
    public void SetTargetArea(RectTransform area)
    {
        targetArea = area;
        UpdatePosition();
    }
}

