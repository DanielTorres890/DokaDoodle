using UnityEngine;

[System.Serializable]
public abstract class BuffBase : ScriptableObject
{
    public int duration;

    public abstract void OnApply(EntityStats stats);

    public abstract void OnRemove(EntityStats stats);
    public abstract void BuffEffect(EntityStats whoWon);
}
