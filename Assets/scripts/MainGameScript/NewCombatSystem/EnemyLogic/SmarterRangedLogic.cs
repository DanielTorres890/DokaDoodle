using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class SmarterRangedLogic : RangedEnemyBehavior
{
    [Tooltip("This is the distance between an attacks effective range (like a fast attack has longer effective range) and how far they should kite")]
    public int KiteAttackDelta;
    public float minimumRange = 5f;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsHost)
        {
            CalculateAttackRanges();
            myManager.onAttack.AddListener(SetNextClosestRange);

        }



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
            bestScore += (bestAttack as MDefault).speed * Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position) / 10;
        }

        foreach (var key in myManager.stats.attacks)
        {
            counter++;
            if (myManager.stateManager[key].cooldown > 0) { continue; }

           
            if (Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position) > attackRanges[counter]) { continue; }
            if (myManager.stateManager[bestAttack].cooldown > 0) { bestAttack = key; }
            

            float thisScore = 0;
            thisScore += CalculateDamage(key, myManager.stats) * Mathf.Clamp(10 - Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position),0, 1000000f);

            Debug.Log("How much was damage favored " +  thisScore);
            thisScore += (key.ablitySize.x + key.ablitySize.y + key.ablitySize.z) * 3;
            if (key is MDefault)
            {
                float speedCalc = (key as MDefault).speed * Vector3.Distance(gameObject.transform.position, targetManager.gameObject.transform.position) / 10;
                thisScore += Mathf.Clamp(speedCalc,0,100000) * 2;
            }
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
       
        kiteRange = attackRanges[bestAttackIndex] - 1;
        StopKitingDistance = kiteRange + KiteAttackDelta;
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
        if(myManager.stats.attacks.Count == 0) { return; }

        attackRanges = new float[myManager.stats.attacks.Count];
        
        for(int i = 0; i < attackRanges.Length; i++)
        {
            attackRanges[i] = (myManager.stats.attacks[i].ablitySize.x + myManager.stats.attacks[i].ablitySize.y + myManager.stats.attacks[i].ablitySize.z) / 5;
            if (myManager.stats.attacks[i] is MDefault)
            {
                MDefault rangedAtk = (myManager.stats.attacks[i] as MDefault);
                //i needed something such that lifespan does matter for early values but loses value quickly over time im not trying to be cringe by using log
                attackRanges[i] += rangedAtk.speed * Mathf.Log(rangedAtk.lifespan / 3);
            }
            if (myManager.stats.attacks[i] is BuffAbility)
            {
                attackRanges[i] = 100f;
            }
            attackRanges[i] = Mathf.Clamp(attackRanges[i], minimumRange, 10000);
        }
        SetNextClosestRange();
    }
    private void SetNextClosestRange()
    {
        int closestRangeIndex = 0;
        for (int i = 0; i < attackRanges.Length; i++)
        {
            if(myManager.stateManager[myManager.stats.attacks[i]].cooldown > 0) { continue; }

            if (attackRanges[i] > attackRanges[closestRangeIndex] ) { continue; }

            closestRangeIndex = i;
        }

        kiteRange = attackRanges[closestRangeIndex] - 1;
        StopKitingDistance = kiteRange + KiteAttackDelta;
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
