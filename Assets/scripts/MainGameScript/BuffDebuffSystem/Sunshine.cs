using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/Special/Sunshine")]
public class Sunshine : StatStatusEffect
{
    public BuffBase releaseInfo;

    public override ItemBuff[] GetStats(EntityStats holder)
    {
        ItemBuff[] stats2 = new ItemBuff[stats.Length];

        bool hasRelease = false;
        foreach(var status in holder.statuses)
        {
            
            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId] == releaseInfo) { hasRelease = true; break; }
        }

        if(hasRelease)
        {
            return stats;
        }


        for(int i = 0; i < stats.Length; i++)
        {
            stats2[i] = new ItemBuff(0);

            if(WorldEventManager.Instance.daysPerWeek - 1 != 0) 
            {
                
                stats2[i].value = Mathf.RoundToInt(stats[i].value * (-Mathf.Cos((float)WorldEventManager.Instance.days / (WorldEventManager.Instance.daysPerWeek - 1) * 2 * Mathf.PI) * .75f + .25f));
                
            }
            stats2[i].attribute = stats[i].attribute;


        }
        return stats2;
    }
}
