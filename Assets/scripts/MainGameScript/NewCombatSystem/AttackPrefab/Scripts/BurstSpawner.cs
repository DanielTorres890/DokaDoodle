using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class BurstSpawner : NonDamage
{
    public int spawnedBursts;

    
    public override void AbilityAction()
    {
        if (!IsServer) { return; }
        lifetimer += Time.deltaTime;

        if (lifetimer > (attackInfo as MultiBurst).interval)
        {
            var burstInfo = (attackInfo as MultiBurst);

            Vector3[] targetPositions = new Vector3[burstInfo.attacksPerBurst];
            int targetIndex = 0;
            if (burstInfo.targetMode == BurstTargetMode.TargetEntities)
            {

                foreach (var combatant in NewCombatManager.instance.allCombatants)
                {
                    if (targetIndex > burstInfo.attacksPerBurst) { break; }
                    if (!combatant.stats.isDead && !combatant.stats.loyaltyTags.Intersect(ownerStats.loyaltyTags).Any())
                    {
                        var individual = prepareObj(0, combatant.transform.position, 0, Quaternion.Euler(Vector3.zero));
                        
                        if(individual.TryGetComponent(out Rigidbody body) && burstInfo.attackInfo is MDefault)
                        {
                            body.linearVelocity = body.transform.forward * (burstInfo.attackInfo as MDefault).speed;
                        }
                        
                        if (burstInfo.individualAttackSound)
                            individual.GetComponent<AttackSoundPlayer>().PlaySoundRpc(NetworkData.Instance.audioDataBase.GetId[burstInfo.individualAttackSound]);

                    }
                }

            }
            
            if(burstInfo.targetMode == BurstTargetMode.Circle)
            {
                for (int i = 0; i < burstInfo.attacksPerBurst; i++)
                {
                    Vector3 angle = transform.localEulerAngles;
                    angle = new Vector3(angle.x, angle.y + i * (360f / burstInfo.attacksPerBurst), angle.z);

                    var individual = prepareObj(i, gameObject.transform.position + owner.transform.TransformDirection(burstInfo.attackOffset), burstInfo.attackRadius, Quaternion.Euler(angle));

                    if (individual.TryGetComponent(out Rigidbody body) && burstInfo.attackInfo is MDefault)
                    {
                        body.linearVelocity = body.transform.forward * (burstInfo.attackInfo as MDefault).speed;
                    }
                    
                    if (burstInfo.individualAttackSound)
                        individual.GetComponent<AttackSoundPlayer>().PlaySoundRpc(NetworkData.Instance.audioDataBase.GetId[burstInfo.individualAttackSound]);
                }
            }
            

            lifetimer = 0;
            spawnedBursts += 1;
            transform.Rotate(Vector3.up * burstInfo.intervalRotation);
            if (spawnedBursts >= burstInfo.totalBursts)
            {
                Destroy(gameObject);
            }
        }


    }
    private GameObject prepareObj(int i, Vector3 spawnPos, float radius, Quaternion rotationAngles)
    {
        var burstInfo = (attackInfo as MultiBurst);
        //var individual = Instantiate(burstInfo.individualAttack);
        var individual = burstInfo.attackInfo.WeaponEffect(owner, Time.time, spawnPos, owner.transform.eulerAngles, chargedDuration, owner.transform.position, owner.transform.forward);
        individual.transform.rotation = rotationAngles;
        individual.transform.GetComponent<NetworkTransform>().Teleport(spawnPos + individual.transform.forward * radius, rotationAngles, individual.transform.localScale);

        var cash = individual.GetComponent<AbilityBase>();
        cash.owner = owner;
        cash.ownerStats = ownerStats;
        cash.attackInfo = burstInfo.attackInfo;
        cash.lifespan = burstInfo.attackInfo.lifespan;
        cash.chargedDuration = chargedDuration;
        individual.transform.localScale = cash.attackInfo.ablitySize;
        return individual;
    }
    public enum BurstTargetMode
    {
        Circle,
        TargetEntities
    }
}
