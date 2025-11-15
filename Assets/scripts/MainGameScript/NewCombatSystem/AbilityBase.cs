using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public abstract class AbilityBase : NetworkBehaviour
{
    public GameObject owner;
    public AttackBase attackInfo;

    public EntityStats ownerStats;
    public AttackTypes attackType;
    
    public float lifespan;
    [DoNotSerialize]public float lifetimer;
    public float chargedDuration;


    private AudioSource AudioSource;

    private void Awake()
    {
        lifetimer = 0f;
        TryGetComponent(out AudioSource);
    }
    public void Update()
    {
        if (!IsServer) { return; }
        if (lifespan < lifetimer)
        {
            Destroy(gameObject);

        }
        lifetimer += Time.deltaTime;

    }
    public virtual void OnHit()
    {
        
        Destroy(gameObject);
    }
    public int DamageCalculator(EntityStats defender)
    {
        float totalDamge = attackInfo.baseDamage;

        foreach (var offense in attackInfo.multipliers)
        {
 
            totalDamge += offense.mult * ownerStats.postStatusStats[offense.attribute];
        }
        foreach (var defense in attackInfo.defenseMult)
        {
            totalDamge -= defense.mult * defender.postStatusStats[defense.attribute];
        }
        
        totalDamge *= (1 - defender.dmgReduction[attackType]/100f);
        totalDamge *= attackInfo.ChargeMultiplier(ownerStats, chargedDuration) * attackInfo.maxChargeAtkBuff;
        if (totalDamge < 0)
            return 0;
        else
            return Mathf.RoundToInt(totalDamge);

    }
    public virtual void OnTriggerEnter(Collider other)
    {
        
        if(!IsServer || other.gameObject == owner) { return; }
        
        

        
        if (other.gameObject.TryGetComponent(out AbilityManager hitby))
        {
            
            if(hitby.stats.loyaltyTags.Intersect(ownerStats.loyaltyTags).Any())
            {
                return;
            }


            hitby.ImHitRpc(DamageCalculator(hitby.stats));
            if (AudioSource && attackInfo.onHitSound)
            {
                PlayHitSoundRpc(NetworkData.Instance.audioDataBase.GetId[attackInfo.onHitSound]);
            }
            else
            {
                Debug.LogWarning(attackInfo.attackName + " Does not contain a hit SFX if you even care.... \nor this ability prefab doesn't contain an AudioSource");
            }
        }
        
        OnHit();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = true)]
    private void PlayHitSoundRpc(int soundId)
    {
        AudioSource.resource = NetworkData.Instance.audioDataBase.GetItem[soundId];
        AudioSource.Play();
    }

}
public enum AttackTypes
{
    Physical,
    Magic,

}