using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DefaultTile : TileScript
{
    public EnemyBase[] enemies;
    
    public EventBase[] events;
    public override void TileEvent()
    {
        Debug.Log("WHO ARE U " + NetworkData.Instance.currentPlayer);
        Debug.Log("Who am I " + NetworkManager.Singleton.LocalClientId);
        if (!NetworkManager.Singleton.IsServer) { return; }
        //PlayerMoveManager.Instance.NextTurnRpc();
        if (Random.Range(1,4) == 5)  
        {
            int eventToSet = Random.Range(0,events.Length);
            ClientChecks.Instance.SyncEventRpc(eventToSet);

        }
        else
        {
            int enemyId = PlayerCombatManager.Instance.EnemyDataBase.GetId[enemies[Random.Range(0, enemies.Length)]];

            ClientChecks.Instance.SyncEnemyRpc(enemyId);
        }
    }
}
