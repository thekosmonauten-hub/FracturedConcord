using UnityEngine;

[CreateAssetMenu(fileName = "StackAdjustment", menuName = "Dexiled/Stacks/Adjustment")]
public class StackAdjustmentDefinition : ScriptableObject
{
    [Header("Flat Stack Changes")]
    public int agitateStacks;
    public int toleranceStacks;
    public int potentialStacks;
    public int momentumStacks;
    public int flowStacks;
    public int corruptionStacks;
    public int battleRhythmStacks;

    [Header("Modifiers")]
    [Tooltip("Applied as a multiplicative 'more' bonus (e.g., 0.5 = 50% more). Used when attached to effects that scale existing gain/removal.")]
    public float agitateMoreMultiplier = 0f;
    public float toleranceMoreMultiplier = 0f;
    public float potentialMoreMultiplier = 0f;
    public float momentumMoreMultiplier = 0f;
    public float flowMoreMultiplier = 0f;
    public float corruptionMoreMultiplier = 0f;
    public float battleRhythmMoreMultiplier = 0f;

    [Tooltip("Applied as an additive 'increased' bonus in percentage (e.g., 50 = 50% increased).")]
    public float agitateIncreasedPercent = 0f;
    public float toleranceIncreasedPercent = 0f;
    public float potentialIncreasedPercent = 0f;
    public float momentumIncreasedPercent = 0f;
    public float flowIncreasedPercent = 0f;
    public float corruptionIncreasedPercent = 0f;
    public float battleRhythmIncreasedPercent = 0f;

    public StackAdjustmentDefinition Clone()
    {
        var clone = CreateInstance<StackAdjustmentDefinition>();
        clone.CopyFrom(this);
        return clone;
    }

    public void CopyFrom(StackAdjustmentDefinition other)
    {
        if (other == null) return;
        agitateStacks = other.agitateStacks;
        toleranceStacks = other.toleranceStacks;
        potentialStacks = other.potentialStacks;
        momentumStacks = other.momentumStacks;
        flowStacks = other.flowStacks;
        corruptionStacks = other.corruptionStacks;
        battleRhythmStacks = other.battleRhythmStacks;
        agitateMoreMultiplier = other.agitateMoreMultiplier;
        toleranceMoreMultiplier = other.toleranceMoreMultiplier;
        potentialMoreMultiplier = other.potentialMoreMultiplier;
        momentumMoreMultiplier = other.momentumMoreMultiplier;
        flowMoreMultiplier = other.flowMoreMultiplier;
        corruptionMoreMultiplier = other.corruptionMoreMultiplier;
        battleRhythmMoreMultiplier = other.battleRhythmMoreMultiplier;
        agitateIncreasedPercent = other.agitateIncreasedPercent;
        toleranceIncreasedPercent = other.toleranceIncreasedPercent;
        potentialIncreasedPercent = other.potentialIncreasedPercent;
        momentumIncreasedPercent = other.momentumIncreasedPercent;
        flowIncreasedPercent = other.flowIncreasedPercent;
        corruptionIncreasedPercent = other.corruptionIncreasedPercent;
        battleRhythmIncreasedPercent = other.battleRhythmIncreasedPercent;
    }

    public void MergeFrom(StackAdjustmentDefinition other)
    {
        if (other == null) return;
        agitateStacks += other.agitateStacks;
        toleranceStacks += other.toleranceStacks;
        potentialStacks += other.potentialStacks;
        momentumStacks += other.momentumStacks;
        flowStacks += other.flowStacks;
        corruptionStacks += other.corruptionStacks;
        battleRhythmStacks += other.battleRhythmStacks;
        agitateMoreMultiplier += other.agitateMoreMultiplier;
        toleranceMoreMultiplier += other.toleranceMoreMultiplier;
        potentialMoreMultiplier += other.potentialMoreMultiplier;
        momentumMoreMultiplier += other.momentumMoreMultiplier;
        flowMoreMultiplier += other.flowMoreMultiplier;
        corruptionMoreMultiplier += other.corruptionMoreMultiplier;
        battleRhythmMoreMultiplier += other.battleRhythmMoreMultiplier;
        agitateIncreasedPercent += other.agitateIncreasedPercent;
        toleranceIncreasedPercent += other.toleranceIncreasedPercent;
        potentialIncreasedPercent += other.potentialIncreasedPercent;
        momentumIncreasedPercent += other.momentumIncreasedPercent;
        flowIncreasedPercent += other.flowIncreasedPercent;
        corruptionIncreasedPercent += other.corruptionIncreasedPercent;
        battleRhythmIncreasedPercent += other.battleRhythmIncreasedPercent;
    }
}

