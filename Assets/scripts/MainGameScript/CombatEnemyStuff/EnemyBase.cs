using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Combat System/Enemy")]
public class EnemyBase : ScriptableObject
{

    public string enemyName;
    public GameObject enemyPrefab;
    public ItemBuff[] Stats = new ItemBuff[7];
    public AttackBase[] Attacks;
    public DefenseBase Defend;
    

}
    

