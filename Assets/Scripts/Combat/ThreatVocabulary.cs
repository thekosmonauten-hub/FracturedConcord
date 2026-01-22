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
    None,
    Charging,
    Primed,
    Anchoring,
    Leeching,
    Suppressing,
    Escalating,
    Volatile,
    Retaliating,
    Channeling,
    Converting,
    Shielded,
    Terminal
}

public static class ThreatVocabulary
{
    private static readonly Dictionary<ThreatWord, ThreatAxis[]> WordAxes = new Dictionary<ThreatWord, ThreatAxis[]>
    {
        { ThreatWord.Charging, new[] { ThreatAxis.TimePressure } },
        { ThreatWord.Primed, new[] { ThreatAxis.Volatility } },
        { ThreatWord.Anchoring, new[] { ThreatAxis.Protection } },
        { ThreatWord.Leeching, new[] { ThreatAxis.Punishment } },
        { ThreatWord.Suppressing, new[] { ThreatAxis.Disruption } },
        { ThreatWord.Escalating, new[] { ThreatAxis.TimePressure } },
        { ThreatWord.Volatile, new[] { ThreatAxis.Volatility } },
        { ThreatWord.Retaliating, new[] { ThreatAxis.Punishment } },
        { ThreatWord.Channeling, new[] { ThreatAxis.TimePressure, ThreatAxis.Disruption } },
        { ThreatWord.Converting, new[] { ThreatAxis.Disruption } },
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
