using UnityEngine;

public class BuffHolder 
{

    public int whoAppliedId;
    public float timeRemaining;
    public int buffId;
    public BuffHolder(float time, int buffId, EntityStats whoapplied)
    {
        this.timeRemaining = time;
        this.buffId = buffId;

        this.whoAppliedId = -1;
        if (whoapplied is playerData)
        {
            this.whoAppliedId = NetworkData.Instance.players[NetworkData.Instance.players.IndexOf(whoapplied as playerData)].playerNumber;
        }
        
    }
    public bool ProgressStatus(float time = 1)
    {
        timeRemaining -= time;
        if (timeRemaining <= 0) { return true; }
        return false;
    }
}
