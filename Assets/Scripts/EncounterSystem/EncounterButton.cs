using System;
using System.Text;
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
    [SerializeField] private EncounterDataAsset encounterAsset;
    public int encounterID = 1;
    public string encounterName = "Where Night First Fell";
    [Tooltip("When true the encounter is pulled from EncounterDataAsset or EncounterManager on enable.")]
    [SerializeField] private bool autoSyncEncounterData = true;

    [Header("Requirements")]
    [SerializeField] private bool requireUnlockedEncounter = true;
    [SerializeField] private bool requireSelectedCharacter = true;
    [SerializeField] private bool requireActiveDeck = true;

    [Header("UI References")]
    public Button button;
    [SerializeField] private Text buttonText;
    [SerializeField] private TMP_Text buttonTextTMP;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private CanvasGroup lockedOverlay;
    [SerializeField] private GameObject lockIcon;

    [Header("Events")]
    [SerializeField] private UnityEvent onEncounterStarted;

    [Header("Debug")]
    public bool verboseDebug = true;

    private bool _listenersHooked;

    private void Awake()
    {
        TryAssignReferences();
    }

    private void OnEnable()
    {
        TryAssignReferences();
        ResolveEncounterFromAsset();
        WireClickHandler();
        EnsureEventSystemAndRaycaster();
        HookManagerEvents();
        RefreshVisuals();
    }

    private void OnDisable()
    {
        UnhookManagerEvents();
    }

    private void OnEncounterButtonClick()
    {
        if (!EvaluateAvailability(out string reason))
        {
            if (verboseDebug)
            {
                Debug.LogWarning($"[EncounterButton] Encounter {encounterID} blocked. Reason: {reason}", this);
            }
            return;
        }

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

        if (buttonText == null)
            buttonText = GetComponentInChildren<Text>();

        if (buttonTextTMP == null)
            buttonTextTMP = GetComponentInChildren<TMP_Text>();
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
        }
    }

    private void RefreshVisuals()
    {
        UpdateButtonText();
        UpdateButtonState();
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

    private void UpdateButtonState()
    {
        bool canStart = EvaluateAvailability(out string reason);
        if (button != null)
        {
            button.interactable = canStart;
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

        EncounterManager encounterManager = EncounterManager.Instance;
        EncounterData data = encounterManager != null ? encounterManager.GetEncounterByID(encounterID) : null;

        if (requireUnlockedEncounter)
        {
            if (data == null)
            {
                sb.AppendLine("Encounter data missing.");
                allGood = false;
            }
            else if (!data.isUnlocked)
            {
                sb.AppendLine("Encounter locked.");
                allGood = false;
            }
        }

        if (requireSelectedCharacter)
        {
            var charMgr = CharacterManager.Instance;
            if (charMgr == null || charMgr.currentCharacter == null)
            {
                sb.AppendLine("Select a character first.");
                allGood = false;
            }
        }

        if (requireActiveDeck)
        {
            var deckMgr = DeckManager.Instance;
            if (deckMgr == null || !deckMgr.HasActiveDeck())
            {
                sb.AppendLine("Activate a deck in Deck Builder.");
                allGood = false;
            }
        }

        reason = sb.ToString().Trim();
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
        }
    }
#endif
}
