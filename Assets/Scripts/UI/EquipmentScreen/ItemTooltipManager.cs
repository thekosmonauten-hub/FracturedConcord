using Dexiled.Data.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Centralised entry point for showing equipment-related tooltips (weapon, armour, effigy).
/// Instantiate this on a scene object and assign the tooltip prefabs + canvas in the inspector.
/// </summary>
public class ItemTooltipManager : MonoBehaviour
{
    public static ItemTooltipManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private RectTransform tooltipContainer;

    [Header("Tooltip Prefabs")]
    [SerializeField] private GameObject weaponTooltipPrefab;
    [SerializeField] private GameObject equipmentTooltipPrefab;
    [SerializeField] private GameObject effigyTooltipPrefab;
    [SerializeField] private GameObject cardTooltipPrefab;
    [SerializeField] private GameObject currencyTooltipPrefab;
    [SerializeField] private GameObject warrantTooltipPrefab;

    [Header("Positioning")]
    [SerializeField] private float cursorHorizontalOffset = 32f;
    [SerializeField] private float cursorVerticalOffset = 0f;
    [SerializeField] private float screenEdgePadding = 16f;
    
    // Minimum gap to maintain between cursor and tooltip to prevent overlap
    private const float minHorizontalGap = 52f; // cursorHorizontalOffset (32f) + extra gap (20f)

    private GameObject activeTooltip;

