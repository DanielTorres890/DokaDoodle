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
    public AbilityManager targetManager;


    public NavMeshAgent agent;

    public LayerMask whatIsGround, whatIsPLayer;

    public AbilityManager myManager;


    public float attackRange;
    public bool InAttackRange;

    public float stateDuration;


    public override void OnNetworkSpawn()
    {
        
        
        agent = GetComponent<NavMeshAgent>();
        FindEnemy();
        agent.speed += myManager.stats.speedFormula();

       
    }

    private void FindEnemy()
    {
        
        if (targetManager == null || targetManager.stats.isDead) { targetManager = NewCombatManager.instance.allCombatants[0];  }
        
        foreach (var entity in NewCombatManager.instance.allCombatants)
        {
            
            if (gameObject == entity.gameObject || entity.stats.isDead) { continue; }

            if (targetManager.gameObject == gameObject || targetManager.stats.isDead) { targetManager = entity; }
            

            if (!entity.stats.loyaltyTags.Intersect(myManager.stats.loyaltyTags).Any() && Vector3.Distance(gameObject.transform.position, entity.gameObject.transform.position) < Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position))
            {
                targetManager = entity;
            }

        }
    }
    private void Update()
    {
       
        if(NewCombatManager.instance.fightOver) { return; }

        InAttackRange = Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position) < attackRange ;
        
        if(!InAttackRange) { ChasePlayer();  }


        if (targetManager == null || targetManager.gameObject.gameObject == gameObject || targetManager.stats.isDead) { FindEnemy(); }


        if (InAttackRange) { AttackPlayer(); }
        
        
        
    }


    
    private void ChasePlayer()
    {
        

        if (myManager.combatantstate == combatantStates.Free ||  myManager.combatantstate == combatantStates.StartUpFree)
        agent.SetDestination(targetManager.gameObject.transform.position);

        if (!IsServer) { return; }
        myManager.stateManager[myManager.stats.attacks[0]].pressed = false;
    }
    private void AttackPlayer()
    {
        transform.LookAt(new Vector3(targetManager.gameObject.transform.position.x, transform.position.y , targetManager.gameObject.transform.position.z));
        if(!IsServer) { return; }

        myManager.stateManager[myManager.stats.attacks[0]].pressed = true;

        if (myManager.combatantstate != combatantStates.Free || myManager.combatantstate != combatantStates.StartUpFree)
        {
            agent.SetDestination(transform.position);
        }
        
    }
}
