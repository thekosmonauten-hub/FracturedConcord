using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Controls a single forge material display item (prefab instance).
/// Similar to CurrencyDisplayItem but for forge materials.
/// </summary>
public class ForgeMaterialDisplayItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image materialIcon;
    [SerializeField] private TextMeshProUGUI materialNameText;
    [SerializeField] private TextMeshProUGUI materialCountText;
    [SerializeField] private Image backgroundImage;
    
    private ForgeMaterialType currentMaterialType;
    private int currentQuantity;
    
    /// <summary>
    /// Initialize the material display with a material type and quantity
    /// </summary>
    public void Initialize(ForgeMaterialType materialType, int quantity)
    {
        currentMaterialType = materialType;
        currentQuantity = quantity;
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// Update the display with new quantity
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        currentQuantity = newQuantity;
        UpdateDisplay();
    }
    
    /// <summary>
    /// Update the visual display
    /// </summary>
    private void UpdateDisplay()
    {
        // Get material data from database
        var materialData = ForgeMaterialDatabase.Instance?.GetMaterialData(currentMaterialType);
        
        // Update icon sprite
        if (materialIcon != null)
        {
            if (materialData != null && materialData.materialSprite != null)
            {
                materialIcon.sprite = materialData.materialSprite;
                materialIcon.enabled = true;
            }
            else
            {
                // Hide icon if no sprite available
                materialIcon.enabled = false;
            }
        }
        
        // Update name text
        if (materialNameText != null)
        {
            string displayName = materialData != null && !string.IsNullOrEmpty(materialData.displayName)
                ? materialData.displayName
                : GetDefaultDisplayName(currentMaterialType);
            materialNameText.text = displayName;
        }
        
        // Update count text
        if (materialCountText != null)
        {
            materialCountText.text = currentQuantity.ToString();
        }
        
        // Update background color if available
        if (backgroundImage != null && materialData != null)
        {
            // You can set a tint color if desired
            // backgroundImage.color = materialData.displayColor;
        }
    }
    
    /// <summary>
    /// Get default display name (fallback)
    /// </summary>
    private string GetDefaultDisplayName(ForgeMaterialType materialType)
    {
        switch (materialType)
        {
            case ForgeMaterialType.WeaponScraps:
                return "Weapon Scraps";
            case ForgeMaterialType.ArmourScraps:
                return "Armour Scraps";
            case ForgeMaterialType.EffigySplinters:
                return "Effigy Splinters";
            case ForgeMaterialType.WarrantShards:
                return "Warrant Shards";
            default:
                return materialType.ToString();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // TODO: Show tooltip with material description
        var materialData = ForgeMaterialDatabase.Instance?.GetMaterialData(currentMaterialType);
        if (materialData != null && !string.IsNullOrEmpty(materialData.description))
        {
            // You can implement tooltip here if needed
            Debug.Log($"[ForgeMaterialDisplayItem] {materialData.displayName}: {materialData.description}");
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide tooltip if implemented
    }
    
    #region Editor Setup Helper
#if UNITY_EDITOR
    /// <summary>
    /// Auto-assigns references in the Unity Editor (call from Inspector context menu).
    /// </summary>
    [ContextMenu("Auto-Assign References")]
    private void AutoAssignReferences()
    {
        // Try to find IconImage
        if (materialIcon == null)
        {
            Transform iconTransform = transform.Find("Content/IconImage");
            if (iconTransform == null)
                iconTransform = transform.Find("IconImage");
            if (iconTransform == null)
                iconTransform = transform.Find("Image");
            
            if (iconTransform != null)
            {
                materialIcon = iconTransform.GetComponent<Image>();
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log("[ForgeMaterialDisplayItem] Auto-assigned materialIcon");
            }
        }
        
        // Try to find NameText
        if (materialNameText == null)
        {
            Transform nameTransform = transform.Find("Content/NameText");
            if (nameTransform == null)
                nameTransform = transform.Find("NameText");
            
            if (nameTransform != null)
            {
                materialNameText = nameTransform.GetComponent<TextMeshProUGUI>();
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log("[ForgeMaterialDisplayItem] Auto-assigned materialNameText");
            }
        }
        
        // Try to find CountText
        if (materialCountText == null)
        {
            Transform countTransform = transform.Find("Content/CountText");
            if (countTransform == null)
                countTransform = transform.Find("CountText");
            
            if (countTransform != null)
            {
                materialCountText = countTransform.GetComponent<TextMeshProUGUI>();
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log("[ForgeMaterialDisplayItem] Auto-assigned materialCountText");
            }
        }
        
        // Try to find Background
        if (backgroundImage == null)
        {
            Transform bgTransform = transform.Find("Background");
            if (bgTransform != null)
            {
                backgroundImage = bgTransform.GetComponent<Image>();
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log("[ForgeMaterialDisplayItem] Auto-assigned backgroundImage");
            }
        }
    }
#endif
    #endregion
}

