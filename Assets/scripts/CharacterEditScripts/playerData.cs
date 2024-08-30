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

    public int curTileId;


    public Dictionary<ItemType, int> equipItems = new Dictionary<ItemType, int>
    {
        { ItemType.Equipment , -1 },
        { ItemType.Weapon,  -1},
        { ItemType.Magic , -1 },
        { ItemType.Shield, -1 }


    };
    public Dictionary<Attributes, int> stats = new Dictionary<Attributes, int>
    {
        {Attributes.MaxHealth, 0 },
        {Attributes.Health, 0 },
        {Attributes.Attack, 0 },
        {Attributes.Defense, 0 },
        {Attributes.Magic, 0 },
        {Attributes.MDefense, 0 },
        {Attributes.Dexterity, 0 }


    };
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
    
    public FixedString32Bytes getName()
    {
        return playerName;
    }
    
}
