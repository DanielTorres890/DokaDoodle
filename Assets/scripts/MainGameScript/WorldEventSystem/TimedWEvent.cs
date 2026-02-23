using UnityEngine;

public class TimedWEvent : WorldEventBase
{
    [Tooltip("In days")]
    public int duration;

    public override void OnActivate()
    {
        
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
    }
    public override bool Condition(int turns)
    {
        //returns true IF the event should end
        if (duration <= turns) { return true; } 
            
        return false; 
    }

}
