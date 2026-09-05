using UnityEngine;

public class SmartDodgeBehavior : SmarterMeleeEnemy
{
    public float dashDuration;
    public float timeToChaseDash;
    public float dashCost;
    [Tooltip("Chance of dodging away after an attack")]
    public int dodgeAwayChance;
    [Tooltip("An additional random modifier when dodging")]
    public float dodgeVariance;
    private float timeToChaseDashTimer;
    private bool recentlyDashed = false;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsServer) { return; }
        myManager.onEndAttack.AddListener(delegate {
            
            if(myManager.combatantstate != combatantStates.Free || recentlyDashed) { return; }
            DashBack();
        });
    }

    public override void ChasePlayer()
    {

        if (!myManager.CanWalk()) { return; }

        timeToChaseDashTimer += Time.deltaTime;
        if (timeToChaseDashTimer > timeToChaseDash)
        {
            //var prevRotation = transform.rotation;
            //transform.Rotate(Vector3.up * Random.Range(-dodgeVariance, dodgeVariance));
            Dash(transform.forward);
            //transform.rotation = prevRotation;
            return;
        }

        base.ChasePlayer();

    }
    public override void AttackPlayer()
    {
        recentlyDashed = false;
        timeToChaseDashTimer = 0;
        base.AttackPlayer();
    }
    public void Dash(Vector3 direction)
    {

        timeToChaseDashTimer = 0;
        recentlyDashed = true;
        if (myManager.currentEnergy < dashCost) { return; }

        
        myManager.currentEnergy -= dashCost;
        myManager.combatantstate = combatantStates.Dashing;
        agent.enabled = false;
        rb.isKinematic = false;
        rb.AddForce(direction * myManager.stats.dashFormula(), ForceMode.Impulse);
        myManager.stateDuration = dashDuration;
    }
    public void DashBack()
    {
     
        if(Random.Range(0,101) < dodgeAwayChance)
        {
            var prevRotation = transform.rotation;
            transform.Rotate(Vector3.up * Random.Range(-dodgeVariance, dodgeVariance));
            Dash(transform.TransformDirection(Vector3.back));
            transform.rotation = prevRotation;
        }
    }

}
