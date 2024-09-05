using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Database", menuName = "Combat System/EnemyDataBase")]
public class EnemyDataBase : ScriptableObject, ISerializationCallbackReceiver
{
    public EnemyBase[] Enemies;

    public Dictionary<EnemyBase, int> GetId = new Dictionary<EnemyBase, int>();
    public Dictionary<int, EnemyBase> GetEnemies = new Dictionary<int, EnemyBase>();

    //

    public void OnAfterDeserialize()
    {
        GetId = new Dictionary<EnemyBase, int>();
        GetEnemies = new Dictionary<int, EnemyBase>();
        for (int i = 0; i < Enemies.Length; i++)
        {
            GetId.Add(Enemies[i], i);
            GetEnemies.Add(i, Enemies[i]);
        }

    }

    public void OnBeforeSerialize()
    {

    }
}
