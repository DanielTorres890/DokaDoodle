using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class playerData
{

    public int playerClass;
    public FixedString32Bytes playerName;
    public int playerFace;
    public int playerHair;
    public List<InventoryObject> Inventories = new List<InventoryObject>();
    public int curTileId;
   
    public playerData()
    {
        playerClass = 0;
        playerName = "";
        playerFace = 0;
        playerHair = 0;
        curTileId = 0;
    }
    public playerData(int PlayerClass, FixedString32Bytes PlayerName, int PlayerFace, int PlayerHair)
    {
        playerClass = PlayerClass;
        playerName = PlayerName;
        playerFace = PlayerFace;
        playerHair = PlayerHair;
    } 
    

    
}
