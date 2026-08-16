using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Money Event", menuName = "WorldEvents/TimedMonsterEvent")]
public class TimedMonsterEvent : TimedWEvent
{
    public EnemyBase enemy;
    public int mapToSpawn;
    public int tileIdToSpawn;
    public int fameToAward;
    public override void OnActivate(int randomNum)
    {
        var enemyspawn = new EnemyCombat(enemy);
        enemyspawn.persistant = true;
        List<EnemyCombat> enemies = new List<EnemyCombat>
        {
            enemyspawn
        };

        if (MapTileSpecialEvents.Instance.mapTiles[mapToSpawn] != null)
        {
            MapTileSpecialEvents.Instance.mapTiles[mapToSpawn][tileIdToSpawn].tileEnemy.Add(enemyspawn);
            if (mapToSpawn == PlayerMoveManager.Instance.mapNumber) { PlayerMoveManager.Instance.spawnEnemyOverworld(tileIdToSpawn, enemies); }

        }

        base.OnActivate(randomNum);
        //maybe add another check but im p sure events should only occur on the overworld
    }

    public override void OnDeactivate()
    {
        if (MapTileSpecialEvents.Instance.mapTiles[mapToSpawn] != null)
        {
            var tileinfo = MapTileSpecialEvents.Instance.mapTiles[mapToSpawn][tileIdToSpawn].tileEnemy;
            for (int i = 0; i < tileinfo.Count; i++)
            {
                if (tileinfo[i].enemyId == PlayerCombatManager.Instance.EnemyDataBase.GetId[enemy])
                {
                    tileinfo.RemoveAt(i);
                    break;
                }
            }
        }
        foreach (var player in NetworkData.Instance.players)
        {
            if (player.curMap == mapToSpawn && player.curTileId == tileIdToSpawn)
            {
                player.playerInfo[PlayerInfo.fame] += fameToAward;
            }
        }
        base.OnDeactivate();
    }
    public override bool Condition(int turns)
    {
        bool outOfTime = base.Condition(turns);
        if (outOfTime) { return true; }


        bool enemyDefeated = true;
        var tileinfo = MapTileSpecialEvents.Instance.mapTiles[mapToSpawn][tileIdToSpawn].tileEnemy;
        for (int i = 0; i < tileinfo.Count; i++)
        {
            if (tileinfo[i].enemyId == PlayerCombatManager.Instance.EnemyDataBase.GetId[enemy])
            {
                enemyDefeated = false;
                break;
            }
        }


        return enemyDefeated;
    }
}
