using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/MagicAttacks/PlacedAttack")]
public class PlacedAttack : AttackBase
{
    [Tooltip("How long before the hitbox becomes active")]
    public float timeTillActive;
    public AudioClip activatedSound;
    public override GameObject WeaponEffect(GameObject caster)
    {
        var attack = base.WeaponEffect(caster);
        var casterManager = caster.GetComponent<AbilityManager>().stats;
        if (casterManager is playerData)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            

            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, targets);

            foreach (var hit in hits)
            {
                if (hit.transform.gameObject == caster) { continue; }
              
                attack.transform.position = hit.point;

                break;
            }

        }
        return attack;

    }
    public override GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking, float chargeDuration)

    {

        var attack = base.WeaponEffect(caster, time, whereiscaster, casterLooking, chargeDuration);

        var rigid = attack.GetComponent<Rigidbody>();

        var hitbox = attack.GetComponent<DelayedActive>().prewarmDuration = timeTillActive;

        RaycastHit[] hits = Physics.RaycastAll(attack.transform.position, attack.transform.forward, Mathf.Infinity, targets);
       
        foreach (var hit in hits)
        {
            if (hit.transform.gameObject == caster) { continue; }

           
            attack.GetComponent<NetworkTransform>().Teleport(hit.point, Quaternion.identity, attack.transform.localScale);
            break;
        }
        

        
        



        return attack;
    }

}
