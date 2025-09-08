using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTileEventHold 
{
    public List<EnemyCombat> tileEnemy = new List<EnemyCombat>();

    public List<int> players = new List<int>();

    public List<int> trapIds = new List<int>();//traps active on this


    public int xpOnTile;
    public int moneyOnTile;


    public int townMoneyLevel; //im not sure if this is a great spot for it but it'll have to do since i use this class for persistant tile data
    
}
