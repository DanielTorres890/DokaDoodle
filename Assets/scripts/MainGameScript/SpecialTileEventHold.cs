using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SpecialTileEventHold 
{
    [DoNotSerialize]public List<EnemyCombat> tileEnemy = new List<EnemyCombat>();

    [DoNotSerialize] public List<int> players = new List<int>();

    [DoNotSerialize] public List<int> trapIds = new List<int>();//traps active on this

    [DoNotSerialize] public string battleArea;
    public int xpOnTile;
    public int moneyOnTile;


    public int tileOwner = -1;
    [DoNotSerialize] public int townId;
    [DoNotSerialize] public int townMoneyLevel; //im not sure if this is a great spot for it but it'll have to do since i use this class for persistant tile data
    [DoNotSerialize] public int defenseLevel;
    [DoNotSerialize] public int unitLevel; //basically you'll be able to station a guy at a town then they can train up until whatever the level of the town is
    
}
