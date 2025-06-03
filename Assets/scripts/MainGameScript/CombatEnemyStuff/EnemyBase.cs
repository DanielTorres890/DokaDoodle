using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Combat System/Enemy")]
public class EnemyBase : ScriptableObject, ISerializationCallbackReceiver
{

    public string enemyName;
    public GameObject enemyPrefab;
    public GameObject enemyNonCombatPrefab;
    public ItemBuff[] Stats = new ItemBuff[7];
    public List<string> loyaltyTags;
    public AttackBase[] Attackss;
    public DefenseBase[] Defendss;
    public ItemBase[] DroppedItems;

   
    public int[] probability;
    public int droppedXp;
    public int droppedMoney;

    public void OnAfterDeserialize()
    {
        
    }

    public void OnBeforeSerialize()
    {
        
    }

    public int rollItem()
    {
        int drop = Random.Range(0,100);
        if (DroppedItems.Length > 0)
        {
            int dropTracker = 0;
            for (int i = 0; i < DroppedItems.Length; i++)
            {
                dropTracker += probability[i];
                if (dropTracker > drop)
                {
                    return i;
                }
            }
        }
        return -1;
    }

}
    

