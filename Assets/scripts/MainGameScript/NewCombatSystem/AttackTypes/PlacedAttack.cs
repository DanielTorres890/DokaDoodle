using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/MagicAttacks/PlacedAttack")]
public class PlacedAttack : AttackBase
{
    [Tooltip("How long before the hitbox becomes active")]
    public float timeTillActive;
    public AudioClip activatedSound;
    
    //i really didnt want to do this but it seems really bad if you dont sad
    public Vector3 temporaryPosition;
    public override GameObject WeaponEffect(GameObject caster)
    {
        var attack = base.WeaponEffect(caster);
        var casterManager = caster.GetComponent<AbilityManager>().stats;
        if (casterManager is playerData)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            

            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, targets);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 9999999);
            if(hits.Length > 0)
            {
                Vector3 closestHit = hits[0].point;
                foreach (var hit in hits)
                {
                    if (hit.transform.gameObject == caster) { continue; }

                    if(Vector3.Distance(hit.point,caster.transform.position) < Vector3.Distance(closestHit, caster.transform.position))
                    {
                        closestHit = hit.point;
                        
                    }
                }
                attack.transform.position = closestHit;
            }
            

        }
        return attack;

    }
    public override GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking, float chargeDuration, Vector3 origin, Vector3 direction)

    {

        var attack = base.WeaponEffect(caster, time, whereiscaster, casterLooking, chargeDuration, origin, direction);

        var rigid = attack.GetComponent<Rigidbody>();

        var hitbox = attack.GetComponent<DelayedActive>().prewarmDuration = timeTillActive;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, Mathf.Infinity, targets);

        Vector3 closestHit = whereiscaster;
        if (hits.Length > 0)
        {
            closestHit = hits[0].point;
            foreach (var hit in hits)
            {
                if (hit.transform.gameObject == caster) { continue; }

                if (Vector3.Distance(hit.point, caster.transform.position) < Vector3.Distance(closestHit, caster.transform.position))
                {
                    closestHit = hit.point;
                }
            }
        }
        attack.GetComponent<NetworkTransform>().Teleport(closestHit, Quaternion.identity, attack.transform.localScale);
        
        

        
        



        return attack;
    }

}
