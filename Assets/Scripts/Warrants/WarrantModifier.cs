using System;
using UnityEngine;

[Serializable]
public class WarrantModifier
{
    public string modifierId;
    public string displayName;
    public WarrantModifierOperation operation = WarrantModifierOperation.Additive;
    public float value = 1f;
    [TextArea] public string description;
}

