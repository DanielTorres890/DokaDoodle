using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackBase : ScriptableObject
{
    [TextArea(15, 20)]
    public string description;
    public string attackName;
    public AttackMult[] multipliers = new AttackMult[7];

    public abstract void WeaponEffect();
}

[System.Serializable]
public class AttackMult
{
    public Attributes attribute;
    public float mult;
}
