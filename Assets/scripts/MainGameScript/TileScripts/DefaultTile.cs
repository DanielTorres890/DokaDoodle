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
        if (Random.Range(1,10) == 1)  
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
