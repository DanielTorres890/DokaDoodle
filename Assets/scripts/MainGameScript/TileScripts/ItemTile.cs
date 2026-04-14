using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.InputSystem;

public class ItemTile : TileScript
{
    public ItemTileRewards[] items;
  
    public override void TileEvent()
    {
       
        if (!NetworkManager.Singleton.IsServer) { return; }
        int rando = UnityEngine.Random.Range(0,items.Length);
        

        ClientChecks.Instance.RandomizedItemSelectRpc(NetworkData.Instance.currentPlayer, rando);
        
    }

    
}
