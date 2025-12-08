using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages aura activation and deactivation
/// Handles reliance cost checking and updates character data
/// </summary>
public class AuraManagerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AuraStorageUI auraStorage;
    [SerializeField] private ActiveAurasDisplay activeAurasDisplay;
    
    private CharacterManager characterManager;
    
    void Start()
    {
        characterManager = CharacterManager.Instance;
        
        if (auraStorage != null)
        {
            auraStorage.SetAuraManager(this);
            auraStorage.OnAuraClicked += OnAuraClicked;
        }
        
        RefreshDisplays();
    }
    
    /// <summary>
    /// Toggle aura activation (activate if inactive, deactivate if active)
    /// </summary>
    public void ToggleAura(RelianceAura aura)
    {
        if (aura == null || characterManager == null || !characterManager.HasCharacter())
        {
            Debug.LogWarning("[AuraManagerUI] Cannot toggle aura - missing aura or character");
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        
        // Check if owned
        if (character.ownedRelianceAuras == null || !character.ownedRelianceAuras.Contains(aura.auraName))
        {
            Debug.LogWarning($"[AuraManagerUI] Cannot activate {aura.auraName} - not owned");
            return;
        }
        
        // Check if active
        bool isActive = character.activeRelianceAuras != null && character.activeRelianceAuras.Contains(aura.auraName);
        
        if (isActive)
        {
            // Deactivate
            DeactivateAura(aura, character);
        }
        else
        {
            // Activate (check reliance cost)
            ActivateAura(aura, character);
        }
        
        RefreshDisplays();
    }
    
    /// <summary>
    /// Activate an aura (check reliance cost)
    /// </summary>
    private void ActivateAura(RelianceAura aura, Character character)
    {
        if (character.activeRelianceAuras == null)
        {
            character.activeRelianceAuras = new List<string>();
        }
        
        // Calculate total reliance cost of all active auras + this one
        int currentCost = GetTotalRelianceCost(character);
        int newCost = currentCost + aura.relianceCost;
        
        // Check if player has enough reliance (you'll need to implement this check)
        // For now, we'll just activate it
        // TODO: Add reliance cost checking
        
        if (!character.activeRelianceAuras.Contains(aura.auraName))
        {
            character.activeRelianceAuras.Add(aura.auraName);
            Debug.Log($"[AuraManagerUI] Activated {aura.auraName} (Reliance cost: {aura.relianceCost}, Total: {newCost})");
        }
    }
    
    /// <summary>
    /// Deactivate an aura
    /// </summary>
    private void DeactivateAura(RelianceAura aura, Character character)
    {
        if (character.activeRelianceAuras != null)
        {
            character.activeRelianceAuras.Remove(aura.auraName);
            Debug.Log($"[AuraManagerUI] Deactivated {aura.auraName}");
        }
    }
    
    /// <summary>
    /// Get total reliance cost of all active auras
    /// </summary>
    private int GetTotalRelianceCost(Character character)
    {
        if (character.activeRelianceAuras == null || character.activeRelianceAuras.Count == 0)
            return 0;
        
        int totalCost = 0;
        RelianceAuraDatabase database = RelianceAuraDatabase.Instance;
        
        if (database != null)
        {
            foreach (string auraName in character.activeRelianceAuras)
            {
                RelianceAura aura = database.GetAura(auraName);
                if (aura != null)
                {
                    totalCost += aura.relianceCost;
                }
            }
        }
        
        return totalCost;
    }
    
    /// <summary>
    /// Refresh all displays
    /// </summary>
    public void RefreshDisplays()
    {
        if (auraStorage != null)
        {
            auraStorage.Refresh();
        }
        
        if (activeAurasDisplay != null)
        {
            activeAurasDisplay.Refresh();
        }
    }
    
    private void OnAuraClicked(RelianceAura aura)
    {
        // This is handled by ToggleAura
    }
}

