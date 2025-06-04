using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public abstract class AbilityBase : NetworkBehaviour
{
    public GameObject owner;
    public AttackBase attackInfo;

    public EntityStats ownerStats;
    public AttackTypes attackType;
    
    public float lifespan;
    private float lifetimer;


    private void Awake()
    {
        lifetimer = 0f;
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
        float totalDamge = 0;
        foreach (var offense in attackInfo.multipliers)
        {
            totalDamge += offense.mult * ownerStats.postStatusStats[offense.attribute];
        }
        foreach (var defense in attackInfo.defenseMult)
        {
            totalDamge -= defense.mult * defender.postStatusStats[defense.attribute];
        }
        totalDamge *= (1 - defender.dmgReduction[attackType]);
        if (totalDamge < 0)
            return 0;
        else
            return Mathf.RoundToInt(totalDamge);

    }
    public virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log("Did i enter their hitbox");
        if(!IsServer || other.gameObject == owner) { return; }
        
        OnHit();

        Debug.Log(other.gameObject);
        Debug.Log(owner);

        if (other.gameObject.TryGetComponent(out AbilityManager hitby))
        {
            
         
            Debug.Log("ERRRR" + other.GetType());

            hitby.ImHitRpc(DamageCalculator(hitby.stats));
        }

    }


}
public enum AttackTypes
{
    Physical,
    Magic,

}