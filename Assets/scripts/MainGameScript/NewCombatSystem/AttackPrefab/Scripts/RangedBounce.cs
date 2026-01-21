using Unity.Netcode;
using UnityEngine;

public class RangedBounce : RangedAbility
{

    public GameObject burstHitbox;


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

    public override void OnTriggerEnter(Collider other)
    {

        if (!IsServer || other.gameObject == owner) { return; }

        OnHit();

    }
    public virtual void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || collision.gameObject == owner) { return; }

        OnHit();
        if(collision.gameObject.TryGetComponent<AbilityManager>(out AbilityManager hitEntity))
        {
            Destroy(gameObject);
        }
    }
}
