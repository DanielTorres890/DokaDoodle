using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/Special/SunshineRelease")]
public class Release :  BuffBase
{

    public BuffBase sunshine;
    public override void BuffEffect(EntityStats whoWon)
    {
        
    }

    public override void OnApply(EntityStats stats)
    {
        stats.PostStatusStatCalc();
        base.OnApply(stats);
    }

    public override int GetDuration(EntityStats stats)
    {
        bool hasSunshine = false;

        //this should not be possible without having at least 1 status effect
        BuffHolder boof = stats.statuses[0];
        foreach(var status in stats.statuses)
        {
            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId] == sunshine) { boof = status; hasSunshine = true; break; }

        }
        if(!hasSunshine) { return 0; }

        int difference = (sunshine.duration - Mathf.RoundToInt(boof.timeRemaining)) * 5;

        Debug.Log("What is this? " + boof.timeRemaining);
        Debug.Log("For how long? " + (sunshine.duration - Mathf.RoundToInt(boof.timeRemaining)));

        boof.timeRemaining = sunshine.duration;
        return difference;


    }
}
