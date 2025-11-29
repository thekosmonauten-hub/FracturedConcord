using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Database for mapping shop IDs to their corresponding UI panels and controllers.
/// Allows dialogue actions to open shops without hardcoding shop-specific logic.
/// </summary>
[CreateAssetMenu(fileName = "Shop Database", menuName = "Dexiled/Dialogue/Shop Database")]
public class ShopDatabase : ScriptableObject
{
    [System.Serializable]
    public class ShopMapping
    {
        [Tooltip("Shop ID used in dialogue actions (e.g., 'MazeVendor', 'Blinket', 'TownBlacksmith')")]
        public string shopId;
        
        [Tooltip("Friendly name for this shop (for editor/display purposes)")]
        public string shopName;
        
        [Header("Panel Access Methods")]
        [Tooltip("Method to find the shop panel:\n" +
                 "MazeHubController - Uses MazeHubController to access panel\n" +
                 "DirectPanel - Activates panel GameObject directly\n" +
                 "ComponentType - Finds and activates component by type\n" +
                 "GameObjectName - Finds panel by GameObject name")]
        public PanelAccessMethod accessMethod = PanelAccessMethod.MazeHubController;
        
        [Header("MazeHubController Access (for accessMethod = MazeHubController)")]
        [Tooltip("Name of the panel field in MazeHubController (e.g., 'vendorPanel', 'forgePanel')")]
        public string panelFieldName = "vendorPanel";
        
        [Header("Direct Panel Access (for accessMethod = DirectPanel)")]
        [Tooltip("Direct reference to the panel GameObject (only used if accessMethod = DirectPanel)")]
        public GameObject panelGameObject;
        
        [Header("Component Type Access (for accessMethod = ComponentType)")]
        [Tooltip("Full type name of the component (e.g., 'Dexiled.MazeSystem.MazeVendorUI')")]
        public string componentTypeName;
        
        [Header("GameObject Name Access (for accessMethod = GameObjectName)")]
        [Tooltip("Name(s) of the GameObject to search for (searches for any of these names)")]
        public List<string> gameObjectNames = new List<string>();
    }
    
    public enum PanelAccessMethod
    {
        MazeHubController,  // Access via MazeHubController field (e.g., vendorPanel, forgePanel)
        DirectPanel,        // Direct GameObject reference (static assignment)
        ComponentType,      // Find component by type name (e.g., MazeVendorUI)
        GameObjectName      // Find GameObject by name
    }
    
    [Header("Shop Mappings")]
    [Tooltip("List of shop ID to panel mappings")]
    public List<ShopMapping> shopMappings = new List<ShopMapping>();
    
    /// <summary>
    /// Get a shop mapping by shop ID (case-insensitive)
    /// </summary>
    public ShopMapping GetShopMapping(string shopId)
    {
        if (string.IsNullOrEmpty(shopId))
            return null;
        
        return shopMappings.FirstOrDefault(m => 
            m != null && 
            !string.IsNullOrEmpty(m.shopId) && 
            m.shopId.Equals(shopId, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Check if a shop ID exists in the database
    /// </summary>
    public bool HasShop(string shopId)
    {
        return GetShopMapping(shopId) != null;
    }
    
    /// <summary>
    /// Add or update a shop mapping
    /// </summary>
    public void SetShopMapping(string shopId, string shopName, PanelAccessMethod accessMethod, string panelFieldName = null)
    {
        if (string.IsNullOrEmpty(shopId))
            return;
        
        var existing = GetShopMapping(shopId);
        if (existing != null)
        {
            existing.shopName = shopName;
            existing.accessMethod = accessMethod;
            existing.panelFieldName = panelFieldName;
        }
        else
        {
            shopMappings.Add(new ShopMapping
            {
                shopId = shopId,
                shopName = shopName,
                accessMethod = accessMethod,
                panelFieldName = panelFieldName
            });
        }
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}

