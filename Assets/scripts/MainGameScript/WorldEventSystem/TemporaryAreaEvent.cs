using UnityEngine;

[CreateAssetMenu(fileName = "New Area Event", menuName = "WorldEvents/Temp Area Event")]
public class TemporaryAreaEvent : WorldEventBase
{
    public int eventDuration;
    public int mapId;
    public int tileId;

    [Tooltip("There doesn't have to be a boss to work")]
    public EnemyBase bossEnemy;
    public int bossTileId;

    public bool teleportAllPlayers = false;


    [Tooltip("Should the players be sent back? ")]
    public bool returnPlayer = true;
    public int mapToReturn;
    public int tileToReturn;
    
    public override void OnActivate(int randomNum)
    {

        int playerToSend = randomNum % NetworkData.Instance.players.Count;
        if (MapTileSpecialEvents.Instance.mapTiles[mapId] != null && bossEnemy)
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

        if(!teleportAllPlayers)
        NetworkData.Instance.players[playerToSend].TeleportPlayer(mapId, tileId);
        else
        {
       
            foreach(var player in NetworkData.Instance.players)
            {
                player.TeleportPlayer(mapId, tileId);
            }
        }

        base.OnActivate(randomNum);
    }
    public override bool Condition(int turns)
    {
        return eventDuration <= turns;
    }
    public override void OnDeactivate()
    {
        if(!returnPlayer) { base.OnDeactivate(); return; }


        foreach (var player in NetworkData.Instance.players)
        {
            if (player.curMap == mapId)
            {
                player.curMap = mapToReturn;
                player.curTileId = tileToReturn;

            }
        }

        base.OnDeactivate();
    }

}
