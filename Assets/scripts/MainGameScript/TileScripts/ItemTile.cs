using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemTile : TileScript
{
    public ItemBase[] items;
    public int itemType;
  
    public override void TileEvent()
    {
        Debug.Log("Happened");
       
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer,NetworkManager.Singleton.LocalClientId)) { return; }
        int rando = UnityEngine.Random.Range(0,items.Length);
        ClientChecks.Instance.ConfirmItemPickupRpc(NetworkData.Instance.currentPlayer, rando, itemType);
        
    }

    
}
