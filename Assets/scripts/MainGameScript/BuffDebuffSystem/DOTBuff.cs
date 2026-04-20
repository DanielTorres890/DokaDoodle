using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/DOT")]
public class DOTBuff : BuffBase
{
    [Header("Dot values should be between 0 - 1")]
    public float percentDamageOnTurn;
    public float percentDamagePerSecond;
    public override void BuffEffect(EntityStats whoWon)
    {
        
    }



    public override void OnEverySecond(AbilityManager whom)
    {
        if (NewCombatManager.instance == null) { return; }
        base.OnEverySecond(whom);
        
        if(NetworkManager.Singleton.IsServer)
        whom.ImHitRpc(Mathf.Clamp(Mathf.RoundToInt(percentDamagePerSecond * whom.stats.stats[Attributes.MaxHealth]), 1 , 9999));
      

    }
}
