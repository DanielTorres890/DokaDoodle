using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/StunStatus")]
public class StunDebuff : BuffBase
{
    //honestly its kinda weird bc the way buffs/debuffs are set up theres no good way to track if someone should be stunned
    //other than by name
    public override void BuffEffect(EntityStats whoWon)
    {
       
    }

    public override void OnApply(EntityStats stats)
    {
        //means we're in combat
        base.OnApply(stats);
        if(NewCombatManager.instance)
        {
            foreach (var combatant in NewCombatManager.instance.allCombatants)
            {
                if (combatant.stats == stats)
                {
                    combatant.combatantstate = combatantStates.Endlag;
                    combatant.stateDuration = 999;
                    if (!buffFx) { return; }
                    
                    var fx = Instantiate(buffFx,combatant.transform);
                    combatant.onStatus.AddListener(delegate
                    {
                        foreach(var status in stats.statuses)
                        {
                            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId] == this)
                            {
                                return;
                            }
                        }


                        Destroy(fx);
                    });

                }
            }
        }
    }

    public override void OnRemove(EntityStats stats)
    {
        base.OnRemove(stats);
        if (NewCombatManager.instance)
        {
            foreach (var combatant in NewCombatManager.instance.allCombatants)
            {
                if (combatant.stats == stats)
                {
                    combatant.combatantstate = combatantStates.Free;
                    combatant.stateDuration = 0;
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
