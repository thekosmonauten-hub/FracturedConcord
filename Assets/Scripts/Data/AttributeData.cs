using System;

[Serializable]
public class AttributeData
{
    public string strength;
    public string dexterity;
    public string intelligence;

    public AttributeData(string str, string dex, string intel)
    {
        strength = str;
        dexterity = dex;
        intelligence = intel;
    }
}

[Serializable]
public class ResourceData
{
    public string health;
    public string mana;
    public string energyShield;
    public string reliance;

    public ResourceData(string hp, string mp, string es, string rel)
    {
        health = hp;
        mana = mp;
        energyShield = es;
        reliance = rel;
    }
}

[Serializable]
public class DamageData
{
    public string type;
    public string flat;
    public string increased;
    public string more;

    public DamageData(string damageType, string f, string inc, string m)
    {
        type = damageType;
        flat = f;
        increased = inc;
        more = m;
    }
}

[Serializable]
public class ResistanceData
{
    public string fire;
    public string cold;
    public string lightning;
    public string chaos;

    public ResistanceData(string f, string c, string l, string ch)
    {
        fire = f;
        cold = c;
        lightning = l;
        chaos = ch;
    }
}
