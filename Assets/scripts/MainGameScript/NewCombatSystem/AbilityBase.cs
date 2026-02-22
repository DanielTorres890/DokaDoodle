using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
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

    public bool destroyOnWallCollide;

    public GameObject hitGameObject;
    public AudioSource AudioSource;

    public bool stickInOpponent = false;
    private bool weaponsHot = true; //whether its still an active hitbox
    private void Awake()
    {
        lifetimer = 0f;
        TryGetComponent(out AudioSource);
        
       
    }
    public override void OnNetworkSpawn()
    {
        if(!IsServer) { return; }
        NewCombatManager.instance.onCombatEnd.AddListener(delegate { Destroy(gameObject); });

    }
    public virtual void Update()
    {

        AbilityAction();
    }
    public virtual void OnHit()
    {

        if (hitGameObject)
        {
          
            var fx = Instantiate(hitGameObject);
            fx.transform.position = transform.position;
            fx.GetComponent<NetworkObject>().Spawn();
        }
        if(!stickInOpponent) { Destroy(gameObject); }
        
        
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
        if(!weaponsHot) { return; }

        bool isEntity = false;
        
        if (other.gameObject.TryGetComponent(out AbilityManager hitby))
        {
           
            isEntity = true;
            if(NewCombatManager.instance && NewCombatManager.instance.fightOver) { return; }

            if(hitby.stats.loyaltyTags.Intersect(ownerStats.loyaltyTags).Any())
            {
                return;
            }


            hitby.ImHitRpc(DamageCalculator(hitby.stats));
            int[] buffIds = new int[attackInfo.onHitEffects.Length];
            for (int i = 0; i < attackInfo.onHitEffects.Length; i++)
            {
                buffIds[i] = NetworkData.Instance.buffDataBase.GetId[attackInfo.onHitEffects[i]];
            }
            hitby.IGainedBuffRpc(buffIds);
            if (AudioSource && attackInfo.onHitSound)
            {
                if(ownerStats is playerData)
                PlayHitSoundRpc(NetworkData.Instance.audioDataBase.GetId[attackInfo.onHitSound], RpcTarget.Single((ulong)(ownerStats as playerData).playerNumber, RpcTargetUse.Temp));
                
            }
            else
            {
                Debug.LogWarning(attackInfo.attackName + " Does not contain a hit SFX if you even care.... \nor this ability prefab doesn't contain an AudioSource");
            }
        }
        if(isEntity || destroyOnWallCollide)
        {
            if(stickInOpponent)
            {
                StartCoroutine(delay());
                NetworkObject networkedPart = GetComponent<NetworkObject>();
                networkedPart.TrySetParent(other.transform);
                
                if(networkedPart.TryGetComponent(out NetworkTransform component))
                {
                    component.enabled = false;
                }
                weaponsHot = false;
            }
            OnHit();
        }
        
    }

    [Rpc(SendTo.SpecifiedInParams, RequireOwnership = true)]
    private void PlayHitSoundRpc(int soundId, RpcParams rpcsend)
    {
        
        AudioSource.PlayClipAtPoint(NetworkData.Instance.audioDataBase.GetItem[soundId], transform.position, SettingsManager.instance.SFXVolume);

    }

    public virtual void AbilityAction()
    {
        if (!IsServer) { return; }
        if (lifespan < lifetimer)
        {
            Destroy(gameObject);

        }
        lifetimer += Time.deltaTime;
    }

    private IEnumerator delay()
    {
        yield return new WaitForSeconds(.15f);
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }
}
public enum AttackTypes
{
    Physical,
    Magic,

}