using Unity.Netcode;
using UnityEngine;

public class MeteorySpawner : NonDamage
{
    public int spawnedMeteors;
    

    public override void AbilityAction()
    {
        if (!IsServer) { return; }
        lifetimer += Time.deltaTime;

        if(lifetimer > (attackInfo as MeteorShower).interval)
        {
            var meteorInfo = (attackInfo as MeteorShower);
            var meteor = Instantiate(meteorInfo.meteor);
            meteor.transform.position = gameObject.transform.position + owner.transform.TransformDirection(meteorInfo.meteorOffset); //not that this should matter

            var cash = meteor.GetComponent<AbilityBase>();
            cash.owner = owner;
            cash.ownerStats = ownerStats;
            cash.attackInfo = meteorInfo.meteorInfo;
            cash.lifespan = meteorInfo.lifespan;
            cash.chargedDuration = chargedDuration;

            meteor.transform.localScale = meteorInfo.ablitySize;
            var rigid = meteor.GetComponent<Rigidbody>();
            rigid.linearVelocity = new Vector3(Random.Range(-meteorInfo.directionWidth.x, meteorInfo.directionWidth.x), meteorInfo.directionWidth.y, Random.Range(-meteorInfo.directionWidth.z, meteorInfo.directionWidth.z));

            meteor.GetComponent<NetworkObject>().Spawn();
            meteor.GetComponent<AttackSoundPlayer>().PlaySoundRpc(NetworkData.Instance.audioDataBase.GetId[meteorInfo.meteorSound]);
            lifetimer = 0;
            spawnedMeteors += 1;
            if(spawnedMeteors >= meteorInfo.meteorCount)
            {
                Destroy(gameObject);
            }
        }
    }
}
