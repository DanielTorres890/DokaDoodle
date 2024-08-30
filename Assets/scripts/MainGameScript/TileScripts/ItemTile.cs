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
        if (items[rando].type == ItemType.Food) { itemType = 0; }

        if (items[rando].type == ItemType.Weapon || items[rando].type == ItemType.Shield) {  itemType = 1; }

        if (items[rando].type == ItemType.Magic || items[rando].type == ItemType.Equipment) { itemType = 2; }
        ClientChecks.Instance.ConfirmItemPickupRpc(NetworkData.Instance.currentPlayer, rando, itemType);
        
    }

    
}
