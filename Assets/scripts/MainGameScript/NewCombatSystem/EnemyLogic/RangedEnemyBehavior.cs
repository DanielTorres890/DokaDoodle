using UnityEngine;
using UnityEngine.AI;

public class RangedEnemyBehavior : BaseEnemyBehavior
{
    public float kiteRange;
    public float kiteAmount;

    [Tooltip("How much it'll look around to find an open area \nthink of it like 1 meaning itll turn 1 degree until it finds an open area to run to")]
    public float Precision = 1f;
    public float StopKitingDistance;

    
    private int failedEscapeCounter;

    [Tooltip("How many times the enemy will kite attempt to kite away before attacking anyways")]
    public int failedEscapeAmount;


    public bool inKiteRange;

    public bool avoiding;

    public override void Update()
    {

        if (myManager.CanWalk() && targetManager != null && !myManager.stats.isDead)
        {
            
            kiteFinish();

            if (avoiding) { return; }

            inKiteRange = Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position) < kiteRange;

            if (inKiteRange && failedEscapeCounter < failedEscapeAmount)
            {
                KiteAway();
                return;
            }
            failedEscapeCounter = 0;
        }
        
        
        base.Update();
    }

    
    public override void selectAttack()
    {
        selectedAttack = myManager.stats.attacks[0];
        foreach (var key in myManager.stats.attacks)
        {
            
            if (myManager.stateManager[key].cooldown > 0) { continue; }

        
            selectedAttack = key;
            
            
            
        }
    }
    
    public void KiteAway()
    {
        agent.enabled = true;
        Quaternion NOJANK = gameObject.transform.rotation;
        SetAttackingAnim(false);
        SetStartUpAnim(false);
        SetWalkingAnim(true);
        agent.speed = myManager.stats.speedFormula() + baseMoveSpeed;
        gameObject.transform.LookAt(targetManager.gameObject.transform.position);
        gameObject.transform.Rotate(Vector3.up, -Precision);
        bool freeme = true;
        while (freeme)
        {
            gameObject.transform.Rotate(Vector3.up, Precision);
            freeme = NavMesh.Raycast(gameObject.transform.position, gameObject.transform.TransformDirection(Vector3.back) * kiteAmount, out NavMeshHit hit, NavMesh.AllAreas);
        }

        Debug.Log("RUN AWAYYYY");
        agent.SetDestination(gameObject.transform.TransformDirection(Vector3.back) * kiteAmount);
        avoiding = true;
        gameObject.transform.rotation = NOJANK;
    }

    private void kiteFinish()
    {
        if (avoiding && Vector3.Distance(gameObject.transform.position, agent.destination) < StopKitingDistance)
        {

            failedEscapeCounter += 1;
            avoiding = false;
           
        }
    }
}
