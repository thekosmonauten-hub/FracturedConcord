using UnityEngine;
using UnityEngine.UIElements;

public class CombatSceneUI : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    public CombatManager combatManager;
    public CombatUI combatUI;
    public AnimatedCombatUI animatedCombatUI; // Updated to AnimatedCombatUI
    
    private void Start()
    {
        // Auto-find components if not set
        if (uiDocument == null)
            uiDocument = FindFirstObjectByType<UIDocument>();
        if (combatManager == null)
            combatManager = FindFirstObjectByType<CombatManager>();
        if (combatUI == null)
            combatUI = FindFirstObjectByType<CombatUI>();
        if (animatedCombatUI == null)
            animatedCombatUI = FindFirstObjectByType<AnimatedCombatUI>();
        
        // Pass references to CombatUI
        if (combatUI != null)
        {
            combatUI.combatManager = combatManager;
        }
        
        // Initialize animated combat UI
        if (animatedCombatUI != null)
        {
            Debug.Log("AnimatedCombatUI found and initialized.");
        }
        else
        {
            Debug.LogWarning("AnimatedCombatUI not found! Cards will not be displayed.");
        }
    }
}
