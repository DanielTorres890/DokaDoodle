using Unity.Netcode;
using UnityEngine;

public abstract class InCombatOnStruck : BuffBase
{
    //this buff does nothing if not applied in combat !
    public override void OnApply(EntityStats stats)
    {
        if (NewCombatManager.instance == null) { return; }
        base.OnApply(stats);
        
        foreach(var combatant in NewCombatManager.instance.allCombatants)
        {
            if(combatant.stats == stats)
            {
                combatant.onHit.AddListener(delegate { OnStruck(combatant); });
            }
        }


    }
    public abstract void OnStruck(AbilityManager buffOwnerManager);
}
