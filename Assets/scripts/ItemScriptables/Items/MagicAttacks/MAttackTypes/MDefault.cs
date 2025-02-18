using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "MagicAttacks/MagDefault")]
public class MDefault : AttackBase
{
    public override void WeaponEffect(GameObject caster)
    {
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
