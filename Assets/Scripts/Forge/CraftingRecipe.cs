using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

/// <summary>
/// Defines a crafting recipe for creating items from materials
/// </summary>
[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Dexiled/Forge/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Database Reference (Optional)")]
    [Tooltip("Optional direct reference to ItemDatabase. If not set, will use Resources.Load. Setting this ensures faster access and better editor validation.")]
    [SerializeField] private ItemDatabase itemDatabaseReference;
    [Header("Recipe Information")]
    public string recipeName = "New Recipe";
    [TextArea(3, 5)]
    public string description = "Recipe description";
    
    [Header("Required Materials")]
    [Tooltip("Materials required to craft this item")]
    public List<MaterialRequirement> requiredMaterials = new List<MaterialRequirement>();
    
    [Header("Cost Calculator")]
    [Tooltip("If true, costs will be auto-calculated based on item level and rarity")]
    public bool useAutoCalculatedCosts = false;
    
    [Tooltip("Cost calculation preset (Default is balanced, Alternative is cheaper, Expensive is costly)")]
    public ForgeCostCalculator.CostConfig costConfig = ForgeCostCalculator.CostConfig.Default;
    
    [Header("Crafted Item")]
    [Tooltip("The item type to craft")]
    public ItemType itemType;
    
    [Tooltip("Filter by specific equipment type (if enabled)")]
    public bool filterByEquipmentType = false;
    
    [Tooltip("The equipment type to filter by (only used if filterByEquipmentType is true)")]
    public EquipmentType equipmentType;
    
    [Header("Armour Filters")]
    [Tooltip("Filter by armour slot (Helmet, BodyArmour, Gloves, Boots, Shield)")]
    public bool filterByArmourSlot = false;
    
    [Tooltip("The armour slot to filter by (only used if filterByArmourSlot is true)")]
    public ArmourSlot armourSlot;
    
    [Tooltip("Filter by primary defense type (items with this defense stat > 0)")]
    public bool filterByDefenseType = false;
    
    [Tooltip("The defense type to filter by (only used if filterByDefenseType is true)")]
    public DefenseType defenseType;
    
    [Header("Weapon Filters")]
    [Tooltip("Filter by weapon type (Axe, Bow, Sword, Dagger, etc.)")]
    public bool filterByWeaponType = false;
    
    [Tooltip("The weapon type to filter by (only used if filterByWeaponType is true)")]
    public WeaponItemType weaponType;
    
    [Tooltip("Filter by weapon handedness (OneHanded, TwoHanded)")]
    public bool filterByWeaponHandedness = false;
    
    [Tooltip("The weapon handedness to filter by (only used if filterByWeaponHandedness is true)")]
    public WeaponHandedness weaponHandedness;
    
    [Tooltip("Specific item to craft (if null, generates random item of type)")]
    public BaseItem specificItem;
    
    [Header("Crafting Options")]
    [Tooltip("If true, craft a random item of the specified type appropriate for player level")]
    public bool craftRandomItem = true;
    
    [Tooltip("Minimum item level (if crafting random)")]
    public int minItemLevel = 1;
    
    [Tooltip("Maximum item level (if crafting random, -1 = use character level)")]
    public int maxItemLevel = -1;
    
    [Tooltip("Rarity of crafted item (if crafting random)")]
    public ItemRarity craftedRarity = ItemRarity.Magic;
    
    [Header("Dynamic Item Selection")]
    [Tooltip("Level range above player level to show items (e.g., 5 = show items up to 5 levels above player)")]
    public int levelRangeAbovePlayer = 5;
    
    [Tooltip("Temporarily stores the selected item when player chooses which specific item to craft")]
    [System.NonSerialized]
    public BaseItem selectedItemToCraft;
    
    /// <summary>
    /// Auto-calculate and set material costs based on item level and rarity
    /// </summary>
    public void CalculateAndSetCosts()
    {
        // Determine material type based on item type
        ForgeMaterialType materialType = GetMaterialTypeForItemType();
        
        // Calculate cost
        int cost = ForgeCostCalculator.CalculateCostForRecipe(this, costConfig);
        
        // Ensure list exists
        if (requiredMaterials == null)
        {
            requiredMaterials = new List<MaterialRequirement>();
        }
        
        // Remove old requirement for this material type if exists
        requiredMaterials.RemoveAll(r => r.materialType == materialType);
        
        // Add new requirement
        requiredMaterials.Add(new MaterialRequirement
        {
            materialType = materialType,
            quantity = cost
        });
    }
    
    /// <summary>
    /// Get the material type for this recipe's item type
    /// </summary>
    private ForgeMaterialType GetMaterialTypeForItemType()
    {
        switch (itemType)
        {
            case ItemType.Weapon:
                return ForgeMaterialType.WeaponScraps;
            case ItemType.Armour:
            case ItemType.Accessory:
                return ForgeMaterialType.ArmourScraps;
            case ItemType.Effigy:
                return ForgeMaterialType.EffigySplinters;
            case ItemType.Warrant:
                return ForgeMaterialType.WarrantShards;
            default:
                return ForgeMaterialType.WeaponScraps;
        }
    }
    
    /// <summary>
    /// Check if character has required materials
    /// </summary>
    public bool CanCraft(Character character)
    {
        if (character == null) return false;
        
        // Auto-calculate costs if enabled and not already set
        if (useAutoCalculatedCosts && (requiredMaterials == null || requiredMaterials.Count == 0))
        {
            CalculateAndSetCosts();
        }
        
        foreach (var requirement in requiredMaterials)
        {
            int available = ForgeMaterialManager.GetMaterialQuantity(character, requirement.materialType);
            if (available < requirement.quantity)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Get formatted description of required materials
    /// </summary>
    public string GetMaterialRequirementsText()
    {
        if (requiredMaterials == null || requiredMaterials.Count == 0)
        {
            return "No materials required";
        }
        
        List<string> requirements = new List<string>();
        foreach (var req in requiredMaterials)
        {
            requirements.Add($"{req.quantity} {req.materialType}");
        }
        
        return string.Join(", ", requirements);
    }
    
    /// <summary>
    /// Get eligible items for crafting based on character level.
    /// Returns items where requiredLevel is at or below (characterLevel + levelRangeAbovePlayer)
    /// </summary>
    public List<BaseItem> GetEligibleItemsForLevel(int characterLevel)
    {
        // Get ItemDatabase - prefer direct reference, fallback to singleton
        ItemDatabase database = GetItemDatabase();
        if (database == null)
        {
            Debug.LogError("[CraftingRecipe] ItemDatabase is null! Make sure ItemDatabase exists in Resources folder or assign it directly to this recipe.");
            return new List<BaseItem>();
        }
        
        if (database.armour == null)
        {
            Debug.LogError("[CraftingRecipe] ItemDatabase.armour list is null!");
            return new List<BaseItem>();
        }
        
        int maxLevel = characterLevel + levelRangeAbovePlayer;
        List<BaseItem> eligibleItems = new List<BaseItem>();
        
        switch (itemType)
        {
            case ItemType.Weapon:
                if (database.weapons == null)
                {
                    Debug.LogError("[CraftingRecipe] ItemDatabase.weapons list is null!");
                    break;
                }
                var weapons = database.GetWeaponsByLevel(minItemLevel, maxLevel);
                if (weapons == null)
                {
                    Debug.LogWarning($"[CraftingRecipe] GetWeaponsByLevel returned null for levels {minItemLevel}-{maxLevel}");
                    break;
                }
                eligibleItems = weapons
                    .Where(w => w != null && 
                                (!filterByEquipmentType || w.equipmentType == equipmentType) &&
                                (!filterByWeaponType || w.weaponType == weaponType) &&
                                (!filterByWeaponHandedness || w.handedness == weaponHandedness))
                    .Cast<BaseItem>()
                    .ToList();
                break;
                
            case ItemType.Armour:
                if (database.armour == null)
                {
                    Debug.LogError("[CraftingRecipe] ItemDatabase.armour list is null!");
                    break;
                }
                
                // Count null entries for debugging
                int nullCount = database.armour.Count(a => a == null);
                if (nullCount > 0)
                {
                    Debug.LogWarning($"[CraftingRecipe] Found {nullCount} null entries in ItemDatabase.armour list. These will be filtered out.");
                }
                
                eligibleItems = database.armour
                    .Where(a => a != null && 
                                a.requiredLevel >= minItemLevel && 
                                a.requiredLevel <= maxLevel &&
                                (!filterByEquipmentType || a.equipmentType == equipmentType) &&
                                (!filterByArmourSlot || a.armourSlot == armourSlot) &&
                                (!filterByDefenseType || MatchesDefenseType(a)))
                    .Cast<BaseItem>()
                    .ToList();
                break;
                
            case ItemType.Accessory:
                if (database.jewellery == null)
                {
                    Debug.LogError("[CraftingRecipe] ItemDatabase.jewellery list is null!");
                    break;
                }
                
                // Count null entries for debugging
                int nullCountJ = database.jewellery.Count(j => j == null);
                if (nullCountJ > 0)
                {
                    Debug.LogWarning($"[CraftingRecipe] Found {nullCountJ} null entries in ItemDatabase.jewellery list. These will be filtered out.");
                }
                
                eligibleItems = database.jewellery
                    .Where(j => j != null && 
                                j.requiredLevel >= minItemLevel && 
                                j.requiredLevel <= maxLevel &&
                                (!filterByEquipmentType || j.equipmentType == equipmentType))
                    .Cast<BaseItem>()
                    .ToList();
                break;
                
            default:
                Debug.LogWarning($"[CraftingRecipe] Item type {itemType} not supported for dynamic item selection.");
                break;
        }
        
        // Sort by required level (ascending) so lower level items appear first
        // Filter out any null items that might have slipped through
        eligibleItems = eligibleItems
            .Where(item => item != null)
            .OrderBy(item => item.requiredLevel)
            .ToList();
        
        return eligibleItems;
    }
    
    /// <summary>
    /// Check if armour matches the specified defense type
    /// Handles both single and hybrid defense bases
    /// </summary>
    private bool MatchesDefenseType(Armour armour)
    {
        if (!filterByDefenseType) return true;
        
        switch (defenseType)
        {
            // Single defense types - must have this defense and no other primary defenses
            case DefenseType.Armour:
                return armour.armour > 0 && armour.evasion == 0 && armour.energyShield == 0 && armour.ward == 0;
            case DefenseType.Evasion:
                return armour.evasion > 0 && armour.armour == 0 && armour.energyShield == 0 && armour.ward == 0;
            case DefenseType.EnergyShield:
                return armour.energyShield > 0 && armour.armour == 0 && armour.evasion == 0 && armour.ward == 0;
            case DefenseType.Ward:
                return armour.ward > 0 && armour.armour == 0 && armour.evasion == 0 && armour.energyShield == 0;
            
            // Hybrid defense types - must have both specified defenses
            case DefenseType.ArmourEvasion:
                return armour.armour > 0 && armour.evasion > 0;
            case DefenseType.ArmourEnergyShield:
                return armour.armour > 0 && armour.energyShield > 0;
            case DefenseType.ArmourWard:
                return armour.armour > 0 && armour.ward > 0;
            case DefenseType.EvasionEnergyShield:
                return armour.evasion > 0 && armour.energyShield > 0;
            case DefenseType.EvasionWard:
                return armour.evasion > 0 && armour.ward > 0;
            case DefenseType.EnergyShieldWard:
                return armour.energyShield > 0 && armour.ward > 0;
            
            default:
                return true;
        }
    }
    
    /// <summary>
    /// Get ItemDatabase - prefer direct reference, fallback to singleton instance
    /// </summary>
    private ItemDatabase GetItemDatabase()
    {
        // Prefer direct reference if assigned (faster and better for editor validation)
        if (itemDatabaseReference != null)
        {
            return itemDatabaseReference;
        }
        
        // Fallback to singleton instance (Resources.Load)
        return ItemDatabase.Instance;
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Editor helper: Validate and auto-calculate costs on asset modification
    /// </summary>
    private void OnValidate()
    {
        if (useAutoCalculatedCosts && Application.isPlaying == false)
        {
            // Only auto-calculate in editor, not at runtime
            CalculateAndSetCosts();
        }
    }
#endif
}

/// <summary>
/// Defense type for filtering armour by defense stat(s)
/// Supports both single and hybrid defense bases
/// </summary>
public enum DefenseType
{
    Armour,              // Pure Armour
    Evasion,             // Pure Evasion
    EnergyShield,         // Pure Energy Shield
    Ward,                 // Pure Ward
    ArmourEvasion,        // Hybrid: Armour + Evasion
    ArmourEnergyShield,   // Hybrid: Armour + Energy Shield
    ArmourWard,           // Hybrid: Armour + Ward
    EvasionEnergyShield,  // Hybrid: Evasion + Energy Shield
    EvasionWard,          // Hybrid: Evasion + Ward
    EnergyShieldWard      // Hybrid: Energy Shield + Ward
}

[System.Serializable]
public class MaterialRequirement
{
    public ForgeMaterialType materialType;
    public int quantity = 1;
}

