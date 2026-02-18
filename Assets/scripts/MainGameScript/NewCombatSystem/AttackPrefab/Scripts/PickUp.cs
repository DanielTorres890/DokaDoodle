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
        Debug.Log("Old roation" + transform.rotation);
        transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().linearVelocity.normalized);
        Debug.Log("new rotation " + transform.rotation);
        Debug.Log("what was my intial velocity? " + GetComponent<Rigidbody>().linearVelocity);
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
            Debug.Log("whomst " + other.gameObject.name);
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            onStopMove.Invoke();
        }
        else if(prewarmDuration <= prewarmTimer)
        {
            OnHit();
        }
        

    }



}
