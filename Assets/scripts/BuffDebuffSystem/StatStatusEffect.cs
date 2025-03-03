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
        Debug.Log("Is there something in here " + NetworkData.Instance.buffDataBase.GetId.Count);
        stats.GainStatus(this);
        stats.PostStatusStatCalc();
    }

    public override void OnRemove(EntityStats stats)
    {

    }
}
