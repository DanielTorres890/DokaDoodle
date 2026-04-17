using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DefaultTile : TileScript
{
    public EncounterWrapper[] enemies;
    
    public EventWrapper[] events;

    public ConditionalCombat[] conditionalCombats;

    private static bool forceEvent = false;
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

        bool noRealEnemies = true;
        foreach( var enemy in currentTile.tileEnemy)
        {
            if(!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any()) 
            {
                Debug.Log("There was a not matching tag...");
                noRealEnemies = false;
            }
        }
        if(noRealEnemies && !enemyAlly && !enemyPlayer)
        {
            ClientChecks.Instance.NoEnemiesToFightRpc(tileId);
        }
        else if ((Random.Range(1,11) == 1 && currentTile.tileEnemy.Count == 0 && !enemyAlly && !enemyPlayer) || forceEvent)  
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
            List<EncounterWrapper> combinedEncounter = new List<EncounterWrapper>();
            
            foreach(var encounter in enemies)
            {
                combinedEncounter.Add(encounter);
            }

            foreach(var encounter in conditionalCombats)
            {
                if(encounter.condition.CanBeginQuest()) 
                {
                    var wrapper = new EncounterWrapper();
                    wrapper.encounter = encounter.encounter;
                    wrapper.weight = encounter.weight;
                    combinedEncounter.Add(wrapper); 

                }
            }
            EnemyEncounter selectedEncounter = combinedEncounter[0].encounter;

            int totalWeight = 0;
            foreach (var weighted in combinedEncounter)
            {
                totalWeight += weighted.weight;
            }

            int randomWeight = Random.Range(0, totalWeight);
            int currentWeight = 0;
            foreach (var weighted in combinedEncounter)
            {
                currentWeight += weighted.weight;
                if (randomWeight < currentWeight)
                {
                    selectedEncounter = weighted.encounter;
                    break;
                }
             

            }
            int encounterId = PlayerCombatManager.Instance.EnemyEncounterDataBase.GetId[selectedEncounter];

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

[System.Serializable]
public class EncounterWrapper
{
    public EnemyEncounter encounter;
    public int weight = 25;
}

[System.Serializable]
public class ConditionalCombat
{
    public EnemyEncounter encounter;
    public QuestCondition condition;
    public int weight = 25;

}
