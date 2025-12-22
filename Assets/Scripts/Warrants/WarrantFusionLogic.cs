using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        
        // Check for sealed affixes - warrants with sealed affixes cannot fuse with other sealed warrants
        bool hasSealed1 = HasSealedAffix(warrant1);
        bool hasSealed2 = HasSealedAffix(warrant2);
        bool hasSealed3 = HasSealedAffix(warrant3);
        int sealedCount = (hasSealed1 ? 1 : 0) + (hasSealed2 ? 1 : 0) + (hasSealed3 ? 1 : 0);
        
        if (sealedCount > 1)
        {
            return new FusionResult(null, false, "Cannot fuse multiple warrants with sealed/locked affixes. Only one sealed warrant can be fused at a time.");
        }
        
        // Create new warrant instance
        WarrantDefinition fusedWarrant = ScriptableObject.CreateInstance<WarrantDefinition>();
        
        // Copy base properties from the first warrant (highest priority)
        fusedWarrant.warrantId = $"fused_{warrant1.warrantId}_{System.DateTime.Now.Ticks}";
        
        // Set display name and icon based on selected notable (if available)
        if (selectedNotable != null)
        {
            string notableName = !string.IsNullOrEmpty(selectedNotable.displayName) 
                ? selectedNotable.displayName 
                : (!string.IsNullOrEmpty(selectedNotable.notableId) ? selectedNotable.notableId : "Unknown");
            fusedWarrant.displayName = $"Fused Warrant of {notableName}";
        }
        else
        {
            // Fallback to first warrant's name if no notable selected
            fusedWarrant.displayName = $"Fused {warrant1.displayName}";
        }
        
        // Get icon from icon library if available, otherwise use first warrant's icon
        if (warrantDatabase != null)
        {
            Sprite randomIcon = GetRandomIconFromDatabase(warrantDatabase);
            fusedWarrant.icon = randomIcon != null ? randomIcon : warrant1.icon;
        }
        else
        {
            fusedWarrant.icon = warrant1.icon;
        }
        fusedWarrant.rangeDirection = warrant1.rangeDirection;
        fusedWarrant.rangeDepth = warrant1.rangeDepth;
        fusedWarrant.affectDiagonals = warrant1.affectDiagonals;
        fusedWarrant.isKeystone = false; // Fused warrants cannot be keystones
        fusedWarrant.isBlueprint = false;
        
        // Determine fused rarity (upgrade by one tier, max Rare)
        fusedWarrant.rarity = DetermineFusedRarity(warrant1.rarity, warrant2.rarity, warrant3.rarity);
        
        // NEW FUSION LOGIC: Only include selected affix + random affixes (not all affixes)
        // Rules:
        // - 3 Common → 1 Magic: Selected notable + Selected affix + 1 random affix (total 2 affixes)
        // - 3 Magic → 1 Rare: Selected notable + Selected affix + 1-2 random affixes (total 2-3 affixes, but can be 3-4 if including locked affixes)
        
        List<WarrantModifier> finalModifiers = new List<WarrantModifier>();
        HashSet<string> usedModifierIds = new HashSet<string>();
        
        // Step 1: Add the selected/locked affix (guaranteed to be included)
        if (lockedModifiers != null)
        {
            AddLockedModifiers(finalModifiers, warrant1, 0, lockedModifiers, usedModifierIds);
            AddLockedModifiers(finalModifiers, warrant2, 1, lockedModifiers, usedModifierIds);
            AddLockedModifiers(finalModifiers, warrant3, 2, lockedModifiers, usedModifierIds);
        }
        
        // Step 1.5: Add sealed affix if present (only one warrant can have sealed affix)
        WarrantModifier sealedAffix = null;
        if (hasSealed1)
        {
            sealedAffix = GetSealedAffix(warrant1);
        }
        else if (hasSealed2)
        {
            sealedAffix = GetSealedAffix(warrant2);
        }
        else if (hasSealed3)
        {
            sealedAffix = GetSealedAffix(warrant3);
        }
        
        if (sealedAffix != null && !usedModifierIds.Contains(sealedAffix.modifierId))
        {
            finalModifiers.Add(sealedAffix);
            usedModifierIds.Add(sealedAffix.modifierId);
            Debug.Log($"[WarrantFusionLogic] Added sealed affix: {sealedAffix.displayName ?? sealedAffix.modifierId}");
        }
        
        Debug.Log($"[WarrantFusionLogic] After adding selected affix and sealed affix: {finalModifiers.Count} modifiers");
        
        // Step 2: Determine how many random affixes to add based on rarity and sealed affix presence
        bool hasSealedAffix = sealedAffix != null;
        int randomAffixesNeeded = GetRandomAffixCountForRarity(fusedWarrant.rarity, hasSealedAffix);
        
        Debug.Log($"[WarrantFusionLogic] Fusing to {fusedWarrant.rarity}: already have {finalModifiers.Count} affix(es) (selected + sealed), need {randomAffixesNeeded} random affix(es)");
        
        // Step 3: Roll random affixes from WarrantAffixDatabase (NOT from input warrants)
        if (randomAffixesNeeded > 0 && warrantDatabase != null)
        {
            var affixDb = GetAffixDatabase(warrantDatabase);
            if (affixDb != null)
            {
                var regularAffixes = affixDb.GetRegularAffixes();
                if (regularAffixes != null && regularAffixes.Count > 0)
                {
                    int added = 0;
                    int attempts = 0;
                    int maxAttempts = randomAffixesNeeded * 10; // Prevent infinite loops
                    
                    while (added < randomAffixesNeeded && attempts < maxAttempts)
                    {
                        attempts++;
                        
                        // Roll a random affix from the database
                        var affixEntry = RollAffixFromDatabase(regularAffixes);
                        if (affixEntry == null || string.IsNullOrWhiteSpace(affixEntry.affixId))
                            continue;
                        
                        // Skip if we already have this affix (by ID)
                        if (usedModifierIds.Contains(affixEntry.affixId))
                            continue;
                        
                        // Roll the value
                        int minValue = Mathf.RoundToInt(affixEntry.minPercent);
                        int maxValue = Mathf.RoundToInt(affixEntry.maxPercent);
                        int rolledValue = Random.Range(minValue, maxValue + 1);
                        
                        // Create the modifier
                        string description;
                        if (affixEntry.isFlat)
                        {
                            description = !string.IsNullOrWhiteSpace(affixEntry.displayName) 
                                ? affixEntry.displayName 
                                : $"+{rolledValue} {affixEntry.statKey}";
                        }
                        else
                        {
                            description = !string.IsNullOrWhiteSpace(affixEntry.displayName) 
                                ? affixEntry.displayName 
                                : $"{rolledValue:+0;-0;0}% {affixEntry.statKey}";
                        }
                        
                        var mod = new WarrantModifier
                        {
                            modifierId = affixEntry.affixId,
                            displayName = affixEntry.displayName,
                            operation = WarrantModifierOperation.Additive,
                            value = rolledValue,
                            description = description
                        };
                        
                        finalModifiers.Add(mod);
                        usedModifierIds.Add(affixEntry.affixId);
                        added++;
                        
                        Debug.Log($"[WarrantFusionLogic] Rolled random affix from database: {affixEntry.displayName ?? affixEntry.affixId} ({rolledValue}%)");
                    }
                    
                    Debug.Log($"[WarrantFusionLogic] Added {added} random affixes from database (attempted {attempts} times)");
                }
                else
                {
                    Debug.LogWarning("[WarrantFusionLogic] WarrantAffixDatabase has no regular affixes available for rolling.");
                }
            }
            else
            {
                Debug.LogWarning("[WarrantFusionLogic] WarrantDatabase does not have an affix database assigned. Cannot roll random affixes.");
            }
        }
        
        fusedWarrant.modifiers = finalModifiers;
        
        // Handle Notable: Use ONLY the selected notable if provided
        // IMPORTANT: Only one notable should be set on the fused warrant
        // First, explicitly clear any existing notable to prevent duplicates
        fusedWarrant.notable = null;
        fusedWarrant.notableId = null;
        
        if (selectedNotable != null)
        {
            // Use ONLY the selected notable (this is the user's choice)
            // Do NOT include notables from input warrants - only use the selected one
            fusedWarrant.notable = selectedNotable;
            fusedWarrant.notableId = !string.IsNullOrEmpty(selectedNotable.notableId) ? selectedNotable.notableId : selectedNotable.name;
            Debug.Log($"[WarrantFusionLogic] Using ONLY selected notable: {selectedNotable.displayName ?? selectedNotable.notableId} (cleared any existing notable first)");
        }
        else
        {
            // Fallback: If no notable selected, randomly select ONE from available
            List<WarrantNotableDefinition> availableNotables = new List<WarrantNotableDefinition>();
            if (warrant1.notable != null) availableNotables.Add(warrant1.notable);
            if (warrant2.notable != null) availableNotables.Add(warrant2.notable);
            if (warrant3.notable != null) availableNotables.Add(warrant3.notable);
            
            if (availableNotables.Count > 0)
            {
                // Randomly select ONE notable from available ones (not multiple)
                fusedWarrant.notable = availableNotables[Random.Range(0, availableNotables.Count)];
                fusedWarrant.notableId = fusedWarrant.notable != null ? (fusedWarrant.notable.notableId ?? fusedWarrant.notable.name) : null;
                Debug.Log($"[WarrantFusionLogic] Randomly selected notable: {fusedWarrant.notable.displayName ?? fusedWarrant.notableId}");
            }
            else
            {
                // No notable available - clear it
                fusedWarrant.notable = null;
                fusedWarrant.notableId = null;
                Debug.Log("[WarrantFusionLogic] No notable available from input warrants.");
            }
        }
        
        // Ensure only one notable is set (safety check)
        if (fusedWarrant.notable != null && fusedWarrant.notableId == null)
        {
            fusedWarrant.notableId = fusedWarrant.notable.notableId ?? fusedWarrant.notable.name;
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
        Dictionary<int, List<string>> lockedModifiers,
        HashSet<string> usedModifierIds)
    {
        if (lockedModifiers == null || !lockedModifiers.ContainsKey(warrantIndex))
            return;
        
        var lockedIds = lockedModifiers[warrantIndex];
        if (lockedIds == null || lockedIds.Count == 0)
            return;
        
        // Get notable modifier IDs to exclude
        HashSet<string> notableModifierIds = GetNotableModifierIds(warrant);
        
        foreach (var modifier in warrant.modifiers)
        {
            // Only add if it's locked AND not a notable modifier AND not already used
            if (lockedIds.Contains(modifier.modifierId) && 
                !notableModifierIds.Contains(modifier.modifierId) &&
                !usedModifierIds.Contains(modifier.modifierId))
            {
                // Mark the selected affix as sealed (yellow/gold color in UI)
                // Check if it's already sealed, if not, add the prefix
                string sealedModifierId = modifier.modifierId;
                if (!sealedModifierId.StartsWith("__SEALED__", System.StringComparison.OrdinalIgnoreCase) &&
                    !sealedModifierId.StartsWith("__LOCKED__", System.StringComparison.OrdinalIgnoreCase))
                {
                    sealedModifierId = $"__SEALED__{modifier.modifierId}";
                }
                
                // Create a copy with the sealed modifier ID
                var sealedModifier = new WarrantModifier
                {
                    modifierId = sealedModifierId,
                    displayName = modifier.displayName,
                    operation = modifier.operation,
                    value = modifier.value,
                    description = modifier.description
                };
                
                targetList.Add(sealedModifier);
                usedModifierIds.Add(modifier.modifierId); // Use original ID to prevent duplicates
                usedModifierIds.Add(sealedModifierId); // Also add sealed ID
            }
        }
    }
    
    /// <summary>
    /// Collects available affixes from a warrant that can be used for random selection.
    /// Excludes notable modifiers, locked/selected modifiers, sealed affixes, and already used modifiers.
    /// </summary>
    private static void CollectAvailableAffixes(
        List<WarrantModifier> targetList,
        WarrantDefinition warrant,
        int warrantIndex,
        Dictionary<int, List<string>> lockedModifiers,
        HashSet<string> usedModifierIds)
    {
        HashSet<string> lockedIds = null;
        if (lockedModifiers != null && lockedModifiers.ContainsKey(warrantIndex))
        {
            lockedIds = new HashSet<string>(lockedModifiers[warrantIndex]);
        }
        
        // Get notable modifier IDs to exclude
        HashSet<string> notableModifierIds = GetNotableModifierIds(warrant);
        
        foreach (var modifier in warrant.modifiers)
        {
            if (modifier == null || string.IsNullOrEmpty(modifier.modifierId))
                continue;
            
            // Check if this is a sealed affix (exclude from random pool)
            bool isSealed = modifier.modifierId.StartsWith("__SEALED__", System.StringComparison.OrdinalIgnoreCase) ||
                           modifier.modifierId.StartsWith("__LOCKED__", System.StringComparison.OrdinalIgnoreCase);
            
            // Only add if it's not locked, not a notable modifier, not sealed, and not already used
            if (!isSealed &&
                (lockedIds == null || !lockedIds.Contains(modifier.modifierId)) && 
                !notableModifierIds.Contains(modifier.modifierId) &&
                !usedModifierIds.Contains(modifier.modifierId))
            {
                targetList.Add(modifier);
            }
        }
    }
    
    /// <summary>
    /// Gets a set of modifier IDs that belong to the warrant's notable.
    /// These should be excluded when collecting modifiers for fusion.
    /// </summary>
    private static HashSet<string> GetNotableModifierIds(WarrantDefinition warrant)
    {
        HashSet<string> notableModifierIds = new HashSet<string>();
        
        if (warrant.notable != null && warrant.notable.modifiers != null)
        {
            foreach (var notableMod in warrant.notable.modifiers)
            {
                if (notableMod != null && !string.IsNullOrEmpty(notableMod.modifierId))
                {
                    notableModifierIds.Add(notableMod.modifierId);
                    // Also add by displayName for loose matching
                    if (!string.IsNullOrEmpty(notableMod.displayName))
                    {
                        notableModifierIds.Add(notableMod.displayName.ToLowerInvariant());
                    }
                }
            }
        }
        
        return notableModifierIds;
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
    
    /// <summary>
    /// Gets the number of random affixes to add when fusing to the given rarity.
    /// Rules:
    /// - 3 Common → 1 Magic: 1 selected affix + 1 random = 2 total
    /// - 3 Magic → 1 Rare WITHOUT sealed: 1 selected affix + 1-3 random = 2-4 total
    /// - 3 Magic → 1 Rare WITH sealed: 1 selected affix + 1 sealed affix + 1-2 random = 3-4 total
    /// </summary>
    private static int GetRandomAffixCountForRarity(WarrantRarity rarity, bool hasSealedAffix)
    {
        switch (rarity)
        {
            case WarrantRarity.Common:
                return 0; // Common: 1 affix total (shouldn't happen in fusion, but just in case)
            case WarrantRarity.Magic:
                return 1; // Magic: 1 selected + 1 random = 2 total
            case WarrantRarity.Rare:
                if (hasSealedAffix)
                {
                    // Rare WITH sealed: 1 selected + 1 sealed + 1-2 random = 3-4 total
                    // Randomly choose 1 or 2 additional affixes
                    return Random.Range(1, 3); // Returns 1 or 2
                }
                else
                {
                    // Rare WITHOUT sealed: 1 selected + 1-3 random = 2-4 total
                    // Randomly choose 1, 2, or 3 additional affixes
                    return Random.Range(1, 4); // Returns 1, 2, or 3
                }
            default:
                return 1;
        }
    }
    
    /// <summary>
    /// Legacy method - kept for compatibility but not used in new fusion logic.
    /// </summary>
    private static int GetMaxModifiersForRarity(WarrantRarity rarity)
    {
        switch (rarity)
        {
            case WarrantRarity.Common:
                return 1;
            case WarrantRarity.Magic:
                return 2;
            case WarrantRarity.Rare:
                return 4;
            default:
                return 1;
        }
    }
    
    /// <summary>
    /// Checks if a warrant has a sealed/locked affix.
    /// Sealed affixes are identified by having a modifier with "__SEALED__" prefix in modifierId,
    /// or by checking if the warrant came from a previous fusion with a selected affix.
    /// </summary>
    private static bool HasSealedAffix(WarrantDefinition warrant)
    {
        if (warrant == null || warrant.modifiers == null)
            return false;
        
        // Check if any modifier has the sealed identifier
        foreach (var modifier in warrant.modifiers)
        {
            if (modifier != null && !string.IsNullOrEmpty(modifier.modifierId))
            {
                // Check for sealed identifier in modifierId
                if (modifier.modifierId.StartsWith("__SEALED__", System.StringComparison.OrdinalIgnoreCase) ||
                    modifier.modifierId.StartsWith("__LOCKED__", System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Gets the sealed affix from a warrant (assumes HasSealedAffix was already checked).
    /// </summary>
    private static WarrantModifier GetSealedAffix(WarrantDefinition warrant)
    {
        if (warrant == null || warrant.modifiers == null)
            return null;
        
        foreach (var modifier in warrant.modifiers)
        {
            if (modifier != null && !string.IsNullOrEmpty(modifier.modifierId))
            {
                // Check for sealed identifier in modifierId
                if (modifier.modifierId.StartsWith("__SEALED__", System.StringComparison.OrdinalIgnoreCase) ||
                    modifier.modifierId.StartsWith("__LOCKED__", System.StringComparison.OrdinalIgnoreCase))
                {
                    return modifier;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the WarrantAffixDatabase from a WarrantDatabase using reflection (since it's private).
    /// </summary>
    private static WarrantAffixDatabase GetAffixDatabase(WarrantDatabase warrantDatabase)
    {
        if (warrantDatabase == null)
            return null;
        
        // Use reflection to access the private affixDatabase field
        var dbType = typeof(WarrantDatabase);
        var affixDbField = dbType.GetField("affixDatabase", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (affixDbField != null)
        {
            return affixDbField.GetValue(warrantDatabase) as WarrantAffixDatabase;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets a random icon from the WarrantDatabase's icon library using reflection.
    /// </summary>
    private static Sprite GetRandomIconFromDatabase(WarrantDatabase warrantDatabase)
    {
        if (warrantDatabase == null)
            return null;
        
        // Use reflection to access the private iconLibrary field and call GetRandomIcon
        var dbType = typeof(WarrantDatabase);
        var iconLibField = dbType.GetField("iconLibrary", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (iconLibField != null)
        {
            var iconLibrary = iconLibField.GetValue(warrantDatabase) as WarrantIconLibrary;
            if (iconLibrary != null)
            {
                return iconLibrary.GetRandomIcon();
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Rolls a random affix from a list of WarrantAffixEntry using weighted selection.
    /// </summary>
    private static WarrantAffixDatabase.WarrantAffixEntry RollAffixFromDatabase(List<WarrantAffixDatabase.WarrantAffixEntry> pool)
    {
        if (pool == null || pool.Count == 0)
            return null;
        
        int totalWeight = 0;
        foreach (var entry in pool)
        {
            if (entry != null && entry.weight > 0)
            {
                totalWeight += entry.weight;
            }
        }
        
        if (totalWeight <= 0)
            return null;
        
        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in pool)
        {
            if (entry == null || entry.weight <= 0)
                continue;
            
            cumulative += entry.weight;
            if (roll < cumulative)
            {
                return entry;
            }
        }
        
        // Fallback: return last valid entry
        return pool.LastOrDefault(e => e != null && e.weight > 0);
    }
}


