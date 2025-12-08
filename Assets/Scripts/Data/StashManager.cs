using UnityEngine;
using System.Collections.Generic;
using Dexiled.Data.Items;

/// <summary>
/// Global stash manager - stores items shared across all characters
/// Separate from character-specific inventory
/// </summary>
public class StashManager : MonoBehaviour
{
    private static StashManager _instance;
    public static StashManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<StashManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("StashManager");
                    _instance = go.AddComponent<StashManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Stash Storage")]
    [Tooltip("Global stash items (shared across all characters)")]
    public List<BaseItem> stashItems = new List<BaseItem>();
    
    [Header("Stash Events")]
    public System.Action<BaseItem> OnItemAdded;
    public System.Action<BaseItem> OnItemRemoved;
    public System.Action OnStashChanged;
    
    private const string STASH_SAVE_KEY = "GlobalStash";
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadStash();
    }
    
    /// <summary>
    /// Add an item to the global stash
    /// </summary>
    public void AddItem(BaseItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("[StashManager] Cannot add null item to stash");
            return;
        }
        
        stashItems.Add(item);
        OnItemAdded?.Invoke(item);
        OnStashChanged?.Invoke();
        
        SaveStash();
        Debug.Log($"[StashManager] Added {item.GetDisplayName()} to stash. Total: {stashItems.Count}");
    }
    
    /// <summary>
    /// Remove an item from the global stash
    /// </summary>
    public bool RemoveItem(BaseItem item)
    {
        if (item == null || !stashItems.Contains(item))
        {
            return false;
        }
        
        stashItems.Remove(item);
        OnItemRemoved?.Invoke(item);
        OnStashChanged?.Invoke();
        
        SaveStash();
        Debug.Log($"[StashManager] Removed {item.GetDisplayName()} from stash. Total: {stashItems.Count}");
        return true;
    }
    
    /// <summary>
    /// Remove item at index
    /// </summary>
    public bool RemoveItemAt(int index)
    {
        if (index < 0 || index >= stashItems.Count)
        {
            return false;
        }
        
        BaseItem item = stashItems[index];
        return RemoveItem(item);
    }
    
    /// <summary>
    /// Get all stash items
    /// </summary>
    public List<BaseItem> GetAllItems()
    {
        return new List<BaseItem>(stashItems);
    }
    
    /// <summary>
    /// Get item at index
    /// </summary>
    public BaseItem GetItemAt(int index)
    {
        if (index < 0 || index >= stashItems.Count)
        {
            return null;
        }
        return stashItems[index];
    }
    
    /// <summary>
    /// Get stash size
    /// </summary>
    public int GetStashSize()
    {
        return stashItems.Count;
    }
    
    /// <summary>
    /// Clear all items from stash
    /// </summary>
    public void ClearStash()
    {
        stashItems.Clear();
        OnStashChanged?.Invoke();
        SaveStash();
        Debug.Log("[StashManager] Stash cleared");
    }
    
    /// <summary>
    /// Swap two items in stash
    /// </summary>
    public void SwapItems(int index1, int index2)
    {
        if (index1 < 0 || index1 >= stashItems.Count || index2 < 0 || index2 >= stashItems.Count)
        {
            Debug.LogWarning("[StashManager] Invalid indices for swap");
            return;
        }
        
        BaseItem temp = stashItems[index1];
        stashItems[index1] = stashItems[index2];
        stashItems[index2] = temp;
        
        OnStashChanged?.Invoke();
        SaveStash();
        Debug.Log($"[StashManager] Swapped items at {index1} and {index2}");
    }
    
    /// <summary>
    /// Save stash to persistent storage
    /// </summary>
    public void SaveStash()
    {
        try
        {
            // Serialize stash items using Character's serialization
            List<SerializedItemData> serializedItems = new List<SerializedItemData>();
            foreach (var item in stashItems)
            {
                if (item == null) continue;
                
                // Use reflection to call private static method, or create item data directly
                SerializedItemData itemData = SerializeItemForStash(item);
                if (itemData != null)
                {
                    serializedItems.Add(itemData);
                }
            }
            
            // Convert to JSON
            string json = JsonUtility.ToJson(new StashSaveData { items = serializedItems }, true);
            
            // Save to PlayerPrefs (or use a file system for larger data)
            PlayerPrefs.SetString(STASH_SAVE_KEY, json);
            PlayerPrefs.Save();
            
            Debug.Log($"[StashManager] Saved {serializedItems.Count} items to stash");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StashManager] Failed to save stash: {e.Message}");
        }
    }
    
    /// <summary>
    /// Load stash from persistent storage
    /// </summary>
    public void LoadStash()
    {
        try
        {
            if (!PlayerPrefs.HasKey(STASH_SAVE_KEY))
            {
                Debug.Log("[StashManager] No saved stash found - starting with empty stash");
                stashItems = new List<BaseItem>();
                return;
            }
            
            string json = PlayerPrefs.GetString(STASH_SAVE_KEY);
            if (string.IsNullOrEmpty(json))
            {
                stashItems = new List<BaseItem>();
                return;
            }
            
            StashSaveData saveData = JsonUtility.FromJson<StashSaveData>(json);
            if (saveData == null || saveData.items == null)
            {
                stashItems = new List<BaseItem>();
                return;
            }
            
            // Deserialize items
            stashItems.Clear();
            foreach (var itemData in saveData.items)
            {
                BaseItem item = DeserializeItemFromStash(itemData);
                if (item != null)
                {
                    stashItems.Add(item);
                }
            }
            
            Debug.Log($"[StashManager] Loaded {stashItems.Count} items from stash");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StashManager] Failed to load stash: {e.Message}");
            stashItems = new List<BaseItem>();
        }
    }
    
    /// <summary>
    /// Serialize an item for stash storage (reuses Character's serialization logic)
    /// </summary>
    private SerializedItemData SerializeItemForStash(BaseItem item)
    {
        if (item == null) return null;
        
        var data = new SerializedItemData
        {
            itemName = item.itemName,
            rarity = item.rarity
        };
        
        // Determine item type and save type-specific data
        if (item is WeaponItem weapon)
        {
            data.itemType = "WeaponItem";
            data.rolledBaseDamage = weapon.rolledBaseDamage;
            
            // Save affixes with rolled values
            if (weapon.prefixes != null)
            {
                foreach (var affix in weapon.prefixes)
                {
                    data.affixes.Add(SerializeAffixForStash(affix));
                }
            }
            if (weapon.suffixes != null)
            {
                foreach (var affix in weapon.suffixes)
                {
                    data.affixes.Add(SerializeAffixForStash(affix));
                }
            }
            if (weapon.implicitModifiers != null)
            {
                foreach (var affix in weapon.implicitModifiers)
                {
                    data.affixes.Add(SerializeAffixForStash(affix));
                }
            }
        }
        else if (item is Armour armour)
        {
            data.itemType = "Armour";
            
            // Save affixes
            if (armour.prefixes != null)
            {
                foreach (var affix in armour.prefixes)
                {
                    data.affixes.Add(SerializeAffixForStash(affix));
                }
            }
            if (armour.suffixes != null)
            {
                foreach (var affix in armour.suffixes)
                {
                    data.affixes.Add(SerializeAffixForStash(affix));
                }
            }
            if (armour.implicitModifiers != null)
            {
                foreach (var affix in armour.implicitModifiers)
                {
                    data.affixes.Add(SerializeAffixForStash(affix));
                }
            }
        }
        else if (item is Effigy effigy)
        {
            data.itemType = "Effigy";
            // Effigies don't have affixes, just save basic data
        }
        else
        {
            data.itemType = "BaseItem";
        }
        
        return data;
    }
    
    /// <summary>
    /// Serialize an affix for stash storage
    /// </summary>
    private SerializedAffix SerializeAffixForStash(Affix affix)
    {
        if (affix == null) return null;
        
        var data = new SerializedAffix
        {
            affixName = affix.name,
            description = affix.description,
            affixType = affix.affixType,
            isRolled = affix.isRolled,
            rolledValue = affix.rolledValue
        };
        
        // Serialize modifiers
        if (affix.modifiers != null)
        {
            foreach (var modifier in affix.modifiers)
            {
                if (modifier == null) continue;
                
                data.modifiers.Add(new SerializedAffixModifier
                {
                    modifierType = modifier.modifierType,
                    minValue = modifier.minValue,
                    maxValue = modifier.maxValue,
                    isRolled = modifier.isRolled,
                    rolledValue = modifier.rolledValue,
                    isDualRange = modifier.isDualRange,
                    rolledFirstValue = modifier.rolledFirstValue,
                    rolledSecondValue = modifier.rolledSecondValue
                });
            }
        }
        
        return data;
    }
    
    /// <summary>
    /// Deserialize an item from stash storage (reuses Character's deserialization logic)
    /// </summary>
    private BaseItem DeserializeItemFromStash(SerializedItemData data)
    {
        if (data == null) return null;
        
        BaseItem item = null;
        
        // Create appropriate item type
        if (data.itemType == "WeaponItem")
        {
            item = ScriptableObject.CreateInstance<WeaponItem>();
            var weapon = item as WeaponItem;
            weapon.rolledBaseDamage = data.rolledBaseDamage;
        }
        else if (data.itemType == "Armour")
        {
            item = ScriptableObject.CreateInstance<Armour>();
        }
        else if (data.itemType == "Effigy")
        {
            item = ScriptableObject.CreateInstance<Effigy>();
        }
        else
        {
            item = ScriptableObject.CreateInstance<BaseItem>();
        }
        
        // Set basic properties
        item.itemName = data.itemName;
        item.rarity = data.rarity;
        
        // Deserialize affixes
        var prefixes = new List<Affix>();
        var suffixes = new List<Affix>();
        var implicits = new List<Affix>();
        
        foreach (var affixData in data.affixes)
        {
            Affix affix = DeserializeAffixFromStash(affixData);
            if (affix != null)
            {
                switch (affix.affixType)
                {
                    case AffixType.Prefix:
                        prefixes.Add(affix);
                        break;
                    case AffixType.Suffix:
                        suffixes.Add(affix);
                        break;
                    default:
                        implicits.Add(affix);
                        break;
                }
            }
        }
        
        // Apply affixes to item
        if (item is WeaponItem weapon2)
        {
            weapon2.prefixes = prefixes;
            weapon2.suffixes = suffixes;
            weapon2.implicitModifiers = implicits;
        }
        else if (item is Armour armour)
        {
            armour.prefixes = prefixes;
            armour.suffixes = suffixes;
            armour.implicitModifiers = implicits;
        }
        
        return item;
    }
    
    /// <summary>
    /// Deserialize an affix from stash storage
    /// </summary>
    private Affix DeserializeAffixFromStash(SerializedAffix data)
    {
        if (data == null) return null;
        
        // Create affix with required constructor parameters
        var affix = new Affix(
            data.affixName,
            data.description,
            data.affixType,
            AffixTier.Tier1 // Default tier
        )
        {
            isRolled = data.isRolled,
            rolledValue = (int)data.rolledValue // Cast float to int
        };
        
        // Deserialize modifiers
        foreach (var modData in data.modifiers)
        {
            // Use 4-parameter constructor: statName, minValue, maxValue, modifierType
            var modifier = new AffixModifier(
                modData.modifierType.ToString(), // statName
                modData.minValue,
                modData.maxValue,
                modData.modifierType
            )
            {
                isRolled = modData.isRolled,
                rolledValue = modData.rolledValue,
                isDualRange = modData.isDualRange,
                rolledFirstValue = modData.rolledFirstValue,
                rolledSecondValue = modData.rolledSecondValue
            };
            
            affix.modifiers.Add(modifier);
        }
        
        return affix;
    }
}

/// <summary>
/// Data structure for saving stash
/// </summary>
[System.Serializable]
public class StashSaveData
{
    public List<SerializedItemData> items = new List<SerializedItemData>();
}

