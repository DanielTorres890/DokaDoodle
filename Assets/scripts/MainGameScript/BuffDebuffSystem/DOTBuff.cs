using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/DOT")]
public class DOTBuff : BuffBase
{
    [Header("Dot values should be between 0 - 1")]
    public float percentDamageOnTurn;
    public float percentDamagePerSecond;
 



    public override void OnEverySecond(AbilityManager whom, AbilityManager whomApplied)
    {
        if (NewCombatManager.instance == null) { return; }
        base.OnEverySecond(whom, whomApplied);
        
        if(NetworkManager.Singleton.IsServer)
        {
            int applierIndex = NewCombatManager.instance.allCombatants.IndexOf(whomApplied);
            whom.ImHitRpc(Mathf.Clamp(Mathf.RoundToInt(percentDamagePerSecond * whom.stats.stats[Attributes.MaxHealth]), 1, 9999), applierIndex);
        }
       
      

    }
}
