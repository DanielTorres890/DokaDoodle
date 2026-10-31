using UnityEngine;

[System.Serializable]
public abstract class BuffBase : ScriptableObject
{
    public float duration;

    public bool progressInCombat = true;
    public bool combatOnly = false;
    public bool stackable = false;
    public bool removeOnDeath = false;
    public GameObject buffFx;

    public Sprite displayIcon;

    [TextArea(3, 5)]
    public string buffDescription;
    public virtual void OnApply(EntityStats stats)
    {

        ApplyBuffFx(stats);

    }

    public virtual void OnRemove(EntityStats stats, EntityStats applier)
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
        if(PlayerMoveManager.Instance != null)
        {
            if (stats is EnemyCombat)
            {
                var enemyCombat = (EnemyCombat)stats;
                foreach(var spawned in PlayerMoveManager.Instance.spawnedEnemies)
                {

                    if(spawned.Key == enemyCombat)
                    {
                        
                        var fx = Instantiate(buffFx, spawned.Value.transform);

                        enemyCombat.onStatusProgress.AddListener(delegate
                        {


                            if (!buffFx) { return; }



                            foreach (var status in enemyCombat.statuses)
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
  


    public virtual void OnEveryTick(AbilityManager stats) { }
    public virtual void OnEverySecond(AbilityManager whom, AbilityManager whomApplied) { }

    public virtual float GetDuration(EntityStats stats) { return duration; }

    public virtual string GetDescription(EntityStats stats, EntityStats applier) 
    {
        string description = new(buffDescription);

        if(applier != null)
            description = description.Replace("<applierName>", applier.name);
        if(stats != null)
            description = description.Replace("<buffOwner>", stats.name);

        return description; 
    }
    public virtual void OnCombatBegin(EntityStats stats, EntityStats applier) { }
}
