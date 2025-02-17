using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[Serializable]
public class EntityStats 
{
    public string name;

    public bool isDead = false;
    public List<string> loyaltyTags = new List<string>();

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
    [SerializeField] public List<AttackBase> attacks = new List<AttackBase>();
    [SerializeField] public DefenseBase[] defenses = new DefenseBase[4];


}
