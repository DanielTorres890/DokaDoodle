using UnityEngine;

[System.Serializable]
public abstract class BuffBase : ScriptableObject
{
    public int duration;
    public abstract void BuffEffect(EntityStats whoWon);
}
