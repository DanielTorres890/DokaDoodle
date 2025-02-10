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

   

}
