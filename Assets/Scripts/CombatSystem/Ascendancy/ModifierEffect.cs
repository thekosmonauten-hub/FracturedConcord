using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Effect that triggers on a specific event
/// </summary>
[System.Serializable]
public class ModifierEffect
{
    public ModifierEventType eventType;
    public List<ModifierCondition> conditions = new List<ModifierCondition>();
    public List<ModifierAction> actions = new List<ModifierAction>();
    public int priority = 0;
}

