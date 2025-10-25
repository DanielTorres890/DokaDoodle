using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTileEventHold 
{
    public List<EnemyCombat> tileEnemy = new List<EnemyCombat>();

    public List<int> players = new List<int>();

    public List<int> trapIds = new List<int>();//traps active on this

    public string battleArea;
    public int xpOnTile;
    public int moneyOnTile;


    public int tileOwner = -1;
    public int townId;
    public int townMoneyLevel; //im not sure if this is a great spot for it but it'll have to do since i use this class for persistant tile data
    public int defenseLevel;
    public int unitLevel; //basically you'll be able to station a guy at a town then they can train up until whatever the level of the town is
    
}
