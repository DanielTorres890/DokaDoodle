using UnityEngine;

public class BuffHolder 
{

    public int timeRemaining;
    public int buffId;
    public BuffHolder(int time, int buffId)
    {
        this.timeRemaining = time;
        this.buffId = buffId;   
    }
    public bool ProgressStatus(int time = 1)
    {
        timeRemaining -= time;
        if (timeRemaining <= 0) { return true; }
        return false;
    }
}
