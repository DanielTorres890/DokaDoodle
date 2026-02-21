using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PickUp : AbilityBase
{
    [Tooltip("how long to wait before becoming actively pickupable ")]
    public float prewarmDuration;
    private float prewarmTimer;


    public GameObject burstHitbox;
    public UnityEvent onStopMove;
    public int pickupLimit = 3;
    public override void OnNetworkSpawn()
    {
        if(!IsHost) { return; }
        Rigidbody body = GetComponent<Rigidbody>();
        transform.rotation = Quaternion.LookRotation(body.linearVelocity.normalized);
        transform.eulerAngles = new Vector3 (0,transform.eulerAngles.y,0);
        base.OnNetworkSpawn();
       
    }
    public override void Update()
    {
        prewarmTimer += Time.deltaTime;
        base.Update();
    }
    public override void OnHit()
    {
        int totalStatuses = 0;
        foreach(var status in ownerStats.statuses)
        {
            if (status.buffId == NetworkData.Instance.buffDataBase.GetId[attackInfo.onHitEffects[0]])
            {
                totalStatuses++;
            }
        }
        if(totalStatuses >= pickupLimit) { return; }

        int[] onHitIds = new int[attackInfo.onHitEffects.Length];
        int counter = 0;
        foreach(var status in attackInfo.onHitEffects)
        {
            onHitIds[counter] = NetworkData.Instance.buffDataBase.GetId[status];
            
        }
        owner.GetComponent<AbilityManager>().IGainedBuffRpc(onHitIds);
        base.OnHit();
    }
    public override void OnTriggerEnter(Collider other)
    {

        if (!IsServer) { return; }
        if (prewarmDuration > prewarmTimer) { return; }

        if (other.TryGetComponent<AbilityBase>(out AbilityBase hoe))
        {
            return;
        }

        //3 being the floor layer number
        if (other.gameObject.layer == 3)
        {
           
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            onStopMove.Invoke();
        }
        else if(prewarmDuration <= prewarmTimer && other.TryGetComponent(out AbilityManager othersManager))
        {
            if(othersManager.stats == ownerStats)
            OnHit();
        }
        

    }

    

}
