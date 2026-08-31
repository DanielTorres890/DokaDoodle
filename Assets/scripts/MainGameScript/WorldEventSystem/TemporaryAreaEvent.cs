using UnityEngine;

[CreateAssetMenu(fileName = "New Area Event", menuName = "WorldEvents/Temp Area Event")]
public class TemporaryAreaEvent : WorldEventBase
{
    public int eventDuration;
    public int mapId;
    public int tileId;

    public EnemyBase bossEnemy;
    public int bossTileId;
    public override void OnActivate(int randomNum)
    {

        int playerToSend = randomNum % NetworkData.Instance.players.Count;
        if (MapTileSpecialEvents.Instance.mapTiles[mapId] != null)
        {
            bool hasBoss = false;
            var tileEnemies = MapTileSpecialEvents.Instance.mapTiles[mapId][bossTileId].tileEnemy;
            for (int i = 0; i < tileEnemies.Count; i++)
            {
                if (tileEnemies[i].enemyId == PlayerCombatManager.Instance.EnemyDataBase.GetId[bossEnemy])
                {
                    hasBoss = true;
                    break;
                }
            }

            if (!hasBoss)
            {
                var enemystats = new EnemyCombat(bossEnemy);
                enemystats.persistant = true;
                tileEnemies.Add(enemystats);

            }
        }


        NetworkData.Instance.players[playerToSend].TeleportPlayer(mapId, tileId);
  
        base.OnActivate(randomNum);
    }
    public override bool Condition(int turns)
    {
        return eventDuration <= turns;
    }
    public override void OnDeactivate()
    {
        Debug.Log("did i deactivate? ");
        foreach (var player in NetworkData.Instance.players)
        {
            if (player.curMap == mapId)
            {
                player.curMap = 0;
                player.curTileId = 0;

            }
        }
        base.OnDeactivate();
    }

}
