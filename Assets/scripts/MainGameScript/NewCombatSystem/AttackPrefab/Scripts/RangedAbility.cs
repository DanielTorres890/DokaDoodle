using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RangedAbility : AbilityBase
{

    private int pierceCounter = 0;
    // Update is called once per frame
    private void Start()
    {
        
    }

    private new void Update()
    {
        base.Update();

    }
    public override void OnHit()
    {

        
        if (hitGameObject)
        {

            var fx = Instantiate(hitGameObject);
            fx.transform.position = transform.position;
            fx.GetComponent<NetworkObject>().Spawn();
        }
        if(pierceCounter >= (attackInfo as MDefault).pierceCount)
        {
            Destroy(gameObject);
        }
        pierceCounter++;
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
