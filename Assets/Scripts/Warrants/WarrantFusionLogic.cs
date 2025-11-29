using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles the logic for fusing 3 warrants into 1 with modifier locking support.
/// Used by the Peacekeeper faction for warrant enhancement.
/// </summary>
public static class WarrantFusionLogic
{
    /// <summary>
    /// Result of a warrant fusion operation
    /// </summary>
    public class FusionResult
    {
        public WarrantDefinition fusedWarrant;
        public bool success;
        public string errorMessage;
        
        public FusionResult(WarrantDefinition warrant, bool success, string error = null)
        {
            fusedWarrant = warrant;
            this.success = success;
            errorMessage = error;
        }
    }
    
    /// <summary>
    /// Fuses 3 warrants into 1 new warrant instance.
    /// Locked modifiers are guaranteed to be preserved from their source warrants.
    /// </summary>
    /// <param name="warrant1">First warrant to fuse</param>
    /// <param name="warrant2">Second warrant to fuse</param>
    /// <param name="warrant3">Third warrant to fuse</param>
    /// <param name="lockedModifiers">Dictionary mapping warrant index (0-2) to list of modifier IDs to lock from that warrant</param>
    /// <param name="warrantDatabase">Database to create the new warrant instance in</param>
    /// <returns>FusionResult containing the fused warrant or error information</returns>
    public static FusionResult FuseWarrants(
        WarrantDefinition warrant1,
        WarrantDefinition warrant2,
        WarrantDefinition warrant3,
        Dictionary<int, List<string>> lockedModifiers = null,
        WarrantDatabase warrantDatabase = null,
        WarrantNotableDefinition selectedNotable = null)
    {
        // Validate inputs
        if (warrant1 == null || warrant2 == null || warrant3 == null)
        {
            return new FusionResult(null, false, "All three warrants must be provided for fusion.");
        }
        
        if (warrantDatabase == null)
        {
            return new FusionResult(null, false, "WarrantDatabase is required to create fused warrant instance.");
        }
        
        // Cannot fuse Unique warrants
        if (warrant1.rarity == WarrantRarity.Unique || 
            warrant2.rarity == WarrantRarity.Unique || 
            warrant3.rarity == WarrantRarity.Unique)
        {
            return new FusionResult(null, false, "Cannot fuse Unique warrants.");
        }
        
        // Cannot fuse blueprints
        if (warrant1.isBlueprint || warrant2.isBlueprint || warrant3.isBlueprint)
        {
            return new FusionResult(null, false, "Cannot fuse blueprint warrants.");
        }
        
        // Create new warrant instance
        WarrantDefinition fusedWarrant = ScriptableObject.CreateInstance<WarrantDefinition>();
        
        // Copy base properties from the first warrant (highest priority)
        fusedWarrant.warrantId = $"fused_{warrant1.warrantId}_{System.DateTime.Now.Ticks}";
        fusedWarrant.displayName = $"Fused {warrant1.displayName}";
        fusedWarrant.icon = warrant1.icon;
        fusedWarrant.rangeDirection = warrant1.rangeDirection;
        fusedWarrant.rangeDepth = warrant1.rangeDepth;
        fusedWarrant.affectDiagonals = warrant1.affectDiagonals;
        fusedWarrant.isKeystone = false; // Fused warrants cannot be keystones
        fusedWarrant.isBlueprint = false;
        
        // Determine fused rarity (upgrade by one tier, max Rare)
        fusedWarrant.rarity = DetermineFusedRarity(warrant1.rarity, warrant2.rarity, warrant3.rarity);
        
        // Collect all modifiers from all three warrants
        List<WarrantModifier> allModifiers = new List<WarrantModifier>();
        
        // Add locked modifiers first (guaranteed to be included)
        if (lockedModifiers != null)
        {
            AddLockedModifiers(allModifiers, warrant1, 0, lockedModifiers);
            AddLockedModifiers(allModifiers, warrant2, 1, lockedModifiers);
            AddLockedModifiers(allModifiers, warrant3, 2, lockedModifiers);
        }
        
        // Add remaining modifiers from all warrants (excluding locked ones)
        AddUnlockedModifiers(allModifiers, warrant1, 0, lockedModifiers);
        AddUnlockedModifiers(allModifiers, warrant2, 1, lockedModifiers);
        AddUnlockedModifiers(allModifiers, warrant3, 2, lockedModifiers);
        
        // Remove duplicates (same modifierId) - keep the first occurrence
        List<WarrantModifier> uniqueModifiers = new List<WarrantModifier>();
        HashSet<string> seenModifierIds = new HashSet<string>();
        
        foreach (var modifier in allModifiers)
        {
            if (!seenModifierIds.Contains(modifier.modifierId))
            {
                uniqueModifiers.Add(modifier);
                seenModifierIds.Add(modifier.modifierId);
            }
        }
        
        // Limit modifiers based on rarity
        int maxModifiers = GetMaxModifiersForRarity(fusedWarrant.rarity);
        if (uniqueModifiers.Count > maxModifiers)
        {
            // Prioritize locked modifiers, then take first N
            uniqueModifiers = uniqueModifiers.Take(maxModifiers).ToList();
        }
        
        fusedWarrant.modifiers = uniqueModifiers;
        
        // Handle Notable: Use selected notable if provided, otherwise randomly select one
        if (selectedNotable != null)
        {
            // Use the selected notable
            fusedWarrant.notable = selectedNotable;
            fusedWarrant.notableId = !string.IsNullOrEmpty(selectedNotable.notableId) ? selectedNotable.notableId : selectedNotable.name;
        }
        else
        {
            // Fallback: If no notable selected, randomly select one from available
            List<WarrantNotableDefinition> availableNotables = new List<WarrantNotableDefinition>();
            if (warrant1.notable != null) availableNotables.Add(warrant1.notable);
            if (warrant2.notable != null) availableNotables.Add(warrant2.notable);
            if (warrant3.notable != null) availableNotables.Add(warrant3.notable);
            
            if (availableNotables.Count > 0)
            {
                // Randomly select one notable from available ones
                fusedWarrant.notable = availableNotables[Random.Range(0, availableNotables.Count)];
                fusedWarrant.notableId = fusedWarrant.notable != null ? fusedWarrant.notable.name : null;
            }
        }
        
        // Note: The fused warrant is a runtime instance and should be registered with WarrantLockerGrid
        // rather than being added to the database (which contains asset definitions)
        // The caller should handle adding it to the locker inventory
        
        Debug.Log($"[WarrantFusionLogic] Successfully fused 3 warrants into '{fusedWarrant.warrantId}' with {fusedWarrant.modifiers.Count} modifiers and rarity {fusedWarrant.rarity}");
        
        return new FusionResult(fusedWarrant, true);
    }
    
