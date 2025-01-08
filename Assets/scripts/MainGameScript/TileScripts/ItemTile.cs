using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemTile : TileScript
{
    public ItemBase[] items;
    private int itemType;
  
    public override void TileEvent()
    {
        Debug.Log("Happened");
       
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer,NetworkManager.Singleton.LocalClientId)) { return; }
        int rando = UnityEngine.Random.Range(0,items.Length);
        itemType = items[rando].determineType();
        ClientChecks.Instance.ConfirmItemPickupRpc(NetworkData.Instance.currentPlayer, NetworkData.Instance.playerInventories[0][itemType].database.GetId[items[rando]], itemType);
        
    }

    
}
