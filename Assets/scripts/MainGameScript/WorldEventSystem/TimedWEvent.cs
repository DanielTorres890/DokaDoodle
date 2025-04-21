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
        if (duration <= turns) { return false; } else { return true; }
    }

}
