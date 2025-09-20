using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject CharacterStatsPanel;
    public GameObject InventoryPanel;
    public GameObject EquipmentPanel;
    public GameObject SkillTreePanel;
    public GameObject QuestPanel;
    public GameObject SettingsPanel;

    private void Start()
    {
        // Initialize panels - start with CharacterStatsPanel inactive
        if (CharacterStatsPanel != null)
        {
            CharacterStatsPanel.SetActive(false);
        }
        
        // Keep other panels active for now (adjust as needed)
        if (InventoryPanel != null) InventoryPanel.SetActive(true);
        if (EquipmentPanel != null) EquipmentPanel.SetActive(true);
        if (SkillTreePanel != null) SkillTreePanel.SetActive(true);
        if (QuestPanel != null) QuestPanel.SetActive(true);
        if (SettingsPanel != null) SettingsPanel.SetActive(true);
    }

    public void ToggleCharacterStats()
    {
        // Use the centralized CharacterStatsPanelManager if available
        if (CharacterStatsPanelManager.Instance != null)
        {
            CharacterStatsPanelManager.Instance.TogglePanel();
        }
        else
        {
            // Fallback to direct panel control
            if (CharacterStatsPanel != null)
            {
                CharacterStatsPanel.SetActive(!CharacterStatsPanel.activeSelf);
                Debug.Log($"CharacterStatsPanel toggled: {CharacterStatsPanel.activeSelf}");
            }
            else
            {
                Debug.LogWarning("CharacterStatsPanel is not assigned!");
            }
        }
    }

    public void ShowCharacterStats()
    {
        // Use the centralized CharacterStatsPanelManager if available
        if (CharacterStatsPanelManager.Instance != null)
        {
            CharacterStatsPanelManager.Instance.ShowPanel();
        }
        else
        {
            // Fallback to direct panel control
            SetPanelVisibility(CharacterStatsPanel, true);
        }
    }

    public void HideCharacterStats()
    {
        // Use the centralized CharacterStatsPanelManager if available
        if (CharacterStatsPanelManager.Instance != null)
        {
            CharacterStatsPanelManager.Instance.HidePanel();
        }
        else
        {
            // Fallback to direct panel control
            SetPanelVisibility(CharacterStatsPanel, false);
        }
    }

    public bool IsCharacterStatsVisible()
    {
        // Use the centralized CharacterStatsPanelManager if available
        if (CharacterStatsPanelManager.Instance != null)
        {
            return CharacterStatsPanelManager.Instance.IsPanelVisible();
        }
        else
        {
            // Fallback to direct panel control
            return CharacterStatsPanel != null && CharacterStatsPanel.activeSelf;
        }
    }

    public void ToggleInventory()
    {
        if (InventoryPanel != null)
        {
            InventoryPanel.SetActive(!InventoryPanel.activeSelf);
        }
    }

    public void ToggleEquipment()
    {
        if (EquipmentPanel != null)
        {
            EquipmentPanel.SetActive(!EquipmentPanel.activeSelf);
        }
    }

    public void ToggleSkillTree()
    {
        if (SkillTreePanel != null)
        {
            SkillTreePanel.SetActive(!SkillTreePanel.activeSelf);
        }
    }

    public void ToggleQuest()
    {
        if (QuestPanel != null)
        {
            QuestPanel.SetActive(!QuestPanel.activeSelf);
        }
    }

    public void ToggleSettings()
    {
        if (SettingsPanel != null)
        {
            SettingsPanel.SetActive(!SettingsPanel.activeSelf);
        }
    }

    private void SetPanelVisibility(GameObject panel, bool visible)
    {
        if (panel != null)
        {
            panel.SetActive(visible);
            Debug.Log($"{panel.name} {(visible ? "activated" : "deactivated")}");
        }
        else
        {
            Debug.LogWarning($"Panel is null - cannot set visibility to {visible}");
        }
    }
}