using UnityEngine;

public class TimedWEvent : WorldEventBase
{
    [Tooltip("In days")]
    public int duration;

    [TextArea(2, 5)]
    public string eventToolTip;

    public Sprite eventIcon;
    public override void OnActivate(int randomNum)
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
