using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class EntityStats 
{
    public string name;

    public Dictionary<Attributes, int> stats = new Dictionary<Attributes, int>
    {
        {Attributes.MaxHealth, 0 },
        {Attributes.Health, 0 },
        {Attributes.Attack, 0 },
        {Attributes.Defense, 0 },
        {Attributes.Magic, 0 },
        {Attributes.MDefense, 0 },
        {Attributes.Dexterity, 0 }


    };


}
