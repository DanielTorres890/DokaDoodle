using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MapTileSpecialEvents : NetworkBehaviour, IDataPersistance
{
    public SpecialTileEventHold[][] mapTiles = new SpecialTileEventHold[10][];
  

    public static MapTileSpecialEvents Instance;

    // Start is called before the first frame update
    public void Awake()
    {
        Debug.Log("I should be setup");
        Instance = this;
        this.mapTiles = new SpecialTileEventHold[10][];
    }
    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += destroyself;
    }

    private void destroyself(ulong id)
    {
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
}
