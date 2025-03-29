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

    public AttackBase selectedAttack;
    public float attackRange;
    public bool InAttackRange;


    public float releaseTimer = 0;
    public float timeToHold;

    public override void OnNetworkSpawn()
    {
        
        
        agent = GetComponent<NavMeshAgent>();
        FindEnemy();
        agent.speed += myManager.stats.speedFormula();

       
    }

    public void FindEnemy()
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
    public virtual void Update()
    {
        AttackHold();

        if (NewCombatManager.instance.fightOver) { return; }

        InAttackRange = Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position) < attackRange ;

        if (targetManager == null || targetManager.gameObject.gameObject == gameObject || targetManager.stats.isDead) { FindEnemy(); }



        if (!InAttackRange) { ChasePlayer();  }


        

        if (InAttackRange) { AttackPlayer(); }
        
        
        
    }


    
    public virtual void ChasePlayer()
    {
        

        if (myManager.combatantstate == combatantStates.Free ||  myManager.combatantstate == combatantStates.StartUpFree)
        agent.SetDestination(targetManager.gameObject.transform.position);

        if (!IsServer) { return; }
        myManager.stateManager[myManager.stats.attacks[0]].pressed = false;
    }


    public virtual void AttackPlayer()
    {
        
        transform.LookAt(new Vector3(targetManager.gameObject.transform.position.x, transform.position.y , targetManager.gameObject.transform.position.z));
        if(!IsServer) { return; }
        if( myManager.CanAct())
        {
            
            selectAttack();
            Debug.Log("What are u" + selectedAttack);
            myManager.stateManager[selectedAttack].pressed = true;
        }
        

        if (!myManager.CanMove())
        {
            agent.SetDestination(transform.position);
        }
        
    }

    public virtual void selectAttack()
    {
        selectedAttack = myManager.stats.attacks[0];
    }

    public void AttackHold()
    {
        if (!myManager.CanMove())
        {
            releaseTimer += Time.deltaTime;
           
            if (releaseTimer > timeToHold)
            {
                Debug.Log("Did this happen");
                myManager.stateManager[selectedAttack].pressed = false;
                releaseTimer = 0;
            }
        }
    }
}
