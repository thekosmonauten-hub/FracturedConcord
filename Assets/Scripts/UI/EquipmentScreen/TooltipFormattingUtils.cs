using System.Collections.Generic;

public static class TooltipFormattingUtils
{
    public static string ColorizeByRarity(string text, ItemRarity rarity)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        string hex = ItemRarityCalculator.GetRarityColor(rarity);
        return $"<color={hex}>{text}</color>";
    }

    public static string FormatAffix(Affix affix)
    {
        if (affix == null)
            return string.Empty;

        if (!string.IsNullOrEmpty(affix.description))
        {
            if (!string.IsNullOrEmpty(affix.name))
                return $"{affix.name}: {affix.description}";

            return affix.description;
        }

        return affix.name ?? string.Empty;
    }

    public static string FormatRequirements(int requiredLevel, int strength, int dexterity, int intelligence)
    {
        var lines = new List<string>();

        if (requiredLevel > 1)
            lines.Add($"Level {requiredLevel}");

        if (strength > 0)
            lines.Add($"{strength} Strength");

        if (dexterity > 0)
            lines.Add($"{dexterity} Dexterity");

        if (intelligence > 0)
            lines.Add($"{intelligence} Intelligence");

        if (lines.Count == 0)
            return "None";

        return string.Join("\n", lines);
    }
}


