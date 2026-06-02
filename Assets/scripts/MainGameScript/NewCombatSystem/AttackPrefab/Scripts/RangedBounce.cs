using Unity.Netcode;
using UnityEngine;

public class RangedBounce : RangedAbility
{

    public GameObject burstHitbox;

    public float firstBounceExtraGravity = 0f;
    public float maxYVelocity;
    public bool hitGround = false;

    private bool caughtVelocity;

    [SerializeField] private Vector3 trueBounce;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();


        
        
    }
    public override void OnHit()
    {
        var burst = Instantiate(burstHitbox);
        burst.transform.position = gameObject.transform.position;

        var cash = burst.GetComponent<AbilityBase>();
        var burstattack = (attackInfo as BurstAtk);
        cash.owner = owner;
        cash.ownerStats = ownerStats;
        cash.attackInfo = attackInfo;
        cash.lifespan = burstattack.burstLifespan;

        burst.transform.localScale = burstattack.burstSize;
        burst.GetComponent<NetworkObject>().Spawn();

    }

    public override void Update()
    {
        base.Update();
        if (!IsServer) { return; }
        if (firstBounceExtraGravity != 0)
        {
            if(!caughtVelocity && body.linearVelocity.magnitude > 4f)
            {
                trueBounce = body.linearVelocity;
                body.linearVelocity = new Vector3(0, -firstBounceExtraGravity, 0);
                caughtVelocity = true;
            }
            
        }
    }

    public override void OnTriggerEnter(Collider other)
    {

        if (!IsServer || other.gameObject == owner) { return; }

        OnHit();

    }
    public virtual void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || collision.gameObject == owner) { return; }

        if (!hitGround && firstBounceExtraGravity != 0)
        {
            float lastY = body.linearVelocity.y;
            body.linearVelocity = trueBounce;
            body.linearVelocity = new Vector3(body.linearVelocity.x, Mathf.Clamp(lastY, -maxYVelocity, maxYVelocity), body.linearVelocity.z);
        }
            
        hitGround = true;

        
        OnHit();
        if(collision.gameObject.TryGetComponent<AbilityManager>(out AbilityManager hitEntity))
        {
            Destroy(gameObject);
        }
    }
}
