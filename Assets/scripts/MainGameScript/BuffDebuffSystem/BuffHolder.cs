using UnityEngine;

public class BuffHolder 
{

    public EntityStats whoAppliedThis;
    public float timeRemaining;
    public int buffId;
    public BuffHolder(float time, int buffId, EntityStats whoapplied)
    {
        this.timeRemaining = time;
        this.buffId = buffId;   
        this.whoAppliedThis = whoapplied;
    }
    public bool ProgressStatus(float time = 1)
    {
        timeRemaining -= time;
        if (timeRemaining <= 0) { return true; }
        return false;
    }
}
