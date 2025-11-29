using System.Collections.Generic;

public static class EffigyStatHelper
{
    public static Dictionary<string, float> CalculateStats(IEnumerable<Effigy> effigies)
    {
        var totals = new Dictionary<string, float>();
        if (effigies == null)
            return totals;

        HashSet<Effigy> processed = new HashSet<Effigy>();
        foreach (var effigy in effigies)
        {
            if (effigy == null || !processed.Add(effigy))
                continue;

            AccumulateAffixes(totals, effigy.implicitModifiers);
            AccumulateAffixes(totals, effigy.prefixes);
            AccumulateAffixes(totals, effigy.suffixes);
        }

        return totals;
    }

    private static void AccumulateAffixes(Dictionary<string, float> totals, List<Affix> affixes)
    {
        if (affixes == null)
            return;

        foreach (var affix in affixes)
        {
            if (affix?.modifiers == null)
                continue;

            foreach (var modifier in affix.modifiers)
            {
                if (modifier == null)
                    continue;

                float value = modifier.isDualRange ? modifier.rolledFirstValue : modifier.minValue;
                if (totals.TryGetValue(modifier.statName, out float existing))
                {
                    totals[modifier.statName] = existing + value;
                }
                else
                {
                    totals[modifier.statName] = value;
                }
            }
        }
    }
}


