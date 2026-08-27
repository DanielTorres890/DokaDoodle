using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

public class ThwompingTreeLogic : BaseEnemyBehavior
{

    public float cooldownBetweenAttacks;
    [SerializeField]private float attackCooldownTimer;

    [Tooltip("What is considered close")]
    [SerializeField]private float closeProximity;


    private int attackCount = 0;
    private int poisonInterval = 5;
    private UnityAction halfHealth;
    private int appleCount;
    private int previousAttack = -1;
    // Update is called once per frame
    public void Start()
    {

        attackCooldownTimer = cooldownBetweenAttacks;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        FindEnemy();
        halfHealth = () =>
        {
            if (myManager.stats.stats[Attributes.Health] <= myManager.stats.stats[Attributes.MaxHealth] / 2)
            {
                cooldownBetweenAttacks /= 2f;
                myManager.onHit.RemoveListener(halfHealth);
            }
           
        };

        myManager.onHit.AddListener(halfHealth);
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
        if(!IsServer) { return; }
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
        int additionalWeight = 0;
        foreach(var combatant in NewCombatManager.instance.allCombatants)
        {
            if(combatant == myManager) { continue; }
            if(combatant.stats.isDead) { continue; }
            if(Vector3.Distance(combatant.transform.position, transform.position) > closeProximity) 
            {
                additionalWeight += 25 / (NewCombatManager.instance.allCombatants.Count - 1);
            }
            else
            {
                additionalWeight -= 25 / (NewCombatManager.instance.allCombatants.Count - 1);
            }

        }
        
        int attackIndex = -1;
      
        while(attackIndex == -1 || attackIndex == previousAttack)
        {

            int random = 0;
            if(additionalWeight > 0) { random = Random.Range(additionalWeight, 100); }
            else { random = Random.Range(0, 100 + additionalWeight); }


            if (random < 25) { attackIndex = 0; }
            else if (random < 50) { attackIndex = 1; }
            else if (random < 75) { attackIndex = 2; }
            else { attackIndex = 3; }

            if (attackCount % poisonInterval == 0) { attackIndex = 4; }

            if (appleCount > 0)
            {
                appleCount = -1;
                attackIndex = 2;
                attackCount -= 1;
                break;

            }
        }

   
        if (attackIndex == 2)
        {
            appleCount += 1;
        }

        previousAttack = attackIndex;
        selectedAttack = myManager.stats.attacks[attackIndex];
    }
    public override void AttackPlayer()
    {
        if (!IsServer) { return; }
        if (myManager.CanAct())
        {
          
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
                attackCooldownTimer += cachedAtk.interval * (cachedAtk.totalBursts - 1);
            } 
            for (int i = 0; i < myManager.stats.attacks.Count; i++)
            {
                if (myManager.stats.attacks[i] == selectedAttack)
                {
                    AttackAnimRpc(i);
                    attackCount++;
                }
            }

            myManager.stateManager[selectedAttack].pressed = true;

        }

        if (rb && !myManager.CanMove()) { rb.isKinematic = false; }
    }
}
