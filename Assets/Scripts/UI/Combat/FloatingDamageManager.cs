using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages floating damage number spawning and pooling
/// </summary>
public class FloatingDamageManager : MonoBehaviour
{
    public static FloatingDamageManager Instance { get; private set; }
    
    [Header("Prefab")]
    [Tooltip("Prefab for floating damage text")]
    public GameObject floatingDamagePrefab;
    
    [Header("Spawn Settings")]
    [Tooltip("Parent transform for damage numbers (should be on top Canvas)")]
    public Transform damageNumberContainer;
    
    [Tooltip("Offset from target position (y-axis)")]
    public float spawnOffset = 50f;
    
    [Header("Pooling")]
    [Tooltip("Enable object pooling")]
    public bool usePooling = true;
    
    [Tooltip("Initial pool size")]
    public int poolSize = 20;
    
    private Queue<FloatingDamageText> damageTextPool = new Queue<FloatingDamageText>();
    private List<FloatingDamageText> activeDamageTexts = new List<FloatingDamageText>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        if (usePooling)
        {
            InitializePool();
        }
    }
    
    private void InitializePool()
    {
        if (floatingDamagePrefab == null)
        {
            Debug.LogError("[FloatingDamageManager] Floating damage prefab not assigned!");
            return;
        }
        
        if (damageNumberContainer == null)
        {
            Debug.LogWarning("[FloatingDamageManager] Damage number container not assigned. Using this transform.");
            damageNumberContainer = transform;
        }
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(floatingDamagePrefab, damageNumberContainer);
            obj.name = $"FloatingDamage_{i}";
            obj.SetActive(false);
            
            FloatingDamageText damageText = obj.GetComponent<FloatingDamageText>();
            if (damageText != null)
            {
                damageTextPool.Enqueue(damageText);
            }
            else
            {
                Debug.LogError("[FloatingDamageManager] Prefab missing FloatingDamageText component!");
                Destroy(obj);
            }
        }
        
        Debug.Log($"[FloatingDamageManager] Initialized pool with {damageTextPool.Count} damage texts");
    }
    
    /// <summary>
    /// Spawn floating damage number at target position
    /// </summary>
    public void ShowDamage(float damage, bool isCritical, Transform target)
    {
        if (target == null) return;
        
        Vector3 spawnPosition = target.position + new Vector3(0, spawnOffset, 0);
        ShowDamageAtPosition(damage, isCritical, spawnPosition);
    }
    
    /// <summary>
    /// Spawn floating damage number at specific world position
    /// </summary>
    public void ShowDamageAtPosition(float damage, bool isCritical, Vector3 worldPosition)
    {
        FloatingDamageText damageText = GetDamageText();
        
        if (damageText == null)
        {
            Debug.LogWarning("[FloatingDamageManager] Failed to get damage text from pool!");
            return;
        }
        
        damageText.gameObject.SetActive(true);
        activeDamageTexts.Add(damageText);
        
        damageText.Show(damage, isCritical, worldPosition, () => ReturnToPool(damageText));
    }
    
    /// <summary>
    /// Spawn floating heal number at target position
    /// </summary>
    public void ShowHeal(float healAmount, Transform target)
    {
        if (target == null) return;
        
        Vector3 spawnPosition = target.position + new Vector3(0, spawnOffset, 0);
        
        FloatingDamageText damageText = GetDamageText();
        
        if (damageText == null)
        {
            Debug.LogWarning("[FloatingDamageManager] Failed to get damage text from pool!");
            return;
        }
        
        damageText.gameObject.SetActive(true);
        activeDamageTexts.Add(damageText);
        
        damageText.ShowHeal(healAmount, spawnPosition, () => ReturnToPool(damageText));
    }
    
    /// <summary>
    /// Get damage text from pool or create new
    /// </summary>
    private FloatingDamageText GetDamageText()
    {
        // Try to get from pool
        if (usePooling && damageTextPool.Count > 0)
        {
            return damageTextPool.Dequeue();
        }
        
        // Create new instance
        if (floatingDamagePrefab == null)
        {
            Debug.LogError("[FloatingDamageManager] Cannot create damage text - prefab is null!");
            return null;
        }
        
        GameObject obj = Instantiate(floatingDamagePrefab, damageNumberContainer);
        FloatingDamageText damageText = obj.GetComponent<FloatingDamageText>();
        
        if (damageText == null)
        {
            Debug.LogError("[FloatingDamageManager] Prefab missing FloatingDamageText component!");
            Destroy(obj);
            return null;
        }
        
        return damageText;
    }
    
    /// <summary>
    /// Return damage text to pool
    /// </summary>
    private void ReturnToPool(FloatingDamageText damageText)
    {
        if (damageText == null) return;
        
        activeDamageTexts.Remove(damageText);
        
        if (usePooling)
        {
            damageText.gameObject.SetActive(false);
            damageTextPool.Enqueue(damageText);
        }
        else
        {
            Destroy(damageText.gameObject);
        }
    }
    
    /// <summary>
    /// Clear all active damage texts immediately
    /// </summary>
    public void ClearAll()
    {
        List<FloatingDamageText> textsToReturn = new List<FloatingDamageText>(activeDamageTexts);
        
        foreach (var text in textsToReturn)
        {
            if (text != null)
            {
                text.Hide();
                ReturnToPool(text);
            }
        }
        
        activeDamageTexts.Clear();
    }
    
    private void OnValidate()
    {
        if (floatingDamagePrefab == null)
        {
            Debug.LogWarning("[FloatingDamageManager] Floating damage prefab not assigned!");
        }
    }
}












