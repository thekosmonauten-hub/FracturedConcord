using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectEffect", menuName = "Dexiled/Enemies/Effects/Status")]
public class StatusEffectEffect : AbilityEffect
{
    public StatusEffectType statusType = StatusEffectType.Vulnerable;
    public float magnitude = 1f;
    public int durationTurns = 2;

    public override void Execute(AbilityContext ctx)
    {
        var playerDisplay = Object.FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay == null) return;
        var statusMgr = playerDisplay.GetStatusEffectManager();
        if (statusMgr == null) return;

        statusMgr.AddStatusEffect(new StatusEffect(statusType, statusType.ToString(), magnitude, durationTurns, false));
    }
}



