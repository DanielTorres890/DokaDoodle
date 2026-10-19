using UnityEngine;


[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/OnStruck/GainBuff")]
public class GainBuffOnStruck : InCombatOnStruck
{

    public BuffBase buffToGain;

    public override void OnStruck(AbilityManager buffOwnerManager)
    {
        buffOwnerManager.stats.GainStatus(buffToGain, buffOwnerManager.stats);
        buffOwnerManager.stats.PostStatusStatCalc();
        Debug.Log("I was hit and gained a buff ");
    }
}
