using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DefaultTile : TileScript
{
    public EnemyEncounter[] enemies;
    
    public EventWrapper[] events;

    private static bool forceEvent = true;
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
        bool enemyPlayer = false;
        foreach( var enemy in currentTile.players)
        {
            if(enemy != NetworkData.Instance.currentPlayer) { enemyPlayer = true; break;}
        }
        
        if ((Random.Range(1,11) == 1 && currentTile.tileEnemy.Count == 0 && !enemyAlly && !enemyPlayer) || forceEvent)  
        {
            EventBase selectedEvent = events[0].tileEvent;
            int totalWeight = 0;
            foreach (var weighted in events)
            {
                totalWeight += weighted.weight;
            }

            int randomWeight = Random.Range(0, totalWeight);
            int currentWeight = 0;
            int eventIndex = 0;
            foreach (var weighted in events)
            {
                currentWeight += weighted.weight;
                if (randomWeight < currentWeight)
                {
                    selectedEvent = weighted.tileEvent;
                    break;
                }
                eventIndex += 1;

            }

            
            ClientChecks.Instance.SyncEventRpc(eventIndex);

        }
        else
        {

            int encounterId = PlayerCombatManager.Instance.EnemyEncounterDataBase.GetId[enemies[Random.Range(0, enemies.Length)]];

            ClientChecks.Instance.SyncEnemyRpc(encounterId);
        }
    }
}

[System.Serializable]
public class EventWrapper
{
    public EventBase tileEvent;
    public int weight;
}
