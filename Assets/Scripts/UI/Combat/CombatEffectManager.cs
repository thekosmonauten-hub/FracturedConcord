using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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
    [Tooltip("Dedicated canvas for visual effects (FxCanvas). If not assigned, will auto-find or use targetCanvas")]
    [SerializeField] private Canvas fxCanvas;
    [Tooltip("If true, automatically find and use the main combat canvas")]
    [SerializeField] private bool autoFindCanvas = true;
    [Tooltip("If true, automatically find FxCanvas by name")]
    [SerializeField] private bool autoFindFxCanvas = true;
    
    [Header("Effects Database")]
    [Tooltip("Database of all visual effects. Auto-loads from Resources if not assigned.")]
    [SerializeField] private EffectsDatabase effectsDatabase;
    
    /// <summary>
    /// Get the effects database (for singleton access)
    /// </summary>
    public EffectsDatabase GetEffectsDatabase() => effectsDatabase;
    
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
        
        // Initialize effects database if not assigned
        if (effectsDatabase == null)
        {
            effectsDatabase = EffectsDatabase.Instance;
            if (effectsDatabase != null)
            {
                Debug.Log($"CombatEffectManager auto-loaded EffectsDatabase: {effectsDatabase.name}");
            }
            else
            {
                Debug.LogWarning("CombatEffectManager: EffectsDatabase not found! Please assign it in the Inspector or create it in Resources/EffectsDatabase.asset");
            }
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
    /// Enemies always use "Default" effect point
    /// </summary>
    public void PlayDamageEffectOnTarget(Transform target, bool isCritical = false)
    {
        if (target == null) return;
        
        // Use database if available
        if (effectsDatabase != null || EffectsDatabase.Instance != null)
        {
            if (effectsDatabase == null) effectsDatabase = EffectsDatabase.Instance;
            
            var query = new EffectQuery
            {
                effectType = VisualEffectType.Impact,
                damageType = DamageType.Physical,
                isCritical = isCritical
            };
            
            EffectData impactData = effectsDatabase.FindEffect(query);
            if (impactData != null)
            {
                // Enemies always use "Default" - handled in PlayEffectFromData
                PlayEffectFromData(impactData, target, "Default");
                return;
            }
        }
        
        // Fallback to old method
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
    /// Enemies always use "Default" effect point
    /// </summary>
    public void PlayElementalDamageEffectOnTarget(Transform target, DamageType damageType, bool isCritical = false)
    {
        if (target == null) return;
        
        // Use database if available, otherwise fallback to old method
        if (effectsDatabase != null || EffectsDatabase.Instance != null)
        {
            if (effectsDatabase == null) effectsDatabase = EffectsDatabase.Instance;
            
            EffectData impactData = effectsDatabase.FindImpactEffect(damageType, isCritical);
            if (impactData != null)
            {
                // Enemies always use "Default" - handled in PlayEffectFromData
                PlayEffectFromData(impactData, target, "Default");
                return;
            }
        }
        
        // Fallback to old method
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
    /// Find the FxCanvas for visual effects
    /// </summary>
    private Canvas FindFxCanvas()
    {
        // Look for canvas with "Fx" in the name (case-insensitive)
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        foreach (Canvas canvas in canvases)
        {
            string canvasName = canvas.name.ToLower();
            if (canvasName.Contains("fxcanvas") || canvasName.Contains("fx canvas") || canvasName == "fxcanvas")
            {
                return canvas;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the canvas to use for effects (prefers FxCanvas, falls back to targetCanvas)
    /// </summary>
    private Canvas GetEffectsCanvas()
    {
        if (fxCanvas != null) return fxCanvas;
        return targetCanvas;
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
    
    #region Effects Database Integration
    
    /// <summary>
    /// Play effect using database lookup
    /// </summary>
    public void PlayEffectFromDatabase(EffectQuery query, Transform target, 
        string effectPointName = "Default")
    {
        if (effectsDatabase == null)
        {
            effectsDatabase = EffectsDatabase.Instance;
        }
        
        if (effectsDatabase == null)
        {
            Debug.LogError("EffectsDatabase not found!");
            return;
        }
        
        EffectData effectData = effectsDatabase.FindEffect(query);
        if (effectData == null)
        {
            Debug.LogWarning($"No effect found for query: {query.effectType}, {query.damageType}");
            return;
        }
        
        PlayEffectFromData(effectData, target, effectPointName);
    }
    
    /// <summary>
    /// Play effect from EffectData
    /// </summary>
    public void PlayEffectFromData(EffectData effectData, Transform target, 
        string effectPointName = "Default")
    {
        if (effectData == null || effectData.effectPrefab == null)
        {
            Debug.LogWarning("[CombatEffectManager] EffectData or prefab is null!");
            return;
        }
        
        // Enemies always use "Default" effect point (dynamic sprites)
        bool isEnemy = IsEnemyTarget(target);
        if (isEnemy)
        {
            effectPointName = "Default";
            Debug.Log($"[CombatEffectManager] Enemy target detected - forcing effect point to 'Default'");
        }
        
        // Get effect point
        Transform effectPoint = GetEffectPointFromTarget(target, effectPointName);
        
        if (effectPoint == null)
        {
            Debug.LogError($"[CombatEffectManager] Could not get effect point '{effectPointName}' from {target?.name}! Using target transform as fallback.");
            effectPoint = target;
        }
        
        RectTransform targetRect = effectPoint.GetComponent<RectTransform>();
        
        if (targetRect == null)
        {
            Debug.LogWarning($"[CombatEffectManager] Effect point {effectPoint.name} doesn't have RectTransform! Trying to add one...");
            targetRect = effectPoint.gameObject.AddComponent<RectTransform>();
            if (targetRect == null)
            {
                Debug.LogError($"[CombatEffectManager] Could not create RectTransform for effect point!");
                return;
            }
        }
        
        // Validate enemy Default point
        if (isEnemy && effectPoint != null)
        {
            // Use GetComponentInChildren because EffectPointProvider may be a child GameObject
            EffectPointProvider provider = target.GetComponentInChildren<EffectPointProvider>(includeInactive: false);
            if (provider != null && provider.effectPoint_Default != null)
            {
                if (effectPoint != provider.effectPoint_Default)
                {
                    Debug.LogWarning($"[CombatEffectManager] Effect point mismatch! Expected Default ({provider.effectPoint_Default.name}), got {effectPoint.name}. Using Default.");
                    effectPoint = provider.effectPoint_Default;
                    targetRect = effectPoint.GetComponent<RectTransform>();
                }
                else
                {
                    Debug.Log($"[CombatEffectManager] ✓ Impact effect using enemy Default point: {effectPoint.name}");
                }
            }
        }
        
        Debug.Log($"[CombatEffectManager] Playing impact effect '{effectData.effectName}' at point: {effectPoint.name} (position: {targetRect.anchoredPosition})");
        
        // Instantiate effect
        GameObject effect = Instantiate(effectData.effectPrefab);
        
        // Setup UIParticle (will check if already exists in prefab)
        // Use FxCanvas if available, otherwise use targetCanvas
        Canvas effectsCanvas = GetEffectsCanvas();
        effect = UIParticleHelper.SetupUIParticle(effect, effectData, effectsCanvas);
        
        // Position effect
        RectTransform effectRect = effect.GetComponent<RectTransform>();
        if (effectRect != null)
        {
            Vector2 targetPos = GetAnchoredPositionInCanvas(targetRect);
            effectRect.anchoredPosition = targetPos;
            
            // Make sure size is small (not visible) - UIParticle doesn't need a large RectTransform
            if (effectRect.sizeDelta.x > 10 || effectRect.sizeDelta.y > 10)
            {
                effectRect.sizeDelta = new Vector2(1, 1);
            }
            
            // Fix scale if it's zero (can cause rendering issues)
            if (effect.transform.localScale == Vector3.zero)
            {
                effect.transform.localScale = Vector3.one;
            }
            
            // CRITICAL: Disable raycastTarget on UIParticle to prevent red border
            Coffee.UIExtensions.UIParticle uiParticle = effect.GetComponent<Coffee.UIExtensions.UIParticle>();
            if (uiParticle != null)
            {
                uiParticle.raycastTarget = false;
            }
            
            // Disable any Image components that might show a red border
            UnityEngine.UI.Image image = effect.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.raycastTarget = false;
                var color = image.color;
                color.a = 0f;
                image.color = color;
            }
            
            Debug.Log($"[CombatEffectManager] Positioned impact effect at: {targetPos}");
        }
        
        // Play effect
        effect.SetActive(true);
        
        ParticleSystem particles = effect.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
            Debug.Log($"[CombatEffectManager] Started ParticleSystem for impact effect");
        }
        
        // Auto-destroy
        if (effectData.duration > 0)
        {
            Destroy(effect, effectData.duration);
        }
    }
    
    /// <summary>
    /// Play effect for a specific card
    /// First checks for card-specific effect, then falls back to damage type
    /// </summary>
    public void PlayEffectForCard(Card card, Transform target, bool isCritical = false)
    {
        if (card == null || target == null)
        {
            Debug.LogWarning("Card or target is null!");
            return;
        }
        
        if (effectsDatabase == null)
        {
            effectsDatabase = EffectsDatabase.Instance;
        }
        
        if (effectsDatabase == null)
        {
            Debug.LogWarning("EffectsDatabase not found!");
            return;
        }
        
        // Priority 1: Check for card-specific effect
        EffectData cardEffect = effectsDatabase.FindEffectByCardName(card.cardName);
        if (cardEffect != null)
        {
            Debug.Log($"Playing card-specific effect for {card.cardName}");
            PlayEffectFromData(cardEffect, target);
            return;
        }
        
        // Priority 2: Use damage type-based effect
        DamageType primaryDamageType = card.primaryDamageType;
        if (primaryDamageType == DamageType.None)
        {
            primaryDamageType = DamageType.Physical; // Default fallback
        }
        
        Debug.Log($"[CombatEffectManager] Looking for impact effect - Card: {card.cardName}, DamageType: {primaryDamageType}, isCritical: {isCritical}");
        Debug.Log($"[CombatEffectManager] EffectsDatabase has {effectsDatabase.allEffects.Count(e => e != null)} effects");
        
        // Debug: List all Physical impact effects
        var physicalImpacts = effectsDatabase.allEffects.Where(e => 
            e != null && 
            e.effectType == VisualEffectType.Impact && 
            e.damageType == DamageType.Physical).ToList();
        Debug.Log($"[CombatEffectManager] Found {physicalImpacts.Count} Physical Impact effects:");
        foreach (var effect in physicalImpacts)
        {
            Debug.Log($"  - {effect.effectName} (type: {effect.effectType}, damageType: {effect.damageType}, isProjectile: {effect.isProjectile})");
        }
        
        EffectData impactEffect = effectsDatabase.FindImpactEffect(primaryDamageType, isCritical);
        if (impactEffect != null)
        {
            Debug.Log($"[CombatEffectManager] ✓ Found impact effect: {impactEffect.effectName} for {card.cardName}");
            PlayEffectFromData(impactEffect, target);
            return;
        }
        
        Debug.LogWarning($"[CombatEffectManager] ✗ No effect found for card {card.cardName} (type: {primaryDamageType}, isCritical: {isCritical})");
    }
    
    /// <summary>
    /// Play Area effect for a card (AoE effects)
    /// Priority:
    /// 1. Card-specific Area effect (associatedCardName matches) - highest priority
    /// 2. Tag-based Area effect (card has tag that matches effect tag) - fallback
    /// Plays once at the targeted enemy's "Default" effect point
    /// </summary>
    public void PlayAreaEffectForCard(Card card, Transform targetEnemy, bool isCritical = false)
    {
        if (card == null)
        {
            Debug.LogWarning("[CombatEffectManager] Card is null!");
            return;
        }
        
        if (targetEnemy == null)
        {
            Debug.LogWarning("[CombatEffectManager] Target enemy is null! Cannot play Area effect.");
            return;
        }
        
        if (effectsDatabase == null)
        {
            effectsDatabase = EffectsDatabase.Instance;
        }
        
        if (effectsDatabase == null)
        {
            Debug.LogWarning("[CombatEffectManager] EffectsDatabase not found!");
            return;
        }
        
        // Priority 1: Check for card-specific Area effect (associatedCardName matches)
        EffectData cardAreaEffect = effectsDatabase.FindEffectByCardName(card.cardName);
        if (cardAreaEffect != null && cardAreaEffect.effectType == VisualEffectType.Area)
        {
            // Check if this is a Warcry effect (should play from player HEAD)
            bool isWarcry = cardAreaEffect.tags != null && cardAreaEffect.tags.Contains("Warcry", System.StringComparer.OrdinalIgnoreCase);
            if (isWarcry)
            {
                Transform playerIcon = FindPlayerCharacterIcon();
                if (playerIcon != null)
                {
                    Debug.Log($"[CombatEffectManager] Playing card-specific Warcry Area effect '{cardAreaEffect.effectName}' for {card.cardName} from player HEAD location");
                    PlayEffectFromData(cardAreaEffect, playerIcon, "HEAD");
                }
                else
                {
                    Debug.LogWarning($"[CombatEffectManager] Could not find player icon for Warcry effect! Falling back to enemy Default point.");
                    PlayEffectFromData(cardAreaEffect, targetEnemy, "Default");
                }
            }
            else
            {
                Debug.Log($"[CombatEffectManager] Playing card-specific Area effect '{cardAreaEffect.effectName}' for {card.cardName} at targeted enemy's Default point");
                PlayEffectFromData(cardAreaEffect, targetEnemy, "Default");
            }
            return;
        }
        
        // Priority 2: Check for tag-based Area effect (card has tag that matches effect tag)
        if (card.tags != null && card.tags.Count > 0)
        {
            foreach (string cardTag in card.tags)
            {
                EffectData tagAreaEffect = effectsDatabase.FindEffectByTag(cardTag, VisualEffectType.Area);
                if (tagAreaEffect != null)
                {
                    // Check if this is a Warcry effect (should play from player HEAD)
                    bool isWarcry = tagAreaEffect.tags != null && tagAreaEffect.tags.Contains("Warcry", System.StringComparer.OrdinalIgnoreCase);
                    if (isWarcry)
                    {
                        Transform playerIcon = FindPlayerCharacterIcon();
                        if (playerIcon != null)
                        {
                            Debug.Log($"[CombatEffectManager] Playing tag-based Warcry Area effect '{tagAreaEffect.effectName}' for {card.cardName} (tag: '{cardTag}') from player HEAD location");
                            PlayEffectFromData(tagAreaEffect, playerIcon, "HEAD");
                        }
                        else
                        {
                            Debug.LogWarning($"[CombatEffectManager] Could not find player icon for Warcry effect! Falling back to enemy Default point.");
                            PlayEffectFromData(tagAreaEffect, targetEnemy, "Default");
                        }
                    }
                    else
                    {
                        Debug.Log($"[CombatEffectManager] Playing tag-based Area effect '{tagAreaEffect.effectName}' for {card.cardName} (tag: '{cardTag}') at targeted enemy's Default point");
                        PlayEffectFromData(tagAreaEffect, targetEnemy, "Default");
                    }
                    return;
                }
            }
        }
        
        Debug.Log($"[CombatEffectManager] No Area effect found for '{card.cardName}'. Checked card-specific (associatedCardName) and tag-based effects.");
    }
    
    /// <summary>
    /// Play an effect from EffectData at a specific RectTransform position (for Area effects)
    /// Uses the same positioning logic as PlayEffectFromData for consistency
    /// </summary>
    private void PlayEffectFromDataAtPosition(EffectData effectData, RectTransform targetRect)
    {
        if (effectData == null || effectData.effectPrefab == null)
        {
            Debug.LogWarning("[CombatEffectManager] EffectData or prefab is null!");
            return;
        }
        
        if (targetRect == null)
        {
            Debug.LogWarning("[CombatEffectManager] Target RectTransform is null!");
            return;
        }
        
        Debug.Log($"[CombatEffectManager] Playing Area effect '{effectData.effectName}' at AoE area position (target: {targetRect.name})");
        
        // Instantiate effect
        GameObject effect = Instantiate(effectData.effectPrefab);
        
        // Setup UIParticle (will check if already exists in prefab)
        // Use FxCanvas if available, otherwise use targetCanvas
        Canvas effectsCanvas = GetEffectsCanvas();
        effect = UIParticleHelper.SetupUIParticle(effect, effectData, effectsCanvas);
        
        // Position effect using the same logic as PlayEffectFromData
        RectTransform effectRect = effect.GetComponent<RectTransform>();
        if (effectRect != null)
        {
            // Use GetAnchoredPositionInCanvas for proper canvas-space positioning (same as PlayEffectFromData)
            Vector2 targetPos = GetAnchoredPositionInCanvas(targetRect);
            effectRect.anchoredPosition = targetPos;
            
            Debug.Log($"[CombatEffectManager] Area effect positioned at canvas-space: {targetPos} (target rect: {targetRect.anchoredPosition})");
            
            // Make sure size is small (not visible) - UIParticle doesn't need a large RectTransform
            if (effectRect.sizeDelta.x > 10 || effectRect.sizeDelta.y > 10)
            {
                effectRect.sizeDelta = new Vector2(1, 1);
            }
            
            // Fix scale if it's zero (can cause rendering issues)
            if (effect.transform.localScale == Vector3.zero)
            {
                effect.transform.localScale = Vector3.one;
            }
            
            // CRITICAL: Disable raycastTarget on UIParticle to prevent red border
            Coffee.UIExtensions.UIParticle uiParticle = effect.GetComponent<Coffee.UIExtensions.UIParticle>();
            if (uiParticle != null)
            {
                uiParticle.raycastTarget = false;
            }
            
            // Disable any Image components that might show a red border
            UnityEngine.UI.Image image = effect.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.raycastTarget = false;
                // Make it invisible if it exists
                var color = image.color;
                color.a = 0f;
                image.color = color;
            }
        }
        else
        {
            // World space effect - position at world position
            effect.transform.position = targetRect.position;
            Debug.Log($"[CombatEffectManager] Area effect (world space) positioned at: {targetRect.position}");
        }
        
        effect.SetActive(true);
        
        // Auto-destroy after duration
        if (autoDestroyEffects && effectData.duration > 0)
        {
            Destroy(effect, effectData.duration);
        }
        
        Debug.Log($"[CombatEffectManager] ✓ Area effect '{effectData.effectName}' playing at AoE area");
    }
    
    /// <summary>
    /// Play projectile effect for a specific card
    /// First checks for card-specific projectile, then falls back to damage type
    /// </summary>
    public void PlayProjectileForCard(Card card, Transform startTransform, Transform endTransform,
        string startPointName = "Weapon", string endPointName = "Chest", bool isCritical = false, System.Action onProjectileHit = null)
    {
        if (card == null || startTransform == null || endTransform == null)
        {
            Debug.LogWarning("Card or transforms are null!");
            return;
        }
        
        if (effectsDatabase == null)
        {
            effectsDatabase = EffectsDatabase.Instance;
        }
        
        if (effectsDatabase == null)
        {
            Debug.LogWarning("EffectsDatabase not found!");
            return;
        }
        
        // Priority 1: Check for card-specific projectile
        Debug.Log($"[CombatEffectManager] Looking for card-specific effect for card name: '{card.cardName}'");
        EffectData cardProjectile = effectsDatabase.FindEffectByCardName(card.cardName);
        if (cardProjectile != null)
        {
            Debug.Log($"[CombatEffectManager] Found card-specific effect: '{cardProjectile.effectName}' (isProjectile: {cardProjectile.isProjectile}, associatedCardName: '{cardProjectile.associatedCardName}')");
            if (cardProjectile.isProjectile)
            {
                Debug.Log($"Playing card-specific projectile for {card.cardName}");
                PlayProjectileFromData(cardProjectile, startTransform, endTransform, startPointName, endPointName, onProjectileHit);
                return;
            }
            else
            {
                Debug.LogWarning($"[CombatEffectManager] Card-specific effect '{cardProjectile.effectName}' is not a projectile! Falling back to damage type.");
            }
        }
        else
        {
            Debug.Log($"[CombatEffectManager] No card-specific effect found for '{card.cardName}'. Checking all effects...");
            // Debug: List all effects with associated card names
            if (effectsDatabase.allEffects != null)
            {
                foreach (var effect in effectsDatabase.allEffects)
                {
                    if (effect != null && !string.IsNullOrEmpty(effect.associatedCardName))
                    {
                        Debug.Log($"  - Effect '{effect.effectName}' is associated with card: '{effect.associatedCardName}'");
                    }
                }
            }
        }
        
        // Priority 2: Use damage type-based projectile
        DamageType primaryDamageType = card.primaryDamageType;
        if (primaryDamageType == DamageType.None)
        {
            primaryDamageType = DamageType.Physical; // Default fallback
        }
        
        EffectData projectileEffect = effectsDatabase.FindProjectileEffect(primaryDamageType, isCritical);
        if (projectileEffect != null)
        {
            Debug.Log($"Playing damage-type projectile ({primaryDamageType}) for {card.cardName}");
            PlayProjectileFromData(projectileEffect, startTransform, endTransform, startPointName, endPointName, onProjectileHit);
            return;
        }
        
        Debug.LogWarning($"No projectile found for card {card.cardName} (type: {primaryDamageType})");
    }
    
    /// <summary>
    /// Play projectile effect using database
    /// Enemies always use "Default" as end point
    /// </summary>
    public void PlayProjectileFromDatabase(Transform startTransform, Transform endTransform,
        DamageType damageType, bool isCritical = false,
        string startPointName = "Weapon", string endPointName = "Chest")
    {
        if (effectsDatabase == null)
        {
            effectsDatabase = EffectsDatabase.Instance;
        }
        
        EffectData projectileData = effectsDatabase.FindProjectileEffect(damageType, isCritical);
        if (projectileData == null)
        {
            Debug.LogWarning($"No projectile effect found for {damageType}");
            return;
        }
        
        // Enemies always use "Default" - this will be handled in PlayProjectileFromData
        PlayProjectileFromData(projectileData, startTransform, endTransform, 
            startPointName, endPointName, null);
    }
    
    /// <summary>
    /// Play projectile from EffectData
    /// </summary>
    public void PlayProjectileFromData(EffectData projectileData, Transform startTransform, 
        Transform endTransform, string startPointName = "Weapon", string endPointName = "Chest", System.Action onProjectileHit = null)
    {
        Debug.Log($"[CombatEffectManager] PlayProjectileFromData called: prefab={(projectileData?.effectPrefab != null ? projectileData.effectPrefab.name : "NULL")}, start={startTransform?.name}, end={endTransform?.name}");
        
        if (projectileData == null || !projectileData.isProjectile)
        {
            Debug.LogWarning("Invalid projectile data!");
            return;
        }
        
        if (projectileData.effectPrefab == null)
        {
            Debug.LogError($"Projectile effect '{projectileData.effectName}' has no prefab assigned!");
            return;
        }
        
        // Enemies always use "Default" effect point (dynamic sprites)
        if (IsEnemyTarget(endTransform))
        {
            endPointName = "Default";
        }
        
        // Get effect points
        Transform startPoint = GetEffectPointFromTarget(startTransform, startPointName);
        Transform endPoint = GetEffectPointFromTarget(endTransform, endPointName);
        
        Debug.Log($"[CombatEffectManager] Effect points - start: {(startPoint != null ? startPoint.name : "NULL")} (requested: '{startPointName}'), end: {(endPoint != null ? endPoint.name : "NULL")} (requested: '{endPointName}', forced to 'Default' for enemy: {IsEnemyTarget(endTransform)})");
        
        // Validate that enemy end point is actually "Default"
        if (IsEnemyTarget(endTransform) && endPoint != null)
        {
            // Use GetComponentInChildren because EffectPointProvider may be a child GameObject
            EffectPointProvider provider = endTransform.GetComponentInChildren<EffectPointProvider>(includeInactive: false);
            if (provider != null)
            {
                Transform defaultPoint = provider.effectPoint_Default;
                if (defaultPoint != endPoint)
                {
                    Debug.LogWarning($"[CombatEffectManager] Enemy end point mismatch! Expected Default point ({defaultPoint?.name}), got {endPoint.name}. Using Default point.");
                    endPoint = defaultPoint ?? endTransform;
                }
                else
                {
                    Debug.Log($"[CombatEffectManager] ✓ Enemy end point verified as Default: {endPoint.name}");
                }
            }
        }
        
        RectTransform startRect = startPoint != null ? startPoint.GetComponent<RectTransform>() : null;
        RectTransform endRect = endPoint != null ? endPoint.GetComponent<RectTransform>() : null;
        
        if (startRect == null)
        {
            Debug.LogWarning($"Start point '{startPointName}' on {startTransform?.name} needs RectTransform! Creating fallback...");
            // Try to use the transform itself
            if (startTransform != null)
            {
                startRect = startTransform.GetComponent<RectTransform>();
                if (startRect == null)
                {
                    Debug.LogError($"Cannot create projectile: {startTransform.name} has no RectTransform!");
                    return;
                }
            }
        }
        
        if (endRect == null)
        {
            Debug.LogWarning($"End point '{endPointName}' on {endTransform?.name} needs RectTransform! Creating fallback...");
            // Try to use the transform itself
            if (endTransform != null)
            {
                endRect = endTransform.GetComponent<RectTransform>();
                if (endRect == null)
                {
                    Debug.LogError($"Cannot create projectile: {endTransform.name} has no RectTransform!");
                    return;
                }
            }
        }
        
        if (startRect == null || endRect == null)
        {
            Debug.LogError("Start/end points need RectTransform!");
            return;
        }
        
        Debug.Log($"[CombatEffectManager] Instantiating projectile prefab: {projectileData.effectPrefab.name}");
        // Instantiate projectile
        GameObject projectile = Instantiate(projectileData.effectPrefab);
        Debug.Log($"[CombatEffectManager] Projectile instantiated: {projectile.name}, active: {projectile.activeSelf}");
        
        // Flag to prevent double-triggering (callback vs coroutine)
        bool impactTriggered = false;
        System.Action triggerImpact = () => {
            if (impactTriggered) return; // Prevent double-triggering
            impactTriggered = true;
            
            Transform targetTransform = endRect != null ? endRect.transform : endTransform;
            Debug.Log($"[CombatEffectManager] ⚡ Projectile arrived! Triggering impact/damage...");
            
            // Play impact effect first
            if (projectileData.hasImpactEffect && projectileData.impactEffect != null)
            {
                Debug.Log($"[CombatEffectManager] Playing impact effect: {projectileData.impactEffect.effectName}");
                PlayEffectFromData(projectileData.impactEffect, targetTransform);
            }
            
            // Then trigger damage numbers (if provided)
            if (onProjectileHit != null)
            {
                Debug.Log($"[CombatEffectManager] Triggering damage number callback");
                onProjectileHit.Invoke();
            }
        };
        
        // Create callback for manual animations (will call triggerImpact)
        System.Action onProjectileArrival = null;
        if (projectileData.hasImpactEffect && projectileData.impactEffect != null || onProjectileHit != null)
        {
            onProjectileArrival = triggerImpact;
        }
        
        Debug.Log($"[CombatEffectManager] Setting up projectile with startRect={startRect.name} (pos: {startRect.anchoredPosition}), endRect={endRect.name} (pos: {endRect.anchoredPosition})");
        Debug.Log($"[CombatEffectManager] End point is enemy: {IsEnemyTarget(endTransform)}, should use 'Default': {endPointName == "Default"}");
        
        // Validate enemy Default point setup
        if (IsEnemyTarget(endTransform))
        {
            if (EffectValidationHelper.ValidateEnemyEffectPoint(endTransform, out Transform validatedPoint, out string validationError))
            {
                Debug.Log($"[CombatEffectManager] ✓ Enemy Default point validated: {validatedPoint.name}");
                if (validatedPoint != endPoint)
                {
                    Debug.LogWarning($"[CombatEffectManager] ⚠ Using validated Default point instead of original");
                    endPoint = validatedPoint;
                    endRect = endPoint.GetComponent<RectTransform>();
                }
            }
            else
            {
                Debug.LogError($"[CombatEffectManager] ✗ Enemy Default point validation failed: {validationError}");
            }
        }
        
        // Ensure endRect is properly positioned at enemy's Default point
        if (IsEnemyTarget(endTransform) && endRect != null)
        {
            // Verify the endRect is actually at the enemy's Default point
            Vector2 endWorldPos = endRect.position;
            Debug.Log($"[CombatEffectManager] Enemy Default point world position: {endWorldPos}, anchored: {endRect.anchoredPosition}");
        }
        
        // Calculate canvas-space positions for accurate distance/timing calculation
        Vector2 startCanvasPos = GetAnchoredPositionInCanvas(startRect);
        Vector2 endCanvasPos = GetAnchoredPositionInCanvas(endRect);
        float distance = Vector2.Distance(startCanvasPos, endCanvasPos);
        float travelTime = distance / projectileData.projectileSpeed;
        
        Debug.Log($"[CombatEffectManager] Projectile travel calculation - startCanvasPos: {startCanvasPos}, endCanvasPos: {endCanvasPos}, distance: {distance:F1}, travelTime: {travelTime:F2}s");
        
        // Setup with UIParticleHelper (passes callback for manual animation)
        // Use FxCanvas if available, otherwise use targetCanvas
        Canvas effectsCanvas = GetEffectsCanvas();
        UIParticleHelper.SetupProjectile(projectile, projectileData, startRect, endRect, effectsCanvas, onProjectileArrival);
        
        Debug.Log($"[CombatEffectManager] Projectile setup complete. Active: {projectile.activeSelf}, Position: {projectile.transform.position}, Local: {projectile.transform.localPosition}");
        
        // Validate ParticleAttractor setup if it was used
        EffectValidationHelper.LogValidationResults(endTransform, projectile, endRect);
        
        // Also use time-based coroutine as fallback (works for ParticleAttractor which doesn't have callbacks)
        // This ensures impact plays even if callback isn't triggered
        // Use pre-calculated travel time based on canvas-space positions
        // Pass the triggerImpact action so it can check the flag to prevent double-triggering
        if (projectileData.hasImpactEffect && projectileData.impactEffect != null || onProjectileHit != null)
        {
            Debug.Log($"[CombatEffectManager] Starting impact/damage coroutine (travel time: {travelTime:F2}s) as fallback");
            StartCoroutine(PlayImpactOnProjectileArrival(projectileData.impactEffect, endRect, travelTime, triggerImpact));
        }
        else
        {
            Debug.LogWarning($"[CombatEffectManager] Projectile '{projectileData.effectName}' has no impact effect or damage callback configured");
        }
        
        // Auto-destroy
        Destroy(projectile, 5f); // Max duration
    }
    
    /// <summary>
    /// Play impact effect when projectile arrives (fallback for ParticleAttractor)
    /// </summary>
    private System.Collections.IEnumerator PlayImpactOnProjectileArrival(EffectData impactData, 
        RectTransform targetRect, float travelTime, System.Action triggerImpact)
    {
        if (targetRect == null)
        {
            Debug.LogError("[CombatEffectManager] PlayImpactOnProjectileArrival called with null targetRect!");
            yield break;
        }
        
        // Add small buffer to ensure projectile has visually arrived
        float waitTime = travelTime + 0.05f;
        
        Debug.Log($"[CombatEffectManager] Waiting {waitTime:F2}s before playing impact/damage (fallback coroutine)");
        
        yield return new WaitForSeconds(waitTime);
        
        // Trigger impact (will check flag internally to prevent double-triggering)
        if (triggerImpact != null)
        {
            Debug.Log($"[CombatEffectManager] ⚡ [FALLBACK] Triggering impact/damage via coroutine");
            triggerImpact.Invoke();
        }
    }
    
    /// <summary>
    /// Check if target is an enemy
    /// </summary>
    private bool IsEnemyTarget(Transform target)
    {
        if (target == null) return false;
        
        // Check if it's an EnemyCombatDisplay
        EnemyCombatDisplay enemyDisplay = target.GetComponent<EnemyCombatDisplay>();
        if (enemyDisplay != null) return true;
        
        // Check parent for EnemyCombatDisplay (in case target is a child)
        enemyDisplay = target.GetComponentInParent<EnemyCombatDisplay>();
        if (enemyDisplay != null) return true;
        
        return false;
    }
    
    /// <summary>
    /// Get effect point from target (EffectPointProvider, EnemyCombatDisplay, or fallback)
    /// Enemies always use "Default" point regardless of requested point name
    /// </summary>
    private Transform GetEffectPointFromTarget(Transform target, string pointName)
    {
        if (target == null) return null;
        
        // Enemies always use "Default" effect point (dynamic sprites can't use fixed positions)
        if (IsEnemyTarget(target))
        {
            pointName = "Default";
        }
        
        // Try EffectPointProvider first (works for character icons and enemies)
        // Use GetComponentInChildren because EffectPointProvider may be a child GameObject
        EffectPointProvider provider = target.GetComponentInChildren<EffectPointProvider>(includeInactive: false);
        if (provider != null)
        {
            Transform effectPoint = provider.GetEffectPoint(pointName);
            if (effectPoint != null)
            {
                Debug.Log($"[CombatEffectManager] Found EffectPointProvider on {target.name}, got point '{pointName}': {effectPoint.name}");
                return effectPoint;
            }
        }
        
        // Try EnemyCombatDisplay (if it has effect points)
        EnemyCombatDisplay enemyDisplay = target.GetComponent<EnemyCombatDisplay>();
        if (enemyDisplay != null)
        {
            // Check if EnemyCombatDisplay has EffectPointProvider (also search in children)
            provider = enemyDisplay.GetComponentInChildren<EffectPointProvider>(includeInactive: false);
            if (provider != null)
            {
                Transform effectPoint = provider.GetEffectPoint(pointName); // Will be "Default" due to check above
                if (effectPoint != null)
                {
                    return effectPoint;
                }
            }
        }
        
        // Fallback to target itself
        return target;
    }
    
    /// <summary>
    /// Get anchored position in canvas space
    /// Properly converts RectTransform position to the effects canvas local space
    /// </summary>
    private Vector2 GetAnchoredPositionInCanvas(RectTransform targetRect)
    {
        // Use FxCanvas if available, otherwise use targetCanvas
        Canvas effectsCanvas = GetEffectsCanvas();
        if (effectsCanvas == null || targetRect == null) 
        {
            return targetRect != null ? targetRect.anchoredPosition : Vector2.zero;
        }
        
        RectTransform canvasRect = effectsCanvas.GetComponent<RectTransform>();
        if (canvasRect == null) return targetRect.anchoredPosition;
        
        // Check if target is already in the same canvas
        Canvas targetCanvas = targetRect.GetComponentInParent<Canvas>();
        if (targetCanvas != null && targetCanvas == effectsCanvas)
        {
            // Same canvas - use anchored position directly (but need to account for parent hierarchy)
            // Get world position and convert to canvas local space
            Vector2 screenPoint = targetRect.position;
            Vector2 canvasPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                null, // No camera for Screen Space Overlay
                out canvasPos))
            {
                return canvasPos;
            }
        }
        
        // Different canvases - need to convert between them
        Vector2 screenPoint2;
        Camera cam = effectsCanvas.worldCamera;
        
        if (effectsCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // For Screen Space Overlay, RectTransform.position is already in screen coordinates
            screenPoint2 = targetRect.position;
            cam = null; // No camera needed for overlay
        }
        else
        {
            // For Camera or World Space canvases, convert world position to screen
            screenPoint2 = RectTransformUtility.WorldToScreenPoint(cam ?? Camera.main, targetRect.position);
        }
        
        // Convert screen point to canvas local space
        Vector2 canvasPos2;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint2,
            cam,
            out canvasPos2))
        {
            Debug.Log($"[CombatEffectManager] GetAnchoredPositionInCanvas - targetRect.anchoredPosition: {targetRect.anchoredPosition}, targetRect.position: {targetRect.position}, screenPoint: {screenPoint2}, canvasPos: {canvasPos2}, renderMode: {effectsCanvas.renderMode}, canvasSize: {canvasRect.sizeDelta}");
            return canvasPos2;
        }
        else
        {
            // Fallback: use screen position directly (assuming same canvas size)
            Debug.LogWarning($"[CombatEffectManager] Failed to convert screen point to canvas local space. Using screen position as fallback: {screenPoint2}");
            return screenPoint2;
        }
    }
    
    /// <summary>
    /// Find the currently spawned player character icon
    /// </summary>
    public Transform FindPlayerCharacterIcon()
    {
        // Try to find by common names
        string[] possibleNames = { "RangerIcon", "ThiefIcon", "WitchIcon", 
                                   "MarauderIcon", "BrawlerIcon", "ApostleIcon" };
        
        foreach (string name in possibleNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null) return found.transform;
        }
        
        // Try to find by PlayerPortrait parent
        GameObject playerPortrait = GameObject.Find("PlayerPortrait");
        if (playerPortrait != null && playerPortrait.transform.childCount > 0)
        {
            return playerPortrait.transform.GetChild(0);
        }
        
        return null;
    }
    
    #endregion
}
