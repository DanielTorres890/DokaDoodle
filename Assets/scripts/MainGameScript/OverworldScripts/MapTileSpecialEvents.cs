using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MapTileSpecialEvents : NetworkBehaviour, IDataPersistance
{
    public SpecialTileEventHold[][] mapTiles = new SpecialTileEventHold[10][];
  

    public static MapTileSpecialEvents Instance;

    // Start is called before the first frame update
    public void Awake()
    {
        Instance = this;
        this.mapTiles = new SpecialTileEventHold[10][];
    }
    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += destroyself;
    }

    private void destroyself(ulong id)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= destroyself;
        Destroy(gameObject);
    }
    public SpecialTileEventHold GetCurrentTile()
    {
        return mapTiles[NetworkData.Instance.GetCurrentPlayer().curMap][NetworkData.Instance.GetCurrentPlayer().curTileId];
    }

    public void LoadData(GameData data)
    {
        Debug.Log("Is this getting in the way?");
        mapTiles = data.tileEvents;
    }

    public void SaveData(ref GameData data)
    {
        data.tileEvents = mapTiles;
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnEnemyRpc(int mapId, int tileId, int monsterId)
    {

        var enemyspawn = new EnemyCombat(PlayerCombatManager.Instance.EnemyDataBase.GetItem[monsterId]);
        enemyspawn.persistant = true;
        List<EnemyCombat> enemies = new List<EnemyCombat>
        {
            enemyspawn
        };

        if (MapTileSpecialEvents.Instance.mapTiles[mapId] != null)
        {
            MapTileSpecialEvents.Instance.mapTiles[mapId][tileId].tileEnemy.Add(enemyspawn);
            if (mapId == PlayerMoveManager.Instance.mapNumber) { PlayerMoveManager.Instance.spawnEnemyOverworld(tileId, enemies); }

        }
    }
}
