using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public abstract class AttackBase : ScriptableObject
{
    
    [TextArea(15, 20)]
    public string description;
    public string attackName;

    [Header("Attack frame data")]
    public float startUp;
    public float attackDuration; //its kinda a weird thing but basically if it was a dash attack or something similar where you're moving we need a seperate state for that (i think)
    public float endLag;
    public float cooldown;
    public float lifespan;


    [Header("Charging info")]
    public float energyDrain = 1f;

    [Tooltip("Max charge refers to an attack charged by whatever the duration of MaxChargeDuration is\nThis is BEFORE its affected by potency ")]
    public float maxChargeAtkBuff = 1.2f; 
    public float maxChargeSizeBuff = 1.2f;
    public float maxChargeSpeedBuff = 1.1f; 
    public float maxChargeDuration = 3f;
    [Tooltip("The amount of potency required to reach maximum effectiveness ")]
    public int requiredPotency = 0;
    public float dmgPotencyEffect = 2;
    public float sizePotencyEffect = 2;

    [Tooltip("What state should an attack enter after cast (like for dash attack you enter attacking so you can move) ")]
    public combatantStates stateToBe;

    public bool chargeable;

    [Header("Prefabs and prefab modifiers")]
    public GameObject attackPrefab;
    public GameObject startUpPrefab; //tea

    [Tooltip("Additional Fx for on spawn just incase")]
    public GameObject spawnFx;
    public Vector3 offset = Vector3.zero;
    public Vector3 ablitySize = Vector3.one;


    [Header("Damage info")]
    public int baseDamage;
    public AttackMult[] multipliers = new AttackMult[7] {new AttackMult(Attributes.MaxHealth), new AttackMult(Attributes.Health) , new AttackMult(Attributes.Attack) , new AttackMult(Attributes.Defense) , new AttackMult(Attributes.Magic) , new AttackMult(Attributes.MDefense) , new AttackMult(Attributes.Dexterity)};
    public AttackMult[] defenseMult;
    public AttackMult[] antiGuardMultipliers = new AttackMult[7] { new AttackMult(Attributes.MaxHealth, 1), new AttackMult(Attributes.Health, 1), new AttackMult(Attributes.Attack, 1), new AttackMult(Attributes.Defense, 1), new AttackMult(Attributes.Magic, 1), new AttackMult(Attributes.MDefense, 1), new AttackMult(Attributes.Dexterity, 1) };

    [Tooltip("This is specifically for players, enemies have their clips in their own stuff")]

    public AnimationClip startUpAnimation;
    public AnimationClip attackAnimation;
    public float animationSpeed = 1f;
    public ItemBuff[] LevelRequirements;

    public AudioClip attackSound;
    public AudioClip onHitSound;

    public BuffBase[] onHitEffects; //im not really a fan of this one i'll be honest but it makes the most sense in my brain
    //^ Saves me the annoyance of setting them everytime i create a scriptable
    public LayerMask targets;

    
    public virtual GameObject WeaponEffect(GameObject caster)
    {
       
        var attack = Instantiate(attackPrefab);
        attack.transform.position = caster.transform.position + caster.transform.TransformDirection(offset);
        attack.transform.rotation = caster.transform.rotation;
        attack.transform.localScale = ablitySize;

        var info = attack.GetComponent<AbilityBase>();

        info.owner = caster;
        info.lifespan = lifespan;
        info.ownerStats = caster.GetComponent<AbilityManager>().stats;
        return attack; 
        

    }
    public virtual GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking, float chargedDuration)
    {
        var manager = caster.GetComponent<AbilityManager>();
        var attack = Instantiate(attackPrefab);
        var casterManager = caster.GetComponent<AbilityManager>().stats;


        

        attack.transform.position = whereiscaster +  Quaternion.Euler(casterLooking) * offset;
        attack.transform.rotation = caster.transform.rotation;
        attack.transform.localScale = ChargeMultiplier(manager.stats, chargedDuration) * maxChargeSizeBuff * ablitySize;


        attack.GetComponent<NetworkObject>().Spawn(true);

        if(spawnFx)
        {
            var fx = Instantiate(spawnFx);
            fx.transform.position = caster.transform.position;
            fx.transform.rotation = caster.transform.rotation;
            fx.transform.localScale = ChargeMultiplier(manager.stats, chargedDuration) * maxChargeSizeBuff * ablitySize;
            fx.GetComponent<NetworkObject>().Spawn(true);
        }
        

        var info = attack.GetComponent<AbilityBase>();
        
        info.owner = caster;
        info.lifespan = lifespan - (time - NetworkManager.Singleton.ServerTime.TimeAsFloat);
        info.chargedDuration = chargedDuration;
        
        manager.RealAttackRpc(caster.GetComponent<NetworkObject>().NetworkManager.RpcTarget.Single(caster.GetComponent<NetworkObject>().OwnerClientId, RpcTargetUse.Temp));
        info.ownerStats = casterManager;
       
        info.attackInfo = this;
        return attack;
    }
    public virtual void OnStartUp(GameObject caster)
    {

    }

    public virtual float ChargeMultiplier(EntityStats entity, float chargeDuration)
    {
        if(chargeDuration > maxChargeDuration)
        {
            chargeDuration = maxChargeDuration;
        }
        int usedPotency = entity.stats[Attributes.Potency];
        if(usedPotency > requiredPotency)
        {
            usedPotency = requiredPotency;
        }

        return 1 + (usedPotency + 1) / (requiredPotency + 1) * (chargeDuration / maxChargeDuration);
    }
    public bool meetsRequirement(EntityStats entity)
    {
        foreach(var Requirement in LevelRequirements)
        {
            if (entity.stats[Requirement.attribute]  < entity.stats[Requirement.attribute])
            {
                return false;
            }
        }
        return true;
    }
}

[System.Serializable]
public class AttackMult
{
    public Attributes attribute;
    public float mult = 0f;

    public AttackMult(Attributes attributes)
    {
        attribute = attributes;
    }

    public AttackMult(Attributes attributes, float multi)
    {
        attribute = attributes;
        mult = multi;
    }
}
