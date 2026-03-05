using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyCombat : EntityStats
{
    public int enemyId;
    public bool persistant = false; //basically if the enemy should stay on the tile even if a fight ends with their victory

    public EnemyCombat()
    {
        
    }
    public EnemyCombat(EnemyBase EnemyInfo) : base() 
    {
        foreach ( var temp in EnemyInfo.Stats)
        {
            base.stats[temp.attribute] = temp.value;
            enemyId = PlayerCombatManager.Instance.EnemyDataBase.GetId[EnemyInfo];
            base.name = EnemyInfo.enemyName;
        }

       
       
        base.loyaltyTags = new List<string>(EnemyInfo.loyaltyTags);
        ResetMyAttacks();
        base.defenses = EnemyInfo.Defendss;
    
    }
    public void ResetMyAttacks()
    {
        
        List<AttackBase> attacks = new List<AttackBase>();
        foreach (var attack in PlayerCombatManager.Instance.EnemyDataBase.GetItem[enemyId].Attackss)
        {
            attacks.Add(attack);
        }
        Debug.Log("Ive set my attacks " + attacks.Count);
        base.attacks = new List<AttackBase>(attacks);
    }
}
