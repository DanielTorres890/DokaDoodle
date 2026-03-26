using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/StatStatuses")]
public class StatStatusEffect : BuffBase
{
    public ItemBuff[] stats;
    public override void BuffEffect(EntityStats whoWon)
    {
        
    }

    public override void OnApply(EntityStats stats)
    {
        stats.PostStatusStatCalc();
        base.OnApply(stats);
    }

    public override void OnEveryTick(AbilityManager stats)
    {
        
    }
    public virtual ItemBuff[] GetStats(EntityStats holder)
    {
        return stats;
    }

    public override void OnRemove(EntityStats stats)
    {
        //its kinda weird but its implied(?) since itll be removed in some other way
        base.OnRemove(stats);
    }
}
