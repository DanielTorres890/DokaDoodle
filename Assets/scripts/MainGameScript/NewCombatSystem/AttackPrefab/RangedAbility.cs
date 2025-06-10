using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAbility : AbilityBase
{
    
    
    // Update is called once per frame
    private void Start()
    {
        
    }

    private new void Update()
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
        Debug.Log("I HIT SOMEONE FOR " + DamageCalculator(info.stats));
        info.stats.stats[Attributes.Health] -= DamageCalculator(info.stats);
        Destroy(gameObject);

        //This would deal damage (hopefully)
        //info.stats.stats[Attributes.Health] -= 1;
    }*/
}
