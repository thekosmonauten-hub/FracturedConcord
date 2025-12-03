using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RandomBuffEffect", menuName = "Dexiled/Enemies/Effects/Random Buff")]
public class RandomBuffEffect : AbilityEffect
{
    [Header("Random Buff Selection")]
    [Tooltip("List of possible status effects to choose from randomly")]
    public List<StatusEffectEffect> possibleBuffs = new List<StatusEffectEffect>();
    
    [Tooltip("If true, applies all buffs. If false, picks one random buff.")]
    public bool applyAllBuffs = false;
    
    public override void Execute(AbilityContext ctx)
    {
        if (ctx == null) return;
        
        if (possibleBuffs == null || possibleBuffs.Count == 0)
        {
            Debug.LogWarning("[RandomBuffEffect] No possible buffs defined!");
            return;
        }
        
        if (applyAllBuffs)
        {
            // Apply all buffs
            foreach (var buff in possibleBuffs)
            {
                if (buff != null)
                {
                    buff.Execute(ctx);
                }
            }
            Debug.Log($"[RandomBuffEffect] Applied all {possibleBuffs.Count} buffs");
        }
        else
        {
            // Pick one random buff
            int randomIndex = Random.Range(0, possibleBuffs.Count);
            StatusEffectEffect chosenBuff = possibleBuffs[randomIndex];
            
            if (chosenBuff != null)
            {
                chosenBuff.Execute(ctx);
                Debug.Log($"[RandomBuffEffect] Randomly selected and applied buff #{randomIndex}");
            }
        }
    }
}

