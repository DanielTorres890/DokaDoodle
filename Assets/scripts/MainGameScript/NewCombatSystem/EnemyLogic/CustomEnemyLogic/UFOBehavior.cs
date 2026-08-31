using PrimeTween;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class UFOBehavior : BaseEnemyBehavior
{
    public float cooldownBetweenAttacks;
    public float cooldownBetweenLasers;
    [SerializeField] private float stateCooldown;

    

    public float Precision = 1f;
    public float MoveDistance;
    [Tooltip("How random will the UFOS movement be")]
    public float UFOMoveVariance;
    public float MinUFOHeight = 3;
    public float MaxUFOHeight;
    public UFOStates currentState;
    public int attackCount = 0;
   
    private UnityAction halfHealth;
    private Vector3 destination;
    private int previousAttack = -1;
    [SerializeField] private int dashesPerRest = 2;
    // Update is called once per frame
    public void Start()
    {

        stateCooldown = cooldownBetweenAttacks;
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
        if (!IsServer) { return; }
        agent.enabled = false;
        if (myManager.stats.isDead) { return; }

        stateCooldown -= Time.deltaTime;
        if (stateCooldown < 0)
        {
            
            if (currentState == UFOStates.None)
            {
                currentState = UFOStates.Attacking;
                stateCooldown = cooldownBetweenLasers;
                AttackPlayer();
                Debug.Log("Attack");
            }
            else if(currentState == UFOStates.Attacking)
            {
                

                currentState = UFOStates.Moving;
                stateCooldown = cooldownBetweenLasers;
                PickDestination();
                Debug.Log("Strafe");

            }
            else
            {
                stateCooldown = cooldownBetweenLasers;
                currentState = UFOStates.None;
                attackCount += 1;
                Debug.Log("Await Attack");
                //for now 2 is the amount of lasers it'll do before stopping
                if (attackCount > dashesPerRest)
                {
                    stateCooldown = cooldownBetweenAttacks;
                    currentState = UFOStates.Moving;
                    attackCount = -1;
                    GoToFloor();
                    Debug.Log("Break");
                }
               

            }
          
        }
        else if (selectedAttack)
        {
            myManager.stateManager[selectedAttack].pressed = false;
        }
        if(currentState == UFOStates.Moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, baseMoveSpeed * Time.deltaTime);
        }
        transform.LookAt(targetManager.transform);
    }
    public override void selectAttack()
    {
       
        int attackIndex = 0;

        
        previousAttack = attackIndex;
        selectedAttack = myManager.stats.attacks[attackIndex];
    }
    public override void AttackPlayer()
    {
        if (!IsServer) { return; }
        if (myManager.CanAct())
        {
            Debug.Log("I AM ATTACKING");
            FindEnemy();

            if (targetManager)
            {
                transform.LookAt(targetManager.transform);
            }



            selectAttack();
          
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
    public void PickDestination()
    {
        
        Quaternion NOJANK = gameObject.transform.rotation;
        SetAttackingAnim(false);
        SetStartUpAnim(false);
        SetWalkingAnim(true);
        agent.speed = myManager.stats.speedFormula() + baseMoveSpeed;
        gameObject.transform.LookAt(gameObject.transform.position + 
        new Vector3(Random.Range(-UFOMoveVariance, UFOMoveVariance),Mathf.Clamp(gameObject.transform.position.y + Random.Range(-UFOMoveVariance, UFOMoveVariance), MinUFOHeight, MaxUFOHeight), Random.Range(-UFOMoveVariance, UFOMoveVariance)));
        
        
        gameObject.transform.Rotate(Vector3.up, -Precision);
        bool freeme = true;
        while (freeme)
        {
            gameObject.transform.Rotate(Vector3.up, Precision);
            freeme = NavMesh.Raycast(gameObject.transform.position, gameObject.transform.TransformDirection(Vector3.forward) * MoveDistance, out NavMeshHit hit, NavMesh.AllAreas);
        }


        //agent.SetDestination(gameObject.transform.TransformDirection(Vector3.forward) * MoveDistance);
        destination = gameObject.transform.TransformDirection(Vector3.forward) * MoveDistance;
        destination = new Vector3(destination.x, Mathf.Clamp(destination.y, MinUFOHeight, MaxUFOHeight), destination.z);
        gameObject.transform.rotation = NOJANK;
    }
    private void GoToFloor()
    {
        destination = new Vector3(transform.position.x, MinUFOHeight, transform.position.z);
    }
    private void FaceTarget()
    {
        var prevRotation = transform.rotation;

        transform.LookAt(targetManager.transform);
        Vector3 newTarget = transform.localEulerAngles;
        transform.rotation = prevRotation;
        Tween.LocalRotation(transform, newTarget, 0.5f);
    }
}
public enum UFOStates
{
    None,
    Attacking,
    Moving,


}
