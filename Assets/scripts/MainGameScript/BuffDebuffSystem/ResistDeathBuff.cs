using UnityEngine;
[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/ResistDeath")]
public class ResistDeathBuff : BuffBase
{

    public int resistDeathCount;

    public override void OnApply(EntityStats stats)
    {
        stats.resistDeath += resistDeathCount;
        stats.PostStatusStatCalc();
        base.OnApply(stats);
        
    }

  
    // Update is called once per frame
    void Update()
    {
        
    }
}
