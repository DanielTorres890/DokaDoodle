using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Money Event", menuName = "WorldEvents/MonsterEvent")]
public class MonsterEvent : WorldEventBase
{
    public EnemyBase enemy;
    public int mapToSpawn;
    public int tileIdToSpawn;
    public int fameToAward;
    public override bool Condition(int turns)
    {
       foreach (var enemys in MapTileSpecialEvents.Instance.mapTiles[mapToSpawn][tileIdToSpawn].tileEnemy)
        {
            if (enemys.enemyId == PlayerCombatManager.Instance.EnemyDataBase.GetId[enemy])
            {
                return false;
            }
        }
        return true;
    }

    public override void OnActivate()
    {
        var enemyspawn = new EnemyCombat(enemy);
        enemyspawn.persistant = true;
        List<EnemyCombat> enemies = new List<EnemyCombat>
        {
            enemyspawn
        };

        MapTileSpecialEvents.Instance.mapTiles[mapToSpawn][tileIdToSpawn].tileEnemy.Add(enemyspawn);
        if(mapToSpawn == PlayerMoveManager.Instance.mapNumber) { PlayerMoveManager.Instance.spawnEnemyOverworld(tileIdToSpawn, enemies); }
        base.OnActivate();
        //maybe add another check but im p sure events should only occur on the overworld
    }

    public override void OnDeactivate()
    {
        foreach(var player in NetworkData.Instance.players)
        {
            if(player.curMap == mapToSpawn && player.curTileId == tileIdToSpawn)
            {
                player.playerInfo[PlayerInfo.fame] += fameToAward;
            }
        }
        base.OnDeactivate();
    }
}
