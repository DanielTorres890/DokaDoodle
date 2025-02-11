using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAbility : AbilityBase
{

    [SerializeField] private float speed;
    // Update is called once per frame
    private void Start()
    {
        gameObject.GetComponent<Rigidbody>().velocity = transform.TransformDirection(Vector3.forward * speed);
    }

    private new void Update()
    {
        base.Update();

    }
    public void OnTriggerEnter(Collider other)
    {
        if (!(other.gameObject.CompareTag("damageable") && other.gameObject != base.owner))
        {
            return;

        }
        var info = other.gameObject.GetComponent<AbilityManager>();
        Debug.Log("I HIT SOMEONE FOR " + DamageCalculator(owner.GetComponent<AbilityManager>().stats, info.stats));
        info.stats.stats[Attributes.Health] -= DamageCalculator(owner.GetComponent<AbilityManager>().stats, info.stats);
        Destroy(gameObject);

        //This would deal damage (hopefully)
        //info.stats.stats[Attributes.Health] -= 1;
    }
}
