using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/Special/Sunshine")]
public class Sunshine : StatStatusEffect
{
    public override ItemBuff[] GetStats()
    {
        ItemBuff[] stats2 = new ItemBuff[stats.Length];
        for(int i = 0; i < stats.Length; i++)
        {
            if(WorldEventManager.Instance.daysPerWeek - 1 != 0)
            stats2[i].value = Mathf.RoundToInt(stats[i].value * -Mathf.Cos((float)WorldEventManager.Instance.days  / (WorldEventManager.Instance.daysPerWeek - 1) * 2 * Mathf.PI));
            stats2[i].attribute = stats[i].attribute;


        }
        return stats2;
    }
}
