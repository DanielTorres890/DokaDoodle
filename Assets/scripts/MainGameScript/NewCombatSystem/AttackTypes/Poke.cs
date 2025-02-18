using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "WeaponAttacks/Poke")]
public class Poke : AttackBase
{
    


    public override void WeaponEffect(GameObject caster)
    {
        Debug.Log("I SHOULD HAPPEN?");
        var attack = Instantiate(attackPrefab);
        attack.GetComponent<NetworkObject>().Spawn();
        attack.transform.rotation = caster.transform.rotation;
        var info = attack.GetComponent<AbilityBase>();
        info.owner = caster;
        info.ownerStats = caster.GetComponent<AbilityManager>().stats;
        info.attackInfo = this;
        attack.transform.position = caster.transform.position + caster.transform.TransformDirection(offset);



    }
}
