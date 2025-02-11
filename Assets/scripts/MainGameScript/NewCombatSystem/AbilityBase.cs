using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    public GameObject owner;
    public AttackBase attackInfo;


    public float lifespan;
    private float lifetimer;


    public void Update()
    {
       if (lifespan < lifetimer)
        {
            Destroy(gameObject);

        }
        lifetimer += Time.deltaTime;

    }

   public int DamageCalculator(EntityStats attacker, EntityStats defender)
    {
        float totalDamge = 0;
        foreach (var offense in attackInfo.multipliers)
        {
            totalDamge += offense.mult * attacker.stats[offense.attribute];
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

}
