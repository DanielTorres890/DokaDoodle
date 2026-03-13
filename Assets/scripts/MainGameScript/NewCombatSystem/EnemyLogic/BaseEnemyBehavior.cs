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
    public float[] attackRanges;
    public bool InAttackRange;

    public Animator animator;
    [DoNotSerialize]public AnimatorOverrideController overrideController;

    public AnimationClip walkingAnimation;

    [Tooltip("This array works under the assumption that every attack has both a startUp and attack Animation")]
    public AttackAnimation[] attackAnimations; 

    public float releaseTimer = 0;
    public float timeToHold;

    public float baseMoveSpeed = 4f;

    [DoNotSerialize] public Rigidbody rb; 
    public override void OnNetworkSpawn()
    {
        agent = GetComponent<NavMeshAgent>();
        myManager = GetComponent<AbilityManager>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        overrideController["DefaultWalking"] = walkingAnimation;
        animator.runtimeAnimatorController = overrideController;


        

        if(IsServer)
        {
            myManager.onHit.AddListener(FindEnemy);
            FindEnemy();
        }
        
        

       
    }

    public void FindEnemy()
    {

        if (targetManager == null || targetManager.stats.isDead) { targetManager = NewCombatManager.instance.allCombatants[0]; }

        foreach (var entity in NewCombatManager.instance.allCombatants)
        {
            
            if (entity == null || gameObject == entity.gameObject || entity.stats.isDead) { continue; }

            if (targetManager.gameObject == gameObject || targetManager.stats.isDead) { targetManager = entity; }
            
            
            if (!entity.stats.loyaltyTags.Intersect(myManager.stats.loyaltyTags).Any() )
            {
               
                if (targetManager.stats.loyaltyTags.Intersect(myManager.stats.loyaltyTags).Any())
                {
                    targetManager = entity;
                 
                    continue;
                }
                if(Vector3.Distance(gameObject.transform.position, entity.gameObject.transform.position) < Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position))
                {
                    targetManager = entity;
                    
                }
            }
            

        }
        


       
    }
    public virtual void Update()
    {
        if(myManager.stats.isDead) { agent.enabled = false; return; }
        if(myManager.CanWalk()) { agent.enabled = true; rb.isKinematic = true; }
        else { agent.enabled = false; }


   
        


        if (!IsServer) { return; }
        AttackHold();

        if (NewCombatManager.instance.fightOver) { return; }

        

        if (targetManager == null || targetManager.gameObject.gameObject == gameObject || targetManager.stats.isDead || targetManager.stats.loyaltyTags.Intersect(myManager.stats.loyaltyTags).Any()) { FindEnemy(); } //the checks on the if are kinda redundant but thats okay

        
        if (targetManager == null) { return; }


        InAttackRange = InRange();

        if (!InAttackRange) { ChasePlayer();  }


        

        if (InAttackRange || myManager.combatantstate == combatantStates.Attacking) { AttackPlayer(); }
        
        
        
    }


    public virtual bool InRange()
    {
        return Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position) < attackRanges[0];
    }
    public virtual void ChasePlayer()
    {
        
        if(!myManager.CanWalk()) { return; }

        if (myManager.combatantstate == combatantStates.Free ||  myManager.combatantstate == combatantStates.StartUpFree)

        agent.speed = myManager.stats.speedFormula() + baseMoveSpeed;
        agent.SetDestination(targetManager.gameObject.transform.position);
       

        if (!IsServer) { return; }

        if (myManager.combatantstate == combatantStates.Free)
        {
            animator.SetBool("Attacking", false);
            animator.SetBool("StartUp", false);
            animator.SetBool("Walking", true);
            animator.SetFloat("AnimSpeed", 1);
        }
        
        myManager.stateManager[myManager.stats.attacks[0]].pressed = false;
    }


    public virtual void AttackPlayer()
    {
        
        transform.LookAt(new Vector3(targetManager.gameObject.transform.position.x, transform.position.y , targetManager.gameObject.transform.position.z));
        if(!IsServer) { return; }

        

        if ( myManager.CanAct())
        {
            
            selectAttack();
            if (myManager.stateManager[selectedAttack].cooldown > 0) { return; }
            for (int i = 0; i < myManager.stats.attacks.Count; i++)
            {
                if (myManager.stats.attacks[i] == selectedAttack)
                {
                    AttackAnimRpc(i);
                }
            }
           
            myManager.stateManager[selectedAttack].pressed = true;
            
        }

        if (rb && !myManager.CanMove()) { rb.isKinematic = false; }



    }

    public virtual void selectAttack()
    {
        Debug.Log("I have selected attacks dont I? " + myManager.stats.attacks.Count);
        selectedAttack = myManager.stats.attacks[0];
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public virtual void AttackAnimRpc(int attackIndex)
    {
        
        selectedAttack = myManager.stats.attacks[attackIndex];

        
        overrideController["DefaultStartUp"] = attackAnimations[attackIndex].startUp;
        
        overrideController["DefaultAttack"] = attackAnimations[attackIndex].attack;
        animator.SetFloat("AnimSpeed", attackAnimations[attackIndex].animationSpeed);
        animator.runtimeAnimatorController = overrideController;
        animator.SetBool("Walking", false);
        
        
    }
    public void AttackHold()
    {
        if (!myManager.CanMove() && selectedAttack)
        {
            releaseTimer += Time.deltaTime;
           
            if (releaseTimer > timeToHold)
            {
                
                myManager.stateManager[selectedAttack].pressed = false;
                releaseTimer = 0;
            }
        }
    }
    public void SetStartUpAnim(bool whatDo)
    {
      
        animator.SetBool("StartUp", whatDo);
    }
    public void SetAttackingAnim(bool whatDo)//bc the way network objects work these functions are directly connected to the ability manager (IN THE PREFAB BTW) events bc fmcl
    {
     
        animator.SetBool("Attacking", whatDo);
    }
    public void SetWalkingAnim(bool whatDo)
    {
        animator.SetBool("Walking", whatDo);
    }
}

[System.Serializable]
public class AttackAnimation
{
    public AnimationClip startUp;
    public AnimationClip attack;
    public float animationSpeed = 1;
}
