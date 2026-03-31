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

        burst.transform.localScale = burstattack.burstSize * burstattack.ChargeMultiplier(owner.GetComponent<AbilityManager>().stats, chargedDuration) * burstattack.maxChargeSizeBuff;
        cash.hitGameObject = hitGameObject;
        burst.GetComponent<NetworkObject>().Spawn();


        if (hitGameObject)
        {

            var fx = Instantiate(hitGameObject);
            fx.transform.position = transform.position;
            fx.transform.localScale = burst.transform.localScale;
            fx.GetComponent<NetworkObject>().Spawn();
        }
        if (pierceCounter >= (attackInfo as MDefault).pierceCount)
        {
            Destroy(gameObject);
        }
        pierceCounter++;
    }

    public override void OnTriggerEnter(Collider other)
    {

        if (!IsServer || other.gameObject == owner) { return; }

        OnHit();       

    }
}
