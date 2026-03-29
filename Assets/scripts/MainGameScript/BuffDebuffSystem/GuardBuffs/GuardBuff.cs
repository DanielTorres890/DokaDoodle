using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/GuardStatus")]
public class GuardBuff : StatStatusEffect
{
    public float parryWindow;

    public AudioClip parryNoise;
    public AudioClip guardNoise;

    public override void OnEveryTick(AbilityManager stats)
    {
        stats.stats.PostStatusStatCalc();
        if(stats.CanAct() && stats.IsOwner)
        {
            stats.ILostBuffRpc(NetworkData.Instance.buffDataBase.GetId[this]);
            
        }
    }

    public override ItemBuff[] GetStats(EntityStats holder)
    {
        if (!withinParryWindow(holder)) { return stats; }
        
        
        ItemBuff[] parryBuffs = new ItemBuff[stats.Length];
        for (int i = 0; i < stats.Length; i++)
        {
           
            parryBuffs[i] = new ItemBuff(Mathf.RoundToInt(Mathf.Clamp(stats[i].value * 1.5f,0,100)));
            parryBuffs[i].attribute = stats[i].attribute;

        }

        return parryBuffs;
    }

    public bool withinParryWindow(EntityStats holder)
    {
        BuffHolder boof = holder.statuses[0];
        foreach (var status in holder.statuses)
        {
            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId] == this) { boof = status; break; }

        }
        float timing = duration - boof.timeRemaining;
        return timing <= parryWindow;
    }
}
