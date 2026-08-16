using UnityEngine;

public abstract class ConditionalWEvent : WorldEventBase
{
    public override void OnActivate(int randomNum)
    {
        throw new System.NotImplementedException();
    }

    public override void OnDeactivate()
    {
        throw new System.NotImplementedException();
    }
    public override bool Condition(int turns)
    {
        throw new System.NotImplementedException();
    }
}
