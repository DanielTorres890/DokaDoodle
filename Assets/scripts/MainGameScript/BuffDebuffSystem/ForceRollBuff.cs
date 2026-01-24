using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/ForceRollBuff")]
public class ForceRollBuff : BuffBase
{
    public int forcedNumber;
    public override void BuffEffect(EntityStats whoWon)
    {

    }
}
