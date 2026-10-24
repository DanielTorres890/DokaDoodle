using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/Special/Boast")]
public class BoastBuff : BuffBase
{
    public int fameAmount;

   //boast is applied onto the target, so stats is the person who was called out while applier is the person who called them out
    public override void OnRemove(EntityStats stats, EntityStats applier)
    {
        if(NewCombatManager.instance == null) 
        {
            var player = applier as playerData;
            player.playerInfo[PlayerInfo.fame] += fameAmount * (WorldEventManager.Instance.weeks + 1);

            var player2 = stats as playerData;
            player2.playerInfo[PlayerInfo.fame] -= fameAmount * (WorldEventManager.Instance.weeks + 1);
            return; 
        }

        if(!stats.isDead)
        {
            if(stats is playerData)
            {
               
                var player = stats as playerData;
                player.playerInfo[PlayerInfo.fame] += fameAmount * (WorldEventManager.Instance.weeks + 1);

                var player2 = applier as playerData;
                player2.playerInfo[PlayerInfo.fame] -= fameAmount * (WorldEventManager.Instance.weeks + 1);
            }

        }
        else
        {
            
            var player = stats as playerData;
            player.playerInfo[PlayerInfo.fame] -= fameAmount * (WorldEventManager.Instance.weeks + 1);

            var player2 = applier as playerData;
            player2.playerInfo[PlayerInfo.fame] += fameAmount * (WorldEventManager.Instance.weeks + 1);
        }
    }

    public override void OnCombatBegin(EntityStats stats, EntityStats whoApplied)
    {
        var combatants = PlayerCombatManager.Instance.combatants;
        if (!(combatants.Contains(whoApplied) && combatants.Contains(whoApplied))) { return; }
        NewCombatManager.instance.onCombatEnd.AddListener(delegate
        {
            if(stats.isDead || whoApplied.isDead) 
            {

                stats.RemoveStatus(NetworkData.Instance.buffDataBase.GetId[this]);
            }
        });
    }
    
    

}
