using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyCombat : EntityStats
{
    public int enemyId;

    public EnemyCombat(EnemyBase EnemyInfo) : base() 
    {
        foreach ( var temp in EnemyInfo.Stats)
        {
            base.stats[temp.attribute] = temp.value;
            enemyId = PlayerCombatManager.Instance.EnemyDataBase.GetId[EnemyInfo];
            base.name = EnemyInfo.enemyName;
        }

        List<AttackBase> attacks = new List<AttackBase>();
        foreach ( var attack in EnemyInfo.Attackss)
        {
            attacks.Add(attack);
        }

        base.attacks = new List<AttackBase>(attacks);
       
    
        base.defenses = EnemyInfo.Defendss;
    
    }
}
