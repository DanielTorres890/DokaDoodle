using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "MagicAttacks/MagDefault")]
public class MDefault : AttackBase
{
    public override GameObject WeaponEffect(GameObject caster)
    {
        return base.WeaponEffect(caster);

    }
    public override GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking)

    {
        Debug.Log("I tried shootin magic");
        var attack = base.WeaponEffect(caster, time, whereiscaster, casterLooking);
        
        var rigid = attack.GetComponent<Rigidbody>();

        rigid.position += attack.transform.TransformDirection(rigid.linearVelocity) * (time - NetworkManager.Singleton.ServerTime.TimeAsFloat);
        
        return attack;
    }


}
