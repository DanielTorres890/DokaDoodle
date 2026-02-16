using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DefaultTile : TileScript
{
    public EnemyEncounter[] enemies;
    
    public EventBase[] events;
    
    public override void TileEvent()
    {
        
        if (!NetworkManager.Singleton.IsServer) { return; }
        //PlayerMoveManager.Instance.NextTurnRpc();

        var currentTile = MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.GetCurrentPlayer().curMap][NetworkData.Instance.GetCurrentPlayer().curTileId];

        
        bool enemyAlly = false;
        foreach( var enemy in currentTile.partyMembers )
        {
            if(enemy.allyOwner == NetworkData.Instance.GetCurrentPlayer().playerNumber) { enemyAlly = true; break;}
        }
        
        
        if (Random.Range(1,11) == 1 && currentTile.tileEnemy.Count == 0 && !enemyAlly)  
        {
            int eventToSet = Random.Range(0,events.Length);
            ClientChecks.Instance.SyncEventRpc(eventToSet);

        }
        else
        {

            int encounterId = PlayerCombatManager.Instance.EnemyEncounterDataBase.GetId[enemies[Random.Range(0, enemies.Length)]];

            ClientChecks.Instance.SyncEnemyRpc(encounterId);
        }
    }
}
