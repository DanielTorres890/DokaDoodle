using UnityEngine;

[CreateAssetMenu(fileName = "New Money Event", menuName = "WorldEvents/MonsterEvent")]
public class MonsterEvent : WorldEventBase
{
    public EnemyBase enemy;
    public int mapToSpawn;
    public int tileIdToSpawn;

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
        MapTileSpecialEvents.Instance.mapTiles[mapToSpawn][tileIdToSpawn].tileEnemy.Add(enemyspawn);
        if(mapToSpawn == PlayerMoveManager.Instance.mapNumber) { PlayerMoveManager.Instance.spawnEnemyOverworld(tileIdToSpawn, enemyspawn.enemyId); } 
        //maybe add another check but im p sure events should only occur on the overworld
    }

    public override void OnDeactivate()
    {
        
    }
}
