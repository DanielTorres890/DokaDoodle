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
        var burst = Instantiate(burstHitbox);
        burst.transform.position = owner.transform.position;
        burst.transform.rotation = owner.transform.rotation;
        var cash = burst.GetComponent<AbilityBase>();
        var burstattack = (attackInfo as BurstAtk);
        cash.owner = owner;
        cash.ownerStats = ownerStats;
        cash.attackInfo = attackInfo;
        cash.lifespan = burstattack.burstLifespan;
        burst.transform.position += burstattack.offset;

        burst.transform.localScale = burstattack.burstSize;
        cash.hitGameObject = hitGameObject;
        burst.GetComponent<NetworkObject>().Spawn();


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
