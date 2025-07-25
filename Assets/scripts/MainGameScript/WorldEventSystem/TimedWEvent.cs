using UnityEngine;

public class TimedWEvent : WorldEventBase
{
    public int duration;

    public override void OnActivate()
    {
        throw new System.NotImplementedException();
    }

    public override void OnDeactivate()
    {
        throw new System.NotImplementedException();
    }
    public override bool Condition(int turns)
    {
        //returns true IF the event should end
        if (duration <= turns) { return true; } 
            
        return false; 
    }

}
