using System.Collections.Generic;

public enum ThreatAxis
{
    TimePressure,
    Punishment,
    Disruption,
    Protection,
    Volatility
}

public enum ThreatWord
{
    None = 0,
    Charging = 1,
    Anchoring = 3,
    Leeching = 4,
    Suppressing = 5,
    Escalating = 6,
    Volatile = 7,
    Retaliating = 8,
    Channeling = 9,
    Shielded = 11,
    Terminal = 12
}

public static class ThreatVocabulary
{
    private static readonly Dictionary<ThreatWord, ThreatAxis[]> WordAxes = new Dictionary<ThreatWord, ThreatAxis[]>
    {
        { ThreatWord.Charging, new[] { ThreatAxis.TimePressure } },
        { ThreatWord.Anchoring, new[] { ThreatAxis.Protection } },
        { ThreatWord.Leeching, new[] { ThreatAxis.Punishment } },
        { ThreatWord.Suppressing, new[] { ThreatAxis.Disruption } },
        { ThreatWord.Escalating, new[] { ThreatAxis.TimePressure } },
        { ThreatWord.Volatile, new[] { ThreatAxis.Volatility } },
        { ThreatWord.Retaliating, new[] { ThreatAxis.Punishment } },
        { ThreatWord.Channeling, new[] { ThreatAxis.TimePressure, ThreatAxis.Disruption } },
        { ThreatWord.Shielded, new[] { ThreatAxis.Protection } },
        { ThreatWord.Terminal, new[] { ThreatAxis.TimePressure, ThreatAxis.Volatility } }
    };

    public static IReadOnlyList<ThreatAxis> GetAxes(ThreatWord word)
    {
        if (WordAxes.TryGetValue(word, out var axes))
            return axes;
        return new ThreatAxis[0];
    }
}
