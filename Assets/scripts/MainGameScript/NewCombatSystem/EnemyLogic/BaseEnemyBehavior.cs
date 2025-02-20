using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemyBehavior : NetworkBehaviour
{
    public GameObject target;

    public NavMeshAgent agent;

    public LayerMask whatIsGround, whatIsPLayer;

    public AbilityManager myManager;

    public float attackRange;
    public bool InAttackRange;

    public float stateDuration;


    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }
        
        agent = GetComponent<NavMeshAgent>();
        FindEnemy();

       
    }

    private void FindEnemy()
    {
        if (target == null) { target = NewCombatManager.instance.allCombatants[0].gameObject;  }

        
        foreach (var entity in NewCombatManager.instance.allCombatants)
        {
            
            if (gameObject == entity.gameObject) { continue; }

            if (target == gameObject) { target = entity.gameObject; }
            

            if (!entity.stats.loyaltyTags.Intersect(myManager.stats.loyaltyTags).Any() && Vector3.Distance(gameObject.transform.position, entity.gameObject.transform.position) < Vector3.Distance(gameObject.transform.position, target.transform.position))
            {
                target = entity.gameObject;
            }

        }
    }
    private void Update()
    {
        if (!IsServer) { return; }
        
        InAttackRange = Vector3.Distance(gameObject.transform.position, target.transform.position) < attackRange ;
        
        if(!InAttackRange) { ChasePlayer();  }
        
        if(InAttackRange) { AttackPlayer(); }
        
        if(target == null || target.gameObject == gameObject) { FindEnemy(); }
        
    }


    
    private void ChasePlayer()
    {
        myManager.stateManager[myManager.stats.attacks[0]].pressed = false;

        if (myManager.combatantstate == combatantStates.Free ||  myManager.combatantstate == combatantStates.StartUpFree)
        agent.SetDestination(target.transform.position);
    }
    private void AttackPlayer()
    {
        transform.LookAt(new Vector3(target.transform.position.x, transform.position.y ,target.transform.position.z));
        myManager.stateManager[myManager.stats.attacks[0]].pressed = true;

        if (myManager.combatantstate != combatantStates.Free || myManager.combatantstate != combatantStates.StartUpFree)
        {
            agent.SetDestination(transform.position);
        }
        
    }
}
