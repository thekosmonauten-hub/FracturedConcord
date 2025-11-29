using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles filtering of warrants in the locker by modifiers and rarities.
/// Integrates with WarrantLockerGrid to update the displayed items.
/// </summary>
public class WarrantLockerFilterController : MonoBehaviour
{
    [Header("Filter UI References")]
    [SerializeField] private ToggleGroup rarityToggleGroup;
    [SerializeField] private List<Toggle> rarityToggles = new List<Toggle>();
    [SerializeField] private TMP_Dropdown modifierDropdown;
    [SerializeField] private Button clearFiltersButton;
    
    [Header("References")]
    [SerializeField] private WarrantLockerGrid lockerGrid;
    [SerializeField] private List<WarrantDefinition> allWarrants = new List<WarrantDefinition>();
    
    private WarrantRarity? selectedRarity = null;
    private string selectedModifierId = null;
    private List<WarrantDefinition> filteredWarrants = new List<WarrantDefinition>();
    
    private void Awake()
    {
        InitializeFilters();
    }
    
    private void Start()
    {
        if (clearFiltersButton != null)
        {
            clearFiltersButton.onClick.AddListener(ClearFilters);
        }
        
        SetupRarityToggles();
        SetupModifierDropdown();

        if ((allWarrants == null || allWarrants.Count == 0) && lockerGrid != null)
        {
            allWarrants = new List<WarrantDefinition>(lockerGrid.GetInventorySnapshot());
            RefreshModifierDropdown();
        }
    }
    
    private void InitializeFilters()
    {
        if (lockerGrid == null)
        {
            lockerGrid = GetComponentInParent<WarrantLockerGrid>();
        }
    }
    
    public void SetAvailableWarrants(List<WarrantDefinition> warrants)
    {
        allWarrants = warrants ?? new List<WarrantDefinition>();
        RefreshModifierDropdown();
        ApplyFilters();
    }
    
    private void SetupRarityToggles()
    {
        if (rarityToggles == null || rarityToggles.Count == 0) return;
        
        for (int i = 0; i < rarityToggles.Count; i++)
        {
            int index = i; // Capture for lambda
            Toggle toggle = rarityToggles[i];
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener((bool isOn) =>
                {
                    if (isOn)
                    {
                        OnRarityFilterChanged(index);
                    }
                    else
                    {
                        selectedRarity = null;
                        ApplyFilters();
                    }
                });
            }
        }
    }
    
    private void SetupModifierDropdown()
    {
        if (modifierDropdown != null)
        {
            modifierDropdown.onValueChanged.AddListener(OnModifierFilterChanged);
        }
    }
    
    private void OnRarityFilterChanged(int rarityIndex)
    {
        if (rarityIndex < 0 || rarityIndex >= System.Enum.GetValues(typeof(WarrantRarity)).Length)
        {
            selectedRarity = null;
        }
        else
        {
            selectedRarity = (WarrantRarity)rarityIndex;
        }
        
        ApplyFilters();
    }
    
    private void OnModifierFilterChanged(int dropdownIndex)
    {
        if (modifierDropdown == null || dropdownIndex <= 0)
        {
            selectedModifierId = null;
        }
        else
        {
            // Index 0 is "All Modifiers", so subtract 1
            List<string> allModifierIds = GetAllModifierIds();
            if (dropdownIndex - 1 < allModifierIds.Count)
            {
                selectedModifierId = allModifierIds[dropdownIndex - 1];
            }
            else
            {
                selectedModifierId = null;
            }
        }
        
        ApplyFilters();
    }
    
    private void RefreshModifierDropdown()
    {
        if (modifierDropdown == null) return;
        
        List<string> modifierIds = GetAllModifierIds();
        List<string> options = new List<string> { "All Modifiers" };
        
        foreach (var modifierId in modifierIds)
        {
            // Try to find a display name for this modifier
            string displayName = GetModifierDisplayName(modifierId);
            options.Add(displayName);
        }
        
        modifierDropdown.ClearOptions();
        modifierDropdown.AddOptions(options);
    }
    
    private List<string> GetAllModifierIds()
    {
        HashSet<string> modifierIds = new HashSet<string>();
        
        foreach (var warrant in allWarrants)
        {
            if (warrant == null || warrant.modifiers == null) continue;
            
            foreach (var modifier in warrant.modifiers)
            {
                if (modifier != null && !string.IsNullOrEmpty(modifier.modifierId))
                {
                    modifierIds.Add(modifier.modifierId);
                }
            }
        }
        
        return modifierIds.OrderBy(id => id).ToList();
    }
    
    private string GetModifierDisplayName(string modifierId)
    {
        foreach (var warrant in allWarrants)
        {
            if (warrant == null || warrant.modifiers == null) continue;
            
            var modifier = warrant.modifiers.FirstOrDefault(m => m != null && m.modifierId == modifierId);
            if (modifier != null && !string.IsNullOrEmpty(modifier.displayName))
            {
                return modifier.displayName;
            }
        }
        
        return modifierId; // Fallback to ID if no display name found
    }
    
    private void ApplyFilters()
    {
        if (lockerGrid != null)
        {
            allWarrants = new List<WarrantDefinition>(lockerGrid.GetInventorySnapshot());
        }

        filteredWarrants = new List<WarrantDefinition>(allWarrants);
        
        // Rarity filter
        if (selectedRarity.HasValue)
        {
            filteredWarrants = filteredWarrants
                .Where(w => w != null && w.rarity == selectedRarity.Value)
                .ToList();
        }
        
        // Modifier filter
        if (!string.IsNullOrEmpty(selectedModifierId))
        {
            filteredWarrants = filteredWarrants
                .Where(w => w != null && 
                    w.modifiers != null && 
                    w.modifiers.Any(m => m != null && m.modifierId == selectedModifierId))
                .ToList();
        }
        
        // Update grid
        if (lockerGrid != null)
        {
            lockerGrid.ApplyFilter(filteredWarrants);
        }
    }
    
    public void ClearFilters()
    {
        selectedRarity = null;
        selectedModifierId = null;
        
        if (rarityToggleGroup != null)
        {
            rarityToggleGroup.SetAllTogglesOff();
        }
        
        if (modifierDropdown != null)
        {
            modifierDropdown.value = 0;
        }
        
        ApplyFilters();
    }
    
    public List<WarrantDefinition> GetFilteredWarrants()
    {
        return new List<WarrantDefinition>(filteredWarrants);
    }
}

