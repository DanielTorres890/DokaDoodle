using PrimeTween;
using UnityEngine;

public class ThwompingTreeLogic : BaseEnemyBehavior
{

    public float cooldownBetweenAttacks;
    [SerializeField]private float attackCooldownTimer;

    
    // Update is called once per frame
    public void Start()
    {

        attackCooldownTimer = cooldownBetweenAttacks;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        FindEnemy();

        if (targetManager)
        {
            var prevRotation = transform.rotation;
            transform.LookAt(targetManager.transform);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            Vector3 newTarget = transform.localEulerAngles;
            transform.rotation = prevRotation;
            Tween.LocalRotation(transform, newTarget, 1f);


        }
        transform.position = Vector3.zero;

    }
    public override void Update()
    {
        agent.enabled = false;
        if (myManager.stats.isDead) {  return; }
        
        attackCooldownTimer -= Time.deltaTime;
        if(attackCooldownTimer < 0)
        {
            AttackPlayer();
        }
        else if(selectedAttack)
        {
            myManager.stateManager[selectedAttack].pressed = false;
        }

    }
    public override void selectAttack()
    {
        selectedAttack = myManager.stats.attacks[Random.Range(0, myManager.stats.attacks.Count)];
    }
    public override void AttackPlayer()
    {
        if (!IsServer) { return; }
        if (myManager.CanAct())
        {
            Debug.Log("I AM ATTACKING");
            FindEnemy();

            if(targetManager)
            {
                var prevRotation = transform.rotation;
                transform.LookAt(targetManager.transform);
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
                Vector3 newTarget = transform.localEulerAngles;
                transform.rotation = prevRotation;
                Tween.LocalRotation(transform, newTarget, 1f);
                
                
            }
            


            selectAttack();
            attackCooldownTimer = cooldownBetweenAttacks + selectedAttack.startUp;
            if(selectedAttack is MultiBurst)
            {
                var cachedAtk = (selectedAttack as MultiBurst);
                attackCooldownTimer += cachedAtk.interval * cachedAtk.totalBursts;
            } 
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
}
