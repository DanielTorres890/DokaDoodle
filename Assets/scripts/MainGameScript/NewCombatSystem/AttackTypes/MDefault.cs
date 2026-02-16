using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/MagicAttacks/MagDefault")]
public class MDefault : AttackBase
{
    public float speed;
    
    public override GameObject WeaponEffect(GameObject caster)
    {
        var attack = base.WeaponEffect(caster);
        var casterManager = caster.GetComponent<AbilityManager>().stats;
        if (casterManager is playerData)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, targets);

            foreach (var hit in hits)
            {
                if (hit.transform.gameObject == caster) { continue; }

                attack.transform.LookAt(hit.point);
                break;
            }

        }
        return attack;

    }
    public override GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking, float chargeDuration)

    {
        
        var attack = base.WeaponEffect(caster, time, whereiscaster, casterLooking, chargeDuration);
        
        var rigid = attack.GetComponent<Rigidbody>();
        

        rigid.position += attack.transform.TransformDirection(rigid.linearVelocity) * (time - NetworkManager.Singleton.ServerTime.TimeAsFloat);

        
        rigid.linearVelocity = attack.transform.TransformDirection(Vector3.forward * speed);
        

        return attack;
    }


}