    private static void AddLockedModifiers(
        List<WarrantModifier> targetList,
        WarrantDefinition warrant,
        int warrantIndex,
        Dictionary<int, List<string>> lockedModifiers)
    {
        if (lockedModifiers == null || !lockedModifiers.ContainsKey(warrantIndex))
            return;
        
        var lockedIds = lockedModifiers[warrantIndex];
        if (lockedIds == null || lockedIds.Count == 0)
            return;
        
        foreach (var modifier in warrant.modifiers)
        {
            if (lockedIds.Contains(modifier.modifierId))
            {
                targetList.Add(modifier);
            }
        }
    }
    
    private static void AddUnlockedModifiers(
        List<WarrantModifier> targetList,
        WarrantDefinition warrant,
        int warrantIndex,
        Dictionary<int, List<string>> lockedModifiers)
    {
        HashSet<string> lockedIds = null;
        if (lockedModifiers != null && lockedModifiers.ContainsKey(warrantIndex))
        {
            lockedIds = new HashSet<string>(lockedModifiers[warrantIndex]);
        }
        
        foreach (var modifier in warrant.modifiers)
        {
            if (lockedIds == null || !lockedIds.Contains(modifier.modifierId))
            {
                targetList.Add(modifier);
            }
        }
    }
    
    private static WarrantRarity DetermineFusedRarity(
        WarrantRarity r1,
        WarrantRarity r2,
        WarrantRarity r3)
    {
        // Upgrade by one tier, max Rare
        // Common -> Magic -> Rare -> Rare (stays Rare)
        WarrantRarity highestInput = r1;
        if (r2 > highestInput) highestInput = r2;
        if (r3 > highestInput) highestInput = r3;
        
        // Upgrade by one tier
        switch (highestInput)
        {
            case WarrantRarity.Common:
                return WarrantRarity.Magic;
            case WarrantRarity.Magic:
                return WarrantRarity.Rare;
            case WarrantRarity.Rare:
                return WarrantRarity.Rare; // Stays Rare
            default:
                return WarrantRarity.Rare;
        }
    }
    
    private static int GetMaxModifiersForRarity(WarrantRarity rarity)
    {
        switch (rarity)
        {
            case WarrantRarity.Common:
                return 2;
            case WarrantRarity.Magic:
                return 4;
            case WarrantRarity.Rare:
                return 6;
            default:
                return 2;
        }
    }
}

