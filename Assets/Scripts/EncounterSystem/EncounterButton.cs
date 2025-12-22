using System;
using System.Collections;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class EncounterButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Encounter Settings")]
    [SerializeField] public EncounterDataAsset encounterAsset;
    public int encounterID = 1;
    public string encounterName = "Where Night First Fell";
    [Tooltip("When true the encounter is pulled from EncounterDataAsset or EncounterManager on enable.")]
    [SerializeField] public bool autoSyncEncounterData = true;

    [Header("Requirements")]
    [SerializeField] private bool requireUnlockedEncounter = true;
    [SerializeField] private bool requireSelectedCharacter = true;
    [SerializeField] private bool requireActiveDeck = true;

    [Header("UI References")]
    public Button button;
    [SerializeField] private Text buttonText;
    [SerializeField] private TMP_Text buttonTextTMP;
    [SerializeField] private TMP_Text areaLevelLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private TMP_Text wavePreviewLabel;
    [SerializeField] private CanvasGroup lockedOverlay;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Image encounterImage;

    [Header("Events")]
    [SerializeField] private UnityEvent onEncounterStarted;

    [Header("Debug")]
    public bool verboseDebug = true;

    [Header("Runtime Data")]
    public int areaLevel = 1;
    public string sceneName = "CombatScene";

    private bool _listenersHooked;
    private Sprite encounterSprite;
    private Coroutine retrySyncCoroutine;

    private void Awake()
    {
        TryAssignReferences();
    }

    private void Start()
    {
        // Delayed sync check - EncounterManager might not be initialized in OnEnable
        if (autoSyncEncounterData && encounterAsset == null && EncounterManager.Instance != null)
        {
            ResolveEncounterFromManager();
            RefreshVisuals();
        }
    }

    private void OnEnable()
    {
        TryAssignReferences();
        // Only sync from asset if autoSyncEncounterData is enabled
        if (autoSyncEncounterData)
        {
            ResolveEncounterFromAsset();
            // If asset is null or didn't provide data, try syncing from EncounterManager by ID
            if (encounterAsset == null)
            {
                ResolveEncounterFromManager();
            }
        }
        WireClickHandler();
        EnsureEventSystemAndRaycaster();
        HookManagerEvents();
        HookEncounterEvents();
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.EncounterGraphChanged += HandleEncounterGraphChanged;
            // Try syncing from manager again in case it wasn't initialized yet
            if (autoSyncEncounterData && encounterAsset == null)
            {
                ResolveEncounterFromManager();
            }
        }
        RefreshVisuals();
    }

    private void OnDisable()
    {
        UnhookManagerEvents();
        UnhookEncounterEvents();
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.EncounterGraphChanged -= HandleEncounterGraphChanged;
        }
        
        // Stop retry coroutine if running
        if (retrySyncCoroutine != null)
        {
            StopCoroutine(retrySyncCoroutine);
            retrySyncCoroutine = null;
        }
    }

    private void OnEncounterButtonClick()
    {
        Debug.Log($"[EncounterButton] ===== CLICK DETECTED =====");
        Debug.Log($"[EncounterButton] Encounter ID: {encounterID}");
        Debug.Log($"[EncounterButton] Encounter Name: {encounterName}");
        Debug.Log($"[EncounterButton] GameObject: {gameObject.name}");
        Debug.Log($"[EncounterButton] EncounterManager.Instance: {(EncounterManager.Instance != null ? "EXISTS" : "NULL")}");
        
        if (!EvaluateAvailability(out string reason))
        {
            Debug.LogError($"[EncounterButton] ===== ENCOUNTER {encounterID} BLOCKED =====");
            Debug.LogError($"[EncounterButton] Reason: {reason}");
            Debug.LogError($"[EncounterButton] Encounter ID: {encounterID}");
            Debug.LogError($"[EncounterButton] Encounter Name: {encounterName}");
            
            // Additional diagnostic info
            if (EncounterManager.Instance != null)
            {
                var data = EncounterManager.Instance.GetEncounterByID(encounterID);
                Debug.LogError($"[EncounterButton] EncounterData: {(data != null ? $"FOUND - {data.encounterName}" : "NULL")}");
                var prog = EncounterManager.Instance.GetProgression(encounterID);
                Debug.LogError($"[EncounterButton] Progression: {(prog != null ? $"Unlocked={prog.isUnlocked}, Completed={prog.isCompleted}" : "NULL")}");
                bool isUnlocked = EncounterManager.Instance.IsEncounterUnlocked(encounterID);
                Debug.LogError($"[EncounterButton] IsEncounterUnlocked({encounterID}): {isUnlocked}");
            }
            
            if (verboseDebug)
            {
                Debug.LogWarning($"[EncounterButton] Encounter {encounterID} blocked. Reason: {reason}", this);
            }
            return;
        }

        Debug.Log($"[EncounterButton] ===== ENCOUNTER {encounterID} AVAILABLE - STARTING =====");
        if (verboseDebug)
        {
            Debug.Log($"[EncounterButton] Click: {encounterName} (ID: {encounterID})", this);
        }

        EncounterManager.Instance.StartEncounter(encounterID);
        onEncounterStarted?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (verboseDebug) Debug.Log($"[EncounterButton] OnPointerClick detected on {gameObject.name} for ID {encounterID}");
            OnEncounterButtonClick();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (verboseDebug) Debug.Log($"[EncounterButton] PointerDown on {gameObject.name}");
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (verboseDebug) Debug.Log($"[EncounterButton] PointerEnter on {gameObject.name}");
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (verboseDebug) Debug.Log($"[EncounterButton] PointerExit on {gameObject.name}");
    }

    private void TryAssignReferences()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (button == null)
            button = GetComponentInChildren<Button>(true);

        if (buttonText == null)
            buttonText = GetComponentInChildren<Text>();

        if (buttonTextTMP == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            if (texts.Length > 0)
            {
                buttonTextTMP = texts[0];
            }
        }

        if (encounterImage == null)
        {
            encounterImage = GetComponent<Image>();
            if (encounterImage == null)
            {
                // Try to find Image component in children (including inactive)
                Image[] images = GetComponentsInChildren<Image>(true);
                encounterImage = images.FirstOrDefault();
                
                if (encounterImage == null && verboseDebug)
                {
                    Debug.LogWarning($"[EncounterButton] No Image component found for Encounter {encounterID} ({encounterName}). Searched in GameObject '{gameObject.name}' and all children.");
                }
                else if (encounterImage != null && verboseDebug)
                {
                    Debug.Log($"[EncounterButton] Found encounterImage for Encounter {encounterID} ({encounterName}) on GameObject '{encounterImage.gameObject.name}'");
                }
            }
        }

        if (areaLevelLabel == null)
        {
            TMP_Text[] labels = GetComponentsInChildren<TMP_Text>(true);
            foreach (var tmp in labels)
            {
                if (tmp == buttonTextTMP)
                    continue;
                if (tmp != null && tmp.name.IndexOf("area", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    areaLevelLabel = tmp;
                    break;
                }
            }
        }

        EnsureWavePreviewLabel();
    }

    private void EnsureWavePreviewLabel()
    {
        if (wavePreviewLabel != null)
            return;

        var existing = transform.Find("WavePreview");
        if (existing != null)
        {
            wavePreviewLabel = existing.GetComponent<TMP_Text>();
        }

        if (wavePreviewLabel == null)
        {
            var go = new GameObject("WavePreview", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -34f);
            rect.sizeDelta = new Vector2(220f, 24f);

            wavePreviewLabel = go.AddComponent<TextMeshProUGUI>();
            wavePreviewLabel.alignment = TextAlignmentOptions.Center;
            wavePreviewLabel.fontSize = buttonTextTMP != null
                ? Mathf.Clamp(buttonTextTMP.fontSize * 0.6f, 14f, 28f)
                : 20f;
            wavePreviewLabel.color = new Color(0.85f, 0.95f, 1f, 0.92f);
            wavePreviewLabel.raycastTarget = false;
        }

        if (wavePreviewLabel != null)
        {
            wavePreviewLabel.gameObject.SetActive(false);
        }
    }

    private void WireClickHandler()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnEncounterButtonClick);
            button.onClick.AddListener(OnEncounterButtonClick);
            if (verboseDebug) Debug.Log($"[EncounterDebug] EncounterButton: Wired onClick for '{encounterName}' (ID {encounterID}) on {gameObject.name}");
        }
        else
        {
            Debug.LogError($"[EncounterDebug] EncounterButton: No Button component found on {gameObject.name}; clicks will not register.");
        }
    }

    private bool EnsureEventSystemAndRaycaster()
    {
        bool ok = true;
        if (EventSystem.current == null)
        {
            var esGO = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            esGO.AddComponent<StandaloneInputModule>();
#endif
            Debug.LogWarning("[EncounterDebug] EventSystem was missing. Created one automatically.");
        }

        var parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("[EncounterDebug] No parent Canvas found; Button requires a Canvas to receive clicks.");
            ok = false;
        }
        else if (parentCanvas.GetComponent<GraphicRaycaster>() == null)
        {
            parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
            Debug.LogWarning("[EncounterDebug] Added missing GraphicRaycaster to parent Canvas.");
        }

        if (button != null && button.targetGraphic != null)
        {
            button.targetGraphic.raycastTarget = true;
        }

        return ok;
    }

    private void ResolveEncounterFromAsset()
    {
        if (encounterAsset != null)
        {
            encounterID = encounterAsset.encounterID;
            if (!string.IsNullOrWhiteSpace(encounterAsset.encounterName))
            {
                encounterName = encounterAsset.encounterName;
            }
            sceneName = string.IsNullOrWhiteSpace(encounterAsset.sceneName) ? sceneName : encounterAsset.sceneName;
            areaLevel = encounterAsset.areaLevel;
            
            // Always update sprite if asset has one
            if (encounterAsset.encounterSprite != null)
            {
                encounterSprite = encounterAsset.encounterSprite;
                if (verboseDebug)
                {
                    Debug.Log($"[EncounterButton] Loaded sprite from EncounterDataAsset for {encounterID} ({encounterName}): {encounterSprite.name}");
                }
            }
            else if (verboseDebug)
            {
                Debug.LogWarning($"[EncounterButton] EncounterDataAsset for {encounterID} ({encounterName}) has no encounterSprite assigned.");
            }
            
            UpdateEncounterSprite();
            UpdateAreaLevelText();
            UpdateButtonText();
        }
    }

    /// <summary>
    /// Resolve encounter data from EncounterManager using encounterID.
    /// This is used as a fallback when encounterAsset is not assigned.
    /// </summary>
    private void ResolveEncounterFromManager()
    {
        if (EncounterManager.Instance == null)
        {
            if (verboseDebug)
            {
                Debug.LogWarning($"[EncounterButton] EncounterManager.Instance is null. Cannot resolve encounter {encounterID} from manager.", this);
            }
            // Start retry coroutine if not already running
            if (retrySyncCoroutine == null)
            {
                retrySyncCoroutine = StartCoroutine(RetryResolveFromManager());
            }
            return;
        }

        // Check if EncounterManager has finished loading encounters
        if (!EncounterManager.Instance.IsInitialized)
        {
            if (verboseDebug)
            {
                Debug.Log($"[EncounterButton] EncounterManager not initialized yet. Will retry for encounter {encounterID}.", this);
            }
            // Start retry coroutine if not already running
            if (retrySyncCoroutine == null)
            {
                retrySyncCoroutine = StartCoroutine(RetryResolveFromManager());
            }
            return;
        }

        EncounterData data = EncounterManager.Instance.GetEncounterByID(encounterID);
        if (data != null)
        {
            ApplyEncounterData(data);
            if (verboseDebug)
            {
                Debug.Log($"[EncounterButton] Resolved encounter {encounterID} from EncounterManager: {data.encounterName}", this);
            }
            // Stop retry coroutine if data was found
            if (retrySyncCoroutine != null)
            {
                StopCoroutine(retrySyncCoroutine);
                retrySyncCoroutine = null;
            }
        }
        else
        {
            if (verboseDebug)
            {
                Debug.LogWarning($"[EncounterButton] Encounter {encounterID} not found in EncounterManager. Will retry.", this);
            }
            // Start retry coroutine if not already running
            if (retrySyncCoroutine == null)
            {
                retrySyncCoroutine = StartCoroutine(RetryResolveFromManager());
            }
        }
    }

    /// <summary>
    /// Retry resolving encounter data from manager with delays.
    /// </summary>
    private IEnumerator RetryResolveFromManager()
    {
        int maxRetries = 10;
        int retryCount = 0;
        float retryDelay = 0.1f;

        while (retryCount < maxRetries)
        {
            yield return new WaitForSeconds(retryDelay);
            retryCount++;

            if (EncounterManager.Instance != null && EncounterManager.Instance.IsInitialized)
            {
                EncounterData data = EncounterManager.Instance.GetEncounterByID(encounterID);
                if (data != null)
                {
                    ApplyEncounterData(data);
                    RefreshVisuals();
                    if (verboseDebug)
                    {
                        Debug.Log($"[EncounterButton] Successfully resolved encounter {encounterID} after {retryCount} retries: {data.encounterName}", this);
                    }
                    retrySyncCoroutine = null;
                    yield break;
                }
            }

            if (verboseDebug && retryCount < maxRetries)
            {
                Debug.Log($"[EncounterButton] Retry {retryCount}/{maxRetries} - Encounter {encounterID} still not available", this);
            }
        }

        Debug.LogError($"[EncounterButton] Failed to resolve encounter {encounterID} after {maxRetries} retries. EncounterManager may not have this encounter loaded.", this);
        retrySyncCoroutine = null;
    }

    private void RefreshVisuals()
    {
        if (verboseDebug)
        {
            Debug.Log($"[EncounterButton] RefreshVisuals called for Encounter {encounterID} on {gameObject.name}");
        }
        UpdateButtonText();
        UpdateButtonState();
        UpdateAreaLevelText();
        UpdateEncounterSprite();
        UpdateWavePreview();
    }

    private void UpdateWavePreview()
    {
        if (wavePreviewLabel == null)
            return;

        EncounterData data = EncounterManager.Instance != null
            ? EncounterManager.Instance.GetEncounterByID(encounterID)
            : null;

        if (data == null || data.totalWaves <= 0)
        {
            wavePreviewLabel.text = string.Empty;
            wavePreviewLabel.gameObject.SetActive(false);
            return;
        }

        var character = CharacterManager.Instance?.GetCurrentCharacter();
        float moveMultiplier = character != null ? character.GetMovementSpeedMultiplier() : 1f;
        moveMultiplier = Mathf.Max(1f, moveMultiplier); // only compress when faster

        int baseWaves = Mathf.Max(1, data.totalWaves);
        int compressedWaves = Mathf.Max(1, Mathf.CeilToInt(baseWaves / moveMultiplier));
        float compressionPercent = 1f - (float)compressedWaves / baseWaves;
        float movePercent = (moveMultiplier - 1f) * 100f;

        wavePreviewLabel.gameObject.SetActive(true);
        if (compressionPercent > 0.0001f)
        {
            wavePreviewLabel.text = $"Waves {baseWaves} → {compressedWaves} ({compressionPercent * 100f:+0;-0;0}% via MS +{movePercent:0}%)";
        }
        else
        {
            wavePreviewLabel.text = $"Waves {baseWaves} (MS +{movePercent:0}% )";
        }
    }

    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = encounterName;
        }
        if (buttonTextTMP != null)
        {
            buttonTextTMP.text = encounterName;
        }
    }

    private void UpdateAreaLevelText()
    {
        if (areaLevelLabel != null)
        {
            areaLevelLabel.text = areaLevel.ToString();
        }
    }

    private void UpdateEncounterSprite()
    {
        if (encounterImage != null)
        {
            encounterImage.sprite = encounterSprite;
            encounterImage.enabled = encounterSprite != null;
            
            if (verboseDebug)
            {
                if (encounterSprite != null)
                {
                    Debug.Log($"[EncounterButton] Set sprite for Encounter {encounterID} ({encounterName}): {encounterSprite.name}");
                }
                else
                {
                    Debug.LogWarning($"[EncounterButton] No sprite found for Encounter {encounterID} ({encounterName}). encounterImage will be disabled.");
                }
            }
        }
        else
        {
            if (verboseDebug)
            {
                Debug.LogWarning($"[EncounterButton] encounterImage is null for Encounter {encounterID} ({encounterName}). Cannot set sprite.");
            }
        }
    }

    private void UpdateButtonState()
    {
        if (verboseDebug)
        {
            Debug.Log($"[EncounterButton] UpdateButtonState for Encounter {encounterID}");
        }
        bool canStart = EvaluateAvailability(out string reason);
        
        if (verboseDebug)
        {
            Debug.Log($"[EncounterButton] UpdateButtonState result: canStart={canStart}, reason={(string.IsNullOrEmpty(reason) ? "None" : reason)}");
        }
        
        if (button != null)
        {
            button.interactable = canStart;
            if (verboseDebug)
            {
                Debug.Log($"[EncounterButton] Button interactable set to: {canStart}");
            }
        }

        if (lockedOverlay != null)
        {
            lockedOverlay.alpha = canStart ? 0f : 1f;
            lockedOverlay.blocksRaycasts = !canStart;
            lockedOverlay.interactable = !canStart;
        }
        if (lockIcon != null)
        {
            lockIcon.SetActive(!canStart);
            if (verboseDebug)
            {
                Debug.Log($"[EncounterButton] Lock icon active: {!canStart}");
            }
        }
        if (statusLabel != null)
        {
            statusLabel.text = canStart ? string.Empty : reason;
        }
    }

    private bool EvaluateAvailability(out string reason)
    {
        StringBuilder sb = new StringBuilder();
        bool allGood = true;

        if (verboseDebug)
        {
            Debug.Log($"[EncounterButton] EvaluateAvailability for Encounter {encounterID} ({encounterName}) on {gameObject.name}");
        }

        EncounterManager encounterManager = EncounterManager.Instance;
        if (verboseDebug)
        {
            Debug.Log($"[EncounterButton] EncounterManager.Instance: {(encounterManager != null ? "EXISTS" : "NULL")}");
        }

        EncounterData data = encounterManager != null ? encounterManager.GetEncounterByID(encounterID) : null;
        if (verboseDebug)
        {
            Debug.Log($"[EncounterButton] EncounterData for ID {encounterID}: {(data != null ? $"FOUND - {data.encounterName}" : "NULL")}");
        }

        // Only sync data from EncounterManager if autoSyncEncounterData is enabled
        // This ensures buttons without encounterAsset assigned can still get their data
        if (data != null && autoSyncEncounterData)
        {
            ApplyEncounterData(data);
        }
        // If we still don't have data and encounterAsset is null, try to resolve from manager
        else if (data == null && encounterAsset == null && autoSyncEncounterData && encounterManager != null)
        {
            ResolveEncounterFromManager();
            data = encounterManager.GetEncounterByID(encounterID);
        }

        if (requireUnlockedEncounter)
        {
            if (data == null)
            {
                // If data is null but manager exists and is initialized, try one more time
                if (encounterManager != null && encounterManager.IsInitialized)
                {
                    if (verboseDebug)
                    {
                        Debug.LogWarning($"[EncounterButton] Encounter {encounterID} data is NULL but manager is initialized. Attempting to resolve...");
                    }
                    ResolveEncounterFromManager();
                    data = encounterManager.GetEncounterByID(encounterID);
                }
                
                if (data == null)
                {
                    if (verboseDebug)
                    {
                        Debug.LogWarning($"[EncounterButton] Encounter {encounterID} data is NULL - cannot check availability. Manager initialized: {(encounterManager != null ? encounterManager.IsInitialized.ToString() : "N/A")}");
                    }
                    // For encounter 1, we can still check unlock status even without data
                    if (encounterID == 1 && encounterManager != null)
                    {
                        bool unlocked = encounterManager.IsEncounterUnlocked(1);
                        if (unlocked)
                        {
                            if (verboseDebug)
                            {
                                Debug.Log($"[EncounterButton] Encounter 1 is unlocked even without data. Allowing access.");
                            }
                            // Don't block encounter 1
                        }
                        else
                        {
                            sb.AppendLine("Encounter data missing.");
                            allGood = false;
                        }
                    }
                    else
                    {
                        sb.AppendLine("Encounter data missing.");
                        allGood = false;
                    }
                }
            }
            
            // If we have data now, check unlock status
            if (data != null)
            {
                // Encounter 1 is always unlocked - bypass check
                bool unlocked;
                if (encounterID == 1)
                {
                    unlocked = true;
                    if (verboseDebug)
                    {
                        Debug.Log($"[EncounterButton] Encounter 1 - FORCING UNLOCKED (bypassing all checks)");
                    }
                }
                else
                {
                    unlocked = encounterManager == null || encounterManager.IsEncounterUnlocked(encounterID);
                    if (verboseDebug)
                    {
                        Debug.Log($"[EncounterButton] Encounter {encounterID} - IsEncounterUnlocked returned: {unlocked}");
                    }
                }
                
                if (!unlocked)
                {
                    if (verboseDebug)
                    {
                        Debug.LogWarning($"[EncounterButton] Encounter {encounterID} is LOCKED. Checking why...");
                        if (encounterManager != null)
                        {
                            var progData = encounterManager.GetProgression(encounterID);
                            Debug.Log($"[EncounterButton] Progression data: {(progData != null ? $"Unlocked={progData.isUnlocked}, Completed={progData.isCompleted}" : "NULL")}");
                        }
                    }
                    sb.AppendLine("Complete the prerequisite encounters first.");
                    allGood = false;
                }
                else
                {
                    if (verboseDebug)
                    {
                        Debug.Log($"[EncounterButton] Encounter {encounterID} is UNLOCKED");
                    }
                }
            }
        }

        if (requireSelectedCharacter)
        {
            var charMgr = CharacterManager.Instance;
            if (charMgr == null || charMgr.currentCharacter == null)
            {
                if (verboseDebug)
                {
                    Debug.LogWarning($"[EncounterButton] Character check FAILED - CharacterManager: {(charMgr != null ? "EXISTS" : "NULL")}, Character: {(charMgr != null && charMgr.currentCharacter != null ? charMgr.currentCharacter.characterName : "NULL")}");
                }
                sb.AppendLine("Select a character first.");
                allGood = false;
            }
            else
            {
                if (verboseDebug)
                {
                    Debug.Log($"[EncounterButton] Character check PASSED - {charMgr.currentCharacter.characterName}");
                }
            }
        }

        if (requireActiveDeck)
        {
            var deckMgr = DeckManager.Instance;
            if (deckMgr == null || !deckMgr.HasActiveDeck())
            {
                if (verboseDebug)
                {
                    Debug.LogWarning($"[EncounterButton] Deck check FAILED - DeckManager: {(deckMgr != null ? "EXISTS" : "NULL")}, HasActiveDeck: {(deckMgr != null ? deckMgr.HasActiveDeck().ToString() : "N/A")}");
                }
                sb.AppendLine("Activate a deck in Deck Builder.");
                allGood = false;
            }
            else
            {
                if (verboseDebug)
                {
                    Debug.Log($"[EncounterButton] Deck check PASSED");
                }
            }
        }

        reason = sb.ToString().Trim();
        
        if (verboseDebug)
        {
            Debug.Log($"[EncounterButton] Final availability result for Encounter {encounterID}: {(allGood ? "AVAILABLE" : "BLOCKED")}. Reason: {(string.IsNullOrEmpty(reason) ? "None" : reason)}");
        }
        
        return allGood;
    }

    private void HookManagerEvents()
    {
        if (_listenersHooked)
            return;

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.OnDeckChanged += HandleDeckChanged;
        }
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharacterLoaded += HandleCharacterChanged;
            CharacterManager.Instance.OnCharacterSaved += HandleCharacterChanged;
        }
        _listenersHooked = true;
    }

    private void UnhookManagerEvents()
    {
        if (!_listenersHooked)
            return;

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.OnDeckChanged -= HandleDeckChanged;
        }
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharacterLoaded -= HandleCharacterChanged;
            CharacterManager.Instance.OnCharacterSaved -= HandleCharacterChanged;
        }
        _listenersHooked = false;
    }

    private void HookEncounterEvents()
    {
        // Subscribe to encounter system events to sync when data becomes available
        EncounterEvents.OnEncounterSystemInitialized += HandleSystemInitialized;
        EncounterEvents.OnEncounterDataLoaded += HandleDataLoaded;
    }

    private void UnhookEncounterEvents()
    {
        EncounterEvents.OnEncounterSystemInitialized -= HandleSystemInitialized;
        EncounterEvents.OnEncounterDataLoaded -= HandleDataLoaded;
    }

    private void HandleSystemInitialized()
    {
        // When system initializes, try to sync if we don't have an asset
        if (autoSyncEncounterData && encounterAsset == null)
        {
            if (verboseDebug)
            {
                Debug.Log($"[EncounterButton] System initialized - resolving encounter {encounterID} from manager", this);
            }
            ResolveEncounterFromManager();
            RefreshVisuals();
        }
    }

    private void HandleDataLoaded()
    {
        // When data loads, try to sync if we don't have an asset
        if (autoSyncEncounterData && encounterAsset == null)
        {
            if (verboseDebug)
            {
                Debug.Log($"[EncounterButton] Data loaded - resolving encounter {encounterID} from manager", this);
            }
            ResolveEncounterFromManager();
            RefreshVisuals();
        }
    }

    private void HandleDeckChanged(DeckPreset deck)
    {
        RefreshVisuals();
    }

    private void HandleCharacterChanged(Character character)
    {
        RefreshVisuals();
    }

    public void RefreshVisualsFromOutside()
    {
        RefreshVisuals();
    }

    private void ApplyEncounterData(EncounterData data)
    {
        if (data == null)
            return;

        encounterName = string.IsNullOrWhiteSpace(data.encounterName) ? encounterName : data.encounterName;
        encounterID = data.encounterID;
        sceneName = string.IsNullOrWhiteSpace(data.sceneName) ? sceneName : data.sceneName;
        areaLevel = data.areaLevel;
        
        // Always update sprite if data has one (don't skip if null, but do update if present)
        if (data.encounterSprite != null)
        {
            encounterSprite = data.encounterSprite;
            if (verboseDebug)
            {
                Debug.Log($"[EncounterButton] Loaded sprite from EncounterData for {encounterID} ({encounterName}): {encounterSprite.name}");
            }
        }
        else if (verboseDebug)
        {
            Debug.LogWarning($"[EncounterButton] EncounterData for {encounterID} ({encounterName}) has no encounterSprite assigned.");
        }

        UpdateButtonText();
        UpdateAreaLevelText();
        UpdateEncounterSprite();
    }

    private void HandleEncounterGraphChanged()
    {
        // If we don't have an asset assigned, try to sync from manager when graph changes
        if (autoSyncEncounterData && encounterAsset == null && EncounterManager.Instance != null)
        {
            ResolveEncounterFromManager();
        }
        RefreshVisuals();
    }

    private void OnDestroy()
    {
        UnhookManagerEvents();
        if (button != null)
        {
            button.onClick.RemoveListener(OnEncounterButtonClick);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        TryAssignReferences();
        if (autoSyncEncounterData)
        {
            ResolveEncounterFromAsset();
        }
        if (!Application.isPlaying)
        {
            UpdateButtonText();
            UpdateAreaLevelText();
            UpdateEncounterSprite();
        }
    }
#endif
}
