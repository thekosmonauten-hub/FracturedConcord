using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple Legacy UI StatsPanel using traditional Unity UI elements
/// No memory leaks, no complex initialization, just basic show/hide functionality
/// </summary>
public class LegacyStatsPanel : MonoBehaviour
{
    [Header("Character Info")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterClassText;
    public TextMeshProUGUI characterLevelText;
    public Slider experienceSlider;
    public TextMeshProUGUI experienceText;

    [Header("Stats Sections")]
    public Transform attributesContainer;
    public Transform resourcesContainer;
    public Transform damageContainer;
    public Transform resistancesContainer;

    [Header("Prefabs")]
    public GameObject statRowPrefab;

    private void Start()
    {
        Debug.Log("[LegacyStatsPanel] Initialized - ready for use");
    }

    /// <summary>
    /// Update the panel with current character data
    /// </summary>
    public void UpdateWithCharacterData()
    {
        if (CharacterManager.Instance == null || !CharacterManager.Instance.HasCharacter())
        {
            Debug.LogWarning("[LegacyStatsPanel] No character data available");
            return;
        }

        Character character = CharacterManager.Instance.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogWarning("[LegacyStatsPanel] Character is null");
            return;
        }

        UpdateCharacterInfo(character);
        UpdateAttributes(character);
        UpdateResources(character);
        UpdateDamage(character);
        UpdateResistances(character);

        Debug.Log($"[LegacyStatsPanel] Updated with character: {character.characterName}");
    }

    private void UpdateCharacterInfo(Character character)
    {
        if (characterNameText != null)
            characterNameText.text = character.characterName;
        
        if (characterClassText != null)
            characterClassText.text = character.characterClass;
        
        if (characterLevelText != null)
            characterLevelText.text = $"Level {character.level}";
        
        if (experienceSlider != null)
        {
            float progress = character.experience / 100f; // Assuming max exp is 100
            experienceSlider.value = progress;
        }
        
        if (experienceText != null)
            experienceText.text = $"{character.experience}/100";
    }

    private void UpdateAttributes(Character character)
    {
        if (attributesContainer == null) return;

        // Clear existing content
        foreach (Transform child in attributesContainer)
        {
            Destroy(child.gameObject);
        }

        // Create attribute rows
        CreateStatRow(attributesContainer, "Strength", character.strength.ToString());
        CreateStatRow(attributesContainer, "Dexterity", character.dexterity.ToString());
        CreateStatRow(attributesContainer, "Intelligence", character.intelligence.ToString());
    }

    private void UpdateResources(Character character)
    {
        if (resourcesContainer == null) return;

        // Clear existing content
        foreach (Transform child in resourcesContainer)
        {
            Destroy(child.gameObject);
        }

        // Create resource rows
        CreateStatRow(resourcesContainer, "Health", $"{character.currentHealth}/{character.maxHealth}");
        CreateStatRow(resourcesContainer, "Mana", $"{character.maxMana}");
        CreateStatRow(resourcesContainer, "Energy Shield", "0/0");
        CreateStatRow(resourcesContainer, "Reliance", "0/0");
    }

    private void UpdateDamage(Character character)
    {
        if (damageContainer == null) return;

        // Clear existing content
        foreach (Transform child in damageContainer)
        {
            Destroy(child.gameObject);
        }

        // Create damage rows
        CreateStatRow(damageContainer, "Physical", "50 | 110% | 30%");
        CreateStatRow(damageContainer, "Fire", "50 | 110% | 30%");
        CreateStatRow(damageContainer, "Cold", "50 | 110% | 30%");
        CreateStatRow(damageContainer, "Lightning", "50 | 110% | 30%");
        CreateStatRow(damageContainer, "Chaos", "50 | 110% | 30%");
    }

    private void UpdateResistances(Character character)
    {
        if (resistancesContainer == null) return;

        // Clear existing content
        foreach (Transform child in resistancesContainer)
        {
            Destroy(child.gameObject);
        }

        // Create resistance rows
        CreateStatRow(resistancesContainer, "Fire", "0/75");
        CreateStatRow(resistancesContainer, "Cold", "0/75");
        CreateStatRow(resistancesContainer, "Lightning", "0/75");
        CreateStatRow(resistancesContainer, "Chaos", "0/75");
    }

    private void CreateStatRow(Transform parent, string label, string value)
    {
        if (statRowPrefab == null)
        {
            // Create simple text row if no prefab
            GameObject row = new GameObject($"StatRow_{label}");
            row.transform.SetParent(parent, false);
            
            TextMeshProUGUI text = row.AddComponent<TextMeshProUGUI>();
            text.text = $"{label}: {value}";
            text.fontSize = 14;
            
            RectTransform rect = row.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 20);
        }
        else
        {
            // Use prefab if available
            GameObject row = Instantiate(statRowPrefab, parent);
            TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = label;
                texts[1].text = value;
            }
        }
    }

    /// <summary>
    /// Simple refresh method - just update with current character data
    /// </summary>
    public void Refresh()
    {
        UpdateWithCharacterData();
    }

    private void OnEnable()
    {
        // Update data when panel becomes visible
        UpdateWithCharacterData();
    }
}









