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

    public string enemyLevel = "1";
    public int droppedXp;
    public int droppedMoney;
    public int droppedFame;
    public bool scalingMoney = false;

    [Tooltip("If true an enemy wont persist if combat ends early")]
    public bool flee = false;


    [Tooltip("If the enemy should override the bgm insert this")]
    public AudioClip SpecialMusic;
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
    public int GetDroppedMoney()
    {
        if (scalingMoney) { return droppedMoney * (WorldEventManager.Instance.weeks + 1); } else { return droppedMoney; }
    
    }
}
    

