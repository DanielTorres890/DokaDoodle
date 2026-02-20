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
        
        if(!buffFx) { return; }
        if(stats is not playerData) { return; }
        for(int i = 0; i < NetworkData.Instance.players.Count;i++)
        {
            var player = NetworkData.Instance.players[i];
            if(player != stats) { continue; }
            var fx = Instantiate(buffFx, NetworkData.Instance.playerSticks[i].transform);
            Debug.Log("I assigned ");
            player.onStatusProgress.AddListener(delegate
            {
                Debug.Log("I checked ");

                if (!buffFx) { return; }
                if (stats is not playerData) { return; }

                Debug.Log("I'm passed the boilers");

                foreach(var status in player.statuses)
                {
                    if(status.buffId == NetworkData.Instance.buffDataBase.GetId[this])
                    {
                        return;
                    }
                }
                Debug.Log("I made it past this? ");
                Destroy(fx);
                
            });
            return;
        }
        
    }

    public virtual void OnRemove(EntityStats stats)
    {
        

    }
    public abstract void BuffEffect(EntityStats whoWon);

    public virtual void OnEveryTick(AbilityManager stats) { }
}
