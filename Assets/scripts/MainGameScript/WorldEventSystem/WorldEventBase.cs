using UnityEngine;

public abstract class WorldEventBase : ScriptableObject
{

    public abstract void OnActivate();

    public abstract void OnDeactivate();

    public abstract bool Condition(int turns);
}
