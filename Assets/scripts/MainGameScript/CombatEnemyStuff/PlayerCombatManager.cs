using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCombatManager : MonoBehaviour
{

    public static PlayerCombatManager Instance;
    public EnemyDataBase EnemyDataBase;
    public EnemyEncounterDataBase EnemyEncounterDataBase;

    public List<EntityStats> combatants = new List<EntityStats>();
    public EnemyEncounter currentEncounter;
    public EntityStats combatant1;//LEGACY STUFF RIGHT HERE
    public EntityStats combatant2;
    
    private void Awake()
    {
        if (Instance != null) 
        {
            Destroy(gameObject);
            return; }
        Instance = this;
    }
    
    public string BattleSetUp(int encounterId)
    {
        PlayerCombatManager.Instance.combatants.Clear();
        PlayerCombatManager.Instance.combatants.Add(NetworkData.Instance.players[NetworkData.Instance.currentPlayer]);
        string encounterName = "";
        PlayerCombatManager.Instance.currentEncounter = PlayerCombatManager.Instance.EnemyEncounterDataBase.GetItem[encounterId];

        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].setCombatActions();

        var currentTile = MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId];
        bool rumble = false; //is there another player that we fight
       
        foreach (var players in currentTile.players)
        {
            if (players != NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber && PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].canFight)
            {
                Debug.Log("i set this player up " +  players);
                PlayerCombatManager.Instance.combatants.Add(NetworkData.Instance.players[players]);
                NetworkData.Instance.players[players].setCombatActions();
                rumble = true;
                encounterName = NetworkData.Instance.players[players].name;
            }

        }

        //Pretty much everything that isn't these two is stuff from the old system
        List<EntityStats> potentialEnemies = new List<EntityStats>(currentTile.tileEnemy);

        foreach(var enemy in currentTile.tileEnemy)
        {
            enemy.ResetMyAttacks();
        }


        foreach(var ally in currentTile.partyMembers)
        {
            if(ally.allyOwner == NetworkData.Instance.currentPlayer) { continue; }

            potentialEnemies.Add(ally);
        }

        if (potentialEnemies.Count == 0)
        {
            if (!rumble)
            {
                foreach (var enemy in PlayerCombatManager.Instance.EnemyEncounterDataBase.GetItem[encounterId].enemies)
                {

                    var temp = new EnemyCombat(enemy);
                    encounterName = PlayerCombatManager.Instance.EnemyEncounterDataBase.GetItem[encounterId].EncounterName;
                    PlayerCombatManager.Instance.combatants.Add(temp);
                    currentTile.tileEnemy.Add(temp);
                    

                }

            }


        }
        else
        {
            

        }
        foreach (var enemyy in potentialEnemies)
        {
            PlayerCombatManager.Instance.combatants.Add(enemyy);
            encounterName = enemyy.name;
        }
        if (potentialEnemies.Count > 1)
        {
            encounterName = "More than 1 guy";
        }

        foreach (var ally in currentTile.partyMembers)
        {
            if (ally.allyOwner != NetworkData.Instance.currentPlayer) { continue; }
            PlayerCombatManager.Instance.combatants.Add(ally);
        }

       
        return encounterName;
    }
}
