using UnityEngine;

public class SmartDodgeBehavior : SmarterMeleeEnemy
{
    public float dashDuration;
    public float timeToChaseDash;
    public float dashCost;
    [Tooltip("Chance of dodging away after an attack")]
    public int dodgeAwayChance;
    private float timeToChaseDashTimer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsServer) { return; }
        myManager.onEndAttack.AddListener(DashBack);
    }

    public override void ChasePlayer()
    {

        if (!myManager.CanWalk()) { return; }

        timeToChaseDashTimer += Time.deltaTime;
        if (timeToChaseDashTimer > timeToChaseDash)
        {
            Dash(transform.forward);
            return;
        }

        base.ChasePlayer();

    }
    public void Dash(Vector3 direction)
    {
        timeToChaseDashTimer = 0;
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
            Dash(transform.TransformDirection(Vector3.back));
        }
    }
}
