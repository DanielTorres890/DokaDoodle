using UnityEngine;

[System.Serializable]
public abstract class BuffBase : ScriptableObject
{
    public int duration;
    public bool combatOnly = false;
    public bool stackable = false;
    public GameObject buffFx;
    public virtual void OnApply(EntityStats stats)
    {

        ApplyBuffFx(stats);

    }

    public virtual void OnRemove(EntityStats stats)
    {

      
    }
    public virtual void ApplyBuffFx(EntityStats stats)
    {
        if (!buffFx) { return; }
        if (stats is playerData)
        {
            for (int i = 0; i < NetworkData.Instance.players.Count; i++)
            {
                var player = NetworkData.Instance.players[i];
                if (player != stats) { continue; }
                var fx = Instantiate(buffFx, NetworkData.Instance.playerSticks[i].transform);

                player.onStatusProgress.AddListener(delegate
                {


                    if (!buffFx) { return; }
                    if (stats is not playerData) { return; }



                    foreach (var status in player.statuses)
                    {
                        if (status.buffId == NetworkData.Instance.buffDataBase.GetId[this])
                        {
                            return;
                        }
                    }

                    Destroy(fx);

                });
                break;
            }

        }

        if (NewCombatManager.instance)
        {
            foreach (var combatant in NewCombatManager.instance.allCombatants)
            {
                if (combatant.stats == stats)
                {

                    if (!buffFx) { return; }

                    var fx = Instantiate(buffFx, combatant.transform);
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
    public abstract void BuffEffect(EntityStats whoWon);

    public virtual void OnEveryTick(AbilityManager stats) { }

    public virtual int GetDuration(EntityStats stats) { return duration; }
}
