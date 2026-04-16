using UnityEngine;

public class BuffHolder 
{

    public float timeRemaining;
    public int buffId;
    public BuffHolder(float time, int buffId)
    {
        this.timeRemaining = time;
        this.buffId = buffId;   
    }
    public bool ProgressStatus(float time = 1)
    {
        timeRemaining -= time;
        if (timeRemaining <= 0) { return true; }
        return false;
    }
}
