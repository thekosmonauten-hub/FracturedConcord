using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

/// <summary>
/// Handles populating the aura tooltip prefab with runtime data (aura info, level, effects, embossings, etc.).
/// </summary>
public class AuraTooltipView : MonoBehaviour
{
    [Header("Aura Elements (in display order)")]
    [SerializeField] private Image auraIconImage;
    [SerializeField] private TextMeshProUGUI relianceCostText;
    [SerializeField] private TextMeshProUGUI requirementsText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI nextLevelEffectText;
    
    [Header("Optional Elements")]
    [SerializeField] private TextMeshProUGUI auraNameText;
    [SerializeField] private Image backgroundImage;

    private void Awake()
    {
        CacheUIElements();
    }

    /// <summary>
    /// Populate the tooltip UI with the provided aura information.
    /// </summary>
    public void SetData(RelianceAura aura, Character character)
    {
        if (aura == null)
        {
            gameObject.SetActive(false);
            return;
        }

        EnsureUIReferencesCached();

        // Get aura level from experience manager
        int auraLevel = 1;
        if (AuraExperienceManager.Instance != null)
        {
            auraLevel = AuraExperienceManager.Instance.GetAuraLevel(aura.auraName);
        }

        // Populate in the requested order:
        PopulateIcon(aura);
        PopulateRelianceCost(aura);
        PopulateRequirements(aura, character);
        PopulateCurrentLevel(auraLevel);
        PopulateDescription(aura);
        PopulateEffect(aura, auraLevel);
        PopulateNextLevelEffect(aura, auraLevel);
        
        // Optional: Set name and theme color
        if (auraNameText != null)
        {
            auraNameText.text = aura.auraName;
            auraNameText.color = aura.themeColor;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = aura.themeColor;
        }
    }

    private void PopulateIcon(RelianceAura aura)
    {
        if (auraIconImage != null)
        {
            auraIconImage.sprite = aura.icon;
            auraIconImage.enabled = aura.icon != null;
        }
    }

    private void PopulateRelianceCost(RelianceAura aura)
    {
        if (relianceCostText != null)
        {
            relianceCostText.text = $"Reliance Cost: {aura.relianceCost}";
        }
    }

    private void PopulateRequirements(RelianceAura aura, Character character)
    {
        if (requirementsText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Check level requirement
            if (aura.requiredLevel > 1)
            {
                bool meetsLevel = character != null && character.level >= aura.requiredLevel;
                string color = meetsLevel ? "green" : "red";
                sb.Append($"<color={color}>Level {aura.requiredLevel}");
                if (!meetsLevel && character != null)
                {
                    int needed = aura.requiredLevel - character.level;
                    sb.Append($" (Need {needed} more)");
                }
                sb.Append("</color>\n");
            }
            
            // Check unlock requirement (quest/challenge)
            if (!string.IsNullOrEmpty(aura.unlockRequirement))
            {
                sb.Append($"<color=yellow>{aura.unlockRequirement}</color>\n");
            }
            
            if (sb.Length == 0)
            {
                sb.Append("<color=green>No requirements</color>");
            }
            
            requirementsText.text = sb.ToString().TrimEnd('\n');
        }
    }

    private void PopulateCurrentLevel(int level)
    {
        if (currentLevelText != null)
        {
            currentLevelText.text = $"Current Level: {level}/20";
        }
    }

    private void PopulateDescription(RelianceAura aura)
    {
        if (descriptionText != null)
        {
            descriptionText.text = aura.description;
        }
    }

    private void PopulateEffect(RelianceAura aura, int level)
    {
        if (effectText != null)
        {
            // Use interpolated effect description for the current level
            string effectDescription = GetInterpolatedEffect(aura, level);
            effectText.text = effectDescription;
        }
    }

    private void PopulateNextLevelEffect(RelianceAura aura, int currentLevel)
    {
        if (nextLevelEffectText != null)
        {
            if (currentLevel >= 20)
            {
                nextLevelEffectText.text = "Max Level - No further upgrades";
            }
            else
            {
                int nextLevel = currentLevel + 1;
                string nextEffect = GetInterpolatedEffect(aura, nextLevel);
                nextLevelEffectText.text = $"Next Level ({nextLevel}): {nextEffect}";
            }
        }
    }

