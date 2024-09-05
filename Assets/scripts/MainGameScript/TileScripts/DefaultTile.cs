using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DefaultTile : TileScript
{
    public EnemyBase[] enemies;
    
    public override void TileEvent()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        //PlayerMoveManager.Instance.NextTurnRpc();
        int enemyId = PlayerCombatManager.Instance.EnemyDataBase.GetId[enemies[Random.Range(0, enemies.Length)]];

        ClientChecks.Instance.SyncEnemyRpc(enemyId);
    }
}
