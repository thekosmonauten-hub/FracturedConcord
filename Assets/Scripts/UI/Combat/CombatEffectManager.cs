using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages visual effects for combat actions like damage, healing, guard gain, etc.
/// </summary>
public class CombatEffectManager : MonoBehaviour
{
    [Header("Impact Effect Prefabs")]
    [SerializeField] private GameObject damageImpactPrefab;
    [SerializeField] private GameObject healImpactPrefab;
    [SerializeField] private GameObject guardImpactPrefab;
    [SerializeField] private GameObject criticalImpactPrefab;
    [SerializeField] private GameObject statusEffectImpactPrefab;
    
    [Header("Elemental Impact Effects")]
    [SerializeField] private GameObject fireImpactPrefab;
    [SerializeField] private GameObject coldImpactPrefab;
    [SerializeField] private GameObject lightningImpactPrefab;
    [SerializeField] private GameObject chaosImpactPrefab;
    [SerializeField] private GameObject physicalImpactPrefab;
    
    [Header("Effect Settings")]
    [SerializeField] private float effectDuration = 2f;
    [SerializeField] private Vector3 effectOffset = new Vector3(0, 50, 0); // Offset above target
    [SerializeField] private bool autoDestroyEffects = true;
    
    [Header("Effect Pooling")]
    [SerializeField] private int poolSize = 10;
    [SerializeField] private Transform effectPoolParent;
    
    [Header("Canvas Settings")]
    [SerializeField] private Canvas targetCanvas;
    [Tooltip("If true, automatically find and use the main combat canvas")]
    [SerializeField] private bool autoFindCanvas = true;
    
    // Effect pools for performance
    private Queue<GameObject> damageEffectPool = new Queue<GameObject>();
    private Queue<GameObject> healEffectPool = new Queue<GameObject>();
    private Queue<GameObject> guardEffectPool = new Queue<GameObject>();
    private Queue<GameObject> criticalEffectPool = new Queue<GameObject>();
    private Queue<GameObject> statusEffectPool = new Queue<GameObject>();
    
    // Elemental effect pools
    private Dictionary<DamageType, Queue<GameObject>> elementalEffectPools = new Dictionary<DamageType, Queue<GameObject>>();
    private Dictionary<DamageType, GameObject> elementalEffectPrefabs = new Dictionary<DamageType, GameObject>();
    
    // Singleton pattern
    public static CombatEffectManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeEffectPools();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeEffectPools()
    {
        // Create pool parent if not assigned
        if (effectPoolParent == null)
        {
            GameObject poolParent = new GameObject("EffectPool");
            poolParent.transform.SetParent(transform);
            effectPoolParent = poolParent.transform;
        }
        
        // Initialize basic effect pools
        InitializePool(damageEffectPool, damageImpactPrefab, "DamageEffects");
        InitializePool(healEffectPool, healImpactPrefab, "HealEffects");
        InitializePool(guardEffectPool, guardImpactPrefab, "GuardEffects");
        InitializePool(criticalEffectPool, criticalImpactPrefab, "CriticalEffects");
        InitializePool(statusEffectPool, statusEffectImpactPrefab, "StatusEffects");
        
        // Initialize elemental effect pools
        InitializeElementalEffects();
        
        // Find target canvas if auto-find is enabled
        if (autoFindCanvas && targetCanvas == null)
        {
            targetCanvas = FindMainCombatCanvas();
            if (targetCanvas != null)
            {
                Debug.Log($"CombatEffectManager auto-found target canvas: {targetCanvas.name}");
            }
        }
        
        Debug.Log("CombatEffectManager initialized with effect pools");
    }
    
    private void InitializePool(Queue<GameObject> pool, GameObject prefab, string poolName)
    {
        if (prefab == null) return;
        
        GameObject poolContainer = new GameObject(poolName);
        poolContainer.transform.SetParent(effectPoolParent);
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject effect = Instantiate(prefab, poolContainer.transform);
            effect.SetActive(false);
            pool.Enqueue(effect);
        }
        
