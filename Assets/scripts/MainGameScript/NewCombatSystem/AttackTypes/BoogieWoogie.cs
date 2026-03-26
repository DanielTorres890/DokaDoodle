using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/Special/BoogieWoogie")]
public class BoogieWoogie : AttackBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Tooltip("How kind the raycast will be when firing(aka it doesnt have to be a perfect hit)")]
    public float buffer;

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

        
        
        RaycastHit[] hits = Physics.SphereCastAll(caster.transform.position, buffer, caster.transform.forward, Mathf.Infinity, targets);
        foreach(var hit in hits)
        {
            if (hit.transform == caster.transform) { continue; }
         


            Vector3 oldPos = hit.collider.transform.position;


       
            //hit.collider.transform.GetComponent<Rigidbody>().position = caster.transform.position + Vector3.up;
            
            //it.collider.transform.position = caster.transform.position + Vector3.up;
            hit.transform.GetComponent<AbilityManager>().TeleportMeRpc(caster.transform.position + Vector3.up, hit.transform.eulerAngles);
            
            Rigidbody rb = caster.transform.GetComponent<Rigidbody>();

            
           
            //casterRigid.position = oldPos + Vector3.up;
            caster.transform.GetComponent<AbilityManager>().TeleportMeRpc(oldPos + Vector3.up, caster.transform.eulerAngles += Vector3.up * 180f);
            Physics.SyncTransforms();
            break;
        }
        


        return attack;
    }
}
