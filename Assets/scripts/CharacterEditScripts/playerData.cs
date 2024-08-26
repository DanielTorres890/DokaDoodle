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

    public int equipWeaponId;
    public int equipMagicId;
    public int equipEquipmentId;

    public Dictionary<Attributes, int> stats = new Dictionary<Attributes, int>
    {
        {Attributes.MaxHealth, 10 },
        {Attributes.Health, 10 },
        {Attributes.Attack, 10 },
        {Attributes.Defense, 10 },
        {Attributes.Magic, 10 },
        {Attributes.MDefense, 10 },
        {Attributes.Dexterity, 10 }


    };
    public playerData()
    {
        playerClass = 0;
        playerName = "";
        playerFace = 0;
        playerHair = 0;
        curTileId = 0;
        equipWeaponId = -1;
        equipMagicId = -1;
        equipEquipmentId = -1;
    }
    public playerData(int PlayerClass, FixedString32Bytes PlayerName, int PlayerFace, int PlayerHair)
    {
        playerClass = PlayerClass;
        playerName = PlayerName;
        playerFace = PlayerFace;
        playerHair = PlayerHair;
        equipWeaponId = -1;
        equipMagicId = -1;
        equipEquipmentId = -1;
    } 
    
    
    
}
