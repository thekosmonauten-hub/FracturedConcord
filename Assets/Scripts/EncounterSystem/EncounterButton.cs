using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class EncounterButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Encounter Settings")]
    public int encounterID = 1;
    public string encounterName = "Where Night First Fell";
    
    [Header("UI References")]
    public Button button;
    public Text buttonText;
    [Header("Debug")]
    public bool verboseDebug = true;
    
    private void OnEnable()
    {
        TryAssignReferences();
        WireClickHandler();
        EnsureEventSystemAndRaycaster();
        UpdateButtonText();
        UpdateButtonState();
    }
    
    private void OnEncounterButtonClick()
    {
        if (verboseDebug) Debug.Log($"[EncounterButton] Click: {encounterName} (ID: {encounterID})");
        EncounterManager.Instance.StartEncounter(encounterID);
    }

    // Fallback: ensure pointer clicks register even if Button wiring fails
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
            // Auto-create an EventSystem compatible with the active input backend
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

        // Ensure the target graphic can receive raycasts
        if (button != null && button.targetGraphic != null)
        {
            button.targetGraphic.raycastTarget = true;
        }

        return ok;
    }

    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = encounterName;
            return;
        }

        // Support TextMeshProUGUI if used
        var tmp = GetComponentInChildren<TMP_Text>();
        if (tmp != null)
            tmp.text = encounterName;
    }
    
    private void UpdateButtonState()
    {
        if (button != null)
        {
            EncounterData encounter = EncounterManager.Instance.GetEncounterByID(encounterID);
            if (encounter != null)
            {
                button.interactable = encounter.isUnlocked;
                
                // Visual feedback for completed encounters
                if (encounter.isCompleted)
                {
                    // You can add visual indicators here (checkmark, different color, etc.)
                    Debug.Log($"Encounter {encounterID} is completed!");
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event listener
        if (button != null)
        {
            button.onClick.RemoveListener(OnEncounterButtonClick);
        }
    }
}
