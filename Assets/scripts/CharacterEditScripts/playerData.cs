using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class playerData : EntityStats
{

    public int playerClass;

    public int playerFace;
    public int playerHair;
    public int playerNumber;
    public int curTileId;


    public Dictionary<ItemType, int> equipItems = new Dictionary<ItemType, int>
    {
        { ItemType.Equipment , -1 },
        { ItemType.Weapon,  -1},
        { ItemType.Magic , -1 },
        { ItemType.Shield, -1 }


    };
    
    public playerData()
    {
        playerClass = 0;
        name = "";
        playerFace = 0;
        playerHair = 0;
        curTileId = 0;
     
    }
    public playerData(int PlayerClass, FixedString32Bytes PlayerName, int PlayerFace, int PlayerHair)
    {
        playerClass = PlayerClass;
        name = PlayerName.ToString();
        playerFace = PlayerFace;
        playerHair = PlayerHair;
   
    } 
    
    public FixedString32Bytes getName()
    {
        return name;
    }
    
}
