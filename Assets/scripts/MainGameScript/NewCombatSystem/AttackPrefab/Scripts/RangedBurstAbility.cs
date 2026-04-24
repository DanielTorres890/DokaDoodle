using Unity.Netcode;
using UnityEngine;

public class RangedBurstAbility : RangedAbility
{
    public GameObject burstHitbox;
    private bool alreadyExploded;

    public override void OnHit()
    {
        Debug.Log("BRUH IM HITTING FRICK U");
        if(alreadyExploded) { return; }

        alreadyExploded = true;
        var burst = Instantiate(burstHitbox);
        burst.transform.position = gameObject.transform.position;

        var cash = burst.GetComponent<AbilityBase>();
        Debug.Log("bru who is this " + attackInfo.attackName);
        var burstattack = (attackInfo as BurstAtk);
        cash.owner = owner;
        cash.ownerStats = ownerStats;
        cash.attackInfo = attackInfo;
        cash.lifespan = burstattack.burstLifespan;

        burst.transform.localScale = Mathf.Clamp(burstattack.ChargeMultiplier(ownerStats, chargedDuration) * (burstattack.maxChargeSizeBuff - 1) + 1, 1, 99999) * burstattack.burstSize;
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
