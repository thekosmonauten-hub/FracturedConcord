// 8/23/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterPanelHierarchyBuilder : MonoBehaviour
{
    void Start()
    {
        // Root Panel
        GameObject characterPanel = CreateUIElement("CharacterPanel", this.transform);
        RectTransform panelRect = characterPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(500, 1000);

        // Background
        CreateUIElementWithImage("Background", characterPanel.transform);

        // Header
        GameObject header = CreateUIElement("Header", characterPanel.transform);
        CreateText("Title", header.transform, "Character Stats");
        CreateButton("CloseButton", header.transform);

        // CharacterInfo
        GameObject characterInfo = CreateUIElement("CharacterInfo", characterPanel.transform);
        CreateUIElementWithImage("CharacterPortrait", characterInfo.transform);
        CreateText("CharacterName", characterInfo.transform, "Name");
        CreateText("CharacterClass", characterInfo.transform, "Class");
        CreateText("CharacterLevel", characterInfo.transform, "Level");

        // ExperienceBar
        GameObject experienceBar = CreateUIElement("ExperienceBar", characterPanel.transform);
        Slider slider = experienceBar.AddComponent<Slider>();
        CreateUIElementWithImage("Background", experienceBar.transform);
        GameObject fillArea = CreateUIElement("Fill Area", experienceBar.transform);
        CreateUIElementWithImage("Fill", fillArea.transform);
        CreateText("ExperienceText", experienceBar.transform, "XP");
        CreateText("NextLevelText", experienceBar.transform, "Next Level");

        // Attributes
        GameObject attributes = CreateUIElement("Attributes", characterPanel.transform);
        CreateText("StrengthText", attributes.transform, "Strength");
        CreateText("DexterityText", attributes.transform, "Dexterity");
        CreateText("IntelligenceText", attributes.transform, "Intelligence");

        // DerivedStats
        GameObject derivedStats = CreateUIElement("DerivedStats", characterPanel.transform);
        CreateText("MaxHealthText", derivedStats.transform, "Max Health");
        CreateText("MaxManaText", derivedStats.transform, "Max Mana");
        CreateText("MaxEnergyShieldText", derivedStats.transform, "Max Energy Shield");
        CreateText("AttackPowerText", derivedStats.transform, "Attack Power");
        CreateText("DefenseText", derivedStats.transform, "Defense");
        CreateText("CriticalChanceText", derivedStats.transform, "Critical Chance");
        CreateText("CriticalMultiplierText", derivedStats.transform, "Critical Multiplier");

        // CombatResources
        GameObject combatResources = CreateUIElement("CombatResources", characterPanel.transform);
        CreateText("ManaRecoveryText", combatResources.transform, "Mana Recovery");
        CreateText("CardsDrawnText", combatResources.transform, "Cards Drawn");
        CreateText("RelianceText", combatResources.transform, "Reliance");

        // DamageModifiers
        GameObject damageModifiers = CreateUIElement("DamageModifiers", characterPanel.transform);
        CreateText("PhysicalDamageText", damageModifiers.transform, "Physical Damage");
        CreateText("FireDamageText", damageModifiers.transform, "Fire Damage");
        CreateText("ColdDamageText", damageModifiers.transform, "Cold Damage");
        CreateText("LightningDamageText", damageModifiers.transform, "Lightning Damage");
        CreateText("ChaosDamageText", damageModifiers.transform, "Chaos Damage");

        // Resistances
        GameObject resistances = CreateUIElement("Resistances", characterPanel.transform);
        CreateText("PhysicalResistanceText", resistances.transform, "Physical Resistance");
        CreateText("FireResistanceText", resistances.transform, "Fire Resistance");
        CreateText("ColdResistanceText", resistances.transform, "Cold Resistance");
        CreateText("LightningResistanceText", resistances.transform, "Lightning Resistance");
        CreateText("ChaosResistanceText", resistances.transform, "Chaos Resistance");

        // EquipmentSummary
        GameObject equipmentSummary = CreateUIElement("EquipmentSummary", characterPanel.transform);
        CreateText("EquippedWeaponText", equipmentSummary.transform, "Equipped Weapon");
        CreateText("EquippedArmorText", equipmentSummary.transform, "Equipped Armor");
        CreateText("TotalEquipmentStatsText", equipmentSummary.transform, "Total Equipment Stats");
    }

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private GameObject CreateUIElementWithImage(string name, Transform parent)
    {
        GameObject obj = CreateUIElement(name, parent);
        obj.AddComponent<Image>();
        return obj;
    }

    private void CreateText(string name, Transform parent, string text)
    {
        GameObject obj = CreateUIElement(name, parent);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
    }

    private void CreateButton(string name, Transform parent)
    {
        GameObject obj = CreateUIElement(name, parent);
        obj.AddComponent<Button>();
    }
}
