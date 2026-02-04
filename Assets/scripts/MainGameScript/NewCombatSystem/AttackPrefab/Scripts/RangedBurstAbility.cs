using Unity.Netcode;
using UnityEngine;

public class RangedBurstAbility : RangedAbility
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
        cash.hitGameObject = hitGameObject;
        burst.GetComponent<NetworkObject>().Spawn();
        

        base.OnHit();
    }
    public override void OnTriggerEnter(Collider other)
    {

        if (!IsServer || other.gameObject == owner) { return; }

        OnHit();       

    }
}
