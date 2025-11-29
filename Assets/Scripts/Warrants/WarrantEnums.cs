using System;

public enum WarrantRarity
{
    Common,
    Magic,
    Rare,
    Unique
}

public enum WarrantModifierOperation
{
    Additive,
    Multiplicative,
    More,
    Override
}

public enum WarrantNodeType
{
    Anchor,
    Socket,
    Effect,
    SpecialSocket,
    Keystone
}

public enum WarrantRowType
{
    Socket,
    Effect
}

[Flags]
public enum WarrantRangeDirection
{
    None = 0,
    Forward = 1 << 0,
    Backward = 1 << 1,
    All = Forward | Backward
}














