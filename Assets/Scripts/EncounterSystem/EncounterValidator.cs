using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Validates encounter data for integrity and correctness.
/// </summary>
public static class EncounterValidator
{
    public class ValidationResult
    {
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        
        public bool IsValid => errors.Count == 0;
        
        public void AddError(string error)
        {
            errors.Add(error);
        }
        
        public void AddWarning(string warning)
        {
            warnings.Add(warning);
        }
        
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (errors.Count > 0)
            {
                sb.AppendLine($"Errors ({errors.Count}):");
                foreach (var error in errors)
                {
                    sb.AppendLine($"  - {error}");
                }
            }
            if (warnings.Count > 0)
            {
                sb.AppendLine($"Warnings ({warnings.Count}):");
                foreach (var warning in warnings)
                {
                    sb.AppendLine($"  - {warning}");
                }
            }
            if (IsValid && warnings.Count == 0)
            {
                sb.AppendLine("Validation passed!");
            }
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// Validate a collection of encounters.
    /// </summary>
    public static ValidationResult ValidateEncounters(IEnumerable<EncounterData> encounters)
    {
        ValidationResult result = new ValidationResult();
        List<EncounterData> encounterList = encounters.ToList();
        
        if (encounterList.Count == 0)
        {
            result.AddWarning("No encounters to validate.");
            return result;
        }
        
        // Check for duplicate IDs
        ValidateDuplicateIDs(encounterList, result);
        
        // Check for missing prerequisites
        ValidatePrerequisiteReferences(encounterList, result);
        
        // Check for circular prerequisites
        ValidatePrerequisiteCycles(encounterList, result);
        
        // Check for encounter 1
        ValidateEncounter1(encounterList, result);
        
        // Check for invalid scene names
        ValidateSceneNames(encounterList, result);
        
        return result;
    }
    
    private static void ValidateDuplicateIDs(List<EncounterData> encounters, ValidationResult result)
    {
        var duplicates = encounters
            .GroupBy(e => e.encounterID)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
        
        foreach (var dupId in duplicates)
        {
            result.AddError($"Duplicate encounter ID: {dupId}");
        }
    }
    
    private static void ValidatePrerequisiteReferences(List<EncounterData> encounters, ValidationResult result)
    {
        var encounterIDs = new HashSet<int>(encounters.Select(e => e.encounterID));
        
        foreach (var encounter in encounters)
        {
            if (encounter.prerequisiteEncounterIDs != null)
            {
                foreach (var prereqId in encounter.prerequisiteEncounterIDs)
                {
                    if (prereqId == encounter.encounterID)
                    {
                        result.AddError($"Encounter {encounter.encounterID} has itself as a prerequisite.");
                    }
                    else if (!encounterIDs.Contains(prereqId))
                    {
                        result.AddError($"Encounter {encounter.encounterID} references missing prerequisite: {prereqId}");
                    }
                }
            }
        }
    }
    
    private static void ValidatePrerequisiteCycles(List<EncounterData> encounters, ValidationResult result)
    {
        var encounterDict = encounters.ToDictionary(e => e.encounterID);
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();
        
        foreach (var encounter in encounters)
        {
            if (!visited.Contains(encounter.encounterID))
            {
                if (HasCycle(encounter.encounterID, encounterDict, visited, recursionStack))
                {
                    result.AddError($"Circular prerequisite detected involving encounter {encounter.encounterID}");
                }
            }
        }
    }
    
    private static bool HasCycle(int encounterID, Dictionary<int, EncounterData> encounters, HashSet<int> visited, HashSet<int> recursionStack)
    {
        visited.Add(encounterID);
        recursionStack.Add(encounterID);
        
        var encounter = encounters[encounterID];
        if (encounter.prerequisiteEncounterIDs != null)
        {
            foreach (var prereqId in encounter.prerequisiteEncounterIDs)
            {
                if (!visited.Contains(prereqId))
                {
                    if (HasCycle(prereqId, encounters, visited, recursionStack))
                    {
                        return true;
                    }
                }
                else if (recursionStack.Contains(prereqId))
                {
                    return true;
                }
            }
        }
        
        recursionStack.Remove(encounterID);
        return false;
    }
    
    private static void ValidateEncounter1(List<EncounterData> encounters, ValidationResult result)
    {
        var encounter1 = encounters.FirstOrDefault(e => e.encounterID == 1);
        if (encounter1 == null)
        {
            result.AddError("Encounter 1 is missing. There must be an encounter with ID 1.");
        }
        else if (encounter1.prerequisiteEncounterIDs != null && encounter1.prerequisiteEncounterIDs.Count > 0)
        {
            result.AddWarning("Encounter 1 has prerequisites, but it should always be unlocked. Prerequisites will be ignored.");
        }
    }
    
    private static void ValidateSceneNames(List<EncounterData> encounters, ValidationResult result)
    {
        foreach (var encounter in encounters)
        {
            if (string.IsNullOrWhiteSpace(encounter.sceneName))
            {
                result.AddError($"Encounter {encounter.encounterID} has no scene name.");
            }
        }
    }
}

