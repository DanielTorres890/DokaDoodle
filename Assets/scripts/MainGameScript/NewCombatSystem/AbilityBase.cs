using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class AbilityBase : NetworkBehaviour
{
    public GameObject owner;
    public AttackBase attackInfo;

    public EntityStats ownerStats;

    public float lifespan;
    private float lifetimer;


    private void Awake()
    {

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

    public int DamageCalculator(EntityStats defender)
    {
        float totalDamge = 0;
        foreach (var offense in attackInfo.multipliers)
        {
            totalDamge += offense.mult * ownerStats.stats[offense.attribute];
        }
        foreach (var defense in attackInfo.defenseMult)
        {
            totalDamge -= defense.mult * defender.stats[defense.attribute];
        }

        if (totalDamge < 0)
            return 0;
        else
            return Mathf.RoundToInt(totalDamge);

    }
    /*public void OnTriggerEnter(Collider other)
    {

        if (!(other.gameObject.CompareTag("damageable") && other.gameObject != owner))
        {
            return;

        }
        Debug.Log(other.gameObject);
        Debug.Log(owner);
        other.gameObject.GetComponent<AbilityManager>().OnTriggerEnter(GetComponent<Collider>());


    }*/


}