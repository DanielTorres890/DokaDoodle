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
        base.WeaponEffect(caster);

    }
}
