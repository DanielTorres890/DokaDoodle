using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[Serializable]
public class EntityStats 
{
    public string name;

    public Dictionary<Attributes, int> stats = new Dictionary<Attributes, int>
    {
        {Attributes.MaxHealth, 5 },
        {Attributes.Health, 5 },
        {Attributes.Attack, 5 },
        {Attributes.Defense, 5 },
        {Attributes.Magic, 5 },
        {Attributes.MDefense, 5 },
        {Attributes.Dexterity, 5}


    };
    [SerializeField] public AttackBase[] attacks = new AttackBase[4];
    [SerializeField] public DefenseBase[] defenses = new DefenseBase[4];


}