    /// <summary>
    /// Get interpolated effect description for a specific level by extracting and scaling numeric values.
    /// </summary>
    private string GetInterpolatedEffect(RelianceAura aura, int level)
    {
        if (level <= 1)
            return aura.effectLevel1;
        if (level >= 20)
            return aura.effectLevel20;

        // Use level 1 as base template
        string baseText = aura.effectLevel1;
        string level20Text = aura.effectLevel20;

        // Extract all numeric values (including decimals) from both texts
        // Pattern matches: 15, 15.5, 25, etc.
        Regex numberRegex = new Regex(@"\b(\d+(?:\.\d+)?)\b");
        
        var level1Matches = numberRegex.Matches(baseText);
        var level20Matches = numberRegex.Matches(level20Text);

        // If we have matching number counts, interpolate each value
        if (level1Matches.Count == level20Matches.Count && level1Matches.Count > 0)
        {
            string result = baseText;
            
            // Calculate interpolation factor (0 at level 1, 1 at level 20)
            float t = (level - 1) / 19f;

            // Replace from end to start to preserve indices
            for (int i = level1Matches.Count - 1; i >= 0; i--)
            {
                if (float.TryParse(level1Matches[i].Value, out float value1) &&
                    float.TryParse(level20Matches[i].Value, out float value20))
                {
                    // Interpolate the value
                    float interpolatedValue = Mathf.Lerp(value1, value20, t);
                    
                    // Format based on whether it's a percentage or whole number
                    string formattedValue;
                    bool isPercentage = baseText.Contains("%") && level20Text.Contains("%");
                    
                    if (isPercentage)
                    {
                        // For percentages, show one decimal place if needed
                        formattedValue = (interpolatedValue % 1f < 0.01f) 
                            ? interpolatedValue.ToString("F0") 
                            : interpolatedValue.ToString("F1");
                    }
                    else
                    {
                        // For whole numbers, round to nearest integer
                        formattedValue = Mathf.RoundToInt(interpolatedValue).ToString();
                    }

                    // Replace the number at this match position
                    int matchIndex = level1Matches[i].Index;
                    int matchLength = level1Matches[i].Length;
                    result = result.Substring(0, matchIndex) + 
                            formattedValue + 
                            result.Substring(matchIndex + matchLength);
                }
            }

            return result;
        }

        // Fallback: if we can't match numbers, just show both
        return $"{baseText} → {level20Text} (Level {level})";
    }

    private void CacheUIElements()
    {
        // Icon (1st)
        if (auraIconImage == null)
        {
            var iconObj = transform.Find("Icon");
            if (iconObj == null) iconObj = transform.Find("AuraIcon");
            if (iconObj == null) iconObj = transform.Find("Header/Icon");
            auraIconImage = iconObj ? iconObj.GetComponent<Image>() : null;
        }

        // RelianceCost (2nd)
        if (relianceCostText == null)
        {
            var costObj = transform.Find("RelianceCost");
            if (costObj == null) costObj = transform.Find("RelianceCostText");
            if (costObj == null) costObj = transform.Find("Content/RelianceCost");
            relianceCostText = costObj ? costObj.GetComponent<TextMeshProUGUI>() : null;
        }

        // Requirements (3rd)
        if (requirementsText == null)
        {
            var reqObj = transform.Find("Requirements");
            if (reqObj == null) reqObj = transform.Find("RequirementsText");
            if (reqObj == null) reqObj = transform.Find("Content/Requirements");
            requirementsText = reqObj ? reqObj.GetComponent<TextMeshProUGUI>() : null;
        }

        // CurrentLevel (4th)
        if (currentLevelText == null)
        {
            var levelObj = transform.Find("CurrentLevel");
            if (levelObj == null) levelObj = transform.Find("CurrentLevelText");
            if (levelObj == null) levelObj = transform.Find("Level");
            if (levelObj == null) levelObj = transform.Find("Content/CurrentLevel");
            currentLevelText = levelObj ? levelObj.GetComponent<TextMeshProUGUI>() : null;
        }

        // Description (5th)
        if (descriptionText == null)
        {
            var descObj = transform.Find("Description");
            if (descObj == null) descObj = transform.Find("DescriptionText");
            if (descObj == null) descObj = transform.Find("Content/Description");
            descriptionText = descObj ? descObj.GetComponent<TextMeshProUGUI>() : null;
        }

        // Effect (6th)
        if (effectText == null)
        {
            var effectObj = transform.Find("Effect");
            if (effectObj == null) effectObj = transform.Find("EffectText");
            if (effectObj == null) effectObj = transform.Find("Content/Effect");
            effectText = effectObj ? effectObj.GetComponent<TextMeshProUGUI>() : null;
        }

        // NextLevelEffect (7th)
        if (nextLevelEffectText == null)
        {
            var nextEffectObj = transform.Find("NextLevelEffect");
            if (nextEffectObj == null) nextEffectObj = transform.Find("NextLevelEffectText");
            if (nextEffectObj == null) nextEffectObj = transform.Find("Content/NextLevelEffect");
            nextLevelEffectText = nextEffectObj ? nextEffectObj.GetComponent<TextMeshProUGUI>() : null;
        }

        // Optional elements
        if (auraNameText == null)
        {
            var nameObj = transform.Find("AuraName");
            if (nameObj == null) nameObj = transform.Find("Header/AuraName");
            auraNameText = nameObj ? nameObj.GetComponent<TextMeshProUGUI>() : null;
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }

    private void EnsureUIReferencesCached()
    {
        if (auraIconImage == null || relianceCostText == null || descriptionText == null)
        {
            CacheUIElements();
        }
    }
}

