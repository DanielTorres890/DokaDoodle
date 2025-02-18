using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AttackBase : ScriptableObject
{
    [TextArea(15, 20)]
    public string description;
    public string attackName;

    public float startUp;
    public float endLag;
    public float cooldown;
    public combatantStates stateToBe;
    public bool chargeable;

    public GameObject attackPrefab;
    public Vector3 offset;

    public AttackMult[] multipliers = new AttackMult[7] {new AttackMult(Attributes.MaxHealth), new AttackMult(Attributes.Health) , new AttackMult(Attributes.Attack) , new AttackMult(Attributes.Defense) , new AttackMult(Attributes.Magic) , new AttackMult(Attributes.MDefense) , new AttackMult(Attributes.Dexterity)};
    public AttackMult[] defenseMult;
    public AttackMult[] antiGuardMultipliers = new AttackMult[7] { new AttackMult(Attributes.MaxHealth, 1), new AttackMult(Attributes.Health, 1), new AttackMult(Attributes.Attack, 1), new AttackMult(Attributes.Defense, 1), new AttackMult(Attributes.Magic, 1), new AttackMult(Attributes.MDefense, 1), new AttackMult(Attributes.Dexterity, 1) };
    //^ Saves me the annoyance of setting them everytime i create a scriptable
    public abstract void WeaponEffect(GameObject caster);
}

[System.Serializable]
public class AttackMult
{
    public Attributes attribute;
    public float mult = 0f;

    public AttackMult(Attributes attributes)
    {
        attribute = attributes;
    }

    public AttackMult(Attributes attributes, float multi)
    {
        attribute = attributes;
        mult = multi;
    }
}
