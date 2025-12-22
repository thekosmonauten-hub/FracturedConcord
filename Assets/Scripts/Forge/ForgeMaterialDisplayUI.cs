using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Displays current forge materials in a panel
/// </summary>
public class ForgeMaterialDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform materialContainer;
    [SerializeField] private GameObject materialEntryPrefab;
    
    [Header("Display Settings")]
    [SerializeField] private bool showZeroQuantities = false;
    
    private Dictionary<ForgeMaterialType, ForgeMaterialDisplayItem> materialEntries = new Dictionary<ForgeMaterialType, ForgeMaterialDisplayItem>();
    
    private void Start()
    {
        RefreshDisplay();
    }
    
    private void OnEnable()
    {
        RefreshDisplay();
    }
    
    /// <summary>
    /// Refresh the material display with current character materials
    /// </summary>
    public void RefreshDisplay()
    {
        if (CharacterManager.Instance == null)
        {
            Debug.LogWarning("[ForgeMaterialDisplayUI] CharacterManager not found.");
            return;
        }
        
        var character = CharacterManager.Instance.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogWarning("[ForgeMaterialDisplayUI] No current character.");
            return;
        }
        
        // Get all material types
        var allMaterialTypes = System.Enum.GetValues(typeof(ForgeMaterialType)).Cast<ForgeMaterialType>();
        
        foreach (var materialType in allMaterialTypes)
        {
            int quantity = ForgeMaterialManager.GetMaterialQuantity(character, materialType);
            
            // Skip zero quantities if not showing them
            if (!showZeroQuantities && quantity == 0)
            {
                // Hide or remove entry
                if (materialEntries.ContainsKey(materialType) && materialEntries[materialType] != null)
                {
                    materialEntries[materialType].gameObject.SetActive(false);
                }
                continue;
            }
            
            // Create or update entry
            if (!materialEntries.ContainsKey(materialType))
            {
                CreateMaterialEntry(materialType);
            }
            
            UpdateMaterialEntry(materialType, quantity);
        }
    }
    
    /// <summary>
    /// Create a new material entry UI element
    /// </summary>
    private void CreateMaterialEntry(ForgeMaterialType materialType)
    {
        if (materialContainer == null)
        {
            Debug.LogWarning("[ForgeMaterialDisplayUI] Material container not assigned.");
            return;
        }
        
        GameObject entry;
        if (materialEntryPrefab != null)
        {
            entry = Instantiate(materialEntryPrefab, materialContainer);
            
            // Get or add ForgeMaterialDisplayItem component
            var displayItem = entry.GetComponent<ForgeMaterialDisplayItem>();
            if (displayItem == null)
            {
                displayItem = entry.AddComponent<ForgeMaterialDisplayItem>();
                #if UNITY_EDITOR
                // Try to auto-assign references
                displayItem.SendMessage("AutoAssignReferences", UnityEngine.SendMessageOptions.DontRequireReceiver);
                #endif
            }
            
            materialEntries[materialType] = displayItem;
        }
        else
        {
            // Create a simple entry if no prefab
            entry = new GameObject($"Material_{materialType}");
            entry.transform.SetParent(materialContainer);
            
            var layout = entry.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.spacing = 10f;
            
            var nameText = new GameObject("Name");
            nameText.transform.SetParent(entry.transform);
            var nameTMP = nameText.AddComponent<TextMeshProUGUI>();
            nameTMP.text = GetMaterialDisplayName(materialType);
            nameTMP.fontSize = 16;
            
            var quantityText = new GameObject("Quantity");
            quantityText.transform.SetParent(entry.transform);
            var qtyTMP = quantityText.AddComponent<TextMeshProUGUI>();
            qtyTMP.text = "0";
            qtyTMP.fontSize = 16;
            qtyTMP.color = Color.yellow;
            qtyTMP.name = "QuantityText";
            
            // Create a simple display item component for consistency
            var displayItem = entry.AddComponent<ForgeMaterialDisplayItem>();
            materialEntries[materialType] = displayItem;
        }
    }
    
    /// <summary>
    /// Update an existing material entry with new quantity
    /// </summary>
    private void UpdateMaterialEntry(ForgeMaterialType materialType, int quantity)
    {
        if (!materialEntries.ContainsKey(materialType))
        {
            return;
        }
        
        var displayItem = materialEntries[materialType];
        if (displayItem == null)
        {
            return;
        }
        
        displayItem.gameObject.SetActive(true);
        displayItem.Initialize(materialType, quantity);
    }
    
    /// <summary>
    /// Get display name for material type
    /// </summary>
    private string GetMaterialDisplayName(ForgeMaterialType materialType)
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
}

