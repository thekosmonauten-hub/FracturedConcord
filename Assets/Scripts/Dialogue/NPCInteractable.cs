using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Reflection;
using DG.Tweening;

/// <summary>
/// Component for NPCs that can be clicked to start dialogue.
/// </summary>
public class NPCInteractable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("NPC Info")]
    [SerializeField] private string npcId;
    [SerializeField] private string npcName;
    [SerializeField] private Sprite npcPortrait;
    
    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject hoverIndicator;
    [SerializeField] private bool showHoverEffect = true;
    
    [Header("Click Animation")]
    [SerializeField] private GameObject characterObject; // PeacekeeperJoreg GameObject
    [SerializeField] private RectTransform backgroundRectTransform; // Background RectTransform
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Ease scaleEase = Ease.OutQuad;
    [SerializeField] private Ease backgroundEase = Ease.OutQuad;
    
    // Store original values for reverse animation
    private Vector3 originalCharacterScale;
    private Vector3 originalBackgroundScale;
    private bool hasStoredOriginalValues = false;
    
    private void Start()
    {
        if (hoverIndicator != null)
        {
            hoverIndicator.SetActive(false);
        }
        
        // Ensure we have a Graphic component for pointer events
        var image = GetComponent<Image>();
        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
            image.color = new Color(1, 1, 1, 0.01f); // Nearly transparent for raycasting
            Debug.Log($"[NPCInteractable] Added Image component to '{npcName}' for raycasting");
        }
        if (image != null)
        {
            image.raycastTarget = true;
            
            // Verify EventSystem exists
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                Debug.LogError($"[NPCInteractable] No EventSystem found in scene! NPC '{npcName}' clicks will not work. Add an EventSystem GameObject to the scene.");
            }
            
            // Verify Canvas exists in hierarchy
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning($"[NPCInteractable] NPC '{npcName}' is not under a Canvas! UI clicks may not work. Make sure the NPC is a child of a Canvas.");
            }
            else
            {
                // Verify GraphicRaycaster exists
                if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                {
                    Debug.LogWarning($"[NPCInteractable] Canvas '{canvas.name}' is missing GraphicRaycaster component! UI clicks may not work.");
                }
            }
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[NPCInteractable] OnPointerClick called for '{npcName}' ({npcId})");
        PlayClickAnimation(() => {
            StartDialogue();
        });
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (showHoverEffect && hoverIndicator != null)
        {
            hoverIndicator.SetActive(true);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverIndicator != null)
        {
            hoverIndicator.SetActive(false);
        }
    }
    
    public void StartDialogue()
    {
        Debug.Log($"[NPCInteractable] StartDialogue() called for '{npcName}' ({npcId})");
        
        if (dialogueData == null)
        {
            Debug.LogWarning($"[NPCInteractable] No dialogue data assigned to NPC '{npcName}' ({npcId})");
            return;
        }
        
        // Activate DialoguePanel before starting dialogue
        ActivateDialoguePanel();
        
        if (DialogueManager.Instance != null)
        {
            Debug.Log($"[NPCInteractable] Starting dialogue '{dialogueData.dialogueId}' via DialogueManager");
            DialogueManager.Instance.StartDialogue(dialogueData, this); // Pass self as NPC reference
        }
        else
        {
            Debug.LogError("[NPCInteractable] DialogueManager.Instance is null! Cannot start dialogue. Make sure DialogueManager exists in the scene.");
        }
    }
    
    /// <summary>
    /// Finds and activates the DialoguePanel in the scene
    /// </summary>
    private void ActivateDialoguePanel()
    {
        // Try to find DialogueUI component first
        DialogueUI dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (dialogueUI != null)
        {
            // Get the dialoguePanel GameObject from DialogueUI
            GameObject panel = dialogueUI.gameObject;
            
            // Try to get the dialoguePanel field via reflection (since it's private)
            var panelField = typeof(DialogueUI).GetField("dialoguePanel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (panelField != null)
            {
                var panelObj = panelField.GetValue(dialogueUI) as GameObject;
                if (panelObj != null)
                {
                    panel = panelObj;
                }
            }
            
            // Activate the panel and all its parents
            if (panel != null)
            {
                // Activate entire parent chain (including Menus, TownCanvas, etc.)
                Transform current = panel.transform;
                while (current != null)
                {
                    if (!current.gameObject.activeSelf)
                    {
                        Debug.Log($"[NPCInteractable] Activating parent: {current.name}");
                        current.gameObject.SetActive(true);
                    }
                    current = current.parent;
                }
                
                // Also ensure Canvas is active (find it even if parents are inactive)
                Canvas canvas = panel.GetComponentInParent<Canvas>(true);
                if (canvas != null && !canvas.gameObject.activeSelf)
                {
                    Debug.Log($"[NPCInteractable] Activating Canvas: {canvas.name}");
                    canvas.gameObject.SetActive(true);
                }
                
                Debug.Log($"[NPCInteractable] Activated DialoguePanel '{panel.name}' and all parents");
            }
        }
        else
        {
            // Fallback: try to find by name
            GameObject panel = GameObject.Find("DialoguePanel");
            if (panel != null)
            {
                // Activate entire parent chain
                Transform current = panel.transform;
                while (current != null)
                {
                    if (!current.gameObject.activeSelf)
                    {
                        current.gameObject.SetActive(true);
                    }
                    current = current.parent;
                }
                
                Debug.Log($"[NPCInteractable] Found and activated DialoguePanel by name");
            }
            else
            {
                Debug.LogWarning("[NPCInteractable] Could not find DialoguePanel in scene. Dialogue may not display.");
            }
        }
    }
    
    /// <summary>
    /// Alternative method to start dialogue - can be called from Button onClick event
    /// </summary>
    public void OnButtonClick()
    {
        Debug.Log($"[NPCInteractable] OnButtonClick() called for '{npcName}'");
        PlayClickAnimation(() => {
            StartDialogue();
        });
    }
    
    /// <summary>
    /// Plays animation when NPC is clicked: scales character up and zooms background out
    /// </summary>
    private void PlayClickAnimation(System.Action onComplete = null)
    {
        Sequence animationSequence = DOTween.Sequence();
        
        // Animate character scale to (1, 1, 1)
        if (characterObject != null)
        {
            originalCharacterScale = characterObject.transform.localScale;
            hasStoredOriginalValues = true;
            Debug.Log($"[NPCInteractable] Animating character scale from {originalCharacterScale} to (1, 1, 1)");
            
            animationSequence.Join(
                characterObject.transform.DOScale(Vector3.one, animationDuration)
                    .SetEase(scaleEase)
            );
        }
        else
        {
            Debug.LogWarning($"[NPCInteractable] Character object not assigned for '{npcName}'. Skipping scale animation.");
        }
        
        // Animate background scale from 1.2x, 1.2y to 1x, 1y
        if (backgroundRectTransform != null)
        {
            originalBackgroundScale = backgroundRectTransform.localScale;
            hasStoredOriginalValues = true;
            Debug.Log($"[NPCInteractable] Animating background scale from {originalBackgroundScale} to (1, 1, 1)");
            
            // Animate scale to (1, 1, 1)
            animationSequence.Join(
                backgroundRectTransform.DOScale(Vector3.one, animationDuration)
                    .SetEase(backgroundEase)
            );
        }
        else
        {
            Debug.LogWarning($"[NPCInteractable] Background RectTransform not assigned for '{npcName}'. Skipping background animation.");
        }
        
        // Call onComplete when animation finishes
        if (onComplete != null)
        {
            animationSequence.OnComplete(() => {
                Debug.Log($"[NPCInteractable] Click animation complete for '{npcName}'");
                onComplete();
            });
        }
        
        // Start the animation
        if (animationSequence.Duration() > 0)
        {
            Debug.Log($"[NPCInteractable] Starting click animation for '{npcName}' (duration: {animationDuration}s)");
            animationSequence.Play();
        }
        else
        {
            // No animations to play, just call onComplete immediately
            Debug.Log($"[NPCInteractable] No animations configured, calling onComplete immediately");
            onComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Plays reverse animation to reset character and background to original positions
    /// </summary>
    public void PlayReverseAnimation(System.Action onComplete = null)
    {
        if (!hasStoredOriginalValues)
        {
            Debug.LogWarning($"[NPCInteractable] Cannot play reverse animation - original values not stored. NPC: '{npcName}'");
            onComplete?.Invoke();
            return;
        }
        
        Sequence animationSequence = DOTween.Sequence();
        
        // Animate character scale back to original
        if (characterObject != null)
        {
            Debug.Log($"[NPCInteractable] Reversing character scale from {characterObject.transform.localScale} to {originalCharacterScale}");
            
            animationSequence.Join(
                characterObject.transform.DOScale(originalCharacterScale, animationDuration)
                    .SetEase(scaleEase)
            );
        }
        
        // Animate background scale back to original
        if (backgroundRectTransform != null)
        {
            Debug.Log($"[NPCInteractable] Reversing background scale from {backgroundRectTransform.localScale} to {originalBackgroundScale}");
            
            // Animate scale back to original
            animationSequence.Join(
                backgroundRectTransform.DOScale(originalBackgroundScale, animationDuration)
                    .SetEase(backgroundEase)
            );
        }
        
        // Call onComplete when animation finishes
        if (onComplete != null)
        {
            animationSequence.OnComplete(() => {
                Debug.Log($"[NPCInteractable] Reverse animation complete for '{npcName}'");
                onComplete();
            });
        }
        
        // Start the animation
        if (animationSequence.Duration() > 0)
        {
            Debug.Log($"[NPCInteractable] Starting reverse animation for '{npcName}' (duration: {animationDuration}s)");
            animationSequence.Play();
        }
        else
        {
            // No animations to play, just call onComplete immediately
            Debug.Log($"[NPCInteractable] No reverse animations configured, calling onComplete immediately");
            onComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Kill all active DOTween animations on this NPC to prevent errors during scene transitions
    /// </summary>
    public void KillAllAnimations()
    {
        if (characterObject != null)
        {
            characterObject.transform.DOKill();
        }
        if (backgroundRectTransform != null)
        {
            backgroundRectTransform.DOKill();
        }
    }
    
    private void OnDestroy()
    {
        // Kill any active DOTween animations to prevent memory leaks
        if (characterObject != null)
        {
            characterObject.transform.DOKill();
        }
        if (backgroundRectTransform != null)
        {
            backgroundRectTransform.DOKill();
        }
    }
    
    // Public setters for runtime assignment
    public void SetDialogueData(DialogueData dialogue)
    {
        dialogueData = dialogue;
    }
    
    public string GetNPCId() => npcId;
    public string GetNPCName() => npcName;
    public Sprite GetNPCPortrait() => npcPortrait;
    public DialogueData GetDialogueData() => dialogueData;
}