    /// <summary>
    /// Ensure singleton style access (scene scoped).
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[ItemTooltipManager] Duplicate detected on {gameObject.name}. Destroying the new instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInParent<Canvas>();
        }

        if (tooltipContainer == null && targetCanvas != null)
        {
            tooltipContainer = targetCanvas.transform as RectTransform;
        }
    }

    /// <summary>
    /// Show tooltip for a weapon item.
    /// </summary>
    public void ShowWeaponTooltip(WeaponItem weapon, Vector2 screenPosition)
    {
        if (weapon == null)
        {
            Debug.LogWarning("[ItemTooltipManager] ShowWeaponTooltip called with null weapon.");
            return;
        }

        ShowTooltipInternal(weaponTooltipPrefab, screenPosition, tooltip =>
        {
            var view = tooltip.GetComponent<WeaponTooltipView>();
            if (view == null)
            {
                Debug.LogWarning("[ItemTooltipManager] WeaponTooltipPrefab missing WeaponTooltipView component.");
                return;
            }

            view.SetData(weapon);
        });
    }

    /// <summary>
    /// Show tooltip for general equipment (armour, jewellery, off-hand, etc).
    /// </summary>
    public void ShowEquipmentTooltip(BaseItem item, Vector2 screenPosition)
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemTooltipManager] ShowEquipmentTooltip called with null item.");
            return;
        }

        if (item is WeaponItem weaponItem)
        {
            ShowWeaponTooltip(weaponItem, screenPosition);
            return;
        }

        if (item is Effigy effigyItem)
        {
            ShowEffigyTooltip(effigyItem, screenPosition);
            return;
        }

        ShowTooltipInternal(equipmentTooltipPrefab, screenPosition, tooltip =>
        {
            var view = tooltip.GetComponent<EquipmentTooltipView>();
            if (view == null)
            {
                Debug.LogWarning("[ItemTooltipManager] EquipmentTooltipPrefab missing EquipmentTooltipView component.");
                return;
            }

            view.SetData(item);
        });
    }

    /// <summary>
    /// Show tooltip for equipment represented by ItemData (legacy pipeline).
    /// </summary>
    public void ShowEquipmentTooltip(ItemData itemData, Vector2 screenPosition)
    {
        if (itemData == null)
        {
            HideTooltip();
            return;
        }

        if (itemData.sourceItem is WeaponItem weapon)
        {
            ShowWeaponTooltip(weapon, screenPosition);
            return;
        }

        if (itemData.sourceItem is Effigy effigy)
        {
            ShowEffigyTooltip(effigy, screenPosition);
            return;
        }

        if (itemData.sourceItem is BaseItem baseItem)
        {
            ShowEquipmentTooltip(baseItem, screenPosition);
            return;
        }

        ShowTooltipInternal(equipmentTooltipPrefab, screenPosition, tooltip =>
        {
            var view = tooltip.GetComponent<EquipmentTooltipView>();
            if (view == null)
            {
                Debug.LogWarning("[ItemTooltipManager] EquipmentTooltipPrefab missing EquipmentTooltipView component.");
                return;
            }

            view.SetData(itemData);
        });
    }

    /// <summary>
    /// Show tooltip for effigies.
    /// </summary>
    public void ShowEffigyTooltip(Effigy effigy, Vector2 screenPosition)
    {
        if (effigy == null)
        {
            Debug.LogWarning("[ItemTooltipManager] ShowEffigyTooltip called with null effigy.");
            return;
        }

        ShowTooltipInternal(effigyTooltipPrefab, screenPosition, tooltip =>
        {
            var view = tooltip.GetComponent<EffigyTooltipView>();
            if (view == null)
            {
                Debug.LogWarning("[ItemTooltipManager] EffigyTooltipPrefab missing EffigyTooltipView component.");
                return;
            }

            view.SetData(effigy);
        });
    }

    /// <summary>
    /// Show tooltip for currency data.
    /// </summary>
    public void ShowCurrencyTooltip(CurrencyData currency, Vector2 screenPosition)
    {
        if (currency == null)
        {
            HideTooltip();
            return;
        }

        ShowTooltipInternal(currencyTooltipPrefab, screenPosition, tooltip =>
        {
            var view = tooltip.GetComponent<CurrencyTooltipView>();
            if (view == null)
            {
                Debug.LogWarning("[ItemTooltipManager] CurrencyTooltipPrefab missing CurrencyTooltipView component.");
                return;
            }

            view.SetData(currency);
        });
    }

    /// <summary>
    /// Show tooltip for cards (includes embossing information).
    /// </summary>
    public void ShowCardTooltip(Card card, Character character, Vector2 screenPosition)
    {
        if (card == null)
        {
            Debug.LogWarning("[ItemTooltipManager] ShowCardTooltip called with null card.");
            return;
        }

        ShowTooltipInternal(cardTooltipPrefab, screenPosition, tooltip =>
        {
            var view = tooltip.GetComponent<CardTooltipView>();
            if (view == null)
            {
                Debug.LogWarning("[ItemTooltipManager] CardTooltipPrefab missing CardTooltipView component.");
                return;
            }

            view.SetData(card, character);
        });
    }

    /// <summary>
    /// Hide the currently active tooltip (if any).
    /// </summary>
    public void HideTooltip()
    {
        if (activeTooltip == null)
        {
            return;
        }

        Destroy(activeTooltip);
        activeTooltip = null;
    }

    private void ShowTooltipInternal(GameObject prefab, Vector2 screenPosition, System.Action<GameObject> configure)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[ItemTooltipManager] ShowTooltipInternal called with null prefab.");
            return;
        }

        HideTooltip();

        if (tooltipContainer == null)
        {
            Debug.LogWarning("[ItemTooltipManager] Tooltip container is not assigned.");
            return;
        }

        activeTooltip = Instantiate(prefab, tooltipContainer);
        var tooltipRect = activeTooltip.transform as RectTransform;

        if (tooltipRect == null)
        {
            Debug.LogWarning("[ItemTooltipManager] Instantiated tooltip is missing RectTransform.");
            Destroy(activeTooltip);
            activeTooltip = null;
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

        // CRITICAL: Disable raycasting on ALL graphics in the tooltip to prevent blocking pointer events
        // This prevents the tooltip from intercepting mouse events and causing flicker
        var allGraphics = activeTooltip.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        foreach (var graphic in allGraphics)
        {
            graphic.raycastTarget = false;
        }
        
        // Also disable any CanvasGroup to ensure no blocking
        var canvasGroups = activeTooltip.GetComponentsInChildren<CanvasGroup>(true);
        foreach (var cg in canvasGroups)
        {
            cg.blocksRaycasts = false;
        }

        PositionTooltip(tooltipRect, screenPosition);

        configure?.Invoke(activeTooltip);
    }

    public void ShowWarrantTooltip(WarrantDefinition warrant, Vector2 screenPosition)
    {
        if (warrant == null)
        {
            HideTooltip();
            return;
        }

        ShowWarrantTooltip(WarrantTooltipUtility.BuildSingleWarrantData(warrant), screenPosition);
    }

    public void ShowWarrantTooltip(WarrantTooltipData data, Vector2 screenPosition)
    {
        if (data == null)
        {
            HideTooltip();
            return;
        }

        if (tooltipContainer == null)
        {
            Debug.LogWarning("[ItemTooltipManager] Tooltip container is not assigned.");
            return;
        }

        HideTooltip();

        activeTooltip = warrantTooltipPrefab != null
            ? Instantiate(warrantTooltipPrefab, tooltipContainer)
            : CreateRuntimeWarrantTooltip();

        var tooltipRect = activeTooltip.transform as RectTransform;
        if (tooltipRect == null)
        {
            Debug.LogWarning("[ItemTooltipManager] Warrant tooltip missing RectTransform.");
            Destroy(activeTooltip);
            activeTooltip = null;
            return;
        }

        var view = activeTooltip.GetComponent<WarrantTooltipView>();
        if (view == null)
        {
            view = activeTooltip.AddComponent<WarrantTooltipView>();
        }

        view.SetData(data);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        PositionTooltip(tooltipRect, screenPosition);
    }

    private void PositionTooltip(RectTransform tooltipRect, Vector2 screenPosition)
    {
        if (targetCanvas == null)
        {
            Debug.LogWarning("[ItemTooltipManager] targetCanvas not set, cannot position tooltip.");
            return;
        }

        var canvasRect = targetCanvas.transform as RectTransform;
        if (canvasRect == null)
        {
            Debug.LogWarning("[ItemTooltipManager] Canvas does not have a RectTransform.");
            return;
        }

        Camera eventCamera = null;
        if (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera || targetCanvas.renderMode == RenderMode.WorldSpace)
        {
            eventCamera = targetCanvas.worldCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, eventCamera, out var localPoint))
        {
            Vector2 anchorPosition = ApplyCursorOffset(localPoint, tooltipRect);
            tooltipRect.anchoredPosition = ClampToCanvasBounds(anchorPosition, tooltipRect, canvasRect);
        }
    }

    private Vector2 ApplyCursorOffset(Vector2 localPoint, RectTransform tooltipRect)
    {
        Vector2 size = tooltipRect.rect.size;
        float pivotX = tooltipRect.pivot.x;
        float pivotY = tooltipRect.pivot.y;

        // Determine which side of cursor to place tooltip
        // If cursor is on left side of screen, place tooltip to the right
        // If cursor is on right side of screen, place tooltip to the left
        bool placeOnRight = localPoint.x < 0f;

        // Calculate horizontal offset - ensure tooltip doesn't overlap cursor
        // Add extra padding to prevent cursor from landing on tooltip
        float minHorizontalGap = cursorHorizontalOffset + 20f; // Extra gap to prevent overlap
        
        float horizontalShift = placeOnRight
            ? (size.x * (1f - pivotX)) + minHorizontalGap
            : -((size.x * pivotX) + minHorizontalGap);

        // Vertical offset - prefer above cursor, but can adjust if needed
        float verticalShift = cursorVerticalOffset;
        
        // If vertical offset is 0, add a small upward offset to keep tooltip above cursor
        if (Mathf.Approximately(verticalShift, 0f))
        {
            verticalShift = size.y * 0.1f; // Small upward offset (10% of tooltip height)
        }

        return new Vector2(localPoint.x + horizontalShift, localPoint.y + verticalShift);
    }

    private Vector2 ClampToCanvasBounds(Vector2 desiredPosition, RectTransform tooltipRect, RectTransform canvasRect)
    {
        float pivotX = tooltipRect.pivot.x;
        float pivotY = tooltipRect.pivot.y;
        Vector2 size = tooltipRect.rect.size;

        float minX = canvasRect.rect.xMin + screenEdgePadding;
        float maxX = canvasRect.rect.xMax - screenEdgePadding;
        float minY = canvasRect.rect.yMin + screenEdgePadding;
        float maxY = canvasRect.rect.yMax - screenEdgePadding;

        float left = desiredPosition.x - size.x * pivotX;
        float right = desiredPosition.x + size.x * (1f - pivotX);
        float bottom = desiredPosition.y - size.y * pivotY;
        float top = desiredPosition.y + size.y * (1f - pivotY);

        Vector2 adjustedPosition = desiredPosition;
        bool wasAdjusted = false;

        // Clamp horizontally
        if (left < minX)
        {
            adjustedPosition.x += minX - left;
            wasAdjusted = true;
        }
        else if (right > maxX)
        {
            adjustedPosition.x -= right - maxX;
            wasAdjusted = true;
        }

        // Clamp vertically
        if (bottom < minY)
        {
            adjustedPosition.y += minY - bottom;
            wasAdjusted = true;
        }
        else if (top > maxY)
        {
            adjustedPosition.y -= top - maxY;
            wasAdjusted = true;
        }

        // If we had to adjust position due to edge constraints, ensure we maintain minimum gap from cursor
        // This prevents the tooltip from overlapping the cursor when clamped to edges
        if (wasAdjusted)
        {
            // Recalculate bounds after adjustment
            left = adjustedPosition.x - size.x * pivotX;
            right = adjustedPosition.x + size.x * (1f - pivotX);
            bottom = adjustedPosition.y - size.y * pivotY;
            top = adjustedPosition.y + size.y * (1f - pivotY);
            
            // If tooltip is at right edge, ensure it's far enough left to not overlap cursor
            if (right >= maxX - 5f) // Within 5 pixels of right edge
            {
                // Ensure minimum gap from where cursor would be
                float cursorX = desiredPosition.x - (size.x * (1f - pivotX) + cursorHorizontalOffset);
                if (right - cursorX < minHorizontalGap)
                {
                    adjustedPosition.x -= (minHorizontalGap - (right - cursorX));
                }
            }
            
            // If tooltip is at left edge, ensure it's far enough right to not overlap cursor
            if (left <= minX + 5f) // Within 5 pixels of left edge
            {
                float cursorX = desiredPosition.x + (size.x * pivotX + cursorHorizontalOffset);
                if (cursorX - left < minHorizontalGap)
                {
                    adjustedPosition.x += (minHorizontalGap - (cursorX - left));
                }
            }
        }

        return adjustedPosition;
    }

    /// <summary>
    /// Convenience method for pointer events to pass current cursor position.
    /// </summary>
    public void ShowTooltipForPointer(BaseItem item, PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        Vector2 screenPosition = eventData.position;

        switch (item)
        {
            case WeaponItem weapon:
                ShowWeaponTooltip(weapon, screenPosition);
                break;
            case Effigy effigy:
                ShowEffigyTooltip(effigy, screenPosition);
                break;
            default:
                ShowEquipmentTooltip(item, screenPosition);
                break;
        }
    }

    /// <summary>
    /// Convenience overload for ItemData when handling pointer events.
    /// </summary>
    public void ShowTooltipForPointer(ItemData itemData, PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        ShowEquipmentTooltip(itemData, eventData.position);
    }

    /// <summary>
    /// Convenience overload for card tooltips when handling pointer events.
    /// </summary>
    public void ShowCardTooltipForPointer(Card card, Character character, PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        ShowCardTooltip(card, character, eventData.position);
    }

    public void ShowCurrencyTooltipForPointer(CurrencyData currency, PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        ShowCurrencyTooltip(currency, eventData.position);
    }

    public void ShowWarrantTooltipForPointer(WarrantTooltipData data, PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        ShowWarrantTooltip(data, eventData.position);
    }

    public void ShowWarrantTooltipForPointer(WarrantDefinition definition, PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        ShowWarrantTooltip(definition, eventData.position);
    }

    private GameObject CreateRuntimeWarrantTooltip()
    {
        var tooltipGO = new GameObject("RuntimeWarrantTooltip", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(WarrantTooltipView));
        tooltipGO.transform.SetParent(tooltipContainer, false);

        var rect = tooltipGO.GetComponent<RectTransform>();
        rect.pivot = new Vector2(0, 1);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.sizeDelta = new Vector2(320, 200);

        var background = tooltipGO.GetComponent<Image>();
        background.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);
        background.raycastTarget = false;

        return tooltipGO;
    }
}


