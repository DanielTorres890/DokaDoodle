using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "WeaponAttacks/Poke")]
public class Poke : AttackBase
{
    


    public override void WeaponEffect(GameObject caster)
    {
        Debug.Log("I SHOULD HAPPEN?");
        var attack = Instantiate(attackPrefab);
        attack.transform.rotation = caster.transform.rotation;
        var info = attack.GetComponent<AbilityBase>();
        info.owner = caster;
        info.attackInfo = this;
        attack.transform.position = caster.transform.position + offset;



    }
}
