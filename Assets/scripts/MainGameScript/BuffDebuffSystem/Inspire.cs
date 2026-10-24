using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/Special/Inspire")]
public class Inspire : BuffBase
{
    public BuffBase[] buffs;

  

    public override void OnApply(EntityStats stats)
    {
        if(stats is playerData) 
        {
            playerData thisplayer = (playerData)stats;
            foreach(var ally in thisplayer.partyMembers)
            {
                if(ally.boardMovementState != PlayerFollowingStates.WithOwner) { continue; }

                foreach(var buff in buffs)
                {
                    
                    ally.GainStatus(buff, stats);
                }
                
            }
        }


        base.OnApply(stats);
    }

    public override void OnRemove(EntityStats stats, EntityStats applier)
    {
        if (stats is playerData)
        {
            playerData thisplayer = (playerData)stats;
            foreach (var ally in thisplayer.partyMembers)
            {

                foreach (var buff in buffs)
                {
                    ally.RemoveStatus(NetworkData.Instance.buffDataBase.GetId[buff]);
                }

            }
        }
        base.OnRemove(stats, applier);
    }
}
