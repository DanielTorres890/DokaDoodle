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
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        //PlayerMoveManager.Instance.NextTurnRpc();
        if (Random.Range(1,2) == 1)  
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
