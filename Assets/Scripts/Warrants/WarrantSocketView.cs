using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Concrete socket UI with drag/drop affordances and state synchronization.
/// </summary>
public class WarrantSocketView : WarrantNodeView,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,
    IPointerClickHandler,
    IWarrantDragPayload
{
    [Header("Visual References")]
    [SerializeField] private GameObject highlight;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite emptyIcon;
    [SerializeField] private Color emptyIconTint = Color.white;
    [SerializeField] private Color assignedIconTint = Color.white;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Dependencies")]
    [SerializeField] private WarrantBoardStateController boardState;
    [SerializeField] private WarrantIconLibrary iconLibrary;
    [SerializeField] private WarrantLockerGrid lockerGrid;
    [SerializeField] private ItemTooltipManager tooltipManager;
    [SerializeField] private WarrantBoardRuntimeGraph runtimeGraph;
    [SerializeField] private Canvas dragCanvasOverride;
    [SerializeField, Min(0.5f)] private float dragGhostScale = 1f;
    
    [Header("Character Reference")]
    [SerializeField] private Character characterRef;

    private string assignedWarrantId;
    private RectTransform dragGhost;
    private Image dragGhostImage;
    private Canvas cachedRootCanvas;
    private bool isDragging;
    private PointerEventData lastPointerEvent;

    public string WarrantId => assignedWarrantId;
    Sprite IWarrantDragPayload.Icon => iconImage != null ? iconImage.sprite : null;

    protected override void Awake()
    {
        base.Awake();
        CacheCanvasGroup();
        ToggleHighlight(false);
        UpdateIconSprite();
        
        // Ensure the background Image can receive raycasts for drop detection
        if (BackgroundImage != null)
        {
            BackgroundImage.raycastTarget = true;
        }
        
        // Ensure iconImage can also receive raycasts if needed
        if (iconImage != null)
        {
            iconImage.raycastTarget = true;
        }
    }

    private void OnEnable()
    {
        // Refresh character reference when enabled (scene loaded/object activated)
        RefreshCharacterReference();
        SyncFromState();
        SyncLockState();
    }

    /// <summary>
    /// Injects runtime dependencies that cannot live on the prefab asset.
    /// </summary>
    public void Configure(WarrantBoardStateController stateController, WarrantIconLibrary iconLookup, WarrantLockerGrid locker, ItemTooltipManager tooltip, WarrantBoardRuntimeGraph graph)
    {
        boardState = stateController;
        runtimeGraph = graph;
        if (iconLookup != null)
        {
            iconLibrary = iconLookup;
        }
        if (locker != null)
        {
            lockerGrid = locker;
        }
        tooltipManager = tooltip ?? tooltipManager ?? ItemTooltipManager.Instance;
        
        // Refresh character reference (don't cache - always get latest)
        RefreshCharacterReference();
        
        SyncFromState();
        SyncLockState();
    }
    
    /// <summary>
    /// Refresh character reference from CharacterManager (always gets latest)
    /// </summary>
    private void RefreshCharacterReference()
    {
        var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
        if (charManager != null && charManager.HasCharacter())
        {
            characterRef = charManager.GetCurrentCharacter();
            if (characterRef != null)
            {
                Debug.Log($"[WarrantSocketView] Refreshed character reference: {characterRef.characterName} (Level {characterRef.level}, Skill Points: {characterRef.skillPoints})");
            }
        }
        else
        {
            characterRef = null;
            Debug.LogWarning("[WarrantSocketView] Could not refresh character reference - CharacterManager not found or no character loaded.");
        }
    }

    private void SyncLockState()
    {
        if (boardState == null || string.IsNullOrEmpty(NodeId))
        {
            SetLocked(true);
            return;
        }

        bool unlocked = boardState.IsNodeUnlocked(NodeId);
        SetLocked(!unlocked);
    }

    public override void Initialize(string nodeId, WarrantNodeType type, Color color)
    {
        base.Initialize(nodeId, type, color);
        SyncFromState();
    }

    public override void SetLocked(bool locked)
    {
        base.SetLocked(locked);
        
        // While locked, hide the icon image to avoid showing a white square.
        if (iconImage != null)
        {
            if (locked)
            {
                iconImage.enabled = false;
            }
            else
            {
                // When unlocked, restore icon visibility based on current assignment/empty icon
                UpdateIconSprite();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right click: Return warrant to locker
            ReturnAssignedWarrantToLocker();
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Left click: Try to unlock if locked
            if (IsLocked)
            {
                TryUnlockNode();
            }
        }
    }

    private void TryUnlockNode()
    {
        if (boardState == null || runtimeGraph == null || string.IsNullOrEmpty(NodeId))
            return;

        // Always refresh character reference to get latest skill points
        var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
        if (charManager == null)
        {
            Debug.LogWarning("[WarrantSocketView] Cannot unlock: CharacterManager not found.");
            return;
        }
        
        if (!charManager.HasCharacter())
        {
            Debug.LogWarning("[WarrantSocketView] Cannot unlock: No character loaded.");
            return;
        }
        
        // Get fresh character reference (don't cache - always get latest)
        Character currentCharacter = charManager.GetCurrentCharacter();
        if (currentCharacter == null)
        {
            Debug.LogWarning("[WarrantSocketView] Cannot unlock: Character is null.");
            return;
        }

        // Get current skill points from character
        int skillPoints = currentCharacter.skillPoints;
        Debug.Log($"[WarrantSocketView] Attempting to unlock {NodeId}. Current skill points: {skillPoints}");
        
        if (boardState.TryUnlockNode(NodeId, runtimeGraph, ref skillPoints))
        {
            // Update character's skill points
            currentCharacter.skillPoints = skillPoints;
            
            // Update cached reference for consistency
            characterRef = currentCharacter;
            
            // Save character
            charManager.SaveCharacter();
            
            SyncLockState();
            Debug.Log($"[WarrantSocketView] ✅ Unlocked node {NodeId}. Remaining skill points: {skillPoints}");
            
            // Refresh character warrant modifiers (unlocking socket nodes allows warrant assignment)
            if (charManager != null && charManager.HasCharacter())
            {
                charManager.GetCurrentCharacter().RefreshWarrantModifiers();
            }
        }
        else
        {
            if (boardState.IsNodeUnlocked(NodeId))
            {
                Debug.Log($"[WarrantSocketView] Node {NodeId} is already unlocked.");
            }
            else if (skillPoints < 1)
            {
                Debug.LogWarning($"[WarrantSocketView] ❌ Cannot unlock {NodeId}: Not enough skill points (need 1, have {skillPoints}). Character: {currentCharacter.characterName}, Level: {currentCharacter.level}");
            }
            else
            {
                Debug.LogWarning($"[WarrantSocketView] Cannot unlock {NodeId}: No adjacent unlocked nodes found.");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        lastPointerEvent = eventData;
        ToggleHighlight(true);
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging)
        {
            ToggleHighlight(false);
            HideTooltip();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Can't drag from locked nodes
        if (IsLocked || !HasPayload || eventData == null)
            return;

        isDragging = true;
        HideTooltip();
        ToggleHighlight(true);
        CacheCanvasGroup();
        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;
        CreateDragGhost();
        UpdateDragGhostPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || eventData == null)
            return;

        UpdateDragGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;
        ToggleHighlight(false);
        lastPointerEvent = eventData;
        CacheCanvasGroup();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        DestroyDragGhost();
        ShowTooltip();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Can't drop on locked nodes
        if (IsLocked)
        {
            Debug.LogWarning($"[WarrantSocketView] Cannot drop on locked node {NodeId}");
            return;
        }

        var payload = GetPayload(eventData);
        if (payload == null)
        {
            Debug.LogWarning($"[WarrantSocketView] OnDrop: No payload found for node {NodeId}");
            return;
        }

        if (string.IsNullOrEmpty(payload.WarrantId))
        {
            Debug.LogWarning($"[WarrantSocketView] OnDrop: Payload has no WarrantId for node {NodeId}");
            return;
        }

        // Prevent dropping a socket onto itself
        if ((object)payload == (object)this && eventData.pointerDrag == gameObject)
            return;

        var replaced = assignedWarrantId;
        Debug.Log($"[WarrantSocketView] Dropping warrant {payload.WarrantId} onto socket {NodeId} (replacing {replaced ?? "nothing"})");
        
        if (TryAssignInternal(payload.WarrantId))
        {
            payload.OnAssignmentAccepted(this, replaced);
            Debug.Log($"[WarrantSocketView] Successfully assigned warrant {payload.WarrantId} to socket {NodeId}");
            ShowTooltip();
        }
        else
        {
            Debug.LogWarning($"[WarrantSocketView] Failed to assign warrant {payload.WarrantId} to socket {NodeId}");
        }
    }

    public void OnAssignmentAccepted(WarrantSocketView target, string replacedWarrantId)
    {
        if (target == this)
            return;

        if (string.IsNullOrEmpty(replacedWarrantId))
        {
            ClearAssignment();
        }
        else
        {
            TryAssignInternal(replacedWarrantId);
        }
        ShowTooltip();
    }

    private bool HasPayload => !string.IsNullOrEmpty(assignedWarrantId);

    /// <summary>
    /// Syncs the socket view's assigned warrant from the board state.
    /// Called after loading state to ensure socketed warrants are displayed correctly.
    /// </summary>
    public void SyncFromState()
    {
        if (boardState == null || string.IsNullOrEmpty(NodeId))
        {
            assignedWarrantId = null;
            UpdateIconSprite();
            return;
        }

        assignedWarrantId = boardState.GetWarrantAtNode(NodeId);
        UpdateIconSprite();
        
        // Debug logging to trace socket state syncing
        if (!string.IsNullOrEmpty(assignedWarrantId))
        {
            Debug.Log($"[WarrantSocketView] Synced socket '{NodeId}': assigned warrant = '{assignedWarrantId}'");
        }
    }

    private bool ClearAssignment()
    {
        if (string.IsNullOrEmpty(assignedWarrantId))
        {
            return false;
        }

        if (string.IsNullOrEmpty(NodeId) || boardState == null)
        {
            assignedWarrantId = null;
            UpdateIconSprite();
            HideTooltip();
            return true;
        }

        if (boardState.TryRemoveWarrant(NodeId))
        {
            assignedWarrantId = null;
            UpdateIconSprite();
            HideTooltip();
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Clears the assignment when dropping back into the locker (free swapping).
    /// Returns the removed warrant id if successful.
    /// </summary>
    public string ClearAssignmentForLockerReturn()
    {
        var removedId = assignedWarrantId;
        if (ClearAssignment())
        {
            return removedId;
        }

        return null;
    }

    private void ReturnAssignedWarrantToLocker()
    {
        if (string.IsNullOrEmpty(assignedWarrantId))
            return;

        var warrantId = assignedWarrantId;
        if (ClearAssignment())
        {
            lockerGrid?.ReturnWarrantToLocker(warrantId);
        }
    }

    private void ShowTooltip()
    {
        if (tooltipManager == null)
        {
            tooltipManager = ItemTooltipManager.Instance;
        }

        var definition = GetAssignedDefinition();
        if (definition == null)
        {
            tooltipManager?.HideTooltip();
            return;
        }

        var subtitle = $"Socket: {NodeId}";
        var notableDb = lockerGrid?.GetNotableDatabase();
        var data = WarrantTooltipUtility.BuildSingleWarrantData(definition, subtitle, notableDb);
        if (data == null)
        {
            tooltipManager?.HideTooltip();
            return;
        }

        var position = lastPointerEvent != null ? lastPointerEvent.position : (Vector2)Input.mousePosition;
        tooltipManager?.ShowWarrantTooltip(data, position);
    }

    private void HideTooltip()
    {
        tooltipManager?.HideTooltip();
    }

    private WarrantDefinition GetAssignedDefinition()
    {
        if (string.IsNullOrEmpty(assignedWarrantId))
            return null;

        return lockerGrid?.GetDefinition(assignedWarrantId);
    }

    private bool TryAssignInternal(string warrantId)
    {
        if (boardState == null || string.IsNullOrEmpty(NodeId))
            return false;

        bool result;
        if (string.IsNullOrEmpty(warrantId))
        {
            result = boardState.TryRemoveWarrant(NodeId);
            if (result)
            {
                assignedWarrantId = null;
            }
        }
        else
        {
            result = boardState.TryAssignWarrant(NodeId, warrantId);
            if (result)
            {
                assignedWarrantId = warrantId;
            }
        }

        if (result)
        {
            UpdateIconSprite();
        }

        return result;
    }

    private void UpdateIconSprite()
    {
        if (iconImage == null)
            return;

        // Do not show any icon visuals while locked
        if (IsLocked)
        {
            iconImage.enabled = false;
            return;
        }

		Sprite sprite = null;
		if (!string.IsNullOrEmpty(assignedWarrantId))
		{
			// Prefer database (via lockerGrid) as source of truth
			if (lockerGrid != null)
			{
				var def = lockerGrid.GetDefinition(assignedWarrantId);
				if (def != null && def.icon != null)
				{
					sprite = def.icon;
					Debug.Log($"[WarrantSocketView] UpdateIconSprite for '{NodeId}': Found warrant definition '{assignedWarrantId}' with icon");
				}
				else if (def == null)
				{
					Debug.LogWarning($"[WarrantSocketView] UpdateIconSprite for '{NodeId}': Warrant definition not found for '{assignedWarrantId}' (lockerGrid.GetDefinition returned null). Locker grid may not have loaded warrants yet.");
				}
				else if (def.icon == null)
				{
					Debug.LogWarning($"[WarrantSocketView] UpdateIconSprite for '{NodeId}': Warrant definition found for '{assignedWarrantId}' but icon is null");
				}
			}
			else
			{
				Debug.LogWarning($"[WarrantSocketView] UpdateIconSprite for '{NodeId}': lockerGrid is null, cannot retrieve warrant definition for '{assignedWarrantId}'");
			}
			
			// Fallback to icon library only if database has no icon
			if (sprite == null && iconLibrary != null)
			{
				sprite = iconLibrary.GetIcon(assignedWarrantId);
				if (sprite != null)
				{
					Debug.Log($"[WarrantSocketView] UpdateIconSprite for '{NodeId}': Using fallback icon from iconLibrary for '{assignedWarrantId}'");
				}
			}
		}

        if (sprite == null)
        {
            iconImage.sprite = emptyIcon;
            iconImage.color = emptyIconTint;
            iconImage.enabled = emptyIcon != null;
            if (!string.IsNullOrEmpty(assignedWarrantId))
            {
                Debug.LogWarning($"[WarrantSocketView] UpdateIconSprite for '{NodeId}': No sprite found for assigned warrant '{assignedWarrantId}', showing empty icon");
            }
        }
        else
        {
            iconImage.sprite = sprite;
            iconImage.color = assignedIconTint;
            iconImage.enabled = true;
        }
    }

    private void ToggleHighlight(bool value)
    {
        if (highlight != null)
        {
            highlight.SetActive(value);
        }
    }

    private void CacheCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void CreateDragGhost()
    {
        DestroyDragGhost();
        var canvas = dragCanvasOverride != null ? dragCanvasOverride : GetRootCanvas();
        if (canvas == null || iconImage == null || iconImage.sprite == null)
            return;

        var ghostObj = new GameObject("WarrantDragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        ghostObj.transform.SetParent(canvas.transform, false);
        dragGhost = ghostObj.GetComponent<RectTransform>();
        dragGhostImage = ghostObj.GetComponent<Image>();
        dragGhostImage.sprite = iconImage.sprite;
        dragGhostImage.color = assignedIconTint;
        dragGhostImage.raycastTarget = false;

        var group = ghostObj.GetComponent<CanvasGroup>();
        group.alpha = 0.9f;

        dragGhost.sizeDelta = iconImage.rectTransform.rect.size * dragGhostScale;
    }

    private void DestroyDragGhost()
    {
        if (dragGhost != null)
        {
            Destroy(dragGhost.gameObject);
            dragGhost = null;
            dragGhostImage = null;
        }
    }

    private void UpdateDragGhostPosition(PointerEventData eventData)
    {
        if (dragGhost == null || eventData == null)
            return;

        dragGhost.position = eventData.position;
    }

    private Canvas GetRootCanvas()
    {
        if (cachedRootCanvas == null)
        {
            cachedRootCanvas = GetComponentInParent<Canvas>();
        }
        return cachedRootCanvas;
    }

    private static IWarrantDragPayload GetPayload(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null)
            return null;

        return eventData.pointerDrag.GetComponent<IWarrantDragPayload>();
    }
}


