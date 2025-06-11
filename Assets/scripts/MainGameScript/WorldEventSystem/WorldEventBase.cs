using UnityEngine;

public abstract class WorldEventBase : ScriptableObject
{
    [TextArea(5,15)]
    public string ActivateText;
    [TextArea(5, 15)]
    public string DeactivateText;

    public Sprite eventDisplay;
    public abstract void OnActivate();

    public abstract void OnDeactivate();

    public abstract bool Condition(int turns);
}