        Debug.Log($"Initialized {poolName} pool with {poolSize} effects");
    }
    
    private void InitializeElementalEffects()
    {
        // Map damage types to prefabs
        elementalEffectPrefabs[DamageType.Fire] = fireImpactPrefab;
        elementalEffectPrefabs[DamageType.Cold] = coldImpactPrefab;
        elementalEffectPrefabs[DamageType.Lightning] = lightningImpactPrefab;
        elementalEffectPrefabs[DamageType.Chaos] = chaosImpactPrefab;
        elementalEffectPrefabs[DamageType.Physical] = physicalImpactPrefab;
        
        // Initialize pools for each elemental type
        foreach (var kvp in elementalEffectPrefabs)
        {
            if (kvp.Value != null)
            {
                Queue<GameObject> pool = new Queue<GameObject>();
                InitializePool(pool, kvp.Value, $"{kvp.Key}Effects");
                elementalEffectPools[kvp.Key] = pool;
                Debug.Log($"Initialized {kvp.Key} effect pool");
            }
        }
    }
    
    /// <summary>
    /// Play damage impact effect at target position
    /// </summary>
    public void PlayDamageEffect(Vector3 targetPosition, bool isCritical = false)
    {
        GameObject effect = GetEffectFromPool(isCritical ? criticalEffectPool : damageEffectPool, 
                                           isCritical ? criticalImpactPrefab : damageImpactPrefab);
        
        if (effect != null)
        {
            PlayEffect(effect, targetPosition, isCritical ? "Critical Hit!" : "Damage");
        }
    }
    
    /// <summary>
    /// Play damage effect on a specific target (player or enemy)
    /// </summary>
    public void PlayDamageEffectOnTarget(Transform target, bool isCritical = false)
    {
        if (target == null) return;
        
        Vector3 effectPosition = target.position + effectOffset;
        PlayDamageEffect(effectPosition, isCritical);
    }
    
    /// <summary>
    /// Play elemental damage effect based on damage type
    /// </summary>
    public void PlayElementalDamageEffect(Vector3 targetPosition, DamageType damageType, bool isCritical = false)
    {
        GameObject effect = null;
        
        // Try to get elemental effect first
        if (elementalEffectPools.ContainsKey(damageType) && elementalEffectPools[damageType].Count > 0)
        {
            effect = elementalEffectPools[damageType].Dequeue();
        }
        else if (elementalEffectPrefabs.ContainsKey(damageType) && elementalEffectPrefabs[damageType] != null)
        {
            // Create new effect if pool is empty
            effect = Instantiate(elementalEffectPrefabs[damageType], effectPoolParent);
            effect.SetActive(false);
        }
        else
        {
            // Fallback to basic damage effect
            effect = GetEffectFromPool(isCritical ? criticalEffectPool : damageEffectPool, 
                                     isCritical ? criticalImpactPrefab : damageImpactPrefab);
        }
        
        if (effect != null)
        {
            string effectName = isCritical ? $"Critical {damageType} Damage!" : $"{damageType} Damage";
            Debug.Log($"<color=green>Playing {effectName} effect at {targetPosition}</color>");
            Debug.Log($"<color=cyan>Effect GameObject: {effect.name}, Active: {effect.activeInHierarchy}</color>");
            PlayEffect(effect, targetPosition, effectName);
        }
        else
        {
            Debug.LogWarning($"<color=red>No effect prefab found for {damageType} damage type!</color>");
            Debug.Log($"<color=yellow>Available prefabs:</color>");
            Debug.Log($"  Fire: {(fireImpactPrefab != null ? "ASSIGNED" : "NULL")}");
            Debug.Log($"  Cold: {(coldImpactPrefab != null ? "ASSIGNED" : "NULL")}");
            Debug.Log($"  Lightning: {(lightningImpactPrefab != null ? "ASSIGNED" : "NULL")}");
            Debug.Log($"  Chaos: {(chaosImpactPrefab != null ? "ASSIGNED" : "NULL")}");
            Debug.Log($"  Physical: {(physicalImpactPrefab != null ? "ASSIGNED" : "NULL")}");
        }
    }
    
    /// <summary>
    /// Play elemental damage effect on a specific target
    /// </summary>
    public void PlayElementalDamageEffectOnTarget(Transform target, DamageType damageType, bool isCritical = false)
    {
        if (target == null) return;
        
        Vector3 effectPosition = target.position + effectOffset;
        PlayElementalDamageEffect(effectPosition, damageType, isCritical);
    }
    
    /// <summary>
    /// Play heal impact effect at target position
    /// </summary>
    public void PlayHealEffect(Vector3 targetPosition)
    {
        GameObject effect = GetEffectFromPool(healEffectPool, healImpactPrefab);
        
        if (effect != null)
        {
            PlayEffect(effect, targetPosition, "Heal");
        }
    }
    
    /// <summary>
    /// Play heal effect on a specific target
    /// </summary>
    public void PlayHealEffectOnTarget(Transform target)
    {
        if (target == null) return;
        
        Vector3 effectPosition = target.position + effectOffset;
        PlayHealEffect(effectPosition);
    }
    
    /// <summary>
    /// Play guard gain impact effect at target position
    /// </summary>
    public void PlayGuardEffect(Vector3 targetPosition)
    {
        GameObject effect = GetEffectFromPool(guardEffectPool, guardImpactPrefab);
        
        if (effect != null)
        {
            PlayEffect(effect, targetPosition, "Guard Up!");
        }
    }
    
    /// <summary>
    /// Play guard effect on a specific target
    /// </summary>
    public void PlayGuardEffectOnTarget(Transform target)
    {
        if (target == null) return;
        
        Vector3 effectPosition = target.position + effectOffset;
        PlayGuardEffect(effectPosition);
    }
    
    /// <summary>
    /// Play status effect impact (buffs/debuffs)
    /// </summary>
    public void PlayStatusEffect(Vector3 targetPosition, string effectName = "Status Effect")
    {
        GameObject effect = GetEffectFromPool(statusEffectPool, statusEffectImpactPrefab);
        
        if (effect != null)
        {
            PlayEffect(effect, targetPosition, effectName);
        }
    }
    
    /// <summary>
    /// Play status effect on a specific target
    /// </summary>
    public void PlayStatusEffectOnTarget(Transform target, string effectName = "Status Effect")
    {
        if (target == null) return;
        
        Vector3 effectPosition = target.position + effectOffset;
        PlayStatusEffect(effectPosition, effectName);
    }
    
    /// <summary>
    /// Get effect from pool or create new one
    /// </summary>
    private GameObject GetEffectFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        else if (prefab != null)
        {
            // Create new effect if pool is empty
            GameObject newEffect = Instantiate(prefab, effectPoolParent);
            newEffect.SetActive(false);
            return newEffect;
        }
        
        return null;
    }
    
    /// <summary>
    /// Position effect in world space while keeping it under canvas
    /// </summary>
    private void PositionEffectInWorld(GameObject effect, Vector3 worldPosition)
    {
        RectTransform rectTransform = effect.GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            // This is a UI element - position it in screen space
            if (targetCanvas != null)
            {
                // Convert world position to screen position
                Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
                
                // Convert screen position to canvas position
                Vector2 canvasPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    targetCanvas.GetComponent<RectTransform>(),
                    screenPosition,
                    targetCanvas.worldCamera,
                    out canvasPosition
                );
                
                rectTransform.anchoredPosition = canvasPosition;
                Debug.Log($"<color=cyan>Positioned UI effect {effect.name} at screen {screenPosition} -> canvas {canvasPosition}</color>");
            }
            else
            {
                // Fallback: use world position directly
                effect.transform.position = worldPosition;
                Debug.Log($"<color=cyan>Positioned UI effect {effect.name} at world position {worldPosition} (no canvas)</color>");
            }
        }
        else
        {
            // This is a world effect - position it directly
            effect.transform.position = worldPosition;
            Debug.Log($"<color=cyan>Positioned world effect {effect.name} at world position {worldPosition}</color>");
        }
    }
    
    /// <summary>
    /// Play the effect at specified position
    /// </summary>
    private void PlayEffect(GameObject effect, Vector3 position, string effectName)
    {
        if (effect == null) return;
        
        Debug.Log($"<color=blue>Setting up effect {effectName}</color>");
        Debug.Log($"<color=blue>  Position: {position}</color>");
        Debug.Log($"<color=blue>  Effect active before: {effect.activeInHierarchy}</color>");
        
        // Position the effect
        PositionEffectInWorld(effect, position);
        effect.SetActive(true);
        
        Debug.Log($"<color=blue>  Effect active after: {effect.activeInHierarchy}</color>");
        Debug.Log($"<color=blue>  Effect parent: {(effect.transform.parent != null ? effect.transform.parent.name : "NULL")}</color>");
        
        // Play effect animation/particles
        PlayEffectAnimation(effect);
        
        // Return to pool after duration
        if (autoDestroyEffects)
        {
            StartCoroutine(ReturnEffectToPool(effect, effectName));
        }
        
        Debug.Log($"<color=green>Playing {effectName} effect at {position}</color>");
    }
    
    /// <summary>
    /// Play effect animation (can be overridden for different effect types)
    /// </summary>
    private void PlayEffectAnimation(GameObject effect)
    {
        // Ensure the effect is properly set up as a UI element if needed
        EnsureEffectIsUI(effect);
        
        Debug.Log($"<color=magenta>Playing animation for {effect.name}</color>");
        
        // Try to find and play particle system
        ParticleSystem particles = effect.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            Debug.Log($"<color=magenta>  Found ParticleSystem, playing...</color>");
            Debug.Log($"<color=magenta>    ParticleSystem emission enabled: {particles.emission.enabled}</color>");
            Debug.Log($"<color=magenta>    ParticleSystem emission rate: {particles.emission.rateOverTime.constant}</color>");
            Debug.Log($"<color=magenta>    ParticleSystem max particles: {particles.main.maxParticles}</color>");
            Debug.Log($"<color=magenta>    ParticleSystem start lifetime: {particles.main.startLifetime.constant}</color>");
            Debug.Log($"<color=magenta>    ParticleSystem start speed: {particles.main.startSpeed.constant}</color>");
            Debug.Log($"<color=magenta>    ParticleSystem start size: {particles.main.startSize.constant}</color>");
            Debug.Log($"<color=magenta>    ParticleSystem start color: {particles.main.startColor.color}</color>");
            
            // Enable emission if it's disabled
            if (!particles.emission.enabled)
            {
                var emission = particles.emission;
                emission.enabled = true;
                Debug.Log($"<color=magenta>    Enabled ParticleSystem emission</color>");
            }
            
            // Ensure particle count is reasonable
            if (particles.main.maxParticles <= 0)
            {
                var main = particles.main;
                main.maxParticles = 100;
                Debug.Log($"<color=magenta>    Set ParticleSystem max particles to 100</color>");
            }
            
            particles.Play();
            
            // Check if particles are actually emitting
            StartCoroutine(CheckParticleEmission(particles, effect.name));
        }
        else
        {
            Debug.Log($"<color=yellow>  No ParticleSystem found</color>");
        }
        
        // Try to find and play animator
        Animator animator = effect.GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log($"<color=magenta>  Found Animator, playing 'Play'...</color>");
            animator.Play("Play");
        }
        else
        {
            Debug.Log($"<color=yellow>  No Animator found</color>");
        }
        
        // Try to find and play animation component
        Animation animation = effect.GetComponent<Animation>();
        if (animation != null)
        {
            Debug.Log($"<color=magenta>  Found Animation, playing...</color>");
            animation.Play();
        }
        else
        {
            Debug.Log($"<color=yellow>  No Animation found</color>");
        }
        
        // Check for Image component (for UI effects)
        Image image = effect.GetComponent<Image>();
        if (image != null)
        {
            Debug.Log($"<color=magenta>  Found Image component, color: {image.color}</color>");
        }
        
        // Check for SpriteRenderer (for world effects)
        SpriteRenderer spriteRenderer = effect.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Debug.Log($"<color=magenta>  Found SpriteRenderer, color: {spriteRenderer.color}</color>");
        }
    }
    
    /// <summary>
    /// Ensure effect is properly set up as a UI element
    /// </summary>
    private void EnsureEffectIsUI(GameObject effect)
    {
        // Check if this is a UI element
        RectTransform rectTransform = effect.GetComponent<RectTransform>();
        Canvas canvas = effect.GetComponentInParent<Canvas>();
        
        Debug.Log($"<color=orange>Ensuring effect {effect.name} is properly set up</color>");
        Debug.Log($"<color=orange>  Has RectTransform: {rectTransform != null}</color>");
        Debug.Log($"<color=orange>  Current parent: {(effect.transform.parent != null ? effect.transform.parent.name : "NULL")}</color>");
        Debug.Log($"<color=orange>  Canvas in parent: {canvas != null}</color>");
        
        // Check if this is a world ParticleSystem that shouldn't be moved to Canvas
        ParticleSystem particles = effect.GetComponent<ParticleSystem>();
        bool isWorldParticleSystem = particles != null && rectTransform == null;
        
        if (isWorldParticleSystem)
        {
            Debug.Log($"<color=orange>  World ParticleSystem detected - keeping in world space</color>");
            Debug.Log($"<color=orange>  NOT moving to Canvas to preserve particle rendering</color>");
            
            // Move world ParticleSystem out of Canvas hierarchy
            if (effect.transform.parent != null && effect.transform.parent.GetComponentInParent<Canvas>() != null)
            {
                Debug.Log($"<color=orange>  Moving world ParticleSystem out of Canvas hierarchy</color>");
                effect.transform.SetParent(null, true); // Keep world position
                Debug.Log($"<color=orange>  World ParticleSystem now at root level</color>");
            }
        }
        else if (targetCanvas != null && effect.transform.parent != targetCanvas.transform)
        {
            Debug.Log($"<color=orange>  Moving UI effect from {effect.transform.parent.name} to target canvas: {targetCanvas.name}</color>");
            effect.transform.SetParent(targetCanvas.transform, false);
            
            // If it's a UI element, set proper positioning
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
                Debug.Log($"<color=orange>  Set UI positioning: anchoredPosition={rectTransform.anchoredPosition}, localScale={rectTransform.localScale}</color>");
            }
        }
        else if (canvas == null && !isWorldParticleSystem)
        {
            // Fallback: find the main combat canvas (only for UI elements)
            Canvas combatCanvas = FindMainCombatCanvas();
            if (combatCanvas != null)
            {
                Debug.Log($"<color=orange>  Moving UI effect to auto-found combat canvas: {combatCanvas.name}</color>");
                effect.transform.SetParent(combatCanvas.transform, false);
                
                // If it's a UI element, set proper positioning
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.localScale = Vector3.one;
                }
            }
            else
            {
                Debug.LogWarning($"<color=red>UI effect {effect.name} has no canvas found! It may not render properly.</color>");
            }
        }
        else if (isWorldParticleSystem)
        {
            Debug.Log($"<color=orange>  World ParticleSystem is properly positioned in world space</color>");
        }
        else
        {
            Debug.Log($"<color=orange>  Effect is already properly positioned under canvas: {canvas.name}</color>");
        }
    }
    
    /// <summary>
    /// Find the main combat canvas
    /// </summary>
    private Canvas FindMainCombatCanvas()
    {
        // Look for canvas in the scene
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        // Prefer a canvas with "Combat" in the name
        foreach (Canvas canvas in canvases)
        {
            if (canvas.name.ToLower().Contains("combat"))
            {
                return canvas;
            }
        }
        
        // Fallback to any canvas
        if (canvases.Length > 0)
        {
            return canvases[0];
        }
        
        return null;
    }
    
    /// <summary>
    /// Return effect to pool after duration
    /// </summary>
    private System.Collections.IEnumerator ReturnEffectToPool(GameObject effect, string effectName)
    {
        yield return new WaitForSeconds(effectDuration);
        
        if (effect != null)
        {
            effect.SetActive(false);
            
            // Return to appropriate pool based on effect name
            bool returnedToPool = false;
            
            // Check for elemental damage types
            foreach (DamageType damageType in System.Enum.GetValues(typeof(DamageType)))
            {
                if (effectName.Contains(damageType.ToString()))
                {
                    if (elementalEffectPools.ContainsKey(damageType))
                    {
                        elementalEffectPools[damageType].Enqueue(effect);
                        returnedToPool = true;
                        break;
                    }
                }
            }
            
            // Fallback to basic pools
            if (!returnedToPool)
            {
                if (effectName.Contains("Critical") || effectName.Contains("Damage"))
                {
                    if (effectName.Contains("Critical"))
                        criticalEffectPool.Enqueue(effect);
                    else
                        damageEffectPool.Enqueue(effect);
                }
                else if (effectName.Contains("Heal"))
                {
                    healEffectPool.Enqueue(effect);
                }
                else if (effectName.Contains("Guard"))
                {
                    guardEffectPool.Enqueue(effect);
                }
                else if (effectName.Contains("Status"))
                {
                    statusEffectPool.Enqueue(effect);
                }
            }
            
            Debug.Log($"Returned {effectName} effect to pool");
        }
    }
    
    #region Context Menu Debug Methods
    
    [ContextMenu("Test Damage Effect")]
    private void TestDamageEffect()
    {
        PlayDamageEffect(transform.position);
    }
    
    [ContextMenu("Test Critical Damage Effect")]
    private void TestCriticalDamageEffect()
    {
        PlayDamageEffect(transform.position, true);
    }
    
    [ContextMenu("Test Heal Effect")]
    private void TestHealEffect()
    {
        PlayHealEffect(transform.position);
    }
    
    [ContextMenu("Test Guard Effect")]
    private void TestGuardEffect()
    {
        PlayGuardEffect(transform.position);
    }
    
    [ContextMenu("Test Status Effect")]
    private void TestStatusEffect()
    {
        PlayStatusEffect(transform.position, "Test Buff");
    }
    
    [ContextMenu("Test Fire Damage Effect")]
    private void TestFireDamageEffect()
    {
        PlayElementalDamageEffect(transform.position, DamageType.Fire);
    }
    
    [ContextMenu("Test Cold Damage Effect")]
    private void TestColdDamageEffect()
    {
        PlayElementalDamageEffect(transform.position, DamageType.Cold);
    }
    
    [ContextMenu("Test Lightning Damage Effect")]
    private void TestLightningDamageEffect()
    {
        PlayElementalDamageEffect(transform.position, DamageType.Lightning);
    }
    
    [ContextMenu("Test Chaos Damage Effect")]
    private void TestChaosDamageEffect()
    {
        PlayElementalDamageEffect(transform.position, DamageType.Chaos);
    }
    
    [ContextMenu("Test Physical Damage Effect")]
    private void TestPhysicalDamageEffect()
    {
        PlayElementalDamageEffect(transform.position, DamageType.Physical);
    }
    
    
    [ContextMenu("Test Critical Fire Damage Effect")]
    private void TestCriticalFireDamageEffect()
    {
        PlayElementalDamageEffect(transform.position, DamageType.Fire, true);
    }
    
    [ContextMenu("Debug Effect Pools")]
    private void DebugEffectPools()
    {
        Debug.Log("=== EFFECT POOLS DEBUG ===");
        Debug.Log($"Damage Effects: {damageEffectPool.Count}/{poolSize}");
        Debug.Log($"Heal Effects: {healEffectPool.Count}/{poolSize}");
        Debug.Log($"Guard Effects: {guardEffectPool.Count}/{poolSize}");
        Debug.Log($"Critical Effects: {criticalEffectPool.Count}/{poolSize}");
        Debug.Log($"Status Effects: {statusEffectPool.Count}/{poolSize}");
        
        Debug.Log("=== ELEMENTAL EFFECT POOLS ===");
        foreach (var kvp in elementalEffectPools)
        {
            Debug.Log($"{kvp.Key} Effects: {kvp.Value.Count}/{poolSize}");
        }
        Debug.Log("=== END DEBUG ===");
    }
    
    [ContextMenu("Debug Canvas Setup")]
    private void DebugCanvasSetup()
    {
        Debug.Log("=== CANVAS DEBUG ===");
        Debug.Log($"Target Canvas: {(targetCanvas != null ? targetCanvas.name : "NULL")}");
        Debug.Log($"Auto Find Canvas: {autoFindCanvas}");
        
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"Total Canvases Found: {allCanvases.Length}");
        
        for (int i = 0; i < allCanvases.Length; i++)
        {
            Canvas canvas = allCanvases[i];
            Debug.Log($"  Canvas {i}: {canvas.name} (Sort Order: {canvas.sortingOrder}, Render Mode: {canvas.renderMode})");
        }
        
        Canvas mainCanvas = FindMainCombatCanvas();
        Debug.Log($"Main Combat Canvas: {(mainCanvas != null ? mainCanvas.name : "NULL")}");
        
        Debug.Log("=== END CANVAS DEBUG ===");
    }
    
    [ContextMenu("Force Canvas Auto-Find")]
    private void ForceCanvasAutoFind()
    {
        targetCanvas = FindMainCombatCanvas();
        if (targetCanvas != null)
        {
            Debug.Log($"Force found target canvas: {targetCanvas.name}");
        }
        else
        {
            Debug.LogWarning("No combat canvas found!");
        }
    }
    
    [ContextMenu("Debug Effect Prefabs")]
    private void DebugEffectPrefabs()
    {
        Debug.Log("=== EFFECT PREFAB DEBUG ===");
        Debug.Log($"Physical Impact Prefab: {(physicalImpactPrefab != null ? physicalImpactPrefab.name : "NULL")}");
        Debug.Log($"Fire Impact Prefab: {(fireImpactPrefab != null ? fireImpactPrefab.name : "NULL")}");
        Debug.Log($"Cold Impact Prefab: {(coldImpactPrefab != null ? coldImpactPrefab.name : "NULL")}");
        Debug.Log($"Lightning Impact Prefab: {(lightningImpactPrefab != null ? lightningImpactPrefab.name : "NULL")}");
        Debug.Log($"Chaos Impact Prefab: {(chaosImpactPrefab != null ? chaosImpactPrefab.name : "NULL")}");
        
        if (physicalImpactPrefab != null)
        {
            Debug.Log($"Physical Prefab Components:");
            Component[] components = physicalImpactPrefab.GetComponents<Component>();
            foreach (Component comp in components)
            {
                Debug.Log($"  - {comp.GetType().Name}");
            }
        }
        Debug.Log("=== END PREFAB DEBUG ===");
    }
    
    [ContextMenu("Test Effect Visibility")]
    private void TestEffectVisibility()
    {
        // Create a simple test effect to check visibility
        GameObject testEffect = new GameObject("TestEffect");
        testEffect.transform.SetParent(targetCanvas.transform, false);
        
        // Add Image component for visibility
        Image testImage = testEffect.AddComponent<Image>();
        testImage.color = Color.red;
        
        // Position in center of screen
        RectTransform rectTransform = testEffect.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(100, 100);
        rectTransform.anchoredPosition = Vector2.zero;
        
        Debug.Log("Created red test effect in center of screen");
        Debug.Log($"Test effect parent: {testEffect.transform.parent.name}");
        Debug.Log($"Test effect position: {testEffect.transform.position}");
        
        // Destroy after 3 seconds
        Destroy(testEffect, 3f);
    }
    
    /// <summary>
    /// Check if particles are actually emitting
    /// </summary>
    private System.Collections.IEnumerator CheckParticleEmission(ParticleSystem particles, string effectName)
    {
        yield return new WaitForSeconds(0.1f); // Wait a bit for particles to start
        
        int particleCount = particles.particleCount;
        bool isPlaying = particles.isPlaying;
        bool isEmitting = particles.emission.enabled;
        
        Debug.Log($"<color=lime>ParticleSystem Check for {effectName}:</color>");
        Debug.Log($"<color=lime>  Is Playing: {isPlaying}</color>");
        Debug.Log($"<color=lime>  Is Emitting: {isEmitting}</color>");
        Debug.Log($"<color=lime>  Particle Count: {particleCount}</color>");
        Debug.Log($"<color=lime>  Position: {particles.transform.position}</color>");
        Debug.Log($"<color=lime>  Local Position: {particles.transform.localPosition}</color>");
        
        if (particleCount == 0 && isPlaying)
        {
            Debug.LogWarning($"<color=red>ParticleSystem {effectName} is playing but not emitting particles!</color>");
            Debug.LogWarning($"<color=red>Check ParticleSystem settings: emission rate, lifetime, speed, size</color>");
            
            // Additional debugging for world vs UI space issue
            Debug.LogWarning($"<color=red>DIAGNOSIS: World ParticleSystem moved to Canvas!</color>");
            Debug.LogWarning($"<color=red>SOLUTION: Keep world effects in world space, not under Canvas</color>");
            Debug.LogWarning($"<color=red>FIX: Move effect back to world space or use UI ParticleSystem</color>");
        }
        else if (particleCount > 0)
        {
            Debug.Log($"<color=lime>ParticleSystem {effectName} is working correctly with {particleCount} particles</color>");
        }
    }
    
    #endregion
}
