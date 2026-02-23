using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/IndirectEffect")]
public class StackingVisual : BuffBase
{

    public override void BuffEffect(EntityStats whoWon)
    {
        
    }

    public override void OnApply(EntityStats stats)
    {

        if (!buffFx) { return; }
        if (stats is not playerData) { return; }

        float offset = 1.5f;
        
        if (NewCombatManager.instance)
        {
            foreach (var combatant in NewCombatManager.instance.allCombatants)
            {
                if (combatant.stats == stats)
                {
                    if (!buffFx) { return; }

                    var fx = Instantiate(buffFx, combatant.transform);
                    int existingStatuses = -2;
                    for (int j = 0; j < stats.statuses.Count; j++)
                    {
                        if (stats.statuses[j].buffId == NetworkData.Instance.buffDataBase.GetId[this])
                        {
                            existingStatuses++;
                        }
                    }
                    
                    fx.transform.localEulerAngles += new Vector3(0, 0, -45 * existingStatuses);
                    fx.transform.localPosition += Vector3.down * offset;
                    combatant.onStatus.AddListener(delegate
                    {
                        foreach (var status in stats.statuses)
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
}
