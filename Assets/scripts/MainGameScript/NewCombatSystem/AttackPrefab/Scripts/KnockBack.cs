using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor.PackageManager;
using UnityEngine;

public class KnockBack : RangedAbility
{
    [Tooltip("Relative to the ability itself")]
    public Vector3 knockbackDirection;

    public override void OnTriggerEnter(Collider other)
    {
        if (!IsServer || other.gameObject == owner) { return; }
        base.OnTriggerEnter(other);

        bool isEntity = false;
        if (other.gameObject.TryGetComponent(out AbilityManager hitby))
        {

            isEntity = true;
            if (NewCombatManager.instance && NewCombatManager.instance.fightOver) { return; }

            if (hitby.stats.loyaltyTags.Intersect(ownerStats.loyaltyTags).Any())
            {
                return;
            }
            if (isEntity)
            {
                transform.LookAt(other.transform);
                Vector3 relativeKnockback = transform.TransformDirection(knockbackDirection);
                GetKnockedBackRpc(relativeKnockback ,hitby, RpcTarget.Single(other.transform.GetComponent<NetworkObject>().OwnerClientId, RpcTargetUse.Temp));
            }
        }
        

    }
    [Rpc(SendTo.SpecifiedInParams)]

    public void GetKnockedBackRpc(Vector3 knockBack,NetworkBehaviourReference reference, RpcParams rpcStuff)
    {
        if(reference.TryGet(out AbilityManager entity))
        {
            var rigid = entity.GetComponent<Rigidbody>();
            rigid.isKinematic = false;
            rigid.AddForce(knockBack, ForceMode.Impulse);
            
            Debug.Log("Am i getting knocked back? " + knockBack);
        }
    }

}
