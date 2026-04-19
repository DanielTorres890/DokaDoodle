using UnityEngine;

public class SmarterMeleeEnemy : BaseEnemyBehavior
{
    public float minimumRange = 0f;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

       
        CalculateAttackRanges();
        
        
        
    }

    //theres a lot going on but the priorities of this is the the speed and size of your attack add the same proprtional value
    //but the closer an enemy is the higher weight damage will have
    public override void selectAttack()
    {
        AttackBase bestAttack = myManager.stats.attacks[0];
        float bestScore = CalculateDamage(bestAttack,myManager.stats) * (10 - Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position));
        bestScore += (bestAttack.ablitySize.x + bestAttack.ablitySize.y + bestAttack.ablitySize.z) * 3;
        int bestAttackIndex = 0;
        int counter = -1;
        
        if(bestAttack is MDefault)
        {
            bestScore += (bestAttack as MDefault).speed * 2;
        }

        foreach (var key in myManager.stats.attacks)
        {
            counter++;
            if (myManager.stateManager[key].cooldown > 0) { continue; }

           
            if (Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position) > attackRanges[counter]) { continue; }
            if (myManager.stateManager[bestAttack].cooldown > 0) { bestAttack = key; }
            

            float thisScore = 0;
            thisScore += CalculateDamage(key, myManager.stats) * Mathf.Clamp(10 - Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position),0, 1000000f) / (key.startUp + key.endLag);


            thisScore += (key.ablitySize.x + key.ablitySize.y + key.ablitySize.z) * 3;
            if(key is BuffAbility)
            {
                thisScore = 10000;
            }



            if(thisScore > bestScore)
            {
                bestAttackIndex = counter;
                bestScore = thisScore;
                bestAttack = key;
            }
            Debug.Log("The weight of this attack: " + key.attackName + " was " + thisScore);
            
        }
       
   
        selectedAttack = bestAttack;
    }
    public override bool InRange()
    {
        if(attackRanges.Length == 0) { CalculateAttackRanges(); }
        for(int i = 0; i < attackRanges.Length; i++)
        {
            if (myManager.stateManager[myManager.stats.attacks[i]].cooldown > 0) { continue; }

            if (Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position) < attackRanges[i])
            {
                return true;
            }
        }
        return false;
    }
    private void CalculateAttackRanges()
    {
        if(myManager.stats.attacks.Count <= 0) { return; }


        attackRanges = new float[myManager.stats.attacks.Count];
        
        for(int i = 0; i < attackRanges.Length; i++)
        {
            
         
            attackRanges[i] = (myManager.stats.attacks[i].ablitySize.z / 2) + myManager.stats.attacks[i].offset.z - 1;
            if (myManager.stats.attacks[i] is MDefault)
            {
                MDefault rangedAtk = (myManager.stats.attacks[i] as MDefault);
                attackRanges[i] += rangedAtk.speed * (rangedAtk.lifespan / 3);
            }
            if (myManager.stats.attacks[i] is BuffAbility)
            {
                attackRanges[i] = 100f;
            }
            attackRanges[i] = Mathf.Max(attackRanges[i], minimumRange);
        }

    }
    

    private int CalculateDamage(AttackBase attack, EntityStats attacker)
    {
        float damage = 0;
        foreach(var attribute in attack.multipliers)
        {
            damage += attacker.stats[attribute.attribute] * attribute.mult;
        }
        return Mathf.RoundToInt(damage);
    }
}
