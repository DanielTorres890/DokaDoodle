using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Burst.Intrinsics.X86.Avx;

public class PlayerCombatManager : MonoBehaviour
{

    public static PlayerCombatManager Instance;
    public EnemyDataBase EnemyDataBase;
    public EnemyEncounterDataBase EnemyEncounterDataBase;

    public List<EntityStats> combatants = new List<EntityStats>();
    public EnemyEncounter currentEncounter;
    public EntityStats combatant1;//LEGACY STUFF RIGHT HERE
    public EntityStats combatant2;

    public bool isRaid;
    public float spawnRadius;//this is a weird way to get information to newcombnatmanager
    private void Awake()
    {
       
        Instance = this;
    }

    public string BattleSetUp(int encounterId)
    {
        Debug.Log("Did i set up the battle??");
        var currentPlayer = NetworkData.Instance.players[NetworkData.Instance.currentPlayer];
        var currentTile = MapTileSpecialEvents.Instance.mapTiles[currentPlayer.curMap][currentPlayer.curTileId];
        currentPlayer.setCombatActions();


        PlayerCombatManager.Instance.combatants.Clear();
        PlayerCombatManager.Instance.combatants.Add(currentPlayer);


        string encounterName = "";
        PlayerCombatManager.Instance.currentEncounter = PlayerCombatManager.Instance.EnemyEncounterDataBase.GetItem[encounterId];


        bool rumble = false; //is there another player that we fight

        isRaid = PlayerCombatManager.Instance.EnemyEncounterDataBase.GetItem[encounterId].isRaid;

        
        foreach (var players in NetworkData.Instance.players)
        {
            
            if(players.curMap != currentPlayer.curMap || players.curTileId != currentPlayer.curTileId) { continue; }

            if(currentPlayer == players) { continue; }

            if ((PlayerMoveManager.Instance.mapTiles[currentPlayer.curTileId].canFight || isRaid))
            {

                Debug.Log("added player " + players.name);
                PlayerCombatManager.Instance.combatants.Add(players);
                players.setCombatActions();
                rumble = true;
                encounterName = players.name;
            }

        }


        //Pretty much everything that isn't these two is stuff from the old system
        List<EntityStats> potentialEnemies = new List<EntityStats>(currentTile.tileEnemy);

        foreach (var enemy in currentTile.tileEnemy)
        {
            enemy.ResetMyAttacks();
        }


        foreach (var ally in currentTile.partyMembers)
        {
            Debug.Log("Looked at this ally");
            if (ally.allyOwner == NetworkData.Instance.currentPlayer) { continue; }
            Debug.Log("Added this ally to combat" + ally.name);
            potentialEnemies.Add(ally);
            ally.setCombatActions();
        }

        if (potentialEnemies.Count == 0 || isRaid)
        {
            if (!rumble || isRaid)
            {
                foreach (var enemy in PlayerCombatManager.Instance.EnemyEncounterDataBase.GetItem[encounterId].enemies)
                {
                    Debug.Log("enemy " + enemy.enemyName + "has been added to combat ");
                    var temp = new EnemyCombat(enemy);
                    encounterName = PlayerCombatManager.Instance.EnemyEncounterDataBase.GetItem[encounterId].EncounterName;
                    PlayerCombatManager.Instance.combatants.Add(temp);
                    currentTile.tileEnemy.Add(temp);
                    if(isRaid)
                    {
                        temp.stats[Attributes.Health] *= Mathf.Clamp(Mathf.CeilToInt(NetworkData.Instance.players.Count / 2f), 1, 2);
                        temp.stats[Attributes.MaxHealth] *= Mathf.Clamp(Mathf.CeilToInt(NetworkData.Instance.players.Count / 2f), 1, 2);
                        temp.stats[Attributes.Attack] *= Mathf.Clamp(Mathf.CeilToInt(NetworkData.Instance.players.Count / 2f), 1, 2);
                        temp.stats[Attributes.Magic] *= Mathf.Clamp(Mathf.CeilToInt(NetworkData.Instance.players.Count / 2f), 1, 2);

                    }


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

        if (potentialEnemies.Count > 0 && potentialEnemies[potentialEnemies.Count - 1] is PartyMember)
        {
            PartyMember lastEntity = potentialEnemies[potentialEnemies.Count - 1] as PartyMember;
            if(potentialEnemies.Contains(NetworkData.Instance.players[lastEntity.allyOwner]))
            {
                encounterName = NetworkData.Instance.players[lastEntity.allyOwner].name;
            }
        }

        if (potentialEnemies.Count > 1)
        {
            EntityStats strongest = potentialEnemies[0];
            foreach(var enemy in potentialEnemies)
            {
                if (strongest.stats[Attributes.MaxHealth] > enemy.stats[Attributes.MaxHealth])
                {
                    strongest = enemy;
                }
            }

            encounterName = strongest.name;
        }

        foreach (var ally in currentTile.partyMembers)
        {
            Debug.Log("Looking at this ally " + ally.name);
            if (ally.allyOwner != NetworkData.Instance.currentPlayer) { continue; }

            Debug.Log("Added this ally to combat" + ally.name);
            PlayerCombatManager.Instance.combatants.Add(ally);
            ally.setCombatActions();
        }

       foreach(var combatant in PlayerCombatManager.Instance.combatants)
       {
            if(combatant is not EnemyCombat) { continue; }
            if(NetworkData.Instance.seenEnemies.Contains((combatant as EnemyCombat).enemyId)) { continue; }

            NetworkData.Instance.seenEnemies.Add((combatant as EnemyCombat).enemyId);

       }


        return encounterName;
    }
}
