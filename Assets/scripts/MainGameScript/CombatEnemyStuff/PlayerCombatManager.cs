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
        if (Instance != null) { return; }
        Instance = this;
    }
    
    public string BattleSetUp(int encounterId)
    {
        PlayerCombatManager.Instance.combatants.Clear();
        PlayerCombatManager.Instance.combatants.Add(NetworkData.Instance.players[NetworkData.Instance.currentPlayer]);
        string encounterName = PlayerCombatManager.Instance.EnemyEncounterDataBase.GetItem[encounterId].EncounterName;
        PlayerCombatManager.Instance.currentEncounter = PlayerCombatManager.Instance.EnemyEncounterDataBase.GetItem[encounterId];
        Debug.Log("Step 1 before it was set up");
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].setCombatActions();
        Debug.Log("Step 2 it SHOULD SET UP HERE ");

        bool rumble = false; //is there another player that we fight
       
        foreach (var players in MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players)
        {
            if (players != NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber && PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].canFight)
            {
                
                PlayerCombatManager.Instance.combatants.Add(NetworkData.Instance.players[players]);
                NetworkData.Instance.players[players].setCombatActions();
                rumble = true;
                encounterName = NetworkData.Instance.players[players].name;
            }

        }

        //Pretty much everything that isn't these two is stuff from the old system

        if (MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.Count == 0)
        {
            if (!rumble)
            {
                foreach (var enemy in PlayerCombatManager.Instance.EnemyEncounterDataBase.GetItem[encounterId].enemies)
                {

                    var temp = new EnemyCombat(enemy);
                    PlayerCombatManager.Instance.combatants.Add(temp);
                    MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.Add(temp);
                    

                }

            }


        }
        else
        {
            foreach (var enemyy in MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy)
            {
                PlayerCombatManager.Instance.combatants.Add(enemyy);
                encounterName = enemyy.name;
            }

        }
        return encounterName;
    }
}
