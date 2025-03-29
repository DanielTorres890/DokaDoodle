using UnityEngine;

public class RangedEnemyBehavior : BaseEnemyBehavior
{
    public float kiteRange;
    public bool inKiteRange;

    

    public override void Update()
    {
        
        

        inKiteRange = Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position) < kiteRange;

        if(inKiteRange) 
        { 
            KiteAway();
            return;
        }
        base.Update();
    }

    
    public override void selectAttack()
    {
        foreach (var key in myManager.stats.attacks)
        {
            
            if (myManager.stateManager[key].cooldown > 0) { continue; }

            if (key.attackPrefab.TryGetComponent(out RangedAbility ranged))
            {
                selectedAttack = key;
            }
            
            
        }
    }
    
    public void KiteAway()
    {
        Vector3 direction = targetManager.gameObject.transform.position - transform.position;
        Vector3 oppositeDirection = transform.position - direction;

        agent.SetDestination(oppositeDirection);
    }
}
