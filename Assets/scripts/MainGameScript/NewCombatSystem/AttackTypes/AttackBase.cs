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

    public float startUp;
    public float attackDuration; //its kinda a weird thing but basically if it was a dash attack or something similar where you're moving we need a seperate state for that (i think)
    public float endLag;
    public float cooldown;
    public float lifespan;

    public combatantStates stateToBe;

    public bool chargeable;

    public GameObject attackPrefab;
    public GameObject startUpPrefab; //tea
    public Vector3 offset = Vector3.zero;
    public Vector3 ablitySize = Vector3.one;

    public AttackMult[] multipliers = new AttackMult[7] {new AttackMult(Attributes.MaxHealth), new AttackMult(Attributes.Health) , new AttackMult(Attributes.Attack) , new AttackMult(Attributes.Defense) , new AttackMult(Attributes.Magic) , new AttackMult(Attributes.MDefense) , new AttackMult(Attributes.Dexterity)};
    public AttackMult[] defenseMult;
    public AttackMult[] antiGuardMultipliers = new AttackMult[7] { new AttackMult(Attributes.MaxHealth, 1), new AttackMult(Attributes.Health, 1), new AttackMult(Attributes.Attack, 1), new AttackMult(Attributes.Defense, 1), new AttackMult(Attributes.Magic, 1), new AttackMult(Attributes.MDefense, 1), new AttackMult(Attributes.Dexterity, 1) };

    [Tooltip("This is specifically for players, enemies have their clips in their own stuff")]
    public AnimationClip attackAnimation;


    public AudioClip attackSound;
    //^ Saves me the annoyance of setting them everytime i create a scriptable
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
    public virtual GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking)
    {
        var attack = Instantiate(attackPrefab);
        attack.transform.position = whereiscaster +  Quaternion.Euler(casterLooking) * offset;
        attack.transform.rotation = caster.transform.rotation;
        attack.transform.localScale = ablitySize;
        attack.GetComponent<NetworkObject>().Spawn(true);
        


        var info = attack.GetComponent<AbilityBase>();
        
        info.owner = caster;
        info.lifespan = lifespan - (time - NetworkManager.Singleton.ServerTime.TimeAsFloat);

        var manager = caster.GetComponent<AbilityManager>();
        manager.RealAttackRpc(caster.GetComponent<NetworkObject>().NetworkManager.RpcTarget.Single(caster.GetComponent<NetworkObject>().OwnerClientId, RpcTargetUse.Temp));
        info.ownerStats = caster.GetComponent<AbilityManager>().stats;
       
        info.attackInfo = this;
        return attack;
    }
    public virtual void OnStartUp(GameObject caster)
    {

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
