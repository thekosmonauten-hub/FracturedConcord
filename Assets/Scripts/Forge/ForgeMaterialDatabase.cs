using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Database for forge material sprites and display information
/// </summary>
[CreateAssetMenu(fileName = "ForgeMaterialDatabase", menuName = "Dexiled/Forge/Material Database")]
public class ForgeMaterialDatabase : ScriptableObject
{
    [System.Serializable]
    public class MaterialData
    {
        public ForgeMaterialType materialType;
        public string displayName;
        public string description;
        public Sprite materialSprite;
        public Color displayColor = Color.white;
    }
    
    [Header("Material Definitions")]
    [SerializeField] private List<MaterialData> materials = new List<MaterialData>();
    
    private static ForgeMaterialDatabase _instance;
    public static ForgeMaterialDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ForgeMaterialDatabase>("ForgeMaterialDatabase");
                if (_instance == null)
                {
                    Debug.LogWarning("[ForgeMaterialDatabase] Database not found in Resources. Create one at Resources/ForgeMaterialDatabase.asset");
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Get material data for a specific material type
    /// </summary>
    public MaterialData GetMaterialData(ForgeMaterialType materialType)
    {
        if (materials == null)
        {
            return null;
        }
        
        var data = materials.FirstOrDefault(m => m != null && m.materialType == materialType);
        
        // If not found, create a default entry
        if (data == null)
        {
            data = new MaterialData
            {
                materialType = materialType,
                displayName = GetDefaultDisplayName(materialType),
                description = $"Material obtained from salvaging {GetDefaultDisplayName(materialType)}"
            };
            
            // Add to list for future use
            materials.Add(data);
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        
        return data;
    }
    
    /// <summary>
    /// Get sprite for a material type
    /// </summary>
    public Sprite GetMaterialSprite(ForgeMaterialType materialType)
    {
        var data = GetMaterialData(materialType);
        return data?.materialSprite;
    }
    
    /// <summary>
    /// Get display name for a material type
    /// </summary>
    public string GetDisplayName(ForgeMaterialType materialType)
    {
        var data = GetMaterialData(materialType);
        return data != null && !string.IsNullOrEmpty(data.displayName) 
            ? data.displayName 
            : GetDefaultDisplayName(materialType);
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
    
    /// <summary>
    /// Initialize default materials if database is empty
    /// </summary>
    [ContextMenu("Initialize Default Materials")]
    public void InitializeDefaultMaterials()
    {
        if (materials == null)
        {
            materials = new List<MaterialData>();
        }
        
        var allTypes = System.Enum.GetValues(typeof(ForgeMaterialType)).Cast<ForgeMaterialType>();
        
        foreach (var type in allTypes)
        {
            if (!materials.Any(m => m != null && m.materialType == type))
            {
                materials.Add(new MaterialData
                {
                    materialType = type,
                    displayName = GetDefaultDisplayName(type),
                    description = $"Material obtained from salvaging {GetDefaultDisplayName(type)}"
                });
            }
        }
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
        
        Debug.Log($"[ForgeMaterialDatabase] Initialized {materials.Count} default materials");
    }
}

