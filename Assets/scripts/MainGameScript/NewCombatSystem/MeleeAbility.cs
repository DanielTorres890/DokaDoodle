using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAbility : AbilityBase
{
    
    public new void Update()
    {
        base.Update();
    }

    /*public void OnTriggerEnter(Collider other)
    {
        if (!(other.gameObject.CompareTag("damageable") && other.gameObject != base.owner)) 
        {
            return;

        }
        var info = other.gameObject.GetComponent<AbilityManager>();
        Debug.Log("I HIT SOMEONE FOR " + DamageCalculator( info.stats));
        info.stats.stats[Attributes.Health] -= DamageCalculator(info.stats);

        //This would deal damage (hopefully)
        //info.stats.stats[Attributes.Health] -= 1;
    }*/
}
